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
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine;
using Synix_Control_Panel.SynixEngine.ModManagement;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class RestoredSettingsWorkflowTests : IDisposable
{
	private readonly string _root = Path.Combine(Path.GetTempPath(), "SynixRestoreSettings-" + Guid.NewGuid().ToString("N"));
	private readonly bool _custom = Settings.Default.UseCustomBackupPath;
	private readonly string _backupPath = Settings.Default.CustomBackupPath;
	private readonly string? _journals = Core.RestoreJournalFolderOverride;
	private readonly string? _data = ModPackageManager.DataRootOverride;
	private readonly GameServer[] _servers = ServerRegistry.Snapshot().ToArray();
	private readonly GameServer _server;
	private string Config => Path.Combine(_server.InstallPath, "ShooterGame", "Saved", "Config", "WindowsServer", "GameUserSettings.ini");
	private string World => Path.Combine(_server.InstallPath, "world.dat");
	private string[] Journals => Directory.Exists(Core.RestoreJournalFolderOverride!)
		? Directory.GetFiles(Core.RestoreJournalFolderOverride!, "*.json") : [];

	public RestoredSettingsWorkflowTests()
	{
		_server = new() { Game = "ARK: Survival Ascended", ServerName = "Fixture", InstallPath = Path.Combine(_root, "server"),
			Status = "Stopped", MaxPlayers = 10, PasswordStorageVersion = Core.CurrentStorageVersion };
		Directory.CreateDirectory(Path.GetDirectoryName(Config)!);
		File.WriteAllText(World, "saved world");
		Settings.Default.UseCustomBackupPath = true;
		Settings.Default.CustomBackupPath = Path.Combine(_root, "backups");
		Directory.CreateDirectory(Settings.Default.CustomBackupPath);
		Core.RestoreJournalFolderOverride = Path.Combine(_root, "journals");
		ModPackageManager.DataRootOverride = Path.Combine(_root, "history");
		ServerRegistry.Servers.Clear();
		ServerRegistry.Servers.Add(_server);
	}

	[Theory]
	[InlineData("ARK: Survival Ascended")]
	[InlineData("ARK: Survival Evolved")]
	public async Task SharedRestoreRefreshesSynixBeforeReturningSuccess(string game)
	{
		_server.Game = game;
		File.WriteAllText(Config, "[/Script/Engine.GameSession]\nMaxPlayers=32\n");
		Assert.True(await Core.Instance.ExecuteBackup(_server, StartContext.Manual));
		ServerBackupArchive archive = Assert.Single(Core.Instance.GetServerBackups(_server));
		File.WriteAllText(Config, "[/Script/Engine.GameSession]\nMaxPlayers=10\n");
		File.WriteAllText(World, "current world");
		int saves = 0;
		ServerBackupRestoreResult result = await Core.Instance.RestoreServerBackupAsync(_server, archive,
			persist: () => { saves++; return true; });
		Assert.True(result.Succeeded, result.Message);
		Assert.Equal(32, _server.MaxPlayers);
		Assert.Equal(1, saves);
		Assert.Equal("saved world", File.ReadAllText(World));
		Assert.Equal("Stopped", _server.Status);
		Assert.Null(_server.PID);
		Assert.Empty(Journals);
	}

	[Fact]
	public async Task SharedMinecraftRestoreUpdatesTheDisplayedPlayerLimit()
	{
		_server.Game = "Minecraft";
		_server.MinecraftEdition = "Java";
		_server.GameMode = "Survival";
		_server.WorldName = "world";
		_server.MaxPlayersFromQuery = 99;
		File.WriteAllText(Path.Combine(_server.InstallPath, "server.properties"), "max-players=26\n");
		Assert.True(await Core.Instance.ExecuteBackup(_server, StartContext.Manual));
		ServerBackupArchive archive = Assert.Single(Core.Instance.GetServerBackups(_server));
		ServerBackupRestoreResult result = await Core.Instance.RestoreServerBackupAsync(_server, archive, persist: () => true);
		Assert.True(result.Succeeded, result.Message);
		Assert.Equal(26, _server.MaxPlayers);
		Assert.Equal(0, _server.MaxPlayersFromQuery);
		Assert.Empty(Journals);
	}

	[Fact]
	public async Task FailedSettingsSaveRetainsRecoveryAcrossRestartUntilItCanBeRetried()
	{
		File.WriteAllText(Config, "[/Script/Engine.GameSession]\nMaxPlayers=32\n");
		Assert.True(await Core.Instance.ExecuteBackup(_server, StartContext.Manual));
		ServerBackupArchive archive = Assert.Single(Core.Instance.GetServerBackups(_server));
		File.WriteAllText(World, "current world");
		ServerBackupRestoreResult result = await Core.Instance.RestoreServerBackupAsync(_server, archive, persist: () => false);
		Assert.False(result.Succeeded);
		Assert.Contains("files are restored", result.Message);
		Assert.Equal(10, _server.MaxPlayers);
		Assert.Equal("saved world", File.ReadAllText(World));
		Assert.Single(Journals);
		string original = Assert.Single(Directory.GetDirectories(_root, "server.synix-restore-rollback-*"));
		Assert.Equal("current world", File.ReadAllText(Path.Combine(original, "world.dat")));
		Assert.Equal(0, Core.RecoverInterruptedServerRestores());
		Assert.Single(Journals);
		Assert.True(Directory.Exists(original));
		Assert.True(Core.SynchronizePendingServerRestores(_server, () => true));
		Assert.Equal(32, _server.MaxPlayers);
		Assert.Empty(Journals);
		Assert.False(Directory.Exists(original));
	}

	[Fact]
	public async Task InvalidRestoredSettingsBlockStartWithoutLosingRecoveryFiles()
	{
		File.WriteAllText(Config, "[/Script/Engine.GameSession]\nMaxPlayers=invalid\n");
		Assert.True(await Core.Instance.ExecuteBackup(_server, StartContext.Manual));
		ServerBackupArchive archive = Assert.Single(Core.Instance.GetServerBackups(_server));
		ServerBackupRestoreResult result = await Core.Instance.RestoreServerBackupAsync(_server, archive,
			persist: () => throw new InvalidOperationException("Invalid settings must not be persisted"));
		Assert.False(result.Succeeded);
		List<string> messages = [];
		await Servers.Start(_server, (message, _) => messages.Add(message));
		Assert.Null(_server.PID);
		Assert.Equal("Stopped", _server.Status);
		Assert.NotEmpty(messages);
		Assert.Single(Journals);
		File.WriteAllText(Config, "[/Script/Engine.GameSession]\nMaxPlayers=24\n");
		Assert.True(Core.SynchronizePendingServerRestores(_server, () => true));
		Assert.Equal(24, _server.MaxPlayers);
		Assert.Empty(Journals);
	}

	public void Dispose()
	{
		Settings.Default.UseCustomBackupPath = _custom;
		Settings.Default.CustomBackupPath = _backupPath;
		Core.RestoreJournalFolderOverride = _journals;
		ModPackageManager.DataRootOverride = _data;
		ServerRegistry.Servers.Clear();
		foreach (GameServer server in _servers) ServerRegistry.Servers.Add(server);
		ModPathSafety.EnsureTreeHasNoLinks(_root);
		Directory.Delete(_root, recursive: true);
	}
}
