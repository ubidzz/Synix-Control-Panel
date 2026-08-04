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
using Synix_Control_Panel.SynixEngine;
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;

namespace Synix_Control_Panel.SynixApp.SteamCMDHandler
{
	public static class ServerInstaller
	{
		public static int Install(
			string installPath,
			string appId,
			Action<string> logCallback,
			Action<int>? onPidStarted = null)
		{
			int hasInternalError = 0;
			string lastLoggedLine = "";
			object lineSync = new();

			ProcessStartInfo startInfo = new()
			{
				FileName = @"C:\Synix\SteamCMD\steamcmd.exe",
				Arguments = $"+force_install_dir \"{installPath}\" +login anonymous +app_update {appId} validate +quit",
				WorkingDirectory = @"C:\Synix\SteamCMD",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true
			};

			using Process process = new() { StartInfo = startInfo };

			// The stream readers must never wait for Core.Log/MainGUI.AppendLog.
			// They only place complete SteamCMD messages in this queue.
			Channel<string> logQueue = Channel.CreateUnbounded<string>(
				new UnboundedChannelOptions
				{
					SingleReader = true,
					SingleWriter = false,
					AllowSynchronousContinuations = false
				});

			Task dashboardWriter = Task.Run(async () =>
			{
				await foreach (string line in logQueue.Reader.ReadAllAsync())
				{
					try
					{
						// Keep the existing Core.Log -> MainGUI.AppendLog chain unchanged.
						logCallback?.Invoke(line);
					}
					catch
					{
						// A dashboard logging failure must not stop SteamCMD output capture.
					}
				}
			});

			void QueueSteamLine(string text)
			{
				string line = text.Trim();

				if (line.Length == 0)
					return;

				if (line.Contains("ERROR!", StringComparison.OrdinalIgnoreCase) ||
					line.Contains("subscription", StringComparison.OrdinalIgnoreCase) ||
					line.Contains("AppID not found", StringComparison.OrdinalIgnoreCase) ||
					line.Contains("FAILED", StringComparison.OrdinalIgnoreCase))
				{
					Interlocked.Exchange(ref hasInternalError, 1);
				}

				lock (lineSync)
				{
					if (line.Equals(lastLoggedLine, StringComparison.Ordinal))
						return;

					lastLoggedLine = line;
				}

				logQueue.Writer.TryWrite(line);
			}

			try
			{
				process.Start();
				onPidStarted?.Invoke(process.Id);

				// Read SteamCMD's raw character streams.
				// SteamCMD often terminates progress updates with '\r' instead of '\n'.
				Task outputReader = PumpStreamAsync(
					process.StandardOutput,
					QueueSteamLine);

				Task errorReader = PumpStreamAsync(
					process.StandardError,
					QueueSteamLine);

				process.WaitForExit();

				// Drain both redirected pipes completely.
				Task.WhenAll(outputReader, errorReader)
					.GetAwaiter()
					.GetResult();

				// Finish printing every queued message before returning.
				logQueue.Writer.TryComplete();
				dashboardWriter.GetAwaiter().GetResult();

				Core.Instance.UpdateGridStatus();

				return Volatile.Read(ref hasInternalError) == 1
					? 99
					: process.ExitCode;
			}
			catch (Exception ex)
			{
				logQueue.Writer.TryWrite(
					$"[CRITICAL] Launcher Error: {ex.Message}");

				logQueue.Writer.TryComplete();

				try
				{
					dashboardWriter.GetAwaiter().GetResult();
				}
				catch
				{
					// Preserve the original launcher error result.
				}

				return -1;
			}
		}

		private static async Task PumpStreamAsync(
			StreamReader reader,
			Action<string> queueLine)
		{
			char[] readBuffer = new char[256];
			StringBuilder pending = new();
			bool previousWasCarriageReturn = false;

			while (true)
			{
				int charactersRead = await reader
					.ReadAsync(readBuffer.AsMemory(0, readBuffer.Length))
					.ConfigureAwait(false);

				if (charactersRead == 0)
					break;

				for (int index = 0; index < charactersRead; index++)
				{
					char character = readBuffer[index];

					if (character == '\r')
					{
						FlushPending(pending, queueLine);
						previousWasCarriageReturn = true;
						continue;
					}

					if (character == '\n')
					{
						// Do not create an empty second message for a CRLF pair.
						if (!previousWasCarriageReturn)
							FlushPending(pending, queueLine);

						previousWasCarriageReturn = false;
						continue;
					}

					previousWasCarriageReturn = false;
					pending.Append(character);
				}
			}

			FlushPending(pending, queueLine);
		}

		private static void FlushPending(
			StringBuilder pending,
			Action<string> queueLine)
		{
			if (pending.Length == 0)
				return;

			string line = pending.ToString();
			pending.Clear();
			queueLine(line);
		}

		public static string GetSteamError(int code)
		{
			return code switch
			{
				0 => "Success",
				99 => "Steam Error: AppID not found, no subscription, or SteamCMD reported a failure.",
				5 => "Invalid Arguments",
				7 => "Disk Space Full",
				8 => "Network Connection Lost",
				_ => $"SteamCMD Failure (Code: {code})"
			};
		}
	}
}