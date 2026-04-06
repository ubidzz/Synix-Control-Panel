using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.ComponentModel;

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
						// 1. Clear the static list first
						MainGUI.serverList.Clear();

						// 2. Repopulate it
						foreach (var server in loadedServers)
						{
							MainGUI.serverList.Add(server);
						}
					}
				}
				catch (Exception ex)
				{
					System.Windows.Forms.MessageBox.Show($"Could not load servers: {ex.Message}");
				}
			}
		}
	}
}