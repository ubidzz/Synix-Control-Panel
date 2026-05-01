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
using Synix_Control_Panel.MonitoringHandler;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		// 🎯 THE CACHE: Store the hardware RAM total here so we don't poll WMI every second
		private static double? _cachedPhysicalRamGb = null;

		private void UpdateResourceStats()
		{
			// 1. Get the summary for the GUI totals (This ALREADY calculates server.RamUsage inside ResourceMonitor!)
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
		}
	}
}