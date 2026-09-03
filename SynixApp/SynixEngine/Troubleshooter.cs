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
using Synix_Control_Panel.SynixApp.Database.GameConfigurations;
using Synix_Control_Panel.SynixApp.ServerHandler;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Synix_Control_Panel.SynixEngine
{
	internal enum SynixHealthLevel
	{
		Passed,
		Warning,
		Failed
	}

	internal enum SynixHealthAction
	{
		None,
		RepairSteamCmd,
		ValidateServerFiles,
		FixConfiguration,
		OpenServerFolder,
		OpenFirewallSettings,
		RecoverProcesses,
		OpenLatestLog,
		OpenUpdate
	}

	internal sealed record SynixHealthItem(
		SynixHealthLevel Level,
		string Area,
		string Subject,
		string Details,
		SynixHealthAction Action = SynixHealthAction.None,
		GameServer? Server = null)
	{
		internal string ResultText => Level switch
		{
			SynixHealthLevel.Passed => "Passed",
			SynixHealthLevel.Warning => "Warning",
			_ => "Failed"
		};
	}

	internal sealed class SynixHealthReport
	{
		internal SynixHealthReport(
			DateTimeOffset completedAtUtc,
			IReadOnlyList<SynixHealthItem> items)
		{
			CompletedAtUtc = completedAtUtc;
			Items = items;
		}

		internal DateTimeOffset CompletedAtUtc { get; }
		internal IReadOnlyList<SynixHealthItem> Items { get; }
		internal int PassedCount => Items.Count(item => item.Level == SynixHealthLevel.Passed);
		internal int WarningCount => Items.Count(item => item.Level == SynixHealthLevel.Warning);
		internal int FailedCount => Items.Count(item => item.Level == SynixHealthLevel.Failed);
		internal bool IsHealthy => FailedCount == 0;

		internal string ToPlainText(string title = "SYNIX TROUBLESHOOTER REPORT")
		{
			StringBuilder text = new();
			text.AppendLine(title);
			text.AppendLine();
			text.AppendLine($"Completed: {CompletedAtUtc.ToLocalTime():g}");
			text.AppendLine($"Synix: v{Core.GetCurrentVersion().ToString(3)} ({Core.DetectCurrentInstallation().DisplayName})");
			text.AppendLine($"Passed: {PassedCount}  Warnings: {WarningCount}  Failed: {FailedCount}");
			text.AppendLine();
			foreach (SynixHealthItem item in Items)
			{
				text.AppendLine($"[{item.ResultText.ToUpperInvariant()}] {item.Area} — {item.Subject}");
				text.AppendLine(item.Details);
			}

			return text.ToString().TrimEnd();
		}
	}

	internal static class SynixTroubleshooter
	{
		private const long OneGibibyte = 1024L * 1024L * 1024L;
		private const int MaximumLogBytes = 512 * 1024;
		private static readonly Regex ErrorLinePattern = new(
			@"(?im)^.*(?:fatal error|unhandled exception|access violation|couldn['’]?t allocate|cannot bind|address already in use|server packages file not found|missing required|failed to load|redis.*(?:failed|missing|connection)|error\s*:).*$",
			RegexOptions.Compiled | RegexOptions.CultureInvariant,
			TimeSpan.FromSeconds(2));

		internal static async Task<SynixHealthReport> RunAsync(
			IReadOnlyList<GameServer> servers,
			bool checkForUpdates,
			IProgress<string>? progress = null,
			CancellationToken cancellationToken = default,
			bool includeUpdateStatus = true)
		{
			ArgumentNullException.ThrowIfNull(servers);
			List<SynixHealthItem> items = [];
			progress?.Report("Checking SteamCMD and shared runtimes...");
			CheckSteamCmd(items);
			CheckJavaRuntimes(servers, items);
			CheckDiskSpace(servers, items);

			FirewallSnapshot firewall = WindowsFirewallInspector.Capture();
			if (!firewall.InspectionSucceeded)
			{
				items.Add(new SynixHealthItem(
					SynixHealthLevel.Warning,
					"Windows Firewall",
					"Firewall inspection",
					firewall.Problem ?? "Windows Firewall rules could not be inspected.",
					SynixHealthAction.OpenFirewallSettings));
			}
			else if (!firewall.Enabled)
			{
				items.Add(new SynixHealthItem(
					SynixHealthLevel.Warning,
					"Windows Firewall",
					"Firewall protection",
					"Windows Firewall is disabled for every active network profile. Synix did not change it.",
					SynixHealthAction.OpenFirewallSettings));
			}

			for (int index = 0; index < servers.Count; index++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				GameServer server = servers[index];
				progress?.Report($"Checking {server.ServerName} ({index + 1} of {servers.Count})...");
				GameInfo? game = GameDatabase.GetGame(server.Game);
				if (game == null)
				{
					items.Add(new SynixHealthItem(
						SynixHealthLevel.Failed,
						"Server files",
						server.ServerName,
						$"The saved game '{server.Game}' no longer exists in the built-in definition library.",
						SynixHealthAction.OpenServerFolder,
						server));
					continue;
				}

				RunServerCheck(items, "Server files", server, () => CheckServerFiles(server, game, items));
				RunServerCheck(items, "SteamCMD and runtimes", server, () => CheckRuntimeRequirements(server, game, items));
				try
				{
					await CheckConfigurationAsync(server, items);
				}
				catch (Exception exception)
				{
					AddCheckFailure(items, "Configuration health", server, exception);
				}
				RunServerCheck(items, "Ports", server, () => CheckPorts(server, game, servers, items));
				RunServerCheck(items, "Windows Firewall", server, () => CheckFirewall(server, game, firewall, items));
				RunServerCheck(items, "Process tracking", server, () => CheckProcessRecovery(server, items));
				RunServerCheck(items, "Recent server logs", server, () => CheckLatestLog(server, items));
			}

			if (includeUpdateStatus && checkForUpdates)
			{
				progress?.Report("Checking the installed Synix version...");
				await CheckUpdateAsync(items, cancellationToken);
			}
			else if (includeUpdateStatus)
			{
				items.Add(new SynixHealthItem(
					SynixHealthLevel.Passed,
					"Synix update",
					$"v{Core.GetCurrentVersion().ToString(3)}",
					$"Installed as {Core.DetectCurrentInstallation().DisplayName}. The online update check was skipped."));
			}

			return new SynixHealthReport(DateTimeOffset.UtcNow, items);
		}

		private static void CheckSteamCmd(ICollection<SynixHealthItem> items)
		{
			bool executableExists = File.Exists(Core.SteamCmdExe);
			bool initialized = Directory.Exists(Path.Combine(Core.SteamCmdPath, "public"));
			items.Add(new SynixHealthItem(
				executableExists && initialized
					? SynixHealthLevel.Passed
					: SynixHealthLevel.Failed,
				"SteamCMD and runtimes",
				"SteamCMD",
				executableExists && initialized
					? $"SteamCMD is initialized at {Core.SteamCmdPath}."
					: "SteamCMD is missing or has not completed its first-run initialization.",
				executableExists && initialized
					? SynixHealthAction.None
					: SynixHealthAction.RepairSteamCmd));
		}

		private static void CheckJavaRuntimes(
			IEnumerable<GameServer> servers,
			ICollection<SynixHealthItem> items)
		{
			foreach (IGrouping<int, GameServer> requiredRuntime in servers
				.Where(server => server.RequiredJavaVersion > 0)
				.GroupBy(server => server.RequiredJavaVersion)
				.OrderBy(group => group.Key))
			{
				int version = requiredRuntime.Key;
				GameServer affectedServer = requiredRuntime.First();
				string directory = Path.Combine(Core.RuntimesPath, $"Java{version}");
				bool available = false;
				try
				{
					available = Directory.Exists(directory) &&
						Directory.EnumerateFiles(directory, "java.exe", SearchOption.AllDirectories).Any();
				}
				catch
				{
				}
				items.Add(new SynixHealthItem(
					available ? SynixHealthLevel.Passed : SynixHealthLevel.Failed,
					"SteamCMD and runtimes",
					$"Java {version}",
					available
						? $"The managed Java {version} runtime is available."
						: $"A saved Minecraft server requires Java {version}, but its managed runtime is missing. Update that server to restore it.",
					available ? SynixHealthAction.None : SynixHealthAction.ValidateServerFiles,
					affectedServer));
			}
		}

		private static void CheckServerFiles(
			GameServer server,
			GameInfo game,
			ICollection<SynixHealthItem> items)
		{
			if (string.IsNullOrWhiteSpace(server.InstallPath) ||
				!Directory.Exists(server.InstallPath))
			{
				items.Add(new SynixHealthItem(
					SynixHealthLevel.Failed,
					"Server files",
					server.ServerName,
					"The installed server folder is missing.",
					SynixHealthAction.ValidateServerFiles,
					server));
				return;
			}

			string executable;
			try
			{
				executable = Path.GetFullPath(
					GameLaunchCommandBuilder.ResolveExecutablePath(server, game));
			}
			catch (Exception exception)
			{
				items.Add(new SynixHealthItem(
					SynixHealthLevel.Failed,
					"Server files",
					server.ServerName,
					$"The saved executable path is invalid: {exception.Message}",
					SynixHealthAction.OpenServerFolder,
					server));
				return;
			}
			List<string> missing = [];
			if (!File.Exists(executable))
				missing.Add(Path.GetFileName(executable));
			foreach (string relativePath in game.RequiredLaunchFiles)
			{
				try
				{
					if (!File.Exists(Path.GetFullPath(Path.Combine(server.InstallPath, relativePath))))
						missing.Add(relativePath);
				}
				catch
				{
					missing.Add(relativePath);
				}
			}

			items.Add(new SynixHealthItem(
				missing.Count == 0 ? SynixHealthLevel.Passed : SynixHealthLevel.Failed,
				"Server files",
				server.ServerName,
				missing.Count == 0
					? "The server executable and every declared required launch file are present. Use Validate Server Files for a complete Steam byte check."
					: "Missing required files: " + string.Join(", ", missing),
				SynixHealthAction.ValidateServerFiles,
				server));
		}

		private static void CheckRuntimeRequirements(
			GameServer server,
			GameInfo game,
			ICollection<SynixHealthItem> items)
		{
			GamePrerequisiteReport report = GamePrerequisiteChecker.CheckCurrentSystem(game);
			foreach (GamePrerequisiteItem result in report.Items
				.Where(result => result.State != GamePrerequisiteState.Passed))
			{
				items.Add(new SynixHealthItem(
					result.State == GamePrerequisiteState.Failed
						? SynixHealthLevel.Failed
						: SynixHealthLevel.Warning,
					"SteamCMD and runtimes",
					$"{server.ServerName}: {result.Name}",
					result.Message,
					SynixHealthAction.None,
					server));
			}
		}

		private static async Task CheckConfigurationAsync(
			GameServer server,
			ICollection<SynixHealthItem> items)
		{
			if (server.PreserveImportedConfiguration)
			{
				items.Add(new SynixHealthItem(
					SynixHealthLevel.Passed,
					"Configuration health",
					server.ServerName,
					"This imported server keeps its existing configuration. Synix will begin managing supported values only after you open Server Settings and save changes.",
					SynixHealthAction.None,
					server));
				return;
			}

			ConfigurationValidationReport report =
				await GameFix.ValidateManagedConfiguration(server);
			SynixHealthLevel level = report.FailedCount > 0
				? SynixHealthLevel.Failed
				: report.WarningCount > 0
					? SynixHealthLevel.Warning
					: SynixHealthLevel.Passed;
			string details = level == SynixHealthLevel.Passed
				? "The managed configuration is complete and uses the current template revision."
				: string.Join(
					" ",
					report.Items
						.Where(item => item.State != ConfigurationValidationState.Passed)
						.Take(3)
						.Select(item => item.Message));
			items.Add(new SynixHealthItem(
				level,
				"Configuration health",
				server.ServerName,
				details,
				report.FixConfigAvailable && level != SynixHealthLevel.Passed
					? SynixHealthAction.FixConfiguration
					: SynixHealthAction.None,
				server));
		}

		private static void CheckPorts(
			GameServer server,
			GameInfo game,
			IReadOnlyList<GameServer> servers,
			ICollection<SynixHealthItem> items)
		{
			foreach ((int port, string name) in Core.GetRequiredServerPorts(server, game))
			{
				List<GameServer> configuredOwners = servers
					.Where(candidate => candidate != server)
					.Where(candidate =>
					{
						GameInfo? candidateGame = GameDatabase.GetGame(candidate.Game);
						return candidateGame != null &&
							Core.GetRequiredServerPorts(candidate, candidateGame)
								.Any(required => required.Port == port);
					})
					.ToList();
				bool serverActive = Core.IsActivePortReservation(server);
				if (configuredOwners.Count > 0)
				{
					items.Add(new SynixHealthItem(
						SynixHealthLevel.Failed,
						"Ports",
						$"{server.ServerName}: {port}",
						$"The {name} is also assigned to {string.Join(", ", configuredOwners.Select(owner => owner.ServerName))}. Every saved server, including each cluster member, must use its own port.",
						SynixHealthAction.None,
						server));
					continue;
				}

				bool occupied = Core.Instance.IsPortInUseLocally(port);
				items.Add(new SynixHealthItem(
					occupied && !serverActive
						? SynixHealthLevel.Failed
						: SynixHealthLevel.Passed,
					"Ports",
					$"{server.ServerName}: {port}",
					occupied
						? serverActive
							? $"The {name} is bound while this server is active."
							: $"The {name} is occupied by another program while this server is stopped."
						: $"The {name} is currently available.",
					SynixHealthAction.None,
					server));
			}
		}

		private static void CheckFirewall(
			GameServer server,
			GameInfo game,
			FirewallSnapshot firewall,
			ICollection<SynixHealthItem> items)
		{
			if (!firewall.InspectionSucceeded || !firewall.Enabled ||
				string.IsNullOrWhiteSpace(server.InstallPath))
			{
				return;
			}

			string launchFile;
			try
			{
				launchFile = Path.GetFullPath(
					GameLaunchCommandBuilder.ResolveExecutablePath(server, game));
			}
			catch
			{
				return;
			}
			if (!File.Exists(launchFile))
				return;

			string? allowedExecutable = WindowsFirewallInspector.FindAllowedExecutable(
				server.InstallPath,
				launchFile,
				firewall.AllowedExecutables);
			bool allowed = allowedExecutable != null;
			string checkedFile = GetInstalledRelativePath(server.InstallPath, launchFile);
			string matchedFile = allowedExecutable == null
				? string.Empty
				: GetInstalledRelativePath(server.InstallPath, allowedExecutable);
			items.Add(new SynixHealthItem(
				allowed ? SynixHealthLevel.Passed : SynixHealthLevel.Warning,
				"Windows Firewall",
				server.ServerName,
				allowed
					? $"An enabled inbound program rule targets the installed server executable '{matchedFile}'."
					: $"No enabled inbound program rule points to an executable inside this server's install folder. The configured launch file is '{checkedFile}'. This check does not use ports; Windows may prompt when the server starts, and router port forwarding is separate.",
				allowed ? SynixHealthAction.None : SynixHealthAction.OpenFirewallSettings,
				server));
		}

		private static string GetInstalledRelativePath(string installPath, string path)
		{
			try
			{
				string relative = Path.GetRelativePath(
					Path.GetFullPath(installPath),
					Path.GetFullPath(path));
				return relative.StartsWith("..", StringComparison.Ordinal)
					? Path.GetFileName(path)
					: relative;
			}
			catch
			{
				return Path.GetFileName(path);
			}
		}

		private static void CheckProcessRecovery(
			GameServer server,
			ICollection<SynixHealthItem> items)
		{
			IReadOnlyList<ServerProcessIdentity> processes =
				Servers.RefreshServerProcessRegistry(server, forceDiscovery: true);
			bool statusClaimsActive =
				server.Status == Core.StatusManager.GetStatus(Core.ServerState.Running) ||
				server.Status == Core.StatusManager.GetStatus(Core.ServerState.Starting) ||
				server.Status == Core.StatusManager.GetStatus(Core.ServerState.Stopping);
			bool primaryTracked = server.PID.HasValue &&
				processes.Any(process => process.ProcessId == server.PID.Value);

			if (processes.Count > 0)
			{
				string processDetails = string.Join(
					", ",
					processes.Select(process =>
						$"{Path.GetFileName(process.ExecutablePath)} (PID {process.ProcessId})"));
				bool needsRecovery = !statusClaimsActive || !primaryTracked;
				items.Add(new SynixHealthItem(
					needsRecovery ? SynixHealthLevel.Warning : SynixHealthLevel.Passed,
					"Process tracking",
					server.ServerName,
					needsRecovery
						? $"Found {processes.Count} installed server process(es), but the saved server state needs to be reconnected: {processDetails}."
						: $"Synix is tracking {processes.Count} verified server process(es): {processDetails}.",
					needsRecovery ? SynixHealthAction.RecoverProcesses : SynixHealthAction.None,
					server));
				return;
			}

			items.Add(new SynixHealthItem(
				statusClaimsActive ? SynixHealthLevel.Warning : SynixHealthLevel.Passed,
				"Process tracking",
				server.ServerName,
				statusClaimsActive
					? "The saved status says this server is active, but no verified executable is currently running. Recover Processes will reconcile its state."
					: "No server executable is running, which is correct while this server is stopped.",
				statusClaimsActive ? SynixHealthAction.RecoverProcesses : SynixHealthAction.None,
				server));
		}

		private static void CheckLatestLog(
			GameServer server,
			ICollection<SynixHealthItem> items)
		{
			GameLogDiscoveryResult result = GameLogDiscovery.FindLatest(server);
			if (!result.Found)
			{
				items.Add(new SynixHealthItem(
					GameLogDiscovery.HasDeclaredLogs(server.Game)
						? SynixHealthLevel.Warning
						: SynixHealthLevel.Passed,
					"Recent server logs",
					server.ServerName,
					GameLogDiscovery.HasDeclaredLogs(server.Game)
						? "No file was found at the declared game-log locations yet."
						: "This game does not have a verified log location in its built-in definition yet.",
					SynixHealthAction.None,
					server));
				return;
			}

			string tail = ReadLogTail(result.LatestLogPath!);
			string? likelyProblem = FindLikelyLogProblem(tail);
			items.Add(new SynixHealthItem(
				likelyProblem == null ? SynixHealthLevel.Passed : SynixHealthLevel.Warning,
				"Recent server logs",
				server.ServerName,
				likelyProblem == null
					? $"No common startup failure was found in {Path.GetFileName(result.LatestLogPath)}."
					: likelyProblem,
				SynixHealthAction.OpenLatestLog,
				server));
		}

		internal static string? FindLikelyLogProblem(string logText)
		{
			if (string.IsNullOrWhiteSpace(logText))
				return null;
			Match match = ErrorLinePattern.Matches(logText).Cast<Match>().LastOrDefault()!;
			if (match == null || !match.Success)
				return null;
			string line = Regex.Replace(match.Value.Trim(), @"\s+", " ");
			if (line.Length > 360)
				line = line[..360] + "…";
			return "The newest log contains a likely startup problem: " + line;
		}

		private static string ReadLogTail(string path)
		{
			try
			{
				using FileStream stream = new(
					path,
					FileMode.Open,
					FileAccess.Read,
					FileShare.ReadWrite | FileShare.Delete);
				if (stream.Length > MaximumLogBytes)
					stream.Seek(-MaximumLogBytes, SeekOrigin.End);
				using StreamReader reader = new(stream, detectEncodingFromByteOrderMarks: true);
				return reader.ReadToEnd();
			}
			catch
			{
				return string.Empty;
			}
		}

		private static void CheckDiskSpace(
			IEnumerable<GameServer> servers,
			ICollection<SynixHealthItem> items)
		{
			HashSet<string> roots = new(StringComparer.OrdinalIgnoreCase);
			foreach (string path in servers.Select(server => server.InstallPath).Append(Core.RootPath))
			{
				try
				{
					if (!string.IsNullOrWhiteSpace(path))
					{
						string? root = Path.GetPathRoot(Path.GetFullPath(path));
						if (!string.IsNullOrWhiteSpace(root))
							roots.Add(root);
					}
				}
				catch
				{
				}
			}
			foreach (string root in roots)
			{
				try
				{
					DriveInfo drive = new(root);
					long free = drive.AvailableFreeSpace;
					SynixHealthLevel level = free < OneGibibyte
						? SynixHealthLevel.Failed
						: free < 10 * OneGibibyte
							? SynixHealthLevel.Warning
							: SynixHealthLevel.Passed;
					items.Add(new SynixHealthItem(
						level,
						"Available disk space",
						root,
						$"{FormatBytes(free)} is available. Large installs, updates, validation, and backups may require additional temporary space."));
				}
				catch (Exception exception)
				{
					items.Add(new SynixHealthItem(
						SynixHealthLevel.Warning,
						"Available disk space",
						root,
						$"Synix could not inspect this drive: {exception.Message}"));
				}
			}
		}

		private static async Task CheckUpdateAsync(
			ICollection<SynixHealthItem> items,
			CancellationToken cancellationToken)
		{
			try
			{
				Version current = Core.GetCurrentVersion();
				SynixUpdateCheckResult result = await Core.CheckForUpdatesAsync(
					current,
					cancellationToken);
				items.Add(new SynixHealthItem(
					result.UpdateAvailable
						? SynixHealthLevel.Warning
						: SynixHealthLevel.Passed,
					"Synix update",
					$"v{current.ToString(3)} — {result.Installation.DisplayName}",
					result.UpdateAvailable
						? result.ReleaseReady
							? $"Synix {result.AdvertisedVersion!.ToString(3)} is available and its release assets are ready."
							: result.Problem ?? "A newer version is being prepared."
						: "This installation matches or exceeds the advertised Synix version.",
					result.UpdateAvailable
						? SynixHealthAction.OpenUpdate
						: SynixHealthAction.None));
			}
			catch (Exception exception) when (exception is HttpRequestException or
				TaskCanceledException or InvalidDataException)
			{
				items.Add(new SynixHealthItem(
					SynixHealthLevel.Warning,
					"Synix update",
					$"v{Core.GetCurrentVersion().ToString(3)}",
					$"The online update check was unavailable: {exception.Message}",
					SynixHealthAction.OpenUpdate));
			}
		}

		private static string FormatBytes(long bytes)
		{
			string[] units = ["B", "KB", "MB", "GB", "TB"];
			double value = bytes;
			int unit = 0;
			while (value >= 1024 && unit < units.Length - 1)
			{
				value /= 1024;
				unit++;
			}
			return $"{value:0.##} {units[unit]}";
		}

		private static void RunServerCheck(
			ICollection<SynixHealthItem> items,
			string area,
			GameServer server,
			Action check)
		{
			try
			{
				check();
			}
			catch (Exception exception)
			{
				AddCheckFailure(items, area, server, exception);
			}
		}

		private static void AddCheckFailure(
			ICollection<SynixHealthItem> items,
			string area,
			GameServer server,
			Exception exception)
		{
			items.Add(new SynixHealthItem(
				SynixHealthLevel.Warning,
				area,
				server.ServerName,
				$"This check could not finish: {exception.Message}",
				SynixHealthAction.None,
				server));
		}
	}

	internal sealed record FirewallSnapshot(
		bool InspectionSucceeded,
		bool Enabled,
		IReadOnlySet<string> AllowedExecutables,
		IReadOnlySet<string> ProgramExecutables,
		string? Problem);

	internal static class WindowsFirewallInspector
	{
		internal static string? FindAllowedExecutable(
			string installPath,
			string configuredLaunchPath,
			IEnumerable<string> allowedExecutables)
		{
			if (!TryNormalizePath(installPath, out string normalizedInstallPath))
				return null;

			TryNormalizePath(configuredLaunchPath, out string normalizedLaunchPath);
			List<string> normalizedRules = [];
			foreach (string allowedExecutable in allowedExecutables)
			{
				if (TryNormalizePath(allowedExecutable, out string normalizedRule) &&
					normalizedRule.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
				{
					normalizedRules.Add(normalizedRule);
				}
			}

			string? exactMatch = normalizedRules.FirstOrDefault(path =>
				!string.IsNullOrWhiteSpace(normalizedLaunchPath) &&
				string.Equals(path, normalizedLaunchPath, StringComparison.OrdinalIgnoreCase));
			if (exactMatch != null)
				return exactMatch;

			return normalizedRules.FirstOrDefault(path =>
				IsPathInsideDirectory(path, normalizedInstallPath));
		}

		private static bool TryNormalizePath(string path, out string normalizedPath)
		{
			normalizedPath = string.Empty;
			if (string.IsNullOrWhiteSpace(path))
				return false;

			try
			{
				string expanded = Environment.ExpandEnvironmentVariables(path.Trim().Trim('"'));
				normalizedPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(expanded));
				return true;
			}
			catch
			{
				return false;
			}
		}

		private static bool IsPathInsideDirectory(string path, string directory)
		{
			if (string.Equals(path, directory, StringComparison.OrdinalIgnoreCase))
				return false;

			string directoryPrefix = Path.EndsInDirectorySeparator(directory)
				? directory
				: directory + Path.DirectorySeparatorChar;
			return path.StartsWith(directoryPrefix, StringComparison.OrdinalIgnoreCase);
		}

		internal static FirewallSnapshot Capture()
		{
			if (!OperatingSystem.IsWindows())
				return new(
					false,
					false,
					new HashSet<string>(),
					new HashSet<string>(),
					"Windows Firewall inspection is available only on Windows.");

			object? policy = null;
			object? rules = null;
			try
			{
				Type? policyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
				if (policyType == null)
					return new(false, false, new HashSet<string>(), new HashSet<string>(), "Windows Firewall services are unavailable.");
				policy = Activator.CreateInstance(policyType);
				if (policy == null)
					return new(false, false, new HashSet<string>(), new HashSet<string>(), "Windows Firewall could not be opened.");

				dynamic firewallPolicy = policy;
				int currentProfiles = Convert.ToInt32(firewallPolicy.CurrentProfileTypes);
				bool enabled = false;
				foreach (int profile in new[] { 1, 2, 4 })
				{
					if ((currentProfiles & profile) != 0 &&
						Convert.ToBoolean(firewallPolicy.FirewallEnabled[profile]))
					{
						enabled = true;
					}
				}

				rules = firewallPolicy.Rules;
				HashSet<string> allowed = new(StringComparer.OrdinalIgnoreCase);
				HashSet<string> programExecutables = new(StringComparer.OrdinalIgnoreCase);
				foreach (object ruleObject in (IEnumerable)rules)
				{
					dynamic rule = ruleObject;
					try
					{
						string applicationName = Convert.ToString(rule.ApplicationName) ?? string.Empty;
						if (!TryNormalizePath(applicationName, out string executablePath) ||
							!executablePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
						{
							continue;
						}

						programExecutables.Add(executablePath);
						bool ruleEnabled = Convert.ToBoolean(rule.Enabled);
						int direction = Convert.ToInt32(rule.Direction);
						int action = Convert.ToInt32(rule.Action);
						if (ruleEnabled && direction == 1 && action == 1)
							allowed.Add(executablePath);
					}
					catch
					{
					}
					finally
					{
						if (Marshal.IsComObject(ruleObject))
							Marshal.FinalReleaseComObject(ruleObject);
					}
				}

				return new(true, enabled, allowed, programExecutables, null);
			}
			catch (Exception exception)
			{
				return new(
					false,
					false,
					new HashSet<string>(),
					new HashSet<string>(),
					exception.Message);
			}
			finally
			{
				if (rules != null && Marshal.IsComObject(rules))
					Marshal.FinalReleaseComObject(rules);
				if (policy != null && Marshal.IsComObject(policy))
					Marshal.FinalReleaseComObject(policy);
			}
		}
	}
}
