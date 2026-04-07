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
using System.ComponentModel; // Required for BindingList
using System.Diagnostics;
using System.Linq;

namespace Synix_Control_Panel.MonitoringHandler
{
	public static class ResourceMonitor
	{
		public struct ServerUsage
		{
			public double TotalCpuPercent;
			public double TotalRamMB;
			public int ActiveServerCount;
		}

		// 1. This handles standard Lists
		public static ServerUsage GetTotalResources(List<GameServer> serverList)
		{
			return CalculateUsage(serverList);
		}

		// 2. This handles the BindingList used by your DataGridView
		// FIX: Change 'object' to 'ServerUsage'
		public static ServerUsage GetTotalResources(BindingList<GameServer> serverList)
		{
			// We convert the BindingList to a List so we can use the same logic
			return CalculateUsage(serverList.ToList());
		}

		// 3. Shared logic to keep things clean
		public static ServerUsage CalculateUsage(IEnumerable<GameServer> serverList)
		{
			ServerUsage total = new ServerUsage();

			foreach (var server in serverList)
			{
				// Check if server is actually running
				if (server.PID.HasValue && server.Status?.ToLower() == "online")
				{
					try
					{
						using (Process proc = Process.GetProcessById(server.PID.Value))
						{
							if (!proc.HasExited)
							{
								// 1. RAM Usage
								total.TotalRamMB += (proc.WorkingSet64 / 1024.0 / 1024.0);

								// 2. CPU Usage (The proper way for .NET)
								// This gets the total time the CPU has worked since the process started.
								// For a dashboard, we'll use a simpler 'Total System' check or 
								// just use the ActiveServerCount to verify it's working.
								total.ActiveServerCount++;

								// For now, let's just put a random 'Pulse' to see if the graph moves
								// We will add the high-precision delta math once we see the line!
								total.TotalCpuPercent += 5.0;
							}
						}
					}
					catch
					{
						server.Status = "Offline"; // Auto-detect if it crashed
					}
				}
			}
			return total;
		}
	}
}