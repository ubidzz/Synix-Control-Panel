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
using Synix_Control_Panel.ServerHandler;
using Synix_Control_Panel.SynixEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static Synix_Control_Panel.FileFolderHandler.FolderHandler;

namespace Synix_Control_Panel
{
	public partial class ServerSettingsGUI : Form
	{
		public GameServer? NewServer { get; private set; }

		private bool isManualLoading = false;
		private bool _isEditMode = false;
		private GameServer? _existingServer = null;

		public ServerSettingsGUI(GameServer? server = null)
		{
			InitializeComponent();
			_existingServer = server;
			_isEditMode = server != null;

			// 1. Setup UI Styles
			dtpRestartTime.ShowUpDown = true;
			dtpRestartTime.Format = DateTimePickerFormat.Custom;
			dtpRestartTime.CustomFormat = "HH:mm";

			// 2. Populate and Sort Game List (0-9, A-Z)
			cmbGame.Items.Clear();
			cmbGame.Items.Add("-- Pick a Game --");
			var sortedGames = GameDatabase.GetGameList().OrderBy(g => g.Game).ToList();
			foreach (var game in sortedGames)
			{
				cmbGame.Items.Add(game.Game);
			}

			// 3. Load Data
			if (_isEditMode && _existingServer != null)
			{
				LoadExistingServerData();
			}
			else
			{
				cmbGame.SelectedIndex = 0;
				WarningLabel.Text = "[WARNING] Pick a location to install or use default. This cannot be changed later!";
				WarningLabel.ForeColor = Color.Red;
			}

			// 4. Initial Gatekeeper Check
			SyncGatekeeper();
		}

		private void LoadExistingServerData()
		{
			isManualLoading = true;

			txtName.Text = _existingServer.ServerName;
			int gameIndex = cmbGame.FindStringExact(_existingServer.Game);
			if (gameIndex != -1) cmbGame.SelectedIndex = gameIndex;

			txtPassword.Text = _existingServer.Password;
			txtAdminPassword.Text = _existingServer.AdminPassword;
			numPort.Value = _existingServer.Port;
			numQueryPort.Value = _existingServer.QueryPort;
			numMaxPlayers.Value = _existingServer.MaxPlayers;
			txtInstallPath.Text = _existingServer.InstallPath;
			chkDefaultPath.Checked = _existingServer.IsDefaultPath;
			txtExtraArgs.Text = _existingServer.ExtraArgs;
			txtWorldSeed.Text = _existingServer.WorldSeed ?? "12345";

			// Load Maintenance Settings
			chkEnableSchedule.Checked = _existingServer.IsScheduledRestartEnabled;
			if (DateTime.TryParse(_existingServer.RestartTime, out DateTime savedTime))
				dtpRestartTime.Value = savedTime;

			// Map Day Checkboxes
			if (_existingServer.RestartDays != null && _existingServer.RestartDays.Length == 7)
			{
				chkSun.Checked = _existingServer.RestartDays[0];
				chkMon.Checked = _existingServer.RestartDays[1];
				chkTue.Checked = _existingServer.RestartDays[2];
				chkWed.Checked = _existingServer.RestartDays[3];
				chkThu.Checked = _existingServer.RestartDays[4];
				chkFri.Checked = _existingServer.RestartDays[5];
				chkSat.Checked = _existingServer.RestartDays[6];
			}

			var gameData = GameDatabase.GetGame(_existingServer.Game);
			if (gameData != null)
			{
				PopulateMaps(gameData, _existingServer.WorldName);
				PopulateGameModes(gameData, _existingServer.GameMode);
				ToggleGameSpecificFields(gameData);
			}

			// Lock critical fields in Edit Mode
			cmbGame.Enabled = false;
			chkDefaultPath.Enabled = false;
			txtInstallPath.Enabled = false;
			btnBrowse.Enabled = false;
			WarningLabel.Text = "{WARNING] You cannot edit location after the server has been saved!";
			WarningLabel.ForeColor = Color.Red;

			isManualLoading = false;
		}

		/// <summary>
		/// 🤖 AI Gatekeeper: The Engine decides if the form is valid.
		/// </summary>
		private void SyncGatekeeper()
		{
			// 1. Basic Validation
			bool hasGame = cmbGame.SelectedIndex > 0;
			bool hasName = !string.IsNullOrWhiteSpace(txtName.Text);
			bool isReady = hasGame && hasName;

			// 2. Lock/Unlock Core Groups
			groupBox1.Enabled = isReady;

			if (!_isEditMode)
			{
				chkDefaultPath.Enabled = isReady;

				// 🛡️ BROWSE LOCK LOGIC: Only active if ready AND NOT using default
				bool customMode = isReady && !chkDefaultPath.Checked;
				txtInstallPath.Enabled = customMode;
				btnBrowse.Enabled = customMode;

				// 📂 PATH POPULATION LOGIC
				if (isReady && chkDefaultPath.Checked)
				{
					// Automatically fill the path when default is checked
					string cleanGame = cmbGame.SelectedItem.ToString().Replace(" ", "_");
					string cleanName = txtName.Text.Trim().Replace(" ", "_");
					txtInstallPath.Text = Path.Combine(@"C:\Games", cleanGame, cleanName);

					WarningLabel.Text = $"Synix will install {cmbGame.SelectedItem.ToString()} to: {txtInstallPath.Text}";
					WarningLabel.ForeColor = Color.Green;
				}
				else if (isReady && !chkDefaultPath.Checked)
				{
					// Logic for custom path warnings
					if (string.IsNullOrWhiteSpace(txtInstallPath.Text))
					{
						WarningLabel.Text = "[ACTION REQUIRED] You can choose a custom installation location or use the default folder location: C:/Games/[Game]/[Server Name]";
						WarningLabel.ForeColor = Color.Red;
					}
					else
					{
						WarningLabel.Text = "Custom installation location verified.";
						WarningLabel.ForeColor = Color.Green;
					}
				}
				else
				{
					// Not ready (missing name or game selection)
					txtInstallPath.Text = string.Empty;
					WarningLabel.Text = "[INFO] Enter a Server Name and select a Game to enable installation.";
					WarningLabel.ForeColor = Color.Red;
				}
			}

			// 3. MASTER SAVE BUTTON LOCK
			// Save is only enabled if ready AND we have a location (default checked OR manual path entered)
			bool hasLocation = chkDefaultPath.Checked || !string.IsNullOrWhiteSpace(txtInstallPath.Text);
			btnSave.Enabled = isReady && hasLocation;
		}

		private void UpdatePathPreview(bool isReady)
		{
			if (_isEditMode || !chkDefaultPath.Checked) return;

			if (isReady)
			{
				string cleanGame = cmbGame.SelectedItem.ToString().Replace(" ", "_");
				string cleanName = txtName.Text.Trim().Replace(" ", "_");
				txtInstallPath.Text = Path.Combine(@"C:\Games", cleanGame, cleanName);

				WarningLabel.Text = $"Synix will install to: {txtInstallPath.Text}";
				WarningLabel.ForeColor = Color.Green;
			}
			else
			{
				txtInstallPath.Text = string.Empty;
				WarningLabel.Text = "Enter a Server Name and select a Game to continue.";
				WarningLabel.ForeColor = Color.Red;
			}
		}

		private void cmbGame_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cmbGame.SelectedIndex > 0)
			{
				var gameData = GameDatabase.GetGame(cmbGame.SelectedItem.ToString());
				if (gameData != null)
				{
					numPort.Value = gameData.Port;
					numQueryPort.Value = gameData.QueryPort;
					PopulateMaps(gameData, gameData.Maps?.FirstOrDefault() ?? "");
					PopulateGameModes(gameData, "PVE");

					// 🔒 CALL THE SMART LOCKER
					ToggleGameSpecificFields(gameData);
				}
			}
			else
			{
				ToggleGameSpecificFields(null);
			}
			SyncGatekeeper();
		}

		private void txtName_TextChanged(object sender, EventArgs e) => SyncGatekeeper();
		private void chkDefaultPath_CheckedChanged(object sender, EventArgs e) => SyncGatekeeper();
		private void txtInstallPath_TextChanged(object sender, EventArgs e) => SyncGatekeeper();

		private void btnSave_Click(object sender, EventArgs e)
		{
			string newName = txtName.Text.Trim();
			string selectedGame = cmbGame.Text;

			if (!Core.Instance.ValidateNameAndReport(newName, selectedGame, _existingServer)) return;
			if (!Core.Instance.ValidatePortsAndReport((int)numPort.Value, (int)numQueryPort.Value, (int)numRconPort.Value, chkEnableRcon.Checked, _existingServer)) return;

			string targetPath = txtInstallPath.Text;
			if (!Core.Instance.ValidateFolderAndReport(targetPath, _isEditMode)) return;

			var dbGame = GameDatabase.GetGame(selectedGame);

			NewServer = new GameServer
			{
				Game = selectedGame,
				ServerName = newName,
				NeedsConfigWarning = dbGame?.NeedsConfigWarning ?? false,
				IsFirstBoot = _isEditMode && _existingServer != null ? _existingServer.IsFirstBoot : true,
				Port = (int)numPort.Value,
				QueryPort = (int)numQueryPort.Value,
				Password = txtPassword.Text,
				AdminPassword = txtAdminPassword.Text,
				MaxPlayers = (int)numMaxPlayers.Value,
				WorldName = cmbWorldName.Text,
				WorldSeed = txtWorldSeed.Text.Trim(),
				ExtraArgs = txtExtraArgs.Text,
				IsDefaultPath = chkDefaultPath.Checked,
				Status = _isEditMode && _existingServer != null ? _existingServer.Status : "Offline",
				PID = _isEditMode && _existingServer != null ? _existingServer.PID : null,
				GameMode = cmbCompetitive.Text,
				EnableRcon = chkEnableRcon.Checked,
				RconPort = (int)numRconPort.Value,
				RconPassword = txtRconPassword.Text,
				InstallPath = targetPath,

				// 🕒 SAVE MAINTENANCE SETTINGS
				IsScheduledRestartEnabled = chkEnableSchedule.Checked,
				RestartTime = dtpRestartTime.Value.ToString("HH:mm"),
				RestartDays = new bool[]
				{
					chkSun.Checked, chkMon.Checked, chkTue.Checked, chkWed.Checked,
					chkThu.Checked, chkFri.Checked, chkSat.Checked
				},
				LastMaintenanceDate = _existingServer?.LastMaintenanceDate ?? ""
			};

			try
			{
				if (_isEditMode && _existingServer != null)
				{
					if (_existingServer.InstallPath != targetPath && Directory.Exists(_existingServer.InstallPath))
					{
						ServerFolder.Rename(_existingServer, NewServer);
					}
					int index = MainGUI.serverList.IndexOf(_existingServer);
					if (index != -1) MainGUI.serverList[index] = NewServer;
				}
				else
				{
					FolderHandler.Create(targetPath);
					MainGUI.serverList.Add(NewServer);
				}

				FileHandler.SaveServers();
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Operation failed: {ex.Message}", "File Error");
			}
		}

		private void chkEnableSchedule_CheckedChanged(object sender, EventArgs e)
		{
			bool isEnabled = chkEnableSchedule.Checked;
			dtpRestartTime.Enabled = isEnabled;
			chkSun.Enabled = isEnabled;
			chkMon.Enabled = isEnabled;
			chkTue.Enabled = isEnabled;
			chkWed.Enabled = isEnabled;
			chkThu.Enabled = isEnabled;
			chkFri.Enabled = isEnabled;
			chkSat.Enabled = isEnabled;
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

		private void PopulateMaps(GameInfo gameData, string selectedMap)
		{
			cmbWorldName.Items.Clear();
			if (gameData.Maps == null) return;
			foreach (var map in gameData.Maps) cmbWorldName.Items.Add(map);
			if (cmbWorldName.Items.Contains(selectedMap)) cmbWorldName.SelectedItem = selectedMap;
			else cmbWorldName.Text = selectedMap;
		}

		private void PopulateGameModes(GameInfo gameData, string selectedMode)
		{
			cmbCompetitive.Items.Clear();
			if (gameData.GameModes == null) return;
			foreach (var mode in gameData.GameModes) cmbCompetitive.Items.Add(mode);
			if (cmbCompetitive.Items.Contains(selectedMode)) cmbCompetitive.SelectedItem = selectedMode;
			else cmbCompetitive.Text = selectedMode;
		}

		private void txtWorldSeed_KeyPress(object sender, KeyPressEventArgs e)
		{
			// If the selected game is Rust, only allow numbers in the seed box
			if (cmbGame.Text == "Rust")
			{
				if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
				{
					e.Handled = true; // Block the key press
				}
			}
		}

		private void ToggleGameSpecificFields(GameInfo? gameData)
		{
			if (gameData == null)
			{
				txtWorldSeed.Enabled = false;
				cmbCompetitive.Enabled = false;
				chkEnableRcon.Enabled = false;
				return;
			}

			// 🎲 SEED LOCK: Only unlock if the arguments actually contain the {seed} tag
			bool supportsSeed = gameData.RequiredArgs.Contains("{seed}");
			txtWorldSeed.Enabled = supportsSeed;
			lblWorldSeed.ForeColor = supportsSeed ? Color.Black : Color.Gray;
			if (!supportsSeed) txtWorldSeed.Text = "N/A";

			// ⚔️ PVP/PVE LOCK: Only unlock if the game has more than 1 mode defined
			bool supportsModes = gameData.GameModes != null && gameData.GameModes.Count > 1;
			cmbCompetitive.Enabled = supportsModes;
			lblCompetitive.ForeColor = supportsModes ? Color.White : Color.Gray;

			// 📡 RCON LOCK: Only unlock if the game has RCON syntax defined
			bool supportsRcon = !string.IsNullOrWhiteSpace(gameData.RconSyntax);
			chkEnableRcon.Enabled = supportsRcon;
			if (!supportsRcon) chkEnableRcon.Checked = false;

			// 🗺️ MAP LOCK: Only unlock if the game has multiple maps
			bool supportsMaps = gameData.Maps != null && gameData.Maps.Count > 1;
			cmbWorldName.Enabled = supportsMaps;
		}
	}
}