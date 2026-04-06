using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Game_Server_Control_Panel
{
	public partial class ServerSettingsGUI : Form
	{
		public GameServer? NewServer { get; private set; }
		private bool isManualLoading = false;
		private bool _isEditMode = false;
		private GameServer? _existingServer = null;

		// Add (GameServer server) inside the parentheses here!
		// Change this line to make the server optional
		public ServerSettingsGUI(GameServer? server = null)
		{
			InitializeComponent();

			// This now works for both ADD (null) and EDIT (existing server)
			_existingServer = server;

			if (server != null)
			{
				_isEditMode = true;

				cmbGame.Enabled = true;
				chkDefaultPath.Enabled = false;
				txtInstallPath.Enabled = false;
				btnBrowse.Enabled = false;

				WarningLabel.Text = @"Warning! You cannot edit this after the server has been saved!
					If you used the Default Location and you changed the Server Name, 
					the folder name will be changed to:
					C:\Games\[Game Name]\[Your Server Name]";

				// Fill the GUI with existing data
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
		}

		private void ServerSettingsGUI_Load(object sender, EventArgs e)
		{
			cmbGame.SelectedIndexChanged -= cmbGame_SelectedIndexChanged;

			cmbGame.Items.Clear();
			cmbGame.Items.Add("-- Pick a Game --");
			foreach (var game in GameDatabase.GetGameList())
			{
				cmbGame.Items.Add(game.Game);
			}
			cmbGame.SelectedIndex = 0;

			if (btnSave.Text == "Update Server" && NewServer != null)
			{
				isManualLoading = true;

				txtName.Text = NewServer.ServerName;

				int gameIndex = cmbGame.FindStringExact(NewServer.Game);
				if (gameIndex != -1) cmbGame.SelectedIndex = gameIndex;

				numPort.Value = NewServer.Port;
				numQueryPort.Value = NewServer.QueryPort;
				txtPassword.Text = NewServer.Password;
				numMaxPlayers.Value = NewServer.MaxPlayers;
				txtExtraArgs.Text = NewServer.ExtraArgs;
				txtInstallPath.Text = NewServer.InstallPath;
				chkDefaultPath.Checked = NewServer.IsDefaultPath;

				// THE FIX: Repopulate Maps list so we can select the saved world
				var gameData = GameDatabase.GetGame(NewServer.Game);
				if (gameData != null)
				{
					cmbWorldName.Items.Clear();
					foreach (var map in gameData.Maps)
					{
						cmbWorldName.Items.Add(map);
					}
					// Select the saved world name
					cmbWorldName.Text = NewServer.WorldName;
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
				cmbWorldName.Items.Clear(); // Clear maps for new server until game is picked

				chkDefaultPath.Enabled = false;
				btnSave.Enabled = false;
				cmbGame.Enabled = true;
			}

			cmbGame.SelectedIndexChanged += cmbGame_SelectedIndexChanged;
			UpdateControlStates();
		}

		private void UpdatePathPreview()
		{
			// Don't mess with the path if we are just editing an existing server
			if (btnSave.Text == "Update Server") return;

			string selectedGame = cmbGame.SelectedItem?.ToString() ?? "";
			string serverName = txtName.Text.Trim();

			if (chkDefaultPath.Checked)
			{
				// ONLY build a path if we have a Game AND a Name
				if (cmbGame.SelectedIndex > 0 && !string.IsNullOrWhiteSpace(serverName))
				{
					// Result: C:\GameServers\Soulmask\Buffalo
					txtInstallPath.Text = Path.Combine(@"C:\GameServers", selectedGame, serverName);
				}
				else
				{
					// If one is missing, clear the path box so they don't think it's ready
					txtInstallPath.Text = string.Empty;
				}
			}
		}

		// Inside your ServerSettingsForm class
		public void FillFormForEditing(GameServer existingServer)
		{
			// 1. SET THE MODE AND SHIELD
			isManualLoading = true;
			btnSave.Text = "Update Server";

			// 2. REFILL Game List (Standard in your repo)
			cmbGame.Items.Clear();
			cmbGame.Items.Add("-- Pick a Game --");
			foreach (var g in GameDatabase.GetGameList())
			{
				cmbGame.Items.Add(g.Game);
			}

			// 3. LOAD SAVED SERVER DATA
			txtName.Text = existingServer.Game; // This stays "testing" because of the shield

			// Force Game Selection
			int gameIndex = cmbGame.FindStringExact(existingServer.Game);
			if (gameIndex != -1)
			{
				cmbGame.SelectedIndex = gameIndex;
			}

			// Load Numeric/Text Values (Corrected to your specific property names)
			numPort.Value = existingServer.Port;
			numQueryPort.Value = existingServer.QueryPort;
			txtPassword.Text = existingServer.Password;
			numMaxPlayers.Value = existingServer.MaxPlayers;
			txtExtraArgs.Text = existingServer.ExtraArgs;

			// 4. LOAD WORLD NAMES
			var gameData = GameDatabase.GetGame(existingServer.Game);
			if (gameData != null)
			{
				cmbWorldName.Items.Clear();
				foreach (var map in gameData.Maps)
				{
					cmbWorldName.Items.Add(map);
				}

				if (cmbWorldName.Items.Contains(existingServer.WorldName))
				{
					cmbWorldName.SelectedItem = existingServer.WorldName;
				}
				else
				{
					cmbWorldName.Text = existingServer.WorldName;
				}
			}

			// 5. LOAD PATHS
			chkDefaultPath.Checked = existingServer.IsDefaultPath;
			txtInstallPath.Text = existingServer.InstallPath;

			// 6. LOCK DOWN UI
			cmbGame.Enabled = false;
			chkDefaultPath.Enabled = false;
			btnBrowse.Enabled = false;
			txtInstallPath.Enabled = false;

			// 7. REMOVE SHIELD
			isManualLoading = false;

			// Final UI refresh
			UpdatePathPreview();
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			// 1. Validation: Don't allow empty names
			if (string.IsNullOrWhiteSpace(txtName.Text))
			{
				MessageBox.Show("Please enter a Server Name.", "Validation Error");
				return;
			}

			// 2. Port Conflict Check
			// We check the 'public static' list in MainGUI to see if anyone else is using this port
			int selectedPort = (int)numPort.Value;
			foreach (var s in MainGUI.serverList)
			{
				// Skip the check if we are editing the SAME server
				if (_isEditMode && s == _existingServer) continue;

				if (s.Port == selectedPort)
				{
					MessageBox.Show($"Port {selectedPort} is already in use by '{s.ServerName}'.", "Port Conflict");
					return;
				}
			}

			// 3. Create the New Server Data Object
			var NewServer = new GameServer
			{
				Game = cmbGame.Text,
				ServerName = txtName.Text,
				Port = (int)numPort.Value,
				QueryPort = (int)numQueryPort.Value,
				Password = txtPassword.Text,
				AdminPassword = txtAdminPassword.Text, // Matches Soulmask/Palworld logic
				MaxPlayers = (int)numMaxPlayers.Value,
				WorldName = cmbWorldName.Text,
				ExtraArgs = txtExtraArgs.Text,
				IsDefaultPath = chkDefaultPath.Checked,
				Status = _isEditMode ? _existingServer.Status : "Stopped",
				PID = _isEditMode ? _existingServer.PID : null
			};

			// 4. Handle Folder Logic (Create or Rename)
			string cleanGameName = NewServer.Game.Replace(" ", "_");
			string cleanServerName = NewServer.ServerName.Replace(" ", "_");
			string targetPath = chkDefaultPath.Checked
				? $@"C:\Games\{cleanGameName}\{cleanServerName}"
				: txtInstallPath.Text;

			try
			{
				if (_isEditMode && _existingServer != null)
				{
					// RENAME LOGIC: If the path changed, move the physical folder
					if (_existingServer.InstallPath != targetPath && Directory.Exists(_existingServer.InstallPath))
					{
						ServerManager.RenameServerFolder(_existingServer, NewServer);
						MainGUI.Instance?.AppendLog($"[RENAME] Folder moved to: {targetPath}");
					}

					// Update the existing server in the static list (Surgical Swap)
					int index = MainGUI.serverList.IndexOf(_existingServer);
					NewServer.InstallPath = targetPath;
					MainGUI.serverList[index] = NewServer;
				}
				else
				{
					// ADD LOGIC: Create brand new folders
					NewServer.InstallPath = targetPath;
					ServerManager.CreateFolders(targetPath);
					MainGUI.serverList.Add(NewServer);
					MainGUI.Instance?.AppendLog($"[NEW] Server '{NewServer.ServerName}' added.");
				}

				// 5. Final Save to JSON
				// Calls the static method we fixed in MainGUI
				MainGUI.SaveServersToDisk();

				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Operation failed: {ex.Message}", "File Error");
			}
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			using (var fbd = new FolderBrowserDialog())
			{
				if (fbd.ShowDialog() == DialogResult.OK)
				{
					txtInstallPath.Text = fbd.SelectedPath;
				}
			}
		}

		private void chkDefaultPath_CheckedChanged(object sender, EventArgs e)
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
			// 1. Get the current values
			string serverName = txtName.Text.Trim();
			bool isGamePicked = cmbGame.SelectedIndex > 0;

			// 2. STRICT RULE: Name must be at least 1 character long
			bool hasValidName = !string.IsNullOrWhiteSpace(serverName);

			// 3. ONLY unlock the location options if BOTH are true
			bool canSelectLocation = isGamePicked && hasValidName;

			chkDefaultPath.Enabled = canSelectLocation;

			// 4. Handle the Browse/Path box visibility
			// They only unlock if location is allowed AND "Default" is unchecked
			bool manualPathMode = canSelectLocation && !chkDefaultPath.Checked;
			btnBrowse.Enabled = manualPathMode;
			txtInstallPath.Enabled = manualPathMode;

			// 5. The SAVE button stays dead until we have both a name and a game
			btnSave.Enabled = canSelectLocation;
		}

		private void cmbGame_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (isManualLoading || cmbGame.SelectedIndex <= 0) return;

			var gameData = GameDatabase.GetGame(cmbGame.SelectedItem.ToString());

			if (gameData != null)
			{
				numPort.Value = gameData.Port;
				numQueryPort.Value = gameData.QueryPort;
				txtExtraArgs.Text = gameData.ExtraArgs;

				cmbWorldName.Items.Clear();

				// CHANGED: Use .Count instead of .Length for List<string>
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

		private void txtName_TextChanged(object sender, EventArgs e)
		{
			UpdateControlStates();
			UpdatePathPreview();
		}
	}
}
