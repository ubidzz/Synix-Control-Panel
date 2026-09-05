// PROJECT: Synix Game Server Control Panel
// COPYRIGHT: © 2026 Jason Turner (ubidzz). All Rights Reserved.
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.Design.Controls;

namespace Synix_Control_Panel.SynixApp.UI.ServerManagement;

partial class SatisfactoryControlDialog
{
	private Panel pages = null!;
	private Panel setupPage = null!;
	private Panel overviewPage = null!;
	private Panel optionsPage = null!;
	private Panel savesPage = null!;
	private Panel consolePage = null!;
	private readonly Dictionary<Panel, ModernSettingsButton> pageButtons = [];
	private TextBox commandInput = null!;
	private TextBox saveNameInput = null!;
	private TextBox consoleOutput = null!;
	private DataGridView overviewGrid = null!;
	private DataGridView optionsGrid = null!;
	private DataGridView savesGrid = null!;
	private Label connectionStatus = null!;
	private ModernSettingsButton connectAutomatically = null!;
	private ModernSettingsButton forget = null!;
	private ModernSettingsButton close = null!;
	private readonly List<ModernSettingsButton> actionButtons = [];

	private void InitializeComponent()
	{
		SuspendLayout();
		Name = "SatisfactoryControlDialog";
		Text = T("Title");
		StartPosition = FormStartPosition.CenterParent;
		ClientSize = new Size(960, 780);
		MinimumSize = new Size(880, 760);
		Font = new Font("Segoe UI", 9.5F);
		BackColor = SettingsPalette.Window;
		ForeColor = SettingsPalette.PrimaryText;
		ShowInTaskbar = false;
		MinimizeBox = false;

		TableLayoutPanel shell = new()
		{
			Dock = DockStyle.Fill, Padding = new Padding(24), ColumnCount = 1, RowCount = 5
		};
		shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
		shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
		shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
		shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
		shell.Controls.Add(new Label { Text = T("Title"), Dock = DockStyle.Fill,
			Font = new Font("Segoe UI", 19, FontStyle.Bold) }, 0, 0);
		connectionStatus = new Label { Name = "connectionStatus", Text = T("NotConnected"),
			Dock = DockStyle.Fill, ForeColor = SettingsPalette.SecondaryText };
		shell.Controls.Add(connectionStatus, 0, 1);
		pages = new Panel { Name = "satisfactoryPages", Dock = DockStyle.Fill };
		setupPage = Page("Setup");
		overviewPage = Page("Overview");
		optionsPage = Page("Options");
		savesPage = Page("Saves");
		consolePage = Page("Console");
		pages.Controls.AddRange([setupPage, overviewPage, optionsPage, savesPage, consolePage]);
		TableLayoutPanel navigation = new() { Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 1, Margin = Padding.Empty };
		navigation.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		int index = 0;
		foreach (Panel page in pages.Controls)
		{
			navigation.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
			ModernSettingsButton button = Button(page.Name[..^4], (_, _) => ShowPage(page));
			button.Dock = DockStyle.Fill;
			pageButtons.Add(page, button);
			navigation.Controls.Add(button, index++, 0);
		}
		shell.Controls.Add(navigation, 0, 2);
		shell.Controls.Add(pages, 0, 3);
		FlowLayoutPanel footer = new() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
		close = Button("Close", (_, _) => Close());
		footer.Controls.Add(close);
		shell.Controls.Add(footer, 0, 4);
		Controls.Add(shell);
		CancelButton = close;

		TableLayoutPanel setup = PageLayout(setupPage, 5);
		setup.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
		setup.RowStyles.Add(new RowStyle(SizeType.Absolute, 118));
		setup.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
		setup.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));
		setup.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		setup.Controls.Add(new Label { Text = T("ConnectHeading"), Dock = DockStyle.Fill,
			Font = new Font("Segoe UI", 15, FontStyle.Bold) }, 0, 0);
		setup.Controls.Add(new Label { Name = "tokenSteps", Text = T("SetupSteps"), Dock = DockStyle.Fill }, 0, 1);
		connectAutomatically = Button("ConnectAutomatically", async (_, _) => await RunAsync(ConnectAutomaticallyAsync));
		connectAutomatically.UseAccentStyle = true;
		connectAutomatically.Size = new Size(340, 48);
		setup.Controls.Add(connectAutomatically, 0, 2);
		setup.Controls.Add(new Label { Text = T("TokenSafety"), Dock = DockStyle.Fill,
			Padding = new Padding(0, 14, 0, 0), ForeColor = SettingsPalette.SecondaryText }, 0, 3);

		TableLayoutPanel overview = PageLayout(overviewPage, 2);
		overview.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		overview.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
		overviewGrid = Grid("overviewGrid", true, ("Field", 45), ("Value", 55));
		overview.Controls.Add(TableCard(overviewGrid), 0, 0);
		FlowLayoutPanel lifecycle = new() { Dock = DockStyle.Fill };
		lifecycle.Controls.Add(ActionButton("Refresh", async () => await RefreshOverviewAsync()));
		lifecycle.Controls.Add(Button("Start", async (_, _) => await LifecycleAsync(false, false)));
		lifecycle.Controls.Add(Button("Restart", async (_, _) => await LifecycleAsync(true, false)));
		lifecycle.Controls.Add(Button("Stop", async (_, _) => await LifecycleAsync(false, true)));
		forget = Button("Disconnect", (_, _) => Disconnect());
		lifecycle.Controls.Add(forget);
		overview.Controls.Add(lifecycle, 0, 1);

		TableLayoutPanel options = PageLayout(optionsPage, 3);
		options.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
		options.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		options.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
		options.Controls.Add(new Label { Text = T("OptionsHelp"), Dock = DockStyle.Fill }, 0, 0);
		optionsGrid = Grid("optionsGrid", false, ("Setting", 50), ("Value", 25), ("Pending", 25));
		optionsGrid.Columns[0].ReadOnly = optionsGrid.Columns[2].ReadOnly = true;
		options.Controls.Add(TableCard(optionsGrid), 0, 1);
		FlowLayoutPanel optionButtons = new() { Dock = DockStyle.Fill };
		optionButtons.Controls.Add(ActionButton("Refresh", RefreshOptionsAsync));
		optionButtons.Controls.Add(ActionButton("ApplyOptions", ApplyOptionsAsync));
		options.Controls.Add(optionButtons, 0, 2);

		TableLayoutPanel saves = PageLayout(savesPage, 4);
		saves.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
		saves.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		saves.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
		saves.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
		saves.Controls.Add(new Label { Text = T("SavesHelp"), Dock = DockStyle.Fill }, 0, 0);
		savesGrid = Grid("savesGrid", true, ("Session", 40), ("SaveName", 40), ("Date", 20));
		saves.Controls.Add(TableCard(savesGrid), 0, 1);
		saveNameInput = new TextBox { Name = "saveNameInput", PlaceholderText = T("SaveNameHint"), Dock = DockStyle.Fill };
		saves.Controls.Add(InputCard(saveNameInput), 0, 2);
		FlowLayoutPanel saveButtons = new() { Dock = DockStyle.Fill };
		saveButtons.Controls.Add(ActionButton("Refresh", RefreshSavesAsync));
		saveButtons.Controls.Add(ActionButton("SaveNow", SaveNowAsync));
		saveButtons.Controls.Add(ActionButton("LoadSave", LoadSaveAsync));
		saveButtons.Controls.Add(ActionButton("Upload", UploadAsync));
		saveButtons.Controls.Add(ActionButton("Download", DownloadAsync));
		saves.Controls.Add(saveButtons, 0, 3);

		TableLayoutPanel console = PageLayout(consolePage, 4);
		console.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
		console.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		console.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
		console.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
		console.Controls.Add(new Label { Text = T("ConsoleHelp"), Dock = DockStyle.Fill }, 0, 0);
		consoleOutput = new TextBox { Name = "consoleOutput", Multiline = true, ReadOnly = true,
			ScrollBars = ScrollBars.Both, Dock = DockStyle.Fill, WordWrap = false };
		console.Controls.Add(InputCard(consoleOutput), 0, 1);
		commandInput = new TextBox { Name = "commandInput", Dock = DockStyle.Fill, MaxLength = 1024 };
		console.Controls.Add(InputCard(commandInput), 0, 2);
		console.Controls.Add(ActionButton("SendCommand", SendCommandAsync), 0, 3);
		ShowPage(setupPage);
		ResumeLayout(true);
	}

	private void ShowPage(Panel selected)
	{
		foreach (var pair in pageButtons)
		{
			pair.Key.Visible = ReferenceEquals(pair.Key, selected);
			pair.Value.UseAccentStyle = ReferenceEquals(pair.Key, selected);
		}
		selected.BringToFront();
	}
	private static Panel Page(string key) => new()
	{ Name = key + "Page", Dock = DockStyle.Fill, AutoScroll = true, BackColor = SettingsPalette.Window, ForeColor = SettingsPalette.PrimaryText };
	private static TableLayoutPanel PageLayout(Panel page, int rows)
	{
		TableLayoutPanel layout = new() { Dock = DockStyle.Fill, RowCount = rows, ColumnCount = 1, AutoScroll = true };
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		ModernSettingsCard card = new() { Name = page.Name + "Card", Dock = DockStyle.Fill,
			Padding = new Padding(18), CornerRadius = 14, FillColor = SettingsPalette.Card,
			BackColor = SettingsPalette.Card, BorderColor = SettingsPalette.Border };
		layout.BackColor = SettingsPalette.Card;
		card.Controls.Add(layout);
		page.Controls.Add(card);
		return layout;
	}
	private static ModernSettingsCard InputCard(TextBox input)
	{
		input.BorderStyle = BorderStyle.None;
		input.BackColor = SettingsPalette.Input;
		input.ForeColor = SettingsPalette.PrimaryText;
		input.Dock = DockStyle.Fill;
		ModernSettingsCard card = new() { Name = input.Name + "Card", Dock = DockStyle.Fill,
			Padding = new Padding(10, 8, 10, 8), FillColor = SettingsPalette.Input,
			BackColor = SettingsPalette.Input, BorderColor = SettingsPalette.BorderHover, CornerRadius = 8, Margin = new Padding(0, 3, 0, 3) };
		card.Controls.Add(input);
		return card;
	}
	private static ModernSettingsCard TableCard(DataGridView grid)
	{
		ModernSettingsCard card = new() { Name = grid.Name + "Card", Dock = DockStyle.Fill,
			Padding = new Padding(2), FillColor = SettingsPalette.Input,
			BackColor = SettingsPalette.Input, BorderColor = SettingsPalette.Border, CornerRadius = 10, Margin = Padding.Empty };
		card.Controls.Add(grid);
		return card;
	}
	private static ModernSettingsButton Button(string key, EventHandler handler)
	{
		ModernSettingsButton button = new() { Name = "satisfactory" + key, Text = T(key), Size = new Size(184, 44), Margin = new Padding(4) };
		button.Click += handler;
		return button;
	}
	private ModernSettingsButton ActionButton(string key, Func<Task> action)
	{
		ModernSettingsButton button = Button(key, async (_, _) => await RunAsync(action));
		actionButtons.Add(button);
		return button;
	}
	private static DataGridView Grid(string name, bool readOnly, params (string Key, int Weight)[] columns)
	{
		DataGridView grid = new() { Name = name, Dock = DockStyle.Fill, ReadOnly = readOnly, AutoGenerateColumns = false,
			AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false,
			RowHeadersVisible = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
			AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells };
		foreach (var column in columns)
			grid.Columns.Add(new DataGridViewTextBoxColumn { Name = column.Key, HeaderText = T(column.Key),
				AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = column.Weight });
		GridStyler.DarkTheme(grid);
		GridStyler.ApplyDashboardTheme(grid);
		StyleTable(grid);
		grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
		return grid;
	}
	private static void StyleTable(DataGridView grid)
	{
		grid.EnableHeadersVisualStyles = false;
		grid.BorderStyle = BorderStyle.None;
		grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
		grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
		grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
		grid.ColumnHeadersHeight = 42;
		grid.RowTemplate.MinimumHeight = 38;
		grid.BackgroundColor = SettingsPalette.Input;
		grid.GridColor = SettingsPalette.Border;
		grid.ColumnHeadersDefaultCellStyle.BackColor = SettingsPalette.Sidebar;
		grid.ColumnHeadersDefaultCellStyle.ForeColor = SettingsPalette.SecondaryText;
		grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = SettingsPalette.Sidebar;
		grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = SettingsPalette.SecondaryText;
		grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 6, 10, 6);
		grid.DefaultCellStyle.BackColor = SettingsPalette.Input;
		grid.DefaultCellStyle.ForeColor = SettingsPalette.PrimaryText;
		grid.DefaultCellStyle.SelectionBackColor = SettingsPalette.Selection;
		grid.DefaultCellStyle.SelectionForeColor = SettingsPalette.PrimaryText;
		grid.DefaultCellStyle.Padding = new Padding(10, 7, 10, 7);
		grid.AlternatingRowsDefaultCellStyle.BackColor = SettingsPalette.AlternateInput;
	}
}
