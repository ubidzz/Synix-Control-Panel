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
			// We just update the values; we don't define them again.
			var usage = ResourceMonitor.GetTotalResources(MainGUI.serverList);

			TotalCpuUsage = usage.TotalCpuPercent;
			TotalRamUsageGb = usage.TotalRamMB / 1024.0;
		}
	}
}