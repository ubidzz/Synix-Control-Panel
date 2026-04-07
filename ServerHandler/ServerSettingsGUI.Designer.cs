namespace Synix_Control_Panel
{
	partial class ServerSettingsGUI
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
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
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ServerNameLabel = new Label();
			GameServerLabel = new Label();
			PortLabel = new Label();
			FolderPathLabel = new Label();
			txtName = new TextBox();
			cmbGame = new ComboBox();
			numPort = new NumericUpDown();
			btnBrowse = new Button();
			btnSave = new Button();
			chkDefaultPath = new CheckBox();
			label1 = new Label();
			ServerPasswordLabel = new Label();
			txtPassword = new TextBox();
			MaxPlayerLabel = new Label();
			numMaxPlayers = new NumericUpDown();
			MapLabel = new Label();
			label3 = new Label();
			txtExtraArgs = new TextBox();
			label4 = new Label();
			QueryPortLabel = new Label();
			numQueryPort = new NumericUpDown();
			WarningLabel = new Label();
			cmbWorldName = new ComboBox();
			AdminPasswordLabel = new Label();
			txtAdminPassword = new TextBox();
			cmbCompetitive = new ComboBox();
			CompetitiveLabel = new Label();
			txtInstallPath = new TextBox();
			label5 = new Label();
			chkEnableRcon = new CheckBox();
			numRconPort = new NumericUpDown();
			txtRconPassword = new TextBox();
			label2 = new Label();
			label6 = new Label();
			((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
			((System.ComponentModel.ISupportInitialize)numMaxPlayers).BeginInit();
			((System.ComponentModel.ISupportInitialize)numQueryPort).BeginInit();
			((System.ComponentModel.ISupportInitialize)numRconPort).BeginInit();
			SuspendLayout();
			// 
			// ServerNameLabel
			// 
			ServerNameLabel.AutoSize = true;
			ServerNameLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			ServerNameLabel.Location = new Point(12, 9);
			ServerNameLabel.Name = "ServerNameLabel";
			ServerNameLabel.Size = new Size(87, 17);
			ServerNameLabel.TabIndex = 0;
			ServerNameLabel.Text = "Server Name";
			// 
			// GameServerLabel
			// 
			GameServerLabel.AutoSize = true;
			GameServerLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			GameServerLabel.Location = new Point(12, 70);
			GameServerLabel.Name = "GameServerLabel";
			GameServerLabel.Size = new Size(86, 17);
			GameServerLabel.TabIndex = 1;
			GameServerLabel.Text = "Game Server";
			// 
			// PortLabel
			// 
			PortLabel.AutoSize = true;
			PortLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			PortLabel.Location = new Point(12, 135);
			PortLabel.Name = "PortLabel";
			PortLabel.Size = new Size(34, 17);
			PortLabel.TabIndex = 2;
			PortLabel.Text = "Port";
			// 
			// FolderPathLabel
			// 
			FolderPathLabel.AutoSize = true;
			FolderPathLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			FolderPathLabel.Location = new Point(414, 9);
			FolderPathLabel.Name = "FolderPathLabel";
			FolderPathLabel.Size = new Size(104, 17);
			FolderPathLabel.TabIndex = 3;
			FolderPathLabel.Text = "Folder Location";
			// 
			// txtName
			// 
			txtName.Location = new Point(12, 29);
			txtName.Name = "txtName";
			txtName.Size = new Size(368, 23);
			txtName.TabIndex = 4;
			txtName.TextChanged += txtName_TextChanged;
			// 
			// cmbGame
			// 
			cmbGame.FormattingEnabled = true;
			cmbGame.Items.AddRange(new object[] { "Game List" });
			cmbGame.Location = new Point(12, 90);
			cmbGame.Name = "cmbGame";
			cmbGame.Size = new Size(211, 23);
			cmbGame.TabIndex = 5;
			cmbGame.Text = "Pick Game";
			cmbGame.SelectedIndexChanged += cmbGame_SelectedIndexChanged;
			// 
			// numPort
			// 
			numPort.Location = new Point(12, 155);
			numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
			numPort.Minimum = new decimal(new int[] { 1024, 0, 0, 0 });
			numPort.Name = "numPort";
			numPort.Size = new Size(63, 23);
			numPort.TabIndex = 6;
			numPort.Value = new decimal(new int[] { 1024, 0, 0, 0 });
			// 
			// btnBrowse
			// 
			btnBrowse.Location = new Point(434, 145);
			btnBrowse.Name = "btnBrowse";
			btnBrowse.Size = new Size(75, 23);
			btnBrowse.TabIndex = 8;
			btnBrowse.Text = "Browse";
			btnBrowse.UseVisualStyleBackColor = true;
			btnBrowse.Click += btnBrowse_Click;
			// 
			// btnSave
			// 
			btnSave.Cursor = Cursors.Hand;
			btnSave.Location = new Point(328, 500);
			btnSave.Name = "btnSave";
			btnSave.Size = new Size(138, 44);
			btnSave.TabIndex = 9;
			btnSave.Text = "Save Server";
			btnSave.UseVisualStyleBackColor = true;
			btnSave.Click += btnSave_Click;
			// 
			// chkDefaultPath
			// 
			chkDefaultPath.AutoSize = true;
			chkDefaultPath.Location = new Point(414, 33);
			chkDefaultPath.Name = "chkDefaultPath";
			chkDefaultPath.Size = new Size(230, 19);
			chkDefaultPath.TabIndex = 10;
			chkDefaultPath.Text = "Use Default Location (C:\\GameServers)";
			chkDefaultPath.UseVisualStyleBackColor = true;
			chkDefaultPath.CheckedChanged += chkDefaultPath_CheckedChanged;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			label1.Location = new Point(506, 147);
			label1.Name = "label1";
			label1.Size = new Size(73, 17);
			label1.TabIndex = 11;
			label1.Text = " a location";
			// 
			// ServerPasswordLabel
			// 
			ServerPasswordLabel.AutoSize = true;
			ServerPasswordLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			ServerPasswordLabel.Location = new Point(414, 213);
			ServerPasswordLabel.Name = "ServerPasswordLabel";
			ServerPasswordLabel.Size = new Size(109, 17);
			ServerPasswordLabel.TabIndex = 12;
			ServerPasswordLabel.Text = "Server Password";
			// 
			// txtPassword
			// 
			txtPassword.Location = new Point(414, 233);
			txtPassword.Name = "txtPassword";
			txtPassword.Size = new Size(368, 23);
			txtPassword.TabIndex = 13;
			// 
			// MaxPlayerLabel
			// 
			MaxPlayerLabel.AutoSize = true;
			MaxPlayerLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			MaxPlayerLabel.Location = new Point(178, 135);
			MaxPlayerLabel.Name = "MaxPlayerLabel";
			MaxPlayerLabel.Size = new Size(76, 17);
			MaxPlayerLabel.TabIndex = 14;
			MaxPlayerLabel.Text = "Max Player";
			// 
			// numMaxPlayers
			// 
			numMaxPlayers.Location = new Point(178, 155);
			numMaxPlayers.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
			numMaxPlayers.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			numMaxPlayers.Name = "numMaxPlayers";
			numMaxPlayers.Size = new Size(76, 23);
			numMaxPlayers.TabIndex = 15;
			numMaxPlayers.Value = new decimal(new int[] { 10, 0, 0, 0 });
			// 
			// MapLabel
			// 
			MapLabel.AutoSize = true;
			MapLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			MapLabel.Location = new Point(229, 70);
			MapLabel.Name = "MapLabel";
			MapLabel.Size = new Size(35, 17);
			MapLabel.TabIndex = 16;
			MapLabel.Text = "Map";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			label3.Location = new Point(12, 410);
			label3.Name = "label3";
			label3.Size = new Size(124, 17);
			label3.TabIndex = 18;
			label3.Text = "Launch Arguments";
			// 
			// txtExtraArgs
			// 
			txtExtraArgs.Location = new Point(12, 445);
			txtExtraArgs.Name = "txtExtraArgs";
			txtExtraArgs.Size = new Size(368, 23);
			txtExtraArgs.TabIndex = 19;
			// 
			// label4
			// 
			label4.AutoSize = true;
			label4.Location = new Point(12, 427);
			label4.Name = "label4";
			label4.Size = new Size(257, 15);
			label4.TabIndex = 20;
			label4.Text = "Example:  -log, -nosteamclient, or -forceupdate";
			// 
			// QueryPortLabel
			// 
			QueryPortLabel.AutoSize = true;
			QueryPortLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			QueryPortLabel.Location = new Point(91, 135);
			QueryPortLabel.Name = "QueryPortLabel";
			QueryPortLabel.Size = new Size(76, 17);
			QueryPortLabel.TabIndex = 21;
			QueryPortLabel.Text = "Query Port";
			// 
			// numQueryPort
			// 
			numQueryPort.Location = new Point(91, 155);
			numQueryPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
			numQueryPort.Minimum = new decimal(new int[] { 1024, 0, 0, 0 });
			numQueryPort.Name = "numQueryPort";
			numQueryPort.Size = new Size(63, 23);
			numQueryPort.TabIndex = 22;
			numQueryPort.Value = new decimal(new int[] { 27015, 0, 0, 0 });
			// 
			// WarningLabel
			// 
			WarningLabel.AutoEllipsis = true;
			WarningLabel.ForeColor = Color.Red;
			WarningLabel.Location = new Point(414, 55);
			WarningLabel.Name = "WarningLabel";
			WarningLabel.Size = new Size(368, 86);
			WarningLabel.TabIndex = 23;
			// 
			// cmbWorldName
			// 
			cmbWorldName.FormattingEnabled = true;
			cmbWorldName.Location = new Point(229, 90);
			cmbWorldName.Name = "cmbWorldName";
			cmbWorldName.Size = new Size(151, 23);
			cmbWorldName.TabIndex = 24;
			// 
			// AdminPasswordLabel
			// 
			AdminPasswordLabel.AutoSize = true;
			AdminPasswordLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			AdminPasswordLabel.Location = new Point(415, 269);
			AdminPasswordLabel.Name = "AdminPasswordLabel";
			AdminPasswordLabel.Size = new Size(154, 17);
			AdminPasswordLabel.TabIndex = 25;
			AdminPasswordLabel.Text = "Server Admin Password";
			// 
			// txtAdminPassword
			// 
			txtAdminPassword.Location = new Point(415, 289);
			txtAdminPassword.Name = "txtAdminPassword";
			txtAdminPassword.Size = new Size(368, 23);
			txtAdminPassword.TabIndex = 26;
			// 
			// cmbCompetitive
			// 
			cmbCompetitive.FormattingEnabled = true;
			cmbCompetitive.Location = new Point(277, 155);
			cmbCompetitive.Name = "cmbCompetitive";
			cmbCompetitive.Size = new Size(103, 23);
			cmbCompetitive.TabIndex = 27;
			// 
			// CompetitiveLabel
			// 
			CompetitiveLabel.AutoSize = true;
			CompetitiveLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			CompetitiveLabel.Location = new Point(277, 135);
			CompetitiveLabel.Name = "CompetitiveLabel";
			CompetitiveLabel.Size = new Size(83, 17);
			CompetitiveLabel.TabIndex = 28;
			CompetitiveLabel.Text = "Competitive";
			// 
			// txtInstallPath
			// 
			txtInstallPath.Location = new Point(414, 174);
			txtInstallPath.Name = "txtInstallPath";
			txtInstallPath.Size = new Size(368, 23);
			txtInstallPath.TabIndex = 29;
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			label5.Location = new Point(414, 147);
			label5.Name = "label5";
			label5.Size = new Size(23, 17);
			label5.TabIndex = 30;
			label5.Text = "Or";
			// 
			// chkEnableRcon
			// 
			chkEnableRcon.AutoSize = true;
			chkEnableRcon.Location = new Point(12, 191);
			chkEnableRcon.Name = "chkEnableRcon";
			chkEnableRcon.Size = new Size(105, 19);
			chkEnableRcon.TabIndex = 31;
			chkEnableRcon.Text = "Activate RCON";
			chkEnableRcon.UseVisualStyleBackColor = true;
			// 
			// numRconPort
			// 
			numRconPort.Location = new Point(12, 233);
			numRconPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
			numRconPort.Minimum = new decimal(new int[] { 1024, 0, 0, 0 });
			numRconPort.Name = "numRconPort";
			numRconPort.Size = new Size(63, 23);
			numRconPort.TabIndex = 32;
			numRconPort.Value = new decimal(new int[] { 1024, 0, 0, 0 });
			// 
			// txtRconPassword
			// 
			txtRconPassword.Location = new Point(107, 233);
			txtRconPassword.Name = "txtRconPassword";
			txtRconPassword.Size = new Size(223, 23);
			txtRconPassword.TabIndex = 33;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			label2.Location = new Point(12, 213);
			label2.Name = "label2";
			label2.Size = new Size(74, 17);
			label2.TabIndex = 34;
			label2.Text = "RCON Port";
			// 
			// label6
			// 
			label6.AutoSize = true;
			label6.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			label6.Location = new Point(107, 213);
			label6.Name = "label6";
			label6.Size = new Size(106, 17);
			label6.TabIndex = 35;
			label6.Text = "RCON Password";
			// 
			// ServerSettingsGUI
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(795, 558);
			Controls.Add(label6);
			Controls.Add(label2);
			Controls.Add(txtRconPassword);
			Controls.Add(numRconPort);
			Controls.Add(chkEnableRcon);
			Controls.Add(label5);
			Controls.Add(txtInstallPath);
			Controls.Add(CompetitiveLabel);
			Controls.Add(cmbCompetitive);
			Controls.Add(txtAdminPassword);
			Controls.Add(AdminPasswordLabel);
			Controls.Add(cmbWorldName);
			Controls.Add(WarningLabel);
			Controls.Add(numQueryPort);
			Controls.Add(QueryPortLabel);
			Controls.Add(label4);
			Controls.Add(txtExtraArgs);
			Controls.Add(label3);
			Controls.Add(MapLabel);
			Controls.Add(numMaxPlayers);
			Controls.Add(MaxPlayerLabel);
			Controls.Add(txtPassword);
			Controls.Add(ServerPasswordLabel);
			Controls.Add(label1);
			Controls.Add(chkDefaultPath);
			Controls.Add(btnSave);
			Controls.Add(btnBrowse);
			Controls.Add(numPort);
			Controls.Add(cmbGame);
			Controls.Add(txtName);
			Controls.Add(FolderPathLabel);
			Controls.Add(PortLabel);
			Controls.Add(GameServerLabel);
			Controls.Add(ServerNameLabel);
			Name = "ServerSettingsGUI";
			Text = "ServerSettingsForm";
			Load += ServerSettingsGUI_Load;
			((System.ComponentModel.ISupportInitialize)numPort).EndInit();
			((System.ComponentModel.ISupportInitialize)numMaxPlayers).EndInit();
			((System.ComponentModel.ISupportInitialize)numQueryPort).EndInit();
			((System.ComponentModel.ISupportInitialize)numRconPort).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Label ServerNameLabel;
		private Label GameServerLabel;
		private Label PortLabel;
		private Label FolderPathLabel;
		private TextBox txtName;
		private ComboBox cmbGame;
		private NumericUpDown numPort;
		private Button btnBrowse;
		private Button btnSave;
		private CheckBox chkDefaultPath;
		private Label label1;
		private Label ServerPasswordLabel;
		private TextBox txtPassword;
		private Label MaxPlayerLabel;
		private NumericUpDown numMaxPlayers;
		private Label MapLabel;
		private Label label3;
		private TextBox txtExtraArgs;
		private Label label4;
		private Label QueryPortLabel;
		private NumericUpDown numQueryPort;
		private Label WarningLabel;
		private ComboBox cmbWorldName;
		private Label AdminPasswordLabel;
		private TextBox txtAdminPassword;
		private ComboBox cmbCompetitive;
		private Label CompetitiveLabel;
		private TextBox txtInstallPath;
		private Label label5;
		private CheckBox chkEnableRcon;
		private NumericUpDown numRconPort;
		private TextBox txtRconPassword;
		private Label label2;
		private Label label6;
	}
}