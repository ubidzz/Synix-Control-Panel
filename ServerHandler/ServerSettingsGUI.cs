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

			// 🎯 SAFETY: Cast to decimal for NumericUpDown
			numPort.Value = Math.Clamp((decimal)_existingServer.Port, numPort.Minimum, numPort.Maximum);
			numQueryPort.Value = Math.Clamp((decimal)_existingServer.QueryPort, numQueryPort.Minimum, numQueryPort.Maximum);
			if (numAppPort != null)
				numAppPort.Value = Math.Clamp((decimal)_existingServer.AppPort, numAppPort.Minimum, numAppPort.Maximum);

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
				Status = _existingServer?.Status ?? StatusManager.GetStatus(ServerState.Stopped),
				BackupOnStart = chkBackupOnStart.Checked,
			};

			// 4. DATABASE UPDATE (Kept exactly as you wrote it)
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
			if (isManualLoading) return; // 🎯 Block during initialization

			// 1. COLLECT DATA
			string currentName = txtName.Text.Trim();
			int gPort = (int)numPort.Value;
			int qPort = (int)numQueryPort.Value;
			int rPort = (int)numRconPort.Value;
			int aPort = (int)numAppPort.Value;
			string selectedGame = cmbGame.Text.Trim();

			// 🎯 THE KEY: Conditional flags for optional features
			bool isAppPortActive = numAppPort.Enabled;
			bool rconActive = chkEnableRcon.Checked;

			bool hasName = !string.IsNullOrWhiteSpace(currentName);
			bool hasGame = cmbGame.SelectedIndex > 0;
			bool isBaseReady = hasName && hasGame;

			// 2. LIVE COLLISIONS (Internal + OS)
			// 🎯 SILENT CHECK: No MessageBox here to keep typing smooth
			bool isNameTaken = MainGUI.serverList.Any(s =>
				s != _existingServer &&
				s.Game.Equals(selectedGame, StringComparison.OrdinalIgnoreCase) &&
				s.ServerName.Equals(currentName, StringComparison.OrdinalIgnoreCase));

			string? gOwner = Core.Instance.GetPortCollisionOwner(gPort, _existingServer);
			string? qOwner = Core.Instance.GetPortCollisionOwner(qPort, _existingServer);

			// 🎯 CONDITIONAL OWNER CHECKS: Only look for owners if the feature is enabled
			string? rOwner = rconActive ? Core.Instance.GetPortCollisionOwner(rPort, _existingServer) : null;
			string? aOwner = isAppPortActive ? Core.Instance.GetPortCollisionOwner(aPort, _existingServer) : null;

			// 🎯 CONDITIONAL OS CHECKS: Only check OS listeners for ports that are "Live"
			bool osConflict = Core.Instance.IsPortInUseLocally(gPort) ||
							  Core.Instance.IsPortInUseLocally(qPort) ||
							  (rconActive && Core.Instance.IsPortInUseLocally(rPort)) ||
							  (isAppPortActive && Core.Instance.IsPortInUseLocally(aPort));

			// 3. UI STATE ENGINE
			if (_isEditMode)
			{
				WarningLabel.Text = $"[READY] Editing existing server: {currentName}.";
				WarningLabel.ForeColor = Color.LimeGreen;
				btnSave.Enabled = true;
			}
			else if (!isBaseReady)
			{
				WarningLabel.Text = "[LOCKED] Required: Provide a Server Name and select a Game Template.";
				WarningLabel.ForeColor = Color.Orange;
				btnSave.Enabled = false;
			}
			else if (isNameTaken)
			{
				WarningLabel.Text = $"[CONFLICT] The name '{currentName}' is already in use for {selectedGame}.";
				WarningLabel.ForeColor = Color.Red;
				btnSave.Enabled = false;
			}
			else if (gOwner != null || qOwner != null || rOwner != null || aOwner != null || osConflict)
			{
				// 🎯 The engine now identifies if RCON or App Port is the specific culprit
				string conflictSource = gOwner ?? qOwner ?? rOwner ?? aOwner ?? "System Process";
				WarningLabel.Text = $"[CONFLICT] Port Collision detected with: {conflictSource}";
				WarningLabel.ForeColor = Color.Red;
				btnSave.Enabled = false;
			}
			else if (isAppPortActive && selectedGame.Contains("Rust", StringComparison.OrdinalIgnoreCase) && aPort < 10000)
			{
				WarningLabel.Text = "[LOCKED] Rust App Port must be 10000+ for mobile compatibility.";
				WarningLabel.ForeColor = Color.Orange;
				btnSave.Enabled = false;
			}
			else
			{
				// 🚀 ALL CLEAR: Generate Paths
				string safeName = System.Text.RegularExpressions.Regex.Replace(currentName, @"[^a-zA-Z0-9_\-]", "_");
				string displayPath = $@"C:\Synix\Games\{selectedGame}\{safeName}";

				WarningLabel.Text = $"[READY] Configuration valid. Default Path: {displayPath}";
				WarningLabel.ForeColor = Color.LimeGreen;

				if (chkDefaultPath.Checked) txtInstallPath.Text = displayPath;
				btnSave.Enabled = !string.IsNullOrWhiteSpace(txtInstallPath.Text);
			}
		}

		private void ToggleGameSpecificFields(GameInfo? gameData)
		{
			if (gameData == null)
			{
				txtPassword.Enabled = txtAdminPassword.Enabled = txtWorldSeed.Enabled = cmbCompetitive.Enabled =
				chkEnableRcon.Enabled = numAppPort.Enabled = numMaxPlayers.Enabled = numQueryPort.Enabled =
				cmbWorldName.Enabled = chkUpdateOnStart.Enabled = false;
				return;
			}

			string args = gameData.RequiredArgs.ToLower();
			txtPassword.Enabled = args.Contains("{pass}");
			txtAdminPassword.Enabled = args.Contains("{adminpass}");
			txtWorldSeed.Enabled = args.Contains("{seed}");
			cmbCompetitive.Enabled = args.Contains("{mode}");
			numMaxPlayers.Enabled = args.Contains("{maxplayers}");
			numQueryPort.Enabled = args.Contains("{query}");
			cmbWorldName.Enabled = args.Contains("{map}");
			if (numAppPort != null) numAppPort.Enabled = args.Contains("{app_port}");
			chkUpdateOnStart.Enabled = args.Contains("{steamappid}");

			chkEnableRcon.Enabled = args.Contains("{rcon}") || args.Contains("{rcon_port}") || args.Contains("{rcon_pass}");
			chkEnableRcon_CheckedChanged(null, null);
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

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		#endregion
	}
}