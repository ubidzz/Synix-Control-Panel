using Synix_Control_Panel.SynixEngine;
using System.Text.Json;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class SynixUpdateServiceTests
{
	private static readonly string StandaloneDigest =
		"sha256:" + new string('a', 64);
	private static readonly string SetupDigest =
		"sha256:" + new string('b', 64);

	[Fact]
	public void NormalBuilds_AreMarkedAsDevelopmentBuilds()
	{
		Assert.Equal(
			SynixBuildInfo.DevelopmentChannel,
			SynixBuildInfo.UpdateChannel);
		Assert.False(SynixBuildInfo.IsOfficialRelease);
	}

	[Theory]
	[InlineData("Stable", true)]
	[InlineData("stable", true)]
	[InlineData("Development", false)]
	[InlineData("", false)]
	[InlineData(null, false)]
	public void OnlyStableChannel_CanInstallUpdates(
		string? channel,
		bool expectedOfficial)
	{
		Assert.Equal(
			expectedOfficial,
			SynixBuildInfo.IsOfficialChannel(channel));
	}

	[Theory]
	[InlineData("1.0.20", 1, 0, 20)]
	[InlineData("v1.0.20", 1, 0, 20)]
	[InlineData("\uFEFF1.0.20", 1, 0, 20)]
	[InlineData("\uFEFF  v1.0.20  ", 1, 0, 20)]
	public void VersionText_AcceptsGitHubUtf8Marker(
		string text,
		int major,
		int minor,
		int build)
	{
		bool parsed = SynixUpdateService.TryParseVersionText(
			text,
			out Version? version);

		Assert.True(parsed);
		Assert.Equal(new Version(major, minor, build), version);
	}

	[Fact]
	public void DevelopmentBuild_NeverUsesInstalledEditionDetection()
	{
		SynixInstallation installation = SynixUpdateService.DetectInstallation(
			@"C:\Users\Player\AppData\Roaming\Synix\Synix Control Panel.exe",
			@"C:\Users\Player\AppData\Roaming\Synix\",
			"WinGet",
			officialRelease: false);

		Assert.Equal(SynixInstallationKind.Development, installation.Kind);
		Assert.False(installation.CanInstallUpdates);
	}

	[Theory]
	[InlineData(null, SynixInstallationKind.Setup)]
	[InlineData("Setup", SynixInstallationKind.Setup)]
	[InlineData("WinGet", SynixInstallationKind.WinGet)]
	[InlineData("winget", SynixInstallationKind.WinGet)]
	public void InstalledExecutable_UsesSetupOrWinGetEdition(
		string? installSource,
		SynixInstallationKind expectedKind)
	{
		SynixInstallation installation = SynixUpdateService.DetectInstallation(
			@"C:\Users\Player\AppData\Roaming\Synix\Synix Control Panel.exe",
			@"C:\Users\Player\AppData\Roaming\Synix\",
			installSource,
			officialRelease: true);

		Assert.Equal(expectedKind, installation.Kind);
		Assert.True(installation.CanInstallUpdates);
	}

	[Fact]
	public void StandaloneCopy_RemainsStandaloneWhenSetupIsAlsoInstalled()
	{
		SynixInstallation installation = SynixUpdateService.DetectInstallation(
			@"D:\Portable Apps\Synix Control Panel.exe",
			@"C:\Users\Player\AppData\Roaming\Synix\",
			"WinGet",
			officialRelease: true);

		Assert.Equal(SynixInstallationKind.Standalone, installation.Kind);
		Assert.Equal(
			Path.GetFullPath(@"D:\Portable Apps\Synix Control Panel.exe"),
			installation.ExecutablePath);
	}

	[Fact]
	public void DifferentlyNamedStandalone_InSetupFolderRemainsStandalone()
	{
		SynixInstallation installation = SynixUpdateService.DetectInstallation(
			@"C:\Users\Player\AppData\Roaming\Synix\Synix.Control.Panel.exe",
			@"C:\Users\Player\AppData\Roaming\Synix\",
			"WinGet",
			officialRelease: true);

		Assert.Equal(SynixInstallationKind.Standalone, installation.Kind);
	}

	[Fact]
	public void StableGitHubRelease_ParsesVerifiedAssets()
	{
		SynixReleaseInfo release = SynixUpdateService.ParseReleaseJson(
			BuildReleaseJson());

		Assert.Equal(new Version(1, 0, 22), release.Version);
		Assert.Equal("1.0.22", release.VersionText);
		Assert.Equal("Synix v1.0.22", release.Name);
		Assert.Equal(2, release.Assets.Count);
		Assert.Equal(new string('a', 64), release.Assets[0].Sha256);
		Assert.Equal(new string('b', 64), release.Assets[1].Sha256);
		Assert.Equal(
			SynixUpdateService.StandaloneAssetName,
			release.Assets[0].Name);
	}

	[Fact]
	public void DraftAndPrereleaseUpdates_AreRejected()
	{
		Assert.Throws<InvalidDataException>(() =>
			SynixUpdateService.ParseReleaseJson(
				BuildReleaseJson(draft: true)));
		Assert.Throws<InvalidDataException>(() =>
			SynixUpdateService.ParseReleaseJson(
				BuildReleaseJson(prerelease: true)));
	}

	[Fact]
	public void AssetsWithoutOfficialUrlDigestOrSafeSize_AreIgnored()
	{
		object[] unsafeAssets =
		[
			CreateAsset(
				SynixUpdateService.StandaloneAssetName,
				"https://example.com/Synix.Control.Panel.exe",
				StandaloneDigest,
				1024),
			CreateAsset(
				SynixUpdateService.SetupAssetName,
				"https://github.com/ubidzz/Synix-Control-Panel/releases/download/v1.0.22/SynixSetup.exe",
				string.Empty,
				1024),
			CreateAsset(
				"TooLarge.exe",
				"https://github.com/ubidzz/Synix-Control-Panel/releases/download/v1.0.22/TooLarge.exe",
				StandaloneDigest,
				600L * 1024 * 1024)
		];

		SynixReleaseInfo release = SynixUpdateService.ParseReleaseJson(
			BuildReleaseJson(assets: unsafeAssets));

		Assert.Empty(release.Assets);
	}

	[Theory]
	[InlineData(SynixInstallationKind.Standalone, "Synix.Control.Panel.exe")]
	[InlineData(SynixInstallationKind.Setup, "SynixSetup.exe")]
	[InlineData(SynixInstallationKind.WinGet, "SynixSetup.exe")]
	public void Edition_SelectsItsExactVerifiedDownload(
		SynixInstallationKind installationKind,
		string expectedAsset)
	{
		SynixReleaseInfo release = SynixUpdateService.ParseReleaseJson(
			BuildReleaseJson());

		SynixReleaseAsset? asset = SynixUpdateService.SelectAsset(
			release,
			installationKind);

		Assert.NotNull(asset);
		Assert.Equal(expectedAsset, asset.Name);
	}

	[Fact]
	public void UpdateReadiness_RequiresMatchingReleaseAndOfficialBuild()
	{
		SynixReleaseInfo release = SynixUpdateService.ParseReleaseJson(
			BuildReleaseJson());
		SynixReleaseAsset asset = Assert.Single(
			release.Assets,
			item => item.Name == SynixUpdateService.StandaloneAssetName);
		SynixInstallation development = new(
			SynixInstallationKind.Development,
			@"C:\Build\Synix Control Panel.exe",
			null);
		SynixInstallation standalone = new(
			SynixInstallationKind.Standalone,
			@"D:\Synix Control Panel.exe",
			null);

		SynixUpdateCheckResult developmentCheck = new(
			new Version(1, 0, 21),
			new Version(1, 0, 22),
			development,
			release,
			asset,
			null);
		SynixUpdateCheckResult standaloneCheck = developmentCheck with
		{
			Installation = standalone
		};

		Assert.True(developmentCheck.ReleaseReady);
		Assert.False(developmentCheck.CanInstall);
		Assert.True(standaloneCheck.CanInstall);
	}

	[Fact]
	public void ReleaseHighlights_AreShortAndRemoveMarkdownLinks()
	{
		string notes = """
			## UI
			- Added [automatic updates](https://example.com/update).
			- Added full release notes.
			- Added rollback.
			- Added Setup support.
			- Added Standalone support.
			- Added WinGet support.
			- This seventh item should not appear.
			""";

		string highlights = SynixUpdateService.BuildHighlights(notes);

		Assert.Equal(6, highlights.Split(Environment.NewLine).Length);
		Assert.Contains("automatic updates", highlights);
		Assert.DoesNotContain("https://", highlights);
		Assert.DoesNotContain("seventh", highlights);
	}

	[Fact]
	public void FullReleaseNotes_ConvertCommonMarkdownForPlainTextWindow()
	{
		string notes = """
			## Safety
			- Verify the **SHA-256** digest.
			""";

		string formatted = SynixUpdateService.FormatReleaseNotes(notes);

		Assert.Contains("SAFETY", formatted);
		Assert.Contains("• Verify the SHA-256 digest.", formatted);
	}

	private static string BuildReleaseJson(
		bool draft = false,
		bool prerelease = false,
		object[]? assets = null)
	{
		assets ??=
		[
			CreateAsset(
				SynixUpdateService.StandaloneAssetName,
				"https://github.com/ubidzz/Synix-Control-Panel/releases/download/v1.0.22/Synix.Control.Panel.exe",
				StandaloneDigest,
				9_345_994),
			CreateAsset(
				SynixUpdateService.SetupAssetName,
				"https://github.com/ubidzz/Synix-Control-Panel/releases/download/v1.0.22/SynixSetup.exe",
				SetupDigest,
				6_519_406)
		];

		return JsonSerializer.Serialize(new
		{
			draft,
			prerelease,
			tag_name = "v1.0.22",
			html_url = "https://github.com/ubidzz/Synix-Control-Panel/releases/tag/v1.0.22",
			name = "Synix v1.0.22",
			body = "## Update\n- Safer automatic updates.",
			published_at = "2026-08-21T12:00:00Z",
			assets
		});
	}

	private static object CreateAsset(
		string name,
		string url,
		string digest,
		long size)
	{
		return new
		{
			name,
			size,
			browser_download_url = url,
			digest
		};
	}
}
