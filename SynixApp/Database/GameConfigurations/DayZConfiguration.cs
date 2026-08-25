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
				description = "Managed by Synix";
				password = "{Password}";
				passwordAdmin = "{AdminPassword}";
				enableWhitelist = 0;
				disableBanlist = false;
				disablePrioritylist = false;
				maxPlayers = {MaxPlayers};
				verifySignatures = 2;
				forceSameBuild = 1;
				disableVoN = 0;
				vonCodecQuality = 20;
				disable3rdPerson = 0;
				disableCrosshair = 0;
				serverTime = "SystemTime";
				serverTimeAcceleration = 1;
				serverNightTimeAcceleration = 1;
				serverTimePersistent = 0;
				guaranteedUpdates = 1;
				loginQueueConcurrentPlayers = 5;
				loginQueueMaxPlayers = 500;
				instanceId = 1;
				storageAutoFix = 1;
				respawnTime = 5;
				motd[] = { "Welcome to {ServerName}" };
				motdInterval = 1;
				timeStampFormat = "Short";
				logAverageFps = 60;
				logMemory = 60;
				logPlayers = 60;
				logFile = "server_console.log";
				adminLogPlayerHitsOnly = 0;
				defaultObjectViewDistance = 1375;
				lightingConfig = 0;
				disablePersonalLight = 1;
				disableBaseDamage = 0;
				disableContainerDamage = 0;
				disableRespawnDialog = 0;
				pingWarning = 200;
				pingCritical = 250;
				MaxPing = 300;
				serverFpsWarning = 15;
				shotValidation = 1;
				clientPort = 2304;

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
		public override int SchemaVersion => 2;
		protected override IReadOnlyList<ConfigurationTemplate> Templates => Files;
	}
}
