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
	internal sealed class StarRuptureConfiguration : TemplateConfigurationDefinition
	{
		private static readonly ConfigurationTemplate[] Files =
		[
			new("DSSettings.txt",
				"""{ "SessionName": "{ServerName}", "SaveGameInterval": "300", "StartNewGame": "true", "LoadSavedGame": "false", "SaveGameName": "AutoSave0.sav" }""")
		];

		public override string GameName => "StarRupture";
		protected override IReadOnlyList<ConfigurationTemplate> Templates => Files;
	}
}
