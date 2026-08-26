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
	internal sealed class PalworldConfiguration : ConfigurationDefinition
	{
		private static readonly IReadOnlyDictionary<string, string> RequiredManagedSettings =
			new Dictionary<string, string>(StringComparer.Ordinal)
			{
				["bEnablePlayerToPlayerDamage"] = "False",
				["bEnableDefenseOtherGuildPlayer"] = "False",
				["CrossplayPlatforms"] = "(Steam,Xbox,PS5,Mac)"
			};

		private static readonly ConfigurationBinding[] ManagedBindings =
		[
			new("ServerName", context => context.Server.ServerName),
			new("AdminPassword", context => context.Passwords.AdminPassword),
			new("ServerPassword", context => context.Passwords.ServerPassword),
			new("ServerPlayerMaxNum", context => context.Server.MaxPlayers.ToString()),
			new("PublicPort", context => context.Server.Port.ToString()),
			new("PublicIP", _ => string.Empty),
			new("RCONEnabled", context => context.Server.EnableRcon.ToString()),
			new("RCONPort", context => context.Server.RconPort.ToString()),
			new("RESTAPIPort", context => context.Server.QueryPort.ToString()),
			new("CrossplayPlatforms", CrossplayPlatforms),
			new("bIsPvP", PvpEnabled),
			new("bEnablePlayerToPlayerDamage", PvpEnabled),
			new("bEnableDefenseOtherGuildPlayer", PvpEnabled)
		];

		private static string PvpEnabled(ConfigurationContext context) =>
			string.Equals(
				context.Server.GameMode,
				"PVP",
				StringComparison.OrdinalIgnoreCase).ToString();

		private static string CrossplayPlatforms(ConfigurationContext context) =>
			context.Server.CrossplayEnabled
				? "(Steam,Xbox,PS5,Mac)"
				: "(Steam)";

		public override string GameName => "Palworld";
		public override int SchemaVersion => 6;
		public override bool SupportsFullReset => true;
		public override ManagedConfigurationInput SupportedInputs =>
			ManagedConfigurationInput.ServerPassword |
			ManagedConfigurationInput.AdminPassword |
			ManagedConfigurationInput.GameMode |
			ManagedConfigurationInput.MaxPlayers |
			ManagedConfigurationInput.QueryPort |
			ManagedConfigurationInput.Rcon |
			ManagedConfigurationInput.Crossplay |
			ManagedConfigurationInput.Port;
		public override string RelativePath => @"Pal\Saved\Config\WindowsServer\PalWorldSettings.ini";
		public override ConfigFormat Format => ConfigFormat.StandardINI;
		public override IReadOnlyList<ConfigurationBinding> Bindings => ManagedBindings;

		public override ConfigurationApplyResult Apply(ConfigurationContext context)
		{
			ConfigurationApplyResult first = base.Apply(context);
			if (!first.Succeeded)
				return first;

			string path = ResolveFullPath(context.Server);
			List<ConfigLine> values = ConfigHandler.LoadConfig(path, Format);
			if (RequiredManagedSettings.Keys.All(key => values.Any(value =>
				string.Equals(value.Key, key, StringComparison.Ordinal))))
			{
				return first;
			}

			string backupPath = path + ".synix.bak";
			byte[] preservedBackup = File.Exists(backupPath)
				? File.ReadAllBytes(backupPath)
				: File.ReadAllBytes(path);
			try
			{
				ConfigHandler.EnsureStandardIniTupleValues(
					path,
					"OptionSettings",
					RequiredManagedSettings);
				ConfigurationApplyResult second = base.Apply(context);
				return second with
				{
					Changed = true,
					Created = first.Created || second.Created
				};
			}
			catch (Exception exception)
			{
				return ConfigurationApplyResult.Failure(
					$"The Palworld managed settings could not be upgraded safely: {exception.Message}");
			}
			finally
			{
				File.WriteAllBytes(backupPath, preservedBackup);
			}
		}

		public override string? CreateTemplate(ConfigurationContext context)
		{
			string sourcePath = ResolveFullPath(
				context.Server,
				"DefaultPalWorldSettings.ini");
			return File.Exists(sourcePath)
				? File.ReadAllText(sourcePath)
				: null;
		}
	}
}
