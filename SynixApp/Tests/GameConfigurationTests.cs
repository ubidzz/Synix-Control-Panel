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
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Database.GameConfigurations;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class GameConfigurationTests : IDisposable
{
	private readonly string _testRoot = Path.Combine(
		Path.GetTempPath(),
		"SynixGameConfigurationTests",
		Guid.NewGuid().ToString("N"));

	public GameConfigurationTests()
	{
		Directory.CreateDirectory(_testRoot);
	}

	public static TheoryData<string> ManagedGameNames => new()
	{
		"7 Days to Die",
		"Soulmask",
		"Palworld",
		"Rust",
		"Minecraft",
		"Minecraft Java",
		"StarRupture",
		"Subsistence",
		"Windrose",
		"ASKA",
		"Just Cause 3 Multiplayer",
		"Sons Of The Forest",
		"Enshrouded",
		"Longvinter",
		"Ground Branch",
		"Holdfast: Nations At War",
		"V Rising",
		"Out of Reach",
		"NS2: Combat",
		"Just Cause 2: Multiplayer",
		"Beyond the Wire",
		"Colony Survival",
		"Core Keeper",
		"Factorio",
		"Eco",
		"Project CARS 2",
		"Assetto Corsa Competizione",
		"rFactor 2",
		"Survive the Nights",
		"Foundry",
		"HumanitZ",
		"ASTRONEER",
		"DayZ",
		"Arma 3",
		"Arma Reforger",
		"Mount & Blade II: Bannerlord",
		"Dysterra",
		"Serious Sam 2017",
		"Serious Sam HD: The Second Encounter",
		"Serious Sam HD: The First Encounter",
		"Serious Sam 3: BFE",
		"Wreckfest"
	};

	[Theory]
	[MemberData(nameof(ManagedGameNames))]
	public void GameFix_IndexFindsEveryConfiguration(string gameName)
	{
		Assert.True(GameFix.TryGetConfiguration(gameName, out ConfigurationDefinition? definition));
		Assert.NotNull(definition);
	}

	[Theory]
	[MemberData(nameof(ManagedGameNames))]
	public void EveryIndexedConfiguration_CanBeApplied(string gameName)
	{
		Assert.True(GameFix.TryGetConfiguration(gameName, out ConfigurationDefinition? definition));
		Assert.NotNull(definition);
		GameServer server = CreateServer(gameName);
		if (gameName == "Wreckfest")
		{
			File.WriteAllText(
				Path.Combine(server.InstallPath, "initial_server_config.cfg"),
				"server_name=Generated");
		}

		ConfigurationApplyResult result = definition.Apply(CreateContext(server));

		Assert.True(result.Succeeded, result.Message);
		Assert.True(result.Complete, result.Message);
		Assert.True(definition.ConfigurationFileExists(server));
		foreach (string file in Directory.EnumerateFiles(
			server.InstallPath,
			"*",
			SearchOption.AllDirectories))
		{
			string content = File.ReadAllText(file);
			Assert.DoesNotContain("{ServerName}", content);
			Assert.DoesNotContain("{Password}", content);
			Assert.DoesNotContain("{AdminPassword}", content);
			Assert.DoesNotContain("{MaxPlayers}", content);
			Assert.DoesNotContain("{Port}", content);
			Assert.DoesNotContain("{QueryPort}", content);
		}
	}

	[Theory]
	[InlineData("7 Days to Die")]
	[InlineData("Palworld")]
	[InlineData("Rust")]
	[InlineData("Minecraft")]
	public void ConfigurationDefinitions_MatchMainDatabasePaths(string gameName)
	{
		Assert.True(GameFix.TryGetConfiguration(gameName, out ConfigurationDefinition? definition));
		GameInfo? game = GameDatabase.GetGame(gameName);

		Assert.NotNull(definition);
		Assert.NotNull(game);
		Assert.Equal(game.RelativeConfigPath, definition.RelativePath);
		Assert.Equal(game.Format, definition.Format);
	}

	[Fact]
	public void Minecraft_CreatesAndUpdatesOnlyManagedProperties()
	{
		MinecraftConfiguration definition = new();
		GameServer server = CreateServer("Minecraft");
		server.ServerName = "Initial Server";
		server.Port = 25565;
		server.QueryPort = 25566;
		server.MaxPlayers = 12;
		server.WorldName = "world_one";
		server.WorldSeed = "seed-one";
		server.EnableRcon = true;
		server.RconPort = 25575;

		ConfigurationApplyResult created = definition.Apply(CreateContext(server));
		string path = definition.ResolveFullPath(server);
		File.AppendAllText(path, "difficulty=hard\n");

		server.ServerName = "Updated Server";
		server.Port = 26565;
		server.MaxPlayers = 20;
		ConfigurationApplyResult updated = definition.Apply(CreateContext(server));

		Assert.True(created.Succeeded);
		Assert.True(created.Created);
		Assert.True(updated.Succeeded);
		Assert.True(updated.Changed);
		Assert.Equal("Updated Server", GetValue(path, definition.Format, "motd"));
		Assert.Equal("26565", GetValue(path, definition.Format, "server-port"));
		Assert.Equal("20", GetValue(path, definition.Format, "max-players"));
		Assert.Equal("hard", GetValue(path, definition.Format, "difficulty"));
		Assert.True(File.Exists(path + ".synix.bak"));
	}

	[Fact]
	public void Palworld_UpdatesNestedManagedValuesAndPreservesGameplaySettings()
	{
		PalworldConfiguration definition = new();
		GameServer server = CreateServer("Palworld");
		server.ServerName = "Pal One";
		server.Port = 8211;
		server.QueryPort = 8212;
		server.MaxPlayers = 10;
		server.GameMode = "PVE";
		server.EnableRcon = false;
		server.RconPort = 25575;

		ConfigurationApplyResult created = definition.Apply(CreateContext(server));
		string path = definition.ResolveFullPath(server);
		SetValue(path, definition.Format, "ExpRate", "2.500000");

		server.ServerName = "Pal Two";
		server.MaxPlayers = 24;
		server.GameMode = "PVP";
		server.EnableRcon = true;
		ConfigurationApplyResult updated = definition.Apply(CreateContext(server));

		Assert.True(created.Succeeded);
		Assert.True(updated.Succeeded);
		Assert.Equal("Pal Two", GetValue(path, definition.Format, "ServerName"));
		Assert.Equal("24", GetValue(path, definition.Format, "ServerPlayerMaxNum"));
		Assert.Equal("True", GetValue(path, definition.Format, "bIsPvP"));
		Assert.Equal("True", GetValue(path, definition.Format, "RCONEnabled"));
		Assert.Equal("2.500000", GetValue(path, definition.Format, "ExpRate"));
	}

	[Fact]
	public void SevenDaysToDie_UpdatesXmlPropertiesAndPreservesOtherProperties()
	{
		SevenDaysToDieConfiguration definition = new();
		GameServer server = CreateServer("7 Days to Die");
		server.ServerName = "Seven One";
		server.Port = 26900;
		server.MaxPlayers = 8;
		server.WorldName = "Navezgane";
		server.WorldSeed = "first-seed";
		server.WorldSize = 6144;

		ConfigurationApplyResult created = definition.Apply(CreateContext(server));
		string path = definition.ResolveFullPath(server);
		string xml = File.ReadAllText(path).Replace(
			"</ServerSettings>",
			"  <property name=\"ServerDescription\" value=\"Keep this text\"/>\n</ServerSettings>",
			StringComparison.Ordinal);
		File.WriteAllText(path, xml);

		server.ServerName = "Seven Two";
		server.MaxPlayers = 16;
		ConfigurationApplyResult updated = definition.Apply(CreateContext(server));

		Assert.True(created.Succeeded);
		Assert.True(updated.Succeeded);
		Assert.Equal("Seven Two", GetValue(path, definition.Format, "ServerName"));
		Assert.Equal("16", GetValue(path, definition.Format, "ServerMaxPlayerCount"));
		Assert.Equal("Keep this text", GetValue(path, definition.Format, "ServerDescription"));
	}

	[Fact]
	public void Rust_UpdatesConfigurationWhenIdentityDoesNotChange()
	{
		RustConfiguration definition = new();
		GameServer server = CreateServer("Rust");
		server.ServerName = "Rust Test";
		server.MaxPlayers = 18;
		server.WorldName = "Procedural Map";
		server.WorldSeed = "12345";
		server.WorldSize = 4000;
		server.GameMode = "PVP";
		server.EnableRcon = true;
		server.RconPort = 28016;

		Assert.True(definition.Apply(CreateContext(server)).Succeeded);
		string path = definition.ResolveFullPath(server);
		SetValue(path, definition.Format, "fps.limit", "144");
		server.MaxPlayers = 32;
		server.GameMode = "PVE";

		ConfigurationApplyResult updated = definition.Apply(CreateContext(server));

		Assert.True(updated.Succeeded);
		Assert.Equal("32", GetValue(path, definition.Format, "server.maxplayers"));
		Assert.Equal("True", GetValue(path, definition.Format, "server.pve"));
		Assert.Equal("144", GetValue(path, definition.Format, "fps.limit"));
	}

	[Fact]
	public void Rust_DisabledRconDoesNotWriteSavedPassword()
	{
		RustConfiguration definition = new();
		GameServer server = CreateServer("Rust");
		server.EnableRcon = false;
		server.RconPort = 28016;

		ConfigurationApplyResult result = definition.Apply(CreateContext(server));
		string path = definition.ResolveFullPath(server);

		Assert.True(result.Succeeded);
		Assert.Equal(string.Empty, GetValue(path, definition.Format, "rcon.password"));
		Assert.Equal("True", GetValue(path, definition.Format, "rcon.web"));
	}

	[Fact]
	public void Soulmask_LeavesGameplayJsonForAdvancedEditor()
	{
		SoulmaskConfiguration definition = new();
		GameServer server = CreateServer("Soulmask");

		ConfigurationApplyResult result = definition.Apply(CreateContext(server));

		Assert.True(result.Succeeded);
		Assert.True(result.Complete);
		Assert.False(result.Changed);
		Assert.Empty(Directory.EnumerateFileSystemEntries(server.InstallPath));
	}

	[Fact]
	public void TemplateConfiguration_CreatesEveryRequiredFileWithoutOverwriting()
	{
		SubsistenceConfiguration definition = new();
		GameServer server = CreateServer("Subsistence");

		ConfigurationApplyResult created = definition.Apply(CreateContext(server));
		string settingsPath = Path.Combine(
			server.InstallPath,
			@"UDKGame\Config\UDKDedServerSettings.ini");
		string enginePath = Path.Combine(
			server.InstallPath,
			@"UDKGame\Config\UDKEngine.ini");
		File.AppendAllText(settingsPath, "CustomSetting=True\n");

		ConfigurationApplyResult repeated = definition.Apply(CreateContext(server));

		Assert.True(created.Succeeded);
		Assert.True(created.Created);
		Assert.True(File.Exists(settingsPath));
		Assert.True(File.Exists(enginePath));
		Assert.True(repeated.Succeeded);
		Assert.False(repeated.Changed);
		Assert.Contains("CustomSetting=True", File.ReadAllText(settingsPath));
	}

	[Fact]
	public void Wreckfest_CopiesGeneratedConfigurationOnce()
	{
		WreckfestConfiguration definition = new();
		GameServer server = CreateServer("Wreckfest");
		string sourcePath = Path.Combine(server.InstallPath, "initial_server_config.cfg");
		string targetPath = Path.Combine(server.InstallPath, "server_config.cfg");
		File.WriteAllText(sourcePath, "server_name=Generated");

		ConfigurationApplyResult created = definition.Apply(CreateContext(server));
		File.WriteAllText(sourcePath, "server_name=ChangedSource");
		ConfigurationApplyResult repeated = definition.Apply(CreateContext(server));

		Assert.True(created.Succeeded);
		Assert.True(created.Created);
		Assert.True(repeated.Succeeded);
		Assert.False(repeated.Changed);
		Assert.Equal("server_name=Generated", File.ReadAllText(targetPath));
	}

	[Fact]
	public void ConfigurationPath_CannotLeaveServerInstallFolder()
	{
		EscapingConfiguration definition = new();
		GameServer server = CreateServer("Unsafe Test");

		ConfigurationApplyResult result = definition.Apply(CreateContext(server));

		Assert.False(result.Succeeded);
		Assert.False(File.Exists(Path.Combine(_testRoot, "outside.cfg")));
	}

	public void Dispose()
	{
		if (Directory.Exists(_testRoot))
		{
			Directory.Delete(_testRoot, true);
		}
	}

	private GameServer CreateServer(string game)
	{
		string installPath = Path.Combine(_testRoot, Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(installPath);
		return new GameServer
		{
			Game = game,
			InstallPath = installPath,
			ServerName = "Test Server",
			WorldName = "world",
			WorldSeed = "12345",
			WorldSize = 4000,
			GameMode = "PVE",
			Port = 7777,
			QueryPort = 27015,
			MaxPlayers = 10,
			RconPort = 28016
		};
	}

	private static ConfigurationContext CreateContext(GameServer server)
	{
		return new ConfigurationContext(
			server,
			new SynixServerPasswords("server-secret", "admin-secret", "rcon-secret"),
			Core.Instance.GetSafeName(server.ServerName),
			"192.0.2.10",
			"198.51.100.10");
	}

	private static string GetValue(string path, ConfigFormat format, string key)
	{
		ConfigLine value = Assert.Single(
			ConfigHandler.LoadConfig(path, format),
			item => item.Key == key);
		return value.Value;
	}

	private static void SetValue(
		string path,
		ConfigFormat format,
		string key,
		string value)
	{
		List<ConfigLine> values = ConfigHandler.LoadConfig(path, format);
		ConfigLine target = Assert.Single(values, item => item.Key == key);
		target.Value = value;
		ConfigHandler.SaveConfig(path, values, format);
	}

	private sealed class EscapingConfiguration : ConfigurationDefinition
	{
		public override string GameName => "Unsafe Test";
		public override string RelativePath => @"..\outside.cfg";
		public override IReadOnlyList<ConfigurationBinding> Bindings => [];
		public override string CreateTemplate(ConfigurationContext context) => "value=test\n";
	}
}
