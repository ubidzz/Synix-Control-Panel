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
// 🎯 THE FIX: You must include this so the Core knows what a "GameServer" is
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

			InitializeAndRebind();
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

				// 🎯 FIX: We MUST await the actual post, otherwise it dies when the method scope ends
				var response = await _discordClient.PostAsync(server.DiscordWebhook, content);

				if (!response.IsSuccessStatusCode)
				{
					System.Diagnostics.Debug.WriteLine($"[👾 DISCORD] Webhook failed: {response.StatusCode}");
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[👾 DISCORD ERROR] {ex.Message}");
			}
		}

		private async void Heartbeat_Tick(object? sender, EventArgs e)
		{
			PerformWatchdogCheck();
			UpdateResourceStats();
			PerformMaintenanceCheck();
			CheckForDDoS();

			foreach (GameServer server in MainGUI.serverList)
			{
				if (server.Status == StatusManager.GetStatus(ServerState.Running))
				{
					// 1. Keep your existing player count update
					_ = UpdatePlayerCount(server);
					// 🎯 2. NEW: RAM Threshold Alert
					// This uses the 80.0 limit you set to keep things stable
					// We assume 'RamUsage' is a property in your GameServer class
					if (server.RamUsage >= 80.0)
					{
						_ = SendDiscordAlert(server, "RESOURCE WARNING",
							$"High RAM usage detected: {server.RamUsage:F1}%. Performance may be impacted.",
							Color.Gold);
					}
				}
			}

			UpdateGridStatus();
		}

		private void PerformMaintenanceCheck()
		{
			DateTime now = DateTime.Now;
			string currentTime = now.ToString("HH:mm");
			string todayBookmark = now.ToString("yyyy-MM-dd");
			int dayIndex = (int)now.DayOfWeek;

			// 🎯 THE FIX: Explicitly type the server variable
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

					Log($"[ENGINE] Scheduled weekly maintenance triggered for {server.ServerName}.");
					ExecuteMaintenanceRestart(server);
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
				System.Windows.Forms.MessageBox.Show(
					$"[🛡️ RESOURCE GUARD] Global CPU Load is at {globalCpu:F1}%.\n\nStarting another server now would push the host into instability. Please wait for load to drop.",
					"CPU Overload Protection",
					System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Warning);

				return false; // BLOCK THE START
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
			// $$Percentage = \frac{Used}{UsablePool} \times 100$$
			double ramUsagePercent = (usedGb / usablePool) * 100.0;

			// Setting this to 85.0 RAM limit
			if (ramUsagePercent >= 85.0)
			{
				System.Windows.Forms.MessageBox.Show(
					$"[🛡️ RESOURCE GUARD] System RAM usage is at {ramUsagePercent:F1}% of the {usablePool:F1}GB usable pool.\n\nPlease stop a server before starting another.",
					"System Resource Exhaustion",
					System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Warning);

				return false; // BLOCK THE START
			}

			return true; // BOTH ARE SAFE - PROCEED WITH START
		}

		public bool IsBasicInfoValid(string name, string game) => !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(game);
		public bool IsServerSetupValid(string name, string game) => !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(game);
	}
}