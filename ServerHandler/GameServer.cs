using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Game_Server_Control_Panel.ServerHandler
{
	public class GameInfo
	{
		// Keep 'init' for Game because it's in the JSON and set during creation
		public string Game { get; init; } = string.Empty;

		// CHANGE THESE TO 'set' so JsonManager can fill them after loading
		[JsonIgnore]
		public string AppID { get; set; } = string.Empty;

		[JsonIgnore]
		public string ExeName { get; set; } = string.Empty;

		[JsonIgnore]
		public string RequiredArgs { get; set; } = string.Empty;

		[JsonIgnore]
		public List<string> Maps { get; set; } = [];

		public int Port { get; set; }
		public int QueryPort { get; set; }
		public string ExtraArgs { get; set; } = string.Empty;
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
		public int? PID { get; set; }

		[JsonIgnore]
		public Process? RunningProcess { get; set; }
	}
}