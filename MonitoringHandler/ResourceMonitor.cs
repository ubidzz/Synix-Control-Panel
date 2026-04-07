// Copyright (c) 2026 ubidzz. All Rights Reserved.
//
// This file is part of Synix Control Panel.
//
// This code is provided for transparent viewing and personal use only.
// Unauthorized distribution, public modification, or commercial
// use of this source code or the compiled executable is strictly
// prohibited. Please refer to the LICENSE file in the root
// directory for full terms.

using Synix_Control_Panel.ServerHandler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Synix_Control_Panel.MonitoringHandler
{
	public static class ResourceMonitor
	{
		// We store the last time we saw each process to calculate the "Delta"
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
				// Only monitor if server is Online and has a PID
				if (server.PID.HasValue && server.Status?.ToLower() == "online")
				{
					try
					{
						using (Process proc = Process.GetProcessById(server.PID.Value))
						{
							if (!proc.HasExited)
							{
								// 1. RAM Usage (Simple snapshot)
								total.TotalRamMB += (proc.WorkingSet64 / 1024.0 / 1024.0);

								// 2. CPU Usage (The "Delta" Math)
								// We compare how much CPU time the process used vs how much real time passed
								DateTime currentTime = DateTime.Now;
								TimeSpan currentCpuTime = proc.TotalProcessorTime;

								if (lastCpuTime.ContainsKey(proc.Id))
								{
									double cpuUsedMs = (currentCpuTime - lastCpuTime[proc.Id]).TotalMilliseconds;
									double totalMsPassed = (currentTime - lastCheckTime[proc.Id]).TotalMilliseconds;

									// Calculate % based on all CPU cores
									double cpuPercent = (cpuUsedMs / (totalMsPassed * processorCount)) * 100;

									// Add to the total (Sum of all running servers)
									total.TotalCpuPercent += cpuPercent;
								}

								// Update the "Last Seen" data for the next tick
								lastCpuTime[proc.Id] = currentCpuTime;
								lastCheckTime[proc.Id] = currentTime;
							}
						}
					}
					catch
					{
						// Process closed or access denied; clean up the dictionary
						if (server.PID.HasValue)
						{
							lastCpuTime.Remove(server.PID.Value);
							lastCheckTime.Remove(server.PID.Value);
						}
					}
				}
			}

			return total;
		}
		public static double GetTotalSystemRamGB()
		{
			var gcInfo = GC.GetGCMemoryInfo();
			// Converts Bytes to GB
			return (double)gcInfo.TotalAvailableMemoryBytes / 1024 / 1024 / 1024;
		}

		public static ServerUsage GetTotalResources(System.ComponentModel.BindingList<GameServer> serverList)
		{
			return CalculateUsage(serverList.ToList());
		}
	}
}