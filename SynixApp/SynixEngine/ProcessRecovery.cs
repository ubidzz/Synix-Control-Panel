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
using System.Diagnostics;

namespace Synix_Control_Panel.SynixEngine
{
	internal static class ProcessRecovery
	{
		internal static Process? FindInstalledServerProcess(
			GameServer server,
			GameInfo game,
			int? excludedProcessId = null)
		{
			ArgumentNullException.ThrowIfNull(server);
			ArgumentNullException.ThrowIfNull(game);
			string executableName = MinecraftControlProfile.ResolveExecutableName(server, game);
			if (string.IsNullOrWhiteSpace(server.InstallPath) ||
				string.IsNullOrWhiteSpace(executableName) ||
				!executableName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}

			string expectedPath;
			try
			{
				expectedPath = Path.GetFullPath(Path.Combine(
					server.InstallPath,
					executableName));
			}
			catch
			{
				return null;
			}

			string processName = Path.GetFileNameWithoutExtension(executableName);
			foreach (Process process in Process.GetProcessesByName(processName))
			{
				if (process.Id == Environment.ProcessId ||
					process.Id == excludedProcessId)
				{
					process.Dispose();
					continue;
				}

				try
				{
					if (!process.HasExited &&
						string.Equals(
							Path.GetFullPath(process.MainModule?.FileName ?? string.Empty),
							expectedPath,
							StringComparison.OrdinalIgnoreCase))
					{
						return process;
					}
				}
				catch
				{
				}

				process.Dispose();
			}

			return null;
		}

		internal static bool IsRecordedProcessValid(
			GameServer server,
			GameInfo game)
		{
			if (!server.PID.HasValue || server.PID.Value <= 0)
				return false;

			try
			{
				using Process process = Process.GetProcessById(server.PID.Value);
				if (process.HasExited)
					return false;

				string executableName = MinecraftControlProfile.ResolveExecutableName(server, game);
				if (executableName.EndsWith(".bat", StringComparison.OrdinalIgnoreCase))
					return process.ProcessName.Equals("cmd", StringComparison.OrdinalIgnoreCase);

				string expectedPath = Path.GetFullPath(Path.Combine(server.InstallPath, executableName));
				string actualPath = Path.GetFullPath(process.MainModule?.FileName ?? string.Empty);
				return string.Equals(expectedPath, actualPath, StringComparison.OrdinalIgnoreCase);
			}
			catch
			{
				return false;
			}
		}
	}
}
