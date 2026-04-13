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
			var usage = ResourceMonitor.GetTotalResources(MainGUI.serverList);

			TotalCpuUsage = usage.TotalCpuPercent;
			TotalRamUsageGb = usage.TotalRamMB / 1024.0;

			// 🎯 YOUR LOGIC: Get Total Physical RAM and subtract 7GB for Windows overhead
			// ResourceMonitor should have a way to get the total system memory
			double physicalRamGb = ResourceMonitor.GetTotalSystemRamMB() / 1024.0;
			TotalRamGb = physicalRamGb - 7.0;

			// Fallback: If a machine has very low RAM, don't let TotalRamGb go to 0 or negative
			if (TotalRamGb < 1) TotalRamGb = physicalRamGb;
		}
	}
}