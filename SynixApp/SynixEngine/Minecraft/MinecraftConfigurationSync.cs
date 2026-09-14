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
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine.ModManagement;
using System.Globalization;
using System.Text;

namespace Synix_Control_Panel.SynixEngine.Minecraft;

internal static class MinecraftConfigurationSync
{
	internal static bool IsProperties(GameServer server, string path) =>
		MinecraftControlProfile.IsJava(server) || MinecraftControlProfile.IsBedrock(server)
			? Path.GetFullPath(path).Equals(ModPathSafety.Resolve(server.InstallPath, "server.properties"), StringComparison.OrdinalIgnoreCase)
			: false;

	internal static IReadOnlyDictionary<string, string> ReadProperties(string text)
	{
		Dictionary<string, string> result = new(StringComparer.Ordinal);
		string pending = "";
		foreach (string physical in text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
		{
			string line = pending + (pending.Length > 0 ? physical.TrimStart() : physical.TrimStart('\uFEFF', ' ', '\t', '\f'));
			int trailing = line.Length - line.TrimEnd('\\').Length;
			if (trailing % 2 != 0) { pending = line[..^1]; continue; }
			pending = "";
			if (line.Length == 0 || line[0] is '#' or '!') continue;
			int separator = 0;
			bool escaped = false;
			for (; separator < line.Length; separator++)
			{
				char c = line[separator];
				if (!escaped && (c is '=' or ':' || char.IsWhiteSpace(c))) break;
				escaped = !escaped && c == '\\';
			}
			string key = Decode(line[..separator]);
			int valueAt = separator;
			while (valueAt < line.Length && char.IsWhiteSpace(line[valueAt])) valueAt++;
			if (valueAt < line.Length && line[valueAt] is '=' or ':') valueAt++;
			while (valueAt < line.Length && char.IsWhiteSpace(line[valueAt])) valueAt++;
			result[key] = Decode(line[valueAt..]);
		}
		if (pending.Length > 0) throw MinecraftContentTransactions.Error("Properties");
		return result;
	}

	private static string Decode(string value)
	{
		StringBuilder result = new();
		for (int i = 0; i < value.Length; i++)
		{
			if (value[i] != '\\') { result.Append(value[i]); continue; }
			if (++i == value.Length) throw MinecraftContentTransactions.Error("Properties");
			if (value[i] == 'u')
			{
				if (i + 4 >= value.Length || !ushort.TryParse(value.AsSpan(i + 1, 4), NumberStyles.HexNumber,
					CultureInfo.InvariantCulture, out ushort number)) throw MinecraftContentTransactions.Error("Properties");
				result.Append((char)number); i += 4;
			}
			else result.Append(value[i] switch { 'n' => '\n', 'r' => '\r', 't' => '\t', 'f' => '\f', _ => value[i] });
		}
		return result.ToString();
	}

	internal static void Save(GameServer server, string path, string text, string? expectedHash, Func<bool> persist)
	{
		using ServerOperationLease operation = ModPackageManager.BeginOperation(server);
		ModPackageManager.EnsureStopped(server);
		string relative = Path.GetRelativePath(server.InstallPath, path).Replace('\\', '/');
		ModPathSafety.Resolve(server.InstallPath, relative);
		if (text.Length > 4 * 1024 * 1024) throw MinecraftContentTransactions.Error("Size");
		State before = State.Capture(server);
		State after = IsProperties(server, path) ? ReadState(server, text) : before;
		byte[] bytes = File.Exists(path) ? ConfigurationTextSnapshot.Read(path).Encode(text) : new UTF8Encoding(false, true).GetBytes(text);
		using MinecraftStaging staging = new();
		using MemoryStream input = new(bytes);
		staging.Add(relative, input, bytes.Length);
		string source = staging.Files[relative];
		MinecraftChangeReceipt receipt = MinecraftContentTransactions.Apply(server, relative, "config",
			[new(relative, source, MinecraftContentTransactions.HashFile(source), expectedHash)]);
		try
		{
			after.Apply(server);
			if (before != after && !persist()) throw MinecraftContentTransactions.Error("SaveEntry");
		}
		catch
		{
			before.Apply(server);
			MinecraftContentTransactions.Undo(server, receipt.Id, rollback: true);
			throw;
		}
		Core.Instance.UpdateGridStatus();
	}

	internal static bool Synchronize(GameServer server, Func<bool> persist)
	{
		if (!MinecraftControlProfile.IsJava(server) && !MinecraftControlProfile.IsBedrock(server)) return false;
		using ServerOperationLease operation = ModPackageManager.BeginOperation(server);
		ModPackageManager.EnsureStopped(server);
		string path = ModPathSafety.Resolve(server.InstallPath, "server.properties");
		if (!File.Exists(path)) return false;
		if (new FileInfo(path).Length > 4 * 1024 * 1024) throw MinecraftContentTransactions.Error("Size");
		State before = State.Capture(server);
		State after = ReadState(server, ConfigurationTextSnapshot.Read(path).Text);
		if (before == after) return false;
		try
		{
			after.Apply(server);
			if (!persist()) throw MinecraftContentTransactions.Error("SaveEntry");
		}
		catch { before.Apply(server); throw; }
		Core.Instance.UpdateGridStatus();
		return true;
	}

	internal static void SynchronizeRestored(GameServer server, Func<bool> persist, bool fullRestore = false)
	{
		using ServerOperationLease operation = ModPackageManager.BeginOperation(server);
		ModPackageManager.EnsureStopped(server);
		State before = State.Capture(server);
		MinecraftInstalledRuntime runtimeBefore = MinecraftInstalledRuntime.Capture(server);
		try
		{
			MinecraftRuntimeUpdater.ReadInstalled(server)?.Apply(server);
			string path = ModPathSafety.Resolve(server.InstallPath, "server.properties");
			if (File.Exists(path))
			{
				if (new FileInfo(path).Length > 4 * 1024 * 1024) throw MinecraftContentTransactions.Error("Size");
				ReadState(server, ConfigurationTextSnapshot.Read(path).Text).Apply(server);
			}
			if (!persist()) throw MinecraftContentTransactions.Error("SaveEntry");
		}
		catch { before.Apply(server); runtimeBefore.Apply(server); throw; }
		if (fullRestore) MinecraftContentTransactions.RecordFullRestore(server);
		Core.Instance.UpdateGridStatus();
	}

	internal static string SetProperty(string text, string key, string value)
	{
		// Appending a final assignment follows Java properties' last-value-wins semantics,
		// including escaped keys and continued values, without rewriting unrelated settings.
		string newline = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
		return text.TrimEnd('\r', '\n') + newline + key + "=" +
			value.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n") + newline;
	}

	private static State ReadState(GameServer server, string text)
	{
		IReadOnlyDictionary<string, string> values = ReadProperties(text);
		State old = State.Capture(server);
		bool bedrock = MinecraftControlProfile.IsBedrock(server);
		static Exception Invalid(string key) => new InvalidDataException(LocalizationManager.Get("MinecraftWorkspace.Error.InvalidValue", key));
		int Number(string key, int fallback, int maximum, bool allowZero = false)
		{
			if (!values.TryGetValue(key, out string? value)) return fallback;
			if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) ||
				parsed < (allowZero ? 0 : 1) || parsed > maximum) throw Invalid(key);
			return parsed;
		}
		bool Boolean(string key, bool fallback)
		{
			if (!values.TryGetValue(key, out string? value)) return fallback;
			return bool.TryParse(value, out bool parsed) ? parsed : throw Invalid(key);
		}
		string mode = values.GetValueOrDefault("gamemode", MinecraftControlProfile.NormalizeGameMode(old.Mode)).ToLowerInvariant();
		mode = mode switch { "0" => "survival", "1" => "creative", "2" => "adventure", "3" => "spectator", _ => mode };
		if (mode is not ("survival" or "creative" or "adventure" or "spectator") || (bedrock && mode == "spectator"))
			throw Invalid("gamemode");
		string world = values.GetValueOrDefault("level-name", string.IsNullOrWhiteSpace(old.World) ? "world" : old.World);
		if (!ModPathSafety.IsSafeRelativePath(world) || world.Contains('/') || world.Contains('\\') || world.Length > 128)
			throw MinecraftContentTransactions.Error("WorldName");
		string seed = values.GetValueOrDefault("level-seed", old.Seed);
		if (seed.Length > 128 || seed.Any(char.IsControl)) throw MinecraftContentTransactions.Error("Properties");
		string? advertised = values.TryGetValue(bedrock ? "server-name" : "motd", out string? advertisedValue) ? advertisedValue : old.Advertised;
		if (advertised?.Length > 2048 || advertised?.Contains('\0') == true) throw MinecraftContentTransactions.Error("Properties");
		if (bedrock && advertised != null && (advertised.Any(char.IsControl) || advertised.Contains(';')))
			throw Invalid("server-name");
		string rcon = old.RconPassword;
		if (!bedrock && values.TryGetValue("rcon.password", out string? password))
		{
			if (password.Length > 4096 || password.Any(char.IsControl)) throw MinecraftContentTransactions.Error("Properties");
			if (!Core.IsProtected(rcon) || Core.Reveal(rcon) != password) rcon = Core.Protect(password);
		}
		bool management = !bedrock && Boolean("management-server-enabled", old.Management && MinecraftControlProfile.SupportsManagementProtocol(server.GameVersion));
		if (management && values.TryGetValue("management-server-host", out string? host) &&
			host is not ("localhost" or "127.0.0.1" or "::1" or "[::1]")) throw MinecraftContentTransactions.Error("ManagementLocal");
		State result = old with
		{
			Port = Number("server-port", old.Port, 65535),
			Query = Number(bedrock ? "server-portv6" : "query.port", old.Query, 65535),
			Players = Number("max-players", old.Players, 100000),
			World = world, Seed = seed, Mode = char.ToUpperInvariant(mode[0]) + mode[1..], Advertised = advertised,
			QueryEnabled = !bedrock && Boolean("enable-query", old.QueryEnabled),
			Rcon = !bedrock && Boolean("enable-rcon", old.Rcon),
			RconPort = bedrock ? old.RconPort : Number("rcon.port", old.RconPort, 65535),
			RconPassword = rcon, Management = management,
			ManagementPort = bedrock ? old.ManagementPort : Number("management-server-port", old.ManagementPort, 65535, !management),
			ManagementTls = !bedrock && Boolean("management-server-tls-enabled", old.ManagementTls)
		};
		if (management && values.ContainsKey("management-server-enabled") && result.ManagementPort < 1)
			throw Invalid("management-server-port");
		int[] ports = new[] { result.Port, bedrock || result.QueryEnabled ? result.Query : 0,
			result.Rcon ? result.RconPort : 0, result.Management ? result.ManagementPort : 0 }.Where(port => port > 0).ToArray();
		// Java gameplay is TCP, query is UDP; sharing that number is valid. Bedrock's
		// two gameplay ports bind different address families. RCON/management are TCP.
		int[] tcpPorts = bedrock ? [] : new[] { result.Port, result.Rcon ? result.RconPort : 0,
			result.Management ? result.ManagementPort : 0 }.Where(port => port > 0).ToArray();
		if (tcpPorts.Distinct().Count() != tcpPorts.Length) throw MinecraftContentTransactions.Error("Properties");
		foreach (GameServer other in ServerRegistry.Servers.Where(other => !ReferenceEquals(other, server) &&
			!other.InstallPath.Equals(server.InstallPath, StringComparison.OrdinalIgnoreCase)))
			if (ports.Any(port => Core.HasConfiguredPort(other, port))) throw MinecraftContentTransactions.Error("PortConflict");
		return result;
	}

	private sealed record State(int Port, int Query, int Players, string World, string Seed, string Mode,
		string? Advertised, bool QueryEnabled, bool Rcon, int RconPort, string RconPassword,
		bool Management, int ManagementPort, bool ManagementTls)
	{
		internal static State Capture(GameServer server) => new(server.Port, server.QueryPort, server.MaxPlayers,
			server.WorldName, server.WorldSeed, server.GameMode, server.MinecraftAdvertisedName, server.MinecraftQueryEnabled,
			server.EnableRcon, server.RconPort, server.RconPassword, server.EnableMinecraftManagementProtocol,
			server.MinecraftManagementPort, server.MinecraftManagementTlsEnabled);
		internal void Apply(GameServer server)
		{
			server.Port = Port; server.QueryPort = Query; server.MaxPlayers = Players; server.WorldName = World;
			server.WorldSeed = Seed; server.GameMode = Mode; server.MinecraftAdvertisedName = Advertised;
			server.MinecraftQueryEnabled = QueryEnabled; server.EnableRcon = Rcon; server.RconPort = RconPort;
			server.RconPassword = RconPassword; server.EnableMinecraftManagementProtocol = Management;
			server.MinecraftManagementPort = ManagementPort; server.MinecraftManagementTlsEnabled = ManagementTls;
		}
	}
}
