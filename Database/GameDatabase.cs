/*
 * Copyright (c) 2026 ubidzz. All Rights Reserved.
 *
 * This file is part of Synix Control Panel.
 *
 * This code is provided for transparent viewing and personal use only.
 * Unauthorized distribution, public modification, or commercial 
 * use of this source code or the compiled executable is strictly 
 * prohibited. Please refer to the LICENSE file in the root 
 * directory for full terms.
 */
using Synix_Control_Panel.ServerHandler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;

namespace Synix_Control_Panel.Database
{	
	public static class GameDatabase
	{
		public static IReadOnlyList<GameInfo> GetGames => games;
		// Use ReadOnly to protect the master list
		private static readonly IReadOnlyList<GameInfo> games =
		[
			new() {
				Game = "StarRupture",
				AppID = "3809400",
				ExeName = @"StarRupture\Binaries\Win64\StarRuptureServerEOS-Win64-Shipping.exe",
				RequiredArgs = "{map}?Listen -server -log -MULTIHOME=0.0.0.0 -Port={port} -QueryPort={query} -ServerName=\"{ServerName}\" -MaxPlayers={MaxPlayers} -SteamAppId={steamAppID}",
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"StarRupture\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["MainWorld"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Windrose",
				AppID = "4129620",
				ExeName = @"R5\Binaries\Win64\WindroseServer-Win64-Shipping.exe",
				RequiredArgs = "{map} -server -log -MULTIHOME=0.0.0.0 -Port={port} -QueryPort={query} -ServerName=\"{ServerName}\" -MaxPlayers={MaxPlayers}",
				Port = 7777,
				QueryPort = 7778,
				RelativeConfigPath = @"R5\ServerDescription.json",
				Format = ConfigFormat.JSON,
				Maps = ["Default"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Soulmask",
				AppID = "3017310",
				ExeName = @"WS\Binaries\Win64\WSServer-Win64-Shipping.exe",
				RequiredArgs = "{map} -server -log -UTF8Output -Port={port} -QueryPort={query} -EchoPort=18888 -mainserverport=19000 -SteamServerName=\"{ServerName}\" -MaxPlayers={MaxPlayers} -PSW=\"{pass}\" -adminpsw=\"{adminpass}\" -{mode} -serverid=1 -forcepassthrough -SteamAppId={steamAppID} -online=Steam",
				RelativeConfigPath = @"WS\Saved\GameplaySettings\GameXishu.json",
				Port = 8777,
				QueryPort = 27016,
				Maps = ["Level01_Main", "DLC_Level01_Main"],
				GameModes = ["PVP", "PVE"],
				Format = ConfigFormat.JSON,
				NeedsConfigWarning = true
			},
			new() {
				Game = "7 Days to Die",
				AppID = "294420",
				ExeName = "7DaysToDieServer.exe",
				RequiredArgs = "-batchmode -nographics -dedicated -configfile=serverconfig.xml -port {port} -SteamAppId={steamAppID}",
				Port = 26900,
				QueryPort = 26900,
				RelativeConfigPath = "serverconfig.xml",
				Format = ConfigFormat.XML,
				Maps = ["Navezgane", "Pregen01"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Valheim",
				AppID = "896660",
				ExeName = "valheim_server.exe",
				RequiredArgs = "-nographics -SteamAppId={steamAppID} -batchmode -name \"{ServerName}\" -port {port} -world \"{map}\" -password \"{pass}\"",
				Port = 2456,
				QueryPort = 2457,
				Format = ConfigFormat.StandardINI,
				Maps = ["Dedicated"]
			},
			new() {
				Game = "Palworld",
				AppID = "2394010",
				ExeName = "Pal\\Binaries\\Win64\\PalServer-Win64-Shipping.exe",
				RequiredArgs = "EpicApp=PalServer -useperfthreads -NoAsyncLoadingThread -UseMultithreadForDS -SteamAppId={steamAppID} -port={port} -queryport={query} -players={MaxPlayers} -ServerName=\"{ServerName}\"",
				RelativeConfigPath = "Pal\\Saved\\Config\\WindowsServer\\PalWorldSettings.ini",
				Port = 8211,
				QueryPort = 27015,
				Maps = ["DefaultWorld"],
				Format = ConfigFormat.Palworld,
				NeedsConfigWarning = true
			},
			new() {
				Game = "ARK: Survival Evolved",
				AppID = "376030",
				ExeName = @"ShooterGame\Binaries\Win64\ShooterGameServer.exe",
				RequiredArgs = "{map}?Listen?SessionName=\"{ServerName}\"?Port={port}?QueryPort={query}?MaxPlayers={MaxPlayers} -server -log -SteamAppId={steamAppID}",
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"ShooterGame\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["TheIsland", "ScorchedEarth_P", "Aberration_P", "Extinction", "Genesis", "Ragnarok", "TheCenter", "Valguero_P", "CrystalIsles", "Gen2", "LostIsland", "Fjordur"],
				GameModes = ["PVE", "PVP"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "ARK: Survival Ascended",
				AppID = "2430930",
				ExeName = @"ShooterGame\Binaries\Win64\ArkServer.exe",
				RequiredArgs = "{map}?Listen?SessionName=\"{ServerName}\"?ServerPassword=\"{pass}\"?ServerAdminPassword=\"{adminpass}\"?Port={port}?QueryPort={query}?MaxPlayers={MaxPlayers} -server -log -SteamAppId={steamAppID}",
				RconSyntax = "?RCONEnabled=True?RCONPort={rcon_port}?ServerAdminPassword=\"{rcon_pass}\"",
				RelativeConfigPath = @"ShooterGame\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				Port = 7777,
				QueryPort = 27015,
				Maps = ["TheIsland_WP", "ScorchedEarth_WP", "TheCenter_WP", "Aberration_WP", "Extinction_WP"],
				GameModes = ["PVE", "PVP"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Sons Of The Forest",
				AppID = "2465200",
				ExeName = "SonsOfTheForestDS.exe",
				RequiredArgs = "-userdatapath \"Saves\" -port {port} -queryport {query} -maxplayers {MaxPlayers} -servername \"{ServerName}\" -password \"{pass}\" -SteamAppId={steamAppID}",
				Port = 8766,
				QueryPort = 27016,
				RelativeConfigPath = @"userdata\dedicated_server.cfg",
				Format = ConfigFormat.JSON,
				Maps = ["Default"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Enshrouded",
				AppID = "2278520",
				ExeName = "enshrouded_server.exe",
				RelativeConfigPath = "enshrouded_server.json",
				RequiredArgs = "-port {port} -queryport {query} -SteamAppId={steamAppID}",
				Format = ConfigFormat.JSON,
				Port = 15636,
				QueryPort = 15637,
				Maps = ["Embervale"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Core Keeper",
				AppID = "1963720",
				ExeName = "CoreKeeperServer.exe",
				RequiredArgs = "-world {map} -worldname \"{ServerName}\" -port {port} -maxplayers {MaxPlayers} -SteamAppId={steamAppID}",
				Port = 27015,
				QueryPort = 27016,
				RelativeConfigPath = @"DedicatedServer\ServerConfig.json",
				Format = ConfigFormat.JSON,
				Maps = ["0", "1", "2"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Terraria",
				AppID = "105600",
				ExeName = "TerrariaServer.exe",
				RequiredArgs = "-port {port} -maxplayers {MaxPlayers} -world \"{map}\" -password \"{pass}\" -SteamAppId={steamAppID}",
				Port = 7777,
				QueryPort = 7777,
				RelativeConfigPath = "serverconfig.txt",
				Format = ConfigFormat.StandardINI,
				Maps = ["World1.wld"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Counter-Strike 2",
				AppID = "730",
				ExeName = @"game\bin\win64\cs2.exe",
				RequiredArgs = "-dedicated +map {map} -port {port} -maxplayers {MaxPlayers} +sv_password \"{pass}\" -SteamAppId={steamAppID}",
				RconSyntax = "+rcon_password \"{rcon_pass}\"",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"game\csgo\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["de_dust2", "de_inferno", "de_mirage", "de_nuke", "de_vertigo"]
			},
			new() {
				Game = "Team Fortress 2",
				AppID = "232250",
				ExeName = "srcds.exe",
				RequiredArgs = "-game tf -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map} +sv_password \"{pass}\" +hostname \"{ServerName}\"",
				RconSyntax = "+rcon_password \"{rcon_pass}\"",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"tf\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["ctf_2fort", "pl_upward", "cp_dustbowl", "koth_harvest"]
			},
			new() {
				Game = "Left 4 Dead 2",
				AppID = "222860",
				ExeName = "srcds.exe",
				RequiredArgs = "-game left4dead2 -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map} +mp_gamemode {mode} +hostname \"{ServerName}\" {rcon}",
				RconSyntax = "+rcon_password \"{rcon_pass}\"",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"left4dead2\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["c1m1_hotel", "c2m1_highway", "c8m1_apartment", "c14m1_junkyard", "c1m4_atrium"],
				GameModes = ["coop", "versus", "survival", "scavenge"]
			},
			new() {
				Game = "Squad",
				AppID = "403240",
				ExeName = "SquadGameServer.exe",
				RequiredArgs = "Port={port} QueryPort={query} FIXEDMAXPLAYERS={MaxPlayers} +map {map} -SteamAppId={steamAppID}",
				Port = 7787,
				QueryPort = 27165,
				RelativeConfigPath = @"SquadGame\ServerConfig\Server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["Mutaha_AAS_v1", "Gorodok_RAAS_v1", "Fallujah_AAS_v1"]
			},
			new() {
				Game = "Stationeers",
				AppID = "600760",
				ExeName = "rocketstation_DedicatedServer.exe",
				RequiredArgs = "-batchmode -nographics -SteamAppId={steamAppID} -autostart -loadlatest {map} -settings StartLocalHost true ServerVisible true ServerMaxPlayers {MaxPlayers} ServerPort {port} ServerName \"{ServerName}\" ServerPassword \"{pass}\"",
				Port = 27016,
				QueryPort = 27015,
				RelativeConfigPath = "default.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["Moon", "Mars", "Europa", "Mimas"]
			},
			new() {
				Game = "Empyrion - Galactic Survival",
				AppID = "530870",
				ExeName = "EmpyrionDedicated.cmd",
				RequiredArgs = "-batchmode -nographics -SteamAppId={steamAppID} -port {port} -queryport {query} -logFile Logs\\current.log",
				Port = 30000,
				QueryPort = 30001,
				Format = ConfigFormat.StandardINI,
				Maps = ["Default Multiplayer"]
			},
			new() {
				Game = "Stormworks: Build and Rescue",
				AppID = "1247090",
				ExeName = "server64.exe",
				RequiredArgs = "+server_name \"{ServerName}\" +port {port} +password \"{pass}\" +max_players {MaxPlayers} -SteamAppId={steamAppID}",				
				Port = 25564,
				QueryPort = 25564,
				RelativeConfigPath = "server_config.xml",
				Format = ConfigFormat.XML,
				Maps = ["Default"]
			},
			new() {
				Game = "The Forest",
				AppID = "556450",
				ExeName = "TheForestDedicatedServer.exe",
				RequiredArgs = "-batchmode -nographics -SteamAppId={steamAppID} -savefolderpath \"Saves\" -serverip 0.0.0.0 -serverplayers {MaxPlayers} -serverpassword \"{pass}\" -serverpassword_admin \"{adminpass}\" -serverport {port} -serverqueryport {query}",
				Port = 27015,
				QueryPort = 27016,
				RelativeConfigPath = @"ds\Server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["Saves"]
			},
			new() {
				Game = "Astroneer",
				AppID = "533830",
				ExeName = @"Astro\Binaries\Win64\AstroServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -SteamAppId={steamAppID}",				
				Port = 8777,
				QueryPort = 8777,
				RelativeConfigPath = @"Astro\Saved\Config\WindowsServer\AstroServerSettings.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["Sylva"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Barotrauma",
				AppID = "1022710",
				ExeName = "DedicatedServer.exe",
				RequiredArgs = "-port {port} -queryport {query} -name \"{ServerName}\" -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				RelativeConfigPath = "serversettings.xml",
				Format = ConfigFormat.XML,
				Maps = ["Campaign"]
			},
			new() {
				Game = "Abiotic Factor",
				AppID = "2816220",
				ExeName = @"AbioticFactor\Binaries\Win64\AbioticFactorServer-Win64-Shipping.exe",
				RequiredArgs = "{map}?Listen -log -MaxPlayers={MaxPlayers} -Port={port} -QueryPort={query} -ServerPassword=\"{pass}\" -SteamAppId={steamAppID}",				
				RelativeConfigPath = @"AbioticFactor\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				Port = 7777,
				QueryPort = 27015,
				Maps = ["Cascade"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Icarus",
				AppID = "2089300",
				ExeName = @"Icarus\Binaries\Win64\IcarusServer-Win64-Shipping.exe",
				RequiredArgs = "{map} -Log -SteamServerName=\"{ServerName}\" -Port={port} -QueryPort={query} -SteamAppId={steamAppID}",				
				RelativeConfigPath = @"Icarus\Saved\Config\WindowsServer\ServerSettings.ini",
				Format = ConfigFormat.StandardINI,
				Port = 17777,
				QueryPort = 27016,
				Maps = ["Olympus", "Styx", "Prometheus", "Elysium"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Don't Starve Together",
				AppID = "343050",
				ExeName = @"bin\dontstarve_dedicated_server_nullrenderer.exe",
				RequiredArgs = "-console -cluster \"{Identity}\" -shard {map} -SteamAppId={steamAppID}",				
				Port = 10999,
				QueryPort = 27016,
				RelativeConfigPath = @"DoNotStarveTogether\{Identity}\cluster.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["Master", "Caves"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Killing Floor 2",
				AppID = "232130",
				ExeName = @"Binaries\Win64\KFServer.exe",
				RequiredArgs = "{map}?Game=KFGameContent.KFGameInfo_{mode}?AdminPassword=\"{adminpass}\"?GamePassword=\"{pass}\" -Port={port} -SteamAppId={steamAppID}",				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"KFGame\Config\PCServer-KFGame.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["KF-BioticsLab", "KF-BurningParis", "KF-Outpost", "KF-ZedLanding"],
				GameModes = ["Survival", "Versus"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "The Front",
				AppID = "2568660",
				ExeName = @"ProjectWar\Binaries\Win64\TheFrontServer-Win64-Shipping.exe",
				RequiredArgs = "{map}?Listen?MaxPlayers={MaxPlayers}?ServerName=\"{ServerName}\"?ServerPassword=\"{pass}\"?ServerAdminPassword=\"{adminpass}\"?Port={port}?QueryPort={query} -server -log -SteamAppId={steamAppID} -{mode}",
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"ProjectWar\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["TheFront"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Smalland: Survive the Wilds",
				AppID = "2404090",
				ExeName = @"SMALLAND\Binaries\Win64\SMALLANDServer-Win64-Shipping.exe",
				RequiredArgs = "{map}?Listen -log -ServerName=\"{ServerName}\" -Password=\"{pass}\" -MaxPlayers={MaxPlayers} -Port={port} -SteamAppId={steamAppID}",				
				RelativeConfigPath = @"SMALLAND\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				Port = 7777,
				QueryPort = 27015,
				Maps = ["Smalland"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Sunkenland",
				AppID = "2605310",
				ExeName = "Sunkenland Dedicated Server.exe",
				RequiredArgs = "-batchmode -nographics -SteamAppId={steamAppID} -serverName \"{ServerName}\" -password \"{pass}\" -port {port} -maxPlayers {MaxPlayers}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"Worlds\{Identity}\ServerConfig.txt",
				Format = ConfigFormat.StandardINI,
				Maps = ["World1"]
			},
			new() {
				Game = "Risk of Rain 2",
				AppID = "1180760",
				ExeName = "Risk of Rain 2 Dedicated Server.exe",
				RequiredArgs = "-batchmode -nographics -SteamAppId={steamAppID} -server_port {port} -server_query_port {query}",
				Port = 27015,
				QueryPort = 27015,
				Format = ConfigFormat.StandardINI,
				Maps = ["Default"]
			},
			new() {
				Game = "V Rising",
				AppID = "1829350",
				ExeName = "VRisingServer.exe",
				RequiredArgs = "-persistentDataPath .\\save-data -server -ServerName \"{ServerName}\" -SteamAppId={steamAppID}",
				Port = 9876,
				QueryPort = 9877,
				RelativeConfigPath = @"VRisingServer_Data\StreamingAssets\Settings\ServerHostSettings.json",
				Format = ConfigFormat.JSON,
				Maps = ["World1"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "DayZ",
				AppID = "223350",
				ExeName = "DayZServer_x64.exe",
				RequiredArgs = "-config=serverDZ.cfg -port={port} -name=\"{ServerName}\" -SteamAppId={steamAppID}",
				Port = 2302,
				QueryPort = 27016,
				RelativeConfigPath = "serverDZ.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["ChernarusPlus"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Conan Exiles",
				AppID = "443030",
				ExeName = @"ConanSandbox\Binaries\Win64\ConanSandboxServer.exe",
				RequiredArgs = "{map}?Listen?MaxPlayers={MaxPlayers}?ServerName=\"{ServerName}\"?ServerPassword=\"{pass}\"?Port={port}?QueryPort={query} -server -log -SteamAppId={steamAppID}",
				Port = 7777,
				QueryPort = 27015,
				Maps = ["TheExiledLands"],
				RelativeConfigPath = @"ConanSandbox\Saved\Config\WindowsServer\ServerSettings.ini",
				Format = ConfigFormat.StandardINI,
				GameModes = ["PVE", "PVP"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Garry's Mod",
				AppID = "4000",
				ExeName = "srcds.exe",
				RequiredArgs = "-game garrysmod -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map} +gamemode {mode} +hostname \"{ServerName}\" +sv_password \"{pass}\"",
				RconSyntax = "+rcon_password \"{rcon_pass}\"",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"garrysmod\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["gm_construct", "gm_flatgrass", "ttt_67thway_v3", "ttt_clue", "rp_downtown_v4c_v2", "zs_obj_vertigo", "ph_office", "mu_clue", "dr_playstation"],
				GameModes = ["sandbox", "ttt", "darkrp", "zombiesurvival", "prophunt", "murder", "deathrun", "basewars"]
			},
			new() {
				Game = "Project Zomboid",
				AppID = "380870",
				ExeName = "StartServer64.bat",
				RequiredArgs = "-port {port} -server -servername \"{ServerName}\" -SteamAppId={steamAppID}",
				Port = 16261,
				QueryPort = 16262,
				RelativeConfigPath = @"Zomboid\Server\{Identity}.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["Muldraugh, KY"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Mordhau",
				AppID = "629800",
				ExeName = @"Mordhau\Binaries\Win64\MordhauServer-Win64-Shipping.exe",
				RequiredArgs = "{map} -log -port={port} -QueryPort={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"Mordhau\Saved\Config\WindowsServer\Game.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["FFA_Contraband", "SKM_Camp"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Valheim (Crossplay)",
				AppID = "896660",
				ExeName = "valheim_server.exe",
				RequiredArgs = "-nographics -batchmode -name \"{ServerName}\" -port {port} -world \"{map}\" -password \"{pass}\" -crossplay -SteamAppId={steamAppID}",
				Port = 2456,
				QueryPort = 2457,
				Format = ConfigFormat.StandardINI,
				Maps = ["Dedicated"]
			},
			new()
			{
				Game = "Satisfactory",
				AppID = "1690800",
				ExeName = @"FactoryGame\Binaries\Win64\FactoryServer-Win64-Shipping.exe",
				RequiredArgs = "-log -unattended -ServerQueryPort={query} -multihome=0.0.0.0 -port={port} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 15777,
				RelativeConfigPath = @"FactoryGame\Saved\Config\WindowsServer\Game.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["Satisfactory"],
				NeedsConfigWarning = true
			},
			new()
			{
				Game = "Factorio",
				AppID = "428200",
				ExeName = @"bin\x64\factorio.exe",
				RequiredArgs = "--start-server {map}.zip --server-settings data\\server-settings.json --port {port} -SteamAppId={steamAppID}",
				Port = 34197,
				QueryPort = 34197,
				RelativeConfigPath = @"data\server-settings.json",
				Format = ConfigFormat.JSON,
				Maps = ["FactorioWorld"],
				NeedsConfigWarning = true
			},
			new()
			{
				Game = "Unturned",
				AppID = "1110390",
				ExeName = "Unturned.exe",
				RequiredArgs = "-batchmode -nographics -SteamAppId={steamAppID} \"+InternetServer/{Identity}\" +map {map} -port {port} -password \"{pass}\" {mode}",
				QueryPort = 27016,
				RelativeConfigPath = @"Servers\{Identity}\Server\Commands.dat",
				Format = ConfigFormat.StandardINI,
				Maps = ["PEI", "Washington", "Russia", "Germany", "Hawaii"],
				GameModes = ["PVP", "PVE"]
			},
			new()
			{
				Game = "Space Engineers",
				AppID = "298740",
				ExeName = @"DedicatedServer64\SpaceEngineersDedicated.exe",
				RequiredArgs = "-noconsole -ignorelastsession -port {port} -SteamAppId={steamAppID}",				
				Port = 27016,
				QueryPort = 27016,
				GameModes = ["Creative"],
				RelativeConfigPath = @"Instance\SpaceEngineers-Dedicated.cfg",
				Format = ConfigFormat.XML,
				Maps = ["StarSystem", "AlienPlanet", "EmptyWorld"],
				NeedsConfigWarning = true
			},
			new()
			{
				Game = "Arma 3",
				AppID = "233780",
				ExeName = "arma3server.exe",
				RequiredArgs = "-port={port} -name=\"{ServerName}\" -config=server.cfg -world={map} -SteamAppId={steamAppID}",				
				Port = 2302,
				QueryPort = 2303,
				RelativeConfigPath = "server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["empty", "Altis", "Stratis", "Tanoa", "Malden"]
			},
			new()
			{
				Game = "Insurgency: Sandstorm",
				AppID = "581330",
				ExeName = @"Insurgency\Binaries\Win64\InsurgencyServer-Win64-Shipping.exe",
				RequiredArgs = "{map}?Scenario={map}?MaxPlayers={MaxPlayers} -port={port} -queryport={query} -hostname=\"{ServerName}\" -password=\"{pass}\" -SteamAppId={steamAppID}",				
				Port = 27102,
				QueryPort = 27131,
				RelativeConfigPath = @"Insurgency\Saved\Config\WindowsServer\Game.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["Refinery", "Farmhouse", "Hideout", "Crossing"],
				NeedsConfigWarning = true
			},
			new()
			{
				Game = "Myth of Empires",
				AppID = "1371580",
				ExeName = @"MOE\Binaries\Win64\MOEServer-Win64-Shipping.exe",
				RequiredArgs = "{map}?Listen -server -log -SteamAppId={steamAppID} -Port={port} -QueryPort={query} -ServerName=\"{ServerName}\" -ServerPassword=\"{pass}\"",
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"MOE\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				GameModes = ["PVE", "PVP"],
				Maps = ["ZhongZhou", "DongZhou"],
				NeedsConfigWarning = true
			},
			new()
			{
				Game = "Mount & Blade II: Bannerlord",
				AppID = "1863440",
				ExeName = @"bin\Win64_Shipping_Server\Bannerlord.DedicatedServer.exe",
				RequiredArgs = "_MODULES_*Native*Multiplayer*_MODULES_ /dedicatedcustomserverconfigfile {map} /port {port} -SteamAppId={steamAppID}",				
				Port = 7230,
				QueryPort = 7230,
				Maps = ["CustomServerconfig.txt"],
				Format = ConfigFormat.StandardINI,
			},
			new()
			{
				Game = "Arma Reforger",
				AppID = "1874900",
				ExeName = "ArmaReforgerServer.exe",
				RequiredArgs = "-config \"{map}\" -profile \"profile\" -maxPlayers {MaxPlayers} -port {port} -SteamAppId={steamAppID}",			
				Port = 2001,
				QueryPort = 2001,
				RelativeConfigPath = @"configs\server.json",
				Format = ConfigFormat.JSON,
				Maps = ["server.json"]
			},
			new()
			{
				Game = "Avorion",
				AppID = "565060",
				ExeName = @"bin\AvorionServer.exe",
				RequiredArgs = "--galaxy-name \"{map}\" --seed {seed} --server-name \"{ServerName}\" --admin \"{adminpass}\" --port {port} --use-steam-networking 1 -SteamAppId={steamAppID}",				
				Port = 27000,
				QueryPort = 27003,
				RelativeConfigPath = @"galaxies\avorion_galaxy\server.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["galaxy"]
			},
			new()
			{
				Game = "PixARK",
				AppID = "824360",
				ExeName = @"ShooterGame\Binaries\Win64\PixARKServer.exe",
				RequiredArgs = "{map}?Listen?SessionName=\"{ServerName}\"?ServerPVE={mode}?ServerPassword=\"{pass}\"?ServerAdminPassword=\"{adminpass}\"?Port={port}?QueryPort={query} {rcon} -server -log -SteamAppId={steamAppID}",
				RconSyntax = "?RCONEnabled=True?RCONPort={rcon_port}?ServerAdminPassword=\"{rcon_pass}\"",
				Port = 7777,
				QueryPort = 27015,
				GameModes = ["PVE", "PVP"],
				RelativeConfigPath = @"ShooterGame\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["CubeWorld_Light"],
				NeedsConfigWarning = true
			},
			new()
			{
				Game = "Atlas",
				AppID = "1006030",
				ExeName = @"ShooterGame\Binaries\Win64\ShooterGameServer.exe",
				RequiredArgs = "Ocean?ServerX=0?ServerY=0?AltSaveDirectoryName=\"{map}\"?ServerPVE={mode}?ServerAdminPassword=\"{adminpass}\"?MaxPlayers={MaxPlayers}?Port={port}?QueryPort={query} {rcon} ?SeamlessIP=0.0.0.0 -log -server -SteamAppId={steamAppID}",				
				RconSyntax = "?RCONEnabled=True?RCONPort={rcon_port}?ServerAdminPassword=\"{rcon_pass}\"",
				Port = 57555,
				QueryPort = 57555,
				GameModes = ["True", "False"],
				RelativeConfigPath = @"ShooterGame\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["00"],
				NeedsConfigWarning = true
			},
			new()
			{
				Game = "SCUM",
				AppID = "683000",
				ExeName = @"SCUM\Binaries\Win64\SCUMServer-Win64-Shipping.exe",
				RequiredArgs = "Port={port} QueryPort={query} -ServerName=\"{ServerName}\" -SteamAppId={steamAppID}",
				RelativeConfigPath = @"SCUM\Saved\Config\WindowsServer\ServerSettings.ini",
				Format = ConfigFormat.StandardINI,
				Port = 7042,
				QueryPort = 7043,
				Maps = ["SCUM_Island"],
				NeedsConfigWarning = true
			},
			new()
			{
				Game = "Eco",
				AppID = "739590",
				ExeName = "EcoServer.exe",
				RequiredArgs = "-nogui -port {port} -SteamAppId={steamAppID}",				
				Port = 3000,
				QueryPort = 3001,
				RelativeConfigPath = @"Configs\Network.eco",
				Format = ConfigFormat.JSON,
				Maps = ["DefaultWorld"]
			},
			new()
			{
				Game = "Hell Let Loose",
				AppID = "1062090",
				ExeName = @"HLL\Binaries\Win64\HLLServer-Win64-Shipping.exe",
				RequiredArgs = "-port={port} -queryport={query} -log -SteamAppId={steamAppID}",				
				Port = 8211,
				QueryPort = 27015,
				RelativeConfigPath = @"HLL\Saved\Config\WindowsServer\Server.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["SainteMarieDuMont_Warfare"],
				NeedsConfigWarning = true
			},
			new()
			{
				Game = "SCP: Secret Laboratory",
				AppID = "996560",
				ExeName = "LocalAdmin.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 7777,
				Format = ConfigFormat.StandardINI,
				Maps = ["Facility"]
			},
			new()
			{
				Game = "Starbound",
				AppID = "211820",
				ExeName = @"win64\starbound_server.exe",
				RequiredArgs = "-port {port} -config \"storage\\starbound_server.config\" -SteamAppId={steamAppID}",				
				Port = 21025,
				QueryPort = 21025,
				RelativeConfigPath = @"storage\starbound_server.config",
				Format = ConfigFormat.JSON,
				Maps = ["Universe"]
			},
			new()
			{
				Game = "Wurm Unlimited",
				AppID = "402370",
				ExeName = "WurmServerLauncher-64.exe",
				RequiredArgs = "{map} -SteamAppId={steamAppID}",			
				Port = 3724,
				QueryPort = 27016,
				RelativeConfigPath = @"Creative\gameserver.conf",
				Format = ConfigFormat.StandardINI,
				Maps = ["Adventure"],
				NeedsConfigWarning = true
			},
			new()
			{
				Game = "Nightingale",
				AppID = "2445990",
				ExeName = @"NWX\Binaries\Win64\NWXServer-Win64-Shipping.exe",
				RequiredArgs = "{map}?Listen -log -ServerName=\"{ServerName}\" -Password=\"{pass}\" -Port={port} -QueryPort={query} -SteamAppId={steamAppID}",				
				RelativeConfigPath = @"NWX\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				Port = 7777,
				QueryPort = 27015,
				Maps = ["SylvanGlade"],
				NeedsConfigWarning = true
			},
			new()
			{
				Game = "Holdfast: Nations At War",
				AppID = "589290",
				ExeName = "Holdfast NaW - Dedicated Server.exe",
				RequiredArgs = "-batchmode -nographics -SteamAppId={steamAppID} -server_name=\"{ServerName}\" -port={port} -query_port={query} -map_name=\"{map}\"",
				Port = 20101,
				QueryPort = 27015,
				RelativeConfigPath = @"Holdfast NaW_Data\StreamingAssets\Config\serverConfig_Core.txt",
				Format = ConfigFormat.StandardINI,
				Maps = ["FortSchwarz", "CampNile"],
				NeedsConfigWarning = true
			},
			new()
			{
				Game = "DeadPoly",
				AppID = "1682440",
				ExeName = @"DeadPoly\Binaries\Win64\DeadPolyServer-Win64-Shipping.exe",
				RequiredArgs = "{map}?Listen -log -port={port} -queryport={query} -ServerName=\"{ServerName}\" -Password=\"{pass}\" -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"DeadPoly\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["DeadPoly"],
				NeedsConfigWarning = true
			},
			new()
			{
				Game = "Bellwright",
				AppID = "2862850",
				ExeName = @"Bellwright\Binaries\Win64\BellwrightServer-Win64-Shipping.exe",
				RequiredArgs = "{map}?Listen -log -port={port} -queryport={query} -ServerName=\"{ServerName}\" -Password=\"{pass}\" -SteamAppId={steamAppID}",				
				RelativeConfigPath = @"Bellwright\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				Port = 7777,
				QueryPort = 27015,
				Maps = ["Bellwright"],
				NeedsConfigWarning = true
			},
			new()
			{
				Game = "No More Room in Hell",
				AppID = "317590",
				ExeName = "srcds.exe",
				RequiredArgs = "-game nmrih -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map} +hostname \"{ServerName}\" {rcon}",
				RconSyntax = "+rcon_password \"{rcon_pass}\"",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"nmrih\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["nmo_broadway", "nmo_cabin"]
			},
			new()
			{
				Game = "Sven Co-op",
				AppID = "276060",
				ExeName = "svends.exe",
				RequiredArgs = "-console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map} +hostname \"{ServerName}\" {rcon}",
				RconSyntax = "+rcon_password \"{rcon_pass}\"",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"svencoop\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["stadium4"]
			},
			new()
			{
				Game = "Craftopia",
				AppID = "1385410",
				ExeName = "Craftopia.exe",
				RequiredArgs = "-batchmode -showlogs -nographics -port {port} -name \"{ServerName}\" -pwd \"{pass}\" -SteamAppId={steamAppID}",				
				Port = 8787,
				QueryPort = 8787,
				RelativeConfigPath = @"Craftopia_Data\Server\ServerSetting.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["Default"],
				NeedsConfigWarning = true
			},
			new()
			{
				Game = "The Isle",
				AppID = "412680",
				ExeName = @"TheIsle\Binaries\Win64\TheIsleServer-Win64-Shipping.exe",
				RequiredArgs = "{map}?Listen?ServerName=\"{ServerName}\"?ServerPassword=\"{pass}\"?Port={port}?QueryPort={query} -log -SteamAppId={steamAppID}",				
				RelativeConfigPath = @"TheIsle\Saved\Config\WindowsServer\Game.ini",
				Format = ConfigFormat.StandardINI,
				Port = 7777,
				QueryPort = 27015,
				Maps = ["Spiro", "Gateway"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Ready or Not",
				AppID = "1845110",
				ExeName = @"ReadyOrNotServer.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"ReadyOrNot\Saved\Config\WindowsServer\Engine.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Grounded",
				AppID = "2162980",
				ExeName = @"Maine\Binaries\Win64\MaineServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"Maine\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Rising Storm 2: Vietnam",
				AppID = "418480",
				ExeName = @"Binaries\Win64\VNGame.exe",
				RequiredArgs = "{map}?MaxPlayers={MaxPlayers} -Port={port} -QueryPort={query} -Log=ServerLog.log -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"ROGame\Config\ROGame.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["VNTE-SongBe", "VNSU-AnLaoValley"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Hurtworld",
				AppID = "405100",
				ExeName = "Hurtworld.exe",
				RequiredArgs = "-batchmode -nographics -SteamAppId={steamAppID} -exec \"host {port};queryport {query};servername {ServerName};addadmin {adminpass}\" -logfile \"gamelog.txt\"",
				Port = 12871,
				QueryPort = 12881,
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Day of Dragons",
				AppID = "1088320",
				ExeName = @"Dragons\Binaries\Win64\DragonsServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"Dragons\Saved\Config\WindowsServer\Game.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["OpenWorld"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Miscreated",
				AppID = "302200",
				ExeName = @"Bin64_dedicated\MiscreatedServer.exe",
				RequiredArgs = "-sv_port {port} +sv_maxplayers {MaxPlayers} +sv_servername \"{ServerName}\" +map {map} -SteamAppId={steamAppID}",				Port = 64090,
				QueryPort = 64091,
				RelativeConfigPath = "miscreated.db",
				Format = ConfigFormat.StandardINI,
				Maps = ["islands"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "American Truck Simulator",
				AppID = "270880",
				ExeName = @"bin\win_x64\amtrucks.exe",
				RequiredArgs = "-dedicated -server_config server_config.sii -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				RelativeConfigPath = "server_config.sii",
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Life is Feudal: Your Own",
				AppID = "320850",
				ExeName = "DedicatedServer.exe",
				RequiredArgs = "-world {map} -port {port} -SteamAppId={steamAppID}",				
				Port = 28000,
				QueryPort = 28001,
				RelativeConfigPath = "config_local.xml",
				Format = ConfigFormat.XML,
				Maps = ["yo_main"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Citadel: Forged with Fire",
				AppID = "487120",
				ExeName = @"Citadel\Binaries\Win64\CitadelServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",			
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"Citadel\Saved\Config\WindowsServer\Game.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["Ignus"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "CryoFall",
				AppID = "829590",
				ExeName = "CryoFall_Server.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",			
				Port = 6000,
				QueryPort = 6000,
				RelativeConfigPath = "Settings.xml",
				Format = ConfigFormat.XML,
				Maps = ["Default"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Counter-Strike: Source",
				AppID = "232330",
				ExeName = "srcds.exe",
				RequiredArgs = "-game cstrike -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map} +hostname \"{ServerName}\" {rcon}",
				RconSyntax = "+rcon_password \"{rcon_pass}\"",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"cstrike\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["de_dust2", "cs_office"]
			},
			new() {
				Game = "Day of Defeat: Source",
				AppID = "232290",
				ExeName = "srcds.exe",
				RequiredArgs = "-game dod -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map} +hostname \"{ServerName}\" {rcon}",
				RconSyntax = "+rcon_password \"{rcon_pass}\"",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"dod\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["dod_donner", "dod_avalanche"]
			},
			new() {
				Game = "Half-Life 2: Deathmatch",
				AppID = "232370",
				ExeName = "srcds.exe",
				RequiredArgs = "-game hl2mp -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map} +hostname \"{ServerName}\" {rcon}",
				RconSyntax = "+rcon_password \"{rcon_pass}\"",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"hl2mp\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["crossfire", "bounce", "data-core", "ns_mines"]
			},
			new() {
				Game = "Left 4 Dead (1)",
				AppID = "222840",
				ExeName = "srcds.exe",
				RequiredArgs = "-game left4dead -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map} +hostname \"{ServerName}\" {rcon}",
				RconSyntax = "+rcon_password \"{rcon_pass}\"",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"left4dead\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["l4d_hospital01_apartment"]
			},
			new() {
				Game = "Day of Infamy",
				AppID = "447820",
				ExeName = "srcds.exe",
				RequiredArgs = "-game doi -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map} +hostname \"{ServerName}\" {rcon}",
				RconSyntax = "+rcon_password \"{rcon_pass}\"",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"doi\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["bastogne", "dog_red"]
			},
			new() {
				Game = "Insurgency (2014)",
				AppID = "237410",
				ExeName = "srcds.exe",
				RequiredArgs = "-game insurgency -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map} +hostname \"{ServerName}\" {rcon}",
				RconSyntax = "+rcon_password \"{rcon_pass}\"",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"insurgency\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["market", "sinjar"]
			},
			new() {
				Game = "Killing Floor (1)",
				AppID = "215350",
				ExeName = "ucc.exe",
				RequiredArgs = "server {map}.rom?game=KFmod.KFGameType?VACProtected=true -port={port} -SteamAppId={steamAppID}",				
				Port = 7707,
				QueryPort = 7708,
				Format = ConfigFormat.StandardINI,
				Maps = ["KF-BioticsLab"]
			},
			new() {
				Game = "Black Mesa",
				AppID = "362890",
				ExeName = "srcds.exe",
				RequiredArgs = "-game bms -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map} +hostname \"{ServerName}\" {rcon}",
				RconSyntax = "+rcon_password \"{rcon_pass}\"",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"bms\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["bm_c1a0a", "bm_c1a1a"]
			},
			new() {
				Game = "Contagion",
				AppID = "238430",
				ExeName = "srcds.exe",
				RequiredArgs = "-game contagion -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map} +hostname \"{ServerName}\" {rcon}",
				RconSyntax = "+rcon_password \"{rcon_pass}\"",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"contagion\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["ce_barlowsquare", "ce_roanokepd"]
			},
			new() {
				Game = "Dino D-Day",
				AppID = "70000",
				ExeName = "srcds.exe",
				RequiredArgs = "-game dinodday -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map} +hostname \"{ServerName}\" {rcon}",
				RconSyntax = "+rcon_password \"{rcon_pass}\"",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"dinodday\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["ddd_desiccant", "ddd_fortress"]
			},
			new() {
				Game = "Primal Carnage: Extinction",
				AppID = "321360",
				ExeName = @"PrimalCarnage\Binaries\Win64\PrimalCarnageServer.exe",
				RequiredArgs = "{map}?MaxPlayers={MaxPlayers}?Port={port}?QueryPort={query} -server -log -SteamAppId={steamAppID}",
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"PrimalCarnage\Saved\Config\WindowsServer\Game.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["PC-Docks"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Ranch Simulator",
				AppID = "1530750",
				ExeName = @"Ranch_Simulator\Binaries\Win64\Ranch_Simulator-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",		
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"Ranch_Simulator\Saved\Config\WindowsServer\Game.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["RanchMap"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Memories of Mars",
				AppID = "644290",
				ExeName = @"MemoriesOfMars\Binaries\Win64\MemoriesOfMarsServer.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"MemoriesOfMars\Saved\Config\WindowsServer\Game.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["Mars"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Fistful of Frags",
				AppID = "265210",
				ExeName = "srcds.exe",
				RequiredArgs = "-game fof -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map} +hostname \"{ServerName}\" {rcon}",
				RconSyntax = "+rcon_password \"{rcon_pass}\"",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"fof\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["fof_robertlee", "fof_desperados"]
			},
			new() {
				Game = "Deadside",
				AppID = "895390",
				ExeName = @"DeadsideServer\Binaries\Win64\DeadsideServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",			
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.StandardINI,
				Maps = ["Default"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Wreckfest",
				AppID = "335350",
				ExeName = "Wreckfest.exe",
				RequiredArgs = "-s server_config.cfg -SteamAppId={steamAppID}",			
				Port = 27015,
				QueryPort = 27016,
				RelativeConfigPath = "server_config.cfg",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Assetto Corsa",
				AppID = "244210",
				ExeName = "acServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 9600,
				QueryPort = 9601,
				RelativeConfigPath = @"cfg\server_cfg.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["imola", "monza", "spa"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "BeamNG.drive",
				AppID = "284160",
				ExeName = "BeamNG.drive.exe",
				RequiredArgs = "-dedicated -SteamAppId={steamAppID}",				
				Port = 30814,
				QueryPort = 30814,
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Last Oasis",
				AppID = "903950",
				ExeName = @"OasisServer\Binaries\Win64\OasisServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",			
				Port = 7777,
				QueryPort = 27015,
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Dark and Light",
				AppID = "529180",
				ExeName = @"DNL\Binaries\Win64\DNLServer.exe",
				RequiredArgs = "{map}?Listen?Port={port}?QueryPort={query} -server -log -SteamAppId={steamAppID}",
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"DNL\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["TheArchos"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Vagante",
				AppID = "323220",
				ExeName = "Vagante.exe",
				RequiredArgs = "-dedicated -port {port} -SteamAppId={steamAppID}",			
				Port = 1234,
				QueryPort = 1234,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Zombie Panic! Source",
				AppID = "17505",
				ExeName = "srcds.exe",
				RequiredArgs = "-game zps -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map} +hostname \"{ServerName}\" {rcon}",
				RconSyntax = "+rcon_password \"{rcon_pass}\"",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"zps\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["zps_cinema", "zps_policestation"]
			},
			new() {
				Game = "Alien Swarm: Reactive Drop",
				AppID = "563560",
				ExeName = "srcds.exe",
				RequiredArgs = "-game reactivedrop -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map} +hostname \"{ServerName}\" {rcon}",
				RconSyntax = "+rcon_password \"{rcon_pass}\"",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"reactivedrop\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["asi-jac1-landingbay_01"]
			},
			new() {
				Game = "Half-Life Deathmatch: Source",
				AppID = "232370",
				ExeName = "srcds.exe",
				RequiredArgs = "-game hl1mp -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map} +hostname \"{ServerName}\" {rcon}",
				RconSyntax = "+rcon_password \"{rcon_pass}\"",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"hl1mp\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["crossfire", "stalkyard"]
			},
			new() {
				Game = "Soldat",
				AppID = "638490",
				ExeName = "soldatserver.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 23073,
				QueryPort = 23073,
				RelativeConfigPath = "server.cfg",
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Project CARS 2",
				AppID = "378030",
				ExeName = "pCARS2AVServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				RelativeConfigPath = "server_config.json",
				Format = ConfigFormat.JSON
			},
			new() {
				Game = "The Stomping Land",
				AppID = "263360",
				ExeName = @"ShooterGame\Binaries\Win64\ShooterGameServer.exe",
				RequiredArgs = "TheIsland?Listen?Port={port}?QueryPort={query} -server -log -SteamAppId={steamAppID}",
				Port = 7777,
				QueryPort = 27015,
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Battlefield 2 (Direct Download)",
				AppID = "0", // Needs custom download logic
				ExeName = "bf2_w32ded.exe",
				RequiredArgs = "+dedicated +map {map} +port {port} -SteamAppId={steamAppID}",			
				Port = 16567,
				QueryPort = 29900,
				Maps = ["strike_at_karkand", "wake_island_2007"]
			},
			new() {
				Game = "Gray Zone Warfare",
				AppID = "2548800",
				ExeName = @"GZW\Binaries\Win64\GZWServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamServerName=\"{ServerName}\" -SteamAppId={steamAppID}",			
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"GZW\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true // 🛡️ REQUIRES GZW SERVER TOKEN
			},
			new() {
				Game = "ASKA",
				AppID = "2730300",
				ExeName = "ASKAServer.exe",
				RequiredArgs = "-batchmode -nographics -SteamAppId={steamAppID} -port {port} -queryport {query} -name \"{ServerName}\" -password \"{pass}\"",
				Port = 27015,
				QueryPort = 27016,
				RelativeConfigPath = "server_settings.json",
				Format = ConfigFormat.JSON,
				NeedsConfigWarning = true
			},
			new() {
				Game = "HumanitZ",
				AppID = "2465360",
				ExeName = @"HumanitZ\Binaries\Win64\HumanitZServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -ServerName=\"{ServerName}\" -SteamAppId={steamAppID}",			
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"HumanitZ\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Raft Dedicated Server",
				AppID = "2521190",
				ExeName = "RaftServer.exe",
				RequiredArgs = "-batchmode -nographics -SteamAppId={steamAppID} -port {port}",
				Port = 27015,
				QueryPort = 27016,
				RelativeConfigPath = "server_config.json",
				Format = ConfigFormat.JSON,
				NeedsConfigWarning = true
			},
			new() {
				Game = "VoidTrain",
				AppID = "1159690",
				ExeName = @"VoidTrain\Binaries\Win64\VoidTrainServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Volcanoids",
				AppID = "954080",
				ExeName = "VolcanoidsServer.exe",
				RequiredArgs = "-batchmode -nographics -SteamAppId={steamAppID} -port {port} -queryport {query}",
				Port = 27015,
				QueryPort = 27016,
				RelativeConfigPath = "server_settings.json",
				Format = ConfigFormat.JSON,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Pavlov VR",
				AppID = "622970",
				ExeName = @"Pavlov\Binaries\Win64\PavlovServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"Pavlov\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Contractors VR",
				AppID = "963930",
				ExeName = "ContractorsServer.exe",
				RequiredArgs = "-port {port} -queryport {query} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Isonzo",
				AppID = "1548670",
				ExeName = "IsonzoServer.exe",
				RequiredArgs = "-port {port} -queryport {query} -name \"{ServerName}\" -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Verdun",
				AppID = "242860",
				ExeName = "VerdunServer.exe",
				RequiredArgs = "-port {port} -queryport {query} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Tannenberg",
				AppID = "633460",
				ExeName = "TannenbergServer.exe",
				RequiredArgs = "-port {port} -queryport {query} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Quake II (Enhanced)",
				AppID = "232430",
				ExeName = "q2pro.exe",
				RequiredArgs = "+set dedicated 1 +set net_port {port} +map {map} -SteamAppId={steamAppID}",				
				Port = 27910,
				QueryPort = 27910,
				Maps = ["base1", "q2dm1", "q2dm2", "q2dm3"]
			},
			new() {
				Game = "Quake III Arena",
				AppID = "2200",
				ExeName = "q3ded.exe",
				RequiredArgs = "+set dedicated 1 +set net_port {port} +exec server.cfg -SteamAppId={steamAppID}",				
				Port = 27960,
				QueryPort = 27960,
				RelativeConfigPath = "server.cfg",
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Star Wars Jedi Knight: Jedi Academy",
				AppID = "6020",
				ExeName = "jampded.exe",
				RequiredArgs = "+set dedicated 1 +set net_port {port} +exec server.cfg -SteamAppId={steamAppID}",				
				Port = 29070,
				QueryPort = 29070,
				RelativeConfigPath = "server.cfg",
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Star Wars Battlefront II (Classic)",
				AppID = "6060",
				ExeName = "battlefront2.exe",
				RequiredArgs = "/dedicated /port {port} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27015,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Half-Life (Classic)",
				AppID = "70",
				ExeName = "hlds.exe",
				RequiredArgs = "-console -game valve +port {port} +maxplayers {MaxPlayers} +map {map} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"valve\config.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["crossfire", "bounce", "data-core"]
			},
			new() {
				Game = "Day of Defeat (GoldSrc)",
				AppID = "300",
				ExeName = "hlds.exe",
				RequiredArgs = "-console -game dod +port {port} +maxplayers {MaxPlayers} +map {map} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"dod\config.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["dod_donner", "dod_avalanche"]
			},
			new() {
				Game = "Team Fortress Classic",
				AppID = "20",
				ExeName = "hlds.exe",
				RequiredArgs = "-console -game tfc +port {port} +maxplayers {MaxPlayers} +map {map} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"tfc\config.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["2fort", "well", "badlands"]
			},
			new() {
				Game = "Ricochet (Classic)",
				AppID = "30",
				ExeName = "hlds.exe",
				RequiredArgs = "-console -game ricochet +port {port} +maxplayers {MaxPlayers} +map {map} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27015,
				Maps = ["rc_arena", "rc_deathmatch"]
			},
			new() {
				Game = "Counter-Strike: Condition Zero",
				AppID = "80",
				ExeName = "hlds.exe",
				RequiredArgs = "-console -game czero +port {port} +maxplayers {MaxPlayers} +map {map} -SteamAppId={steamAppID}",			
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"czero\server.cfg",
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Necesse",
				AppID = "1169650",
				ExeName = "NecesseServer.exe",
				RequiredArgs = "-port {port} -slots {MaxPlayers} -owner \"{adminpass}\" -SteamAppId={steamAppID}",				
				Port = 14159,
				QueryPort = 14159,
				RelativeConfigPath = "server.cfg",
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Zero Hour",
				AppID = "1359090",
				ExeName = "ZeroHourServer.exe",
				RequiredArgs = "-port {port} -queryport {query} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Green Hell",
				AppID = "1145600",
				ExeName = "GreenHellServer.exe",
				RequiredArgs = "-port {port} -queryport {query} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Medieval Dynasty",
				AppID = "1129580",
				ExeName = "MedievalDynastyServer.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "WolfQuest: Anniversary Edition",
				AppID = "1111610",
				ExeName = "WolfQuestServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Double Action: Boogaloo",
				AppID = "317360",
				ExeName = "srcds.exe",
				RequiredArgs = "-game dab -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"dab\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["da_rooftops"]
			},
			new() {
				Game = "The Ship",
				AppID = "2400",
				ExeName = "srcds.exe",
				RequiredArgs = "-game ship -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"ship\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["ship_cabins"]
			},
			new() {
				Game = "Keplerth",
				AppID = "747200",
				ExeName = "KeplerthServer.exe",
				RequiredArgs = "-port {port} -name \"{ServerName}\" -password \"{pass}\" -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				RelativeConfigPath = "config.json",
				Format = ConfigFormat.JSON
			},
			new() {
				Game = "Longvinter",
				AppID = "1908050",
				ExeName = @"Longvinter\Binaries\Win64\LongvinterServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -servername=\"{ServerName}\" -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"Longvinter\Saved\Config\WindowsServer\Game.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Ground Branch",
				AppID = "436320",
				ExeName = @"GroundBranch\Binaries\Win64\GroundBranchServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -ServerName=\"{ServerName}\" -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"GroundBranch\Saved\Config\WindowsServer\Game.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Red Orchestra 2: Heroes of Stalingrad",
				AppID = "212542",
				ExeName = @"Binaries\Win64\ROGame.exe",
				RequiredArgs = "{map}?MaxPlayers={MaxPlayers} -Port={port} -QueryPort={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"ROGame\Config\ROGame.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["TE-Apartments", "TE-Gumrak"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Sengoku Dynasty",
				AppID = "1701460",
				ExeName = "SengokuDynastyServer.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"SengokuDynasty\Saved\Config\WindowsServer\Game.ini",
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Beasts of Bermuda",
				AppID = "1016730",
				ExeName = @"BeastsOfBermuda\Binaries\Win64\BeastsOfBermudaServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"BeastsOfBermuda\Saved\Config\WindowsServer\Game.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "The Isle (Evrima)",
				AppID = "412680",
				ExeName = @"TheIsle\Binaries\Win64\TheIsleServer-Win64-Shipping.exe",
				RequiredArgs = "{map}?Listen?Port={port}?QueryPort={query} -log -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"TheIsle\Saved\Config\WindowsServer\Game.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["Gateway"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Pirates, Vikings, and Knights II",
				AppID = "17570",
				ExeName = "srcds.exe",
				RequiredArgs = "-game pvkii -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"pvkii\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["tw_tortuga", "bt_island"]
			},
			new() {
				Game = "Minimum",
				AppID = "214190",
				ExeName = "MinimumServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Stranded Deep",
				AppID = "313120",
				ExeName = "Stranded Deep Dedicated Server.exe",
				RequiredArgs = "-batchmode -nographics -SteamAppId={steamAppID} -port {port} -servername \"{ServerName}\"",
				Port = 27015,
				QueryPort = 27016,
				RelativeConfigPath = "ServerConfig.json",
				Format = ConfigFormat.JSON,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Osiris: New Dawn",
				AppID = "402710",
				ExeName = "OsirisServer.exe",
				RequiredArgs = "-port {port} -name \"{ServerName}\" -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Hellion",
				AppID = "588210",
				ExeName = "HellionServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				RelativeConfigPath = "GameServer.ini",
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Staxel",
				AppID = "405710",
				ExeName = "Staxel.Server.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 38465,
				QueryPort = 38465,
				RelativeConfigPath = "server.config",
				Format = ConfigFormat.JSON,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Boundless",
				AppID = "324510",
				ExeName = "BoundlessServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.JSON
			},
			new() {
				Game = "Subsistence",
				AppID = "418030",
				ExeName = "SubsistenceServer.exe",
				RequiredArgs = "-port {port} -servername \"{ServerName}\" -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Fountain of Youth",
				AppID = "1202200",
				ExeName = "FoYServer.exe",
				RequiredArgs = "-port {port} -queryport {query} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Rising World",
				AppID = "324080",
				ExeName = "RisingWorldServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 4255,
				QueryPort = 4255,
				RelativeConfigPath = "server.properties",
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Creativerse",
				AppID = "280790",
				ExeName = "CreativerseServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.JSON
			},
			new() {
				Game = "Heat",
				AppID = "656240",
				ExeName = "HeatServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				RelativeConfigPath = "ServerSettings.ini",
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Farming Simulator 19",
				AppID = "787860",
				ExeName = "DedicatedServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 10823,
				QueryPort = 10823,
				RelativeConfigPath = "dedicatedServerConfig.xml",
				Format = ConfigFormat.XML,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Automobilista 2",
				AppID = "1066890",
				ExeName = "AMS2DedicatedServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				RelativeConfigPath = "server_config.json",
				Format = ConfigFormat.JSON
			},
			new() {
				Game = "Assetto Corsa Competizione",
				AppID = "805550",
				ExeName = "accServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 9000,
				QueryPort = 9001,
				RelativeConfigPath = @"cfg\settings.json",
				Format = ConfigFormat.JSON,
				NeedsConfigWarning = true
			},
			new() {
				Game = "rFactor 2",
				AppID = "365960",
				ExeName = "rFactor2Dedicated.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 54297,
				QueryPort = 54297,
				RelativeConfigPath = @"UserData\player\Multiplayer.json",
				Format = ConfigFormat.JSON
			},
			new() {
				Game = "Trackmania (2020)",
				AppID = "2225070",
				ExeName = "TrackmaniaServer.exe",
				RequiredArgs = "/port={port} /dedicated_cfg=dedicated_cfg.txt -SteamAppId={steamAppID}",				
				Port = 2350,
				QueryPort = 3450,
				RelativeConfigPath = "dedicated_cfg.txt",
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "ShootMania Storm",
				AppID = "229870",
				ExeName = "ShootManiaServer.exe",
				RequiredArgs = "/port={port} -SteamAppId={steamAppID}",				
				Port = 2350,
				QueryPort = 3450,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Painkiller: Hell & Damnation",
				AppID = "214870",
				ExeName = "PainkillerServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Serious Sam 3: BFE",
				AppID = "41070",
				ExeName = "Sam3_DedicatedServer.exe",
				RequiredArgs = "+port {port} -SteamAppId={steamAppID}",			
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Quake 4",
				AppID = "2210",
				ExeName = "Quake4Server.exe",
				RequiredArgs = "+set net_port {port} -SteamAppId={steamAppID}",				
				Port = 28004,
				QueryPort = 28004,
				RelativeConfigPath = "server.cfg",
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Quake Live",
				AppID = "282440",
				ExeName = "QuakeLiveServer.exe",
				RequiredArgs = "+set net_port {port} -SteamAppId={steamAppID}",				
				Port = 27960,
				QueryPort = 27960,
				RelativeConfigPath = "server.cfg",
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Medieval Engineers",
				AppID = "367970",
				ExeName = @"DedicatedServer64\MedievalEngineersDedicated.exe",
				RequiredArgs = "-console -noconsole -path {map} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				RelativeConfigPath = @"Instance\MedievalEngineers-Dedicated.cfg",
				Format = ConfigFormat.XML,
				Maps = ["Default"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "BattleBit Remastered",
				AppID = "1152000",
				ExeName = "BattleBitDedicated.exe",
				RequiredArgs = "-port {port} -queryport {query} -servername \"{ServerName}\" -SteamAppId={steamAppID}",				
				Port = 30000,
				QueryPort = 30001,
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Euro Truck Simulator 2",
				AppID = "1948160",
				ExeName = @"bin\win_x64\eurotrucks2.exe",
				RequiredArgs = "-dedicated -server_config server_config.sii -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				RelativeConfigPath = "server_config.sii",
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Natural Selection 2",
				AppID = "4920",
				ExeName = "Server.exe",
				RequiredArgs = "-name \"{ServerName}\" -port {port} -limit {MaxPlayers} -map {map} -config_path \"config\" -SteamAppId={steamAppID}",				Port = 27015,
				QueryPort = 27016,
				RelativeConfigPath = @"config\ServerConfig.json",
				Format = ConfigFormat.JSON,
				Maps = ["ns2_docking", "ns2_summit"]
			},
			new() {
				Game = "Return to Moria",
				AppID = "3349480",
				ExeName = @"Moria\Binaries\Win64\MoriaServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Reign of Kings",
				AppID = "344760",
				ExeName = "ROK.exe",
				RequiredArgs = "-batchmode -nographics -SteamAppId={steamAppID} -port {port}",
				Port = 7350,
				QueryPort = 7350,
				RelativeConfigPath = @"Configuration\ServerSettings.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["Stormhold"]
			},
			new() {
				Game = "Outlaws of the Old West",
				AppID = "955060",
				ExeName = @"Outlaws\Binaries\Win64\OutlawsServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"Outlaws\Saved\Config\WindowsServer\Game.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["Outlaws"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Squad 44 (Post Scriptum)",
				AppID = "746200",
				ExeName = "PostScriptumServer.exe",
				RequiredArgs = "Port={port} QueryPort={query} FIXEDMAXPLAYERS={MaxPlayers} +map {map} -SteamAppId={steamAppID}",				
				Port = 7787,
				QueryPort = 27165,
				RelativeConfigPath = @"PostScriptum\ServerConfig\Server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["Carentan_AAS_v1"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "SCP: Pandemic",
				AppID = "1402320",
				ExeName = @"Pandemic\Binaries\Win64\PandemicServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
				new() {
				Game = "Deathmatch Classic",
				AppID = "90",
				ExeName = "hlds.exe",
				RequiredArgs = "-console -game dmc +port {port} +maxplayers {MaxPlayers} +map {map} -SteamAppId={steamAppID}",				
					Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"dmc\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["dmc_dm4", "dmc_dm6"]
			},
			new() {
				Game = "Half-Life: Opposing Force",
				AppID = "90",
				ExeName = "hlds.exe",
				RequiredArgs = "-console -game gearbox +port {port} +maxplayers {MaxPlayers} +map {map} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"gearbox\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["op4dm1"]
			},
			new() {
				Game = "Aliens vs Predator (2010)",
				AppID = "34120",
				ExeName = "AvP_DedicatedServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27015,
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Alien Swarm",
				AppID = "635",
				ExeName = "srcds.exe",
				RequiredArgs = "-game swarm -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"swarm\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["asi-jac1-landingbay_01"]
			},
			new() {
				Game = "Dark Messiah of Might & Magic",
				AppID = "2145",
				ExeName = "srcds.exe",
				RequiredArgs = "-game mm -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"mm\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["dm_castle"]
			},
			new() {
				Game = "Darkest Hour: Europe '44-'45",
				AppID = "1290",
				ExeName = "RedOrchestra.exe",
				RequiredArgs = "server {map}?Listen -port={port} -SteamAppId={steamAppID}",				
				Port = 7757,
				QueryPort = 7758,
				Maps = ["DH-Stonne"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Fortress Forever",
				AppID = "329710",
				ExeName = "srcds.exe",
				RequiredArgs = "-game fortressforever -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"fortressforever\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["ff_2fort", "ff_well"]
			},
			new() {
				Game = "Nuclear Dawn",
				AppID = "111710",
				ExeName = "srcds.exe",
				RequiredArgs = "-game nucleardawn -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"nucleardawn\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["nd_hydro", "nd_metro"]
			},
			new() {
				Game = "Red Orchestra: Ostfront 41-45",
				AppID = "1203",
				ExeName = "RedOrchestra.exe",
				RequiredArgs = "server {map}?Listen -port={port} -SteamAppId={steamAppID}",				
				Port = 7757,
				QueryPort = 7758,
				Maps = ["RO-Arad"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "SiN 1",
				AppID = "1314",
				ExeName = "sin.exe",
				RequiredArgs = "+set dedicated 1 +set port {port} +map {map} -SteamAppId={steamAppID}",				
				Port = 28001,
				QueryPort = 28001,
				Maps = ["sin_dm1"]
			},
			new() {
				Game = "Military Conflict: Vietnam",
				AppID = "1136190",
				ExeName = "srcds.exe",
				RequiredArgs = "-game vietnam -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"vietnam\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["vdm_hue"]
			},
			new() {
				Game = "Monday Night Combat",
				AppID = "63220",
				ExeName = "MNC.exe",
				RequiredArgs = "server {map}?Listen -port={port} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				Maps = ["MNC-Crossfire"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "NS2: Combat",
				AppID = "313900",
				ExeName = "Server.exe",
				RequiredArgs = "-port {port} -name \"{ServerName}\" -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.JSON,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Operation: Harsh Doorstop",
				AppID = "733530",
				ExeName = @"HarshDoorstop\Binaries\Win64\HarshDoorstopServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -ServerName=\"{ServerName}\" -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"HarshDoorstop\Saved\Config\WindowsServer\Engine.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "No One Survived",
				AppID = "1963390",
				ExeName = "NoOneSurvivedServer.exe",
				RequiredArgs = "-port {port} -queryport {query} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "The Mean Greens - Plastic Warfare",
				AppID = "421670",
				ExeName = @"MeanGreens\Binaries\Win64\MeanGreensServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "America's Army: Proving Grounds",
				AppID = "203300",
				ExeName = @"AAGame\Binaries\Win64\AAGameServer.exe",
				RequiredArgs = "server {map}?Port={port} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				Maps = ["AA_Downtown"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "NEOTOKYO",
				AppID = "313600",
				ExeName = "srcds.exe",
				RequiredArgs = "-game neotokyo -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"neotokyo\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["nt_engage_ctg"]
			},
			new() {
				Game = "Just Cause 2: Multiplayer",
				AppID = "261140",
				ExeName = "JcmpServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 7777,
				RelativeConfigPath = "config.lua",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Out of Reach",
				AppID = "406800",
				ExeName = "OutOfReachServer.exe",
				RequiredArgs = "-batchmode -nographics -SteamAppId={steamAppID} -port {port}",
				Port = 27015,
				QueryPort = 27016,
				RelativeConfigPath = "ServerConfig.json",
				Format = ConfigFormat.JSON,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Reflex Arena",
				AppID = "328070",
				ExeName = "ReflexServer.exe",
				RequiredArgs = "+port {port} +map {map} -SteamAppId={steamAppID}",				
				Port = 25787,
				QueryPort = 25787,
				Maps = ["dm1"]
			},
			new() {
				Game = "SiN Episodes: Emergence",
				AppID = "1300",
				ExeName = "srcds.exe",
				RequiredArgs = "-game emerge -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"emerge\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["emerge_arena"]
			},
			new() {
				Game = "Toxikk",
				AppID = "324810",
				ExeName = @"UDKGame\Binaries\Win64\ToxikkServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"UDKGame\Config\UDKEngine.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Unreal Tournament 2004",
				AppID = "13230",
				ExeName = @"System\ucc.exe",
				RequiredArgs = "server {map}?game=XGame.XDeathmatch?AdminName=admin?AdminPassword=admin -port={port} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 7778,
				RelativeConfigPath = @"System\UT2004.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["DM-Rankin", "DM-Deck17"]
			},
			new() {
				Game = "Unreal Tournament 3",
				AppID = "13210",
				ExeName = @"Binaries\UT3.exe",
				RequiredArgs = "server {map}?MaxPlayers={MaxPlayers} -port={port} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"UTGame\Config\UTEngine.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["DM-Deck"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Viscera Cleanup Detail",
				AppID = "246900",
				ExeName = @"Binaries\Win64\VisceraServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Blackwake",
				AppID = "420290",
				ExeName = "BlackwakeServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Serious Sam HD: The First Encounter",
				AppID = "41000",
				ExeName = "SeriousSamHD.exe",
				RequiredArgs = "+port {port} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Serious Sam HD: The Second Encounter",
				AppID = "41014",
				ExeName = "SeriousSamHD_TSE.exe",
				RequiredArgs = "+port {port} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Sanctum 2",
				AppID = "210770",
				ExeName = @"UDKGame\Binaries\Win64\Sanctum2Server.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Beyond the Wire",
				AppID = "1055540",
				ExeName = @"BeyondTheWire\Binaries\Win64\BeyondTheWireServer-Win64-Shipping.exe",
				RequiredArgs = "Port={port} QueryPort={query} FIXEDMAXPLAYERS={MaxPlayers} +map {map} -SteamAppId={steamAppID}",				
				Port = 7787,
				QueryPort = 27165,
				RelativeConfigPath = @"BeyondTheWire\ServerConfig\Server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["Ancre_AAS_v1"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "War of Rights",
				AppID = "614050",
				ExeName = "WarOfRightsServer.exe",
				RequiredArgs = "-port {port} -queryport {query} -servername \"{ServerName}\" -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Colony Survival",
				AppID = "366090",
				ExeName = "ColonySurvivalServer.exe",
				RequiredArgs = "+server.port {port} +server.name \"{ServerName}\" -SteamAppId={steamAppID}",				
				Port = 27016,
				QueryPort = 27016,
				RelativeConfigPath = "config.json",
				Format = ConfigFormat.JSON,
				NeedsConfigWarning = true
			},
			new() {
				Game = "BrainBread 2",
				AppID = "457870",
				ExeName = "srcds.exe",
				RequiredArgs = "-game brainbread2 -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"brainbread2\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["bb_creek"]
			},
			new() {
				Game = "Synergy",
				AppID = "17520",
				ExeName = "srcds.exe",
				RequiredArgs = "-game synergy -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"synergy\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["syn_d1_trainstation_01"]
			},
			new() {
				Game = "Sanctum",
				AppID = "91600",
				ExeName = @"UDKGame\Binaries\Win64\SanctumServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "The Haunted: Hell's Reach",
				AppID = "43190",
				ExeName = @"UDKGame\Binaries\Win64\HauntedServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Aliens versus Predator Classic 2000",
				AppID = "3730",
				ExeName = "AvP.exe",
				RequiredArgs = "-dedicated -port {port} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27015,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Farming Simulator 17",
				AppID = "447020",
				ExeName = "DedicatedServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 10823,
				QueryPort = 10823,
				RelativeConfigPath = "dedicatedServerConfig.xml",
				Format = ConfigFormat.XML,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Farming Simulator 15",
				AppID = "313290",
				ExeName = "DedicatedServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 10823,
				QueryPort = 10823,
				RelativeConfigPath = "dedicatedServerConfig.xml",
				Format = ConfigFormat.XML,
				NeedsConfigWarning = true
			},
			new() {
				Game = "The Wild Eight",
				AppID = "526160",
				ExeName = "TheWildEightServer.exe",
				RequiredArgs = "-port {port} -queryport {query} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Natural Selection (Retail)",
				AppID = "70",
				ExeName = "hlds.exe",
				RequiredArgs = "-console -game ns +port {port} +maxplayers {MaxPlayers} +map {map} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"ns\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["ns_mines"]
			},
			new() {
				Game = "Natural Selection",
				AppID = "70",
				ExeName = "hlds.exe",
				RequiredArgs = "-console -game ns +port {port} +maxplayers {MaxPlayers} +map {map} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"ns\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["ns_mines"]
			},
			new() {
				Game = "Codename CURE",
				AppID = "355180",
				ExeName = "srcds.exe",
				RequiredArgs = "-game cure -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"cure\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["cure_bunker"]
			},
			new() {
				Game = "Age of Chivalry",
				AppID = "17510",
				ExeName = "srcds.exe",
				RequiredArgs = "-game ageofchivalry -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"ageofchivalry\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["aoc_siege"]
			},
			new() {
				Game = "Zombie Master",
				AppID = "299000",
				ExeName = "srcds.exe",
				RequiredArgs = "-game zombiemaster -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"zombiemaster\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["zm_factory"]
			},
			new() {
				Game = "Empires Mod",
				AppID = "17740",
				ExeName = "srcds.exe",
				RequiredArgs = "-game empires -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"empires\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["emp_canyon"]
			},
			new() {
				Game = "Resistance and Liberation",
				AppID = "17530",
				ExeName = "srcds.exe",
				RequiredArgs = "-game reslib -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"reslib\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["rnl_st_mere_eglise"]
			},
			new() {
				Game = "E.Y.E: Divine Cybermancy",
				AppID = "91700",
				ExeName = "srcds.exe",
				RequiredArgs = "-game eye -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"eye\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["cc_temple"]
			},
			new() {
				Game = "The Hidden: Source",
				AppID = "220",
				ExeName = "srcds.exe",
				RequiredArgs = "-game hidden -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"hidden\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["hdn_exec"]
			},
			new() {
				Game = "S.T.A.L.K.E.R.: Shadow of Chernobyl",
				AppID = "4500",
				ExeName = @"bin\XR_3DA.exe",
				RequiredArgs = "-start server({map}/deathmatch) -port {port} -SteamAppId={steamAppID}",				
				Port = 5445,
				QueryPort = 5445,
				Maps = ["mp_agroprom"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "S.T.A.L.K.E.R.: Call of Pripyat",
				AppID = "41700",
				ExeName = @"bin\xrEngine.exe",
				RequiredArgs = "-start server({map}/deathmatch) -port {port} -SteamAppId={steamAppID}",				
				Port = 5445,
				QueryPort = 5445,
				Maps = ["mp_pool"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "S.T.A.L.K.E.R.: Clear Sky",
				AppID = "20510",
				ExeName = @"bin\xrEngine.exe",
				RequiredArgs = "-start server({map}/deathmatch) -port {port} -SteamAppId={steamAppID}",				
				Port = 5445,
				QueryPort = 5445,
				Maps = ["mp_rembase"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Scrap Mechanic",
				AppID = "1366820",
				ExeName = "ScrapMechanic.exe",
				RequiredArgs = "-server -port {port} -maxplayers {MaxPlayers} -password \"{pass}\" -SteamAppId={steamAppID}",				
				Port = 50000,
				QueryPort = 50001,
				RelativeConfigPath = "ServerConfig.json",
				Format = ConfigFormat.JSON,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Terraria (tModLoader)",
				AppID = "1281930",
				ExeName = "tModLoaderServer.exe",
				RequiredArgs = "-port {port} -players {MaxPlayers} -world \"{map}\" -password \"{pass}\" -motd \"{ServerName}\" -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 7777,
				RelativeConfigPath = "serverconfig.txt",
				Format = ConfigFormat.StandardINI,
				Maps = ["World1.wld"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Counter-Strike: Global Offensive (Legacy)",
				AppID = "740",
				ExeName = "srcds.exe",
				RequiredArgs = "-game csgo -console -usercon +game_type 0 +game_mode 1 +mapgroup mg_active +map {map} -port {port} +sv_password \"{pass}\" +hostname \"{ServerName}\" {rcon} -SteamAppId={steamAppID}",				
				RconSyntax = "+rcon_password \"{rcon_pass}\"",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"csgo\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["de_dust2", "de_mirage", "de_inferno"]
			},
			new() {
				Game = "Arma 2: Operation Arrowhead",
				AppID = "33390",
				ExeName = "arma2oaserver.exe",
				RequiredArgs = "-port={port} -name=\"{ServerName}\" -config=server.cfg -world={map} -SteamAppId={steamAppID}",				
				Port = 2302,
				QueryPort = 2303,
				RelativeConfigPath = "server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["Takistan"]
			},
			new() {
				Game = "Arma 2: DayZ Mod",
				AppID = "224900",
				ExeName = "arma2oaserver.exe",
				RequiredArgs = "-mod=@DayZ -port={port} -name=\"{ServerName}\" -config=server.cfg -world={map} -SteamAppId={steamAppID}",				
				Port = 2302,
				QueryPort = 2303,
				RelativeConfigPath = "server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["Chernarus"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Brink",
				AppID = "22350",
				ExeName = "brink_ded.exe",
				RequiredArgs = "+set net_port {port} +exec server.cfg +map {map} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"base\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["cc_reactor"]
			},
			new() {
				Game = "Dirty Bomb",
				AppID = "333930",
				ExeName = @"ShooterGame\Binaries\Win64\ShooterGameServer.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"ShooterGame\Saved\Config\WindowsServer\ShooterEngine.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Mortal Online 2",
				AppID = "1435650",
				ExeName = @"MortalOnline2\Binaries\Win64\MortalOnline2Server-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -ServerName=\"{ServerName}\" -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"MortalOnline2\Saved\Config\WindowsServer\Game.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "XERA: Survival",
				AppID = "873740",
				ExeName = @"Xera\Binaries\Win64\XeraServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -ServerName=\"{ServerName}\" -SteamAppId={steamAppID}",				
				Port = 8000,
				QueryPort = 27015,
				RelativeConfigPath = @"Xera\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Survive the Nights",
				AppID = "1353340",
				ExeName = "SurviveTheNights.exe",
				RequiredArgs = "-batchmode -nographics -SteamAppId={steamAppID} -port {port} -servername \"{ServerName}\" -password \"{pass}\"",
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = "ServerConfig.json",
				Format = ConfigFormat.JSON,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Desolate",
				AppID = "1205160",
				ExeName = @"Desolate\Binaries\Win64\DesolateServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -ServerName=\"{ServerName}\" -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"Desolate\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Savage Lands",
				AppID = "397340",
				ExeName = "SavageLandsServer.exe",
				RequiredArgs = "-batchmode -nographics -SteamAppId={steamAppID} -port {port} -servername \"{ServerName}\"",
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = "ServerConfig.json",
				Format = ConfigFormat.JSON,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Fragmented",
				AppID = "441790",
				ExeName = @"Fragmented\Binaries\Win64\FragmentedServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"Fragmented\Saved\Config\WindowsServer\Game.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "GRAV",
				AppID = "400640",
				ExeName = @"CAG\Binaries\Win64\CAGServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -ServerName=\"{ServerName}\" -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"CAG\Saved\Config\WindowsServer\Game.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Eden Star",
				AppID = "431560",
				ExeName = @"EdenGame\Binaries\Win64\EdenGameServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"EdenGame\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Rokh",
				AppID = "462440",
				ExeName = @"Rokh\Binaries\Win64\RokhServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"Rokh\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Outpost Zero",
				AppID = "653660",
				ExeName = @"OutpostZero\Binaries\Win64\OutpostZeroServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"OutpostZero\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Rend",
				AppID = "550650",
				ExeName = @"Rend\Binaries\Win64\RendServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"Rend\Saved\Config\WindowsServer\Game.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Night of the Dead",
				AppID = "1377380",
				ExeName = @"LF\Binaries\Win64\LFServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -ServerName=\"{ServerName}\" -Password=\"{pass}\" -SteamAppId={steamAppID}",				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"LF\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Tower Unite",
				AppID = "394690",
				ExeName = @"TowerUnite\Binaries\Win64\TowerServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"TowerUnite\Saved\Config\WindowsServer\TowerGame.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Witch It",
				AppID = "559650",
				ExeName = @"WitchIt\Binaries\Win64\WitchItServer-Win64-Shipping.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"WitchIt\Saved\Config\WindowsServer\GameUserSettings.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Shattered Skies",
				AppID = "439860",
				ExeName = @"ShatteredSkies\Binaries\Win64\ShatteredSkiesServer.exe",
				RequiredArgs = "-log -port={port} -queryport={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Chivalry: Medieval Warfare",
				AppID = "219640",
				ExeName = @"UDKGame\Binaries\Win64\UDK.exe",
				RequiredArgs = "server {map}?steamsockets?Port={port}?QueryPort={query} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"UDKGame\Config\PCServer-UDKGame.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["AOCTD-Arena_P"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Farming Simulator 22",
				AppID = "1248130",
				ExeName = "DedicatedServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 10823,
				QueryPort = 10823,
				RelativeConfigPath = "dedicatedServerConfig.xml",
				Format = ConfigFormat.XML,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Dinkum",
				AppID = "1062520",
				ExeName = "DinkumServer.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Interstellar Rift",
				AppID = "804360",
				ExeName = "IR.exe",
				RequiredArgs = "-server -port {port} -SteamAppId={steamAppID}",				
				Port = 6124,
				QueryPort = 6124,
				RelativeConfigPath = "server.ini",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Golf With Your Friends",
				AppID = "431240",
				ExeName = "GWYF_Server.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Ricochet",
				AppID = "60",
				ExeName = "hlds.exe",
				RequiredArgs = "-console -game ricochet +port {port} +maxplayers {MaxPlayers} +map {map} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"ricochet\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["rc_arena"]
			},
			new() {
				Game = "Rust",
				AppID = "258550",
				ExeName = "RustDedicated.exe",
				RequiredArgs = "-batchmode -nographics +server.ip 0.0.0.0 +server.port {port} +server.queryport {query} +app.port {app_port} +server.level \"{map}\" +server.seed {seed} +server.worldsize 4000 +server.maxplayers {MaxPlayers} +server.hostname \"{ServerName}\" +server.identity \"{Identity}\" +server.pve {mode} {rcon} -SteamAppId={steamAppID} -logfile \"server_log.txt\"",
				RconSyntax = "+rcon.port {rcon_port} +rcon.password \"{rcon_pass}\" +rcon.web 1",
				Port = 28015,
				QueryPort = 28016,
				RelativeConfigPath = @"server\{Identity}\cfg\server.cfg",
				Format = ConfigFormat.Space,
				Maps = ["Procedural Map", "Barren", "HapisIsland", "CraggyIsland"],
				GameModes = ["PVP", "PVE"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "DayZ (Experimental)",
				AppID = "1042420",
				ExeName = "DayZServer_x64.exe",
				RequiredArgs = "-config=serverDZ.cfg -port={port} -name=\"{ServerName}\" -SteamAppId={steamAppID}",				
				Port = 2302,
				QueryPort = 27016,
				RelativeConfigPath = "serverDZ.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["ChernarusPlus"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Conan Exiles (TestLive)",
				AppID = "443030",
				ExeName = @"ConanSandbox\Binaries\Win64\ConanSandboxServer.exe",
				RequiredArgs = "-beta testlive {map}?Listen?MaxPlayers={MaxPlayers}?ServerName=\"{ServerName}\"?ServerPassword=\"{pass}\"?AdminPassword=\"{adminpass}\" -Port={port} -QueryPort={query} -nosteam -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 27015,
				Maps = ["TheExiledLands"],
				RelativeConfigPath = @"ConanSandbox\Saved\Config\WindowsServer\ServerSettings.ini",
				Format = ConfigFormat.StandardINI,
				GameModes = ["PVE", "PVP"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Space Engineers (Crossplay)",
				AppID = "298740",
				ExeName = @"DedicatedServer64\SpaceEngineersDedicated.exe",
				RequiredArgs = "-noconsole -ignorelastsession -port {port} -SteamAppId={steamAppID}",
				Port = 27016,
				QueryPort = 27016,
				GameModes = ["Creative"],
				RelativeConfigPath = @"Instance\SpaceEngineers-Dedicated.cfg",
				Format = ConfigFormat.XML,
				Maps = ["StarSystem"],
				NeedsConfigWarning = true
				},
			new() {
				Game = "Mindustry",
				AppID = "1127400",
				ExeName = "Mindustry.exe",
				RequiredArgs = "-server -SteamAppId={steamAppID}",				
				Port = 6567,
				QueryPort = 6567,
				RelativeConfigPath = @"config\server-settings.json",
				Format = ConfigFormat.JSON,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Mount & Blade: Warband",
				AppID = "48700",
				ExeName = "mb_warband_dedicated.exe",
				RequiredArgs = "-r Sample_Battle.txt -m Native -port {port} -SteamAppId={steamAppID}",				
				Port = 7240,
				QueryPort = 7240,
				RelativeConfigPath = "Sample_Battle.txt",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Cry of Fear",
				AppID = "225420",
				ExeName = "hlds.exe",
				RequiredArgs = "-console -game cryoffear +port {port} +maxplayers {MaxPlayers} +map {map} -SteamAppId={steamAppID}",			
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"cryoffear\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["cof_campaign_01"]
			},
			new() {
				Game = "The Ship: Murder Party",
				AppID = "2400",
				ExeName = "srcds.exe",
				RequiredArgs = "-game ship -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"ship\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["bataleon"]
			},
			new() {
				Game = "Blade Symphony",
				AppID = "225600",
				ExeName = "srcds.exe",
				RequiredArgs = "-game berimbau -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"berimbau\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["duel_monastery"]
			},
			new() {
				Game = "Dystopia",
				AppID = "17580",
				ExeName = "srcds.exe",
				RequiredArgs = "-game dystopia -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"dystopia\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["dys_vaccine"]
			},
			new() {
				Game = "Half-Life 2: Capture the Flag",
				AppID = "17550",
				ExeName = "srcds.exe",
				RequiredArgs = "-game hl2ctf -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"hl2ctf\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["ctf_2fort"]
			},
			new() {
				Game = "Zombie Master: Reborn",
				AppID = "35140",
				ExeName = "srcds.exe",
				RequiredArgs = "-game zombiemaster -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"zombiemaster\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["zm_farm"]
			},
			new() {
				Game = "Action: Source",
				AppID = "211720",
				ExeName = "srcds.exe",
				RequiredArgs = "-game action -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"action\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["as_city"]
			},
			new() {
				Game = "Murder Miners",
				AppID = "274900",
				ExeName = "MurderMinersServer.exe",
				RequiredArgs = "-port {port} -name \"{ServerName}\" -maxplayers {MaxPlayers} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27015,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Lead and Gold: Gangs of the Wild West",
				AppID = "42120",
				ExeName = "lag_server.exe",
				RequiredArgs = "-port {port} -servername \"{ServerName}\" -SteamAppId={steamAppID}",			
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.StandardINI
			},
			new() {
				Game = "Orion: Prelude",
				AppID = "104900",
				ExeName = @"UDKGame\Binaries\Win64\OrionGameServer.exe",
				RequiredArgs = "{map}?steamsockets?Port={port}?QueryPort={query} -SteamAppId={steamAppID}",		
				Port = 7777,
				QueryPort = 27015,
				RelativeConfigPath = @"UDKGame\Config\PCServer-UDKGame.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["c1m1_survive"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Saurian",
				AppID = "587450",
				ExeName = "SaurianServer.exe",
				RequiredArgs = "-batchmode -nographics -SteamAppId={steamAppID} -port {port}",
				Port = 7777,
				QueryPort = 27015,
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Factorio (Experimental)",
				AppID = "428200",
				ExeName = @"bin\x64\factorio.exe",
				RequiredArgs = "-beta experimental --start-server {map}.zip --server-settings data\\server-settings.json --port {port} -SteamAppId={steamAppID}",			
				Port = 34197,
				QueryPort = 34197,
				RelativeConfigPath = @"data\server-settings.json",
				Format = ConfigFormat.JSON,
				Maps = ["FactorioWorld"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Project Zomboid (Beta)",
				AppID = "380870",
				ExeName = "StartServer64.bat",
				RequiredArgs = "-beta b41multiplayer -port {port} -servername \"{Identity}\" -adminpassword \"{adminpass}\" -SteamAppId={steamAppID}",				
				Port = 16261,
				QueryPort = 16262,
				RelativeConfigPath = @"Zomboid\Server\{Identity}.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["Muldraugh, KY"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "The Isle (Legacy)",
				AppID = "412680",
				ExeName = @"TheIsle\Binaries\Win64\TheIsleServer-Win64-Shipping.exe",
				RequiredArgs = "-beta legacy {map}?Listen?ServerName=\"{ServerName}\"?ServerPassword=\"{pass}\"?Port={port}?QueryPort={query} -log -SteamAppId={steamAppID}",				
				RelativeConfigPath = @"TheIsle\Saved\Config\WindowsServer\Game.ini",
				Format = ConfigFormat.StandardINI,
				Port = 7777,
				QueryPort = 27015,
				Maps = ["V3"],
				NeedsConfigWarning = true
			},
			new() {
				Game = "Barotrauma (Unstable)",
				AppID = "1022710",
				ExeName = "DedicatedServer.exe",
				RequiredArgs = "-beta unstable -port {port} -queryport {query} -name \"{ServerName}\" -SteamAppId={steamAppID}",			
				Port = 27015,
				QueryPort = 27016,
				RelativeConfigPath = "serversettings.xml",
				Format = ConfigFormat.XML,
				Maps = ["Campaign"]
			},
			new() {
				Game = "Palworld (Experimental)",
				AppID = "2394010",
				ExeName = "Pal\\Binaries\\Win64\\PalServer-Win64-Shipping.exe",
				RequiredArgs = "EpicApp=PalServer -useperfthreads -NoAsyncLoadingThread -UseMultithreadForDS -port={port} -queryport={query} -players={MaxPlayers} -ServerName=\"{ServerName}\" -ServerPassword=\"{pass}\" -AdminPassword=\"{adminpass}\" -SteamAppId={steamAppID}",
				RelativeConfigPath = "Pal\\Saved\\Config\\WindowsServer\\PalWorldSettings.ini",
				Port = 8211,
				QueryPort = 27015,
				Maps = ["DefaultWorld"],
				Format = ConfigFormat.Palworld,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Counter-Strike 1.6",
				AppID = "90",
				ExeName = "hlds.exe",
				RequiredArgs = "-console -game cstrike +port {port} +maxplayers {MaxPlayers} +map {map} -SteamAppId={steamAppID}",			
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"cstrike\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["de_dust2", "cs_assault"]
			},
			new() {
				Game = "Day of Defeat",
				AppID = "90",
				ExeName = "hlds.exe",
				RequiredArgs = "-console -game dod +port {port} +maxplayers {MaxPlayers} +map {map} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"dod\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["dod_avalanche"]
			},
			new() {
				Game = "Half-Life: Blue Shift",
				AppID = "90",
				ExeName = "hlds.exe",
				RequiredArgs = "-console -game bshift +port {port} +maxplayers {MaxPlayers} +map {map} -SteamAppId={steamAppID}",			
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"bshift\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["ba_yard"]
			},
			new() {
				Game = "GoldenEye: Source",
				AppID = "244310",
				ExeName = "srcds.exe",
				RequiredArgs = "-game gesource -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"gesource\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["ge_facility"]
			},
			new() {
				Game = "Star Wars Jedi Knight II: Jedi Outcast",
				AppID = "6020",
				ExeName = @"GameData\jk2ded.exe",
				RequiredArgs = "+set dedicated 2 +set net_port {port} +exec server.cfg -SteamAppId={steamAppID}",				
				Port = 28070,
				QueryPort = 28070,
				RelativeConfigPath = @"GameData\base\server.cfg",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "OpenTTD",
				AppID = "1536610",
				ExeName = "openttd.exe",
				RequiredArgs = "-D 0.0.0.0:{port} -SteamAppId={steamAppID}",				
				Port = 3979,
				QueryPort = 3979,
				RelativeConfigPath = "openttd.cfg",
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Halo: The Master Chief Collection",
				AppID = "1164850",
				ExeName = "mcctcnd.exe",
				RequiredArgs = "-port {port} -SteamAppId={steamAppID}",				
				Port = 27015,
				QueryPort = 27016,
				Format = ConfigFormat.StandardINI,
				NeedsConfigWarning = true
			},
			new() {
				Game = "Obsidian Conflict",
				AppID = "31580",
				ExeName = "srcds.exe",
				RequiredArgs = "-game obsidian -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"obsidian\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["oc_harvest"]
			},
			new() {
				Game = "Bloody Good Time",
				AppID = "2450",
				ExeName = "srcds.exe",
				RequiredArgs = "-game bgt -console -port {port} -SteamAppId={steamAppID} +maxplayers {MaxPlayers} +map {map}",
				Port = 27015,
				QueryPort = 27015,
				RelativeConfigPath = @"bgt\cfg\server.cfg",
				Format = ConfigFormat.StandardINI,
				Maps = ["bgt_casino"]
			},
			new() {
				Game = "Unreal Tournament (1999)",
				AppID = "13240",
				ExeName = @"System\ucc.exe",
				RequiredArgs = "server {map}?game=Botpack.DeathMatchPlus -port={port} -SteamAppId={steamAppID}",				
				Port = 7777,
				QueryPort = 7778,
				RelativeConfigPath = @"System\UnrealTournament.ini",
				Format = ConfigFormat.StandardINI,
				Maps = ["DM-Morpheus"]
			},
			new() {
				Game = "Factorio (Space Age)",
				AppID = "428200",
				ExeName = @"bin\x64\factorio.exe",
				RequiredArgs = "--start-server {map}.zip --server-settings data\\server-settings.json --port {port} -SteamAppId={steamAppID}",				
				Port = 34197,
				QueryPort = 34197,
				RelativeConfigPath = @"data\server-settings.json",
				Format = ConfigFormat.JSON,
				Maps = ["SpaceAgeWorld"],
				NeedsConfigWarning = true
			},
		];

		public static IReadOnlyList<GameInfo> GetGameList()
		{
			return games;
		}

		public static GameInfo? GetGame(string gameName)
		{
			return games.FirstOrDefault(g => g.Game.Equals(gameName, StringComparison.OrdinalIgnoreCase));
		}

		public class PostInstallStep
		{
			// Can be "CopySteamDlls" or "CreateFile"
			public string ActionType { get; init; } = "";

			// Where the file goes (e.g., "WS\Binaries\Win64" or "DSSetings.txt")
			public string TargetPath { get; init; } = "";

			// The raw text to write into the file (Only used for "CreateFile")
			public string FileContent { get; init; } = "";
		}
	}
}