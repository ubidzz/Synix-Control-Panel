using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Database.GameConfigurations;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixApp.UI.Configuration;
using Synix_Control_Panel.SynixApp.UI.ServerSetup;
using Synix_Control_Panel.SynixEngine;
using Synix_Control_Panel.SynixEngine.Minecraft;
using Synix_Control_Panel.SynixEngine.ModManagement;
using System.Text;
using System.Windows.Forms;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class ServerConfigurationSyncTests : IDisposable
{
	private readonly string _root = Path.Combine(Path.GetTempPath(), "SynixConfigSync-" + Guid.NewGuid().ToString("N"));
	private readonly GameServer[] _previousServers = ServerRegistry.Snapshot().ToArray();
	private readonly string? _previousData = ModPackageManager.DataRootOverride;
	private readonly GameServer _server;

	public ServerConfigurationSyncTests()
	{
		Directory.CreateDirectory(_root);
		ModPackageManager.DataRootOverride = Path.Combine(_root, "history");
		ServerRegistry.Servers.Clear();
		_server = new GameServer { Game = "ARK: Survival Ascended", ServerName = "Original", InstallPath = Path.Combine(_root, "server"),
			DataSchemaVersion = ServerDataMigrator.CurrentVersion, PasswordStorageVersion = Core.CurrentStorageVersion,
			Status = "Stopped", Port = 44001, QueryPort = 44002, RconPort = 44003, MaxPlayers = 4,
			WorldSeed = "12345", WorldSize = 4000, WorldName = "world", GameMode = "PVE" };
		Directory.CreateDirectory(_server.InstallPath);
		ServerRegistry.Servers.Add(_server);
	}

	// Keep fixture paths independent of their contents: a password-bearing test value
	// must not make a path-returning helper look like a credential lookup to CodeQL.
	private string FixturePath(string relative) => ModPathSafety.Resolve(_server.InstallPath, relative);

	private static void WriteFixture(string path, string text, Encoding? encoding = null)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.WriteAllText(path, text, encoding ?? new UTF8Encoding(false));
	}

	private void Save(string path, string text, Func<bool>? persist = null, ConfigFormat format = ConfigFormat.StandardINI) =>
		ServerConfigurationSync.Save(_server, path, text, format, MinecraftContentTransactions.HashFile(path), persist ?? (() => true));

	private const string ArkPath = "ShooterGame/Saved/Config/WindowsServer/GameUserSettings.ini";
	private const string ArkConfig = """
		; Keep this comment and these unrelated settings.
		[ServerSettings]
		ServerPassword="fixture-password"
		ServerAdminPassword="fixture-admin"
		RCONPort=44009
		RCONEnabled=True
		ServerPVE=False
		TamingSpeedMultiplier=3.5
		[SessionSettings]
		SessionName="Updated server"
		[/Script/Engine.GameSession]
		MaxPlayers=32
		[UnrelatedPlugin]
		ServerAdminPassword="not-the-server-password"
		""";

	[Theory]
	[InlineData("ARK: Survival Ascended")]
	[InlineData("ARK: Survival Evolved")]
	public void RestoredConfigurationRefreshesSharedValuesWithoutRewritingTheConfig(string game)
	{
		_server.Game = game;
		string path = FixturePath(ArkPath);
		WriteFixture(path, ArkConfig, Encoding.Unicode);
		byte[] original = File.ReadAllBytes(path);
		int saves = 0;
		ServerConfigurationSync.SynchronizeRestored(_server, () => { saves++; return true; });
		Assert.Equal(32, _server.MaxPlayers);
		Assert.Equal("Updated server", _server.ServerName);
		Assert.Equal("fixture-admin", Core.Reveal(_server.AdminPassword));
		Assert.True(Core.IsProtected(_server.AdminPassword));
		Assert.Equal(original, File.ReadAllBytes(path));
		Assert.False(File.Exists(path + ".synix.bak"));
		ServerConfigurationSync.SynchronizeRestored(_server, () => { saves++; return true; });
		Assert.Equal(1, saves);
	}

	[Fact]
	public void FailedRestoredSettingsPersistenceLeavesTheProfileAndConfigUnchanged()
	{
		string path = FixturePath(ArkPath);
		WriteFixture(path, ArkConfig);
		string before = Core.SerializeServersForStorage([_server]);
		Assert.Throws<InvalidDataException>(() => ServerConfigurationSync.SynchronizeRestored(_server, () => false));
		Assert.Equal(before, Core.SerializeServersForStorage([_server]));
		Assert.Equal(ArkConfig, File.ReadAllText(path));
	}

	[Theory]
	[InlineData("ARK: Survival Ascended")]
	[InlineData("ARK: Survival Evolved")]
	public void ArkSaveUpdatesSharedFieldsAndEncryptedStorageWithoutTouchingOtherServers(string game)
	{
		_server.Game = game;
		GameServer other = new() { ServerName = "Other", Game = game, InstallPath = Path.Combine(_root, "other"), MaxPlayers = 9 };
		ServerRegistry.Servers.Add(other);
		string path = FixturePath(ArkPath);
		WriteFixture(path, "[ServerSettings]\r\nServerAdminPassword=old\r\n", Encoding.Unicode);
		string storage = Path.Combine(_root, "servers.json");
		string backupFolder = Core.Instance.GetActiveServerBackupFolder(_server);
		int saves = 0;
		Save(path, ArkConfig, () => { saves++; File.WriteAllText(storage, Core.SerializeServersForStorage(ServerRegistry.Servers)); return true; });
		Assert.Equal("Updated server", _server.ServerName);
		Assert.Equal(32, _server.MaxPlayers);
		Assert.Equal(44009, _server.RconPort);
		Assert.True(_server.EnableRcon);
		Assert.Equal("PVP", _server.GameMode);
		Assert.Equal("fixture-password", Core.Reveal(_server.Password));
		Assert.Equal("fixture-admin", Core.Reveal(_server.AdminPassword));
		Assert.True(Core.IsProtected(_server.AdminPassword));
		string json = File.ReadAllText(storage);
		Assert.DoesNotContain("fixture-password", json);
		Assert.DoesNotContain("fixture-admin", json);
		GameServer reloaded = Core.DeserializeServersAndMigrate(json, out int _).Single(item => item.InstallPath == _server.InstallPath);
		Assert.Equal("fixture-admin", Core.Reveal(reloaded.AdminPassword));
		Assert.Equal(32, reloaded.MaxPlayers);
		Assert.Equal("Other", other.ServerName);
		Assert.Equal(9, other.MaxPlayers);
		Assert.Equal(1, saves);
		Assert.Equal(backupFolder, Core.Instance.GetActiveServerBackupFolder(_server));
		Assert.Equal(ArkConfig, File.ReadAllText(path));
		Assert.Equal(new byte[] { 0xff, 0xfe }, File.ReadAllBytes(path)[..2]);
	}

	[Theory]
	[InlineData("ARK: Survival Ascended")]
	[InlineData("ARK: Survival Evolved")]
	public void ActualEditorSaveThenReopenedServerSetupDisplaysNewPasswords(string game) => WorkflowUiTest.Run(() =>
	{
		_server.Game = game;
		string path = FixturePath(ArkPath);
		WriteFixture(path, "[ServerSettings]\nServerPassword=old\nServerAdminPassword=old-admin\n");
		using (ServerConfig editor = new(path, ConfigFormat.StandardINI, _server))
		{
			string storage = Path.Combine(_root, "servers.json");
			editor.PersistServerChanges = () => { File.WriteAllText(storage, Core.SerializeServersForStorage(ServerRegistry.Servers)); return true; };
			WorkflowUiTest.Invoke(editor, "LoadConfiguration");
			DataGridView grid = Assert.IsType<DataGridView>(editor.Controls.Find("dgvConfig", true).Single());
			grid.Rows.Cast<DataGridViewRow>().Single(row => row.Tag is ConfigLine { Key: "ServerAdminPassword" }).Cells["colValue"].Value = "new-fixture-admin";
			grid.Rows.Cast<DataGridViewRow>().Single(row => row.Tag is ConfigLine { Key: "ServerPassword" }).Cells["colValue"].Value = "new-fixture-password";
			WorkflowUiTest.Invoke(editor, "SaveConfiguration");
			Assert.Equal(DialogResult.OK, editor.DialogResult);
			GameServer persisted = Core.DeserializeServersAndMigrate(File.ReadAllText(storage), out int _).Single();
			Assert.Equal("new-fixture-admin", Core.Reveal(persisted.AdminPassword));
		}
		using ServerSettingsGUI setup = new(_server);
		ServerSettingsSecurityPage security = Assert.IsType<ServerSettingsSecurityPage>(setup.Controls.Find("pnlPageSecurity", true).Single());
		Assert.Equal("new-fixture-admin", security.txtAdminPassword.Text);
		Assert.Equal("new-fixture-password", security.txtPassword.Text);
	});

	[Fact]
	public void SavingUneditedConfigRepairsDriftAndRepeatedSaveDoesNotReencryptOrPersist()
	{
		string path = FixturePath(ArkPath);
		WriteFixture(path, ArkConfig);
		int saves = 0;
		Save(path, ArkConfig, () => { saves++; return true; });
		string ciphertext = _server.AdminPassword;
		Save(path, ArkConfig, () => { saves++; return true; });
		Assert.Equal(1, saves);
		Assert.Equal(ciphertext, _server.AdminPassword);
		Assert.Equal(32, _server.MaxPlayers);
	}

	[Fact]
	public void ExplicitEmptyPasswordClearsButMissingFieldsAndPluginKeysDoNot()
	{
		_server.Password = Core.Protect("old-password");
		_server.AdminPassword = Core.Protect("old-admin");
		string text = "[ServerSettings]\nServerPassword=\n[Plugin]\nServerAdminPassword=unrelated\n";
		string path = FixturePath(ArkPath);
		WriteFixture(path, text);
		Save(path, text);
		Assert.Empty(_server.Password);
		Assert.Equal("old-admin", Core.Reveal(_server.AdminPassword));
		Assert.Equal(4, _server.MaxPlayers);
	}

	[Theory]
	[InlineData("Plugin/GameUserSettings.ini")]
	[InlineData("ShooterGame/Saved/Config/WindowsServer/Other.ini")]
	public void UnmappedFilesDoNotUpdateTheServerEvenWhenKeysLookSimilar(string relative)
	{
		string path = FixturePath(relative);
		WriteFixture(path, "ServerAdminPassword=old");
		Save(path, "ServerAdminPassword=new", () => throw new InvalidOperationException("Must not persist"));
		Assert.Empty(_server.AdminPassword);
		Assert.Equal("ServerAdminPassword=new", File.ReadAllText(path));
	}

	[Theory]
	[InlineData("[ServerSettings]\nRCONPort=99999")]
	[InlineData("[/Script/Engine.GameSession]\nMaxPlayers=oops")]
	[InlineData("[ServerSettings]\nRCONEnabled=maybe")]
	[InlineData("[ServerSettings]\nServerAdminPassword=one\nServerAdminPassword=two")]
	public void InvalidOrAmbiguousValuesRejectBothWrites(string text)
	{
		string path = FixturePath(ArkPath);
		WriteFixture(path, "[ServerSettings]\nServerAdminPassword=old");
		Assert.Throws<InvalidDataException>(() => Save(path, text));
		Assert.Equal("[ServerSettings]\nServerAdminPassword=old", File.ReadAllText(path));
		Assert.Empty(_server.AdminPassword);
		Assert.Equal(4, _server.MaxPlayers);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void FailedStorageRollsBackFileAndMemoryAndPreservesBackup(bool throws)
	{
		_server.AdminPassword = Core.Protect("before");
		string encrypted = _server.AdminPassword;
		string path = FixturePath(ArkPath);
		WriteFixture(path, "[ServerSettings]\nServerAdminPassword=before");
		File.WriteAllText(path + ".synix.bak", "older backup");
		Assert.ThrowsAny<Exception>(() => Save(path, ArkConfig, () => throws ? throw new IOException("fixture failure") : false));
		Assert.Equal("[ServerSettings]\nServerAdminPassword=before", File.ReadAllText(path));
		Assert.Equal(encrypted, _server.AdminPassword);
		Assert.Equal("Original", _server.ServerName);
		Assert.Equal(4, _server.MaxPlayers);
		Assert.Equal("older backup", File.ReadAllText(path + ".synix.bak"));
	}

	[Fact]
	public void ExternalEditAfterOpenIsNotOverwritten()
	{
		string path = FixturePath(ArkPath);
		WriteFixture(path, ArkConfig);
		string? hash = MinecraftContentTransactions.HashFile(path);
		File.WriteAllText(path, "external edit");
		Assert.Throws<InvalidDataException>(() => ServerConfigurationSync.Save(_server, path, ArkConfig, ConfigFormat.StandardINI, hash, () => true));
		Assert.Equal("external edit", File.ReadAllText(path));
		Assert.Equal("Original", _server.ServerName);
	}

	[Theory]
	[InlineData("Enshrouded", "enshrouded_server.json", ConfigFormat.JSON, "{\"name\":\"Updated server\",\"userGroups\":[{\"name\":\"Visitor\",\"password\":\"fixture-password\"},{\"name\":\"Admin\",\"password\":\"fixture-admin\"}],\"slotCount\":12,\"queryPort\":44100}")]
	[InlineData("Just Cause 2: Multiplayer", "config.lua", ConfigFormat.StandardINI, "Name = \"Updated server\", -- comment\nPassword = \"fixture-password\",\nMaxPlayers = 12,\nBindPort = 44100,")]
	[InlineData("Project CARS 2", "server.cfg", ConfigFormat.StandardINI, "name : \"Updated server\" // comment\npassword : \"fixture-password\"\nmaxPlayerCount : 12\nhostPort : 44100")]
	[InlineData("7 Days to Die", "serverconfig.xml", ConfigFormat.XML, "<ServerSettings><property name=\"ServerName\" value=\"Updated server\"/><property name=\"ServerPassword\" value=\"fixture-password\"/><property name=\"ServerMaxPlayerCount\" value=\"12\"/><property name=\"ServerPort\" value=\"44100\"/></ServerSettings>")]
	[InlineData("Rust", "server/Original/cfg/server.cfg", ConfigFormat.Space, "server.hostname \"Updated server\"\nserver.maxplayers 12\nrcon.password \"fixture-password\"\nrcon.port 44100")]
	[InlineData("Palworld", "Pal/Saved/Config/WindowsServer/PalWorldSettings.ini", ConfigFormat.StandardINI, "[/Script/Pal.PalGameWorldSettings]\nOptionSettings=(ServerName=\"Updated server\",ServerPassword=\"fixture-password\",ServerPlayerMaxNum=12,PublicPort=44100)")]
	public void DifferentGameHandlersAndFormatsUpdateTheSameServerRecord(string game, string relative, ConfigFormat format, string text)
	{
		_server.Game = game;
		string path = FixturePath(relative);
		WriteFixture(path, text);
		int saves = 0;
		Save(path, text, () => { saves++; return true; }, format);
		Assert.Equal("Updated server", _server.ServerName);
		Assert.Equal(12, _server.MaxPlayers);
		Assert.Equal("fixture-password", Core.Reveal(game == "Rust" ? _server.RconPassword : _server.Password));
		if (game == "Enshrouded") Assert.Equal("fixture-admin", Core.Reveal(_server.AdminPassword));
		if (game == "Rust")
		{
			Assert.Equal("Original", Core.GetServerIdentity(_server));
			Assert.True(GameFix.TryGetConfiguration(game, out ConfigurationDefinition? definition));
			Assert.Equal(path, definition!.ResolveFullPath(_server));
			Assert.True(GameLaunchCommandBuilder.TryBuildArguments(_server, GameDatabase.GetGame(game)!, "258550",
				Core.RevealServerPasswords(_server), out string arguments, out string error), error);
			Assert.Contains("+server.identity \"Original\"", arguments);
		}
		Assert.Equal(1, saves);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void LegacyCredentialMigrationDoesNotDoubleEncryptAndRollsBackOnFailure(bool fail)
	{
		_server.PasswordStorageVersion = 0;
		_server.Password = "legacy-password";
		_server.AdminPassword = "legacy-admin";
		_server.AuthenticationToken = "legacy-token";
		string path = FixturePath(ArkPath);
		WriteFixture(path, "[ServerSettings]\nServerAdminPassword=updated-admin");
		bool Persist() { _ = Core.SerializeServersForStorage([_server]); return !fail; }
		if (fail)
		{
			Assert.ThrowsAny<Exception>(() => Save(path, File.ReadAllText(path), Persist));
			Assert.Equal(0, _server.PasswordStorageVersion);
			Assert.Equal("legacy-admin", _server.AdminPassword);
			Assert.Equal("legacy-password", _server.Password);
			Assert.Equal("legacy-token", _server.AuthenticationToken);
		}
		else
		{
			Save(path, File.ReadAllText(path), Persist);
			Assert.Equal(Core.CurrentStorageVersion, _server.PasswordStorageVersion);
			Assert.Equal("updated-admin", Core.Reveal(_server.AdminPassword));
			Assert.Equal("legacy-password", Core.Reveal(_server.Password));
			Assert.Equal("legacy-token", Core.Reveal(_server.AuthenticationToken));
		}
	}

	[Fact]
	public void IniCasingStillMatchesTheSharedSetting()
	{
		string path = FixturePath(ArkPath);
		WriteFixture(path, "[serversettings]\nserveradminpassword=updated-admin");
		Save(path, File.ReadAllText(path));
		Assert.Equal("updated-admin", Core.Reveal(_server.AdminPassword));
	}

	[Fact]
	public void RollbackDoesNotOverwriteAnExternalEditMadeDuringFailedPersistence()
	{
		string path = FixturePath(ArkPath);
		WriteFixture(path, "[ServerSettings]\nServerAdminPassword=before");
		Assert.Throws<IOException>(() => Save(path, ArkConfig, () => { File.WriteAllText(path, "external edit"); return false; }));
		Assert.Equal("external edit", File.ReadAllText(path));
		Assert.Equal("[ServerSettings]\nServerAdminPassword=before", File.ReadAllText(path + ".synix.bak"));
		Assert.Empty(_server.AdminPassword);
	}

	[Fact]
	public void PortsPlayersAndWorldSettingsComeFromYamlWithoutRewritingIt()
	{
		_server.Game = "Empyrion - Galactic Survival";
		const string text = "ServerConfig:\n  Srv_Port: 44200\n  Srv_Name: 'YAML server'\n  Srv_Password: 'yaml-password'\n  Srv_MaxPlayers: 12\nGameConfig:\n  Seed: 4567\n  CustomScenario: 'Custom world'\n";
		string path = FixturePath("dedicated.yaml");
		WriteFixture(path, text);
		Save(path, text, format: ConfigFormat.YAML);
		Assert.Equal("YAML server", _server.ServerName);
		Assert.Equal(44200, _server.Port);
		Assert.Equal(12, _server.MaxPlayers);
		Assert.Equal("4567", _server.WorldSeed);
		Assert.Equal("Custom world", _server.WorldName);
		Assert.Equal(text, File.ReadAllText(path));
	}

	[Fact]
	public void DerivedOrCompositeTemplateValuesCannotOverwriteSharedFields()
	{
		var bindings = ConfigurationServerBinding.FromTemplate("[Server]\nName={ServerName}\nDescription=Welcome to {ServerName}\nIdentity={Identity}\nProtected={HasPassword}\n", ConfigFormat.StandardINI);
		ConfigurationServerBinding binding = Assert.Single(bindings);
		Assert.Equal("Name", binding.Key);
		Assert.Equal(ConfigurationServerField.ServerName, binding.Field);
	}

	public static IEnumerable<object[]> TemplateGames => GameDatabase.GetGames
		.Where(game => GameFix.TryGetConfiguration(game.Game, out ConfigurationDefinition? definition) && definition is TemplateConfigurationDefinition)
		.Select(game => new object[] { game.Game });

	[Theory]
	[MemberData(nameof(TemplateGames))]
	public void EveryDatabaseTemplateCanResolveSharedSettingsAndSaveWithoutSpecialGameBranches(string game)
	{
		_server.Game = game;
		Assert.True(GameFix.TryGetConfiguration(game, out ConfigurationDefinition? definition));
		Assert.NotNull(definition);
		ConfigurationContext context = new(_server, new("fixture-password", "fixture-admin", "fixture-rcon"), "Original", "127.0.0.1", "127.0.0.1");
		if (game == "ASTRONEER")
		{
			WriteFixture(FixturePath("Astro/Saved/Config/WindowsServer/AstroServerSettings.ini"), "PublicIP=127.0.0.1\nOwnerName=\nOwnerGuid=0");
			WriteFixture(FixturePath("Astro/Saved/Config/WindowsServer/Engine.ini"), "[URL]\nPort=44001");
		}
		ConfigurationApplyResult result = definition.Apply(context);
		Assert.True(result.Succeeded, result.Message);
		ServerConfigurationSync.SynchronizeRestored(_server, () => true);
		foreach (string path in definition.ResolveConfigurationPaths(_server))
		{
			Assert.True(File.Exists(path), path);
			IReadOnlyList<ConfigurationServerBinding> bindings = definition.GetServerBindings(_server, path);
			string text = File.ReadAllText(path);
			Save(path, text, format: definition.Format);
			if (bindings.Any(binding => binding.Field == ConfigurationServerField.Password))
				Assert.Equal("fixture-password", Core.Reveal(_server.Password));
			if (bindings.Any(binding => binding.Field == ConfigurationServerField.AdminPassword))
				Assert.Equal("fixture-admin", Core.Reveal(_server.AdminPassword));
		}
	}

	public void Dispose()
	{
		ServerRegistry.Servers.Clear();
		foreach (GameServer server in _previousServers) ServerRegistry.Servers.Add(server);
		ModPackageManager.DataRootOverride = _previousData;
		Directory.Delete(_root, recursive: true);
	}
}
