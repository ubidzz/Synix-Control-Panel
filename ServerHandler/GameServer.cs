using System.Diagnostics;
using System.Text.Json.Serialization;
using static Synix_Control_Panel.GameDatabase;

public class GameInfo
{
	public string Game { get; init; } = string.Empty;

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
	public PostInstallStep[]? PostInstallSteps { get; init; }
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
}