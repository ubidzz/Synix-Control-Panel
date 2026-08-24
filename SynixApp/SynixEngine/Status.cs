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

		public async Task RebindProcesses()
		{
			bool stateChanged = false;
			foreach (var server in MainGUI.serverList)
			{
				GameInfo? gameData = GameDatabase.GetGame(server.Game);
				Process? recoveredProcess = null;
				bool discoveredByPath = false;
				if (gameData != null && ProcessRecovery.IsRecordedProcessValid(server, gameData))
				{
					try
					{
						recoveredProcess = Process.GetProcessById(server.PID!.Value);
					}
					catch
					{
					}
				}
				else if (gameData != null)
				{
					recoveredProcess = ProcessRecovery.FindInstalledServerProcess(server, gameData);
					discoveredByPath = recoveredProcess != null;
				}

				if (recoveredProcess != null)
				{
					BindRecoveredProcess(server, recoveredProcess, discoveredByPath);
					stateChanged = true;
				}
				else if (server.PID.HasValue || IsInterruptedRuntimeStatus(server.Status))
				{
					string interruptedStatus = server.Status;
					CleanupStoppedState(server);
					stateChanged = true;
					Log($"[🔧 RECOVERY] Cleared an interrupted {interruptedStatus} state for {server.ServerName}.", Color.Orange, true);
				}

				if (IsSteamOperationStatus(server.Status))
				{
					bool isSteamCmdActive = false;
					if (server.SteamPID.HasValue)
					{
						try
						{
							using var installer = Process.GetProcessById(server.SteamPID.Value);
							isSteamCmdActive = !installer.HasExited && installer.ProcessName.Contains("steamcmd", StringComparison.OrdinalIgnoreCase);
						}
						catch { }
					}

					if (isSteamCmdActive)
					{
						Log($"[🔗 REBIND] Found {server.Game} SteamCMD operation still active (PID: {server.SteamPID})", Color.BlueViolet, true);
					}
					else
					{
						bool postInstallNeeded = server.Status == StatusManager.GetStatus(ServerState.Installing) ||
							server.Status == StatusManager.GetStatus(ServerState.Updating);
						server.Status = StatusManager.GetStatus(ServerState.Stopped);
						server.SteamPID = null;
						if (postInstallNeeded)
						{
							await GameFix.PostInstall(server);
							await RefreshServerIconAsync(server);
							Log($"[🔧 RECOVERY] {server.Game} finished while Synix was closed. Applied its safe post-install actions.", Color.Green, true);
						}
						stateChanged = true;
					}
				}
			}
			if (stateChanged)
				FileHandler.SaveServers();
			UpdateGridStatus();
		}

		private void BindRecoveredProcess(GameServer server, Process process, bool discoveredByPath)
		{
			server.RunningProcess?.Dispose();
			server.RunningProcess = process;
			server.PID = process.Id;
			server.Status = StatusManager.GetStatus(ServerState.Running);
			try
			{
				server.StartTime ??= process.StartTime;
			}
			catch { }
			process.Exited += async (_, _) =>
			{
				try
				{
					if (server.Status == StatusManager.GetStatus(ServerState.Running))
						await ExecuteStartSequence(server, "WATCHDOG");
					else if (!server.Status.StartsWith(StatusManager.GetStatus(ServerState.Stopping), StringComparison.OrdinalIgnoreCase))
						CleanupStoppedState(server);
				}
				catch (Exception exception)
				{
					Log($"[🚨 CRASH HANDLER ERROR] {exception.Message}", Color.Red);
					CleanupStoppedState(server);
				}
			};
			process.EnableRaisingEvents = true;
			Log(
				discoveredByPath
					? $"[🔗 CRASH RECOVERY] Reconnected {server.ServerName} by its exact installed executable path (PID: {process.Id})."
					: $"[🔗 REBIND] Reconnected {server.ServerName} (PID: {process.Id}).",
				Color.BlueViolet,
				true);
		}

		private static bool IsSteamOperationStatus(string? status) =>
			status == StatusManager.GetStatus(ServerState.Installing) ||
			status == StatusManager.GetStatus(ServerState.Updating) ||
			status == StatusManager.GetStatus(ServerState.Validating);

		private static bool IsInterruptedRuntimeStatus(string? status) =>
			status == StatusManager.GetStatus(ServerState.Running) ||
			status == StatusManager.GetStatus(ServerState.Starting) ||
			status == StatusManager.GetStatus(ServerState.Stopping) ||
			status == StatusManager.GetStatus(ServerState.Crashed) ||
			status == StatusManager.GetStatus(ServerState.BackingUp) ||
			status == StatusManager.GetStatus(ServerState.Restoring) ||
			status == StatusManager.GetStatus(ServerState.Export);

		private void CleanupStoppedState(GameServer server)
		{
			server.Status = StatusManager.GetStatus(ServerState.Stopped);
			server.PID = null;
			server.RunningProcess?.Dispose();
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
			Export = 9,
			Restoring = 10
		}

		public static class StatusManager
		{

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
					ServerState.Restoring => "Restoring",
					_ => "Unknown"
				};
			}

			public static string GetStatus(int code) => GetStatus((ServerState)code);
		}

		private static string? _cachedLocalIp = null;
		public async Task<string> GetLocalIP()
		{
			if (_cachedLocalIp != null) return _cachedLocalIp;
			try
			{

				using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
				{
					socket.Connect("8.8.8.8", 65530);
					IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
					_cachedLocalIp = endPoint?.Address.ToString() ?? "127.0.0.1";
					return _cachedLocalIp;
				}
			}
			catch
			{
				return "127.0.0.1";
			}
		}

		private static readonly HttpClient _sharedNetworkClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

		public async Task<string> GetPublicIP()
		{
			try
			{
				return await _sharedNetworkClient.GetStringAsync("https://api.ipify.org");
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
					if (success) return;
				}
				server.CurrentPlayers = 0;
				return;
			}

			GameInfo? gameData = GameDatabase.GetGame(server.Game);
			if (GameDatabase.GetProbeProtocol(gameData) != ServerProbeProtocol.A2S)
			{
				server.CurrentPlayers = 0;
				return;
			}

			using var udpClient = new System.Net.Sockets.UdpClient();
			try
			{

				if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
				{
					const int SIO_UDP_CONNRESET = -1744830452;
					udpClient.Client.IOControl(SIO_UDP_CONNRESET, new byte[] { 0 }, null);
				}

				foreach (var ip in targets)
				{
					try
					{
						System.Net.IPEndPoint remoteEP = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ip), server.QueryPort);

						await udpClient.SendAsync(_a2sInfoRequest, _a2sInfoRequest.Length, remoteEP);

						UdpReceiveResult result;
						using (var receiveTimeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(1500)))
						{
							result = await udpClient.ReceiveAsync(receiveTimeout.Token);
						}

						byte[] data = result.Buffer;

						if (data.Length >= 9 && data[4] == 0x41)
						{
							byte[] challengeRequest = new byte[_a2sInfoRequest.Length + 4];
							Array.Copy(_a2sInfoRequest, 0, challengeRequest, 0, _a2sInfoRequest.Length);
							Array.Copy(data, 5, challengeRequest, _a2sInfoRequest.Length, 4);

							await udpClient.SendAsync(challengeRequest, challengeRequest.Length, remoteEP);

							using (var challengeTimeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(1500)))
							{
								result = await udpClient.ReceiveAsync(challengeTimeout.Token);
							}

							data = result.Buffer;
						}

						if (data.Length > 5 && data[4] == 0x49)
						{
							int pointer = 6;

							for (int i = 0; i < 4; i++)
							{
								while (pointer < data.Length && data[pointer] != 0x00) pointer++;
								pointer++;
							}

							pointer += 2;

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
