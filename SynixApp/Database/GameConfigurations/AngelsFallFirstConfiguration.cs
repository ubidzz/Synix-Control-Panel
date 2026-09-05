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
	internal sealed class AngelsFallFirstConfiguration : ConfigurationDefinition
	{
		private static readonly ConfigurationBinding[] ManagedBindings =
		[
			new("AdminPassword", context => context.Passwords.AdminPassword,
				"[Engine.AccessControl] / AdminPassword"),
			new("MaxPlayers", context => context.Server.MaxPlayers.ToString(),
				"[Engine.GameInfo] / MaxPlayers"),
			new("MaxPlayers", context => context.Server.MaxPlayers.ToString(),
				"[AFFGame.AFFGameInfo_Incursion] / MaxPlayers"),
			new("ServerName", context => context.Server.ServerName,
				"[Engine.GameReplicationInfo] / ServerName"),
			new("ServerName", context => context.Server.ServerName,
				"[AFFGame.AFFGameReplicationInfo] / ServerName")
		];

		public override string GameName => "Angels Fall First";
		public override int SchemaVersion => 2;
		public override bool PreservesInstalledTemplate => true;
		public override ManagedConfigurationInput SupportedInputs =>
			ManagedConfigurationInput.AdminPassword |
			ManagedConfigurationInput.MaxPlayers |
			ManagedConfigurationInput.ServerName;
		public override string RelativePath => @"AFFGame\Config\PCServer-AFFGame.ini";
		public override ConfigFormat Format => ConfigFormat.StandardINI;
		public override IReadOnlyList<ConfigurationBinding> Bindings => ManagedBindings;
	}
}
