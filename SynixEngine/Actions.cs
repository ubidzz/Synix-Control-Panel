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
		// Inside Actions.cs
		public async Task StopServerAndReport(GameServer server, bool isManual = true)
		{
			if (server.RunningProcess == null && !server.PID.HasValue)
			{
				MessageBox.Show($"No active process found for '{server.ServerName}'.", "Information");
				return;
			}

			server.Status = StatusManager.GetStatus(ServerState.Stopping);
			UpdateGridStatus();

			await Task.Run(() =>
			{
				Servers.Stop(server, msg =>
				{
					MainGUI.Instance?.Invoke((Action)(() => MainGUI.Instance.AppendLog(msg)));
				}, 
				isManual);
			});
			server.Status = StatusManager.GetStatus(ServerState.Stopped);
			server.PID = null;
			FileHandler.SaveServers();
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
				// Wrapping the path in quotes handle spaces in folder names perfectly
				Process.Start("explorer.exe", $"\"{server.InstallPath}\"");
			}
			else
			{
				Log($"[ERROR] Folder does not exist: {server.InstallPath}", Color.Red);
			}
		}

		// ACTION 4: DELETE SERVER (Merged & Safe)
		public void DeleteServerAndReport(GameServer server)
		{
			// 1. Safety Checks (Installing/Running)
			string status = server.Status ?? "";
			if (status == "Installing" || status == "Updating" || (server.PID.HasValue && server.PID > 0))
			{
				MessageBox.Show("Cannot delete an active or installing server.", "Action Locked");
				return;
			}

			// 🎯 THE FIX: Declare 'confirm' right here by assigning the MessageBox result
			DialogResult confirm = MessageBox.Show($"Are you sure you want to PERMANENTLY delete '{server.ServerName}'?\n\n" +
												   $"This will wipe: {server.InstallPath}",
												   "Confirm Total Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

			// Now the compiler knows what 'confirm' is!
			if (confirm == DialogResult.Yes)
			{
				try
				{
					// 2. RAM FIX: Pull it out of the list so the engine stops "thinking" about it
					if (MainGUI.serverList.Contains(server))
					{
						MainGUI.serverList.Remove(server);
					}

					// 3. WIPE FILES: Call your folder handler
					FolderHandler.ServerFolder.Delete(server, msg =>
					{
						// Use the Core logger to print the result to the UI
						Core.Instance.Log(msg);
					});

					// 4. Update the Grid
					Core.Instance.UpdateGridStatus();
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Files were partially deleted, but an error occurred: {ex.Message}");
					Core.Instance.UpdateGridStatus();
				}
			}
		}

		// Update these two methods in Actions.cs (Core partial class)

		public async Task UpdateServerAndReport(GameServer server)
		{
			// 1. YOUR SAFETY: Don't update if server is running
			if (server.Status == StatusManager.GetStatus(ServerState.Running))
			{
				MessageBox.Show("You must stop the server before updating it.", "Server Active", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			// 2. YOUR SAFETY: Don't update if already busy
			if (server.Status == StatusManager.GetStatus(ServerState.Updating) || server.Status == StatusManager.GetStatus(ServerState.Installing) || isDownloadActive)
			{
				MessageBox.Show("A download or update is already in progress.", "System Busy", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			// 3. YOUR CONFIRMATION
			var confirm = MessageBox.Show($"Are you sure you want to update {server.ServerName}?", "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (confirm != DialogResult.Yes) return;

			// 4. YOUR DATABASE LOOKUP
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
				server.Status = StatusManager.GetStatus(ServerState.Updating);
				isDownloadActive = true;
				UpdateGridStatus();

				Log($"--- UPDATE STARTED: {server.Game} ---", Color.White, true);
				Log($"--- [WARNING] Synix close window button is disabled! ---", Color.Orange, true);
				Log($"--- [INFO] Updating {server.Game} can take up to 5 minutes ---", Color.DeepSkyBlue, true);

				// 6. RUN INSTALLER (Thread-safe background task)
				int exitCode = await Task.Run(() =>
				{
					// Updated to include the PID callback
					return ServerInstaller.Install(server.InstallPath, appId,
						msg => { MainGUI.Instance?.Invoke((Action)(() => Log(msg))); },
						pid => {
							server.SteamPID = pid;
							FileHandler.SaveServers();
						});
				});

				// 7. HANDLE ERRORS
				if (exitCode != 0)
				{
					string errorDetail = ServerInstaller.GetSteamError(exitCode);
					MessageBox.Show($"Update Failed!\n\nReason: {errorDetail}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Log($"[CRITICAL ERROR] Update failed with code {exitCode}.", Color.Red, true);
					return;
				}

				// 8. YOUR SUCCESS LOGIC: Re-apply GameFixes
				bool fixApplied = GameFix.PostInstall(server);
				if (fixApplied) Log($"[SUCCESS] Re-applied missing files to the {server.Game} server.", Color.Green);

				Log($"--- UPDATE FINISHED: {server.Game} ---", Color.Green, true);
				Log($"--- [WARNING] Synix close window button is now Enabled! ---", Color.Orange, true);
			}
			finally
			{
				// 9. CLEANUP: Always unlock the app and CLEAR the SteamPID
				server.Status = StatusManager.GetStatus(ServerState.Stopped); ;
				server.SteamPID = null;
				isDownloadActive = false;
				FileHandler.SaveServers();
				UpdateGridStatus();
			}
		}

		public async Task ValidationServerAndReport(GameServer server)
		{
			// 1. YOUR SAFETY: Don't validate if server is running
			if (server.Status == StatusManager.GetStatus(ServerState.Running))
			{
				MessageBox.Show("You must stop the server before validating server files.", "Server Active", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			// 2. YOUR SAFETY: Don't update if already busy
			if (server.Status == StatusManager.GetStatus(ServerState.Updating) || server.Status == StatusManager.GetStatus(ServerState.Installing) || isDownloadActive)
			{
				MessageBox.Show("A download, update or validation is already in progress.", "System Busy", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			// 3. YOUR CONFIRMATION
			var confirm = MessageBox.Show($"Are you sure you want to Validate the {server.ServerName} server files?", "Confirm Validate", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (confirm != DialogResult.Yes) return;

			// 4. YOUR DATABASE LOOKUP
			var gameData = GameDatabase.GetGame(server.Game);
			string appId = gameData?.AppID ?? "";

			if (string.IsNullOrEmpty(appId))
			{
				MessageBox.Show("Could not find the AppID for this game. Cannot validate server files.", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			try
			{
				// 5. SET BUSY STATE
				server.Status = StatusManager.GetStatus(ServerState.Updating);
				isDownloadActive = true;
				UpdateGridStatus();

				Log($"--- Validating STARTED: {server.Game} ---", Color.White, true);
				Log($"--- [WARNING] Synix close window button is disabled! ---", Color.Orange, true);
				Log($"--- [INFO] Validating {server.Game} can take up to 5 minutes ---", Color.DeepSkyBlue, true);

				// 6. RUN INSTALLER (Thread-safe background task)
				int exitCode = await Task.Run(() =>
				{
					// Updated to include the PID callback
					return ServerInstaller.Install(server.InstallPath, appId,
						msg => { MainGUI.Instance?.Invoke((Action)(() => Log(msg))); },
						pid => {
							server.SteamPID = pid;
							FileHandler.SaveServers();
						});
				});

				// 7. HANDLE ERRORS
				if (exitCode != 0)
				{
					string errorDetail = ServerInstaller.GetSteamError(exitCode);
					MessageBox.Show($"Update Failed!\n\nReason: {errorDetail}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Log($"[CRITICAL ERROR] Validate failed with code {exitCode}.", Color.Red, true);
					return;
				}

				// 8. YOUR SUCCESS LOGIC: Re-apply GameFixes
				bool fixApplied = GameFix.PostInstall(server);
				if (fixApplied) Log($"[SUCCESS] Re-applied missing files to the {server.Game} server.", Color.Green);

				Log($"--- UPDATE FINISHED: {server.Game} ---", Color.Green, true);
				Log($"--- [WARNING] Synix close window button is now Enabled! ---", Color.Orange, true);
			}
			finally
			{
				// 9. CLEANUP: Always unlock the app and CLEAR the SteamPID
				server.Status = StatusManager.GetStatus(ServerState.Stopped); ;
				server.SteamPID = null;
				isDownloadActive = false;
				FileHandler.SaveServers();
				UpdateGridStatus();
			}
		}

		public async Task AddServerAndReport()
		{
			using (ServerSettingsGUI settingsForm = new())
			{
				if (settingsForm.ShowDialog() == DialogResult.OK && settingsForm.NewServer != null)
				{
					GameServer newServer = settingsForm.NewServer;
					var gameData = GameDatabase.GetGame(newServer.Game);
					string appId = gameData?.AppID ?? "";
					GameFix.ManualConfigWasCreated = false;

					if (string.IsNullOrEmpty(appId))
					{
						MessageBox.Show("Could not find the AppID for this game. Installation aborted.", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}

					try
					{
						newServer.Status = StatusManager.GetStatus(ServerState.Installing);
						isDownloadActive = true;
						UpdateGridStatus();

						Log($"--- AUTO-INSTALL STARTED: {newServer.Game} ---", Color.White, true);

						int exitCode = await Task.Run(() =>
						{
							// Updated to include the PID callback
							return ServerInstaller.Install(newServer.InstallPath, appId,
								msg => Log(msg),
								pid => {
									newServer.SteamPID = pid;
									FileHandler.SaveServers(); // Save the PID immediately
								});
						});

						if (exitCode != 0)
						{
							string errorMsg = ServerInstaller.GetSteamError(exitCode);
							MessageBox.Show($"Installation Failed!\n\nReason: {errorMsg}", "SteamCMD Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
							newServer.Status = "Failed";
							return;
						}

						bool fixApplied = GameFix.PostInstall(newServer);
						if (fixApplied) Log($"[SUCCESS] Re-applied missing files to the {newServer.Game} server.", Color.Green);
						newServer.IsFirstBoot = GameFix.ManualConfigWasCreated;
						FileHandler.SaveServers();
						Log($"--- AUTO-INSTALL FINISHED: {newServer.Game} ---", Color.Green, true);
					}
					catch (Exception ex)
					{
						MessageBox.Show($"An unexpected error occurred during installation: {ex.Message}", "System Error");
					}
					finally
					{
						newServer.Status = StatusManager.GetStatus(ServerState.Stopped); ;
						newServer.SteamPID = null;
						isDownloadActive = false;
						UpdateGridStatus();
					}
				}
			}
		}

		public void EditServerAndReport(GameServer server)
		{
			// 1. RUNNING SAFETY: Don't edit a live server!
			if (server.Status == StatusManager.GetStatus(ServerState.Running) || (server.PID.HasValue && server.PID > 0))
			{
				MessageBox.Show("Please stop the server before editing its settings.",
								"Server Active", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			// 2. 🛡️ INSTALL/UPDATE SAFETY: Protect the JSON while SteamCMD is active
			if (server.Status == StatusManager.GetStatus(ServerState.Installing) || server.Status == StatusManager.GetStatus(ServerState.Updating) || (server.SteamPID.HasValue && server.SteamPID > 0))
			{
				string currentAction = (server.Status == StatusManager.GetStatus(ServerState.Updating)) ? StatusManager.GetStatus(ServerState.Updating) : StatusManager.GetStatus(ServerState.Installing);

				MessageBox.Show($"Cannot edit '{server.ServerName}' while it is {currentAction}.\n\nPlease wait for the process to finish.",
								"System Busy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			// 3. ACTION: Open the Settings GUI in Edit Mode
			using (var editForm = new ServerSettingsGUI(server))
			{
				if (editForm.ShowDialog() == DialogResult.OK)
				{
					// 4. FEEDBACK: Log success and refresh the grid
					Log($"[SUCCESS] {server.ServerName} settings updated and saved.", Color.Green);
					UpdateGridStatus();
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
					Servers.Start(server, msg => Log(msg), StartContext.Scheduled);
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
			if (server.Status == StatusManager.GetStatus(ServerState.Stopped))
			{
				Log($"[RESTART] Port verified. Booting {server.Game}...", Color.Green);

				// 🎯 UPDATE: Pass StartContext.Manual to trigger the backup
				await Servers.Start(server, msg =>
				{
					MainGUI.Instance?.Invoke((Action)(() => Log(msg)));
				}, StartContext.Manual);
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

			_ = SendDiscordAlert(server, "CRASH DETECTED",
				$"{server.ServerName} has terminated. Synix is attempting an automatic restart.",
				Color.Red);

			// 2. SCRUB: The Stop method handles Ctrl+C and the mandatory taskkill fallback
			await Task.Run(() =>
			{
				Servers.Stop(server, msg =>
				{
					MainGUI.Instance?.Invoke((Action)(() => Log(msg, Color.Yellow)));
				}, false);
			});

			// 3. COOL DOWN: Wait for Windows to fully release file locks and ports
			await Task.Delay(4000);

			// 4. VERIFY & RESTART
			if (server.Status == StatusManager.GetStatus(ServerState.Stopped))
			{
				Log($"[WATCHDOG] Environment cleared. Restarting {server.Game}...", Color.Green);

				// 🎯 UPDATE: Pass StartContext.CrashRecovery to skip the backup
				await Servers.Start(server, msg =>
				{
					MainGUI.Instance?.Invoke((Action)(() => Log(msg)));
				}, StartContext.CrashRecovery);
			}
			UpdateGridStatus();
		}

		public void RunUniversalHealthCheck()
		{
			foreach (var server in MainGUI.serverList)
			{
				// Only monitor servers marked as Running
				if (server.Status == StatusManager.GetStatus(ServerState.Running))
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

		public async Task InstallOrUpdate(GameServer server)
		{
			try
			{
				var dbEntry = GameDatabase.GetGame(server.Game);
				if (dbEntry == null) return;

				int exitCode = await Task.Run(() =>
				{
					return ServerInstaller.Install(
						server.InstallPath,
						dbEntry.AppID,
						msg => { MainGUI.Instance?.Invoke((Action)(() => Log(msg))); },
						pid => {
							server.SteamPID = pid;
							FileHandler.SaveServers(); // Save PID so monitor sees it
						});
				});

				// 🎯 3. Log the result based on your exit codes
				if (exitCode != 0)
				{
					string errorDetail = ServerInstaller.GetSteamError(exitCode);
					Log($"[ERROR] Update failed for {server.ServerName}: {errorDetail}", Color.Red);
				}
				else
				{
					Log($"[SUCCESS] {server.ServerName} is up to date.", Color.Green);
				}
			}
			catch (Exception ex)
			{
				Log($"[CRITICAL] InstallOrUpdate Exception: {ex.Message}", Color.Red);
			}
			finally
			{
				// Always clear the SteamPID when the task finishes
				server.SteamPID = null;
				FileHandler.SaveServers();
				UpdateGridStatus();
			}
		}

		// 🎯 RENAME and CLEAN this method:
		public async Task StartServerAndReport(GameServer server)
		{
			if (!PassResourceGuard(out string guardMsg))
			{
				Log(guardMsg, System.Drawing.Color.Red, true); // Bold red for critical logs
				System.Windows.Forms.MessageBox.Show(guardMsg, "System Resource Exhaustion",
					System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
				return;
			}

			// 1. Technical & Logic Checks (Your existing safety locks)
			if (!PassStartSpamLock(server, out string lockMsg)) { Log(lockMsg, System.Drawing.Color.Orange); return; }
			if (!ValidateIntegrityAndReport(server)) return;
			if (ShouldBlockForConfig(server)) return;

			// 2. Backup Logic
			if (server.BackupOnStart)
			{
				server.Status = StatusManager.GetStatus(ServerState.BackingUp);
				UpdateGridStatus();
			}

			// 3. EXECUTE: This calls Servers.Start which runs the BackupManager and the .exe
			await Servers.Start(server, msg =>
			{
				server.StartTime = DateTime.Now;
				Log(msg);
				UpdateGridStatus();
			}, StartContext.Manual);

			UpdateGridStatus();
		}
	}
}