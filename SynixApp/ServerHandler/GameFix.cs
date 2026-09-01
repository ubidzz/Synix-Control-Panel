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
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Database.GameConfigurations;
using Synix_Control_Panel.SynixApp.Database.GameDefinitions;
using Synix_Control_Panel.SynixEngine;
using System.Reflection;

namespace Synix_Control_Panel.SynixApp.ServerHandler
{
	public static class GameFix
	{
		private static readonly IReadOnlyDictionary<string, ConfigurationDefinition> ConfigurationIndex =
			CreateConfigurationIndex();

		public static bool ManualConfigWasCreated { get; set; }

		internal static bool ManagedConfigurationsEnabled =>
			ShouldUseManagedConfigurations(
				Core.IsOfficialRelease,
				Properties.Settings.Default.DisablePremadeConfigurationsForDevelopment);

		internal static bool ShouldUseManagedConfigurations(
			bool isOfficialRelease,
			bool disabledForDevelopment)
		{
			return isOfficialRelease || !disabledForDevelopment;
		}

		internal static bool TryGetConfiguration(
			string gameName,
			out ConfigurationDefinition? definition)
		{
			return ConfigurationIndex.TryGetValue(
				GameDatabase.GetCanonicalGameName(gameName),
				out definition);
		}

		internal static ConfigFileCreationMode GetConfigFileCreationMode(
			string gameName)
		{
			return GameDatabase.GetGame(gameName)?.ConfigFileCreation ??
				ConfigFileCreationMode.Unknown;
		}

		internal static string ResolveGameModeValue(
			GameInfo? game,
			string selectedMode)
		{
			if (game == null)
				return selectedMode;
			if (string.Equals(selectedMode, "PVP", StringComparison.OrdinalIgnoreCase))
				return game.PvpValue;
			if (string.Equals(selectedMode, "PVE", StringComparison.OrdinalIgnoreCase))
				return game.PveValue;
			return selectedMode;
		}

		internal static string ResolveBooleanValue(
			GameInfo? game,
			bool enabled)
		{
			if (game == null)
				return enabled ? "true" : "false";
			return enabled ? game.BooleanTrueValue : game.BooleanFalseValue;
		}

		internal static string ResolveCrossplayValue(
			GameInfo? game,
			bool enabled)
		{
			if (game == null)
				return enabled ? "true" : "false";
			return enabled ? game.CrossplayEnabledValue : game.CrossplayDisabledValue;
		}

		internal static bool CanResetManagedConfiguration(GameServer server)
		{
			return server != null &&
				GetConfigFileCreationMode(server.Game) is
					ConfigFileCreationMode.SynixTemplate or
					ConfigFileCreationMode.GameGenerated &&
				TryGetConfiguration(server.Game, out ConfigurationDefinition? definition) &&
				definition?.SupportsFullReset == true;
		}

		internal static ConfigurationBackupSnapshot? BackupManagedConfiguration(
			GameServer server,
			string reason)
		{
			return TryGetConfiguration(server.Game, out ConfigurationDefinition? definition) &&
				definition != null
				? ConfigurationBackupManager.CreateSnapshot(server, definition, reason)
				: null;
		}

		internal static bool HasManagedConfigurationBackup(GameServer server)
		{
			return TryGetConfiguration(server.Game, out ConfigurationDefinition? definition) &&
				definition != null &&
				ConfigurationBackupManager.HasSnapshot(server, definition);
		}

		internal static ConfigurationRestoreResult RestorePreviousManagedConfiguration(
			GameServer server)
		{
			return TryGetConfiguration(server.Game, out ConfigurationDefinition? definition) &&
				definition != null
				? ConfigurationBackupManager.RestoreLatest(server, definition)
				: new ConfigurationRestoreResult(
					false,
					0,
					"This game does not have a managed configuration definition.");
		}

		internal static bool NeedsManagedConfigurationRepair(GameServer server)
		{
			if (server.PreserveImportedConfiguration)
				return false;

			if (GetConfigFileCreationMode(server.Game) is
					ConfigFileCreationMode.Unknown or
					ConfigFileCreationMode.LaunchArgumentsOnly ||
				!TryGetConfiguration(server.Game, out ConfigurationDefinition? definition) ||
				definition?.SupportsFullReset != true)
			{
				return false;
			}

			try
			{
				ConfigurationContext context = new(
					server,
					new SynixServerPasswords("template", "template", "template"),
					Core.Instance.GetSafeName(server.ServerName),
					"0.0.0.0",
					"0.0.0.0");
				return definition.NeedsStructuralRepair(context);
			}
			catch
			{
				return true;
			}
		}

		internal static async Task<ConfigurationValidationReport>
			ValidateManagedConfiguration(GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);
			List<ConfigurationValidationItem> items = [];
			ConfigFileCreationMode creationMode =
				GetConfigFileCreationMode(server.Game);

			if (creationMode == ConfigFileCreationMode.Unknown)
			{
				items.Add(new ConfigurationValidationItem(
					ConfigurationValidationState.Warning,
					"Configuration behavior",
					"This game's configuration-file behavior has not been verified."));
				return new ConfigurationValidationReport(
					server.Game,
					server.ManagedConfigurationVersion,
					0,
					false,
					items);
			}

			if (!TryGetConfiguration(
				server.Game,
				out ConfigurationDefinition? definition) ||
				definition == null)
			{
				ConfigurationValidationState state =
					creationMode == ConfigFileCreationMode.LaunchArgumentsOnly
						? ConfigurationValidationState.Passed
						: ConfigurationValidationState.Failed;
				items.Add(new ConfigurationValidationItem(
					state,
					creationMode == ConfigFileCreationMode.LaunchArgumentsOnly
						? "Launch arguments"
						: "Managed definition",
					creationMode == ConfigFileCreationMode.LaunchArgumentsOnly
						? "This game applies its supported values through launch arguments instead of a configuration file."
						: "Synix does not have a managed configuration definition for this game."));
				return new ConfigurationValidationReport(
					server.Game,
					server.ManagedConfigurationVersion,
					0,
					false,
					items);
			}

			if (!ManagedConfigurationsEnabled)
			{
				items.Add(new ConfigurationValidationItem(
					ConfigurationValidationState.Warning,
					"Development setting",
					"Premade game configurations are disabled for this development build. Validation remains read-only."));
			}

			if (server.ManagedConfigurationVersion == definition.SchemaVersion)
			{
				items.Add(new ConfigurationValidationItem(
					ConfigurationValidationState.Passed,
					"Template revision",
					$"The server is recorded with the current template revision {definition.SchemaVersion}."));
			}
			else if (server.ManagedConfigurationVersion < definition.SchemaVersion)
			{
				items.Add(new ConfigurationValidationItem(
					ConfigurationValidationState.Warning,
					"Template revision",
					$"The server is recorded with revision {server.ManagedConfigurationVersion}, but Synix now uses revision {definition.SchemaVersion}. Save Server Settings to apply the newer managed values."));
			}
			else
			{
				items.Add(new ConfigurationValidationItem(
					ConfigurationValidationState.Warning,
					"Template revision",
					$"The server was last managed by revision {server.ManagedConfigurationVersion}, which is newer than this Synix definition revision {definition.SchemaVersion}."));
			}

			string localIp = string.Empty;
			string publicIp = string.Empty;
			if (definition.RequiresNetworkAddresses)
			{
				try
				{
					localIp = await Core.Instance.GetLocalIP();
					publicIp = await Core.Instance.GetPublicIP();
				}
				catch (Exception exception)
				{
					items.Add(new ConfigurationValidationItem(
						ConfigurationValidationState.Failed,
						"Network values",
						$"Synix could not obtain the network addresses required to validate this template: {exception.Message}"));
					return new ConfigurationValidationReport(
						server.Game,
						server.ManagedConfigurationVersion,
						definition.SchemaVersion,
						definition.SupportsFullReset,
						items);
				}
			}

			SynixServerPasswords passwords = default;
			if (definition.UsesConfigurationFile)
			{
				try
				{
					passwords = Core.RevealServerPasswords(server);
				}
				catch (SynixPasswordProtectionException)
				{
					items.Add(new ConfigurationValidationItem(
						ConfigurationValidationState.Failed,
						"Protected passwords",
						"Synix could not unlock the saved passwords. Re-enter them in Server Settings before validating the configuration."));
					return new ConfigurationValidationReport(
						server.Game,
						server.ManagedConfigurationVersion,
						definition.SchemaVersion,
						definition.SupportsFullReset,
						items);
				}
			}

			ConfigurationContext context = new(
				server,
				passwords,
				Core.Instance.GetSafeName(server.ServerName),
				localIp,
				publicIp);
			items.AddRange(definition.Validate(context));
			items.Add(new ConfigurationValidationItem(
				ConfigurationValidationState.Passed,
				"Fix Config",
				definition.SupportsFullReset
					? "A complete trusted template is available. Fix Config creates a backup before replacing the file and reapplying the saved Synix values."
					: "Validation is available, but Synix will not offer a full rebuild because this game does not have a complete trusted reset template."));

			return new ConfigurationValidationReport(
				server.Game,
				server.ManagedConfigurationVersion,
				definition.SchemaVersion,
				definition.SupportsFullReset,
				items);
		}

		internal static ManagedConfigurationInput GetManagedConfigurationInputs(
			string gameName)
		{
			if (GetConfigFileCreationMode(gameName) is
					ConfigFileCreationMode.Unknown or
					ConfigFileCreationMode.LaunchArgumentsOnly ||
				!TryGetConfiguration(gameName, out ConfigurationDefinition? definition) ||
				definition == null)
			{
				return ManagedConfigurationInput.None;
			}

			return definition.SupportedInputs;
		}

		internal static GameManagementCapability GetManagementCapabilities(
			GameInfo? game)
		{
			if (game == null)
				return GameManagementCapability.None;

			string arguments = game.RequiredArgs ?? string.Empty;
			string rconSyntax = game.RconSyntax ?? string.Empty;
			ManagedConfigurationInput configuration =
				GetManagedConfigurationInputs(game.Game);
			GameManagementCapability capabilities = GameManagementCapability.None;

			void Include(
				bool argumentUsesValue,
				ManagedConfigurationInput configurationInput,
				GameManagementCapability capability)
			{
				if (argumentUsesValue ||
					(configuration & configurationInput) != ManagedConfigurationInput.None)
				{
					capabilities |= capability;
				}
			}

			Include(
				arguments.Contains("{pass}", StringComparison.OrdinalIgnoreCase),
				ManagedConfigurationInput.ServerPassword,
				GameManagementCapability.ServerPassword);
			Include(
				arguments.Contains("{adminpass}", StringComparison.OrdinalIgnoreCase),
				ManagedConfigurationInput.AdminPassword,
				GameManagementCapability.AdminPassword);
			Include(
				arguments.Contains("{seed}", StringComparison.OrdinalIgnoreCase),
				ManagedConfigurationInput.WorldSeed,
				GameManagementCapability.WorldSeed);
			Include(
				arguments.Contains("{mode}", StringComparison.OrdinalIgnoreCase) ||
				game.GameModes.Count > 0,
				ManagedConfigurationInput.GameMode,
				GameManagementCapability.GameMode);
			Include(
				arguments.Contains("{MaxPlayers}", StringComparison.OrdinalIgnoreCase),
				ManagedConfigurationInput.MaxPlayers,
				GameManagementCapability.MaxPlayers);
			Include(
				arguments.Contains("{query}", StringComparison.OrdinalIgnoreCase),
				ManagedConfigurationInput.QueryPort,
				GameManagementCapability.QueryPort);
			Include(
				arguments.Contains("{map}", StringComparison.OrdinalIgnoreCase),
				ManagedConfigurationInput.WorldName,
				GameManagementCapability.WorldName);
			Include(
				arguments.Contains("{rcon}", StringComparison.OrdinalIgnoreCase) ||
				rconSyntax.Contains("{rcon_port}", StringComparison.OrdinalIgnoreCase),
				ManagedConfigurationInput.Rcon,
				GameManagementCapability.Rcon);
			Include(
				arguments.Contains("{world_size}", StringComparison.OrdinalIgnoreCase),
				ManagedConfigurationInput.WorldSize,
				GameManagementCapability.WorldSize);
			Include(
				arguments.Contains("{port}", StringComparison.OrdinalIgnoreCase),
				ManagedConfigurationInput.Port,
				GameManagementCapability.Port);
			Include(
				arguments.Contains("{app_port}", StringComparison.OrdinalIgnoreCase),
				ManagedConfigurationInput.AppPort,
				GameManagementCapability.AppPort);
			Include(
				arguments.Contains("{crossplay}", StringComparison.Ordinal),
				ManagedConfigurationInput.Crossplay,
				GameManagementCapability.Crossplay);

			if (arguments.Contains("{ram}", StringComparison.OrdinalIgnoreCase))
				capabilities |= GameManagementCapability.Ram;
			if (GameCapabilityResolver.UsesMinecraftConfiguration(game))
				capabilities |= GameManagementCapability.GameVersion;

			return capabilities;
		}

		internal static bool NeedsManagedConfiguration(GameServer server)
		{
			if (server.PreserveImportedConfiguration)
			{
				return false;
			}

			if (!ManagedConfigurationsEnabled)
			{
				return false;
			}

			ConfigFileCreationMode creationMode =
				GetConfigFileCreationMode(server.Game);
			if (creationMode is ConfigFileCreationMode.Unknown or
				ConfigFileCreationMode.LaunchArgumentsOnly)
			{
				return false;
			}

			if (!TryGetConfiguration(server.Game, out ConfigurationDefinition? definition) ||
				definition == null)
			{
				return false;
			}

			if (creationMode == ConfigFileCreationMode.GameGenerated)
			{
				try
				{
					if (!definition.ConfigurationFileExists(server))
					{
						return false;
					}
				}
				catch
				{
					return false;
				}
			}

			if (server.ManagedConfigurationVersion < definition.SchemaVersion)
			{
				return true;
			}

			try
			{
				return !definition.ConfigurationFileExists(server);
			}
			catch
			{
				return true;
			}
		}

		internal static async Task<ConfigurationApplyResult> ApplyManagedConfiguration(
			GameServer server)
		{
			if (!ManagedConfigurationsEnabled)
			{
				return new ConfigurationApplyResult(
					true,
					true,
					false,
					false,
					"Premade game configurations are disabled for this development build.");
			}

			ConfigFileCreationMode creationMode =
				GetConfigFileCreationMode(server.Game);
			if (creationMode == ConfigFileCreationMode.Unknown)
			{
				return new ConfigurationApplyResult(
					true,
					true,
					false,
					false,
					"This game's configuration-file behavior has not been verified.");
			}

			if (creationMode == ConfigFileCreationMode.LaunchArgumentsOnly)
			{
				return ConfigurationApplyResult.ArgumentsOnly();
			}

			if (!TryGetConfiguration(server.Game, out ConfigurationDefinition? definition) ||
				definition == null)
			{
				return new ConfigurationApplyResult(
					true,
					true,
					false,
					false,
					"This game does not have a managed configuration definition.");
			}

			string localIp = string.Empty;
			string publicIp = string.Empty;
			bool needsNetworkAddresses = definition.RequiresNetworkAddresses;
			if (needsNetworkAddresses)
			{
				try
				{
					needsNetworkAddresses = !definition.ConfigurationFileExists(server);
				}
				catch
				{
				}
			}

			if (needsNetworkAddresses)
			{
				localIp = await Core.Instance.GetLocalIP();
				publicIp = await Core.Instance.GetPublicIP();
			}

			SynixServerPasswords passwords = default;
			if (definition.UsesConfigurationFile)
			{
				try
				{
					passwords = Core.RevealServerPasswords(server);
				}
				catch (SynixPasswordProtectionException)
				{
					return ConfigurationApplyResult.Failure(
						"Synix could not unlock the saved passwords. Re-enter them in Server Settings before applying the game configuration.");
				}
			}

			ConfigurationContext context = new(
				server,
				passwords,
				Core.Instance.GetSafeName(server.ServerName),
				localIp,
				publicIp);
			ConfigurationBackupSnapshot? snapshot =
				ConfigurationBackupManager.CreateSnapshot(
					server,
					definition,
					"Before applying saved Synix server settings");
			ConfigurationApplyResult result = definition.Apply(context);
			if (!result.Changed)
				ConfigurationBackupManager.Discard(snapshot);
			if (result.Succeeded && result.Complete)
			{
				server.ManagedConfigurationVersion = definition.SchemaVersion;
			}

			return result;
		}

		internal static async Task<ConfigurationApplyResult?> ApplyFirstGeneratedConfiguration(
			GameServer server)
		{
			if (GetConfigFileCreationMode(server.Game) != ConfigFileCreationMode.GameGenerated ||
				!NeedsManagedConfiguration(server))
			{
				return null;
			}

			return await ApplyManagedConfiguration(server);
		}

		internal static async Task<ConfigurationApplyResult> ResetManagedConfiguration(
			GameServer server)
		{
			if (GetConfigFileCreationMode(server.Game) is
					ConfigFileCreationMode.Unknown or
					ConfigFileCreationMode.LaunchArgumentsOnly ||
				!TryGetConfiguration(server.Game, out ConfigurationDefinition? definition) ||
				definition == null ||
				!definition.SupportsFullReset)
			{
				return ConfigurationApplyResult.Failure(
					"Synix does not have a complete reset template for this game.");
			}

			string localIp = string.Empty;
			string publicIp = string.Empty;
			if (definition.RequiresNetworkAddresses)
			{
				try
				{
					localIp = await Core.Instance.GetLocalIP();
					publicIp = await Core.Instance.GetPublicIP();
				}
				catch (Exception exception)
				{
					return ConfigurationApplyResult.Failure(
						$"Synix could not obtain the network addresses required by this template. {exception.Message}");
				}
			}

			SynixServerPasswords passwords = default;
			if (definition.UsesConfigurationFile)
			{
				try
				{
					passwords = Core.RevealServerPasswords(server);
				}
				catch (SynixPasswordProtectionException)
				{
					return ConfigurationApplyResult.Failure(
						"Synix could not unlock the saved passwords. Re-enter them in Server Settings before resetting the game configuration.");
				}
			}

			ConfigurationContext context = new(
				server,
				passwords,
				Core.Instance.GetSafeName(server.ServerName),
				localIp,
				publicIp);
			ConfigurationBackupSnapshot? snapshot =
				ConfigurationBackupManager.CreateSnapshot(
					server,
					definition,
					"Before resetting from the Synix template");
			ConfigurationApplyResult result = definition.ResetToTemplate(context);
			if (!result.Changed)
				ConfigurationBackupManager.Discard(snapshot);
			if (result.Succeeded && result.Complete)
			{
				server.ManagedConfigurationVersion = definition.SchemaVersion;
			}

			return result;
		}

		public static async Task<bool> PostInstall(GameServer server)
		{
			if (string.IsNullOrWhiteSpace(server.InstallPath) ||
				!Directory.Exists(server.InstallPath))
			{
				return false;
			}

			bool applied = false;
			if (GameCapabilityResolver.UsesMinecraftConfiguration(server))
			{
				ManualConfigWasCreated = true;
				applied = true;
			}

			try
			{
				TrustedPostInstallExecutionResult postInstall =
					TrustedPostInstallExecutor.Execute(server);
				foreach (string message in postInstall.Messages)
				{
					Color color = postInstall.Succeeded
						? message.Contains("not present", StringComparison.OrdinalIgnoreCase)
							? Color.Orange
							: Color.LightGreen
						: Color.Red;
					Core.Instance.Log($"[POST-INSTALL] {message}", color);
				}
				if (!postInstall.Succeeded)
				{
					Core.Instance.Log(
						"[POST-INSTALL ERROR] The trusted post-install recipe did not complete.",
						Color.Red);
				}
				applied |= postInstall.Changed;

				ConfigFileCreationMode configurationMode =
					GetConfigFileCreationMode(server.Game);
				if ((configurationMode is
						ConfigFileCreationMode.SynixTemplate or
						ConfigFileCreationMode.GameGenerated) &&
					TryGetConfiguration(server.Game, out ConfigurationDefinition? definition) &&
					definition != null)
				{
					bool configurationAvailable =
						configurationMode == ConfigFileCreationMode.SynixTemplate;
					if (!configurationAvailable)
					{
						configurationAvailable = definition.ConfigurationFileExists(server);
					}

					if (configurationAvailable)
					{
						ConfigurationApplyResult result = await ApplyManagedConfiguration(server);
						if (!result.Succeeded)
						{
							Core.Instance.Log($"[CONFIG ERROR] {result.Message}", Color.Red);
						}
						else
						{
							if (result.Created)
							{
								ManualConfigWasCreated = true;
							}

							if (result.Changed)
							{
								applied = true;
							}

							if (!result.Complete)
							{
								Core.Instance.Log($"[CONFIG WARNING] {result.Message}", Color.Orange);
							}
						}
					}
				}
			}
			catch (Exception)
			{
				return false;
			}

			return applied;
		}

		private static IReadOnlyDictionary<string, ConfigurationDefinition> CreateConfigurationIndex()
		{
			Dictionary<string, ConfigurationDefinition> index =
				new(StringComparer.OrdinalIgnoreCase);
			foreach (ConfigurationDefinition definition in DiscoverCompiledConfigurations())
			{
				AddConfiguration(index, definition);
			}

			foreach (EmbeddedGamePackage package in TrustedGameDefinitionCatalog.Packages)
			{
				if (package.Configuration == null)
					continue;

				AddConfiguration(
					index,
					new EmbeddedTemplateConfigurationDefinition(
						package.Definition.Game,
						package.Configuration));
			}

			return index;
		}

		private static IEnumerable<ConfigurationDefinition> DiscoverCompiledConfigurations()
		{
			Type definitionType = typeof(ConfigurationDefinition);
			foreach (Type type in typeof(GameFix).Assembly.GetTypes()
				.Where(type =>
					type != definitionType &&
					!type.IsAbstract &&
					definitionType.IsAssignableFrom(type))
				.OrderBy(type => type.FullName, StringComparer.Ordinal))
			{
				ConstructorInfo? constructor = type.GetConstructor(
					BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
					binder: null,
					Type.EmptyTypes,
					modifiers: null);
				if (constructor?.Invoke(null) is ConfigurationDefinition definition)
					yield return definition;
			}
		}

		private static void AddConfiguration(
			Dictionary<string, ConfigurationDefinition> index,
			ConfigurationDefinition definition)
		{
			if (!index.TryAdd(definition.GameName, definition))
				throw new InvalidDataException($"Duplicate configuration definition: {definition.GameName}.");

			foreach (string alias in definition.Aliases)
			{
				if (!index.TryAdd(alias, definition))
					throw new InvalidDataException($"Duplicate configuration name or alias: {alias}.");
			}
		}

	}
}
