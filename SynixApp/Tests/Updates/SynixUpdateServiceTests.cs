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
using System.Text.Json;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class SynixUpdateServiceTests
{
	private static readonly string StandaloneDigest =
		"sha256:" + new string('a', 64);
	private static readonly string MsiDigest =
		"sha256:" + new string('b', 64);

	[Fact]
	public void NormalBuilds_AreMarkedAsDevelopmentBuilds()
	{
		Assert.Equal(
			Core.DevelopmentChannel,
			Core.UpdateChannel);
		Assert.False(Core.IsOfficialRelease);
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
			Core.IsOfficialChannel(channel));
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
		bool parsed = Core.TryParseVersionText(
			text,
			out Version? version);

		Assert.True(parsed);
		Assert.Equal(new Version(major, minor, build), version);
	}

	[Fact]
	public void DevelopmentBuild_NeverUsesInstalledEditionDetection()
	{
		SynixInstallation installation = Core.DetectInstallation(
			@"C:\Users\Player\AppData\Roaming\Synix\Synix Control Panel.exe",
			@"C:\Users\Player\AppData\Roaming\Synix\",
			"WinGet",
			officialRelease: false);

		Assert.Equal(SynixInstallationKind.Development, installation.Kind);
		Assert.False(installation.CanInstallUpdates);
	}

	[Fact]
	public async Task DevelopmentBuild_UpdaterCannotCreateSelfCopy()
	{
		SynixUpdateCheckResult check = new(
			new Version(1, 0, 21),
			new Version(1, 0, 22),
			new SynixInstallation(
				SynixInstallationKind.Development,
				@"C:\Build\Synix Control Panel.exe",
				null),
			null,
			null,
			null);
		InvalidOperationException exception = await Assert.ThrowsAsync<
			InvalidOperationException>(() => Core.PrepareUpdateAsync(check));

		Assert.Contains("official Stable", exception.Message);
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
		SynixInstallation installation = Core.DetectInstallation(
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
		SynixInstallation installation = Core.DetectInstallation(
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
		SynixInstallation installation = Core.DetectInstallation(
			@"C:\Users\Player\AppData\Roaming\Synix\Synix.Control.Panel.exe",
			@"C:\Users\Player\AppData\Roaming\Synix\",
			"WinGet",
			officialRelease: true);

		Assert.Equal(SynixInstallationKind.Standalone, installation.Kind);
	}

	[Fact]
	public void StableGitHubRelease_ParsesVerifiedAssets()
	{
		SynixReleaseInfo release = Core.ParseReleaseJson(
			BuildReleaseJson());

		Assert.Equal(new Version(1, 0, 22), release.Version);
		Assert.Equal("1.0.22", release.VersionText);
		Assert.Equal("Synix v1.0.22", release.Name);
		Assert.Equal(2, release.Assets.Count);
		Assert.Equal(new string('a', 64), release.Assets[0].Sha256);
		Assert.Equal(new string('b', 64), release.Assets[1].Sha256);
		Assert.Equal(
			Core.StandaloneAssetName,
			release.Assets[0].Name);
	}

	[Fact]
	public void ReleaseName_RepairsMalformedTwoPartTag()
	{
		SynixReleaseInfo release = Core.ParseReleaseJson(
			BuildReleaseJson(
				tagName: "v1.023",
				releaseName: "v1.0.23"));

		Assert.Equal(new Version(1, 0, 23), release.Version);
		Assert.Equal("1.0.23", release.VersionText);
	}

	[Fact]
	public void ValidThreePartTag_RemainsAuthoritative()
	{
		SynixReleaseInfo release = Core.ParseReleaseJson(
			BuildReleaseJson(
				tagName: "v1.0.22",
				releaseName: "Synix v9.9.9"));

		Assert.Equal(new Version(1, 0, 22), release.Version);
	}

	[Theory]
	[InlineData("Synix.Control.Panel.exe", "4D5A000000000000")]
	[InlineData("SynixSetup.msi", "D0CF11E0A1B11AE1")]
	public void UpdateDownloads_AcceptExpectedWindowsPackageHeader(
		string assetName,
		string headerHex)
	{
		using MemoryStream stream = new(Convert.FromHexString(headerHex));

		Core.ValidateDownloadedAssetHeader(stream, assetName);
	}

	[Theory]
	[InlineData("Synix.Control.Panel.exe", "D0CF11E0A1B11AE1")]
	[InlineData("SynixSetup.msi", "4D5A000000000000")]
	public void UpdateDownloads_RejectWrongWindowsPackageHeader(
		string assetName,
		string headerHex)
	{
		using MemoryStream stream = new(Convert.FromHexString(headerHex));

		Assert.Throws<InvalidDataException>(() =>
			Core.ValidateDownloadedAssetHeader(stream, assetName));
	}

	[Fact]
	public void DraftAndPrereleaseUpdates_AreRejected()
	{
		Assert.Throws<InvalidDataException>(() =>
			Core.ParseReleaseJson(
				BuildReleaseJson(draft: true)));
		Assert.Throws<InvalidDataException>(() =>
			Core.ParseReleaseJson(
				BuildReleaseJson(prerelease: true)));
	}

	[Fact]
	public void AssetsWithoutOfficialUrlDigestOrSafeSize_AreIgnored()
	{
		object[] unsafeAssets =
		[
			CreateAsset(
				Core.StandaloneAssetName,
				"https://example.com/Synix.Control.Panel.exe",
				StandaloneDigest,
				1024),
			CreateAsset(
				Core.MsiAssetName,
				"https://github.com/ubidzz/Synix-Control-Panel/releases/download/v1.0.22/SynixSetup.msi",
				string.Empty,
				1024),
			CreateAsset(
				"TooLarge.exe",
				"https://github.com/ubidzz/Synix-Control-Panel/releases/download/v1.0.22/TooLarge.exe",
				StandaloneDigest,
				600L * 1024 * 1024)
		];

		SynixReleaseInfo release = Core.ParseReleaseJson(
			BuildReleaseJson(assets: unsafeAssets));

		Assert.Empty(release.Assets);
	}

	[Theory]
	[InlineData(SynixInstallationKind.Standalone, "Synix.Control.Panel.exe")]
	[InlineData(SynixInstallationKind.Setup, "SynixSetup.msi")]
	[InlineData(SynixInstallationKind.WinGet, "SynixSetup.msi")]
	public void Edition_SelectsItsExactVerifiedDownload(
		SynixInstallationKind installationKind,
		string expectedAsset)
	{
		SynixReleaseInfo release = Core.ParseReleaseJson(
			BuildReleaseJson());

		SynixReleaseAsset? asset = Core.SelectAsset(
			release,
			installationKind);

		Assert.NotNull(asset);
		Assert.Equal(expectedAsset, asset.Name);
	}

	[Fact]
	public void UpdateReadiness_RequiresMatchingReleaseAndOfficialBuild()
	{
		SynixReleaseInfo release = Core.ParseReleaseJson(
			BuildReleaseJson());
		SynixReleaseAsset asset = Assert.Single(
			release.Assets,
			item => item.Name == Core.StandaloneAssetName);
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

		string highlights = Core.BuildHighlights(notes);

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

		string formatted = Core.FormatReleaseNotes(notes);

		Assert.Contains("SAFETY", formatted);
		Assert.Contains("• Verify the SHA-256 digest.", formatted);
	}

	private static string BuildReleaseJson(
		bool draft = false,
		bool prerelease = false,
		object[]? assets = null,
		string tagName = "v1.0.22",
		string releaseName = "Synix v1.0.22")
	{
		assets ??=
		[
			CreateAsset(
				Core.StandaloneAssetName,
				"https://github.com/ubidzz/Synix-Control-Panel/releases/download/v1.0.22/Synix.Control.Panel.exe",
				StandaloneDigest,
				9_345_994),
			CreateAsset(
				Core.MsiAssetName,
				"https://github.com/ubidzz/Synix-Control-Panel/releases/download/v1.0.22/SynixSetup.msi",
				MsiDigest,
				6_519_406)
		];

		return JsonSerializer.Serialize(new
		{
			draft,
			prerelease,
			tag_name = tagName,
			html_url = "https://github.com/ubidzz/Synix-Control-Panel/releases/tag/v1.0.22",
			name = releaseName,
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
