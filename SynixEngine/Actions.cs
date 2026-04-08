// Copyright (c) 2026 ubidzz. All Rights Reserved.
//
// This file is part of Synix Control Panel.
//
// This code is provided for transparent viewing and personal use only.
// Unauthorized distribution, public modification, or commercial
// use of this source code or the compiled executable is strictly
// prohibited. Please refer to the LICENSE file in the root
// directory for full terms.
using Synix_Control_Panel.Database;
using Synix_Control_Panel.FileFolderHandler;
using Synix_Control_Panel.ServerHandler;
using Synix_Control_Panel.SteamCMDHandler;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		// ACTION 1: STOP SERVER
		public void StopServerAndReport(GameServer server)
		{
			if (server.RunningProcess == null && !server.PID.HasValue)
			{
				MessageBox.Show($"No active process found for '{server.ServerName}'.",
								"Process Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			Servers.Stop(server, msg =>
			{
				MainGUI.Instance?.Invoke((Action)(() => MainGUI.Instance.AppendLog(msg)));
			});

			UpdateGridStatus();
		}

		// ACTION 2: CONFIG EDITOR
		public void OpenConfigEditor(GameServer server)
		{
			var blueprint = GameDatabase.GetGame(server.Game);

			if (blueprint == null || string.IsNullOrEmpty(blueprint.RelativeConfigPath))
			{
				MessageBox.Show("This game does not have a config path defined.", "No Config");
				return;
			}

			string fullPath = Path.Combine(server.InstallPath, blueprint.RelativeConfigPath);

			if (File.Exists(fullPath))
			{
				using (ServerConfig editor = new ServerConfig(fullPath, blueprint.Format))
				{
					editor.ShowDialog();
				}
			}
			else
			{
				MessageBox.Show($"Could not find the config file at:\n{fullPath}", "Missing Config");
			}
		}

		// ACTION 3: OPEN FOLDER
		public void OpenServerFolder(GameServer server)
		{
			if (Directory.Exists(server.InstallPath))
			{
				Process.Start("explorer.exe", server.InstallPath);
			}
			else
			{
				// This uses the Log helper in Core.cs
				Log($"[ERROR] Folder does not exist: {server.InstallPath}", Color.Red);
			}
		}

		// ACTION 4: DELETE SERVER (Merged & Safe)
		public void DeleteServerAndReport(GameServer server)
		{
			// 1. AI Safety Check: Prevent deleting an active server
			if (server.Status == "Online" || (server.PID.HasValue && server.PID > 0))
			{
				MessageBox.Show($"Cannot delete '{server.ServerName}' while it is Online.\n\nPlease stop the server first.",
								"Server Active", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			// 2. AI Confirmation: The "Point of No Return" warning
			var confirm = MessageBox.Show($"Are you sure you want to PERMANENTLY delete '{server.ServerName}'?\n\n" +
										  $"THIS WILL REMOVE ALL FILES AT:\n{server.InstallPath}",
										  "Confirm Total Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

			if (confirm == DialogResult.Yes)
			{
				try
				{
					// 3. AI Action: Call the folder handler to wipe the files
					FolderHandler.ServerFolder.Delete(server, msg =>
					{
						MainGUI.Instance?.Invoke((Action)(() => MainGUI.Instance.AppendLog(msg)));
					});

					// 4. AI Feedback: Refresh the UI
					UpdateGridStatus();
				}
				catch (Exception ex)
				{
					MessageBox.Show($"The server was removed from the list, but some files couldn't be deleted: {ex.Message}",
									"Cleanup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

					UpdateGridStatus();
				}
			}
		}

		public async Task UpdateServerAndReport(GameServer server)
		{
			// 1. AI Safety: Don't update if server is running
			if (server.Status == "Online")
			{
				MessageBox.Show("You must stop the server before updating it.", "Server Active", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			// 2. AI Safety: Don't update if already busy
			if (server.Status == "Updating" || server.Status == "Installing" || isDownloadActive)
			{
				MessageBox.Show("A download or update is already in progress.", "System Busy", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			// 3. AI Confirmation
			var confirm = MessageBox.Show($"Are you sure you want to update {server.ServerName}?", "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (confirm != DialogResult.Yes) return;

			// 4. AI Database Lookup
			var gameData = GameDatabase.GetGame(server.Game);
			string appId = gameData?.AppID ?? "";

			if (string.IsNullOrEmpty(appId))
			{
				MessageBox.Show("Could not find the AppID for this game. Cannot update.", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			try
			{
				// 5. SET BUSY STATE
				server.Status = "Updating";
				isDownloadActive = true;
				UpdateGridStatus();

				Log($"--- UPDATE STARTED: {server.Game} ---", Color.White, true);
				Log($"--- [WARNING] Synix close window button is disabled! ---", Color.Orange, true);
				Log($"--- [INFO] Updating {server.Game} can take up to 5 minutes ---", Color.DeepSkyBlue, true);

				// 6. RUN INSTALLER (Thread-safe background task)
				int exitCode = await Task.Run(() =>
				{
					// Uses the Log helper we put in Core.cs!
					return ServerInstaller.Install(server.InstallPath, appId, msg => Log(msg));
				});

				// 7. HANDLE ERRORS
				if (exitCode != 0)
				{
					string errorDetail = ServerInstaller.GetSteamError(exitCode);
					MessageBox.Show($"Update Failed!\n\nReason: {errorDetail}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Log($"[CRITICAL ERROR] Update failed with code {exitCode}.", Color.Red, true);

					server.Status = "Offline";
					return;
				}

				// 8. SUCCESS: Re-apply GameFixes
				bool fixApplied = GameFix.PostInstall(server);
				if (fixApplied) Log($"[SUCCESS] Re-applied missing files to the {server.Game} server.", Color.Green);

				Log($"--- UPDATE FINISHED: {server.Game} ---", Color.Green, true);
				Log($"--- [WARNING] Synix close window button is now Enabled! ---", Color.Orange, true);
			}
			finally
			{
				// 9. CLEANUP: Always unlock the app, even if it crashes
				server.Status = "Offline";
				isDownloadActive = false;
				UpdateGridStatus();
			}
		}

		public void EditServerAndReport(GameServer server)
		{
			// 1. AI Safety Check: Don't edit a live server!
			if (server.Status == "Online")
			{
				MessageBox.Show("Please stop the server before editing its settings.",
								"Server Active", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			// 2. AI Action: Open the Settings GUI in Edit Mode
			using (var editForm = new ServerSettingsGUI(server)) //
			{
				if (editForm.ShowDialog() == DialogResult.OK)
				{
					// 3. AI Feedback: Log success and refresh the grid
					Log($"[SUCCESS] {server.ServerName} settings updated and saved.", Color.Green);
					UpdateGridStatus();
				}
			}
		}

		public async Task AddServerAndReport()
		{
			// 1. AI Action: Open the configuration window
			using (ServerSettingsGUI settingsForm = new())
			{
				if (settingsForm.ShowDialog() == DialogResult.OK && settingsForm.NewServer != null)
				{
					GameServer newServer = settingsForm.NewServer;
					var gameData = GameDatabase.GetGame(newServer.Game);
					string appId = gameData?.AppID ?? "";

					if (string.IsNullOrEmpty(appId))
					{
						MessageBox.Show("Could not find the AppID for this game. Installation aborted.", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}

					try
					{
						// 2. SET BUSY STATE
						newServer.Status = "Installing";
						isDownloadActive = true;
						UpdateGridStatus();

						Log($"--- AUTO-INSTALL STARTED: {newServer.Game} ---", Color.White, true);

						// 3. RUN INSTALLER (Thread-safe background task)
						int exitCode = await Task.Run(() =>
						{
							return ServerInstaller.Install(newServer.InstallPath, appId, msg => Log(msg));
						});

						// 4. HANDLE ERRORS
						if (exitCode != 0)
						{
							string errorMsg = ServerInstaller.GetSteamError(exitCode);
							MessageBox.Show($"Installation Failed!\n\nReason: {errorMsg}", "SteamCMD Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

							newServer.Status = "Failed";
							return;
						}

						// 5. SUCCESS: Re-apply GameFixes and Save
						bool fixApplied = GameFix.PostInstall(newServer);
						if (fixApplied) Log($"[SUCCESS] Re-applied missing files to the {newServer.Game} server.", Color.Green);

						FileHandler.SaveServers();
						Log($"--- AUTO-INSTALL FINISHED: {newServer.Game} ---", Color.Green, true);
					}
					catch (Exception ex)
					{
						MessageBox.Show($"An unexpected error occurred during installation: {ex.Message}", "System Error");
					}
					finally
					{
						// 6. CLEANUP: Reset status and unlock the app
						newServer.Status = "Offline";
						isDownloadActive = false;
						UpdateGridStatus();
					}
				}
			}
		}
	}
}