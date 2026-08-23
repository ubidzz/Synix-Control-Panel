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
using Synix_Control_Panel.SynixApp.Database.GameDefinitions;

namespace Synix_Control_Panel.SynixApp.Database.GameConfigurations
{
	internal sealed class EmbeddedTemplateConfigurationDefinition :
		TemplateConfigurationDefinition
	{
		private readonly string _gameName;
		private readonly int _schemaVersion;
		private readonly bool _requiresNetworkAddresses;
		private readonly ManagedConfigurationInput _managedInputs;
		private readonly ConfigurationTemplate[] _templates;

		internal EmbeddedTemplateConfigurationDefinition(
			string gameName,
			EmbeddedConfigurationDefinition definition)
		{
			_gameName = gameName;
			_schemaVersion = definition.Revision;
			_requiresNetworkAddresses = definition.RequiresNetworkAddresses;
			_managedInputs = definition.ManagedInputs.Aggregate(
				ManagedConfigurationInput.None,
				(current, input) => current | input);
			_templates = definition.Templates
				.Select(template => new ConfigurationTemplate(
					template.RelativePath,
					template.Content,
					template.Revision))
				.ToArray();
		}

		public override string GameName => _gameName;
		public override int SchemaVersion => _schemaVersion;
		public override bool RequiresNetworkAddresses => _requiresNetworkAddresses;
		public override ManagedConfigurationInput SupportedInputs =>
			_managedInputs == ManagedConfigurationInput.None
				? base.SupportedInputs
				: _managedInputs;
		protected override IReadOnlyList<ConfigurationTemplate> Templates => _templates;
	}
}
