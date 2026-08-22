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
	internal sealed class BannerlordConfiguration : TemplateConfigurationDefinition
	{
		private static readonly ConfigurationTemplate[] Files =
		[
			new(@"Modules\Native\CustomServerconfig.txt",
				"""
				ServerName {ServerName}
				GamePassword {Password}
				AdminPassword {AdminPassword}
				GameType TeamDeathmatch
				MaxNumberOfPlayers {MaxPlayers}
				start_game_and_mission
				""")
		];

		public override string GameName => "Mount & Blade II: Bannerlord";
		protected override IReadOnlyList<ConfigurationTemplate> Templates => Files;
	}
}

