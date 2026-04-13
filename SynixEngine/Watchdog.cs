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
		// ⏱️ INTERNAL GRACE PERIOD: Tracks countdown ticks for each PID
		private readonly Dictionary<int, int> _watchdogGracePeriods = new();

		// 🛡️ AI MONITOR: This runs every 1 second via the Heartbeat
		private void PerformWatchdogCheck()
		{
			foreach (var server in MainGUI.serverList.ToList())
			{
				// 🛡️ GHOST PROTECTION: Pull ExeName from Database
				var dbEntry = GameDatabase.GetGame(server.Game);
				string exePathFromDB = dbEntry?.ExeName ?? "";

				// --- 1. THE PROMOTION MECHANIC (Starting -> Running) ---
				if (server.Status == StatusManager.GetStatus(ServerState.Starting) && server.PID.HasValue)
				{
					// If it survives the identity check, promote it!
					if (IsProcessAlive(server.PID.Value, exePathFromDB))
					{
						server.Status = StatusManager.GetStatus(ServerState.Running);

						// Force the grid to redraw so it turns Green immediately
						MainGUI.Instance?.BeginInvoke((MethodInvoker)delegate { MainGUI.Instance.UpdateGrid(); });
					}
					continue; // Move to the next server for this tick
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
					// Clean up dictionary if the server is stopped or Stopped
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
			// 🛡️ LOG FIRST: Ensure we see the PID before it's cleared
			Log($"[WATCHDOG] {server.ServerName} stopped unexpectedly! PID: {server.PID}", Color.Red, true);

			server.Status = StatusManager.GetStatus(ServerState.Crashed);
			server.PID = null;
			server.RunningProcess = null;

			// 2. Prepare for auto-restart
			Log($"[WATCHDOG] Attempting to restart {server.ServerName} in 2 seconds...", Color.Yellow);

			// 3. The recovery sequence
			Task.Delay(2000).ContinueWith(_ =>
			{
				MainGUI.Instance?.Invoke((Action)(() =>
				{
					if (server.Status == StatusManager.GetStatus(ServerState.Crashed))
					{
						Log($"[WATCHDOG] Restarting {server.ServerName} now...", Color.Cyan);

						// 🎯 THE FIX: Only one call is needed, using the 'CrashRecovery' context
						// This tells the engine to skip the backup and keep that 1% CPU usage stable.
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