using System;
using System.Diagnostics;
using System.IO;

// Change namespace to SteamCMD to avoid naming conflicts!
namespace Game_Server_Control_Panel.SteamCMD
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