// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.FileFolderHandler;

namespace Synix_Control_Panel.SynixEngine
{
	public static class ApplicationLogService
	{
		public static void Write(string message, Color? color = null, bool bold = false)
		{
			if (ApplicationUiService.PublishLog(
				message,
				color ?? Color.White,
				bold))
			{
				return;
			}

			FileHandler.QueueLog(
				"Synix_Background_Service",
				$"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
		}
	}
}
