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
namespace Synix_Control_Panel.FileFolderHandler
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
			public static void Delete(GameServer server, Action<string> logCallback)
			{
				try
				{
					if (Directory.Exists(server.InstallPath))
					{
						Directory.Delete(server.InstallPath, true);
					}

					MainGUI.serverList.Remove(server);
					FileHandler.SaveServers();

					logCallback?.Invoke($"[CLEANUP] Deleted server '{server.ServerName}' and all files at {server.InstallPath}");
				}
				catch (Exception ex)
				{
					throw new Exception(ex.Message);
				}
			}

			public static bool Rename(GameServer oldServer, GameServer newServer)
			{
				if (!oldServer.IsDefaultPath)
				{
					return false;
				}

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