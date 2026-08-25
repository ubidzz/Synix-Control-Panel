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
using System.IO.Compression;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class OxideRuntimeManagerTests
{
	[Fact]
	public void PublishedSha256DigestIsStrictlyValidated()
	{
		string hash = new('a', 64);

		Assert.Equal(hash.ToUpperInvariant(),
			OxideRuntimeManager.NormalizeSha256Digest("sha256:" + hash));
		Assert.Throws<InvalidDataException>(() =>
			OxideRuntimeManager.NormalizeSha256Digest(hash));
		Assert.Throws<InvalidDataException>(() =>
			OxideRuntimeManager.NormalizeSha256Digest("sha256:not-a-hash"));
	}

	[Theory]
	[InlineData("https://github.com/OxideMod/Oxide.Rust/releases/download/2.0.7598/Oxide.Rust.zip", true)]
	[InlineData("http://github.com/OxideMod/Oxide.Rust/releases/download/2.0.7598/Oxide.Rust.zip", false)]
	[InlineData("https://example.com/Oxide.Rust.zip", false)]
	[InlineData("https://github.com/Other/Oxide.Rust/releases/download/2.0.7598/Oxide.Rust.zip", false)]
	[InlineData("https://github.com/OxideMod/Oxide.Rust/releases/download/2.0.7598/Oxide.Rust-linux.zip", false)]
	[InlineData("https://github.com:444/OxideMod/Oxide.Rust/releases/download/2.0.7598/Oxide.Rust.zip", false)]
	[InlineData("https://github.com/OxideMod/Oxide.Rust/releases/download/2.0.7598/Oxide.Rust.zip?untrusted=1", false)]
	public void OnlyTheOfficialWindowsReleaseAssetIsAccepted(string url, bool expected)
	{
		Assert.Equal(expected,
			OxideRuntimeManager.TryValidateDownloadUri(url, "2.0.7598", out _));
	}

	[Fact]
	public void ArchiveExtractionRejectsPathsOutsideTheStagingFolder()
	{
		string root = CreateTestRoot();
		string archivePath = Path.Combine(root, "unsafe.zip");
		string destination = Path.Combine(root, "staging");
		try
		{
			using (ZipArchive archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
			{
				ZipArchiveEntry entry = archive.CreateEntry("../outside.txt");
				using StreamWriter writer = new(entry.Open());
				writer.Write("unsafe");
			}

			Assert.Throws<InvalidDataException>(() =>
				OxideRuntimeManager.ExtractArchiveSafely(archivePath, destination));
			Assert.False(File.Exists(Path.Combine(root, "outside.txt")));
		}
		finally
		{
			Directory.Delete(root, true);
		}
	}

	[Fact]
	public void ValidArchiveAndOverlayStayInsideTheServerFolder()
	{
		string root = CreateTestRoot();
		string archivePath = Path.Combine(root, "oxide.zip");
		string staging = Path.Combine(root, "staging");
		string server = Path.Combine(root, "server");
		string rollback = Path.Combine(root, "rollback");
		try
		{
			using (ZipArchive archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
			{
				ZipArchiveEntry entry = archive.CreateEntry(
					"RustDedicated_Data/Managed/Oxide.Core.dll");
				using StreamWriter writer = new(entry.Open());
				writer.Write("trusted-test-content");
			}

			OxideRuntimeManager.ExtractArchiveSafely(archivePath, staging);
			Directory.CreateDirectory(server);
			OxideRuntimeManager.ApplyOverlayWithRollback(staging, server, rollback);

			Assert.Equal(
				"trusted-test-content",
				File.ReadAllText(Path.Combine(
					server,
					"RustDedicated_Data",
					"Managed",
					"Oxide.Core.dll")));
		}
		finally
		{
			Directory.Delete(root, true);
		}
	}

	private static string CreateTestRoot()
	{
		string path = Path.Combine(
			Path.GetTempPath(),
			$"SynixOxideTests-{Guid.NewGuid():N}");
		Directory.CreateDirectory(path);
		return path;
	}
}
