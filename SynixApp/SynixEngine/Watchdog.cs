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
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.ServerHandler;
using System.Diagnostics;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		private readonly Dictionary<int, int> _watchdogGracePeriods = [];
		private static PerformanceCounter? _cpuCounter = null;
		private long _lastTotalBytes = 0;
		private static System.Net.NetworkInformation.NetworkInterface[]? _activeInterfaces = null;
		private bool _isAlertActive = false;

		private void PerformWatchdogCheck()
		{
			foreach (var server in ServerRegistry.Snapshot())
			{
				var dbEntry = GameDatabase.GetGame(server.Game);
				bool usesExternalLifecycle = dbEntry?.LaunchBehavior.LifecycleTracking ==
					GameLifecycleTrackingMode.ExternalDeployment;

				if (server.Status == StatusManager.GetStatus(ServerState.Starting))
				{
					if (!server.PID.HasValue &&
						server.ServerProcesses.Count == 0 &&
						!usesExternalLifecycle)
					{
						continue;
					}

					bool isAlive = usesExternalLifecycle ||
						Servers.ReconcileActiveServerProcesses(server);

					if (isAlive)
					{
						if (server.IsProbing && server.LastProbeTime.HasValue &&
							(DateTime.Now - server.LastProbeTime.Value).TotalSeconds >= 45)
						{

							server.IsProbing = false;
							Log($"[PROBE] Reset a stale startup probe for {server.ServerName}.", Color.OrangeRed);
						}

						if (!server.HasAnnouncedOnline && !server.IsProbing)
						{
							if (server.LastProbeTime == null || (DateTime.Now - server.LastProbeTime.Value).TotalSeconds >= 5)
							{
								bool isFirstProbe = server.LastProbeTime == null;
								server.LastProbeTime = DateTime.Now;
								server.IsProbing = true;
								if (isFirstProbe)
								{
									Log($"[PROBE] Startup verification active for {server.ServerName}; waiting for its network listener...", Color.Cyan);
								}

								_ = Task.Run(async () =>
								{
									try
									{
										bool isResponding = false;

										isResponding = await ExecuteDynamicProbes(server, "127.0.0.1");

										if (!isResponding)
										{
											string localIp = await GetLocalIP();
											if (!string.IsNullOrEmpty(localIp))
												isResponding = await ExecuteDynamicProbes(server, localIp);
										}

										if (!isResponding)
										{
											using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
											string publicIP = await GetPublicIP().WaitAsync(cts.Token);
											if (!string.IsNullOrEmpty(publicIP))
												isResponding = await ExecuteDynamicProbes(server, publicIP);
										}

										if (isResponding)
										{
											server.HasAnnouncedOnline = true;
											_ = SendDiscordNotification(
												server,
												DiscordNotificationEvent.ServerOnline,
												"SERVER ONLINE",
												"Synix successfully verified server connectivity.",
												Color.LimeGreen);

											server.Status = StatusManager.GetStatus(ServerState.Running);
											UpdateGridStatus();
										}
									}
									catch (Exception ex)
									{
										Log($"[Watchdog Error] {server.Game}: {ex.Message}");
									}
									finally
									{
										server.IsProbing = false;
									}
								});
							}
						}
					}
					else
					{
						server.Status = StatusManager.GetStatus(ServerState.Stopped);
						server.IsProbing = false;

						UpdateGridStatus();

						Log($"[Watchdog] {server.Game} process terminated during startup. Aborting sequence.");
					}
					continue;
				}

				if (server.Status == StatusManager.GetStatus(ServerState.Running))
				{
					if (!usesExternalLifecycle)
					{
						if (!Servers.ReconcileActiveServerProcesses(server))
						{
							_ = ExecuteStartSequence(server, "WATCHDOG");
						}
					}
				}
			}
		}

		private void CheckForDDoS()
		{
			const long ATTACK_THRESHOLD_BYTES = 20971520;

			long currentBps = GetBytesPerSecond();

			if (currentBps > ATTACK_THRESHOLD_BYTES)
			{
				float cpuUsage = GetSystemCpuUsage();

				Process[] steamProcesses = Process.GetProcessesByName("steamcmd");
				bool isSteamActive;
				try
				{
					isSteamActive = steamProcesses.Length > 0;
				}
				finally
				{
					foreach (Process steamProcess in steamProcesses)
						steamProcess.Dispose();
				}

				if (!isSteamActive && cpuUsage > 90 && !_isAlertActive)
				{
					_isAlertActive = true;

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
			System.Threading.Tasks.Task.Run(() =>
			{
				MessageBox.Show(
					"🚨 SYNIX NETWORK GUARD 🚨\n\n" +
					"Critical bandwidth saturation detected on the network interface.\n\n" +
					"System resources are redlining. Please check your firewall immediately.",
					"Possible Network Flood Detected",
					MessageBoxButtons.OK,
					MessageBoxIcon.Stop,
					MessageBoxDefaultButton.Button1,
					MessageBoxOptions.ServiceNotification
				);
			});
		}

		private static bool _isInitializingCpu = false;

		private float GetSystemCpuUsage()
		{
			try
			{
				if (_cpuCounter == null)
				{
					if (!_isInitializingCpu)
					{
						_isInitializingCpu = true;

						Task.Run(() =>
						{
							_cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
							_cpuCounter.NextValue();
						});
					}
					return 0f;
				}

				return _cpuCounter.NextValue();
			}
			catch { return 0f; }
		}

		private long GetBytesPerSecond()
		{
			try
			{
				if (_activeInterfaces == null)
				{
					_activeInterfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
						.Where(ni => ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
									 ni.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
						.ToArray();
				}

				long currentTotalBytes = 0;

				foreach (var ni in _activeInterfaces)
				{
					currentTotalBytes += ni.GetIPv4Statistics().BytesReceived;
				}

				if (_lastTotalBytes == 0)
				{
					_lastTotalBytes = currentTotalBytes;
					return 0;
				}

				long bytesPerSecond = currentTotalBytes - _lastTotalBytes;
				_lastTotalBytes = currentTotalBytes;

				return bytesPerSecond;
			}
			catch
			{
				return 0;
			}
		}
	}
}
