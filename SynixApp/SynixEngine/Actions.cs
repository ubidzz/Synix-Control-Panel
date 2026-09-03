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
		public async Task<bool> StopServerAndReport(GameServer server, bool isManual = true)
		{
			using ServerOperationLease operation =
				ServerOperationCoordinator.TryBegin(server, ServerOperationKind.Stop);
			if (!operation.Acquired)
			{
				Log($"[STOP BLOCKED] {operation.FailureReason}", Color.Orange, true);
				return false;
			}

			server.Status = StatusManager.GetStatus(ServerState.Stopping);
			Core.Instance.UpdateGridStatus();
			_ = SendDiscordNotification(
				server,
				DiscordNotificationEvent.ServerStopping,
				isManual ? "SERVER STOPPING" : "AUTOMATIC STOP",
				isManual
					? "A shutdown command was issued from Synix."
					: "Synix is stopping the server as part of an automatic operation.",
				Color.Orange);

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
				_ = SendDiscordNotification(
					server,
					DiscordNotificationEvent.ServerStopped,
					"SERVER STOPPED",
					$"{server.ServerName} is fully stopped.",
					Color.LimeGreen);
			}

			FileHandler.SaveServers();
			Core.Instance.UpdateGridStatus();
			return stopped;
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
				_ = SendDiscordNotification(
					server,
					DiscordNotificationEvent.ConfigurationWarning,
					"CONFIGURATION ERROR",
					result.Message,
					Color.Red);
				return;
			}

			if (!result.Complete)
			{
				Log($"[CONFIG WARNING] {result.Message}", Color.Orange, true);
				_ = SendDiscordNotification(
					server,
					DiscordNotificationEvent.ConfigurationWarning,
					"CONFIGURATION WARNING",
					result.Message,
					Color.Orange);
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
			if (!GeneratedConfigurationCollector.AutomaticCollectionEnabled)
			{
				return;
			}

			bool includeAllGeneratedFiles = !GameFix.ManagedConfigurationsEnabled;
			if (!includeAllGeneratedFiles &&
				GameFix.GetConfigFileCreationMode(server.Game) is
					ConfigFileCreationMode.SynixTemplate or
					ConfigFileCreationMode.LaunchArgumentsOnly)
			{
				return;
			}

			try
			{
				GeneratedConfigurationCaptureResult result = await Task.Run(() =>
					GeneratedConfigurationCollector.CollectServer(
						server,
						includeAllGeneratedFiles: includeAllGeneratedFiles));
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

			if (string.IsNullOrWhiteSpace(server.InstallPath))
			{
				Log("Server installation path is not set.", Color.Red, true);
				return;
			}

			IReadOnlyList<ConfigurationEditorFile> configurationFiles;
			try
			{
				PrepareConfigurationEditorFiles(server);
				configurationFiles = ResolveConfigurationEditorFiles(server);
			}
			catch (Exception exception)
			{
				Log($"Could not resolve the config file safely:\n{exception.Message}", Color.Red, true);
				return;
			}

			if (configurationFiles.Count == 0)
			{
				Log("This game does not have a config path defined.", Color.Red, true);
				return;
			}

			if (configurationFiles.Any(file => File.Exists(file.Path)) ||
				GameFix.CanResetManagedConfiguration(server))
			{
				try
				{
					using ServerConfig editor = new(configurationFiles, server);
					editor.ShowDialog(MainGUI.Instance);
				}
				catch (Exception exception)
				{
					Log(
						$"[CONFIG EDITOR] Could not open the configuration editor: {exception.Message}",
						Color.Red,
						true);
					PlainEnglishErrorDialog.ShowError(
						MainGUI.Instance,
						"open the configuration editor",
						exception.ToString());
				}
			}
			else
			{
				string locations = string.Join(
					Environment.NewLine,
					configurationFiles.Select(file => file.Path));
				Log($"Could not find the game configuration file(s) at:\n{locations}", Color.Red, true);
			}
		}

		internal static void PrepareConfigurationEditorFiles(GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);
			if (GameFix.TryGetConfiguration(
				server.Game,
				out ConfigurationDefinition? definition) &&
				definition?.UsesConfigurationFile == true)
			{
				definition.PrepareConfigurationFilesForEditing(server);
			}
		}

		internal static IReadOnlyList<ConfigurationEditorFile>
			ResolveConfigurationEditorFiles(GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);
			if (string.IsNullOrWhiteSpace(server.InstallPath))
				return [];

			Dictionary<string, ConfigurationEditorFile> files =
				new(StringComparer.OrdinalIgnoreCase);
			if (GameFix.TryGetConfiguration(
				server.Game,
				out ConfigurationDefinition? definition) &&
				definition?.UsesConfigurationFile == true)
			{
				foreach (string path in definition.ResolveConfigurationPaths(server))
					files[path] = new ConfigurationEditorFile(path, definition.Format);
			}

			GameInfo? blueprint = GameDatabase.GetGame(server.Game);
			if (blueprint != null && !string.IsNullOrWhiteSpace(blueprint.RelativeConfigPath))
			{
				string fullPath = ResolveConfigurationEditorPath(
					server,
					blueprint.RelativeConfigPath);
				files.TryAdd(
					fullPath,
					new ConfigurationEditorFile(fullPath, blueprint.Format));
			}

			AddSiblingConfigurationFiles(server, files);

			return files.Values.ToArray();
		}

		private static void AddSiblingConfigurationFiles(
			GameServer server,
			Dictionary<string, ConfigurationEditorFile> files)
		{
			foreach (ConfigurationEditorFile declaredFile in files.Values.ToArray())
			{
				string? directory = Path.GetDirectoryName(declaredFile.Path);
				if (string.IsNullOrWhiteSpace(directory) ||
					!Directory.Exists(directory) ||
					!IsDeclaredConfigurationDirectory(server, directory))
				{
					continue;
				}

				string declaredExtension = Path.GetExtension(declaredFile.Path);
				foreach (string path in Directory
					.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly)
					.OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase))
				{
					string fileName = Path.GetFileName(path);
					if (files.ContainsKey(path) || IsEditorFileExcluded(fileName))
						continue;

					ConfigFormat format;
					if (Path.GetExtension(path).Equals(
						declaredExtension,
						StringComparison.OrdinalIgnoreCase))
					{
						format = declaredFile.Format;
					}
					else if (!ConfigHandler.TryGetFormatFromPath(path, out format))
					{
						continue;
					}

					files[path] = new ConfigurationEditorFile(path, format);
				}
			}
		}

		private static bool IsDeclaredConfigurationDirectory(
			GameServer server,
			string directory)
		{
			string installRoot = Path.GetFullPath(server.InstallPath)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string fullDirectory = Path.GetFullPath(directory)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			if (fullDirectory.Equals(installRoot, StringComparison.OrdinalIgnoreCase))
				return false;

			string relativeDirectory = Path.GetRelativePath(installRoot, fullDirectory);
			if (relativeDirectory.StartsWith("..", StringComparison.Ordinal) ||
				Path.IsPathRooted(relativeDirectory))
			{
				return false;
			}

			return relativeDirectory.Split(
				[Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
				StringSplitOptions.RemoveEmptyEntries)
				.Any(part =>
					part.Equals("cfg", StringComparison.OrdinalIgnoreCase) ||
					part.Contains("config", StringComparison.OrdinalIgnoreCase) ||
					part.Contains("setting", StringComparison.OrdinalIgnoreCase));
		}

		private static bool IsEditorFileExcluded(string fileName)
		{
			return fileName.StartsWith(".", StringComparison.Ordinal) ||
				fileName.Contains(".synix.", StringComparison.OrdinalIgnoreCase) ||
				fileName.EndsWith(".template", StringComparison.OrdinalIgnoreCase) ||
				fileName.EndsWith(".bak", StringComparison.OrdinalIgnoreCase) ||
				fileName.EndsWith(".backup", StringComparison.OrdinalIgnoreCase);
		}

		internal static bool CanOpenConfigurationEditor(GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);
			try
			{
				IReadOnlyList<ConfigurationEditorFile> files =
					ResolveConfigurationEditorFiles(server);
				return files.Count > 0 &&
					(files.Any(file => File.Exists(file.Path)) ||
					 GameFix.CanResetManagedConfiguration(server));
			}
			catch
			{
				return false;
			}
		}

		private static string ResolveConfigurationEditorPath(
			GameServer server,
			string relativePathTemplate)
		{
			string cleanIdentity = Instance.GetSafeName(server.ServerName);
			string resolvedRelativePath = relativePathTemplate
				.Replace("{Identity}", cleanIdentity, StringComparison.Ordinal)
				.Replace("{ServerName}", cleanIdentity, StringComparison.Ordinal)
				.Replace("{map}", server.WorldName ?? string.Empty, StringComparison.Ordinal)
				.Replace("{port}", server.Port.ToString(), StringComparison.Ordinal)
				.Replace("{query}", server.QueryPort.ToString(), StringComparison.Ordinal)
				.Replace('/', Path.DirectorySeparatorChar)
				.Replace('\\', Path.DirectorySeparatorChar);
			string installRoot = Path.GetFullPath(server.InstallPath)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string fullPath = Path.GetFullPath(Path.Combine(installRoot, resolvedRelativePath));
			if (!fullPath.StartsWith(
				installRoot + Path.DirectorySeparatorChar,
				StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException(
					"The configuration path leaves the server installation folder.");
			}

			return fullPath;
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

		public async Task<bool> DeleteServerAndReportAsync(GameServer server)
		{
			using ServerOperationLease operation =
				ServerOperationCoordinator.TryBegin(server, ServerOperationKind.Delete);
			if (!operation.Acquired)
			{
				Log($"[DELETE BLOCKED] {operation.FailureReason}", Color.Orange, true);
				return false;
			}

			string status = server.Status ?? string.Empty;
			if (status == StatusManager.GetStatus(ServerState.Installing) ||
				status == StatusManager.GetStatus(ServerState.Updating) ||
				(server.PID.HasValue && server.PID > 0))
			{
				Log("Cannot delete an active or installing server.", Color.Red, true);
				return false;
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

			TaskDialogButton result = MainGUI.Instance == null
				? TaskDialog.ShowDialog(page)
				: TaskDialog.ShowDialog(MainGUI.Instance, page);
			if (result != TaskDialogButton.Yes)
				return false;

			bool deleteBackups = page.Verification.Checked;
			string previousStatus = status;
			server.Status = StatusManager.GetStatus(ServerState.Deleting);
			UpdateGridStatus();

			try
			{
				if (Properties.Settings.Default.enableRunAsAdmin)
				{
					GameInfo? definition = GameDatabase.GetGame(server.Game);
					string executableName = definition == null
						? string.Empty
						: MinecraftControlProfile.ResolveExecutableName(server, definition);
					string serverExePath = Path.Combine(server.InstallPath, executableName);

					if (File.Exists(serverExePath))
						await CleanFirewallRulesAsync(serverExePath);
				}

				ServerFolderDeletionResult deletion =
					await FolderHandler.ServerFolder.DeleteFilesAsync(server, deleteBackups);

				if (deletion.InstallationDeleted)
				{
					Log(
						$"[CLEANUP] Deleted server '{server.ServerName}' and all files at {deletion.InstallationPath}",
						Color.Yellow);
				}
				if (deletion.BackupsDeleted)
					Log($"[CLEANUP] Deleted server backups at {deletion.BackupPath}", Color.LimeGreen);

				if (ServerRegistry.Servers.Contains(server))
					ServerRegistry.Servers.Remove(server);

				FileHandler.SaveServers();
				UpdateGridStatus();
				return true;
			}
			catch (Exception ex)
			{
				server.Status = previousStatus;
				Log($"Files were partially deleted, but an error occurred:\n{ex.Message}", Color.Red, true);
				MessageBox.Show($"Files were partially deleted, but an error occurred:\n{ex.Message}", "Deletion Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				UpdateGridStatus();
				return false;
			}
		}

		public async Task OpenBackFolderAsync(GameServer selectedServer)
		{
			IReadOnlyList<ServerBackupArchive> backups =
				await GetServerBackupsAsync(selectedServer);
			string fullPath = backups.Count > 0
				? Path.GetDirectoryName(backups[0].ArchivePath) ?? GetActiveServerBackupFolder(selectedServer)
				: GetActiveServerBackupFolder(selectedServer);

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

			ServerOperationKind operationKind = ServerUpdating
				? ServerOperationKind.Update
				: ServerOperationKind.Validate;
			using ServerOperationLease operation =
				ServerOperationCoordinator.TryBegin(server, operationKind);
			if (!operation.Acquired)
			{
				Log($"[STEAMCMD BLOCKED] {operation.FailureReason}", Color.Orange, true);
				return;
			}
			DiscordNotificationEvent startedEvent = ServerUpdating
				? DiscordNotificationEvent.UpdateStarted
				: DiscordNotificationEvent.VerificationStarted;
			DiscordNotificationEvent completedEvent = ServerUpdating
				? DiscordNotificationEvent.UpdateCompleted
				: DiscordNotificationEvent.VerificationCompleted;
			DiscordNotificationEvent failedEvent = ServerUpdating
				? DiscordNotificationEvent.UpdateFailed
				: DiscordNotificationEvent.VerificationFailed;
			string operationName = ServerUpdating ? "UPDATE" : "FILE VERIFICATION";

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
				_ = SendDiscordNotification(
					server,
					startedEvent,
					$"{operationName} STARTED",
					$"SteamCMD is processing {server.ServerName}.",
					Color.Cyan);

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
					_ = SendDiscordNotification(
						server,
						failedEvent,
						$"{operationName} FAILED",
						errorDetail,
						Color.Red);
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
						if (!IsBackgroundServiceMode)
							MessageBox.Show(
								$"The Rust {ManifestMessage} completed, but Oxide could not be reapplied. Synix will block the modded server from starting until you retry with Update or Validate.\n\n{exception.Message}",
								"Oxide Update Failed",
								MessageBoxButtons.OK,
								MessageBoxIcon.Error);
						_ = SendDiscordNotification(
							server,
							failedEvent,
							$"{operationName} FAILED",
							$"SteamCMD completed, but Oxide could not be reapplied: {exception.Message}",
							Color.Red);
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
				_ = SendDiscordNotification(
					server,
					completedEvent,
					$"{operationName} COMPLETED",
					$"{server.ServerName} completed successfully.",
					Color.LimeGreen);
				ManifestMessage = "";
			}
			catch (Exception exception)
			{
				Log($"[🚨 {operationName} ERROR] {exception.Message}", Color.Red, true);
				_ = SendDiscordNotification(
					server,
					failedEvent,
					$"{operationName} FAILED",
					exception.Message,
					Color.Red);
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

					using ServerOperationLease operation =
						ServerOperationCoordinator.TryBegin(
							newServer,
							ServerOperationKind.Install);
					if (!operation.Acquired)
					{
						Log($"[INSTALL BLOCKED] {operation.FailureReason}", Color.Orange, true);
						return;
					}

					try
					{
						isDownloadActive = true;
						Log($"[⚠ WARNING] Synix close window button is now Disabled!", Color.Orange, true);

						Log($"[SYNIX] AUTO-INSTALL STARTED: {newServer.Game}", Color.LightCyan, true);
						newServer.Status = StatusManager.GetStatus(ServerState.Installing);
						Core.Instance.UpdateGridStatus();
						_ = SendDiscordNotification(
							newServer,
							DiscordNotificationEvent.InstallStarted,
							"INSTALL STARTED",
							$"SteamCMD is installing {newServer.Game}.",
							Color.Cyan);

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
							_ = SendDiscordNotification(
								newServer,
								DiscordNotificationEvent.InstallFailed,
								"INSTALL FAILED",
								errorMsg,
								Color.Red);
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
								_ = SendDiscordNotification(
									newServer,
									DiscordNotificationEvent.InstallFailed,
									"INSTALL FAILED",
									$"The server files installed, but Oxide could not be installed: {exception.Message}",
									Color.Red);
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
						_ = SendDiscordNotification(
							newServer,
							DiscordNotificationEvent.InstallCompleted,
							"INSTALL COMPLETED",
							$"{newServer.Game} is installed and ready for its first start.",
							Color.LimeGreen);
						RecordGameVerification(newServer.Game, GameVerificationKind.Install);
					}
					catch (Exception ex)
					{
						Log($"An unexpected error occurred during installation: {ex.Message}", Color.Red, true);
						_ = SendDiscordNotification(
							newServer,
							DiscordNotificationEvent.InstallFailed,
							"INSTALL FAILED",
							ex.Message,
							Color.Red);
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
			if (IsBackgroundServiceMode)
			{
				Log(
					$"[STEAM LOGIN] {server.ServerName} needs a Steam account name. Open Synix and start or update it manually once.",
					Color.Orange,
					true);
				return false;
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

		public async Task<bool> ExecuteStartSequence(GameServer server, string status = "")
		{
			ServerOperationKind operationKind = string.IsNullOrWhiteSpace(status)
				? ServerOperationKind.Start
				: ServerOperationKind.Restart;
			using ServerOperationLease operation =
				ServerOperationCoordinator.TryBegin(server, operationKind);
			if (!operation.Acquired)
			{
				Log($"[START BLOCKED] {operation.FailureReason}", Color.Orange, true);
				return false;
			}

			bool isRestart = status.Equals("RESTART", StringComparison.OrdinalIgnoreCase);
			bool isMaintenance = status.Equals("MAINTENANCE", StringComparison.OrdinalIgnoreCase);
			bool isWatchdog = status.Equals("WATCHDOG", StringComparison.OrdinalIgnoreCase);
			bool requiresVerifiedStop = RequiresVerifiedStopBeforeStartValidation(status);
			StartContext currentContext = isMaintenance
				? StartContext.Scheduled
				: isWatchdog
					? StartContext.CrashRecovery
					: StartContext.Manual;
			try
			{
				if (!PassResourceGuard(out string guardMsg))
				{
					Log(guardMsg, System.Drawing.Color.Red, true);
					if (!IsBackgroundServiceMode)
						MessageBox.Show(guardMsg, "System Resource Exhaustion",
							System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
					return false;
				}

				if (!await EnsureSteamAuthenticationAfterImport(server, status))
					return false;

				bool showInteractiveErrors = string.IsNullOrWhiteSpace(status) || isRestart;

				if (isRestart)
				{
					Log($"[SYNIX] Starting restart sequence for {server.ServerName}...", Color.Cyan);
				}
				else if (isMaintenance)
				{
					Log($"[🛠 MAINTENANCE] Scheduled restart sequence for {server.ServerName}.", Color.Cyan, true);
				}
				else if (isWatchdog)
				{
					server.Status = StatusManager.GetStatus(ServerState.Crashed);
					string reason = !server.RunningProcess?.Responding ?? false ? "FREEZE" : "CRASH/CLOSE";
					Log($"[🛡️ WATCHDOG] {reason} detected on {server.ServerName}. Initializing recovery...", Color.Orange);

					_ = SendDiscordNotification(
						server,
						DiscordNotificationEvent.ServerCrashed,
						"CRASH DETECTED",
						$"{server.ServerName} has terminated. Synix is attempting an automatic restart.",
						Color.Red);

					Core.Instance.UpdateGridStatus();
				}

				if (requiresVerifiedStop)
				{
					Log($"[SYNIX] Stopping the {server.ServerName} server and verifying its installed process is fully closed.", Color.Cyan, true);

					bool stopped = await StopServerAndReport(server, isManual: isRestart);
					if (!stopped)
					{
						Log(
							$"[RESTART BLOCKED] {server.ServerName} was not fully shut down, so Synix will not launch a second copy.",
							Color.Red,
							true);
						return false;
					}
				}

				if (!ValidateIntegrityAndReport(server, showInteractiveErrors)) return false;
				SafetyChecklistReport safetyReport = UserGuidance.BuildSafetyChecklist(server);
				if (!safetyReport.CanContinue)
				{
					SafetyCheckItem blocked = safetyReport.Items.First(item =>
						item.Level == SafetyCheckLevel.Blocked);
					Log($"[SAFETY CHECK BLOCKED] {blocked.Name}: {blocked.Details}", Color.Red, true);
					if (showInteractiveErrors)
					{
						PlainEnglishErrorDialog.ShowError(
							MainGUI.Instance,
							"start the server safely",
							blocked.Details);
					}
					return false;
				}
				Log(
					$"[SAFETY CHECK] Automatic checklist passed at {safetyReport.CompletionPercentage}% readiness.",
					Color.LimeGreen);
				if (GameFix.NeedsManagedConfiguration(server))
				{
					ConfigurationApplyResult configurationResult =
						await GameFix.ApplyManagedConfiguration(server);

					if (!configurationResult.Succeeded)
					{
						Log($"[CONFIG ERROR] {configurationResult.Message}", Color.Red, true);
						if (showInteractiveErrors)
							MessageBox.Show(
								configurationResult.Message,
								"Configuration Could Not Be Applied",
								MessageBoxButtons.OK,
								MessageBoxIcon.Error);
						return false;
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
				if (server.IsFirstBoot && !showInteractiveErrors)
				{
					Log($"[SETUP REQUIRED] {server.ServerName} must be started manually once to complete its first-start setup.", Color.Orange, true);
					return false;
				}
				if (ShouldBlockForConfig(server)) return false;
				if (!EnsureRequiredLaunchFilesAndReport(
					server,
					showDialog: showInteractiveErrors &&
						!displayedFirstBootWarning &&
						!status.Equals("WATCHDOG", StringComparison.OrdinalIgnoreCase)))
				{
					return false;
				}

				if (server.Status == StatusManager.GetStatus(ServerState.Stopped))
				{
					Log($"[SYNIX] Starting the {server.ServerName} server.", Color.Cyan, true);
					if (!PassSpamLock(server, out string lockMsg, "Start")) { Log(lockMsg, System.Drawing.Color.Orange); return false; }

					await Servers.Start(server, (message, color) => Log(message, color), currentContext);
				}
				else
				{
					if (server.Status != StatusManager.GetStatus(ServerState.Starting))
					{
						Log($"[🚨 CRITICAL] Restart failed: {server.ServerName} is still stuck!", Color.Red);
					}
				}
				return server.Status == StatusManager.GetStatus(ServerState.Starting) ||
					server.Status == StatusManager.GetStatus(ServerState.Running);
			}
			catch (Exception ex)
			{
				Log($"[🚨 CRITICAL ENGINE ERROR] Sequence failed for {server.ServerName}: {ex.Message}", Color.Red, true);
				if (currentContext == StartContext.Manual && MainGUI.Instance != null)
				{
					MainGUI.Instance.BeginInvoke((Action)(() =>
						PlainEnglishErrorDialog.ShowError(
							MainGUI.Instance,
							"complete the server action",
							ex.ToString())));
				}
				return false;
			}
		}

		internal static bool RequiresVerifiedStopBeforeStartValidation(string? status) =>
			status?.Equals("RESTART", StringComparison.OrdinalIgnoreCase) == true ||
			status?.Equals("MAINTENANCE", StringComparison.OrdinalIgnoreCase) == true ||
			status?.Equals("WATCHDOG", StringComparison.OrdinalIgnoreCase) == true;

		public void RunUniversalHealthCheck()
		{
			foreach (var server in ServerRegistry.Snapshot())
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
					"[🚨 ERROR] Synix could not unlock the saved credentials. Re-enter them in Server Settings before exporting a launch file.",
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

			if (!GameServerInputValidator.TryValidate(
					dbEntry,
					server.ServerName,
					batchPasswords,
					out string credentialError))
			{
				Log(
					$"[🚨 ERROR] Launch file export blocked: {credentialError}",
					Color.Red,
					true);
				MessageBox.Show(
					$"{credentialError}\n\nOpen Server Settings, enter the required credential, and save before exporting the batch file.",
					"Server Credentials Need Attention",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
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

				string fullExePath = GameLaunchCommandBuilder.ResolveExecutablePath(server, dbEntry);
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

		public async Task CleanFirewallRulesAsync(string executablePath)
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

				using Process? cleanup = Process.Start(psi);
				if (cleanup == null)
				{
					Log($"[FIREWALL] Windows could not start firewall cleanup for {executablePath}.", Color.Orange, true);
					return;
				}

				await cleanup.WaitForExitAsync();
				if (cleanup.ExitCode == 0)
					Log($"[FIREWALL] Successfully removed rules for {executablePath}", Color.LimeGreen);
				else
					Log($"[FIREWALL] Windows firewall cleanup exited with code {cleanup.ExitCode} for {executablePath}.", Color.Orange, true);
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
