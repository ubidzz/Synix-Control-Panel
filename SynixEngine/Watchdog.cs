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
using System.Diagnostics;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using Synix_Control_Panel.ServerHandler;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		// 🛡️ AI MONITOR: This runs every 1 second via the Heartbeat
		private void PerformWatchdogCheck()
		{
			foreach (var server in MainGUI.serverList.ToList())
			{
				// 🛡️ ONLY MONITOR STABLE ONLINE SERVERS
				// If status is "Stopping" or "Offline", this loop skips them as requested.
				if (server.Status == "Online" && server.PID.HasValue)
				{
					// We pass the full path to ensure we aren't fooled by PID reuse
					if (!IsProcessAlive(server.PID.Value, server.InstallPath, server.ExeName))
					{
						HandleCrash(server);
					}
				}
			}
		}

		private bool IsProcessAlive(int pid, string installPath, string exeName)
		{
			try
			{
				// 1. Attempt to hook the process by its ID
				using var p = Process.GetProcessById(pid);

				// 2. Immediate check if it has already exited
				if (p.HasExited) return false;

				// 3. 🛡️ GHOST PID PROTECTION: Verify the file path
				// We check if the process path starts with our installation directory.
				// This ensures that if Windows reused the PID for another app, we catch it.
				string currentPath = p.MainModule.FileName;

				// Compare path to ensure it belongs to this server instance
				return currentPath.StartsWith(installPath, StringComparison.OrdinalIgnoreCase);
			}
			catch
			{
				// Catching "Process not found" or "Access denied" (which happens with System PIDs)
				return false;
			}
		}

		// 🚀 AI RECOVERY: Merged logic for crash reporting and auto-restart
		private void HandleCrash(GameServer server)
		{
			server.Status = "Crashed"; //
			server.PID = null;
			server.RunningProcess = null;

			// 1. Log the failure immediately using the AI Log helper
			Log($"[WATCHDOG] {server.ServerName} stopped unexpectedly!", Color.Red, true);

			// 2. Prepare for auto-restart
			Log($"[WATCHDOG] Attempting to restart {server.ServerName} in 2 seconds...", Color.Yellow);

			// 3. The Recovery Sequence
			// Small delay allows Windows to fully release any file locks from the crash
			Task.Delay(2000).ContinueWith(_ =>
			{
				MainGUI.Instance?.Invoke((Action)(() =>
				{
					// Verify the user didn't click "Stop" while we were waiting
					if (server.Status == "Crashed")
					{
						Log($"[WATCHDOG] Restarting {server.ServerName} now...", Color.Cyan);

						// Re-use your existing Start logic from the Servers handler
						Servers.Start(server, msg => Log(msg));
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