// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using Synix_Control_Panel.SynixApp.ServerHandler;
using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel.SynixEngine.ModManagement
{
	internal sealed record ModInventoryItem(
		string Name,
		string Type,
		string Version,
		string Status,
		string SecurityStatus,
		string Source,
		string RelativePath,
		string FullPath,
		string? InstallationId,
		bool CanRemove);

	internal sealed record ModImportResult(
		string InstallationId,
		string DisplayName,
		int InstalledFileCount,
		string BackupFolder,
		bool RestartRequired);

	internal sealed record ModImportSecurityContext(bool IsCurrentProcessElevated)
	{
		internal static ModImportSecurityContext CaptureCurrent() =>
			new(ModSecurityScanner.IsCurrentProcessElevated());
	}

	internal sealed class ModInstallationLedger
	{
		public int SchemaVersion { get; set; } = 1;
		public List<ModInstallationRecord> Installations { get; set; } = [];
	}

	internal sealed class ModInstallationRecord
	{
		public string Id { get; set; } = string.Empty;
		public string ProfileId { get; set; } = string.Empty;
		public string TargetId { get; set; } = string.Empty;
		public string DisplayName { get; set; } = string.Empty;
		public string SourceFileName { get; set; } = string.Empty;
		public string PackageSha256 { get; set; } = string.Empty;
		public string SecurityReview { get; set; } = string.Empty;
		public DateTime? SecurityReviewedAtUtc { get; set; }
		public DateTime InstalledAtUtc { get; set; }
		public string TransactionFolder { get; set; } = string.Empty;
		public List<ModInstalledFile> Files { get; set; } = [];
	}

	internal sealed class ModInstalledFile
	{
		public string RelativePath { get; set; } = string.Empty;
		public string Sha256 { get; set; } = string.Empty;
		public bool ReplacedExistingFile { get; set; }
		public string BackupRelativePath { get; set; } = string.Empty;
	}

	internal sealed record ProviderConfigurationSnapshot(
		string Path,
		bool Existed,
		byte[] Contents);

	internal sealed class ProviderIdConfigurationChange
	{
		private readonly GameServer _server;
		private readonly string _previousExtraArguments;
		private readonly IReadOnlyList<ProviderConfigurationSnapshot> _snapshots;
		private bool _rolledBack;

		internal ProviderIdConfigurationChange(
			GameServer server,
			string previousExtraArguments,
			IReadOnlyList<ProviderConfigurationSnapshot> snapshots)
		{
			_server = server;
			_previousExtraArguments = previousExtraArguments;
			_snapshots = snapshots;
		}

		internal void Rollback()
		{
			if (_rolledBack)
				return;
			_rolledBack = true;
			_server.ExtraArgs = _previousExtraArguments;
			foreach (ProviderConfigurationSnapshot snapshot in _snapshots.Reverse())
			{
				if (!snapshot.Existed)
				{
					if (File.Exists(snapshot.Path))
						File.Delete(snapshot.Path);
					continue;
				}

				string? directory = Path.GetDirectoryName(snapshot.Path);
				if (string.IsNullOrWhiteSpace(directory))
					continue;
				Directory.CreateDirectory(directory);
				string temporary = Path.Combine(
					directory,
					$".{Path.GetFileName(snapshot.Path)}.{Guid.NewGuid():N}.rollback");
				try
				{
					File.WriteAllBytes(temporary, snapshot.Contents);
					File.Move(temporary, snapshot.Path, true);
				}
				finally
				{
					if (File.Exists(temporary))
						File.Delete(temporary);
				}
			}
		}
	}

	internal static class ModPackageManager
	{
		private const int CurrentLedgerSchemaVersion = 1;
		private const int MaximumArchiveEntries = 2048;
		private const long MaximumSingleFileBytes = 256L * 1024 * 1024;
		private const long MaximumArchiveBytes = 512L * 1024 * 1024;
		private static readonly JsonSerializerOptions LedgerJsonOptions = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			PropertyNameCaseInsensitive = false,
			WriteIndented = true
		};

		internal static string? DataRootOverride { get; set; }
		private static string DataRoot => DataRootOverride ?? Path.Combine(Core.DataPath, "AddOns");

		internal static IReadOnlyList<ModInventoryItem> Scan(
			GameServer server,
			ModSystemProfile profile)
		{
			ArgumentNullException.ThrowIfNull(server);
			ArgumentNullException.ThrowIfNull(profile);
			if (!Directory.Exists(server.InstallPath))
				return [];

			ModInstallationLedger ledger = LoadLedger(server);
			Dictionary<string, ModInstallationReference> trackedFiles = BuildTrackedFileMap(ledger);
			List<ModInventoryItem> results = [];
			HashSet<string> inventoryPaths = new(StringComparer.OrdinalIgnoreCase);

			foreach (ModInstallTarget target in profile.Targets)
			{
				if (target.CanManageIds)
				{
					foreach (string id in GetProviderIds(server, target))
					{
						string location = target.Mode == ModTargetMode.ArgumentIds
							? $"{target.ArgumentName}={id}"
							: $"{Path.GetFileName(target.IdStores[0].RelativePath)} • {id}";
						results.Add(new ModInventoryItem(
							id,
							"Mod ID",
							"Provider managed",
							"Configured for next start",
							"Provider download not pre-scanned",
							string.IsNullOrWhiteSpace(target.ProviderName)
								? "Game provider"
								: target.ProviderName,
							location,
							string.Empty,
							$"provider:{target.Id}:{id}",
							true));
					}
					continue;
				}

				string targetRoot = ModSystemCatalog.ResolveInsideInstallPath(
					server.InstallPath,
					target.RelativePath);
				if (!Directory.Exists(targetRoot))
					continue;

				SearchOption searchOption = target.Recursive
					? SearchOption.AllDirectories
					: SearchOption.TopDirectoryOnly;
				HashSet<string> targetNames = new(StringComparer.OrdinalIgnoreCase);
				foreach (string file in Directory.EnumerateFiles(targetRoot, "*", searchOption)
					.Where(path => IsAllowedFile(path, target))
					.OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
				{
					string relativePath = NormalizeRelativePath(
						Path.GetRelativePath(server.InstallPath, file));
					if (!inventoryPaths.Add(relativePath))
						continue;
					string hash = ComputeSha256(file);
					trackedFiles.TryGetValue(relativePath, out ModInstallationReference? reference);
					bool healthy = reference != null &&
						hash.Equals(reference.File.Sha256, StringComparison.OrdinalIgnoreCase);
					string name = Path.GetFileNameWithoutExtension(file);
					targetNames.Add(name);
					results.Add(new ModInventoryItem(
						name,
						target.Kind.ToString(),
						ReadVersion(file),
							reference == null
								? "Detected on disk"
								: healthy ? "Healthy" : "Changed outside Synix",
							reference == null
								? "Not reviewed by Synix"
								: reference.Installation.SecurityReviewedAtUtc == null ||
									string.IsNullOrWhiteSpace(reference.Installation.SecurityReview)
									? "Legacy install • not reviewed"
									: reference.Installation.SecurityReview.Equals(
										"Structural checks completed",
										StringComparison.Ordinal)
										? "Structural checks only"
										: "Pre-install review recorded",
							reference == null
							? string.IsNullOrWhiteSpace(target.ProviderName) ? "External" : target.ProviderName
							: "Synix import",
						relativePath,
						file,
						reference?.Installation.Id,
						healthy));
				}

				if (!target.ScanDirectories)
					continue;
				foreach (string directory in Directory.EnumerateDirectories(
					targetRoot,
					"*",
					SearchOption.TopDirectoryOnly)
					.OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
				{
					if (!targetNames.Add(Path.GetFileName(directory)))
						continue;
					string relativePath = NormalizeRelativePath(
						Path.GetRelativePath(server.InstallPath, directory));
					if (!inventoryPaths.Add(relativePath))
						continue;
					results.Add(new ModInventoryItem(
						Path.GetFileName(directory),
						target.Kind.ToString(),
						"Provider managed",
						"Detected on disk",
						"Not reviewed by Synix",
						string.IsNullOrWhiteSpace(target.ProviderName)
							? "External provider"
							: target.ProviderName,
						relativePath,
						directory,
						null,
						false));
				}
			}

			return results.ToArray();
		}

		internal static ModImportResult Import(
			GameServer server,
			ModSystemProfile profile,
			ModInstallTarget target,
			string packagePath,
			string? expectedPackageSha256 = null,
			string? securityReviewSummary = null,
			ModImportSecurityContext? securityContext = null)
		{
			ArgumentNullException.ThrowIfNull(server);
			ArgumentNullException.ThrowIfNull(profile);
			ArgumentNullException.ThrowIfNull(target);
			EnsureStopped(server);
			securityContext ??= ModImportSecurityContext.CaptureCurrent();
			if (securityContext.IsCurrentProcessElevated)
			{
				throw new InvalidOperationException(
					"Restart Synix normally instead of using Run as administrator before installing add-ons. This prevents mod code from inheriting administrator access.");
			}
			if (profile.SupportLevel != ModSystemSupportLevel.Managed || !target.CanImport)
				throw new InvalidOperationException(
					"Synix can inspect this add-on system, but its provider must install the files.");
			if (!File.Exists(packagePath))
				throw new FileNotFoundException("The selected add-on package no longer exists.", packagePath);

			FileInfo package = new(packagePath);
			if (package.Length is <= 0 or > MaximumArchiveBytes)
				throw new InvalidDataException("The selected add-on package has an invalid size.");

			string targetRoot = ModSystemCatalog.ResolveInsideInstallPath(
				server.InstallPath,
				target.RelativePath);
			string transactionId = Guid.NewGuid().ToString("N");
			string transactionRoot = Path.Combine(GetServerDataFolder(server), "Transactions", transactionId);
			string extractionRoot = Path.Combine(transactionRoot, "Staging");
			string backupRoot = Path.Combine(transactionRoot, "PreviousFiles");
			List<InstallSource> sources = [];
			try
			{
				Directory.CreateDirectory(transactionRoot);
				string packageExtension = Path.GetExtension(packagePath);
				string packageSnapshot = Path.Combine(
					transactionRoot,
					"Incoming",
					$"package{packageExtension.ToLowerInvariant()}");
				CopyPackageSnapshot(packagePath, packageSnapshot);
				string packageSha256 = ComputeSha256(packageSnapshot);
				if (!string.IsNullOrWhiteSpace(expectedPackageSha256) &&
					(!IsSha256(expectedPackageSha256) ||
						!packageSha256.Equals(expectedPackageSha256, StringComparison.OrdinalIgnoreCase)))
				{
					throw new InvalidDataException(
						"The package changed after its security review. Synix stopped before installing anything.");
				}
				IReadOnlyList<ModSecurityFinding> structuralFindings =
					ModSecurityScanner.InspectPackageStructure(packageSnapshot, target);
				if (structuralFindings.Any(finding =>
					finding.Severity == ModSecurityFindingSeverity.Blocked))
				{
					throw new InvalidDataException(
						structuralFindings.First(finding =>
							finding.Severity == ModSecurityFindingSeverity.Blocked).Message);
				}

				if (packageExtension.Equals(".zip", StringComparison.OrdinalIgnoreCase))
				{
					if (!target.AllowArchives)
						throw new InvalidDataException("This add-on system does not accept ZIP packages.");
					sources.AddRange(ExtractPackage(
						packageSnapshot,
						extractionRoot,
						target,
						Path.GetFileNameWithoutExtension(packagePath)));
				}
				else
				{
					if (target.ArchiveOnly)
						throw new InvalidDataException("This add-on area accepts complete ZIP packages only.");
					if (!IsAllowedFile(packageSnapshot, target))
						throw new InvalidDataException(BuildAllowedExtensionMessage(target));
					if (package.Length > MaximumSingleFileBytes)
						throw new InvalidDataException("The selected add-on file exceeds the safety limit.");
					sources.Add(new InstallSource(packageSnapshot, Path.GetFileName(packagePath)));
				}

				if (sources.Count == 0)
					throw new InvalidDataException("The package did not contain a supported add-on file.");

				Directory.CreateDirectory(targetRoot);
				List<AppliedFile> applied = [];
				try
				{
					foreach (InstallSource source in sources)
					{
						string destination = ResolveInsideRoot(targetRoot, source.RelativePath);
						string destinationRelative = NormalizeRelativePath(
							Path.GetRelativePath(server.InstallPath, destination));
						Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
						bool existed = File.Exists(destination);
						string backupRelative = source.RelativePath;
						string backup = ResolveInsideRoot(backupRoot, backupRelative);
						if (existed)
						{
							Directory.CreateDirectory(Path.GetDirectoryName(backup)!);
							File.Copy(destination, backup, true);
						}

						string temporary = destination + ".synix-addon-" + Guid.NewGuid().ToString("N");
						try
						{
							File.Copy(source.FullPath, temporary, false);
							File.Move(temporary, destination, true);
						}
						finally
						{
							TryDeleteFile(temporary);
						}
						applied.Add(new AppliedFile(
							destination,
							destinationRelative,
							existed,
							backup,
							NormalizeRelativePath(backupRelative),
							ComputeSha256(destination)));
					}

					ModInstallationLedger ledger = LoadLedger(server);
					ModInstallationRecord record = new()
					{
						Id = transactionId,
						ProfileId = profile.Id,
						TargetId = target.Id,
						DisplayName = Path.GetFileNameWithoutExtension(packagePath),
						SourceFileName = Path.GetFileName(packagePath),
						PackageSha256 = packageSha256,
						SecurityReview = string.IsNullOrWhiteSpace(expectedPackageSha256)
							? "Structural checks completed"
							: string.IsNullOrWhiteSpace(securityReviewSummary)
								? "Pre-install security review completed"
								: securityReviewSummary,
						SecurityReviewedAtUtc = DateTime.UtcNow,
						InstalledAtUtc = DateTime.UtcNow,
						TransactionFolder = NormalizeRelativePath(
							Path.GetRelativePath(GetServerDataFolder(server), transactionRoot)),
						Files = applied.Select(file => new ModInstalledFile
						{
							RelativePath = file.RelativePath,
							Sha256 = file.Sha256,
							ReplacedExistingFile = file.ReplacedExistingFile,
							BackupRelativePath = file.BackupRelativePath
						}).ToList()
					};
					ledger.Installations.Add(record);
					SaveLedger(server, ledger);

					return new ModImportResult(
						record.Id,
						record.DisplayName,
						record.Files.Count,
						backupRoot,
						profile.RestartRequired);
				}
				catch
				{
					RollbackAppliedFiles(applied);
					throw;
				}
			}
			catch
			{
				TryDeleteDirectory(transactionRoot);
				throw;
			}
		}

		internal static string Remove(GameServer server, string installationId)
		{
			ArgumentNullException.ThrowIfNull(server);
			EnsureStopped(server);
			ModInstallationLedger ledger = LoadLedger(server);
			ModInstallationRecord record = ledger.Installations.FirstOrDefault(candidate =>
				candidate.Id.Equals(installationId, StringComparison.OrdinalIgnoreCase)) ??
				throw new InvalidOperationException("Synix no longer has an installation record for this add-on.");

			foreach (ModInstalledFile file in record.Files)
			{
				string installedPath = ResolveInsideRoot(server.InstallPath, file.RelativePath);
				if (File.Exists(installedPath) &&
					!ComputeSha256(installedPath).Equals(file.Sha256, StringComparison.OrdinalIgnoreCase))
				{
					throw new InvalidOperationException(
						$"{Path.GetFileName(installedPath)} changed after Synix installed it. Synix left it in place so your changes are not lost.");
				}
			}

			string serverData = GetServerDataFolder(server);
			string transactionRoot = ResolveInsideRoot(serverData, record.TransactionFolder);
			List<RemovedFile> removed = [];
			try
			{
				foreach (ModInstalledFile file in record.Files)
				{
					string installedPath = ResolveInsideRoot(server.InstallPath, file.RelativePath);
					string removalBackup = Path.Combine(
						transactionRoot,
						"RemovalRollback",
						file.BackupRelativePath);
					if (File.Exists(installedPath))
					{
						Directory.CreateDirectory(Path.GetDirectoryName(removalBackup)!);
						File.Copy(installedPath, removalBackup, true);
					}

					string previous = Path.Combine(
						transactionRoot,
						"PreviousFiles",
						file.BackupRelativePath);
					if (file.ReplacedExistingFile && File.Exists(previous))
					{
						Directory.CreateDirectory(Path.GetDirectoryName(installedPath)!);
						File.Copy(previous, installedPath, true);
					}
					else
					{
						TryDeleteFile(installedPath);
					}
					removed.Add(new RemovedFile(installedPath, removalBackup));
				}

				ledger.Installations.Remove(record);
				SaveLedger(server, ledger);
				return record.DisplayName;
			}
			catch
			{
				foreach (RemovedFile file in removed.AsEnumerable().Reverse())
				{
					if (!File.Exists(file.RollbackPath))
						continue;
					Directory.CreateDirectory(Path.GetDirectoryName(file.DestinationPath)!);
					File.Copy(file.RollbackPath, file.DestinationPath, true);
				}
				throw;
			}
		}

		internal static IReadOnlyList<string> ParseArgumentIds(
			string? extraArguments,
			ModInstallTarget target)
		{
			ArgumentNullException.ThrowIfNull(target);
			if (target.Mode != ModTargetMode.ArgumentIds || string.IsNullOrWhiteSpace(extraArguments))
				return [];
			Match match = CreateArgumentIdRegex(target).Match(extraArguments);
			if (!match.Success)
				return [];
			return NormalizeProviderIds(match.Groups["ids"].Value, target.MaximumIds);
		}

		internal static IReadOnlyList<string> GetProviderIds(
			GameServer server,
			ModInstallTarget target)
		{
			ArgumentNullException.ThrowIfNull(server);
			ArgumentNullException.ThrowIfNull(target);
			if (target.Mode == ModTargetMode.ArgumentIds)
				return ParseArgumentIds(server.ExtraArgs, target);
			if (target.Mode != ModTargetMode.ConfigurationIds)
				return [];

			foreach (ModIdStore store in target.IdStores)
			{
				string path = ModSystemCatalog.ResolveInsideInstallPath(
					server.InstallPath,
					store.RelativePath);
				IReadOnlyList<string> ids = ReadIniIds(path, store, target.MaximumIds);
				if (ids.Count > 0)
					return ids;
			}
			return [];
		}

		internal static ProviderIdConfigurationChange ConfigureProviderIds(
			GameServer server,
			ModInstallTarget target,
			IEnumerable<string> ids)
		{
			ArgumentNullException.ThrowIfNull(server);
			ArgumentNullException.ThrowIfNull(target);
			EnsureStopped(server);
			if (!target.CanManageIds)
				throw new InvalidOperationException("This add-on target does not use provider IDs.");

			string[] normalized = NormalizeProviderIds(ids, target.MaximumIds).ToArray();
			List<ProviderConfigurationSnapshot> snapshots = [];
			if (target.Mode == ModTargetMode.ConfigurationIds)
			{
				foreach (string path in target.IdStores
					.Select(store => ModSystemCatalog.ResolveInsideInstallPath(
						server.InstallPath,
						store.RelativePath))
					.Distinct(StringComparer.OrdinalIgnoreCase))
				{
					snapshots.Add(new ProviderConfigurationSnapshot(
						path,
						File.Exists(path),
						File.Exists(path) ? File.ReadAllBytes(path) : []));
				}
			}

			ProviderIdConfigurationChange change = new(
				server,
				server.ExtraArgs,
				snapshots);
			try
			{
				string updatedArguments;
				if (target.Mode == ModTargetMode.ArgumentIds)
				{
					updatedArguments = BuildExtraArgumentsWithIds(
						server.ExtraArgs,
						target,
						normalized);
				}
				else
				{
					foreach (ModIdStore store in target.IdStores)
					{
						string path = ModSystemCatalog.ResolveInsideInstallPath(
							server.InstallPath,
							store.RelativePath);
						if (!File.Exists(path) && normalized.Length == 0)
							continue;
						string current = File.Exists(path) ? File.ReadAllText(path) : string.Empty;
						FileHandler.WriteTextAtomically(
							path,
							UpdateIniIdStore(current, store, normalized));
					}
					updatedArguments = AddRequiredArguments(
						server.ExtraArgs,
						normalized.Length == 0 ? [] : target.RequiredArguments);
				}

				if (!Core.TryValidateExtraArguments(updatedArguments, out string validationError))
					throw new InvalidDataException(validationError);
				server.ExtraArgs = updatedArguments;
				return change;
			}
			catch
			{
				change.Rollback();
				throw;
			}
		}

		internal static string BuildExtraArgumentsWithIds(
			string? extraArguments,
			ModInstallTarget target,
			IEnumerable<string> ids)
		{
			ArgumentNullException.ThrowIfNull(target);
			if (target.Mode != ModTargetMode.ArgumentIds)
				throw new InvalidOperationException("This add-on target does not use provider IDs.");
			string[] normalized = NormalizeProviderIds(ids, target.MaximumIds).ToArray();
			string existing = extraArguments?.Trim() ?? string.Empty;
			Match existingMatch = CreateArgumentIdRegex(target).Match(existing);
			string withoutManagedArgument = existing;
			if (existingMatch.Success)
			{
				string before = existing[..existingMatch.Index].TrimEnd();
				string after = existing[(existingMatch.Index + existingMatch.Length)..].TrimStart();
				withoutManagedArgument = string.IsNullOrEmpty(before)
					? after
					: string.IsNullOrEmpty(after) ? before : $"{before} {after}";
			}
			if (normalized.Length == 0)
				return withoutManagedArgument;
			string managedArgument = $"{target.ArgumentName}={string.Join(',', normalized)}";
			return string.IsNullOrWhiteSpace(withoutManagedArgument)
				? managedArgument
				: $"{withoutManagedArgument} {managedArgument}";
		}

		internal static IReadOnlyList<string> NormalizeProviderIds(
			string value,
			int maximumIds) => NormalizeProviderIds(
			value.Split([',', ';', ' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries),
			maximumIds);

		private static IReadOnlyList<string> NormalizeProviderIds(
			IEnumerable<string> ids,
			int maximumIds)
		{
			int limit = Math.Clamp(maximumIds, 1, 1000);
			List<string> normalized = [];
			HashSet<string> seen = new(StringComparer.Ordinal);
			foreach (string candidate in ids)
			{
				string id = candidate.Trim();
				if (id.Length is < 1 or > 20 || !id.All(char.IsAsciiDigit))
					throw new InvalidDataException($"'{candidate}' is not a valid numeric provider mod ID.");
				if (!seen.Add(id))
					continue;
				normalized.Add(id);
				if (normalized.Count > limit)
					throw new InvalidDataException($"This profile allows up to {limit} mod IDs.");
			}
			return normalized;
		}

		private static Regex CreateArgumentIdRegex(ModInstallTarget target) => new(
			$@"(?<!\S){Regex.Escape(target.ArgumentName)}(?:=|\s+)(?<ids>[0-9]+(?:,[0-9]+)*)(?=\s|$)",
			RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
			TimeSpan.FromMilliseconds(250));

		private static IReadOnlyList<string> ReadIniIds(
			string path,
			ModIdStore store,
			int maximumIds)
		{
			if (!File.Exists(path))
				return [];
			List<string> ids = [];
			bool inSection = false;
			foreach (string line in File.ReadLines(path))
			{
				string trimmed = line.Trim();
				if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
				{
					inSection = trimmed[1..^1].Trim()
						.Equals(store.Section, StringComparison.OrdinalIgnoreCase);
					continue;
				}
				if (!inSection || trimmed.StartsWith(';') || trimmed.StartsWith('#'))
					continue;
				int separator = line.IndexOf('=');
				if (separator <= 0 || !line[..separator].Trim()
					.Equals(store.Key, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				string value = StripIniComment(line[(separator + 1)..]);
				if (store.Style == ModIdStoreStyle.Csv)
					ids.AddRange(value.Split([',', ';', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries));
				else if (!string.IsNullOrWhiteSpace(value))
					ids.Add(value.Trim());
			}
			return NormalizeProviderIds(ids, maximumIds);
		}

		private static string UpdateIniIdStore(
			string contents,
			ModIdStore store,
			IReadOnlyList<string> ids)
		{
			string newline = contents.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
			bool hadFinalNewline = contents.EndsWith('\n') || contents.EndsWith('\r');
			List<string> lines = contents
				.Replace("\r\n", "\n", StringComparison.Ordinal)
				.Replace('\r', '\n')
				.Split('\n')
				.ToList();
			if (lines.Count == 1 && lines[0].Length == 0)
				lines.Clear();

			int sectionStart = -1;
			for (int index = 0; index < lines.Count; index++)
			{
				string trimmed = lines[index].Trim();
				if (trimmed.StartsWith('[') && trimmed.EndsWith(']') &&
					trimmed[1..^1].Trim().Equals(store.Section, StringComparison.OrdinalIgnoreCase))
				{
					sectionStart = index;
					break;
				}
			}

			if (sectionStart < 0)
			{
				if (ids.Count == 0)
					return contents;
				if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines[^1]))
					lines.Add(string.Empty);
				sectionStart = lines.Count;
				lines.Add($"[{store.Section}]");
			}

			int sectionEnd = lines.Count;
			for (int index = sectionStart + 1; index < lines.Count; index++)
			{
				string trimmed = lines[index].Trim();
				if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
				{
					sectionEnd = index;
					break;
				}
			}

			int insertionIndex = sectionEnd;
			for (int index = sectionEnd - 1; index > sectionStart; index--)
			{
				if (!IsIniKeyLine(lines[index], store.Key))
					continue;
				insertionIndex = index;
				lines.RemoveAt(index);
			}
			IEnumerable<string> replacements = store.Style == ModIdStoreStyle.Csv
				? ids.Count == 0 ? [] : [$"{store.Key}={string.Join(',', ids)}"]
				: ids.Select(id => $"{store.Key}={id}");
			lines.InsertRange(insertionIndex, replacements);

			string updated = string.Join(newline, lines);
			return hadFinalNewline && !updated.EndsWith(newline, StringComparison.Ordinal)
				? updated + newline
				: updated;
		}

		private static bool IsIniKeyLine(string line, string key)
		{
			string trimmed = line.TrimStart();
			if (trimmed.StartsWith(';') || trimmed.StartsWith('#'))
				return false;
			int separator = line.IndexOf('=');
			return separator > 0 && line[..separator].Trim()
				.Equals(key, StringComparison.OrdinalIgnoreCase);
		}

		private static string StripIniComment(string value)
		{
			for (int index = 0; index < value.Length; index++)
			{
				if (value[index] is ';' or '#' && (index == 0 || char.IsWhiteSpace(value[index - 1])))
					return value[..index].Trim();
			}
			return value.Trim();
		}

		private static string AddRequiredArguments(
			string? extraArguments,
			IEnumerable<string> requiredArguments)
		{
			string updated = extraArguments?.Trim() ?? string.Empty;
			foreach (string required in requiredArguments)
			{
				if (Regex.IsMatch(
					updated,
					$@"(?<!\S){Regex.Escape(required)}(?=\s|$)",
					RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
					TimeSpan.FromMilliseconds(250)))
				{
					continue;
				}
				updated = string.IsNullOrWhiteSpace(updated) ? required : $"{updated} {required}";
			}
			return updated;
		}

		internal static string GetServerDataFolder(GameServer server)
		{
			string fullInstallPath = Path.GetFullPath(server.InstallPath)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
				.ToUpperInvariant();
			string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(fullInstallPath)))[..16];
			string safeName = string.Concat((server.ServerName ?? "Server")
				.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character))
				.Trim();
			if (safeName.Length > 48)
				safeName = safeName[..48];
			if (safeName.Length == 0)
				safeName = "Server";
			return Path.Combine(DataRoot, $"{safeName}-{hash}");
		}

		private static ModInstallationLedger LoadLedger(GameServer server)
		{
			string path = Path.Combine(GetServerDataFolder(server), "installed.json");
			if (!File.Exists(path))
				return new ModInstallationLedger();
			try
			{
				ModInstallationLedger ledger = JsonSerializer.Deserialize<ModInstallationLedger>(
					File.ReadAllText(path),
					LedgerJsonOptions) ?? throw new InvalidDataException("The add-on history is empty.");
				if (ledger.SchemaVersion != CurrentLedgerSchemaVersion)
					throw new InvalidDataException("The add-on history uses an unsupported format.");
				return ledger;
			}
			catch (JsonException exception)
			{
				throw new InvalidDataException(
					"Synix could not read this server's add-on history. No files were changed.",
					exception);
			}
		}

		private static void SaveLedger(GameServer server, ModInstallationLedger ledger)
		{
			string path = Path.Combine(GetServerDataFolder(server), "installed.json");
			FileHandler.WriteTextAtomically(path, JsonSerializer.Serialize(ledger, LedgerJsonOptions));
		}

		private static Dictionary<string, ModInstallationReference> BuildTrackedFileMap(
			ModInstallationLedger ledger)
		{
			Dictionary<string, ModInstallationReference> map = new(StringComparer.OrdinalIgnoreCase);
			foreach (ModInstallationRecord installation in ledger.Installations
				.OrderBy(record => record.InstalledAtUtc))
			{
				foreach (ModInstalledFile file in installation.Files)
					map[file.RelativePath] = new ModInstallationReference(installation, file);
			}
			return map;
		}

		private static IEnumerable<InstallSource> ExtractPackage(
			string archivePath,
			string extractionRoot,
			ModInstallTarget target,
			string packageName)
		{
			Directory.CreateDirectory(extractionRoot);
			string root = Path.GetFullPath(extractionRoot)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
				Path.DirectorySeparatorChar;
			List<InstallSource> sources = [];
			using ZipArchive archive = ZipFile.OpenRead(archivePath);
			if (archive.Entries.Count > MaximumArchiveEntries)
				throw new InvalidDataException("The add-on package contains too many files.");
			bool wrapRootFiles = target.WrapRootArchiveFiles && archive.Entries.Any(entry =>
				!string.IsNullOrWhiteSpace(entry.Name) &&
				entry.FullName.IndexOfAny(['/', '\\']) < 0 &&
				entry.Name.Equals(target.RequiredArchiveFileName, StringComparison.OrdinalIgnoreCase));
			string packageFolder = BuildSafePackageFolderName(packageName);
			long extractedBytes = 0;
			foreach (ZipArchiveEntry entry in archive.Entries)
			{
				if (string.IsNullOrWhiteSpace(entry.Name))
					continue;
				extractedBytes = checked(extractedBytes + entry.Length);
				if (entry.Length > MaximumSingleFileBytes || extractedBytes > MaximumArchiveBytes)
					throw new InvalidDataException("The add-on package exceeds the extraction safety limit.");

				string relative = NormalizeRelativePath(entry.FullName);
				if (wrapRootFiles)
					relative = NormalizeRelativePath(Path.Combine(packageFolder, relative));
				string destination = ResolveInsideRoot(root, relative);
				if (!target.PreserveArchiveContents && !IsAllowedFile(destination, target))
					continue;
				Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
				using Stream source = entry.Open();
				using FileStream output = new(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None);
				source.CopyTo(output);
				sources.Add(new InstallSource(destination, relative));
			}
			return sources;
		}

		private static string BuildSafePackageFolderName(string packageName)
		{
			string safe = string.Concat(packageName.Select(character =>
				Path.GetInvalidFileNameChars().Contains(character) || character is '/' or '\\'
					? '_'
					: character)).Trim().TrimEnd('.', ' ');
			if (safe.Length > 80)
				safe = safe[..80];
			return string.IsNullOrWhiteSpace(safe) ? "ImportedMod" : safe;
		}

		private static bool IsAllowedFile(string path, ModInstallTarget target)
		{
			string extension = Path.GetExtension(path);
			return target.AllowedExtensions.Any(allowed =>
				allowed.Equals(extension, StringComparison.OrdinalIgnoreCase));
		}

		private static string BuildAllowedExtensionMessage(ModInstallTarget target)
		{
			string extensions = string.Join(", ", target.AllowedExtensions);
			return $"This add-on area accepts these file types: {extensions}.";
		}

		private static string ReadVersion(string path)
		{
			try
			{
				string extension = Path.GetExtension(path);
				if (extension.Equals(".dll", StringComparison.OrdinalIgnoreCase))
				{
					FileVersionInfo info = FileVersionInfo.GetVersionInfo(path);
					string? version = info.ProductVersion ?? info.FileVersion;
					if (!string.IsNullOrWhiteSpace(version))
						return version.Trim();
				}
				if (extension.Equals(".jar", StringComparison.OrdinalIgnoreCase))
				{
					using ZipArchive archive = ZipFile.OpenRead(path);
					ZipArchiveEntry? manifest = archive.GetEntry("META-INF/MANIFEST.MF");
					if (manifest != null && manifest.Length <= 128 * 1024)
					{
						using StreamReader reader = new(manifest.Open());
						foreach (string line in reader.ReadToEnd().Split('\n'))
						{
							int separator = line.IndexOf(':');
							if (separator <= 0)
								continue;
							string key = line[..separator].Trim();
							if (key is not ("Implementation-Version" or "Specification-Version" or "Bundle-Version"))
								continue;
							string value = line[(separator + 1)..].Trim();
							if (!string.IsNullOrWhiteSpace(value))
								return value;
						}
					}
				}
			}
			catch
			{
			}
			return "Not reported";
		}

		private static string ComputeSha256(string path)
		{
			using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			return Convert.ToHexString(SHA256.HashData(stream));
		}

		private static void CopyPackageSnapshot(string sourcePath, string destinationPath)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
			using FileStream source = new(
				sourcePath,
				FileMode.Open,
				FileAccess.Read,
				FileShare.Read,
				81920,
				FileOptions.SequentialScan);
			using FileStream destination = new(
				destinationPath,
				FileMode.CreateNew,
				FileAccess.Write,
				FileShare.None,
				81920,
				FileOptions.WriteThrough);
			source.CopyTo(destination);
			destination.Flush(flushToDisk: true);
		}

		private static bool IsSha256(string value) =>
			value.Length == 64 && value.All(character => char.IsAsciiHexDigit(character));

		private static string ResolveInsideRoot(string rootPath, string relativePath)
		{
			if (!ModSystemCatalog.IsSafeRelativePath(relativePath))
				throw new InvalidDataException("The add-on package contains an unsafe path.");
			string root = Path.GetFullPath(rootPath)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
				Path.DirectorySeparatorChar;
			string destination = Path.GetFullPath(Path.Combine(root, relativePath));
			if (!destination.StartsWith(root, StringComparison.OrdinalIgnoreCase))
				throw new InvalidDataException("The add-on package contains an unsafe path.");
			return destination;
		}

		private static string NormalizeRelativePath(string value) =>
			value.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
				.TrimStart(Path.DirectorySeparatorChar);

		private static void EnsureStopped(GameServer server)
		{
			bool processIsRunning = false;
			try
			{
				processIsRunning = Servers.ReconcileActiveServerProcesses(server, forceDiscovery: true);
			}
			catch
			{
			}
			if (processIsRunning || server.Status != StatusManager.GetStatus(ServerState.Stopped))
			{
				throw new InvalidOperationException(
					"Stop the server before installing or removing add-ons. This protects files that the game may still be using.");
			}
		}

		private static void RollbackAppliedFiles(IEnumerable<AppliedFile> appliedFiles)
		{
			foreach (AppliedFile file in appliedFiles.Reverse())
			{
				try
				{
					if (file.ReplacedExistingFile && File.Exists(file.BackupPath))
						File.Copy(file.BackupPath, file.DestinationPath, true);
					else
						TryDeleteFile(file.DestinationPath);
				}
				catch
				{
				}
			}
		}

		private static void TryDeleteFile(string path)
		{
			try
			{
				if (File.Exists(path))
				{
					File.SetAttributes(path, FileAttributes.Normal);
					File.Delete(path);
				}
			}
			catch
			{
			}
		}

		private static void TryDeleteDirectory(string path)
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

		private sealed record ModInstallationReference(
			ModInstallationRecord Installation,
			ModInstalledFile File);

		private sealed record InstallSource(string FullPath, string RelativePath);

		private sealed record AppliedFile(
			string DestinationPath,
			string RelativePath,
			bool ReplacedExistingFile,
			string BackupPath,
			string BackupRelativePath,
			string Sha256);

		private sealed record RemovedFile(string DestinationPath, string RollbackPath);
	}
}
