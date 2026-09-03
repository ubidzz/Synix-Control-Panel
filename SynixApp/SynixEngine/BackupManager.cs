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
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Synix_Control_Panel.SynixEngine
{
	public enum StartContext { Manual, Scheduled, CrashRecovery }

	internal enum ServerBackupIntegrity
	{
		Recorded,
		Legacy,
		Invalid
	}

	internal sealed record ServerBackupArchive(
		string ArchivePath,
		DateTime CreatedUtc,
		long CompressedBytes,
		long UncompressedBytes,
		ServerBackupIntegrity Integrity,
		DateTime? LastVerifiedUtc)
	{
		public string FileName => Path.GetFileName(ArchivePath);
		public DateTime CreatedLocal => CreatedUtc.ToLocalTime();
		public DateTime? LastVerifiedLocal => LastVerifiedUtc?.ToLocalTime();
		public string IntegrityText => Integrity switch
		{
			ServerBackupIntegrity.Recorded => "SHA-256 receipt",
			ServerBackupIntegrity.Legacy => "Legacy backup",
			_ => "Invalid receipt"
		};
	}

	internal sealed record ServerBackupPreflight(
		bool Succeeded,
		string Message,
		string BackupFolder,
		long SourceBytes,
		long FileCount,
		long RequiredBytes,
		long AvailableBytes)
	{
		public bool HasEnoughSpace => Succeeded && AvailableBytes >= RequiredBytes;
	}

	internal sealed record ServerBackupManagementResult(
		bool Succeeded,
		string Message);

	internal sealed record ServerBackupRestoreResult(
		bool Succeeded,
		string Message,
		long RestoredBytes = 0);

	public partial class Core
	{
		private const string RestoreOperationPrefix = ".synix-restore-";
		private const string RestoreRollbackMarker = ".synix-restore-rollback-";
		private const string BackupReceiptExtension = ".sha256";
		private const int MaximumReceiptBytes = 4096;
		private const long BackupSpaceSafetyMargin = 256L * 1024 * 1024;
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
			using ServerOperationLease operation =
				ServerOperationCoordinator.TryBegin(server, ServerOperationKind.Backup);
			if (!operation.Acquired)
			{
				Log($"[BACKUP BLOCKED] {operation.FailureReason}", Color.Orange, true);
				return;
			}

			if (server.Status != StatusManager.GetStatus(ServerState.Stopped))
			{
				Log($"[🚨 ERROR] {server.ServerName} must be Stopped to perform a backup.", Color.Orange);
				_ = SendDiscordNotification(
					server,
					DiscordNotificationEvent.BackupFailed,
					"BACKUP BLOCKED",
					"The server must be stopped before a backup can begin.",
					Color.Orange);
				return;
			}

			ServerBackupPreflight preflight = await CreateServerBackupPreflightAsync(server);
			if (!preflight.Succeeded)
			{
				Log($"[🚨 BACKUP ERROR] {preflight.Message}", Color.Red, true);
				_ = SendDiscordNotification(
					server,
					DiscordNotificationEvent.BackupFailed,
					"BACKUP FAILED",
					preflight.Message,
					Color.Red);
				return;
			}
			if (!preflight.HasEnoughSpace)
			{
				Log(
					$"[🚨 BACKUP ERROR] The backup drive needs up to {FormatBytes(preflight.RequiredBytes)} free, but only {FormatBytes(preflight.AvailableBytes)} is available.",
					Color.Red,
					true);
				_ = SendDiscordNotification(
					server,
					DiscordNotificationEvent.BackupFailed,
					"BACKUP FAILED",
					$"Not enough free space. Up to {FormatBytes(preflight.RequiredBytes)} is needed, but {FormatBytes(preflight.AvailableBytes)} is available.",
					Color.Red);
				return;
			}

			Log("[⚠ WARNING] Synix close window button is now Disabled!", Color.Orange, true);
			isDownloadActive = true;
			Log($"[💾 BACKUP] Starting backup compression for {server.ServerName}...", Color.Cyan);
			_ = SendDiscordNotification(
				server,
				DiscordNotificationEvent.BackupStarted,
				"BACKUP STARTED",
				$"Synix is backing up {FormatBytes(preflight.SourceBytes)} across {preflight.FileCount:N0} files.",
				Color.Cyan);

			server.Status = StatusManager.GetStatus(ServerState.BackingUp);
			UpdateGridStatus();

			string sourceDir = server.InstallPath;
			string backupRoot = GetActiveServerBackupFolder(server);
			string timestamp = DateTime.UtcNow.ToString("yyyy_MM_dd_HHmmss_fff");
			string zipPath = Path.Combine(backupRoot, $"backup_{timestamp}.zip");
			string temporaryZipPath = zipPath + ".partial";
			string receiptPath = GetBackupReceiptPath(zipPath);
			string temporaryReceiptPath = receiptPath + ".partial";
			bool receiptPublished = false;
			bool backupPublished = false;

			try
			{
				int maximumBackups = Math.Max(1, Properties.Settings.Default.MaxBackups);
				await Task.Run(() =>
				{
					Directory.CreateDirectory(backupRoot);
					CleanupIncompleteBackupFiles(backupRoot);

					if (!Directory.Exists(sourceDir))
						throw new DirectoryNotFoundException($"The server folder does not exist: {sourceDir}");
					if (IsSameOrDescendantPath(zipPath, sourceDir))
						throw new InvalidOperationException("The backup folder cannot be stored inside the server installation folder.");

					ZipFile.CreateFromDirectory(
						sourceDirectoryName: sourceDir,
						destinationArchiveFileName: temporaryZipPath,
						compressionLevel: CompressionLevel.Optimal,
						includeBaseDirectory: true);
					string hash = ComputeFileSha256(temporaryZipPath);
					WriteBackupReceipt(
						temporaryReceiptPath,
						hash,
						Path.GetFileName(zipPath),
						preflight.SourceBytes,
						DateTimeOffset.UtcNow,
						DateTimeOffset.UtcNow);
					File.Move(temporaryReceiptPath, receiptPath);
					receiptPublished = true;
					File.Move(temporaryZipPath, zipPath);
					backupPublished = true;

					List<FileInfo> files = new DirectoryInfo(backupRoot)
						.GetFiles("*.zip")
						.OrderByDescending(file => file.LastWriteTimeUtc)
						.ToList();
					while (files.Count > maximumBackups)
					{
						FileInfo expiredBackup = files.Last();
						expiredBackup.Delete();
						TryDeleteBackupRestoreFile(GetBackupReceiptPath(expiredBackup.FullName));
						files.RemoveAt(files.Count - 1);
					}
				});

				Log($"[💾 BACKUP] SHA-256 integrity receipt created for {Path.GetFileName(zipPath)}.", Color.LimeGreen);
				Log($"[💾 BACKUP] Backup location: {zipPath}.", Color.LimeGreen);
				Log($"[💾 BACKUP] Finished backing up {server.ServerName}.", Color.LimeGreen);
				_ = SendDiscordNotification(
					server,
					DiscordNotificationEvent.BackupCompleted,
					"BACKUP COMPLETED",
					$"Created {Path.GetFileName(zipPath)} with a SHA-256 integrity receipt.",
					Color.LimeGreen);
			}
			catch (Exception exception)
			{
				TryDeleteBackupRestoreFile(temporaryZipPath);
				TryDeleteBackupRestoreFile(temporaryReceiptPath);
				if (backupPublished)
					TryDeleteBackupRestoreFile(zipPath);
				if (receiptPublished)
					TryDeleteBackupRestoreFile(receiptPath);
				Log($"[🚨 BACKUP ERROR] {exception.Message}", Color.Red, true);
				_ = SendDiscordNotification(
					server,
					DiscordNotificationEvent.BackupFailed,
					"BACKUP FAILED",
					exception.Message,
					Color.Red);
			}
			finally
			{
				server.Status = StatusManager.GetStatus(ServerState.Stopped);
				isDownloadActive = false;
				Log("[⚠ WARNING] Synix close window button is now Enabled!", Color.Orange, true);
				UpdateGridStatus();
			}
		}

		internal Task<ServerBackupPreflight> CreateServerBackupPreflightAsync(
			GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);
			return Task.Run(() => CreateServerBackupPreflight(server));
		}

		private ServerBackupPreflight CreateServerBackupPreflight(GameServer server)
		{
			string sourcePath;
			string backupFolder;
			try
			{
				sourcePath = Path.GetFullPath(server.InstallPath);
				backupFolder = Path.GetFullPath(GetActiveServerBackupFolder(server));
				if (!Directory.Exists(sourcePath))
				{
					return new ServerBackupPreflight(
						false,
						$"The server folder does not exist: {sourcePath}",
						backupFolder,
						0,
						0,
						0,
						0);
				}
				if (IsSameOrDescendantPath(backupFolder, sourcePath))
				{
					return new ServerBackupPreflight(
						false,
						"The backup folder cannot be stored inside the server installation folder.",
						backupFolder,
						0,
						0,
						0,
						0);
				}

				long sourceBytes = 0;
				long fileCount = 0;
				foreach (string filePath in Directory.EnumerateFiles(
					sourcePath,
					"*",
					SearchOption.AllDirectories))
				{
					sourceBytes = checked(sourceBytes + new FileInfo(filePath).Length);
					fileCount++;
				}

				if (fileCount == 0)
				{
					return new ServerBackupPreflight(
						false,
						"The server folder contains no files to back up.",
						backupFolder,
						0,
						0,
						0,
						0);
				}

				long requiredBytes = sourceBytes >= long.MaxValue - BackupSpaceSafetyMargin
					? long.MaxValue
					: sourceBytes + BackupSpaceSafetyMargin;
				long availableBytes = GetAvailableFreeSpace(GetVolumeRoot(backupFolder));
				return new ServerBackupPreflight(
					true,
					string.Empty,
					backupFolder,
					sourceBytes,
					fileCount,
					requiredBytes,
					availableBytes);
			}
			catch (Exception exception) when (exception is IOException or
				UnauthorizedAccessException or
				ArgumentException or
				NotSupportedException or
				OverflowException)
			{
				return new ServerBackupPreflight(
					false,
					$"Synix could not measure the server backup: {exception.Message}",
					string.Empty,
					0,
					0,
					0,
					0);
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
						ServerBackupIntegrity integrity = InspectBackupIntegrity(
							file.FullName,
							out BackupReceipt? receipt);
						long uncompressedBytes = receipt?.UncompressedBytes ??
							TryReadArchiveUncompressedBytes(file.FullName);
						backups.Add(new ServerBackupArchive(
							file.FullName,
							file.LastWriteTimeUtc,
							file.Length,
							uncompressedBytes,
							integrity,
							receipt?.VerifiedUtc?.UtcDateTime));
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

		internal Task<IReadOnlyList<ServerBackupArchive>> GetServerBackupsAsync(
			GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);
			return Task.Run(() => GetServerBackups(server));
		}

		internal bool HasServerBackups(GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);
			HashSet<string> folders = new(StringComparer.OrdinalIgnoreCase)
			{
				GetActiveServerBackupFolder(server),
				Path.Combine(DefaultBackupPath, GetSafeName(server.Game), GetSafeName(server.ServerName))
			};

			foreach (string folder in folders)
			{
				try
				{
					if (Directory.Exists(folder) &&
						Directory.EnumerateFiles(folder, "*.zip", SearchOption.TopDirectoryOnly).Any())
					{
						return true;
					}
				}
				catch (IOException)
				{
				}
				catch (UnauthorizedAccessException)
				{
				}
			}

			return false;
		}

		internal Task<bool> HasServerBackupsAsync(GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);
			return Task.Run(() => HasServerBackups(server));
		}

		internal async Task<ServerBackupManagementResult> VerifyServerBackupAsync(
			GameServer server,
			ServerBackupArchive backup)
		{
			ArgumentNullException.ThrowIfNull(server);
			ArgumentNullException.ThrowIfNull(backup);
			using ServerOperationLease operation =
				ServerOperationCoordinator.TryBegin(server, ServerOperationKind.Backup);
			if (!operation.Acquired)
				return new ServerBackupManagementResult(false, operation.FailureReason);
			if (!await Task.Run(() => IsKnownServerBackup(server, backup.ArchivePath)))
			{
				return new ServerBackupManagementResult(
					false,
					"The selected backup is missing or is not stored in this server's backup folder.");
			}
			if (backup.Integrity == ServerBackupIntegrity.Invalid)
			{
				return new ServerBackupManagementResult(
					false,
					"The selected backup has an invalid integrity receipt. Synix will not replace that receipt or trust the archive.");
			}

			try
			{
				await Task.Run(() =>
				{
					long uncompressedBytes = ValidateBackupArchive(backup.ArchivePath);
					string hash = ComputeFileSha256(backup.ArchivePath);
					if (backup.Integrity == ServerBackupIntegrity.Recorded)
					{
						BackupReceipt receipt = ReadRequiredBackupReceipt(backup.ArchivePath);
						EnsureMatchingBackupHash(receipt.Hash, hash);
					}

					DateTimeOffset now = DateTimeOffset.UtcNow;
					WriteBackupReceiptAtomically(
						backup.ArchivePath,
						hash,
						uncompressedBytes,
						new DateTimeOffset(File.GetLastWriteTimeUtc(backup.ArchivePath), TimeSpan.Zero),
						now);
				});
				return new ServerBackupManagementResult(
					true,
					"The backup archive, safe paths, and SHA-256 integrity check passed.");
			}
			catch (Exception exception)
			{
				return new ServerBackupManagementResult(false, exception.Message);
			}
		}

		internal ServerBackupManagementResult DeleteServerBackup(
			GameServer server,
			ServerBackupArchive backup)
		{
			ArgumentNullException.ThrowIfNull(server);
			ArgumentNullException.ThrowIfNull(backup);
			using ServerOperationLease operation =
				ServerOperationCoordinator.TryBegin(server, ServerOperationKind.Backup);
			if (!operation.Acquired)
				return new ServerBackupManagementResult(false, operation.FailureReason);
			if (!IsKnownServerBackup(server, backup.ArchivePath))
			{
				return new ServerBackupManagementResult(
					false,
					"The selected backup is missing or is not stored in this server's backup folder.");
			}

			try
			{
				File.Delete(backup.ArchivePath);
				TryDeleteBackupRestoreFile(GetBackupReceiptPath(backup.ArchivePath));
				return new ServerBackupManagementResult(true, "The selected backup was deleted.");
			}
			catch (Exception exception) when (exception is IOException or
				UnauthorizedAccessException)
			{
				return new ServerBackupManagementResult(
					false,
					$"Synix could not delete the selected backup: {exception.Message}");
			}
		}

		internal Task<ServerBackupManagementResult> DeleteServerBackupAsync(
			GameServer server,
			ServerBackupArchive backup)
		{
			ArgumentNullException.ThrowIfNull(server);
			ArgumentNullException.ThrowIfNull(backup);
			return Task.Run(() => DeleteServerBackup(server, backup));
		}

		internal async Task<ServerBackupRestoreResult> RestoreServerBackupAsync(
			GameServer server,
			ServerBackupArchive backup,
			IProgress<string>? progress = null)
		{
			ArgumentNullException.ThrowIfNull(server);
			ArgumentNullException.ThrowIfNull(backup);
			using ServerOperationLease operation =
				ServerOperationCoordinator.TryBegin(server, ServerOperationKind.Restore);
			if (!operation.Acquired)
				return new ServerBackupRestoreResult(false, operation.FailureReason);

			if (server.Status != StatusManager.GetStatus(ServerState.Stopped) ||
				server.PID.HasValue)
			{
				return new ServerBackupRestoreResult(
					false,
					"Stop the server before restoring a backup.");
			}

			string selectedPath = Path.GetFullPath(backup.ArchivePath);
			if (!await Task.Run(() => IsKnownServerBackup(server, selectedPath)))
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
			_ = SendDiscordNotification(
				server,
				DiscordNotificationEvent.RestoreStarted,
				"RESTORE STARTED",
				$"Synix is restoring {backup.FileName}.",
				Color.Cyan);

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
				_ = SendDiscordNotification(
					server,
					result.Succeeded
						? DiscordNotificationEvent.RestoreCompleted
						: DiscordNotificationEvent.RestoreFailed,
					result.Succeeded ? "RESTORE COMPLETED" : "RESTORE FAILED",
					result.Succeeded
						? $"Restored {backup.FileName} successfully."
						: result.Message,
					result.Succeeded ? Color.LimeGreen : Color.Red);
				return result;
			}
			catch (Exception exception)
			{
				Log($"[🚨 RESTORE ERROR] {exception.Message}", Color.Red, true);
				_ = SendDiscordNotification(
					server,
					DiscordNotificationEvent.RestoreFailed,
					"RESTORE FAILED",
					exception.Message,
					Color.Red);
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
			progress?.Report("Verifying the selected backup...");
			VerifyBackupIntegrity(backup.ArchivePath);
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

		private static ServerBackupIntegrity InspectBackupIntegrity(
			string archivePath,
			out BackupReceipt? receipt)
		{
			receipt = null;
			string receiptPath = GetBackupReceiptPath(archivePath);
			if (!File.Exists(receiptPath))
				return ServerBackupIntegrity.Legacy;

			return TryReadBackupReceipt(
				receiptPath,
				Path.GetFileName(archivePath),
				out receipt)
				? ServerBackupIntegrity.Recorded
				: ServerBackupIntegrity.Invalid;
		}

		private bool IsKnownServerBackup(GameServer server, string archivePath)
		{
			string selectedPath = Path.GetFullPath(archivePath);
			return File.Exists(selectedPath) && GetServerBackups(server).Any(candidate =>
				string.Equals(
					Path.GetFullPath(candidate.ArchivePath),
					selectedPath,
					StringComparison.OrdinalIgnoreCase));
		}

		private static void CleanupIncompleteBackupFiles(string backupRoot)
		{
			foreach (string pattern in new[]
			{
				"backup_*.zip.partial",
				"backup_*.zip.sha256.partial"
			})
			{
				foreach (string path in Directory.EnumerateFiles(
					backupRoot,
					pattern,
					SearchOption.TopDirectoryOnly))
				{
					TryDeleteBackupRestoreFile(path);
				}
			}

			foreach (string receiptPath in Directory.EnumerateFiles(
				backupRoot,
				"backup_*.zip.sha256",
				SearchOption.TopDirectoryOnly))
			{
				string archivePath = receiptPath[..^BackupReceiptExtension.Length];
				if (!File.Exists(archivePath))
					TryDeleteBackupRestoreFile(receiptPath);
			}
		}

		private static void VerifyBackupIntegrity(string archivePath)
		{
			string receiptPath = GetBackupReceiptPath(archivePath);
			if (!File.Exists(receiptPath))
				return;

			if (!TryReadBackupReceipt(
				receiptPath,
				Path.GetFileName(archivePath),
				out BackupReceipt? receipt) || receipt == null)
			{
				throw new InvalidDataException(
					"The selected backup has an invalid SHA-256 integrity receipt.");
			}

			string actualHash = ComputeFileSha256(archivePath);
			EnsureMatchingBackupHash(receipt.Hash, actualHash);
			WriteBackupReceiptAtomically(
				archivePath,
				actualHash,
				receipt.UncompressedBytes > 0
					? receipt.UncompressedBytes
					: TryReadArchiveUncompressedBytes(archivePath),
				receipt.CreatedUtc ?? new DateTimeOffset(
					File.GetLastWriteTimeUtc(archivePath),
					TimeSpan.Zero),
				DateTimeOffset.UtcNow);
		}

		private static void EnsureMatchingBackupHash(
			string expectedHash,
			string actualHash)
		{
			byte[] expectedBytes = Convert.FromHexString(expectedHash);
			byte[] actualBytes = Convert.FromHexString(actualHash);
			if (!CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes))
			{
				throw new InvalidDataException(
					"The selected backup failed its SHA-256 integrity check. The archive may be damaged or may have changed after Synix created it.");
			}
		}

		private static string ComputeFileSha256(string path)
		{
			using FileStream stream = new(
				path,
				FileMode.Open,
				FileAccess.Read,
				FileShare.Read,
				bufferSize: 1024 * 1024,
				FileOptions.SequentialScan);
			return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
		}

		private static void WriteBackupReceipt(
			string path,
			string hash,
			string archiveFileName,
			long uncompressedBytes,
			DateTimeOffset createdUtc,
			DateTimeOffset verifiedUtc)
		{
			File.WriteAllText(
				path,
				$"{hash}  {archiveFileName}{Environment.NewLine}" +
				$"UncompressedBytes={Math.Max(0, uncompressedBytes)}{Environment.NewLine}" +
				$"CreatedUtc={createdUtc:O}{Environment.NewLine}" +
				$"VerifiedUtc={verifiedUtc:O}{Environment.NewLine}",
				new UTF8Encoding(false));
		}

		private static void WriteBackupReceiptAtomically(
			string archivePath,
			string hash,
			long uncompressedBytes,
			DateTimeOffset createdUtc,
			DateTimeOffset verifiedUtc)
		{
			string receiptPath = GetBackupReceiptPath(archivePath);
			string temporaryPath = receiptPath + ".partial";
			try
			{
				WriteBackupReceipt(
					temporaryPath,
					hash,
					Path.GetFileName(archivePath),
					uncompressedBytes,
					createdUtc,
					verifiedUtc);
				File.Move(temporaryPath, receiptPath, true);
			}
			finally
			{
				TryDeleteBackupRestoreFile(temporaryPath);
			}
		}

		private static bool TryReadBackupReceipt(
			string receiptPath,
			string archiveFileName,
			out BackupReceipt? receiptData)
		{
			receiptData = null;
			try
			{
				FileInfo receipt = new(receiptPath);
				if (receipt.Length is <= 0 or > MaximumReceiptBytes)
					return false;

				string[] lines = File.ReadAllLines(receiptPath);
				string line = lines.FirstOrDefault()?.Trim() ?? string.Empty;
				int separator = line.IndexOf("  ", StringComparison.Ordinal);
				if (separator != 64)
					return false;

				string candidateHash = line[..separator];
				string candidateFileName = line[(separator + 2)..].TrimStart('*');
				if (!string.Equals(
					candidateFileName,
					archiveFileName,
					StringComparison.OrdinalIgnoreCase) ||
					candidateHash.Any(character => !Uri.IsHexDigit(character)))
				{
					return false;
				}

				long uncompressedBytes = 0;
				DateTimeOffset? createdUtc = null;
				DateTimeOffset? verifiedUtc = null;
				foreach (string metadataLine in lines.Skip(1))
				{
					int equalsIndex = metadataLine.IndexOf('=');
					if (equalsIndex <= 0)
						continue;
					string key = metadataLine[..equalsIndex].Trim();
					string value = metadataLine[(equalsIndex + 1)..].Trim();
					if (key.Equals("UncompressedBytes", StringComparison.OrdinalIgnoreCase) &&
						long.TryParse(value, out long parsedBytes) &&
						parsedBytes >= 0)
					{
						uncompressedBytes = parsedBytes;
					}
					else if (key.Equals("CreatedUtc", StringComparison.OrdinalIgnoreCase) &&
						DateTimeOffset.TryParse(value, out DateTimeOffset parsedCreated))
					{
						createdUtc = parsedCreated.ToUniversalTime();
					}
					else if (key.Equals("VerifiedUtc", StringComparison.OrdinalIgnoreCase) &&
						DateTimeOffset.TryParse(value, out DateTimeOffset parsedVerified))
					{
						verifiedUtc = parsedVerified.ToUniversalTime();
					}
				}

				receiptData = new BackupReceipt(
					candidateHash.ToLowerInvariant(),
					uncompressedBytes,
					createdUtc,
					verifiedUtc);
				return true;
			}
			catch (Exception exception) when (exception is IOException or
				UnauthorizedAccessException or
				NotSupportedException)
			{
				return false;
			}
		}

		private static BackupReceipt ReadRequiredBackupReceipt(string archivePath)
		{
			if (!TryReadBackupReceipt(
				GetBackupReceiptPath(archivePath),
				Path.GetFileName(archivePath),
				out BackupReceipt? receipt) || receipt == null)
			{
				throw new InvalidDataException(
					"The selected backup has an invalid SHA-256 integrity receipt.");
			}
			return receipt;
		}

		private static long TryReadArchiveUncompressedBytes(string archivePath)
		{
			try
			{
				using ZipArchive archive = ZipFile.OpenRead(archivePath);
				long totalBytes = 0;
				foreach (ZipArchiveEntry entry in archive.Entries)
					totalBytes = checked(totalBytes + entry.Length);
				return totalBytes;
			}
			catch
			{
				return 0;
			}
		}

		private static long ValidateBackupArchive(string archivePath)
		{
			using ZipArchive archive = ZipFile.OpenRead(archivePath);
			if (archive.Entries.Count == 0)
				throw new InvalidDataException("The selected backup is empty.");

			string validationRoot = Path.Combine(
				Path.GetTempPath(),
				"SynixBackupValidation",
				Guid.NewGuid().ToString("N"));
			string safeRoot = Path.GetFullPath(validationRoot)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
				Path.DirectorySeparatorChar;
			long totalBytes = 0;
			int fileCount = 0;
			foreach (ZipArchiveEntry entry in archive.Entries)
			{
				if (IsSymbolicLink(entry) || Path.IsPathRooted(entry.FullName))
					throw new InvalidDataException("The backup contains an unsafe path or symbolic link.");
				if (entry.FullName.Split('/', '\\').Any(segment => segment.Contains(':')))
					throw new InvalidDataException("The backup contains an unsupported alternate file stream path.");

				string destinationPath = Path.GetFullPath(Path.Combine(validationRoot, entry.FullName));
				if (!destinationPath.StartsWith(safeRoot, StringComparison.OrdinalIgnoreCase))
					throw new InvalidDataException("The backup contains an unsafe path.");
				totalBytes = checked(totalBytes + entry.Length);
				if (!string.IsNullOrEmpty(entry.Name))
				{
					fileCount++;
					using Stream entryStream = entry.Open();
					entryStream.CopyTo(Stream.Null);
				}
			}

			if (fileCount == 0)
				throw new InvalidDataException("The selected backup contains no server files.");
			return totalBytes;
		}

		private static string GetBackupReceiptPath(string archivePath) =>
			archivePath + BackupReceiptExtension;

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

		internal static string FormatBytes(long bytes)
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

		private sealed record BackupReceipt(
			string Hash,
			long UncompressedBytes,
			DateTimeOffset? CreatedUtc,
			DateTimeOffset? VerifiedUtc);
	}
}
