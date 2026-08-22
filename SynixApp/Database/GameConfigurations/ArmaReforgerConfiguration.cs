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
	internal sealed class ArmaReforgerConfiguration : ConfigurationDefinition
	{
		private static readonly ConfigurationBinding[] ManagedBindings =
		[
			new("bindPort", context => context.Server.Port.ToString()),
			new("publicPort", context => context.Server.Port.ToString()),
			new("a2s.port", context => context.Server.QueryPort.ToString()),
			new("game.name", context => context.Server.ServerName),
			new("game.password", context => NormalizeOptionalValue(context.Passwords.ServerPassword)),
			new("game.passwordAdmin", context => NormalizeOptionalValue(context.Passwords.AdminPassword)),
			new("game.maxPlayers", context => context.Server.MaxPlayers.ToString())
		];

		public override string GameName => "Arma Reforger";
		public override int SchemaVersion => 2;
		public override ManagedConfigurationInput SupportedInputs =>
			ManagedConfigurationInput.ServerPassword |
			ManagedConfigurationInput.AdminPassword |
			ManagedConfigurationInput.MaxPlayers |
			ManagedConfigurationInput.QueryPort |
			ManagedConfigurationInput.Port;
		public override string RelativePath => @"configs\server.json";
		public override ConfigFormat Format => ConfigFormat.JSON;
		public override IReadOnlyList<ConfigurationBinding> Bindings => ManagedBindings;

		public override string CreateTemplate(ConfigurationContext context) =>
			"""
			{
			  "bindPort": 2001,
			  "publicPort": 2001,
			  "a2s": {
			    "address": "0.0.0.0",
			    "port": 17777
			  },
			  "game": {
			    "name": "Synix Server",
			    "password": "",
			    "passwordAdmin": "",
			    "scenarioId": "{ECC61978EDCC2B5A}Missions/23_Campaign.conf",
			    "maxPlayers": 10,
			    "visible": true,
			    "crossPlatform": true,
			    "gameProperties": {
			      "fastValidation": true,
			      "battlEye": true
			    },
			    "mods": []
			  }
			}
			""";

		private static string NormalizeOptionalValue(string value)
		{
			return string.Equals(value, "Not Required", StringComparison.OrdinalIgnoreCase)
				? string.Empty
				: value;
		}
	}
}
