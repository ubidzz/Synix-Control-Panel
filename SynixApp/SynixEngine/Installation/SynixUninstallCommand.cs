using System.Configuration;
using System.Diagnostics;
using Synix_Control_Panel.SynixApp.UI.Installation;

namespace Synix_Control_Panel.SynixEngine;

/// <summary>MSI-only startup path. Never starts servers or accepts deletion paths/consent from arguments.</summary>
internal static class SynixUninstallCommand
{
	internal const string Argument = "--synix-uninstall";
	internal const string DashboardMutex = @"Local\SynixControlPanel.SingleInstance";
	internal static bool IsCommand(IReadOnlyList<string> args) =>
		args.Count > 0 && args[0].Equals(Argument, StringComparison.OrdinalIgnoreCase);
	internal static bool IsInteractive(IReadOnlyList<string> args) =>
		args.Count == 2 && IsCommand(args) && args[1] is "3" or "4" or "5";

	internal static int Run(string[] args)
	{
		bool interactive = IsInteractive(args);
		try
		{
			// Fail closed on unknown arguments. There is deliberately no /purge flag.
			if (args.Length != 2 || !IsCommand(args) || args[1] is not ("2" or "3" or "4" or "5")) return 1;
			if (!interactive)
			{
				PrepareAppRemoval();
				return 0; // Silent removal cleans app files but ALWAYS keeps server data/backups.
			}
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			try { LocalizationManager.Initialize(Properties.Settings.Default.UiLanguage); }
			catch (ConfigurationErrorsException) { LocalizationManager.Initialize("en-US"); }
			using SynixUninstallDialog dialog = new(
				deleteBackups => SynixUninstallPlan.Create(CreatePaths(), deleteBackups), RemoveDataAsync, PrepareAppRemoval);
			return dialog.ShowDialog() == DialogResult.OK ? 0 : 1602;
		}
		catch (Exception exception)
		{
			if (interactive) MessageBox.Show(exception.Message, LocalizationManager.Get("Uninstall.Title"),
				MessageBoxButtons.OK, MessageBoxIcon.Error);
			return 1;
		}
	}

	private static SynixUninstallPaths CreatePaths()
	{
		Properties.Settings.Default.Reload();
		string settingsFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
		string roamingFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoaming).FilePath;
		return new(Core.RootPath, AppContext.BaseDirectory,
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Synix"),
			Path.GetDirectoryName(settingsFile),
			Properties.Settings.Default.CustomBackupPath,
			Path.GetTempPath(), Path.GetDirectoryName(roamingFile));
	}

	private static Task RemoveDataAsync(SynixUninstallPlan plan) => Task.Run(() => WithClosedDashboard(() =>
	{
		plan.EnsurePathsUnchanged(CreatePaths());
		plan.Execute(explicitlyConfirmed: true, () => EnsureServersStopped(plan));
		RemoveStartupEntry();
	}));

	private static void WithClosedDashboard(Action action)
	{
		// Prevent a dashboard/another cleanup from starting during removal. Do not
		// kill server processes: users must stop them themselves before confirming.
		using Mutex gate = new(false, DashboardMutex);
		bool acquired;
		try { acquired = gate.WaitOne(0); }
		catch (AbandonedMutexException) { acquired = true; }
		if (!acquired) throw CloseRequired();
		try
		{
			if (!BackgroundServiceManager.WaitForStop(TimeSpan.FromSeconds(5))) throw CloseRequired();
			action();
		}
		finally { gate.ReleaseMutex(); }
	}

	private static void PrepareAppRemoval() => WithClosedDashboard(() =>
	{
		SynixUninstallPlan plan = SynixUninstallPlan.Create(CreatePaths(), removeServers: false);
		// Game servers may keep running: this scope never touches their folders,
		// C:\Synix, backup locations, server prerequisites or recovery data.
		plan.Execute(explicitlyConfirmed: true, () => { });
		RemoveStartupEntry();
	});

	private static void RemoveStartupEntry()
	{
		using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
			@"Software\Microsoft\Windows\CurrentVersion\Run", writable: true);
		if (Environment.ProcessPath is string executable &&
			IsOwnedStartupCommand(key?.GetValue("Synix Background Service") as string, executable))
			key!.DeleteValue("Synix Background Service", throwOnMissingValue: false);
		// The MSI removes its own registration values and Windows Apps entry.
		// Never sweep legacy installers' registrations or another portable copy.
	}

	internal static bool IsOwnedStartupCommand(string? command, string executable) =>
		string.Equals(command?.Trim(), BackgroundServiceManager.BuildLaunchCommand(executable), StringComparison.OrdinalIgnoreCase);

	private static void EnsureServersStopped(SynixUninstallPlan plan)
	{
		foreach (int pid in plan.Servers.SelectMany(server => server.ProcessIds).Distinct())
		{
			try
			{
				using Process process = Process.GetProcessById(pid);
				if (!process.HasExited) throw StopRequired();
			}
			catch (ArgumentException) { } // PID no longer exists. Stale/reused live PIDs fail safely.
		}
		foreach (Process process in Process.GetProcesses())
		{
			using (process)
			{
				string? executable;
				try { executable = process.MainModule?.FileName; }
				catch (System.ComponentModel.Win32Exception) { continue; } // Inaccessible system/other-user process.
				catch (InvalidOperationException) { continue; } // Exited during enumeration.
				if (executable != null && plan.Folders.Any(folder => SynixUninstallPlan.ContainsPath(folder, executable)))
					throw StopRequired();
			}
		}
	}

	private static InvalidOperationException StopRequired() => new(LocalizationManager.Get("Uninstall.Stop"));
	private static InvalidOperationException CloseRequired() => new(LocalizationManager.Get("Uninstall.CloseApp"));
}
