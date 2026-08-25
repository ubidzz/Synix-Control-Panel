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
using System.Text.RegularExpressions;

namespace Synix_Control_Panel.SynixApp.Database.GameConfigurations
{
	internal sealed class JustCause2MultiplayerConfiguration : ConfigurationDefinition
	{
		private static readonly Regex PropertyPattern = new(
			@"(?m)^[ \t]*(?<key>[A-Za-z_][A-Za-z0-9_]*)[ \t]*=",
			RegexOptions.CultureInvariant);

		public override string GameName => "Just Cause 2: Multiplayer";
		public override int SchemaVersion => 2;
		public override bool SupportsFullReset => true;
		public override ManagedConfigurationInput SupportedInputs =>
			ManagedConfigurationInput.ServerPassword |
			ManagedConfigurationInput.MaxPlayers |
			ManagedConfigurationInput.Port |
			ManagedConfigurationInput.ServerName;
		public override string RelativePath => "config.lua";

		public override IReadOnlyList<ConfigurationValidationItem> Validate(
			ConfigurationContext context)
		{
			string path = ResolveFullPath(context.Server);
			if (!File.Exists(path))
			{
				return [new ConfigurationValidationItem(
					ConfigurationValidationState.Failed,
					RelativePath,
					"The managed configuration file is missing.")];
			}

			try
			{
				List<ConfigurationValidationItem> items = [];
				bool structureMatches = !NeedsStructuralRepair(context);
				items.Add(new ConfigurationValidationItem(
					structureMatches
						? ConfigurationValidationState.Passed
						: ConfigurationValidationState.Failed,
					"Template structure",
					structureMatches
						? "The required template structure is present."
						: "One or more required template tags are missing or invalid."));
				string text = File.ReadAllText(path);
				items.Add(ValidateValue(text, "MaxPlayers", context.Server.MaxPlayers.ToString()));
				items.Add(ValidateValue(text, "BindPort", context.Server.Port.ToString()));
				items.Add(ValidateValue(text, "Name", Quote(context.Server.ServerName)));
				items.Add(ValidateValue(text, "Password", Quote(context.Passwords.ServerPassword)));
				return items;
			}
			catch (Exception exception)
			{
				return [new ConfigurationValidationItem(
					ConfigurationValidationState.Failed,
					"Configuration read",
					$"Synix could not safely inspect this configuration: {exception.Message}")];
			}
		}

		public override string? CreateTemplate(ConfigurationContext context)
		{
			string sourcePath = ResolveFullPath(context.Server, "default_config.lua");
			return File.Exists(sourcePath)
				? File.ReadAllText(sourcePath)
				: null;
		}

		public override ConfigurationApplyResult Apply(ConfigurationContext context)
		{
			try
			{
				string path = ResolveFullPath(context.Server);
				bool created = false;
				if (!File.Exists(path))
				{
					string? template = CreateTemplate(context);
					if (template == null)
					{
						return ConfigurationApplyResult.Failure(
							"The complete Just Cause 2 default_config.lua is missing from the server installation.");
					}

					WriteNewFile(path, template);
					created = true;
				}

				string text = File.ReadAllText(path);
				List<string> missing = [];
				bool changed = false;
				text = ReplaceValue(text, "MaxPlayers", context.Server.MaxPlayers.ToString(), missing, ref changed);
				text = ReplaceValue(text, "BindPort", context.Server.Port.ToString(), missing, ref changed);
				text = ReplaceValue(text, "Name", Quote(context.Server.ServerName), missing, ref changed);
				text = ReplaceValue(text, "Password", Quote(context.Passwords.ServerPassword), missing, ref changed);
				if (changed)
				{
					File.WriteAllText(path, text);
				}

				if (missing.Count > 0)
				{
					return new ConfigurationApplyResult(
						true,
						false,
						changed,
						created,
						$"The complete file was preserved, but these settings were not found: {string.Join(", ", missing)}.");
				}

				return new ConfigurationApplyResult(
					true,
					true,
					created || changed,
					created,
					created
						? "Created the complete Just Cause 2 configuration from its installed default file."
						: changed
							? "Updated the managed Just Cause 2 settings."
							: "The Just Cause 2 configuration is already current.");
			}
			catch (Exception exception)
			{
				return ConfigurationApplyResult.Failure(
					$"The Just Cause 2 configuration could not be applied: {exception.Message}");
			}
		}

		public override bool NeedsStructuralRepair(ConfigurationContext context)
		{
			string? template = CreateTemplate(context);
			string path = ResolveFullPath(context.Server);
			if (template == null || !File.Exists(path))
			{
				return template != null;
			}

			Dictionary<string, int> expected = GetPropertyCounts(template);
			Dictionary<string, int> existing = GetPropertyCounts(File.ReadAllText(path));
			return expected.Any(pair =>
				!existing.TryGetValue(pair.Key, out int count) || count < pair.Value);
		}

		private static string ReplaceValue(
			string text,
			string key,
			string value,
			List<string> missing,
			ref bool changed)
		{
			Regex pattern = new(
				@"(?m)^(?<prefix>[ \t]*" + Regex.Escape(key) +
				@"[ \t]*=[ \t]*)(?<value>""(?:\\.|[^""])*""|[-+]?\d+(?:\.\d+)?|true|false)(?<suffix>[ \t]*,?[ \t]*(?:--.*)?\r?$)",
				RegexOptions.CultureInvariant);
			MatchCollection matches = pattern.Matches(text);
			if (matches.Count != 1)
			{
				missing.Add(key);
				return text;
			}

			if (string.Equals(matches[0].Groups["value"].Value, value, StringComparison.Ordinal))
			{
				return text;
			}

			changed = true;
			return pattern.Replace(
				text,
				match => match.Groups["prefix"].Value + value + match.Groups["suffix"].Value,
				1);
		}

		private static ConfigurationValidationItem ValidateValue(
			string text,
			string key,
			string expectedValue)
		{
			Regex pattern = new(
				@"(?m)^(?<prefix>[ \t]*" + Regex.Escape(key) +
				@"[ \t]*=[ \t]*)(?<value>""(?:\\.|[^""])*""|[-+]?\d+(?:\.\d+)?|true|false)(?<suffix>[ \t]*,?[ \t]*(?:--.*)?\r?$)",
				RegexOptions.CultureInvariant);
			MatchCollection matches = pattern.Matches(text);
			if (matches.Count != 1)
			{
				return new ConfigurationValidationItem(
					ConfigurationValidationState.Failed,
					key,
					matches.Count == 0
						? "The managed configuration tag is missing."
						: "The managed tag appears more than once, so Synix cannot safely identify one value.");
			}

			bool matchesSavedValue = string.Equals(
				matches[0].Groups["value"].Value,
				expectedValue,
				StringComparison.Ordinal);
			return new ConfigurationValidationItem(
				matchesSavedValue
					? ConfigurationValidationState.Passed
					: ConfigurationValidationState.Failed,
				key,
				matchesSavedValue
					? "The file value matches the value saved in Synix."
					: "The file value does not match the value saved in Synix.");
		}

		private static Dictionary<string, int> GetPropertyCounts(string text)
		{
			return PropertyPattern.Matches(text)
				.Select(match => match.Groups["key"].Value)
				.GroupBy(key => key, StringComparer.Ordinal)
				.ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
		}

		private static string Quote(string value)
		{
			return $"\"{EscapeQuoted(value)}\"";
		}
	}
}
