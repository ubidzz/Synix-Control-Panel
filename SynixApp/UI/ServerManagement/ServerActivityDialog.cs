using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.Design.Controls;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine;

namespace Synix_Control_Panel.SynixApp.UI.ServerManagement;

internal sealed class ServerActivityDialog : Form
{
	private readonly GameServer _server;
	private readonly ServerActivityHistory _history;
	private readonly TextBox _search = new() { Name = "historySearch", Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, TabIndex = 0 };
	private readonly CheckBox _attention = new() { Name = "historyAttention", AutoSize = true, TabIndex = 1, Margin = new Padding(0, 10, 0, 10) };
	private readonly DataGridView _grid = new()
	{
		Name = "historyGrid", Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
		AllowUserToDeleteRows = false, RowHeadersVisible = false, AutoGenerateColumns = false,
		AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, MultiSelect = false,
		SelectionMode = DataGridViewSelectionMode.FullRowSelect, TabIndex = 2,
		AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
	};
	private readonly Label _notice = new() { Name = "historyNotice", AutoSize = true, Dock = DockStyle.Fill, UseMnemonic = false, Margin = new Padding(0, 12, 0, 8) };
	private readonly Label _emptyState = new() { Name = "historyEmptyState", TextAlign = ContentAlignment.MiddleCenter, UseMnemonic = false };
	private readonly Label _searchLabel = new() { Name = "historySearchLabel", AutoSize = true, Dock = DockStyle.Fill, UseMnemonic = false, Margin = new Padding(0, 0, 0, 4) };
	private readonly Label _detailsLabel = new() { Name = "historyDetailsLabel", AutoSize = true, Dock = DockStyle.Fill, UseMnemonic = false };
	private readonly TextBox _details = new() { Name = "historyDetails", ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, TabIndex = 3 };
	private readonly ModernSettingsButton _refresh = new() { AutoSize = true, TabIndex = 4 };
	private readonly ModernSettingsButton _close = new() { AutoSize = true, DialogResult = DialogResult.Cancel, TabIndex = 5 };
	private IReadOnlyList<ServerActivityEntry> _entries = [];
	private bool _loading;
	private bool _reloadRequested;
	private bool _unavailable;

	internal ServerActivityDialog(GameServer server, ServerActivityHistory? history = null)
	{
		_server = server;
		_history = history ?? ServerActivityHistory.Current;
		Name = nameof(ServerActivityDialog);
		AutoScaleMode = AutoScaleMode.Dpi;
		Font = new Font("Segoe UI", 10);
		ClientSize = new Size(860, 600);
		MinimumSize = new Size(560, 560);
		StartPosition = FormStartPosition.CenterParent;
		ShowInTaskbar = false;
		BackColor = SettingsPalette.Window;
		ForeColor = SettingsPalette.PrimaryText;
		// Keep the window header edge-to-edge; only the content needs an inset.
		Padding = Padding.Empty;
		TableLayoutPanel layout = new() { Name = "historyLayout", Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7, Padding = new Padding(16), BackColor = SettingsPalette.Window };
		layout.ColumnStyles.Add(new(SizeType.Percent, 100));
		for (int row = 0; row < 7; row++) layout.RowStyles.Add(new(row is 3 or 4 ? SizeType.Percent : SizeType.AutoSize, row == 3 ? 55 : row == 4 ? 45 : 0));
		Label identity = new() { Name = "historyIdentity", AutoSize = true, Dock = DockStyle.Fill, UseMnemonic = false, Margin = new Padding(0, 0, 0, 12), ForeColor = SettingsPalette.PrimaryText };
		LocalizationManager.SetDataText(identity, server.ServerName + " · " + server.DisplayGameName);
		layout.Controls.Add(identity, 0, 0);
		TableLayoutPanel searchSurface = new() { Name = "historySearchSurface", AutoSize = true, Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(10, 9, 10, 9), Margin = Padding.Empty, BackColor = SettingsPalette.Input };
		searchSurface.ColumnStyles.Add(new(SizeType.Percent, 100));
		searchSurface.RowStyles.Add(new(SizeType.AutoSize));
		searchSurface.RowStyles.Add(new(SizeType.AutoSize));
		_searchLabel.ForeColor = SettingsPalette.SecondaryText;
		_searchLabel.Font = new Font("Segoe UI", 9);
		_search.BackColor = SettingsPalette.Input;
		_search.ForeColor = SettingsPalette.PrimaryText;
		_search.Margin = Padding.Empty;
		searchSurface.Controls.Add(_searchLabel, 0, 0);
		searchSurface.Controls.Add(_search, 0, 1);
		layout.Controls.Add(searchSurface, 0, 1);
		_attention.ForeColor = SettingsPalette.PrimaryText;
		layout.Controls.Add(_attention, 0, 2);
		_grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "When", DataPropertyName = "When", MinimumWidth = 170, FillWeight = 35, SortMode = DataGridViewColumnSortMode.NotSortable });
		_grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Activity", DataPropertyName = "Activity", MinimumWidth = 220, FillWeight = 65, SortMode = DataGridViewColumnSortMode.NotSortable });
		_grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
		layout.Controls.Add(_grid, 0, 3);
		TableLayoutPanel detailSurface = new() { Name = "historyDetailsSurface", Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(10), Margin = new Padding(0, 8, 0, 0), BackColor = SettingsPalette.Input };
		detailSurface.ColumnStyles.Add(new(SizeType.Percent, 100));
		detailSurface.RowStyles.Add(new(SizeType.AutoSize));
		detailSurface.RowStyles.Add(new(SizeType.Percent, 100));
		_detailsLabel.ForeColor = SettingsPalette.SecondaryText;
		_details.ForeColor = SettingsPalette.PrimaryText;
		_details.BackColor = SettingsPalette.Input;
		detailSurface.Controls.Add(_detailsLabel, 0, 0);
		detailSurface.Controls.Add(_details, 0, 1);
		layout.Controls.Add(detailSurface, 0, 4);
		_notice.ForeColor = SettingsPalette.SecondaryText;
		layout.Controls.Add(_notice, 0, 5);
		FlowLayoutPanel actions = new() { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = true, Margin = Padding.Empty, BackColor = SettingsPalette.Window };
		actions.Controls.AddRange([_close, _refresh]);
		layout.Controls.Add(actions, 0, 6);
		Controls.Add(layout);
		layout.SizeChanged += (_, _) =>
		{
			int width = Math.Max(1, layout.DisplayRectangle.Width - 8);
			_notice.MaximumSize = new Size(width, 0);
			identity.MaximumSize = new Size(width, 0);
			_attention.MaximumSize = new Size(width, 0);
		};
		CancelButton = _close;
		GridStyler.DarkTheme(_grid);
		GridStyler.ApplyDashboardTheme(_grid);
		_grid.Margin = Padding.Empty;
		_grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
		_grid.DefaultCellStyle.Padding = new Padding(10, 8, 10, 8);
		_grid.RowTemplate.MinimumHeight = 40;
		_grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
		_grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 8, 10, 8);
		_grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		_grid.AllowUserToResizeColumns = true;
		_emptyState.BackColor = SettingsPalette.Input;
		_emptyState.ForeColor = SettingsPalette.SecondaryText;
		_grid.Controls.Add(_emptyState);
		_grid.SizeChanged += (_, _) => ArrangeEmptyState();
		_grid.ColumnHeadersHeightChanged += (_, _) => ArrangeEmptyState();
		_grid.SelectionChanged += (_, _) => ShowEntryDetails();
		ThemeManager.Apply(this);
		_search.TextChanged += (_, _) => ApplyFilter();
		_attention.CheckedChanged += (_, _) => ApplyFilter();
		_refresh.Click += async (_, _) => await ReloadAsync();
		Shown += async (_, _) => { await ReloadAsync(); _search.Focus(); };
		LocalizationManager.LanguageChanged += LanguageChanged;
		_history.Changed += HistoryChanged;
		Disposed += (_, _) => { LocalizationManager.LanguageChanged -= LanguageChanged; _history.Changed -= HistoryChanged; };
		LanguageChanged(null, EventArgs.Empty);
	}

	private void LanguageChanged(object? sender, EventArgs args)
	{
		LocalizationManager.BindText(this, "History.Title");
		_search.PlaceholderText = LocalizationManager.Get("History.Search");
		LocalizationManager.BindAccessibleName(_search, "History.Search");
		LocalizationManager.BindText(_searchLabel, "History.Search");
		LocalizationManager.BindText(_attention, "History.Attention");
		LocalizationManager.BindText(_detailsLabel, "History.Details");
		LocalizationManager.BindAccessibleName(_details, "History.Details");
		LocalizationManager.BindText(_refresh, "ModManager.Button.Refresh");
		LocalizationManager.BindText(_close, "ModManager.Button.Close");
		_grid.Columns[0].HeaderText = LocalizationManager.Get("History.When");
		_grid.Columns[1].HeaderText = LocalizationManager.Get("History.Activity");
		if (!_loading) ApplyFilter();
	}
	private void HistoryChanged(string key)
	{
		if (key != ServerActivityHistory.ServerKey(_server) || IsDisposed || !IsHandleCreated) return;
		try { BeginInvoke(new Action(async () => await ReloadAsync())); } catch (InvalidOperationException) { }
	}
	internal async Task ReloadAsync()
	{
		if (IsDisposed || Disposing) return;
		if (_loading) { _reloadRequested = true; return; }
		_loading = true;
		_refresh.Enabled = false;
		try
		{
			do
			{
				_reloadRequested = false;
				ServerActivityReadResult result = await Task.Run(() => _history.Read(_server));
				if (IsDisposed || Disposing) return;
				_entries = result.Entries;
				_unavailable = result.Unavailable;
				LocalizationManager.BindText(_notice, result.Unavailable ? "History.Unavailable" : result.Recovered ? "History.Recovered" : "History.Retention", ServerActivityHistory.MaximumEntries);
				ApplyFilter();
			} while (_reloadRequested);
		}
		finally { _loading = false; if (!IsDisposed) _refresh.Enabled = true; }
	}
	private void ApplyFilter()
	{
		HistoryRow? selected = _grid.CurrentRow?.DataBoundItem as HistoryRow;
		string query = _search.Text.Trim();
		System.Globalization.CompareInfo comparison = System.Globalization.CultureInfo.CurrentUICulture.CompareInfo;
		bool Matches(string value) => comparison.IndexOf(value, query,
			System.Globalization.CompareOptions.IgnoreCase | System.Globalization.CompareOptions.IgnoreNonSpace) >= 0;
		var visibleEntries = _entries.OrderByDescending(entry => entry.OccurredUtc)
			.Where(entry => (!_attention.Checked || entry.NeedsAttention) &&
				(query.Length == 0 || Matches(entry.Description) ||
				 Matches(entry.OccurredUtc.ToLocalTime().ToString("G", System.Globalization.CultureInfo.CurrentUICulture))))
			.Select(entry => new HistoryRow(entry.OccurredUtc.ToLocalTime().ToString("G", System.Globalization.CultureInfo.CurrentUICulture), entry.Description)).ToList();
		_grid.DataSource = visibleEntries;
		if (selected != null && visibleEntries.IndexOf(selected) is int index && index >= 0)
			_grid.CurrentCell = _grid.Rows[index].Cells["Activity"];
		ShowEntryDetails();
		LocalizationManager.BindText(_emptyState, _unavailable ? "History.Unavailable" : _entries.Count == 0 ? "History.Empty" : "History.NoMatches");
		_emptyState.Visible = visibleEntries.Count == 0;
		ArrangeEmptyState();
	}

	private void ShowEntryDetails() => LocalizationManager.SetDataText(_details,
		(_grid.CurrentRow?.DataBoundItem as HistoryRow)?.Description ?? string.Empty);

	private sealed record HistoryRow(string When, string Description)
	{
		public string Activity => Description.Length <= 180 ? Description : Description[..180] + "…";
	}

	private void ArrangeEmptyState()
	{
		int inset = Math.Max(16, 16 * _grid.DeviceDpi / 96);
		int width = Math.Max(1, _grid.ClientSize.Width - inset * 2);
		int top = _grid.ColumnHeadersHeight + inset;
		int availableHeight = Math.Max(0, _grid.ClientSize.Height - top - inset);
		int height = Math.Min(availableHeight, _emptyState.GetPreferredSize(new Size(width, 0)).Height);
		_emptyState.SetBounds(inset, top + (availableHeight - height) / 2, width, height);
	}
}
