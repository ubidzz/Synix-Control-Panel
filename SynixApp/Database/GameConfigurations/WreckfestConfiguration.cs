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
	internal sealed class WreckfestConfiguration : ConfigurationDefinition
	{
		public override string GameName => "Wreckfest";
		public override bool SupportsFullReset => true;
		public override string RelativePath => "server_config.cfg";

		public override bool NeedsStructuralRepair(ConfigurationContext context)
		{
			string sourcePath = ResolveFullPath(
				context.Server,
				"initial_server_config.cfg");
			if (!File.Exists(sourcePath))
			{
				return false;
			}

			string targetPath = ResolveFullPath(context.Server);
			return !ConfigHandler.HasRequiredStructure(
				targetPath,
				File.ReadAllText(sourcePath),
				Format);
		}

		public override ConfigurationApplyResult ResetToTemplate(
			ConfigurationContext context)
		{
			try
			{
				string sourcePath = ResolveFullPath(
					context.Server,
					"initial_server_config.cfg");
				if (!File.Exists(sourcePath))
				{
					return ConfigurationApplyResult.Failure(
						LocalizationManager.Get(
							"Configuration.Apply.InitialNotGenerated",
							GameName));
				}

				return ReplaceWithTemplates(
					context,
					[new ResetTemplate(
						RelativePath,
						File.ReadAllText(sourcePath),
						Format)]);
			}
			catch (Exception exception)
			{
				return ConfigurationApplyResult.Failure(
					LocalizationManager.Get(
						"Configuration.Apply.ResetGameFailed",
						GameName,
						exception.Message));
			}
		}

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
						LocalizationManager.Get(
							"Configuration.Apply.AlreadyPresent",
							GameName));
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
						LocalizationManager.Get(
							"Configuration.Apply.InitialNotGenerated",
							GameName));
				}

				File.Copy(sourcePath, targetPath, false);
				return new ConfigurationApplyResult(
					true,
					true,
					true,
					true,
					LocalizationManager.Get(
						"Configuration.Apply.ServerConfigurationCreated",
						GameName));
			}
			catch (Exception ex)
			{
				return ConfigurationApplyResult.Failure(
					LocalizationManager.Get(
						"Configuration.Apply.CreateFailed",
						GameName,
						ex.Message));
			}
		}
	}
}
