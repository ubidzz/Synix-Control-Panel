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
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using Synix_Control_Panel.SynixEngine;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Synix_Control_Panel.SynixApp.ServerHandler
{
	public static class GameFix
	{
		public static bool ManualConfigWasCreated { get; set; } = false;

		public static async Task<bool> PostInstall(GameServer server)
		{
			if (string.IsNullOrWhiteSpace(server.InstallPath) || !Directory.Exists(server.InstallPath))
				return false;

			bool applied = false;
			string publicIp = await Core.Instance.GetPublicIP();
			string localIp = await Core.Instance.GetLocalIP();
			string cleanIdentity = server.ServerName.Replace(" ", "_");

			try
			{
				// --------------------------------------------------------
				// PHASE 1: STEAM API DLL INJECTIONS
				// --------------------------------------------------------
				switch (server.Game)
				{
					case "StarRupture":
						if (CopySteamDLLs(server.InstallPath, @"StarRupture\Binaries\Win64")) applied = true; break;
					case "Soulmask":
						if (CopySteamDLLs(server.InstallPath, @"WS\Binaries\Win64")) applied = true; break;
					case "Palworld":
					case "Palworld (Experimental)":
						if (CopySteamDLLs(server.InstallPath, @"Pal\Binaries\Win64")) applied = true; break;
					case "ARK: Survival Evolved":
					case "ARK: Survival Ascended":
					case "ARK: Survival Ascended (Scorched Earth)":
					case "PixARK":
					case "Atlas":
					case "The Stomping Land":
					case "Dirty Bomb":
						if (CopySteamDLLs(server.InstallPath, @"ShooterGame\Binaries\Win64")) applied = true; break;
					case "Foundry":
						if (CopySteamDLLs(server.InstallPath, "")) applied = true; break;
					case "Astroneer":
						if (CopySteamDLLs(server.InstallPath, @"Astro\Binaries\Win64")) applied = true; break;
					case "Abiotic Factor":
						if (CopySteamDLLs(server.InstallPath, @"AbioticFactor\Binaries\Win64")) applied = true; break;
					case "BATTALION: Legacy":
						if (CopySteamDLLs(server.InstallPath, @"Battalion\Binaries\Win64")) applied = true; break;
					case "Icarus":
						if (CopySteamDLLs(server.InstallPath, @"Icarus\Binaries\Win64")) applied = true; break;
					case "The Front":
						if (CopySteamDLLs(server.InstallPath, @"ProjectWar\Binaries\Win64")) applied = true; break;
					case "Smalland: Survive the Wilds":
						if (CopySteamDLLs(server.InstallPath, @"SMALLAND\Binaries\Win64")) applied = true; break;
					case "Conan Exiles":
					case "Conan Exiles (TestLive)":
						if (CopySteamDLLs(server.InstallPath, @"ConanSandbox\Binaries\Win64")) applied = true; break;
					case "Mordhau":
						if (CopySteamDLLs(server.InstallPath, @"Mordhau\Binaries\Win64")) applied = true; break;
					case "Satisfactory":
						if (CopySteamDLLs(server.InstallPath, @"FactoryGame\Binaries\Win64")) applied = true; break;
					case "Insurgency: Sandstorm":
						if (CopySteamDLLs(server.InstallPath, @"Insurgency\Binaries\Win64")) applied = true; break;
					case "Myth of Empires":
						if (CopySteamDLLs(server.InstallPath, @"MOE\Binaries\Win64")) applied = true; break;
					case "SCUM":
						if (CopySteamDLLs(server.InstallPath, @"SCUM\Binaries\Win64")) applied = true; break;
					case "Hell Let Loose":
						if (CopySteamDLLs(server.InstallPath, @"HLL\Binaries\Win64")) applied = true; break;
					case "Nightingale":
						if (CopySteamDLLs(server.InstallPath, @"NWX\Binaries\Win64")) applied = true; break;
					case "DeadPoly":
						if (CopySteamDLLs(server.InstallPath, @"DeadPoly\Binaries\Win64")) applied = true; break;
					case "Bellwright":
						if (CopySteamDLLs(server.InstallPath, @"Bellwright\Binaries\Win64")) applied = true; break;
					case "The Isle":
					case "The Isle (Evrima)":
					case "The Isle (Legacy)":
						if (CopySteamDLLs(server.InstallPath, @"TheIsle\Binaries\Win64")) applied = true; break;
					case "Grounded":
						if (CopySteamDLLs(server.InstallPath, @"Maine\Binaries\Win64")) applied = true; break;
					case "Day of Dragons":
						if (CopySteamDLLs(server.InstallPath, @"Dragons\Binaries\Win64")) applied = true; break;
					case "Return to Moria":
						if (CopySteamDLLs(server.InstallPath, @"Moria\Binaries\Win64")) applied = true; break;
					case "Citadel: Forged with Fire":
						if (CopySteamDLLs(server.InstallPath, @"Citadel\Binaries\Win64")) applied = true; break;
					case "Outlaws of the Old West":
						if (CopySteamDLLs(server.InstallPath, @"Outlaws\Binaries\Win64")) applied = true; break;
					case "Primal Carnage: Extinction":
						if (CopySteamDLLs(server.InstallPath, @"PrimalCarnage\Binaries\Win64")) applied = true; break;
					case "Ranch Simulator":
						if (CopySteamDLLs(server.InstallPath, @"Ranch_Simulator\Binaries\Win64")) applied = true; break;
					case "Memories of Mars":
						if (CopySteamDLLs(server.InstallPath, @"MemoriesOfMars\Binaries\Win64")) applied = true; break;
					case "Deadside":
						if (CopySteamDLLs(server.InstallPath, @"DeadsideServer\Binaries\Win64")) applied = true; break;
					case "Last Oasis":
						if (CopySteamDLLs(server.InstallPath, @"OasisServer\Binaries\Win64")) applied = true; break;
					case "Dark and Light":
						if (CopySteamDLLs(server.InstallPath, @"DNL\Binaries\Win64")) applied = true; break;
					case "SCP: Pandemic":
						if (CopySteamDLLs(server.InstallPath, @"Pandemic\Binaries\Win64")) applied = true; break;
					case "Gray Zone Warfare":
						if (CopySteamDLLs(server.InstallPath, @"GZW\Binaries\Win64")) applied = true; break;
					case "HumanitZ":
						if (CopySteamDLLs(server.InstallPath, @"HumanitZ\Binaries\Win64")) applied = true; break;
					case "VoidTrain":
						if (CopySteamDLLs(server.InstallPath, @"VoidTrain\Binaries\Win64")) applied = true; break;
					case "Pavlov VR":
						if (CopySteamDLLs(server.InstallPath, @"Pavlov\Binaries\Win64")) applied = true; break;
					case "Longvinter":
						if (CopySteamDLLs(server.InstallPath, @"Longvinter\Binaries\Win64")) applied = true; break;
					case "Ground Branch":
						if (CopySteamDLLs(server.InstallPath, @"GroundBranch\Binaries\Win64")) applied = true; break;
					case "Beasts of Bermuda":
						if (CopySteamDLLs(server.InstallPath, @"BeastsOfBermuda\Binaries\Win64")) applied = true; break;
					case "The Mean Greens - Plastic Warfare":
						if (CopySteamDLLs(server.InstallPath, @"MeanGreens\Binaries\Win64")) applied = true; break;
					case "Operation: Harsh Doorstop":
						if (CopySteamDLLs(server.InstallPath, @"HarshDoorstop\Binaries\Win64")) applied = true; break;
					case "America's Army: Proving Grounds":
						if (CopySteamDLLs(server.InstallPath, @"AAGame\Binaries\Win64")) applied = true; break;
					case "Monday Night Combat":
						if (CopySteamDLLs(server.InstallPath, @"MNC\Binaries\Win64")) applied = true; break;
					case "Chivalry 2":
						if (CopySteamDLLs(server.InstallPath, @"TBL\Binaries\Win64")) applied = true; break;
					case "Depth":
						if (CopySteamDLLs(server.InstallPath, @"Binaries\Win64")) applied = true; break;
					case "Primal Carnage":
						if (CopySteamDLLs(server.InstallPath, @"Binaries\Win32")) applied = true; break;
					case "Toxikk":
					case "Sanctum 2":
					case "Sanctum":
					case "The Haunted: Hell's Reach":
					case "Chivalry: Medieval Warfare":
					case "Orion: Prelude":
						if (CopySteamDLLs(server.InstallPath, @"UDKGame\Binaries\Win64")) applied = true; break;
					case "Beyond the Wire":
						if (CopySteamDLLs(server.InstallPath, @"BeyondTheWire\Binaries\Win64")) applied = true; break;
					case "Mortal Online 2":
						if (CopySteamDLLs(server.InstallPath, @"MortalOnline2\Binaries\Win64")) applied = true; break;
					case "XERA: Survival":
						if (CopySteamDLLs(server.InstallPath, @"Xera\Binaries\Win64")) applied = true; break;
					case "Desolate":
						if (CopySteamDLLs(server.InstallPath, @"Desolate\Binaries\Win64")) applied = true; break;
					case "Fragmented":
						if (CopySteamDLLs(server.InstallPath, @"Fragmented\Binaries\Win64")) applied = true; break;
					case "GRAV":
						if (CopySteamDLLs(server.InstallPath, @"CAG\Binaries\Win64")) applied = true; break;
					case "Eden Star":
						if (CopySteamDLLs(server.InstallPath, @"EdenGame\Binaries\Win64")) applied = true; break;
					case "Rokh":
						if (CopySteamDLLs(server.InstallPath, @"Rokh\Binaries\Win64")) applied = true; break;
					case "Outpost Zero":
						if (CopySteamDLLs(server.InstallPath, @"OutpostZero\Binaries\Win64")) applied = true; break;
					case "Rend":
						if (CopySteamDLLs(server.InstallPath, @"Rend\Binaries\Win64")) applied = true; break;
					case "Night of the Dead":
						if (CopySteamDLLs(server.InstallPath, @"LF\Binaries\Win64")) applied = true; break;
					case "Tower Unite":
						if (CopySteamDLLs(server.InstallPath, @"TowerUnite\Binaries\Win64")) applied = true; break;
					case "Witch It":
						if (CopySteamDLLs(server.InstallPath, @"WitchIt\Binaries\Win64")) applied = true; break;
					case "Shattered Skies":
						if (CopySteamDLLs(server.InstallPath, @"ShatteredSkies\Binaries\Win64")) applied = true; break;
					case "Ready or Not":
						if (CopySteamDLLs(server.InstallPath, @"ReadyOrNot\Binaries\Win64")) applied = true; break;
					case "No One Survived":
						if (CopySteamDLLs(server.InstallPath, @"NoOneSurvived\Binaries\Win64")) applied = true; break;
					case "Killing Floor 2":
					case "Rising Storm 2: Vietnam":
					case "Red Orchestra 2: Heroes of Stalingrad":
					case "Unreal Tournament 3":
					case "Viscera Cleanup Detail":
						if (CopySteamDLLs(server.InstallPath, @"Binaries\Win64")) applied = true; break;
					case "Windrose":
						if (CopySteamDLLs(server.InstallPath, @"R5\Binaries\Win64")) applied = true; break;
					case "Subsistence":
						if (CopySteamDLLs(server.InstallPath, @"Binaries\Win64")) applied = true; break;
					case "Cepheus Protocol":
						if (CopySteamDLLs(server.InstallPath, @"CepheusProtocol\Binaries\Win64")) applied = true; break;
				}

				// --------------------------------------------------------
				// PHASE 2: CONFIGURATION FILE CREATION & PARSING
				// --------------------------------------------------------
				switch (server.Game)
				{
					case "Rust":
						string rustCfg = @"// Synix Custom Rust Configuration
server.hostname ""{ServerName}""
server.password ""{Password}""
server.seed {WorldSeed}
server.worldsize 4000
server.description ""Welcome to {ServerName}!\n\nManaged via Synix Control Panel.""
server.url ""https://github.com/ubidzz/Synix-Control-Panel""
server.saveinterval 300
server.globalchat true
server.secure true
server.radiation true
server.official true";

						string rustCfgPath = Path.Combine("server", cleanIdentity, "cfg", "server.cfg");
						if (CreateGameConfig(server, rustCfgPath, rustCfg, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "StarRupture":
						string srJson = @"{ ""SessionName"": ""{ServerName}"", ""SaveGameInterval"": ""300"", ""StartNewGame"": ""true"", ""LoadSavedGame"": ""false"", ""SaveGameName"": ""AutoSave0.sav"" }";
						if (CreateGameConfig(server, @"StarRupture\Binaries\Win64\DSSettings.txt", srJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Subsistence":
						string subEngineIni = @"[URL]
Port={Port}

[IpDrv.TcpNetDriver]
Port={Port}

[OnlineSubsystemSteamworks.OnlineSubsystemSteamworks]
QueryPort={QueryPort}";

						string subSettingsIni = @"[SubDedicatedServer.SubServerConfig]
ServerName=""{ServerName}""
ServerPassword=""{Password}""
AdminPassword=""{AdminPassword}""
MaxPlayers={MaxPlayers}";

						if (CreateGameConfig(server, @"UDKGame\Config\UDKEngine.ini", subEngineIni, cleanIdentity, localIp, publicIp)) applied = true;
						if (CreateGameConfig(server, @"UDKGame\Config\UDKDedServerSettings.ini", subSettingsIni, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Windrose":
						string windroseJson = @"{ 
    ""Password"": ""{Password}"",
    ""ServerName"": ""{ServerName}"",
    ""MaxPlayerCount"": ""{MaxPlayers}"",
    ""UserSelectedRegion"": """",
    ""P2pProxyAddress"": ""{LocalIP}"",
    ""AutoRestart"": true,
    ""UseDirectConnection"": false,
    ""DirectConnectionServerAddress"": ""{PublicIP}"",
    ""DirectConnectionServerPort"": ""{Port}"",
    ""DirectConnectionProxyAddress"": ""0.0.0.0""
}";
						if (CreateGameConfig(server, @"R5\ServerDescription.json", windroseJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "ASKA":
						string askaJson = @"{ ""ServerName"": ""{ServerName}"", ""Password"": ""{Password}"", ""MaxPlayers"": {MaxPlayers} }";
						if (CreateGameConfig(server, "server_settings.json", askaJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Just Cause 3: Multiplayer":
						string jc3Json = @"{
    ""ServerName"": ""{ServerName}"",
    ""MaxPlayers"": {MaxPlayers},
    ""BindIP"": ""0.0.0.0"",
    ""Port"": {Port}
}";
						if (CreateGameConfig(server, "config.json", jc3Json, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Raft Dedicated Server":
						string raftJson = @"{ ""ServerName"": ""{ServerName}"", ""MaxPlayers"": {MaxPlayers} }";
						if (CreateGameConfig(server, "server_config.json", raftJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Sons Of The Forest":
						string sotfCfg = @"{ ""ServerName"": ""{ServerName}"", ""MaxPlayers"": {MaxPlayers}, ""ServerPlayMode"": ""Normal"" }";
						if (CreateGameConfig(server, @"userdata\dedicated_server.cfg", sotfCfg, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Cepheus Protocol":
						string cpIni = @"[/Script/Engine.GameSession]
MaxPlayers={MaxPlayers}
ServerName=""{ServerName}""
Password=""{Password}""
AdminPassword=""{AdminPassword}""";
						if (CreateGameConfig(server, @"CepheusProtocol\Saved\Config\WindowsServer\Game.ini", cpIni, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Palworld":
					case "Palworld (Experimental)":
						string palIni = @"[/Script/Pal.PalGameWorldSettings]
OptionSettings=(ServerName=""{ServerName}"",ServerPassword=""{Password}"",AdminPassword=""{AdminPassword}"")";
						if (CreateGameConfig(server, @"Pal\Saved\Config\WindowsServer\PalWorldSettings.ini", palIni, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Enshrouded":
						string enshroudedJson = @"{ ""name"": ""{ServerName}"", ""slotCount"": {MaxPlayers} }";
						if (CreateGameConfig(server, "enshrouded_server.json", enshroudedJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Longvinter":
						string longIni = @"[/Script/Longvinter.LongvinterGameMode]
ServerName=""{ServerName}""";
						if (CreateGameConfig(server, @"Longvinter\Saved\Config\WindowsServer\Game.ini", longIni, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Ground Branch":
						string gbIni = @"[/Script/GroundBranch.GBGameMode]
ServerName=""{ServerName}""";
						if (CreateGameConfig(server, @"GroundBranch\Saved\Config\WindowsServer\Game.ini", gbIni, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Stranded Deep":
						string sdJson = @"{ ""ServerName"": ""{ServerName}"", ""MaxPlayers"": {MaxPlayers} }";
						if (CreateGameConfig(server, "ServerConfig.json", sdJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Staxel":
						string staxelJson = @"{ ""ServerName"": ""{ServerName}"", ""MaxPlayers"": {MaxPlayers} }";
						if (CreateGameConfig(server, "server.config", staxelJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Volcanoids":
						string volJson = @"{ ""ServerName"": ""{ServerName}"" }";
						if (CreateGameConfig(server, "server_settings.json", volJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Holdfast: Nations At War":
						string hfTxt = @"server_name {ServerName}";
						if (CreateGameConfig(server, @"Holdfast NaW_Data\StreamingAssets\Config\serverConfig_Core.txt", hfTxt, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "V Rising":
						string vrJson = @"{ ""Name"": ""{ServerName}"" }";
						if (CreateGameConfig(server, @"VRisingServer_Data\StreamingAssets\Settings\ServerHostSettings.json", vrJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "7 Days to Die":
					case "7 Days to Die (Experimental)":
						string sd2dXml = @"<ServerSettings><property name=""ServerName"" value=""{ServerName}""/></ServerSettings>";
						if (CreateGameConfig(server, "serverconfig.xml", sd2dXml, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Out of Reach":
						string oorJson = @"{ ""ServerName"": ""{ServerName}"", ""MaxPlayers"": {MaxPlayers} }";
						if (CreateGameConfig(server, "ServerConfig.json", oorJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "NS2: Combat":
						string ns2cJson = @"{ ""serverName"": ""{ServerName}"", ""maxPlayers"": {MaxPlayers} }";
						if (CreateGameConfig(server, "ServerConfig.json", ns2cJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Just Cause 2: Multiplayer":
						string jc2Lua = @"ServerName = ""{ServerName}""";
						if (CreateGameConfig(server, "config.lua", jc2Lua, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Beyond the Wire":
						string btwCfg = @"ServerName=""{ServerName}""";
						if (CreateGameConfig(server, @"BeyondTheWire\ServerConfig\Server.cfg", btwCfg, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Colony Survival":
						string csJson = @"{ ""serverName"": ""{ServerName}"" }";
						if (CreateGameConfig(server, "config.json", csJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Core Keeper":
						string coreJson = @"{ ""serverName"": ""{ServerName}"", ""maxPlayers"": {MaxPlayers} }";
						if (CreateGameConfig(server, @"DedicatedServer\ServerConfig.json", coreJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Factorio":
					case "Factorio (Experimental)":
					case "Factorio (Space Age)":
						string factJson = @"{ ""name"": ""{ServerName}"", ""max_players"": {MaxPlayers} }";
						if (CreateGameConfig(server, @"data\server-settings.json", factJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Eco":
						string ecoJson = @"{ ""Description"": ""{ServerName}"", ""MaxConnections"": {MaxPlayers} }";
						if (CreateGameConfig(server, @"Configs\Network.eco", ecoJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Starbound":
						string sbJson = @"{ ""serverName"": ""{ServerName}"", ""maxPlayers"": {MaxPlayers} }";
						if (CreateGameConfig(server, @"storage\starbound_server.config", sbJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Project CARS 2":
						string pcarsJson = @"{ ""server"": { ""name"": ""{ServerName}"" } }";
						if (CreateGameConfig(server, "server_config.json", pcarsJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Keplerth":
						string kepJson = @"{ ""ServerName"": ""{ServerName}"" }";
						if (CreateGameConfig(server, "config.json", kepJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Assetto Corsa Competizione":
						string accJson = @"{ ""serverName"": ""{ServerName}"", ""maxClients"": {MaxPlayers} }";
						if (CreateGameConfig(server, @"cfg\settings.json", accJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "rFactor 2":
						string rf2Json = @"{ ""ServerName"": ""{ServerName}"" }";
						if (CreateGameConfig(server, @"UserData\player\Multiplayer.json", rf2Json, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Mindustry":
						string minJson = @"{ ""name"": ""{ServerName}"" }";
						if (CreateGameConfig(server, @"config\server-settings.json", minJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Portal Knights":
						string pkJson = @"{ ""basicServerData"": { ""name"": ""{ServerName}"", ""port"": {Port} }, ""universeSize"": ""Normal"" }";
						if (CreateGameConfig(server, "server_config.json", pkJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Survive the Nights":
					case "Savage Lands":
						string stnJson = @"{ ""ServerName"": ""{ServerName}"" }";
						if (CreateGameConfig(server, "ServerConfig.json", stnJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Foundry":
						string foundryCfg = @"server_name={ServerName}
server_description=Hosted using Synix
server_world_name={Identity}
server_port={Port}
server_query_port={QueryPort}
server_is_public=true
server_max_players={MaxPlayers}
server_password={Password}
server_autosave_interval=300
server_save_slots=10
server_pause_when_empty=true";
						if (CreateGameConfig(server, "app.cfg", foundryCfg, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "HumanitZ":
						string hzIni = @"ServerName=""{ServerName}""
Password=""{Password}""
AdminPassword=""{AdminPassword}""
MaxPlayers={MaxPlayers}";
						if (CreateGameConfig(server, @"HumanitZServer\GameServerSettings.ini", hzIni, cleanIdentity, localIp, publicIp)) applied = true;
						break;
				}
			}
			catch (Exception)
			{
				return false;
			}
			return applied;
		}

		// --------------------------------------------------------
		// UNIFIED UTILITY FUNCTIONS
		// --------------------------------------------------------

		private static bool CreateGameConfig(GameServer server, string relativeFilePath, string contentTemplate, string identity, string localIp, string publicIp)
		{
			string fullFilePath = Path.Combine(server.InstallPath, relativeFilePath);

			string targetFolder = Path.GetDirectoryName(fullFilePath);
			if (!Directory.Exists(targetFolder))
			{
				Directory.CreateDirectory(targetFolder);
			}

			if (!File.Exists(fullFilePath))
			{
				string finalContent = contentTemplate
					.Replace("{ServerName}", server.ServerName)
					.Replace("{Password}", server.Password)
					.Replace("{AdminPassword}", server.AdminPassword)
					.Replace("{Port}", server.Port.ToString())
					.Replace("{QueryPort}", server.QueryPort.ToString())
					.Replace("{MaxPlayers}", server.MaxPlayers.ToString())
					.Replace("{WorldSeed}", string.IsNullOrWhiteSpace(server.WorldSeed) ? "12345" : server.WorldSeed)
					.Replace("{Identity}", identity)
					.Replace("{LocalIP}", localIp)
					.Replace("{PublicIP}", publicIp);

				ManualConfigWasCreated = true;
				return FileHandler.Create(targetFolder, Path.GetFileName(fullFilePath), finalContent);
			}
			return false;
		}

		private static bool CopySteamDLLs(string installPath, string BinariesDir)
		{
			bool filesCopied = false;
			string[] dlls = { "steamclient64.dll", "tier0_s64.dll", "vstdlib_s64.dll" };

			string targetDir = Path.Combine(installPath, BinariesDir);
			string steamCmdPath = @"C:\Synix\SteamCMD";

			if (!Directory.Exists(targetDir))
			{
				Directory.CreateDirectory(targetDir);
			}

			foreach (string dll in dlls)
			{
				string sourcePath = Path.Combine(steamCmdPath, dll);

				if (File.Exists(sourcePath) && !File.Exists(Path.Combine(targetDir, dll)))
				{
					if (FileHandler.Copy(sourcePath, targetDir, dll, false))
					{
						filesCopied = true;
					}
				}
			}
			return filesCopied;
		}
	}
}