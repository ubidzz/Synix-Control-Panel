// Copyright (c) 2026 ubidzz. All Rights Reserved.
//
// This file is part of Synix Control Panel.
//
// This code is provided for transparent viewing and personal use only.
// Unauthorized distribution, public modification, or commercial
// use of this source code or the compiled executable is strictly
// prohibited. Please refer to the LICENSE file in the root
// directory for full terms.
using Synix_Control_Panel.MonitoringHandler;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		private void UpdateResourceStats()
		{
			// 1. Get the summary for the GUI totals
			var usage = ResourceMonitor.GetTotalResources(MainGUI.serverList);

			TotalCpuUsage = usage.TotalCpuPercent;
			TotalRamUsageGb = usage.TotalRamMB / 1024.0;

			// 2. 🎯 CALCULATE THE "OVERHEAD" RAM
			double physicalRamGb = ResourceMonitor.GetTotalSystemRamMB() / 1024.0;
			TotalRamGb = physicalRamGb - 7.0; // Subtracting 7GB for Windows

			if (TotalRamGb < 1) TotalRamGb = physicalRamGb;

			// 3. 🎯 PER-SERVER TRACKING: Populate the RamUsage for your alerts
			foreach (var server in MainGUI.serverList)
			{
				if (server.Status == "Running" && server.RunningProcess != null)
				{
					// Get the individual process usage from your Monitor
					// We calculate the % based on the "Available" RAM (TotalRamGb)
					double serverMB = ResourceMonitor.GetProcessRamMB(server.PID ?? 0);
					server.RamUsage = (serverMB / 1024.0 / TotalRamGb) * 100.0;
				}
				else
				{
					server.RamUsage = 0;
				}
			}
		}
	}
}