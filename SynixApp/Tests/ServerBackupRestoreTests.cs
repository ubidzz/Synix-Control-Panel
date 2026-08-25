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
using Synix_Control_Panel.Properties;
using Synix_Control_Panel.SynixEngine;
using System.IO.Compression;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class ServerBackupRestoreTests
{
	[Fact]
	public async Task BackupDiscoveryAndRestore_ReplacesTheServerFolder()
	{
		string testRoot = CreateTestRoot();
		string installPath = Path.Combine(testRoot, "server");
		string backupPath = Path.Combine(testRoot, "backups");
		bool originalCustomSetting = Settings.Default.UseCustomBackupPath;
		string originalCustomPath = Settings.Default.CustomBackupPath;
		int originalMaximum = Settings.Default.MaxBackups;
		string? originalJournalOverride = Core.RestoreJournalFolderOverride;

		try
		{
			Directory.CreateDirectory(installPath);
			Directory.CreateDirectory(backupPath);
			File.WriteAllText(Path.Combine(installPath, "world.dat"), "original world");
			Settings.Default.UseCustomBackupPath = true;
			Settings.Default.CustomBackupPath = backupPath;
			Settings.Default.MaxBackups = 3;
			Core.RestoreJournalFolderOverride = Path.Combine(testRoot, "restore-journals");
			GameServer server = CreateServer(installPath);
			string serverBackupPath = Core.Instance.GetActiveServerBackupFolder(server);
			Directory.CreateDirectory(serverBackupPath);
			File.WriteAllText(Path.Combine(serverBackupPath, "backup_interrupted.zip.partial"), "partial");
			File.WriteAllText(Path.Combine(serverBackupPath, "backup_orphan.zip.sha256"), "orphan");

			await Core.Instance.ExecuteBackup(server, StartContext.Manual);
			IReadOnlyList<ServerBackupArchive> backups = Core.Instance.GetServerBackups(server);
			Assert.Single(backups);
			Assert.True(Core.Instance.HasServerBackups(server));
			Assert.Equal(ServerBackupIntegrity.Recorded, backups[0].Integrity);
			Assert.True(File.Exists(backups[0].ArchivePath + ".sha256"));
			Assert.Empty(Directory.GetFiles(backupPath, "*.partial", SearchOption.AllDirectories));
			Assert.False(File.Exists(Path.Combine(serverBackupPath, "backup_orphan.zip.sha256")));

			File.WriteAllText(Path.Combine(installPath, "world.dat"), "changed world");
			File.WriteAllText(Path.Combine(installPath, "new-file.txt"), "remove me");
			ServerBackupRestoreResult result = await Core.Instance.RestoreServerBackupAsync(
				server,
				backups[0]);

			Assert.True(result.Succeeded, result.Message);
			Assert.Equal("original world", File.ReadAllText(Path.Combine(installPath, "world.dat")));
			Assert.False(File.Exists(Path.Combine(installPath, "new-file.txt")));
			Assert.Equal(Core.StatusManager.GetStatus(Core.ServerState.Stopped), server.Status);
		}
		finally
		{
			Settings.Default.UseCustomBackupPath = originalCustomSetting;
			Settings.Default.CustomBackupPath = originalCustomPath;
			Settings.Default.MaxBackups = originalMaximum;
			Core.RestoreJournalFolderOverride = originalJournalOverride;
			TryDeleteDirectory(testRoot);
		}
	}

	[Fact]
	public async Task ChangedProtectedBackup_IsRejectedBeforeServerFilesAreTouched()
	{
		string testRoot = CreateTestRoot();
		string installPath = Path.Combine(testRoot, "server");
		string backupPath = Path.Combine(testRoot, "backups");
		bool originalCustomSetting = Settings.Default.UseCustomBackupPath;
		string originalCustomPath = Settings.Default.CustomBackupPath;
		string? originalJournalOverride = Core.RestoreJournalFolderOverride;

		try
		{
			Directory.CreateDirectory(installPath);
			Directory.CreateDirectory(backupPath);
			File.WriteAllText(Path.Combine(installPath, "world.dat"), "protected world");
			Settings.Default.UseCustomBackupPath = true;
			Settings.Default.CustomBackupPath = backupPath;
			Core.RestoreJournalFolderOverride = Path.Combine(testRoot, "restore-journals");
			GameServer server = CreateServer(installPath);

			await Core.Instance.ExecuteBackup(server, StartContext.Manual);
			ServerBackupArchive backup = Assert.Single(Core.Instance.GetServerBackups(server));
			File.WriteAllText(Path.Combine(installPath, "world.dat"), "current world");
			using (FileStream stream = new(backup.ArchivePath, FileMode.Append, FileAccess.Write, FileShare.None))
				stream.WriteByte(0x5A);

			ServerBackupRestoreResult result = await Core.Instance.RestoreServerBackupAsync(
				server,
				backup);

			Assert.False(result.Succeeded);
			Assert.Contains("SHA-256 integrity check", result.Message, StringComparison.OrdinalIgnoreCase);
			Assert.Equal("current world", File.ReadAllText(Path.Combine(installPath, "world.dat")));
		}
		finally
		{
			Settings.Default.UseCustomBackupPath = originalCustomSetting;
			Settings.Default.CustomBackupPath = originalCustomPath;
			Core.RestoreJournalFolderOverride = originalJournalOverride;
			TryDeleteDirectory(testRoot);
		}
	}

	[Fact]
	public async Task LegacyBackupWithoutReceipt_RemainsRestorable()
	{
		string testRoot = CreateTestRoot();
		string installPath = Path.Combine(testRoot, "server");
		string backupPath = Path.Combine(testRoot, "backups");
		bool originalCustomSetting = Settings.Default.UseCustomBackupPath;
		string originalCustomPath = Settings.Default.CustomBackupPath;
		string? originalJournalOverride = Core.RestoreJournalFolderOverride;

		try
		{
			Directory.CreateDirectory(installPath);
			Directory.CreateDirectory(backupPath);
			File.WriteAllText(Path.Combine(installPath, "world.dat"), "legacy world");
			Settings.Default.UseCustomBackupPath = true;
			Settings.Default.CustomBackupPath = backupPath;
			Core.RestoreJournalFolderOverride = Path.Combine(testRoot, "restore-journals");
			GameServer server = CreateServer(installPath);
			string serverBackupPath = Core.Instance.GetActiveServerBackupFolder(server);
			Directory.CreateDirectory(serverBackupPath);
			string archivePath = Path.Combine(serverBackupPath, "backup_legacy.zip");
			ZipFile.CreateFromDirectory(
				installPath,
				archivePath,
				CompressionLevel.Optimal,
				includeBaseDirectory: true);

			ServerBackupArchive backup = Assert.Single(Core.Instance.GetServerBackups(server));
			Assert.Equal(ServerBackupIntegrity.Legacy, backup.Integrity);
			File.WriteAllText(Path.Combine(installPath, "world.dat"), "current world");
			ServerBackupRestoreResult result = await Core.Instance.RestoreServerBackupAsync(
				server,
				backup);

			Assert.True(result.Succeeded, result.Message);
			Assert.Equal("legacy world", File.ReadAllText(Path.Combine(installPath, "world.dat")));
		}
		finally
		{
			Settings.Default.UseCustomBackupPath = originalCustomSetting;
			Settings.Default.CustomBackupPath = originalCustomPath;
			Core.RestoreJournalFolderOverride = originalJournalOverride;
			TryDeleteDirectory(testRoot);
		}
	}

	[Fact]
	public async Task InvalidIntegrityReceipt_IsRejectedBeforeExtraction()
	{
		string testRoot = CreateTestRoot();
		string installPath = Path.Combine(testRoot, "server");
		string backupPath = Path.Combine(testRoot, "backups");
		bool originalCustomSetting = Settings.Default.UseCustomBackupPath;
		string originalCustomPath = Settings.Default.CustomBackupPath;
		string? originalJournalOverride = Core.RestoreJournalFolderOverride;

		try
		{
			Directory.CreateDirectory(installPath);
			Directory.CreateDirectory(backupPath);
			File.WriteAllText(Path.Combine(installPath, "world.dat"), "current world");
			Settings.Default.UseCustomBackupPath = true;
			Settings.Default.CustomBackupPath = backupPath;
			Core.RestoreJournalFolderOverride = Path.Combine(testRoot, "restore-journals");
			GameServer server = CreateServer(installPath);
			string serverBackupPath = Core.Instance.GetActiveServerBackupFolder(server);
			Directory.CreateDirectory(serverBackupPath);
			string archivePath = Path.Combine(serverBackupPath, "backup_invalid.zip");
			ZipFile.CreateFromDirectory(
				installPath,
				archivePath,
				CompressionLevel.Optimal,
				includeBaseDirectory: true);
			File.WriteAllText(archivePath + ".sha256", "not a valid receipt");

			ServerBackupArchive backup = Assert.Single(Core.Instance.GetServerBackups(server));
			Assert.Equal(ServerBackupIntegrity.Invalid, backup.Integrity);
			ServerBackupRestoreResult result = await Core.Instance.RestoreServerBackupAsync(
				server,
				backup);

			Assert.False(result.Succeeded);
			Assert.Contains("invalid SHA-256", result.Message, StringComparison.OrdinalIgnoreCase);
			Assert.Equal("current world", File.ReadAllText(Path.Combine(installPath, "world.dat")));
		}
		finally
		{
			Settings.Default.UseCustomBackupPath = originalCustomSetting;
			Settings.Default.CustomBackupPath = originalCustomPath;
			Core.RestoreJournalFolderOverride = originalJournalOverride;
			TryDeleteDirectory(testRoot);
		}
	}

	[Fact]
	public async Task UnsafeArchive_IsRejectedWithoutChangingTheServerFolder()
	{
		string testRoot = CreateTestRoot();
		string installPath = Path.Combine(testRoot, "server");
		string backupPath = Path.Combine(testRoot, "backups");
		bool originalCustomSetting = Settings.Default.UseCustomBackupPath;
		string originalCustomPath = Settings.Default.CustomBackupPath;
		string? originalJournalOverride = Core.RestoreJournalFolderOverride;

		try
		{
			Directory.CreateDirectory(installPath);
			Directory.CreateDirectory(backupPath);
			File.WriteAllText(Path.Combine(installPath, "world.dat"), "current world");
			Settings.Default.UseCustomBackupPath = true;
			Settings.Default.CustomBackupPath = backupPath;
			Core.RestoreJournalFolderOverride = Path.Combine(testRoot, "restore-journals");
			GameServer server = CreateServer(installPath);
			string serverBackupPath = Core.Instance.GetActiveServerBackupFolder(server);
			Directory.CreateDirectory(serverBackupPath);
			string archivePath = Path.Combine(serverBackupPath, "backup_unsafe.zip");
			using (ZipArchive archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
			{
				ZipArchiveEntry unsafeEntry = archive.CreateEntry("../outside.txt");
				using StreamWriter writer = new(unsafeEntry.Open());
				writer.Write("unsafe");
			}

			ServerBackupArchive backup = Assert.Single(Core.Instance.GetServerBackups(server));
			ServerBackupRestoreResult result = await Core.Instance.RestoreServerBackupAsync(
				server,
				backup);

			Assert.False(result.Succeeded);
			Assert.Equal("current world", File.ReadAllText(Path.Combine(installPath, "world.dat")));
			Assert.False(File.Exists(Path.Combine(testRoot, "outside.txt")));
		}
		finally
		{
			Settings.Default.UseCustomBackupPath = originalCustomSetting;
			Settings.Default.CustomBackupPath = originalCustomPath;
			Core.RestoreJournalFolderOverride = originalJournalOverride;
			TryDeleteDirectory(testRoot);
		}
	}

	private static GameServer CreateServer(string installPath) => new()
	{
		Game = "Restore Test Game",
		ServerName = "Restore Test Server",
		InstallPath = installPath,
		Status = Core.StatusManager.GetStatus(Core.ServerState.Stopped)
	};

	private static string CreateTestRoot()
	{
		string path = Path.Combine(
			Path.GetTempPath(),
			"SynixServerRestoreTests",
			Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(path);
		return path;
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
}
