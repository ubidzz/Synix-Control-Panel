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
using Synix_Control_Panel.SteamCMDHandler;
using Synix_Control_Panel.SynixEngine;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel.ServerHandler
{
	public static class Servers
	{
		#region Win32 API for Graceful Shutdown
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool AttachConsole(uint dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		static extern bool FreeConsole();

		[DllImport("kernel32.dll")]
		static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

		[DllImport("kernel32.dll")]
		static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate? HandlerRoutine, bool Add);
		delegate bool ConsoleCtrlDelegate(uint CtrlType);

		const uint CTRL_C_EVENT = 0;
		#endregion

		public static async Task Start(GameServer server, Action<string> logCallback, StartContext context = StartContext.Manual)
		{
			try
			{
				// 1. PRE-FLIGHT (Backup & Update)
				if (server.BackupOnStart && context != StartContext.CrashRecovery)
				{
					logCallback?.Invoke("[BACKUP] Starting...");
					await Task.Run(() => BackupManager.ExecuteBackup(server, context));
					logCallback?.Invoke("[BACKUP] Finished...");
				}

				if (server.UpdateOnStart)
				{
					logCallback?.Invoke($"[ACTION] Update on Start is ON. Pausing launch for update...");
					await Synix_Control_Panel.SynixEngine.Core.Instance.InstallOrUpdate(server);
				}

				// 2. TEMPLATE VALIDATION
				server.Status = StatusManager.GetStatus(ServerState.Starting);
				var dbEntry = GameDatabase.GetGame(server.Game);
				if (dbEntry == null)
				{
					logCallback?.Invoke("[ERROR] Game template not found.");
					return;
				}

				// 3. PATH SETUP
				string fullExePath = Path.Combine(server.InstallPath, dbEntry.ExeName);
				string binDir = Path.GetDirectoryName(fullExePath) ?? "";

				if (!File.Exists(fullExePath))
				{
					logCallback?.Invoke($"[ERROR] Executable missing: {fullExePath}");
					server.Status = StatusManager.GetStatus(ServerState.Stopped);
					return;
				}

				// 4. DYNAMIC IDENTITY & SEARCH
				string targetId = dbEntry.AppID;
				string invokedId = targetId;
				string appidPath = "";

				string binPath = Path.Combine(binDir, "steam_appid.txt");
				string rootPath = Path.Combine(server.InstallPath, "steam_appid.txt");

				if (File.Exists(binPath)) appidPath = binPath;
				else if (File.Exists(rootPath)) appidPath = rootPath;
				else
				{
					try
					{
						string[] foundFiles = Directory.GetFiles(server.InstallPath, "steam_appid.txt", SearchOption.AllDirectories);
						appidPath = foundFiles.Length > 0 ? foundFiles[0] : binPath;
					}
					catch { appidPath = binPath; }
				}

				// 🎯 THE INVOKE: Pull the ID from the file for {steamAppID}
				if (File.Exists(appidPath))
				{
					try
					{
						string fileContent = File.ReadAllText(appidPath).Trim();
						if (!string.IsNullOrWhiteSpace(fileContent))
						{
							invokedId = fileContent;
							logCallback?.Invoke($"[ENGINE] {server.ServerName} invoked {invokedId} from file.");
						}
					}
					catch (Exception ex) { logCallback?.Invoke($"[WARNING] File Read Error: {ex.Message}"); }
				}

				// 🛠️ 6. ARGUMENT REPLACEMENT
				string cleanIdentity = BackupManager.GetSafeName(server.ServerName);

				string args = dbEntry.RequiredArgs
					.Replace("{app_port}", server.AppPort?.ToString() ?? "0")
					.Replace("{seed}", string.IsNullOrWhiteSpace(server.WorldSeed) ? "12345" : server.WorldSeed)
					.Replace("{map}", server.WorldName)
					.Replace("{steamAppID}", invokedId) // 🎯 Uses the file content
					.Replace("{appid}", targetId)       // 🎯 Keeps the DB ID separate
					.Replace("{port}", server.Port.ToString())
					.Replace("{query}", server.QueryPort.ToString())
					.Replace("{MaxPlayers}", server.MaxPlayers.ToString())
					.Replace("{pass}", server.Password ?? "")
					.Replace("{adminpass}", server.AdminPassword ?? "")
					.Replace("{ServerName}", server.ServerName)
					.Replace("{InstallPath}", server.InstallPath)
					.Replace("{Identity}", cleanIdentity);

				// 🎯 RCON LOGIC RESTORED
				if (args.Contains("{rcon}"))
				{
					string formattedRcon = server.EnableRcon && !string.IsNullOrWhiteSpace(dbEntry.RconSyntax)
						? dbEntry.RconSyntax.Replace("{rcon_port}", server.RconPort.ToString()).Replace("{rcon_pass}", server.RconPassword ?? "")
						: "";
					args = args.Replace("{rcon}", formattedRcon);
				}

				// 🎯 GAME MODE TRANSLATION RESTORED
				if (args.Contains("{mode}") && !string.IsNullOrWhiteSpace(server.GameMode))
				{
					string translatedMode = (server.GameMode == "PVE" && (server.Game.Contains("ARK") || server.Game == "Atlas" || server.Game == "Rust"))
						? "True" : (server.GameMode == "PVP" && (server.Game.Contains("ARK") || server.Game == "Atlas" || server.Game == "Rust"))
						? "False" : server.GameMode;
					args = args.Replace("{mode}", translatedMode);
				}

				// 🚀 7. CONFIGURE PROCESS
				ProcessStartInfo psi = new()
				{
					FileName = fullExePath,
					Arguments = args.Replace("  ", " ").Trim(),
					WorkingDirectory = binDir,
					UseShellExecute = false,
					CreateNoWindow = false
				};

				// 🎯 MEMORY INJECTION
				psi.EnvironmentVariables["SteamAppId"] = invokedId;
				psi.EnvironmentVariables["SteamGameId"] = invokedId;

				logCallback?.Invoke($"[LAUNCHING] {server.Game} with identity: {invokedId}");

				// 🚀 8. EXECUTION & MONITORING
				Process? proc = Process.Start(psi);
				if (proc != null)
				{
					server.RunningProcess = proc;
					server.PID = proc.Id;
					server.Status = StatusManager.GetStatus(ServerState.Running);

					if (server.StartTime == null) server.StartTime = DateTime.Now;

					// 🎯 DISCORD ALERT: Server Online (Clean alert)
					_ = SynixEngine.Core.Instance.SendDiscordAlert(server, "SERVER STARTING", $"{server.ServerName} process has been initiated.", Color.Cyan);

					proc.EnableRaisingEvents = true;
					proc.Exited += async (s, e) => {
						if (server.Status == StatusManager.GetStatus(ServerState.Running))
						{
							// Keep local Red log for your Buffalo control panel
							MainGUI.Instance?.Invoke((Action)(() =>
								MainGUI.Instance.AppendLog($"[CRASH] {server.ServerName} stopped unexpectedly!", Color.Red)));

							// Watchdog handles the single Discord crash notification
							await Synix_Control_Panel.SynixEngine.Core.Instance.ExecuteRestartSequence(server);
						}
						else
						{
							server.Status = StatusManager.GetStatus(ServerState.Stopped);
							server.PID = null;
							server.RunningProcess = null;
							MainGUI.Instance?.Invoke((Action)(() => MainGUI.Instance.UpdateGrid()));
						}
					};
					FileHandler.SaveServers();
				}
			}
			catch (Exception ex) { logCallback?.Invoke($"[CRITICAL ERROR] {ex.Message}"); }
		}

		public static void Stop(GameServer server, Action<string> logCallback, bool isManual = true)
		{
			try
			{
				server.Status = StatusManager.GetStatus(ServerState.Stopping);
				MainGUI.Instance?.Invoke((Action)(() => MainGUI.Instance.UpdateGrid()));

				int targetPid = server.RunningProcess?.Id ?? server.PID ?? 0;
				if (targetPid <= 0)
				{
					logCallback?.Invoke($"[ERROR] {server.ServerName} has no valid PID to stop.");
					return;
				}

				// 🎯 DISCORD ALERT: Manual Shutdown
				if (isManual)
				{
					_ = SynixEngine.Core.Instance.SendDiscordAlert(server, "MANUAL SHUTDOWN",
						"A shutdown command was issued via the Control Panel.", Color.Orange);
				}

				logCallback?.Invoke($"[SHUTDOWN] Sending save signal to {server.ServerName}...");

				if (AttachConsole((uint)targetPid))
				{
					SetConsoleCtrlHandler(null, true);
					GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0);
					bool cleanExit = server.RunningProcess?.WaitForExit(25000) ?? false;
					FreeConsole();
					SetConsoleCtrlHandler(null, false);

					if (cleanExit)
					{
						logCallback?.Invoke($"[STOP] {server.ServerName} saved and closed cleanly.");
						FinalizeStoppedState(server);
						return;
					}
				}

				logCallback?.Invoke($"[WATCHDOG] {server.ServerName} did not respond. Forcing taskkill...");
				ProcessStartInfo killInfo = new ProcessStartInfo
				{
					FileName = "taskkill",
					Arguments = $"/F /T /PID {targetPid}",
					CreateNoWindow = true,
					UseShellExecute = false
				};

				using (Process? killProcess = Process.Start(killInfo)) { killProcess?.WaitForExit(); }
				FinalizeStoppedState(server);
				logCallback?.Invoke($"[WATCHDOG] {server.ServerName} forced closed.");
			}
			catch (Exception ex) { logCallback?.Invoke($"[ERROR] Failed to stop {server.ServerName}: {ex.Message}"); }
		}

		private static void FinalizeStoppedState(GameServer server)
		{
			server.Status = StatusManager.GetStatus(ServerState.Stopped);
			server.PID = null;
			server.RunningProcess = null;
		}
	}
}