using System;
using System.IO;

namespace Game_Server_Control_Panel.FileEditor
{
	public static class RenameServerFolder
	{
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
