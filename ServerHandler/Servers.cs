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

		public static void Start(GameServer server, Action<string> logCallback, string StatusMessage = "")
		{
			try
			{
				server.Status = StatusManager.GetStatus(ServerState.Starting);
				var dbEntry = GameDatabase.GetGame(server.Game);
				if (dbEntry == null) return;

				// 1. Setup paths
				string fullExePath = Path.Combine(server.InstallPath, dbEntry.ExeName);
				string binDir = Path.GetDirectoryName(fullExePath) ?? "";

				if (!File.Exists(fullExePath))
				{
					logCallback?.Invoke($"[ERROR] Executable missing: {fullExePath}");
					return;
				}

				// 🎯 2. THE ADVANCED SEARCH & PULL: Find steam_appid.txt ANYWHERE
				string targetId = (server.Game == "Soulmask") ? "2646460" : dbEntry.AppID;
				string appidPath = "";

				// Fast Check 1: The folder where the .exe lives (Unreal Engine standard)
				string binPath = Path.Combine(binDir, "steam_appid.txt");
				// Fast Check 2: The main server root folder (Unity / Source standard)
				string rootPath = Path.Combine(server.InstallPath, "steam_appid.txt");

				if (File.Exists(binPath))
				{
					appidPath = binPath;
				}
				else if (File.Exists(rootPath))
				{
					appidPath = rootPath;
				}
				else
				{
					// Deep Check 3: Search every single subfolder in the server installation
					try
					{
						string[] foundFiles = Directory.GetFiles(server.InstallPath, "steam_appid.txt", SearchOption.AllDirectories);

						if (foundFiles.Length > 0)
						{
							appidPath = foundFiles[0]; // Grab the exact path of the file it found
						}
						else
						{
							// If the file truly doesn't exist ANYWHERE yet, default to creating it next to the .exe
							appidPath = binPath;
						}
					}
					catch (Exception ex)
					{
						logCallback?.Invoke($"[WARNING] Deep search for AppID file failed: {ex.Message}");
						appidPath = binPath; // Safe fallback
					}
				}

				// 🛠️ 3. PHYSICAL FILE SELF-HEALING (Using the dynamically found path)
				try
				{
					if (!File.Exists(appidPath) || File.ReadAllText(appidPath).Trim() != targetId)
					{
						File.WriteAllText(appidPath, targetId);
						logCallback?.Invoke($"[ENGINE] Self-Healed steam_appid.txt at: {appidPath}");
					}
				}
				catch (Exception ex) { logCallback?.Invoke($"[WARNING] File access error: {ex.Message}"); }

				// 🛠️ 4. DYNAMIC ARGUMENT REPLACEMENT
				string cleanIdentity = server.ServerName.Replace(" ", "_");

				string args = dbEntry.RequiredArgs
					.Replace("{map}", server.WorldName)
					.Replace("{steamAppID}", targetId)
					.Replace("{appid}", targetId)
					.Replace("{port}", server.Port.ToString())
					.Replace("{query}", server.QueryPort.ToString())
					.Replace("{MaxPlayers}", server.MaxPlayers.ToString())
					.Replace("{pass}", server.Password ?? "")
					.Replace("{adminpass}", server.AdminPassword ?? "")
					.Replace("{ServerName}", server.ServerName)
					.Replace("{InstallPath}", server.InstallPath)
					.Replace("{Identity}", cleanIdentity)
					.Replace("{app_port}", (server.Port + 67).ToString())
					.Replace("{seed}", string.IsNullOrWhiteSpace(server.WorldSeed) ? "12345" : server.WorldSeed);

				// --- RCON and Mode Logic ---
				if (args.Contains("{rcon}"))
				{
					string formattedRcon = server.EnableRcon && !string.IsNullOrWhiteSpace(dbEntry.RconSyntax)
						? dbEntry.RconSyntax.Replace("{rcon_port}", server.RconPort.ToString()).Replace("{rcon_pass}", server.RconPassword ?? "")
						: "";
					args = args.Replace("{rcon}", formattedRcon);
				}

				if (args.Contains("{mode}") && !string.IsNullOrWhiteSpace(server.GameMode))
				{
					string translatedMode = server.GameMode;
					if (server.Game.Contains("ARK") || server.Game == "Atlas")
						translatedMode = (server.GameMode == "PVE") ? "True" : "False";

					args = args.Replace("{mode}", translatedMode);
				}

				args = args.Replace("  ", " ").Trim();

				// 🚀 5. CONFIGURE THE PROCESS
				ProcessStartInfo psi = new()
				{
					FileName = fullExePath,
					Arguments = args,
					WorkingDirectory = binDir,
					UseShellExecute = false, // 🎯 Required to force EnvironmentVariables
					CreateNoWindow = false
				};

				// 🚀 THE INVOKE: Force OS identity to match the command line
				psi.EnvironmentVariables["SteamAppId"] = targetId;
				psi.EnvironmentVariables["SteamGameId"] = targetId;

				logCallback?.Invoke($"[LAUNCHING] {server.Game} with identity: {targetId}");
				logCallback?.Invoke($"[COMMAND] {args}");

				// 🚀 6. THE SINGLE START
				Process? proc = Process.Start(psi);
				if (proc != null)
				{
					server.RunningProcess = proc;
					server.PID = proc.Id;
					proc.EnableRaisingEvents = true;
					proc.Exited += async (s, e) => {
						if (server.Status == StatusManager.GetStatus(ServerState.Running))
						{
							MainGUI.Instance?.AppendLog($"[CRASH] {server.ServerName} stopped unexpectedly!", System.Drawing.Color.Red);
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

		public static void Stop(GameServer server, Action<string> logCallback)
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

				logCallback?.Invoke($"[SHUTDOWN] Sending save signal to {server.ServerName}...");

				// --- 1. ATTEMPT GRACEFUL SHUTDOWN (Ctrl+C) ---
				if (AttachConsole((uint)targetPid))
				{
					// Prevent the Control Panel from closing itself when the signal is sent
					SetConsoleCtrlHandler(null, true);

					// Send the Ctrl+C signal
					GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0);

					// Wait for the server to save and exit (25s limit to allow the server to shutdown it's self)
					bool cleanExit = server.RunningProcess?.WaitForExit(25000) ?? false;

					// Cleanup
					FreeConsole();
					SetConsoleCtrlHandler(null, false);

					if (cleanExit)
					{
						logCallback?.Invoke($"[STOP] {server.ServerName} saved and closed cleanly.");
						FinalizeStoppedState(server);
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

				FinalizeStoppedState(server);
				logCallback?.Invoke($"[WATCHDOG] {server.ServerName} forced closed.");
			}
			catch (Exception ex)
			{
				logCallback?.Invoke($"[ERROR] Failed to stop {server.ServerName}: {ex.Message}");
			}
		}

		private static void FinalizeStoppedState(GameServer server)
		{
			server.Status = StatusManager.GetStatus(ServerState.Stopped);
			server.PID = null;
			server.RunningProcess = null;
		}
	}
}