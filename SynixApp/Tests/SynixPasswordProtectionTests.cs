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
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using Synix_Control_Panel.SynixEngine;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class SynixPasswordProtectionTests
{
	private const string ServerPassword = "server-secret-98!";
	private const string AdminPassword = "admin-secret-42!";
	private const string RconPassword = "rcon-secret-73!";
	private const string DiscordWebhook =
		"https://discord.com/api/webhooks/123456789/test-webhook-token";
	private const string BackupWebhook =
		"https://discord.com/api/webhooks/987654321/backup-webhook-token";

	[Fact]
	[Trait("Category", "Regression")]
	public void LegacyServerDataRunsEveryMigrationInOrder()
	{
		string json =
			"""
			[
			  {
			    "Game": " Minecraft Java ",
			    "QueryPort": 0,
			    "GameMode": "PVP",
			    "MinecraftEdition": "bedrock",
			    "MinecraftLoader": "forge",
			    "RestartDays": [true, false],
			    "MaintenanceMaximumDelayMinutes": -10
			  }
			]
			""";

		List<GameServer> migrated = Core.DeserializeServersAndMigrate(
			json,
			out ServerDataMigrationSummary summary);

		GameServer server = Assert.Single(migrated);
		Assert.Equal(0, summary.SourceVersion);
		Assert.Equal(ServerDataMigrator.CurrentVersion, summary.TargetVersion);
		Assert.Equal(1, summary.MigratedServerCount);
		Assert.Equal(ServerDataMigrator.CurrentVersion, server.DataSchemaVersion);
		Assert.Equal("Minecraft", server.Game);
		Assert.Equal(25565, server.QueryPort);
		Assert.Equal("Survival", server.GameMode);
		Assert.Equal("Bedrock", server.MinecraftEdition);
		Assert.Equal("Forge", server.MinecraftLoader);
		Assert.Equal(7, server.RestartDays.Length);
		Assert.True(server.RestartDays[0]);
		Assert.False(server.RestartDays[1]);
		Assert.Equal(0, server.MaintenanceMaximumDelayMinutes);
	}

	[Fact]
	public void CurrentServerDataMigrationIsIdempotent()
	{
		GameServer server = new()
		{
			DataSchemaVersion = ServerDataMigrator.CurrentVersion,
			Game = "Rust",
			QueryPort = 28015
		};

		Assert.False(ServerDataMigrator.Migrate(server));
		Assert.Equal(ServerDataMigrator.CurrentVersion, server.DataSchemaVersion);
		Assert.Equal(28015, server.QueryPort);
	}

	[Fact]
	[Trait("Category", "Regression")]
	public void LegacyMinecraftBedrockAliasPreservesItsEdition()
	{
		GameServer server = new()
		{
			Game = "Minecraft Bedrock",
			MinecraftEdition = "Java"
		};

		Assert.True(ServerDataMigrator.Migrate(server));
		Assert.Equal("Minecraft", server.Game);
		Assert.Equal("Bedrock", server.MinecraftEdition);
	}

	[Fact]
	public void FutureServerDataSchemaIsRejectedWithoutModification()
	{
		GameServer server = new()
		{
			DataSchemaVersion = ServerDataMigrator.CurrentVersion + 1,
			Game = "Rust"
		};

		InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
			ServerDataMigrator.Migrate(server));

		Assert.Contains("newer Synix version", exception.Message);
		Assert.Equal(ServerDataMigrator.CurrentVersion + 1, server.DataSchemaVersion);
	}

	[Fact]
	[Trait("Category", "Regression")]
	public void MigrationBackupPreservesTheOriginalFileOnlyOnce()
	{
		string root = Path.Combine(
			Path.GetTempPath(),
			$"SynixMigrationBackupTests-{Guid.NewGuid():N}");
		Directory.CreateDirectory(root);
		try
		{
			string sourcePath = Path.Combine(root, "servers.json");
			File.WriteAllText(sourcePath, "original server data");

			string backupPath = FileHandler.CreateServerDataMigrationBackup(
				sourcePath,
				ServerDataMigrator.CurrentVersion);
			File.WriteAllText(sourcePath, "updated server data");
			FileHandler.CreateServerDataMigrationBackup(
				sourcePath,
				ServerDataMigrator.CurrentVersion);

			Assert.Equal("original server data", File.ReadAllText(backupPath));
		}
		finally
		{
			if (Directory.Exists(root))
				Directory.Delete(root, true);
		}
	}

	[Fact]
	public void SetAndRevealPasswords_RoundTripsAllManagedValues()
	{
		GameServer server = new();

		Core.SetServerPasswords(
			server,
			PlaintextPasswords());

		Assert.Equal(
			Core.CurrentStorageVersion,
			server.PasswordStorageVersion);
		Assert.True(Core.IsProtected(server.Password));
		Assert.True(Core.IsProtected(server.AdminPassword));
		Assert.True(Core.IsProtected(server.RconPassword));
		Assert.DoesNotContain(ServerPassword, server.Password);
		Assert.Equal(
			PlaintextPasswords(),
			Core.RevealServerPasswords(server));
	}

	[Fact]
	public void LegacyJson_IsDetectedAndMigratedWithoutLosingPasswords()
	{
		GameServer legacyServer = new()
		{
			Game = "Palworld",
			ServerName = "Legacy Server",
			Password = ServerPassword,
			AdminPassword = AdminPassword,
			RconPassword = RconPassword,
			DiscordWebhook = DiscordWebhook
		};
		string legacyJson = JsonSerializer.Serialize(new[] { legacyServer });

		List<GameServer> migrated = Core
			.DeserializeServersAndMigrate(
				legacyJson,
				out int migratedServerCount);

		GameServer result = Assert.Single(migrated);
		Assert.Equal(1, migratedServerCount);
		Assert.Equal(
			PlaintextSecrets(),
			Core.RevealServerSecrets(result));
		Assert.True(Core.IsProtected(result.Password));
		Assert.True(Core.IsProtected(result.AdminPassword));
		Assert.True(Core.IsProtected(result.RconPassword));
		Assert.True(Core.IsProtected(result.DiscordWebhook));
	}

	[Fact]
	public void StorageJson_NeverContainsReadableManagedPasswords()
	{
		GameServer server = new()
		{
			Game = "Rust",
			ServerName = "Protected Server",
			Password = ServerPassword,
			AdminPassword = AdminPassword,
			RconPassword = RconPassword,
			DiscordWebhook = DiscordWebhook
		};

		string storageJson = Core
			.SerializeServersForStorage(new[] { server });

		Assert.DoesNotContain(ServerPassword, storageJson);
		Assert.DoesNotContain(AdminPassword, storageJson);
		Assert.DoesNotContain(RconPassword, storageJson);
		Assert.DoesNotContain(DiscordWebhook, storageJson);
		Assert.DoesNotContain("test-webhook-token", storageJson);
		Assert.Contains(
			Core.ProtectedValuePrefix,
			storageJson);
		Assert.Contains("\"PasswordStorageVersion\": 3", storageJson);
	}

	[Fact]
	public void VersionOneStorage_ProtectsWebhookWithoutLosingPasswords()
	{
		GameServer versionOneServer = new()
		{
			Game = "Palworld",
			ServerName = "Version One Server",
			PasswordStorageVersion = 1,
			Password = Core.Protect(ServerPassword),
			AdminPassword = Core.Protect(AdminPassword),
			RconPassword = Core.Protect(RconPassword),
			DiscordWebhook = DiscordWebhook
		};

		bool migrated = Core.MigrateLegacyServer(
			versionOneServer);

		Assert.True(migrated);
		Assert.Equal(3, versionOneServer.PasswordStorageVersion);
		Assert.True(Core.IsProtected(
			versionOneServer.DiscordWebhook));
		Assert.Equal(
			PlaintextSecrets(),
			Core.RevealServerSecrets(versionOneServer));
	}

	[Fact]
	public void VersionTwoStorage_MigratesEncryptedMasterWebhookToRouteStorage()
	{
		GameServer versionTwoServer = new()
		{
			PasswordStorageVersion = 2,
			Password = Core.Protect(ServerPassword),
			AdminPassword = Core.Protect(AdminPassword),
			RconPassword = Core.Protect(RconPassword),
			DiscordWebhook = Core.Protect(DiscordWebhook)
		};

		Assert.True(Core.MigrateLegacyServer(versionTwoServer));
		Assert.Equal(Core.CurrentStorageVersion, versionTwoServer.PasswordStorageVersion);
		Assert.Equal(DiscordWebhook, Core.RevealDiscordWebhook(versionTwoServer));
		Assert.Empty(Core.RevealDiscordWebhookRoutes(versionTwoServer));
	}

	[Fact]
	public void AlreadyProtectedServer_IsNotEncryptedTwice()
	{
		GameServer server = new();
		Core.SetServerPasswords(
			server,
			PlaintextPasswords());
		string firstProtectedValue = server.Password;

		bool migrated = Core.MigrateLegacyServer(server);

		Assert.False(migrated);
		Assert.Equal(firstProtectedValue, server.Password);
		Assert.Equal(
			ServerPassword,
			Core.Reveal(server.Password));
	}

	[Fact]
	public void LegacyPasswordThatLooksLikeAStoragePrefix_IsStillProtectedAsText()
	{
		string prefixLikePassword =
			Core.ProtectedValuePrefix + "my-real-password";
		GameServer legacyServer = new()
		{
			PasswordStorageVersion = 0,
			Password = prefixLikePassword
		};

		bool migrated = Core.MigrateLegacyServer(legacyServer);

		Assert.True(migrated);
		Assert.NotEqual(prefixLikePassword, legacyServer.Password);
		Assert.Equal(
			prefixLikePassword,
			Core.Reveal(legacyServer.Password));
	}

	[Fact]
	public void EditingPasswords_ReplacesTheProtectedValues()
	{
		GameServer server = new();
		Core.SetServerPasswords(
			server,
			PlaintextPasswords());
		string oldStoredPassword = server.Password;
		SynixServerPasswords edited = new(
			"new-server-password!",
			"new-admin-password!",
			"new-rcon-password!");

		Core.SetServerPasswords(server, edited);

		Assert.NotEqual(oldStoredPassword, server.Password);
		Assert.Equal(
			edited,
			Core.RevealServerPasswords(server));
	}

	[Fact]
	public void DamagedProtectedValue_IsRejectedWithoutReturningText()
	{
		string damaged =
			Core.ProtectedValuePrefix + "not-valid-base64***";

		Assert.Throws<SynixPasswordProtectionException>(
			() => Core.Reveal(damaged));
	}

	[Fact]
	public void AtomicWriter_ReplacesCompleteFileAndLeavesNoTempFile()
	{
		string directory = Path.Combine(
			Path.GetTempPath(),
			$"SynixPasswordTests-{Guid.NewGuid():N}");
		string destination = Path.Combine(directory, "servers.json");

		try
		{
			Directory.CreateDirectory(directory);
			File.WriteAllText(destination, "old contents");

			FileHandler.WriteTextAtomically(destination, "new complete contents");

			Assert.Equal("new complete contents", File.ReadAllText(destination));
			Assert.Empty(Directory.EnumerateFiles(directory, "*.tmp"));
		}
		finally
		{
			if (Directory.Exists(directory))
				Directory.Delete(directory, recursive: true);
		}
	}

	[Fact]
	public void EncryptedTransferVault_RestoresPasswordsForTheImportingUser()
	{
		using TransferFolders folders = new();
		const string transferPassword = "portable-transfer-password-91!";
		GameServer exportedServer = CreateProtectedServer();
		string oldProtectedPassword = exportedServer.Password;

		Core.PrepareEncryptedExport(
			folders.SourceRoot,
			transferPassword,
			new[] { exportedServer });

		string sourceVault = Core
			.GetVaultPath(folders.SourceRoot);
		byte[] vaultBytes = File.ReadAllBytes(sourceVault);
		string vaultAsText = Encoding.UTF8.GetString(vaultBytes);
		Assert.DoesNotContain(ServerPassword, vaultAsText);
		Assert.DoesNotContain(AdminPassword, vaultAsText);
		Assert.DoesNotContain(RconPassword, vaultAsText);
		Assert.DoesNotContain(DiscordWebhook, vaultAsText);
		Assert.DoesNotContain("test-webhook-token", vaultAsText);
		Assert.DoesNotContain(BackupWebhook, vaultAsText);
		Assert.DoesNotContain("backup-webhook-token", vaultAsText);

		folders.PrepareImportedFiles(exportedServer, sourceVault);
		bool restored = Core.RestoreEncryptedImport(
			folders.ImportRoot,
			transferPassword);

		Assert.True(restored);
		Assert.False(File.Exists(
			Core.GetVaultPath(folders.ImportRoot)));
		GameServer importedServer = Assert.Single(
			JsonSerializer.Deserialize<List<GameServer>>(
				File.ReadAllText(folders.ImportedServersPath))!);
		Assert.NotEqual(oldProtectedPassword, importedServer.Password);
		Assert.Equal(
			PlaintextSecrets(),
			Core.RevealServerSecrets(importedServer));
		DiscordWebhookRoute importedRoute = Assert.Single(
			Core.RevealDiscordWebhookRoutes(importedServer));
		Assert.Equal("Backups", importedRoute.Name);
		Assert.Equal(BackupWebhook, importedRoute.WebhookUrl);
		Assert.Equal(
			DiscordNotificationEvent.BackupStarted |
			DiscordNotificationEvent.BackupCompleted |
			DiscordNotificationEvent.BackupFailed,
			importedRoute.Events);
	}

	[Fact]
	public void WrongTransferPassword_LeavesImportedServerListUntouched()
	{
		using TransferFolders folders = new();
		GameServer exportedServer = CreateProtectedServer();
		Core.PrepareEncryptedExport(
			folders.SourceRoot,
			"correct-transfer-password-11!",
			new[] { exportedServer });
		folders.PrepareImportedFiles(
			exportedServer,
			Core.GetVaultPath(folders.SourceRoot));
		string originalServersJson = File.ReadAllText(
			folders.ImportedServersPath);

		Assert.Throws<SynixPasswordProtectionException>(() =>
			Core.RestoreEncryptedImport(
				folders.ImportRoot,
				"wrong-transfer-password-22!"));

		Assert.Equal(
			originalServersJson,
			File.ReadAllText(folders.ImportedServersPath));
		Assert.True(File.Exists(
			Core.GetVaultPath(folders.ImportRoot)));
	}

	[Fact]
	public async Task EncryptedSynixPackage_CarriesPasswordsAcrossTheFullTransfer()
	{
		using TransferFolders folders = new();
		const string transferPassword = "full-package-transfer-password-77!";
		GameServer exportedServer = CreateProtectedServer();
		string sourceServersPath = Path.Combine(
			folders.SourceRoot,
			"SynixData",
			"servers.json");
		Directory.CreateDirectory(Path.GetDirectoryName(sourceServersPath)!);
		File.WriteAllText(
			sourceServersPath,
			Core.SerializeServersForStorage(
				new[] { exportedServer }));
		Core.PrepareEncryptedExport(
			folders.SourceRoot,
			transferPassword,
			new[] { exportedServer });

		await Core.ExportAsync(
			folders.SourceRoot,
			folders.PackagePath,
			transferPassword);
		await Core.ImportAsync(
			folders.PackagePath,
			folders.ImportRoot,
			transferPassword);

		Assert.True(Core.RestoreEncryptedImport(
			folders.ImportRoot,
			transferPassword));
		GameServer importedServer = Assert.Single(
			JsonSerializer.Deserialize<List<GameServer>>(
				File.ReadAllText(folders.ImportedServersPath))!);
		Assert.Equal(
			PlaintextSecrets(),
			Core.RevealServerSecrets(importedServer));
	}

	private static SynixServerPasswords PlaintextPasswords()
	{
		return new SynixServerPasswords(
			ServerPassword,
			AdminPassword,
			RconPassword);
	}

	private static SynixServerSecrets PlaintextSecrets()
	{
		return new SynixServerSecrets(
			PlaintextPasswords(),
			DiscordWebhook);
	}

	private static GameServer CreateProtectedServer()
	{
		GameServer server = new()
		{
			Game = "Palworld",
			ServerName = "Portable Server",
			InstallPath = @"C:\Synix\Games\Portable Server"
		};
		Core.SetServerSecrets(
			server,
			PlaintextSecrets());
		Core.SetDiscordWebhookRoutes(
			server,
			[
				new DiscordWebhookRoute
				{
					Name = "Backups",
					WebhookUrl = BackupWebhook,
					Events = DiscordNotificationEvent.BackupStarted |
						DiscordNotificationEvent.BackupCompleted |
						DiscordNotificationEvent.BackupFailed
				}
			]);
		return server;
	}

	private sealed class TransferFolders : IDisposable
	{
		private readonly string _root = Path.Combine(
			Path.GetTempPath(),
			$"SynixPortablePasswordTests-{Guid.NewGuid():N}");

		public string SourceRoot => Path.Combine(_root, "source", "Synix");
		public string ImportRoot => Path.Combine(_root, "import", "Synix");
		public string PackagePath => Path.Combine(_root, "transfer.synixbackup");
		public string ImportedServersPath => Path.Combine(
			ImportRoot,
			"SynixData",
			"servers.json");

		public void PrepareImportedFiles(
			GameServer server,
			string sourceVault)
		{
			string dataDirectory = Path.Combine(ImportRoot, "SynixData");
			Directory.CreateDirectory(dataDirectory);
			File.WriteAllText(
				ImportedServersPath,
				JsonSerializer.Serialize(new[] { server }));
			File.Copy(
				sourceVault,
				Core.GetVaultPath(ImportRoot),
				overwrite: true);
		}

		public void Dispose()
		{
			if (Directory.Exists(_root))
				Directory.Delete(_root, recursive: true);
		}
	}
}
