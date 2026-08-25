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
	internal sealed class BannerlordConfiguration : ConfigurationDefinition
	{
		private static readonly ConfigurationBinding[] ManagedBindings =
		[
			new("ServerName", context => context.Server.ServerName),
			new("GamePassword", context => context.Passwords.ServerPassword),
			new("AdminPassword", context => context.Passwords.AdminPassword),
			new("MaxNumberOfPlayers", context => context.Server.MaxPlayers.ToString())
		];

		public override string GameName => "Mount & Blade II: Bannerlord";
		public override int SchemaVersion => 2;
		public override bool SupportsFullReset => true;
		public override ManagedConfigurationInput SupportedInputs =>
			ManagedConfigurationInput.ServerPassword |
			ManagedConfigurationInput.AdminPassword |
			ManagedConfigurationInput.MaxPlayers;
		public override string RelativePath =>
			@"Modules\Native\CustomServerconfig.txt";
		public override ConfigFormat Format => ConfigFormat.Space;
		public override IReadOnlyList<ConfigurationBinding> Bindings => ManagedBindings;

		public override string? CreateTemplate(ConfigurationContext context)
		{
			string nativeDirectory = ResolveFullPath(
				context.Server,
				@"Modules\Native\placeholder.txt");
			nativeDirectory = Path.GetDirectoryName(nativeDirectory)!;
			string preferred = Path.Combine(nativeDirectory, "ds_config_tdm.txt");
			if (File.Exists(preferred))
			{
				return File.ReadAllText(preferred);
			}

			string? available = Directory.Exists(nativeDirectory)
				? Directory.EnumerateFiles(nativeDirectory, "ds_config_*.txt")
					.OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
					.FirstOrDefault()
				: null;
			return available == null ? null : File.ReadAllText(available);
		}
	}
}
