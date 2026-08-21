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
using Synix_Control_Panel.SynixApp.Design;
using System.Diagnostics;

namespace Synix_Control_Panel.Database
{
	public partial class WarningDatabase : Form
	{
		private GameServer _server;

		private static readonly Dictionary<string, string> _messages = new()
		{
			{
				"Minecraft Java",
				"MINECRAFT EULA AGREEMENT REQUIRED:\n\n" +
				"By starting this server, you agree to the Minecraft End User License Agreement (EULA).\n\n" +
				"If you do not agree to these terms, click Decline and the server will not start.\n\n" +
				"Official EULA Document: https://aka.ms/MinecraftEULA"
			},
			{
				"StarRupture",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Modify GameUserSettings.ini to define your custom parameters and world structures.\n\n" +
				"3. Official Documentation: https://starrupture.wiki.gg/wiki/Server_Hosting"
			},
			{
				"Subsistence",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to generate 'UDKDedServerSettings.ini', then shut it down.\n\n" +
				"2. Edit the configuration file to configure your server passwords and gameplay options.\n\n" +
				"3. Official Documentation: https://subsistence.fandom.com/wiki/Dedicated_Servers"
			},
			{
				"Windrose",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Edit 'ServerDescription.json' to set up your multiplayer rules and identifiers.\n\n" +
				"3. Official Documentation: https://windrose.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"HumanitZ",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to generate 'GameServerSettings.ini', then shut it down.\n\n" +
				"2. Modify the INI configuration file to set up Admin passwords, player slots, and world rules.\n\n" +
				"3. Official Documentation: https://humanitz.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Soulmask",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server for the first time to build the target folder hierarchy, then shut it down.\n\n" +
				"2. Open the JSON configuration file to verify your security passwords and player limits.\n\n" +
				"3. Official Documentation: https://soulmask.wiki.gg/wiki/Server_Hosting"
			},
			{
				"Dune: Awakening",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Ensure Windows Hyper-V and hardware virtualization are enabled on your host machine.\n\n" +
				"2. You MUST run 'battlegroup.bat' as Administrator to launch the server deployment menu. (This is done automatically)\n\n" +
				"3. Select 'initial-setup' to build the VM and input your Self-Host Service Token.\n\n" +
				"4. Official Documentation:\n https://duneawakening.com/self-hosted-servers/ \n\n" +
				"5. Synix cannot control Dune: Awakening servers and is only good for installing, updating, backup and start the server."
			},
			{
				"Cepheus Protocol",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Modify 'Game.ini' to set up game rules and administrative access parameters.\n\n" +
				"3. Official Documentation: https://cepheusprotocol.wiki.gg/wiki/Dedicated_Servers"
			},
			{
				"7 Days to Die",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to generate 'serverconfig.xml', then shut it down.\n\n" +
				"2. Edit 'serverconfig.xml' to define your custom Server Name, Port, and Admin Passwords.\n\n" +
				"3. Official Documentation: https://steamcommunity.com/sharedfiles/filedetails/?id=2952870191"
			},
			{
				"Palworld",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to generate 'PalWorldSettings.ini', then shut it down.\n\n" +
				"2. Modify the configuration file to establish your Admin Password and community options.\n\n" +
				"3. Official Documentation: https://tech.palworldgame.com/"
			},
			{
				"ARK: Survival Evolved",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Launch the server initially to provision the GameUserSettings.ini file, then shut it down.\n\n" +
				"2. Configure your Server Admin Password and RCON settings directly inside the configuration file.\n\n" +
				"3. Official Documentation: https://ark.wiki.gg/wiki/Dedicated_server_setup"
			},
			{
				"ARK: Survival Ascended",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Edit GameUserSettings.ini to input your specific cluster ID, passwords, and custom parameters.\n\n" +
				"3. Official Documentation: https://ark.wiki.gg/wiki/Dedicated_server_setup"
			},
			{
				"Sons Of The Forest",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to initialize 'dedicated_server.cfg', then shut it down.\n\n" +
				"2. Configure your server name, passwords, and target player caps inside the JSON config.\n\n" +
				"3. Official Documentation: https://endnight.gamepedia.com/Sons_of_the_Forest_Dedicated_Server"
			},
			{
				"Enshrouded",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to generate 'enshrouded_server.json', then shut it down.\n\n" +
				"2. Update the JSON settings with your desired server name, slot counts, and passwords.\n\n" +
				"3. Official Documentation: https://enshrouded.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Core Keeper",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Edit 'ServerConfig.json' to input your world identifier, game ID, and network ports.\n\n" +
				"3. Official Documentation: https://corekeeper.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Terraria",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Launch the server script once to generate 'serverconfig.txt', then shut it down.\n\n" +
				"2. Edit 'serverconfig.txt' to adjust world paths, max player limits, and passwords.\n\n" +
				"3. Official Documentation: https://terraria.wiki.gg/wiki/Guide:A_guide_to_setting_up_a_dedicated_server"
			},
			{
				"Astroneer",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to generate 'AstroServerSettings.ini', then shut it down.\n\n" +
				"2. Modify the INI configuration file to configure custom universe settings and owner lists.\n\n" +
				"3. Official Documentation: https://astroneer.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Abiotic Factor",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to provision 'GameUserSettings.ini', then shut it down.\n\n" +
				"2. Set your server passwords and network variables directly within the INI file.\n\n" +
				"3. Official Documentation: https://abioticfactor.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Icarus",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Modify 'ServerSettings.ini' to assign your prospect rules and administrator access.\n\n" +
				"3. Official Documentation: https://icarus.wiki.gg/wiki/Dedicated_Servers"
			},
			{
				"Don't Starve Together",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. You must obtain a cluster token from Klei and paste it into 'cluster.ini'.\n\n" +
				"2. Boot the server once to generate structure templates, shut down, and configure settings before public deployment.\n\n" +
				"3. Official Documentation: https://dontstarve.wiki.gg/wiki/Guides/A_Guide_to_Setting_Up_a_Dedicated_Server"
			},
			{
				"Killing Floor 2",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to create 'PCServer-KFGame.ini', then shut it down.\n\n" +
				"2. Configure admin passwords and web admin parameters inside the INI file.\n\n" +
				"3. Official Documentation: https://wiki.tripwireinteractive.com/index.php?title=Dedicated_Server_(Killing_Floor_2)"
			},
			{
				"The Front",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server initially to build 'GameUserSettings.ini', then shut it down.\n\n" +
				"2. Modify the file to set administrative passwords and game modes.\n\n" +
				"3. Official Documentation: https://playthefront.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Smalland: Survive the Wilds",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Edit 'GameUserSettings.ini' to update server titles, maximum connections, and passcodes.\n\n" +
				"3. Official Documentation: https://smalland.wiki.gg/wiki/Dedicated_Servers"
			},
			{
				"V Rising",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to build the JSON configuration architecture, then shut it down.\n\n" +
				"2. Set your unique SaveName and ServerHostSettings parameters before relaunching.\n\n" +
				"3. Official Documentation: https://github.com/StunlockStudios/vrising-dedicated-server-instructions"
			},
			{
				"DayZ",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to create 'serverDZ.cfg', then shut it down.\n\n" +
				"2. Edit 'serverDZ.cfg' to modify mission parameters, server names, and security rules.\n\n" +
				"3. Official Documentation: https://community.bistudio.com/wiki/DayZ:Server_Configuration"
			},
			{
				"Conan Exiles",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to generate 'ServerSettings.ini', then shut it down.\n\n" +
				"2. Update the INI file to handle administrative logins, server rates, and building permissions.\n\n" +
				"3. Official Documentation: https://conanexiles.wiki.gg/wiki/Server_Configuration"
			},
			{
				"Project Zomboid",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Launch the server initialization batch script once to create the configuration ini file, then shut it down.\n\n" +
				"2. Edit the generated INI inside your Zomboid server folder to set safehouse options, administrative passwords, and map options.\n\n" +
				"3. Official Documentation: https://pzwiki.net/wiki/Dedicated_Server"
			},
			{
				"Mordhau",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to produce 'Game.ini', then shut it down.\n\n" +
				"2. Modify the file to define map rotations, player limits, and admin credentials.\n\n" +
				"3. Official Documentation: https://mordhau.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Satisfactory",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Modify 'Game.ini' to configure advanced gameplay and session tokens.\n\n" +
				"3. Official Documentation: https://satisfactory.wiki.gg/wiki/Dedicated_servers"
			},
			{
				"Factorio",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Edit 'server-settings.json' to include your official Factorio service credentials and user token for public listing visibility.\n\n" +
				"2. Official Documentation: https://wiki.factorio.com/Multiplayer#Server_settings"
			},
			{
				"Space Engineers",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the dedicated server utility once to generate 'SpaceEngineers-Dedicated.cfg', then shut it down.\n\n" +
				"2. Edit the XML configuration file to define world parameters, mods, and admin lists.\n\n" +
				"3. Official Documentation: https://spaceengineers.wiki.gg/wiki/Dedicated_Servers"
			},
			{
				"Insurgency: Sandstorm",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Edit 'Game.ini' to adjust round parameters, mutators, and administrative options.\n\n" +
				"3. Official Documentation: https://sandstorm-support.newworldinteractive.com/"
			},
			{
				"Myth of Empires",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to provision 'GameUserSettings.ini', then shut it down.\n\n" +
				"2. Edit the settings file to configure guild limits, admin keys, and server rules.\n\n" +
				"3. Official Documentation: https://mythofempires.wiki.gg/wiki/Server_Hosting"
			},
			{
				"PixARK",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to generate 'GameUserSettings.ini', then shut it down.\n\n" +
				"2. Edit the file to add your admin passcodes and game mode constraints.\n\n" +
				"3. Official Documentation: https://pixark.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Atlas",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to produce 'GameUserSettings.ini', then shut it down.\n\n" +
				"2. Configure grid maps, database connections, and server passwords inside the configuration file.\n\n" +
				"3. Official Documentation: https://playatlas.wiki.gg/wiki/Dedicated_server_setup"
			},
			{
				"SCUM",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Modify 'ServerSettings.ini' to adjust puppet spawns, item decay rates, and admin settings.\n\n" +
				"3. Official Documentation: https://scum.wiki.gg/wiki/Server_Hosting"
			},
			{
				"Hell Let Loose",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to provision 'Server.ini', then shut it down.\n\n" +
				"2. Set your RCON passwords, VIP lists, and server names within the configuration file.\n\n" +
				"3. Official Documentation: https://hellletloose.wiki.gg/wiki/Server_Hosting"
			},
			{
				"Wurm Unlimited",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to create 'gameserver.conf', then shut it down.\n\n" +
				"2. Edit the configuration properties file to adjust server limits and gameplay features.\n\n" +
				"3. Official Documentation: https://wurmpedia.com/index.php/Dedicated_server"
			},
			{
				"Nightingale",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server initially to generate 'GameUserSettings.ini', then shut it down.\n\n" +
				"2. Adjust the settings file to define realm parameters and user credentials.\n\n" +
				"3. Official Documentation: https://playnightingale.wiki.gg/wiki/Dedicated_Servers"
			},
			{
				"Holdfast: Nations At War",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Modify 'serverConfig_Core.txt' to adjust regiment options, game modes, and admin IDs.\n\n" +
				"3. Official Documentation: https://holdfastgame.com/"
			},
			{
				"DeadPoly",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to build 'GameUserSettings.ini', then shut it down.\n\n" +
				"2. Update the INI parameters for player stats, passwords, and server descriptions.\n\n" +
				"3. Official Documentation: https://deadpoly.wiki.gg/wiki/Dedicated_Servers"
			},
			{
				"Bellwright",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to provision 'GameUserSettings.ini', then shut it down.\n\n" +
				"2. Edit the file to apply password security and initial world parameters.\n\n" +
				"3. Official Documentation: https://bellwright.wiki.gg/wiki/Server_Hosting"
			},
			{
				"Craftopia",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to generate 'ServerSetting.ini', then shut it down.\n\n" +
				"2. Configure your server names, passwords, and world seeds inside the configuration file.\n\n" +
				"3. Official Documentation: https://craftopia.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"The Isle",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Edit 'Game.ini' to define your administrative passwords and ruleset details.\n\n" +
				"3. Official Documentation: https://theisle.wiki.gg/wiki/Server_Hosting"
			},
			{
				"Ready or Not",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to create 'Engine.ini', then shut it down.\n\n" +
				"2. Modify the configuration paths to establish custom game parameters and player counts.\n\n" +
				"3. Official Documentation: https://readyornot.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Grounded",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to provision 'GameUserSettings.ini', then shut it down.\n\n" +
				"2. Modify the file to set up automated saves and administrative settings.\n\n" +
				"3. Official Documentation: https://grounded.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Rising Storm 2: Vietnam",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to generate 'ROGame.ini', then shut it down.\n\n" +
				"2. Edit the INI file to set map voting structures and server configurations.\n\n" +
				"3. Official Documentation: https://tripwireinteractive.atlassian.net/wiki/spaces/RS2G/pages/53707787/Dedicated+Server"
			},
			{
				"Hurtworld",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Modify your startup arguments or configuration strings to define administrative tags and map configurations.\n\n" +
				"3. Official Documentation: https://hurtworld.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Day of Dragons",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to generate 'Game.ini', then shut it down.\n\n" +
				"2. Configure your server parameters and rule files inside the saved configuration directories.\n\n" +
				"3. Official Documentation: https://dayofdragons.com/"
			},
			{
				"Miscreated",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to create initialization databases, then shut it down.\n\n" +
				"2. Edit your configuration scripts to define database variables and player rules.\n\n" +
				"3. Official Documentation: https://miscreated.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Life is Feudal: Your Own",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to generate 'config_local.xml', then shut it down.\n\n" +
				"2. Modify the XML settings file to map database links and server ports.\n\n" +
				"3. Official Documentation: https://lifeisfeudal.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Citadel: Forged with Fire",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Edit 'Game.ini' to update server rules, passwords, and player attributes.\n\n" +
				"3. Official Documentation: https://citadel.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"CryoFall",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to output 'Settings.xml', then shut it down.\n\n" +
				"2. Edit the XML document to customize game server rules and protection values.\n\n" +
				"3. Official Documentation: https://cryofall.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Primal Carnage: Extinction",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to provision 'Game.ini', then shut it down.\n\n" +
				"2. Configure team balancing parameters and server configurations within the INI file.\n\n" +
				"3. Official Documentation: https://primalcarnage.wiki.gg/wiki/Server_Hosting"
			},
			{
				"Ranch Simulator",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to create 'Game.ini', then shut it down.\n\n" +
				"2. Edit the file to configure save data parameters and connection criteria.\n\n" +
				"3. Official Documentation: https://ranchsimulator.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Memories of Mars",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Modify 'Game.ini' to set up sector rules and secure access controls.\n\n" +
				"3. Official Documentation: https://memoriesofmars.wiki.gg/wiki/Server_Hosting"
			},
			{
				"Deadside",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to provision configuration directories, then shut it down.\n\n" +
				"2. Adjust server variables to define safe zones and loot multiplier settings.\n\n" +
				"3. Official Documentation: https://deadside.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Wreckfest",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to generate 'server_config.cfg', then shut it down.\n\n" +
				"2. Edit the config file to establish voting parameters, tracks, and vehicle classes.\n\n" +
				"3. Official Documentation: https://wreckfest.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Assetto Corsa",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to produce 'server_cfg.ini', then shut it down.\n\n" +
				"2. Configure track selections, assist rules, and entry lists inside the ini file.\n\n" +
				"3. Official Documentation: https://www.assettocorsa.net/"
			},
			{
				"BeamNG.drive",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Adjust authorization files and authentication keys generated in the root server profile.\n\n" +
				"3. Official Documentation: https://wiki.beamng.com/BeamNG_Server"
			},
			{
				"Last Oasis",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to create configuration folders, then shut it down.\n\n" +
				"2. Update the configuration layers to manage walker rules and network limits.\n\n" +
				"3. Official Documentation: https://lastoasis.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Dark and Light",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to build 'GameUserSettings.ini', then shut it down.\n\n" +
				"2. Edit the INI configuration for harvesting rates and admin controls.\n\n" +
				"3. Official Documentation: https://darkandlight.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Medieval Dynasty",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to create the server settings path, then shut it down.\n\n" +
				"2. Adjust server configs to control building limits and season lengths.\n\n" +
				"3. Official Documentation: https://medievaldynasty.wiki.gg/wiki/Dedicated_Servers"
			},
			{
				"Longvinter",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Edit 'Game.ini' to configure island economy and permissions.\n\n" +
				"3. Official Documentation: https://longvinter.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Ground Branch",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to generate 'Game.ini', then shut it down.\n\n" +
				"2. Edit the file to configure game modes, round timeouts, and player lists.\n\n" +
				"3. Official Documentation: https://groundbranch.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Red Orchestra 2: Heroes of Stalingrad",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to provision 'ROGame.ini', then shut it down.\n\n" +
				"2. Configure realism settings and map rotation rules inside the INI file.\n\n" +
				"3. Official Documentation: https://tripwireinteractive.atlassian.net/"
			},
			{
				"Beasts of Bermuda",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to generate 'Game.ini', then shut it down.\n\n" +
				"2. Edit the file to configure growth multipliers, weather variables, and admin permissions.\n\n" +
				"3. Official Documentation: https://beastsofbermuda.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"The Isle (Evrima)",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Modify 'Game.ini' to establish admin credentials and gameplay constraints.\n\n" +
				"3. Official Documentation: https://theisle.wiki.gg/wiki/Server_Hosting"
			},
			{
				"Foundry",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to generate 'app.cfg', then shut it down.\n\n" +
				"2. Edit 'app.cfg' to modify server descriptions, passwords, and port configurations.\n\n" +
				"3. Official Documentation: https://foundry.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Stranded Deep",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to create 'ServerConfig.json', then shut it down.\n\n" +
				"2. Edit the JSON file to specify server names, slots, and seed configurations.\n\n" +
				"3. Official Documentation: https://strandeddeep.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Staxel",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to build 'server.config', then shut it down.\n\n" +
				"2. Update the JSON settings with your village rules and player access levels.\n\n" +
				"3. Official Documentation: https://staxel.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Farming Simulator 19",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Edit 'dedicatedServerConfig.xml' to define passwords, savegames, and admin accounts.\n\n" +
				"3. Official Documentation: https://farming-simulator.com/"
			},
			{
				"Assetto Corsa Competizione",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to output 'settings.json', then shut it down.\n\n" +
				"2. Configure event rules, entry lists, and server parameters inside the JSON config.\n\n" +
				"3. Official Documentation: https://www.assettocorsa.net/acc/"
			},
			{
				"Medieval Engineers",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server instance utility once to generate configuration files, then shut it down.\n\n" +
				"2. Edit 'MedievalEngineers-Dedicated.cfg' to define structural integrity settings and limits.\n\n" +
				"3. Official Documentation: https://spaceengineers.wiki.gg/wiki/Medieval_Engineers"
			},
			{
				"BattleBit Remastered",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to generate default configuration properties, then shut it down.\n\n" +
				"2. Edit the server settings script to define map selection lists, tickers, and passwords.\n\n" +
				"3. Official Documentation: https://battlebit.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Return to Moria",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Modify the output INI files to declare session options and permissions.\n\n" +
				"3. Official Documentation: https://returntomoria.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Outlaws of the Old West",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to generate 'Game.ini', then shut it down.\n\n" +
				"2. Edit the INI file to manage law enforcement parameters, building health, and crafting rates.\n\n" +
				"3. Official Documentation: https://outlaws.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Squad 44 (Post Scriptum)",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to build 'Server.cfg', then shut it down.\n\n" +
				"2. Update the configuration file to include squad rules, player limits, and license tokens.\n\n" +
				"3. Official Documentation: https://joinsquad44.com/"
			},
			{
				"SCP: Pandemic",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to create the required profiles, then shut it down.\n\n" +
				"2. Edit the server configurations to establish passwords and game modes.\n\n" +
				"3. Official Documentation: https://scppandemic.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Aliens vs Predator (2010)",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Edit the target dedicated configuration templates to establish game rules.\n\n" +
				"3. Official Documentation: https://steamcommunity.com/"
			},
			{
				"Darkest Hour: Europe '44-'45",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to generate layout configs, then shut it down.\n\n" +
				"2. Modify configuration settings to map out objectives and team variables.\n\n" +
				"3. Official Documentation: https://darkesthourgame.com/"
			},
			{
				"Red Orchestra: Ostfront 41-45",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to generate setup paths, then shut it down.\n\n" +
				"2. Edit server files to manage campaign modes and player slots.\n\n" +
				"3. Official Documentation: https://tripwireinteractive.atlassian.net/"
			},
			{
				"Monday Night Combat",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once for initial configuration generation, then shut it down.\n\n" +
				"2. Modify game parameters inside the generated server property files.\n\n" +
				"3. Official Documentation: https://uberent.com/"
			},
			{
				"NS2: Combat",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Edit 'ServerConfig.json' to customize team rules and mod installations.\n\n" +
				"3. Official Documentation: https://ns2combat.com/"
			},
			{
				"Operation: Harsh Doorstop",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to build 'Engine.ini', then shut it down.\n\n" +
				"2. Modify the file to set server names, workshop IDs, and game modes.\n\n" +
				"3. Official Documentation: https://harshdoorstop.com/"
			},
			{
				"No One Survived",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to create configuration folders, then shut it down.\n\n" +
				"2. Adjust INI configuration settings to control zombie difficulty and loot decay.\n\n" +
				"3. Official Documentation: https://noonesurvived.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"The Mean Greens - Plastic Warfare",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to provision configuration structures, then shut it down.\n\n" +
				"2. Edit the output configuration files to change map selection order.\n\n" +
				"3. Official Documentation: https://themeangreens.com/"
			},
			{
				"America's Army: Proving Grounds",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Modify target config maps to adjust authentication and security levels.\n\n" +
				"3. Official Documentation: https://americasarmy.com/"
			},
			{
				"Just Cause 2: Multiplayer",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to generate 'config.lua', then shut it down.\n\n" +
				"2. Edit the Lua configuration script to add admin accounts and connection names.\n\n" +
				"3. Official Documentation: https://jc-mp.com/"
			},
			{
				"Out of Reach",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to generate 'ServerConfig.json', then shut it down.\n\n" +
				"2. Update the JSON file to define clan rules and island types.\n\n" +
				"3. Official Documentation: https://outreachgame.com/"
			},
			{
				"Toxikk",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to provision 'UDKEngine.ini', then shut it down.\n\n" +
				"2. Edit the configuration properties to set up arena parameters.\n\n" +
				"3. Official Documentation: https://toxikk.com/"
			},
			{
				"Unreal Tournament 3",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Modify 'UTEngine.ini' to adjust mutators, player caps, and web admin tools.\n\n" +
				"3. Official Documentation: https://epicgames.com/"
			},
			{
				"Viscera Cleanup Detail",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to generate initialization configurations, then shut it down.\n\n" +
				"2. Edit the config settings to establish map targets and password limits.\n\n" +
				"3. Official Documentation: https://ruetristate.com/viscera-cleanup-detail/"
			},
			{
				"Blackwake",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to generate server files, then shut it down.\n\n" +
				"2. Edit configuration parameters to configure crew sizes and round timers.\n\n" +
				"3. Official Documentation: https://blackwake.com/"
			},
			{
				"Beyond the Wire",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to create 'Server.cfg', then shut it down.\n\n" +
				"2. Configure squad structures and administrative settings inside the config file.\n\n" +
				"3. Official Documentation: https://beyondthewiregame.com/"
			},
			{
				"War of Rights",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Edit the server files to configure drilling modes, regiment passes, and server titles.\n\n" +
				"3. Official Documentation: https://warofrights.com/"
			},
			{
				"Colony Survival",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to output 'config.json', then shut it down.\n\n" +
				"2. Modify the JSON file to assign custom names, colony limits, and passwords.\n\n" +
				"3. Official Documentation: https://colonysurvival.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Farming Simulator 17",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to produce 'dedicatedServerConfig.xml', then shut it down.\n\n" +
				"2. Edit the XML document to specify farm save games and administrator keys.\n\n" +
				"3. Official Documentation: https://farming-simulator.com/"
			},
			{
				"Farming Simulator 15",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to generate 'dedicatedServerConfig.xml', then shut it down.\n\n" +
				"2. Modify the XML settings file to manage multiplayer connections.\n\n" +
				"3. Official Documentation: https://farming-simulator.com/"
			},
			{
				"The Wild Eight",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Update the generated configuration files to manage survival difficulty rates.\n\n" +
				"3. Official Documentation: https://thewildeight.com/"
			},
			{
				"S.T.A.L.K.E.R.: Shadow of Chernobyl",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to create profile settings, then shut it down.\n\n" +
				"2. Edit the execution parameters to modify faction rules and map choices.\n\n" +
				"3. Official Documentation: https://stalker-game.com/"
			},
			{
				"S.T.A.L.K.E.R.: Call of Pripyat",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to build configuration scripts, then shut it down.\n\n" +
				"2. Adjust settings to establish match limits and connection criteria.\n\n" +
				"3. Official Documentation: https://stalker-game.com/"
			},
			{
				"S.T.A.L.K.E.R.: Clear Sky",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to generate system configurations, then shut it down.\n\n" +
				"2. Modify configuration parameters for optimal multiplayer stability.\n\n" +
				"3. Official Documentation: https://stalker-game.com/"
			},
			{
				"Scrap Mechanic",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Edit 'ServerConfig.json' to change maximum connections and password values.\n\n" +
				"3. Official Documentation: https://scrapmechanic.wiki.gg/wiki/Dedicated_Server"
			},
			{
				"Terraria (tModLoader)",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to generate 'serverconfig.txt', then shut it down.\n\n" +
				"2. Edit 'serverconfig.txt' to load mods, world files, and player limits.\n\n" +
				"3. Official Documentation: https://tmodloader.wiki.gg/wiki/Guide:Hosting_a_Server"
			},
			{
				"Arma 2: DayZ Mod",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to create 'server.cfg', then shut it down.\n\n" +
				"2. Modify 'server.cfg' to handle mod paths (@DayZ), administrative tools, and server titles.\n\n" +
				"3. Official Documentation: https://dayzmod.com/"
			},
			{
				"Dirty Bomb",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to provision 'ShooterEngine.ini', then shut it down.\n\n" +
				"2. Edit the INI file to set match constraints and password keys.\n\n" +
				"3. Official Documentation: https://dirtybomb.fandom.com/"
			},
			{
				"Mortal Online 2",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Modify 'Game.ini' to adjust node configurations and administrative access.\n\n" +
				"3. Official Documentation: https://mortalonline2.com/"
			},
			{
				"XERA: Survival",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to generate 'GameUserSettings.ini', then shut it down.\n\n" +
				"2. Edit the INI file to manage safe zones, loot spawn rates, and server naming.\n\n" +
				"3. Official Documentation: https://xerasurvival.com/"
			},
			{
				"Survive the Nights",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to generate 'ServerConfig.json', then shut it down.\n\n" +
				"2. Edit the JSON file to define server options, day/night ratios, and security settings.\n\n" +
				"3. Official Documentation: https://survivethennights.net/"
			},
			{
				"Desolate",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to build 'GameUserSettings.ini', then shut it down.\n\n" +
				"2. Modify the file to set session parameters and player limits.\n\n" +
				"3. Official Documentation: https://playdesolate.com/"
			},
			{
				"Savage Lands",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Edit 'ServerConfig.json' to modify building decay and world options.\n\n" +
				"3. Official Documentation: https://savagelands.wiki.gg/"
			},
			{
				"Fragmented",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to build 'Game.ini', then shut it down.\n\n" +
				"2. Edit the file to alter science progression multipliers and server properties.\n\n" +
				"3. Official Documentation: https://playfragmented.com/"
			},
			{
				"GRAV",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to provision 'Game.ini', then shut it down.\n\n" +
				"2. Update the INI parameters to change planet generation and server rules.\n\n" +
				"3. Official Documentation: https://playgrav.com/"
			},
			{
				"Eden Star",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to build 'GameUserSettings.ini', then shut it down.\n\n" +
				"2. Edit the INI configuration to adjust physics parameters and resource rules.\n\n" +
				"3. Official Documentation: https://edenstar.co.uk/"
			},
			{
				"Rokh",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Modify 'GameUserSettings.ini' to configure crafting stations and environmental hazards.\n\n" +
				"3. Official Documentation: https://rokh.io/"
			},
			{
				"Outpost Zero",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to create 'GameUserSettings.ini', then shut it down.\n\n" +
				"2. Edit the configuration settings to manage AI robot automation limits.\n\n" +
				"3. Official Documentation: https://outpostzerogame.com/"
			},
			{
				"Rend",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to create 'Game.ini', then shut it down.\n\n" +
				"2. Configure faction periods, faction rosters, and server rules inside the INI file.\n\n" +
				"3. Official Documentation: https://rendgame.com/"
			},
			{
				"Night of the Dead",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to build 'GameUserSettings.ini', then shut it down.\n\n" +
				"2. Modify the file to define zombie wave intensity and structural durability.\n\n" +
				"3. Official Documentation: https://nightofthedead.wiki.gg/"
			},
			{
				"Tower Unite",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Edit 'TowerGame.ini' to adjust plaza rules and connection settings.\n\n" +
				"3. Official Documentation: https://towerunite.com/"
			},
			{
				"Witch It",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to build 'GameUserSettings.ini', then shut it down.\n\n" +
				"2. Modify settings to set round lengths and hiding criteria.\n\n" +
				"3. Official Documentation: https://witchit.com/"
			},
			{
				"Shattered Skies",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to create initialization rules, then shut it down.\n\n" +
				"2. Edit the parameters to control loot spawns and pvp rules.\n\n" +
				"3. Official Documentation: https://playshatteredskies.com/"
			},
			{
				"Chivalry: Medieval Warfare",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to generate 'PCServer-UDKGame.ini', then shut it down.\n\n" +
				"2. Edit the file to manage map rotations and team damage rules.\n\n" +
				"3. Official Documentation: https://chivalrythegame.com/"
			},
			{
				"Farming Simulator 22",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Edit 'dedicatedServerConfig.xml' to define web admin passwords and farm save IDs.\n\n" +
				"3. Official Documentation: https://farming-simulator.com/"
			},
			{
				"Dinkum",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to create setup files, then shut it down.\n\n" +
				"2. Edit the generated configs to establish island names and visitor codes.\n\n" +
				"3. Official Documentation: https://dinkum.wiki.gg/"
			},
			{
				"Interstellar Rift",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to generate 'server.ini', then shut it down.\n\n" +
				"2. Edit 'server.ini' to manage station parameters and faction options.\n\n" +
				"3. Official Documentation: https://interstellarrift.wiki.gg/"
			},
			{
				"Orion: Prelude",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to build 'PCServer-UDKGame.ini', then shut it down.\n\n" +
				"2. Modify configuration parameters for dinosaur survival waves.\n\n" +
				"3. Official Documentation: https://orionprelude.com/"
			},
			{
				"Saurian",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Update the config files to define ecosystem criteria and simulation ticks.\n\n" +
				"3. Official Documentation: https://sauriangame.com/"
			},
			{
				"Factorio (Experimental)",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Edit 'server-settings.json' to input your official Factorio service credentials and user token for public listing visibility.\n\n" +
				"2. Official Documentation: https://wiki.factorio.com/Multiplayer#Server_settings"
			},
			{
				"Project Zomboid (Beta)",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Launch the server script once to generate the target INI file, then shut it down.\n\n" +
				"2. Edit the corresponding INI file under the server directory to configure sandbox variables and administrative privileges.\n\n" +
				"3. Official Documentation: https://pzwiki.net/wiki/Dedicated_Server"
			},
			{
				"The Isle (Legacy)",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Modify 'Game.ini' to set legacy branch server rules and admin passwords.\n\n" +
				"3. Official Documentation: https://theisle.wiki.gg/"
			},
			{
				"Barotrauma (Unstable)",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to create 'serversettings.xml', then shut it down.\n\n" +
				"2. Edit the XML document to customize campaign parameters and sub settings.\n\n" +
				"3. Official Documentation: https://underthewaves.wiki.gg/wiki/Barotrauma"
			},
			{
				"Palworld (Experimental)",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to build 'PalWorldSettings.ini', then shut it down.\n\n" +
				"2. Update the configuration keys to set experimental branch parameters and admin keys.\n\n" +
				"3. Official Documentation: https://tech.palworldgame.com/"
			},
			{
				"Star Wars Jedi Knight II: Jedi Outcast",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to generate 'server.cfg', then shut it down.\n\n" +
				"2. Edit the config script to set up game rules, map rotations, and passwords.\n\n" +
				"3. Official Documentation: https://lucasarts.fandom.com/"
			},
			{
				"OpenTTD",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Edit 'openttd.cfg' to modify company limits, economics, and server names.\n\n" +
				"3. Official Documentation: https://wiki.openttd.org/"
			},
			{
				"Halo: The Master Chief Collection",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to build configuration profiles, then shut it down.\n\n" +
				"2. Adjust server settings files to define playlist rotations and player limitations.\n\n" +
				"3. Official Documentation: https://support.halowaypoint.com/"
			},
			{
				"Factorio (Space Age)",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Edit 'server-settings.json' to include your official Factorio service credentials and user token for space-age deployment.\n\n" +
				"2. Official Documentation: https://wiki.factorio.com/Multiplayer#Server_settings"
			},
			{
				"BATTALION: Legacy",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Run the server once to generate 'Game.ini', then shut it down.\n\n" +
				"2. Modify the file to define competitive settings and match lengths.\n\n" +
				"3. Official Documentation: https://bulkheadinteractive.com/"
			},
			{
				"Chivalry 2",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Boot the server once to build 'Engine.ini', then shut it down.\n\n" +
				"2. Edit the INI file to handle player limits, map pools, and session names.\n\n" +
				"3. Official Documentation: https://chivalry2.com/"
			},
			{
				"Depth",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Start the server completely for its initial startup, shut it down, and then configure your server settings!\n\n" +
				"2. Edit 'PCServer-DepthEngine.ini' to adjust game modes and team balance parameters.\n\n" +
				"3. Official Documentation: https://depthgame.com/"
			},
			{
				"Primal Carnage",
				"CRITICAL SETUP REQUIRED:\n\n" +
				"1. Initialize the server once to provision 'PCServer-PrimalCarnage.ini', then shut it down.\n\n" +
				"2. Edit the configuration properties to set up match rules and passwords.\n\n" +
				"3. Official Documentation: https://primalcarnage.com/"
			}
		};

		public WarningDatabase(GameServer server)
		{
			InitializeComponent();
			ThemeManager.Apply(this);
			_server = server;

			// Ensure LinkBehavior is set so links are properly formatted as hyperlinks
			lblWarningText.Links.Clear();
			lblWarningText.LinkClicked += LblWarningText_LinkClicked;

			// Set the specific warning message and extract the link
			if (_messages.TryGetValue(server.Game, out string customMessage))
			{
				lblWarningText.Text = customMessage;
				FormatUrlLink(customMessage);

				if (server.Game.StartsWith("Minecraft", StringComparison.OrdinalIgnoreCase))
				{
					btnStart.Text = "I Agree";
					btnNo.Text = "Decline";
				}
			}
			else
			{
				lblWarningText.Text = "Configuration required before the first launch. \n1. If the Config file is missing in the game then the server needs to run once to create the config file. \n2. Then shut the server down and go to `Server Actions -> Server Options -> Edit Config File` and edit the config file. \n3. Some Servers use their own server manager in the game to fully setup the server.";
			}
		}

		// Helper to find the URL in the text and set the active link area
		private void FormatUrlLink(string text)
		{
			lblWarningText.Links.Clear();

			int linkIndex = text.IndexOf("http");
			if (linkIndex != -1)
			{
				// Find where the URL ends (by looking for whitespace, newline, or end of string)
				int spaceIndex = text.IndexOfAny(new char[] { ' ', '\n', '\r' }, linkIndex);
				int linkLength = (spaceIndex != -1) ? spaceIndex - linkIndex : text.Length - linkIndex;

				string url = text.Substring(linkIndex, linkLength).Trim();

				// Add only the exact URL range to the LinkLabel
				lblWarningText.Links.Add(linkIndex, linkLength, url);
			}
		}

		// Event handler to open the URL in the default web browser
		private void LblWarningText_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (e.Link.LinkData != null)
			{
				string targetUrl = e.Link.LinkData.ToString();

				try
				{
					Process.Start(new ProcessStartInfo
					{
						FileName = targetUrl,
						UseShellExecute = true
					});
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Failed to open link: {ex.Message}", "Link Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void btnStart_Click(object sender, EventArgs e)
		{
			if (_server.Game.Equals("Minecraft Java", StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					SynixApp.FileFolderHandler.FileHandler.Create(_server.InstallPath, "eula.txt", "eula=true");
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Could not write eula.txt. Please check folder permissions:\n{ex.Message}", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
			}

			_server.IsFirstBoot = false;
			try
			{
				FileHandler.SaveServers();

				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error: {ex.Message}");
			}
		}

		private void btnNo_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}
