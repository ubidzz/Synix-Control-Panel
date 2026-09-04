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
using System.Text.RegularExpressions;

namespace Synix_Control_Panel.SynixApp.Database.GameDefinitions
{
	internal sealed record GameDefinitionArgumentTag(
		string Token,
		string Name,
		string Description);

	internal static partial class GameDefinitionArgumentTags
	{
		internal static IReadOnlyList<GameDefinitionArgumentTag> LaunchArguments { get; } =
		[
			Create("{ServerName}", "ServerName"),
			Create("{Identity}", "Identity"),
			Create("{port}", "Port"),
			Create("{query}", "QueryPort"),
			Create("{app_port}", "AppPort"),
			Create("{MaxPlayers}", "MaxPlayers"),
			Create("{pass}", "Password"),
			Create("{adminpass}", "AdminPassword"),
			Create("{auth_token}", "AuthenticationToken"),
			Create("{map}", "Map"),
			Create("{seed}", "Seed"),
			Create("{world_size}", "WorldSize"),
			Create("{mode}", "Mode"),
			Create("{crossplay}", "Crossplay"),
			Create("{crossplay_public_ip}", "CrossplayPublicIp"),
			Create("{ram}", "Ram"),
			Create("{rcon}", "RconArguments"),
			Create("{steamAppID}", "InstalledSteamAppId"),
			Create("{appid}", "DefinitionSteamAppId"),
			Create("{PublicIP}", "PublicIp"),
			Create("{InstallPath}", "InstallPath")
		];

		internal static IReadOnlyList<GameDefinitionArgumentTag> RconSyntax { get; } =
		[
			Create("{rcon_port}", "RconPort"),
			Create("{rcon_pass}", "RconPassword"),
			Create("{rcon_enabled}", "RconEnabled"),
			Create("{adminpass}", "SharedAdminPassword"),
			Create("{steamAppID}", "InstalledSteamAppId")
		];

		private static GameDefinitionArgumentTag Create(string token, string resourceSuffix) =>
			new(
				token,
				LocalizationManager.Get($"GameDefinition.Tag.{resourceSuffix}.Name"),
				LocalizationManager.Get($"GameDefinition.Tag.{resourceSuffix}.Description"));

		private static readonly HashSet<string> LaunchTokens =
			LaunchArguments.Select(tag => tag.Token).ToHashSet(StringComparer.Ordinal);
		private static readonly HashSet<string> RconTokens =
			RconSyntax.Select(tag => tag.Token).ToHashSet(StringComparer.Ordinal);

		internal static void ValidateLaunchArguments(
			string arguments,
			string resourceName) =>
			Validate(arguments, LaunchTokens, "arguments", resourceName);

		internal static void ValidateRconSyntax(
			string syntax,
			string resourceName) =>
			Validate(syntax, RconTokens, "rconSyntax", resourceName);

		private static void Validate(
			string value,
			HashSet<string> supportedTokens,
			string field,
			string resourceName)
		{
			if (string.IsNullOrEmpty(value))
				return;

			string remaining = TagPattern().Replace(value, match =>
			{
				string token = match.Value;
				if (!supportedTokens.Contains(token))
				{
					throw new InvalidDataException(
						LocalizationManager.Get(
							"GameDefinition.Error.UnsupportedTag",
							resourceName,
							field,
							token));
				}
				return string.Empty;
			});
			if (remaining.Contains('{') || remaining.Contains('}'))
			{
				throw new InvalidDataException(
					LocalizationManager.Get(
						"GameDefinition.Error.IncompleteTag",
						resourceName,
						field));
			}
		}

		[GeneratedRegex("\\{[^{}]+\\}", RegexOptions.CultureInvariant)]
		private static partial Regex TagPattern();
	}
}
