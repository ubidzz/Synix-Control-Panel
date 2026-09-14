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
	internal sealed class HoldfastConfiguration : ConfigurationDefinition
	{
		private static readonly ConfigurationBinding[] ManagedBindings =
		[
			new("server_name", context => context.Server.ServerName, serverField: ConfigurationServerField.ServerName),
			new("server_password", context => context.Passwords.ServerPassword, serverField: ConfigurationServerField.Password),
			new("server_admin_password", context => context.Passwords.AdminPassword, serverField: ConfigurationServerField.AdminPassword),
			new("server_port", context => context.Server.Port.ToString(), serverField: ConfigurationServerField.Port),
			new("steam_query_port", context => context.Server.QueryPort.ToString(), serverField: ConfigurationServerField.QueryPort),
			new("maximum_players", context => context.Server.MaxPlayers.ToString(), serverField: ConfigurationServerField.MaxPlayers)
		];

		public override string GameName => "Holdfast: Nations At War";
		public override int SchemaVersion => 3;
		public override bool SupportsFullReset => true;
		public override bool PreservesInstalledTemplate => true;
		public override ManagedConfigurationInput SupportedInputs =>
			ManagedConfigurationInput.ServerPassword |
			ManagedConfigurationInput.AdminPassword |
			ManagedConfigurationInput.MaxPlayers |
			ManagedConfigurationInput.QueryPort |
			ManagedConfigurationInput.Port;
		public override string RelativePath =>
			@"Holdfast NaW_Data\StreamingAssets\Config\serverConfig_Core.txt";
		public override ConfigFormat Format => ConfigFormat.Space;
		public override IReadOnlyList<ConfigurationBinding> Bindings => ManagedBindings;
	}
}
