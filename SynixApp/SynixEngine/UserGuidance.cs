// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.ServerHandler;

namespace Synix_Control_Panel.SynixEngine
{
	internal sealed record ConfigurationSupportPresentation(
		string Status,
		string Summary,
		Color Color);

	internal enum SafetyCheckLevel
	{
		Ready,
		Review,
		Blocked
	}

	internal sealed record SafetyCheckItem(
		SafetyCheckLevel Level,
		string Name,
		string Details);

	internal sealed class SafetyChecklistReport
	{
		internal SafetyChecklistReport(IReadOnlyList<SafetyCheckItem> items) =>
			Items = items;

		internal IReadOnlyList<SafetyCheckItem> Items { get; }
		internal bool CanContinue => Items.All(item => item.Level != SafetyCheckLevel.Blocked);
		internal int CompletionPercentage
		{
			get
			{
				if (Items.Count == 0)
					return 0;
				double points = Items.Sum(item => item.Level switch
				{
					SafetyCheckLevel.Ready => 1d,
					SafetyCheckLevel.Review => 0.5d,
					_ => 0d
				});
				return (int)Math.Round(points * 100d / Items.Count);
			}
		}
	}

	internal sealed record SetupCompletionState(
		bool HasServerName,
		bool HasGame,
		bool HasInstallFolder,
		bool PortsAreValid,
		bool RequirementsAreMet,
		bool ReadyToSave);

	internal sealed record PlainEnglishError(
		string Heading,
		string Explanation,
		string NextStep,
		string TechnicalDetails);

	internal static class UserGuidance
	{
		internal static ConfigurationSupportPresentation GetConfigurationSupport(
			GameInfo? game)
		{
			if (game == null)
			{
				return new(
					"Select a game",
					"Choose a game to see exactly what Synix can configure.",
					SettingsPalette.MutedText);
			}

			return game.ConfigFileCreation switch
			{
				ConfigFileCreationMode.SynixTemplate => new(
					"Full configuration support",
					"Synix creates the configuration, applies the settings you choose, and protects existing files with backups.",
					SettingsPalette.Success),
				ConfigFileCreationMode.GameGenerated => new(
					"Guided configuration support",
					"The game creates its configuration on first start; Synix can manage it after that file exists.",
					SettingsPalette.Accent),
				ConfigFileCreationMode.LaunchArgumentsOnly => new(
					"Launch-setting support",
					"This game is configured through its launch command, so no separate managed configuration file is required.",
					SettingsPalette.Accent),
				_ => new(
					"Basic installation support",
					"Synix can install and run this server, but its game configuration is not fully managed yet.",
					SettingsPalette.Warning)
			};
		}

		internal static int CalculateSetupCompletion(SetupCompletionState state)
		{
			ArgumentNullException.ThrowIfNull(state);
			int points = 0;
			if (state.HasServerName) points += 15;
			if (state.HasGame) points += 20;
			if (state.HasInstallFolder) points += 20;
			if (state.PortsAreValid) points += 20;
			if (state.RequirementsAreMet) points += 15;
			if (state.ReadyToSave) points += 10;
			return Math.Clamp(points, 0, 100);
		}

		internal static SafetyChecklistReport BuildSafetyChecklist(GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);
			List<SafetyCheckItem> items = [];
			GameInfo? game = GameDatabase.GetGame(server.Game);

			items.Add(string.IsNullOrWhiteSpace(server.ServerName)
				? new(SafetyCheckLevel.Blocked, "Server identity", "Enter a server name before starting.")
				: new(SafetyCheckLevel.Ready, "Server identity", $"{server.ServerName} is saved as a {server.Game} server."));

			items.Add(game == null
				? new(SafetyCheckLevel.Blocked, "Game definition", "The selected game definition could not be found.")
				: new(SafetyCheckLevel.Ready, "Game definition", $"The {game.Game} launch definition is loaded."));

			items.Add(string.IsNullOrWhiteSpace(server.InstallPath)
				? new(SafetyCheckLevel.Blocked, "Installation folder", "Choose an installation folder.")
				: Directory.Exists(server.InstallPath)
					? new(SafetyCheckLevel.Ready, "Installation folder", "The selected server folder is available.")
					: new(SafetyCheckLevel.Review, "Installation folder", "Synix will create this folder and install the server files on first start."));

			bool portsValid = server.Port is >= 1 and <= 65535 &&
				(server.QueryPort == 0 || server.QueryPort is >= 1 and <= 65535) &&
				(!server.EnableRcon || server.RconPort is >= 1 and <= 65535);
			items.Add(portsValid
				? new(SafetyCheckLevel.Ready, "Network ports", GetPortSummary(server))
				: new(SafetyCheckLevel.Blocked, "Network ports", "One or more required ports are outside the valid 1–65535 range."));

			ConfigurationSupportPresentation support = GetConfigurationSupport(game);
			items.Add(game?.ConfigFileCreation == ConfigFileCreationMode.Unknown
				? new(SafetyCheckLevel.Review, "Configuration support", support.Summary)
				: new(SafetyCheckLevel.Ready, "Configuration support", support.Summary));

			if (game != null)
			{
				GamePrerequisiteItem? failure = GamePrerequisiteChecker
					.CheckCurrentSystem(game)
					.FirstFailure;
				items.Add(failure == null
					? new(SafetyCheckLevel.Ready, "Computer requirements", "This PC meets the known requirements for this game server.")
					: new(SafetyCheckLevel.Blocked, "Computer requirements", failure.Message));
			}

			return new SafetyChecklistReport(items);
		}

		internal static string GetPortSummary(GameServer server)
		{
			List<string> ports = [$"game {server.Port}"];
			if (server.QueryPort > 0 && server.QueryPort != server.Port)
				ports.Add($"query {server.QueryPort}");
			if (server.EnableRcon && server.RconPort > 0)
				ports.Add($"RCON {server.RconPort}");
			if (server.AppPort is > 0)
				ports.Add($"app {server.AppPort.Value}");
			return "Configured ports: " + string.Join(", ", ports) + ".";
		}

		internal static PlainEnglishError TranslateError(
			string operation,
			string? technicalDetails)
		{
			string details = string.IsNullOrWhiteSpace(technicalDetails)
				? "No additional technical details were provided."
				: technicalDetails.Trim();
			string searchable = details.ToLowerInvariant();

			if (searchable.Contains("game not found") || searchable.Contains("game definition"))
			{
				return new(
					$"Synix could not {operation}",
					"The saved game definition is no longer available.",
					"Edit this server and select a supported game. If the game was added by a custom definition, restore or re-import that definition first.",
					details);
			}

			if (searchable.Contains("blocked this package") ||
				searchable.Contains("security review") ||
				searchable.Contains("antivirus:"))
			{
				return new(
					$"Synix could not {operation}",
					"The selected add-on did not pass Synix's security review.",
					"Do not bypass a confirmed threat. If the scan was only unavailable or inconclusive, review the warning and install only when you trust the exact source and SHA-256 shown.",
					details);
			}

			if (searchable.Contains("port") &&
				(searchable.Contains("use") || searchable.Contains("occup") || searchable.Contains("bind")))
			{
				return new(
					$"Synix could not {operation}",
					"A network port needed by this server is already being used.",
					"Open Live Process Details and stop any older copy of this server, or edit the server and choose a free port. Then try again.",
					details);
			}

			if (searchable.Contains("not found") || searchable.Contains("missing") || searchable.Contains("could not find"))
			{
				return new(
					$"Synix could not {operation}",
					"A required server file could not be found.",
					"Use Server Options > Validate Game Files. If this is a new server, run Update Server to finish the installation.",
					details);
			}

			if (searchable.Contains("access") || searchable.Contains("permission") || searchable.Contains("unauthorized"))
			{
				return new(
					$"Synix could not {operation}",
					"Windows would not allow Synix to read or change a required file.",
					"Close any program using the server folder, confirm your Windows account can write to it, and try again.",
					details);
			}

			if (searchable.Contains("configuration") || searchable.Contains("config file"))
			{
				return new(
					$"Synix could not {operation}",
					"The server configuration is incomplete or could not be updated safely.",
					"Open the Server Readiness Center and use Fix Config when it is offered. Synix creates a backup before rebuilding a managed configuration.",
					details);
			}

			if (searchable.Contains(" requires ") || searchable.Contains("requirement"))
			{
				return new(
					$"Synix could not {operation}",
					"This computer does not meet a known requirement for the selected game server.",
					"Read the requirement shown in the technical details, install or enable the missing Windows component, and run the Readiness Center again.",
					details);
			}

			if (searchable.Contains("network") || searchable.Contains("http") || searchable.Contains("github") || searchable.Contains("timed out"))
			{
				return new(
					$"Synix could not {operation}",
					"Synix could not reach the required online service.",
					"Check the internet connection and Windows Firewall, wait a moment, and try again.",
					details);
			}

			if (searchable.Contains("process") || searchable.Contains("shutdown") || searchable.Contains("still running"))
			{
				return new(
					$"Synix could not {operation}",
					"One or more parts of the game server are still running.",
					"Open Live Process Details, wait for every process in this server group to close, and then try again. The Readiness Center can recover stale process tracking.",
					details);
			}

			return new(
				$"Synix could not {operation}",
				"The requested action did not finish.",
				"Open the Server Readiness Center for guided checks. You can copy the technical details below if you need to report the problem.",
				details);
		}
	}
}
