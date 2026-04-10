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
		// Change 'public void' to 'public async Task'
		public async Task StopServerAndReport(GameServer server)
		{
			if (server.RunningProcess == null && !server.PID.HasValue)
			{
				MessageBox.Show($"No active process found for '{server.ServerName}'.", "Information");
				return;
			}

			// 1. SHIELD THE SERVER: Watchdog will now ignore this server
			server.Status = "Stopping";
			UpdateGridStatus();

			// 2. STOP ASYNC: No more app freezing
			await Task.Run(() =>
			{
				Servers.Stop(server, msg =>
				{
					MainGUI.Instance?.Invoke((Action)(() => MainGUI.Instance.AppendLog(msg)));
				});
			});

			// 3. CLEANUP & SAVE
			server.Status = "Offline";
			server.PID = null;
			FileHandler.SaveServers(); // Write the Offline status to disk
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

			// 🛠️ THE IDENTITY FIX: Standardize spaces to underscores for file paths
			string cleanIdentity = server.ServerName.Replace(" ", "_");

			// 1. RESOLVE ALL TAGS: This handles Rust, Sunkenland, and others
			string resolvedRelativePath = blueprint.RelativeConfigPath
				.Replace("{Identity}", cleanIdentity)
				.Replace("{ServerName}", cleanIdentity) // Ensures folder paths use underscores
				.Replace("{map}", server.WorldName)
				.Replace("{port}", server.Port.ToString())
				.Replace("{query}", server.QueryPort.ToString());

			// 2. COMBINE: Build the absolute path
			string fullPath = Path.Combine(server.InstallPath, resolvedRelativePath);

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

		public void ExecuteMaintenanceRestart(GameServer server)
		{
			Log($"[MAINTENANCE] Scheduled restart triggered for {server.ServerName}.", Color.Cyan, true);

			// 1. Stop the server
			StopServerAndReport(server);

			// 2. Wait 5 seconds for the PID to fully clear and the OS to breathe
			Task.Delay(5000).ContinueWith(_ =>
			{
				MainGUI.Instance?.Invoke((Action)(() =>
				{
					Log($"[MAINTENANCE] Restarting {server.ServerName}...", Color.Cyan);

					// 3. Start it back up
					Servers.Start(server, msg => Log(msg));
				}));
			});
		}

		public async Task ExecuteRestartSequence(GameServer server)
		{
			Log($"[RESTART] Starting sequence for {server.ServerName}...", Color.Cyan);

			// 1. Wait for the stop process to finish (Ctrl+C + 15s grace period)
			await Task.Run(() =>
			{
				Servers.Stop(server, msg =>
				{
					MainGUI.Instance?.Invoke((Action)(() => Log(msg, Color.Yellow)));
				});
			});

			// 2. The "OS Breath": Wait 3 seconds for Windows to release the IP ports
			await Task.Delay(3000);

			// 3. Verify it is actually stopped before restarting
			if (server.Status == "Offline")
			{
				Log($"[RESTART] Port verified. Booting {server.Game}...", Color.Green);

				Servers.Start(server, msg =>
				{
					MainGUI.Instance?.Invoke((Action)(() => Log(msg)));
				});
			}
			else
			{
				Log($"[CRITICAL] Restart failed: {server.ServerName} is still stuck!", Color.Red);
			}

			UpdateGridStatus();
		}

		// Inside Synix_Control_Panel.SynixEngine.Core
		public async Task RecoverServer(GameServer server)
		{
			// 1. Identify failure type
			string reason = !server.RunningProcess?.Responding ?? false ? "FREEZE" : "CRASH/CLOSE";
			Log($"[WATCHDOG] {reason} detected on {server.ServerName}. Initializing recovery...", Color.Orange);

			// 2. SCRUB: The Stop method handles Ctrl+C and the mandatory taskkill fallback
			// This ensures that even if the EXE is "stuck," it is forcefully cleared.
			await Task.Run(() =>
			{
				Servers.Stop(server, msg =>
				{
					MainGUI.Instance?.Invoke((Action)(() => Log(msg, Color.Yellow)));
				});
			});

			// 3. COOL DOWN: Wait for Windows to fully release file locks and ports
			await Task.Delay(4000);

			// 4. VERIFY & RESTART
			if (server.Status == "Offline")
			{
				Log($"[WATCHDOG] Environment cleared. Restarting {server.Game}...", Color.Green);
				Servers.Start(server, msg =>
				{
					MainGUI.Instance?.Invoke((Action)(() => Log(msg)));
				});
			}
			UpdateGridStatus();
		}

		public void RunUniversalHealthCheck()
		{
			foreach (var server in MainGUI.serverList)
			{
				// Only monitor servers marked as Online
				if (server.Status == "Online")
				{
					// Scenario A: The process object is gone or exited (Crash/Manual Close)
					if (server.RunningProcess == null || server.RunningProcess.HasExited)
					{
						_ = RecoverServer(server);
						continue;
					}

					// Scenario B: The PID is active but the logic is frozen (Not Responding)
					try
					{
						server.RunningProcess.Refresh(); // Force Windows to update process stats
						if (!server.RunningProcess.Responding)
						{
							_ = RecoverServer(server);
						}
					}
					catch { /* Process might have closed during the check */ }
				}
			}
		}
	}
}