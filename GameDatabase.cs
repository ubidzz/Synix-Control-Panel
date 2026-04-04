using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Game_Server_Control_Panel
{
	public class GameInfo
	{
		public string Name { get; set; }
		public string AppID { get; set; }
		public string ExeName { get; set; }
		public string DefaultArgs { get; set; }
		public int DefaultPort { get; set; }      // NEW
		public int DefaultQueryPort { get; set; } // NEW
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
		private static List<GameInfo> games = new List<GameInfo>
        {
            new GameInfo { 
                Name = "Soulmask", 
                AppID = "3017310", 
                ExeName = "SoulmaskServer.exe", 
                DefaultArgs = "-log",
                DefaultPort = 8777,
                DefaultQueryPort = 27015
            },
            new GameInfo { 
                Name = "StarRupture", 
                AppID = "3809400", 
                ExeName = "StarRuptureServer.exe", 
                DefaultArgs = "-log -nosound",
                DefaultPort = 8777,
                DefaultQueryPort = 27015
            }
        };

        // This method is what MainGUI line 272 is looking for!
        public static GameInfo GetGame(string name)
        {
            return games.Find(g => g.Name == name);
        }

        public static List<GameInfo> GetGameList()
        {
            return games;
        }
	};
}