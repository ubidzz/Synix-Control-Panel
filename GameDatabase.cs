using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Game_Server_Control_Panel
{
	// These MUST be outside the GameDatabase class to be visible everywhere
	public class GameInfo
	{
		public string Game { get; set; } = string.Empty; // Changed back from Name
		public string AppID { get; set; } = string.Empty;
		public string ExeName { get; set; } = string.Empty;
		public string ExtraArgs { get; set; } = string.Empty; // Changed back from DefaultArgs
		public int Port { get; set; } // Changed back from DefaultPort
		public int QueryPort { get; set; } // Changed back from DefaultQueryPort
		public List<string> Maps { get; set; } = [];
	}

	public class GameServer : GameInfo
	{
		public string InstallPath { get; set; } = string.Empty;
		public string ServerName { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string Status { get; set; } = "Stopped";
		public int MaxPlayers { get; set; } = 10;
		public string WorldName { get; set; } = "NewWorld";
		public bool IsDefaultPath { get; set; } = true;

		[JsonIgnore]
		public Process? RunningProcess { get; set; }
	}

	public static class GameDatabase
	{
		public static List<GameInfo> GetGameList() => [
			new() {
			Game = "StarRupture",
			AppID = "3809400",
			ExeName = "StarRuptureServer.exe",
			ExtraArgs = "-log -nosound",
			Port = 8777,
			QueryPort = 27015,
			Maps = ["Default", "Experimental"]
		},
		new() {
			Game = "Soulmask",
			AppID = "2646460",
			ExeName = "SoulmaskServer.exe",
			ExtraArgs = "-log",
			Port = 8777,
			QueryPort = 27015,
			Maps = ["MainWorld"]
		},
		new() {
			Game = "Rust",
			AppID = "258550",
			ExeName = "RustDedicated.exe",
			ExtraArgs = "-batchmode +server.level \"Procedural Map\" +server.identity \"{Identity}\" +server.hostname \"{Hostname}\"",
			Port = 28015,
			QueryPort = 28016,
			Maps = ["Procedural Map", "Barren"]
		},
		new() {
			Game = "7 Days to Die",
			AppID = "294420",
			ExeName = "7DaysToDieServer.exe",
			ExtraArgs = "-configfile=serverconfig.xml -quit -batchmode -nographics",
			Port = 26900,
			QueryPort = 26900,
			Maps = ["Navezgane", "Pregen01"]
		},
		new() {
			Game = "DayZ",
			AppID = "223350",
			ExeName = "DayZServer_x64.exe",
			ExtraArgs = "-config=serverDZ.cfg -port=2302 -BEpath= -logs= -profiles=Profiles",
			Port = 2302,
			QueryPort = 27016,
			Maps = ["ChernarusPlus", "Livonia"]
		},
		new() {
			Game = "Enshrouded",
			AppID = "2278520",
			ExeName = "enshrouded_server.exe",
			ExtraArgs = "",
			Port = 15636,
			QueryPort = 15637,
			Maps = ["Default"]
		},
		new() {
			Game = "Icarus",
			AppID = "2089390",
			ExeName = "IcarusServer-Win64-Shipping.exe",
			ExtraArgs = "-Log",
			Port = 17777,
			QueryPort = 27015,
			Maps = ["Olympus", "Styx", "Prometheus"]
		}
		];

		public static GameInfo? GetGame(string name) => GetGameList().Find(g => g.Game == name);
	}
}