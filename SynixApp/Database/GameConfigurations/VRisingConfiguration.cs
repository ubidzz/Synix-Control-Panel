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
	internal sealed class VRisingConfiguration : TemplateConfigurationDefinition
	{
		private static readonly ConfigurationTemplate[] Files =
		[
			new(@"save-data\Settings\ServerHostSettings.json",
				"""
				{
				  "Name": "{ServerName}",
				  "Description": "Managed by Synix",
				  "Port": {Port},
				  "QueryPort": {QueryPort},
				  "Address": "0.0.0.0",
				  "HideIPAddress": false,
				  "MaxConnectedUsers": {MaxPlayers},
				  "MaxConnectedAdmins": 4,
				  "ServerFps": 30,
				  "LowerFPSWhenEmpty": true,
				  "LowerFPSWhenEmptyValue": 5,
				  "Password": "{Password}",
				  "Secure": true,
				  "ListOnEOS": true,
				  "ListOnSteam": true,
				  "GameSettingsPreset": "",
				  "GameDifficultyPreset": "",
				  "SaveName": "{WorldName}",
				  "AutoSaveCount": 20,
				  "AutoSaveInterval": 120,
				  "AutoSaveSmartKeep": "10:1:1,30:0:1,60:0:1,120:0:1,180:0:1,240:0:1,360:0:1,720:0:1,1440:0:1,2880:0:1,52560000:99:0",
				  "LanMode": false,
				  "ResetDaysInterval": 0,
				  "DayOfReset": "Any",
				  "SafeReconnectTime": 300,
				  "SafeReconnectSlots": 10,
				  "Rcon": {
				    "Enabled": {EnableRcon},
				    "Port": {RCONPort},
				    "Password": "{RCONPassword}",
				    "BindAddress": "0.0.0.0"
				  }
				}
				""")
		];

		public override string GameName => "V Rising";
		public override int SchemaVersion => 2;
		protected override IReadOnlyList<ConfigurationTemplate> Templates => Files;
	}
}
