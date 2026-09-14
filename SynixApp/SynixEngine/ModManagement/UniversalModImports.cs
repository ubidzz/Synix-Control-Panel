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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Synix_Control_Panel.SynixEngine.ModManagement;

// Server-scoped, declarative import rules. They cannot execute installers or change
// another server's profile. Their lifetime follows the existing add-on recovery data.
internal static class UniversalModImports
{
	private const string FileName = "import-locations.modsystem.json";
	private const string ProfileId = "server-import-locations";
	private static readonly JsonSerializerOptions Options = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true,
		Converters = { new JsonStringEnumConverter() }
	};

	internal static ModSystemProfile? Load(GameServer server)
	{
		if (string.IsNullOrWhiteSpace(server.InstallPath) || !Path.IsPathFullyQualified(server.InstallPath)) return null;
		string path = ModPathSafety.Resolve(ModPackageManager.GetServerDataFolder(server), FileName);
		if (!File.Exists(path)) return null;
		ModPathSafety.EnsureNoLinks(path);
		using FileStream input = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		if (input.Length > 256 * 1024) throw Error("InvalidLocations");
		using StreamReader reader = new(input);
		ModSystemCatalogDocument document = ModSystemCatalog.Parse(reader.ReadToEnd(), FileName);
		if (document.Profiles.Count != 1) throw Error("InvalidLocations");
		ModSystemProfile profile = document.Profiles[0];
		if (profile.Id != ProfileId || profile.GameNames.Count != 1 || !profile.GameNames[0].Equals(server.Game, StringComparison.OrdinalIgnoreCase) ||
			profile.SupportLevel != ModSystemSupportLevel.Managed || profile.Targets.Count > 32 ||
			profile.Targets.Any(target => target.PackageLayout != ModPackageLayout.FolderTree)) throw Error("InvalidLocations");
		foreach (ModInstallTarget target in profile.Targets)
			ModSystemCatalog.ResolveInsideInstallPath(server.InstallPath, target.RelativePath);
		return Profile(server, profile.Targets);
	}

	internal static ModSystemProfile SaveLocation(GameServer server, string name, string relativePath,
		string extensions, ModContentKind kind)
	{
		using ServerOperationLease operation = ModPackageManager.BeginOperation(server);
		ModPackageManager.EnsureStopped(server);
		if (!Directory.Exists(server.InstallPath)) throw Error("InstallFolder");
		ModInstallTarget target = CreateTarget(server, name, relativePath, extensions, kind);
		List<ModInstallTarget> targets = Load(server)?.Targets.ToList() ?? [];
		int index = targets.FindIndex(existing => existing.Id == target.Id);
		if (index < 0) targets.Add(target); else targets[index] = target;
		if (targets.Count > 32) throw Error("LocationLimit");
		ModSystemProfile profile = Profile(server, targets);
		string json = JsonSerializer.Serialize(new ModSystemCatalogDocument { SchemaVersion = 1, Profiles = [profile] }, Options);
		_ = ModSystemCatalog.Parse(json, FileName);
		string path = ModPathSafety.Resolve(ModPackageManager.GetServerDataFolder(server), FileName);
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
		try
		{
			File.WriteAllText(temporary, json);
			ModPathSafety.EnsureNoLinks(path);
			File.Move(temporary, path, overwrite: true);
		}
		finally { if (File.Exists(temporary)) File.Delete(temporary); }
		return profile;
	}

	internal static ModInstallTarget CreateTarget(GameServer server, string name, string relativePath,
		string extensions, ModContentKind kind)
	{
		name = name.Trim();
		relativePath = relativePath.Trim().Replace('\\', '/');
		if (name.Length is < 1 or > 80 || name.Any(char.IsControl) || kind is not (ModContentKind.Mod or ModContentKind.Plugin))
			throw Error("LocationName");
		_ = ModSystemCatalog.ResolveInsideInstallPath(server.InstallPath, relativePath);
		string[] types = extensions.Split([',', ';', ' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries)
			.Select(value => value.ToLowerInvariant()).Distinct().ToArray();
		if (types.Length is < 1 or > 32) throw Error("Extensions");
		string id = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(relativePath.ToUpperInvariant())))[..16].ToLowerInvariant();
		ModInstallTarget target = new()
		{
			Id = "local-" + id, DisplayName = name, Kind = kind, Mode = ModTargetMode.FileImport,
			PackageLayout = ModPackageLayout.FolderTree, RelativePath = relativePath, AllowedExtensions = types.ToList(),
			AllowArchives = true, AllowFolderImport = true, PreserveArchiveContents = true, Recursive = true, ScanDirectories = true
		};
		// The same allow-list and path validation as shipped/external profiles applies here.
		_ = ModSystemCatalog.Parse(JsonSerializer.Serialize(new ModSystemCatalogDocument
			{ SchemaVersion = 1, Profiles = [Profile(server, [target])] }, Options), FileName);
		return target;
	}

	internal static ModSystemProfile Profile(GameServer server, List<ModInstallTarget> targets) => new()
	{
		Id = ProfileId, DisplayName = LocalizationManager.Get("UniversalMods.Locations.Profile"),
		Description = LocalizationManager.Get("UniversalMods.Locations.Warning"),
		GameNames = [server.Game], SupportLevel = ModSystemSupportLevel.Managed,
		UserConfigured = true, Targets = targets, RestartRequired = true
	};

	internal static InvalidDataException Error(string key) => new(LocalizationManager.Get("UniversalMods.Error." + key));
}

internal sealed record ModPackageFilePreview(string Source, string Destination, long Bytes, bool ReplacesFile);
internal sealed record ModPackagePreview(IReadOnlyList<string> Roots, IReadOnlyList<ModPackageFilePreview> Files, string PackageSha256);

internal static class UniversalModPackage
{
	internal static ModPackagePreview Preview(GameServer server, ModInstallTarget target, string source, string selectedRoot = "", CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		ModPathSafety.EnsureNoLinks(source);
		using FileStream input = new(source, FileMode.Open, FileAccess.Read, FileShare.Read);
		if (input.Length is <= 0 || input.Length > ModPackageLimits.For(target).TotalBytes) throw ModPackageFiles.Error("Size");
		using IncrementalHash digest = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
		byte[] buffer = new byte[81920];
		int count;
		while ((count = input.Read(buffer, 0, buffer.Length)) != 0)
		{
			cancellationToken.ThrowIfCancellationRequested();
			digest.AppendData(buffer, 0, count);
		}
		string hash = Convert.ToHexString(digest.GetHashAndReset());
		input.Position = 0;
		List<ModPackageFilePreview> files = [];
		string destinationRoot = ModSystemCatalog.ResolveInsideInstallPath(server.InstallPath, target.RelativePath);
		void Add(string from, string to, long bytes)
		{
			string destination = ModPathSafety.Resolve(destinationRoot, to);
			if (Directory.Exists(destination)) throw UniversalModImports.Error("Collision");
			files.Add(new(from, Path.GetRelativePath(server.InstallPath, destination), bytes, File.Exists(destination)));
		}
		if (!Path.GetExtension(source).Equals(".zip", StringComparison.OrdinalIgnoreCase))
		{
			if (target.ArchiveOnly || selectedRoot.Length > 0 || !target.AllowedExtensions.Contains(Path.GetExtension(source), StringComparer.OrdinalIgnoreCase))
				throw UniversalModImports.Error("NoFiles");
			Add(Path.GetFileName(source), Path.GetFileName(source), input.Length);
			return new([string.Empty], files, hash);
		}
		if (!target.AllowArchives) throw ModPackageFiles.Error("Layout");
		using ZipArchive zip = new(input, ZipArchiveMode.Read);
		IReadOnlyDictionary<string, string> paths = ModPackageHandlers.Map(zip, target, Path.GetFileNameWithoutExtension(source),
			selectedRoot.Length == 0 ? null : selectedRoot, cancellationToken);
		HashSet<string> roots = new(StringComparer.OrdinalIgnoreCase) { string.Empty, selectedRoot };
		foreach (ZipArchiveEntry entry in zip.Entries.Where(entry => !entry.FullName.EndsWith('/') && !entry.FullName.EndsWith('\\')))
		{
			cancellationToken.ThrowIfCancellationRequested();
			string path = entry.FullName.Replace('\\', '/');
			if (paths.TryGetValue(path, out string? mapped)) Add(path, mapped, entry.Length);
			for (int slash = path.IndexOf('/'); slash >= 0; slash = path.IndexOf('/', slash + 1))
			{
				// Keep the picker bounded even for packages containing many asset folders.
				if (roots.Count >= 4096) break;
				roots.Add(path[..(slash + 1)]);
			}
		}
		return new(roots.Order(StringComparer.OrdinalIgnoreCase).ToArray(), files, hash);
	}

	internal static IReadOnlyDictionary<string, string> Map(ZipArchive zip, ModInstallTarget target, string selectedRoot, CancellationToken cancellationToken = default)
	{
		if (selectedRoot.Length > 0 && (!selectedRoot.EndsWith('/') || !ModPathSafety.IsSafeRelativePath(selectedRoot[..^1])))
			throw UniversalModImports.Error("PackageRoot");
		ModPackageLimits limits = ModPackageLimits.For(target);
		if (zip.Entries.Count > limits.Entries) throw ModPackageFiles.Error("Size");
		Dictionary<string, string> mapped = new(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, bool> all = new(StringComparer.OrdinalIgnoreCase);
		long bytes = 0;
		foreach (ZipArchiveEntry entry in zip.Entries)
		{
			cancellationToken.ThrowIfCancellationRequested();
			bool directory = entry.FullName.EndsWith('/') || entry.FullName.EndsWith('\\');
			string normalized = entry.FullName.Replace('\\', '/');
			string path = directory ? normalized[..^1] : normalized;
			if (!ModPathSafety.IsSafeRelativePath(path) || path.Split('/').Length > 32 ||
				((entry.ExternalAttributes >> 16) & 0xf000) == 0xa000 || (entry.ExternalAttributes & (int)FileAttributes.ReparsePoint) != 0 ||
				!all.TryAdd(path, directory)) throw ModPackageFiles.Error("Layout");
			bytes = checked(bytes + entry.Length);
			if (entry.Length < 0 || entry.Length > limits.FileBytes || bytes > limits.TotalBytes) throw ModPackageFiles.Error("Size");
			if (!directory && path.StartsWith(selectedRoot, StringComparison.OrdinalIgnoreCase))
				mapped.Add(path, path[selectedRoot.Length..]);
		}
		foreach (string path in all.Keys)
			for (int slash = path.LastIndexOf('/'); slash >= 0; slash = path.LastIndexOf('/', slash - 1))
				if (all.TryGetValue(path[..slash], out bool directory) && !directory) throw UniversalModImports.Error("Collision");
		if (mapped.Count == 0 || !mapped.Keys.Any(path => target.AllowedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase)))
			throw UniversalModImports.Error("NoFiles");
		return mapped;
	}
}
