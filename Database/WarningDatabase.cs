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
				lblWarningText.Text = "Configuration required before the first launch. \n1. If the Config file is missing in the game then the server needs to run once to create the config file. \n2. Then shut the server down and go to `Server Ations -> Server Options -> Edit Config File` and edit the config file. \n3. Some Servers use their own server manager in the game to fully setup the server.";
		}

		// 🛠️ THE "OPEN CONFIG MANAGER" BUTTON (Formerly 'Yes')
		private void btnYes_Click(object sender, EventArgs e)
		{
			_server.IsFirstBoot = false;
			try
			{
				FileHandler.SaveServers();

				// 1. Get the game data template
				var gameData = GameDatabase.GetGame(_server.Game);
				if (gameData != null && !string.IsNullOrEmpty(gameData.RelativeConfigPath))
				{
					// 2. "Clean" the identity: get the ServerName and replace " " with "_"
					// We use the Identity property which should already be clean, 
					// but this ensures no spaces sneak into the path.
					string cleanIdentity = _server.ServerName.Replace(" ", "_");

					// 3. Replace the placeholder in the path
					string relativePath = gameData.RelativeConfigPath.Replace("{Identity}", cleanIdentity);

					// 4. Combine with the root install path
					string fullPath = Path.Combine(_server.InstallPath, relativePath);

					if (File.Exists(fullPath))
					{
						this.Hide();

						// 5. Open the Config Editor using the cleaned path
						using (ServerConfig editor = new ServerConfig(fullPath, gameData.Format))
						{
							editor.ShowDialog();
						}
					}
					else
					{
						// Detailed error helps find if the pathing is wrong
						MessageBox.Show($"Config file not found!\n\nTarget Path: {fullPath}", "Path Error");
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