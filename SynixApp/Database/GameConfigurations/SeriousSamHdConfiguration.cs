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
	internal sealed class SeriousSamHdConfiguration : TemplateConfigurationDefinition
	{
		private static readonly ConfigurationTemplate[] Files =
		[
			new("server.cfg",
				"""
				rconpass = "{AdminPassword}";
				sessionname = "{ServerName}"
				gam_ctMaxPlayers = {MaxPlayers}
				gamemode = "Cooperative"
				gam_bAutoCycleMaps = 1
				""")
		];

		private static readonly string[] GameAliases =
		[
			"Serious Sam HD: The First Encounter",
			"Serious Sam 3: BFE"
		];

		public override string GameName => "Serious Sam HD: The Second Encounter";
		public override IReadOnlyList<string> Aliases => GameAliases;
		protected override IReadOnlyList<ConfigurationTemplate> Templates => Files;
	}
}

