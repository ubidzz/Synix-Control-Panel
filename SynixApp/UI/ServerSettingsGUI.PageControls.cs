// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Design;

namespace Synix_Control_Panel
{
	public partial class ServerSettingsGUI
	{
		private TextBox txtName => pnlPageGeneral.txtName;
		private ModernSettingsComboBox cmbGame => pnlPageGeneral.cmbGame;
		private ModernSettingsComboBox cmbGameVersion => pnlPageGeneral.cmbGameVersion;
		private ModernSettingsCard cardMinecraftRuntime => pnlPageGeneral.cardMinecraftRuntime;
		private ModernSettingsCard cardCompatibility => pnlPageGeneral.cardCompatibility;
		private Label lblMinecraftRuntimeTitle => pnlPageGeneral.lblMinecraftRuntimeTitle;
		private Label lblMinecraftLoader => pnlPageGeneral.lblMinecraftLoader;
		private ModernSettingsComboBox cmbMinecraftLoader => pnlPageGeneral.cmbMinecraftLoader;
		private Label lblMinecraftLoaderVersion => pnlPageGeneral.lblMinecraftLoaderVersion;
		private ModernSettingsComboBox cmbMinecraftLoaderVersion => pnlPageGeneral.cmbMinecraftLoaderVersion;
		private Label lblMinecraftJava => pnlPageGeneral.lblMinecraftJava;
		private Label lblMinecraftJavaValue => pnlPageGeneral.lblMinecraftJavaValue;
		private Label lblMinecraftRuntimeHelper => pnlPageGeneral.lblMinecraftRuntimeHelper;
		private Label lblCrossplay => pnlPageGeneral.lblCrossplay;
		private ModernSettingsToggle chkCrossplay => pnlPageGeneral.chkCrossplay;
		private ModernSettingsComboBox cmbWorldName => pnlPageGeneral.cmbWorldName;
		private ModernSettingsComboBox cmbCompetitive => pnlPageGeneral.cmbCompetitive;
		private Label MaxPlayerLabel => pnlPageGeneral.MaxPlayerLabel;
		private ModernSettingsNumericUpDown numMaxPlayers => pnlPageGeneral.numMaxPlayers;
		private ModernSettingsNumericUpDown numRam => pnlPageGeneral.numRam;
		private Label lblInstallVerification => pnlPageGeneral.lblInstallVerification;
		private Label lblStartVerification => pnlPageGeneral.lblStartVerification;
		private Label lblStopVerification => pnlPageGeneral.lblStopVerification;
		private Label lblMonitoringVerification => pnlPageGeneral.lblMonitoringVerification;
		private Label lblLastTestedVersion => pnlPageGeneral.lblLastTestedVersion;

		private ModernSettingsCard cardCredentials => pnlPageSecurity.cardCredentials;
		private TextBox txtPassword => pnlPageSecurity.txtPassword;
		private TextBox txtAdminPassword => pnlPageSecurity.txtAdminPassword;

		private TextBox txtWorldSeed => pnlPageWorld.txtWorldSeed;
		private ModernSettingsNumericUpDown numWorldSize => pnlPageWorld.numWorldSize;

		private ModernSettingsNumericUpDown numPort => pnlPageNetwork.numPort;
		private Label QueryPortLabel => pnlPageNetwork.QueryPortLabel;
		private ModernSettingsNumericUpDown numQueryPort => pnlPageNetwork.numQueryPort;
		private ModernSettingsNumericUpDown numAppPort => pnlPageNetwork.numAppPort;
		private ModernSettingsCard cardRcon => pnlPageNetwork.cardRcon;
		private ModernSettingsToggle chkEnableRcon => pnlPageNetwork.chkEnableRcon;
		private ModernSettingsNumericUpDown numRconPort => pnlPageNetwork.numRconPort;
		private TextBox txtRconPassword => pnlPageNetwork.txtRconPassword;

		private ModernSettingsToggle chkUpdateOnStart => pnlPageAutomation.chkUpdateOnStart;
		private ModernSettingsToggle chkBackupOnStart => pnlPageAutomation.chkBackupOnStart;
		private ModernSettingsToggle chkEnableSchedule => pnlPageAutomation.chkEnableSchedule;
		private ModernSettingsButton btnEditSchedule => pnlPageAutomation.btnEditSchedule;

		private ModernSettingsToggle chkDefaultPath => pnlPageInstall.chkDefaultPath;
		private TextBox txtInstallPath => pnlPageInstall.txtInstallPath;
		private ModernSettingsButton btnBrowse => pnlPageInstall.btnBrowse;
		private ModernSettingsCard cardLaunchArguments => pnlPageInstall.cardLaunchArguments;
		private ModernSettingsButton btnViewArgs => pnlPageInstall.btnViewArgs;
		private TextBox txtExtraArgs => pnlPageInstall.txtExtraArgs;
	}
}
