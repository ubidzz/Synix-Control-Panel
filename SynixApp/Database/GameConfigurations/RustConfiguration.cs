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
	internal sealed class RustConfiguration : ConfigurationDefinition
	{
		private static readonly ConfigurationBinding[] ManagedBindings =
		[
			new("server.hostname", context => context.Server.ServerName),
			new("server.maxplayers", context => context.Server.MaxPlayers.ToString()),
			new("server.seed", context => string.IsNullOrWhiteSpace(context.Server.WorldSeed) ? "12345" : context.Server.WorldSeed),
			new("server.worldsize", context => (context.Server.WorldSize > 0 ? context.Server.WorldSize : 4000).ToString()),
			new("server.level", context => string.IsNullOrWhiteSpace(context.Server.WorldName) ? "Procedural Map" : context.Server.WorldName),
			new("server.pve", context => string.Equals(context.Server.GameMode, "PVE", StringComparison.OrdinalIgnoreCase).ToString()),
			new("rcon.port", context => context.Server.RconPort.ToString()),
			new("rcon.password", context => context.Server.EnableRcon ? context.Passwords.RconPassword : string.Empty),
			new("rcon.web", context => bool.TrueString)
		];

		public override string GameName => "Rust";
		public override string RelativePath => @"server\{Identity}\cfg\server.cfg";
		public override ConfigFormat Format => ConfigFormat.Space;
		public override IReadOnlyList<ConfigurationBinding> Bindings => ManagedBindings;

		public override string CreateTemplate(ConfigurationContext context)
		{
			string serverName = EscapeQuoted(context.Server.ServerName);
			string worldSeed = RequireSingleLine(
				string.IsNullOrWhiteSpace(context.Server.WorldSeed) ? "12345" : context.Server.WorldSeed,
				"WorldSeed");
			string level = EscapeQuoted(
				string.IsNullOrWhiteSpace(context.Server.WorldName) ? "Procedural Map" : context.Server.WorldName);
			string rconPassword = EscapeQuoted(
				context.Server.EnableRcon ? context.Passwords.RconPassword : string.Empty);
			bool isPve = string.Equals(context.Server.GameMode, "PVE", StringComparison.OrdinalIgnoreCase);

			return string.Join("\n",
				$"server.hostname \"{serverName}\"",
				$"server.description \"Welcome to {serverName}! Managed via Synix Control Panel.\"",
				"server.url \"https://github.com/ubidzz/Synix-Control-Panel\"",
				"server.headerimage \"\"",
				"server.tags \"vanilla\"",
				$"server.maxplayers {context.Server.MaxPlayers}",
				$"server.seed {worldSeed}",
				$"server.worldsize {(context.Server.WorldSize > 0 ? context.Server.WorldSize : 4000)}",
				$"server.level \"{level}\"",
				"server.saveinterval 300",
				$"server.pve {isPve.ToString().ToLowerInvariant()}",
				"server.globalchat true",
				"server.airdropminplayers 10",
				"server.stability true",
				"server.radiation true",
				"craft.instant false",
				"decay.upkeep true",
				"decay.scale 1.0",
				"fps.limit 60",
				"server.tickrate 30",
				"gc.buffer 256",
				"antihack.enabled true",
				"server.secure true",
				"server.official false",
				$"rcon.port {context.Server.RconPort}",
				$"rcon.password \"{rconPassword}\"",
				"rcon.web true",
				string.Empty);
		}
	}
}
