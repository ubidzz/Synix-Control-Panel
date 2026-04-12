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
			lblPassword = new Label();
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
			lblAdminPassword = new Label();
			txtAdminPassword = new TextBox();
			cmbCompetitive = new ComboBox();
			lblCompetitive = new Label();
			txtInstallPath = new TextBox();
			textLabel2 = new Label();
			chkEnableRcon = new CheckBox();
			numRconPort = new NumericUpDown();
			txtRconPassword = new TextBox();
			lblRCONpassword = new Label();
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
			lblRCONport = new Label();
			lblaruments = new Label();
			lblAppPort = new Label();
			AppPortNumeric = new NumericUpDown();
			btnViewArgs = new Button();
			chkUpdateOnStart = new CheckBox();
			((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
			((System.ComponentModel.ISupportInitialize)numMaxPlayers).BeginInit();
			((System.ComponentModel.ISupportInitialize)numQueryPort).BeginInit();
			((System.ComponentModel.ISupportInitialize)numRconPort).BeginInit();
			groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)AppPortNumeric).BeginInit();
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
			PortLabel.Location = new Point(94, 205);
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
			cmbGame.Size = new Size(273, 23);
			cmbGame.TabIndex = 5;
			cmbGame.Text = "Pick Game";
			cmbGame.SelectedIndexChanged += cmbGame_SelectedIndexChanged;
			// 
			// numPort
			// 
			numPort.Location = new Point(94, 225);
			numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
			numPort.Minimum = new decimal(new int[] { 1024, 0, 0, 0 });
			numPort.Name = "numPort";
			numPort.Size = new Size(63, 23);
			numPort.TabIndex = 6;
			numPort.Value = new decimal(new int[] { 1024, 0, 0, 0 });
			// 
			// btnBrowse
			// 
			btnBrowse.Location = new Point(47, 445);
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
			chkDefaultPath.Appearance = Appearance.Button;
			chkDefaultPath.BackColor = Color.Transparent;
			chkDefaultPath.FlatStyle = FlatStyle.Flat;
			chkDefaultPath.ForeColor = Color.White;
			chkDefaultPath.Location = new Point(12, 363);
			chkDefaultPath.Name = "chkDefaultPath";
			chkDefaultPath.Size = new Size(193, 29);
			chkDefaultPath.TabIndex = 10;
			chkDefaultPath.Text = "Use Default Location (C:\\Game)";
			chkDefaultPath.TextAlign = ContentAlignment.MiddleCenter;
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
			// lblPassword
			// 
			lblPassword.AutoSize = true;
			lblPassword.BackColor = Color.Transparent;
			lblPassword.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblPassword.ForeColor = Color.White;
			lblPassword.Location = new Point(414, 9);
			lblPassword.Name = "lblPassword";
			lblPassword.Size = new Size(109, 17);
			lblPassword.TabIndex = 12;
			lblPassword.Text = "Server Password";
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
			MaxPlayerLabel.Location = new Point(12, 205);
			MaxPlayerLabel.Name = "MaxPlayerLabel";
			MaxPlayerLabel.Size = new Size(76, 17);
			MaxPlayerLabel.TabIndex = 14;
			MaxPlayerLabel.Text = "Max Player";
			// 
			// numMaxPlayers
			// 
			numMaxPlayers.Location = new Point(12, 225);
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
			MapLabel.Location = new Point(12, 135);
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
			TextLabel3.Location = new Point(415, 359);
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
			QueryPortLabel.Location = new Point(163, 205);
			QueryPortLabel.Name = "QueryPortLabel";
			QueryPortLabel.Size = new Size(76, 17);
			QueryPortLabel.TabIndex = 21;
			QueryPortLabel.Text = "Query Port";
			// 
			// numQueryPort
			// 
			numQueryPort.Location = new Point(163, 225);
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
			WarningLabel.Location = new Point(12, 395);
			WarningLabel.Name = "WarningLabel";
			WarningLabel.Size = new Size(368, 47);
			WarningLabel.TabIndex = 23;
			// 
			// cmbWorldName
			// 
			cmbWorldName.FormattingEnabled = true;
			cmbWorldName.Location = new Point(12, 155);
			cmbWorldName.Name = "cmbWorldName";
			cmbWorldName.Size = new Size(186, 23);
			cmbWorldName.TabIndex = 24;
			// 
			// lblAdminPassword
			// 
			lblAdminPassword.AutoSize = true;
			lblAdminPassword.BackColor = Color.Transparent;
			lblAdminPassword.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblAdminPassword.ForeColor = Color.White;
			lblAdminPassword.Location = new Point(414, 70);
			lblAdminPassword.Name = "lblAdminPassword";
			lblAdminPassword.Size = new Size(154, 17);
			lblAdminPassword.TabIndex = 25;
			lblAdminPassword.Text = "Server Admin Password";
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
			cmbCompetitive.Location = new Point(291, 90);
			cmbCompetitive.Name = "cmbCompetitive";
			cmbCompetitive.Size = new Size(89, 23);
			cmbCompetitive.TabIndex = 27;
			// 
			// lblCompetitive
			// 
			lblCompetitive.AutoSize = true;
			lblCompetitive.BackColor = Color.Transparent;
			lblCompetitive.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblCompetitive.ForeColor = Color.White;
			lblCompetitive.Location = new Point(291, 70);
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
			chkEnableRcon.Appearance = Appearance.Button;
			chkEnableRcon.BackColor = Color.White;
			chkEnableRcon.FlatStyle = FlatStyle.Flat;
			chkEnableRcon.ForeColor = Color.Black;
			chkEnableRcon.Location = new Point(12, 267);
			chkEnableRcon.Name = "chkEnableRcon";
			chkEnableRcon.Size = new Size(105, 26);
			chkEnableRcon.TabIndex = 31;
			chkEnableRcon.Text = "Activate RCON";
			chkEnableRcon.TextAlign = ContentAlignment.MiddleCenter;
			chkEnableRcon.UseVisualStyleBackColor = false;
			chkEnableRcon.CheckedChanged += chkEnableRcon_CheckedChanged;
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
			// lblRCONpassword
			// 
			lblRCONpassword.AutoSize = true;
			lblRCONpassword.BackColor = Color.Transparent;
			lblRCONpassword.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblRCONpassword.ForeColor = Color.White;
			lblRCONpassword.Location = new Point(107, 296);
			lblRCONpassword.Name = "lblRCONpassword";
			lblRCONpassword.Size = new Size(106, 17);
			lblRCONpassword.TabIndex = 35;
			lblRCONpassword.Text = "RCON Password";
			// 
			// TextLabel4
			// 
			TextLabel4.AutoSize = true;
			TextLabel4.BackColor = Color.Transparent;
			TextLabel4.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			TextLabel4.ForeColor = Color.White;
			TextLabel4.Location = new Point(414, 135);
			TextLabel4.Name = "TextLabel4";
			TextLabel4.Size = new Size(237, 17);
			TextLabel4.TabIndex = 36;
			TextLabel4.Text = "Maintenance & Auto-Restart Scheduler";
			// 
			// chkEnableSchedule
			// 
			chkEnableSchedule.Appearance = Appearance.Button;
			chkEnableSchedule.FlatStyle = FlatStyle.Flat;
			chkEnableSchedule.ForeColor = Color.White;
			chkEnableSchedule.Location = new Point(6, 18);
			chkEnableSchedule.Name = "chkEnableSchedule";
			chkEnableSchedule.Size = new Size(69, 26);
			chkEnableSchedule.TabIndex = 0;
			chkEnableSchedule.Text = "Activate";
			chkEnableSchedule.TextAlign = ContentAlignment.MiddleCenter;
			chkEnableSchedule.UseVisualStyleBackColor = true;
			chkEnableSchedule.CheckedChanged += chkEnableSchedule_CheckedChanged;
			// 
			// dtpRestartTime
			// 
			dtpRestartTime.CustomFormat = "HH:mm";
			dtpRestartTime.Format = DateTimePickerFormat.Custom;
			dtpRestartTime.Location = new Point(82, 18);
			dtpRestartTime.Name = "dtpRestartTime";
			dtpRestartTime.ShowUpDown = true;
			dtpRestartTime.Size = new Size(63, 23);
			dtpRestartTime.TabIndex = 37;
			// 
			// chkMon
			// 
			chkMon.Appearance = Appearance.Button;
			chkMon.FlatStyle = FlatStyle.Flat;
			chkMon.ForeColor = Color.White;
			chkMon.Location = new Point(5, 52);
			chkMon.Name = "chkMon";
			chkMon.Size = new Size(70, 25);
			chkMon.TabIndex = 38;
			chkMon.Text = "Monday";
			chkMon.TextAlign = ContentAlignment.MiddleCenter;
			chkMon.UseVisualStyleBackColor = true;
			// 
			// chkTue
			// 
			chkTue.Appearance = Appearance.Button;
			chkTue.FlatStyle = FlatStyle.Flat;
			chkTue.ForeColor = Color.White;
			chkTue.Location = new Point(82, 52);
			chkTue.Name = "chkTue";
			chkTue.Size = new Size(70, 25);
			chkTue.TabIndex = 39;
			chkTue.Text = "Tuesday";
			chkTue.TextAlign = ContentAlignment.MiddleCenter;
			chkTue.UseVisualStyleBackColor = true;
			// 
			// chkWed
			// 
			chkWed.Appearance = Appearance.Button;
			chkWed.AutoCheck = false;
			chkWed.AutoSize = true;
			chkWed.FlatStyle = FlatStyle.Flat;
			chkWed.ForeColor = Color.White;
			chkWed.Location = new Point(159, 52);
			chkWed.Name = "chkWed";
			chkWed.Size = new Size(78, 25);
			chkWed.TabIndex = 40;
			chkWed.Text = "Wednesday";
			chkWed.TextAlign = ContentAlignment.MiddleCenter;
			chkWed.UseVisualStyleBackColor = true;
			// 
			// chkThu
			// 
			chkThu.Appearance = Appearance.Button;
			chkThu.FlatStyle = FlatStyle.Flat;
			chkThu.ForeColor = Color.White;
			chkThu.Location = new Point(243, 52);
			chkThu.Name = "chkThu";
			chkThu.Size = new Size(75, 25);
			chkThu.TabIndex = 41;
			chkThu.Text = "Thursday";
			chkThu.TextAlign = ContentAlignment.MiddleCenter;
			chkThu.UseVisualStyleBackColor = true;
			// 
			// chkFri
			// 
			chkFri.Appearance = Appearance.Button;
			chkFri.FlatStyle = FlatStyle.Flat;
			chkFri.ForeColor = Color.White;
			chkFri.Location = new Point(6, 90);
			chkFri.Name = "chkFri";
			chkFri.Size = new Size(58, 24);
			chkFri.TabIndex = 42;
			chkFri.Text = "Friday";
			chkFri.TextAlign = ContentAlignment.MiddleCenter;
			chkFri.UseVisualStyleBackColor = true;
			// 
			// chkSat
			// 
			chkSat.Appearance = Appearance.Button;
			chkSat.FlatStyle = FlatStyle.Flat;
			chkSat.ForeColor = Color.White;
			chkSat.Location = new Point(70, 90);
			chkSat.Name = "chkSat";
			chkSat.Size = new Size(72, 24);
			chkSat.TabIndex = 43;
			chkSat.Text = "Saturday";
			chkSat.TextAlign = ContentAlignment.MiddleCenter;
			chkSat.UseVisualStyleBackColor = true;
			// 
			// chkSun
			// 
			chkSun.Appearance = Appearance.Button;
			chkSun.FlatStyle = FlatStyle.Flat;
			chkSun.ForeColor = Color.White;
			chkSun.Location = new Point(148, 90);
			chkSun.Name = "chkSun";
			chkSun.Size = new Size(65, 24);
			chkSun.TabIndex = 44;
			chkSun.Text = "Sunday";
			chkSun.TextAlign = ContentAlignment.MiddleCenter;
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
			groupBox1.Location = new Point(414, 155);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new Size(351, 128);
			groupBox1.TabIndex = 45;
			groupBox1.TabStop = false;
			// 
			// lblWorldSeed
			// 
			lblWorldSeed.AutoSize = true;
			lblWorldSeed.BackColor = Color.Transparent;
			lblWorldSeed.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblWorldSeed.ForeColor = Color.White;
			lblWorldSeed.Location = new Point(204, 135);
			lblWorldSeed.Name = "lblWorldSeed";
			lblWorldSeed.Size = new Size(79, 17);
			lblWorldSeed.TabIndex = 46;
			lblWorldSeed.Text = "World Seed";
			// 
			// txtWorldSeed
			// 
			txtWorldSeed.Location = new Point(204, 155);
			txtWorldSeed.Name = "txtWorldSeed";
			txtWorldSeed.Size = new Size(176, 23);
			txtWorldSeed.TabIndex = 47;
			txtWorldSeed.KeyPress += txtWorldSeed_KeyPress;
			// 
			// lblRCONport
			// 
			lblRCONport.AutoSize = true;
			lblRCONport.BackColor = Color.Transparent;
			lblRCONport.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblRCONport.ForeColor = Color.White;
			lblRCONport.Location = new Point(12, 296);
			lblRCONport.Name = "lblRCONport";
			lblRCONport.Size = new Size(74, 17);
			lblRCONport.TabIndex = 48;
			lblRCONport.Text = "RCON Port";
			// 
			// lblaruments
			// 
			lblaruments.AutoEllipsis = true;
			lblaruments.BackColor = Color.Transparent;
			lblaruments.ForeColor = Color.White;
			lblaruments.Location = new Point(414, 376);
			lblaruments.Name = "lblaruments";
			lblaruments.Size = new Size(368, 66);
			lblaruments.TabIndex = 52;
			lblaruments.Text = resources.GetString("lblaruments.Text");
			// 
			// lblAppPort
			// 
			lblAppPort.AutoSize = true;
			lblAppPort.BackColor = Color.Transparent;
			lblAppPort.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblAppPort.ForeColor = Color.White;
			lblAppPort.Location = new Point(245, 205);
			lblAppPort.Name = "lblAppPort";
			lblAppPort.Size = new Size(63, 17);
			lblAppPort.TabIndex = 55;
			lblAppPort.Text = "App Port";
			// 
			// AppPortNumeric
			// 
			AppPortNumeric.Location = new Point(245, 225);
			AppPortNumeric.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
			AppPortNumeric.Minimum = new decimal(new int[] { 1024, 0, 0, 0 });
			AppPortNumeric.Name = "AppPortNumeric";
			AppPortNumeric.Size = new Size(78, 23);
			AppPortNumeric.TabIndex = 56;
			AppPortNumeric.Value = new decimal(new int[] { 1024, 0, 0, 0 });
			// 
			// btnViewArgs
			// 
			btnViewArgs.Location = new Point(537, 429);
			btnViewArgs.Name = "btnViewArgs";
			btnViewArgs.Size = new Size(135, 21);
			btnViewArgs.TabIndex = 57;
			btnViewArgs.Text = "View Arguments";
			btnViewArgs.UseVisualStyleBackColor = true;
			btnViewArgs.Click += btnViewArgs_Click;
			// 
			// chkUpdateOnStart
			// 
			chkUpdateOnStart.Appearance = Appearance.Button;
			chkUpdateOnStart.FlatStyle = FlatStyle.Flat;
			chkUpdateOnStart.Location = new Point(414, 296);
			chkUpdateOnStart.Name = "chkUpdateOnStart";
			chkUpdateOnStart.Size = new Size(141, 24);
			chkUpdateOnStart.TabIndex = 58;
			chkUpdateOnStart.Text = "Auto Update on Start";
			chkUpdateOnStart.TextAlign = ContentAlignment.MiddleCenter;
			chkUpdateOnStart.UseVisualStyleBackColor = true;
			chkUpdateOnStart.CheckedChanged += chkUpdateOnStart_CheckedChanged;
			// 
			// ServerSettingsGUI
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackgroundImage = Properties.Resources.background;
			BackgroundImageLayout = ImageLayout.Stretch;
			ClientSize = new Size(795, 558);
			Controls.Add(chkUpdateOnStart);
			Controls.Add(btnViewArgs);
			Controls.Add(AppPortNumeric);
			Controls.Add(lblAppPort);
			Controls.Add(lblaruments);
			Controls.Add(lblRCONport);
			Controls.Add(txtWorldSeed);
			Controls.Add(lblWorldSeed);
			Controls.Add(groupBox1);
			Controls.Add(TextLabel4);
			Controls.Add(lblRCONpassword);
			Controls.Add(txtRconPassword);
			Controls.Add(numRconPort);
			Controls.Add(chkEnableRcon);
			Controls.Add(textLabel2);
			Controls.Add(txtInstallPath);
			Controls.Add(lblCompetitive);
			Controls.Add(cmbCompetitive);
			Controls.Add(txtAdminPassword);
			Controls.Add(lblAdminPassword);
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
			Controls.Add(lblPassword);
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
			((System.ComponentModel.ISupportInitialize)AppPortNumeric).EndInit();
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
		private Label lblPassword;
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
		private Label lblAdminPassword;
		private TextBox txtAdminPassword;
		private ComboBox cmbCompetitive;
		private Label lblCompetitive;
		private TextBox txtInstallPath;
		private Label textLabel2;
		private CheckBox chkEnableRcon;
		private NumericUpDown numRconPort;
		private TextBox txtRconPassword;
		private Label TextLabel5;
		private Label lblRCONpassword;
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
		private Label lblRCONport;
		private Label lblaruments;
		private NumericUpDown numericUpDown1;
		private Label lblAppPort;
		private NumericUpDown AppPortNumeric;
		private Button btnViewArgs;
		private CheckBox chkUpdateOnStart;
	}
}