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
	internal sealed class GameSupportCatalog : Form
	{
		private readonly TextBox _search;
		private readonly DataGridView _grid;
		private readonly Label _count;
		private readonly ModernSettingsButton _viewDetails;
		private readonly IReadOnlyList<GameSupportRow> _allRows;

		internal GameSupportCatalog()
		{
			Text = "Game Support Catalog";
			StartPosition = FormStartPosition.CenterParent;
			ShowInTaskbar = false;
			MinimumSize = new Size(980, 600);
			ClientSize = new Size(1180, 720);
			BackColor = SettingsPalette.Window;
			ForeColor = SettingsPalette.PrimaryText;
			Font = new Font("Segoe UI", 9.5F);
			_allRows = GameDatabase.GetGameList().Select(CreateRow).ToArray();

			Controls.Add(new Label
			{
				Text = "Game Support Catalog",
				Font = new Font("Segoe UI", 19F, FontStyle.Bold),
				Location = new Point(28, 22),
				Size = new Size(620, 42),
				ForeColor = SettingsPalette.PrimaryText
			});
			Controls.Add(new Label
			{
				Text = "See exactly what Synix can install, configure, monitor, and query before creating a server.",
				Location = new Point(30, 66),
				Size = new Size(840, 28),
				ForeColor = SettingsPalette.SecondaryText
			});
			_search = new TextBox
			{
				Location = new Point(28, 108),
				Size = new Size(520, 38),
				BackColor = SettingsPalette.Input,
				ForeColor = SettingsPalette.PrimaryText,
				BorderStyle = BorderStyle.FixedSingle,
				Font = new Font("Segoe UI", 10F),
				PlaceholderText = "Search by game, executable, or support status…",
				Anchor = AnchorStyles.Top | AnchorStyles.Left
			};
			_search.TextChanged += (_, _) => ApplyFilter();
			Controls.Add(_search);
			_count = new Label
			{
				Location = new Point(568, 113),
				Size = new Size(330, 28),
				ForeColor = SettingsPalette.SecondaryText
			};
			Controls.Add(_count);

			_grid = new DataGridView
			{
				Location = new Point(28, 164),
				Size = new Size(1124, 476),
				Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
				ReadOnly = true,
				AllowUserToAddRows = false,
				AllowUserToDeleteRows = false,
				AllowUserToResizeRows = false,
				AutoGenerateColumns = false,
				AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				MultiSelect = false,
				RowHeadersVisible = false,
				ScrollBars = ScrollBars.Both,
				AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
				RowTemplate = { Height = 42 }
			};
			AddColumn("Game", "GAME", 250, true);
			AddColumn("Compatibility", "COMPATIBILITY", 190);
			AddColumn("Configuration", "CONFIGURATION", 210);
			AddColumn("PlayerData", "PLAYER DETAILS", 155);
			AddColumn("Crossplay", "CROSSPLAY", 110);
			AddColumn("Executable", "SERVER PROGRAM", 240);
			AddColumn("LastVerified", "LAST VERIFIED", 130);
			GridStyler.DarkTheme(_grid);
			GridStyler.ApplyDashboardTheme(_grid);
			_grid.AllowUserToResizeColumns = true;
			_grid.CellDoubleClick += GridCellDoubleClick;
			_grid.CellMouseEnter += GridCellMouseEnter;
			_grid.SelectionChanged += (_, _) => UpdateDetailsButton();
			_grid.KeyDown += GridKeyDown;
			Controls.Add(_grid);

			_viewDetails = new ModernSettingsButton
			{
				Text = "View Details",
				Location = new Point(836, 656),
				Size = new Size(156, 44),
				UseAccentStyle = true,
				Anchor = AnchorStyles.Bottom | AnchorStyles.Right
			};
			_viewDetails.Click += (_, _) => OpenSelectedGameDetails();
			Controls.Add(_viewDetails);

			ModernSettingsButton close = new()
			{
				Text = "Close",
				Location = new Point(1002, 656),
				Size = new Size(150, 44),
				DialogResult = DialogResult.OK,
				Anchor = AnchorStyles.Bottom | AnchorStyles.Right
			};
			Controls.Add(close);
			AcceptButton = _viewDetails;
			CancelButton = close;
			ApplyFilter();
			ThemeManager.Apply(this);
		}

		private void AddColumn(
			string property,
			string heading,
			int width,
			bool frozen = false)
		{
			_grid.Columns.Add(new DataGridViewTextBoxColumn
			{
				DataPropertyName = property,
				HeaderText = heading,
				Name = property,
				Width = width,
				MinimumWidth = Math.Min(width, 120),
				AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
				Frozen = frozen
			});
		}

		private void ApplyFilter()
		{
			string search = _search.Text.Trim();
			GameSupportRow[] visible = _allRows
				.Where(row => string.IsNullOrWhiteSpace(search) || row.SearchText.Contains(
					search,
					StringComparison.OrdinalIgnoreCase))
				.ToArray();
			_grid.DataSource = visible;
			_count.Text = $"{visible.Length} of {_allRows.Count} games  •  Double-click a row for details";
			UpdateDetailsButton();
		}

		private void GridCellDoubleClick(object? sender, DataGridViewCellEventArgs eventArgs)
		{
			if (eventArgs.RowIndex >= 0)
				OpenSelectedGameDetails();
		}

		private void GridCellMouseEnter(object? sender, DataGridViewCellEventArgs eventArgs)
		{
			if (eventArgs.RowIndex < 0 || eventArgs.ColumnIndex < 0)
				return;

			DataGridViewCell cell = _grid.Rows[eventArgs.RowIndex].Cells[eventArgs.ColumnIndex];
			string value = Convert.ToString(cell.FormattedValue)?.Trim() ?? string.Empty;
			cell.ToolTipText = string.IsNullOrWhiteSpace(value)
				? "Double-click to view game support details."
				: $"{value}{Environment.NewLine}{Environment.NewLine}Double-click to view game support details.";
		}

		private void GridKeyDown(object? sender, KeyEventArgs eventArgs)
		{
			if (eventArgs.KeyCode != Keys.Enter)
				return;

			eventArgs.Handled = true;
			eventArgs.SuppressKeyPress = true;
			OpenSelectedGameDetails();
		}

		private void OpenSelectedGameDetails()
		{
			if (_grid.CurrentRow?.DataBoundItem is not GameSupportRow row)
				return;

			GameInfo? game = GameDatabase.GetGame(row.Game);
			if (game == null)
				return;

			using GameSupportDetailsDialog dialog = new(game);
			dialog.ShowDialog(this);
		}

		private void UpdateDetailsButton()
		{
			if (_viewDetails != null)
				_viewDetails.Enabled = _grid.CurrentRow?.DataBoundItem is GameSupportRow;
		}

		private static GameSupportRow CreateRow(GameInfo game)
		{
			GameCompatibilitySummary compatibility = Core.GetGameCompatibilitySummary(game.Game);
			ConfigurationSupportPresentation configuration = UserGuidance.GetConfigurationSupport(game);
			GameManagementCapability capabilities = GameFix.GetManagementCapabilities(game);
			bool crossplay = capabilities.HasFlag(GameManagementCapability.Crossplay);
			string playerData = GameDatabase.GetProbeProtocol(game) == ServerProbeProtocol.A2S
				? "Named players"
				: GameDatabase.IsMinecraft(game.Game)
					? "Player count"
					: "Not available";
			string lastVerified = compatibility.Verification.LastTested?.VerifiedAtUtc
				.ToLocalTime().ToString("yyyy-MM-dd") ?? "Not verified";
			return new(
				game.Game,
				compatibility.DisplayName,
				configuration.Status,
				playerData,
				crossplay ? "Available" : "Not listed",
				game.ExeName,
				lastVerified);
		}

		private sealed record GameSupportRow(
			string Game,
			string Compatibility,
			string Configuration,
			string PlayerData,
			string Crossplay,
			string Executable,
			string LastVerified)
		{
			internal string SearchText => string.Join(
				' ',
				Game,
				Compatibility,
				Configuration,
				PlayerData,
				Crossplay,
				Executable,
				LastVerified);
		}
	}
}
