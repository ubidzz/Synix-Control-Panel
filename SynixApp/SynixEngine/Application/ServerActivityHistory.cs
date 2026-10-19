using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Synix_Control_Panel.SynixApp.ServerHandler;

namespace Synix_Control_Panel.SynixEngine;

// Retain event identifiers and sanitized, translatable diagnostics. Never capture
// arbitrary notification bodies, console output, commands or config documents.
internal sealed record ServerActivityEntry(DateTimeOffset OccurredUtc, string Event, ServerActivityDiagnostic? Diagnostic = null)
{
	public string Description => Diagnostic?.Description ?? LocalizationManager.Get(Event == "ConfigurationSaved"
		? "History.ConfigurationSaved" : "Discord.Event." + Event);
	public bool NeedsAttention => Diagnostic?.Attention == true || Event.EndsWith("Failed", StringComparison.Ordinal) ||
		Event.EndsWith("Warning", StringComparison.Ordinal) || Event == "ServerCrashed";
}

internal sealed record ServerActivityReadResult(
	IReadOnlyList<ServerActivityEntry> Entries, bool Unavailable = false, bool Recovered = false);

internal sealed class ServerActivityHistory
{
	internal const int MaximumEntries = 1000;
	private const long MaximumFileBytes = 512 * 1024;
	private readonly string? _directory;
	private readonly object _gate = new();
	private readonly Dictionary<string, List<ServerActivityEntry>> _memory = new();
	private readonly HashSet<string> _failedWrites = new();
	// Hosts explicitly enable persistence. Designer and test hosts never touch live server data.
	internal static ServerActivityHistory Current { get; private set; } = new(null);
	internal static void Initialize(string directory) => Current = new(directory);
	internal event Action<string>? Changed;
	internal event Action<string>? BackupChanged;

	internal ServerActivityHistory(string? directory) =>
		_directory = directory == null ? null : Path.GetFullPath(directory);

	internal static string ServerKey(GameServer server)
	{
		string identity = server.Game + "|" + server.ServerName;
		try
		{
			if (!string.IsNullOrWhiteSpace(server.InstallPath))
				identity = Path.TrimEndingDirectorySeparator(Path.GetFullPath(server.InstallPath));
		}
		catch (ArgumentException) { /* An invalid draft path must not break the dashboard. */ }
		return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity.ToUpperInvariant())));
	}

	internal static bool IsKnownEvent(string? value) => value == "ConfigurationSaved" ||
		Enum.TryParse(value, out DiscordNotificationEvent kind) &&
		Enum.IsDefined(kind) && kind != DiscordNotificationEvent.None && kind != DiscordNotificationEvent.All &&
		kind.ToString() == value;

	internal bool Record(GameServer server, DiscordNotificationEvent kind) => Record(server, kind.ToString());

	internal bool Record(GameServer server, string kind, DateTimeOffset? occurredUtc = null)
	{
		if (!IsKnownEvent(kind)) return false;
		return Append(server, new((occurredUtc ?? DateTimeOffset.UtcNow).ToUniversalTime(), kind));
	}

	internal static string Describe(GameServer server, string resourceKey, params object?[] arguments)
	{
		Current.RecordDiagnostic(server, resourceKey, null, arguments);
		return LocalizationManager.Get(resourceKey, arguments);
	}

	internal bool RecordDiagnostic(GameServer server, string resourceKey, Color? color = null, params object?[] arguments)
	{
		if (!ServerActivityDiagnostic.IsAllowedKey(resourceKey) || arguments.Length > ServerActivityDiagnostic.MaximumArguments) return false;
		try
		{
			ServerActivityDiagnostic diagnostic = new(resourceKey,
				ServerActivityDiagnostic.SanitizeArguments(server, arguments), ServerActivityDiagnostic.IsAttention(resourceKey, color));
			if (!diagnostic.IsValid) return false;
			return Append(server, new(DateTimeOffset.UtcNow, "Diagnostic", diagnostic));
		}
		catch (Exception error) when (error is ArgumentException or SynixPasswordProtectionException or System.Security.SecurityException)
		{
			return false; // Logging must not interrupt the underlying operation.
		}
	}

	private bool Append(GameServer server, ServerActivityEntry newEntry)
	{
		string key;
		bool saved;
		try
		{
			key = ServerKey(server);
			lock (_gate)
			{
				using Mutex mutex = CreateMutex();
				if (!Acquire(mutex)) { _failedWrites.Add(key); return false; }
				try
				{
					ServerActivityReadResult existing = ReadCore(key);
					if (existing.Unavailable) { _failedWrites.Add(key); return false; }
					List<ServerActivityEntry> entries = existing.Entries
						.Append(newEntry)
						.OrderBy(entry => entry.OccurredUtc).TakeLast(MaximumEntries).ToList();
					// A character count is not a byte bound (JSON can escape Unicode).
					// Trim oldest complete entries before writing, never a partial line.
					List<string> lines = entries.Select(entry => JsonSerializer.Serialize(
						new StoredEntry(entry.Diagnostic == null ? 1 : 2, entry.OccurredUtc, entry.Event, entry.Diagnostic))).ToList();
					long bytes = lines.Sum(line => (long)Encoding.UTF8.GetByteCount(line) + Environment.NewLine.Length);
					int remove = 0;
					while (bytes > MaximumFileBytes && remove < lines.Count)
						bytes -= Encoding.UTF8.GetByteCount(lines[remove++]) + Environment.NewLine.Length;
					if (remove > 0) { entries.RemoveRange(0, remove); lines.RemoveRange(0, remove); }
					if (_directory == null) _memory[key] = entries;
					else
					{
						Directory.CreateDirectory(_directory);
						string path = Path.Combine(_directory, key + ".jsonl");
						string temporary = path + ".tmp";
						// Replacement is atomic on this volume; a crash leaves the previous complete history.
						using (FileStream file = new(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
						{
							using StreamWriter writer = new(file, new UTF8Encoding(false), leaveOpen: true);
							foreach (string line in lines) writer.WriteLine(line);
							writer.Flush();
							file.Flush(flushToDisk: true);
						}
						File.Move(temporary, path, overwrite: true);
					}
					_failedWrites.Remove(key);
					saved = true;
				}
				finally { mutex.ReleaseMutex(); }
			}
		}
		catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException or System.Security.SecurityException)
		{
			// History must never fail a server operation. Expose persistence failure in the history UI.
			try { lock (_gate) _failedWrites.Add(ServerKey(server)); } catch (ArgumentException) { }
			return false;
		}
		if (saved)
		{
			Notify(Changed, key);
			// A diagnostic entry must not trigger a new backup-folder scan. The
			// completed lifecycle events (including failures) invalidate that cache.
			if (newEntry.Event.StartsWith("Backup", StringComparison.Ordinal) ||
				newEntry.Event.StartsWith("Restore", StringComparison.Ordinal))
				Notify(BackupChanged, key);
		}
		return saved;
	}

	private static void Notify(Action<string>? subscribers, string key)
	{
		if (subscribers == null) return;
		foreach (Action<string> subscriber in subscribers.GetInvocationList().Cast<Action<string>>())
			try { subscriber(key); } catch (InvalidOperationException) { /* A closing window is not an operation failure. */ }
	}

	internal ServerActivityReadResult Read(GameServer server)
	{
		try
		{
			string key = ServerKey(server);
			lock (_gate)
			{
				using Mutex mutex = CreateMutex();
				if (!Acquire(mutex)) return new([], Unavailable: true);
				try
				{
					ServerActivityReadResult result = ReadCore(key);
					return result with { Unavailable = result.Unavailable || _failedWrites.Contains(key) };
				}
				finally { mutex.ReleaseMutex(); }
			}
		}
		catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException or System.Security.SecurityException)
		{ return new([], Unavailable: true); }
	}

	private ServerActivityReadResult ReadCore(string key)
	{
		if (_directory == null) return new(_memory.TryGetValue(key, out var values) ? values.ToArray() : []);
		string path = Path.Combine(_directory, key + ".jsonl");
		try
		{
			using FileStream file = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			if (file.Length > MaximumFileBytes) return new([], Unavailable: true);
			using StreamReader reader = new(file, Encoding.UTF8);
			List<ServerActivityEntry> entries = [];
			bool recovered = false, futureVersion = false;
			while (reader.ReadLine() is string line)
			{
				try
				{
					StoredEntry? entry = JsonSerializer.Deserialize<StoredEntry>(line);
					if (entry?.Version > 2) futureVersion = true;
					if (entry?.Version == 1 && IsKnownEvent(entry.Event) && entry.OccurredUtc != default)
						entries.Add(new(entry.OccurredUtc.ToUniversalTime(), entry.Event));
					else if (entry?.Version == 2 && entry.Event == "Diagnostic" && entry.Diagnostic?.IsValid == true && entry.OccurredUtc != default)
						entries.Add(new(entry.OccurredUtc.ToUniversalTime(), entry.Event, entry.Diagnostic));
					else recovered = true;
				}
				catch (JsonException) { recovered = true; }
			}
			return new(entries.OrderBy(entry => entry.OccurredUtc).TakeLast(MaximumEntries).ToArray(), Unavailable: futureVersion, Recovered: recovered);
		}
		catch (FileNotFoundException) { return new([]); }
		catch (DirectoryNotFoundException) { return new([]); }
	}

	private Mutex CreateMutex()
	{
		string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes((_directory ?? "memory").ToUpperInvariant())));
		return new Mutex(false, @"Local\SynixActivityHistory-" + hash);
	}
	private static bool Acquire(Mutex mutex)
	{
		try { return mutex.WaitOne(TimeSpan.FromSeconds(1)); }
		catch (AbandonedMutexException) { return true; }
	}
	private sealed record StoredEntry(int Version, DateTimeOffset OccurredUtc, string Event, ServerActivityDiagnostic? Diagnostic = null);
}
