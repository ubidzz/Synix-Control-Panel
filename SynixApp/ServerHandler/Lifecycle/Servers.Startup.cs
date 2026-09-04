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
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using Synix_Control_Panel.SynixEngine;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel.SynixApp.ServerHandler
{
	public static partial class Servers
	{
		public static async Task Start(GameServer server, Action<string, Color> logCallback, StartContext context = StartContext.Manual)
		{
			ArgumentNullException.ThrowIfNull(logCallback);
			try
			{
				GameInfo? selectedDefinition = GameDatabase.GetGame(server.Game);
				if (selectedDefinition == null)
				{
					logCallback?.Invoke(
						$"[START ERROR] The built-in definition for '{server.Game}' could not be loaded.",
						Color.Red);
					_ = Core.Instance.SendDiscordNotification(
						server,
						DiscordNotificationEvent.ConfigurationWarning,
						"START CONFIGURATION ERROR",
						$"The built-in definition for {server.Game} could not be loaded.",
						Color.Red);
					return;
				}

				GamePrerequisiteReport prerequisites = await Task.Run(() =>
					GamePrerequisiteChecker.CheckCurrentSystem(
						selectedDefinition,
						server,
						port => Core.Instance.GetPortCollisionOwner(
							port,
							server,
							activeOnly: true),
						Core.Instance.IsPortInUseLocally));
				foreach (GamePrerequisiteItem warning in prerequisites.Items.Where(item =>
					item.State == GamePrerequisiteState.Warning))
				{
					logCallback?.Invoke($"[REQUIREMENT WARNING] {warning.Message}", Color.Orange);
				}
				if (!prerequisites.CanStart)
				{
					string message = prerequisites.ToDisplayText();
					logCallback?.Invoke($"[START BLOCKED] {message}", Color.Red);
					_ = Core.Instance.SendDiscordNotification(
						server,
						DiscordNotificationEvent.MonitoringWarning,
						"START BLOCKED",
						message,
						Color.Red);
					if (context == StartContext.Manual)
					{
						LocalizedMessageBox.Show(
							message,
							"Server Requirements Not Met",
							MessageBoxButtons.OK,
							MessageBoxIcon.Warning);
					}
					return;
				}

				SynixServerPasswords launchPasswords;
				try
				{
					launchPasswords = Core
						.RevealServerPasswords(server);
				}
				catch (SynixPasswordProtectionException)
				{
					logCallback?.Invoke(
						"[🚨 ERROR] Synix could not unlock this server's credentials. Open Server Settings, re-enter them, and save before starting the server.",
						Color.Red);
					return;
				}
				if (!GameServerInputValidator.TryValidate(
					selectedDefinition,
					server.ServerName,
					launchPasswords,
					out string serverInputError))
				{
					string message = $"{serverInputError} Open Server Settings, correct the credential, and save before starting.";
					logCallback?.Invoke($"[START BLOCKED] {message}", Color.Red);
					if (context == StartContext.Manual && !Core.IsBackgroundServiceMode)
					{
						LocalizedMessageBox.Show(
							message,
							"Server Settings Need Attention",
							MessageBoxButtons.OK,
							MessageBoxIcon.Warning);
					}
					return;
				}

				bool isSystemSafe = await Task.Run(() => IsSystemSafeToStart());
				if (!isSystemSafe) return;

				if (!Core.Instance.PassResourceGuard(out string guardMsg))
				{
					logCallback?.Invoke(guardMsg, Color.Orange);
					if (context == StartContext.Manual && !Core.IsBackgroundServiceMode)
						LocalizedMessageBox.Show(guardMsg, "System Resource Exhaustion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				bool maintenanceBackup = context == StartContext.Scheduled &&
					server.SmartMaintenanceEnabled &&
					server.MaintenanceBackupBeforeRestart;
				if ((server.BackupOnStart || maintenanceBackup) &&
					context != StartContext.CrashRecovery)
				{
					await Task.Run(() => Core.Instance.ExecuteBackup(server, context));
				}

				bool maintenanceUpdate = context == StartContext.Scheduled &&
					server.SmartMaintenanceEnabled &&
					server.MaintenanceUpdateBeforeRestart;
				if (server.UpdateOnStart || maintenanceUpdate)
				{
					await Task.Run(() => Core.Instance.UpdateServerAndReport(server, "UPDATE", true));
				}
				if (OxideRuntimeManager.IsEnabled(server, selectedDefinition) &&
					string.Equals(
						server.ServerFrameworkVersion,
						OxideRuntimeManager.FailedVersion,
						StringComparison.OrdinalIgnoreCase))
				{
					logCallback?.Invoke(
						"[OXIDE ERROR] Start blocked because Oxide was not installed successfully. Run Update or Validate to retry.",
						Color.Red);
					return;
				}
				if (OxideRuntimeManager.RequiresVanillaRestore(server, selectedDefinition))
				{
					logCallback?.Invoke(
						"[OXIDE] Start blocked because Rust was changed to Vanilla. Run Update or Validate to restore the official files first.",
						Color.Orange);
					return;
				}

				string launchPublicIp = string.Empty;
				if (selectedDefinition.RequiredArgs.Contains(
					"{PublicIP}",
					StringComparison.Ordinal))
				{
					launchPublicIp = (await Core.Instance.GetPublicIP()).Trim();
					if (string.IsNullOrWhiteSpace(launchPublicIp))
					{
						logCallback?.Invoke(
							"[PUBLIC LISTING] Synix could not detect the current public address. The server will use its own automatic address detection.",
							Color.Orange);
					}
				}

				server.HasAnnouncedOnline = false;
				server.IsProbing = false;
				server.LastProbeTime = null;
				server.Status = StatusManager.GetStatus(ServerState.Starting);
				Core.Instance.UpdateGridStatus();

				ProcessStartInfo? psi = null;
				string finalArgs = "";
				bool isMinecraft = false;
				bool hideWindow = selectedDefinition != null &&
					GameLaunchCommandBuilder.ShouldHideServerWindow(
						selectedDefinition,
						Properties.Settings.Default.ShowServerWindow);

				await Task.Run(() =>
				{
					var dbEntry = GameDatabase.GetGame(server.Game);
					if (dbEntry == null)
					{
						logCallback?.Invoke("[🚨 ERROR] Game template not found.", Color.Red);
						return;
					}

					string fullExePath = GameLaunchCommandBuilder.ResolveExecutablePath(server, dbEntry);
					string binDir = Path.GetDirectoryName(fullExePath) ?? "";

					if (!File.Exists(fullExePath))
					{
						logCallback?.Invoke($"[🚨 ERROR] Executable missing: {fullExePath}", Color.Red);
						_ = Core.Instance.SendDiscordNotification(
							server,
							DiscordNotificationEvent.ConfigurationWarning,
							"SERVER FILE MISSING",
							"The configured server launch file is missing. Run Update or Validate before starting.",
							Color.Red);
						server.Status = StatusManager.GetStatus(ServerState.Stopped);
						Core.Instance.UpdateGridStatus();
						return;
					}

					isMinecraft = GameCapabilityResolver.UsesMinecraftLifecycle(server);
					if (MinecraftControlProfile.IsJava(server))
					{
						PrepareMinecraftLauncher(fullExePath, logCallback!);
					}

					string invokedId = GameLaunchCommandBuilder.ResolveInvokedAppId(
						server,
						dbEntry,
						fullExePath);

					string cleanIdentity = Core.Instance.GetSafeName(server.ServerName);
					if (!GameLaunchCommandBuilder.TryBuildArguments(
						server,
						dbEntry,
						invokedId,
						launchPasswords,
						launchPublicIp,
						out string args,
						out string argumentError))
					{
						logCallback?.Invoke(
							$"[🚨 SECURITY] {argumentError} Startup was blocked.",
							Color.Red);
						_ = Core.Instance.SendDiscordNotification(
							server,
							DiscordNotificationEvent.SecurityWarning,
							"UNSAFE STARTUP BLOCKED",
							argumentError,
							Color.Red);
						server.Status = StatusManager.GetStatus(ServerState.Stopped);
						Core.Instance.UpdateGridStatus();
						return;
					}

					finalArgs = args;
					if (server.Game.Equals("Arma Reforger", StringComparison.OrdinalIgnoreCase))
					{
						string profilePath = Path.Combine(
							server.InstallPath,
							"profiles",
							cleanIdentity);
						Directory.CreateDirectory(profilePath);
						logCallback?.Invoke(
							$"[ARMA REFORGER] Profile and crash logs: {profilePath}",
							Color.Cyan);
					}

					psi = GameLaunchCommandBuilder.CreateProcessStartInfo(
						fullExePath,
						finalArgs,
						binDir,
						dbEntry.LaunchBehavior.RunElevated,
						hideWindow,
						isMinecraft && hideWindow,
						isMinecraft && hideWindow);

					if (!psi.UseShellExecute)
					{
						psi.EnvironmentVariables["SteamAppId"] = invokedId;
						psi.EnvironmentVariables["SteamGameId"] = invokedId;
					}
				});

				if (psi == null) return;

				string safeLogArgs = "[arguments unavailable]";
				if (selectedDefinition != null)
				{
					string fullExePath = GameLaunchCommandBuilder.ResolveExecutablePath(
						server,
						selectedDefinition);
					string invokedId = GameLaunchCommandBuilder.ResolveInvokedAppId(
						server,
						selectedDefinition,
						fullExePath);
					if (!GameLaunchCommandBuilder.TryBuildArguments(
						server,
						selectedDefinition,
						invokedId,
						GameLaunchCommandBuilder.CreateRedactedPasswords(launchPasswords),
						launchPublicIp,
						out safeLogArgs,
						out _))
					{
						safeLogArgs = "[arguments passed securely; preview unavailable]";
					}
					else if (!string.IsNullOrWhiteSpace(launchPublicIp))
					{
						safeLogArgs = safeLogArgs.Replace(
							launchPublicIp,
							"[PUBLIC IP]",
							StringComparison.Ordinal);
					}
				}

				logCallback?.Invoke($"[ARGUMENT] {safeLogArgs}", Color.Cyan);

				Process? proc = Process.Start(psi);
				if (proc != null)
				{
					if (isMinecraft && psi.RedirectStandardInput)
					{
						proc.StandardInput.AutoFlush = true;
					}
					if (isMinecraft && psi.RedirectStandardOutput)
					{
						MinecraftConsoleHub.Attach(server, proc);
					}

					server.RunningProcess = proc;
					server.PID = proc.Id;
					server.LastProcessDiscoveryUtc = DateTime.MinValue;
					RefreshServerProcessRegistry(server, forceDiscovery: true);
					_ = CaptureSpawnedServerProcesses(server, logCallback!);
					if (hideWindow && !psi.UseShellExecute)
					{
						_ = HideServerWindowAfterLaunch(proc);
					}

					server.StartTime = DateTime.Now;

					_ = Core.Instance.SendDiscordNotification(
						server,
						DiscordNotificationEvent.ServerStarting,
						"SERVER STARTING",
						$"{server.ServerName} process has been initiated.",
						Color.Cyan);

					proc.EnableRaisingEvents = true;
					proc.Exited += async (s, e) =>
					{
						try
						{
							if (IsStoppingStatus(server.Status))
							{
								return;
							}

							if (ReconcileActiveServerProcesses(server, forceDiscovery: true))
							{
								logCallback?.Invoke(
									$"[PROCESS TRACKING] The launcher exited, but {server.ServerProcesses.Count} verified server process(es) remain active: {FormatProcessRegistry(server.ServerProcesses)}",
									Color.Cyan);
								FileHandler.SaveServers();
								return;
							}

							if (server.Status == StatusManager.GetStatus(ServerState.Running))
							{
								await Core.Instance.ExecuteStartSequence(server, "WATCHDOG");
							}
							else
							{
								FinalizeStoppedState(server);
								await Core.Instance.SynchronizeFirstGeneratedConfiguration(server);
								FileHandler.SaveServers();
							}
						}
						catch (Exception ex)
						{
							logCallback?.Invoke($"[🚨 CRASH HANDLER ERROR] {ex.Message}", Color.Red);
							FinalizeStoppedState(server);
						}
					};
					FileHandler.SaveServers();
				}
			}
			catch (Exception ex)
			{
				logCallback?.Invoke($"[🚨 CRITICAL ERROR] {ex.Message}", Color.Red);
				_ = Core.Instance.SendDiscordNotification(
					server,
					DiscordNotificationEvent.MonitoringWarning,
					"START FAILED",
					ex.Message,
					Color.Red);
			}
		}

		private static void PrepareMinecraftLauncher(string launcherPath, Action<string, Color> logCallback)
		{
			try
			{
				string original = File.ReadAllText(launcherPath);
				string updated = original
					.Replace(" %* <NUL", " %*", StringComparison.OrdinalIgnoreCase)
					.Replace("if %errorlevel% neq 0 pause", "exit /b %errorlevel%", StringComparison.OrdinalIgnoreCase);

				if (!string.Equals(original, updated, StringComparison.Ordinal))
				{
					File.WriteAllText(launcherPath, updated);
					logCallback?.Invoke("[MINECRAFT] Updated the legacy launcher so clean console shutdown commands are accepted.", Color.Cyan);
				}
			}
			catch (Exception ex)
			{

				logCallback?.Invoke($"[⚠️ MINECRAFT] Could not update Start.bat for graceful shutdown: {ex.Message}", Color.OrangeRed);
			}
		}


		private static Task HideServerWindowAfterLaunch(Process process)
		{
			return Task.Run(async () =>
			{
				DateTime? firstHiddenAt = null;
				for (int attempt = 0; attempt < 60; attempt++)
				{
					try
					{
						if (Properties.Settings.Default.ShowServerWindow || process.HasExited)
							return;

						if (await TryHideServerWindow(process).ConfigureAwait(false))
						{
							firstHiddenAt ??= DateTime.UtcNow;
						}

						if (firstHiddenAt.HasValue &&
							DateTime.UtcNow - firstHiddenAt.Value >= TimeSpan.FromSeconds(3))
						{
							return;
						}
					}
					catch
					{
						return;
					}

					await Task.Delay(250).ConfigureAwait(false);
				}
			});
		}

		private static async Task<bool> TryHideServerWindow(Process process)
		{
			bool hidden = false;
			try
			{
				process.Refresh();
				IntPtr mainWindow = process.MainWindowHandle;
				if (mainWindow != IntPtr.Zero)
				{
					ShowWindowAsync(mainWindow, SW_HIDE);
					hidden = true;
				}
			}
			catch (Exception suppressedException)
			{
				Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException);
			}

			await _consoleLock.WaitAsync().ConfigureAwait(false);
			bool attached = false;
			try
			{
				attached = AttachConsole((uint)process.Id);
				if (!attached)
					return hidden;

				IntPtr consoleWindow = GetConsoleWindow();
				if (consoleWindow == IntPtr.Zero)
					return hidden;

				ShowWindowAsync(consoleWindow, SW_HIDE);
				return true;
			}
			finally
			{
				if (attached)
					FreeConsole();
				_consoleLock.Release();
			}
		}
	}
}
