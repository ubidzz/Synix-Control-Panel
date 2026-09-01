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
using Synix_Control_Panel.SynixEngine;
using System.Diagnostics;
using System.Management;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel.SynixApp.MonitoringHandler
{
	public static class ResourceMonitor
	{
		private static PerformanceCounter? _globalCpuCounter = null;
		private static Dictionary<int, TimeSpan> lastCpuTime = new Dictionary<int, TimeSpan>();
		private static Dictionary<int, DateTime> lastCheckTime = new Dictionary<int, DateTime>();

		public struct ServerUsage
		{
			public double TotalCpuPercent;
			public double TotalRamMB;
		}

		public static ServerUsage CalculateUsage(IEnumerable<GameServer> serverList)
		{
			ServerUsage total = new ServerUsage();
			int processorCount = Environment.ProcessorCount;

			List<GameServer> servers = serverList.ToList();
			Dictionary<GameServer, int[]> processIdsByServer = servers.ToDictionary(
				server => server,
				server => GetProcessIds(server).ToArray());
			HashSet<int> activePids = processIdsByServer.Values
				.SelectMany(processIds => processIds)
				.ToHashSet();

			List<int> deadPids = new List<int>();
			foreach (var pid in lastCpuTime.Keys) { if (!activePids.Contains(pid)) deadPids.Add(pid); }
			foreach (var pid in deadPids)
			{
				lastCpuTime.Remove(pid);
				lastCheckTime.Remove(pid);
			}

			HashSet<int> sampledPids = [];
			foreach (var server in servers)
			{
				string currentStatus = server.Status ?? "";
				bool isRunning = string.Equals(currentStatus, StatusManager.GetStatus(ServerState.Running), StringComparison.OrdinalIgnoreCase);
				bool isStarting = string.Equals(currentStatus, StatusManager.GetStatus(ServerState.Starting), StringComparison.OrdinalIgnoreCase);

				if ((isRunning || isStarting) &&
					processIdsByServer.TryGetValue(server, out int[]? serverProcessIds) &&
					serverProcessIds.Length > 0)
				{
					double serverTotalMb = 0;
					foreach (int processId in serverProcessIds)
					{
						if (!sampledPids.Add(processId))
							continue;
						try
						{
							using Process proc = Process.GetProcessById(processId);
							if (proc.HasExited)
								continue;

							double serverMB = proc.WorkingSet64 / 1024.0 / 1024.0;
							serverTotalMb += serverMB;
							total.TotalRamMB += serverMB;

							try
							{
								DateTime currentTime = DateTime.Now;
								TimeSpan currentCpuTime = proc.TotalProcessorTime;

								if (lastCpuTime.ContainsKey(proc.Id))
								{
									double cpuUsedMs = (currentCpuTime - lastCpuTime[proc.Id]).TotalMilliseconds;
									double totalMsPassed = (currentTime - lastCheckTime[proc.Id]).TotalMilliseconds;

									if (totalMsPassed > 0)
									{
										double cpuPercent = (cpuUsedMs / (totalMsPassed * processorCount)) * 100.0;
										total.TotalCpuPercent += cpuPercent;
									}
								}

								lastCpuTime[proc.Id] = currentCpuTime;
								lastCheckTime[proc.Id] = currentTime;
							}
							catch
							{

							}
						}
						catch
						{
							// A process may close between registry refresh and sampling.
						}
					}
					server.RamUsage = Core.TotalRamGb > 0
						? (serverTotalMb / 1024.0 / Core.TotalRamGb) * 100.0
						: 0;
				}
				else
				{
					server.RamUsage = 0;
				}
			}
			return total;
		}

		internal static IReadOnlyList<int> GetProcessIds(GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);
			HashSet<int> processIds = (server.ServerProcesses ?? [])
				.Select(process => process.ProcessId)
				.Where(processId => processId > 0)
				.ToHashSet();
			if (server.PID is > 0)
				processIds.Add(server.PID.Value);
			return processIds.ToArray();
		}

		public static double GetTotalSystemRamGB()
		{
			try
			{
				double totalBytes = 0;

				using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
				using (ManagementObjectCollection collection = searcher.Get())
				{
					foreach (ManagementObject obj in collection)
					{
						totalBytes = Convert.ToDouble(obj["TotalPhysicalMemory"]);
						obj.Dispose();
					}
				}

				return totalBytes / 1024.0 / 1024.0 / 1024.0;
			}
			catch (Exception)
			{
				return 16.0;
			}
		}

		public static ServerUsage GetTotalResources(System.ComponentModel.BindingList<GameServer> serverList)
		{
			return CalculateUsage(serverList);
		}

		public static double GetTotalSystemRamMB()
		{
			var gcInfo = GC.GetGCMemoryInfo();
			return GetTotalSystemRamGB() * 1024.0;
		}

		public static double GetProcessRamMB(int pid)
		{
			try
			{
				if (pid <= 0) return 0;

				using (Process proc = Process.GetProcessById(pid))
				{
					if (proc.HasExited) return 0;
					return proc.WorkingSet64 / 1024.0 / 1024.0;
				}
			}
			catch (Exception)
			{
				return 0;
			}
		}

		public static double GetGlobalCpuUsage()
		{
			try
			{
				if (_globalCpuCounter == null)
				{
					_globalCpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
				}

				return Math.Round((double)_globalCpuCounter.NextValue(), 1);
			}
			catch (Exception)
			{
				return 0.0;
			}
		}
	}
}
