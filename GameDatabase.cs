using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Game_Server_Control_Panel
{
	public class GameInfo
	{
		public string Name { get; set; }
		public string AppId { get; set; }
		public string ExeName { get; set; }
		public string DefaultArgs { get; set; }

		public GameInfo(string name, string appId, string exeName, string defaultArgs = "")
		{
			Name = name;
			AppId = appId;
			ExeName = exeName;
			DefaultArgs = defaultArgs;
		}
	}

	// --- NEW CLASS ADDED HERE ---
	public class GameServer
	{
		public string Name { get; set; }
		public string Game { get; set; }
		public int Port { get; set; }
		public int QueryPort { get; set; }
		public string Password { get; set; }
		public string Status { get; set; }

		// This tracks the running process while the app is open
		[JsonIgnore]
		public Process RunningProcess { get; set; }
	}

	public static class GameDatabase
	{
		public static readonly List<GameInfo> SupportedGames = new List<GameInfo>
		{
			new GameInfo("Soulmask", "3017300", "SoulmaskServer.exe", "-log"),
			new GameInfo("StarRupture", "2519830", "StarRuptureServer.exe", ""),
			new GameInfo("Palworld", "2394010", "PalServer.exe", "")
		};

		public static GameInfo GetGame(string name)
		{
			return SupportedGames.Find(g => g.Name == name);
		}
	}
}