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
using Synix_Control_Panel.SynixEngine;
using System.Diagnostics;
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

		public static async Task Start(GameServer server, Action<string, Color> logCallback, StartContext context = StartContext.Manual)
		{
			// 🛡️ THE SAFEGUARD: Only check if it's a Manual start
			if (!IsSystemSafeToStart()) return;
			try
			{
				if (!Core.Instance.PassResourceGuard(out string guardMsg))
				{
					logCallback?.Invoke(guardMsg, Color.Orange);
					MessageBox.Show(guardMsg, "System Resource Exhaustion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				// 1. PRE-FLIGHT (Backup & Update)
				if (server.BackupOnStart && context != StartContext.CrashRecovery)
				{
					await Task.Run(() => Core.Instance.ExecuteBackup(server, context));
				}

				if (server.UpdateOnStart)
				{
					await Task.Run(() => Core.Instance.UpdateServerAndReport(server, "UPDATE", true));
				}

				// 2. TEMPLATE VALIDATION
				server.Status = StatusManager.GetStatus(ServerState.Starting);
				var dbEntry = GameDatabase.GetGame(server.Game);
				if (dbEntry == null)
				{
					logCallback?.Invoke("[🚨 ERROR] Game template not found.", Color.Red);
					return;
				}

				// 3. PATH SETUP
				string fullExePath = Path.Combine(server.InstallPath, dbEntry.ExeName);
				string binDir = Path.GetDirectoryName(fullExePath) ?? "";

				if (!File.Exists(fullExePath))
				{
					logCallback?.Invoke($"[🚨 ERROR] Executable missing: {fullExePath}", Color.Red);
					server.Status = StatusManager.GetStatus(ServerState.Stopped);
					return;
				}

				// 4. DYNAMIC IDENTITY & SEARCH
				string targetId = dbEntry.AppID;
				string invokedId = targetId;

				string appidPath = "";

				try
				{
					// This creates a "scanner" that looks through every single subfolder
					var scanner = Directory.EnumerateFiles(server.InstallPath, "steam_appid.txt", new EnumerationOptions
					{
						// Keep looking through every subfolder
						RecurseSubdirectories = true,

						// If it hits a folder it can't open (locked/protected), skip it and keep going
						IgnoreInaccessible = true,

						// Use the maximum possible depth (effectively unlimited)
						MaxRecursionDepth = int.MaxValue,

						// Skip things like symlinks to avoid getting stuck in a loop
						AttributesToSkip = FileAttributes.ReparsePoint
					});

					// Find the first one that exists
					appidPath = scanner.FirstOrDefault();
				}
				catch
				{
					// If something goes catastrophic, fallback to the root
					appidPath = Path.Combine(server.InstallPath, "steam_appid.txt");
				}

				// If it's still empty, it truly isn't in that install folder
				if (string.IsNullOrEmpty(appidPath))
				{
					appidPath = Path.Combine(server.InstallPath, "steam_appid.txt");
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
						}
					}
					catch (Exception ex) { logCallback?.Invoke($"[⚠️ WARNING] File Read Error: {ex.Message}", Color.OrangeRed); }
				}

				// 🛠️ 6. ARGUMENT REPLACEMENT
				string cleanIdentity = Core.Instance.GetSafeName(server.ServerName);

				string args = dbEntry.RequiredArgs
					.Replace("{app_port}", server.AppPort?.ToString() ?? "0")
					.Replace("{seed}", string.IsNullOrWhiteSpace(server.WorldSeed) ? "12345" : server.WorldSeed)
					.Replace("{map}", server.WorldName)
					.Replace("{steamAppID}", invokedId)
					.Replace("{appid}", targetId)
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

				if(!string.IsNullOrWhiteSpace(server.ExtraArgs))
				{
					args = args + " " + server.ExtraArgs;
				}

				args = args.Replace("  ", " ").Trim();

				// 🚀 7. CONFIGURE PROCESS
				ProcessStartInfo psi = new()
				{
					FileName = fullExePath,
					Arguments = args,
					WorkingDirectory = binDir,
					UseShellExecute = false,
					CreateNoWindow = false
				};

				// 🎯 MEMORY INJECTION
				psi.EnvironmentVariables["SteamAppId"] = invokedId;
				psi.EnvironmentVariables["SteamGameId"] = invokedId;

				logCallback?.Invoke($"[ARGUMENT] {args}", Color.Cyan);

				// 🚀 8. EXECUTION & MONITORING
				Process? proc = Process.Start(psi);
				if (proc != null)
				{
					server.RunningProcess = proc;
					server.PID = proc.Id;

					if (server.StartTime == null) server.StartTime = DateTime.Now;

					// 🎯 DISCORD ALERT: Server Online (Clean alert)
					_ = Core.Instance.SendDiscordAlert(server, "SERVER STARTING", $"{server.ServerName} process has been initiated.", Color.Cyan);

					proc.EnableRaisingEvents = true;
					proc.Exited += async (s, e) =>
					{
						if (server.Status == StatusManager.GetStatus(ServerState.Running))
						{
							// Watchdog handles the single Discord crash notification
							await Core.Instance.ExecuteStartSequence(server, "WATCHDOG");
						}
						else
						{
							FinalizeStoppedState(server);
						}
					};
					FileHandler.SaveServers();
				}
			}
			catch (Exception ex) { logCallback?.Invoke($"[🚨 CRITICAL ERROR] {ex.Message}", Color.Red); }
		}

		public static async Task Stop(GameServer server, Action<string, Color> logCallback, bool isManual = true)
		{
			try
			{
				server.Status = StatusManager.GetStatus(ServerState.Stopping);
				MainGUI.Instance?.Invoke((Action)(() => MainGUI.Instance.UpdateGrid()));

				int targetPid = server.RunningProcess?.Id ?? server.PID ?? 0;
				if (targetPid <= 0)
				{
					logCallback?.Invoke($"[🚨 ERROR] {server.ServerName} has no valid PID to stop.", Color.Red);
					return;
				}

				// 🎯 DISCORD ALERT: Manual Shutdown
				if (isManual)
				{
					_ = Core.Instance.SendDiscordAlert(server, "MANUAL SHUTDOWN",
						"A shutdown command was issued via the Synix Control Panel.", Color.Orange);
				}

				logCallback?.Invoke($"[SHUTDOWN] Sending save signal to {server.ServerName}...", Color.Aqua);

				if (AttachConsole((uint)targetPid))
				{
					SetConsoleCtrlHandler(null, true);
					GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0);

					bool cleanExit = await Task.Run(() => server.RunningProcess?.WaitForExit(25000) ?? false);

					FreeConsole();
					SetConsoleCtrlHandler(null, false);

					if (cleanExit)
					{
						logCallback?.Invoke($"[STOP] {server.ServerName} saved and closed cleanly.", Color.Lime);
						FinalizeStoppedState(server);
						return;
					}
				}

				logCallback?.Invoke($"[🛡️ WATCHDOG] {server.ServerName} did not respond. Forcing taskkill...", Color.Violet);
				ProcessStartInfo killInfo = new ProcessStartInfo
				{
					FileName = "taskkill",
					Arguments = $"/F /T /PID {targetPid}",
					CreateNoWindow = true,
					UseShellExecute = false
				};

				using (Process? killProcess = Process.Start(killInfo))
				{
					if (killProcess != null)
					{
						await Task.Run(() => killProcess.WaitForExit());
					}
				}

				FinalizeStoppedState(server);
				logCallback?.Invoke($"[🛡️ WATCHDOG] {server.ServerName} forced closed.", Color.Violet);
			}
			catch (Exception ex) { logCallback?.Invoke($"[🚨 ERROR] Failed to stop {server.ServerName}: {ex.Message}", Color.Red); }
		}

		private static void FinalizeStoppedState(GameServer server)
		{
			server.Status = StatusManager.GetStatus(ServerState.Stopped);
			server.PID = null;
			server.RunningProcess?.Dispose();
			server.RunningProcess = null;
			MainGUI.Instance?.Invoke((Action)(() => MainGUI.Instance.UpdateGrid()));
		}
	}
}
