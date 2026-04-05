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
		public List<string> Maps { get; set; } = new List<string>();
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

		// Add this so server.PID = proc.Id works
		public int? PID { get; set; }

		[JsonIgnore]
		public Process? RunningProcess { get; set; }
	}

	public static class GameDatabase
	{
		private static List<GameInfo> games =
[
			new() {
				Game = "StarRupture",
				AppID = "3333140",
				ExeName = @"StarRupture\Binaries\Win64\StarRuptureServerEOS-Win64-Shipping.exe",
				Port = 7777,
				QueryPort = 27015,
				ExtraArgs = "-log",
				Maps = ["MainWorld"]
			},
			new() {
				Game = "Soulmask",
				AppID = "3017310",
				ExeName = @"WS\Binaries\Win64\SoulmaskServer-Win64-Shipping.exe",
				Port = 8777,
				QueryPort = 27015,
				ExtraArgs = "-log",
				Maps = ["MainWorld"]
			},
			new() {
				Game = "7 Days to Die",
				AppID = "294420",
				ExeName = "7DaysToDieServer.exe",
				Port = 26900,
				QueryPort = 26900,
				ExtraArgs = "-configfile=serverconfig.xml -quit -batchmode -nographics",
				Maps = ["Navezgane", "Pregen01"]
			},
			new() {
				Game = "Rust",
				AppID = "258550",
				ExeName = "RustDedicated.exe",
				Port = 28015,
				QueryPort = 28016,
				ExtraArgs = "-batchmode +server.port 28015",
				Maps = ["Procedural Map"]
			},
			new() {
				Game = "DayZ",
				AppID = "223350",
				ExeName = "DayZServer_x64.exe",
				Port = 2302,
				QueryPort = 27016,
				ExtraArgs = "-config=serverDZ.cfg -port=2302 -BEpath= -logs= -profiles=Profiles",
				Maps = ["Chernarus", "Livonia"]
			},
			new() {
				Game = "Enshrouded",
				AppID = "2278520",
				ExeName = "enshrouded_server.exe",
				Port = 15636,
				QueryPort = 15637,
				ExtraArgs = "",
				Maps = ["Enshrouded"]
			},
			new() {
				Game = "Icarus",
				AppID = "2089390",
				// FIXED: Icarus requires the deep path to the actual engine binary
				ExeName = @"Icarus\Binaries\Win64\IcarusServer-Win64-Shipping.exe",
				Port = 17777,
				QueryPort = 27015,
				ExtraArgs = "-Log",
				Maps = ["Styx", "Olympus", "Prometheus"]
			},
			new() {
				Game = "Valheim",
				AppID = "896660",
				// FIXED: Valheim dedicated server binary name
				ExeName = "valheim_server.exe",
				Port = 2456,
				QueryPort = 2457,
				ExtraArgs = "-nographics -batchmode -name \"MyServer\" -port 2456 -world \"Dedicated\" -password \"secret\"",
				Maps = ["Dedicated"]
			},
			new() {
				Game = "Palworld",
				AppID = "2394010",
				ExeName = "PalServer.exe",
				Port = 8211,
				QueryPort = 27015,
				ExtraArgs = "Port=8211,QueryPort=27015",
				Maps = ["DefaultWorld"]
			}
		];

		// Returns the full list for the dropdowns
		public static List<GameInfo> GetGameList()
		{
			return games;
		}

		// Returns a single game for the settings
		public static GameInfo GetGame(string gameName)
		{
			return games.FirstOrDefault(g => g.Game.Equals(gameName, StringComparison.OrdinalIgnoreCase))!;
		}
}
}