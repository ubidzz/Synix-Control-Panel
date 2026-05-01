// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// 
// LEGAL NOTICE:
// This source code is proprietary and confidential. 
// 1. Permission is granted for PERSONAL, NON-COMMERCIAL use only.
// 2. You may modify this code for your own use, but you may NOT redistribute,
//    rebrand, or sell this code or derivative works without written consent.
// 3. The "Synix" brand and logic remain the property of Jason Turner.
// ============================================================================
using Synix_Control_Panel.Database;
using Synix_Control_Panel.FileFolderHandler;
using Synix_Control_Panel.ServerHandler;
using Synix_Control_Panel.SteamCMDHandler;
using System.Diagnostics;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
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

		public void OpenConfigEditor(GameServer server)
		{
			var blueprint = GameDatabase.GetGame(server.Game);

			if (blueprint == null || string.IsNullOrEmpty(blueprint.RelativeConfigPath))
			{
				MessageBox.Show("This game does not have a config path defined.", "No Config");
				return;
			}

			string cleanIdentity = server.ServerName.Replace(" ", "_");
			string resolvedRelativePath = blueprint.RelativeConfigPath
				.Replace("{Identity}", cleanIdentity)
				.Replace("{ServerName}", cleanIdentity)
				.Replace("{map}", server.WorldName)
				.Replace("{port}", server.Port.ToString())
				.Replace("{query}", server.QueryPort.ToString());

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

		public void OpenServerFolder(GameServer server)
		{
			if (Directory.Exists(server.InstallPath))
			{
				Process.Start("explorer.exe", $"\"{server.InstallPath}\"");
			}
			else
			{
				Log($"[🚨 ERROR] Folder does not exist: {server.InstallPath}", Color.Red);
			}
		}

		public void DeleteServerAndReport(GameServer server)
		{
			string status = server.Status ?? "";
			if (status == StatusManager.GetStatus(ServerState.Installing) || status == StatusManager.GetStatus(ServerState.Updating) || (server.PID.HasValue && server.PID > 0))
			{
				MessageBox.Show("Cannot delete an active or installing server.", "Action Locked");
				return;
			}

			DialogResult confirm = MessageBox.Show($"Are you sure you want to PERMANENTLY delete '{server.ServerName}'?\n\n" +
												   $"This will wipe: {server.InstallPath}",
												   "Confirm Total Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

			if (confirm == DialogResult.Yes)
			{
				try
				{
					if (MainGUI.serverList.Contains(server))
					{
						MainGUI.serverList.Remove(server);
					}

					FolderHandler.ServerFolder.Delete(server, msg =>
					{
						Core.Instance.Log(msg);
					});

					Core.Instance.UpdateGridStatus();
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Files were partially deleted, but an error occurred: {ex.Message}");
					Core.Instance.UpdateGridStatus();
				}
			}
		}

		public async Task UpdateServerAndReport(GameServer server)
		{
			if (server.Status == StatusManager.GetStatus(ServerState.Running))
			{
				MessageBox.Show("You must stop the server before updating it.", "Server Active", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (server.Status == StatusManager.GetStatus(ServerState.Updating) || server.Status == StatusManager.GetStatus(ServerState.Installing) || isDownloadActive)
			{
				MessageBox.Show("A download or update is already in progress.", "System Busy", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			var confirm = MessageBox.Show($"Are you sure you want to update {server.ServerName}?", "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (confirm != DialogResult.Yes) return;

			var gameData = GameDatabase.GetGame(server.Game);
			string appId = gameData?.AppID ?? "";

			if (string.IsNullOrEmpty(appId))
			{
				MessageBox.Show("Could not find the AppID for this game. Cannot update.", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			try
			{
				server.Status = StatusManager.GetStatus(ServerState.Updating);
				isDownloadActive = true;
				UpdateGridStatus();

				Log($"--- [🔒 WARNING] Synix close window button is disabled! ---", Color.Orange, true);
				Log($"--- UPDATE STARTED: {server.Game} ---", Color.White, true);
				Log($"--- [📜 INFO] Updating {server.Game} can take up to 5 minutes ---", Color.DeepSkyBlue, true);

				int exitCode = await Task.Run(() =>
				{
					return ServerInstaller.Install(server.InstallPath, appId,
						msg => { MainGUI.Instance?.Invoke((Action)(() => Log(msg))); },
						pid =>
						{
							server.SteamPID = pid;
							FileHandler.SaveServers();
						});
				});

				if (exitCode != 0)
				{
					string errorDetail = ServerInstaller.GetSteamError(exitCode);
					MessageBox.Show($"Update Failed!\n\nReason: {errorDetail}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Log($"[🚨 CRITICAL ERROR] Update failed with code {exitCode}.", Color.Red, true);
					return;
				}

				bool fixApplied = await GameFix.PostInstall(server);
				if (fixApplied) Log($"[✔️ SUCCESS] Re-applied missing files to the {server.Game} server.", Color.Green);
				Log($"--- UPDATE FINISHED: {server.Game} ---", Color.Green, true);
				Log($"--- [🔓 WARNING] Synix close window button is now Enabled! ---", Color.Orange, true);
			}
			finally
			{
				server.Status = StatusManager.GetStatus(ServerState.Stopped); ;
				server.SteamPID = null;
				isDownloadActive = false;
				FileHandler.SaveServers();
				UpdateGridStatus();
			}
		}

		public async Task ValidationServerAndReport(GameServer server)
		{
			if (server.Status == StatusManager.GetStatus(ServerState.Running))
			{
				MessageBox.Show("You must stop the server before validating server files.", "Server Active", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (server.Status == StatusManager.GetStatus(ServerState.Updating) || server.Status == StatusManager.GetStatus(ServerState.Installing) || isDownloadActive)
			{
				MessageBox.Show("A download, update or validation is already in progress.", "System Busy", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			var confirm = MessageBox.Show($"Are you sure you want to Validate the {server.ServerName} server files?", "Confirm Validate", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (confirm != DialogResult.Yes) return;

			var gameData = GameDatabase.GetGame(server.Game);
			string appId = gameData?.AppID ?? "";

			if (string.IsNullOrEmpty(appId))
			{
				MessageBox.Show("Could not find the AppID for this game. Cannot validate server files.", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			try
			{
				server.Status = StatusManager.GetStatus(ServerState.Updating);
				isDownloadActive = true;
				UpdateGridStatus();

				Log($"--- Validating STARTED: {server.Game} ---", Color.White, true);
				Log($"--- [🔒 WARNING] Synix close window button is disabled! ---", Color.Orange, true);
				Log($"--- [📜 INFO] Validating {server.Game} can take up to 5 minutes ---", Color.DeepSkyBlue, true);

				int exitCode = await Task.Run(() =>
				{
					return ServerInstaller.Install(server.InstallPath, appId,
						msg => { MainGUI.Instance?.Invoke((Action)(() => Log(msg))); },
						pid =>
						{
							server.SteamPID = pid;
							FileHandler.SaveServers();
						});
				});

				if (exitCode != 0)
				{
					string errorDetail = ServerInstaller.GetSteamError(exitCode);
					MessageBox.Show($"Update Failed!\n\nReason: {errorDetail}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Log($"[🚨 CRITICAL ERROR] Validate failed with code {exitCode}.", Color.Red, true);
					return;
				}

				bool fixApplied = await GameFix.PostInstall(server);
				if (fixApplied) Log($"[✔️ SUCCESS] Re-applied missing files to the {server.Game} server.", Color.Green);

				Log($"--- UPDATE FINISHED: {server.Game} ---", Color.Green, true);
				Log($"--- [🔓 WARNING] Synix close window button is now Enabled! ---", Color.Orange, true);
			}
			finally
			{
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
							return ServerInstaller.Install(newServer.InstallPath, appId,
								msg => Log(msg),
								pid =>
								{
									newServer.SteamPID = pid;
									FileHandler.SaveServers();
								});
						});

						if (exitCode != 0)
						{
							string errorMsg = ServerInstaller.GetSteamError(exitCode);
							MessageBox.Show($"Installation Failed!\n\nReason: {errorMsg}", "SteamCMD Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
							newServer.Status = "Failed";
							return;
						}

						bool fixApplied = await GameFix.PostInstall(newServer);
						if (fixApplied) Log($"[✔️ SUCCESS] Re-applied missing files to the {newServer.Game} server.", Color.Green);
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
			if (server.Status == StatusManager.GetStatus(ServerState.Running) || (server.PID.HasValue && server.PID > 0))
			{
				MessageBox.Show("Please stop the server before editing its settings.",
								"Server Active", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (server.Status == StatusManager.GetStatus(ServerState.Installing) || server.Status == StatusManager.GetStatus(ServerState.Updating) || (server.SteamPID.HasValue && server.SteamPID > 0))
			{
				string currentAction = (server.Status == StatusManager.GetStatus(ServerState.Updating)) ? StatusManager.GetStatus(ServerState.Updating) : StatusManager.GetStatus(ServerState.Installing);

				MessageBox.Show($"Cannot edit '{server.ServerName}' while it is {currentAction}.\n\nPlease wait for the process to finish.",
								"System Busy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			using (var editForm = new ServerSettingsGUI(server))
			{
				if (editForm.ShowDialog() == DialogResult.OK)
				{
					Log($"[✔️ SUCCESS] {server.ServerName} settings updated and saved.", Color.Green);
					UpdateGridStatus();
				}
			}
		}

		public async Task ExecuteStartSequence(GameServer server, string status = "")
		{
			bool stopServer = false;
			if (!PassResourceGuard(out string guardMsg))
			{
				Log(guardMsg, System.Drawing.Color.Red, true);
				System.Windows.Forms.MessageBox.Show(guardMsg, "System Resource Exhaustion",
					System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
				return;
			}

			if (!ValidateIntegrityAndReport(server)) return;
			if (ShouldBlockForConfig(server)) return;

			if (status == "RESTART")
			{
				Log($"[SYNIX] Starting restart sequence for {server.ServerName}...", Color.Cyan);
				stopServer = true;
			}
			else if (status == "MAINTENANCE")
			{
				Log($"[MAINTENANCE] Scheduled restart sequence for {server.ServerName}.", Color.Cyan, true);
				stopServer = true;
			}

			if (stopServer)
			{
				await Task.Run(() =>
				{
					Servers.Stop(server, msg =>
					{
						MainGUI.Instance?.Invoke((Action)(() => Log(msg, Color.Yellow)));
					});
				});
			}

			await Task.Delay(3000);

			if (server.Status == StatusManager.GetStatus(ServerState.Stopped))
			{
				Log($"[SYNIX] Starting the {server.ServerName} server.", Color.Cyan, true);
				if (!PassStartSpamLock(server, out string lockMsg)) { Log(lockMsg, System.Drawing.Color.Orange); return; }

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

		public async Task RecoverServer(GameServer server)
		{
			// 1. Identify failure type
			string reason = !server.RunningProcess?.Responding ?? false ? "FREEZE" : "CRASH/CLOSE";
			Log($"[🛡️ WATCHDOG] {reason} detected on {server.ServerName}. Initializing recovery...", Color.Orange);

			_ = SendDiscordAlert(server, "🚨 CRASH DETECTED",
				$"{server.ServerName} has terminated. Synix is attempting an automatic restart.",
				Color.Red);

			await Task.Run(() =>
			{
				Servers.Stop(server, msg =>
				{
					MainGUI.Instance?.Invoke((Action)(() => Log(msg, Color.Yellow)));
				}, false);
			});

			await Task.Delay(4000);

			if (server.Status == StatusManager.GetStatus(ServerState.Stopped))
			{
				Log($"[🛡️ WATCHDOG] Environment cleared. Restarting {server.Game}...", Color.Green);

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
				if (server.Status == StatusManager.GetStatus(ServerState.Running))
				{
					if (server.RunningProcess == null || server.RunningProcess.HasExited)
					{
						_ = RecoverServer(server);
						continue;
					}

					try
					{
						server.RunningProcess.Refresh();
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
						pid =>
						{
							server.SteamPID = pid;
							FileHandler.SaveServers();
						});
				});

				if (exitCode != 0)
				{
					string errorDetail = ServerInstaller.GetSteamError(exitCode);
					Log($"[🚨 ERROR] Update failed for {server.ServerName}: {errorDetail}", Color.Red);
				}
				else
				{
					Log($"[✔️ SUCCESS] {server.ServerName} is up to date.", Color.Green);
				}
			}
			catch (Exception ex)
			{
				Log($"[CRITICAL] InstallOrUpdate Exception: {ex.Message}", Color.Red);
			}
			finally
			{
				server.SteamPID = null;
				FileHandler.SaveServers();
				UpdateGridStatus();
			}
		}
	}
}
