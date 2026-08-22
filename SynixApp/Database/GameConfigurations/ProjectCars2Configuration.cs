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
using System.Text.RegularExpressions;

namespace Synix_Control_Panel.SynixApp.Database.GameConfigurations
{
	internal sealed class ProjectCars2Configuration : ConfigurationDefinition
	{
		private static readonly Regex PropertyPattern = new(
			@"(?m)^[ \t]*(?<key>[A-Za-z_][A-Za-z0-9_]*)[ \t]*:",
			RegexOptions.CultureInvariant);

		public override string GameName => "Project CARS 2";
		public override int SchemaVersion => 2;
		public override bool SupportsFullReset => true;
		public override ManagedConfigurationInput SupportedInputs =>
			ManagedConfigurationInput.ServerPassword |
			ManagedConfigurationInput.MaxPlayers |
			ManagedConfigurationInput.QueryPort |
			ManagedConfigurationInput.Port;
		public override string RelativePath => "server.cfg";

		public override string? CreateTemplate(ConfigurationContext context)
		{
			string sourcePath = ResolveFullPath(
				context.Server,
				@"config_sample\server.cfg");
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
							"The complete Project CARS 2 sample configuration is missing from the server installation.");
					}

					WriteNewFile(path, template);
					created = true;
				}

				string text = File.ReadAllText(path);
				List<string> missing = [];
				bool changed = false;
				text = ReplaceValue(text, "name", Quote(context.Server.ServerName), missing, ref changed);
				text = ReplaceValue(text, "password", Quote(context.Passwords.ServerPassword), missing, ref changed);
				text = ReplaceValue(text, "maxPlayerCount", context.Server.MaxPlayers.ToString(), missing, ref changed);
				text = ReplaceValue(text, "hostPort", context.Server.Port.ToString(), missing, ref changed);
				text = ReplaceValue(text, "queryPort", context.Server.QueryPort.ToString(), missing, ref changed);
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
						? "Created the complete Project CARS 2 configuration from its installed sample."
						: changed
							? "Updated the managed Project CARS 2 settings."
							: "The Project CARS 2 configuration is already current.");
			}
			catch (Exception exception)
			{
				return ConfigurationApplyResult.Failure(
					$"The Project CARS 2 configuration could not be applied: {exception.Message}");
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
				@"[ \t]*:[ \t]*)(?<value>""(?:\\.|[^""])*""|[-+]?\d+(?:\.\d+)?|true|false)(?<suffix>[ \t]*(?://.*)?\r?$)",
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
