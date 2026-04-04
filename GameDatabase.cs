using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Game_Server_Control_Panel
{
	// These MUST be outside the GameDatabase class to be visible everywhere
	public class GameInfo
	{
		public string Name { get; set; }
		public string AppID { get; set; }
		public string ExeName { get; set; }
		public string DefaultArgs { get; set; }
		public int DefaultPort { get; set; }
		public int DefaultQueryPort { get; set; }
		public List<string> Maps { get; set; } = new List<string>();
	}

	public class GameServer
	{
		public string Name { get; set; }
		public string Game { get; set; }
		public int Port { get; set; }
		public int QueryPort { get; set; }
		public string Password { get; set; }
		public string Status { get; set; } = "Stopped";
		public string InstallPath { get; set; }
		public int MaxPlayers { get; set; } = 10;
		public string WorldName { get; set; } = "NewWorld";
		public string ExtraArgs { get; set; } = "-log";
		public bool IsDefaultPath { get; set; }

		[JsonIgnore]
		public Process RunningProcess { get; set; }
	}

	public static class GameDatabase
	{
		private static List<GameInfo> SupportedGames = new List<GameInfo>
		{
			new GameInfo
			{
				Name = "StarRupture",
				AppID = "3809400",
				ExeName = "StarRuptureServer.exe",
				DefaultArgs = "-log -nosound",
				DefaultPort = 8777,
				DefaultQueryPort = 27015,
				Maps = new List<string> { "Default", "Experimental" }
			},

			new GameInfo
			{
				Name = "Soulmask",
				AppID = "3017310",
				ExeName = "SoulmaskServer.exe",
				DefaultArgs = "-log",
				DefaultPort = 8777,
				DefaultQueryPort = 27015,
				Maps = new List<string> { "MainWorld" }
			},

			new GameInfo
			{
				Name = "7 Days to Die",
				AppID = "294420",
				ExeName = "7DaysToDieServer.exe",
				DefaultArgs = "-configfile=serverconfig.xml -dedicated",
				DefaultPort = 26900,
				DefaultQueryPort = 26901,
				Maps = new List<string> { "Navezgane", "PREGEN01", "PREGEN02", "RWG" }
			},

			new GameInfo
			{
				Name = "Palworld",
				AppID = "2394010",
				ExeName = "PalServer-Win64-Shipping.exe",
				DefaultArgs = "-log -port=8211",
				DefaultPort = 8211,
				DefaultQueryPort = 27015,
				Maps = new List<string> { "Palpagos" }
			},

			new GameInfo
			{
				Name = "ARK: Survival Ascended",
				AppID = "2430930",
				ExeName = "ArkAscendedServer.exe",
				DefaultArgs = "-log -server",
				DefaultPort = 7777,
				DefaultQueryPort = 27015,
				Maps = new List<string> { "TheIsland_WP", "TheCenter_WP", "ScorchedEarth_WP" }
			},

			new GameInfo
			{
				Name = "Abiotic Factor",
				AppID = "2857200",
				ExeName = "AbioticFactorServer.exe",
				DefaultArgs = "-log",
				DefaultPort = 7777,
				DefaultQueryPort = 27015,
				Maps = new List<string> { "Facility" }
			},

			new GameInfo
			{
				Name = "Satisfactory",
				AppID = "1690800",
				ExeName = "FactoryServer.exe",
				DefaultArgs = "-log -unattended",
				DefaultPort = 7777,
				DefaultQueryPort = 15000,
				Maps = new List<string> { "GrassFields", "RockyDesert", "NorthernForest", "DuneDesert" }
			},

			new GameInfo
			{
				Name = "Valheim",
				AppID = "896660",
				ExeName = "valheim_server.exe",
				DefaultArgs = "-nographics -batchmode -public 0",
				DefaultPort = 2456,
				DefaultQueryPort = 2457,
				Maps = new List<string> { "Dedicated" }
			},

			new GameInfo
			{
				Name = "Sons of the Forest",
				AppID = "2465200",
				ExeName = "SonsOfTheForestDS.exe",
				DefaultArgs = "-userdatapath ./userdata",
				DefaultPort = 8766,
				DefaultQueryPort = 27016,
				Maps = new List<string> { "Site2" }
			},

			new GameInfo
			{
				Name = "Icarus",
				AppID = "2089390",
				ExeName = "IcarusServer.exe",
				DefaultArgs = "-log",
				DefaultPort = 17777,
				DefaultQueryPort = 27015,
				Maps = new List<string> { "Olympus", "Styx", "Prometheus" }
			}
		};

		public static GameInfo GetGame(string name) => SupportedGames.Find(g => g.Name == name);
		public static List<GameInfo> GetGameList() => SupportedGames;
	}
}