using Game_Server_Control_Panel.FileFolderHandler;
using Game_Server_Control_Panel;
using System;
using System.Diagnostics;
using System.IO;

namespace Game_Server_Control_Panel.ServerHandler
{
	public static class Servers
	{
		public static void Start(Game_Server_Control_Panel.GameServer server, Action<string> logCallback)
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
					server.Status = "Running";
					server.PID = proc.Id;

					JsonManager.Save();
				}
			}
			catch (Exception ex)
			{
				logCallback?.Invoke($"[CRITICAL ERROR] Failed to start server: {ex.Message}");
			}
		}

		public static void Stop(Game_Server_Control_Panel.GameServer server, Action<string> logCallback)
		{
			try
			{
				logCallback?.Invoke($"Stopping {server.Game}: {server.ServerName}...");

				// 1. Try to kill by the active process object
				if (server.RunningProcess != null && !server.RunningProcess.HasExited)
				{
					server.RunningProcess.Kill();
				}
				// 2. Recovery: Try to kill by PID (if the app was restarted)
				else if (server.PID.HasValue)
				{
					try
					{
						Process oldProc = Process.GetProcessById(server.PID.Value);
						if (!oldProc.HasExited)
						{
							oldProc.Kill();
						}
					}
					catch { /* Process already gone */ }
				}

				logCallback?.Invoke($"[STOPPED] {server.ServerName} has been shut down.");
			}
			catch (Exception ex)
			{
				logCallback?.Invoke($"[ERROR] Stop failed: {ex.Message}");
			}
			finally
			{
				// 3. Always clean up the server data regardless of errors
				server.Status = "Stopped";
				server.PID = null;
				server.RunningProcess = null;

				// 4. Save the status change to disk immediately
				JsonManager.Save();
			}
		}
	}
}