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

public sealed class SynixTransferPackageTests
{
	private const string TransferPassword =
		"Permanent Synix test password 123!";

	[Fact]
	public async Task EncryptedPackage_VerifiesAndRestoresEveryFile()
	{
		using TemporaryDirectory test = new();
		string source = test.CreateSourceTree();
		string package = test.PathFor("encrypted.synixbackup");
		string restored = test.PathFor("encrypted-restored");

		await Core.ExportAsync(
			source,
			package,
			TransferPassword);

		SynixImportEstimate estimate = Core.EstimateImport(
			package,
			restored);
		Assert.True(estimate.IsPasswordProtected);
		Assert.True(estimate.UsesLowDiskFormat);
		Assert.Equal(3, estimate.FileCount);

		await Core.VerifyAsync(
			package,
			TransferPassword);
		await Core.ImportAsync(
			package,
			restored,
			TransferPassword);

		AssertDirectoryTreesEqual(source, restored);
	}

	[Fact]
	public async Task UnencryptedPackage_VerifiesAndRestoresEveryFile()
	{
		using TemporaryDirectory test = new();
		string source = test.CreateSourceTree();
		string package = test.PathFor("normal.synixbackup");
		string restored = test.PathFor("normal-restored");

		await Core.ExportUnencryptedAsync(source, package);

		SynixImportEstimate estimate = Core.EstimateImport(
			package,
			restored);
		Assert.False(estimate.IsPasswordProtected);
		Assert.True(estimate.UsesLowDiskFormat);

		await Core.VerifyAsync(package, string.Empty);
		await Core.ImportAsync(
			package,
			restored,
			string.Empty);

		AssertDirectoryTreesEqual(source, restored);
	}

	[Fact]
	public async Task Export_DoesNotCarryGitHubAuthorizationToAnotherComputer()
	{
		using TemporaryDirectory test = new();
		string source = test.CreateSourceTree();
		string connectionDirectory = Path.Combine(source, "SynixData");
		Directory.CreateDirectory(connectionDirectory);
		await File.WriteAllTextAsync(
			Path.Combine(connectionDirectory, "github-connection.json"),
			"encrypted GitHub authorization");
		string package = test.PathFor("without-github.synixbackup");
		string restored = test.PathFor("without-github-restored");

		await Core.ExportUnencryptedAsync(source, package);
		await Core.ImportAsync(package, restored, string.Empty);

		Assert.False(File.Exists(Path.Combine(
			restored,
			"SynixData",
			"github-connection.json")));
		Assert.True(File.Exists(Path.Combine(restored, "settings.json")));
	}

	[Fact]
	public async Task WrongPassword_IsRejectedBeforeDestinationIsChanged()
	{
		using TemporaryDirectory test = new();
		string source = test.CreateSourceTree();
		string package = test.PathFor("encrypted.synixbackup");
		string destination = test.PathFor("destination");

		await Core.ExportAsync(
			source,
			package,
			TransferPassword);

		await Assert.ThrowsAsync<InvalidDataException>(() =>
			Core.ImportAsync(
				package,
				destination,
				"This password is incorrect"));

		Assert.False(Directory.Exists(destination));
	}

	[Fact]
	public async Task DamagedUnencryptedPackage_FailsVerification()
	{
		using TemporaryDirectory test = new();
		string source = test.CreateSourceTree();
		string package = test.PathFor("normal.synixbackup");
		string damagedPackage = test.PathFor("normal-damaged.synixbackup");

		await Core.ExportUnencryptedAsync(source, package);
		byte[] packageBytes = await File.ReadAllBytesAsync(package);
		packageBytes[packageBytes.Length / 2] ^= 0x5A;
		await File.WriteAllBytesAsync(damagedPackage, packageBytes);

		await Assert.ThrowsAsync<InvalidDataException>(() =>
			Core.VerifyAsync(
				damagedPackage,
				string.Empty));
	}

	[Fact]
	public async Task Import_ReplacesMatchingFilesAndPreservesOtherFiles()
	{
		using TemporaryDirectory test = new();
		string source = test.CreateSourceTree();
		string package = test.PathFor("normal.synixbackup");
		string destination = test.PathFor("existing-synix");
		Directory.CreateDirectory(destination);
		await File.WriteAllTextAsync(
			Path.Combine(destination, "settings.json"),
			"old settings");
		await File.WriteAllTextAsync(
			Path.Combine(destination, "keep-me.txt"),
			"this file is not in the package");

		await Core.ExportUnencryptedAsync(source, package);
		await Core.ImportAsync(
			package,
			destination,
			string.Empty);

		Assert.Equal(
			await File.ReadAllTextAsync(Path.Combine(source, "settings.json")),
			await File.ReadAllTextAsync(Path.Combine(destination, "settings.json")));
		Assert.Equal(
			"this file is not in the package",
			await File.ReadAllTextAsync(Path.Combine(destination, "keep-me.txt")));
	}

	[Fact]
	public void ExportEstimate_RejectsDestinationInsideSourceFolder()
	{
		using TemporaryDirectory test = new();
		string source = test.CreateSourceTree();
		string unsafeDestination = Path.Combine(
			source,
			"recursive.synixbackup");

		Assert.Throws<InvalidOperationException>(() =>
			Core.EstimateExport(
				source,
				unsafeDestination));
	}

	[Fact]
	public async Task CancelledExport_DoesNotPublishPartialPackage()
	{
		using TemporaryDirectory test = new();
		string source = test.CreateSourceTree();
		string package = test.PathFor("cancelled.synixbackup");
		using CancellationTokenSource cancellation = new();
		cancellation.Cancel();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
			Core.ExportUnencryptedAsync(
				source,
				package,
				cancellationToken: cancellation.Token));

		Assert.False(File.Exists(package));
		Assert.Empty(Directory.GetFiles(
			test.Root,
			"*.tmp",
			SearchOption.AllDirectories));
	}

	private static void AssertDirectoryTreesEqual(
		string expectedRoot,
		string actualRoot)
	{
		string[] expectedFiles = Directory
			.GetFiles(expectedRoot, "*", SearchOption.AllDirectories)
			.Select(path => Path.GetRelativePath(expectedRoot, path))
			.OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
			.ToArray();
		string[] actualFiles = Directory
			.GetFiles(actualRoot, "*", SearchOption.AllDirectories)
			.Select(path => Path.GetRelativePath(actualRoot, path))
			.OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
			.ToArray();
		Assert.Equal(expectedFiles, actualFiles);

		foreach (string relativePath in expectedFiles)
		{
			Assert.Equal(
				File.ReadAllBytes(Path.Combine(expectedRoot, relativePath)),
				File.ReadAllBytes(Path.Combine(actualRoot, relativePath)));
		}
	}

	private sealed class TemporaryDirectory : IDisposable
	{
		public TemporaryDirectory()
		{
			Root = Path.Combine(
				Path.GetTempPath(),
				"synix-permanent-tests-" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(Root);
		}

		public string Root { get; }

		public string PathFor(string name) => Path.Combine(Root, name);

		public string CreateSourceTree()
		{
			string source = PathFor("source");
			Directory.CreateDirectory(Path.Combine(source, "nested"));
			File.WriteAllText(
				Path.Combine(source, "settings.json"),
				"{\"password\":\"permanent-test-secret\"}");
			byte[] data = new byte[1_250_000];
			new Random(20260821).NextBytes(data);
			File.WriteAllBytes(Path.Combine(source, "nested", "data.bin"), data);
			File.WriteAllBytes(Path.Combine(source, "empty.dat"), []);
			return source;
		}

		public void Dispose()
		{
			if (Directory.Exists(Root))
			{
				Directory.Delete(Root, recursive: true);
			}
		}
	}
}
