// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using System.Runtime.CompilerServices;

namespace Synix_Control_Panel.SynixEngine
{
	public static class ApplicationLogService
	{
		private const string ApplicationLogName = "Synix_Log";

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

		/// <summary>
		/// Records an intentionally suppressed exception in the text log without
		/// publishing it to Activity &amp; Diagnostics on the dashboard.
		/// </summary>
		public static void WriteSuppressedException(
			Exception exception,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "")
		{
			ArgumentNullException.ThrowIfNull(exception);
			string sourceName = Path.GetFileName(sourceFilePath);
			string location = string.IsNullOrWhiteSpace(sourceName)
				? memberName
				: $"{sourceName}::{memberName}";
			string entry =
				$"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [DEBUG] Suppressed exception in {location}:{Environment.NewLine}{exception}";

			try
			{
				if (!FileHandler.QueueLog(ApplicationLogName, entry))
					_ = FileHandler.WriteLogImmediate(ApplicationLogName, entry);
			}
			catch (Exception loggingException)
			{
				System.Diagnostics.Debug.WriteLine(
					$"[SUPPRESSED EXCEPTION LOG FAILURE] {loggingException}{Environment.NewLine}{entry}");
			}
		}
	}
}
