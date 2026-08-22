// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
//
// LEGAL NOTICE:
// This source code is proprietary and confidential.
// 1. Permission is granted for PERSONAL, NON-COMMERCIAL use only.
// 2. You may modify this code for your own use, but you may NOT redistribute,
//    rebrand, or sell this code or derivative works without written consent.
// 3. The "Synix" brand and logic remain the property of Jason Turner.
// ============================================================================
using Synix_Control_Panel.SynixApp.ServerHandler;
using System.Diagnostics;
using System.Text.Json.Serialization;
using static Synix_Control_Panel.SynixApp.Database.GameDatabase;
using static Synix_Control_Panel.SynixEngine.Core;

public enum ServerProbeProtocol
{
	Auto,
	A2S,
	EpicOnlineServices,
	RestApi,
	Tcp
}

public enum ConfigFileCreationMode
{
	Unknown,
	GameGenerated,
	SynixTemplate,
	LaunchArgumentsOnly
}

public class GameInfo
{
	public string Game { get; set; } = string.Empty;
	[JsonIgnore]
	public System.Drawing.Image DisplayIcon { get; set; }
	[JsonIgnore]
	public bool HasAnnouncedOnline { get; set; } = false;
	[JsonIgnore]
	public bool NeedsConfigWarning { get; internal set; }
	[JsonIgnore]
	public string WarningMessage { get; set; } = "This game requires configuration before it can boot properly.";
	public ConfigFormat Format { get; set; }
	[JsonIgnore]
	public ConfigFileCreationMode ConfigFileCreation { get; init; } =
		ConfigFileCreationMode.Unknown;
	[JsonIgnore]
	public string RelativeConfigPath { get; init; } = string.Empty;
	[JsonIgnore]
	public string ExternalDataFolderName { get; init; } = string.Empty;
	[JsonIgnore]
	public string[] RequiredLaunchFiles { get; init; } = [];
	[JsonIgnore]
	public string[] OptionalLaunchFiles { get; init; } = [];
	[JsonIgnore]
	public string LaunchFileSetupInstructions { get; init; } = string.Empty;
	[JsonIgnore]
	public string AppID { get; set; } = string.Empty;
	[JsonIgnore]
	public bool RequiresSteamLogin { get; init; }
	[JsonIgnore]
	public string ExeName { get; set; } = string.Empty;
	[JsonIgnore]
	public string DownloadUrl { get; init; } = string.Empty;
	public int WorldSize { get; set; }
	public string WorldSeed { get; set; } = "12345";
	[JsonIgnore]
	public string RequiredArgs { get; set; } = string.Empty;
	[JsonIgnore]
	public List<string> Maps { get; set; } = [];
	public int Port { get; set; }
	public int QueryPort { get; set; }
	public int? AppPort { get; set; }

	public string ExtraArgs { get; set; } = string.Empty;
	public List<string> GameModes { get; set; } = [];
	public string RconSyntax { get; init; } = "";
	[JsonIgnore]
	public PostInstallStep[]? PostInstallSteps { get; init; }
	[JsonIgnore]
	public int CurrentPlayers { get; set; } = 0;
	public bool IsScheduledRestartEnabled { get; set; } = false;
	public string RestartTime { get; set; } = "04:00";
	public bool[] RestartDays { get; set; } = new bool[7] { true, true, true, true, true, true, true };
	public string LastMaintenanceDate { get; set; } = "";
	[JsonIgnore]
	public int MaxPlayersFromQuery { get; set; } = 0;
	[JsonIgnore]
	public DateTime? LastProbeTime { get; set; }
	[JsonIgnore]
	public string IconUrl { get; init; } = string.Empty;
	[JsonIgnore]
	public bool IsQueryable { get; init; } = true;
	[JsonIgnore]
	public ServerProbeProtocol ProbeProtocol { get; init; } = ServerProbeProtocol.Auto;
	[JsonIgnore]
	public bool SupportsManualConnectionTesting { get; init; } = true;
	[JsonIgnore]
	public string ProbePath { get; init; } = string.Empty;
	[JsonIgnore]
	public string EosDeploymentId { get; init; } = string.Empty;
}

public class GameServer : GameInfo
{
	public int PasswordStorageVersion { get; set; }
	public string SteamAccountName { get; set; } = string.Empty;
	public bool SteamAuthenticationRequired { get; set; }
	public string InstallPath { get; set; } = string.Empty;
	public string ServerName { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
	public string AdminPassword { get; set; } = string.Empty;
	public string Status { get; set; } = StatusManager.GetStatus(ServerState.Stopped);
	public int MaxPlayers { get; set; } = 10;
	public string WorldName { get; set; } = "NewWorld";
	public bool IsDefaultPath { get; set; } = true;
	public int? PID { get; set; }
	public int? SteamPID { get; set; }
	[JsonIgnore]
	public Process? RunningProcess { get; set; }
	public string GameMode { get; set; } = "PVE";
	[JsonIgnore]
	public double LastCpuMillis { get; set; } = 0;
	[JsonIgnore]
	public DateTime LastSampleTime { get; set; } = DateTime.Now;
	public bool EnableRcon { get; set; } = false;
	public int RconPort { get; set; }
	public string RconPassword { get; set; } = "";
	public bool IsFirstBoot { get; set; } = true;
	[JsonIgnore]
	public string PlayerCount => $"{CurrentPlayers} / {MaxPlayers}";
	public bool UpdateOnStart { get; set; } = false;
	public bool BackupOnStart { get; set; } = false;
	public int ManagedConfigurationVersion { get; set; }
	public bool IsDiscordAlertEnabled { get; set; } = false;
	public string DiscordWebhook { get; set; } = string.Empty;
	public DateTime? StartTime { get; set; }
	public double RamUsage { get; set; }
	[JsonIgnore]
	public bool IsProbing { get; set; } = false;
	public string GameVersion { get; set; } = "Latest";
	public string MinecraftLoader { get; set; } = "Vanilla";
	public string MinecraftLoaderVersion { get; set; } = "Official";
	public int RequiredJavaVersion { get; set; } = 0;
	public int MaxRam { get; set; } = 4;

	[JsonIgnore]
	public string Uptime
	{
		get
		{
			if (Status != StatusManager.GetStatus(ServerState.Running) || !StartTime.HasValue)
				return "--:--:--";

			TimeSpan duration = DateTime.Now - StartTime.Value;

			if (duration.TotalDays >= 1)
				return $"{(int)duration.TotalDays}d {duration.Hours:D2}h {duration.Minutes:D2}m";

			return $"{duration.Hours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
		}
	}
}
