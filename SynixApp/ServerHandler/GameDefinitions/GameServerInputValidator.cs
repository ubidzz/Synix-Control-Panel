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
				error = LocalizationManager.Get(
					"GameInput.Password.MinimumLength",
					definition.Game,
					definition.MinimumServerPasswordLength);
				return false;
			}

			if (definition.ServerPasswordMustNotAppearInName &&
				!string.IsNullOrEmpty(password) &&
				(serverName ?? string.Empty).Contains(
					password,
					StringComparison.OrdinalIgnoreCase))
			{
				error = LocalizationManager.Get(
					"GameInput.Password.NotInServerName",
					definition.Game);
				return false;
			}

			string authenticationToken = passwords.AuthenticationToken ?? string.Empty;
			string authenticationTokenLabel = string.IsNullOrWhiteSpace(
				definition.AuthenticationTokenLabel)
					? LocalizationManager.Get("GameInput.AuthenticationToken")
					: definition.AuthenticationTokenLabel;
			if (definition.RequiresAuthenticationToken &&
				string.IsNullOrWhiteSpace(authenticationToken))
			{
				error = LocalizationManager.Get(
					"GameInput.AuthenticationToken.Required",
					definition.Game,
					authenticationTokenLabel);
				return false;
			}

			if (!string.IsNullOrEmpty(authenticationToken) &&
				(authenticationToken.Length > 4096 ||
				 authenticationToken.Any(character =>
					char.IsControl(character) ||
					char.IsWhiteSpace(character) ||
					character is '"' or '\'' or '&' or '|' or '<' or '>' or '^' or '%' or '!')))
			{
				error = LocalizationManager.Get(
					"GameInput.AuthenticationToken.Unsafe",
					authenticationTokenLabel);
				return false;
			}

			error = string.Empty;
			return true;
		}
	}
}
