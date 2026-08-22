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
	public sealed class DevelopmentSettingsPage : UserControl
	{
		private ModernSettingsToggle? _usePremadeConfigurationsToggle;
		private ModernSettingsToggle? _collectGeneratedConfigurationsToggle;

		public DevelopmentSettingsPage()
		{
			BackColor = SettingsPalette.Window;
			Size = new Size(818, 520);
			AutoScroll = true;
			AutoScrollMinSize = new Size(0, 548);
			AddPremadeConfigurationsCard();
			AddGeneratedConfigurationsCard();
			AddReleaseReadinessCard();
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool UsePremadeConfigurations
		{
			get => _usePremadeConfigurationsToggle?.Checked ?? true;
			set
			{
				if (_usePremadeConfigurationsToggle != null)
					_usePremadeConfigurationsToggle.Checked = value;
			}
		}

		[Browsable(false)]
		public event EventHandler? UsePremadeConfigurationsChanged;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CollectGeneratedConfigurations
		{
			get => _collectGeneratedConfigurationsToggle?.Checked ?? false;
			set
			{
				if (_collectGeneratedConfigurationsToggle != null)
					_collectGeneratedConfigurationsToggle.Checked = value;
			}
		}

		[Browsable(false)]
		public event EventHandler? CollectGeneratedConfigurationsChanged;

		[Browsable(false)]
		public event EventHandler? CollectGeneratedConfigurationsRequested;

		[Browsable(false)]
		public event EventHandler? ReleaseReadinessRequested;

		private void AddPremadeConfigurationsCard()
		{
			ModernSettingsCard card = CreateCard(0, 156);
			card.Controls.Add(CreateGlyph("⚙"));
			card.Controls.Add(CreateTitle("Use Premade Game Configurations"));
			card.Controls.Add(CreateDescription(
				"Turn this off to stop Synix from creating or updating premade game configuration files. Existing files are never deleted.",
				76));

			_usePremadeConfigurationsToggle = new ModernSettingsToggle
			{
				AccessibleName = "Use premade game configurations",
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				BackColor = SettingsPalette.Card,
				Checked = true,
				Location = new Point(card.Width - 79, 28),
				Size = new Size(54, 30)
			};
			_usePremadeConfigurationsToggle.CheckedChanged += (_, eventArgs) =>
				UsePremadeConfigurationsChanged?.Invoke(this, eventArgs);
			card.Controls.Add(_usePremadeConfigurationsToggle);
			Controls.Add(card);
		}

		private void AddGeneratedConfigurationsCard()
		{
			ModernSettingsCard card = CreateCard(176, 176);
			card.Controls.Add(CreateGlyph("⇩"));
			card.Controls.Add(CreateTitle("Collect Generated Game Configurations"));
			card.Controls.Add(CreateDescription(
				"After a server is stopped, copy game-created config files into one folder per game under Documents\\Synix Generated Configurations. Live files are unchanged and secret fields become template placeholders.",
				100));

			_collectGeneratedConfigurationsToggle = new ModernSettingsToggle
			{
				AccessibleName = "Automatically collect generated game configurations",
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				BackColor = SettingsPalette.Card,
				Location = new Point(card.Width - 79, 28),
				Size = new Size(54, 30)
			};
			_collectGeneratedConfigurationsToggle.CheckedChanged += (_, eventArgs) =>
				CollectGeneratedConfigurationsChanged?.Invoke(this, eventArgs);

			ModernSettingsButton collectButton = new()
			{
				AccessibleName = "Collect generated game configurations now",
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				Location = new Point(card.Width - 185, 102),
				Size = new Size(160, 42),
				Text = "Collect Now",
				UseAccentStyle = true
			};
			collectButton.Click += (_, eventArgs) =>
				CollectGeneratedConfigurationsRequested?.Invoke(this, eventArgs);

			card.Controls.Add(_collectGeneratedConfigurationsToggle);
			card.Controls.Add(collectButton);
			Controls.Add(card);
		}

		private void AddReleaseReadinessCard()
		{
			ModernSettingsCard card = CreateCard(372, 156);
			card.Controls.Add(CreateGlyph("✓"));
			card.Controls.Add(CreateTitle("Release Readiness Checker"));
			card.Controls.Add(CreateDescription(
				"Check matching versions, Stable publish files, MSI upgrade safety, SHA-256 hashes, and the complete automated test suite.",
				76));

			ModernSettingsButton checkButton = new()
			{
				AccessibleName = "Check release readiness",
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				Location = new Point(card.Width - 185, 54),
				Size = new Size(160, 42),
				Text = "Check Release",
				UseAccentStyle = true
			};
			checkButton.Click += (_, eventArgs) =>
				ReleaseReadinessRequested?.Invoke(this, eventArgs);

			card.Controls.Add(checkButton);
			Controls.Add(card);
		}

		private ModernSettingsCard CreateCard(int y, int height)
		{
			return new ModernSettingsCard
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				BackColor = SettingsPalette.Card,
				BorderColor = SettingsPalette.Border,
				FillColor = SettingsPalette.Card,
				CornerRadius = 13,
				Location = new Point(0, y),
				Size = new Size(GetCardWidth(), height)
			};
		}

		private static ModernSettingsGlyph CreateGlyph(string glyph)
		{
			return new ModernSettingsGlyph
			{
				BackColor = SettingsPalette.Card,
				ForeColor = SettingsPalette.Accent,
				Font = new Font("Segoe UI Symbol", 15F),
				Glyph = glyph,
				Location = new Point(22, 24),
				Size = new Size(42, 42)
			};
		}

		private static Label CreateTitle(string text)
		{
			return new Label
			{
				AutoEllipsis = true,
				BackColor = SettingsPalette.Card,
				ForeColor = SettingsPalette.PrimaryText,
				Font = new Font("Segoe UI", 12F, FontStyle.Bold),
				Location = new Point(80, 22),
				Size = new Size(570, 31),
				Text = text
			};
		}

		private static Label CreateDescription(string text, int height)
		{
			return new Label
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				BackColor = SettingsPalette.Card,
				ForeColor = SettingsPalette.SecondaryText,
				Font = new Font("Segoe UI", 9.5F),
				Location = new Point(80, 55),
				Size = new Size(520, height),
				Text = text
			};
		}

		private int GetCardWidth()
		{
			return Math.Max(
				640,
				ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 2);
		}
	}
}
