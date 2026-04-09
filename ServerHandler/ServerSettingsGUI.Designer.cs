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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerSettingsGUI));
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
			ltextLabel1 = new Label();
			ServerPasswordLabel = new Label();
			txtPassword = new TextBox();
			MaxPlayerLabel = new Label();
			numMaxPlayers = new NumericUpDown();
			MapLabel = new Label();
			TextLabel3 = new Label();
			txtExtraArgs = new TextBox();
			TextLabel7 = new Label();
			QueryPortLabel = new Label();
			numQueryPort = new NumericUpDown();
			WarningLabel = new Label();
			cmbWorldName = new ComboBox();
			AdminPasswordLabel = new Label();
			txtAdminPassword = new TextBox();
			cmbCompetitive = new ComboBox();
			lblCompetitive = new Label();
			txtInstallPath = new TextBox();
			textLabel2 = new Label();
			chkEnableRcon = new CheckBox();
			numRconPort = new NumericUpDown();
			txtRconPassword = new TextBox();
			TextLabel5 = new Label();
			TextLabel6 = new Label();
			TextLabel4 = new Label();
			chkEnableSchedule = new CheckBox();
			dtpRestartTime = new DateTimePicker();
			chkMon = new CheckBox();
			chkTue = new CheckBox();
			chkWed = new CheckBox();
			chkThu = new CheckBox();
			chkFri = new CheckBox();
			chkSat = new CheckBox();
			chkSun = new CheckBox();
			groupBox1 = new GroupBox();
			lblWorldSeed = new Label();
			txtWorldSeed = new TextBox();
			((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
			((System.ComponentModel.ISupportInitialize)numMaxPlayers).BeginInit();
			((System.ComponentModel.ISupportInitialize)numQueryPort).BeginInit();
			((System.ComponentModel.ISupportInitialize)numRconPort).BeginInit();
			groupBox1.SuspendLayout();
			SuspendLayout();
			// 
			// ServerNameLabel
			// 
			ServerNameLabel.AutoSize = true;
			ServerNameLabel.BackColor = Color.Transparent;
			ServerNameLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			ServerNameLabel.ForeColor = Color.White;
			ServerNameLabel.Location = new Point(12, 9);
			ServerNameLabel.Name = "ServerNameLabel";
			ServerNameLabel.Size = new Size(87, 17);
			ServerNameLabel.TabIndex = 0;
			ServerNameLabel.Text = "Server Name";
			// 
			// GameServerLabel
			// 
			GameServerLabel.AutoSize = true;
			GameServerLabel.BackColor = Color.Transparent;
			GameServerLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			GameServerLabel.ForeColor = Color.White;
			GameServerLabel.Location = new Point(12, 70);
			GameServerLabel.Name = "GameServerLabel";
			GameServerLabel.Size = new Size(86, 17);
			GameServerLabel.TabIndex = 1;
			GameServerLabel.Text = "Game Server";
			// 
			// PortLabel
			// 
			PortLabel.AutoSize = true;
			PortLabel.BackColor = Color.Transparent;
			PortLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			PortLabel.ForeColor = Color.White;
			PortLabel.Location = new Point(12, 135);
			PortLabel.Name = "PortLabel";
			PortLabel.Size = new Size(34, 17);
			PortLabel.TabIndex = 2;
			PortLabel.Text = "Port";
			// 
			// FolderPathLabel
			// 
			FolderPathLabel.AutoSize = true;
			FolderPathLabel.BackColor = Color.Transparent;
			FolderPathLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			FolderPathLabel.ForeColor = Color.White;
			FolderPathLabel.Location = new Point(12, 343);
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
			btnBrowse.Location = new Point(41, 445);
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
			chkDefaultPath.BackColor = Color.Transparent;
			chkDefaultPath.ForeColor = Color.White;
			chkDefaultPath.Location = new Point(12, 363);
			chkDefaultPath.Name = "chkDefaultPath";
			chkDefaultPath.Size = new Size(230, 19);
			chkDefaultPath.TabIndex = 10;
			chkDefaultPath.Text = "Use Default Location (C:\\GameServers)";
			chkDefaultPath.UseVisualStyleBackColor = false;
			chkDefaultPath.Click += chkDefaultPath_CheckedChanged;
			// 
			// ltextLabel1
			// 
			ltextLabel1.AutoSize = true;
			ltextLabel1.BackColor = Color.Transparent;
			ltextLabel1.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			ltextLabel1.ForeColor = Color.White;
			ltextLabel1.Location = new Point(122, 447);
			ltextLabel1.Name = "ltextLabel1";
			ltextLabel1.Size = new Size(73, 17);
			ltextLabel1.TabIndex = 11;
			ltextLabel1.Text = " a location";
			// 
			// ServerPasswordLabel
			// 
			ServerPasswordLabel.AutoSize = true;
			ServerPasswordLabel.BackColor = Color.Transparent;
			ServerPasswordLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			ServerPasswordLabel.ForeColor = Color.White;
			ServerPasswordLabel.Location = new Point(414, 9);
			ServerPasswordLabel.Name = "ServerPasswordLabel";
			ServerPasswordLabel.Size = new Size(109, 17);
			ServerPasswordLabel.TabIndex = 12;
			ServerPasswordLabel.Text = "Server Password";
			// 
			// txtPassword
			// 
			txtPassword.Location = new Point(414, 29);
			txtPassword.Name = "txtPassword";
			txtPassword.Size = new Size(368, 23);
			txtPassword.TabIndex = 13;
			// 
			// MaxPlayerLabel
			// 
			MaxPlayerLabel.AutoSize = true;
			MaxPlayerLabel.BackColor = Color.Transparent;
			MaxPlayerLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			MaxPlayerLabel.ForeColor = Color.White;
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
			MapLabel.BackColor = Color.Transparent;
			MapLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			MapLabel.ForeColor = Color.White;
			MapLabel.Location = new Point(229, 70);
			MapLabel.Name = "MapLabel";
			MapLabel.Size = new Size(35, 17);
			MapLabel.TabIndex = 16;
			MapLabel.Text = "Map";
			// 
			// TextLabel3
			// 
			TextLabel3.AutoSize = true;
			TextLabel3.BackColor = Color.Transparent;
			TextLabel3.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			TextLabel3.ForeColor = Color.White;
			TextLabel3.Location = new Point(415, 434);
			TextLabel3.Name = "TextLabel3";
			TextLabel3.Size = new Size(124, 17);
			TextLabel3.TabIndex = 18;
			TextLabel3.Text = "Launch Arguments";
			// 
			// txtExtraArgs
			// 
			txtExtraArgs.Location = new Point(415, 471);
			txtExtraArgs.Name = "txtExtraArgs";
			txtExtraArgs.Size = new Size(368, 23);
			txtExtraArgs.TabIndex = 19;
			// 
			// TextLabel7
			// 
			TextLabel7.AutoSize = true;
			TextLabel7.BackColor = Color.Transparent;
			TextLabel7.ForeColor = Color.White;
			TextLabel7.Location = new Point(415, 453);
			TextLabel7.Name = "TextLabel7";
			TextLabel7.Size = new Size(257, 15);
			TextLabel7.TabIndex = 20;
			TextLabel7.Text = "Example:  -log, -nosteamclient, or -forceupdate";
			// 
			// QueryPortLabel
			// 
			QueryPortLabel.AutoSize = true;
			QueryPortLabel.BackColor = Color.Transparent;
			QueryPortLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			QueryPortLabel.ForeColor = Color.White;
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
			WarningLabel.BackColor = Color.Transparent;
			WarningLabel.ForeColor = Color.Red;
			WarningLabel.Location = new Point(12, 385);
			WarningLabel.Name = "WarningLabel";
			WarningLabel.Size = new Size(368, 57);
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
			AdminPasswordLabel.BackColor = Color.Transparent;
			AdminPasswordLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			AdminPasswordLabel.ForeColor = Color.White;
			AdminPasswordLabel.Location = new Point(414, 70);
			AdminPasswordLabel.Name = "AdminPasswordLabel";
			AdminPasswordLabel.Size = new Size(154, 17);
			AdminPasswordLabel.TabIndex = 25;
			AdminPasswordLabel.Text = "Server Admin Password";
			// 
			// txtAdminPassword
			// 
			txtAdminPassword.Location = new Point(415, 90);
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
			// lblCompetitive
			// 
			lblCompetitive.AutoSize = true;
			lblCompetitive.BackColor = Color.Transparent;
			lblCompetitive.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblCompetitive.ForeColor = Color.White;
			lblCompetitive.Location = new Point(277, 135);
			lblCompetitive.Name = "lblCompetitive";
			lblCompetitive.Size = new Size(83, 17);
			lblCompetitive.TabIndex = 28;
			lblCompetitive.Text = "Competitive";
			// 
			// txtInstallPath
			// 
			txtInstallPath.Location = new Point(12, 471);
			txtInstallPath.Name = "txtInstallPath";
			txtInstallPath.Size = new Size(368, 23);
			txtInstallPath.TabIndex = 29;
			txtInstallPath.TextChanged += txtInstallPath_TextChanged;
			// 
			// textLabel2
			// 
			textLabel2.AutoSize = true;
			textLabel2.BackColor = Color.Transparent;
			textLabel2.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			textLabel2.ForeColor = Color.White;
			textLabel2.Location = new Point(23, 447);
			textLabel2.Name = "textLabel2";
			textLabel2.Size = new Size(23, 17);
			textLabel2.TabIndex = 30;
			textLabel2.Text = "Or";
			// 
			// chkEnableRcon
			// 
			chkEnableRcon.AutoSize = true;
			chkEnableRcon.BackColor = Color.Transparent;
			chkEnableRcon.ForeColor = Color.White;
			chkEnableRcon.Location = new Point(12, 274);
			chkEnableRcon.Name = "chkEnableRcon";
			chkEnableRcon.Size = new Size(105, 19);
			chkEnableRcon.TabIndex = 31;
			chkEnableRcon.Text = "Activate RCON";
			chkEnableRcon.UseVisualStyleBackColor = false;
			// 
			// numRconPort
			// 
			numRconPort.Location = new Point(12, 317);
			numRconPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
			numRconPort.Minimum = new decimal(new int[] { 1024, 0, 0, 0 });
			numRconPort.Name = "numRconPort";
			numRconPort.Size = new Size(63, 23);
			numRconPort.TabIndex = 32;
			numRconPort.Value = new decimal(new int[] { 1024, 0, 0, 0 });
			// 
			// txtRconPassword
			// 
			txtRconPassword.Location = new Point(107, 316);
			txtRconPassword.Name = "txtRconPassword";
			txtRconPassword.Size = new Size(223, 23);
			txtRconPassword.TabIndex = 33;
			// 
			// TextLabel5
			// 
			TextLabel5.AutoSize = true;
			TextLabel5.BackColor = Color.Transparent;
			TextLabel5.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			TextLabel5.ForeColor = Color.White;
			TextLabel5.Location = new Point(12, 296);
			TextLabel5.Name = "TextLabel5";
			TextLabel5.Size = new Size(74, 17);
			TextLabel5.TabIndex = 34;
			TextLabel5.Text = "RCON Port";
			// 
			// TextLabel6
			// 
			TextLabel6.AutoSize = true;
			TextLabel6.BackColor = Color.Transparent;
			TextLabel6.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			TextLabel6.ForeColor = Color.White;
			TextLabel6.Location = new Point(107, 296);
			TextLabel6.Name = "TextLabel6";
			TextLabel6.Size = new Size(106, 17);
			TextLabel6.TabIndex = 35;
			TextLabel6.Text = "RCON Password";
			// 
			// TextLabel4
			// 
			TextLabel4.AutoSize = true;
			TextLabel4.BackColor = Color.Transparent;
			TextLabel4.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			TextLabel4.ForeColor = Color.White;
			TextLabel4.Location = new Point(415, 135);
			TextLabel4.Name = "TextLabel4";
			TextLabel4.Size = new Size(237, 17);
			TextLabel4.TabIndex = 36;
			TextLabel4.Text = "Maintenance & Auto-Restart Scheduler";
			// 
			// chkEnableSchedule
			// 
			chkEnableSchedule.AutoSize = true;
			chkEnableSchedule.ForeColor = Color.White;
			chkEnableSchedule.Location = new Point(6, 18);
			chkEnableSchedule.Name = "chkEnableSchedule";
			chkEnableSchedule.Size = new Size(69, 19);
			chkEnableSchedule.TabIndex = 0;
			chkEnableSchedule.Text = "Activate";
			chkEnableSchedule.UseVisualStyleBackColor = true;
			chkEnableSchedule.CheckedChanged += chkEnableSchedule_CheckedChanged;
			// 
			// dtpRestartTime
			// 
			dtpRestartTime.CustomFormat = "HH:mm";
			dtpRestartTime.Format = DateTimePickerFormat.Custom;
			dtpRestartTime.Location = new Point(81, 13);
			dtpRestartTime.Name = "dtpRestartTime";
			dtpRestartTime.ShowUpDown = true;
			dtpRestartTime.Size = new Size(63, 23);
			dtpRestartTime.TabIndex = 37;
			// 
			// chkMon
			// 
			chkMon.AutoSize = true;
			chkMon.ForeColor = Color.White;
			chkMon.Location = new Point(6, 43);
			chkMon.Name = "chkMon";
			chkMon.Size = new Size(70, 19);
			chkMon.TabIndex = 38;
			chkMon.Text = "Monday";
			chkMon.UseVisualStyleBackColor = true;
			// 
			// chkTue
			// 
			chkTue.AutoSize = true;
			chkTue.ForeColor = Color.White;
			chkTue.Location = new Point(82, 43);
			chkTue.Name = "chkTue";
			chkTue.Size = new Size(70, 19);
			chkTue.TabIndex = 39;
			chkTue.Text = "Tuesday";
			chkTue.UseVisualStyleBackColor = true;
			// 
			// chkWed
			// 
			chkWed.AutoSize = true;
			chkWed.ForeColor = Color.White;
			chkWed.Location = new Point(158, 43);
			chkWed.Name = "chkWed";
			chkWed.Size = new Size(87, 19);
			chkWed.TabIndex = 40;
			chkWed.Text = "Wednesday";
			chkWed.UseVisualStyleBackColor = true;
			// 
			// chkThu
			// 
			chkThu.AutoSize = true;
			chkThu.ForeColor = Color.White;
			chkThu.Location = new Point(251, 43);
			chkThu.Name = "chkThu";
			chkThu.Size = new Size(75, 19);
			chkThu.TabIndex = 41;
			chkThu.Text = "Thursday";
			chkThu.UseVisualStyleBackColor = true;
			// 
			// chkFri
			// 
			chkFri.AutoSize = true;
			chkFri.ForeColor = Color.White;
			chkFri.Location = new Point(6, 68);
			chkFri.Name = "chkFri";
			chkFri.Size = new Size(58, 19);
			chkFri.TabIndex = 42;
			chkFri.Text = "Friday";
			chkFri.UseVisualStyleBackColor = true;
			// 
			// chkSat
			// 
			chkSat.AutoSize = true;
			chkSat.ForeColor = Color.White;
			chkSat.Location = new Point(82, 68);
			chkSat.Name = "chkSat";
			chkSat.Size = new Size(72, 19);
			chkSat.TabIndex = 43;
			chkSat.Text = "Saturday";
			chkSat.UseVisualStyleBackColor = true;
			// 
			// chkSun
			// 
			chkSun.AutoSize = true;
			chkSun.ForeColor = Color.White;
			chkSun.Location = new Point(158, 68);
			chkSun.Name = "chkSun";
			chkSun.Size = new Size(65, 19);
			chkSun.TabIndex = 44;
			chkSun.Text = "Sunday";
			chkSun.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			groupBox1.BackColor = Color.Transparent;
			groupBox1.BackgroundImageLayout = ImageLayout.None;
			groupBox1.Controls.Add(chkFri);
			groupBox1.Controls.Add(dtpRestartTime);
			groupBox1.Controls.Add(chkThu);
			groupBox1.Controls.Add(chkEnableSchedule);
			groupBox1.Controls.Add(chkSun);
			groupBox1.Controls.Add(chkWed);
			groupBox1.Controls.Add(chkSat);
			groupBox1.Controls.Add(chkTue);
			groupBox1.Controls.Add(chkMon);
			groupBox1.Location = new Point(415, 151);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new Size(351, 105);
			groupBox1.TabIndex = 45;
			groupBox1.TabStop = false;
			// 
			// lblWorldSeed
			// 
			lblWorldSeed.AutoSize = true;
			lblWorldSeed.BackColor = Color.Transparent;
			lblWorldSeed.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblWorldSeed.ForeColor = Color.White;
			lblWorldSeed.Location = new Point(12, 194);
			lblWorldSeed.Name = "lblWorldSeed";
			lblWorldSeed.Size = new Size(79, 17);
			lblWorldSeed.TabIndex = 46;
			lblWorldSeed.Text = "World Seed";
			// 
			// txtWorldSeed
			// 
			txtWorldSeed.Location = new Point(12, 214);
			txtWorldSeed.Name = "txtWorldSeed";
			txtWorldSeed.Size = new Size(142, 23);
			txtWorldSeed.TabIndex = 47;
			txtWorldSeed.KeyPress += txtWorldSeed_KeyPress;
			// 
			// ServerSettingsGUI
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackgroundImage = Properties.Resources.background;
			BackgroundImageLayout = ImageLayout.Stretch;
			ClientSize = new Size(795, 558);
			Controls.Add(txtWorldSeed);
			Controls.Add(lblWorldSeed);
			Controls.Add(groupBox1);
			Controls.Add(TextLabel4);
			Controls.Add(TextLabel6);
			Controls.Add(TextLabel5);
			Controls.Add(txtRconPassword);
			Controls.Add(numRconPort);
			Controls.Add(chkEnableRcon);
			Controls.Add(textLabel2);
			Controls.Add(txtInstallPath);
			Controls.Add(lblCompetitive);
			Controls.Add(cmbCompetitive);
			Controls.Add(txtAdminPassword);
			Controls.Add(AdminPasswordLabel);
			Controls.Add(cmbWorldName);
			Controls.Add(WarningLabel);
			Controls.Add(numQueryPort);
			Controls.Add(QueryPortLabel);
			Controls.Add(TextLabel7);
			Controls.Add(txtExtraArgs);
			Controls.Add(TextLabel3);
			Controls.Add(MapLabel);
			Controls.Add(numMaxPlayers);
			Controls.Add(MaxPlayerLabel);
			Controls.Add(txtPassword);
			Controls.Add(ServerPasswordLabel);
			Controls.Add(ltextLabel1);
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
			FormBorderStyle = FormBorderStyle.FixedSingle;
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "ServerSettingsGUI";
			Text = "Server Settings";
			((System.ComponentModel.ISupportInitialize)numPort).EndInit();
			((System.ComponentModel.ISupportInitialize)numMaxPlayers).EndInit();
			((System.ComponentModel.ISupportInitialize)numQueryPort).EndInit();
			((System.ComponentModel.ISupportInitialize)numRconPort).EndInit();
			groupBox1.ResumeLayout(false);
			groupBox1.PerformLayout();
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
		private Label ltextLabel1;
		private Label ServerPasswordLabel;
		private TextBox txtPassword;
		private Label MaxPlayerLabel;
		private NumericUpDown numMaxPlayers;
		private Label MapLabel;
		private Label TextLabel3;
		private TextBox txtExtraArgs;
		private Label TextLabel7;
		private Label QueryPortLabel;
		private NumericUpDown numQueryPort;
		private Label WarningLabel;
		private ComboBox cmbWorldName;
		private Label AdminPasswordLabel;
		private TextBox txtAdminPassword;
		private ComboBox cmbCompetitive;
		private Label lblCompetitive;
		private TextBox txtInstallPath;
		private Label textLabel2;
		private CheckBox chkEnableRcon;
		private NumericUpDown numRconPort;
		private TextBox txtRconPassword;
		private Label TextLabel5;
		private Label TextLabel6;
		private Label TextLabel4;
		private CheckBox chkEnableSchedule;
		private DateTimePicker dtpRestartTime;
		private CheckBox chkMon;
		private CheckBox chkTue;
		private CheckBox chkWed;
		private CheckBox chkThu;
		private CheckBox chkFri;
		private CheckBox chkSat;
		private CheckBox chkSun;
		private GroupBox groupBox1;
		private Label lblWorldSeed;
		private TextBox txtWorldSeed;
	}
}