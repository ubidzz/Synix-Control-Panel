using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Game_Server_Control_Panel
{
	public class GameInfo
	{
		public string Game { get; set; } = string.Empty; 
		public string AppID { get; set; } = string.Empty;
		public string ExeName { get; set; } = string.Empty;
		public string ExtraArgs { get; set; } = string.Empty; 
		public int Port { get; set; } 
		public int QueryPort { get; set; } 
		public List<string> Maps { get; set; } = new List<string>();
		public string RequiredArgs { get; set; }
	}

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

		// Add this so server.PID = proc.Id works
		public int? PID { get; set; }

		[JsonIgnore]
		public Process? RunningProcess { get; set; }
	}

	public static class GameDatabase
	{
		private static List<GameInfo> games = [
			new() {
				Game = "StarRupture",
				AppID = "3333140",
				ExeName = @"StarRupture\Binaries\Win64\StarRuptureServerEOS-Win64-Shipping.exe",
				RequiredArgs = "-server -log -Port={port} -Password={pass} -Name=\"{ServerName}\"",
				Port = 7777,
				QueryPort = 27015,
				ExtraArgs = "",
				Maps = ["MainWorld"]
			},
			new() {
				Game = "Soulmask",
				AppID = "3017310",
				// FIXED: Changed from SoulmaskServer to WSServer as seen in your folder
				ExeName = @"WS\Binaries\Win64\WSServer-Win64-Shipping.exe",
				RequiredArgs = "{map} -server -log -NOSTEAM -SteamAppId={appid} -Port={port} -QueryPort={query} -PSW=\"{pass}\" -adminpsw=\"{adminpass}\" -SteamServerName=\"{ServerName}\"",
				Port = 8777,
				QueryPort = 27015,
				ExtraArgs = "",
				Maps = ["Level01_Main"]
			},
			new() {
				Game = "7 Days to Die",
				AppID = "294420",
				ExeName = "7DaysToDieServer.exe",
				RequiredArgs = "-configfile=serverconfig.xml -port={port} -quit -batchmode -nographics",
				Port = 26900,
				QueryPort = 26900,
				ExtraArgs = "",
				Maps = ["Navezgane", "Pregen01"]
			},
			new() {
				Game = "Rust",
				AppID = "258550",
				ExeName = "RustDedicated.exe",
				RequiredArgs = "-batchmode +server.port {port} +server.queryport {query} +server.hostname \"{name}\"",
				Port = 28015,
				QueryPort = 28016,
				ExtraArgs = "",
				Maps = ["Procedural Map"]
			},
			new() {
				Game = "DayZ",
				AppID = "223350",
				ExeName = "DayZServer_x64.exe",
				RequiredArgs = "-config=serverDZ.cfg -port={port} -BEpath= -logs= -profiles=Profiles",
				Port = 2302,
				QueryPort = 27016,
				ExtraArgs = "",
				Maps = ["Chernarus", "Livonia"]
			},
			new() {
				Game = "Enshrouded",
				AppID = "2278520",
				ExeName = "enshrouded_server.exe",
				RequiredArgs = "",
				Port = 15636,
				QueryPort = 15637,
				ExtraArgs = "",
				Maps = ["Enshrouded"]
			},
			new() {
				Game = "Icarus",
				AppID = "2089390",
				ExeName = @"Icarus\Binaries\Win64\IcarusServer-Win64-Shipping.exe",
				RequiredArgs = "-Log -port={port} -queryport={query}",
				Port = 17777,
				QueryPort = 27015,
				ExtraArgs = "",
				Maps = ["Styx", "Olympus", "Prometheus"]
			},
			new() {
				Game = "Valheim",
				AppID = "896660",
				ExeName = "valheim_server.exe",
				RequiredArgs = "-nographics -batchmode -name \"{name}\" -port {port} -world \"{world}\" -password \"{pass}\"",
				Port = 2456,
				QueryPort = 2457,
				ExtraArgs = "",
				Maps = ["Dedicated"]
			},
			new() {
				Game = "Palworld",
				AppID = "2394010",
				ExeName = "PalServer.exe",
				RequiredArgs = "-port={port} -queryport={query} -AdminPassword=\"{pass}\"",
				Port = 8211,
				QueryPort = 27015,
				ExtraArgs = "",
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