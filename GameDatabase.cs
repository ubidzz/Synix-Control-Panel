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
		public string AppID { get; set; } = string.Empty;
		public string ExeName { get; set; } = string.Empty;
		public string RequiredArgs { get; set; } = string.Empty;
		public int Port { get; set; }
		public int QueryPort { get; set; }
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
			new() {
				Game = "StarRupture",
				AppID = "3333140",
				ExeName = @"StarRupture\Binaries\Win64\StarRuptureServerEOS-Win64-Shipping.exe",
				RequiredArgs = "-server -log -port={port} -password={pass} -name=\"{ServerName}\"",
				Port = 7777,
				QueryPort = 27015,
				Maps = ["MainWorld"]
			},
			new() {
				Game = "Soulmask",
				AppID = "3017310",
				ExeName = @"WS\Binaries\Win64\WSServer-Win64-Shipping.exe",
				RequiredArgs = "{map} -server -log -NOSTEAM -SteamAppId={appid} -Port={port} -QueryPort={query} -PSW=\"{pass}\" -adminpsw=\"{adminpass}\" -MaxPlayers={MaxPlayers} -SteamServerName=\"{ServerName}\" -forcepassthrough",
				Port = 8777,
				QueryPort = 27015,
				Maps = ["Level01_Main"]
			},
			new() {
				Game = "7 Days to Die",
				AppID = "294420",
				ExeName = "7DaysToDieServer.exe",
				RequiredArgs = "-configfile=serverconfig.xml -port={port} -quit -batchmode -nographics",
				Port = 26900,
				QueryPort = 26900,
				Maps = ["Navezgane", "Pregen01"]
			},
            // I standardized the tags here for Valheim/Rust to match your Soulmask logic
            new() {
				Game = "Rust",
				AppID = "258550",
				ExeName = "RustDedicated.exe",
				RequiredArgs = "-batchmode +server.port {port} +server.queryport {query} +server.hostname \"{ServerName}\"",
				Port = 28015,
				QueryPort = 28016,
				Maps = ["Procedural Map"]
			},
			new() {
				Game = "Valheim",
				AppID = "896660",
				ExeName = "valheim_server.exe",
				RequiredArgs = "-nographics -batchmode -name \"{ServerName}\" -port {port} -world \"{map}\" -password \"{pass}\"",
				Port = 2456,
				QueryPort = 2457,
				Maps = ["Dedicated"]
			},
			new() {
				Game = "Palworld",
				AppID = "2394010",
				ExeName = "PalServer.exe", 
				// Using {pass} for the AdminPassword since Palworld uses one main password for admin rights
				RequiredArgs = "-port={port} -queryport={query} -AdminPassword=\"{pass}\"",
				Port = 8211,
				QueryPort = 27015,
				Maps = ["DefaultWorld"]
			}
            // Add more games following the same {tag} pattern...
        ];

		public static List<GameInfo> GetGameList() => games;

		public static GameInfo? GetGame(string gameName)
		{
			return games.FirstOrDefault(g => g.Game.Equals(gameName, StringComparison.OrdinalIgnoreCase));
		}
	}
}