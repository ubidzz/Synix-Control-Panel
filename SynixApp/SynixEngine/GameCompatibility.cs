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
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using Synix_Control_Panel.SynixApp.ServerHandler;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Synix_Control_Panel.SynixEngine
{
	public enum GameVerificationKind
	{
		Install,
		Start,
		Stop,
		Monitoring,
		Arguments,
		Configuration
	}

	public sealed record GameVerificationEvidence(
		string SynixVersion,
		DateTimeOffset VerifiedAtUtc);

	public sealed record GameCompatibilitySummary(
		GameCompatibilityStatus Status,
		string DisplayName,
		GameCompatibilityVerification Verification);

	public sealed record GameVerificationQueueItem(
		string Game,
		ConfigFileCreationMode ConfigurationMode,
		GameCompatibilityVerification Verification)
	{
		public bool ConfigurationApplicable =>
			ConfigurationMode != ConfigFileCreationMode.LaunchArgumentsOnly;

		public int RequiredSteps => ConfigurationApplicable ? 6 : 5;

		public int CompletedSteps =>
			(Verification.Install != null ? 1 : 0) +
			(Verification.Start != null ? 1 : 0) +
			(Verification.Stop != null ? 1 : 0) +
			(Verification.Monitoring != null ? 1 : 0) +
			(Verification.Arguments != null ? 1 : 0) +
			(ConfigurationApplicable && Verification.Configuration != null ? 1 : 0);

		public bool HasKnownConfigurationBehavior =>
			ConfigurationMode != ConfigFileCreationMode.Unknown;

		public bool IsFullyVerified =>
			HasKnownConfigurationBehavior && CompletedSteps == RequiredSteps;
	}

	public sealed class GameCompatibilityVerification
	{
		public string Game { get; set; } = string.Empty;
		public GameVerificationEvidence? Install { get; set; }
		public GameVerificationEvidence? Start { get; set; }
		public GameVerificationEvidence? Stop { get; set; }
		public GameVerificationEvidence? Monitoring { get; set; }
		public GameVerificationEvidence? Arguments { get; set; }
		public GameVerificationEvidence? Configuration { get; set; }

		[JsonIgnore]
		public GameVerificationEvidence? LastTested =>
			new[] { Install, Start, Stop, Monitoring, Arguments, Configuration }
				.Where(evidence => evidence != null)
				.Cast<GameVerificationEvidence>()
				.OrderByDescending(evidence => ParseVersion(evidence.SynixVersion))
				.ThenByDescending(evidence => evidence.VerifiedAtUtc)
				.FirstOrDefault();

		private static Version ParseVersion(string value)
		{
			return Version.TryParse(value, out Version? version)
				? version
				: new Version(0, 0, 0);
		}
	}

	public partial class Core
	{
		private static readonly object _gameCompatibilityLock = new();
		private static readonly JsonSerializerOptions _gameCompatibilityJsonOptions = new()
		{
			PropertyNameCaseInsensitive = true,
			WriteIndented = true
		};

		private static string GameCompatibilityPath =>
			Path.Combine(DataPath, "game-compatibility.json");

		public static GameCompatibilityVerification GetGameCompatibility(string? game)
		{
			return GetGameCompatibility(game, GameCompatibilityPath);
		}

		public static GameCompatibilitySummary GetGameCompatibilitySummary(string? game)
		{
			return GetGameCompatibilitySummary(game, GameCompatibilityPath);
		}

		public static IReadOnlyList<GameVerificationQueueItem> GetGameVerificationQueue()
		{
			return GetGameVerificationQueue(GameCompatibilityPath);
		}

		internal static GameCompatibilitySummary GetGameCompatibilitySummary(
			string? game,
			string compatibilityPath)
		{
			GameCompatibilityVerification verification = GetGameCompatibility(
				game,
				compatibilityPath);
			bool install = verification.Install != null;
			bool start = verification.Start != null;
			bool stop = verification.Stop != null;
			bool monitoring = verification.Monitoring != null;
			GameCompatibilityStatus status;

			if (install && start && stop && monitoring)
			{
				status = GameCompatibilityStatus.FullyVerified;
			}
			else if (GameFix.GetConfigFileCreationMode(game ?? string.Empty) ==
				ConfigFileCreationMode.Unknown)
			{
				status = GameCompatibilityStatus.NeedsConfigurationTemplate;
			}
			else if (install && !start && !stop && !monitoring)
			{
				status = GameCompatibilityStatus.InstallationVerifiedOnly;
			}
			else if (install || start || stop || monitoring)
			{
				status = GameCompatibilityStatus.PartiallyVerified;
			}
			else
			{
				status = GameCompatibilityStatus.NeedsCommunityTesting;
			}

			return new GameCompatibilitySummary(
				status,
				GetCompatibilityStatusDisplayName(status),
				verification);
		}

		public static string GetCompatibilityStatusDisplayName(
			GameCompatibilityStatus status)
		{
			return status switch
			{
				GameCompatibilityStatus.FullyVerified => "Fully verified",
				GameCompatibilityStatus.PartiallyVerified => "Partially verified",
				GameCompatibilityStatus.InstallationVerifiedOnly => "Install verified",
				GameCompatibilityStatus.NeedsConfigurationTemplate => "Needs configuration template",
				_ => "Needs community testing"
			};
		}

		public static bool RecordGameVerification(
			string? game,
			GameVerificationKind verificationKind)
		{
			return RecordGameVerification(
				game,
				verificationKind,
				GetCurrentVersion().ToString(3),
				DateTimeOffset.UtcNow,
				GameCompatibilityPath);
		}

		public static bool ClearGameVerification(
			string? game,
			GameVerificationKind verificationKind)
		{
			return ClearGameVerification(
				game,
				verificationKind,
				GameCompatibilityPath);
		}

		internal static IReadOnlyList<GameVerificationQueueItem>
			GetGameVerificationQueue(string compatibilityPath)
		{
			lock (_gameCompatibilityLock)
			{
				List<GameCompatibilityVerification> records =
					LoadGameCompatibilityRecords(compatibilityPath);
				Dictionary<string, GameCompatibilityVerification> recordsByGame =
					records
						.GroupBy(
							record => NormalizeCompatibilityGameName(record.Game),
							StringComparer.OrdinalIgnoreCase)
						.ToDictionary(
							group => group.Key,
							group => group.First(),
							StringComparer.OrdinalIgnoreCase);

				return GameDatabase.GetGameList()
					.OrderBy(game => game.CatalogOrder)
					.ThenBy(game => game.Game, StringComparer.OrdinalIgnoreCase)
					.Select(game =>
					{
						if (!recordsByGame.TryGetValue(
							game.Game,
							out GameCompatibilityVerification? verification))
						{
							verification = new GameCompatibilityVerification
							{
								Game = game.Game
							};
						}

						return new GameVerificationQueueItem(
							game.Game,
							game.ConfigFileCreation,
							verification);
					})
					.ToArray();
			}
		}

		internal static GameCompatibilityVerification GetGameCompatibility(
			string? game,
			string compatibilityPath)
		{
			string canonicalGame = NormalizeCompatibilityGameName(game);
			if (canonicalGame.Length == 0)
				return new GameCompatibilityVerification();

			lock (_gameCompatibilityLock)
			{
				List<GameCompatibilityVerification> records = LoadGameCompatibilityRecords(
					compatibilityPath);
				return records.FirstOrDefault(record => string.Equals(
					record.Game,
					canonicalGame,
					StringComparison.OrdinalIgnoreCase)) ??
					new GameCompatibilityVerification { Game = canonicalGame };
			}
		}

		internal static bool RecordGameVerification(
			string? game,
			GameVerificationKind verificationKind,
			string synixVersion,
			DateTimeOffset verifiedAtUtc,
			string compatibilityPath)
		{
			string canonicalGame = NormalizeCompatibilityGameName(game);
			if (canonicalGame.Length == 0 ||
				string.IsNullOrWhiteSpace(synixVersion) ||
				string.IsNullOrWhiteSpace(compatibilityPath))
			{
				return false;
			}

			lock (_gameCompatibilityLock)
			{
				try
				{
					List<GameCompatibilityVerification> records = LoadGameCompatibilityRecords(
						compatibilityPath);
					GameCompatibilityVerification? record = records.FirstOrDefault(candidate =>
						string.Equals(
							candidate.Game,
							canonicalGame,
							StringComparison.OrdinalIgnoreCase));

					if (record == null)
					{
						record = new GameCompatibilityVerification { Game = canonicalGame };
						records.Add(record);
					}

					GameVerificationEvidence? currentEvidence = verificationKind switch
					{
						GameVerificationKind.Install => record.Install,
						GameVerificationKind.Start => record.Start,
						GameVerificationKind.Stop => record.Stop,
						GameVerificationKind.Monitoring => record.Monitoring,
						GameVerificationKind.Arguments => record.Arguments,
						GameVerificationKind.Configuration => record.Configuration,
						_ => null
					};

					if (!ShouldReplaceEvidence(currentEvidence, synixVersion))
						return false;

					GameVerificationEvidence newEvidence = new(
						synixVersion.Trim(),
						verifiedAtUtc.ToUniversalTime());

					switch (verificationKind)
					{
						case GameVerificationKind.Install:
							record.Install = newEvidence;
							break;
						case GameVerificationKind.Start:
							record.Start = newEvidence;
							break;
						case GameVerificationKind.Stop:
							record.Stop = newEvidence;
							break;
						case GameVerificationKind.Monitoring:
							record.Monitoring = newEvidence;
							break;
						case GameVerificationKind.Arguments:
							record.Arguments = newEvidence;
							break;
						case GameVerificationKind.Configuration:
							record.Configuration = newEvidence;
							break;
					}

					records = records
						.OrderBy(candidate => candidate.Game, StringComparer.OrdinalIgnoreCase)
						.ToList();
					string json = JsonSerializer.Serialize(records, _gameCompatibilityJsonOptions);
					FileHandler.WriteTextAtomically(compatibilityPath, json);
					return true;
				}
				catch (Exception exception) when (exception is IOException or
					UnauthorizedAccessException or
					JsonException or
					NotSupportedException)
				{
					System.Diagnostics.Debug.WriteLine(
						$"[COMPATIBILITY RECORD] {exception.Message}");
					return false;
				}
			}
		}

		internal static bool ClearGameVerification(
			string? game,
			GameVerificationKind verificationKind,
			string compatibilityPath)
		{
			string canonicalGame = NormalizeCompatibilityGameName(game);
			if (canonicalGame.Length == 0 ||
				string.IsNullOrWhiteSpace(compatibilityPath))
			{
				return false;
			}

			lock (_gameCompatibilityLock)
			{
				try
				{
					List<GameCompatibilityVerification> records =
						LoadGameCompatibilityRecords(compatibilityPath);
					GameCompatibilityVerification? record = records.FirstOrDefault(
						candidate => string.Equals(
							candidate.Game,
							canonicalGame,
							StringComparison.OrdinalIgnoreCase));
					if (record == null || !HasVerification(record, verificationKind))
						return false;

					switch (verificationKind)
					{
						case GameVerificationKind.Install:
							record.Install = null;
							break;
						case GameVerificationKind.Start:
							record.Start = null;
							break;
						case GameVerificationKind.Stop:
							record.Stop = null;
							break;
						case GameVerificationKind.Monitoring:
							record.Monitoring = null;
							break;
						case GameVerificationKind.Arguments:
							record.Arguments = null;
							break;
						case GameVerificationKind.Configuration:
							record.Configuration = null;
							break;
					}

					if (!Enum.GetValues<GameVerificationKind>()
						.Any(kind => HasVerification(record, kind)))
					{
						records.Remove(record);
					}

					string json = JsonSerializer.Serialize(
						records.OrderBy(
							candidate => candidate.Game,
							StringComparer.OrdinalIgnoreCase),
						_gameCompatibilityJsonOptions);
					FileHandler.WriteTextAtomically(compatibilityPath, json);
					return true;
				}
				catch (Exception exception) when (exception is IOException or
					UnauthorizedAccessException or
					JsonException or
					NotSupportedException)
				{
					System.Diagnostics.Debug.WriteLine(
						$"[COMPATIBILITY CLEAR] {exception.Message}");
					return false;
				}
			}
		}

		private static List<GameCompatibilityVerification> LoadGameCompatibilityRecords(
			string compatibilityPath)
		{
			try
			{
				if (!File.Exists(compatibilityPath))
					return [];

				string json = File.ReadAllText(compatibilityPath);
				return JsonSerializer.Deserialize<List<GameCompatibilityVerification>>(
					json,
					_gameCompatibilityJsonOptions) ?? [];
			}
			catch (Exception exception) when (exception is IOException or
				UnauthorizedAccessException or
				JsonException or
				NotSupportedException)
			{
				System.Diagnostics.Debug.WriteLine(
					$"[COMPATIBILITY LOAD] {exception.Message}");
				return [];
			}
		}

		private static string NormalizeCompatibilityGameName(string? game)
		{
			string canonicalGame = GameDatabase.GetCanonicalGameName(game);
			return canonicalGame.Length > 0
				? canonicalGame
				: game?.Trim() ?? string.Empty;
		}

		private static bool ShouldReplaceEvidence(
			GameVerificationEvidence? currentEvidence,
			string synixVersion)
		{
			if (currentEvidence == null)
				return true;

			if (!Version.TryParse(synixVersion, out Version? newVersion))
				return !string.Equals(
					currentEvidence.SynixVersion,
					synixVersion,
					StringComparison.OrdinalIgnoreCase);

			if (!Version.TryParse(currentEvidence.SynixVersion, out Version? currentVersion))
				return true;

			return newVersion > currentVersion;
		}

		private static bool HasVerification(
			GameCompatibilityVerification verification,
			GameVerificationKind verificationKind)
		{
			return verificationKind switch
			{
				GameVerificationKind.Install => verification.Install != null,
				GameVerificationKind.Start => verification.Start != null,
				GameVerificationKind.Stop => verification.Stop != null,
				GameVerificationKind.Monitoring => verification.Monitoring != null,
				GameVerificationKind.Arguments => verification.Arguments != null,
				GameVerificationKind.Configuration => verification.Configuration != null,
				_ => false
			};
		}
	}
}
