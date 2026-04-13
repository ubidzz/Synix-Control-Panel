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
using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		// Standard Steam A2S_INFO query header for probing game servers
		private readonly byte[] _a2sInfoRequest = new byte[]
		{
			0xFF, 0xFF, 0xFF, 0xFF, 0x54, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65,
			0x20, 0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65, 0x20, 0x51, 0x75, 0x65,
			0x72, 0x79, 0x00
		};

		/// <summary>
		/// Probes a UDP port to verify if the game server is reachable and responding.
		/// </summary>
		/// <param name="ip">The IP address to test (Use 127.0.0.1 for local, Public IP for external).</param>
		/// <param name="port">The Query Port assigned to the server.</param>
		/// <param name="timeoutMs">Wait time before declaring the port closed.</param>
		/// <returns>True if the server replies with valid game data.</returns>
		public async Task<bool> TestServerConnectivity(string ip, int port, int timeoutMs = 2500)
		{
			using var udpClient = new UdpClient();
			try
			{
				// Ensure the IP is valid
				if (!IPAddress.TryParse(ip, out IPAddress address))
				{
					Log($"[NETWORK] Invalid IP Address: {ip}", Color.Red);
					return false;
				}

				IPEndPoint remoteEP = new IPEndPoint(address, port);

				// Send the A2S_INFO challenge packet
				await udpClient.SendAsync(_a2sInfoRequest, _a2sInfoRequest.Length, remoteEP);

				// Create a timeout task to prevent the engine from hanging on silent ports
				var receiveTask = udpClient.ReceiveAsync();
				var timeoutTask = Task.Delay(timeoutMs);

				// Return true only if we receive a response before the timeout
				if (await Task.WhenAny(receiveTask, timeoutTask) == receiveTask)
				{
					var result = await receiveTask;
					return result.Buffer.Length > 0;
				}

				return false;
			}
			catch (Exception ex)
			{
				Log($"[NETWORK ERROR] Probe failed for {ip}:{port} - {ex.Message}", Color.Red);
				return false;
			}
		}

		public bool IsPortInUseLocally(int port)
		{
			var ipProps = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();

			// 🎯 Checks UDP and TCP listeners
			bool udpInUse = ipProps.GetActiveUdpListeners().Any(l => l.Port == port);
			bool tcpInUse = ipProps.GetActiveTcpListeners().Any(l => l.Port == port);

			return udpInUse || tcpInUse;
		}
	}
}