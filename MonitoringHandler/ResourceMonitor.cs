// Copyright (c) 2026 ubidzz. All Rights Reserved.
//
// This file is part of Synix Control Panel.
//
// This code is provided for transparent viewing and personal use only.
// Unauthorized distribution, public modification, or commercial
// use of this source code or the compiled executable is strictly
// prohibited. Please refer to the LICENSE file in the root
// directory for full terms.

using Synix_Control_Panel.Database;
using Synix_Control_Panel.ServerHandler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using static Synix_Control_Panel.SynixEngine.Core;

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
				// 1. Only check servers that are supposed to be "Running or Starting"
				string currentStatus = server.Status ?? "";
				bool isRunning = string.Equals(currentStatus, StatusManager.GetStatus(ServerState.Running), StringComparison.OrdinalIgnoreCase);
				bool isStarting = string.Equals(currentStatus, StatusManager.GetStatus(ServerState.Starting), StringComparison.OrdinalIgnoreCase);
				if (server.PID.HasValue && (isRunning || isStarting))
				{
					try
					{
						// We attempt to find the process using the ID we saved when we clicked Start
						Process proc = Process.GetProcessById(server.PID.Value);

						if (proc.HasExited)
						{
							// THE AUTO-FIX: The process is gone! Set it to Stopped.
							server.Status = StatusManager.GetStatus(ServerState.Stopped);
							server.PID = null;
							continue;
						}

						// 2. RAM Calculation
						total.TotalRamMB += (proc.WorkingSet64 / 1024.0 / 1024.0);

						// 3. CPU Calculation (Delta Math)
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

						// Save current stats for the next tick
						lastCpuTime[proc.Id] = currentCpuTime;
						lastCheckTime[proc.Id] = currentTime;
					}
					catch
					{
						// If we hit an error (like "Access Denied"), the server is likely closing/gone
						server.Status = StatusManager.GetStatus(ServerState.Stopped);
						server.PID = null;
					}
				}
			}
			return total;
		}

		public static double GetTotalSystemRamGB()
		{
			try
			{
				double totalBytes = 0;

				// This talks directly to the PC hardware to find the REAL RAM total
				using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
				{
					foreach (ManagementObject obj in searcher.Get())
					{
						totalBytes = Convert.ToDouble(obj["TotalPhysicalMemory"]);
					}
				}

				// Bytes -> Kilobytes -> Megabytes -> Gigabytes
				return totalBytes / 1024.0 / 1024.0 / 1024.0;
			}
			catch (Exception)
			{
				// If the hardware check fails, we'll just guess 16GB so the app doesn't crash
				return 16.0;
			}
		}

		public static ServerUsage GetTotalResources(System.ComponentModel.BindingList<GameServer> serverList)
		{
			return CalculateUsage(serverList.ToList());
		}

		public static double GetTotalSystemRamMB()
		{
			// 🎯 THE FIX: Efficiently pull total physical memory
			var gcInfo = GC.GetGCMemoryInfo();

			// TotalAvailableMemoryBytes represents the total physical memory 
			// accessible to the OS/Process.
			return (double)gcInfo.TotalAvailableMemoryBytes / (1024 * 1024);
		}
	}
}