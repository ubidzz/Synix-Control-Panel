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
		public override int SchemaVersion => 3;
		public override bool SupportsFullReset => true;
		public override ManagedConfigurationInput SupportedInputs =>
			ManagedConfigurationInput.ServerPassword |
			ManagedConfigurationInput.AdminPassword |
			ManagedConfigurationInput.GameMode |
			ManagedConfigurationInput.MaxPlayers |
			ManagedConfigurationInput.QueryPort |
			ManagedConfigurationInput.Rcon |
			ManagedConfigurationInput.Port;
		public override string RelativePath => @"Pal\Saved\Config\WindowsServer\PalWorldSettings.ini";
		public override ConfigFormat Format => ConfigFormat.StandardINI;
		public override IReadOnlyList<ConfigurationBinding> Bindings => ManagedBindings;

		public override string? CreateTemplate(ConfigurationContext context)
		{
			string sourcePath = ResolveFullPath(
				context.Server,
				"DefaultPalWorldSettings.ini");
			return File.Exists(sourcePath)
				? File.ReadAllText(sourcePath)
				: null;
		}
	}
}
