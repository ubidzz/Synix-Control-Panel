using System.Security.Cryptography;
using System.Text.Json;
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using Synix_Control_Panel.SynixEngine.ModManagement;

namespace Synix_Control_Panel.SynixEngine;

/// <summary>Explicit local locations; never supplied by MSI properties or command-line paths.</summary>
internal sealed record SynixUninstallPaths(string Root, string Application, string LocalData,
	string? UserSettings, string? CustomBackups, string? Temp = null, string? RoamingSettings = null)
{
	internal string Data => Path.Combine(Root, "SynixData");
	internal string ServersFile => Path.Combine(Data, "servers.json");
	internal IEnumerable<string> OwnedDataFolders => [Path.Combine(Root, "SteamCMD"), Data];
}

internal sealed record SynixUninstallServer(string InstallPath, string Game, string Identity,
	IReadOnlyList<int> ProcessIds);

/// <summary>
/// Separate app, server and backup scopes. App-only keeps the ENTIRE server root.
/// This includes servers.json and supporting metadata needed to reload the existing
/// server list after reinstalling; these are server data, never disposable app settings.
/// Shared backup roots are never swept, even when selected servers' backups are removed.
/// </summary>
internal sealed class SynixUninstallPlan
{
	private readonly SynixUninstallPaths _paths;
	private readonly string _registryFingerprint;
	private readonly IReadOnlyList<string> _backupCandidates;
	private readonly IReadOnlyList<string> _backupRoots;
	private readonly IReadOnlyList<string> _emptyContainers;
	private readonly IReadOnlyList<string> _ownedTargets;
	private readonly string _recoveryFingerprint;
	internal bool RemovesServers { get; }
	internal bool DeletesBackups { get; }
	internal IReadOnlyList<string> Folders { get; }
	internal IReadOnlyList<string> Files { get; }
	internal IReadOnlyList<string> BackupFolders { get; }
	internal IReadOnlyList<string> BackupFiles { get; }
	internal bool HasBackups => BackupFolders.Count > 0 || BackupFiles.Count > 0;
	internal IReadOnlyList<string> KeptFolders { get; }
	internal IReadOnlyList<SynixUninstallServer> Servers { get; }

	private SynixUninstallPlan(SynixUninstallPaths paths, string fingerprint,
		IReadOnlyList<SynixUninstallServer> servers, SynixAppCleanup app, bool removeServers, bool deleteBackups)
	{
		_paths = paths;
		_registryFingerprint = fingerprint;
		Servers = servers;
		RemovesServers = removeServers;
		DeletesBackups = removeServers && deleteBackups;
		SynixRestoreCleanup restore = SynixRestoreCleanup.Discover(paths, servers);
		_recoveryFingerprint = restore.Fingerprint;
		BackupFiles = removeServers ? restore.JournalFiles.AsReadOnly() : [];
		_backupRoots = app.BackupRoots.Select(Normalize).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
		List<string> recoveries = [.. app.RecoveryFolders,
			Path.Combine(paths.Data, "AddOns"), Path.Combine(paths.Data, ".synix-transfer-recovery")];
		string journalRoot = Normalize(Path.Combine(paths.LocalData, "RestoreOperations"));
		List<string> candidates = [.. recoveries.Where(path => !SamePath(path, journalRoot)), .. restore.BackupFolders];
		foreach (SynixUninstallServer server in removeServers ? servers : [])
			foreach (string root in _backupRoots)
				candidates.Add(Path.Combine(root, SafeName(server.Game), SafeName(server.Identity)));
		_backupCandidates = candidates.Select(Normalize).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
		// Do not touch network backup shares or enumerate server trees for app-only cleanup.
		BackupFolders = removeServers ? FindBackups() : [];
		KeptFolders = _backupRoots.Concat(recoveries.Select(Normalize))
			.Concat(restore.ProtectedFolders)
			.Concat(removeServers ? [] : new[] { Normalize(paths.Root) }.Concat(servers.Select(server => Normalize(server.InstallPath))))
			.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
		_ownedTargets = app.Folders.Concat(app.Files).Concat(app.EmptyContainers).Concat(paths.OwnedDataFolders)
			.Concat(recoveries).Concat(BackupFiles).Select(Normalize).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
		_emptyContainers = new[] { journalRoot }.Concat(app.EmptyContainers).Select(Normalize).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

		List<string> ordinary = [];
		if (removeServers)
			foreach (SynixUninstallServer server in servers) AddFolder(server.InstallPath, server: true);
		foreach (string folder in app.Folders) AddFolder(folder);
		// Registry is LAST: if a server cannot be removed, its registration remains recoverable.
		if (removeServers)
			foreach (string folder in paths.OwnedDataFolders) AddFolder(folder);
		Files = app.Files.Select(path => ValidateTarget(path, isFile: true)).Where(path => !IsKept(path))
			.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
		foreach (string folder in DeletesBackups ? BackupFolders : [])
		{
			ValidateTarget(folder);
			// Never let a backup path encompass a live installation or the whole data root.
			if (ContainsPath(folder, paths.Data) || servers.Any(server => ContainsPath(folder, server.InstallPath))) throw Unsafe(folder);
		}
		string[] preferences = app.PreferenceFolders.Select(Normalize).ToArray();
		Folders = ordinary.Where(path => !SamePath(path, paths.Data) && !preferences.Contains(path, StringComparer.OrdinalIgnoreCase))
			.Concat(DeletesBackups ? BackupFolders : [])
			.Concat(ordinary.Where(path => preferences.Contains(path, StringComparer.OrdinalIgnoreCase)))
			.Concat(ordinary.Where(path => SamePath(path, paths.Data))).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

		void AddFolder(string path, bool server = false)
		{
			string full = ValidateTarget(path);
			if (IsKept(full))
			{
				if (server) throw Unsafe(full);
				return;
			}
			if (ordinary.Contains(full, StringComparer.OrdinalIgnoreCase))
			{
				if (server) throw Unsafe(full);
				return;
			}
			if (ordinary.Any(other => ContainsPath(full, other) || ContainsPath(other, full))) throw Unsafe(full);
			ordinary.Add(full);
		}
	}

	internal static SynixUninstallPlan Create(SynixUninstallPaths paths, bool deleteBackups = false, bool removeServers = true)
	{
		(byte[]? bytes, string fingerprint) = ReadRegistry(paths.ServersFile);
		List<SynixUninstallServer> servers = [];
		if (bytes is not null)
		{
			using JsonDocument document = JsonDocument.Parse(bytes);
			if (document.RootElement.ValueKind != JsonValueKind.Array) throw Changed();
			foreach (JsonElement row in document.RootElement.EnumerateArray())
			{
				string install = Normalize(RequiredString(row, "InstallPath"));
				string game = RequiredString(row, "Game");
				string identity = OptionalString(row, "ConfigurationIdentity");
				if (string.IsNullOrWhiteSpace(identity)) identity = RequiredString(row, "ServerName");
				List<int> pids = [];
				ReadPid(row, "PID", pids);
				ReadPid(row, "SteamPID", pids);
				if (row.TryGetProperty("ServerProcesses", out JsonElement processes) && processes.ValueKind == JsonValueKind.Array)
					foreach (JsonElement process in processes.EnumerateArray()) ReadPid(process, "ProcessId", pids);
				servers.Add(new(install, game, identity, pids.AsReadOnly()));
			}
		}
		return new(paths, fingerprint, servers.AsReadOnly(), SynixAppCleanup.Discover(paths), removeServers, deleteBackups);
	}

	internal void Execute(bool explicitlyConfirmed, Action ensureStopped, Action<string>? progress = null)
	{
		if (!explicitlyConfirmed) throw new InvalidOperationException(LocalizationManager.Get("Uninstall.Confirm"));
		ensureStopped();
		EnsureUnchanged();
		// Check every deletion target before the first mutation, including files and
		// old preference versions. Kept subtrees are not traversed, including links.
		foreach (string folder in Folders)
		{
			ValidateTarget(folder);
			CheckTree(folder, IsBackupTarget(folder) ? folder : null);
		}
		foreach (string file in Files) ValidateTarget(file, isFile: true);
		if (DeletesBackups)
			foreach (string journal in BackupFiles) ValidateTarget(journal, isFile: true);
		foreach (string container in _emptyContainers) ValidateTarget(container);
		ensureStopped();
		EnsureUnchanged();
		foreach (string file in Files)
		{
			ValidateTarget(file, isFile: true);
			if (!IsKept(file)) File.Delete(file);
		}
		foreach (string folder in Folders.Where(path => !SamePath(path, _paths.Data)))
		{
			progress?.Invoke(folder);
			ValidateTarget(folder);
			DeleteTreeWithoutFollowingLinks(folder, IsBackupTarget(folder) ? folder : null);
		}
		// Journal references stay intact if deleting a server or rollback copy fails.
		if (DeletesBackups)
			foreach (string journal in BackupFiles)
			{
				ValidateTarget(journal, isFile: true);
				File.Delete(journal);
			}
		// Only known empty app containers. Never prune C:\Synix, backup roots,
		// user-profile/publisher roots, or any directory with files left in it.
		foreach (string folder in _emptyContainers)
		{
			ValidateTarget(folder);
			if ((!IsKept(folder) || (DeletesBackups && SamePath(folder, Path.Combine(_paths.LocalData, "RestoreOperations")))) &&
				Directory.Exists(folder) && !SynixAppCleanup.Entries(folder).Any())
				Directory.Delete(folder, recursive: false);
		}
		foreach (string folder in Folders.Where(path => SamePath(path, _paths.Data)))
		{
			progress?.Invoke(folder);
			ValidateTarget(folder);
			DeleteTreeWithoutFollowingLinks(folder, selectedBackupRoot: null);
		}
	}

	internal void EnsurePathsUnchanged(SynixUninstallPaths paths)
	{
		if (paths != _paths) throw Changed();
	}

	private void EnsureUnchanged()
	{
		if (ReadRegistry(_paths.ServersFile).Fingerprint != _registryFingerprint) throw Changed();
		string[] currentBackupRoots = SynixAppCleanup.Discover(_paths).BackupRoots.Select(Normalize)
			.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
		if (SynixRestoreCleanup.Discover(_paths, Servers).Fingerprint != _recoveryFingerprint ||
			!_backupRoots.SequenceEqual(currentBackupRoots, StringComparer.OrdinalIgnoreCase) ||
			(RemovesServers && !BackupFolders.SequenceEqual(FindBackups(), StringComparer.OrdinalIgnoreCase))) throw Changed();
	}

	private IReadOnlyList<string> FindBackups()
	{
		List<string> found = [];
		foreach (string candidate in _backupCandidates)
		{
			if (candidate.StartsWith(@"\\", StringComparison.Ordinal)) throw Unsafe(candidate);
			if (SynixAppCleanup.Entries(candidate).Any()) found.Add(candidate);
		}
		return found.AsReadOnly();
	}

	private static (byte[]? Bytes, string Fingerprint) ReadRegistry(string path)
	{
		ModPathSafety.EnsureNoLinks(path);
		try
		{
			using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			if (stream.Length > 16 * 1024 * 1024) throw Changed();
			byte[] bytes = new byte[checked((int)stream.Length)];
			stream.ReadExactly(bytes);
			return (bytes, Convert.ToHexString(SHA256.HashData(bytes)));
		}
		catch (FileNotFoundException) { return (null, "missing"); }
		catch (DirectoryNotFoundException) { return (null, "missing"); }
	}

	private string ValidateTarget(string path, bool isFile = false)
	{
		if (path.StartsWith(@"\\", StringComparison.Ordinal)) throw Unsafe(path);
		string full = Normalize(path);
		if (SamePath(full, Path.GetPathRoot(full)!) || ContainsPath(full, _paths.Root) ||
			ContainsPath(full, _paths.Application) || ContainsPath(_paths.Application, full)) throw Unsafe(path);
		ModPathSafety.EnsureNoLinks(full);
		if (isFile ? Directory.Exists(full) : File.Exists(full)) throw Unsafe(path);
		bool owned = _ownedTargets.Contains(full, StringComparer.OrdinalIgnoreCase) ||
			(_backupCandidates.Contains(full, StringComparer.OrdinalIgnoreCase) && _ownedTargets.Any(root => ContainsPath(root, full)));
		if (!owned)
			full = ServerDeletionSafety.ValidatePath(full);
		foreach (Environment.SpecialFolder special in Enum.GetValues<Environment.SpecialFolder>())
		{
			string protectedPath = Environment.GetFolderPath(special);
			if (!string.IsNullOrWhiteSpace(protectedPath) && ContainsPath(full, protectedPath)) throw Unsafe(path);
		}
		return full;
	}

	private bool IsBackupTarget(string path) => DeletesBackups && BackupFolders.Contains(path, StringComparer.OrdinalIgnoreCase);
	private bool IsKept(string path) => KeptFolders.Any(root => ContainsPath(root, path));
	private bool KeepEntry(string path, string? selectedBackupRoot) => KeptFolders.Any(root =>
		ContainsPath(root, path) && (selectedBackupRoot == null || !ContainsPath(root, selectedBackupRoot)));

	private void CheckTree(string path, string? selectedBackupRoot)
	{
		if (KeepEntry(path, selectedBackupRoot)) return;
		ModPathSafety.EnsureNoLinks(path);
		if (!Directory.Exists(path)) return;
		foreach (string child in SynixAppCleanup.Entries(path)) CheckTree(child, selectedBackupRoot);
	}

	private void DeleteTreeWithoutFollowingLinks(string path, string? selectedBackupRoot)
	{
		if (KeepEntry(path, selectedBackupRoot)) return;
		ModPathSafety.EnsureNoLinks(path);
		if (!Directory.Exists(path)) return;
		foreach (string child in SynixAppCleanup.Entries(path))
		{
			if (KeepEntry(child, selectedBackupRoot)) continue;
			ModPathSafety.EnsureNoLinks(child);
			if ((File.GetAttributes(child) & FileAttributes.Directory) != 0) DeleteTreeWithoutFollowingLinks(child, selectedBackupRoot);
			else File.Delete(child); // Do not alter ACLs or shared hard-link metadata.
		}
		ModPathSafety.EnsureNoLinks(path);
		if (!SynixAppCleanup.Entries(path).Any()) Directory.Delete(path, recursive: false);
	}

	internal static string Normalize(string path)
	{
		string full = ServerDeletionSafety.NormalizePath(path);
		// Keep UNC roots as exclusions only; deleting from network shares is unsupported.
		if (full.StartsWith(@"\\", StringComparison.Ordinal)) return full;
		ModPathSafety.EnsureNoLinks(full);
		// Expand short names on the closest existing ancestor, including absent children.
		string? existing = full;
		while (existing is not null && !Directory.Exists(existing) && !File.Exists(existing)) existing = Path.GetDirectoryName(existing);
		if (existing is null) return full;
		string expanded = ServerDeletionSafety.ExpandExistingLongPath(existing);
		return Path.TrimEndingDirectorySeparator(existing.Equals(full, StringComparison.OrdinalIgnoreCase) ? expanded :
			Path.GetFullPath(Path.Combine(expanded, Path.GetRelativePath(existing, full))));
	}

	internal static bool ContainsPath(string root, string child)
	{
		root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
		child = Path.TrimEndingDirectorySeparator(Path.GetFullPath(child));
		return child.Equals(root, StringComparison.OrdinalIgnoreCase) ||
			child.StartsWith(root.TrimEnd('\\', '/') + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
	}
	private static bool SamePath(string left, string right) => ContainsPath(left, right) && ContainsPath(right, left);

	private static string SafeName(string value)
	{
		string name = value.Replace(" ", "_").Replace(":", "_");
		foreach (char character in Path.GetInvalidFileNameChars()) name = name.Replace(character.ToString(), "");
		if (!ModPathSafety.IsSafeRelativePath(name) || name.Contains('\\') || name.Contains('/')) throw Unsafe(name);
		return name;
	}
	private static string OptionalString(JsonElement row, string key) =>
		row.TryGetProperty(key, out JsonElement value) && value.ValueKind == JsonValueKind.String ? value.GetString()! : string.Empty;
	private static string RequiredString(JsonElement row, string key)
	{
		string value = OptionalString(row, key);
		return !string.IsNullOrWhiteSpace(value) ? value : throw Changed();
	}
	private static void ReadPid(JsonElement row, string key, List<int> pids)
	{
		if (row.TryGetProperty(key, out JsonElement value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int pid) && pid > 0)
			pids.Add(pid);
	}
	private static InvalidOperationException Changed() => new(LocalizationManager.Get("Uninstall.Changed"));
	internal static InvalidOperationException Unsafe(string path) => new(LocalizationManager.Get("FileSystem.Error.UnsafeDeletionPath", path));
}
