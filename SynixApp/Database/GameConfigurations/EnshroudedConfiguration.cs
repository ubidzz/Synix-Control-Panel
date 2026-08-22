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
	internal sealed class EnshroudedConfiguration : TemplateConfigurationDefinition
	{
		private static readonly ConfigurationTemplate[] Files =
		[
			new("enshrouded_server.json",
				"""
				{
				  "name": "{ServerName}",
				  "saveDirectory": "./savegame",
				  "logDirectory": "./logs",
				  "ip": "0.0.0.0",
				  "queryPort": {QueryPort},
				  "slotCount": {MaxPlayers},
				  "tags": [],
				  "voiceChatMode": "Proximity",
				  "enableVoiceChat": false,
				  "enableTextChat": false,
				  "gameSettingsPreset": "Default",
				  "gameSettings": {
				    "playerHealthFactor": 1,
				    "playerManaFactor": 1,
				    "playerStaminaFactor": 1,
				    "playerBodyHeatFactor": 1,
				    "playerDivingTimeFactor": 1,
				    "enableDurability": true,
				    "enableStarvingDebuff": false,
				    "foodBuffDurationFactor": 1,
				    "fromHungerToStarving": 600000000000,
				    "shroudTimeFactor": 1,
				    "tombstoneMode": "AddBackpackMaterials",
				    "enableGliderTurbulences": true,
				    "weatherFrequency": "Normal",
				    "fishingDifficulty": "Normal",
				    "miningDamageFactor": 1,
				    "plantGrowthSpeedFactor": 1,
				    "resourceDropStackAmountFactor": 1,
				    "factoryProductionSpeedFactor": 1,
				    "perkUpgradeRecyclingFactor": 0.5,
				    "perkCostFactor": 1,
				    "experienceCombatFactor": 1,
				    "experienceMiningFactor": 1,
				    "experienceExplorationQuestsFactor": 1,
				    "randomSpawnerAmount": "Normal",
				    "aggroPoolAmount": "Normal",
				    "enemyDamageFactor": 1,
				    "enemyHealthFactor": 1,
				    "enemyStaminaFactor": 1,
				    "enemyPerceptionRangeFactor": 1,
				    "bossDamageFactor": 1,
				    "bossHealthFactor": 1,
				    "threatBonus": 1,
				    "pacifyAllEnemies": false,
				    "tamingStartleRepercussion": "LoseSomeProgress",
				    "dayTimeDuration": 1800000000000,
				    "nightTimeDuration": 720000000000,
				    "curseModifier": "Normal"
				  },
				  "userGroups": [
				    {
				      "name": "Player",
				      "password": "{Password}",
				      "canKickBan": false,
				      "canAccessInventories": true,
				      "canEditWorld": true,
				      "canEditBase": true,
				      "canExtendBase": false,
				      "reservedSlots": 0
				    }
				  ],
				  "bans": []
				}
				""")
		];

		public override string GameName => "Enshrouded";
		public override int SchemaVersion => 2;
		protected override IReadOnlyList<ConfigurationTemplate> Templates => Files;
	}
}
