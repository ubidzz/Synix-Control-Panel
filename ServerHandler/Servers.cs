using System;
using System.Diagnostics;
using System.IO;

namespace Game_Server_Control_Panel.ServerHandler
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

					MainGUI.SaveServersToDisk();
				}
			}
			catch (Exception ex)
			{
				logCallback?.Invoke($"[CRITICAL ERROR] Failed to start server: {ex.Message}");
			}
		}
	}
}