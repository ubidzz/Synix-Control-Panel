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
	internal sealed class SonsOfTheForestConfiguration : TemplateConfigurationDefinition
	{
		private static readonly ConfigurationTemplate[] Files =
		[
			new(@"userdata\dedicated_server.cfg",
				"""{ "ServerName": "{ServerName}", "MaxPlayers": {MaxPlayers}, "ServerPlayMode": "Normal" }""")
		];

		public override string GameName => "Sons Of The Forest";
		protected override IReadOnlyList<ConfigurationTemplate> Templates => Files;
	}
}

