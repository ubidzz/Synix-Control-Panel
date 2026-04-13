/*
 * Copyright (c) 2026 ubidzz. All Rights Reserved.
 *
 * This file is part of Synix Control Panel.
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

		private bool isManualLoading = false; // 🎯 THE SHIELD: Stops crashes during Load
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
			if (AppPortNumeric != null)
				AppPortNumeric.Value = Math.Clamp((decimal)_existingServer.AppPort, AppPortNumeric.Minimum, AppPortNumeric.Maximum);

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

			string newPath = txtInstallPath.Text.Trim();
			if (_isEditMode && !string.IsNullOrEmpty(_oldPath) && _oldPath != newPath)
			{
				try { if (Directory.Exists(_oldPath)) Directory.Move(_oldPath, newPath); }
				catch (Exception ex) { MessageBox.Show($"Move failed: {ex.Message}"); }
			}

			NewServer = new GameServer
			{
				Game = selectedGame,
				ServerName = newName,
				Port = (int)numPort.Value,
				QueryPort = (int)numQueryPort.Value,
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
				RconPort = (int)numRconPort.Value,
				RconPassword = txtRconPassword.Text,
				InstallPath = newPath,
				IsScheduledRestartEnabled = chkEnableSchedule.Checked,
				RestartTime = _selectedTime,
				RestartDays = (bool[])_selectedDays.Clone(),
				Status = _existingServer?.Status ?? StatusManager.GetStatus(ServerState.Stopped),
				AppPort = (int)(AppPortNumeric?.Value ?? 8777)
			};

			try
			{
				if (_isEditMode && _existingServer != null)
				{
					int index = -1;
					for (int i = 0; i < MainGUI.serverList.Count; i++)
					{
						if (MainGUI.serverList[i].ServerName == _existingServer.ServerName) { index = i; break; }
					}
					if (index != -1) MainGUI.serverList[index] = NewServer;
				}
				else MainGUI.serverList.Add(NewServer);

				FileHandler.SaveServers();
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			catch (Exception ex) { MessageBox.Show($"Operation failed: {ex.Message}"); }
		}

		private void SyncGatekeeper()
		{
			if (isManualLoading) return; // 🎯 BLOCK logic during Load

			bool hasName = !string.IsNullOrWhiteSpace(txtName.Text);
			bool hasGame = cmbGame.SelectedIndex > 0;
			bool isBaseReady = hasName && hasGame;

			if (_isEditMode)
			{
				// 🎯 PRO TEXT: Ready state for editing
				WarningLabel.Text = $"[READY] Editing existing server: {txtName.Text}. Game and Path modifications are restricted.";
				WarningLabel.ForeColor = Color.LimeGreen;

				chkDefaultPath.Enabled = false;
				btnBrowse.Enabled = false;
				txtInstallPath.Enabled = false;
				btnSave.Enabled = true;
			}
			else if (!isBaseReady)
			{
				WarningLabel.Text = "[LOCKED] Required: Provide a Server Name and select a Game Template to unlock configuration.";
				WarningLabel.ForeColor = Color.Orange;
				chkDefaultPath.Enabled = chkEnableSchedule.Enabled = chkUpdateOnStart.Enabled = false;
				btnBrowse.Enabled = txtInstallPath.Enabled = btnSave.Enabled = false;
			}
			else
			{
				string safeName = Regex.Replace(txtName.Text.Trim(), @"[^a-zA-Z0-9_\-]", "_");
				string selectedGame = cmbGame.Text.Trim();
				string displayPath = $@"C:\Synix\Games\{selectedGame}\{safeName}";

				WarningLabel.Text = $"[READY] Configuration active. Specify a custom directory or use the default: {displayPath}";
				WarningLabel.ForeColor = Color.LimeGreen;

				chkDefaultPath.Enabled = chkEnableSchedule.Enabled = chkUpdateOnStart.Enabled = true;

				if (chkDefaultPath.Checked)
				{
					txtInstallPath.Text = displayPath;
					btnBrowse.Enabled = txtInstallPath.Enabled = false;
				}
				else
				{
					btnBrowse.Enabled = txtInstallPath.Enabled = true;
				}
				btnSave.Enabled = !string.IsNullOrWhiteSpace(txtInstallPath.Text);
			}

			if (btnEditSchedule != null) btnEditSchedule.Enabled = isBaseReady && chkEnableSchedule.Checked;
		}

		private void ToggleGameSpecificFields(GameInfo? gameData)
		{
			if (gameData == null)
			{
				txtPassword.Enabled = txtAdminPassword.Enabled = txtWorldSeed.Enabled = cmbCompetitive.Enabled =
				chkEnableRcon.Enabled = AppPortNumeric.Enabled = numMaxPlayers.Enabled = numQueryPort.Enabled =
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
			if (AppPortNumeric != null) AppPortNumeric.Enabled = args.Contains("{app_port}");
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