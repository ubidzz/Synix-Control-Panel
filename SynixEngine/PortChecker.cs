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
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

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

		public async Task<bool> TestServerConnectivity(string ip, int port, int timeoutMs = 2500)
		{
			using var udpClient = new UdpClient();
			try
			{
				// This is necessary because UE5 often sends an ICMP unreachable 
				// packet that crashes the standard .NET UdpClient.
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

				await udpClient.SendAsync(_a2sInfoRequest, _a2sInfoRequest.Length, remoteEP);

				var receiveTask = udpClient.ReceiveAsync();
				var timeoutTask = Task.Delay(timeoutMs);

				if (await Task.WhenAny(receiveTask, timeoutTask) == receiveTask)
				{
					var result = await receiveTask;

					// Ensure the buffer actually contains data from the server.
					// result.Buffer.Length > 0 confirms the server responded to your probe.
					return result.Buffer != null && result.Buffer.Length > 0;
				}

				return false;
			}
			catch (Exception ex)
			{
				// This stays exactly as you had it for your logging
				Log($"[NETWORK ERROR] Probe failed for {ip}:{port} - {ex.Message}", Color.Red);
				return false;
			}
		}

		public bool IsPortInUseLocally(int port)
		{
			try
			{
				var ipProps = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();

				// Now that we added 'using System.Linq', these will work correctly
				bool udpInUse = ipProps.GetActiveUdpListeners().Any(l => l.Port == port);
				bool tcpInUse = ipProps.GetActiveTcpListeners().Any(l => l.Port == port);

				return udpInUse || tcpInUse;
			}
			catch { return false; }
		}
	}
}