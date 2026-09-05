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
using System.Text;
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
		private const int MaximumReadinessTailBytes = 512 * 1024;

		internal static bool HasDeclaredLogs(string? game) =>
			GameDatabase.GetGame(game ?? string.Empty)?.LogPaths.Count > 0;

		internal static bool HasDetectedLogs(GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);
			try
			{
				return FindLatest(server).Found;
			}
			catch (Exception exception) when (exception is IOException or
				UnauthorizedAccessException or InvalidDataException or
				ArgumentException or NotSupportedException or
				System.Security.SecurityException)
			{
				ApplicationLogService.WriteSuppressedException(exception);
				return false;
			}
		}

		internal static GameLogDiscoveryResult FindLatest(GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);
			GameInfo? game = GameDatabase.GetGame(server.Game);
			if (game == null || game.LogPaths.Count == 0)
				return new(null, 0, [LocalizationManager.Get(
					"GameLogs.Error.NoDeclaredLocations")]);
			if (string.IsNullOrWhiteSpace(server.InstallPath) ||
				!Directory.Exists(server.InstallPath))
				return new(null, 0, [LocalizationManager.Get(
					"GameLogs.Error.ServerFolderMissing")]);

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

		internal static bool ContainsCurrentStartupText(
			GameServer server,
			string expectedText)
		{
			ArgumentNullException.ThrowIfNull(server);
			if (string.IsNullOrWhiteSpace(expectedText) || !server.StartTime.HasValue)
				return false;

			try
			{
				GameLogDiscoveryResult result = FindLatest(server);
				if (!result.Found)
					return false;

				FileInfo log = new(result.LatestLogPath!);
				DateTime startupUtc = server.StartTime.Value.ToUniversalTime();
				if (!log.Exists || log.LastWriteTimeUtc < startupUtc.AddSeconds(-2))
					return false;

				using FileStream stream = new(
					log.FullName,
					FileMode.Open,
					FileAccess.Read,
					FileShare.ReadWrite | FileShare.Delete);
				long tailStart = Math.Max(0, stream.Length - MaximumReadinessTailBytes);
				stream.Seek(tailStart, SeekOrigin.Begin);
				using StreamReader reader = new(
					stream,
					Encoding.UTF8,
					detectEncodingFromByteOrderMarks: true,
					bufferSize: 4096,
					leaveOpen: false);
				if (tailStart > 0)
					_ = reader.ReadLine();

				return reader.ReadToEnd().Contains(
					expectedText,
					StringComparison.OrdinalIgnoreCase);
			}
			catch (Exception exception) when (exception is IOException or
				UnauthorizedAccessException or InvalidDataException or
				ArgumentException or NotSupportedException or
				System.Security.SecurityException)
			{
				ApplicationLogService.WriteSuppressedException(exception);
				return false;
			}
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
				throw new InvalidDataException(LocalizationManager.Get(
					"GameLogs.Error.PatternUnsafe"));

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
					throw new InvalidDataException(LocalizationManager.Get(
						"GameLogs.Error.SearchLimit"));
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
				throw new InvalidDataException(LocalizationManager.Get(
					"GameLogs.Error.PathOutsideServer"));
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
				ApplicationLogService.WriteLocalized(
					"GameLogs.Activity.NotFound",
					Color.Orange,
					true,
					server.ServerName,
					details);
				return;
			}

			try
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = result.LatestLogPath!,
					UseShellExecute = true
				});
				ApplicationLogService.WriteLocalized(
					"GameLogs.Activity.Opened",
					Color.Cyan,
					false,
					server.ServerName,
					result.LatestLogPath);
			}
			catch (Exception exception) when (exception is InvalidOperationException or
				System.ComponentModel.Win32Exception)
			{
				ApplicationLogService.WriteLocalized(
					"GameLogs.Activity.OpenFailed",
					Color.OrangeRed,
					true,
					server.ServerName,
					exception.Message);
			}
		}
	}
}
