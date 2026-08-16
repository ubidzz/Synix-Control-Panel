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
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixEngine;
using System.Text.Json;
using System.Threading.Channels;

namespace Synix_Control_Panel.SynixApp.FileFolderHandler
{
	public static class FileHandler
	{
		private static readonly string FolderPath = Core.DataPath;
		private static readonly string FileName = "servers.json";
		private static readonly object _logWriteLock = new();
		private static readonly Channel<(string LogFileName, string Content)> _logQueue = Channel.CreateUnbounded<(string LogFileName, string Content)>(
		new UnboundedChannelOptions
		{
			SingleReader = true,
			SingleWriter = false
		});

		static FileHandler()
		{
			_ = Task.Run(ProcessLogQueueAsync);
		}

		public static void SaveServers()
		{
			try
			{
				var options = new JsonSerializerOptions { WriteIndented = true };
				string jsonString = JsonSerializer.Serialize(MainGUI.serverList, options);

				bool success = Create(Core.DataPath, FileName, jsonString);

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
			string fullPath = Path.Combine(Core.DataPath, FileName);

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

								string fullExePath = Path.Combine(server.InstallPath, server.ExeName);

								string iconPath = Synix_Control_Panel.SynixEngine.Core.GetLocalServerIcon(server.Game, fullExePath);

								if (File.Exists(iconPath))
								{
									if (!MainGUI.ServerIconsCache.ContainsKey(server.Game))
									{
										using (var ms = new MemoryStream(File.ReadAllBytes(iconPath)))
										{
											using (var tempImage = System.Drawing.Image.FromStream(ms))
											{
												MainGUI.ServerIconsCache[server.Game] = new Bitmap(tempImage);
											}
										}
									}
									server.DisplayIcon = MainGUI.ServerIconsCache[server.Game];
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

		public static bool QueueLog(string logFileName, string content)
		{
			if (string.IsNullOrWhiteSpace(content))
				return false;

			return _logQueue.Writer.TryWrite((logFileName, content));
		}

		private static async Task ProcessLogQueueAsync()
		{
			await foreach (var entry in _logQueue.Reader.ReadAllAsync())
			{
				WriteLogCore(entry.LogFileName, entry.Content);
			}
		}

		public static bool WriteLogImmediate(string logFileName, string content)
		{
			lock (_logWriteLock)
			{
				return WriteLogCore(logFileName, content);
			}
		}

		private static bool WriteLogCore(string logFileName, string content)
		{
			try
			{
				FolderHandler.Create(Core.LogsPath);

				string fileName = $"{logFileName}_{DateTime.Now:yyyy-MM-dd}.log";
				string fullPath = Path.Combine(Core.LogsPath, fileName);

				File.AppendAllText(
					fullPath,
					content.TrimEnd() + Environment.NewLine);

				var logFiles = new DirectoryInfo(Core.LogsPath)
					.GetFiles("*.log")
					.OrderByDescending(f => f.Name)
					.ToList();

				for (int i = 10; i < logFiles.Count; i++)
				{
					logFiles[i].Delete();
				}

				return true;
			}
			catch
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
