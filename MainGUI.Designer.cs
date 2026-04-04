namespace Game_Server_Control_Panel
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
			dataGridView1 = new DataGridView();
			colGame = new DataGridViewTextBoxColumn();
			colName = new DataGridViewTextBoxColumn();
			colPort = new DataGridViewTextBoxColumn();
			colQueryPort = new DataGridViewTextBoxColumn();
			colPassword = new DataGridViewTextBoxColumn();
			colStatus = new DataGridViewTextBoxColumn();
			btnDelete = new Button();
			btnAddServer = new Button();
			btnEdit = new Button();
			rtbLog = new RichTextBox();
			ServerManagerLabel = new Label();
			btnStart = new Button();
			btnStop = new Button();
			((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
			SuspendLayout();
			// 
			// dataGridView1
			// 
			dataGridView1.AllowUserToAddRows = false;
			dataGridView1.AllowUserToDeleteRows = false;
			dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridView1.Columns.AddRange(new DataGridViewColumn[] { colGame, colName, colPort, colQueryPort, colPassword, colStatus });
			dataGridView1.Location = new Point(12, 42);
			dataGridView1.Name = "dataGridView1";
			dataGridView1.ReadOnly = true;
			dataGridView1.Size = new Size(698, 503);
			dataGridView1.TabIndex = 0;
			// 
			// colGame
			// 
			colGame.HeaderText = "Game";
			colGame.Name = "colGame";
			colGame.ReadOnly = true;
			// 
			// colName
			// 
			colName.HeaderText = "Name";
			colName.Name = "colName";
			colName.ReadOnly = true;
			// 
			// colPort
			// 
			colPort.HeaderText = "Port";
			colPort.Name = "colPort";
			colPort.ReadOnly = true;
			// 
			// colQueryPort
			// 
			colQueryPort.HeaderText = "Query Port";
			colQueryPort.Name = "colQueryPort";
			colQueryPort.ReadOnly = true;
			// 
			// colPassword
			// 
			colPassword.HeaderText = "Password";
			colPassword.Name = "colPassword";
			colPassword.ReadOnly = true;
			// 
			// colStatus
			// 
			colStatus.HeaderText = "Status";
			colStatus.Name = "colStatus";
			colStatus.ReadOnly = true;
			// 
			// btnDelete
			// 
			btnDelete.Location = new Point(308, 561);
			btnDelete.Name = "btnDelete";
			btnDelete.Size = new Size(142, 28);
			btnDelete.TabIndex = 2;
			btnDelete.Text = "Delete Server";
			btnDelete.UseVisualStyleBackColor = true;
			btnDelete.Click += btnDelete_Click;
			// 
			// btnAddServer
			// 
			btnAddServer.Location = new Point(12, 561);
			btnAddServer.Name = "btnAddServer";
			btnAddServer.Size = new Size(142, 28);
			btnAddServer.TabIndex = 4;
			btnAddServer.Text = "Add Server";
			btnAddServer.UseVisualStyleBackColor = true;
			btnAddServer.Click += btnAddServer_Click;
			// 
			// btnEdit
			// 
			btnEdit.Location = new Point(160, 561);
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
			rtbLog.Location = new Point(723, 42);
			rtbLog.Name = "rtbLog";
			rtbLog.ReadOnly = true;
			rtbLog.Size = new Size(398, 547);
			rtbLog.TabIndex = 6;
			rtbLog.Text = "";
			// 
			// ServerManagerLabel
			// 
			ServerManagerLabel.AutoSize = true;
			ServerManagerLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
			ServerManagerLabel.Location = new Point(12, 2);
			ServerManagerLabel.Name = "ServerManagerLabel";
			ServerManagerLabel.Size = new Size(196, 32);
			ServerManagerLabel.TabIndex = 7;
			ServerManagerLabel.Text = "Server Manager";
			// 
			// btnStart
			// 
			btnStart.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			btnStart.ForeColor = Color.Green;
			btnStart.Location = new Point(498, 561);
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
			btnStop.Location = new Point(607, 561);
			btnStop.Name = "btnStop";
			btnStop.Size = new Size(103, 28);
			btnStop.TabIndex = 9;
			btnStop.Text = "Stop Server";
			btnStop.UseVisualStyleBackColor = true;
			btnStop.Click += btnStop_Click;
			// 
			// MainGUI
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1133, 601);
			Controls.Add(btnStop);
			Controls.Add(btnStart);
			Controls.Add(ServerManagerLabel);
			Controls.Add(rtbLog);
			Controls.Add(btnEdit);
			Controls.Add(btnAddServer);
			Controls.Add(btnDelete);
			Controls.Add(dataGridView1);
			Name = "MainGUI";
			Text = "Game Server Control Panel";
			FormClosing += GUI_FormClosing;
			Shown += MainGUI_Shown;
			((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private DataGridView dataGridView1;
		private Button btnDelete;
		private Button btnAddServer;
		private Button btnEdit;
		private RichTextBox rtbLog;
		private DataGridViewTextBoxColumn colGame;
		private DataGridViewTextBoxColumn colName;
		private DataGridViewTextBoxColumn colPort;
		private DataGridViewTextBoxColumn colQueryPort;
		private DataGridViewTextBoxColumn colPassword;
		private DataGridViewTextBoxColumn colStatus;
		private Label ServerManagerLabel;
		private Button btnStart;
		private Button btnStop;
	}
}
