// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Database.GameConfigurations;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.ServerHandler;

namespace Synix_Control_Panel.SynixEngine
{
	internal sealed class GameSupportDetailsDialog : Form
	{
		private readonly Panel _content;

		internal GameSupportDetailsDialog(GameInfo game)
		{
			ArgumentNullException.ThrowIfNull(game);
			Text = $"{game.Game} Support Details";
			StartPosition = FormStartPosition.CenterParent;
			ShowInTaskbar = false;
			MinimizeBox = false;
			MaximizeBox = false;
			FormBorderStyle = FormBorderStyle.FixedDialog;
			ClientSize = new Size(840, 690);
			BackColor = SettingsPalette.Window;
			ForeColor = SettingsPalette.PrimaryText;
			Font = new Font("Segoe UI", 9.5F);

			Controls.Add(new Label
			{
				Text = game.Game,
				Font = new Font("Segoe UI", 19F, FontStyle.Bold),
				Location = new Point(28, 22),
				Size = new Size(784, 42),
				ForeColor = SettingsPalette.PrimaryText,
				AutoEllipsis = true
			});
			Controls.Add(new Label
			{
				Text = "What Synix currently knows how to install, configure, start, monitor, and query for this game.",
				Location = new Point(30, 66),
				Size = new Size(780, 28),
				ForeColor = SettingsPalette.SecondaryText
			});

			_content = new Panel
			{
				Location = new Point(24, 104),
				Size = new Size(792, 516),
				AutoScroll = true,
				Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
			};
			Controls.Add(_content);

			BuildDetails(game);

			ModernSettingsButton close = new()
			{
				Text = "Close",
				Location = new Point(654, 634),
				Size = new Size(158, 44),
				DialogResult = DialogResult.OK,
				UseAccentStyle = true,
				Anchor = AnchorStyles.Bottom | AnchorStyles.Right
			};
			Controls.Add(close);
			AcceptButton = close;
			CancelButton = close;
			ThemeManager.Apply(this);
		}

		private void BuildDetails(GameInfo game)
		{
			GameCompatibilitySummary compatibility = Core.GetGameCompatibilitySummary(game.Game);
			ConfigurationSupportPresentation configuration = UserGuidance.GetConfigurationSupport(game);
			GameManagementCapability capabilities = GameFix.GetManagementCapabilities(game);
			string lastVerified = compatibility.Verification.LastTested is GameVerificationEvidence evidence
				? $"{evidence.VerifiedAtUtc.ToLocalTime():MMMM d, yyyy} with Synix {evidence.SynixVersion}"
				: "Not verified by a Synix user yet";

			AddSection(
				"Current Synix support",
				new[]
				{
					("Compatibility", compatibility.DisplayName),
					("Last verified", lastVerified),
					("Configuration", configuration.Status)
				},
				configuration.Summary);

			AddSection(
				"Server program and connections",
				new[]
				{
					("Server program", GameDatabase.IsMinecraft(game.Game)
						? "Java: Start.bat • Bedrock: bedrock_server.exe"
						: game.ExeName),
					("Steam App ID", string.IsNullOrWhiteSpace(game.AppID) ? "Not used" : game.AppID),
					("Default ports", FormatPorts(game)),
					("Player details", GetPlayerDetails(game)),
					("Crossplay option", GameDatabase.IsMinecraft(game.Game)
						? "Bedrock supports Bedrock clients across supported platforms"
						: capabilities.HasFlag(GameManagementCapability.Crossplay) ? "Available in Synix" : "Not listed in this game definition"),
					("Status check", FormatProbe(game))
				});

			AddSection(
				"Configuration and startup",
				new[]
				{
					("Configuration file", FormatConfigurationFile(game)),
					("Server window", game.LaunchBehavior.RequiresVisibleWindow ? "Visible while the server runs" : "Runs in the background when supported"),
					("Process tracking", game.LaunchBehavior.LifecycleTracking == GameLifecycleTrackingMode.ExternalDeployment ? "External deployment" : "Synix-managed server process"),
					("Required launch files", GameDatabase.IsMinecraft(game.Game)
						? "Edition-specific and verified after installation"
						: game.RequiredLaunchFiles.Length == 0 ? "None" : string.Join(", ", game.RequiredLaunchFiles.Select(Path.GetFileName)))
				},
				"A support status describes Synix's current definition. It does not mean the game itself lacks features that are not listed here.");
		}

		private void AddSection(
			string title,
			IReadOnlyList<(string Label, string Value)> rows,
			string? note = null)
		{
			int cardHeight = 54 + (rows.Count * 32) + (string.IsNullOrWhiteSpace(note) ? 10 : 62);
			int top = _content.Controls.Count == 0
				? 0
				: _content.Controls.Cast<Control>().Max(control => control.Bottom) + 14;
			ModernSettingsCard card = new()
			{
				Location = new Point(4, top),
				Size = new Size(760, cardHeight),
				FillColor = SettingsPalette.Card,
				BorderColor = SettingsPalette.Divider
			};
			card.Controls.Add(new Label
			{
				Text = title,
				Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
				Location = new Point(20, 15),
				Size = new Size(710, 26),
				ForeColor = SettingsPalette.PrimaryText
			});

			int rowTop = 48;
			foreach ((string label, string value) in rows)
			{
				card.Controls.Add(new Label
				{
					Text = label,
					Location = new Point(20, rowTop),
					Size = new Size(190, 25),
					ForeColor = SettingsPalette.SecondaryText
				});
				card.Controls.Add(new Label
				{
					Text = string.IsNullOrWhiteSpace(value) ? "Not specified" : value,
					Location = new Point(218, rowTop),
					Size = new Size(516, 25),
					ForeColor = SettingsPalette.PrimaryText,
					AutoEllipsis = true
				});
				rowTop += 32;
			}

			if (!string.IsNullOrWhiteSpace(note))
			{
				card.Controls.Add(new Label
				{
					Text = note,
					Location = new Point(20, rowTop + 4),
					Size = new Size(714, 48),
					ForeColor = SettingsPalette.SecondaryText
				});
			}

			_content.Controls.Add(card);
		}

		private static string FormatPorts(GameInfo game)
		{
			if (GameDatabase.IsMinecraft(game.Game))
				return "Java 25565 • Bedrock UDP 19132 and IPv6 UDP 19133";

			List<string> ports = [];
			if (game.Port > 0)
				ports.Add($"game {game.Port}");
			if (game.QueryPort > 0)
				ports.Add($"query {game.QueryPort}");
			if (game.AppPort > 0)
				ports.Add($"app {game.AppPort}");
			return ports.Count == 0 ? "Assigned during setup" : string.Join(", ", ports);
		}

		private static string GetPlayerDetails(GameInfo game) =>
			game.CrossplayDisablesPlayerTracking
				? "Named players and player count in Steam mode; unavailable in Crossplay mode"
				: GameDatabase.GetProbeProtocol(game) == ServerProbeProtocol.A2S
				? "Named players and player count"
				: GameDatabase.IsMinecraft(game.Game)
					? "Player count; Java player names when local management or RCON is available"
					: "Not available from the current status check";

		private static string FormatProbe(GameInfo game) =>
			GameDatabase.IsMinecraft(game.Game)
				? "Java status protocol or Bedrock RakNet status"
				: game.CrossplayDisablesPlayerTracking
					? "Steam server query; Crossplay uses PlayFab"
				: GameDatabase.GetProbeProtocol(game) switch
			{
				ServerProbeProtocol.A2S => "Steam server query",
				ServerProbeProtocol.EpicOnlineServices => "Epic Online Services",
				ServerProbeProtocol.RestApi => "Game web API",
				ServerProbeProtocol.Tcp => "Network port check",
				_ => "Automatic"
			};

		private static string FormatConfigurationFile(GameInfo game)
		{
			if (!string.IsNullOrWhiteSpace(game.RelativeConfigPath))
				return game.RelativeConfigPath;
			return game.ConfigFileCreation switch
			{
				ConfigFileCreationMode.LaunchArgumentsOnly => "Uses launch settings",
				ConfigFileCreationMode.GameGenerated => "Created by the game on first start",
				ConfigFileCreationMode.SynixTemplate => "Created and managed by Synix",
				_ => "Not defined yet"
			};
		}
	}
}
