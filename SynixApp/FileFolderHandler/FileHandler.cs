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
		private static readonly string FileName = "servers.json";

		private const int LogQueueCapacity = 4096;
		private const int MaxLogFilesPerCategory = 10;

		private static readonly object _logWriteLock = new();

		private static readonly Channel<(string LogFileName, string Content)>
			_logQueue = Channel.CreateBounded<(string LogFileName, string Content)>(
				new BoundedChannelOptions(LogQueueCapacity)
				{
					SingleReader = true,
					SingleWriter = false,
					AllowSynchronousContinuations = false,
					FullMode = BoundedChannelFullMode.DropOldest
				});

		private static readonly Task _logWorkerTask;
		private static int _loggingShutdownStarted;

		static FileHandler()
		{
			_logWorkerTask = Task.Run(ProcessLogQueueAsync);
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
					string savedPath = Path.Combine(Core.DataPath, FileName);

					MainGUI.Instance?.AppendLog($"[📜 INFO] JSON saved successfully to {savedPath}.", Color.DarkSeaGreen);
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
						bool migratedLegacyGameName = false;
						MainGUI.serverList.Clear();
						foreach (var server in loadedServers)
						{
							string canonicalGameName = GameDatabase.GetCanonicalGameName(server.Game);
							if (!server.Game.Equals(canonicalGameName, StringComparison.Ordinal))
							{
								server.Game = canonicalGameName;
								migratedLegacyGameName = true;
							}

							var masterData = GameDatabase.GetGame(server.Game);
							if (masterData != null)
							{
								server.AppID = masterData.AppID;
								server.ExeName = masterData.ExeName;
								server.RequiredArgs = masterData.RequiredArgs;
								server.Maps = masterData.Maps.ToList();
								if (server.QueryPort <= 0)
									server.QueryPort = masterData.QueryPort;

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

						if (migratedLegacyGameName)
						{
							SaveServers();
							MainGUI.Instance?.AppendLog(
								"[MIGRATION] Updated legacy 'Minecraft Java' server entries to 'Minecraft'.",
								Color.DarkSeaGreen);
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
			if (string.IsNullOrWhiteSpace(logFileName) ||
				string.IsNullOrWhiteSpace(content))
			{
				return false;
			}

			// Do not accept more queued entries after application shutdown starts.
			if (Volatile.Read(ref _loggingShutdownStarted) != 0)
				return false;

			return _logQueue.Writer.TryWrite(
				(logFileName, content));
		}

		private static async Task ProcessLogQueueAsync()
		{
			await foreach (var entry in
				_logQueue.Reader.ReadAllAsync())
			{
				WriteLogCore(
					entry.LogFileName,
					entry.Content);
			}
		}

		/// <summary>
		/// Completes the logging queue and waits until every queued entry
		/// has been processed. Call this after Application.Run returns.
		/// </summary>
		public static async Task FlushLogsAsync()
		{
			if (Interlocked.Exchange(
					ref _loggingShutdownStarted,
					1) == 0)
			{
				_logQueue.Writer.TryComplete();
			}

			try
			{
				await _logWorkerTask.ConfigureAwait(false);
			}
			catch
			{
				// Application shutdown must continue even if logging failed.
			}
		}

		public static bool WriteLogImmediate(
	string logFileName,
	string content)
		{
			if (string.IsNullOrWhiteSpace(logFileName) ||
				string.IsNullOrWhiteSpace(content))
			{
				return false;
			}

			return WriteLogCore(logFileName, content);
		}

		private static bool WriteLogCore(
			string logFileName,
			string content)
		{
			lock (_logWriteLock)
			{
				try
				{
					FolderHandler.Create(Core.LogsPath);

					string safeLogFileName =
						SanitizeLogFileName(logFileName);

					DateTime currentTime = DateTime.Now;

					string dailyFileName =
						$"{safeLogFileName}_{currentTime:yyyy-MM-dd}.log";

					string fullPath = Path.Combine(
						Core.LogsPath,
						dailyFileName);

					bool isNewDailyFile = !File.Exists(fullPath);

					File.AppendAllText(
						fullPath,
						content.TrimEnd() + Environment.NewLine);

					// Retention only needs to run when a new daily file
					// is created, not after every individual log message.
					if (isNewDailyFile)
					{
						CleanupOldLogFiles(safeLogFileName);
					}

					return true;
				}
				catch
				{
					return false;
				}
			}
		}

		private static void CleanupOldLogFiles(
			string safeLogFileName)
		{
			try
			{
				var oldLogFiles = new DirectoryInfo(Core.LogsPath)
					.GetFiles(
						$"{safeLogFileName}_*.log",
						SearchOption.TopDirectoryOnly)
					.OrderByDescending(
						file => file.LastWriteTimeUtc)
					.Skip(MaxLogFilesPerCategory)
					.ToList();

				foreach (FileInfo oldLogFile in oldLogFiles)
				{
					try
					{
						oldLogFile.Delete();
					}
					catch
					{
						// A locked old log should not prevent current logging.
					}
				}
			}
			catch
			{
				// Retention failure should not make the current write fail.
			}
		}

		private static string SanitizeLogFileName(
			string logFileName)
		{
			char[] characters = logFileName.Trim().ToCharArray();
			char[] invalidCharacters = Path.GetInvalidFileNameChars();

			for (int index = 0; index < characters.Length; index++)
			{
				char character = characters[index];

				if (Array.IndexOf(invalidCharacters, character) >= 0 ||
					character == '/' ||
					character == '\\' ||
					character == '*' ||
					character == '?')
				{
					characters[index] = '_';
				}
			}

			string safeName = new string(characters)
				.Trim(' ', '.');

			if (string.IsNullOrWhiteSpace(safeName))
				safeName = "Synix";

			// Prevent excessively long Windows paths.
			if (safeName.Length > 80)
				safeName = safeName[..80];

			return safeName;
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
