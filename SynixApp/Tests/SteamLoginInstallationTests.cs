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
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixApp.SteamCMDHandler;
using Synix_Control_Panel.SynixEngine;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class SteamLoginInstallationTests
{
	[Fact]
	public void GameDatabaseMarksEveryAuthenticatedSteamInstallation()
	{
		Dictionary<string, string> expected = new(StringComparer.Ordinal)
		{
			["Arma 3"] = "233780",
			["Assetto Corsa"] = "302550",
			["Chivalry: Deadliest Warrior"] = "258680",
			["Darkest Hour: Europe '44-'45"] = "1290",
			["DayZ"] = "223350",
			["Dino D-Day"] = "70010",
			["E.Y.E: Divine Cybermancy"] = "91720",
			["Killing Floor"] = "215350",
			["Monday Night Combat"] = "63220",
			["Painkiller: Hell & Damnation"] = "230030",
			["Project CARS 2"] = "413770",
			["Red Orchestra 2: Heroes of Stalingrad"] = "212542",
			["Serious Sam HD: The First Encounter"] = "41005",
			["Takedown: Red Sabre"] = "261020",
			["Terraria"] = "105610",
			["The Haunted: Hell's Reach"] = "43210"
		};
		Dictionary<string, string> actual = GameDatabase.GetGames
			.Where(game => game.RequiresSteamLogin)
			.ToDictionary(
				game => game.Game,
				game => game.AppID,
				StringComparer.Ordinal);

		Assert.Equal(
			expected.OrderBy(pair => pair.Key),
			actual.OrderBy(pair => pair.Key));
	}

	[Fact]
	public void AuthenticatedSteamInstallUsesVisibleSecurePrompt()
	{
		GameInfo game = GameDatabase.GetGame("Arma 3")!;
		GameServer server = new()
		{
			InstallPath = @"C:\Synix\Games\Arma_3\Test",
			SteamAccountName = "server_account",
			Password = "game-server-password",
			AdminPassword = "game-admin-password"
		};

		var startInfo = ServerInstaller.CreateSteamProcessStartInfo(server, game);
		string arguments = string.Join(" ", startInfo.ArgumentList);

		Assert.False(startInfo.CreateNoWindow);
		Assert.False(startInfo.RedirectStandardOutput);
		Assert.False(startInfo.RedirectStandardError);
		Assert.Contains("server_account", startInfo.ArgumentList);
		Assert.DoesNotContain("anonymous", startInfo.ArgumentList);
		Assert.DoesNotContain(server.Password, arguments);
		Assert.DoesNotContain(server.AdminPassword, arguments);
	}

	[Fact]
	public void AuthenticatedSteamInstallRejectsMissingAccountName()
	{
		GameInfo game = GameDatabase.GetGame("Arma 3")!;
		GameServer server = new()
		{
			InstallPath = @"C:\Synix\Games\Arma_3\Test"
		};

		Assert.Throws<InvalidOperationException>(() =>
			ServerInstaller.CreateSteamProcessStartInfo(server, game));
	}

	[Fact]
	public void AnonymousGamesKeepSilentInstallation()
	{
		GameInfo game = GameDatabase.GetGame("Palworld")!;
		GameServer server = new()
		{
			InstallPath = @"C:\Synix\Games\Palworld\Test"
		};

		var startInfo = ServerInstaller.CreateSteamProcessStartInfo(server, game);

		Assert.True(startInfo.CreateNoWindow);
		Assert.True(startInfo.RedirectStandardOutput);
		Assert.True(startInfo.RedirectStandardError);
		Assert.Contains("anonymous", startInfo.ArgumentList);
	}

	[Fact]
	public void LimitedSteamDownloadAddsSessionOnlyThrottleBeforeUpdate()
	{
		GameInfo game = GameDatabase.GetGame("Palworld")!;
		GameServer server = new()
		{
			InstallPath = @"C:\Synix\Games\Palworld\Test"
		};

		var startInfo = ServerInstaller.CreateSteamProcessStartInfo(
			server,
			game,
			ServerInstaller.ConvertDownloadLimitToKbps(25));
		List<string> arguments = startInfo.ArgumentList.ToList();
		int throttleIndex = arguments.IndexOf("+set_download_throttle");
		int updateIndex = arguments.IndexOf("+app_update");

		Assert.True(throttleIndex >= 0);
		Assert.Equal("25000", arguments[throttleIndex + 1]);
		Assert.Equal("false", arguments[throttleIndex + 2]);
		Assert.True(throttleIndex < updateIndex);
	}

	[Fact]
	public void UnlimitedSteamDownloadDoesNotAddThrottle()
	{
		GameInfo game = GameDatabase.GetGame("Palworld")!;
		GameServer server = new()
		{
			InstallPath = @"C:\Synix\Games\Palworld\Test"
		};

		var startInfo = ServerInstaller.CreateSteamProcessStartInfo(
			server,
			game,
			downloadThrottleKbps: null);

		Assert.DoesNotContain(
			"+set_download_throttle",
			startInfo.ArgumentList);
	}

	[Theory]
	[InlineData(0, 1000)]
	[InlineData(25, 25000)]
	[InlineData(10001, 10000000)]
	public void DownloadLimitConversionStaysInsideSupportedUiRange(
		int megabitsPerSecond,
		int expectedKbps)
	{
		Assert.Equal(
			expectedKbps,
			ServerInstaller.ConvertDownloadLimitToKbps(megabitsPerSecond));
	}

	[Fact]
	public void ImportedServerAuthorizationUsesLoginWithoutReinstalling()
	{
		GameInfo game = GameDatabase.GetGame("Arma 3")!;
		GameServer server = new()
		{
			InstallPath = @"C:\Synix\Games\Arma_3\Test",
			SteamAccountName = "server_account",
			Password = "game-server-password",
			AdminPassword = "game-admin-password",
			SteamAuthenticationRequired = true
		};

		var startInfo = ServerInstaller.CreateSteamAuthenticationStartInfo(
			server,
			game);
		string arguments = string.Join(" ", startInfo.ArgumentList);

		Assert.False(startInfo.CreateNoWindow);
		Assert.False(startInfo.RedirectStandardOutput);
		Assert.False(startInfo.RedirectStandardError);
		Assert.Contains("+login", startInfo.ArgumentList);
		Assert.Contains("server_account", startInfo.ArgumentList);
		Assert.Contains("+quit", startInfo.ArgumentList);
		Assert.DoesNotContain("+app_update", startInfo.ArgumentList);
		Assert.DoesNotContain(server.InstallPath, arguments);
		Assert.DoesNotContain(server.Password, arguments);
		Assert.DoesNotContain(server.AdminPassword, arguments);
	}

	[Fact]
	public void ImportMarksOnlyGamesThatRequireSteamAuthentication()
	{
		GameServer authenticatedServer = new()
		{
			Game = "Arma 3"
		};
		GameServer anonymousServer = new()
		{
			Game = "Palworld",
			SteamAuthenticationRequired = true
		};

		int authenticationCount = Core.MarkImportedSteamAuthenticationRequired(
			[authenticatedServer, anonymousServer]);

		Assert.Equal(1, authenticationCount);
		Assert.True(authenticatedServer.SteamAuthenticationRequired);
		Assert.False(anonymousServer.SteamAuthenticationRequired);
	}
}
