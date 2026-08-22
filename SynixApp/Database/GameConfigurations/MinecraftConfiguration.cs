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
	internal sealed class MinecraftConfiguration : ConfigurationDefinition
	{
		private static readonly ConfigurationBinding[] ManagedBindings =
		[
			new("motd", context => EscapeProperty(context.Server.ServerName)),
			new("server-port", context => context.Server.Port.ToString()),
			new("query.port", context => context.Server.QueryPort.ToString()),
			new("max-players", context => context.Server.MaxPlayers.ToString()),
			new("level-name", context => EscapeProperty(string.IsNullOrWhiteSpace(context.Server.WorldName) ? "world" : context.Server.WorldName)),
			new("level-seed", context => EscapeProperty(context.Server.WorldSeed)),
			new("enable-rcon", context => context.Server.EnableRcon.ToString()),
			new("rcon.port", context => context.Server.RconPort.ToString()),
			new("rcon.password", context => EscapeProperty(context.Passwords.RconPassword))
		];

		public override string GameName => "Minecraft";
		public override string RelativePath => "server.properties";
		public override ConfigFormat Format => ConfigFormat.StandardINI;
		public override IReadOnlyList<ConfigurationBinding> Bindings => ManagedBindings;

		public override string CreateTemplate(ConfigurationContext context)
		{
			return string.Join("\n",
				$"motd={EscapeProperty(context.Server.ServerName)}",
				$"server-port={context.Server.Port}",
				$"query.port={context.Server.QueryPort}",
				$"max-players={context.Server.MaxPlayers}",
				$"level-name={EscapeProperty(string.IsNullOrWhiteSpace(context.Server.WorldName) ? "world" : context.Server.WorldName)}",
				$"level-seed={EscapeProperty(context.Server.WorldSeed)}",
				$"enable-rcon={context.Server.EnableRcon.ToString().ToLowerInvariant()}",
				$"rcon.port={context.Server.RconPort}",
				$"rcon.password={EscapeProperty(context.Passwords.RconPassword)}",
				string.Empty);
		}
	}
}
