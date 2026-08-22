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
using Synix_Control_Panel.SynixApp.ServerHandler;

namespace Synix_Control_Panel.SynixApp.Database.GameConfigurations
{
	internal sealed class PalworldConfiguration : ConfigurationDefinition
	{
		private static readonly ConfigurationBinding[] ManagedBindings =
		[
			new("ServerName", context => context.Server.ServerName),
			new("AdminPassword", context => context.Passwords.AdminPassword),
			new("ServerPassword", context => context.Passwords.ServerPassword),
			new("ServerPlayerMaxNum", context => context.Server.MaxPlayers.ToString()),
			new("PublicPort", context => context.Server.Port.ToString()),
			new("RCONEnabled", context => context.Server.EnableRcon.ToString()),
			new("RCONPort", context => context.Server.RconPort.ToString()),
			new("RESTAPIPort", context => context.Server.QueryPort.ToString()),
			new("bIsPvP", context => string.Equals(context.Server.GameMode, "PVP", StringComparison.OrdinalIgnoreCase).ToString())
		];

		public override string GameName => "Palworld";
		public override bool RequiresNetworkAddresses => true;
		public override string RelativePath => @"Pal\Saved\Config\WindowsServer\PalWorldSettings.ini";
		public override ConfigFormat Format => ConfigFormat.StandardINI;
		public override IReadOnlyList<ConfigurationBinding> Bindings => ManagedBindings;

		public override string CreateTemplate(ConfigurationContext context)
		{
			string serverName = EscapeQuoted(context.Server.ServerName);
			string adminPassword = EscapeQuoted(context.Passwords.AdminPassword);
			string serverPassword = EscapeQuoted(context.Passwords.ServerPassword);
			string publicIp = EscapeQuoted(context.PublicIp);
			string rconEnabled = context.Server.EnableRcon.ToString();
			string isPvp = string.Equals(context.Server.GameMode, "PVP", StringComparison.OrdinalIgnoreCase).ToString();

			return "[/Script/Pal.PalGameWorldSettings]\n" +
				"OptionSettings=(" +
				$"ServerName=\"{serverName}\"," +
				"ServerDescription=\"Managed via Synix Control Panel\"," +
				$"AdminPassword=\"{adminPassword}\"," +
				$"ServerPassword=\"{serverPassword}\"," +
				$"ServerPlayerMaxNum={context.Server.MaxPlayers}," +
				$"PublicIP=\"{publicIp}\"," +
				$"PublicPort={context.Server.Port}," +
				$"RCONEnabled={rconEnabled}," +
				$"RCONPort={context.Server.RconPort}," +
				"RESTAPIEnabled=False," +
				$"RESTAPIPort={context.Server.QueryPort}," +
				"ChatPostLimitPerMinute=10," +
				"CrossplayPlatforms=(Steam,Xbox,PS5,Mac)," +
				"LogFormatType=\"Text\"," +
				"bIsShowJoinLeftMessage=True," +
				"bIsUseBackupSaveData=True," +
				"bEnableBuildingPlayerUidDisplay=True," +
				"bAllowClientMod=False," +
				"BaseCampMaxNum=128," +
				"BaseCampMaxNumInGuild=4," +
				"BaseCampWorkerMaxNum=15," +
				"ItemContainerForceMarkDirtyInterval=10.000000," +
				"MaxBuildingLimitNum=0," +
				"PhysicsActiveDropItemMaxNum=1000," +
				"ServerReplicatePawnCullDistance=15000.000000," +
				"AutoResetGuildTimeNoOnlinePlayers=72.000000," +
				"bAllowEnemyCampSpawnNearBaseCamp=True," +
				"bAllowEnhanceStat_Attack=True," +
				"bAllowEnhanceStat_Health=True," +
				"bAllowEnhanceStat_Stamina=True," +
				"bAllowEnhanceStat_Weight=True," +
				"bAllowEnhanceStat_WorkSpeed=True," +
				"bAllowGlobalPalboxExport=False," +
				"bAllowGlobalPalboxImport=False," +
				"bAutoResetGuildNoOnlinePlayers=False," +
				"bBuildAreaLimit=True," +
				"bCharacterRecreateInHardcore=False," +
				"bDisplayPvPItemNumOnWorldMap_BaseCamp=False," +
				"bDisplayPvPItemNumOnWorldMap_Player=False," +
				"bEnableFastTravel=True," +
				"bEnableFastTravelOnlyBaseCamp=False," +
				"bEnableInvaderEnemy=True," +
				"bEnableVoiceChat=True," +
				"bExistPlayerAfterLogout=False," +
				"bHardcore=False," +
				"bInvisibleOtherGuildBaseCampAreaFX=False," +
				$"bIsPvP={isPvp}," +
				"bIsRandomizerPalLevelRandom=False," +
				"bIsStartLocationSelectByMap=True," +
				"bShowPlayerList=True," +
				"RandomizerSeed=\"\"," +
				"RandomizerType=\"None\"," +
				"VoiceChatMaxVolumeDistance=2000.000000," +
				"VoiceChatZeroVolumeDistance=3000.000000," +
				"AdditionalDropItemNumWhenPlayerKillingInPvPMode=0," +
				"AdditionalDropItemWhenPlayerKillingInPvPMode=\"\"," +
				"bAdditionalDropItemWhenPlayerKillingInPvPMode=False," +
				"BlockRespawnTime=10.000000," +
				"bPalLost=False," +
				"BuildObjectDamageRate=1.000000," +
				"BuildObjectDeteriorationDamageRate=1.000000," +
				"CollectionDropRate=1.000000," +
				"CollectionObjectHpRate=1.000000," +
				"CollectionObjectRespawnSpeedRate=1.000000," +
				"DayTimeSpeedRate=1.000000," +
				"DeathPenalty=\"All\"," +
				"DenyTechnologyList=()," +
				"EnemyDropItemRate=1.000000," +
				"EquipmentDurabilityDamageRate=1.000000," +
				"ExpRate=1.000000," +
				"GuildPlayerMaxNum=20," +
				"GuildRejoinCooldownMinutes=0," +
				"ItemCorruptionMultiplier=1.000000," +
				"ItemWeightRate=1.000000," +
				"MonsterFarmActionSpeedRate=1.000000," +
				"NightTimeSpeedRate=1.000000," +
				"PalAutoHPRegeneRate=1.000000," +
				"PalAutoHpRegeneRateInSleep=1.000000," +
				"PalCaptureRate=1.000000," +
				"PalDamageRateAttack=1.000000," +
				"PalDamageRateDefense=1.000000," +
				"PalEggDefaultHatchingTime=72.000000," +
				"PalSpawnNumRate=1.000000," +
				"PalStaminaDecreaceRate=1.000000," +
				"PalStomachDecreaceRate=1.000000," +
				"PlayerAutoHPRegeneRate=1.000000," +
				"PlayerAutoHpRegeneRateInSleep=1.000000," +
				"PlayerDamageRateAttack=1.000000," +
				"PlayerDamageRateDefense=1.000000," +
				"PlayerStaminaDecreaceRate=1.000000," +
				"PlayerStomachDecreaceRate=1.000000," +
				"RespawnPenaltyDurationThreshold=300.000000," +
				"RespawnPenaltyTimeScale=1.000000," +
				"SupplyDropSpan=180)\n";
		}
	}
}
