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
namespace Synix_Control_Panel
{
	partial class ResourceMonitorGUI
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
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResourceMonitorGUI));
			DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
			shellLayout = new TableLayoutPanel();
			titleBar = new Panel();
			picLogo = new PictureBox();
			lblWindowTitle = new Label();
			btnMinimize = new Button();
			btnClose = new Button();
			titleBottomBorder = new Label();
			contentPanel = new Panel();
			contentLayout = new TableLayoutPanel();
			headerPanel = new Panel();
			lblPageHeading = new Label();
			lblPageSubtitle = new Label();
			lblLiveIndicator = new Label();
			metricsLayout = new TableLayoutPanel();
			pnlCpuCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			glyphCpu = new Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph();
			lblTotalCpuTitle = new Label();
			lblTotalCpuValue = new Label();
			lblTotalCpuCaption = new Label();
			pnlCpuTrack = new Panel();
			pnlCpuFill = new Panel();
			pnlRamCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			glyphRam = new Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph();
			lblTotalRamTitle = new Label();
			lblTotalRamValue = new Label();
			lblTotalRamCaption = new Label();
			pnlRamTrack = new Panel();
			pnlRamFill = new Panel();
			pnlActiveServersCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			glyphActiveServers = new Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph();
			lblActiveServersTitle = new Label();
			lblActiveServersValue = new Label();
			lblActiveServersCaption = new Label();
			lblActiveIndicator = new Label();
			pnlGridCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			lblGridTitle = new Label();
			lblGridSubtitle = new Label();
			resourceGrid = new DataGridView();
			colStatus = new DataGridViewTextBoxColumn();
			colServerName = new DataGridViewTextBoxColumn();
			colPid = new DataGridViewTextBoxColumn();
			colExecutable = new DataGridViewTextBoxColumn();
			colCpuUsage = new DataGridViewTextBoxColumn();
			colRamUsage = new DataGridViewTextBoxColumn();
			footerSeparator = new Label();
			lblServerCount = new Label();
			lblLastUpdated = new Label();
			tmrRefresh = new System.Windows.Forms.Timer(components);
			shellLayout.SuspendLayout();
			titleBar.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
			contentPanel.SuspendLayout();
			contentLayout.SuspendLayout();
			headerPanel.SuspendLayout();
			metricsLayout.SuspendLayout();
			pnlCpuCard.SuspendLayout();
			pnlCpuTrack.SuspendLayout();
			pnlRamCard.SuspendLayout();
			pnlRamTrack.SuspendLayout();
			pnlActiveServersCard.SuspendLayout();
			pnlGridCard.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)resourceGrid).BeginInit();
			SuspendLayout();
			//
			// shellLayout
			//
			shellLayout.BackColor = Color.FromArgb(8, 13, 24);
			shellLayout.ColumnCount = 1;
			shellLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			shellLayout.Controls.Add(titleBar, 0, 0);
			shellLayout.Controls.Add(contentPanel, 0, 1);
			shellLayout.Dock = DockStyle.Fill;
			shellLayout.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
			shellLayout.Location = new Point(1, 1);
			shellLayout.Margin = new Padding(0);
			shellLayout.Name = "shellLayout";
			shellLayout.RowCount = 2;
			shellLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
			shellLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			shellLayout.Size = new Size(1178, 758);
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
			titleBar.Size = new Size(1178, 56);
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
			lblWindowTitle.Size = new Size(147, 21);
			lblWindowTitle.TabIndex = 1;
			lblWindowTitle.Text = "Resource Monitor";
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
			btnMinimize.Location = new Point(1082, 0);
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
			btnClose.Location = new Point(1130, 0);
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
			titleBottomBorder.Size = new Size(1178, 1);
			titleBottomBorder.TabIndex = 4;
			//
			// contentPanel
			//
			contentPanel.BackColor = Color.FromArgb(8, 13, 24);
			contentPanel.Controls.Add(contentLayout);
			contentPanel.Dock = DockStyle.Fill;
			contentPanel.Location = new Point(0, 56);
			contentPanel.Margin = new Padding(0);
			contentPanel.Name = "contentPanel";
			contentPanel.Padding = new Padding(28, 24, 28, 28);
			contentPanel.Size = new Size(1178, 702);
			contentPanel.TabIndex = 1;
			//
			// contentLayout
			//
			contentLayout.BackColor = Color.FromArgb(8, 13, 24);
			contentLayout.ColumnCount = 1;
			contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			contentLayout.Controls.Add(headerPanel, 0, 0);
			contentLayout.Controls.Add(metricsLayout, 0, 1);
			contentLayout.Controls.Add(pnlGridCard, 0, 3);
			contentLayout.Dock = DockStyle.Fill;
			contentLayout.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
			contentLayout.Location = new Point(28, 24);
			contentLayout.Margin = new Padding(0);
			contentLayout.Name = "contentLayout";
			contentLayout.RowCount = 4;
			contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
			contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
			contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 16F));
			contentLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			contentLayout.Size = new Size(1122, 650);
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
			headerPanel.Size = new Size(1122, 80);
			headerPanel.TabIndex = 0;
			//
			// lblPageHeading
			//
			lblPageHeading.AutoSize = true;
			lblPageHeading.BackColor = Color.FromArgb(8, 13, 24);
			lblPageHeading.Font = new Font("Segoe UI", 22F, FontStyle.Bold, GraphicsUnit.Point);
			lblPageHeading.ForeColor = Color.FromArgb(245, 247, 251);
			lblPageHeading.Location = new Point(0, 0);
			lblPageHeading.Name = "lblPageHeading";
			lblPageHeading.Size = new Size(277, 41);
			lblPageHeading.TabIndex = 0;
			lblPageHeading.Text = "Resource Monitor";
			//
			// lblPageSubtitle
			//
			lblPageSubtitle.AutoSize = true;
			lblPageSubtitle.BackColor = Color.FromArgb(8, 13, 24);
			lblPageSubtitle.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
			lblPageSubtitle.ForeColor = Color.FromArgb(158, 172, 194);
			lblPageSubtitle.Location = new Point(3, 47);
			lblPageSubtitle.Name = "lblPageSubtitle";
			lblPageSubtitle.Size = new Size(389, 19);
			lblPageSubtitle.TabIndex = 1;
			lblPageSubtitle.Text = "Live performance across every managed game server process.";
			//
			// lblLiveIndicator
			//
			lblLiveIndicator.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lblLiveIndicator.BackColor = Color.FromArgb(28, 75, 91);
			lblLiveIndicator.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
			lblLiveIndicator.ForeColor = Color.FromArgb(32, 214, 199);
			lblLiveIndicator.Location = new Point(958, 8);
			lblLiveIndicator.Name = "lblLiveIndicator";
			lblLiveIndicator.Size = new Size(164, 30);
			lblLiveIndicator.TabIndex = 2;
			lblLiveIndicator.Text = "●  LIVE MONITORING";
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
			metricsLayout.Controls.Add(pnlActiveServersCard, 2, 0);
			metricsLayout.Dock = DockStyle.Fill;
			metricsLayout.Location = new Point(0, 80);
			metricsLayout.Margin = new Padding(0);
			metricsLayout.Name = "metricsLayout";
			metricsLayout.RowCount = 1;
			metricsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			metricsLayout.Size = new Size(1122, 150);
			metricsLayout.TabIndex = 1;
			//
			// pnlCpuCard
			//
			pnlCpuCard.BackColor = Color.FromArgb(17, 27, 45);
			pnlCpuCard.BorderColor = Color.FromArgb(38, 52, 77);
			pnlCpuCard.Controls.Add(glyphCpu);
			pnlCpuCard.Controls.Add(lblTotalCpuTitle);
			pnlCpuCard.Controls.Add(lblTotalCpuValue);
			pnlCpuCard.Controls.Add(lblTotalCpuCaption);
			pnlCpuCard.Controls.Add(pnlCpuTrack);
			pnlCpuCard.CornerRadius = 12;
			pnlCpuCard.Dock = DockStyle.Fill;
			pnlCpuCard.FillColor = Color.FromArgb(17, 27, 45);
			pnlCpuCard.Location = new Point(0, 0);
			pnlCpuCard.Margin = new Padding(0, 0, 10, 0);
			pnlCpuCard.Name = "pnlCpuCard";
			pnlCpuCard.Size = new Size(364, 150);
			pnlCpuCard.TabIndex = 0;
			//
			// glyphCpu
			//
			glyphCpu.BackColor = Color.FromArgb(17, 27, 45);
			glyphCpu.Font = new Font("Segoe UI Symbol", 15F, FontStyle.Regular, GraphicsUnit.Point);
			glyphCpu.ForeColor = Color.FromArgb(32, 214, 199);
			glyphCpu.Glyph = "⌁";
			glyphCpu.Location = new Point(22, 20);
			glyphCpu.Name = "glyphCpu";
			glyphCpu.Size = new Size(42, 42);
			glyphCpu.TabIndex = 0;
			//
			// lblTotalCpuTitle
			//
			lblTotalCpuTitle.AutoSize = true;
			lblTotalCpuTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblTotalCpuTitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
			lblTotalCpuTitle.ForeColor = Color.FromArgb(158, 172, 194);
			lblTotalCpuTitle.Location = new Point(78, 18);
			lblTotalCpuTitle.Name = "lblTotalCpuTitle";
			lblTotalCpuTitle.Size = new Size(72, 17);
			lblTotalCpuTitle.TabIndex = 1;
			lblTotalCpuTitle.Text = "TOTAL CPU";
			//
			// lblTotalCpuValue
			//
			lblTotalCpuValue.AutoSize = true;
			lblTotalCpuValue.BackColor = Color.FromArgb(17, 27, 45);
			lblTotalCpuValue.Font = new Font("Segoe UI", 22F, FontStyle.Bold, GraphicsUnit.Point);
			lblTotalCpuValue.ForeColor = Color.FromArgb(32, 214, 199);
			lblTotalCpuValue.Location = new Point(75, 35);
			lblTotalCpuValue.Name = "lblTotalCpuValue";
			lblTotalCpuValue.Size = new Size(84, 41);
			lblTotalCpuValue.TabIndex = 2;
			lblTotalCpuValue.Text = "0.0%";
			//
			// lblTotalCpuCaption
			//
			lblTotalCpuCaption.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			lblTotalCpuCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblTotalCpuCaption.Font = new Font("Segoe UI", 8.75F, FontStyle.Regular, GraphicsUnit.Point);
			lblTotalCpuCaption.ForeColor = Color.FromArgb(105, 124, 153);
			lblTotalCpuCaption.Location = new Point(22, 88);
			lblTotalCpuCaption.Name = "lblTotalCpuCaption";
			lblTotalCpuCaption.Size = new Size(320, 18);
			lblTotalCpuCaption.TabIndex = 3;
			lblTotalCpuCaption.Text = "Across all managed server processes";
			//
			// pnlCpuTrack
			//
			pnlCpuTrack.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
			pnlCpuTrack.BackColor = Color.FromArgb(32, 45, 66);
			pnlCpuTrack.Controls.Add(pnlCpuFill);
			pnlCpuTrack.Location = new Point(22, 120);
			pnlCpuTrack.Name = "pnlCpuTrack";
			pnlCpuTrack.Size = new Size(320, 7);
			pnlCpuTrack.TabIndex = 4;
			pnlCpuTrack.SizeChanged += MetricTrack_SizeChanged;
			//
			// pnlCpuFill
			//
			pnlCpuFill.BackColor = Color.FromArgb(32, 214, 199);
			pnlCpuFill.Dock = DockStyle.Left;
			pnlCpuFill.Location = new Point(0, 0);
			pnlCpuFill.Name = "pnlCpuFill";
			pnlCpuFill.Size = new Size(0, 7);
			pnlCpuFill.TabIndex = 0;
			//
			// pnlRamCard
			//
			pnlRamCard.BackColor = Color.FromArgb(17, 27, 45);
			pnlRamCard.BorderColor = Color.FromArgb(38, 52, 77);
			pnlRamCard.Controls.Add(glyphRam);
			pnlRamCard.Controls.Add(lblTotalRamTitle);
			pnlRamCard.Controls.Add(lblTotalRamValue);
			pnlRamCard.Controls.Add(lblTotalRamCaption);
			pnlRamCard.Controls.Add(pnlRamTrack);
			pnlRamCard.CornerRadius = 12;
			pnlRamCard.Dock = DockStyle.Fill;
			pnlRamCard.FillColor = Color.FromArgb(17, 27, 45);
			pnlRamCard.Location = new Point(380, 0);
			pnlRamCard.Margin = new Padding(6, 0, 6, 0);
			pnlRamCard.Name = "pnlRamCard";
			pnlRamCard.Size = new Size(362, 150);
			pnlRamCard.TabIndex = 1;
			//
			// glyphRam
			//
			glyphRam.BackColor = Color.FromArgb(17, 27, 45);
			glyphRam.Font = new Font("Segoe UI Symbol", 15F, FontStyle.Regular, GraphicsUnit.Point);
			glyphRam.ForeColor = Color.FromArgb(167, 139, 250);
			glyphRam.Glyph = "▦";
			glyphRam.Location = new Point(22, 20);
			glyphRam.Name = "glyphRam";
			glyphRam.Size = new Size(42, 42);
			glyphRam.TabIndex = 0;
			//
			// lblTotalRamTitle
			//
			lblTotalRamTitle.AutoSize = true;
			lblTotalRamTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblTotalRamTitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
			lblTotalRamTitle.ForeColor = Color.FromArgb(158, 172, 194);
			lblTotalRamTitle.Location = new Point(78, 18);
			lblTotalRamTitle.Name = "lblTotalRamTitle";
			lblTotalRamTitle.Size = new Size(75, 17);
			lblTotalRamTitle.TabIndex = 1;
			lblTotalRamTitle.Text = "TOTAL RAM";
			//
			// lblTotalRamValue
			//
			lblTotalRamValue.AutoSize = true;
			lblTotalRamValue.BackColor = Color.FromArgb(17, 27, 45);
			lblTotalRamValue.Font = new Font("Segoe UI", 22F, FontStyle.Bold, GraphicsUnit.Point);
			lblTotalRamValue.ForeColor = Color.FromArgb(167, 139, 250);
			lblTotalRamValue.Location = new Point(75, 35);
			lblTotalRamValue.Name = "lblTotalRamValue";
			lblTotalRamValue.Size = new Size(120, 41);
			lblTotalRamValue.TabIndex = 2;
			lblTotalRamValue.Text = "0.00 GB";
			//
			// lblTotalRamCaption
			//
			lblTotalRamCaption.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			lblTotalRamCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblTotalRamCaption.Font = new Font("Segoe UI", 8.75F, FontStyle.Regular, GraphicsUnit.Point);
			lblTotalRamCaption.ForeColor = Color.FromArgb(105, 124, 153);
			lblTotalRamCaption.Location = new Point(22, 88);
			lblTotalRamCaption.Name = "lblTotalRamCaption";
			lblTotalRamCaption.Size = new Size(318, 18);
			lblTotalRamCaption.TabIndex = 3;
			lblTotalRamCaption.Text = "0.0% of system memory";
			//
			// pnlRamTrack
			//
			pnlRamTrack.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
			pnlRamTrack.BackColor = Color.FromArgb(32, 45, 66);
			pnlRamTrack.Controls.Add(pnlRamFill);
			pnlRamTrack.Location = new Point(22, 120);
			pnlRamTrack.Name = "pnlRamTrack";
			pnlRamTrack.Size = new Size(318, 7);
			pnlRamTrack.TabIndex = 4;
			pnlRamTrack.SizeChanged += MetricTrack_SizeChanged;
			//
			// pnlRamFill
			//
			pnlRamFill.BackColor = Color.FromArgb(167, 139, 250);
			pnlRamFill.Dock = DockStyle.Left;
			pnlRamFill.Location = new Point(0, 0);
			pnlRamFill.Name = "pnlRamFill";
			pnlRamFill.Size = new Size(0, 7);
			pnlRamFill.TabIndex = 0;
			//
			// pnlActiveServersCard
			//
			pnlActiveServersCard.BackColor = Color.FromArgb(17, 27, 45);
			pnlActiveServersCard.BorderColor = Color.FromArgb(38, 52, 77);
			pnlActiveServersCard.Controls.Add(glyphActiveServers);
			pnlActiveServersCard.Controls.Add(lblActiveServersTitle);
			pnlActiveServersCard.Controls.Add(lblActiveServersValue);
			pnlActiveServersCard.Controls.Add(lblActiveServersCaption);
			pnlActiveServersCard.Controls.Add(lblActiveIndicator);
			pnlActiveServersCard.CornerRadius = 12;
			pnlActiveServersCard.Dock = DockStyle.Fill;
			pnlActiveServersCard.FillColor = Color.FromArgb(17, 27, 45);
			pnlActiveServersCard.Location = new Point(758, 0);
			pnlActiveServersCard.Margin = new Padding(10, 0, 0, 0);
			pnlActiveServersCard.Name = "pnlActiveServersCard";
			pnlActiveServersCard.Size = new Size(364, 150);
			pnlActiveServersCard.TabIndex = 2;
			//
			// glyphActiveServers
			//
			glyphActiveServers.BackColor = Color.FromArgb(17, 27, 45);
			glyphActiveServers.Font = new Font("Segoe UI Symbol", 15F, FontStyle.Regular, GraphicsUnit.Point);
			glyphActiveServers.ForeColor = Color.FromArgb(52, 211, 153);
			glyphActiveServers.Glyph = "◉";
			glyphActiveServers.Location = new Point(22, 20);
			glyphActiveServers.Name = "glyphActiveServers";
			glyphActiveServers.Size = new Size(42, 42);
			glyphActiveServers.TabIndex = 0;
			//
			// lblActiveServersTitle
			//
			lblActiveServersTitle.AutoSize = true;
			lblActiveServersTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblActiveServersTitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
			lblActiveServersTitle.ForeColor = Color.FromArgb(158, 172, 194);
			lblActiveServersTitle.Location = new Point(78, 18);
			lblActiveServersTitle.Name = "lblActiveServersTitle";
			lblActiveServersTitle.Size = new Size(108, 17);
			lblActiveServersTitle.TabIndex = 1;
			lblActiveServersTitle.Text = "ACTIVE SERVERS";
			//
			// lblActiveServersValue
			//
			lblActiveServersValue.AutoSize = true;
			lblActiveServersValue.BackColor = Color.FromArgb(17, 27, 45);
			lblActiveServersValue.Font = new Font("Segoe UI", 22F, FontStyle.Bold, GraphicsUnit.Point);
			lblActiveServersValue.ForeColor = Color.FromArgb(52, 211, 153);
			lblActiveServersValue.Location = new Point(75, 35);
			lblActiveServersValue.Name = "lblActiveServersValue";
			lblActiveServersValue.Size = new Size(36, 41);
			lblActiveServersValue.TabIndex = 2;
			lblActiveServersValue.Text = "0";
			//
			// lblActiveServersCaption
			//
			lblActiveServersCaption.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			lblActiveServersCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblActiveServersCaption.Font = new Font("Segoe UI", 8.75F, FontStyle.Regular, GraphicsUnit.Point);
			lblActiveServersCaption.ForeColor = Color.FromArgb(105, 124, 153);
			lblActiveServersCaption.Location = new Point(22, 88);
			lblActiveServersCaption.Name = "lblActiveServersCaption";
			lblActiveServersCaption.Size = new Size(320, 38);
			lblActiveServersCaption.TabIndex = 3;
			lblActiveServersCaption.Text = "No running server processes detected";
			//
			// lblActiveIndicator
			//
			lblActiveIndicator.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lblActiveIndicator.BackColor = Color.FromArgb(17, 27, 45);
			lblActiveIndicator.Font = new Font("Segoe UI Symbol", 14F, FontStyle.Bold, GraphicsUnit.Point);
			lblActiveIndicator.ForeColor = Color.FromArgb(52, 211, 153);
			lblActiveIndicator.Location = new Point(316, 17);
			lblActiveIndicator.Name = "lblActiveIndicator";
			lblActiveIndicator.Size = new Size(26, 26);
			lblActiveIndicator.TabIndex = 4;
			lblActiveIndicator.Text = "●";
			lblActiveIndicator.TextAlign = ContentAlignment.MiddleCenter;
			//
			// pnlGridCard
			//
			pnlGridCard.BackColor = Color.FromArgb(17, 27, 45);
			pnlGridCard.BorderColor = Color.FromArgb(38, 52, 77);
			pnlGridCard.Controls.Add(lblGridTitle);
			pnlGridCard.Controls.Add(lblGridSubtitle);
			pnlGridCard.Controls.Add(resourceGrid);
			pnlGridCard.Controls.Add(footerSeparator);
			pnlGridCard.Controls.Add(lblServerCount);
			pnlGridCard.Controls.Add(lblLastUpdated);
			pnlGridCard.CornerRadius = 12;
			pnlGridCard.Dock = DockStyle.Fill;
			pnlGridCard.FillColor = Color.FromArgb(17, 27, 45);
			pnlGridCard.Location = new Point(0, 246);
			pnlGridCard.Margin = new Padding(0);
			pnlGridCard.Name = "pnlGridCard";
			pnlGridCard.Size = new Size(1122, 404);
			pnlGridCard.TabIndex = 2;
			//
			// lblGridTitle
			//
			lblGridTitle.AutoSize = true;
			lblGridTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblGridTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point);
			lblGridTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblGridTitle.Location = new Point(22, 17);
			lblGridTitle.Name = "lblGridTitle";
			lblGridTitle.Size = new Size(172, 28);
			lblGridTitle.TabIndex = 0;
			lblGridTitle.Text = "Running Servers";
			//
			// lblGridSubtitle
			//
			lblGridSubtitle.AutoSize = true;
			lblGridSubtitle.BackColor = Color.FromArgb(17, 27, 45);
			lblGridSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
			lblGridSubtitle.ForeColor = Color.FromArgb(105, 124, 153);
			lblGridSubtitle.Location = new Point(24, 50);
			lblGridSubtitle.Name = "lblGridSubtitle";
			lblGridSubtitle.Size = new Size(383, 15);
			lblGridSubtitle.TabIndex = 1;
			lblGridSubtitle.Text = "Process identity and live resource usage for every active game server.";
			//
			// resourceGrid
			//
			resourceGrid.AllowUserToAddRows = false;
			resourceGrid.AllowUserToDeleteRows = false;
			resourceGrid.AllowUserToResizeRows = false;
			resourceGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			dataGridViewCellStyle1.BackColor = Color.FromArgb(13, 23, 39);
			dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
			dataGridViewCellStyle1.ForeColor = Color.FromArgb(158, 172, 194);
			dataGridViewCellStyle1.Padding = new Padding(12, 0, 12, 0);
			dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(13, 23, 39);
			dataGridViewCellStyle1.SelectionForeColor = Color.FromArgb(158, 172, 194);
			dataGridViewCellStyle1.WrapMode = DataGridViewTriState.False;
			resourceGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			resourceGrid.ColumnHeadersHeight = 44;
			resourceGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			resourceGrid.Columns.AddRange(new DataGridViewColumn[] { colStatus, colServerName, colPid, colExecutable, colCpuUsage, colRamUsage });
			dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = Color.FromArgb(12, 21, 36);
			dataGridViewCellStyle2.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
			dataGridViewCellStyle2.ForeColor = Color.FromArgb(220, 226, 237);
			dataGridViewCellStyle2.Padding = new Padding(12, 0, 12, 0);
			dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(24, 47, 63);
			dataGridViewCellStyle2.SelectionForeColor = Color.FromArgb(245, 247, 251);
			dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
			resourceGrid.DefaultCellStyle = dataGridViewCellStyle2;
			resourceGrid.BackgroundColor = Color.FromArgb(12, 21, 36);
			resourceGrid.BorderStyle = BorderStyle.None;
			resourceGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
			resourceGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
			resourceGrid.EnableHeadersVisualStyles = false;
			resourceGrid.GridColor = Color.FromArgb(30, 43, 63);
			resourceGrid.Location = new Point(22, 80);
			resourceGrid.MultiSelect = false;
			resourceGrid.Name = "resourceGrid";
			resourceGrid.ReadOnly = true;
			resourceGrid.RowHeadersVisible = false;
			resourceGrid.RowHeadersWidth = 48;
			resourceGrid.RowTemplate.Height = 52;
			resourceGrid.ScrollBars = ScrollBars.Both;
			resourceGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			resourceGrid.Size = new Size(1078, 264);
			resourceGrid.TabIndex = 2;
			resourceGrid.CellPainting += resourceGrid_CellPainting;
			resourceGrid.Paint += resourceGrid_Paint;
			//
			// colStatus
			//
			dataGridViewCellStyle3.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
			colStatus.DefaultCellStyle = dataGridViewCellStyle3;
			colStatus.HeaderText = "STATUS";
			colStatus.MinimumWidth = 105;
			colStatus.Name = "colStatus";
			colStatus.ReadOnly = true;
			colStatus.Resizable = DataGridViewTriState.False;
			colStatus.Width = 125;
			//
			// colServerName
			//
			colServerName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			dataGridViewCellStyle4.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
			colServerName.DefaultCellStyle = dataGridViewCellStyle4;
			colServerName.FillWeight = 28F;
			colServerName.HeaderText = "SERVER NAME";
			colServerName.MinimumWidth = 150;
			colServerName.Name = "colServerName";
			colServerName.ReadOnly = true;
			//
			// colPid
			//
			dataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle5.ForeColor = Color.FromArgb(158, 172, 194);
			colPid.DefaultCellStyle = dataGridViewCellStyle5;
			colPid.HeaderText = "PID";
			colPid.MinimumWidth = 75;
			colPid.Name = "colPid";
			colPid.ReadOnly = true;
			colPid.Resizable = DataGridViewTriState.False;
			colPid.Width = 90;
			//
			// colExecutable
			//
			colExecutable.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			colExecutable.FillWeight = 40F;
			colExecutable.HeaderText = "EXECUTABLE";
			colExecutable.MinimumWidth = 210;
			colExecutable.Name = "colExecutable";
			colExecutable.ReadOnly = true;
			//
			// colCpuUsage
			//
			colCpuUsage.HeaderText = "CPU USAGE";
			colCpuUsage.MinimumWidth = 145;
			colCpuUsage.Name = "colCpuUsage";
			colCpuUsage.ReadOnly = true;
			colCpuUsage.Resizable = DataGridViewTriState.False;
			colCpuUsage.Width = 155;
			//
			// colRamUsage
			//
			colRamUsage.HeaderText = "RAM USAGE";
			colRamUsage.MinimumWidth = 155;
			colRamUsage.Name = "colRamUsage";
			colRamUsage.ReadOnly = true;
			colRamUsage.Resizable = DataGridViewTriState.False;
			colRamUsage.Width = 170;
			//
			// footerSeparator
			//
			footerSeparator.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			footerSeparator.BackColor = Color.FromArgb(38, 52, 77);
			footerSeparator.Location = new Point(22, 355);
			footerSeparator.Name = "footerSeparator";
			footerSeparator.Size = new Size(1078, 1);
			footerSeparator.TabIndex = 3;
			//
			// lblServerCount
			//
			lblServerCount.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			lblServerCount.AutoSize = true;
			lblServerCount.BackColor = Color.FromArgb(17, 27, 45);
			lblServerCount.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
			lblServerCount.ForeColor = Color.FromArgb(158, 172, 194);
			lblServerCount.Location = new Point(22, 372);
			lblServerCount.Name = "lblServerCount";
			lblServerCount.Size = new Size(108, 15);
			lblServerCount.TabIndex = 4;
			lblServerCount.Text = "0 running servers";
			//
			// lblLastUpdated
			//
			lblLastUpdated.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			lblLastUpdated.BackColor = Color.FromArgb(17, 27, 45);
			lblLastUpdated.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
			lblLastUpdated.ForeColor = Color.FromArgb(105, 124, 153);
			lblLastUpdated.Location = new Point(776, 369);
			lblLastUpdated.Name = "lblLastUpdated";
			lblLastUpdated.Size = new Size(324, 21);
			lblLastUpdated.TabIndex = 5;
			lblLastUpdated.Text = "Waiting for first sample  •  Auto-refresh every 1 second";
			lblLastUpdated.TextAlign = ContentAlignment.MiddleRight;
			//
			// tmrRefresh
			//
			tmrRefresh.Enabled = false;
			tmrRefresh.Interval = 1000;
			tmrRefresh.Tick += tmrRefresh_Tick;
			//
			// ResourceMonitorGUI
			//
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.FromArgb(38, 52, 77);
			ClientSize = new Size(1180, 760);
			Controls.Add(shellLayout);
			Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
			ForeColor = Color.FromArgb(245, 247, 251);
			FormBorderStyle = FormBorderStyle.None;
			Icon = (Icon)resources.GetObject("$this.Icon");
			KeyPreview = true;
			MinimumSize = new Size(980, 640);
			Name = "ResourceMonitorGUI";
			Padding = new Padding(1);
			StartPosition = FormStartPosition.CenterParent;
			Text = "Resource Monitor";
			FormClosed += ResourceMonitorGUI_FormClosed;
			shellLayout.ResumeLayout(false);
			titleBar.ResumeLayout(false);
			titleBar.PerformLayout();
			((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
			contentPanel.ResumeLayout(false);
			contentLayout.ResumeLayout(false);
			headerPanel.ResumeLayout(false);
			headerPanel.PerformLayout();
			metricsLayout.ResumeLayout(false);
			pnlCpuCard.ResumeLayout(false);
			pnlCpuCard.PerformLayout();
			pnlCpuTrack.ResumeLayout(false);
			pnlRamCard.ResumeLayout(false);
			pnlRamCard.PerformLayout();
			pnlRamTrack.ResumeLayout(false);
			pnlActiveServersCard.ResumeLayout(false);
			pnlActiveServersCard.PerformLayout();
			pnlGridCard.ResumeLayout(false);
			pnlGridCard.PerformLayout();
			((System.ComponentModel.ISupportInitialize)resourceGrid).EndInit();
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
		private Panel contentPanel;
		private TableLayoutPanel contentLayout;
		private Panel headerPanel;
		private Label lblPageHeading;
		private Label lblPageSubtitle;
		private Label lblLiveIndicator;
		private TableLayoutPanel metricsLayout;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsCard pnlCpuCard;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph glyphCpu;
		private Label lblTotalCpuTitle;
		private Label lblTotalCpuValue;
		private Label lblTotalCpuCaption;
		private Panel pnlCpuTrack;
		private Panel pnlCpuFill;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsCard pnlRamCard;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph glyphRam;
		private Label lblTotalRamTitle;
		private Label lblTotalRamValue;
		private Label lblTotalRamCaption;
		private Panel pnlRamTrack;
		private Panel pnlRamFill;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsCard pnlActiveServersCard;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph glyphActiveServers;
		private Label lblActiveServersTitle;
		private Label lblActiveServersValue;
		private Label lblActiveServersCaption;
		private Label lblActiveIndicator;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsCard pnlGridCard;
		private Label lblGridTitle;
		private Label lblGridSubtitle;
		private DataGridView resourceGrid;
		private DataGridViewTextBoxColumn colStatus;
		private DataGridViewTextBoxColumn colServerName;
		private DataGridViewTextBoxColumn colPid;
		private DataGridViewTextBoxColumn colExecutable;
		private DataGridViewTextBoxColumn colCpuUsage;
		private DataGridViewTextBoxColumn colRamUsage;
		private Label footerSeparator;
		private Label lblServerCount;
		private Label lblLastUpdated;
		private System.Windows.Forms.Timer tmrRefresh;
	}
}
