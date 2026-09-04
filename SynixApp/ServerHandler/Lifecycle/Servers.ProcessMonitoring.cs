// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
//
// LEGAL NOTICE:
// This source code is proprietary and confidential.
// 1. Permission is granted for PERSONAL, NON-COMMERCIAL use only.
// 2. You may modify this code for your own use, but you may NOT redistribute,
//    rebrand, or sell this code or derivative works without written consent.
// 3. The "Synix" brand and logic remain the property of Jason Turner.
// ============================================================================
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using Synix_Control_Panel.SynixEngine;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel.SynixApp.ServerHandler
{
	public static partial class Servers
	{
		private static readonly object _serverProcessRegistryLock = new();
		private static readonly TimeSpan _processDiscoveryInterval = TimeSpan.FromSeconds(5);

		private static bool IsStoppingStatus(string? status)
		{
			return status?.StartsWith(
				StatusManager.GetStatus(ServerState.Stopping),
				StringComparison.OrdinalIgnoreCase) == true;
		}

		private static void RefreshTrackedProcesses(
			GameServer server,
			int targetPid,
			Dictionary<int, DateTime?> trackedProcesses)
		{
			if (targetPid > 0 && IsTrackedProcessAlive(targetPid, trackedProcesses))
			{
				TrackProcessTree(targetPid, trackedProcesses);
			}

			TrackInstallDirectoryProcesses(server, trackedProcesses);
		}

		internal static IReadOnlyList<ServerProcessIdentity> RefreshServerProcessRegistry(
			GameServer server,
			bool forceDiscovery = false)
		{
			ArgumentNullException.ThrowIfNull(server);
			Dictionary<int, DateTime?> trackedProcesses = [];
			TrackSavedServerProcesses(server, trackedProcesses);

			int targetPid = GetInitialTargetPid(server);
			if (targetPid > 0)
			{
				TrackProcessTree(targetPid, trackedProcesses);
			}

			DateTime now = DateTime.UtcNow;
			if (forceDiscovery ||
				trackedProcesses.Count == 0 ||
				now - server.LastProcessDiscoveryUtc >= _processDiscoveryInterval)
			{
				TrackInstallDirectoryProcesses(server, trackedProcesses);
				server.LastProcessDiscoveryUtc = now;
			}

			SynchronizeServerProcessRegistry(server, trackedProcesses);
			lock (_serverProcessRegistryLock)
			{
				return server.ServerProcesses.ToArray();
			}
		}

		internal static bool ReconcileActiveServerProcesses(
			GameServer server,
			bool forceDiscovery = false)
		{
			IReadOnlyList<ServerProcessIdentity> processes =
				RefreshServerProcessRegistry(server, forceDiscovery);
			if (processes.Count == 0)
			{
				return false;
			}

			int primaryPid = SelectPrimaryProcess(
				server,
				processes.Select(process => process.ProcessId).ToArray(),
				server.PID.GetValueOrDefault());
			if (primaryPid <= 0)
			{
				return false;
			}

			try
			{
				bool alreadyBound = server.RunningProcess != null &&
					!server.RunningProcess.HasExited &&
					server.RunningProcess.Id == primaryPid;
				if (!alreadyBound)
				{
					Process replacement = Process.GetProcessById(primaryPid);
					server.RunningProcess?.Dispose();
					server.RunningProcess = replacement;
				}

				server.PID = primaryPid;
				return true;
			}
			catch (Exception exception)
			{
				ApplicationLogService.WriteSuppressedException(exception);
				return false;
			}
		}

		private static async Task CaptureSpawnedServerProcesses(
			GameServer server,
			Action<string, Color> logCallback)
		{
			int previousCount = 0;
			for (int attempt = 0; attempt < 10; attempt++)
			{
				await Task.Delay(1000).ConfigureAwait(false);
				if (IsStoppingStatus(server.Status) ||
					server.Status == StatusManager.GetStatus(ServerState.Stopped))
				{
					return;
				}

				IReadOnlyList<ServerProcessIdentity> processes =
					RefreshServerProcessRegistry(server, forceDiscovery: true);
				if (processes.Count > previousCount)
				{
					previousCount = processes.Count;
					logCallback?.Invoke(
						LocalizationManager.Get(
							"ServerProcess.Activity.Registered",
							processes.Count,
							FormatProcessRegistry(processes)),
						Color.Cyan);
				}
			}

			FileHandler.SaveServers();
		}

		private static void TrackSavedServerProcesses(
			GameServer server,
			Dictionary<int, DateTime?> trackedProcesses)
		{
			ServerProcessIdentity[] savedProcesses;
			lock (_serverProcessRegistryLock)
			{
				savedProcesses = (server.ServerProcesses ?? []).ToArray();
			}

			foreach (ServerProcessIdentity identity in savedProcesses)
			{
				if (IsSavedServerProcessAlive(server, identity))
				{
					trackedProcesses[identity.ProcessId] = identity.StartTimeUtc;
				}
			}
		}

		private static bool IsSavedServerProcessAlive(
			GameServer server,
			ServerProcessIdentity identity)
		{
			if (identity.ProcessId <= 0 ||
				identity.ProcessId == Environment.ProcessId ||
				string.IsNullOrWhiteSpace(identity.ExecutablePath) ||
				!IsPathInsideDirectory(identity.ExecutablePath, server.InstallPath))
			{
				return false;
			}

			try
			{
				using Process process = Process.GetProcessById(identity.ProcessId);
				if (process.HasExited)
				{
					return false;
				}

				if (identity.StartTimeUtc.HasValue &&
					process.StartTime.ToUniversalTime() != identity.StartTimeUtc.Value)
				{
					return false;
				}

				string? actualPath = TryGetProcessImagePath(process);
				if (!string.IsNullOrWhiteSpace(actualPath))
				{
					return string.Equals(
						Path.GetFullPath(actualPath),
						Path.GetFullPath(identity.ExecutablePath),
						StringComparison.OrdinalIgnoreCase);
				}

				return process.ProcessName.Equals(
					Path.GetFileNameWithoutExtension(identity.ExecutablePath),
					StringComparison.OrdinalIgnoreCase);
			}
			catch (Exception exception)
			{
				ApplicationLogService.WriteSuppressedException(exception);
				return false;
			}
		}

		private static void SynchronizeServerProcessRegistry(
			GameServer server,
			Dictionary<int, DateTime?> trackedProcesses)
		{
			HashSet<int> verifiedLaunchProcessTree = GetVerifiedLaunchProcessTreeIds(server);
			Dictionary<int, ServerProcessIdentity> existing;
			lock (_serverProcessRegistryLock)
			{
				existing = (server.ServerProcesses ?? [])
					.Where(process => process.ProcessId > 0)
					.GroupBy(process => process.ProcessId)
					.ToDictionary(group => group.Key, group => group.First());
			}

			List<ServerProcessIdentity> liveIdentities = [];
			foreach (int processId in GetLiveTrackedProcesses(trackedProcesses))
			{
				try
				{
					using Process process = Process.GetProcessById(processId);
					string? executablePath = TryGetProcessImagePath(process);
					if (string.IsNullOrWhiteSpace(executablePath) &&
						existing.TryGetValue(processId, out ServerProcessIdentity? savedIdentity))
					{
						executablePath = savedIdentity.ExecutablePath;
					}

					bool isInstalledServerExecutable = !string.IsNullOrWhiteSpace(executablePath) &&
						IsPathInsideDirectory(executablePath, server.InstallPath);
					bool isVerifiedLaunchProcess = verifiedLaunchProcessTree.Contains(processId);
					if (string.IsNullOrWhiteSpace(executablePath) ||
						(!isInstalledServerExecutable && !isVerifiedLaunchProcess))
					{
						continue;
					}

					DateTime? startTimeUtc = null;
					try
					{
						startTimeUtc = process.StartTime.ToUniversalTime();
					}
					catch (Exception exception)
					{
						ApplicationLogService.WriteSuppressedException(exception);
						if (existing.TryGetValue(processId, out ServerProcessIdentity? recoveredIdentity))
						{
							startTimeUtc = recoveredIdentity.StartTimeUtc;
						}
					}

					liveIdentities.Add(new ServerProcessIdentity
					{
						ProcessId = processId,
						ExecutablePath = Path.GetFullPath(executablePath),
						StartTimeUtc = startTimeUtc
					});
				}
				catch (Exception suppressedException)
				{
					Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException);
				}
			}

			lock (_serverProcessRegistryLock)
			{
				server.ServerProcesses = liveIdentities
					.OrderBy(process => process.ProcessId)
					.ToList();
			}
		}

		private static HashSet<int> GetVerifiedLaunchProcessTreeIds(GameServer server)
		{
			try
			{
				GameInfo? game = GameDatabase.GetGame(server.Game);
				if (game == null)
				{
					return [];
				}

				string launchPath = GameLaunchCommandBuilder.ResolveExecutablePath(server, game);
				string extension = Path.GetExtension(launchPath);
				if (!extension.Equals(".bat", StringComparison.OrdinalIgnoreCase) &&
					!extension.Equals(".cmd", StringComparison.OrdinalIgnoreCase))
				{
					return [];
				}

				Process? launchProcess = server.RunningProcess;
				if (launchProcess == null ||
					launchProcess.HasExited ||
					server.PID.GetValueOrDefault() != launchProcess.Id)
				{
					return [];
				}

				return GetProcessTreeIds(launchProcess.Id);
			}
			catch (Exception exception)
			{
				ApplicationLogService.WriteSuppressedException(exception);
				return [];
			}
		}

		private static string FormatProcessRegistry(
			IEnumerable<ServerProcessIdentity> processes)
		{
			string result = string.Join(
				", ",
				processes.Select(process =>
					$"{Path.GetFileName(process.ExecutablePath)} (PID {process.ProcessId})"));
			return string.IsNullOrWhiteSpace(result)
				? LocalizationManager.Get("ServerProcess.None")
				: result;
		}
	}
}
