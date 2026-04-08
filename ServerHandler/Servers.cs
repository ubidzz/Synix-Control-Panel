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
using System.IO;

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

				// --- THE RCON INJECTOR ---
				if (args.Contains("{rcon}"))
				{
					if (server.EnableRcon && !string.IsNullOrWhiteSpace(dbEntry.RconSyntax))
					{
						// 1. Fill out the formatting rule with the user's specific port and password
						string formattedRcon = dbEntry.RconSyntax
							.Replace("{rcon_port}", server.RconPort.ToString())
							.Replace("{rcon_pass}", server.RconPassword ?? "");

						// 2. Inject it into the main launch string
						args = args.Replace("{rcon}", formattedRcon);
					}
					else
					{
						// The user has RCON turned OFF. Just delete the {rcon} tag entirely.
						args = args.Replace("{rcon}", "");
					}
				}
				// -------------------------

				// Clean up any double spaces
				args = args.Replace("  ", " ").Trim();

				if (args.Contains("{mode}") && !string.IsNullOrWhiteSpace(server.SelectedMode))
				{
					// Simply inject the exact word the user selected from the dropdown
					args = args.Replace("{mode}", server.SelectedMode);
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
				logCallback?.Invoke($"Attempting to shut down {server.ServerName}...");

				// 1. Get the "Clean" process name (e.g., "PalServer-Win64-Shipping")
				string targetName = Path.GetFileNameWithoutExtension(server.ExeName);

				// 2. Identify the PID to kill
				int? pidToKill = null;

				if (server.RunningProcess != null && !server.RunningProcess.HasExited)
				{
					pidToKill = server.RunningProcess.Id;
				}
				else if (server.PID.HasValue)
				{
					pidToKill = server.PID.Value;
				}

				if (pidToKill.HasValue)
				{
					try
					{
						Process proc = Process.GetProcessById(pidToKill.Value);

						// FIXED SAFETY CHECK: Compare only the filename, not the full path
						if (proc.ProcessName.Equals(targetName, StringComparison.OrdinalIgnoreCase))
						{
							// Use a helper to kill the process and all its children
							KillProcessTree(pidToKill.Value);
							logCallback?.Invoke($"[STOPPED] {server.ServerName} (PID {pidToKill}) has been shut down.");
						}
						else
						{
							logCallback?.Invoke($"[DEBUG] Safety block: Process {pidToKill} is '{proc.ProcessName}', not '{targetName}'.");
						}
					}
					catch (ArgumentException)
					{
						logCallback?.Invoke($"[DEBUG] Process {pidToKill} is already dead.");
					}
				}
			}
			catch (Exception ex)
			{
				logCallback?.Invoke($"[ERROR] Stop failed: {ex.Message}");
			}
			finally
			{
				server.Status = "Offline";
				server.PID = null;
				server.RunningProcess = null;
				FileHandler.SaveServers();
			}
		}

		// Add this helper method inside your Server class or a helper class
		private static void KillProcessTree(int pid)
		{
			if (pid == 0) return;

			try
			{
				// Find all child processes
				using (var searcher = new System.Management.ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid))
				using (var moc = searcher.Get())
				{
					foreach (var mo in moc)
					{
						KillProcessTree(Convert.ToInt32(mo["ProcessID"]));
					}
				}

				// Kill the actual process
				var proc = Process.GetProcessById(pid);
				proc.Kill();
			}
			catch (Exception) { /* Handle cases where process is already gone */ }
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
							process.Exited += (s, e) => {
								server.Status = "Offline";
								server.PID = null;
								server.RunningProcess = null;
								MainGUI.Instance?.UpdateGrid();
							};
						}
					}
					catch (Exception)
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