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
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Synix_Control_Panel.SynixEngine.ModManagement;

internal sealed record ArkModPackageInfo(string ModId, string Name, string Version, string DescriptorPath);

// Reads untrusted package metadata only. It does not extract/execute content, register a
// provider installation, or claim that a ZIP's bytes are the version ARK will download.
internal static class ArkModPackageReader
{
	internal const string Ascended = "ARK: Survival Ascended";
	internal const string Evolved = "ARK: Survival Evolved";
	internal const int MaximumEntries = 65536;
	internal const int MaximumDescriptors = 100;
	internal const int MaximumDescriptorBytes = 256 * 1024;
	private const int MaximumDepth = 24;
	private static readonly Regex ProjectId = new(@"(?:^|[&\s])cf_ugcID=([1-9][0-9]{0,9})(?=$|[&\s])",
		RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));

	internal static bool Supports(string game, ModInstallTarget? target) =>
		(game.Equals(Ascended, StringComparison.OrdinalIgnoreCase) && target is { Mode: ModTargetMode.ArgumentIds } &&
		 target.ArgumentName.Equals("-mods", StringComparison.OrdinalIgnoreCase) && target.ProviderName.Equals("CurseForge", StringComparison.OrdinalIgnoreCase)) ||
		(game.Equals(Evolved, StringComparison.OrdinalIgnoreCase) && target is { Mode: ModTargetMode.ConfigurationIds } &&
		 target.ProviderName.Equals("Steam Workshop", StringComparison.OrdinalIgnoreCase) &&
		 target.IdStores.Any(store => store.Key == "ActiveMods" && store.Section == "ServerSettings" &&
			store.RelativePath.Replace('\\', '/') == "ShooterGame/Saved/Config/WindowsServer/GameUserSettings.ini"));

	internal static IReadOnlyList<ArkModPackageInfo> Read(string sourcePath, CancellationToken cancellationToken = default) =>
		Read(sourcePath, Ascended, cancellationToken);

	internal static IReadOnlyList<ArkModPackageInfo> Read(string sourcePath, string game, CancellationToken cancellationToken = default)
	{
		if (!game.Equals(Ascended, StringComparison.OrdinalIgnoreCase) && !game.Equals(Evolved, StringComparison.OrdinalIgnoreCase))
			throw Error("Game");
		bool evolved = game.Equals(Evolved, StringComparison.OrdinalIgnoreCase);
		ModPathSafety.EnsureNoLinks(sourcePath);
		if (Directory.Exists(sourcePath)) return ReadFolder(sourcePath, evolved, cancellationToken);
		if (!Path.GetExtension(sourcePath).Equals(".zip", StringComparison.OrdinalIgnoreCase)) throw Error("Source");
		using FileStream source = new(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
		using ZipArchive zip = new(source, ZipArchiveMode.Read);
		if (zip.Entries.Count > MaximumEntries) throw Error("Limit");
		Dictionary<string, PackageEntry> entries = new(StringComparer.OrdinalIgnoreCase);
		foreach (ZipArchiveEntry entry in zip.Entries)
		{
			cancellationToken.ThrowIfCancellationRequested();
			bool directory = entry.FullName.EndsWith('/') || entry.FullName.EndsWith('\\');
			string relative = directory ? entry.FullName[..^1] : entry.FullName;
			if (((entry.ExternalAttributes >> 16) & 0xf000) == 0xa000 ||
				(entry.ExternalAttributes & (int)FileAttributes.ReparsePoint) != 0) throw Error("Layout");
			AddEntry(entries, relative, directory, entry.Length, entry.Open);
		}
		return ReadDescriptors(entries, evolved, cancellationToken);
	}

	private static IReadOnlyList<ArkModPackageInfo> ReadFolder(string root, bool evolved, CancellationToken cancellationToken)
	{
		Dictionary<string, PackageEntry> entries = new(StringComparer.OrdinalIgnoreCase);
		Stack<string> pending = new();
		pending.Push(root);
		while (pending.TryPop(out string? folder))
		{
			cancellationToken.ThrowIfCancellationRequested();
			foreach (string path in Directory.EnumerateFileSystemEntries(folder))
			{
				cancellationToken.ThrowIfCancellationRequested();
				ModPathSafety.EnsureNoLinks(path);
				bool directory = Directory.Exists(path);
				AddEntry(entries, Path.GetRelativePath(root, path), directory,
					directory ? 0 : new FileInfo(path).Length, () =>
					{
						ModPathSafety.EnsureNoLinks(path);
						return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
					});
				if (directory) pending.Push(path);
			}
		}
		return ReadDescriptors(entries, evolved, cancellationToken);
	}

	private static void AddEntry(Dictionary<string, PackageEntry> entries, string path, bool directory, long length, Func<Stream> open)
	{
		string relative = path.Replace('\\', '/');
		if (!ModPathSafety.IsSafeRelativePath(relative) || relative.Split('/').Length > MaximumDepth ||
			length < 0 || !entries.TryAdd(relative, new(relative, directory, length, open))) throw Error("Layout");
		if (entries.Count > MaximumEntries) throw Error("Limit");
	}

	private static IReadOnlyList<ArkModPackageInfo> ReadDescriptors(Dictionary<string, PackageEntry> entries, bool evolved, CancellationToken cancellationToken)
	{
		if (entries.Values.Any(entry => !entry.Directory && entry.Path.EndsWith(evolved ? ".uplugin" : ".mod", StringComparison.OrdinalIgnoreCase)))
			throw new InvalidDataException(LocalizationManager.Get("ArkPackages.Error.WrongEdition"));
		PackageEntry[] descriptors = entries.Values.Where(entry => !entry.Directory &&
			entry.Path.EndsWith(evolved ? ".mod" : ".uplugin", StringComparison.OrdinalIgnoreCase)).OrderBy(entry => entry.Path, StringComparer.OrdinalIgnoreCase).ToArray();
		if (descriptors.Length == 0) throw evolved ? EvolvedError() : Error("Descriptor");
		if (descriptors.Length > MaximumDescriptors) throw Error("Limit");
		// Reject file/directory collisions even when the ZIP omits directory entries.
		foreach (PackageEntry entry in entries.Values)
		{
			cancellationToken.ThrowIfCancellationRequested();
			for (int slash = entry.Path.LastIndexOf('/'); slash >= 0; slash = entry.Path.LastIndexOf('/', slash - 1))
				if (entries.TryGetValue(entry.Path[..slash], out PackageEntry? parent) && !parent.Directory) throw Error("Layout");
		}
		List<ArkModPackageInfo> result = [];
		foreach (PackageEntry descriptor in descriptors)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (evolved)
			{
				ArkModPackageInfo mod = ReadEvolvedDescriptor(descriptor, entries);
				if (result.Any(previous => previous.ModId == mod.ModId)) throw Error("DuplicateMod");
				result.Add(mod);
				continue;
			}
			using JsonDocument json = ReadJson(descriptor);
			if (json.RootElement.ValueKind != JsonValueKind.Object) throw Error("Metadata");
			Dictionary<string, JsonElement> fields = new(StringComparer.OrdinalIgnoreCase);
			foreach (JsonProperty property in json.RootElement.EnumerateObject())
				if (!fields.TryAdd(property.Name, property.Value)) throw Error("Metadata");
			string GetText(string key) => fields.TryGetValue(key, out JsonElement value) && value.ValueKind == JsonValueKind.String
				? value.GetString() ?? string.Empty : string.Empty;
			if (!IsAsaMarketplaceUrl(GetText("MarketplaceURL"))) throw Error("Game");
			MatchCollection matches = ProjectId.Matches(GetText("Description"));
			if (matches.Count != 1 || !uint.TryParse(matches[0].Groups[1].Value, NumberStyles.None,
				CultureInfo.InvariantCulture, out uint modId) || modId == 0) throw Error("Id");
			string prefix = descriptor.Path.Contains('/') ? descriptor.Path[..(descriptor.Path.LastIndexOf('/') + 1)] : string.Empty;
			ValidateServerContent(entries, prefix);
			string name = CleanDisplay(GetText("FriendlyName"), 160);
			if (string.IsNullOrWhiteSpace(name)) name = Path.GetFileNameWithoutExtension(descriptor.Path);
			ArkModPackageInfo info = new(modId.ToString(CultureInfo.InvariantCulture), name,
				CleanDisplay(GetText("VersionName"), 100), descriptor.Path);
			// Two versions of one project in a bundle are ambiguous, not an implicit update choice.
			if (result.Any(previous => previous.ModId == info.ModId)) throw Error("DuplicateMod");
			result.Add(info);
		}
		return result;
	}

	private static void ValidateServerContent(Dictionary<string, PackageEntry> entries, string prefix)
	{
		string serverPrefix = prefix + "Content/Paks/WindowsServer/";
		PackageEntry[] payload = entries.Values.Where(entry => !entry.Directory && entry.Length > 0 &&
			entry.Path.StartsWith(serverPrefix, StringComparison.OrdinalIgnoreCase)).ToArray();
		if (!payload.Any(entry => entry.Path.EndsWith(".pak", StringComparison.OrdinalIgnoreCase))) throw Error("ServerBuild");
		foreach (PackageEntry entry in payload)
		{
			string extension = Path.GetExtension(entry.Path);
			if (!extension.Equals(".ucas", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".utoc", StringComparison.OrdinalIgnoreCase)) continue;
			string other = Path.ChangeExtension(entry.Path, extension.Equals(".ucas", StringComparison.OrdinalIgnoreCase) ? ".utoc" : ".ucas");
			if (!payload.Any(candidate => candidate.Path.Equals(other, StringComparison.OrdinalIgnoreCase))) throw Error("ServerBuild");
		}
	}

	private static JsonDocument ReadJson(PackageEntry entry)
	{
		byte[] data = ReadMetadata(entry);
		int offset = data.Length >= 3 && data[0] == 0xef && data[1] == 0xbb && data[2] == 0xbf ? 3 : 0;
		try { return JsonDocument.Parse(data.AsMemory(offset), new JsonDocumentOptions { MaxDepth = 16 }); }
		catch (JsonException) { throw Error("Metadata"); }
	}

	private static byte[] ReadMetadata(PackageEntry entry)
	{
		if (entry.Length is <= 0 or > MaximumDescriptorBytes) throw Error("Limit");
		using Stream input = entry.Open();
		using MemoryStream bytes = new();
		ModPackageFiles.CopyExactBounded(input, bytes, entry.Length);
		return bytes.ToArray();
	}

	private static ArkModPackageInfo ReadEvolvedDescriptor(PackageEntry descriptor, Dictionary<string, PackageEntry> entries)
	{
		try
		{
			using BinaryReader reader = new(new MemoryStream(ReadMetadata(descriptor)), Encoding.UTF8);
			ulong id = reader.ReadUInt64();
			if (id == 0) throw EvolvedError();
			string modId = id.ToString(CultureInfo.InvariantCulture);
			string name = ReadUnrealString(reader);
			_ = ReadUnrealString(reader); // Embedded paths are metadata only; never follow them.
			int mapCount = reader.ReadInt32();
			if (mapCount is < 0 or > 256) throw EvolvedError();
			for (int index = 0; index < mapCount; index++) _ = ReadUnrealString(reader);
			if (reader.ReadUInt32() != 0xff22ff33 || reader.ReadInt32() != 2) throw EvolvedError();
			_ = reader.ReadByte();
			int metadataCount = reader.ReadInt32();
			if (metadataCount is < 0 or > 256) throw EvolvedError();
			HashSet<string> keys = new(StringComparer.OrdinalIgnoreCase);
			for (int index = 0; index < metadataCount; index++)
			{
				if (!keys.Add(ReadUnrealString(reader))) throw EvolvedError();
				_ = ReadUnrealString(reader);
			}
			if (reader.BaseStream.Position != reader.BaseStream.Length) throw EvolvedError();
			string parent = descriptor.Path.Contains('/') ? descriptor.Path[..(descriptor.Path.LastIndexOf('/') + 1)] : string.Empty;
			string fileName = Path.GetFileName(descriptor.Path);
			string stem = Path.GetFileNameWithoutExtension(fileName);
			if (stem.Length > 0 && stem.All(char.IsAsciiDigit) && stem != modId) throw EvolvedError();
			// Installed exports use <id>.mod beside <id>/. Some prepared downloads use
			// a .mod marker inside the content folder. Neither case relies on the ZIP name.
			string content = fileName.Equals(".mod", StringComparison.OrdinalIgnoreCase) ? parent : parent + modId + "/";
			if (!entries.TryGetValue(content + "mod.info", out PackageEntry? info) || info.Directory ||
				!entries.Values.Any(entry => !entry.Directory && entry.Length > 0 && entry.Path.StartsWith(content, StringComparison.OrdinalIgnoreCase) &&
					(Path.GetExtension(entry.Path).ToLowerInvariant() is ".uasset" or ".umap" or ".pak"))) throw EvolvedError();
			using BinaryReader infoReader = new(new MemoryStream(ReadMetadata(info)), Encoding.UTF8);
			string infoName = ReadUnrealString(infoReader);
			int infoMaps = infoReader.ReadInt32();
			if (infoMaps is < 0 or > 256) throw EvolvedError();
			for (int index = 0; index < infoMaps; index++) _ = ReadUnrealString(infoReader);
			if (!string.IsNullOrWhiteSpace(infoName)) name = infoName;
			return new(modId, string.IsNullOrWhiteSpace(name) ? modId : CleanDisplay(name, 160),
				LocalizationManager.Get("ModManager.Known.ProviderManaged"), descriptor.Path);
		}
		catch (EndOfStreamException) { throw EvolvedError(); }
		catch (DecoderFallbackException) { throw EvolvedError(); }
	}

	private static string ReadUnrealString(BinaryReader reader)
	{
		int length = reader.ReadInt32();
		if (length == 0) return string.Empty;
		if (length is < -4096 or > 4096) throw EvolvedError();
		bool wide = length < 0;
		int count = Math.Abs(length) * (wide ? 2 : 1);
		byte[] bytes = reader.ReadBytes(count);
		int terminator = wide ? 2 : 1;
		if (bytes.Length != count || bytes[^1] != 0 || (wide && bytes[^2] != 0)) throw EvolvedError();
		return (wide ? (Encoding)new UnicodeEncoding(false, false, true) : new UTF8Encoding(false, true)).GetString(bytes, 0, count - terminator);
	}

	private static InvalidDataException EvolvedError() => new(LocalizationManager.Get("AsePackages.Error.Layout"));

	private static bool IsAsaMarketplaceUrl(string value) =>
		Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) && uri.Scheme == Uri.UriSchemeHttps &&
		uri.IsDefaultPort && string.IsNullOrEmpty(uri.UserInfo) &&
		(uri.Host.Equals("curseforge.com", StringComparison.OrdinalIgnoreCase) ||
		 uri.Host.Equals("www.curseforge.com", StringComparison.OrdinalIgnoreCase) ||
		 uri.Host.Equals("legacy.curseforge.com", StringComparison.OrdinalIgnoreCase)) &&
		uri.AbsolutePath.StartsWith("/ark-survival-ascended/mods/", StringComparison.OrdinalIgnoreCase) &&
		uri.AbsolutePath.Length > "/ark-survival-ascended/mods/".Length;

	private static string CleanDisplay(string text, int limit) =>
		string.Concat(text.Take(limit).Select(character => char.IsControl(character) ? ' ' : character)).Trim();
	private static InvalidDataException Error(string key) => new(LocalizationManager.Get("AsaPackages.Error." + key));
	private sealed record PackageEntry(string Path, bool Directory, long Length, Func<Stream> Open);
}
