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
using System.Text.Json;

namespace Synix_Control_Panel.SynixApp.Database.GameConfigurations
{
	internal readonly record struct ConfigurationTemplate(
		string RelativePath,
		string Content);

	internal abstract class TemplateConfigurationDefinition : ConfigurationDefinition
	{
		protected abstract IReadOnlyList<ConfigurationTemplate> Templates { get; }
		public override bool SupportsFullReset => true;
		public override ConfigFormat Format =>
			GameDatabase.GetGame(GameName)?.Format ?? ConfigFormat.StandardINI;

		public override string RelativePath => Templates.Count > 0
			? Templates[0].RelativePath
			: string.Empty;

		public override ManagedConfigurationInput SupportedInputs =>
			GetSupportedInputs();

		public override ConfigurationApplyResult Apply(ConfigurationContext context)
		{
			try
			{
				bool created = false;
				bool changed = false;
				List<string> missing = [];
				foreach (ConfigurationTemplate template in Templates)
				{
					string fullPath = ResolveFullPath(context.Server, template.RelativePath);
					string expandedTemplate = ExpandTemplate(template.Content, context);
					if (!File.Exists(fullPath))
					{
						WriteNewFile(fullPath, expandedTemplate);
						created = true;
						changed = true;
						continue;
					}

					(bool fileChanged, IReadOnlyList<string> fileMissing) =
						UpdateManagedValues(
							fullPath,
							template.Content,
							expandedTemplate,
							context);
					changed |= fileChanged;
					missing.AddRange(fileMissing.Select(setting =>
						$"{template.RelativePath}: {setting}"));
				}

				if (missing.Count > 0)
				{
					return new ConfigurationApplyResult(
						true,
						false,
						changed,
						created,
						$"The existing files were preserved, but these managed settings were not found: {string.Join(", ", missing)}.");
				}

				return new ConfigurationApplyResult(
					true,
					true,
					changed,
					created,
					created
						? $"Created the required {GameName} configuration files."
						: changed
							? $"Updated the managed {GameName} configuration settings."
							: $"The {GameName} configuration files are already current.");
			}
			catch (Exception ex)
			{
				return ConfigurationApplyResult.Failure(
					$"The {GameName} configuration could not be created: {ex.Message}");
			}
		}

		public override bool NeedsStructuralRepair(ConfigurationContext context)
		{
			foreach (ConfigurationTemplate template in Templates)
			{
				string path = ResolveFullPath(context.Server, template.RelativePath);
				string content = ExpandTemplate(template.Content, context);
				if (!ConfigHandler.HasRequiredStructure(path, content, Format))
				{
					return true;
				}
			}

			return false;
		}

		public override ConfigurationApplyResult ResetToTemplate(
			ConfigurationContext context)
		{
			return ReplaceWithTemplates(
				context,
				Templates.Select(template => new ResetTemplate(
					template.RelativePath,
					ExpandTemplate(template.Content, context),
					Format))
					.ToArray());
		}

		public override bool ConfigurationFileExists(GameServer server)
		{
			return Templates.All(template =>
				File.Exists(ResolveFullPath(server, template.RelativePath)));
		}

		internal override IReadOnlyList<string> ResolveConfigurationPaths(
			GameServer server)
		{
			return Templates
				.Select(template => ResolveFullPath(server, template.RelativePath))
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToArray();
		}

		private string ExpandTemplate(
			string template,
			ConfigurationContext context,
			bool useProbeValues = false)
		{
			GameServer server = context.Server;
			string ProbeText(string value, string name) => useProbeValues
				? $"{value}__synix_probe_{name}__"
				: value;
			string TextValue(string value, string name)
			{
				string expanded = ProbeText(value, name);
				return Format == ConfigFormat.JSON
					? JsonEncodedText.Encode(expanded).ToString()
					: expanded;
			}
			int ProbeNumber(int value) => useProbeValues
				? value == int.MaxValue ? value - 1 : value + 1
				: value;
			bool ProbeBoolean(bool value) => useProbeValues ? !value : value;

			return template
				.Replace("{ServerName}", TextValue(RequireSingleLine(server.ServerName, "ServerName"), "server_name"), StringComparison.Ordinal)
				.Replace("{Password}", TextValue(RequireSingleLine(context.Passwords.ServerPassword, "Password"), "password"), StringComparison.Ordinal)
				.Replace("{AdminPassword}", TextValue(RequireSingleLine(context.Passwords.AdminPassword, "AdminPassword"), "admin_password"), StringComparison.Ordinal)
				.Replace("{MaxPlayers}", ProbeNumber(server.MaxPlayers).ToString(), StringComparison.Ordinal)
				.Replace("{Port}", ProbeNumber(server.Port).ToString(), StringComparison.Ordinal)
				.Replace("{QueryPort}", ProbeNumber(server.QueryPort).ToString(), StringComparison.Ordinal)
				.Replace("{RCONPort}", ProbeNumber(server.RconPort).ToString(), StringComparison.Ordinal)
				.Replace("{RCONPassword}", TextValue(RequireSingleLine(context.Passwords.RconPassword, "RCONPassword"), "rcon_password"), StringComparison.Ordinal)
				.Replace("{EnableRcon}", ProbeBoolean(server.EnableRcon).ToString().ToLowerInvariant(), StringComparison.Ordinal)
				.Replace("{Identity}", TextValue(context.Identity, "identity"), StringComparison.Ordinal)
				.Replace("{WorldName}", TextValue(RequireSingleLine(server.WorldName, "WorldName"), "world_name"), StringComparison.Ordinal)
				.Replace("{WorldSeed}", TextValue(RequireSingleLine(server.WorldSeed, "WorldSeed"), "world_seed"), StringComparison.Ordinal)
				.Replace("{WorldSize}", ProbeNumber(server.WorldSize).ToString(), StringComparison.Ordinal)
				.Replace("{AppPort}", ProbeNumber(server.AppPort ?? 0).ToString(), StringComparison.Ordinal)
				.Replace("{LocalIP}", TextValue(RequireSingleLine(context.LocalIp, "LocalIP"), "local_ip"), StringComparison.Ordinal)
				.Replace("{PublicIP}", TextValue(RequireSingleLine(context.PublicIp, "PublicIP"), "public_ip"), StringComparison.Ordinal)
				.Replace("{IsPvp}", ProbeBoolean(string.Equals(server.GameMode, "PVP", StringComparison.OrdinalIgnoreCase)).ToString().ToLowerInvariant(), StringComparison.Ordinal)
				.Replace("{IsPve}", ProbeBoolean(string.Equals(server.GameMode, "PVE", StringComparison.OrdinalIgnoreCase)).ToString().ToLowerInvariant(), StringComparison.Ordinal)
				.Replace("{GameMode}", TextValue(RequireSingleLine(server.GameMode, "GameMode").ToLowerInvariant(), "game_mode"), StringComparison.Ordinal);
		}

		private (bool Changed, IReadOnlyList<string> Missing) UpdateManagedValues(
			string path,
			string template,
			string expandedTemplate,
			ConfigurationContext context)
		{
			List<ConfigLine> desiredValues =
				ConfigHandler.LoadConfigText(expandedTemplate, Format);
			Dictionary<string, ConfigLine> probeValues =
				ConfigHandler.LoadConfigText(
					ExpandTemplate(template, context, true),
					Format)
				.ToDictionary(value => value.Id, StringComparer.Ordinal);
			List<ConfigLine> managedValues = desiredValues
				.Where(value =>
					probeValues.TryGetValue(value.Id, out ConfigLine? probe) &&
					!TemplateValuesMatch(value, probe))
				.ToList();

			if (managedValues.Count == 0)
			{
				return (false, []);
			}

			List<ConfigLine> existingValues = ConfigHandler.LoadConfig(path, Format);
			Dictionary<string, ConfigLine> existingById = existingValues
				.ToDictionary(value => value.Id, StringComparer.Ordinal);
			List<string> missing = [];
			bool changed = false;

			foreach (ConfigLine desired in managedValues)
			{
				if (!existingById.TryGetValue(desired.Id, out ConfigLine? existing) ||
					!string.Equals(existing.Key, desired.Key, StringComparison.Ordinal) ||
					!string.Equals(existing.Path, desired.Path, StringComparison.Ordinal) ||
					existing.Type != desired.Type)
				{
					missing.Add(string.IsNullOrWhiteSpace(desired.Path)
						? desired.Key
						: desired.Path);
					continue;
				}

				if (TemplateValuesMatch(existing, desired))
				{
					continue;
				}

				existing.Value = desired.Value;
				changed = true;
			}

			if (changed)
			{
				ConfigHandler.SaveConfig(path, existingValues, Format);
			}

			return (changed, missing);
		}

		private static bool TemplateValuesMatch(ConfigLine first, ConfigLine second)
		{
			if (first.Type == ConfigValueType.Boolean &&
				bool.TryParse(first.Value, out bool firstBoolean) &&
				bool.TryParse(second.Value, out bool secondBoolean))
			{
				return firstBoolean == secondBoolean;
			}

			return string.Equals(first.Value, second.Value, StringComparison.Ordinal);
		}

		private ManagedConfigurationInput GetSupportedInputs()
		{
			ManagedConfigurationInput supported = ManagedConfigurationInput.None;
			foreach (ConfigurationTemplate template in Templates)
			{
				string content = template.Content;
				if (content.Contains("{Password}", StringComparison.Ordinal))
					supported |= ManagedConfigurationInput.ServerPassword;
				if (content.Contains("{AdminPassword}", StringComparison.Ordinal))
					supported |= ManagedConfigurationInput.AdminPassword;
				if (content.Contains("{WorldSeed}", StringComparison.Ordinal))
					supported |= ManagedConfigurationInput.WorldSeed;
				if (content.Contains("{GameMode}", StringComparison.Ordinal) ||
					content.Contains("{IsPvp}", StringComparison.Ordinal) ||
					content.Contains("{IsPve}", StringComparison.Ordinal))
					supported |= ManagedConfigurationInput.GameMode;
				if (content.Contains("{MaxPlayers}", StringComparison.Ordinal))
					supported |= ManagedConfigurationInput.MaxPlayers;
				if (content.Contains("{QueryPort}", StringComparison.Ordinal))
					supported |= ManagedConfigurationInput.QueryPort;
				if (content.Contains("{WorldName}", StringComparison.Ordinal))
					supported |= ManagedConfigurationInput.WorldName;
				if (content.Contains("{RCONPort}", StringComparison.Ordinal) ||
					content.Contains("{RCONPassword}", StringComparison.Ordinal) ||
					content.Contains("{EnableRcon}", StringComparison.Ordinal))
				{
					supported |= ManagedConfigurationInput.Rcon;
				}
				if (content.Contains("{WorldSize}", StringComparison.Ordinal))
					supported |= ManagedConfigurationInput.WorldSize;
				if (content.Contains("{Port}", StringComparison.Ordinal))
					supported |= ManagedConfigurationInput.Port;
				if (content.Contains("{AppPort}", StringComparison.Ordinal))
					supported |= ManagedConfigurationInput.AppPort;
			}

			return supported;
		}
	}
}
