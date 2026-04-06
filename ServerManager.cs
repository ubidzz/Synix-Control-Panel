using Game_Server_Control_Panel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;

public static class ServerManager
{
	private static string steamCmdDir = @"C:\Games\SteamCMD";
	private static string steamCmdExe = Path.Combine(steamCmdDir, "steamcmd.exe");
	private static string zipPath = Path.Combine(steamCmdDir, "steamcmd.zip");

	public static void EnsureSteamCMD(Action<string> logCallback)
	{
		string steamCmdDir = @"C:\Games\SteamCMD";
		string steamCmdExe = Path.Combine(steamCmdDir, "steamcmd.exe");
		string zipPath = Path.Combine(steamCmdDir, "steamcmd.zip");

		logCallback("Checking for SteamCMD...");

		try
		{
			// 1. Create directory if missing
			if (!Directory.Exists(steamCmdDir))
			{
				logCallback("Creating SteamCMD directory...");
				Directory.CreateDirectory(steamCmdDir);
			}

			// 2. Download and Unzip if EXE is missing
			if (!File.Exists(steamCmdExe))
			{
				logCallback("Downloading SteamCMD...");
				using (WebClient client = new WebClient())
				{
					client.DownloadFile("https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip", zipPath);
				}

				logCallback("Unzipping SteamCMD...");
				ZipFile.ExtractToDirectory(zipPath, steamCmdDir);
				if (File.Exists(zipPath)) File.Delete(zipPath);
			}

			// 3. The "Deep Clean" Initialization
			string publicFolder = Path.Combine(steamCmdDir, "public");
			if (!Directory.Exists(publicFolder))
			{
				logCallback("Starting first-run updates (this takes a few minutes)...");

				// Delete corrupt package folder if it exists from a failed run
				string packageFolder = Path.Combine(steamCmdDir, "package");
				if (Directory.Exists(packageFolder)) Directory.Delete(packageFolder, true);

				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					FileName = steamCmdExe,
					Arguments = "+quit",
					WorkingDirectory = steamCmdDir,
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardOutput = true, // REQUIRED for live logs
					RedirectStandardError = true   // REQUIRED to avoid the redirection error
				};

				using (Process proc = new Process { StartInfo = startInfo })
				{
					// These events allow live logging to your RichTextBox
					proc.OutputDataReceived += (s, ev) => { if (!string.IsNullOrEmpty(ev.Data)) logCallback(ev.Data); };
					proc.ErrorDataReceived += (s, ev) => { if (!string.IsNullOrEmpty(ev.Data)) logCallback("STEAMCMD ERROR: " + ev.Data); };

					proc.Start();

					// You can only call these if RedirectStandardOutput/Error are true
					proc.BeginOutputReadLine();
					proc.BeginErrorReadLine();

					proc.WaitForExit(300000);
				}
				logCallback("SteamCMD initialization complete.");
			}
			else
			{
				logCallback("SteamCMD is already initialized.");
			}
		}
		catch (Exception ex)
		{
			logCallback("CRITICAL ERROR: " + ex.Message);
		}
	}

	// The Master Folder Creation
	public static void CreateFolders(string path)
	{
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
	}

	// The Physical Rename Logic
	public static bool RenameServerFolder(GameServer oldServer, GameServer newServer)
	{
		// 1. GATEKEEPER: If they didn't use Default Location, DO NOT RENAME.
		if (!oldServer.IsDefaultPath)
		{
			return false; // Exit early; no physical folder movement
		}

		// 2. Only move if the path actually changed
		if (oldServer.InstallPath != newServer.InstallPath)
		{
			try
			{
				if (Directory.Exists(oldServer.InstallPath))
				{
					Directory.Move(oldServer.InstallPath, newServer.InstallPath);
					return true;
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Folder move failed: " + ex.Message);
			}
		}
		return false;
	}

	// The SteamCMD Engine
	public static void RunUpdate(string steamCmdPath, string installPath, string appId, Action<string> logCallback)
	{
		if (!File.Exists(steamCmdPath))
		{
			logCallback?.Invoke($"[ERROR] SteamCMD.exe not found at: {steamCmdPath}");
			return;
		}

		// 1. CLEAN THE PATH
		// If the path ends in a backslash (like C:\Games\Soulmask\), SteamCMD treats 
		// the last \" as an escape character and the whole command fails.
		string cleanPath = installPath.TrimEnd('\\', '/');

		// 2. THE EXACT BATCH ARGUMENTS
		// This matches your working .bat: +force_install_dir, +login, +app_update, validate, +quit
		string args = $"+force_install_dir \"{cleanPath}\" +login anonymous +app_update {appId} validate +quit";

		ProcessStartInfo psi = new ProcessStartInfo
		{
			FileName = steamCmdPath,
			Arguments = args,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true,
			// CRITICAL: Run from the SteamCMD folder so it uses its already-installed metadata
			WorkingDirectory = Path.GetDirectoryName(steamCmdPath)
		};

		Process process = new Process { StartInfo = psi };

		process.OutputDataReceived += (s, e) =>
		{
			if (!string.IsNullOrEmpty(e.Data)) logCallback?.Invoke(e.Data);
		};

		process.ErrorDataReceived += (s, e) =>
		{
			if (!string.IsNullOrEmpty(e.Data)) logCallback?.Invoke($"[SteamCMD Error] {e.Data}");
		};

		try
		{
			if (!Directory.Exists(cleanPath)) Directory.CreateDirectory(cleanPath);

			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			// THIS IS THE LINE THAT LOCKS THE 'X'
			// It must be exactly like this. No Task.Run wrapper.
			process.WaitForExit();

			logCallback?.Invoke("--- STEAMCMD PROCESS FINISHED ---");
		}
		catch (Exception ex)
		{
			logCallback?.Invoke($"[ERROR]: {ex.Message}");
		}
		finally { process.Dispose(); }
	}

	public static void StartServer(GameServer server, Action<string> logCallback)
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
			// Standardized tags: {map}, {appid}, {port}, {query}, {MaxPlayers}, {pass}, {adminpass}, {ServerName}
			string args = dbEntry.RequiredArgs
				.Replace("{map}", server.WorldName)
				.Replace("{appid}", dbEntry.AppID)
				.Replace("{port}", server.Port.ToString())
				.Replace("{query}", server.QueryPort.ToString())
				.Replace("{MaxPlayers}", server.MaxPlayers.ToString())
				.Replace("{pass}", server.Password ?? "")
				.Replace("{adminpass}", server.AdminPassword ?? "")
				.Replace("{ServerName}", server.ServerName);

			// 3. Append User's "ExtraArgs" from the JSON
			// We trim and add a space only if the user actually typed something
			if (!string.IsNullOrWhiteSpace(server.ExtraArgs))
			{
				args += " " + server.ExtraArgs.Trim();
			}

			// 4. Build the Full Executable Path
			// This combines the user's folder with the DB's relative EXE path
			string fullExePath = Path.Combine(server.InstallPath, dbEntry.ExeName);

			// Safety check: Does the file actually exist?
			if (!File.Exists(fullExePath))
			{
				logCallback?.Invoke($"[ERROR] Executable not found at: {fullExePath}");
				return;
			}

			// 5. Configure the Process Launch
			ProcessStartInfo psi = new ProcessStartInfo
			{
				FileName = fullExePath,
				Arguments = args,
				// Run the process FROM its own folder to prevent 'Missing DLL' errors
				WorkingDirectory = Path.GetDirectoryName(fullExePath),
				UseShellExecute = false,
				CreateNoWindow = false
			};

			logCallback?.Invoke($"[LAUNCHING] {server.Game}...");
			logCallback?.Invoke($"[COMMAND] {args}");

			// 6. Fire it up!
			Process proc = Process.Start(psi);
			if (proc != null)
			{
				server.RunningProcess = proc;
				server.Status = "Running";
				server.PID = proc.Id; // This allows MainGUI to track it

				// Immediately save the PID to JSON so the GUI remembers it if it crashes
				MainGUI.SaveServersToDisk();
			}
		}
		catch (Exception ex)
		{
			logCallback?.Invoke($"[CRITICAL ERROR] Failed to start server: {ex.Message}");
		}
	}

	// 1. We remove 'BindingList<GameServer> servers' because we use the Static MainGUI.serverList
	// 2. We remove 'Action<string> logCallback' because we use the Instance Bridge
	public static void CheckServerStatus()
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
				// 2. Recovery: Check by PID
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
					catch { isAlive = false; }
				}

				// 3. The "Crashed" or "Stopped" Handler
				if (!isAlive)
				{
					server.Status = "Stopped";
					server.PID = null;
					server.RunningProcess = null;

					// Use the Bridge to log to the MainGUI RichTextBox
					MainGUI.Instance?.AppendLog($"[MONITOR] {server.ServerName} has stopped or crashed.");

					// Save the "Stopped" status to the JSON immediately
					MainGUI.SaveServersToDisk();
				}
			}
		}
	}
}