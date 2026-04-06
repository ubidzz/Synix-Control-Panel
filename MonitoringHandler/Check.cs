using Game_Server_Control_Panel.ServerHandler;
using System;
using System.Diagnostics;

namespace Game_Server_Control_Panel.MonitoringHandler
{
	public static class Check
	{
		public static void ServerStatus()
		{
			// Use the static list directly from MainGUI
			foreach (var server in MainGUI.serverList)
			{
				if (server.Status == "Running")
				{
					bool isAlive = false;

					// 1. Check by active Process object
					if (server.RunningProcess != null)
					{
						if (!server.RunningProcess.HasExited) isAlive = true;
					}
					// 2. Recovery: Check by PID if the app was restarted
					else if (server.PID.HasValue)
					{
						try
						{
							var existingProc = Process.GetProcessById(server.PID.Value);
							if (existingProc != null && !existingProc.HasExited)
							{
								server.RunningProcess = existingProc;
								isAlive = true;
							}
						}
						catch
						{
							isAlive = false; // Process not found
						}
					}

					// 3. The "Crashed" or "Stopped" Handler
					if (!isAlive)
					{
						server.Status = "Stopped";
						server.PID = null;
						server.RunningProcess = null;

						// Log to the MainGUI RichTextBox
						MainGUI.Instance?.AppendLog($"[MONITOR] {server.ServerName} has stopped or crashed.");

						// Save the "Stopped" status to the JSON immediately
						JsonManager.Save();
					}
				}
			}
		}
	}
}
