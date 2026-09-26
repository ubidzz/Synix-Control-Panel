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
using System.Net.NetworkInformation;
using Synix_Control_Panel.SynixEngine;

namespace Synix_Control_Panel.Tests;

internal static class ServerSetupTestPorts
{
	internal static (int Game, int Query) FindAvailablePair()
	{
		IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
		HashSet<int> occupied = properties.GetActiveTcpListeners()
			.Concat(properties.GetActiveUdpListeners())
			.Select(endpoint => endpoint.Port)
			.Concat(properties.GetActiveTcpConnections().Select(connection => connection.LocalEndPoint.Port))
			.ToHashSet();

		// These are unsaved setup entries, not listeners. Stay below Windows' usual
		// dynamic-port range and check the current host instead of assuming a fixed
		// port is free on every developer PC or GitHub runner.
		for (int gamePort = 20000; gamePort < 49000; gamePort += 2)
		{
			int queryPort = gamePort + 1;
			if (!occupied.Contains(gamePort) && !occupied.Contains(queryPort) &&
				Core.Instance.GetConfiguredPortCollisionOwner(gamePort) == null &&
				Core.Instance.GetConfiguredPortCollisionOwner(queryPort) == null)
			{
				return (gamePort, queryPort);
			}
		}

		throw new InvalidOperationException("No unused game/query port pair is available for the setup UI test.");
	}
}
