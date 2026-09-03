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
			new("{ServerName}", "Server name", "The server name entered by the user."),
			new("{Identity}", "Safe server identity", "A folder-safe version of the server name."),
			new("{port}", "Game port", "The main game connection port."),
			new("{query}", "Query port", "The Steam or server-browser query port."),
			new("{app_port}", "Additional app port", "The optional extra application port."),
			new("{MaxPlayers}", "Maximum players", "The maximum player count selected by the user."),
			new("{pass}", "Server password", "The saved player/server password."),
			new("{adminpass}", "Administrator password", "The saved administrator password."),
			new("{auth_token}", "Online authentication token", "The protected third-party authentication token required by this server."),
			new("{map}", "Map or world name", "The selected map, scenario, shard, or world name."),
			new("{seed}", "World seed", "The selected world-generation seed."),
			new("{world_size}", "World size", "The selected world size."),
			new("{mode}", "Game mode", "The selected game mode or PVE/PVP value."),
			new("{crossplay}", "Crossplay", "The game-specific enabled value, flag, or no argument when Crossplay is disabled."),
			new("{crossplay_public_ip}", "Crossplay public IPv4 argument", "The ARK: Survival Evolved Epic public-IP argument when Crossplay is enabled and the address is available."),
			new("{ram}", "Memory limit", "The configured memory value in megabytes."),
			new("{rcon}", "Optional RCON arguments", "The RCON recipe below, or nothing when RCON is disabled."),
			new("{steamAppID}", "Installed Steam AppID", "The AppID Synix determined for the installed server."),
			new("{appid}", "Definition Steam AppID", "The Steam AppID stored in this game definition."),
			new("{PublicIP}", "Current public IPv4 address", "The current public IPv4 address, or no argument when it cannot be determined."),
			new("{InstallPath}", "Server install folder", "The complete server installation folder.")
		];

		internal static IReadOnlyList<GameDefinitionArgumentTag> RconSyntax { get; } =
		[
			new("{rcon_port}", "RCON port", "The RCON port selected by the user."),
			new("{rcon_pass}", "RCON password", "The saved RCON password."),
			new("{rcon_enabled}", "RCON enabled value", "The exact enabled value required by this game, such as true, True, or 1."),
			new("{adminpass}", "Administrator password", "The saved administrator password, used by games that share it with RCON."),
			new("{steamAppID}", "Installed Steam AppID", "The AppID Synix determined for the installed server.")
		];

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
						$"{resourceName} contains unsupported or incorrectly capitalized {field} tag {token}.");
				}
				return string.Empty;
			});
			if (remaining.Contains('{') || remaining.Contains('}'))
			{
				throw new InvalidDataException(
					$"{resourceName} contains an incomplete {field} tag.");
			}
		}

		[GeneratedRegex("\\{[^{}]+\\}", RegexOptions.CultureInvariant)]
		private static partial Regex TagPattern();
	}
}
