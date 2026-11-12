using System.Configuration;
using System.Drawing;
using System.Windows.Forms;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.Design.Controls;
using Synix_Control_Panel.SynixApp.Localization;
using Synix_Control_Panel.SynixApp.UI.Settings;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class AppearanceThemeTests
{
	public static IEnumerable<object[]> Themes() =>
		from theme in ThemeCatalog.Themes
		from dark in new[] { true, false }
		select new object[] { theme.Id, dark };

	[Theory]
	[MemberData(nameof(Themes))]
	public void ThemeAccentsKeepReadableText(string theme, bool dark)
	{
		try
		{
			foreach (var accent in ThemeCatalog.Accents)
			{
				ThemeManager.Initialize(dark, theme, accent.Id);
				AssertContrast(SettingsPalette.PrimaryText, SettingsPalette.Window);
				AssertContrast(SettingsPalette.PrimaryText, SettingsPalette.Input);
				AssertContrast(SettingsPalette.SecondaryText, SettingsPalette.Card);
				AssertContrast(SettingsPalette.PrimaryText, SettingsPalette.Selection);
				AssertContrast(SettingsPalette.AccentText, SettingsPalette.Accent);
				AssertContrast(SettingsPalette.AccentText, SettingsPalette.AccentHover);
			}
		}
		finally { ThemeManager.Initialize(true); }
	}

	[Fact]
	public void OldAndInvalidPreferencesUseSafeDefaults()
	{
		ThemeManager.Initialize(false, "removed-theme", "#bad-color", "unknown");
		try
		{
			Assert.False(ThemeManager.IsDarkMode);
			Assert.Equal("synix", ThemeManager.ThemeId);
			Assert.Equal("default", ThemeManager.AccentId);
			Assert.Equal("rounded", ThemeManager.StyleId);
			Assert.Equal("ocean", ThemeCatalog.NormalizeTheme(" OCEAN "));
			Assert.Equal("rose", ThemeCatalog.NormalizeAccent("ROSE"));
			foreach (string name in new[] { "ThemeName", "ThemeAccent", "ThemeStyle", "DarkMode" })
			{
				var property = typeof(Properties.Settings).GetProperty(name)!;
				Assert.NotNull(Attribute.GetCustomAttribute(property, typeof(UserScopedSettingAttribute)));
				Assert.NotNull(Attribute.GetCustomAttribute(property, typeof(DefaultSettingValueAttribute)));
			}
		}
		finally { ThemeManager.Initialize(true); }
	}

	[Fact]
	public void SwitchingOpenWindowsRepeatedlyKeepsRolesDataAndFutureControls()
	{
		WorkflowUiTest.Run(() =>
		{
			ThemeManager.Initialize(true);
			try
			{
				using Form host = Host();
				using ModernSettingsCard card = new() { Dock = DockStyle.Fill };
				using Label title = new() { ForeColor = SettingsPalette.PrimaryText, Text = "Server 世界", Top = 10 };
				using Label secondary = new() { ForeColor = SettingsPalette.SecondaryText, Top = 40 };
				using TextBox input = new() { Text = "user data 日本語", Top = 70 };
				using SynixButton action = new() { FillColor = SettingsPalette.Accent, ForeColor = SettingsPalette.Window, Top = 100 };
				using ModernSettingsButton button = new() { Top = 150, UseAccentStyle = true };
				using DataGridView grid = new() { Top = 210, Size = new Size(400, 160), AllowUserToAddRows = false };
				grid.Columns.Add("server", "Server");
				GridStyler.DarkTheme(grid);
				GridStyler.ApplyDashboardTheme(grid);
				GridStyler.ApplyRoundedCorners(grid, 12);
				grid.Rows.Add("Save");
				using ContextMenuStrip menu = new();
				var sub = new ToolStripMenuItem("Actions");
				sub.DropDownItems.Add("Open"); menu.Items.Add(sub);
				card.ContextMenuStrip = menu;
				card.Controls.AddRange([title, secondary, input, action, button, grid]);
				host.Controls.Add(card); host.Show(); ThemeManager.Apply(host);
				for (int round = 0; round < 2; round++)
				foreach (bool dark in new[] { false, true })
				foreach (var theme in ThemeCatalog.Themes)
				{
					string style = dark ? "rounded" : "square";
					ThemeManager.SetAppearance(dark, theme.Id, "rose", style);
					Assert.Equal(SettingsPalette.Window, host.BackColor);
					Assert.Equal(SettingsPalette.Card, card.FillColor);
					Assert.Equal(SettingsPalette.Border, card.BorderColor);
					Assert.Equal(SettingsPalette.PrimaryText, title.ForeColor);
					Assert.Equal(SettingsPalette.SecondaryText, secondary.ForeColor);
					Assert.Equal(SettingsPalette.Input, input.BackColor);
					Assert.Equal("user data 日本語", input.Text);
					Assert.Equal(SettingsPalette.Accent, action.FillColor);
					Assert.Equal(SettingsPalette.AccentText, action.ForeColor);
					Assert.Equal(SettingsPalette.Input, grid.BackgroundColor);
					Assert.Equal(SettingsPalette.Selection, grid.DefaultCellStyle.SelectionBackColor);
					Assert.Equal(SettingsPalette.Sidebar, grid.ColumnHeadersDefaultCellStyle.BackColor);
					Assert.Equal("Save", grid.Rows[0].Cells[0].Value);
					Assert.Equal(SettingsPalette.Card, sub.DropDown.BackColor);
					Assert.Equal(dark ? 12 : 0, card.CornerRadius);
					using Label added = new() { BackColor = Color.FromArgb(17, 27, 45), ForeColor = Color.FromArgb(158, 172, 194) };
					card.Controls.Add(added);
					Assert.Equal(SettingsPalette.Card, added.BackColor);
					Assert.Equal(SettingsPalette.SecondaryText, added.ForeColor);
				}
				ThemeManager.SetAppearance(true, "synix", "default", "rounded");
				Assert.Equal(Color.FromArgb(17, 27, 45), card.FillColor);
				Assert.Equal(Color.FromArgb(32, 214, 199), action.FillColor);
				Assert.Equal(12, card.CornerRadius);
			}
			finally { ThemeManager.Initialize(true); }
		});
	}

	[Theory]
	[MemberData(nameof(OperationalLayoutTests.Languages), MemberType = typeof(OperationalLayoutTests))]
	public void AppearancePickerFitsAndLanguageChangesPreserveIds(string language)
	{
		WorkflowUiTest.Run(() =>
		{
			try
			{
				LocalizationManager.Initialize("en-US");
				ThemeManager.Initialize(true);
				using Form host = Host();
				using GeneralSettingsPage page = new() { Dock = DockStyle.Fill };
				host.Controls.Add(page); host.Show(); ThemeManager.Apply(host);
				page.ThemeName = "ocean"; page.ThemeAccent = "rose"; page.ThemeStyle = "square";
				int changes = 0;
				page.AppearanceChanged += (_, _) => changes++;
				LocalizationManager.SetLanguage(language);
				Assert.Equal("ocean", page.ThemeName);
				Assert.Equal("rose", page.ThemeAccent);
				Assert.Equal("square", page.ThemeStyle);
				Assert.Equal(0, changes);
				ModernSettingsCard card = Find<ModernSettingsCard>(page, "settingsCardAppearance");
				foreach (int width in new[] { 600, 1000 })
				{
					host.ClientSize = new Size(width, 760);
					page.ArrangeLocalizedCards(); Application.DoEvents(); page.PerformLayout();
					Assert.True(card.Height > 200);
					foreach (Label label in Descendants(card).OfType<Label>())
					{
						int needed = TextRenderer.MeasureText(label.Text, label.Font, new Size(label.Width, int.MaxValue),
							TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height;
						Assert.True(label.Height >= needed, $"{language}/{width}: {label.Name} needs {needed}, has {label.Height}");
					}
					Assert.True(Find<Button>(card, "appearanceReset").Bottom <= Find<TableLayoutPanel>(card, "appearanceLayout").Height);
					foreach (var choice in Descendants(card).OfType<ComboBox>())
						Assert.True(choice.Width >= 120);
				}
				Find<Button>(card, "appearanceReset").PerformClick();
				Assert.Equal(1, changes);
				Assert.Equal("synix", page.ThemeName);
				Assert.Equal("default", page.ThemeAccent);
				Assert.Equal("rounded", page.ThemeStyle);
			}
			finally { ThemeManager.Initialize(true); LocalizationManager.Initialize("en-US"); }
		});
	}

	[Theory]
	[MemberData(nameof(Themes))]
	public void NewSettingsWindowsUseSelectedAppearance(string theme, bool dark)
	{
		WorkflowUiTest.Run(() =>
		{
			try
			{
				ThemeManager.Initialize(dark, theme, "default", dark ? "rounded" : "square");
				using Form host = Host();
				using GeneralSettingsPage page = new() { Dock = DockStyle.Fill };
				page.DarkMode = dark; page.ThemeName = theme; page.ThemeStyle = ThemeManager.StyleId;
				host.Controls.Add(page); host.Show(); ThemeManager.Apply(host); Application.DoEvents();
				Assert.Equal(SettingsPalette.Window, host.BackColor);
				Assert.Equal(SettingsPalette.Input, Find<ComboBox>(page, "appearanceTheme").BackColor);
				Assert.Equal(SettingsPalette.Card, Find<ModernSettingsCard>(page, "settingsCardAppearance").FillColor);
				using Panel console = new() { BackColor = SettingsPalette.Console };
				host.Controls.Add(console);
				ThemeManager.SetAppearance(dark, "synix", "default", "rounded");
				Assert.Equal(SettingsPalette.Console, console.BackColor);
				Assert.Equal(SettingsPalette.Card, Find<ModernSettingsCard>(page, "settingsCardAppearance").FillColor);
				ThemeManager.SetAppearance(dark, theme, "default", dark ? "rounded" : "square");
				if (Environment.GetEnvironmentVariable("SYNIX_THEME_PREVIEW_DIRECTORY") is { Length: > 0 } directory)
				{
					Directory.CreateDirectory(directory);
					using Bitmap preview = new(page.Width, page.Height);
					page.DrawToBitmap(preview, page.ClientRectangle);
					preview.Save(Path.Combine(directory, $"{theme}-{(dark ? "dark" : "light")}.png"));
				}
			}
			finally { ThemeManager.Initialize(true); }
		});
	}

	private static Form Host() => new()
	{
		ClientSize = new Size(900, 850), BackColor = SettingsPalette.Window,
		FormBorderStyle = FormBorderStyle.None, ShowInTaskbar = false,
		StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000)
	};
	private static T Find<T>(Control root, string name) where T : Control => (T)root.Controls.Find(name, true).Single();
	private static IEnumerable<Control> Descendants(Control root) =>
		root.Controls.Cast<Control>().SelectMany(child => new[] { child }.Concat(Descendants(child)));

	private static void AssertContrast(Color foreground, Color background)
	{
		static double Luminance(Color color)
		{
			static double Linear(byte channel) => channel / 255d <= .04045 ? channel / 255d / 12.92 : Math.Pow((channel / 255d + .055) / 1.055, 2.4);
			return .2126 * Linear(color.R) + .7152 * Linear(color.G) + .0722 * Linear(color.B);
		}
		double first = Luminance(foreground), second = Luminance(background);
		double contrast = (Math.Max(first, second) + .05) / (Math.Min(first, second) + .05);
		Assert.True(contrast >= 4.5, $"{ThemeManager.ThemeId}/{ThemeManager.AccentId}/{ThemeManager.IsDarkMode}: {foreground} on {background} has {contrast:F2}:1 contrast.");
	}
}
