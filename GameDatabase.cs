using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;

namespace Game_Server_Control_Panel
{
	// The "Blueprint" - Define what every game needs
	public class GameInfo
	{
		public string Game { get; set; } = string.Empty;
		[JsonIgnore]
		public string AppID { get; set; } = string.Empty;
		[JsonIgnore]
		public string ExeName { get; set; } = string.Empty;
		[JsonIgnore]
		public string RequiredArgs { get; set; } = string.Empty;
		public int Port { get; set; }
		public int QueryPort { get; set; }
		[JsonIgnore]
		public List<string> Maps { get; set; } = [];

		// This is a "Default" field for new servers
		public string ExtraArgs { get; set; } = string.Empty;
	}

	// The "Instance" - This is what actually gets saved to your JSON
	public class GameServer : GameInfo
	{
		public string InstallPath { get; set; } = string.Empty;
		public string ServerName { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string AdminPassword { get; set; } = string.Empty;
		public string Status { get; set; } = "Stopped";
		public int MaxPlayers { get; set; } = 10;
		public string WorldName { get; set; } = "NewWorld";
		public bool IsDefaultPath { get; set; } = true;
		public int? PID { get; set; }

		[JsonIgnore]
		public Process? RunningProcess { get; set; }
	}

	public static class GameDatabase
	{
		// Use ReadOnly to protect the master list
		private static readonly List<GameInfo> games =
		[
			new()
			{
				Game = "StarRupture",
				AppID = "3333140",
				ExeName = @"StarRupture\Binaries\Win64\StarRuptureServerEOS-Win64-Shipping.exe",
				RequiredArgs = "-server -log -port={port} -password={pass} -name=\"{ServerName}\"",
				Port = 7777,
				QueryPort = 27015,
				Maps = ["MainWorld"]
			},
			new()
			{
				Game = "Soulmask",
				AppID = "3017310",
				ExeName = @"WS\Binaries\Win64\WSServer-Win64-Shipping.exe",
				RequiredArgs = "{map} -server -log -NOSTEAM -SteamAppId={appid} -Port={port} -QueryPort={query} -PSW=\"{pass}\" -adminpsw=\"{adminpass}\" -MaxPlayers={MaxPlayers} -SteamServerName=\"{ServerName}\" -forcepassthrough",
				Port = 8777,
				QueryPort = 27015,
				Maps = ["Level01_Main"]
			},
			new()
			{
				Game = "7 Days to Die",
				AppID = "294420",
				ExeName = "7DaysToDieServer.exe",
				RequiredArgs = "-configfile=serverconfig.xml -port={port} -quit -batchmode -nographics",
				Port = 26900,
				QueryPort = 26900,
				Maps = ["Navezgane", "Pregen01"]
			},
			new()
			{
				Game = "Rust",
				AppID = "258550",
				ExeName = "RustDedicated.exe",
				RequiredArgs = "-batchmode +server.port {port} +server.queryport {query} +server.hostname \"{ServerName}\"",
				Port = 28015,
				QueryPort = 28016,
				Maps = ["Procedural Map"]
			},
			new()
			{
				Game = "Valheim",
				AppID = "896660",
				ExeName = "valheim_server.exe",
				RequiredArgs = "-nographics -batchmode -name \"{ServerName}\" -port {port} -world \"{map}\" -password \"{pass}\"",
				Port = 2456,
				QueryPort = 2457,
				Maps = ["Dedicated"]
			},
			new()
			{
				Game = "Palworld",
				AppID = "2394010",
				ExeName = "PalServer.exe",
				RequiredArgs = "-port={port} -queryport={query} -AdminPassword=\"{pass}\"",
				Port = 8211,
				QueryPort = 27015,
				Maps = ["DefaultWorld"]
			},
            // --- NEWLY ADDED GAMES BELOW ---
            new()
			{
				Game = "ARK: Survival Evolved",
				AppID = "376030",
				ExeName = @"ShooterGame\Binaries\Win64\ShooterGameServer.exe",
				RequiredArgs = "{map}?Listen?SessionName=\"{ServerName}\"?ServerPassword=\"{pass}\"?ServerAdminPassword=\"{adminpass}\"?Port={port}?QueryPort={query} -server -log",
				Port = 7777,
				QueryPort = 27015,
				Maps = ["TheIsland", "ScorchedEarth_P", "Aberration_P", "Extinction", "Genesis", "Ragnarok"]
			},
			new()
			{
				Game = "ARK: Survival Ascended",
				AppID = "2430930",
				ExeName = @"ShooterGame\Binaries\Win64\ArkAscendedServer.exe",
				RequiredArgs = "{map}?Listen?SessionName=\"{ServerName}\"?ServerPassword=\"{pass}\"?ServerAdminPassword=\"{adminpass}\"?Port={port}?QueryPort={query} -server -log",
				Port = 7777,
				QueryPort = 27015,
				Maps = ["TheIsland_WP", "ScorchedEarth_WP"]
			},
			new()
			{
				Game = "Conan Exiles",
				AppID = "443030",
				ExeName = @"ConanSandbox\Binaries\Win64\ConanSandboxServer.exe",
				RequiredArgs = "-log -MaxPlayers={MaxPlayers} -ServerName=\"{ServerName}\" -ServerPassword=\"{pass}\" -Port={port} -QueryPort={query}",
				Port = 7777,
				QueryPort = 27015,
				Maps = ["ConanSandbox", "Camp"]
			},
			new()
			{
				Game = "DayZ",
				AppID = "223350",
				ExeName = "DayZServer_x64.exe",
				RequiredArgs = "-config=serverDZ.cfg -port={port} -name=\"{ServerName}\"",
				Port = 2302,
				QueryPort = 27016,
				Maps = ["chernarusplus", "enoch"]
			},
			new()
			{
				Game = "Project Zomboid",
				AppID = "380870",
				ExeName = "ProjectZomboid64.exe",
				RequiredArgs = "-servername \"{ServerName}\" -adminpassword \"{adminpass}\" -port {port}",
				Port = 16261,
				QueryPort = 16262,
				Maps = ["Muldraugh, KY"]
			},
			new()
			{
				Game = "Garry's Mod",
				AppID = "4020",
				ExeName = "srcds.exe",
				RequiredArgs = "-game garrysmod -console -port {port} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				Maps = ["gm_construct", "gm_flatgrass"]
			},
			new()
			{
				Game = "V Rising",
				AppID = "1828900",
				ExeName = "VRisingServer.exe",
				RequiredArgs = "-persistentDataPath .\\save-data -serverName \"{ServerName}\" -saveName \"{map}\" -logLevel \"info\"",
				Port = 9876,
				QueryPort = 9877,
				Maps = ["world1"]
			},
			new()
			{
				Game = "Satisfactory",
				AppID = "1690800",
				ExeName = @"FactoryGame\Binaries\Win64\FactoryServer-Win64-Shipping.exe",
				RequiredArgs = "-log -unattended -ServerQueryPort={query} -multihome=0.0.0.0 -port={port}",
				Port = 7777,
				QueryPort = 15777,
				Maps = ["Satisfactory"]
			},
			new()
			{
				Game = "Sons Of The Forest",
				AppID = "2465200",
				ExeName = "SonsOfTheForestDS.exe",
				RequiredArgs = "-userdatapath \"config\" -servername \"{ServerName}\" -serverpassword \"{pass}\" -serverport {port} -queryport {query}",
				Port = 8766,
				QueryPort = 27016,
				Maps = ["Default"]
			},
			new()
			{
				Game = "Enshrouded",
				AppID = "2278520",
				ExeName = "enshrouded_server.exe",
                // Enshrouded uses a JSON config file (enshrouded_server.json) mostly instead of launch args
                RequiredArgs = "",
				Port = 15636,
				QueryPort = 15637,
				Maps = ["Embervale"]
			}
		];

		public static List<GameInfo> GetGameList() => games;

		public static GameInfo? GetGame(string gameName)
		{
			return games.FirstOrDefault(g => g.Game.Equals(gameName, StringComparison.OrdinalIgnoreCase));
		}
	}
}