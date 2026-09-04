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
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Synix_Control_Panel.SynixEngine
{
	public sealed record GameVerificationProjectExportResult(
		string FilePath,
		int GameCount,
		int EvidenceCount);

	internal sealed class GameVerificationProjectDocument
	{
		public int SchemaVersion { get; set; } = 1;
		public string ExportedBySynixVersion { get; set; } = string.Empty;
		public DateTimeOffset ExportedAtUtc { get; set; }
		public List<GameCompatibilityVerification> Games { get; set; } = [];
	}

	public partial class Core
	{
		private const string ProjectVerificationFileName = "game-verification.json";
		private static readonly JsonSerializerOptions _projectVerificationJsonOptions = new()
		{
			PropertyNameCaseInsensitive = false,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = true,
			UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
		};

		private static readonly Lazy<IReadOnlyDictionary<string, GameCompatibilityVerification>>
			_builtInGameCompatibility = new(
				LoadBuiltInGameCompatibility,
				LazyThreadSafetyMode.ExecutionAndPublication);

		public static GameVerificationProjectExportResult
			ExportGameVerificationToProject(string projectDirectory)
		{
			if (IsOfficialRelease)
			{
				throw new InvalidOperationException(
					"Verification evidence can be exported only from a development build.");
			}

			return ExportGameVerificationToProject(
				projectDirectory,
				GameCompatibilityPath);
		}

		internal static GameVerificationProjectExportResult
			ExportGameVerificationToProject(
				string projectDirectory,
				string compatibilityPath)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(projectDirectory);
			ArgumentException.ThrowIfNullOrWhiteSpace(compatibilityPath);

			string projectRoot = Path.GetFullPath(projectDirectory);
			string projectFile = Path.Combine(
				projectRoot,
				"Synix Control Panel.csproj");
			if (!File.Exists(projectFile))
			{
				throw new DirectoryNotFoundException(
					"The selected folder does not contain Synix Control Panel.csproj.");
			}

			string definitionsDirectory = Path.GetFullPath(Path.Combine(
				projectRoot,
				"Database",
				"GameDefinitions"));
			if (!Directory.Exists(definitionsDirectory))
			{
				throw new DirectoryNotFoundException(
					"The project game-definition folder could not be found.");
			}

			string destination = Path.GetFullPath(Path.Combine(
				definitionsDirectory,
				ProjectVerificationFileName));
			if (!destination.StartsWith(
				definitionsDirectory + Path.DirectorySeparatorChar,
				StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException(
					"The verification export path escaped the project game-definition folder.");
			}

			GameCompatibilityVerification[] records =
				GetEffectiveGameVerificationQueue(compatibilityPath)
					.Select(item => item.Verification)
					.Where(HasAnyEvidence)
					.OrderBy(item => item.Game, StringComparer.OrdinalIgnoreCase)
					.ToArray();
			GameVerificationProjectDocument document = new()
			{
				SchemaVersion = 1,
				ExportedBySynixVersion = GetCurrentVersion().ToString(3),
				ExportedAtUtc = DateTimeOffset.UtcNow,
				Games = records.ToList()
			};
			string json = JsonSerializer.Serialize(
				document,
				_projectVerificationJsonOptions) + Environment.NewLine;
			FileHandler.WriteTextAtomically(destination, json);

			return new GameVerificationProjectExportResult(
				destination,
				records.Length,
				records.Sum(CountEvidence));
		}

		private static GameCompatibilityVerification GetBuiltInGameCompatibility(
			string? game)
		{
			string canonicalGame = NormalizeCompatibilityGameName(game);
			if (canonicalGame.Length == 0 ||
				!_builtInGameCompatibility.Value.TryGetValue(
					canonicalGame,
					out GameCompatibilityVerification? verification))
			{
				return new GameCompatibilityVerification { Game = canonicalGame };
			}

			return verification;
		}

		private static IReadOnlyDictionary<string, GameCompatibilityVerification>
			LoadBuiltInGameCompatibility()
		{
			try
			{
				Assembly assembly = typeof(Core).Assembly;
				string? resourceName = assembly.GetManifestResourceNames()
					.SingleOrDefault(name => name.EndsWith(
						".GameDefinitions.game-verification.json",
						StringComparison.OrdinalIgnoreCase));
				if (resourceName == null)
					return new Dictionary<string, GameCompatibilityVerification>();

				using Stream stream = assembly.GetManifestResourceStream(resourceName) ??
					throw new InvalidDataException(
						"The embedded game-verification resource could not be opened.");
				if (stream.Length > 1024 * 1024)
					throw new InvalidDataException("The game-verification resource is too large.");
				using StreamReader reader = new(stream);
				GameVerificationProjectDocument document =
					JsonSerializer.Deserialize<GameVerificationProjectDocument>(
						reader.ReadToEnd(),
						_projectVerificationJsonOptions) ??
					throw new InvalidDataException(
						"The embedded game-verification resource is empty.");
				if (document.SchemaVersion != 1)
					throw new InvalidDataException(
						$"Unsupported game-verification schema {document.SchemaVersion}.");

				Dictionary<string, GameCompatibilityVerification> records =
					new(StringComparer.OrdinalIgnoreCase);
				foreach (GameCompatibilityVerification record in document.Games)
				{
					string canonicalGame = NormalizeCompatibilityGameName(record.Game);
					if (canonicalGame.Length == 0 || !HasAnyEvidence(record))
						continue;
					if (!records.TryAdd(canonicalGame, record))
						throw new InvalidDataException(
							$"Duplicate project verification record for {canonicalGame}.");
					record.Game = canonicalGame;
				}
				return records;
			}
			catch (Exception exception) when (exception is IOException or
				JsonException or
				InvalidDataException or
				NotSupportedException)
			{
				System.Diagnostics.Debug.WriteLine(
					$"[PROJECT VERIFICATION LOAD] {exception.Message}");
				return new Dictionary<string, GameCompatibilityVerification>();
			}
		}

		private static bool HasAnyEvidence(GameCompatibilityVerification verification)
		{
			return CountEvidence(verification) > 0;
		}

		private static int CountEvidence(GameCompatibilityVerification verification)
		{
			return (verification.Install != null ? 1 : 0) +
				(verification.Start != null ? 1 : 0) +
				(verification.Stop != null ? 1 : 0) +
				(verification.Monitoring != null ? 1 : 0) +
				(verification.Arguments != null ? 1 : 0) +
				(verification.Configuration != null ? 1 : 0);
		}
	}
}
