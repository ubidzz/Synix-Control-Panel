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
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;

namespace Synix_Control_Panel.SynixApp.ServerHandler
{
	internal static class OxideRuntimeManager
	{
		internal const string FrameworkName = "Oxide";
		internal const string VanillaFrameworkName = "Vanilla";
		internal const string FailedVersion = "Install failed";
		internal const string VanillaRestoreRequiredVersion = "Validation required";
		private const string RustDefinitionId = "rust";
		private const string RustAppId = "258550";
		private const string WindowsAssetName = "Oxide.Rust.zip";
		private const long MaximumDownloadBytes = 128L * 1024 * 1024;
		private const long MaximumExtractedBytes = 512L * 1024 * 1024;
		private const int MaximumArchiveEntries = 4096;
		private static readonly Uri LatestReleaseUri = new(
			"https://api.github.com/repos/OxideMod/Oxide.Rust/releases/latest");
		private static readonly HttpClient Client = CreateClient();

		internal static bool IsEnabled(GameServer server, GameInfo definition)
		{
			return IsTrustedRustDefinition(definition) &&
				definition.SupportedServerFrameworks.Any(framework =>
					framework.Equals(FrameworkName, StringComparison.OrdinalIgnoreCase)) &&
				string.Equals(
					server.ServerFramework,
					FrameworkName,
					StringComparison.OrdinalIgnoreCase);
		}

		internal static bool RequiresVanillaRestore(
			GameServer server,
			GameInfo definition)
		{
			return IsTrustedRustDefinition(definition) &&
				string.Equals(
					server.ServerFramework,
					VanillaFrameworkName,
					StringComparison.OrdinalIgnoreCase) &&
				string.Equals(
					server.ServerFrameworkVersion,
					VanillaRestoreRequiredVersion,
					StringComparison.OrdinalIgnoreCase);
		}

		private static bool IsTrustedRustDefinition(GameInfo definition) =>
			definition.DefinitionId.Equals(RustDefinitionId, StringComparison.OrdinalIgnoreCase) &&
			definition.AppID == RustAppId;

		internal static async Task<string> InstallOrUpdateAsync(
			GameServer server,
			GameInfo definition,
			Action<string, Color>? log = null,
			CancellationToken cancellationToken = default)
		{
			if (!IsEnabled(server, definition))
				return server.ServerFrameworkVersion ?? "Official";
			if (!Directory.Exists(server.InstallPath))
				throw new DirectoryNotFoundException("The Rust server folder does not exist.");

			server.ServerFrameworkVersion = FailedVersion;
			log?.Invoke("[OXIDE] Checking the official Oxide.Rust release...", Color.Cyan);
			OxideRelease release = await GetLatestReleaseAsync(cancellationToken);
			string tempRoot = Path.Combine(
				Path.GetTempPath(),
				"Synix",
				"Oxide",
				Guid.NewGuid().ToString("N"));
			string archivePath = Path.Combine(tempRoot, WindowsAssetName);
			string stagingPath = Path.Combine(tempRoot, "staging");
			string rollbackPath = Path.Combine(tempRoot, "rollback");

			try
			{
				Directory.CreateDirectory(tempRoot);
				await DownloadAsync(
					release.DownloadUri,
					archivePath,
					cancellationToken);
				string actualDigest = await ComputeSha256Async(archivePath, cancellationToken);
				if (!CryptographicOperations.FixedTimeEquals(
					Convert.FromHexString(actualDigest),
					Convert.FromHexString(release.Sha256)))
				{
					throw new InvalidDataException(
						"The Oxide download did not match the SHA-256 digest published by GitHub.");
				}

				ExtractArchiveSafely(archivePath, stagingPath);
				if (!Directory.Exists(Path.Combine(stagingPath, "RustDedicated_Data")))
					throw new InvalidDataException("The official Oxide archive has an unexpected layout.");

				ApplyOverlayWithRollback(stagingPath, server.InstallPath, rollbackPath);
				server.ServerFrameworkVersion = release.Version;
				log?.Invoke(
					$"[OXIDE] Official Oxide.Rust {release.Version} installed. Synix did not add any plugins.",
					Color.LimeGreen);
				return release.Version;
			}
			finally
			{
				TryDeleteDirectory(tempRoot);
			}
		}

		private static HttpClient CreateClient()
		{
			HttpClient client = new()
			{
				Timeout = TimeSpan.FromMinutes(5)
			};
			client.DefaultRequestHeaders.UserAgent.Add(
				new ProductInfoHeaderValue("Synix-Control-Panel", "1.0"));
			client.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
			client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
			return client;
		}

		private static async Task<OxideRelease> GetLatestReleaseAsync(
			CancellationToken cancellationToken)
		{
			using HttpResponseMessage response = await Client.GetAsync(
				LatestReleaseUri,
				HttpCompletionOption.ResponseHeadersRead,
				cancellationToken);
			response.EnsureSuccessStatusCode();
			await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
			using JsonDocument document = await JsonDocument.ParseAsync(
				stream,
				cancellationToken: cancellationToken);
			JsonElement root = document.RootElement;
			string version = root.GetProperty("tag_name").GetString()?.Trim() ?? string.Empty;
			string[] versionParts = version.Split('.');
			if (version.Length is < 3 or > 64 ||
				versionParts.Length is < 2 or > 4 ||
				versionParts.Any(part =>
					part.Length == 0 || part.Any(character => !char.IsAsciiDigit(character))))
			{
				throw new InvalidDataException("GitHub returned an invalid Oxide release version.");
			}

			foreach (JsonElement asset in root.GetProperty("assets").EnumerateArray())
			{
				if (!string.Equals(
					asset.GetProperty("name").GetString(),
					WindowsAssetName,
					StringComparison.Ordinal))
				{
					continue;
				}

				string digest = asset.GetProperty("digest").GetString() ?? string.Empty;
				string sha256 = NormalizeSha256Digest(digest);
				string url = asset.GetProperty("browser_download_url").GetString() ?? string.Empty;
				if (!TryValidateDownloadUri(url, version, out Uri? downloadUri))
					throw new InvalidDataException("GitHub returned an unsafe Oxide asset URL.");
				long size = asset.GetProperty("size").GetInt64();
				if (size is <= 0 or > MaximumDownloadBytes)
					throw new InvalidDataException("The Oxide asset size is invalid.");

				return new OxideRelease(version, downloadUri!, sha256);
			}

			throw new InvalidDataException(
				$"The official release did not contain {WindowsAssetName}.");
		}

		internal static string NormalizeSha256Digest(string digest)
		{
			const string prefix = "sha256:";
			if (!digest.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
				throw new InvalidDataException("The Oxide release is missing its SHA-256 digest.");
			string value = digest[prefix.Length..];
			if (value.Length != 64 || value.Any(character => !Uri.IsHexDigit(character)))
				throw new InvalidDataException("The Oxide release has an invalid SHA-256 digest.");
			return value.ToUpperInvariant();
		}

		internal static bool TryValidateDownloadUri(
			string value,
			string version,
			out Uri? uri)
		{
			uri = null;
			if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? candidate) ||
				candidate.Scheme != Uri.UriSchemeHttps ||
				!candidate.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) ||
				!candidate.IsDefaultPort ||
				!string.IsNullOrEmpty(candidate.UserInfo) ||
				!string.IsNullOrEmpty(candidate.Query) ||
				!string.IsNullOrEmpty(candidate.Fragment))
			{
				return false;
			}

			string expectedPath =
				$"/OxideMod/Oxide.Rust/releases/download/{version}/{WindowsAssetName}";
			if (!candidate.AbsolutePath.Equals(expectedPath, StringComparison.Ordinal))
				return false;
			uri = candidate;
			return true;
		}

		private static async Task DownloadAsync(
			Uri uri,
			string destinationPath,
			CancellationToken cancellationToken)
		{
			using HttpResponseMessage response = await Client.GetAsync(
				uri,
				HttpCompletionOption.ResponseHeadersRead,
				cancellationToken);
			response.EnsureSuccessStatusCode();
			long? contentLength = response.Content.Headers.ContentLength;
			if (contentLength is <= 0 or > MaximumDownloadBytes)
				throw new InvalidDataException("The Oxide download size is invalid.");

			await using Stream source = await response.Content.ReadAsStreamAsync(cancellationToken);
			await using FileStream destination = new(
				destinationPath,
				FileMode.CreateNew,
				FileAccess.Write,
				FileShare.None,
				81920,
				FileOptions.Asynchronous | FileOptions.SequentialScan);
			byte[] buffer = new byte[81920];
			long total = 0;
			while (true)
			{
				int read = await source.ReadAsync(buffer, cancellationToken);
				if (read == 0)
					break;
				total += read;
				if (total > MaximumDownloadBytes)
					throw new InvalidDataException("The Oxide download exceeded its size limit.");
				await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
			}
		}

		internal static async Task<string> ComputeSha256Async(
			string path,
			CancellationToken cancellationToken = default)
		{
			await using FileStream stream = new(
				path,
				FileMode.Open,
				FileAccess.Read,
				FileShare.Read,
				81920,
				FileOptions.Asynchronous | FileOptions.SequentialScan);
			byte[] digest = await SHA256.HashDataAsync(stream, cancellationToken);
			return Convert.ToHexString(digest);
		}

		internal static void ExtractArchiveSafely(string archivePath, string destinationRoot)
		{
			Directory.CreateDirectory(destinationRoot);
			string root = Path.GetFullPath(destinationRoot)
				.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
			using ZipArchive archive = ZipFile.OpenRead(archivePath);
			if (archive.Entries.Count > MaximumArchiveEntries)
				throw new InvalidDataException("The Oxide archive contains too many entries.");
			long extractedBytes = 0;
			foreach (ZipArchiveEntry entry in archive.Entries)
			{
				extractedBytes = checked(extractedBytes + entry.Length);
				if (extractedBytes > MaximumExtractedBytes)
					throw new InvalidDataException("The Oxide archive exceeds the extraction size limit.");
				string normalized = entry.FullName.Replace('/', Path.DirectorySeparatorChar);
				if (string.IsNullOrWhiteSpace(normalized))
					continue;
				string destination = Path.GetFullPath(Path.Combine(root, normalized));
				if (!destination.StartsWith(root, StringComparison.OrdinalIgnoreCase))
					throw new InvalidDataException("The Oxide archive contains an unsafe path.");
				if (normalized.EndsWith(Path.DirectorySeparatorChar))
				{
					Directory.CreateDirectory(destination);
					continue;
				}

				Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
				using Stream source = entry.Open();
				using FileStream target = new(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None);
				source.CopyTo(target);
			}
		}

		internal static void ApplyOverlayWithRollback(
			string sourceRoot,
			string destinationRoot,
			string rollbackRoot)
		{
			string source = Path.GetFullPath(sourceRoot);
			string destination = Path.GetFullPath(destinationRoot)
				.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
			List<string> createdFiles = [];
			List<(string Destination, string Backup)> replacedFiles = [];

			try
			{
				foreach (string sourceFile in Directory.EnumerateFiles(
					source,
					"*",
					SearchOption.AllDirectories))
				{
					string relative = Path.GetRelativePath(source, sourceFile);
					string destinationFile = Path.GetFullPath(Path.Combine(destination, relative));
					if (!destinationFile.StartsWith(destination, StringComparison.OrdinalIgnoreCase))
						throw new InvalidDataException("The Oxide overlay contains an unsafe path.");

					Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);
					if (File.Exists(destinationFile))
					{
						string backupFile = Path.Combine(rollbackRoot, relative);
						Directory.CreateDirectory(Path.GetDirectoryName(backupFile)!);
						File.Copy(destinationFile, backupFile, true);
						replacedFiles.Add((destinationFile, backupFile));
						File.SetAttributes(destinationFile, FileAttributes.Normal);
					}
					else
					{
						createdFiles.Add(destinationFile);
					}

					string temporaryFile = destinationFile + ".synix-oxide-" + Guid.NewGuid().ToString("N");
					try
					{
						File.Copy(sourceFile, temporaryFile, false);
						File.Move(temporaryFile, destinationFile, true);
					}
					finally
					{
						try
						{
							if (File.Exists(temporaryFile))
								File.Delete(temporaryFile);
						}
						catch
						{
						}
					}
				}
			}
			catch
			{
				foreach (string createdFile in createdFiles.AsEnumerable().Reverse())
				{
					try
					{
						if (File.Exists(createdFile))
							File.Delete(createdFile);
					}
					catch
					{
					}
				}
				foreach ((string destinationFile, string backupFile) in replacedFiles.AsEnumerable().Reverse())
				{
					try
					{
						File.Copy(backupFile, destinationFile, true);
					}
					catch
					{
					}
				}
				throw;
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

		private sealed record OxideRelease(string Version, Uri DownloadUri, string Sha256);
	}
}
