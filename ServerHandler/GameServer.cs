using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Game_Server_Control_Panel.ServerHandler
{
	public class GameInfo
	{
		// 'init' locks these down. They can be set ONCE in the GameDatabase, and never changed again.
		public string Game { get; init; } = string.Empty;

		[JsonIgnore]
		public string AppID { get; init; } = string.Empty;

		[JsonIgnore]
		public string ExeName { get; init; } = string.Empty;

		[JsonIgnore]
		public string RequiredArgs { get; init; } = string.Empty;

		[JsonIgnore]
		public List<string> Maps { get; init; } = [];

		// 'set' keeps these flexible because the user can type new ports or args in the UI
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