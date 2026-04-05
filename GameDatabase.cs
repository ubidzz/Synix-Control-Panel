using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Game_Server_Control_Panel
{
	// These MUST be outside the GameDatabase class to be visible everywhere
	public class GameInfo
	{
		public string Name { get; set; } = string.Empty;
		public string AppID { get; set; } = string.Empty;
		public string ExeName { get; set; } = string.Empty;
		public string DefaultArgs { get; set; } = string.Empty;
		public int DefaultPort { get; set; }
		public int DefaultQueryPort { get; set; }
		public List<string> Maps { get; set; } = [];
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
		public static List<GameInfo> GetGameList() => [
			new() {
			Name = "StarRupture",
			AppID = "3809400",
			ExeName = "StarRuptureServer.exe",
			DefaultArgs = "-log -nosound",
			DefaultPort = 8777,
			DefaultQueryPort = 27015,
			Maps = ["Default", "Experimental"]
		},
		new() {
			Name = "Soulmask",
			AppID = "2646460",
			ExeName = "SoulmaskServer.exe",
			DefaultArgs = "-log",
			DefaultPort = 8777,
			DefaultQueryPort = 27015,
			Maps = ["MainWorld"]
		},
		new() {
			Name = "Rust",
			AppID = "258550",
			ExeName = "RustDedicated.exe",
			DefaultArgs = "-batchmode +server.level \"Procedural Map\" +server.identity \"{Identity}\" +server.hostname \"{Hostname}\"",
			DefaultPort = 28015,
			DefaultQueryPort = 28016,
			Maps = ["Procedural Map", "Barren"]
		},
		new() {
			Name = "7 Days to Die",
			AppID = "294420",
			ExeName = "7DaysToDieServer.exe",
			DefaultArgs = "-configfile=serverconfig.xml -quit -batchmode -nographics",
			DefaultPort = 26900,
			DefaultQueryPort = 26900,
			Maps = ["Navezgane", "Pregen01"]
		},
		new() {
			Name = "DayZ",
			AppID = "223350",
			ExeName = "DayZServer_x64.exe",
			DefaultArgs = "-config=serverDZ.cfg -port=2302 -BEpath= -logs= -profiles=Profiles",
			DefaultPort = 2302,
			DefaultQueryPort = 27016,
			Maps = ["ChernarusPlus", "Livonia"]
		},
		new() {
			Name = "Enshrouded",
			AppID = "2278520",
			ExeName = "enshrouded_server.exe",
			DefaultArgs = "",
			DefaultPort = 15636,
			DefaultQueryPort = 15637,
			Maps = ["Default"]
		},
		new() {
			Name = "Icarus",
			AppID = "2089390",
			ExeName = "IcarusServer-Win64-Shipping.exe",
			DefaultArgs = "-Log",
			DefaultPort = 17777,
			DefaultQueryPort = 27015,
			Maps = ["Olympus", "Styx", "Prometheus"]
		}
		];

		public static GameInfo? GetGame(string name) => GetGameList().Find(g => g.Name == name);
	}
}