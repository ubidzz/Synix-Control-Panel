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
using System.Reflection;

namespace Synix_Control_Panel.SynixApp.Database.GameConfigurations
{
	internal sealed class MinecraftConfiguration : ConfigurationDefinition
	{
		private const string JavaTemplateResourceSuffix =
			".Database.GameDefinitions.minecraft.Templates.server.properties";
		private static readonly MinecraftBedrockConfiguration BedrockConfiguration = new();
		private static readonly Lazy<string> JavaTemplate = new(LoadJavaTemplate);
		private static readonly ConfigurationBinding[] ManagedBindings =
		[
			new("motd", context => EscapeProperty(context.Server.ServerName)),
			new("gamemode", context => MinecraftControlProfile
				.NormalizeGameMode(context.Server.GameMode)
				.ToLowerInvariant()),
			new("server-port", context => context.Server.Port.ToString()),
			new("enable-query", _ => bool.TrueString),
			new("query.port", context => context.Server.QueryPort.ToString()),
			new("max-players", context => context.Server.MaxPlayers.ToString()),
			new("level-name", context => EscapeProperty(string.IsNullOrWhiteSpace(context.Server.WorldName) ? "world" : context.Server.WorldName)),
			new("level-seed", context => EscapeProperty(context.Server.WorldSeed)),
			new("enable-rcon", context => context.Server.EnableRcon.ToString().ToLowerInvariant()),
			new("rcon.port", context => context.Server.RconPort.ToString()),
			new("rcon.password", context => EscapeProperty(context.Passwords.RconPassword)),
			new("management-server-enabled", context => ManagementEnabled(context).ToString().ToLowerInvariant()),
			new("management-server-host", _ => "localhost"),
			new("management-server-port", context => ManagementEnabled(context)
				? EnsureManagementPort(context).ToString()
				: "0"),
			new("management-server-secret", context => ManagementEnabled(context)
				? MinecraftControlProfile.GetOrCreateManagementSecret(context.Server)
				: string.Empty),
			new("management-server-tls-enabled", _ => bool.FalseString.ToLowerInvariant()),
			new("status-heartbeat-interval", context => ManagementEnabled(context) ? "5" : "0")
		];

		public override string GameName => "Minecraft";
		public override int SchemaVersion => 6;
		public override bool SupportsFullReset => true;
		public override ManagedConfigurationInput SupportedInputs =>
			ManagedConfigurationInput.WorldSeed |
			ManagedConfigurationInput.GameMode |
			ManagedConfigurationInput.MaxPlayers |
			ManagedConfigurationInput.QueryPort |
			ManagedConfigurationInput.WorldName |
			ManagedConfigurationInput.Rcon |
			ManagedConfigurationInput.Port;
		public override string RelativePath => "server.properties";
		public override ConfigFormat Format => ConfigFormat.StandardINI;
		public override IReadOnlyList<ConfigurationBinding> Bindings => ManagedBindings;

		public override ConfigurationApplyResult Apply(ConfigurationContext context) =>
			MinecraftControlProfile.IsBedrock(context.Server)
				? BedrockConfiguration.Apply(context)
				: base.Apply(context);

		public override IReadOnlyList<ConfigurationValidationItem> Validate(
			ConfigurationContext context) =>
			MinecraftControlProfile.IsBedrock(context.Server)
				? BedrockConfiguration.Validate(context)
				: base.Validate(context);

		public override ConfigurationApplyResult ResetToTemplate(ConfigurationContext context) =>
			MinecraftControlProfile.IsBedrock(context.Server)
				? BedrockConfiguration.ResetToTemplate(context)
				: base.ResetToTemplate(context);

		public override bool NeedsStructuralRepair(ConfigurationContext context) =>
			MinecraftControlProfile.IsBedrock(context.Server)
				? BedrockConfiguration.NeedsStructuralRepair(context)
				: base.NeedsStructuralRepair(context);

		public override string CreateTemplate(ConfigurationContext context)
		{
			return JavaTemplate.Value;
		}

		private static string LoadJavaTemplate()
		{
			Assembly assembly = typeof(MinecraftConfiguration).Assembly;
			string? resourceName = assembly.GetManifestResourceNames().FirstOrDefault(name =>
				name.EndsWith(JavaTemplateResourceSuffix, StringComparison.OrdinalIgnoreCase));
			if (resourceName == null)
			{
				throw new InvalidDataException(
				LocalizationManager.Get("Minecraft.Configuration.TemplateMissing"));
			}

			using Stream? stream = assembly.GetManifestResourceStream(resourceName);
			if (stream == null)
			{
				throw new InvalidDataException(
				LocalizationManager.Get("Minecraft.Configuration.TemplateOpenFailed"));
			}

			using StreamReader reader = new(stream);
			return reader.ReadToEnd();
		}

		private static bool ManagementEnabled(ConfigurationContext context) =>
			MinecraftControlProfile.ShouldEnableManagementProtocol(context.Server);

		private static int EnsureManagementPort(ConfigurationContext context)
		{
			MinecraftControlProfile.EnsureDefaults(context.Server);
			return context.Server.MinecraftManagementPort;
		}
	}

	internal sealed class MinecraftBedrockConfiguration : ConfigurationDefinition
	{
		private static readonly ConfigurationBinding[] ManagedBindings =
		[
			new("server-name", context => EscapeProperty(context.Server.ServerName)),
			new("server-port", context => context.Server.Port.ToString()),
			new("server-portv6", context => context.Server.QueryPort.ToString()),
			new("max-players", context => context.Server.MaxPlayers.ToString()),
			new("level-name", context => EscapeProperty(
				string.IsNullOrWhiteSpace(context.Server.WorldName) ? "Bedrock level" : context.Server.WorldName)),
			new("level-seed", context => EscapeProperty(context.Server.WorldSeed)),
			new("gamemode", context => MinecraftControlProfile
				.NormalizeGameMode(context.Server.GameMode)
				.ToLowerInvariant())
		];

		public override string GameName => "Minecraft Bedrock";
		public override int SchemaVersion => 1;
		public override bool SupportsFullReset => true;
		public override ManagedConfigurationInput SupportedInputs =>
			ManagedConfigurationInput.WorldSeed |
			ManagedConfigurationInput.GameMode |
			ManagedConfigurationInput.MaxPlayers |
			ManagedConfigurationInput.QueryPort |
			ManagedConfigurationInput.WorldName |
			ManagedConfigurationInput.Port |
			ManagedConfigurationInput.ServerName;
		public override string RelativePath => "server.properties";
		public override ConfigFormat Format => ConfigFormat.StandardINI;
		public override IReadOnlyList<ConfigurationBinding> Bindings => ManagedBindings;

		public override string CreateTemplate(ConfigurationContext context)
		{
			return string.Join("\n",
				$"server-name={EscapeProperty(context.Server.ServerName)}",
				$"gamemode={MinecraftControlProfile.NormalizeGameMode(context.Server.GameMode).ToLowerInvariant()}",
				"force-gamemode=false",
				"difficulty=easy",
				"allow-cheats=false",
				$"max-players={context.Server.MaxPlayers}",
				"online-mode=true",
				"allow-list=false",
				$"server-port={context.Server.Port}",
				$"server-portv6={context.Server.QueryPort}",
				"enable-lan-visibility=true",
				"view-distance=32",
				"tick-distance=4",
				"player-idle-timeout=30",
				"max-threads=8",
				$"level-name={EscapeProperty(string.IsNullOrWhiteSpace(context.Server.WorldName) ? "Bedrock level" : context.Server.WorldName)}",
				$"level-seed={EscapeProperty(context.Server.WorldSeed)}",
				"default-player-permission-level=member",
				"texturepack-required=false",
				"content-log-file-enabled=false",
				"compression-threshold=1",
				"compression-algorithm=zlib",
				"server-authoritative-movement=server-auth",
				"server-authoritative-block-breaking=false",
				"chat-restriction=None",
				"disable-player-interaction=false",
				"client-side-chunk-generation=true",
				string.Empty);
		}

	}
}
