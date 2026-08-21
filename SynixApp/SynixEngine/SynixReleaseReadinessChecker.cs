// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace Synix_Control_Panel.SynixEngine
{
	public enum SynixReleaseCheckLevel
	{
		Passed,
		Warning,
		Failed
	}

	public sealed record SynixReleaseCheckItem(
		SynixReleaseCheckLevel Level,
		string Name,
		string Details);

	public sealed record SynixReleaseReadinessReport(
		Version? Version,
		string ProjectDirectory,
		string PublishDirectory,
		string? StandaloneSha256,
		string? SetupSha256,
		IReadOnlyList<SynixReleaseCheckItem> Items)
	{
		public bool IsReady => Items.All(item =>
			item.Level != SynixReleaseCheckLevel.Failed);

		public int PassedCount => Items.Count(item =>
			item.Level == SynixReleaseCheckLevel.Passed);

		public int WarningCount => Items.Count(item =>
			item.Level == SynixReleaseCheckLevel.Warning);

		public int FailedCount => Items.Count(item =>
			item.Level == SynixReleaseCheckLevel.Failed);

		public string ToPlainText()
		{
			StringBuilder report = new();
			report.AppendLine("SYNIX RELEASE READINESS REPORT");
			report.AppendLine(new string('=', 36));
			report.AppendLine($"Result: {(IsReady ? "READY TO RELEASE" : "NOT READY")}");
			report.AppendLine($"Version: {(Version is null ? "Unknown" : Version.ToString(3))}");
			report.AppendLine($"Passed: {PassedCount}  Warnings: {WarningCount}  Failed: {FailedCount}");
			report.AppendLine($"Project: {ProjectDirectory}");
			report.AppendLine($"Publish folder: {PublishDirectory}");
			report.AppendLine();

			foreach (SynixReleaseCheckItem item in Items)
			{
				string marker = item.Level switch
				{
					SynixReleaseCheckLevel.Passed => "PASS",
					SynixReleaseCheckLevel.Warning => "WARN",
					_ => "FAIL"
				};
				report.AppendLine($"[{marker}] {item.Name}");
				report.AppendLine($"       {item.Details}");
			}

			report.AppendLine();
			report.AppendLine("GITHUB RELEASE ASSETS");
			report.AppendLine($"Synix.Control.Panel.exe  SHA-256: {StandaloneSha256 ?? "Unavailable"}");
			report.AppendLine($"SynixSetup.exe           SHA-256: {SetupSha256 ?? "Unavailable"}");
			report.AppendLine();
			report.AppendLine("Upload the published 'Synix Control Panel.exe' as 'Synix.Control.Panel.exe'.");
			report.AppendLine("Upload 'package\\Output\\SynixSetup.exe' as 'SynixSetup.exe'.");
			return report.ToString().TrimEnd();
		}
	}

	public sealed class SynixReleaseReadinessChecker
	{
		public const string ManifestFileName = "Synix.release-manifest.txt";
		public const string ManifestBackupRelativePath = @"package\Output\Synix.release-manifest.txt";
		public const string PublishedExecutableName = "Synix Control Panel.exe";
		public const string SetupRelativePath = @"package\Output\SynixSetup.exe";
		public const string InnoScriptRelativePath = @"package\Synix.iss";

		private const long MinimumExecutableSize = 1024L * 1024L;
		private const string ExpectedAppId = "D3E8B790-86E8-4485-B827-7A743AB72BDB";

		public async Task<SynixReleaseReadinessReport> CheckAsync(
			string projectDirectory,
			string publishDirectory,
			IProgress<string>? progress = null,
			CancellationToken cancellationToken = default)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(projectDirectory);
			ArgumentException.ThrowIfNullOrWhiteSpace(publishDirectory);

			string fullProjectDirectory = Path.GetFullPath(projectDirectory);
			string fullPublishDirectory = Path.GetFullPath(publishDirectory);
			List<SynixReleaseCheckItem> items = [];
			Version? projectVersion = null;
			Version? versionFileVersion = null;
			string? standaloneHash = null;
			string? setupHash = null;

			progress?.Report("Checking project versions...");
			string projectFile = Path.Combine(
				fullProjectDirectory,
				"Synix Control Panel.csproj");
			if (!File.Exists(projectFile))
			{
				AddFailure(items, "Project file", "Synix Control Panel.csproj was not found.");
			}
			else
			{
				projectVersion = ReadProjectVersion(projectFile);
				if (projectVersion is null)
					AddFailure(items, "Project version", "The project Version value is missing or invalid.");
				else
					AddPassed(items, "Project version", $"The project is set to v{projectVersion.ToString(3)}.");
			}

			string versionFile = Path.Combine(
				fullProjectDirectory,
				"SynixEngine",
				"version.txt");
			if (!File.Exists(versionFile))
			{
				AddFailure(items, "Version file", "SynixEngine\\version.txt was not found.");
			}
			else
			{
				try
				{
					if (!SynixUpdateService.TryParseVersionText(
						File.ReadAllText(versionFile),
						out versionFileVersion))
					{
						AddFailure(items, "Version file", "version.txt does not contain a valid version.");
					}
					else if (projectVersion is null || versionFileVersion != projectVersion)
					{
						AddFailure(
							items,
							"Version agreement",
							$"The project is v{FormatVersion(projectVersion)}, but version.txt is v{FormatVersion(versionFileVersion)}.");
					}
					else
					{
						AddPassed(items, "Version agreement", "The project and version.txt match.");
					}
				}
				catch (Exception exception)
				{
					AddFailure(
						items,
						"Version file",
						$"version.txt could not be read: {exception.Message}");
				}
			}

			progress?.Report("Checking published programs...");
			if (!Directory.Exists(fullPublishDirectory))
			{
				AddFailure(items, "Publish folder", "The selected publish folder does not exist.");
			}
			else
			{
				AddPassed(items, "Publish folder", "The publish folder is available.");
			}

			string standalonePath = Path.Combine(
				fullPublishDirectory,
				PublishedExecutableName);
			string setupPath = Path.Combine(
				fullPublishDirectory,
				SetupRelativePath);

			standaloneHash = await CheckArtifactAsync(
				items,
				"Published standalone program",
				standalonePath,
				projectVersion,
				progress,
				cancellationToken);
			setupHash = await CheckArtifactAsync(
				items,
				"Inno Setup installer",
				setupPath,
				projectVersion,
				progress,
				cancellationToken);

			string manifestPath = FindReleaseManifestPath(fullPublishDirectory);
			progress?.Report("Checking Stable-build manifest...");
			CheckManifest(
				items,
				manifestPath,
				projectVersion,
				standaloneHash,
				setupHash);

			progress?.Report("Checking Inno Setup safety settings...");
			CheckInnoScript(
				items,
				Path.Combine(fullPublishDirectory, InnoScriptRelativePath));

			progress?.Report("Checking the Publish test receipt...");
			CheckAutomatedTestReceipt(
				items,
				manifestPath);

			items.Add(new SynixReleaseCheckItem(
				SynixReleaseCheckLevel.Passed,
				"GitHub asset names",
				"Upload the standalone program as Synix.Control.Panel.exe and the installer as SynixSetup.exe."));

			return new SynixReleaseReadinessReport(
				projectVersion,
				fullProjectDirectory,
				fullPublishDirectory,
				standaloneHash,
				setupHash,
				items);
		}

		public static string? FindProjectDirectory(string startDirectory)
		{
			if (string.IsNullOrWhiteSpace(startDirectory))
				return null;

			DirectoryInfo? directory;
			try
			{
				directory = new DirectoryInfo(Path.GetFullPath(startDirectory));
			}
			catch
			{
				return null;
			}

			for (int depth = 0; directory is not null && depth < 12; depth++)
			{
				if (File.Exists(Path.Combine(
					directory.FullName,
					"Synix Control Panel.csproj")))
				{
					return directory.FullName;
				}
				directory = directory.Parent;
			}

			return null;
		}

		public static string? FindPublishDirectory(string projectDirectory)
		{
			string profilesDirectory = Path.Combine(
				projectDirectory,
				"Properties",
				"PublishProfiles");
			if (!Directory.Exists(profilesDirectory))
				return null;

			List<string> candidates = [];
			foreach (string profile in Directory.EnumerateFiles(
				profilesDirectory,
				"*.pubxml",
				SearchOption.TopDirectoryOnly))
			{
				try
				{
					XDocument document = XDocument.Load(profile);
					string? value = document
						.Descendants()
						.FirstOrDefault(element =>
							element.Name.LocalName == "PublishDir")
						?.Value;
					if (string.IsNullOrWhiteSpace(value))
						continue;

					string expanded = Environment.ExpandEnvironmentVariables(
						value.Trim());
					string fullPath = Path.GetFullPath(
						Path.IsPathRooted(expanded)
							? expanded
							: Path.Combine(projectDirectory, expanded));
					if (!candidates.Contains(fullPath, StringComparer.OrdinalIgnoreCase))
						candidates.Add(fullPath);
				}
				catch
				{
					// Ignore invalid or machine-specific publish profiles.
				}
			}

			return candidates.FirstOrDefault(candidate =>
				File.Exists(Path.Combine(candidate, PublishedExecutableName))) ??
				candidates.FirstOrDefault(Directory.Exists) ??
				candidates.FirstOrDefault();
		}

		public static string FindReleaseManifestPath(string publishDirectory)
		{
			string fullPublishDirectory = Path.GetFullPath(publishDirectory);
			string primaryPath = Path.Combine(
				fullPublishDirectory,
				ManifestFileName);
			if (File.Exists(primaryPath))
				return primaryPath;

			string backupPath = Path.Combine(
				fullPublishDirectory,
				ManifestBackupRelativePath);
			return File.Exists(backupPath) ? backupPath : primaryPath;
		}

		public static IReadOnlyDictionary<string, string> ReadManifest(
			string manifestPath)
		{
			Dictionary<string, string> values = new(
				StringComparer.OrdinalIgnoreCase);
			foreach (string rawLine in File.ReadLines(manifestPath))
			{
				string line = rawLine.Trim().TrimStart('\uFEFF');
				if (line.Length == 0 || line.StartsWith('#'))
					continue;

				int separator = line.IndexOf('=');
				if (separator <= 0)
					continue;

				values[line[..separator].Trim()] =
					line[(separator + 1)..].Trim();
			}
			return values;
		}

		public static bool TryGetPassingTestReceipt(
			IReadOnlyDictionary<string, string> manifest,
			out DateTimeOffset completedUtc)
		{
			ArgumentNullException.ThrowIfNull(manifest);
			completedUtc = default;
			return manifest.TryGetValue(
				"AutomatedTests",
				out string? result) &&
				string.Equals(
					result,
					"Passed",
					StringComparison.OrdinalIgnoreCase) &&
				manifest.TryGetValue(
					"AutomatedTestsUtc",
					out string? completedText) &&
				DateTimeOffset.TryParse(
					completedText,
					System.Globalization.CultureInfo.InvariantCulture,
					System.Globalization.DateTimeStyles.AssumeUniversal |
						System.Globalization.DateTimeStyles.AdjustToUniversal,
					out completedUtc);
		}

		private static Version? ReadProjectVersion(string projectFile)
		{
			try
			{
				XDocument document = XDocument.Load(projectFile);
				string? versionText = document
					.Descendants()
					.FirstOrDefault(element =>
						element.Name.LocalName == "Version")
					?.Value;
				return SynixUpdateService.TryParseVersionText(
					versionText,
					out Version? version)
						? version
						: null;
			}
			catch
			{
				return null;
			}
		}

		private static async Task<string?> CheckArtifactAsync(
			List<SynixReleaseCheckItem> items,
			string checkName,
			string path,
			Version? expectedVersion,
			IProgress<string>? progress,
			CancellationToken cancellationToken)
		{
			if (!File.Exists(path))
			{
				AddFailure(items, checkName, $"Missing file: {path}");
				return null;
			}

			try
			{
				FileInfo file = new(path);
				if (file.Length < MinimumExecutableSize)
				{
					AddFailure(items, checkName, $"The file is unexpectedly small ({FormatBytes(file.Length)}).");
					return null;
				}

				FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(path);
				bool versionValid = SynixUpdateService.TryParseVersionText(
					versionInfo.ProductVersion,
					out Version? artifactVersion);
				if (!versionValid || expectedVersion is null ||
					artifactVersion != expectedVersion)
				{
					AddFailure(
						items,
						checkName,
						$"Expected v{FormatVersion(expectedVersion)}, but the file reports v{FormatVersion(artifactVersion)}.");
				}
				else
				{
					AddPassed(
						items,
						checkName,
						$"v{artifactVersion!.ToString(3)} is present ({FormatBytes(file.Length)}).");
				}

				progress?.Report($"Calculating SHA-256 for {Path.GetFileName(path)}...");
				await using FileStream stream = new(
					path,
					FileMode.Open,
					FileAccess.Read,
					FileShare.Read,
					81920,
					FileOptions.Asynchronous | FileOptions.SequentialScan);
				byte[] hash = await SHA256.HashDataAsync(stream, cancellationToken);
				string result = Convert.ToHexString(hash).ToLowerInvariant();
				CryptographicOperations.ZeroMemory(hash);
				return result;
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception exception)
			{
				AddFailure(
					items,
					checkName,
					$"The file could not be inspected: {exception.Message}");
				return null;
			}
		}

		private static void CheckManifest(
			List<SynixReleaseCheckItem> items,
			string manifestPath,
			Version? expectedVersion,
			string? standaloneHash,
			string? setupHash)
		{
			if (!File.Exists(manifestPath))
			{
				AddFailure(
					items,
					"Stable-build manifest",
					"The publish manifest is missing. Publish Synix again before releasing.");
				return;
			}

			IReadOnlyDictionary<string, string> manifest;
			try
			{
				manifest = ReadManifest(manifestPath);
			}
			catch (Exception exception)
			{
				AddFailure(
					items,
					"Stable-build manifest",
					$"The manifest could not be read: {exception.Message}");
				return;
			}
			List<string> problems = [];
			if (!manifest.TryGetValue("FormatVersion", out string? format) || format != "1")
				problems.Add("unsupported manifest format");
			if (!manifest.TryGetValue("Channel", out string? channel) ||
				!SynixBuildInfo.IsOfficialChannel(channel))
			{
				problems.Add("the published EXE is not marked Stable");
			}
			if (!manifest.TryGetValue("Version", out string? versionText) ||
				!SynixUpdateService.TryParseVersionText(versionText, out Version? manifestVersion) ||
				expectedVersion is null || manifestVersion != expectedVersion)
			{
				problems.Add("manifest version does not match the project");
			}
			if (!ManifestValueMatches(
				manifest,
				"StandaloneFile",
				PublishedExecutableName))
			{
				problems.Add("standalone filename is incorrect");
			}
			if (!ManifestValueMatches(
				manifest,
				"SetupFile",
				SetupRelativePath))
			{
				problems.Add("Setup filename is incorrect");
			}
			if (!HashMatches(manifest, "StandaloneSha256", standaloneHash))
				problems.Add("standalone SHA-256 does not match the published file");
			if (!HashMatches(manifest, "SetupSha256", setupHash))
				problems.Add("Setup SHA-256 does not match the installer");

			if (problems.Count == 0)
			{
				AddPassed(
					items,
					"Stable-build manifest",
					"The published files match the Stable manifest and both SHA-256 values.");
			}
			else
			{
				AddFailure(
					items,
					"Stable-build manifest",
					string.Join("; ", problems) + ".");
			}
		}

		private static void CheckInnoScript(
			List<SynixReleaseCheckItem> items,
			string scriptPath)
		{
			if (!File.Exists(scriptPath))
			{
				AddFailure(items, "Inno Setup settings", $"Missing file: {scriptPath}");
				return;
			}

			string script;
			try
			{
				script = File.ReadAllText(scriptPath);
			}
			catch (Exception exception)
			{
				AddFailure(
					items,
					"Inno Setup settings",
					$"The Inno Setup script could not be read: {exception.Message}");
				return;
			}
			(string Text, string Description)[] requirements =
			[
				(ExpectedAppId, "fixed Synix AppId"),
				("DefaultDirName={userappdata}\\Synix", "per-user install folder"),
				("PrivilegesRequired=lowest", "non-administrator installation"),
				("OutputBaseFilename=SynixSetup", "expected Setup filename"),
				("CloseApplications=yes", "safe application closing")
			];
			List<string> missing = requirements
				.Where(requirement => !script.Contains(
					requirement.Text,
					StringComparison.OrdinalIgnoreCase))
				.Select(requirement => requirement.Description)
				.ToList();

			if (missing.Count == 0)
			{
				AddPassed(
					items,
					"Inno Setup settings",
					"AppId, install location, permissions, filename, and close behavior are correct.");
			}
			else
			{
				AddFailure(
					items,
					"Inno Setup settings",
					"Missing or incorrect: " + string.Join(", ", missing) + ".");
			}
		}

		private static void CheckAutomatedTestReceipt(
			List<SynixReleaseCheckItem> items,
			string manifestPath)
		{
			if (!File.Exists(manifestPath))
			{
				AddFailure(
					items,
					"Automated tests",
					"The Publish test receipt is missing. Publish Synix again before releasing.");
				return;
			}

			try
			{
				IReadOnlyDictionary<string, string> manifest = ReadManifest(
					manifestPath);
				if (TryGetPassingTestReceipt(manifest, out DateTimeOffset completedUtc))
				{
					AddPassed(
						items,
						"Automated tests",
						$"The complete suite passed during Visual Studio Publish at {completedUtc.ToLocalTime():g}.");
				}
				else
				{
					AddFailure(
						items,
						"Automated tests",
						"The manifest does not contain a valid passing test receipt. Publish Synix again.");
				}
			}
			catch (Exception exception)
			{
				AddFailure(
					items,
					"Automated tests",
					$"The Publish test receipt could not be read: {exception.Message}");
			}
		}

		private static bool ManifestValueMatches(
			IReadOnlyDictionary<string, string> manifest,
			string key,
			string expected)
		{
			return manifest.TryGetValue(key, out string? value) &&
				string.Equals(
					value.Replace('/', '\\'),
					expected.Replace('/', '\\'),
					StringComparison.OrdinalIgnoreCase);
		}

		private static bool HashMatches(
			IReadOnlyDictionary<string, string> manifest,
			string key,
			string? actualHash)
		{
			return actualHash is not null &&
				manifest.TryGetValue(key, out string? expectedHash) &&
				string.Equals(
					expectedHash,
					actualHash,
					StringComparison.OrdinalIgnoreCase);
		}

		private static string FormatVersion(Version? version)
		{
			return version?.ToString(3) ?? "Unknown";
		}

		private static string FormatBytes(long bytes)
		{
			double megabytes = bytes / 1024d / 1024d;
			return $"{megabytes:0.##} MB";
		}

		private static void AddPassed(
			List<SynixReleaseCheckItem> items,
			string name,
			string details)
		{
			items.Add(new SynixReleaseCheckItem(
				SynixReleaseCheckLevel.Passed,
				name,
				details));
		}

		private static void AddFailure(
			List<SynixReleaseCheckItem> items,
			string name,
			string details)
		{
			items.Add(new SynixReleaseCheckItem(
				SynixReleaseCheckLevel.Failed,
				name,
				details));
		}
	}
}
