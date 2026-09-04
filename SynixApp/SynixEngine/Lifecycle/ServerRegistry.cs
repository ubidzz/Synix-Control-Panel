// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using System.ComponentModel;

namespace Synix_Control_Panel.SynixEngine
{
	public static class ServerRegistry
	{
		public static BindingList<GameServer> Servers { get; } = [];

		public static List<GameServer> Snapshot()
		{
			return Servers.ToList();
		}
	}
}
