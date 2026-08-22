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
				server_world_name={Identity}
				server_password={Password}
				pause_server_when_empty=true
				autosave_interval=300
				server_is_public=true
				server_port={Port}
				server_query_port={QueryPort}
				mapseed={WorldSeed}
				server_persistent_data_override_folder=
				server_name={ServerName}
				server_max_players={MaxPlayers}
				""")
		];

		public override string GameName => "Foundry";
		public override int SchemaVersion => 2;
		protected override IReadOnlyList<ConfigurationTemplate> Templates => Files;
	}
}
