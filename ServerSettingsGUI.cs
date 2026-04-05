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
		public GameServer NewServer { get; set; }
		private bool isManualLoading = false;

		public ServerSettingsGUI()
		{
			InitializeComponent();
			cmbGame.Enabled = true;
			chkDefaultPath.Enabled = false;
			txtInstallPath.Enabled = false;
			btnBrowse.Enabled = false;

			WarningLabel.Text = @"Warning! You cannot edit this after the server has been saved!
				If you used the Default Location and you changed the Server Name, 
				the folder name will be changed to:
				C:\Games\[Game Name]\[Your Server Name]";
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
			if (cmbGame.SelectedIndex == -1) return;

			// 1. Calculate Path
			string finalPath = txtInstallPath.Text;
			if (chkDefaultPath.Checked)
			{
				string gameFolder = cmbGame.Text.Replace(" ", "_");
				string serverFolder = txtName.Text.Replace(" ", "_");
				finalPath = $@"C:\Games\{gameFolder}\{serverFolder}";
			}

			// 2. Fetch the "Master Template" from GameDatabase
			var gameData = GameDatabase.GetGame(cmbGame.Text);

			// 3. Create the Server Object with ALL data points
			NewServer = new GameServer
			{
				// User Inputs
				ServerName = txtName.Text,
				Password = txtPassword.Text,
				AdminPassword = txtAdminPassword.Text, // Captures the new input
				Port = (int)numPort.Value,
				QueryPort = (int)numQueryPort.Value,
				MaxPlayers = (int)numMaxPlayers.Value,
				WorldName = cmbWorldName.Text,
				InstallPath = finalPath,

				// Database Defaults
				Game = cmbGame.Text,
				AppID = gameData?.AppID ?? "0",
				ExeName = gameData?.ExeName ?? "",
				Maps = gameData?.Maps ?? new List<string>(),
				Status = "Stopped"
			};

			this.DialogResult = DialogResult.OK;
			this.Close();
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
