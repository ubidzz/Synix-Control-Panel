// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Design;

namespace Synix_Control_Panel.SynixApp.UI.ServerSetup
{
	partial class ServerSettingsGeneralPage
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				components?.Dispose();

			base.Dispose(disposing);
		}

		#region Component Designer generated code

		private void InitializeComponent()
		{
			cardIdentity = new ModernSettingsCard();
			lblIdentityIcon = new Label();
			lblIdentityTitle = new Label();
			ServerNameLabel = new Label();
			txtName = new TextBox();
			GameServerLabel = new Label();
			cmbGame = new ModernSettingsComboBox();
			lblGameVersion = new Label();
			cmbGameVersion = new ModernSettingsComboBox();
			lblIdentityHelper = new Label();
			cardGameplay = new ModernSettingsCard();
			lblGameplayIcon = new Label();
			lblGameplayTitle = new Label();
			lblCrossplay = new Label();
			chkCrossplay = new ModernSettingsToggle();
			MapLabel = new Label();
			cmbWorldName = new ModernSettingsComboBox();
			lblCompetitive = new Label();
			cmbCompetitive = new ModernSettingsComboBox();
			MaxPlayerLabel = new Label();
			numMaxPlayers = new ModernSettingsNumericUpDown();
			label1 = new Label();
			numRam = new ModernSettingsNumericUpDown();
			lblGameplayHelper = new Label();
			cardMinecraftRuntime = new ModernSettingsCard();
			lblMinecraftRuntimeIcon = new Label();
			lblMinecraftRuntimeTitle = new Label();
			lblMinecraftLoader = new Label();
			cmbMinecraftLoader = new ModernSettingsComboBox();
			lblMinecraftLoaderVersion = new Label();
			cmbMinecraftLoaderVersion = new ModernSettingsComboBox();
			lblMinecraftJava = new Label();
			lblMinecraftJavaValue = new Label();
			lblMinecraftRuntimeHelper = new Label();
			cardCompatibility = new ModernSettingsCard();
			lblCompatibilityIcon = new Label();
			lblCompatibilityTitle = new Label();
			lblCompatibilityHelper = new Label();
			lblInstallVerification = new Label();
			lblStartVerification = new Label();
			lblStopVerification = new Label();
			lblMonitoringVerification = new Label();
			lblLastTestedVersion = new Label();
			lblMinecraftEdition = new Label();
			cmbMinecraftEdition = new ModernSettingsComboBox();
			((System.ComponentModel.ISupportInitialize)numMaxPlayers).BeginInit();
			((System.ComponentModel.ISupportInitialize)numRam).BeginInit();
			cardIdentity.SuspendLayout();
			cardGameplay.SuspendLayout();
			cardMinecraftRuntime.SuspendLayout();
			cardCompatibility.SuspendLayout();
			SuspendLayout();
			// cardIdentity
			cardIdentity.BackColor = Color.FromArgb(17, 27, 45);
			cardIdentity.BorderColor = Color.FromArgb(38, 52, 77);
			cardIdentity.Controls.Add(lblIdentityIcon);
			cardIdentity.Controls.Add(lblIdentityTitle);
			cardIdentity.Controls.Add(ServerNameLabel);
			cardIdentity.Controls.Add(txtName);
			cardIdentity.Controls.Add(GameServerLabel);
			cardIdentity.Controls.Add(cmbGame);
			cardIdentity.Controls.Add(lblGameVersion);
			cardIdentity.Controls.Add(cmbGameVersion);
			cardIdentity.Controls.Add(lblIdentityHelper);
			cardIdentity.CornerRadius = 12;
			cardIdentity.FillColor = Color.FromArgb(17, 27, 45);
			cardIdentity.Location = new Point(0, 0);
			cardIdentity.Name = "cardIdentity";
			cardIdentity.Size = new Size(438, 226);
			cardIdentity.TabIndex = 0;

			// lblIdentityIcon
			lblIdentityIcon.BackColor = Color.FromArgb(17, 27, 45);
			lblIdentityIcon.Font = new Font("Segoe UI Symbol", 16F);
			lblIdentityIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblIdentityIcon.Location = new Point(20, 14);
			lblIdentityIcon.Name = "lblIdentityIcon";
			lblIdentityIcon.Size = new Size(28, 30);
			lblIdentityIcon.TabIndex = 0;
			lblIdentityIcon.Text = "▤";
			lblIdentityIcon.TextAlign = ContentAlignment.MiddleCenter;

			// lblIdentityTitle
			lblIdentityTitle.AutoSize = true;
			lblIdentityTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblIdentityTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblIdentityTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblIdentityTitle.Location = new Point(54, 19);
			lblIdentityTitle.Name = "lblIdentityTitle";
			lblIdentityTitle.Size = new Size(127, 21);
			lblIdentityTitle.TabIndex = 1;
			lblIdentityTitle.Text = "Server Identity";

			// ServerNameLabel
			ServerNameLabel.AutoSize = true;
			ServerNameLabel.BackColor = Color.FromArgb(17, 27, 45);
			ServerNameLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			ServerNameLabel.ForeColor = Color.FromArgb(245, 247, 251);
			ServerNameLabel.Location = new Point(24, 55);
			ServerNameLabel.Name = "ServerNameLabel";
			ServerNameLabel.Size = new Size(80, 15);
			ServerNameLabel.TabIndex = 2;
			ServerNameLabel.Text = "Server Name";

			// txtName
			txtName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			txtName.AutoSize = false;
			txtName.BackColor = Color.FromArgb(12, 21, 36);
			txtName.BorderStyle = BorderStyle.FixedSingle;
			txtName.Font = new Font("Segoe UI", 10F);
			txtName.ForeColor = Color.FromArgb(245, 247, 251);
			txtName.Location = new Point(24, 75);
			txtName.Name = "txtName";
			txtName.Size = new Size(390, 34);
			txtName.TabIndex = 3;

			// GameServerLabel
			GameServerLabel.AutoSize = true;
			GameServerLabel.BackColor = Color.FromArgb(17, 27, 45);
			GameServerLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			GameServerLabel.ForeColor = Color.FromArgb(245, 247, 251);
			GameServerLabel.Location = new Point(24, 121);
			GameServerLabel.Name = "GameServerLabel";
			GameServerLabel.Size = new Size(76, 15);
			GameServerLabel.TabIndex = 4;
			GameServerLabel.Text = "Game Server";

			// cmbGame
			cmbGame.BackColor = Color.FromArgb(12, 21, 36);
			cmbGame.BorderColor = Color.FromArgb(38, 52, 77);
			cmbGame.DrawMode = DrawMode.OwnerDrawFixed;
			cmbGame.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbGame.FlatStyle = FlatStyle.Flat;
			cmbGame.FocusBorderColor = Color.FromArgb(38, 52, 77);
			cmbGame.Font = new Font("Segoe UI", 9.5F);
			cmbGame.ForeColor = Color.FromArgb(245, 247, 251);
			cmbGame.FormattingEnabled = true;
			cmbGame.ItemHeight = 28;
			cmbGame.Location = new Point(24, 141);
			cmbGame.Name = "cmbGame";
			cmbGame.Size = new Size(238, 34);
			cmbGame.TabIndex = 5;

			// lblGameVersion
			lblGameVersion.AutoSize = true;
			lblGameVersion.BackColor = Color.FromArgb(17, 27, 45);
			lblGameVersion.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblGameVersion.ForeColor = Color.FromArgb(245, 247, 251);
			lblGameVersion.Location = new Point(276, 121);
			lblGameVersion.Name = "lblGameVersion";
			lblGameVersion.Size = new Size(84, 15);
			lblGameVersion.TabIndex = 6;
			lblGameVersion.Text = "Game Version";

			// cmbGameVersion
			cmbGameVersion.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cmbGameVersion.BackColor = Color.FromArgb(12, 21, 36);
			cmbGameVersion.BorderColor = Color.FromArgb(38, 52, 77);
			cmbGameVersion.DrawMode = DrawMode.OwnerDrawFixed;
			cmbGameVersion.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbGameVersion.FlatStyle = FlatStyle.Flat;
			cmbGameVersion.FocusBorderColor = Color.FromArgb(38, 52, 77);
			cmbGameVersion.Font = new Font("Segoe UI", 9.5F);
			cmbGameVersion.ForeColor = Color.FromArgb(245, 247, 251);
			cmbGameVersion.FormattingEnabled = true;
			cmbGameVersion.ItemHeight = 28;
			cmbGameVersion.Location = new Point(276, 141);
			cmbGameVersion.Name = "cmbGameVersion";
			cmbGameVersion.Size = new Size(138, 34);
			cmbGameVersion.TabIndex = 7;

			// lblIdentityHelper
			lblIdentityHelper.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblIdentityHelper.AutoEllipsis = true;
			lblIdentityHelper.BackColor = Color.FromArgb(17, 27, 45);
			lblIdentityHelper.Font = new Font("Segoe UI", 8F);
			lblIdentityHelper.ForeColor = Color.FromArgb(158, 172, 194);
			lblIdentityHelper.Location = new Point(24, 187);
			lblIdentityHelper.Name = "lblIdentityHelper";
			lblIdentityHelper.Size = new Size(390, 22);
			lblIdentityHelper.TabIndex = 8;
			lblIdentityHelper.Text = "Required fields update automatically for the selected game.";

			// cardGameplay
			cardGameplay.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardGameplay.BackColor = Color.FromArgb(17, 27, 45);
			cardGameplay.BorderColor = Color.FromArgb(38, 52, 77);
			cardGameplay.Controls.Add(lblGameplayIcon);
			cardGameplay.Controls.Add(lblGameplayTitle);
			cardGameplay.Controls.Add(lblCrossplay);
			cardGameplay.Controls.Add(chkCrossplay);
			cardGameplay.Controls.Add(MapLabel);
			cardGameplay.Controls.Add(cmbWorldName);
			cardGameplay.Controls.Add(lblCompetitive);
			cardGameplay.Controls.Add(cmbCompetitive);
			cardGameplay.Controls.Add(MaxPlayerLabel);
			cardGameplay.Controls.Add(numMaxPlayers);
			cardGameplay.Controls.Add(label1);
			cardGameplay.Controls.Add(numRam);
			cardGameplay.Controls.Add(lblGameplayHelper);
			cardGameplay.CornerRadius = 12;
			cardGameplay.FillColor = Color.FromArgb(17, 27, 45);
			cardGameplay.Location = new Point(454, 0);
			cardGameplay.Name = "cardGameplay";
			cardGameplay.Size = new Size(460, 226);
			cardGameplay.TabIndex = 1;

			// lblGameplayIcon
			lblGameplayIcon.BackColor = Color.FromArgb(17, 27, 45);
			lblGameplayIcon.Font = new Font("Segoe UI Symbol", 16F);
			lblGameplayIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblGameplayIcon.Location = new Point(20, 14);
			lblGameplayIcon.Name = "lblGameplayIcon";
			lblGameplayIcon.Size = new Size(28, 30);
			lblGameplayIcon.TabIndex = 0;
			lblGameplayIcon.Text = "◎";
			lblGameplayIcon.TextAlign = ContentAlignment.MiddleCenter;

			// lblGameplayTitle
			lblGameplayTitle.AutoSize = true;
			lblGameplayTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblGameplayTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblGameplayTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblGameplayTitle.Location = new Point(54, 19);
			lblGameplayTitle.Name = "lblGameplayTitle";
			lblGameplayTitle.Size = new Size(144, 21);
			lblGameplayTitle.TabIndex = 1;
			lblGameplayTitle.Text = "Gameplay Profile";

			// lblCrossplay
			lblCrossplay.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lblCrossplay.AutoSize = true;
			lblCrossplay.BackColor = Color.FromArgb(17, 27, 45);
			lblCrossplay.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblCrossplay.ForeColor = Color.FromArgb(245, 247, 251);
			lblCrossplay.Location = new Point(300, 22);
			lblCrossplay.Name = "lblCrossplay";
			lblCrossplay.Size = new Size(59, 15);
			lblCrossplay.TabIndex = 2;
			lblCrossplay.Text = "Crossplay";
			lblCrossplay.Visible = false;

			// chkCrossplay
			chkCrossplay.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			chkCrossplay.BackColor = Color.FromArgb(17, 27, 45);
			chkCrossplay.Checked = true;
			chkCrossplay.Location = new Point(382, 14);
			chkCrossplay.Name = "chkCrossplay";
			chkCrossplay.Size = new Size(54, 30);
			chkCrossplay.TabIndex = 3;
			chkCrossplay.Visible = false;

			// MapLabel
			MapLabel.AutoSize = true;
			MapLabel.BackColor = Color.FromArgb(17, 27, 45);
			MapLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			MapLabel.ForeColor = Color.FromArgb(245, 247, 251);
			MapLabel.Location = new Point(24, 55);
			MapLabel.Name = "MapLabel";
			MapLabel.Size = new Size(31, 15);
			MapLabel.TabIndex = 2;
			MapLabel.Text = "Map";

			// cmbWorldName
			cmbWorldName.BackColor = Color.FromArgb(12, 21, 36);
			cmbWorldName.BorderColor = Color.FromArgb(38, 52, 77);
			cmbWorldName.DrawMode = DrawMode.OwnerDrawFixed;
			cmbWorldName.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbWorldName.FlatStyle = FlatStyle.Flat;
			cmbWorldName.FocusBorderColor = Color.FromArgb(38, 52, 77);
			cmbWorldName.Font = new Font("Segoe UI", 9.5F);
			cmbWorldName.ForeColor = Color.FromArgb(245, 247, 251);
			cmbWorldName.FormattingEnabled = true;
			cmbWorldName.ItemHeight = 28;
			cmbWorldName.Location = new Point(24, 75);
			cmbWorldName.Name = "cmbWorldName";
			cmbWorldName.Size = new Size(192, 34);
			cmbWorldName.TabIndex = 3;

			// lblCompetitive
			lblCompetitive.AutoSize = true;
			lblCompetitive.BackColor = Color.FromArgb(17, 27, 45);
			lblCompetitive.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblCompetitive.ForeColor = Color.FromArgb(245, 247, 251);
			lblCompetitive.Location = new Point(238, 55);
			lblCompetitive.Name = "lblCompetitive";
			lblCompetitive.Size = new Size(72, 15);
			lblCompetitive.TabIndex = 4;
			lblCompetitive.Text = "Game Mode";

			// cmbCompetitive
			cmbCompetitive.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cmbCompetitive.BackColor = Color.FromArgb(12, 21, 36);
			cmbCompetitive.BorderColor = Color.FromArgb(38, 52, 77);
			cmbCompetitive.DrawMode = DrawMode.OwnerDrawFixed;
			cmbCompetitive.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbCompetitive.FlatStyle = FlatStyle.Flat;
			cmbCompetitive.FocusBorderColor = Color.FromArgb(38, 52, 77);
			cmbCompetitive.Font = new Font("Segoe UI", 9.5F);
			cmbCompetitive.ForeColor = Color.FromArgb(245, 247, 251);
			cmbCompetitive.FormattingEnabled = true;
			cmbCompetitive.ItemHeight = 28;
			cmbCompetitive.Location = new Point(238, 75);
			cmbCompetitive.Name = "cmbCompetitive";
			cmbCompetitive.Size = new Size(198, 34);
			cmbCompetitive.TabIndex = 5;

			// MaxPlayerLabel
			MaxPlayerLabel.AutoSize = true;
			MaxPlayerLabel.BackColor = Color.FromArgb(17, 27, 45);
			MaxPlayerLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			MaxPlayerLabel.ForeColor = Color.FromArgb(245, 247, 251);
			MaxPlayerLabel.Location = new Point(24, 124);
			MaxPlayerLabel.Name = "MaxPlayerLabel";
			MaxPlayerLabel.Size = new Size(72, 15);
			MaxPlayerLabel.TabIndex = 6;
			MaxPlayerLabel.Text = "Max Players";

			// numMaxPlayers
			numMaxPlayers.BackColor = Color.FromArgb(12, 21, 36);
			numMaxPlayers.Font = new Font("Segoe UI", 10F);
			numMaxPlayers.ForeColor = Color.FromArgb(245, 247, 251);
			numMaxPlayers.Location = new Point(24, 145);
			numMaxPlayers.Maximum = 1000;
			numMaxPlayers.Minimum = 1;
			numMaxPlayers.Name = "numMaxPlayers";
			numMaxPlayers.Size = new Size(192, 34);
			numMaxPlayers.TabIndex = 7;
			numMaxPlayers.Value = 10;

			// label1
			label1.AutoSize = true;
			label1.BackColor = Color.FromArgb(17, 27, 45);
			label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			label1.ForeColor = Color.FromArgb(245, 247, 251);
			label1.Location = new Point(238, 124);
			label1.Name = "label1";
			label1.Size = new Size(98, 15);
			label1.TabIndex = 8;
			label1.Text = "Server RAM (GB)";

			// numRam
			numRam.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			numRam.BackColor = Color.FromArgb(12, 21, 36);
			numRam.Font = new Font("Segoe UI", 10F);
			numRam.ForeColor = Color.FromArgb(245, 247, 251);
			numRam.Location = new Point(238, 145);
			numRam.Maximum = 128;
			numRam.Minimum = 1;
			numRam.Name = "numRam";
			numRam.Size = new Size(198, 34);
			numRam.TabIndex = 9;
			numRam.Value = 2;

			// lblGameplayHelper
			lblGameplayHelper.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblGameplayHelper.AutoEllipsis = true;
			lblGameplayHelper.BackColor = Color.FromArgb(17, 27, 45);
			lblGameplayHelper.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
			lblGameplayHelper.ForeColor = Color.FromArgb(158, 172, 194);
			lblGameplayHelper.Location = new Point(24, 187);
			lblGameplayHelper.Name = "lblGameplayHelper";
			lblGameplayHelper.Size = new Size(412, 22);
			lblGameplayHelper.TabIndex = 10;
			lblGameplayHelper.Text = "Map and mode choices come directly from the selected game template.";

			// cardMinecraftRuntime
			cardMinecraftRuntime.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardMinecraftRuntime.BackColor = Color.FromArgb(17, 27, 45);
			cardMinecraftRuntime.BorderColor = Color.FromArgb(38, 52, 77);
			cardMinecraftRuntime.Controls.Add(lblMinecraftRuntimeIcon);
			cardMinecraftRuntime.Controls.Add(lblMinecraftRuntimeTitle);
			cardMinecraftRuntime.Controls.Add(lblMinecraftEdition);
			cardMinecraftRuntime.Controls.Add(cmbMinecraftEdition);
			cardMinecraftRuntime.Controls.Add(lblMinecraftLoader);
			cardMinecraftRuntime.Controls.Add(cmbMinecraftLoader);
			cardMinecraftRuntime.Controls.Add(lblMinecraftLoaderVersion);
			cardMinecraftRuntime.Controls.Add(cmbMinecraftLoaderVersion);
			cardMinecraftRuntime.Controls.Add(lblMinecraftJava);
			cardMinecraftRuntime.Controls.Add(lblMinecraftJavaValue);
			cardMinecraftRuntime.Controls.Add(lblMinecraftRuntimeHelper);
			cardMinecraftRuntime.CornerRadius = 12;
			cardMinecraftRuntime.FillColor = Color.FromArgb(17, 27, 45);
			cardMinecraftRuntime.Location = new Point(0, 242);
			cardMinecraftRuntime.Name = "cardMinecraftRuntime";
			cardMinecraftRuntime.Size = new Size(914, 200);
			cardMinecraftRuntime.TabIndex = 2;
			cardMinecraftRuntime.Visible = true;

			// lblMinecraftRuntimeIcon
			lblMinecraftRuntimeIcon.BackColor = Color.FromArgb(17, 27, 45);
			lblMinecraftRuntimeIcon.Font = new Font("Segoe UI Symbol", 16F);
			lblMinecraftRuntimeIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblMinecraftRuntimeIcon.Location = new Point(20, 12);
			lblMinecraftRuntimeIcon.Name = "lblMinecraftRuntimeIcon";
			lblMinecraftRuntimeIcon.Size = new Size(28, 30);
			lblMinecraftRuntimeIcon.TabIndex = 0;
			lblMinecraftRuntimeIcon.Text = "⬡";
			lblMinecraftRuntimeIcon.TextAlign = ContentAlignment.MiddleCenter;

			// lblMinecraftRuntimeTitle
			lblMinecraftRuntimeTitle.AutoSize = true;
			lblMinecraftRuntimeTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblMinecraftRuntimeTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblMinecraftRuntimeTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblMinecraftRuntimeTitle.Location = new Point(54, 17);
			lblMinecraftRuntimeTitle.Name = "lblMinecraftRuntimeTitle";
			lblMinecraftRuntimeTitle.Size = new Size(145, 21);
			lblMinecraftRuntimeTitle.TabIndex = 1;
			lblMinecraftRuntimeTitle.Text = "Minecraft Runtime";

			// lblMinecraftEdition
			lblMinecraftEdition.AutoSize = true;
			lblMinecraftEdition.BackColor = Color.FromArgb(17, 27, 45);
			lblMinecraftEdition.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblMinecraftEdition.ForeColor = Color.FromArgb(245, 247, 251);
			lblMinecraftEdition.Location = new Point(24, 52);
			lblMinecraftEdition.Name = "lblMinecraftEdition";
			lblMinecraftEdition.Size = new Size(43, 15);
			lblMinecraftEdition.TabIndex = 2;
			lblMinecraftEdition.Text = "Edition";

			// cmbMinecraftEdition
			cmbMinecraftEdition.BackColor = Color.FromArgb(12, 21, 36);
			cmbMinecraftEdition.BorderColor = Color.FromArgb(38, 52, 77);
			cmbMinecraftEdition.DrawMode = DrawMode.OwnerDrawFixed;
			cmbMinecraftEdition.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbMinecraftEdition.FlatStyle = FlatStyle.Flat;
			cmbMinecraftEdition.FocusBorderColor = Color.FromArgb(38, 52, 77);
			cmbMinecraftEdition.Font = new Font("Segoe UI", 9.5F);
			cmbMinecraftEdition.ForeColor = Color.FromArgb(245, 247, 251);
			cmbMinecraftEdition.FormattingEnabled = true;
			cmbMinecraftEdition.ItemHeight = 28;
			cmbMinecraftEdition.Items.AddRange(new object[] { "Java", "Bedrock" });
			cmbMinecraftEdition.Location = new Point(24, 72);
			cmbMinecraftEdition.Name = "cmbMinecraftEdition";
			cmbMinecraftEdition.Size = new Size(260, 34);
			cmbMinecraftEdition.TabIndex = 3;

			// lblMinecraftLoader
			lblMinecraftLoader.AutoSize = true;
			lblMinecraftLoader.BackColor = Color.FromArgb(17, 27, 45);
			lblMinecraftLoader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblMinecraftLoader.ForeColor = Color.FromArgb(245, 247, 251);
			lblMinecraftLoader.Location = new Point(24, 106);
			lblMinecraftLoader.Name = "lblMinecraftLoader";
			lblMinecraftLoader.Size = new Size(43, 15);
			lblMinecraftLoader.TabIndex = 2;
			lblMinecraftLoader.Text = "Loader";

			// cmbMinecraftLoader
			cmbMinecraftLoader.BackColor = Color.FromArgb(12, 21, 36);
			cmbMinecraftLoader.BorderColor = Color.FromArgb(38, 52, 77);
			cmbMinecraftLoader.DrawMode = DrawMode.OwnerDrawFixed;
			cmbMinecraftLoader.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbMinecraftLoader.FlatStyle = FlatStyle.Flat;
			cmbMinecraftLoader.FocusBorderColor = Color.FromArgb(38, 52, 77);
			cmbMinecraftLoader.Font = new Font("Segoe UI", 9.5F);
			cmbMinecraftLoader.ForeColor = Color.FromArgb(245, 247, 251);
			cmbMinecraftLoader.FormattingEnabled = true;
			cmbMinecraftLoader.ItemHeight = 28;
			cmbMinecraftLoader.Items.AddRange(new object[] { "Vanilla", "Fabric", "Forge" });
			cmbMinecraftLoader.Location = new Point(24, 126);
			cmbMinecraftLoader.Name = "cmbMinecraftLoader";
			cmbMinecraftLoader.Size = new Size(260, 34);
			cmbMinecraftLoader.TabIndex = 3;

			// lblMinecraftLoaderVersion
			lblMinecraftLoaderVersion.AutoSize = true;
			lblMinecraftLoaderVersion.BackColor = Color.FromArgb(17, 27, 45);
			lblMinecraftLoaderVersion.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblMinecraftLoaderVersion.ForeColor = Color.FromArgb(245, 247, 251);
			lblMinecraftLoaderVersion.Location = new Point(310, 106);
			lblMinecraftLoaderVersion.Name = "lblMinecraftLoaderVersion";
			lblMinecraftLoaderVersion.Size = new Size(88, 15);
			lblMinecraftLoaderVersion.TabIndex = 4;
			lblMinecraftLoaderVersion.Text = "Loader Version";

			// cmbMinecraftLoaderVersion
			cmbMinecraftLoaderVersion.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cmbMinecraftLoaderVersion.BackColor = Color.FromArgb(12, 21, 36);
			cmbMinecraftLoaderVersion.BorderColor = Color.FromArgb(38, 52, 77);
			cmbMinecraftLoaderVersion.DrawMode = DrawMode.OwnerDrawFixed;
			cmbMinecraftLoaderVersion.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbMinecraftLoaderVersion.FlatStyle = FlatStyle.Flat;
			cmbMinecraftLoaderVersion.FocusBorderColor = Color.FromArgb(38, 52, 77);
			cmbMinecraftLoaderVersion.Font = new Font("Segoe UI", 9.5F);
			cmbMinecraftLoaderVersion.ForeColor = Color.FromArgb(245, 247, 251);
			cmbMinecraftLoaderVersion.FormattingEnabled = true;
			cmbMinecraftLoaderVersion.ItemHeight = 28;
			cmbMinecraftLoaderVersion.Location = new Point(310, 126);
			cmbMinecraftLoaderVersion.Name = "cmbMinecraftLoaderVersion";
			cmbMinecraftLoaderVersion.Size = new Size(350, 34);
			cmbMinecraftLoaderVersion.TabIndex = 5;

			// lblMinecraftJava
			lblMinecraftJava.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lblMinecraftJava.AutoSize = true;
			lblMinecraftJava.BackColor = Color.FromArgb(17, 27, 45);
			lblMinecraftJava.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblMinecraftJava.ForeColor = Color.FromArgb(245, 247, 251);
			lblMinecraftJava.Location = new Point(686, 106);
			lblMinecraftJava.Name = "lblMinecraftJava";
			lblMinecraftJava.Size = new Size(82, 15);
			lblMinecraftJava.TabIndex = 6;
			lblMinecraftJava.Text = "Portable Java";

			// lblMinecraftJavaValue
			lblMinecraftJavaValue.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lblMinecraftJavaValue.BackColor = Color.FromArgb(12, 21, 36);
			lblMinecraftJavaValue.BorderStyle = BorderStyle.FixedSingle;
			lblMinecraftJavaValue.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			lblMinecraftJavaValue.ForeColor = Color.FromArgb(32, 214, 199);
			lblMinecraftJavaValue.Location = new Point(686, 126);
			lblMinecraftJavaValue.Name = "lblMinecraftJavaValue";
			lblMinecraftJavaValue.Padding = new Padding(10, 0, 10, 0);
			lblMinecraftJavaValue.Size = new Size(204, 34);
			lblMinecraftJavaValue.TabIndex = 7;
			lblMinecraftJavaValue.Text = "Resolved automatically";
			lblMinecraftJavaValue.TextAlign = ContentAlignment.MiddleLeft;

			// lblMinecraftRuntimeHelper
			lblMinecraftRuntimeHelper.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblMinecraftRuntimeHelper.AutoEllipsis = true;
			lblMinecraftRuntimeHelper.BackColor = Color.FromArgb(17, 27, 45);
			lblMinecraftRuntimeHelper.Font = new Font("Segoe UI", 8F);
			lblMinecraftRuntimeHelper.ForeColor = Color.FromArgb(158, 172, 194);
			lblMinecraftRuntimeHelper.Location = new Point(24, 170);
			lblMinecraftRuntimeHelper.Name = "lblMinecraftRuntimeHelper";
			lblMinecraftRuntimeHelper.Size = new Size(866, 18);
			lblMinecraftRuntimeHelper.TabIndex = 8;
			lblMinecraftRuntimeHelper.Text = "Synix installs the selected server loader and matching portable Java. Add your own mods after installation.";

			// cardCompatibility
			cardCompatibility.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardCompatibility.BackColor = Color.FromArgb(17, 27, 45);
			cardCompatibility.BorderColor = Color.FromArgb(38, 52, 77);
			cardCompatibility.Controls.Add(lblCompatibilityIcon);
			cardCompatibility.Controls.Add(lblCompatibilityTitle);
			cardCompatibility.Controls.Add(lblCompatibilityHelper);
			cardCompatibility.Controls.Add(lblInstallVerification);
			cardCompatibility.Controls.Add(lblStartVerification);
			cardCompatibility.Controls.Add(lblStopVerification);
			cardCompatibility.Controls.Add(lblMonitoringVerification);
			cardCompatibility.Controls.Add(lblLastTestedVersion);
			cardCompatibility.CornerRadius = 12;
			cardCompatibility.FillColor = Color.FromArgb(17, 27, 45);
			cardCompatibility.Location = new Point(0, 458);
			cardCompatibility.Name = "cardCompatibility";
			cardCompatibility.Size = new Size(914, 146);
			cardCompatibility.TabIndex = 3;

			// lblCompatibilityIcon
			lblCompatibilityIcon.BackColor = Color.FromArgb(17, 27, 45);
			lblCompatibilityIcon.Font = new Font("Segoe UI Symbol", 16F);
			lblCompatibilityIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblCompatibilityIcon.Location = new Point(20, 12);
			lblCompatibilityIcon.Name = "lblCompatibilityIcon";
			lblCompatibilityIcon.Size = new Size(28, 30);
			lblCompatibilityIcon.TabIndex = 0;
			lblCompatibilityIcon.Text = "✓";
			lblCompatibilityIcon.TextAlign = ContentAlignment.MiddleCenter;

			// lblCompatibilityTitle
			lblCompatibilityTitle.AutoSize = true;
			lblCompatibilityTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblCompatibilityTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblCompatibilityTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblCompatibilityTitle.Location = new Point(54, 17);
			lblCompatibilityTitle.Name = "lblCompatibilityTitle";
			lblCompatibilityTitle.Size = new Size(211, 21);
			lblCompatibilityTitle.TabIndex = 1;
			lblCompatibilityTitle.Text = "Compatibility Verification";

			// lblCompatibilityHelper
			lblCompatibilityHelper.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblCompatibilityHelper.BackColor = Color.FromArgb(17, 27, 45);
			lblCompatibilityHelper.Font = new Font("Segoe UI", 8F);
			lblCompatibilityHelper.ForeColor = Color.FromArgb(158, 172, 194);
			lblCompatibilityHelper.Location = new Point(280, 18);
			lblCompatibilityHelper.Name = "lblCompatibilityHelper";
			lblCompatibilityHelper.Size = new Size(610, 20);
			lblCompatibilityHelper.TabIndex = 2;
			lblCompatibilityHelper.Text = "Synix verifies each action automatically after it succeeds on this PC.";
			lblCompatibilityHelper.TextAlign = ContentAlignment.MiddleRight;

			// lblInstallVerification
			lblInstallVerification.BackColor = Color.FromArgb(12, 21, 36);
			lblInstallVerification.BorderStyle = BorderStyle.FixedSingle;
			lblInstallVerification.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblInstallVerification.ForeColor = Color.FromArgb(158, 172, 194);
			lblInstallVerification.Location = new Point(24, 52);
			lblInstallVerification.Name = "lblInstallVerification";
			lblInstallVerification.Padding = new Padding(10, 0, 10, 0);
			lblInstallVerification.Size = new Size(200, 30);
			lblInstallVerification.TabIndex = 3;
			lblInstallVerification.Text = "Install  — Not verified yet";
			lblInstallVerification.TextAlign = ContentAlignment.MiddleLeft;

			// lblStartVerification
			lblStartVerification.BackColor = Color.FromArgb(12, 21, 36);
			lblStartVerification.BorderStyle = BorderStyle.FixedSingle;
			lblStartVerification.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblStartVerification.ForeColor = Color.FromArgb(158, 172, 194);
			lblStartVerification.Location = new Point(238, 52);
			lblStartVerification.Name = "lblStartVerification";
			lblStartVerification.Padding = new Padding(10, 0, 10, 0);
			lblStartVerification.Size = new Size(200, 30);
			lblStartVerification.TabIndex = 4;
			lblStartVerification.Text = "Start  — Not verified yet";
			lblStartVerification.TextAlign = ContentAlignment.MiddleLeft;

			// lblStopVerification
			lblStopVerification.BackColor = Color.FromArgb(12, 21, 36);
			lblStopVerification.BorderStyle = BorderStyle.FixedSingle;
			lblStopVerification.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblStopVerification.ForeColor = Color.FromArgb(158, 172, 194);
			lblStopVerification.Location = new Point(452, 52);
			lblStopVerification.Name = "lblStopVerification";
			lblStopVerification.Padding = new Padding(10, 0, 10, 0);
			lblStopVerification.Size = new Size(200, 30);
			lblStopVerification.TabIndex = 5;
			lblStopVerification.Text = "Stop  — Not verified yet";
			lblStopVerification.TextAlign = ContentAlignment.MiddleLeft;

			// lblMonitoringVerification
			lblMonitoringVerification.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblMonitoringVerification.BackColor = Color.FromArgb(12, 21, 36);
			lblMonitoringVerification.BorderStyle = BorderStyle.FixedSingle;
			lblMonitoringVerification.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblMonitoringVerification.ForeColor = Color.FromArgb(158, 172, 194);
			lblMonitoringVerification.Location = new Point(666, 52);
			lblMonitoringVerification.Name = "lblMonitoringVerification";
			lblMonitoringVerification.Padding = new Padding(10, 0, 10, 0);
			lblMonitoringVerification.Size = new Size(224, 30);
			lblMonitoringVerification.TabIndex = 6;
			lblMonitoringVerification.Text = "Monitoring  — Not verified yet";
			lblMonitoringVerification.TextAlign = ContentAlignment.MiddleLeft;

			// lblLastTestedVersion
			lblLastTestedVersion.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblLastTestedVersion.BackColor = Color.FromArgb(17, 27, 45);
			lblLastTestedVersion.Font = new Font("Segoe UI", 9F);
			lblLastTestedVersion.ForeColor = Color.FromArgb(158, 172, 194);
			lblLastTestedVersion.Location = new Point(24, 96);
			lblLastTestedVersion.Name = "lblLastTestedVersion";
			lblLastTestedVersion.Size = new Size(866, 28);
			lblLastTestedVersion.TabIndex = 7;
			lblLastTestedVersion.Text = "Last-tested Synix version: Not verified yet";
			lblLastTestedVersion.TextAlign = ContentAlignment.MiddleLeft;

			// ServerSettingsGeneralPage
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			AutoScroll = true;
			BackColor = Color.FromArgb(8, 13, 24);
			Controls.Add(cardIdentity);
			Controls.Add(cardGameplay);
			Controls.Add(cardMinecraftRuntime);
			Controls.Add(cardCompatibility);
			Name = "ServerSettingsGeneralPage";
			Size = new Size(914, 496);
			((System.ComponentModel.ISupportInitialize)numMaxPlayers).EndInit();
			((System.ComponentModel.ISupportInitialize)numRam).EndInit();
			cardIdentity.ResumeLayout(false);
			cardIdentity.PerformLayout();
			cardGameplay.ResumeLayout(false);
			cardGameplay.PerformLayout();
			cardMinecraftRuntime.ResumeLayout(false);
			cardMinecraftRuntime.PerformLayout();
			cardCompatibility.ResumeLayout(false);
			cardCompatibility.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		internal ModernSettingsCard cardIdentity;
		internal Label lblIdentityIcon;
		internal Label lblIdentityTitle;
		internal Label ServerNameLabel;
		internal TextBox txtName;
		internal Label GameServerLabel;
		internal ModernSettingsComboBox cmbGame;
		internal Label lblGameVersion;
		internal ModernSettingsComboBox cmbGameVersion;
		internal Label lblIdentityHelper;
		internal ModernSettingsCard cardGameplay;
		internal Label lblGameplayIcon;
		internal Label lblGameplayTitle;
		internal Label lblCrossplay;
		internal ModernSettingsToggle chkCrossplay;
		internal Label MapLabel;
		internal ModernSettingsComboBox cmbWorldName;
		internal Label lblCompetitive;
		internal ModernSettingsComboBox cmbCompetitive;
		internal Label MaxPlayerLabel;
		internal ModernSettingsNumericUpDown numMaxPlayers;
		internal Label label1;
		internal ModernSettingsNumericUpDown numRam;
		internal Label lblGameplayHelper;
		internal ModernSettingsCard cardMinecraftRuntime;
		internal Label lblMinecraftRuntimeIcon;
		internal Label lblMinecraftRuntimeTitle;
		internal Label lblMinecraftLoader;
		internal ModernSettingsComboBox cmbMinecraftLoader;
		internal Label lblMinecraftLoaderVersion;
		internal ModernSettingsComboBox cmbMinecraftLoaderVersion;
		internal Label lblMinecraftJava;
		internal Label lblMinecraftJavaValue;
		internal Label lblMinecraftRuntimeHelper;
		internal ModernSettingsCard cardCompatibility;
		internal Label lblCompatibilityIcon;
		internal Label lblCompatibilityTitle;
		internal Label lblCompatibilityHelper;
		internal Label lblInstallVerification;
		internal Label lblStartVerification;
		internal Label lblStopVerification;
		internal Label lblMonitoringVerification;
		internal Label lblLastTestedVersion;
		internal Label lblMinecraftEdition;
		internal ModernSettingsComboBox cmbMinecraftEdition;
	}
}
