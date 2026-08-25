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
	internal sealed class SurviveTheNightsConfiguration : ConfigurationDefinition
	{
		private static readonly ConfigurationBinding[] ManagedBindings =
		[
			new("ServerName", context => context.Server.ServerName),
			new("ServerPassword", context => context.Passwords.ServerPassword),
			new("ServerPort", context => context.Server.Port.ToString())
		];

		public override string GameName => "Survive the Nights";
		public override int SchemaVersion => 2;
		public override bool SupportsFullReset => true;
		public override ManagedConfigurationInput SupportedInputs =>
			ManagedConfigurationInput.ServerPassword |
			ManagedConfigurationInput.Port;
		public override string RelativePath => @"Config\ServerConfig.txt";
		public override ConfigFormat Format => ConfigFormat.StandardINI;
		public override IReadOnlyList<ConfigurationBinding> Bindings => ManagedBindings;

		public override string? CreateTemplate(ConfigurationContext context)
		{
			string sourcePath = ResolveFullPath(
				context.Server,
				@"STN_Dedicated_Server_Data\StreamingAssets\Config_Template\ServerConfig.txt");
			return File.Exists(sourcePath)
				? File.ReadAllText(sourcePath)
				: null;
		}
	}
}
