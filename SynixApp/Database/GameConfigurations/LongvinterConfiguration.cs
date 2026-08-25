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
	internal sealed class LongvinterConfiguration : TemplateConfigurationDefinition
	{
		private static readonly ConfigurationTemplate[] Files =
		[
			new(@"Longvinter\Saved\Config\WindowsServer\Game.ini",
				"""
				[/Game/Blueprints/Server/GI_AdvancedSessions.GI_AdvancedSessions_C]
				ServerName={ServerName}
				ServerMOTD=Welcome to {ServerName}
				MaxPlayers={MaxPlayers}
				Password={Password}
				CommunityWebsite=
				CoopPlay=false
				CheckVPN=true
				CoopSpawn=0
				Tag=none
				ChestRespawnTime=600
				DisableWanderingTraders=false
				ServerRegion=

				[/Game/Blueprints/Server/GM_Longvinter.GM_Longvinter_C]
				AdminSteamID=
				PVP={IsPvp}
				TentDecay=true
				MaxTents=2
				SaveBackups=false
				""")
		];

		public override string GameName => "Longvinter";
		public override int SchemaVersion => 2;
		protected override IReadOnlyList<ConfigurationTemplate> Templates => Files;
	}
}
