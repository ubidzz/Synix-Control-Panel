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
using Synix_Control_Panel.SynixApp.MonitoringHandler;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{

		private static double? _cachedPhysicalRamGb = null;

		private void UpdateResourceStats()
		{

			var usage = ResourceMonitor.GetTotalResources(ServerRegistry.Servers);

			TotalCpuUsage = usage.TotalCpuPercent;
			TotalRamUsageGb = usage.TotalRamMB / 1024.0;

			if (_cachedPhysicalRamGb == null)
			{

				_cachedPhysicalRamGb = ResourceMonitor.GetTotalSystemRamMB() / 1024.0;
			}

			TotalRamGb = _cachedPhysicalRamGb.Value - 5.0;

			if (TotalRamGb < 1) TotalRamGb = _cachedPhysicalRamGb.Value;
		}
	}
}
