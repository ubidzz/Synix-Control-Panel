using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
			// If we are already in Update mode, do not reset the dropdown
			if (btnSave.Text == "Update Server")
			{
				return;
			}

			// Default setup for NEW servers
			cmbGame.SelectedIndexChanged -= cmbGame_SelectedIndexChanged;

			cmbGame.Items.Clear();
			cmbGame.Items.Add("-- Pick a Game --");
			foreach (var game in GameDatabase.GetGameList())
			{
				cmbGame.Items.Add(game.Name);
			}

			cmbGame.SelectedIndex = 0;

			// Only listen for changes if it's a new server
			cmbGame.SelectedIndexChanged += cmbGame_SelectedIndexChanged;
		}

		private void UpdatePathPreview()
		{
			// 1. Define what "Ready" looks like
			bool hasGame = cmbGame.SelectedIndex != -1 && cmbGame.Text != "Pick Game";
			bool hasName = !string.IsNullOrWhiteSpace(txtName.Text);

			// 2. Only unlock the checkbox if BOTH are true
			if (hasGame && hasName)
			{
				chkDefaultPath.Enabled = true;
			}
			else
			{
				chkDefaultPath.Enabled = false;
				chkDefaultPath.Checked = false; // Force it off if they delete the name
			}

			// 3. Handle the path string if the box is checked
			if (chkDefaultPath.Checked)
			{
				string gameFolder = cmbGame.Text.Replace(" ", "_");
				string serverFolder = txtName.Text.Replace(" ", "_");
				txtInstallPath.Text = $@"C:\Games\{gameFolder}\{serverFolder}";
			}
		}

		// Inside your ServerSettingsForm class
		public void FillFormForEditing(GameServer existingServer)
		{
			// 1. SET THE MODE FIRST
			btnSave.Text = "Update Server";

			// 2. SILENCE selection events so defaults don't overwrite saved data
			cmbGame.SelectedIndexChanged -= cmbGame_SelectedIndexChanged;

			// 3. REFILL Game List
			cmbGame.Items.Clear();
			cmbGame.Items.Add("-- Pick a Game --");
			foreach (var g in GameDatabase.GetGameList())
			{
				cmbGame.Items.Add(g.Name);
			}

			// 4. LOAD SAVED SERVER DATA
			txtName.Text = existingServer.Name;

			// Force Game Selection
			int gameIndex = cmbGame.FindStringExact(existingServer.Game);
			if (gameIndex != -1)
			{
				cmbGame.SelectedIndex = gameIndex;
			}

			// Load Numeric/Text Values
			numPort.Value = existingServer.Port;
			numQueryPort.Value = existingServer.QueryPort;
			txtPassword.Text = existingServer.Password;
			numMaxPlayers.Value = existingServer.MaxPlayers;
			txtExtraArgs.Text = existingServer.ExtraArgs;

			// 5. LOAD WORLD NAMES (The Fix for the missing World Name)
			var gameData = GameDatabase.GetGame(existingServer.Game);
			if (gameData != null)
			{
				cmbWorldName.Items.Clear();
				foreach (var map in gameData.Maps)
				{
					cmbWorldName.Items.Add(map);
				}

				// Set the specific saved map
				if (cmbWorldName.Items.Contains(existingServer.WorldName))
				{
					cmbWorldName.SelectedItem = existingServer.WorldName;
				}
				else
				{
					cmbWorldName.Text = existingServer.WorldName;
				}
			}

			// 6. LOAD PATHS
			chkDefaultPath.Checked = existingServer.IsDefaultPath;
			txtInstallPath.Text = existingServer.InstallPath;

			// 7. LOCK DOWN UI (Prevents breaking folder structures)
			cmbGame.Enabled = false;        // Locked
			chkDefaultPath.Enabled = false; // Locked
			btnBrowse.Enabled = false;      // Locked
			txtInstallPath.Enabled = false; // Locked

			// 8. RESTORE EVENT (Logic won't fire because cmbGame is disabled)
			cmbGame.SelectedIndexChanged += cmbGame_SelectedIndexChanged;

			// Final UI refresh
			UpdatePathPreview();
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			// 1. Safety Guard: Must pick a game
			if (cmbGame.SelectedIndex == -1 || cmbGame.Text == "Pick Game")
			{
				MessageBox.Show("Please select a game server from the list!", "Selection Required");
				return;
			}

			// 2. Dynamic Path Calculation
			string finalPath = txtInstallPath.Text;
			if (chkDefaultPath.Checked)
			{
				// Clean the names (remove spaces/special chars) to avoid Windows naming errors
				string gameFolder = cmbGame.Text.Replace(" ", "_");
				string serverFolder = txtName.Text.Replace(" ", "_");
				finalPath = $@"C:\Games\{gameFolder}\{serverFolder}";
			}

			// 3. Package the Data
			NewServer = new GameServer
			{
				Name = txtName.Text,
				Game = cmbGame.Text,
				Port = (int)numPort.Value,
				QueryPort = (int)numQueryPort.Value,
				Password = txtPassword.Text,
				MaxPlayers = (int)numMaxPlayers.Value,
				WorldName = cmbWorldName.Text,
				ExtraArgs = txtExtraArgs.Text,
				InstallPath = finalPath,
				IsDefaultPath = chkDefaultPath.Checked,
				Status = "Stopped"
			};

			// If this is a NEW server (the button text is "Save" and not "Update")
			if (btnSave.Text == "Save Server")
			{
				// We set the DialogResult so Form1 knows to add it to the list
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			else
			{
				// Logic for Updating an existing server
				this.DialogResult = DialogResult.OK;
				this.Close();
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
			// MUST HAVE: A name typed AND a real game picked (not index 0)
			bool hasName = !string.IsNullOrWhiteSpace(txtName.Text);
			bool isGamePicked = cmbGame.SelectedIndex > 0;
			bool canEnableLocation = hasName && isGamePicked;

			// Set the states
			chkDefaultPath.Enabled = canEnableLocation;

			// Browse/Path only unlock if both are true AND "Default Location" is NOT checked
			bool browseState = canEnableLocation && !chkDefaultPath.Checked;
			btnBrowse.Enabled = browseState;
			txtInstallPath.Enabled = browseState;
		}

		private void cmbGame_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Do nothing if no game is selected or if we are in Edit mode
			if (cmbGame.SelectedIndex <= 0 || btnSave.Text == "Update Server")
			{
				return;
			}

			var gameData = GameDatabase.GetGame(cmbGame.SelectedItem.ToString());
			if (gameData != null)
			{
				// Apply Factory Defaults for NEW servers
				numPort.Value = gameData.DefaultPort;
				numQueryPort.Value = gameData.DefaultQueryPort;
				txtExtraArgs.Text = gameData.DefaultArgs;

				// Fill Map Dropdown
				cmbWorldName.Items.Clear();
				foreach (var map in gameData.Maps)
				{
					cmbWorldName.Items.Add(map);
				}
				if (cmbWorldName.Items.Count > 0) cmbWorldName.SelectedIndex = 0;
			}

			UpdateControlStates();
			UpdatePathPreview();
		}

		private void txtName_TextChanged(object sender, EventArgs e)
		{
			UpdateControlStates();
		}
	}
}
