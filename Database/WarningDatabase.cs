using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Synix_Control_Panel.ServerHandler;
using Synix_Control_Panel.FileFolderHandler;

namespace Synix_Control_Panel.Database
{
	public partial class WarningDatabase : Form
	{
		private GameServer _server;

		// 📖 Dynamic instructions for each game
		private static readonly Dictionary<string, string> _messages = new()
		{
			{ "7 Days to Die", "You must edit 'serverconfig.xml' to set your Server Name and Admin Password." },
			{ "ARK: Survival Evolved", "Ensure you set a 'ServerAdminPassword' in the config for admin rights." },
			{ "Don't Starve Together", "CRITICAL: You must paste your Cluster Token into 'cluster.ini' or the server will crash." },
			{ "Factorio", "You must enter your Factorio Username and Token in 'server-settings.json' for public listing." },
			{ "Palworld", "Set an 'AdminPassword' in PalWorldSettings.ini to manage your server in-game." },
			{ "Soulmask", "Soulmask requires an 'adminpsw' and 'PSW' to be set in the JSON config." },
			{ "V Rising", "You must set a unique 'SaveName' and 'ServerName' or the game won't generate a world." }
			// Add more messages as needed...
		};

		public WarningDatabase(GameServer server)
		{
			InitializeComponent();
			_server = server;

			// Set the specific warning message
			if (_messages.TryGetValue(server.Game, out string customMessage))
				lblWarningText.Text = customMessage;
			else
				lblWarningText.Text = "Configuration required before the first launch.";
		}

		// 🛠️ THE "OPEN CONFIG MANAGER" BUTTON (Formerly 'Yes')
		private void btnYes_Click(object sender, EventArgs e)
		{
			// 1. Update the JSON status
			_server.IsFirstBoot = false;
			try
			{
				FileHandler.SaveServers();

				// 2. Find the config path using the GameDatabase
				var gameData = GameDatabase.GetGame(_server.Game);
				if (gameData != null && !string.IsNullOrEmpty(gameData.RelativeConfigPath))
				{
					string fullPath = Path.Combine(_server.InstallPath, gameData.RelativeConfigPath);

					if (File.Exists(fullPath))
					{
						// 3. Close this window first as requested
						this.Hide();

						// 4. Open the Config Editor
						using (ServerConfig editor = new ServerConfig(fullPath, gameData.Format))
						{
							editor.ShowDialog();
						}
					}
					else
					{
						MessageBox.Show($"Config file not found at:\n{fullPath}\n\nPlease ensure the game is installed correctly.", "Missing File");
					}
				}

				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error: {ex.Message}");
			}
		}

		// 🛠️ THE "CANCEL" BUTTON (Formerly 'No')
		// Ensure this is linked in the Designer events!
		private void btnNo_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}