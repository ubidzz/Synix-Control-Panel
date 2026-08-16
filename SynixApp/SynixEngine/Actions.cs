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
using Synix_Control_Panel.ServerHandler;
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixApp.SteamCMDHandler;
using System.Diagnostics;
using System.Text;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		private static readonly HashSet<string> _activeSequences = new HashSet<string>();

		public async Task StopServerAndReport(GameServer server, bool isManual = true)
		{
			server.Status = StatusManager.GetStatus(ServerState.Stopping);
			Core.Instance.UpdateGridStatus();

			await Servers.Stop(server, (msg, Color) =>
			{
				Log(msg);
			}, isManual);

			server.Status = StatusManager.GetStatus(ServerState.Stopped);
			server.PID = null;
			FileHandler.SaveServers();
			Core.Instance.UpdateGridStatus();
		}

		public void OpenConfigEditor(GameServer server)
		{
			if (server == null) return;

			var blueprint = GameDatabase.GetGame(server.Game);

			if (blueprint == null || string.IsNullOrEmpty(blueprint.RelativeConfigPath))
			{
				Log("This game does not have a config path defined.", Color.Red, true);
				return;
			}

			if (string.IsNullOrWhiteSpace(server.InstallPath))
			{
				Log("Server installation path is not set.", Color.Red, true);
				return;
			}

			string cleanIdentity = !string.IsNullOrWhiteSpace(server.ServerName)
				? server.ServerName.Replace(" ", "_")
				: "Server";

			string worldName = server.WorldName ?? "";

			string resolvedRelativePath = blueprint.RelativeConfigPath
				.Replace("{Identity}", cleanIdentity)
				.Replace("{ServerName}", cleanIdentity)
				.Replace("{map}", worldName)
				.Replace("{port}", server.Port.ToString())
				.Replace("{query}", server.QueryPort.ToString())
				.Replace('/', Path.DirectorySeparatorChar)
				.Replace('\\', Path.DirectorySeparatorChar);

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
				Log($"Could not find the config file at:\n{fullPath}", Color.Red, true);
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
				Log($"[🚨 ERROR] Folder does not exist: {server.InstallPath}", Color.Red, true);
			}
		}

		public void DeleteServerAndReport(GameServer server)
		{
			string status = server.Status ?? "";
			if (status == StatusManager.GetStatus(ServerState.Installing) || status == StatusManager.GetStatus(ServerState.Updating) || (server.PID.HasValue && server.PID > 0))
			{
				Log("Cannot delete an active or installing server.", Color.Red, true);
				return;
			}

			var page = new TaskDialogPage()
			{
				Caption = "Confirm Total Deletion",
				Heading = $"Are you sure you want to PERMANENTLY delete '{server.ServerName}'?",
				Text = $"This will wipe the installation at:\n{server.InstallPath}",
				Icon = TaskDialogIcon.Warning,
				Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },

				Verification = new TaskDialogVerificationCheckBox()
				{
					Text = "Also delete all server backup archives"
				}
			};

			TaskDialogButton result = TaskDialog.ShowDialog(MainGUI.Instance, page);

			if (result == TaskDialogButton.Yes)
			{
				bool deleteBackups = page.Verification.Checked;

				try
				{
					if (Properties.Settings.Default.enableRunAsAdmin)
					{
						string serverExePath = Path.Combine(server.InstallPath, server.ExeName);

						if (File.Exists(serverExePath))
						{
							CleanFirewallRules(serverExePath);
						}
					}

					FolderHandler.ServerFolder.Delete(server, deleteBackups, (msg, logColor) =>
					{
						Log(msg, logColor);
					});

					UpdateGridStatus();
				}
				catch (Exception ex)
				{
					Log($"Files were partially deleted, but an error occurred:\n{ex.Message}", Color.Red, true);
					MessageBox.Show($"Files were partially deleted, but an error occurred:\n{ex.Message}", "Deletion Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					UpdateGridStatus();
				}
			}
		}

		public void OpenBackFolder(GameServer selectedServer)
		{
			string cleanGame = Core.Instance.GetSafeName(selectedServer.Game);
			string cleanServer = Core.Instance.GetSafeName(selectedServer.ServerName);

			string fullPath = Path.Combine(@"C:\Synix\BackupGames", cleanGame, cleanServer);

			if (Directory.Exists(fullPath))
			{
				Process.Start("explorer.exe", fullPath);
				Log($"[✔ SYNIX] Opening vault: {selectedServer.ServerName}", Color.Cyan);
			}
			else
			{
				Log($"[🚨 SYNIX] There are no created backups at: {fullPath}", Color.Yellow, true);
			}
		}

		public async Task UpdateServerAndReport(GameServer server, string serverProcess, bool autoRestart = false)
		{
			bool ServerUpdating = false;

			if (serverProcess == "UPDATE")
			{
				if (server.Status == StatusManager.GetStatus(ServerState.Running))
				{
					MessageBox.Show("You must stop the server before updating it.", "Server Active", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
				if (server.Status == StatusManager.GetStatus(ServerState.Updating) || server.Status == StatusManager.GetStatus(ServerState.Installing) || server.Status == StatusManager.GetStatus(ServerState.Validating) || isDownloadActive)
				{
					Log("A Downloading or Updating is already in progress.", Color.Orange);
					return;
				}

				ServerUpdating = true;
				if (!autoRestart)
				{
					var confirm = MessageBox.Show($"Are you sure you want to Update the {server.ServerName} server files?", "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (confirm != DialogResult.Yes) return;
				}
			}
			else if (serverProcess == "VALIDATE")
			{
				if (server.Status == StatusManager.GetStatus(ServerState.Running))
				{
					MessageBox.Show("You must stop the server before validating server files.", "Server Active", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				if (server.Status == StatusManager.GetStatus(ServerState.Updating) || server.Status == StatusManager.GetStatus(ServerState.Installing) || server.Status == StatusManager.GetStatus(ServerState.Validating) || isDownloadActive)
				{
					MessageBox.Show("A download, update or validation is already in progress.", "System Busy", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}

				var confirm = MessageBox.Show($"Are you sure you want to Validate the {server.ServerName} server files?", "Confirm Validate", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (confirm != DialogResult.Yes) return;
			}
			else
			{ return; }

			var gameData = GameDatabase.GetGame(server.Game);

			if (gameData == null || string.IsNullOrEmpty(gameData.AppID))
			{
				Log($"Could not find the database blueprint or AppID for {server.Game}.", Color.Red, true);
				return;
			}

			try
			{
				Log($"[🔒 WARNING] Synix close window button is disabled!", Color.Orange, true);
				isDownloadActive = true;
				string ManifestMessage = "";

				if (ServerUpdating)
				{
					Log($"UPDATE STARTED: {server.Game} ---", Color.White, true);
					server.Status = StatusManager.GetStatus(ServerState.Updating);
					Log($"[📜 INFO] Fetching update manifest from Steam... Please wait.", Color.DeepSkyBlue, true);
					Log($"[⏳ NOTE] SteamCMD is working silently in the background. Progress text will stream shortly!", Color.Gray);
					ManifestMessage = "update";
				}
				else
				{
					Log($"VALIDATION STARTED: {server.Game}", Color.White, true);
					server.Status = StatusManager.GetStatus(ServerState.Validating);
					Log($"[📜 INFO] Analyzing local files... Please wait.", Color.DeepSkyBlue, true);
					Log($"[⏳ NOTE] SteamCMD is validating bytes silently. Progress text will stream shortly!", Color.Gray);
					ManifestMessage = "validation";
				}

				Core.Instance.UpdateGridStatus();

				string steamAppsPath = Path.Combine(server.InstallPath, "steamapps");
				string manifestPath = Path.Combine(steamAppsPath, $"appmanifest_{gameData.AppID}.acf"); // Used gameData object

				if (File.Exists(manifestPath))
				{
					try
					{
						File.Delete(manifestPath);
						Log($"[🛠️ SYSTEM] Cleared old Steam manifest to force a clean {ManifestMessage}.", Color.SeaGreen);
					}
					catch (Exception ex)
					{
						Log($"[⚠ WARNING] Could not clear manifest. The {ManifestMessage} might fail. Error: {ex.Message}", Color.Red, true);
					}
				}

				int exitCode = await Task.Run(() =>
				{
					return ServerInstaller.Install(server, gameData,
						msg => { MainGUI.Instance?.BeginInvoke((Action)(() => Log(msg))); },
						pid =>
						{
							server.SteamPID = pid;
							FileHandler.SaveServers();
						});
				});

				if (exitCode != 0)
				{
					string errorDetail = ServerInstaller.GetSteamError(exitCode);
					Log($"[SYNIX] Failed!\n\nReason: {errorDetail}", Color.Red, true);
					Log($"[🚨 CRITICAL ERROR] Failed with code {exitCode}.", Color.Red, true);
					isDownloadActive = false;
					Log($"[🔓 WARNING] Synix close window button is now Enabled!", Color.Orange, true);
					return;
				}

				if (ServerUpdating)
				{
					Log($"[SYNIX] UPDATE FINISHED: {server.Game}", Color.Green, true);
				}
				else
				{
					Log($"[SYNIX] Validating FINISHED: {server.Game}", Color.Green, true);
				}
				ManifestMessage = "";
			}
			finally
			{
				server.Status = StatusManager.GetStatus(ServerState.Stopped);
				server.SteamPID = null;
				FileHandler.SaveServers();
				Core.Instance.UpdateGridStatus();
			}
			isDownloadActive = false;
			Log($"[🔓 WARNING] Synix close window button is now Enabled!", Color.Orange, true);
		}

		public async Task AddServerAndReport()
		{
			using (ServerSettingsGUI settingsForm = new())
			{
				if (settingsForm.ShowDialog() == DialogResult.OK && settingsForm.NewServer != null)
				{
					GameServer newServer = settingsForm.NewServer;
					var gameData = GameDatabase.GetGame(newServer.Game);
					GameFix.ManualConfigWasCreated = false;

					if (gameData == null || string.IsNullOrEmpty(gameData.AppID))
					{
						Log("Could not find the AppID for this game. Installation aborted.", Color.Red, true);
						return;
					}

					try
					{
						isDownloadActive = true;
						Log($"[⚠ WARNING] Synix close window button is now Disabled!", Color.Orange, true);

						Log($"[SYNIX] AUTO-INSTALL STARTED: {newServer.Game}", Color.LightCyan, true);
						newServer.Status = StatusManager.GetStatus(ServerState.Installing);
						Core.Instance.UpdateGridStatus();

						int exitCode = await Task.Run(() =>
						{
							return ServerInstaller.Install(newServer, gameData,
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
							Log($"Installation Failed!\n\nReason: {errorMsg}", Color.Red, true);
							newServer.Status = "Failed";
							return;
						}

						bool fixApplied = await GameFix.PostInstall(newServer);
						if (fixApplied) Log($"[✔️ SUCCESS] Re-applied missing files to the {newServer.Game} server.", Color.Green);
						newServer.IsFirstBoot = fixApplied;
						Log($"AUTO-INSTALL FINISHED: {newServer.Game}", Color.Green, true);
					}
					catch (Exception ex)
					{
						Log($"An unexpected error occurred during installation: {ex.Message}", Color.Red, true);
					}
					finally
					{
						newServer.Status = StatusManager.GetStatus(ServerState.Stopped);
						newServer.SteamPID = null;
						isDownloadActive = false;
						FileHandler.SaveServers();
						Core.Instance.UpdateGridStatus();
					}
					Log($"[⚠ WARNING] Synix close window button is now Enabled!", Color.Orange, true);
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
					Core.Instance.UpdateGridStatus();
				}
			}
		}

		public async Task ExecuteStartSequence(GameServer server, string status = "")
		{
			lock (_activeSequences)
			{
				if (_activeSequences.Contains(server.ServerName))
				{
					return;
				}
				_activeSequences.Add(server.ServerName);
			}

			try
			{
				bool stopServer = false;
				StartContext currentContext = StartContext.Manual;

				if (!PassResourceGuard(out string guardMsg))
				{
					Log(guardMsg, System.Drawing.Color.Red, true);
					MessageBox.Show(guardMsg, "System Resource Exhaustion",
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
					Log($"[🛠 MAINTENANCE] Scheduled restart sequence for {server.ServerName}.", Color.Cyan, true);
					stopServer = true;
					currentContext = StartContext.Scheduled;
				}
				else if (status == "WATCHDOG")
				{
					server.Status = StatusManager.GetStatus(ServerState.Crashed);
					string reason = !server.RunningProcess?.Responding ?? false ? "FREEZE" : "CRASH/CLOSE";
					Log($"[🛡️ WATCHDOG] {reason} detected on {server.ServerName}. Initializing recovery...", Color.Orange);

					_ = SendDiscordAlert(server, "🚨 CRASH DETECTED",
					$"{server.ServerName} has terminated. Synix is attempting an automatic restart.",
					Color.Red);

					stopServer = true;
					currentContext = StartContext.CrashRecovery;
					Core.Instance.UpdateGridStatus();
				}

				if (stopServer && server.PID != null)
				{
					Log($"[SYNIX] Stoping the {server.ServerName} server.", Color.Cyan, true);
					await StopServerAndReport(server);
				}

				if (server.Status == StatusManager.GetStatus(ServerState.Stopped))
				{
					Log($"[SYNIX] Starting the {server.ServerName} server.", Color.Cyan, true);
					if (!PassSpamLock(server, out string lockMsg, "Start")) { Log(lockMsg, System.Drawing.Color.Orange); return; }

					await Servers.Start(server, (msg, Color) => MainGUI.Instance?.Invoke((Action)(() => Log(msg, Color))), currentContext);
				}
				else
				{
					if (server.Status != StatusManager.GetStatus(ServerState.Starting))
					{
						Log($"[🚨 CRITICAL] Restart failed: {server.ServerName} is still stuck!", Color.Red);
					}
				}
				stopServer = false;
			}
			catch (Exception ex)
			{
				Log($"[🚨 CRITICAL ENGINE ERROR] Sequence failed for {server.ServerName}: {ex.Message}", Color.Red, true);
			}
			finally
			{
				lock (_activeSequences)
				{
					_activeSequences.Remove(server.ServerName);
				}
			}
		}

		public void RunUniversalHealthCheck()
		{
			foreach (var server in MainGUI.serverList)
			{
				if (server.Status == StatusManager.GetStatus(ServerState.Running))
				{
					if (server.RunningProcess == null || server.RunningProcess.HasExited)
					{
						_ = ExecuteStartSequence(server, "WATCHDOG");
						continue;
					}

					try
					{
						server.RunningProcess.Refresh();
						if (!server.RunningProcess.Responding)
						{
							_ = ExecuteStartSequence(server, "WATCHDOG");
						}
					}
					catch { /* Process might have closed during the check */ }
				}
			}
		}

		public bool ExportServerToBatch(GameServer server)
		{
			if (server == null || string.IsNullOrWhiteSpace(server.InstallPath))
			{
				Log("[🚨 ERROR] Cannot export: Invalid server or missing install path.", Color.Red);
				return false;
			}

			var dbEntry = GameDatabase.GetGame(server.Game);
			if (dbEntry == null)
			{
				Log($"[🚨 ERROR] Game database entry for '{server.Game}' not found.", Color.Red);
				return false;
			}

			if (server.Game == "Dune: Awakening")
			{
				Log("[⚠️ NOTICE] Dune: Awakening requires the official battlegroup.bat script. Export aborted.", Color.Orange);
				MessageBox.Show(
					"Dune: Awakening relies on a dedicated Hyper-V deployment script (battlegroup.bat) to initialize its virtual machine cluster.\n\nA standard batch file cannot be generated for this game.",
					"Export Disabled",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return false;
			}

			try
			{
				string targetId = dbEntry.AppID ?? "";
				string invokedId = targetId;
				string appidPath = "";

				try
				{
					var scanner = Directory.EnumerateFiles(server.InstallPath, "steam_appid.txt", new EnumerationOptions
					{
						RecurseSubdirectories = true,
						IgnoreInaccessible = true,
						MaxRecursionDepth = int.MaxValue,
						AttributesToSkip = FileAttributes.ReparsePoint
					});

					appidPath = scanner.FirstOrDefault() ?? "";
				}
				catch
				{
					appidPath = Path.Combine(server.InstallPath, "steam_appid.txt");
				}

				if (string.IsNullOrEmpty(appidPath))
				{
					appidPath = Path.Combine(server.InstallPath, "steam_appid.txt");
				}

				if (File.Exists(appidPath))
				{
					try
					{
						string fileContent = File.ReadAllText(appidPath).Trim();
						if (!string.IsNullOrWhiteSpace(fileContent))
						{
							invokedId = fileContent;
						}
					}
					catch { /* Silent fail */ }
				}

				string cleanIdentity = GetSafeName(server.ServerName ?? "Server");
				string args = dbEntry.RequiredArgs ?? "";

				args = args.Replace("{app_port}", server.AppPort?.ToString() ?? "0")
						   .Replace("{seed}", string.IsNullOrWhiteSpace(server.WorldSeed) ? "12345" : server.WorldSeed)
						   .Replace("{map}", server.WorldName ?? "")
						   .Replace("{steamAppID}", invokedId)
						   .Replace("{appid}", targetId)
						   .Replace("{port}", server.Port.ToString())
						   .Replace("{query}", server.QueryPort.ToString())
						   .Replace("{MaxPlayers}", server.MaxPlayers.ToString())
						   .Replace("{pass}", server.Password ?? "")
						   .Replace("{adminpass}", server.AdminPassword ?? "")
						   .Replace("{ServerName}", server.ServerName ?? "SynixServer")
						   .Replace("{InstallPath}", server.InstallPath ?? "")
						   .Replace("{Identity}", cleanIdentity)
						   .Replace("{world_size}", server.WorldSize.ToString());

				if (args.Contains("{rcon}"))
				{
					string formattedRcon = server.EnableRcon && !string.IsNullOrWhiteSpace(dbEntry.RconSyntax)
						? dbEntry.RconSyntax.Replace("{rcon_port}", server.RconPort.ToString()).Replace("{rcon_pass}", server.RconPassword ?? "")
						: "";
					args = args.Replace("{rcon}", formattedRcon);
				}

				if (args.Contains("{mode}") && !string.IsNullOrWhiteSpace(server.GameMode))
				{
					string translatedMode = (server.GameMode == "PVE" && ((server.Game?.Contains("ARK") ?? false) || server.Game == "Atlas" || server.Game == "Rust"))
						? "True" : (server.GameMode == "PVP" && ((server.Game?.Contains("ARK") ?? false) || server.Game == "Atlas" || server.Game == "Rust"))
						? "False" : server.GameMode;
					args = args.Replace("{mode}", translatedMode);
				}

				if (!string.IsNullOrWhiteSpace(server.ExtraArgs))
				{
					args = args + " " + server.ExtraArgs.Trim();
				}

				args = args.Replace("  ", " ").Trim();

				string fullExePath = Path.Combine(server.InstallPath, dbEntry.ExeName ?? "");
				string binDir = Path.GetDirectoryName(fullExePath) ?? server.InstallPath;
				string exeNameOnly = Path.GetFileName(fullExePath);

				// 4. CONSTRUCT ISOLATED BATCH SCRIPT
				StringBuilder batchContent = new StringBuilder();
				batchContent.AppendLine("@echo off");
				batchContent.AppendLine($"echo :: ===========================================================================");
				batchContent.AppendLine($"echo :: SYNIX AUTOMATICALLY GENERATED LAUNCH SCRIPT");
				batchContent.AppendLine($"echo :: Server: {server.ServerName} ({server.Game})");
				batchContent.AppendLine($"echo :: Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
				batchContent.AppendLine($"echo :: ===========================================================================");
				batchContent.AppendLine();
				batchContent.AppendLine($":: Move execution context to the actual binaries directory");
				batchContent.AppendLine($"cd /d \"{binDir}\"");
				batchContent.AppendLine();
				batchContent.AppendLine($":: Inject Steam App Variables into Windows Memory");
				batchContent.AppendLine($"set SteamAppId={invokedId}");
				batchContent.AppendLine($"set SteamGameId={invokedId}");
				batchContent.AppendLine();
				batchContent.AppendLine($":: Execute the standalone server payload");
				batchContent.AppendLine($"start \"{server.ServerName}\" \"{exeNameOnly}\" {args}");
				batchContent.AppendLine();
				batchContent.AppendLine("echo.");
				batchContent.AppendLine($"echo Starting the {server.ServerName} Server. Please wait...");
				batchContent.AppendLine("timeout /t 5 /nobreak >nul");
				batchContent.AppendLine("echo.");
				batchContent.AppendLine("echo Press any key to close this window.");
				batchContent.AppendLine("pause >nul");

				string safeFileName = $"Run_{cleanIdentity}_Server.bat";
				string fullOutputPath = Path.Combine(server.InstallPath, safeFileName);

				File.WriteAllText(fullOutputPath, batchContent.ToString());

				Log($"[✔️ SUCCESS] Exported launch script to: {fullOutputPath}", Color.SpringGreen);
				return true;
			}
			catch (Exception ex)
			{
				Log($"[🚨 ERROR] Failed to generate batch file payload: {ex.Message}", Color.Red, true);
				return false;
			}
		}

		public void CleanFirewallRules(string executablePath)
		{
			try
			{
				ProcessStartInfo psi = new ProcessStartInfo
				{
					FileName = "netsh",
					Arguments = $"advfirewall firewall delete rule name=all program=\"{executablePath}\"",
					UseShellExecute = true,
					Verb = "runas",
					WindowStyle = ProcessWindowStyle.Hidden
				};

				Process cleanup = Process.Start(psi);
				cleanup?.WaitForExit();

				Log($"[FIREWALL] Successfully removed rules for {executablePath}", Color.LimeGreen);
			}
			catch (System.ComponentModel.Win32Exception)
			{
				Log("[FIREWALL] User denied Admin rights. Rule was not deleted.", Color.Orange, true);
			}
			catch (Exception ex)
			{
				Log($"[FIREWALL ERROR] {ex.Message}", Color.Red, true);
			}
		}
	}
}