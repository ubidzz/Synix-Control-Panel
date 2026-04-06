using System;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;
using Game_Server_Control_Panel.ServerHandler;

namespace Game_Server_Control_Panel.FileFolderHandler
{
	public static class JsonManager
	{
		private static readonly string FileName = "servers.json";

		public static void Save()
		{
			try
			{
				var options = new JsonSerializerOptions { WriteIndented = true };
				// Directly use the static list from MainGUI
				string jsonString = JsonSerializer.Serialize(MainGUI.serverList, options);
				File.WriteAllText(FileName, jsonString);

				MainGUI.Instance?.AppendLog("JSON saved successfully.");
			}
			catch (Exception ex)
			{
				MainGUI.Instance?.AppendLog("Save Error: " + ex.Message);
			}
		}

		public static void Load()
		{
			if (File.Exists(FileName))
			{
				try
				{
					string jsonString = File.ReadAllText(FileName);
					var loadedServers = JsonSerializer.Deserialize<List<GameServer>>(jsonString);

					if (loadedServers != null)
					{
						MainGUI.serverList.Clear();
						foreach (var server in loadedServers)
						{
							// Find the original game data using the name (e.g., "Soulmask")
							var masterData = GameDatabase.GetGame(server.Game);

							if (masterData != null)
							{
								// Put the "brains" back into the server object
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