// Copyright (c) 2026 ubidzz. All Rights Reserved.
//
// This file is part of Synix Control Panel.
//
// This code is provided for transparent viewing and personal use only.
// Unauthorized distribution, public modification, or commercial
// use of this source code or the compiled executable is strictly
// prohibited. Please refer to the LICENSE file in the root
// directory for full terms.
using System;
using System.Linq;
using System.Windows.Forms;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		private static Core? _instance;
		public static Core Instance => _instance ??= new Core();

		public double TotalCpuUsage { get; set; }
		public double TotalRamUsageGb { get; set; }
		public bool isDownloadActive = false;

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
			// The engine automatically handles the thread safety
			MainGUI.Instance?.Invoke((Action)(() =>
			{
				MainGUI.Instance.AppendLog(message, color ?? Color.White, bold);
			}));
		}

		private void Heartbeat_Tick(object? sender, EventArgs e)
		{
			PerformWatchdogCheck();
			UpdateResourceStats();
			PerformMaintenanceCheck();
			UpdateGridStatus();
		}

		private void PerformMaintenanceCheck()
		{
			string currentTime = DateTime.Now.ToString("HH:mm");
			string todayDate = DateTime.Now.ToShortDateString();

			// Get current day as index (0 = Sunday, 6 = Saturday)
			int dayIndex = (int)DateTime.Now.DayOfWeek;

			foreach (var server in MainGUI.serverList)
			{
				// 🛡️ The AI's 4-Point Safety Check
				if (server.IsScheduledRestartEnabled &&
					server.RestartDays[dayIndex] &&
					server.RestartTime == currentTime &&
					server.LastMaintenanceDate != todayDate &&
					server.Status == StatusManager.GetStatus(ServerState.Running))
				{
					server.LastMaintenanceDate = todayDate;
					ExecuteMaintenanceRestart(server); //
				}
			}
		}
		public bool IsBasicInfoValid(string name, string game)
		{
			// The AI's simple rule: You need a name and a game selected
			return !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(game);
		}

		public bool IsServerSetupValid(string name, string game)
		{
			// The Engine decides the rules here. 
			// If you later decide you also need a Port to be valid, you only change it here.
			return !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(game);
		}
	}
}