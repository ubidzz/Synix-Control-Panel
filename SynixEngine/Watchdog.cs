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

				if (server.Status == StatusManager.GetStatus(ServerState.Starting))
				{
					if (!server.PID.HasValue) continue;

					if (IsProcessAlive(server.PID.Value, exePathFromDB))
					{
						if (!server.HasAnnouncedOnline)
						{
							if (server.LastProbeTime == null || (DateTime.Now - server.LastProbeTime.Value).TotalSeconds >= 5)
							{
								server.LastProbeTime = DateTime.Now;
								_ = Task.Run(async () =>
								{
									string publicIp = await GetPublicIP();
									bool isResponding = await TestServerConnectivity(publicIp, server.QueryPort);
									if (isResponding)
									{
										MainGUI.Instance?.Invoke((Action)(() =>
										{
											// 🚀 CALLING DISCORD: SERVER ONLINE
											_ = SendDiscordAlert(server, "SERVER ONLINE",
												$"Successfully tested the server connectivity! If you still can't connect to the server please wait a minutes or two then try again.",
												Color.LimeGreen);

											server.Status = StatusManager.GetStatus(ServerState.Running);
											MainGUI.Instance.UpdateGrid();
										}));
									}
								});
							}
						}
					}
					else { _ = RecoverServer(server); }
					continue;
				}

				// --- 2. MONITOR STABLE SERVERS ---
				if (server.Status == StatusManager.GetStatus(ServerState.Running) && server.PID.HasValue)
				{
					if (!IsProcessAlive(server.PID.Value, exePathFromDB))
					{
						_ = RecoverServer(server);
					}
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

		public void InitializeAndRebind()
		{
			// Re-links processes if the app was restarted while servers were running
			RebindProcesses();
		}
	}
}