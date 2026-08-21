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
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace Synix_Control_Panel.SynixApp.ServerHandler
{
	internal static class MinecraftMetadataService
	{
		internal const string VanillaLoader = "Vanilla";
		internal const string FabricLoader = "Fabric";
		internal const string ForgeLoader = "Forge";

		private const string MojangManifestUrl =
			"https://piston-meta.mojang.com/mc/game/version_manifest_v2.json";
		private const string FabricMetaBaseUrl = "https://meta.fabricmc.net/v2/versions";
		private const string ForgeMavenBaseUrl =
			"https://maven.minecraftforge.net/net/minecraftforge/forge";

		private static readonly HttpClient HttpClient = CreateHttpClient();
		private static readonly SemaphoreSlim CatalogLock = new(1, 1);
		private static readonly SemaphoreSlim ForgeCatalogLock = new(1, 1);
		private static MinecraftVersionCatalog? _cachedCatalog;
		private static DateTime _catalogExpiresUtc = DateTime.MinValue;
		private static IReadOnlyList<string>? _cachedForgeArtifactVersions;
		private static DateTime _forgeCatalogExpiresUtc = DateTime.MinValue;

		internal sealed record MinecraftVersionCatalog(
			string LatestRelease,
			IReadOnlyList<string> ReleaseVersions,
			IReadOnlyDictionary<string, string> MetadataUrls);

		internal sealed record MinecraftVersionMetadata(
			string Version,
			string ServerDownloadUrl,
			string ServerSha1,
			long ServerSize,
			int JavaMajorVersion);

		internal static string NormalizeLoader(string? loader)
		{
			if (string.Equals(loader, FabricLoader, StringComparison.OrdinalIgnoreCase))
				return FabricLoader;
			if (string.Equals(loader, ForgeLoader, StringComparison.OrdinalIgnoreCase))
				return ForgeLoader;

			return VanillaLoader;
		}

		internal static async Task<MinecraftVersionCatalog> GetVersionCatalogAsync(
			CancellationToken cancellationToken = default)
		{
			if (_cachedCatalog != null && DateTime.UtcNow < _catalogExpiresUtc)
				return _cachedCatalog;

			await CatalogLock.WaitAsync(cancellationToken).ConfigureAwait(false);
			try
			{
				if (_cachedCatalog != null && DateTime.UtcNow < _catalogExpiresUtc)
					return _cachedCatalog;

				string manifestJson = await HttpClient.GetStringAsync(
					MojangManifestUrl,
					cancellationToken).ConfigureAwait(false);

				using JsonDocument document = JsonDocument.Parse(manifestJson);
				JsonElement root = document.RootElement;
				string latestRelease = root.GetProperty("latest").GetProperty("release").GetString() ?? "";
				List<string> releases = [];
				Dictionary<string, string> metadataUrls = new(StringComparer.OrdinalIgnoreCase);

				foreach (JsonElement version in root.GetProperty("versions").EnumerateArray())
				{
					string? id = version.GetProperty("id").GetString();
					string? type = version.GetProperty("type").GetString();
					string? url = version.GetProperty("url").GetString();

					if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(url))
						continue;

					metadataUrls[id] = url;
					if (string.Equals(type, "release", StringComparison.OrdinalIgnoreCase))
						releases.Add(id);
				}

				_cachedCatalog = new MinecraftVersionCatalog(
					latestRelease,
					releases,
					metadataUrls);
				_catalogExpiresUtc = DateTime.UtcNow.AddMinutes(30);
				return _cachedCatalog;
			}
			finally
			{
				CatalogLock.Release();
			}
		}

		internal static async Task<string> ResolveVersionIdAsync(
			string? selectedVersion,
			CancellationToken cancellationToken = default)
		{
			MinecraftVersionCatalog catalog = await GetVersionCatalogAsync(cancellationToken)
				.ConfigureAwait(false);
			string version = selectedVersion?.Trim() ?? "";

			if (version.Length == 0 || version.Equals("latest", StringComparison.OrdinalIgnoreCase))
				return catalog.LatestRelease;

			if (!catalog.MetadataUrls.ContainsKey(version))
				throw new InvalidOperationException($"Minecraft version '{version}' was not found in Mojang's manifest.");

			return version;
		}

		internal static async Task<MinecraftVersionMetadata> GetVersionMetadataAsync(
			string? selectedVersion,
			CancellationToken cancellationToken = default)
		{
			MinecraftVersionCatalog catalog = await GetVersionCatalogAsync(cancellationToken)
				.ConfigureAwait(false);
			string version = await ResolveVersionIdAsync(selectedVersion, cancellationToken)
				.ConfigureAwait(false);

			if (!catalog.MetadataUrls.TryGetValue(version, out string? metadataUrl) ||
				string.IsNullOrWhiteSpace(metadataUrl))
			{
				throw new InvalidOperationException($"Mojang metadata is missing for Minecraft {version}.");
			}

			string versionJson = await HttpClient.GetStringAsync(metadataUrl, cancellationToken)
				.ConfigureAwait(false);
			using JsonDocument document = JsonDocument.Parse(versionJson);
			JsonElement root = document.RootElement;

			if (!root.TryGetProperty("downloads", out JsonElement downloads) ||
				!downloads.TryGetProperty("server", out JsonElement serverDownload))
			{
				throw new InvalidOperationException($"Minecraft {version} does not publish a dedicated server download.");
			}

			string downloadUrl = serverDownload.GetProperty("url").GetString() ?? "";
			string sha1 = serverDownload.TryGetProperty("sha1", out JsonElement sha1Element)
				? sha1Element.GetString() ?? ""
				: "";
			long size = serverDownload.TryGetProperty("size", out JsonElement sizeElement)
				? sizeElement.GetInt64()
				: 0;
			int javaMajor = ResolveJavaMajor(root, version);

			if (string.IsNullOrWhiteSpace(downloadUrl))
				throw new InvalidOperationException($"Minecraft {version} has an empty server download URL.");

			return new MinecraftVersionMetadata(version, downloadUrl, sha1, size, javaMajor);
		}

		internal static async Task<IReadOnlyList<string>> GetLoaderVersionsAsync(
			string? loader,
			string? selectedGameVersion,
			CancellationToken cancellationToken = default)
		{
			string normalizedLoader = NormalizeLoader(loader);
			if (normalizedLoader == VanillaLoader)
				return ["Official"];

			string gameVersion = await ResolveVersionIdAsync(selectedGameVersion, cancellationToken)
				.ConfigureAwait(false);

			return normalizedLoader == FabricLoader
				? await GetFabricLoaderVersionsAsync(gameVersion, cancellationToken).ConfigureAwait(false)
				: await GetForgeLoaderVersionsAsync(gameVersion, cancellationToken).ConfigureAwait(false);
		}

		internal static async Task<string> ResolveLoaderVersionAsync(
			string? loader,
			string? selectedGameVersion,
			string? selectedLoaderVersion,
			CancellationToken cancellationToken = default)
		{
			string normalizedLoader = NormalizeLoader(loader);
			IReadOnlyList<string> compatibleBuilds = await GetLoaderVersionsAsync(
				normalizedLoader,
				selectedGameVersion,
				cancellationToken).ConfigureAwait(false);

			return ResolveSelectedBuild(selectedLoaderVersion, compatibleBuilds, normalizedLoader);
		}

		internal static async Task<Uri> GetFabricServerJarUriAsync(
			string selectedGameVersion,
			string selectedLoaderVersion,
			CancellationToken cancellationToken = default)
		{
			string gameVersion = await ResolveVersionIdAsync(selectedGameVersion, cancellationToken)
				.ConfigureAwait(false);
			IReadOnlyList<string> compatibleLoaders = await GetFabricLoaderVersionsAsync(
				gameVersion,
				cancellationToken).ConfigureAwait(false);
			string loaderVersion = ResolveSelectedBuild(selectedLoaderVersion, compatibleLoaders, FabricLoader);

			string installersJson = await HttpClient.GetStringAsync(
				$"{FabricMetaBaseUrl}/installer",
				cancellationToken).ConfigureAwait(false);
			using JsonDocument installerDocument = JsonDocument.Parse(installersJson);
			JsonElement[] installers = installerDocument.RootElement.EnumerateArray().ToArray();
			string installerVersion = "";
			foreach (JsonElement installer in installers)
			{
				if (installer.TryGetProperty("stable", out JsonElement stable) && stable.GetBoolean())
				{
					installerVersion = installer.GetProperty("version").GetString() ?? "";
					break;
				}
			}

			if (installerVersion.Length == 0 && installers.Length > 0)
				installerVersion = installers[0].GetProperty("version").GetString() ?? "";
			if (installerVersion.Length == 0)
				throw new InvalidOperationException("Fabric did not publish a usable server installer version.");

			return new Uri(
				$"{FabricMetaBaseUrl}/loader/{Uri.EscapeDataString(gameVersion)}/" +
				$"{Uri.EscapeDataString(loaderVersion)}/{Uri.EscapeDataString(installerVersion)}/server/jar");
		}

		internal static async Task<Uri> GetForgeInstallerUriAsync(
			string selectedGameVersion,
			string selectedLoaderVersion,
			CancellationToken cancellationToken = default)
		{
			string gameVersion = await ResolveVersionIdAsync(selectedGameVersion, cancellationToken)
				.ConfigureAwait(false);
			IReadOnlyList<string> compatibleBuilds = await GetForgeLoaderVersionsAsync(
				gameVersion,
				cancellationToken).ConfigureAwait(false);
			string forgeVersion = ResolveSelectedBuild(selectedLoaderVersion, compatibleBuilds, ForgeLoader);
			IReadOnlyList<string> artifactVersions = await GetForgeArtifactVersionsAsync(cancellationToken)
				.ConfigureAwait(false);
			string? artifactVersion = artifactVersions.FirstOrDefault(version =>
				TryExtractForgeLoaderVersion(version, gameVersion, out string build) &&
				build.Equals(forgeVersion, StringComparison.OrdinalIgnoreCase));
			if (artifactVersion == null)
				throw new InvalidOperationException(
					$"Forge {forgeVersion} metadata disappeared for Minecraft {gameVersion}.");

			string escapedArtifactVersion = Uri.EscapeDataString(artifactVersion);

			return new Uri(
				$"{ForgeMavenBaseUrl}/{escapedArtifactVersion}/forge-{escapedArtifactVersion}-installer.jar");
		}

		private static async Task<IReadOnlyList<string>> GetFabricLoaderVersionsAsync(
			string gameVersion,
			CancellationToken cancellationToken)
		{
			string json = await HttpClient.GetStringAsync(
				$"{FabricMetaBaseUrl}/loader/{Uri.EscapeDataString(gameVersion)}",
				cancellationToken).ConfigureAwait(false);
			using JsonDocument document = JsonDocument.Parse(json);
			List<string> stable = [];
			List<string> all = [];

			foreach (JsonElement entry in document.RootElement.EnumerateArray())
			{
				if (!entry.TryGetProperty("loader", out JsonElement loaderData) ||
					!loaderData.TryGetProperty("version", out JsonElement versionElement))
					continue;

				string? version = versionElement.GetString();
				if (string.IsNullOrWhiteSpace(version) || all.Contains(version, StringComparer.OrdinalIgnoreCase))
					continue;

				all.Add(version);
				if (loaderData.TryGetProperty("stable", out JsonElement stableElement) && stableElement.GetBoolean())
					stable.Add(version);
			}

			return stable.Count > 0 ? stable : all;
		}

		private static async Task<IReadOnlyList<string>> GetForgeLoaderVersionsAsync(
			string gameVersion,
			CancellationToken cancellationToken)
		{
			IReadOnlyList<string> artifactVersions = await GetForgeArtifactVersionsAsync(cancellationToken)
				.ConfigureAwait(false);
			List<string> builds = artifactVersions
				.Where(version => TryExtractForgeLoaderVersion(version, gameVersion, out _))
				.Select(version =>
				{
					TryExtractForgeLoaderVersion(version, gameVersion, out string build);
					return build;
				})
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.Reverse()
				.ToList();

			return builds;
		}

		private static async Task<IReadOnlyList<string>> GetForgeArtifactVersionsAsync(
			CancellationToken cancellationToken)
		{
			if (_cachedForgeArtifactVersions != null && DateTime.UtcNow < _forgeCatalogExpiresUtc)
				return _cachedForgeArtifactVersions;

			await ForgeCatalogLock.WaitAsync(cancellationToken).ConfigureAwait(false);
			try
			{
				if (_cachedForgeArtifactVersions != null && DateTime.UtcNow < _forgeCatalogExpiresUtc)
					return _cachedForgeArtifactVersions;

				string xml = await HttpClient.GetStringAsync(
					$"{ForgeMavenBaseUrl}/maven-metadata.xml",
					cancellationToken).ConfigureAwait(false);
				XmlReaderSettings settings = new()
				{
					DtdProcessing = DtdProcessing.Prohibit,
					XmlResolver = null
				};

				using StringReader stringReader = new(xml);
				using XmlReader xmlReader = XmlReader.Create(stringReader, settings);
				XDocument document = XDocument.Load(xmlReader, LoadOptions.None);
				_cachedForgeArtifactVersions = document
					.Descendants("version")
					.Select(element => element.Value.Trim())
					.Where(version => version.Length > 0)
					.Distinct(StringComparer.OrdinalIgnoreCase)
					.ToList();
				_forgeCatalogExpiresUtc = DateTime.UtcNow.AddMinutes(30);
				return _cachedForgeArtifactVersions;
			}
			finally
			{
				ForgeCatalogLock.Release();
			}
		}

		private static bool TryExtractForgeLoaderVersion(
			string artifactVersion,
			string gameVersion,
			out string loaderVersion)
		{
			string standardPrefix = gameVersion + "-";
			string unobfuscatedPrefix = "default-" + gameVersion + "-";
			string prefix = artifactVersion.StartsWith(
				unobfuscatedPrefix,
				StringComparison.OrdinalIgnoreCase)
				? unobfuscatedPrefix
				: standardPrefix;

			if (!artifactVersion.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				loaderVersion = "";
				return false;
			}

			loaderVersion = artifactVersion[prefix.Length..];
			return loaderVersion.Length > 0;
		}

		private static string ResolveSelectedBuild(
			string? selectedVersion,
			IReadOnlyList<string> compatibleBuilds,
			string loaderName)
		{
			if (compatibleBuilds.Count == 0)
				throw new InvalidOperationException($"No compatible {loaderName} server build was found.");

			string selected = selectedVersion?.Trim() ?? "";
			if (selected.Length == 0 || selected.Equals("latest", StringComparison.OrdinalIgnoreCase))
				return compatibleBuilds[0];

			string? exact = compatibleBuilds.FirstOrDefault(
				build => build.Equals(selected, StringComparison.OrdinalIgnoreCase));
			if (exact == null)
				throw new InvalidOperationException(
					$"{loaderName} {selected} is not compatible with the selected Minecraft version.");

			return exact;
		}

		private static int ResolveJavaMajor(JsonElement versionMetadata, string version)
		{
			if (versionMetadata.TryGetProperty("javaVersion", out JsonElement javaVersion) &&
				javaVersion.TryGetProperty("majorVersion", out JsonElement majorVersion) &&
				majorVersion.TryGetInt32(out int javaMajor) && javaMajor > 0)
			{
				return javaMajor;
			}

			string[] versionParts = version.Split('.');
			if (versionParts.Length > 0 && int.TryParse(versionParts[0], out int calendarMajor) && calendarMajor >= 26)
				return 25;
			if (!TryParseReleaseVersion(version, out int minor, out int patch))
				return 8;
			if (minor > 20 || (minor == 20 && patch >= 5))
				return 21;
			if (minor >= 18)
				return 17;
			if (minor == 17)
				return 16;

			return 8;
		}

		private static bool TryParseReleaseVersion(string version, out int minor, out int patch)
		{
			minor = 0;
			patch = 0;
			string[] parts = version.Split('.');
			if (parts.Length < 2 || parts[0] != "1" || !int.TryParse(parts[1], out minor))
				return false;

			if (parts.Length > 2)
				int.TryParse(parts[2].Split('-')[0], out patch);
			return true;
		}

		private static HttpClient CreateHttpClient()
		{
			HttpClient client = new()
			{
				Timeout = TimeSpan.FromSeconds(30)
			};
			client.DefaultRequestHeaders.UserAgent.ParseAdd($"Synix-Control-Panel/{Application.ProductVersion}");
			return client;
		}
	}
}
