using System;
using System.Diagnostics;
using System.IO;

namespace Game_Server_Control_Panel.SteamCMD
{
	public static class ServerInstaller
	{
		public static void Install(string steamCmdPath, string installPath, string appId, Action<string> logCallback)
		{
			if (!File.Exists(steamCmdPath))
			{
				logCallback?.Invoke($"[ERROR] SteamCMD.exe not found at: {steamCmdPath}");
				return;
			}

			string cleanPath = installPath.TrimEnd('\\', '/');
			string args = $"+force_install_dir \"{cleanPath}\" +login anonymous +app_update {appId} validate +quit";

			ProcessStartInfo psi = new()
			{
				FileName = steamCmdPath,
				Arguments = args,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true,
				WorkingDirectory = Path.GetDirectoryName(steamCmdPath)
			};

			Process process = new() { StartInfo = psi };

			process.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) logCallback?.Invoke(e.Data); };
			process.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) logCallback?.Invoke($"[SteamCMD Error] {e.Data}"); };

			try
			{
				if (!Directory.Exists(cleanPath)) Directory.CreateDirectory(cleanPath);

				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				process.WaitForExit();

				logCallback?.Invoke("--- STEAMCMD PROCESS FINISHED ---");
			}
			catch (Exception ex)
			{
				logCallback?.Invoke($"[ERROR]: {ex.Message}");
			}
			finally
			{
				process.Dispose();
			}
		}
	}
}