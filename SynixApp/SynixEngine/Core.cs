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
using Synix_Control_Panel.SynixApp.MonitoringHandler;
using System.Collections.Concurrent;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		private static Core? _instance;
		public static Core Instance => _instance ??= new Core();

		private static readonly HttpClient _discordClient = new()
		{
			Timeout = TimeSpan.FromSeconds(15)
		};

		public double TotalCpuUsage { get; set; }
		public double TotalRamUsageGb { get; set; }
		public bool isDownloadActive = false;
		public static double TotalRamGb { get; set; }
		private System.Threading.Timer _heartbeatTimer;
		private readonly ConcurrentDictionary<string, byte> _activePlayerQueries = new(StringComparer.OrdinalIgnoreCase);
		private readonly ConcurrentDictionary<string, DateTime> _lastRamWarning = new(StringComparer.OrdinalIgnoreCase);
		private readonly SemaphoreSlim _maintenanceLock = new(1, 1);
		public static readonly string RootPath = @"C:\Synix";
		public static string DataPath => Path.Combine(RootPath, "SynixData");
		public static string LogsPath => Path.Combine(DataPath, "logs");
		public static string GameIconsPath => Path.Combine(DataPath, "GameIcons");
		public static string RuntimesPath => Path.Combine(DataPath, "Runtimes");
		public static string SteamCmdPath => Path.Combine(RootPath, "SteamCMD");
		public static string SteamCmdExe => Path.Combine(SteamCmdPath, "steamcmd.exe");
		public static string DefaultBackupPath => Path.Combine(RootPath, "BackupGames");
		public static string GamesPath => Path.Combine(RootPath, "Games");

		private Core()
		{
			_instance = this;
			_heartbeatTimer = new System.Threading.Timer(Heartbeat_Tick, null, 1000, Timeout.Infinite);
		}

		public void Log(string message, Color? color = null, bool bold = false)
		{
			MainGUI.Instance?.Invoke((Action)(() =>
			{
				MainGUI.Instance.AppendLog(message, color ?? Color.White, bold);
			}));
		}

		public Task SendDiscordAlert(GameServer server, string title, string message, Color color) =>
			SendDiscordNotification(
				server,
				DiscordNotificationEvent.MonitoringWarning,
				title,
				message,
				color);

		private void Heartbeat_Tick(object? state)
		{
			try
			{
				PerformWatchdogCheck();
				UpdateResourceStats();
				_ = PerformMaintenanceCheckAsync();

				if (Properties.Settings.Default.CheckDDoS)
				{
					CheckForDDoS();
				}

				foreach (GameServer server in MainGUI.serverList.ToList())
				{
					if (server.Status == StatusManager.GetStatus(ServerState.Running))
					{
						string srvId = server.ServerName ?? "unknown_server";

						if (_activePlayerQueries.TryAdd(srvId, 0))
						{
							_ = Task.Run(async () =>
							{
								try
								{
									await UpdatePlayerCount(server);
								}
								catch (Exception ex)
								{
									System.Diagnostics.Debug.WriteLine(
										$"[PLAYER QUERY ERROR] {server.ServerName}: {ex}");
								}
								finally
								{
									_activePlayerQueries.TryRemove(srvId, out _);
								}
							});
						}

						if (server.RamUsage >= 80.0)
						{
							bool canAlert = !_lastRamWarning.TryGetValue(srvId, out DateTime lastAlert) ||
											(DateTime.Now - lastAlert).TotalMinutes >= 15;

							if (canAlert)
							{
								_lastRamWarning[srvId] = DateTime.Now;
								_ = SendDiscordNotification(
									server,
									DiscordNotificationEvent.ResourceWarning,
									"RESOURCE WARNING",
									$"High RAM usage detected: {server.RamUsage:F1}%. Performance may be impacted.",
									Color.Gold);
							}
						}
					}
				}

				UpdateGridStatus();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[HEARTBEAT ERROR] {ex}");
			}
			finally
			{
				_heartbeatTimer?.Change(1000, Timeout.Infinite);
			}
		}

		private async Task PerformMaintenanceCheckAsync()
		{
			if (!await _maintenanceLock.WaitAsync(0))
				return;

			try
			{
				DateTime now = DateTime.Now;
				string currentTime = now.ToString("HH:mm");
				string todayBookmark = now.ToString("yyyy-MM-dd");
				int dayIndex = (int)now.DayOfWeek;

				foreach (GameServer server in MainGUI.serverList.ToList())
				{
					bool[]? restartDays = server.RestartDays;
					bool hasValidRestartDay =
						restartDays != null &&
						restartDays.Length > dayIndex &&
						restartDays[dayIndex];

					if (server.IsScheduledRestartEnabled &&
						hasValidRestartDay &&
						server.RestartTime == currentTime &&
						server.LastMaintenanceDate != todayBookmark)
					{
						server.LastMaintenanceDate = todayBookmark;

						_ = SendDiscordNotification(
							server,
							DiscordNotificationEvent.ServerRestarting,
							"SCHEDULED RESTART",
							"Weekly maintenance is starting now. The server will be back online shortly.",
							Color.Cyan);

						Log($"[SYNIX] Scheduled weekly maintenance triggered for {server.ServerName}.");

						await ExecuteStartSequence(server, "MAINTENANCE");
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[MAINTENANCE ERROR] {ex}");
			}
			finally
			{
				_maintenanceLock.Release();
			}
		}

		public static bool IsSystemSafeToStart()
		{

			double globalCpu = ResourceMonitor.GetGlobalCpuUsage();

			if (globalCpu >= 85.0)
			{
				MessageBox.Show(
					$"[🛡️ RESOURCE GUARD] Global CPU Load is at {globalCpu:F1}%.\n\nStarting another server now would push the host into instability. Please wait for load to drop.",
					"CPU Overload Protection",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);

				return false;
			}

			double physicalRamGb = ResourceMonitor.GetTotalSystemRamGB();

			double usablePool = physicalRamGb - 5.0;
			if (usablePool < 1) usablePool = physicalRamGb;

			var usage = ResourceMonitor.GetTotalResources(MainGUI.serverList);
			double usedGb = usage.TotalRamMB / 1024.0;

			double ramUsagePercent = (usedGb / usablePool) * 100.0;

			if (ramUsagePercent >= 85.0)
			{
				MessageBox.Show(
					$"[🛡️ RESOURCE GUARD] System RAM usage is at {ramUsagePercent:F1}% of the {usablePool:F1}GB usable pool.\n\nPlease stop a server before starting another.",
					"System Resource Exhaustion",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);

				return false;
			}

			return true;
		}

		public bool IsBasicInfoValid(string name, string game) => !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(game);
		public bool IsServerSetupValid(string name, string game) => !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(game);
	}
}
