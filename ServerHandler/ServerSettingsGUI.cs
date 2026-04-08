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
using Synix_Control_Panel.SynixEngine; // 👈 Namespace for the Brain
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

			if (server != null)
			{
				_isEditMode = true;
				cmbGame.Enabled = true;
				chkDefaultPath.Enabled = false;
				txtInstallPath.Enabled = false;
				btnBrowse.Enabled = false;

				WarningLabel.Text = "Warning! You cannot edit location after the server has been saved!";

				txtName.Text = server.ServerName;
				cmbGame.Text = server.Game;
				txtPassword.Text = server.Password;
				txtAdminPassword.Text = server.AdminPassword;
				numPort.Value = server.Port;
				numQueryPort.Value = server.QueryPort;
				numMaxPlayers.Value = server.MaxPlayers;
				cmbWorldName.Text = server.WorldName;
				txtInstallPath.Text = server.InstallPath;
				chkDefaultPath.Checked = server.IsDefaultPath;
				txtExtraArgs.Text = server.ExtraArgs;
				cmbCompetitive.Text = server.GameMode;
			}
			else
			{
				_isEditMode = false;
				WarningLabel.Text = "[WARNING] Pick a location to install or use default. This cannot be changed later!";
			}
		}

		private void ServerSettingsGUI_Load(object? sender, EventArgs e)
		{
			cmbGame.SelectedIndexChanged -= cmbGame_SelectedIndexChanged;

			cmbGame.Items.Clear();
			cmbGame.Items.Add("-- Pick a Game --");
			foreach (var game in GameDatabase.GetGameList())
			{
				cmbGame.Items.Add(game.Game);
			}
			cmbGame.SelectedIndex = 0;

			if (_isEditMode && _existingServer != null)
			{
				isManualLoading = true;
				txtName.Text = _existingServer.ServerName;
				int gameIndex = cmbGame.FindStringExact(_existingServer.Game);
				if (gameIndex != -1) cmbGame.SelectedIndex = gameIndex;

				numPort.Value = _existingServer.Port;
				numQueryPort.Value = _existingServer.QueryPort;
				txtPassword.Text = _existingServer.Password;
				numMaxPlayers.Value = _existingServer.MaxPlayers;
				txtExtraArgs.Text = _existingServer.ExtraArgs;
				txtInstallPath.Text = _existingServer.InstallPath;
				chkDefaultPath.Checked = _existingServer.IsDefaultPath;
				chkEnableRcon.Checked = _existingServer.EnableRcon;
				numRconPort.Value = _existingServer.RconPort > 0 ? _existingServer.RconPort : 0;
				txtRconPassword.Text = _existingServer.RconPassword ?? "";

				var gameData = GameDatabase.GetGame(_existingServer.Game);
				if (gameData != null)
				{
					PopulateMaps(gameData, _existingServer.WorldName);
					PopulateGameModes(gameData, _existingServer.GameMode);
				}

				cmbGame.Enabled = false;
				chkDefaultPath.Enabled = false;
				txtInstallPath.Enabled = false;
				btnBrowse.Enabled = false;
				isManualLoading = false;
			}
			else
			{
				btnSave.Text = "Save Server";
				cmbCompetitive.SelectedItem = "PVE";
			}

			cmbGame.SelectedIndexChanged += cmbGame_SelectedIndexChanged;
			UpdateControlStates();
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

		private void UpdatePathPreview()
		{
			if (_isEditMode || !chkDefaultPath.Checked) return;

			string selectedGame = cmbGame.SelectedItem?.ToString() ?? "";
			string serverName = txtName.Text.Trim();

			if (cmbGame.SelectedIndex > 0 && !string.IsNullOrWhiteSpace(serverName))
			{
				string cleanGame = selectedGame.Replace(" ", "_");
				string cleanName = serverName.Replace(" ", "_");
				txtInstallPath.Text = Path.Combine(@"C:\Games", cleanGame, cleanName);
			}
			else txtInstallPath.Text = string.Empty;
		}

		private void btnBrowse_Click(object? sender, EventArgs e)
		{
			using var fbd = new FolderBrowserDialog();
			if (fbd.ShowDialog() == DialogResult.OK)
			{
				txtInstallPath.Text = fbd.SelectedPath;
				UpdateControlStates();
			}
		}

		private void chkDefaultPath_CheckedChanged(object? sender, EventArgs e)
		{
			txtInstallPath.Enabled = !chkDefaultPath.Checked;
			btnBrowse.Enabled = !chkDefaultPath.Checked;
			if (chkDefaultPath.Checked) UpdatePathPreview();
			UpdateControlStates();
		}

		private void UpdateControlStates()
		{
			bool isGamePicked = cmbGame.SelectedIndex > 0;
			bool hasName = !string.IsNullOrWhiteSpace(txtName.Text);
			chkDefaultPath.Enabled = isGamePicked && hasName;
			bool hasLocation = chkDefaultPath.Checked || !string.IsNullOrWhiteSpace(txtInstallPath.Text);
			btnSave.Enabled = isGamePicked && hasName && hasLocation;
		}

		private void cmbGame_SelectedIndexChanged(object? sender, EventArgs e)
		{
			if (isManualLoading || cmbGame.SelectedIndex <= 0) return;

			var gameData = GameDatabase.GetGame(cmbGame.SelectedItem?.ToString() ?? "");
			if (gameData != null)
			{
				numPort.Value = gameData.Port;
				numQueryPort.Value = gameData.QueryPort;
				txtExtraArgs.Text = gameData.ExtraArgs;
				PopulateMaps(gameData, gameData.Maps.FirstOrDefault() ?? "");
				PopulateGameModes(gameData, "PVE");
				UpdatePathPreview();
			}
			UpdateControlStates();
		}

		private void txtName_TextChanged(object? sender, EventArgs e)
		{
			UpdateControlStates();
			UpdatePathPreview();
		}

		private void btnSave_Click(object? sender, EventArgs e)
		{
			string newName = txtName.Text.Trim();
			string selectedGame = cmbGame.Text;

			if (string.IsNullOrWhiteSpace(newName)) return;

			// 1. Let the Engine handle the Name Check & MessageBox
			if (!Core.Instance.ValidateNameAndReport(newName, selectedGame, _existingServer)) return;

			// 2. Let the Engine handle the Port Checks & MessageBox
			if (!Core.Instance.ValidatePortsAndReport((int)numPort.Value, (int)numQueryPort.Value, (int)numRconPort.Value, chkEnableRcon.Checked, _existingServer)) return;

			// Path calculation
			string cleanGame = selectedGame.Replace(" ", "_");
			string cleanName = newName.Replace(" ", "_");
			string targetPath = chkDefaultPath.Checked ? $@"C:\Games\{cleanGame}\{cleanName}" : txtInstallPath.Text;

			// 3. Let the Engine handle the Folder Check & MessageBox
			if (!Core.Instance.ValidateFolderAndReport(targetPath, _isEditMode)) return;

			// --- ALL CHECKS PASSED ---
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
				ExtraArgs = txtExtraArgs.Text,
				IsDefaultPath = chkDefaultPath.Checked,
				Status = _isEditMode && _existingServer != null ? _existingServer.Status : "Offline",
				PID = _isEditMode && _existingServer != null ? _existingServer.PID : null,
				GameMode = cmbCompetitive.Text,
				EnableRcon = chkEnableRcon.Checked,
				RconPort = (int)numRconPort.Value,
				RconPassword = txtRconPassword.Text,
				InstallPath = targetPath
			};

			try
			{
				if (_isEditMode && _existingServer != null)
				{
					if (_existingServer.InstallPath != targetPath && Directory.Exists(_existingServer.InstallPath))
					{
						ServerFolder.Rename(_existingServer, NewServer);
						MainGUI.Instance?.AppendLog($"[RENAME] Folder moved to: {targetPath}");
					}
					int index = MainGUI.serverList.IndexOf(_existingServer);
					if (index != -1) MainGUI.serverList[index] = NewServer;
				}
				else
				{
					FolderHandler.Create(targetPath);
					MainGUI.serverList.Add(NewServer);
					MainGUI.Instance?.AppendLog($"[NEW] Server '{NewServer.ServerName}' added.");
				}

				FileHandler.SaveServers();
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			catch (Exception ex)
			{
				// You can move this catch block to a Core.ReportError(ex) if you want even more centralization
				MessageBox.Show($"Operation failed: {ex.Message}", "File Error");
			}
		}
	}
}