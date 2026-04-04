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
			// 1. COMPLETELY WIPE any hard-coded items from the Designer
			cmbGame.Items.Clear();

			// 2. Loop through your neat GameDatabase list
			foreach (var game in GameDatabase.GetGameList())
			{
				cmbGame.Items.Add(game.Name);
			}

			// 3. Set the default display
			if (cmbGame.Items.Count > 0)
			{
				cmbGame.SelectedIndex = 0;
			}
			else
			{
				cmbGame.Text = "Pick Game";
			}
		}

		private void cmbGame_SelectedIndexChanged(object sender, EventArgs e)
		{
			// 1. Check if they picked something other than the default text
			if (cmbGame.SelectedIndex != -1 && cmbGame.Text != "Pick Game")
			{
				chkDefaultPath.Enabled = true;

				// --- NEW AUTO-FILL LOGIC ---
				// Only pull defaults if we are ADDING a new server (Save Mode)
				if (btnSave.Text == "Save Server")
				{
					var selectedGameName = cmbGame.SelectedItem.ToString();
					var gameData = GameDatabase.GetGame(selectedGameName);

					if (gameData != null)
					{
						// Pull the specific defaults from the Database list
						numPort.Value = gameData.DefaultPort;
						numQueryPort.Value = gameData.DefaultQueryPort;
						txtExtraArgs.Text = gameData.DefaultArgs;
					}
				}
				// ----------------------------
			}
			else
			{
				chkDefaultPath.Enabled = false;
				chkDefaultPath.Checked = false;
			}

			// 2. Refresh the path preview
			UpdatePathPreview();
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
			// 1. Basic Info
			txtName.Text = existingServer.Name;

			// 2. Lock the Game Selection (Changing games breaks the server install)
			cmbGame.Text = existingServer.Game;
			cmbGame.Enabled = false;

			// 3. Location Settings - LOCK DOWN
			chkDefaultPath.Checked = existingServer.IsDefaultPath;
			txtInstallPath.Text = existingServer.InstallPath;

			// These must be false so the user can't manually break the link to the files
			chkDefaultPath.Enabled = false;
			txtInstallPath.Enabled = false;
			btnBrowse.Enabled = false;

			// 4. Server Configuration (Still editable)
			numPort.Value = existingServer.Port;
			numQueryPort.Value = existingServer.QueryPort;
			txtPassword.Text = existingServer.Password;
			numMaxPlayers.Value = existingServer.MaxPlayers;
			cmbWorldName.Text = existingServer.WorldName;
			txtExtraArgs.Text = existingServer.ExtraArgs;

			// 5. Visual Cue
			// Change the button text so they know they are updating, not adding
			btnSave.Text = "Update Server";
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

		private void cmbGame_SelectedIndexChanged_1(object sender, EventArgs e)
		{
			// 1. Check if a valid game is picked
			if (cmbGame.SelectedIndex != -1 && cmbGame.Text != "Pick Game")
			{
				chkDefaultPath.Enabled = true;

				// Get the data for the selected game from our Database
				var selectedGameName = cmbGame.SelectedItem.ToString();
				var gameData = GameDatabase.GetGame(selectedGameName);

				if (gameData != null)
				{
					// 2. Populate the Map Dropdown (cmbWorldName)
					cmbWorldName.Items.Clear();
					foreach (var map in gameData.Maps)
					{
						cmbWorldName.Items.Add(map);
					}

					// Auto-select the first map if available
					if (cmbWorldName.Items.Count > 0) cmbWorldName.SelectedIndex = 0;

					// 3. Auto-fill Ports and Args (ONLY if we are adding a NEW server)
					if (btnSave.Text == "Save Server")
					{
						numPort.Value = gameData.DefaultPort;
						numQueryPort.Value = gameData.DefaultQueryPort;
						txtExtraArgs.Text = gameData.DefaultArgs;
					}
				}
			}
			else
			{
				// Reset if no game is selected
				chkDefaultPath.Enabled = false;
				chkDefaultPath.Checked = false;
				cmbWorldName.Items.Clear();
			}

			// 4. Update the folder path preview label
			UpdatePathPreview();
		}
	}
}
