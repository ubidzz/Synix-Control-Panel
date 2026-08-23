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
using Synix_Control_Panel.SynixApp.Database.GameDefinitions;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class DynamicGameDefinitionTests
{
	[Fact]
	public void EmbeddedDefinitionsAreLoadedFromTheSynixAssembly()
	{
		GameInfo starRupture = GameDatabase.GetGame("StarRupture")!;
		GameInfo beamMp = GameDatabase.GetGame("BeamMP")!;
		IReadOnlyList<GameInfo> games = GameDatabase.GetGameList();

		Assert.True(starRupture.IsEmbeddedDefinition);
		Assert.True(beamMp.IsEmbeddedDefinition);
		Assert.Equal("3809400", starRupture.AppID);
		Assert.Equal(ServerProbeProtocol.Tcp, beamMp.ProbeProtocol);
		Assert.Equal(228, games.Count);
		Assert.Equal(games.Count, TrustedGameDefinitionCatalog.Packages.Count);
		Assert.All(games, game => Assert.True(game.IsEmbeddedDefinition));
		Assert.Equal(
			Enumerable.Range(0, games.Count),
			games.Select(game => game.CatalogOrder));
		Assert.Equal(
			games.Count,
			games.Select(game => game.DefinitionId).Distinct(StringComparer.OrdinalIgnoreCase).Count());
		Assert.All(
			TrustedGameDefinitionCatalog.Packages,
			package => Assert.EndsWith(".game.json", package.ResourceName));
	}

	[Theory]
	[InlineData("Soulmask", "3017310", 8777, 27016)]
	[InlineData("Rust", "258550", 7777, 28015)]
	[InlineData("Palworld", "2394010", 8211, 8212)]
	[InlineData("Icarus", "2089300", 17777, 27016)]
	public void KnownWorkingGameDefinitionsRemainIntact(
		string gameName,
		string appId,
		int port,
		int queryPort)
	{
		GameInfo game = GameDatabase.GetGame(gameName)!;

		Assert.True(game.IsEmbeddedDefinition);
		Assert.Equal(appId, game.AppID);
		Assert.Equal(port, game.Port);
		Assert.Equal(queryPort, game.QueryPort);
		Assert.False(string.IsNullOrWhiteSpace(game.ExeName));
		Assert.False(string.IsNullOrWhiteSpace(game.RequiredArgs));
	}

	[Theory]
	[InlineData("Atlas", "PVP", "False")]
	[InlineData("Atlas", "PVE", "True")]
	[InlineData("PixARK", "PVP", "False")]
	[InlineData("PixARK", "PVE", "True")]
	[InlineData("Palworld", "PVP", "False")]
	[InlineData("Palworld", "PVE", "True")]
	[InlineData("HumanitZ", "PVP", "false")]
	[InlineData("HumanitZ", "PVE", "true")]
	[InlineData("Longvinter", "PVP", "false")]
	[InlineData("Longvinter", "PVE", "true")]
	[InlineData("Rust", "PVP", "false")]
	[InlineData("Rust", "PVE", "true")]
	[InlineData("Soulmask", "PVP", "pvp")]
	[InlineData("Soulmask", "PVE", "pve")]
	[InlineData("Unturned", "PVP", "PvP")]
	[InlineData("Unturned", "PVE", "PvE")]
	public void FriendlyPvpAndPveChoicesUseEachGamesExactValue(
		string gameName,
		string selectedMode,
		string expectedValue)
	{
		GameInfo game = GameDatabase.GetGame(gameName)!;

		Assert.Equal(expectedValue, GameFix.ResolveGameModeValue(game, selectedMode));
	}

	[Theory]
	[InlineData("HumanitZ")]
	[InlineData("Longvinter")]
	[InlineData("Palworld")]
	public void ManagedPvpGamesKeepFriendlyPvpAndPveChoices(string gameName)
	{
		GameInfo game = GameDatabase.GetGame(gameName)!;

		Assert.Contains("PVP", game.GameModes);
		Assert.Contains("PVE", game.GameModes);
	}

	[Theory]
	[InlineData("Conan Exiles")]
	[InlineData("Dune: Awakening")]
	[InlineData("Myth of Empires")]
	public void UnmanagedModeChoicesAreNotShownAsIfTheyWereApplied(string gameName)
	{
		GameInfo game = GameDatabase.GetGame(gameName)!;

		Assert.Empty(game.GameModes);
		Assert.DoesNotContain("{mode}", game.RequiredArgs);
	}

	[Theory]
	[InlineData("ARK: Survival Ascended", true, "True")]
	[InlineData("ARK: Survival Ascended", false, "False")]
	[InlineData("Rust", true, "true")]
	[InlineData("Rust", false, "false")]
	public void BooleanSettingsUseEachGamesExactValue(
		string gameName,
		bool enabled,
		string expectedValue)
	{
		GameInfo game = GameDatabase.GetGame(gameName)!;

		Assert.Equal(expectedValue, GameFix.ResolveBooleanValue(game, enabled));
	}

	[Fact]
	public void RconDefinitionsUseTheRconPasswordAndCorrectProtocolSyntax()
	{
		IReadOnlyList<EmbeddedGamePackage> packages = TrustedGameDefinitionCatalog.Packages;
		GameInfo rust = packages.Single(package =>
			package.Definition.DefinitionId == "rust").Definition;

		Assert.Equal(
			"+rcon.port {rcon_port} +rcon.password \"{rcon_pass}\" +rcon.web 1",
			rust.RconSyntax);
		Assert.All(
			packages.Where(package =>
				!string.IsNullOrWhiteSpace(package.Definition.RconSyntax)),
			package =>
			{
				Assert.Contains("{rcon}", package.Definition.RequiredArgs);
				Assert.DoesNotContain("{adminpass}", package.Definition.RconSyntax);
			});
		Assert.All(
			packages.Where(package =>
				package.Definition.RconSyntax.StartsWith(
					"+rcon_password",
					StringComparison.Ordinal)),
			package => Assert.Equal(
				"+rcon_password \"{rcon_pass}\"",
				package.Definition.RconSyntax));
	}

	[Fact]
	public void DarkMessiahDefinitionIncludesTransparentFirstStartRequirements()
	{
		GameInfo game = GameDatabase.GetGame(
			"Dark Messiah of Might & Magic Dedicated Server")!;
		GameServer server = new()
		{
			Game = game.Game,
			ServerName = "Test"
		};
		string warning = Synix_Control_Panel.Database.WarningDatabase.GetWarningText(server);

		Assert.Equal("2145", game.AppID);
		Assert.Equal("srcds.exe", game.ExeName);
		Assert.Equal("ctf_3", game.Maps[0]);
		Assert.Equal(["0", "1", "2", "3", "4"], game.GameModes);
		Assert.True(game.NeedsConfigWarning);
		Assert.Contains("cannot download, copy, or redistribute", warning);
		Assert.Contains("must be completed by the user", warning);
	}

	[Fact]
	public void InstalledServersDoNotInheritExecutableGameDefinitions()
	{
		Assert.False(typeof(GameDefinition).IsAssignableFrom(typeof(GameServer)));

		string json = Core.SerializeServersForStorage(
			[new GameServer
			{
				Game = "BeamMP",
				ServerName = "Test",
				InstallPath = @"C:\Synix\Games\BeamMP\Test",
				Port = 30814,
				QueryPort = 30814
			}]);

		Assert.Contains("\"Game\": \"BeamMP\"", json);
		Assert.DoesNotContain("\"AppID\"", json);
		Assert.DoesNotContain("\"ExeName\"", json);
		Assert.DoesNotContain("\"RequiredArgs\"", json);
	}

	[Fact]
	public void EmbeddedTemplatesAutomaticallyBecomeManagedConfigurations()
	{
		Assert.True(GameFix.TryGetConfiguration(
			"StarRupture",
			out ConfigurationDefinition? definition));
		Assert.IsType<EmbeddedTemplateConfigurationDefinition>(definition);

		GameManagementCapability capabilities =
			GameFix.GetManagementCapabilities(GameDatabase.GetGame("StarRupture"));
		Assert.True((capabilities & GameManagementCapability.Port) != 0);
	}

	[Fact]
	public void TemplatePlaceholdersAutomaticallyExposeSettingsCapabilities()
	{
		EmbeddedGamePackage package = TrustedGameDefinitionCatalog.ParsePackage(
			"""
			{
			  "schemaVersion": 1,
			  "id": "capability-test",
			  "game": "Capability Test",
			  "appId": "1",
			  "executable": "server.exe",
			  "arguments": "",
			  "port": 7777,
			  "queryPort": 7778,
			  "configFileCreation": "SynixTemplate",
			  "format": "JSON",
			  "configuration": {
			    "schemaVersion": 1,
			    "templates": [
			      {
			        "relativePath": "config.json",
			        "content": "{ \"maxPlayers\": {MaxPlayers}, \"password\": \"{Password}\" }"
			      }
			    ]
			  }
			}
			""",
			"capability-test.game.json");

		EmbeddedTemplateConfigurationDefinition definition = new(
			package.Definition.Game,
			package.Configuration!);

		Assert.True(
			(definition.SupportedInputs & ManagedConfigurationInput.MaxPlayers) != 0);
		Assert.True(
			(definition.SupportedInputs & ManagedConfigurationInput.ServerPassword) != 0);
	}

	[Fact]
	public void UnsafeDefinitionPathsAreRejected()
	{
		InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
			TrustedGameDefinitionCatalog.ParsePackage(
				"""
				{
				  "schemaVersion": 1,
				  "id": "unsafe-test",
				  "game": "Unsafe Test",
				  "appId": "1",
				  "executable": "..\\outside.exe",
				  "port": 7777,
				  "queryPort": 7778
				}
				""",
				"unsafe-test.game.json"));

		Assert.Contains("unsafe executable path", exception.Message);
	}

	[Fact]
	public void UnknownDefinitionPropertiesAreRejected()
	{
		Assert.Throws<InvalidDataException>(() =>
			TrustedGameDefinitionCatalog.ParsePackage(
				"""
				{
				  "schemaVersion": 1,
				  "id": "unknown-field-test",
				  "game": "Unknown Field Test",
				  "appId": "1",
				  "executable": "server.exe",
				  "port": 7777,
				  "queryPort": 7778,
				  "pluginAssembly": "untrusted.dll"
				}
				""",
				"unknown-field.game.json"));
	}

	[Theory]
	[InlineData("-port {Port}", "")]
	[InlineData("-port {unknown}", "")]
	[InlineData("{rcon}", "+rcon.password {RconPassword}")]
	public void UnsupportedOrIncorrectlyCapitalizedArgumentTagsAreRejected(
		string arguments,
		string rconSyntax)
	{
		InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
			TrustedGameDefinitionCatalog.ParsePackage(
				$$"""
				{
				  "schemaVersion": 1,
				  "definitionRevision": 1,
				  "id": "invalid-tag-test",
				  "catalogOrder": 0,
				  "game": "Invalid Tag Test",
				  "appId": "1",
				  "executable": "server.exe",
				  "arguments": "{{arguments}}",
				  "rconSyntax": "{{rconSyntax}}",
				  "port": 7777,
				  "queryPort": 7778
				}
				""",
				"invalid-tag-test.game.json"));

		Assert.Contains("unsupported or incorrectly capitalized", exception.Message);
	}

	[Fact]
	public void DocumentedRconTagsAreAccepted()
	{
		EmbeddedGamePackage package = TrustedGameDefinitionCatalog.ParsePackage(
			"""
			{
			  "schemaVersion": 1,
			  "definitionRevision": 1,
			  "id": "rcon-tag-test",
			  "catalogOrder": 0,
			  "game": "RCON Tag Test",
			  "appId": "1",
			  "executable": "server.exe",
			  "arguments": "{rcon}",
			  "rconSyntax": "+rcon.port {rcon_port} +rcon.password {rcon_pass} +enabled {rcon_enabled} +admin {adminpass} -SteamAppId={steamAppID}",
			  "port": 7777,
			  "queryPort": 7778
			}
			""",
			"rcon-tag-test.game.json");

		Assert.Equal("RCON Tag Test", package.Definition.Game);
	}

	[Theory]
	[InlineData("{rcon}", "")]
	[InlineData("", "+rcon_password {rcon_pass}")]
	public void RconSyntaxAndLaunchTagMustBeDeclaredTogether(
		string arguments,
		string rconSyntax)
	{
		InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
			TrustedGameDefinitionCatalog.ParsePackage(
				$$"""
				{
				  "schemaVersion": 1,
				  "definitionRevision": 1,
				  "id": "rcon-pair-test",
				  "catalogOrder": 0,
				  "game": "RCON Pair Test",
				  "appId": "1",
				  "executable": "server.exe",
				  "arguments": "{{arguments}}",
				  "rconSyntax": "{{rconSyntax}}",
				  "port": 7777,
				  "queryPort": 7778
				}
				""",
				"rcon-pair-test.game.json"));

		Assert.Contains("rcon", exception.Message, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void UnsafeDefinitionValueMappingsAreRejected()
	{
		InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
			TrustedGameDefinitionCatalog.ParsePackage(
				"""
				{
				  "schemaVersion": 1,
				  "definitionRevision": 1,
				  "id": "unsafe-value-test",
				  "catalogOrder": 0,
				  "game": "Unsafe Value Test",
				  "appId": "1",
				  "executable": "server.exe",
				  "pvpValue": "true&unsafe",
				  "port": 7777,
				  "queryPort": 7778
				}
				""",
				"unsafe-value-test.game.json"));

		Assert.Contains("unsafe pvpValue", exception.Message);
	}

	[Fact]
	public void SharedGoldSrcDefinitionsSelectTheCorrectSteamPackage()
	{
		Dictionary<string, string> expected = new(StringComparer.OrdinalIgnoreCase)
		{
			["Counter-Strike 1.6"] = "90 mod cstrike",
			["Counter-Strike: Condition Zero"] = "90 mod czero",
			["Day of Defeat"] = "90 mod dod",
			["Deathmatch Classic"] = "90 mod dmc",
			["Half-Life: Opposing Force"] = "90 mod gearbox",
			["Ricochet"] = "90 mod ricochet",
			["Team Fortress Classic"] = "90 mod tfc"
		};

		foreach ((string gameName, string appConfig) in expected)
		{
			GameInfo game = GameDatabase.GetGame(gameName)!;
			Assert.Equal("90", game.AppID);
			Assert.Equal("hlds.exe", game.ExeName);
			Assert.Equal(appConfig, game.SteamAppConfig);
			Assert.Equal(ServerProbeProtocol.A2S, game.ProbeProtocol);
		}
	}

	[Fact]
	public void UnsafeSteamAppConfigurationIsRejected()
	{
		InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
			TrustedGameDefinitionCatalog.ParsePackage(
				"""
				{
				  "schemaVersion": 1,
				  "definitionRevision": 1,
				  "id": "unsafe-steam-config",
				  "catalogOrder": 0,
				  "game": "Unsafe Steam Config",
				  "appId": "90",
				  "steamAppConfig": "90 mod cstrike +quit",
				  "executable": "hlds.exe",
				  "port": 27015,
				  "queryPort": 27015
				}
				""",
				"unsafe-steam-config.game.json"));

		Assert.Contains("unsafe steamAppConfig", exception.Message);
	}

	[Fact]
	public void CompleteSourceLibraryPassesDefinitionValidation()
	{
		string? projectDirectory =
			Core.FindProjectDirectory(AppContext.BaseDirectory);
		Assert.NotNull(projectDirectory);

		GameDefinitionValidationReport report =
			GameDefinitionValidator.ValidateSourceDirectory(projectDirectory!);

		Assert.True(report.IsValid, report.ToPlainText());
		Assert.Equal(228, report.DefinitionCount);
		Assert.Equal(4, report.TemplateCount);
		Assert.Equal(62, report.PostInstallActionCount);
	}

	[Fact]
	public void UnsafePostInstallTargetIsRejected()
	{
		InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
			TrustedGameDefinitionCatalog.ParsePackage(
				"""
				{
				  "schemaVersion": 1,
				  "definitionRevision": 1,
				  "id": "unsafe-action-test",
				  "catalogOrder": 0,
				  "game": "Unsafe Action Test",
				  "appId": "1",
				  "executable": "server.exe",
				  "port": 7777,
				  "queryPort": 7778,
				  "postInstallActions": [
				    {
				      "type": "CopySteamRuntimeFiles",
				      "targetDirectory": "..\\outside"
				    }
				  ]
				}
				""",
				"unsafe-action-test.game.json"));

		Assert.Contains("unsafe postInstallActions.targetDirectory path", exception.Message);
	}

	[Fact]
	public void PostInstallActionTypeMustBeExplicitAndAllowlisted()
	{
		InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
			TrustedGameDefinitionCatalog.ParsePackage(
				"""
				{
				  "schemaVersion": 1,
				  "definitionRevision": 1,
				  "id": "missing-action-type",
				  "catalogOrder": 0,
				  "game": "Missing Action Type",
				  "appId": "1",
				  "executable": "server.exe",
				  "port": 7777,
				  "queryPort": 7778,
				  "postInstallActions": [
				    {
				      "targetDirectory": "Binaries/Win64"
				    }
				  ]
				}
				""",
				"missing-action-type.game.json"));

		Assert.Contains("unsupported post-install action type", exception.Message);
	}

	[Fact]
	public void DefinitionBuilderCreatesRevisionedValidatedSourceFiles()
	{
		string root = Path.Combine(
			Path.GetTempPath(),
			$"SynixDefinitionBuilderTests-{Guid.NewGuid():N}");
		Directory.CreateDirectory(root);
		try
		{
			string templateSource = Path.Combine(root, "server.cfg");
			File.WriteAllText(
				templateSource,
				"name={ServerName}\nport={Port}\ncustom=preserved\n");
			GameDefinitionDraft draft = new()
			{
				Id = "builder-test",
				CatalogOrder = GameDefinitionAuthoring.GetNextCatalogOrder(),
				DefinitionRevision = 2,
				Game = "Builder Test",
				AppId = "1",
				Executable = "server.exe",
				Maps = ["Map One", "Map Two"],
				GameModes = ["PVE", "PVP"],
				Port = 7777,
				QueryPort = 7778,
				ConfigFileCreation = ConfigFileCreationMode.SynixTemplate,
				Format = ConfigFormat.StandardINI,
				RelativeConfigPath = "cfg/server.cfg",
				TemplateSourcePath = templateSource,
				ConfigurationRevision = 3,
				ExternalDataFolderName = "Builder Test",
				RequiredLaunchFiles = ["player-data/server.dat"],
				OptionalLaunchFiles = ["player-data/server.cfg"],
				LaunchFileSetupInstructions =
					"Create the files in the normal game, then copy them into the server folder.",
				NeedsConfigWarning = true,
				WarningMessage = "The user must supply files from their own game installation.",
				IconUrl = "https://example.com/game.png",
				CopySteamRuntimeFiles = true,
				SteamRuntimeTargetDirectory = "Binaries/Win64"
			};

			GameDefinitionSaveResult result =
				GameDefinitionAuthoring.SaveDraft(draft, root);
			EmbeddedGamePackage parsed =
				GameDefinitionAuthoring.ValidateDraft(draft);

			Assert.True(File.Exists(result.DefinitionPath));
			Assert.True(File.Exists(result.TemplatePath));
			Assert.Equal(2, parsed.Definition.DefinitionRevision);
			Assert.Equal(3, parsed.Configuration!.Revision);
			Assert.Equal(3, parsed.Configuration.Templates[0].Revision);
			Assert.Equal(["Map One", "Map Two"], parsed.Definition.Maps);
			Assert.Equal(["PVE", "PVP"], parsed.Definition.GameModes);
			Assert.Equal(["player-data/server.dat"], parsed.Definition.RequiredLaunchFiles);
			Assert.Equal(["player-data/server.cfg"], parsed.Definition.OptionalLaunchFiles);
			Assert.Equal("Builder Test", parsed.Definition.ExternalDataFolderName);
			Assert.True(parsed.Definition.NeedsConfigWarning);
			Assert.Equal(
				"The user must supply files from their own game installation.",
				parsed.Definition.WarningMessage);
			Assert.Equal("https://example.com/game.png", parsed.Definition.IconUrl);
			Assert.Single(parsed.PostInstallActions);
			Assert.Contains("\"definitionRevision\": 2", result.Json);
			Assert.Contains("\"pvpValue\": \"PVP\"", result.Json);
			Assert.Contains("\"booleanTrueValue\": \"true\"", result.Json);
		}
		finally
		{
			if (Directory.Exists(root))
				Directory.Delete(root, true);
		}
	}

	[Fact]
	public void DefinitionBuilderRequiresRevisionIncreaseBeforeOverwrite()
	{
		string root = Path.Combine(
			Path.GetTempPath(),
			$"SynixDefinitionRevisionTests-{Guid.NewGuid():N}");
		string definitionDirectory = Path.Combine(
			root,
			"Database",
			"GameDefinitions",
			"revision-test");
		Directory.CreateDirectory(definitionDirectory);
		try
		{
			File.WriteAllText(
				Path.Combine(definitionDirectory, "revision-test.game.json"),
				"""
				{
				  "schemaVersion": 1,
				  "definitionRevision": 2,
				  "id": "revision-test",
				  "catalogOrder": 220,
				  "game": "Revision Test",
				  "appId": "1",
				  "executable": "server.exe",
				  "port": 7777,
				  "queryPort": 7778
				}
				""");
			GameDefinitionDraft draft = new()
			{
				Id = "revision-test",
				CatalogOrder = GameDefinitionAuthoring.GetNextCatalogOrder(),
				DefinitionRevision = 2,
				Game = "Revision Test",
				AppId = "1",
				Executable = "server.exe",
				Port = 7777,
				QueryPort = 7778
			};

			InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
				GameDefinitionAuthoring.SaveDraft(draft, root));

			Assert.Contains("Increase definitionRevision above 2", exception.Message);
		}
		finally
		{
			if (Directory.Exists(root))
				Directory.Delete(root, true);
		}
	}

	[Fact]
	public void DefinitionBuilderSavesMultipleConfigurationTemplates()
	{
		string root = Path.Combine(
			Path.GetTempPath(),
			$"SynixMultiTemplateBuilderTests-{Guid.NewGuid():N}");
		Directory.CreateDirectory(root);
		try
		{
			string primarySource = Path.Combine(root, "server.cfg");
			string secondarySource = Path.Combine(root, "mapcycle.txt");
			File.WriteAllText(primarySource, "hostname={ServerName}\nport={Port}\n");
			File.WriteAllText(secondarySource, "map_one\nmap_two\n");
			GameDefinitionDraft draft = new()
			{
				Id = "multi-template-builder-test",
				CatalogOrder = GameDefinitionAuthoring.GetNextCatalogOrder(),
				Game = "Multi Template Builder Test",
				AppId = "2",
				Executable = "server.exe",
				Port = 27015,
				QueryPort = 27016,
				ConfigFileCreation = ConfigFileCreationMode.SynixTemplate,
				RelativeConfigPath = "game/cfg/server.cfg",
				TemplateSourcePath = primarySource,
				AdditionalTemplates =
				[
					new GameDefinitionTemplateDraft(
						"game/cfg/mapcycle.txt",
						secondarySource)
				],
				ConfigurationRevision = 2
			};

			GameDefinitionSaveResult result =
				GameDefinitionAuthoring.SaveDraft(draft, root);
			EmbeddedGamePackage parsed =
				GameDefinitionAuthoring.ValidateDraft(draft);

			Assert.Equal(2, parsed.Configuration!.Templates.Count);
			Assert.Equal(2, result.TemplatePaths.Count);
			Assert.All(result.TemplatePaths, path => Assert.True(File.Exists(path)));
			Assert.Contains(
				parsed.Configuration.Templates,
				template => template.RelativePath == "game/cfg/mapcycle.txt");
		}
		finally
		{
			if (Directory.Exists(root))
				Directory.Delete(root, true);
		}
	}

	[Fact]
	public void TrustedPostInstallActionsCopyOnlyAllowlistedSteamFiles()
	{
		string root = Path.Combine(
			Path.GetTempPath(),
			$"SynixPostInstallTests-{Guid.NewGuid():N}");
		string install = Path.Combine(root, "install");
		string steam = Path.Combine(root, "steamcmd");
		Directory.CreateDirectory(install);
		Directory.CreateDirectory(steam);
		try
		{
			File.WriteAllText(Path.Combine(steam, "steamclient64.dll"), "trusted-test-file");
			File.WriteAllText(Path.Combine(steam, "not-allowlisted.dll"), "must-not-copy");
			GameServer server = new()
			{
				Game = "StarRupture",
				ServerName = "Test",
				InstallPath = install,
				Port = 7777,
				QueryPort = 7778
			};

			TrustedPostInstallExecutionResult result =
				TrustedPostInstallExecutor.Execute(server, steam);
			string target = Path.Combine(install, "StarRupture", "Binaries", "Win64");

			Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Messages));
			Assert.True(result.Changed);
			Assert.True(File.Exists(Path.Combine(target, "steamclient64.dll")));
			Assert.False(File.Exists(Path.Combine(target, "not-allowlisted.dll")));
		}
		finally
		{
			if (Directory.Exists(root))
				Directory.Delete(root, true);
		}
	}

	[Fact]
	public void TemplateRevisionUpgradePreservesBackupAndCustomSettings()
	{
		EmbeddedGamePackage package = TrustedGameDefinitionCatalog.ParsePackage(
			"""
			{
			  "schemaVersion": 1,
			  "definitionRevision": 1,
			  "id": "upgrade-test",
			  "catalogOrder": 0,
			  "game": "Upgrade Test",
			  "appId": "1",
			  "executable": "server.exe",
			  "port": 7777,
			  "queryPort": 7778,
			  "configFileCreation": "SynixTemplate",
			  "relativeConfigPath": "server.cfg",
			  "format": "StandardINI",
			  "configuration": {
			    "schemaVersion": 1,
			    "revision": 2,
			    "templates": [
			      {
			        "revision": 2,
			        "relativePath": "server.cfg",
			        "content": "name={ServerName}\nport={Port}\n"
			      }
			    ]
			  }
			}
			""",
			"upgrade-test.game.json");
		string root = Path.Combine(
			Path.GetTempPath(),
			$"SynixTemplateUpgradeTests-{Guid.NewGuid():N}");
		Directory.CreateDirectory(root);
		try
		{
			EmbeddedTemplateConfigurationDefinition definition = new(
				package.Definition.Game,
				package.Configuration!);
			GameServer server = new()
			{
				Game = "Upgrade Test",
				ServerName = "Before Upgrade",
				InstallPath = root,
				Port = 7777,
				QueryPort = 7778,
				ManagedConfigurationVersion = 0
			};
			ConfigurationContext first = new(
				server,
				default,
				"upgrade-test",
				string.Empty,
				string.Empty);
			Assert.True(definition.Apply(first).Succeeded);
			string configPath = Path.Combine(root, "server.cfg");
			File.AppendAllText(configPath, "custom=user-value\n");
			server.ServerName = "After Upgrade";
			server.ManagedConfigurationVersion = 1;

			ConfigurationApplyResult upgraded = definition.Apply(new ConfigurationContext(
				server,
				default,
				"upgrade-test",
				string.Empty,
				string.Empty));
			string backupPath = configPath + ".synix.before-template-v2.bak";

			Assert.True(upgraded.Succeeded, upgraded.Message);
			Assert.True(File.Exists(backupPath));
			Assert.Contains("name=Before Upgrade", File.ReadAllText(backupPath));
			Assert.Contains("custom=user-value", File.ReadAllText(configPath));
			Assert.Contains("name=After Upgrade", File.ReadAllText(configPath));
		}
		finally
		{
			if (Directory.Exists(root))
				Directory.Delete(root, true);
		}
	}
}
