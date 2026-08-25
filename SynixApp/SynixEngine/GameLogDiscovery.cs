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
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Synix_Control_Panel.SynixEngine
{
	internal sealed record GameLogDiscoveryResult(
		string? LatestLogPath,
		int MatchedFiles,
		IReadOnlyList<string> Errors)
	{
		public bool Found => !string.IsNullOrWhiteSpace(LatestLogPath);
	}

	internal static class GameLogDiscovery
	{
		private const int MaximumFilesExamined = 50_000;

		internal static bool HasDeclaredLogs(string? game) =>
			GameDatabase.GetGame(game ?? string.Empty)?.LogPaths.Count > 0;

		internal static GameLogDiscoveryResult FindLatest(GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);
			GameInfo? game = GameDatabase.GetGame(server.Game);
			if (game == null || game.LogPaths.Count == 0)
				return new(null, 0, ["This game definition does not declare any log locations yet."]);
			if (string.IsNullOrWhiteSpace(server.InstallPath) ||
				!Directory.Exists(server.InstallPath))
				return new(null, 0, ["The installed server folder could not be found."]);

			string root = Path.GetFullPath(server.InstallPath)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			List<string> matches = [];
			List<string> errors = [];
			foreach (string declaredPattern in game.LogPaths)
			{
				try
				{
					FindMatches(root, ExpandPattern(declaredPattern, server), matches);
				}
				catch (Exception exception) when (exception is IOException or
					UnauthorizedAccessException or InvalidDataException)
				{
					errors.Add($"{declaredPattern}: {exception.Message}");
				}
			}

			string? latest = matches
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.Select(path => new FileInfo(path))
				.Where(file => file.Exists)
				.OrderByDescending(file => file.LastWriteTimeUtc)
				.ThenBy(file => file.FullName, StringComparer.OrdinalIgnoreCase)
				.Select(file => file.FullName)
				.FirstOrDefault();
			return new(latest, matches.Count, errors);
		}

		private static string ExpandPattern(string pattern, GameServer server)
		{
			string identity = Core.Instance.GetSafeName(server.ServerName);
			string worldName = Core.Instance.GetSafeName(server.WorldName ?? string.Empty);
			return pattern
				.Replace("{Identity}", identity, StringComparison.Ordinal)
				.Replace("{ServerName}", identity, StringComparison.Ordinal)
				.Replace("{WorldName}", worldName, StringComparison.Ordinal)
				.Replace("{Port}", server.Port.ToString(), StringComparison.Ordinal)
				.Replace("{QueryPort}", server.QueryPort.ToString(), StringComparison.Ordinal)
				.Replace('\\', '/');
		}

		private static void FindMatches(
			string root,
			string relativePattern,
			List<string> matches)
		{
			if (Path.IsPathRooted(relativePattern) ||
				relativePattern.Contains(':') ||
				relativePattern.Split('/', StringSplitOptions.RemoveEmptyEntries)
					.Any(segment => segment is "." or ".."))
				throw new InvalidDataException("The log pattern is unsafe.");

			int wildcardIndex = relativePattern.IndexOfAny(['*', '?']);
			if (wildcardIndex < 0)
			{
				string exactPath = ResolveInside(root, relativePattern);
				if (File.Exists(exactPath))
					matches.Add(exactPath);
				return;
			}

			int separatorIndex = relativePattern.LastIndexOf('/', wildcardIndex);
			string searchRootRelative = separatorIndex < 0
				? string.Empty
				: relativePattern[..separatorIndex];
			string searchRoot = searchRootRelative.Length == 0
				? root
				: ResolveInside(root, searchRootRelative);
			if (!Directory.Exists(searchRoot))
				return;

			Regex matcher = CreateMatcher(relativePattern);
			int examined = 0;
			foreach (string file in Directory.EnumerateFiles(
				searchRoot,
				"*",
				SearchOption.AllDirectories))
			{
				if (++examined > MaximumFilesExamined)
					throw new InvalidDataException("The log search exceeded the safe file limit.");
				string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
				if (matcher.IsMatch(relative))
					matches.Add(file);
			}
		}

		private static Regex CreateMatcher(string pattern)
		{
			string expression = Regex.Escape(pattern)
				.Replace(@"\*\*", "__SYNIX_RECURSIVE__", StringComparison.Ordinal)
				.Replace(@"\*", "[^/]*", StringComparison.Ordinal)
				.Replace(@"\?", "[^/]", StringComparison.Ordinal)
				.Replace("__SYNIX_RECURSIVE__", ".*", StringComparison.Ordinal);
			return new Regex(
				"^" + expression + "$",
				RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
				TimeSpan.FromSeconds(2));
		}

		private static string ResolveInside(string root, string relativePath)
		{
			string path = Path.GetFullPath(Path.Combine(
				root,
				relativePath.Replace('/', Path.DirectorySeparatorChar)));
			if (!path.StartsWith(
				root + Path.DirectorySeparatorChar,
				StringComparison.OrdinalIgnoreCase))
				throw new InvalidDataException("The log path leaves the installed server folder.");
			return path;
		}
	}

	public partial class Core
	{
		public void OpenLatestGameLog(GameServer server)
		{
			GameLogDiscoveryResult result = GameLogDiscovery.FindLatest(server);
			if (!result.Found)
			{
				string details = result.Errors.Count > 0
					? " " + string.Join(" ", result.Errors.Take(2))
					: string.Empty;
				Log($"[LOGS] No declared game log was found for {server.ServerName}.{details}", Color.Orange, true);
				return;
			}

			try
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = result.LatestLogPath!,
					UseShellExecute = true
				});
				Log($"[LOGS] Opened the newest declared game log for {server.ServerName}: {result.LatestLogPath}", Color.Cyan);
			}
			catch (Exception exception) when (exception is InvalidOperationException or
				System.ComponentModel.Win32Exception)
			{
				Log($"[LOGS] Windows could not open the newest log for {server.ServerName}: {exception.Message}", Color.OrangeRed, true);
			}
		}
	}
}
