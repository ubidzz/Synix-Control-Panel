// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Design;

namespace Synix_Control_Panel.SynixEngine
{
	internal sealed class PlayerManagementCenter : Form
	{
		private readonly GameServer _server;
		private readonly DataGridView _grid;
		private readonly Label _summary;
		private readonly Label _status;
		private readonly ModernSettingsButton _refresh;

		internal PlayerManagementCenter(GameServer server)
		{
			_server = server ?? throw new ArgumentNullException(nameof(server));
			Text = "Player Management Center";
			StartPosition = FormStartPosition.CenterParent;
			ShowInTaskbar = false;
			MinimizeBox = false;
			MaximizeBox = true;
			MinimumSize = new Size(760, 520);
			ClientSize = new Size(900, 620);
			BackColor = SettingsPalette.Window;
			ForeColor = SettingsPalette.PrimaryText;
			Font = new Font("Segoe UI", 9.5F);

			Controls.Add(new Label
			{
				Text = "Player Management Center",
				Font = new Font("Segoe UI", 19F, FontStyle.Bold),
				Location = new Point(28, 22),
				Size = new Size(620, 42),
				ForeColor = SettingsPalette.PrimaryText,
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			});
			_summary = new Label
			{
				Text = $"{_server.ServerName} • {_server.Game} • {_server.PlayerCount}",
				Location = new Point(30, 66),
				Size = new Size(820, 28),
				ForeColor = SettingsPalette.SecondaryText,
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			};
			Controls.Add(_summary);

			_grid = new DataGridView
			{
				Location = new Point(28, 112),
				Size = new Size(844, 388),
				Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
				ReadOnly = true,
				AllowUserToAddRows = false,
				AllowUserToDeleteRows = false,
				AllowUserToResizeRows = false,
				AutoGenerateColumns = false,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				MultiSelect = false,
				RowHeadersVisible = false
			};
			_grid.Columns.Add(new DataGridViewTextBoxColumn
			{
				Name = "PlayerName",
				HeaderText = "PLAYER",
				DataPropertyName = nameof(GamePlayerInfo.Name),
				AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
				FillWeight = 60
			});
			_grid.Columns.Add(new DataGridViewTextBoxColumn
			{
				Name = "Score",
				HeaderText = "SCORE",
				DataPropertyName = nameof(GamePlayerInfo.Score),
				Width = 150
			});
			_grid.Columns.Add(new DataGridViewTextBoxColumn
			{
				Name = "Connected",
				HeaderText = "CONNECTED",
				Width = 190
			});
			GridStyler.DarkTheme(_grid);
			GridStyler.ApplyDashboardTheme(_grid);
			Controls.Add(_grid);

			_status = new Label
			{
				Text = "Refresh to load player details directly from the local server.",
				Location = new Point(28, 516),
				Size = new Size(520, 54),
				ForeColor = SettingsPalette.SecondaryText,
				Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
			};
			Controls.Add(_status);

			_refresh = new ModernSettingsButton
			{
				Text = "Refresh Players",
				Location = new Point(566, 532),
				Size = new Size(148, 44),
				UseAccentStyle = true,
				Anchor = AnchorStyles.Bottom | AnchorStyles.Right
			};
			_refresh.Click += async (_, _) => await RefreshPlayersAsync();
			ModernSettingsButton close = new()
			{
				Text = "Close",
				Location = new Point(726, 532),
				Size = new Size(146, 44),
				DialogResult = DialogResult.OK,
				Anchor = AnchorStyles.Bottom | AnchorStyles.Right
			};
			Controls.AddRange([_refresh, close]);
			CancelButton = close;
			ThemeManager.Apply(this);
		}

		protected override async void OnShown(EventArgs eventArgs)
		{
			base.OnShown(eventArgs);
			await RefreshPlayersAsync();
		}

		private async Task RefreshPlayersAsync()
		{
			_refresh.Enabled = false;
			_status.Text = "Loading player details…";
			try
			{
				PlayerQueryResult result = await PlayerQueryService.QueryAsync(_server);
				_grid.Rows.Clear();
				foreach (GamePlayerInfo player in result.Players)
				{
					int row = _grid.Rows.Add(player.Name, player.Score, FormatDuration(player.ConnectedFor));
					_grid.Rows[row].Tag = player;
				}
				_summary.Text = $"{_server.ServerName} • {_server.Game} • {result.Players.Count} named player(s)";
				_status.Text = result.Message + (result.IsSupported
					? " Player actions remain disabled unless a game provides a verified administration protocol."
					: string.Empty);
				_status.ForeColor = result.IsSuccessful
					? SettingsPalette.Success
					: result.IsSupported ? SettingsPalette.Warning : SettingsPalette.SecondaryText;
			}
			finally
			{
				_refresh.Enabled = true;
			}
		}

		private static string FormatDuration(TimeSpan duration) =>
			duration.TotalHours >= 1
				? $"{(int)duration.TotalHours}h {duration.Minutes:D2}m"
				: $"{duration.Minutes}m {duration.Seconds:D2}s";
	}
}
