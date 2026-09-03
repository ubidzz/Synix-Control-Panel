// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Design;

namespace Synix_Control_Panel.SynixEngine
{
	internal sealed class FirewallCleanupConfirmationDialog : Form
	{
		internal FirewallCleanupConfirmationDialog(
			IReadOnlyList<string> executablePaths)
		{
			ArgumentNullException.ThrowIfNull(executablePaths);
			if (executablePaths.Count == 0)
				throw new ArgumentException(
					"At least one firewall executable path is required.",
					nameof(executablePaths));

			Text = "Firewall Cleanup Review";
			StartPosition = FormStartPosition.CenterParent;
			ShowInTaskbar = false;
			MinimizeBox = false;
			MaximizeBox = false;
			FormBorderStyle = FormBorderStyle.FixedDialog;
			ClientSize = new Size(780, 664);
			BackColor = SettingsPalette.Window;
			ForeColor = SettingsPalette.PrimaryText;
			Font = new Font("Segoe UI", 9.5F);

			Controls.Add(new ModernSettingsGlyph
			{
				Glyph = "!",
				ForeColor = SettingsPalette.Warning,
				Location = new Point(28, 24),
				Size = new Size(44, 44)
			});
			Controls.Add(new Label
			{
				Name = "firewallCleanupHeading",
				Text = "Review orphaned firewall rules",
				Font = new Font("Segoe UI", 18F, FontStyle.Bold),
				ForeColor = SettingsPalette.PrimaryText,
				Location = new Point(86, 20),
				Size = new Size(666, 42)
			});
			Controls.Add(new Label
			{
				Name = "firewallCleanupSubtitle",
				Text = BuildSummary(executablePaths.Count),
				ForeColor = SettingsPalette.SecondaryText,
				Location = new Point(88, 62),
				Size = new Size(664, 42)
			});

			ModernSettingsCard reasonCard = new()
			{
				Name = "firewallInspectionReasonCard",
				Location = new Point(28, 112),
				Size = new Size(724, 86),
				FillColor = SettingsPalette.InfoSurface,
				BorderColor = SettingsPalette.Divider,
				CornerRadius = 11
			};
			reasonCard.Controls.Add(new Label
			{
				Text = "WHY SYNIX FLAGGED THESE RULES",
				Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
				ForeColor = SettingsPalette.Accent,
				Location = new Point(18, 12),
				Size = new Size(688, 22)
			});
			reasonCard.Controls.Add(new Label
			{
				Name = "firewallInspectionReasonText",
				Text = "Each rule points to an executable under C:\\Synix\\Games\\[Game]\\[Server], but that individual server folder is gone and no installed Synix server owns the path.",
				ForeColor = SettingsPalette.PrimaryText,
				Location = new Point(18, 38),
				Size = new Size(688, 38)
			});
			Controls.Add(reasonCard);

			ModernSettingsCard pathsCard = new()
			{
				Name = "firewallPathsCard",
				Location = new Point(28, 212),
				Size = new Size(724, 260),
				FillColor = SettingsPalette.Card,
				BorderColor = SettingsPalette.Divider,
				CornerRadius = 13
			};
			pathsCard.Controls.Add(new Label
			{
				Text = $"EXECUTABLE RULES READY FOR REMOVAL  •  {executablePaths.Count}",
				Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
				ForeColor = SettingsPalette.Warning,
				Location = new Point(20, 16),
				Size = new Size(684, 24)
			});
			pathsCard.Controls.Add(new TextBox
			{
				Name = "firewallRuleList",
				AccessibleName = "Firewall executable rules ready for removal",
				Text = BuildPathList(executablePaths),
				Multiline = true,
				ReadOnly = true,
				WordWrap = false,
				ScrollBars = ScrollBars.Both,
				BackColor = SettingsPalette.Input,
				ForeColor = SettingsPalette.PrimaryText,
				BorderStyle = BorderStyle.FixedSingle,
				Font = new Font("Consolas", 9.5F),
				Location = new Point(20, 48),
				Size = new Size(684, 188)
			});
			Controls.Add(pathsCard);

			ModernSettingsCard safetyCard = new()
			{
				Name = "firewallSafetyCard",
				Location = new Point(28, 486),
				Size = new Size(724, 108),
				FillColor = SettingsPalette.InfoSurface,
				BorderColor = SettingsPalette.Warning,
				CornerRadius = 11
			};
			safetyCard.Controls.Add(new Label
			{
				Text = "WHAT HAPPENS AFTER YOU CONTINUE",
				Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
				ForeColor = SettingsPalette.Warning,
				Location = new Point(18, 10),
				Size = new Size(688, 22)
			});
			safetyCard.Controls.Add(new Label
			{
				Name = "firewallCleanupActionText",
				Text = "Windows requests administrator permission. Synix then removes only firewall rules matching the exact executable paths above and scans again to verify the cleanup.",
				ForeColor = SettingsPalette.PrimaryText,
				Location = new Point(18, 34),
				Size = new Size(688, 34)
			});
			safetyCard.Controls.Add(new Label
			{
				Name = "firewallCleanupSafetyText",
				Text = "Not changed: game files, saved servers, port-only rules, custom install folders, and firewall rules outside C:\\Synix\\Games.",
				ForeColor = SettingsPalette.SecondaryText,
				Location = new Point(18, 73),
				Size = new Size(688, 26)
			});
			Controls.Add(safetyCard);

			ModernSettingsButton cancelButton = new()
			{
				Name = "cancelFirewallCleanupButton",
				Text = "Cancel",
				Location = new Point(418, 608),
				Size = new Size(158, 42),
				DialogResult = DialogResult.Cancel
			};
			ModernSettingsButton removeButton = new()
			{
				Name = "confirmFirewallCleanupButton",
				Text = "Remove Rules",
				AccessibleName = "Confirm removal of the listed firewall rules",
				Location = new Point(594, 608),
				Size = new Size(158, 42),
				DialogResult = DialogResult.OK,
				UseAccentStyle = true
			};
			Controls.Add(cancelButton);
			Controls.Add(removeButton);
			CancelButton = cancelButton;

			ThemeManager.Apply(this);
		}

		private static string BuildSummary(int count) =>
			$"Synix found {count} firewall {(count == 1 ? "rule" : "rules")} that reference deleted servers under C:\\Synix\\Games. Nothing changes until you approve removal.";

		private static string BuildPathList(
			IEnumerable<string> executablePaths) =>
			string.Join(
				Environment.NewLine,
				executablePaths.Select(path => $"• {path}"));
	}
}
