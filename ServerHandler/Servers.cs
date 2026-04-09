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
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

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

		public static void Start(GameServer server, Action<string> logCallback)
		{
			try
			{
				var dbEntry = GameDatabase.GetGame(server.Game);
				if (dbEntry == null)
				{
					logCallback?.Invoke($"[ERROR] Game definition for '{server.Game}' not found in Database.");
					return;
				}

				// 🛠️ CREATE THE IDENTITY (No Spaces)
				// This replaces " " with "_" for folder-safe paths.
				string cleanIdentity = server.ServerName.Replace(" ", "_");

				string args = dbEntry.RequiredArgs
					.Replace("{map}", server.WorldName)
					.Replace("{appid}", dbEntry.AppID)
					.Replace("{port}", server.Port.ToString())
					.Replace("{query}", server.QueryPort.ToString())
					.Replace("{MaxPlayers}", server.MaxPlayers.ToString())
					.Replace("{pass}", server.Password ?? "")
					.Replace("{adminpass}", server.AdminPassword ?? "")
					.Replace("{ServerName}", server.ServerName) // Original name for Hostnames
					.Replace("{InstallPath}", server.InstallPath)
					.Replace("{Identity}", cleanIdentity); // 🎯 FIXED: Was incorrectly using InstallPath

				if (args.Contains("{rcon}"))
				{
					if (server.EnableRcon && !string.IsNullOrWhiteSpace(dbEntry.RconSyntax))
					{
						string formattedRcon = dbEntry.RconSyntax
							.Replace("{rcon_port}", server.RconPort.ToString())
							.Replace("{rcon_pass}", server.RconPassword ?? "");
						args = args.Replace("{rcon}", formattedRcon);
					}
					else args = args.Replace("{rcon}", "");
				}

				if (args.Contains("{mode}") && !string.IsNullOrWhiteSpace(server.GameMode))
				{
					string translatedMode = server.GameMode;

					// 🦖 ARK & ATLAS TRANSLATOR
					// These games use ?ServerPVE=True/False, so we translate the UI text here.
					if (server.Game == "ARK: Survival Evolved" || server.Game == "ARK: Survival Ascended" || server.Game == "Atlas")
					{
						translatedMode = (server.GameMode == "PVE") ? "True" : "False";
					}

					// Now perform the actual replacement in the command line
					args = args.Replace("{mode}", server.GameMode);
				}

				args = args.Replace("  ", " ").Trim();

				string fullExePath = Path.Combine(server.InstallPath, dbEntry.ExeName);

				if (!File.Exists(fullExePath))
				{
					logCallback?.Invoke($"[ERROR] Executable not found at: {fullExePath}");
					return;
				}

				ProcessStartInfo psi = new()
				{
					FileName = fullExePath,
					Arguments = args,
					WorkingDirectory = Path.GetDirectoryName(fullExePath),
					UseShellExecute = false,
					CreateNoWindow = false
				};

				logCallback?.Invoke($"[LAUNCHING] {server.Game}...");
				logCallback?.Invoke($"[COMMAND] {args}");

				Process? proc = Process.Start(psi);
				if (proc != null)
				{
					server.RunningProcess = proc;
					server.Status = "Online";
					server.PID = proc.Id;

					// 🛡️ WATCHDOG INTEGRATION
					// We subscribe here so new servers also benefit from auto-restart
					proc.EnableRaisingEvents = true;
					proc.Exited += async (s, e) => {
						if (server.Status == "Online")
						{
							MainGUI.Instance?.AppendLog($"[CRASH] {server.ServerName} stopped unexpectedly! Restarting...", System.Drawing.Color.Red);

							// Trigger the master recovery logic in your Core
							await Synix_Control_Panel.SynixEngine.Core.Instance.ExecuteRestartSequence(server);
						}
						else
						{
							server.Status = "Offline";
							server.PID = null;
							server.RunningProcess = null;
							MainGUI.Instance?.Invoke((Action)(() => MainGUI.Instance.UpdateGrid()));
						}
					};

					FileHandler.SaveServers();
				}
			}
			catch (Exception ex)
			{
				logCallback?.Invoke($"[CRITICAL ERROR] Failed to start server: {ex.Message}");
			}
		}

		public static void Stop(GameServer server, Action<string> logCallback)
		{
			try
			{
				int targetPid = server.RunningProcess?.Id ?? server.PID ?? 0;

				if (targetPid <= 0)
				{
					logCallback?.Invoke($"[ERROR] {server.ServerName} has no valid PID to stop.");
					return;
				}

				logCallback?.Invoke($"[SHUTDOWN] Sending save signal to {server.ServerName}...");

				// --- 1. ATTEMPT GRACEFUL SHUTDOWN (Ctrl+C) ---
				if (AttachConsole((uint)targetPid))
				{
					// Prevent the Control Panel from closing itself when the signal is sent
					SetConsoleCtrlHandler(null, true);

					// Send the Ctrl+C signal
					GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0);

					// Wait for the server to save and exit (15s limit)
					bool cleanExit = server.RunningProcess?.WaitForExit(20000) ?? false;

					// Cleanup
					FreeConsole();
					SetConsoleCtrlHandler(null, false);

					if (cleanExit)
					{
						logCallback?.Invoke($"[STOP] {server.ServerName} saved and closed cleanly.");
						FinalizeOfflineState(server);
						return;
					}
				}

				// --- 2. HARD KILL FALLBACK (Taskkill) ---
				logCallback?.Invoke($"[WATCHDOG] {server.ServerName} did not respond. Forcing taskkill...");

				ProcessStartInfo killInfo = new ProcessStartInfo
				{
					FileName = "taskkill",
					Arguments = $"/F /T /PID {targetPid}",
					CreateNoWindow = true,
					UseShellExecute = false
				};

				using (Process? killProcess = Process.Start(killInfo))
				{
					killProcess?.WaitForExit();
				}

				FinalizeOfflineState(server);
				logCallback?.Invoke($"[WATCHDOG] {server.ServerName} forced closed.");
			}
			catch (Exception ex)
			{
				logCallback?.Invoke($"[ERROR] Failed to stop {server.ServerName}: {ex.Message}");
			}
		}

		private static void FinalizeOfflineState(GameServer server)
		{
			server.Status = "Offline";
			server.PID = null;
			server.RunningProcess = null;
		}

		public static void RebindProcesses(BindingList<GameServer> servers)
		{
			if (servers.Count == 0) return;

			foreach (var server in servers)
			{
				if (server.PID.HasValue && server.PID.Value > 0)
				{
					try
					{
						var process = Process.GetProcessById(server.PID.Value);

						if (process != null && !process.HasExited)
						{
							server.RunningProcess = process;
							server.Status = "Online";

							MainGUI.Instance?.AppendLog($"--- [REBIND] Found {server.Game} still running (PID: {server.PID}) ---", Color.BlueViolet, true);

							process.EnableRaisingEvents = true;

							// 🛡️ THE WATCHDOG: This triggers if the process closes
							process.Exited += async (s, e) => {
								// If we didn't plan to stop it, it's a failure
								if (server.Status == "Online")
								{
									await SynixEngine.Core.Instance.RecoverServer(server);
								}
								else
								{
									server.Status = "Offline";
									server.PID = null;
									server.RunningProcess = null;
									MainGUI.Instance?.Invoke((Action)(() => MainGUI.Instance.UpdateGrid()));
								}
							};
						}
					}
					catch
					{
						// The process isn't in Task Manager anymore
						server.Status = "Offline";
						server.PID = null;
						MainGUI.Instance?.AppendLog($"--- [REBIND] {server.ServerName} process not found in Windows. ---", Color.Gray);
					}
				}
			}
		}
	}
}