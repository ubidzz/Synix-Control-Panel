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
		public static int Install(string installPath, string appId, Action<string> logCallback)
		{
			bool hasInternalError = false;

			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = @"C:\Games\SteamCMD\steamcmd.exe",
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

				// We wrap your logCallback to look for the word "ERROR!"
				Action<string> checkForErrors = (msg) =>
				{
					if (msg.Contains("ERROR!")) hasInternalError = true;
					logCallback?.Invoke(msg);
				};

				Task outputTask = ReadStreamAsync(process.StandardOutput, checkForErrors);
				Task errorTask = ReadStreamAsync(process.StandardError, checkForErrors);

				process.WaitForExit();
				Task.WaitAll(outputTask, errorTask);

				// If we saw "ERROR!" in the text, return 99, otherwise return the real exit code
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

		private static async Task ReadStreamAsync(StreamReader stream, Action<string> logCallback)
		{
			char[] buffer = new char[1];
			string currentLine = "";

			while (await stream.ReadAsync(buffer, 0, 1) > 0)
			{
				char c = buffer[0];
				if (c == '\r' || c == '\n')
				{
					if (!string.IsNullOrWhiteSpace(currentLine))
					{
						logCallback(currentLine);
						currentLine = "";
					}
				}
				else
				{
					currentLine += c;
				}
			}
			if (!string.IsNullOrWhiteSpace(currentLine)) logCallback(currentLine);
		}
	}
}