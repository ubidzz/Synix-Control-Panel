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
	internal sealed class AmericanTruckSimulatorConfiguration : ConfigurationDefinition
	{
		private static readonly ConfigurationBinding[] ManagedBindings =
		[
			new("lobby_name", context => context.Server.ServerName),
			new("password", context => context.Passwords.ServerPassword),
			new("max_players", context => context.Server.MaxPlayers.ToString()),
			new("connection_dedicated_port", context => context.Server.Port.ToString()),
			new("query_dedicated_port", context => context.Server.QueryPort.ToString())
		];

		public override string GameName => "American Truck Simulator";
		public override int SchemaVersion => 2;
		public override bool PreservesInstalledTemplate => true;
		public override ManagedConfigurationInput SupportedInputs =>
			ManagedConfigurationInput.ServerName |
			ManagedConfigurationInput.ServerPassword |
			ManagedConfigurationInput.MaxPlayers |
			ManagedConfigurationInput.Port |
			ManagedConfigurationInput.QueryPort;
		public override string RelativePath => "server_config.sii";
		public override ConfigFormat Format => ConfigFormat.SII;
		public override IReadOnlyList<ConfigurationBinding> Bindings => ManagedBindings;
	}
}
