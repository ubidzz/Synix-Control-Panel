using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace Game_Server_Control_Panel.SteamCMDHandler
{
	public static class SteamCMD
	{
		// 1. Centralized SteamCMD Paths
		public static readonly string SteamCmdDir = @"C:\Games\SteamCMD";
		public static readonly string SteamCmdExe = Path.Combine(SteamCmdDir, "steamcmd.exe");
		private static readonly string ZipPath = Path.Combine(SteamCmdDir, "steamcmd.zip");

		// 2. The Download and Setup Engine
		public static async Task EnsureSteamCMD(Action<string> logCallback)
		{
			try
			{
				// 1. Create the Directory if it's missing
				if (!Directory.Exists(SteamCmdDir))
				{
					logCallback?.Invoke("[INIT] Creating SteamCMD directory...");
					Directory.CreateDirectory(SteamCmdDir);
				}

				// 2. Download and Extract if the EXE is missing
				if (!File.Exists(SteamCmdExe))
				{
					logCallback?.Invoke("[INIT] Downloading SteamCMD...");
					using (var client = new HttpClient())
					{
						var response = await client.GetByteArrayAsync("https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip");
						await File.WriteAllBytesAsync(ZipPath, response);
					}

					logCallback?.Invoke("[INIT] Unzipping SteamCMD...");
					ZipFile.ExtractToDirectory(ZipPath, SteamCmdDir, true);

					if (File.Exists(ZipPath)) File.Delete(ZipPath);
				}

				// 3. The "Deep Clean" Initialization (Only runs if the 'public' folder is missing)
				string publicFolder = Path.Combine(SteamCmdDir, "public");
				if (!Directory.Exists(publicFolder))
				{
					logCallback?.Invoke("[INIT] Starting first-run updates (this may take a few minutes)...");

					// Clear out any corrupted 'package' folders from a failed previous run
					string packageFolder = Path.Combine(SteamCmdDir, "package");
					if (Directory.Exists(packageFolder)) Directory.Delete(packageFolder, true);

					ProcessStartInfo startInfo = new()
					{
						FileName = SteamCmdExe,
						Arguments = "+quit",
						WorkingDirectory = SteamCmdDir,
						UseShellExecute = false,
						CreateNoWindow = true,
						RedirectStandardOutput = true,
						RedirectStandardError = true
					};

					using (Process proc = new() { StartInfo = startInfo })
					{
						proc.OutputDataReceived += (s, ev) => { if (!string.IsNullOrEmpty(ev.Data)) logCallback?.Invoke(ev.Data); };
						proc.Start();
						proc.BeginOutputReadLine();
						await proc.WaitForExitAsync(); // Waits for SteamCMD to finish its first-run downloads
					}
					logCallback?.Invoke("[INIT] SteamCMD is ready for action.");
				}
				else
				{
					logCallback?.Invoke("[INIT] SteamCMD already initialized.");
				}
			}
			catch (Exception ex)
			{
				logCallback?.Invoke($"[CRITICAL ERROR] SteamCMD Setup Failed: {ex.Message}");
			}
		}
	}
}