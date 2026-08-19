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
	partial class MainGUI
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainGUI));
			titleBar = new Panel();
			logo = new PictureBox();
			lblAppName = new Label();
			btnSettings = new Button();
			btnGithub = new Button();
			btnDiscord = new Button();
			btnMinimize = new Button();
			btnClose = new Button();
			lblDashboardTitle = new Label();
			lblDashboardSubtitle = new Label();
			installedCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			lblInstalledCaption = new Label();
			lblInstalledValue = new Label();
			lblInstalledHint = new Label();
			runningCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			lblRunningCaption = new Label();
			lblRunningValue = new Label();
			lblRunningHint = new Label();
			cpuCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			lblCpuCaption = new Label();
			lblCpuValue = new Label();
			lblCpuHint = new Label();
			cpuGauge = new Synix_Control_Panel.SynixApp.Design.SynixGauge();
			ramCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			lblRamCaption = new Label();
			lblRamValue = new Label();
			lblRamHint = new Label();
			ramGauge = new Synix_Control_Panel.SynixApp.Design.SynixGauge();
			serversCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			lblServersTitle = new Label();
			lblServersCount = new Label();
			btnAddServer = new Synix_Control_Panel.SynixApp.Design.SynixButton();
			searchPanel = new Panel();
			lblSearchIcon = new Label();
			txtServerSearch = new TextBox();
			cmbStatusFilter = new Synix_Control_Panel.SynixApp.Design.ModernSettingsComboBox();
			dataGridView1 = new DataGridView();
			colGame = new DataGridViewTextBoxColumn();
			colName = new DataGridViewTextBoxColumn();
			colPort = new DataGridViewTextBoxColumn();
			colQueryPort = new DataGridViewTextBoxColumn();
			colPlayerCount = new DataGridViewTextBoxColumn();
			colUptime = new DataGridViewTextBoxColumn();
			colStatus = new DataGridViewTextBoxColumn();
			activityCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			lblActivityTitle = new Label();
			btnClearLog = new Button();
			rtbLog = new RichTextBox();
			networkCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			lblNetworkTitle = new Label();
			lblPublicIP = new Label();
			lblLocalIP1 = new Label();
			actionCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			picSelectedServer = new PictureBox();
			lblSelectedGame = new Label();
			lblSelectedServerName = new Label();
			btnServerActions = new Synix_Control_Panel.SynixApp.Design.SynixButton();
			btnConfigure = new Synix_Control_Panel.SynixApp.Design.SynixButton();
			btnStart = new Synix_Control_Panel.SynixApp.Design.SynixButton();
			btnRestart = new Synix_Control_Panel.SynixApp.Design.SynixButton();
			btnStop = new Synix_Control_Panel.SynixApp.Design.SynixButton();
			footerPanel = new Panel();
			lblSteamStatus = new Label();
			lblUpdateStatus = new Label();
			btnDownloadUpdate = new Synix_Control_Panel.SynixApp.Design.SynixButton();
			contextMenuStrip = new ContextMenuStrip(components);
			btnHelp = new ToolStripMenuItem();
			openServerConfig = new ToolStripMenuItem();
			openServerFolderToolStripMenuItem = new ToolStripMenuItem();
			backupToolStripMenuItem = new ToolStripMenuItem();
			openServerConfigFileToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator5 = new ToolStripSeparator();
			updateServerToolStripMenuItem = new ToolStripMenuItem();
			fileValidationToolStripMenuItem = new ToolStripMenuItem();
			btnExportBatch = new ToolStripMenuItem();
			backupServerToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator3 = new ToolStripSeparator();
			connectionTestToolStripMenuItem = new ToolStripMenuItem();
			connectionLocalTestToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator4 = new ToolStripSeparator();
			deleteServerToolStripMenuItem = new ToolStripMenuItem();
			installServer = new ToolStripMenuItem();
			toolStripSeparator1 = new ToolStripSeparator();
			tmrResourceUpdates = new System.Windows.Forms.Timer(components);
			toolTip1 = new ToolTip(components);
			titleBar.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)logo).BeginInit();
			installedCard.SuspendLayout();
			runningCard.SuspendLayout();
			cpuCard.SuspendLayout();
			ramCard.SuspendLayout();
			serversCard.SuspendLayout();
			searchPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
			activityCard.SuspendLayout();
			networkCard.SuspendLayout();
			actionCard.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)picSelectedServer).BeginInit();
			footerPanel.SuspendLayout();
			contextMenuStrip.SuspendLayout();
			SuspendLayout();
			// 
			// titleBar
			// 
			titleBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			titleBar.BackColor = Color.FromArgb(6, 12, 22);
			titleBar.Controls.Add(logo);
			titleBar.Controls.Add(lblAppName);
			titleBar.Controls.Add(btnSettings);
			titleBar.Controls.Add(btnGithub);
			titleBar.Controls.Add(btnDiscord);
			titleBar.Controls.Add(btnMinimize);
			titleBar.Controls.Add(btnClose);
			titleBar.Location = new Point(0, 0);
			titleBar.Margin = new Padding(0);
			titleBar.Name = "titleBar";
			titleBar.Size = new Size(1440, 56);
			titleBar.TabIndex = 0;
			titleBar.MouseDown += Form_Drag_MouseDown;
			// 
			// logo
			// 
			logo.BackColor = Color.FromArgb(6, 12, 22);
			logo.Image = Properties.Resources.synix_logo;
			logo.Location = new Point(17, 10);
			logo.Name = "logo";
			logo.Size = new Size(38, 36);
			logo.SizeMode = PictureBoxSizeMode.Zoom;
			logo.TabIndex = 0;
			logo.TabStop = false;
			logo.MouseDown += Form_Drag_MouseDown;
			// 
			// lblAppName
			// 
			lblAppName.AutoSize = true;
			lblAppName.BackColor = Color.FromArgb(6, 12, 22);
			lblAppName.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblAppName.ForeColor = Color.FromArgb(245, 247, 251);
			lblAppName.Location = new Point(64, 17);
			lblAppName.Name = "lblAppName";
			lblAppName.Size = new Size(160, 21);
			lblAppName.TabIndex = 1;
			lblAppName.Text = "Synix Control Panel";
			lblAppName.MouseDown += Form_Drag_MouseDown;
			// 
			// btnSettings
			// 
			btnSettings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnSettings.BackColor = Color.FromArgb(6, 12, 22);
			btnSettings.Cursor = Cursors.Hand;
			btnSettings.FlatAppearance.BorderSize = 0;
			btnSettings.FlatAppearance.MouseDownBackColor = Color.FromArgb(24, 55, 73);
			btnSettings.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, 33, 54);
			btnSettings.FlatStyle = FlatStyle.Flat;
			btnSettings.Font = new Font("Segoe UI Symbol", 12F, FontStyle.Bold);
			btnSettings.ForeColor = Color.FromArgb(245, 247, 251);
			btnSettings.Location = new Point(1217, 10);
			btnSettings.Name = "btnSettings";
			btnSettings.Size = new Size(36, 36);
			btnSettings.TabIndex = 2;
			btnSettings.TabStop = false;
			btnSettings.Text = "⚙";
			toolTip1.SetToolTip(btnSettings, "Synix Settings");
			btnSettings.UseVisualStyleBackColor = false;
			btnSettings.Click += btnSettings_Click;
			// 
			// btnGithub
			// 
			btnGithub.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnGithub.BackColor = Color.FromArgb(6, 12, 22);
			btnGithub.Cursor = Cursors.Hand;
			btnGithub.FlatAppearance.BorderSize = 0;
			btnGithub.FlatAppearance.MouseDownBackColor = Color.FromArgb(24, 55, 73);
			btnGithub.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, 33, 54);
			btnGithub.FlatStyle = FlatStyle.Flat;
			btnGithub.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
			btnGithub.ForeColor = Color.FromArgb(245, 247, 251);
			btnGithub.Location = new Point(1259, 10);
			btnGithub.Name = "btnGithub";
			btnGithub.Size = new Size(36, 36);
			btnGithub.TabIndex = 3;
			btnGithub.TabStop = false;
			btnGithub.Text = "GH";
			toolTip1.SetToolTip(btnGithub, "Open Synix on GitHub");
			btnGithub.UseVisualStyleBackColor = false;
			btnGithub.Click += btnGithub_Click;
			// 
			// btnDiscord
			// 
			btnDiscord.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnDiscord.BackColor = Color.FromArgb(6, 12, 22);
			btnDiscord.Cursor = Cursors.Hand;
			btnDiscord.FlatAppearance.BorderSize = 0;
			btnDiscord.FlatAppearance.MouseDownBackColor = Color.FromArgb(24, 55, 73);
			btnDiscord.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, 33, 54);
			btnDiscord.FlatStyle = FlatStyle.Flat;
			btnDiscord.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			btnDiscord.ForeColor = Color.FromArgb(245, 247, 251);
			btnDiscord.Location = new Point(1301, 10);
			btnDiscord.Name = "btnDiscord";
			btnDiscord.Size = new Size(36, 36);
			btnDiscord.TabIndex = 4;
			btnDiscord.TabStop = false;
			btnDiscord.Text = "D";
			toolTip1.SetToolTip(btnDiscord, "Open the Synix Discord");
			btnDiscord.UseVisualStyleBackColor = false;
			btnDiscord.Click += btnDiscord_Click;
			// 
			// btnMinimize
			// 
			btnMinimize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnMinimize.BackColor = Color.FromArgb(6, 12, 22);
			btnMinimize.Cursor = Cursors.Hand;
			btnMinimize.FlatAppearance.BorderSize = 0;
			btnMinimize.FlatAppearance.MouseDownBackColor = Color.FromArgb(24, 55, 73);
			btnMinimize.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, 33, 54);
			btnMinimize.FlatStyle = FlatStyle.Flat;
			btnMinimize.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
			btnMinimize.ForeColor = Color.FromArgb(245, 247, 251);
			btnMinimize.Location = new Point(1343, 10);
			btnMinimize.Name = "btnMinimize";
			btnMinimize.Size = new Size(36, 36);
			btnMinimize.TabIndex = 5;
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
			btnClose.FlatAppearance.MouseDownBackColor = Color.FromArgb(178, 11, 22);
			btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 17, 35);
			btnClose.FlatStyle = FlatStyle.Flat;
			btnClose.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
			btnClose.ForeColor = Color.FromArgb(245, 247, 251);
			btnClose.Location = new Point(1385, 10);
			btnClose.Name = "btnClose";
			btnClose.Size = new Size(36, 36);
			btnClose.TabIndex = 6;
			btnClose.TabStop = false;
			btnClose.Text = "✕";
			btnClose.UseVisualStyleBackColor = false;
			btnClose.Click += btnClose_Click;
			// 
			// lblDashboardTitle
			// 
			lblDashboardTitle.AutoSize = true;
			lblDashboardTitle.BackColor = Color.FromArgb(8, 13, 24);
			lblDashboardTitle.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
			lblDashboardTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblDashboardTitle.Location = new Point(28, 72);
			lblDashboardTitle.Name = "lblDashboardTitle";
			lblDashboardTitle.Size = new Size(269, 41);
			lblDashboardTitle.TabIndex = 1;
			lblDashboardTitle.Text = "Server Dashboard";
			// 
			// lblDashboardSubtitle
			// 
			lblDashboardSubtitle.AutoSize = true;
			lblDashboardSubtitle.BackColor = Color.FromArgb(8, 13, 24);
			lblDashboardSubtitle.Font = new Font("Segoe UI", 10F);
			lblDashboardSubtitle.ForeColor = Color.FromArgb(158, 172, 194);
			lblDashboardSubtitle.Location = new Point(30, 116);
			lblDashboardSubtitle.Name = "lblDashboardSubtitle";
			lblDashboardSubtitle.Size = new Size(386, 19);
			lblDashboardSubtitle.TabIndex = 2;
			lblDashboardSubtitle.Text = "Monitor and manage every game server from one workspace.";
			// 
			// installedCard
			// 
			installedCard.BackColor = Color.FromArgb(17, 27, 45);
			installedCard.BorderColor = Color.FromArgb(38, 52, 77);
			installedCard.Controls.Add(lblInstalledCaption);
			installedCard.Controls.Add(lblInstalledValue);
			installedCard.Controls.Add(lblInstalledHint);
			installedCard.FillColor = Color.FromArgb(17, 27, 45);
			installedCard.Location = new Point(28, 150);
			installedCard.Margin = new Padding(0);
			installedCard.Name = "installedCard";
			installedCard.Size = new Size(334, 112);
			installedCard.TabIndex = 3;
			// 
			// lblInstalledCaption
			// 
			lblInstalledCaption.AutoSize = true;
			lblInstalledCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblInstalledCaption.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
			lblInstalledCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblInstalledCaption.Location = new Point(18, 14);
			lblInstalledCaption.Name = "lblInstalledCaption";
			lblInstalledCaption.Size = new Size(120, 19);
			lblInstalledCaption.TabIndex = 0;
			lblInstalledCaption.Text = "Installed Servers";
			// 
			// lblInstalledValue
			// 
			lblInstalledValue.AutoSize = true;
			lblInstalledValue.BackColor = Color.FromArgb(17, 27, 45);
			lblInstalledValue.Font = new Font("Segoe UI", 25F, FontStyle.Bold);
			lblInstalledValue.ForeColor = Color.FromArgb(245, 247, 251);
			lblInstalledValue.Location = new Point(16, 35);
			lblInstalledValue.Name = "lblInstalledValue";
			lblInstalledValue.Size = new Size(40, 46);
			lblInstalledValue.TabIndex = 1;
			lblInstalledValue.Text = "0";
			// 
			// lblInstalledHint
			// 
			lblInstalledHint.AutoSize = true;
			lblInstalledHint.BackColor = Color.FromArgb(17, 27, 45);
			lblInstalledHint.Font = new Font("Segoe UI", 9F);
			lblInstalledHint.ForeColor = Color.FromArgb(105, 124, 153);
			lblInstalledHint.Location = new Point(19, 88);
			lblInstalledHint.Name = "lblInstalledHint";
			lblInstalledHint.Size = new Size(99, 15);
			lblInstalledHint.TabIndex = 2;
			lblInstalledHint.Text = "Ready to manage";
			// 
			// runningCard
			// 
			runningCard.BackColor = Color.FromArgb(17, 27, 45);
			runningCard.BorderColor = Color.FromArgb(38, 52, 77);
			runningCard.Controls.Add(lblRunningCaption);
			runningCard.Controls.Add(lblRunningValue);
			runningCard.Controls.Add(lblRunningHint);
			runningCard.FillColor = Color.FromArgb(17, 27, 45);
			runningCard.Location = new Point(378, 150);
			runningCard.Margin = new Padding(0);
			runningCard.Name = "runningCard";
			runningCard.Size = new Size(334, 112);
			runningCard.TabIndex = 4;
			// 
			// lblRunningCaption
			// 
			lblRunningCaption.AutoSize = true;
			lblRunningCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblRunningCaption.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
			lblRunningCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblRunningCaption.Location = new Point(18, 14);
			lblRunningCaption.Name = "lblRunningCaption";
			lblRunningCaption.Size = new Size(98, 19);
			lblRunningCaption.TabIndex = 0;
			lblRunningCaption.Text = "Running Now";
			// 
			// lblRunningValue
			// 
			lblRunningValue.AutoSize = true;
			lblRunningValue.BackColor = Color.FromArgb(17, 27, 45);
			lblRunningValue.Font = new Font("Segoe UI", 25F, FontStyle.Bold);
			lblRunningValue.ForeColor = Color.FromArgb(32, 214, 199);
			lblRunningValue.Location = new Point(16, 35);
			lblRunningValue.Name = "lblRunningValue";
			lblRunningValue.Size = new Size(40, 46);
			lblRunningValue.TabIndex = 1;
			lblRunningValue.Text = "0";
			// 
			// lblRunningHint
			// 
			lblRunningHint.AutoSize = true;
			lblRunningHint.BackColor = Color.FromArgb(17, 27, 45);
			lblRunningHint.Font = new Font("Segoe UI", 9F);
			lblRunningHint.ForeColor = Color.FromArgb(105, 124, 153);
			lblRunningHint.Location = new Point(19, 88);
			lblRunningHint.Name = "lblRunningHint";
			lblRunningHint.Size = new Size(80, 15);
			lblRunningHint.TabIndex = 2;
			lblRunningHint.Text = "Servers online";
			// 
			// cpuCard
			// 
			cpuCard.BackColor = Color.FromArgb(17, 27, 45);
			cpuCard.BorderColor = Color.FromArgb(38, 52, 77);
			cpuCard.Controls.Add(lblCpuCaption);
			cpuCard.Controls.Add(lblCpuValue);
			cpuCard.Controls.Add(lblCpuHint);
			cpuCard.Controls.Add(cpuGauge);
			cpuCard.FillColor = Color.FromArgb(17, 27, 45);
			cpuCard.Location = new Point(728, 150);
			cpuCard.Margin = new Padding(0);
			cpuCard.Name = "cpuCard";
			cpuCard.Size = new Size(334, 112);
			cpuCard.TabIndex = 5;
			// 
			// lblCpuCaption
			// 
			lblCpuCaption.AutoSize = true;
			lblCpuCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblCpuCaption.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
			lblCpuCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblCpuCaption.Location = new Point(18, 14);
			lblCpuCaption.Name = "lblCpuCaption";
			lblCpuCaption.Size = new Size(82, 19);
			lblCpuCaption.TabIndex = 0;
			lblCpuCaption.Text = "CPU Usage";
			// 
			// lblCpuValue
			// 
			lblCpuValue.AutoSize = true;
			lblCpuValue.BackColor = Color.FromArgb(17, 27, 45);
			lblCpuValue.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
			lblCpuValue.ForeColor = Color.FromArgb(245, 247, 251);
			lblCpuValue.Location = new Point(16, 39);
			lblCpuValue.Name = "lblCpuValue";
			lblCpuValue.Size = new Size(66, 31);
			lblCpuValue.TabIndex = 1;
			lblCpuValue.Text = "0.0%";
			// 
			// lblCpuHint
			// 
			lblCpuHint.AutoSize = true;
			lblCpuHint.BackColor = Color.FromArgb(17, 27, 45);
			lblCpuHint.Font = new Font("Segoe UI", 8.5F);
			lblCpuHint.ForeColor = Color.FromArgb(105, 124, 153);
			lblCpuHint.Location = new Point(19, 84);
			lblCpuHint.Name = "lblCpuHint";
			lblCpuHint.Size = new Size(99, 15);
			lblCpuHint.TabIndex = 2;
			lblCpuHint.Text = "Total system load";
			// 
			// cpuGauge
			// 
			cpuGauge.BackColor = Color.FromArgb(17, 27, 45);
			cpuGauge.Cursor = Cursors.Hand;
			cpuGauge.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			cpuGauge.ForeColor = Color.FromArgb(245, 247, 251);
			cpuGauge.Location = new Point(226, 10);
			cpuGauge.Name = "cpuGauge";
			cpuGauge.Size = new Size(92, 92);
			cpuGauge.TabIndex = 3;
			toolTip1.SetToolTip(cpuGauge, "Open Resource Monitor");
			cpuGauge.Click += ResourceGraph_Click;
			// 
			// ramCard
			// 
			ramCard.BackColor = Color.FromArgb(17, 27, 45);
			ramCard.BorderColor = Color.FromArgb(38, 52, 77);
			ramCard.Controls.Add(lblRamCaption);
			ramCard.Controls.Add(lblRamValue);
			ramCard.Controls.Add(lblRamHint);
			ramCard.Controls.Add(ramGauge);
			ramCard.FillColor = Color.FromArgb(17, 27, 45);
			ramCard.Location = new Point(1078, 150);
			ramCard.Margin = new Padding(0);
			ramCard.Name = "ramCard";
			ramCard.Size = new Size(334, 112);
			ramCard.TabIndex = 6;
			// 
			// lblRamCaption
			// 
			lblRamCaption.AutoSize = true;
			lblRamCaption.BackColor = Color.FromArgb(17, 27, 45);
			lblRamCaption.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
			lblRamCaption.ForeColor = Color.FromArgb(158, 172, 194);
			lblRamCaption.Location = new Point(18, 14);
			lblRamCaption.Name = "lblRamCaption";
			lblRamCaption.Size = new Size(86, 19);
			lblRamCaption.TabIndex = 0;
			lblRamCaption.Text = "RAM Usage";
			// 
			// lblRamValue
			// 
			lblRamValue.AutoSize = true;
			lblRamValue.BackColor = Color.FromArgb(17, 27, 45);
			lblRamValue.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
			lblRamValue.ForeColor = Color.FromArgb(245, 247, 251);
			lblRamValue.Location = new Point(16, 39);
			lblRamValue.Name = "lblRamValue";
			lblRamValue.Size = new Size(96, 31);
			lblRamValue.TabIndex = 1;
			lblRamValue.Text = "0.00 GB";
			// 
			// lblRamHint
			// 
			lblRamHint.AutoSize = true;
			lblRamHint.BackColor = Color.FromArgb(17, 27, 45);
			lblRamHint.Font = new Font("Segoe UI", 8.5F);
			lblRamHint.ForeColor = Color.FromArgb(105, 124, 153);
			lblRamHint.Location = new Point(19, 84);
			lblRamHint.Name = "lblRamHint";
			lblRamHint.Size = new Size(153, 15);
			lblRamHint.TabIndex = 2;
			lblRamHint.Text = "Available game-server RAM";
			// 
			// ramGauge
			// 
			ramGauge.BackColor = Color.FromArgb(17, 27, 45);
			ramGauge.Cursor = Cursors.Hand;
			ramGauge.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			ramGauge.ForeColor = Color.FromArgb(245, 247, 251);
			ramGauge.Location = new Point(226, 10);
			ramGauge.Name = "ramGauge";
			ramGauge.Size = new Size(92, 92);
			ramGauge.TabIndex = 3;
			toolTip1.SetToolTip(ramGauge, "Open Resource Monitor");
			ramGauge.Click += ResourceGraph_Click;
			// 
			// serversCard
			// 
			serversCard.BackColor = Color.FromArgb(17, 27, 45);
			serversCard.BorderColor = Color.FromArgb(38, 52, 77);
			serversCard.Controls.Add(lblServersTitle);
			serversCard.Controls.Add(lblServersCount);
			serversCard.Controls.Add(btnAddServer);
			serversCard.Controls.Add(searchPanel);
			serversCard.Controls.Add(cmbStatusFilter);
			serversCard.Controls.Add(dataGridView1);
			serversCard.FillColor = Color.FromArgb(17, 27, 45);
			serversCard.Location = new Point(28, 278);
			serversCard.Margin = new Padding(0);
			serversCard.Name = "serversCard";
			serversCard.Size = new Size(974, 470);
			serversCard.TabIndex = 7;
			// 
			// lblServersTitle
			// 
			lblServersTitle.AutoSize = true;
			lblServersTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblServersTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
			lblServersTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblServersTitle.Location = new Point(18, 15);
			lblServersTitle.Name = "lblServersTitle";
			lblServersTitle.Size = new Size(134, 25);
			lblServersTitle.TabIndex = 0;
			lblServersTitle.Text = "Game Servers";
			// 
			// lblServersCount
			// 
			lblServersCount.AutoSize = true;
			lblServersCount.BackColor = Color.FromArgb(17, 27, 45);
			lblServersCount.Font = new Font("Segoe UI", 9F);
			lblServersCount.ForeColor = Color.FromArgb(105, 124, 153);
			lblServersCount.Location = new Point(169, 22);
			lblServersCount.Name = "lblServersCount";
			lblServersCount.Size = new Size(52, 15);
			lblServersCount.TabIndex = 1;
			lblServersCount.Text = "0 servers";
			// 
			// btnAddServer
			// 
			btnAddServer.BackColor = Color.FromArgb(17, 27, 45);
			btnAddServer.BorderColor = Color.FromArgb(32, 214, 199);
			btnAddServer.Cursor = Cursors.Hand;
			btnAddServer.FillColor = Color.FromArgb(22, 111, 109);
			btnAddServer.FillColorSecondary = Color.FromArgb(31, 139, 135);
			btnAddServer.FlatStyle = FlatStyle.Flat;
			btnAddServer.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnAddServer.ForeColor = Color.FromArgb(245, 247, 251);
			btnAddServer.Location = new Point(824, 10);
			btnAddServer.Name = "btnAddServer";
			btnAddServer.Size = new Size(132, 38);
			btnAddServer.TabIndex = 2;
			btnAddServer.TabStop = false;
			btnAddServer.Text = "+  Add Server";
			btnAddServer.UseMnemonic = false;
			btnAddServer.UseVisualStyleBackColor = false;
			btnAddServer.Click += btnAddServer_Click;
			// 
			// searchPanel
			// 
			searchPanel.BackColor = Color.FromArgb(12, 21, 36);
			searchPanel.Controls.Add(lblSearchIcon);
			searchPanel.Controls.Add(txtServerSearch);
			searchPanel.Location = new Point(18, 58);
			searchPanel.Name = "searchPanel";
			searchPanel.Size = new Size(736, 40);
			searchPanel.TabIndex = 3;
			// 
			// lblSearchIcon
			// 
			lblSearchIcon.BackColor = Color.FromArgb(12, 21, 36);
			lblSearchIcon.Font = new Font("Segoe UI Symbol", 12F);
			lblSearchIcon.ForeColor = Color.FromArgb(105, 124, 153);
			lblSearchIcon.Location = new Point(10, 3);
			lblSearchIcon.Name = "lblSearchIcon";
			lblSearchIcon.Size = new Size(30, 34);
			lblSearchIcon.TabIndex = 0;
			lblSearchIcon.Text = "⌕";
			lblSearchIcon.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// txtServerSearch
			// 
			txtServerSearch.BackColor = Color.FromArgb(12, 21, 36);
			txtServerSearch.BorderStyle = BorderStyle.None;
			txtServerSearch.Font = new Font("Segoe UI", 10F);
			txtServerSearch.ForeColor = Color.FromArgb(245, 247, 251);
			txtServerSearch.Location = new Point(44, 11);
			txtServerSearch.Name = "txtServerSearch";
			txtServerSearch.PlaceholderText = "Search game or server name...";
			txtServerSearch.Size = new Size(675, 18);
			txtServerSearch.TabIndex = 1;
			txtServerSearch.TextChanged += ServerFilterChanged;
			// 
			// cmbStatusFilter
			// 
			cmbStatusFilter.ArrowColor = Color.FromArgb(158, 172, 194);
			cmbStatusFilter.BackColor = Color.FromArgb(12, 21, 36);
			cmbStatusFilter.BorderColor = Color.FromArgb(38, 52, 77);
			cmbStatusFilter.DrawMode = DrawMode.OwnerDrawFixed;
			cmbStatusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbStatusFilter.FlatStyle = FlatStyle.Flat;
			cmbStatusFilter.FocusBorderColor = Color.FromArgb(32, 214, 199);
			cmbStatusFilter.Font = new Font("Segoe UI", 10F);
			cmbStatusFilter.ForeColor = Color.FromArgb(245, 247, 251);
			cmbStatusFilter.FormattingEnabled = true;
			cmbStatusFilter.ItemHeight = 28;
			cmbStatusFilter.Items.AddRange(new object[] { "All Statuses", "Running", "Stopped", "In Progress", "Needs Attention" });
			cmbStatusFilter.Location = new Point(768, 61);
			cmbStatusFilter.Name = "cmbStatusFilter";
			cmbStatusFilter.SelectedItemBackColor = Color.FromArgb(24, 55, 73);
			cmbStatusFilter.Size = new Size(188, 34);
			cmbStatusFilter.TabIndex = 4;
			cmbStatusFilter.SelectedIndexChanged += ServerFilterChanged;
			// 
			// dataGridView1
			// 
			dataGridView1.AllowUserToAddRows = false;
			dataGridView1.AllowUserToDeleteRows = false;
			dataGridView1.AllowUserToResizeColumns = false;
			dataGridView1.AllowUserToResizeRows = false;
			dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			dataGridView1.BackgroundColor = Color.FromArgb(12, 21, 36);
			dataGridView1.BorderStyle = BorderStyle.None;
			dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
			dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
			dataGridView1.ColumnHeadersHeight = 40;
			dataGridView1.Columns.AddRange(new DataGridViewColumn[] { colGame, colName, colPort, colQueryPort, colPlayerCount, colUptime, colStatus });
			dataGridView1.EnableHeadersVisualStyles = false;
			dataGridView1.Location = new Point(18, 112);
			dataGridView1.MultiSelect = false;
			dataGridView1.Name = "dataGridView1";
			dataGridView1.ReadOnly = true;
			dataGridView1.RowHeadersVisible = false;
			dataGridView1.RowTemplate.Height = 44;
			dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dataGridView1.Size = new Size(938, 340);
			dataGridView1.TabIndex = 5;
			dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
			dataGridView1.CellFormatting += dataGridView1_CellFormatting;
			dataGridView1.DataBindingComplete += dataGridView1_DataBindingComplete;
			dataGridView1.DataError += dataGridView1_DataError;
			dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
			// 
			// colGame
			// 
			colGame.DataPropertyName = "Game";
			colGame.FillWeight = 18F;
			colGame.HeaderText = "Game";
			colGame.Name = "colGame";
			colGame.ReadOnly = true;
			colGame.SortMode = DataGridViewColumnSortMode.NotSortable;
			// 
			// colName
			// 
			colName.DataPropertyName = "ServerName";
			colName.FillWeight = 24F;
			colName.HeaderText = "Server Name";
			colName.Name = "colName";
			colName.ReadOnly = true;
			colName.SortMode = DataGridViewColumnSortMode.NotSortable;
			// 
			// colPort
			// 
			colPort.DataPropertyName = "Port";
			colPort.FillWeight = 10F;
			colPort.HeaderText = "Port";
			colPort.Name = "colPort";
			colPort.ReadOnly = true;
			colPort.SortMode = DataGridViewColumnSortMode.NotSortable;
			// 
			// colQueryPort
			// 
			colQueryPort.DataPropertyName = "QueryPort";
			colQueryPort.FillWeight = 10F;
			colQueryPort.HeaderText = "Query";
			colQueryPort.Name = "colQueryPort";
			colQueryPort.ReadOnly = true;
			colQueryPort.SortMode = DataGridViewColumnSortMode.NotSortable;
			// 
			// colPlayerCount
			// 
			colPlayerCount.DataPropertyName = "PlayerCount";
			colPlayerCount.FillWeight = 10F;
			colPlayerCount.HeaderText = "Players";
			colPlayerCount.Name = "colPlayerCount";
			colPlayerCount.ReadOnly = true;
			colPlayerCount.SortMode = DataGridViewColumnSortMode.NotSortable;
			// 
			// colUptime
			// 
			colUptime.DataPropertyName = "Uptime";
			colUptime.FillWeight = 13F;
			colUptime.HeaderText = "Uptime";
			colUptime.Name = "colUptime";
			colUptime.ReadOnly = true;
			colUptime.SortMode = DataGridViewColumnSortMode.NotSortable;
			// 
			// colStatus
			// 
			colStatus.DataPropertyName = "Status";
			colStatus.FillWeight = 15F;
			colStatus.HeaderText = "Status";
			colStatus.Name = "colStatus";
			colStatus.ReadOnly = true;
			colStatus.SortMode = DataGridViewColumnSortMode.NotSortable;
			// 
			// activityCard
			// 
			activityCard.BackColor = Color.FromArgb(17, 27, 45);
			activityCard.BorderColor = Color.FromArgb(38, 52, 77);
			activityCard.Controls.Add(lblActivityTitle);
			activityCard.Controls.Add(btnClearLog);
			activityCard.Controls.Add(rtbLog);
			activityCard.FillColor = Color.FromArgb(17, 27, 45);
			activityCard.Location = new Point(1018, 278);
			activityCard.Margin = new Padding(0);
			activityCard.Name = "activityCard";
			activityCard.Size = new Size(394, 306);
			activityCard.TabIndex = 8;
			// 
			// lblActivityTitle
			// 
			lblActivityTitle.AutoSize = true;
			lblActivityTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblActivityTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblActivityTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblActivityTitle.Location = new Point(16, 16);
			lblActivityTitle.Name = "lblActivityTitle";
			lblActivityTitle.Size = new Size(180, 21);
			lblActivityTitle.TabIndex = 0;
			lblActivityTitle.Text = "Activity & Diagnostics";
			lblActivityTitle.UseMnemonic = false;
			// 
			// btnClearLog
			// 
			btnClearLog.BackColor = Color.FromArgb(17, 27, 45);
			btnClearLog.Cursor = Cursors.Hand;
			btnClearLog.FlatAppearance.BorderSize = 0;
			btnClearLog.FlatAppearance.MouseDownBackColor = Color.FromArgb(24, 55, 73);
			btnClearLog.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, 33, 54);
			btnClearLog.FlatStyle = FlatStyle.Flat;
			btnClearLog.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
			btnClearLog.ForeColor = Color.FromArgb(105, 124, 153);
			btnClearLog.Location = new Point(316, 10);
			btnClearLog.Name = "btnClearLog";
			btnClearLog.Size = new Size(62, 34);
			btnClearLog.TabIndex = 1;
			btnClearLog.Text = "CLEAR";
			btnClearLog.UseVisualStyleBackColor = false;
			btnClearLog.Click += btnClearLog_Click;
			// 
			// rtbLog
			// 
			rtbLog.BackColor = Color.FromArgb(12, 21, 36);
			rtbLog.BorderStyle = BorderStyle.None;
			rtbLog.Font = new Font("Consolas", 8.5F);
			rtbLog.ForeColor = Color.FromArgb(158, 172, 194);
			rtbLog.Location = new Point(16, 52);
			rtbLog.Name = "rtbLog";
			rtbLog.ReadOnly = true;
			rtbLog.Size = new Size(362, 236);
			rtbLog.TabIndex = 2;
			rtbLog.Text = "";
			// 
			// networkCard
			// 
			networkCard.BackColor = Color.FromArgb(17, 27, 45);
			networkCard.BorderColor = Color.FromArgb(38, 52, 77);
			networkCard.Controls.Add(lblNetworkTitle);
			networkCard.Controls.Add(lblPublicIP);
			networkCard.Controls.Add(lblLocalIP1);
			networkCard.FillColor = Color.FromArgb(17, 27, 45);
			networkCard.Location = new Point(1018, 600);
			networkCard.Margin = new Padding(0);
			networkCard.Name = "networkCard";
			networkCard.Size = new Size(394, 148);
			networkCard.TabIndex = 9;
			// 
			// lblNetworkTitle
			// 
			lblNetworkTitle.AutoSize = true;
			lblNetworkTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblNetworkTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblNetworkTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblNetworkTitle.Location = new Point(16, 16);
			lblNetworkTitle.Name = "lblNetworkTitle";
			lblNetworkTitle.Size = new Size(76, 21);
			lblNetworkTitle.TabIndex = 0;
			lblNetworkTitle.Text = "Network";
			// 
			// lblPublicIP
			// 
			lblPublicIP.AutoEllipsis = true;
			lblPublicIP.BackColor = Color.FromArgb(12, 21, 36);
			lblPublicIP.Cursor = Cursors.Hand;
			lblPublicIP.Font = new Font("Segoe UI", 9.5F);
			lblPublicIP.ForeColor = Color.FromArgb(158, 172, 194);
			lblPublicIP.Location = new Point(16, 52);
			lblPublicIP.Name = "lblPublicIP";
			lblPublicIP.Padding = new Padding(12, 0, 0, 0);
			lblPublicIP.Size = new Size(362, 34);
			lblPublicIP.TabIndex = 1;
			lblPublicIP.Text = "Public IP: Fetching...";
			lblPublicIP.TextAlign = ContentAlignment.MiddleLeft;
			lblPublicIP.Click += lblPublicIP_Click;
			// 
			// lblLocalIP1
			// 
			lblLocalIP1.AutoEllipsis = true;
			lblLocalIP1.BackColor = Color.FromArgb(12, 21, 36);
			lblLocalIP1.Cursor = Cursors.Hand;
			lblLocalIP1.Font = new Font("Segoe UI", 9.5F);
			lblLocalIP1.ForeColor = Color.FromArgb(158, 172, 194);
			lblLocalIP1.Location = new Point(16, 96);
			lblLocalIP1.Name = "lblLocalIP1";
			lblLocalIP1.Padding = new Padding(12, 0, 0, 0);
			lblLocalIP1.Size = new Size(362, 34);
			lblLocalIP1.TabIndex = 2;
			lblLocalIP1.Text = "LAN IP: Fetching...";
			lblLocalIP1.TextAlign = ContentAlignment.MiddleLeft;
			lblLocalIP1.Click += lblLocalIP_Click;
			// 
			// actionCard
			// 
			actionCard.BackColor = Color.FromArgb(17, 27, 45);
			actionCard.BorderColor = Color.FromArgb(38, 52, 77);
			actionCard.Controls.Add(picSelectedServer);
			actionCard.Controls.Add(lblSelectedGame);
			actionCard.Controls.Add(lblSelectedServerName);
			actionCard.Controls.Add(btnServerActions);
			actionCard.Controls.Add(btnConfigure);
			actionCard.Controls.Add(btnStart);
			actionCard.Controls.Add(btnRestart);
			actionCard.Controls.Add(btnStop);
			actionCard.FillColor = Color.FromArgb(17, 27, 45);
			actionCard.Location = new Point(28, 764);
			actionCard.Margin = new Padding(0);
			actionCard.Name = "actionCard";
			actionCard.Size = new Size(1384, 86);
			actionCard.TabIndex = 10;
			// 
			// picSelectedServer
			// 
			picSelectedServer.BackColor = Color.FromArgb(12, 21, 36);
			picSelectedServer.Location = new Point(14, 14);
			picSelectedServer.Name = "picSelectedServer";
			picSelectedServer.Padding = new Padding(8);
			picSelectedServer.Size = new Size(58, 58);
			picSelectedServer.SizeMode = PictureBoxSizeMode.Zoom;
			picSelectedServer.TabIndex = 0;
			picSelectedServer.TabStop = false;
			// 
			// lblSelectedGame
			// 
			lblSelectedGame.AutoEllipsis = true;
			lblSelectedGame.BackColor = Color.FromArgb(17, 27, 45);
			lblSelectedGame.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
			lblSelectedGame.ForeColor = Color.FromArgb(245, 247, 251);
			lblSelectedGame.Location = new Point(86, 18);
			lblSelectedGame.Name = "lblSelectedGame";
			lblSelectedGame.Size = new Size(480, 22);
			lblSelectedGame.TabIndex = 1;
			lblSelectedGame.Text = "Select a game server";
			// 
			// lblSelectedServerName
			// 
			lblSelectedServerName.AutoEllipsis = true;
			lblSelectedServerName.BackColor = Color.FromArgb(17, 27, 45);
			lblSelectedServerName.Font = new Font("Segoe UI", 9F);
			lblSelectedServerName.ForeColor = Color.FromArgb(105, 124, 153);
			lblSelectedServerName.Location = new Point(87, 46);
			lblSelectedServerName.Name = "lblSelectedServerName";
			lblSelectedServerName.Size = new Size(479, 18);
			lblSelectedServerName.TabIndex = 2;
			lblSelectedServerName.Text = "Choose a row to unlock server controls";
			// 
			// btnServerActions
			// 
			btnServerActions.BackColor = Color.FromArgb(17, 27, 45);
			btnServerActions.BorderColor = Color.FromArgb(55, 76, 108);
			btnServerActions.Cursor = Cursors.Hand;
			btnServerActions.FillColor = Color.FromArgb(12, 21, 36);
			btnServerActions.FillColorSecondary = Color.FromArgb(20, 33, 54);
			btnServerActions.FlatStyle = FlatStyle.Flat;
			btnServerActions.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnServerActions.ForeColor = Color.FromArgb(245, 247, 251);
			btnServerActions.Location = new Point(698, 21);
			btnServerActions.Name = "btnServerActions";
			btnServerActions.Size = new Size(154, 44);
			btnServerActions.TabIndex = 3;
			btnServerActions.TabStop = false;
			btnServerActions.Text = "Server Actions  ▴";
			btnServerActions.UseMnemonic = false;
			btnServerActions.UseVisualStyleBackColor = false;
			btnServerActions.Click += btnServerActionsMenu_Click;
			// 
			// btnConfigure
			// 
			btnConfigure.BackColor = Color.FromArgb(17, 27, 45);
			btnConfigure.BorderColor = Color.FromArgb(55, 76, 108);
			btnConfigure.Cursor = Cursors.Hand;
			btnConfigure.FillColor = Color.FromArgb(12, 21, 36);
			btnConfigure.FillColorSecondary = Color.FromArgb(20, 33, 54);
			btnConfigure.FlatStyle = FlatStyle.Flat;
			btnConfigure.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnConfigure.ForeColor = Color.FromArgb(158, 172, 194);
			btnConfigure.Location = new Point(862, 21);
			btnConfigure.Name = "btnConfigure";
			btnConfigure.Size = new Size(124, 44);
			btnConfigure.TabIndex = 4;
			btnConfigure.TabStop = false;
			btnConfigure.Text = "Configure";
			btnConfigure.UseMnemonic = false;
			btnConfigure.UseVisualStyleBackColor = false;
			btnConfigure.Click += btnEdit_Click;
			// 
			// btnStart
			// 
			btnStart.BackColor = Color.FromArgb(17, 27, 45);
			btnStart.BorderColor = Color.FromArgb(46, 151, 119);
			btnStart.Cursor = Cursors.Hand;
			btnStart.FillColor = Color.FromArgb(20, 92, 74);
			btnStart.FillColorSecondary = Color.FromArgb(25, 116, 91);
			btnStart.FlatStyle = FlatStyle.Flat;
			btnStart.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnStart.ForeColor = Color.FromArgb(80, 230, 164);
			btnStart.Location = new Point(1002, 21);
			btnStart.Name = "btnStart";
			btnStart.Size = new Size(116, 44);
			btnStart.TabIndex = 5;
			btnStart.TabStop = false;
			btnStart.Text = "▶  Start";
			btnStart.UseMnemonic = false;
			btnStart.UseVisualStyleBackColor = false;
			btnStart.Click += btnStart_Click;
			// 
			// btnRestart
			// 
			btnRestart.BackColor = Color.FromArgb(17, 27, 45);
			btnRestart.BorderColor = Color.FromArgb(42, 112, 151);
			btnRestart.Cursor = Cursors.Hand;
			btnRestart.FillColor = Color.FromArgb(15, 65, 89);
			btnRestart.FillColorSecondary = Color.FromArgb(22, 84, 113);
			btnRestart.FlatStyle = FlatStyle.Flat;
			btnRestart.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnRestart.ForeColor = Color.FromArgb(87, 190, 240);
			btnRestart.Location = new Point(1128, 21);
			btnRestart.Name = "btnRestart";
			btnRestart.Size = new Size(116, 44);
			btnRestart.TabIndex = 6;
			btnRestart.TabStop = false;
			btnRestart.Text = "↻  Restart";
			btnRestart.UseMnemonic = false;
			btnRestart.UseVisualStyleBackColor = false;
			btnRestart.Click += btnRestart_Click;
			// 
			// btnStop
			// 
			btnStop.BackColor = Color.FromArgb(17, 27, 45);
			btnStop.BorderColor = Color.FromArgb(148, 60, 74);
			btnStop.Cursor = Cursors.Hand;
			btnStop.FillColor = Color.FromArgb(88, 35, 48);
			btnStop.FillColorSecondary = Color.FromArgb(112, 42, 58);
			btnStop.FlatStyle = FlatStyle.Flat;
			btnStop.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnStop.ForeColor = Color.FromArgb(250, 116, 128);
			btnStop.Location = new Point(1254, 21);
			btnStop.Name = "btnStop";
			btnStop.Size = new Size(116, 44);
			btnStop.TabIndex = 7;
			btnStop.TabStop = false;
			btnStop.Text = "■  Stop";
			btnStop.UseMnemonic = false;
			btnStop.UseVisualStyleBackColor = false;
			btnStop.Click += btnStop_Click;
			// 
			// footerPanel
			// 
			footerPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			footerPanel.BackColor = Color.FromArgb(6, 12, 22);
			footerPanel.Controls.Add(lblSteamStatus);
			footerPanel.Controls.Add(lblUpdateStatus);
			footerPanel.Controls.Add(btnDownloadUpdate);
			footerPanel.Location = new Point(0, 868);
			footerPanel.Margin = new Padding(0);
			footerPanel.Name = "footerPanel";
			footerPanel.Size = new Size(1440, 32);
			footerPanel.TabIndex = 11;
			// 
			// lblSteamStatus
			// 
			lblSteamStatus.AutoSize = true;
			lblSteamStatus.BackColor = Color.FromArgb(6, 12, 22);
			lblSteamStatus.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
			lblSteamStatus.ForeColor = Color.FromArgb(32, 214, 199);
			lblSteamStatus.Location = new Point(20, 8);
			lblSteamStatus.Name = "lblSteamStatus";
			lblSteamStatus.Size = new Size(117, 15);
			lblSteamStatus.TabIndex = 0;
			lblSteamStatus.Text = "●  SteamCMD ready";
			// 
			// lblUpdateStatus
			// 
			lblUpdateStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lblUpdateStatus.AutoEllipsis = true;
			lblUpdateStatus.BackColor = Color.FromArgb(6, 12, 22);
			lblUpdateStatus.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
			lblUpdateStatus.ForeColor = Color.FromArgb(105, 124, 153);
			lblUpdateStatus.Location = new Point(792, 5);
			lblUpdateStatus.Name = "lblUpdateStatus";
			lblUpdateStatus.Size = new Size(450, 22);
			lblUpdateStatus.TabIndex = 1;
			lblUpdateStatus.Text = "Checking for updates...";
			lblUpdateStatus.TextAlign = ContentAlignment.MiddleRight;
			// 
			// btnDownloadUpdate
			// 
			btnDownloadUpdate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnDownloadUpdate.BackColor = Color.FromArgb(6, 12, 22);
			btnDownloadUpdate.BorderColor = Color.FromArgb(32, 214, 199);
			btnDownloadUpdate.BorderRadius = 7;
			btnDownloadUpdate.Cursor = Cursors.Hand;
			btnDownloadUpdate.FillColor = Color.FromArgb(22, 111, 109);
			btnDownloadUpdate.FillColorSecondary = Color.FromArgb(31, 139, 135);
			btnDownloadUpdate.FlatStyle = FlatStyle.Flat;
			btnDownloadUpdate.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
			btnDownloadUpdate.ForeColor = Color.FromArgb(245, 247, 251);
			btnDownloadUpdate.Location = new Point(1252, 3);
			btnDownloadUpdate.Name = "btnDownloadUpdate";
			btnDownloadUpdate.Size = new Size(168, 26);
			btnDownloadUpdate.TabIndex = 2;
			btnDownloadUpdate.TabStop = false;
			btnDownloadUpdate.Text = "Download Update";
			btnDownloadUpdate.UseMnemonic = false;
			btnDownloadUpdate.UseVisualStyleBackColor = false;
			btnDownloadUpdate.Visible = false;
			btnDownloadUpdate.Click += btnDownloadUpdate_Click;
			// 
			// contextMenuStrip
			// 
			contextMenuStrip.Items.AddRange(new ToolStripItem[] { btnHelp, openServerConfig, installServer, toolStripSeparator1 });
			contextMenuStrip.Name = "contextMenuStrip";
			contextMenuStrip.Size = new Size(181, 98);
			// 
			// btnHelp
			// 
			btnHelp.Name = "btnHelp";
			btnHelp.Size = new Size(180, 22);
			btnHelp.Text = "Help Center";
			btnHelp.Click += btnHelp_Click;
			// 
			// openServerConfig
			// 
			openServerConfig.DropDownItems.AddRange(new ToolStripItem[] { openServerFolderToolStripMenuItem, backupToolStripMenuItem, openServerConfigFileToolStripMenuItem, toolStripSeparator5, updateServerToolStripMenuItem, fileValidationToolStripMenuItem, btnExportBatch, backupServerToolStripMenuItem, toolStripSeparator3, connectionTestToolStripMenuItem, connectionLocalTestToolStripMenuItem, toolStripSeparator4, deleteServerToolStripMenuItem });
			openServerConfig.Name = "openServerConfig";
			openServerConfig.Size = new Size(180, 22);
			openServerConfig.Text = "Server Options";
			// 
			// openServerFolderToolStripMenuItem
			// 
			openServerFolderToolStripMenuItem.Name = "openServerFolderToolStripMenuItem";
			openServerFolderToolStripMenuItem.Size = new Size(196, 22);
			openServerFolderToolStripMenuItem.Text = "Open Server Folder";
			openServerFolderToolStripMenuItem.Click += btnOpenFolder_Click;
			// 
			// backupToolStripMenuItem
			// 
			backupToolStripMenuItem.Name = "backupToolStripMenuItem";
			backupToolStripMenuItem.Size = new Size(196, 22);
			backupToolStripMenuItem.Text = "Open Backup Folder";
			backupToolStripMenuItem.Click += btnOpenBackup_Click;
			// 
			// openServerConfigFileToolStripMenuItem
			// 
			openServerConfigFileToolStripMenuItem.Name = "openServerConfigFileToolStripMenuItem";
			openServerConfigFileToolStripMenuItem.Size = new Size(196, 22);
			openServerConfigFileToolStripMenuItem.Text = "Open Config Editor";
			openServerConfigFileToolStripMenuItem.Click += btnOpenConfig_Click;
			// 
			// toolStripSeparator5
			// 
			toolStripSeparator5.Name = "toolStripSeparator5";
			toolStripSeparator5.Size = new Size(193, 6);
			// 
			// updateServerToolStripMenuItem
			// 
			updateServerToolStripMenuItem.Name = "updateServerToolStripMenuItem";
			updateServerToolStripMenuItem.Size = new Size(196, 22);
			updateServerToolStripMenuItem.Text = "Update Server";
			updateServerToolStripMenuItem.Click += btnUpdate_Click;
			// 
			// fileValidationToolStripMenuItem
			// 
			fileValidationToolStripMenuItem.Name = "fileValidationToolStripMenuItem";
			fileValidationToolStripMenuItem.Size = new Size(196, 22);
			fileValidationToolStripMenuItem.Text = "Validate Game Files";
			fileValidationToolStripMenuItem.Click += btnFileValidation_Click;
			// 
			// btnExportBatch
			// 
			btnExportBatch.Name = "btnExportBatch";
			btnExportBatch.Size = new Size(196, 22);
			btnExportBatch.Text = "Create Batch File";
			btnExportBatch.Click += btnExportBatch_Click;
			// 
			// backupServerToolStripMenuItem
			// 
			backupServerToolStripMenuItem.Name = "backupServerToolStripMenuItem";
			backupServerToolStripMenuItem.Size = new Size(196, 22);
			backupServerToolStripMenuItem.Text = "Backup Server";
			backupServerToolStripMenuItem.Click += btnBackup_Click;
			// 
			// toolStripSeparator3
			// 
			toolStripSeparator3.Name = "toolStripSeparator3";
			toolStripSeparator3.Size = new Size(193, 6);
			// 
			// connectionTestToolStripMenuItem
			// 
			connectionTestToolStripMenuItem.Name = "connectionTestToolStripMenuItem";
			connectionTestToolStripMenuItem.Size = new Size(196, 22);
			connectionTestToolStripMenuItem.Text = "Test WAN Connectivity";
			connectionTestToolStripMenuItem.Click += btnPublicConnection_Click;
			// 
			// connectionLocalTestToolStripMenuItem
			// 
			connectionLocalTestToolStripMenuItem.Name = "connectionLocalTestToolStripMenuItem";
			connectionLocalTestToolStripMenuItem.Size = new Size(196, 22);
			connectionLocalTestToolStripMenuItem.Text = "Test LAN Connectivity";
			connectionLocalTestToolStripMenuItem.Click += btnLocalConnection_Click;
			// 
			// toolStripSeparator4
			// 
			toolStripSeparator4.Name = "toolStripSeparator4";
			toolStripSeparator4.Size = new Size(193, 6);
			// 
			// deleteServerToolStripMenuItem
			// 
			deleteServerToolStripMenuItem.Name = "deleteServerToolStripMenuItem";
			deleteServerToolStripMenuItem.Size = new Size(196, 22);
			deleteServerToolStripMenuItem.Text = "Delete Server";
			deleteServerToolStripMenuItem.Click += btnDelete_Click;
			// 
			// installServer
			// 
			installServer.Name = "installServer";
			installServer.Size = new Size(180, 22);
			installServer.Text = "Install New Server";
			installServer.Click += btnAddServer_Click;
			// 
			// toolStripSeparator1
			// 
			toolStripSeparator1.Name = "toolStripSeparator1";
			toolStripSeparator1.Size = new Size(177, 6);
			// 
			// tmrResourceUpdates
			// 
			tmrResourceUpdates.Interval = 1000;
			tmrResourceUpdates.Tick += tmrResourceUpdates_Tick;
			// 
			// MainGUI
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.FromArgb(8, 13, 24);
			ClientSize = new Size(1440, 900);
			Controls.Add(titleBar);
			Controls.Add(lblDashboardTitle);
			Controls.Add(lblDashboardSubtitle);
			Controls.Add(installedCard);
			Controls.Add(runningCard);
			Controls.Add(cpuCard);
			Controls.Add(ramCard);
			Controls.Add(serversCard);
			Controls.Add(activityCard);
			Controls.Add(networkCard);
			Controls.Add(actionCard);
			Controls.Add(footerPanel);
			Font = new Font("Segoe UI", 9F);
			ForeColor = Color.FromArgb(245, 247, 251);
			FormBorderStyle = FormBorderStyle.None;
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			Name = "MainGUI";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "Synix Control Panel";
			FormClosing += MainForm_FormClosing;
			Shown += MainGUI_Shown;
			MouseDown += Form_Drag_MouseDown;
			titleBar.ResumeLayout(false);
			titleBar.PerformLayout();
			((System.ComponentModel.ISupportInitialize)logo).EndInit();
			installedCard.ResumeLayout(false);
			installedCard.PerformLayout();
			runningCard.ResumeLayout(false);
			runningCard.PerformLayout();
			cpuCard.ResumeLayout(false);
			cpuCard.PerformLayout();
			ramCard.ResumeLayout(false);
			ramCard.PerformLayout();
			serversCard.ResumeLayout(false);
			serversCard.PerformLayout();
			searchPanel.ResumeLayout(false);
			searchPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
			activityCard.ResumeLayout(false);
			activityCard.PerformLayout();
			networkCard.ResumeLayout(false);
			networkCard.PerformLayout();
			actionCard.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)picSelectedServer).EndInit();
			footerPanel.ResumeLayout(false);
			footerPanel.PerformLayout();
			contextMenuStrip.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Panel titleBar;
		private PictureBox logo;
		private Label lblAppName;
		private Button btnSettings;
		private Button btnGithub;
		private Button btnDiscord;
		private Button btnMinimize;
		private Button btnClose;
		private Label lblDashboardTitle;
		private Label lblDashboardSubtitle;
		private SynixApp.Design.ModernSettingsCard installedCard;
		private Label lblInstalledCaption;
		private Label lblInstalledValue;
		private Label lblInstalledHint;
		private SynixApp.Design.ModernSettingsCard runningCard;
		private Label lblRunningCaption;
		private Label lblRunningValue;
		private Label lblRunningHint;
		private SynixApp.Design.ModernSettingsCard cpuCard;
		private Label lblCpuCaption;
		private Label lblCpuValue;
		private Label lblCpuHint;
		private SynixApp.Design.SynixGauge cpuGauge;
		private SynixApp.Design.ModernSettingsCard ramCard;
		private Label lblRamCaption;
		private Label lblRamValue;
		private Label lblRamHint;
		private SynixApp.Design.SynixGauge ramGauge;
		private SynixApp.Design.ModernSettingsCard serversCard;
		private Label lblServersTitle;
		private Label lblServersCount;
		private SynixApp.Design.SynixButton btnAddServer;
		private Panel searchPanel;
		private Label lblSearchIcon;
		private TextBox txtServerSearch;
		private SynixApp.Design.ModernSettingsComboBox cmbStatusFilter;
		private DataGridView dataGridView1;
		private DataGridViewTextBoxColumn colGame;
		private DataGridViewTextBoxColumn colName;
		private DataGridViewTextBoxColumn colPort;
		private DataGridViewTextBoxColumn colQueryPort;
		private DataGridViewTextBoxColumn colPlayerCount;
		private DataGridViewTextBoxColumn colUptime;
		private DataGridViewTextBoxColumn colStatus;
		private SynixApp.Design.ModernSettingsCard activityCard;
		private Label lblActivityTitle;
		private Button btnClearLog;
		private RichTextBox rtbLog;
		private SynixApp.Design.ModernSettingsCard networkCard;
		private Label lblNetworkTitle;
		private Label lblPublicIP;
		private Label lblLocalIP1;
		private SynixApp.Design.ModernSettingsCard actionCard;
		private PictureBox picSelectedServer;
		private Label lblSelectedGame;
		private Label lblSelectedServerName;
		private SynixApp.Design.SynixButton btnServerActions;
		private SynixApp.Design.SynixButton btnConfigure;
		private SynixApp.Design.SynixButton btnStart;
		private SynixApp.Design.SynixButton btnRestart;
		private SynixApp.Design.SynixButton btnStop;
		private Panel footerPanel;
		private Label lblSteamStatus;
		private Label lblUpdateStatus;
		private SynixApp.Design.SynixButton btnDownloadUpdate;
		private ContextMenuStrip contextMenuStrip;
		private ToolStripMenuItem btnHelp;
		private ToolStripMenuItem openServerConfig;
		private ToolStripMenuItem openServerFolderToolStripMenuItem;
		private ToolStripMenuItem backupToolStripMenuItem;
		private ToolStripMenuItem openServerConfigFileToolStripMenuItem;
		private ToolStripSeparator toolStripSeparator5;
		private ToolStripMenuItem updateServerToolStripMenuItem;
		private ToolStripMenuItem fileValidationToolStripMenuItem;
		private ToolStripMenuItem btnExportBatch;
		private ToolStripMenuItem backupServerToolStripMenuItem;
		private ToolStripSeparator toolStripSeparator3;
		private ToolStripMenuItem connectionTestToolStripMenuItem;
		private ToolStripMenuItem connectionLocalTestToolStripMenuItem;
		private ToolStripSeparator toolStripSeparator4;
		private ToolStripMenuItem deleteServerToolStripMenuItem;
		private ToolStripMenuItem installServer;
		private ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.Timer tmrResourceUpdates;
		private ToolTip toolTip1;
	}
}