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
	public partial class PrivacySettingsPage : UserControl
	{
		private readonly ModernSettingsToggle _shareActivityCount = new();
		private readonly Label _remoteStatus = new();

		public PrivacySettingsPage()
		{
			InitializeComponent();
			AddActivityCountCard();
			AddRemoteManagerCard();
		}

		private void AddRemoteManagerCard()
		{
			var card = new ModernSettingsCard
			{
				Name = "remoteManagerCard", Location = new Point(0, 480), Size = new Size(818, 220),
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				BackColor = SettingsPalette.Window, FillColor = SettingsPalette.Card,
				BorderColor = SettingsPalette.Border, Padding = new Padding(22, 18, 22, 18)
			};
			var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = SettingsPalette.Card };
			layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
			layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
			layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
			layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
			var title = new Label { Dock = DockStyle.Fill, ForeColor = SettingsPalette.PrimaryText,
				Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
			LocalizationManager.BindText(title, "RemoteManager.Title");
			layout.Controls.Add(title, 0, 0);
			var description = new Label { Dock = DockStyle.Fill, ForeColor = SettingsPalette.SecondaryText, Font = new Font("Segoe UI", 9.5F) };
			LocalizationManager.BindText(description, "RemoteManager.Description");
			layout.Controls.Add(description, 0, 1);
			var button = new ModernSettingsButton { AutoSize = true, Dock = DockStyle.Left };
			LocalizationManager.BindText(button, "RemoteManager.ManagePairing");
			button.Click += (_, _) => { using var dialog = new RemoteManagerDialog(); dialog.ShowDialog(FindForm()); };
			_remoteStatus.Name = "remoteConnectionStatus";
			_remoteStatus.Dock = DockStyle.Fill;
			_remoteStatus.TextAlign = ContentAlignment.MiddleLeft;
			_remoteStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
			layout.Controls.Add(_remoteStatus, 0, 2);
			layout.Controls.Add(button, 0, 3); card.Controls.Add(layout); Controls.Add(card);
			void RefreshStatus()
			{
				var service = SynixEngine.RemoteManagerService.Shared;
				LocalizationManager.SetDataText(_remoteStatus, service.ConnectionStatus);
				_remoteStatus.ForeColor = service.IsConnected ? Color.Turquoise : SettingsPalette.SecondaryText;
			}
			// UI-only refresh; no network calls. Dispose with the page and pause when hidden.
			components ??= new Container();
			var timer = new System.Windows.Forms.Timer(components) { Interval = 1000 };
			timer.Tick += (_, _) => RefreshStatus();
			VisibleChanged += (_, _) => { timer.Enabled = Visible; if (Visible) RefreshStatus(); };
			RefreshStatus(); timer.Enabled = Visible;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ShareActivityCount
		{
			get => _shareActivityCount.Checked;
			set => _shareActivityCount.Checked = value;
		}

		[Browsable(false)]
		public event EventHandler? ShareActivityCountChanged
		{
			add => _shareActivityCount.CheckedChanged += value;
			remove => _shareActivityCount.CheckedChanged -= value;
		}

		private void AddActivityCountCard()
		{
			AutoScroll = true;
			var card = new ModernSettingsCard
			{
				Name = "activityCountCard", Location = new Point(0, 264), Size = new Size(818, 200),
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				BackColor = SettingsPalette.Window, FillColor = SettingsPalette.Card,
				BorderColor = SettingsPalette.Border, Padding = new Padding(22, 18, 22, 18)
			};
			var layout = new TableLayoutPanel
			{
				Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, BackColor = SettingsPalette.Card
			};
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
			var title = new Label
			{
				Text = LocalizationManager.Get("Settings.Privacy.ActivityCount.Title"), AutoSize = true,
				Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12F, FontStyle.Bold),
				ForeColor = SettingsPalette.PrimaryText, Margin = new Padding(0, 0, 12, 12)
			};
			var description = new Label
			{
				Text = LocalizationManager.Get("Settings.Privacy.ActivityCount.Description"),
				Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5F),
				ForeColor = SettingsPalette.SecondaryText, Margin = Padding.Empty
			};
			_shareActivityCount.Name = "shareActivityCount";
			_shareActivityCount.AccessibleName = title.Text;
			_shareActivityCount.AccessibleDescription = description.Text;
			_shareActivityCount.Size = new Size(54, 30);
			_shareActivityCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			layout.Controls.Add(title, 0, 0);
			layout.Controls.Add(_shareActivityCount, 1, 0);
			layout.Controls.Add(description, 0, 1);
			layout.SetColumnSpan(description, 2);
			card.Controls.Add(layout);
			Controls.Add(card);
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool PrivacyMode
		{
			get => chkPrivacyMode.Checked;
			set => chkPrivacyMode.Checked = value;
		}

		[Browsable(false)]
		public event EventHandler? PrivacyModeChanged
		{
			add => chkPrivacyMode.CheckedChanged += value;
			remove => chkPrivacyMode.CheckedChanged -= value;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CheckForDDoS
		{
			get => chkCheckForDDoS.Checked;
			set => chkCheckForDDoS.Checked = value;
		}

		[Browsable(false)]
		public event EventHandler? CheckForDDoSChanged
		{
			add => chkCheckForDDoS.CheckedChanged += value;
			remove => chkCheckForDDoS.CheckedChanged -= value;
		}
	}
}
