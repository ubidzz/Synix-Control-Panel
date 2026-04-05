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
			cmbGame.SelectedIndexChanged -= cmbGame_SelectedIndexChanged; // Detach

			cmbGame.Items.Clear();
			cmbGame.Items.Add("-- Pick a Game --");
			foreach (var game in GameDatabase.GetGameList()) cmbGame.Items.Add(game.Name);
			cmbGame.SelectedIndex = 0;

			cmbGame.SelectedIndexChanged += cmbGame_SelectedIndexChanged; // Re-attach
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
			// 1. Mute the event so it doesn't overwrite ports with defaults
			cmbGame.SelectedIndexChanged -= cmbGame_SelectedIndexChanged;

			btnSave.Text = "Update Server";

			// 2. Refresh the list items
			cmbGame.Items.Clear();
			cmbGame.Items.Add("-- Pick a Game --");
			foreach (var g in GameDatabase.GetGameList()) cmbGame.Items.Add(g.Name);

			// 3. FORCE the selection by matching the text
			txtName.Text = existingServer.Name;

			// Find the item by text rather than index to be safe
			for (int i = 0; i < cmbGame.Items.Count; i++)
			{
				if (cmbGame.Items[i].ToString() == existingServer.Game)
				{
					cmbGame.SelectedIndex = i;
					break;
				}
			}

			// 4. Fill all other saved settings
			numPort.Value = existingServer.Port;
			numQueryPort.Value = existingServer.QueryPort;
			txtPassword.Text = existingServer.Password;
			numMaxPlayers.Value = existingServer.MaxPlayers;
			txtExtraArgs.Text = existingServer.ExtraArgs;

			// 5. Load the specific maps for this game
			var gameData = GameDatabase.GetGame(existingServer.Game);
			if (gameData != null)
			{
				cmbWorldName.Items.Clear();
				foreach (var map in gameData.Maps) cmbWorldName.Items.Add(map);
				cmbWorldName.Text = existingServer.WorldName;
			}

			// 6. Set Paths and LOCK them
			chkDefaultPath.Checked = existingServer.IsDefaultPath;
			txtInstallPath.Text = existingServer.InstallPath;

			// 7. Re-attach and lock the UI
			cmbGame.SelectedIndexChanged += cmbGame_SelectedIndexChanged;

			// Disable location editing during 'Update' mode
			chkDefaultPath.Enabled = false;
			btnBrowse.Enabled = false;
			txtInstallPath.Enabled = false;
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
			if (isManualLoading) return; // Silent if loading existing data

			bool isGamePicked = cmbGame.SelectedIndex > 0;
			UpdateControlStates();

			if (isGamePicked)
			{
				var gameData = GameDatabase.GetGame(cmbGame.SelectedItem.ToString());
				if (gameData != null)
				{
					// ALWAYS update the maps list so the dropdown works
					cmbWorldName.Items.Clear();
					foreach (var map in gameData.Maps) cmbWorldName.Items.Add(map);

					// ONLY auto-fill defaults for NEW servers (Save mode)
					if (btnSave.Text == "Save Server")
					{
						numPort.Value = gameData.DefaultPort;
						numQueryPort.Value = gameData.DefaultQueryPort;
						txtExtraArgs.Text = gameData.DefaultArgs;
						if (cmbWorldName.Items.Count > 0) cmbWorldName.SelectedIndex = 0;
					}
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
