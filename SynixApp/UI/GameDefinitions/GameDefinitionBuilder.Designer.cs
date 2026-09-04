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

#nullable enable
#pragma warning disable CS8600

namespace Synix_Control_Panel.SynixApp.UI.GameDefinitions
{
	partial class GameDefinitionBuilder
	{
		private System.ComponentModel.IContainer? components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				components?.Dispose();
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameDefinitionBuilder));
			lblTitle = new Label();
			lblDescription = new Label();
			pnlInputs = new Panel();
			lblGame = new Label();
			txtGame = new TextBox();
			lblId = new Label();
			txtId = new TextBox();
			lblAppId = new Label();
			txtAppId = new TextBox();
			lblExecutable = new Label();
			txtExecutable = new TextBox();
			lblArguments = new Label();
			txtArguments = new TextBox();
			lblArgumentTag = new Label();
			cmbArgumentTag = new ModernSettingsComboBox();
			btnInsertArgumentTag = new ModernSettingsButton();
			lblRconSyntax = new Label();
			txtRconSyntax = new TextBox();
			lblCatalogOrder = new Label();
			numCatalogOrder = new ModernSettingsNumericUpDown();
			lblDefinitionRevision = new Label();
			numDefinitionRevision = new ModernSettingsNumericUpDown();
			lblPort = new Label();
			numPort = new ModernSettingsNumericUpDown();
			lblQueryPort = new Label();
			numQueryPort = new ModernSettingsNumericUpDown();
			lblConfigMode = new Label();
			cmbConfigMode = new ModernSettingsComboBox();
			lblFormat = new Label();
			cmbFormat = new ModernSettingsComboBox();
			lblConfigModeHelp = new Label();
			lblConfigPath = new Label();
			txtConfigPath = new TextBox();
			lblTemplate = new Label();
			txtTemplate = new TextBox();
			btnBrowseTemplate = new ModernSettingsButton();
			lblConfigRevision = new Label();
			numConfigRevision = new ModernSettingsNumericUpDown();
			lblSteamTarget = new Label();
			txtSteamRuntimeTarget = new TextBox();
			chkSteamLogin = new ModernSettingsToggle();
			lblSteamLoginOption = new Label();
			lblSteamLoginHelp = new Label();
			chkQueryable = new ModernSettingsToggle();
			lblQueryableOption = new Label();
			lblQueryableHelp = new Label();
			chkSteamRuntime = new ModernSettingsToggle();
			lblSteamRuntimeOption = new Label();
			lblSteamRuntimeHelp = new Label();
			lblMaps = new Label();
			txtMaps = new TextBox();
			lblGameModes = new Label();
			txtGameModes = new TextBox();
			lblRequiredLaunchFiles = new Label();
			txtRequiredLaunchFiles = new TextBox();
			lblOptionalLaunchFiles = new Label();
			txtOptionalLaunchFiles = new TextBox();
			lblExternalDataFolder = new Label();
			txtExternalDataFolder = new TextBox();
			lblSetupInstructions = new Label();
			txtSetupInstructions = new TextBox();
			lblIconUrl = new Label();
			txtIconUrl = new TextBox();
			chkFirstStartWarning = new ModernSettingsToggle();
			lblFirstStartWarningOption = new Label();
			lblFirstStartWarningHelp = new Label();
			lblWarningMessage = new Label();
			txtWarningMessage = new TextBox();
			lblAdditionalTemplates = new Label();
			lblAdditionalTemplatesHelp = new Label();
			dgvAdditionalTemplates = new DataGridView();
			colTemplateDestination = new DataGridViewTextBoxColumn();
			colTemplateSource = new DataGridViewTextBoxColumn();
			btnAddTemplates = new ModernSettingsButton();
			btnRemoveTemplate = new ModernSettingsButton();
			lblSteamAppConfig = new Label();
			txtSteamAppConfig = new TextBox();
			lblRuntimeSection = new Label();
			lblRuntimeSectionHelp = new Label();
			lblMinimumRam = new Label();
			numMinimumRam = new ModernSettingsNumericUpDown();
			lblDotNetFramework = new Label();
			cmbDotNetFramework = new ModernSettingsComboBox();
			lblDotNetFrameworkHelp = new Label();
			chkRequiresVisualCpp2013 = new ModernSettingsToggle();
			lblRequiresVisualCpp2013Option = new Label();
			lblRequiresVisualCpp2013Help = new Label();
			chkRequiresVisualCpp2015To2022 = new ModernSettingsToggle();
			lblRequiresVisualCpp2015To2022Option = new Label();
			lblRequiresVisualCpp2015To2022Help = new Label();
			chkRequiresAvx2 = new ModernSettingsToggle();
			lblRequiresAvx2Option = new Label();
			lblRequiresAvx2Help = new Label();
			chkRequiresVirtualization = new ModernSettingsToggle();
			lblRequiresVirtualizationOption = new Label();
			lblRequiresVirtualizationHelp = new Label();
			chkRequiresHyperV = new ModernSettingsToggle();
			lblRequiresHyperVOption = new Label();
			lblRequiresHyperVHelp = new Label();
			chkRequiresWindowsPro = new ModernSettingsToggle();
			lblRequiresWindowsProOption = new Label();
			lblRequiresWindowsProHelp = new Label();
			lblLaunchSection = new Label();
			lblLaunchSectionHelp = new Label();
			chkRunElevated = new ModernSettingsToggle();
			lblRunElevatedOption = new Label();
			lblRunElevatedHelp = new Label();
			chkRequiresVisibleWindow = new ModernSettingsToggle();
			lblRequiresVisibleWindowOption = new Label();
			lblRequiresVisibleWindowHelp = new Label();
			lblLifecycleTracking = new Label();
			cmbLifecycleTracking = new ModernSettingsComboBox();
			lblLifecycleTrackingHelp = new Label();
			chkAllowLaunchExport = new ModernSettingsToggle();
			lblAllowLaunchExportOption = new Label();
			lblAllowLaunchExportHelp = new Label();
			lblReadyMessage = new Label();
			txtReadyMessage = new TextBox();
			lblLogPaths = new Label();
			txtLogPaths = new TextBox();
			lblRightPane = new Label();
			btnShowGuide = new ModernSettingsButton();
			btnShowPreview = new ModernSettingsButton();
			rtbGuide = new RichTextBox();
			rtbPreview = new RichTextBox();
			lblStatus = new Label();
			btnValidate = new ModernSettingsButton();
			btnSave = new ModernSettingsButton();
			pnlInputs.SuspendLayout();
			(numCatalogOrder).BeginInit();
			(numDefinitionRevision).BeginInit();
			(numPort).BeginInit();
			(numQueryPort).BeginInit();
			(numConfigRevision).BeginInit();
			((System.ComponentModel.ISupportInitialize)dgvAdditionalTemplates).BeginInit();
			(numMinimumRam).BeginInit();
			SuspendLayout();
			// 
			// lblTitle
			// 
			lblTitle.AutoSize = true;
			lblTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
			lblTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblTitle.Location = new Point(28, 20);
			lblTitle.Name = "lblTitle";
			lblTitle.Size = new Size(326, 37);
			lblTitle.TabIndex = 0;
			lblTitle.Text = "Game Definition Builder";
			// 
			// lblDescription
			// 
			lblDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblDescription.Font = new Font("Segoe UI", 9.5F);
			lblDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblDescription.Location = new Point(31, 62);
			lblDescription.Name = "lblDescription";
			lblDescription.Size = new Size(1110, 42);
			lblDescription.TabIndex = 1;
			lblDescription.Text = "Create a validated built-in game definition without plugins or scripts. Definitions are saved into the project and become available only after Synix is rebuilt.";
			// 
			// pnlInputs
			// 
			pnlInputs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			pnlInputs.AutoScroll = true;
			pnlInputs.AutoScrollMinSize = new Size(0, 4200);
			pnlInputs.BackColor = Color.FromArgb(17, 27, 45);
			pnlInputs.Controls.Add(lblGame);
			pnlInputs.Controls.Add(txtGame);
			pnlInputs.Controls.Add(lblId);
			pnlInputs.Controls.Add(txtId);
			pnlInputs.Controls.Add(lblAppId);
			pnlInputs.Controls.Add(txtAppId);
			pnlInputs.Controls.Add(lblExecutable);
			pnlInputs.Controls.Add(txtExecutable);
			pnlInputs.Controls.Add(lblArguments);
			pnlInputs.Controls.Add(txtArguments);
			pnlInputs.Controls.Add(lblArgumentTag);
			pnlInputs.Controls.Add(cmbArgumentTag);
			pnlInputs.Controls.Add(btnInsertArgumentTag);
			pnlInputs.Controls.Add(lblRconSyntax);
			pnlInputs.Controls.Add(txtRconSyntax);
			pnlInputs.Controls.Add(lblCatalogOrder);
			pnlInputs.Controls.Add(numCatalogOrder);
			pnlInputs.Controls.Add(lblDefinitionRevision);
			pnlInputs.Controls.Add(numDefinitionRevision);
			pnlInputs.Controls.Add(lblPort);
			pnlInputs.Controls.Add(numPort);
			pnlInputs.Controls.Add(lblQueryPort);
			pnlInputs.Controls.Add(numQueryPort);
			pnlInputs.Controls.Add(lblConfigMode);
			pnlInputs.Controls.Add(cmbConfigMode);
			pnlInputs.Controls.Add(lblFormat);
			pnlInputs.Controls.Add(cmbFormat);
			pnlInputs.Controls.Add(lblConfigModeHelp);
			pnlInputs.Controls.Add(lblConfigPath);
			pnlInputs.Controls.Add(txtConfigPath);
			pnlInputs.Controls.Add(lblTemplate);
			pnlInputs.Controls.Add(txtTemplate);
			pnlInputs.Controls.Add(btnBrowseTemplate);
			pnlInputs.Controls.Add(lblConfigRevision);
			pnlInputs.Controls.Add(numConfigRevision);
			pnlInputs.Controls.Add(lblSteamTarget);
			pnlInputs.Controls.Add(txtSteamRuntimeTarget);
			pnlInputs.Controls.Add(chkSteamLogin);
			pnlInputs.Controls.Add(lblSteamLoginOption);
			pnlInputs.Controls.Add(lblSteamLoginHelp);
			pnlInputs.Controls.Add(chkQueryable);
			pnlInputs.Controls.Add(lblQueryableOption);
			pnlInputs.Controls.Add(lblQueryableHelp);
			pnlInputs.Controls.Add(chkSteamRuntime);
			pnlInputs.Controls.Add(lblSteamRuntimeOption);
			pnlInputs.Controls.Add(lblSteamRuntimeHelp);
			pnlInputs.Controls.Add(lblMaps);
			pnlInputs.Controls.Add(txtMaps);
			pnlInputs.Controls.Add(lblGameModes);
			pnlInputs.Controls.Add(txtGameModes);
			pnlInputs.Controls.Add(lblRequiredLaunchFiles);
			pnlInputs.Controls.Add(txtRequiredLaunchFiles);
			pnlInputs.Controls.Add(lblOptionalLaunchFiles);
			pnlInputs.Controls.Add(txtOptionalLaunchFiles);
			pnlInputs.Controls.Add(lblExternalDataFolder);
			pnlInputs.Controls.Add(txtExternalDataFolder);
			pnlInputs.Controls.Add(lblSetupInstructions);
			pnlInputs.Controls.Add(txtSetupInstructions);
			pnlInputs.Controls.Add(lblIconUrl);
			pnlInputs.Controls.Add(txtIconUrl);
			pnlInputs.Controls.Add(chkFirstStartWarning);
			pnlInputs.Controls.Add(lblFirstStartWarningOption);
			pnlInputs.Controls.Add(lblFirstStartWarningHelp);
			pnlInputs.Controls.Add(lblWarningMessage);
			pnlInputs.Controls.Add(txtWarningMessage);
			pnlInputs.Controls.Add(lblAdditionalTemplates);
			pnlInputs.Controls.Add(lblAdditionalTemplatesHelp);
			pnlInputs.Controls.Add(dgvAdditionalTemplates);
			pnlInputs.Controls.Add(btnAddTemplates);
			pnlInputs.Controls.Add(btnRemoveTemplate);
			pnlInputs.Controls.Add(lblSteamAppConfig);
			pnlInputs.Controls.Add(txtSteamAppConfig);
			pnlInputs.Controls.Add(lblRuntimeSection);
			pnlInputs.Controls.Add(lblRuntimeSectionHelp);
			pnlInputs.Controls.Add(lblMinimumRam);
			pnlInputs.Controls.Add(numMinimumRam);
			pnlInputs.Controls.Add(lblDotNetFramework);
			pnlInputs.Controls.Add(cmbDotNetFramework);
			pnlInputs.Controls.Add(lblDotNetFrameworkHelp);
			pnlInputs.Controls.Add(chkRequiresVisualCpp2013);
			pnlInputs.Controls.Add(lblRequiresVisualCpp2013Option);
			pnlInputs.Controls.Add(lblRequiresVisualCpp2013Help);
			pnlInputs.Controls.Add(chkRequiresVisualCpp2015To2022);
			pnlInputs.Controls.Add(lblRequiresVisualCpp2015To2022Option);
			pnlInputs.Controls.Add(lblRequiresVisualCpp2015To2022Help);
			pnlInputs.Controls.Add(chkRequiresAvx2);
			pnlInputs.Controls.Add(lblRequiresAvx2Option);
			pnlInputs.Controls.Add(lblRequiresAvx2Help);
			pnlInputs.Controls.Add(chkRequiresVirtualization);
			pnlInputs.Controls.Add(lblRequiresVirtualizationOption);
			pnlInputs.Controls.Add(lblRequiresVirtualizationHelp);
			pnlInputs.Controls.Add(chkRequiresHyperV);
			pnlInputs.Controls.Add(lblRequiresHyperVOption);
			pnlInputs.Controls.Add(lblRequiresHyperVHelp);
			pnlInputs.Controls.Add(chkRequiresWindowsPro);
			pnlInputs.Controls.Add(lblRequiresWindowsProOption);
			pnlInputs.Controls.Add(lblRequiresWindowsProHelp);
			pnlInputs.Controls.Add(lblLaunchSection);
			pnlInputs.Controls.Add(lblLaunchSectionHelp);
			pnlInputs.Controls.Add(chkRunElevated);
			pnlInputs.Controls.Add(lblRunElevatedOption);
			pnlInputs.Controls.Add(lblRunElevatedHelp);
			pnlInputs.Controls.Add(chkRequiresVisibleWindow);
			pnlInputs.Controls.Add(lblRequiresVisibleWindowOption);
			pnlInputs.Controls.Add(lblRequiresVisibleWindowHelp);
			pnlInputs.Controls.Add(lblLifecycleTracking);
			pnlInputs.Controls.Add(cmbLifecycleTracking);
			pnlInputs.Controls.Add(lblLifecycleTrackingHelp);
			pnlInputs.Controls.Add(chkAllowLaunchExport);
			pnlInputs.Controls.Add(lblAllowLaunchExportOption);
			pnlInputs.Controls.Add(lblAllowLaunchExportHelp);
			pnlInputs.Controls.Add(lblReadyMessage);
			pnlInputs.Controls.Add(txtReadyMessage);
			pnlInputs.Controls.Add(lblLogPaths);
			pnlInputs.Controls.Add(txtLogPaths);
			pnlInputs.Location = new Point(28, 112);
			pnlInputs.Name = "pnlInputs";
			pnlInputs.Size = new Size(558, 606);
			pnlInputs.TabIndex = 2;
			// 
			// lblGame
			// 
			lblGame.AutoSize = true;
			lblGame.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblGame.ForeColor = Color.FromArgb(158, 172, 194);
			lblGame.Location = new Point(12, 12);
			lblGame.Name = "lblGame";
			lblGame.Size = new Size(74, 15);
			lblGame.TabIndex = 0;
			lblGame.Text = "Game name";
			// 
			// txtGame
			// 
			txtGame.BackColor = Color.FromArgb(12, 21, 36);
			txtGame.BorderStyle = BorderStyle.FixedSingle;
			txtGame.Font = new Font("Segoe UI", 10F);
			txtGame.ForeColor = Color.FromArgb(245, 247, 251);
			txtGame.Location = new Point(12, 38);
			txtGame.Name = "txtGame";
			txtGame.Size = new Size(516, 25);
			txtGame.TabIndex = 1;
			txtGame.Leave += txtGame_Leave;
			// 
			// lblId
			// 
			lblId.AutoSize = true;
			lblId.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblId.ForeColor = Color.FromArgb(158, 172, 194);
			lblId.Location = new Point(12, 84);
			lblId.Name = "lblId";
			lblId.Size = new Size(79, 15);
			lblId.TabIndex = 2;
			lblId.Text = "Definition ID";
			// 
			// txtId
			// 
			txtId.BackColor = Color.FromArgb(12, 21, 36);
			txtId.BorderStyle = BorderStyle.FixedSingle;
			txtId.Font = new Font("Segoe UI", 10F);
			txtId.ForeColor = Color.FromArgb(245, 247, 251);
			txtId.Location = new Point(12, 110);
			txtId.Name = "txtId";
			txtId.Size = new Size(516, 25);
			txtId.TabIndex = 3;
			// 
			// lblAppId
			// 
			lblAppId.AutoSize = true;
			lblAppId.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblAppId.ForeColor = Color.FromArgb(158, 172, 194);
			lblAppId.Location = new Point(12, 156);
			lblAppId.Name = "lblAppId";
			lblAppId.Size = new Size(81, 15);
			lblAppId.TabIndex = 4;
			lblAppId.Text = "Steam AppID";
			// 
			// txtAppId
			// 
			txtAppId.BackColor = Color.FromArgb(12, 21, 36);
			txtAppId.BorderStyle = BorderStyle.FixedSingle;
			txtAppId.Font = new Font("Segoe UI", 10F);
			txtAppId.ForeColor = Color.FromArgb(245, 247, 251);
			txtAppId.Location = new Point(12, 182);
			txtAppId.Name = "txtAppId";
			txtAppId.Size = new Size(516, 25);
			txtAppId.TabIndex = 5;
			// 
			// lblExecutable
			// 
			lblExecutable.AutoSize = true;
			lblExecutable.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblExecutable.ForeColor = Color.FromArgb(158, 172, 194);
			lblExecutable.Location = new Point(12, 228);
			lblExecutable.Name = "lblExecutable";
			lblExecutable.Size = new Size(192, 15);
			lblExecutable.TabIndex = 6;
			lblExecutable.Text = "Server executable (relative path)";
			// 
			// txtExecutable
			// 
			txtExecutable.BackColor = Color.FromArgb(12, 21, 36);
			txtExecutable.BorderStyle = BorderStyle.FixedSingle;
			txtExecutable.Font = new Font("Segoe UI", 10F);
			txtExecutable.ForeColor = Color.FromArgb(245, 247, 251);
			txtExecutable.Location = new Point(12, 254);
			txtExecutable.Name = "txtExecutable";
			txtExecutable.Size = new Size(516, 25);
			txtExecutable.TabIndex = 7;
			// 
			// lblArguments
			// 
			lblArguments.AutoSize = true;
			lblArguments.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblArguments.ForeColor = Color.FromArgb(158, 172, 194);
			lblArguments.Location = new Point(12, 300);
			lblArguments.Name = "lblArguments";
			lblArguments.Size = new Size(341, 15);
			lblArguments.TabIndex = 8;
			lblArguments.Text = "Default launch arguments (everything after the executable)";
			// 
			// txtArguments
			// 
			txtArguments.BackColor = Color.FromArgb(12, 21, 36);
			txtArguments.BorderStyle = BorderStyle.FixedSingle;
			txtArguments.Font = new Font("Segoe UI", 10F);
			txtArguments.ForeColor = Color.FromArgb(245, 247, 251);
			txtArguments.Location = new Point(12, 326);
			txtArguments.Name = "txtArguments";
			txtArguments.Size = new Size(516, 25);
			txtArguments.TabIndex = 9;
			// 
			// lblArgumentTag
			// 
			lblArgumentTag.AutoSize = true;
			lblArgumentTag.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblArgumentTag.ForeColor = Color.FromArgb(158, 172, 194);
			lblArgumentTag.Location = new Point(12, 372);
			lblArgumentTag.Name = "lblArgumentTag";
			lblArgumentTag.Size = new Size(221, 15);
			lblArgumentTag.TabIndex = 10;
			lblArgumentTag.Text = "Insert a supported Synix argument tag";
			// 
			// cmbArgumentTag
			// 
			cmbArgumentTag.ArrowColor = Color.FromArgb(158, 172, 194);
			cmbArgumentTag.BackColor = Color.FromArgb(12, 21, 36);
			cmbArgumentTag.BorderColor = Color.FromArgb(38, 52, 77);
			cmbArgumentTag.DrawMode = DrawMode.OwnerDrawFixed;
			cmbArgumentTag.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbArgumentTag.FlatStyle = FlatStyle.Flat;
			cmbArgumentTag.FocusBorderColor = Color.FromArgb(38, 52, 77);
			cmbArgumentTag.Font = new Font("Segoe UI", 10F);
			cmbArgumentTag.ForeColor = Color.FromArgb(245, 247, 251);
			cmbArgumentTag.ItemHeight = 28;
			cmbArgumentTag.Location = new Point(12, 396);
			cmbArgumentTag.Name = "cmbArgumentTag";
			cmbArgumentTag.SelectedItemBackColor = Color.FromArgb(24, 55, 73);
			cmbArgumentTag.Size = new Size(390, 34);
			cmbArgumentTag.TabIndex = 11;
			// 
			// btnInsertArgumentTag
			// 
			btnInsertArgumentTag.BackColor = Color.FromArgb(12, 21, 36);
			btnInsertArgumentTag.FlatStyle = FlatStyle.Flat;
			btnInsertArgumentTag.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnInsertArgumentTag.ForeColor = Color.FromArgb(245, 247, 251);
			btnInsertArgumentTag.Location = new Point(414, 396);
			btnInsertArgumentTag.Name = "btnInsertArgumentTag";
			btnInsertArgumentTag.Size = new Size(114, 42);
			btnInsertArgumentTag.TabIndex = 12;
			btnInsertArgumentTag.Text = "Insert tag";
			btnInsertArgumentTag.UseVisualStyleBackColor = false;
			btnInsertArgumentTag.Click += btnInsertArgumentTag_Click;
			// 
			// lblRconSyntax
			// 
			lblRconSyntax.AutoSize = true;
			lblRconSyntax.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblRconSyntax.ForeColor = Color.FromArgb(158, 172, 194);
			lblRconSyntax.Location = new Point(12, 454);
			lblRconSyntax.Name = "lblRconSyntax";
			lblRconSyntax.Size = new Size(357, 15);
			lblRconSyntax.TabIndex = 13;
			lblRconSyntax.Text = "Optional RCON syntax — launch arguments must contain {rcon}";
			// 
			// txtRconSyntax
			// 
			txtRconSyntax.BackColor = Color.FromArgb(12, 21, 36);
			txtRconSyntax.BorderStyle = BorderStyle.FixedSingle;
			txtRconSyntax.Font = new Font("Segoe UI", 10F);
			txtRconSyntax.ForeColor = Color.FromArgb(245, 247, 251);
			txtRconSyntax.Location = new Point(12, 480);
			txtRconSyntax.Name = "txtRconSyntax";
			txtRconSyntax.Size = new Size(516, 25);
			txtRconSyntax.TabIndex = 14;
			// 
			// lblCatalogOrder
			// 
			lblCatalogOrder.AutoSize = true;
			lblCatalogOrder.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblCatalogOrder.ForeColor = Color.FromArgb(158, 172, 194);
			lblCatalogOrder.Location = new Point(12, 536);
			lblCatalogOrder.Name = "lblCatalogOrder";
			lblCatalogOrder.Size = new Size(82, 15);
			lblCatalogOrder.TabIndex = 15;
			lblCatalogOrder.Text = "Catalog order";
			// 
			// numCatalogOrder
			// 
			numCatalogOrder.AccessibleRole = AccessibleRole.SpinButton;
			numCatalogOrder.BackColor = Color.FromArgb(12, 21, 36);
			numCatalogOrder.Font = new Font("Segoe UI", 11F);
			numCatalogOrder.ForeColor = Color.FromArgb(245, 247, 251);
			numCatalogOrder.Location = new Point(12, 562);
			numCatalogOrder.Maximum = 10000;
			numCatalogOrder.Name = "numCatalogOrder";
			numCatalogOrder.Size = new Size(250, 42);
			numCatalogOrder.TabIndex = 16;
			// 
			// lblDefinitionRevision
			// 
			lblDefinitionRevision.AutoSize = true;
			lblDefinitionRevision.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblDefinitionRevision.ForeColor = Color.FromArgb(158, 172, 194);
			lblDefinitionRevision.Location = new Point(278, 536);
			lblDefinitionRevision.Name = "lblDefinitionRevision";
			lblDefinitionRevision.Size = new Size(110, 15);
			lblDefinitionRevision.TabIndex = 17;
			lblDefinitionRevision.Text = "Definition revision";
			// 
			// numDefinitionRevision
			// 
			numDefinitionRevision.AccessibleRole = AccessibleRole.SpinButton;
			numDefinitionRevision.BackColor = Color.FromArgb(12, 21, 36);
			numDefinitionRevision.Font = new Font("Segoe UI", 11F);
			numDefinitionRevision.ForeColor = Color.FromArgb(245, 247, 251);
			numDefinitionRevision.Location = new Point(278, 562);
			numDefinitionRevision.Maximum = 10000;
			numDefinitionRevision.Name = "numDefinitionRevision";
			numDefinitionRevision.Size = new Size(250, 42);
			numDefinitionRevision.TabIndex = 18;
			// 
			// lblPort
			// 
			lblPort.AutoSize = true;
			lblPort.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblPort.ForeColor = Color.FromArgb(158, 172, 194);
			lblPort.Location = new Point(12, 620);
			lblPort.Name = "lblPort";
			lblPort.Size = new Size(67, 15);
			lblPort.TabIndex = 19;
			lblPort.Text = "Game port";
			// 
			// numPort
			// 
			numPort.AccessibleRole = AccessibleRole.SpinButton;
			numPort.BackColor = Color.FromArgb(12, 21, 36);
			numPort.Font = new Font("Segoe UI", 11F);
			numPort.ForeColor = Color.FromArgb(245, 247, 251);
			numPort.Location = new Point(12, 646);
			numPort.Maximum = 65535;
			numPort.Name = "numPort";
			numPort.Size = new Size(250, 42);
			numPort.TabIndex = 20;
			numPort.Value = 7777;
			// 
			// lblQueryPort
			// 
			lblQueryPort.AutoSize = true;
			lblQueryPort.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblQueryPort.ForeColor = Color.FromArgb(158, 172, 194);
			lblQueryPort.Location = new Point(278, 620);
			lblQueryPort.Name = "lblQueryPort";
			lblQueryPort.Size = new Size(68, 15);
			lblQueryPort.TabIndex = 21;
			lblQueryPort.Text = "Query port";
			// 
			// numQueryPort
			// 
			numQueryPort.AccessibleRole = AccessibleRole.SpinButton;
			numQueryPort.BackColor = Color.FromArgb(12, 21, 36);
			numQueryPort.Font = new Font("Segoe UI", 11F);
			numQueryPort.ForeColor = Color.FromArgb(245, 247, 251);
			numQueryPort.Location = new Point(278, 646);
			numQueryPort.Maximum = 65535;
			numQueryPort.Name = "numQueryPort";
			numQueryPort.Size = new Size(250, 42);
			numQueryPort.TabIndex = 22;
			numQueryPort.Value = 27015;
			// 
			// lblConfigMode
			// 
			lblConfigMode.AutoSize = true;
			lblConfigMode.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblConfigMode.ForeColor = Color.FromArgb(158, 172, 194);
			lblConfigMode.Location = new Point(12, 704);
			lblConfigMode.Name = "lblConfigMode";
			lblConfigMode.Size = new Size(135, 15);
			lblConfigMode.TabIndex = 23;
			lblConfigMode.Text = "Configuration behavior";
			// 
			// cmbConfigMode
			// 
			cmbConfigMode.ArrowColor = Color.FromArgb(158, 172, 194);
			cmbConfigMode.BackColor = Color.FromArgb(12, 21, 36);
			cmbConfigMode.BorderColor = Color.FromArgb(38, 52, 77);
			cmbConfigMode.DrawMode = DrawMode.OwnerDrawFixed;
			cmbConfigMode.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbConfigMode.FlatStyle = FlatStyle.Flat;
			cmbConfigMode.FocusBorderColor = Color.FromArgb(38, 52, 77);
			cmbConfigMode.Font = new Font("Segoe UI", 10F);
			cmbConfigMode.ForeColor = Color.FromArgb(245, 247, 251);
			cmbConfigMode.ItemHeight = 28;
			cmbConfigMode.Location = new Point(12, 730);
			cmbConfigMode.Name = "cmbConfigMode";
			cmbConfigMode.SelectedItemBackColor = Color.FromArgb(24, 55, 73);
			cmbConfigMode.Size = new Size(121, 34);
			cmbConfigMode.TabIndex = 24;
			cmbConfigMode.SelectedIndexChanged += cmbConfigMode_SelectedIndexChanged;
			// 
			// lblFormat
			// 
			lblFormat.AutoSize = true;
			lblFormat.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblFormat.ForeColor = Color.FromArgb(158, 172, 194);
			lblFormat.Location = new Point(278, 704);
			lblFormat.Name = "lblFormat";
			lblFormat.Size = new Size(125, 15);
			lblFormat.TabIndex = 25;
			lblFormat.Text = "Configuration format";
			// 
			// cmbFormat
			// 
			cmbFormat.ArrowColor = Color.FromArgb(158, 172, 194);
			cmbFormat.BackColor = Color.FromArgb(12, 21, 36);
			cmbFormat.BorderColor = Color.FromArgb(38, 52, 77);
			cmbFormat.DrawMode = DrawMode.OwnerDrawFixed;
			cmbFormat.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbFormat.FlatStyle = FlatStyle.Flat;
			cmbFormat.FocusBorderColor = Color.FromArgb(38, 52, 77);
			cmbFormat.Font = new Font("Segoe UI", 10F);
			cmbFormat.ForeColor = Color.FromArgb(245, 247, 251);
			cmbFormat.ItemHeight = 28;
			cmbFormat.Location = new Point(278, 730);
			cmbFormat.Name = "cmbFormat";
			cmbFormat.SelectedItemBackColor = Color.FromArgb(24, 55, 73);
			cmbFormat.Size = new Size(121, 34);
			cmbFormat.TabIndex = 26;
			// 
			// lblConfigModeHelp
			// 
			lblConfigModeHelp.Font = new Font("Segoe UI", 8.5F);
			lblConfigModeHelp.ForeColor = Color.FromArgb(158, 172, 194);
			lblConfigModeHelp.Location = new Point(12, 780);
			lblConfigModeHelp.Name = "lblConfigModeHelp";
			lblConfigModeHelp.Size = new Size(516, 48);
			lblConfigModeHelp.TabIndex = 27;
			// 
			// lblConfigPath
			// 
			lblConfigPath.AutoSize = true;
			lblConfigPath.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblConfigPath.ForeColor = Color.FromArgb(158, 172, 194);
			lblConfigPath.Location = new Point(12, 840);
			lblConfigPath.Name = "lblConfigPath";
			lblConfigPath.Size = new Size(319, 15);
			lblConfigPath.TabIndex = 28;
			lblConfigPath.Text = "Configuration path relative to the installed server folder";
			// 
			// txtConfigPath
			// 
			txtConfigPath.BackColor = Color.FromArgb(12, 21, 36);
			txtConfigPath.BorderStyle = BorderStyle.FixedSingle;
			txtConfigPath.Font = new Font("Segoe UI", 10F);
			txtConfigPath.ForeColor = Color.FromArgb(245, 247, 251);
			txtConfigPath.Location = new Point(12, 866);
			txtConfigPath.Name = "txtConfigPath";
			txtConfigPath.Size = new Size(516, 25);
			txtConfigPath.TabIndex = 29;
			// 
			// lblTemplate
			// 
			lblTemplate.AutoSize = true;
			lblTemplate.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblTemplate.ForeColor = Color.FromArgb(158, 172, 194);
			lblTemplate.Location = new Point(12, 912);
			lblTemplate.Name = "lblTemplate";
			lblTemplate.Size = new Size(266, 15);
			lblTemplate.TabIndex = 30;
			lblTemplate.Text = "Complete, working configuration template file";
			// 
			// txtTemplate
			// 
			txtTemplate.BackColor = Color.FromArgb(12, 21, 36);
			txtTemplate.BorderStyle = BorderStyle.FixedSingle;
			txtTemplate.Font = new Font("Segoe UI", 10F);
			txtTemplate.ForeColor = Color.FromArgb(245, 247, 251);
			txtTemplate.Location = new Point(12, 938);
			txtTemplate.Name = "txtTemplate";
			txtTemplate.Size = new Size(394, 25);
			txtTemplate.TabIndex = 31;
			// 
			// btnBrowseTemplate
			// 
			btnBrowseTemplate.BackColor = Color.FromArgb(12, 21, 36);
			btnBrowseTemplate.FlatStyle = FlatStyle.Flat;
			btnBrowseTemplate.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnBrowseTemplate.ForeColor = Color.FromArgb(245, 247, 251);
			btnBrowseTemplate.Location = new Point(414, 938);
			btnBrowseTemplate.Name = "btnBrowseTemplate";
			btnBrowseTemplate.Size = new Size(96, 42);
			btnBrowseTemplate.TabIndex = 32;
			btnBrowseTemplate.UseVisualStyleBackColor = false;
			btnBrowseTemplate.Click += btnBrowseTemplate_Click;
			// 
			// lblConfigRevision
			// 
			lblConfigRevision.AutoSize = true;
			lblConfigRevision.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblConfigRevision.ForeColor = Color.FromArgb(158, 172, 194);
			lblConfigRevision.Location = new Point(12, 990);
			lblConfigRevision.Name = "lblConfigRevision";
			lblConfigRevision.Size = new Size(106, 15);
			lblConfigRevision.TabIndex = 33;
			lblConfigRevision.Text = "Template revision";
			// 
			// numConfigRevision
			// 
			numConfigRevision.AccessibleRole = AccessibleRole.SpinButton;
			numConfigRevision.BackColor = Color.FromArgb(12, 21, 36);
			numConfigRevision.Font = new Font("Segoe UI", 11F);
			numConfigRevision.ForeColor = Color.FromArgb(245, 247, 251);
			numConfigRevision.Location = new Point(12, 1016);
			numConfigRevision.Maximum = 10000;
			numConfigRevision.Name = "numConfigRevision";
			numConfigRevision.Size = new Size(250, 42);
			numConfigRevision.TabIndex = 34;
			// 
			// lblSteamTarget
			// 
			lblSteamTarget.AutoSize = true;
			lblSteamTarget.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblSteamTarget.ForeColor = Color.FromArgb(158, 172, 194);
			lblSteamTarget.Location = new Point(12, 1074);
			lblSteamTarget.Name = "lblSteamTarget";
			lblSteamTarget.Size = new Size(265, 15);
			lblSteamTarget.TabIndex = 35;
			lblSteamTarget.Text = "Steam runtime target directory (relative path)";
			// 
			// txtSteamRuntimeTarget
			// 
			txtSteamRuntimeTarget.BackColor = Color.FromArgb(12, 21, 36);
			txtSteamRuntimeTarget.BorderStyle = BorderStyle.FixedSingle;
			txtSteamRuntimeTarget.Font = new Font("Segoe UI", 10F);
			txtSteamRuntimeTarget.ForeColor = Color.FromArgb(245, 247, 251);
			txtSteamRuntimeTarget.Location = new Point(12, 1100);
			txtSteamRuntimeTarget.Name = "txtSteamRuntimeTarget";
			txtSteamRuntimeTarget.Size = new Size(516, 25);
			txtSteamRuntimeTarget.TabIndex = 36;
			// 
			// chkSteamLogin
			// 
			chkSteamLogin.AccessibleName = "Steam account login required";
			chkSteamLogin.AccessibleRole = AccessibleRole.CheckButton;
			chkSteamLogin.BackColor = Color.FromArgb(17, 27, 45);
			chkSteamLogin.Location = new Point(474, 1164);
			chkSteamLogin.Name = "chkSteamLogin";
			chkSteamLogin.Size = new Size(54, 30);
			chkSteamLogin.TabIndex = 37;
			chkSteamLogin.UseVisualStyleBackColor = false;
			// 
			// lblSteamLoginOption
			// 
			lblSteamLoginOption.AutoSize = true;
			lblSteamLoginOption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblSteamLoginOption.ForeColor = Color.FromArgb(245, 247, 251);
			lblSteamLoginOption.Location = new Point(12, 1158);
			lblSteamLoginOption.Name = "lblSteamLoginOption";
			lblSteamLoginOption.Size = new Size(171, 15);
			lblSteamLoginOption.TabIndex = 38;
			lblSteamLoginOption.Text = "Steam account login required";
			// 
			// lblSteamLoginHelp
			// 
			lblSteamLoginHelp.Font = new Font("Segoe UI", 8.5F);
			lblSteamLoginHelp.ForeColor = Color.FromArgb(158, 172, 194);
			lblSteamLoginHelp.Location = new Point(12, 1182);
			lblSteamLoginHelp.Name = "lblSteamLoginHelp";
			lblSteamLoginHelp.Size = new Size(438, 34);
			lblSteamLoginHelp.TabIndex = 39;
			lblSteamLoginHelp.Text = "Enable only when anonymous SteamCMD installation fails and a Steam account is required.";
			// 
			// chkQueryable
			// 
			chkQueryable.AccessibleName = "Enable server query monitoring";
			chkQueryable.AccessibleRole = AccessibleRole.CheckButton;
			chkQueryable.BackColor = Color.FromArgb(17, 27, 45);
			chkQueryable.Checked = true;
			chkQueryable.CheckState = CheckState.Checked;
			chkQueryable.Location = new Point(474, 1232);
			chkQueryable.Name = "chkQueryable";
			chkQueryable.Size = new Size(54, 30);
			chkQueryable.TabIndex = 40;
			chkQueryable.UseVisualStyleBackColor = false;
			// 
			// lblQueryableOption
			// 
			lblQueryableOption.AutoSize = true;
			lblQueryableOption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblQueryableOption.ForeColor = Color.FromArgb(245, 247, 251);
			lblQueryableOption.Location = new Point(12, 1226);
			lblQueryableOption.Name = "lblQueryableOption";
			lblQueryableOption.Size = new Size(182, 15);
			lblQueryableOption.TabIndex = 41;
			lblQueryableOption.Text = "Enable server query monitoring";
			// 
			// lblQueryableHelp
			// 
			lblQueryableHelp.Font = new Font("Segoe UI", 8.5F);
			lblQueryableHelp.ForeColor = Color.FromArgb(158, 172, 194);
			lblQueryableHelp.Location = new Point(12, 1250);
			lblQueryableHelp.Name = "lblQueryableHelp";
			lblQueryableHelp.Size = new Size(438, 34);
			lblQueryableHelp.TabIndex = 42;
			lblQueryableHelp.Text = "Enable when the server has a verified query or network probe that Synix can monitor.";
			// 
			// chkSteamRuntime
			// 
			chkSteamRuntime.AccessibleName = "Copy allowlisted Steam runtime files after install";
			chkSteamRuntime.AccessibleRole = AccessibleRole.CheckButton;
			chkSteamRuntime.BackColor = Color.FromArgb(17, 27, 45);
			chkSteamRuntime.Location = new Point(474, 1300);
			chkSteamRuntime.Name = "chkSteamRuntime";
			chkSteamRuntime.Size = new Size(54, 30);
			chkSteamRuntime.TabIndex = 43;
			chkSteamRuntime.UseVisualStyleBackColor = false;
			chkSteamRuntime.CheckedChanged += chkSteamRuntime_CheckedChanged;
			// 
			// lblSteamRuntimeOption
			// 
			lblSteamRuntimeOption.AutoSize = true;
			lblSteamRuntimeOption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblSteamRuntimeOption.ForeColor = Color.FromArgb(245, 247, 251);
			lblSteamRuntimeOption.Location = new Point(12, 1294);
			lblSteamRuntimeOption.Name = "lblSteamRuntimeOption";
			lblSteamRuntimeOption.Size = new Size(297, 15);
			lblSteamRuntimeOption.TabIndex = 44;
			lblSteamRuntimeOption.Text = "Copy approved Steam runtime files after installation";
			// 
			// lblSteamRuntimeHelp
			// 
			lblSteamRuntimeHelp.Font = new Font("Segoe UI", 8.5F);
			lblSteamRuntimeHelp.ForeColor = Color.FromArgb(158, 172, 194);
			lblSteamRuntimeHelp.Location = new Point(12, 1318);
			lblSteamRuntimeHelp.Name = "lblSteamRuntimeHelp";
			lblSteamRuntimeHelp.Size = new Size(438, 48);
			lblSteamRuntimeHelp.TabIndex = 45;
			lblSteamRuntimeHelp.Text = "Use only when testing proves the server needs the approved Steam DLL files. The target must stay inside the server folder.";
			// 
			// lblMaps
			// 
			lblMaps.AutoSize = true;
			lblMaps.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblMaps.ForeColor = Color.FromArgb(158, 172, 194);
			lblMaps.Location = new Point(12, 1384);
			lblMaps.Name = "lblMaps";
			lblMaps.Size = new Size(249, 15);
			lblMaps.TabIndex = 46;
			lblMaps.Text = "Maps or scenarios (one exact value per line)";
			// 
			// txtMaps
			// 
			txtMaps.AcceptsReturn = true;
			txtMaps.BackColor = Color.FromArgb(12, 21, 36);
			txtMaps.BorderStyle = BorderStyle.FixedSingle;
			txtMaps.Font = new Font("Segoe UI", 9.5F);
			txtMaps.ForeColor = Color.FromArgb(245, 247, 251);
			txtMaps.Location = new Point(12, 1410);
			txtMaps.Multiline = true;
			txtMaps.Name = "txtMaps";
			txtMaps.ScrollBars = ScrollBars.Vertical;
			txtMaps.Size = new Size(516, 90);
			txtMaps.TabIndex = 47;
			txtMaps.WordWrap = false;
			// 
			// lblGameModes
			// 
			lblGameModes.AutoSize = true;
			lblGameModes.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblGameModes.ForeColor = Color.FromArgb(158, 172, 194);
			lblGameModes.Location = new Point(12, 1516);
			lblGameModes.Name = "lblGameModes";
			lblGameModes.Size = new Size(224, 15);
			lblGameModes.TabIndex = 48;
			lblGameModes.Text = "Game modes (one exact value per line)";
			// 
			// txtGameModes
			// 
			txtGameModes.AcceptsReturn = true;
			txtGameModes.BackColor = Color.FromArgb(12, 21, 36);
			txtGameModes.BorderStyle = BorderStyle.FixedSingle;
			txtGameModes.Font = new Font("Segoe UI", 9.5F);
			txtGameModes.ForeColor = Color.FromArgb(245, 247, 251);
			txtGameModes.Location = new Point(12, 1542);
			txtGameModes.Multiline = true;
			txtGameModes.Name = "txtGameModes";
			txtGameModes.ScrollBars = ScrollBars.Vertical;
			txtGameModes.Size = new Size(516, 70);
			txtGameModes.TabIndex = 49;
			txtGameModes.WordWrap = false;
			// 
			// lblRequiredLaunchFiles
			// 
			lblRequiredLaunchFiles.AutoSize = true;
			lblRequiredLaunchFiles.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblRequiredLaunchFiles.ForeColor = Color.FromArgb(158, 172, 194);
			lblRequiredLaunchFiles.Location = new Point(12, 1628);
			lblRequiredLaunchFiles.Name = "lblRequiredLaunchFiles";
			lblRequiredLaunchFiles.Size = new Size(320, 15);
			lblRequiredLaunchFiles.TabIndex = 50;
			lblRequiredLaunchFiles.Text = "Required user-supplied files (relative paths, one per line)";
			// 
			// txtRequiredLaunchFiles
			// 
			txtRequiredLaunchFiles.AcceptsReturn = true;
			txtRequiredLaunchFiles.BackColor = Color.FromArgb(12, 21, 36);
			txtRequiredLaunchFiles.BorderStyle = BorderStyle.FixedSingle;
			txtRequiredLaunchFiles.Font = new Font("Segoe UI", 9.5F);
			txtRequiredLaunchFiles.ForeColor = Color.FromArgb(245, 247, 251);
			txtRequiredLaunchFiles.Location = new Point(12, 1654);
			txtRequiredLaunchFiles.Multiline = true;
			txtRequiredLaunchFiles.Name = "txtRequiredLaunchFiles";
			txtRequiredLaunchFiles.ScrollBars = ScrollBars.Vertical;
			txtRequiredLaunchFiles.Size = new Size(516, 84);
			txtRequiredLaunchFiles.TabIndex = 51;
			txtRequiredLaunchFiles.WordWrap = false;
			// 
			// lblOptionalLaunchFiles
			// 
			lblOptionalLaunchFiles.AutoSize = true;
			lblOptionalLaunchFiles.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblOptionalLaunchFiles.ForeColor = Color.FromArgb(158, 172, 194);
			lblOptionalLaunchFiles.Location = new Point(12, 1754);
			lblOptionalLaunchFiles.Name = "lblOptionalLaunchFiles";
			lblOptionalLaunchFiles.Size = new Size(280, 15);
			lblOptionalLaunchFiles.TabIndex = 52;
			lblOptionalLaunchFiles.Text = "Optional import files (relative paths, one per line)";
			// 
			// txtOptionalLaunchFiles
			// 
			txtOptionalLaunchFiles.AcceptsReturn = true;
			txtOptionalLaunchFiles.BackColor = Color.FromArgb(12, 21, 36);
			txtOptionalLaunchFiles.BorderStyle = BorderStyle.FixedSingle;
			txtOptionalLaunchFiles.Font = new Font("Segoe UI", 9.5F);
			txtOptionalLaunchFiles.ForeColor = Color.FromArgb(245, 247, 251);
			txtOptionalLaunchFiles.Location = new Point(12, 1780);
			txtOptionalLaunchFiles.Multiline = true;
			txtOptionalLaunchFiles.Name = "txtOptionalLaunchFiles";
			txtOptionalLaunchFiles.ScrollBars = ScrollBars.Vertical;
			txtOptionalLaunchFiles.Size = new Size(516, 84);
			txtOptionalLaunchFiles.TabIndex = 53;
			txtOptionalLaunchFiles.WordWrap = false;
			// 
			// lblExternalDataFolder
			// 
			lblExternalDataFolder.AutoSize = true;
			lblExternalDataFolder.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblExternalDataFolder.ForeColor = Color.FromArgb(158, 172, 194);
			lblExternalDataFolder.Location = new Point(12, 1880);
			lblExternalDataFolder.Name = "lblExternalDataFolder";
			lblExternalDataFolder.Size = new Size(329, 15);
			lblExternalDataFolder.TabIndex = 54;
			lblExternalDataFolder.Text = "Documents source folder for automatic imports (optional)";
			// 
			// txtExternalDataFolder
			// 
			txtExternalDataFolder.BackColor = Color.FromArgb(12, 21, 36);
			txtExternalDataFolder.BorderStyle = BorderStyle.FixedSingle;
			txtExternalDataFolder.Font = new Font("Segoe UI", 10F);
			txtExternalDataFolder.ForeColor = Color.FromArgb(245, 247, 251);
			txtExternalDataFolder.Location = new Point(12, 1906);
			txtExternalDataFolder.Name = "txtExternalDataFolder";
			txtExternalDataFolder.Size = new Size(516, 25);
			txtExternalDataFolder.TabIndex = 55;
			// 
			// lblSetupInstructions
			// 
			lblSetupInstructions.AutoSize = true;
			lblSetupInstructions.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblSetupInstructions.ForeColor = Color.FromArgb(158, 172, 194);
			lblSetupInstructions.Location = new Point(12, 1960);
			lblSetupInstructions.Name = "lblSetupInstructions";
			lblSetupInstructions.Size = new Size(296, 15);
			lblSetupInstructions.TabIndex = 56;
			lblSetupInstructions.Text = "How the user obtains and places required game files";
			// 
			// txtSetupInstructions
			// 
			txtSetupInstructions.AcceptsReturn = true;
			txtSetupInstructions.BackColor = Color.FromArgb(12, 21, 36);
			txtSetupInstructions.BorderStyle = BorderStyle.FixedSingle;
			txtSetupInstructions.Font = new Font("Segoe UI", 9.5F);
			txtSetupInstructions.ForeColor = Color.FromArgb(245, 247, 251);
			txtSetupInstructions.Location = new Point(12, 1986);
			txtSetupInstructions.Multiline = true;
			txtSetupInstructions.Name = "txtSetupInstructions";
			txtSetupInstructions.ScrollBars = ScrollBars.Vertical;
			txtSetupInstructions.Size = new Size(516, 100);
			txtSetupInstructions.TabIndex = 57;
			// 
			// lblIconUrl
			// 
			lblIconUrl.AutoSize = true;
			lblIconUrl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblIconUrl.ForeColor = Color.FromArgb(158, 172, 194);
			lblIconUrl.Location = new Point(12, 2102);
			lblIconUrl.Name = "lblIconUrl";
			lblIconUrl.Size = new Size(188, 15);
			lblIconUrl.TabIndex = 58;
			lblIconUrl.Text = "Game icon HTTPS URL (optional)";
			// 
			// txtIconUrl
			// 
			txtIconUrl.BackColor = Color.FromArgb(12, 21, 36);
			txtIconUrl.BorderStyle = BorderStyle.FixedSingle;
			txtIconUrl.Font = new Font("Segoe UI", 10F);
			txtIconUrl.ForeColor = Color.FromArgb(245, 247, 251);
			txtIconUrl.Location = new Point(12, 2128);
			txtIconUrl.Name = "txtIconUrl";
			txtIconUrl.Size = new Size(516, 25);
			txtIconUrl.TabIndex = 59;
			// 
			// chkFirstStartWarning
			// 
			chkFirstStartWarning.AccessibleName = "Show a first-start setup warning";
			chkFirstStartWarning.AccessibleRole = AccessibleRole.CheckButton;
			chkFirstStartWarning.BackColor = Color.FromArgb(17, 27, 45);
			chkFirstStartWarning.Location = new Point(474, 2188);
			chkFirstStartWarning.Name = "chkFirstStartWarning";
			chkFirstStartWarning.Size = new Size(54, 30);
			chkFirstStartWarning.TabIndex = 60;
			chkFirstStartWarning.UseVisualStyleBackColor = false;
			// 
			// lblFirstStartWarningOption
			// 
			lblFirstStartWarningOption.AutoSize = true;
			lblFirstStartWarningOption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblFirstStartWarningOption.ForeColor = Color.FromArgb(245, 247, 251);
			lblFirstStartWarningOption.Location = new Point(12, 2182);
			lblFirstStartWarningOption.Name = "lblFirstStartWarningOption";
			lblFirstStartWarningOption.Size = new Size(185, 15);
			lblFirstStartWarningOption.TabIndex = 61;
			lblFirstStartWarningOption.Text = "Show a first-start setup warning";
			// 
			// lblFirstStartWarningHelp
			// 
			lblFirstStartWarningHelp.Font = new Font("Segoe UI", 8.5F);
			lblFirstStartWarningHelp.ForeColor = Color.FromArgb(158, 172, 194);
			lblFirstStartWarningHelp.Location = new Point(12, 2206);
			lblFirstStartWarningHelp.Name = "lblFirstStartWarningHelp";
			lblFirstStartWarningHelp.Size = new Size(438, 34);
			lblFirstStartWarningHelp.TabIndex = 62;
			lblFirstStartWarningHelp.Text = "Required files and Synix-created templates automatically enable a warning.";
			// 
			// lblWarningMessage
			// 
			lblWarningMessage.AutoSize = true;
			lblWarningMessage.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblWarningMessage.ForeColor = Color.FromArgb(158, 172, 194);
			lblWarningMessage.Location = new Point(12, 2250);
			lblWarningMessage.Name = "lblWarningMessage";
			lblWarningMessage.Size = new Size(215, 15);
			lblWarningMessage.TabIndex = 63;
			lblWarningMessage.Text = "First-start message shown to the user";
			// 
			// txtWarningMessage
			// 
			txtWarningMessage.AcceptsReturn = true;
			txtWarningMessage.BackColor = Color.FromArgb(12, 21, 36);
			txtWarningMessage.BorderStyle = BorderStyle.FixedSingle;
			txtWarningMessage.Font = new Font("Segoe UI", 9.5F);
			txtWarningMessage.ForeColor = Color.FromArgb(245, 247, 251);
			txtWarningMessage.Location = new Point(12, 2276);
			txtWarningMessage.Multiline = true;
			txtWarningMessage.Name = "txtWarningMessage";
			txtWarningMessage.ScrollBars = ScrollBars.Vertical;
			txtWarningMessage.Size = new Size(516, 100);
			txtWarningMessage.TabIndex = 64;
			// 
			// lblAdditionalTemplates
			// 
			lblAdditionalTemplates.AutoSize = true;
			lblAdditionalTemplates.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblAdditionalTemplates.ForeColor = Color.FromArgb(158, 172, 194);
			lblAdditionalTemplates.Location = new Point(12, 2394);
			lblAdditionalTemplates.Name = "lblAdditionalTemplates";
			lblAdditionalTemplates.Size = new Size(167, 15);
			lblAdditionalTemplates.TabIndex = 65;
			lblAdditionalTemplates.Text = "Additional configuration files";
			// 
			// lblAdditionalTemplatesHelp
			// 
			lblAdditionalTemplatesHelp.Font = new Font("Segoe UI", 8.5F);
			lblAdditionalTemplatesHelp.ForeColor = Color.FromArgb(158, 172, 194);
			lblAdditionalTemplatesHelp.Location = new Point(12, 2418);
			lblAdditionalTemplatesHelp.Name = "lblAdditionalTemplatesHelp";
			lblAdditionalTemplatesHelp.Size = new Size(516, 42);
			lblAdditionalTemplatesHelp.TabIndex = 66;
			lblAdditionalTemplatesHelp.Text = "Add every other complete template the game needs. Edit Installed location so each path is relative to the installed server folder.";
			// 
			// dgvAdditionalTemplates
			// 
			dgvAdditionalTemplates.AllowUserToAddRows = false;
			dgvAdditionalTemplates.AllowUserToDeleteRows = false;
			dgvAdditionalTemplates.AllowUserToResizeRows = false;
			dgvAdditionalTemplates.BackgroundColor = Color.FromArgb(12, 21, 36);
			dgvAdditionalTemplates.ColumnHeadersHeight = 36;
			dgvAdditionalTemplates.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			dgvAdditionalTemplates.Columns.AddRange(new DataGridViewColumn[] { colTemplateDestination, colTemplateSource });
			dgvAdditionalTemplates.EnableHeadersVisualStyles = false;
			dgvAdditionalTemplates.Location = new Point(12, 2466);
			dgvAdditionalTemplates.Name = "dgvAdditionalTemplates";
			dgvAdditionalTemplates.RowHeadersVisible = false;
			dgvAdditionalTemplates.RowTemplate.Height = 34;
			dgvAdditionalTemplates.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dgvAdditionalTemplates.Size = new Size(516, 166);
			dgvAdditionalTemplates.TabIndex = 67;
			// 
			// colTemplateDestination
			// 
			colTemplateDestination.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			colTemplateDestination.FillWeight = 44F;
			colTemplateDestination.HeaderText = "Installed location";
			colTemplateDestination.Name = "colTemplateDestination";
			// 
			// colTemplateSource
			// 
			colTemplateSource.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			colTemplateSource.FillWeight = 56F;
			colTemplateSource.HeaderText = "Selected source file";
			colTemplateSource.Name = "colTemplateSource";
			colTemplateSource.ReadOnly = true;
			// 
			// btnAddTemplates
			// 
			btnAddTemplates.BackColor = Color.FromArgb(12, 21, 36);
			btnAddTemplates.FlatStyle = FlatStyle.Flat;
			btnAddTemplates.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnAddTemplates.ForeColor = Color.FromArgb(245, 247, 251);
			btnAddTemplates.Location = new Point(244, 2644);
			btnAddTemplates.Name = "btnAddTemplates";
			btnAddTemplates.Size = new Size(138, 42);
			btnAddTemplates.TabIndex = 68;
			btnAddTemplates.Text = "Add files";
			btnAddTemplates.UseVisualStyleBackColor = false;
			btnAddTemplates.Click += btnAddTemplates_Click;
			// 
			// btnRemoveTemplate
			// 
			btnRemoveTemplate.BackColor = Color.FromArgb(12, 21, 36);
			btnRemoveTemplate.FlatStyle = FlatStyle.Flat;
			btnRemoveTemplate.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnRemoveTemplate.ForeColor = Color.FromArgb(245, 247, 251);
			btnRemoveTemplate.Location = new Point(390, 2644);
			btnRemoveTemplate.Name = "btnRemoveTemplate";
			btnRemoveTemplate.Size = new Size(138, 42);
			btnRemoveTemplate.TabIndex = 69;
			btnRemoveTemplate.Text = "Remove selected";
			btnRemoveTemplate.UseVisualStyleBackColor = false;
			btnRemoveTemplate.Click += btnRemoveTemplate_Click;
			// 
			// lblSteamAppConfig
			// 
			lblSteamAppConfig.AutoSize = true;
			lblSteamAppConfig.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblSteamAppConfig.ForeColor = Color.FromArgb(158, 172, 194);
			lblSteamAppConfig.Location = new Point(12, 2702);
			lblSteamAppConfig.Name = "lblSteamAppConfig";
			lblSteamAppConfig.Size = new Size(263, 15);
			lblSteamAppConfig.TabIndex = 70;
			lblSteamAppConfig.Text = "SteamCMD app configuration (normally blank)";
			// 
			// txtSteamAppConfig
			// 
			txtSteamAppConfig.BackColor = Color.FromArgb(12, 21, 36);
			txtSteamAppConfig.BorderStyle = BorderStyle.FixedSingle;
			txtSteamAppConfig.Font = new Font("Segoe UI", 10F);
			txtSteamAppConfig.ForeColor = Color.FromArgb(245, 247, 251);
			txtSteamAppConfig.Location = new Point(12, 2728);
			txtSteamAppConfig.Name = "txtSteamAppConfig";
			txtSteamAppConfig.Size = new Size(516, 25);
			txtSteamAppConfig.TabIndex = 71;
			// 
			// lblRuntimeSection
			// 
			lblRuntimeSection.AutoSize = true;
			lblRuntimeSection.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
			lblRuntimeSection.ForeColor = Color.FromArgb(32, 214, 199);
			lblRuntimeSection.Location = new Point(12, 2794);
			lblRuntimeSection.Name = "lblRuntimeSection";
			lblRuntimeSection.Size = new Size(167, 20);
			lblRuntimeSection.TabIndex = 72;
			lblRuntimeSection.Text = "Runtime requirements";
			// 
			// lblRuntimeSectionHelp
			// 
			lblRuntimeSectionHelp.Font = new Font("Segoe UI", 8.5F);
			lblRuntimeSectionHelp.ForeColor = Color.FromArgb(158, 172, 194);
			lblRuntimeSectionHelp.Location = new Point(12, 2823);
			lblRuntimeSectionHelp.Name = "lblRuntimeSectionHelp";
			lblRuntimeSectionHelp.Size = new Size(516, 42);
			lblRuntimeSectionHelp.TabIndex = 73;
			lblRuntimeSectionHelp.Text = "Verified hardware and Windows requirements checked before Synix installs or launches the server.";
			// 
			// lblMinimumRam
			// 
			lblMinimumRam.AutoSize = true;
			lblMinimumRam.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblMinimumRam.ForeColor = Color.FromArgb(158, 172, 194);
			lblMinimumRam.Location = new Point(12, 2880);
			lblMinimumRam.Name = "lblMinimumRam";
			lblMinimumRam.Size = new Size(295, 15);
			lblMinimumRam.TabIndex = 74;
			lblMinimumRam.Text = "Minimum system RAM in GB (0 means no minimum)";
			// 
			// numMinimumRam
			// 
			numMinimumRam.AccessibleRole = AccessibleRole.SpinButton;
			numMinimumRam.BackColor = Color.FromArgb(12, 21, 36);
			numMinimumRam.Font = new Font("Segoe UI", 11F);
			numMinimumRam.ForeColor = Color.FromArgb(245, 247, 251);
			numMinimumRam.Location = new Point(12, 2906);
			numMinimumRam.Maximum = 1024;
			numMinimumRam.Name = "numMinimumRam";
			numMinimumRam.Size = new Size(250, 42);
			numMinimumRam.TabIndex = 75;
			// 
			// lblDotNetFramework
			// 
			lblDotNetFramework.AutoSize = true;
			lblDotNetFramework.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblDotNetFramework.ForeColor = Color.FromArgb(158, 172, 194);
			lblDotNetFramework.Location = new Point(12, 2962);
			lblDotNetFramework.Name = "lblDotNetFramework";
			lblDotNetFramework.Size = new Size(173, 15);
			lblDotNetFramework.TabIndex = 76;
			lblDotNetFramework.Text = ".NET Framework requirement";
			// 
			// cmbDotNetFramework
			// 
			cmbDotNetFramework.ArrowColor = Color.FromArgb(158, 172, 194);
			cmbDotNetFramework.BackColor = Color.FromArgb(12, 21, 36);
			cmbDotNetFramework.BorderColor = Color.FromArgb(38, 52, 77);
			cmbDotNetFramework.DrawMode = DrawMode.OwnerDrawFixed;
			cmbDotNetFramework.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbDotNetFramework.FlatStyle = FlatStyle.Flat;
			cmbDotNetFramework.FocusBorderColor = Color.FromArgb(38, 52, 77);
			cmbDotNetFramework.Font = new Font("Segoe UI", 10F);
			cmbDotNetFramework.ForeColor = Color.FromArgb(245, 247, 251);
			cmbDotNetFramework.ItemHeight = 28;
			cmbDotNetFramework.Location = new Point(12, 2988);
			cmbDotNetFramework.Name = "cmbDotNetFramework";
			cmbDotNetFramework.SelectedItemBackColor = Color.FromArgb(24, 55, 73);
			cmbDotNetFramework.Size = new Size(516, 34);
			cmbDotNetFramework.TabIndex = 77;
			// 
			// lblDotNetFrameworkHelp
			// 
			lblDotNetFrameworkHelp.Font = new Font("Segoe UI", 8.5F);
			lblDotNetFrameworkHelp.ForeColor = Color.FromArgb(158, 172, 194);
			lblDotNetFrameworkHelp.Location = new Point(12, 3038);
			lblDotNetFrameworkHelp.Name = "lblDotNetFrameworkHelp";
			lblDotNetFrameworkHelp.Size = new Size(516, 34);
			lblDotNetFrameworkHelp.TabIndex = 78;
			lblDotNetFrameworkHelp.Text = "Checks the installed Windows .NET Framework release before the server starts.";
			// 
			// chkRequiresVisualCpp2013
			// 
			chkRequiresVisualCpp2013.AccessibleName = "Require Visual C++ 2013 x64 runtime";
			chkRequiresVisualCpp2013.AccessibleRole = AccessibleRole.CheckButton;
			chkRequiresVisualCpp2013.BackColor = Color.FromArgb(17, 27, 45);
			chkRequiresVisualCpp2013.Location = new Point(474, 3102);
			chkRequiresVisualCpp2013.Name = "chkRequiresVisualCpp2013";
			chkRequiresVisualCpp2013.Size = new Size(54, 30);
			chkRequiresVisualCpp2013.TabIndex = 79;
			chkRequiresVisualCpp2013.UseVisualStyleBackColor = false;
			// 
			// lblRequiresVisualCpp2013Option
			// 
			lblRequiresVisualCpp2013Option.AutoSize = true;
			lblRequiresVisualCpp2013Option.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblRequiresVisualCpp2013Option.ForeColor = Color.FromArgb(245, 247, 251);
			lblRequiresVisualCpp2013Option.Location = new Point(12, 3096);
			lblRequiresVisualCpp2013Option.Name = "lblRequiresVisualCpp2013Option";
			lblRequiresVisualCpp2013Option.Size = new Size(215, 15);
			lblRequiresVisualCpp2013Option.TabIndex = 80;
			lblRequiresVisualCpp2013Option.Text = "Require Visual C++ 2013 x64 runtime";
			// 
			// lblRequiresVisualCpp2013Help
			// 
			lblRequiresVisualCpp2013Help.Font = new Font("Segoe UI", 8.5F);
			lblRequiresVisualCpp2013Help.ForeColor = Color.FromArgb(158, 172, 194);
			lblRequiresVisualCpp2013Help.Location = new Point(12, 3120);
			lblRequiresVisualCpp2013Help.Name = "lblRequiresVisualCpp2013Help";
			lblRequiresVisualCpp2013Help.Size = new Size(438, 34);
			lblRequiresVisualCpp2013Help.TabIndex = 81;
			lblRequiresVisualCpp2013Help.Text = "Blocks launch with clear Microsoft download guidance when the runtime is missing.";
			// 
			// chkRequiresVisualCpp2015To2022
			// 
			chkRequiresVisualCpp2015To2022.AccessibleName = "Require Visual C++ 2015-2022 x64 runtime";
			chkRequiresVisualCpp2015To2022.AccessibleRole = AccessibleRole.CheckButton;
			chkRequiresVisualCpp2015To2022.BackColor = Color.FromArgb(17, 27, 45);
			chkRequiresVisualCpp2015To2022.Location = new Point(474, 3174);
			chkRequiresVisualCpp2015To2022.Name = "chkRequiresVisualCpp2015To2022";
			chkRequiresVisualCpp2015To2022.Size = new Size(54, 30);
			chkRequiresVisualCpp2015To2022.TabIndex = 82;
			chkRequiresVisualCpp2015To2022.UseVisualStyleBackColor = false;
			// 
			// lblRequiresVisualCpp2015To2022Option
			// 
			lblRequiresVisualCpp2015To2022Option.AutoSize = true;
			lblRequiresVisualCpp2015To2022Option.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblRequiresVisualCpp2015To2022Option.ForeColor = Color.FromArgb(245, 247, 251);
			lblRequiresVisualCpp2015To2022Option.Location = new Point(12, 3168);
			lblRequiresVisualCpp2015To2022Option.Name = "lblRequiresVisualCpp2015To2022Option";
			lblRequiresVisualCpp2015To2022Option.Size = new Size(248, 15);
			lblRequiresVisualCpp2015To2022Option.TabIndex = 83;
			lblRequiresVisualCpp2015To2022Option.Text = "Require Visual C++ 2015-2022 x64 runtime";
			// 
			// lblRequiresVisualCpp2015To2022Help
			// 
			lblRequiresVisualCpp2015To2022Help.Font = new Font("Segoe UI", 8.5F);
			lblRequiresVisualCpp2015To2022Help.ForeColor = Color.FromArgb(158, 172, 194);
			lblRequiresVisualCpp2015To2022Help.Location = new Point(12, 3192);
			lblRequiresVisualCpp2015To2022Help.Name = "lblRequiresVisualCpp2015To2022Help";
			lblRequiresVisualCpp2015To2022Help.Size = new Size(438, 34);
			lblRequiresVisualCpp2015To2022Help.TabIndex = 84;
			lblRequiresVisualCpp2015To2022Help.Text = "Covers the unified Microsoft runtime used by current 2015, 2017, 2019, and 2022 servers.";
			// 
			// chkRequiresAvx2
			// 
			chkRequiresAvx2.AccessibleName = "Require an AVX2-capable processor";
			chkRequiresAvx2.AccessibleRole = AccessibleRole.CheckButton;
			chkRequiresAvx2.BackColor = Color.FromArgb(17, 27, 45);
			chkRequiresAvx2.Location = new Point(474, 3234);
			chkRequiresAvx2.Name = "chkRequiresAvx2";
			chkRequiresAvx2.Size = new Size(54, 30);
			chkRequiresAvx2.TabIndex = 85;
			chkRequiresAvx2.UseVisualStyleBackColor = false;
			// 
			// lblRequiresAvx2Option
			// 
			lblRequiresAvx2Option.AutoSize = true;
			lblRequiresAvx2Option.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblRequiresAvx2Option.ForeColor = Color.FromArgb(245, 247, 251);
			lblRequiresAvx2Option.Location = new Point(12, 3228);
			lblRequiresAvx2Option.Name = "lblRequiresAvx2Option";
			lblRequiresAvx2Option.Size = new Size(204, 15);
			lblRequiresAvx2Option.TabIndex = 86;
			lblRequiresAvx2Option.Text = "Require an AVX2-capable processor";
			// 
			// lblRequiresAvx2Help
			// 
			lblRequiresAvx2Help.Font = new Font("Segoe UI", 8.5F);
			lblRequiresAvx2Help.ForeColor = Color.FromArgb(158, 172, 194);
			lblRequiresAvx2Help.Location = new Point(12, 3252);
			lblRequiresAvx2Help.Name = "lblRequiresAvx2Help";
			lblRequiresAvx2Help.Size = new Size(438, 34);
			lblRequiresAvx2Help.TabIndex = 87;
			lblRequiresAvx2Help.Text = "Blocks setup with a clear message when the processor does not support AVX2.";
			// 
			// chkRequiresVirtualization
			// 
			chkRequiresVirtualization.AccessibleName = "Require hardware virtualization";
			chkRequiresVirtualization.AccessibleRole = AccessibleRole.CheckButton;
			chkRequiresVirtualization.BackColor = Color.FromArgb(17, 27, 45);
			chkRequiresVirtualization.Location = new Point(474, 3306);
			chkRequiresVirtualization.Name = "chkRequiresVirtualization";
			chkRequiresVirtualization.Size = new Size(54, 30);
			chkRequiresVirtualization.TabIndex = 88;
			chkRequiresVirtualization.UseVisualStyleBackColor = false;
			// 
			// lblRequiresVirtualizationOption
			// 
			lblRequiresVirtualizationOption.AutoSize = true;
			lblRequiresVirtualizationOption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblRequiresVirtualizationOption.ForeColor = Color.FromArgb(245, 247, 251);
			lblRequiresVirtualizationOption.Location = new Point(12, 3300);
			lblRequiresVirtualizationOption.Name = "lblRequiresVirtualizationOption";
			lblRequiresVirtualizationOption.Size = new Size(183, 15);
			lblRequiresVirtualizationOption.TabIndex = 89;
			lblRequiresVirtualizationOption.Text = "Require hardware virtualization";
			// 
			// lblRequiresVirtualizationHelp
			// 
			lblRequiresVirtualizationHelp.Font = new Font("Segoe UI", 8.5F);
			lblRequiresVirtualizationHelp.ForeColor = Color.FromArgb(158, 172, 194);
			lblRequiresVirtualizationHelp.Location = new Point(12, 3324);
			lblRequiresVirtualizationHelp.Name = "lblRequiresVirtualizationHelp";
			lblRequiresVirtualizationHelp.Size = new Size(438, 34);
			lblRequiresVirtualizationHelp.TabIndex = 90;
			lblRequiresVirtualizationHelp.Text = "Checks whether virtualization support is enabled and available to Windows.";
			// 
			// chkRequiresHyperV
			// 
			chkRequiresHyperV.AccessibleName = "Require Microsoft Hyper-V";
			chkRequiresHyperV.AccessibleRole = AccessibleRole.CheckButton;
			chkRequiresHyperV.BackColor = Color.FromArgb(17, 27, 45);
			chkRequiresHyperV.Location = new Point(474, 3378);
			chkRequiresHyperV.Name = "chkRequiresHyperV";
			chkRequiresHyperV.Size = new Size(54, 30);
			chkRequiresHyperV.TabIndex = 91;
			chkRequiresHyperV.UseVisualStyleBackColor = false;
			chkRequiresHyperV.CheckedChanged += chkRequiresHyperV_CheckedChanged;
			// 
			// lblRequiresHyperVOption
			// 
			lblRequiresHyperVOption.AutoSize = true;
			lblRequiresHyperVOption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblRequiresHyperVOption.ForeColor = Color.FromArgb(245, 247, 251);
			lblRequiresHyperVOption.Location = new Point(12, 3372);
			lblRequiresHyperVOption.Name = "lblRequiresHyperVOption";
			lblRequiresHyperVOption.Size = new Size(157, 15);
			lblRequiresHyperVOption.TabIndex = 92;
			lblRequiresHyperVOption.Text = "Require Microsoft Hyper-V";
			// 
			// lblRequiresHyperVHelp
			// 
			lblRequiresHyperVHelp.Font = new Font("Segoe UI", 8.5F);
			lblRequiresHyperVHelp.ForeColor = Color.FromArgb(158, 172, 194);
			lblRequiresHyperVHelp.Location = new Point(12, 3396);
			lblRequiresHyperVHelp.Name = "lblRequiresHyperVHelp";
			lblRequiresHyperVHelp.Size = new Size(438, 34);
			lblRequiresHyperVHelp.TabIndex = 93;
			lblRequiresHyperVHelp.Text = "Use only when the server is deployed through Hyper-V or Windows containers.";
			// 
			// chkRequiresWindowsPro
			// 
			chkRequiresWindowsPro.AccessibleName = "Require Windows Professional or higher";
			chkRequiresWindowsPro.AccessibleRole = AccessibleRole.CheckButton;
			chkRequiresWindowsPro.BackColor = Color.FromArgb(17, 27, 45);
			chkRequiresWindowsPro.Location = new Point(474, 3450);
			chkRequiresWindowsPro.Name = "chkRequiresWindowsPro";
			chkRequiresWindowsPro.Size = new Size(54, 30);
			chkRequiresWindowsPro.TabIndex = 94;
			chkRequiresWindowsPro.UseVisualStyleBackColor = false;
			// 
			// lblRequiresWindowsProOption
			// 
			lblRequiresWindowsProOption.AutoSize = true;
			lblRequiresWindowsProOption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblRequiresWindowsProOption.ForeColor = Color.FromArgb(245, 247, 251);
			lblRequiresWindowsProOption.Location = new Point(12, 3444);
			lblRequiresWindowsProOption.Name = "lblRequiresWindowsProOption";
			lblRequiresWindowsProOption.Size = new Size(229, 15);
			lblRequiresWindowsProOption.TabIndex = 95;
			lblRequiresWindowsProOption.Text = "Require Windows Professional or higher";
			// 
			// lblRequiresWindowsProHelp
			// 
			lblRequiresWindowsProHelp.Font = new Font("Segoe UI", 8.5F);
			lblRequiresWindowsProHelp.ForeColor = Color.FromArgb(158, 172, 194);
			lblRequiresWindowsProHelp.Location = new Point(12, 3468);
			lblRequiresWindowsProHelp.Name = "lblRequiresWindowsProHelp";
			lblRequiresWindowsProHelp.Size = new Size(438, 34);
			lblRequiresWindowsProHelp.TabIndex = 96;
			lblRequiresWindowsProHelp.Text = "Required for features such as Hyper-V that are unavailable on Windows Home.";
			// 
			// lblLaunchSection
			// 
			lblLaunchSection.AutoSize = true;
			lblLaunchSection.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
			lblLaunchSection.ForeColor = Color.FromArgb(32, 214, 199);
			lblLaunchSection.Location = new Point(12, 3522);
			lblLaunchSection.Name = "lblLaunchSection";
			lblLaunchSection.Size = new Size(124, 20);
			lblLaunchSection.TabIndex = 97;
			lblLaunchSection.Text = "Launch behavior";
			// 
			// lblLaunchSectionHelp
			// 
			lblLaunchSectionHelp.Font = new Font("Segoe UI", 8.5F);
			lblLaunchSectionHelp.ForeColor = Color.FromArgb(158, 172, 194);
			lblLaunchSectionHelp.Location = new Point(12, 3551);
			lblLaunchSectionHelp.Name = "lblLaunchSectionHelp";
			lblLaunchSectionHelp.Size = new Size(516, 42);
			lblLaunchSectionHelp.TabIndex = 98;
			lblLaunchSectionHelp.Text = "Choose only the built-in launch behavior verified for this dedicated server.";
			// 
			// chkRunElevated
			// 
			chkRunElevated.AccessibleName = "Launch with administrator permission";
			chkRunElevated.AccessibleRole = AccessibleRole.CheckButton;
			chkRunElevated.BackColor = Color.FromArgb(17, 27, 45);
			chkRunElevated.Location = new Point(474, 3614);
			chkRunElevated.Name = "chkRunElevated";
			chkRunElevated.Size = new Size(54, 30);
			chkRunElevated.TabIndex = 99;
			chkRunElevated.UseVisualStyleBackColor = false;
			// 
			// lblRunElevatedOption
			// 
			lblRunElevatedOption.AutoSize = true;
			lblRunElevatedOption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblRunElevatedOption.ForeColor = Color.FromArgb(245, 247, 251);
			lblRunElevatedOption.Location = new Point(12, 3608);
			lblRunElevatedOption.Name = "lblRunElevatedOption";
			lblRunElevatedOption.Size = new Size(215, 15);
			lblRunElevatedOption.TabIndex = 100;
			lblRunElevatedOption.Text = "Launch with administrator permission";
			// 
			// lblRunElevatedHelp
			// 
			lblRunElevatedHelp.Font = new Font("Segoe UI", 8.5F);
			lblRunElevatedHelp.ForeColor = Color.FromArgb(158, 172, 194);
			lblRunElevatedHelp.Location = new Point(12, 3632);
			lblRunElevatedHelp.Name = "lblRunElevatedHelp";
			lblRunElevatedHelp.Size = new Size(438, 34);
			lblRunElevatedHelp.TabIndex = 101;
			lblRunElevatedHelp.Text = "Enable only when the server cannot run correctly without Windows elevation.";
			// 
			// chkRequiresVisibleWindow
			// 
			chkRequiresVisibleWindow.AccessibleName = "Require the server manager window to remain visible";
			chkRequiresVisibleWindow.AccessibleRole = AccessibleRole.CheckButton;
			chkRequiresVisibleWindow.BackColor = Color.FromArgb(17, 27, 45);
			chkRequiresVisibleWindow.Location = new Point(474, 3686);
			chkRequiresVisibleWindow.Name = "chkRequiresVisibleWindow";
			chkRequiresVisibleWindow.Size = new Size(54, 30);
			chkRequiresVisibleWindow.TabIndex = 102;
			chkRequiresVisibleWindow.UseVisualStyleBackColor = false;
			// 
			// lblRequiresVisibleWindowOption
			// 
			lblRequiresVisibleWindowOption.AutoSize = true;
			lblRequiresVisibleWindowOption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblRequiresVisibleWindowOption.ForeColor = Color.FromArgb(245, 247, 251);
			lblRequiresVisibleWindowOption.Location = new Point(12, 3680);
			lblRequiresVisibleWindowOption.Name = "lblRequiresVisibleWindowOption";
			lblRequiresVisibleWindowOption.Size = new Size(236, 15);
			lblRequiresVisibleWindowOption.TabIndex = 103;
			lblRequiresVisibleWindowOption.Text = "Require a visible server manager window";
			// 
			// lblRequiresVisibleWindowHelp
			// 
			lblRequiresVisibleWindowHelp.Font = new Font("Segoe UI", 8.5F);
			lblRequiresVisibleWindowHelp.ForeColor = Color.FromArgb(158, 172, 194);
			lblRequiresVisibleWindowHelp.Location = new Point(12, 3704);
			lblRequiresVisibleWindowHelp.Name = "lblRequiresVisibleWindowHelp";
			lblRequiresVisibleWindowHelp.Size = new Size(438, 34);
			lblRequiresVisibleWindowHelp.TabIndex = 104;
			lblRequiresVisibleWindowHelp.Text = "Overrides Synix's hide-console preference for servers managed through their own window.";
			// 
			// lblLifecycleTracking
			// 
			lblLifecycleTracking.AutoSize = true;
			lblLifecycleTracking.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblLifecycleTracking.ForeColor = Color.FromArgb(158, 172, 194);
			lblLifecycleTracking.Location = new Point(12, 3754);
			lblLifecycleTracking.Name = "lblLifecycleTracking";
			lblLifecycleTracking.Size = new Size(143, 15);
			lblLifecycleTracking.TabIndex = 105;
			lblLifecycleTracking.Text = "Server lifecycle tracking";
			// 
			// cmbLifecycleTracking
			// 
			cmbLifecycleTracking.ArrowColor = Color.FromArgb(158, 172, 194);
			cmbLifecycleTracking.BackColor = Color.FromArgb(12, 21, 36);
			cmbLifecycleTracking.BorderColor = Color.FromArgb(38, 52, 77);
			cmbLifecycleTracking.DrawMode = DrawMode.OwnerDrawFixed;
			cmbLifecycleTracking.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbLifecycleTracking.FlatStyle = FlatStyle.Flat;
			cmbLifecycleTracking.FocusBorderColor = Color.FromArgb(38, 52, 77);
			cmbLifecycleTracking.Font = new Font("Segoe UI", 10F);
			cmbLifecycleTracking.ForeColor = Color.FromArgb(245, 247, 251);
			cmbLifecycleTracking.ItemHeight = 28;
			cmbLifecycleTracking.Location = new Point(12, 3780);
			cmbLifecycleTracking.Name = "cmbLifecycleTracking";
			cmbLifecycleTracking.SelectedItemBackColor = Color.FromArgb(24, 55, 73);
			cmbLifecycleTracking.Size = new Size(516, 34);
			cmbLifecycleTracking.TabIndex = 106;
			cmbLifecycleTracking.SelectedIndexChanged += cmbLifecycleTracking_SelectedIndexChanged;
			// 
			// lblLifecycleTrackingHelp
			// 
			lblLifecycleTrackingHelp.Font = new Font("Segoe UI", 8.5F);
			lblLifecycleTrackingHelp.ForeColor = Color.FromArgb(158, 172, 194);
			lblLifecycleTrackingHelp.Location = new Point(12, 3830);
			lblLifecycleTrackingHelp.Name = "lblLifecycleTrackingHelp";
			lblLifecycleTrackingHelp.Size = new Size(516, 34);
			lblLifecycleTrackingHelp.TabIndex = 107;
			lblLifecycleTrackingHelp.Text = "External deployment is for launchers or virtual machines and disables query monitoring.";
			// 
			// chkAllowLaunchExport
			// 
			chkAllowLaunchExport.AccessibleName = "Allow launch-file export";
			chkAllowLaunchExport.AccessibleRole = AccessibleRole.CheckButton;
			chkAllowLaunchExport.BackColor = Color.FromArgb(17, 27, 45);
			chkAllowLaunchExport.Checked = true;
			chkAllowLaunchExport.CheckState = CheckState.Checked;
			chkAllowLaunchExport.Location = new Point(474, 3886);
			chkAllowLaunchExport.Name = "chkAllowLaunchExport";
			chkAllowLaunchExport.Size = new Size(54, 30);
			chkAllowLaunchExport.TabIndex = 108;
			chkAllowLaunchExport.UseVisualStyleBackColor = false;
			// 
			// lblAllowLaunchExportOption
			// 
			lblAllowLaunchExportOption.AutoSize = true;
			lblAllowLaunchExportOption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblAllowLaunchExportOption.ForeColor = Color.FromArgb(245, 247, 251);
			lblAllowLaunchExportOption.Location = new Point(12, 3880);
			lblAllowLaunchExportOption.Name = "lblAllowLaunchExportOption";
			lblAllowLaunchExportOption.Size = new Size(141, 15);
			lblAllowLaunchExportOption.TabIndex = 109;
			lblAllowLaunchExportOption.Text = "Allow launch-file export";
			// 
			// lblAllowLaunchExportHelp
			// 
			lblAllowLaunchExportHelp.Font = new Font("Segoe UI", 8.5F);
			lblAllowLaunchExportHelp.ForeColor = Color.FromArgb(158, 172, 194);
			lblAllowLaunchExportHelp.Location = new Point(12, 3904);
			lblAllowLaunchExportHelp.Name = "lblAllowLaunchExportHelp";
			lblAllowLaunchExportHelp.Size = new Size(438, 48);
			lblAllowLaunchExportHelp.TabIndex = 110;
			lblAllowLaunchExportHelp.Text = "Lets the user create a reviewed launch file. Disable for deployment commands that must stay inside Synix.";
			// 
			// lblReadyMessage
			// 
			lblReadyMessage.AutoSize = true;
			lblReadyMessage.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblReadyMessage.ForeColor = Color.FromArgb(158, 172, 194);
			lblReadyMessage.Location = new Point(12, 3966);
			lblReadyMessage.Name = "lblReadyMessage";
			lblReadyMessage.Size = new Size(343, 15);
			lblReadyMessage.TabIndex = 111;
			lblReadyMessage.Text = "Message shown after special readiness checks pass (optional)";
			// 
			// txtReadyMessage
			// 
			txtReadyMessage.AcceptsReturn = true;
			txtReadyMessage.BackColor = Color.FromArgb(12, 21, 36);
			txtReadyMessage.BorderStyle = BorderStyle.FixedSingle;
			txtReadyMessage.Font = new Font("Segoe UI", 9.5F);
			txtReadyMessage.ForeColor = Color.FromArgb(245, 247, 251);
			txtReadyMessage.Location = new Point(12, 3992);
			txtReadyMessage.Multiline = true;
			txtReadyMessage.Name = "txtReadyMessage";
			txtReadyMessage.ScrollBars = ScrollBars.Vertical;
			txtReadyMessage.Size = new Size(516, 74);
			txtReadyMessage.TabIndex = 112;
			// 
			// lblLogPaths
			// 
			lblLogPaths.AutoSize = true;
			lblLogPaths.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblLogPaths.ForeColor = Color.FromArgb(158, 172, 194);
			lblLogPaths.Location = new Point(12, 4082);
			lblLogPaths.Name = "lblLogPaths";
			lblLogPaths.Size = new Size(378, 15);
			lblLogPaths.TabIndex = 113;
			lblLogPaths.Text = "Server log locations (one relative path or wildcard pattern per line)";
			// 
			// txtLogPaths
			// 
			txtLogPaths.AcceptsReturn = true;
			txtLogPaths.BackColor = Color.FromArgb(12, 21, 36);
			txtLogPaths.BorderStyle = BorderStyle.FixedSingle;
			txtLogPaths.Font = new Font("Segoe UI", 9.5F);
			txtLogPaths.ForeColor = Color.FromArgb(245, 247, 251);
			txtLogPaths.Location = new Point(12, 4108);
			txtLogPaths.Multiline = true;
			txtLogPaths.Name = "txtLogPaths";
			txtLogPaths.PlaceholderText = "Logs\\*.log\r\nSaved\\Logs\\**\\*.log";
			txtLogPaths.ScrollBars = ScrollBars.Vertical;
			txtLogPaths.Size = new Size(516, 76);
			txtLogPaths.TabIndex = 114;
			// 
			// lblRightPane
			// 
			lblRightPane.AutoSize = true;
			lblRightPane.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
			lblRightPane.ForeColor = Color.FromArgb(245, 247, 251);
			lblRightPane.Location = new Point(612, 112);
			lblRightPane.Name = "lblRightPane";
			lblRightPane.Size = new Size(242, 20);
			lblRightPane.TabIndex = 3;
			lblRightPane.Text = "Builder guide and supported tags";
			// 
			// btnShowGuide
			// 
			btnShowGuide.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnShowGuide.BackColor = Color.FromArgb(12, 21, 36);
			btnShowGuide.FlatStyle = FlatStyle.Flat;
			btnShowGuide.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnShowGuide.ForeColor = Color.FromArgb(245, 247, 251);
			btnShowGuide.Location = new Point(938, 104);
			btnShowGuide.Name = "btnShowGuide";
			btnShowGuide.Size = new Size(102, 34);
			btnShowGuide.TabIndex = 4;
			btnShowGuide.Text = "Guide";
			btnShowGuide.UseAccentStyle = true;
			btnShowGuide.UseVisualStyleBackColor = false;
			btnShowGuide.Click += btnShowGuide_Click;
			// 
			// btnShowPreview
			// 
			btnShowPreview.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnShowPreview.BackColor = Color.FromArgb(12, 21, 36);
			btnShowPreview.FlatStyle = FlatStyle.Flat;
			btnShowPreview.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnShowPreview.ForeColor = Color.FromArgb(245, 247, 251);
			btnShowPreview.Location = new Point(1048, 104);
			btnShowPreview.Name = "btnShowPreview";
			btnShowPreview.Size = new Size(104, 34);
			btnShowPreview.TabIndex = 5;
			btnShowPreview.Text = "Preview";
			btnShowPreview.UseVisualStyleBackColor = false;
			btnShowPreview.Click += btnShowPreview_Click;
			// 
			// rtbGuide
			// 
			rtbGuide.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			rtbGuide.BackColor = Color.FromArgb(12, 21, 36);
			rtbGuide.BorderStyle = BorderStyle.FixedSingle;
			rtbGuide.Font = new Font("Segoe UI", 9.25F);
			rtbGuide.ForeColor = Color.FromArgb(245, 247, 251);
			rtbGuide.Location = new Point(612, 142);
			rtbGuide.Name = "rtbGuide";
			rtbGuide.ReadOnly = true;
			rtbGuide.Size = new Size(540, 520);
			rtbGuide.TabIndex = 6;
			rtbGuide.Text = "";
			// 
			// rtbPreview
			// 
			rtbPreview.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			rtbPreview.BackColor = Color.FromArgb(12, 21, 36);
			rtbPreview.BorderStyle = BorderStyle.FixedSingle;
			rtbPreview.Font = new Font("Cascadia Mono", 9F);
			rtbPreview.ForeColor = Color.FromArgb(245, 247, 251);
			rtbPreview.Location = new Point(612, 142);
			rtbPreview.Name = "rtbPreview";
			rtbPreview.ReadOnly = true;
			rtbPreview.Size = new Size(540, 520);
			rtbPreview.TabIndex = 7;
			rtbPreview.Text = "";
			rtbPreview.Visible = false;
			rtbPreview.WordWrap = false;
			// 
			// lblStatus
			// 
			lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			lblStatus.Font = new Font("Segoe UI", 9F);
			lblStatus.ForeColor = Color.FromArgb(158, 172, 194);
			lblStatus.Location = new Point(612, 672);
			lblStatus.Name = "lblStatus";
			lblStatus.Size = new Size(540, 46);
			lblStatus.TabIndex = 8;
			lblStatus.Text = "Enter the game information, then validate before saving.";
			// 
			// btnValidate
			// 
			btnValidate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			btnValidate.BackColor = Color.FromArgb(12, 21, 36);
			btnValidate.FlatStyle = FlatStyle.Flat;
			btnValidate.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnValidate.ForeColor = Color.FromArgb(245, 247, 251);
			btnValidate.Location = new Point(802, 736);
			btnValidate.Name = "btnValidate";
			btnValidate.Size = new Size(164, 44);
			btnValidate.TabIndex = 9;
			btnValidate.Text = "Validate & Preview";
			btnValidate.UseVisualStyleBackColor = false;
			btnValidate.Click += btnValidate_Click;
			// 
			// btnSave
			// 
			btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			btnSave.BackColor = Color.FromArgb(12, 21, 36);
			btnSave.FlatStyle = FlatStyle.Flat;
			btnSave.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnSave.ForeColor = Color.FromArgb(245, 247, 251);
			btnSave.Location = new Point(980, 736);
			btnSave.Name = "btnSave";
			btnSave.Size = new Size(172, 44);
			btnSave.TabIndex = 10;
			btnSave.Text = "Save to Project";
			btnSave.UseAccentStyle = true;
			btnSave.UseVisualStyleBackColor = false;
			btnSave.Click += btnSave_Click;
			// 
			// GameDefinitionBuilder
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.FromArgb(8, 13, 24);
			ClientSize = new Size(1180, 804);
			Controls.Add(lblTitle);
			Controls.Add(lblDescription);
			Controls.Add(pnlInputs);
			Controls.Add(lblRightPane);
			Controls.Add(btnShowGuide);
			Controls.Add(btnShowPreview);
			Controls.Add(rtbGuide);
			Controls.Add(rtbPreview);
			Controls.Add(lblStatus);
			Controls.Add(btnValidate);
			Controls.Add(btnSave);
			Icon = (Icon)resources.GetObject("$this.Icon");
			MinimumSize = new Size(1040, 720);
			Name = "GameDefinitionBuilder";
			StartPosition = FormStartPosition.CenterParent;
			Text = "Synix Game Definition Builder";
			pnlInputs.ResumeLayout(false);
			pnlInputs.PerformLayout();
			(numCatalogOrder).EndInit();
			(numDefinitionRevision).EndInit();
			(numPort).EndInit();
			(numQueryPort).EndInit();
			(numConfigRevision).EndInit();
			((System.ComponentModel.ISupportInitialize)dgvAdditionalTemplates).EndInit();
			(numMinimumRam).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		private Label lblTitle = null!;
		private Label lblDescription = null!;
		private Panel pnlInputs = null!;
		private Label lblGame = null!;
		private TextBox txtGame = null!;
		private Label lblId = null!;
		private TextBox txtId = null!;
		private Label lblAppId = null!;
		private TextBox txtAppId = null!;
		private Label lblExecutable = null!;
		private TextBox txtExecutable = null!;
		private Label lblArguments = null!;
		private TextBox txtArguments = null!;
		private Label lblArgumentTag = null!;
		private ModernSettingsComboBox cmbArgumentTag = null!;
		private ModernSettingsButton btnInsertArgumentTag = null!;
		private Label lblRconSyntax = null!;
		private TextBox txtRconSyntax = null!;
		private Label lblCatalogOrder = null!;
		private ModernSettingsNumericUpDown numCatalogOrder = null!;
		private Label lblDefinitionRevision = null!;
		private ModernSettingsNumericUpDown numDefinitionRevision = null!;
		private Label lblPort = null!;
		private ModernSettingsNumericUpDown numPort = null!;
		private Label lblQueryPort = null!;
		private ModernSettingsNumericUpDown numQueryPort = null!;
		private Label lblConfigMode = null!;
		private ModernSettingsComboBox cmbConfigMode = null!;
		private Label lblFormat = null!;
		private ModernSettingsComboBox cmbFormat = null!;
		private Label lblConfigPath = null!;
		private TextBox txtConfigPath = null!;
		private Label lblTemplate = null!;
		private TextBox txtTemplate = null!;
		private ModernSettingsButton btnBrowseTemplate = null!;
		private Label lblConfigRevision = null!;
		private ModernSettingsNumericUpDown numConfigRevision = null!;
		private Label lblSteamTarget = null!;
		private TextBox txtSteamRuntimeTarget = null!;
		private Label lblMaps = null!;
		private TextBox txtMaps = null!;
		private Label lblGameModes = null!;
		private TextBox txtGameModes = null!;
		private Label lblRequiredLaunchFiles = null!;
		private TextBox txtRequiredLaunchFiles = null!;
		private Label lblOptionalLaunchFiles = null!;
		private TextBox txtOptionalLaunchFiles = null!;
		private Label lblExternalDataFolder = null!;
		private TextBox txtExternalDataFolder = null!;
		private Label lblSetupInstructions = null!;
		private TextBox txtSetupInstructions = null!;
		private Label lblIconUrl = null!;
		private TextBox txtIconUrl = null!;
		private ModernSettingsToggle chkSteamLogin = null!;
		private ModernSettingsToggle chkQueryable = null!;
		private ModernSettingsToggle chkSteamRuntime = null!;
		private ModernSettingsToggle chkFirstStartWarning = null!;
		private Label lblSteamLoginOption = null!;
		private Label lblSteamLoginHelp = null!;
		private Label lblQueryableOption = null!;
		private Label lblQueryableHelp = null!;
		private Label lblSteamRuntimeOption = null!;
		private Label lblSteamRuntimeHelp = null!;
		private Label lblFirstStartWarningOption = null!;
		private Label lblFirstStartWarningHelp = null!;
		private Label lblWarningMessage = null!;
		private TextBox txtWarningMessage = null!;
		private Label lblAdditionalTemplates = null!;
		private Label lblAdditionalTemplatesHelp = null!;
		private DataGridView dgvAdditionalTemplates = null!;
		private DataGridViewTextBoxColumn colTemplateDestination = null!;
		private DataGridViewTextBoxColumn colTemplateSource = null!;
		private ModernSettingsButton btnAddTemplates = null!;
		private ModernSettingsButton btnRemoveTemplate = null!;
		private Label lblSteamAppConfig = null!;
		private TextBox txtSteamAppConfig = null!;
		private Label lblConfigModeHelp = null!;
		private Label lblRuntimeSection = null!;
		private Label lblRuntimeSectionHelp = null!;
		private Label lblMinimumRam = null!;
		private ModernSettingsNumericUpDown numMinimumRam = null!;
		private Label lblDotNetFramework = null!;
		private ModernSettingsComboBox cmbDotNetFramework = null!;
		private Label lblDotNetFrameworkHelp = null!;
		private ModernSettingsToggle chkRequiresVisualCpp2013 = null!;
		private Label lblRequiresVisualCpp2013Option = null!;
		private Label lblRequiresVisualCpp2013Help = null!;
		private ModernSettingsToggle chkRequiresVisualCpp2015To2022 = null!;
		private Label lblRequiresVisualCpp2015To2022Option = null!;
		private Label lblRequiresVisualCpp2015To2022Help = null!;
		private ModernSettingsToggle chkRequiresAvx2 = null!;
		private Label lblRequiresAvx2Option = null!;
		private Label lblRequiresAvx2Help = null!;
		private ModernSettingsToggle chkRequiresVirtualization = null!;
		private Label lblRequiresVirtualizationOption = null!;
		private Label lblRequiresVirtualizationHelp = null!;
		private ModernSettingsToggle chkRequiresHyperV = null!;
		private Label lblRequiresHyperVOption = null!;
		private Label lblRequiresHyperVHelp = null!;
		private ModernSettingsToggle chkRequiresWindowsPro = null!;
		private Label lblRequiresWindowsProOption = null!;
		private Label lblRequiresWindowsProHelp = null!;
		private Label lblLaunchSection = null!;
		private Label lblLaunchSectionHelp = null!;
		private ModernSettingsToggle chkRunElevated = null!;
		private Label lblRunElevatedOption = null!;
		private Label lblRunElevatedHelp = null!;
		private ModernSettingsToggle chkRequiresVisibleWindow = null!;
		private Label lblRequiresVisibleWindowOption = null!;
		private Label lblRequiresVisibleWindowHelp = null!;
		private Label lblLifecycleTracking = null!;
		private ModernSettingsComboBox cmbLifecycleTracking = null!;
		private Label lblLifecycleTrackingHelp = null!;
		private ModernSettingsToggle chkAllowLaunchExport = null!;
		private Label lblAllowLaunchExportOption = null!;
		private Label lblAllowLaunchExportHelp = null!;
		private Label lblReadyMessage = null!;
		private TextBox txtReadyMessage = null!;
		private Label lblLogPaths = null!;
		private TextBox txtLogPaths = null!;
		private Label lblRightPane = null!;
		private ModernSettingsButton btnShowGuide = null!;
		private ModernSettingsButton btnShowPreview = null!;
		private RichTextBox rtbGuide = null!;
		private RichTextBox rtbPreview = null!;
		private Label lblStatus = null!;
		private ModernSettingsButton btnValidate = null!;
		private ModernSettingsButton btnSave = null!;
	}
}
