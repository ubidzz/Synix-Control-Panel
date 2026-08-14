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
using static Synix_Control_Panel.SynixEngine.Core;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Synix_Control_Panel.SynixApp.ServerHandler
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
			try
			{
				bool isSystemSafe = await Task.Run(() => IsSystemSafeToStart());
				if (!isSystemSafe) return;

				if (!Core.Instance.PassResourceGuard(out string guardMsg))
				{
					logCallback?.Invoke(guardMsg, Color.Orange);
					MessageBox.Show(guardMsg, "System Resource Exhaustion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				if (server.BackupOnStart && context != StartContext.CrashRecovery)
				{
					await Task.Run(() => Core.Instance.ExecuteBackup(server, context));
				}

				if (server.UpdateOnStart)
				{
					await Task.Run(() => Core.Instance.UpdateServerAndReport(server, "UPDATE", true));
				}

				server.Status = StatusManager.GetStatus(ServerState.Starting);
				MainGUI.Instance?.Invoke((Action)(() => MainGUI.Instance.UpdateGrid()));

				ProcessStartInfo? psi = null;
				string finalArgs = "";

				await Task.Run(() =>
				{
					var dbEntry = GameDatabase.GetGame(server.Game);
					if (dbEntry == null)
					{
						logCallback?.Invoke("[🚨 ERROR] Game template not found.", Color.Red);
						return;
					}

					string fullExePath = Path.Combine(server.InstallPath, dbEntry.ExeName);
					string binDir = Path.GetDirectoryName(fullExePath) ?? "";

					if (!File.Exists(fullExePath))
					{
						logCallback?.Invoke($"[🚨 ERROR] Executable missing: {fullExePath}", Color.Red);
						MainGUI.Instance?.Invoke((Action)(() => server.Status = StatusManager.GetStatus(ServerState.Stopped)));
						return;
					}

					string targetId = dbEntry.AppID;
					string invokedId = targetId;

					string rootAppIdPath = Path.Combine(server.InstallPath, "steam_appid.txt");
					string binAppIdPath = Path.Combine(binDir, "steam_appid.txt");
					string appidPath = rootAppIdPath;

					if (File.Exists(rootAppIdPath))
					{
						appidPath = rootAppIdPath;
					}
					else if (File.Exists(binAppIdPath))
					{
						appidPath = binAppIdPath;
					}
					else
					{
						try
						{
							var scanner = Directory.EnumerateFiles(server.InstallPath, "steam_appid.txt", new EnumerationOptions
							{
								RecurseSubdirectories = true,
								IgnoreInaccessible = true,
								MaxRecursionDepth = 15,
								AttributesToSkip = FileAttributes.ReparsePoint
							});

							appidPath = scanner.FirstOrDefault() ?? rootAppIdPath;
						}
						catch
						{
							appidPath = rootAppIdPath;
						}
					}

					if (File.Exists(appidPath))
					{
						try
						{
							string fileContent = File.ReadAllText(appidPath).Trim();

							fileContent = fileContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim() ?? "";

							if (!string.IsNullOrWhiteSpace(fileContent))
							{
								invokedId = fileContent;
							}
						}
						catch (Exception ex) { logCallback?.Invoke($"[⚠️ WARNING] File Read Error: {ex.Message}", Color.OrangeRed); }
					}

					if (server.Game == "Minecraft Java")
					{
						int selectedGb = (int)server.MaxRam;
						server.MaxRam = selectedGb * 1024;
					}

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
						.Replace("{world_size}", server.WorldSize.ToString())
						.Replace("{Identity}", cleanIdentity)
					    .Replace("{ram}", server.MaxRam.ToString());

					if (args.Contains("{rcon}"))
					{
						string formattedRcon = "";

						if (server.EnableRcon && !string.IsNullOrWhiteSpace(dbEntry.RconSyntax))
						{
							formattedRcon = dbEntry.RconSyntax
								.Replace("{rcon_port}", server.RconPort.ToString())
								.Replace("{rcon_pass}", server.RconPassword ?? "");

							if (string.Equals(server.Game, "Rust", StringComparison.OrdinalIgnoreCase))
							{
								formattedRcon += " +rcon.web 1";
							}
						}

						args = args.Replace("{rcon}", formattedRcon);
					}

					if (args.Contains("{mode}") && !string.IsNullOrWhiteSpace(server.GameMode))
					{
						bool usesBooleanMode =
							server.Game.Equals("ARK: Survival Evolved", StringComparison.OrdinalIgnoreCase) ||
							server.Game.Equals("ARK: Survival Ascended", StringComparison.OrdinalIgnoreCase) ||
							server.Game.Equals("PixARK", StringComparison.OrdinalIgnoreCase) ||
							server.Game.Equals("Atlas", StringComparison.OrdinalIgnoreCase) ||
							server.Game.Equals("Rust", StringComparison.OrdinalIgnoreCase);

						string translatedMode = server.GameMode;

						if (usesBooleanMode)
						{
							if (server.GameMode.Equals("PVE", StringComparison.OrdinalIgnoreCase))
								translatedMode = "True";
							else if (server.GameMode.Equals("PVP", StringComparison.OrdinalIgnoreCase))
								translatedMode = "False";
						}

						args = args.Replace("{mode}", translatedMode);
					}

					if (!string.IsNullOrWhiteSpace(server.ExtraArgs))
					{
						if (!IsGameServerConfigSafe(server.ExtraArgs))
						{
							logCallback?.Invoke("[🚨 SECURITY] Illegal characters detected in the extra arguments. Aborting startup.", Color.Red);
							MainGUI.Instance?.Invoke((Action)(() => server.Status = StatusManager.GetStatus(ServerState.Stopped)));
							return;
						}

						args = $"{args} \"{server.ExtraArgs.Trim()}\"";
					}

					args = args.Replace("  ", " ").Trim();
					/*
					if (!IsStringSafe(args))
					{
						logCallback?.Invoke("[🚨 SECURITY] Illegal characters detected. Aborting startup.", Color.Red);
						MainGUI.Instance?.Invoke((Action)(() => server.Status = StatusManager.GetStatus(ServerState.Stopped)));
						return;
					}*/

					finalArgs = args;

					psi = new ProcessStartInfo
					{
						FileName = fullExePath,
						Arguments = finalArgs,
						WorkingDirectory = binDir,
						UseShellExecute = false,
						CreateNoWindow = false,
					};

					if (server.Game == "Dune: Awakening")
					{
						psi.UseShellExecute = true;
						psi.Verb = "runas";
					}
					else
					{
						psi.EnvironmentVariables["SteamAppId"] = invokedId;
						psi.EnvironmentVariables["SteamGameId"] = invokedId;
					}
				});

				if (psi == null) return;
				logCallback?.Invoke($"[ARGUMENT] {finalArgs}", Color.Cyan);

				Process? proc = Process.Start(psi);
				if (proc != null)
				{
					server.RunningProcess = proc;
					server.PID = proc.Id;

					server.StartTime = DateTime.Now;

					_ = Core.Instance.SendDiscordAlert(server, "SERVER STARTING", $"{server.ServerName} process has been initiated.", Color.Cyan);

					proc.EnableRaisingEvents = true;
					proc.Exited += async (s, e) =>
					{
						if (server.Status == StatusManager.GetStatus(ServerState.Running))
						{
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

					if (server.RunningProcess != null && server.RunningProcess.StartInfo.RedirectStandardInput)
					{
						try
						{
							// Instantly pipes 'Y' and hits Enter
							server.RunningProcess.StandardInput.WriteLine("Y");
							server.RunningProcess.StandardInput.Flush();
						}
						catch { } // Failsafe in case it closed faster than we could write to it
					}

					bool cleanExit = await Task.Run(() => server.RunningProcess?.WaitForExit(25000) ?? false);

					FreeConsole();
					SetConsoleCtrlHandler(null, false);

					if (cleanExit)
					{
						logCallback?.Invoke($"[SYNIX] {server.ServerName} saved and closed cleanly.", Color.Lime);
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
