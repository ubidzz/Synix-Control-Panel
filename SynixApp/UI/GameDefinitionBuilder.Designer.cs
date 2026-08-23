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

namespace Synix_Control_Panel.SynixEngine
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
			cmbConfigMode = new ModernSettingsComboBox
			{
				Location = new Point(12, 566),
				Size = new Size(250, 42),
				DropDownStyle = ComboBoxStyle.DropDownList
			};
			lblFormat = new Label();
			cmbFormat = new ModernSettingsComboBox
			{
				Location = new Point(278, 566),
				Size = new Size(250, 42),
				DropDownStyle = ComboBoxStyle.DropDownList
			};
			lblConfigPath = new Label();
			txtConfigPath = new TextBox();
			lblTemplate = new Label();
			txtTemplate = new TextBox();
			btnBrowseTemplate = new ModernSettingsButton
			{
				Location = new Point(414, 722),
				Size = new Size(114, 38),
				Text = "Browse"
			};
			lblConfigRevision = new Label();
			numConfigRevision = new ModernSettingsNumericUpDown();
			lblSteamTarget = new Label();
			txtSteamRuntimeTarget = new TextBox();
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
			chkSteamLogin = new ModernSettingsToggle();
			chkQueryable = new ModernSettingsToggle();
			chkSteamRuntime = new ModernSettingsToggle();
			chkFirstStartWarning = new ModernSettingsToggle();
			lblSteamLoginOption = new Label();
			lblSteamLoginHelp = new Label();
			lblQueryableOption = new Label();
			lblQueryableHelp = new Label();
			lblSteamRuntimeOption = new Label();
			lblSteamRuntimeHelp = new Label();
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
			lblConfigModeHelp = new Label();

			lblRightPane = new Label();
			btnShowGuide = new ModernSettingsButton();
			btnShowPreview = new ModernSettingsButton();
			rtbGuide = new RichTextBox();
			rtbPreview = new RichTextBox();
			lblStatus = new Label();
			btnValidate = new ModernSettingsButton();
			btnSave = new ModernSettingsButton();

			SuspendLayout();
			pnlInputs.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)dgvAdditionalTemplates).BeginInit();

			lblTitle.AutoSize = true;
			lblTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
			lblTitle.ForeColor = SettingsPalette.PrimaryText;
			lblTitle.Location = new Point(28, 20);
			lblTitle.Text = "Game Definition Builder";

			lblDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblDescription.Font = new Font("Segoe UI", 9.5F);
			lblDescription.ForeColor = SettingsPalette.SecondaryText;
			lblDescription.Location = new Point(31, 62);
			lblDescription.Size = new Size(1110, 42);
			lblDescription.Text = "Create a validated built-in game definition without plugins or scripts. Definitions are saved into the project and become available only after Synix is rebuilt.";

			pnlInputs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			pnlInputs.AutoScroll = true;
			pnlInputs.AutoScrollMinSize = new Size(0, 2850);
			pnlInputs.BackColor = SettingsPalette.Card;
			pnlInputs.Location = new Point(28, 112);
			pnlInputs.Size = new Size(558, 606);

			lblGame.AutoSize = true;
			lblGame.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblGame.ForeColor = SettingsPalette.SecondaryText;
			lblGame.Location = new Point(12, 12);
			lblGame.Text = "Game name";
			txtGame.BackColor = SettingsPalette.Input;
			txtGame.BorderStyle = BorderStyle.FixedSingle;
			txtGame.Font = new Font("Segoe UI", 10F);
			txtGame.ForeColor = SettingsPalette.PrimaryText;
			txtGame.Location = new Point(12, 38);
			txtGame.Size = new Size(516, 38);

			lblId.AutoSize = true;
			lblId.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblId.ForeColor = SettingsPalette.SecondaryText;
			lblId.Location = new Point(12, 84);
			lblId.Text = "Definition ID";
			txtId.BackColor = SettingsPalette.Input;
			txtId.BorderStyle = BorderStyle.FixedSingle;
			txtId.Font = new Font("Segoe UI", 10F);
			txtId.ForeColor = SettingsPalette.PrimaryText;
			txtId.Location = new Point(12, 110);
			txtId.Size = new Size(516, 38);

			lblAppId.AutoSize = true;
			lblAppId.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblAppId.ForeColor = SettingsPalette.SecondaryText;
			lblAppId.Location = new Point(12, 156);
			lblAppId.Text = "Steam AppID";
			txtAppId.BackColor = SettingsPalette.Input;
			txtAppId.BorderStyle = BorderStyle.FixedSingle;
			txtAppId.Font = new Font("Segoe UI", 10F);
			txtAppId.ForeColor = SettingsPalette.PrimaryText;
			txtAppId.Location = new Point(12, 182);
			txtAppId.Size = new Size(516, 38);

			lblExecutable.AutoSize = true;
			lblExecutable.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblExecutable.ForeColor = SettingsPalette.SecondaryText;
			lblExecutable.Location = new Point(12, 228);
			lblExecutable.Text = "Server executable (relative path)";
			txtExecutable.BackColor = SettingsPalette.Input;
			txtExecutable.BorderStyle = BorderStyle.FixedSingle;
			txtExecutable.Font = new Font("Segoe UI", 10F);
			txtExecutable.ForeColor = SettingsPalette.PrimaryText;
			txtExecutable.Location = new Point(12, 254);
			txtExecutable.Size = new Size(516, 38);

			lblArguments.AutoSize = true;
			lblArguments.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblArguments.ForeColor = SettingsPalette.SecondaryText;
			lblArguments.Location = new Point(12, 300);
			lblArguments.Text = "Default launch arguments (everything after the executable)";
			txtArguments.BackColor = SettingsPalette.Input;
			txtArguments.BorderStyle = BorderStyle.FixedSingle;
			txtArguments.Font = new Font("Segoe UI", 10F);
			txtArguments.ForeColor = SettingsPalette.PrimaryText;
			txtArguments.Location = new Point(12, 326);
			txtArguments.Size = new Size(516, 38);

			lblArgumentTag.AutoSize = true;
			lblArgumentTag.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblArgumentTag.ForeColor = SettingsPalette.SecondaryText;
			lblArgumentTag.Location = new Point(12, 372);
			lblArgumentTag.Text = "Insert a supported Synix argument tag";
			cmbArgumentTag.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbArgumentTag.Location = new Point(12, 396);
			cmbArgumentTag.Size = new Size(390, 42);
			btnInsertArgumentTag.Location = new Point(414, 396);
			btnInsertArgumentTag.Size = new Size(114, 42);
			btnInsertArgumentTag.Text = "Insert tag";
			btnInsertArgumentTag.Click += btnInsertArgumentTag_Click;

			lblRconSyntax.AutoSize = true;
			lblRconSyntax.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblRconSyntax.ForeColor = SettingsPalette.SecondaryText;
			lblRconSyntax.Location = new Point(12, 454);
			lblRconSyntax.Text = "Optional RCON syntax — launch arguments must contain {rcon}";
			txtRconSyntax.BackColor = SettingsPalette.Input;
			txtRconSyntax.BorderStyle = BorderStyle.FixedSingle;
			txtRconSyntax.Font = new Font("Segoe UI", 10F);
			txtRconSyntax.ForeColor = SettingsPalette.PrimaryText;
			txtRconSyntax.Location = new Point(12, 480);
			txtRconSyntax.Size = new Size(516, 38);

			lblCatalogOrder.AutoSize = true;
			lblCatalogOrder.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblCatalogOrder.ForeColor = SettingsPalette.SecondaryText;
			lblCatalogOrder.Location = new Point(12, 536);
			lblCatalogOrder.Text = "Catalog order";
			numCatalogOrder.Location = new Point(12, 562);
			numCatalogOrder.Maximum = 10000;
			numCatalogOrder.Size = new Size(250, 42);
			lblDefinitionRevision.AutoSize = true;
			lblDefinitionRevision.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblDefinitionRevision.ForeColor = SettingsPalette.SecondaryText;
			lblDefinitionRevision.Location = new Point(278, 536);
			lblDefinitionRevision.Text = "Definition revision";
			numDefinitionRevision.Location = new Point(278, 562);
			numDefinitionRevision.Minimum = 1;
			numDefinitionRevision.Maximum = 10000;
			numDefinitionRevision.Value = 1;
			numDefinitionRevision.Size = new Size(250, 42);

			lblPort.AutoSize = true;
			lblPort.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblPort.ForeColor = SettingsPalette.SecondaryText;
			lblPort.Location = new Point(12, 620);
			lblPort.Text = "Game port";
			numPort.Location = new Point(12, 646);
			numPort.Minimum = 1;
			numPort.Maximum = 65535;
			numPort.Value = 7777;
			numPort.Size = new Size(250, 42);
			lblQueryPort.AutoSize = true;
			lblQueryPort.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblQueryPort.ForeColor = SettingsPalette.SecondaryText;
			lblQueryPort.Location = new Point(278, 620);
			lblQueryPort.Text = "Query port";
			numQueryPort.Location = new Point(278, 646);
			numQueryPort.Minimum = 1;
			numQueryPort.Maximum = 65535;
			numQueryPort.Value = 27015;
			numQueryPort.Size = new Size(250, 42);

			lblConfigMode.AutoSize = true;
			lblConfigMode.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblConfigMode.ForeColor = SettingsPalette.SecondaryText;
			lblConfigMode.Location = new Point(12, 704);
			lblConfigMode.Text = "Configuration behavior";
			lblFormat.AutoSize = true;
			lblFormat.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblFormat.ForeColor = SettingsPalette.SecondaryText;
			lblFormat.Location = new Point(278, 704);
			lblFormat.Text = "Configuration format";
			cmbConfigMode.Location = new Point(12, 730);
			cmbFormat.Location = new Point(278, 730);
			lblConfigModeHelp.Font = new Font("Segoe UI", 8.5F);
			lblConfigModeHelp.ForeColor = SettingsPalette.SecondaryText;
			lblConfigModeHelp.Location = new Point(12, 780);
			lblConfigModeHelp.Size = new Size(516, 48);

			lblConfigPath.AutoSize = true;
			lblConfigPath.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblConfigPath.ForeColor = SettingsPalette.SecondaryText;
			lblConfigPath.Location = new Point(12, 840);
			lblConfigPath.Text = "Configuration path relative to the installed server folder";
			txtConfigPath.BackColor = SettingsPalette.Input;
			txtConfigPath.BorderStyle = BorderStyle.FixedSingle;
			txtConfigPath.Font = new Font("Segoe UI", 10F);
			txtConfigPath.ForeColor = SettingsPalette.PrimaryText;
			txtConfigPath.Location = new Point(12, 866);
			txtConfigPath.Size = new Size(516, 38);

			lblTemplate.AutoSize = true;
			lblTemplate.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblTemplate.ForeColor = SettingsPalette.SecondaryText;
			lblTemplate.Location = new Point(12, 912);
			lblTemplate.Text = "Complete, working configuration template file";
			txtTemplate.BackColor = SettingsPalette.Input;
			txtTemplate.BorderStyle = BorderStyle.FixedSingle;
			txtTemplate.Font = new Font("Segoe UI", 10F);
			txtTemplate.ForeColor = SettingsPalette.PrimaryText;
			txtTemplate.Location = new Point(12, 938);
			txtTemplate.Size = new Size(394, 38);
			btnBrowseTemplate.Location = new Point(414, 938);

			lblConfigRevision.AutoSize = true;
			lblConfigRevision.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblConfigRevision.ForeColor = SettingsPalette.SecondaryText;
			lblConfigRevision.Location = new Point(12, 990);
			lblConfigRevision.Text = "Template revision";
			numConfigRevision.Location = new Point(12, 1016);
			numConfigRevision.Minimum = 1;
			numConfigRevision.Maximum = 10000;
			numConfigRevision.Value = 1;
			numConfigRevision.Size = new Size(250, 42);

			lblSteamTarget.AutoSize = true;
			lblSteamTarget.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblSteamTarget.ForeColor = SettingsPalette.SecondaryText;
			lblSteamTarget.Location = new Point(12, 1074);
			lblSteamTarget.Text = "Steam runtime target directory (relative path)";
			txtSteamRuntimeTarget.BackColor = SettingsPalette.Input;
			txtSteamRuntimeTarget.BorderStyle = BorderStyle.FixedSingle;
			txtSteamRuntimeTarget.Font = new Font("Segoe UI", 10F);
			txtSteamRuntimeTarget.ForeColor = SettingsPalette.PrimaryText;
			txtSteamRuntimeTarget.Location = new Point(12, 1100);
			txtSteamRuntimeTarget.Size = new Size(516, 38);

			chkSteamLogin.BackColor = SettingsPalette.Card;
			chkSteamLogin.Location = new Point(474, 1164);
			chkSteamLogin.AccessibleName = "Steam account login required";
			chkSteamLogin.Size = new Size(54, 30);
			lblSteamLoginOption.AutoSize = true;
			lblSteamLoginOption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblSteamLoginOption.ForeColor = SettingsPalette.PrimaryText;
			lblSteamLoginOption.Location = new Point(12, 1158);
			lblSteamLoginOption.Text = "Steam account login required";
			lblSteamLoginHelp.Font = new Font("Segoe UI", 8.5F);
			lblSteamLoginHelp.ForeColor = SettingsPalette.SecondaryText;
			lblSteamLoginHelp.Location = new Point(12, 1182);
			lblSteamLoginHelp.Size = new Size(438, 34);
			lblSteamLoginHelp.Text = "Enable only when anonymous SteamCMD installation fails and a Steam account is required.";

			chkQueryable.BackColor = SettingsPalette.Card;
			chkQueryable.Checked = true;
			chkQueryable.Location = new Point(474, 1232);
			chkQueryable.AccessibleName = "Enable server query monitoring";
			chkQueryable.Size = new Size(54, 30);
			lblQueryableOption.AutoSize = true;
			lblQueryableOption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblQueryableOption.ForeColor = SettingsPalette.PrimaryText;
			lblQueryableOption.Location = new Point(12, 1226);
			lblQueryableOption.Text = "Enable server query monitoring";
			lblQueryableHelp.Font = new Font("Segoe UI", 8.5F);
			lblQueryableHelp.ForeColor = SettingsPalette.SecondaryText;
			lblQueryableHelp.Location = new Point(12, 1250);
			lblQueryableHelp.Size = new Size(438, 34);
			lblQueryableHelp.Text = "Enable when the server has a verified query or network probe that Synix can monitor.";

			chkSteamRuntime.BackColor = SettingsPalette.Card;
			chkSteamRuntime.Location = new Point(474, 1300);
			chkSteamRuntime.AccessibleName = "Copy allowlisted Steam runtime files after install";
			chkSteamRuntime.Size = new Size(54, 30);
			chkSteamRuntime.CheckedChanged += chkSteamRuntime_CheckedChanged;
			lblSteamRuntimeOption.AutoSize = true;
			lblSteamRuntimeOption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblSteamRuntimeOption.ForeColor = SettingsPalette.PrimaryText;
			lblSteamRuntimeOption.Location = new Point(12, 1294);
			lblSteamRuntimeOption.Text = "Copy approved Steam runtime files after installation";
			lblSteamRuntimeHelp.Font = new Font("Segoe UI", 8.5F);
			lblSteamRuntimeHelp.ForeColor = SettingsPalette.SecondaryText;
			lblSteamRuntimeHelp.Location = new Point(12, 1318);
			lblSteamRuntimeHelp.Size = new Size(438, 48);
			lblSteamRuntimeHelp.Text = "Use only when testing proves the server needs the approved Steam DLL files. The target must stay inside the server folder.";

			lblMaps.AutoSize = true;
			lblMaps.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblMaps.ForeColor = SettingsPalette.SecondaryText;
			lblMaps.Location = new Point(12, 1384);
			lblMaps.Text = "Maps or scenarios (one exact value per line)";
			txtMaps.AcceptsReturn = true;
			txtMaps.BackColor = SettingsPalette.Input;
			txtMaps.BorderStyle = BorderStyle.FixedSingle;
			txtMaps.Font = new Font("Segoe UI", 9.5F);
			txtMaps.ForeColor = SettingsPalette.PrimaryText;
			txtMaps.Location = new Point(12, 1410);
			txtMaps.Multiline = true;
			txtMaps.ScrollBars = ScrollBars.Vertical;
			txtMaps.Size = new Size(516, 90);
			txtMaps.WordWrap = false;

			lblGameModes.AutoSize = true;
			lblGameModes.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblGameModes.ForeColor = SettingsPalette.SecondaryText;
			lblGameModes.Location = new Point(12, 1516);
			lblGameModes.Text = "Game modes (one exact value per line)";
			txtGameModes.AcceptsReturn = true;
			txtGameModes.BackColor = SettingsPalette.Input;
			txtGameModes.BorderStyle = BorderStyle.FixedSingle;
			txtGameModes.Font = new Font("Segoe UI", 9.5F);
			txtGameModes.ForeColor = SettingsPalette.PrimaryText;
			txtGameModes.Location = new Point(12, 1542);
			txtGameModes.Multiline = true;
			txtGameModes.ScrollBars = ScrollBars.Vertical;
			txtGameModes.Size = new Size(516, 70);
			txtGameModes.WordWrap = false;

			lblRequiredLaunchFiles.AutoSize = true;
			lblRequiredLaunchFiles.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblRequiredLaunchFiles.ForeColor = SettingsPalette.SecondaryText;
			lblRequiredLaunchFiles.Location = new Point(12, 1628);
			lblRequiredLaunchFiles.Text = "Required user-supplied files (relative paths, one per line)";
			txtRequiredLaunchFiles.AcceptsReturn = true;
			txtRequiredLaunchFiles.BackColor = SettingsPalette.Input;
			txtRequiredLaunchFiles.BorderStyle = BorderStyle.FixedSingle;
			txtRequiredLaunchFiles.Font = new Font("Segoe UI", 9.5F);
			txtRequiredLaunchFiles.ForeColor = SettingsPalette.PrimaryText;
			txtRequiredLaunchFiles.Location = new Point(12, 1654);
			txtRequiredLaunchFiles.Multiline = true;
			txtRequiredLaunchFiles.ScrollBars = ScrollBars.Vertical;
			txtRequiredLaunchFiles.Size = new Size(516, 84);
			txtRequiredLaunchFiles.WordWrap = false;

			lblOptionalLaunchFiles.AutoSize = true;
			lblOptionalLaunchFiles.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblOptionalLaunchFiles.ForeColor = SettingsPalette.SecondaryText;
			lblOptionalLaunchFiles.Location = new Point(12, 1754);
			lblOptionalLaunchFiles.Text = "Optional import files (relative paths, one per line)";
			txtOptionalLaunchFiles.AcceptsReturn = true;
			txtOptionalLaunchFiles.BackColor = SettingsPalette.Input;
			txtOptionalLaunchFiles.BorderStyle = BorderStyle.FixedSingle;
			txtOptionalLaunchFiles.Font = new Font("Segoe UI", 9.5F);
			txtOptionalLaunchFiles.ForeColor = SettingsPalette.PrimaryText;
			txtOptionalLaunchFiles.Location = new Point(12, 1780);
			txtOptionalLaunchFiles.Multiline = true;
			txtOptionalLaunchFiles.ScrollBars = ScrollBars.Vertical;
			txtOptionalLaunchFiles.Size = new Size(516, 84);
			txtOptionalLaunchFiles.WordWrap = false;

			lblExternalDataFolder.AutoSize = true;
			lblExternalDataFolder.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblExternalDataFolder.ForeColor = SettingsPalette.SecondaryText;
			lblExternalDataFolder.Location = new Point(12, 1880);
			lblExternalDataFolder.Text = "Documents source folder for automatic imports (optional)";
			txtExternalDataFolder.BackColor = SettingsPalette.Input;
			txtExternalDataFolder.BorderStyle = BorderStyle.FixedSingle;
			txtExternalDataFolder.Font = new Font("Segoe UI", 10F);
			txtExternalDataFolder.ForeColor = SettingsPalette.PrimaryText;
			txtExternalDataFolder.Location = new Point(12, 1906);
			txtExternalDataFolder.Size = new Size(516, 38);

			lblSetupInstructions.AutoSize = true;
			lblSetupInstructions.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblSetupInstructions.ForeColor = SettingsPalette.SecondaryText;
			lblSetupInstructions.Location = new Point(12, 1960);
			lblSetupInstructions.Text = "How the user obtains and places required game files";
			txtSetupInstructions.AcceptsReturn = true;
			txtSetupInstructions.BackColor = SettingsPalette.Input;
			txtSetupInstructions.BorderStyle = BorderStyle.FixedSingle;
			txtSetupInstructions.Font = new Font("Segoe UI", 9.5F);
			txtSetupInstructions.ForeColor = SettingsPalette.PrimaryText;
			txtSetupInstructions.Location = new Point(12, 1986);
			txtSetupInstructions.Multiline = true;
			txtSetupInstructions.ScrollBars = ScrollBars.Vertical;
			txtSetupInstructions.Size = new Size(516, 100);

			lblIconUrl.AutoSize = true;
			lblIconUrl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblIconUrl.ForeColor = SettingsPalette.SecondaryText;
			lblIconUrl.Location = new Point(12, 2102);
			lblIconUrl.Text = "Game icon HTTPS URL (optional)";
			txtIconUrl.BackColor = SettingsPalette.Input;
			txtIconUrl.BorderStyle = BorderStyle.FixedSingle;
			txtIconUrl.Font = new Font("Segoe UI", 10F);
			txtIconUrl.ForeColor = SettingsPalette.PrimaryText;
			txtIconUrl.Location = new Point(12, 2128);
			txtIconUrl.Size = new Size(516, 38);

			chkFirstStartWarning.BackColor = SettingsPalette.Card;
			chkFirstStartWarning.Location = new Point(474, 2188);
			chkFirstStartWarning.AccessibleName = "Show a first-start setup warning";
			chkFirstStartWarning.Size = new Size(54, 30);
			lblFirstStartWarningOption.AutoSize = true;
			lblFirstStartWarningOption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblFirstStartWarningOption.ForeColor = SettingsPalette.PrimaryText;
			lblFirstStartWarningOption.Location = new Point(12, 2182);
			lblFirstStartWarningOption.Text = "Show a first-start setup warning";
			lblFirstStartWarningHelp.Font = new Font("Segoe UI", 8.5F);
			lblFirstStartWarningHelp.ForeColor = SettingsPalette.SecondaryText;
			lblFirstStartWarningHelp.Location = new Point(12, 2206);
			lblFirstStartWarningHelp.Size = new Size(438, 34);
			lblFirstStartWarningHelp.Text = "Required files and Synix-created templates automatically enable a warning.";
			lblWarningMessage.AutoSize = true;
			lblWarningMessage.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblWarningMessage.ForeColor = SettingsPalette.SecondaryText;
			lblWarningMessage.Location = new Point(12, 2250);
			lblWarningMessage.Text = "First-start message shown to the user";
			txtWarningMessage.AcceptsReturn = true;
			txtWarningMessage.BackColor = SettingsPalette.Input;
			txtWarningMessage.BorderStyle = BorderStyle.FixedSingle;
			txtWarningMessage.Font = new Font("Segoe UI", 9.5F);
			txtWarningMessage.ForeColor = SettingsPalette.PrimaryText;
			txtWarningMessage.Location = new Point(12, 2276);
			txtWarningMessage.Multiline = true;
			txtWarningMessage.ScrollBars = ScrollBars.Vertical;
			txtWarningMessage.Size = new Size(516, 100);

			lblAdditionalTemplates.AutoSize = true;
			lblAdditionalTemplates.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblAdditionalTemplates.ForeColor = SettingsPalette.SecondaryText;
			lblAdditionalTemplates.Location = new Point(12, 2394);
			lblAdditionalTemplates.Text = "Additional configuration files";
			lblAdditionalTemplatesHelp.Font = new Font("Segoe UI", 8.5F);
			lblAdditionalTemplatesHelp.ForeColor = SettingsPalette.SecondaryText;
			lblAdditionalTemplatesHelp.Location = new Point(12, 2418);
			lblAdditionalTemplatesHelp.Size = new Size(516, 42);
			lblAdditionalTemplatesHelp.Text = "Add every other complete template the game needs. Edit Installed location so each path is relative to the installed server folder.";

			dgvAdditionalTemplates.AllowUserToAddRows = false;
			dgvAdditionalTemplates.AllowUserToDeleteRows = false;
			dgvAdditionalTemplates.AllowUserToResizeRows = false;
			dgvAdditionalTemplates.AutoGenerateColumns = false;
			dgvAdditionalTemplates.BackgroundColor = SettingsPalette.Input;
			dgvAdditionalTemplates.BorderStyle = BorderStyle.FixedSingle;
			dgvAdditionalTemplates.ColumnHeadersHeight = 36;
			dgvAdditionalTemplates.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			dgvAdditionalTemplates.Columns.AddRange(colTemplateDestination, colTemplateSource);
			dgvAdditionalTemplates.EnableHeadersVisualStyles = false;
			dgvAdditionalTemplates.Location = new Point(12, 2466);
			dgvAdditionalTemplates.MultiSelect = true;
			dgvAdditionalTemplates.RowHeadersVisible = false;
			dgvAdditionalTemplates.RowTemplate.Height = 34;
			dgvAdditionalTemplates.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dgvAdditionalTemplates.Size = new Size(516, 166);
		colTemplateDestination.HeaderText = "Installed location";
		colTemplateDestination.Name = "colTemplateDestination";
		colTemplateDestination.FillWeight = 44F;
		colTemplateDestination.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
		colTemplateSource.HeaderText = "Selected source file";
		colTemplateSource.Name = "colTemplateSource";
		colTemplateSource.FillWeight = 56F;
		colTemplateSource.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
		colTemplateSource.ReadOnly = true;

		btnAddTemplates.Location = new Point(244, 2644);
		btnAddTemplates.Size = new Size(138, 42);
		btnAddTemplates.Text = "Add files";
		btnAddTemplates.Click += btnAddTemplates_Click;
		btnRemoveTemplate.Location = new Point(390, 2644);
		btnRemoveTemplate.Size = new Size(138, 42);
			btnRemoveTemplate.Text = "Remove selected";
			btnRemoveTemplate.Click += btnRemoveTemplate_Click;

			lblSteamAppConfig.AutoSize = true;
			lblSteamAppConfig.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblSteamAppConfig.ForeColor = SettingsPalette.SecondaryText;
			lblSteamAppConfig.Location = new Point(12, 2702);
			lblSteamAppConfig.Text = "SteamCMD app configuration (normally blank)";
			txtSteamAppConfig.BackColor = SettingsPalette.Input;
			txtSteamAppConfig.BorderStyle = BorderStyle.FixedSingle;
			txtSteamAppConfig.Font = new Font("Segoe UI", 10F);
			txtSteamAppConfig.ForeColor = SettingsPalette.PrimaryText;
			txtSteamAppConfig.Location = new Point(12, 2728);
			txtSteamAppConfig.Size = new Size(516, 38);

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

			lblRightPane.AutoSize = true;
			lblRightPane.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
			lblRightPane.ForeColor = SettingsPalette.PrimaryText;
			lblRightPane.Location = new Point(612, 112);
			lblRightPane.Text = "Builder guide and supported tags";

			btnShowGuide.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnShowGuide.Location = new Point(938, 104);
			btnShowGuide.Size = new Size(102, 34);
			btnShowGuide.Text = "Guide";
			btnShowGuide.UseAccentStyle = true;
			btnShowGuide.Click += btnShowGuide_Click;
			btnShowPreview.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnShowPreview.Location = new Point(1048, 104);
			btnShowPreview.Size = new Size(104, 34);
			btnShowPreview.Text = "Preview";
			btnShowPreview.Click += btnShowPreview_Click;

			rtbGuide.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			rtbGuide.BackColor = SettingsPalette.Input;
			rtbGuide.BorderStyle = BorderStyle.FixedSingle;
			rtbGuide.Font = new Font("Segoe UI", 9.25F);
			rtbGuide.ForeColor = SettingsPalette.PrimaryText;
			rtbGuide.Location = new Point(612, 142);
			rtbGuide.ReadOnly = true;
			rtbGuide.Size = new Size(540, 520);
			rtbGuide.WordWrap = true;

			rtbPreview.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			rtbPreview.BackColor = SettingsPalette.Input;
			rtbPreview.BorderStyle = BorderStyle.FixedSingle;
			rtbPreview.Font = new Font("Cascadia Mono", 9F);
			rtbPreview.ForeColor = SettingsPalette.PrimaryText;
			rtbPreview.Location = new Point(612, 142);
			rtbPreview.ReadOnly = true;
			rtbPreview.Size = new Size(540, 520);
			rtbPreview.Visible = false;
			rtbPreview.WordWrap = false;

			lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			lblStatus.Font = new Font("Segoe UI", 9F);
			lblStatus.ForeColor = SettingsPalette.SecondaryText;
			lblStatus.Location = new Point(612, 672);
			lblStatus.Size = new Size(540, 46);
			lblStatus.Text = "Enter the game information, then validate before saving.";

			btnValidate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			btnValidate.Location = new Point(802, 736);
			btnValidate.Size = new Size(164, 44);
			btnValidate.Text = "Validate & Preview";
			btnValidate.Click += btnValidate_Click;

			btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			btnSave.Location = new Point(980, 736);
			btnSave.Size = new Size(172, 44);
			btnSave.Text = "Save to Project";
			btnSave.UseAccentStyle = true;
			btnSave.Click += btnSave_Click;

			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = SettingsPalette.Window;
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
			MinimumSize = new Size(1040, 720);
			Name = "GameDefinitionBuilder";
			StartPosition = FormStartPosition.CenterParent;
			Text = "Synix Game Definition Builder";

			pnlInputs.ResumeLayout(false);
			pnlInputs.PerformLayout();
			((System.ComponentModel.ISupportInitialize)dgvAdditionalTemplates).EndInit();
			ResumeLayout(false);
			PerformLayout();

			txtGame.Leave += txtGame_Leave;
			cmbConfigMode.SelectedIndexChanged += cmbConfigMode_SelectedIndexChanged;
			btnBrowseTemplate.Click += btnBrowseTemplate_Click;
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
