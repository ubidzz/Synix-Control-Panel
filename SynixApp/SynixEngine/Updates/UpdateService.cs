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
using Microsoft.Win32;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Synix_Control_Panel.SynixEngine
{
	public enum SynixInstallationKind
	{
		Development,
		Standalone,
		Setup,
		WinGet
	}

	public sealed record SynixInstallation(
		SynixInstallationKind Kind,
		string ExecutablePath,
		string? SetupInstallLocation)
	{
		public bool CanInstallUpdates =>
			Kind != SynixInstallationKind.Development;

		public string DisplayName => Kind switch
		{
			SynixInstallationKind.Standalone => "Standalone edition",
			SynixInstallationKind.Setup => "Setup edition",
			SynixInstallationKind.WinGet => "WinGet edition",
			_ => "Development build"
		};
	}

	public sealed record SynixReleaseAsset(
		string Name,
		long Size,
		Uri DownloadUri,
		string Sha256);

	public sealed record SynixReleaseInfo(
		Version Version,
		string VersionText,
		string Name,
		string Notes,
		Uri ReleaseUri,
		DateTimeOffset? PublishedAt,
		IReadOnlyList<SynixReleaseAsset> Assets);

	public sealed record SynixUpdateCheckResult(
		Version CurrentVersion,
		Version? AdvertisedVersion,
		SynixInstallation Installation,
		SynixReleaseInfo? Release,
		SynixReleaseAsset? Asset,
		string? Problem)
	{
		public bool UpdateAvailable =>
			AdvertisedVersion is not null &&
			AdvertisedVersion > CurrentVersion;

		public bool ReleaseReady =>
			UpdateAvailable &&
			Release is not null &&
			Asset is not null &&
			Release.Version == AdvertisedVersion;

		public bool CanInstall =>
			ReleaseReady && Installation.CanInstallUpdates;
	}

	public readonly record struct SynixUpdateDownloadProgress(
		long BytesReceived,
		long TotalBytes)
	{
		public int Percent => TotalBytes <= 0
			? 0
			: (int)Math.Clamp(
				BytesReceived * 100L / TotalBytes,
				0,
				100);
	}

	public partial class Core
	{
		public const string DevelopmentChannel = "Development";
		public const string StableChannel = "Stable";

		public static string UpdateChannel
		{
			get
			{
				Assembly assembly = typeof(Core).Assembly;
				return assembly
					.GetCustomAttributes<AssemblyMetadataAttribute>()
					.FirstOrDefault(attribute => string.Equals(
						attribute.Key,
						"SynixBuildChannel",
						StringComparison.OrdinalIgnoreCase))
					?.Value ?? DevelopmentChannel;
			}
		}

		public static bool IsOfficialRelease => IsOfficialChannel(
			UpdateChannel);

		public static bool IsOfficialChannel(string? channel)
		{
			return string.Equals(
				channel,
				StableChannel,
				StringComparison.OrdinalIgnoreCase);
		}
	}

	public partial class Core
	{
		public const string StandaloneAssetName = "Synix.Control.Panel.exe";
		public const string MsiAssetName = "SynixSetup.msi";
		public const string WinGetPackageId = "ubidzz.Synix";
		private const string InstalledExecutableName = "Synix Control Panel.exe";
		public static readonly Uri ReleasesUri = new(
			"https://github.com/ubidzz/Synix-Control-Panel/releases");

		internal const string MsiInstallRegistryKey =
			@"Software\ubidzz\Synix Control Panel";
		private const string LegacySetupUninstallKey =
			@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{D3E8B790-86E8-4485-B827-7A743AB72BDB}_is1";
		private static readonly Uri VersionUri = new(
			"https://raw.githubusercontent.com/ubidzz/Synix-Control-Panel/refs/heads/master/SynixApp/SynixEngine/version.txt");
		private static readonly Uri LatestReleaseApiUri = new(
			"https://api.github.com/repos/ubidzz/Synix-Control-Panel/releases/latest");
		private const int MaximumMetadataBytes = 2 * 1024 * 1024;
		private const long MaximumAssetBytes = 512L * 1024 * 1024;
		private const int MaximumReleaseNotesCharacters = 250_000;

		private static readonly HttpClient HttpClient = CreateHttpClient();

		public static async Task<SynixUpdateCheckResult> CheckForUpdatesAsync(
			Version currentVersion,
			CancellationToken cancellationToken = default)
		{
			SynixInstallation installation = DetectCurrentInstallation();
			try
			{
				SynixReleaseInfo release = await GetLatestReleaseAsync(
					cancellationToken);
				Version advertisedVersion = release.Version;
				SynixReleaseAsset? asset = advertisedVersion > currentVersion
					? SelectAsset(release, installation.Kind)
					: null;

				string? problem = advertisedVersion > currentVersion && asset is null
					? $"The verified {GetExpectedAssetName(installation.Kind)} download is not attached to this release yet."
					: null;

				return new SynixUpdateCheckResult(
					currentVersion,
					advertisedVersion,
					installation,
					release,
					asset,
					problem);
			}
			catch (Exception exception) when (
				exception is HttpRequestException or
				TaskCanceledException or
				JsonException or
				InvalidDataException)
			{
				Version advertisedVersion = await GetAdvertisedVersionAsync(
					cancellationToken);
				return new SynixUpdateCheckResult(
					currentVersion,
					advertisedVersion,
					installation,
					null,
					null,
					advertisedVersion > currentVersion
						? "The update was detected, but its verified release details could not be loaded. Try again in a moment."
						: null);
			}
		}

		public static SynixInstallation DetectCurrentInstallation()
		{
			string executablePath = Environment.ProcessPath ??
				throw new InvalidOperationException(
					"Synix could not determine its executable path.");

			if (!Core.IsOfficialRelease)
			{
				return DetectInstallation(
					executablePath,
					null,
					null,
					officialRelease: false);
			}

			string[] registrationKeys =
			[
				MsiInstallRegistryKey,
				LegacySetupUninstallKey
			];
			foreach (string registrationKey in registrationKeys)
			{
				if (!TryReadInstallRegistration(
					registrationKey,
					out string? installLocation,
					out string? installSource))
				{
					continue;
				}

				SynixInstallation candidate = DetectInstallation(
					executablePath,
					installLocation,
					installSource,
					officialRelease: true);
				if (candidate.Kind is SynixInstallationKind.Setup or
					SynixInstallationKind.WinGet)
				{
					return candidate;
				}
			}

			return DetectInstallation(
				executablePath,
				null,
				null,
				officialRelease: true);
		}

		internal static string? GetMsiInstalledExecutablePath()
		{
			if (!TryReadInstallRegistration(
				MsiInstallRegistryKey,
				out string? installLocation,
				out _))
			{
				return null;
			}

			try
			{
				return string.IsNullOrWhiteSpace(installLocation)
					? null
					: Path.GetFullPath(Path.Combine(
						installLocation,
						InstalledExecutableName));
			}
			catch (Exception exception) when (
				exception is ArgumentException or NotSupportedException or PathTooLongException)
			{
				return null;
			}
		}

		public static SynixInstallation DetectInstallation(
			string executablePath,
			string? setupInstallLocation,
			string? installSource,
			bool officialRelease)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
			string fullExecutablePath = Path.GetFullPath(executablePath);

			if (!officialRelease)
			{
				return new SynixInstallation(
					SynixInstallationKind.Development,
					fullExecutablePath,
					setupInstallLocation);
			}

			bool matchesSetup = false;
			if (!string.IsNullOrWhiteSpace(setupInstallLocation))
			{
				try
				{
					matchesSetup = PathsEqual(
						fullExecutablePath,
						Path.Combine(
							setupInstallLocation,
							InstalledExecutableName));
				}
				catch (Exception exception) when (
					exception is ArgumentException or NotSupportedException or PathTooLongException)
				{
					matchesSetup = false;
				}
			}

			SynixInstallationKind kind = matchesSetup
				? string.Equals(
					installSource,
					"WinGet",
					StringComparison.OrdinalIgnoreCase)
					? SynixInstallationKind.WinGet
					: SynixInstallationKind.Setup
				: SynixInstallationKind.Standalone;

			return new SynixInstallation(
				kind,
				fullExecutablePath,
				setupInstallLocation);
		}

		public static SynixReleaseAsset? SelectAsset(
			SynixReleaseInfo release,
			SynixInstallationKind installationKind)
		{
			string expectedName = GetExpectedAssetName(installationKind);

			return release.Assets.FirstOrDefault(asset =>
				string.Equals(
					asset.Name,
					expectedName,
					StringComparison.OrdinalIgnoreCase) &&
				!string.IsNullOrWhiteSpace(asset.Sha256));
		}

		public static Version GetCurrentVersion()
		{
			string rawVersion = Application.ProductVersion;
			return TryParseVersionText(rawVersion, out Version? version)
				? version!
				: new Version(0, 0, 0);
		}

		public static async Task DownloadUpdateAssetAsync(
			SynixReleaseAsset asset,
			string destinationPath,
			IProgress<SynixUpdateDownloadProgress>? progress = null,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(asset);
			ValidateDownloadUri(asset.DownloadUri);
			if (asset.Size <= 0 || asset.Size > MaximumAssetBytes)
				throw new InvalidDataException("The update download has an unsafe size.");

			string fullDestinationPath = Path.GetFullPath(destinationPath);
			string? directory = Path.GetDirectoryName(fullDestinationPath);
			if (string.IsNullOrWhiteSpace(directory))
				throw new InvalidOperationException("The update download folder is missing.");
			Directory.CreateDirectory(directory);

			using HttpResponseMessage response = await HttpClient.GetAsync(
				asset.DownloadUri,
				HttpCompletionOption.ResponseHeadersRead,
				cancellationToken);
			response.EnsureSuccessStatusCode();

			long responseLength = response.Content.Headers.ContentLength ?? asset.Size;
			if (responseLength <= 0 || responseLength > MaximumAssetBytes)
				throw new InvalidDataException("The update server returned an unsafe download size.");

			await using Stream input = await response.Content.ReadAsStreamAsync(
				cancellationToken);
			long received = 0;
			await using (FileStream output = new(
				fullDestinationPath,
				FileMode.CreateNew,
				FileAccess.Write,
				FileShare.None,
				81920,
				FileOptions.Asynchronous | FileOptions.WriteThrough))
			{
				byte[] buffer = new byte[81920];
				while (true)
				{
					int read = await input.ReadAsync(buffer, cancellationToken);
					if (read == 0)
						break;

					received += read;
					if (received > MaximumAssetBytes || received > asset.Size)
						throw new InvalidDataException("The update download exceeded its expected size.");

					await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
					progress?.Report(new SynixUpdateDownloadProgress(
						received,
						asset.Size));
				}

				await output.FlushAsync(cancellationToken);
				output.Flush(flushToDisk: true);
			}
			if (received != asset.Size)
				throw new InvalidDataException("The update download is incomplete.");

			await VerifyDownloadedAssetAsync(
				fullDestinationPath,
				asset.Name,
				asset.Sha256,
				cancellationToken);
		}

		public static async Task<SynixReleaseInfo> GetLatestReleaseAsync(
			CancellationToken cancellationToken = default)
		{
			string json = await DownloadSmallTextAsync(
				LatestReleaseApiUri,
				MaximumMetadataBytes,
				cancellationToken);
			return ParseReleaseJson(json);
		}

		public static SynixReleaseInfo ParseReleaseJson(string json)
		{
			using JsonDocument document = JsonDocument.Parse(json);
			JsonElement root = document.RootElement;
			if (root.GetProperty("draft").GetBoolean() ||
				root.GetProperty("prerelease").GetBoolean())
			{
				throw new InvalidDataException(
					"GitHub returned a draft or prerelease instead of a stable release.");
			}

			string tagName = root.GetProperty("tag_name").GetString() ?? string.Empty;
			string releaseName = root.TryGetProperty("name", out JsonElement nameElement)
				? nameElement.GetString() ?? string.Empty
				: string.Empty;
			if (!TryResolveReleaseVersion(tagName, releaseName, out Version? version))
				throw new InvalidDataException("The GitHub release has an invalid version.");

			string htmlUrl = root.GetProperty("html_url").GetString() ?? string.Empty;
			if (!Uri.TryCreate(htmlUrl, UriKind.Absolute, out Uri? releaseUri) ||
				releaseUri.Scheme != Uri.UriSchemeHttps ||
				!string.Equals(releaseUri.Host, "github.com", StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException("The GitHub release link is invalid.");
			}

			List<SynixReleaseAsset> assets = [];
			foreach (JsonElement assetElement in root.GetProperty("assets").EnumerateArray())
			{
				string name = assetElement.GetProperty("name").GetString() ?? string.Empty;
				long size = assetElement.GetProperty("size").GetInt64();
				string url = assetElement.GetProperty("browser_download_url").GetString() ?? string.Empty;
				string digest = assetElement.TryGetProperty("digest", out JsonElement digestElement)
					? digestElement.GetString() ?? string.Empty
					: string.Empty;

				if (size <= 0 || size > MaximumAssetBytes ||
					!Uri.TryCreate(url, UriKind.Absolute, out Uri? downloadUri) ||
					!TryNormalizeSha256(digest, out string sha256))
				{
					continue;
				}

				try
				{
					ValidateDownloadUri(downloadUri);
				}
				catch (InvalidDataException)
				{
					continue;
				}

				assets.Add(new SynixReleaseAsset(
					name,
					size,
					downloadUri,
					sha256));
			}

			string notes = root.TryGetProperty("body", out JsonElement bodyElement)
				? bodyElement.GetString() ?? string.Empty
				: string.Empty;
			if (notes.Length > MaximumReleaseNotesCharacters)
				notes = notes[..MaximumReleaseNotesCharacters];

			DateTimeOffset? publishedAt = null;
			if (root.TryGetProperty("published_at", out JsonElement publishedElement) &&
				publishedElement.ValueKind == JsonValueKind.String &&
				DateTimeOffset.TryParse(publishedElement.GetString(), out DateTimeOffset parsedDate))
			{
				publishedAt = parsedDate;
			}

			return new SynixReleaseInfo(
				version!,
				version!.ToString(3),
				string.IsNullOrWhiteSpace(releaseName) ? tagName : releaseName,
				notes,
				releaseUri,
				publishedAt,
				assets);
		}

		public static string BuildHighlights(string? markdown, int maximumItems = 6)
		{
			if (string.IsNullOrWhiteSpace(markdown))
				return "Open the full release notes on GitHub to see what changed.";

			List<string> highlights = markdown
				.Replace("\r\n", "\n", StringComparison.Ordinal)
				.Split('\n')
				.Select(line => line.Trim())
				.Where(line => line.StartsWith("- ", StringComparison.Ordinal) ||
					line.StartsWith("* ", StringComparison.Ordinal))
				.Select(line => CleanMarkdown(line[2..]))
				.Where(line => line.Length > 0)
				.Take(Math.Clamp(maximumItems, 1, 12))
				.ToList();

			if (highlights.Count == 0)
			{
				highlights = markdown
					.Replace("\r\n", "\n", StringComparison.Ordinal)
					.Split('\n')
					.Select(line => CleanMarkdown(line.Trim()))
					.Where(line => line.Length > 0 && !line.StartsWith('#'))
					.Take(3)
					.ToList();
			}

			return string.Join(
				Environment.NewLine,
				highlights.Select(item => $"• {Truncate(item, 180)}"));
		}

		public static string FormatReleaseNotes(string? markdown)
		{
			if (string.IsNullOrWhiteSpace(markdown))
				return "No release notes were provided.";

			IEnumerable<string> lines = markdown
				.Replace("\r\n", "\n", StringComparison.Ordinal)
				.Split('\n')
				.Select(line =>
				{
					string trimmed = line.TrimEnd();
					if (trimmed.StartsWith("### ", StringComparison.Ordinal))
						return trimmed[4..].ToUpperInvariant();
					if (trimmed.StartsWith("## ", StringComparison.Ordinal))
						return trimmed[3..].ToUpperInvariant();
					if (trimmed.StartsWith("# ", StringComparison.Ordinal))
						return trimmed[2..].ToUpperInvariant();
					if (trimmed.StartsWith("- ", StringComparison.Ordinal) ||
						trimmed.StartsWith("* ", StringComparison.Ordinal))
					{
						return "• " + CleanMarkdown(trimmed[2..]);
					}

					return CleanMarkdown(trimmed);
				});

			return string.Join(Environment.NewLine, lines);
		}

		private static async Task<Version> GetAdvertisedVersionAsync(
			CancellationToken cancellationToken)
		{
			string rawVersion = await DownloadSmallTextAsync(
				VersionUri,
				128,
				cancellationToken);
			if (!TryParseVersionText(rawVersion, out Version? version))
				throw new InvalidDataException("GitHub returned an invalid Synix version.");
			return version!;
		}

		private static async Task<string> DownloadSmallTextAsync(
			Uri uri,
			int maximumBytes,
			CancellationToken cancellationToken)
		{
			using HttpResponseMessage response = await HttpClient.GetAsync(
				uri,
				HttpCompletionOption.ResponseHeadersRead,
				cancellationToken);
			response.EnsureSuccessStatusCode();

			if (response.Content.Headers.ContentLength > maximumBytes)
				throw new InvalidDataException("The update metadata is unexpectedly large.");

			await using Stream stream = await response.Content.ReadAsStreamAsync(
				cancellationToken);
			using MemoryStream buffer = new();
			byte[] chunk = new byte[8192];
			while (true)
			{
				int read = await stream.ReadAsync(chunk, cancellationToken);
				if (read == 0)
					break;
				if (buffer.Length + read > maximumBytes)
					throw new InvalidDataException("The update metadata is unexpectedly large.");
				buffer.Write(chunk, 0, read);
			}

			return Encoding.UTF8
				.GetString(buffer.ToArray())
				.TrimStart('\uFEFF');
		}

		private static async Task VerifyDownloadedAssetAsync(
			string path,
			string assetName,
			string expectedSha256,
			CancellationToken cancellationToken)
		{
			await using FileStream stream = new(
				path,
				FileMode.Open,
				FileAccess.Read,
				FileShare.Read,
				81920,
				FileOptions.Asynchronous | FileOptions.SequentialScan);
			byte[] actualHash = await SHA256.HashDataAsync(stream, cancellationToken);
			byte[] expectedHash = Convert.FromHexString(expectedSha256);
			bool matches = CryptographicOperations.FixedTimeEquals(
				actualHash,
				expectedHash);
			CryptographicOperations.ZeroMemory(actualHash);
			CryptographicOperations.ZeroMemory(expectedHash);

			if (!matches)
				throw new InvalidDataException("The update failed its SHA-256 safety check.");

			stream.Position = 0;
			ValidateDownloadedAssetHeader(stream, assetName);
		}

		internal static void ValidateDownloadedAssetHeader(
			Stream stream,
			string assetName)
		{
			Span<byte> header = stackalloc byte[8];
			int bytesRead = stream.Read(header);
			string extension = Path.GetExtension(assetName);
			if (string.Equals(extension, ".exe", StringComparison.OrdinalIgnoreCase))
			{
				if (bytesRead < 2 || header[0] != (byte)'M' || header[1] != (byte)'Z')
					throw new InvalidDataException("The downloaded standalone update is not a valid Windows executable.");
				return;
			}

			ReadOnlySpan<byte> msiHeader = [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1];
			if (string.Equals(extension, ".msi", StringComparison.OrdinalIgnoreCase))
			{
				if (bytesRead < msiHeader.Length || !header.SequenceEqual(msiHeader))
					throw new InvalidDataException("The downloaded Setup update is not a valid Windows Installer package.");
				return;
			}

			throw new InvalidDataException("The update uses an unsupported file type.");
		}

		internal static bool TryResolveReleaseVersion(
			string? tagName,
			string? releaseName,
			out Version? version)
		{
			string cleanedTag = (tagName ?? string.Empty)
				.Trim()
				.TrimStart('v', 'V');
			Match tagMatch = Regex.Match(
				cleanedTag,
				@"^(?<major>\d+)\.(?<minor>\d+)\.(?<build>\d+)$",
				RegexOptions.CultureInvariant);
			if (TryCreateThreePartVersion(tagMatch, out version))
				return true;

			Match nameMatch = Regex.Match(
				releaseName ?? string.Empty,
				@"(?<!\d)(?<major>\d+)\.(?<minor>\d+)\.(?<build>\d+)(?!\d)",
				RegexOptions.CultureInvariant);
			if (TryCreateThreePartVersion(nameMatch, out version))
				return true;

			return TryParseVersionText(tagName, out version);
		}

		private static bool TryCreateThreePartVersion(
			Match match,
			out Version? version)
		{
			if (match.Success &&
				int.TryParse(match.Groups["major"].Value, out int major) &&
				int.TryParse(match.Groups["minor"].Value, out int minor) &&
				int.TryParse(match.Groups["build"].Value, out int build))
			{
				version = new Version(major, minor, build);
				return true;
			}

			version = null;
			return false;
		}

		private static string GetExpectedAssetName(
			SynixInstallationKind installationKind)
		{
			return installationKind == SynixInstallationKind.Standalone
				? StandaloneAssetName
				: MsiAssetName;
		}

		public static bool TryParseVersionText(
			string? rawVersion,
			out Version? version)
		{
			string cleaned = (rawVersion ?? string.Empty)
				.Trim()
				.TrimStart('\uFEFF')
				.Trim()
				.TrimStart('v', 'V');
			if (!Version.TryParse(cleaned, out Version? parsed))
			{
				version = null;
				return false;
			}

			version = new Version(
				parsed.Major,
				Math.Max(0, parsed.Minor),
				Math.Max(0, parsed.Build));
			return true;
		}

		private static bool TryNormalizeSha256(
			string? digest,
			out string sha256)
		{
			sha256 = string.Empty;
			const string prefix = "sha256:";
			if (string.IsNullOrWhiteSpace(digest) ||
				!digest.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			string candidate = digest[prefix.Length..].Trim();
			if (candidate.Length != 64 ||
				candidate.Any(character => !Uri.IsHexDigit(character)))
			{
				return false;
			}

			sha256 = candidate.ToLowerInvariant();
			return true;
		}

		private static void ValidateDownloadUri(Uri uri)
		{
			if (uri.Scheme != Uri.UriSchemeHttps ||
				!string.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException(
					"The update download does not point to the official GitHub release host.");
			}
		}

		private static bool TryReadInstallRegistration(
			string registryPath,
			out string? installLocation,
			out string? installSource)
		{
			installLocation = null;
			installSource = null;
			try
			{
				using RegistryKey? key = Registry.CurrentUser.OpenSubKey(
					registryPath,
					writable: false);
				if (key is null)
					return false;

				installLocation = key.GetValue("InstallLocation") as string;
				installSource = key.GetValue("SynixInstallSource") as string;
				if (string.IsNullOrWhiteSpace(installLocation))
				{
					string? uninstallString = key.GetValue("UninstallString") as string;
					installLocation = TryGetDirectoryFromCommand(uninstallString);
				}

				return !string.IsNullOrWhiteSpace(installLocation);
			}
			catch (Exception exception) when (
				exception is SecurityException or UnauthorizedAccessException)
			{
				installLocation = null;
				installSource = null;
				return false;
			}
		}

		private static string? TryGetDirectoryFromCommand(string? command)
		{
			if (string.IsNullOrWhiteSpace(command))
				return null;

			string trimmed = command.Trim();
			string executable = trimmed.StartsWith('"')
				? trimmed[1..].Split('"', 2)[0]
				: trimmed.Split(' ', 2)[0];
			return Path.GetDirectoryName(executable);
		}

		private static bool PathsEqual(string? left, string? right)
		{
			if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
				return false;

			try
			{
				return string.Equals(
					Path.TrimEndingDirectorySeparator(Path.GetFullPath(left)),
					Path.TrimEndingDirectorySeparator(Path.GetFullPath(right)),
					StringComparison.OrdinalIgnoreCase);
			}
			catch (Exception exception) when (
				exception is ArgumentException or NotSupportedException or PathTooLongException)
			{
				return false;
			}
		}

		private static string CleanMarkdown(string text)
		{
			StringBuilder result = new(text.Length);
			bool insideLinkText = false;
			for (int index = 0; index < text.Length; index++)
			{
				char character = text[index];
				if (character is '*' or '`')
					continue;
				if (character == '[')
				{
					insideLinkText = true;
					continue;
				}
				if (character == ']' && insideLinkText)
				{
					insideLinkText = false;
					if (index + 1 < text.Length && text[index + 1] == '(')
					{
						int closing = text.IndexOf(')', index + 2);
						if (closing >= 0)
							index = closing;
					}
					continue;
				}
				result.Append(character);
			}

			return result.ToString().Trim();
		}

		private static string Truncate(string value, int maximumLength)
		{
			return value.Length <= maximumLength
				? value
				: value[..(maximumLength - 1)].TrimEnd() + "…";
		}

		private static HttpClient CreateHttpClient()
		{
			HttpClient client = new()
			{
				Timeout = TimeSpan.FromSeconds(15)
			};
			client.DefaultRequestHeaders.UserAgent.Add(
				new ProductInfoHeaderValue("Synix-Control-Panel", "1.0"));
			return client;
		}
	}
}
