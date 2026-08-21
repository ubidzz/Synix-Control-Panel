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
namespace Synix_Control_Panel.SynixEngine
{
	partial class AppSettings
	{
		private System.ComponentModel.IContainer? components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppSettings));
			shellLayout = new TableLayoutPanel();
			titleBar = new Panel();
			picLogo = new PictureBox();
			lblWindowTitle = new Label();
			btnMinimize = new Button();
			btnClose = new Button();
			titleBottomBorder = new Label();
			bodyLayout = new TableLayoutPanel();
			sidebarPanel = new Panel();
			navigationFlow = new FlowLayoutPanel();
			btnGeneral = new Synix_Control_Panel.SynixApp.Design.ModernSettingsNavButton();
			btnBackups = new Synix_Control_Panel.SynixApp.Design.ModernSettingsNavButton();
			btnPrivacy = new Synix_Control_Panel.SynixApp.Design.ModernSettingsNavButton();
			btnAdvanced = new Synix_Control_Panel.SynixApp.Design.ModernSettingsNavButton();
			lblVersion = new Label();
			sidebarRightBorder = new Label();
			contentLayout = new TableLayoutPanel();
			contentHeader = new Panel();
			lblPageHeading = new Label();
			lblPageSubtitle = new Label();
			pageHost = new Panel();
			advancedSettingsPage = new AdvancedSettingsPage();
			privacySettingsPage = new PrivacySettingsPage();
			backupSettingsPage = new BackupSettingsPage();
			generalSettingsPage = new GeneralSettingsPage();
			shellLayout.SuspendLayout();
			titleBar.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
			bodyLayout.SuspendLayout();
			sidebarPanel.SuspendLayout();
			navigationFlow.SuspendLayout();
			contentLayout.SuspendLayout();
			contentHeader.SuspendLayout();
			pageHost.SuspendLayout();
			SuspendLayout();
			//
			// shellLayout
			//
			shellLayout.BackColor = Color.FromArgb(8, 13, 24);
			shellLayout.ColumnCount = 1;
			shellLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			shellLayout.Controls.Add(titleBar, 0, 0);
			shellLayout.Controls.Add(bodyLayout, 0, 1);
			shellLayout.Dock = DockStyle.Fill;
			shellLayout.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
			shellLayout.Location = new Point(1, 1);
			shellLayout.Margin = new Padding(0);
			shellLayout.Name = "shellLayout";
			shellLayout.RowCount = 2;
			shellLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
			shellLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			shellLayout.Size = new Size(1098, 718);
			shellLayout.TabIndex = 0;
			//
			// titleBar
			//
			titleBar.BackColor = Color.FromArgb(6, 12, 22);
			titleBar.Controls.Add(picLogo);
			titleBar.Controls.Add(lblWindowTitle);
			titleBar.Controls.Add(btnMinimize);
			titleBar.Controls.Add(btnClose);
			titleBar.Controls.Add(titleBottomBorder);
			titleBar.Dock = DockStyle.Fill;
			titleBar.Location = new Point(0, 0);
			titleBar.Margin = new Padding(0);
			titleBar.Name = "titleBar";
			titleBar.Size = new Size(1098, 56);
			titleBar.TabIndex = 0;
			titleBar.MouseDown += TitleBar_MouseDown;
			//
			// picLogo
			//
			picLogo.BackColor = Color.FromArgb(6, 12, 22);
			picLogo.Image = Properties.Resources.synix_logo;
			picLogo.Location = new Point(18, 13);
			picLogo.Name = "picLogo";
			picLogo.Size = new Size(30, 30);
			picLogo.SizeMode = PictureBoxSizeMode.Zoom;
			picLogo.TabIndex = 0;
			picLogo.TabStop = false;
			picLogo.MouseDown += TitleBar_MouseDown;
			//
			// lblWindowTitle
			//
			lblWindowTitle.AutoSize = true;
			lblWindowTitle.BackColor = Color.FromArgb(6, 12, 22);
			lblWindowTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
			lblWindowTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblWindowTitle.Location = new Point(58, 17);
			lblWindowTitle.Name = "lblWindowTitle";
			lblWindowTitle.Size = new Size(70, 21);
			lblWindowTitle.TabIndex = 1;
			lblWindowTitle.Text = "Settings";
			lblWindowTitle.MouseDown += TitleBar_MouseDown;
			//
			// btnMinimize
			//
			btnMinimize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnMinimize.BackColor = Color.FromArgb(6, 12, 22);
			btnMinimize.Cursor = Cursors.Hand;
			btnMinimize.FlatAppearance.BorderSize = 0;
			btnMinimize.FlatAppearance.MouseDownBackColor = Color.FromArgb(28, 42, 60);
			btnMinimize.FlatAppearance.MouseOverBackColor = Color.FromArgb(21, 34, 52);
			btnMinimize.FlatStyle = FlatStyle.Flat;
			btnMinimize.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
			btnMinimize.ForeColor = Color.FromArgb(245, 247, 251);
			btnMinimize.Location = new Point(1002, 0);
			btnMinimize.Name = "btnMinimize";
			btnMinimize.Size = new Size(48, 55);
			btnMinimize.TabIndex = 2;
			btnMinimize.TabStop = false;
			btnMinimize.Text = "—";
			btnMinimize.UseVisualStyleBackColor = false;
			btnMinimize.Click += btnMinimize_Click;
			//
			// btnClose
			//
			btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnClose.BackColor = Color.FromArgb(6, 12, 22);
			btnClose.Cursor = Cursors.Hand;
			btnClose.FlatAppearance.BorderSize = 0;
			btnClose.FlatAppearance.MouseDownBackColor = Color.FromArgb(175, 35, 50);
			btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(205, 48, 64);
			btnClose.FlatStyle = FlatStyle.Flat;
			btnClose.Font = new Font("Segoe UI", 15F, FontStyle.Regular, GraphicsUnit.Point);
			btnClose.ForeColor = Color.FromArgb(245, 247, 251);
			btnClose.Location = new Point(1050, 0);
			btnClose.Name = "btnClose";
			btnClose.Size = new Size(48, 55);
			btnClose.TabIndex = 3;
			btnClose.TabStop = false;
			btnClose.Text = "×";
			btnClose.UseVisualStyleBackColor = false;
			btnClose.Click += btnClose_Click;
			//
			// titleBottomBorder
			//
			titleBottomBorder.BackColor = Color.FromArgb(38, 52, 77);
			titleBottomBorder.Dock = DockStyle.Bottom;
			titleBottomBorder.Location = new Point(0, 55);
			titleBottomBorder.Margin = new Padding(0);
			titleBottomBorder.Name = "titleBottomBorder";
			titleBottomBorder.Size = new Size(1098, 1);
			titleBottomBorder.TabIndex = 4;
			//
			// bodyLayout
			//
			bodyLayout.BackColor = Color.FromArgb(8, 13, 24);
			bodyLayout.ColumnCount = 2;
			bodyLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 218F));
			bodyLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			bodyLayout.Controls.Add(sidebarPanel, 0, 0);
			bodyLayout.Controls.Add(contentLayout, 1, 0);
			bodyLayout.Dock = DockStyle.Fill;
			bodyLayout.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
			bodyLayout.Location = new Point(0, 56);
			bodyLayout.Margin = new Padding(0);
			bodyLayout.Name = "bodyLayout";
			bodyLayout.RowCount = 1;
			bodyLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			bodyLayout.Size = new Size(1098, 662);
			bodyLayout.TabIndex = 1;
			//
			// sidebarPanel
			//
			sidebarPanel.BackColor = Color.FromArgb(10, 18, 32);
			sidebarPanel.Controls.Add(navigationFlow);
			sidebarPanel.Controls.Add(lblVersion);
			sidebarPanel.Controls.Add(sidebarRightBorder);
			sidebarPanel.Dock = DockStyle.Fill;
			sidebarPanel.Location = new Point(0, 0);
			sidebarPanel.Margin = new Padding(0);
			sidebarPanel.Name = "sidebarPanel";
			sidebarPanel.Size = new Size(218, 662);
			sidebarPanel.TabIndex = 0;
			//
			// navigationFlow
			//
			navigationFlow.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			navigationFlow.BackColor = Color.FromArgb(10, 18, 32);
			navigationFlow.Controls.Add(btnGeneral);
			navigationFlow.Controls.Add(btnBackups);
			navigationFlow.Controls.Add(btnPrivacy);
			navigationFlow.Controls.Add(btnAdvanced);
			navigationFlow.FlowDirection = FlowDirection.TopDown;
			navigationFlow.Location = new Point(16, 24);
			navigationFlow.Margin = new Padding(0);
			navigationFlow.Name = "navigationFlow";
			navigationFlow.Size = new Size(185, 286);
			navigationFlow.TabIndex = 0;
			navigationFlow.WrapContents = false;
			//
			// btnGeneral
			//
			btnGeneral.BackColor = Color.FromArgb(10, 18, 32);
			btnGeneral.Cursor = Cursors.Hand;
			btnGeneral.FlatAppearance.BorderSize = 0;
			btnGeneral.FlatStyle = FlatStyle.Flat;
			btnGeneral.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
			btnGeneral.ForeColor = Color.FromArgb(158, 172, 194);
			btnGeneral.IconGlyph = "⚙";
			btnGeneral.Location = new Point(0, 0);
			btnGeneral.Margin = new Padding(0, 0, 0, 8);
			btnGeneral.Name = "btnGeneral";
			btnGeneral.Selected = true;
			btnGeneral.Size = new Size(185, 54);
			btnGeneral.TabIndex = 0;
			btnGeneral.Text = "General";
			btnGeneral.TextAlign = ContentAlignment.MiddleLeft;
			btnGeneral.UseVisualStyleBackColor = false;
			btnGeneral.Click += btnGeneral_Click;
			//
			// btnBackups
			//
			btnBackups.BackColor = Color.FromArgb(10, 18, 32);
			btnBackups.Cursor = Cursors.Hand;
			btnBackups.FlatAppearance.BorderSize = 0;
			btnBackups.FlatStyle = FlatStyle.Flat;
			btnBackups.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
			btnBackups.ForeColor = Color.FromArgb(158, 172, 194);
			btnBackups.IconGlyph = "▤";
			btnBackups.Location = new Point(0, 62);
			btnBackups.Margin = new Padding(0, 0, 0, 8);
			btnBackups.Name = "btnBackups";
			btnBackups.Selected = false;
			btnBackups.Size = new Size(185, 54);
			btnBackups.TabIndex = 1;
			btnBackups.Text = "Backups";
			btnBackups.TextAlign = ContentAlignment.MiddleLeft;
			btnBackups.UseVisualStyleBackColor = false;
			btnBackups.Click += btnBackups_Click;
			//
			// btnPrivacy
			//
			btnPrivacy.BackColor = Color.FromArgb(10, 18, 32);
			btnPrivacy.Cursor = Cursors.Hand;
			btnPrivacy.FlatAppearance.BorderSize = 0;
			btnPrivacy.FlatStyle = FlatStyle.Flat;
			btnPrivacy.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
			btnPrivacy.ForeColor = Color.FromArgb(158, 172, 194);
			btnPrivacy.IconGlyph = "◇";
			btnPrivacy.Location = new Point(0, 124);
			btnPrivacy.Margin = new Padding(0, 0, 0, 8);
			btnPrivacy.Name = "btnPrivacy";
			btnPrivacy.Selected = false;
			btnPrivacy.Size = new Size(185, 54);
			btnPrivacy.TabIndex = 2;
			btnPrivacy.Text = "Privacy & Security";
			btnPrivacy.TextAlign = ContentAlignment.MiddleLeft;
			btnPrivacy.UseVisualStyleBackColor = false;
			btnPrivacy.Click += btnPrivacy_Click;
			//
			// btnAdvanced
			//
			btnAdvanced.BackColor = Color.FromArgb(10, 18, 32);
			btnAdvanced.Cursor = Cursors.Hand;
			btnAdvanced.FlatAppearance.BorderSize = 0;
			btnAdvanced.FlatStyle = FlatStyle.Flat;
			btnAdvanced.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
			btnAdvanced.ForeColor = Color.FromArgb(158, 172, 194);
			btnAdvanced.IconGlyph = "⚡";
			btnAdvanced.Location = new Point(0, 186);
			btnAdvanced.Margin = new Padding(0, 0, 0, 8);
			btnAdvanced.Name = "btnAdvanced";
			btnAdvanced.Selected = false;
			btnAdvanced.Size = new Size(185, 54);
			btnAdvanced.TabIndex = 3;
			btnAdvanced.Text = "Advanced";
			btnAdvanced.TextAlign = ContentAlignment.MiddleLeft;
			btnAdvanced.UseVisualStyleBackColor = false;
			btnAdvanced.Click += btnAdvanced_Click;
			//
			// lblVersion
			//
			lblVersion.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			lblVersion.BackColor = Color.FromArgb(10, 18, 32);
			lblVersion.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
			lblVersion.ForeColor = Color.FromArgb(105, 124, 153);
			lblVersion.Location = new Point(20, 594);
			lblVersion.Name = "lblVersion";
			lblVersion.Size = new Size(180, 48);
			lblVersion.TabIndex = 1;
			lblVersion.Text = "SYNIX CONTROL PANEL  •  version";
			lblVersion.TextAlign = ContentAlignment.MiddleLeft;
			//
			// sidebarRightBorder
			//
			sidebarRightBorder.BackColor = Color.FromArgb(38, 52, 77);
			sidebarRightBorder.Dock = DockStyle.Right;
			sidebarRightBorder.Location = new Point(217, 0);
			sidebarRightBorder.Margin = new Padding(0);
			sidebarRightBorder.Name = "sidebarRightBorder";
			sidebarRightBorder.Size = new Size(1, 662);
			sidebarRightBorder.TabIndex = 2;
			//
			// contentLayout
			//
			contentLayout.BackColor = Color.FromArgb(8, 13, 24);
			contentLayout.ColumnCount = 1;
			contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			contentLayout.Controls.Add(contentHeader, 0, 0);
			contentLayout.Controls.Add(pageHost, 0, 1);
			contentLayout.Dock = DockStyle.Fill;
			contentLayout.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
			contentLayout.Location = new Point(218, 0);
			contentLayout.Margin = new Padding(0);
			contentLayout.Name = "contentLayout";
			contentLayout.RowCount = 2;
			contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 112F));
			contentLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			contentLayout.Size = new Size(880, 662);
			contentLayout.TabIndex = 1;
			//
			// contentHeader
			//
			contentHeader.BackColor = Color.FromArgb(8, 13, 24);
			contentHeader.Controls.Add(lblPageHeading);
			contentHeader.Controls.Add(lblPageSubtitle);
			contentHeader.Dock = DockStyle.Fill;
			contentHeader.Location = new Point(0, 0);
			contentHeader.Margin = new Padding(0);
			contentHeader.Name = "contentHeader";
			contentHeader.Size = new Size(880, 112);
			contentHeader.TabIndex = 0;
			//
			// lblPageHeading
			//
			lblPageHeading.AutoSize = true;
			lblPageHeading.BackColor = Color.FromArgb(8, 13, 24);
			lblPageHeading.Font = new Font("Segoe UI", 23F, FontStyle.Bold, GraphicsUnit.Point);
			lblPageHeading.ForeColor = Color.FromArgb(245, 247, 251);
			lblPageHeading.Location = new Point(36, 22);
			lblPageHeading.Name = "lblPageHeading";
			lblPageHeading.Size = new Size(134, 42);
			lblPageHeading.TabIndex = 0;
			lblPageHeading.Text = "General";
			//
			// lblPageSubtitle
			//
			lblPageSubtitle.AutoSize = true;
			lblPageSubtitle.BackColor = Color.FromArgb(8, 13, 24);
			lblPageSubtitle.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
			lblPageSubtitle.ForeColor = Color.FromArgb(158, 172, 194);
			lblPageSubtitle.Location = new Point(39, 69);
			lblPageSubtitle.Name = "lblPageSubtitle";
			lblPageSubtitle.Size = new Size(343, 19);
			lblPageSubtitle.TabIndex = 1;
			lblPageSubtitle.Text = "Configure basic Synix behavior on this computer.";
			//
			// pageHost
			//
			pageHost.BackColor = Color.FromArgb(8, 13, 24);
			pageHost.Controls.Add(advancedSettingsPage);
			pageHost.Controls.Add(privacySettingsPage);
			pageHost.Controls.Add(backupSettingsPage);
			pageHost.Controls.Add(generalSettingsPage);
			pageHost.Dock = DockStyle.Fill;
			pageHost.Location = new Point(0, 112);
			pageHost.Margin = new Padding(0);
			pageHost.Name = "pageHost";
			pageHost.Padding = new Padding(36, 0, 28, 30);
			pageHost.Size = new Size(880, 550);
			pageHost.TabIndex = 1;
			//
			// advancedSettingsPage
			//
			advancedSettingsPage.BackColor = Color.FromArgb(8, 13, 24);
			advancedSettingsPage.Dock = DockStyle.Fill;
			advancedSettingsPage.Location = new Point(36, 0);
			advancedSettingsPage.Margin = new Padding(0);
			advancedSettingsPage.Name = "advancedSettingsPage";
			advancedSettingsPage.Size = new Size(816, 520);
			advancedSettingsPage.TabIndex = 3;
			advancedSettingsPage.Visible = false;
			//
			// privacySettingsPage
			//
			privacySettingsPage.BackColor = Color.FromArgb(8, 13, 24);
			privacySettingsPage.Dock = DockStyle.Fill;
			privacySettingsPage.Location = new Point(36, 0);
			privacySettingsPage.Margin = new Padding(0);
			privacySettingsPage.Name = "privacySettingsPage";
			privacySettingsPage.Size = new Size(816, 520);
			privacySettingsPage.TabIndex = 2;
			privacySettingsPage.Visible = false;
			//
			// backupSettingsPage
			//
			backupSettingsPage.BackColor = Color.FromArgb(8, 13, 24);
			backupSettingsPage.Dock = DockStyle.Fill;
			backupSettingsPage.Location = new Point(36, 0);
			backupSettingsPage.Margin = new Padding(0);
			backupSettingsPage.Name = "backupSettingsPage";
			backupSettingsPage.Size = new Size(816, 520);
			backupSettingsPage.TabIndex = 1;
			backupSettingsPage.Visible = false;
			//
			// generalSettingsPage
			//
			generalSettingsPage.BackColor = Color.FromArgb(8, 13, 24);
			generalSettingsPage.Dock = DockStyle.Fill;
			generalSettingsPage.Location = new Point(36, 0);
			generalSettingsPage.Margin = new Padding(0);
			generalSettingsPage.Name = "generalSettingsPage";
			generalSettingsPage.Size = new Size(816, 520);
			generalSettingsPage.TabIndex = 0;
			//
			// AppSettings
			//
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			BackColor = Color.FromArgb(38, 52, 77);
			ClientSize = new Size(1100, 720);
			Controls.Add(shellLayout);
			Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
			ForeColor = Color.FromArgb(245, 247, 251);
			FormBorderStyle = FormBorderStyle.None;
			Icon = (Icon)resources.GetObject("$this.Icon");
			KeyPreview = true;
			MaximizeBox = false;
			MinimizeBox = true;
			MinimumSize = new Size(920, 620);
			Name = "AppSettings";
			Padding = new Padding(1);
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Synix Settings";
			shellLayout.ResumeLayout(false);
			titleBar.ResumeLayout(false);
			titleBar.PerformLayout();
			((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
			bodyLayout.ResumeLayout(false);
			sidebarPanel.ResumeLayout(false);
			navigationFlow.ResumeLayout(false);
			contentLayout.ResumeLayout(false);
			contentHeader.ResumeLayout(false);
			contentHeader.PerformLayout();
			pageHost.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private TableLayoutPanel shellLayout;
		private Panel titleBar;
		private PictureBox picLogo;
		private Label lblWindowTitle;
		private Button btnMinimize;
		private Button btnClose;
		private Label titleBottomBorder;
		private TableLayoutPanel bodyLayout;
		private Panel sidebarPanel;
		private FlowLayoutPanel navigationFlow;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsNavButton btnGeneral;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsNavButton btnBackups;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsNavButton btnPrivacy;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsNavButton btnAdvanced;
		private Label lblVersion;
		private Label sidebarRightBorder;
		private TableLayoutPanel contentLayout;
		private Panel contentHeader;
		private Label lblPageHeading;
		private Label lblPageSubtitle;
		private Panel pageHost;
		private GeneralSettingsPage generalSettingsPage;
		private BackupSettingsPage backupSettingsPage;
		private PrivacySettingsPage privacySettingsPage;
		private AdvancedSettingsPage advancedSettingsPage;
	}
}
