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
using Synix_Control_Panel.FileFolderHandler;
using Synix_Control_Panel.ServerHandler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Synix_Control_Panel
{
	public partial class ServerSettingsGUI : Form
	{
		// 1. Properly marked as nullable (?) to prevent CS8618 and CS8625
		public GameServer? NewServer { get; private set; }

		private bool isManualLoading = false;
		private bool _isEditMode = false;
		private GameServer? _existingServer = null;

		// 2. The Unified Constructor (Handles both Add and Edit)
		public ServerSettingsGUI(GameServer? server = null)
		{
			InitializeComponent();

			_existingServer = server;

			// FIX: Check 'server' directly instead of '_existingServer' to satisfy the compiler's null-flow analysis
			if (server != null)
			{
				_isEditMode = true;

				cmbGame.Enabled = true;
				chkDefaultPath.Enabled = false;
				txtInstallPath.Enabled = false;
				btnBrowse.Enabled = false;

				WarningLabel.Text = "Warning! You cannot edit this after the server has been saved!\n" +
								   "If you used the Default Folder Location and you changed the " +
								   "Server Name the location and name will be changed to:\n" +
								   @"C:\Games\[Game Name]\[Your Server Name]";

				// Fill the GUI with existing data. Compiler now knows 'server' is not null!
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
			}
			else
			{
				_isEditMode = false;
				WarningLabel.Text = "[WARNING] Make sure to pick a place on your pc to install the server\n" +
									"or use the default location because you can't change this later!\n" +
								   "[INFO] If you used the Default Folder Location, the folder name and\n" +
								   "the location will be:\n" +
								   @"C:\Games\[Game Name]\[Your Server Name]";
			}
		}

		// 3. Nullable 'sender' (object?) prevents CS8022 delegate warnings
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

				// Repopulate Maps list so we can select the saved world
				var gameData = GameDatabase.GetGame(_existingServer.Game);
				if (gameData != null && gameData.Maps != null)
				{
					cmbWorldName.Items.Clear();
					foreach (var map in gameData.Maps)
					{
						cmbWorldName.Items.Add(map);
					}
					if (cmbWorldName.Items.Contains(_existingServer.WorldName))
					{
						cmbWorldName.SelectedItem = _existingServer.WorldName;
					}
					else
					{
						cmbWorldName.Text = _existingServer.WorldName;
					}
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
				txtName.Text = string.Empty;
				txtInstallPath.Text = string.Empty;
				cmbWorldName.Items.Clear();

				chkDefaultPath.Enabled = false;
				btnSave.Enabled = false;
				cmbGame.Enabled = true;
			}

			cmbGame.SelectedIndexChanged += cmbGame_SelectedIndexChanged;
			UpdateControlStates();
		}

		private void UpdatePathPreview()
		{
			if (_isEditMode) return; // Don't mess with the path if editing

			string selectedGame = cmbGame.SelectedItem?.ToString() ?? "";
			string serverName = txtName.Text.Trim();

			if (chkDefaultPath.Checked)
			{
				if (cmbGame.SelectedIndex > 0 && !string.IsNullOrWhiteSpace(serverName))
				{
					string cleanGameName = selectedGame.Replace(" ", "_");
					string cleanServerName = serverName.Replace(" ", "_");
					txtInstallPath.Text = Path.Combine(@"C:\Games", cleanGameName, cleanServerName);
				}
				else
				{
					txtInstallPath.Text = string.Empty;
				}
			}
		}

		private void btnSave_Click(object? sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(txtName.Text))
			{
				MessageBox.Show("Please enter a Server Name.", "Validation Error");
				return;
			}

			// 1. Create the server object using ONLY user-defined data
			// We leave out AppID, ExeName, RequiredArgs, and Maps because 
			// those are pulled from the GameDatabase at runtime.
			NewServer = new GameServer
			{
				Game = cmbGame.Text,
				ServerName = txtName.Text,
				Port = (int)numPort.Value,
				QueryPort = (int)numQueryPort.Value,
				Password = txtPassword.Text,
				AdminPassword = txtAdminPassword.Text,
				MaxPlayers = (int)numMaxPlayers.Value,
				WorldName = cmbWorldName.Text,
				ExtraArgs = txtExtraArgs.Text,
				IsDefaultPath = chkDefaultPath.Checked,
				Status = _isEditMode && _existingServer != null ? _existingServer.Status : "Offline",
				PID = _isEditMode && _existingServer != null ? _existingServer.PID : null
			};

			// 2. Calculate the target path
			string cleanGameName = NewServer.Game.Replace(" ", "_");
			string cleanServerName = NewServer.ServerName.Replace(" ", "_");
			string targetPath = chkDefaultPath.Checked ? $@"C:\Games\{cleanGameName}\{cleanServerName}" : txtInstallPath.Text;

			// 3. CRITICAL: Assign the path to NewServer BEFORE the rename logic runs
			// This fixes the "Parameter 'destDirName' cannot be empty" error.
			NewServer.InstallPath = targetPath;

			try
			{
				if (_isEditMode && _existingServer != null)
				{
					// 4. Check if the folder needs to be renamed
					if (_existingServer.InstallPath != targetPath && Directory.Exists(_existingServer.InstallPath))
					{
						// This calls your logic to physically move the folder on the hard drive
						RenameServerFolder.Rename(_existingServer, NewServer);
						MainGUI.Instance?.AppendLog($"[RENAME] Folder moved to: {targetPath}");
					}

					// Update the existing item in your main list
					int index = MainGUI.serverList.IndexOf(_existingServer);
					if (index != -1) MainGUI.serverList[index] = NewServer;
				}
				else
				{
					// Logic for a brand new server
					CreateFolders.Create(targetPath);
					MainGUI.serverList.Add(NewServer);
					MainGUI.Instance?.AppendLog($"[NEW] Server '{NewServer.ServerName}' added.");
				}

				// 5. Save the updated list to servers.json
				CreateFiles.SaveServers();
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Operation failed: {ex.Message}", "File Error");
			}
		}

		private void btnBrowse_Click(object? sender, EventArgs e)
		{
			using var fbd = new FolderBrowserDialog();
			if (fbd.ShowDialog() == DialogResult.OK)
			{
				txtInstallPath.Text = fbd.SelectedPath;
			}
		}

		private void chkDefaultPath_CheckedChanged(object? sender, EventArgs e)
		{
			if (chkDefaultPath.Checked)
			{
				txtInstallPath.Enabled = false;
				btnBrowse.Enabled = false;
				UpdatePathPreview();
			}
			else
			{
				txtInstallPath.Enabled = true;
				btnBrowse.Enabled = true;
			}
		}

		private void UpdateControlStates()
		{
			string serverName = txtName.Text.Trim();
			bool isGamePicked = cmbGame.SelectedIndex > 0;
			bool hasValidName = !string.IsNullOrWhiteSpace(serverName);
			bool canSelectLocation = isGamePicked && hasValidName;

			chkDefaultPath.Enabled = canSelectLocation;

			bool manualPathMode = canSelectLocation && !chkDefaultPath.Checked;
			btnBrowse.Enabled = manualPathMode;
			txtInstallPath.Enabled = manualPathMode;

			btnSave.Enabled = canSelectLocation;
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

				cmbWorldName.Items.Clear();

				if (gameData.Maps != null && gameData.Maps.Count > 0)
				{
					foreach (var map in gameData.Maps)
					{
						cmbWorldName.Items.Add(map);
					}
					cmbWorldName.SelectedIndex = 0;
				}

				UpdatePathPreview();
			}

			UpdateControlStates();
		}

		private void txtName_TextChanged(object? sender, EventArgs e)
		{
			UpdateControlStates();
			UpdatePathPreview();
		}
	}
}