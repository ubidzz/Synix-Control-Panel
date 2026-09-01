// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.ServerHandler;

namespace Synix_Control_Panel.SynixEngine
{
	internal sealed class MinecraftConsoleDialog : Form
	{
		private readonly GameServer _server;
		private readonly RichTextBox _output;
		private readonly TextBox _command;
		private readonly Label _status;
		private readonly ModernSettingsButton _send;
		private readonly ToolTip _quickCommandTips = new();

		internal MinecraftConsoleDialog(GameServer server)
		{
			_server = server ?? throw new ArgumentNullException(nameof(server));
			Text = "Minecraft Server Console";
			StartPosition = FormStartPosition.CenterParent;
			ShowInTaskbar = false;
			MinimumSize = new Size(780, 540);
			ClientSize = new Size(980, 680);
			BackColor = SettingsPalette.Window;
			ForeColor = SettingsPalette.PrimaryText;
			Font = new Font("Segoe UI", 9.5F);

			Controls.Add(new Label
			{
				Text = "Minecraft Server Console",
				Font = new Font("Segoe UI", 19F, FontStyle.Bold),
				Location = new Point(28, 22),
				Size = new Size(620, 42),
				ForeColor = SettingsPalette.PrimaryText,
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			});
			Controls.Add(new Label
			{
				Text = $"{_server.ServerName} • Minecraft {MinecraftControlProfile.NormalizeEdition(_server.MinecraftEdition)}",
				Location = new Point(30, 66),
				Size = new Size(900, 26),
				ForeColor = SettingsPalette.SecondaryText,
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			});

			_output = new RichTextBox
			{
				Location = new Point(28, 106),
				Size = new Size(924, 338),
				Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
				BackColor = Color.FromArgb(6, 12, 22),
				ForeColor = SettingsPalette.PrimaryText,
				BorderStyle = BorderStyle.FixedSingle,
				ReadOnly = true,
				DetectUrls = false,
				Font = new Font("Cascadia Mono", 9F),
				WordWrap = false
			};
			Controls.Add(_output);

			Controls.Add(new Label
			{
				Text = "Quick Commands — choose one to prepare it, then review and send it",
				Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
				Location = new Point(28, 458),
				Size = new Size(924, 24),
				Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
				ForeColor = SettingsPalette.SecondaryText
			});

			FlowLayoutPanel quickCommands = new()
			{
				Name = "minecraftQuickCommands",
				Location = new Point(28, 486),
				Size = new Size(924, 86),
				Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
				BackColor = SettingsPalette.Window,
				FlowDirection = FlowDirection.LeftToRight,
				WrapContents = true,
				AutoScroll = false,
				Margin = Padding.Empty,
				Padding = Padding.Empty
			};
			foreach (MinecraftQuickCommand quickCommand in GetQuickCommands(_server))
			{
				quickCommands.Controls.Add(CreateQuickCommandButton(quickCommand));
			}
			Controls.Add(quickCommands);

			_command = new TextBox
			{
				Name = "minecraftCommandInput",
				Location = new Point(28, 590),
				Size = new Size(730, 32),
				Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
				BackColor = SettingsPalette.Input,
				ForeColor = SettingsPalette.PrimaryText,
				BorderStyle = BorderStyle.FixedSingle,
				PlaceholderText = "Enter a server command, for example: say Server maintenance in 5 minutes"
			};
			_command.KeyDown += async (_, eventArgs) =>
			{
				if (eventArgs.KeyCode != Keys.Enter)
					return;
				eventArgs.SuppressKeyPress = true;
				await SendCommandAsync();
			};

			_send = new ModernSettingsButton
			{
				Name = "minecraftSendCommand",
				Text = "Send Command",
				Location = new Point(774, 584),
				Size = new Size(178, 44),
				Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
				UseAccentStyle = true
			};
			_send.Click += async (_, _) => await SendCommandAsync();
			Controls.AddRange([_command, _send]);

			_status = new Label
			{
				Text = "Commands stay on this computer unless you intentionally configure Java RCON for remote access.",
				Location = new Point(28, 638),
				Size = new Size(924, 24),
				Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
				ForeColor = SettingsPalette.SecondaryText
			};
			Controls.Add(_status);

			MinecraftConsoleHub.LineReceived += HandleLineReceived;
			ThemeManager.Apply(this);
		}

		protected override void OnShown(EventArgs eventArgs)
		{
			base.OnShown(eventArgs);
			foreach (MinecraftConsoleLine line in MinecraftConsoleHub.GetSnapshot(_server))
				AppendLine(line);
			_command.Focus();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				MinecraftConsoleHub.LineReceived -= HandleLineReceived;
				_quickCommandTips.Dispose();
			}
			base.Dispose(disposing);
		}

		internal static IReadOnlyList<MinecraftQuickCommand> GetQuickCommands(GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);
			bool isJava = MinecraftControlProfile.IsJava(server);
			List<MinecraftQuickCommand> commands =
			[
				new("Announce", "say ", "Type the announcement after 'say', then send it."),
				new("List Players", "list", "Shows the players currently connected to the server."),
				new("Kick Player", "kick ", "Type the player's name after 'kick', then send it."),
				new("Make Operator", "op ", "Type the player's name after 'op', then send it."),
				new("Remove Operator", "deop ", "Type the player's name after 'deop', then send it."),
				isJava
					? new("View Whitelist", "whitelist list", "Shows every player on the Java Edition whitelist.")
					: new("View Allowlist", "allowlist list", "Shows every player on the Bedrock Edition allowlist."),
				new("Set Day", "time set day", "Changes the current world time to daytime."),
				new("Clear Weather", "weather clear", "Clears rain and thunderstorms in the current world."),
				new("Command Help", "help", "Shows the commands supported by this Minecraft server.")
			];

			if (isJava)
			{
				commands.Add(new(
					"Save World",
					"save-all",
					"Requests an immediate Java Edition world save."));
			}

			commands.Add(new(
				"Stop Server",
				"stop",
				"CAUTION: Sending this command saves and shuts down the Minecraft server.",
				true));
			return commands;
		}

		private ModernSettingsButton CreateQuickCommandButton(MinecraftQuickCommand quickCommand)
		{
			using Font buttonFont = new("Segoe UI", 9F, FontStyle.Bold);
			int textWidth = TextRenderer.MeasureText(quickCommand.Label, buttonFont).Width;
			ModernSettingsButton button = new()
			{
				Name = "minecraftQuick" + new string(
					quickCommand.Label.Where(char.IsLetterOrDigit).ToArray()),
				Text = quickCommand.Label,
				Size = new Size(Math.Clamp(textWidth + 30, 94, 142), 34),
				Margin = new Padding(0, 0, 8, 8),
				Font = new Font("Segoe UI", 9F, FontStyle.Bold),
				ForeColor = quickCommand.IsDangerous
					? SettingsPalette.Danger
					: SettingsPalette.PrimaryText,
				AccessibleName = quickCommand.Label + " quick command",
				AccessibleDescription = quickCommand.Guidance
			};
			button.Click += (_, _) => PrepareQuickCommand(quickCommand);
			_quickCommandTips.SetToolTip(button, quickCommand.Guidance);
			return button;
		}

		private void PrepareQuickCommand(MinecraftQuickCommand quickCommand)
		{
			_command.Text = quickCommand.Command;
			_command.SelectionStart = _command.TextLength;
			_command.SelectionLength = 0;
			_status.Text = quickCommand.Guidance;
			_status.ForeColor = quickCommand.IsDangerous
				? SettingsPalette.Danger
				: SettingsPalette.SecondaryText;
			_command.Focus();
		}

		private async Task SendCommandAsync()
		{
			string command = _command.Text;
			_send.Enabled = false;
			try
			{
				(bool succeeded, string message) = await Servers.SendMinecraftCommandAsync(
					_server,
					command);
				_status.Text = message;
				_status.ForeColor = succeeded ? SettingsPalette.Success : SettingsPalette.Warning;
				if (succeeded)
					_command.Clear();
			}
			finally
			{
				_send.Enabled = true;
				_command.Focus();
			}
		}

		private void HandleLineReceived(GameServer server, MinecraftConsoleLine line)
		{
			if (!MinecraftConsoleHub.IsSameServer(_server, server) || IsDisposed)
				return;
			if (InvokeRequired)
			{
				BeginInvoke(new Action(() => AppendLine(line)));
				return;
			}
			AppendLine(line);
		}

		private void AppendLine(MinecraftConsoleLine line)
		{
			_output.SelectionStart = _output.TextLength;
			_output.SelectionColor = line.IsError ? SettingsPalette.Danger : SettingsPalette.PrimaryText;
			_output.AppendText($"[{line.Timestamp:HH:mm:ss}] {line.Text}{Environment.NewLine}");
			_output.SelectionStart = _output.TextLength;
			_output.ScrollToCaret();
		}
	}

	internal sealed record MinecraftQuickCommand(
		string Label,
		string Command,
		string Guidance,
		bool IsDangerous = false);
}
