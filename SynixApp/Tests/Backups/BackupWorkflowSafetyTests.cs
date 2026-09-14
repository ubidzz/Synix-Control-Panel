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
using Synix_Control_Panel.SynixEngine.ModManagement;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class BackupWorkflowSafetyTests : IDisposable
{
	private readonly string _root = Path.Combine(Path.GetTempPath(), "SynixBackupWorkflow-" + Guid.NewGuid().ToString("N"));
	private readonly bool _custom = Settings.Default.UseCustomBackupPath;
	private readonly string _backupPath = Settings.Default.CustomBackupPath;
	private readonly int _maximum = Settings.Default.MaxBackups;
	private readonly GameServer _server;
	private string World => Path.Combine(_server.InstallPath, "world.dat");

	public BackupWorkflowSafetyTests()
	{
		_server = new() { Game = "Backup workflow fixture", ServerName = "Fixture", Status = "Stopped",
			InstallPath = Path.Combine(_root, "server") };
		Directory.CreateDirectory(_server.InstallPath);
		File.WriteAllText(World, "original world");
		Settings.Default.UseCustomBackupPath = true;
		Settings.Default.CustomBackupPath = Path.Combine(_root, "backups");
		Directory.CreateDirectory(Settings.Default.CustomBackupPath);
		Settings.Default.MaxBackups = 1;
	}

	[Fact]
	public async Task LockedOldBackupCannotDeleteTheNewlyCompletedBackup()
	{
		Assert.True(await Core.Instance.ExecuteBackup(_server, StartContext.Manual));
		ServerBackupArchive old = Assert.Single(Core.Instance.GetServerBackups(_server));
		File.SetLastWriteTimeUtc(old.ArchivePath, DateTime.UtcNow.AddDays(-1));
		File.WriteAllText(World, "new world");
		using FileStream locked = new(old.ArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read);
		Assert.True(await Core.Instance.ExecuteBackup(_server, StartContext.Manual));
		IReadOnlyList<ServerBackupArchive> backups = Core.Instance.GetServerBackups(_server);
		Assert.Equal(2, backups.Count);
		ServerBackupArchive latest = Assert.Single(backups, item => item.ArchivePath != old.ArchivePath);
		Assert.True(File.Exists(latest.ArchivePath + ".sha256"));
		Assert.True((await Core.Instance.VerifyServerBackupAsync(_server, latest)).Succeeded);
		Assert.True(File.Exists(old.ArchivePath));
		Assert.Equal("Stopped", _server.Status);
	}

	[Fact]
	public async Task FutureDatedOldArchiveCannotCauseRetentionToDeleteTheNewBackup()
	{
		Assert.True(await Core.Instance.ExecuteBackup(_server, StartContext.Manual));
		ServerBackupArchive old = Assert.Single(Core.Instance.GetServerBackups(_server));
		File.SetLastWriteTimeUtc(old.ArchivePath, DateTime.UtcNow.AddDays(10));
		Assert.True(await Core.Instance.ExecuteBackup(_server, StartContext.Manual));
		ServerBackupArchive current = Assert.Single(Core.Instance.GetServerBackups(_server));
		Assert.NotEqual(old.ArchivePath, current.ArchivePath);
		Assert.True((await Core.Instance.VerifyServerBackupAsync(_server, current)).Succeeded);
	}

	[Fact]
	public async Task FailedCreationPreservesOlderBackupsAndCanBeRetried()
	{
		Assert.True(await Core.Instance.ExecuteBackup(_server, StartContext.Manual));
		ServerBackupArchive old = Assert.Single(Core.Instance.GetServerBackups(_server));
		using (FileStream locked = new(World, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
			Assert.False(await Core.Instance.ExecuteBackup(_server, StartContext.Manual));
		Assert.Equal(old.ArchivePath, Assert.Single(Core.Instance.GetServerBackups(_server)).ArchivePath);
		Assert.Empty(Directory.GetFiles(Settings.Default.CustomBackupPath, "*.partial", SearchOption.AllDirectories));
		Assert.Equal("Stopped", _server.Status);
		Assert.True(await Core.Instance.ExecuteBackup(_server, StartContext.Manual));
	}

	[Theory]
	[InlineData("Running", null)]
	[InlineData("Stopped", 12345)]
	public async Task BackupRejectsActiveProcessesWithoutPublishingAnArchive(string status, int? pid)
	{
		_server.Status = status; _server.PID = pid;
		Assert.False(await Core.Instance.ExecuteBackup(_server, StartContext.Manual));
		Assert.Empty(Core.Instance.GetServerBackups(_server));
		Assert.Equal(status, _server.Status);
	}

	[Fact]
	public async Task UnavailableCustomDestinationCannotSilentlyFallBackToTheSystemDrive()
	{
		Settings.Default.CustomBackupPath = Path.Combine(_root, "offline");
		Assert.StartsWith(Settings.Default.CustomBackupPath, Core.Instance.GetActiveServerBackupFolder(_server));
		ServerBackupPreflight preflight = await Core.Instance.CreateServerBackupPreflightAsync(_server);
		Assert.False(preflight.Succeeded);
		Assert.Contains("custom backup folder", preflight.Message);
		Assert.False(await Core.Instance.ExecuteBackup(_server, StartContext.Manual));
		Assert.False(Directory.Exists(Settings.Default.CustomBackupPath));
	}

	[Fact]
	public async Task MissingFolderReturnsFailureInsteadOfImplyingSuccessfulBackup()
	{
		_server.InstallPath = Path.Combine(_root, "missing");
		Assert.False(await Core.Instance.ExecuteBackup(_server, StartContext.Manual));
		Assert.Equal("Stopped", _server.Status);
		Assert.False(Directory.Exists(_server.InstallPath));
	}

	public void Dispose()
	{
		Settings.Default.UseCustomBackupPath = _custom;
		Settings.Default.CustomBackupPath = _backupPath;
		Settings.Default.MaxBackups = _maximum;
		ModPathSafety.EnsureTreeHasNoLinks(_root);
		Directory.Delete(_root, recursive: true);
	}
}
