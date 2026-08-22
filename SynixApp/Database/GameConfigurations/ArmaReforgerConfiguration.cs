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
	internal sealed class ArmaReforgerConfiguration : TemplateConfigurationDefinition
	{
		private static readonly ConfigurationTemplate[] Files =
		[
			new(@"configs\server.json",
				"""
				{
				  "bindPort": {Port},
				  "publicPort": {Port},
				  "a2s": {
				    "address": "0.0.0.0",
				    "port": {QueryPort}
				  },
				  "game": {
				    "name": "{ServerName}",
				    "password": "{Password}",
				    "passwordAdmin": "{AdminPassword}",
				    "scenarioId": "{ECC61978EDCC2B5A}Missions/23_Campaign.conf",
				    "maxPlayers": {MaxPlayers},
				    "visible": true,
				    "crossPlatform": true,
				    "gameProperties": {
				      "fastValidation": true,
				      "battlEye": true
				    },
				    "mods": []
				  }
				}
				""")
		];

		public override string GameName => "Arma Reforger";
		protected override IReadOnlyList<ConfigurationTemplate> Templates => Files;
	}
}

