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
				"""
				{
				  "IpAddress": "0.0.0.0",
				  "GamePort": {Port},
				  "QueryPort": {QueryPort},
				  "BlobSyncPort": {AppPort},
				  "ServerName": "{ServerName}",
				  "MaxPlayers": {MaxPlayers},
				  "Password": "{Password}",
				  "LanOnly": false,
				  "SaveSlot": 1,
				  "SaveMode": "Continue",
				  "GameMode": "{GameMode}",
				  "SaveInterval": 600,
				  "IdleDayCycleSpeed": 0.0,
				  "IdleTargetFramerate": 5,
				  "ActiveTargetFramerate": 60,
				  "LogFilesEnabled": true,
				  "TimestampLogFilenames": true,
				  "TimestampLogEntries": true,
				  "SkipNetworkAccessibilityTest": false,
				  "GameSettings": {
				    "Gameplay.TreeRegrowth": true,
				    "Structure.Damage": true
				  },
				  "CustomGameModeSettings": {
				    "GameSetting.Multiplayer.Cheats": false,
				    "GameSetting.Multiplayer.PvpDamage": "Normal",
				    "GameSetting.Vail.EnemySpawn": true,
				    "GameSetting.Vail.EnemyHealth": "Normal",
				    "GameSetting.Vail.EnemyDamage": "Normal",
				    "GameSetting.Vail.EnemyArmour": "Normal",
				    "GameSetting.Vail.EnemyAggression": "Normal",
				    "GameSetting.Vail.AnimalSpawnRate": "Normal",
				    "GameSetting.Vail.EnemySearchParties": "Normal",
				    "GameSetting.Environment.StartingSeason": "Summer",
				    "GameSetting.Environment.SeasonLength": "Default",
				    "GameSetting.Environment.DayLength": "Default",
				    "GameSetting.Environment.PrecipitationFrequency": "Default",
				    "GameSetting.Survival.ConsumableEffects": "Normal",
				    "GameSetting.Survival.PlayerStatsDamage": "Off",
				    "GameSetting.Survival.ColdPenalties": "Off",
				    "GameSetting.Survival.StatRegenerationPenalty": "Off",
				    "GameSetting.Survival.ReducedFoodInContainers": false,
				    "GameSetting.Survival.SingleUseContainers": true,
				    "GameSetting.Survival.BuildingResistance": "Normal",
				    "GameSetting.Survival.CreativeMode": false,
				    "GameSetting.Survival.PlayersImmortalMode": false,
				    "GameSetting.FreeForm.ForcePlaceFullLoad": false,
				    "GameSetting.Construction.NoCuttingsSpawn": false,
				    "GameSetting.Survival.OneHitToCutTrees": false
				  }
				}
				""")
		];

		public override string GameName => "Sons Of The Forest";
		public override int SchemaVersion => 2;
		protected override IReadOnlyList<ConfigurationTemplate> Templates => Files;
	}
}
