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
	internal sealed class FoundryConfiguration : TemplateConfigurationDefinition
	{
		private static readonly ConfigurationTemplate[] Files =
		[
			new("app.cfg",
				"""
				server_name={ServerName}
				server_description=Hosted using Synix
				server_world_name={Identity}
				server_port={Port}
				server_query_port={QueryPort}
				server_is_public=true
				server_max_players={MaxPlayers}
				server_password={Password}
				server_autosave_interval=300
				server_save_slots=10
				server_pause_when_empty=true
				""")
		];

		public override string GameName => "Foundry";
		protected override IReadOnlyList<ConfigurationTemplate> Templates => Files;
	}
}

