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
	internal sealed class DysterraConfiguration : ConfigurationDefinition
	{
		private static readonly ConfigurationBinding[] ManagedBindings =
		[
			new("WorldName", context => context.Server.ServerName),
			new("Password", context => context.Passwords.ServerPassword),
			new("MaxPlayers", context => context.Server.MaxPlayers.ToString())
		];

		public override string GameName => "Dysterra";
		public override int SchemaVersion => 2;
		public override bool SupportsFullReset => true;
		public override ManagedConfigurationInput SupportedInputs =>
			ManagedConfigurationInput.ServerPassword |
			ManagedConfigurationInput.MaxPlayers;
		public override string RelativePath => @"Dysterra\WorldSettings\MyServer.json";
		public override ConfigFormat Format => ConfigFormat.JSON;
		public override IReadOnlyList<ConfigurationBinding> Bindings => ManagedBindings;

		public override string? CreateTemplate(ConfigurationContext context)
		{
			string sourcePath = ResolveFullPath(
				context.Server,
				@"Dysterra\WorldSettings\Survival_Landscape_Template.json");
			return File.Exists(sourcePath)
				? File.ReadAllText(sourcePath)
				: null;
		}
	}
}
