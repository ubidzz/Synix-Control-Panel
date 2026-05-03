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
		public void ExecuteBackup(GameServer server, StartContext context)
		{
			if (context != StartContext.Manual && !server.BackupOnStart) return;

			if (server.Status != StatusManager.GetStatus(ServerState.Stopped))
			{
				Log($"[🚨 ERROR] {server.ServerName} must be Stopped to perform a backup.", Color.Orange);
				return;
			}

			Log($"[⚠ WARNING] Synix close window button is now Disabled!", Color.Orange, true);
			Log($"[💾 BACKUP] Starting backup compression for {server.ServerName}...", Color.Cyan);

			server.Status = StatusManager.GetStatus(Core.ServerState.BackingUp);
			UpdateGridStatus();

			// Never backup during a crash recovery
			if (context == StartContext.CrashRecovery) return;

			// 1. DYNAMIC PATHING
			string sourceDir = server.InstallPath;

			// 🎯 THE FIX: Apply the sanitizer HERE to ensure the folder matches the UI
			string cleanGame = GetSafeName(server.Game);
			string cleanServer = GetSafeName(server.ServerName);

			// Path: C:\Synix\BackupGames\Soulmask_Dedicated\My_Server_Instance\
			string backupRoot = Path.Combine(@"C:\Synix\BackupGames", cleanGame, cleanServer);

			// 2. TIMESTAMP & FILENAME
			string timestamp = DateTime.UtcNow.ToString("yyyy_MM_dd_HHmmss");
			string zipPath = Path.Combine(backupRoot, $"backup_{timestamp}.zip");

			try
			{
				// Ensure the sanitized folder structure exists
				if (!Directory.Exists(backupRoot)) Directory.CreateDirectory(backupRoot);

				// 3. ROTATION LOGIC: Keep only 3 newest
				var files = new DirectoryInfo(backupRoot).GetFiles("*.zip")
								.OrderByDescending(f => f.CreationTime).ToList();

				while (files.Count >= 3)
				{
					files.Last().Delete();
					files.RemoveAt(files.Count - 1);
				}

				// 4. COMPRESSION: Respecting the 1% CPU target
				if (Directory.Exists(sourceDir))
				{
					// Using Fastest compression to avoid lag on your 6-core rig
					ZipFile.CreateFromDirectory(sourceDir, zipPath, CompressionLevel.Fastest, false);
				}
				Log($"[💾 BACKUP] Finished backing up {server.ServerName}.", Color.LimeGreen);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[BACKUP ERROR] {ex.Message}");
			}
			server.Status = StatusManager.GetStatus(Core.ServerState.Stopped);
			Log($"[⚠ WARNING] Synix close window button is now Enabled!", Color.Orange, true);
			UpdateGridStatus();
		}

		// 🎯 SHARED SANITIZER: Public Static so MainGUI.cs can use it too
		public string GetSafeName(string name)
		{
			if (string.IsNullOrWhiteSpace(name)) return "Unknown";

			// Replace spaces with underscores for file-system safety
			return name.Replace(" ", "_");
		}
	}
}
