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

namespace Synix_Control_Panel.SynixApp.Database.GameConfigurations
{
	internal readonly record struct ConfigurationTemplate(
		string RelativePath,
		string Content);

	internal abstract class TemplateConfigurationDefinition : ConfigurationDefinition
	{
		protected abstract IReadOnlyList<ConfigurationTemplate> Templates { get; }

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
				foreach (ConfigurationTemplate template in Templates)
				{
					string fullPath = ResolveFullPath(context.Server, template.RelativePath);
					if (File.Exists(fullPath))
					{
						continue;
					}

					WriteNewFile(fullPath, ExpandTemplate(template.Content, context));
					created = true;
				}

				return new ConfigurationApplyResult(
					true,
					true,
					created,
					created,
					created
						? $"Created the required {GameName} configuration files."
						: $"The {GameName} configuration files are already present.");
			}
			catch (Exception ex)
			{
				return ConfigurationApplyResult.Failure(
					$"The {GameName} configuration could not be created: {ex.Message}");
			}
		}

		public override bool ConfigurationFileExists(GameServer server)
		{
			return Templates.All(template =>
				File.Exists(ResolveFullPath(server, template.RelativePath)));
		}

		private static string ExpandTemplate(
			string template,
			ConfigurationContext context)
		{
			GameServer server = context.Server;
			return template
				.Replace("{ServerName}", RequireSingleLine(server.ServerName, "ServerName"), StringComparison.Ordinal)
				.Replace("{Password}", RequireSingleLine(context.Passwords.ServerPassword, "Password"), StringComparison.Ordinal)
				.Replace("{AdminPassword}", RequireSingleLine(context.Passwords.AdminPassword, "AdminPassword"), StringComparison.Ordinal)
				.Replace("{MaxPlayers}", server.MaxPlayers.ToString(), StringComparison.Ordinal)
				.Replace("{Port}", server.Port.ToString(), StringComparison.Ordinal)
				.Replace("{QueryPort}", server.QueryPort.ToString(), StringComparison.Ordinal)
				.Replace("{RCONPort}", server.RconPort.ToString(), StringComparison.Ordinal)
				.Replace("{RCONPassword}", RequireSingleLine(context.Passwords.RconPassword, "RCONPassword"), StringComparison.Ordinal)
				.Replace("{EnableRcon}", server.EnableRcon.ToString().ToLowerInvariant(), StringComparison.Ordinal)
				.Replace("{Identity}", context.Identity, StringComparison.Ordinal)
				.Replace("{WorldName}", RequireSingleLine(server.WorldName, "WorldName"), StringComparison.Ordinal)
				.Replace("{WorldSeed}", RequireSingleLine(server.WorldSeed, "WorldSeed"), StringComparison.Ordinal)
				.Replace("{WorldSize}", server.WorldSize.ToString(), StringComparison.Ordinal)
				.Replace("{AppPort}", (server.AppPort ?? 0).ToString(), StringComparison.Ordinal)
				.Replace("{LocalIP}", RequireSingleLine(context.LocalIp, "LocalIP"), StringComparison.Ordinal)
				.Replace("{PublicIP}", RequireSingleLine(context.PublicIp, "PublicIP"), StringComparison.Ordinal)
				.Replace("{GameMode}", RequireSingleLine(server.GameMode, "GameMode").ToLowerInvariant(), StringComparison.Ordinal);
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
				if (content.Contains("{GameMode}", StringComparison.Ordinal))
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
