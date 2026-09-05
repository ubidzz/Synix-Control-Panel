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
using System.Text;

namespace Synix_Control_Panel.SynixEngine
{
	internal static class SynixSessionRecovery
	{
		private const string SessionFileName = "active-session.marker";
		private const string FirstRunFileName = "first-run-guide.complete";

		internal static bool PreviousSessionWasInterrupted { get; private set; }

		internal static void BeginSession() => BeginSession(GetSessionPath());

		internal static void EndSession() => EndSession(GetSessionPath());

		internal static bool ShouldShowFirstRunGuide() =>
			!File.Exists(Path.Combine(GetLocalStateDirectory(), FirstRunFileName));

		internal static void CompleteFirstRunGuide()
		{
			string directory = GetLocalStateDirectory();
			Directory.CreateDirectory(directory);
			File.WriteAllText(
				Path.Combine(directory, FirstRunFileName),
				DateTimeOffset.UtcNow.ToString("O"),
				new UTF8Encoding(false));
		}

		internal static void BeginSession(string markerPath)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(markerPath);
			PreviousSessionWasInterrupted = File.Exists(markerPath);
			string? directory = Path.GetDirectoryName(Path.GetFullPath(markerPath));
			if (!string.IsNullOrWhiteSpace(directory))
				Directory.CreateDirectory(directory);

			string temporary = markerPath + ".new";
			File.WriteAllText(
				temporary,
				$"ProcessId={Environment.ProcessId}{Environment.NewLine}StartedUtc={DateTimeOffset.UtcNow:O}",
				new UTF8Encoding(false));
			File.Move(temporary, markerPath, overwrite: true);
		}

		internal static void EndSession(string markerPath)
		{
			try
			{
				if (File.Exists(markerPath))
					File.Delete(markerPath);
			}
			catch (Exception suppressedException)
			{
				Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException);
			}
		}

		private static string GetSessionPath() =>
			Path.Combine(GetLocalStateDirectory(), SessionFileName);

		private static string GetLocalStateDirectory() =>
			Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Synix");
	}
}
