// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================

namespace Synix_Control_Panel.Help
{
	partial class ServerInfo
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerInfo));
			shellLayout = new TableLayoutPanel();
			titleBar = new Panel();
			picLogo = new PictureBox();
			lblWindowTitle = new Label();
			btnMinimize = new Button();
			btnClose = new Button();
			titleBottomBorder = new Label();
			contentScroll = new Panel();
			contentLayout = new TableLayoutPanel();
			headerPanel = new Panel();
			lblPageHeading = new Label();
			lblPageSubtitle = new Label();
			lblLiveIndicator = new Label();
			metricsLayout = new TableLayoutPanel();
			pnlCpuCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			lblCpuTitle = new Label();
			lblCpuCardValue = new Label();
			lblCpuCaption = new Label();
			pnlCpuTrack = new Panel();
			pnlCpuFill = new Panel();
			pnlRamCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			lblRamTitle = new Label();
			lblRamCardValue = new Label();
			lblRamCaption = new Label();
			pnlRamTrack = new Panel();
			pnlRamFill = new Panel();
			pnlStatusCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			pnlStatusIndicator = new Panel();
			lblStatusTitle = new Label();
			lblStatusCardValue = new Label();
			lblStatusCaption = new Label();
			lblProcessIdCaption = new Label();
			lblProcessIdValue = new Label();
			detailsLayout = new TableLayoutPanel();
			pnlDetailsCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			lblDetailsTitle = new Label();
			lblDetailsSubtitle = new Label();
			detailsTable = new TableLayoutPanel();
			lblServerNameCaption = new Label();
			lblServerNameText = new Label();
			lblGameServerCaption = new Label();
			lblGameServerText = new Label();
			lblGameVersionCaption = new Label();
			lblGameVersion = new Label();
			lblMapCaption = new Label();
			lblMapText = new Label();
			lblSeedCaption = new Label();
			lblSeedText = new Label();
			lblGameModeCaption = new Label();
			lblCompetitiveText = new Label();
			lblMaxPlayersCaption = new Label();
			lblMaxPlayersText = new Label();
			lblGamePortCaption = new Label();
			lblGamePortText = new Label();
			lblQueryPortCaption = new Label();
			lblQueryPortText = new Label();
			lblAppPortCaption = new Label();
			lblAppPortText = new Label();
			pnlConfigurationCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			lblConfigurationTitle = new Label();
			lblConfigurationSubtitle = new Label();
			configurationTable = new TableLayoutPanel();
			lblServerPasswordCaption = new Label();
			lblServerPasswordText = new Label();
			lblAdminPasswordCaption = new Label();
			lblServerAdminPasswordText = new Label();
			lblRconCaption = new Label();
			lblRconActiveText = new Label();
			lblRconPortCaption = new Label();
			lblRconPortText = new Label();
			lblRconPasswordCaption = new Label();
			lblRconPasswordText = new Label();
			lblBackupOnStartCaption = new Label();
			lblBackupOnStartText = new Label();
			lblUpdateOnStartCaption = new Label();
			lbllUpdateOnStartText = new Label();
			lblDiscordAlertsCaption = new Label();
			lblDiscordActivateText = new Label();
			lblAutoRestartCaption = new Label();
			lblAutoRestartText = new Label();
			pnlPathsCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			lblPathsTitle = new Label();
			lblPathsSubtitle = new Label();
			pathsTable = new TableLayoutPanel();
			lblServerFolderCaption = new Label();
			txtServerFolderValue = new TextBox();
			lblDiscordWebhookCaption = new Label();
			txtDiscordWebhookValue = new TextBox();
			lblExtraArgsCaption = new Label();
			txtExtraArgsValue = new TextBox();
			shellLayout.SuspendLayout();
			titleBar.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
			contentScroll.SuspendLayout();
			contentLayout.SuspendLayout();
			headerPanel.SuspendLayout();
			metricsLayout.SuspendLayout();
			pnlCpuCard.SuspendLayout();
			pnlCpuTrack.SuspendLayout();
			pnlRamCard.SuspendLayout();
			pnlRamTrack.SuspendLayout();
			pnlStatusCard.SuspendLayout();
			detailsLayout.SuspendLayout();
			pnlDetailsCard.SuspendLayout();
			detailsTable.SuspendLayout();
			pnlConfigurationCard.SuspendLayout();
			configurationTable.SuspendLayout();
			pnlPathsCard.SuspendLayout();
			pathsTable.SuspendLayout();
			SuspendLayout();
			// 
			// shellLayout
			// 
			shellLayout.BackColor = Color.FromArgb(8, 13, 24);
			shellLayout.ColumnCount = 1;
			shellLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			shellLayout.Controls.Add(titleBar, 0, 0);
			shellLayout.Controls.Add(contentScroll, 0, 1);
			shellLayout.Dock = DockStyle.Fill;
			shellLayout.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
			shellLayout.Location = new Point(1, 1);
			shellLayout.Margin = new Padding(0);
			shellLayout.Name = "shellLayout";
			shellLayout.RowCount = 2;
			shellLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
			shellLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			shellLayout.Size = new Size(1118, 758);
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
			titleBar.Size = new Size(1118, 56);
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
			lblWindowTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblWindowTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblWindowTitle.Location = new Point(58, 17);
			lblWindowTitle.Name = "lblWindowTitle";
			lblWindowTitle.Size = new Size(94, 21);
			lblWindowTitle.TabIndex = 1;
			lblWindowTitle.Text = "Server Info";
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
			btnMinimize.Font = new Font("Segoe UI", 12F);
			btnMinimize.ForeColor = Color.FromArgb(245, 247, 251);
			btnMinimize.Location = new Point(1022, 0);
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
			btnClose.Font = new Font("Segoe UI", 15F);
			btnClose.ForeColor = Color.FromArgb(245, 247, 251);
			btnClose.Location = new Point(1070, 0);
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
			titleBottomBorder.Size = new Size(1118, 1);
			titleBottomBorder.TabIndex = 4;
			// 
			// contentScroll
			// 
			contentScroll.AutoScroll = true;
			contentScroll.AutoScrollMinSize = new Size(0, 848);
			contentScroll.BackColor = Color.FromArgb(8, 13, 24);
			contentScroll.Controls.Add(contentLayout);
			contentScroll.Dock = DockStyle.Fill;
			contentScroll.Location = new Point(0, 56);
			contentScroll.Margin = new Padding(0);
			contentScroll.Name = "contentScroll";
			contentScroll.Padding = new Padding(28, 24, 28, 28);
			contentScroll.Size = new Size(1118, 702);
			contentScroll.TabIndex = 1;
			// 
			// contentLayout
			// 
			contentLayout.BackColor = Color.FromArgb(8, 13, 24);
			contentLayout.ColumnCount = 1;
			contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			contentLayout.Controls.Add(headerPanel, 0, 0);
			contentLayout.Controls.Add(metricsLayout, 0, 1);
			contentLayout.Controls.Add(detailsLayout, 0, 3);
			contentLayout.Controls.Add(pnlPathsCard, 0, 5);
			contentLayout.Dock = DockStyle.Top;
			contentLayout.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
			contentLayout.Location = new Point(28, 24);
			contentLayout.Margin = new Padding(0);
			contentLayout.Name = "contentLayout";
			contentLayout.RowCount = 7;
			contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
			contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 132F));
			contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 16F));
			contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 375F));
			contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 16F));
			contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 205F));
			contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
			contentLayout.Size = new Size(1045, 848);
			contentLayout.TabIndex = 0;
			// 
			// headerPanel
			// 
			headerPanel.BackColor = Color.FromArgb(8, 13, 24);
			headerPanel.Controls.Add(lblPageHeading);
			headerPanel.Controls.Add(lblPageSubtitle);
			headerPanel.Controls.Add(lblLiveIndicator);
			headerPanel.Dock = DockStyle.Fill;
			headerPanel.Location = new Point(0, 0);
			headerPanel.Margin = new Padding(0);
			headerPanel.Name = "headerPanel";
			headerPanel.Size = new Size(1045, 80);
			headerPanel.TabIndex = 0;
			// 
			// lblPageHeading
			// 
			lblPageHeading.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblPageHeading.AutoEllipsis = true;
			lblPageHeading.BackColor = Color.FromArgb(8, 13, 24);
			lblPageHeading.Font = new Font("Segoe UI", 23F, FontStyle.Bold);
			lblPageHeading.ForeColor = Color.FromArgb(245, 247, 251);
			lblPageHeading.Location = new Point(2, 3);
			lblPageHeading.Name = "lblPageHeading";
			lblPageHeading.Size = new Size(850, 43);
			lblPageHeading.TabIndex = 0;
			lblPageHeading.Text = "Server Overview";
			// 
			// lblPageSubtitle
			// 
			lblPageSubtitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblPageSubtitle.AutoEllipsis = true;
			lblPageSubtitle.BackColor = Color.FromArgb(8, 13, 24);
			lblPageSubtitle.Font = new Font("Segoe UI", 10F);
			lblPageSubtitle.ForeColor = Color.FromArgb(158, 172, 194);
			lblPageSubtitle.Location = new Point(4, 49);
			lblPageSubtitle.Name = "lblPageSubtitle";
			lblPageSubtitle.Size = new Size(900, 24);
			lblPageSubtitle.TabIndex = 1;
			lblPageSubtitle.Text = "Live performance and configuration details";
			// 
			// lblLiveIndicator
			// 
			lblLiveIndicator.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lblLiveIndicator.BackColor = Color.FromArgb(28, 75, 91);
			lblLiveIndicator.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
			lblLiveIndicator.ForeColor = Color.FromArgb(32, 214, 199);
			lblLiveIndicator.Location = new Point(910, 10);
			lblLiveIndicator.Name = "lblLiveIndicator";
			lblLiveIndicator.Size = new Size(128, 28);
			lblLiveIndicator.TabIndex = 2;
			lblLiveIndicator.Text = "●  LIVE TELEMETRY";
			lblLiveIndicator.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// metricsLayout
			// 
			metricsLayout.BackColor = Color.FromArgb(8, 13, 24);
			metricsLayout.ColumnCount = 3;
			metricsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
			metricsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
			metricsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33334F));
			metricsLayout.Controls.Add(pnlCpuCard, 0, 0);
			metricsLayout.Controls.Add(pnlRamCard, 1, 0);
			metricsLayout.Controls.Add(pnlStatusCard, 2, 0);
			metricsLayout.Dock = DockStyle.Fill;
			metricsLayout.Location = new Point(0, 80);
			metricsLayout.Margin = new Padding(0);
			metricsLayout.Name = "metricsLayout";
			metricsLayout.RowCount = 1;
			metricsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			metricsLayout.Size = new Size(1045, 132);
			metricsLayout.TabIndex = 1;
			// 
			// pnlCpuCard
			// 
			pnlCpuCard.BackColor = Color.FromArgb(17, 27, 45);
			pnlCpuCard.BorderColor = Color.FromArgb(38, 52, 77);
			pnlCpuCard.Controls.Add(lblCpuTitle);
			pnlCpuCard.Controls.Add(lblCpuCardValue);
			pnlCpuCard.Controls.Add(lblCpuCaption);
			pnlCpuCard.Controls.Add(pnlCpuTrack);
			pnlCpuCard.CornerRadius = 14;
			pnlCpuCard.Dock = DockStyle.Fill;
			pnlCpuCard.FillColor = Color.FromArgb(17, 27, 45);
			pnlCpuCard.Location = new Point(0, 0);
			pnlCpuCard.Margin = new Padding(0, 0, 8, 0);
			pnlCpuCard.Name = "pnlCpuCard";
			pnlCpuCard.Size = new Size(340, 132);
			pnlCpuCard.TabIndex = 0;
			// 
			// lblCpuTitle
			// 
			lblCpuTitle.AutoSize = true;
			lblCpuTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblCpuTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
			lblCpuTitle.ForeColor = Color.FromArgb(158, 172, 194);
			lblCpuTitle.Location = new Point(22, 15);
			lblCpuTitle.Name = "lblCpuTitle";
			lblCpuTitle.Size = new Size(84, 19);
			lblCpuTitle.TabIndex = 0;
			lblCpuTitle.Text = "CPU USAGE";
			// 
			// lblCpuCardValue
			// 
			lblCpuCardValue.AutoSize = true;
			lblCpuCardValue.BackColor = Color.FromArgb(17, 27, 45);
			lblCpuCardValue.Font = new Font("Segoe UI", 23F, FontStyle.Bold);
			lblCpuCardValue.ForeColor = Color.FromArgb(245, 247, 251);
			lblCpuCardValue.Location = new Point(19, 34);
			lblCpuCardValue.Name = "lblCpuCardValue";
			lblCpuCardValue.Size = new Size(89, 42);
			lblCpuCardValue.TabIndex = 1;
			lblCpuCardValue.Text = "0.0%";
			// 
			// lblCpuCaption
			// 
			lblCpuCaption.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			lblCpuCaption.AutoEllipsis = true;
			lblCpuCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblCpuCaption.Font = new Font("Segoe UI", 8.75F);
			lblCpuCaption.ForeColor = Color.FromArgb(105, 124, 153);
			lblCpuCaption.Location = new Point(22, 80);
			lblCpuCaption.Name = "lblCpuCaption";
			lblCpuCaption.Size = new Size(295, 19);
			lblCpuCaption.TabIndex = 2;
			lblCpuCaption.Text = "Waiting for a running server process";
			// 
			// pnlCpuTrack
			// 
			pnlCpuTrack.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			pnlCpuTrack.BackColor = Color.FromArgb(12, 21, 36);
			pnlCpuTrack.Controls.Add(pnlCpuFill);
			pnlCpuTrack.Location = new Point(22, 108);
			pnlCpuTrack.Name = "pnlCpuTrack";
			pnlCpuTrack.Size = new Size(295, 8);
			pnlCpuTrack.TabIndex = 3;
			pnlCpuTrack.SizeChanged += MetricTrack_SizeChanged;
			// 
			// pnlCpuFill
			// 
			pnlCpuFill.BackColor = Color.FromArgb(32, 214, 199);
			pnlCpuFill.Location = new Point(0, 0);
			pnlCpuFill.Name = "pnlCpuFill";
			pnlCpuFill.Size = new Size(1, 8);
			pnlCpuFill.TabIndex = 0;
			// 
			// pnlRamCard
			// 
			pnlRamCard.BackColor = Color.FromArgb(17, 27, 45);
			pnlRamCard.BorderColor = Color.FromArgb(38, 52, 77);
			pnlRamCard.Controls.Add(lblRamTitle);
			pnlRamCard.Controls.Add(lblRamCardValue);
			pnlRamCard.Controls.Add(lblRamCaption);
			pnlRamCard.Controls.Add(pnlRamTrack);
			pnlRamCard.CornerRadius = 14;
			pnlRamCard.Dock = DockStyle.Fill;
			pnlRamCard.FillColor = Color.FromArgb(17, 27, 45);
			pnlRamCard.Location = new Point(356, 0);
			pnlRamCard.Margin = new Padding(8, 0, 8, 0);
			pnlRamCard.Name = "pnlRamCard";
			pnlRamCard.Size = new Size(332, 132);
			pnlRamCard.TabIndex = 1;
			// 
			// lblRamTitle
			// 
			lblRamTitle.AutoSize = true;
			lblRamTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblRamTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
			lblRamTitle.ForeColor = Color.FromArgb(158, 172, 194);
			lblRamTitle.Location = new Point(22, 15);
			lblRamTitle.Name = "lblRamTitle";
			lblRamTitle.Size = new Size(88, 19);
			lblRamTitle.TabIndex = 0;
			lblRamTitle.Text = "RAM USAGE";
			// 
			// lblRamCardValue
			// 
			lblRamCardValue.AutoSize = true;
			lblRamCardValue.BackColor = Color.FromArgb(17, 27, 45);
			lblRamCardValue.Font = new Font("Segoe UI", 23F, FontStyle.Bold);
			lblRamCardValue.ForeColor = Color.FromArgb(245, 247, 251);
			lblRamCardValue.Location = new Point(19, 34);
			lblRamCardValue.Name = "lblRamCardValue";
			lblRamCardValue.Size = new Size(131, 42);
			lblRamCardValue.TabIndex = 1;
			lblRamCardValue.Text = "0.00 GB";
			// 
			// lblRamCaption
			// 
			lblRamCaption.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			lblRamCaption.AutoEllipsis = true;
			lblRamCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblRamCaption.Font = new Font("Segoe UI", 8.75F);
			lblRamCaption.ForeColor = Color.FromArgb(105, 124, 153);
			lblRamCaption.Location = new Point(22, 80);
			lblRamCaption.Name = "lblRamCaption";
			lblRamCaption.Size = new Size(287, 19);
			lblRamCaption.TabIndex = 2;
			lblRamCaption.Text = "0.0% of system memory";
			// 
			// pnlRamTrack
			// 
			pnlRamTrack.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			pnlRamTrack.BackColor = Color.FromArgb(12, 21, 36);
			pnlRamTrack.Controls.Add(pnlRamFill);
			pnlRamTrack.Location = new Point(22, 108);
			pnlRamTrack.Name = "pnlRamTrack";
			pnlRamTrack.Size = new Size(287, 8);
			pnlRamTrack.TabIndex = 3;
			pnlRamTrack.SizeChanged += MetricTrack_SizeChanged;
			// 
			// pnlRamFill
			// 
			pnlRamFill.BackColor = Color.FromArgb(167, 139, 250);
			pnlRamFill.Location = new Point(0, 0);
			pnlRamFill.Name = "pnlRamFill";
			pnlRamFill.Size = new Size(1, 8);
			pnlRamFill.TabIndex = 0;
			// 
			// pnlStatusCard
			// 
			pnlStatusCard.BackColor = Color.FromArgb(17, 27, 45);
			pnlStatusCard.BorderColor = Color.FromArgb(38, 52, 77);
			pnlStatusCard.Controls.Add(pnlStatusIndicator);
			pnlStatusCard.Controls.Add(lblStatusTitle);
			pnlStatusCard.Controls.Add(lblStatusCardValue);
			pnlStatusCard.Controls.Add(lblStatusCaption);
			pnlStatusCard.Controls.Add(lblProcessIdCaption);
			pnlStatusCard.Controls.Add(lblProcessIdValue);
			pnlStatusCard.CornerRadius = 14;
			pnlStatusCard.Dock = DockStyle.Fill;
			pnlStatusCard.FillColor = Color.FromArgb(17, 27, 45);
			pnlStatusCard.Location = new Point(704, 0);
			pnlStatusCard.Margin = new Padding(8, 0, 0, 0);
			pnlStatusCard.Name = "pnlStatusCard";
			pnlStatusCard.Size = new Size(341, 132);
			pnlStatusCard.TabIndex = 2;
			// 
			// pnlStatusIndicator
			// 
			pnlStatusIndicator.BackColor = Color.FromArgb(248, 113, 113);
			pnlStatusIndicator.Location = new Point(22, 20);
			pnlStatusIndicator.Name = "pnlStatusIndicator";
			pnlStatusIndicator.Size = new Size(10, 10);
			pnlStatusIndicator.TabIndex = 0;
			// 
			// lblStatusTitle
			// 
			lblStatusTitle.AutoSize = true;
			lblStatusTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblStatusTitle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
			lblStatusTitle.ForeColor = Color.FromArgb(158, 172, 194);
			lblStatusTitle.Location = new Point(40, 15);
			lblStatusTitle.Name = "lblStatusTitle";
			lblStatusTitle.Size = new Size(111, 19);
			lblStatusTitle.TabIndex = 1;
			lblStatusTitle.Text = "SERVER STATUS";
			// 
			// lblStatusCardValue
			// 
			lblStatusCardValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblStatusCardValue.AutoEllipsis = true;
			lblStatusCardValue.BackColor = Color.FromArgb(17, 27, 45);
			lblStatusCardValue.Font = new Font("Segoe UI", 19F, FontStyle.Bold);
			lblStatusCardValue.ForeColor = Color.FromArgb(248, 113, 113);
			lblStatusCardValue.Location = new Point(19, 37);
			lblStatusCardValue.Name = "lblStatusCardValue";
			lblStatusCardValue.Size = new Size(299, 39);
			lblStatusCardValue.TabIndex = 2;
			lblStatusCardValue.Text = "Stopped";
			// 
			// lblStatusCaption
			// 
			lblStatusCaption.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblStatusCaption.AutoEllipsis = true;
			lblStatusCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblStatusCaption.Font = new Font("Segoe UI", 8.75F);
			lblStatusCaption.ForeColor = Color.FromArgb(105, 124, 153);
			lblStatusCaption.Location = new Point(22, 77);
			lblStatusCaption.Name = "lblStatusCaption";
			lblStatusCaption.Size = new Size(296, 18);
			lblStatusCaption.TabIndex = 3;
			lblStatusCaption.Text = "The game server process is not running";
			// 
			// lblProcessIdCaption
			// 
			lblProcessIdCaption.AutoSize = true;
			lblProcessIdCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblProcessIdCaption.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
			lblProcessIdCaption.ForeColor = Color.FromArgb(105, 124, 153);
			lblProcessIdCaption.Location = new Point(22, 104);
			lblProcessIdCaption.Name = "lblProcessIdCaption";
			lblProcessIdCaption.Size = new Size(27, 15);
			lblProcessIdCaption.TabIndex = 4;
			lblProcessIdCaption.Text = "PID";
			// 
			// lblProcessIdValue
			// 
			lblProcessIdValue.AutoSize = true;
			lblProcessIdValue.BackColor = Color.FromArgb(17, 27, 45);
			lblProcessIdValue.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
			lblProcessIdValue.ForeColor = Color.FromArgb(245, 247, 251);
			lblProcessIdValue.Location = new Point(55, 104);
			lblProcessIdValue.Name = "lblProcessIdValue";
			lblProcessIdValue.Size = new Size(19, 15);
			lblProcessIdValue.TabIndex = 5;
			lblProcessIdValue.Text = "—";
			// 
			// detailsLayout
			// 
			detailsLayout.BackColor = Color.FromArgb(8, 13, 24);
			detailsLayout.ColumnCount = 2;
			detailsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			detailsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			detailsLayout.Controls.Add(pnlDetailsCard, 0, 0);
			detailsLayout.Controls.Add(pnlConfigurationCard, 1, 0);
			detailsLayout.Dock = DockStyle.Fill;
			detailsLayout.Location = new Point(0, 228);
			detailsLayout.Margin = new Padding(0);
			detailsLayout.Name = "detailsLayout";
			detailsLayout.RowCount = 1;
			detailsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			detailsLayout.Size = new Size(1045, 375);
			detailsLayout.TabIndex = 2;
			// 
			// pnlDetailsCard
			// 
			pnlDetailsCard.BackColor = Color.FromArgb(17, 27, 45);
			pnlDetailsCard.BorderColor = Color.FromArgb(38, 52, 77);
			pnlDetailsCard.Controls.Add(lblDetailsTitle);
			pnlDetailsCard.Controls.Add(lblDetailsSubtitle);
			pnlDetailsCard.Controls.Add(detailsTable);
			pnlDetailsCard.CornerRadius = 14;
			pnlDetailsCard.Dock = DockStyle.Fill;
			pnlDetailsCard.FillColor = Color.FromArgb(17, 27, 45);
			pnlDetailsCard.Location = new Point(0, 0);
			pnlDetailsCard.Margin = new Padding(0, 0, 8, 0);
			pnlDetailsCard.Name = "pnlDetailsCard";
			pnlDetailsCard.Size = new Size(514, 375);
			pnlDetailsCard.TabIndex = 0;
			// 
			// lblDetailsTitle
			// 
			lblDetailsTitle.AutoSize = true;
			lblDetailsTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblDetailsTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
			lblDetailsTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblDetailsTitle.Location = new Point(22, 17);
			lblDetailsTitle.Name = "lblDetailsTitle";
			lblDetailsTitle.Size = new Size(145, 28);
			lblDetailsTitle.TabIndex = 0;
			lblDetailsTitle.Text = "Server Details";
			// 
			// lblDetailsSubtitle
			// 
			lblDetailsSubtitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblDetailsSubtitle.AutoEllipsis = true;
			lblDetailsSubtitle.BackColor = Color.FromArgb(17, 27, 45);
			lblDetailsSubtitle.Font = new Font("Segoe UI", 9F);
			lblDetailsSubtitle.ForeColor = Color.FromArgb(158, 172, 194);
			lblDetailsSubtitle.Location = new Point(24, 49);
			lblDetailsSubtitle.Name = "lblDetailsSubtitle";
			lblDetailsSubtitle.Size = new Size(466, 20);
			lblDetailsSubtitle.TabIndex = 1;
			lblDetailsSubtitle.Text = "Identity, world, player, and network information";
			// 
			// detailsTable
			// 
			detailsTable.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			detailsTable.BackColor = Color.FromArgb(17, 27, 45);
			detailsTable.ColumnCount = 2;
			detailsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132F));
			detailsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			detailsTable.Controls.Add(lblServerNameCaption, 0, 0);
			detailsTable.Controls.Add(lblServerNameText, 1, 0);
			detailsTable.Controls.Add(lblGameServerCaption, 0, 1);
			detailsTable.Controls.Add(lblGameServerText, 1, 1);
			detailsTable.Controls.Add(lblGameVersionCaption, 0, 2);
			detailsTable.Controls.Add(lblGameVersion, 1, 2);
			detailsTable.Controls.Add(lblMapCaption, 0, 3);
			detailsTable.Controls.Add(lblMapText, 1, 3);
			detailsTable.Controls.Add(lblSeedCaption, 0, 4);
			detailsTable.Controls.Add(lblSeedText, 1, 4);
			detailsTable.Controls.Add(lblGameModeCaption, 0, 5);
			detailsTable.Controls.Add(lblCompetitiveText, 1, 5);
			detailsTable.Controls.Add(lblMaxPlayersCaption, 0, 6);
			detailsTable.Controls.Add(lblMaxPlayersText, 1, 6);
			detailsTable.Controls.Add(lblGamePortCaption, 0, 7);
			detailsTable.Controls.Add(lblGamePortText, 1, 7);
			detailsTable.Controls.Add(lblQueryPortCaption, 0, 8);
			detailsTable.Controls.Add(lblQueryPortText, 1, 8);
			detailsTable.Controls.Add(lblAppPortCaption, 0, 9);
			detailsTable.Controls.Add(lblAppPortText, 1, 9);
			detailsTable.Location = new Point(22, 78);
			detailsTable.Margin = new Padding(0);
			detailsTable.Name = "detailsTable";
			detailsTable.RowCount = 10;
			detailsTable.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
			detailsTable.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
			detailsTable.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
			detailsTable.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
			detailsTable.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
			detailsTable.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
			detailsTable.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
			detailsTable.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
			detailsTable.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
			detailsTable.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
			detailsTable.Size = new Size(470, 272);
			detailsTable.TabIndex = 2;
			// 
			// lblServerNameCaption
			// 
			lblServerNameCaption.AutoEllipsis = true;
			lblServerNameCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblServerNameCaption.Dock = DockStyle.Fill;
			lblServerNameCaption.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
			lblServerNameCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblServerNameCaption.Location = new Point(0, 0);
			lblServerNameCaption.Margin = new Padding(0);
			lblServerNameCaption.Name = "lblServerNameCaption";
			lblServerNameCaption.Size = new Size(132, 27);
			lblServerNameCaption.TabIndex = 0;
			lblServerNameCaption.Text = "Server Name";
			lblServerNameCaption.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblServerNameText
			// 
			lblServerNameText.AutoEllipsis = true;
			lblServerNameText.BackColor = Color.FromArgb(17, 27, 45);
			lblServerNameText.Dock = DockStyle.Fill;
			lblServerNameText.Font = new Font("Segoe UI", 9.25F);
			lblServerNameText.ForeColor = Color.FromArgb(245, 247, 251);
			lblServerNameText.Location = new Point(132, 0);
			lblServerNameText.Margin = new Padding(0);
			lblServerNameText.Name = "lblServerNameText";
			lblServerNameText.Size = new Size(338, 27);
			lblServerNameText.TabIndex = 1;
			lblServerNameText.Text = "My Dedicated Server";
			lblServerNameText.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblGameServerCaption
			// 
			lblGameServerCaption.AutoEllipsis = true;
			lblGameServerCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblGameServerCaption.Dock = DockStyle.Fill;
			lblGameServerCaption.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
			lblGameServerCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblGameServerCaption.Location = new Point(0, 27);
			lblGameServerCaption.Margin = new Padding(0);
			lblGameServerCaption.Name = "lblGameServerCaption";
			lblGameServerCaption.Size = new Size(132, 27);
			lblGameServerCaption.TabIndex = 2;
			lblGameServerCaption.Text = "Game Server";
			lblGameServerCaption.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblGameServerText
			// 
			lblGameServerText.AutoEllipsis = true;
			lblGameServerText.BackColor = Color.FromArgb(17, 27, 45);
			lblGameServerText.Dock = DockStyle.Fill;
			lblGameServerText.Font = new Font("Segoe UI", 9.25F);
			lblGameServerText.ForeColor = Color.FromArgb(245, 247, 251);
			lblGameServerText.Location = new Point(132, 27);
			lblGameServerText.Margin = new Padding(0);
			lblGameServerText.Name = "lblGameServerText";
			lblGameServerText.Size = new Size(338, 27);
			lblGameServerText.TabIndex = 3;
			lblGameServerText.Text = "Example Game";
			lblGameServerText.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblGameVersionCaption
			// 
			lblGameVersionCaption.AutoEllipsis = true;
			lblGameVersionCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblGameVersionCaption.Dock = DockStyle.Fill;
			lblGameVersionCaption.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
			lblGameVersionCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblGameVersionCaption.Location = new Point(0, 54);
			lblGameVersionCaption.Margin = new Padding(0);
			lblGameVersionCaption.Name = "lblGameVersionCaption";
			lblGameVersionCaption.Size = new Size(132, 27);
			lblGameVersionCaption.TabIndex = 4;
			lblGameVersionCaption.Text = "Game Version";
			lblGameVersionCaption.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblGameVersion
			// 
			lblGameVersion.AutoEllipsis = true;
			lblGameVersion.BackColor = Color.FromArgb(17, 27, 45);
			lblGameVersion.Dock = DockStyle.Fill;
			lblGameVersion.Font = new Font("Segoe UI", 9.25F);
			lblGameVersion.ForeColor = Color.FromArgb(245, 247, 251);
			lblGameVersion.Location = new Point(132, 54);
			lblGameVersion.Margin = new Padding(0);
			lblGameVersion.Name = "lblGameVersion";
			lblGameVersion.Size = new Size(338, 27);
			lblGameVersion.TabIndex = 5;
			lblGameVersion.Text = "N/A";
			lblGameVersion.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblMapCaption
			// 
			lblMapCaption.AutoEllipsis = true;
			lblMapCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblMapCaption.Dock = DockStyle.Fill;
			lblMapCaption.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
			lblMapCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblMapCaption.Location = new Point(0, 81);
			lblMapCaption.Margin = new Padding(0);
			lblMapCaption.Name = "lblMapCaption";
			lblMapCaption.Size = new Size(132, 27);
			lblMapCaption.TabIndex = 6;
			lblMapCaption.Text = "Map / World";
			lblMapCaption.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblMapText
			// 
			lblMapText.AutoEllipsis = true;
			lblMapText.BackColor = Color.FromArgb(17, 27, 45);
			lblMapText.Dock = DockStyle.Fill;
			lblMapText.Font = new Font("Segoe UI", 9.25F);
			lblMapText.ForeColor = Color.FromArgb(245, 247, 251);
			lblMapText.Location = new Point(132, 81);
			lblMapText.Margin = new Padding(0);
			lblMapText.Name = "lblMapText";
			lblMapText.Size = new Size(338, 27);
			lblMapText.TabIndex = 7;
			lblMapText.Text = "Main World";
			lblMapText.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblSeedCaption
			// 
			lblSeedCaption.AutoEllipsis = true;
			lblSeedCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblSeedCaption.Dock = DockStyle.Fill;
			lblSeedCaption.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
			lblSeedCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblSeedCaption.Location = new Point(0, 108);
			lblSeedCaption.Margin = new Padding(0);
			lblSeedCaption.Name = "lblSeedCaption";
			lblSeedCaption.Size = new Size(132, 27);
			lblSeedCaption.TabIndex = 8;
			lblSeedCaption.Text = "World Seed";
			lblSeedCaption.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblSeedText
			// 
			lblSeedText.AutoEllipsis = true;
			lblSeedText.BackColor = Color.FromArgb(17, 27, 45);
			lblSeedText.Dock = DockStyle.Fill;
			lblSeedText.Font = new Font("Segoe UI", 9.25F);
			lblSeedText.ForeColor = Color.FromArgb(245, 247, 251);
			lblSeedText.Location = new Point(132, 108);
			lblSeedText.Margin = new Padding(0);
			lblSeedText.Name = "lblSeedText";
			lblSeedText.Size = new Size(338, 27);
			lblSeedText.TabIndex = 9;
			lblSeedText.Text = "12345";
			lblSeedText.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblGameModeCaption
			// 
			lblGameModeCaption.AutoEllipsis = true;
			lblGameModeCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblGameModeCaption.Dock = DockStyle.Fill;
			lblGameModeCaption.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
			lblGameModeCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblGameModeCaption.Location = new Point(0, 135);
			lblGameModeCaption.Margin = new Padding(0);
			lblGameModeCaption.Name = "lblGameModeCaption";
			lblGameModeCaption.Size = new Size(132, 27);
			lblGameModeCaption.TabIndex = 10;
			lblGameModeCaption.Text = "Game Mode";
			lblGameModeCaption.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblCompetitiveText
			// 
			lblCompetitiveText.AutoEllipsis = true;
			lblCompetitiveText.BackColor = Color.FromArgb(17, 27, 45);
			lblCompetitiveText.Dock = DockStyle.Fill;
			lblCompetitiveText.Font = new Font("Segoe UI", 9.25F);
			lblCompetitiveText.ForeColor = Color.FromArgb(245, 247, 251);
			lblCompetitiveText.Location = new Point(132, 135);
			lblCompetitiveText.Margin = new Padding(0);
			lblCompetitiveText.Name = "lblCompetitiveText";
			lblCompetitiveText.Size = new Size(338, 27);
			lblCompetitiveText.TabIndex = 11;
			lblCompetitiveText.Text = "PVE";
			lblCompetitiveText.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblMaxPlayersCaption
			// 
			lblMaxPlayersCaption.AutoEllipsis = true;
			lblMaxPlayersCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblMaxPlayersCaption.Dock = DockStyle.Fill;
			lblMaxPlayersCaption.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
			lblMaxPlayersCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblMaxPlayersCaption.Location = new Point(0, 162);
			lblMaxPlayersCaption.Margin = new Padding(0);
			lblMaxPlayersCaption.Name = "lblMaxPlayersCaption";
			lblMaxPlayersCaption.Size = new Size(132, 27);
			lblMaxPlayersCaption.TabIndex = 12;
			lblMaxPlayersCaption.Text = "Max Players";
			lblMaxPlayersCaption.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblMaxPlayersText
			// 
			lblMaxPlayersText.AutoEllipsis = true;
			lblMaxPlayersText.BackColor = Color.FromArgb(17, 27, 45);
			lblMaxPlayersText.Dock = DockStyle.Fill;
			lblMaxPlayersText.Font = new Font("Segoe UI", 9.25F);
			lblMaxPlayersText.ForeColor = Color.FromArgb(245, 247, 251);
			lblMaxPlayersText.Location = new Point(132, 162);
			lblMaxPlayersText.Margin = new Padding(0);
			lblMaxPlayersText.Name = "lblMaxPlayersText";
			lblMaxPlayersText.Size = new Size(338, 27);
			lblMaxPlayersText.TabIndex = 13;
			lblMaxPlayersText.Text = "10";
			lblMaxPlayersText.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblGamePortCaption
			// 
			lblGamePortCaption.AutoEllipsis = true;
			lblGamePortCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblGamePortCaption.Dock = DockStyle.Fill;
			lblGamePortCaption.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
			lblGamePortCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblGamePortCaption.Location = new Point(0, 189);
			lblGamePortCaption.Margin = new Padding(0);
			lblGamePortCaption.Name = "lblGamePortCaption";
			lblGamePortCaption.Size = new Size(132, 27);
			lblGamePortCaption.TabIndex = 14;
			lblGamePortCaption.Text = "Game Port";
			lblGamePortCaption.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblGamePortText
			// 
			lblGamePortText.AutoEllipsis = true;
			lblGamePortText.BackColor = Color.FromArgb(17, 27, 45);
			lblGamePortText.Dock = DockStyle.Fill;
			lblGamePortText.Font = new Font("Segoe UI", 9.25F);
			lblGamePortText.ForeColor = Color.FromArgb(245, 247, 251);
			lblGamePortText.Location = new Point(132, 189);
			lblGamePortText.Margin = new Padding(0);
			lblGamePortText.Name = "lblGamePortText";
			lblGamePortText.Size = new Size(338, 27);
			lblGamePortText.TabIndex = 15;
			lblGamePortText.Text = "7777";
			lblGamePortText.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblQueryPortCaption
			// 
			lblQueryPortCaption.AutoEllipsis = true;
			lblQueryPortCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblQueryPortCaption.Dock = DockStyle.Fill;
			lblQueryPortCaption.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
			lblQueryPortCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblQueryPortCaption.Location = new Point(0, 216);
			lblQueryPortCaption.Margin = new Padding(0);
			lblQueryPortCaption.Name = "lblQueryPortCaption";
			lblQueryPortCaption.Size = new Size(132, 27);
			lblQueryPortCaption.TabIndex = 16;
			lblQueryPortCaption.Text = "Query Port";
			lblQueryPortCaption.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblQueryPortText
			// 
			lblQueryPortText.AutoEllipsis = true;
			lblQueryPortText.BackColor = Color.FromArgb(17, 27, 45);
			lblQueryPortText.Dock = DockStyle.Fill;
			lblQueryPortText.Font = new Font("Segoe UI", 9.25F);
			lblQueryPortText.ForeColor = Color.FromArgb(245, 247, 251);
			lblQueryPortText.Location = new Point(132, 216);
			lblQueryPortText.Margin = new Padding(0);
			lblQueryPortText.Name = "lblQueryPortText";
			lblQueryPortText.Size = new Size(338, 27);
			lblQueryPortText.TabIndex = 17;
			lblQueryPortText.Text = "27015";
			lblQueryPortText.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblAppPortCaption
			// 
			lblAppPortCaption.AutoEllipsis = true;
			lblAppPortCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblAppPortCaption.Dock = DockStyle.Fill;
			lblAppPortCaption.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
			lblAppPortCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblAppPortCaption.Location = new Point(0, 243);
			lblAppPortCaption.Margin = new Padding(0);
			lblAppPortCaption.Name = "lblAppPortCaption";
			lblAppPortCaption.Size = new Size(132, 29);
			lblAppPortCaption.TabIndex = 18;
			lblAppPortCaption.Text = "App Port";
			lblAppPortCaption.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblAppPortText
			// 
			lblAppPortText.AutoEllipsis = true;
			lblAppPortText.BackColor = Color.FromArgb(17, 27, 45);
			lblAppPortText.Dock = DockStyle.Fill;
			lblAppPortText.Font = new Font("Segoe UI", 9.25F);
			lblAppPortText.ForeColor = Color.FromArgb(245, 247, 251);
			lblAppPortText.Location = new Point(132, 243);
			lblAppPortText.Margin = new Padding(0);
			lblAppPortText.Name = "lblAppPortText";
			lblAppPortText.Size = new Size(338, 29);
			lblAppPortText.TabIndex = 19;
			lblAppPortText.Text = "N/A";
			lblAppPortText.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// pnlConfigurationCard
			// 
			pnlConfigurationCard.BackColor = Color.FromArgb(17, 27, 45);
			pnlConfigurationCard.BorderColor = Color.FromArgb(38, 52, 77);
			pnlConfigurationCard.Controls.Add(lblConfigurationTitle);
			pnlConfigurationCard.Controls.Add(lblConfigurationSubtitle);
			pnlConfigurationCard.Controls.Add(configurationTable);
			pnlConfigurationCard.CornerRadius = 14;
			pnlConfigurationCard.Dock = DockStyle.Fill;
			pnlConfigurationCard.FillColor = Color.FromArgb(17, 27, 45);
			pnlConfigurationCard.Location = new Point(530, 0);
			pnlConfigurationCard.Margin = new Padding(8, 0, 0, 0);
			pnlConfigurationCard.Name = "pnlConfigurationCard";
			pnlConfigurationCard.Size = new Size(515, 375);
			pnlConfigurationCard.TabIndex = 1;
			// 
			// lblConfigurationTitle
			// 
			lblConfigurationTitle.AutoSize = true;
			lblConfigurationTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblConfigurationTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
			lblConfigurationTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblConfigurationTitle.Location = new Point(22, 17);
			lblConfigurationTitle.Name = "lblConfigurationTitle";
			lblConfigurationTitle.Size = new Size(232, 28);
			lblConfigurationTitle.TabIndex = 0;
			lblConfigurationTitle.Text = "Configuration & Security";
			// 
			// lblConfigurationSubtitle
			// 
			lblConfigurationSubtitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblConfigurationSubtitle.AutoEllipsis = true;
			lblConfigurationSubtitle.BackColor = Color.FromArgb(17, 27, 45);
			lblConfigurationSubtitle.Font = new Font("Segoe UI", 9F);
			lblConfigurationSubtitle.ForeColor = Color.FromArgb(158, 172, 194);
			lblConfigurationSubtitle.Location = new Point(24, 49);
			lblConfigurationSubtitle.Name = "lblConfigurationSubtitle";
			lblConfigurationSubtitle.Size = new Size(467, 20);
			lblConfigurationSubtitle.TabIndex = 1;
			lblConfigurationSubtitle.Text = "Access controls, startup behavior, and integrations";
			// 
			// configurationTable
			// 
			configurationTable.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			configurationTable.BackColor = Color.FromArgb(17, 27, 45);
			configurationTable.ColumnCount = 2;
			configurationTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
			configurationTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			configurationTable.Controls.Add(lblServerPasswordCaption, 0, 0);
			configurationTable.Controls.Add(lblServerPasswordText, 1, 0);
			configurationTable.Controls.Add(lblAdminPasswordCaption, 0, 1);
			configurationTable.Controls.Add(lblServerAdminPasswordText, 1, 1);
			configurationTable.Controls.Add(lblRconCaption, 0, 2);
			configurationTable.Controls.Add(lblRconActiveText, 1, 2);
			configurationTable.Controls.Add(lblRconPortCaption, 0, 3);
			configurationTable.Controls.Add(lblRconPortText, 1, 3);
			configurationTable.Controls.Add(lblRconPasswordCaption, 0, 4);
			configurationTable.Controls.Add(lblRconPasswordText, 1, 4);
			configurationTable.Controls.Add(lblBackupOnStartCaption, 0, 5);
			configurationTable.Controls.Add(lblBackupOnStartText, 1, 5);
			configurationTable.Controls.Add(lblUpdateOnStartCaption, 0, 6);
			configurationTable.Controls.Add(lbllUpdateOnStartText, 1, 6);
			configurationTable.Controls.Add(lblDiscordAlertsCaption, 0, 7);
			configurationTable.Controls.Add(lblDiscordActivateText, 1, 7);
			configurationTable.Controls.Add(lblAutoRestartCaption, 0, 8);
			configurationTable.Controls.Add(lblAutoRestartText, 1, 8);
			configurationTable.Location = new Point(22, 78);
			configurationTable.Margin = new Padding(0);
			configurationTable.Name = "configurationTable";
			configurationTable.RowCount = 9;
			configurationTable.RowStyles.Add(new RowStyle(SizeType.Percent, 11.11111F));
			configurationTable.RowStyles.Add(new RowStyle(SizeType.Percent, 11.11111F));
			configurationTable.RowStyles.Add(new RowStyle(SizeType.Percent, 11.11111F));
			configurationTable.RowStyles.Add(new RowStyle(SizeType.Percent, 11.11111F));
			configurationTable.RowStyles.Add(new RowStyle(SizeType.Percent, 11.11111F));
			configurationTable.RowStyles.Add(new RowStyle(SizeType.Percent, 11.11111F));
			configurationTable.RowStyles.Add(new RowStyle(SizeType.Percent, 11.11111F));
			configurationTable.RowStyles.Add(new RowStyle(SizeType.Percent, 11.11111F));
			configurationTable.RowStyles.Add(new RowStyle(SizeType.Percent, 11.11112F));
			configurationTable.Size = new Size(471, 272);
			configurationTable.TabIndex = 2;
			// 
			// lblServerPasswordCaption
			// 
			lblServerPasswordCaption.AutoEllipsis = true;
			lblServerPasswordCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblServerPasswordCaption.Dock = DockStyle.Fill;
			lblServerPasswordCaption.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
			lblServerPasswordCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblServerPasswordCaption.Location = new Point(0, 0);
			lblServerPasswordCaption.Margin = new Padding(0);
			lblServerPasswordCaption.Name = "lblServerPasswordCaption";
			lblServerPasswordCaption.Size = new Size(150, 30);
			lblServerPasswordCaption.TabIndex = 0;
			lblServerPasswordCaption.Text = "Server Password";
			lblServerPasswordCaption.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblServerPasswordText
			// 
			lblServerPasswordText.AutoEllipsis = true;
			lblServerPasswordText.BackColor = Color.FromArgb(17, 27, 45);
			lblServerPasswordText.Dock = DockStyle.Fill;
			lblServerPasswordText.Font = new Font("Segoe UI", 9.25F);
			lblServerPasswordText.ForeColor = Color.FromArgb(245, 247, 251);
			lblServerPasswordText.Location = new Point(150, 0);
			lblServerPasswordText.Margin = new Padding(0);
			lblServerPasswordText.Name = "lblServerPasswordText";
			lblServerPasswordText.Size = new Size(321, 30);
			lblServerPasswordText.TabIndex = 1;
			lblServerPasswordText.Text = "Not Required";
			lblServerPasswordText.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblAdminPasswordCaption
			// 
			lblAdminPasswordCaption.AutoEllipsis = true;
			lblAdminPasswordCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblAdminPasswordCaption.Dock = DockStyle.Fill;
			lblAdminPasswordCaption.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
			lblAdminPasswordCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblAdminPasswordCaption.Location = new Point(0, 30);
			lblAdminPasswordCaption.Margin = new Padding(0);
			lblAdminPasswordCaption.Name = "lblAdminPasswordCaption";
			lblAdminPasswordCaption.Size = new Size(150, 30);
			lblAdminPasswordCaption.TabIndex = 2;
			lblAdminPasswordCaption.Text = "Admin Password";
			lblAdminPasswordCaption.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblServerAdminPasswordText
			// 
			lblServerAdminPasswordText.AutoEllipsis = true;
			lblServerAdminPasswordText.BackColor = Color.FromArgb(17, 27, 45);
			lblServerAdminPasswordText.Dock = DockStyle.Fill;
			lblServerAdminPasswordText.Font = new Font("Segoe UI", 9.25F);
			lblServerAdminPasswordText.ForeColor = Color.FromArgb(245, 247, 251);
			lblServerAdminPasswordText.Location = new Point(150, 30);
			lblServerAdminPasswordText.Margin = new Padding(0);
			lblServerAdminPasswordText.Name = "lblServerAdminPasswordText";
			lblServerAdminPasswordText.Size = new Size(321, 30);
			lblServerAdminPasswordText.TabIndex = 3;
			lblServerAdminPasswordText.Text = "Not Required";
			lblServerAdminPasswordText.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblRconCaption
			// 
			lblRconCaption.AutoEllipsis = true;
			lblRconCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblRconCaption.Dock = DockStyle.Fill;
			lblRconCaption.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
			lblRconCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblRconCaption.Location = new Point(0, 60);
			lblRconCaption.Margin = new Padding(0);
			lblRconCaption.Name = "lblRconCaption";
			lblRconCaption.Size = new Size(150, 30);
			lblRconCaption.TabIndex = 4;
			lblRconCaption.Text = "RCON";
			lblRconCaption.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblRconActiveText
			// 
			lblRconActiveText.AutoEllipsis = true;
			lblRconActiveText.BackColor = Color.FromArgb(17, 27, 45);
			lblRconActiveText.Dock = DockStyle.Fill;
			lblRconActiveText.Font = new Font("Segoe UI", 9.25F);
			lblRconActiveText.ForeColor = Color.FromArgb(245, 247, 251);
			lblRconActiveText.Location = new Point(150, 60);
			lblRconActiveText.Margin = new Padding(0);
			lblRconActiveText.Name = "lblRconActiveText";
			lblRconActiveText.Size = new Size(321, 30);
			lblRconActiveText.TabIndex = 5;
			lblRconActiveText.Text = "Off";
			lblRconActiveText.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblRconPortCaption
			// 
			lblRconPortCaption.AutoEllipsis = true;
			lblRconPortCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblRconPortCaption.Dock = DockStyle.Fill;
			lblRconPortCaption.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
			lblRconPortCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblRconPortCaption.Location = new Point(0, 90);
			lblRconPortCaption.Margin = new Padding(0);
			lblRconPortCaption.Name = "lblRconPortCaption";
			lblRconPortCaption.Size = new Size(150, 30);
			lblRconPortCaption.TabIndex = 6;
			lblRconPortCaption.Text = "RCON Port";
			lblRconPortCaption.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblRconPortText
			// 
			lblRconPortText.AutoEllipsis = true;
			lblRconPortText.BackColor = Color.FromArgb(17, 27, 45);
			lblRconPortText.Dock = DockStyle.Fill;
			lblRconPortText.Font = new Font("Segoe UI", 9.25F);
			lblRconPortText.ForeColor = Color.FromArgb(245, 247, 251);
			lblRconPortText.Location = new Point(150, 90);
			lblRconPortText.Margin = new Padding(0);
			lblRconPortText.Name = "lblRconPortText";
			lblRconPortText.Size = new Size(321, 30);
			lblRconPortText.TabIndex = 7;
			lblRconPortText.Text = "27016";
			lblRconPortText.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblRconPasswordCaption
			// 
			lblRconPasswordCaption.AutoEllipsis = true;
			lblRconPasswordCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblRconPasswordCaption.Dock = DockStyle.Fill;
			lblRconPasswordCaption.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
			lblRconPasswordCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblRconPasswordCaption.Location = new Point(0, 120);
			lblRconPasswordCaption.Margin = new Padding(0);
			lblRconPasswordCaption.Name = "lblRconPasswordCaption";
			lblRconPasswordCaption.Size = new Size(150, 30);
			lblRconPasswordCaption.TabIndex = 8;
			lblRconPasswordCaption.Text = "RCON Password";
			lblRconPasswordCaption.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblRconPasswordText
			// 
			lblRconPasswordText.AutoEllipsis = true;
			lblRconPasswordText.BackColor = Color.FromArgb(17, 27, 45);
			lblRconPasswordText.Dock = DockStyle.Fill;
			lblRconPasswordText.Font = new Font("Segoe UI", 9.25F);
			lblRconPasswordText.ForeColor = Color.FromArgb(245, 247, 251);
			lblRconPasswordText.Location = new Point(150, 120);
			lblRconPasswordText.Margin = new Padding(0);
			lblRconPasswordText.Name = "lblRconPasswordText";
			lblRconPasswordText.Size = new Size(321, 30);
			lblRconPasswordText.TabIndex = 9;
			lblRconPasswordText.Text = "Not Required";
			lblRconPasswordText.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblBackupOnStartCaption
			// 
			lblBackupOnStartCaption.AutoEllipsis = true;
			lblBackupOnStartCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblBackupOnStartCaption.Dock = DockStyle.Fill;
			lblBackupOnStartCaption.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
			lblBackupOnStartCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblBackupOnStartCaption.Location = new Point(0, 150);
			lblBackupOnStartCaption.Margin = new Padding(0);
			lblBackupOnStartCaption.Name = "lblBackupOnStartCaption";
			lblBackupOnStartCaption.Size = new Size(150, 30);
			lblBackupOnStartCaption.TabIndex = 10;
			lblBackupOnStartCaption.Text = "Backup on Start";
			lblBackupOnStartCaption.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblBackupOnStartText
			// 
			lblBackupOnStartText.AutoEllipsis = true;
			lblBackupOnStartText.BackColor = Color.FromArgb(17, 27, 45);
			lblBackupOnStartText.Dock = DockStyle.Fill;
			lblBackupOnStartText.Font = new Font("Segoe UI", 9.25F);
			lblBackupOnStartText.ForeColor = Color.FromArgb(245, 247, 251);
			lblBackupOnStartText.Location = new Point(150, 150);
			lblBackupOnStartText.Margin = new Padding(0);
			lblBackupOnStartText.Name = "lblBackupOnStartText";
			lblBackupOnStartText.Size = new Size(321, 30);
			lblBackupOnStartText.TabIndex = 11;
			lblBackupOnStartText.Text = "Off";
			lblBackupOnStartText.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblUpdateOnStartCaption
			// 
			lblUpdateOnStartCaption.AutoEllipsis = true;
			lblUpdateOnStartCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblUpdateOnStartCaption.Dock = DockStyle.Fill;
			lblUpdateOnStartCaption.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
			lblUpdateOnStartCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblUpdateOnStartCaption.Location = new Point(0, 180);
			lblUpdateOnStartCaption.Margin = new Padding(0);
			lblUpdateOnStartCaption.Name = "lblUpdateOnStartCaption";
			lblUpdateOnStartCaption.Size = new Size(150, 30);
			lblUpdateOnStartCaption.TabIndex = 12;
			lblUpdateOnStartCaption.Text = "Update on Start";
			lblUpdateOnStartCaption.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lbllUpdateOnStartText
			// 
			lbllUpdateOnStartText.AutoEllipsis = true;
			lbllUpdateOnStartText.BackColor = Color.FromArgb(17, 27, 45);
			lbllUpdateOnStartText.Dock = DockStyle.Fill;
			lbllUpdateOnStartText.Font = new Font("Segoe UI", 9.25F);
			lbllUpdateOnStartText.ForeColor = Color.FromArgb(245, 247, 251);
			lbllUpdateOnStartText.Location = new Point(150, 180);
			lbllUpdateOnStartText.Margin = new Padding(0);
			lbllUpdateOnStartText.Name = "lbllUpdateOnStartText";
			lbllUpdateOnStartText.Size = new Size(321, 30);
			lbllUpdateOnStartText.TabIndex = 13;
			lbllUpdateOnStartText.Text = "Off";
			lbllUpdateOnStartText.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblDiscordAlertsCaption
			// 
			lblDiscordAlertsCaption.AutoEllipsis = true;
			lblDiscordAlertsCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblDiscordAlertsCaption.Dock = DockStyle.Fill;
			lblDiscordAlertsCaption.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
			lblDiscordAlertsCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblDiscordAlertsCaption.Location = new Point(0, 210);
			lblDiscordAlertsCaption.Margin = new Padding(0);
			lblDiscordAlertsCaption.Name = "lblDiscordAlertsCaption";
			lblDiscordAlertsCaption.Size = new Size(150, 30);
			lblDiscordAlertsCaption.TabIndex = 14;
			lblDiscordAlertsCaption.Text = "Discord Alerts";
			lblDiscordAlertsCaption.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblDiscordActivateText
			// 
			lblDiscordActivateText.AutoEllipsis = true;
			lblDiscordActivateText.BackColor = Color.FromArgb(17, 27, 45);
			lblDiscordActivateText.Dock = DockStyle.Fill;
			lblDiscordActivateText.Font = new Font("Segoe UI", 9.25F);
			lblDiscordActivateText.ForeColor = Color.FromArgb(245, 247, 251);
			lblDiscordActivateText.Location = new Point(150, 210);
			lblDiscordActivateText.Margin = new Padding(0);
			lblDiscordActivateText.Name = "lblDiscordActivateText";
			lblDiscordActivateText.Size = new Size(321, 30);
			lblDiscordActivateText.TabIndex = 15;
			lblDiscordActivateText.Text = "Off";
			lblDiscordActivateText.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblAutoRestartCaption
			// 
			lblAutoRestartCaption.AutoEllipsis = true;
			lblAutoRestartCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblAutoRestartCaption.Dock = DockStyle.Fill;
			lblAutoRestartCaption.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
			lblAutoRestartCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblAutoRestartCaption.Location = new Point(0, 240);
			lblAutoRestartCaption.Margin = new Padding(0);
			lblAutoRestartCaption.Name = "lblAutoRestartCaption";
			lblAutoRestartCaption.Size = new Size(150, 32);
			lblAutoRestartCaption.TabIndex = 16;
			lblAutoRestartCaption.Text = "Auto Restart";
			lblAutoRestartCaption.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblAutoRestartText
			// 
			lblAutoRestartText.AutoEllipsis = true;
			lblAutoRestartText.BackColor = Color.FromArgb(17, 27, 45);
			lblAutoRestartText.Dock = DockStyle.Fill;
			lblAutoRestartText.Font = new Font("Segoe UI", 9.25F);
			lblAutoRestartText.ForeColor = Color.FromArgb(245, 247, 251);
			lblAutoRestartText.Location = new Point(150, 240);
			lblAutoRestartText.Margin = new Padding(0);
			lblAutoRestartText.Name = "lblAutoRestartText";
			lblAutoRestartText.Size = new Size(321, 32);
			lblAutoRestartText.TabIndex = 17;
			lblAutoRestartText.Text = "No Days Scheduled";
			lblAutoRestartText.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// pnlPathsCard
			// 
			pnlPathsCard.BackColor = Color.FromArgb(17, 27, 45);
			pnlPathsCard.BorderColor = Color.FromArgb(38, 52, 77);
			pnlPathsCard.Controls.Add(lblPathsTitle);
			pnlPathsCard.Controls.Add(lblPathsSubtitle);
			pnlPathsCard.Controls.Add(pathsTable);
			pnlPathsCard.CornerRadius = 14;
			pnlPathsCard.Dock = DockStyle.Fill;
			pnlPathsCard.FillColor = Color.FromArgb(17, 27, 45);
			pnlPathsCard.Location = new Point(0, 619);
			pnlPathsCard.Margin = new Padding(0);
			pnlPathsCard.Name = "pnlPathsCard";
			pnlPathsCard.Size = new Size(1045, 205);
			pnlPathsCard.TabIndex = 3;
			// 
			// lblPathsTitle
			// 
			lblPathsTitle.AutoSize = true;
			lblPathsTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblPathsTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
			lblPathsTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblPathsTitle.Location = new Point(22, 17);
			lblPathsTitle.Name = "lblPathsTitle";
			lblPathsTitle.Size = new Size(215, 28);
			lblPathsTitle.TabIndex = 0;
			lblPathsTitle.Text = "Paths & Launch Details";
			// 
			// lblPathsSubtitle
			// 
			lblPathsSubtitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblPathsSubtitle.AutoEllipsis = true;
			lblPathsSubtitle.BackColor = Color.FromArgb(17, 27, 45);
			lblPathsSubtitle.Font = new Font("Segoe UI", 9F);
			lblPathsSubtitle.ForeColor = Color.FromArgb(158, 172, 194);
			lblPathsSubtitle.Location = new Point(24, 49);
			lblPathsSubtitle.Name = "lblPathsSubtitle";
			lblPathsSubtitle.Size = new Size(997, 20);
			lblPathsSubtitle.TabIndex = 1;
			lblPathsSubtitle.Text = "Read-only values can be selected and copied for diagnostics";
			// 
			// pathsTable
			// 
			pathsTable.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			pathsTable.BackColor = Color.FromArgb(17, 27, 45);
			pathsTable.ColumnCount = 2;
			pathsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 142F));
			pathsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			pathsTable.Controls.Add(lblServerFolderCaption, 0, 0);
			pathsTable.Controls.Add(txtServerFolderValue, 1, 0);
			pathsTable.Controls.Add(lblDiscordWebhookCaption, 0, 1);
			pathsTable.Controls.Add(txtDiscordWebhookValue, 1, 1);
			pathsTable.Controls.Add(lblExtraArgsCaption, 0, 2);
			pathsTable.Controls.Add(txtExtraArgsValue, 1, 2);
			pathsTable.Location = new Point(22, 78);
			pathsTable.Margin = new Padding(0);
			pathsTable.Name = "pathsTable";
			pathsTable.RowCount = 3;
			pathsTable.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
			pathsTable.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
			pathsTable.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33334F));
			pathsTable.Size = new Size(1001, 105);
			pathsTable.TabIndex = 2;
			// 
			// lblServerFolderCaption
			// 
			lblServerFolderCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblServerFolderCaption.Dock = DockStyle.Fill;
			lblServerFolderCaption.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
			lblServerFolderCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblServerFolderCaption.Location = new Point(0, 0);
			lblServerFolderCaption.Margin = new Padding(0);
			lblServerFolderCaption.Name = "lblServerFolderCaption";
			lblServerFolderCaption.Size = new Size(142, 34);
			lblServerFolderCaption.TabIndex = 0;
			lblServerFolderCaption.Text = "Server Folder";
			lblServerFolderCaption.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// txtServerFolderValue
			// 
			txtServerFolderValue.BackColor = Color.FromArgb(12, 21, 36);
			txtServerFolderValue.BorderStyle = BorderStyle.FixedSingle;
			txtServerFolderValue.Dock = DockStyle.Fill;
			txtServerFolderValue.Font = new Font("Segoe UI", 9.25F);
			txtServerFolderValue.ForeColor = Color.FromArgb(245, 247, 251);
			txtServerFolderValue.Location = new Point(142, 3);
			txtServerFolderValue.Margin = new Padding(0, 3, 0, 5);
			txtServerFolderValue.Name = "txtServerFolderValue";
			txtServerFolderValue.ReadOnly = true;
			txtServerFolderValue.Size = new Size(859, 24);
			txtServerFolderValue.TabIndex = 1;
			txtServerFolderValue.TabStop = false;
			txtServerFolderValue.Text = "C:\\Synix\\Games\\Example Server";
			// 
			// lblDiscordWebhookCaption
			// 
			lblDiscordWebhookCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblDiscordWebhookCaption.Dock = DockStyle.Fill;
			lblDiscordWebhookCaption.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
			lblDiscordWebhookCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblDiscordWebhookCaption.Location = new Point(0, 34);
			lblDiscordWebhookCaption.Margin = new Padding(0);
			lblDiscordWebhookCaption.Name = "lblDiscordWebhookCaption";
			lblDiscordWebhookCaption.Size = new Size(142, 34);
			lblDiscordWebhookCaption.TabIndex = 2;
			lblDiscordWebhookCaption.Text = "Discord Webhook";
			lblDiscordWebhookCaption.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// txtDiscordWebhookValue
			// 
			txtDiscordWebhookValue.BackColor = Color.FromArgb(12, 21, 36);
			txtDiscordWebhookValue.BorderStyle = BorderStyle.FixedSingle;
			txtDiscordWebhookValue.Dock = DockStyle.Fill;
			txtDiscordWebhookValue.Font = new Font("Segoe UI", 9.25F);
			txtDiscordWebhookValue.ForeColor = Color.FromArgb(245, 247, 251);
			txtDiscordWebhookValue.Location = new Point(142, 37);
			txtDiscordWebhookValue.Margin = new Padding(0, 3, 0, 5);
			txtDiscordWebhookValue.Name = "txtDiscordWebhookValue";
			txtDiscordWebhookValue.ReadOnly = true;
			txtDiscordWebhookValue.Size = new Size(859, 24);
			txtDiscordWebhookValue.TabIndex = 3;
			txtDiscordWebhookValue.TabStop = false;
			txtDiscordWebhookValue.Text = "Not Configured";
			// 
			// lblExtraArgsCaption
			// 
			lblExtraArgsCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblExtraArgsCaption.Dock = DockStyle.Fill;
			lblExtraArgsCaption.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
			lblExtraArgsCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblExtraArgsCaption.Location = new Point(0, 68);
			lblExtraArgsCaption.Margin = new Padding(0);
			lblExtraArgsCaption.Name = "lblExtraArgsCaption";
			lblExtraArgsCaption.Size = new Size(142, 37);
			lblExtraArgsCaption.TabIndex = 4;
			lblExtraArgsCaption.Text = "Extra Arguments";
			lblExtraArgsCaption.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// txtExtraArgsValue
			// 
			txtExtraArgsValue.BackColor = Color.FromArgb(12, 21, 36);
			txtExtraArgsValue.BorderStyle = BorderStyle.FixedSingle;
			txtExtraArgsValue.Dock = DockStyle.Fill;
			txtExtraArgsValue.Font = new Font("Segoe UI", 9.25F);
			txtExtraArgsValue.ForeColor = Color.FromArgb(245, 247, 251);
			txtExtraArgsValue.Location = new Point(142, 71);
			txtExtraArgsValue.Margin = new Padding(0, 3, 0, 5);
			txtExtraArgsValue.Name = "txtExtraArgsValue";
			txtExtraArgsValue.ReadOnly = true;
			txtExtraArgsValue.Size = new Size(859, 24);
			txtExtraArgsValue.TabIndex = 5;
			txtExtraArgsValue.TabStop = false;
			txtExtraArgsValue.Text = "No extra arguments";
			// 
			// ServerInfo
			// 
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			BackColor = Color.FromArgb(38, 52, 77);
			ClientSize = new Size(1120, 760);
			Controls.Add(shellLayout);
			Font = new Font("Segoe UI", 9F);
			ForeColor = Color.FromArgb(245, 247, 251);
			FormBorderStyle = FormBorderStyle.None;
			Icon = (Icon)resources.GetObject("$this.Icon");
			KeyPreview = true;
			MinimumSize = new Size(960, 680);
			Name = "ServerInfo";
			Padding = new Padding(1);
			StartPosition = FormStartPosition.CenterParent;
			Text = "Server Info";
			FormClosing += ServerInfo_FormClosing;
			shellLayout.ResumeLayout(false);
			titleBar.ResumeLayout(false);
			titleBar.PerformLayout();
			((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
			contentScroll.ResumeLayout(false);
			contentLayout.ResumeLayout(false);
			headerPanel.ResumeLayout(false);
			metricsLayout.ResumeLayout(false);
			pnlCpuCard.ResumeLayout(false);
			pnlCpuCard.PerformLayout();
			pnlCpuTrack.ResumeLayout(false);
			pnlRamCard.ResumeLayout(false);
			pnlRamCard.PerformLayout();
			pnlRamTrack.ResumeLayout(false);
			pnlStatusCard.ResumeLayout(false);
			pnlStatusCard.PerformLayout();
			detailsLayout.ResumeLayout(false);
			pnlDetailsCard.ResumeLayout(false);
			pnlDetailsCard.PerformLayout();
			detailsTable.ResumeLayout(false);
			pnlConfigurationCard.ResumeLayout(false);
			pnlConfigurationCard.PerformLayout();
			configurationTable.ResumeLayout(false);
			pnlPathsCard.ResumeLayout(false);
			pnlPathsCard.PerformLayout();
			pathsTable.ResumeLayout(false);
			pathsTable.PerformLayout();
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
		private Panel contentScroll;
		private TableLayoutPanel contentLayout;
		private Panel headerPanel;
		private Label lblPageHeading;
		private Label lblPageSubtitle;
		private Label lblLiveIndicator;
		private TableLayoutPanel metricsLayout;
		private SynixApp.Design.ModernSettingsCard pnlCpuCard;
		private Label lblCpuTitle;
		private Label lblCpuCardValue;
		private Label lblCpuCaption;
		private Panel pnlCpuTrack;
		private Panel pnlCpuFill;
		private SynixApp.Design.ModernSettingsCard pnlRamCard;
		private Label lblRamTitle;
		private Label lblRamCardValue;
		private Label lblRamCaption;
		private Panel pnlRamTrack;
		private Panel pnlRamFill;
		private SynixApp.Design.ModernSettingsCard pnlStatusCard;
		private Panel pnlStatusIndicator;
		private Label lblStatusTitle;
		private Label lblStatusCardValue;
		private Label lblStatusCaption;
		private Label lblProcessIdCaption;
		private Label lblProcessIdValue;
		private TableLayoutPanel detailsLayout;
		private SynixApp.Design.ModernSettingsCard pnlDetailsCard;
		private Label lblDetailsTitle;
		private Label lblDetailsSubtitle;
		private TableLayoutPanel detailsTable;
		private Label lblServerNameCaption;
		private Label lblServerNameText;
		private Label lblGameServerCaption;
		private Label lblGameServerText;
		private Label lblGameVersionCaption;
		private Label lblGameVersion;
		private Label lblMapCaption;
		private Label lblMapText;
		private Label lblSeedCaption;
		private Label lblSeedText;
		private Label lblGameModeCaption;
		private Label lblCompetitiveText;
		private Label lblMaxPlayersCaption;
		private Label lblMaxPlayersText;
		private Label lblGamePortCaption;
		private Label lblGamePortText;
		private Label lblQueryPortCaption;
		private Label lblQueryPortText;
		private Label lblAppPortCaption;
		private Label lblAppPortText;
		private SynixApp.Design.ModernSettingsCard pnlConfigurationCard;
		private Label lblConfigurationTitle;
		private Label lblConfigurationSubtitle;
		private TableLayoutPanel configurationTable;
		private Label lblServerPasswordCaption;
		private Label lblServerPasswordText;
		private Label lblAdminPasswordCaption;
		private Label lblServerAdminPasswordText;
		private Label lblRconCaption;
		private Label lblRconActiveText;
		private Label lblRconPortCaption;
		private Label lblRconPortText;
		private Label lblRconPasswordCaption;
		private Label lblRconPasswordText;
		private Label lblBackupOnStartCaption;
		private Label lblBackupOnStartText;
		private Label lblUpdateOnStartCaption;
		private Label lbllUpdateOnStartText;
		private Label lblDiscordAlertsCaption;
		private Label lblDiscordActivateText;
		private Label lblAutoRestartCaption;
		private Label lblAutoRestartText;
		private SynixApp.Design.ModernSettingsCard pnlPathsCard;
		private Label lblPathsTitle;
		private Label lblPathsSubtitle;
		private TableLayoutPanel pathsTable;
		private Label lblServerFolderCaption;
		private TextBox txtServerFolderValue;
		private Label lblDiscordWebhookCaption;
		private TextBox txtDiscordWebhookValue;
		private Label lblExtraArgsCaption;
		private TextBox txtExtraArgsValue;
	}
}
