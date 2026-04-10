// Copyright (c) 2026 ubidzz. All Rights Reserved.
//
// This file is part of Synix Control Panel.
//
// This code is provided for transparent viewing and personal use only.
// Unauthorized distribution, public modification, or commercial
// use of this source code or the compiled executable is strictly
// prohibited. Please refer to the LICENSE file in the root
// directory for full terms.
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
				// If status is "Stopping" or "Offline", this loop skips them
				if (server.Status == "Online" && server.PID.HasValue)
				{
					if (!IsProcessAlive(server.PID.Value, server.ExeName))
					{
						HandleCrash(server);
					}
				}
			}
		}

		private bool IsProcessAlive(int pid, string dbExePath)
		{
			try
			{
				// 1. Attempt to hook the process by its ID
				using var p = Process.GetProcessById(pid);

				// 2. Immediate check if it has already exited
				if (p.HasExited) return false;

				// 3. Extract just the filename (e.g., "StarRuptureServerEOS-Win64-Shipping")
				string expectedName = System.IO.Path.GetFileNameWithoutExtension(dbExePath);

				// 4. Identity Match
				return p.ProcessName.Equals(expectedName, StringComparison.OrdinalIgnoreCase);
			}
			catch
			{
				// Catching "Process not found" or "Access denied"
				return false;
			}
		}

		// 🚀 AI RECOVERY: Merged logic for crash reporting and auto-restart
		private void HandleCrash(GameServer server)
		{
			server.Status = "Crashed";
			server.PID = null;
			server.RunningProcess = null;

			// 1. Log the failure immediately using the AI Log helper
			Log($"[WATCHDOG] {server.ServerName} stopped unexpectedly!", Color.Red, true);

			// 2. Prepare for auto-restart
			Log($"[WATCHDOG] Attempting to restart {server.ServerName} in 2 seconds...", Color.Yellow);

			// 3. The AI takes the wheel
			// We use a small delay so the system can finish cleaning up the old process
			Task.Delay(2000).ContinueWith(_ =>
			{
				MainGUI.Instance?.Invoke((Action)(() =>
				{
					Log($"[WATCHDOG] Restarting {server.ServerName} now...", Color.Cyan);

					// Re-use the existing Start logic from the Servers handler
					Servers.Start(server, msg => Log(msg));
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