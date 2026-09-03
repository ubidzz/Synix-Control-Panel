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
using Synix_Control_Panel.SynixApp.Database.GameDefinitions;
using System.Text.Json;

namespace Synix_Control_Panel.SynixApp.Database.GameConfigurations
{
	internal sealed class WindroseConfiguration : ConfigurationDefinition
	{
		private const string PersistentPath = "ServerDescription_Persistent.";
		private const int MaximumPlayers = 8;

		private static readonly ConfigurationBinding[] ManagedBindings =
		[
			new(PersistentPath + "Password", context => context.Passwords.ServerPassword),
			new(
				PersistentPath + "IsPasswordProtected",
				context => (!string.IsNullOrEmpty(context.Passwords.ServerPassword)).ToString()),
			new(PersistentPath + "ServerName", context => context.Server.ServerName),
			new(
				PersistentPath + "MaxPlayerCount",
				context => Math.Clamp(context.Server.MaxPlayers, 1, MaximumPlayers).ToString()),
			new(
				PersistentPath + "InviteCode",
				context => ResolveInviteCode(context.Server)),
			new(
				PersistentPath + "DirectConnectionServerPort",
				context => context.Server.Port.ToString())
		];

		public override string GameName => "Windrose";
		public override int SchemaVersion => 3;
		public override bool SupportsFullReset => true;
		public override bool PreservesInstalledTemplate => true;
		public override ManagedConfigurationInput SupportedInputs =>
			ManagedConfigurationInput.ServerPassword |
			ManagedConfigurationInput.ServerName |
			ManagedConfigurationInput.MaxPlayers |
			ManagedConfigurationInput.Port |
			ManagedConfigurationInput.InviteCode;
		public override string RelativePath => @"R5\ServerDescription.json";
		public override ConfigFormat Format => ConfigFormat.JSON;
		public override IReadOnlyList<ConfigurationBinding> Bindings => ManagedBindings;

		public override string? CreateTemplate(ConfigurationContext context)
		{
			string? trustedTemplate = GetTrustedStructureTemplate();
			if (trustedTemplate == null)
				return null;

			string? preservedTemplate = base.CreateTemplate(context);
			if (IsSafePerServerTemplate(preservedTemplate, trustedTemplate))
				return preservedTemplate;

			string installedPath = ResolveFullPath(context.Server);
			if (!File.Exists(installedPath))
				return null;

			try
			{
				string installedConfiguration = File.ReadAllText(installedPath);
				return IsSafePerServerTemplate(installedConfiguration, trustedTemplate)
					? installedConfiguration
					: null;
			}
			catch (Exception exception) when (exception is IOException or
				UnauthorizedAccessException or ArgumentException or NotSupportedException)
			{
				return null;
			}
		}

		public override bool NeedsStructuralRepair(ConfigurationContext context)
		{
			string? trustedTemplate = GetTrustedStructureTemplate();
			return trustedTemplate == null ||
				!ConfigHandler.HasRequiredStructure(
					ResolveFullPath(context.Server),
					trustedTemplate,
					Format);
		}

		public override ConfigurationApplyResult Apply(ConfigurationContext context)
		{
			context.Server.MaxPlayers = Math.Clamp(
				context.Server.MaxPlayers,
				1,
				MaximumPlayers);
			if (string.IsNullOrWhiteSpace(context.Server.InviteCode))
				context.Server.InviteCode = ReadInstalledInviteCode(context.Server);

			return base.Apply(context);
		}

		internal static string ReadInstalledInviteCode(GameServer server)
		{
			try
			{
				WindroseConfiguration definition = new();
				string path = definition.ResolveFullPath(server);
				if (!File.Exists(path))
					return string.Empty;

				return ConfigHandler.LoadConfig(path, ConfigFormat.JSON)
					.SingleOrDefault(value => string.Equals(
						value.Key,
						PersistentPath + "InviteCode",
						StringComparison.Ordinal))
					?.Value ?? string.Empty;
			}
			catch (Exception exception) when (exception is IOException or
				UnauthorizedAccessException or InvalidDataException or
				ArgumentException or NotSupportedException)
			{
				return string.Empty;
			}
		}

		private static string ResolveInviteCode(GameServer server)
		{
			return string.IsNullOrWhiteSpace(server.InviteCode)
				? ReadInstalledInviteCode(server)
				: server.InviteCode.Trim();
		}

		private static string? GetTrustedStructureTemplate()
		{
			if (!TrustedGameDefinitionCatalog.TryGetPackage(
				"Windrose",
				out EmbeddedGamePackage? package))
			{
				return null;
			}

			return package?.Configuration?.Templates
				.SingleOrDefault(template => string.Equals(
					template.RelativePath,
					@"R5\ServerDescription.json",
					StringComparison.OrdinalIgnoreCase))
				?.Content;
		}

		private static bool IsSafePerServerTemplate(
			string? content,
			string trustedTemplate)
		{
			if (string.IsNullOrWhiteSpace(content))
				return false;

			try
			{
				if (!ConfigHandler.HasRequiredStructureText(
					content,
					trustedTemplate,
					ConfigFormat.JSON))
				{
					return false;
				}

				using JsonDocument document = JsonDocument.Parse(content);
				JsonElement root = document.RootElement;
				if (!HasNonEmptyString(root, "DeploymentId") ||
					!root.TryGetProperty("ServerDescription_Persistent", out JsonElement persistent) ||
					persistent.ValueKind != JsonValueKind.Object)
				{
					return false;
				}

				return HasNonEmptyString(persistent, "PersistentServerId") &&
					HasNonEmptyString(persistent, "WorldIslandId");
			}
			catch (JsonException)
			{
				return false;
			}
		}

		private static bool HasNonEmptyString(JsonElement parent, string propertyName)
		{
			return parent.TryGetProperty(propertyName, out JsonElement value) &&
				value.ValueKind == JsonValueKind.String &&
				!string.IsNullOrWhiteSpace(value.GetString());
		}
	}
}
