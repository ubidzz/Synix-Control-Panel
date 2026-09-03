// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;

namespace Synix_Control_Panel.SynixEngine
{
	internal sealed record FirewallOrphanScanResult(
		bool Succeeded,
		IReadOnlyList<string> ExecutablePaths,
		string Message);

	internal sealed record ElevatedFirewallCleanupResult(
		bool Succeeded,
		bool Canceled,
		string Message);

	internal static class FirewallCleanupService
	{
		internal const string CleanupArgument =
			"--synix-clean-orphaned-firewall-rules";

		internal static bool IsCleanupCommand(string[] args) =>
			args.Length == 1 &&
			string.Equals(
				args[0],
				CleanupArgument,
				StringComparison.OrdinalIgnoreCase);

		internal static FirewallOrphanScanResult ScanCurrentRules(
			IEnumerable<GameServer>? registeredServers = null)
		{
			FirewallSnapshot snapshot = WindowsFirewallInspector.Capture();
			if (!snapshot.InspectionSucceeded)
			{
				return new FirewallOrphanScanResult(
					false,
					[],
					snapshot.Problem ?? "Windows Firewall rules could not be inspected.");
			}

			IEnumerable<GameServer> servers = registeredServers ??
				ServerRegistry.Snapshot();
			IReadOnlyList<string> orphaned =
				FindOrphanedDefaultServerExecutables(
					snapshot.ProgramExecutables,
					Core.GamesPath,
					servers.Select(server => server.InstallPath),
					Directory.Exists);

			return new FirewallOrphanScanResult(
				true,
				orphaned,
				orphaned.Count == 0
					? "No orphaned firewall rules were found in the default Synix Games folder."
					: $"Found firewall rules for {orphaned.Count} deleted server executable path(s).");
		}

		internal static IReadOnlyList<string>
			FindOrphanedDefaultServerExecutables(
				IEnumerable<string> firewallExecutablePaths,
				string gamesRoot,
				IEnumerable<string> registeredInstallPaths,
				Func<string, bool> directoryExists)
		{
			ArgumentNullException.ThrowIfNull(firewallExecutablePaths);
			ArgumentException.ThrowIfNullOrWhiteSpace(gamesRoot);
			ArgumentNullException.ThrowIfNull(registeredInstallPaths);
			ArgumentNullException.ThrowIfNull(directoryExists);

			if (!TryNormalizePath(gamesRoot, out string normalizedGamesRoot))
				return [];

			HashSet<string> registeredRoots = new(StringComparer.OrdinalIgnoreCase);
			foreach (string registeredInstallPath in registeredInstallPaths)
			{
				if (TryNormalizePath(registeredInstallPath, out string normalizedRoot))
					registeredRoots.Add(normalizedRoot);
			}
			HashSet<string> orphaned = new(StringComparer.OrdinalIgnoreCase);
			foreach (string candidate in firewallExecutablePaths)
			{
				if (!TryNormalizePath(candidate, out string executablePath) ||
					!executablePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
					!IsPathInsideDirectory(executablePath, normalizedGamesRoot))
				{
					continue;
				}

				string relativePath = Path.GetRelativePath(
					normalizedGamesRoot,
					executablePath);
				string[] segments = relativePath.Split(
					[Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
					StringSplitOptions.RemoveEmptyEntries);
				if (segments.Length < 3 ||
					segments[0] is "." or ".." ||
					segments[1] is "." or "..")
				{
					continue;
				}

				string serverFolder = Path.Combine(
					normalizedGamesRoot,
					segments[0],
					segments[1]);
				bool belongsToRegisteredServer = registeredRoots.Any(root =>
					IsPathInsideOrEqual(executablePath, root));
				if (belongsToRegisteredServer || directoryExists(serverFolder))
					continue;

				orphaned.Add(executablePath);
			}

			return orphaned
				.OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
				.ToArray();
		}

		internal static async Task<ElevatedFirewallCleanupResult>
			RunElevatedCleanupAsync()
		{
			if (!OperatingSystem.IsWindows())
			{
				return new ElevatedFirewallCleanupResult(
					false,
					false,
					"Firewall cleanup is available only on Windows.");
			}

			try
			{
				ProcessStartInfo startInfo = new(Application.ExecutablePath)
				{
					UseShellExecute = true,
					Verb = "runas",
					WindowStyle = ProcessWindowStyle.Hidden
				};
				startInfo.ArgumentList.Add(CleanupArgument);
				using Process? process = Process.Start(startInfo);
				if (process == null)
				{
					return new ElevatedFirewallCleanupResult(
						false,
						false,
						"Windows could not start the approved firewall cleanup task.");
				}

				await process.WaitForExitAsync();
				return process.ExitCode == 0
					? new ElevatedFirewallCleanupResult(
						true,
						false,
						"Synix removed and verified the orphaned firewall rules.")
					: new ElevatedFirewallCleanupResult(
						false,
						false,
						"The elevated firewall cleanup did not complete. No rules outside the default Synix Games folder were targeted.");
			}
			catch (Win32Exception exception) when (exception.NativeErrorCode == 1223)
			{
				return new ElevatedFirewallCleanupResult(
					false,
					true,
					"Administrator permission was canceled. No firewall rules were changed.");
			}
			catch (Exception exception)
			{
				return new ElevatedFirewallCleanupResult(
					false,
					false,
					$"Firewall cleanup could not start: {exception.Message}");
			}
		}

		internal static int RunElevatedCleanupCommand()
		{
			if (!OperatingSystem.IsWindows() || !IsCurrentProcessAdministrator())
				return 2;

			if (!TryLoadSavedServers(out IReadOnlyList<GameServer> servers))
				return 3;

			FirewallOrphanScanResult scan = ScanCurrentRules(servers);
			if (!scan.Succeeded)
				return 4;
			if (scan.ExecutablePaths.Count == 0)
				return 0;

			foreach (string executablePath in scan.ExecutablePaths)
			{
				if (!DeleteRulesForExecutable(executablePath))
					return 5;
			}

			FirewallOrphanScanResult verification = ScanCurrentRules(servers);
			return verification.Succeeded && verification.ExecutablePaths.Count == 0
				? 0
				: 6;
		}

		private static bool TryLoadSavedServers(
			out IReadOnlyList<GameServer> servers)
		{
			servers = [];
			try
			{
				string path = Path.Combine(Core.DataPath, "servers.json");
				if (!File.Exists(path))
					return true;

				string json = File.ReadAllText(path);
				servers = Core.DeserializeServersAndMigrate(json, out int _);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private static bool DeleteRulesForExecutable(string executablePath)
		{
			if (!TryGetDefaultServerFolder(executablePath, out string serverFolder) ||
				Directory.Exists(serverFolder))
			{
				return false;
			}

			ProcessStartInfo startInfo = new(
				Path.Combine(Environment.SystemDirectory, "netsh.exe"))
			{
				UseShellExecute = false,
				CreateNoWindow = true
			};
			startInfo.ArgumentList.Add("advfirewall");
			startInfo.ArgumentList.Add("firewall");
			startInfo.ArgumentList.Add("delete");
			startInfo.ArgumentList.Add("rule");
			startInfo.ArgumentList.Add("name=all");
			startInfo.ArgumentList.Add($"program={executablePath}");
			using Process? process = Process.Start(startInfo);
			if (process == null)
				return false;
			process.WaitForExit();
			return process.ExitCode == 0;
		}

		private static bool TryGetDefaultServerFolder(
			string executablePath,
			out string serverFolder)
		{
			serverFolder = string.Empty;
			if (!TryNormalizePath(Core.GamesPath, out string gamesRoot) ||
				!TryNormalizePath(executablePath, out string normalizedExecutable) ||
				!IsPathInsideDirectory(normalizedExecutable, gamesRoot))
			{
				return false;
			}

			string[] segments = Path.GetRelativePath(gamesRoot, normalizedExecutable)
				.Split(
					[Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
					StringSplitOptions.RemoveEmptyEntries);
			if (segments.Length < 3)
				return false;

			serverFolder = Path.Combine(gamesRoot, segments[0], segments[1]);
			return true;
		}

		private static bool IsCurrentProcessAdministrator()
		{
			using WindowsIdentity identity = WindowsIdentity.GetCurrent();
			return new WindowsPrincipal(identity).IsInRole(
				WindowsBuiltInRole.Administrator);
		}

		private static bool TryNormalizePath(
			string? path,
			out string normalizedPath)
		{
			normalizedPath = string.Empty;
			if (string.IsNullOrWhiteSpace(path))
				return false;

			try
			{
				normalizedPath = Path.TrimEndingDirectorySeparator(
					Path.GetFullPath(
						Environment.ExpandEnvironmentVariables(path.Trim().Trim('"'))));
				return true;
			}
			catch
			{
				return false;
			}
		}

		private static bool IsPathInsideOrEqual(string path, string directory) =>
			string.Equals(path, directory, StringComparison.OrdinalIgnoreCase) ||
			IsPathInsideDirectory(path, directory);

		private static bool IsPathInsideDirectory(string path, string directory)
		{
			string prefix = Path.EndsInDirectorySeparator(directory)
				? directory
				: directory + Path.DirectorySeparatorChar;
			return path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
		}
	}
}
