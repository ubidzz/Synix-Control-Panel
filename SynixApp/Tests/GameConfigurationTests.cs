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
using System.Text.Json;
using System.Text.Json.Nodes;
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

	public static TheoryData<string> IndexedGameNames => new()
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
		"ARK: Survival Ascended",
		"ARK: Survival Evolved",
		"Mount & Blade II: Bannerlord",
		"Dysterra",
		"Serious Sam 2017",
		"Serious Sam HD: The Second Encounter",
		"Serious Sam HD: The First Encounter",
		"Serious Sam 3: BFE",
		"Wreckfest"
	};

	public static TheoryData<string> ManagedGameNames => new()
	{
		"7 Days to Die",
		"Soulmask",
		"Palworld",
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
		"Just Cause 2: Multiplayer",
		"Beyond the Wire",
		"Colony Survival",
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
		"ARK: Survival Ascended",
		"ARK: Survival Evolved",
		"Mount & Blade II: Bannerlord",
		"Dysterra",
		"Wreckfest"
	};

	[Theory]
	[MemberData(nameof(IndexedGameNames))]
	public void GameFix_IndexFindsEveryConfiguration(string gameName)
	{
		Assert.True(GameFix.TryGetConfiguration(gameName, out ConfigurationDefinition? definition));
		Assert.NotNull(definition);
	}

	[Fact]
	public void EverySynixTemplateGameHasACompleteConfigurationDefinition()
	{
		GameInfo[] templateGames = GameDatabase.GetGames
			.Where(game =>
				game.ConfigFileCreation == ConfigFileCreationMode.SynixTemplate)
			.ToArray();

		Assert.NotEmpty(templateGames);
		foreach (GameInfo game in templateGames)
		{
			Assert.False(string.IsNullOrWhiteSpace(game.RelativeConfigPath));
			Assert.True(
				GameFix.TryGetConfiguration(
					game.Game,
					out ConfigurationDefinition? definition),
				$"{game.Game} is marked for Synix template creation but has no definition.");
			Assert.NotNull(definition);
			Assert.True(definition.UsesConfigurationFile);
			Assert.True(definition.SupportsFullReset);
			Assert.False(string.IsNullOrWhiteSpace(definition.RelativePath));
		}
	}

	[Theory]
	[InlineData("Soulmask")]
	[InlineData("Wreckfest")]
	[InlineData("ASTRONEER")]
	[InlineData("ASKA")]
	[InlineData("Assetto Corsa Competizione")]
	[InlineData("7 Days to Die")]
	[InlineData("Subsistence")]
	[InlineData("Holdfast: Nations At War")]
	[InlineData("Windrose")]
	[InlineData("Just Cause 3 Multiplayer")]
	[InlineData("rFactor 2")]
	[InlineData("Ground Branch")]
	public void GameGeneratedConfigurationsAreExplicitlyMarked(string gameName)
	{
		Assert.Equal(
			ConfigFileCreationMode.GameGenerated,
			GameFix.GetConfigFileCreationMode(gameName));
		Assert.True(GameFix.TryGetConfiguration(
			gameName,
			out ConfigurationDefinition? definition));
		Assert.NotNull(definition);
	}

	[Fact]
	public void UnverifiedGamesDoNotTriggerAutomaticConfigCreation()
	{
		GameInfo[] unverifiedGames = GameDatabase.GetGames
			.Where(game =>
				game.ConfigFileCreation == ConfigFileCreationMode.Unknown)
			.ToArray();

		Assert.NotEmpty(unverifiedGames);
		foreach (GameInfo game in unverifiedGames)
		{
			Assert.False(GameFix.NeedsManagedConfiguration(
				CreateServer(game.Game)));
		}
	}

	[Fact]
	public async Task PostInstall_DoesNotCreateAnUnverifiedPartialConfiguration()
	{
		GameServer server = CreateServer("Out of Reach");
		string configPath = Path.Combine(server.InstallPath, "ServerConfig.json");

		bool changed = await GameFix.PostInstall(server);

		Assert.False(changed);
		Assert.False(File.Exists(configPath));
	}

	[Fact]
	public void CoreKeeperUsesVerifiedLaunchArgumentsWithoutCreatingAConfig()
	{
		GameServer server = CreateServer("Core Keeper");
		Assert.Equal(
			ConfigFileCreationMode.LaunchArgumentsOnly,
			GameFix.GetConfigFileCreationMode(server.Game));
		Assert.False(GameFix.NeedsManagedConfiguration(server));
		Assert.True(GameFix.TryGetConfiguration(
			server.Game,
			out ConfigurationDefinition? definition));
		Assert.NotNull(definition);
		Assert.False(definition.UsesConfigurationFile);
		Assert.False(definition.SupportsFullReset);
	}

	[Theory]
	[MemberData(nameof(ManagedGameNames))]
	public void EveryIndexedConfiguration_CanBeApplied(string gameName)
	{
		Assert.True(GameFix.TryGetConfiguration(gameName, out ConfigurationDefinition? definition));
		Assert.NotNull(definition);
		GameServer server = CreateServer(gameName);
		PrepareGeneratedConfiguration(gameName, server);

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
	[MemberData(nameof(ManagedGameNames))]
	public void EveryIndexedConfiguration_CanBeReappliedAfterSettingsChange(
		string gameName)
	{
		Assert.True(GameFix.TryGetConfiguration(
			gameName,
			out ConfigurationDefinition? definition));
		Assert.NotNull(definition);
		GameServer server = CreateServer(gameName);
		PrepareGeneratedConfiguration(gameName, server);

		ConfigurationApplyResult created = definition.Apply(CreateContext(server));
		Assert.True(created.Succeeded, created.Message);

		server.MaxPlayers = 28;
		server.Port = 28888;
		server.QueryPort = 28889;
		server.RconPort = 28890;
		server.AppPort = 28891;
		ConfigurationApplyResult updated = definition.Apply(CreateContext(server));

		Assert.True(updated.Succeeded, updated.Message);
		Assert.True(updated.Complete, updated.Message);
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
		SetValue(path, definition.Format, "difficulty", "hard");

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
		PrepareGeneratedConfiguration("Palworld", server);
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
		Assert.Equal("True", GetValue(path, definition.Format, "bEnablePlayerToPlayerDamage"));
		Assert.Equal("True", GetValue(path, definition.Format, "bEnableDefenseOtherGuildPlayer"));
		Assert.Equal("True", GetValue(path, definition.Format, "RCONEnabled"));
		Assert.Equal(string.Empty, GetValue(path, definition.Format, "PublicIP"));
		Assert.Equal("2.500000", GetValue(path, definition.Format, "ExpRate"));
	}

	[Fact]
	public void SevenDaysToDie_UpdatesXmlPropertiesAndPreservesOtherProperties()
	{
		SevenDaysToDieConfiguration definition = new();
		GameServer server = CreateServer("7 Days to Die");
		PrepareGeneratedConfiguration("7 Days to Die", server);
		server.ServerName = "Seven One";
		server.Port = 26900;
		server.MaxPlayers = 8;
		server.WorldName = "Navezgane";
		server.WorldSeed = "first-seed";
		server.WorldSize = 6144;

		ConfigurationApplyResult created = definition.Apply(CreateContext(server));
		string path = definition.ResolveFullPath(server);
		SetValue(path, definition.Format, "ServerDescription", "Keep this text");

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
	public async Task SevenDaysToDie_FirstGeneratedConfigurationReceivesSavedSynixSettings()
	{
		GameServer server = CreateServer("7 Days to Die");
		server.ServerName = "Saved Seven Server";
		server.Port = 26942;
		server.MaxPlayers = 14;
		server.WorldName = "Pregen8k";
		server.WorldSeed = "saved-seed";
		server.WorldSize = 8192;
		Core.SetServerPasswords(
			server,
			new SynixServerPasswords("saved-password", string.Empty, string.Empty));
		PrepareGeneratedConfiguration("7 Days to Die", server);

		ConfigurationApplyResult? result =
			await GameFix.ApplyFirstGeneratedConfiguration(server);
		SevenDaysToDieConfiguration definition = new();
		string path = definition.ResolveFullPath(server);

		Assert.NotNull(result);
		ConfigurationApplyResult applied = result.Value;
		Assert.True(applied.Succeeded);
		Assert.True(applied.Complete);
		Assert.True(applied.Changed);
		Assert.Equal("Saved Seven Server", GetValue(path, definition.Format, "ServerName"));
		Assert.Equal("saved-password", GetValue(path, definition.Format, "ServerPassword"));
		Assert.Equal("26942", GetValue(path, definition.Format, "ServerPort"));
		Assert.Equal("14", GetValue(path, definition.Format, "ServerMaxPlayerCount"));
		Assert.Equal("Pregen8k", GetValue(path, definition.Format, "GameWorld"));
		Assert.Equal("saved-seed", GetValue(path, definition.Format, "WorldGenSeed"));
		Assert.Equal("8192", GetValue(path, definition.Format, "WorldGenSize"));
		Assert.Equal(definition.SchemaVersion, server.ManagedConfigurationVersion);
		Assert.Null(await GameFix.ApplyFirstGeneratedConfiguration(server));
	}

	[Fact]
	public void ConfigurationValidator_PassesWhenSavedValuesMatchTheFile()
	{
		SevenDaysToDieConfiguration definition = new();
		GameServer server = CreateServer("7 Days to Die");
		PrepareGeneratedConfiguration("7 Days to Die", server);
		ConfigurationContext context = CreateContext(server);

		ConfigurationApplyResult applied = definition.Apply(context);
		IReadOnlyList<ConfigurationValidationItem> items =
			definition.Validate(context);

		Assert.True(applied.Succeeded);
		Assert.DoesNotContain(items, item =>
			item.State == ConfigurationValidationState.Failed);
		Assert.Contains(items, item =>
			item.Setting == "ServerName" &&
			item.State == ConfigurationValidationState.Passed);
	}

	[Fact]
	public void ConfigurationValidator_FailsWhenSavedValueWasNotApplied()
	{
		SevenDaysToDieConfiguration definition = new();
		GameServer server = CreateServer("7 Days to Die");
		PrepareGeneratedConfiguration("7 Days to Die", server);
		ConfigurationContext originalContext = CreateContext(server);
		Assert.True(definition.Apply(originalContext).Succeeded);

		server.ServerName = "A newer saved server name";
		IReadOnlyList<ConfigurationValidationItem> items =
			definition.Validate(CreateContext(server));

		Assert.Contains(items, item =>
			item.Setting == "ServerName" &&
			item.State == ConfigurationValidationState.Failed &&
			item.Message.Contains("does not match", StringComparison.Ordinal));
	}

	[Fact]
	public void ConfigurationValidator_FailsWhenManagedTagIsMissing()
	{
		SevenDaysToDieConfiguration definition = new();
		GameServer server = CreateServer("7 Days to Die");
		PrepareGeneratedConfiguration("7 Days to Die", server);
		ConfigurationContext context = CreateContext(server);
		Assert.True(definition.Apply(context).Succeeded);
		string path = definition.ResolveFullPath(server);
		string text = File.ReadAllText(path);
		text = text.Replace(
			"  <property name=\"ServerName\" value=\"Test Server\"/>\r\n",
			string.Empty,
			StringComparison.Ordinal);
		text = text.Replace(
			"  <property name=\"ServerName\" value=\"Test Server\"/>\n",
			string.Empty,
			StringComparison.Ordinal);
		File.WriteAllText(path, text);

		IReadOnlyList<ConfigurationValidationItem> items =
			definition.Validate(context);

		Assert.Contains(items, item =>
			item.Setting == "ServerName" &&
			item.State == ConfigurationValidationState.Failed &&
			item.Message.Contains("missing", StringComparison.OrdinalIgnoreCase));
	}

	[Fact]
	public void ConfigurationValidationReport_DoesNotExposePasswordValues()
	{
		ConfigurationValidationReport report = new(
			"Test Game",
			1,
			2,
			true,
			[
				new ConfigurationValidationItem(
					ConfigurationValidationState.Failed,
					"ServerPassword",
					"The file value does not match the value saved in Synix.")
			]);

		string text = report.ToPlainText();

		Assert.Contains("ServerPassword", text, StringComparison.Ordinal);
		Assert.DoesNotContain("server-secret", text, StringComparison.Ordinal);
		Assert.DoesNotContain("admin-secret", text, StringComparison.Ordinal);
		Assert.DoesNotContain("rcon-secret", text, StringComparison.Ordinal);
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
	public void HumanitZ_MapsFriendlyModeAndRconToBooleanConfigurationValues()
	{
		HumanitZConfiguration definition = new();
		GameServer server = CreateServer("HumanitZ");
		server.GameMode = "PVE";
		server.EnableRcon = false;
		server.RconPort = 27020;

		ConfigurationApplyResult created = definition.Apply(CreateContext(server));
		string path = definition.ResolveFullPath(server);

		Assert.True(created.Succeeded, created.Message);
		Assert.Equal("false", GetValue(path, definition.Format, "PVP").ToLowerInvariant());
		Assert.Equal("false", GetValue(path, definition.Format, "RCONEnabled").ToLowerInvariant());

		server.GameMode = "PVP";
		server.EnableRcon = true;
		ConfigurationApplyResult updated = definition.Apply(CreateContext(server));

		Assert.True(updated.Succeeded, updated.Message);
		Assert.Equal("true", GetValue(path, definition.Format, "PVP").ToLowerInvariant());
		Assert.Equal("true", GetValue(path, definition.Format, "RCONEnabled").ToLowerInvariant());
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
		File.AppendAllText(settingsPath, "\nCustomSetting=True\n");

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
	public void TemplateConfiguration_UpdatesManagedValuesAndPreservesOtherSettings()
	{
		SubsistenceConfiguration definition = new();
		GameServer server = CreateServer("Subsistence");
		server.ServerName = "First Name";
		server.MaxPlayers = 10;
		ConfigurationContext firstContext = CreateContext(server);

		Assert.True(definition.Apply(firstContext).Succeeded);
		string settingsPath = definition.ResolveFullPath(server);
		File.AppendAllText(settingsPath, "\nCustomSetting=True\n");

		server.ServerName = "Second Name";
		server.MaxPlayers = 24;
		ConfigurationApplyResult updated = definition.Apply(CreateContext(server));

		Assert.True(updated.Succeeded, updated.Message);
		Assert.True(updated.Complete, updated.Message);
		Assert.True(updated.Changed);
		Assert.Equal("Second Name", GetValue(
			settingsPath,
			definition.Format,
			"ServerName"));
		Assert.Equal("24", GetValue(
			settingsPath,
			definition.Format,
			"MaxPlayers"));
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

	[Theory]
	[MemberData(nameof(ManagedGameNames))]
	public void CompleteConfigurationTemplates_CanResetExistingFiles(string gameName)
	{
		Assert.True(GameFix.TryGetConfiguration(gameName, out ConfigurationDefinition? definition));
		Assert.NotNull(definition);

		if (gameName is "Soulmask" or "ASTRONEER")
		{
			Assert.False(definition.SupportsFullReset);
			return;
		}

		Assert.True(definition.SupportsFullReset);
		GameServer server = CreateServer(gameName);
		PrepareGeneratedConfiguration(gameName, server);

		ConfigurationContext context = CreateContext(server);
		ConfigurationApplyResult created = definition.Apply(context);
		Assert.True(created.Succeeded, created.Message);

		string primaryPath = definition.ResolveFullPath(server);
		File.WriteAllText(primaryPath, "manually broken config");
		ConfigurationApplyResult reset = definition.ResetToTemplate(context);

		Assert.True(reset.Succeeded, reset.Message);
		Assert.True(reset.Complete, reset.Message);
		Assert.True(reset.Changed);
		Assert.True(File.Exists(primaryPath));
		Assert.True(File.Exists(primaryPath + ".synix.bak"));
		Assert.Equal("manually broken config", File.ReadAllText(primaryPath + ".synix.bak"));
		Assert.DoesNotContain("manually broken config", File.ReadAllText(primaryPath));
	}

	[Fact]
	public void ConfigurationReset_ReappliesSavedServerValuesAndRemovesManualValues()
	{
		MinecraftConfiguration definition = new();
		GameServer server = CreateServer("Minecraft");
		server.ServerName = "Before Reset";
		server.MaxPlayers = 10;
		ConfigurationContext firstContext = CreateContext(server);

		Assert.True(definition.Apply(firstContext).Succeeded);
		string path = definition.ResolveFullPath(server);
		SetValue(path, definition.Format, "difficulty", "hard");
		server.ServerName = "After Reset";
		server.MaxPlayers = 24;

		ConfigurationApplyResult reset = definition.ResetToTemplate(CreateContext(server));

		Assert.True(reset.Succeeded, reset.Message);
		Assert.Equal("After Reset", GetValue(path, definition.Format, "motd"));
		Assert.Equal("24", GetValue(path, definition.Format, "max-players"));
		Assert.Equal("easy", GetValue(path, definition.Format, "difficulty"));
		Assert.Contains("difficulty=hard", File.ReadAllText(path + ".synix.bak"));
	}

	[Fact]
	public void MultiFileConfigurationReset_RebuildsEveryTemplateFile()
	{
		SubsistenceConfiguration definition = new();
		GameServer server = CreateServer("Subsistence");
		PrepareGeneratedConfiguration("Subsistence", server);
		ConfigurationContext context = CreateContext(server);

		Assert.True(definition.Apply(context).Succeeded);
		string settingsPath = definition.ResolveFullPath(server);
		string enginePath = Path.Combine(
			server.InstallPath,
			@"UDKGame\Config\UDKEngine.ini");
		File.WriteAllText(settingsPath, "broken-settings");
		File.WriteAllText(enginePath, "broken-engine");

		ConfigurationApplyResult reset = definition.ResetToTemplate(context);

		Assert.True(reset.Succeeded, reset.Message);
		Assert.DoesNotContain("broken-settings", File.ReadAllText(settingsPath));
		Assert.DoesNotContain("broken-engine", File.ReadAllText(enginePath));
		Assert.Equal("broken-settings", File.ReadAllText(settingsPath + ".synix.bak"));
		Assert.Equal("broken-engine", File.ReadAllText(enginePath + ".synix.bak"));
		Assert.Equal(server.ServerName, GetValue(settingsPath, definition.Format, "ServerName"));
		ConfigLine[] managedPorts = ConfigHandler.LoadConfig(enginePath, definition.Format)
			.Where(value => value.Key == "Port")
			.ToArray();
		Assert.NotEmpty(managedPorts);
		Assert.All(managedPorts, value => Assert.Equal(server.Port.ToString(), value.Value));
		Assert.Equal(server.QueryPort.ToString(), GetValue(enginePath, definition.Format, "QueryPort"));
	}

	[Fact]
	public void GeneratedConfigurationReset_ReappliesSavedServerValues()
	{
		RFactor2Configuration definition = new();
		GameServer server = CreateServer("rFactor 2");
		PrepareGeneratedConfiguration("rFactor 2", server);
		ConfigurationContext context = CreateContext(server);

		Assert.True(definition.Apply(context).Succeeded);
		string path = definition.ResolveFullPath(server);
		File.WriteAllText(path, "broken-generated-config");

		ConfigurationApplyResult reset = definition.ResetToTemplate(context);

		Assert.True(reset.Succeeded, reset.Message);
		Assert.True(reset.Complete, reset.Message);
		Assert.Equal(server.Port.ToString(), GetValue(path, definition.Format, "Simulation Port"));
		Assert.Equal(server.QueryPort.ToString(), GetValue(path, definition.Format, "HTTP Server Port"));
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

	[Theory]
	[InlineData("ARK: Survival Ascended", 350)]
	[InlineData("ARK: Survival Evolved", 200)]
	public void ArkWorkingTemplates_CreateCompleteManagedConfigurations(
		string gameName,
		int minimumLineCount)
	{
		Assert.Equal(
			ConfigFileCreationMode.SynixTemplate,
			GameFix.GetConfigFileCreationMode(gameName));
		Assert.True(GameFix.TryGetConfiguration(
			gameName,
			out ConfigurationDefinition? definition));
		Assert.IsType<EmbeddedTemplateConfigurationDefinition>(definition);
		Assert.True(definition.SupportsFullReset);
		AssertInput(definition.SupportedInputs, ManagedConfigurationInput.ServerPassword);
		AssertInput(definition.SupportedInputs, ManagedConfigurationInput.AdminPassword);
		AssertInput(definition.SupportedInputs, ManagedConfigurationInput.MaxPlayers);
		AssertInput(definition.SupportedInputs, ManagedConfigurationInput.GameMode);
		AssertInput(definition.SupportedInputs, ManagedConfigurationInput.Rcon);

		GameServer server = CreateServer(gameName);
		server.ServerName = "Synix ARK Server";
		server.MaxPlayers = 42;
		server.RconPort = 28020;
		server.EnableRcon = true;
		server.GameMode = "PVE";

		ConfigurationApplyResult result = definition.Apply(CreateContext(server));

		Assert.True(result.Succeeded, result.Message);
		Assert.True(result.Complete, result.Message);
		Assert.True(result.Created);
		string path = definition.ResolveFullPath(server);
		string content = File.ReadAllText(path);
		Assert.True(File.ReadLines(path).Count() >= minimumLineCount);
		Assert.Contains("[ServerSettings]", content);
		Assert.Contains("[SessionSettings]", content);
		Assert.Contains("[/Script/Engine.GameSession]", content);
		Assert.Equal("Synix ARK Server", GetValue(path, definition.Format, "SessionName"));
		Assert.Equal("server-secret", GetValue(path, definition.Format, "ServerPassword"));
		Assert.Equal("admin-secret", GetValue(path, definition.Format, "ServerAdminPassword"));
		Assert.Equal("42", GetValue(path, definition.Format, "MaxPlayers"));
		Assert.Equal("28020", GetValue(path, definition.Format, "RCONPort"));
		Assert.Equal("True", GetValue(path, definition.Format, "RCONEnabled"));
		Assert.Equal("True", GetValue(path, definition.Format, "ServerPVE"));
		Assert.False(definition.NeedsStructuralRepair(CreateContext(server)));
	}

	[Fact]
	public void CraftopiaTemplate_CreatesTheCompleteConfigurationAndAppliesSavedValues()
	{
		Assert.Equal(
			ConfigFileCreationMode.SynixTemplate,
			GameFix.GetConfigFileCreationMode("Craftopia"));
		Assert.True(GameFix.TryGetConfiguration(
			"Craftopia",
			out ConfigurationDefinition? definition));
		Assert.IsType<EmbeddedTemplateConfigurationDefinition>(definition);
		Assert.True(definition.SupportsFullReset);
		AssertInput(definition.SupportedInputs, ManagedConfigurationInput.ServerPassword);
		AssertInput(definition.SupportedInputs, ManagedConfigurationInput.MaxPlayers);
		AssertInput(definition.SupportedInputs, ManagedConfigurationInput.WorldName);
		AssertInput(definition.SupportedInputs, ManagedConfigurationInput.Port);

		GameServer server = CreateServer("Craftopia");
		server.WorldName = "Synix Craftopia World";
		server.MaxPlayers = 12;
		server.Port = 7000;
		ConfigurationContext protectedContext = new(
			server,
			new SynixServerPasswords("12345678", string.Empty, string.Empty),
			Core.Instance.GetSafeName(server.ServerName),
			string.Empty,
			string.Empty);

		ConfigurationApplyResult created = definition.Apply(protectedContext);

		Assert.True(created.Succeeded, created.Message);
		Assert.True(created.Complete, created.Message);
		Assert.True(created.Created);
		string path = definition.ResolveFullPath(server);
		string content = File.ReadAllText(path);
		Assert.Contains("[GameWorld]", content);
		Assert.Contains("[Host]", content);
		Assert.Contains("[Graphics]", content);
		Assert.Contains("[Save]", content);
		Assert.Contains("[CreativeModeSetting]", content);
		Assert.Contains("[CreativeModePlStatus]", content);
		Assert.Equal("Synix Craftopia World", GetValue(path, definition.Format, "name"));
		Assert.Equal("7000", GetValue(path, definition.Format, "port"));
		Assert.Equal("12", GetValue(path, definition.Format, "maxPlayerNumber"));
		Assert.Equal("1", GetValue(path, definition.Format, "usePassword"));
		Assert.Equal("12345678", GetValue(path, definition.Format, "serverPassword"));

		ConfigurationContext publicContext = new(
			server,
			new SynixServerPasswords(string.Empty, string.Empty, string.Empty),
			Core.Instance.GetSafeName(server.ServerName),
			string.Empty,
			string.Empty);
		ConfigurationApplyResult reset = definition.ResetToTemplate(publicContext);

		Assert.True(reset.Succeeded, reset.Message);
		Assert.Equal("0", GetValue(path, definition.Format, "usePassword"));
		Assert.Equal(string.Empty, GetValue(path, definition.Format, "serverPassword"));
	}

	[Fact]
	public void ManagedConfigurations_CanOnlyBeDisabledForDevelopmentBuilds()
	{
		Assert.False(GameFix.ShouldUseManagedConfigurations(false, true));
		Assert.True(GameFix.ShouldUseManagedConfigurations(false, false));
		Assert.True(GameFix.ShouldUseManagedConfigurations(true, true));
	}

	[Fact]
	public void TemplateConfigurations_ReportInputsUsedByTheirTemplates()
	{
		Assert.True(GameFix.TryGetConfiguration(
			"Arma 3",
			out ConfigurationDefinition? definition));
		Assert.NotNull(definition);

		ManagedConfigurationInput inputs = definition.SupportedInputs;
		AssertInput(inputs, ManagedConfigurationInput.ServerPassword);
		AssertInput(inputs, ManagedConfigurationInput.AdminPassword);
		AssertInput(inputs, ManagedConfigurationInput.MaxPlayers);
		Assert.Equal(
			ManagedConfigurationInput.None,
			inputs & ManagedConfigurationInput.QueryPort);
	}

	[Fact]
	public void ConfigurationOnlyInputs_AreReportedWithoutLaunchArgumentTags()
	{
		GameInfo? game = GameDatabase.GetGame("Subsistence");
		Assert.NotNull(game);
		Assert.DoesNotContain("{MaxPlayers}", game.RequiredArgs);
		Assert.DoesNotContain("{port}", game.RequiredArgs);
		Assert.DoesNotContain("{query}", game.RequiredArgs);

		ManagedConfigurationInput inputs =
			GameFix.GetManagedConfigurationInputs("Subsistence");
		AssertInput(
			inputs,
			ManagedConfigurationInput.MaxPlayers);
		AssertInput(
			inputs,
			ManagedConfigurationInput.Port);
		AssertInput(
			inputs,
			ManagedConfigurationInput.QueryPort);
	}

	[Fact]
	public void KeyManagedConfigurations_ReportInputsUsedByTheirBindings()
	{
		Assert.True(GameFix.TryGetConfiguration(
			"Minecraft",
			out ConfigurationDefinition? minecraft));
		Assert.True(GameFix.TryGetConfiguration(
			"Arma Reforger",
			out ConfigurationDefinition? reforger));
		Assert.NotNull(minecraft);
		Assert.NotNull(reforger);

		AssertInput(minecraft.SupportedInputs, ManagedConfigurationInput.WorldSeed);
		AssertInput(minecraft.SupportedInputs, ManagedConfigurationInput.WorldName);
		AssertInput(minecraft.SupportedInputs, ManagedConfigurationInput.Rcon);
		AssertInput(reforger.SupportedInputs, ManagedConfigurationInput.ServerPassword);
		AssertInput(reforger.SupportedInputs, ManagedConfigurationInput.AdminPassword);
		AssertInput(reforger.SupportedInputs, ManagedConfigurationInput.Port);
	}

	[Fact]
	public void ArmaReforger_UsesManagedConfigAndProfileFolders()
	{
		GameInfo? game = GameDatabase.GetGame("Arma Reforger");

		Assert.NotNull(game);
		Assert.Contains("-config \".\\configs\\{map}\"", game.RequiredArgs);
		Assert.Contains("-profile \".\\profiles\\{Identity}\"", game.RequiredArgs);
		Assert.Contains("-maxFPS 60", game.RequiredArgs);
	}

	[Fact]
	public void ArmaReforger_RepairsDisabledFieldPlaceholders()
	{
		ArmaReforgerConfiguration definition = new();
		GameServer server = CreateServer("Arma Reforger");
		server.Port = 2001;
		server.QueryPort = 17777;

		Assert.True(definition.Apply(CreateContext(server)).Succeeded);
		string path = definition.ResolveFullPath(server);
		SetValue(path, definition.Format, "game.password", "Not Required");
		SetValue(path, definition.Format, "game.passwordAdmin", "Not Required");

		ConfigurationApplyResult result = definition.Apply(
			new ConfigurationContext(
				server,
				new SynixServerPasswords("Not Required", "Not Required", string.Empty),
				Core.Instance.GetSafeName(server.ServerName),
				string.Empty,
				string.Empty));

		Assert.True(result.Succeeded, result.Message);
		Assert.Equal(string.Empty, GetValue(path, definition.Format, "game.password"));
		Assert.Equal(string.Empty, GetValue(path, definition.Format, "game.passwordAdmin"));
		Assert.Equal("2001", GetValue(path, definition.Format, "publicPort"));
		Assert.Equal("17777", GetValue(path, definition.Format, "a2s.port"));
	}

	[Fact]
	public void ArmaReforger_CreatesRunnableBohemiaConfiguration()
	{
		ArmaReforgerConfiguration definition = new();
		GameServer server = CreateServer("Arma Reforger");
		server.ServerName = "Reforger Test";
		server.Port = 2001;
		server.QueryPort = 17777;
		server.MaxPlayers = 64;

		ConfigurationApplyResult result = definition.Apply(CreateContext(server));
		string path = definition.ResolveFullPath(server);
		using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
		JsonElement root = document.RootElement;
		JsonElement game = root.GetProperty("game");

		Assert.True(result.Succeeded, result.Message);
		Assert.Equal("0.0.0.0", root.GetProperty("bindAddress").GetString());
		Assert.Equal(2001, root.GetProperty("bindPort").GetInt32());
		Assert.Equal(string.Empty, root.GetProperty("publicAddress").GetString());
		Assert.Equal(2001, root.GetProperty("publicPort").GetInt32());
		Assert.False(root.TryGetProperty("rcon", out _));
		Assert.Equal("0.0.0.0", root.GetProperty("a2s").GetProperty("address").GetString());
		Assert.Equal(17777, root.GetProperty("a2s").GetProperty("port").GetInt32());
		Assert.Equal("Reforger Test", game.GetProperty("name").GetString());
		Assert.Equal(64, game.GetProperty("maxPlayers").GetInt32());
		Assert.True(game.GetProperty("crossPlatform").GetBoolean());
		Assert.True(game.GetProperty("modsRequiredByDefault").GetBoolean());
		Assert.True(game.GetProperty("gameProperties").GetProperty("fastValidation").GetBoolean());
		Assert.Equal(
			50,
			game.GetProperty("gameProperties")
				.GetProperty("serverMinGrassDistance")
				.GetInt32());
		JsonElement operating = root.GetProperty("operating");
		Assert.True(operating.GetProperty("lobbyPlayerSynchronise").GetBoolean());
		Assert.False(operating.GetProperty("disableCrashReporter").GetBoolean());
		Assert.False(operating.GetProperty("disableServerShutdown").GetBoolean());
		Assert.False(operating.GetProperty("disableAI").GetBoolean());
		Assert.Equal(120, operating.GetProperty("playerSaveTime").GetInt32());
		Assert.Equal(-1, operating.GetProperty("aiLimit").GetInt32());
		Assert.Equal(60, operating.GetProperty("slotReservationTimeout").GetInt32());
		Assert.Equal(
			0,
			operating
				.GetProperty("joinQueue")
				.GetProperty("maxSize")
				.GetInt32());
	}

	[Fact]
	public void ArmaReforger_CreatesAndRemovesCompleteRconConfiguration()
	{
		ArmaReforgerConfiguration definition = new();
		GameServer server = CreateServer("Arma Reforger");
		server.EnableRcon = true;
		server.RconPort = 19999;

		ConfigurationApplyResult enabledResult = definition.Apply(CreateContext(server));
		string path = definition.ResolveFullPath(server);
		using (JsonDocument enabledDocument = JsonDocument.Parse(File.ReadAllText(path)))
		{
			JsonElement rcon = enabledDocument.RootElement.GetProperty("rcon");
			Assert.True(enabledResult.Succeeded, enabledResult.Message);
			Assert.Equal("0.0.0.0", rcon.GetProperty("address").GetString());
			Assert.Equal(19999, rcon.GetProperty("port").GetInt32());
			Assert.Equal("rcon-secret", rcon.GetProperty("password").GetString());
			Assert.Equal(16, rcon.GetProperty("maxClients").GetInt32());
			Assert.Equal("admin", rcon.GetProperty("permission").GetString());
			Assert.Empty(rcon.GetProperty("blacklist").EnumerateArray());
			Assert.Empty(rcon.GetProperty("whitelist").EnumerateArray());
		}

		server.EnableRcon = false;
		ConfigurationApplyResult disabledResult = definition.Apply(CreateContext(server));
		using JsonDocument disabledDocument = JsonDocument.Parse(File.ReadAllText(path));

		Assert.True(disabledResult.Succeeded, disabledResult.Message);
		Assert.False(disabledDocument.RootElement.TryGetProperty("rcon", out _));
	}

	[Fact]
	public void ArmaReforger_RepairCheckUsesTemplateTagsWithoutComparingValues()
	{
		ArmaReforgerConfiguration definition = new();
		GameServer server = CreateServer("Arma Reforger");
		ConfigurationContext context = CreateContext(server);

		Assert.True(definition.Apply(context).Succeeded);
		Assert.False(definition.NeedsStructuralRepair(context));

		string path = definition.ResolveFullPath(server);
		JsonNode root = JsonNode.Parse(File.ReadAllText(path))!;
		root["game"]!["name"] = "A different user value";
		root["customUserSetting"] = true;
		File.WriteAllText(path, root.ToJsonString(new JsonSerializerOptions
		{
			WriteIndented = true
		}));

		Assert.False(definition.NeedsStructuralRepair(context));

		_ = root["game"]!["gameProperties"]!.AsObject()
			.Remove("fastValidation");
		File.WriteAllText(path, root.ToJsonString(new JsonSerializerOptions
		{
			WriteIndented = true
		}));

		Assert.True(definition.NeedsStructuralRepair(context));
	}

	[Fact]
	public void ArmaReforger_RepairCheckDetectsUnreadableAndMissingFiles()
	{
		ArmaReforgerConfiguration definition = new();
		GameServer server = CreateServer("Arma Reforger");
		ConfigurationContext context = CreateContext(server);
		string path = definition.ResolveFullPath(server);

		Assert.True(definition.NeedsStructuralRepair(context));

		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.WriteAllText(path, "{ not valid json");
		Assert.ThrowsAny<Exception>(() => definition.NeedsStructuralRepair(context));
		Assert.True(GameFix.NeedsManagedConfigurationRepair(server));
	}

	[Fact]
	public void ArmaReforger_RejectsAdminPasswordsContainingWhitespace()
	{
		ArmaReforgerConfiguration definition = new();
		GameServer server = CreateServer("Arma Reforger");

		ConfigurationApplyResult result = definition.Apply(
			new ConfigurationContext(
				server,
				new SynixServerPasswords(string.Empty, "not allowed", string.Empty),
				Core.Instance.GetSafeName(server.ServerName),
				string.Empty,
				string.Empty));

		Assert.False(result.Succeeded);
		Assert.Contains("cannot contain spaces", result.Message);
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

	private static void PrepareGeneratedConfiguration(
		string gameName,
		GameServer server)
	{
		if (gameName == "7 Days to Die")
		{
			File.WriteAllText(
				Path.Combine(server.InstallPath, "serverconfig.xml"),
				"""
				<?xml version="1.0"?>
				<ServerSettings>
				  <property name="ServerName" value="Generated"/>
				  <property name="ServerDescription" value="Complete generated layout"/>
				  <property name="ServerPassword" value=""/>
				  <property name="ServerPort" value="26900"/>
				  <property name="ServerMaxPlayerCount" value="8"/>
				  <property name="GameWorld" value="Navezgane"/>
				  <property name="GameName" value="Generated"/>
				  <property name="WorldGenSeed" value="asdf"/>
				  <property name="WorldGenSize" value="6144"/>
				  <property name="EACEnabled" value="true"/>
				</ServerSettings>
				""");
			return;
		}

		if (gameName == "Subsistence")
		{
			string configDirectory = Path.Combine(
				server.InstallPath,
				@"UDKGame\Config");
			Directory.CreateDirectory(configDirectory);
			File.WriteAllText(
				Path.Combine(configDirectory, "UDKDedServerSettings.ini"),
				"""
				[SubDedicatedServer.SubServerConfig]
				ServerName="Generated"
				ServerPassword=""
				AdminPassword=""
				MaxPlayers=32
				ProfileId=1
				HuntersEnabled=true
				Difficulty=normal
				""");
			File.WriteAllText(
				Path.Combine(configDirectory, "UDKEngine.ini"),
				"""
				[URL]
				Port=7777

				[IpDrv.TcpNetDriver]
				Port=7777
				MaxInternetClientRate=10000

				[OnlineSubsystemSteamworks.OnlineSubsystemSteamworks]
				QueryPort=27015
				bEnableSteam=true
				""");
			return;
		}

		if (gameName == "Palworld")
		{
			File.WriteAllText(
				Path.Combine(server.InstallPath, "DefaultPalWorldSettings.ini"),
				"""
				[/Script/Pal.PalGameWorldSettings]
				OptionSettings=(Difficulty=None,bIsPvP=False,ExpRate=1.000000,DayTimeSpeedRate=1.000000,ServerPlayerMaxNum=32,ServerName="Default Palworld Server",AdminPassword="",ServerPassword="",PublicPort=8211,PublicIP="0.0.0.0",RCONEnabled=False,RCONPort=25575,RESTAPIPort=8212)
				""");
			return;
		}

		if (gameName == "Project CARS 2")
		{
			string sampleDirectory = Path.Combine(server.InstallPath, "config_sample");
			Directory.CreateDirectory(sampleDirectory);
			File.WriteAllText(
				Path.Combine(sampleDirectory, "server.cfg"),
				"""
				logLevel : "info"
				eventsLogSize : 10000
				name : "Dedicated Server"
				secure : true
				password : ""
				maxPlayerCount : 64
				bindIP : ""
				steamPort : 8766
				hostPort : 27015
				queryPort : 27016
				sleepWaiting : 50
				sleepActive : 10
				""");
			return;
		}

		if (gameName == "Dysterra")
		{
			string worldDirectory = Path.Combine(
				server.InstallPath,
				@"Dysterra\WorldSettings");
			Directory.CreateDirectory(worldDirectory);
			File.WriteAllText(
				Path.Combine(worldDirectory, "Survival_Landscape_Template.json"),
				"""
				{
				  "WorldName": "Generated",
				  "WorldInfo": "Complete installed template",
				  "Password": "",
				  "MaxPlayers": 16,
				  "ValueOverrides": { "DayLength": 1.0 }
				}
				""");
			return;
		}

		if (gameName == "Mount & Blade II: Bannerlord")
		{
			string nativeDirectory = Path.Combine(
				server.InstallPath,
				@"Modules\Native");
			Directory.CreateDirectory(nativeDirectory);
			File.WriteAllText(
				Path.Combine(nativeDirectory, "ds_config_tdm.txt"),
				"ServerName Generated\nGamePassword none\nAdminPassword none\nMaxNumberOfPlayers 16\nGameType tdm\nMapName mp_tdm_map_001\n");
			return;
		}

		if (gameName == "Ground Branch")
		{
			string configDirectory = Path.Combine(
				server.InstallPath,
				@"GroundBranch\ServerConfig");
			Directory.CreateDirectory(configDirectory);
			File.WriteAllText(
				Path.Combine(configDirectory, "Server.ini"),
				"ServerName=Generated\nServerMOTD=Complete generated layout\nServerPassword=\nMaxPlayers=16\nSpectatorMode=1\n");
			return;
		}

		if (gameName == "Holdfast: Nations At War")
		{
			string configDirectory = Path.Combine(
				server.InstallPath,
				@"Holdfast NaW_Data\StreamingAssets\Config");
			Directory.CreateDirectory(configDirectory);
			File.WriteAllText(
				Path.Combine(configDirectory, "serverConfig_Core.txt"),
				"server_name Generated\nserver_password none\nserver_admin_password none\nserver_port 20101\nsteam_query_port 27015\nmaximum_players 16\nserver_map_rotation FortSchwarz\n");
			return;
		}

		if (gameName == "Windrose")
		{
			string windroseDirectory = Path.Combine(server.InstallPath, "R5");
			Directory.CreateDirectory(windroseDirectory);
			File.WriteAllText(
				Path.Combine(windroseDirectory, "ServerDescription.json"),
				"""
				{
				  "Password": "",
				  "ServerName": "Generated",
				  "MaxPlayerCount": "16",
				  "PersistentServerId": "generated-id",
				  "InviteCode": "abcd1234",
				  "UserSelectedRegion": "",
				  "AutoRestart": true,
				  "UseDirectConnection": false,
				  "DirectConnectionServerPort": "7777"
				}
				""");
			return;
		}

		if (gameName == "Just Cause 3 Multiplayer")
		{
			File.WriteAllText(
				Path.Combine(server.InstallPath, "config.json"),
				"""
				{
				  "announce": true,
				  "description": "Complete generated layout",
				  "host": "0.0.0.0",
				  "httpPort": 4203,
				  "logLevel": "info",
				  "maxPlayers": 20,
				  "name": "Generated",
				  "password": "",
				  "port": 4200,
				  "queryPort": 4201,
				  "steamPort": 4202,
				  "requiredDLC": []
				}
				""");
			return;
		}

		if (gameName == "rFactor 2")
		{
			string playerDirectory = Path.Combine(
				server.InstallPath,
				@"UserData\player");
			Directory.CreateDirectory(playerDirectory);
			File.WriteAllText(
				Path.Combine(playerDirectory, "Multiplayer.json"),
				"""
				{
				  "Simulation Port": 54297,
				  "HTTP Server Port": 64297,
				  "Announce Host": true,
				  "Pause while zero players": true
				}
				""");
			return;
		}

		if (gameName == "Just Cause 2: Multiplayer")
		{
			File.WriteAllText(
				Path.Combine(server.InstallPath, "default_config.lua"),
				"""
				Server =
				{
				    MaxPlayers = 5000,
				    BindIP = "",
				    BindPort = 7777,
				    Timeout = 10000,
				    Name = "JC2-MP Server",
				    Description = "No description available.",
				    Password = "",
				    Announce = true,
				    SyncUpdate = 180,
				    IKnowWhatImDoing = false
				}
				SyncRates =
				{
				    Vehicle = 75,
				    OnFoot = 120,
				    Passenger = 1000,
				    MountedGun = 250,
				    StuntPosition = 350
				}
				Streamer =
				{
				    StreamDistance = 500
				}
				Vehicle =
				{
				    DeathRespawnTime = 10,
				    DeathRemove = false,
				    UnoccupiedRespawnTime = 45,
				    UnoccupiedRemove = false
				}
				Player =
				{
				    SpawnPosition = Vector3( -6550, 209, -3290 )
				}
				Module =
				{
				    MaxErrorCount = 5,
				    ErrorDecrementTime = 500,
				    SendAutorunWhenEmpty = false
				}
				World =
				{
				    Time = 0.0,
				    TimeStep = 1,
				    WeatherSeverity = 0
				}
				""");
			return;
		}

		if (gameName == "Survive the Nights")
		{
			string templateDirectory = Path.Combine(
				server.InstallPath,
				@"STN_Dedicated_Server_Data\StreamingAssets\Config_Template");
			Directory.CreateDirectory(templateDirectory);
			File.WriteAllText(
				Path.Combine(templateDirectory, "ServerConfig.txt"),
				"""
				ServerIP=
				ServerPort=7950
				ServerOwner=
				ServerName="New Private Server"
				ServerPassword=
				WelcomeMessage="Welcome to the server."
				RecurringWelcomeMessage="Welcome to the server."
				ProgressTime=true
				DayCycleInMinutes=45
				TimePersistence=true
				StartingWeather=0
				RandomWeather=true
				NameTagDistance=2
				ShowLoginMessages=true
				ShowDeathMessages=true
				PlayerNutrition=true
				StaminaDrainRate=true
				LootSpawnRate=3
				HordeDifficulty=2
				ZombieAmount=2
				PassiveAiAmount=2
				VehicleSpawnRate=2
				StartingComponentsAmount=2
				ShowInPublicLobby=true
				PvpDisabled=false
				PlayerStartingItems=2574
				SoloDifficulty=2
				""");
			return;
		}

		if (gameName == "Eco")
		{
			string ecoDirectory = Path.Combine(server.InstallPath, "Configs");
			Directory.CreateDirectory(ecoDirectory);
			File.WriteAllText(
				Path.Combine(ecoDirectory, "Network.eco.template"),
				"""
				{
				  "PublicServer": false,
				  "Password": "",
				  "Name": "Generated World",
				  "DetailedDescription": "",
				  "IPAddress": "0.0.0.0",
				  "GameServerPort": 3000,
				  "WebServerPort": 3001,
				  "RconServerPort": 3002,
				  "RconPassword": "",
				  "DefaultSlots": -1,
				  "ReservedSlots": 5,
				  "MaxUsersLoadingAtSameTime": 20,
				  "UPnPEnabled": true
				}
				""");
			return;
		}

		if (gameName == "ASKA")
		{
			File.WriteAllText(
				Path.Combine(server.InstallPath, "server properties.txt"),
				"display name = Generated\nserver name = Generated\npassword =\nsteam game port = 7777\nsteam query port = 27015\nauthentication token = preserved-token\n");
			return;
		}

		if (gameName == "Assetto Corsa Competizione")
		{
			string accDirectory = Path.Combine(server.InstallPath, "cfg");
			Directory.CreateDirectory(accDirectory);
			File.WriteAllText(
				Path.Combine(accDirectory, "settings.json"),
				"""{"serverName":"Generated","password":"","adminPassword":"","maxCarSlots":10,"configVersion":1}""");
			return;
		}

		if (gameName == "Wreckfest")
		{
			File.WriteAllText(
				Path.Combine(server.InstallPath, "initial_server_config.cfg"),
				"server_name=Generated");
			return;
		}

		if (gameName != "ASTRONEER")
		{
			return;
		}

		string directory = Path.Combine(
			server.InstallPath,
			@"Astro\Saved\Config\WindowsServer");
		Directory.CreateDirectory(directory);
		File.WriteAllText(
			Path.Combine(directory, "AstroServerSettings.ini"),
			"PublicIP=0.0.0.0\nOwnerName=GeneratedOwner\nOwnerGuid=0\nDenyUnlistedPlayers=0\n");
		File.WriteAllText(
			Path.Combine(directory, "Engine.ini"),
			"[URL]\nPort=8777\n");
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

	private static void AssertInput(
		ManagedConfigurationInput available,
		ManagedConfigurationInput expected)
	{
		Assert.Equal(expected, available & expected);
	}

	private sealed class EscapingConfiguration : ConfigurationDefinition
	{
		public override string GameName => "Unsafe Test";
		public override string RelativePath => @"..\outside.cfg";
		public override IReadOnlyList<ConfigurationBinding> Bindings => [];
		public override string CreateTemplate(ConfigurationContext context) => "value=test\n";
	}
}
