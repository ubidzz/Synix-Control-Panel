/*
 * Copyright (c) 2026 ubidzz. All Rights Reserved.
 *
 * This file is part of Synix Control Panel.
 *
 * This code is provided for transparent viewing and personal use only.
 * Unauthorized distribution, public modification, or commercial 
 * use of this source code or the compiled executable is strictly 
 * prohibited. Please refer to the LICENSE file in the root 
 * directory for full terms.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Synix_Control_Panel.ServerHandler
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

		public List<string> GameModes { get; internal set; } = [];
	}

	public class GameServer : GameInfo
	{
		public string InstallPath { get; set; } = string.Empty;
		public string ServerName { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string AdminPassword { get; set; } = string.Empty;
		public string Status { get; set; } = "Offline";
		public int MaxPlayers { get; set; } = 10;
		public string WorldName { get; set; } = "NewWorld";
		public bool IsDefaultPath { get; set; } = true;
		public int? PID { get; set; }
		[JsonIgnore]
		public Process? RunningProcess { get; set; }
		public string GameMode { get; internal set; } = "PVE";
	}
}