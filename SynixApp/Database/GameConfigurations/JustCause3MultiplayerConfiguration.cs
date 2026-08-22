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
	internal sealed class JustCause3MultiplayerConfiguration : ConfigurationDefinition
	{
		private static readonly ConfigurationBinding[] ManagedBindings =
		[
			new("name", context => context.Server.ServerName),
			new("password", context => context.Passwords.ServerPassword),
			new("maxPlayers", context => context.Server.MaxPlayers.ToString()),
			new("port", context => context.Server.Port.ToString()),
			new("queryPort", context => context.Server.QueryPort.ToString())
		];

		public override string GameName => "Just Cause 3 Multiplayer";
		public override int SchemaVersion => 2;
		public override bool SupportsFullReset => true;
		public override bool PreservesInstalledTemplate => true;
		public override ManagedConfigurationInput SupportedInputs =>
			ManagedConfigurationInput.ServerPassword |
			ManagedConfigurationInput.MaxPlayers |
			ManagedConfigurationInput.QueryPort |
			ManagedConfigurationInput.Port;
		public override string RelativePath => "config.json";
		public override ConfigFormat Format => ConfigFormat.JSON;
		public override IReadOnlyList<ConfigurationBinding> Bindings => ManagedBindings;
	}
}
