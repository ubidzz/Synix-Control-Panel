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
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		private readonly byte[] _a2sInfoRequest = new byte[]
		{
			0xFF, 0xFF, 0xFF, 0xFF, 0x54, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65,
			0x20, 0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65, 0x20, 0x51, 0x75, 0x65,
			0x72, 0x79, 0x00
		};

		public async Task<bool> TestAllProtocolsConnectivity(string ip, int gamePort, int queryPort)
		{
			var t1 = TestTcpConnectivity(ip, gamePort);
			var t2 = TestTcpConnectivity(ip, queryPort);
			var t3 = TestServerConnectivity(ip, gamePort);
			var t4 = TestServerConnectivity(ip, queryPort);

			await Task.WhenAll(t1, t2, t3, t4);

			return t1.Result || t2.Result || t3.Result || t4.Result;
		}

		public async Task<bool> TestServerConnectivity(string ip, int port, int timeoutMs = 2500)
		{
			using var udpClient = new UdpClient();
			try
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					const int SIO_UDP_CONNRESET = -1744830452;
					udpClient.Client.IOControl(SIO_UDP_CONNRESET, new byte[] { 0 }, null);
				}

				if (!IPAddress.TryParse(ip, out IPAddress address))
				{
					var hostAddresses = await Dns.GetHostAddressesAsync(ip);
					if (hostAddresses.Length == 0) return false;
					address = hostAddresses[0];
				}

				IPEndPoint remoteEP = new IPEndPoint(address, port);
				udpClient.Client.SendTimeout = timeoutMs;
				udpClient.Client.ReceiveTimeout = timeoutMs;

				byte[] requestPayload = _a2sInfoRequest;

				await udpClient.SendAsync(requestPayload, requestPayload.Length, remoteEP);

				var receiveTask = udpClient.ReceiveAsync();
				var timeoutTask = Task.Delay(timeoutMs);

				if (await Task.WhenAny(receiveTask, timeoutTask) == receiveTask)
				{
					var result = await receiveTask;
					byte[] buffer = result.Buffer;

					if (buffer == null || buffer.Length < 5) return false;

					if (buffer[4] == 0x41)
					{
						byte[] challenge = new byte[4];
						Array.Copy(buffer, 5, challenge, 0, 4);

						byte[] challengePayload = new byte[requestPayload.Length + 4];
						Buffer.BlockCopy(requestPayload, 0, challengePayload, 0, requestPayload.Length);
						Buffer.BlockCopy(challenge, 0, challengePayload, requestPayload.Length, 4);

						await udpClient.SendAsync(challengePayload, challengePayload.Length, remoteEP);

						var finalReceiveTask = udpClient.ReceiveAsync();
						if (await Task.WhenAny(finalReceiveTask, Task.Delay(timeoutMs)) == finalReceiveTask)
						{
							var finalResult = await finalReceiveTask;
							return finalResult.Buffer != null && finalResult.Buffer.Length > 4 && finalResult.Buffer[4] == 0x49;
						}
						return false;
					}

					return buffer[4] == 0x49;
				}

				return false;
			}
			catch (Exception ex)
			{
				Log($"[🛰️ NETWORK ERROR] Probe failed for {ip}:{port} - {ex.Message}", Color.Red);
				return false;
			}
		}

		public async Task<bool> ExecuteDynamicProbes(GameServer server, string ip)
		{
			if (await TestServerConnectivity(ip, server.QueryPort))
			{
				Log($"[PROBE SUCCESS] {server.Game} verified via -> A2S (Steam UDP) on Port {server.QueryPort}");
				return true;
			}

			if (await TestTcpConnectivity(ip, server.Port))
			{
				Log($"[PROBE SUCCESS] {server.Game} verified via -> TCP Handshake on Port {server.Port}");
				return true;
			}

			if (await TestTcpConnectivity(ip, server.QueryPort))
			{
				Log($"[PROBE SUCCESS] {server.Game} verified via -> TCP Handshake on Port {server.QueryPort}");
				return true;
			}

			if (await TestServerConnectivity(ip, server.Port))
			{
				Log($"[PROBE SUCCESS] {server.Game} verified via -> UDP Check on Port {server.Port}");
				return true;
			}

			if (server.StartTime.HasValue && (DateTime.Now - server.StartTime.Value).TotalSeconds >= 180)
			{
				if (IsPortInUseLocally(server.Port))
				{
					Log($"[PROBE SUCCESS] {server.Game} verified via -> OS Binding (Game Port {server.Port} In Use)");
					return true;
				}

				if (IsPortInUseLocally(server.QueryPort))
				{
					Log($"[PROBE SUCCESS] {server.Game} verified via -> OS Binding (Query Port {server.QueryPort} In Use)");
					return true;
				}
			}

			return false;
		}

		public async Task<bool> TestTcpConnectivity(string ip, int port, int timeoutMs = 2000)
		{
			try
			{
				if (!IPAddress.TryParse(ip, out IPAddress address))
				{
					var hostAddresses = await Dns.GetHostAddressesAsync(ip);
					if (hostAddresses.Length == 0) return false;
					address = hostAddresses[0];
				}

				using var tcpClient = new TcpClient();
				var connectTask = tcpClient.ConnectAsync(address, port);
				var timeoutTask = Task.Delay(timeoutMs);

				if (await Task.WhenAny(connectTask, timeoutTask) == connectTask)
				{
					return tcpClient.Connected;
				}

				return false;
			}
			catch
			{
				return false;
			}
		}
		/*
		// This not used and only added for maybe used later on
		public async Task<bool> TestEOSWebAPI(GameServer server)
		{
			// EOS requires specific Deployment IDs per game, which you would store in your GameDatabase
			string eosDeploymentId = "GAME_SPECIFIC_DEPLOYMENT_ID";

			// In a real scenario, you must retrieve an OAuth token first using your Epic Client ID & Secret
			string eosOAuthToken = "YOUR_BEARER_TOKEN";

			if (string.IsNullOrEmpty(eosOAuthToken) || eosOAuthToken == "YOUR_BEARER_TOKEN")
			{
				// Skip silently if no token is configured
				return false;
			}

			try
			{
				using var client = new HttpClient();
				client.Timeout = TimeSpan.FromSeconds(5);

				// Epic's public sessions endpoint for matchmaking
				string url = $"https://api.epicgames.dev/matchmaking/v1/public/sessions?deploymentId={eosDeploymentId}";

				// Attach the required Bearer token to prove we are authorized to ask Epic for data
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", eosOAuthToken);
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				HttpResponseMessage response = await client.GetAsync(url);

				if (response.IsSuccessStatusCode)
				{
					string jsonResponse = await response.Content.ReadAsStringAsync();

					// The JSON response contains a massive list of all active servers for that game globally.
					// We search the raw JSON text to see if our specific server's IP and Port are actively listed.

					// Note: A more robust method would be using System.Text.Json to deserialize the payload,
					// but a quick string check is highly efficient for the watchdog loop.
					string targetIpPort = $"{await GetPublicIP()}:{server.Port}";

					if (jsonResponse.Contains(targetIpPort) || jsonResponse.Contains(server.ServerName))
					{
						return true; // Epic confirms our server is alive and listed!
					}
				}

				return false;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[EOS API Error] {server.Game}: {ex.Message}");
				return false;
			}
		}
		*/

		public bool IsPortInUseLocally(int port)
		{
			try
			{
				var properties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();

				if (properties.GetActiveUdpListeners().Any(ep => ep.Port == port))
				{
					return true;
				}

				if (properties.GetActiveTcpListeners().Any(ep => ep.Port == port))
				{
					return true;
				}

				return false;
			}
			catch
			{
				return false;
			}
		}

		// ========================================================================
		// ⛏️ MINECRAFT NATIVE SERVER LIST PING (TCP PROTOCOL)
		// ========================================================================
		private async Task<bool> UpdateMinecraftPlayerCount(GameServer server, string ip)
		{
			try
			{
				using var tcpClient = new System.Net.Sockets.TcpClient();

				using (var connectTimeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(1500)))
				{
					await tcpClient.ConnectAsync(ip, server.Port, connectTimeout.Token);
				}

				using var stream = tcpClient.GetStream();

				List<byte> handshake = new List<byte> { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x0F }; // PacketID (0) + Protocol (-1)

				byte[] hostBytes = Encoding.UTF8.GetBytes(ip);
				handshake.Add((byte)hostBytes.Length);
				handshake.AddRange(hostBytes);

				handshake.Add((byte)((server.Port >> 8) & 0xFF));
				handshake.Add((byte)(server.Port & 0xFF));
				handshake.Add(0x01);

				List<byte> payload = new List<byte>();
				payload.Add((byte)handshake.Count);
				payload.AddRange(handshake);

				payload.Add(0x01);
				payload.Add(0x00);

				using (var writeTimeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(1500)))
				{
					await stream.WriteAsync(payload.ToArray().AsMemory(), writeTimeout.Token);
				}

				byte[] buffer = new byte[4096];
				int bytesRead;

				using (var readTimeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(1500)))
				{
					bytesRead = await stream.ReadAsync(buffer.AsMemory(), readTimeout.Token);
				}

				if (bytesRead > 0)
				{
					string rawStr = Encoding.UTF8.GetString(buffer);

					var onlineMatch = System.Text.RegularExpressions.Regex.Match(rawStr, @"""online""\s*:\s*(\d+)");
					var maxMatch = System.Text.RegularExpressions.Regex.Match(rawStr, @"""max""\s*:\s*(\d+)");

					if (onlineMatch.Success && int.TryParse(onlineMatch.Groups[1].Value, out int online))
					{
						server.CurrentPlayers = online;

						if (maxMatch.Success && int.TryParse(maxMatch.Groups[1].Value, out int max))
						{
							server.MaxPlayersFromQuery = max;
						}

						return true;
					}
				}
			}
			catch { }

			return false;
		}
	}
}
