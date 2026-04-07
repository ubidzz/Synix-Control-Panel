using System.Diagnostics;
using System.Text.Json.Serialization;

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

	// Change 'internal set' to 'public set' so it saves/loads in JSON
	// We removed [JsonIgnore] because you want this to save
	public List<string> GameModes { get; set; } = [];
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

	// Change to 'public set' to ensure the user's choice is saved
	public string GameMode { get; set; } = "PVE";
	[JsonIgnore]
	public double LastCpuMillis { get; set; } = 0;
	[JsonIgnore]
	public DateTime LastSampleTime { get; set; } = DateTime.Now;
}