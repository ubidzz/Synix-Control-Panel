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
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixEngine;
using Synix_Control_Panel.Database;
using System.Drawing;
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

	[Fact]
	public void ReactiveDrop_UsesTheDedicatedConsoleServerAndSafeNetworkDefaults()
	{
		GameInfo game = GameDatabase.GetGame("Alien Swarm: Reactive Drop")!;

		Assert.Equal("srcds_console.exe", game.ExeName);
		Assert.Contains("-ip 0.0.0.0", game.RequiredArgs);
		Assert.Contains("+exec server", game.RequiredArgs);
		Assert.Contains("+map {map}", game.RequiredArgs);
		Assert.Equal(27050, game.Port);
		Assert.Equal(27050, game.QueryPort);
		Assert.Equal("lobby", game.Maps[0]);
	}

	[Fact]
	public void AmericasArmyProvingGrounds_UsesTheInstalledDedicatedServerExecutable()
	{
		GameInfo game = GameDatabase.GetGame("America's Army: Proving Grounds")!;

		Assert.Equal("203300", game.AppID);
		Assert.Equal(@"Binaries\Win32\AAGameServer.exe", game.ExeName);
	}

	[Fact]
	public void AmericanTruckSimulator_UsesAnIsolatedHomeAndRequiresExportedPackages()
	{
		GameInfo game = GameDatabase.GetGame("American Truck Simulator")!;

		Assert.Contains("-homedir \"{InstallPath}\"", game.RequiredArgs);
		Assert.Contains("-server \"server_packages.sii\"", game.RequiredArgs);
		Assert.Contains("-server_cfg \"server_config.sii\"", game.RequiredArgs);
		Assert.Equal(
			["server_packages.sii", "server_packages.dat"],
			game.RequiredLaunchFiles);

		string warning = WarningDatabase.GetWarningText(new GameServer
		{
			Game = game.Game,
			ServerName = "Test"
		});
		Assert.Contains("server_packages.sii", warning);
		Assert.Contains("server_packages.dat", warning);
		Assert.Contains("export_server_packages", warning);
		Assert.Contains("uset g_console \"1\"", warning);
	}

	[Fact]
	public void RequiredLaunchFiles_AreImportedWithoutOverwritingServerFiles()
	{
		string testRoot = Path.Combine(
			Path.GetTempPath(),
			"SynixRequiredLaunchFileTests",
			Guid.NewGuid().ToString("N"));
		string source = Path.Combine(testRoot, "source");
		string serverRoot = Path.Combine(testRoot, "server");
		Directory.CreateDirectory(source);
		File.WriteAllText(Path.Combine(source, "server_packages.sii"), "packages");
		File.WriteAllText(Path.Combine(source, "server_packages.dat"), "map-data");
		File.WriteAllText(Path.Combine(source, "server_config.sii"), "source-config");
		Directory.CreateDirectory(serverRoot);
		File.WriteAllText(Path.Combine(serverRoot, "server_config.sii"), "saved-config");

		try
		{
			GameInfo game = GameDatabase.GetGame("American Truck Simulator")!;
			GameServer server = new()
			{
				Game = game.Game,
				InstallPath = serverRoot,
				ServerName = "Test"
			};

			RequiredLaunchFileResult result = Core.PrepareRequiredLaunchFiles(
				server,
				game,
				[source]);

			Assert.Empty(result.MissingFiles);
			Assert.Equal(2, result.CopiedFiles.Count);
			Assert.Equal(
				"packages",
				File.ReadAllText(Path.Combine(serverRoot, "server_packages.sii")));
			Assert.Equal(
				"map-data",
				File.ReadAllText(Path.Combine(serverRoot, "server_packages.dat")));
			Assert.Equal(
				"saved-config",
				File.ReadAllText(Path.Combine(serverRoot, "server_config.sii")));
		}
		finally
		{
			Directory.Delete(testRoot, true);
		}
	}

	[Fact]
	public void RequiredServerPorts_DoesNotCheckOneSharedPortTwice()
	{
		GameInfo game = new()
		{
			RequiredArgs = "-port {port} -queryport {query}"
		};
		GameServer server = new()
		{
			Port = 27050,
			QueryPort = 27050
		};

		IReadOnlyList<(int Port, string Name)> ports =
			Core.GetRequiredServerPorts(server, game);

		Assert.Single(ports);
		Assert.Equal(27050, ports[0].Port);
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

	[Theory]
	[InlineData("Starting", "Starting")]
	[InlineData("Stopping \\", "Stopping")]
	[InlineData("Installing /", "Installing")]
	[InlineData("Updating --", "Updating")]
	[InlineData("Backing Up |", "Backing Up")]
	[InlineData("Validating", "Validating")]
	[InlineData("Exporting", "Exporting")]
	public void BusyStatusPresentation_RecognizesCurrentAndLegacyBusyText(
		string status,
		string expectedState)
	{
		Assert.True(BusyStatusPresentation.TryGetBusyState(status, out string busyState));
		Assert.Equal(expectedState, busyState);
		Assert.Equal(expectedState, BusyStatusPresentation.GetDisplayStatus(status));
	}

	[Theory]
	[InlineData("Running")]
	[InlineData("Stopped")]
	[InlineData("Crashed")]
	public void BusyStatusPresentation_PreservesNonBusyText(string status)
	{
		Assert.False(BusyStatusPresentation.TryGetBusyState(status, out _));
		Assert.Equal(status, BusyStatusPresentation.GetDisplayStatus(status));
	}

	[Fact]
	public void ApplyServerIcon_ReplacesTheCacheAndUpdatesMatchingServers()
	{
		string testRoot = Path.Combine(
			Path.GetTempPath(),
			"SynixIconTests",
			Guid.NewGuid().ToString("N"));
		string iconPath = Path.Combine(testRoot, "server.png");
		Directory.CreateDirectory(testRoot);
		using (Bitmap bitmap = new(24, 24))
		{
			using Graphics graphics = Graphics.FromImage(bitmap);
			graphics.Clear(Color.Cyan);
			bitmap.Save(iconPath, System.Drawing.Imaging.ImageFormat.Png);
		}

		GameServer first = new()
		{
			Game = "Icon Test Game",
			ServerName = "First"
		};
		GameServer second = new()
		{
			Game = "Icon Test Game",
			ServerName = "Second"
		};
		MainGUI.serverList.Add(first);
		MainGUI.serverList.Add(second);

		try
		{
			Assert.True(Core.ApplyServerIcon(first, iconPath));
			Assert.NotNull(first.DisplayIcon);
			Assert.Same(first.DisplayIcon, second.DisplayIcon);
			Assert.Same(
				first.DisplayIcon,
				MainGUI.ServerIconsCache["Icon Test Game"]);
		}
		finally
		{
			MainGUI.serverList.Remove(first);
			MainGUI.serverList.Remove(second);
			if (MainGUI.ServerIconsCache.Remove(
				"Icon Test Game",
				out Image? refreshedIcon))
			{
				refreshedIcon.Dispose();
			}
			Directory.Delete(testRoot, true);
		}
	}
}
