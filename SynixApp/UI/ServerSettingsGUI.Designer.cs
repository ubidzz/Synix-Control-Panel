using Synix_Control_Panel.SynixApp.Design;

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
			chkDefaultPath = new SynixToggle();
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
			chkEnableRcon = new SynixToggle();
			numRconPort = new NumericUpDown();
			txtRconPassword = new TextBox();
			lblRCONpassword = new Label();
			chkEnableSchedule = new SynixToggle();
			lblWorldSeed = new Label();
			txtWorldSeed = new TextBox();
			lblRCONport = new Label();
			lblaruments = new Label();
			lblAppPort = new Label();
			numAppPort = new NumericUpDown();
			btnViewArgs = new Button();
			chkUpdateOnStart = new SynixToggle();
			btnEditSchedule = new Button();
			chkBackupOnStart = new SynixToggle();
			chkEnableDiscord = new SynixToggle();
			txtDiscordWebhook = new TextBox();
			btnTestDiscord = new Button();
			lblConfigWarning = new Label();
			lblWorldSize = new Label();
			numWorldSize = new NumericUpDown();
			cmbGameVersion = new ComboBox();
			lblGameVersion = new Label();
			label1 = new Label();
			numRam = new NumericUpDown();
			btnSave = new SynixButton();
			btnCancel = new SynixButton();
			((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
			((System.ComponentModel.ISupportInitialize)numMaxPlayers).BeginInit();
			((System.ComponentModel.ISupportInitialize)numQueryPort).BeginInit();
			((System.ComponentModel.ISupportInitialize)numRconPort).BeginInit();
			((System.ComponentModel.ISupportInitialize)numAppPort).BeginInit();
			((System.ComponentModel.ISupportInitialize)numWorldSize).BeginInit();
			((System.ComponentModel.ISupportInitialize)numRam).BeginInit();
			SuspendLayout();
			// 
			// ServerNameLabel
			// 
			ServerNameLabel.AutoSize = true;
			ServerNameLabel.BackColor = Color.Transparent;
			ServerNameLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			ServerNameLabel.ForeColor = Color.White;
			ServerNameLabel.Location = new Point(12, 59);
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
			GameServerLabel.Location = new Point(12, 116);
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
			PortLabel.Location = new Point(330, 240);
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
			FolderPathLabel.Location = new Point(414, 464);
			FolderPathLabel.Name = "FolderPathLabel";
			FolderPathLabel.Size = new Size(104, 17);
			FolderPathLabel.TabIndex = 3;
			FolderPathLabel.Text = "Folder Location";
			// 
			// txtName
			// 
			txtName.Location = new Point(12, 79);
			txtName.Name = "txtName";
			txtName.Size = new Size(368, 23);
			txtName.TabIndex = 4;
			txtName.TextChanged += txtName_TextChanged;
			// 
			// cmbGame
			// 
			cmbGame.FormattingEnabled = true;
			cmbGame.Items.AddRange(new object[] { "Game List" });
			cmbGame.Location = new Point(12, 136);
			cmbGame.Name = "cmbGame";
			cmbGame.Size = new Size(276, 23);
			cmbGame.TabIndex = 5;
			cmbGame.Text = "Pick Game";
			cmbGame.SelectedIndexChanged += cmbGame_SelectedIndexChanged;
			// 
			// numPort
			// 
			numPort.Location = new Point(330, 260);
			numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
			numPort.Minimum = new decimal(new int[] { 1024, 0, 0, 0 });
			numPort.Name = "numPort";
			numPort.Size = new Size(50, 23);
			numPort.TabIndex = 6;
			numPort.Value = new decimal(new int[] { 1024, 0, 0, 0 });
			// 
			// btnBrowse
			// 
			btnBrowse.Location = new Point(606, 489);
			btnBrowse.Name = "btnBrowse";
			btnBrowse.Size = new Size(75, 23);
			btnBrowse.TabIndex = 8;
			btnBrowse.Text = "Browse";
			btnBrowse.UseVisualStyleBackColor = true;
			btnBrowse.Click += btnBrowse_Click;
			// 
			// chkDefaultPath
			// 
			chkDefaultPath.Appearance = Appearance.Button;
			chkDefaultPath.BackColor = Color.Transparent;
			chkDefaultPath.FlatStyle = FlatStyle.Flat;
			chkDefaultPath.ForeColor = Color.White;
			chkDefaultPath.Location = new Point(414, 484);
			chkDefaultPath.Name = "chkDefaultPath";
			chkDefaultPath.Size = new Size(157, 32);
			chkDefaultPath.TabIndex = 10;
			chkDefaultPath.Text = "Default Location";
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
			ltextLabel1.Location = new Point(687, 491);
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
			lblPassword.Location = new Point(12, 390);
			lblPassword.Name = "lblPassword";
			lblPassword.Size = new Size(109, 17);
			lblPassword.TabIndex = 12;
			lblPassword.Text = "Server Password";
			// 
			// txtPassword
			// 
			txtPassword.Location = new Point(12, 410);
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
			MaxPlayerLabel.Location = new Point(242, 240);
			MaxPlayerLabel.Name = "MaxPlayerLabel";
			MaxPlayerLabel.Size = new Size(76, 17);
			MaxPlayerLabel.TabIndex = 14;
			MaxPlayerLabel.Text = "Max Player";
			// 
			// numMaxPlayers
			// 
			numMaxPlayers.Location = new Point(242, 260);
			numMaxPlayers.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
			numMaxPlayers.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			numMaxPlayers.Name = "numMaxPlayers";
			numMaxPlayers.Size = new Size(74, 23);
			numMaxPlayers.TabIndex = 15;
			numMaxPlayers.Value = new decimal(new int[] { 10, 0, 0, 0 });
			// 
			// MapLabel
			// 
			MapLabel.AutoSize = true;
			MapLabel.BackColor = Color.Transparent;
			MapLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			MapLabel.ForeColor = Color.White;
			MapLabel.Location = new Point(12, 179);
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
			TextLabel3.Location = new Point(415, 314);
			TextLabel3.Name = "TextLabel3";
			TextLabel3.Size = new Size(124, 17);
			TextLabel3.TabIndex = 18;
			TextLabel3.Text = "Launch Arguments";
			// 
			// txtExtraArgs
			// 
			txtExtraArgs.Location = new Point(414, 422);
			txtExtraArgs.Name = "txtExtraArgs";
			txtExtraArgs.Size = new Size(368, 23);
			txtExtraArgs.TabIndex = 19;
			// 
			// TextLabel7
			// 
			TextLabel7.AutoSize = true;
			TextLabel7.BackColor = Color.Transparent;
			TextLabel7.ForeColor = Color.White;
			TextLabel7.Location = new Point(415, 404);
			TextLabel7.Name = "TextLabel7";
			TextLabel7.Size = new Size(257, 15);
			TextLabel7.TabIndex = 20;
			TextLabel7.Text = "Example:  -log, -nosteamclient, or -forceupdate";
			// 
			// QueryPortLabel
			// 
			QueryPortLabel.BackColor = Color.Transparent;
			QueryPortLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			QueryPortLabel.ForeColor = Color.White;
			QueryPortLabel.Location = new Point(12, 304);
			QueryPortLabel.Name = "QueryPortLabel";
			QueryPortLabel.Size = new Size(53, 37);
			QueryPortLabel.TabIndex = 21;
			QueryPortLabel.Text = "Query Port";
			// 
			// numQueryPort
			// 
			numQueryPort.Location = new Point(12, 344);
			numQueryPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
			numQueryPort.Minimum = new decimal(new int[] { 1024, 0, 0, 0 });
			numQueryPort.Name = "numQueryPort";
			numQueryPort.Size = new Size(53, 23);
			numQueryPort.TabIndex = 22;
			numQueryPort.Value = new decimal(new int[] { 27015, 0, 0, 0 });
			// 
			// WarningLabel
			// 
			WarningLabel.AutoEllipsis = true;
			WarningLabel.BackColor = Color.Transparent;
			WarningLabel.ForeColor = Color.Red;
			WarningLabel.Location = new Point(12, 3);
			WarningLabel.Name = "WarningLabel";
			WarningLabel.Size = new Size(771, 49);
			WarningLabel.TabIndex = 23;
			// 
			// cmbWorldName
			// 
			cmbWorldName.FormattingEnabled = true;
			cmbWorldName.Location = new Point(12, 199);
			cmbWorldName.Name = "cmbWorldName";
			cmbWorldName.Size = new Size(368, 23);
			cmbWorldName.TabIndex = 24;
			// 
			// lblAdminPassword
			// 
			lblAdminPassword.AutoSize = true;
			lblAdminPassword.BackColor = Color.Transparent;
			lblAdminPassword.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblAdminPassword.ForeColor = Color.White;
			lblAdminPassword.Location = new Point(12, 449);
			lblAdminPassword.Name = "lblAdminPassword";
			lblAdminPassword.Size = new Size(154, 17);
			lblAdminPassword.TabIndex = 25;
			lblAdminPassword.Text = "Server Admin Password";
			// 
			// txtAdminPassword
			// 
			txtAdminPassword.Location = new Point(12, 469);
			txtAdminPassword.Name = "txtAdminPassword";
			txtAdminPassword.Size = new Size(368, 23);
			txtAdminPassword.TabIndex = 26;
			// 
			// cmbCompetitive
			// 
			cmbCompetitive.FormattingEnabled = true;
			cmbCompetitive.Location = new Point(291, 136);
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
			lblCompetitive.Location = new Point(294, 116);
			lblCompetitive.Name = "lblCompetitive";
			lblCompetitive.Size = new Size(83, 17);
			lblCompetitive.TabIndex = 28;
			lblCompetitive.Text = "Competitive";
			// 
			// txtInstallPath
			// 
			txtInstallPath.Location = new Point(414, 522);
			txtInstallPath.Name = "txtInstallPath";
			txtInstallPath.ReadOnly = true;
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
			textLabel2.Location = new Point(577, 491);
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
			chkEnableRcon.Location = new Point(415, 59);
			chkEnableRcon.Name = "chkEnableRcon";
			chkEnableRcon.Size = new Size(104, 32);
			chkEnableRcon.TabIndex = 31;
			chkEnableRcon.Text = "RCON";
			chkEnableRcon.TextAlign = ContentAlignment.MiddleCenter;
			chkEnableRcon.UseVisualStyleBackColor = false;
			chkEnableRcon.CheckedChanged += chkEnableRcon_CheckedChanged;
			// 
			// numRconPort
			// 
			numRconPort.Location = new Point(415, 117);
			numRconPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
			numRconPort.Minimum = new decimal(new int[] { 1024, 0, 0, 0 });
			numRconPort.Name = "numRconPort";
			numRconPort.Size = new Size(63, 23);
			numRconPort.TabIndex = 32;
			numRconPort.Value = new decimal(new int[] { 1024, 0, 0, 0 });
			// 
			// txtRconPassword
			// 
			txtRconPassword.Location = new Point(503, 116);
			txtRconPassword.Name = "txtRconPassword";
			txtRconPassword.Size = new Size(280, 23);
			txtRconPassword.TabIndex = 33;
			// 
			// lblRCONpassword
			// 
			lblRCONpassword.AutoSize = true;
			lblRCONpassword.BackColor = Color.Transparent;
			lblRCONpassword.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblRCONpassword.ForeColor = Color.White;
			lblRCONpassword.Location = new Point(503, 96);
			lblRCONpassword.Name = "lblRCONpassword";
			lblRCONpassword.Size = new Size(106, 17);
			lblRCONpassword.TabIndex = 35;
			lblRCONpassword.Text = "RCON Password";
			// 
			// chkEnableSchedule
			// 
			chkEnableSchedule.Appearance = Appearance.Button;
			chkEnableSchedule.BackColor = Color.FromArgb(32, 32, 32);
			chkEnableSchedule.FlatStyle = FlatStyle.Flat;
			chkEnableSchedule.ForeColor = Color.White;
			chkEnableSchedule.Location = new Point(415, 164);
			chkEnableSchedule.Name = "chkEnableSchedule";
			chkEnableSchedule.Size = new Size(164, 32);
			chkEnableSchedule.TabIndex = 0;
			chkEnableSchedule.Text = "Auto Restart";
			chkEnableSchedule.TextAlign = ContentAlignment.MiddleCenter;
			chkEnableSchedule.UseVisualStyleBackColor = false;
			chkEnableSchedule.CheckedChanged += chkEnableSchedule_CheckedChanged;
			// 
			// lblWorldSeed
			// 
			lblWorldSeed.AutoSize = true;
			lblWorldSeed.BackColor = Color.Transparent;
			lblWorldSeed.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblWorldSeed.ForeColor = Color.White;
			lblWorldSeed.Location = new Point(12, 240);
			lblWorldSeed.Name = "lblWorldSeed";
			lblWorldSeed.Size = new Size(79, 17);
			lblWorldSeed.TabIndex = 46;
			lblWorldSeed.Text = "World Seed";
			// 
			// txtWorldSeed
			// 
			txtWorldSeed.Location = new Point(12, 260);
			txtWorldSeed.Name = "txtWorldSeed";
			txtWorldSeed.Size = new Size(224, 23);
			txtWorldSeed.TabIndex = 47;
			txtWorldSeed.KeyPress += txtWorldSeed_KeyPress;
			// 
			// lblRCONport
			// 
			lblRCONport.AutoSize = true;
			lblRCONport.BackColor = Color.Transparent;
			lblRCONport.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblRCONport.ForeColor = Color.White;
			lblRCONport.Location = new Point(415, 96);
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
			lblaruments.Location = new Point(414, 331);
			lblaruments.Name = "lblaruments";
			lblaruments.Size = new Size(368, 66);
			lblaruments.TabIndex = 52;
			lblaruments.Text = resources.GetString("lblaruments.Text");
			// 
			// lblAppPort
			// 
			lblAppPort.BackColor = Color.Transparent;
			lblAppPort.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblAppPort.ForeColor = Color.White;
			lblAppPort.Location = new Point(71, 305);
			lblAppPort.Name = "lblAppPort";
			lblAppPort.Size = new Size(53, 36);
			lblAppPort.TabIndex = 55;
			lblAppPort.Text = "App Port";
			// 
			// numAppPort
			// 
			numAppPort.Location = new Point(71, 344);
			numAppPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
			numAppPort.Minimum = new decimal(new int[] { 10000, 0, 0, 0 });
			numAppPort.Name = "numAppPort";
			numAppPort.Size = new Size(53, 23);
			numAppPort.TabIndex = 56;
			numAppPort.Value = new decimal(new int[] { 10000, 0, 0, 0 });
			// 
			// btnViewArgs
			// 
			btnViewArgs.Location = new Point(542, 376);
			btnViewArgs.Name = "btnViewArgs";
			btnViewArgs.Size = new Size(158, 21);
			btnViewArgs.TabIndex = 57;
			btnViewArgs.Text = "View Default Arguments";
			btnViewArgs.UseVisualStyleBackColor = true;
			btnViewArgs.Click += btnViewArgs_Click;
			// 
			// chkUpdateOnStart
			// 
			chkUpdateOnStart.Appearance = Appearance.Button;
			chkUpdateOnStart.BackColor = Color.FromArgb(32, 32, 32);
			chkUpdateOnStart.FlatStyle = FlatStyle.Flat;
			chkUpdateOnStart.Location = new Point(415, 202);
			chkUpdateOnStart.Name = "chkUpdateOnStart";
			chkUpdateOnStart.Size = new Size(164, 32);
			chkUpdateOnStart.TabIndex = 58;
			chkUpdateOnStart.Text = "Update on Start";
			chkUpdateOnStart.TextAlign = ContentAlignment.MiddleCenter;
			chkUpdateOnStart.UseVisualStyleBackColor = false;
			// 
			// btnEditSchedule
			// 
			btnEditSchedule.Location = new Point(585, 167);
			btnEditSchedule.Name = "btnEditSchedule";
			btnEditSchedule.Size = new Size(104, 26);
			btnEditSchedule.TabIndex = 62;
			btnEditSchedule.Text = "Edit Scheduler";
			btnEditSchedule.UseVisualStyleBackColor = true;
			btnEditSchedule.Click += btnEditSchedule_Click;
			// 
			// chkBackupOnStart
			// 
			chkBackupOnStart.BackColor = Color.Transparent;
			chkBackupOnStart.Location = new Point(585, 203);
			chkBackupOnStart.Name = "chkBackupOnStart";
			chkBackupOnStart.Size = new Size(164, 32);
			chkBackupOnStart.TabIndex = 64;
			chkBackupOnStart.Text = "Backup on Start";
			chkBackupOnStart.UseVisualStyleBackColor = true;
			// 
			// chkEnableDiscord
			// 
			chkEnableDiscord.BackColor = Color.Transparent;
			chkEnableDiscord.Location = new Point(415, 240);
			chkEnableDiscord.Name = "chkEnableDiscord";
			chkEnableDiscord.Size = new Size(164, 32);
			chkEnableDiscord.TabIndex = 65;
			chkEnableDiscord.Text = "Activate Discord";
			chkEnableDiscord.UseVisualStyleBackColor = true;
			chkEnableDiscord.Click += chkEnableDiscord_CheckedChanged;
			// 
			// txtDiscordWebhook
			// 
			txtDiscordWebhook.Location = new Point(415, 278);
			txtDiscordWebhook.Name = "txtDiscordWebhook";
			txtDiscordWebhook.Size = new Size(368, 23);
			txtDiscordWebhook.TabIndex = 66;
			// 
			// btnTestDiscord
			// 
			btnTestDiscord.Location = new Point(585, 244);
			btnTestDiscord.Name = "btnTestDiscord";
			btnTestDiscord.Size = new Size(115, 23);
			btnTestDiscord.TabIndex = 67;
			btnTestDiscord.Text = "Test Discord";
			btnTestDiscord.UseVisualStyleBackColor = true;
			btnTestDiscord.Click += btnTestDiscord_Click;
			// 
			// lblConfigWarning
			// 
			lblConfigWarning.BackColor = Color.Red;
			lblConfigWarning.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblConfigWarning.ForeColor = Color.Yellow;
			lblConfigWarning.Location = new Point(12, 554);
			lblConfigWarning.Name = "lblConfigWarning";
			lblConfigWarning.Size = new Size(771, 22);
			lblConfigWarning.TabIndex = 68;
			lblConfigWarning.Text = "🚨 Please boot the server completely for its initial startup, shut it down, and then configure your server settings files.";
			lblConfigWarning.TextAlign = ContentAlignment.MiddleCenter;
			lblConfigWarning.Visible = false;
			// 
			// lblWorldSize
			// 
			lblWorldSize.BackColor = Color.Transparent;
			lblWorldSize.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblWorldSize.ForeColor = Color.White;
			lblWorldSize.Location = new Point(130, 304);
			lblWorldSize.Name = "lblWorldSize";
			lblWorldSize.Size = new Size(50, 37);
			lblWorldSize.TabIndex = 69;
			lblWorldSize.Text = "World \r\nSize";
			// 
			// numWorldSize
			// 
			numWorldSize.Location = new Point(130, 344);
			numWorldSize.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
			numWorldSize.Minimum = new decimal(new int[] { 50, 0, 0, 0 });
			numWorldSize.Name = "numWorldSize";
			numWorldSize.Size = new Size(50, 23);
			numWorldSize.TabIndex = 70;
			numWorldSize.Value = new decimal(new int[] { 4000, 0, 0, 0 });
			// 
			// cmbGameVersion
			// 
			cmbGameVersion.FormattingEnabled = true;
			cmbGameVersion.Location = new Point(186, 344);
			cmbGameVersion.Name = "cmbGameVersion";
			cmbGameVersion.Size = new Size(93, 23);
			cmbGameVersion.TabIndex = 71;
			// 
			// lblGameVersion
			// 
			lblGameVersion.BackColor = Color.Transparent;
			lblGameVersion.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblGameVersion.ForeColor = Color.White;
			lblGameVersion.Location = new Point(188, 305);
			lblGameVersion.Name = "lblGameVersion";
			lblGameVersion.Size = new Size(91, 36);
			lblGameVersion.TabIndex = 72;
			lblGameVersion.Text = "Game Version";
			// 
			// label1
			// 
			label1.BackColor = Color.Transparent;
			label1.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
			label1.ForeColor = Color.White;
			label1.Location = new Point(285, 305);
			label1.Name = "label1";
			label1.Size = new Size(72, 36);
			label1.TabIndex = 73;
			label1.Text = "Server RAM (GB)";
			// 
			// numRam
			// 
			numRam.Location = new Point(285, 344);
			numRam.Maximum = new decimal(new int[] { 128, 0, 0, 0 });
			numRam.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			numRam.Name = "numRam";
			numRam.Size = new Size(60, 23);
			numRam.TabIndex = 74;
			numRam.Value = new decimal(new int[] { 2, 0, 0, 0 });
			// 
			// btnSave
			// 
			btnSave.BackColor = Color.Transparent;
			btnSave.BorderColor = Color.FromArgb(0, 80, 150);
			btnSave.BorderRadius = 8;
			btnSave.BorderSize = 1;
			btnSave.FillColor = Color.FromArgb(10, 20, 30);
			btnSave.FillColorSecondary = Color.FromArgb(20, 35, 50);
			btnSave.FlatAppearance.BorderSize = 0;
			btnSave.FlatAppearance.MouseDownBackColor = Color.Transparent;
			btnSave.FlatAppearance.MouseOverBackColor = Color.Transparent;
			btnSave.FlatStyle = FlatStyle.Flat;
			btnSave.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			btnSave.ForeColor = Color.FromArgb(50, 220, 50);
			btnSave.Location = new Point(250, 589);
			btnSave.Name = "btnSave";
			btnSave.Size = new Size(130, 40);
			btnSave.TabIndex = 75;
			btnSave.Text = "Save Server";
			btnSave.UseVisualStyleBackColor = false;
			btnSave.Click += btnSave_Click;
			// 
			// btnCancel
			// 
			btnCancel.BackColor = Color.Transparent;
			btnCancel.BorderColor = Color.FromArgb(0, 80, 150);
			btnCancel.BorderRadius = 8;
			btnCancel.BorderSize = 1;
			btnCancel.FillColor = Color.FromArgb(10, 20, 30);
			btnCancel.FillColorSecondary = Color.FromArgb(20, 35, 50);
			btnCancel.FlatAppearance.BorderSize = 0;
			btnCancel.FlatAppearance.MouseDownBackColor = Color.Transparent;
			btnCancel.FlatAppearance.MouseOverBackColor = Color.Transparent;
			btnCancel.FlatStyle = FlatStyle.Flat;
			btnCancel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			btnCancel.ForeColor = Color.Red;
			btnCancel.Location = new Point(409, 589);
			btnCancel.Name = "btnCancel";
			btnCancel.Size = new Size(130, 40);
			btnCancel.TabIndex = 76;
			btnCancel.Text = "Cancel";
			btnCancel.UseVisualStyleBackColor = false;
			btnCancel.Click += btnCancel_Click;
			// 
			// ServerSettingsGUI
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackgroundImage = Properties.Resources.background;
			BackgroundImageLayout = ImageLayout.Stretch;
			ClientSize = new Size(795, 642);
			Controls.Add(btnCancel);
			Controls.Add(btnSave);
			Controls.Add(numRam);
			Controls.Add(label1);
			Controls.Add(lblGameVersion);
			Controls.Add(cmbGameVersion);
			Controls.Add(numWorldSize);
			Controls.Add(lblWorldSize);
			Controls.Add(lblConfigWarning);
			Controls.Add(WarningLabel);
			Controls.Add(btnTestDiscord);
			Controls.Add(txtDiscordWebhook);
			Controls.Add(chkEnableDiscord);
			Controls.Add(chkBackupOnStart);
			Controls.Add(btnEditSchedule);
			Controls.Add(chkEnableSchedule);
			Controls.Add(chkUpdateOnStart);
			Controls.Add(btnViewArgs);
			Controls.Add(numAppPort);
			Controls.Add(lblAppPort);
			Controls.Add(lblaruments);
			Controls.Add(lblRCONport);
			Controls.Add(txtWorldSeed);
			Controls.Add(lblWorldSeed);
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
			((System.ComponentModel.ISupportInitialize)numAppPort).EndInit();
			((System.ComponentModel.ISupportInitialize)numWorldSize).EndInit();
			((System.ComponentModel.ISupportInitialize)numRam).EndInit();
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
		private SynixToggle chkDefaultPath;
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
		private SynixToggle chkEnableRcon;
		private NumericUpDown numRconPort;
		private TextBox txtRconPassword;
		private Label TextLabel5;
		private Label lblRCONpassword;
		private SynixToggle chkEnableSchedule;
		private Label lblWorldSeed;
		private TextBox txtWorldSeed;
		private Label lblRCONport;
		private Label lblaruments;
		private NumericUpDown numWorldSize;
		private Label lblAppPort;
		private NumericUpDown numAppPort;
		private Button btnViewArgs;
		private SynixToggle chkUpdateOnStart;
		private Button btnEditSchedule;
		private SynixToggle chkBackupOnStart;
		private SynixToggle chkEnableDiscord;
		private TextBox txtDiscordWebhook;
		private Button btnTestDiscord;
		private Label lblConfigWarning;
		private Label lblWorldSize;
		private ComboBox cmbGameVersion;
		private Label lblGameVersion;
		private Label label1;
		private NumericUpDown numRam;
		private SynixButton btnSave;
		private SynixButton btnCancel;
	}
}