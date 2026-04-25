// Copyright (c) 2026 ubidzz. All Rights Reserved.
//
// This file is part of Synix Control Panel.
//
// This code is provided for transparent viewing and personal use only.
// Unauthorized distribution, public modification, or commercial
// use of this source code or the compiled executable is strictly
// prohibited. Please refer to the LICENSE file in the root
// directory for full terms.
using Synix_Control_Panel.ServerHandler;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		private void UpdateGridStatus()
		{
			if (MainGUI.Instance != null && !MainGUI.Instance.IsDisposed && MainGUI.Instance.IsHandleCreated)
			{
				MainGUI.Instance.BeginInvoke((MethodInvoker)delegate
				{
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
							MainGUI.Instance?.AppendLog($"--- [🔗 REBIND] Found {server.Game} still running (PID: {server.PID}) ---", Color.BlueViolet, true);

							process.EnableRaisingEvents = true;
							process.Exited += async (s, e) =>
							{
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
							MainGUI.Instance?.AppendLog($"--- [🔗 REBIND] Found {server.Game} install still active (PID: {server.SteamPID}) ---", Color.BlueViolet, true);
						}
					}
					catch
					{
						// If process is GONE, it finished while Synix was closed
						server.Status = StatusManager.GetStatus(ServerState.Stopped);
						server.SteamPID = null;

						// 🛠️ RUN SURGERY: Fix missing DLLs/Configs for the orphaned install
						GameFix.PostInstall(server);

						MainGUI.Instance?.AppendLog($"--- [🔧 RECOVERY] {server.Game} install finished while Synix was closed. Applied fixes. ---", Color.Green, true);
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
					ServerState.BackingUp => "Backing Up",
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

			// 🎯 Use your dynamic LAN IP and Loopback
			string localIp = await Core.Instance.GetLocalIP();
			var targets = new List<string> { "127.0.0.1", localIp }.Where(x => !string.IsNullOrEmpty(x)).Distinct();

			using var udpClient = new System.Net.Sockets.UdpClient();
			try
			{
				// Windows ICMP Fix (Essential for UE5 servers)
				if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
				{
					const int SIO_UDP_CONNRESET = -1744830452;
					udpClient.Client.IOControl(SIO_UDP_CONNRESET, new byte[] { 0 }, null);
				}

				udpClient.Client.ReceiveTimeout = 1500;

				foreach (var ip in targets)
				{
					try
					{
						System.Net.IPEndPoint remoteEP = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ip), server.QueryPort);

						// 1. Send the standard request
						await udpClient.SendAsync(_a2sInfoRequest, _a2sInfoRequest.Length, remoteEP);
						var result = await udpClient.ReceiveAsync();
						byte[] data = result.Buffer;

						// 🎯 THE WINDROSE FIX: Handle the 0x41 Challenge
						// Rust usually skips this, but UE5 demands it.
						if (data.Length >= 9 && data[4] == 0x41)
						{
							// Copy original request + 4 bytes of challenge data from the server
							byte[] challengeRequest = new byte[_a2sInfoRequest.Length + 4];
							Array.Copy(_a2sInfoRequest, 0, challengeRequest, 0, _a2sInfoRequest.Length);
							Array.Copy(data, 5, challengeRequest, _a2sInfoRequest.Length, 4);

							// Re-send with the "Proof" the server wants
							await udpClient.SendAsync(challengeRequest, challengeRequest.Length, remoteEP);
							result = await udpClient.ReceiveAsync();
							data = result.Buffer;
						}

						// 2. Parse the actual data (Header 0x49)
						if (data.Length > 5 && data[4] == 0x49)
						{
							int pointer = 6; // Skip Header, Type, Protocol

							// Skip the 4 strings: Name, Map, Folder, Game
							for (int i = 0; i < 4; i++)
							{
								while (pointer < data.Length && data[pointer] != 0x00) pointer++;
								pointer++;
							}

							pointer += 2; // Skip ID section

							if (pointer + 1 < data.Length)
							{
								server.CurrentPlayers = data[pointer];
								server.MaxPlayersFromQuery = data[pointer + 1];
								return; // 🎯 SUCCESS: Found the server and parsed data
							}
						}
					}
					catch { continue; } // Try the next IP if this one times out
				}
			}
			catch { server.CurrentPlayers = 0; }
		}
	}
}