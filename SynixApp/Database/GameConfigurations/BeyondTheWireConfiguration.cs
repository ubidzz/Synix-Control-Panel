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
namespace Synix_Control_Panel.SynixApp.Database.GameConfigurations
{
	internal sealed class BeyondTheWireConfiguration : TemplateConfigurationDefinition
	{
		private static readonly ConfigurationTemplate[] Files =
		[
			new(@"BeyondTheWire\ServerConfig\Server.cfg",
				"""
				ServerName="{ServerName}"
				ServerPassword="{Password}"
				MaxPlayers={MaxPlayers}
				NumReservedSlots=0
				IsLANMatch=false
				ShouldAdvertise=true
				NumPlayersDiffForTeamChanges=3
				AllowTeamChanges=true
				PreventTeamChangeIfUnbalanced=true
				EnforceTeamBalance=true
				RejoinSquadDelayAfterKick=180
				RecordDemos=false
				ServerMessageInterval=300
				VehicleClaimingDisabled=false
				AllowCommunityAdminAccess=false
				AllowDevProfiling=false
				PublicQueueLimit=10
				""")
		];

		public override string GameName => "Beyond the Wire";
		public override int SchemaVersion => 2;
		protected override IReadOnlyList<ConfigurationTemplate> Templates => Files;
	}
}
