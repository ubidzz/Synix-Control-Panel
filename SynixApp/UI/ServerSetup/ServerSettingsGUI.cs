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
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Database.GameConfigurations;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using Synix_Control_Panel.SynixApp.Localization;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine;
using System.ComponentModel;
using System.Runtime.InteropServices;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel.SynixApp.UI.ServerSetup
{
	public partial class ServerSettingsGUI : Form
	{
		public GameServer? NewServer { get; private set; }
		private bool isPrivacyLoading = false;
		private bool _isEditMode = false;
		private GameServer? _existingServer = null;
		private System.Windows.Forms.Timer? debounceTimer;
		private System.Windows.Forms.Timer? _navigationAttentionTimer;
		private float _navigationAttentionPhase;
		private bool _PrivacyMode = false;
		private bool _passwordUnlockFailed;
		private string _validationMessage =
			"  🔒 [REQUIRED] Enter a Server Name and select a Game Template.";
		private bool _advancedMode;
		private ModernSettingsButton? _experienceModeButton;
		private Label? _completionLabel;
		private Panel? _completionTrack;
		private Panel? _completionFill;

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
			pnlPageGeneral.Initialize(server);
			pnlPageInstall.Initialize(() =>
				GameDatabase.GetGame(pnlPageGeneral.SelectedGame));
			WirePageControlEvents();
			ThemeManager.Apply(this);

			if (_isEditMode && _existingServer != null)
			{
				LoadExistingServerData();
			}
			else
			{
				pnlPageGeneral.SelectNoGame();
				ToggleGameSpecificFields(null);
			}
			pnlPageGeneral.RefreshCompatibilityVerification(_existingServer?.Game);

			isPrivacyLoading = false;
			PrivacyMode();
			SyncGatekeeper();

			if (_isEditMode && _existingServer != null)
			{
				Shown += async (_, _) =>
					await pnlPageGeneral.InitializeExistingMinecraftSelectionAsync();
			}
		}

		private void ConfigureModernShell()
		{
			lblModeBadge.Text = _isEditMode ? "EDIT SERVER" : "NEW SERVER";
			btnSave.Text = _isEditMode ? "Save Changes" : "Save Server";
			Text = _isEditMode ? "Edit Server" : "Server Setup";
			InitializeGuidanceControls();
			InitializeNavigationAttention();
			ShowSettingsPage(
				pnlPageGeneral,
				btnNavGeneral,
				"General",
				"Choose the game and define the server identity.");
			UpdateModernStatus();
		}

		private void InitializeNavigationAttention()
		{
			components ??= new System.ComponentModel.Container();
			_navigationAttentionTimer = new System.Windows.Forms.Timer(components)
			{
				Interval = 90
			};
			_navigationAttentionTimer.Tick += (_, _) =>
			{
				_navigationAttentionPhase += 0.32F;
				if (_navigationAttentionPhase >= MathF.Tau)
					_navigationAttentionPhase -= MathF.Tau;

				float pulse = 0.5F + (MathF.Sin(_navigationAttentionPhase) * 0.5F);
				foreach (ModernSettingsNavButton button in GetNavigationButtons())
				{
					if (button.AttentionRequired)
						button.AttentionPulse = pulse;
				}
			};
		}

		private ModernSettingsNavButton[] GetNavigationButtons() =>
		[
			btnNavGeneral,
			btnNavSecurity,
			btnNavWorld,
			btnNavNetwork,
			btnNavAutomation,
			btnNavDiscord,
			btnNavInstall
		];

		private void UpdateNavigationAttention(
			bool general,
			bool security,
			bool world,
			bool network,
			bool automation,
			bool discord,
			bool install)
		{
			(ModernSettingsNavButton Button, bool Required)[] states =
			[
				(btnNavGeneral, general),
				(btnNavSecurity, security),
				(btnNavWorld, world),
				(btnNavNetwork, network),
				(btnNavAutomation, automation),
				(btnNavDiscord, discord),
				(btnNavInstall, install)
			];

			bool anyAttentionRequired = false;
			foreach ((ModernSettingsNavButton button, bool required) in states)
			{
				button.AttentionRequired = required;
				button.AccessibleDescription = required
					? $"{button.Text} contains settings that require attention before saving."
					: $"{button.Text} has no settings that require attention.";
				anyAttentionRequired |= required;
			}

			if (_navigationAttentionTimer == null)
				return;

			if (anyAttentionRequired)
			{
				if (!_navigationAttentionTimer.Enabled)
					_navigationAttentionTimer.Start();
				return;
			}

			_navigationAttentionTimer.Stop();
			_navigationAttentionPhase = 0F;
			foreach (ModernSettingsNavButton button in GetNavigationButtons())
				button.AttentionPulse = 0F;
		}

		private void WirePageControlEvents()
		{
			debounceTimer = new System.Windows.Forms.Timer()
			{
				Interval = 300
			};
			debounceTimer.Tick += (_, _) =>
			{
				debounceTimer.Stop();
				SyncGatekeeper();
			};

			pnlPageGeneral.SettingsChanged += PageSettingsChanged;
			pnlPageGeneral.GameSelectionChanged += cmbGame_SelectedIndexChanged;
			pnlPageGeneral.MinecraftEditionChanged += MinecraftEditionChanged;
			pnlPageSecurity.SettingsChanged += PageSettingsChanged;
			pnlPageWorld.SettingsChanged += PageSettingsChanged;
			pnlPageNetwork.SettingsChanged += PageSettingsChanged;
			pnlPageAutomation.SettingsChanged += PageSettingsChanged;
			pnlPageInstall.SettingsChanged += PageSettingsChanged;
			discordSettingsPage.SettingsChanged += PageSettingsChanged;
		}

		private void PageSettingsChanged(object? sender, EventArgs eventArgs)
		{
			if (isPrivacyLoading || debounceTimer == null)
				return;
			debounceTimer.Stop();
			debounceTimer.Start();
		}

		private void MinecraftEditionChanged(object? sender, EventArgs eventArgs)
		{
			pnlPageNetwork.ApplyMinecraftEditionDefaults(
				pnlPageGeneral.IsMinecraftBedrockSelected);
			ToggleGameSpecificFields(GameDatabase.GetGame("Minecraft"));
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
				Text = LocalizationManager.Get(
					"ServerSetup.Completion",
					0),
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
			pnlPageNetwork.SetAdvancedMode(_advancedMode);
			pnlPageInstall.SetAdvancedMode(_advancedMode);

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
			Control page,
			ModernSettingsNavButton navigationButton,
			string title,
			string description)
		{
			Control[] pages =
			{
				pnlPageGeneral,
				pnlPageSecurity,
				pnlPageWorld,
				pnlPageNetwork,
				pnlPageAutomation,
				pnlPageDiscord,
				pnlPageInstall
			};
			ModernSettingsNavButton[] navigationButtons =
			{
				btnNavGeneral,
				btnNavSecurity,
				btnNavWorld,
				btnNavNetwork,
				btnNavAutomation,
				btnNavDiscord,
				btnNavInstall
			};

			foreach (Control candidate in pages)
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

			lblSidebarStatus.Text = LocalizationManager.Get(ready
				? "ServerSetup.Status.Ready"
				: "ServerSetup.Status.ActionRequired");
			lblSidebarStatus.ForeColor = ready
				? SettingsPalette.Accent
				: SettingsPalette.Warning;
			lblSidebarStatusDetail.Text = LocalizationManager.Get(ready
				? "ServerSetup.Status.AllChecksPassed"
				: "ServerSetup.Status.SeeValidationMessage");
			lblFooterStatus.Text = validationMessage;
			lblFooterStatus.ForeColor = ready
				? SettingsPalette.Accent
				: SettingsPalette.Warning;

			bool hasGame = pnlPageGeneral.HasSelectedGame;
			bool blockedByPort = validationMessage.Contains("[CONFLICT]", StringComparison.OrdinalIgnoreCase) &&
				validationMessage.Contains("Port", StringComparison.OrdinalIgnoreCase);
			bool requirementsMet = !validationMessage.Contains("[REQUIREMENT]", StringComparison.OrdinalIgnoreCase) &&
				!validationMessage.Contains("[MINECRAFT]", StringComparison.OrdinalIgnoreCase) &&
				!validationMessage.Contains("[VALIDATION ERROR]", StringComparison.OrdinalIgnoreCase);
			int completion = UserGuidance.CalculateSetupCompletion(new SetupCompletionState(
				!string.IsNullOrWhiteSpace(pnlPageGeneral.ServerName),
				hasGame,
				!string.IsNullOrWhiteSpace(pnlPageInstall.InstallPath),
				hasGame && !blockedByPort,
				hasGame && requirementsMet,
				ready));
			if (_completionLabel != null)
			{
				_completionLabel.Text = LocalizationManager.Get(
					"ServerSetup.Completion",
					completion);
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
			pnlPageSecurity.ClearSecrets();
			pnlPageNetwork.ClearSecret();
			discordSettingsPage.ClearSecrets();
			debounceTimer?.Stop();
			debounceTimer?.Dispose();
			_navigationAttentionTimer?.Stop();
			_navigationAttentionTimer?.Dispose();
			base.OnFormClosed(eventArgs);
		}

		private void PrivacyMode()
		{
			pnlPageSecurity.SetPrivacyMode(_PrivacyMode);
			pnlPageNetwork.SetPrivacyMode(_PrivacyMode);
			discordSettingsPage.SetPrivacyMode(_PrivacyMode);
		}

		private void LoadExistingServerData()
		{
			if (_existingServer == null) return;
			isPrivacyLoading = true;
			GameInfo? gameData = GameDatabase.GetGame(_existingServer.Game);
			pnlPageGeneral.LoadServer(_existingServer, gameData);
			pnlPageWorld.LoadServer(_existingServer, gameData);
			pnlPageNetwork.LoadServer(_existingServer, gameData);
			pnlPageAutomation.LoadServer(_existingServer);
			pnlPageInstall.LoadServer(_existingServer);

			string inviteCode = _existingServer.InviteCode ?? string.Empty;
			if (string.IsNullOrWhiteSpace(inviteCode) &&
				(GameFix.GetManagementCapabilities(gameData) &
				 GameManagementCapability.InviteCode) != GameManagementCapability.None)
			{
				inviteCode = WindroseConfiguration.ReadInstalledInviteCode(_existingServer);
			}
			if (Core.TryRevealServerSecrets(
					_existingServer,
					out SynixServerSecrets secrets) &&
				Core.TryRevealDiscordWebhookRoutes(
					_existingServer,
					out IReadOnlyList<DiscordWebhookRoute> discordRoutes))
			{
				SynixServerPasswords passwords = secrets.Passwords;
				pnlPageSecurity.LoadSecrets(passwords, inviteCode);
				pnlPageNetwork.SetRconPassword(passwords.RconPassword);
				discordSettingsPage.LoadSettings(
					_existingServer.IsDiscordAlertEnabled,
					secrets.DiscordWebhook,
					_existingServer.DiscordEvents,
					discordRoutes);
			}
			else
			{
				_passwordUnlockFailed = true;
				pnlPageSecurity.ClearProtectedSecrets(inviteCode);
				pnlPageNetwork.SetRconPassword(string.Empty);
				discordSettingsPage.LoadSettings(
					false,
					string.Empty,
					DiscordNotificationEvent.All,
					[]);
				Shown += ShowPasswordUnlockWarning;
			}
			discordSettingsPage.SetServerName(_existingServer.ServerName);
			ToggleGameSpecificFields(gameData);
			isPrivacyLoading = false;
		}

		private void ShowPasswordUnlockWarning(object? sender, EventArgs eventArgs)
		{
			Shown -= ShowPasswordUnlockWarning;
			if (!_passwordUnlockFailed)
				return;

			LocalizedMessageBox.Show(
				"Synix could not unlock this server's saved passwords, authentication token, or Discord webhooks. They may have come from another Windows user or computer.\n\nEnter the credentials again and press Save Changes to protect them for this Windows user.",
				"Re-enter Server Credentials",
				MessageBoxButtons.OK,
				MessageBoxIcon.Warning);
		}

		private void SyncGatekeeper()
		{
			if (isPrivacyLoading)
				return;

			try
			{
				string currentName = pnlPageGeneral.ServerName;
				bool hasName = !string.IsNullOrWhiteSpace(currentName);
				bool hasGame = pnlPageGeneral.HasSelectedGame;
				string selectedGame = pnlPageGeneral.SelectedGame;
				bool isBaseReady = hasName && hasGame;
				GameInfo? selectedDefinition = hasGame
					? GameDatabase.GetGame(selectedGame)
					: null;
				bool isMinecraft = pnlPageGeneral.IsMinecraftSelected;
				bool isMinecraftBedrock = pnlPageGeneral.IsMinecraftBedrockSelected;

				pnlPageGeneral.ApplyAvailability(hasGame);
				pnlPageSecurity.ApplyAvailability(hasGame);
				pnlPageWorld.ApplyAvailability(hasGame);
				pnlPageNetwork.ApplyAvailability(hasGame);
				pnlPageAutomation.UpdateAvailability(isBaseReady);
				pnlPageInstall.UpdateAvailability(isBaseReady, _isEditMode);
				discordSettingsPage.SetServerName(currentName);
				discordSettingsPage.SetEditingEnabled(isBaseReady);

				if (!_isEditMode && isBaseReady && pnlPageInstall.UseDefaultPath)
				{
					string safeName = Core.Instance.GetSafeName(currentName);
					string safeGame = Core.Instance.GetSafeName(selectedGame);
					pnlPageInstall.SetInstallPath(
						Path.Combine(Core.GamesPath, safeGame, safeName));
				}

				string? serverInputError = null;
				if (!pnlPageSecurity.TryValidate(
						currentName,
						pnlPageNetwork.RconPassword,
						out string validationError))
				{
					serverInputError = validationError;
				}

				GamePrerequisiteItem? missingRequirement = selectedDefinition == null
					? null
					: GamePrerequisiteChecker
						.CheckCurrentSystem(selectedDefinition)
						.FirstFailure;
				PortValidationResult portValidation =
					pnlPageNetwork.ValidatePorts(_existingServer);
				bool isNameTaken = ServerRegistry.Servers.Any(server =>
					server != _existingServer &&
					server.Game.Equals(selectedGame, StringComparison.OrdinalIgnoreCase) &&
					server.ServerName.Equals(currentName, StringComparison.OrdinalIgnoreCase));
				bool minecraftVersionNeedsAttention = isMinecraft &&
					(string.IsNullOrWhiteSpace(pnlPageGeneral.GameVersion) ||
					 (!isMinecraftBedrock &&
					  !string.IsNullOrWhiteSpace(pnlPageGeneral.MinecraftMetadataError)));
				bool minecraftLoaderNeedsAttention = isMinecraft &&
					!pnlPageGeneral.HasMinecraftLoaderSelection;
				bool scheduleNeedsAttention = isBaseReady &&
					!pnlPageAutomation.HasValidSchedule;
				bool extraArgumentsValid =
					pnlPageInstall.TryValidateExtraArguments(out string extraArgumentsError);
				string discordSettingsError = string.Empty;
				bool discordSettingsValid = !isBaseReady ||
					discordSettingsPage.TryGetSettings(
						out _,
						out discordSettingsError);

				UpdateNavigationAttention(
					general: !isBaseReady ||
						minecraftVersionNeedsAttention ||
						minecraftLoaderNeedsAttention ||
						missingRequirement != null ||
						isNameTaken,
					security: isBaseReady &&
						(pnlPageSecurity.RequiredAdminPasswordMissing ||
						 pnlPageSecurity.RequiredAuthenticationTokenMissing ||
						 !string.IsNullOrWhiteSpace(serverInputError)),
					world: false,
					network: isBaseReady && portValidation.HasConflict,
					automation: scheduleNeedsAttention,
					discord: isBaseReady && !discordSettingsValid,
					install: isBaseReady &&
						(string.IsNullOrWhiteSpace(pnlPageInstall.InstallPath) ||
						 !extraArgumentsValid));

				if (!isBaseReady)
				{
					_validationMessage = !hasName && !hasGame
						? "  🔒 [REQUIRED] Enter a Server Name and select a Game Template."
						: !hasName
							? "  🔒 [REQUIRED] Enter a Server Name before this server can be saved."
							: "  🔒 [REQUIRED] Select a Game Template before this server can be saved.";
					btnSave.Enabled = false;
				}
				else if (isMinecraft &&
					!isMinecraftBedrock &&
					pnlPageGeneral.IsLoadingMinecraftMetadata)
				{
					_validationMessage =
						"  ◌ [MINECRAFT] Loading compatible versions and Java requirements...";
					btnSave.Enabled = false;
				}
				else if (isMinecraft &&
					!isMinecraftBedrock &&
					!string.IsNullOrWhiteSpace(pnlPageGeneral.MinecraftMetadataError))
				{
					_validationMessage =
						$"  ⚠️ [MINECRAFT] {pnlPageGeneral.MinecraftMetadataError}";
					btnSave.Enabled = false;
				}
				else if (isMinecraft &&
					string.IsNullOrWhiteSpace(pnlPageGeneral.GameVersion))
				{
					_validationMessage =
						"  🔒 [MINECRAFT] Select a Minecraft game version.";
					btnSave.Enabled = false;
				}
				else if (pnlPageSecurity.RequiredAdminPasswordMissing)
				{
					_validationMessage =
						"  🔒 [REQUIRED] Enter an Admin Password to protect the server administrator role.";
					btnSave.Enabled = false;
				}
				else if (pnlPageSecurity.RequiredAuthenticationTokenMissing)
				{
					_validationMessage =
						$"  🔒 [REQUIRED] Enter the required {pnlPageSecurity.AuthenticationTokenLabel} before this server can be saved.";
					btnSave.Enabled = false;
				}
				else if (!string.IsNullOrWhiteSpace(serverInputError))
				{
					_validationMessage = $"  🔒 [REQUIRED] {serverInputError}";
					btnSave.Enabled = false;
				}
				else if (minecraftLoaderNeedsAttention)
				{
					_validationMessage =
						"  🔒 [MINECRAFT] No compatible loader build is selected.";
					btnSave.Enabled = false;
				}
				else if (missingRequirement != null)
				{
					_validationMessage =
						$"  ⚠️ [REQUIREMENT] {missingRequirement.Message}";
					btnSave.Enabled = false;
				}
				else if (isNameTaken)
				{
					_validationMessage =
						$"  ⚠️ [CONFLICT] Name '{currentName}' is already used for {selectedGame}.";
					btnSave.Enabled = false;
				}
				else if (scheduleNeedsAttention)
				{
					_validationMessage =
						"  🔒 [REQUIRED] Select at least one day for the automatic restart schedule.";
					btnSave.Enabled = false;
				}
				else if (portValidation.HasConflict)
				{
					_validationMessage = portValidation.ErrorMessage;
					btnSave.Enabled = false;
				}
				else if (string.IsNullOrWhiteSpace(pnlPageInstall.InstallPath))
				{
					_validationMessage =
						"  🔒 [REQUIRED] Select an install folder or enable the default install path.";
					btnSave.Enabled = false;
				}
				else if (!extraArgumentsValid)
				{
					_validationMessage = $"  ⚠️ [LAUNCH] {extraArgumentsError}";
					btnSave.Enabled = false;
				}
				else if (!discordSettingsValid)
				{
					_validationMessage = $"  🔒 [DISCORD] {discordSettingsError}";
					btnSave.Enabled = false;
				}
				else
				{
					_validationMessage = !string.IsNullOrWhiteSpace(
						selectedDefinition?.LaunchBehavior.ReadyMessage)
						? $"  ✔ [READY] NOTE: {selectedDefinition.LaunchBehavior.ReadyMessage}"
						: _isEditMode
							? $"  ✔ [READY] Updating: {currentName}"
							: "  ✔ [READY] Configuration is valid and safe.";
					btnSave.Enabled = true;
				}

				UpdateModernStatus();
			}
			catch (Exception exception)
			{
				System.Diagnostics.Debug.WriteLine(
					$"[GATEKEEPER CRASH] {exception.Message}");
				_validationMessage =
					$"  ⚠️ [VALIDATION ERROR] Validation could not complete: {exception.Message}";
				btnSave.Enabled = false;
				UpdateNavigationAttention(
					general: true,
					security: false,
					world: false,
					network: false,
					automation: false,
					discord: false,
					install: false);
				UpdateModernStatus();
			}
		}

		private void ToggleGameSpecificFields(GameInfo? gameData)
		{
			bool isMinecraftBedrock =
				pnlPageGeneral.IsMinecraftBedrockSelected;
			pnlPageGeneral.ConfigureForGame(gameData);
			pnlPageSecurity.ConfigureForGame(gameData);
			pnlPageWorld.ConfigureForGame(gameData, isMinecraftBedrock);
			pnlPageNetwork.ConfigureForGame(
				gameData,
				isMinecraftBedrock,
				_isEditMode);

			ConfigurationSupportPresentation support =
				UserGuidance.GetConfigurationSupport(gameData);
			string portMappingSummary = GetPortMappingSummary(gameData);
			lblTemplateBehavior.Text =
				$"◇  CONFIGURATION SUPPORT: {support.Status}  •  {portMappingSummary}";
			lblTemplateBehavior.ForeColor = portMappingSummary.StartsWith(
				"Needs mapping:",
				StringComparison.Ordinal)
					? SettingsPalette.Warning
					: support.Color;
			ApplyExperienceMode();
			SyncGatekeeper();
		}

		private void btnNavSecurity_Click(object? sender, EventArgs eventArgs)
		{
			ShowSettingsPage(
				pnlPageSecurity,
				btnNavSecurity,
				"Security",
				"Manage server passwords and online-service credentials.");
		}

		internal static string GetPortMappingSummary(GameInfo? gameData)
		{
			if (gameData == null)
				return "Select a game to see its managed port mappings.";

			GameManagementCapability capabilities =
				GameFix.GetManagementCapabilities(gameData);
			List<string> missing = [];
			if ((capabilities & GameManagementCapability.Port) == 0)
				missing.Add("Game Port");
			if ((capabilities & GameManagementCapability.QueryPort) == 0)
				missing.Add("Query Port");
			if (gameData.AppPort.HasValue &&
				(capabilities & GameManagementCapability.AppPort) == 0)
			{
				missing.Add("App Port");
			}

			return missing.Count == 0
				? "All declared ports are mapped by arguments or configuration."
				: $"Needs mapping: {string.Join(", ", missing)} (arguments or configuration template).";
		}

		private void btnNavDiscord_Click(object? sender, EventArgs eventArgs)
		{
			ShowSettingsPage(
				pnlPageDiscord,
				btnNavDiscord,
				"Discord Notifications",
				"Use one master webhook or route different Synix events to multiple Discord channels.");
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			string newName = pnlPageGeneral.ServerName;
			string selectedGame = pnlPageGeneral.SelectedGame;
			if (!Core.Instance.ValidateNameAndReport(
				newName,
				selectedGame,
				_existingServer))
			{
				return;
			}

			GameInfo? masterData = GameDatabase.GetGame(selectedGame);
			if (!pnlPageSecurity.TryValidate(
					newName,
					pnlPageNetwork.RconPassword,
					out string serverInputError))
			{
				LocalizedMessageBox.Show(
					serverInputError,
					"Server Settings Need Attention",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				btnNavSecurity.PerformClick();
				pnlPageSecurity.FocusFirstRequiredInput();
				return;
			}

			int gamePort = pnlPageNetwork.GamePort;
			int queryPort = pnlPageNetwork.QueryPort;
			int rconPort = pnlPageNetwork.RconPort;
			int? appPort = pnlPageNetwork.AppPort;
			if (!Core.Instance.ValidatePortsAndReport(
				_existingServer,
				gamePort,
				queryPort,
				rconPort,
				pnlPageNetwork.RconEnabled,
				appPort ?? 0,
				pnlPageNetwork.AppPortEnabled,
				selectedGame,
				pnlPageNetwork.GamePortEnabled,
				pnlPageNetwork.QueryPortEnabled))
			{
				return;
			}
			if (!pnlPageInstall.TryValidateExtraArguments(
					out string extraArgumentsError))
			{
				LocalizedMessageBox.Show(
					extraArgumentsError,
					"Extra Arguments Blocked",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				pnlPageInstall.FocusExtraArguments();
				return;
			}
			if (!discordSettingsPage.TryGetSettings(
				out DiscordSettingsSnapshot discordSettings,
				out string discordSettingsError))
			{
				LocalizedMessageBox.Show(
					discordSettingsError,
					"Discord Settings Need Attention",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				btnNavDiscord.PerformClick();
				return;
			}

			int worldSize = pnlPageWorld.WorldSize;
			string worldName = pnlPageGeneral.WorldName;
			if (ServerSettingsWorldPage.IsSevenDaysToDie(masterData))
			{
				worldName = SevenDaysToDieConfiguration.NormalizeWorldName(worldName);
				worldSize = SevenDaysToDieConfiguration.NormalizeWorldSize(worldSize);
			}
			bool isMinecraft = pnlPageGeneral.IsMinecraftSelected;
			bool isMinecraftBedrock =
				pnlPageGeneral.IsMinecraftBedrockSelected;
			bool supportsServerFramework =
				masterData?.SupportedServerFrameworks.Count > 0;
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
				DataSchemaVersion = ServerDataMigrator.CurrentVersion,
				Game = selectedGame,
				SteamAccountName = steamAccountName,
				ServerName = newName,
				Port = gamePort,
				QueryPort = queryPort,
				RconPort = rconPort,
				AppPort = appPort,
				Password = pnlPageSecurity.ServerPassword,
				AdminPassword = pnlPageSecurity.AdminPassword,
				AuthenticationToken = pnlPageSecurity.AuthenticationToken,
				InviteCode = pnlPageSecurity.InviteCode,
				MaxPlayers = pnlPageGeneral.MaximumPlayers,
				WorldName = worldName,
				GameMode = isMinecraft
					? MinecraftControlProfile.NormalizeGameMode(
						pnlPageGeneral.GameMode)
					: pnlPageGeneral.GameMode,
				CrossplayEnabled = pnlPageGeneral.CrossplayEnabled,
				WorldSeed = pnlPageWorld.WorldSeed,
				WorldSize = worldSize,
				ExtraArgs = pnlPageInstall.ExtraArguments,
				IsDefaultPath = pnlPageInstall.UseDefaultPath,
				UpdateOnStart = pnlPageAutomation.UpdateOnStart,
				EnableRcon = !isMinecraftBedrock && pnlPageNetwork.RconEnabled,
				RconPassword = isMinecraftBedrock
					? string.Empty
					: pnlPageNetwork.RconPassword,
				InstallPath = pnlPageInstall.InstallPath.Trim(),
				MaxRam = pnlPageGeneral.MaximumRam,
				GameVersion = pnlPageGeneral.GameVersion,
				MinecraftEdition = isMinecraft
					? pnlPageGeneral.MinecraftEdition
					: MinecraftControlProfile.JavaEdition,
				MinecraftLoader = isMinecraft && !isMinecraftBedrock
					? pnlPageGeneral.MinecraftLoader
					: MinecraftMetadataService.VanillaLoader,
				MinecraftLoaderVersion = isMinecraft && !isMinecraftBedrock
					? pnlPageGeneral.MinecraftLoaderVersion
					: "Official",
				EnableMinecraftManagementProtocol =
					isMinecraft &&
					!isMinecraftBedrock &&
					(_existingServer?.EnableMinecraftManagementProtocol ?? true),
				MinecraftManagementPort = isMinecraft && !isMinecraftBedrock
					? _existingServer?.MinecraftManagementPort ?? 0
					: 0,
				ServerFramework = supportsServerFramework
					? pnlPageGeneral.SelectedRuntime
					: "Vanilla",
				ServerFrameworkVersion = supportsServerFramework &&
					string.Equals(
						_existingServer?.ServerFramework,
						pnlPageGeneral.SelectedRuntime,
						StringComparison.OrdinalIgnoreCase)
						? _existingServer?.ServerFrameworkVersion ?? "Official"
						: "Official",
				RequiredJavaVersion = isMinecraft && !isMinecraftBedrock
					? pnlPageGeneral.ResolvedMinecraftJavaVersion
					: 0,
				IsScheduledRestartEnabled =
					pnlPageAutomation.ScheduleEnabled,
				RestartTime = pnlPageAutomation.SelectedTime,
				RestartDays = pnlPageAutomation.SelectedDays,
				SmartMaintenanceEnabled =
					pnlPageAutomation.SmartMaintenanceEnabled,
				MaintenanceWaitForPlayers =
					pnlPageAutomation.WaitForPlayers,
				MaintenanceMaximumDelayMinutes =
					pnlPageAutomation.MaximumDelayMinutes,
				MaintenanceBackupBeforeRestart =
					pnlPageAutomation.BackupBeforeRestart,
				MaintenanceUpdateBeforeRestart =
					pnlPageAutomation.UpdateBeforeRestart,
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
				Status = _existingServer?.Status ??
					StatusManager.GetStatus(ServerState.Stopped),
				BackupOnStart = pnlPageAutomation.BackupOnStart
			};

			if (MinecraftControlProfile.IsJava(NewServer))
				MinecraftControlProfile.EnsureDefaults(
					NewServer,
					ServerRegistry.Servers);
			if (!IsGameServerConfigSafe(NewServer))
			{
				LocalizedMessageBox.Show(
					"Security Alert: One of your inputs contains illegal characters.",
					"Input Blocked",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				return;
			}

			try
			{
				Core.SetServerSecrets(
					NewServer,
					new SynixServerSecrets(
						new SynixServerPasswords(
							pnlPageSecurity.ServerPassword,
							pnlPageSecurity.AdminPassword,
							pnlPageNetwork.RconPassword,
							pnlPageSecurity.AuthenticationToken),
						discordSettings.MasterWebhook));
				Core.SetDiscordWebhookRoutes(NewServer, discordSettings.Routes);

				if (_isEditMode && _existingServer != null)
				{
					GameServer? existing = ServerRegistry.Servers.FirstOrDefault(
						server => server.ServerName == _existingServer.ServerName);
					if (existing != null)
					{
						NewServer.IsFirstBoot = false;
						int index = ServerRegistry.Servers.IndexOf(existing);
						ServerRegistry.Servers[index] = NewServer;
					}
				}
				else
				{
					ServerRegistry.Servers.Add(NewServer);
				}

				if (masterData != null)
				{
					string fullExecutablePath =
						GameLaunchCommandBuilder.ResolveExecutablePath(
							NewServer,
							masterData);
					string iconPath = Core.GetLocalServerIcon(
						NewServer.Game,
						fullExecutablePath);
					if (File.Exists(iconPath))
					{
						if (!ServerIconCache.Icons.TryGetValue(
								NewServer.Game,
								out Image? cachedIcon))
						{
							using MemoryStream stream =
								new(File.ReadAllBytes(iconPath));
							using Image sourceImage = Image.FromStream(stream);
							cachedIcon = new Bitmap(sourceImage);
							ServerIconCache.Icons[NewServer.Game] = cachedIcon;
						}
						NewServer.DisplayIcon = cachedIcon;
					}
				}

				FileHandler.SaveServers();
				DialogResult = DialogResult.OK;
				Close();
			}
			catch (Exception exception)
			{
				PlainEnglishErrorDialog.ShowError(
					this,
					"save the server settings",
					exception.ToString());
			}
		}

		private async void cmbGame_SelectedIndexChanged(
			object? sender,
			EventArgs eventArgs)
		{
			if (isPrivacyLoading)
				return;

			if (pnlPageGeneral.HasSelectedGame)
			{
				GameInfo? gameData =
					GameDatabase.GetGame(pnlPageGeneral.SelectedGame);
				if (gameData != null)
				{
					ToggleGameSpecificFields(gameData);
					pnlPageWorld.ApplyDefaultWorldSize(gameData);
					pnlPageNetwork.ApplyDefaultPorts(gameData);
					pnlPageGeneral.PopulateMaps(
						gameData,
						gameData.Maps?.FirstOrDefault() ?? string.Empty);
					pnlPageGeneral.PopulateGameModes(
						gameData,
						gameData.GameModes?.FirstOrDefault() ?? "PVE");
					await pnlPageGeneral.PopulateVersionsAsync(
						gameData,
						_existingServer?.GameVersion ?? "latest");
					if (pnlPageGeneral.IsMinecraftSelected)
					{
						await pnlPageGeneral.RefreshMinecraftRuntimeAsync(
							_existingServer?.MinecraftLoader ??
								MinecraftMetadataService.VanillaLoader,
							_existingServer?.MinecraftLoaderVersion ?? "Official");
					}
				}
			}
			else
			{
				ToggleGameSpecificFields(null);
			}

			pnlPageGeneral.RefreshCompatibilityVerification(
				pnlPageGeneral.HasSelectedGame
					? pnlPageGeneral.SelectedGame
					: null);
			SyncGatekeeper();
		}

		private void btnCancel_Click(object sender, EventArgs e) { this.DialogResult = DialogResult.Cancel; this.Close(); }
	}
}
