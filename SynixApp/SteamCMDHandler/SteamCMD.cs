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
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using Synix_Control_Panel.SynixEngine;
using System.Diagnostics;
using System.IO.Compression;

namespace Synix_Control_Panel.SynixApp.SteamCMDHandler
{
	public static class SteamCMD
	{
		private static readonly string ZipPath = Path.Combine(Core.SteamCmdPath, "steamcmd.zip");

		public static async Task EnsureSteamCMD(Action<string, Color> logCallback)
		{
			try
			{
				logCallback?.Invoke(LocalizationManager.Get("SteamCmd.Activity.CloseDisabled"), Color.Orange);
				Core.Instance.isDownloadActive = true;
				logCallback?.Invoke(LocalizationManager.Get("SteamCmd.Activity.Checking"), Color.Cyan);

				if (!Directory.Exists(Core.SteamCmdPath))
				{
					logCallback?.Invoke(LocalizationManager.Get("SteamCmd.Activity.CreatingFolder"), Color.Yellow);
					FolderHandler.Create(Core.SteamCmdPath);
				}

				if (!File.Exists(Core.SteamCmdExe))
				{
					logCallback?.Invoke(LocalizationManager.Get("SteamCmd.Activity.Downloading"), Color.Cyan);
					using (var client = new HttpClient())
					{
						var response = await client.GetByteArrayAsync("https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip");
						await File.WriteAllBytesAsync(ZipPath, response);
					}

					logCallback?.Invoke(LocalizationManager.Get("SteamCmd.Activity.Extracting"), Color.Cyan);
					ZipFile.ExtractToDirectory(ZipPath, Core.SteamCmdPath, true);

					if (File.Exists(ZipPath)) File.Delete(ZipPath);
				}

				string publicFolder = Path.Combine(Core.SteamCmdPath, "public");
				if (!Directory.Exists(publicFolder))
				{
					logCallback?.Invoke(LocalizationManager.Get("SteamCmd.Activity.FirstRun"), Color.Yellow);

					string packageFolder = Path.Combine(Core.SteamCmdPath, "package");
					if (Directory.Exists(packageFolder)) Directory.Delete(packageFolder, true);

					ProcessStartInfo startInfo = new()
					{
						FileName = Core.SteamCmdExe,
						Arguments = "+quit",
						WorkingDirectory = Core.SteamCmdPath,
						UseShellExecute = false,
						CreateNoWindow = true,
						RedirectStandardOutput = true,
						RedirectStandardError = true
					};

					using (Process proc = new() { StartInfo = startInfo })
					{
						proc.OutputDataReceived += (s, ev) => { if (!string.IsNullOrEmpty(ev.Data)) logCallback?.Invoke(ev.Data, Color.White); };
						proc.Start();
						proc.BeginOutputReadLine();
						await proc.WaitForExitAsync();
					}
					logCallback?.Invoke(LocalizationManager.Get("SteamCmd.Activity.Ready"), Color.Lime);
				}
				else
				{
					logCallback?.Invoke(LocalizationManager.Get("SteamCmd.Activity.AlreadyReady"), Color.Cyan);
				}
				logCallback?.Invoke(LocalizationManager.Get("SteamCmd.Activity.Complete"), Color.LimeGreen);
			}
			catch (Exception ex)
			{
				logCallback?.Invoke(LocalizationManager.Get("SteamCmd.Activity.SetupFailed", ex.Message), Color.Red);
			}
			finally
			{
				Core.Instance.isDownloadActive = false;
				logCallback?.Invoke(LocalizationManager.Get("SteamCmd.Activity.CloseEnabled"), Color.Orange);
			}
		}
	}
}
