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
using System.IO;
using System.Threading.Tasks;

namespace Synix_Control_Panel.SteamCMDHandler
{
	public static class ServerInstaller
	{
		public static int Install(string installPath, string appId, Action<string> logCallback, Action<int>? onPidStarted = null)
		{
			bool hasInternalError = false;

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

			try
			{
				process.Start();

				// 🛡️ Immediately report the PID back to the manager
				onPidStarted?.Invoke(process.Id);

				// 🛡️ BULLETPROOF ERROR CHECKING
				Action<string> checkForErrors = (msg) =>
				{
					// OrdinalIgnoreCase catches "No Subscription" even if it's "no subscription"
					bool isSubError = msg.Contains("subscription", StringComparison.OrdinalIgnoreCase);
					bool isAppError = msg.Contains("AppID not found", StringComparison.OrdinalIgnoreCase);

					if (msg.Contains("ERROR!") || isSubError || isAppError)
						hasInternalError = true;

					logCallback?.Invoke(msg);
				};

				// 🚀 REAL-TIME OUTPUT HANDLING
				Task outputTask = ReadStreamAsync(process.StandardOutput, checkForErrors);
				Task errorTask = ReadStreamAsync(process.StandardError, checkForErrors);

				process.WaitForExit();
				Task.WaitAll(outputTask, errorTask);

				return hasInternalError ? 99 : process.ExitCode;
			}
			catch (Exception ex)
			{
				logCallback?.Invoke($"[CRITICAL] Launcher Error: {ex.Message}");
				return -1;
			}
		}

		private static async Task ReadStreamAsync(StreamReader stream, Action<string> logCallback)
		{
			char[] buffer = new char[1];
			var lineBuilder = new System.Text.StringBuilder();
			string lastLoggedLine = "";

			while (await stream.ReadAsync(buffer, 0, 1) > 0)
			{
				char c = buffer[0];
				if (c == '\r' || c == '\n')
				{
					string line = lineBuilder.ToString().Trim();
					if (!string.IsNullOrWhiteSpace(line))
					{
						// 🛡️ THE "DUMP" FILTER: Skip logging if the line is exactly the same as the last
						// This prevents 50 identical lines with the same timestamp.
						if (line != lastLoggedLine)
						{
							logCallback(line);
							lastLoggedLine = line;
						}
					}
					lineBuilder.Clear();
				}
				else
				{
					lineBuilder.Append(c);
				}
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