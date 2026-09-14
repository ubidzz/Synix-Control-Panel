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
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.ServerHandler;

namespace Synix_Control_Panel.SynixApp.UI.ServerManagement
{
	internal sealed class MinecraftConsoleDialog : Form
	{
		private readonly GameServer _server;
		private readonly RichTextBox _output;
		private readonly TextBox _command;
		private readonly Label _status;
		private readonly ModernSettingsButton _send;
		private readonly ToolTip _quickCommandTips = new();
		private readonly System.Collections.Concurrent.ConcurrentQueue<MinecraftConsoleLine> _pendingLines = new();
		private readonly System.Windows.Forms.Timer _lineTimer = new() { Interval = 150 };

		private bool _sending;

		internal MinecraftConsoleDialog(GameServer server, bool embedded = false)
		{
			_lineTimer.Tick += (_, _) =>
			{
				for (int i = 0; i < 50 && _pendingLines.TryDequeue(out MinecraftConsoleLine? line); i++) AppendLine(line);
			};
			Shown += (_, _) => _lineTimer.Start();
			Disposed += (_, _) => { _lineTimer.Dispose(); _pendingLines.Clear(); };
			_server = server ?? throw new ArgumentNullException(nameof(server));
			Text = LocalizationManager.Get("Menu.MinecraftServerConsole");
			StartPosition = FormStartPosition.CenterParent;
			ShowInTaskbar = false;
			MinimumSize = embedded ? Size.Empty : new Size(780, 540);
			ClientSize = new Size(980, 680);
			BackColor = SettingsPalette.Window;
			ForeColor = SettingsPalette.PrimaryText;
			Font = new Font("Segoe UI", 9.5F);

			Controls.Add(new Label
			{
				Text = LocalizationManager.Get("Menu.MinecraftServerConsole"),
				Font = new Font("Segoe UI", 19F, FontStyle.Bold),
				Location = new Point(28, 22),
				Size = new Size(620, 42),
				ForeColor = SettingsPalette.PrimaryText,
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			});
			Controls.Add(new Label
			{
				Text = LocalizationManager.Get(
					"MinecraftConsole.ServerSummary",
					_server.ServerName,
					MinecraftControlProfile.NormalizeEdition(_server.MinecraftEdition)),
				Location = new Point(30, 66),
				Size = new Size(900, 26),
				ForeColor = SettingsPalette.SecondaryText,
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			});

			_output = new RichTextBox
			{
				Name = "minecraftConsoleOutput",
				Location = new Point(28, 106),
				Size = new Size(924, 338),
				Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
				BackColor = SettingsPalette.Input,
				ForeColor = SettingsPalette.PrimaryText,
				BorderStyle = BorderStyle.FixedSingle,
				ReadOnly = true,
				Multiline = true,
				DetectUrls = false,
				Font = new Font("Cascadia Mono", 9F),
				WordWrap = true,
				ScrollBars = RichTextBoxScrollBars.Vertical
			};
			Controls.Add(_output);

			Controls.Add(new Label
			{
				Text = LocalizationManager.Get("Text.EC423125FB2CAF05C95B"),
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
				PlaceholderText = LocalizationManager.Get(
					"MinecraftConsole.CommandPlaceholder",
					"say Server maintenance in 5 minutes")
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
				Text = LocalizationManager.Get("Text.93D8CAEF74F07185F0BF"),
				Location = new Point(774, 584),
				Size = new Size(178, 44),
				Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
				UseAccentStyle = true
			};
			_send.Click += async (_, _) => await SendCommandAsync();
			Controls.AddRange([_command, _send]);

			_status = new Label
			{
				Text = LocalizationManager.Get("Text.B09F5332EE858BE57F71"),
				Location = new Point(28, 638),
				Size = new Size(924, 24),
				Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
				ForeColor = SettingsPalette.SecondaryText
			};
			Controls.Add(_status);

			MinecraftConsoleHub.LineReceived += HandleLineReceived;
			if (!embedded) ThemeManager.Apply(this);
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
				new(
					LocalizationManager.Get("MinecraftConsole.Quick.Announce.Label"),
					"say ",
					LocalizationManager.Get("MinecraftConsole.Quick.Announce.Help")),
				new(
					LocalizationManager.Get("MinecraftConsole.Quick.List.Label"),
					"list",
					LocalizationManager.Get("MinecraftConsole.Quick.List.Help")),
				new(
					LocalizationManager.Get("MinecraftConsole.Quick.Kick.Label"),
					"kick ",
					LocalizationManager.Get("MinecraftConsole.Quick.Kick.Help")),
				new(
					LocalizationManager.Get("PlayerCenter.Action.Operator"),
					"op ",
					LocalizationManager.Get("MinecraftConsole.Quick.Operator.Help")),
				new(
					LocalizationManager.Get("MinecraftConsole.Quick.Deop.Label"),
					"deop ",
					LocalizationManager.Get("MinecraftConsole.Quick.Deop.Help")),
				isJava
					? new(
						LocalizationManager.Get("MinecraftConsole.Quick.Whitelist.Label"),
						"whitelist list",
						LocalizationManager.Get("MinecraftConsole.Quick.Whitelist.Help"))
					: new(
						LocalizationManager.Get("MinecraftConsole.Quick.Allowlist.Label"),
						"allowlist list",
						LocalizationManager.Get("MinecraftConsole.Quick.Allowlist.Help")),
				new(
					LocalizationManager.Get("MinecraftConsole.Quick.Day.Label"),
					"time set day",
					LocalizationManager.Get("MinecraftConsole.Quick.Day.Help")),
				new(
					LocalizationManager.Get("MinecraftConsole.Quick.Weather.Label"),
					"weather clear",
					LocalizationManager.Get("MinecraftConsole.Quick.Weather.Help")),
				new(
					LocalizationManager.Get("MinecraftConsole.Quick.Help.Label"),
					"help",
					LocalizationManager.Get("MinecraftConsole.Quick.Help.Help"))
			];

			if (isJava)
			{
				commands.Add(new(
					LocalizationManager.Get("MinecraftConsole.Quick.Save.Label"),
					"save-all",
					LocalizationManager.Get("MinecraftConsole.Quick.Save.Help")));
			}

			commands.Add(new(
				LocalizationManager.Get("Text.CB7C1E8DC21E59830716"),
				"stop",
				LocalizationManager.Get("MinecraftConsole.Quick.Stop.Help"),
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
				AccessibleName = LocalizationManager.Get(
					"MinecraftConsole.Quick.AccessibleName",
					quickCommand.Label),
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
			if (_sending || IsDisposed) return;
			_sending = true;
			string command = _command.Text;
			_send.Enabled = false;
			try
			{
				(bool succeeded, string message) = await Servers.SendMinecraftCommandAsync(
					_server,
					command);
				if (IsDisposed) return;
				_status.Text = message;
				_status.ForeColor = succeeded ? SettingsPalette.Success : SettingsPalette.Warning;
				if (succeeded && _command.Text == command)
					_command.Clear();
			}
			catch (Exception exception)
			{
				if (!IsDisposed) _status.Text = Core.SanitizeProblemReportText(exception.Message);
			}
			finally
			{
				_sending = false;
				if (!IsDisposed) { _send.Enabled = true; _command.Focus(); }
			}
		}

		private void HandleLineReceived(GameServer server, MinecraftConsoleLine line)
		{
			if (!MinecraftConsoleHub.IsSameServer(_server, server) || IsDisposed)
				return;
			_pendingLines.Enqueue(line);
			while (_pendingLines.Count > 2000) _pendingLines.TryDequeue(out _);
		}

		private void AppendLine(MinecraftConsoleLine line)
		{
			if (_output.TextLength > 200000)
			{
				_output.Select(0, _output.TextLength - 100000);
				_output.SelectedText = string.Empty;
			}
			_output.SelectionStart = _output.TextLength;
			_output.SelectionColor = line.IsError ? SettingsPalette.Danger : SettingsPalette.PrimaryText;
			// Saved Windows logs can retain a trailing CR, while live output may contain
			// multiline errors. Preserve internal breaks and append one record terminator.
			string text = SecretRedactor.Redact(line.Text).ReplaceLineEndings(Environment.NewLine).TrimEnd('\r', '\n');
			if (text.Length > 8192) text = text[..8192] + "…";
			_output.AppendText($"[{line.Timestamp:HH:mm:ss}] {text}{Environment.NewLine}");
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
