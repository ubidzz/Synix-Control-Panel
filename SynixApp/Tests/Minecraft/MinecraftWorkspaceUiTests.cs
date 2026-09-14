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
using Synix_Control_Panel.SynixApp.Design.Controls;
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.UI.Configuration;
using Synix_Control_Panel.SynixApp.Localization;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixApp.UI.ServerManagement;
using Synix_Control_Panel.SynixEngine;
using Synix_Control_Panel.SynixEngine.Minecraft;
using Synix_Control_Panel.SynixEngine.ModManagement;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class MinecraftWorkspaceUiTests : IDisposable
{
	private readonly string _root = Path.Combine(Path.GetTempPath(), "SynixMinecraftUi-" + Guid.NewGuid().ToString("N"));
	private readonly string? _previousData = ModPackageManager.DataRootOverride;
	private readonly GameServer _server;
	public MinecraftWorkspaceUiTests()
	{
		_server = new() { Game = "Minecraft", ServerName = "Minecraft workspace fixture", InstallPath = Path.Combine(_root, "server"),
			Status = "Stopped", Port = 45011, QueryPort = 45012, GameMode = "Survival", WorldName = "world",
			GameVersion = "1.21.8", MinecraftLoader = "Fabric", MinecraftLoaderVersion = "0.16.14", RequiredJavaVersion = 21,
			EnableMinecraftManagementProtocol = false, RestartDays = [false, true, true, true, true, true, false] };
		Directory.CreateDirectory(Path.Combine(_server.InstallPath, "config"));
		File.WriteAllText(Path.Combine(_server.InstallPath, "server.properties"), "# fixture\nmax-players=20\nlevel-name=world");
		File.WriteAllText(Path.Combine(_server.InstallPath, "config", "fixture.toml"), "enabled=true");
		ModPackageManager.DataRootOverride = Path.Combine(_root, "history");
	}

	[Fact]
	public void AllWorkspacePagesRenderAndExposeRealWorkflows() => WorkflowUiTest.Run(() =>
	{
		using MinecraftControlCenter form = new(_server);
		form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-20000, -20000);
		form.Show();
		WorkflowUiTest.WaitUntil(() => !form.UseWaitCursor);
		TabControl tabs = Assert.IsAssignableFrom<TabControl>(form.Controls.Find("minecraftWorkspaceTabs", true).Single());
		Assert.Equal(new[] { "Overview", "Console", "Players", "Settings", "Worlds", "Mods", "Backups", "Schedules", "Recovery", "Diagnostics" },
			tabs.TabPages.Cast<TabPage>().Select(page => page.Name));
		Assert.Equal(10, Descendants(form).Count(control => control.Name.StartsWith("minecraftNav", StringComparison.Ordinal)));
		foreach (TabPage page in tabs.TabPages)
		{
			tabs.SelectedTab = page;
			WorkflowUiTest.WaitUntil(() => !form.UseWaitCursor);
			Assert.NotEmpty(page.Controls.Cast<Control>());
			form.Update();
			Capture(form, "minecraft-" + page.Name + ".png");
		}
		tabs.SelectedTab = tabs.TabPages["Settings"];
		WorkflowUiTest.WaitUntil(() => !form.UseWaitCursor);
		DataGridView settings = Assert.IsType<DataGridView>(form.Controls.Find("minecraftSettingsGrid", true).Single());
		Assert.Equal(2, settings.Rows.Count);
		Assert.Contains(settings.Rows.Cast<DataGridViewRow>(), row => row.Cells[0].Value?.ToString() == Path.Combine("config", "fixture.toml"));
		form.Size = form.MinimumSize;
		Application.DoEvents();
		form.Update();
		Assert.True(tabs.ClientSize.Width > 650);
		Assert.Equal(tabs.ClientSize, tabs.SelectedTab!.Size);
		Capture(form, "minecraft-settings-minimum.png");
		form.Close();
	});

	[Theory]
	[InlineData(true, true)]
	[InlineData(true, false)]
	[InlineData(false, true)]
	[InlineData(false, false)]
	public void ConsoleWrapsLongMessagesAndPreservesBufferedAndLiveLineBreaks(bool embedded, bool dark) => WorkflowUiTest.Run(() =>
	{
		bool beforeTheme = ThemeManager.IsDarkMode;
		try
		{
			ThemeManager.Initialize(dark);
			string longMessage = "[main/INFO] ModLauncher running: args [" +
				string.Join(" ", Enumerable.Range(0, 45).Select(index => $"--exampleArgument{index}=value{index}")) + "]";
			MinecraftConsoleHub.Publish(_server, longMessage, false);
			MinecraftConsoleHub.Publish(_server, "[Server thread/INFO] Preparing spawn area", false);
			using Form form = embedded ? new MinecraftControlCenter(_server) : new MinecraftConsoleDialog(_server);
			form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-20000, -20000); form.Show();
			WorkflowUiTest.WaitUntil(() => !form.UseWaitCursor);
			if (embedded)
			{
				TabControl tabs = Assert.IsAssignableFrom<TabControl>(form.Controls.Find("minecraftWorkspaceTabs", true).Single());
				tabs.SelectedTab = tabs.TabPages["Console"];
				WorkflowUiTest.WaitUntil(() => !form.UseWaitCursor);
			}
			RichTextBox output = Descendants(form).OfType<RichTextBox>().Single();
			ThemeManager.Apply(form); Application.DoEvents(); form.Update();
			Assert.True(output.Multiline);
			Assert.True(output.ReadOnly);
			Assert.True(output.WordWrap, "Long console messages must wrap within the output box.");
			Assert.Equal(RichTextBoxScrollBars.Vertical, output.ScrollBars);
			Assert.Equal(longMessage, output.Lines[0][11..]);
			Assert.Equal("[Server thread/INFO] Preparing spawn area", output.Lines[1][11..]);
			int messageStart = output.Text.IndexOf(longMessage, StringComparison.Ordinal);
			int VisualLineCount() => output.GetLineFromCharIndex(messageStart + longMessage.Length - 1) - output.GetLineFromCharIndex(messageStart);
			int wideLines = VisualLineCount();
			Assert.True(wideLines > 0, "The native text control must actually display the long message on multiple lines.");
			string beforeResize = output.Text;
			int beforeWidth = output.Width;
			form.Size = form.MinimumSize;
			Application.DoEvents(); form.Update();
			Assert.True(output.Width < beforeWidth);
			Assert.True(VisualLineCount() > wideLines, "Console wrapping must follow the available width when resized.");
			Assert.Equal(beforeResize, output.Text);
			WorkflowUiTest.Pump(Task.Run(() =>
			{
				MinecraftConsoleHub.Publish(_server,
					"first error\r\n    at Example.Start()\n\nCaused by: test failure\r    at Example.Run()\r\nrcon.password=fixture-only-secret\r\n", true);
				MinecraftConsoleHub.Publish(_server, "Following log entry", false);
			}));
			WorkflowUiTest.WaitUntil(() => output.Text.Contains("Following log entry", StringComparison.Ordinal));
			string displayed = output.Text.ReplaceLineEndings("\n");
			Assert.Contains("first error\n    at Example.Start()\n\nCaused by: test failure\n    at Example.Run()\nrcon.password=" + SecretRedactor.Removed + "\n[", displayed);
			Assert.DoesNotContain("fixture-only-secret", displayed);
			Assert.Equal(1, displayed.Split("Following log entry", StringSplitOptions.None).Length - 1);
			output.Select(output.Text.IndexOf("first error", StringComparison.Ordinal), "first error".Length);
			Assert.Equal(SettingsPalette.Danger, output.SelectionColor);
			output.Select(output.Text.IndexOf("Following log entry", StringComparison.Ordinal), "Following log entry".Length);
			Assert.Equal(SettingsPalette.PrimaryText, output.SelectionColor);
			output.Select(0, 0); output.ScrollToCaret();
			Capture(form, "minecraft-console-wrapped-" + embedded + "-" + dark + ".png"); form.Close();
		}
		finally { ThemeManager.Initialize(beforeTheme); }
	});

	[Theory]
	[InlineData("\r\n")]
	[InlineData("\n")]
	public void ConsolePreservesLineBreaksWhenLoadingTheRecentLog(string newline) => WorkflowUiTest.Run(() =>
	{
		Directory.CreateDirectory(Path.Combine(_server.InstallPath, "logs"));
		File.WriteAllText(Path.Combine(_server.InstallPath, "logs", "latest.log"),
			"[main/INFO] First saved entry" + newline + "[main/ERROR] Second saved entry");
		using MinecraftConsoleDialog form = new(_server);
		form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-20000, -20000); form.Show();
		RichTextBox output = Descendants(form).OfType<RichTextBox>().Single();
		WorkflowUiTest.WaitUntil(() => output.Text.Contains("Second saved entry", StringComparison.Ordinal));
		string[] lines = output.Text.ReplaceLineEndings("\n").TrimEnd('\n').Split('\n');
		Assert.Equal(2, lines.Length);
		Assert.EndsWith("[main/INFO] First saved entry", lines[0]);
		Assert.EndsWith("[main/ERROR] Second saved entry", lines[1]);
		form.Close();
	});

	[Fact]
	public void BusyWorkspacePreventsDuplicateActionsTabSwitchesAndClosing() => WorkflowUiTest.Run(() =>
	{
		using MinecraftControlCenter form = new(_server);
		TabControl tabs = Assert.IsAssignableFrom<TabControl>(form.Controls.Find("minecraftWorkspaceTabs", true).Single());
		TabPage selected = tabs.SelectedTab!;
		TaskCompletionSource pending = new();
		Task run = (Task)WorkflowUiTest.Invoke(form, "RunAsync", (Func<Task>)(() => pending.Task), false)!;
		Assert.True(form.UseWaitCursor);
		bool repeated = false;
		WorkflowUiTest.Pump((Task)WorkflowUiTest.Invoke(form, "RunAsync", (Func<Task>)(() => { repeated = true; return Task.CompletedTask; }), false)!);
		Assert.False(repeated);
		TabControlCancelEventArgs selecting = new(tabs.TabPages["Settings"], 3, false, TabControlAction.Selecting);
		typeof(TabControl).GetMethod("OnSelecting", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(tabs, [selecting]);
		Assert.True(selecting.Cancel);
		FormClosingEventArgs closing = new(CloseReason.UserClosing, false);
		WorkflowUiTest.Invoke(form, "OnFormClosing", closing);
		Assert.True(closing.Cancel);
		pending.SetResult();
		WorkflowUiTest.Pump(run);
		Assert.False(form.UseWaitCursor);
	});

	[Theory]
	[InlineData("success")]
	[InlineData("cancelled")]
	[InlineData("failure")]
	public void PendingWorkspaceActionCanFinishAfterDisposal(string outcome) => WorkflowUiTest.Run(() =>
	{
		using MinecraftControlCenter form = new(_server);
		TaskCompletionSource pending = new(TaskCreationOptions.RunContinuationsAsynchronously);
		Task run = (Task)WorkflowUiTest.Invoke(form, "RunAsync", (Func<Task>)(() => pending.Task), true)!;
		Assert.False(run.IsCompleted);
		form.Dispose();
		Assert.True(form.IsDisposed);
		if (outcome == "failure") pending.SetException(new IOException("Fixture refresh failed."));
		else if (outcome == "cancelled") pending.SetCanceled();
		else pending.SetResult();
		WorkflowUiTest.Pump(run);
		Assert.False(form.IsHandleCreated);
		bool startedAfterDisposal = false;
		WorkflowUiTest.Pump((Task)WorkflowUiTest.Invoke(form, "RunAsync", (Func<Task>)(() =>
			{ startedAfterDisposal = true; return Task.CompletedTask; }), false)!);
		Assert.False(startedAfterDisposal);
	});

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void PreviewRequiresExplicitCompatibleChanges(bool compatible) => WorkflowUiTest.Run(() =>
	{
		using MinecraftChangePreview form = new("Change preview", [new("mods/example.jar", "fixture", "hash", null)],
			["Compatibility checks are guidance, not a safety guarantee."], compatible);
		Control apply = form.Controls.Find("minecraftApplyChanges", true).Single();
		Assert.Equal(compatible, apply.Enabled);
		Assert.Equal(DialogResult.None, form.DialogResult);
		form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-20000, -20000); form.Show();
		form.Update(); Capture(form, "minecraft-preview-" + compatible + ".png"); form.Close();
	});

	[Fact]
	public void BedrockPlayerActionsDoNotOfferJavaOnlyBans() => WorkflowUiTest.Run(() =>
	{
		_server.MinecraftEdition = "Bedrock";
		using MinecraftControlCenter form = new(_server);
		ModernSettingsComboBox actions = Assert.IsType<ModernSettingsComboBox>(form.Controls.Find("minecraftPlayerAction", true).Single());
		Assert.Equal(5, actions.Items.Count);
		Assert.Equal("op \"Xbox Player\"", MinecraftControlCenter.BuildPlayerCommand(_server, "Xbox Player", "Operator"));
	});

	[Fact]
	public void FileEditorOpensTomlWithoutPretendingItIsIni() => WorkflowUiTest.Run(() =>
	{
		using MinecraftTextEditor form = new(_server, "config/fixture.toml");
		Assert.Contains("fixture.toml", form.Text);
		Assert.Contains(Descendants(form).OfType<TextBox>(), box => box.Multiline && box.Text == "enabled=true");
		Assert.Equal(SettingsPalette.Window, form.BackColor);
		TextBox text = Assert.IsType<TextBox>(form.Controls.Find("minecraftFileText", true).Single());
		Assert.Equal(BorderStyle.None, text.BorderStyle);
		Assert.IsType<ModernSettingsCard>(text.Parent);
		form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-20000, -20000); form.Show();
		form.Update(); Capture(form, "minecraft-file-editor.png"); form.Close();
	});

	[Fact]
	public void StructuredEditorSaveUpdatesTheSynixEntry() => WorkflowUiTest.Run(() =>
	{
		string path = Path.Combine(_server.InstallPath, "server.properties");
		using ServerConfig editor = new(path, ConfigFormat.StandardINI, _server);
		int saves = 0;
		editor.PersistServerChanges = () => { saves++; return true; };
		WorkflowUiTest.Invoke(editor, "LoadConfiguration");
		DataGridView grid = Assert.IsType<DataGridView>(editor.Controls.Find("dgvConfig", true).Single());
		DataGridViewRow row = grid.Rows.Cast<DataGridViewRow>().Single(item => item.Tag is ConfigLine { Key: "max-players" });
		row.Cells["colValue"].Value = "42";
		Assert.True((bool)WorkflowUiTest.Invoke(editor, "TrySaveCurrentConfiguration")!);
		Assert.Equal(42, _server.MaxPlayers);
		Assert.Equal(1, saves);
		Assert.Equal("42", MinecraftConfigurationSync.ReadProperties(File.ReadAllText(path))["max-players"]);
	});

	[Theory]
	[InlineData("de-DE", true)]
	[InlineData("fr-FR", false)]
	[InlineData("es-ES", true)]
	public void WorkspaceNavigationRendersInSupportedLanguagesAndThemes(string language, bool dark) => WorkflowUiTest.Run(() =>
	{
		string beforeLanguage = LocalizationManager.CurrentLanguageCode;
		bool beforeTheme = ThemeManager.IsDarkMode;
		try
		{
			LocalizationManager.Initialize(language); ThemeManager.Initialize(dark);
			using MinecraftControlCenter form = new(_server);
			form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-20000, -20000); form.Show();
			WorkflowUiTest.WaitUntil(() => !form.UseWaitCursor);
			Assert.DoesNotContain(Descendants(form), control => control.Text.StartsWith("MinecraftWorkspace.", StringComparison.Ordinal));
			TabControl tabs = Assert.IsAssignableFrom<TabControl>(form.Controls.Find("minecraftWorkspaceTabs", true).Single());
			tabs.SelectedTab = tabs.TabPages["Settings"];
			WorkflowUiTest.WaitUntil(() => !form.UseWaitCursor);
			form.Size = form.MinimumSize;
			Application.DoEvents();
			form.Update(); Capture(form, "minecraft-" + language + "-" + dark + ".png"); form.Close();
		}
		finally { LocalizationManager.Initialize(beforeLanguage); ThemeManager.Initialize(beforeTheme); }
	});

	[Theory]
	[InlineData("en-US", true, 1F)]
	[InlineData("fr-FR", false, 1F)]
	[InlineData("de-DE", true, 1.5F)]
	public void WorkspaceInputsUseSynixControlsAndStayInsideTheirCards(string language, bool dark, float scale) => WorkflowUiTest.Run(() =>
	{
		string beforeLanguage = LocalizationManager.CurrentLanguageCode;
		bool beforeTheme = ThemeManager.IsDarkMode;
		try
		{
			LocalizationManager.Initialize(language); ThemeManager.Initialize(dark);
			using MinecraftControlCenter form = new(_server);
			form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-20000, -20000); form.Show();
			WorkflowUiTest.WaitUntil(() => !form.UseWaitCursor);
			form.Size = form.MinimumSize;
			if (scale != 1F) form.Scale(new SizeF(scale, scale));
			TabControl tabs = Assert.IsAssignableFrom<TabControl>(form.Controls.Find("minecraftWorkspaceTabs", true).Single());
			foreach ((string page, string inputName) in new[] { ("Players", "minecraftPlayerName"), ("Worlds", "minecraftImportWorldName") })
			{
				tabs.SelectedTab = tabs.TabPages[page];
				WorkflowUiTest.WaitUntil(() => !form.UseWaitCursor);
				Application.DoEvents();
				TextBox input = Assert.IsType<TextBox>(form.Controls.Find(inputName, true).Single());
				input.Text = page == "Players" ? "SamplePlayer" : "Adventure World";
				ModernSettingsCard card = Assert.IsType<ModernSettingsCard>(input.Parent);
				Assert.Equal(BorderStyle.None, input.BorderStyle);
				Assert.Equal(SettingsPalette.Input, card.FillColor);
				Assert.Equal(SettingsPalette.Input, input.BackColor);
				Assert.Equal(SettingsPalette.PrimaryText, input.ForeColor);
				Assert.True(card.DisplayRectangle.Contains(input.Bounds));
				Assert.True(card.Parent!.ClientRectangle.Contains(card.Bounds));
				Assert.Equal(tabs.ClientRectangle, tabs.SelectedTab!.Bounds);
				Assert.True(tabs.RectangleToScreen(tabs.ClientRectangle).Contains(card.RectangleToScreen(card.ClientRectangle)));
				Assert.True(tabs.Parent!.ClientRectangle.Contains(tabs.Bounds));
				foreach (Control ancestor in new[] { card.Parent!, card.Parent!.Parent!, tabs.SelectedTab! })
					Assert.True(ancestor.Parent!.ClientRectangle.Contains(ancestor.Bounds), $"The {page} layout extends outside {ancestor.Parent.GetType().Name}.");
				form.Update(); Capture(form, "minecraft-styled-" + page + "-" + language + ".png");
			}
			ModernSettingsComboBox actions = Assert.IsType<ModernSettingsComboBox>(form.Controls.Find("minecraftPlayerAction", true).Single());
			Assert.Equal(DrawMode.OwnerDrawFixed, actions.DrawMode);
			Assert.Equal(SettingsPalette.Input, actions.BackColor);
			Assert.Equal(SettingsPalette.Border, actions.BorderColor);
			form.Close();
		}
		finally { LocalizationManager.Initialize(beforeLanguage); ThemeManager.Initialize(beforeTheme); }
	});

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void DiagnosticDetailsWrapAndResizeInsteadOfBeingEllipsized(bool dark) => WorkflowUiTest.Run(() =>
	{
		bool beforeTheme = ThemeManager.IsDarkMode;
		try
		{
			ThemeManager.Initialize(dark);
			using MinecraftControlCenter form = new(_server);
			form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-20000, -20000); form.Show();
			WorkflowUiTest.WaitUntil(() => !form.UseWaitCursor);
			TabControl tabs = Assert.IsAssignableFrom<TabControl>(form.Controls.Find("minecraftWorkspaceTabs", true).Single());
			tabs.SelectedTab = tabs.TabPages["Diagnostics"];
			WorkflowUiTest.WaitUntil(() => !form.UseWaitCursor);
			form.Size = form.MinimumSize;
			DataGridView grid = Assert.IsType<DataGridView>(form.Controls.Find("minecraftDiagnosticsGrid", true).Single());
			string detail = string.Join(" ", Enumerable.Repeat(MinecraftControlCenter.TextFor("NoLogFindings"), 3));
			grid.Rows.Clear(); grid.Rows.Add("Recent logs", detail);
			Assert.True(grid.DefaultCellStyle.WrapMode == DataGridViewTriState.True, "Window initialization must preserve wrapped diagnostic rows.");
			// Reapplying the theme must not restore the original one-line style.
			ThemeManager.Apply(form); Application.DoEvents(); form.Update();
			Assert.True(grid.Rows[0].Cells["Detail"].InheritedStyle.WrapMode == DataGridViewTriState.True,
				$"Wrapping: grid={grid.DefaultCellStyle.WrapMode}; rows={grid.RowsDefaultCellStyle.WrapMode}; " +
				$"row={grid.Rows[0].DefaultCellStyle.WrapMode}; template={grid.RowTemplate.DefaultCellStyle.WrapMode}; " +
				$"column={grid.Columns["Detail"]!.DefaultCellStyle.WrapMode}; cell={grid.Rows[0].Cells["Detail"].Style.WrapMode}");
			Assert.Equal(DataGridViewAutoSizeRowsMode.AllCells, grid.AutoSizeRowsMode);
			Assert.True(grid.Columns["Detail"]!.Width > grid.Columns["Area"]!.Width * 2);
			Assert.True(grid.Rows[0].Height > 70);
			Assert.Equal(detail, grid.Rows[0].Cells["Detail"].Value);
			Capture(form, "minecraft-wrapped-diagnostics-" + dark + ".png"); form.Close();
		}
		finally { ThemeManager.Initialize(beforeTheme); }
	});

	[Theory]
	[InlineData(true, 0)]
	[InlineData(false, 0)]
	[InlineData(true, 1)]
	[InlineData(false, 1)]
	[InlineData(true, 2)]
	[InlineData(false, 2)]
	public void CompatibilityReportUsesThemedReadOnlyResultsNotAFileChangePreview(bool dark, int scenario) => WorkflowUiTest.Run(() =>
	{
		bool beforeTheme = ThemeManager.IsDarkMode;
		try
		{
			ThemeManager.Initialize(dark);
			MinecraftCompatibilityReport report = new(scenario == 0 ? [] : [new("mods/example.jar", "example", "1.0", "Fabric", false, [])],
				scenario == 2 ? [new("mods/example.jar", MinecraftControlCenter.TextFor("NoCompatibilityFindings"))] : []);
			using MinecraftChangePreview form = new(MinecraftControlCenter.TextFor("Checks"), report);
			form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-20000, -20000); form.Show();
			form.Size = form.MinimumSize; Application.DoEvents(); form.Update();
			Assert.Equal(SettingsPalette.Window, form.BackColor);
			Assert.DoesNotContain(Descendants(form), control => control.BackColor == SystemColors.Control);
			Assert.Empty(form.Controls.Find("minecraftApplyChanges", true));
			Assert.Equal(DialogResult.None, form.DialogResult);
			DataGridView grid = Assert.IsType<DataGridView>(form.Controls.Find("minecraftChangePreviewGrid", true).Single());
			Assert.Equal(new[] { "Area", "Detail" }, grid.Columns.Cast<DataGridViewColumn>().Select(column => column.Name));
			Assert.True(grid.ReadOnly);
			Assert.Single(grid.Rows.Cast<DataGridViewRow>());
			Assert.Equal(MinecraftControlCenter.TextFor("CompatibilitySummary", scenario == 0 ? 0 : 1, scenario == 2 ? 1 : 0),
				form.Controls.Find("minecraftPreviewSummary", true).Single().Text);
			if (scenario < 2)
				Assert.Equal(MinecraftControlCenter.TextFor(scenario == 0 ? "NoCompatibilityAddOns" : "NoCompatibilityFindings"), grid.Rows[0].Cells["Detail"].Value);
			TextBox notes = Assert.IsType<TextBox>(form.Controls.Find("minecraftPreviewNotes", true).Single());
			Assert.IsType<ModernSettingsCard>(notes.Parent);
			Assert.Equal(BorderStyle.None, notes.BorderStyle);
			Assert.True(notes.ReadOnly);
			Assert.True(notes.WordWrap);
			Capture(form, "minecraft-compatibility-report-" + dark + "-" + scenario + ".png"); form.Close();
		}
		finally { ThemeManager.Initialize(beforeTheme); }
	});

	[Fact]
	public void StandalonePlayerMenuIsHiddenOnlyForMinecraftAndUnavailableServers() => WorkflowUiTest.Run(() =>
	{
		using ToolStripMenuItem item = new();
		foreach (GameInfo game in GameDatabase.GetGames)
		{
			foreach (string status in new[] { "Stopped", "Running" })
			{
				GameServer server = new() { Game = game.Game, Status = status };
				bool expected = !GameDatabase.IsMinecraft(game.Game) && status == "Running" && GameDatabase.SupportsPlayerManagement(server);
				MainGUI.UpdatePlayerManagementMenuItem(item, server);
				Assert.Equal(expected, item.Available);
				Assert.Equal(expected, item.Enabled);
			}
		}
		foreach (GameServer? server in new GameServer?[]
		{
			new() { Game = "Minecraft", MinecraftEdition = "Java", Status = "Running" },
			new() { Game = "Minecraft", MinecraftEdition = "Bedrock", Status = "Running" },
			new() { Game = "Minecraft Bedrock", Status = "Running" }, null
		})
		{
			MainGUI.UpdatePlayerManagementMenuItem(item, server);
			Assert.False(item.Available);
			Assert.False(item.Enabled);
		}
	});

	private static IEnumerable<Control> Descendants(Control control)
	{
		foreach (Control child in control.Controls)
		{
			yield return child;
			foreach (Control nested in Descendants(child)) yield return nested;
		}
	}

	private static void Capture(Form form, string name)
	{
		string? root = Environment.GetEnvironmentVariable("SYNIX_TEST_UI_PREVIEW_DIR");
		if (string.IsNullOrWhiteSpace(root)) return;
		Directory.CreateDirectory(root);
		using Bitmap bitmap = new(form.Width, form.Height);
		form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size));
		bitmap.Save(Path.Combine(root, name), ImageFormat.Png);
	}

	public void Dispose()
	{
		ModPackageManager.DataRootOverride = _previousData;
		Assert.Equal(Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.GetTempPath())), Path.GetDirectoryName(Path.GetFullPath(_root)));
		Assert.StartsWith("SynixMinecraftUi-", Path.GetFileName(_root));
		ModPathSafety.EnsureTreeHasNoLinks(_root);
		Directory.Delete(_root, true);
	}
}
