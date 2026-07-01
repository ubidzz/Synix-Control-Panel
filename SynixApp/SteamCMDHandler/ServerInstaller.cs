// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// 
// LEGAL NOTICE:
// This source code is proprietary and confidential. 
// 1. Permission is granted for PERSONAL, NON-COMMERCIAL use only.
// 2. You may modify this code for your own use, but you may NOT redistribute,
//    rebrand, or sell this code or derivative works without written consent.
// 3. The "Synix" brand and logic remain the property of Jason Turner.
// ============================================================================
using System.Diagnostics;

namespace Synix_Control_Panel.SynixApp.SteamCMDHandler
{
	public static class ServerInstaller
	{
		public static int Install(string installPath, string appId, Action<string> logCallback, Action<int>? onPidStarted = null)
		{
			bool hasInternalError = false;
			string lastLoggedLine = "";

			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = @"C:\Synix\SteamCMD\steamcmd.exe",
				Arguments = $"+force_install_dir \"{installPath}\" +login anonymous +app_update {appId} validate +quit",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true
			};

			using Process process = new Process { StartInfo = startInfo };

			// 🎯 NATIVE ASYNC EVENT HANDLER (Replaces the 1-character loop)
			DataReceivedEventHandler outputHandler = (sender, e) =>
			{
				string line = e.Data;
				if (!string.IsNullOrWhiteSpace(line))
				{
					// Check for errors
					if (line.Contains("ERROR!") ||
						line.Contains("subscription", StringComparison.OrdinalIgnoreCase) ||
						line.Contains("AppID not found", StringComparison.OrdinalIgnoreCase))
					{
						hasInternalError = true;
					}

					// SteamCMD "Dump" Filter: Prevents spamming the exact same line
					if (line != lastLoggedLine)
					{
						logCallback?.Invoke(line.Trim());
						lastLoggedLine = line;
					}
				}
			};

			// Wire up the events
			process.OutputDataReceived += outputHandler;
			process.ErrorDataReceived += outputHandler;

			try
			{
				process.Start();
				onPidStarted?.Invoke(process.Id);

				// Start the asynchronous reading pipeline
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();

				// Wait for SteamCMD to finish
				process.WaitForExit();

				return hasInternalError ? 99 : process.ExitCode;
			}
			catch (Exception ex)
			{
				logCallback?.Invoke($"[CRITICAL] Launcher Error: {ex.Message}");
				return -1;
			}
		}

		public static string GetSteamError(int code)
		{
			return code switch
			{
				0 => "Success",
				99 => "Steam Error: AppID not found or No Subscription.",
				5 => "Invalid Arguments",
				7 => "Disk Space Full",
				8 => "Network Connection Lost",
				_ => $"SteamCMD Failure (Code: {code})"
			};
		}
	}
}