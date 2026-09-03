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
	internal sealed class EcoConfiguration : ConfigurationDefinition
	{
		private const string TemplateSuffix = ".template";
		private static readonly ConfigurationBinding[] ManagedBindings =
		[
			new("Name", context => context.Server.ServerName),
			new("Password", context => context.Passwords.ServerPassword),
			new("GameServerPort", context => context.Server.Port.ToString()),
			new("WebServerPort", context => context.Server.QueryPort.ToString()),
			new("RconServerPort", context => context.Server.RconPort.ToString()),
			new("RconPassword", context =>
				context.Server.EnableRcon ? context.Passwords.RconPassword : string.Empty),
			new("DefaultSlots", context => context.Server.MaxPlayers.ToString())
		];

		public override string GameName => "Eco";
		public override int SchemaVersion => 3;
		public override bool SupportsFullReset => true;
		public override ManagedConfigurationInput SupportedInputs =>
			ManagedConfigurationInput.ServerPassword |
			ManagedConfigurationInput.MaxPlayers |
			ManagedConfigurationInput.QueryPort |
			ManagedConfigurationInput.Rcon |
			ManagedConfigurationInput.Port;
		public override string RelativePath => @"Configs\Network.eco";
		public override ConfigFormat Format => ConfigFormat.JSON;
		public override IReadOnlyList<ConfigurationBinding> Bindings => ManagedBindings;

		public override string? CreateTemplate(ConfigurationContext context)
		{
			string sourcePath = ResolveFullPath(
				context.Server,
				@"Configs\Network.eco.template");
			return File.Exists(sourcePath)
				? File.ReadAllText(sourcePath)
				: null;
		}

		internal override void PrepareConfigurationFilesForEditing(GameServer server)
		{
			string configDirectory = GetConfigurationDirectory(server);
			if (!Directory.Exists(configDirectory))
				return;

			foreach (string templatePath in Directory
				.EnumerateFiles(configDirectory, "*", SearchOption.TopDirectoryOnly)
				.Where(path => path.EndsWith(
					".eco.template",
					StringComparison.OrdinalIgnoreCase))
				.OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase))
			{
				string editablePath = templatePath[..^TemplateSuffix.Length];
				if (File.Exists(editablePath))
					continue;

				WriteNewFile(editablePath, File.ReadAllText(templatePath));
			}
		}

		internal override IReadOnlyList<string> ResolveConfigurationPaths(
			GameServer server)
		{
			string primaryPath = ResolveFullPath(server);
			string configDirectory = GetConfigurationDirectory(server);
			List<string> paths = [primaryPath];
			if (!Directory.Exists(configDirectory))
				return paths;

			paths.AddRange(Directory
				.EnumerateFiles(configDirectory, "*", SearchOption.TopDirectoryOnly)
				.Where(path => path.EndsWith(".eco", StringComparison.OrdinalIgnoreCase))
				.Where(path => !path.Equals(primaryPath, StringComparison.OrdinalIgnoreCase))
				.OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase));
			return paths;
		}

		private string GetConfigurationDirectory(GameServer server)
		{
			return Path.GetDirectoryName(ResolveFullPath(server))
				?? throw new InvalidOperationException(
					"The Eco configuration directory is unavailable.");
		}
	}
}
