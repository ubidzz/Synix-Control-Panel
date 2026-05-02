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
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		private static Core? _instance;
		public static Core Instance => _instance ??= new Core();

		private static readonly HttpClient _discordClient = new HttpClient();

		public double TotalCpuUsage { get; set; }
		public double TotalRamUsageGb { get; set; }
		public bool isDownloadActive = false;
		public static double TotalRamGb { get; set; }
		private System.Windows.Forms.Timer _heartbeatTimer;

		private Core()
		{
			_instance = this;
			_heartbeatTimer = new System.Windows.Forms.Timer { Interval = 1000 };
			_heartbeatTimer.Tick += Heartbeat_Tick;
			_heartbeatTimer.Start();
		}

		public void Log(string message, Color? color = null, bool bold = false)
		{
			MainGUI.Instance?.Invoke((Action)(() =>
			{
				MainGUI.Instance.AppendLog(message, color ?? Color.White, bold);
			}));
		}

		public async Task SendDiscordAlert(GameServer server, string title, string message, Color color)
		{
			if (!server.IsDiscordAlertEnabled || string.IsNullOrWhiteSpace(server.DiscordWebhook))
				return;

			int discordColor = (color.R << 16) | (color.G << 8) | color.B;

			var payload = new
			{
				embeds = new[]
				{
					new
					{
						title = $"🛰️ {server.ServerName} | {title}",
						description = message,
						color = discordColor,
						footer = new { text = "Synix Engine • Professional Automation" },
						timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
					}
				}
			};

			try
			{
				string json = JsonSerializer.Serialize(payload);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await _discordClient.PostAsync(server.DiscordWebhook, content);

				if (!response.IsSuccessStatusCode)
				{
					Log($"[👾 DISCORD] Webhook failed: {response.StatusCode}", Color.Red);
				}
			}
			catch (Exception ex)
			{
				Log($"[👾 DISCORD ERROR] {ex.Message}", Color.Red);
			}
		}

		private Dictionary<string, bool> _activePlayerQueries = new Dictionary<string, bool>();
		private Dictionary<string, DateTime> _lastRamWarning = new Dictionary<string, DateTime>();

		private void Heartbeat_Tick(object? sender, EventArgs e)
		{
			_heartbeatTimer.Stop();

			try
			{
				PerformWatchdogCheck();
				UpdateResourceStats();
				PerformMaintenanceCheck();
				CheckForDDoS();

				foreach (GameServer server in MainGUI.serverList)
				{
					if (server.Status == StatusManager.GetStatus(ServerState.Running))
					{
						string srvId = server.ServerName ?? "unknown_server";

						if (!_activePlayerQueries.TryGetValue(srvId, out bool isQuerying) || !isQuerying)
						{
							_activePlayerQueries[srvId] = true;

							Task.Run(async () =>
							{
								try { await UpdatePlayerCount(server); }
								finally { _activePlayerQueries[srvId] = false; }
							});
						}

						if (server.RamUsage >= 80.0)
						{
							bool canAlert = !_lastRamWarning.TryGetValue(srvId, out DateTime lastAlert) ||
											(DateTime.Now - lastAlert).TotalMinutes >= 15;

							if (canAlert)
							{
								_lastRamWarning[srvId] = DateTime.Now;
								_ = SendDiscordAlert(server, "RESOURCE WARNING",
									$"High RAM usage detected: {server.RamUsage:F1}%. Performance may be impacted.",
									Color.Gold);
							}
						}
					}
				}

				UpdateGridStatus();
			}
			finally
			{
				_heartbeatTimer.Start();
			}
		}

		private async void PerformMaintenanceCheck()
		{
			DateTime now = DateTime.Now;
			string currentTime = now.ToString("HH:mm");
			string todayBookmark = now.ToString("yyyy-MM-dd");
			int dayIndex = (int)now.DayOfWeek;

			foreach (GameServer server in MainGUI.serverList)
			{
				if (server.IsScheduledRestartEnabled &&
					server.RestartDays[dayIndex] &&
					server.RestartTime == currentTime &&
					server.LastMaintenanceDate != todayBookmark)
				{
					server.LastMaintenanceDate = todayBookmark;

					_ = SendDiscordAlert(server, "SCHEDULED RESTART",
						"Weekly maintenance is starting now. The server will be back online shortly.", Color.Cyan);

					Log($"[SYNIX] Scheduled weekly maintenance triggered for {server.ServerName}.");
					await ExecuteStartSequence(server);
				}
			}
		}

		public static bool IsSystemSafeToStart()
		{
			// 🎯 1. CPU GUARD (85% Global Limit)
			// We check the entire system load so Synix doesn't crash a busy host.
			double globalCpu = MonitoringHandler.ResourceMonitor.GetGlobalCpuUsage();

			if (globalCpu >= 85.0)
			{
				MessageBox.Show(
					$"[🛡️ RESOURCE GUARD] Global CPU Load is at {globalCpu:F1}%.\n\nStarting another server now would push the host into instability. Please wait for load to drop.",
					"CPU Overload Protection",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);

				return false;
			}

			// 🎯 2. RAM GUARD (85% Usable Pool Limit)
			// Get the REAL hardware total (e.g., 32GB)
			double physicalRamGb = MonitoringHandler.ResourceMonitor.GetTotalSystemRamGB();

			// Apply your new 5GB Windows overhead
			double usablePool = physicalRamGb - 5.0;
			if (usablePool < 1) usablePool = physicalRamGb;

			// Get the current usage from ALL running servers
			var usage = MonitoringHandler.ResourceMonitor.GetTotalResources(MainGUI.serverList);
			double usedGb = usage.TotalRamMB / 1024.0;

			// THE MATH: Percentage of the usable pool used by servers
			double ramUsagePercent = (usedGb / usablePool) * 100.0;

			// Setting this to 85.0 RAM limit
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
