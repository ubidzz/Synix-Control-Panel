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
using System.Linq;
using System.Windows.Forms;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Drawing;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		private static Core? _instance;
		public static Core Instance => _instance ??= new Core();

		// 🎯 DISCORD WEBHOOK CLIENT: Shared and persistent to prevent memory leaks
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

		// 🎯 THE DISCORD ENGINE: Centralized alerting logic
		public async Task SendDiscordAlert(GameServer server, string title, string message, Color color)
		{
			// 🎯 THE SAFETY GUARD: Only proceed if alerts are toggled ON and the URL is valid
			if (!server.IsDiscordAlertEnabled || string.IsNullOrWhiteSpace(server.DiscordWebhook))
				return;

			// Convert color for Discord embeds (Decimal format)
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

				// Fire-and-forget keeps the Engine Heartbeat from lagging on slow networks
				_ = _discordClient.PostAsync(server.DiscordWebhook, content);
			}
			catch (Exception ex)
			{
				// Silently log to debug so it doesn't crash the engine if the internet is down
				System.Diagnostics.Debug.WriteLine($"[DISCORD ERROR] {ex.Message}");
			}
		}

		private async void Heartbeat_Tick(object? sender, EventArgs e)
		{
			PerformWatchdogCheck();
			UpdateResourceStats();
			PerformMaintenanceCheck();

			foreach (var server in MainGUI.serverList)
			{
				if (server.Status == "Running")
				{
					_ = UpdatePlayerCount(server);
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

			foreach (var server in MainGUI.serverList)
			{
				if (server.IsScheduledRestartEnabled &&
					server.RestartDays[dayIndex] &&
					server.RestartTime == currentTime &&
					server.LastMaintenanceDate != todayBookmark)
				{
					server.LastMaintenanceDate = todayBookmark;

					// 🎯 ALERT DISCORD: Scheduled Reboot
					_ = SendDiscordAlert(server, "SCHEDULED RESTART",
						"Weekly maintenance is starting now. The server will be back online shortly.", Color.Cyan);

					Log($"[ENGINE] Scheduled weekly maintenance triggered for {server.ServerName}.");
					ExecuteMaintenanceRestart(server);
				}
			}
		}

		public bool IsBasicInfoValid(string name, string game) => !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(game);
		public bool IsServerSetupValid(string name, string game) => !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(game);
	}
}