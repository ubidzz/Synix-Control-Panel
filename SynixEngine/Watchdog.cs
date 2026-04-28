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
using Synix_Control_Panel.Database;
using System.Diagnostics;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		private readonly Dictionary<int, int> _watchdogGracePeriods = [];
		private static readonly PerformanceCounter _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

		private void PerformWatchdogCheck()
		{
			foreach (var server in MainGUI.serverList.ToList())
			{
				var dbEntry = GameDatabase.GetGame(server.Game);
				string exePathFromDB = dbEntry?.ExeName ?? "";

				if (server.Status == StatusManager.GetStatus(ServerState.Starting))
				{
					if (!server.PID.HasValue) continue;

					// 🎯 CHECK 1: Ensure the process is actually still there
					if (IsProcessAlive(server.PID.Value, exePathFromDB))
					{
						// 🎯 CHECK 2: Only probe if not already announced AND not currently probing
						if (!server.HasAnnouncedOnline && !server.IsProbing)
						{
							// 5-second throttle to keep the CPU low
							if (server.LastProbeTime == null || (DateTime.Now - server.LastProbeTime.Value).TotalSeconds >= 5)
							{
								server.LastProbeTime = DateTime.Now;
								server.IsProbing = true; // 🔒 LOCK THE GATE

								_ = Task.Run(async () =>
								{
									try
									{
										string publicIP = await GetPublicIP();
										string localIp = await GetLocalIP();
										bool isResponding = false;

										// Test all three connection types
										if (!string.IsNullOrEmpty(publicIP) && await TestServerConnectivity(publicIP, server.QueryPort))
											isResponding = true;
										else if (!string.IsNullOrEmpty(localIp) && await TestServerConnectivity(localIp, server.QueryPort))
											isResponding = true;
										else if (await TestServerConnectivity("127.0.0.1", server.QueryPort))
											isResponding = true;

										if (isResponding)
										{
											MainGUI.Instance?.Invoke((Action)(() =>
											{
												_ = SendDiscordAlert(server, "SERVER ONLINE",
													$"Successfully tested the server connectivity!",
													Color.LimeGreen);

												server.Status = StatusManager.GetStatus(ServerState.Running);
												MainGUI.Instance.UpdateGrid();
											}));
										}
									}
									finally
									{
										server.IsProbing = false; // 🔓 OPEN THE GATE (even if it fails)
									}
								});
							}
						}
					}
					else
					{
						_ = RecoverServer(server);
					}
					continue;
				}

				// --- MONITOR STABLE SERVERS ---
				if (server.Status == StatusManager.GetStatus(ServerState.Running) && server.PID.HasValue)
				{
					if (!IsProcessAlive(server.PID.Value, exePathFromDB))
					{
						_ = RecoverServer(server);
					}
				}
			}
		}

		private bool IsProcessAlive(int pid, string dbExePath)
		{
			try
			{
				// 1. Hook the process by its ID
				using var p = Process.GetProcessById(pid);

				// 2. Immediate check if it has already exited
				if (p.HasExited) return false;

				// 3. 🛡️ GHOST-PROOF IDENTITY MATCH
				string expectedName = Path.GetFileNameWithoutExtension(dbExePath);

				// 4. Return true only if the name in Windows matches the name in our Database
				return p.ProcessName.Equals(expectedName, StringComparison.OrdinalIgnoreCase);
			}
			catch
			{
				// Catching "Process not found" - PID is genuinely gone
				return false;
			}
		}

		public void InitializeAndRebind()
		{
			// Re-links processes if the app was restarted while servers were running
			RebindProcesses();
		}

		private bool _isAlertActive = false;
		private long _lastTotalBytes = 0;

		private void CheckForDDoS()
		{
			// 20MB per second threshold for detection
			const long ATTACK_THRESHOLD_BYTES = 20971520;

			long currentBps = GetBytesPerSecond();

			if (currentBps > ATTACK_THRESHOLD_BYTES)
			{
				// 🎯 FIX 1: Get total system CPU usage to confirm attack patterns
				float cpuUsage = GetSystemCpuUsage();

				// 🎯 FIX 2: Only trigger if SteamCMD isn't downloading updates
				bool isSteamActive = System.Diagnostics.Process.GetProcessesByName("steamcmd").Length > 0;

				if (!isSteamActive && cpuUsage > 90 && !_isAlertActive)
				{
					_isAlertActive = true;

					// Just call the alert with a global message
					TriggerGlobalDDoSAlert();

					MainGUI.Instance?.AppendLog($"[🚨 SECURITY] NETWORK FLOOD: {currentBps / 1024 / 1024} MB/s | System CPU: {cpuUsage:0}%", Color.Maroon);
				}
			}
			else
			{
				_isAlertActive = false;
			}
		}

		private void TriggerGlobalDDoSAlert()
		{
			// Task.Run prevents the UI from freezing while the box is open
			System.Threading.Tasks.Task.Run(() =>
			{
				MessageBox.Show(
					"🚨 SYNIX NETWORK GUARD 🚨\n\n" +
					"Critical bandwidth saturation detected on the network interface.\n\n" +
					"System resources are redlining. Please check your firewall immediately.",
					"Global DDoS Detection",
					MessageBoxButtons.OK,
					MessageBoxIcon.Stop,
					MessageBoxDefaultButton.Button1,
					MessageBoxOptions.ServiceNotification
				);
			});
		}

		// Helper to get the actual system CPU for the alert
		private float GetSystemCpuUsage()
		{
			try { return _cpuCounter.NextValue(); }
			catch { return 0; }
		}

		private long GetBytesPerSecond()
		{
			try
			{
				var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
				long currentTotalBytes = 0;

				foreach (var ni in interfaces)
				{
					if (ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
						ni.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
					{
						currentTotalBytes += ni.GetIPv4Statistics().BytesReceived;
					}
				}

				if (_lastTotalBytes == 0) { _lastTotalBytes = currentTotalBytes; return 0; }

				long diff = currentTotalBytes - _lastTotalBytes;
				_lastTotalBytes = currentTotalBytes;
				return diff;
			}
			catch { return 0; }
		}
	}
}