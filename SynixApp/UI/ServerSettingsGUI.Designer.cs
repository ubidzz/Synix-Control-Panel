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
			btnNavSecurity = new ModernSettingsNavButton();
			btnNavWorld = new ModernSettingsNavButton();
			btnNavNetwork = new ModernSettingsNavButton();
			btnNavAutomation = new ModernSettingsNavButton();
			btnNavDiscord = new ModernSettingsNavButton();
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
			pnlPageGeneral = new ServerSettingsGeneralPage();
			pnlPageSecurity = new ServerSettingsSecurityPage();
			pnlPageWorld = new ServerSettingsWorldPage();
			pnlPageNetwork = new ServerSettingsNetworkPage();
			pnlPageAutomation = new ServerSettingsAutomationPage();
			pnlPageDiscord = new Panel();
			discordSettingsPage = new DiscordSettingsPage();
			pnlPageInstall = new ServerSettingsInstallPage();
			pnlTitleBar.SuspendLayout();
			pnlFooter.SuspendLayout();
			pnlBody.SuspendLayout();
			pnlSidebar.SuspendLayout();
			pnlSidebarStatus.SuspendLayout();
			pnlContent.SuspendLayout();
			pnlPageHost.SuspendLayout();
			pnlPageDiscord.SuspendLayout();
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
			pnlSidebar.Controls.Add(btnNavSecurity);
			pnlSidebar.Controls.Add(btnNavWorld);
			pnlSidebar.Controls.Add(btnNavNetwork);
			pnlSidebar.Controls.Add(btnNavAutomation);
			pnlSidebar.Controls.Add(btnNavDiscord);
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
			btnNavGeneral.Size = new Size(186, 46);
			btnNavGeneral.TabIndex = 1;
			btnNavGeneral.Text = "General";
			btnNavGeneral.Click += btnNavGeneral_Click;

			// btnNavSecurity
			btnNavSecurity.BackColor = Color.FromArgb(10, 18, 32);
			btnNavSecurity.Font = new Font("Segoe UI", 10F);
			btnNavSecurity.ForeColor = Color.FromArgb(158, 172, 194);
			btnNavSecurity.IconGlyph = "◇";
			btnNavSecurity.Location = new Point(12, 108);
			btnNavSecurity.Name = "btnNavSecurity";
			btnNavSecurity.Size = new Size(186, 46);
			btnNavSecurity.TabIndex = 2;
			btnNavSecurity.Text = "Security";
			btnNavSecurity.Click += btnNavSecurity_Click;

			// btnNavWorld
			btnNavWorld.BackColor = Color.FromArgb(10, 18, 32);
			btnNavWorld.Font = new Font("Segoe UI", 10F);
			btnNavWorld.ForeColor = Color.FromArgb(158, 172, 194);
			btnNavWorld.IconGlyph = "◎";
			btnNavWorld.Location = new Point(12, 158);
			btnNavWorld.Name = "btnNavWorld";
			btnNavWorld.Size = new Size(186, 46);
			btnNavWorld.TabIndex = 3;
			btnNavWorld.Text = "World Generation";
			btnNavWorld.Click += btnNavWorld_Click;

			// btnNavNetwork
			btnNavNetwork.BackColor = Color.FromArgb(10, 18, 32);
			btnNavNetwork.Font = new Font("Segoe UI", 10F);
			btnNavNetwork.ForeColor = Color.FromArgb(158, 172, 194);
			btnNavNetwork.IconGlyph = "⌘";
			btnNavNetwork.Location = new Point(12, 208);
			btnNavNetwork.Name = "btnNavNetwork";
			btnNavNetwork.Size = new Size(186, 46);
			btnNavNetwork.TabIndex = 4;
			btnNavNetwork.Text = "Network & RCON";
			btnNavNetwork.Click += btnNavNetwork_Click;

			// btnNavAutomation
			btnNavAutomation.BackColor = Color.FromArgb(10, 18, 32);
			btnNavAutomation.Font = new Font("Segoe UI", 10F);
			btnNavAutomation.ForeColor = Color.FromArgb(158, 172, 194);
			btnNavAutomation.IconGlyph = "⚙";
			btnNavAutomation.Location = new Point(12, 258);
			btnNavAutomation.Name = "btnNavAutomation";
			btnNavAutomation.Size = new Size(186, 46);
			btnNavAutomation.TabIndex = 5;
			btnNavAutomation.Text = "Automation";
			btnNavAutomation.Click += btnNavAutomation_Click;

			// btnNavDiscord
			btnNavDiscord.BackColor = Color.FromArgb(10, 18, 32);
			btnNavDiscord.Font = new Font("Segoe UI", 10F);
			btnNavDiscord.ForeColor = Color.FromArgb(158, 172, 194);
			btnNavDiscord.IconGlyph = "✉";
			btnNavDiscord.Location = new Point(12, 308);
			btnNavDiscord.Name = "btnNavDiscord";
			btnNavDiscord.Size = new Size(186, 46);
			btnNavDiscord.TabIndex = 6;
			btnNavDiscord.Text = "Discord";
			btnNavDiscord.Click += btnNavDiscord_Click;

			// btnNavInstall
			btnNavInstall.BackColor = Color.FromArgb(10, 18, 32);
			btnNavInstall.Font = new Font("Segoe UI", 10F);
			btnNavInstall.ForeColor = Color.FromArgb(158, 172, 194);
			btnNavInstall.IconGlyph = "➜";
			btnNavInstall.Location = new Point(12, 358);
			btnNavInstall.Name = "btnNavInstall";
			btnNavInstall.Size = new Size(186, 46);
			btnNavInstall.TabIndex = 7;
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
			pnlPageHost.Controls.Add(pnlPageSecurity);
			pnlPageHost.Controls.Add(pnlPageWorld);
			pnlPageHost.Controls.Add(pnlPageNetwork);
			pnlPageHost.Controls.Add(pnlPageAutomation);
			pnlPageHost.Controls.Add(pnlPageDiscord);
			pnlPageHost.Controls.Add(pnlPageInstall);
			pnlPageHost.Location = new Point(28, 136);
			pnlPageHost.Name = "pnlPageHost";
			pnlPageHost.Size = new Size(914, 496);
			pnlPageHost.TabIndex = 5;

			// pnlPageGeneral
			pnlPageGeneral.BackColor = Color.FromArgb(8, 13, 24);
			pnlPageGeneral.Dock = DockStyle.Fill;
			pnlPageGeneral.Location = new Point(0, 0);
			pnlPageGeneral.Name = "pnlPageGeneral";
			pnlPageGeneral.Size = new Size(914, 496);
			pnlPageGeneral.TabIndex = 0;
			// pnlPageSecurity
			pnlPageSecurity.BackColor = Color.FromArgb(8, 13, 24);
			pnlPageSecurity.Dock = DockStyle.Fill;
			pnlPageSecurity.Location = new Point(0, 0);
			pnlPageSecurity.Name = "pnlPageSecurity";
			pnlPageSecurity.Size = new Size(914, 496);
			pnlPageSecurity.TabIndex = 1;
			// pnlPageWorld
			pnlPageWorld.BackColor = Color.FromArgb(8, 13, 24);
			pnlPageWorld.Dock = DockStyle.Fill;
			pnlPageWorld.Location = new Point(0, 0);
			pnlPageWorld.Name = "pnlPageWorld";
			pnlPageWorld.Size = new Size(914, 496);
			pnlPageWorld.TabIndex = 2;
			// pnlPageNetwork
			pnlPageNetwork.BackColor = Color.FromArgb(8, 13, 24);
			pnlPageNetwork.Dock = DockStyle.Fill;
			pnlPageNetwork.Location = new Point(0, 0);
			pnlPageNetwork.Name = "pnlPageNetwork";
			pnlPageNetwork.Size = new Size(914, 496);
			pnlPageNetwork.TabIndex = 3;
			// pnlPageAutomation
			pnlPageAutomation.BackColor = Color.FromArgb(8, 13, 24);
			pnlPageAutomation.Dock = DockStyle.Fill;
			pnlPageAutomation.Location = new Point(0, 0);
			pnlPageAutomation.Name = "pnlPageAutomation";
			pnlPageAutomation.Size = new Size(914, 496);
			pnlPageAutomation.TabIndex = 4;
			// pnlPageDiscord
			pnlPageDiscord.BackColor = Color.FromArgb(8, 13, 24);
			pnlPageDiscord.Controls.Add(discordSettingsPage);
			pnlPageDiscord.Dock = DockStyle.Fill;
			pnlPageDiscord.Location = new Point(0, 0);
			pnlPageDiscord.Name = "pnlPageDiscord";
			pnlPageDiscord.Size = new Size(914, 440);
			pnlPageDiscord.TabIndex = 4;
			pnlPageDiscord.Visible = false;

			// discordSettingsPage
			discordSettingsPage.BackColor = Color.FromArgb(8, 13, 24);
			discordSettingsPage.Dock = DockStyle.Fill;
			discordSettingsPage.Location = new Point(0, 0);
			discordSettingsPage.Name = "discordSettingsPage";
			discordSettingsPage.Size = new Size(914, 440);
			discordSettingsPage.TabIndex = 0;

			// pnlPageInstall
			pnlPageInstall.BackColor = Color.FromArgb(8, 13, 24);
			pnlPageInstall.Dock = DockStyle.Fill;
			pnlPageInstall.Location = new Point(0, 0);
			pnlPageInstall.Name = "pnlPageInstall";
			pnlPageInstall.Size = new Size(914, 496);
			pnlPageInstall.TabIndex = 6;
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
			pnlPageDiscord.ResumeLayout(false);
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
		private ModernSettingsNavButton btnNavSecurity;
		private ModernSettingsNavButton btnNavWorld;
		private ModernSettingsNavButton btnNavNetwork;
		private ModernSettingsNavButton btnNavAutomation;
		private ModernSettingsNavButton btnNavDiscord;
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
		private ServerSettingsGeneralPage pnlPageGeneral;
		private ServerSettingsSecurityPage pnlPageSecurity;
		private ServerSettingsWorldPage pnlPageWorld;
		private ServerSettingsNetworkPage pnlPageNetwork;
		private ServerSettingsAutomationPage pnlPageAutomation;
		private Panel pnlPageDiscord;
		private DiscordSettingsPage discordSettingsPage;
		private ServerSettingsInstallPage pnlPageInstall;

	}
}
