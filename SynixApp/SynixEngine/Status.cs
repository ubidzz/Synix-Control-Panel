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
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using Synix_Control_Panel.SynixApp.ServerHandler;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		public void UpdateGridStatus()
		{
			if (MainGUI.Instance != null && !MainGUI.Instance.IsDisposed && MainGUI.Instance.IsHandleCreated)
			{
				MainGUI.Instance.BeginInvoke((MethodInvoker)delegate
				{
					MainGUI.Instance.UpdateGrid();
				});
			}
		}

		public async void RebindProcesses()
		{
			foreach (var server in MainGUI.serverList)
			{
				// --- 1. GAME SERVER REBIND ---
				if (server.PID.HasValue && server.PID.Value > 0)
				{
					bool isServerRunning = false;

					try
					{
						var process = Process.GetProcessById(server.PID.Value);

						if (!process.HasExited)
						{
							var gameData = GameDatabase.GetGame(server.Game);

							if (gameData != null && !string.IsNullOrEmpty(gameData.ExeName))
							{
								string expectedProcessName = System.IO.Path.GetFileNameWithoutExtension(gameData.ExeName);
								if (process.ProcessName.Equals(expectedProcessName, StringComparison.OrdinalIgnoreCase) || (gameData.ExeName.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) && process.ProcessName.Equals("cmd", StringComparison.OrdinalIgnoreCase)))
								{
									isServerRunning = true;
									server.RunningProcess = process;
									server.Status = StatusManager.GetStatus(ServerState.Running);

									if (server.StartTime == null)
									{
										server.StartTime = process.StartTime;
									}

									Log($"[🔗 REBIND] Found {server.Game} still running (PID: {server.PID})", Color.BlueViolet, true);

									process.EnableRaisingEvents = true;
									process.Exited += async (s, e) =>
									{
										if (server.Status == StatusManager.GetStatus(ServerState.Running))
											await ExecuteStartSequence(server);
										else
											CleanupStoppedState(server);
									};
								}
							}
						}
					}
					catch { }

					if (!isServerRunning)
					{
						CleanupStoppedState(server);
					}
				}

				// --- 2. STEAMCMD REBIND (Orphan Recovery) ---
				if ((server.Status == StatusManager.GetStatus(ServerState.Installing) || server.Status == StatusManager.GetStatus(ServerState.Updating)) && server.SteamPID.HasValue)
				{
					bool isSteamCmdActive = false;
					try
					{
						var installer = Process.GetProcessById(server.SteamPID.Value);
						if (!installer.HasExited && installer.ProcessName.Contains("steamcmd", StringComparison.OrdinalIgnoreCase))
						{
							isSteamCmdActive = true;
							Log($"[🔗 REBIND] Found {server.Game} install still active (PID: {server.SteamPID})", Color.BlueViolet, true);
						}
					}
					catch { }

					if (!isSteamCmdActive)
					{
						server.Status = StatusManager.GetStatus(ServerState.Stopped);
						server.SteamPID = null;
						await GameFix.PostInstall(server);
						Log($"[🔧 RECOVERY] {server.Game} install finished while Synix was closed. Applied fixes.", Color.Green, true);
						FileHandler.SaveServers();
						Core.Instance.UpdateGridStatus();
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

		public enum ServerState
		{
			Stopped = 0,
			Running = 1,
			Starting = 2,
			Crashed = 3,
			Stopping = 4,
			Installing = 5,
			Updating = 6,
			BackingUp = 7,
			Validating = 8,
			Export = 9
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
					ServerState.Validating => "Validating",
					ServerState.Export => "Exporting",
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
				return string.Empty;
			}
		}

		public async Task UpdatePlayerCount(GameServer server)
		{
			if (server.Status != StatusManager.GetStatus(ServerState.Running)) return;

			string localIp = await Core.Instance.GetLocalIP();
			var targets = new List<string> { "127.0.0.1", localIp }.Where(x => !string.IsNullOrEmpty(x)).Distinct();

			if (server.Game.Equals("Minecraft", StringComparison.OrdinalIgnoreCase))
			{
				foreach (var ip in targets)
				{
					bool success = await UpdateMinecraftPlayerCount(server, ip);
					if (success) return; // If successful, stop trying other IPs
				}
				server.CurrentPlayers = 0; // Reset if all IPs fail
				return;
			}

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
								return;
							}
						}
					}
					catch { continue; }
				}
			}
			catch { server.CurrentPlayers = 0; }
		}
	}
}