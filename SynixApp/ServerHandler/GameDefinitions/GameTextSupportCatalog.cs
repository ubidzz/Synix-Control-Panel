using System.Text.Json;
using System.Text.Json.Serialization;
using Synix_Control_Panel.SynixApp.Database;

namespace Synix_Control_Panel.SynixApp.ServerHandler;

// Documentation evidence, not a live-server certification or an ASCII allowlist.
internal enum GameTextSupport { Unverified, Documented, Conditional, Restricted, NotApplicable }

internal sealed class GameTextFieldSupport
{
	public GameTextSupport Status { get; init; }
	public string Notes { get; init; } = "";
	public IReadOnlyList<string> Sources { get; init; } = [];
	[JsonIgnore] public string StatusText => LocalizationManager.Get("GameText.Status." + Status);
}

internal sealed class GameTextSupportEntry
{
	public string GameId { get; init; } = "";
	public DateOnly SearchedOn { get; init; }
	public string SearchQuery { get; init; } = "";
	public GameTextFieldSupport ServerTitle { get; init; } = new();
	public GameTextFieldSupport JoinPassword { get; init; } = new();
	public GameTextFieldSupport AdminPassword { get; init; } = new();
	public GameTextFieldSupport RconPassword { get; init; } = new();
	[JsonIgnore] public IEnumerable<GameTextFieldSupport> Fields =>
		[ServerTitle, JoinPassword, AdminPassword, RconPassword];
}

internal sealed class GameTextSupportDocument
{
	public int SchemaVersion { get; init; }
	public string Scope { get; init; } = "";
	public List<GameTextSupportEntry> Entries { get; init; } = [];
}

internal static class GameTextSupportCatalog
{
	private static readonly Lazy<IReadOnlyDictionary<string, GameTextSupportEntry>> Cache = new(Load);
	internal static IReadOnlyCollection<GameTextSupportEntry> Entries => Cache.Value.Values.ToArray();

	internal static GameTextSupportEntry? ForGame(string? gameName)
	{
		string? id = string.IsNullOrWhiteSpace(gameName) ? null : GameDatabase.GetGame(gameName)?.DefinitionId;
		return id != null && Cache.Value.TryGetValue(id, out var entry) ? entry : null;
	}

	internal static string TitleSummary(string? gameName) => LocalizationManager.Get("GameText.TitleSummary",
		ForGame(gameName)?.ServerTitle.StatusText ?? LocalizationManager.Get("GameText.Status.Unverified"));

	internal static string HelpText(string gameName)
	{
		GameTextSupportEntry? entry = ForGame(gameName);
		List<string> paragraphs = [gameName, LocalizationManager.Get("GameText.Introduction")];
		AddField("ServerTitle", entry?.ServerTitle);
		AddField("JoinPassword", entry?.JoinPassword);
		AddField("AdminPassword", entry?.AdminPassword);
		AddField("RconPassword", entry?.RconPassword);
		paragraphs.Add(LocalizationManager.Get("GameText.SafetyNote"));
		if (entry != null)
		{
			paragraphs.Add(LocalizationManager.Get("GameText.SearchedOn", entry.SearchedOn.ToString("yyyy-MM-dd")));
			paragraphs.Add(LocalizationManager.Get("GameText.SearchQuery") + Environment.NewLine + entry.SearchQuery);
		}
		return string.Join(Environment.NewLine + Environment.NewLine, paragraphs);

		void AddField(string field, GameTextFieldSupport? support)
		{
			support ??= new();
			paragraphs.Add(LocalizationManager.Get("GameText.Field." + field) + ": " + support.StatusText +
				Environment.NewLine + (support.Notes.Length == 0 ? LocalizationManager.Get("GameText.UnverifiedNote") : support.Notes));
			if (support.Sources.Count > 0)
				paragraphs.Add(LocalizationManager.Get("GameText.Sources") + Environment.NewLine + string.Join(Environment.NewLine, support.Sources));
		}
	}

	internal static GameTextSupportDocument Parse(string json)
	{
		if (string.IsNullOrWhiteSpace(json) || json.Length > 1024 * 1024) throw Invalid();
		GameTextSupportDocument document;
		try
		{
			document = JsonSerializer.Deserialize<GameTextSupportDocument>(json, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
				Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
			}) ?? throw Invalid();
		}
		catch (JsonException exception) { throw new InvalidDataException(LocalizationManager.Get("GameText.InvalidCatalog"), exception); }
		if (document.SchemaVersion != 1 || string.IsNullOrWhiteSpace(document.Scope) ||
			document.Entries == null || document.Entries.Count is 0 or > 2048) throw Invalid();
		HashSet<string> ids = new(StringComparer.OrdinalIgnoreCase);
		HashSet<string> known = GameDatabase.GetGames.Select(game => game.DefinitionId).ToHashSet(StringComparer.Ordinal);
		foreach (var entry in document.Entries)
		{
			if (entry == null || !known.Contains(entry.GameId) || !ids.Add(entry.GameId) || entry.SearchedOn == default ||
				string.IsNullOrWhiteSpace(entry.SearchQuery) || entry.SearchQuery.Length > 512) throw Invalid();
			foreach (var field in entry.Fields)
			{
				if (field == null || !Enum.IsDefined(field.Status) || field.Notes == null || field.Notes.Length > 4000 ||
					field.Sources == null || field.Sources.Count > 8 || field.Sources.Any(source => !IsSafeSource(source)) ||
					field.Status != GameTextSupport.Unverified && (string.IsNullOrWhiteSpace(field.Notes) || field.Sources.Count == 0)) throw Invalid();
			}
		}
		return document;
	}

	internal static bool IsSafeSource(string? source) => source?.Length <= 2048 &&
		Uri.TryCreate(source, UriKind.Absolute, out Uri? uri) && uri.Scheme == Uri.UriSchemeHttps &&
		uri.IsDefaultPort && uri.UserInfo.Length == 0 && !uri.IsLoopback;

	private static IReadOnlyDictionary<string, GameTextSupportEntry> Load()
	{
		var assembly = typeof(GameTextSupportCatalog).Assembly;
		string name = assembly.GetManifestResourceNames().Single(name => name.EndsWith(".game-text-support.json", StringComparison.Ordinal));
		using Stream stream = assembly.GetManifestResourceStream(name) ?? throw Invalid();
		using StreamReader reader = new(stream);
		return Parse(reader.ReadToEnd()).Entries.ToDictionary(entry => entry.GameId, StringComparer.OrdinalIgnoreCase);
	}

	private static InvalidDataException Invalid() => new(LocalizationManager.Get("GameText.InvalidCatalog"));
}
