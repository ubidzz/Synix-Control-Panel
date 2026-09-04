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
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Synix_Control_Panel.SynixApp.Database.GameConfigurations
{
	internal sealed record ConfigurationBackupSnapshot(
		string DirectoryPath,
		DateTimeOffset CreatedAtUtc,
		string Reason);

	internal readonly record struct ConfigurationRestoreResult(
		bool Succeeded,
		int RestoredFiles,
		string Message);

	internal static class ConfigurationBackupManager
	{
		private const int MaximumSnapshots = 10;
		private const string ManifestFileName = "snapshot.json";
		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			PropertyNameCaseInsensitive = false,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = true
		};

		private sealed class SnapshotManifest
		{
			public int FormatVersion { get; init; } = 1;
			public DateTimeOffset CreatedAtUtc { get; init; }
			public string Reason { get; init; } = string.Empty;
			public IReadOnlyList<SnapshotFile> Files { get; init; } = [];
		}

		private sealed class SnapshotFile
		{
			public string RelativePath { get; init; } = string.Empty;
			public string Sha256 { get; init; } = string.Empty;
		}

		internal static ConfigurationBackupSnapshot? CreateSnapshot(
			GameServer server,
			ConfigurationDefinition definition,
			string reason)
		{
			ArgumentNullException.ThrowIfNull(server);
			ArgumentNullException.ThrowIfNull(definition);
			string installRoot = GetInstallRoot(server);
			IReadOnlyList<string> files = definition.ResolveConfigurationPaths(server)
				.Where(File.Exists)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToArray();
			if (files.Count == 0)
				return null;

			DateTimeOffset createdAtUtc = DateTimeOffset.UtcNow;
			string backupRoot = GetBackupRoot(installRoot);
			Directory.CreateDirectory(backupRoot);
			string snapshotName = $"{createdAtUtc:yyyyMMdd-HHmmss-fffffff}-{Guid.NewGuid():N}";
			string finalDirectory = Path.Combine(backupRoot, snapshotName);
			string stagingDirectory = finalDirectory + ".tmp";
			List<SnapshotFile> manifestFiles = [];

			try
			{
				Directory.CreateDirectory(stagingDirectory);
				foreach (string file in files)
				{
					string relative = GetSafeRelativePath(installRoot, file);
					string destination = ResolveInside(
						Path.Combine(stagingDirectory, "files"),
						relative);
					Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
					File.Copy(file, destination, false);
					manifestFiles.Add(new SnapshotFile
					{
						RelativePath = relative.Replace('\\', '/'),
						Sha256 = ComputeSha256(destination)
					});
				}

				SnapshotManifest manifest = new()
				{
					CreatedAtUtc = createdAtUtc,
					Reason = string.IsNullOrWhiteSpace(reason) ? "Configuration change" : reason.Trim(),
					Files = manifestFiles
				};
				File.WriteAllText(
					Path.Combine(stagingDirectory, ManifestFileName),
					JsonSerializer.Serialize(manifest, JsonOptions),
					new UTF8Encoding(false, true));
				Directory.Move(stagingDirectory, finalDirectory);
				TrimOldSnapshots(backupRoot);
				return new ConfigurationBackupSnapshot(
					finalDirectory,
					createdAtUtc,
					manifest.Reason);
			}
			finally
			{
				TryDeleteDirectory(stagingDirectory);
			}
		}

		internal static bool HasSnapshot(
			GameServer server,
			ConfigurationDefinition definition) =>
			TryGetLatestSnapshot(server, definition, out _, out _);

		internal static ConfigurationRestoreResult RestoreLatest(
			GameServer server,
			ConfigurationDefinition definition)
		{
			if (!TryGetLatestSnapshot(
				server,
				definition,
				out string? snapshotDirectory,
				out SnapshotManifest? manifest) ||
				snapshotDirectory == null || manifest == null)
			{
				return new(false, 0, "No previous managed-configuration backup is available.");
			}

			string installRoot = GetInstallRoot(server);
			_ = CreateSnapshot(server, definition, "Before restoring a previous configuration");
			List<(string Target, string Staged, string? Rollback, bool Existed)> files = [];
			List<(string Target, string? Rollback, bool Existed)> replaced = [];

			try
			{
				foreach (SnapshotFile entry in manifest.Files)
				{
					string source = ResolveInside(
						Path.Combine(snapshotDirectory, "files"),
						entry.RelativePath);
					if (!File.Exists(source) ||
						!string.Equals(
							ComputeSha256(source),
							entry.Sha256,
							StringComparison.OrdinalIgnoreCase))
					{
						throw new InvalidDataException(
							$"Backup verification failed for {entry.RelativePath}.");
					}

					string target = ResolveInside(installRoot, entry.RelativePath);
					string directory = Path.GetDirectoryName(target)!;
					Directory.CreateDirectory(directory);
					string staged = Path.Combine(
						directory,
						$".{Path.GetFileName(target)}.{Guid.NewGuid():N}.synix.restore.tmp");
					File.Copy(source, staged, false);
					bool existed = File.Exists(target);
					string? rollback = null;
					if (existed)
					{
						rollback = Path.Combine(
							directory,
							$".{Path.GetFileName(target)}.{Guid.NewGuid():N}.synix.rollback.tmp");
						File.Copy(target, rollback, false);
					}
					files.Add((target, staged, rollback, existed));
				}

				foreach ((string target, string staged, string? rollback, bool existed) in files)
				{
					replaced.Add((target, rollback, existed));
					File.Move(staged, target, true);
				}

				return new(
					true,
					files.Count,
					$"Restored {files.Count} configuration file(s) from the {manifest.CreatedAtUtc.LocalDateTime:g} backup.");
			}
			catch (Exception exception)
			{
				foreach ((string target, string? rollback, bool existed) in replaced.AsEnumerable().Reverse())
				{
					try
					{
						if (existed && rollback != null && File.Exists(rollback))
							File.Move(rollback, target, true);
						else if (!existed && File.Exists(target))
							File.Delete(target);
					}
					catch (Exception suppressedException)
					{
						Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException);
					}
				}
				return new(false, 0, $"The backup could not be restored. Existing files were rolled back when possible. {exception.Message}");
			}
			finally
			{
				foreach ((_, string staged, string? rollback, _) in files)
				{
					TryDeleteFile(staged);
					if (rollback != null)
						TryDeleteFile(rollback);
				}
			}
		}

		internal static void Discard(ConfigurationBackupSnapshot? snapshot)
		{
			if (snapshot != null)
				TryDeleteDirectory(snapshot.DirectoryPath);
		}

		private static bool TryGetLatestSnapshot(
			GameServer server,
			ConfigurationDefinition definition,
			out string? snapshotDirectory,
			out SnapshotManifest? manifest)
		{
			snapshotDirectory = null;
			manifest = null;
			string backupRoot;
			try
			{
				backupRoot = GetBackupRoot(GetInstallRoot(server));
			}
			catch
			{
				return false;
			}
			if (!Directory.Exists(backupRoot))
				return false;

			HashSet<string> managedPaths;
			try
			{
				managedPaths = definition.ResolveConfigurationPaths(server)
					.Select(path => GetSafeRelativePath(GetInstallRoot(server), path).Replace('\\', '/'))
					.ToHashSet(StringComparer.OrdinalIgnoreCase);
			}
			catch
			{
				return false;
			}

			foreach (string directory in Directory.EnumerateDirectories(backupRoot)
				.OrderByDescending(path => path, StringComparer.OrdinalIgnoreCase))
			{
				try
				{
					string manifestPath = Path.Combine(directory, ManifestFileName);
					if (!File.Exists(manifestPath))
						continue;
					SnapshotManifest? candidate = JsonSerializer.Deserialize<SnapshotManifest>(
						File.ReadAllText(manifestPath),
						JsonOptions);
					if (candidate?.FormatVersion != 1 || candidate.Files.Count == 0 ||
						candidate.Files.Any(file => !managedPaths.Contains(file.RelativePath)))
						continue;
					snapshotDirectory = directory;
					manifest = candidate;
					return true;
				}
				catch (Exception exception) when (exception is IOException or JsonException or UnauthorizedAccessException)
				{
				}
			}
			return false;
		}

		private static string GetInstallRoot(GameServer server)
		{
			if (string.IsNullOrWhiteSpace(server.InstallPath))
				throw new InvalidOperationException("The server installation path is missing.");
			return Path.GetFullPath(server.InstallPath)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}

		private static string GetBackupRoot(string installRoot) =>
			Path.Combine(installRoot, ".synix", "configuration-backups");

		private static string GetSafeRelativePath(string root, string path)
		{
			string fullPath = Path.GetFullPath(path);
			if (!fullPath.StartsWith(
				root + Path.DirectorySeparatorChar,
				StringComparison.OrdinalIgnoreCase))
				throw new InvalidDataException("A managed configuration path leaves the server installation folder.");
			return Path.GetRelativePath(root, fullPath);
		}

		private static string ResolveInside(string root, string relativePath)
		{
			string fullRoot = Path.GetFullPath(root)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string path = Path.GetFullPath(Path.Combine(
				fullRoot,
				relativePath.Replace('/', Path.DirectorySeparatorChar)));
			if (!path.StartsWith(
				fullRoot + Path.DirectorySeparatorChar,
				StringComparison.OrdinalIgnoreCase))
				throw new InvalidDataException("A backup entry attempted to leave its allowed folder.");
			return path;
		}

		private static string ComputeSha256(string path)
		{
			using FileStream stream = File.OpenRead(path);
			return Convert.ToHexString(SHA256.HashData(stream));
		}

		private static void TrimOldSnapshots(string backupRoot)
		{
			foreach (string directory in Directory.EnumerateDirectories(backupRoot)
				.Where(path => !path.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase))
				.OrderByDescending(path => path, StringComparer.OrdinalIgnoreCase)
				.Skip(MaximumSnapshots))
			{
				TryDeleteDirectory(directory);
			}
		}

		private static void TryDeleteDirectory(string path)
		{
			try
			{
				if (Directory.Exists(path))
					Directory.Delete(path, true);
			}
			catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
			{
			}
		}

		private static void TryDeleteFile(string path)
		{
			try
			{
				if (File.Exists(path))
					File.Delete(path);
			}
			catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
			{
			}
		}
	}
}
