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
using Synix_Control_Panel.Database;
using Synix_Control_Panel.FileFolderHandler; // Points to your CreateFolders utility
using System.Text.Json;

namespace Synix_Control_Panel
{
	public static class FileHandler
	{
		private static readonly string FolderPath = @"C:\Synix\SynixData";
		private static readonly string FileName = "servers.json";
		public static void SaveServers()
		{
			try
			{
				var options = new JsonSerializerOptions { WriteIndented = true };
				string jsonString = JsonSerializer.Serialize(MainGUI.serverList, options);

				bool success = Create(FolderPath, FileName, jsonString);

				if (success)
				{
					MainGUI.Instance?.AppendLog("[📜 INFO] JSON saved successfully to C:\\Synix\\SynixData\\servers.json.", Color.DarkSeaGreen);
				}
			}
			catch (Exception ex)
			{
				MainGUI.Instance?.AppendLog("[🚨 ERROR] Save Error: " + ex.Message);
			}
		}

		public static void LoadServers()
		{
			string fullPath = Path.Combine(FolderPath, FileName);

			if (File.Exists(fullPath))
			{
				try
				{
					string jsonString = File.ReadAllText(fullPath);
					var loadedServers = JsonSerializer.Deserialize<List<GameServer>>(jsonString);

					if (loadedServers != null)
					{
						MainGUI.serverList.Clear();
						foreach (var server in loadedServers)
						{
							// 1. Grab the hardcoded data from the switch statement in your screenshot
							var masterData = GameDatabase.GetGame(server.Game);
							if (masterData != null)
							{
								server.AppID = masterData.AppID;
								server.ExeName = masterData.ExeName;
								server.RequiredArgs = masterData.RequiredArgs;
								server.Maps = masterData.Maps.ToList();

								// 2. Smash the JSON path and Hardcoded ExeName together
								string fullExePath = Path.Combine(server.InstallPath, server.ExeName);

								// 3. Extract the icon
								string iconPath = Synix_Control_Panel.SynixEngine.Core.GetLocalServerIcon(server.AppID, fullExePath);

								// 4. Attach it permanently to the object
								if (File.Exists(iconPath))
								{
									server.DisplayIcon = System.Drawing.Image.FromFile(iconPath);
								}
							}
							MainGUI.serverList.Add(server);
						}
					}
				}
				catch (Exception ex)
				{
					MainGUI.Instance?.AppendLog($"[🚨 ERROR] Load failed: {ex.Message}");
				}
			}
		}

		public static bool Create(string folderPath, string fileName, string content)
		{
			try
			{
				FolderHandler.Create(folderPath);

				string fullPath = Path.Combine(folderPath, fileName);

				File.WriteAllText(fullPath, content);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public static bool WriteLog(string logFileName, string content)
		{
			// Prevent logging if the content is empty or just white space
			if (string.IsNullOrWhiteSpace(content)) return false;

			try
			{
				string logFolder = FolderPath + "\\logs";
				FolderHandler.Create(logFolder);

				// Create filename based on today's date
				string fileName = $"{logFileName}_{DateTime.Now:yyyy-MM-dd}.log";
				string fullPath = Path.Combine(logFolder, fileName);

				// .TrimEnd() removes the extra invisible characters that cause the empty lines
				File.AppendAllText(fullPath, content.TrimEnd() + Environment.NewLine);

				// Get all .log files and sort them by Name (descending)
				// Since the name is yyyy-MM-dd, the newest date is always at the top
				var logFiles = new DirectoryInfo(logFolder)
					.GetFiles("*.txt")
					.OrderByDescending(f => f.Name)
					.ToList();

				// Keep only the 10 most recent files
				if (logFiles.Count > 10)
				{
					for (int i = 10; i < logFiles.Count; i++)
					{
						logFiles[i].Delete();
					}
				}

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public static bool Copy(string sourceFilePath, string targetFolderPath, string targetFileName, bool overwrite = true)
		{
			try
			{
				if (!File.Exists(sourceFilePath))
				{
					return false;
				}

				FolderHandler.Create(targetFolderPath);

				string fullTargetPath = Path.Combine(targetFolderPath, targetFileName);

				File.Copy(sourceFilePath, fullTargetPath, overwrite);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
