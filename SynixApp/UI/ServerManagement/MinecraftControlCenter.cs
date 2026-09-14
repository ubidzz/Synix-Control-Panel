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
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine.Minecraft;
using Synix_Control_Panel.SynixEngine.ModManagement;
using System.Text;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel.SynixApp.UI.ServerManagement;

internal sealed class MinecraftControlCenter : Form
{
	private readonly GameServer _initialServer;
	private readonly TabControl _tabs;
	private readonly FlowLayoutPanel _navigation;
	private readonly Label _summary;
	private readonly Label _status;
	private readonly Label _overview;
	private readonly ModernSettingsButton _cancel;
	private readonly Dictionary<string, DataGridView> _grids = [];
	private readonly List<(string Key, ModernSettingsButton Button)> _actions = [];
	private readonly ToolTip _tips = new();
	private readonly System.Windows.Forms.Timer _timer = new() { Interval = 2000 };
	private TextBox _worldName = null!;
	private TextBox _playerName = null!;
	private ModernSettingsComboBox _playerAction = null!;
	private CheckBox _optionalMods = null!;
	private MinecraftConsoleDialog? _console;
	private GameServer? _consoleServer;
	private Label _schedule = null!;
	private bool _busy;
	private bool _disposing;
	private CancellationTokenSource? _cancellation;
	private bool Unavailable => _disposing || Disposing || IsDisposed;
	private GameServer Server => ServerRegistry.Servers.FirstOrDefault(server =>
		server.InstallPath.Equals(_initialServer.InstallPath, StringComparison.OrdinalIgnoreCase)) ?? _initialServer;
	internal static string TextFor(string key, params object[] args) => LocalizationManager.Get("MinecraftWorkspace." + key, args);

	internal MinecraftControlCenter(GameServer server)
	{
		if (!GameDatabase.IsMinecraft(server.Game)) throw new ArgumentException(nameof(server));
		_initialServer = server;
		Text = TextFor("Title");
		StartPosition = FormStartPosition.CenterParent;
		ShowInTaskbar = false;
		ClientSize = new Size(1200, 830);
		MinimumSize = new Size(1020, 760);
		Font = new Font("Segoe UI", 10);
		BackColor = SettingsPalette.Window;
		ForeColor = SettingsPalette.PrimaryText;

		TableLayoutPanel layout = new() { Dock = DockStyle.Fill, Padding = new Padding(22), ColumnCount = 1, RowCount = 3 };
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
		layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
		_summary = new Label { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoEllipsis = true };
		layout.Controls.Add(_summary, 0, 0);
		TableLayoutPanel body = new() { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
		body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 192));
		body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		_navigation = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
			WrapContents = false, AutoScroll = true, Padding = new Padding(0, 4, 8, 0), Margin = Padding.Empty };
		_tabs = new WorkspacePages { Name = "minecraftWorkspaceTabs", Dock = DockStyle.Fill, Margin = Padding.Empty, Multiline = true };
		body.Controls.Add(_navigation, 0, 0); body.Controls.Add(_tabs, 1, 0);
		layout.Controls.Add(body, 0, 1);
		TableLayoutPanel footer = new() { Dock = DockStyle.Fill, ColumnCount = 3 };
		footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
		footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
		_status = new Label { Name = "minecraftWorkspaceStatus", Dock = DockStyle.Fill, Text = TextFor("Ready"),
			ForeColor = SettingsPalette.SecondaryText, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft };
		_cancel = Button("CancelOperation", () => { _cancellation?.Cancel(); return Task.CompletedTask; }, wrap: false);
		_cancel.Visible = false;
		footer.Controls.Add(_status, 0, 0);
		footer.Controls.Add(_cancel, 1, 0);
		footer.Controls.Add(Button("Refresh", async () =>
		{
			if (Server.Status == StatusManager.GetStatus(ServerState.Stopped))
				MinecraftConfigurationSync.Synchronize(Server, FileHandler.SaveServers);
			await RefreshPageAsync();
		}), 2, 0);
		layout.Controls.Add(footer, 0, 2);
		Controls.Add(layout);

		(TabPage overviewPage, TableLayoutPanel overviewLayout, FlowLayoutPanel overviewActions) = Page("Overview", "OverviewHelp");
		_overview = new Label { Dock = DockStyle.Fill, Padding = new Padding(16), Font = new Font("Segoe UI", 12),
			ForeColor = SettingsPalette.PrimaryText, AutoEllipsis = true };
		overviewLayout.Controls.Add(_overview, 0, 1);
		overviewActions.Controls.AddRange([
			Button("Start", async () =>
			{
				if (Server.Status == StatusManager.GetStatus(ServerState.Crashed) && !await Core.Instance.StopServerAndReport(Server))
					throw MinecraftContentTransactions.Error("StartFailed");
				if (!await Core.Instance.ExecuteStartSequence(Server)) throw MinecraftContentTransactions.Error("StartFailed");
			}),
			Button("Stop", async () => { if (Confirm("ConfirmStop")) await Core.Instance.StopServerAndReport(Server); }),
			Button("Restart", async () => { if (Confirm("ConfirmRestart")) await Core.Instance.ExecuteStartSequence(Server, "RESTART"); }),
			Button("Setup", async () => { await Core.Instance.EditServerAndReport(Server); _console?.Dispose(); _console = null; }),
			Button("ApplyRuntime", async () =>
			{
				if (Confirm("RuntimeNotice")) await MinecraftRuntimeUpdater.ApplyAsync(Server, FileHandler.SaveServers, Progress());
			}),
			Button("Readiness", () => { using TroubleshooterDialog dialog = new(Server); dialog.ShowDialog(this); return Task.CompletedTask; }),
			Button("Connection", () => { using ConnectionInformationDialog dialog = new(Server); dialog.ShowDialog(this); return Task.CompletedTask; })
		]);

		TabPage consolePage = new(TextFor("Console")) { Name = "Console", BackColor = SettingsPalette.Window };
		_tabs.TabPages.Add(consolePage);
		(_, TableLayoutPanel playersLayout, FlowLayoutPanel playersActions) = Page("Players", "PlayersHelp");
		playersLayout.Controls.Add(Grid("Players", "Player", "Source"), 0, 1);
		_playerName = new TextBox { Name = "minecraftPlayerName", PlaceholderText = TextFor("Player"), AccessibleName = TextFor("Player") };
		_playerAction = new ModernSettingsComboBox { Name = "minecraftPlayerAction", Width = 200,
			Margin = new Padding(4, 8, 4, 4), AccessibleName = TextFor("ApplyPlayer") };
		foreach (string action in new[] { "Kick", "Allow", "Disallow", "Operator", "Deop", "Ban", "Pardon" })
			if (!MinecraftControlProfile.IsBedrock(server) || action is not ("Ban" or "Pardon"))
				_playerAction.Items.Add(new PlayerActionChoice(action));
		_playerAction.SelectedIndex = 0;
		playersActions.Controls.AddRange([InputCard(_playerName, 210), _playerAction, Button("ApplyPlayer", ApplyPlayerAsync)]);
		_grids["Players"].SelectionChanged += (_, _) =>
		{
			if (_grids["Players"].SelectedRows.Count == 1) _playerName.Text = Convert.ToString(_grids["Players"].SelectedRows[0].Cells[0].Value) ?? "";
		};

		(_, TableLayoutPanel settingsLayout, FlowLayoutPanel settingsActions) = Page("Settings", "SettingsHelp");
		settingsLayout.Controls.Add(Grid("Settings", "File", "Format"), 0, 1);
		settingsActions.Controls.AddRange([
			Button("EditFile", () =>
			{
				if (Selected<string>("Settings") is string file)
				{ using MinecraftTextEditor editor = new(Server, file); editor.ShowDialog(this); }
				return Task.CompletedTask;
			}),
			Button("StructuredEditor", () => { Core.Instance.OpenConfigEditor(Server); return Task.CompletedTask; }),
			Button("Setup", async () => await Core.Instance.EditServerAndReport(Server))
		]);

		(_, TableLayoutPanel worldsLayout, FlowLayoutPanel worldsActions) = Page("Worlds", "WorldsHelp");
		worldsLayout.Controls.Add(Grid("Worlds", "World", "Status", "File"), 0, 1);
		_worldName = new TextBox { Name = "minecraftImportWorldName", PlaceholderText = TextFor("NewWorldName"), AccessibleName = TextFor("NewWorldName") };
		worldsActions.Controls.AddRange([InputCard(_worldName, 230), Button("ImportWorld", ImportWorldAsync),
			Button("ActivateWorld", () =>
			{
				if (Selected<MinecraftWorld>("Worlds") is MinecraftWorld world && Confirm("ConfirmWorld"))
					MinecraftWorlds.Switch(Server, world.Name, FileHandler.SaveServers);
				return Task.CompletedTask;
			})]);

		(_, TableLayoutPanel modsLayout, FlowLayoutPanel modsActions) = Page("Mods", "ModsHelp");
		_optionalMods = new CheckBox { AutoSize = true, Text = TextFor("IncludeOptional"), Margin = new Padding(8, 15, 8, 4) };
		modsLayout.Controls.Add(Grid("Mods", "File", "AddOnId", "Version", "Loader", "Status"), 0, 1);
		modsActions.Controls.AddRange([
			Button("ManageAddOns", () =>
			{
				if (!ModSystemCatalog.CanManageAddOns(Server)) throw MinecraftContentTransactions.Error("JavaOnly");
				using ModPluginManager dialog = new(Server); dialog.ShowDialog(this); return Task.CompletedTask;
			}),
			Button("ImportPack", ImportPackAsync, cancellable: true),
			Button("Checks", ShowCompatibilityAsync), _optionalMods
		]);

		(_, TableLayoutPanel backupsLayout, FlowLayoutPanel backupActions) = Page("Backups", "BackupsHelp");
		backupsLayout.Controls.Add(Grid("Backups", "File", "Created", "Size", "Status"), 0, 1);
		backupActions.Controls.AddRange([Button("CreateBackup", CreateBackupAsync), Button("RestoreBackup", RestoreBackupAsync),
			Button("VerifyBackup", async () =>
			{
				if (Selected<ServerBackupArchive>("Backups") is ServerBackupArchive backup)
				{
					ServerBackupManagementResult result = await Core.Instance.VerifyServerBackupAsync(Server, backup);
					LocalizedMessageBox.Show(this, result.Message, Text, MessageBoxButtons.OK,
						result.Succeeded ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
				}
			})]);

		(_, TableLayoutPanel scheduleLayout, FlowLayoutPanel scheduleActions) = Page("Schedules", "ScheduleHelp");
		_schedule = new Label { Dock = DockStyle.Fill, Padding = new Padding(16), Font = new Font("Segoe UI", 12), ForeColor = SettingsPalette.PrimaryText };
		scheduleLayout.Controls.Add(_schedule, 0, 1);
		scheduleActions.Controls.Add(Button("EditSchedule", async () => await Core.Instance.EditServerAndReport(Server, openAutomation: true)));

		(_, TableLayoutPanel recoveryLayout, FlowLayoutPanel recoveryActions) = Page("Recovery", "RecoveryHelp");
		recoveryLayout.Controls.Add(Grid("Recovery", "Change", "Created", "Files", "Status"), 0, 1);
		recoveryActions.Controls.Add(Button("UndoChange", UndoChangeAsync));

		(_, TableLayoutPanel diagnosticsLayout, FlowLayoutPanel diagnosticsActions) = Page("Diagnostics", "DiagnosticsHelp");
		diagnosticsLayout.Controls.Add(Grid("Diagnostics", "Area", "Detail"), 0, 1);
		diagnosticsActions.Controls.AddRange([Button("Readiness", () =>
		{
			using TroubleshooterDialog dialog = new(Server); dialog.ShowDialog(this); return Task.CompletedTask;
		}), Button("Checks", ShowCompatibilityAsync), Button("Performance", () =>
		{
			using ResourceMonitorGUI dialog = new(Server); dialog.ShowDialog(this); return Task.CompletedTask;
		})]);

		_tabs.Selecting += (_, e) => { if (_busy) e.Cancel = true; };
		foreach (TabPage page in _tabs.TabPages)
		{
			ModernSettingsButton navigation = new() { Name = "minecraftNav" + page.Name, Text = page.Text,
				Width = 174, Height = 40, Margin = new Padding(0, 0, 0, 8), Tag = page,
				UseAccentStyle = page == _tabs.SelectedTab };
			navigation.Click += (_, _) => { if (!_busy) _tabs.SelectedTab = page; };
			_navigation.Controls.Add(navigation);
			_tips.SetToolTip(navigation, page.Text);
		}
		_tabs.SelectedIndexChanged += async (_, _) =>
		{
			UpdateNavigation();
			await RunAsync(RefreshPageAsync);
		};
		_timer.Tick += (_, _) => { if (!_busy) UpdateSummary(); };
		_playerName.TextChanged += (_, _) => UpdateActionAvailability();
		foreach (DataGridView grid in _grids.Values) grid.SelectionChanged += (_, _) => UpdateActionAvailability();
		FormClosing += (_, e) => { if (_busy) { e.Cancel = true; _status.Text = TextFor("Wait"); } };
		ThemeManager.Apply(this);
		UpdateSummary();
	}

	protected override async void OnShown(EventArgs e)
	{
		base.OnShown(e);
		if (Unavailable) return;
		UpdateNavigation();
		_timer.Start();
		await RunAsync(RefreshPageAsync);
	}

	private void UpdateNavigation()
	{
		if (Unavailable) return;
		foreach (ModernSettingsButton item in _navigation.Controls.OfType<ModernSettingsButton>())
			item.UseAccentStyle = ReferenceEquals(item.Tag, _tabs.SelectedTab);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && !_disposing)
		{
			// Child disposal can raise selection events before Form.IsDisposed is set.
			// An in-flight action owns its token source until its continuation finishes.
			_disposing = true;
			_timer.Dispose();
			_console?.Dispose();
			_tips.Dispose();
		}
		base.Dispose(disposing);
	}

	private (TabPage, TableLayoutPanel, FlowLayoutPanel) Page(string key, string guidance)
	{
		TabPage page = new(TextFor(key)) { Name = key, BackColor = SettingsPalette.Window };
		TableLayoutPanel layout = new() { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 1, RowCount = 3 };
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
		layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		layout.RowStyles.Add(new RowStyle(SizeType.Absolute, key == "Overview" ? 110 : 106));
		layout.Controls.Add(new Label { Text = TextFor(guidance), Dock = DockStyle.Fill, ForeColor = SettingsPalette.SecondaryText }, 0, 0);
		FlowLayoutPanel actions = new() { Dock = DockStyle.Fill, WrapContents = true, AutoScroll = true, Padding = new Padding(0, 6, 0, 0) };
		layout.Controls.Add(actions, 0, 2);
		page.Controls.Add(layout);
		_tabs.TabPages.Add(page);
		return (page, layout, actions);
	}

	private DataGridView Grid(string name, params string[] columns)
	{
		DataGridView grid = new() { Name = "minecraft" + name + "Grid", Dock = DockStyle.Fill,
			ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false,
			MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false,
			AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells };
		foreach (string column in columns) grid.Columns.Add(new DataGridViewTextBoxColumn
			{ Name = column, HeaderText = TextFor(column), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 95 });
		StyleContentGrid(grid);
		grid.Paint += (_, e) =>
		{
			if (grid.Rows.Count == 0)
				TextRenderer.DrawText(e.Graphics, TextFor("Empty"), Font,
					new Rectangle(14, grid.ColumnHeadersHeight + 24, Math.Max(0, grid.ClientSize.Width - 28), 90),
					SettingsPalette.MutedText, TextFormatFlags.WordBreak);
		};
		_grids.Add(name, grid);
		return grid;
	}

	internal static void StyleContentGrid(DataGridView grid)
	{
		GridStyler.DarkTheme(grid);
		GridStyler.ApplyDashboardTheme(grid);
		grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
		grid.RowTemplate.MinimumHeight = 44;
		// Assign the complete style after the shared theme. Set the column styles too,
		// so a generated/default row style cannot turn a detailed report into one line.
		grid.DefaultCellStyle = new DataGridViewCellStyle(grid.DefaultCellStyle)
		{
			WrapMode = DataGridViewTriState.True,
			Alignment = DataGridViewContentAlignment.TopLeft,
			Padding = new Padding(8, 8, 6, 8)
		};
		foreach (DataGridViewColumn column in grid.Columns)
			column.DefaultCellStyle = new DataGridViewCellStyle(column.DefaultCellStyle) { WrapMode = DataGridViewTriState.True };
		if (grid.Columns["Detail"] is DataGridViewColumn detail)
		{
			detail.FillWeight = 260;
			detail.MinimumWidth = 230;
			if (grid.Columns.Count == 2)
			{
				DataGridViewColumn area = grid.Columns[0];
				area.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
				area.Width = 190;
			}
		}
	}

	private static ModernSettingsCard InputCard(TextBox input, int width)
	{
		ModernSettingsCard card = new() { Name = input.Name + "Card", Width = width, Height = 42,
			CornerRadius = 8, Padding = new Padding(12), Margin = new Padding(4),
			BackColor = SettingsPalette.Input, FillColor = SettingsPalette.Input };
		input.BorderStyle = BorderStyle.None;
		input.BackColor = SettingsPalette.Input;
		input.ForeColor = SettingsPalette.PrimaryText;
		input.Font = new Font("Segoe UI", 10);
		input.Margin = Padding.Empty;
		card.Layout += (_, _) => input.SetBounds(card.Padding.Left,
			Math.Max(0, (card.ClientSize.Height - input.PreferredHeight) / 2),
			Math.Max(0, card.ClientSize.Width - card.Padding.Horizontal), input.PreferredHeight);
		input.Enter += (_, _) => { card.BorderColor = SettingsPalette.Accent; card.Invalidate(); };
		input.Leave += (_, _) => { card.BorderColor = SettingsPalette.Border; card.Invalidate(); };
		card.Click += (_, _) => input.Focus();
		card.Controls.Add(input);
		return card;
	}

	private ModernSettingsButton Button(string key, Func<Task> action, bool wrap = true, bool cancellable = false)
	{
		ModernSettingsButton button = new() { Name = "minecraft" + key, Text = TextFor(key), Width = 162, Height = 42, Margin = new Padding(4) };
		_actions.Add((key, button));
		_tips.SetToolTip(button, button.Text);
		button.Click += async (_, _) => { if (wrap) await RunAsync(action, cancellable); else await action(); };
		return button;
	}

	private async Task RunAsync(Func<Task> action, bool cancellable = false)
	{
		if (_busy || Unavailable) return;
		_busy = true;
		using CancellationTokenSource cancellation = new();
		_cancellation = cancellation;
		_cancel.Visible = cancellable;
		_status.Text = TextFor("Working");
		UseWaitCursor = true;
		UpdateActionAvailability();
		try { await action(); if (!Unavailable) { UpdateSummary(); _status.Text = TextFor("Ready"); } }
		catch (OperationCanceledException) { if (!Unavailable) _status.Text = TextFor("Cancelled"); }
		catch (Exception exception)
		{
			if (!Unavailable) { _status.Text = Core.SanitizeProblemReportText(exception.Message);
				LocalizedMessageBox.Show(this, _status.Text, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); }
		}
		finally
		{
			_busy = false;
			_cancellation = null;
			if (!Unavailable)
			{
				_cancel.Visible = false;
				UseWaitCursor = false;
				UpdateActionAvailability();
			}
		}
	}

	private bool Confirm(string key) => LocalizedMessageBox.Show(this, TextFor(key), Text,
		MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;

	private IProgress<string> Progress() => new Progress<string>(message => { if (!Unavailable) _status.Text = SecretRedactor.Redact(message); });

	private void UpdateSummary()
	{
		if (Unavailable) return;
		GameServer server = Server;
		_summary.Text = server.ServerName + "  •  Minecraft " + server.MinecraftEdition + "  •  " + server.GameVersion +
			"  •  " + LocalizationManager.TranslateRuntimeText(server.Status) + Environment.NewLine +
			server.MinecraftLoader + " " + server.MinecraftLoaderVersion;
		_overview.Text = TextFor("OverviewValues", server.MinecraftLoader, server.MinecraftLoaderVersion,
			server.RequiredJavaVersion, server.MaxRam, server.PlayerCount, server.Port, server.QueryPort,
			server.WorldName, server.MinecraftAdvertisedName ?? server.ServerName) +
			Environment.NewLine + Environment.NewLine + TextFor("ScheduleHelp");
		UpdateActionAvailability();
	}

	private void UpdateActionAvailability()
	{
		if (Unavailable) return;
		bool stopped = Server.Status == StatusManager.GetStatus(ServerState.Stopped);
		bool crashed = Server.Status == StatusManager.GetStatus(ServerState.Crashed);
		bool running = Server.Status == StatusManager.GetStatus(ServerState.Running);
		foreach ((string key, ModernSettingsButton button) in _actions)
		{
			button.Enabled = key == "CancelOperation" ? _busy : !_busy && (key switch
			{
				"Start" => stopped || crashed,
				"Setup" or "ApplyRuntime" or "EditSchedule" or "ImportWorld" or "CreateBackup" or "StructuredEditor" => stopped,
				"Stop" => running || crashed,
				"Restart" => running,
				"ImportPack" => stopped && MinecraftControlProfile.IsJava(Server),
				"ManageAddOns" => ModSystemCatalog.CanManageAddOns(Server),
				"EditFile" => stopped && Selected<string>("Settings") != null,
				"ActivateWorld" => stopped && Selected<MinecraftWorld>("Worlds") is { Active: false },
				"UndoChange" => stopped && Selected<MinecraftChangeReceipt>("Recovery") is { State: "Applied" or "Prepared" },
				"RestoreBackup" or "VerifyBackup" => stopped && Selected<ServerBackupArchive>("Backups") != null,
				"ApplyPlayer" => running && !string.IsNullOrWhiteSpace(_playerName.Text),
				_ => true
			});
		}
	}

	private async Task RefreshPageAsync()
	{
		if (Unavailable) return;
		UpdateSummary();
		string page = _tabs.SelectedTab?.Name ?? "Overview";
		GameServer server = Server;
		switch (page)
		{
			case "Schedules":
				_schedule.Text = TextFor("ScheduleValues", server.IsScheduledRestartEnabled ? TextFor("Enabled") : TextFor("Disabled"),
					server.RestartTime, string.Join(", ", server.RestartDays.Select((enabled, day) => enabled ?
						System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName((DayOfWeek)day) : null).Where(day => day != null)),
					server.MaintenanceWaitForPlayers, server.MaintenanceBackupBeforeRestart, server.MaintenanceUpdateBeforeRestart);
				break;
			case "Console":
				if (!ReferenceEquals(server, _consoleServer)) { _console?.Dispose(); _console = null; }
				if (_console == null || _console.IsDisposed)
				{
					_consoleServer = server;
					_console = new MinecraftConsoleDialog(server, embedded: true)
						{ TopLevel = false, FormBorderStyle = FormBorderStyle.None, Dock = DockStyle.Fill };
					_tabs.TabPages["Console"]!.Controls.Add(_console);
					_console.Show();
				}
				break;
			case "Players":
				_grids[page].Rows.Clear();
				if (server.Status == StatusManager.GetStatus(ServerState.Running))
				{
					PlayerQueryResult players = await PlayerQueryService.QueryAsync(server);
					if (Unavailable) return;
					foreach (GamePlayerInfo player in players.Players) AddRow(page, player.Name, player.Name, TextFor("Online"));
				}
				foreach (string list in MinecraftControlProfile.IsBedrock(server) ? new[] { "allowlist.json", "permissions.json" } :
					new[] { "whitelist.json", "ops.json", "banned-players.json" })
				{
					string path = ModPathSafety.Resolve(server.InstallPath, list);
					if (!File.Exists(path)) continue;
					if (new FileInfo(path).Length > 4 * 1024 * 1024) throw MinecraftContentTransactions.Error("Size");
					using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(await File.ReadAllTextAsync(path));
					if (Unavailable) return;
					foreach (System.Text.Json.JsonElement player in document.RootElement.EnumerateArray().Take(10000))
						if (player.TryGetProperty("name", out var name)) AddRow(page, name.GetString(), name.GetString(), list);
				}
				break;
			case "Settings":
				_grids[page].Rows.Clear();
				foreach (string file in await Task.Run(() => MinecraftConfigurationFiles.List(server)))
					AddRow(page, file, file, Path.GetExtension(file).TrimStart('.').ToUpperInvariant());
				break;
			case "Worlds":
				_grids[page].Rows.Clear();
				foreach (MinecraftWorld world in await Task.Run(() => MinecraftWorlds.List(server)))
					AddRow(page, world, world.Name, TextFor(world.Active ? "Active" : "Available"), world.RelativePath);
				break;
			case "Mods":
				_grids[page].Rows.Clear();
				MinecraftCompatibilityReport report = await Task.Run(() => MinecraftCompatibility.Scan(server));
				foreach (MinecraftAddOn addon in report.AddOns.Where(addon => !addon.Bundled))
					AddRow(page, addon, addon.File, addon.Id, addon.Version, addon.Loader,
						string.Join("; ", report.Findings.Where(finding => finding.Area == addon.File || finding.Area == addon.Id).Select(finding => finding.Detail)));
				foreach (MinecraftFinding finding in report.Findings.Where(finding => !report.AddOns.Any(addon => addon.File == finding.Area || addon.Id == finding.Area)))
					AddRow(page, finding, finding.Area, "?", "?", "?", finding.Detail);
				break;
			case "Backups":
				_grids[page].Rows.Clear();
				foreach (ServerBackupArchive backup in await Core.Instance.GetServerBackupsAsync(server))
					AddRow(page, backup, backup.FileName, backup.CreatedLocal, Core.FormatBytes(backup.CompressedBytes), backup.IntegrityText);
				break;
			case "Recovery":
				_grids[page].Rows.Clear();
				foreach (MinecraftChangeReceipt receipt in await Task.Run(() => MinecraftContentTransactions.History(server)))
					AddRow(page, receipt, receipt.Title, receipt.CreatedUtc.ToLocalTime(), receipt.Files.Count, TextFor("State." + receipt.State));
				break;
			case "Diagnostics":
				_grids[page].Rows.Clear();
				string log = await Task.Run(() => ReadRecentLogs(server));
				if (Unavailable) return;
				foreach (MinecraftFinding finding in MinecraftLogDiagnostics.Analyze(log))
					AddRow(page, finding, finding.Area, finding.Detail);
				if (_grids[page].Rows.Count == 0) AddRow(page, null, TextFor("Logs"), TextFor("NoLogFindings"));
				break;
		}
	}

	private void AddRow(string grid, object? tag, params object?[] values)
	{
		if (Unavailable) return;
		int row = _grids[grid].Rows.Add(values.Select(value => value ?? string.Empty).ToArray());
		_grids[grid].Rows[row].Tag = tag;
	}
	private T? Selected<T>(string grid) where T : class => _grids.TryGetValue(grid, out DataGridView? table) && table.SelectedRows.Count == 1 ? table.SelectedRows[0].Tag as T : null;

	private async Task ApplyPlayerAsync()
	{
		GameServer server = Server;
		string name = _playerName.Text.Trim();
		string action = ((PlayerActionChoice)_playerAction.SelectedItem!).Key;
		string command = BuildPlayerCommand(server, name, action);
		if (!Confirm("ConfirmPlayer")) return;
		(bool succeeded, string message) = await Servers.SendMinecraftCommandAsync(server, command);
		LocalizedMessageBox.Show(this, message, Text, MessageBoxButtons.OK, succeeded ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
		await RefreshPageAsync();
	}

	internal static string BuildPlayerCommand(GameServer server, string name, string action)
	{
		bool bedrock = MinecraftControlProfile.IsBedrock(server);
		if (bedrock ? name.Length is < 1 or > 32 || name.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not (' ' or '_' or '-')) :
			!MinecraftRconClient.IsSafePlayerName(name)) throw MinecraftContentTransactions.Error("Player");
		string verb = action switch
		{
			"Kick" => "kick", "Allow" => bedrock ? "allowlist add" : "whitelist add",
			"Disallow" => bedrock ? "allowlist remove" : "whitelist remove", "Operator" => "op", "Deop" => "deop",
			"Ban" when !bedrock => "ban", "Pardon" when !bedrock => "pardon",
			_ => throw MinecraftContentTransactions.Error("Player")
		};
		return verb + " " + (bedrock ? "\"" + name + "\"" : name);
	}

	private async Task ImportWorldAsync()
	{
		ModPackageManager.EnsureStopped(Server);
		MinecraftWorlds.ValidateName(_worldName.Text.Trim());
		using OpenFileDialog picker = new() { Filter = TextFor("WorldFilter"), CheckFileExists = true };
		if (picker.ShowDialog(this) != DialogResult.OK) return;
		GameServer server = Server;
		string name = _worldName.Text.Trim();
		string fileName = picker.FileName;
		using MinecraftStaging staging = await Task.Run(() => MinecraftWorlds.PrepareImport(server, fileName, name));
		IReadOnlyList<MinecraftFileChange> changes = await Task.Run(() => staging.Changes(server));
		using MinecraftChangePreview preview = new(TextFor("ImportWorld"), changes, []);
		if (preview.ShowDialog(this) != DialogResult.OK) return;
		await Task.Run(() => MinecraftContentTransactions.Apply(server, name, "world", changes));
		await RefreshPageAsync();
	}

	private async Task ImportPackAsync()
	{
		ModPackageManager.EnsureStopped(Server);
		if (ModSecurityScanner.IsCurrentProcessElevated()) throw new InvalidOperationException(LocalizationManager.Get("ModManager.Error.ElevatedProcess"));
		using OpenFileDialog picker = new() { Filter = TextFor("PackFilter"), CheckFileExists = true };
		if (picker.ShowDialog(this) != DialogResult.OK || !Confirm("PackNotice")) return;
		GameServer server = Server;
		IProgress<string> progress = Progress();
		bool optional = _optionalMods.Checked;
		string fileName = picker.FileName;
		using MinecraftPreparedPack pack = await Task.Run(() => MinecraftModpacks.PrepareAsync(server, fileName, progress, _cancellation!.Token, optional));
		string antivirus = await ModSecurityScanner.ReviewMinecraftContentAsync(pack.Staging.Root, _cancellation!.Token);
		MinecraftCompatibilityReport report = await Task.Run(() => MinecraftCompatibility.Scan(server, pack.Changes));
		List<string> notes = report.Findings.Select(finding => finding.Area + ": " + finding.Detail).ToList();
		notes.Insert(0, antivirus);
		notes.AddRange(pack.Notes.Select(file => TextFor("Skipped", file)));
		using MinecraftChangePreview preview = new(pack.Name + " " + pack.Version, pack.Changes, notes, !report.Findings.Any(finding => finding.DefiniteProblem));
		if (preview.ShowDialog(this) != DialogResult.OK) return;
		await Task.Run(() => MinecraftContentTransactions.Apply(server, pack.Name + " " + pack.Version, "pack:" + pack.Name, pack.Changes));
		await RefreshPageAsync();
	}

	private async Task ShowCompatibilityAsync()
	{
		MinecraftCompatibilityReport report = await Task.Run(() => MinecraftCompatibility.Scan(Server));
		using MinecraftChangePreview preview = new(TextFor("Checks"), report);
		preview.ShowDialog(this);
	}

	private async Task CreateBackupAsync()
	{
		ModPackageManager.EnsureStopped(Server);
		ServerBackupPreflight preflight = await Core.Instance.CreateServerBackupPreflightAsync(Server);
		if (!preflight.Succeeded || !preflight.HasEnoughSpace) throw new IOException(preflight.Message + Environment.NewLine + TextFor("BackupSpace"));
		if (Confirm("ConfirmBackup")) await Core.Instance.ExecuteBackup(Server, StartContext.Manual);
		await RefreshPageAsync();
	}

	private async Task RestoreBackupAsync()
	{
		GameServer server = Server;
		ModPackageManager.EnsureStopped(server);
		using ServerBackupRestoreDialog dialog = new(server, await Core.Instance.GetServerBackupsAsync(server));
		if (dialog.ShowDialog(this) != DialogResult.OK || dialog.SelectedBackup == null || !Confirm("ConfirmRestore")) return;
		ServerBackupRestoreResult result = await Core.Instance.RestoreServerBackupAsync(server, dialog.SelectedBackup, Progress());
		if (result.Succeeded)
		{
			MinecraftConfigurationSync.SynchronizeRestored(server, FileHandler.SaveServers, fullRestore: true);
		}
		LocalizedMessageBox.Show(this, result.Message, Text, MessageBoxButtons.OK, result.Succeeded ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
		await RefreshPageAsync();
	}

	private async Task UndoChangeAsync()
	{
		if (Selected<MinecraftChangeReceipt>("Recovery") is not MinecraftChangeReceipt receipt || receipt.State is not ("Applied" or "Prepared")) return;
		using MinecraftChangePreview preview = new(TextFor("UndoChange"), receipt.Files.Select(file => new MinecraftFileChange(file.Path, file.Before == null ? null : "previous", file.Before, file.After)).ToArray(),
			[TextFor("UndoNotice")]);
		if (preview.ShowDialog(this) != DialogResult.OK) return;
		GameServer server = Server;
		// Keep the journal pending if persistence fails, so Start cannot use a stale profile.
		MinecraftContentTransactions.Undo(server, receipt.Id,
			synchronize: () => MinecraftConfigurationSync.SynchronizeRestored(server, FileHandler.SaveServers));
		await RefreshPageAsync();
	}

	internal static string ReadRecentLogs(GameServer server)
	{
		List<string> files = [];
		string latest = ModPathSafety.Resolve(server.InstallPath, "logs/latest.log");
		if (File.Exists(latest)) files.Add(latest);
		string crashRoot = ModPathSafety.Resolve(server.InstallPath, "crash-reports");
		if (Directory.Exists(crashRoot))
		{
			string? crash = Directory.EnumerateFiles(crashRoot, "*.txt").Take(1000).OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
			if (crash != null) files.Add(crash);
		}
		StringBuilder text = new();
		foreach (string file in files)
		{
			ModPathSafety.EnsureNoLinks(file);
			using FileStream stream = new(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
			stream.Seek(Math.Max(0, stream.Length - 512 * 1024), SeekOrigin.Begin);
			using StreamReader reader = new(stream);
			text.AppendLine(reader.ReadToEnd());
		}
		return text.ToString();
	}

	private sealed record PlayerActionChoice(string Key) { public override string ToString() => TextFor("PlayerAction." + Key); }

	// Pages provide keyboard/focus containment; the themed navigation replaces native tab headers.
	private sealed class WorkspacePages : TabControl
	{
		private bool _fittingPages;
		public override Rectangle DisplayRectangle => ClientRectangle;
		protected override void OnResize(EventArgs args)
		{
			base.OnResize(args);
			FitPages();
		}
		protected override void OnLayout(LayoutEventArgs args)
		{
			base.OnLayout(args);
			FitPages();
		}
		protected override void OnSelectedIndexChanged(EventArgs args)
		{
			FitPages();
			base.OnSelectedIndexChanged(args);
		}
		private void FitPages()
		{
			if (_fittingPages) return;
			_fittingPages = true;
			try
			{
				foreach (TabPage page in TabPages)
					if (page.Bounds != ClientRectangle) page.Bounds = ClientRectangle;
			}
			finally { _fittingPages = false; }
		}
		protected override void WndProc(ref Message message)
		{
			const int AdjustTabRectangle = 0x1328;
			if (message.Msg == AdjustTabRectangle) { message.Result = (IntPtr)1; return; }
			base.WndProc(ref message);
		}
	}
}
