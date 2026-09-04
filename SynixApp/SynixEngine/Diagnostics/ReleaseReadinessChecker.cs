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
using Synix_Control_Panel.SynixApp.Database.GameDefinitions;

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
			report.AppendLine(LocalizationManager.Get("Release.Report.Title"));
			report.AppendLine(new string('=', 36));
			report.AppendLine(LocalizationManager.Get(
				"Release.Report.Result",
				LocalizationManager.Get(IsReady
					? "Release.Report.Result.Ready"
					: "Release.Report.Result.NotReady")));
			report.AppendLine(LocalizationManager.Get(
				"Release.Report.Version",
				Version is null
					? LocalizationManager.Get("Report.Unknown")
					: Version.ToString(3)));
			report.AppendLine(LocalizationManager.Get(
				"Release.Report.Counts",
				PassedCount,
				WarningCount,
				FailedCount));
			report.AppendLine(LocalizationManager.Get(
				"Release.Report.Project",
				ProjectDirectory));
			report.AppendLine(LocalizationManager.Get(
				"Release.Report.PublishFolder",
				PublishDirectory));
			report.AppendLine();

			foreach (SynixReleaseCheckItem item in Items)
			{
				string markerKey = item.Level switch
				{
					SynixReleaseCheckLevel.Passed => "Report.Marker.Pass",
					SynixReleaseCheckLevel.Warning => "Report.Marker.Warning",
					_ => "Report.Marker.Fail"
				};
				report.AppendLine(LocalizationManager.Get(
					"Release.Report.Item",
					LocalizationManager.Get(markerKey),
					LocalizationManager.TranslateKnownText(item.Name)));
				report.AppendLine(LocalizationManager.Get(
					"Release.Report.ItemDetails",
					LocalizationManager.TranslateRuntimeText(item.Details)));
			}

			report.AppendLine();
			report.AppendLine(LocalizationManager.Get(
				"Release.Report.Assets"));
			string unavailable = LocalizationManager.Get("Report.Unavailable");
			report.AppendLine(LocalizationManager.Get(
				"Release.Report.StandaloneHash",
				StandaloneSha256 ?? unavailable));
			report.AppendLine(LocalizationManager.Get(
				"Release.Report.MsiHash",
				MsiSha256 ?? unavailable));
			report.AppendLine();
			report.AppendLine(LocalizationManager.Get(
				"Release.Report.UploadStandalone"));
			report.AppendLine(LocalizationManager.Get(
				"Release.Report.UploadMsi"));
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

			progress?.Report(LocalizationManager.Get(
				"Release.Progress.ProjectVersions"));
			string projectFile = Path.Combine(
				fullProjectDirectory,
				"Synix Control Panel.csproj");
			if (!File.Exists(projectFile))
			{
				AddFailure(
					items,
					LocalizationManager.Get("Release.Check.ProjectFile"),
					LocalizationManager.Get("Release.Check.ProjectFileMissing"));
			}
			else
			{
				projectVersion = ReadProjectVersion(projectFile);
				if (projectVersion is null)
					AddFailure(
						items,
						LocalizationManager.Get("Release.Check.ProjectVersion"),
						LocalizationManager.Get("Release.Check.ProjectVersionInvalid"));
				else
					AddPassed(
						items,
						LocalizationManager.Get("Release.Check.ProjectVersion"),
						LocalizationManager.Get(
							"Release.Check.ProjectVersionValue",
							projectVersion.ToString(3)));
			}

			string versionFile = Path.Combine(
				fullProjectDirectory,
				"SynixEngine",
				"version.txt");
			if (!File.Exists(versionFile))
			{
				AddFailure(
					items,
					LocalizationManager.Get("Release.Check.VersionFile"),
					LocalizationManager.Get("Release.Check.VersionFileMissing"));
			}
			else
			{
				try
				{
					if (!Core.TryParseVersionText(
						File.ReadAllText(versionFile),
						out versionFileVersion))
					{
						AddFailure(
							items,
							LocalizationManager.Get("Release.Check.VersionFile"),
							LocalizationManager.Get("Release.Check.VersionFileInvalid"));
					}
					else if (projectVersion is null || versionFileVersion != projectVersion)
					{
						AddFailure(
							items,
							LocalizationManager.Get("Release.Check.VersionAgreement"),
							LocalizationManager.Get(
								"Release.Check.VersionMismatch",
								FormatReleaseVersion(projectVersion),
								FormatReleaseVersion(versionFileVersion)));
					}
					else
					{
						AddPassed(
							items,
							LocalizationManager.Get("Release.Check.VersionAgreement"),
							LocalizationManager.Get("Release.Check.VersionMatch"));
					}
				}
				catch (Exception exception)
				{
					AddFailure(
						items,
						LocalizationManager.Get("Release.Check.VersionFile"),
						LocalizationManager.Get(
							"Release.Check.VersionFileReadFailed",
							exception.Message));
				}
			}

			progress?.Report(LocalizationManager.Get(
				"Release.Progress.PublishedPrograms"));
			if (!Directory.Exists(fullPublishDirectory))
			{
				AddFailure(
					items,
					LocalizationManager.Get("Release.Check.PublishFolder"),
					LocalizationManager.Get("Release.Check.PublishFolderMissing"));
			}
			else
			{
				AddPassed(
					items,
					LocalizationManager.Get("Release.Check.PublishFolder"),
					LocalizationManager.Get("Release.Check.PublishFolderAvailable"));
			}

			string standalonePath = Path.Combine(
				fullPublishDirectory,
				PublishedExecutableName);
			string msiPath = Path.Combine(
				fullPublishDirectory,
				MsiFileName);

			standaloneHash = await CheckArtifactAsync(
				items,
				LocalizationManager.Get("Release.Check.StandaloneProgram"),
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
			progress?.Report(LocalizationManager.Get(
				"Release.Progress.Manifest"));
			CheckManifest(
				items,
				manifestPath,
				projectVersion,
				standaloneHash,
				msiHash);

			progress?.Report(LocalizationManager.Get(
				"Release.Progress.MsiSettings"));
			CheckMsiProject(
				items,
				fullProjectDirectory);

			progress?.Report(LocalizationManager.Get(
				"Release.Progress.TestReceipt"));
			CheckAutomatedTestReceipt(
				items,
				manifestPath);
			progress?.Report(LocalizationManager.Get(
				"Release.Progress.SecurityReceipt"));
			CheckSecurityRegressionReceipt(items, manifestPath);

			progress?.Report(LocalizationManager.Get(
				"Release.Progress.GameDefinitions"));
			GameDefinitionValidationReport definitionReport =
				GameDefinitionValidator.ValidateSourceDirectory(
					fullProjectDirectory);
			if (!definitionReport.IsValid)
			{
				string problems = string.Join(
					" ",
					definitionReport.Items
						.Where(item =>
							item.Level == GameDefinitionValidationLevel.Failed)
						.Take(3)
						.Select(item => $"{item.Definition}: {item.Details}"));
				AddFailure(
					items,
					LocalizationManager.Get("Release.Check.GameDefinitions"),
					LocalizationManager.Get(
						"Release.Check.GameDefinitionsFailed",
						definitionReport.FailedCount,
						problems));
			}
			else
			{
				AddPassed(
					items,
					LocalizationManager.Get("Release.Check.GameDefinitions"),
					LocalizationManager.Get(
						"Release.Check.GameDefinitionsPassed",
						definitionReport.DefinitionCount,
						definitionReport.TemplateCount,
						definitionReport.PostInstallActionCount));
			}

			items.Add(new SynixReleaseCheckItem(
				SynixReleaseCheckLevel.Passed,
				LocalizationManager.Get("Release.Check.GitHubAssetNames"),
				LocalizationManager.Get("Release.Check.GitHubAssetNames.Valid")));

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
				catch (Exception suppressedException)
				{
					Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException);
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

		public static bool TryGetPassingSecurityReceipt(
			IReadOnlyDictionary<string, string> manifest,
			out DateTimeOffset completedUtc)
		{
			ArgumentNullException.ThrowIfNull(manifest);
			completedUtc = default;
			return manifest.TryGetValue("SecurityRegressionReview", out string? result) &&
				string.Equals(result, "Passed", StringComparison.OrdinalIgnoreCase) &&
				manifest.TryGetValue("SecurityRegressionReviewUtc", out string? completedText) &&
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
				AddFailure(items, checkName, LocalizationManager.Get(
					"Release.Check.MissingFile",
					path));
				return null;
			}

			try
			{
				FileInfo file = new(path);
				if (file.Length < MinimumExecutableSize)
				{
					AddFailure(items, checkName, LocalizationManager.Get(
						"Release.Check.FileTooSmall",
						FormatReleaseBytes(file.Length)));
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
						LocalizationManager.Get(
							"Release.Check.FileVersionMismatch",
							FormatReleaseVersion(expectedVersion),
							FormatReleaseVersion(artifactVersion)));
				}
				else
				{
					AddPassed(
						items,
						checkName,
						LocalizationManager.Get(
							"Release.Check.FilePresent",
							artifactVersion!.ToString(3),
							FormatReleaseBytes(file.Length)));
				}

				progress?.Report(LocalizationManager.Get(
					"Release.Progress.CalculatingHash",
					Path.GetFileName(path)));
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
					LocalizationManager.Get(
						"Release.Check.FileInspectFailed",
						exception.Message));
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
			string checkName = LocalizationManager.Get(
				"Release.Check.WindowsInstaller");
			if (!File.Exists(path))
			{
				AddFailure(items, checkName, LocalizationManager.Get(
					"Release.Check.MissingFile",
					path));
				return null;
			}

			try
			{
				FileInfo file = new(path);
				if (file.Length < MinimumExecutableSize)
				{
					AddFailure(items, checkName, LocalizationManager.Get(
						"Release.Check.FileTooSmall",
						FormatReleaseBytes(file.Length)));
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
					problems.Add(LocalizationManager.Get(
						"Release.Check.MsiVersionMismatch",
						FormatReleaseVersion(expectedVersion),
						FormatReleaseVersion(msiVersion)));
				if (!nameValid)
					problems.Add(LocalizationManager.Get(
						"Release.Check.MsiProductNameIncorrect"));
				if (!publisherValid)
					problems.Add(LocalizationManager.Get(
						"Release.Check.MsiPublisherIncorrect"));
				if (!upgradeCodeValid)
					problems.Add(LocalizationManager.Get(
						"Release.Check.MsiUpgradeCodeIncorrect"));

				if (problems.Count == 0)
				{
					AddPassed(
						items,
						checkName,
						LocalizationManager.Get(
							"Release.Check.MsiPresent",
							msiVersion!.ToString(3),
							FormatReleaseBytes(file.Length)));
				}
				else
				{
					AddFailure(
						items,
						checkName,
						string.Join("; ", problems) + ".");
				}

				progress?.Report(LocalizationManager.Get(
					"Release.Progress.CalculatingHash",
					Path.GetFileName(path)));
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
					LocalizationManager.Get(
						"Release.Check.MsiInspectFailed",
						exception.Message));
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
				throw new InvalidDataException(LocalizationManager.Get(
					"Release.Error.MsiOpen",
					status));

			IntPtr view = IntPtr.Zero;
			try
			{
				status = MsiDatabaseOpenView(
					database,
					"SELECT `Property`, `Value` FROM `Property`",
					out view);
				if (status != success)
					throw new InvalidDataException(LocalizationManager.Get(
						"Release.Error.MsiPropertyOpen",
						status));
				status = MsiViewExecute(view, IntPtr.Zero);
				if (status != success)
					throw new InvalidDataException(LocalizationManager.Get(
						"Release.Error.MsiPropertyRead",
						status));

				Dictionary<string, string> properties = new(
					StringComparer.OrdinalIgnoreCase);
				while (true)
				{
					status = MsiViewFetch(view, out IntPtr record);
					if (status == noMoreItems)
						break;
					if (status != success)
						throw new InvalidDataException(LocalizationManager.Get(
							"Release.Error.MsiPropertyIncomplete",
							status));

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
				throw new InvalidDataException(LocalizationManager.Get(
					"Release.Error.MsiValueRead",
					status));

			StringBuilder value = new(checked((int)characterCount + 1));
			uint capacity = (uint)value.Capacity;
			status = MsiRecordGetString(
				record,
				field,
				value,
				ref capacity);
			if (status != success)
				throw new InvalidDataException(LocalizationManager.Get(
					"Release.Error.MsiValueRead",
					status));
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
					LocalizationManager.Get("Release.Check.Manifest"),
					LocalizationManager.Get("Release.Check.ManifestMissing"));
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
					LocalizationManager.Get("Release.Check.Manifest"),
					LocalizationManager.Get(
						"Release.Check.ManifestReadFailed",
						exception.Message));
				return;
			}
			List<string> problems = [];
			if (!manifest.TryGetValue("FormatVersion", out string? format) || format != "3")
				problems.Add(LocalizationManager.Get(
					"Release.Check.ManifestFormatUnsupported"));
			if (!manifest.TryGetValue("Channel", out string? channel) ||
				!Core.IsOfficialChannel(channel))
			{
				problems.Add(LocalizationManager.Get(
					"Release.Check.ManifestNotStable"));
			}
			if (!manifest.TryGetValue("Version", out string? versionText) ||
				!Core.TryParseVersionText(versionText, out Version? manifestVersion) ||
				expectedVersion is null || manifestVersion != expectedVersion)
			{
				problems.Add(LocalizationManager.Get(
					"Release.Check.ManifestVersionMismatch"));
			}
			if (!ManifestValueMatches(
				manifest,
				"StandaloneFile",
				PublishedExecutableName))
			{
				problems.Add(LocalizationManager.Get(
					"Release.Check.StandaloneNameIncorrect"));
			}
			if (!ManifestValueMatches(
				manifest,
				"MsiFile",
				MsiFileName))
			{
				problems.Add(LocalizationManager.Get(
					"Release.Check.MsiNameIncorrect"));
			}
			if (!HashMatches(manifest, "StandaloneSha256", standaloneHash))
				problems.Add(LocalizationManager.Get(
					"Release.Check.StandaloneHashMismatch"));
			if (!HashMatches(manifest, "MsiSha256", msiHash))
				problems.Add(LocalizationManager.Get(
					"Release.Check.MsiHashMismatch"));

			if (problems.Count == 0)
			{
				AddPassed(
					items,
					LocalizationManager.Get("Release.Check.Manifest"),
					LocalizationManager.Get("Release.Check.ManifestValid"));
			}
			else
			{
				AddFailure(
					items,
					LocalizationManager.Get("Release.Check.Manifest"),
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
					LocalizationManager.Get("Release.Check.MsiSettings"),
					LocalizationManager.Get(
						"Release.Check.MsiProjectFileMissing",
						!File.Exists(msiProjectPath)
							? msiProjectPath
							: msiSourcePath));
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
					LocalizationManager.Get("Release.Check.MsiSettings"),
					LocalizationManager.Get(
						"Release.Check.MsiProjectReadFailed",
						exception.Message));
				return;
			}

			string combined = msiProject + Environment.NewLine + msiSource;
			(string Text, string DescriptionKey)[] requirements =
			[
				(ExpectedUpgradeCode, "Release.Requirement.FixedUpgradeCode"),
				("<Version>$(Version)</Version>", "Release.Requirement.SharedVersion"),
				("SynixVersion=$(Version)", "Release.Requirement.VersionForwarding"),
				("Version=\"$(var.SynixVersion)\"", "Release.Requirement.PackageVersion"),
				("Scope=\"perUser\"", "Release.Requirement.PerUser"),
				("<MajorUpgrade", "Release.Requirement.AutomaticUpgrades"),
				("AllowSameVersionUpgrades=\"yes\"", "Release.Requirement.SafeSameVersion"),
				("<OutputName>SynixSetup</OutputName>", "Release.Requirement.MsiFilename"),
				("<InstallerPlatform>x64</InstallerPlatform>", "Release.Requirement.X64"),
				(@"Software\ubidzz\Synix Control Panel", "Release.Requirement.StableRegistration"),
				("SynixInstallSource", "Release.Requirement.SourceRegistration")
			];
			List<string> missing = requirements
				.Where(requirement => !combined.Contains(
					requirement.Text,
					StringComparison.OrdinalIgnoreCase))
				.Select(requirement => LocalizationManager.Get(
					requirement.DescriptionKey))
				.ToList();
			if (combined.Contains(@"C:\Users\", StringComparison.OrdinalIgnoreCase))
				missing.Add(LocalizationManager.Get(
					"Release.Requirement.PortablePaths"));

			if (missing.Count == 0)
			{
				AddPassed(
					items,
					LocalizationManager.Get("Release.Check.MsiSettings"),
					LocalizationManager.Get("Release.Check.MsiSettingsValid"));
			}
			else
			{
				AddFailure(
					items,
					LocalizationManager.Get("Release.Check.MsiSettings"),
					LocalizationManager.Get(
						"Release.Check.MissingOrIncorrect",
						string.Join(", ", missing)));
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
					LocalizationManager.Get("Release.Check.AutomatedTests"),
					LocalizationManager.Get("Release.Check.TestReceiptMissing"));
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
						LocalizationManager.Get("Release.Check.AutomatedTests"),
						LocalizationManager.Get(
							"Release.Check.TestsPassed",
							completedUtc.ToLocalTime()));
				}
				else
				{
					AddFailure(
						items,
						LocalizationManager.Get("Release.Check.AutomatedTests"),
						LocalizationManager.Get("Release.Check.TestReceiptInvalid"));
				}
			}
			catch (Exception exception)
			{
				AddFailure(
					items,
					LocalizationManager.Get("Release.Check.AutomatedTests"),
					LocalizationManager.Get(
						"Release.Check.TestReceiptReadFailed",
						exception.Message));
			}
		}

		private static void CheckSecurityRegressionReceipt(
			List<SynixReleaseCheckItem> items,
			string manifestPath)
		{
			if (!File.Exists(manifestPath))
			{
				AddFailure(
					items,
					LocalizationManager.Get("Release.Check.SecuritySuite"),
					LocalizationManager.Get("Release.Check.SecurityReceiptMissing"));
				return;
			}

			try
			{
				IReadOnlyDictionary<string, string> manifest = ReadManifest(manifestPath);
				if (TryGetPassingSecurityReceipt(manifest, out DateTimeOffset completedUtc))
				{
					AddPassed(
						items,
						LocalizationManager.Get("Release.Check.SecuritySuite"),
						LocalizationManager.Get(
							"Release.Check.SecurityTestsPassed",
							completedUtc.ToLocalTime()));
				}
				else
				{
					AddFailure(
						items,
						LocalizationManager.Get("Release.Check.SecuritySuite"),
						LocalizationManager.Get("Release.Check.SecurityReceiptInvalid"));
				}
			}
			catch (Exception exception)
			{
				AddFailure(
					items,
					LocalizationManager.Get("Release.Check.SecuritySuite"),
					LocalizationManager.Get(
						"Release.Check.SecurityReceiptReadFailed",
						exception.Message));
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
			return version?.ToString(3) ?? LocalizationManager.Get("Report.Unknown");
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
