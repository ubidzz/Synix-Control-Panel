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
			System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
			System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
			System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainGUI));
			dataGridView1 = new DataGridView();
			colGame = new DataGridViewTextBoxColumn();
			colName = new DataGridViewTextBoxColumn();
			colPort = new DataGridViewTextBoxColumn();
			colQueryPort = new DataGridViewTextBoxColumn();
			PlayerCountDisplay = new DataGridViewTextBoxColumn();
			UptimeDisplay = new DataGridViewTextBoxColumn();
			colStatus = new DataGridViewTextBoxColumn();
			rtbLog = new RichTextBox();
			btnStart = new Button();
			btnStop = new Button();
			logo = new PictureBox();
			chartHeartbeat = new System.Windows.Forms.DataVisualization.Charting.Chart();
			lblTotalRam = new Label();
			lblTotalCpu = new Label();
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
			backupServerToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator3 = new ToolStripSeparator();
			connectionTestToolStripMenuItem = new ToolStripMenuItem();
			connectionLocalTestToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator4 = new ToolStripSeparator();
			deleteServerToolStripMenuItem = new ToolStripMenuItem();
			installServer = new ToolStripMenuItem();
			toolStripSeparator1 = new ToolStripSeparator();
			btnServerActions = new Button();
			tmrResourceUpdates = new System.Windows.Forms.Timer(components);
			lblLocalIP1 = new Label();
			lblPublicIP = new Label();
			btnRestart = new Button();
			lblUpdateStatus = new Label();
			btnDownloadUpdate = new Button();
			((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
			((System.ComponentModel.ISupportInitialize)logo).BeginInit();
			((System.ComponentModel.ISupportInitialize)chartHeartbeat).BeginInit();
			contextMenuStrip.SuspendLayout();
			SuspendLayout();
			// 
			// dataGridView1
			// 
			dataGridView1.AllowUserToAddRows = false;
			dataGridView1.AllowUserToDeleteRows = false;
			dataGridView1.BorderStyle = BorderStyle.None;
			dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridView1.Columns.AddRange(new DataGridViewColumn[] { colGame, colName, colPort, colQueryPort, PlayerCountDisplay, UptimeDisplay, colStatus });
			dataGridView1.Location = new Point(12, 140);
			dataGridView1.Name = "dataGridView1";
			dataGridView1.ReadOnly = true;
			dataGridView1.Size = new Size(881, 442);
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
			colGame.Width = 200;
			// 
			// colName
			// 
			colName.DataPropertyName = "ServerName";
			colName.HeaderText = "Server Name";
			colName.Name = "colName";
			colName.ReadOnly = true;
			colName.Width = 220;
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
			// PlayerCountDisplay
			// 
			PlayerCountDisplay.DataPropertyName = "PlayerCountDisplay";
			PlayerCountDisplay.HeaderText = "Players";
			PlayerCountDisplay.Name = "PlayerCountDisplay";
			PlayerCountDisplay.ReadOnly = true;
			PlayerCountDisplay.Width = 70;
			// 
			// UptimeDisplay
			// 
			UptimeDisplay.DataPropertyName = "UptimeDisplay";
			UptimeDisplay.HeaderText = "UPTIME";
			UptimeDisplay.Name = "UptimeDisplay";
			UptimeDisplay.ReadOnly = true;
			UptimeDisplay.Width = 105;
			// 
			// colStatus
			// 
			colStatus.DataPropertyName = "Status";
			colStatus.HeaderText = "Status";
			colStatus.Name = "colStatus";
			colStatus.ReadOnly = true;
			colStatus.Width = 85;
			// 
			// rtbLog
			// 
			rtbLog.BackColor = SystemColors.ActiveCaptionText;
			rtbLog.ForeColor = Color.Lime;
			rtbLog.Location = new Point(899, 12);
			rtbLog.Name = "rtbLog";
			rtbLog.ReadOnly = true;
			rtbLog.Size = new Size(330, 605);
			rtbLog.TabIndex = 6;
			rtbLog.Text = "";
			// 
			// btnStart
			// 
			btnStart.Cursor = Cursors.Hand;
			btnStart.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			btnStart.ForeColor = Color.Green;
			btnStart.Location = new Point(574, 589);
			btnStart.Name = "btnStart";
			btnStart.Size = new Size(101, 28);
			btnStart.TabIndex = 8;
			btnStart.Text = "Start";
			btnStart.UseVisualStyleBackColor = true;
			btnStart.Click += btnStart_Click;
			// 
			// btnStop
			// 
			btnStop.Cursor = Cursors.Hand;
			btnStop.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			btnStop.ForeColor = Color.Red;
			btnStop.Location = new Point(788, 589);
			btnStop.Name = "btnStop";
			btnStop.Size = new Size(101, 28);
			btnStop.TabIndex = 9;
			btnStop.Text = "Stop";
			btnStop.UseVisualStyleBackColor = true;
			btnStop.Click += btnStop_Click;
			// 
			// logo
			// 
			logo.BackColor = Color.Transparent;
			logo.Image = Properties.Resources.synix_logo;
			logo.Location = new Point(-10, -44);
			logo.Name = "logo";
			logo.Size = new Size(321, 189);
			logo.SizeMode = PictureBoxSizeMode.StretchImage;
			logo.TabIndex = 10;
			logo.TabStop = false;
			// 
			// chartHeartbeat
			// 
			chartArea1.Name = "ChartArea1";
			chartHeartbeat.ChartAreas.Add(chartArea1);
			chartHeartbeat.Cursor = Cursors.Hand;
			legend1.Name = "Legend1";
			chartHeartbeat.Legends.Add(legend1);
			chartHeartbeat.Location = new Point(505, 28);
			chartHeartbeat.Name = "chartHeartbeat";
			series1.ChartArea = "ChartArea1";
			series1.Legend = "Legend1";
			series1.Name = "Series1";
			chartHeartbeat.Series.Add(series1);
			chartHeartbeat.Size = new Size(384, 73);
			chartHeartbeat.TabIndex = 11;
			chartHeartbeat.Text = "chart1";
			chartHeartbeat.Click += ResourceGraph_Click;
			// 
			// lblTotalRam
			// 
			lblTotalRam.AutoSize = true;
			lblTotalRam.BackColor = Color.Transparent;
			lblTotalRam.ForeColor = Color.Fuchsia;
			lblTotalRam.Location = new Point(694, 9);
			lblTotalRam.Name = "lblTotalRam";
			lblTotalRam.Size = new Size(33, 15);
			lblTotalRam.TabIndex = 12;
			lblTotalRam.Text = "RAM";
			// 
			// lblTotalCpu
			// 
			lblTotalCpu.AutoSize = true;
			lblTotalCpu.BackColor = Color.Transparent;
			lblTotalCpu.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblTotalCpu.ForeColor = Color.DarkCyan;
			lblTotalCpu.Location = new Point(522, 9);
			lblTotalCpu.Name = "lblTotalCpu";
			lblTotalCpu.Size = new Size(30, 15);
			lblTotalCpu.TabIndex = 13;
			lblTotalCpu.Text = "CPU";
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
			openServerConfig.DropDownItems.AddRange(new ToolStripItem[] { openServerFolderToolStripMenuItem, backupToolStripMenuItem, toolStripSeparator2, editServerToolStripMenuItem, openServerConfigFileToolStripMenuItem, toolStripSeparator5, updateServerToolStripMenuItem, fileValidationToolStripMenuItem, backupServerToolStripMenuItem, toolStripSeparator3, connectionTestToolStripMenuItem, connectionLocalTestToolStripMenuItem, toolStripSeparator4, deleteServerToolStripMenuItem });
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
			backupToolStripMenuItem.Text = "Backup Files";
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
			// btnServerActions
			// 
			btnServerActions.Location = new Point(12, 590);
			btnServerActions.Name = "btnServerActions";
			btnServerActions.Size = new Size(142, 28);
			btnServerActions.TabIndex = 16;
			btnServerActions.Text = "Server Actions";
			btnServerActions.UseVisualStyleBackColor = true;
			btnServerActions.Click += btnServerActionsMenu_Click;
			// 
			// tmrResourceUpdates
			// 
			tmrResourceUpdates.Enabled = true;
			tmrResourceUpdates.Interval = 1000;
			tmrResourceUpdates.Tick += tmrResourceUpdates_Tick;
			// 
			// lblLocalIP1
			// 
			lblLocalIP1.AutoSize = true;
			lblLocalIP1.BackColor = Color.Transparent;
			lblLocalIP1.Cursor = Cursors.Hand;
			lblLocalIP1.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblLocalIP1.ForeColor = Color.Lime;
			lblLocalIP1.Location = new Point(186, 605);
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
			lblPublicIP.Location = new Point(186, 585);
			lblPublicIP.Name = "lblPublicIP";
			lblPublicIP.Size = new Size(62, 17);
			lblPublicIP.TabIndex = 19;
			lblPublicIP.Text = "Public IP";
			lblPublicIP.Click += lblPublicIP_Click;
			// 
			// btnRestart
			// 
			btnRestart.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			btnRestart.ForeColor = Color.DarkCyan;
			btnRestart.Location = new Point(681, 589);
			btnRestart.Name = "btnRestart";
			btnRestart.Size = new Size(101, 28);
			btnRestart.TabIndex = 20;
			btnRestart.Text = "Restart";
			btnRestart.UseVisualStyleBackColor = true;
			btnRestart.Click += btnRestart_Click;
			// 
			// lblUpdateStatus
			// 
			lblUpdateStatus.BackColor = Color.DodgerBlue;
			lblUpdateStatus.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblUpdateStatus.ImageAlign = ContentAlignment.MiddleLeft;
			lblUpdateStatus.Location = new Point(12, 104);
			lblUpdateStatus.Name = "lblUpdateStatus";
			lblUpdateStatus.Size = new Size(881, 33);
			lblUpdateStatus.TabIndex = 21;
			lblUpdateStatus.Text = "Version Check Message";
			lblUpdateStatus.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// btnDownloadUpdate
			// 
			btnDownloadUpdate.BackColor = SystemColors.Control;
			btnDownloadUpdate.Cursor = Cursors.Hand;
			btnDownloadUpdate.FlatStyle = FlatStyle.Popup;
			btnDownloadUpdate.ImageAlign = ContentAlignment.TopLeft;
			btnDownloadUpdate.Location = new Point(710, 110);
			btnDownloadUpdate.Name = "btnDownloadUpdate";
			btnDownloadUpdate.Size = new Size(161, 22);
			btnDownloadUpdate.TabIndex = 22;
			btnDownloadUpdate.Text = "Download";
			btnDownloadUpdate.UseVisualStyleBackColor = false;
			btnDownloadUpdate.Click += btnDownloadUpdate_Click;
			// 
			// MainGUI
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackgroundImage = Properties.Resources.background;
			BackgroundImageLayout = ImageLayout.Stretch;
			ClientSize = new Size(1241, 628);
			Controls.Add(btnDownloadUpdate);
			Controls.Add(lblUpdateStatus);
			Controls.Add(btnRestart);
			Controls.Add(lblPublicIP);
			Controls.Add(lblLocalIP1);
			Controls.Add(btnServerActions);
			Controls.Add(lblTotalCpu);
			Controls.Add(lblTotalRam);
			Controls.Add(chartHeartbeat);
			Controls.Add(dataGridView1);
			Controls.Add(logo);
			Controls.Add(btnStop);
			Controls.Add(btnStart);
			Controls.Add(rtbLog);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			Name = "MainGUI";
			SizeGripStyle = SizeGripStyle.Hide;
			Text = "Synix Control Panel";
			FormClosing += MainForm_FormClosing;
			Load += MainGUI_Load;
			Shown += MainGUI_Shown;
			((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
			((System.ComponentModel.ISupportInitialize)logo).EndInit();
			((System.ComponentModel.ISupportInitialize)chartHeartbeat).EndInit();
			contextMenuStrip.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private DataGridView dataGridView1;
		private RichTextBox rtbLog;
		private Button btnStart;
		private Button btnStop;
		private PictureBox logo;
		private System.Windows.Forms.DataVisualization.Charting.Chart chartHeartbeat;
		private Label lblTotalRam;
		private Label lblTotalCpu;
		private ContextMenuStrip contextMenuStrip;
		private ToolStripMenuItem installServer;
		private ToolStripMenuItem editServer;
		private ToolStripMenuItem openServerConfig;
		private Button btnServerActions;
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
		private Button btnRestart;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripSeparator toolStripSeparator2;
		private ToolStripSeparator toolStripSeparator3;
		private ToolStripMenuItem backupToolStripMenuItem;
		private DataGridViewTextBoxColumn colGame;
		private DataGridViewTextBoxColumn colName;
		private DataGridViewTextBoxColumn colPort;
		private DataGridViewTextBoxColumn colQueryPort;
		private DataGridViewTextBoxColumn PlayerCountDisplay;
		private DataGridViewTextBoxColumn UptimeDisplay;
		private DataGridViewTextBoxColumn colStatus;
		private ToolStripMenuItem connectionLocalTestToolStripMenuItem;
		private ToolStripSeparator toolStripSeparator4;
		private ToolStripSeparator toolStripSeparator5;
		private ToolStripMenuItem fileValidationToolStripMenuItem;
		private ToolStripMenuItem backupServerToolStripMenuItem;
		private Label lblUpdateStatus;
		private Button btnDownloadUpdate;
	}
}
