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
			// 1. PATH CORRECTION LOGIC
			// We combine the InstallPath with the ExeName. 
			// If the ExeName starts with the Game Folder (e.g. "Soulmask\Binaries..."), 
			// but the InstallPath already ends with it, we fix the "Double Path" bug.
			string fullPath = Path.Combine(server.InstallPath, server.ExeName);

			// If the path doesn't exist, we try a "Shallow Search" 
			// (Removing the game folder prefix from the ExeName)
			if (!File.Exists(fullPath))
			{
				string cleanExe = server.ExeName;
				if (cleanExe.StartsWith(server.Game + "\\", StringComparison.OrdinalIgnoreCase))
				{
					cleanExe = cleanExe.Substring(server.Game.Length + 1);
				}

				string altPath = Path.Combine(server.InstallPath, cleanExe);
				if (File.Exists(altPath))
				{
					fullPath = altPath; // Found it!
				}
			}

			// Final Check: If it still doesn't exist, kill the launch
			if (!File.Exists(fullPath))
			{
				logCallback?.Invoke($"[ERROR] Executable not found at: {fullPath}");
				return;
			}

			// 2. ARGUMENT INJECTION
			// Start with the RequiredArgs template (e.g. "-log -port={port}")
			string args = server.RequiredArgs
				.Replace("{name}", server.ServerName)
				.Replace("{port}", server.Port.ToString())
				.Replace("{query}", server.QueryPort.ToString())
				.Replace("{world}", server.WorldName)
				.Replace("{pass}", server.Password ?? "");

			// Append the user's custom ExtraArgs if they exist
			if (!string.IsNullOrWhiteSpace(server.ExtraArgs))
			{
				args += " " + server.ExtraArgs;
			}

			// 3. LAUNCH PROCESS
			ProcessStartInfo psi = new ProcessStartInfo
			{
				FileName = fullPath,
				Arguments = args,
				WorkingDirectory = Path.GetDirectoryName(fullPath),
				UseShellExecute = true, // Set to true to see the game console window
				CreateNoWindow = false
			};

			logCallback?.Invoke($"[LAUNCHING] {server.Game}: {server.ServerName}");
			logCallback?.Invoke($"[ARGS] {args}");

			Process proc = Process.Start(psi);

			if (proc != null)
			{
				// Update the server object with the running info
				server.RunningProcess = proc;
				server.Status = "Running";
				logCallback?.Invoke($"--- SERVER STARTED: {server.ServerName} (PID: {proc.Id}) ---");
			}
		}
		catch (Exception ex)
		{
			logCallback?.Invoke($"[CRITICAL ERROR] Failed to start server: {ex.Message}");
		}
	}

	public static void CheckServerStatus(BindingList<GameServer> servers, Action<string> logCallback)
	{
		foreach (var server in servers)
		{
			// We only care about servers that are supposed to be "Running"
			if (server.Status == "Running")
			{
				bool isAlive = false;

				// 1. Check by the active Process object (Best for current session)
				if (server.RunningProcess != null)
				{
					if (!server.RunningProcess.HasExited)
					{
						isAlive = true;
					}
				}
				// 2. Recovery: Check by PID if the Process object is null (GUI was restarted)
				else if (server.PID.HasValue)
				{
					try
					{
						// Try to re-hook the process from Windows
						var existingProc = Process.GetProcessById(server.PID.Value);
						if (existingProc != null && !existingProc.HasExited)
						{
							server.RunningProcess = existingProc;
							isAlive = true;
						}
					}
					catch
					{
						// Process ID no longer exists in Windows
						isAlive = false;
					}
				}

				// 3. Handle a Crash or Manual Close
				if (!isAlive)
				{
					server.Status = "Stopped";
					server.PID = null;
					server.RunningProcess = null;
					logCallback?.Invoke($"[MONITOR] {server.ServerName} has stopped.");

					// Call the exact name you have in MainGUI
					MainGUI.SaveServersToDisk(servers);
				}
			}
		}
	}
}