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
		// 🎯 THE CACHE: Store the hardware RAM total here so we don't poll WMI every second
		private static double? _cachedPhysicalRamGb = null;

		private void UpdateResourceStats()
		{
			// 1. Get the summary for the GUI totals
			var usage = ResourceMonitor.GetTotalResources(MainGUI.serverList);

			TotalCpuUsage = usage.TotalCpuPercent;
			TotalRamUsageGb = usage.TotalRamMB / 1024.0;

			// 2. 🎯 CALCULATE THE "OVERHEAD" RAM (Using the Cache!)
			if (_cachedPhysicalRamGb == null)
			{
				// This heavy WMI hardware call now runs EXACTLY ONCE when the engine starts
				_cachedPhysicalRamGb = ResourceMonitor.GetTotalSystemRamMB() / 1024.0;
			}

			// Subtracting 5GB for Windows overhead
			TotalRamGb = _cachedPhysicalRamGb.Value - 5.0;

			if (TotalRamGb < 1) TotalRamGb = _cachedPhysicalRamGb.Value;

			// 3. PER-SERVER TRACKING
			foreach (var server in MainGUI.serverList)
			{
				if (server.Status == StatusManager.GetStatus(ServerState.Running) && server.RunningProcess != null)
				{
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