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

			await Core.Instance.ExecuteBackup(server, StartContext.Manual);
			IReadOnlyList<ServerBackupArchive> backups = Core.Instance.GetServerBackups(server);
			Assert.Single(backups);
			Assert.True(Core.Instance.HasServerBackups(server));

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
