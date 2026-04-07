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
using System;
using System.IO;
using System.Diagnostics;
using Synix_Control_Panel.FileFolderHandler;

namespace Synix_Control_Panel.ServerHandler
{
	public static class Servers
	{
		public static void Start(GameServer server, Action<string> logCallback)
		{
			try
			{
				// 1. Fetch the master template from your Database
				var dbEntry = GameDatabase.GetGame(server.Game);
				if (dbEntry == null)
				{
					logCallback?.Invoke($"[ERROR] Game definition for '{server.Game}' not found in Database.");
					return;
				}

				// 2. The Universal Replacement Map
				string args = dbEntry.RequiredArgs
					.Replace("{map}", server.WorldName)
					.Replace("{appid}", dbEntry.AppID)
					.Replace("{port}", server.Port.ToString())
					.Replace("{query}", server.QueryPort.ToString())
					.Replace("{MaxPlayers}", server.MaxPlayers.ToString())
					.Replace("{pass}", server.Password ?? "")
					.Replace("{adminpass}", server.AdminPassword ?? "")
					.Replace("{ServerName}", server.ServerName)
					.Replace("{InstallPath}", server.InstallPath);

				if (args.Contains("{mode}"))
				{
					// Make sure to add 'SelectedMode' to your GameServer class so the UI can save it!
					string mode = server.SelectedMode ?? "";

					if (server.Game == "Rust")
					{
						// Rust uses true/false for PVE
						if (mode == "PVE") args = args.Replace("{mode}", "true");
						else args = args.Replace("{mode}", "false"); // Default to PVP
					}
					else if (server.Game == "Killing Floor 2")
					{
						// KF2 uses class names
						if (mode == "Versus") args = args.Replace("{mode}", "Versus");
						else args = args.Replace("{mode}", "Survival"); // Default
					}
					else
					{
						// Most standard games use -pve, -pvp, or -creative
						if (mode == "Creative") args = args.Replace("{mode}", "-creative");
						else if (mode == "PVE") args = args.Replace("{mode}", "-pve");
						else if (mode == "PVP") args = args.Replace("{mode}", "-pvp");
						else args = args.Replace("{mode}", ""); // Blank fallback
					}
				}

				// 3. Append User's ExtraArgs
				if (!string.IsNullOrWhiteSpace(server.ExtraArgs))
				{
					args += " " + server.ExtraArgs.Trim();
				}

				// 4. Build the Full Executable Path
				string fullExePath = Path.Combine(server.InstallPath, dbEntry.ExeName);

				if (!File.Exists(fullExePath))
				{
					logCallback?.Invoke($"[ERROR] Executable not found at: {fullExePath}");
					return;
				}

				// 5. Configure the Process Launch
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

				// 6. Fire it up!
				Process? proc = Process.Start(psi);
				if (proc != null)
				{
					server.RunningProcess = proc;
					server.Status = "Online";
					server.PID = proc.Id;

					CreateFiles.SaveServers();
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
				logCallback?.Invoke($"Attempting to shut down {server.ServerName}...");

				// 1. Try to kill the active object
				if (server.RunningProcess != null && !server.RunningProcess.HasExited)
				{
					server.RunningProcess.Kill();
				}
				// 2. Recovery: Kill by PID from JSON
				else if (server.PID.HasValue)
				{
					try
					{
						Process oldProc = Process.GetProcessById(server.PID.Value);
                
						// Safety: Check if the process name matches your game exe (e.g., "Soulmask")
						// This prevents accidentally killing a different app if the PID was reused.
						if (oldProc.ProcessName.Contains(server.ExeName.Replace(".exe", ""), StringComparison.OrdinalIgnoreCase))
						{
							oldProc.Kill();
						}
					}
					catch (Exception pidEx)
					{
						logCallback?.Invoke($"[DEBUG] Could not reach process {server.PID}: {pidEx.Message}");
					}
				}

				logCallback?.Invoke($"[STOPPED] {server.ServerName} has been shut down.");
			}
			catch (Exception ex)
			{
				logCallback?.Invoke($"[ERROR] Stop failed: {ex.Message}");
			}
			finally
			{
				// 3. Cleanup data and save
				server.Status = "Offline";
				server.PID = null;
				server.RunningProcess = null;
				CreateFiles.SaveServers();
			}
		}
	}
}