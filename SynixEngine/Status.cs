// Copyright (c) 2026 ubidzz. All Rights Reserved.
//
// This file is part of Synix Control Panel.
//
// This code is provided for transparent viewing and personal use only.
// Unauthorized distribution, public modification, or commercial
// use of this source code or the compiled executable is strictly
// prohibited. Please refer to the LICENSE file in the root
// directory for full terms.
using Synix_Control_Panel.Database;
using Synix_Control_Panel.ServerHandler;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		private void UpdateGridStatus()
		{
			if (MainGUI.Instance != null && !MainGUI.Instance.IsDisposed && MainGUI.Instance.IsHandleCreated)
			{
				MainGUI.Instance.BeginInvoke((MethodInvoker)delegate {
					MainGUI.Instance.UpdateGrid();
				});
			}
		}

		public void RebindProcesses()
		{
			foreach (var server in MainGUI.serverList)
			{
				// --- 1. GAME SERVER REBIND ---
				if (server.PID.HasValue && server.PID.Value > 0)
				{
					try
					{
						var process = Process.GetProcessById(server.PID.Value);
						if (process != null && !process.HasExited)
						{
							server.RunningProcess = process;
							server.Status = StatusManager.GetStatus(ServerState.Running);
							if (server.StartTime == null)
							{
								server.StartTime = process.StartTime;
							}
							MainGUI.Instance?.AppendLog($"--- [REBIND] Found {server.Game} still running (PID: {server.PID}) ---", Color.BlueViolet, true);

							process.EnableRaisingEvents = true;
							process.Exited += async (s, e) => {
								if (server.Status == StatusManager.GetStatus(ServerState.Running))
									await RecoverServer(server);
								else
									CleanupStoppedState(server);
							};
						}
					}
					catch { CleanupStoppedState(server); }
				}

				// --- 2. STEAMCMD REBIND (Orphan Recovery) ---
				if ((server.Status == StatusManager.GetStatus(ServerState.Installing) || server.Status == StatusManager.GetStatus(ServerState.Updating)) && server.SteamPID.HasValue)
				{
					try
					{
						var installer = Process.GetProcessById(server.SteamPID.Value);
						if (installer != null && !installer.HasExited)
						{
							MainGUI.Instance?.AppendLog($"--- [REBIND] Found {server.Game} install still active (PID: {server.SteamPID}) ---", Color.BlueViolet, true);
						}
					}
					catch
					{
						// If process is GONE, it finished while Synix was closed
						server.Status = StatusManager.GetStatus(ServerState.Stopped);
						server.SteamPID = null;

						// 🛠️ RUN SURGERY: Fix missing DLLs/Configs for the orphaned install
						GameFix.PostInstall(server);

						MainGUI.Instance?.AppendLog($"--- [RECOVERY] {server.Game} install finished while Synix was closed. Applied fixes. ---", Color.Green, true);
						FileHandler.SaveServers();
					}
				}
			}
			UpdateGridStatus();
		}

		private void CleanupStoppedState(GameServer server)
		{
			server.Status = StatusManager.GetStatus(ServerState.Stopped); ;
			server.PID = null;
			server.RunningProcess = null;
			UpdateGridStatus();
		}

		// Did this for now but will put in a multi-language dictionary later to allow users to add their own languages
		public enum ServerState
		{
			Stopped = 0,
			Running = 1,
			Starting = 2,
			Crashed = 3,
			Stopping = 4,
			Installing = 5,
			Updating = 6,
			BackingUp = 7
		}

		public static class StatusManager
		{
			// This is your "one source of truth"
			public static string GetStatus(ServerState state)
			{
				return state switch
				{
					ServerState.Stopped => "Stopped",
					ServerState.Running => "Running",
					ServerState.Starting => "Starting",
					ServerState.Crashed => "Crashed",
					ServerState.Stopping => "Stopping",
					ServerState.Installing => "Installing",
					ServerState.Updating => "Updating",
					ServerState.BackingUp => "BackingUp",
					_ => "Unknown"
				};
			}

			public static string GetStatus(int code) => GetStatus((ServerState)code);
		}

		public async Task<string> GetLocalIP()
		{
			try
			{
				// Looks at the network card to find the internal (LAN) address
				using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
				{
					socket.Connect("8.8.8.8", 65530);
					IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
					return endPoint?.Address.ToString() ?? "127.0.0.1";
				}
			}
			catch
			{
				return "127.0.0.1";
			}
		}

		public async Task<string> GetPublicIP()
		{
			try
			{
				using var client = new System.Net.Http.HttpClient();
				client.Timeout = TimeSpan.FromSeconds(5);
				return await client.GetStringAsync("https://api.ipify.org");
			}
			catch
			{
				return StatusManager.GetStatus(ServerState.Stopped);
			}
		}

		public async Task UpdatePlayerCount(GameServer server)
		{
			if (server.Status != StatusManager.GetStatus(ServerState.Running)) return;

			// 🎯 DYNAMIC IP GATHERING
			string localIp = await GetLocalIP();

			// Create a list of unique, non-empty IPs to check
			var probeTargets = new List<string> { "127.0.0.1", localIp };

			using var udpClient = new System.Net.Sockets.UdpClient();
			try
			{
				// Windows ICMP Fix to prevent crashes on closed ports
				if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
				{
					const int SIO_UDP_CONNRESET = -1744830452;
					udpClient.Client.IOControl(SIO_UDP_CONNRESET, new byte[] { 0 }, null);
				}

				udpClient.Client.ReceiveTimeout = 1000;

				// 🎯 THE PROBE LOOP
				foreach (string ip in probeTargets.Where(s => !string.IsNullOrEmpty(s)).Distinct())
				{
					try
					{
						System.Net.IPEndPoint remoteEP = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ip), server.QueryPort);

						await udpClient.SendAsync(_a2sInfoRequest, _a2sInfoRequest.Length, remoteEP);

						var receiveTask = udpClient.ReceiveAsync();
						if (await Task.WhenAny(receiveTask, Task.Delay(500)) != receiveTask) continue;

						var result = await receiveTask;
						byte[] data = result.Buffer;

						// 🎯 CHALLENGE HANDSHAKE (Required for Windrose/UE5)
						if (data.Length >= 9 && data[4] == 0x41)
						{
							byte[] challengeRequest = new byte[_a2sInfoRequest.Length + 4];
							Array.Copy(_a2sInfoRequest, 0, challengeRequest, 0, _a2sInfoRequest.Length);
							Array.Copy(data, 5, challengeRequest, _a2sInfoRequest.Length, 4);

							await udpClient.SendAsync(challengeRequest, challengeRequest.Length, remoteEP);
							result = await udpClient.ReceiveAsync();
							data = result.Buffer;
						}

						// 🎯 PARSE PLAYER DATA
						if (data.Length > 5 && data[4] == 0x49)
						{
							int pointer = 6;
							for (int i = 0; i < 4; i++) // Skip Name, Map, Folder, Game
							{
								while (pointer < data.Length && data[pointer] != 0x00) pointer++;
								pointer++;
							}
							pointer += 2; // Skip ID section

							if (pointer + 1 < data.Length)
							{
								server.CurrentPlayers = data[pointer];
								server.MaxPlayersFromQuery = data[pointer + 1];
								return; // Found it! Exit the method.
							}
						}
					}
					catch { /* Try the next IP in the list */ }
				}
			}
			catch { server.CurrentPlayers = 0; }
		}
	}
}