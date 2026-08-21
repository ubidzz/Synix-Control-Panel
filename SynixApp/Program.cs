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
using Synix_Control_Panel.SynixEngine;

namespace Synix_Control_Panel.SynixApp
{
	static class Program
	{
		private const string SingleInstanceMutexName = @"Local\SynixControlPanel.SingleInstance";
		private static Mutex? _singleInstanceMutex;

		[STAThread]
		static void Main(string[] args)
		{
			if (Core.TryRunUpdateHelper(args))
				return;
			Core.CleanupStaleOperations();

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
				MessageBox.Show(
					"Synix is already running. Please use the existing Synix window.",
					"Synix Already Running",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			try
			{
				RunSynix(updateSuccessMarker, rolledBackVersion);
			}
			finally
			{
				ReleaseSingleInstanceMutex();
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
			Application.Idle += (_, _) => ThemeManager.ApplyToOpenForms();

			try
			{
				bool importRolledBack = Core
					.RecoverInterruptedImportAsync(Core.RootPath)
					.GetAwaiter()
					.GetResult();

				if (importRolledBack)
				{
					MessageBox.Show(
						"Synix detected an interrupted import and safely restored the previous files before starting.",
						"Synix Import Recovered",
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(
					"Synix found an interrupted import but could not safely restore the previous files. " +
					"Synix will not start to avoid using incomplete data.\n\n" +
					exception.Message,
					"Synix Import Recovery Failed",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				return;
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
					mainWindow.Shown += (_, _) => MessageBox.Show(
						mainWindow,
						$"Synix {rolledBackVersion} could not start successfully, so Synix restored the previous program version. Your C:\\Synix server data was not changed.",
						"Synix Update Rolled Back",
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning);
				}

				Application.Run(mainWindow);
			}
			finally
			{

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
			catch (ApplicationException)
			{

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

				MessageBox.Show($"Synix encountered a critical error and needs to close. Please check {logFilePath} for details.",
							"Engine Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch
			{

			}
		}
	}
}
