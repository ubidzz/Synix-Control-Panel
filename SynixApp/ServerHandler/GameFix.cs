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

			if (server.Game == "Dune: Awakening" || server.Game == "Minecraft")
			{
				ManualConfigWasCreated = true;
				applied = true;
			}

			try
			{

				switch (server.Game)
				{
					case "StarRupture":
						if (CopySteamDLLs(server.InstallPath, @"StarRupture\Binaries\Win64")) applied = true; break;
					case "Soulmask":
						if (CopySteamDLLs(server.InstallPath, @"WS\Binaries\Win64")) applied = true; break;
					case "Palworld":
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
					case "ASTRONEER":
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
					case "Dune: Awakening":
						if (CopySteamDLLs(server.InstallPath, "")) applied = true; break;
					case "Last Oasis":
						if (CopySteamDLLs(server.InstallPath, @"OasisServer\Binaries\Win64")) applied = true; break;
					case "Dark and Light":
						if (CopySteamDLLs(server.InstallPath, @"DNL\Binaries\Win64")) applied = true; break;
					case "SCP: 5K":
						if (CopySteamDLLs(server.InstallPath, @"Pandemic\Binaries\Win64")) applied = true; break;
					case "GROUND BRANCH CTE":
						if (CopySteamDLLs(server.InstallPath, @"GroundBranch\Binaries\Win64")) applied = true; break;
					case "Desynced":
						if (CopySteamDLLs(server.InstallPath, @"Desynced\Binaries\Win64")) applied = true; break;
					case "HYPERCHARGE: Unboxed":
						if (CopySteamDLLs(server.InstallPath, @"Unboxed\Binaries\Win64")) applied = true; break;
					case "Dysterra":
						if (CopySteamDLLs(server.InstallPath, @"Dysterra\Binaries\Win64")) applied = true; break;
					case "D.A.T.A":
						if (CopySteamDLLs(server.InstallPath, @"WindowsServer\ABYSS421\Binaries\Win64")) applied = true; break;
					case "Days of War":
						if (CopySteamDLLs(server.InstallPath, @"DaysOfWar\Binaries\Win64")) applied = true; break;
					case "Angels Fall First":
						if (CopySteamDLLs(server.InstallPath, @"Binaries\Win64")) applied = true; break;
					case "Right to Rule":
						if (CopySteamDLLs(server.InstallPath, @"RightToRule\Binaries\Win64")) applied = true; break;
					case "HELL'S NEW WORLD":
						if (CopySteamDLLs(server.InstallPath, @"WindowsServer\HellsNewWorld\Binaries\Win64")) applied = true; break;
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
				}

				switch (server.Game)
				{
					case "Rust":
						string rustCfg = @"# ============================================================================
# RUST DEDICATED SERVER CONFIGURATION (server.cfg)
# ============================================================================

# --- SERVER DISPLAY & IDENTITY ---
server.hostname ""{ServerName}""
server.description ""Welcome to {ServerName}!\n\nManaged via Synix Control Panel.""
server.url ""https://github.com/ubidzz/Synix-Control-Panel""
server.headerimage """"
server.tags ""vanilla""
server.maxplayers {MaxPlayers}

# --- WORLD GENERATION ---
server.seed {WorldSeed}
server.worldsize {WorldSize}
server.level ""Procedural Map""
server.saveinterval 300

# --- GAMEPLAY & CHAT ---
server.pve false
server.globalchat true
server.airdropminplayers 10
server.stability true
server.radiation true
craft.instant false

# --- DECAY & UPKEEP ---
decay.upkeep true
decay.scale 1.0

# --- PERFORMANCE & SECURITY ---
fps.limit 60
server.tickrate 30
gc.buffer 256
antihack.enabled true
server.secure true
server.official false

# --- RCON (REMOTE CONSOLE) ---
rcon.port {RCONPort}
rcon.password ""{RCONPassword}""
rcon.web {EnableRcon}";

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

					case "Just Cause 3 Multiplayer":
						string jc3Json = @"{
    ""ServerName"": ""{ServerName}"",
    ""MaxPlayers"": {MaxPlayers},
    ""BindIP"": ""0.0.0.0"",
    ""Port"": {Port}
}";
						if (CreateGameConfig(server, "config.json", jc3Json, cleanIdentity, localIp, publicIp)) applied = true;
						break;
					case "Sons Of The Forest":
						string sotfCfg = @"{ ""ServerName"": ""{ServerName}"", ""MaxPlayers"": {MaxPlayers}, ""ServerPlayMode"": ""Normal"" }";
						if (CreateGameConfig(server, @"userdata\dedicated_server.cfg", sotfCfg, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Palworld":
						string palIni = "[/Script/Pal.PalGameWorldSettings]\n" +
							"OptionSettings=(" +
							"ServerName=\"{ServerName}\"," +
							"ServerDescription=\"Managed via Synix Control Panel\"," +
							"AdminPassword=\"{AdminPassword}\"," +
							"ServerPassword=\"{Password}\"," +
							"ServerPlayerMaxNum={MaxPlayers}," +
							"PublicIP=\"{PublicIP}\"," +
							"PublicPort={Port}," +
							"RCONEnabled={EnableRcon}," +
							"RCONPort={RCONPort}," +
							"RESTAPIEnabled=False," +
							"RESTAPIPort={QueryPort}," +
							"ChatPostLimitPerMinute=10," +
							"CrossplayPlatforms=(Steam,Xbox,PS5,Mac)," +
							"LogFormatType=\"Text\"," +
							"bIsShowJoinLeftMessage=True," +
							"bIsUseBackupSaveData=True," +
							"bEnableBuildingPlayerUIdDisplay=True," +
							"bAllowClientMod=False," +
							"BaseCampMaxNum=128," +
							"BaseCampMaxNumInGuild=4," +
							"BaseCampWorkerMaxNum=15," +
							"ItemContainerForceMarkDirtyInterval=10.000000," +
							"MaxBuildingLimitNum=0," +
							"PhysicsActiveDropItemMaxNum=1000," +
							"ServerReplicatePawnCullDistance=15000.000000," +
							"AutoResetGuildTimeNoOnlinePlayers=72.000000," +
							"bAllowEnemyCampSpawnNearBaseCamp=True," +
							"bAllowEnhanceStat_Attack=True," +
							"bAllowEnhanceStat_Health=True," +
							"bAllowEnhanceStat_Stamina=True," +
							"bAllowEnhanceStat_Weight=True," +
							"bAllowEnhanceStat_WorkSpeed=True," +
							"bAllowGlobalPalboxExport=False," +
							"bAllowGlobalPalboxImport=False," +
							"bAutoResetGuildNoOnlinePlayers=False," +
							"bBuildAreaLimit=True," +
							"bCharacterRecreateInHardcore=False," +
							"bDisplayPvPItemNumOnWorldMap_BaseCamp=False," +
							"bDisplayPvPItemNumOnWorldMap_Player=False," +
							"bEnableFastTravel=True," +
							"bEnableFastTravelOnlyBaseCamp=False," +
							"bEnableInvaderEnemy=True," +
							"bEnableVoiceChat=True," +
							"bExistPlayerAfterLogout=False," +
							"bHardcore=False," +
							"bInvisibleOtherGuildBaseCampAreaFX=False," +
							"bIsPvP=False," +
							"bIsRandomizerPalLevelRandom=False," +
							"bIsStartLocationSelectByMap=True," +
							"bShowPlayerList=True," +
							"RandomizerSeed=\"\"," +
							"RandomizerType=\"None\"," +
							"VoiceChatMaxVolumeDistance=2000.000000," +
							"VoiceChatZeroVolumeDistance=3000.000000," +
							"AdditionalDropItemNumWhenPlayerKillingInPvPMode=0," +
							"AdditionalDropItemWhenPlayerKillingInPvPMode=\"\"," +
							"bAdditionalDropItemWhenPlayerKillingInPvPMode=False," +
							"BlockRespawnTime=10.000000," +
							"bPalLost=False," +
							"BuildObjectDamageRate=1.000000," +
							"BuildObjectDeteriorationDamageRate=1.000000," +
							"CollectionDropRate=1.000000," +
							"CollectionObjectHpRate=1.000000," +
							"CollectionObjectRespawnSpeedRate=1.000000," +
							"DayTimeSpeedRate=1.000000," +
							"DeathPenalty=\"All\"," +
							"DenyTechnologyList=()," +
							"EnemyDropItemRate=1.000000," +
							"EquipmentDurabilityDamageRate=1.000000," +
							"ExpRate=1.000000," +
							"GuildPlayerMaxNum=20," +
							"GuildRejoinCooldownMinutes=0," +
							"ItemCorruptionMultiplier=1.000000," +
							"ItemWeightRate=1.000000," +
							"MonsterFarmActionSpeedRate=1.000000," +
							"NightTimeSpeedRate=1.000000," +
							"PalAutoHPRegeneRate=1.000000," +
							"PalAutoHpRegeneRateInSleep=1.000000," +
							"PalCaptureRate=1.000000," +
							"PalDamageRateAttack=1.000000," +
							"PalDamageRateDefense=1.000000," +
							"PalEggDefaultHatchingTime=72.000000," +
							"PalSpawnNumRate=1.000000," +
							"PalStaminaDecreaceRate=1.000000," +
							"PalStomachDecreaceRate=1.000000," +
							"PlayerAutoHPRegeneRate=1.000000," +
							"PlayerAutoHpRegeneRateInSleep=1.000000," +
							"PlayerDamageRateAttack=1.000000," +
							"PlayerDamageRateDefense=1.000000," +
							"PlayerStaminaDecreaceRate=1.000000," +
							"PlayerStomachDecreaceRate=1.000000," +
							"RespawnPenaltyDurationThreshold=300.000000," +
							"RespawnPenaltyTimeScale=1.000000," +
							"SupplyDropSpan=180)";

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
					case "Holdfast: Nations At War":
						string hfTxt = @"server_name {ServerName}";
						if (CreateGameConfig(server, @"Holdfast NaW_Data\StreamingAssets\Config\serverConfig_Core.txt", hfTxt, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "V Rising":
						string vrJson = @"{ ""Name"": ""{ServerName}"" }";
						if (CreateGameConfig(server, @"VRisingServer_Data\StreamingAssets\Settings\ServerHostSettings.json", vrJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "7 Days to Die":
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
						string factJson = @"{ ""name"": ""{ServerName}"", ""max_players"": {MaxPlayers} }";
						if (CreateGameConfig(server, @"data\server-settings.json", factJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Eco":
						string ecoJson = @"{ ""Description"": ""{ServerName}"", ""MaxConnections"": {MaxPlayers} }";
						if (CreateGameConfig(server, @"Configs\Network.eco", ecoJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;
					case "Project CARS 2":
						string pcarsJson = @"{ ""server"": { ""name"": ""{ServerName}"" } }";
						if (CreateGameConfig(server, "server_config.json", pcarsJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "Assetto Corsa Competizione":
						string accJson = @"{ ""serverName"": ""{ServerName}"", ""maxClients"": {MaxPlayers} }";
						if (CreateGameConfig(server, @"cfg\settings.json", accJson, cleanIdentity, localIp, publicIp)) applied = true;
						break;

					case "rFactor 2":
						string rf2Json = @"{ ""ServerName"": ""{ServerName}"" }";
						if (CreateGameConfig(server, @"UserData\player\Multiplayer.json", rf2Json, cleanIdentity, localIp, publicIp)) applied = true;
						break;
					case "Survive the Nights":
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
						string hzIni = @"[Host Settings]
ServerName=""{ServerName}""
Password=""{Password}""
;Server name. Avoid using the word ""Official"" in your server name or your server may not start or showup in the server list.
SaveName=""DedicatedSaveMP""
;Use this to place your server in a specific bucket and use it when searching. Changing this will make your server not show up in the default server list, users will have to indicate the specific SearchID
SearchID=""HumanitZ_Dedicated""
;Used for admin access, you can use /adminaccess <password> in game to access admin commands.
AdminPass=""{AdminPassword}""
MaxPlayers={MaxPlayers}
;Used to reserve slots for certain players. Add their NetID to F_ReservedSlots.txt file. Set to 0 to disable reserving slots.
ReserveSlots={EnableRcon}
;Use it to execute remote command, use any rcon compatible program. Enabling this will allow server ping being displayed in server browser
RCONEnabled=true
;TCP port for rcon
RConPort={RCONPort}
RCONPass=""{RCONPassword}""
;If set to true notification about dead players will be disabled. Admins will still be able to see them.
NoDeathFeedback=true
;If set to true notification about players joining and leaving the server will be disabled. Admins will still be able to see them.
NoJoinFeedback=true
;When set to true players will only have access to random coast spawns and spawn point if available
LimitedSpawns=false
;When true your server will reject known banned players from official servers
UseGlobalBanList=true
;When disabled (false), players will not be able to join the server when using family share feature.
AllowFamilySharing=true

[World Settings]
;Build ID, only relevant inside REF_GameServerSettings.ini because that gets updated when pulling server files off steam CMD.
Version=39
;Multiplier for player experience gain. 1 is default, 2 would be twice as much experience, 0.5 would be half as much experience.
XpMultiplier=1
;The interval in seconds at which the server saves the world. Set to 0 to disable auto-saving.
SaveIntervalSec=300
;When set to true the player that died will lose their character and have to create a a new one
PermaDeath=false
;0=Lose nothing, 1=Lose backpack and weapon in hand, 2=Previous + pockets and backpack, 3=All previous + Equipment
OnDeath=2
;Time in second to be able to spawn again after death
RespawnTimer=15
;false/true=Off/On. This will also affect the ability to interact with player items such as cars and containers
PVP={GameMode}
;Time in seconds before player is allowed to log out if PVP is enabled. Set to 0 to disable logout timer.
LogoutTimer=30
; Set whether air drops are enabled or not
AirDrop=true
;How many game days between each AirDrop
AirDropInterval=1
;If true when weapon durability reaches 0%, the weapon will break and be removed from the player's inventory. Set to false to disable weapon breaking.
WeaponBreak=true
;How many cars you are allowed to own. Non owned cars can't be driven but can be looted. When a car is claimed you can Lock/unlock it and use it as normal
MaxOwnedCars=2
;When true time passes if eveyone performs the sleep emote at the same time, false=Passing time is disabled
MultiplayerSleep=false
;Loot does respawn, false=Never respawn
LootRespawn=true
;If loot respawn is enabled, this is the time in minutes it takes for loot containers to respawn.
LootRespawnTimer=60
;If loot respawn is enabled, this is the time in minutes it takes for pickup items to respawn.
PickupRespawnTimer=90
;From 0 to 4 (Scarce, low, default, Plentiful, abundant) Same for all rarities below.
RarityFood=2
RarityDrink=2
RarityMelee=2
RarityRanged=2
RarityAmmo=2
RarityArmor=2
RarityResources=2
RarityOther=2
;Health difficulty where 0=Very Easy, 1=Easy, 2=Default, 3=Hard, 4=Very Hard, 5=Nightmare
ZombieDiffHealth=1
;Speed difficulty where 0=Very Easy, 1=Easy, 2=Default, 3=Hard, 4=Very Hard, 5=Nightmare
ZombieDiffSpeed=2
;Damage difficulty where 0=Very Easy, 1=Easy, 2=Default, 3=Hard, 4=Very Hard, 5=Nightmare
ZombieDiffDamage=3
;Zombie amount multiplier 2 for example would mean twice as many zombies. 0.5 would mean half as many zombies.
ZombieAmountMulti=1
;Human bandit amount multiplier 2 for example would mean twice as many human bandits. 0.5 would mean half as many human bandits.
HumanAmountMulti=1
;Zombie dog mainly appear at night. 2 for example would mean twice as many zombie dogs. 0.5 would mean half as many zombie dogs.
ZombieDogMulti=1
;The time in minutes it takes for zombies to respawn, set to 0 to disable zombie respawning.
ZombieRespawnTimer=90
;The time in minutes it takes for human bandits to respawn, set to 0 to disable human bandit respawning.
HumanRespawnTimer=90
;Human bandit difficulty where 0=Very Easy, 1=Easy, 2=Default, 3=Hard, 4=Very Hard, 5=Nightmare
HumanHealth=2
;Human bandit speed difficulty where 0=Very Easy, 1=Easy, 2=Default, 3=Hard, 4=Very Hard, 5=Nightmare
HumanSpeed=2
;Human bandit damage difficulty where 0=Very Easy, 1=Easy, 2=Default, 3=Hard, 4=Very Hard, 5=Nightmare
HumanDamage=2
;Animal spawn multiplier, default 1
AnimalMulti=1
;The time in minutes it takes for animals to respawn, set to 0 to disable animal respawning.
AnimalRespawnTimer=90
;0=Summer, 1=Autum, 2=Winter, 3=Spring
StartingSeason=1
;How many game days each season lasts
DaysPerSeason=5
;Day duration in minutes
DayDur=40
;Night duration in minutes
NightDur=20
;How fast your vitals drain 0=Slow, 1=Normal, 2=Fast
VitalDrain=1
;Enable finding dog companions you can recruit set to true to enable or false to disable
DogEnabled=true
;Allow players to recruit companion dogs by dropping food and claiming the dog. Set to false to disable.
RecruitDog=true
;The maximum amount of companion dogs you can find in the world. Dogs will respawn when needed.
DogNum=8
;Health multiplier of player placed buildings. By default set to 1.
BuildingHealth=1
;Dog companion health 0=Low 1=Default 2=High This is not a multiplier, it is a set value for dog companion health.
CompanionHealth=1
;Dog companion damage 0=Low 1=Default 2=High This is not a multiplier, it is a set value for dog companion damage.
CompanionDmg=1
;Enable players to dismantle their own buildings.
AllowDismantle=true
;Enable Players are able to dismantle house props.
AllowHouseDismantle=true
;If true You WILL NOT be allowed to build in someone's spawn point area. For clans only non recruit members can.
Territory=true
;Experimental: When false, certain buildables require a foundation. This reduces floating buildings. Use at your own risk. Provide feedback to devs if you encounter any issues.
FreeBuild=true
;When true, players are not allowed to build in certain zones.
NoBuildZone=true
;How many real days it takes for a spawn point to go from 100% health to being destroyed. Once spawn point is gone, player built stuff will also start decaying
Decay=7
;How many real days it takes for player built buildings to fully decay. No decay happens If a spawn point is protecting it. 0 to disable.
BuildingDecay=7
;How long in game days it takes for dropped pickups to be destroyed (Pickups part of the world and placed items will not be considered), a value of 0 means no cleanup is done.
PickupCleanup=6
;How long in minutes it takes for the white blueprint building to be destroyed, a value of 0 means no cleanup is done.
FakeBuildingCleanup=3000
;Contols how fast food decays 0=Disabled 1=Default. It's a multiplier so if you need to slow down decay by half the value would be 0.5. Values greater than 1 will speed it up.
FoodDecay=1
;Controls how fast house generator consumes fuel. 0.01 to 4. It's a multiplier where 1 is default and less than 1 will slows it down.
GenFuel=1
;After how many real days a claimed car will be recycled into the pool and randomly spawned somewhere. Set to 0 to disable car recycling.
RecycleCar=14
;Sleep deprivation effect false=Disabled true=Enabled
Sleep=true
;When enabled and server is empty time doesn't pass.
FreezeTime=true
;Map is divided into 3 segments, objects that need to spawmn reasonabiliy far are put in Seg0 like houses. Broken cars and other similar objects are put in Seg1. Leave it as is unless you want to tweak performance and spawn distance
;Look at the map as a grid. Seg0=12 means map is divided into 12x12 grid. Once inside a grid, objects inside that grid will spawn.
MapSeg0=12
MapSeg1=20
MapSeg2=60
;When set to 1 or true voice chat will be enabled, set 0 or false to disable
Voip=true
;The game will sometimes spawn AI event such as zombies or human raiding your location. How frequence the event is where 0 = Disabled, 1 Low, 2 = Default, 3 = High, 4 = Insane
AIEvent=2
;Below you can tweak the odds of each weather type. The current season will still dictate which weather types can spawn.
Weather_ClearSky=1
Weather_Cloudy=1
Weather_Foggy=1
Weather_LightRain=1
Weather_Rain=1
Weather_Thunderstorm=1
Weather_LightSnow=1
Weather_Snow=1
Weather_Blizzard=1";
						if (CreateGameConfig(server, @"HumanitZServer\GameServerSettings.ini", hzIni, cleanIdentity, localIp, publicIp)) applied = true;
						break;
					case "ASTRONEER":
						{
							string astroEngineIni = @"[URL]
Port={Port}";

							string astroServerSettingsIni = @"PublicIP={PublicIP}
OwnerName=
OwnerGuid=0";

							if (CreateGameConfig(server, @"Astro\Saved\Config\WindowsServer\Engine.ini", astroEngineIni, cleanIdentity, localIp, publicIp)) applied = true;
							if (CreateGameConfig(server, @"Astro\Saved\Config\WindowsServer\AstroServerSettings.ini", astroServerSettingsIni, cleanIdentity, localIp, publicIp)) applied = true;
							break;
						}

					case "DayZ":
						{
							string dayzCfg = @"hostname = ""{ServerName}"";
password = ""{Password}"";
passwordAdmin = ""{AdminPassword}"";
maxPlayers = {MaxPlayers};
verifySignatures = 2;
forceSameBuild = 1;
disableVoN = 0;
vonCodecQuality = 20;
persistent = 1;
guaranteedUpdates = 1;
instanceId = 1;
storageAutoFix = 1;

class Missions
{
    class DayZ
    {
        template = ""dayzOffline.chernarusplus"";
    };
};";

							if (CreateGameConfig(server, "serverDZ.cfg", dayzCfg, cleanIdentity, localIp, publicIp)) applied = true;
							break;
						}

					case "Arma 3":
						{
							string arma3Cfg = @"hostname = ""{ServerName}"";
password = ""{Password}"";
passwordAdmin = ""{AdminPassword}"";
maxPlayers = {MaxPlayers};
verifySignatures = 2;
BattlEye = 1;
persistent = 1;
kickDuplicate = 1;
disableVoN = 0;
vonCodecQuality = 20;";

							if (CreateGameConfig(server, "server.cfg", arma3Cfg, cleanIdentity, localIp, publicIp)) applied = true;
							break;
						}

					case "Arma Reforger":
						{
							string reforgerJson = @"{
  ""bindPort"": {Port},
  ""publicPort"": {Port},
  ""a2s"": {
    ""address"": ""0.0.0.0"",
    ""port"": {QueryPort}
  },
  ""game"": {
    ""name"": ""{ServerName}"",
    ""password"": ""{Password}"",
    ""passwordAdmin"": ""{AdminPassword}"",
    ""scenarioId"": ""{ECC61978EDCC2B5A}Missions/23_Campaign.conf"",
    ""maxPlayers"": {MaxPlayers},
    ""visible"": true,
    ""crossPlatform"": true,
    ""gameProperties"": {
      ""fastValidation"": true,
      ""battlEye"": true
    },
    ""mods"": []
  }
}";

							if (CreateGameConfig(server, @"configs\server.json", reforgerJson, cleanIdentity, localIp, publicIp)) applied = true;
							break;
						}

					case "Mount & Blade II: Bannerlord":
						{
							string bannerlordCfg = @"ServerName {ServerName}
GamePassword {Password}
AdminPassword {AdminPassword}
GameType TeamDeathmatch
MaxNumberOfPlayers {MaxPlayers}
start_game_and_mission";

							if (CreateGameConfig(server, @"Modules\Native\CustomServerconfig.txt", bannerlordCfg, cleanIdentity, localIp, publicIp)) applied = true;
							break;
						}

					case "Dysterra":
						{
							string dysterraJson = @"{
  ""WorldName"": ""{ServerName}"",
  ""WorldInfo"": ""Managed by Synix"",
  ""Password"": ""{Password}"",
  ""MaxPlayers"": {MaxPlayers},
  ""ValueOverrides"": {}
}";

							if (CreateGameConfig(server, @"Dysterra\WorldSettings\MyServer.json", dysterraJson, cleanIdentity, localIp, publicIp)) applied = true;
							break;
						}

					case "Serious Sam 2017":
						{
							string seriousSamServerCfg = @"rconpass = ""{AdminPassword}"";
sessionname = ""{ServerName}""
port = {Port}";

							string seriousSamGameOptionsCfg = @"gam_ctMaxPlayers = {MaxPlayers}
gam_ctMinPlayers = 1
gamemode = ""Cooperative""
gam_bAutoCycleMaps = 1";

							if (CreateGameConfig(server, "server.cfg", seriousSamServerCfg, cleanIdentity, localIp, publicIp)) applied = true;
							if (CreateGameConfig(server, "gameoptions.cfg", seriousSamGameOptionsCfg, cleanIdentity, localIp, publicIp)) applied = true;
							break;
						}

					case "Serious Sam HD: The Second Encounter":
					case "Serious Sam HD: The First Encounter":
					case "Serious Sam 3: BFE":
						{
							string seriousSamCfg = @"rconpass = ""{AdminPassword}"";
sessionname = ""{ServerName}""
gam_ctMaxPlayers = {MaxPlayers}
gamemode = ""Cooperative""
gam_bAutoCycleMaps = 1";

							if (CreateGameConfig(server, "server.cfg", seriousSamCfg, cleanIdentity, localIp, publicIp)) applied = true;
							break;
						}

					case "Wreckfest":
						{

							string sourceConfig = Path.Combine(server.InstallPath, "initial_server_config.cfg");
							string targetConfig = Path.Combine(server.InstallPath, "server_config.cfg");

							if (File.Exists(sourceConfig) && !File.Exists(targetConfig))
							{
								File.Copy(sourceConfig, targetConfig);
								ManualConfigWasCreated = true;
								applied = true;
							}
							break;
						}
				}
			}
			catch (Exception)
			{
				return false;
			}
			return applied;
		}

		private static bool CreateGameConfig(GameServer server, string relativeFilePath, string contentTemplate, string identity, string localIp, string publicIp)
		{
			SynixServerPasswords passwords;
			try
			{
				passwords = Core.RevealServerPasswords(server);
			}
			catch (SynixPasswordProtectionException)
			{
				Core.Instance.Log(
					"[🚨 ERROR] Synix could not unlock the saved server passwords. Re-enter them in Server Settings before creating the game configuration.",
					Color.Red);
				return false;
			}

			string fullFilePath = Path.Combine(server.InstallPath, relativeFilePath);
			string? targetFolder = Path.GetDirectoryName(fullFilePath);
			if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

			if (!File.Exists(fullFilePath))
			{
				string finalContent = contentTemplate
					.Replace("{ServerName}", server.ServerName)
					.Replace("{Password}", passwords.ServerPassword)
					.Replace("{AdminPassword}", passwords.AdminPassword)
					.Replace("{Port}", server.Port.ToString())
					.Replace("{QueryPort}", server.QueryPort.ToString())
					.Replace("{EnableRcon}", server.Game == "Rust" ? (server.EnableRcon ? "1" : "0") : server.EnableRcon.ToString().ToLower())
					.Replace("{RCONPort}", server.RconPort.ToString())
					.Replace("{RCONPassword}", passwords.RconPassword)
					.Replace("{MaxPlayers}", server.MaxPlayers.ToString())
					.Replace("{GameMode}", server.GameMode?.ToString() ?? "")
					.Replace("{WorldSeed}", string.IsNullOrWhiteSpace(server.WorldSeed) ? "12345" : server.WorldSeed)
					.Replace("{WorldSize}", server.WorldSize > 0 ? server.WorldSize.ToString() : "4000")
					.Replace("{Map}", server.WorldName)
					.Replace("{PVE}", server.GameMode != null && server.GameMode.ToString().Equals("PVE", StringComparison.OrdinalIgnoreCase) ? "true" : "false")
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
			string steamCmdPath = Core.SteamCmdPath;

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
