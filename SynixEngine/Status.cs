// Copyright (c) 2026 ubidzz. All Rights Reserved.
//
// This file is part of Synix Control Panel.
//
// This code is provided for transparent viewing and personal use only.
// Unauthorized distribution, public modification, or commercial
// use of this source code or the compiled executable is strictly
// prohibited. Please refer to the LICENSE file in the root
// directory for full terms.
using Synix_Control_Panel.ServerHandler;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		private void UpdateGridStatus()
		{
			if (MainGUI.Instance != null && !MainGUI.Instance.IsDisposed && MainGUI.Instance.IsHandleCreated)
			{
				MainGUI.Instance.BeginInvoke((MethodInvoker)delegate {
					MainGUI.Instance.UpdateGrid();
				});
			}
		}

		public void RebindProcesses()
		{
			foreach (var server in MainGUI.serverList)
			{
				// --- 1. GAME SERVER REBIND ---
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
							process.Exited += async (s, e) => {
								if (server.Status == "Online")
									await RecoverServer(server);
								else
									CleanupOfflineState(server);
							};
						}
					}
					catch { CleanupOfflineState(server); }
				}

				// --- 2. STEAMCMD REBIND (Orphan Recovery) ---
				// This uses the SteamPID 49012 you saw in your JSON!
				if ((server.Status == "Installing" || server.Status == "Updating") && server.SteamPID.HasValue)
				{
					try
					{
						var installer = Process.GetProcessById(server.SteamPID.Value);
						if (installer != null && !installer.HasExited)
						{
							MainGUI.Instance?.AppendLog($"--- [REBIND] Found {server.Game} install still active (PID: {server.SteamPID}) ---", Color.BlueViolet, true);
						}
					}
					catch
					{
						// If process is GONE, it finished while Synix was closed
						server.Status = "Offline";
						server.SteamPID = null;

						// 🛠️ RUN SURGERY: Fix missing DLLs/Configs for the orphaned install
						GameFix.PostInstall(server);

						MainGUI.Instance?.AppendLog($"--- [RECOVERY] {server.Game} install finished while Synix was closed. Applied fixes. ---", Color.Green, true);
						FileHandler.SaveServers();
					}
				}
			}
			UpdateGridStatus();
		}

		private void CleanupOfflineState(GameServer server)
		{
			server.Status = "Offline";
			server.PID = null;
			server.RunningProcess = null;
			UpdateGridStatus();
		}
	}
}