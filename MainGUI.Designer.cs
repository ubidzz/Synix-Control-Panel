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
			colPassword = new DataGridViewTextBoxColumn();
			colAdminPassword = new DataGridViewTextBoxColumn();
			colStatus = new DataGridViewTextBoxColumn();
			btnDelete = new Button();
			btnAddServer = new Button();
			btnEdit = new Button();
			rtbLog = new RichTextBox();
			btnStart = new Button();
			btnStop = new Button();
			logo = new PictureBox();
			chartHeartbeat = new System.Windows.Forms.DataVisualization.Charting.Chart();
			lblTotalRam = new Label();
			lblTotalCpu = new Label();
			tmrResourceUpdates = new System.Windows.Forms.Timer(components);
			button1 = new Button();
			((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
			((System.ComponentModel.ISupportInitialize)logo).BeginInit();
			((System.ComponentModel.ISupportInitialize)chartHeartbeat).BeginInit();
			SuspendLayout();
			// 
			// dataGridView1
			// 
			dataGridView1.AllowUserToAddRows = false;
			dataGridView1.AllowUserToDeleteRows = false;
			dataGridView1.BorderStyle = BorderStyle.None;
			dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridView1.Columns.AddRange(new DataGridViewColumn[] { colGame, colName, colPort, colQueryPort, colPassword, colAdminPassword, colStatus });
			dataGridView1.Location = new Point(12, 95);
			dataGridView1.Name = "dataGridView1";
			dataGridView1.ReadOnly = true;
			dataGridView1.Size = new Size(881, 487);
			dataGridView1.TabIndex = 0;
			dataGridView1.CellFormatting += dataGridView1_CellFormatting;
			dataGridView1.CellPainting += dataGridView1_CellPainting;
			// 
			// colGame
			// 
			colGame.DataPropertyName = "Game";
			colGame.HeaderText = "Game";
			colGame.Name = "colGame";
			colGame.ReadOnly = true;
			// 
			// colName
			// 
			colName.DataPropertyName = "ServerName";
			colName.HeaderText = "Server Name";
			colName.Name = "colName";
			colName.ReadOnly = true;
			colName.Width = 200;
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
			// colPassword
			// 
			colPassword.DataPropertyName = "Password";
			colPassword.HeaderText = "Password";
			colPassword.Name = "colPassword";
			colPassword.ReadOnly = true;
			colPassword.Width = 150;
			// 
			// colAdminPassword
			// 
			colAdminPassword.DataPropertyName = "AdminPassword";
			colAdminPassword.HeaderText = "Admin Password";
			colAdminPassword.Name = "colAdminPassword";
			colAdminPassword.ReadOnly = true;
			colAdminPassword.Width = 150;
			// 
			// colStatus
			// 
			colStatus.DataPropertyName = "Status";
			colStatus.HeaderText = "Status";
			colStatus.Name = "colStatus";
			colStatus.ReadOnly = true;
			colStatus.Width = 80;
			// 
			// btnDelete
			// 
			btnDelete.Cursor = Cursors.Hand;
			btnDelete.Location = new Point(455, 589);
			btnDelete.Name = "btnDelete";
			btnDelete.Size = new Size(142, 28);
			btnDelete.TabIndex = 2;
			btnDelete.Text = "Delete Server";
			btnDelete.UseVisualStyleBackColor = true;
			btnDelete.Click += btnDelete_Click;
			// 
			// btnAddServer
			// 
			btnAddServer.Cursor = Cursors.Hand;
			btnAddServer.Location = new Point(12, 589);
			btnAddServer.Name = "btnAddServer";
			btnAddServer.Size = new Size(142, 28);
			btnAddServer.TabIndex = 4;
			btnAddServer.Text = "Add Server";
			btnAddServer.UseVisualStyleBackColor = true;
			btnAddServer.Click += btnAddServer_Click;
			// 
			// btnEdit
			// 
			btnEdit.Cursor = Cursors.Hand;
			btnEdit.Location = new Point(160, 588);
			btnEdit.Name = "btnEdit";
			btnEdit.Size = new Size(142, 28);
			btnEdit.TabIndex = 5;
			btnEdit.Text = "Edit Server";
			btnEdit.UseVisualStyleBackColor = true;
			btnEdit.Click += btnEdit_Click;
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
			btnStart.Location = new Point(603, 589);
			btnStart.Name = "btnStart";
			btnStart.Size = new Size(142, 28);
			btnStart.TabIndex = 8;
			btnStart.Text = "Start Server";
			btnStart.UseVisualStyleBackColor = true;
			btnStart.Click += btnStart_Click;
			// 
			// btnStop
			// 
			btnStop.Cursor = Cursors.Hand;
			btnStop.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			btnStop.ForeColor = Color.Red;
			btnStop.Location = new Point(751, 589);
			btnStop.Name = "btnStop";
			btnStop.Size = new Size(142, 28);
			btnStop.TabIndex = 9;
			btnStop.Text = "Stop Server";
			btnStop.UseVisualStyleBackColor = true;
			btnStop.Click += btnStop_Click;
			// 
			// logo
			// 
			logo.BackColor = Color.Transparent;
			logo.Image = Properties.Resources.synix_logo;
			logo.Location = new Point(-19, -49);
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
			chartHeartbeat.Size = new Size(384, 64);
			chartHeartbeat.TabIndex = 11;
			chartHeartbeat.Text = "chart1";
			chartHeartbeat.Click += ResourceGraph_Click;
			// 
			// lblTotalRam
			// 
			lblTotalRam.AutoSize = true;
			lblTotalRam.BackColor = Color.Transparent;
			lblTotalRam.ForeColor = Color.Fuchsia;
			lblTotalRam.Location = new Point(661, 9);
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
			lblTotalCpu.Location = new Point(516, 9);
			lblTotalCpu.Name = "lblTotalCpu";
			lblTotalCpu.Size = new Size(30, 15);
			lblTotalCpu.TabIndex = 13;
			lblTotalCpu.Text = "CPU";
			// 
			// tmrResourceUpdates
			// 
			tmrResourceUpdates.Enabled = true;
			tmrResourceUpdates.Interval = 1000;
			tmrResourceUpdates.Tick += tmrResourceUpdates_Tick;
			// 
			// button1
			// 
			button1.Cursor = Cursors.Hand;
			button1.Location = new Point(307, 588);
			button1.Name = "button1";
			button1.Size = new Size(142, 28);
			button1.TabIndex = 14;
			button1.Text = "Update Server";
			button1.UseVisualStyleBackColor = true;
			button1.Click += btnUpdate_Click;
			// 
			// MainGUI
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackgroundImage = Properties.Resources.background;
			BackgroundImageLayout = ImageLayout.Stretch;
			ClientSize = new Size(1241, 628);
			Controls.Add(button1);
			Controls.Add(lblTotalCpu);
			Controls.Add(lblTotalRam);
			Controls.Add(chartHeartbeat);
			Controls.Add(dataGridView1);
			Controls.Add(logo);
			Controls.Add(btnStop);
			Controls.Add(btnStart);
			Controls.Add(rtbLog);
			Controls.Add(btnEdit);
			Controls.Add(btnAddServer);
			Controls.Add(btnDelete);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			Name = "MainGUI";
			SizeGripStyle = SizeGripStyle.Hide;
			Text = "Synix Control Panel";
			FormClosing += GUI_FormClosing;
			Load += MainGUI_Load;
			Shown += MainGUI_Shown;
			((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
			((System.ComponentModel.ISupportInitialize)logo).EndInit();
			((System.ComponentModel.ISupportInitialize)chartHeartbeat).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private DataGridView dataGridView1;
		private Button btnDelete;
		private Button btnAddServer;
		private Button btnEdit;
		private RichTextBox rtbLog;
		private Button btnStart;
		private Button btnStop;
		private PictureBox logo;
		private System.Windows.Forms.DataVisualization.Charting.Chart chartHeartbeat;
		private Label lblTotalRam;
		private Label lblTotalCpu;
		private System.Windows.Forms.Timer tmrResourceUpdates;
		private DataGridViewTextBoxColumn colGame;
		private DataGridViewTextBoxColumn colName;
		private DataGridViewTextBoxColumn colPort;
		private DataGridViewTextBoxColumn colQueryPort;
		private DataGridViewTextBoxColumn colPassword;
		private DataGridViewTextBoxColumn colAdminPassword;
		private DataGridViewTextBoxColumn colStatus;
		private Button button1;
	}
}
