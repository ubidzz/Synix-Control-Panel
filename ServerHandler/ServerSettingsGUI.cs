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
using Synix_Control_Panel.Design;
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
using System.Windows.Forms;
using static Synix_Control_Panel.SynixEngine.Core;

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
		private System.Windows.Forms.Timer debounceTimer;

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);
		private const int EM_SETCUEBANNER = 0x1501;

		public ServerSettingsGUI(GameServer? server = null)
		{
			InitializeComponent();
			isManualLoading = true;
			_existingServer = server;
			_isEditMode = server != null;

			// UI Styling
			UIStyleHelper.StyleWarningLabel(WarningLabel);
			UIStyleHelper.InitializeToggles(this);
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

			isManualLoading = false;
			SyncGatekeeper();
		}

		private void LoadExistingServerData()
		{
			if (_existingServer == null) return;
			isManualLoading = true;

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
			chkUpdateOnStart.Checked = _existingServer.UpdateOnStart;
			chkEnableRcon.Checked = _existingServer.EnableRcon;
			numRconPort.Value = Math.Clamp((decimal)_existingServer.RconPort, numRconPort.Minimum, numRconPort.Maximum);
			txtRconPassword.Text = _existingServer.RconPassword ?? "";
			chkEnableSchedule.Checked = _existingServer.IsScheduledRestartEnabled;
			if (_existingServer.RestartDays != null) _selectedDays = (bool[])_existingServer.RestartDays.Clone();
			_selectedTime = _existingServer.RestartTime ?? "04:00";
			chkBackupOnStart.Checked = _existingServer.BackupOnStart;

			var gameData = GameDatabase.GetGame(_existingServer.Game);
			if (gameData != null)
			{
				PopulateMaps(gameData, _existingServer.WorldName ?? "");
				PopulateGameModes(gameData, _existingServer.GameMode ?? "PVE");
				ToggleGameSpecificFields(gameData);
			}

			cmbGame.Enabled = false;
			isManualLoading = false;
		}

		private void SyncGatekeeper()
		{
			if (isManualLoading) return;

			try
			{
				// 🎯 1. IDENTITY & CAPABILITY
				string currentName = txtName?.Text?.Trim() ?? "";
				bool hasName = !string.IsNullOrWhiteSpace(currentName);
				bool hasGame = cmbGame != null && cmbGame.SelectedIndex > 0;
				string selectedGame = hasGame ? cmbGame.Text : "";
				bool isBaseReady = hasName && hasGame;

				// Helper to check if a control is allowed by the Game Template
				bool CanUnlock(Control c) => isBaseReady && c.Tag?.ToString() == "Required";

				// 🎯 2. DYNAMIC UI UNLOCKING (FULL RESTORATION)
				txtPassword.Enabled = CanUnlock(txtPassword);
				txtAdminPassword.Enabled = CanUnlock(txtAdminPassword);
				txtWorldSeed.Enabled = CanUnlock(txtWorldSeed);
				cmbCompetitive.Enabled = CanUnlock(cmbCompetitive);
				numMaxPlayers.Enabled = CanUnlock(numMaxPlayers);
				numQueryPort.Enabled = CanUnlock(numQueryPort);
				cmbWorldName.Enabled = CanUnlock(cmbWorldName);

				if (numAppPort != null)
					numAppPort.Tag = CanUnlock(numAppPort) ? "Required" : "Disabled"; // Sync tag

				numPort.Enabled = isBaseReady;
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

				// 🎯 3. FOLDER & BROWSE LOCKING (Grey-out Fix)
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

				// 🎯 4. AUTO-PATH GENERATION
				if (!_isEditMode && isBaseReady && chkDefaultPath.Checked)
				{
					string safeName = BackupManager.GetSafeName(currentName);
					txtInstallPath.Text = $@"C:\Synix\Games\{selectedGame}\{safeName}";
				}

				// 🎯 5. PORT DATA ISOLATION
				int gPort = (int)numPort.Value;
				int qPort = numQueryPort.Enabled ? (int)numQueryPort.Value : 0;
				int aPort = (numAppPort != null && numAppPort.Enabled) ? (int)numAppPort.Value : 0;
				int rPort = rconActive ? (int)numRconPort.Value : 0;

				// 🎯 6. COLLISION DETECTION (FULL OS & DB CHECKS)
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

				// 🎯 7. UI STATE ENGINE (The Rounded Color Bar)
				if (!isBaseReady)
				{
					WarningLabel.Text = "  🔒 [LOCKED] Required: Server Name and Game Template selection.";
					WarningLabel.ForeColor = Color.Gold;
					WarningLabel.BackColor = Color.FromArgb(60, 45, 0);
					btnSave.Enabled = false;
				}
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
					// 🚀 SUCCESS STATE
					WarningLabel.Text = _isEditMode ? $"  ✔ [READY] Updating: {currentName}" : "  ✔ [READY] Configuration is valid and safe.";
					WarningLabel.ForeColor = Color.SpringGreen;
					WarningLabel.BackColor = Color.FromArgb(20, 50, 20);

					// Final Button Unlock
					btnSave.Enabled = !string.IsNullOrWhiteSpace(txtInstallPath.Text);
				}

				// 🎯 8. REFRESH ROUNDED CORNERS
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
			if (gameData == null) { foreach (var c in controls) if (c != null) c.Tag = "Disabled"; }
			else
			{
				string args = (gameData.RequiredArgs ?? "").ToLower();
				string rconTemp = (gameData.RconSyntax ?? "").ToLower();
				txtPassword.Tag = args.Contains("{pass}") ? "Required" : "Disabled";
				txtAdminPassword.Tag = args.Contains("{adminpass}") ? "Required" : "Disabled";
				txtWorldSeed.Tag = args.Contains("{seed}") ? "Required" : "Disabled";
				cmbCompetitive.Tag = args.Contains("{mode}") ? "Required" : "Disabled";
				numMaxPlayers.Tag = args.Contains("{maxplayers}") ? "Required" : "Disabled";
				numQueryPort.Tag = args.Contains("{query}") ? "Required" : "Disabled";
				cmbWorldName.Tag = args.Contains("{map}") ? "Required" : "Disabled";
				if (numAppPort != null) numAppPort.Tag = args.Contains("{app_port}") ? "Required" : "Disabled";
				chkEnableRcon.Tag = (args.Contains("{rcon}") || rconTemp.Contains("{rcon_port}")) ? "Required" : "Disabled";
			}
			SyncGatekeeper();
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
		}

		// 🎯 DESIGNER EVENT HANDLERS (EXACT NAMES)
		private void btnSave_Click(object sender, EventArgs e)
		{
			string newName = txtName.Text.Trim();
			string selectedGame = cmbGame.Text;
			if (!Core.Instance.ValidateNameAndReport(newName, selectedGame, _existingServer)) return;
			int gPort = (int)numPort.Value;
			int qPort = (int)numQueryPort.Value;
			int rPort = (int)numRconPort.Value;
			int? aPort = numAppPort.Enabled ? (int)numAppPort.Value : (int?)null;
			if (!Core.Instance.ValidatePortsAndReport(_existingServer, gPort, qPort, rPort, chkEnableRcon.Checked, aPort ?? 0, numAppPort.Enabled, selectedGame)) return;
			string newPath = txtInstallPath.Text.Trim();
			NewServer = new GameServer { Game = selectedGame, ServerName = newName, Port = gPort, QueryPort = qPort, RconPort = rPort, AppPort = aPort, Password = txtPassword.Text, AdminPassword = txtAdminPassword.Text, MaxPlayers = (int)numMaxPlayers.Value, WorldName = cmbWorldName.Text, GameMode = cmbCompetitive.Text, WorldSeed = txtWorldSeed.Text.Trim(), ExtraArgs = txtExtraArgs.Text, IsDefaultPath = chkDefaultPath.Checked, UpdateOnStart = chkUpdateOnStart.Checked, EnableRcon = chkEnableRcon.Checked, RconPassword = txtRconPassword.Text, InstallPath = newPath, IsScheduledRestartEnabled = chkEnableSchedule.Checked, RestartTime = _selectedTime, RestartDays = (bool[])_selectedDays.Clone(), IsDiscordAlertEnabled = chkEnableDiscord.Checked, DiscordWebhook = txtDiscordWebhook.Text.Trim(), Status = _existingServer?.Status ?? StatusManager.GetStatus(ServerState.Stopped), BackupOnStart = chkBackupOnStart.Checked };
			try
			{
				if (_isEditMode && _existingServer != null)
				{
					var existing = MainGUI.serverList.FirstOrDefault(s => s.ServerName == _existingServer.ServerName);
					if (existing != null)
					{
						int index = MainGUI.serverList.IndexOf(existing);
						MainGUI.serverList[index] = NewServer;
					}
				}
				else MainGUI.serverList.Add(NewServer);
				FileHandler.SaveServers(); this.DialogResult = DialogResult.OK; this.Close();
			}
			catch (Exception ex) { MessageBox.Show(ex.Message); }
		}

		private void cmbGame_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (isManualLoading) return;
			if (cmbGame.SelectedIndex > 0)
			{
				var gameData = GameDatabase.GetGame(cmbGame.SelectedItem.ToString());
				if (gameData != null)
				{
					numPort.Value = Math.Clamp((decimal)gameData.Port, numPort.Minimum, numPort.Maximum);
					numQueryPort.Value = Math.Clamp((decimal)gameData.QueryPort, numQueryPort.Minimum, numQueryPort.Maximum);
					PopulateMaps(gameData, gameData.Maps?.FirstOrDefault() ?? "");
					PopulateGameModes(gameData, "PVE");
					ToggleGameSpecificFields(gameData);
				}
			}
			else ToggleGameSpecificFields(null);
			SyncGatekeeper();
		}

		private void btnBrowse_Click(object sender, EventArgs e) { using var fbd = new FolderBrowserDialog(); if (fbd.ShowDialog() == DialogResult.OK) { txtInstallPath.Text = fbd.SelectedPath; SyncGatekeeper(); } }
		private void chkDefaultPath_CheckedChanged(object sender, EventArgs e) => SyncGatekeeper();
		private void txtInstallPath_TextChanged(object sender, EventArgs e) => SyncGatekeeper();
		private void chkEnableRcon_CheckedChanged(object sender, EventArgs e) { if (isManualLoading) return; bool active = chkEnableRcon.Checked; numRconPort.Enabled = txtRconPassword.Enabled = active; SyncGatekeeper(); }
		private void chkEnableSchedule_CheckedChanged(object sender, EventArgs e) { if (isManualLoading) return; if (btnEditSchedule != null) btnEditSchedule.Enabled = chkEnableSchedule.Checked; SyncGatekeeper(); }
		private void txtWorldSeed_KeyPress(object sender, KeyPressEventArgs e) { if (cmbGame.Text == "Rust" && !char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true; }
		private void btnViewArgs_Click(object sender, EventArgs e) { var gameData = GameDatabase.GetGame(cmbGame.Text); if (gameData != null) { var display = new DefaultArgumentsDisplay(gameData.RequiredArgs); display.ShowDialog(); } }
		private void btnEditSchedule_Click(object sender, EventArgs e) { using var scheduler = new ScheduleSettingsGUI(_selectedDays, _selectedTime); if (scheduler.ShowDialog() == DialogResult.OK) { _selectedDays = scheduler.SelectedDays; _selectedTime = scheduler.SelectedTime; } }
		private void btnCancel_Click(object sender, EventArgs e) { this.DialogResult = DialogResult.Cancel; this.Close(); }
		private void chkEnableDiscord_CheckedChanged(object sender, EventArgs e) { if (isManualLoading) return; txtDiscordWebhook.Enabled = chkEnableDiscord.Checked; SyncGatekeeper(); }
		private async void btnTestDiscord_Click(object sender, EventArgs e) { string url = txtDiscordWebhook.Text.Trim(); if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("https://discord.com/api/webhooks/")) return; await Core.Instance.SendDiscordAlert(new GameServer { ServerName = txtName.Text, DiscordWebhook = url, IsDiscordAlertEnabled = true }, "TEST CONNECTION", "Alert Success", Color.Lime); }
		private void txtName_TextChanged(object sender, EventArgs e) => SyncGatekeeper();

		private void PopulateMaps(GameInfo gameData, string selectedMap) { cmbWorldName.Items.Clear(); if (gameData.Maps == null) return; foreach (var map in gameData.Maps) cmbWorldName.Items.Add(map); cmbWorldName.Text = selectedMap; }
		private void PopulateGameModes(GameInfo gameData, string selectedMode) { cmbCompetitive.Items.Clear(); if (gameData.GameModes == null) return; foreach (var mode in gameData.GameModes) cmbCompetitive.Items.Add(mode); if (cmbCompetitive.Items.Contains(selectedMode)) cmbCompetitive.SelectedItem = selectedMode; else if (cmbCompetitive.Items.Count > 0) cmbCompetitive.SelectedIndex = 0; }
	}
}