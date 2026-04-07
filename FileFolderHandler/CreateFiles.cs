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
using Synix_Control_Panel.FileFolderHandler; // Points to your CreateFolders utility
using Synix_Control_Panel.ServerHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Synix_Control_Panel
{
	public static class CreateFiles
	{
		private static readonly string FolderPath = @"C:\games\SynixData";
		private static readonly string FileName = "servers.json";

		// --- SECTION 1: SERVER SPECIFIC LOGIC ---

		public static void SaveServers()
		{
			try
			{
				var options = new JsonSerializerOptions { WriteIndented = true };
				string jsonString = JsonSerializer.Serialize(MainGUI.serverList, options);

				// Calls the generic Create method below to handle the heavy lifting
				bool success = Create(FolderPath, FileName, jsonString);

				if (success)
				{
					MainGUI.Instance?.AppendLog("JSON saved successfully to C:\\games.");
				}
			}
			catch (Exception ex)
			{
				MainGUI.Instance?.AppendLog("Save Error: " + ex.Message);
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
							var masterData = GameDatabase.GetGame(server.Game);
							if (masterData != null)
							{
								server.AppID = masterData.AppID;
								server.ExeName = masterData.ExeName;
								server.RequiredArgs = masterData.RequiredArgs;
								server.Maps = masterData.Maps.ToList();
							}
							MainGUI.serverList.Add(server);
						}
					}
				}
				catch (Exception ex)
				{
					MainGUI.Instance?.AppendLog($"[ERROR] Load failed: {ex.Message}");
				}
			}
		}

		// --- SECTION 2: GENERIC FILE UTILITY ---

		public static bool Create(string folderPath, string fileName, string content)
		{
			try
			{
				// Uses your dedicated CreateFolders utility to ensure the path is ready
				CreateFolders.Create(folderPath);

				string fullPath = Path.Combine(folderPath, fileName);

				// WriteAllText creates a new file, or overwrites an existing one
				File.WriteAllText(fullPath, content);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}