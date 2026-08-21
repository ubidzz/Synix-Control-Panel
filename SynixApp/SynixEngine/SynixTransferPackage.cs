// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Synix_Control_Panel.SynixEngine
{
	public sealed record SynixTransferProgress(
		string Message,
		int Percent);

	/// <summary>
	/// Creates and restores portable, password-protected copies of the Synix
	/// data folder. The encrypted file is processed in chunks so large game
	/// installations are never loaded into memory at once.
	/// </summary>
	public static class SynixTransferPackage
	{
		private static readonly byte[] Magic =
			Encoding.ASCII.GetBytes("SYNIXPKG");

		private const int FormatVersion = 1;
		private const int SaltSize = 16;
		private const int NonceSize = 12;
		private const int TagSize = 16;
		private const int KeySize = 32;
		private const int ChunkSize = 1024 * 1024;
		private const int Pbkdf2Iterations = 600_000;
		private const int RecoveryJournalVersion = 1;
		private const string RecoveryFolderName = ".synix-transfer-recovery";
		private const string RecoveryJournalFileName = "journal.json";
		private const string RecoveryStatePrepared = "Prepared";
		private const string RecoveryStateCommitting = "Committing";
		private const string RecoveryStateCommitted = "Committed";

		private sealed class ImportRecoveryJournal
		{
			public int Version { get; set; } = RecoveryJournalVersion;
			public string DestinationRoot { get; set; } = string.Empty;
			public string OperationId { get; set; } = string.Empty;
			public string State { get; set; } = RecoveryStatePrepared;
			public List<ImportRecoveryEntry> Entries { get; set; } = [];
		}

		private sealed class ImportRecoveryEntry
		{
			public string RelativePath { get; set; } = string.Empty;
			public bool ExistedBeforeImport { get; set; }
		}

		public static async Task ExportAsync(
			string sourceDirectory,
			string destinationFile,
			string password,
			IProgress<SynixTransferProgress>? progress = null,
			CancellationToken cancellationToken = default)
		{
			ValidatePassword(password);

			string sourceRoot = Path.GetFullPath(sourceDirectory)
				.TrimEnd(Path.DirectorySeparatorChar) +
				Path.DirectorySeparatorChar;
			string destinationPath = Path.GetFullPath(destinationFile);

			if (!Directory.Exists(sourceRoot))
			{
				throw new DirectoryNotFoundException(
					$"The Synix data folder was not found: {sourceDirectory}");
			}

			if (IsInsideDirectory(destinationPath, sourceRoot))
			{
				throw new InvalidOperationException(
					"Save the transfer package outside the C:\\Synix folder.");
			}

			string destinationDirectory =
				Path.GetDirectoryName(destinationPath) ??
				throw new InvalidOperationException(
					"The selected destination is not valid.");
			Directory.CreateDirectory(destinationDirectory);

			string operationId = Guid.NewGuid().ToString("N");
			string temporaryZip = Path.Combine(
				destinationDirectory,
				$".synix-{operationId}.zip.tmp");
			string temporaryEncrypted = Path.Combine(
				destinationDirectory,
				$".synix-{operationId}.backup.tmp");

			try
			{
				progress?.Report(new(
					"Preparing files for transfer...",
					0));

				await CreateArchiveAsync(
					sourceRoot,
					temporaryZip,
					progress,
					cancellationToken).ConfigureAwait(false);

				progress?.Report(new(
					"Encrypting the transfer package...",
					50));

				await EncryptFileAsync(
					temporaryZip,
					temporaryEncrypted,
					password,
					progress,
					cancellationToken).ConfigureAwait(false);

				File.Move(
					temporaryEncrypted,
					destinationPath,
					true);

				progress?.Report(new(
					"Synix transfer package is ready.",
					100));
			}
			finally
			{
				TryDeleteFile(temporaryZip);
				TryDeleteFile(temporaryEncrypted);
			}
		}

		public static async Task ImportAsync(
			string packageFile,
			string destinationDirectory,
			string password,
			IProgress<SynixTransferProgress>? progress = null,
			CancellationToken cancellationToken = default)
		{
			ValidatePassword(password);

			string packagePath = Path.GetFullPath(packageFile);
			if (!File.Exists(packagePath))
			{
				throw new FileNotFoundException(
					"The selected Synix transfer package was not found.",
					packagePath);
			}

			string operationId = Guid.NewGuid().ToString("N");
			string temporaryZip = Path.Combine(
				Path.GetTempPath(),
				$"synix-import-{operationId}.zip.tmp");
			string stagingDirectory = Path.Combine(
				Path.GetTempPath(),
				$"synix-import-{operationId}.stage");

			try
			{
				progress?.Report(new(
					"Checking and decrypting the package...",
					0));

				await DecryptFileAsync(
					packagePath,
					temporaryZip,
					password,
					progress,
					cancellationToken).ConfigureAwait(false);

				progress?.Report(new(
					"Preparing restored files...",
					70));

				await ExtractArchiveAsync(
					temporaryZip,
					stagingDirectory,
					progress,
					cancellationToken).ConfigureAwait(false);

				progress?.Report(new(
					"Safely applying restored files...",
					90));

				await CommitStagedImportAsync(
					stagingDirectory,
					destinationDirectory,
					operationId,
					progress,
					cancellationToken).ConfigureAwait(false);

				progress?.Report(new(
					"Synix files were restored.",
					100));
			}
			catch (CryptographicException exception)
			{
				throw new InvalidDataException(
					"The transfer password is incorrect, or the package is damaged.",
					exception);
			}
			catch (EndOfStreamException exception)
			{
				throw new InvalidDataException(
					"The Synix transfer package is incomplete or damaged.",
					exception);
			}
			finally
			{
				TryDeleteFile(temporaryZip);
				TryDeleteDirectory(stagingDirectory);
			}
		}

		/// <summary>
		/// Rolls back any import that was interrupted after it began replacing
		/// files. This must run before Synix loads servers from disk.
		/// </summary>
		public static async Task<bool> RecoverInterruptedImportAsync(
			string destinationDirectory,
			CancellationToken cancellationToken = default)
		{
			string destinationRoot = GetDirectoryRoot(destinationDirectory);
			string recoveryRoot = Path.Combine(
				destinationRoot,
				RecoveryFolderName);
			if (!Directory.Exists(recoveryRoot))
			{
				return false;
			}

			bool rollbackPerformed = false;
			foreach (string operationDirectory in Directory
				.EnumerateDirectories(recoveryRoot)
				.OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
			{
				cancellationToken.ThrowIfCancellationRequested();
				string journalPath = Path.Combine(
					operationDirectory,
					RecoveryJournalFileName);

				if (!File.Exists(journalPath))
				{
					TryDeleteDirectory(operationDirectory);
					continue;
				}

				ImportRecoveryJournal journal = await ReadJournalAsync(
					journalPath,
					cancellationToken).ConfigureAwait(false);
				ValidateRecoveryJournal(
					journal,
					destinationRoot,
					operationDirectory);

				if (journal.State == RecoveryStateCommitting)
				{
					await RollBackImportAsync(
						journal,
						operationDirectory,
						cancellationToken).ConfigureAwait(false);
					rollbackPerformed = true;
				}
				else if (journal.State != RecoveryStatePrepared &&
					journal.State != RecoveryStateCommitted)
				{
					throw new InvalidDataException(
						"Synix found an invalid import recovery journal.");
				}

				TryDeleteDirectory(operationDirectory);
			}

			TryDeleteDirectoryIfEmpty(recoveryRoot);
			return rollbackPerformed;
		}

		private static async Task CreateArchiveAsync(
			string sourceRoot,
			string archivePath,
			IProgress<SynixTransferProgress>? progress,
			CancellationToken cancellationToken)
		{
			string recoveryRoot = Path.Combine(
				sourceRoot,
				RecoveryFolderName);
			List<FileInfo> files = Directory
				.EnumerateFiles(
					sourceRoot,
					"*",
					new EnumerationOptions
					{
						RecurseSubdirectories = true,
						IgnoreInaccessible = false,
						AttributesToSkip = FileAttributes.ReparsePoint
					})
				.Where(path => !IsInsideDirectory(path, recoveryRoot))
				.Select(path => new FileInfo(path))
				.ToList();

			long totalBytes = Math.Max(1, files.Sum(file => file.Length));
			long completedBytes = 0;

			await using FileStream output = new(
				archivePath,
				FileMode.CreateNew,
				FileAccess.ReadWrite,
				FileShare.None,
				131072,
				FileOptions.Asynchronous | FileOptions.SequentialScan);
			using ZipArchive archive = new(
				output,
				ZipArchiveMode.Create,
				leaveOpen: false);

			foreach (FileInfo file in files)
			{
				cancellationToken.ThrowIfCancellationRequested();

				string entryName = Path.GetRelativePath(
					sourceRoot,
					file.FullName);
				ZipArchiveEntry entry = archive.CreateEntry(
					entryName,
					CompressionLevel.Fastest);
				entry.LastWriteTime = NormalizeZipTimestamp(file.LastWriteTime);

				await using Stream entryStream = entry.Open();
				await using FileStream input = new(
					file.FullName,
					FileMode.Open,
					FileAccess.Read,
					FileShare.ReadWrite | FileShare.Delete,
					131072,
					FileOptions.Asynchronous | FileOptions.SequentialScan);

				await CopyWithProgressAsync(
					input,
					entryStream,
					bytesCopied =>
					{
						long current = completedBytes + bytesCopied;
						int percent = (int)Math.Min(
							49,
							current * 49 / totalBytes);
						progress?.Report(new(
							$"Packing {entryName}...",
							percent));
					},
					cancellationToken).ConfigureAwait(false);

				completedBytes += file.Length;
			}
		}

		private static async Task EncryptFileAsync(
			string inputPath,
			string outputPath,
			string password,
			IProgress<SynixTransferProgress>? progress,
			CancellationToken cancellationToken)
		{
			byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
			byte[] key = DeriveKey(password, salt);
			byte[] plainBuffer = new byte[ChunkSize];
			byte[] cipherBuffer = new byte[ChunkSize];
			byte[] tag = new byte[TagSize];

			try
			{
				await using FileStream input = new(
					inputPath,
					FileMode.Open,
					FileAccess.Read,
					FileShare.Read,
					131072,
					FileOptions.Asynchronous | FileOptions.SequentialScan);
				await using FileStream output = new(
					outputPath,
					FileMode.CreateNew,
					FileAccess.Write,
					FileShare.None,
					131072,
					FileOptions.Asynchronous | FileOptions.SequentialScan);
				using BinaryWriter writer = new(output, Encoding.UTF8, true);
				using AesGcm aes = new(key, TagSize);

				writer.Write(Magic);
				writer.Write(FormatVersion);
				writer.Write(Pbkdf2Iterations);
				writer.Write(ChunkSize);
				writer.Write(salt);

				long completedBytes = 0;
				while (true)
				{
					int bytesRead = await ReadChunkAsync(
						input,
						plainBuffer,
						cancellationToken).ConfigureAwait(false);
					if (bytesRead == 0)
					{
						break;
					}

					byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
					aes.Encrypt(
						nonce,
						plainBuffer.AsSpan(0, bytesRead),
						cipherBuffer.AsSpan(0, bytesRead),
						tag);

					writer.Write(bytesRead);
					writer.Write(nonce);
					writer.Write(tag);
					await output.WriteAsync(
						cipherBuffer.AsMemory(0, bytesRead),
						cancellationToken).ConfigureAwait(false);

					completedBytes += bytesRead;
					int percent = 50 + (int)Math.Min(
						49,
						completedBytes * 49 / Math.Max(1, input.Length));
					progress?.Report(new(
						"Encrypting the transfer package...",
						percent));
				}

				writer.Write(0);
				await output.FlushAsync(cancellationToken).ConfigureAwait(false);
			}
			finally
			{
				CryptographicOperations.ZeroMemory(key);
				CryptographicOperations.ZeroMemory(plainBuffer);
				CryptographicOperations.ZeroMemory(cipherBuffer);
				CryptographicOperations.ZeroMemory(tag);
			}
		}

		private static async Task DecryptFileAsync(
			string inputPath,
			string outputPath,
			string password,
			IProgress<SynixTransferProgress>? progress,
			CancellationToken cancellationToken)
		{
			await using FileStream input = new(
				inputPath,
				FileMode.Open,
				FileAccess.Read,
				FileShare.Read,
				131072,
				FileOptions.Asynchronous | FileOptions.SequentialScan);
			using BinaryReader reader = new(input, Encoding.UTF8, true);

			byte[] magic = reader.ReadBytes(Magic.Length);
			if (!magic.SequenceEqual(Magic))
			{
				throw new InvalidDataException(
					"This is not a Synix transfer package.");
			}

			int version = reader.ReadInt32();
			int iterations = reader.ReadInt32();
			int chunkSize = reader.ReadInt32();
			if (version != FormatVersion ||
				iterations != Pbkdf2Iterations ||
				chunkSize != ChunkSize)
			{
				throw new InvalidDataException(
					"This Synix transfer package version is not supported.");
			}

			byte[] salt = reader.ReadBytes(SaltSize);
			if (salt.Length != SaltSize)
			{
				throw new InvalidDataException(
					"The Synix transfer package is incomplete.");
			}

			byte[] key = DeriveKey(password, salt);
			byte[] cipherBuffer = new byte[ChunkSize];
			byte[] plainBuffer = new byte[ChunkSize];

			try
			{
				await using FileStream output = new(
					outputPath,
					FileMode.CreateNew,
					FileAccess.Write,
					FileShare.None,
					131072,
					FileOptions.Asynchronous | FileOptions.SequentialScan);
				using AesGcm aes = new(key, TagSize);

				while (true)
				{
					cancellationToken.ThrowIfCancellationRequested();
					int cipherLength = reader.ReadInt32();
					if (cipherLength == 0)
					{
						break;
					}

					if (cipherLength < 0 || cipherLength > ChunkSize)
					{
						throw new InvalidDataException(
							"The Synix transfer package contains an invalid data block.");
					}

					byte[] nonce = reader.ReadBytes(NonceSize);
					byte[] tag = reader.ReadBytes(TagSize);
					if (nonce.Length != NonceSize || tag.Length != TagSize)
					{
						throw new InvalidDataException(
							"The Synix transfer package is incomplete.");
					}

					await ReadExactlyAsync(
						input,
						cipherBuffer.AsMemory(0, cipherLength),
						cancellationToken).ConfigureAwait(false);

					aes.Decrypt(
						nonce,
						cipherBuffer.AsSpan(0, cipherLength),
						tag,
						plainBuffer.AsSpan(0, cipherLength));

					await output.WriteAsync(
						plainBuffer.AsMemory(0, cipherLength),
						cancellationToken).ConfigureAwait(false);

					int percent = (int)Math.Min(
						69,
						input.Position * 69 / Math.Max(1, input.Length));
					progress?.Report(new(
						"Checking and decrypting the package...",
						percent));
				}

				if (input.Position != input.Length)
				{
					throw new InvalidDataException(
						"The Synix transfer package contains unexpected trailing data.");
				}

				await output.FlushAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (EndOfStreamException exception)
			{
				throw new InvalidDataException(
					"The Synix transfer package is incomplete.",
					exception);
			}
			finally
			{
				CryptographicOperations.ZeroMemory(key);
				CryptographicOperations.ZeroMemory(cipherBuffer);
				CryptographicOperations.ZeroMemory(plainBuffer);
			}
		}

		private static async Task ExtractArchiveAsync(
			string archivePath,
			string destinationDirectory,
			IProgress<SynixTransferProgress>? progress,
			CancellationToken cancellationToken)
		{
			string destinationRoot = Path.GetFullPath(destinationDirectory)
				.TrimEnd(Path.DirectorySeparatorChar) +
				Path.DirectorySeparatorChar;
			Directory.CreateDirectory(destinationRoot);

			using ZipArchive archive = ZipFile.OpenRead(archivePath);
			List<ZipArchiveEntry> files = archive.Entries
				.Where(entry => !string.IsNullOrEmpty(entry.Name))
				.ToList();
			long totalBytes = Math.Max(1, files.Sum(entry => entry.Length));
			long completedBytes = 0;

			foreach (ZipArchiveEntry entry in archive.Entries)
			{
				cancellationToken.ThrowIfCancellationRequested();
				string normalizedEntryName = entry.FullName.Replace('\\', '/');
				string firstPathPart = normalizedEntryName
					.Split('/', StringSplitOptions.RemoveEmptyEntries)
					.FirstOrDefault() ?? string.Empty;
				if (firstPathPart.Equals(
					RecoveryFolderName,
					StringComparison.OrdinalIgnoreCase))
				{
					throw new InvalidDataException(
						"The package contains reserved Synix recovery files.");
				}

				string destinationPath = Path.GetFullPath(
					Path.Combine(destinationRoot, entry.FullName));

				// Keep this validation next to the filesystem operation. Besides
				// preventing Zip Slip, this inline form allows CodeQL to verify
				// that an archive entry can never escape the Synix destination.
				if (!destinationPath.StartsWith(
						destinationRoot,
						StringComparison.OrdinalIgnoreCase))
				{
					throw new InvalidDataException(
						"The package contains an unsafe file path.");
				}

				if (string.IsNullOrEmpty(entry.Name))
				{
					Directory.CreateDirectory(destinationPath);
					continue;
				}

				Directory.CreateDirectory(
					Path.GetDirectoryName(destinationPath)!);
				await using Stream input = entry.Open();
				await using FileStream output = new(
					destinationPath,
					FileMode.Create,
					FileAccess.Write,
					FileShare.None,
					131072,
					FileOptions.Asynchronous | FileOptions.SequentialScan);

				await CopyWithProgressAsync(
					input,
					output,
					bytesCopied =>
					{
						long current = completedBytes + bytesCopied;
						int percent = 70 + (int)Math.Min(
							19,
							current * 19 / totalBytes);
						progress?.Report(new(
							$"Restoring {entry.FullName}...",
							percent));
					},
					cancellationToken).ConfigureAwait(false);

				completedBytes += entry.Length;
				File.SetLastWriteTime(destinationPath, entry.LastWriteTime.LocalDateTime);
			}
		}

		private static async Task CommitStagedImportAsync(
			string stagingDirectory,
			string destinationDirectory,
			string operationId,
			IProgress<SynixTransferProgress>? progress,
			CancellationToken cancellationToken)
		{
			string stagingRoot = GetDirectoryRoot(stagingDirectory);
			string destinationRoot = GetDirectoryRoot(destinationDirectory);
			Directory.CreateDirectory(destinationRoot);

			string recoveryRoot = Path.Combine(
				destinationRoot,
				RecoveryFolderName);
			string operationDirectory = Path.Combine(
				recoveryRoot,
				operationId);
			string rollbackDirectory = Path.Combine(
				operationDirectory,
				"rollback");
			string journalPath = Path.Combine(
				operationDirectory,
				RecoveryJournalFileName);
			Directory.CreateDirectory(rollbackDirectory);

			List<FileInfo> stagedFiles = Directory
				.EnumerateFiles(
					stagingRoot,
					"*",
					SearchOption.AllDirectories)
				.Select(path => new FileInfo(path))
				.OrderBy(
					file => Path.GetRelativePath(stagingRoot, file.FullName),
					StringComparer.OrdinalIgnoreCase)
				.ToList();

			ImportRecoveryJournal journal = new()
			{
				DestinationRoot = destinationRoot,
				OperationId = operationId,
				State = RecoveryStatePrepared
			};

			bool commitStarted = false;
			bool commitCompleted = false;
			bool safeToCleanRecovery = false;
			try
			{
				foreach (FileInfo stagedFile in stagedFiles)
				{
					cancellationToken.ThrowIfCancellationRequested();
					string relativePath = Path.GetRelativePath(
						stagingRoot,
						stagedFile.FullName);
					string destinationPath = GetSafeImportPath(
						destinationRoot,
						relativePath);
					EnsureNoNestedReparsePoint(
						destinationRoot,
						destinationPath);

					bool existed = File.Exists(destinationPath);
					journal.Entries.Add(new()
					{
						RelativePath = relativePath,
						ExistedBeforeImport = existed
					});

					if (!existed)
					{
						continue;
					}

					string rollbackPath = GetSafeImportPath(
						GetDirectoryRoot(rollbackDirectory),
						relativePath);
					Directory.CreateDirectory(
						Path.GetDirectoryName(rollbackPath)!);
					await CopyFileDurablyAsync(
						destinationPath,
						rollbackPath,
						FileMode.Create,
						cancellationToken).ConfigureAwait(false);
					File.SetLastWriteTimeUtc(
						rollbackPath,
						File.GetLastWriteTimeUtc(destinationPath));
				}

				await WriteJournalAsync(
					journalPath,
					journal,
					cancellationToken).ConfigureAwait(false);

				journal.State = RecoveryStateCommitting;
				await WriteJournalAsync(
					journalPath,
					journal,
					cancellationToken).ConfigureAwait(false);
				commitStarted = true;

				long totalBytes = Math.Max(
					1,
					stagedFiles.Sum(file => file.Length));
				long completedBytes = 0;
				foreach (FileInfo stagedFile in stagedFiles)
				{
					cancellationToken.ThrowIfCancellationRequested();
					string relativePath = Path.GetRelativePath(
						stagingRoot,
						stagedFile.FullName);
					string destinationPath = GetSafeImportPath(
						destinationRoot,
						relativePath);
					EnsureNoNestedReparsePoint(
						destinationRoot,
						destinationPath);
					Directory.CreateDirectory(
						Path.GetDirectoryName(destinationPath)!);

					string temporaryPath = GetImportTemporaryPath(
						destinationPath,
						operationId);
					TryDeleteFile(temporaryPath);
					await CopyFileDurablyAsync(
						stagedFile.FullName,
						temporaryPath,
						FileMode.CreateNew,
						cancellationToken).ConfigureAwait(false);
					File.SetLastWriteTimeUtc(
						temporaryPath,
						stagedFile.LastWriteTimeUtc);
					File.Move(
						temporaryPath,
						destinationPath,
						true);

					completedBytes += stagedFile.Length;
					int percent = 90 + (int)Math.Min(
						9,
						completedBytes * 9 / totalBytes);
					progress?.Report(new(
						$"Applying {relativePath}...",
						percent));
				}

				journal.State = RecoveryStateCommitted;
				await WriteJournalAsync(
					journalPath,
					journal,
					cancellationToken).ConfigureAwait(false);
				commitCompleted = true;
				safeToCleanRecovery = true;
			}
			catch (Exception importException)
			{
				if (!commitStarted || commitCompleted)
				{
					safeToCleanRecovery = true;
					throw;
				}

				try
				{
					await RollBackImportAsync(
						journal,
						operationDirectory,
						CancellationToken.None).ConfigureAwait(false);
					safeToCleanRecovery = true;
				}
				catch (Exception rollbackException)
				{
					throw new AggregateException(
						"The import failed and Synix could not finish the immediate rollback. " +
						"Do not remove the recovery folder; Synix will retry recovery the next time it starts.",
						importException,
						rollbackException);
				}

				throw;
			}
			finally
			{
				if (safeToCleanRecovery)
				{
					TryDeleteDirectory(operationDirectory);
					TryDeleteDirectoryIfEmpty(recoveryRoot);
				}
			}
		}

		private static async Task RollBackImportAsync(
			ImportRecoveryJournal journal,
			string operationDirectory,
			CancellationToken cancellationToken)
		{
			string destinationRoot = GetDirectoryRoot(journal.DestinationRoot);
			string rollbackRoot = GetDirectoryRoot(
				Path.Combine(operationDirectory, "rollback"));

			foreach (ImportRecoveryEntry entry in journal.Entries
				.AsEnumerable()
				.Reverse())
			{
				cancellationToken.ThrowIfCancellationRequested();
				string destinationPath = GetSafeImportPath(
					destinationRoot,
					entry.RelativePath);
				EnsureNoNestedReparsePoint(
					destinationRoot,
					destinationPath);
				TryDeleteFile(GetImportTemporaryPath(
					destinationPath,
					journal.OperationId));

				if (!entry.ExistedBeforeImport)
				{
					if (File.Exists(destinationPath))
					{
						File.Delete(destinationPath);
					}
					continue;
				}

				string rollbackPath = GetSafeImportPath(
					rollbackRoot,
					entry.RelativePath);
				if (!File.Exists(rollbackPath))
				{
					throw new InvalidDataException(
						$"The rollback copy is missing for {entry.RelativePath}.");
				}

				Directory.CreateDirectory(
					Path.GetDirectoryName(destinationPath)!);
				string restoreTemporaryPath = GetImportTemporaryPath(
					destinationPath,
					journal.OperationId);
				await CopyFileDurablyAsync(
					rollbackPath,
					restoreTemporaryPath,
					FileMode.Create,
					cancellationToken).ConfigureAwait(false);
				File.SetLastWriteTimeUtc(
					restoreTemporaryPath,
					File.GetLastWriteTimeUtc(rollbackPath));
				File.Move(
					restoreTemporaryPath,
					destinationPath,
					true);
			}

			RemoveEmptyImportedDirectories(
				destinationRoot,
				journal.Entries.Where(entry => !entry.ExistedBeforeImport));
		}

		private static async Task CopyFileDurablyAsync(
			string sourcePath,
			string destinationPath,
			FileMode destinationMode,
			CancellationToken cancellationToken)
		{
			await using FileStream input = new(
				sourcePath,
				FileMode.Open,
				FileAccess.Read,
				FileShare.ReadWrite | FileShare.Delete,
				131072,
				FileOptions.Asynchronous | FileOptions.SequentialScan);
			await using FileStream output = new(
				destinationPath,
				destinationMode,
				FileAccess.Write,
				FileShare.None,
				131072,
				FileOptions.Asynchronous | FileOptions.SequentialScan);

			await input.CopyToAsync(
				output,
				131072,
				cancellationToken).ConfigureAwait(false);
			await output.FlushAsync(cancellationToken).ConfigureAwait(false);
			output.Flush(flushToDisk: true);
		}

		private static async Task WriteJournalAsync(
			string journalPath,
			ImportRecoveryJournal journal,
			CancellationToken cancellationToken)
		{
			string temporaryPath = journalPath + ".tmp";
			byte[] json = JsonSerializer.SerializeToUtf8Bytes(
				journal,
				new JsonSerializerOptions { WriteIndented = true });

			try
			{
				await using FileStream output = new(
					temporaryPath,
					FileMode.Create,
					FileAccess.Write,
					FileShare.None,
					4096,
					FileOptions.Asynchronous | FileOptions.WriteThrough);
				await output.WriteAsync(
					json,
					cancellationToken).ConfigureAwait(false);
				await output.FlushAsync(cancellationToken).ConfigureAwait(false);
				output.Flush(flushToDisk: true);
			}
			finally
			{
				CryptographicOperations.ZeroMemory(json);
			}

			File.Move(temporaryPath, journalPath, true);
		}

		private static async Task<ImportRecoveryJournal> ReadJournalAsync(
			string journalPath,
			CancellationToken cancellationToken)
		{
			await using FileStream input = new(
				journalPath,
				FileMode.Open,
				FileAccess.Read,
				FileShare.Read,
				4096,
				FileOptions.Asynchronous | FileOptions.SequentialScan);
			ImportRecoveryJournal? journal = await JsonSerializer
				.DeserializeAsync<ImportRecoveryJournal>(
					input,
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);
			return journal ?? throw new InvalidDataException(
				"Synix found an empty import recovery journal.");
		}

		private static void ValidateRecoveryJournal(
			ImportRecoveryJournal journal,
			string expectedDestinationRoot,
			string operationDirectory)
		{
			if (journal.Version != RecoveryJournalVersion ||
				string.IsNullOrWhiteSpace(journal.OperationId) ||
				!Path.GetFileName(operationDirectory).Equals(
					journal.OperationId,
					StringComparison.OrdinalIgnoreCase) ||
				!GetDirectoryRoot(journal.DestinationRoot).Equals(
					expectedDestinationRoot,
					StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException(
					"Synix found an invalid import recovery journal.");
			}

			HashSet<string> uniquePaths = new(
				StringComparer.OrdinalIgnoreCase);
			foreach (ImportRecoveryEntry entry in journal.Entries)
			{
				_ = GetSafeImportPath(
					expectedDestinationRoot,
					entry.RelativePath);
				if (!uniquePaths.Add(entry.RelativePath))
				{
					throw new InvalidDataException(
						"The import recovery journal contains duplicate files.");
				}
			}
		}

		private static string GetSafeImportPath(
			string directoryRoot,
			string relativePath)
		{
			if (string.IsNullOrWhiteSpace(relativePath) ||
				Path.IsPathRooted(relativePath) ||
				IsReservedRecoveryPath(relativePath))
			{
				throw new InvalidDataException(
					"The import contains an unsafe or reserved file path.");
			}

			string fullRoot = GetDirectoryRoot(directoryRoot);
			string fullPath = Path.GetFullPath(
				Path.Combine(fullRoot, relativePath));
			if (!fullPath.StartsWith(
					fullRoot,
					StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException(
					"The import contains a file outside its destination.");
			}

			return fullPath;
		}

		private static string GetDirectoryRoot(string directory)
		{
			return Path.GetFullPath(directory)
				.TrimEnd(
					Path.DirectorySeparatorChar,
					Path.AltDirectorySeparatorChar) +
				Path.DirectorySeparatorChar;
		}

		private static bool IsReservedRecoveryPath(string relativePath)
		{
			string firstPart = relativePath
				.Replace('\\', '/')
				.Split('/', StringSplitOptions.RemoveEmptyEntries)
				.FirstOrDefault() ?? string.Empty;
			return firstPart.Equals(
				RecoveryFolderName,
				StringComparison.OrdinalIgnoreCase);
		}

		private static void EnsureNoNestedReparsePoint(
			string destinationRoot,
			string destinationPath)
		{
			if (File.Exists(destinationPath) &&
				(File.GetAttributes(destinationPath) &
				 FileAttributes.ReparsePoint) != 0)
			{
				throw new InvalidDataException(
					"Synix cannot import through a linked file path.");
			}

			string rootWithoutSeparator = destinationRoot.TrimEnd(
				Path.DirectorySeparatorChar,
				Path.AltDirectorySeparatorChar);
			string? current = Path.GetDirectoryName(destinationPath);
			while (!string.IsNullOrEmpty(current) &&
				!current.Equals(
					rootWithoutSeparator,
					StringComparison.OrdinalIgnoreCase))
			{
				if (Directory.Exists(current) &&
					(File.GetAttributes(current) &
					 FileAttributes.ReparsePoint) != 0)
				{
					throw new InvalidDataException(
						"Synix cannot import through a linked folder path.");
				}

				current = Path.GetDirectoryName(current);
			}
		}

		private static string GetImportTemporaryPath(
			string destinationPath,
			string operationId)
		{
			return destinationPath + $".synix-import-{operationId}.tmp";
		}

		private static void RemoveEmptyImportedDirectories(
			string destinationRoot,
			IEnumerable<ImportRecoveryEntry> newEntries)
		{
			foreach (string directory in newEntries
				.Select(entry => Path.GetDirectoryName(
					GetSafeImportPath(destinationRoot, entry.RelativePath)))
				.Where(directory => !string.IsNullOrEmpty(directory))
				.Cast<string>()
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.OrderByDescending(directory => directory.Length))
			{
				string? current = directory;
				while (!string.IsNullOrEmpty(current) &&
					!current.Equals(
						destinationRoot.TrimEnd(Path.DirectorySeparatorChar),
						StringComparison.OrdinalIgnoreCase))
				{
					if (!Directory.Exists(current) ||
						Directory.EnumerateFileSystemEntries(current).Any())
					{
						break;
					}

					Directory.Delete(current);
					current = Path.GetDirectoryName(current);
				}
			}
		}

		private static byte[] DeriveKey(string password, byte[] salt)
		{
			byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
			try
			{
				return Rfc2898DeriveBytes.Pbkdf2(
					passwordBytes,
					salt,
					Pbkdf2Iterations,
					HashAlgorithmName.SHA256,
					KeySize);
			}
			finally
			{
				CryptographicOperations.ZeroMemory(passwordBytes);
			}
		}

		private static async Task<int> ReadChunkAsync(
			Stream input,
			byte[] buffer,
			CancellationToken cancellationToken)
		{
			int totalRead = 0;
			while (totalRead < buffer.Length)
			{
				int bytesRead = await input.ReadAsync(
					buffer.AsMemory(totalRead),
					cancellationToken).ConfigureAwait(false);
				if (bytesRead == 0)
				{
					break;
				}

				totalRead += bytesRead;
			}

			return totalRead;
		}

		private static async Task ReadExactlyAsync(
			Stream input,
			Memory<byte> buffer,
			CancellationToken cancellationToken)
		{
			int totalRead = 0;
			while (totalRead < buffer.Length)
			{
				int bytesRead = await input.ReadAsync(
					buffer[totalRead..],
					cancellationToken).ConfigureAwait(false);
				if (bytesRead == 0)
				{
					throw new EndOfStreamException();
				}

				totalRead += bytesRead;
			}
		}

		private static async Task CopyWithProgressAsync(
			Stream input,
			Stream output,
			Action<long> reportBytes,
			CancellationToken cancellationToken)
		{
			byte[] buffer = new byte[131072];
			long copied = 0;
			while (true)
			{
				int bytesRead = await input.ReadAsync(
					buffer,
					cancellationToken).ConfigureAwait(false);
				if (bytesRead == 0)
				{
					break;
				}

				await output.WriteAsync(
					buffer.AsMemory(0, bytesRead),
					cancellationToken).ConfigureAwait(false);
				copied += bytesRead;
				reportBytes(copied);
			}
		}

		private static bool IsInsideDirectory(
			string path,
			string directoryRoot)
		{
			string fullRoot = Path.GetFullPath(directoryRoot)
				.TrimEnd(Path.DirectorySeparatorChar) +
				Path.DirectorySeparatorChar;
			string fullPath = Path.GetFullPath(path);
			return fullPath.StartsWith(
				fullRoot,
				StringComparison.OrdinalIgnoreCase);
		}

		private static void ValidatePassword(string password)
		{
			if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
			{
				throw new ArgumentException(
					"Use a transfer password containing at least 8 characters.",
					nameof(password));
			}
		}

		private static DateTimeOffset NormalizeZipTimestamp(DateTime timestamp)
		{
			DateTime minimum = new(1980, 1, 1);
			DateTime maximum = new(2107, 12, 31, 23, 59, 58);
			DateTime normalized = timestamp < minimum
				? minimum
				: timestamp > maximum
					? maximum
					: timestamp;
			return new DateTimeOffset(normalized);
		}

		private static void TryDeleteFile(string path)
		{
			try
			{
				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
			catch
			{
				// A failed cleanup must not hide the original operation result.
			}
		}

		private static void TryDeleteDirectory(string path)
		{
			try
			{
				if (Directory.Exists(path))
				{
					Directory.Delete(path, recursive: true);
				}
			}
			catch
			{
				// Recovery data is intentionally retained if cleanup cannot finish.
			}
		}

		private static void TryDeleteDirectoryIfEmpty(string path)
		{
			try
			{
				if (Directory.Exists(path) &&
					!Directory.EnumerateFileSystemEntries(path).Any())
				{
					Directory.Delete(path);
				}
			}
			catch
			{
				// Leaving an empty recovery folder is harmless.
			}
		}
	}
}
