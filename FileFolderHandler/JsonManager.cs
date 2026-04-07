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
using Synix_Control_Panel.FileFolderHandler; // Points to your utility folder
using Synix_Control_Panel.ServerHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Synix_Control_Panel
{
	public static class JsonManager
	{
		private static readonly string FolderPath = @"C:\games\SynixData";
		private static readonly string FileName = "servers.json";

		public static void Save()
		{
			try
			{
				var options = new JsonSerializerOptions { WriteIndented = true };
				string jsonString = JsonSerializer.Serialize(MainGUI.serverList, options);

				// Use your dedicated CreateFiles class to handle directory checks and writing
				bool success = CreateFiles.Create(FolderPath, FileName, jsonString);

				if (success)
				{
					MainGUI.Instance?.AppendLog("JSON saved successfully to C:\\games.");
				}
				else
				{
					MainGUI.Instance?.AppendLog("Save Error: CreateFiles utility failed to write the file.");
				}
			}
			catch (Exception ex)
			{
				MainGUI.Instance?.AppendLog("Save Error: " + ex.Message);
			}
		}

		public static void Load()
		{
			// Construction of the path for reading
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
	}
}