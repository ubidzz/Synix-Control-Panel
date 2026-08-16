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
using System.IO.Compression;

namespace Synix_Control_Panel.SynixEngine
{
	public enum StartContext { Manual, Scheduled, CrashRecovery }

	public partial class Core
	{
		public async Task ExecuteBackup(GameServer server, StartContext context)
		{
			if (context == StartContext.CrashRecovery) return;

			if (server.Status != StatusManager.GetStatus(ServerState.Stopped))
			{
				Log($"[🚨 ERROR] {server.ServerName} must be Stopped to perform a backup.", Color.Orange);
				return;
			}

			Log($"[⚠ WARNING] Synix close window button is now Disabled!", Color.Orange, true);
			isDownloadActive = true;
			Log($"[💾 BACKUP] Starting backup compression for {server.ServerName}...", Color.Cyan);

			server.Status = StatusManager.GetStatus(Core.ServerState.BackingUp);
			UpdateGridStatus();

			string sourceDir = server.InstallPath;

			string cleanGame = GetSafeName(server.Game);
			string cleanServer = GetSafeName(server.ServerName);
			string baseBackupFolder = DefaultBackupPath;

			if (Properties.Settings.Default.UseCustomBackupPath &&
				!string.IsNullOrWhiteSpace(Properties.Settings.Default.CustomBackupPath) &&
				Directory.Exists(Properties.Settings.Default.CustomBackupPath))
			{
				baseBackupFolder = Properties.Settings.Default.CustomBackupPath;
			}

			string backupRoot = Path.Combine(baseBackupFolder, cleanGame, cleanServer);
			string timestamp = DateTime.UtcNow.ToString("yyyy_MM_dd_HHmmss");
			string zipPath = Path.Combine(backupRoot, $"backup_{timestamp}.zip");

			try
			{
				if (!Directory.Exists(backupRoot)) Directory.CreateDirectory(backupRoot);

				var files = new DirectoryInfo(backupRoot).GetFiles("*.zip").OrderByDescending(f => f.CreationTime).ToList();

				while (files.Count >= Properties.Settings.Default.MaxBackups)
				{
					files.Last().Delete();
					files.RemoveAt(files.Count - 1);
				}

				if (Directory.Exists(sourceDir))
				{
					await Task.Run(() =>
					{
						ZipFile.CreateFromDirectory(
							sourceDirectoryName: sourceDir,
							destinationArchiveFileName: zipPath,
							compressionLevel: CompressionLevel.Optimal,
							includeBaseDirectory: true
						);
					});
				}
				Log($"[💾 BACKUP] Backup location: {zipPath}.", Color.LimeGreen);
				Log($"[💾 BACKUP] Finished backing up {server.ServerName}.", Color.LimeGreen);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[BACKUP ERROR] {ex.Message}");
			}

			server.Status = StatusManager.GetStatus(Core.ServerState.Stopped);
			isDownloadActive = false;
			Log($"[⚠ WARNING] Synix close window button is now Enabled!", Color.Orange, true);
			UpdateGridStatus();
		}

		public string GetSafeName(string name)
		{
			if (string.IsNullOrWhiteSpace(name)) return "Unknown";
			string cleanName = name.Replace(" ", "_").Replace(":", "_");

			// Strip out any illegal Windows path characters (like <, >, *, ?, ", \, |, /)
			foreach (char c in System.IO.Path.GetInvalidFileNameChars())
			{
				cleanName = cleanName.Replace(c.ToString(), "");
			}

			return cleanName;
		}
	}
}
