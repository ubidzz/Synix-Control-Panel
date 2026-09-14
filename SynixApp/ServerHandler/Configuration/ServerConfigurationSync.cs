using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Database.GameConfigurations;
using Synix_Control_Panel.SynixEngine;
using Synix_Control_Panel.SynixEngine.Minecraft;
using Synix_Control_Panel.SynixEngine.ModManagement;
using System.Globalization;
using System.Security.Cryptography;

namespace Synix_Control_Panel.SynixApp.ServerHandler;

internal static class ServerConfigurationSync
{
	private sealed record Change(ConfigurationServerField Field, object? Before, object? After);
	private sealed record SecretState(string Password, string Admin, string Rcon, string Token, string Webhook,
		List<DiscordWebhookRoute> Routes, int Version)
	{
		internal static SecretState Capture(GameServer server) => new(server.Password, server.AdminPassword,
			server.RconPassword, server.AuthenticationToken, server.DiscordWebhook, server.DiscordWebhookRoutes, server.PasswordStorageVersion);
		internal void Restore(GameServer server)
		{
			server.Password = Password; server.AdminPassword = Admin; server.RconPassword = Rcon;
			server.AuthenticationToken = Token; server.DiscordWebhook = Webhook;
			server.DiscordWebhookRoutes = Routes; server.PasswordStorageVersion = Version;
		}
	}

	internal static void Save(GameServer server, string path, string text, ConfigFormat format,
		string? expectedHash, Func<bool> persist)
	{
		// Java properties have escaping/continuation rules and edition-specific fields.
		if (MinecraftConfigurationSync.IsProperties(server, path))
		{
			MinecraftConfigurationSync.Save(server, path, text, expectedHash, persist);
			return;
		}
		using ServerOperationLease operation = ServerOperationCoordinator.TryBegin(server, ServerOperationKind.Configure);
		if (!operation.Acquired) throw new InvalidOperationException(operation.FailureReason);
		ModPackageManager.EnsureStopped(server);
		path = ModPathSafety.Resolve(server.InstallPath, Path.GetRelativePath(server.InstallPath, path));
		string backup = path + ".synix.bak";
		ModPathSafety.EnsureNoLinks(backup);
		if (MinecraftContentTransactions.HashFile(path) != expectedHash) throw Error("Changed");
		ConfigurationTextSnapshot snapshot = ConfigurationTextSnapshot.Read(path);
		byte[] original = File.ReadAllBytes(path);
		if (Hash(original) != expectedHash || Hash(snapshot.Encode(snapshot.Text)) != expectedHash) throw Error("Changed");
		_ = ConfigHandler.LoadConfigText(text, format);
		IReadOnlyList<Change> changes = ReadChanges(server, path, text);
		byte[] updated = snapshot.Encode(text);
		byte[]? priorBackup = File.Exists(backup) ? File.ReadAllBytes(backup) : null;
		bool wrote = false;
		int previousReportedMaximum = server.MaxPlayersFromQuery;
		SecretState secretsBefore = SecretState.Capture(server);
		try
		{
			if (!original.AsSpan().SequenceEqual(updated))
			{
				ModPathSafety.EnsureNoLinks(path);
				ModPathSafety.EnsureNoLinks(backup);
				if (MinecraftContentTransactions.HashFile(path) != expectedHash) throw Error("Changed");
				ConfigurationFileWriter.WriteAtomically(path, updated);
				wrote = true;
			}
			else if (MinecraftContentTransactions.HashFile(path) != expectedHash) throw Error("Changed");
			// Upgrade legacy credential storage before assigning protected values; otherwise
			// persistence could mistake new ciphertext for an old plaintext password.
			if (changes.Count > 0) Core.MigrateLegacyServer(server);
			foreach (Change change in changes) Set(server, change.Field, change.After);
			if (changes.Any(change => change.Field == ConfigurationServerField.MaxPlayers)) server.MaxPlayersFromQuery = 0;
			if (changes.Count > 0 && !persist()) throw Error("SaveEntry");
		}
		catch (Exception saveError)
		{
			foreach (Change change in changes) Set(server, change.Field, change.Before);
			server.MaxPlayersFromQuery = previousReportedMaximum;
			secretsBefore.Restore(server);
			if (wrote)
			{
				try
				{
					ModPathSafety.EnsureNoLinks(path);
					ModPathSafety.EnsureNoLinks(backup);
					// Never roll back over a new external edit.
					if (MinecraftContentTransactions.HashFile(path) != Hash(updated)) throw Error("Changed");
					ConfigurationFileWriter.WriteAtomically(path, original);
					File.WriteAllBytes(backup, priorBackup ?? original);
				}
				catch (Exception rollbackError)
				{
					throw new IOException(LocalizationManager.Get("Configuration.Sync.RollbackFailed", backup),
						new AggregateException(saveError, rollbackError));
				}
			}
			throw;
		}
		Core.Instance.UpdateGridStatus();
	}

	private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
	private static InvalidDataException Error(string name, string? key = null) =>
		new(LocalizationManager.Get("Configuration.Sync." + name, key ?? string.Empty));

	private static IReadOnlyList<Change> ReadChanges(GameServer server, string path, string text)
	{
		if (!GameFix.TryGetConfiguration(server.Game, out ConfigurationDefinition? definition) || definition == null)
			return [];
		IReadOnlyList<ConfigurationServerBinding> bindings = definition.GetServerBindings(server, path);
		if (bindings.Count == 0) return [];
		List<ConfigLine> lines = definition.ReadServerValues(text);
		GameInfo? game = GameDatabase.GetGame(server.Game);
		Dictionary<ConfigurationServerField, object?> values = [];
		StringComparison comparison = definition.Format == ConfigFormat.StandardINI ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
		foreach (ConfigurationServerBinding binding in bindings)
		{
			ConfigLine[] matches = binding.FindValues(lines, comparison).ToArray();
			// Missing keys do not imply empty/zero values. Duplicate addresses are ambiguous.
			if (matches.Length == 0) continue;
			if (matches.Length != 1) throw Error("Ambiguous", binding.Key);
			string value = matches[0].Value;
			if (value.IndexOfAny(['\r', '\n', '\0']) >= 0) throw Error("InvalidValue", binding.Key);
			ConfigurationServerField field = binding.Field switch
			{
				ConfigurationServerField.IsPvp or ConfigurationServerField.IsPve => ConfigurationServerField.GameMode,
				ConfigurationServerField.CrossplayPlatforms => ConfigurationServerField.CrossplayEnabled,
				_ => binding.Field
			};
			object? parsed = ConvertValue(binding, value, game);
			if (values.TryGetValue(field, out object? existing) && !Equals(existing, parsed))
				throw Error("Ambiguous", binding.Key);
			values[field] = parsed;
		}
		List<Change> changes = [];
		foreach ((ConfigurationServerField field, object? value) in values)
		{
			object? before = Get(server, field);
			object? after = value;
			if (field is ConfigurationServerField.Password or ConfigurationServerField.AdminPassword or
				ConfigurationServerField.RconPassword)
			{
				string previousPassword = server.PasswordStorageVersion == 0 ? (string?)before ?? string.Empty : Core.Reveal((string?)before ?? string.Empty);
				if (previousPassword == (string?)value) continue;
				after = Core.Protect((string?)value ?? string.Empty);
			}
			if (!Equals(before, after)) changes.Add(new(field, before, after));
		}
		if (changes.Any(change => change.Field == ConfigurationServerField.ServerName) && string.IsNullOrEmpty(server.ConfigurationIdentity))
			changes.Add(new(ConfigurationServerField.ConfigurationIdentity, server.ConfigurationIdentity, Core.GetServerIdentity(server)));
		return changes;
	}

	private static object? ConvertValue(ConfigurationServerBinding binding, string value, GameInfo? game)
	{
		int Number(int minimum, int maximum)
		{
			if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int number) ||
				number < minimum || number > maximum) throw Error("InvalidValue", binding.Key);
			return number;
		}
		bool Boolean()
		{
			if (value.Equals(game?.BooleanTrueValue ?? "true", StringComparison.OrdinalIgnoreCase)) return true;
			if (value.Equals(game?.BooleanFalseValue ?? "false", StringComparison.OrdinalIgnoreCase)) return false;
			if (bool.TryParse(value, out bool boolean)) return boolean;
			if (value == "1") return true;
			if (value == "0") return false;
			throw Error("InvalidValue", binding.Key);
		}
		switch (binding.Field)
		{
			case ConfigurationServerField.Port: return Number(1, 65535);
			case ConfigurationServerField.QueryPort:
			case ConfigurationServerField.RconPort: return Number(0, 65535);
			case ConfigurationServerField.AppPort:
				int port = Number(0, 65535); return port == 0 ? null : port;
			case ConfigurationServerField.MaxPlayers: return Number(1, game?.MaximumPlayers ?? 1000);
			case ConfigurationServerField.WorldSize: return Number(0, int.MaxValue);
			case ConfigurationServerField.WorldSeed:
				if (!GameServerInputValidator.TryValidateWorldSeed(game, value, out _)) throw Error("InvalidValue", binding.Key);
				return value;
			case ConfigurationServerField.EnableRcon:
			case ConfigurationServerField.CrossplayEnabled: return Boolean();
			case ConfigurationServerField.IsPvp: return Boolean() ? "PVP" : "PVE";
			case ConfigurationServerField.IsPve: return Boolean() ? "PVE" : "PVP";
			case ConfigurationServerField.CrossplayPlatforms:
				string[] platforms = value.Trim('(', ')').Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
				if (platforms.Length == 0 || platforms.Any(item => item is not ("Steam" or "Xbox" or "PS5" or "Mac")))
					throw Error("InvalidValue", binding.Key);
				return platforms.Any(item => item != "Steam");
			case ConfigurationServerField.GameMode:
				if (value.Equals(game?.PvpValue ?? "PVP", StringComparison.OrdinalIgnoreCase)) return "PVP";
				if (value.Equals(game?.PveValue ?? "PVE", StringComparison.OrdinalIgnoreCase)) return "PVE";
				return value;
			default: return value;
		}
	}

	private static object? Get(GameServer server, ConfigurationServerField field) => field switch
	{
		ConfigurationServerField.ServerName => server.ServerName,
		ConfigurationServerField.Password => server.Password,
		ConfigurationServerField.AdminPassword => server.AdminPassword,
		ConfigurationServerField.RconPassword => server.RconPassword,
		ConfigurationServerField.ConfigurationIdentity => server.ConfigurationIdentity,
		ConfigurationServerField.Port => server.Port,
		ConfigurationServerField.QueryPort => server.QueryPort,
		ConfigurationServerField.AppPort => server.AppPort,
		ConfigurationServerField.MaxPlayers => server.MaxPlayers,
		ConfigurationServerField.WorldSeed => server.WorldSeed,
		ConfigurationServerField.WorldSize => server.WorldSize,
		ConfigurationServerField.WorldName => server.WorldName,
		ConfigurationServerField.GameMode => server.GameMode,
		ConfigurationServerField.EnableRcon => server.EnableRcon,
		ConfigurationServerField.RconPort => server.RconPort,
		ConfigurationServerField.CrossplayEnabled => server.CrossplayEnabled,
		ConfigurationServerField.InviteCode => server.InviteCode,
		_ => throw new ArgumentOutOfRangeException(nameof(field))
	};

	private static void Set(GameServer server, ConfigurationServerField field, object? value)
	{
		switch (field)
		{
			case ConfigurationServerField.ServerName: server.ServerName = (string)value!; break;
			case ConfigurationServerField.Password: server.Password = (string)value!; break;
			case ConfigurationServerField.AdminPassword: server.AdminPassword = (string)value!; break;
			case ConfigurationServerField.RconPassword: server.RconPassword = (string)value!; break;
			case ConfigurationServerField.ConfigurationIdentity: server.ConfigurationIdentity = (string)value!; break;
			case ConfigurationServerField.Port: server.Port = (int)value!; break;
			case ConfigurationServerField.QueryPort: server.QueryPort = (int)value!; break;
			case ConfigurationServerField.AppPort: server.AppPort = (int?)value; break;
			case ConfigurationServerField.MaxPlayers: server.MaxPlayers = (int)value!; break;
			case ConfigurationServerField.WorldSeed: server.WorldSeed = (string)value!; break;
			case ConfigurationServerField.WorldSize: server.WorldSize = (int)value!; break;
			case ConfigurationServerField.WorldName: server.WorldName = (string)value!; break;
			case ConfigurationServerField.GameMode: server.GameMode = (string)value!; break;
			case ConfigurationServerField.EnableRcon: server.EnableRcon = (bool)value!; break;
			case ConfigurationServerField.RconPort: server.RconPort = (int)value!; break;
			case ConfigurationServerField.CrossplayEnabled: server.CrossplayEnabled = (bool)value!; break;
			case ConfigurationServerField.InviteCode: server.InviteCode = (string)value!; break;
			default: throw new ArgumentOutOfRangeException(nameof(field));
		}
	}
}
