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
using Synix_Control_Panel.SynixEngine;

namespace Synix_Control_Panel.SynixApp
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// 🛡️ 1. CATCH UI THREAD CRASHES
			// Catches things like bad button clicks or grid rendering errors
			Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);

			// 🛡️ 2. CATCH BACKGROUND THREAD CRASHES
			// Catches things like Watchdog failures or Engine loop crashes
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			try
			{
				Application.Run(new MainGUI());
			}
			finally
			{
				// Wait for queued log entries to reach disk before exiting.
				FileHandler.FlushLogsAsync()
					.GetAwaiter()
					.GetResult();
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
				// Silent fail
			}
		}
	}
}