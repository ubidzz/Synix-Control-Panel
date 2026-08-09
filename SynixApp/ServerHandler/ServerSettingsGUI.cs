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
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using Synix_Control_Panel.SynixEngine;
using System.Runtime.InteropServices;
using static Synix_Control_Panel.SynixEngine.Core;
using System.Management;
using System.Runtime.Intrinsics.X86;

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
		private System.Windows.Forms.Timer debounceTimer;
		private bool _PrivacyMode = false;

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);
		private const int EM_SETCUEBANNER = 0x1501;

		public ServerSettingsGUI(GameServer? server = null)
		{
			InitializeComponent();
			isPrivacyLoading = true;
			_existingServer = server;
			_isEditMode = server != null;
			_PrivacyMode = Properties.Settings.Default.PrivacyMode;

			// UI Styling
			UIStyleHelper.StyleWarningLabel(WarningLabel);
			UIStyleHelper.InitializeToggles(this);
			UIStyleHelper.StyleWarningLabel(lblConfigWarning);
			WireUpGatekeeperEvents();

			// Tags for Pill logic
			chkDefaultPath.Tag = "Default Folder";
			chkEnableSchedule.Tag = "Activate Scheduler";
			chkUpdateOnStart.Tag = "Update on Start";
			chkEnableRcon.Tag = "RCON";
			chkBackupOnStart.Tag = "Backup on Start";
			chkEnableDiscord.Tag = "Discord Alerts";

			SendMessage(txtDiscordWebhook.Handle, EM_SETCUEBANNER, 0, "Paste Discord Webhook URL here...");

			// Game List Setup
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

			isPrivacyLoading = false;
			lblConfigWarning.Visible = false;
			PrivacyMode();
			SyncGatekeeper();
		}

		private void PrivacyMode()
		{
			if (_PrivacyMode)
			{
				// Mask the textboxes with system password characters (dots)
				txtPassword.UseSystemPasswordChar = true;
				txtAdminPassword.UseSystemPasswordChar = true;
				txtRconPassword.UseSystemPasswordChar = true;
				txtDiscordWebhook.UseSystemPasswordChar = true;

			}
			else
			{
				// Reveal text if Streamer Mode is off
				txtPassword.UseSystemPasswordChar = false;
				txtAdminPassword.UseSystemPasswordChar = false;
				txtRconPassword.UseSystemPasswordChar = false;
				txtDiscordWebhook.UseSystemPasswordChar = false;
			}
		}

		private void LoadExistingServerData()
		{
			if (_existingServer == null) return;
			isPrivacyLoading = true;

			txtName.Text = _existingServer.ServerName ?? "";
			int gameIndex = cmbGame.FindStringExact(_existingServer.Game);
			if (gameIndex != -1) cmbGame.SelectedIndex = gameIndex;

			txtPassword.Text = _existingServer.Password ?? "";
			txtAdminPassword.Text = _existingServer.AdminPassword ?? "";
			chkEnableDiscord.Checked = _existingServer.IsDiscordAlertEnabled;
			txtDiscordWebhook.Text = _existingServer.DiscordWebhook ?? "";
			txtDiscordWebhook.Enabled = chkEnableDiscord.Checked;

			numPort.Value = Math.Clamp((decimal)_existingServer.Port, numPort.Minimum, numPort.Maximum);
			numQueryPort.Value = Math.Clamp((decimal)_existingServer.QueryPort, numQueryPort.Minimum, numQueryPort.Maximum);
			if (numAppPort != null) numAppPort.Value = Math.Clamp((decimal)(_existingServer.AppPort ?? numAppPort.Minimum), numAppPort.Minimum, numAppPort.Maximum);

			numMaxPlayers.Value = Math.Clamp((decimal)_existingServer.MaxPlayers, numMaxPlayers.Minimum, numMaxPlayers.Maximum);
			txtInstallPath.Text = _existingServer.InstallPath ?? "";
			chkDefaultPath.Checked = _existingServer.IsDefaultPath;
			txtExtraArgs.Text = _existingServer.ExtraArgs ?? "";
			txtWorldSeed.Text = _existingServer.WorldSeed ?? "12345";
			numWorldSize.Value = Math.Clamp((decimal)_existingServer.WorldSize, numWorldSize.Minimum, numWorldSize.Maximum);

			chkUpdateOnStart.Checked = _existingServer.UpdateOnStart;
			chkEnableRcon.Checked = _existingServer.EnableRcon;
			numRconPort.Value = Math.Clamp((decimal)_existingServer.RconPort, numRconPort.Minimum, numRconPort.Maximum);
			txtRconPassword.Text = _existingServer.RconPassword ?? "";
			chkEnableSchedule.Checked = _existingServer.IsScheduledRestartEnabled;
			if (_existingServer.RestartDays != null) _selectedDays = (bool[])_existingServer.RestartDays.Clone();
			_selectedTime = _existingServer.RestartTime ?? "04:00";
			chkBackupOnStart.Checked = _existingServer.BackupOnStart;
			cmbGameVersion.Text = _existingServer.GameVersion ?? "latest";
			numRam.Value = Math.Clamp((decimal)_existingServer.MaxRam, numRam.Minimum, numRam.Maximum);

			var gameData = GameDatabase.GetGame(_existingServer.Game);
			if (gameData != null)
			{
				PopulateMaps(gameData, _existingServer.WorldName ?? "");
				PopulateGameModes(gameData, _existingServer.GameMode ?? "PVE");
				ToggleGameSpecificFields(gameData);
			}

			cmbGame.Enabled = false;
			isPrivacyLoading = false;
		}

		private void SyncGatekeeper()
		{
			if (isPrivacyLoading) return;

			try
			{
				string currentName = txtName?.Text?.Trim() ?? "";
				bool hasName = !string.IsNullOrWhiteSpace(currentName);
				bool hasGame = cmbGame != null && cmbGame.SelectedIndex > 0;
				string selectedGame = hasGame ? cmbGame.Text : "";
				bool isBaseReady = hasName && hasGame;
				bool CanUnlock(Control c) => hasGame && c.Tag?.ToString() == "Required";

				// --- DUNE: AWAKENING HARDWARE & OS CHECKS ---
				bool isDuneAwakening = selectedGame.Equals("Dune: Awakening", StringComparison.OrdinalIgnoreCase);
				bool virtMissing = false;
				string missingTechName = "";
				bool isHomeEdition = false;
				bool hyperVMissing = false;
				bool avx2Missing = false;
				bool ramMissing = false;
				double sysRam = 0;

				if (isDuneAwakening)
				{
					var virtData = CheckVirtualizationStatus();
					virtMissing = !virtData.IsEnabled;
					missingTechName = virtData.TechName;

					isHomeEdition = !IsWindowsProOrBetter();
					hyperVMissing = !IsHypervisorPresent();
					avx2Missing = !Avx2.IsSupported;

					sysRam = GetSystemRamGB();
					ramMissing = sysRam < 23.0; // 24GB minimum, allowing a tiny margin for hardware reserved RAM
				}

				txtPassword.Enabled = CanUnlock(txtPassword);
				txtAdminPassword.Enabled = CanUnlock(txtAdminPassword);
				txtWorldSeed.Enabled = CanUnlock(txtWorldSeed);
				cmbCompetitive.Enabled = CanUnlock(cmbCompetitive);
				numMaxPlayers.Enabled = CanUnlock(numMaxPlayers);
				numQueryPort.Enabled = CanUnlock(numQueryPort);
				cmbWorldName.Enabled = CanUnlock(cmbWorldName);
				numWorldSize.Enabled = CanUnlock(numWorldSize);
				cmbGameVersion.Enabled = CanUnlock(cmbGameVersion);
				numRam.Enabled = CanUnlock(numRam);

				if (numAppPort != null)
					numAppPort.Tag = CanUnlock(numAppPort) ? "Required" : "Disabled";

				numPort.Enabled = hasGame;
				if (numAppPort != null) numAppPort.Enabled = CanUnlock(numAppPort);

				chkEnableRcon.Enabled = CanUnlock(chkEnableRcon);
				bool rconActive = chkEnableRcon.Enabled && chkEnableRcon.Checked;
				numRconPort.Enabled = rconActive;
				txtRconPassword.Enabled = rconActive;

				chkUpdateOnStart.Enabled = isBaseReady;
				chkBackupOnStart.Enabled = isBaseReady;
				chkEnableSchedule.Enabled = isBaseReady;
				if (btnEditSchedule != null) btnEditSchedule.Enabled = isBaseReady && chkEnableSchedule.Checked;

				chkEnableDiscord.Enabled = isBaseReady;
				txtDiscordWebhook.Enabled = isBaseReady && chkEnableDiscord.Checked;

				if (_isEditMode)
				{
					chkDefaultPath.Enabled = false;
					btnBrowse.Enabled = false;
					txtInstallPath.Enabled = false;
				}
				else
				{
					chkDefaultPath.Enabled = isBaseReady;
					bool manualMode = isBaseReady && !chkDefaultPath.Checked;
					btnBrowse.Enabled = manualMode;
					txtInstallPath.Enabled = manualMode;
				}

				if (!_isEditMode && isBaseReady && chkDefaultPath.Checked)
				{
					string safeName = Core.Instance.GetSafeName(currentName);
					string safeFolderName = Core.Instance.GetSafeName(selectedGame);
					txtInstallPath.Text = $@"C:\Synix\Games\{safeFolderName}\{safeName}";
				}

				GameInfo? selectedGameData = hasGame ? GameDatabase.GetGame(selectedGame) : null;

				bool usesQueryPort = selectedGameData?.RequiredArgs?.Contains( "{query}", StringComparison.OrdinalIgnoreCase) == true;

				int gPort = (int)numPort.Value;
				int qPort = usesQueryPort ? (int)numQueryPort.Value : 0;
				int aPort = (numAppPort != null && numAppPort.Enabled) ? (int)numAppPort.Value : 0;
				int rPort = rconActive ? (int)numRconPort.Value : 0;

				string? gOwner = Core.Instance.GetPortCollisionOwner(gPort, _existingServer);
				bool gOS = Core.Instance.IsPortInUseLocally(gPort);

				string? qOwner = (qPort > 0) ? Core.Instance.GetPortCollisionOwner(qPort, _existingServer) : null;
				bool qOS = (qPort > 0) && Core.Instance.IsPortInUseLocally(qPort);

				string? rOwner = (rPort > 0) ? Core.Instance.GetPortCollisionOwner(rPort, _existingServer) : null;
				bool rOS = (rPort > 0) && Core.Instance.IsPortInUseLocally(rPort);

				string? aOwner = (aPort > 0) ? Core.Instance.GetPortCollisionOwner(aPort, _existingServer) : null;
				bool aOS = (aPort > 0) && Core.Instance.IsPortInUseLocally(aPort);

				bool isNameTaken = MainGUI.serverList.Any(s =>
					s != _existingServer &&
					s.Game.Equals(selectedGame, StringComparison.OrdinalIgnoreCase) &&
					s.ServerName.Equals(currentName, StringComparison.OrdinalIgnoreCase));

				if (!isBaseReady)
				{
					WarningLabel.Text = "  🔒 [LOCKED] Required: Server Name and Game Template selection.";
					WarningLabel.ForeColor = Color.Gold;
					WarningLabel.BackColor = Color.FromArgb(60, 45, 0);
					btnSave.Enabled = false;
				}
				// --- DUNE CHECK: Minimum RAM ---
				else if (ramMissing)
				{
					WarningLabel.Text = $"  ⚠️ [HARDWARE] 'Dune: Awakening' requires at least 24GB of RAM (Detected: {sysRam:0.0} GB).";
					WarningLabel.ForeColor = Color.Red;
					WarningLabel.BackColor = Color.FromArgb(60, 20, 20);
					btnSave.Enabled = false;
				}
				// --- DUNE CHECK: AVX2 Processor Support ---
				else if (avx2Missing)
				{
					WarningLabel.Text = "  ⚠️ [HARDWARE] 'Dune: Awakening' strictly requires a CPU with AVX2 support.";
					WarningLabel.ForeColor = Color.Red;
					WarningLabel.BackColor = Color.FromArgb(60, 20, 20);
					btnSave.Enabled = false;
				}
				// --- DUNE CHECK: Windows Pro/Enterprise ---
				else if (isHomeEdition)
				{
					WarningLabel.Text = "  ⚠️ [OS CHECK] Windows Pro/Enterprise is required. Home editions do not support Hyper-V.";
					WarningLabel.ForeColor = Color.Red;
					WarningLabel.BackColor = Color.FromArgb(60, 20, 20);
					btnSave.Enabled = false;
				}
				// --- DUNE CHECK: BIOS Virtualization ---
				else if (virtMissing)
				{
					WarningLabel.Text = $"  ⚠️ [HARDWARE] 'Dune: Awakening' requires {missingTechName} to be enabled in your PC's BIOS.";
					WarningLabel.ForeColor = Color.Red;
					WarningLabel.BackColor = Color.FromArgb(60, 20, 20);
					btnSave.Enabled = false;
				}
				// --- DUNE CHECK: Hyper-V Enabled in Windows ---
				else if (hyperVMissing)
				{
					WarningLabel.Text = "  ⚠️ [SYSTEM] Windows Hyper-V is disabled. Please turn it on in 'Windows Features'.";
					WarningLabel.ForeColor = Color.Red;
					WarningLabel.BackColor = Color.FromArgb(60, 20, 20);
					btnSave.Enabled = false;
				}
				// ------------------------------------------
				else if (isNameTaken)
				{
					WarningLabel.Text = $"  ⚠️ [CONFLICT] Name '{currentName}' is already used for {selectedGame}.";
					WarningLabel.ForeColor = Color.Red;
					WarningLabel.BackColor = Color.FromArgb(60, 20, 20);
					btnSave.Enabled = false;
				}
				else if (gOwner != null || gOS)
				{
					WarningLabel.Text = $"  ⚠️ [CONFLICT] Game Port {gPort} is blocked by: {gOwner ?? "System Process"}";
					WarningLabel.ForeColor = Color.Red;
					WarningLabel.BackColor = Color.FromArgb(60, 20, 20);
					btnSave.Enabled = false;
				}
				else if (qOwner != null || qOS)
				{
					WarningLabel.Text = $"  ⚠️ [CONFLICT] Query Port {qPort} is blocked by: {qOwner ?? "System Process"}";
					WarningLabel.ForeColor = Color.Red;
					WarningLabel.BackColor = Color.FromArgb(60, 20, 20);
					btnSave.Enabled = false;
				}
				else if (rOwner != null || rOS)
				{
					WarningLabel.Text = $"  ⚠️ [CONFLICT] RCON Port {rPort} is blocked by: {rOwner ?? "System Process"}";
					WarningLabel.ForeColor = Color.Red;
					WarningLabel.BackColor = Color.FromArgb(60, 20, 20);
					btnSave.Enabled = false;
				}
				else if (aOwner != null || aOS)
				{
					WarningLabel.Text = $"  ⚠️ [CONFLICT] App Port {aPort} is blocked by: {aOwner ?? "System Process"}";
					WarningLabel.ForeColor = Color.Red;
					WarningLabel.BackColor = Color.FromArgb(60, 20, 20);
					btnSave.Enabled = false;
				}
				else
				{
					if (isDuneAwakening)
					{
						WarningLabel.Text = "  ✔ [READY] NOTE: Have your Self-Host Token ready for the battlegroup.bat prompt.";
						WarningLabel.ForeColor = Color.Orange;
						WarningLabel.BackColor = Color.FromArgb(20, 50, 20);
					}
					else
					{
						WarningLabel.Text = _isEditMode ? $"  ✔ [READY] Updating: {currentName}" : "  ✔ [READY] Configuration is valid and safe.";
						WarningLabel.ForeColor = Color.SpringGreen;
						WarningLabel.BackColor = Color.FromArgb(20, 50, 20);
					}

					btnSave.Enabled = !string.IsNullOrWhiteSpace(txtInstallPath.Text);
				}

				WarningLabel.Invalidate();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[GATEKEEPER CRASH] {ex.Message}");
			}
		}

		private void ToggleGameSpecificFields(GameInfo? gameData)
		{
			var controls = new Control[] { txtPassword, txtAdminPassword, txtWorldSeed, cmbCompetitive, numAppPort, numMaxPlayers, numQueryPort, cmbWorldName, chkEnableRcon };
			if (gameData == null)
			{
				foreach (var c in controls) if (c != null) c.Tag = "Disabled";
				lblConfigWarning.Visible = false;

				SetupManagedPlaceholder(txtPassword, "Select a game...");
				SetupManagedPlaceholder(txtAdminPassword, "Select a game...");
				SetupManagedPlaceholder(txtWorldSeed, "Select a game...");
			}
			else
			{
				string args = (gameData.RequiredArgs ?? "").ToLower();
				string rconTemp = (gameData.RconSyntax ?? "").ToLower();

				// Password Field
				bool needsPass = args.Contains("{pass}");
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
					txtPassword.ForeColor = SystemColors.WindowText;
					txtPassword.GotFocus -= Placeholder_GotFocus;
					txtPassword.LostFocus -= Placeholder_LostFocus;
				}

				// Admin Password Field
				bool needsAdminPass = args.Contains("{adminpass}");
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
					txtAdminPassword.ForeColor = SystemColors.WindowText;
					txtAdminPassword.GotFocus -= Placeholder_GotFocus;
					txtAdminPassword.LostFocus -= Placeholder_LostFocus;
				}

				// World Seed Field
				bool needsSeed = args.Contains("{seed}");
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
					txtWorldSeed.ForeColor = SystemColors.WindowText;
					txtWorldSeed.GotFocus -= Placeholder_GotFocus;
					txtWorldSeed.LostFocus -= Placeholder_LostFocus;
				}

				cmbCompetitive.Tag = (args.Contains("{mode}") || (gameData.GameModes != null && gameData.GameModes.Count > 0)) ? "Required" : "Disabled";
				numMaxPlayers.Tag = args.Contains("{maxplayers}") ? "Required" : "Disabled";
				numQueryPort.Tag = args.Contains("{query}") ? "Required" : "Disabled";
				cmbWorldName.Tag = args.Contains("{map}") ? "Required" : "Disabled";
				if (numAppPort != null) numAppPort.Tag = args.Contains("{app_port}") ? "Required" : "Disabled";
				chkEnableRcon.Tag = (args.Contains("{rcon}") || rconTemp.Contains("{rcon_port}")) ? "Required" : "Disabled";
				numWorldSize.Tag = args.Contains("{world_size}") ? "Required" : "Disabled";
				cmbGameVersion.Tag = gameData.Game == "Minecraft Java" ? "Required" : "Disabled";
				numRam.Tag = args.Contains("{ram}") ? "Required" : "Disabled";

				if (gameData.NeedsConfigWarning == true)
				{
					lblConfigWarning.Visible = true;
				}
				else
				{
					lblConfigWarning.Visible = false;
				}
			}
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
				txt.ForeColor = SystemColors.WindowText;
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

		private void WireUpGatekeeperEvents()
		{
			if (debounceTimer == null) { debounceTimer = new System.Windows.Forms.Timer(); debounceTimer.Interval = 300; debounceTimer.Tick += (s, e) => { debounceTimer.Stop(); SyncGatekeeper(); }; }
			Action trigger = () => { debounceTimer.Stop(); debounceTimer.Start(); };
			txtName.TextChanged += (s, e) => trigger();
			cmbGame.SelectedIndexChanged += (s, e) => trigger();
			numPort.ValueChanged += (s, e) => trigger();
			numQueryPort.ValueChanged += (s, e) => trigger();
			numAppPort.ValueChanged += (s, e) => trigger();
			numRconPort.ValueChanged += (s, e) => trigger();
			chkEnableRcon.CheckedChanged += (s, e) => trigger();
			chkDefaultPath.CheckedChanged += (s, e) => trigger();
			numWorldSize.ValueChanged += (s, e) => trigger();
			cmbGameVersion.SelectedIndexChanged += (s, e) => trigger();
			numRam.ValueChanged += (s, e) => trigger();
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			string newName = txtName.Text.Trim();
			string selectedGame = cmbGame.Text;
			if (!Core.Instance.ValidateNameAndReport(newName, selectedGame, _existingServer)) return;

			GameInfo? selectedGameData = GameDatabase.GetGame(selectedGame);
			bool usesQueryPort = selectedGameData?.RequiredArgs?.Contains("{query}", StringComparison.OrdinalIgnoreCase) == true;

			int gPort = (int)numPort.Value;
			int qPort = usesQueryPort ? (int)numQueryPort.Value : 0;
			int rPort = (int)numRconPort.Value;
			int wSize = (int)numWorldSize.Value;

			int? aPort = numAppPort.Enabled ? (int)numAppPort.Value : (int?)null;
			if (!Core.Instance.ValidatePortsAndReport(_existingServer, gPort, qPort, rPort, chkEnableRcon.Checked, aPort ?? 0, numAppPort.Enabled, selectedGame)) return;
			string newPath = txtInstallPath.Text.Trim();
			NewServer = new GameServer { Game = selectedGame, ServerName = newName, Port = gPort, QueryPort = qPort, RconPort = rPort, AppPort = aPort, Password = txtPassword.Text, AdminPassword = txtAdminPassword.Text, MaxPlayers = (int)numMaxPlayers.Value, WorldName = cmbWorldName.Text, GameMode = cmbCompetitive.Text, WorldSeed = txtWorldSeed.Text.Trim(), WorldSize = wSize, ExtraArgs = txtExtraArgs.Text, IsDefaultPath = chkDefaultPath.Checked, UpdateOnStart = chkUpdateOnStart.Checked, EnableRcon = chkEnableRcon.Checked, RconPassword = txtRconPassword.Text, InstallPath = newPath, MaxRam = (int)numRam.Value, GameVersion = cmbGameVersion.Text.Trim(),  IsScheduledRestartEnabled = chkEnableSchedule.Checked, RestartTime = _selectedTime, RestartDays = (bool[])_selectedDays.Clone(), IsDiscordAlertEnabled = chkEnableDiscord.Checked, DiscordWebhook = txtDiscordWebhook.Text.Trim(), Status = _existingServer?.Status ?? StatusManager.GetStatus(ServerState.Stopped), BackupOnStart = chkBackupOnStart.Checked };

			if (!IsGameServerConfigSafe(NewServer))
			{
				MessageBox.Show("Security Alert: One of your inputs contains illegal characters.",
								"Input Blocked", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			try
			{
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

				var masterData = GameDatabase.GetGame(NewServer.Game);
				if (masterData != null)
				{
					NewServer.AppID = masterData.AppID;
					NewServer.ExeName = masterData.ExeName;

					string fullExePath = System.IO.Path.Combine(NewServer.InstallPath, NewServer.ExeName);
					string iconPath = Synix_Control_Panel.SynixEngine.Core.GetLocalServerIcon(NewServer.Game, fullExePath);

					if (System.IO.File.Exists(iconPath))
					{
						NewServer.DisplayIcon = System.Drawing.Image.FromFile(iconPath);
					}
				}

				FileHandler.SaveServers(); this.DialogResult = DialogResult.OK; this.Close();
			}
			catch (Exception ex) { MessageBox.Show(ex.Message); }
		}

		// ====================================================================
		// HARDWARE & OS GATEKEEPER CHECKS
		// ====================================================================
		private (bool IsEnabled, string TechName) CheckVirtualizationStatus()
		{
			bool isEnabled = true;
			string techName = "Hardware Virtualization";

			try
			{
				using (var searcher = new ManagementObjectSearcher("Select VirtualizationFirmwareEnabled, Manufacturer FROM Win32_Processor"))
				{
					foreach (var obj in searcher.Get())
					{
						if (obj["Manufacturer"] != null)
						{
							string manufacturer = obj["Manufacturer"].ToString();
							if (manufacturer.Contains("Intel", StringComparison.OrdinalIgnoreCase))
								techName = "Intel VT-x";
							else if (manufacturer.Contains("AMD", StringComparison.OrdinalIgnoreCase))
								techName = "AMD-V (SVM)";
						}

						if (obj["VirtualizationFirmwareEnabled"] != null)
							isEnabled = (bool)obj["VirtualizationFirmwareEnabled"];

						break;
					}
				}
			}
			catch { }
			return (isEnabled, techName);
		}

		private bool IsWindowsProOrBetter()
		{
			try
			{
				using (var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem"))
				using (var collection = searcher.Get())
				{
					foreach (ManagementObject obj in collection)
					{
						string caption = obj["Caption"]?.ToString() ?? "";
						obj.Dispose();

						if (caption.Contains("Home", StringComparison.OrdinalIgnoreCase)) return false;
					}
				}
			}
			catch { }
			return true;
		}

		private bool IsHypervisorPresent()
		{
			try
			{
				using (var searcher = new ManagementObjectSearcher("SELECT HypervisorPresent FROM Win32_ComputerSystem"))
				{
					foreach (var obj in searcher.Get())
					{
						if (obj["HypervisorPresent"] != null) return (bool)obj["HypervisorPresent"];
					}
				}
			}
			catch { }
			return true;
		}

		private double GetSystemRamGB()
		{
			try
			{
				using (var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
				{
					foreach (var obj in searcher.Get())
					{
						if (obj["TotalPhysicalMemory"] != null)
						{
							ulong bytes = Convert.ToUInt64(obj["TotalPhysicalMemory"]);
							return bytes / (1024.0 * 1024.0 * 1024.0);
						}
					}
				}
			}
			catch { }
			return 999.0; // Default to pass on WMI error so user isn't locked out
		}

		private async void cmbGame_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (isPrivacyLoading) return;
			if (cmbGame.SelectedIndex > 0)
			{
				var gameData = GameDatabase.GetGame(cmbGame.SelectedItem.ToString());
				if (gameData != null)
				{
					numPort.Value = Math.Clamp((decimal)gameData.Port, numPort.Minimum, numPort.Maximum);
					numQueryPort.Value = Math.Clamp((decimal)gameData.QueryPort, numQueryPort.Minimum, numQueryPort.Maximum);
					PopulateMaps(gameData, gameData.Maps?.FirstOrDefault() ?? "");
					PopulateGameModes(gameData, "PVE");

					await PopulateVersionsAsync(gameData, _existingServer?.GameVersion ?? "latest");

					ToggleGameSpecificFields(gameData);
				}
			}
			else ToggleGameSpecificFields(null);
			SyncGatekeeper();
		}

		private async Task PopulateVersionsAsync(GameInfo gameData, string selectedVersion)
		{
			cmbGameVersion.Items.Clear();
			cmbGameVersion.Items.Add("latest");

			// If the game is Minecraft, ping the Mojang API to fill the dropdown!
			if (gameData.Game.StartsWith("Minecraft", StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					using HttpClient client = new HttpClient();
					string manifestJson = await client.GetStringAsync("https://launchermeta.mojang.com/mc/game/version_manifest.json");
					var manifestNode = System.Text.Json.Nodes.JsonNode.Parse(manifestJson);
					var versionsArray = manifestNode?["versions"]?.AsArray();

					if (versionsArray != null)
					{
						foreach (var version in versionsArray)
						{
							// Only add stable releases to the dropdown
							if (version?["type"]?.ToString() == "release")
							{
								string id = version?["id"]?.ToString() ?? "";
								if (!string.IsNullOrEmpty(id)) cmbGameVersion.Items.Add(id);
							}
						}
					}
				}
				catch
				{
					// If the API fails (no internet), it gracefully falls back to just "latest"
				}
			}

			// Apply the previously saved version, or default to latest
			if (cmbGameVersion.Items.Contains(selectedVersion))
				cmbGameVersion.SelectedItem = selectedVersion;
			else if (cmbGameVersion.Items.Count > 0)
				cmbGameVersion.SelectedIndex = 0;
		}

		private void btnBrowse_Click(object sender, EventArgs e) { using var fbd = new FolderBrowserDialog(); if (fbd.ShowDialog() == DialogResult.OK) { txtInstallPath.Text = fbd.SelectedPath; SyncGatekeeper(); } }
		private void chkDefaultPath_CheckedChanged(object sender, EventArgs e) => SyncGatekeeper();
		private void txtInstallPath_TextChanged(object sender, EventArgs e) => SyncGatekeeper();
		private void chkEnableRcon_CheckedChanged(object sender, EventArgs e) { if (isPrivacyLoading) return; bool active = chkEnableRcon.Checked; numRconPort.Enabled = txtRconPassword.Enabled = active; SyncGatekeeper(); }
		private void chkEnableSchedule_CheckedChanged(object sender, EventArgs e) { if (isPrivacyLoading) return; if (btnEditSchedule != null) btnEditSchedule.Enabled = chkEnableSchedule.Checked; SyncGatekeeper(); }
		private void txtWorldSeed_KeyPress(object sender, KeyPressEventArgs e) { if (cmbGame.Text == "Rust" && !char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; }
		private void btnViewArgs_Click(object sender, EventArgs e) { var gameData = GameDatabase.GetGame(cmbGame.Text); if (gameData != null) { var display = new DefaultArgumentsDisplay(gameData.RequiredArgs); display.ShowDialog(); } }
		private void btnEditSchedule_Click(object sender, EventArgs e) { using var scheduler = new ScheduleSettingsGUI(_selectedDays, _selectedTime); if (scheduler.ShowDialog() == DialogResult.OK) { _selectedDays = scheduler.SelectedDays; _selectedTime = scheduler.SelectedTime; } }
		private void btnCancel_Click(object sender, EventArgs e) { this.DialogResult = DialogResult.Cancel; this.Close(); }
		private void chkEnableDiscord_CheckedChanged(object sender, EventArgs e) { if (isPrivacyLoading) return; txtDiscordWebhook.Enabled = chkEnableDiscord.Checked; SyncGatekeeper(); }
		private async void btnTestDiscord_Click(object sender, EventArgs e) { string url = txtDiscordWebhook.Text.Trim(); if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("https://discord.com/api/webhooks/")) return; await Core.Instance.SendDiscordAlert(new GameServer { ServerName = txtName.Text, DiscordWebhook = url, IsDiscordAlertEnabled = true }, "TEST CONNECTION", "Alert Success", Color.Lime); }
		private void txtName_TextChanged(object sender, EventArgs e) => SyncGatekeeper();

		private void PopulateMaps(GameInfo gameData, string selectedMap) { cmbWorldName.Items.Clear(); if (gameData.Maps == null) return; foreach (var map in gameData.Maps) cmbWorldName.Items.Add(map); cmbWorldName.Text = selectedMap; }
		private void PopulateGameModes(GameInfo gameData, string selectedMode) { cmbCompetitive.Items.Clear(); if (gameData.GameModes == null) return; foreach (var mode in gameData.GameModes) cmbCompetitive.Items.Add(mode); if (cmbCompetitive.Items.Contains(selectedMode)) cmbCompetitive.SelectedItem = selectedMode; else if (cmbCompetitive.Items.Count > 0) cmbCompetitive.SelectedIndex = 0; }
	}
}