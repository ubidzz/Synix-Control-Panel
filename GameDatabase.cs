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
		public List<string> Maps { get; set; }
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
		public static List<GameInfo> GetGameList()
		{
			return [
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
					DefaultArgs = "-batchmode +server.level \"Procedural Map\" +server.seed 12345 +server.worldsize 3000 +server.identity \"{Identity}\" +server.hostname \"{Hostname}\" +rcon.port 28016 +rcon.password \"changeit\" +rcon.web 1",
					DefaultPort = 28015,
					DefaultQueryPort = 28016,
					Maps = ["Procedural Map", "Barren", "Hapaislands"]
				},
				new() {
					Name = "7 Days to Die",
					AppID = "294420",
					ExeName = "7DaysToDieServer.exe",
					DefaultArgs = "-configfile=serverconfig.xml -quit -batchmode -nographics",
					DefaultPort = 26900,
					DefaultQueryPort = 26900,
					Maps = ["Navezgane", "Pregen01", "Random Gen"]
				},
				new() {
					Name = "ARK: Survival Evolved",
					AppID = "376030",
					ExeName = "ShooterGameServer.exe",
					DefaultArgs = "TheIsland?listen?SessionName=\"{Hostname}\"?ServerPassword=changeit -log",
					DefaultPort = 7777,
					DefaultQueryPort = 27015,
					Maps = ["TheIsland", "TheCenter", "Ragnarok"]
				},
				new() {
					Name = "Palworld",
					AppID = "2394010",
					ExeName = "PalServer-Win64-Shipping.exe",
					DefaultArgs = "EpicApp=PalServer -useperfthreads -NoAsyncLoadingThread",
					DefaultPort = 8211,
					DefaultQueryPort = 27015,
					Maps = ["Default"]
				},
				new() {
					Name = "Valheim",
					AppID = "896660",
					ExeName = "valheim_server.exe",
					DefaultArgs = "-nographics -batchmode -name \"{Hostname}\" -port 2456 -world \"{Identity}\" -password \"changeit\"",
					DefaultPort = 2456,
					DefaultQueryPort = 2457,
					Maps = ["Dedicated"]
				},
				new() {
					Name = "Counter-Strike 2",
					AppID = "730",
					ExeName = "cs2.exe",
					DefaultArgs = "-dedicated +map de_dust2 +servercfgfile server.cfg",
					DefaultPort = 27015,
					DefaultQueryPort = 27015,
					Maps = ["de_dust2", "de_mirage", "de_inferno"]
				},
				new() {
					Name = "Sons of the Forest",
					AppID = "2465200",
					ExeName = "SonsOfTheForestDS.exe",
					DefaultArgs = "-dedicated",
					DefaultPort = 8766,
					DefaultQueryPort = 27016,
					Maps = ["Default"]
				},
				new() {
					Name = "Conan Exiles",
					AppID = "443030",
					ExeName = "ConanSandboxServer.exe",
					DefaultArgs = "-log -MaxPlayers=40",
					DefaultPort = 7777,
					DefaultQueryPort = 27015,
					Maps = ["TheExiledLands", "Siptah"]
				}
			];
		}

		public static GameInfo GetGame(string name)
		{
			return GetGameList().Find(g => g.Name == name);
		}
	}
}