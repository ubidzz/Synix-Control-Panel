// Copyright (c) 2026 ubidzz. All Rights Reserved.
//
// This file is part of Synix Control Panel.
//
// This code is provided for transparent viewing and personal use only.
// Unauthorized distribution, public modification, or commercial
// use of this source code or the compiled executable is strictly
// prohibited. Please refer to the LICENSE file in the root
// directory for full terms.

namespace Synix_Control_Panel
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

			// Starts your actual UI
			Application.Run(new MainGUI());
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
				// 1. Define the hardcoded path
				string logDirectory = @"C:\Synix\SynixData\logs";

				// 2. Force Windows to create the folder if it doesn't exist
				Directory.CreateDirectory(logDirectory);

				// 3. Combine the folder path with the exact file name
				string logFilePath = Path.Combine(logDirectory, "synix_fatal_crashes.log");

				string message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [FATAL CRASH]\r\n{ex.Message}\r\n{ex.StackTrace}\r\n----------------------------------------\r\n";

				File.AppendAllText(logFilePath, message);

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