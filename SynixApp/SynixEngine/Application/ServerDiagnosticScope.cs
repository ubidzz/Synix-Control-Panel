using System.Globalization;
using System.Text.Json;

namespace Synix_Control_Panel.SynixEngine;

// Only deliberately scoped, translated application diagnostics enter history.
// Never infer a server from the selected grid row, its name, or a log substring.
internal sealed class ServerDiagnosticScope : IDisposable
{
	private static readonly AsyncLocal<GameServer?> ActiveServer = new();
	private readonly GameServer? _previous;
	private bool _disposed;
	private ServerDiagnosticScope(GameServer server)
	{
		_previous = ActiveServer.Value;
		ActiveServer.Value = server;
	}
	internal static ServerDiagnosticScope Begin(GameServer server) => new(server);
	internal static void Record(string resourceKey, Color? color, object?[] arguments)
	{
		if (ActiveServer.Value is GameServer server)
			ServerActivityHistory.Current.RecordDiagnostic(server, resourceKey, color, arguments);
	}
	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;
		ActiveServer.Value = _previous;
	}
}

internal sealed record ServerActivityDiagnostic(string ResourceKey, JsonElement[] Arguments, bool Attention)
{
	internal const int MaximumArguments = 12;
	internal const int MaximumArgumentLength = 1200;

	internal static bool IsAllowedKey(string? key) => key is { Length: <= 160 } &&
		(key.StartsWith("ServerStart.Activity.", StringComparison.Ordinal) ||
		 key.StartsWith("ServerStart.Maintenance.", StringComparison.Ordinal) ||
		 key.StartsWith("ServerStop.Activity.", StringComparison.Ordinal) ||
		 key == "ServerProcess.Activity.Registered" ||
		 key.StartsWith("Minecraft.Activity.StopThrough", StringComparison.Ordinal) ||
		 key == "Minecraft.Activity.ConsoleFallback" ||
		 key.StartsWith("ServerActions.Activity.", StringComparison.Ordinal) ||
		 key.StartsWith("ServerActions.Operation.", StringComparison.Ordinal) ||
		 key.StartsWith("Backup.Activity.", StringComparison.Ordinal) ||
		 key.StartsWith("Backup.Restore.Activity.", StringComparison.Ordinal) ||
		 key.StartsWith("Backup.Retention.", StringComparison.Ordinal) ||
		 key.StartsWith("Watchdog.Activity.", StringComparison.Ordinal) ||
		 key.StartsWith("Status.Activity.", StringComparison.Ordinal) ||
		 key.StartsWith("Probe.Activity.", StringComparison.Ordinal) ||
		 key.StartsWith("Core.Activity.Maintenance", StringComparison.Ordinal) ||
		 key.StartsWith("Dashboard.Activity.Wan", StringComparison.Ordinal) ||
		 key.StartsWith("Dashboard.Activity.Lan", StringComparison.Ordinal)) &&
		key != "ServerStart.Activity.Arguments" && LocalizationManager.GetEnglish(key) != "[" + key + "]";

	internal static bool IsAttention(string key, Color? color) =>
		color == Color.Red || color == Color.OrangeRed || color == Color.Orange || color == Color.Yellow ||
		new[] { "Failed", "Error", "Warning", "Blocked", "Terminated", "Missing", "Unavailable", "Locked", "RequiresStopped",
			"Forcing", "Forced", "Fallback", "ExitCode", "LiveProcessRemains", "RestoreRequired" }
			.Any(word => key.Contains(word, StringComparison.Ordinal));

	internal bool IsValid => IsAllowedKey(ResourceKey) && Arguments is { Length: <= MaximumArguments } &&
		Arguments.All(value => value.ValueKind is JsonValueKind.Null or JsonValueKind.True or JsonValueKind.False or JsonValueKind.Number ||
			value.ValueKind == JsonValueKind.String && value.GetString()!.Length <= MaximumArgumentLength) && CanFormat();

	private bool CanFormat()
	{
		try { _ = LocalizationManager.Get(ResourceKey, Values); return true; }
		catch (FormatException) { return false; }
	}

	internal string Description
	{
		get
		{
			try { return SecretRedactor.Redact(LocalizationManager.Get(ResourceKey, Values)); }
			catch (FormatException) { return LocalizationManager.Get("History.Recovered"); }
		}
	}
	private object?[] Values => Arguments.Select(value => value.ValueKind switch
		{
			JsonValueKind.Number => value.TryGetInt64(out long integer) ? (object)integer : value.GetDouble(),
			JsonValueKind.True => true,
			JsonValueKind.False => false,
			JsonValueKind.Null => null,
			_ => (object?)value.GetString()
		}).ToArray();

	internal static JsonElement[] SanitizeArguments(GameServer server, object?[] arguments)
	{
		// Raw consoles, config documents and commands are intentionally not inputs.
		// Also remove known credentials in free-form error arguments, including
		// values that have no "password=" label. If secrets cannot be unlocked,
		// omit textual arguments instead of guessing what is safe to persist.
		List<string> secrets = [];
		bool unlocked = Core.TryRevealServerSecrets(server, out SynixServerSecrets revealed);
		unlocked &= Core.TryRevealDiscordWebhookRoutes(server, out var routes);
		if (unlocked)
		{
			secrets.AddRange([revealed.Passwords.ServerPassword, revealed.Passwords.AdminPassword,
				revealed.Passwords.RconPassword, revealed.Passwords.AuthenticationToken, revealed.DiscordWebhook]);
			secrets.AddRange(routes.Select(route => route.WebhookUrl));
		}
		secrets.AddRange([server.Password, server.AdminPassword, server.RconPassword, server.AuthenticationToken, server.DiscordWebhook]);
		string[] sensitive = secrets.Where(value => !string.IsNullOrEmpty(value)).Distinct(StringComparer.Ordinal)
			.SelectMany(value => new[] { value, Uri.EscapeDataString(value), JsonSerializer.Serialize(value)[1..^1] })
			.Distinct(StringComparer.Ordinal).OrderByDescending(value => value.Length).ToArray();
		return arguments.Select(argument =>
		{
			if (argument == null) return JsonSerializer.SerializeToElement<object?>(null);
			string text = Convert.ToString(argument, CultureInfo.InvariantCulture) ?? string.Empty;
			if (!unlocked || text.Length > MaximumArgumentLength)
				return JsonSerializer.SerializeToElement(SecretRedactor.Omitted);
			string safe = SecretRedactor.Redact(text);
			// Match all secrets against the original redacted text so replacements
			// cannot be mangled by another short password (for example "s").
			bool[] mask = new bool[safe.Length];
			foreach (string secret in sensitive)
				for (int start = 0; start < safe.Length;)
				{
					int index = safe.IndexOf(secret, start, StringComparison.Ordinal);
					if (index < 0) break;
					Array.Fill(mask, true, index, secret.Length);
					start = index + 1;
				}
			System.Text.StringBuilder result = new();
			for (int index = 0; index < safe.Length; index++)
			{
				if (!mask[index]) result.Append(safe[index]);
				else if (index == 0 || !mask[index - 1]) result.Append(SecretRedactor.Removed);
			}
			string output = result.ToString();
			if (output.Length > MaximumArgumentLength) output = SecretRedactor.Omitted;
			if (output == text && argument is byte or short or int or long or ushort or uint or ulong or decimal or bool)
				return JsonSerializer.SerializeToElement(argument, argument.GetType());
			if (output == text && argument is double d && double.IsFinite(d)) return JsonSerializer.SerializeToElement(d);
			if (output == text && argument is float f && float.IsFinite(f)) return JsonSerializer.SerializeToElement(f);
			return JsonSerializer.SerializeToElement(output);
		}).ToArray();
	}
}
