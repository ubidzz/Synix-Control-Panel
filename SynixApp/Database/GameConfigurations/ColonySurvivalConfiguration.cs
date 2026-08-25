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
	internal sealed class ColonySurvivalConfiguration : TemplateConfigurationDefinition
	{
		private static readonly ConfigurationTemplate[] Files =
		[
			new("server.config.json",
				"""
				{
				  "NewOptions": {
				    "WorldName": "{WorldName}",
				    "Seed": "{WorldSeed}",
				    "DifficultyKey": "2",
				    "DifficultyIsLocked": false,
				    "WorldType": 2,
				    "TerrainGeneratorSettings": {
				      "CacheMaximumMetaTiles": 64,
				      "CacheMaxTiles": 64,
				      "CacheMaxTileRegenerationPerTick": 0,
				      "TilesPerMetaTile": 32,
				      "TerrainHeightDefault": 166,
				      "TerrainWaterLevel": 164,
				      "IslandRadiusMin": 0.6,
				      "IslandRadiusRelaxed": 0.8,
				      "IslandRadiusMax": 0.9,
				      "HillCounts": { "Min": 3, "MaxExclusive": 5 },
				      "HeathCounts": { "Min": 3, "MaxExclusive": 5 },
				      "FenCounts": { "Min": 3, "MaxExclusive": 5 },
				      "BigTreeCounts": { "Min": 3, "MaxExclusive": 7 },
				      "HillSettings": {
				        "MinDimension": 400.0,
				        "MaxDimensionPrimary": 1000.0,
				        "MinDimensionSecondary": 400.0,
				        "MaxDimensionSecondary": 500.0,
				        "MinHeight": 100.0,
				        "MaxHeight": 150.0
				      },
				      "HeathSettings": {
				        "MinDimension": 350.0,
				        "MaxDimensionPrimary": 450.0,
				        "MinDimensionSecondary": 400.0,
				        "MaxDimensionSecondary": 550.0,
				        "MinHeightOffset": 5.0,
				        "MaxHeightOffset": 15.0,
				        "MinHeightScale": 5.0,
				        "MaxHeightScale": 10.0
				      },
				      "FenSettings": {
				        "MinDimension": 350.0,
				        "MaxDimensionPrimary": 450.0,
				        "MinDimensionSecondary": 400.0,
				        "MaxDimensionSecondary": 550.0
				      },
				      "River": {
				        "OutwardPressureMin": 0.125,
				        "OutwardPressureMax": 0.25,
				        "WidthMinimum": 2.0,
				        "WidthMaximum": 5.0,
				        "Depth": 3.0
				      },
				      "MainIsland": {
				        "IslandWaterDepth": 8.0,
				        "LakeHeight": -10.0,
				        "HillHeights": 90.0,
				        "HillWeight0": 1.0,
				        "HillWeight1": 0.5,
				        "HillWeight2": 0.25,
				        "LakeThreshold": 0.5
				      },
				      "BigTree": {
				        "MinHeight": 40.0,
				        "MaxHeight": 60.0,
				        "MinTopRadius": 1.2,
				        "MaxTopRadius": 1.6,
				        "MinBottomRadius": 2.5,
				        "MaxBottomRadius": 3.5,
				        "MinBranchMinHeight": 0.35,
				        "MaxBranchMinHeight": 0.5,
				        "MinBranchInterval": 6.0,
				        "MaxBranchInterval": 8.0
				      }
				    }
				  },
				  "ServerSettings": {
				    "ServerName": "{ServerName}",
				    "ServerPassword": "{Password}",
				    "ServerIP": "0.0.0.0",
				    "ServerGamePort": {Port},
				    "ServerQueryPort": {QueryPort},
				    "UseVAC": false,
				    "MaxPlayerCount": {MaxPlayers},
				    "MaxDrawDistance": 1024,
				    "NetworkType": "SteamOnline",
				    "RCONPassword": "{RCONPassword}"
				  }
				}
				""")
		];

		public override string GameName => "Colony Survival";
		public override int SchemaVersion => 2;
		protected override IReadOnlyList<ConfigurationTemplate> Templates => Files;
	}
}
