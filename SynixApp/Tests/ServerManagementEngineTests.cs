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
	public void Atlas_RequiresItsExportedGridBeforeStarting()
	{
		GameInfo game = GameDatabase.GetGame("Atlas")!;

		Assert.Equal(
			[
				@"ShooterGame\ServerGrid.json",
				@"ShooterGame\ServerGrid.ServerOnly.json"
			],
			game.RequiredLaunchFiles);
		Assert.Equal("Synix ATLAS Grid", game.ExternalDataFolderName);
		Assert.Contains("ServerGridEditor", game.LaunchFileSetupInstructions);
		Assert.Contains("redis-server_start.bat", game.LaunchFileSetupInstructions);
		Assert.Contains("127.0.0.1", game.LaunchFileSetupInstructions);

		string warning = WarningDatabase.GetWarningText(new GameServer
		{
			Game = game.Game,
			ServerName = "Test"
		});
		Assert.Contains("ADDITIONAL SETUP FILES REQUIRED", warning);
		Assert.Contains("ServerGrid.ServerOnly.json", warning);
		Assert.Contains("official setup tool", warning);
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
	public void RequiredLaunchFiles_CanImportFlatUserFilesIntoNestedDestinations()
	{
		string testRoot = Path.Combine(
			Path.GetTempPath(),
			"SynixNestedLaunchFileTests",
			Guid.NewGuid().ToString("N"));
		string source = Path.Combine(testRoot, "source");
		string serverRoot = Path.Combine(testRoot, "server");
		Directory.CreateDirectory(source);
		File.WriteAllText(Path.Combine(source, "ServerGrid.json"), "grid");
		File.WriteAllText(
			Path.Combine(source, "ServerGrid.ServerOnly.json"),
			"database");

		try
		{
			GameInfo game = GameDatabase.GetGame("Atlas")!;
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
				"grid",
				File.ReadAllText(Path.Combine(
					serverRoot,
					"ShooterGame",
					"ServerGrid.json")));
			Assert.Equal(
				"database",
				File.ReadAllText(Path.Combine(
					serverRoot,
					"ShooterGame",
					"ServerGrid.ServerOnly.json")));
		}
		finally
		{
			if (Directory.Exists(testRoot))
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

	[Fact]
	public void RequiredServerPorts_UsesManagedConfigurationCapabilities()
	{
		GameInfo astroneer = GameDatabase.GetGame("ASTRONEER")!;
		GameServer server = new()
		{
			Game = astroneer.Game,
			Port = 8778,
			QueryPort = 8777
		};

		IReadOnlyList<(int Port, string Name)> ports =
			Core.GetRequiredServerPorts(server, astroneer);

		Assert.Single(ports);
		Assert.Equal((8778, "game port"), ports[0]);
	}

	[Theory]
	[InlineData(Core.ServerState.Stopped, false)]
	[InlineData(Core.ServerState.Crashed, false)]
	[InlineData(Core.ServerState.Running, true)]
	[InlineData(Core.ServerState.Starting, true)]
	[InlineData(Core.ServerState.Stopping, true)]
	public void ActivePortReservation_OnlyIncludesServersThatCanOwnTheSocket(
		Core.ServerState state,
		bool expected)
	{
		GameServer server = new()
		{
			Status = Core.StatusManager.GetStatus(state)
		};

		Assert.Equal(expected, Core.IsActivePortReservation(server));
	}

	[Theory]
	[InlineData("")]
	[InlineData("-log -port 7777 +maxplayers 20")]
	[InlineData("/Config=server.json -Map=\"My Map\"")]
	[InlineData("-url \"https://example.com/?one=1&two=2\"")]
	[InlineData("-password \"one!two\"")]
	[InlineData("-message \"Hello!\" -other \"Okay!\"")]
	[InlineData("-rates 10% 20%")]
	[InlineData("-XX:+UseG1GC -XX:MaxGCPauseMillis=200 -Dterminal.jline=false")]
	[InlineData("@user_jvm_args.txt @libraries/net/minecraftforge/forge/win_args.txt nogui")]
	[InlineData("-javaagent:\"mods/example agent.jar\" -Dfml.readTimeout=180")]
	public void ExtraArguments_AcceptsNormalGameServerSyntax(string arguments)
	{
		Assert.True(Core.TryValidateExtraArguments(arguments, out string error));
		Assert.Empty(error);
	}

	[Theory]
	[InlineData("arg & shutdown /s /t 0", "'&'")]
	[InlineData("arg && shutdown /s /t 0", "'&&'")]
	[InlineData("arg | powershell", "'|'")]
	[InlineData("arg || powershell", "'||'")]
	[InlineData("arg > overwritten.txt", "'>'")]
	[InlineData("arg < commands.txt", "'<'")]
	[InlineData("arg\r\nshutdown /s /t 0", "line breaks")]
	[InlineData("arg %COMSPEC%", "%VARIABLE%")]
	[InlineData("arg !COMSPEC!", "!VARIABLE!")]
	[InlineData("arg %1", "%VARIABLE%")]
	[InlineData("arg ^& shutdown /s /t 0", "'^'")]
	[InlineData("arg \"unclosed", "unclosed")]
	public void ExtraArguments_BlocksWindowsCommandInjection(
		string arguments,
		string expectedReason)
	{
		Assert.False(Core.TryValidateExtraArguments(arguments, out string error));
		Assert.Contains(expectedReason, error, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void ServerConfiguration_UsesTheDedicatedExtraArgumentValidator()
	{
		GameServer safeServer = new()
		{
			Game = "Test",
			ServerName = "Test",
			ExtraArgs = "/Config=server.json -Map=\"My Map\""
		};
		GameServer unsafeServer = new()
		{
			Game = "Test",
			ServerName = "Test",
			ExtraArgs = "arg & shutdown /s /t 0"
		};

		Assert.True(Core.IsGameServerConfigSafe(safeServer));
		Assert.False(Core.IsGameServerConfigSafe(unsafeServer));
	}

	[Fact]
	public void BatchCommandEscaping_PreservesQuotedValuesAndNeutralizesOperators()
	{
		string escaped = Core.EscapeWindowsBatchCommandLine(
			"-url \"https://example.com/?one=1&two=2\" arg & shutdown /s /t 0 %VALUE%");

		Assert.Contains("\"https://example.com/?one=1&two=2\"", escaped);
		Assert.Contains("arg ^& shutdown", escaped);
		Assert.Contains("%%VALUE%%", escaped);
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
	[InlineData(Core.ServerState.Restoring, "Restoring")]
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
	[InlineData("Restoring /", "Restoring")]
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
