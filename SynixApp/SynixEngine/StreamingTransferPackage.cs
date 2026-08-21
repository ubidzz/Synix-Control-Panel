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
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Synix_Control_Panel.SynixEngine
{
	public sealed record SynixImportEstimate(
		long PackageBytes,
		long? DataBytes,
		int? FileCount,
		long AdditionalSpaceRequiredBytes,
		long AvailableBytes,
		string DestinationVolume,
		bool UsesLowDiskFormat,
		bool IsPasswordProtected)
	{
		public bool HasEnoughSpace =>
			AvailableBytes >= AdditionalSpaceRequiredBytes;
	}

	public partial class Core
	{
		private static readonly byte[] StreamingPayloadMagic =
			Encoding.ASCII.GetBytes("SYNIXV2D");
		private const int LegacyStreamingHeaderSize = 56;
		private const int StreamingHeaderSize = 57;
		private const int StreamingEndMarker = 0x32444E45;
		private const int MaximumStreamingPathBytes = 32 * 1024;
		private const int MaximumStreamingFiles = 10_000_000;
		private const byte CompressionNone = 0;
		private const byte CompressionDeflate = 1;

		private sealed record StreamingHeader(
			byte[] AuthenticatedBytes,
			byte[] Salt,
			long TotalBytes,
			int FileCount,
			long LargestFileBytes,
			SynixTransferProtection Protection);

		private sealed record StreamingEntry(
			string RelativePath,
			long Length,
			DateTime LastWriteTimeUtc);

		[DllImport(
			"Kernel32.dll",
			CharSet = CharSet.Unicode,
			SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CreateHardLink(
			string newFileName,
			string existingFileName,
			IntPtr securityAttributes);

		private static int ReadPackageVersion(string packagePath)
		{
			using FileStream input = new(
				packagePath,
				FileMode.Open,
				FileAccess.Read,
				FileShare.Read);
			using BinaryReader reader = new(input, Encoding.UTF8, true);
			byte[] magic = reader.ReadBytes(Magic.Length);
			if (!magic.SequenceEqual(Magic))
			{
				throw new InvalidDataException(
					"This is not a Synix transfer package.");
			}

			return reader.ReadInt32();
		}

		public static SynixImportEstimate EstimateImport(
			string packageFile,
			string destinationDirectory)
		{
			string packagePath = Path.GetFullPath(packageFile);
			if (!File.Exists(packagePath))
			{
				throw new FileNotFoundException(
					"The selected Synix transfer package was not found.",
					packagePath);
			}

			long packageBytes = new FileInfo(packagePath).Length;
			string destinationRoot = GetDirectoryRoot(destinationDirectory);
			string volumeRoot = GetVolumeRoot(destinationRoot);
			long availableBytes = GetAvailableFreeSpace(volumeRoot);
			int version = ReadPackageVersion(packagePath);
			if (version == LegacyStreamingFormatVersion ||
				version == StreamingFormatVersion)
			{
				StreamingHeader header = ReadStreamingHeader(packagePath);
				bool hardLinksExpected = SupportsHardLinks(volumeRoot);
				long rollbackAllowance = hardLinksExpected
					? 0
					: header.TotalBytes;
				long requiredBytes = AddWithLimit(
					AddWithLimit(header.TotalBytes, rollbackAllowance),
					ExportSpaceSafetyReserve);
				return new(
					packageBytes,
					header.TotalBytes,
					header.FileCount,
					requiredBytes,
					availableBytes,
					volumeRoot,
					true,
					header.Protection ==
						SynixTransferProtection.PasswordProtected);
			}

			if (version != FormatVersion)
			{
				throw new InvalidDataException(
					"This Synix transfer package version is not supported.");
			}

			long legacyRequired = AddWithLimit(
				MultiplyWithLimit(packageBytes, 3),
				ExportSpaceSafetyReserve);
			return new(
				packageBytes,
				null,
				null,
				legacyRequired,
				availableBytes,
				volumeRoot,
				false,
				true);
		}

		private static async Task ExportStreamingAsync(
			string sourceDirectory,
			string destinationFile,
			string password,
			SynixTransferProtection protection,
			IProgress<SynixTransferProgress>? progress,
			CancellationToken cancellationToken)
		{
			if (protection == SynixTransferProtection.PasswordProtected)
			{
				ValidatePassword(password);
			}
			else if (protection != SynixTransferProtection.Unencrypted)
			{
				throw new ArgumentOutOfRangeException(nameof(protection));
			}
			string sourceRoot = GetDirectoryRoot(sourceDirectory);
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

			SynixExportEstimate estimate = EstimateExport(
				sourceRoot,
				destinationPath,
				cancellationToken);
			ThrowIfInsufficientExportSpace(estimate);

			List<FileInfo> files = GetExportFiles(sourceRoot)
				.OrderBy(
					file => Path.GetRelativePath(sourceRoot, file.FullName),
					StringComparer.OrdinalIgnoreCase)
				.ToList();
			long totalBytes = files.Sum(file => file.Length);
			long largestFileBytes = files.Count == 0
				? 0
				: files.Max(file => file.Length);
			byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
			byte[] headerBytes = CreateStreamingHeader(
				salt,
				totalBytes,
				files.Count,
				largestFileBytes,
				protection);
			byte[] key = protection ==
				SynixTransferProtection.PasswordProtected
					? DeriveKey(password, salt)
					: Array.Empty<byte>();
			string operationId = Guid.NewGuid().ToString("N");
			string temporaryEncrypted = Path.Combine(
				destinationDirectory,
				$".synix-{operationId}.backup.tmp");

			try
			{
				await using FileStream output = new(
					temporaryEncrypted,
					FileMode.CreateNew,
					FileAccess.Write,
					FileShare.None,
					131072,
					FileOptions.Asynchronous | FileOptions.SequentialScan);
				await output.WriteAsync(
					headerBytes,
					cancellationToken).ConfigureAwait(false);
				using AesGcm? aes = protection ==
					SynixTransferProtection.PasswordProtected
						? new AesGcm(key, TagSize)
						: null;

				long completedBytes = 0;
				for (int entryIndex = 0; entryIndex < files.Count; entryIndex++)
				{
					cancellationToken.ThrowIfCancellationRequested();
					FileInfo file = files[entryIndex];
					string relativePath = Path.GetRelativePath(
						sourceRoot,
						file.FullName);
					byte[] metadata = SerializeStreamingMetadata(
						relativePath,
						file.Length,
						file.LastWriteTimeUtc);
					await WriteProtectedMetadataAsync(
						output,
						aes,
						protection,
						headerBytes,
						entryIndex,
						metadata,
						cancellationToken).ConfigureAwait(false);

					await using FileStream input = new(
						file.FullName,
						FileMode.Open,
						FileAccess.Read,
						FileShare.ReadWrite | FileShare.Delete,
						131072,
						FileOptions.Asynchronous | FileOptions.SequentialScan);
					byte[] plainBuffer = new byte[ChunkSize];
					int chunkIndex = 0;
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

						await WriteProtectedDataChunkAsync(
							output,
							aes,
							protection,
							headerBytes,
							entryIndex,
							chunkIndex++,
							plainBuffer.AsMemory(0, bytesRead),
							cancellationToken).ConfigureAwait(false);
						completedBytes += bytesRead;
						int percent = (int)Math.Min(
							99,
							completedBytes * 99 / Math.Max(1, totalBytes));
						progress?.Report(new(
							protection == SynixTransferProtection.PasswordProtected
								? $"Packing and encrypting {relativePath}..."
								: $"Packing {relativePath}...",
							percent,
							completedBytes,
							totalBytes));
					}

					CryptographicOperations.ZeroMemory(plainBuffer);
				}

				await WriteStreamingEndMarkerAsync(
					output,
					aes,
					protection,
					headerBytes,
					cancellationToken).ConfigureAwait(false);
				await output.FlushAsync(cancellationToken).ConfigureAwait(false);
				output.Flush(flushToDisk: true);
				output.Close();
				File.Move(temporaryEncrypted, destinationPath, true);
				progress?.Report(new(
					"Synix transfer package is ready.",
					100,
					totalBytes,
					totalBytes));
			}
			finally
			{
				CryptographicOperations.ZeroMemory(key);
				CryptographicOperations.ZeroMemory(salt);
				TryDeleteFile(temporaryEncrypted);
			}
		}

		private static async Task ImportStreamingAsync(
			string packagePath,
			string destinationDirectory,
			string password,
			IProgress<SynixTransferProgress>? progress,
			CancellationToken cancellationToken)
		{
			StreamingHeader header = ReadStreamingHeader(packagePath);
			if (header.Protection == SynixTransferProtection.PasswordProtected)
			{
				ValidatePassword(password);
			}
			SynixImportEstimate estimate = EstimateImport(
				packagePath,
				destinationDirectory);
			if (!estimate.HasEnoughSpace)
			{
				throw new IOException(
					$"There is not enough free space on {estimate.DestinationVolume}. " +
					$"Synix needs about {FormatTransferBytes(estimate.AdditionalSpaceRequiredBytes)}, " +
					$"but only {FormatTransferBytes(estimate.AvailableBytes)} is available.");
			}

			byte[] key = header.Protection ==
				SynixTransferProtection.PasswordProtected
					? DeriveKey(password, header.Salt)
					: Array.Empty<byte>();
			string destinationRoot = GetDirectoryRoot(destinationDirectory);
			string operationId = Guid.NewGuid().ToString("N");
			try
			{
				progress?.Report(new(
					"Verifying the transfer package without unpacking it...",
					0,
					0,
					MultiplyWithLimit(header.TotalBytes, 2)));
				List<StreamingEntry> entries = await ProcessStreamingPackageAsync(
					packagePath,
					header,
					key,
					destinationRoot,
					null,
					null,
					false,
					progress,
					cancellationToken).ConfigureAwait(false);

				await CommitStreamingImportAsync(
					packagePath,
					header,
					key,
					entries,
					destinationRoot,
					operationId,
					progress,
					cancellationToken).ConfigureAwait(false);
				progress?.Report(new(
					"Synix files were restored.",
					100,
					MultiplyWithLimit(header.TotalBytes, 2),
					MultiplyWithLimit(header.TotalBytes, 2)));
			}
			catch (CryptographicException exception)
			{
				throw new InvalidDataException(
					header.Protection == SynixTransferProtection.PasswordProtected
						? "The transfer password is incorrect, or the package is damaged."
						: "The unencrypted transfer package is damaged.",
					exception);
			}
			finally
			{
				CryptographicOperations.ZeroMemory(key);
			}
		}

		private static async Task VerifyStreamingAsync(
			string packagePath,
			string password,
			IProgress<SynixTransferProgress>? progress,
			CancellationToken cancellationToken)
		{
			StreamingHeader header = ReadStreamingHeader(packagePath);
			if (header.Protection == SynixTransferProtection.PasswordProtected)
			{
				ValidatePassword(password);
			}

			byte[] key = header.Protection ==
				SynixTransferProtection.PasswordProtected
					? DeriveKey(password, header.Salt)
					: Array.Empty<byte>();
			string validationRoot = GetDirectoryRoot(Path.Combine(
				Path.GetTempPath(),
				"synix-package-verification"));
			try
			{
				progress?.Report(new(
					"Verifying the transfer package without importing it...",
					0,
					0,
					header.TotalBytes));
				await ProcessStreamingPackageAsync(
					packagePath,
					header,
					key,
					validationRoot,
					null,
					null,
					true,
					progress,
					cancellationToken).ConfigureAwait(false);
				progress?.Report(new(
					"The transfer package passed verification.",
					100,
					header.TotalBytes,
					header.TotalBytes));
			}
			catch (CryptographicException exception)
			{
				throw new InvalidDataException(
					header.Protection == SynixTransferProtection.PasswordProtected
						? "The transfer password is incorrect, or the package is damaged."
						: "The unencrypted transfer package is damaged.",
					exception);
			}
			finally
			{
				CryptographicOperations.ZeroMemory(key);
			}
		}

		private static async Task<List<StreamingEntry>> ProcessStreamingPackageAsync(
			string packagePath,
			StreamingHeader expectedHeader,
			byte[] key,
			string destinationRoot,
			IReadOnlyList<StreamingEntry>? expectedEntries,
			string? operationId,
			bool standaloneVerification,
			IProgress<SynixTransferProgress>? progress,
			CancellationToken cancellationToken)
		{
			await using FileStream input = new(
				packagePath,
				FileMode.Open,
				FileAccess.Read,
				FileShare.Read,
				131072,
				FileOptions.Asynchronous | FileOptions.SequentialScan);
			byte[] headerBytes = new byte[
				expectedHeader.AuthenticatedBytes.Length];
			await ReadExactlyAsync(
				input,
				headerBytes,
				cancellationToken).ConfigureAwait(false);
			if (!headerBytes.SequenceEqual(expectedHeader.AuthenticatedBytes))
			{
				throw new InvalidDataException(
					"The Synix transfer package header changed while it was being read.");
			}

			using AesGcm? aes = expectedHeader.Protection ==
				SynixTransferProtection.PasswordProtected
					? new AesGcm(key, TagSize)
					: null;
			List<StreamingEntry> entries = new(expectedHeader.FileCount);
			HashSet<string> uniquePaths = new(StringComparer.OrdinalIgnoreCase);
			long completedBytes = 0;
			long expectedWork = standaloneVerification
				? expectedHeader.TotalBytes
				: MultiplyWithLimit(expectedHeader.TotalBytes, 2);
			bool writing = operationId is not null;

			for (int entryIndex = 0;
				entryIndex < expectedHeader.FileCount;
				entryIndex++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				byte[] metadata = await ReadProtectedMetadataAsync(
					input,
					aes,
					expectedHeader.Protection,
					headerBytes,
					entryIndex,
					cancellationToken).ConfigureAwait(false);
				StreamingEntry entry = DeserializeStreamingMetadata(metadata);
				CryptographicOperations.ZeroMemory(metadata);
				string destinationPath = GetSafeImportPath(
					destinationRoot,
					entry.RelativePath);
				EnsureNoNestedReparsePoint(destinationRoot, destinationPath);
				if (!uniquePaths.Add(entry.RelativePath))
				{
					throw new InvalidDataException(
						"The transfer package contains duplicate files.");
				}

				if (expectedEntries is not null &&
					(entryIndex >= expectedEntries.Count ||
					 entry != expectedEntries[entryIndex]))
				{
					throw new InvalidDataException(
						"The transfer package changed after verification.");
				}

				entries.Add(entry);
				FileStream? destination = null;
				string? temporaryPath = null;
				try
				{
					if (writing)
					{
						Directory.CreateDirectory(
							Path.GetDirectoryName(destinationPath)!);
						temporaryPath = GetImportTemporaryPath(
							destinationPath,
							operationId!);
						TryDeleteFile(temporaryPath);
						destination = new FileStream(
							temporaryPath,
							FileMode.CreateNew,
							FileAccess.Write,
							FileShare.None,
							131072,
							FileOptions.Asynchronous | FileOptions.SequentialScan);
					}

					long fileCompleted = 0;
					int chunkIndex = 0;
					while (fileCompleted < entry.Length)
					{
						byte[] plain = await ReadProtectedDataChunkAsync(
							input,
							aes,
							expectedHeader.Protection,
							headerBytes,
							entryIndex,
							chunkIndex++,
							cancellationToken).ConfigureAwait(false);
						if (plain.Length == 0 ||
							fileCompleted + plain.Length > entry.Length)
						{
							throw new InvalidDataException(
								"The transfer package contains an invalid file length.");
						}

						if (destination is not null)
						{
							await destination.WriteAsync(
								plain,
								cancellationToken).ConfigureAwait(false);
						}

						fileCompleted += plain.Length;
						completedBytes += plain.Length;
						long overallCompleted = writing
							? AddWithLimit(expectedHeader.TotalBytes, completedBytes)
							: completedBytes;
						int percent = writing
							? 50 + (int)Math.Min(
								49,
								completedBytes * 49 /
								Math.Max(1, expectedHeader.TotalBytes))
							: standaloneVerification
								? (int)Math.Min(
									99,
									completedBytes * 99 /
									Math.Max(1, expectedHeader.TotalBytes))
								: (int)Math.Min(
									49,
									completedBytes * 49 /
									Math.Max(1, expectedHeader.TotalBytes));
						progress?.Report(new(
							writing
								? $"Restoring {entry.RelativePath}..."
								: $"Verifying {entry.RelativePath}...",
							percent,
							overallCompleted,
							expectedWork));
						CryptographicOperations.ZeroMemory(plain);
					}

					if (destination is not null)
					{
						await destination.FlushAsync(cancellationToken)
							.ConfigureAwait(false);
						destination.Flush(flushToDisk: true);
						await destination.DisposeAsync().ConfigureAwait(false);
						destination = null;
						File.SetLastWriteTimeUtc(
							temporaryPath!,
							entry.LastWriteTimeUtc);
						File.Move(temporaryPath!, destinationPath, true);
					}
				}
				finally
				{
					if (destination is not null)
					{
						await destination.DisposeAsync().ConfigureAwait(false);
					}

					if (temporaryPath is not null && File.Exists(temporaryPath))
					{
						TryDeleteFile(temporaryPath);
					}
				}
			}

			if (completedBytes != expectedHeader.TotalBytes)
			{
				throw new InvalidDataException(
					"The transfer package data size does not match its header.");
			}

			await ReadAndValidateStreamingEndMarkerAsync(
				input,
				aes,
				expectedHeader.Protection,
				headerBytes,
				cancellationToken).ConfigureAwait(false);
			if (input.Position != input.Length)
			{
				throw new InvalidDataException(
					"The transfer package contains unexpected trailing data.");
			}

			return entries;
		}

		private static async Task CommitStreamingImportAsync(
			string packagePath,
			StreamingHeader header,
			byte[] key,
			IReadOnlyList<StreamingEntry> entries,
			string destinationRoot,
			string operationId,
			IProgress<SynixTransferProgress>? progress,
			CancellationToken cancellationToken)
		{
			Directory.CreateDirectory(destinationRoot);
			string recoveryRoot = Path.Combine(
				destinationRoot,
				RecoveryFolderName);
			string operationDirectory = Path.Combine(recoveryRoot, operationId);
			string rollbackDirectory = Path.Combine(operationDirectory, "rollback");
			string journalPath = Path.Combine(
				operationDirectory,
				RecoveryJournalFileName);
			Directory.CreateDirectory(rollbackDirectory);

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
				foreach (StreamingEntry entry in entries)
				{
					cancellationToken.ThrowIfCancellationRequested();
					string destinationPath = GetSafeImportPath(
						destinationRoot,
						entry.RelativePath);
					EnsureNoNestedReparsePoint(destinationRoot, destinationPath);
					bool existed = File.Exists(destinationPath);
					journal.Entries.Add(new()
					{
						RelativePath = entry.RelativePath,
						ExistedBeforeImport = existed
					});
					if (!existed)
					{
						continue;
					}

					string rollbackPath = GetSafeImportPath(
						GetDirectoryRoot(rollbackDirectory),
						entry.RelativePath);
					Directory.CreateDirectory(Path.GetDirectoryName(rollbackPath)!);
					if (!TryCreateHardLink(rollbackPath, destinationPath))
					{
						await CopyFileDurablyAsync(
							destinationPath,
							rollbackPath,
							FileMode.Create,
							cancellationToken).ConfigureAwait(false);
						File.SetLastWriteTimeUtc(
							rollbackPath,
							File.GetLastWriteTimeUtc(destinationPath));
					}
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

				await ProcessStreamingPackageAsync(
					packagePath,
					header,
					key,
					destinationRoot,
					entries,
					operationId,
					false,
					progress,
					cancellationToken).ConfigureAwait(false);

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
						"Synix will retry recovery the next time it starts.",
						importException,
						rollbackException);
				}

				throw;
			}
			finally
			{
				if (safeToCleanRecovery)
				{
					TryDeleteTransferDirectory(operationDirectory);
					TryDeleteDirectoryIfEmpty(recoveryRoot);
				}
			}
		}

		private static bool TryCreateHardLink(
			string rollbackPath,
			string destinationPath)
		{
			try
			{
				return CreateHardLink(
					rollbackPath,
					destinationPath,
					IntPtr.Zero);
			}
			catch
			{
				return false;
			}
		}

		private static bool SupportsHardLinks(string volumeRoot)
		{
			try
			{
				return new DriveInfo(volumeRoot).DriveFormat.Equals(
					"NTFS",
					StringComparison.OrdinalIgnoreCase);
			}
			catch
			{
				return false;
			}
		}

		private static byte[] CreateStreamingHeader(
			byte[] salt,
			long totalBytes,
			int fileCount,
			long largestFileBytes,
			SynixTransferProtection protection)
		{
			using MemoryStream memory = new(StreamingHeaderSize);
			using BinaryWriter writer = new(memory, Encoding.UTF8, true);
			writer.Write(Magic);
			writer.Write(StreamingFormatVersion);
			writer.Write(Pbkdf2Iterations);
			writer.Write(ChunkSize);
			writer.Write(salt);
			writer.Write(totalBytes);
			writer.Write(fileCount);
			writer.Write(largestFileBytes);
			writer.Write((byte)protection);
			writer.Flush();
			return memory.ToArray();
		}

		private static StreamingHeader ReadStreamingHeader(string packagePath)
		{
			int version = ReadPackageVersion(packagePath);
			int headerSize = version switch
			{
				LegacyStreamingFormatVersion => LegacyStreamingHeaderSize,
				StreamingFormatVersion => StreamingHeaderSize,
				_ => throw new InvalidDataException(
					"This Synix transfer package version is not supported.")
			};
			using FileStream input = new(
				packagePath,
				FileMode.Open,
				FileAccess.Read,
				FileShare.Read);
			byte[] headerBytes = new byte[headerSize];
			int totalRead = 0;
			while (totalRead < headerBytes.Length)
			{
				int read = input.Read(
					headerBytes,
					totalRead,
					headerBytes.Length - totalRead);
				if (read == 0)
				{
					throw new InvalidDataException(
						"The Synix transfer package is incomplete.");
				}

				totalRead += read;
			}

			using MemoryStream memory = new(headerBytes, writable: false);
			using BinaryReader reader = new(memory, Encoding.UTF8, true);
			if (!reader.ReadBytes(Magic.Length).SequenceEqual(Magic) ||
				reader.ReadInt32() != version ||
				reader.ReadInt32() != Pbkdf2Iterations ||
				reader.ReadInt32() != ChunkSize)
			{
				throw new InvalidDataException(
					"This Synix transfer package version is not supported.");
			}

			byte[] salt = reader.ReadBytes(SaltSize);
			long totalBytes = reader.ReadInt64();
			int fileCount = reader.ReadInt32();
			long largestFileBytes = reader.ReadInt64();
			SynixTransferProtection protection = version ==
				LegacyStreamingFormatVersion
					? SynixTransferProtection.PasswordProtected
					: (SynixTransferProtection)reader.ReadByte();
			if (salt.Length != SaltSize || totalBytes < 0 ||
				fileCount < 0 || fileCount > MaximumStreamingFiles ||
				largestFileBytes < 0 || largestFileBytes > totalBytes ||
				(protection != SynixTransferProtection.PasswordProtected &&
				 protection != SynixTransferProtection.Unencrypted) ||
				memory.Position != memory.Length)
			{
				throw new InvalidDataException(
					"The Synix transfer package header is invalid.");
			}

			return new(
				headerBytes,
				salt,
				totalBytes,
				fileCount,
				largestFileBytes,
				protection);
		}

		private static byte[] SerializeStreamingMetadata(
			string relativePath,
			long length,
			DateTime lastWriteTimeUtc)
		{
			byte[] pathBytes = Encoding.UTF8.GetBytes(relativePath);
			if (pathBytes.Length == 0 || pathBytes.Length > MaximumStreamingPathBytes)
			{
				throw new InvalidDataException(
					"A Synix file path is too long for the transfer package.");
			}

			using MemoryStream memory = new();
			using BinaryWriter writer = new(memory, Encoding.UTF8, true);
			writer.Write(StreamingPayloadMagic);
			writer.Write(pathBytes.Length);
			writer.Write(pathBytes);
			writer.Write(length);
			writer.Write(lastWriteTimeUtc.Ticks);
			writer.Flush();
			return memory.ToArray();
		}

		private static StreamingEntry DeserializeStreamingMetadata(byte[] metadata)
		{
			using MemoryStream memory = new(metadata, writable: false);
			using BinaryReader reader = new(memory, Encoding.UTF8, true);
			if (!reader.ReadBytes(StreamingPayloadMagic.Length)
				.SequenceEqual(StreamingPayloadMagic))
			{
				throw new InvalidDataException(
					"The transfer package contains invalid file metadata.");
			}

			int pathLength = reader.ReadInt32();
			if (pathLength <= 0 || pathLength > MaximumStreamingPathBytes)
			{
				throw new InvalidDataException(
					"The transfer package contains an invalid file path.");
			}

			byte[] pathBytes = reader.ReadBytes(pathLength);
			if (pathBytes.Length != pathLength)
			{
				throw new InvalidDataException(
					"The transfer package contains incomplete file metadata.");
			}

			string relativePath = new UTF8Encoding(false, true)
				.GetString(pathBytes);
			long length = reader.ReadInt64();
			long ticks = reader.ReadInt64();
			if (length < 0 || memory.Position != memory.Length ||
				ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks)
			{
				throw new InvalidDataException(
					"The transfer package contains invalid file metadata.");
			}

			return new(
				relativePath,
				length,
				new DateTime(ticks, DateTimeKind.Utc));
		}

		private static async Task WriteProtectedMetadataAsync(
			Stream output,
			AesGcm? aes,
			SynixTransferProtection protection,
			byte[] header,
			int entryIndex,
			byte[] metadata,
			CancellationToken cancellationToken)
		{
			byte[] nonce = protection == SynixTransferProtection.PasswordProtected
				? RandomNumberGenerator.GetBytes(NonceSize)
				: new byte[NonceSize];
			byte[] aad = CreateFrameAad(header, (byte)'M', entryIndex, 0);
			byte[] protectedData = ProtectFrame(
				aes,
				protection,
				nonce,
				metadata,
				aad,
				out byte[] tag);
			await WriteInt32Async(output, protectedData.Length, cancellationToken)
				.ConfigureAwait(false);
			await output.WriteAsync(nonce, cancellationToken).ConfigureAwait(false);
			await output.WriteAsync(tag, cancellationToken).ConfigureAwait(false);
			await output.WriteAsync(protectedData, cancellationToken).ConfigureAwait(false);
			CryptographicOperations.ZeroMemory(protectedData);
		}

		private static async Task<byte[]> ReadProtectedMetadataAsync(
			Stream input,
			AesGcm? aes,
			SynixTransferProtection protection,
			byte[] header,
			int entryIndex,
			CancellationToken cancellationToken)
		{
			int length = await ReadInt32Async(input, cancellationToken)
				.ConfigureAwait(false);
			if (length <= 0 || length > MaximumStreamingPathBytes + 64)
			{
				throw new InvalidDataException(
					"The transfer package contains invalid file metadata.");
			}

			byte[] nonce = new byte[NonceSize];
			byte[] tag = new byte[TagSize];
			byte[] protectedData = new byte[length];
			await ReadExactlyAsync(input, nonce, cancellationToken).ConfigureAwait(false);
			await ReadExactlyAsync(input, tag, cancellationToken).ConfigureAwait(false);
			await ReadExactlyAsync(input, protectedData, cancellationToken).ConfigureAwait(false);
			byte[] aad = CreateFrameAad(header, (byte)'M', entryIndex, 0);
			byte[] plain = UnprotectFrame(
				aes,
				protection,
				nonce,
				protectedData,
				tag,
				aad);
			CryptographicOperations.ZeroMemory(protectedData);
			return plain;
		}

		private static async Task WriteProtectedDataChunkAsync(
			Stream output,
			AesGcm? aes,
			SynixTransferProtection protection,
			byte[] header,
			int entryIndex,
			int chunkIndex,
			ReadOnlyMemory<byte> plain,
			CancellationToken cancellationToken)
		{
			byte[] compressed = CompressChunk(plain.Span);
			byte compression = compressed.Length < plain.Length
				? CompressionDeflate
				: CompressionNone;
			ReadOnlyMemory<byte> stored = compression == CompressionDeflate
				? compressed
				: plain;
			byte[] nonce = protection == SynixTransferProtection.PasswordProtected
				? RandomNumberGenerator.GetBytes(NonceSize)
				: new byte[NonceSize];
			byte[] aad = CreateDataAad(
				header,
				entryIndex,
				chunkIndex,
				plain.Length,
				stored.Length,
				compression);
			byte[] protectedData = ProtectFrame(
				aes,
				protection,
				nonce,
				stored.Span,
				aad,
				out byte[] tag);
			await WriteInt32Async(output, plain.Length, cancellationToken)
				.ConfigureAwait(false);
			await WriteInt32Async(output, stored.Length, cancellationToken)
				.ConfigureAwait(false);
			await output.WriteAsync(
				new byte[] { compression },
				cancellationToken).ConfigureAwait(false);
			await output.WriteAsync(nonce, cancellationToken).ConfigureAwait(false);
			await output.WriteAsync(tag, cancellationToken).ConfigureAwait(false);
			await output.WriteAsync(protectedData, cancellationToken).ConfigureAwait(false);
			CryptographicOperations.ZeroMemory(compressed);
			CryptographicOperations.ZeroMemory(protectedData);
		}

		private static async Task<byte[]> ReadProtectedDataChunkAsync(
			Stream input,
			AesGcm? aes,
			SynixTransferProtection protection,
			byte[] header,
			int entryIndex,
			int chunkIndex,
			CancellationToken cancellationToken)
		{
			int plainLength = await ReadInt32Async(input, cancellationToken)
				.ConfigureAwait(false);
			int storedLength = await ReadInt32Async(input, cancellationToken)
				.ConfigureAwait(false);
			byte[] compressionBuffer = new byte[1];
			await ReadExactlyAsync(input, compressionBuffer, cancellationToken)
				.ConfigureAwait(false);
			byte compression = compressionBuffer[0];
			if (plainLength <= 0 || plainLength > ChunkSize ||
				storedLength <= 0 || storedLength > ChunkSize ||
				(compression != CompressionNone &&
				 compression != CompressionDeflate) ||
				(compression == CompressionNone && storedLength != plainLength))
			{
				throw new InvalidDataException(
					"The transfer package contains an invalid data block.");
			}

			byte[] nonce = new byte[NonceSize];
			byte[] tag = new byte[TagSize];
			byte[] protectedData = new byte[storedLength];
			await ReadExactlyAsync(input, nonce, cancellationToken).ConfigureAwait(false);
			await ReadExactlyAsync(input, tag, cancellationToken).ConfigureAwait(false);
			await ReadExactlyAsync(input, protectedData, cancellationToken).ConfigureAwait(false);
			byte[] aad = CreateDataAad(
				header,
				entryIndex,
				chunkIndex,
				plainLength,
				storedLength,
				compression);
			byte[] stored = UnprotectFrame(
				aes,
				protection,
				nonce,
				protectedData,
				tag,
				aad);
			CryptographicOperations.ZeroMemory(protectedData);
			if (compression == CompressionNone)
			{
				return stored;
			}

			byte[] plain = DecompressChunk(stored, plainLength);
			CryptographicOperations.ZeroMemory(stored);
			return plain;
		}

		private static byte[] CompressChunk(ReadOnlySpan<byte> plain)
		{
			using MemoryStream memory = new();
			using (DeflateStream compressor = new(
				memory,
				CompressionLevel.Fastest,
				leaveOpen: true))
			{
				compressor.Write(plain);
			}

			return memory.ToArray();
		}

		private static byte[] DecompressChunk(byte[] compressed, int expectedLength)
		{
			using MemoryStream input = new(compressed, writable: false);
			using DeflateStream decompressor = new(
				input,
				CompressionMode.Decompress);
			byte[] plain = new byte[expectedLength];
			int totalRead = 0;
			while (totalRead < plain.Length)
			{
				int read = decompressor.Read(
					plain,
					totalRead,
					plain.Length - totalRead);
				if (read == 0)
				{
					throw new InvalidDataException(
						"The transfer package contains incomplete compressed data.");
				}

				totalRead += read;
			}

			if (decompressor.ReadByte() != -1)
			{
				throw new InvalidDataException(
					"The transfer package contains too much compressed data.");
			}

			return plain;
		}

		private static byte[] CreateFrameAad(
			byte[] header,
			byte frameType,
			int entryIndex,
			int chunkIndex)
		{
			using MemoryStream memory = new();
			using BinaryWriter writer = new(memory, Encoding.UTF8, true);
			writer.Write(header);
			writer.Write(frameType);
			writer.Write(entryIndex);
			writer.Write(chunkIndex);
			writer.Flush();
			return memory.ToArray();
		}

		private static byte[] CreateDataAad(
			byte[] header,
			int entryIndex,
			int chunkIndex,
			int plainLength,
			int storedLength,
			byte compression)
		{
			using MemoryStream memory = new();
			using BinaryWriter writer = new(memory, Encoding.UTF8, true);
			writer.Write(header);
			writer.Write((byte)'D');
			writer.Write(entryIndex);
			writer.Write(chunkIndex);
			writer.Write(plainLength);
			writer.Write(storedLength);
			writer.Write(compression);
			writer.Flush();
			return memory.ToArray();
		}

		private static async Task WriteStreamingEndMarkerAsync(
			Stream output,
			AesGcm? aes,
			SynixTransferProtection protection,
			byte[] header,
			CancellationToken cancellationToken)
		{
			byte[] nonce = protection == SynixTransferProtection.PasswordProtected
				? RandomNumberGenerator.GetBytes(NonceSize)
				: new byte[NonceSize];
			byte[] aad = CreateFrameAad(header, (byte)'F', -1, -1);
			byte[] protectedData = ProtectFrame(
				aes,
				protection,
				nonce,
				ReadOnlySpan<byte>.Empty,
				aad,
				out byte[] tag);
			await WriteInt32Async(output, StreamingEndMarker, cancellationToken)
				.ConfigureAwait(false);
			await output.WriteAsync(nonce, cancellationToken).ConfigureAwait(false);
			await output.WriteAsync(tag, cancellationToken).ConfigureAwait(false);
			CryptographicOperations.ZeroMemory(protectedData);
		}

		private static async Task ReadAndValidateStreamingEndMarkerAsync(
			Stream input,
			AesGcm? aes,
			SynixTransferProtection protection,
			byte[] header,
			CancellationToken cancellationToken)
		{
			int marker = await ReadInt32Async(input, cancellationToken)
				.ConfigureAwait(false);
			if (marker != StreamingEndMarker)
			{
				throw new InvalidDataException(
					"The transfer package is incomplete.");
			}

			byte[] nonce = new byte[NonceSize];
			byte[] tag = new byte[TagSize];
			await ReadExactlyAsync(input, nonce, cancellationToken).ConfigureAwait(false);
			await ReadExactlyAsync(input, tag, cancellationToken).ConfigureAwait(false);
			byte[] aad = CreateFrameAad(header, (byte)'F', -1, -1);
			byte[] plain = UnprotectFrame(
				aes,
				protection,
				nonce,
				ReadOnlySpan<byte>.Empty,
				tag,
				aad);
			CryptographicOperations.ZeroMemory(plain);
		}

		private static byte[] ProtectFrame(
			AesGcm? aes,
			SynixTransferProtection protection,
			byte[] nonce,
			ReadOnlySpan<byte> plain,
			byte[] associatedData,
			out byte[] tag)
		{
			byte[] protectedData = plain.ToArray();
			if (protection == SynixTransferProtection.PasswordProtected)
			{
				if (aes is null)
				{
					throw new InvalidOperationException(
						"Encryption was not initialized.");
				}

				tag = new byte[TagSize];
				aes.Encrypt(
					nonce,
					plain,
					protectedData,
					tag,
					associatedData);
				return protectedData;
			}

			tag = CalculateIntegrityTag(associatedData, protectedData);
			return protectedData;
		}

		private static byte[] UnprotectFrame(
			AesGcm? aes,
			SynixTransferProtection protection,
			byte[] nonce,
			ReadOnlySpan<byte> protectedData,
			byte[] tag,
			byte[] associatedData)
		{
			byte[] plain = new byte[protectedData.Length];
			if (protection == SynixTransferProtection.PasswordProtected)
			{
				if (aes is null)
				{
					throw new InvalidOperationException(
						"Encryption was not initialized.");
				}

				aes.Decrypt(
					nonce,
					protectedData,
					tag,
					plain,
					associatedData);
				return plain;
			}

			byte[] expectedTag = CalculateIntegrityTag(
				associatedData,
				protectedData);
			bool valid = CryptographicOperations.FixedTimeEquals(
				expectedTag,
				tag);
			CryptographicOperations.ZeroMemory(expectedTag);
			if (!valid)
			{
				throw new CryptographicException(
					"The unencrypted transfer package is damaged.");
			}

			protectedData.CopyTo(plain);
			return plain;
		}

		private static byte[] CalculateIntegrityTag(
			byte[] associatedData,
			ReadOnlySpan<byte> data)
		{
			using IncrementalHash hash = IncrementalHash.CreateHash(
				HashAlgorithmName.SHA256);
			hash.AppendData(associatedData);
			hash.AppendData(data);
			byte[] digest = hash.GetHashAndReset();
			byte[] tag = digest.AsSpan(0, TagSize).ToArray();
			CryptographicOperations.ZeroMemory(digest);
			return tag;
		}

		private static async Task WriteInt32Async(
			Stream output,
			int value,
			CancellationToken cancellationToken)
		{
			byte[] buffer = BitConverter.GetBytes(value);
			await output.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
		}

		private static async Task<int> ReadInt32Async(
			Stream input,
			CancellationToken cancellationToken)
		{
			byte[] buffer = new byte[sizeof(int)];
			await ReadExactlyAsync(input, buffer, cancellationToken).ConfigureAwait(false);
			return BitConverter.ToInt32(buffer);
		}
	}
}
