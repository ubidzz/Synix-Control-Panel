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
using System.Text;
namespace Synix_Control_Panel.SynixApp.Database.GameConfigurations
{
	internal sealed class SevenDaysToDieConfiguration : ConfigurationDefinition
	{
		private const string DefaultTemplateResourceName =
			"Synix.GameDefinitions.SevenDaysToDie.serverconfig.xml";
		private static readonly int[] SupportedWorldSizes = [6144, 8192, 10240];
		private static readonly Lazy<string> DefaultTemplate =
			new(LoadDefaultTemplate, LazyThreadSafetyMode.ExecutionAndPublication);

		private static readonly ConfigurationBinding[] ManagedBindings =
		[
			new("ServerName", context => context.Server.ServerName),
			new("ServerPassword", context => context.Passwords.ServerPassword),
			new("ServerPort", context => context.Server.Port.ToString()),
			new("ServerMaxPlayerCount", context => context.Server.MaxPlayers.ToString()),
			new("GameWorld", context => NormalizeWorldName(context.Server.WorldName)),
			new("GameName", context => context.Identity),
			new("WorldGenSeed", context => string.IsNullOrWhiteSpace(context.Server.WorldSeed) ? "12345" : context.Server.WorldSeed),
			new("WorldGenSize", context => NormalizeWorldSize(context.Server.WorldSize).ToString())
		];

		public override string GameName => "7 Days to Die";
		public override int SchemaVersion => 5;
		public override bool SupportsFullReset => true;
		public override ManagedConfigurationInput SupportedInputs =>
			ManagedConfigurationInput.ServerName |
			ManagedConfigurationInput.ServerPassword |
			ManagedConfigurationInput.WorldSeed |
			ManagedConfigurationInput.MaxPlayers |
			ManagedConfigurationInput.WorldName |
			ManagedConfigurationInput.WorldSize |
			ManagedConfigurationInput.Port;
		public override string RelativePath => "serverconfig.xml";
		public override ConfigFormat Format => ConfigFormat.XML;
		public override IReadOnlyList<ConfigurationBinding> Bindings => ManagedBindings;
		public override string? CreateTemplate(ConfigurationContext context)
		{
			ArgumentNullException.ThrowIfNull(context);
			return DefaultTemplate.Value;
		}

		internal static string NormalizeWorldName(string? worldName)
		{
			string normalized = worldName?.Trim() ?? string.Empty;
			return normalized.ToLowerInvariant() switch
			{
				"pregen6k" => "Pregen06k01",
				"pregen8k" => "Pregen08k01",
				"pregen10k" => "Navezgane",
				"" => "Navezgane",
				_ => normalized
			};
		}

		internal static int NormalizeWorldSize(int worldSize) =>
			SupportedWorldSizes.Contains(worldSize) ? worldSize : 6144;

		private static string LoadDefaultTemplate()
		{
			using Stream? stream = typeof(SevenDaysToDieConfiguration).Assembly
				.GetManifestResourceStream(DefaultTemplateResourceName);
			if (stream == null)
			{
				throw new InvalidDataException(
					"The trusted 7 Days to Die serverconfig.xml template is missing.");
			}

			using StreamReader reader = new(
				stream,
				Encoding.UTF8,
				detectEncodingFromByteOrderMarks: true);
			return reader.ReadToEnd();
		}
	}
}
