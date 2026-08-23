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
using Synix_Control_Panel.SynixApp.Database.GameConfigurations;
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

			bool stopped = await Servers.Stop(server, (msg, logColor) =>
			{
				Log(msg, logColor);
			}, isManual);

			if (!stopped)
			{
				Log($"[🚨 STOP FAILED] {server.ServerName} is still running. Synix kept its live PID and status.", Color.Red, true);
			}
			else
			{
				RecordGameVerification(server.Game, GameVerificationKind.Stop);
				await CollectGeneratedConfigurationAfterStop(server);
				await SynchronizeFirstGeneratedConfiguration(server);
			}

			FileHandler.SaveServers();
			Core.Instance.UpdateGridStatus();
		}

		internal async Task SynchronizeFirstGeneratedConfiguration(GameServer server)
		{
			ConfigurationApplyResult? optionalResult =
				await GameFix.ApplyFirstGeneratedConfiguration(server);
			if (!optionalResult.HasValue)
			{
				return;
			}
			ConfigurationApplyResult result = optionalResult.Value;

			if (!result.Succeeded)
			{
				Log($"[CONFIG ERROR] {result.Message}", Color.Red, true);
				return;
			}

			if (!result.Complete)
			{
				Log($"[CONFIG WARNING] {result.Message}", Color.Orange, true);
				return;
			}

			if (result.Changed)
			{
				Log(
					$"[CONFIG] Applied the saved Synix settings to the newly generated {server.Game} configuration.",
					Color.LimeGreen,
					true);
			}
		}

		private async Task CollectGeneratedConfigurationAfterStop(GameServer server)
		{
			if (!GeneratedConfigurationCollector.AutomaticCollectionEnabled ||
				GameFix.GetConfigFileCreationMode(server.Game) is
					ConfigFileCreationMode.SynixTemplate or
					ConfigFileCreationMode.LaunchArgumentsOnly)
			{
				return;
			}

			try
			{
				GeneratedConfigurationCaptureResult result = await Task.Run(() =>
					GeneratedConfigurationCollector.CollectServer(server));
				if (result.CopiedFiles > 0)
				{
					Log(
						$"[CONFIG CAPTURE] Copied {result.CopiedFiles} generated configuration file(s) for {server.ServerName} to {result.DestinationRoot}.",
						Color.Cyan);
				}

				foreach (string error in result.Errors.Take(3))
				{
					Log($"[CONFIG CAPTURE] {error}", Color.OrangeRed);
				}
			}
			catch (Exception exception)
			{
				Log(
					$"[CONFIG CAPTURE] Could not collect the generated configuration for {server.ServerName}: {exception.Message}",
					Color.OrangeRed);
			}
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

			string fullPath;
			ConfigFormat format = blueprint.Format;
			if (GameFix.TryGetConfiguration(
				server.Game,
				out ConfigurationDefinition? definition) &&
				definition?.UsesConfigurationFile == true)
			{
				try
				{
					fullPath = definition.ResolveFullPath(server);
					format = definition.Format;
				}
				catch (Exception exception)
				{
					Log($"Could not resolve the config file safely:\n{exception.Message}", Color.Red, true);
					return;
				}
			}
			else
			{
				string cleanIdentity = GetSafeName(server.ServerName);
				string resolvedRelativePath = blueprint.RelativeConfigPath
					.Replace("{Identity}", cleanIdentity)
					.Replace("{ServerName}", cleanIdentity)
					.Replace("{map}", server.WorldName ?? string.Empty)
					.Replace("{port}", server.Port.ToString())
					.Replace("{query}", server.QueryPort.ToString())
					.Replace('/', Path.DirectorySeparatorChar)
					.Replace('\\', Path.DirectorySeparatorChar);
				string installRoot = Path.GetFullPath(server.InstallPath)
					.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				fullPath = Path.GetFullPath(Path.Combine(installRoot, resolvedRelativePath));
				if (!fullPath.StartsWith(
					installRoot + Path.DirectorySeparatorChar,
					StringComparison.OrdinalIgnoreCase))
				{
					Log("The config path leaves the server installation folder.", Color.Red, true);
					return;
				}
			}

			if (File.Exists(fullPath) || GameFix.CanResetManagedConfiguration(server))
			{
				using (ServerConfig editor = new ServerConfig(
					fullPath,
					format,
					server))
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
						string executableName = GameDatabase.GetGame(server.Game)?.ExeName ?? string.Empty;
						string serverExePath = Path.Combine(server.InstallPath, executableName);

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

			string fullPath = Path.Combine(DefaultBackupPath, cleanGame, cleanServer);

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

			if (!EnsureSteamAccountName(server, gameData))
				return;

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
				string manifestPath = Path.Combine(steamAppsPath, $"appmanifest_{gameData.AppID}.acf");

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

				bool fixApplied = await GameFix.PostInstall(server);
				if (fixApplied)
					Log($"[✔️ SUCCESS] Re-applied required files to the {server.Game} server.", Color.Green);
				if (OxideRuntimeManager.RequiresVanillaRestore(server, gameData))
				{
					server.ServerFrameworkVersion = "Official";
					Log(
						"[OXIDE] Steam restored the official Rust server files. The server is now set to Vanilla; user plugin files were left untouched.",
						Color.LimeGreen,
						true);
				}
				if (OxideRuntimeManager.IsEnabled(server, gameData))
				{
					try
					{
						await OxideRuntimeManager.InstallOrUpdateAsync(
							server,
							gameData,
							(message, color) => Log(message, color, true));
					}
					catch (Exception exception)
					{
						Log($"[OXIDE ERROR] {exception.Message}", Color.Red, true);
						MessageBox.Show(
							$"The Rust {ManifestMessage} completed, but Oxide could not be reapplied. Synix will block the modded server from starting until you retry with Update or Validate.\n\n{exception.Message}",
							"Oxide Update Failed",
							MessageBoxButtons.OK,
							MessageBoxIcon.Error);
						return;
					}
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
				isDownloadActive = false;
				FileHandler.SaveServers();
				Core.Instance.UpdateGridStatus();
				Log("[🔓 WARNING] Synix close window button is now Enabled!", Color.Orange, true);
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
						if (OxideRuntimeManager.IsEnabled(newServer, gameData))
						{
							try
							{
								await OxideRuntimeManager.InstallOrUpdateAsync(
									newServer,
									gameData,
									(message, color) => Log(message, color, true));
							}
							catch (Exception exception)
							{
								Log($"[OXIDE ERROR] {exception.Message}", Color.Red, true);
								MessageBox.Show(
									"The Rust server installed, but Oxide could not be installed. Synix will block the modded server from starting until you retry with Update or Validate.\n\n" + exception.Message,
									"Oxide Installation Failed",
									MessageBoxButtons.OK,
									MessageBoxIcon.Error);
								return;
							}
						}
						newServer.IsFirstBoot =
							fixApplied ||
							gameData.NeedsConfigWarning ||
							gameData.RequiredLaunchFiles.Length > 0;
						if (await RefreshServerIconAsync(newServer))
						{
							Core.Instance.UpdateGridStatus();
							Log($"[ICON] Updated the dashboard icon for {newServer.Game}.", Color.Cyan);
						}
						Log($"AUTO-INSTALL FINISHED: {newServer.Game}", Color.Green, true);
						RecordGameVerification(newServer.Game, GameVerificationKind.Install);
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
						Log($"[⚠ WARNING] Synix close window button is now Enabled!", Color.Orange, true);
					}
				}
			}
		}

		private bool EnsureSteamAccountName(
			GameServer server,
			GameInfo blueprint,
			bool forcePrompt = false,
			bool restoringImportedServer = false)
		{
			if (!blueprint.RequiresSteamLogin ||
				(!forcePrompt &&
				 !string.IsNullOrWhiteSpace(server.SteamAccountName)))
			{
				return true;
			}

			using SteamAccountLoginDialog loginDialog = new(
				blueprint.Game,
				server.SteamAccountName,
				restoringImportedServer);
			if (loginDialog.ShowDialog(MainGUI.Instance) != DialogResult.OK)
			{
				Log(
					restoringImportedServer
						? "Steam authorization was cancelled. The server was not started."
						: "Steam account login was cancelled. No files were changed.",
					Color.Orange,
					true);
				return false;
			}

			server.SteamAccountName = loginDialog.SteamAccountName;
			FileHandler.SaveServers();
			return true;
		}

		private async Task<bool> EnsureSteamAuthenticationAfterImport(
			GameServer server,
			string status)
		{
			if (!server.SteamAuthenticationRequired)
				return true;

			GameInfo? blueprint = GameDatabase.GetGame(server.Game);
			if (blueprint?.RequiresSteamLogin != true)
			{
				server.SteamAuthenticationRequired = false;
				FileHandler.SaveServers();
				return true;
			}

			if (status is "WATCHDOG" or "MAINTENANCE")
			{
				Log(
					$"[STEAM LOGIN] {server.ServerName} was imported and needs Steam authorization on this PC. Start it manually once to complete the login.",
					Color.Orange,
					true);
				return false;
			}

			if (!EnsureSteamAccountName(
					server,
					blueprint,
					forcePrompt: true,
					restoringImportedServer: true))
			{
				return false;
			}

			if (!File.Exists(Core.SteamCmdExe))
			{
				await SteamCMD.EnsureSteamCMD((message, color) => Log(message, color));
				if (!File.Exists(Core.SteamCmdExe))
				{
					Log(
						"SteamCMD could not be prepared, so Steam authorization could not start.",
						Color.Red,
						true);
					return false;
				}
			}

			try
			{
				isDownloadActive = true;
				Log(
					"[LOCKED] Synix cannot close while Steam authorization is running.",
					Color.Orange,
					true);

				int exitCode = await Task.Run(() =>
					ServerInstaller.AuthenticateSteamAccount(
						server,
						blueprint,
						message => MainGUI.Instance?.BeginInvoke(
							(Action)(() => Log(message))),
						pid =>
						{
							server.SteamPID = pid;
							FileHandler.SaveServers();
						}));

				if (exitCode != 0)
				{
					Log(
						$"Steam authorization was not completed. {ServerInstaller.GetSteamError(exitCode)} The server was not started.",
						Color.Red,
						true);
					return false;
				}

				server.SteamAuthenticationRequired = false;
				FileHandler.SaveServers();
				Log(
					$"[STEAM LOGIN] {server.ServerName} is authorized on this PC.",
					Color.Green,
					true);
				return true;
			}
			finally
			{
				server.SteamPID = null;
				isDownloadActive = false;
				FileHandler.SaveServers();
				Core.Instance.UpdateGridStatus();
				Log(
					"[UNLOCKED] Synix can close again.",
					Color.Orange,
					true);
			}
		}

		internal static int MarkImportedSteamAuthenticationRequired(
			IEnumerable<GameServer> servers)
		{
			ArgumentNullException.ThrowIfNull(servers);

			int authenticationCount = 0;
			foreach (GameServer server in servers)
			{
				GameInfo? blueprint = GameDatabase.GetGame(server.Game);
				server.SteamAuthenticationRequired =
					blueprint?.RequiresSteamLogin == true;

				if (server.SteamAuthenticationRequired)
					authenticationCount++;
			}

			return authenticationCount;
		}

		public async Task EditServerAndReport(GameServer server)
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

			string previousFramework = server.ServerFramework ?? "Vanilla";
			using (var editForm = new ServerSettingsGUI(server))
			{
				if (editForm.ShowDialog() == DialogResult.OK && editForm.NewServer != null)
				{
					GameServer updatedServer = editForm.NewServer;
					ConfigurationApplyResult configurationResult =
						await GameFix.ApplyManagedConfiguration(updatedServer);

					if (!configurationResult.Succeeded)
					{
						Log($"[CONFIG ERROR] {configurationResult.Message}", Color.Red, true);
					}
					else if (!configurationResult.Complete)
					{
						Log($"[CONFIG WARNING] {configurationResult.Message}", Color.Orange, true);
					}
					else if (configurationResult.Changed)
					{
						Log($"[CONFIG] {configurationResult.Message}", Color.Green);
					}

					GameInfo? definition = GameDatabase.GetGame(updatedServer.Game);
					if (definition != null &&
						OxideRuntimeManager.IsEnabled(updatedServer, definition) &&
						!previousFramework.Equals(
							OxideRuntimeManager.FrameworkName,
							StringComparison.OrdinalIgnoreCase))
					{
						try
						{
							isDownloadActive = true;
							await OxideRuntimeManager.InstallOrUpdateAsync(
								updatedServer,
								definition,
								(message, color) => Log(message, color, true));
						}
						catch (Exception exception)
						{
							updatedServer.ServerFramework = OxideRuntimeManager.VanillaFrameworkName;
							updatedServer.ServerFrameworkVersion = "Official";
							Log($"[OXIDE ERROR] {exception.Message}", Color.Red, true);
							MessageBox.Show(
								"Oxide could not be installed. The server has been left set to Vanilla.\n\n" + exception.Message,
								"Oxide Installation Failed",
								MessageBoxButtons.OK,
								MessageBoxIcon.Error);
						}
						finally
						{
							isDownloadActive = false;
						}
					}
					else if (previousFramework.Equals(
						OxideRuntimeManager.FrameworkName,
						StringComparison.OrdinalIgnoreCase) &&
						string.Equals(
							updatedServer.ServerFramework,
							OxideRuntimeManager.VanillaFrameworkName,
							StringComparison.OrdinalIgnoreCase))
					{
						updatedServer.ServerFrameworkVersion =
							OxideRuntimeManager.VanillaRestoreRequiredVersion;
						Log(
							"[OXIDE] Framework set to Vanilla. Start is blocked until Update or Validate restores the official Rust server files.",
							Color.Orange,
							true);
						MessageBox.Show(
							"Rust is now set to Vanilla. Run Update or Validate before starting so Steam can restore the official server files.\n\nSynix will not delete your oxide folder or plugins.",
							"Validation Required",
							MessageBoxButtons.OK,
							MessageBoxIcon.Information);
					}

					FileHandler.SaveServers();
					Log($"[✔️ SUCCESS] {updatedServer.ServerName} settings updated and saved.", Color.Green);
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

				if (!await EnsureSteamAuthenticationAfterImport(server, status))
					return;

				if (!ValidateIntegrityAndReport(server)) return;
				if (GameFix.NeedsManagedConfiguration(server))
				{
					ConfigurationApplyResult configurationResult =
						await GameFix.ApplyManagedConfiguration(server);

					if (!configurationResult.Succeeded)
					{
						Log($"[CONFIG ERROR] {configurationResult.Message}", Color.Red, true);
						MessageBox.Show(
							configurationResult.Message,
							"Configuration Could Not Be Applied",
							MessageBoxButtons.OK,
							MessageBoxIcon.Error);
						return;
					}

					if (!configurationResult.Complete)
					{
						Log($"[CONFIG WARNING] {configurationResult.Message}", Color.Orange, true);
					}
					else if (configurationResult.Changed)
					{
						Log($"[CONFIG] {configurationResult.Message}", Color.Green);
					}

					FileHandler.SaveServers();
				}
				bool displayedFirstBootWarning = server.IsFirstBoot;
				if (ShouldBlockForConfig(server)) return;
				if (!EnsureRequiredLaunchFilesAndReport(
					server,
					showDialog:
						!displayedFirstBootWarning &&
						!status.Equals("WATCHDOG", StringComparison.OrdinalIgnoreCase)))
				{
					return;
				}

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

					await StopServerAndReport(server, isManual: status == "RESTART");
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
					catch { }
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

			SynixServerPasswords batchPasswords;
			try
			{
				batchPasswords = Core
					.RevealServerPasswords(server);
			}
			catch (SynixPasswordProtectionException)
			{
				Log(
					"[🚨 ERROR] Synix could not unlock the saved passwords. Re-enter them in Server Settings before exporting a launch file.",
					Color.Red);
				return false;
			}

			if (!dbEntry.LaunchBehavior.AllowLaunchFileExport)
			{
				Log($"[⚠️ NOTICE] {server.Game} does not allow generated launch files. Export aborted.", Color.Orange);
				MessageBox.Show(
					$"{server.Game} relies on its official launch or deployment file. A separate launch file cannot be safely generated for this game.",
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
					catch { }
				}

				string cleanIdentity = GetSafeName(server.ServerName ?? "Server");
				if (!GameLaunchCommandBuilder.TryBuildArguments(
					server,
					dbEntry,
					invokedId,
					batchPasswords,
					out string args,
					out string argumentError))
				{
					Log(
						$"[🚨 SECURITY] Launch file export blocked: {argumentError}",
						Color.Red,
						true);
					return false;
				}

				string safeArgs = EscapeWindowsBatchCommandLine(args);

				string fullExePath = Path.Combine(server.InstallPath, dbEntry.ExeName ?? "");
				string binDir = Path.GetDirectoryName(fullExePath) ?? server.InstallPath;
				string exeNameOnly = Path.GetFileName(fullExePath);
				string safeIdentity = EscapeWindowsBatchCommandLine(cleanIdentity);
				string safeBinDir = EscapeWindowsBatchCommandLine(binDir);
				string safeExeName = EscapeWindowsBatchCommandLine(exeNameOnly);
				string safeInvokedId = EscapeWindowsBatchCommandLine(invokedId);

				StringBuilder batchContent = new StringBuilder();
				batchContent.AppendLine("@echo off");
				batchContent.AppendLine("setlocal DisableDelayedExpansion");
				batchContent.AppendLine($"echo :: ===========================================================================");
				batchContent.AppendLine($"echo :: SYNIX AUTOMATICALLY GENERATED LAUNCH SCRIPT");
				batchContent.AppendLine($"echo :: Server: {safeIdentity}");
				batchContent.AppendLine($"echo :: Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
				batchContent.AppendLine($"echo :: ===========================================================================");
				batchContent.AppendLine();
				batchContent.AppendLine($":: Move execution context to the actual binaries directory");
				batchContent.AppendLine($"cd /d \"{safeBinDir}\"");
				batchContent.AppendLine();
				batchContent.AppendLine($":: Inject Steam App Variables into Windows Memory");
				batchContent.AppendLine($"set \"SteamAppId={safeInvokedId}\"");
				batchContent.AppendLine($"set \"SteamGameId={safeInvokedId}\"");
				batchContent.AppendLine();
				batchContent.AppendLine($":: Execute the standalone server payload and instantly close this script window");
				batchContent.AppendLine($"start \"\" \"{safeExeName}\" {safeArgs}");
				batchContent.AppendLine("exit");

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
