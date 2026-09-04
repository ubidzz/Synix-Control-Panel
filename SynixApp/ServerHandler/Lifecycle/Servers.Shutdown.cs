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
		public static async Task<bool> Stop(GameServer server, Action<string, Color> logCallback, bool isManual = true)
		{
			ArgumentNullException.ThrowIfNull(logCallback);
			Dictionary<int, DateTime?> trackedProcesses = [];
			int targetPid = 0;

			try
			{
				server.Status = StatusManager.GetStatus(ServerState.Stopping);
				Core.Instance.UpdateGridStatus();
				TrackSavedServerProcesses(server, trackedProcesses);

				targetPid = GetInitialTargetPid(server);
				if (targetPid > 0)
				{
					TrackProcessTree(targetPid, trackedProcesses);
				}

				TrackInstallDirectoryProcesses(server, trackedProcesses);
				SynchronizeServerProcessRegistry(server, trackedProcesses);
				List<int> liveProcesses = GetLiveTrackedProcesses(trackedProcesses);

				if (targetPid <= 0 || !liveProcesses.Contains(targetPid))
				{
					targetPid = SelectPrimaryProcess(server, liveProcesses, 0);
					if (targetPid > 0)
					{
						TrackProcessTree(targetPid, trackedProcesses);
						liveProcesses = GetLiveTrackedProcesses(trackedProcesses);
					}
				}

				if (liveProcesses.Count == 0)
				{
					logCallback?.Invoke(LocalizationManager.Get("ServerStop.Activity.NoProcess", server.ServerName), Color.Lime);
					FinalizeStoppedState(server);
					return true;
				}

				logCallback?.Invoke(
					LocalizationManager.Get(
						"ServerStop.Activity.Tracking",
						liveProcesses.Count,
						server.ServerName,
						FormatProcessRegistry(server.ServerProcesses)),
					Color.Aqua);

				logCallback?.Invoke(LocalizationManager.Get("ServerStop.Activity.SaveSignal", server.ServerName), Color.Aqua);

				bool isMinecraft = GameCapabilityResolver.UsesMinecraftLifecycle(server);
				bool signalSent = isMinecraft
					? await TrySendMinecraftStopCommand(server, targetPid, logCallback!)
					: targetPid > 0 && await TrySendConsoleShutdownSignal(targetPid, server);
				TimeSpan gracefulTimeout = isMinecraft
					? TimeSpan.FromSeconds(60)
					: TimeSpan.FromSeconds(25);

				if (signalSent)
				{
					liveProcesses = await WaitForServerProcessesToExit(server, targetPid, trackedProcesses, gracefulTimeout);
				}
				else
				{
					RefreshTrackedProcesses(server, targetPid, trackedProcesses);
					liveProcesses = GetLiveTrackedProcesses(trackedProcesses);
				}

				if (liveProcesses.Count == 0)
				{
					logCallback?.Invoke(LocalizationManager.Get("ServerStop.Activity.CleanStop", server.ServerName), Color.Lime);
					FinalizeStoppedState(server);
					return true;
				}

				logCallback?.Invoke(
					LocalizationManager.Get("ServerStop.Activity.Forcing", server.ServerName, liveProcesses.Count),
					Color.Violet);

				await ForceTerminateProcesses(liveProcesses, targetPid, trackedProcesses, logCallback!);
				liveProcesses = await WaitForServerProcessesToExit(server, targetPid, trackedProcesses, TimeSpan.FromSeconds(10));

				if (liveProcesses.Count > 0)
				{
					RestoreLiveServerState(server, liveProcesses, targetPid);
					logCallback?.Invoke(
						LocalizationManager.Get(
							"ServerStop.Activity.LiveProcessRemains",
							server.ServerName,
							string.Join(", ", liveProcesses)),
						Color.Red);
					return false;
				}

				FinalizeStoppedState(server);
				logCallback?.Invoke(LocalizationManager.Get("ServerStop.Activity.ForcedAndVerified", server.ServerName), Color.Violet);
				return true;
			}
			catch (Exception ex)
			{
				logCallback?.Invoke(LocalizationManager.Get("ServerStop.Activity.Failed", server.ServerName, ex.Message), Color.Red);

				RefreshTrackedProcesses(server, targetPid, trackedProcesses);
				List<int> liveProcesses = GetLiveTrackedProcesses(trackedProcesses);
				if (liveProcesses.Count == 0)
				{
					FinalizeStoppedState(server);
					return true;
				}

				RestoreLiveServerState(server, liveProcesses, targetPid);
				return false;
			}
		}

		private static async Task<List<int>> WaitForServerProcessesToExit(
			GameServer server,
			int targetPid,
			Dictionary<int, DateTime?> trackedProcesses,
			TimeSpan timeout)
		{
			return await WaitForStableProcessExit(
				() =>
				{
					RefreshTrackedProcesses(server, targetPid, trackedProcesses);
					SynchronizeServerProcessRegistry(server, trackedProcesses);
					return GetLiveTrackedProcesses(trackedProcesses);
				},
				timeout,
				TimeSpan.FromSeconds(3),
				TimeSpan.FromMilliseconds(500));
		}

		internal static async Task<List<int>> WaitForStableProcessExit(
			Func<List<int>> getLiveProcesses,
			TimeSpan timeout,
			TimeSpan quietPeriod,
			TimeSpan pollInterval)
		{
			const int minimumConsecutiveEmptySamples = 3;
			ArgumentNullException.ThrowIfNull(getLiveProcesses);
			if (timeout < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
			if (quietPeriod < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(quietPeriod));
			if (pollInterval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(pollInterval));

			DateTime deadline = DateTime.UtcNow.Add(timeout);
			DateTime? quietSince = null;
			int consecutiveEmptySamples = 0;

			while (true)
			{
				List<int> liveProcesses = getLiveProcesses();
				DateTime now = DateTime.UtcNow;

				if (liveProcesses.Count > 0)
				{
					quietSince = null;
					consecutiveEmptySamples = 0;
					if (now >= deadline)
					{
						return liveProcesses;
					}
				}
				else
				{
					quietSince ??= now;
					consecutiveEmptySamples++;
					if (consecutiveEmptySamples >= minimumConsecutiveEmptySamples &&
						now - quietSince.Value >= quietPeriod)
					{
						return liveProcesses;
					}
				}

				await Task.Delay(pollInterval);
			}
		}

		private static async Task ForceTerminateProcesses(
			List<int> liveProcesses,
			int targetPid,
			Dictionary<int, DateTime?> trackedProcesses,
			Action<string, Color> logCallback)
		{
			IEnumerable<int> orderedProcesses = liveProcesses
				.OrderBy(processId => processId == targetPid ? 0 : 1)
				.ThenBy(processId => processId);

			foreach (int processId in orderedProcesses)
			{
				if (!IsTrackedProcessAlive(processId, trackedProcesses))
				{
					continue;
				}

				try
				{
					using Process process = Process.GetProcessById(processId);
					process.Kill(entireProcessTree: true);
				}
				catch (Exception ex)
				{
					logCallback?.Invoke(LocalizationManager.Get("ServerStop.Activity.DirectKillFailed", processId, ex.Message), Color.OrangeRed);
				}
			}

			await Task.Delay(300);

			foreach (int processId in GetLiveTrackedProcesses(trackedProcesses))
			{
				ProcessStartInfo killInfo = new ProcessStartInfo
				{
					FileName = "taskkill.exe",
					CreateNoWindow = true,
					UseShellExecute = false
				};
				killInfo.ArgumentList.Add("/F");
				killInfo.ArgumentList.Add("/T");
				killInfo.ArgumentList.Add("/PID");
				killInfo.ArgumentList.Add(processId.ToString());

				try
				{
					using Process? killProcess = Process.Start(killInfo);
					if (killProcess != null)
					{
						await killProcess.WaitForExitAsync();
						if (killProcess.ExitCode != 0 && IsTrackedProcessAlive(processId, trackedProcesses))
						{
							logCallback?.Invoke(LocalizationManager.Get("ServerStop.Activity.TaskkillExitCode", killProcess.ExitCode, processId), Color.OrangeRed);
						}
					}
				}
				catch (Exception ex)
				{
					logCallback?.Invoke(LocalizationManager.Get("ServerStop.Activity.TaskkillFailed", processId, ex.Message), Color.OrangeRed);
				}
			}
		}

		private static void RestoreLiveServerState(GameServer server, IReadOnlyCollection<int> liveProcesses, int preferredPid)
		{
			int survivingPid = SelectPrimaryProcess(server, liveProcesses, preferredPid);
			if (survivingPid <= 0)
			{
				return;
			}

			Process? survivingProcess = null;
			try
			{
				survivingProcess = Process.GetProcessById(survivingPid);
				if (survivingProcess.HasExited)
				{
					survivingProcess.Dispose();
					survivingProcess = null;
					return;
				}

				bool alreadyBound = false;
				try
				{
					alreadyBound = server.RunningProcess != null && server.RunningProcess.Id == survivingPid;
				}
				catch (Exception suppressedException)
				{
					Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException);
				}

				if (!alreadyBound)
				{
					server.RunningProcess?.Dispose();
					server.RunningProcess = survivingProcess;
					survivingProcess = null;
				}
			}
			catch (Exception suppressedException)
			{
				Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException);
			}
			finally
			{
				survivingProcess?.Dispose();
			}

			server.PID = survivingPid;
			server.Status = StatusManager.GetStatus(ServerState.Running);
			Core.Instance.UpdateGridStatus();
		}

		private static void FinalizeStoppedState(GameServer server)
		{
			MinecraftConsoleHub.NotifyStopped(server);
			server.Status = StatusManager.GetStatus(ServerState.Stopped);
			server.PID = null;
			lock (_serverProcessRegistryLock)
			{
				server.ServerProcesses = [];
			}
			server.LastProcessDiscoveryUtc = DateTime.MinValue;
			server.HasAnnouncedOnline = false;
			server.IsProbing = false;
			server.LastProbeTime = null;
			server.RunningProcess?.Dispose();
			server.RunningProcess = null;
			Core.Instance.UpdateGridStatus();
		}
	}
}
