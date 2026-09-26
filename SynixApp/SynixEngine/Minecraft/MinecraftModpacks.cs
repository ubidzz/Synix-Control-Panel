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
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine.ModManagement;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;

namespace Synix_Control_Panel.SynixEngine.Minecraft;

internal sealed class MinecraftPreparedPack : IDisposable
{
	internal MinecraftStaging Staging { get; } = new();
	internal string Name { get; set; } = "";
	internal string Version { get; set; } = "";
	internal List<string> Notes { get; } = [];
	internal List<MinecraftFileChange> Changes { get; } = [];
	internal void Freeze(GameServer server)
	{
		Changes.AddRange(Staging.Changes(server));
		// A repeat import updates only the same pack's previously managed JARs.
		// Never remove custom configs, worlds, or files modified outside this transaction.
		MinecraftChangeReceipt? previous = MinecraftContentTransactions.History(server)
			.FirstOrDefault(item => item.Kind == "pack:" + Name && item.State == "Applied");
		if (previous == null) return;
		foreach (MinecraftChangedFile file in previous.Files.Where(file => file.After != null &&
			(file.Path.StartsWith("mods/", StringComparison.OrdinalIgnoreCase) ||
			 file.Path.StartsWith("plugins/", StringComparison.OrdinalIgnoreCase)) &&
			file.Path.EndsWith(".jar", StringComparison.OrdinalIgnoreCase) && !Staging.Files.ContainsKey(file.Path)))
		{
			if (MinecraftContentTransactions.HashFile(ModPathSafety.Resolve(server.InstallPath, file.Path)) != file.After)
				throw MinecraftContentTransactions.Error("Changed");
			Changes.Add(new(file.Path, null, null, file.After));
		}
	}
	public void Dispose() => Staging.Dispose();
}

internal static class MinecraftModpacks
{
	private static readonly HashSet<string> ContentRoots = new(StringComparer.OrdinalIgnoreCase)
		{ "mods", "plugins", "config", "defaultconfigs", "kubejs", "scripts" };
	private static readonly HashSet<string> ExecutableExtensions = new(StringComparer.OrdinalIgnoreCase)
		{ ".exe", ".com", ".bat", ".cmd", ".ps1", ".vbs", ".vbe", ".wsf", ".wsh", ".hta", ".lnk", ".url", ".msi", ".msp", ".scr", ".reg", ".dll" };
	private static readonly HttpClient Client = new(new HttpClientHandler { AllowAutoRedirect = false })
		{ Timeout = TimeSpan.FromMinutes(3) };

	internal static bool IsExecutableFile(string path) => ExecutableExtensions.Contains(Path.GetExtension(path));

	internal static bool IsContentPath(string path)
	{
		if (!ModPathSafety.IsSafeRelativePath(path) || IsExecutableFile(path)) return false;
		string[] parts = path.Replace('\\', '/').Split('/');
		if (parts.Length < 2 || !ContentRoots.Contains(parts[0])) return false;
		string extension = Path.GetExtension(path).ToLowerInvariant();
		if (parts[0].Equals("mods", StringComparison.OrdinalIgnoreCase)) return parts.Length == 2 && extension == ".jar";
		if (parts[0].Equals("plugins", StringComparison.OrdinalIgnoreCase) && parts.Length == 2 && extension == ".jar") return true;
		if (extension == ".js") return parts[0].Equals("kubejs", StringComparison.OrdinalIgnoreCase);
		return extension is not (".jar" or ".class");
	}

	internal static async Task<MinecraftPreparedPack> PrepareAsync(GameServer server, string path,
		IProgress<string>? progress = null, CancellationToken token = default, bool includeOptional = false)
	{
		if (!MinecraftControlProfile.IsJava(server)) throw MinecraftContentTransactions.Error("JavaOnly");
		ModPathSafety.EnsureNoLinks(path);
		if (new FileInfo(path).Length > MinecraftStaging.MaxTotalBytes) throw MinecraftContentTransactions.Error("Size");
		using ZipArchive zip = ZipFile.OpenRead(path);
		MinecraftStaging.ValidateArchive(zip);
		MinecraftPreparedPack pack = new() { Name = Path.GetFileNameWithoutExtension(path) };
		try
		{
			ZipArchiveEntry? index = zip.GetEntry("modrinth.index.json");
			if (index != null)
				await ReadMrpackAsync(server, zip, index, pack, progress, token, includeOptional).ConfigureAwait(false);
			else
				ReadServerZip(server, zip, pack, token);
			if (pack.Staging.Files.Count == 0) throw MinecraftContentTransactions.Error("PackEmpty");
			pack.Freeze(server);
			return pack;
		}
		catch { pack.Dispose(); throw; }
	}

	private static void CheckTarget(GameServer server, string path)
	{
		if (path.StartsWith("mods/", StringComparison.OrdinalIgnoreCase) &&
			(MinecraftPluginRuntime.IsPluginRuntime(server.MinecraftLoader) || server.MinecraftLoader == "Vanilla"))
			throw MinecraftContentTransactions.Error("LoaderMismatch");
		if (path.StartsWith("plugins/", StringComparison.OrdinalIgnoreCase) && !MinecraftPluginRuntime.IsPluginRuntime(server.MinecraftLoader))
			throw MinecraftContentTransactions.Error("LoaderMismatch");
	}

	private static void ReadServerZip(GameServer server, ZipArchive zip, MinecraftPreparedPack pack, CancellationToken token)
	{
		string[] names = zip.Entries.Where(entry => entry.Name.Length > 0).Select(entry => entry.FullName.Replace('\\', '/')).ToArray();
		if (names.Length == 0) throw MinecraftContentTransactions.Error("PackEmpty");
		string? wrapper = names.Select(name => name.Split('/')[0]).Distinct(StringComparer.OrdinalIgnoreCase).Count() == 1
			? names[0].Split('/')[0] + "/" : null;
		if (wrapper != null && ContentRoots.Contains(wrapper.TrimEnd('/'))) wrapper = null;
		if (zip.Entries.Any(entry => entry.Name == "manifest.json") &&
			!names.Any(name => name.EndsWith(".jar", StringComparison.OrdinalIgnoreCase)))
			throw MinecraftContentTransactions.Error("CurseForgeManifest");
		foreach (ZipArchiveEntry entry in zip.Entries)
		{
			token.ThrowIfCancellationRequested();
			if (entry.Name.Length == 0) continue;
			string path = entry.FullName.Replace('\\', '/');
			if (wrapper != null) path = path[wrapper.Length..];
			if (!IsContentPath(path)) { pack.Notes.Add(path); continue; }
			CheckTarget(server, path);
			using Stream input = entry.Open();
			pack.Staging.Add(path, input, entry.Length);
		}
	}

	private static async Task ReadMrpackAsync(GameServer server, ZipArchive zip, ZipArchiveEntry index,
		MinecraftPreparedPack pack, IProgress<string>? progress, CancellationToken token, bool includeOptional)
	{
		using JsonDocument document = JsonDocument.Parse(MinecraftStaging.ReadSmall(index));
		JsonElement root = document.RootElement;
		if (root.GetProperty("formatVersion").GetInt32() != 1 || root.GetProperty("game").GetString() != "minecraft")
			throw MinecraftContentTransactions.Error("Layout");
		pack.Name = root.GetProperty("name").GetString() ?? "";
		pack.Version = root.GetProperty("versionId").GetString() ?? "";
		if (pack.Name.Length is 0 or > 200 || pack.Version.Length > 100) throw MinecraftContentTransactions.Error("Layout");
		ValidateDependencies(server, root.GetProperty("dependencies"));
		if (root.GetProperty("files").GetArrayLength() > 10000) throw MinecraftContentTransactions.Error("Size");
		foreach (JsonElement file in root.GetProperty("files").EnumerateArray())
		{
			token.ThrowIfCancellationRequested();
			string path = file.GetProperty("path").GetString() ?? "";
			if (!ModPathSafety.IsSafeRelativePath(path)) throw MinecraftContentTransactions.Error("Layout");
			if (file.TryGetProperty("env", out JsonElement environment) && environment.TryGetProperty("server", out JsonElement support))
			{
				if (support.GetString() == "unsupported") { pack.Notes.Add(path); continue; }
				if (support.GetString() is not ("required" or "optional")) throw MinecraftContentTransactions.Error("Layout");
				if (support.GetString() == "optional" && !includeOptional) { pack.Notes.Add(path); continue; }
			}
			if (!IsContentPath(path)) { pack.Notes.Add(path); continue; }
			CheckTarget(server, path);
			long length = file.GetProperty("fileSize").GetInt64();
			if (length < 0 || length > MinecraftStaging.MaxFileBytes) throw MinecraftContentTransactions.Error("Size");
			string hash = file.GetProperty("hashes").GetProperty("sha512").GetString() ?? "";
			if (hash.Length != 128 || !hash.All(Uri.IsHexDigit)) throw MinecraftContentTransactions.Error("Layout");
			Uri? url = file.GetProperty("downloads").EnumerateArray()
				.Select(value => Uri.TryCreate(value.GetString(), UriKind.Absolute, out Uri? uri) ? uri : null)
				.FirstOrDefault(uri => uri != null && IsAllowedDownload(uri));
			if (url == null) throw MinecraftContentTransactions.Error("DownloadSource");
			progress?.Report(path);
			string downloaded = ModPathSafety.Resolve(pack.Staging.Root, "download");
			try
			{
				await DownloadAsync(url, downloaded, length, hash, token).ConfigureAwait(false);
				using FileStream input = File.OpenRead(downloaded);
				pack.Staging.Add(path, input, input.Length);
			}
			finally { if (File.Exists(downloaded)) File.Delete(downloaded); }
		}
		// Server overrides win over common files. Client overrides are never imported.
		foreach (string layer in new[] { "overrides/", "server-overrides/" })
			foreach (ZipArchiveEntry entry in zip.Entries)
			{
				token.ThrowIfCancellationRequested();
				string path = entry.FullName.Replace('\\', '/');
				if (entry.Name.Length == 0 || !path.StartsWith(layer, StringComparison.Ordinal)) continue;
				path = path[layer.Length..];
				if (!IsContentPath(path)) { pack.Notes.Add(path); continue; }
				CheckTarget(server, path);
				using Stream input = entry.Open();
				pack.Staging.Add(path, input, entry.Length, replace: true);
			}
	}

	internal static void ValidateDependencies(GameServer server, JsonElement dependencies)
	{
		if (dependencies.GetProperty("minecraft").GetString() != server.GameVersion)
			throw MinecraftContentTransactions.Error("PackVersion");
		int loaderCount = 0;
		foreach (JsonProperty dependency in dependencies.EnumerateObject())
		{
			if (dependency.Name == "minecraft") continue;
			string loader = dependency.Name switch
			{
				"fabric-loader" => "Fabric", "forge" => "Forge", "neoforge" => "NeoForge", _ => ""
			};
			if (++loaderCount > 1 || loader.Length == 0 || loader != server.MinecraftLoader ||
				dependency.Value.GetString() != server.MinecraftLoaderVersion)
				throw MinecraftContentTransactions.Error("PackVersion");
		}
		if (loaderCount == 0 && server.MinecraftLoader != "Vanilla") throw MinecraftContentTransactions.Error("PackVersion");
	}

	internal static bool IsAllowedDownload(Uri uri) =>
		uri.Scheme == "https" && uri.IsDefaultPort && uri.UserInfo.Length == 0 &&
		uri.Host is "cdn.modrinth.com" or "github.com" or "raw.githubusercontent.com" or "gitlab.com" or
			"release-assets.githubusercontent.com" or "objects.githubusercontent.com";

	internal static async Task DownloadAsync(Uri url, string path, long length, string sha512, CancellationToken token, HttpClient? client = null)
	{
		using CancellationTokenSource deadline = CancellationTokenSource.CreateLinkedTokenSource(token);
		deadline.CancelAfter(TimeSpan.FromMinutes(3));
		for (int redirects = 0; redirects <= 5; redirects++)
		{
			if (!IsAllowedDownload(url)) throw MinecraftContentTransactions.Error("DownloadSource");
			using HttpResponseMessage response = await (client ?? Client).GetAsync(url, HttpCompletionOption.ResponseHeadersRead, deadline.Token).ConfigureAwait(false);
			if ((int)response.StatusCode is >= 300 and < 400 && response.Headers.Location != null)
			{ url = new Uri(url, response.Headers.Location); continue; }
			response.EnsureSuccessStatusCode();
			if (response.Content.Headers.ContentLength is long declared && declared != length) throw MinecraftContentTransactions.Error("Size");
			using Stream input = await response.Content.ReadAsStreamAsync(deadline.Token).ConfigureAwait(false);
			using FileStream output = new(path, FileMode.CreateNew, FileAccess.Write);
			using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA512);
			byte[] buffer = new byte[81920];
			long received = 0;
			int count;
			while ((count = await input.ReadAsync(buffer, deadline.Token).ConfigureAwait(false)) != 0)
			{
				if ((received += count) > length) throw MinecraftContentTransactions.Error("Size");
				hash.AppendData(buffer, 0, count);
				await output.WriteAsync(buffer.AsMemory(0, count), deadline.Token).ConfigureAwait(false);
			}
			if (received != length || !Convert.ToHexString(hash.GetHashAndReset()).Equals(sha512, StringComparison.OrdinalIgnoreCase))
				throw MinecraftContentTransactions.Error("DownloadHash");
			return;
		}
		throw MinecraftContentTransactions.Error("DownloadSource");
	}
}
