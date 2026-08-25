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
			new("enable-query", _ => bool.TrueString),
			new("query.port", context => context.Server.QueryPort.ToString()),
			new("max-players", context => context.Server.MaxPlayers.ToString()),
			new("level-name", context => EscapeProperty(string.IsNullOrWhiteSpace(context.Server.WorldName) ? "world" : context.Server.WorldName)),
			new("level-seed", context => EscapeProperty(context.Server.WorldSeed)),
			new("enable-rcon", context => context.Server.EnableRcon.ToString().ToLowerInvariant()),
			new("rcon.port", context => context.Server.RconPort.ToString()),
			new("rcon.password", context => EscapeProperty(context.Passwords.RconPassword))
		];

		public override string GameName => "Minecraft";
		public override int SchemaVersion => 3;
		public override bool SupportsFullReset => true;
		public override ManagedConfigurationInput SupportedInputs =>
			ManagedConfigurationInput.WorldSeed |
			ManagedConfigurationInput.MaxPlayers |
			ManagedConfigurationInput.QueryPort |
			ManagedConfigurationInput.WorldName |
			ManagedConfigurationInput.Rcon |
			ManagedConfigurationInput.Port;
		public override string RelativePath => "server.properties";
		public override ConfigFormat Format => ConfigFormat.StandardINI;
		public override IReadOnlyList<ConfigurationBinding> Bindings => ManagedBindings;

		public override string CreateTemplate(ConfigurationContext context)
		{
			return string.Join("\n",
				"accepts-transfers=false",
				"allow-flight=false",
				"broadcast-console-to-ops=true",
				"broadcast-rcon-to-ops=true",
				"bug-report-link=",
				"chat-spam-threshold-seconds=10",
				"command-spam-threshold-seconds=10",
				"difficulty=easy",
				"enable-code-of-conduct=false",
				"enable-jmx-monitoring=false",
				"enable-query=true",
				$"enable-rcon={context.Server.EnableRcon.ToString().ToLowerInvariant()}",
				"enable-status=true",
				"enforce-secure-profile=true",
				"enforce-whitelist=false",
				"entity-broadcast-range-percentage=100",
				"force-gamemode=false",
				"function-permission-level=2",
				"gamemode=survival",
				"generate-structures=true",
				"generator-settings={}",
				"hardcore=false",
				"hide-online-players=false",
				"initial-disabled-packs=",
				"initial-enabled-packs=vanilla",
				$"level-name={EscapeProperty(string.IsNullOrWhiteSpace(context.Server.WorldName) ? "world" : context.Server.WorldName)}",
				$"level-seed={EscapeProperty(context.Server.WorldSeed)}",
				"level-type=minecraft\\:normal",
				"log-ips=true",
				"management-server-allowed-origins=",
				"management-server-enabled=false",
				"management-server-host=localhost",
				"management-server-port=0",
				"management-server-secret=",
				"management-server-tls-enabled=true",
				"management-server-tls-keystore=",
				"management-server-tls-keystore-password=",
				"max-chained-neighbor-updates=1000000",
				$"max-players={context.Server.MaxPlayers}",
				"max-tick-time=60000",
				"max-world-size=29999984",
				$"motd={EscapeProperty(context.Server.ServerName)}",
				"network-compression-threshold=256",
				"online-mode=true",
				"op-permission-level=4",
				"pause-when-empty-seconds=60",
				"player-idle-timeout=0",
				"prevent-proxy-connections=false",
				$"query.port={context.Server.QueryPort}",
				"rate-limit=0",
				$"rcon.password={EscapeProperty(context.Passwords.RconPassword)}",
				$"rcon.port={context.Server.RconPort}",
				"region-file-compression=deflate",
				"require-resource-pack=false",
				"resource-pack=",
				"resource-pack-id=",
				"resource-pack-prompt=",
				"resource-pack-sha1=",
				"server-ip=",
				$"server-port={context.Server.Port}",
				"simulation-distance=10",
				"spawn-protection=16",
				"status-heartbeat-interval=0",
				"sync-chunk-writes=true",
				"text-filtering-config=",
				"text-filtering-version=0",
				"use-native-transport=true",
				"view-distance=10",
				"white-list=false",
				string.Empty);
		}
	}
}
