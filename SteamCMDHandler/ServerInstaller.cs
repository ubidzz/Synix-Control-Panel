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
		public static void Install(string installPath, string appId, Action<string> logCallback)
		{
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
			process.Start();

			// Create asynchronous tasks to read the streams in real-time
			Task outputTask = ReadStreamAsync(process.StandardOutput, logCallback);
			Task errorTask = ReadStreamAsync(process.StandardError, logCallback);

			// Wait for both the process to exit AND the streams to finish reading
			process.WaitForExit();
			Task.WaitAll(outputTask, errorTask);
		}

		// A dedicated async reader that doesn't get hung up on buffer sizes
		private static async Task ReadStreamAsync(StreamReader stream, Action<string> logCallback)
		{
			char[] buffer = new char[1]; // Read literally one character at a time
			string currentLine = "";

			while (await stream.ReadAsync(buffer, 0, 1) > 0)
			{
				char c = buffer[0];

				// If we hit a newline or carriage return, flush the line to the UI!
				if (c == '\r' || c == '\n')
				{
					if (!string.IsNullOrWhiteSpace(currentLine))
					{
						logCallback(currentLine);
						currentLine = ""; // Reset for the next line
					}
				}
				else
				{
					currentLine += c;
				}
			}

			// Catch any leftover text that didn't end in a newline when the process closed
			if (!string.IsNullOrWhiteSpace(currentLine))
			{
				logCallback(currentLine);
			}
		}
	}
}