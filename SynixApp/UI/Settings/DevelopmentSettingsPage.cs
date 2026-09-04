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

namespace Synix_Control_Panel.SynixApp.UI.Settings
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
			AutoScrollMinSize = new Size(0, 936);
			AddPremadeConfigurationsCard();
			AddGeneratedConfigurationsCard();
			AddReleaseReadinessCard();
			AddGameDefinitionsCard();
			AddReliabilityTestCard();
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

		[Browsable(false)]
		public event EventHandler? GameDefinitionValidationRequested;

		[Browsable(false)]
		public event EventHandler? GameDefinitionBuilderRequested;

		[Browsable(false)]
		public event EventHandler? GameVerificationQueueRequested;

		[Browsable(false)]
		public event EventHandler? ReliabilityTestRequested;

		private void AddPremadeConfigurationsCard()
		{
			ModernSettingsCard card = CreateCard(0, 156);
			card.Controls.Add(CreateGlyph("⚙"));
			card.Controls.Add(CreateTitle(
				LocalizationManager.Get("Text.86286BA8D6C58145405E")));
			card.Controls.Add(CreateDescription(
				LocalizationManager.Get("Settings.Development.Premade.Description"),
				76));

			_usePremadeConfigurationsToggle = new ModernSettingsToggle
			{
				AccessibleName = LocalizationManager.Get("Text.86286BA8D6C58145405E"),
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
			card.Controls.Add(CreateTitle(
				LocalizationManager.Get("Settings.Development.Collect.Title")));
			card.Controls.Add(CreateDescription(
				LocalizationManager.Get("Settings.Development.Collect.Description"),
				100));

			_collectGeneratedConfigurationsToggle = new ModernSettingsToggle
			{
				AccessibleName = LocalizationManager.Get("Text.FC52030FEF2C9AEC802D"),
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				BackColor = SettingsPalette.Card,
				Location = new Point(card.Width - 79, 28),
				Size = new Size(54, 30)
			};
			_collectGeneratedConfigurationsToggle.CheckedChanged += (_, eventArgs) =>
				CollectGeneratedConfigurationsChanged?.Invoke(this, eventArgs);

			ModernSettingsButton collectButton = new()
			{
				AccessibleName = LocalizationManager.Get("Text.FC61CB13704E7289E632"),
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				Location = new Point(card.Width - 185, 102),
				Size = new Size(160, 42),
				Text = LocalizationManager.Get("Text.1BB88EF61843E3570111"),
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
			card.Controls.Add(CreateTitle(
				LocalizationManager.Get("Text.E8986299CC046EAA3D40")));
			card.Controls.Add(CreateDescription(
				LocalizationManager.Get("Settings.Development.Release.Description"),
				76));

			ModernSettingsButton checkButton = new()
			{
				AccessibleName = LocalizationManager.Get("Text.5C75164CBDDF6C71885B"),
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				Location = new Point(card.Width - 185, 54),
				Size = new Size(160, 42),
				Text = LocalizationManager.Get("Text.975CB60570D8F3E9A641"),
				UseAccentStyle = true
			};
			checkButton.Click += (_, eventArgs) =>
				ReleaseReadinessRequested?.Invoke(this, eventArgs);

			card.Controls.Add(checkButton);
			Controls.Add(card);
		}

		private void AddGameDefinitionsCard()
		{
			ModernSettingsCard card = CreateCard(548, 196);
			card.Controls.Add(CreateGlyph("◇"));
			card.Controls.Add(CreateTitle(
				LocalizationManager.Get("Settings.Development.Definitions.Title")));
			card.Controls.Add(CreateDescription(
				LocalizationManager.Get("Settings.Development.Definitions.Description"),
				72));

			ModernSettingsButton validateButton = new()
			{
				AccessibleName = LocalizationManager.Get("Text.B976ECB48B1B2D4AC318"),
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				Location = new Point(card.Width - 505, 136),
				Size = new Size(150, 42),
				Text = LocalizationManager.Get("Text.8CBBAC76C84A2410D3BD")
			};
			validateButton.Click += (_, eventArgs) =>
				GameDefinitionValidationRequested?.Invoke(this, eventArgs);

			ModernSettingsButton queueButton = new()
			{
				AccessibleName = LocalizationManager.Get("Text.E673C4B555E4960E2796"),
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				Location = new Point(card.Width - 345, 136),
				Size = new Size(150, 42),
				Text = LocalizationManager.Get("DynamicText.F96281A2FB88C022D64B")
			};
			queueButton.Click += (_, eventArgs) =>
				GameVerificationQueueRequested?.Invoke(this, eventArgs);

			ModernSettingsButton builderButton = new()
			{
				AccessibleName = LocalizationManager.Get("Text.6DA034A48B203D145071"),
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				Location = new Point(card.Width - 185, 136),
				Size = new Size(160, 42),
				Text = LocalizationManager.Get("Text.C7F120FC21CAE9933BF3"),
				UseAccentStyle = true
			};
			builderButton.Click += (_, eventArgs) =>
				GameDefinitionBuilderRequested?.Invoke(this, eventArgs);

			card.Controls.Add(validateButton);
			card.Controls.Add(queueButton);
			card.Controls.Add(builderButton);
			Controls.Add(card);
		}

		private void AddReliabilityTestCard()
		{
			ModernSettingsCard card = CreateCard(764, 156);
			card.Controls.Add(CreateGlyph("⌁"));
			card.Controls.Add(CreateTitle(
				LocalizationManager.Get("Text.522E549C1259D9FA47D1")));
			card.Controls.Add(CreateDescription(
				LocalizationManager.Get("Settings.Development.Reliability.Description"),
				76));

			ModernSettingsButton runButton = new()
			{
				AccessibleName = LocalizationManager.Get("Text.897C522834169BCBAA15"),
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				Location = new Point(card.Width - 185, 54),
				Size = new Size(160, 42),
				Text = LocalizationManager.Get("Text.1C76BC80677D0B5E4DDA"),
				UseAccentStyle = true
			};
			runButton.Click += (_, eventArgs) => ReliabilityTestRequested?.Invoke(this, eventArgs);
			card.Controls.Add(runButton);
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
