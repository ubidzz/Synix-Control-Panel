// Copyright (c) 2026 ubidzz. All Rights Reserved.
//
// This file is part of Synix Control Panel.
//
// This code is provided for transparent viewing and personal use only.
// Unauthorized distribution, public modification, or commercial
// use of this source code or the compiled executable is strictly
// prohibited. Please refer to the LICENSE file in the root
// directory for full terms.
// ADD THIS LINE:
using Synix_Control_Panel.ServerHandler;

using System.Diagnostics;
using System.Text.Json.Serialization;
using static Synix_Control_Panel.Database.GameDatabase;

public class GameInfo
{
	public string Game { get; init; } = string.Empty;
	[JsonIgnore]
	public bool NeedsConfigWarning { get; internal set; }
	[JsonIgnore]
	public string WarningMessage { get; set; } = "This game requires configuration before it can boot properly.";
	public ConfigFormat Format { get; set; }
	[JsonIgnore]
	public string RelativeConfigPath { get; init; } = string.Empty;
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
	public List<string> GameModes { get; set; } = [];
	public string RconSyntax { get; init; } = "";
	[JsonIgnore]
	public PostInstallStep[]? PostInstallSteps { get; init; }


	public bool IsScheduledRestartEnabled { get; set; } = false;
	public string RestartTime { get; set; } = "04:00";

	// Index 0 = Sunday, 1 = Monday, etc. (Matches .NET DayOfWeek)
	public bool[] RestartDays { get; set; } = new bool[7] { true, true, true, true, true, true, true };

	public string LastMaintenanceDate { get; set; } = "";
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

	public string GameMode { get; set; } = "PVE";

	[JsonIgnore]
	public double LastCpuMillis { get; set; } = 0;

	[JsonIgnore]
	public DateTime LastSampleTime { get; set; } = DateTime.Now;

	public string? SelectedMode { get; set; } = "PVE";
	public bool EnableRcon { get; set; } = false;
	public int RconPort { get; set; }
	public string RconPassword { get; set; } = "";
	public bool IsFirstBoot { get; set; } = true;
	public string WorldSeed { get; set; } = "12345";
}