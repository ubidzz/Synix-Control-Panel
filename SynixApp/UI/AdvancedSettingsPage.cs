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
		private ModernSettingsToggle _backgroundServiceToggle = null!;
		private Label _backgroundServiceStatus = null!;

		public AdvancedSettingsPage()
		{
			InitializeComponent();
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
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool BackgroundServiceEnabled
		{
			get => _backgroundServiceToggle.Checked;
			set
			{
				_backgroundServiceToggle.Checked = value;
				SetBackgroundServiceStatus(
					value
						? "Enabled — monitoring continues after the dashboard closes."
						: "Disabled — scheduled work runs only while Synix is open.",
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
			_backgroundServiceStatus.Text = message;
			_backgroundServiceStatus.ForeColor = success
				? SettingsPalette.Success
				: SettingsPalette.SecondaryText;
		}

		private void AddBackgroundServiceCard()
		{
			troubleshooterCard.Top = 292;
			ModernSettingsCard card = new()
			{
				Location = new Point(0, 146),
				Size = new Size(818, 126),
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
				Text = "Synix Background Service",
				Font = new Font("Segoe UI", 12F, FontStyle.Bold),
				ForeColor = SettingsPalette.PrimaryText,
				Location = new Point(80, 18),
				Size = new Size(540, 30)
			});
			card.Controls.Add(new Label
			{
				Text = "Keeps crash recovery and smart maintenance active while the dashboard is closed. Runs only for this Windows user and requires no administrator password.",
				ForeColor = SettingsPalette.SecondaryText,
				Location = new Point(80, 48),
				Size = new Size(610, 44)
			});
			_backgroundServiceStatus = new Label
			{
				Text = "Disabled — scheduled work runs only while Synix is open.",
				ForeColor = SettingsPalette.SecondaryText,
				Location = new Point(80, 94),
				Size = new Size(610, 24)
			};
			card.Controls.Add(_backgroundServiceStatus);
			_backgroundServiceToggle = new ModernSettingsToggle
			{
				Location = new Point(739, 26),
				AccessibleName = "Synix background service",
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			};
			card.Controls.Add(_backgroundServiceToggle);
			Controls.Add(card);
			card.BringToFront();
		}

	}
}
