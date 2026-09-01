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
using Synix_Control_Panel.Help;
using Synix_Control_Panel.ServerHandler;
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Database.GameConfigurations;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine;
using System.ComponentModel;
using System.Runtime.InteropServices;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel
{
	public partial class ServerSettingsGUI : Form
	{
		public GameServer? NewServer { get; private set; }
		private bool isPrivacyLoading = false;
		private bool _isEditMode = false;
		private GameServer? _existingServer = null;
		private string _oldPath = string.Empty;
		private bool[] _selectedDays = new bool[7] { false, false, false, false, false, false, false };
		private string _selectedTime = "04:00";
		private bool _smartMaintenanceEnabled = true;
		private bool _maintenanceWaitForPlayers = true;
		private int _maintenanceMaximumDelayMinutes = 30;
		private bool _maintenanceBackupBeforeRestart = true;
		private bool _maintenanceUpdateBeforeRestart;
		private System.Windows.Forms.Timer? debounceTimer;
		private bool _PrivacyMode = false;
		private bool _isApplyingPortOffset = false;
		private bool _suppressMinecraftMetadataEvents = false;
		private bool _isLoadingMinecraftMetadata = false;
		private int _minecraftMetadataRequestId = 0;
		private int _resolvedMinecraftJavaVersion = 0;
		private string _minecraftMetadataError = string.Empty;
		private bool _passwordUnlockFailed;
		private string _validationMessage =
			"  🔒 [REQUIRED] Enter a Server Name and select a Game Template.";
		private bool _advancedMode;
		private ModernSettingsButton? _experienceModeButton;
		private Label? _completionLabel;
		private Panel? _completionTrack;
		private Panel? _completionFill;
		private Label? _minecraftEditionLabel;
		private ModernSettingsComboBox? _minecraftEditionCombo;

		private const int WmNcLeftButtonDown = 0x00A1;
		private const int HtCaption = 0x0002;
		private const int DwmWindowCornerPreference = 33;
		private const int DwmRound = 2;

		[DllImport("user32.dll")]
		private static extern bool ReleaseCapture();

		[DllImport("user32.dll", EntryPoint = "SendMessageW")]
		private static extern IntPtr SendWindowMessage(
			IntPtr windowHandle,
			int message,
			IntPtr parameter,
			IntPtr additionalParameter);

		[DllImport("dwmapi.dll")]
		private static extern int DwmSetWindowAttribute(
			IntPtr windowHandle,
			int attribute,
			ref int value,
			int valueSize);

		public ServerSettingsGUI(GameServer? server = null)
		{
			InitializeComponent();
			isPrivacyLoading = true;
			_existingServer = server;
			_isEditMode = server != null;
			_PrivacyMode = Properties.Settings.Default.PrivacyMode;
			ConfigureModernShell();

			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
				DesignMode ||
				Site?.DesignMode == true)
			{
				isPrivacyLoading = false;
				return;
			}
			ThemeManager.Apply(this);

			WireUpGatekeeperEvents();

			chkDefaultPath.Tag = "Default Folder";
			chkEnableSchedule.Tag = "Activate Scheduler";
			chkUpdateOnStart.Tag = "Update on Start";
			chkEnableRcon.Tag = "RCON";
			chkCrossplay.Tag = "Crossplay";
			chkBackupOnStart.Tag = "Backup on Start";
			discordSettingsPage.SettingsChanged += (_, _) => SyncGatekeeper();

			cmbGame.Items.Clear();
			cmbGame.Items.Add("-- Pick a Game --");
			var sortedGames = GameDatabase.GetGameList().OrderBy(g => g.Game).ToList();
			foreach (var game in sortedGames) cmbGame.Items.Add(game.Game);

			if (_isEditMode && _existingServer != null)
			{
				_oldPath = _existingServer.InstallPath;
				LoadExistingServerData();
			}
			else
			{
				cmbGame.SelectedIndex = 0;
				ToggleGameSpecificFields(null);
			}
			RefreshCompatibilityVerification(_existingServer?.Game);

			isPrivacyLoading = false;
			PrivacyMode();
			SyncGatekeeper();

			if (_isEditMode && _existingServer != null)
			{
				Shown += async (_, _) => await InitializeExistingMinecraftSelectionAsync();
			}
		}

		private void ConfigureModernShell()
		{
			lblModeBadge.Text = _isEditMode ? "EDIT SERVER" : "NEW SERVER";
			btnSave.Text = _isEditMode ? "Save Changes" : "Save Server";
			Text = _isEditMode ? "Edit Server" : "Server Setup";
			txtInstallPath.ReadOnly = true;
			txtInstallPath.TabStop = false;
			txtInstallPath.ShortcutsEnabled = false;
			txtInstallPath.Cursor = Cursors.Default;
			InitializeGuidanceControls();
			InitializeMinecraftEditionControls();
			ShowSettingsPage(
				pnlPageGeneral,
				btnNavGeneral,
				"General",
				"Choose the game and define the server identity.");
			UpdateModernStatus();
		}

		private void InitializeMinecraftEditionControls()
		{
			_minecraftEditionLabel = new Label
			{
				Name = "lblMinecraftEdition",
				AutoSize = true,
				BackColor = SettingsPalette.Card,
				ForeColor = SettingsPalette.PrimaryText,
				Font = new Font("Segoe UI", 9F, FontStyle.Bold),
				Location = new Point(24, 52),
				Text = "Edition"
			};
			_minecraftEditionCombo = new ModernSettingsComboBox
			{
				Name = "cmbMinecraftEdition",
				BackColor = Color.FromArgb(12, 21, 36),
				ForeColor = SettingsPalette.PrimaryText,
				Font = new Font("Segoe UI", 9.5F),
				DrawMode = DrawMode.OwnerDrawFixed,
				DropDownStyle = ComboBoxStyle.DropDownList,
				FlatStyle = FlatStyle.Flat,
				ItemHeight = 28,
				Location = new Point(24, 72),
				Size = new Size(260, 34)
			};
			_minecraftEditionCombo.Items.AddRange([
				MinecraftControlProfile.JavaEdition,
				MinecraftControlProfile.BedrockEdition
			]);
			_minecraftEditionCombo.SelectedItem = MinecraftControlProfile.JavaEdition;

			foreach (Control control in new Control[]
			{
				lblMinecraftLoader,
				cmbMinecraftLoader,
				lblMinecraftLoaderVersion,
				cmbMinecraftLoaderVersion,
				lblMinecraftJava,
				lblMinecraftJavaValue,
				lblMinecraftRuntimeHelper
			})
			{
				control.Top += 54;
			}

			cardMinecraftRuntime.Height += 54;
			cardMinecraftRuntime.Controls.Add(_minecraftEditionLabel);
			cardMinecraftRuntime.Controls.Add(_minecraftEditionCombo);
		}

		private void InitializeGuidanceControls()
		{
			_advancedMode = Properties.Settings.Default.AdvancedServerSetupMode;
			_experienceModeButton = new ModernSettingsButton
			{
				Name = "btnExperienceMode",
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				Location = new Point(628, 25),
				Size = new Size(168, 34),
				Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
			};
			_experienceModeButton.Click += (_, _) =>
			{
				_advancedMode = !_advancedMode;
				Properties.Settings.Default.AdvancedServerSetupMode = _advancedMode;
				try
				{
					Properties.Settings.Default.Save();
				}
				catch (Exception exception)
				{
					PlainEnglishErrorDialog.ShowError(
						this,
						"save the setup mode",
						exception.Message);
				}
				ApplyExperienceMode();
			};
			pnlContent.Controls.Add(_experienceModeButton);
			_experienceModeButton.BringToFront();

			pnlSidebarStatus.Height = 176;
			lblSidebarStatusDetail.Height = 32;
			_completionLabel = new Label
			{
				Name = "lblSetupCompletion",
				Text = "Setup completion: 0%",
				Location = new Point(22, 123),
				Size = new Size(166, 20),
				Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
				ForeColor = SettingsPalette.SecondaryText
			};
			_completionTrack = new Panel
			{
				Name = "pnlSetupCompletionTrack",
				Location = new Point(22, 151),
				Size = new Size(166, 7),
				BackColor = SettingsPalette.Divider
			};
			_completionFill = new Panel
			{
				Name = "pnlSetupCompletionFill",
				Location = Point.Empty,
				Size = new Size(0, 7),
				BackColor = SettingsPalette.Accent
			};
			_completionTrack.Controls.Add(_completionFill);
			pnlSidebarStatus.Controls.Add(_completionLabel);
			pnlSidebarStatus.Controls.Add(_completionTrack);
			ApplyExperienceMode();
		}

		private void ApplyExperienceMode()
		{
			if (_experienceModeButton == null)
				return;

			_experienceModeButton.Text = _advancedMode
				? "Mode: Advanced"
				: "Mode: Beginner";
			_experienceModeButton.UseAccentStyle = !_advancedMode;
			_experienceModeButton.AccessibleName = _advancedMode
				? "Advanced server setup mode. Click to use Beginner mode."
				: "Beginner server setup mode. Click to show advanced settings.";
			cardRcon.Visible = _advancedMode;
			cardLaunchArguments.Visible = _advancedMode;

			if (pnlPageNetwork.Visible)
			{
				lblPageDescription.Text = _advancedMode
					? "Assign service ports and secure remote administration."
					: "Use the recommended game and query ports. Advanced mode adds RCON controls.";
			}
			if (pnlPageInstall.Visible)
			{
				lblPageDescription.Text = _advancedMode
					? "Choose server storage and customize launch arguments."
					: "Choose where the server will be installed. Synix supplies the recommended launch settings.";
			}
		}

		private void ShowSettingsPage(
			Panel page,
			ModernSettingsNavButton navigationButton,
			string title,
			string description)
		{
			Panel[] pages =
			{
				pnlPageGeneral,
				pnlPageWorld,
				pnlPageNetwork,
				pnlPageAutomation,
				pnlPageDiscord,
				pnlPageInstall
			};
			ModernSettingsNavButton[] navigationButtons =
			{
				btnNavGeneral,
				btnNavWorld,
				btnNavNetwork,
				btnNavAutomation,
				btnNavDiscord,
				btnNavInstall
			};

			foreach (Panel candidate in pages)
			{
				candidate.Visible = ReferenceEquals(candidate, page);
			}

			foreach (ModernSettingsNavButton candidate in navigationButtons)
			{
				candidate.Selected = ReferenceEquals(candidate, navigationButton);
			}

			lblPageTitle.Text = title;
			lblPageDescription.Text = description;
			page.BringToFront();
		}

		private void UpdateModernStatus()
		{
			bool ready = btnSave.Enabled;
			string validationMessage = string.IsNullOrWhiteSpace(_validationMessage)
				? "Validation is waiting for the required server information."
				: _validationMessage.Trim();

			lblSidebarStatus.Text = ready ? "●  Ready to save" : "●  Action required";
			lblSidebarStatus.ForeColor = ready
				? SettingsPalette.Accent
				: SettingsPalette.Warning;
			lblSidebarStatusDetail.Text = ready
				? "All required checks passed"
				: "See the exact validation message below";
			lblFooterStatus.Text = validationMessage;
			lblFooterStatus.ForeColor = ready
				? SettingsPalette.Accent
				: SettingsPalette.Warning;

			bool hasGame = cmbGame.SelectedIndex > 0;
			bool blockedByPort = validationMessage.Contains("[CONFLICT]", StringComparison.OrdinalIgnoreCase) &&
				validationMessage.Contains("Port", StringComparison.OrdinalIgnoreCase);
			bool requirementsMet = !validationMessage.Contains("[REQUIREMENT]", StringComparison.OrdinalIgnoreCase) &&
				!validationMessage.Contains("[MINECRAFT]", StringComparison.OrdinalIgnoreCase) &&
				!validationMessage.Contains("[VALIDATION ERROR]", StringComparison.OrdinalIgnoreCase);
			int completion = UserGuidance.CalculateSetupCompletion(new SetupCompletionState(
				!string.IsNullOrWhiteSpace(txtName.Text),
				hasGame,
				!string.IsNullOrWhiteSpace(txtInstallPath.Text),
				hasGame && !blockedByPort,
				hasGame && requirementsMet,
				ready));
			if (_completionLabel != null)
			{
				_completionLabel.Text = $"Setup completion: {completion}%";
				_completionLabel.ForeColor = completion == 100
					? SettingsPalette.Success
					: SettingsPalette.SecondaryText;
			}
			if (_completionTrack != null && _completionFill != null)
			{
				_completionFill.Width = (int)Math.Round(
					_completionTrack.ClientSize.Width * completion / 100d);
				_completionFill.BackColor = completion == 100
					? SettingsPalette.Success
					: SettingsPalette.Accent;
			}
		}

		private void btnNavGeneral_Click(object? sender, EventArgs eventArgs)
		{
			ShowSettingsPage(
				pnlPageGeneral,
				btnNavGeneral,
				"General",
				"Choose the game and define the server identity.");
		}

		private void btnNavWorld_Click(object? sender, EventArgs eventArgs)
		{
			ShowSettingsPage(
				pnlPageWorld,
				btnNavWorld,
				"World Generation",
				"Configure world seed, size, and game-specific world options.");
		}

		private void btnNavNetwork_Click(object? sender, EventArgs eventArgs)
		{
			ShowSettingsPage(
				pnlPageNetwork,
				btnNavNetwork,
				"Network & RCON",
				"Assign service ports and secure remote administration.");
		}

		private void btnNavAutomation_Click(object? sender, EventArgs eventArgs)
		{
			ShowSettingsPage(
				pnlPageAutomation,
				btnNavAutomation,
				"Automation",
				"Control startup tasks, scheduled restarts, backups, and alerts.");
		}

		private void btnNavInstall_Click(object? sender, EventArgs eventArgs)
		{
			ShowSettingsPage(
				pnlPageInstall,
				btnNavInstall,
				"Install & Launch",
				"Choose server storage and customize launch arguments.");
		}

		private void TitleBar_MouseDown(object? sender, MouseEventArgs eventArgs)
		{
			if (eventArgs.Button != MouseButtons.Left)
			{
				return;
			}

			_ = ReleaseCapture();
			_ = SendWindowMessage(
				Handle,
				WmNcLeftButtonDown,
				new IntPtr(HtCaption),
				IntPtr.Zero);
		}

		private void btnTitleMinimize_Click(object? sender, EventArgs eventArgs)
		{
			WindowState = FormWindowState.Minimized;
		}

		private void btnTitleClose_Click(object? sender, EventArgs eventArgs)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		protected override void OnHandleCreated(EventArgs eventArgs)
		{
			base.OnHandleCreated(eventArgs);

			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
				DesignMode ||
				Site?.DesignMode == true)
			{
				return;
			}

			try
			{
				int preference = DwmRound;
				_ = DwmSetWindowAttribute(
					Handle,
					DwmWindowCornerPreference,
					ref preference,
					sizeof(int));
			}
			catch
			{

			}
		}

		protected override void OnFormClosed(FormClosedEventArgs eventArgs)
		{
			txtPassword.Clear();
			txtAdminPassword.Clear();
			txtRconPassword.Clear();
			discordSettingsPage.ClearSecrets();
			debounceTimer?.Stop();
			debounceTimer?.Dispose();
			base.OnFormClosed(eventArgs);
		}

		private void PrivacyMode()
		{
			if (_PrivacyMode)
			{
				txtPassword.UseSystemPasswordChar = true;
				txtAdminPassword.UseSystemPasswordChar = true;
				txtRconPassword.UseSystemPasswordChar = true;
				discordSettingsPage.SetPrivacyMode(true);
			}
			else
			{
				txtPassword.UseSystemPasswordChar = false;
				txtAdminPassword.UseSystemPasswordChar = false;
				txtRconPassword.UseSystemPasswordChar = false;
				discordSettingsPage.SetPrivacyMode(false);
			}
		}

		private void LoadExistingServerData()
		{
			if (_existingServer == null) return;
			isPrivacyLoading = true;

			txtName.Text = _existingServer.ServerName ?? "";
			int gameIndex = cmbGame.FindStringExact(_existingServer.Game);
			if (gameIndex != -1) cmbGame.SelectedIndex = gameIndex;
			GameInfo? gameData = GameDatabase.GetGame(_existingServer.Game);
			if (_minecraftEditionCombo != null && GameDatabase.IsMinecraft(_existingServer.Game))
			{
				SelectComboBoxValue(
					_minecraftEditionCombo,
					MinecraftControlProfile.NormalizeEdition(_existingServer.MinecraftEdition),
					MinecraftControlProfile.JavaEdition);
			}

			if (Core.TryRevealServerSecrets(
					_existingServer,
					out SynixServerSecrets secrets) &&
				Core.TryRevealDiscordWebhookRoutes(
					_existingServer,
					out IReadOnlyList<DiscordWebhookRoute> discordRoutes))
			{
				SynixServerPasswords passwords = secrets.Passwords;
				txtPassword.Text = passwords.ServerPassword;
				txtAdminPassword.Text = passwords.AdminPassword;
				txtRconPassword.Text = passwords.RconPassword;
				discordSettingsPage.LoadSettings(
					_existingServer.IsDiscordAlertEnabled,
					secrets.DiscordWebhook,
					_existingServer.DiscordEvents,
					discordRoutes);
			}
			else
			{
				_passwordUnlockFailed = true;
				txtPassword.Clear();
				txtAdminPassword.Clear();
				txtRconPassword.Clear();
				discordSettingsPage.LoadSettings(
					false,
					string.Empty,
					DiscordNotificationEvent.All,
					[]);
				Shown += ShowPasswordUnlockWarning;
			}
			discordSettingsPage.SetServerName(_existingServer.ServerName);

			numPort.Value = Math.Clamp(_existingServer.Port, numPort.Minimum, numPort.Maximum);
			int queryPortToLoad = _existingServer.QueryPort > 0
				? _existingServer.QueryPort
				: gameData?.QueryPort ?? (int)numQueryPort.Minimum;
			numQueryPort.Value = Math.Clamp(queryPortToLoad, numQueryPort.Minimum, numQueryPort.Maximum);
			if (numAppPort != null) numAppPort.Value = Math.Clamp(_existingServer.AppPort ?? numAppPort.Minimum, numAppPort.Minimum, numAppPort.Maximum);

			numMaxPlayers.Value = Math.Clamp(_existingServer.MaxPlayers, numMaxPlayers.Minimum, numMaxPlayers.Maximum);
			txtInstallPath.Text = _existingServer.InstallPath ?? "";
			chkDefaultPath.Checked = _existingServer.IsDefaultPath;
			txtExtraArgs.Text = _existingServer.ExtraArgs ?? "";
			txtWorldSeed.Text = _existingServer.WorldSeed ?? "12345";
			ConfigureWorldSizeInput(gameData);
			int worldSizeToLoad = IsSevenDaysToDie(gameData)
				? SevenDaysToDieConfiguration.NormalizeWorldSize(_existingServer.WorldSize)
				: _existingServer.WorldSize;
			numWorldSize.Value = Math.Clamp(worldSizeToLoad, numWorldSize.Minimum, numWorldSize.Maximum);

			chkUpdateOnStart.Checked = _existingServer.UpdateOnStart;
			chkEnableRcon.Checked = _existingServer.EnableRcon;
			chkCrossplay.Checked = _existingServer.CrossplayEnabled;
			numRconPort.Value = Math.Clamp(_existingServer.RconPort, numRconPort.Minimum, numRconPort.Maximum);
			chkEnableSchedule.Checked = _existingServer.IsScheduledRestartEnabled;
			if (_existingServer.RestartDays != null) _selectedDays = (bool[])_existingServer.RestartDays.Clone();
			_selectedTime = _existingServer.RestartTime ?? "04:00";
			_smartMaintenanceEnabled = _existingServer.SmartMaintenanceEnabled;
			_maintenanceWaitForPlayers = _existingServer.MaintenanceWaitForPlayers;
			_maintenanceMaximumDelayMinutes = _existingServer.MaintenanceMaximumDelayMinutes;
			_maintenanceBackupBeforeRestart = _existingServer.MaintenanceBackupBeforeRestart;
			_maintenanceUpdateBeforeRestart = _existingServer.MaintenanceUpdateBeforeRestart;
			chkBackupOnStart.Checked = _existingServer.BackupOnStart;
			cmbGameVersion.Text = _existingServer.GameVersion ?? "latest";
			numRam.Value = Math.Clamp(_existingServer.MaxRam, numRam.Minimum, numRam.Maximum);

			if (gameData != null)
			{
				string worldNameToLoad = IsSevenDaysToDie(gameData)
					? SevenDaysToDieConfiguration.NormalizeWorldName(_existingServer.WorldName)
					: _existingServer.WorldName ?? "";
				PopulateMaps(gameData, worldNameToLoad);
				PopulateGameModes(gameData, _existingServer.GameMode ?? "PVE");
				ToggleGameSpecificFields(gameData);
			}

			cmbGame.Enabled = false;
			isPrivacyLoading = false;
		}

		private void ShowPasswordUnlockWarning(object? sender, EventArgs eventArgs)
		{
			Shown -= ShowPasswordUnlockWarning;
			if (!_passwordUnlockFailed)
				return;

			MessageBox.Show(
				"Synix could not unlock this server's saved passwords or Discord webhooks. They may have come from another Windows user or computer.\n\nEnter the credentials again and press Save Changes to protect them for this Windows user.",
				"Re-enter Server Credentials",
				MessageBoxButtons.OK,
				MessageBoxIcon.Warning);
		}

		private void SyncGatekeeper()
		{
			if (isPrivacyLoading) return;

			try
			{
				string currentName = txtName?.Text?.Trim() ?? "";
				bool hasName = !string.IsNullOrWhiteSpace(currentName);
				bool hasGame = cmbGame?.SelectedIndex > 0;
				string selectedGame = hasGame ? cmbGame?.Text ?? string.Empty : string.Empty;
				bool isBaseReady = hasName && hasGame;
				GameInfo? selectedDefinition = hasGame
					? GameDatabase.GetGame(selectedGame)
					: null;
				bool isMinecraft = selectedGame.Equals("Minecraft", StringComparison.OrdinalIgnoreCase);
				bool isMinecraftBedrock = isMinecraft && IsMinecraftBedrockSelected();
				bool supportsServerFramework =
					selectedDefinition?.SupportedServerFrameworks.Count > 0;
				bool CanUnlock(Control c) => hasGame && c.Tag?.ToString() == "Required";

				GamePrerequisiteItem? missingRequirement = selectedDefinition == null
					? null
					: GamePrerequisiteChecker
						.CheckCurrentSystem(selectedDefinition)
						.FirstFailure;

				txtPassword.Enabled = CanUnlock(txtPassword);
				txtAdminPassword.Enabled = CanUnlock(txtAdminPassword);
				txtWorldSeed.Enabled = CanUnlock(txtWorldSeed);
				cmbCompetitive.Enabled = CanUnlock(cmbCompetitive);
				numMaxPlayers.Enabled = CanUnlock(numMaxPlayers);
				numQueryPort.Enabled = CanUnlock(numQueryPort);
				cmbWorldName.Enabled = CanUnlock(cmbWorldName);
				numWorldSize.Enabled = CanUnlock(numWorldSize);
				cmbGameVersion.Enabled = CanUnlock(cmbGameVersion) && !_isLoadingMinecraftMetadata;
				cmbMinecraftLoader.Enabled =
					(isMinecraft || supportsServerFramework) &&
					!isMinecraftBedrock &&
					!_isLoadingMinecraftMetadata;
				cmbMinecraftLoaderVersion.Enabled = isMinecraft &&
					!isMinecraftBedrock &&
					!_isLoadingMinecraftMetadata &&
					!MinecraftMetadataService.NormalizeLoader(cmbMinecraftLoader.Text)
						.Equals(MinecraftMetadataService.VanillaLoader, StringComparison.OrdinalIgnoreCase);
				numRam.Enabled = CanUnlock(numRam);
				numPort.Enabled = CanUnlock(numPort);
				numAppPort.Enabled = CanUnlock(numAppPort);

				if (numAppPort != null)
					numAppPort.Tag = CanUnlock(numAppPort) ? "Required" : "Disabled";

				chkEnableRcon.Enabled = CanUnlock(chkEnableRcon);
				chkCrossplay.Enabled = CanUnlock(chkCrossplay);
				bool rconActive = chkEnableRcon.Enabled && chkEnableRcon.Checked;
				numRconPort.Enabled = rconActive;
				txtRconPassword.Enabled = rconActive;

				chkUpdateOnStart.Enabled = isBaseReady;
				chkBackupOnStart.Enabled = isBaseReady;
				chkEnableSchedule.Enabled = isBaseReady;
				if (btnEditSchedule != null) btnEditSchedule.Enabled = isBaseReady && chkEnableSchedule.Checked;

				discordSettingsPage.SetServerName(currentName);
				discordSettingsPage.SetEditingEnabled(isBaseReady);

				if (_isEditMode)
				{
					chkDefaultPath.Enabled = false;
					btnBrowse.Enabled = false;
				}
				else
				{
					chkDefaultPath.Enabled = isBaseReady;
					bool manualMode = isBaseReady && !chkDefaultPath.Checked;
					btnBrowse.Enabled = manualMode;
				}
				txtInstallPath.Enabled = true;

				if (!_isEditMode && isBaseReady && chkDefaultPath.Checked)
				{
					string safeName = Core.Instance.GetSafeName(currentName);
					string safeFolderName = Core.Instance.GetSafeName(selectedGame);
					string safeFolderPath = Path.Combine(safeFolderName, safeName);
					txtInstallPath.Text = Path.Combine(Core.GamesPath, safeFolderPath);
				}

				int gPort = (int)numPort.Value;
				int qPort = (int)numQueryPort.Value;
				bool checkGamePort = numPort.Enabled;
				bool checkQueryPort = numQueryPort.Enabled;
				int aPort = (numAppPort != null && numAppPort.Enabled) ? (int)numAppPort.Value : 0;
				int rPort = rconActive ? (int)numRconPort.Value : 0;
				var selectedPorts = new List<(int Port, string Name)>();
				if (checkGamePort) selectedPorts.Add((gPort, "Game Port"));
				if (checkQueryPort) selectedPorts.Add((qPort, "Query Port"));
				if (rPort > 0) selectedPorts.Add((rPort, "RCON Port"));
				if (aPort > 0) selectedPorts.Add((aPort, "App Port"));
				var duplicateSelection = selectedPorts
					.GroupBy(port => port.Port)
					.FirstOrDefault(group => group.Count() > 1);

				string? gOwner = checkGamePort
					? Core.Instance.GetConfiguredPortCollisionOwner(gPort, _existingServer)
					: null;
				bool gOS = checkGamePort && Core.Instance.IsPortInUseLocally(gPort);

				string? qOwner = (checkQueryPort && qPort > 0)
					? Core.Instance.GetConfiguredPortCollisionOwner(qPort, _existingServer)
					: null;
				bool qOS = checkQueryPort && qPort > 0 && Core.Instance.IsPortInUseLocally(qPort);

				string? rOwner = (rPort > 0) ? Core.Instance.GetConfiguredPortCollisionOwner(rPort, _existingServer) : null;
				bool rOS = (rPort > 0) && Core.Instance.IsPortInUseLocally(rPort);

				string? aOwner = (aPort > 0) ? Core.Instance.GetConfiguredPortCollisionOwner(aPort, _existingServer) : null;
				bool aOS = (aPort > 0) && Core.Instance.IsPortInUseLocally(aPort);

				bool isNameTaken = MainGUI.serverList.Any(s =>
					s != _existingServer &&
					s.Game.Equals(selectedGame, StringComparison.OrdinalIgnoreCase) &&
					s.ServerName.Equals(currentName, StringComparison.OrdinalIgnoreCase));

				if (!isBaseReady)
				{
					if (!hasName && !hasGame)
					{
						_validationMessage = "  🔒 [REQUIRED] Enter a Server Name and select a Game Template.";
					}
					else if (!hasName)
					{
						_validationMessage = "  🔒 [REQUIRED] Enter a Server Name before this server can be saved.";
					}
					else
					{
						_validationMessage = "  🔒 [REQUIRED] Select a Game Template before this server can be saved.";
					}

					btnSave.Enabled = false;
				}
				else if (isMinecraft && !isMinecraftBedrock && _isLoadingMinecraftMetadata)
				{
					_validationMessage = "  ◌ [MINECRAFT] Loading compatible versions and Java requirements...";
					btnSave.Enabled = false;
				}
				else if (isMinecraft && !isMinecraftBedrock && !string.IsNullOrWhiteSpace(_minecraftMetadataError))
				{
					_validationMessage = $"  ⚠️ [MINECRAFT] {_minecraftMetadataError}";
					btnSave.Enabled = false;
				}
				else if (isMinecraft && string.IsNullOrWhiteSpace(cmbGameVersion.Text))
				{
					_validationMessage = "  🔒 [MINECRAFT] Select a Minecraft game version.";
					btnSave.Enabled = false;
				}
				else if (isMinecraft && !isMinecraftBedrock &&
					!MinecraftMetadataService.NormalizeLoader(cmbMinecraftLoader.Text)
						.Equals(MinecraftMetadataService.VanillaLoader, StringComparison.OrdinalIgnoreCase) &&
					string.IsNullOrWhiteSpace(cmbMinecraftLoaderVersion.Text))
				{
					_validationMessage = "  🔒 [MINECRAFT] No compatible loader build is selected.";
					btnSave.Enabled = false;
				}

				else if (missingRequirement != null)
				{
					_validationMessage = $"  ⚠️ [REQUIREMENT] {missingRequirement.Message}";
					btnSave.Enabled = false;
				}

				else if (isNameTaken)
				{
					_validationMessage = $"  ⚠️ [CONFLICT] Name '{currentName}' is already used for {selectedGame}.";
					btnSave.Enabled = false;
				}
				else if (duplicateSelection != null)
				{
					string roles = string.Join(
						" and ",
						duplicateSelection.Select(port => port.Name));
					_validationMessage =
						$"  ⚠️ [CONFLICT] {roles} cannot both use port {duplicateSelection.Key}.";
					btnSave.Enabled = false;
				}
				else if (gOwner != null || gOS)
				{
					_validationMessage = $"  ⚠️ [CONFLICT] Game Port {gPort} is blocked by: {gOwner ?? "System Process"}";
					btnSave.Enabled = false;
				}
				else if (qOwner != null || qOS)
				{
					_validationMessage = $"  ⚠️ [CONFLICT] Query Port {qPort} is blocked by: {qOwner ?? "System Process"}";
					btnSave.Enabled = false;
				}
				else if (rOwner != null || rOS)
				{
					_validationMessage = $"  ⚠️ [CONFLICT] RCON Port {rPort} is blocked by: {rOwner ?? "System Process"}";
					btnSave.Enabled = false;
				}
				else if (aOwner != null || aOS)
				{
					_validationMessage = $"  ⚠️ [CONFLICT] App Port {aPort} is blocked by: {aOwner ?? "System Process"}";
					btnSave.Enabled = false;
				}
				else if (string.IsNullOrWhiteSpace(txtInstallPath.Text))
				{
					_validationMessage = "  🔒 [REQUIRED] Select an install folder or enable the default install path.";
					btnSave.Enabled = false;
				}
				else
				{
					if (!string.IsNullOrWhiteSpace(
						selectedDefinition?.LaunchBehavior.ReadyMessage))
					{
						_validationMessage =
							$"  ✔ [READY] NOTE: {selectedDefinition.LaunchBehavior.ReadyMessage}";
					}
					else
					{
						_validationMessage = _isEditMode ? $"  ✔ [READY] Updating: {currentName}" : "  ✔ [READY] Configuration is valid and safe.";
					}

					btnSave.Enabled = true;
				}

				UpdateModernStatus();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[GATEKEEPER CRASH] {ex.Message}");
				_validationMessage = $"  ⚠️ [VALIDATION ERROR] Validation could not complete: {ex.Message}";
				btnSave.Enabled = false;
				UpdateModernStatus();
			}
		}

		private void ToggleGameSpecificFields(GameInfo? gameData)
		{
			var controls = new Control[] { txtPassword, txtAdminPassword, txtWorldSeed, cmbCompetitive, numAppPort, numMaxPlayers, numQueryPort, cmbWorldName, chkEnableRcon, chkCrossplay };
			if (gameData == null)
			{
				ConfigureRuntimeCard(null);
				foreach (var c in controls) if (c != null) c.Tag = "Disabled";
				lblCrossplay.Visible = false;
				chkCrossplay.Visible = false;

				SetupManagedPlaceholder(txtPassword, "Select a game...");
				SetupManagedPlaceholder(txtAdminPassword, "Select a game...");
				SetupManagedPlaceholder(txtWorldSeed, "Select a game...");
			}
			else
			{
				ConfigureRuntimeCard(gameData);
				bool isMinecraft = GameDatabase.IsMinecraft(gameData.Game);
				GameManagementCapability capabilities =
					GameFix.GetManagementCapabilities(gameData);
				if (isMinecraft && IsMinecraftBedrockSelected())
				{
					capabilities =
						GameManagementCapability.WorldSeed |
						GameManagementCapability.GameMode |
						GameManagementCapability.MaxPlayers |
						GameManagementCapability.QueryPort |
						GameManagementCapability.WorldName |
						GameManagementCapability.Port |
						GameManagementCapability.GameVersion;
				}
				bool Supports(GameManagementCapability capability) =>
					(capabilities & capability) != GameManagementCapability.None;

				bool needsPass = Supports(GameManagementCapability.ServerPassword);
				txtPassword.Tag = needsPass ? "Required" : "Disabled";
				if (!needsPass)
				{
					SetupManagedPlaceholder(txtPassword, "Not Required");
				}
				else
				{
					if (txtPassword.Text == "Select a game..." || txtPassword.Text == "Not Required" || txtPassword.ForeColor == Color.Gray)
					{
						txtPassword.Text = "";
					}
					txtPassword.ForeColor = SettingsPalette.PrimaryText;
					txtPassword.GotFocus -= Placeholder_GotFocus;
					txtPassword.LostFocus -= Placeholder_LostFocus;
				}

				bool needsAdminPass = Supports(GameManagementCapability.AdminPassword);
				txtAdminPassword.Tag = needsAdminPass ? "Required" : "Disabled";
				if (!needsAdminPass)
				{
					SetupManagedPlaceholder(txtAdminPassword, "Not Required");
				}
				else
				{
					if (txtAdminPassword.Text == "Select a game..." || txtAdminPassword.Text == "Not Required" || txtAdminPassword.ForeColor == Color.Gray)
					{
						txtAdminPassword.Text = "";
					}
					txtAdminPassword.ForeColor = SettingsPalette.PrimaryText;
					txtAdminPassword.GotFocus -= Placeholder_GotFocus;
					txtAdminPassword.LostFocus -= Placeholder_LostFocus;
				}

				bool needsSeed = Supports(GameManagementCapability.WorldSeed);
				txtWorldSeed.Tag = needsSeed ? "Required" : "Disabled";
				if (!needsSeed)
				{
					SetupManagedPlaceholder(txtWorldSeed, "Not Required");
				}
				else
				{
					if (txtWorldSeed.Text == "Select a game..." || txtWorldSeed.Text == "Not Required" || txtWorldSeed.ForeColor == Color.Gray)
					{
						txtWorldSeed.Text = "";
					}
					txtWorldSeed.ForeColor = SettingsPalette.PrimaryText;
					txtWorldSeed.GotFocus -= Placeholder_GotFocus;
					txtWorldSeed.LostFocus -= Placeholder_LostFocus;
				}

				cmbCompetitive.Tag = Supports(GameManagementCapability.GameMode)
					? "Required"
					: "Disabled";
				numMaxPlayers.Tag = Supports(GameManagementCapability.MaxPlayers)
					? "Required"
					: "Disabled";
				bool usesQueryPort = Supports(GameManagementCapability.QueryPort);
				QueryPortLabel.Text = isMinecraft && IsMinecraftBedrockSelected()
					? "IPv6 Port"
					: "Query Port";
				numQueryPort.Tag = usesQueryPort ? "Required" : "Disabled";
				cmbWorldName.Tag = Supports(GameManagementCapability.WorldName)
					? "Required"
					: "Disabled";
				chkEnableRcon.Tag = Supports(GameManagementCapability.Rcon)
					? "Required"
					: "Disabled";
				bool supportsCrossplay = Supports(GameManagementCapability.Crossplay);
				chkCrossplay.Tag = supportsCrossplay ? "Required" : "Disabled";
				lblCrossplay.Visible = supportsCrossplay;
				chkCrossplay.Visible = supportsCrossplay;
				numWorldSize.Tag = Supports(GameManagementCapability.WorldSize)
					? "Required"
					: "Disabled";
				cmbGameVersion.Tag = Supports(GameManagementCapability.GameVersion)
					? "Required"
					: "Disabled";
				numRam.Tag = Supports(GameManagementCapability.Ram)
					? "Required"
					: "Disabled";
				numPort.Tag = Supports(GameManagementCapability.Port)
					? "Required"
					: "Disabled";
				numAppPort.Tag = Supports(GameManagementCapability.AppPort)
					? "Required"
					: "Disabled";

			}
			ConfigurationSupportPresentation support =
				UserGuidance.GetConfigurationSupport(gameData);
			lblTemplateBehavior.Text =
				$"◇  CONFIGURATION SUPPORT: {support.Status}  •  Unsupported settings are disabled automatically.";
			lblTemplateBehavior.ForeColor = support.Color;
			ApplyExperienceMode();
			SyncGatekeeper();
		}

		private void SetupManagedPlaceholder(TextBox textBox, string placeholderText)
		{
			textBox.GotFocus -= Placeholder_GotFocus;
			textBox.LostFocus -= Placeholder_LostFocus;

			textBox.Tag = placeholderText;

			if (string.IsNullOrWhiteSpace(textBox.Text) ||
				textBox.Text == "Select a game..." ||
				textBox.Text == "Not Required" ||
				textBox.ForeColor == Color.Gray)
			{
				textBox.ForeColor = Color.Gray;
				textBox.Text = placeholderText;
			}

			textBox.GotFocus += Placeholder_GotFocus;
			textBox.LostFocus += Placeholder_LostFocus;
		}

		private void Placeholder_GotFocus(object? sender, EventArgs e)
		{
			if (sender is TextBox txt && txt.Text == (string?)(txt.Tag ?? ""))
			{
				txt.Text = "";
				txt.ForeColor = SettingsPalette.PrimaryText;
			}
		}

		private void Placeholder_LostFocus(object? sender, EventArgs e)
		{
			if (sender is TextBox txt && string.IsNullOrWhiteSpace(txt.Text))
			{
				txt.ForeColor = Color.Gray;
				txt.Text = (string?)(txt.Tag ?? "");
			}
		}

		private void btnNavDiscord_Click(object? sender, EventArgs eventArgs)
		{
			ShowSettingsPage(
				pnlPageDiscord,
				btnNavDiscord,
				"Discord Notifications",
				"Use one master webhook or route different Synix events to multiple Discord channels.");
		}

		private static string GetEnteredValue(TextBox textBox)
		{
			if (textBox.ForeColor == Color.Gray ||
				textBox.Text == "Select a game..." ||
				textBox.Text == "Not Required")
			{
				return string.Empty;
			}

			return textBox.Text;
		}

		private void WireUpGatekeeperEvents()
		{
			if (debounceTimer == null) { debounceTimer = new System.Windows.Forms.Timer(); debounceTimer.Interval = 300; debounceTimer.Tick += (s, e) => { debounceTimer.Stop(); SyncGatekeeper(); }; }
			Action trigger = () => { debounceTimer.Stop(); debounceTimer.Start(); };
			txtName.TextChanged += (s, e) => trigger();
			cmbGame.SelectedIndexChanged += (s, e) => trigger();
			numPort.TextChanged += GamePort_TextChanged;
			numPort.ValueChanged += (s, e) => trigger();
			numQueryPort.ValueChanged += (s, e) => trigger();
			numAppPort.ValueChanged += (s, e) => trigger();
			numRconPort.ValueChanged += (s, e) => trigger();
			chkEnableRcon.CheckedChanged += (s, e) => trigger();
			chkCrossplay.CheckedChanged += (s, e) => trigger();
			chkDefaultPath.CheckedChanged += (s, e) => trigger();
			numWorldSize.ValueChanged += (s, e) => trigger();
			cmbGameVersion.SelectedIndexChanged += async (s, e) =>
			{
				trigger();
				if (!isPrivacyLoading && !_suppressMinecraftMetadataEvents && IsMinecraftSelected())
				{
					await RefreshMinecraftRuntimeAsync(cmbMinecraftLoader.Text, "latest");
				}
			};
			cmbMinecraftLoader.SelectedIndexChanged += async (s, e) =>
			{
				trigger();
				if (!isPrivacyLoading && !_suppressMinecraftMetadataEvents && IsMinecraftSelected())
				{
					await RefreshMinecraftRuntimeAsync(cmbMinecraftLoader.Text, "latest");
				}
			};
			cmbMinecraftLoaderVersion.SelectedIndexChanged += (s, e) => trigger();
			if (_minecraftEditionCombo != null)
			{
				_minecraftEditionCombo.SelectedIndexChanged += async (_, _) =>
				{
					trigger();
					if (isPrivacyLoading || _suppressMinecraftMetadataEvents || !IsMinecraftSelected())
						return;

					GameInfo? minecraft = GameDatabase.GetGame("Minecraft");
					if (minecraft == null)
						return;

					try
					{
						_suppressMinecraftMetadataEvents = true;
						ApplyMinecraftEditionDefaults();
						await PopulateVersionsAsync(minecraft, "latest");
					}
					finally
					{
						_suppressMinecraftMetadataEvents = false;
					}
					if (!IsMinecraftBedrockSelected())
					{
						await RefreshMinecraftRuntimeAsync(
							MinecraftMetadataService.VanillaLoader,
							"Official");
					}
				};
			}
			numRam.ValueChanged += (s, e) => trigger();
		}

		private bool IsMinecraftSelected()
		{
			return cmbGame.SelectedIndex > 0 &&
				cmbGame.Text.Equals("Minecraft", StringComparison.OrdinalIgnoreCase);
		}

		private void ConfigureRuntimeCard(GameInfo? gameData)
		{
			bool isMinecraft = gameData?.Game.Equals(
				"Minecraft",
				StringComparison.OrdinalIgnoreCase) == true;
			bool supportsServerFramework = gameData?.SupportedServerFrameworks.Count > 0;
			bool visible = isMinecraft || supportsServerFramework;
			cardMinecraftRuntime.Visible = visible;
			cardCredentials.Location = visible
				? new Point(0, cardMinecraftRuntime.Bottom + 16)
				: new Point(0, 242);
			cardCompatibility.Location = new Point(0, cardCredentials.Bottom + 16);

			if (isMinecraft)
			{
				lblMinecraftRuntimeTitle.Text = "Minecraft Runtime";
				bool bedrock = IsMinecraftBedrockSelected();
				if (_minecraftEditionLabel != null) _minecraftEditionLabel.Visible = true;
				if (_minecraftEditionCombo != null) _minecraftEditionCombo.Visible = true;
				lblMinecraftLoader.Text = bedrock ? "Server Package" : "Loader";
				lblMinecraftLoaderVersion.Visible = !bedrock;
				cmbMinecraftLoaderVersion.Visible = !bedrock;
				lblMinecraftJava.Visible = !bedrock;
				lblMinecraftJavaValue.Visible = !bedrock;
				cmbMinecraftLoader.Items.Clear();
				if (bedrock)
				{
					cmbMinecraftLoader.Items.Add("Official Bedrock");
					cmbMinecraftLoader.SelectedIndex = 0;
					cmbMinecraftLoader.Enabled = false;
					lblMinecraftRuntimeHelper.Text =
						"Synix installs Microsoft's official Bedrock Dedicated Server. Java and Java mod loaders do not apply.";
				}
				else
				{
					cmbMinecraftLoader.Items.AddRange(["Vanilla", "Fabric", "Forge"]);
					if (MinecraftMetadataService.IsNeoForgeCompatibleVersion(cmbGameVersion.Text))
						cmbMinecraftLoader.Items.Add(MinecraftMetadataService.NeoForgeLoader);
				}
			}
			else if (supportsServerFramework && gameData != null)
			{
				lblMinecraftRuntimeTitle.Text = "Server Framework";
				if (_minecraftEditionLabel != null) _minecraftEditionLabel.Visible = false;
				if (_minecraftEditionCombo != null) _minecraftEditionCombo.Visible = false;
				lblMinecraftLoader.Text = "Framework";
				lblMinecraftLoaderVersion.Visible = false;
				cmbMinecraftLoaderVersion.Visible = false;
				lblMinecraftJava.Visible = false;
				lblMinecraftJavaValue.Visible = false;
				cmbMinecraftLoader.Items.Clear();
				cmbMinecraftLoader.Items.Add("Vanilla");
				foreach (string framework in gameData.SupportedServerFrameworks)
					cmbMinecraftLoader.Items.Add(framework);
				string preferred = _existingServer?.ServerFramework ?? "Vanilla";
				SelectComboBoxValue(cmbMinecraftLoader, preferred, "Vanilla");
				lblMinecraftRuntimeHelper.Text =
					"Synix installs the official Oxide runtime only. Plugins remain user-managed in the server's oxide\\plugins folder.";
			}

			if (!visible)
			{
				if (_minecraftEditionLabel != null) _minecraftEditionLabel.Visible = false;
				if (_minecraftEditionCombo != null) _minecraftEditionCombo.Visible = false;
				_minecraftMetadataRequestId++;
				_suppressMinecraftMetadataEvents = false;
				_isLoadingMinecraftMetadata = false;
				_minecraftMetadataError = string.Empty;
				_resolvedMinecraftJavaVersion = 0;
			}
		}

		private async Task InitializeExistingMinecraftSelectionAsync()
		{
			if (_existingServer == null || !IsMinecraftSelected() || IsDisposed)
				return;

			GameInfo? gameData = GameDatabase.GetGame(_existingServer.Game);
			if (gameData == null)
				return;

			try
			{
				await PopulateVersionsAsync(gameData, _existingServer.GameVersion ?? "latest");
				if (IsMinecraftBedrockSelected())
				{
					ConfigureRuntimeCard(gameData);
					return;
				}
				await RefreshMinecraftRuntimeAsync(
					_existingServer.MinecraftLoader,
					_existingServer.MinecraftLoaderVersion);
			}
			catch (Exception ex)
			{
				_minecraftMetadataError = $"Metadata could not be loaded: {ex.Message}";
				SyncGatekeeper();
			}
		}

		private async Task RefreshMinecraftRuntimeAsync(
			string? preferredLoader,
			string? preferredLoaderVersion)
		{
			if (!IsMinecraftSelected() || IsMinecraftBedrockSelected() || IsDisposed)
				return;

			int requestId = ++_minecraftMetadataRequestId;
			_isLoadingMinecraftMetadata = true;
			_minecraftMetadataError = string.Empty;
			_suppressMinecraftMetadataEvents = true;
			ConfigureRuntimeCard(GameDatabase.GetGame("Minecraft"));
			SyncGatekeeper();

			string loader = MinecraftMetadataService.NormalizeLoader(preferredLoader);
			if (!cmbMinecraftLoader.Items.Contains(loader))
				loader = MinecraftMetadataService.VanillaLoader;
			try
			{
				SelectComboBoxValue(cmbMinecraftLoader, loader, MinecraftMetadataService.VanillaLoader);

				cmbMinecraftLoaderVersion.Items.Clear();
				cmbMinecraftLoaderVersion.Items.Add("Loading compatible builds...");
				cmbMinecraftLoaderVersion.SelectedIndex = 0;
				cmbMinecraftLoaderVersion.Enabled = false;
				lblMinecraftJavaValue.Text = "Resolving...";
				lblMinecraftRuntimeHelper.Text = loader == MinecraftMetadataService.VanillaLoader
					? "Synix installs the official server and matching portable Java."
					: $"Synix installs the compatible {loader} server loader. Add your own mods after installation.";

				Task<MinecraftMetadataService.MinecraftVersionMetadata> metadataTask =
					MinecraftMetadataService.GetVersionMetadataAsync(cmbGameVersion.Text);
				Task<IReadOnlyList<string>> loaderTask =
					MinecraftMetadataService.GetLoaderVersionsAsync(loader, cmbGameVersion.Text);

				await Task.WhenAll(metadataTask, loaderTask);
				if (requestId != _minecraftMetadataRequestId || IsDisposed || !IsMinecraftSelected())
					return;

				MinecraftMetadataService.MinecraftVersionMetadata metadata = await metadataTask;
				IReadOnlyList<string> compatibleBuilds = await loaderTask;
				if (compatibleBuilds.Count == 0)
				{
					throw new InvalidOperationException(
						$"No compatible {loader} server build exists for Minecraft {metadata.Version}.");
				}

				cmbMinecraftLoaderVersion.Items.Clear();
				foreach (string build in compatibleBuilds)
					cmbMinecraftLoaderVersion.Items.Add(build);

				string requestedBuild = preferredLoaderVersion?.Trim() ?? "";
				if (requestedBuild.Length == 0 ||
					requestedBuild.Equals("latest", StringComparison.OrdinalIgnoreCase) ||
					!cmbMinecraftLoaderVersion.Items.Contains(requestedBuild))
				{
					cmbMinecraftLoaderVersion.SelectedIndex = 0;
				}
				else
				{
					cmbMinecraftLoaderVersion.SelectedItem = requestedBuild;
				}

				_resolvedMinecraftJavaVersion = metadata.JavaMajorVersion;
				lblMinecraftJavaValue.Text = $"Java {metadata.JavaMajorVersion}";
				lblMinecraftRuntimeHelper.Text = loader == MinecraftMetadataService.VanillaLoader
					? $"Minecraft {metadata.Version} uses the official Mojang server and Java {metadata.JavaMajorVersion}."
					: $"Minecraft {metadata.Version} + {loader} {cmbMinecraftLoaderVersion.Text} uses Java {metadata.JavaMajorVersion}. Add mods after installation.";
			}
			catch (Exception ex)
			{
				if (requestId != _minecraftMetadataRequestId || IsDisposed)
					return;

				_resolvedMinecraftJavaVersion = 0;
				_minecraftMetadataError = $"{ex.Message} Re-select the version or loader to retry.";
				cmbMinecraftLoaderVersion.Items.Clear();
				lblMinecraftJavaValue.Text = "Unavailable";
				lblMinecraftRuntimeHelper.Text = "Synix could not verify this loader combination from the official metadata service.";
			}
			finally
			{
				if (requestId == _minecraftMetadataRequestId && !IsDisposed)
				{
					_suppressMinecraftMetadataEvents = false;
					_isLoadingMinecraftMetadata = false;
					SyncGatekeeper();
				}
			}
		}

		private static void SelectComboBoxValue(
			ComboBox comboBox,
			string value,
			string fallback)
		{
			if (comboBox.Items.Contains(value))
				comboBox.SelectedItem = value;
			else if (comboBox.Items.Contains(fallback))
				comboBox.SelectedItem = fallback;
			else if (comboBox.Items.Count > 0)
				comboBox.SelectedIndex = 0;
		}

		private void GamePort_TextChanged(object? sender, EventArgs e)
		{
			if (isPrivacyLoading ||
				_isApplyingPortOffset ||
				cmbGame.SelectedIndex <= 0 ||
				!numPort.Enabled ||
				!numQueryPort.Enabled)
				return;

			GameInfo? gameData = GameDatabase.GetGame(cmbGame.Text);
			if (gameData == null ||
				!int.TryParse(numPort.Text, out int gamePort))
			{
				return;
			}

			long defaultOffset = (long)gameData.QueryPort - gameData.Port;
			long calculatedQueryPort = gamePort + defaultOffset;
			int clampedQueryPort = (int)Math.Clamp(
				calculatedQueryPort,
				(long)numQueryPort.Minimum,
				(long)numQueryPort.Maximum);
			if (!_isEditMode)
			{
				clampedQueryPort = ExistingServerImport.FindAvailablePort(
					clampedQueryPort,
					MainGUI.serverList.Concat([new GameServer { Port = gamePort }]));
			}

			try
			{
				_isApplyingPortOffset = true;
				numQueryPort.Value = clampedQueryPort;
			}
			finally
			{
				_isApplyingPortOffset = false;
			}
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			string newName = txtName.Text.Trim();
			string selectedGame = cmbGame.Text;
			if (!Core.Instance.ValidateNameAndReport(newName, selectedGame, _existingServer)) return;

			int gPort = (int)numPort.Value;
			int qPort = (int)numQueryPort.Value;
			int rPort = (int)numRconPort.Value;
			int wSize = (int)numWorldSize.Value;
			string worldName = cmbWorldName.Text;
			if (selectedGame.Equals("7 Days to Die", StringComparison.OrdinalIgnoreCase))
			{
				worldName = SevenDaysToDieConfiguration.NormalizeWorldName(worldName);
				wSize = SevenDaysToDieConfiguration.NormalizeWorldSize(wSize);
			}

			int? aPort = numAppPort.Enabled ? (int)numAppPort.Value : (int?)null;
			bool checkRconPort = chkEnableRcon.Enabled && chkEnableRcon.Checked;
			if (!Core.Instance.ValidatePortsAndReport(
				_existingServer,
				gPort,
				qPort,
				rPort,
				checkRconPort,
				aPort ?? 0,
				numAppPort.Enabled,
				selectedGame,
				numPort.Enabled,
				numQueryPort.Enabled)) return;
			if (!Core.TryValidateExtraArguments(
				txtExtraArgs.Text,
				out string extraArgumentsError))
			{
				MessageBox.Show(
					extraArgumentsError,
					"Extra Arguments Blocked",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				txtExtraArgs.Focus();
				return;
			}
			if (!discordSettingsPage.TryGetSettings(
				out DiscordSettingsSnapshot discordSettings,
				out string discordSettingsError))
			{
				MessageBox.Show(
					discordSettingsError,
					"Discord Settings Need Attention",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				btnNavDiscord.PerformClick();
				return;
			}
			string newPath = txtInstallPath.Text.Trim();
			bool isMinecraft = selectedGame.Equals("Minecraft", StringComparison.OrdinalIgnoreCase);
			bool isMinecraftBedrock = isMinecraft && IsMinecraftBedrockSelected();
			GameInfo? masterData = GameDatabase.GetGame(selectedGame);
			bool supportsServerFramework = masterData?.SupportedServerFrameworks.Count > 0;
			string steamAccountName = masterData?.RequiresSteamLogin == true
				? _existingServer?.SteamAccountName ?? string.Empty
				: string.Empty;

			if (masterData?.RequiresSteamLogin == true &&
				string.IsNullOrWhiteSpace(steamAccountName))
			{
				using SteamAccountLoginDialog loginDialog = new(selectedGame);
				if (loginDialog.ShowDialog(this) != DialogResult.OK)
					return;

				steamAccountName = loginDialog.SteamAccountName;
			}

			NewServer = new GameServer
			{
				Game = selectedGame,
				SteamAccountName = steamAccountName,
				ServerName = newName,
				Port = gPort,
				QueryPort = qPort,
				RconPort = rPort,
				AppPort = aPort,
				Password = GetEnteredValue(txtPassword),
				AdminPassword = GetEnteredValue(txtAdminPassword),
				MaxPlayers = (int)numMaxPlayers.Value,
				WorldName = worldName,
				GameMode = cmbCompetitive.Text,
				CrossplayEnabled = chkCrossplay.Checked,
				WorldSeed = GetEnteredValue(txtWorldSeed).Trim(),
				WorldSize = wSize,
				ExtraArgs = txtExtraArgs.Text,
				IsDefaultPath = chkDefaultPath.Checked,
				UpdateOnStart = chkUpdateOnStart.Checked,
				EnableRcon = !isMinecraftBedrock && chkEnableRcon.Checked,
				RconPassword = isMinecraftBedrock ? string.Empty : GetEnteredValue(txtRconPassword),
				InstallPath = newPath,
				MaxRam = (int)numRam.Value,
				GameVersion = cmbGameVersion.Text.Trim(),
				MinecraftEdition = isMinecraft
					? MinecraftControlProfile.NormalizeEdition(_minecraftEditionCombo?.Text)
					: MinecraftControlProfile.JavaEdition,
				MinecraftLoader = isMinecraft && !isMinecraftBedrock
					? MinecraftMetadataService.NormalizeLoader(cmbMinecraftLoader.Text)
					: MinecraftMetadataService.VanillaLoader,
				MinecraftLoaderVersion = isMinecraft && !isMinecraftBedrock
					? cmbMinecraftLoaderVersion.Text.Trim()
					: "Official",
				EnableMinecraftManagementProtocol = isMinecraft && !isMinecraftBedrock &&
					(_existingServer?.EnableMinecraftManagementProtocol ?? true),
				MinecraftManagementPort = isMinecraft && !isMinecraftBedrock
					? _existingServer?.MinecraftManagementPort ?? 0
					: 0,
				ServerFramework = supportsServerFramework
					? cmbMinecraftLoader.Text.Trim()
					: "Vanilla",
				ServerFrameworkVersion = supportsServerFramework &&
					string.Equals(
						_existingServer?.ServerFramework,
						cmbMinecraftLoader.Text.Trim(),
						StringComparison.OrdinalIgnoreCase)
						? _existingServer?.ServerFrameworkVersion ?? "Official"
						: "Official",
				RequiredJavaVersion = isMinecraft && !isMinecraftBedrock
					? _resolvedMinecraftJavaVersion
					: 0,
				IsScheduledRestartEnabled = chkEnableSchedule.Checked,
				RestartTime = _selectedTime,
				RestartDays = (bool[])_selectedDays.Clone(),
				SmartMaintenanceEnabled = _smartMaintenanceEnabled,
				MaintenanceWaitForPlayers = _maintenanceWaitForPlayers,
				MaintenanceMaximumDelayMinutes = _maintenanceMaximumDelayMinutes,
				MaintenanceBackupBeforeRestart = _maintenanceBackupBeforeRestart,
				MaintenanceUpdateBeforeRestart = _maintenanceUpdateBeforeRestart,
				IsDiscordAlertEnabled = discordSettings.MasterEnabled,
				DiscordWebhook = discordSettings.MasterWebhook,
				DiscordEvents = discordSettings.MasterEvents,
				DiscordWebhookRoutes = discordSettings.Routes
					.Select(route => new DiscordWebhookRoute
					{
						Id = route.Id,
						Name = route.Name,
						Enabled = route.Enabled,
						WebhookUrl = route.WebhookUrl,
						Events = route.Events
					})
					.ToList(),
				Status = _existingServer?.Status ?? StatusManager.GetStatus(ServerState.Stopped),
				BackupOnStart = chkBackupOnStart.Checked
			};

			if (MinecraftControlProfile.IsJava(NewServer))
				MinecraftControlProfile.EnsureDefaults(NewServer, MainGUI.serverList);

			if (!IsGameServerConfigSafe(NewServer))
			{
				MessageBox.Show("Security Alert: One of your inputs contains illegal characters.",
								"Input Blocked", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			try
			{
				Core.SetServerSecrets(
					NewServer,
					new SynixServerSecrets(
						new SynixServerPasswords(
							GetEnteredValue(txtPassword),
							GetEnteredValue(txtAdminPassword),
							GetEnteredValue(txtRconPassword)),
						discordSettings.MasterWebhook));
				Core.SetDiscordWebhookRoutes(NewServer, discordSettings.Routes);

				if (_isEditMode && _existingServer != null)
				{
					var existing = MainGUI.serverList.FirstOrDefault(s => s.ServerName == _existingServer.ServerName);
					if (existing != null)
					{
						NewServer.IsFirstBoot = false;
						int index = MainGUI.serverList.IndexOf(existing);
						MainGUI.serverList[index] = NewServer;
					}
				}
				else MainGUI.serverList.Add(NewServer);

				if (masterData != null)
				{
					string fullExePath = GameLaunchCommandBuilder.ResolveExecutablePath(NewServer, masterData);
					string iconPath = Synix_Control_Panel.SynixEngine.Core.GetLocalServerIcon(NewServer.Game, fullExePath);

					if (System.IO.File.Exists(iconPath))
					{
						if (!MainGUI.ServerIconsCache.TryGetValue(NewServer.Game, out Image? cachedIcon))
						{
							using MemoryStream stream = new(File.ReadAllBytes(iconPath));
							using Image sourceImage = Image.FromStream(stream);
							cachedIcon = new Bitmap(sourceImage);
							MainGUI.ServerIconsCache[NewServer.Game] = cachedIcon;
						}

						NewServer.DisplayIcon = cachedIcon;
					}
				}

				FileHandler.SaveServers(); this.DialogResult = DialogResult.OK; this.Close();
			}
			catch (Exception ex)
			{
				PlainEnglishErrorDialog.ShowError(this, "save the server settings", ex.ToString());
			}
		}

		private async void cmbGame_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (isPrivacyLoading) return;
			if (cmbGame.SelectedIndex > 0)
			{
				string? selectedGame = cmbGame.SelectedItem?.ToString();
				if (string.IsNullOrWhiteSpace(selectedGame))
					return;
				var gameData = GameDatabase.GetGame(selectedGame);
				if (gameData != null)
				{
					ConfigureWorldSizeInput(gameData);
					int initialWorldSize = IsSevenDaysToDie(gameData)
						? SevenDaysToDieConfiguration.NormalizeWorldSize(gameData.WorldSize)
						: gameData.WorldSize;
					if (initialWorldSize > 0)
					{
						numWorldSize.Value = Math.Clamp(
							initialWorldSize,
							numWorldSize.Minimum,
							numWorldSize.Maximum);
					}
					int gamePort = Math.Clamp(
						gameData.Port,
						(int)numPort.Minimum,
						(int)numPort.Maximum);
					int queryPort = Math.Clamp(
						gameData.QueryPort,
						(int)numQueryPort.Minimum,
						(int)numQueryPort.Maximum);
					if (!_isEditMode)
					{
						gamePort = ExistingServerImport.FindAvailablePort(
							gamePort,
							MainGUI.serverList);
						queryPort = ExistingServerImport.FindAvailablePort(
							queryPort,
							MainGUI.serverList.Concat([new GameServer { Port = gamePort }]));
					}
					numPort.Value = gamePort;
					numQueryPort.Value = queryPort;
					if (gameData.AppPort.HasValue)
					{
						numAppPort.Value = Math.Clamp(
							gameData.AppPort.Value,
							numAppPort.Minimum,
							numAppPort.Maximum);
					}
					PopulateMaps(gameData, gameData.Maps?.FirstOrDefault() ?? "");
					PopulateGameModes(
						gameData,
						gameData.GameModes?.FirstOrDefault() ?? "PVE");

					await PopulateVersionsAsync(gameData, _existingServer?.GameVersion ?? "latest");

					ToggleGameSpecificFields(gameData);
					if (gameData.Game.Equals("Minecraft", StringComparison.OrdinalIgnoreCase))
					{
						await RefreshMinecraftRuntimeAsync(
							_existingServer?.MinecraftLoader ?? MinecraftMetadataService.VanillaLoader,
							_existingServer?.MinecraftLoaderVersion ?? "Official");
					}
				}
			}
			else ToggleGameSpecificFields(null);
			RefreshCompatibilityVerification(cmbGame.SelectedIndex > 0 ? cmbGame.Text : null);
			SyncGatekeeper();
		}

		private void RefreshCompatibilityVerification(string? game)
		{
			GameCompatibilityVerification verification = Core.GetGameCompatibility(game);
			UpdateCompatibilityLabel(
				lblInstallVerification,
				"Install",
				verification.Install);
			UpdateCompatibilityLabel(
				lblStartVerification,
				"Start",
				verification.Start);
			UpdateCompatibilityLabel(
				lblStopVerification,
				"Stop",
				verification.Stop);
			UpdateCompatibilityLabel(
				lblMonitoringVerification,
				"Monitoring",
				verification.Monitoring);

			GameVerificationEvidence? lastTested = verification.LastTested;
			if (lastTested == null)
			{
				lblLastTestedVersion.Text = "Last-tested Synix version: Not verified yet";
				lblLastTestedVersion.ForeColor = Color.FromArgb(158, 172, 194);
				return;
			}

			lblLastTestedVersion.Text =
				$"Last-tested Synix version: v{lastTested.SynixVersion}  •  {lastTested.VerifiedAtUtc.ToLocalTime():MMM d, yyyy}";
			lblLastTestedVersion.ForeColor = Color.FromArgb(32, 214, 199);
		}

		private static void UpdateCompatibilityLabel(
			Label label,
			string action,
			GameVerificationEvidence? evidence)
		{
			bool verified = evidence != null;
			label.Text = verified
				? $"{action}  ✓ Verified"
				: $"{action}  — Not verified yet";
			label.ForeColor = verified
				? Color.FromArgb(32, 214, 199)
				: Color.FromArgb(158, 172, 194);
		}

		private async Task PopulateVersionsAsync(GameInfo gameData, string selectedVersion)
		{
			_suppressMinecraftMetadataEvents = true;
			try
			{
				cmbGameVersion.Items.Clear();
				cmbGameVersion.Items.Add("latest");

				if (gameData.Game.StartsWith("Minecraft", StringComparison.OrdinalIgnoreCase) &&
					!IsMinecraftBedrockSelected())
				{
					try
					{
						MinecraftMetadataService.MinecraftVersionCatalog catalog =
							await MinecraftMetadataService.GetVersionCatalogAsync();
						if (IsDisposed || Disposing)
							return;

						foreach (string version in catalog.ReleaseVersions)
							cmbGameVersion.Items.Add(version);
					}
					catch (Exception ex)
					{
						_minecraftMetadataError = $"Mojang versions could not be loaded: {ex.Message}";
					}
				}

				string versionToSelect = selectedVersion.Equals(
					"latest",
					StringComparison.OrdinalIgnoreCase)
					? "latest"
					: selectedVersion;

				if (!string.IsNullOrWhiteSpace(versionToSelect) &&
					!cmbGameVersion.Items.Contains(versionToSelect))
				{
					cmbGameVersion.Items.Add(versionToSelect);
				}

				if (!string.IsNullOrWhiteSpace(versionToSelect))
					cmbGameVersion.SelectedItem = versionToSelect;
				else if (cmbGameVersion.Items.Count > 0)
					cmbGameVersion.SelectedIndex = 0;
			}
			finally
			{
				if (!IsDisposed)
					_suppressMinecraftMetadataEvents = false;
			}
		}

		private void btnBrowse_Click(object sender, EventArgs e) { using var fbd = new FolderBrowserDialog(); if (fbd.ShowDialog() == DialogResult.OK) { txtInstallPath.Text = fbd.SelectedPath; SyncGatekeeper(); } }
		private void chkDefaultPath_CheckedChanged(object sender, EventArgs e) => SyncGatekeeper();
		private void txtInstallPath_TextChanged(object sender, EventArgs e) => SyncGatekeeper();
		private void chkEnableRcon_CheckedChanged(object sender, EventArgs e) { if (isPrivacyLoading) return; bool active = chkEnableRcon.Checked; numRconPort.Enabled = txtRconPassword.Enabled = active; SyncGatekeeper(); }
		private void chkEnableSchedule_CheckedChanged(object sender, EventArgs e) { if (isPrivacyLoading) return; if (btnEditSchedule != null) btnEditSchedule.Enabled = chkEnableSchedule.Checked; SyncGatekeeper(); }
		private void txtWorldSeed_KeyPress(object sender, KeyPressEventArgs e) { if (cmbGame.Text == "Rust" && !char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; }
		private void btnViewArgs_Click(object sender, EventArgs e)
		{
			GameInfo? gameData = GameDatabase.GetGame(cmbGame.Text);
			if (gameData == null)
				return;

			using DefaultArgumentsDisplay display = new(gameData.RequiredArgs);
			display.ShowDialog(this);
		}
		private void btnEditSchedule_Click(object sender, EventArgs e)
		{
			using ScheduleSettingsGUI scheduler = new(
				_selectedDays,
				_selectedTime,
				_smartMaintenanceEnabled,
				_maintenanceWaitForPlayers,
				_maintenanceMaximumDelayMinutes,
				_maintenanceBackupBeforeRestart,
				_maintenanceUpdateBeforeRestart);
			if (scheduler.ShowDialog(this) != DialogResult.OK)
				return;

			_selectedDays = scheduler.SelectedDays;
			_selectedTime = scheduler.SelectedTime;
			_smartMaintenanceEnabled = scheduler.SmartMaintenanceEnabled;
			_maintenanceWaitForPlayers = scheduler.WaitForPlayers;
			_maintenanceMaximumDelayMinutes = scheduler.MaximumDelayMinutes;
			_maintenanceBackupBeforeRestart = scheduler.BackupBeforeRestart;
			_maintenanceUpdateBeforeRestart = scheduler.UpdateBeforeRestart;
		}

		private bool IsMinecraftBedrockSelected() =>
			IsMinecraftSelected() &&
			MinecraftControlProfile.NormalizeEdition(_minecraftEditionCombo?.Text) ==
				MinecraftControlProfile.BedrockEdition;

		private void ApplyMinecraftEditionDefaults()
		{
			bool bedrock = IsMinecraftBedrockSelected();
			QueryPortLabel.Text = bedrock ? "IPv6 Port" : "Query Port";
			if (!_isEditMode)
			{
				int preferredPort = bedrock
					? MinecraftControlProfile.BedrockDefaultPort
					: 25565;
				int preferredSecondaryPort = bedrock
					? MinecraftControlProfile.BedrockDefaultIpv6Port
					: 25565;
				int gamePort = ExistingServerImport.FindAvailablePort(
					preferredPort,
					MainGUI.serverList);
				int secondaryPort = ExistingServerImport.FindAvailablePort(
					preferredSecondaryPort,
					MainGUI.serverList.Concat([new GameServer { Port = gamePort }]));
				numPort.Value = Math.Clamp(gamePort, numPort.Minimum, numPort.Maximum);
				numQueryPort.Value = Math.Clamp(
					secondaryPort,
					numQueryPort.Minimum,
					numQueryPort.Maximum);
			}

			if (bedrock)
			{
				chkEnableRcon.Checked = false;
				chkEnableRcon.Tag = "Disabled";
				cmbCompetitive.Items.Clear();
				cmbCompetitive.Items.AddRange(["Survival", "Creative", "Adventure"]);
				cmbCompetitive.SelectedItem = "Survival";
			}
			else if (GameDatabase.GetGame("Minecraft") is GameInfo minecraft)
			{
				PopulateGameModes(minecraft, _existingServer?.GameMode ?? "PVE");
			}
			ToggleGameSpecificFields(GameDatabase.GetGame("Minecraft"));
		}
		private void btnCancel_Click(object sender, EventArgs e) { this.DialogResult = DialogResult.Cancel; this.Close(); }
		private void txtName_TextChanged(object sender, EventArgs e) => SyncGatekeeper();

		private void PopulateMaps(GameInfo gameData, string selectedMap)
		{
			cmbWorldName.Items.Clear();
			if (gameData.Maps != null)
			{
				foreach (string map in gameData.Maps)
					cmbWorldName.Items.Add(map);
			}

			if (!string.IsNullOrWhiteSpace(selectedMap) &&
				!cmbWorldName.Items.Contains(selectedMap))
			{
				cmbWorldName.Items.Add(selectedMap);
			}

			if (!string.IsNullOrWhiteSpace(selectedMap))
				cmbWorldName.SelectedItem = selectedMap;
			else if (cmbWorldName.Items.Count > 0)
				cmbWorldName.SelectedIndex = 0;
		}

		private static bool IsSevenDaysToDie(GameInfo? gameData) =>
			gameData?.Game.Equals(
				"7 Days to Die",
				StringComparison.OrdinalIgnoreCase) == true;

		private void ConfigureWorldSizeInput(GameInfo? gameData)
		{
			if (IsSevenDaysToDie(gameData))
			{
				numWorldSize.Maximum = 10240;
				numWorldSize.Minimum = 6144;
				numWorldSize.Increment = 2048;
				return;
			}

			numWorldSize.Minimum = 50;
			numWorldSize.Maximum = 5000;
			numWorldSize.Increment = 1;
		}

		private void PopulateGameModes(GameInfo gameData, string selectedMode)
		{
			cmbCompetitive.Items.Clear();
			if (gameData.GameModes != null)
			{
				foreach (string mode in gameData.GameModes)
					cmbCompetitive.Items.Add(mode);
			}

			if (!string.IsNullOrWhiteSpace(selectedMode) &&
				!cmbCompetitive.Items.Contains(selectedMode))
			{
				cmbCompetitive.Items.Add(selectedMode);
			}

			if (!string.IsNullOrWhiteSpace(selectedMode))
				cmbCompetitive.SelectedItem = selectedMode;
			else if (cmbCompetitive.Items.Count > 0)
				cmbCompetitive.SelectedIndex = 0;
		}
	}
}
