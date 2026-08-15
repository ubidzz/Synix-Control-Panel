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
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainGUI));
			dataGridView1 = new DataGridView();
			colGame = new DataGridViewTextBoxColumn();
			colName = new DataGridViewTextBoxColumn();
			colPort = new DataGridViewTextBoxColumn();
			colQueryPort = new DataGridViewTextBoxColumn();
			colPlayerCount = new DataGridViewTextBoxColumn();
			colUptime = new DataGridViewTextBoxColumn();
			colStatus = new DataGridViewTextBoxColumn();
			rtbLog = new RichTextBox();
			logo = new PictureBox();
			contextMenuStrip = new ContextMenuStrip(components);
			btnHelp = new ToolStripMenuItem();
			openServerConfig = new ToolStripMenuItem();
			openServerFolderToolStripMenuItem = new ToolStripMenuItem();
			backupToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator2 = new ToolStripSeparator();
			editServerToolStripMenuItem = new ToolStripMenuItem();
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
			lblLocalIP1 = new Label();
			lblPublicIP = new Label();
			lblUpdateStatus = new Label();
			btnClose = new Button();
			btnMinimize = new Button();
			btnDiscord = new Button();
			btnGithub = new Button();
			btnSettings = new Button();
			toolTip1 = new ToolTip(components);
			cpuGauge = new Synix_Control_Panel.SynixApp.Design.SynixGauge();
			ramGauge = new Synix_Control_Panel.SynixApp.Design.SynixGauge();
			btnStart = new Synix_Control_Panel.SynixApp.Design.SynixButton();
			btnRestart = new Synix_Control_Panel.SynixApp.Design.SynixButton();
			btnStop = new Synix_Control_Panel.SynixApp.Design.SynixButton();
			btnServerActions = new Synix_Control_Panel.SynixApp.Design.SynixButton();
			btnDownloadUpdate = new Synix_Control_Panel.SynixApp.Design.SynixButton();
			((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
			((System.ComponentModel.ISupportInitialize)logo).BeginInit();
			contextMenuStrip.SuspendLayout();
			SuspendLayout();
			// 
			// dataGridView1
			// 
			dataGridView1.AllowUserToAddRows = false;
			dataGridView1.AllowUserToDeleteRows = false;
			dataGridView1.BorderStyle = BorderStyle.None;
			dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridView1.Columns.AddRange(new DataGridViewColumn[] { colGame, colName, colPort, colQueryPort, colPlayerCount, colUptime, colStatus });
			dataGridView1.Location = new Point(12, 171);
			dataGridView1.MultiSelect = false;
			dataGridView1.Name = "dataGridView1";
			dataGridView1.ReadOnly = true;
			dataGridView1.Size = new Size(881, 538);
			dataGridView1.TabIndex = 0;
			dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
			dataGridView1.CellFormatting += dataGridView1_CellFormatting;
			dataGridView1.CellPainting += dataGridView1_CellPainting;
			// 
			// colGame
			// 
			colGame.DataPropertyName = "Game";
			colGame.HeaderText = "Game";
			colGame.Name = "colGame";
			colGame.ReadOnly = true;
			colGame.Width = 175;
			// 
			// colName
			// 
			colName.DataPropertyName = "ServerName";
			colName.HeaderText = "Server Name";
			colName.Name = "colName";
			colName.ReadOnly = true;
			colName.Width = 265;
			// 
			// colPort
			// 
			colPort.DataPropertyName = "Port";
			colPort.HeaderText = "Port";
			colPort.Name = "colPort";
			colPort.ReadOnly = true;
			colPort.Width = 80;
			// 
			// colQueryPort
			// 
			colQueryPort.DataPropertyName = "QueryPort";
			colQueryPort.HeaderText = "Query Port";
			colQueryPort.Name = "colQueryPort";
			colQueryPort.ReadOnly = true;
			colQueryPort.Width = 80;
			// 
			// colPlayerCount
			// 
			colPlayerCount.DataPropertyName = "PlayerCount";
			colPlayerCount.HeaderText = "Players";
			colPlayerCount.Name = "colPlayerCount";
			colPlayerCount.ReadOnly = true;
			colPlayerCount.Width = 70;
			// 
			// colUptime
			// 
			colUptime.DataPropertyName = "Uptime";
			colUptime.HeaderText = "UPTIME";
			colUptime.Name = "colUptime";
			colUptime.ReadOnly = true;
			colUptime.Width = 80;
			// 
			// colStatus
			// 
			colStatus.DataPropertyName = "Status";
			colStatus.HeaderText = "Status";
			colStatus.Name = "colStatus";
			colStatus.ReadOnly = true;
			colStatus.Width = 90;
			// 
			// rtbLog
			// 
			rtbLog.BackColor = SystemColors.ActiveCaptionText;
			rtbLog.ForeColor = Color.Lime;
			rtbLog.Location = new Point(899, 45);
			rtbLog.Name = "rtbLog";
			rtbLog.ReadOnly = true;
			rtbLog.Size = new Size(330, 664);
			rtbLog.TabIndex = 6;
			rtbLog.Text = "";
			// 
			// logo
			// 
			logo.BackColor = Color.Transparent;
			logo.Image = Properties.Resources.synix_logo;
			logo.Location = new Point(-10, -70);
			logo.Name = "logo";
			logo.Size = new Size(353, 270);
			logo.SizeMode = PictureBoxSizeMode.StretchImage;
			logo.TabIndex = 10;
			logo.TabStop = false;
			logo.MouseDown += Form_Drag_MouseDown;
			// 
			// contextMenuStrip
			// 
			contextMenuStrip.Items.AddRange(new ToolStripItem[] { btnHelp, openServerConfig, installServer, toolStripSeparator1 });
			contextMenuStrip.Name = "contextMenuStrip";
			contextMenuStrip.Size = new Size(152, 76);
			// 
			// btnHelp
			// 
			btnHelp.Name = "btnHelp";
			btnHelp.Size = new Size(151, 22);
			btnHelp.Text = "Help";
			btnHelp.Click += btnHelp_Click;
			// 
			// openServerConfig
			// 
			openServerConfig.DropDownItems.AddRange(new ToolStripItem[] { openServerFolderToolStripMenuItem, backupToolStripMenuItem, toolStripSeparator2, editServerToolStripMenuItem, openServerConfigFileToolStripMenuItem, toolStripSeparator5, updateServerToolStripMenuItem, fileValidationToolStripMenuItem, btnExportBatch, backupServerToolStripMenuItem, toolStripSeparator3, connectionTestToolStripMenuItem, connectionLocalTestToolStripMenuItem, toolStripSeparator4, deleteServerToolStripMenuItem });
			openServerConfig.Name = "openServerConfig";
			openServerConfig.Size = new Size(151, 22);
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
			// toolStripSeparator2
			// 
			toolStripSeparator2.Name = "toolStripSeparator2";
			toolStripSeparator2.Size = new Size(193, 6);
			// 
			// editServerToolStripMenuItem
			// 
			editServerToolStripMenuItem.Name = "editServerToolStripMenuItem";
			editServerToolStripMenuItem.Size = new Size(196, 22);
			editServerToolStripMenuItem.Text = "Edit Server";
			editServerToolStripMenuItem.Click += btnEdit_Click;
			// 
			// openServerConfigFileToolStripMenuItem
			// 
			openServerConfigFileToolStripMenuItem.Name = "openServerConfigFileToolStripMenuItem";
			openServerConfigFileToolStripMenuItem.Size = new Size(196, 22);
			openServerConfigFileToolStripMenuItem.Text = "Server Config File";
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
			fileValidationToolStripMenuItem.Text = "Game Validation";
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
			connectionTestToolStripMenuItem.Text = "Connection Public Test";
			connectionTestToolStripMenuItem.Click += btnPublicConnection_Click;
			// 
			// connectionLocalTestToolStripMenuItem
			// 
			connectionLocalTestToolStripMenuItem.Name = "connectionLocalTestToolStripMenuItem";
			connectionLocalTestToolStripMenuItem.Size = new Size(196, 22);
			connectionLocalTestToolStripMenuItem.Text = "Connection Local Test";
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
			installServer.Size = new Size(151, 22);
			installServer.Text = "Install Server";
			installServer.Click += btnAddServer_Click;
			// 
			// toolStripSeparator1
			// 
			toolStripSeparator1.Name = "toolStripSeparator1";
			toolStripSeparator1.Size = new Size(148, 6);
			// 
			// tmrResourceUpdates
			// 
			tmrResourceUpdates.Enabled = true;
			tmrResourceUpdates.Tick += tmrResourceUpdates_Tick;
			// 
			// lblLocalIP1
			// 
			lblLocalIP1.AutoSize = true;
			lblLocalIP1.BackColor = Color.Transparent;
			lblLocalIP1.Cursor = Cursors.Hand;
			lblLocalIP1.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblLocalIP1.ForeColor = Color.Lime;
			lblLocalIP1.Location = new Point(543, 733);
			lblLocalIP1.Name = "lblLocalIP1";
			lblLocalIP1.Size = new Size(56, 17);
			lblLocalIP1.TabIndex = 18;
			lblLocalIP1.Text = "Local IP";
			lblLocalIP1.Click += lblLocalIP_Click;
			// 
			// lblPublicIP
			// 
			lblPublicIP.AutoSize = true;
			lblPublicIP.BackColor = Color.Transparent;
			lblPublicIP.Cursor = Cursors.Hand;
			lblPublicIP.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblPublicIP.ForeColor = Color.Lime;
			lblPublicIP.Location = new Point(263, 733);
			lblPublicIP.Name = "lblPublicIP";
			lblPublicIP.Size = new Size(62, 17);
			lblPublicIP.TabIndex = 19;
			lblPublicIP.Text = "Public IP";
			lblPublicIP.Click += lblPublicIP_Click;
			// 
			// lblUpdateStatus
			// 
			lblUpdateStatus.BackColor = Color.DodgerBlue;
			lblUpdateStatus.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblUpdateStatus.ImageAlign = ContentAlignment.MiddleLeft;
			lblUpdateStatus.Location = new Point(12, 131);
			lblUpdateStatus.Name = "lblUpdateStatus";
			lblUpdateStatus.Size = new Size(881, 37);
			lblUpdateStatus.TabIndex = 21;
			lblUpdateStatus.Text = "Version Check Message";
			lblUpdateStatus.TextAlign = ContentAlignment.MiddleLeft;
			lblUpdateStatus.MouseDown += Form_Drag_MouseDown;
			// 
			// btnClose
			// 
			btnClose.Cursor = Cursors.Hand;
			btnClose.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			btnClose.Location = new Point(1204, 9);
			btnClose.Name = "btnClose";
			btnClose.Size = new Size(25, 25);
			btnClose.TabIndex = 24;
			btnClose.Text = "❌";
			btnClose.UseVisualStyleBackColor = true;
			btnClose.Click += btnClose_Click;
			// 
			// btnMinimize
			// 
			btnMinimize.Cursor = Cursors.Hand;
			btnMinimize.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			btnMinimize.Location = new Point(1173, 9);
			btnMinimize.Name = "btnMinimize";
			btnMinimize.Size = new Size(25, 25);
			btnMinimize.TabIndex = 25;
			btnMinimize.Text = "-";
			btnMinimize.UseVisualStyleBackColor = true;
			btnMinimize.Click += btnMinimize_Click;
			// 
			// btnDiscord
			// 
			btnDiscord.Cursor = Cursors.Hand;
			btnDiscord.Location = new Point(1142, 9);
			btnDiscord.Name = "btnDiscord";
			btnDiscord.Size = new Size(25, 25);
			btnDiscord.TabIndex = 26;
			btnDiscord.Text = "Discord Icon";
			toolTip1.SetToolTip(btnDiscord, "Go to Synix Discord");
			btnDiscord.UseVisualStyleBackColor = true;
			btnDiscord.Click += btnDiscord_Click;
			// 
			// btnGithub
			// 
			btnGithub.Cursor = Cursors.Hand;
			btnGithub.Location = new Point(1111, 9);
			btnGithub.Name = "btnGithub";
			btnGithub.Size = new Size(25, 25);
			btnGithub.TabIndex = 27;
			btnGithub.Text = "Github";
			toolTip1.SetToolTip(btnGithub, "Go to Synix Github");
			btnGithub.UseVisualStyleBackColor = true;
			btnGithub.Click += btnGithub_Click;
			// 
			// btnSettings
			// 
			btnSettings.Cursor = Cursors.Hand;
			btnSettings.Location = new Point(1080, 9);
			btnSettings.Name = "btnSettings";
			btnSettings.Size = new Size(25, 25);
			btnSettings.TabIndex = 28;
			btnSettings.Text = "Settings";
			toolTip1.SetToolTip(btnSettings, "Synix Settings");
			btnSettings.UseVisualStyleBackColor = true;
			btnSettings.Click += btnSettings_Click;
			// 
			// cpuGauge
			// 
			cpuGauge.BackColor = Color.Transparent;
			cpuGauge.Cursor = Cursors.Hand;
			cpuGauge.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
			cpuGauge.ForeColor = Color.White;
			cpuGauge.Location = new Point(543, 3);
			cpuGauge.MaxValue = 100F;
			cpuGauge.Name = "cpuGauge";
			cpuGauge.Size = new Size(210, 145);
			cpuGauge.TabIndex = 34;
			cpuGauge.Text = "synixGauge1";
			toolTip1.SetToolTip(cpuGauge, "Click to open the resource graph window");
			cpuGauge.Value = 0F;
			cpuGauge.Click += ResourceGraph_Click;
			// 
			// ramGauge
			// 
			ramGauge.BackColor = Color.Transparent;
			ramGauge.Cursor = Cursors.Hand;
			ramGauge.ForeColor = Color.White;
			ramGauge.Location = new Point(694, 3);
			ramGauge.MaxValue = 100F;
			ramGauge.Name = "ramGauge";
			ramGauge.Size = new Size(210, 145);
			ramGauge.TabIndex = 35;
			ramGauge.Text = "synixGauge1";
			toolTip1.SetToolTip(ramGauge, "Click to open the resource graph window");
			ramGauge.Value = 0F;
			ramGauge.Click += ResourceGraph_Click;
			// 
			// btnStart
			// 
			btnStart.BackColor = Color.Transparent;
			btnStart.BorderColor = Color.FromArgb(0, 80, 150);
			btnStart.BorderRadius = 8;
			btnStart.BorderSize = 1;
			btnStart.FillColor = Color.FromArgb(10, 20, 30);
			btnStart.FillColorSecondary = Color.FromArgb(20, 35, 50);
			btnStart.FlatStyle = FlatStyle.Flat;
			btnStart.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			btnStart.ForeColor = Color.FromArgb(50, 220, 50);
			btnStart.Location = new Point(812, 720);
			btnStart.Name = "btnStart";
			btnStart.Size = new Size(130, 40);
			btnStart.TabIndex = 29;
			btnStart.Text = "🚀 Start";
			btnStart.UseVisualStyleBackColor = false;
			btnStart.Click += btnStart_Click;
			// 
			// btnRestart
			// 
			btnRestart.BackColor = Color.Transparent;
			btnRestart.BorderColor = Color.FromArgb(0, 80, 150);
			btnRestart.BorderRadius = 8;
			btnRestart.BorderSize = 1;
			btnRestart.FillColor = Color.FromArgb(10, 20, 30);
			btnRestart.FillColorSecondary = Color.FromArgb(20, 35, 50);
			btnRestart.FlatStyle = FlatStyle.Flat;
			btnRestart.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			btnRestart.ForeColor = Color.FromArgb(0, 192, 192);
			btnRestart.Location = new Point(949, 720);
			btnRestart.Name = "btnRestart";
			btnRestart.Size = new Size(130, 40);
			btnRestart.TabIndex = 30;
			btnRestart.Text = "🔄 Restart";
			btnRestart.UseVisualStyleBackColor = false;
			btnRestart.Click += btnRestart_Click;
			// 
			// btnStop
			// 
			btnStop.BackColor = Color.Transparent;
			btnStop.BorderColor = Color.FromArgb(0, 80, 150);
			btnStop.BorderRadius = 8;
			btnStop.BorderSize = 1;
			btnStop.FillColor = Color.FromArgb(10, 20, 30);
			btnStop.FillColorSecondary = Color.FromArgb(20, 35, 50);
			btnStop.FlatStyle = FlatStyle.Flat;
			btnStop.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			btnStop.ForeColor = Color.Red;
			btnStop.Location = new Point(1085, 720);
			btnStop.Name = "btnStop";
			btnStop.Size = new Size(130, 40);
			btnStop.TabIndex = 31;
			btnStop.Text = "❌ Stop";
			btnStop.UseVisualStyleBackColor = false;
			btnStop.Click += btnStop_Click;
			// 
			// btnServerActions
			// 
			btnServerActions.BackColor = Color.Transparent;
			btnServerActions.BorderColor = Color.FromArgb(0, 80, 150);
			btnServerActions.BorderRadius = 8;
			btnServerActions.BorderSize = 1;
			btnServerActions.FillColor = Color.FromArgb(10, 20, 30);
			btnServerActions.FillColorSecondary = Color.FromArgb(20, 35, 50);
			btnServerActions.FlatStyle = FlatStyle.Flat;
			btnServerActions.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			btnServerActions.ForeColor = Color.White;
			btnServerActions.Location = new Point(21, 720);
			btnServerActions.Name = "btnServerActions";
			btnServerActions.Size = new Size(160, 40);
			btnServerActions.TabIndex = 32;
			btnServerActions.Text = "🛠️ Server Actions";
			btnServerActions.UseVisualStyleBackColor = false;
			btnServerActions.Click += btnServerActionsMenu_Click;
			// 
			// btnDownloadUpdate
			// 
			btnDownloadUpdate.BackColor = Color.Transparent;
			btnDownloadUpdate.BorderColor = Color.FromArgb(0, 80, 150);
			btnDownloadUpdate.BorderRadius = 8;
			btnDownloadUpdate.BorderSize = 1;
			btnDownloadUpdate.FillColor = Color.FromArgb(10, 20, 30);
			btnDownloadUpdate.FillColorSecondary = Color.FromArgb(20, 35, 50);
			btnDownloadUpdate.FlatStyle = FlatStyle.Flat;
			btnDownloadUpdate.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			btnDownloadUpdate.ForeColor = Color.FromArgb(50, 220, 50);
			btnDownloadUpdate.Location = new Point(680, 137);
			btnDownloadUpdate.Name = "btnDownloadUpdate";
			btnDownloadUpdate.Size = new Size(200, 25);
			btnDownloadUpdate.TabIndex = 33;
			btnDownloadUpdate.Text = "Download from GitHub";
			btnDownloadUpdate.UseVisualStyleBackColor = false;
			btnDownloadUpdate.Visible = false;
			btnDownloadUpdate.Click += btnDownloadUpdate_Click;
			// 
			// MainGUI
			// 
			AutoScaleDimensions = new SizeF(7F, 17F);
			AutoScaleMode = AutoScaleMode.Font;
			BackgroundImage = Properties.Resources.background;
			BackgroundImageLayout = ImageLayout.Stretch;
			ClientSize = new Size(1241, 772);
			Controls.Add(btnStart);
			Controls.Add(btnRestart);
			Controls.Add(btnStop);
			Controls.Add(btnServerActions);
			Controls.Add(btnDownloadUpdate);
			Controls.Add(btnSettings);
			Controls.Add(btnGithub);
			Controls.Add(btnDiscord);
			Controls.Add(btnMinimize);
			Controls.Add(btnClose);
			Controls.Add(lblUpdateStatus);
			Controls.Add(lblPublicIP);
			Controls.Add(lblLocalIP1);
			Controls.Add(dataGridView1);
			Controls.Add(logo);
			Controls.Add(rtbLog);
			Controls.Add(cpuGauge);
			Controls.Add(ramGauge);
			Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
			FormBorderStyle = FormBorderStyle.None;
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			Name = "MainGUI";
			SizeGripStyle = SizeGripStyle.Hide;
			Text = "Synix Control Panel";
			FormClosing += MainForm_FormClosing;
			Shown += MainGUI_Shown;
			MouseDown += Form_Drag_MouseDown;
			((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
			((System.ComponentModel.ISupportInitialize)logo).EndInit();
			contextMenuStrip.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private DataGridView dataGridView1;
		private RichTextBox rtbLog;
		private PictureBox logo;
		private ContextMenuStrip contextMenuStrip;
		private ToolStripMenuItem installServer;
		private ToolStripMenuItem editServer;
		private ToolStripMenuItem openServerConfig;
		private System.Windows.Forms.Timer tmrResourceUpdates;
		private ToolStripMenuItem btnHelp;
		private ToolStripMenuItem openServerConfigFileToolStripMenuItem;
		private ToolStripMenuItem openServerFolderToolStripMenuItem;
		private ToolStripMenuItem connectionTestToolStripMenuItem;
		private Label lblLocalIP;
		private Label lblLocalIP1;
		private Label lblPublicIP;
		private ToolStripMenuItem editServerToolStripMenuItem;
		private ToolStripMenuItem deleteServerToolStripMenuItem;
		private ToolStripMenuItem updateServerToolStripMenuItem;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripSeparator toolStripSeparator2;
		private ToolStripSeparator toolStripSeparator3;
		private ToolStripMenuItem backupToolStripMenuItem;
		private ToolStripMenuItem connectionLocalTestToolStripMenuItem;
		private ToolStripSeparator toolStripSeparator4;
		private ToolStripSeparator toolStripSeparator5;
		private ToolStripMenuItem fileValidationToolStripMenuItem;
		private ToolStripMenuItem backupServerToolStripMenuItem;
		private Label lblUpdateStatus;
		private ToolStripMenuItem btnExportBatch;
		private Button btnClose;
		private Button btnMinimize;
		private Button btnDiscord;
		private Button btnGithub;
		private Button btnSettings;
		private ToolTip toolTip1;
		private DataGridViewTextBoxColumn colGame;
		private DataGridViewTextBoxColumn colName;
		private DataGridViewTextBoxColumn colPort;
		private DataGridViewTextBoxColumn colQueryPort;
		private DataGridViewTextBoxColumn colPlayerCount;
		private DataGridViewTextBoxColumn colUptime;
		private DataGridViewTextBoxColumn colStatus;
		private SynixApp.Design.SynixButton btnStart;
		private SynixApp.Design.SynixButton btnRestart;
		private SynixApp.Design.SynixButton btnStop;
		private SynixApp.Design.SynixButton btnServerActions;
		private SynixApp.Design.SynixButton btnDownloadUpdate;
		private SynixApp.Design.SynixGauge cpuGauge;
		private SynixApp.Design.SynixGauge ramGauge;
	}
}