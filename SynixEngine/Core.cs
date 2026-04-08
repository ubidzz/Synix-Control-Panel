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
			UpdateGridStatus();
		}
	}
}