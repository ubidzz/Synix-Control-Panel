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
			AutoScroll = true;
			var cards = new TableLayoutPanel
			{
				Name = "privacySettingsCards", Dock = DockStyle.Top, AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, RowCount = 4,
				Margin = Padding.Empty
			};
			cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			ModernSettingsCard[] sections = [settingsCard, settingsCardDDoS, CreateActivityCountCard(), CreateRemoteManagerCard()];
			for (int index = 0; index < sections.Length; index++)
			{
				cards.RowStyles.Add(new RowStyle(SizeType.AutoSize));
				sections[index].Dock = DockStyle.Top;
				sections[index].Margin = new Padding(0, 0, 0, 14);
				sections[index].TabIndex = index;
				cards.Controls.Add(sections[index], 0, index);
			}
			Controls.Add(cards);
		}

		private ModernSettingsCard CreateRemoteManagerCard()
		{
			var card = new ModernSettingsCard
			{
				Name = "remoteManagerCard", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
				BackColor = SettingsPalette.Window, FillColor = SettingsPalette.Card,
				BorderColor = SettingsPalette.Border, Padding = new Padding(22, 16, 22, 16)
			};
			var layout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, RowCount = 4,
				Margin = Padding.Empty, BackColor = SettingsPalette.Card };
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			for (int row = 0; row < 4; row++) layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			var title = new Label { AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 8),
				ForeColor = SettingsPalette.PrimaryText, UseMnemonic = false, UseCompatibleTextRendering = false,
				Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
			LocalizationManager.BindText(title, "RemoteManager.Title");
			layout.Controls.Add(title, 0, 0);
			var description = new Label { Name = "remoteManagerDescription", AutoSize = true, Dock = DockStyle.Fill,
				Margin = new Padding(0, 0, 0, 10), UseMnemonic = false, UseCompatibleTextRendering = false,
				ForeColor = SettingsPalette.SecondaryText, Font = new Font("Segoe UI", 9.5F) };
			LocalizationManager.BindText(description, "RemoteManager.Description");
			layout.Controls.Add(description, 0, 1);
			var actions = new FlowLayoutPanel { Name = "remoteManagerActions", Dock = DockStyle.Fill,
				AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true,
				Margin = Padding.Empty, BackColor = SettingsPalette.Card };
			var button = new ModernSettingsButton { Name = "manageRemotePairing", AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(0, 36),
				Padding = new Padding(10, 4, 10, 4), Margin = new Padding(0, 0, 12, 6) };
			LocalizationManager.BindText(button, "RemoteManager.ManagePairing");
			var recover = new ModernSettingsButton { Name = "recoverWebsitePassword", AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(0, 36),
				Padding = new Padding(10, 4, 10, 4), Margin = new Padding(0, 0, 0, 6) };
			LocalizationManager.BindText(recover, "RemoteManager.RecoveryTitle");
			recover.Click += (_, _) =>
			{
				RefreshStatus();
				if (!recover.Enabled) return;
				using var dialog = new RemoteRecoveryDialog(SynixEngine.RemoteManagerService.Shared);
				dialog.ShowDialog(FindForm());
				RefreshStatus();
			};
			button.Click += (_, _) =>
			{
				using var dialog = new RemoteManagerDialog();
				dialog.ShowDialog(FindForm());
				RefreshStatus();
			};
			actions.Controls.Add(button); actions.Controls.Add(recover);
			_remoteStatus.Name = "remoteConnectionStatus";
			_remoteStatus.AutoSize = true;
			_remoteStatus.Dock = DockStyle.Fill;
			_remoteStatus.Margin = new Padding(0, 0, 0, 10);
			_remoteStatus.UseMnemonic = false;
			_remoteStatus.UseCompatibleTextRendering = false;
			_remoteStatus.TextAlign = ContentAlignment.MiddleLeft;
			_remoteStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
			layout.Controls.Add(_remoteStatus, 0, 2);
			layout.Controls.Add(actions, 0, 3); card.Controls.Add(layout);
			void FitContent()
			{
				int width = Math.Max(1, card.DisplayRectangle.Width);
				title.MaximumSize = description.MaximumSize = _remoteStatus.MaximumSize = new Size(width, 0);
				actions.MaximumSize = new Size(width, 0);
				button.MaximumSize = new Size(Math.Max(1, width - button.Margin.Horizontal), 0);
				recover.MaximumSize = new Size(width, 0);
			}
			card.SizeChanged += (_, _) => FitContent();
			DpiChangedAfterParent += (_, _) => FitContent();
			void RefreshStatus()
			{
				var service = SynixEngine.RemoteManagerService.Shared;
				LocalizationManager.SetDataText(_remoteStatus, service.ConnectionStatus);
				_remoteStatus.ForeColor = service.IsConnected ? Color.Turquoise : SettingsPalette.SecondaryText;
				recover.Enabled = service.State is { Enabled: true, PendingUnpair: false };
			}
			// UI-only refresh; no network calls. Dispose with the page and pause when hidden.
			components ??= new Container();
			var timer = new System.Windows.Forms.Timer(components) { Interval = 1000 };
			timer.Tick += (_, _) => RefreshStatus();
			VisibleChanged += (_, _) => { timer.Enabled = Visible; if (Visible) RefreshStatus(); };
			RefreshStatus(); timer.Enabled = Visible;
			return card;
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

		private ModernSettingsCard CreateActivityCountCard()
		{
			var card = new ModernSettingsCard
			{
				Name = "activityCountCard", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
				BackColor = SettingsPalette.Window, FillColor = SettingsPalette.Card,
				BorderColor = SettingsPalette.Border, Padding = new Padding(22, 16, 22, 16)
			};
			var layout = new TableLayoutPanel
			{
				Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
				ColumnCount = 2, RowCount = 2, Margin = Padding.Empty, BackColor = SettingsPalette.Card
			};
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			var title = new Label
			{
				AutoSize = true, UseMnemonic = false, UseCompatibleTextRendering = false,
				Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12F, FontStyle.Bold),
				ForeColor = SettingsPalette.PrimaryText, Margin = new Padding(0, 0, 12, 8)
			};
			LocalizationManager.BindText(title, "Settings.Privacy.ActivityCount.Title");
			var description = new Label
			{
				Name = "activityCountDescription", AutoSize = true, UseMnemonic = false, UseCompatibleTextRendering = false,
				Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5F),
				ForeColor = SettingsPalette.SecondaryText, Margin = Padding.Empty
			};
			LocalizationManager.BindText(description, "Settings.Privacy.ActivityCount.Description");
			_shareActivityCount.Name = "shareActivityCount";
			LocalizationManager.BindAccessibleName(_shareActivityCount, "Settings.Privacy.ActivityCount.Title");
			LocalizationManager.BindAccessibleDescription(_shareActivityCount, "Settings.Privacy.ActivityCount.Description");
			_shareActivityCount.Size = new Size(54, 30);
			_shareActivityCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			layout.Controls.Add(title, 0, 0);
			layout.Controls.Add(_shareActivityCount, 1, 0);
			layout.Controls.Add(description, 0, 1);
			layout.SetColumnSpan(description, 2);
			card.Controls.Add(layout);
			void FitContent()
			{
				int width = Math.Max(1, card.DisplayRectangle.Width);
				title.MaximumSize = new Size(Math.Max(1, width - (int)layout.ColumnStyles[1].Width - title.Margin.Horizontal), 0);
				description.MaximumSize = new Size(width, 0);
			}
			card.SizeChanged += (_, _) => FitContent();
			DpiChangedAfterParent += (_, _) => FitContent();
			return card;
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
