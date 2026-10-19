using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.Design.Controls;
using Synix_Control_Panel.SynixApp.Localization;
using Synix_Control_Panel.SynixEngine;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class ServerHistoryAndDashboardDisplayTests
{
	public static IEnumerable<object[]> HistoryCases() =>
		from language in new[] { "en-US", "de-DE", "ja-JP", "zh-Hans", "zh-Hant", "hi-IN", "th-TH" }
		from dark in new[] { true, false }
		select new object[] { language, dark };

	[Theory]
	[MemberData(nameof(HistoryCases))]
	public void HistoryUsesReadableThemeAndShowsEmptyAndFilteredStates(string language, bool dark)
	{
		WorkflowUiTest.Run(() =>
		{
			bool originalTheme = ThemeManager.IsDarkMode;
			try
			{
				LocalizationManager.Initialize(language);
				ThemeManager.Initialize(dark);
				ServerActivityHistory history = new(null);
				GameServer server = new() { Game = "ARK: Survival Ascended", ServerName = "International server · 世界 · Überleben" };
				using ServerActivityDialog dialog = new(server, history) { StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000) };
				dialog.Show();
				WorkflowUiTest.Pump(dialog.ReloadAsync());
				DataGridView grid = Find<DataGridView>(dialog, "historyGrid");
				Label empty = Find<Label>(dialog, "historyEmptyState");
				TextBox search = Find<TextBox>(dialog, "historySearch");
				foreach (Size size in new[] { dialog.Size, dialog.MinimumSize })
				{
					dialog.Size = size; Application.DoEvents();
					Assert.Equal(Padding.Empty, dialog.Padding);
					Assert.Equal(SettingsPalette.Window, dialog.BackColor);
					Assert.Equal(SettingsPalette.Input, grid.BackgroundColor);
					Assert.Equal(SettingsPalette.Input, search.BackColor);
					Assert.Equal(SettingsPalette.PrimaryText, search.ForeColor);
					Assert.Equal(SettingsPalette.PrimaryText, Find<CheckBox>(dialog, "historyAttention").ForeColor);
					Assert.Equal(SettingsPalette.PrimaryText, Find<Label>(dialog, "historyIdentity").ForeColor);
					Assert.Equal(SettingsPalette.SecondaryText, Find<Label>(dialog, "historyNotice").ForeColor);
					Assert.True(empty.Visible);
					Assert.Equal(LocalizationManager.Get("History.Empty"), empty.Text);
					Assert.True(empty.Top > grid.ColumnHeadersHeight);
					Assert.True(empty.Bottom < grid.ClientSize.Height);
					Assert.True(grid.Height > 100, $"{language}: history table height is {grid.Height}");
					AssertLabelFits(Find<Label>(dialog, "historyNotice"));
					AssertLabelFits(empty);
					Assert.True(Find<TableLayoutPanel>(dialog, "historySearchSurface").Height >= search.PreferredHeight + 16);
				}
				SavePreview(dialog, $"history-empty-{language}-{dark}");
				history.Record(server, "ConfigurationSaved");
				history.Record(server, DiscordNotificationEvent.BackupFailed);
				WorkflowUiTest.WaitUntil(() => grid.Rows.Count == 2);
				Assert.False(empty.Visible);
				search.Text = "no matching activity 56789";
				Assert.Empty(grid.Rows.Cast<DataGridViewRow>());
				Assert.True(empty.Visible);
				Assert.Equal(LocalizationManager.Get("History.NoMatches"), empty.Text);
				search.Clear();
				Assert.False(empty.Visible);
				Assert.Equal(2, grid.Rows.Count);
				SavePreview(dialog, $"history-populated-{language}-{dark}");
				string detailedFailure = "archive-disk-full " + string.Join(" ", Enumerable.Repeat("Review the backup destination and free disk space.", 12));
				Assert.True(history.RecordDiagnostic(server, "Backup.Activity.Error", Color.Red, detailedFailure));
				Assert.True(history.RecordDiagnostic(server, "Probe.Activity.A2S", Color.LimeGreen, "Enshrouded", 15637));
				WorkflowUiTest.WaitUntil(() => grid.Rows.Count == 4);
				search.Text = "archive-disk-full";
				Assert.Single(grid.Rows.Cast<DataGridViewRow>());
				TextBox details = Find<TextBox>(dialog, "historyDetails");
				Assert.True(details.ReadOnly);
				Assert.Contains(detailedFailure, details.Text);
				Assert.True(Assert.IsType<string>(grid.Rows[0].Cells["Activity"].Value).Length < details.Text.Length);
				Assert.True(details.Height >= details.Font.Height * 2, $"{language}: details need room to read and scroll");
				Assert.Equal(SettingsPalette.Input, details.BackColor);
				Assert.Equal(SettingsPalette.PrimaryText, details.ForeColor);
				Find<CheckBox>(dialog, "historyAttention").Checked = true;
				search.Text = "15637";
				Assert.Empty(grid.Rows.Cast<DataGridViewRow>());
				Assert.Empty(details.Text);
				Find<CheckBox>(dialog, "historyAttention").Checked = false;
				Assert.Single(grid.Rows.Cast<DataGridViewRow>());
				Assert.Contains("15637", details.Text);
				search.Clear();
				SavePreview(dialog, $"history-detailed-{language}-{dark}");
				ThemeManager.SetDarkMode(!dark);
				Assert.Equal(SettingsPalette.Window, dialog.BackColor);
				Assert.Equal(SettingsPalette.Input, grid.BackgroundColor);
				Assert.Equal(SettingsPalette.SecondaryText, empty.ForeColor);
			}
			finally { ThemeManager.Initialize(originalTheme); LocalizationManager.Initialize("en-US"); }
		});
	}

	[Theory]
	[MemberData(nameof(OperationalLayoutTests.Languages), MemberType = typeof(OperationalLayoutTests))]
	public void EmptyHistoryMessagesExistInEveryInterfaceLanguage(string language)
	{
		try
		{
			LocalizationManager.Initialize("en-US");
			var english = new[] { "History.Empty", "History.NoMatches", "History.Details", "History.Retention" }.ToDictionary(key => key, key => LocalizationManager.Get(key));
			LocalizationManager.Initialize(language);
			foreach (string key in english.Keys)
			{
				string value = LocalizationManager.Get(key);
				Assert.NotEqual(key, value);
				Assert.False(string.IsNullOrWhiteSpace(value));
				if (language != "en-US") Assert.NotEqual(english[key], value);
			}
		}
		finally { LocalizationManager.Initialize("en-US"); }
	}

	[Theory]
	[InlineData("en-US", true, 938, 9.5f)]
	[InlineData("en-US", false, 938, 9.5f)]
	[InlineData("de-DE", true, 740, 9.5f)]
	[InlineData("ja-JP", true, 938, 9.5f)]
	[InlineData("zh-Hans", true, 938, 9.5f)]
	[InlineData("zh-Hant", false, 938, 9.5f)]
	[InlineData("hi-IN", false, 938, 9.5f)]
	[InlineData("th-TH", true, 938, 9.5f)]
	[InlineData("en-US", true, 1172, 14f)]
	[InlineData("de-DE", false, 1407, 16f)]
	public void PopulatedServerGridWrapsNamesAndBackupsWithoutClippingMetrics(string language, bool dark, int width, float fontSize)
	{
		WorkflowUiTest.Run(() =>
		{
			LicenseContext originalContext = LicenseManager.CurrentContext;
			bool originalTheme = ThemeManager.IsDarkMode;
			try
			{
				LocalizationManager.Initialize(language);
				ThemeManager.Initialize(dark);
				LicenseManager.CurrentContext = new DesigntimeLicenseContext();
				using MainGUI dashboard = new(); // Never load installed servers or start background services.
				DataGridView grid = Find<DataGridView>(dashboard, "dataGridView1");
				grid.DataBindingComplete -= Handler<DataGridViewBindingCompleteEventHandler>(dashboard, "dataGridView1_DataBindingComplete");
				grid.SelectionChanged -= Handler<EventHandler>(dashboard, "dataGridView1_SelectionChanged");
				grid.Columns.Insert(0, new DataGridViewImageColumn { Name = "IconCol", HeaderText = "", DataPropertyName = "DisplayIcon", ImageLayout = DataGridViewImageCellLayout.Zoom, AutoSizeMode = DataGridViewAutoSizeColumnMode.None });
				GridStyler.DarkTheme(grid);
				GridStyler.ApplyDashboardTheme(grid);
				dashboard.InitializeActivityHistory(monitorBackups: false);
				grid.CellPainting += Handler<DataGridViewCellPaintingEventHandler>(dashboard, "dataGridView1_CellPainting");
				using Font bodyFont = new("Segoe UI", fontSize);
				GameServer[] servers = [
					new() { Game = "Minecraft", ServerName = "Community survival world", Port = 25565, QueryPort = 25566, Status = "Stopped" },
					new() { Game = "ARK: Survival Ascended", ServerName = "International community · 世界 · Überleben · Cooperative adventures and weekend games with friends around the world", Port = 7777, QueryPort = 27015, Status = "Stopped" },
					new() { Game = "Enshrouded", ServerName = "Adventure with friends", Port = 15636, QueryPort = 15637, Status = "Starting" }
				];
				var cache = (Dictionary<string, (ServerBackupHealth Health, DateTimeOffset Checked)>)typeof(MainGUI)
					.GetField("_backupHealth", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(dashboard)!;
				DateTimeOffset now = DateTimeOffset.UtcNow;
				cache[ServerActivityHistory.ServerKey(servers[0])] = (new(), now);
				cache[ServerActivityHistory.ServerKey(servers[1])] = (new(LatestCompletedUtc: now.AddDays(-34)), now);
				cache[ServerActivityHistory.ServerKey(servers[2])] = (new(LatestCompletedUtc: now.AddDays(-1), LastFailureUtc: now), now);
				using Form host = new() { ClientSize = new Size(width, 440), ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000) };
				grid.Dock = DockStyle.Fill; host.Controls.Add(grid);
				grid.DataSource = new BindingList<GameServer>(servers);
				host.Show(); Application.DoEvents();
				// Reparenting can reset inherited grid fonts; apply text scaling to the
				// displayed table so the large-text cases exercise the actual cell font.
				grid.DefaultCellStyle.Font = bodyFont;
				foreach (DataGridViewRow row in grid.Rows)
					foreach (DataGridViewCell cell in row.Cells) cell.Style.Font = bodyFont;
				Assert.Equal(fontSize, grid.Rows[1].Cells["colName"].InheritedStyle.Font!.Size);
				WorkflowUiTest.Invoke(dashboard, "ResizeServerGridRows");
				Assert.Equal(3, grid.Rows.Count);
				Assert.Equal(SettingsPalette.Input, grid.BackgroundColor);
				foreach (DataGridViewColumn column in grid.Columns.Cast<DataGridViewColumn>().Where(column => column.Name != "IconCol"))
				{
					var style = column.HeaderCell.InheritedStyle;
					int headingWidth = TextRenderer.MeasureText(column.HeaderText, style.Font).Width + style.Padding.Horizontal;
					Assert.True(column.Width >= headingWidth, $"{language}: heading {column.HeaderText} is clipped");
				}
				foreach (DataGridViewRow row in grid.Rows)
				{
					Assert.True(row.Height >= row.GetPreferredHeight(row.Index, DataGridViewAutoSizeRowMode.AllCellsExceptHeader, fixedWidth: true),
						$"{language}: row {row.Index} is too short for its current column widths");
					foreach (string name in new[] { "colGame", "colName", "BackupHealth", "colStatus" })
					{
						DataGridViewCell cell = row.Cells[name];
						Assert.Equal(DataGridViewTriState.True, cell.InheritedStyle.WrapMode);
						Assert.True(row.Height >= cell.PreferredSize.Height, $"{language}/{width}/{fontSize}: {name} requires {cell.PreferredSize.Height}px; row is {row.Height}px");
					}
					foreach (string name in new[] { "colPort", "colQueryPort", "colPlayerCount", "colUptime" })
					{
						DataGridViewCell cell = row.Cells[name];
						Assert.True(cell.Size.Width >= cell.PreferredSize.Width, $"{language}: {name} value {cell.FormattedValue} is clipped");
					}
				}
				Assert.True(grid.Rows[1].Height > 44, "Long server names should expand their row, not use ellipsis.");
				grid.CurrentCell = grid.Rows[2].Cells["colName"];
				SavePreview(grid, $"server-grid-{language}-{dark}-{width}");
				// Refreshing an unbound backup value must also recalculate its row height.
				cache[ServerActivityHistory.ServerKey(servers[1])] = (new(InvalidReceipt: true), now);
				WorkflowUiTest.Invoke(dashboard, "ResizeServerGridRows");
				Assert.True(grid.Rows[1].Height >= grid.Rows[1].Cells["BackupHealth"].PreferredSize.Height);
				Assert.Same(servers[2], grid.CurrentRow!.DataBoundItem);
				grid.Columns["colGame"]!.Width = grid.Columns["colGame"]!.MinimumWidth;
				host.ClientSize = new Size(680, 440); Application.DoEvents();
				foreach (DataGridViewRow row in grid.Rows)
					Assert.True(row.Height >= row.GetPreferredHeight(row.Index, DataGridViewAutoSizeRowMode.AllCellsExceptHeader, fixedWidth: true));
				Assert.Contains(grid.Controls.OfType<HScrollBar>(), bar => bar.Visible);
				LocalizationManager.Initialize(language == "de-DE" ? "en-US" : "de-DE");
				WorkflowUiTest.Invoke(dashboard, "ConfigureServerGridLayout");
				Assert.Equal(LocalizationManager.Get("Text.B80A37564FBB3C075320"), grid.Columns["colQueryPort"]!.HeaderText);
				foreach (DataGridViewColumn column in grid.Columns.Cast<DataGridViewColumn>().Where(column => column.Name != "IconCol"))
				{
					var style = column.HeaderCell.InheritedStyle;
					int needed = TextRenderer.MeasureText(column.HeaderText, style.Font).Width + style.Padding.Horizontal;
					Assert.True(column.Width >= needed, $"Language switch: {column.Name} / {column.HeaderText} needs {needed}px, has {column.Width}px, minimum {column.MinimumWidth}px");
				}
				Assert.False(dashboard.Visible);
			}
			finally { LicenseManager.CurrentContext = originalContext; ThemeManager.Initialize(originalTheme); LocalizationManager.Initialize("en-US"); }
		});
	}

	private static T Find<T>(Control root, string name) where T : Control => (T)root.Controls.Find(name, true).Single();
	private static T Handler<T>(object target, string method) where T : Delegate => (T)Delegate.CreateDelegate(typeof(T), target,
		target.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance)!);
	private static void AssertLabelFits(Label label)
	{
		int needed = TextRenderer.MeasureText(label.Text, label.Font, new Size(label.Width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height;
		Assert.True(label.Height >= needed, $"{label.Name} needs {needed}px but has {label.Height}px");
	}
	private static void SavePreview(Control control, string name)
	{
		if (Environment.GetEnvironmentVariable("SYNIX_RENDER_LANGUAGES") != "1") return;
		using Bitmap image = new(control.Width, control.Height);
		control.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
		image.Save(Path.Combine(AppContext.BaseDirectory, name + ".png"));
	}
}
