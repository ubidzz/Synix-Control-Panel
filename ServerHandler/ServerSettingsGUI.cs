/*
 * Copyright (c) 2026 ubidzz. All Rights Reserved.
 *
 * This file is part of Synix Control Panel.
 *
 * This code is provided for transparent viewing and personal use only.
 * Unauthorized distribution, public modification, or commercial 
 * use of this source code or the compiled executable is strictly 
 * prohibited. Please refer to the LICENSE file in the root 
 * directory for full terms.
 */
using Synix_Control_Panel.Database;
using Synix_Control_Panel.FileFolderHandler;
using Synix_Control_Panel.Help;
using Synix_Control_Panel.ServerHandler;
using Synix_Control_Panel.SynixEngine;
using Synix_Control_Panel.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static Synix_Control_Panel.FileFolderHandler.FolderHandler;
using static Synix_Control_Panel.SynixEngine.Core;
using System.Runtime.InteropServices;

namespace Synix_Control_Panel
{
	public partial class ServerSettingsGUI : Form
	{
		public GameServer? NewServer { get; private set; }

		private bool isManualLoading = false;
		private bool _isEditMode = false;
		private GameServer? _existingServer = null;
		private string _oldPath = string.Empty;

		private bool[] _selectedDays = new bool[7] { false, false, false, false, false, false, false };
		private string _selectedTime = "04:00";

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

		private const int EM_SETCUEBANNER = 0x1501;

		public ServerSettingsGUI(GameServer? server = null)
		{
			InitializeComponent();
			_existingServer = server;
			_isEditMode = server != null;

			// 1. Setup Tags for UI Pills
			chkDefaultPath.Tag = "Default Folder";
			chkEnableSchedule.Tag = "Activate Scheduler";
			chkUpdateOnStart.Tag = "Update on Start";
			chkEnableRcon.Tag = "RCON";
			chkBackupOnStart.Tag = "Backup on Start";

			chkEnableDiscord.Tag = "Discord Alerts";
			SendMessage(txtDiscordWebhook.Handle, EM_SETCUEBANNER, 0, "Paste Discord Webhook URL here...");

			// 2. Style & Game List Setup
			Synix_Control_Panel.UI.UIStyleHelper.InitializeToggles(this);

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
				// Block events for new server default selection
				isManualLoading = true;
				cmbGame.SelectedIndex = 0;
				isManualLoading = false;
			}

			SyncGatekeeper();
		}

		private void LoadExistingServerData()
		{
			if (_existingServer == null) return;

			isManualLoading = true; // 🎯 ACTIVATE SHIELD

			txtName.Text = _existingServer.ServerName ?? "";
			int gameIndex = cmbGame.FindStringExact(_existingServer.Game);
			if (gameIndex != -1) cmbGame.SelectedIndex = gameIndex;

			txtPassword.Text = _existingServer.Password ?? "";
			txtAdminPassword.Text = _existingServer.AdminPassword ?? "";

			chkEnableDiscord.Checked = _existingServer.IsDiscordAlertEnabled;
			txtDiscordWebhook.Text = _existingServer.DiscordWebhook ?? "";

			// Disable the textbox if the toggle is off for better UX
			txtDiscordWebhook.Enabled = chkEnableDiscord.Checked;

			// 🎯 SAFETY: Cast to decimal for NumericUpDown
			numPort.Value = Math.Clamp((decimal)_existingServer.Port, numPort.Minimum, numPort.Maximum);
			numQueryPort.Value = Math.Clamp((decimal)_existingServer.QueryPort, numQueryPort.Minimum, numQueryPort.Maximum);
			if (numAppPort != null)
				numAppPort.Value = Math.Clamp((decimal)(_existingServer.AppPort ?? numAppPort.Minimum), numAppPort.Minimum, numAppPort.Maximum);

			numMaxPlayers.Value = Math.Clamp((decimal)_existingServer.MaxPlayers, numMaxPlayers.Minimum, numMaxPlayers.Maximum);

			txtInstallPath.Text = _existingServer.InstallPath ?? "";
			chkDefaultPath.Checked = _existingServer.IsDefaultPath;
			txtExtraArgs.Text = _existingServer.ExtraArgs ?? "";
			txtWorldSeed.Text = _existingServer.WorldSeed ?? "12345";
			chkUpdateOnStart.Checked = _existingServer.UpdateOnStart;

			chkEnableRcon.Checked = _existingServer.EnableRcon;
			numRconPort.Value = Math.Clamp((decimal)_existingServer.RconPort, numRconPort.Minimum, numRconPort.Maximum);
			txtRconPassword.Text = _existingServer.RconPassword ?? "";

			chkEnableSchedule.Checked = _existingServer.IsScheduledRestartEnabled;
			if (_existingServer.RestartDays != null) _selectedDays = (bool[])_existingServer.RestartDays.Clone();
			_selectedTime = _existingServer.RestartTime ?? "04:00";

			if (btnEditSchedule != null) btnEditSchedule.Enabled = chkEnableSchedule.Checked;
			chkBackupOnStart.Checked = _existingServer.BackupOnStart;

			var gameData = GameDatabase.GetGame(_existingServer.Game);
			if (gameData != null)
			{
				PopulateMaps(gameData, _existingServer.WorldName ?? "");
				PopulateGameModes(gameData, _existingServer.GameMode ?? "PVE");
				ToggleGameSpecificFields(gameData);
			}

			cmbGame.Enabled = false; // Prevent game changes on existing servers
			isManualLoading = false; // 🎯 DEACTIVATE SHIELD
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			string newName = txtName.Text.Trim();
			string selectedGame = cmbGame.Text;

			if (!Core.Instance.ValidateNameAndReport(newName, selectedGame, _existingServer)) return;

			// --- 1. PORT CAPTURE ---
			int gPort = (int)numPort.Value;
			int qPort = (int)numQueryPort.Value;
			int rPort = (int)numRconPort.Value;

			// 🎯 FIX: Changed 'int' to 'int?' so it can actually handle the 'null' later
			int? aPort = numAppPort.Enabled ? (int)numAppPort.Value : (int?)null;

			// Use 'aPort ?? 0' here so the Validator gets a number, even if it's inactive
			if (!Core.Instance.ValidatePortsAndReport(_existingServer, gPort, qPort, rPort, chkEnableRcon.Checked, aPort ?? 0, numAppPort.Enabled, selectedGame)) return;

			// 2. PATH LOGIC (Kept exactly as you wrote it)
			string newPath = txtInstallPath.Text.Trim();
			if (_isEditMode && !string.IsNullOrEmpty(_oldPath) && _oldPath != newPath)
			{
				try { if (Directory.Exists(_oldPath)) Directory.Move(_oldPath, newPath); }
				catch (Exception ex) { MessageBox.Show($"Move failed: {ex.Message}"); }
			}

			// 3. OBJECT CREATION
			NewServer = new GameServer
			{
				Game = selectedGame,
				ServerName = newName,
				Port = gPort,
				QueryPort = qPort,
				RconPort = rPort,

				// 🎯 FIX: Property name must be 'AppPort' (matches your class), not 'aPort'
				AppPort = aPort,

				Password = txtPassword.Text,
				AdminPassword = txtAdminPassword.Text,
				MaxPlayers = (int)numMaxPlayers.Value,
				WorldName = cmbWorldName.Text,
				GameMode = cmbCompetitive.Text,
				WorldSeed = txtWorldSeed.Text.Trim(),
				ExtraArgs = txtExtraArgs.Text,
				IsDefaultPath = chkDefaultPath.Checked,
				UpdateOnStart = chkUpdateOnStart.Checked,
				EnableRcon = chkEnableRcon.Checked,
				RconPassword = txtRconPassword.Text,
				InstallPath = newPath,
				IsScheduledRestartEnabled = chkEnableSchedule.Checked,
				RestartTime = _selectedTime,
				RestartDays = (bool[])_selectedDays.Clone(),
				IsDiscordAlertEnabled = chkEnableDiscord.Checked,
				DiscordWebhook = txtDiscordWebhook.Text.Trim(),
				Status = _existingServer?.Status ?? StatusManager.GetStatus(ServerState.Stopped),
				BackupOnStart = chkBackupOnStart.Checked,
			};

			// 4. DATABASE UPDATE
			try
			{
				if (_isEditMode && _existingServer != null)
				{
					int index = -1;
					for (int i = 0; i < MainGUI.serverList.Count; i++)
					{
						if (MainGUI.serverList[i].ServerName == _existingServer.ServerName)
						{
							index = i;
							break;
						}
					}
					if (index != -1) MainGUI.serverList[index] = NewServer;
				}
				else
				{
					MainGUI.serverList.Add(NewServer);
				}

				FileHandler.SaveServers();
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			catch (Exception ex) { MessageBox.Show($"Operation failed: {ex.Message}"); }
		}

		private void SyncGatekeeper()
		{
			// 🎯 1. THE SHIELD: Prevent logic loops while loading existing data
			if (isManualLoading) return;

			try
			{
				// 🎯 2. IDENTITY CHECK: Does the server have a Name and a Game?
				string currentName = txtName?.Text?.Trim() ?? "";
				bool hasName = !string.IsNullOrWhiteSpace(currentName);
				bool hasGame = cmbGame != null && cmbGame.SelectedIndex > 0;
				string selectedGame = hasGame ? cmbGame.Text : "";

				// This is our Master Key for unlocking the UI
				bool isBaseReady = hasName && hasGame;

				// 🎯 3. CAPABILITY HELPER: Checks if the game supports the tag AND the identity is ready
				bool CanUnlock(Control c) => isBaseReady && c.Tag?.ToString() == "Required";

				// 🎯 4. DYNAMIC UI UNLOCKING
				// Game Specifics
				txtWorldSeed.Enabled = CanUnlock(txtWorldSeed);
				cmbWorldName.Enabled = CanUnlock(cmbWorldName);
				cmbCompetitive.Enabled = CanUnlock(cmbCompetitive);
				numMaxPlayers.Enabled = CanUnlock(numMaxPlayers);

				// Network & Identity
				numPort.Enabled = isBaseReady;
				numQueryPort.Enabled = CanUnlock(numQueryPort);
				numAppPort.Enabled = CanUnlock(numAppPort);

				// Security & RCON
				txtPassword.Enabled = CanUnlock(txtPassword);
				txtAdminPassword.Enabled = CanUnlock(txtAdminPassword);
				chkEnableRcon.Enabled = CanUnlock(chkEnableRcon);

				// Only unlock sub-fields if RCON is both supported AND toggled ON
				bool rconActive = chkEnableRcon.Enabled && chkEnableRcon.Checked;
				numRconPort.Enabled = rconActive;
				txtRconPassword.Enabled = rconActive;

				// Discord Alerts
				chkEnableDiscord.Enabled = isBaseReady;
				txtDiscordWebhook.Enabled = isBaseReady && chkEnableDiscord.Checked;

				// Automation & Backups
				chkUpdateOnStart.Enabled = CanUnlock(chkUpdateOnStart);
				chkBackupOnStart.Enabled = isBaseReady;
				chkEnableSchedule.Enabled = isBaseReady;
				if (btnEditSchedule != null) btnEditSchedule.Enabled = isBaseReady && chkEnableSchedule.Checked;

				// 🎯 5. FOLDER LOCATION (Locked during Edit Mode)
				if (_isEditMode)
				{
					// Hard-lock the path controls so location cannot be changed after install
					chkDefaultPath.Enabled = false;
					btnBrowse.Enabled = false;
					txtInstallPath.Enabled = false;
				}
				else
				{
					// Normal behavior for New Servers
					chkDefaultPath.Enabled = isBaseReady;
					btnBrowse.Enabled = isBaseReady;
					txtInstallPath.Enabled = isBaseReady && !chkDefaultPath.Checked;
				}

				// 🎯 6. VALIDATION ENGINE (Collisions & Conflicts)
				int gPort = (int)numPort.Value;
				int qPort = (int)numQueryPort.Value;
				int rPort = (int)numRconPort.Value;
				int aPort = numAppPort != null ? (int)numAppPort.Value : 0;

				bool isNameTaken = MainGUI.serverList.Any(s =>
					s != _existingServer &&
					s.Game.Equals(selectedGame, StringComparison.OrdinalIgnoreCase) &&
					s.ServerName.Equals(currentName, StringComparison.OrdinalIgnoreCase));

				string? gOwner = Core.Instance.GetPortCollisionOwner(gPort, _existingServer);
				string? qOwner = numQueryPort.Enabled ? Core.Instance.GetPortCollisionOwner(qPort, _existingServer) : null;
				string? rOwner = rconActive ? Core.Instance.GetPortCollisionOwner(rPort, _existingServer) : null;
				string? aOwner = numAppPort.Enabled ? Core.Instance.GetPortCollisionOwner(aPort, _existingServer) : null;

				bool osConflict = Core.Instance.IsPortInUseLocally(gPort) ||
								  (numQueryPort.Enabled && Core.Instance.IsPortInUseLocally(qPort)) ||
								  (rconActive && Core.Instance.IsPortInUseLocally(rPort)) ||
								  (numAppPort.Enabled && Core.Instance.IsPortInUseLocally(aPort));

				// 🎯 7. UI STATE ENGINE (Warnings & Paths)
				if (_isEditMode)
				{
					WarningLabel.Text = $"[READY] Updating existing server: {currentName}";
					WarningLabel.ForeColor = Color.LimeGreen;
					btnSave.Enabled = true;
				}
				else if (!isBaseReady)
				{
					WarningLabel.Text = "[LOCKED] Required: Server Name and Game Template selection.";
					WarningLabel.ForeColor = Color.Orange;
					btnSave.Enabled = false;
				}
				else if (isNameTaken)
				{
					WarningLabel.Text = $"[CONFLICT] Name '{currentName}' is already used for {selectedGame}.";
					WarningLabel.ForeColor = Color.Red;
					btnSave.Enabled = false;
				}
				else if (gOwner != null || qOwner != null || rOwner != null || aOwner != null || osConflict)
				{
					string source = gOwner ?? qOwner ?? rOwner ?? "Local System Process";
					WarningLabel.Text = $"[CONFLICT] Port Collision detected with: {source}";
					WarningLabel.ForeColor = Color.Red;
					btnSave.Enabled = false;
				}
				else
				{
					// 🚀 SUCCESS STATE: Everything is valid
					WarningLabel.Text = "[READY] Configuration is valid and safe to deploy.";
					WarningLabel.ForeColor = Color.LimeGreen;

					// 🎯 THE FIX: Only auto-generate path if NOT in edit mode
					if (!_isEditMode && chkDefaultPath.Checked)
					{
						string safeName = BackupManager.GetSafeName(currentName);
						txtInstallPath.Text = $@"C:\Synix\Games\{selectedGame}\{safeName}";
					}

					btnSave.Enabled = !string.IsNullOrWhiteSpace(txtInstallPath.Text);
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[GATEKEEPER CRASH] {ex.Message}");
			}
		}

		private void ToggleGameSpecificFields(GameInfo? gameData)
		{
			// 🎯 1. RESET TAGS: If no game is picked, everything is "Disabled"
			if (gameData == null)
			{
				var controls = new Control[] {
			txtPassword, txtAdminPassword, txtWorldSeed, cmbCompetitive,
			numAppPort, numMaxPlayers, numQueryPort, cmbWorldName, chkEnableRcon
		};

				foreach (var c in controls) if (c != null) c.Tag = "Disabled";

				SyncGatekeeper();
				return;
			}

			// 🎯 2. SCAN BLUEPRINTS: Pull the strings from your GameDatabase
			string args = (gameData.RequiredArgs ?? "").ToLower();
			string rconTemp = (gameData.RconSyntax ?? "").ToLower();

			// 🎯 3. ASSIGN CAPABILITY TAGS: Mark what the game actually supports
			// We use "Required" so SyncGatekeeper knows it's allowed to unlock these
			txtPassword.Tag = args.Contains("{pass}") ? "Required" : "Disabled";
			txtAdminPassword.Tag = args.Contains("{adminpass}") ? "Required" : "Disabled";
			txtWorldSeed.Tag = args.Contains("{seed}") ? "Required" : "Disabled";
			cmbCompetitive.Tag = args.Contains("{mode}") ? "Required" : "Disabled";
			numMaxPlayers.Tag = args.Contains("{maxplayers}") ? "Required" : "Disabled";
			numQueryPort.Tag = args.Contains("{query}") ? "Required" : "Disabled";
			cmbWorldName.Tag = args.Contains("{map}") ? "Required" : "Disabled";

			if (numAppPort != null)
				numAppPort.Tag = args.Contains("{app_port}") ? "Required" : "Disabled";

			// RCON Check: Does the main string call {rcon} or does the RCON syntax have ports?
			chkEnableRcon.Tag = (args.Contains("{rcon}") || rconTemp.Contains("{rcon_port}")) ? "Required" : "Disabled";

			// Steam Update Check
			chkUpdateOnStart.Tag = args.Contains("{steamappid}") ? "Required" : "Disabled";

			// 🎯 4. REFRESH UI: Let the Gatekeeper apply the final locks
			SyncGatekeeper();
		}

		private void chkEnableDiscord_CheckedChanged(object sender, EventArgs e)
		{
			if (isManualLoading) return;

			// Toggle the textbox based on the pill state
			txtDiscordWebhook.Enabled = chkEnableDiscord.Checked;

			// Optional: Add a visual cue if they turn it on but leave it empty
			SyncGatekeeper();
		}

		#region Event Handlers

		private void txtName_TextChanged(object sender, EventArgs e) => SyncGatekeeper();

		private void cmbGame_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (isManualLoading) return; // 🎯 SHIELDED

			if (cmbGame.SelectedIndex > 0)
			{
				var gameData = GameDatabase.GetGame(cmbGame.SelectedItem.ToString());
				if (gameData != null)
				{
					numPort.Value = Math.Clamp((decimal)gameData.Port, numPort.Minimum, numPort.Maximum);
					numQueryPort.Value = Math.Clamp((decimal)gameData.QueryPort, numQueryPort.Minimum, numQueryPort.Maximum);
					PopulateMaps(gameData, gameData.Maps?.FirstOrDefault() ?? "");
					PopulateGameModes(gameData, _isEditMode ? (_existingServer?.GameMode ?? "PVE") : "PVE");
					ToggleGameSpecificFields(gameData);
				}
			}
			else ToggleGameSpecificFields(null);
			SyncGatekeeper();
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			using var fbd = new FolderBrowserDialog();
			if (fbd.ShowDialog() == DialogResult.OK)
			{
				txtInstallPath.Text = fbd.SelectedPath;
				SyncGatekeeper();
			}
		}

		private void chkDefaultPath_CheckedChanged(object sender, EventArgs e) => SyncGatekeeper();

		private void txtInstallPath_TextChanged(object sender, EventArgs e) => SyncGatekeeper();

		private void chkEnableRcon_CheckedChanged(object sender, EventArgs e)
		{
			if (isManualLoading) return; // 🎯 SHIELDED
			bool active = chkEnableRcon.Checked;
			lblRCONport.Enabled = numRconPort.Enabled = lblRCONpassword.Enabled = txtRconPassword.Enabled = active;
		}

		private void chkEnableSchedule_CheckedChanged(object sender, EventArgs e)
		{
			if (isManualLoading) return; // 🎯 SHIELDED
			if (btnEditSchedule != null) btnEditSchedule.Enabled = chkEnableSchedule.Checked;
			SyncGatekeeper();
		}

		private void txtWorldSeed_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (cmbGame.Text == "Rust" && !char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
		}

		private void btnViewArgs_Click(object sender, EventArgs e)
		{
			string selectedGame = cmbGame.SelectedItem?.ToString() ?? "";
			if (!string.IsNullOrEmpty(selectedGame) && selectedGame != "-- Pick a Game --")
			{
				var gameData = GameDatabase.GetGame(selectedGame);
				if (gameData != null)
				{
					DefaultArgumentsDisplay display = new DefaultArgumentsDisplay(gameData.RequiredArgs);
					display.ShowDialog();
				}
			}
		}

		private void btnEditSchedule_Click(object sender, EventArgs e)
		{
			using (var scheduler = new ScheduleSettingsGUI(_selectedDays, _selectedTime))
			{
				if (scheduler.ShowDialog() == DialogResult.OK)
				{
					_selectedDays = scheduler.SelectedDays;
					_selectedTime = scheduler.SelectedTime;
				}
			}
		}

		private void PopulateMaps(GameInfo gameData, string selectedMap)
		{
			cmbWorldName.Items.Clear();
			if (gameData.Maps == null) return;
			foreach (var map in gameData.Maps) cmbWorldName.Items.Add(map);
			cmbWorldName.Text = selectedMap;
		}

		private void PopulateGameModes(GameInfo gameData, string selectedMode)
		{
			cmbCompetitive.Items.Clear();
			if (gameData.GameModes == null) return;
			foreach (var mode in gameData.GameModes) cmbCompetitive.Items.Add(mode);
			if (!string.IsNullOrEmpty(selectedMode) && cmbCompetitive.Items.Contains(selectedMode))
				cmbCompetitive.SelectedItem = selectedMode;
			else if (cmbCompetitive.Items.Count > 0) cmbCompetitive.SelectedIndex = 0;
		}

		private async void btnTestDiscord_Click(object sender, EventArgs e)
		{
			string url = txtDiscordWebhook.Text.Trim();

			// 1. Validate the URL exists
			if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("https://discord.com/api/webhooks/"))
			{
				MessageBox.Show("Please paste a valid Discord Webhook URL first.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			// 2. Create a temporary 'Dummy' server for the test
			// This allows us to test the link even before the server is saved to the JSON
			GameServer testDummy = new GameServer
			{
				ServerName = !string.IsNullOrWhiteSpace(txtName.Text) ? txtName.Text : "Synix Test Rig",
				DiscordWebhook = url,
				IsDiscordAlertEnabled = true // Force true for the test
			};

			// 3. Fire the Alert
			await Core.Instance.SendDiscordAlert(testDummy, "TEST CONNECTION",
				"Great news! Your Synix Control Panel is now officially linked to this channel. Future alerts will appear here.", Color.Lime);

			MessageBox.Show("Test alert sent! Check your Discord channel.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		#endregion
	}
}