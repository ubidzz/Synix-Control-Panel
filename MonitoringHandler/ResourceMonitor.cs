/*
 * Copyright (c) 2026 ubidzz. All Rights Reserved.
 *
 * This file is part of Synix Control Panel.
 *
 * This code is provided for transparent viewing and personal use only.
 * Unauthorized distribution, public modification, or commercial 
 * use of this source code or the compiled executable is strictly 
 * prohibited. Please refer to the LICENSE file in the root 
 * directory for full terms.
 */
using Synix_Control_Panel.SynixEngine;
using System.Diagnostics;
using System.Management;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel.MonitoringHandler
{
	public static class ResourceMonitor
	{
		private static PerformanceCounter _globalCpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
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

			foreach (var server in serverList)
			{
				string currentStatus = server.Status ?? "";
				bool isRunning = string.Equals(currentStatus, StatusManager.GetStatus(ServerState.Running), StringComparison.OrdinalIgnoreCase);
				bool isStarting = string.Equals(currentStatus, StatusManager.GetStatus(ServerState.Starting), StringComparison.OrdinalIgnoreCase);

				if (server.PID.HasValue && (isRunning || isStarting))
				{
					try
					{
						// THE FIX: 'using' guarantees the unmanaged handle is killed, even on a crash
						using (Process proc = Process.GetProcessById(server.PID.Value))
						{
							// If the process is gone, just set usage to 0 and move on.
							if (proc.HasExited)
							{
								server.RamUsage = 0;
								continue;
							}

							// --- 1. RAM Calculation ---
							double serverMB = proc.WorkingSet64 / 1024.0 / 1024.0;
							total.TotalRamMB += serverMB;

							if (Core.TotalRamGb > 0)
							{
								server.RamUsage = (serverMB / 1024.0 / Core.TotalRamGb) * 100.0;
							}

							// --- 2. CPU Calculation ---
							DateTime currentTime = DateTime.Now;
							TimeSpan currentCpuTime = proc.TotalProcessorTime;

							if (lastCpuTime.ContainsKey(proc.Id))
							{
								double cpuUsedMs = (currentCpuTime - lastCpuTime[proc.Id]).TotalMilliseconds;
								double totalMsPassed = (currentTime - lastCheckTime[proc.Id]).TotalMilliseconds;

								if (totalMsPassed > 0)
								{
									double cpuPercent = (cpuUsedMs / (totalMsPassed * processorCount)) * 100;
									total.TotalCpuPercent += cpuPercent;
								}
							}

							lastCpuTime[proc.Id] = currentCpuTime;
							lastCheckTime[proc.Id] = currentTime;
						} // <- The process handle is cleanly destroyed here automatically
					}
					catch
					{
						server.RamUsage = 0;
					}
				}
				else
				{
					server.RamUsage = 0;
				}
			}
			return total;
		}

		public static double GetTotalSystemRamGB()
		{
			try
			{
				double totalBytes = 0;

				// 🎯 THE LEAK FIX: Added using blocks for the collection and disposed the object
				using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
				using (ManagementObjectCollection collection = searcher.Get())
				{
					foreach (ManagementObject obj in collection)
					{
						totalBytes = Convert.ToDouble(obj["TotalPhysicalMemory"]);
						obj.Dispose(); // Nuke the handle
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
			// 🎯 THE FIX: Efficiently pull total physical memory
			var gcInfo = GC.GetGCMemoryInfo();

			// TotalAvailableMemoryBytes represents the total physical memory 
			// accessible to the OS/Process.
			return GetTotalSystemRamGB() * 1024.0;
		}

		public static double GetProcessRamMB(int pid)
		{
			try
			{
				if (pid <= 0) return 0;

				// Using 'using' ensures we don't leak handles on your 6-core rig
				using (Process proc = Process.GetProcessById(pid))
				{
					if (proc.HasExited) return 0;

					// Matches the logic in your CalculateUsage method
					return proc.WorkingSet64 / 1024.0 / 1024.0;
				}
			}
			catch (Exception)
			{
				// Silent return for the engine heartbeat
				return 0;
			}
		}

		public static double GetGlobalCpuUsage()
		{
			try
			{
				// 🎯 THE WMI KILLER: This reads the CPU directly from the kernel with 0 memory allocation
				return Math.Round((double)_globalCpuCounter.NextValue(), 1);
			}
			catch (Exception)
			{
				return 0.0;
			}
		}
	}
}