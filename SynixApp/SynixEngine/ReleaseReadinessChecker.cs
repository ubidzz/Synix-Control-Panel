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
using System.Diagnostics;
using System.Runtime.InteropServices;
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
		string? MsiSha256,
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
			report.AppendLine($"SynixSetup.msi           SHA-256: {MsiSha256 ?? "Unavailable"}");
			report.AppendLine();
			report.AppendLine("Upload the published 'Synix Control Panel.exe' as 'Synix.Control.Panel.exe'.");
			report.AppendLine("Upload the published 'SynixSetup.msi' as 'SynixSetup.msi'.");
			return report.ToString().TrimEnd();
		}
	}

	public partial class Core
	{
		public const string ManifestFileName = "Synix.release-manifest.txt";
		public const string ManifestBackupRelativePath = "Synix.release-manifest.backup.txt";
		public const string PublishedExecutableName = "Synix Control Panel.exe";
		public const string MsiFileName = "SynixSetup.msi";
		public const string MsiProjectRelativePath = @"Packaging\MSI\SynixInstaller.wixproj";
		public const string MsiSourceRelativePath = @"Packaging\MSI\Package.wxs";

		private const long MinimumExecutableSize = 1024L * 1024L;
		private const string ExpectedUpgradeCode = "e369556b-db95-4d9b-8e86-2b7d50dcd328";

		public static async Task<SynixReleaseReadinessReport> CheckReleaseReadinessAsync(
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
			string? msiHash = null;

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
					if (!Core.TryParseVersionText(
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
							$"The project is v{FormatReleaseVersion(projectVersion)}, but version.txt is v{FormatReleaseVersion(versionFileVersion)}.");
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
			string msiPath = Path.Combine(
				fullPublishDirectory,
				MsiFileName);

			standaloneHash = await CheckArtifactAsync(
				items,
				"Published standalone program",
				standalonePath,
				projectVersion,
				progress,
				cancellationToken);
			msiHash = await CheckMsiArtifactAsync(
				items,
				msiPath,
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
				msiHash);

			progress?.Report("Checking MSI package settings...");
			CheckMsiProject(
				items,
				fullProjectDirectory);

			progress?.Report("Checking the Publish test receipt...");
			CheckAutomatedTestReceipt(
				items,
				manifestPath);

			items.Add(new SynixReleaseCheckItem(
				SynixReleaseCheckLevel.Passed,
				"GitHub asset names",
				"Upload the standalone program as Synix.Control.Panel.exe and the installer as SynixSetup.msi."));

			return new SynixReleaseReadinessReport(
				projectVersion,
				fullProjectDirectory,
				fullPublishDirectory,
				standaloneHash,
				msiHash,
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
				return Core.TryParseVersionText(
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
					AddFailure(items, checkName, $"The file is unexpectedly small ({FormatReleaseBytes(file.Length)}).");
					return null;
				}

				FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(path);
				bool versionValid = Core.TryParseVersionText(
					versionInfo.ProductVersion,
					out Version? artifactVersion);
				if (!versionValid || expectedVersion is null ||
					artifactVersion != expectedVersion)
				{
					AddFailure(
						items,
						checkName,
						$"Expected v{FormatReleaseVersion(expectedVersion)}, but the file reports v{FormatReleaseVersion(artifactVersion)}.");
				}
				else
				{
					AddPassed(
						items,
						checkName,
						$"v{artifactVersion!.ToString(3)} is present ({FormatReleaseBytes(file.Length)}).");
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

		private static async Task<string?> CheckMsiArtifactAsync(
			List<SynixReleaseCheckItem> items,
			string path,
			Version? expectedVersion,
			IProgress<string>? progress,
			CancellationToken cancellationToken)
		{
			const string checkName = "Windows Installer (MSI)";
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
					AddFailure(items, checkName, $"The file is unexpectedly small ({FormatReleaseBytes(file.Length)}).");
					return null;
				}

				IReadOnlyDictionary<string, string> properties =
					ReadMsiProperties(path);
				properties.TryGetValue("ProductVersion", out string? versionText);
				bool versionValid = Core.TryParseVersionText(
					versionText,
					out Version? msiVersion);
				bool nameValid = ManifestValueMatches(
					properties,
					"ProductName",
					"Synix Control Panel");
				bool publisherValid = ManifestValueMatches(
					properties,
					"Manufacturer",
					"ubidzz");
				bool upgradeCodeValid = properties.TryGetValue(
					"UpgradeCode",
					out string? upgradeCode) &&
					string.Equals(
						upgradeCode.Trim().Trim('{', '}'),
						ExpectedUpgradeCode,
						StringComparison.OrdinalIgnoreCase);

				List<string> problems = [];
				if (!versionValid || expectedVersion is null || msiVersion != expectedVersion)
					problems.Add($"expected v{FormatReleaseVersion(expectedVersion)}, but the MSI reports v{FormatReleaseVersion(msiVersion)}");
				if (!nameValid)
					problems.Add("the product name is incorrect");
				if (!publisherValid)
					problems.Add("the publisher is incorrect");
				if (!upgradeCodeValid)
					problems.Add("the upgrade code is incorrect");

				if (problems.Count == 0)
				{
					AddPassed(
						items,
						checkName,
						$"v{msiVersion!.ToString(3)} is present ({FormatReleaseBytes(file.Length)}) with the correct upgrade identity.");
				}
				else
				{
					AddFailure(
						items,
						checkName,
						string.Join("; ", problems) + ".");
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
					$"The MSI could not be inspected: {exception.Message}");
				return null;
			}
		}

		private static IReadOnlyDictionary<string, string> ReadMsiProperties(
			string path)
		{
			const uint success = 0;
			const uint noMoreItems = 259;
			uint status = MsiOpenDatabase(path, IntPtr.Zero, out IntPtr database);
			if (status != success)
				throw new InvalidDataException($"Windows Installer could not open the MSI (error {status}).");

			IntPtr view = IntPtr.Zero;
			try
			{
				status = MsiDatabaseOpenView(
					database,
					"SELECT `Property`, `Value` FROM `Property`",
					out view);
				if (status != success)
					throw new InvalidDataException($"The MSI Property table could not be opened (error {status}).");
				status = MsiViewExecute(view, IntPtr.Zero);
				if (status != success)
					throw new InvalidDataException($"The MSI Property table could not be read (error {status}).");

				Dictionary<string, string> properties = new(
					StringComparer.OrdinalIgnoreCase);
				while (true)
				{
					status = MsiViewFetch(view, out IntPtr record);
					if (status == noMoreItems)
						break;
					if (status != success)
						throw new InvalidDataException($"The MSI Property table is incomplete (error {status}).");

					try
					{
						string name = ReadMsiRecordString(record, 1);
						if (name.Length > 0)
							properties[name] = ReadMsiRecordString(record, 2);
					}
					finally
					{
						MsiCloseHandle(record);
					}
				}

				return properties;
			}
			finally
			{
				if (view != IntPtr.Zero)
					MsiCloseHandle(view);
				MsiCloseHandle(database);
			}
		}

		private static string ReadMsiRecordString(IntPtr record, uint field)
		{
			const uint success = 0;
			const uint moreData = 234;
			uint characterCount = 0;
			uint status = MsiRecordGetString(
				record,
				field,
				null,
				ref characterCount);
			if (status is not (success or moreData))
				throw new InvalidDataException($"An MSI value could not be read (error {status}).");

			StringBuilder value = new(checked((int)characterCount + 1));
			uint capacity = (uint)value.Capacity;
			status = MsiRecordGetString(
				record,
				field,
				value,
				ref capacity);
			if (status != success)
				throw new InvalidDataException($"An MSI value could not be read (error {status}).");
			return value.ToString();
		}

		private static void CheckManifest(
			List<SynixReleaseCheckItem> items,
			string manifestPath,
			Version? expectedVersion,
			string? standaloneHash,
			string? msiHash)
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
			if (!manifest.TryGetValue("FormatVersion", out string? format) || format != "2")
				problems.Add("unsupported manifest format");
			if (!manifest.TryGetValue("Channel", out string? channel) ||
				!Core.IsOfficialChannel(channel))
			{
				problems.Add("the published EXE is not marked Stable");
			}
			if (!manifest.TryGetValue("Version", out string? versionText) ||
				!Core.TryParseVersionText(versionText, out Version? manifestVersion) ||
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
				"MsiFile",
				MsiFileName))
			{
				problems.Add("MSI filename is incorrect");
			}
			if (!HashMatches(manifest, "StandaloneSha256", standaloneHash))
				problems.Add("standalone SHA-256 does not match the published file");
			if (!HashMatches(manifest, "MsiSha256", msiHash))
				problems.Add("MSI SHA-256 does not match the installer");

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

		private static void CheckMsiProject(
			List<SynixReleaseCheckItem> items,
			string projectDirectory)
		{
			string msiProjectPath = Path.Combine(
				projectDirectory,
				MsiProjectRelativePath);
			string msiSourcePath = Path.Combine(
				projectDirectory,
				MsiSourceRelativePath);
			if (!File.Exists(msiProjectPath) || !File.Exists(msiSourcePath))
			{
				AddFailure(
					items,
					"MSI package settings",
					$"Missing MSI project file: {(!File.Exists(msiProjectPath) ? msiProjectPath : msiSourcePath)}");
				return;
			}

			string msiProject;
			string msiSource;
			try
			{
				msiProject = File.ReadAllText(msiProjectPath);
				msiSource = File.ReadAllText(msiSourcePath);
			}
			catch (Exception exception)
			{
				AddFailure(
					items,
					"MSI package settings",
					$"The MSI project could not be read: {exception.Message}");
				return;
			}

			string combined = msiProject + Environment.NewLine + msiSource;
			(string Text, string Description)[] requirements =
			[
				(ExpectedUpgradeCode, "fixed MSI upgrade code"),
				("<Version>$(Version)</Version>", "shared project version"),
				("SynixVersion=$(Version)", "MSI version forwarding"),
				("Version=\"$(var.SynixVersion)\"", "MSI package version"),
				("Scope=\"perUser\"", "non-administrator per-user installation"),
				("<MajorUpgrade", "automatic MSI upgrades"),
				("AllowSameVersionUpgrades=\"yes\"", "safe same-version test upgrades"),
				("<OutputName>SynixSetup</OutputName>", "expected MSI filename"),
				("<InstallerPlatform>x64</InstallerPlatform>", "64-bit installer platform"),
				(@"Software\ubidzz\Synix Control Panel", "stable install registration"),
				("SynixInstallSource", "Setup and WinGet source registration")
			];
			List<string> missing = requirements
				.Where(requirement => !combined.Contains(
					requirement.Text,
					StringComparison.OrdinalIgnoreCase))
				.Select(requirement => requirement.Description)
				.ToList();
			if (combined.Contains(@"C:\Users\", StringComparison.OrdinalIgnoreCase))
				missing.Add("portable paths without a personal user folder");

			if (missing.Count == 0)
			{
				AddPassed(
					items,
					"MSI package settings",
					"Version forwarding, upgrade identity, per-user installation, filename, architecture, and portable paths are correct.");
			}
			else
			{
				AddFailure(
					items,
					"MSI package settings",
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

		private static string FormatReleaseVersion(Version? version)
		{
			return version?.ToString(3) ?? "Unknown";
		}

		private static string FormatReleaseBytes(long bytes)
		{
			double megabytes = bytes / 1024d / 1024d;
			return $"{megabytes:0.##} MB";
		}

		[DllImport("msi.dll", EntryPoint = "MsiOpenDatabaseW", CharSet = CharSet.Unicode)]
		private static extern uint MsiOpenDatabase(
			string databasePath,
			IntPtr persist,
			out IntPtr database);

		[DllImport("msi.dll", EntryPoint = "MsiDatabaseOpenViewW", CharSet = CharSet.Unicode)]
		private static extern uint MsiDatabaseOpenView(
			IntPtr database,
			string query,
			out IntPtr view);

		[DllImport("msi.dll")]
		private static extern uint MsiViewExecute(
			IntPtr view,
			IntPtr record);

		[DllImport("msi.dll", EntryPoint = "MsiViewFetch")]
		private static extern uint MsiViewFetch(
			IntPtr view,
			out IntPtr record);

		[DllImport("msi.dll", EntryPoint = "MsiRecordGetStringW", CharSet = CharSet.Unicode)]
		private static extern uint MsiRecordGetString(
			IntPtr record,
			uint field,
			StringBuilder? value,
			ref uint characterCount);

		[DllImport("msi.dll")]
		private static extern uint MsiCloseHandle(IntPtr handle);

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
