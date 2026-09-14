using Synix_Control_Panel.SynixApp.ServerHandler;
using System.Text.RegularExpressions;

namespace Synix_Control_Panel.SynixApp.Database.GameConfigurations;

// These are explicit destinations in Synix's server record, not guesses based on config key names.
internal enum ConfigurationServerField
{
	None, ServerName, Password, AdminPassword, RconPassword, ConfigurationIdentity,
	Port, QueryPort, AppPort, MaxPlayers, WorldSeed, WorldSize, WorldName, GameMode,
	EnableRcon, RconPort, CrossplayEnabled, InviteCode, IsPvp, IsPve, CrossplayPlatforms
}

internal sealed record ConfigurationServerBinding(string Key, ConfigurationServerField Field, string? Path = null,
	string? ItemIdentityKey = null, string? ItemIdentityValue = null)
{
	internal IEnumerable<ConfigLine> FindValues(IReadOnlyList<ConfigLine> lines, StringComparison comparison)
	{
		if (ItemIdentityKey == null)
			return lines.Where(line => line.Key.Equals(Key, comparison) && (Path == null || line.Path.Equals(Path, comparison)));
		// Named/id-based JSON entries (for example server access groups) may be reordered.
		// Select the same trusted role, never another role's password at its old array index.
		Match item = Regex.Match(Key, @"^(.*)\[\d+\](.+)$");
		Match identity = Regex.Match(ItemIdentityKey, @"^(.*)\[\d+\](.+)$");
		string pattern = "^" + Regex.Escape(item.Groups[1].Value) + @"\[(\d+)\]" + Regex.Escape(item.Groups[2].Value) + "$";
		return lines.Where(line =>
		{
			Match actual = Regex.Match(line.Key, pattern);
			if (!actual.Success) return false;
			string identityKey = identity.Groups[1].Value + "[" + actual.Groups[1].Value + "]" + identity.Groups[2].Value;
			return lines.Any(candidate => candidate.Key == identityKey && candidate.Value == ItemIdentityValue);
		});
	}

	internal static List<ConfigLine> ReadFlatAssignments(string text, string separator, string comment, bool allowComma = false)
	{
		// These games use their own flat assignment grammar, not JSON or Lua execution.
		string pattern = @"(?m)^[ \t]*(?<key>[A-Za-z_][A-Za-z0-9_]*)[ \t]*" + Regex.Escape(separator) +
			@"[ \t]*(?<value>""(?:\\.|[^""\r\n])*""|[-+]?\d+(?:\.\d+)?|true|false)[ \t]*" +
			(allowComma ? ",?[ \\t]*" : "") + "(?:" + Regex.Escape(comment) + @"[^\r\n]*)?\r?$";
		string normalized = string.Join("\n", Regex.Matches(text, pattern, RegexOptions.CultureInvariant)
			.Select(match => match.Groups["key"].Value + "=" + match.Groups["value"].Value));
		return ConfigHandler.LoadConfigText(normalized, ConfigFormat.StandardINI);
	}

	private static readonly IReadOnlyDictionary<string, ConfigurationServerField> Tokens =
		new Dictionary<string, ConfigurationServerField>(StringComparer.Ordinal)
		{
			["{ServerName}"] = ConfigurationServerField.ServerName,
			["{Password}"] = ConfigurationServerField.Password,
			["{AdminPassword}"] = ConfigurationServerField.AdminPassword,
			["{RCONPassword}"] = ConfigurationServerField.RconPassword,
			["{Port}"] = ConfigurationServerField.Port,
			["{QueryPort}"] = ConfigurationServerField.QueryPort,
			["{AppPort}"] = ConfigurationServerField.AppPort,
			["{MaxPlayers}"] = ConfigurationServerField.MaxPlayers,
			["{WorldSeed}"] = ConfigurationServerField.WorldSeed,
			["{WorldSize}"] = ConfigurationServerField.WorldSize,
			["{WorldName}"] = ConfigurationServerField.WorldName,
			["{GameMode}"] = ConfigurationServerField.GameMode,
			["{EnableRcon}"] = ConfigurationServerField.EnableRcon,
			["{RCONPort}"] = ConfigurationServerField.RconPort,
			["{Crossplay}"] = ConfigurationServerField.CrossplayEnabled,
			["{IsPvp}"] = ConfigurationServerField.IsPvp,
			["{IsPve}"] = ConfigurationServerField.IsPve
		};

	internal static IReadOnlyList<ConfigurationServerBinding> FromTemplate(string template, ConfigFormat format)
	{
		// Parse only trusted game templates. Two distinct scalar probes identify whole-value
		// placeholders, preserving sections/nesting and excluding derived/composite settings.
		// Identity, addresses and HasPassword must never be mistaken for editable server fields.
		bool IsBoolean(string token) => token is "{EnableRcon}" or "{Crossplay}" or "{IsPvp}" or "{IsPve}" or "{HasPassword}";
		Dictionary<string, string> values = Regex.Matches(template, @"\{[A-Za-z]+\}")
			.Select(match => match.Value).Distinct(StringComparer.Ordinal)
			.ToDictionary(token => token, token => IsBoolean(token) ? "false" : "111", StringComparer.Ordinal);
		string Expand() => format == ConfigFormat.YAML
			? ConfigHandler.ExpandYamlTemplate(template, values)
			: Regex.Replace(template, @"\{[A-Za-z]+\}", match => values[match.Value]);
		List<ConfigLine> baseline = ConfigHandler.LoadConfigText(Expand(), format);
		List<ConfigurationServerBinding> bindings = [];
		foreach ((string token, ConfigurationServerField field) in Tokens)
		{
			if (!values.TryGetValue(token, out string? first)) continue;
			string second = IsBoolean(token) ? "true" : "222";
			values[token] = second;
			Dictionary<string, ConfigLine> changed = ConfigHandler.LoadConfigText(Expand(), format)
				.ToDictionary(line => line.Id, StringComparer.Ordinal);
			values[token] = first;
			foreach (ConfigLine line in baseline)
			{
				if (line.Value.Equals(first, StringComparison.OrdinalIgnoreCase) &&
					changed.TryGetValue(line.Id, out ConfigLine? other) &&
					line.Key == other.Key && line.Path == other.Path &&
					other.Value.Equals(second, StringComparison.OrdinalIgnoreCase))
				{
					ConfigLine? identity = null;
					if (format == ConfigFormat.JSON)
					{
						Match item = Regex.Match(line.Key, @"^(.*\[\d+\])\.[^.[\]]+$");
						if (item.Success)
							identity = baseline.FirstOrDefault(candidate =>
								new[] { ".id", ".Id", ".name", ".Name" }.Any(suffix => candidate.Key == item.Groups[1].Value + suffix) &&
								candidate.Value != "111" && candidate.Value.Length > 0);
					}
					bindings.Add(new(line.Key, field, line.Path, identity?.Key, identity?.Value));
				}
			}
		}
		return bindings.Distinct().ToArray();
	}
}
