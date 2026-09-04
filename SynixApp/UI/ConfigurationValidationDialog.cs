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
using Synix_Control_Panel.SynixApp.Database.GameConfigurations;
using Synix_Control_Panel.SynixApp.Design;

namespace Synix_Control_Panel.SynixApp.UI
{
	internal sealed partial class ConfigurationValidationDialog : Form
	{
		public ConfigurationValidationDialog()
		{
			InitializeComponent();
			ThemeManager.Apply(this);
		}

		internal ConfigurationValidationDialog(
			ConfigurationValidationReport report)
			: this()
		{
			ArgumentNullException.ThrowIfNull(report);
			_titleLabel.Text = $"{report.GameName} Configuration Check";
			_summaryLabel.Text = report.IsCurrent
				? $"CURRENT  •  {report.PassedCount} checks passed"
				: $"ATTENTION NEEDED  •  {report.FailedCount} failed  •  {report.WarningCount} warning(s)";
			_summaryLabel.ForeColor = report.IsCurrent
				? SettingsPalette.Success
				: report.FailedCount > 0
					? SettingsPalette.Warning
					: SettingsPalette.SecondaryText;
			_reportBox.Text = report.ToPlainText();
			_reportBox.SelectionStart = 0;
			_reportBox.ScrollToCaret();
		}

		private void CopyButton_Click(object? sender, EventArgs eventArgs)
		{
			try
			{
				Clipboard.SetText(_reportBox.Text);
				_summaryLabel.Text = "Configuration report copied to the clipboard.";
				_summaryLabel.ForeColor = SettingsPalette.Success;
			}
			catch
			{
				LocalizedMessageBox.Show(
					this,
					"Windows could not copy the configuration report.",
					"Copy Failed",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
			}
		}
	}
}
