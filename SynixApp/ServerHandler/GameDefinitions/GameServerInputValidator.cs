// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixEngine;

namespace Synix_Control_Panel.SynixApp.ServerHandler
{
	internal static class GameServerInputValidator
	{
		internal static bool TryValidate(
			GameInfo definition,
			string? serverName,
			SynixServerPasswords passwords,
			out string error)
		{
			ArgumentNullException.ThrowIfNull(definition);

			string password = passwords.ServerPassword ?? string.Empty;
			if (password.Length < definition.MinimumServerPasswordLength)
			{
				error = $"{definition.Game} requires a server password with at least " +
					$"{definition.MinimumServerPasswordLength} characters.";
				return false;
			}

			if (definition.ServerPasswordMustNotAppearInName &&
				!string.IsNullOrEmpty(password) &&
				(serverName ?? string.Empty).Contains(
					password,
					StringComparison.OrdinalIgnoreCase))
			{
				error = $"{definition.Game} does not allow the server password to appear in the server name.";
				return false;
			}

			string authenticationToken = passwords.AuthenticationToken ?? string.Empty;
			string authenticationTokenLabel = string.IsNullOrWhiteSpace(
				definition.AuthenticationTokenLabel)
					? "authentication token"
					: definition.AuthenticationTokenLabel;
			if (definition.RequiresAuthenticationToken &&
				string.IsNullOrWhiteSpace(authenticationToken))
			{
				error = $"{definition.Game} requires a valid {authenticationTokenLabel} for online server authentication.";
				return false;
			}

			if (!string.IsNullOrEmpty(authenticationToken) &&
				(authenticationToken.Length > 4096 ||
				 authenticationToken.Any(character =>
					char.IsControl(character) ||
					char.IsWhiteSpace(character) ||
					character is '"' or '\'' or '&' or '|' or '<' or '>' or '^' or '%' or '!')))
			{
				error = $"The {authenticationTokenLabel} contains characters that cannot be passed safely to the server.";
				return false;
			}

			error = string.Empty;
			return true;
		}
	}
}
