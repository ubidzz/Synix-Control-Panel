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
using System.IO.Compression;
using System.Text.Json;

namespace Synix_Control_Panel.SynixEngine
{
	public enum StartContext { Manual, Scheduled, CrashRecovery }

	internal sealed record ServerBackupArchive(
		string ArchivePath,
		DateTime CreatedUtc,
		long CompressedBytes)
	{
		public string FileName => Path.GetFileName(ArchivePath);
		public DateTime CreatedLocal => CreatedUtc.ToLocalTime();
	}

	internal sealed record ServerBackupRestoreResult(
		bool Succeeded,
		string Message,
		long RestoredBytes = 0);

	public partial class Core
	{
		private const string RestoreOperationPrefix = ".synix-restore-";
		private const string RestoreRollbackMarker = ".synix-restore-rollback-";
		internal static string? RestoreJournalFolderOverride { get; set; }
		private static string RestoreJournalFolder =>
			RestoreJournalFolderOverride ??
				Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					"Synix",
					"RestoreOperations");

		public async Task ExecuteBackup(GameServer server, StartContext context)
		{
			if (context == StartContext.CrashRecovery) return;

			if (server.Status != StatusManager.GetStatus(ServerState.Stopped))
			{
				Log($"[🚨 ERROR] {server.ServerName} must be Stopped to perform a backup.", Color.Orange);
				return;
			}

			Log("[⚠ WARNING] Synix close window button is now Disabled!", Color.Orange, true);
			isDownloadActive = true;
			Log($"[💾 BACKUP] Starting backup compression for {server.ServerName}...", Color.Cyan);

			server.Status = StatusManager.GetStatus(ServerState.BackingUp);
			UpdateGridStatus();

			string sourceDir = server.InstallPath;
			string backupRoot = GetActiveServerBackupFolder(server);
			string timestamp = DateTime.UtcNow.ToString("yyyy_MM_dd_HHmmss_fff");
			string zipPath = Path.Combine(backupRoot, $"backup_{timestamp}.zip");

			try
			{
				Directory.CreateDirectory(backupRoot);

				if (!Directory.Exists(sourceDir))
					throw new DirectoryNotFoundException($"The server folder does not exist: {sourceDir}");
				if (IsSameOrDescendantPath(zipPath, sourceDir))
					throw new InvalidOperationException("The backup folder cannot be stored inside the server installation folder.");

				await Task.Run(() => ZipFile.CreateFromDirectory(
					sourceDirectoryName: sourceDir,
					destinationArchiveFileName: zipPath,
					compressionLevel: CompressionLevel.Optimal,
					includeBaseDirectory: true));

				List<FileInfo> files = new DirectoryInfo(backupRoot)
					.GetFiles("*.zip")
					.OrderByDescending(file => file.LastWriteTimeUtc)
					.ToList();
				int maximumBackups = Math.Max(1, Properties.Settings.Default.MaxBackups);
				while (files.Count > maximumBackups)
				{
					files.Last().Delete();
					files.RemoveAt(files.Count - 1);
				}

				Log($"[💾 BACKUP] Backup location: {zipPath}.", Color.LimeGreen);
				Log($"[💾 BACKUP] Finished backing up {server.ServerName}.", Color.LimeGreen);
			}
			catch (Exception exception)
			{
				TryDeleteBackupRestoreFile(zipPath);
				Log($"[🚨 BACKUP ERROR] {exception.Message}", Color.Red, true);
			}
			finally
			{
				server.Status = StatusManager.GetStatus(ServerState.Stopped);
				isDownloadActive = false;
				Log("[⚠ WARNING] Synix close window button is now Enabled!", Color.Orange, true);
				UpdateGridStatus();
			}
		}

		internal string GetActiveServerBackupFolder(GameServer server)
		{
			string cleanGame = GetSafeName(server.Game);
			string cleanServer = GetSafeName(server.ServerName);
			return Path.Combine(GetActiveBackupBaseFolder(), cleanGame, cleanServer);
		}

		internal IReadOnlyList<ServerBackupArchive> GetServerBackups(GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);

			HashSet<string> folders = new(StringComparer.OrdinalIgnoreCase)
			{
				GetActiveServerBackupFolder(server),
				Path.Combine(DefaultBackupPath, GetSafeName(server.Game), GetSafeName(server.ServerName))
			};

			List<ServerBackupArchive> backups = [];
			foreach (string folder in folders)
			{
				try
				{
					if (!Directory.Exists(folder))
						continue;

					foreach (FileInfo file in new DirectoryInfo(folder).GetFiles("*.zip"))
					{
						backups.Add(new ServerBackupArchive(
							file.FullName,
							file.LastWriteTimeUtc,
							file.Length));
					}
				}
				catch (IOException)
				{
				}
				catch (UnauthorizedAccessException)
				{
				}
			}

			return backups
				.GroupBy(backup => backup.ArchivePath, StringComparer.OrdinalIgnoreCase)
				.Select(group => group.First())
				.OrderByDescending(backup => backup.CreatedUtc)
				.ToArray();
		}

		internal bool HasServerBackups(GameServer server) =>
			GetServerBackups(server).Count > 0;

		internal async Task<ServerBackupRestoreResult> RestoreServerBackupAsync(
			GameServer server,
			ServerBackupArchive backup,
			IProgress<string>? progress = null)
		{
			ArgumentNullException.ThrowIfNull(server);
			ArgumentNullException.ThrowIfNull(backup);

			if (server.Status != StatusManager.GetStatus(ServerState.Stopped) ||
				server.PID.HasValue)
			{
				return new ServerBackupRestoreResult(
					false,
					"Stop the server before restoring a backup.");
			}

			string selectedPath = Path.GetFullPath(backup.ArchivePath);
			bool isKnownBackup = GetServerBackups(server).Any(candidate =>
				string.Equals(
					Path.GetFullPath(candidate.ArchivePath),
					selectedPath,
					StringComparison.OrdinalIgnoreCase));
			if (!isKnownBackup || !File.Exists(selectedPath))
			{
				return new ServerBackupRestoreResult(
					false,
					"The selected backup is missing or is not stored in this server's backup folder.");
			}

			isDownloadActive = true;
			server.Status = StatusManager.GetStatus(ServerState.Restoring);
			UpdateGridStatus();
			Log("[⚠ WARNING] Synix close window button is now Disabled!", Color.Orange, true);
			Log($"[♻ RESTORE] Preparing {backup.FileName} for {server.ServerName}...", Color.Cyan, true);

			try
			{
				ServerBackupRestoreResult result = await Task.Run(() =>
					RestoreServerBackup(server, backup, progress));
				Log(
					result.Succeeded
						? $"[♻ RESTORE] {server.ServerName} was restored from {backup.FileName}."
						: $"[🚨 RESTORE ERROR] {result.Message}",
					result.Succeeded ? Color.LimeGreen : Color.Red,
					true);
				return result;
			}
			catch (Exception exception)
			{
				Log($"[🚨 RESTORE ERROR] {exception.Message}", Color.Red, true);
				return new ServerBackupRestoreResult(false, exception.Message);
			}
			finally
			{
				server.Status = StatusManager.GetStatus(ServerState.Stopped);
				isDownloadActive = false;
				Log("[⚠ WARNING] Synix close window button is now Enabled!", Color.Orange, true);
				UpdateGridStatus();
			}
		}

		internal static int RecoverInterruptedServerRestores()
		{
			if (!Directory.Exists(RestoreJournalFolder))
				return 0;

			int recovered = 0;
			foreach (string journalPath in Directory.EnumerateFiles(
				RestoreJournalFolder,
				"*.json",
				SearchOption.TopDirectoryOnly))
			{
				RestoreJournal journal = JsonSerializer.Deserialize<RestoreJournal>(
					File.ReadAllText(journalPath)) ??
					throw new InvalidDataException("A server restore recovery record is empty.");

				ValidateRestoreJournal(journal);
				bool restoreWasActivated = string.Equals(
					journal.Phase,
					"Restored",
					StringComparison.Ordinal);
				if (restoreWasActivated && Directory.Exists(journal.InstallPath))
				{
					if (Directory.Exists(journal.RollbackPath))
						Directory.Delete(journal.RollbackPath, true);
				}
				else if (Directory.Exists(journal.RollbackPath))
				{
					if (Directory.Exists(journal.InstallPath))
						Directory.Delete(journal.InstallPath, true);
					Directory.Move(journal.RollbackPath, journal.InstallPath);
					recovered++;
				}
				else if (!Directory.Exists(journal.InstallPath) &&
					!string.Equals(journal.Phase, "Prepared", StringComparison.Ordinal))
				{
					throw new IOException("An interrupted server restore is missing both its active and rollback folders.");
				}

				if (Directory.Exists(journal.OperationRoot))
					Directory.Delete(journal.OperationRoot, true);
				File.Delete(journalPath);
			}

			return recovered;
		}

		private ServerBackupRestoreResult RestoreServerBackup(
			GameServer server,
			ServerBackupArchive backup,
			IProgress<string>? progress)
		{
			string installPath = ValidateInstallPath(server.InstallPath);
			if (IsSameOrDescendantPath(backup.ArchivePath, installPath))
				throw new InvalidOperationException("A backup stored inside the server folder cannot be restored safely. Move the backup folder outside the server installation first.");
			string parentPath = Path.GetDirectoryName(installPath)!;
			Directory.CreateDirectory(parentPath);

			string operationId = Guid.NewGuid().ToString("N");
			string operationRoot = Path.Combine(
				parentPath,
				$"{RestoreOperationPrefix}{GetSafeName(server.ServerName)}-{operationId}");
			string extractionRoot = Path.Combine(operationRoot, "extracted");
			string preparedRoot = Path.Combine(operationRoot, "prepared");
			string rollbackPath = installPath + RestoreRollbackMarker + operationId;
			string journalPath = Path.Combine(RestoreJournalFolder, operationId + ".json");

			long restoredBytes;
			RestoreJournal journal;
			try
			{
				Directory.CreateDirectory(extractionRoot);
				progress?.Report("Checking the selected backup...");
				restoredBytes = ExtractBackupSafely(
					backup.ArchivePath,
					extractionRoot,
					installPath,
					progress);
				PrepareExtractedFolder(extractionRoot, preparedRoot);

				journal = new RestoreJournal
				{
					InstallPath = installPath,
					OperationRoot = operationRoot,
					RollbackPath = rollbackPath,
					Phase = "Prepared"
				};
				WriteRestoreJournal(journalPath, journal);
			}
			catch
			{
				TryDeleteBackupRestoreDirectory(operationRoot);
				TryDeleteBackupRestoreFile(journalPath);
				throw;
			}

			bool originalMoved = false;
			try
			{
				progress?.Report("Preserving the current server files...");
				if (Directory.Exists(installPath))
				{
					Directory.Move(installPath, rollbackPath);
					originalMoved = true;
					journal.Phase = "OriginalPreserved";
					WriteRestoreJournal(journalPath, journal);
				}

				progress?.Report("Activating the selected backup...");
				journal.Phase = "Activating";
				WriteRestoreJournal(journalPath, journal);
				Directory.Move(preparedRoot, installPath);
				journal.Phase = "Restored";
				WriteRestoreJournal(journalPath, journal);
			}
			catch (Exception activationException)
			{
				Exception? rollbackException = null;
				if (originalMoved && Directory.Exists(rollbackPath))
				{
					try
					{
						if (Directory.Exists(installPath))
							Directory.Delete(installPath, true);
						Directory.Move(rollbackPath, installPath);
					}
					catch (Exception exception)
					{
						rollbackException = exception;
					}
				}
				else if (!originalMoved && Directory.Exists(installPath))
				{
					try
					{
						Directory.Delete(installPath, true);
					}
					catch (Exception exception)
					{
						rollbackException = exception;
					}
				}

				if (rollbackException == null)
				{
					TryDeleteBackupRestoreDirectory(operationRoot);
					TryDeleteBackupRestoreFile(journalPath);
					throw;
				}

				throw new AggregateException(
					"The restore failed and Synix could not automatically return the previous server folder. Restart Synix so recovery can try again.",
					activationException,
					rollbackException);
			}

			progress?.Report("Cleaning up the previous server files...");
			bool cleanupComplete = true;
			try
			{
				if (Directory.Exists(rollbackPath))
					Directory.Delete(rollbackPath, true);
				if (Directory.Exists(operationRoot))
					Directory.Delete(operationRoot, true);
				File.Delete(journalPath);
			}
			catch
			{
				cleanupComplete = false;
			}

			progress?.Report("Restore complete.");
			return new ServerBackupRestoreResult(
				true,
				cleanupComplete
					? "The selected backup was restored successfully."
					: "The backup was restored, but temporary cleanup will finish the next time Synix starts.",
				restoredBytes);
		}

		private static long ExtractBackupSafely(
			string archivePath,
			string extractionRoot,
			string installPath,
			IProgress<string>? progress)
		{
			using ZipArchive archive = ZipFile.OpenRead(archivePath);
			if (archive.Entries.Count == 0)
				throw new InvalidDataException("The selected backup is empty.");

			long totalBytes = 0;
			foreach (ZipArchiveEntry entry in archive.Entries)
			{
				if (IsSymbolicLink(entry))
					throw new InvalidDataException("The backup contains an unsupported symbolic link.");
				totalBytes = checked(totalBytes + entry.Length);
			}

			EnsureRestoreDiskSpace(installPath, totalBytes);
			string safeRoot = Path.GetFullPath(extractionRoot)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
				Path.DirectorySeparatorChar;
			long copiedBytes = 0;
			int copiedFiles = 0;
			int totalFiles = archive.Entries.Count(entry => !string.IsNullOrEmpty(entry.Name));

			foreach (ZipArchiveEntry entry in archive.Entries)
			{
				if (Path.IsPathRooted(entry.FullName))
					throw new InvalidDataException("The backup contains an unsafe absolute path.");
				if (entry.FullName.Split('/', '\\').Any(segment => segment.Contains(':')))
					throw new InvalidDataException("The backup contains an unsupported alternate file stream path.");

				string destinationPath = Path.GetFullPath(Path.Combine(extractionRoot, entry.FullName));
				if (!destinationPath.StartsWith(safeRoot, StringComparison.OrdinalIgnoreCase))
					throw new InvalidDataException("The backup contains an unsafe path.");

				if (string.IsNullOrEmpty(entry.Name))
				{
					Directory.CreateDirectory(destinationPath);
					continue;
				}

				Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
				using (Stream source = entry.Open())
				using (FileStream destination = new(
					destinationPath,
					FileMode.CreateNew,
					FileAccess.Write,
					FileShare.None))
				{
					source.CopyTo(destination);
				}
				File.SetLastWriteTime(destinationPath, entry.LastWriteTime.LocalDateTime);
				copiedBytes = checked(copiedBytes + entry.Length);
				copiedFiles++;
				progress?.Report($"Unpacking backup file {copiedFiles} of {Math.Max(1, totalFiles)}...");
			}

			if (copiedFiles == 0)
				throw new InvalidDataException("The selected backup contains no server files.");
			return copiedBytes;
		}

		private static void PrepareExtractedFolder(
			string extractionRoot,
			string preparedRoot)
		{
			string[] entries = Directory.GetFileSystemEntries(extractionRoot);
			if (entries.Length == 1 && Directory.Exists(entries[0]))
			{
				Directory.Move(entries[0], preparedRoot);
				Directory.Delete(extractionRoot);
				return;
			}

			Directory.Move(extractionRoot, preparedRoot);
		}

		private static void EnsureRestoreDiskSpace(string installPath, long requiredBytes)
		{
			string root = Path.GetPathRoot(installPath) ??
				throw new InvalidOperationException("Synix could not determine the server drive.");
			DriveInfo drive = new(root);
			long safetyMargin = 64L * 1024 * 1024;
			long requiredWithMargin = checked(requiredBytes + safetyMargin);
			if (drive.AvailableFreeSpace < requiredWithMargin)
			{
				throw new IOException(
					$"The server drive needs at least {FormatBytes(requiredWithMargin)} free to stage this restore, but only {FormatBytes(drive.AvailableFreeSpace)} is available.");
			}
		}

		private static string ValidateInstallPath(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				throw new InvalidOperationException("The server installation path is empty.");

			string fullPath = Path.GetFullPath(path)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string? parent = Path.GetDirectoryName(fullPath);
			if (string.IsNullOrWhiteSpace(parent) ||
				string.Equals(fullPath, Path.GetPathRoot(fullPath), StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException("Synix refused to restore into a drive root.");
			}
			return fullPath;
		}

		private static bool IsSameOrDescendantPath(string path, string parentPath)
		{
			string fullPath = Path.GetFullPath(path)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string fullParent = Path.GetFullPath(parentPath)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			return string.Equals(fullPath, fullParent, StringComparison.OrdinalIgnoreCase) ||
				fullPath.StartsWith(
					fullParent + Path.DirectorySeparatorChar,
					StringComparison.OrdinalIgnoreCase);
		}

		private string GetActiveBackupBaseFolder()
		{
			if (Properties.Settings.Default.UseCustomBackupPath &&
				!string.IsNullOrWhiteSpace(Properties.Settings.Default.CustomBackupPath) &&
				Directory.Exists(Properties.Settings.Default.CustomBackupPath))
			{
				return Path.GetFullPath(Properties.Settings.Default.CustomBackupPath);
			}

			return DefaultBackupPath;
		}

		private static bool IsSymbolicLink(ZipArchiveEntry entry)
		{
			int unixFileType = (entry.ExternalAttributes >> 16) & 0xF000;
			return unixFileType == 0xA000;
		}

		private static string FormatBytes(long bytes)
		{
			string[] units = ["B", "KB", "MB", "GB", "TB"];
			double value = Math.Max(0, bytes);
			int unit = 0;
			while (value >= 1024 && unit < units.Length - 1)
			{
				value /= 1024;
				unit++;
			}
			return $"{value:0.##} {units[unit]}";
		}

		private static void WriteRestoreJournal(
			string journalPath,
			RestoreJournal journal)
		{
			Directory.CreateDirectory(RestoreJournalFolder);
			string temporaryPath = journalPath + ".tmp";
			File.WriteAllText(
				temporaryPath,
				JsonSerializer.Serialize(journal));
			File.Move(temporaryPath, journalPath, true);
		}

		private static void ValidateRestoreJournal(RestoreJournal journal)
		{
			string installPath = ValidateInstallPath(journal.InstallPath);
			string parentPath = Path.GetDirectoryName(installPath)!;
			string operationRoot = Path.GetFullPath(journal.OperationRoot);
			string rollbackPath = Path.GetFullPath(journal.RollbackPath);

			if (!string.Equals(Path.GetDirectoryName(operationRoot), parentPath, StringComparison.OrdinalIgnoreCase) ||
				!Path.GetFileName(operationRoot).StartsWith(RestoreOperationPrefix, StringComparison.OrdinalIgnoreCase) ||
				!string.Equals(Path.GetDirectoryName(rollbackPath), parentPath, StringComparison.OrdinalIgnoreCase) ||
				!Path.GetFileName(rollbackPath).StartsWith(
					Path.GetFileName(installPath) + RestoreRollbackMarker,
					StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException("A server restore recovery record contains unsafe paths.");
			}
		}

		private static void TryDeleteBackupRestoreDirectory(string path)
		{
			try
			{
				if (Directory.Exists(path))
					Directory.Delete(path, true);
			}
			catch
			{
			}
		}

		private static void TryDeleteBackupRestoreFile(string path)
		{
			try
			{
				if (File.Exists(path))
					File.Delete(path);
			}
			catch
			{
			}
		}

		public string GetSafeName(string name)
		{
			if (string.IsNullOrWhiteSpace(name)) return "Unknown";
			string cleanName = name.Replace(" ", "_").Replace(":", "_");

			foreach (char character in Path.GetInvalidFileNameChars())
			{
				cleanName = cleanName.Replace(character.ToString(), "");
			}

			return cleanName;
		}

		private sealed class RestoreJournal
		{
			public string InstallPath { get; set; } = string.Empty;
			public string OperationRoot { get; set; } = string.Empty;
			public string RollbackPath { get; set; } = string.Empty;
			public string Phase { get; set; } = string.Empty;
		}
	}
}
