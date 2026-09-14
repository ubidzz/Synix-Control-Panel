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
using System.Text.Json;
using System.Text.Json.Serialization;
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.ServerHandler;

namespace Synix_Control_Panel.SynixEngine.ModManagement;

internal enum GameModEvidence { NeedsVerification, Documented }

internal sealed class GameModSupportEntry
{
	public string GameId { get; init; } = "";
	public GameModEvidence Status { get; init; }
	public DateOnly ReviewedOn { get; init; }
	public IReadOnlyList<string> Methods { get; init; } = [];
	public string Notes { get; init; } = "";
	public IReadOnlyList<string> Sources { get; init; } = [];
	public string StatusText => LocalizationManager.Get("ModSupport." + Status);
}

internal sealed class GameModSupportDocument
{
	public int SchemaVersion { get; init; }
	public string Scope { get; init; } = "";
	public List<GameModSupportEntry> Entries { get; init; } = [];
}

// Evidence of a game's extension system is deliberately separate from executable
// import rules and live-server verification. A missing rule never means "no mods".
internal static class GameModSupportCatalog
{
	private static readonly Lazy<IReadOnlyDictionary<string, GameModSupportEntry>> Cache = new(Load);
	internal static IReadOnlyCollection<GameModSupportEntry> Entries => Cache.Value.Values.ToArray();

	internal static GameModSupportEntry? ForGame(string gameName)
	{
		string? id = GameDatabase.GetGame(gameName)?.DefinitionId;
		return id != null && Cache.Value.TryGetValue(id, out GameModSupportEntry? entry) ? entry : null;
	}

	internal static string StatusText(string gameName) => ForGame(gameName)?.StatusText ??
		LocalizationManager.Get("ModSupport.NeedsVerification");

	internal static string HelpText(string gameName)
	{
		GameModSupportEntry? entry = ForGame(gameName);
		bool managed = !MinecraftControlProfile.IsBedrock(new() { Game = gameName }) &&
			ModSystemCatalog.GetProfiles(gameName).Any(profile => profile.CanManage);
		List<string> paragraphs =
		[
			gameName + " — " + StatusText(gameName),
			LocalizationManager.Get(managed ? "ModSupport.ImportRules" : "ModSupport.ManualSetup"),
			LocalizationManager.Get("ModSupport.EvidenceNote")
		];
		if (entry?.Methods.Count > 0)
			paragraphs.Add(LocalizationManager.Get("ModSupport.Methods") + Environment.NewLine +
				string.Join(Environment.NewLine, entry.Methods.Select(method => "• " + method)));
		paragraphs.Add(entry?.Status == GameModEvidence.Documented ? entry.Notes :
			LocalizationManager.Get("ModSupport.UnverifiedNote"));
		if (entry != null)
			paragraphs.Add(LocalizationManager.Get("ModSupport.Reviewed", entry.ReviewedOn.ToString("yyyy-MM-dd")));
		if (entry?.Sources.Count > 0)
			paragraphs.Add(LocalizationManager.Get("ModSupport.Sources") + Environment.NewLine +
				string.Join(Environment.NewLine, entry.Sources));
		return string.Join(Environment.NewLine + Environment.NewLine, paragraphs);
	}

	internal static GameModSupportDocument Parse(string json)
	{
		if (string.IsNullOrWhiteSpace(json) || json.Length > 512 * 1024) throw Invalid();
		GameModSupportDocument document;
		try
		{
			document = JsonSerializer.Deserialize<GameModSupportDocument>(json, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
				Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
			}) ?? throw Invalid();
		}
		catch (JsonException exception) { throw new InvalidDataException(LocalizationManager.Get("ModSupport.InvalidCatalog"), exception); }
		if (document.SchemaVersion != 1 || string.IsNullOrWhiteSpace(document.Scope) ||
			document.Entries == null || document.Entries.Count is 0 or > 2048) throw Invalid();
		HashSet<string> ids = new(StringComparer.OrdinalIgnoreCase);
		HashSet<string> known = GameDatabase.GetGames.Select(game => game.DefinitionId).ToHashSet(StringComparer.Ordinal);
		foreach (GameModSupportEntry entry in document.Entries)
		{
			if (entry == null || !Enum.IsDefined(entry.Status) || !known.Contains(entry.GameId) || !ids.Add(entry.GameId) ||
				entry.ReviewedOn == default || string.IsNullOrWhiteSpace(entry.Notes) || entry.Notes.Length > 4000 ||
				entry.Methods == null || entry.Sources == null || entry.Methods.Count > 16 || entry.Sources.Count > 16 ||
				entry.Methods.Any(method => string.IsNullOrWhiteSpace(method) || method.Length > 160) ||
				entry.Sources.Any(source => !IsSafeSource(source)) ||
				entry.Status == GameModEvidence.Documented && (entry.Methods.Count == 0 || entry.Sources.Count == 0) ||
				entry.Status == GameModEvidence.NeedsVerification && entry.Methods.Count != 0) throw Invalid();
		}
		return document;
	}

	internal static bool IsSafeSource(string? source) => source?.Length <= 2048 &&
		Uri.TryCreate(source, UriKind.Absolute, out Uri? uri) && uri.Scheme == Uri.UriSchemeHttps &&
		uri.IsDefaultPort && uri.UserInfo.Length == 0 && !uri.IsLoopback;

	private static IReadOnlyDictionary<string, GameModSupportEntry> Load()
	{
		var assembly = typeof(GameModSupportCatalog).Assembly;
		string name = assembly.GetManifestResourceNames().Single(name => name.EndsWith(".game-mod-support.modsupport.json", StringComparison.Ordinal));
		using Stream stream = assembly.GetManifestResourceStream(name) ?? throw Invalid();
		using StreamReader reader = new(stream);
		return Parse(reader.ReadToEnd()).Entries.ToDictionary(entry => entry.GameId, StringComparer.OrdinalIgnoreCase);
	}

	private static InvalidDataException Invalid() => new(LocalizationManager.Get("ModSupport.InvalidCatalog"));
}
