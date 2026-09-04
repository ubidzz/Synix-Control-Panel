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
			ManagedConfigurationInput.Port |
			ManagedConfigurationInput.ServerName;
		public override string RelativePath => "server.cfg";

		public override IReadOnlyList<ConfigurationValidationItem> Validate(
			ConfigurationContext context)
		{
			string path = ResolveFullPath(context.Server);
			if (!File.Exists(path))
			{
				return [new ConfigurationValidationItem(
					ConfigurationValidationState.Failed,
					RelativePath,
					LocalizationManager.Get(
						"Configuration.Check.ManagedFile.Missing"))];
			}

			try
			{
				List<ConfigurationValidationItem> items = [];
				bool structureMatches = !NeedsStructuralRepair(context);
				items.Add(new ConfigurationValidationItem(
					structureMatches
						? ConfigurationValidationState.Passed
						: ConfigurationValidationState.Failed,
					LocalizationManager.Get(
						"Configuration.Check.TemplateStructure"),
					structureMatches
						? LocalizationManager.Get(
							"Configuration.Check.TemplateStructure.Present")
						: LocalizationManager.Get(
							"Configuration.Check.TemplateStructure.Invalid")));
				string text = File.ReadAllText(path);
				items.Add(ValidateValue(text, "name", Quote(context.Server.ServerName)));
				items.Add(ValidateValue(text, "password", Quote(context.Passwords.ServerPassword)));
				items.Add(ValidateValue(text, "maxPlayerCount", context.Server.MaxPlayers.ToString()));
				items.Add(ValidateValue(text, "hostPort", context.Server.Port.ToString()));
				items.Add(ValidateValue(text, "queryPort", context.Server.QueryPort.ToString()));
				return items;
			}
			catch (Exception exception)
			{
				return [new ConfigurationValidationItem(
					ConfigurationValidationState.Failed,
					LocalizationManager.Get(
						"Configuration.Check.ConfigurationRead"),
					LocalizationManager.Get(
						"Configuration.Check.ConfigurationRead.Failed",
						exception.Message))];
			}
		}

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
							LocalizationManager.Get(
								"Configuration.Apply.InstalledSampleMissing",
								GameName));
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
						LocalizationManager.Get(
							"Configuration.Apply.ManagedSettingsMissing",
							string.Join(", ", missing)));
				}

				return new ConfigurationApplyResult(
					true,
					true,
					created || changed,
					created,
					created
						? LocalizationManager.Get(
							"Configuration.Apply.CreatedFromInstalledSample",
							GameName)
						: changed
							? LocalizationManager.Get(
								"Configuration.Apply.ManagedSettingsUpdated",
								GameName)
							: LocalizationManager.Get(
								"Configuration.Apply.Current",
								GameName));
			}
			catch (Exception exception)
			{
				return ConfigurationApplyResult.Failure(
					LocalizationManager.Get(
						"Configuration.Apply.Failed",
						GameName,
						exception.Message));
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

		private static ConfigurationValidationItem ValidateValue(
			string text,
			string key,
			string expectedValue)
		{
			Regex pattern = new(
				@"(?m)^(?<prefix>[ \t]*" + Regex.Escape(key) +
				@"[ \t]*:[ \t]*)(?<value>""(?:\\.|[^""])*""|[-+]?\d+(?:\.\d+)?|true|false)(?<suffix>[ \t]*(?://.*)?\r?$)",
				RegexOptions.CultureInvariant);
			MatchCollection matches = pattern.Matches(text);
			if (matches.Count != 1)
			{
				return new ConfigurationValidationItem(
					ConfigurationValidationState.Failed,
					key,
					matches.Count == 0
						? LocalizationManager.Get(
							"Configuration.Check.ManagedTag.Missing")
						: LocalizationManager.Get(
							"Configuration.Check.ManagedTag.Duplicate"));
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
					? LocalizationManager.Get(
						"Configuration.Check.Value.Matches")
					: LocalizationManager.Get(
						"Configuration.Check.Value.Differs"));
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
