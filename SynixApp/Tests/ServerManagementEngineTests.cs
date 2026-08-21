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
using Synix_Control_Panel.SynixEngine;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class ServerManagementEngineTests
{
	[Fact]
	public void GameDatabase_HasUniqueNamesAndEveryGameCanBeLookedUp()
	{
		IReadOnlyList<GameInfo> games = GameDatabase.GetGameList();
		HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);

		Assert.NotEmpty(games);

		foreach (GameInfo game in games)
		{
			Assert.False(string.IsNullOrWhiteSpace(game.Game));
			Assert.True(names.Add(game.Game), $"Duplicate game name: {game.Game}");
			Assert.Same(game, GameDatabase.GetGame($"  {game.Game}  "));
		}
	}

	[Theory]
	[InlineData("Minecraft", "Minecraft")]
	[InlineData("minecraft java", "Minecraft")]
	[InlineData("  Minecraft Java  ", "Minecraft")]
	[InlineData("  Palworld  ", "Palworld")]
	[InlineData(null, "")]
	public void GameNames_AreNormalizedForCurrentAndOlderSavedServers(
		string? savedName,
		string expectedName)
	{
		Assert.Equal(expectedName, GameDatabase.GetCanonicalGameName(savedName));
	}

	[Fact]
	public void LegacyMinecraftName_UsesTheCurrentDatabaseEntry()
	{
		GameInfo? current = GameDatabase.GetGame("Minecraft");
		GameInfo? legacy = GameDatabase.GetGame("Minecraft Java");

		Assert.NotNull(current);
		Assert.Same(current, legacy);
		Assert.True(GameDatabase.IsMinecraft(" minecraft java "));
	}

	[Theory]
	[InlineData(Core.ServerState.Stopped, "Stopped")]
	[InlineData(Core.ServerState.Running, "Running")]
	[InlineData(Core.ServerState.Starting, "Starting")]
	[InlineData(Core.ServerState.Crashed, "Crashed")]
	[InlineData(Core.ServerState.Stopping, "Stopping")]
	[InlineData(Core.ServerState.Installing, "Installing")]
	[InlineData(Core.ServerState.Updating, "Updating")]
	[InlineData(Core.ServerState.BackingUp, "Backing Up")]
	[InlineData(Core.ServerState.Validating, "Validating")]
	[InlineData(Core.ServerState.Export, "Exporting")]
	public void ServerStates_UseTheExpectedUserFacingText(
		Core.ServerState state,
		string expectedText)
	{
		Assert.Equal(expectedText, Core.StatusManager.GetStatus(state));
	}

	[Fact]
	public void UnknownServerState_UsesSafeFallbackText()
	{
		Assert.Equal("Unknown", Core.StatusManager.GetStatus(999));
	}
}
