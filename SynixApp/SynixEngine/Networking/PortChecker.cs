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
using System.Buffers.Binary;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		private static readonly HttpClient _probeHttpClient = new(new SocketsHttpHandler
		{
			AllowAutoRedirect = false
		})
		{
			Timeout = Timeout.InfiniteTimeSpan
		};

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

				if (!IPAddress.TryParse(ip, out IPAddress? address))
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
				LogLocalized("Probe.Activity.NetworkError", Color.Red, false, ip, port, ex.Message);
				return false;
			}
		}

		public async Task<bool> ExecuteDynamicProbes(GameServer server, string ip)
		{
			GameInfo? gameData = GameDatabase.GetGame(server.Game);
			ServerProbeProtocol probeProtocol = GameDatabase.GetProbeProtocol(gameData);
			bool supportsA2S = probeProtocol == ServerProbeProtocol.A2S;
			bool isLocalAddress = await IsLocalAddressAsync(ip);

			if (isLocalAddress &&
				!string.IsNullOrWhiteSpace(gameData?.LaunchBehavior.ReadyLogText) &&
				GameLogDiscovery.ContainsCurrentStartupText(
					server,
					gameData.LaunchBehavior.ReadyLogText))
			{
				LogLocalized("Probe.Activity.LogReadiness", arguments: [server.Game]);
				return RecordSuccessfulProbe(server);
			}

			if (supportsA2S && await TestServerConnectivity(ip, server.QueryPort))
			{
				LogLocalized("Probe.Activity.A2S", arguments: [server.Game, server.QueryPort]);
				return RecordSuccessfulProbe(server);
			}

			if (!supportsA2S)
			{
				switch (probeProtocol)
				{
					case ServerProbeProtocol.RestApi:
						if (await TestRestApiConnectivity(server, ip, gameData?.ProbePath))
							return RecordSuccessfulProbe(server);
						break;

					case ServerProbeProtocol.EpicOnlineServices:
						if (await TestEOSWebAPI(server, ip))
							return RecordSuccessfulProbe(server);
						break;
				}
			}

			if (probeProtocol != ServerProbeProtocol.EpicOnlineServices &&
				await TestTcpConnectivity(ip, server.Port))
			{
				LogLocalized("Probe.Activity.Tcp", arguments: [server.Game, server.Port]);
				return RecordSuccessfulProbe(server);
			}

			if (probeProtocol != ServerProbeProtocol.RestApi &&
				probeProtocol != ServerProbeProtocol.EpicOnlineServices &&
				await TestTcpConnectivity(ip, server.QueryPort))
			{
				LogLocalized("Probe.Activity.Tcp", arguments: [server.Game, server.QueryPort]);
				return RecordSuccessfulProbe(server);
			}

			if (supportsA2S && await TestServerConnectivity(ip, server.Port))
			{
				LogLocalized("Probe.Activity.Udp", arguments: [server.Game, server.Port]);
				return RecordSuccessfulProbe(server);
			}

			if (server.StartTime.HasValue &&
				(DateTime.Now - server.StartTime.Value).TotalSeconds >= 120 &&
				isLocalAddress)
			{
				if (IsPortInUseLocally(server.Port))
				{
					LogLocalized("Probe.Activity.GamePortBinding", arguments: [server.Game, server.Port]);
					return RecordSuccessfulProbe(server);
				}

				if (IsPortInUseLocally(server.QueryPort))
				{
					LogLocalized("Probe.Activity.QueryPortBinding", arguments: [server.Game, server.QueryPort]);
					return RecordSuccessfulProbe(server);
				}

				if (probeProtocol == ServerProbeProtocol.EpicOnlineServices &&
					Servers.ReconcileActiveServerProcesses(server))
				{
					LogLocalized("Probe.Activity.EosProcess", arguments: [server.Game]);
					return RecordSuccessfulProbe(server);
				}
			}

			return false;
		}

		private static bool RecordSuccessfulProbe(GameServer server)
		{
			RecordGameVerification(server.Game, GameVerificationKind.Monitoring);
			if (server.Status == StatusManager.GetStatus(ServerState.Starting))
				RecordGameVerification(server.Game, GameVerificationKind.Start);

			return true;
		}

		public async Task<bool> TestRestApiConnectivity(
			GameServer server,
			string ip,
			string? probePath,
			int timeoutMs = 2500)
		{
			if (server.QueryPort is < 1 or > 65535)
				return false;

			string normalizedPath = string.IsNullOrWhiteSpace(probePath)
				? "/"
				: probePath.StartsWith('/') ? probePath : $"/{probePath}";

			try
			{
				Uri probeUri = new UriBuilder(Uri.UriSchemeHttp, ip, server.QueryPort, normalizedPath).Uri;
				using var request = new HttpRequestMessage(HttpMethod.Get, probeUri);
				request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));
				using HttpResponseMessage response = await _probeHttpClient.SendAsync(
					request,
					HttpCompletionOption.ResponseHeadersRead,
					timeout.Token);

				LogLocalized("Probe.Activity.RestEndpoint", arguments: [server.Game, server.QueryPort, (int)response.StatusCode]);
				return true;
			}
			catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or UriFormatException)
			{
				System.Diagnostics.Debug.WriteLine($"[REST PROBE] {server.Game}: {ex.Message}");
			}

			if (await TestTcpConnectivity(ip, server.QueryPort, timeoutMs))
			{
				LogLocalized("Probe.Activity.RestTcp", arguments: [server.Game, server.QueryPort]);
				return true;
			}

			return false;
		}

		public async Task<bool> TestTcpConnectivity(string ip, int port, int timeoutMs = 2000)
		{
			try
			{
				if (!IPAddress.TryParse(ip, out IPAddress? address))
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
		public async Task<bool> TestEOSWebAPI(GameServer server, string ip, int timeoutMs = 3500)
		{
			GameInfo? gameData = GameDatabase.GetGame(server.Game);
			string appId = gameData?.AppID ?? string.Empty;
			string deploymentId = gameData?.EosDeploymentId ?? string.Empty;

			if (string.IsNullOrWhiteSpace(deploymentId))
				deploymentId = GetProbeEnvironmentValue("SYNIX_EOS_DEPLOYMENT_ID", appId);

			string accessToken = GetProbeEnvironmentValue("SYNIX_EOS_ACCESS_TOKEN", appId);

			if (!string.IsNullOrWhiteSpace(deploymentId) && !string.IsNullOrWhiteSpace(accessToken))
			{
				try
				{
					string encodedDeploymentId = Uri.EscapeDataString(deploymentId);
					var endpoint = new Uri($"https://api.epicgames.dev/matchmaking/v1/{encodedDeploymentId}/filter");
					string requestJson = JsonSerializer.Serialize(new
					{
						criteria = Array.Empty<object>(),
						maxResults = 200
					});

					using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
					request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
					request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
					request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

					using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));
					using HttpResponseMessage response = await _probeHttpClient.SendAsync(request, timeout.Token);

					if (response.IsSuccessStatusCode)
					{
						string jsonResponse = await response.Content.ReadAsStringAsync(timeout.Token);
						string targetIpPort = $"{ip}:{server.Port}";

						if (jsonResponse.Contains(targetIpPort, StringComparison.OrdinalIgnoreCase) ||
							(!string.IsNullOrWhiteSpace(server.ServerName) &&
							 jsonResponse.Contains(server.ServerName, StringComparison.OrdinalIgnoreCase)))
						{
							LogLocalized("Probe.Activity.EosSession", arguments: [server.Game]);
							return true;
						}
					}
				}
				catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
				{
					System.Diagnostics.Debug.WriteLine($"[EOS API ERROR] {server.Game}: {ex.Message}");
				}
			}

			if (await IsLocalAddressAsync(ip) &&
				(IsPortInUseLocally(server.Port) || IsPortInUseLocally(server.QueryPort)))
			{
				LogLocalized("Probe.Activity.EosSocket", arguments: [server.Game]);
				return true;
			}

			return false;
		}

		private static string GetProbeEnvironmentValue(string baseName, string appId)
		{
			if (!string.IsNullOrWhiteSpace(appId))
			{
				string? gameValue = Environment.GetEnvironmentVariable($"{baseName}_{appId}");
				if (!string.IsNullOrWhiteSpace(gameValue))
					return gameValue;
			}

			return Environment.GetEnvironmentVariable(baseName) ?? string.Empty;
		}

		private static async Task<bool> IsLocalAddressAsync(string host)
		{
			try
			{
				IPAddress[] targetAddresses = IPAddress.TryParse(host, out IPAddress? parsed)
					? [parsed]
					: await Dns.GetHostAddressesAsync(host);

				if (targetAddresses.Any(IPAddress.IsLoopback))
					return true;

				IPAddress[] localAddresses = await Dns.GetHostAddressesAsync(Dns.GetHostName());
				return targetAddresses.Any(target => localAddresses.Contains(target));
			}
			catch (Exception exception)
			{
				ApplicationLogService.WriteSuppressedException(exception);
				return false;
			}
		}

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
			catch (Exception exception)
			{
				ApplicationLogService.WriteSuppressedException(exception);
				return false;
			}
		}

		private async Task<bool> UpdateMinecraftPlayerCount(GameServer server, string ip)
		{
			if (MinecraftControlProfile.IsBedrock(server))
				return await UpdateMinecraftBedrockPlayerCount(server, ip);

			try
			{
				using var tcpClient = new System.Net.Sockets.TcpClient();

				using (var connectTimeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(1500)))
				{
					await tcpClient.ConnectAsync(ip, server.Port, connectTimeout.Token);
				}

				using var stream = tcpClient.GetStream();

				List<byte> handshake = new List<byte> { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x0F };

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
			catch (Exception suppressedException)
			{
				Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException);
			}

			return false;
		}

		private static async Task<bool> UpdateMinecraftBedrockPlayerCount(
			GameServer server,
			string ip)
		{
			ReadOnlySpan<byte> offlineMessageDataId =
				[0x00, 0xFF, 0xFF, 0x00, 0xFE, 0xFE, 0xFE, 0xFE,
				 0xFD, 0xFD, 0xFD, 0xFD, 0x12, 0x34, 0x56, 0x78];
			byte[] request = new byte[33];
			request[0] = 0x01;
			BinaryPrimitives.WriteInt64BigEndian(request.AsSpan(1, 8), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
			offlineMessageDataId.CopyTo(request.AsSpan(9, 16));
			BinaryPrimitives.WriteInt64BigEndian(request.AsSpan(25, 8), Environment.TickCount64);

			try
			{
				using UdpClient client = new(AddressFamily.InterNetwork);
				client.Connect(ip, server.Port);
				using CancellationTokenSource timeout = new(TimeSpan.FromMilliseconds(1500));
				await client.SendAsync(request, timeout.Token);
				byte[] response = (await client.ReceiveAsync(timeout.Token)).Buffer;
				const int textLengthOffset = 33;
				const int textOffset = 35;
				if (response.Length < textOffset || response[0] != 0x1C)
					return false;

				int textLength = BinaryPrimitives.ReadUInt16BigEndian(
					response.AsSpan(textLengthOffset, 2));
				if (textLength <= 0 || response.Length < textOffset + textLength)
					return false;

				string[] fields = Encoding.UTF8
					.GetString(response, textOffset, textLength)
					.Split(';');
				if (fields.Length < 6 ||
					!int.TryParse(fields[4], out int online) ||
					!int.TryParse(fields[5], out int maximum))
				{
					return false;
				}

				server.CurrentPlayers = online;
				server.MaxPlayersFromQuery = maximum;
				return true;
			}
			catch (Exception exception)
			{
				ApplicationLogService.WriteSuppressedException(exception);
				return false;
			}
		}
	}
}
