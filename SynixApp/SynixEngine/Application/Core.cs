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
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using System.Collections.Concurrent;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		private static Core? _instance;
		public static Core Instance => _instance ??= new Core();
		internal static bool IsBackgroundServiceMode { get; set; }

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
			ApplicationLogService.Write(message, color, bold);
		}

		public void LogLocalized(
			string resourceKey,
			Color? color = null,
			bool bold = false,
			params object?[] arguments)
		{
			ApplicationLogService.WriteLocalized(
				resourceKey,
				color,
				bold,
				arguments);
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

				foreach (GameServer server in ServerRegistry.Snapshot())
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
									LocalizationManager.Get("Core.Notification.ResourceWarning.Title"),
									LocalizationManager.Get("Core.Notification.ResourceWarning.Body", server.RamUsage),
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
				string todayBookmark = now.ToString("yyyy-MM-dd");

				foreach (GameServer server in ServerRegistry.Snapshot())
				{
					if (server.Status != StatusManager.GetStatus(ServerState.Running))
						continue;
					if (server.MaintenanceRetryAfterUtc > DateTime.UtcNow)
						continue;

					SmartMaintenancePlan plan = SmartMaintenancePlanner.Evaluate(server, now);
					if (plan.Decision == SmartMaintenanceDecision.DeferForPlayers)
					{
						bool shouldNotify = !server.LastMaintenanceDeferralNoticeUtc.HasValue ||
							(DateTime.UtcNow - server.LastMaintenanceDeferralNoticeUtc.Value).TotalMinutes >= 5;
						if (shouldNotify)
						{
							server.LastMaintenanceDeferralNoticeUtc = DateTime.UtcNow;
							int remainingMinutes = Math.Max(
								0,
								server.MaintenanceMaximumDelayMinutes - (int)plan.Delay.TotalMinutes);
							LogLocalized(
								"Core.Activity.MaintenanceDeferred",
								Color.Cyan,
								false,
								server.ServerName,
								plan.Reason,
								remainingMinutes);
						}
						continue;
					}

					if (plan.Decision == SmartMaintenanceDecision.RunNow)
					{
						server.LastMaintenanceDeferralNoticeUtc = null;

						_ = SendDiscordNotification(
							server,
							DiscordNotificationEvent.ServerRestarting,
							LocalizationManager.Get("Core.Notification.ScheduledRestart.Title"),
							server.CurrentPlayers > 0
								? LocalizationManager.Get("Core.Notification.ScheduledRestart.PlayerWaitBody")
								: LocalizationManager.Get("Core.Notification.ScheduledRestart.Body"),
							Color.Cyan);

						LogLocalized("Core.Activity.MaintenanceTriggered", arguments: [server.ServerName, plan.Reason]);

						bool completed = await ExecuteStartSequence(server, "MAINTENANCE");
						if (completed)
						{
							server.LastMaintenanceDate = todayBookmark;
							server.MaintenanceRetryAfterUtc = null;
							FileHandler.SaveServers();
						}
						else if (server.SmartMaintenanceEnabled)
						{
							server.MaintenanceRetryAfterUtc = DateTime.UtcNow.AddMinutes(5);
							LogLocalized("Core.Activity.MaintenanceRetry", Color.Orange, true, server.ServerName);
						}
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
				if (!IsBackgroundServiceMode)
					LocalizedMessageBox.Show(
					LocalizationManager.Get(
						"ResourceGuard.Cpu.Body",
						globalCpu),
					LocalizationManager.Get(
						"ResourceGuard.Cpu.Title"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);

				return false;
			}

			double physicalRamGb = ResourceMonitor.GetTotalSystemRamGB();

			double usablePool = physicalRamGb - 5.0;
			if (usablePool < 1) usablePool = physicalRamGb;

			var usage = ResourceMonitor.GetTotalResources(ServerRegistry.Servers);
			double usedGb = usage.TotalRamMB / 1024.0;

			double ramUsagePercent = (usedGb / usablePool) * 100.0;

			if (ramUsagePercent >= 85.0)
			{
				if (!IsBackgroundServiceMode)
					LocalizedMessageBox.Show(
					LocalizationManager.Get(
						"ResourceGuard.Memory.Body",
						ramUsagePercent,
						usablePool),
					LocalizationManager.Get(
						"ResourceGuard.Exhaustion.Title"),
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
