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
namespace Synix_Control_Panel.SynixApp.FileFolderHandler
{
	public static class FolderHandler
	{
		public static void Create(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}

		public static class ServerFolder
		{
			public static void Delete(GameServer server, bool deleteBackups, Action<string, Color> logCallback)
			{
				try
				{
					if (Directory.Exists(server.InstallPath))
					{
						Directory.Delete(server.InstallPath, true);
						logCallback?.Invoke($"[CLEANUP] Deleted server '{server.ServerName}' and all files at {server.InstallPath}", Color.Yellow);
					}

					if (deleteBackups)
					{

						string cleanGame = SynixEngine.Core.Instance.GetSafeName(server.Game);
						string cleanServer = SynixEngine.Core.Instance.GetSafeName(server.ServerName);
						string baseBackupFolder = SynixEngine.Core.DefaultBackupPath;

						if (Properties.Settings.Default.UseCustomBackupPath &&
							!string.IsNullOrWhiteSpace(Properties.Settings.Default.CustomBackupPath) &&
							Directory.Exists(Properties.Settings.Default.CustomBackupPath))
						{
							baseBackupFolder = Properties.Settings.Default.CustomBackupPath;
						}

						string backupRoot = Path.Combine(baseBackupFolder, cleanGame, cleanServer);

						if (Directory.Exists(backupRoot))
						{
							Directory.Delete(backupRoot, true);
							logCallback?.Invoke($"[CLEANUP] Deleted server backups at {backupRoot}", Color.LimeGreen);
						}
					}

					if (MainGUI.serverList.Contains(server))
					{
						MainGUI.serverList.Remove(server);
					}

					FileHandler.SaveServers();
				}
				catch (Exception ex)
				{
					throw new Exception(ex.Message);
				}
			}

			public static bool Rename(GameServer oldServer, GameServer newServer)
			{
				// 1. GATEKEEPER: If they didn't use Default Location, DO NOT RENAME.
				if (!oldServer.IsDefaultPath)
				{
					return false; // Exit early; no physical folder movement
				}

				// 2. Only move if the path actually changed
				if (oldServer.InstallPath != newServer.InstallPath)
				{
					try
					{
						if (Directory.Exists(oldServer.InstallPath))
						{
							Directory.Move(oldServer.InstallPath, newServer.InstallPath);
							return true;
						}
					}
					catch (Exception ex)
					{
						throw new Exception("Folder move failed: " + ex.Message);
					}
				}
				return false;
			}
		}
	}
}
