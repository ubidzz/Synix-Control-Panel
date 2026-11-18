using System.Drawing;
using System.Windows.Forms;
using Synix_Control_Panel.SynixApp.Localization;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class LocalizedFontTests
{
	[Theory]
	[InlineData("tzm-Tfng", "Ebrima", 1F)]
	[InlineData("ff-Adlm", "Ebrima", 1F)]
	[InlineData("bo-CN", "Microsoft Himalaya", 1.5F)]
	[InlineData("dz-BT", "Microsoft Himalaya", 1.5F)]
	public void ScriptFontsSurviveThemeChangesAndRestoreWithoutChangingData(string code, string family, float scale)
	{
		WorkflowUiTest.Run(() =>
		{
			try
			{
				LocalizationManager.Initialize("en-US");
				using Font original = new("Segoe UI", 10F, FontStyle.Bold);
				using Font changed = new("Segoe UI", 11F, FontStyle.Italic);
				using Font dataFont = new("Consolas", 9F);
				using Form form = new();
				using Label caption = new() { Font = original, Text = "Save Changes" };
				using TextBox command = new() { Font = dataFont, Text = @"C:\Synix\server.exe --port 7777" };
				using GroupBox group = new() { Font = original, Text = "Settings" };
				using Label inherited = new() { Text = "Save Changes" };
				using DataGridView grid = new() { Font = original };
				grid.ColumnHeadersDefaultCellStyle.Font = original;
				grid.Columns.Add("name", "Server Name");
				grid.Columns[0].DefaultCellStyle.Font = original;
				using ContextMenuStrip menu = new();
				using ToolStripMenuItem action = new("Save Changes") { Font = original };
				group.Controls.Add(inherited);
				menu.Items.Add(action);
				caption.ContextMenuStrip = menu;
				form.Controls.AddRange([caption, command, group, grid]);
				LocalizationManager.Apply(form);
				LocalizationManager.SetLanguage(code);
				for (int i = 0; i < 3; i++) LocalizationManager.Apply(form);
				Assert.Equal(family, caption.Font.Name);
				Assert.Equal(10F * scale, caption.Font.Size);
				Assert.Equal(10F * scale, inherited.Font.Size);
				Assert.Equal(family, action.Font.Name);
				Assert.Equal(family, grid.ColumnHeadersDefaultCellStyle.Font!.Name);
				Assert.Equal(family, grid.Columns[0].DefaultCellStyle.Font!.Name);
				Assert.Same(dataFont, command.Font);
				Assert.Equal(@"C:\Synix\server.exe --port 7777", command.Text);
				caption.Font = changed;
				Assert.Equal(family, caption.Font.Name);
				Assert.Equal(11F * scale, caption.Font.Size);
				Assert.Equal(FontStyle.Italic, caption.Font.Style);
				LocalizationManager.SetLanguage("en-US");
				LocalizationManager.Apply(form);
				Assert.Same(changed, caption.Font);
				Assert.Same(original, group.Font);
				Assert.Same(original, inherited.Font);
				Assert.Same(original, action.Font);
				Assert.Same(original, grid.ColumnHeadersDefaultCellStyle.Font);
				Assert.Same(original, grid.Columns[0].DefaultCellStyle.Font);
			}
			finally { LocalizationManager.Initialize("en-US"); }
		});
	}

	[Theory]
	[InlineData("tzm-Tfng")]
	[InlineData("ff-Adlm")]
	[InlineData("bo-CN")]
	[InlineData("dz-BT")]
	public void ScriptFontsKeepSettingsTextAndSelectorsReadable(string code)
	{
		var language = LocalizationManager.SupportedLanguages.Single(language => language.Code == code);
		new AdditionalLanguageTests().LanguagePickerPreservesSelectionAndShowsDescriptionWithoutClipping(code, language.DisplayName);
	}
}
