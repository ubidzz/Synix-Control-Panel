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
using System.Security;

namespace Synix_Control_Panel.SynixApp.Database.GameConfigurations
{
	internal sealed class SevenDaysToDieConfiguration : ConfigurationDefinition
	{
		private static readonly ConfigurationBinding[] ManagedBindings =
		[
			new("ServerName", context => context.Server.ServerName),
			new("ServerPassword", context => context.Passwords.ServerPassword),
			new("ServerPort", context => context.Server.Port.ToString()),
			new("ServerMaxPlayerCount", context => context.Server.MaxPlayers.ToString()),
			new("GameWorld", context => context.Server.WorldName),
			new("GameName", context => context.Identity),
			new("WorldGenSeed", context => string.IsNullOrWhiteSpace(context.Server.WorldSeed) ? "12345" : context.Server.WorldSeed),
			new("WorldGenSize", context => (context.Server.WorldSize > 0 ? context.Server.WorldSize : 6144).ToString())
		];

		public override string GameName => "7 Days to Die";
		public override string RelativePath => "serverconfig.xml";
		public override ConfigFormat Format => ConfigFormat.XML;
		public override IReadOnlyList<ConfigurationBinding> Bindings => ManagedBindings;

		public override string CreateTemplate(ConfigurationContext context)
		{
			string Escape(string value) => SecurityElement.Escape(RequireSingleLine(value, "XML value")) ?? string.Empty;

			return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
				"<ServerSettings>\n" +
				$"  <property name=\"ServerName\" value=\"{Escape(context.Server.ServerName)}\"/>\n" +
				$"  <property name=\"ServerPassword\" value=\"{Escape(context.Passwords.ServerPassword)}\"/>\n" +
				$"  <property name=\"ServerPort\" value=\"{context.Server.Port}\"/>\n" +
				$"  <property name=\"ServerMaxPlayerCount\" value=\"{context.Server.MaxPlayers}\"/>\n" +
				$"  <property name=\"GameWorld\" value=\"{Escape(context.Server.WorldName)}\"/>\n" +
				$"  <property name=\"GameName\" value=\"{Escape(context.Identity)}\"/>\n" +
				$"  <property name=\"WorldGenSeed\" value=\"{Escape(string.IsNullOrWhiteSpace(context.Server.WorldSeed) ? "12345" : context.Server.WorldSeed)}\"/>\n" +
				$"  <property name=\"WorldGenSize\" value=\"{(context.Server.WorldSize > 0 ? context.Server.WorldSize : 6144)}\"/>\n" +
				"</ServerSettings>\n";
		}
	}
}
