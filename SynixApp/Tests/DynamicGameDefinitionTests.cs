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
		Assert.Equal(220, games.Count);
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
}
