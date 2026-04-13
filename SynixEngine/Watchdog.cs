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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Synix_Control_Panel.ServerHandler;
using Synix_Control_Panel.Database;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		private readonly Dictionary<int, int> _watchdogGracePeriods = new();

		private void PerformWatchdogCheck()
		{
			foreach (var server in MainGUI.serverList.ToList())
			{
				var dbEntry = GameDatabase.GetGame(server.Game);
				string exePathFromDB = dbEntry?.ExeName ?? "";

				// --- 1. THE PROMOTION MECHANIC (Starting -> Running) ---
				if (server.Status == StatusManager.GetStatus(ServerState.Starting) && server.PID.HasValue)
				{
					// Check if the process is physically alive first
					if (IsProcessAlive(server.PID.Value, exePathFromDB))
					{
						// 🎯 A2S PROBE INTEGRATION: Don't promote until the game responds to packets
						// We use a 5-second interval so we don't flood the server while it's loading
						if (server.LastProbeTime == null || (DateTime.Now - server.LastProbeTime.Value).TotalSeconds >= 5)
						{
							server.LastProbeTime = DateTime.Now;

							// Run the probe asynchronously to keep the Heartbeat loop fast
							_ = Task.Run(async () =>
							{
								// Prove the game is truly awake using your A2S logic
								bool isTrulyOnline = await TestServerConnectivity("127.0.0.1", server.QueryPort);

								if (isTrulyOnline)
								{
									MainGUI.Instance?.Invoke((Action)(() =>
									{
										// Final safety check: ensure status hasn't changed since probe started
										if (server.Status == StatusManager.GetStatus(ServerState.Starting))
										{
											server.Status = StatusManager.GetStatus(ServerState.Running);

											// 🚀 PUBLIC ALERT: Fire only when players can actually join!
											_ = SendDiscordAlert(server, "SERVER ONLINE",
												"The server has finished loading and is now accepting connections. Join now!",
												Color.LimeGreen);

											Log($"[ENGINE] {server.ServerName} passed A2S_INFO probe. Promotion complete.", Color.Lime);

											// Redraw the UI grid
											MainGUI.Instance.UpdateGrid();
										}
									}));
								}
							});
						}
					}
					continue;
				}

				// --- 2. MONITOR STABLE RUNNING SERVERS ---
				if (server.Status == StatusManager.GetStatus(ServerState.Running) && server.PID.HasValue)
				{
					int currentPid = server.PID.Value;

					if (!_watchdogGracePeriods.ContainsKey(currentPid))
					{
						_watchdogGracePeriods[currentPid] = 1;
					}

					if (_watchdogGracePeriods[currentPid] > 0)
					{
						_watchdogGracePeriods[currentPid]--;
						continue;
					}

					// 3. 2-Layer Identity Check: PID existence + Process Name match
					if (!IsProcessAlive(currentPid, exePathFromDB))
					{
						_watchdogGracePeriods.Remove(currentPid);
						HandleCrash(server);
					}
				}
				else if (server.PID.HasValue)
				{
					// Clean up dictionary if the server is stopped
					_watchdogGracePeriods.Remove(server.PID.Value);
				}
			}
		}

		private bool IsProcessAlive(int pid, string dbExePath)
		{
			try
			{
				// 1. Hook the process by its ID
				using var p = Process.GetProcessById(pid);

				// 2. Immediate check if it has already exited
				if (p.HasExited) return false;

				// 3. 🛡️ GHOST-PROOF IDENTITY MATCH
				string expectedName = Path.GetFileNameWithoutExtension(dbExePath);

				// 4. Return true only if the name in Windows matches the name in our Database
				return p.ProcessName.Equals(expectedName, StringComparison.OrdinalIgnoreCase);
			}
			catch
			{
				// Catching "Process not found" - PID is genuinely gone
				return false;
			}
		}

		// 🚀 AI RECOVERY: Merged logic for crash reporting and auto-restart
		private void HandleCrash(GameServer server)
		{
			// 🎯 1. LOG FIRST: Immediate local feedback
			Log($"[WATCHDOG] {server.ServerName} stopped unexpectedly! PID: {server.PID}", Color.Red, true);

			// 🎯 2. SEND DISCORD ALERT: This was missing!
			// We do this before clearing the PID so the alert has the right context
			_ = SendDiscordAlert(server, "CRASH DETECTED",
				$"The server process has unexpectedly terminated. Synix is initiating a 2-second recovery delay.",
				Color.Red);

			// 🎯 3. CLEAR STATE: Prepare for the fresh PID
			server.RunningProcess = null;

			Log($"[WATCHDOG] Attempting to restart {server.ServerName} in 2 seconds...", Color.Yellow);

			// 🎯 4. IMPROVED RECOVERY: Using async/await is safer for WinForms context than ContinueWith
			_ = Task.Run(async () =>
			{
				await Task.Delay(2000);

				MainGUI.Instance?.Invoke((Action)(() =>
				{
					// Only restart if the status hasn't been manually changed to 'Stopped' in those 2 seconds
					if (server.Status == StatusManager.GetStatus(ServerState.Crashed))
					{
						Log($"[WATCHDOG] Restarting {server.ServerName} now...", Color.Cyan);

						// Start with CrashRecovery context to skip backups and stay under 1% CPU usage
						Servers.Start(server, msg => Log(msg), StartContext.CrashRecovery);
					}
				}));
			});
		}

		public void InitializeAndRebind()
		{
			// Re-links processes if the app was restarted while servers were running
			RebindProcesses();
		}
	}
}