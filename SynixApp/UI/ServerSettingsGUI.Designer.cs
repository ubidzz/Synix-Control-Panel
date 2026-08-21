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
using Synix_Control_Panel.SynixApp.Design;

namespace Synix_Control_Panel
{
	partial class ServerSettingsGUI
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerSettingsGUI));
			pnlTitleBar = new Panel();
			lblBrand = new Label();
			lblWindowTitle = new Label();
			btnTitleMinimize = new Button();
			btnTitleClose = new Button();
			pnlFooter = new Panel();
			lblFooterStatus = new Label();
			btnCancel = new ModernSettingsButton();
			btnSave = new ModernSettingsButton();
			pnlBody = new Panel();
			pnlSidebar = new Panel();
			lblSidebarSection = new Label();
			btnNavGeneral = new ModernSettingsNavButton();
			btnNavWorld = new ModernSettingsNavButton();
			btnNavNetwork = new ModernSettingsNavButton();
			btnNavAutomation = new ModernSettingsNavButton();
			btnNavInstall = new ModernSettingsNavButton();
			pnlSidebarStatus = new Panel();
			pnlSidebarDivider = new Panel();
			lblSidebarStatusHeading = new Label();
			lblSidebarStatus = new Label();
			lblSidebarStatusDetail = new Label();
			pnlContent = new Panel();
			lblPageTitle = new Label();
			lblPageDescription = new Label();
			lblModeBadge = new Label();
			lblTemplateBehavior = new Label();
			pnlPageHost = new Panel();
			pnlPageGeneral = new Panel();
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
			cardCredentials = new ModernSettingsCard();
			lblCredentialsIcon = new Label();
			lblCredentialsTitle = new Label();
			lblPassword = new Label();
			txtPassword = new TextBox();
			lblAdminPassword = new Label();
			txtAdminPassword = new TextBox();
			lblCredentialsNote = new Label();
			pnlPageWorld = new Panel();
			cardWorldGeneration = new ModernSettingsCard();
			lblWorldIcon = new Label();
			lblWorldTitle = new Label();
			lblWorldDescription = new Label();
			lblWorldSeed = new Label();
			txtWorldSeed = new TextBox();
			lblWorldSize = new Label();
			numWorldSize = new ModernSettingsNumericUpDown();
			pnlPageNetwork = new Panel();
			cardPorts = new ModernSettingsCard();
			lblPortsIcon = new Label();
			lblPortsTitle = new Label();
			lblPortsDescription = new Label();
			PortLabel = new Label();
			numPort = new ModernSettingsNumericUpDown();
			QueryPortLabel = new Label();
			numQueryPort = new ModernSettingsNumericUpDown();
			lblAppPort = new Label();
			numAppPort = new ModernSettingsNumericUpDown();
			cardRcon = new ModernSettingsCard();
			lblRconIcon = new Label();
			lblRconTitle = new Label();
			lblRconDescription = new Label();
			lblRconToggleTitle = new Label();
			chkEnableRcon = new ModernSettingsToggle();
			lblRCONport = new Label();
			numRconPort = new ModernSettingsNumericUpDown();
			lblRCONpassword = new Label();
			txtRconPassword = new TextBox();
			pnlPageAutomation = new Panel();
			cardStartup = new ModernSettingsCard();
			lblStartupIcon = new Label();
			lblStartupTitle = new Label();
			lblUpdateTitle = new Label();
			lblUpdateDescription = new Label();
			chkUpdateOnStart = new ModernSettingsToggle();
			lblBackupTitle = new Label();
			lblBackupDescription = new Label();
			chkBackupOnStart = new ModernSettingsToggle();
			cardSchedule = new ModernSettingsCard();
			lblScheduleIcon = new Label();
			lblScheduleTitle = new Label();
			lblScheduleDescription = new Label();
			chkEnableSchedule = new ModernSettingsToggle();
			btnEditSchedule = new ModernSettingsButton();
			cardDiscord = new ModernSettingsCard();
			lblDiscordIcon = new Label();
			lblDiscordTitle = new Label();
			lblDiscordDescription = new Label();
			chkEnableDiscord = new ModernSettingsToggle();
			txtDiscordWebhook = new TextBox();
			btnTestDiscord = new ModernSettingsButton();
			pnlPageInstall = new Panel();
			cardInstallLocation = new ModernSettingsCard();
			lblInstallIcon = new Label();
			lblInstallTitle = new Label();
			lblDefaultPathTitle = new Label();
			lblDefaultPathDescription = new Label();
			chkDefaultPath = new ModernSettingsToggle();
			FolderPathLabel = new Label();
			txtInstallPath = new TextBox();
			btnBrowse = new ModernSettingsButton();
			cardLaunchArguments = new ModernSettingsCard();
			lblLaunchIcon = new Label();
			lblLaunchTitle = new Label();
			lblaruments = new Label();
			btnViewArgs = new ModernSettingsButton();
			TextLabel3 = new Label();
			TextLabel7 = new Label();
			txtExtraArgs = new TextBox();
			((System.ComponentModel.ISupportInitialize)numMaxPlayers).BeginInit();
			((System.ComponentModel.ISupportInitialize)numRam).BeginInit();
			((System.ComponentModel.ISupportInitialize)numWorldSize).BeginInit();
			((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
			((System.ComponentModel.ISupportInitialize)numQueryPort).BeginInit();
			((System.ComponentModel.ISupportInitialize)numAppPort).BeginInit();
			((System.ComponentModel.ISupportInitialize)numRconPort).BeginInit();
			pnlTitleBar.SuspendLayout();
			pnlFooter.SuspendLayout();
			pnlBody.SuspendLayout();
			pnlSidebar.SuspendLayout();
			pnlSidebarStatus.SuspendLayout();
			pnlContent.SuspendLayout();
			pnlPageHost.SuspendLayout();
			pnlPageGeneral.SuspendLayout();
			cardIdentity.SuspendLayout();
			cardGameplay.SuspendLayout();
			cardMinecraftRuntime.SuspendLayout();
			cardCredentials.SuspendLayout();
			pnlPageWorld.SuspendLayout();
			cardWorldGeneration.SuspendLayout();
			pnlPageNetwork.SuspendLayout();
			cardPorts.SuspendLayout();
			cardRcon.SuspendLayout();
			pnlPageAutomation.SuspendLayout();
			cardStartup.SuspendLayout();
			cardSchedule.SuspendLayout();
			cardDiscord.SuspendLayout();
			pnlPageInstall.SuspendLayout();
			cardInstallLocation.SuspendLayout();
			cardLaunchArguments.SuspendLayout();
			SuspendLayout();

			// pnlTitleBar
			pnlTitleBar.BackColor = Color.FromArgb(6, 12, 22);
			pnlTitleBar.Controls.Add(lblBrand);
			pnlTitleBar.Controls.Add(lblWindowTitle);
			pnlTitleBar.Controls.Add(btnTitleMinimize);
			pnlTitleBar.Controls.Add(btnTitleClose);
			pnlTitleBar.Dock = DockStyle.Top;
			pnlTitleBar.Location = new Point(0, 0);
			pnlTitleBar.Name = "pnlTitleBar";
			pnlTitleBar.Size = new Size(1180, 56);
			pnlTitleBar.TabIndex = 0;
			pnlTitleBar.MouseDown += TitleBar_MouseDown;

			// lblBrand
			lblBrand.AutoSize = true;
			lblBrand.BackColor = Color.FromArgb(6, 12, 22);
			lblBrand.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblBrand.ForeColor = Color.FromArgb(245, 247, 251);
			lblBrand.Location = new Point(20, 17);
			lblBrand.Name = "lblBrand";
			lblBrand.Size = new Size(46, 21);
			lblBrand.TabIndex = 0;
			lblBrand.Text = "Synix";
			lblBrand.MouseDown += TitleBar_MouseDown;

			// lblWindowTitle
			lblWindowTitle.AutoSize = true;
			lblWindowTitle.BackColor = Color.FromArgb(6, 12, 22);
			lblWindowTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
			lblWindowTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblWindowTitle.Location = new Point(78, 18);
			lblWindowTitle.Name = "lblWindowTitle";
			lblWindowTitle.Size = new Size(97, 20);
			lblWindowTitle.TabIndex = 1;
			lblWindowTitle.Text = "Server Setup";
			lblWindowTitle.MouseDown += TitleBar_MouseDown;

			// btnTitleMinimize
			btnTitleMinimize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnTitleMinimize.BackColor = Color.FromArgb(6, 12, 22);
			btnTitleMinimize.Cursor = Cursors.Hand;
			btnTitleMinimize.FlatAppearance.BorderSize = 0;
			btnTitleMinimize.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 33, 54);
			btnTitleMinimize.FlatAppearance.MouseOverBackColor = Color.FromArgb(16, 30, 48);
			btnTitleMinimize.FlatStyle = FlatStyle.Flat;
			btnTitleMinimize.Font = new Font("Segoe UI", 12F);
			btnTitleMinimize.ForeColor = Color.FromArgb(245, 247, 251);
			btnTitleMinimize.Location = new Point(1084, 0);
			btnTitleMinimize.Name = "btnTitleMinimize";
			btnTitleMinimize.Size = new Size(48, 55);
			btnTitleMinimize.TabIndex = 3;
			btnTitleMinimize.Text = "—";
			btnTitleMinimize.UseVisualStyleBackColor = false;
			btnTitleMinimize.Click += btnTitleMinimize_Click;

			// btnTitleClose
			btnTitleClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnTitleClose.BackColor = Color.FromArgb(6, 12, 22);
			btnTitleClose.Cursor = Cursors.Hand;
			btnTitleClose.FlatAppearance.BorderSize = 0;
			btnTitleClose.FlatAppearance.MouseDownBackColor = Color.FromArgb(116, 35, 45);
			btnTitleClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(83, 28, 38);
			btnTitleClose.FlatStyle = FlatStyle.Flat;
			btnTitleClose.Font = new Font("Segoe UI", 12F);
			btnTitleClose.ForeColor = Color.FromArgb(245, 247, 251);
			btnTitleClose.Location = new Point(1132, 0);
			btnTitleClose.Name = "btnTitleClose";
			btnTitleClose.Size = new Size(48, 55);
			btnTitleClose.TabIndex = 4;
			btnTitleClose.Text = "×";
			btnTitleClose.UseVisualStyleBackColor = false;
			btnTitleClose.Click += btnTitleClose_Click;

			// pnlFooter
			pnlFooter.BackColor = Color.FromArgb(6, 12, 22);
			pnlFooter.Controls.Add(lblFooterStatus);
			pnlFooter.Controls.Add(btnCancel);
			pnlFooter.Controls.Add(btnSave);
			pnlFooter.Dock = DockStyle.Bottom;
			pnlFooter.Location = new Point(0, 708);
			pnlFooter.Name = "pnlFooter";
			pnlFooter.Size = new Size(1180, 72);
			pnlFooter.TabIndex = 2;

			// lblFooterStatus
			lblFooterStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblFooterStatus.AutoEllipsis = false;
			lblFooterStatus.BackColor = Color.FromArgb(6, 12, 22);
			lblFooterStatus.Font = new Font("Segoe UI", 9.5F);
			lblFooterStatus.ForeColor = Color.FromArgb(158, 172, 194);
			lblFooterStatus.Location = new Point(24, 12);
			lblFooterStatus.Name = "lblFooterStatus";
			lblFooterStatus.Size = new Size(790, 48);
			lblFooterStatus.TabIndex = 0;
			lblFooterStatus.Text = "🔒 [REQUIRED] Enter a Server Name and select a Game Template.";
			lblFooterStatus.TextAlign = ContentAlignment.MiddleLeft;
			lblFooterStatus.UseMnemonic = false;

			// btnCancel
			btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnCancel.BackColor = Color.FromArgb(12, 21, 36);
			btnCancel.Cursor = Cursors.Hand;
			btnCancel.DialogResult = DialogResult.Cancel;
			btnCancel.FlatAppearance.BorderSize = 0;
			btnCancel.FlatStyle = FlatStyle.Flat;
			btnCancel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
			btnCancel.ForeColor = Color.FromArgb(245, 247, 251);
			btnCancel.Location = new Point(844, 14);
			btnCancel.Name = "btnCancel";
			btnCancel.Padding = new Padding(14, 0, 14, 0);
			btnCancel.Size = new Size(140, 44);
			btnCancel.TabIndex = 1;
			btnCancel.Text = "Cancel";
			btnCancel.UseAccentStyle = false;
			btnCancel.UseVisualStyleBackColor = false;
			btnCancel.Click += btnCancel_Click;

			// btnSave
			btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnSave.BackColor = Color.FromArgb(32, 214, 199);
			btnSave.Cursor = Cursors.Hand;
			btnSave.Enabled = false;
			btnSave.FlatAppearance.BorderSize = 0;
			btnSave.FlatStyle = FlatStyle.Flat;
			btnSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
			btnSave.ForeColor = Color.FromArgb(8, 13, 24);
			btnSave.Location = new Point(996, 14);
			btnSave.Name = "btnSave";
			btnSave.Padding = new Padding(16, 0, 16, 0);
			btnSave.Size = new Size(160, 44);
			btnSave.TabIndex = 2;
			btnSave.Text = "Save Server";
			btnSave.UseAccentStyle = true;
			btnSave.UseVisualStyleBackColor = false;
			btnSave.Click += btnSave_Click;

			// pnlBody
			pnlBody.BackColor = Color.FromArgb(8, 13, 24);
			pnlBody.Controls.Add(pnlContent);
			pnlBody.Controls.Add(pnlSidebar);
			pnlBody.Dock = DockStyle.Fill;
			pnlBody.Location = new Point(0, 56);
			pnlBody.Name = "pnlBody";
			pnlBody.Size = new Size(1180, 652);
			pnlBody.TabIndex = 1;

			// pnlSidebar
			pnlSidebar.BackColor = Color.FromArgb(10, 18, 32);
			pnlSidebar.Controls.Add(lblSidebarSection);
			pnlSidebar.Controls.Add(btnNavGeneral);
			pnlSidebar.Controls.Add(btnNavWorld);
			pnlSidebar.Controls.Add(btnNavNetwork);
			pnlSidebar.Controls.Add(btnNavAutomation);
			pnlSidebar.Controls.Add(btnNavInstall);
			pnlSidebar.Controls.Add(pnlSidebarStatus);
			pnlSidebar.Dock = DockStyle.Left;
			pnlSidebar.Location = new Point(0, 0);
			pnlSidebar.Name = "pnlSidebar";
			pnlSidebar.Size = new Size(210, 652);
			pnlSidebar.TabIndex = 0;

			// lblSidebarSection
			lblSidebarSection.BackColor = Color.FromArgb(10, 18, 32);
			lblSidebarSection.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
			lblSidebarSection.ForeColor = Color.FromArgb(125, 165, 213);
			lblSidebarSection.Location = new Point(20, 24);
			lblSidebarSection.Name = "lblSidebarSection";
			lblSidebarSection.Size = new Size(176, 22);
			lblSidebarSection.TabIndex = 0;
			lblSidebarSection.Text = "SERVER CONFIGURATION";

			// btnNavGeneral
			btnNavGeneral.BackColor = Color.FromArgb(10, 18, 32);
			btnNavGeneral.Font = new Font("Segoe UI", 10F);
			btnNavGeneral.ForeColor = Color.FromArgb(158, 172, 194);
			btnNavGeneral.IconGlyph = "≡";
			btnNavGeneral.Location = new Point(12, 58);
			btnNavGeneral.Name = "btnNavGeneral";
			btnNavGeneral.Selected = true;
			btnNavGeneral.Size = new Size(186, 52);
			btnNavGeneral.TabIndex = 1;
			btnNavGeneral.Text = "General";
			btnNavGeneral.Click += btnNavGeneral_Click;

			// btnNavWorld
			btnNavWorld.BackColor = Color.FromArgb(10, 18, 32);
			btnNavWorld.Font = new Font("Segoe UI", 10F);
			btnNavWorld.ForeColor = Color.FromArgb(158, 172, 194);
			btnNavWorld.IconGlyph = "◎";
			btnNavWorld.Location = new Point(12, 118);
			btnNavWorld.Name = "btnNavWorld";
			btnNavWorld.Size = new Size(186, 52);
			btnNavWorld.TabIndex = 2;
			btnNavWorld.Text = "World Generation";
			btnNavWorld.Click += btnNavWorld_Click;

			// btnNavNetwork
			btnNavNetwork.BackColor = Color.FromArgb(10, 18, 32);
			btnNavNetwork.Font = new Font("Segoe UI", 10F);
			btnNavNetwork.ForeColor = Color.FromArgb(158, 172, 194);
			btnNavNetwork.IconGlyph = "⌘";
			btnNavNetwork.Location = new Point(12, 178);
			btnNavNetwork.Name = "btnNavNetwork";
			btnNavNetwork.Size = new Size(186, 52);
			btnNavNetwork.TabIndex = 3;
			btnNavNetwork.Text = "Network & RCON";
			btnNavNetwork.Click += btnNavNetwork_Click;

			// btnNavAutomation
			btnNavAutomation.BackColor = Color.FromArgb(10, 18, 32);
			btnNavAutomation.Font = new Font("Segoe UI", 10F);
			btnNavAutomation.ForeColor = Color.FromArgb(158, 172, 194);
			btnNavAutomation.IconGlyph = "⚙";
			btnNavAutomation.Location = new Point(12, 238);
			btnNavAutomation.Name = "btnNavAutomation";
			btnNavAutomation.Size = new Size(186, 52);
			btnNavAutomation.TabIndex = 4;
			btnNavAutomation.Text = "Automation";
			btnNavAutomation.Click += btnNavAutomation_Click;

			// btnNavInstall
			btnNavInstall.BackColor = Color.FromArgb(10, 18, 32);
			btnNavInstall.Font = new Font("Segoe UI", 10F);
			btnNavInstall.ForeColor = Color.FromArgb(158, 172, 194);
			btnNavInstall.IconGlyph = "➜";
			btnNavInstall.Location = new Point(12, 298);
			btnNavInstall.Name = "btnNavInstall";
			btnNavInstall.Size = new Size(186, 52);
			btnNavInstall.TabIndex = 5;
			btnNavInstall.Text = "Install & Launch";
			btnNavInstall.Click += btnNavInstall_Click;

			// pnlSidebarStatus
			pnlSidebarStatus.BackColor = Color.FromArgb(10, 18, 32);
			pnlSidebarStatus.Controls.Add(pnlSidebarDivider);
			pnlSidebarStatus.Controls.Add(lblSidebarStatusHeading);
			pnlSidebarStatus.Controls.Add(lblSidebarStatus);
			pnlSidebarStatus.Controls.Add(lblSidebarStatusDetail);
			pnlSidebarStatus.Dock = DockStyle.Bottom;
			pnlSidebarStatus.Location = new Point(0, 502);
			pnlSidebarStatus.Name = "pnlSidebarStatus";
			pnlSidebarStatus.Size = new Size(210, 150);
			pnlSidebarStatus.TabIndex = 6;

			// pnlSidebarDivider
			pnlSidebarDivider.BackColor = Color.FromArgb(38, 52, 77);
			pnlSidebarDivider.Dock = DockStyle.Top;
			pnlSidebarDivider.Location = new Point(0, 0);
			pnlSidebarDivider.Name = "pnlSidebarDivider";
			pnlSidebarDivider.Size = new Size(210, 1);
			pnlSidebarDivider.TabIndex = 0;

			// lblSidebarStatusHeading
			lblSidebarStatusHeading.BackColor = Color.FromArgb(10, 18, 32);
			lblSidebarStatusHeading.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
			lblSidebarStatusHeading.ForeColor = Color.FromArgb(125, 165, 213);
			lblSidebarStatusHeading.Location = new Point(20, 25);
			lblSidebarStatusHeading.Name = "lblSidebarStatusHeading";
			lblSidebarStatusHeading.Size = new Size(176, 20);
			lblSidebarStatusHeading.TabIndex = 1;
			lblSidebarStatusHeading.Text = "CONFIGURATION STATUS";

			// lblSidebarStatus
			lblSidebarStatus.BackColor = Color.FromArgb(10, 18, 32);
			lblSidebarStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
			lblSidebarStatus.ForeColor = Color.FromArgb(245, 185, 76);
			lblSidebarStatus.Location = new Point(20, 57);
			lblSidebarStatus.Name = "lblSidebarStatus";
			lblSidebarStatus.Size = new Size(176, 24);
			lblSidebarStatus.TabIndex = 2;
			lblSidebarStatus.Text = "●  Action required";

			// lblSidebarStatusDetail
			lblSidebarStatusDetail.BackColor = Color.FromArgb(10, 18, 32);
			lblSidebarStatusDetail.Font = new Font("Segoe UI", 8.5F);
			lblSidebarStatusDetail.ForeColor = Color.FromArgb(158, 172, 194);
			lblSidebarStatusDetail.Location = new Point(20, 87);
			lblSidebarStatusDetail.Name = "lblSidebarStatusDetail";
			lblSidebarStatusDetail.Size = new Size(176, 42);
			lblSidebarStatusDetail.TabIndex = 3;
			lblSidebarStatusDetail.Text = "Review the highlighted requirement";

			// pnlContent
			pnlContent.BackColor = Color.FromArgb(8, 13, 24);
			pnlContent.Controls.Add(lblPageTitle);
			pnlContent.Controls.Add(lblPageDescription);
			pnlContent.Controls.Add(lblModeBadge);
			pnlContent.Controls.Add(lblTemplateBehavior);
			pnlContent.Controls.Add(pnlPageHost);
			pnlContent.Dock = DockStyle.Fill;
			pnlContent.Location = new Point(210, 0);
			pnlContent.Name = "pnlContent";
			pnlContent.Size = new Size(970, 652);
			pnlContent.TabIndex = 1;

			// lblPageTitle
			lblPageTitle.AutoSize = true;
			lblPageTitle.BackColor = Color.FromArgb(8, 13, 24);
			lblPageTitle.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
			lblPageTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblPageTitle.Location = new Point(28, 18);
			lblPageTitle.Name = "lblPageTitle";
			lblPageTitle.Size = new Size(126, 41);
			lblPageTitle.TabIndex = 0;
			lblPageTitle.Text = "General";
			lblPageTitle.UseMnemonic = false;

			// lblPageDescription
			lblPageDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblPageDescription.AutoEllipsis = true;
			lblPageDescription.BackColor = Color.FromArgb(8, 13, 24);
			lblPageDescription.Font = new Font("Segoe UI", 9.5F);
			lblPageDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblPageDescription.Location = new Point(30, 61);
			lblPageDescription.Name = "lblPageDescription";
			lblPageDescription.Size = new Size(710, 22);
			lblPageDescription.TabIndex = 1;
			lblPageDescription.Text = "Choose the game and define the server identity.";

			// lblModeBadge
			lblModeBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lblModeBadge.BackColor = Color.FromArgb(12, 47, 59);
			lblModeBadge.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
			lblModeBadge.ForeColor = Color.FromArgb(32, 214, 199);
			lblModeBadge.Location = new Point(810, 25);
			lblModeBadge.Name = "lblModeBadge";
			lblModeBadge.Size = new Size(132, 34);
			lblModeBadge.TabIndex = 2;
			lblModeBadge.Text = "NEW SERVER";
			lblModeBadge.TextAlign = ContentAlignment.MiddleCenter;

			// lblTemplateBehavior
			lblTemplateBehavior.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblTemplateBehavior.BackColor = Color.FromArgb(11, 35, 47);
			lblTemplateBehavior.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblTemplateBehavior.ForeColor = Color.FromArgb(32, 214, 199);
			lblTemplateBehavior.Location = new Point(28, 88);
			lblTemplateBehavior.Name = "lblTemplateBehavior";
			lblTemplateBehavior.Padding = new Padding(14, 0, 14, 0);
			lblTemplateBehavior.Size = new Size(914, 34);
			lblTemplateBehavior.TabIndex = 3;
			lblTemplateBehavior.Text = "◇  Template-aware controls: unavailable settings are disabled automatically for the selected game.";
			lblTemplateBehavior.TextAlign = ContentAlignment.MiddleLeft;
			lblTemplateBehavior.UseMnemonic = false;

			// pnlPageHost
			pnlPageHost.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			pnlPageHost.BackColor = Color.FromArgb(8, 13, 24);
			pnlPageHost.Controls.Add(pnlPageGeneral);
			pnlPageHost.Controls.Add(pnlPageWorld);
			pnlPageHost.Controls.Add(pnlPageNetwork);
			pnlPageHost.Controls.Add(pnlPageAutomation);
			pnlPageHost.Controls.Add(pnlPageInstall);
			pnlPageHost.Location = new Point(28, 136);
			pnlPageHost.Name = "pnlPageHost";
			pnlPageHost.Size = new Size(914, 496);
			pnlPageHost.TabIndex = 5;

			// pnlPageGeneral
			pnlPageGeneral.AutoScroll = true;
			pnlPageGeneral.BackColor = Color.FromArgb(8, 13, 24);
			pnlPageGeneral.Controls.Add(cardIdentity);
			pnlPageGeneral.Controls.Add(cardGameplay);
			pnlPageGeneral.Controls.Add(cardMinecraftRuntime);
			pnlPageGeneral.Controls.Add(cardCredentials);
			pnlPageGeneral.Dock = DockStyle.Fill;
			pnlPageGeneral.Location = new Point(0, 0);
			pnlPageGeneral.Name = "pnlPageGeneral";
			pnlPageGeneral.Size = new Size(914, 440);
			pnlPageGeneral.TabIndex = 0;

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
			txtName.TextChanged += txtName_TextChanged;

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
			cmbGame.SelectedIndexChanged += cmbGame_SelectedIndexChanged;

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
			cardMinecraftRuntime.Size = new Size(914, 146);
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

			// lblMinecraftLoader
			lblMinecraftLoader.AutoSize = true;
			lblMinecraftLoader.BackColor = Color.FromArgb(17, 27, 45);
			lblMinecraftLoader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblMinecraftLoader.ForeColor = Color.FromArgb(245, 247, 251);
			lblMinecraftLoader.Location = new Point(24, 52);
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
			cmbMinecraftLoader.Location = new Point(24, 72);
			cmbMinecraftLoader.Name = "cmbMinecraftLoader";
			cmbMinecraftLoader.Size = new Size(260, 34);
			cmbMinecraftLoader.TabIndex = 3;

			// lblMinecraftLoaderVersion
			lblMinecraftLoaderVersion.AutoSize = true;
			lblMinecraftLoaderVersion.BackColor = Color.FromArgb(17, 27, 45);
			lblMinecraftLoaderVersion.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblMinecraftLoaderVersion.ForeColor = Color.FromArgb(245, 247, 251);
			lblMinecraftLoaderVersion.Location = new Point(310, 52);
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
			cmbMinecraftLoaderVersion.Location = new Point(310, 72);
			cmbMinecraftLoaderVersion.Name = "cmbMinecraftLoaderVersion";
			cmbMinecraftLoaderVersion.Size = new Size(350, 34);
			cmbMinecraftLoaderVersion.TabIndex = 5;

			// lblMinecraftJava
			lblMinecraftJava.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lblMinecraftJava.AutoSize = true;
			lblMinecraftJava.BackColor = Color.FromArgb(17, 27, 45);
			lblMinecraftJava.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblMinecraftJava.ForeColor = Color.FromArgb(245, 247, 251);
			lblMinecraftJava.Location = new Point(686, 52);
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
			lblMinecraftJavaValue.Location = new Point(686, 72);
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
			lblMinecraftRuntimeHelper.Location = new Point(24, 116);
			lblMinecraftRuntimeHelper.Name = "lblMinecraftRuntimeHelper";
			lblMinecraftRuntimeHelper.Size = new Size(866, 18);
			lblMinecraftRuntimeHelper.TabIndex = 8;
			lblMinecraftRuntimeHelper.Text = "Synix installs the selected server loader and matching portable Java. Add your own mods after installation.";

			// cardCredentials
			cardCredentials.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardCredentials.BackColor = Color.FromArgb(17, 27, 45);
			cardCredentials.BorderColor = Color.FromArgb(38, 52, 77);
			cardCredentials.Controls.Add(lblCredentialsIcon);
			cardCredentials.Controls.Add(lblCredentialsTitle);
			cardCredentials.Controls.Add(lblPassword);
			cardCredentials.Controls.Add(txtPassword);
			cardCredentials.Controls.Add(lblAdminPassword);
			cardCredentials.Controls.Add(txtAdminPassword);
			cardCredentials.Controls.Add(lblCredentialsNote);
			cardCredentials.CornerRadius = 12;
			cardCredentials.FillColor = Color.FromArgb(17, 27, 45);
			cardCredentials.Location = new Point(0, 404);
			cardCredentials.Name = "cardCredentials";
			cardCredentials.Size = new Size(914, 154);
			cardCredentials.TabIndex = 2;

			// lblCredentialsIcon
			lblCredentialsIcon.BackColor = Color.FromArgb(17, 27, 45);
			lblCredentialsIcon.Font = new Font("Segoe UI Symbol", 16F);
			lblCredentialsIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblCredentialsIcon.Location = new Point(20, 12);
			lblCredentialsIcon.Name = "lblCredentialsIcon";
			lblCredentialsIcon.Size = new Size(28, 30);
			lblCredentialsIcon.TabIndex = 0;
			lblCredentialsIcon.Text = "◇";
			lblCredentialsIcon.TextAlign = ContentAlignment.MiddleCenter;

			// lblCredentialsTitle
			lblCredentialsTitle.AutoSize = true;
			lblCredentialsTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblCredentialsTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblCredentialsTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblCredentialsTitle.Location = new Point(54, 17);
			lblCredentialsTitle.Name = "lblCredentialsTitle";
			lblCredentialsTitle.Size = new Size(153, 21);
			lblCredentialsTitle.TabIndex = 1;
			lblCredentialsTitle.Text = "Access Credentials";

			// lblPassword
			lblPassword.AutoSize = true;
			lblPassword.BackColor = Color.FromArgb(17, 27, 45);
			lblPassword.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblPassword.ForeColor = Color.FromArgb(245, 247, 251);
			lblPassword.Location = new Point(24, 50);
			lblPassword.Name = "lblPassword";
			lblPassword.Size = new Size(100, 15);
			lblPassword.TabIndex = 2;
			lblPassword.Text = "Server Password";

			// txtPassword
			txtPassword.AutoSize = false;
			txtPassword.BackColor = Color.FromArgb(12, 21, 36);
			txtPassword.BorderStyle = BorderStyle.FixedSingle;
			txtPassword.Font = new Font("Segoe UI", 10F);
			txtPassword.ForeColor = Color.FromArgb(245, 247, 251);
			txtPassword.Location = new Point(24, 70);
			txtPassword.Name = "txtPassword";
			txtPassword.Size = new Size(414, 34);
			txtPassword.TabIndex = 3;

			// lblAdminPassword
			lblAdminPassword.AutoSize = true;
			lblAdminPassword.BackColor = Color.FromArgb(17, 27, 45);
			lblAdminPassword.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblAdminPassword.ForeColor = Color.FromArgb(245, 247, 251);
			lblAdminPassword.Location = new Point(462, 50);
			lblAdminPassword.Name = "lblAdminPassword";
			lblAdminPassword.Size = new Size(102, 15);
			lblAdminPassword.TabIndex = 4;
			lblAdminPassword.Text = "Admin Password";

			// txtAdminPassword
			txtAdminPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			txtAdminPassword.AutoSize = false;
			txtAdminPassword.BackColor = Color.FromArgb(12, 21, 36);
			txtAdminPassword.BorderStyle = BorderStyle.FixedSingle;
			txtAdminPassword.Font = new Font("Segoe UI", 10F);
			txtAdminPassword.ForeColor = Color.FromArgb(245, 247, 251);
			txtAdminPassword.Location = new Point(462, 70);
			txtAdminPassword.Name = "txtAdminPassword";
			txtAdminPassword.Size = new Size(428, 34);
			txtAdminPassword.TabIndex = 5;

			// lblCredentialsNote
			lblCredentialsNote.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblCredentialsNote.BackColor = Color.FromArgb(17, 27, 45);
			lblCredentialsNote.Font = new Font("Segoe UI", 8F);
			lblCredentialsNote.ForeColor = Color.FromArgb(158, 172, 194);
			lblCredentialsNote.Location = new Point(24, 116);
			lblCredentialsNote.Name = "lblCredentialsNote";
			lblCredentialsNote.Size = new Size(866, 22);
			lblCredentialsNote.TabIndex = 6;
			lblCredentialsNote.Text = "◇  Sensitive fields follow the Synix Privacy Mode setting.";

			// pnlPageWorld
			pnlPageWorld.AutoScroll = true;
			pnlPageWorld.BackColor = Color.FromArgb(8, 13, 24);
			pnlPageWorld.Controls.Add(cardWorldGeneration);
			pnlPageWorld.Dock = DockStyle.Fill;
			pnlPageWorld.Location = new Point(0, 0);
			pnlPageWorld.Name = "pnlPageWorld";
			pnlPageWorld.Size = new Size(914, 440);
			pnlPageWorld.TabIndex = 1;
			pnlPageWorld.Visible = false;

			// cardWorldGeneration
			cardWorldGeneration.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardWorldGeneration.BackColor = Color.FromArgb(17, 27, 45);
			cardWorldGeneration.BorderColor = Color.FromArgb(38, 52, 77);
			cardWorldGeneration.Controls.Add(lblWorldIcon);
			cardWorldGeneration.Controls.Add(lblWorldTitle);
			cardWorldGeneration.Controls.Add(lblWorldDescription);
			cardWorldGeneration.Controls.Add(lblWorldSeed);
			cardWorldGeneration.Controls.Add(txtWorldSeed);
			cardWorldGeneration.Controls.Add(lblWorldSize);
			cardWorldGeneration.Controls.Add(numWorldSize);
			cardWorldGeneration.CornerRadius = 12;
			cardWorldGeneration.FillColor = Color.FromArgb(17, 27, 45);
			cardWorldGeneration.Location = new Point(0, 0);
			cardWorldGeneration.Name = "cardWorldGeneration";
			cardWorldGeneration.Size = new Size(914, 206);
			cardWorldGeneration.TabIndex = 0;

			// lblWorldIcon
			lblWorldIcon.BackColor = Color.FromArgb(17, 27, 45);
			lblWorldIcon.Font = new Font("Segoe UI Symbol", 16F);
			lblWorldIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblWorldIcon.Location = new Point(20, 14);
			lblWorldIcon.Name = "lblWorldIcon";
			lblWorldIcon.Size = new Size(28, 30);
			lblWorldIcon.TabIndex = 0;
			lblWorldIcon.Text = "◎";
			lblWorldIcon.TextAlign = ContentAlignment.MiddleCenter;

			// lblWorldTitle
			lblWorldTitle.AutoSize = true;
			lblWorldTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblWorldTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblWorldTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblWorldTitle.Location = new Point(54, 19);
			lblWorldTitle.Name = "lblWorldTitle";
			lblWorldTitle.Size = new Size(145, 21);
			lblWorldTitle.TabIndex = 1;
			lblWorldTitle.Text = "World Generation";

			// lblWorldDescription
			lblWorldDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblWorldDescription.BackColor = Color.FromArgb(17, 27, 45);
			lblWorldDescription.Font = new Font("Segoe UI", 8.5F);
			lblWorldDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblWorldDescription.Location = new Point(24, 50);
			lblWorldDescription.Name = "lblWorldDescription";
			lblWorldDescription.Size = new Size(866, 22);
			lblWorldDescription.TabIndex = 2;
			lblWorldDescription.Text = "These values are enabled only when the selected server template supports them.";

			// lblWorldSeed
			lblWorldSeed.AutoSize = true;
			lblWorldSeed.BackColor = Color.FromArgb(17, 27, 45);
			lblWorldSeed.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblWorldSeed.ForeColor = Color.FromArgb(245, 247, 251);
			lblWorldSeed.Location = new Point(24, 90);
			lblWorldSeed.Name = "lblWorldSeed";
			lblWorldSeed.Size = new Size(68, 15);
			lblWorldSeed.TabIndex = 3;
			lblWorldSeed.Text = "World Seed";

			// txtWorldSeed
			txtWorldSeed.AutoSize = false;
			txtWorldSeed.BackColor = Color.FromArgb(12, 21, 36);
			txtWorldSeed.BorderStyle = BorderStyle.FixedSingle;
			txtWorldSeed.Font = new Font("Segoe UI", 10F);
			txtWorldSeed.ForeColor = Color.FromArgb(245, 247, 251);
			txtWorldSeed.Location = new Point(24, 112);
			txtWorldSeed.Name = "txtWorldSeed";
			txtWorldSeed.Size = new Size(580, 36);
			txtWorldSeed.TabIndex = 4;
			txtWorldSeed.KeyPress += txtWorldSeed_KeyPress;

			// lblWorldSize
			lblWorldSize.AutoSize = true;
			lblWorldSize.BackColor = Color.FromArgb(17, 27, 45);
			lblWorldSize.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblWorldSize.ForeColor = Color.FromArgb(245, 247, 251);
			lblWorldSize.Location = new Point(628, 90);
			lblWorldSize.Name = "lblWorldSize";
			lblWorldSize.Size = new Size(65, 15);
			lblWorldSize.TabIndex = 5;
			lblWorldSize.Text = "World Size";

			// numWorldSize
			numWorldSize.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			numWorldSize.BackColor = Color.FromArgb(12, 21, 36);
			numWorldSize.Font = new Font("Segoe UI", 10F);
			numWorldSize.ForeColor = Color.FromArgb(245, 247, 251);
			numWorldSize.Location = new Point(628, 112);
			numWorldSize.Maximum = 5000;
			numWorldSize.Minimum = 50;
			numWorldSize.Name = "numWorldSize";
			numWorldSize.Size = new Size(262, 36);
			numWorldSize.TabIndex = 6;
			numWorldSize.Value = 4000;

			// pnlPageNetwork
			pnlPageNetwork.AutoScroll = true;
			pnlPageNetwork.BackColor = Color.FromArgb(8, 13, 24);
			pnlPageNetwork.Controls.Add(cardPorts);
			pnlPageNetwork.Controls.Add(cardRcon);
			pnlPageNetwork.Dock = DockStyle.Fill;
			pnlPageNetwork.Location = new Point(0, 0);
			pnlPageNetwork.Name = "pnlPageNetwork";
			pnlPageNetwork.Size = new Size(914, 440);
			pnlPageNetwork.TabIndex = 2;
			pnlPageNetwork.Visible = false;

			// cardPorts
			cardPorts.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardPorts.BackColor = Color.FromArgb(17, 27, 45);
			cardPorts.BorderColor = Color.FromArgb(38, 52, 77);
			cardPorts.Controls.Add(lblPortsIcon);
			cardPorts.Controls.Add(lblPortsTitle);
			cardPorts.Controls.Add(lblPortsDescription);
			cardPorts.Controls.Add(PortLabel);
			cardPorts.Controls.Add(numPort);
			cardPorts.Controls.Add(QueryPortLabel);
			cardPorts.Controls.Add(numQueryPort);
			cardPorts.Controls.Add(lblAppPort);
			cardPorts.Controls.Add(numAppPort);
			cardPorts.CornerRadius = 12;
			cardPorts.FillColor = Color.FromArgb(17, 27, 45);
			cardPorts.Location = new Point(0, 0);
			cardPorts.Name = "cardPorts";
			cardPorts.Size = new Size(914, 190);
			cardPorts.TabIndex = 0;

			// lblPortsIcon
			lblPortsIcon.BackColor = Color.FromArgb(17, 27, 45);
			lblPortsIcon.Font = new Font("Segoe UI Symbol", 16F);
			lblPortsIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblPortsIcon.Location = new Point(20, 14);
			lblPortsIcon.Name = "lblPortsIcon";
			lblPortsIcon.Size = new Size(28, 30);
			lblPortsIcon.TabIndex = 0;
			lblPortsIcon.Text = "⌘";
			lblPortsIcon.TextAlign = ContentAlignment.MiddleCenter;

			// lblPortsTitle
			lblPortsTitle.AutoSize = true;
			lblPortsTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblPortsTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblPortsTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblPortsTitle.Location = new Point(54, 19);
			lblPortsTitle.Name = "lblPortsTitle";
			lblPortsTitle.Size = new Size(117, 21);
			lblPortsTitle.TabIndex = 1;
			lblPortsTitle.Text = "Service Ports";

			// lblPortsDescription
			lblPortsDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblPortsDescription.BackColor = Color.FromArgb(17, 27, 45);
			lblPortsDescription.Font = new Font("Segoe UI", 8.5F);
			lblPortsDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblPortsDescription.Location = new Point(24, 49);
			lblPortsDescription.Name = "lblPortsDescription";
			lblPortsDescription.Size = new Size(866, 24);
			lblPortsDescription.TabIndex = 2;
			lblPortsDescription.Text = "Port availability is checked automatically against running processes and other Synix servers.";

			// PortLabel
			PortLabel.AutoSize = true;
			PortLabel.BackColor = Color.FromArgb(17, 27, 45);
			PortLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			PortLabel.ForeColor = Color.FromArgb(245, 247, 251);
			PortLabel.Location = new Point(24, 91);
			PortLabel.Name = "PortLabel";
			PortLabel.Size = new Size(66, 15);
			PortLabel.TabIndex = 3;
			PortLabel.Text = "Game Port";

			// numPort
			numPort.BackColor = Color.FromArgb(12, 21, 36);
			numPort.Font = new Font("Segoe UI", 10F);
			numPort.ForeColor = Color.FromArgb(245, 247, 251);
			numPort.Location = new Point(24, 113);
			numPort.Maximum = 65535;
			numPort.Minimum = 1024;
			numPort.Name = "numPort";
			numPort.Size = new Size(266, 36);
			numPort.TabIndex = 4;
			numPort.Value = 1024;

			// QueryPortLabel
			QueryPortLabel.AutoSize = true;
			QueryPortLabel.BackColor = Color.FromArgb(17, 27, 45);
			QueryPortLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			QueryPortLabel.ForeColor = Color.FromArgb(245, 247, 251);
			QueryPortLabel.Location = new Point(314, 91);
			QueryPortLabel.Name = "QueryPortLabel";
			QueryPortLabel.Size = new Size(66, 15);
			QueryPortLabel.TabIndex = 5;
			QueryPortLabel.Text = "Query Port";

			// numQueryPort
			numQueryPort.BackColor = Color.FromArgb(12, 21, 36);
			numQueryPort.Font = new Font("Segoe UI", 10F);
			numQueryPort.ForeColor = Color.FromArgb(245, 247, 251);
			numQueryPort.Location = new Point(314, 113);
			numQueryPort.Maximum = 65535;
			numQueryPort.Minimum = 1024;
			numQueryPort.Name = "numQueryPort";
			numQueryPort.Size = new Size(266, 36);
			numQueryPort.TabIndex = 6;
			numQueryPort.Value = 27015;

			// lblAppPort
			lblAppPort.AutoSize = true;
			lblAppPort.BackColor = Color.FromArgb(17, 27, 45);
			lblAppPort.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblAppPort.ForeColor = Color.FromArgb(245, 247, 251);
			lblAppPort.Location = new Point(604, 91);
			lblAppPort.Name = "lblAppPort";
			lblAppPort.Size = new Size(54, 15);
			lblAppPort.TabIndex = 7;
			lblAppPort.Text = "App Port";

			// numAppPort
			numAppPort.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			numAppPort.BackColor = Color.FromArgb(12, 21, 36);
			numAppPort.Font = new Font("Segoe UI", 10F);
			numAppPort.ForeColor = Color.FromArgb(245, 247, 251);
			numAppPort.Location = new Point(604, 113);
			numAppPort.Maximum = 65535;
			numAppPort.Minimum = 10000;
			numAppPort.Name = "numAppPort";
			numAppPort.Size = new Size(286, 36);
			numAppPort.TabIndex = 8;
			numAppPort.Value = 10000;

			// cardRcon
			cardRcon.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardRcon.BackColor = Color.FromArgb(17, 27, 45);
			cardRcon.BorderColor = Color.FromArgb(38, 52, 77);
			cardRcon.Controls.Add(lblRconIcon);
			cardRcon.Controls.Add(lblRconTitle);
			cardRcon.Controls.Add(lblRconDescription);
			cardRcon.Controls.Add(lblRconToggleTitle);
			cardRcon.Controls.Add(chkEnableRcon);
			cardRcon.Controls.Add(lblRCONport);
			cardRcon.Controls.Add(numRconPort);
			cardRcon.Controls.Add(lblRCONpassword);
			cardRcon.Controls.Add(txtRconPassword);
			cardRcon.CornerRadius = 12;
			cardRcon.FillColor = Color.FromArgb(17, 27, 45);
			cardRcon.Location = new Point(0, 206);
			cardRcon.Name = "cardRcon";
			cardRcon.Size = new Size(914, 210);
			cardRcon.TabIndex = 1;

			// lblRconIcon
			lblRconIcon.BackColor = Color.FromArgb(17, 27, 45);
			lblRconIcon.Font = new Font("Segoe UI Symbol", 16F);
			lblRconIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblRconIcon.Location = new Point(20, 14);
			lblRconIcon.Name = "lblRconIcon";
			lblRconIcon.Size = new Size(28, 30);
			lblRconIcon.TabIndex = 0;
			lblRconIcon.Text = "◇";
			lblRconIcon.TextAlign = ContentAlignment.MiddleCenter;

			// lblRconTitle
			lblRconTitle.AutoSize = true;
			lblRconTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblRconTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblRconTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblRconTitle.Location = new Point(54, 19);
			lblRconTitle.Name = "lblRconTitle";
			lblRconTitle.Size = new Size(185, 21);
			lblRconTitle.TabIndex = 1;
			lblRconTitle.Text = "Remote Administration";

			// lblRconDescription
			lblRconDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblRconDescription.BackColor = Color.FromArgb(17, 27, 45);
			lblRconDescription.Font = new Font("Segoe UI", 8.5F);
			lblRconDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblRconDescription.Location = new Point(24, 50);
			lblRconDescription.Name = "lblRconDescription";
			lblRconDescription.Size = new Size(700, 24);
			lblRconDescription.TabIndex = 2;
			lblRconDescription.Text = "Enable RCON only for game templates that support secure remote commands.";

			// lblRconToggleTitle
			lblRconToggleTitle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lblRconToggleTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblRconToggleTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblRconToggleTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblRconToggleTitle.Location = new Point(735, 24);
			lblRconToggleTitle.Name = "lblRconToggleTitle";
			lblRconToggleTitle.Size = new Size(92, 22);
			lblRconToggleTitle.TabIndex = 3;
			lblRconToggleTitle.Text = "Enable RCON";

			// chkEnableRcon
			chkEnableRcon.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			chkEnableRcon.BackColor = Color.FromArgb(17, 27, 45);
			chkEnableRcon.Location = new Point(836, 18);
			chkEnableRcon.Name = "chkEnableRcon";
			chkEnableRcon.Size = new Size(54, 30);
			chkEnableRcon.TabIndex = 4;
			chkEnableRcon.CheckedChanged += chkEnableRcon_CheckedChanged;

			// lblRCONport
			lblRCONport.AutoSize = true;
			lblRCONport.BackColor = Color.FromArgb(17, 27, 45);
			lblRCONport.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblRCONport.ForeColor = Color.FromArgb(245, 247, 251);
			lblRCONport.Location = new Point(24, 95);
			lblRCONport.Name = "lblRCONport";
			lblRCONport.Size = new Size(68, 15);
			lblRCONport.TabIndex = 5;
			lblRCONport.Text = "RCON Port";

			// numRconPort
			numRconPort.BackColor = Color.FromArgb(12, 21, 36);
			numRconPort.Font = new Font("Segoe UI", 10F);
			numRconPort.ForeColor = Color.FromArgb(245, 247, 251);
			numRconPort.Location = new Point(24, 117);
			numRconPort.Maximum = 65535;
			numRconPort.Minimum = 1024;
			numRconPort.Name = "numRconPort";
			numRconPort.Size = new Size(250, 36);
			numRconPort.TabIndex = 6;
			numRconPort.Value = 1024;

			// lblRCONpassword
			lblRCONpassword.AutoSize = true;
			lblRCONpassword.BackColor = Color.FromArgb(17, 27, 45);
			lblRCONpassword.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblRCONpassword.ForeColor = Color.FromArgb(245, 247, 251);
			lblRCONpassword.Location = new Point(298, 95);
			lblRCONpassword.Name = "lblRCONpassword";
			lblRCONpassword.Size = new Size(98, 15);
			lblRCONpassword.TabIndex = 7;
			lblRCONpassword.Text = "RCON Password";

			// txtRconPassword
			txtRconPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			txtRconPassword.AutoSize = false;
			txtRconPassword.BackColor = Color.FromArgb(12, 21, 36);
			txtRconPassword.BorderStyle = BorderStyle.FixedSingle;
			txtRconPassword.Font = new Font("Segoe UI", 10F);
			txtRconPassword.ForeColor = Color.FromArgb(245, 247, 251);
			txtRconPassword.Location = new Point(298, 117);
			txtRconPassword.Name = "txtRconPassword";
			txtRconPassword.Size = new Size(592, 36);
			txtRconPassword.TabIndex = 8;

			// pnlPageAutomation
			pnlPageAutomation.AutoScroll = true;
			pnlPageAutomation.BackColor = Color.FromArgb(8, 13, 24);
			pnlPageAutomation.Controls.Add(cardStartup);
			pnlPageAutomation.Controls.Add(cardSchedule);
			pnlPageAutomation.Controls.Add(cardDiscord);
			pnlPageAutomation.Dock = DockStyle.Fill;
			pnlPageAutomation.Location = new Point(0, 0);
			pnlPageAutomation.Name = "pnlPageAutomation";
			pnlPageAutomation.Size = new Size(914, 440);
			pnlPageAutomation.TabIndex = 3;
			pnlPageAutomation.Visible = false;

			// cardStartup
			cardStartup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardStartup.BackColor = Color.FromArgb(17, 27, 45);
			cardStartup.BorderColor = Color.FromArgb(38, 52, 77);
			cardStartup.Controls.Add(lblStartupIcon);
			cardStartup.Controls.Add(lblStartupTitle);
			cardStartup.Controls.Add(lblUpdateTitle);
			cardStartup.Controls.Add(lblUpdateDescription);
			cardStartup.Controls.Add(chkUpdateOnStart);
			cardStartup.Controls.Add(lblBackupTitle);
			cardStartup.Controls.Add(lblBackupDescription);
			cardStartup.Controls.Add(chkBackupOnStart);
			cardStartup.CornerRadius = 12;
			cardStartup.FillColor = Color.FromArgb(17, 27, 45);
			cardStartup.Location = new Point(0, 0);
			cardStartup.Name = "cardStartup";
			cardStartup.Size = new Size(914, 142);
			cardStartup.TabIndex = 0;

			// lblStartupIcon
			lblStartupIcon.BackColor = Color.FromArgb(17, 27, 45);
			lblStartupIcon.Font = new Font("Segoe UI Symbol", 16F);
			lblStartupIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblStartupIcon.Location = new Point(20, 14);
			lblStartupIcon.Name = "lblStartupIcon";
			lblStartupIcon.Size = new Size(28, 30);
			lblStartupIcon.TabIndex = 0;
			lblStartupIcon.Text = "⚙";
			lblStartupIcon.TextAlign = ContentAlignment.MiddleCenter;

			// lblStartupTitle
			lblStartupTitle.AutoSize = true;
			lblStartupTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblStartupTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblStartupTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblStartupTitle.Location = new Point(54, 19);
			lblStartupTitle.Name = "lblStartupTitle";
			lblStartupTitle.Size = new Size(114, 21);
			lblStartupTitle.TabIndex = 1;
			lblStartupTitle.Text = "Startup Tasks";

			// lblUpdateTitle
			lblUpdateTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblUpdateTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblUpdateTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblUpdateTitle.Location = new Point(24, 62);
			lblUpdateTitle.Name = "lblUpdateTitle";
			lblUpdateTitle.Size = new Size(180, 20);
			lblUpdateTitle.TabIndex = 2;
			lblUpdateTitle.Text = "Update on Start";

			// lblUpdateDescription
			lblUpdateDescription.BackColor = Color.FromArgb(17, 27, 45);
			lblUpdateDescription.Font = new Font("Segoe UI", 8F);
			lblUpdateDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblUpdateDescription.Location = new Point(24, 84);
			lblUpdateDescription.Name = "lblUpdateDescription";
			lblUpdateDescription.Size = new Size(340, 34);
			lblUpdateDescription.TabIndex = 3;
			lblUpdateDescription.Text = "Check SteamCMD for updates before launching the server.";

			// chkUpdateOnStart
			chkUpdateOnStart.BackColor = Color.FromArgb(17, 27, 45);
			chkUpdateOnStart.Location = new Point(374, 70);
			chkUpdateOnStart.Name = "chkUpdateOnStart";
			chkUpdateOnStart.Size = new Size(54, 30);
			chkUpdateOnStart.TabIndex = 4;

			// lblBackupTitle
			lblBackupTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblBackupTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblBackupTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblBackupTitle.Location = new Point(472, 62);
			lblBackupTitle.Name = "lblBackupTitle";
			lblBackupTitle.Size = new Size(180, 20);
			lblBackupTitle.TabIndex = 5;
			lblBackupTitle.Text = "Backup on Start";

			// lblBackupDescription
			lblBackupDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblBackupDescription.BackColor = Color.FromArgb(17, 27, 45);
			lblBackupDescription.Font = new Font("Segoe UI", 8F);
			lblBackupDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblBackupDescription.Location = new Point(472, 84);
			lblBackupDescription.Name = "lblBackupDescription";
			lblBackupDescription.Size = new Size(340, 34);
			lblBackupDescription.TabIndex = 6;
			lblBackupDescription.Text = "Create a protected server backup before each launch.";

			// chkBackupOnStart
			chkBackupOnStart.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			chkBackupOnStart.BackColor = Color.FromArgb(17, 27, 45);
			chkBackupOnStart.Location = new Point(836, 70);
			chkBackupOnStart.Name = "chkBackupOnStart";
			chkBackupOnStart.Size = new Size(54, 30);
			chkBackupOnStart.TabIndex = 7;

			// cardSchedule
			cardSchedule.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardSchedule.BackColor = Color.FromArgb(17, 27, 45);
			cardSchedule.BorderColor = Color.FromArgb(38, 52, 77);
			cardSchedule.Controls.Add(lblScheduleIcon);
			cardSchedule.Controls.Add(lblScheduleTitle);
			cardSchedule.Controls.Add(lblScheduleDescription);
			cardSchedule.Controls.Add(chkEnableSchedule);
			cardSchedule.Controls.Add(btnEditSchedule);
			cardSchedule.CornerRadius = 12;
			cardSchedule.FillColor = Color.FromArgb(17, 27, 45);
			cardSchedule.Location = new Point(0, 158);
			cardSchedule.Name = "cardSchedule";
			cardSchedule.Size = new Size(914, 118);
			cardSchedule.TabIndex = 1;

			// lblScheduleIcon
			lblScheduleIcon.BackColor = Color.FromArgb(17, 27, 45);
			lblScheduleIcon.Font = new Font("Segoe UI Symbol", 16F);
			lblScheduleIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblScheduleIcon.Location = new Point(20, 14);
			lblScheduleIcon.Name = "lblScheduleIcon";
			lblScheduleIcon.Size = new Size(28, 30);
			lblScheduleIcon.TabIndex = 0;
			lblScheduleIcon.Text = "◷";
			lblScheduleIcon.TextAlign = ContentAlignment.MiddleCenter;

			// lblScheduleTitle
			lblScheduleTitle.AutoSize = true;
			lblScheduleTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblScheduleTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblScheduleTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblScheduleTitle.Location = new Point(54, 19);
			lblScheduleTitle.Name = "lblScheduleTitle";
			lblScheduleTitle.Size = new Size(151, 21);
			lblScheduleTitle.TabIndex = 1;
			lblScheduleTitle.Text = "Scheduled Restarts";

			// lblScheduleDescription
			lblScheduleDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblScheduleDescription.BackColor = Color.FromArgb(17, 27, 45);
			lblScheduleDescription.Font = new Font("Segoe UI", 8.5F);
			lblScheduleDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblScheduleDescription.Location = new Point(24, 58);
			lblScheduleDescription.Name = "lblScheduleDescription";
			lblScheduleDescription.Size = new Size(560, 38);
			lblScheduleDescription.TabIndex = 2;
			lblScheduleDescription.Text = "Restart selected days at a configured time while preserving the current scheduler data.";

			// chkEnableSchedule
			chkEnableSchedule.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			chkEnableSchedule.BackColor = Color.FromArgb(17, 27, 45);
			chkEnableSchedule.Location = new Point(662, 43);
			chkEnableSchedule.Name = "chkEnableSchedule";
			chkEnableSchedule.Size = new Size(54, 30);
			chkEnableSchedule.TabIndex = 3;
			chkEnableSchedule.CheckedChanged += chkEnableSchedule_CheckedChanged;

			// btnEditSchedule
			btnEditSchedule.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnEditSchedule.BackColor = Color.FromArgb(12, 21, 36);
			btnEditSchedule.ForeColor = Color.FromArgb(245, 247, 251);
			btnEditSchedule.Location = new Point(735, 36);
			btnEditSchedule.Name = "btnEditSchedule";
			btnEditSchedule.Size = new Size(155, 42);
			btnEditSchedule.TabIndex = 4;
			btnEditSchedule.Text = "Configure Schedule";
			btnEditSchedule.Click += btnEditSchedule_Click;

			// cardDiscord
			cardDiscord.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardDiscord.BackColor = Color.FromArgb(17, 27, 45);
			cardDiscord.BorderColor = Color.FromArgb(38, 52, 77);
			cardDiscord.Controls.Add(lblDiscordIcon);
			cardDiscord.Controls.Add(lblDiscordTitle);
			cardDiscord.Controls.Add(lblDiscordDescription);
			cardDiscord.Controls.Add(chkEnableDiscord);
			cardDiscord.Controls.Add(txtDiscordWebhook);
			cardDiscord.Controls.Add(btnTestDiscord);
			cardDiscord.CornerRadius = 12;
			cardDiscord.FillColor = Color.FromArgb(17, 27, 45);
			cardDiscord.Location = new Point(0, 292);
			cardDiscord.Name = "cardDiscord";
			cardDiscord.Size = new Size(914, 154);
			cardDiscord.TabIndex = 2;

			// lblDiscordIcon
			lblDiscordIcon.BackColor = Color.FromArgb(17, 27, 45);
			lblDiscordIcon.Font = new Font("Segoe UI Symbol", 16F);
			lblDiscordIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblDiscordIcon.Location = new Point(20, 14);
			lblDiscordIcon.Name = "lblDiscordIcon";
			lblDiscordIcon.Size = new Size(28, 30);
			lblDiscordIcon.TabIndex = 0;
			lblDiscordIcon.Text = "✉";
			lblDiscordIcon.TextAlign = ContentAlignment.MiddleCenter;

			// lblDiscordTitle
			lblDiscordTitle.AutoSize = true;
			lblDiscordTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblDiscordTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblDiscordTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblDiscordTitle.Location = new Point(54, 19);
			lblDiscordTitle.Name = "lblDiscordTitle";
			lblDiscordTitle.Size = new Size(116, 21);
			lblDiscordTitle.TabIndex = 1;
			lblDiscordTitle.Text = "Discord Alerts";

			// lblDiscordDescription
			lblDiscordDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblDiscordDescription.BackColor = Color.FromArgb(17, 27, 45);
			lblDiscordDescription.Font = new Font("Segoe UI", 8.5F);
			lblDiscordDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblDiscordDescription.Location = new Point(24, 49);
			lblDiscordDescription.Name = "lblDiscordDescription";
			lblDiscordDescription.Size = new Size(720, 22);
			lblDiscordDescription.TabIndex = 2;
			lblDiscordDescription.Text = "Send server status notifications through an existing Discord webhook.";

			// chkEnableDiscord
			chkEnableDiscord.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			chkEnableDiscord.BackColor = Color.FromArgb(17, 27, 45);
			chkEnableDiscord.Location = new Point(836, 18);
			chkEnableDiscord.Name = "chkEnableDiscord";
			chkEnableDiscord.Size = new Size(54, 30);
			chkEnableDiscord.TabIndex = 3;
			chkEnableDiscord.CheckedChanged += chkEnableDiscord_CheckedChanged;

			// txtDiscordWebhook
			txtDiscordWebhook.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			txtDiscordWebhook.AutoSize = false;
			txtDiscordWebhook.BackColor = Color.FromArgb(12, 21, 36);
			txtDiscordWebhook.BorderStyle = BorderStyle.FixedSingle;
			txtDiscordWebhook.Font = new Font("Segoe UI", 10F);
			txtDiscordWebhook.ForeColor = Color.FromArgb(245, 247, 251);
			txtDiscordWebhook.Location = new Point(24, 87);
			txtDiscordWebhook.Name = "txtDiscordWebhook";
			txtDiscordWebhook.Size = new Size(690, 36);
			txtDiscordWebhook.TabIndex = 4;

			// btnTestDiscord
			btnTestDiscord.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnTestDiscord.BackColor = Color.FromArgb(12, 21, 36);
			btnTestDiscord.Enabled = false;
			btnTestDiscord.ForeColor = Color.FromArgb(245, 247, 251);
			btnTestDiscord.Location = new Point(735, 84);
			btnTestDiscord.Name = "btnTestDiscord";
			btnTestDiscord.Size = new Size(155, 42);
			btnTestDiscord.TabIndex = 5;
			btnTestDiscord.Text = "Test Connection";
			btnTestDiscord.Click += btnTestDiscord_Click;

			// pnlPageInstall
			pnlPageInstall.AutoScroll = true;
			pnlPageInstall.BackColor = Color.FromArgb(8, 13, 24);
			pnlPageInstall.Controls.Add(cardInstallLocation);
			pnlPageInstall.Controls.Add(cardLaunchArguments);
			pnlPageInstall.Dock = DockStyle.Fill;
			pnlPageInstall.Location = new Point(0, 0);
			pnlPageInstall.Name = "pnlPageInstall";
			pnlPageInstall.Size = new Size(914, 440);
			pnlPageInstall.TabIndex = 4;
			pnlPageInstall.Visible = false;

			// cardInstallLocation
			cardInstallLocation.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardInstallLocation.BackColor = Color.FromArgb(17, 27, 45);
			cardInstallLocation.BorderColor = Color.FromArgb(38, 52, 77);
			cardInstallLocation.Controls.Add(lblInstallIcon);
			cardInstallLocation.Controls.Add(lblInstallTitle);
			cardInstallLocation.Controls.Add(lblDefaultPathTitle);
			cardInstallLocation.Controls.Add(lblDefaultPathDescription);
			cardInstallLocation.Controls.Add(chkDefaultPath);
			cardInstallLocation.Controls.Add(FolderPathLabel);
			cardInstallLocation.Controls.Add(txtInstallPath);
			cardInstallLocation.Controls.Add(btnBrowse);
			cardInstallLocation.CornerRadius = 12;
			cardInstallLocation.FillColor = Color.FromArgb(17, 27, 45);
			cardInstallLocation.Location = new Point(0, 0);
			cardInstallLocation.Name = "cardInstallLocation";
			cardInstallLocation.Size = new Size(914, 174);
			cardInstallLocation.TabIndex = 0;

			// lblInstallIcon
			lblInstallIcon.BackColor = Color.FromArgb(17, 27, 45);
			lblInstallIcon.Font = new Font("Segoe UI Symbol", 16F);
			lblInstallIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblInstallIcon.Location = new Point(20, 14);
			lblInstallIcon.Name = "lblInstallIcon";
			lblInstallIcon.Size = new Size(28, 30);
			lblInstallIcon.TabIndex = 0;
			lblInstallIcon.Text = "➜";
			lblInstallIcon.TextAlign = ContentAlignment.MiddleCenter;

			// lblInstallTitle
			lblInstallTitle.AutoSize = true;
			lblInstallTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblInstallTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblInstallTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblInstallTitle.Location = new Point(54, 19);
			lblInstallTitle.Name = "lblInstallTitle";
			lblInstallTitle.Size = new Size(127, 21);
			lblInstallTitle.TabIndex = 1;
			lblInstallTitle.Text = "Install Location";

			// lblDefaultPathTitle
			lblDefaultPathTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblDefaultPathTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblDefaultPathTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblDefaultPathTitle.Location = new Point(24, 52);
			lblDefaultPathTitle.Name = "lblDefaultPathTitle";
			lblDefaultPathTitle.Size = new Size(190, 20);
			lblDefaultPathTitle.TabIndex = 2;
			lblDefaultPathTitle.Text = "Use Synix default folder";

			// lblDefaultPathDescription
			lblDefaultPathDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblDefaultPathDescription.BackColor = Color.FromArgb(17, 27, 45);
			lblDefaultPathDescription.Font = new Font("Segoe UI", 8F);
			lblDefaultPathDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblDefaultPathDescription.Location = new Point(220, 52);
			lblDefaultPathDescription.Name = "lblDefaultPathDescription";
			lblDefaultPathDescription.Size = new Size(570, 22);
			lblDefaultPathDescription.TabIndex = 3;
			lblDefaultPathDescription.Text = "Automatically builds a safe game/server folder below the configured Games path.";

			// chkDefaultPath
			chkDefaultPath.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			chkDefaultPath.BackColor = Color.FromArgb(17, 27, 45);
			chkDefaultPath.Location = new Point(836, 45);
			chkDefaultPath.Name = "chkDefaultPath";
			chkDefaultPath.Size = new Size(54, 30);
			chkDefaultPath.TabIndex = 4;
			chkDefaultPath.CheckedChanged += chkDefaultPath_CheckedChanged;

			// FolderPathLabel
			FolderPathLabel.AutoSize = true;
			FolderPathLabel.BackColor = Color.FromArgb(17, 27, 45);
			FolderPathLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			FolderPathLabel.ForeColor = Color.FromArgb(245, 247, 251);
			FolderPathLabel.Location = new Point(24, 92);
			FolderPathLabel.Name = "FolderPathLabel";
			FolderPathLabel.Size = new Size(76, 15);
			FolderPathLabel.TabIndex = 5;
			FolderPathLabel.Text = "Folder Path";

			// txtInstallPath
			txtInstallPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			txtInstallPath.AutoSize = false;
			txtInstallPath.BackColor = Color.FromArgb(12, 21, 36);
			txtInstallPath.BorderStyle = BorderStyle.None;
			txtInstallPath.Cursor = Cursors.Default;
			txtInstallPath.Font = new Font("Segoe UI", 9.5F);
			txtInstallPath.ForeColor = Color.FromArgb(158, 172, 194);
			txtInstallPath.Location = new Point(24, 113);
			txtInstallPath.Name = "txtInstallPath";
			txtInstallPath.ReadOnly = true;
			txtInstallPath.ShortcutsEnabled = false;
			txtInstallPath.Size = new Size(690, 36);
			txtInstallPath.TabIndex = 6;
			txtInstallPath.TabStop = false;
			txtInstallPath.TextChanged += txtInstallPath_TextChanged;

			// btnBrowse
			btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnBrowse.BackColor = Color.FromArgb(12, 21, 36);
			btnBrowse.ForeColor = Color.FromArgb(245, 247, 251);
			btnBrowse.Location = new Point(735, 110);
			btnBrowse.Name = "btnBrowse";
			btnBrowse.Size = new Size(155, 42);
			btnBrowse.TabIndex = 7;
			btnBrowse.Text = "Browse Folder";
			btnBrowse.Click += btnBrowse_Click;

			// cardLaunchArguments
			cardLaunchArguments.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardLaunchArguments.BackColor = Color.FromArgb(17, 27, 45);
			cardLaunchArguments.BorderColor = Color.FromArgb(38, 52, 77);
			cardLaunchArguments.Controls.Add(lblLaunchIcon);
			cardLaunchArguments.Controls.Add(lblLaunchTitle);
			cardLaunchArguments.Controls.Add(lblaruments);
			cardLaunchArguments.Controls.Add(btnViewArgs);
			cardLaunchArguments.Controls.Add(TextLabel3);
			cardLaunchArguments.Controls.Add(TextLabel7);
			cardLaunchArguments.Controls.Add(txtExtraArgs);
			cardLaunchArguments.CornerRadius = 12;
			cardLaunchArguments.FillColor = Color.FromArgb(17, 27, 45);
			cardLaunchArguments.Location = new Point(0, 190);
			cardLaunchArguments.Name = "cardLaunchArguments";
			cardLaunchArguments.Size = new Size(914, 256);
			cardLaunchArguments.TabIndex = 1;

			// lblLaunchIcon
			lblLaunchIcon.BackColor = Color.FromArgb(17, 27, 45);
			lblLaunchIcon.Font = new Font("Segoe UI Symbol", 16F);
			lblLaunchIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblLaunchIcon.Location = new Point(20, 14);
			lblLaunchIcon.Name = "lblLaunchIcon";
			lblLaunchIcon.Size = new Size(28, 30);
			lblLaunchIcon.TabIndex = 0;
			lblLaunchIcon.Text = ">_";
			lblLaunchIcon.TextAlign = ContentAlignment.MiddleCenter;

			// lblLaunchTitle
			lblLaunchTitle.AutoSize = true;
			lblLaunchTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblLaunchTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblLaunchTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblLaunchTitle.Location = new Point(54, 19);
			lblLaunchTitle.Name = "lblLaunchTitle";
			lblLaunchTitle.Size = new Size(136, 21);
			lblLaunchTitle.TabIndex = 1;
			lblLaunchTitle.Text = "Launch Arguments";

			// lblaruments
			lblaruments.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblaruments.BackColor = Color.FromArgb(17, 27, 45);
			lblaruments.Font = new Font("Segoe UI", 8.5F);
			lblaruments.ForeColor = Color.FromArgb(158, 172, 194);
			lblaruments.Location = new Point(24, 52);
			lblaruments.Name = "lblaruments";
			lblaruments.Size = new Size(665, 55);
			lblaruments.TabIndex = 2;
			lblaruments.Text = resources.GetString("lblaruments.Text");

			// btnViewArgs
			btnViewArgs.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnViewArgs.BackColor = Color.FromArgb(12, 21, 36);
			btnViewArgs.ForeColor = Color.FromArgb(245, 247, 251);
			btnViewArgs.Location = new Point(710, 58);
			btnViewArgs.Name = "btnViewArgs";
			btnViewArgs.Size = new Size(180, 42);
			btnViewArgs.TabIndex = 3;
			btnViewArgs.Text = "View Default Arguments";
			btnViewArgs.Click += btnViewArgs_Click;

			// TextLabel3
			TextLabel3.AutoSize = true;
			TextLabel3.BackColor = Color.FromArgb(17, 27, 45);
			TextLabel3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			TextLabel3.ForeColor = Color.FromArgb(245, 247, 251);
			TextLabel3.Location = new Point(24, 122);
			TextLabel3.Name = "TextLabel3";
			TextLabel3.Size = new Size(99, 15);
			TextLabel3.TabIndex = 4;
			TextLabel3.Text = "Extra Arguments";

			// TextLabel7
			TextLabel7.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			TextLabel7.BackColor = Color.FromArgb(17, 27, 45);
			TextLabel7.Font = new Font("Segoe UI", 8F);
			TextLabel7.ForeColor = Color.FromArgb(158, 172, 194);
			TextLabel7.Location = new Point(140, 121);
			TextLabel7.Name = "TextLabel7";
			TextLabel7.Size = new Size(750, 20);
			TextLabel7.TabIndex = 5;
			TextLabel7.Text = "Optional flags only — for example: -log, -nosteamclient, or -forceupdate";

			// txtExtraArgs
			txtExtraArgs.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			txtExtraArgs.BackColor = Color.FromArgb(12, 21, 36);
			txtExtraArgs.BorderStyle = BorderStyle.FixedSingle;
			txtExtraArgs.Font = new Font("Cascadia Mono", 9F);
			txtExtraArgs.ForeColor = Color.FromArgb(245, 247, 251);
			txtExtraArgs.Location = new Point(24, 149);
			txtExtraArgs.Multiline = true;
			txtExtraArgs.Name = "txtExtraArgs";
			txtExtraArgs.ScrollBars = ScrollBars.Vertical;
			txtExtraArgs.Size = new Size(866, 78);
			txtExtraArgs.TabIndex = 6;

			// ServerSettingsGUI
			AcceptButton = btnSave;
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			BackColor = Color.FromArgb(8, 13, 24);
			CancelButton = btnCancel;
			ClientSize = new Size(1180, 780);
			Controls.Add(pnlBody);
			Controls.Add(pnlFooter);
			Controls.Add(pnlTitleBar);
			Font = new Font("Segoe UI", 9F);
			ForeColor = Color.FromArgb(245, 247, 251);
			FormBorderStyle = FormBorderStyle.None;
			Icon = (Icon)resources.GetObject("$this.Icon");
			KeyPreview = true;
			MaximizeBox = false;
			MinimizeBox = false;
			MinimumSize = new Size(1100, 720);
			Name = "ServerSettingsGUI";
			StartPosition = FormStartPosition.CenterParent;
			Text = "Server Setup";

			((System.ComponentModel.ISupportInitialize)numMaxPlayers).EndInit();
			((System.ComponentModel.ISupportInitialize)numRam).EndInit();
			((System.ComponentModel.ISupportInitialize)numWorldSize).EndInit();
			((System.ComponentModel.ISupportInitialize)numPort).EndInit();
			((System.ComponentModel.ISupportInitialize)numQueryPort).EndInit();
			((System.ComponentModel.ISupportInitialize)numAppPort).EndInit();
			((System.ComponentModel.ISupportInitialize)numRconPort).EndInit();
			cardLaunchArguments.ResumeLayout(false);
			cardLaunchArguments.PerformLayout();
			cardInstallLocation.ResumeLayout(false);
			cardInstallLocation.PerformLayout();
			pnlPageInstall.ResumeLayout(false);
			cardDiscord.ResumeLayout(false);
			cardDiscord.PerformLayout();
			cardSchedule.ResumeLayout(false);
			cardSchedule.PerformLayout();
			cardStartup.ResumeLayout(false);
			cardStartup.PerformLayout();
			pnlPageAutomation.ResumeLayout(false);
			cardRcon.ResumeLayout(false);
			cardRcon.PerformLayout();
			cardPorts.ResumeLayout(false);
			cardPorts.PerformLayout();
			pnlPageNetwork.ResumeLayout(false);
			cardWorldGeneration.ResumeLayout(false);
			cardWorldGeneration.PerformLayout();
			pnlPageWorld.ResumeLayout(false);
			cardCredentials.ResumeLayout(false);
			cardCredentials.PerformLayout();
			cardMinecraftRuntime.ResumeLayout(false);
			cardMinecraftRuntime.PerformLayout();
			cardGameplay.ResumeLayout(false);
			cardGameplay.PerformLayout();
			cardIdentity.ResumeLayout(false);
			cardIdentity.PerformLayout();
			pnlPageGeneral.ResumeLayout(false);
			pnlPageHost.ResumeLayout(false);
			pnlContent.ResumeLayout(false);
			pnlContent.PerformLayout();
			pnlSidebarStatus.ResumeLayout(false);
			pnlSidebar.ResumeLayout(false);
			pnlBody.ResumeLayout(false);
			pnlFooter.ResumeLayout(false);
			pnlTitleBar.ResumeLayout(false);
			pnlTitleBar.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		private Panel pnlTitleBar;
		private Label lblBrand;
		private Label lblWindowTitle;
		private Button btnTitleMinimize;
		private Button btnTitleClose;
		private Panel pnlFooter;
		private Label lblFooterStatus;
		private ModernSettingsButton btnCancel;
		private ModernSettingsButton btnSave;
		private Panel pnlBody;
		private Panel pnlSidebar;
		private Label lblSidebarSection;
		private ModernSettingsNavButton btnNavGeneral;
		private ModernSettingsNavButton btnNavWorld;
		private ModernSettingsNavButton btnNavNetwork;
		private ModernSettingsNavButton btnNavAutomation;
		private ModernSettingsNavButton btnNavInstall;
		private Panel pnlSidebarStatus;
		private Panel pnlSidebarDivider;
		private Label lblSidebarStatusHeading;
		private Label lblSidebarStatus;
		private Label lblSidebarStatusDetail;
		private Panel pnlContent;
		private Label lblPageTitle;
		private Label lblPageDescription;
		private Label lblModeBadge;
		private Label lblTemplateBehavior;
		private Panel pnlPageHost;
		private Panel pnlPageGeneral;
		private ModernSettingsCard cardIdentity;
		private Label lblIdentityIcon;
		private Label lblIdentityTitle;
		private Label ServerNameLabel;
		private TextBox txtName;
		private Label GameServerLabel;
		private ModernSettingsComboBox cmbGame;
		private Label lblGameVersion;
		private ModernSettingsComboBox cmbGameVersion;
		private Label lblIdentityHelper;
		private ModernSettingsCard cardGameplay;
		private Label lblGameplayIcon;
		private Label lblGameplayTitle;
		private Label MapLabel;
		private ModernSettingsComboBox cmbWorldName;
		private Label lblCompetitive;
		private ModernSettingsComboBox cmbCompetitive;
		private Label MaxPlayerLabel;
		private ModernSettingsNumericUpDown numMaxPlayers;
		private Label label1;
		private ModernSettingsNumericUpDown numRam;
		private Label lblGameplayHelper;
		private ModernSettingsCard cardMinecraftRuntime;
		private Label lblMinecraftRuntimeIcon;
		private Label lblMinecraftRuntimeTitle;
		private Label lblMinecraftLoader;
		private ModernSettingsComboBox cmbMinecraftLoader;
		private Label lblMinecraftLoaderVersion;
		private ModernSettingsComboBox cmbMinecraftLoaderVersion;
		private Label lblMinecraftJava;
		private Label lblMinecraftJavaValue;
		private Label lblMinecraftRuntimeHelper;
		private ModernSettingsCard cardCredentials;
		private Label lblCredentialsIcon;
		private Label lblCredentialsTitle;
		private Label lblPassword;
		private TextBox txtPassword;
		private Label lblAdminPassword;
		private TextBox txtAdminPassword;
		private Label lblCredentialsNote;
		private Panel pnlPageWorld;
		private ModernSettingsCard cardWorldGeneration;
		private Label lblWorldIcon;
		private Label lblWorldTitle;
		private Label lblWorldDescription;
		private Label lblWorldSeed;
		private TextBox txtWorldSeed;
		private Label lblWorldSize;
		private ModernSettingsNumericUpDown numWorldSize;
		private Panel pnlPageNetwork;
		private ModernSettingsCard cardPorts;
		private Label lblPortsIcon;
		private Label lblPortsTitle;
		private Label lblPortsDescription;
		private Label PortLabel;
		private ModernSettingsNumericUpDown numPort;
		private Label QueryPortLabel;
		private ModernSettingsNumericUpDown numQueryPort;
		private Label lblAppPort;
		private ModernSettingsNumericUpDown numAppPort;
		private ModernSettingsCard cardRcon;
		private Label lblRconIcon;
		private Label lblRconTitle;
		private Label lblRconDescription;
		private Label lblRconToggleTitle;
		private ModernSettingsToggle chkEnableRcon;
		private Label lblRCONport;
		private ModernSettingsNumericUpDown numRconPort;
		private Label lblRCONpassword;
		private TextBox txtRconPassword;
		private Panel pnlPageAutomation;
		private ModernSettingsCard cardStartup;
		private Label lblStartupIcon;
		private Label lblStartupTitle;
		private Label lblUpdateTitle;
		private Label lblUpdateDescription;
		private ModernSettingsToggle chkUpdateOnStart;
		private Label lblBackupTitle;
		private Label lblBackupDescription;
		private ModernSettingsToggle chkBackupOnStart;
		private ModernSettingsCard cardSchedule;
		private Label lblScheduleIcon;
		private Label lblScheduleTitle;
		private Label lblScheduleDescription;
		private ModernSettingsToggle chkEnableSchedule;
		private ModernSettingsButton btnEditSchedule;
		private ModernSettingsCard cardDiscord;
		private Label lblDiscordIcon;
		private Label lblDiscordTitle;
		private Label lblDiscordDescription;
		private ModernSettingsToggle chkEnableDiscord;
		private TextBox txtDiscordWebhook;
		private ModernSettingsButton btnTestDiscord;
		private Panel pnlPageInstall;
		private ModernSettingsCard cardInstallLocation;
		private Label lblInstallIcon;
		private Label lblInstallTitle;
		private Label lblDefaultPathTitle;
		private Label lblDefaultPathDescription;
		private ModernSettingsToggle chkDefaultPath;
		private Label FolderPathLabel;
		private TextBox txtInstallPath;
		private ModernSettingsButton btnBrowse;
		private ModernSettingsCard cardLaunchArguments;
		private Label lblLaunchIcon;
		private Label lblLaunchTitle;
		private Label lblaruments;
		private ModernSettingsButton btnViewArgs;
		private Label TextLabel3;
		private Label TextLabel7;
		private TextBox txtExtraArgs;
	}
}
