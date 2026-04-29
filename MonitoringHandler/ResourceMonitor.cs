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
			// 🎯 THE FIX: Put the Windows core scaler back so it matches Task Manager exactly
			int processorCount = Environment.ProcessorCount;

			// Clean up dead processes from the dictionary so it doesn't leak memory
			List<int> activePids = new List<int>();
			foreach (var s in serverList) { if (s.PID.HasValue) activePids.Add(s.PID.Value); }

			List<int> deadPids = new List<int>();
			foreach (var pid in lastCpuTime.Keys) { if (!activePids.Contains(pid)) deadPids.Add(pid); }
			foreach (var pid in deadPids)
			{
				lastCpuTime.Remove(pid);
				lastCheckTime.Remove(pid);
			}

			foreach (var server in serverList)
			{
				string currentStatus = server.Status ?? "";
				bool isRunning = string.Equals(currentStatus, StatusManager.GetStatus(ServerState.Running), StringComparison.OrdinalIgnoreCase);
				bool isStarting = string.Equals(currentStatus, StatusManager.GetStatus(ServerState.Starting), StringComparison.OrdinalIgnoreCase);

				if (server.PID.HasValue && (isRunning || isStarting))
				{
					try
					{
						using (Process proc = Process.GetProcessById(server.PID.Value))
						{
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
										// 🎯 THE FIX: Divides by the total cores to perfectly mimic Task Manager
										double cpuPercent = (cpuUsedMs / (totalMsPassed * processorCount)) * 100.0;
										total.TotalCpuPercent += cpuPercent;
									}
								}

								lastCpuTime[proc.Id] = currentCpuTime;
								lastCheckTime[proc.Id] = currentTime;
							}
							catch
							{
								// Silent ignore: Windows denied access to the CPU timer for this specific PID.
							}
						}
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