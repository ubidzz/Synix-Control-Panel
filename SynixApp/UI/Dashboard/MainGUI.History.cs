using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.Design.Controls;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixApp.UI.ServerManagement;
using Synix_Control_Panel.SynixEngine;

namespace Synix_Control_Panel.SynixApp.UI.Dashboard;

public partial class MainGUI
{
	private readonly Dictionary<string, (ServerBackupHealth Health, DateTimeOffset Checked)> _backupHealth = new();
	private readonly Dictionary<string, Task<ServerBackupHealth>> _pendingBackupHealth = new();
	private System.Windows.Forms.Timer? _backupHealthTimer;
	private ModernSettingsButton? _historyButton;
	private ToolStripMenuItem? _historyMenuItem;
	private bool _readingBackupHealth;

	internal void InitializeActivityHistory(bool monitorBackups = true)
	{
		_historyButton = new ModernSettingsButton { Name = "btnServerHistory", AutoSize = true, TabIndex = 0 };
		LocalizationManager.BindText(_historyButton, "History.Title");
		_historyButton.Click += (_, _) => OpenServerHistory();
		_historyMenuItem = new ToolStripMenuItem(LocalizationManager.Get("History.Title"));
		_historyMenuItem.Click += (_, _) => OpenServerHistory();
		contextMenuStrip.Items.Add(_historyMenuItem);

		// Translated headings and actions get their own wrapping rows, not overlapping fixed coordinates.
		activityCard.Controls.Clear();
		activityCard.Padding = new Padding(16);
		TableLayoutPanel layout = new() { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Margin = Padding.Empty };
		layout.ColumnStyles.Add(new(SizeType.Percent, 100));
		layout.RowStyles.Add(new(SizeType.AutoSize));
		layout.RowStyles.Add(new(SizeType.AutoSize));
		layout.RowStyles.Add(new(SizeType.Percent, 100));
		lblActivityTitle.Dock = DockStyle.Fill;
		lblActivityTitle.Margin = new Padding(0, 0, 0, 6);
		FlowLayoutPanel actions = new() { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Margin = Padding.Empty };
		btnClearLog.AutoSize = true;
		btnClearLog.AutoSizeMode = AutoSizeMode.GrowAndShrink;
		btnClearLog.Padding = new Padding(8);
		btnClearLog.TabIndex = 1;
		actions.Controls.AddRange([_historyButton, btnClearLog]);
		rtbLog.Dock = DockStyle.Fill;
		rtbLog.Margin = new Padding(0, 6, 0, 0);
		layout.Controls.Add(lblActivityTitle, 0, 0);
		layout.Controls.Add(actions, 0, 1);
		layout.Controls.Add(rtbLog, 0, 2);
		layout.SizeChanged += (_, _) => lblActivityTitle.MaximumSize = new Size(Math.Max(1, layout.ClientSize.Width), 0);
		activityCard.Controls.Add(layout);
		ThemeManager.Apply(activityCard);
		LocalizationManager.BindText(btnClearLog, "Text.9CC3A043B6A91D450BC3");

		dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
		{
			Name = "BackupHealth", HeaderText = LocalizationManager.Get("SettingsPage.Backups.Heading"),
			MinimumWidth = 135, FillWeight = 20, ReadOnly = true, SortMode = DataGridViewColumnSortMode.NotSortable,
			DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True }
		});
		dataGridView1.CellFormatting += FormatBackupHealth;
		dataGridView1.CellToolTipTextNeeded += BackupHealthToolTip;
		ConfigureServerGridLayout();
		dataGridView1.DpiChangedAfterParent += (_, _) => ConfigureServerGridLayout();
		ServerActivityHistory.Current.BackupChanged += ServerHistoryChanged;
		Disposed += (_, _) =>
		{
			ServerActivityHistory.Current.BackupChanged -= ServerHistoryChanged;
			_backupHealthTimer?.Dispose();
		};
		if (!monitorBackups) return;
		_backupHealthTimer = new System.Windows.Forms.Timer { Interval = 30_000 };
		_backupHealthTimer.Tick += async (_, _) => await RefreshBackupHealthAsync();
		_backupHealthTimer.Start();
		Shown += async (_, _) => await RefreshBackupHealthAsync();
	}

	private void OpenServerHistory()
	{
		if (GetSelectedServer() is not { } server) return;
		using ServerActivityDialog dialog = new(server);
		dialog.ShowDialog(this);
	}

	private void ServerHistoryChanged(string key)
	{
		if (IsDisposed || !IsHandleCreated) return;
		try
		{
			BeginInvoke(new Action(async () =>
			{
				_backupHealth.Remove(key);
				await RefreshBackupHealthAsync();
			}));
		}
		catch (InvalidOperationException) { }
	}

	private async Task RefreshBackupHealthAsync()
	{
		if (_readingBackupHealth || IsDisposed || !IsHandleCreated) return;
		_readingBackupHealth = true;
		try
		{
			DateTimeOffset now = DateTimeOffset.UtcNow;
			GameServer[] servers = _visibleServers.Where(server =>
				!_backupHealth.TryGetValue(ServerActivityHistory.ServerKey(server), out var cached) ||
				now - cached.Checked >= TimeSpan.FromSeconds(30)).ToArray();
			foreach (GameServer server in servers)
			{
				string key = ServerActivityHistory.ServerKey(server);
				// One outstanding scan per server, even if a disconnected network drive stalls.
				if (!_pendingBackupHealth.TryGetValue(key, out Task<ServerBackupHealth>? pending))
				{
					pending = Task.Run(() =>
					{
						try { return Core.Instance.GetServerBackupHealth(server); }
						catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException)
						{ return new ServerBackupHealth(Unavailable: true); }
					});
					_pendingBackupHealth[key] = pending;
				}
				ServerBackupHealth health;
				try { health = await pending.WaitAsync(TimeSpan.FromSeconds(5)); _pendingBackupHealth.Remove(key); }
				catch (TimeoutException) { health = new(Unavailable: true); }
				if (IsDisposed) return;
				_backupHealth[key] = (health, DateTimeOffset.UtcNow);
			}
			ResizeServerGridRows();
			dataGridView1.InvalidateColumn(dataGridView1.Columns["BackupHealth"]!.Index);
		}
		catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
		{ ApplicationLogService.WriteSuppressedException(error); }
		finally { _readingBackupHealth = false; }
	}

	private void FormatBackupHealth(object? sender, DataGridViewCellFormattingEventArgs args)
	{
		if (args.RowIndex < 0 || args.ColumnIndex < 0 || dataGridView1.Columns[args.ColumnIndex].Name != "BackupHealth" ||
			dataGridView1.Rows[args.RowIndex].DataBoundItem is not GameServer server) return;
		if (_backupHealth.TryGetValue(ServerActivityHistory.ServerKey(server), out var cached))
		{
			args.Value = cached.Health.Summary(DateTimeOffset.UtcNow);
			args.CellStyle.ForeColor = cached.Health.NeedsAttention(DateTimeOffset.UtcNow) ? SettingsPalette.Warning : SettingsPalette.SecondaryText;
			args.CellStyle.SelectionForeColor = args.CellStyle.ForeColor;
		}
		else args.Value = LocalizationManager.Get("BackupHealth.Checking");
		args.FormattingApplied = true;
	}

	private void BackupHealthToolTip(object? sender, DataGridViewCellToolTipTextNeededEventArgs args)
	{
		if (args.RowIndex < 0 || args.ColumnIndex < 0 || dataGridView1.Columns[args.ColumnIndex].Name != "BackupHealth" ||
			dataGridView1.Rows[args.RowIndex].DataBoundItem is not GameServer server) return;
		if (_backupHealth.TryGetValue(ServerActivityHistory.ServerKey(server), out var cached))
			args.ToolTipText = cached.Health.Details(DateTimeOffset.UtcNow);
	}
}
