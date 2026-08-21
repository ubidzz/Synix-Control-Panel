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

	[Fact]
	public void SetAndRevealPasswords_RoundTripsAllManagedValues()
	{
		GameServer server = new();

		SynixPasswordProtection.SetServerPasswords(
			server,
			PlaintextPasswords());

		Assert.Equal(
			SynixPasswordProtection.CurrentStorageVersion,
			server.PasswordStorageVersion);
		Assert.True(SynixPasswordProtection.IsProtected(server.Password));
		Assert.True(SynixPasswordProtection.IsProtected(server.AdminPassword));
		Assert.True(SynixPasswordProtection.IsProtected(server.RconPassword));
		Assert.DoesNotContain(ServerPassword, server.Password);
		Assert.Equal(
			PlaintextPasswords(),
			SynixPasswordProtection.RevealServerPasswords(server));
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
			RconPassword = RconPassword
		};
		string legacyJson = JsonSerializer.Serialize(new[] { legacyServer });

		List<GameServer> migrated = SynixPasswordProtection
			.DeserializeServersAndMigrate(
				legacyJson,
				out int migratedServerCount);

		GameServer result = Assert.Single(migrated);
		Assert.Equal(1, migratedServerCount);
		Assert.Equal(
			PlaintextPasswords(),
			SynixPasswordProtection.RevealServerPasswords(result));
		Assert.True(SynixPasswordProtection.IsProtected(result.Password));
		Assert.True(SynixPasswordProtection.IsProtected(result.AdminPassword));
		Assert.True(SynixPasswordProtection.IsProtected(result.RconPassword));
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
			RconPassword = RconPassword
		};

		string storageJson = SynixPasswordProtection
			.SerializeServersForStorage(new[] { server });

		Assert.DoesNotContain(ServerPassword, storageJson);
		Assert.DoesNotContain(AdminPassword, storageJson);
		Assert.DoesNotContain(RconPassword, storageJson);
		Assert.Contains(
			SynixPasswordProtection.ProtectedValuePrefix,
			storageJson);
		Assert.Contains("\"PasswordStorageVersion\": 1", storageJson);
	}

	[Fact]
	public void AlreadyProtectedServer_IsNotEncryptedTwice()
	{
		GameServer server = new();
		SynixPasswordProtection.SetServerPasswords(
			server,
			PlaintextPasswords());
		string firstProtectedValue = server.Password;

		bool migrated = SynixPasswordProtection.MigrateLegacyServer(server);

		Assert.False(migrated);
		Assert.Equal(firstProtectedValue, server.Password);
		Assert.Equal(
			ServerPassword,
			SynixPasswordProtection.Reveal(server.Password));
	}

	[Fact]
	public void LegacyPasswordThatLooksLikeAStoragePrefix_IsStillProtectedAsText()
	{
		string prefixLikePassword =
			SynixPasswordProtection.ProtectedValuePrefix + "my-real-password";
		GameServer legacyServer = new()
		{
			PasswordStorageVersion = 0,
			Password = prefixLikePassword
		};

		bool migrated = SynixPasswordProtection.MigrateLegacyServer(legacyServer);

		Assert.True(migrated);
		Assert.NotEqual(prefixLikePassword, legacyServer.Password);
		Assert.Equal(
			prefixLikePassword,
			SynixPasswordProtection.Reveal(legacyServer.Password));
	}

	[Fact]
	public void EditingPasswords_ReplacesTheProtectedValues()
	{
		GameServer server = new();
		SynixPasswordProtection.SetServerPasswords(
			server,
			PlaintextPasswords());
		string oldStoredPassword = server.Password;
		SynixServerPasswords edited = new(
			"new-server-password!",
			"new-admin-password!",
			"new-rcon-password!");

		SynixPasswordProtection.SetServerPasswords(server, edited);

		Assert.NotEqual(oldStoredPassword, server.Password);
		Assert.Equal(
			edited,
			SynixPasswordProtection.RevealServerPasswords(server));
	}

	[Fact]
	public void DamagedProtectedValue_IsRejectedWithoutReturningText()
	{
		string damaged =
			SynixPasswordProtection.ProtectedValuePrefix + "not-valid-base64***";

		Assert.Throws<SynixPasswordProtectionException>(
			() => SynixPasswordProtection.Reveal(damaged));
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

		SynixPortablePasswordTransfer.PrepareEncryptedExport(
			folders.SourceRoot,
			transferPassword,
			new[] { exportedServer });

		string sourceVault = SynixPortablePasswordTransfer
			.GetVaultPath(folders.SourceRoot);
		byte[] vaultBytes = File.ReadAllBytes(sourceVault);
		string vaultAsText = Encoding.UTF8.GetString(vaultBytes);
		Assert.DoesNotContain(ServerPassword, vaultAsText);
		Assert.DoesNotContain(AdminPassword, vaultAsText);
		Assert.DoesNotContain(RconPassword, vaultAsText);

		folders.PrepareImportedFiles(exportedServer, sourceVault);
		bool restored = SynixPortablePasswordTransfer.RestoreEncryptedImport(
			folders.ImportRoot,
			transferPassword);

		Assert.True(restored);
		Assert.False(File.Exists(
			SynixPortablePasswordTransfer.GetVaultPath(folders.ImportRoot)));
		GameServer importedServer = Assert.Single(
			JsonSerializer.Deserialize<List<GameServer>>(
				File.ReadAllText(folders.ImportedServersPath))!);
		Assert.NotEqual(oldProtectedPassword, importedServer.Password);
		Assert.Equal(
			PlaintextPasswords(),
			SynixPasswordProtection.RevealServerPasswords(importedServer));
	}

	[Fact]
	public void WrongTransferPassword_LeavesImportedServerListUntouched()
	{
		using TransferFolders folders = new();
		GameServer exportedServer = CreateProtectedServer();
		SynixPortablePasswordTransfer.PrepareEncryptedExport(
			folders.SourceRoot,
			"correct-transfer-password-11!",
			new[] { exportedServer });
		folders.PrepareImportedFiles(
			exportedServer,
			SynixPortablePasswordTransfer.GetVaultPath(folders.SourceRoot));
		string originalServersJson = File.ReadAllText(
			folders.ImportedServersPath);

		Assert.Throws<SynixPasswordProtectionException>(() =>
			SynixPortablePasswordTransfer.RestoreEncryptedImport(
				folders.ImportRoot,
				"wrong-transfer-password-22!"));

		Assert.Equal(
			originalServersJson,
			File.ReadAllText(folders.ImportedServersPath));
		Assert.True(File.Exists(
			SynixPortablePasswordTransfer.GetVaultPath(folders.ImportRoot)));
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
			SynixPasswordProtection.SerializeServersForStorage(
				new[] { exportedServer }));
		SynixPortablePasswordTransfer.PrepareEncryptedExport(
			folders.SourceRoot,
			transferPassword,
			new[] { exportedServer });

		await SynixTransferPackage.ExportAsync(
			folders.SourceRoot,
			folders.PackagePath,
			transferPassword);
		await SynixTransferPackage.ImportAsync(
			folders.PackagePath,
			folders.ImportRoot,
			transferPassword);

		Assert.True(SynixPortablePasswordTransfer.RestoreEncryptedImport(
			folders.ImportRoot,
			transferPassword));
		GameServer importedServer = Assert.Single(
			JsonSerializer.Deserialize<List<GameServer>>(
				File.ReadAllText(folders.ImportedServersPath))!);
		Assert.Equal(
			PlaintextPasswords(),
			SynixPasswordProtection.RevealServerPasswords(importedServer));
	}

	private static SynixServerPasswords PlaintextPasswords()
	{
		return new SynixServerPasswords(
			ServerPassword,
			AdminPassword,
			RconPassword);
	}

	private static GameServer CreateProtectedServer()
	{
		GameServer server = new()
		{
			Game = "Palworld",
			ServerName = "Portable Server",
			InstallPath = @"C:\Synix\Games\Portable Server"
		};
		SynixPasswordProtection.SetServerPasswords(
			server,
			PlaintextPasswords());
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
				SynixPortablePasswordTransfer.GetVaultPath(ImportRoot),
				overwrite: true);
		}

		public void Dispose()
		{
			if (Directory.Exists(_root))
				Directory.Delete(_root, recursive: true);
		}
	}
}
