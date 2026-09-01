// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Microsoft.Win32;
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using System.Diagnostics;

namespace Synix_Control_Panel.SynixEngine
{
	internal static class BackgroundServiceManager
	{
		private static bool _suppressStartForCurrentProcess;
		internal const string AgentArgument = "--synix-background-agent";
		private const string AgentMutexName = @"Local\SynixControlPanel.BackgroundAgent";
		private const string AgentStopEventName = @"Local\SynixControlPanel.BackgroundAgent.Stop";
		private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
		private const string RunValueName = "Synix Background Service";

		internal static bool IsAgentCommand(IReadOnlyList<string> arguments) =>
			arguments.Any(argument => argument.Equals(
				AgentArgument,
				StringComparison.OrdinalIgnoreCase));

		internal static string BuildLaunchCommand(string executablePath) =>
			$"\"{Path.GetFullPath(executablePath)}\" {AgentArgument}";

		internal static bool SetEnabled(bool enabled, out string message)
		{
			try
			{
				using RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true)
					?? throw new InvalidOperationException("The Windows startup settings could not be opened.");
				if (enabled)
				{
					string executablePath = GetExecutablePath();
					key.SetValue(RunValueName, BuildLaunchCommand(executablePath), RegistryValueKind.String);
					message = "Enabled. Synix will keep monitoring after the dashboard closes and will start automatically when you sign in.";
				}
				else
				{
					key.DeleteValue(RunValueName, throwOnMissingValue: false);
					RequestStop();
					message = "Disabled. Background monitoring will stop and will not start at sign-in.";
				}
				return true;
			}
			catch (Exception exception)
			{
				message = $"Windows could not update the background service setting: {exception.Message}";
				return false;
			}
		}

		internal static void EnsureRegistrationMatchesSetting()
		{
			if (Properties.Settings.Default.BackgroundServiceEnabled)
				SetEnabled(true, out _);
		}

		internal static void RequestStop()
		{
			try
			{
				using EventWaitHandle stopEvent = EventWaitHandle.OpenExisting(AgentStopEventName);
				stopEvent.Set();
			}
			catch (WaitHandleCannotBeOpenedException)
			{
			}
			catch
			{
			}
		}

		internal static bool WaitForStop(TimeSpan timeout)
		{
			RequestStop();
			DateTime deadline = DateTime.UtcNow + timeout;
			do
			{
				try
				{
					using Mutex mutex = Mutex.OpenExisting(AgentMutexName);
					if (mutex.WaitOne(TimeSpan.FromMilliseconds(100)))
					{
						try { mutex.ReleaseMutex(); }
						catch (ApplicationException) { }
						return true;
					}
				}
				catch (WaitHandleCannotBeOpenedException)
				{
					return true;
				}
				catch
				{
					return false;
				}
			}
			while (DateTime.UtcNow < deadline);

			return false;
		}

		internal static void StartIfEnabled()
		{
			if (_suppressStartForCurrentProcess ||
				!Properties.Settings.Default.BackgroundServiceEnabled ||
				IsAgentRunning())
				return;

			try
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = GetExecutablePath(),
					Arguments = AgentArgument,
					UseShellExecute = false,
					CreateNoWindow = true,
					WindowStyle = ProcessWindowStyle.Hidden
				});
			}
			catch (Exception exception)
			{
				FileHandler.WriteLogImmediate(
					"Synix_Background_Service",
					$"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Could not start: {exception}\r\n");
			}
		}

		internal static void SuppressStartForCurrentProcess() =>
			_suppressStartForCurrentProcess = true;

		internal static int RunAgent()
		{
			using Mutex mutex = new(
				initiallyOwned: true,
				AgentMutexName,
				out bool isFirstAgent);
			if (!isFirstAgent)
				return 0;

			using EventWaitHandle stopEvent = new(
				initialState: false,
				EventResetMode.AutoReset,
				AgentStopEventName);
			try
			{
				Core.IsBackgroundServiceMode = true;
				FileHandler.LoadServers();
				Core.Instance.RebindProcesses().GetAwaiter().GetResult();
				FileHandler.WriteLogImmediate(
					"Synix_Background_Service",
					$"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Background monitoring started.\r\n");
				stopEvent.WaitOne();
				FileHandler.SaveServers();
				FileHandler.FlushLogsAsync().GetAwaiter().GetResult();
				return 0;
			}
			catch (Exception exception)
			{
				FileHandler.WriteLogImmediate(
					"Synix_Background_Service",
					$"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Background monitoring stopped unexpectedly: {exception}\r\n");
				return 1;
			}
			finally
			{
				Core.IsBackgroundServiceMode = false;
				try { mutex.ReleaseMutex(); }
				catch (ApplicationException) { }
			}
		}

		private static bool IsAgentRunning()
		{
			try
			{
				using Mutex mutex = Mutex.OpenExisting(AgentMutexName);
				return true;
			}
			catch (WaitHandleCannotBeOpenedException)
			{
				return false;
			}
			catch
			{
				return false;
			}
		}

		private static string GetExecutablePath() =>
			Environment.ProcessPath ??
			throw new InvalidOperationException("The Synix program path could not be determined.");
	}
}
