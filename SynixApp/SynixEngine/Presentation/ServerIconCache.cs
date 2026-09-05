// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
namespace Synix_Control_Panel.SynixEngine
{
	public static class ServerIconCache
	{
		public static Dictionary<string, Image> Icons { get; } =
			new(StringComparer.OrdinalIgnoreCase);
	}
}
