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
using Synix_Control_Panel.SynixEngine;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class SynixReleaseReadinessCheckerTests
{
	[Fact]
	public void Report_WithNoFailures_IsReady()
	{
		SynixReleaseReadinessReport report = CreateReport(
			new SynixReleaseCheckItem(
				SynixReleaseCheckLevel.Passed,
				"Versions",
				"Versions match."),
			new SynixReleaseCheckItem(
				SynixReleaseCheckLevel.Warning,
				"Optional note",
				"Review this note."));

		Assert.True(report.IsReady);
		Assert.Equal(1, report.PassedCount);
		Assert.Equal(1, report.WarningCount);
		Assert.Equal(0, report.FailedCount);
	}

	[Fact]
	public void Report_WithAnyFailure_IsNotReady()
	{
		SynixReleaseReadinessReport report = CreateReport(
			new SynixReleaseCheckItem(
				SynixReleaseCheckLevel.Passed,
				"Versions",
				"Versions match."),
			new SynixReleaseCheckItem(
				SynixReleaseCheckLevel.Failed,
				"Installer",
				"Installer is missing."));

		Assert.False(report.IsReady);
		Assert.Equal(1, report.FailedCount);
		Assert.Contains("Result: NOT READY", report.ToPlainText());
	}

	[Fact]
	public void PlainTextReport_UsesExactGitHubAssetNamesAndHashes()
	{
		SynixReleaseReadinessReport report = CreateReport(
			new SynixReleaseCheckItem(
				SynixReleaseCheckLevel.Passed,
				"Everything",
				"Ready."));

		string text = report.ToPlainText();

		Assert.Contains("Synix.Control.Panel.exe  SHA-256: standalone-hash", text);
		Assert.Contains("SynixSetup.msi           SHA-256: setup-hash", text);
		Assert.Contains(
			"Upload the published 'Synix Control Panel.exe' as 'Synix.Control.Panel.exe'.",
			text);
		Assert.Contains(
			"Upload the published 'SynixSetup.msi' as 'SynixSetup.msi'.",
			text);
	}

	[Fact]
	public void ManifestReader_AcceptsUtf8MarkerCommentsAndEqualsInValues()
	{
		using TemporaryDirectory temporary = new();
		string manifestPath = Path.Combine(
			temporary.Path,
			Core.ManifestFileName);
		File.WriteAllText(
			manifestPath,
			"\uFEFFFormatVersion=1\n# generated\nChannel = Stable\nNote=value=with=equals\n");

		IReadOnlyDictionary<string, string> manifest =
			Core.ReadManifest(manifestPath);

		Assert.Equal("1", manifest["FormatVersion"]);
		Assert.Equal("Stable", manifest["channel"]);
		Assert.Equal("value=with=equals", manifest["Note"]);
	}

	[Fact]
	public void PassingTestReceipt_RequiresPassedResultAndUtcTime()
	{
		Dictionary<string, string> valid = new(
			StringComparer.OrdinalIgnoreCase)
		{
			["AutomatedTests"] = "Passed",
			["AutomatedTestsUtc"] = "2026-08-21T12:30:00.0000000Z"
		};
		Dictionary<string, string> failed = new(valid)
		{
			["AutomatedTests"] = "Failed"
		};
		Dictionary<string, string> missingTime = new(valid);
		missingTime.Remove("AutomatedTestsUtc");

		Assert.True(Core.TryGetPassingTestReceipt(
			valid,
			out DateTimeOffset completedUtc));
		Assert.Equal(
			new DateTimeOffset(2026, 8, 21, 12, 30, 0, TimeSpan.Zero),
			completedUtc);
		Assert.False(Core.TryGetPassingTestReceipt(
			failed,
			out _));
		Assert.False(Core.TryGetPassingTestReceipt(
			missingTime,
			out _));
	}

	[Fact]
	public void ProjectFolderFinder_WalksUpFromBuildFolder()
	{
		using TemporaryDirectory temporary = new();
		string projectPath = Path.Combine(
			temporary.Path,
			"Synix Control Panel.csproj");
		string buildPath = Path.Combine(
			temporary.Path,
			"bin",
			"Release",
			"net8.0-windows");
		Directory.CreateDirectory(buildPath);
		File.WriteAllText(projectPath, "<Project />");

		string? found = Core.FindProjectDirectory(
			buildPath);

		Assert.Equal(Path.GetFullPath(temporary.Path), found);
	}

	[Fact]
	public void PublishFolderFinder_ReadsRelativeVisualStudioProfile()
	{
		using TemporaryDirectory temporary = new();
		string profiles = Path.Combine(
			temporary.Path,
			"Properties",
			"PublishProfiles");
		string publish = Path.Combine(temporary.Path, "publish-output");
		Directory.CreateDirectory(profiles);
		Directory.CreateDirectory(publish);
		File.WriteAllText(
			Path.Combine(
				publish,
				Core.PublishedExecutableName),
			"test");
		File.WriteAllText(
			Path.Combine(profiles, "FolderProfile.pubxml"),
			"<Project><PropertyGroup><PublishDir>publish-output</PublishDir></PropertyGroup></Project>");

		string? found = Core.FindPublishDirectory(
			temporary.Path);

		Assert.Equal(Path.GetFullPath(publish), found);
	}

	[Fact]
	public void ReleaseManifestFinder_UsesPublishFolderBackupWhenPrimaryIsMissing()
	{
		using TemporaryDirectory temporary = new();
		string backupPath = Path.Combine(
			temporary.Path,
			Core.ManifestBackupRelativePath);
		Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
		File.WriteAllText(backupPath, "FormatVersion=1");

		string found = Core.FindReleaseManifestPath(
			temporary.Path);

		Assert.Equal(backupPath, found);
	}

	[Fact]
	public void ReleaseManifestFinder_PrefersPrimaryReceipt()
	{
		using TemporaryDirectory temporary = new();
		string primaryPath = Path.Combine(
			temporary.Path,
			Core.ManifestFileName);
		string backupPath = Path.Combine(
			temporary.Path,
			Core.ManifestBackupRelativePath);
		Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
		File.WriteAllText(primaryPath, "primary");
		File.WriteAllText(backupPath, "backup");

		string found = Core.FindReleaseManifestPath(
			temporary.Path);

		Assert.Equal(primaryPath, found);
	}

	private static SynixReleaseReadinessReport CreateReport(
		params SynixReleaseCheckItem[] items)
	{
		return new SynixReleaseReadinessReport(
			new Version(1, 0, 21),
			@"C:\Project",
			@"C:\Published",
			"standalone-hash",
			"setup-hash",
			items);
	}

	private sealed class TemporaryDirectory : IDisposable
	{
		public TemporaryDirectory()
		{
			Path = System.IO.Path.Combine(
				System.IO.Path.GetTempPath(),
				$"SynixReleaseCheckerTests-{Guid.NewGuid():N}");
			Directory.CreateDirectory(Path);
		}

		public string Path { get; }

		public void Dispose()
		{
			if (Directory.Exists(Path))
				Directory.Delete(Path, recursive: true);
		}
	}
}
