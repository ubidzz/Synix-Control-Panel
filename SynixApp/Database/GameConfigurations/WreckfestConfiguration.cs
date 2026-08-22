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
	internal sealed class WreckfestConfiguration : ConfigurationDefinition
	{
		public override string GameName => "Wreckfest";
		public override string RelativePath => "server_config.cfg";

		public override ConfigurationApplyResult Apply(ConfigurationContext context)
		{
			try
			{
				string targetPath = ResolveFullPath(context.Server);
				if (File.Exists(targetPath))
				{
					return new ConfigurationApplyResult(
						true,
						true,
						false,
						false,
						"The Wreckfest configuration is already present.");
				}

				string sourcePath = ResolveFullPath(
					context.Server,
					"initial_server_config.cfg");
				if (!File.Exists(sourcePath))
				{
					return new ConfigurationApplyResult(
						true,
						false,
						false,
						false,
						"Wreckfest has not generated its initial configuration yet.");
				}

				File.Copy(sourcePath, targetPath, false);
				return new ConfigurationApplyResult(
					true,
					true,
					true,
					true,
					"Created the Wreckfest server configuration.");
			}
			catch (Exception ex)
			{
				return ConfigurationApplyResult.Failure(
					$"The Wreckfest configuration could not be created: {ex.Message}");
			}
		}
	}
}

