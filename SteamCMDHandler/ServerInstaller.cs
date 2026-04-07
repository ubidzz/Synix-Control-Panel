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

// Change namespace to SteamCMD to avoid naming conflicts!
namespace Synix_Control_Panel.SteamCMDHandler
{
	public static class ServerInstaller
	{
		public static void Install(string steamCmdPath, string installPath, string appId, Action<string> logCallback)
		{
			if (!File.Exists(steamCmdPath))
			{
				logCallback?.Invoke($"[ERROR] SteamCMD not found: {steamCmdPath}");
				return;
			}

			string cleanPath = installPath.TrimEnd('\\', '/');
			string args = $"+force_install_dir \"{cleanPath}\" +login anonymous +app_update {appId} validate +quit";

			ProcessStartInfo psi = new()
			{
				FileName = steamCmdPath,
				Arguments = args,
				WorkingDirectory = Path.GetDirectoryName(steamCmdPath),
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			using Process process = new() { StartInfo = psi };
			process.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) logCallback?.Invoke(e.Data); };

			try
			{
				if (!Directory.Exists(cleanPath)) Directory.CreateDirectory(cleanPath);
				process.Start();
				process.BeginOutputReadLine();
				process.WaitForExit();
				logCallback?.Invoke("--- STEAMCMD PROCESS FINISHED ---");
			}
			catch (Exception ex)
			{
				logCallback?.Invoke($"[ERROR]: {ex.Message}");
			}
		}
	}
}