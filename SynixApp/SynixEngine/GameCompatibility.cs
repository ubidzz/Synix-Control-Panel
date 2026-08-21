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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Synix_Control_Panel.SynixEngine
{
	public enum GameVerificationKind
	{
		Install,
		Start,
		Stop,
		Monitoring
	}

	public sealed record GameVerificationEvidence(
		string SynixVersion,
		DateTimeOffset VerifiedAtUtc);

	public sealed class GameCompatibilityVerification
	{
		public string Game { get; set; } = string.Empty;
		public GameVerificationEvidence? Install { get; set; }
		public GameVerificationEvidence? Start { get; set; }
		public GameVerificationEvidence? Stop { get; set; }
		public GameVerificationEvidence? Monitoring { get; set; }

		[JsonIgnore]
		public GameVerificationEvidence? LastTested =>
			new[] { Install, Start, Stop, Monitoring }
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
	}
}
