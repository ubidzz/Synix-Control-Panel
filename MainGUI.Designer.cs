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
			((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
			((System.ComponentModel.ISupportInitialize)logo).BeginInit();
			SuspendLayout();
			// 
			// dataGridView1
			// 
			dataGridView1.AllowUserToAddRows = false;
			dataGridView1.AllowUserToDeleteRows = false;
			dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridView1.Columns.AddRange(new DataGridViewColumn[] { colGame, colName, colPort, colQueryPort, colPassword, colAdminPassword, colStatus });
			dataGridView1.Location = new Point(12, 95);
			dataGridView1.Name = "dataGridView1";
			dataGridView1.ReadOnly = true;
			dataGridView1.Size = new Size(793, 487);
			dataGridView1.TabIndex = 0;
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
			// 
			// colPort
			// 
			colPort.DataPropertyName = "Port";
			colPort.HeaderText = "Port";
			colPort.Name = "colPort";
			colPort.ReadOnly = true;
			// 
			// colQueryPort
			// 
			colQueryPort.DataPropertyName = "QueryPort";
			colQueryPort.HeaderText = "Query Port";
			colQueryPort.Name = "colQueryPort";
			colQueryPort.ReadOnly = true;
			// 
			// colPassword
			// 
			colPassword.DataPropertyName = "Password";
			colPassword.HeaderText = "Password";
			colPassword.Name = "colPassword";
			colPassword.ReadOnly = true;
			// 
			// colAdminPassword
			// 
			colAdminPassword.DataPropertyName = "AdminPassword";
			colAdminPassword.HeaderText = "Admin Password";
			colAdminPassword.Name = "colAdminPassword";
			colAdminPassword.ReadOnly = true;
			// 
			// colStatus
			// 
			colStatus.DataPropertyName = "Status";
			colStatus.HeaderText = "Status";
			colStatus.Name = "colStatus";
			colStatus.ReadOnly = true;
			// 
			// btnDelete
			// 
			btnDelete.Location = new Point(308, 589);
			btnDelete.Name = "btnDelete";
			btnDelete.Size = new Size(142, 28);
			btnDelete.TabIndex = 2;
			btnDelete.Text = "Delete Server";
			btnDelete.UseVisualStyleBackColor = true;
			btnDelete.Click += btnDelete_Click;
			// 
			// btnAddServer
			// 
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
			btnEdit.Location = new Point(160, 589);
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
			rtbLog.Location = new Point(811, 12);
			rtbLog.Name = "rtbLog";
			rtbLog.ReadOnly = true;
			rtbLog.Size = new Size(418, 605);
			rtbLog.TabIndex = 6;
			rtbLog.Text = "";
			// 
			// btnStart
			// 
			btnStart.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			btnStart.ForeColor = Color.Green;
			btnStart.Location = new Point(593, 589);
			btnStart.Name = "btnStart";
			btnStart.Size = new Size(103, 28);
			btnStart.TabIndex = 8;
			btnStart.Text = "Start Server";
			btnStart.UseVisualStyleBackColor = true;
			btnStart.Click += btnStart_Click;
			// 
			// btnStop
			// 
			btnStop.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			btnStop.ForeColor = Color.Red;
			btnStop.Location = new Point(702, 589);
			btnStop.Name = "btnStop";
			btnStop.Size = new Size(103, 28);
			btnStop.TabIndex = 9;
			btnStop.Text = "Stop Server";
			btnStop.UseVisualStyleBackColor = true;
			btnStop.Click += btnStop_Click;
			// 
			// logo
			// 
			logo.BackColor = Color.Transparent;
			logo.Image = Game_Server_Control_Panel.Properties.Resources.synix_logo;
			logo.Location = new Point(-19, -49);
			logo.Name = "logo";
			logo.Size = new Size(321, 189);
			logo.SizeMode = PictureBoxSizeMode.StretchImage;
			logo.TabIndex = 10;
			logo.TabStop = false;
			// 
			// MainGUI
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackgroundImage = Game_Server_Control_Panel.Properties.Resources.background;
			BackgroundImageLayout = ImageLayout.Stretch;
			ClientSize = new Size(1241, 628);
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
			Shown += MainGUI_Shown;
			((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
			((System.ComponentModel.ISupportInitialize)logo).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private DataGridView dataGridView1;
		private Button btnDelete;
		private Button btnAddServer;
		private Button btnEdit;
		private RichTextBox rtbLog;
		private Button btnStart;
		private Button btnStop;
		private DataGridViewTextBoxColumn colGame;
		private DataGridViewTextBoxColumn colName;
		private DataGridViewTextBoxColumn colPort;
		private DataGridViewTextBoxColumn colQueryPort;
		private DataGridViewTextBoxColumn colPassword;
		private DataGridViewTextBoxColumn colAdminPassword;
		private DataGridViewTextBoxColumn colStatus;
		private PictureBox logo;
	}
}
