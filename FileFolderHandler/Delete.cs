using System;
using System.IO;
using Game_Server_Control_Panel;
using Game_Server_Control_Panel.FileFolderHandler;
using Game_Server_Control_Panel.ServerHandler;

namespace Game_Server_Control_Panel.FileFolderHandler
{
	public static class Delete
	{
		public static void Server(GameServer server, Action<string> logCallback)
		{
			try
			{
				// 1. Delete the physical files first
				if (Directory.Exists(server.InstallPath))
				{
					// 'true' means it deletes all subfolders and files inside
					Directory.Delete(server.InstallPath, true);
				}

				// 2. Remove from the UI list and Save JSON
				// We access the static list from MainGUI directly
				MainGUI.serverList.Remove(server);
				JsonManager.Save();

				logCallback?.Invoke($"[CLEANUP] Deleted server '{server.ServerName}' and all files at {server.InstallPath}");
			}
			catch (Exception ex)
			{
				// Rethrow the error so the GUI can show the specific MessageBox you want
				throw new Exception(ex.Message);
			}
		}
	}
}