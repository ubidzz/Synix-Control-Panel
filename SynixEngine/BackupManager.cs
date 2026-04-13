using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Synix_Control_Panel.SynixEngine
{
	public enum StartContext { Manual, Scheduled, CrashRecovery }

	public static class BackupManager
	{
		public static void ExecuteBackup(GameServer server, StartContext context)
		{
			if (!server.BackupOnStart || context == StartContext.CrashRecovery) return;

			string sourceDir = server.InstallPath;
			string backupRoot = Path.Combine(@"C:\Synix\BackupGames", server.Game, server.ServerName);

			// 🎯 NEW: Professional UTC Timestamp format: backup_YYYY_MM_DD_HHMMSS
			// UTC is the global standard for server logs and coordination
			string timestamp = DateTime.UtcNow.ToString("yyyy_MM_dd_HHmmss");
			string zipPath = Path.Combine(backupRoot, $"backup_{timestamp}.zip");

			try
			{
				if (!Directory.Exists(backupRoot)) Directory.CreateDirectory(backupRoot);

				// 🎯 ROTATION: Keep only 3 newest
				var files = new DirectoryInfo(backupRoot).GetFiles("*.zip")
								.OrderByDescending(f => f.CreationTime).ToList();

				while (files.Count >= 3)
				{
					files.Last().Delete();
					files.RemoveAt(files.Count - 1);
				}

				if (Directory.Exists(sourceDir))
				{
					// 🎯 CPU OPTIMIZATION: Use CompressionLevel.Fastest
					ZipFile.CreateFromDirectory(sourceDir, zipPath, CompressionLevel.Fastest, false);
				}
			}
			catch (Exception ex)
			{
				// Log silently or to a debug console
				System.Diagnostics.Debug.WriteLine($"Backup error: {ex.Message}");
			}
		}
	}
}