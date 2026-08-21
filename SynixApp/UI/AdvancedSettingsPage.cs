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
using System.ComponentModel;
using Synix_Control_Panel.SynixApp.Design;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class AdvancedSettingsPage : UserControl
	{
		public AdvancedSettingsPage()
		{
			InitializeComponent();

			if (LicenseManager.UsageMode != LicenseUsageMode.Designtime &&
				!SynixBuildInfo.IsOfficialRelease)
			{
				AddReleaseReadinessCard();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ElevatedSystemTasks
		{
			get => chkElevatedTasks.Checked;
			set => chkElevatedTasks.Checked = value;
		}

		[Browsable(false)]
		public event EventHandler? ElevatedSystemTasksChanged
		{
			add => chkElevatedTasks.CheckedChanged += value;
			remove => chkElevatedTasks.CheckedChanged -= value;
		}

		[Browsable(false)]
		public event EventHandler? ReleaseReadinessRequested;

		private void AddReleaseReadinessCard()
		{
			ModernSettingsCard releaseCard = new()
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				BackColor = SettingsPalette.Card,
				BorderColor = SettingsPalette.Border,
				FillColor = SettingsPalette.Card,
				CornerRadius = 13,
				Location = new Point(0, 146),
				Size = new Size(818, 156)
			};
			ModernSettingsGlyph glyph = new()
			{
				BackColor = SettingsPalette.Card,
				ForeColor = SettingsPalette.Accent,
				Font = new Font("Segoe UI Symbol", 15F),
				Glyph = "✓",
				Location = new Point(22, 24),
				Size = new Size(42, 42)
			};
			Label title = new()
			{
				AutoEllipsis = true,
				BackColor = SettingsPalette.Card,
				ForeColor = SettingsPalette.PrimaryText,
				Font = new Font("Segoe UI", 12F, FontStyle.Bold),
				Location = new Point(80, 22),
				Size = new Size(520, 31),
				Text = "Release Readiness Checker"
			};
			Label description = new()
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				BackColor = SettingsPalette.Card,
				ForeColor = SettingsPalette.SecondaryText,
				Font = new Font("Segoe UI", 9.5F),
				Location = new Point(80, 55),
				Size = new Size(525, 76),
				Text = "Developer-only check for matching versions, Stable publish files, Inno Setup safety, SHA-256 hashes, and the complete automated test suite."
			};
			ModernSettingsButton checkButton = new()
			{
				AccessibleName = "Check release readiness",
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				Location = new Point(632, 54),
				Size = new Size(160, 42),
				Text = "Check Release",
				UseAccentStyle = true
			};
			checkButton.Click += (_, eventArgs) =>
				ReleaseReadinessRequested?.Invoke(this, eventArgs);

			releaseCard.Controls.Add(glyph);
			releaseCard.Controls.Add(title);
			releaseCard.Controls.Add(description);
			releaseCard.Controls.Add(checkButton);
			Controls.Add(releaseCard);
		}
	}
}
