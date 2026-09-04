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
	internal sealed class SubsistenceConfiguration : TemplateConfigurationDefinition
	{
		private static readonly ConfigurationTemplate[] Files =
		[
			new(@"UDKGame\Config\UDKDedServerSettings.ini",
				"""
				[SubDedicatedServer.SubServerConfig]
				ServerName="{ServerName}"
				ServerPassword="{Password}"
				AdminPassword="{AdminPassword}"
				MaxPlayers={MaxPlayers}
				"""),
			new(@"UDKGame\Config\UDKEngine.ini",
				"""
				[URL]
				Port={Port}

				[IpDrv.TcpNetDriver]
				Port={Port}

				[OnlineSubsystemSteamworks.OnlineSubsystemSteamworks]
				QueryPort={QueryPort}
				""")
		];

		public override string GameName => "Subsistence";
		public override int SchemaVersion => 2;
		protected override IReadOnlyList<ConfigurationTemplate> Templates => Files;

		public override ConfigurationApplyResult Apply(ConfigurationContext context)
		{
			foreach (ConfigurationTemplate template in Templates)
			{
				string path = ResolveFullPath(context.Server, template.RelativePath);
				string snapshotPath = path + ".synix.template";
				if (File.Exists(path) && !File.Exists(snapshotPath))
				{
					WriteNewFile(snapshotPath, File.ReadAllText(path));
				}
			}

			return base.Apply(context);
		}

		public override bool NeedsStructuralRepair(ConfigurationContext context)
		{
			foreach (ConfigurationTemplate template in Templates)
			{
				string path = ResolveFullPath(context.Server, template.RelativePath);
				string snapshotPath = path + ".synix.template";
				if (File.Exists(snapshotPath) &&
					!ConfigHandler.HasRequiredStructure(
						path,
						File.ReadAllText(snapshotPath),
						Format))
				{
					return true;
				}
			}

			return false;
		}

		public override ConfigurationApplyResult ResetToTemplate(
			ConfigurationContext context)
		{
			List<ResetTemplate> snapshots = [];
			foreach (ConfigurationTemplate template in Templates)
			{
				string path = ResolveFullPath(context.Server, template.RelativePath);
				string snapshotPath = path + ".synix.template";
				if (!File.Exists(snapshotPath))
				{
					return ConfigurationApplyResult.Failure(
						LocalizationManager.Get(
							"Configuration.Apply.GenerateBeforeReset",
							GameName));
				}

				snapshots.Add(new ResetTemplate(
					template.RelativePath,
					File.ReadAllText(snapshotPath),
					Format));
			}

			ConfigurationApplyResult reset = ReplaceWithTemplates(context, snapshots);
			if (!reset.Succeeded)
			{
				return reset;
			}

			Dictionary<string, byte[]> originalBackups = snapshots
				.Select(snapshot => ResolveFullPath(context.Server, snapshot.RelativePath) + ".synix.bak")
				.Where(File.Exists)
				.ToDictionary(path => path, File.ReadAllBytes, StringComparer.OrdinalIgnoreCase);
			ConfigurationApplyResult applied;
			try
			{
				applied = Apply(context);
			}
			finally
			{
				foreach ((string path, byte[] content) in originalBackups)
				{
					File.WriteAllBytes(path, content);
				}
			}
			if (!applied.Succeeded || !applied.Complete)
			{
				return new ConfigurationApplyResult(
					applied.Succeeded,
					applied.Complete,
					true,
					reset.Created,
					LocalizationManager.Get(
						"Configuration.Apply.RestoredReapplyFailed",
						GameName,
						applied.Message));
			}

			return reset with
			{
				Message = LocalizationManager.Get(
					"Configuration.Apply.Reapplied",
					reset.Message)
			};
		}
	}
}
