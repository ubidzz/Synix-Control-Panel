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
			cmbGame.Items.Clear();

			// Add the blank placeholder
			cmbGame.Items.Add("-- Pick a Game --");

			foreach (var game in GameDatabase.GetGameList())
			{
				cmbGame.Items.Add(game.Name);
			}

			// Force it to start on the blank placeholder
			cmbGame.SelectedIndex = 0;
		}

		private void cmbGame_SelectedIndexChanged_1(object sender, EventArgs e)
		{
			bool isGamePicked = cmbGame.SelectedIndex != -1 && cmbGame.Text != "Pick Game";

			// Unlock the controls
			chkDefaultPath.Enabled = isGamePicked;

			// If a game is picked and they HAVEN'T checked "Default Path", unlock Browse
			btnBrowse.Enabled = isGamePicked && !chkDefaultPath.Checked;
			txtInstallPath.Enabled = isGamePicked && !chkDefaultPath.Checked;

			if (isGamePicked)
			{
				// ... (Keep your existing Map and Port auto-fill logic here) ...
			}

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
			isManualLoading = true; // START SILENCE

			txtName.Text = existingServer.Name;
			cmbGame.SelectedItem = existingServer.Game;

			chkDefaultPath.Checked = existingServer.IsDefaultPath;
			txtInstallPath.Text = existingServer.InstallPath;

			numPort.Value = existingServer.Port;
			numQueryPort.Value = existingServer.QueryPort;
			txtPassword.Text = existingServer.Password;
			numMaxPlayers.Value = existingServer.MaxPlayers;

			// Set the button to Update mode
			btnSave.Text = "Update Server";

			// Set map after game is selected
			cmbWorldName.Text = existingServer.WorldName;
			txtExtraArgs.Text = existingServer.ExtraArgs;

			isManualLoading = false; // END SILENCE
			UpdateControlStates();
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
			if (isManualLoading) return; // If we are loading an existing server, STOP HERE.

			bool isGamePicked = cmbGame.SelectedIndex > 0;
			UpdateControlStates();

			if (isGamePicked && btnSave.Text == "Save Server")
			{
				var gameData = GameDatabase.GetGame(cmbGame.SelectedItem.ToString());
				if (gameData != null)
				{
					// ONLY auto-fill defaults if it's a BRAND NEW server
					numPort.Value = gameData.DefaultPort;
					numQueryPort.Value = gameData.DefaultQueryPort;
					txtExtraArgs.Text = gameData.DefaultArgs;

					cmbWorldName.Items.Clear();
					foreach (var map in gameData.Maps) cmbWorldName.Items.Add(map);
					if (cmbWorldName.Items.Count > 0) cmbWorldName.SelectedIndex = 0;
				}
			}
			UpdatePathPreview();
		}

		private void txtName_TextChanged(object sender, EventArgs e)
		{
			UpdateControlStates();
		}
	}
}
