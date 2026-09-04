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
using Synix_Control_Panel.SynixApp.Localization;

namespace Synix_Control_Panel.SynixApp.UI.Settings
{
	public partial class AdvancedSettingsPage : UserControl
	{
		private ModernSettingsToggle _backgroundServiceToggle = null!;
		private Label _backgroundServiceStatus = null!;
		private ModernSettingsButton _firewallCleanupButton = null!;
		private Label _firewallCleanupStatus = null!;
		private bool _firewallCleanupInProgress;

		public AdvancedSettingsPage()
		{
			InitializeComponent();
			ArrangeBuiltInCards();
			AddFirewallCleanupCard();
			AddBackgroundServiceCard();
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
		public event EventHandler? TroubleshooterRequested
		{
			add => btnTroubleshooter.Click += value;
			remove => btnTroubleshooter.Click -= value;
		}

		[Browsable(false)]
		public event EventHandler? FirewallCleanupRequested
		{
			add => _firewallCleanupButton.Click += value;
			remove => _firewallCleanupButton.Click -= value;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool BackgroundServiceEnabled
		{
			get => _backgroundServiceToggle.Checked;
			set
			{
				_backgroundServiceToggle.Checked = value;
				SetBackgroundServiceStatus(
					value
						? LocalizationManager.Get("Advanced.Background.EnabledCurrent")
						: LocalizationManager.Get("Advanced.Background.DisabledCurrent"),
					value);
			}
		}

		[Browsable(false)]
		public event EventHandler? BackgroundServiceEnabledChanged
		{
			add => _backgroundServiceToggle.CheckedChanged += value;
			remove => _backgroundServiceToggle.CheckedChanged -= value;
		}

		internal void SetBackgroundServiceStatus(string message, bool success)
		{
			_backgroundServiceStatus.Text =
				LocalizationManager.TranslateKnownText(message);
			_backgroundServiceStatus.ForeColor = success
				? SettingsPalette.Success
				: SettingsPalette.SecondaryText;
		}

		internal void SetFirewallCleanupState(
			string message,
			bool success,
			bool inProgress = false)
		{
			_firewallCleanupInProgress = inProgress;
			_firewallCleanupStatus.Text =
				LocalizationManager.TranslateKnownText(message);
			_firewallCleanupStatus.ForeColor = success
				? SettingsPalette.Success
				: inProgress
					? SettingsPalette.Accent
					: SettingsPalette.SecondaryText;
			UpdateFirewallCleanupAvailability();
		}

		private void ArrangeBuiltInCards()
		{
			settingsCard.Height = 104;
			troubleshooterCard.Location = new Point(0, 390);
			troubleshooterCard.Height = 126;
		}

		private void AddFirewallCleanupCard()
		{
			ModernSettingsCard card = new()
			{
				Name = "firewallCleanupCard",
				Location = new Point(0, 120),
				Size = new Size(818, 126),
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				FillColor = SettingsPalette.Card,
				BorderColor = SettingsPalette.Divider,
				CornerRadius = 13
			};
			card.Controls.Add(new ModernSettingsGlyph
			{
				Glyph = "◇",
				Location = new Point(22, 22),
				Size = new Size(42, 42)
			});
			card.Controls.Add(new Label
			{
				Name = "lblFirewallCleanupTitle",
				Text = LocalizationManager.Get("Text.2E3FED7012F80848E367"),
				Font = new Font("Segoe UI", 12F, FontStyle.Bold),
				ForeColor = SettingsPalette.PrimaryText,
				Location = new Point(80, 16),
				Size = new Size(520, 30),
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			});
			card.Controls.Add(new Label
			{
				Name = "lblFirewallCleanupDescription",
				Text = LocalizationManager.Get("Text.0078A1C33BF28B41D89B"),
				ForeColor = SettingsPalette.SecondaryText,
				Location = new Point(80, 44),
				Size = new Size(520, 50),
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			});
			_firewallCleanupButton = new ModernSettingsButton
			{
				Name = "btnFirewallCleanup",
				Text = LocalizationManager.Get("Text.4A7C3351A85F01259421"),
				AccessibleName = LocalizationManager.Get("Text.7FF6D10F94CC3274F62A"),
				Location = new Point(613, 42),
				Size = new Size(180, 42),
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				UseAccentStyle = true
			};
			_firewallCleanupStatus = new Label
			{
				Name = "lblFirewallCleanupStatus",
				Text = LocalizationManager.Get("Text.54C8BBF4B91A6012A87B"),
				ForeColor = SettingsPalette.SecondaryText,
				Location = new Point(80, 96),
				Size = new Size(520, 22),
				AutoEllipsis = true,
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			};
			card.Controls.Add(_firewallCleanupButton);
			card.Controls.Add(_firewallCleanupStatus);
			Controls.Add(card);
			card.BringToFront();
			UpdateFirewallCleanupAvailability();
		}

		private void UpdateFirewallCleanupAvailability()
		{
			_firewallCleanupButton.Enabled = !_firewallCleanupInProgress;
			LocalizationManager.BindText(
				_firewallCleanupButton,
				_firewallCleanupInProgress
					? "Advanced.Firewall.ButtonChecking"
					: "Text.4A7C3351A85F01259421");
		}

		private void AddBackgroundServiceCard()
		{
			ModernSettingsCard card = new()
			{
				Name = "backgroundServiceCard",
				Location = new Point(0, 262),
				Size = new Size(818, 112),
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				FillColor = SettingsPalette.Card,
				BorderColor = SettingsPalette.Divider,
				CornerRadius = 13
			};
			card.Controls.Add(new ModernSettingsGlyph
			{
				Glyph = "◉",
				Location = new Point(22, 24),
				Size = new Size(42, 42)
			});
			card.Controls.Add(new Label
			{
				Text = LocalizationManager.Get("Text.E26C86E2069FAA56AA9A"),
				Font = new Font("Segoe UI", 12F, FontStyle.Bold),
				ForeColor = SettingsPalette.PrimaryText,
				Location = new Point(80, 14),
				Size = new Size(540, 30)
			});
			card.Controls.Add(new Label
			{
				Text = LocalizationManager.Get("Text.091C22C7EE122D76DAE4"),
				ForeColor = SettingsPalette.SecondaryText,
				Location = new Point(80, 42),
				Size = new Size(610, 36)
			});
			_backgroundServiceStatus = new Label
			{
				Text = LocalizationManager.Get("Text.9F67BB0F58790841820E"),
				ForeColor = SettingsPalette.SecondaryText,
				Location = new Point(80, 82),
				Size = new Size(610, 22),
				AutoEllipsis = true
			};
			card.Controls.Add(_backgroundServiceStatus);
			_backgroundServiceToggle = new ModernSettingsToggle
			{
				Location = new Point(739, 26),
				AccessibleName = LocalizationManager.Get("Text.E26C86E2069FAA56AA9A"),
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			};
			card.Controls.Add(_backgroundServiceToggle);
			Controls.Add(card);
			card.BringToFront();
		}

	}
}
