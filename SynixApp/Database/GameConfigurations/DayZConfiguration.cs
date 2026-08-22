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
	internal sealed class DayZConfiguration : TemplateConfigurationDefinition
	{
		private static readonly ConfigurationTemplate[] Files =
		[
			new("serverDZ.cfg",
				"""
				hostname = "{ServerName}";
				password = "{Password}";
				passwordAdmin = "{AdminPassword}";
				maxPlayers = {MaxPlayers};
				verifySignatures = 2;
				forceSameBuild = 1;
				disableVoN = 0;
				vonCodecQuality = 20;
				persistent = 1;
				guaranteedUpdates = 1;
				instanceId = 1;
				storageAutoFix = 1;

				class Missions
				{
				    class DayZ
				    {
				        template = "dayzOffline.chernarusplus";
				    };
				};
				""")
		];

		public override string GameName => "DayZ";
		protected override IReadOnlyList<ConfigurationTemplate> Templates => Files;
	}
}

