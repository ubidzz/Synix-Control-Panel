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
using System.Text;

namespace Synix_Control_Panel.SynixApp.Database.GameConfigurations
{
	internal enum ConfigurationValidationState
	{
		Passed,
		Warning,
		Failed
	}

	internal readonly record struct ConfigurationValidationItem(
		ConfigurationValidationState State,
		string Setting,
		string Message);

	internal sealed class ConfigurationValidationReport
	{
		public ConfigurationValidationReport(
			string gameName,
			int savedRevision,
			int currentRevision,
			bool fixConfigAvailable,
			IReadOnlyList<ConfigurationValidationItem> items)
		{
			GameName = gameName;
			SavedRevision = savedRevision;
			CurrentRevision = currentRevision;
			FixConfigAvailable = fixConfigAvailable;
			Items = items;
		}

		public string GameName { get; }
		public int SavedRevision { get; }
		public int CurrentRevision { get; }
		public bool FixConfigAvailable { get; }
		public IReadOnlyList<ConfigurationValidationItem> Items { get; }
		public int PassedCount => Items.Count(item =>
			item.State == ConfigurationValidationState.Passed);
		public int WarningCount => Items.Count(item =>
			item.State == ConfigurationValidationState.Warning);
		public int FailedCount => Items.Count(item =>
			item.State == ConfigurationValidationState.Failed);
		public bool IsCurrent => FailedCount == 0 && WarningCount == 0;

		public string ToPlainText()
		{
			StringBuilder report = new();
			report.AppendLine("SYNIX CONFIGURATION APPLICATION REPORT");
			report.AppendLine();
			report.AppendLine($"Game: {GameName}");
			report.AppendLine($"Result: {(IsCurrent ? "CURRENT" : "ATTENTION NEEDED")}");
			report.AppendLine(
				$"Passed: {PassedCount}  Warnings: {WarningCount}  Failed: {FailedCount}");
			report.AppendLine(
				$"Template revision: saved {SavedRevision}, current {CurrentRevision}");
			report.AppendLine(
				$"Fix Config: {(FixConfigAvailable ? "Available while the server is stopped" : "Not available for this game")}");
			report.AppendLine();

			foreach (ConfigurationValidationItem item in Items)
			{
				string state = item.State switch
				{
					ConfigurationValidationState.Passed => "PASS",
					ConfigurationValidationState.Warning => "WARNING",
					_ => "FAIL"
				};
				report.AppendLine($"[{state}] {item.Setting}");
				report.AppendLine(item.Message);
			}

			if (FixConfigAvailable)
			{
				report.AppendLine();
				report.AppendLine(
					"Fix Config rebuilds the complete file from the trusted Synix template, reapplies the saved server values, and preserves a backup. Other custom values can be removed by a full reset.");
			}

			return report.ToString().TrimEnd();
		}
	}
}
