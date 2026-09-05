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
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.Localization;
using Synix_Control_Panel.SynixApp.UI.Dashboard;
using Synix_Control_Panel.SynixEngine;

namespace Synix_Control_Panel.SynixApp
{
	static class Program
	{
		private const string SingleInstanceMutexName = @"Local\SynixControlPanel.SingleInstance";
		private const string PublishSmokeTestArgument = "--synix-publish-smoke-test";
		private static Mutex? _singleInstanceMutex;

		[STAThread]
		static void Main(string[] args)
		{
			if (FirewallCleanupService.IsCleanupCommand(args))
			{
				Environment.ExitCode =
					FirewallCleanupService.RunElevatedCleanupCommand();
				return;
			}

			if (BackgroundServiceManager.IsAgentCommand(args))
			{
				Environment.ExitCode = BackgroundServiceManager.RunAgent();
				return;
			}

			if (args.Any(argument => string.Equals(
				argument,
				PublishSmokeTestArgument,
				StringComparison.OrdinalIgnoreCase)))
			{
				Environment.ExitCode = RunPublishSmokeTest();
				return;
			}

			if (Core.TryRunUpdateHelper(args))
				return;
			Core.CleanupStaleOperations();
			LocalizationManager.Initialize(
				Properties.Settings.Default.UiLanguage);

			string? updateSuccessMarker = Core
				.GetStartupSuccessMarker(args);
			string? rolledBackVersion = Core
				.GetRollbackVersion(args);

			_singleInstanceMutex = new Mutex(
				initiallyOwned: true,
				SingleInstanceMutexName,
				out bool isFirstInstance);

			if (!isFirstInstance)
			{
				_singleInstanceMutex.Dispose();
				_singleInstanceMutex = null;
				LocalizedMessageBox.Show(
					LocalizationManager.Get("Message.AlreadyRunning.Body"),
					LocalizationManager.Get("Message.AlreadyRunning.Title"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			try
			{
				BackgroundServiceManager.WaitForStop(TimeSpan.FromSeconds(5));
				BackgroundServiceManager.EnsureRegistrationMatchesSetting();
				RunSynix(updateSuccessMarker, rolledBackVersion);
			}
			finally
			{
				ReleaseSingleInstanceMutex();
				BackgroundServiceManager.StartIfEnabled();
			}
		}

		/// <summary>
		/// Confirms that the published app host can load the managed Synix assembly,
		/// Windows Forms, and user settings without displaying a window. The publish
		/// target runs this before it creates the MSI and release receipt.
		/// </summary>
		private static int RunPublishSmokeTest()
		{
			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				_ = typeof(Program).Assembly.GetName().Version
					?? throw new InvalidOperationException(
						LocalizationManager.Get("Application.Error.VersionUnavailable"));
				_ = Properties.Settings.Default.DarkMode;

				using Control windowsFormsProbe = new();
				windowsFormsProbe.CreateControl();
				return 0;
			}
			catch
			{
				return 1;
			}
		}

		private static void RunSynix(
			string? updateSuccessMarker,
			string? rolledBackVersion)
		{

			Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);

			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			ThemeManager.Initialize(Properties.Settings.Default.DarkMode);
			Application.Idle += (_, _) =>
			{
				ThemeManager.ApplyToOpenForms();
				LocalizationManager.ApplyToOpenForms();
			};
			try
			{
				bool importRolledBack = Core
					.RecoverInterruptedImportAsync(Core.RootPath)
					.GetAwaiter()
					.GetResult();

				if (importRolledBack)
				{
					LocalizedMessageBox.Show(
						LocalizationManager.Get(
							"Startup.ImportRecovery.Succeeded.Body"),
						LocalizationManager.Get(
							"Startup.ImportRecovery.Succeeded.Title"),
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
				}
			}
			catch (Exception exception)
			{
				LocalizedMessageBox.Show(
					LocalizationManager.Get(
						"Startup.ImportRecovery.Failed.Body",
						LocalizationManager.TranslateRuntimeText(
							exception.Message)),
					LocalizationManager.Get(
						"Startup.ImportRecovery.Failed.Title"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				return;
			}

			try
			{
				int recoveredRestores = Core.RecoverInterruptedServerRestores();
				if (recoveredRestores > 0)
				{
					LocalizedMessageBox.Show(
						LocalizationManager.Get(
							"Startup.ServerRestoreRecovery.Succeeded.Body",
							recoveredRestores),
						LocalizationManager.Get(
							"Startup.ServerRestoreRecovery.Succeeded.Title"),
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
				}
			}
			catch (Exception exception)
			{
				LocalizedMessageBox.Show(
					LocalizationManager.Get(
						"Startup.ServerRestoreRecovery.Failed.Body",
						LocalizationManager.TranslateRuntimeText(
							exception.Message)),
					LocalizationManager.Get(
						"Startup.ServerRestoreRecovery.Failed.Title"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				return;
			}

			try
			{
				SynixSessionRecovery.BeginSession();
			}
			catch (Exception suppressedException)
			{
				Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException);
			}

			try
			{
				MainGUI mainWindow = new();
				if (!string.IsNullOrWhiteSpace(updateSuccessMarker))
				{
					mainWindow.Shown += (_, _) =>
						Core.MarkStartupSuccessful(
							updateSuccessMarker);
				}
				if (!string.IsNullOrWhiteSpace(rolledBackVersion))
				{
					mainWindow.Shown += (_, _) => LocalizedMessageBox.Show(
						mainWindow,
						LocalizationManager.Get(
							"Startup.UpdateRollback.Body",
							rolledBackVersion),
						LocalizationManager.Get(
							"Startup.UpdateRollback.Title"),
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning);
				}

				Application.Run(mainWindow);
			}
			finally
			{
				SynixSessionRecovery.EndSession();
				FileHandler.FlushLogsAsync()
					.GetAwaiter()
					.GetResult();
			}
		}

		private static void ReleaseSingleInstanceMutex()
		{
			try
			{
				_singleInstanceMutex?.ReleaseMutex();
			}
			catch (ApplicationException suppressedException)
			{
				Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException);
			}
			finally
			{
				_singleInstanceMutex?.Dispose();
				_singleInstanceMutex = null;
			}
		}

		static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			LogFatalCrash(e.Exception);
		}

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			if (e.ExceptionObject is Exception ex)
			{
				LogFatalCrash(ex);
			}
		}

		static void LogFatalCrash(Exception ex)
		{
			try
			{
				string logFilePath = Path.Combine(Core.LogsPath, $"Synix_fatal_crashes_{DateTime.Now:yyyy-MM-dd}.log");
				FileHandler.WriteLogImmediate("Synix_fatal_crashes", $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [FATAL CRASH]\r\n{ex.Message}\r\n{ex.StackTrace}\r\n----------------------------------------\r\n");

				LocalizedMessageBox.Show(
					LocalizationManager.Get(
						"Startup.FatalError.Body",
						logFilePath),
					LocalizationManager.Get("Startup.FatalError.Title"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
			catch (Exception suppressedException)
			{
				Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException);
			}
		}
	}
}
