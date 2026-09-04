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
		#region Win32 API for Process Discovery
		private const uint TH32CS_SNAPPROCESS = 0x00000002;
		private const int MAX_PATH = 260;
		private static readonly IntPtr InvalidHandleValue = new IntPtr(-1);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct ProcessEntry32
		{
			public uint Size;
			public uint UsageCount;
			public uint ProcessId;
			public IntPtr DefaultHeapId;
			public uint ModuleId;
			public uint ThreadCount;
			public uint ParentProcessId;
			public int BasePriority;
			public uint Flags;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
			public string ExeFile;
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint processId);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool Process32First(IntPtr snapshot, ref ProcessEntry32 entry);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool Process32Next(IntPtr snapshot, ref ProcessEntry32 entry);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool CloseHandle(IntPtr handle);
		#endregion

		private static int GetInitialTargetPid(GameServer server)
		{
			try
			{
				if (server.RunningProcess != null && !server.RunningProcess.HasExited)
				{
					return server.RunningProcess.Id;
				}
			}
			catch
			{

			}

			int savedPid = server.PID.GetValueOrDefault();
			return savedPid > 0 && IsExpectedServerProcess(server, savedPid) ? savedPid : 0;
		}

		private static bool IsExpectedServerProcess(GameServer server, int processId)
		{
			if (processId <= 0 || processId == Environment.ProcessId)
			{
				return false;
			}

			try
			{
				using Process process = Process.GetProcessById(processId);
				if (process.HasExited)
				{
					return false;
				}

				GameInfo? game = GameDatabase.GetGame(server.Game);
				string configuredExe = game?.ExeName ?? string.Empty;
				string expectedName = Path.GetFileNameWithoutExtension(configuredExe);
				bool launchesScript = configuredExe.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) ||
					configuredExe.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase);

				if ((!string.IsNullOrWhiteSpace(expectedName) &&
					 process.ProcessName.Equals(expectedName, StringComparison.OrdinalIgnoreCase)) ||
					(launchesScript && process.ProcessName.Equals("cmd", StringComparison.OrdinalIgnoreCase)))
				{
					return true;
				}

				string? imagePath = TryGetProcessImagePath(process);
				return imagePath != null && IsPathInsideDirectory(imagePath, server.InstallPath);
			}
			catch
			{
				return false;
			}
		}

		private static void TrackProcessTree(int rootPid, Dictionary<int, DateTime?> trackedProcesses)
		{
			foreach (int processId in GetProcessTreeIds(rootPid))
			{
				TrackProcess(processId, trackedProcesses);
			}
		}

		private static HashSet<int> GetProcessTreeIds(int rootPid)
		{
			HashSet<int> processTree = [];
			if (rootPid <= 0 || rootPid == Environment.ProcessId)
			{
				return processTree;
			}

			processTree.Add(rootPid);
			IntPtr snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
			if (snapshot == InvalidHandleValue)
			{
				return processTree;
			}

			try
			{
				List<(int ProcessId, int ParentProcessId)> allProcesses = [];
				ProcessEntry32 entry = new ProcessEntry32
				{
					Size = (uint)Marshal.SizeOf<ProcessEntry32>()
				};

				if (Process32First(snapshot, ref entry))
				{
					do
					{
						allProcesses.Add(((int)entry.ProcessId, (int)entry.ParentProcessId));
						entry.Size = (uint)Marshal.SizeOf<ProcessEntry32>();
					}
					while (Process32Next(snapshot, ref entry));
				}

				Queue<int> pendingParents = new Queue<int>();
				pendingParents.Enqueue(rootPid);
				while (pendingParents.Count > 0)
				{
					int parentId = pendingParents.Dequeue();
					foreach ((int processId, int parentProcessId) in allProcesses)
					{
						if (parentProcessId == parentId && processTree.Add(processId))
						{
							pendingParents.Enqueue(processId);
						}
					}
				}
			}
			finally
			{
				CloseHandle(snapshot);
			}

			return processTree;
		}

		private static void TrackInstallDirectoryProcesses(
			GameServer server,
			Dictionary<int, DateTime?> trackedProcesses)
		{
			if (string.IsNullOrWhiteSpace(server.InstallPath))
			{
				return;
			}

			Process[] processes = Process.GetProcesses();
			try
			{
				foreach (Process process in processes)
				{
					try
					{
						if (process.Id == Environment.ProcessId || process.HasExited)
						{
							continue;
						}

						string? imagePath = TryGetProcessImagePath(process);
						if (imagePath != null && IsPathInsideDirectory(imagePath, server.InstallPath))
						{
							TrackProcess(process, trackedProcesses);
						}
					}
					catch
					{

					}
				}
			}
			finally
			{
				foreach (Process process in processes)
				{
					process.Dispose();
				}
			}
		}

		private static string? TryGetProcessImagePath(Process process)
		{
			try
			{
				return process.MainModule?.FileName;
			}
			catch
			{
				return null;
			}
		}

		private static bool IsPathInsideDirectory(string filePath, string directoryPath)
		{
			if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(directoryPath))
			{
				return false;
			}

			try
			{
				string normalizedDirectory = Path.GetFullPath(directoryPath)
					.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
				string normalizedFile = Path.GetFullPath(filePath);
				return normalizedFile.StartsWith(normalizedDirectory, StringComparison.OrdinalIgnoreCase);
			}
			catch
			{
				return false;
			}
		}

		private static void TrackProcess(int processId, Dictionary<int, DateTime?> trackedProcesses)
		{
			if (processId <= 0 || processId == Environment.ProcessId || trackedProcesses.ContainsKey(processId))
			{
				return;
			}

			try
			{
				using Process process = Process.GetProcessById(processId);
				TrackProcess(process, trackedProcesses);
			}
			catch
			{

			}
		}

		private static void TrackProcess(Process process, Dictionary<int, DateTime?> trackedProcesses)
		{
			if (process.Id <= 0 || process.Id == Environment.ProcessId || trackedProcesses.ContainsKey(process.Id) || process.HasExited)
			{
				return;
			}

			DateTime? startTime = null;
			try
			{
				startTime = process.StartTime.ToUniversalTime();
			}
			catch
			{

			}

			trackedProcesses[process.Id] = startTime;
		}

		private static bool IsTrackedProcessAlive(int processId, Dictionary<int, DateTime?> trackedProcesses)
		{
			if (!trackedProcesses.TryGetValue(processId, out DateTime? expectedStartTime))
			{
				return false;
			}

			try
			{
				using Process process = Process.GetProcessById(processId);
				if (process.HasExited)
				{
					return false;
				}

				if (expectedStartTime.HasValue)
				{
					try
					{
						return process.StartTime.ToUniversalTime() == expectedStartTime.Value;
					}
					catch
					{
						return false;
					}
				}

				return true;
			}
			catch
			{
				return false;
			}
		}

		private static List<int> GetLiveTrackedProcesses(Dictionary<int, DateTime?> trackedProcesses)
		{
			List<int> liveProcesses = [];
			foreach (int processId in trackedProcesses.Keys.ToArray())
			{
				if (IsTrackedProcessAlive(processId, trackedProcesses))
				{
					liveProcesses.Add(processId);
				}
				else
				{
					trackedProcesses.Remove(processId);
				}
			}

			return liveProcesses;
		}

		private static int SelectPrimaryProcess(GameServer server, IReadOnlyCollection<int> liveProcesses, int preferredPid)
		{
			if (preferredPid > 0 && liveProcesses.Contains(preferredPid))
			{
				return preferredPid;
			}

			GameInfo? game = GameDatabase.GetGame(server.Game);
			string configuredExe = game?.ExeName ?? string.Empty;
			string expectedName = Path.GetFileNameWithoutExtension(configuredExe);
			bool launchesScript = configuredExe.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) ||
				configuredExe.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase);

			foreach (int processId in liveProcesses)
			{
				try
				{
					using Process process = Process.GetProcessById(processId);
					if ((!string.IsNullOrWhiteSpace(expectedName) &&
						 process.ProcessName.Equals(expectedName, StringComparison.OrdinalIgnoreCase)) ||
						(launchesScript && process.ProcessName.Equals("cmd", StringComparison.OrdinalIgnoreCase)))
					{
						return processId;
					}
				}
				catch
				{

				}
			}

			return liveProcesses.FirstOrDefault();
		}
	}
}
