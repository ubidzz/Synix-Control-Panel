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
		// 'init' locks these down. They can be set ONCE in the GameDatabase, and never changed again.
		public string Game { get; init; } = string.Empty;

		[JsonIgnore]
		public string AppID { get; init; } = string.Empty;

		[JsonIgnore]
		public string ExeName { get; init; } = string.Empty;

		[JsonIgnore]
		public string RequiredArgs { get; init; } = string.Empty;

		[JsonIgnore]
		public List<string> Maps { get; init; } = [];

		// 'set' keeps these flexible because the user can type new ports or args in the UI
		public int Port { get; set; }
		public int QueryPort { get; set; }
		public string ExtraArgs { get; set; } = string.Empty;
	}

	// The "Instance" - This remains exactly the same! 
	// Because GameServer inherits from GameInfo, it respects the 'init' locks on the base fields.
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
		private static readonly IReadOnlyList<GameInfo> games =
		[
			new()
			{
				Game = "StarRupture",
				AppID = "3333140",
				ExeName = @"StarRupture\Binaries\Win64\StarRuptureServerEOS-Win64-Shipping.exe",
				RequiredArgs = "{map}?Listen -server -log -port={port} -password={pass} -name=\"{ServerName}\"",
				Port = 7777,
				QueryPort = 27015,
				Maps = ["MainWorld"]
			},
			new()
			{
				Game = "Soulmask",
				AppID = "3017310",
				ExeName = @"WS\Binaries\Win64\WSServer-Win64-Shipping.exe",
				RequiredArgs = "{map} -server -log -online=Steam -SteamAppId={appid} -Port={port} -QueryPort={query} -PSW=\"{pass}\" -adminpsw=\"{adminpass}\" -MaxPlayers={MaxPlayers} -SteamServerName=\"{ServerName}\" -forcepassthrough",
				Port = 8777,
				QueryPort = 27015,
				Maps = ["Level01_Main"]
			},
			new()
			{
				Game = "7 Days to Die",
				AppID = "294420",
				ExeName = "7DaysToDieServer.exe",
				RequiredArgs = "-configfile=serverconfig.xml -batchmode -nographics -dedicated",
				Port = 26900,
				QueryPort = 26900,
				Maps = ["Navezgane", "Pregen01"]
			},
			new()
			{
				Game = "Rust",
				AppID = "258550",
				ExeName = "RustDedicated.exe",
				RequiredArgs = "-batchmode +server.level \"{map}\" +server.port {port} +server.queryport {query} +server.hostname \"{ServerName}\"",
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
				RequiredArgs = "-port={port} -publicqueryport={query} -ServerName=\"{ServerName}\" -Password=\"{pass}\" -AdminPassword=\"{adminpass}\"",
				Port = 8211,
				QueryPort = 27015,
				Maps = ["DefaultWorld"]
			},
			new()
			{
				Game = "ARK: Survival Evolved",
				AppID = "376030",
				ExeName = @"ShooterGame\Binaries\Win64\ShooterGameServer.exe",
				RequiredArgs = "{map}?Listen?SessionName=\"{ServerName}\"?ServerPassword=\"{pass}\"?ServerAdminPassword=\"{adminpass}\"?Port={port}?QueryPort={query}?MaxPlayers={MaxPlayers} -server -log",
				Port = 7777,
				QueryPort = 27015,
				Maps = ["TheIsland", "ScorchedEarth_P", "Aberration_P", "Extinction", "Genesis", "Ragnarok"]
			},
			new()
			{
				Game = "ARK: Survival Ascended",
				AppID = "2430930",
				ExeName = @"ShooterGame\Binaries\Win64\ArkAscendedServer.exe",
				RequiredArgs = "{map}?Listen?SessionName=\"{ServerName}\"?ServerPassword=\"{pass}\"?ServerAdminPassword=\"{adminpass}\"?Port={port}?QueryPort={query}?MaxPlayers={MaxPlayers} -server -log",
				Port = 7777,
				QueryPort = 27015,
				Maps = ["TheIsland_WP", "ScorchedEarth_WP"]
			},
			new()
			{
				Game = "Conan Exiles",
				AppID = "443030",
				ExeName = @"ConanSandbox\Binaries\Win64\ConanSandboxServer.exe",
				RequiredArgs = "{map}?Listen?MaxPlayers={MaxPlayers}?Port={port}?QueryPort={query} -server -log -ServerName=\"{ServerName}\" -ServerPassword=\"{pass}\"",
				Port = 7777,
				QueryPort = 27015,
				Maps = ["ConanSandbox", "Camp"]
			},
			new()
			{
				Game = "DayZ",
				AppID = "223350",
				ExeName = "DayZServer_x64.exe",
				RequiredArgs = "-config=serverDZ.cfg -port={port} -name=\"{ServerName}\" -profiles=profiles -dologs -adminlog",
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
				RequiredArgs = "-persistentDataPath .\\save-data -serverName \"{ServerName}\" -saveName \"{map}\" -logLevel \"info\" -port {port} -queryPort {query}",
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
				RequiredArgs = "",
				Port = 15636,
				QueryPort = 15637,
				Maps = ["Embervale"]
			},
			new()
			{
				Game = "Factorio",
				AppID = "428200",
				ExeName = @"bin\x64\factorio.exe",
				RequiredArgs = "--start-server {map}.zip --server-settings data\\server-settings.json --port {port}",
				Port = 34197,
				QueryPort = 34197,
				Maps = ["FactorioWorld"]
			},
			new()
			{
				Game = "Unturned",
				AppID = "1110390",
				ExeName = "Unturned.exe",
				RequiredArgs = "-batchmode -nographics +InternetServer/{ServerName} +map {map} -port {port} -password {pass}",
				Port = 27015,
				QueryPort = 27016,
				Maps = ["PEI", "Washington", "Russia", "Germany", "Hawaii"]
			},
			new()
			{
				Game = "Space Engineers",
				AppID = "298740",
				ExeName = @"DedicatedServer64\SpaceEngineersDedicated.exe",
				RequiredArgs = "-noconsole -ignorelastsession -port {port}",
				Port = 27016,
				QueryPort = 27016,
				Maps = ["StarSystem", "AlienPlanet", "EmptyWorld"]
			},
			new()
			{
				Game = "Arma 3",
				AppID = "233780",
				ExeName = "arma3server.exe",
				RequiredArgs = "-port={port} -name=\"{ServerName}\" -config=server.cfg -world={map}",
				Port = 2302,
				QueryPort = 2303,
				Maps = ["empty", "Altis", "Stratis", "Tanoa", "Malden"]
			},
			new()
			{
				Game = "Core Keeper",
				AppID = "1963720",
				ExeName = "CoreKeeperServer.exe",
				RequiredArgs = "-world {map} -worldname \"{ServerName}\" -maxplayers {MaxPlayers} -port {port}",
				Port = 27015,
				QueryPort = 27016,
				Maps = ["0", "1", "2"]
			},
			new()
			{
				Game = "Terraria",
				AppID = "105600",
				ExeName = "TerrariaServer.exe",
				RequiredArgs = "-port {port} -maxplayers {MaxPlayers} -world \"{map}\" -password \"{pass}\" -motd \"{ServerName}\"",
				Port = 7777,
				QueryPort = 7777,
				Maps = ["World1.wld"]
			},
			new()
			{
				Game = "Counter-Strike 2",
				AppID = "730",
				ExeName = @"game\bin\win64\cs2.exe",
				RequiredArgs = "-dedicated +map {map} -port {port} -maxplayers {MaxPlayers} +sv_password \"{pass}\" +hostname \"{ServerName}\"",
				Port = 27015,
				QueryPort = 27015,
				Maps = ["de_dust2", "de_inferno", "de_mirage", "de_nuke", "de_vertigo"]
			},
			new()
			{
				Game = "Team Fortress 2",
				AppID = "232250",
				ExeName = "srcds.exe",
				RequiredArgs = "-game tf -console -port {port} +maxplayers {MaxPlayers} +map {map} +sv_password \"{pass}\" +hostname \"{ServerName}\"",
				Port = 27015,
				QueryPort = 27015,
				Maps = ["ctf_2fort", "pl_upward", "cp_dustbowl", "koth_harvest"]
			},
			new()
			{
				Game = "Left 4 Dead 2",
				AppID = "222860",
				ExeName = "srcds.exe",
				RequiredArgs = "-game left4dead2 -console -port {port} +maxplayers {MaxPlayers} +map {map} +hostname \"{ServerName}\"",
				Port = 27015,
				QueryPort = 27015,
				Maps = ["c1m1_hotel", "c2m1_highway", "c8m1_apartment", "c14m1_junkyard"]
			},
			new()
			{
				Game = "Squad",
				AppID = "403240",
				ExeName = "SquadGameServer.exe",
				RequiredArgs = "Port={port} QueryPort={query} FIXEDMAXPLAYERS={MaxPlayers}",
				Port = 7787,
				QueryPort = 27165,
				Maps = ["Mutaha_AAS_v1", "Gorodok_RAAS_v1", "Fallujah_AAS_v1"]
			},
			new()
			{
				Game = "Insurgency: Sandstorm",
				AppID = "581330",
				ExeName = @"Insurgency\Binaries\Win64\InsurgencyServer-Win64-Shipping.exe",
				RequiredArgs = "{map}?Scenario={map}?MaxPlayers={MaxPlayers} -port={port} -queryport={query} -hostname=\"{ServerName}\" -password=\"{pass}\"",
				Port = 27102,
				QueryPort = 27131,
				Maps = ["Refinery", "Farmhouse", "Hideout", "Crossing"]
			},
			new()
			{
				Game = "Stationeers",
				AppID = "600760",
				ExeName = "rocketstation_DedicatedServer.exe",
				RequiredArgs = "-batchmode -nographics -autostart -loadlatest {map} -settings StartLocalHost true ServerVisible true ServerMaxPlayers {MaxPlayers} ServerPort {port} ServerName \"{ServerName}\" ServerPassword \"{pass}\"",
				Port = 27016,
				QueryPort = 27015,
				Maps = ["Moon", "Mars", "Europa", "Mimas"]
			},
			new()
			{
				Game = "Empyrion - Galactic Survival",
				AppID = "530870",
				ExeName = "EmpyrionDedicated.cmd",
				RequiredArgs = "-batchmode -nographics -logFile Logs\\current.log",
				Port = 30000,
				QueryPort = 30001,
				Maps = ["Default Multiplayer"]
			},
			new()
			{
				Game = "Stormworks: Build and Rescue",
				AppID = "1247090",
				ExeName = "server64.exe",
				RequiredArgs = "+server_name \"{ServerName}\" +port {port} +password \"{pass}\" +max_players {MaxPlayers}",
				Port = 25564,
				QueryPort = 25564,
				Maps = ["Default"]
			},
			new()
			{
				Game = "The Forest",
				AppID = "556450",
				ExeName = "TheForestDedicatedServer.exe",
				RequiredArgs = "-batchmode -nographics -savefolderpath \"{map}\" -serverip 0.0.0.0 -serverport {port} -serverplayers {MaxPlayers} -serverpassword \"{pass}\" -serverpasswordadmin \"{adminpass}\"",
				Port = 27015,
				QueryPort = 27016,
				Maps = ["Saves"]
			},
			new()
			{
				Game = "Astroneer",
				AppID = "533830",
				ExeName = @"Astro\Binaries\Win64\AstroServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port}",
				Port = 8777,
				QueryPort = 8777,
				Maps = ["Sylva"]
			},
			new()
			{
				Game = "Barotrauma",
				AppID = "1022710",
				ExeName = "DedicatedServer.exe",
				RequiredArgs = "-port {port} -queryport {query} -name \"{ServerName}\"",
				Port = 27015,
				QueryPort = 27016,
				Maps = ["Campaign"]
			},
			new()
			{
				Game = "Myth of Empires",
				AppID = "1371580",
				ExeName = @"MOE\Binaries\Win64\MOEServer-Win64-Shipping.exe",
				RequiredArgs = "{map}?Listen -server -log -Port={port} -QueryPort={query} -ServerName=\"{ServerName}\" -ServerPassword=\"{pass}\"",
				Port = 7777,
				QueryPort = 27015,
				Maps = ["ZhongZhou", "DongZhou"]
			},
			new()
			{
				Game = "Mount & Blade II: Bannerlord",
				AppID = "1863440",
				ExeName = @"bin\Win64_Shipping_Server\Bannerlord.DedicatedServer.exe",
				RequiredArgs = "_MODULES_*Native*Multiplayer*_MODULES_ /dedicatedcustomserverconfigfile {map} /port {port}",
				Port = 7230,
				QueryPort = 7230,
				Maps = ["CustomServerconfig.txt"]
			},
			new()
			{
				Game = "Abiotic Factor",
				AppID = "2816220",
				ExeName = @"AbioticFactor\Binaries\Win64\AbioticFactorServer-Win64-Shipping.exe",
				RequiredArgs = "{map}?Listen -log -MaxPlayers={MaxPlayers} -Port={port} -QueryPort={query} -ServerPassword=\"{pass}\"",
				Port = 7777,
				QueryPort = 27015,
				Maps = ["Cascade"]
			},
			new()
			{
				Game = "Icarus",
				AppID = "2089300",
				ExeName = @"Icarus\Binaries\Win64\IcarusServer-Win64-Shipping.exe",
				RequiredArgs = "-Log -ServerName=\"{ServerName}\" -Password=\"{pass}\" -Port={port} -QueryPort={query}",
				Port = 17777,
				QueryPort = 27015,
				Maps = ["Olympus", "Styx", "Prometheus"]
			},
			new()
			{
				Game = "Don't Starve Together",
				AppID = "343050",
				ExeName = @"bin\dontstarve_dedicated_server_nullrenderer.exe",
				RequiredArgs = "-console -cluster \"{ServerName}\" -shard {map}",
				Port = 10999,
				QueryPort = 27016,
				Maps = ["Master", "Caves"]
			},
			new()
			{
				Game = "Arma Reforger",
				AppID = "1874900",
				ExeName = "ArmaReforgerServer.exe",
				RequiredArgs = "-config \"{map}\" -profile \"profile\" -maxPlayers {MaxPlayers} -port {port}",
				Port = 2001,
				QueryPort = 2001,
				Maps = ["server.json"]
			},
			new()
			{
				Game = "Killing Floor 2",
				AppID = "232130",
				ExeName = @"Binaries\Win64\KFServer.exe",
				RequiredArgs = "{map}?Game=KFGameContent.KFGameInfo_Survival?AdminPassword=\"{adminpass}\"?GamePassword=\"{pass}\" -Port={port}",
				Port = 7777,
				QueryPort = 27015,
				Maps = ["KF-BioticsLab", "KF-BurningParis", "KF-Outpost", "KF-ZedLanding"]
			},
			new()
			{
				Game = "The Front",
				AppID = "2568660",
				ExeName = @"ProjectWar\Binaries\Win64\TheFrontServer-Win64-Shipping.exe",
				RequiredArgs = "{map}?Listen?MaxPlayers={MaxPlayers}?ServerName=\"{ServerName}\"?ServerPassword=\"{pass}\"?ServerAdminPassword=\"{adminpass}\"?Port={port}?QueryPort={query} -server -log",
				Port = 7777,
				QueryPort = 27015,
				Maps = ["TheFront"]
			},
			new()
			{
				Game = "Smalland: Survive the Wilds",
				AppID = "2404090",
				ExeName = @"SMALLAND\Binaries\Win64\SMALLANDServer-Win64-Shipping.exe",
				RequiredArgs = "{map}?Listen -log -ServerName=\"{ServerName}\" -Password=\"{pass}\" -MaxPlayers={MaxPlayers} -Port={port}",
				Port = 7777,
				QueryPort = 27015,
				Maps = ["Smalland"]
			},
			new()
			{
				Game = "Sunkenland",
				AppID = "2605310",
				ExeName = "Sunkenland Dedicated Server.exe",
				RequiredArgs = "-batchmode -nographics -serverName \"{ServerName}\" -password \"{pass}\" -port {port} -maxPlayers {MaxPlayers}",
				Port = 27015,
				QueryPort = 27015,
				Maps = ["World1"]
			},
			new()
			{
				Game = "Risk of Rain 2",
				AppID = "1180760",
				ExeName = "Risk of Rain 2 Dedicated Server.exe",
				RequiredArgs = "-batchmode -nographics -server_port {port} -server_query_port {query}",
				Port = 27015,
				QueryPort = 27016,
				Maps = ["Default"]
			},
			new()
			{
				Game = "Avorion",
				AppID = "565060",
				ExeName = @"bin\AvorionServer.exe",
				RequiredArgs = "--galaxy-name \"{map}\" --server-name \"{ServerName}\" --admin \"{adminpass}\" --port {port} --use-steam-networking 1",
				Port = 27000,
				QueryPort = 27003,
				Maps = ["galaxy"]
			},
			new()
			{
				Game = "PixARK",
				AppID = "824360",
				ExeName = @"ShooterGame\Binaries\Win64\PixARKServer.exe",
				RequiredArgs = "{map}?Listen?SessionName=\"{ServerName}\"?ServerPassword=\"{pass}\"?ServerAdminPassword=\"{adminpass}\"?Port={port}?QueryPort={query} -server -log",
				Port = 7777,
				QueryPort = 27015,
				Maps = ["CubeWorld_Light"]
			},
			new()
			{
				Game = "Atlas",
				AppID = "1006030",
				ExeName = @"ShooterGame\Binaries\Win64\ShooterGameServer.exe",
				RequiredArgs = "Ocean?ServerX=0?ServerY=0?AltSaveDirectoryName=\"{map}\"?ServerAdminPassword=\"{adminpass}\"?MaxPlayers={MaxPlayers}?Port={port}?QueryPort={query}?SeamlessIP=0.0.0.0 -log -server",
				Port = 57555,
				QueryPort = 57555,
				Maps = ["00"]
			},
			new()
			{
				Game = "SCUM",
				AppID = "683000",
				ExeName = @"SCUM\Binaries\Win64\SCUMServer-Win64-Shipping.exe",
				RequiredArgs = "Port={port} QueryPort={query} -ServerName=\"{ServerName}\" -ServerPassword=\"{pass}\" -log",
				Port = 7042,
				QueryPort = 7043,
				Maps = ["SCUM_Island"]
			},
			new()
			{
				Game = "Eco",
				AppID = "739590",
				ExeName = "EcoServer.exe",
				RequiredArgs = "-nogui",
				Port = 3000,
				QueryPort = 3001,
				Maps = ["DefaultWorld"]
			},
			new()
			{
				Game = "Mordhau",
				AppID = "629800",
				ExeName = @"Mordhau\Binaries\Win64\MordhauServer-Win64-Shipping.exe",
				RequiredArgs = "{map}?Listen -log -port={port} -queryport={query}",
				Port = 7777,
				QueryPort = 27015,
				Maps = ["ThePit", "Camp", "Crossroads"]
			},
			new()
			{
				Game = "Hell Let Loose",
				AppID = "1062090",
				ExeName = @"HLL\Binaries\Win64\HLLServer-Win64-Shipping.exe",
				RequiredArgs = "-port={port} -queryport={query} -log",
				Port = 8211,
				QueryPort = 27015,
				Maps = ["SainteMarieDuMont_Warfare"]
			},
			new()
			{
				Game = "SCP: Secret Laboratory",
				AppID = "996560",
				ExeName = "LocalAdmin.exe",
				RequiredArgs = "-port {port}",
				Port = 7777,
				QueryPort = 7777,
				Maps = ["Facility"]
			},
			new()
			{
				Game = "Starbound",
				AppID = "211820",
				ExeName = @"win64\starbound_server.exe",
				RequiredArgs = "",
				Port = 21025,
				QueryPort = 21025,
				Maps = ["Universe"]
			},
			new()
			{
				Game = "Wurm Unlimited",
				AppID = "402370",
				ExeName = "WurmServerLauncher-64.exe",
				RequiredArgs = "{map}",
				Port = 3724,
				QueryPort = 27016,
				Maps = ["Adventure"]
			},
			new()
			{
				Game = "Nightingale",
				AppID = "2445990",
				ExeName = @"NWX\Binaries\Win64\NWXServer-Win64-Shipping.exe",
				RequiredArgs = "{map}?Listen -log -ServerName=\"{ServerName}\" -Password=\"{pass}\" -Port={port} -QueryPort={query}",
				Port = 7777,
				QueryPort = 27015,
				Maps = ["SylvanGlade"]
			},
			new()
			{
				Game = "Holdfast: Nations At War",
				AppID = "589290",
				ExeName = "Holdfast NaW - Dedicated Server.exe",
				RequiredArgs = "-batchmode -nographics -server_name=\"{ServerName}\" -port={port} -query_port={query} -map_name=\"{map}\"",
				Port = 20101,
				QueryPort = 27015,
				Maps = ["FortSchwarz", "CampNile"]
			},
			new()
			{
				Game = "DeadPoly",
				AppID = "1682440",
				ExeName = @"DeadPoly\Binaries\Win64\DeadPolyServer-Win64-Shipping.exe",
				RequiredArgs = "{map}?Listen -log -port={port} -queryport={query} -ServerName=\"{ServerName}\" -Password=\"{pass}\"",
				Port = 7777,
				QueryPort = 27015,
				Maps = ["DeadPoly"]
			},
			new()
			{
				Game = "Bellwright",
				AppID = "2862850",
				ExeName = @"Bellwright\Binaries\Win64\BellwrightServer-Win64-Shipping.exe",
				RequiredArgs = "{map}?Listen -log -port={port} -queryport={query} -ServerName=\"{ServerName}\" -Password=\"{pass}\"",
				Port = 7777,
				QueryPort = 27015,
				Maps = ["Bellwright"]
			},
			new()
			{
				Game = "No More Room in Hell",
				AppID = "317590",
				ExeName = "srcds.exe",
				RequiredArgs = "-game nmrih -console -port {port} +maxplayers {MaxPlayers} +map {map} +hostname \"{ServerName}\"",
				Port = 27015,
				QueryPort = 27015,
				Maps = ["nmo_broadway", "nmo_cabin"]
			},
			new()
			{
				Game = "Sven Co-op",
				AppID = "276060",
				ExeName = "svends.exe",
				RequiredArgs = "-console -port {port} +maxplayers {MaxPlayers} +map {map} +hostname \"{ServerName}\"",
				Port = 27015,
				QueryPort = 27015,
				Maps = ["stadium4"]
			},
			new()
			{
				Game = "Craftopia",
				AppID = "1385410",
				ExeName = "Craftopia.exe",
				RequiredArgs = "-batchmode -showlogs -nographics -port {port} -name \"{ServerName}\" -pwd \"{pass}\"",
				Port = 8787,
				QueryPort = 8787,
				Maps = ["Default"]
			},
			new()
			{
				Game = "The Isle",
				AppID = "412680",
				ExeName = @"TheIsle\Binaries\Win64\TheIsleServer-Win64-Shipping.exe",
				RequiredArgs = "{map}?Listen?ServerName=\"{ServerName}\"?ServerPassword=\"{pass}\"?Port={port}?QueryPort={query} -log",
				Port = 7777,
				QueryPort = 27015,
				Maps = ["Spiro", "Gateway"]
			}
		];

		public static IReadOnlyList<GameInfo> GetGameList()
		{
			return games;
		}

		public static GameInfo? GetGame(string gameName)
		{
			return games.FirstOrDefault(g => g.Game.Equals(gameName, StringComparison.OrdinalIgnoreCase));
		}
	}
}