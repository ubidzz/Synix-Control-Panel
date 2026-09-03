// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class GameServerInputValidatorTests
{
	[Fact]
	public void ValheimRejectsPasswordsShorterThanFiveCharacters()
	{
		GameInfo definition = GameDatabase.GetGame("Valheim")!;

		bool valid = GameServerInputValidator.TryValidate(
			definition,
			"Dedicated Server",
			new SynixServerPasswords("123", string.Empty, string.Empty),
			out string error);

		Assert.False(valid);
		Assert.Contains("at least 5 characters", error);
	}

	[Fact]
	public void ValheimRejectsPasswordContainedInServerName()
	{
		GameInfo definition = GameDatabase.GetGame("Valheim")!;

		bool valid = GameServerInputValidator.TryValidate(
			definition,
			"My VikingSecret Realm",
			new SynixServerPasswords("secret", string.Empty, string.Empty),
			out string error);

		Assert.False(valid);
		Assert.Contains("appear in the server name", error);
	}

	[Fact]
	public void ValheimAcceptsAValidIndependentPassword()
	{
		GameInfo definition = GameDatabase.GetGame("Valheim")!;

		bool valid = GameServerInputValidator.TryValidate(
			definition,
			"Dedicated Server",
			new SynixServerPasswords("VikingSecret", string.Empty, string.Empty),
			out string error);

		Assert.True(valid, error);
		Assert.Empty(error);
	}

	[Fact]
	public void GamesWithoutDeclaredRulesKeepExistingPasswordBehavior()
	{
		GameInfo definition = GameDatabase.GetGame("Rust")!;

		bool valid = GameServerInputValidator.TryValidate(
			definition,
			"123",
			new SynixServerPasswords("123", string.Empty, string.Empty),
			out string error);

		Assert.True(valid, error);
		Assert.Empty(error);
	}
}
