using System.Globalization;
using System.Windows.Forms;
using Synix_Control_Panel.SynixApp.Localization;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class RightToLeftLanguageTests
{
	public static IEnumerable<object[]> Languages => LocalizationManager.SupportedLanguages
		.Where(language => CultureInfo.GetCultureInfo(language.ResourceCultureName).TextInfo.IsRightToLeft)
		.Select(language => new object[] { language.Code });

	[Theory]
	[MemberData(nameof(Languages))]
	public void LanguageSwitchChangesReadingDirectionWithoutChangingTechnicalData(string language)
	{
		WorkflowUiTest.Run(() =>
		{
			CultureInfo formattingCulture = CultureInfo.CurrentCulture;
			try
			{
				LocalizationManager.Initialize("en-US");
				using Form form = new();
				using Panel scroll = new() { AutoScroll = true, Width = 450, Height = 200 };
				using Label caption = new() { Text = "Save Changes", Left = 20, Width = 150 };
				using Label serverName = new();
				using TextBox command = new() { Text = @"C:\Synix\العالم\Server.exe --port 7777", Top = 40 };
				using TextBox password = new() { Text = "Save!123", UseSystemPasswordChar = true, Top = 80 };
				using RichTextBox console = new() { Text = "[INFO] 127.0.0.1:7777", Top = 120 };
				using RichTextBox article = new() { ReadOnly = true, Text = "Article", Top = 160 };
				form.Controls.Add(scroll);
				scroll.Controls.AddRange([caption, serverName, command, password, console, article]);
				LocalizationManager.SetDataText(serverName, "Save Changes");
				LocalizationManager.UseInterfaceTextDirection(article);
				LocalizationManager.Apply(form);
				var bounds = caption.Bounds;
				LocalizationManager.SetLanguage(language);
				LocalizationManager.Apply(form);
				Assert.True(LocalizationManager.IsRightToLeft);
				Assert.Equal(RightToLeft.Yes, caption.RightToLeft);
				Assert.Equal(RightToLeft.Yes, article.RightToLeft);
				Assert.Equal(RightToLeft.No, scroll.RightToLeft);
				Assert.Equal(bounds, caption.Bounds);
				Assert.NotEqual("Save Changes", caption.Text);
				Assert.Equal("Save Changes", serverName.Text);
				foreach (Control technical in new Control[] { serverName, command, password, console })
					Assert.Equal(RightToLeft.No, technical.RightToLeft);
				Assert.Equal(@"C:\Synix\العالم\Server.exe --port 7777", command.Text);
				Assert.Equal("Save!123", password.Text);
				Assert.Equal("[INFO] 127.0.0.1:7777", console.Text);
				Assert.Same(formattingCulture, CultureInfo.CurrentCulture);
				Assert.Equal(MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign,
					LocalizedMessageBox.WithReadingDirection(0));

				using Label added = new() { Text = "Save Changes" };
				scroll.Controls.Add(added);
				Assert.Equal(RightToLeft.Yes, added.RightToLeft);
				Assert.Equal(caption.Text, added.Text);
				LocalizationManager.SetLanguage("en-US");
				LocalizationManager.Apply(form);
				Assert.Equal("Save Changes", caption.Text);
				Assert.Equal(RightToLeft.No, caption.RightToLeft);
				Assert.Equal(RightToLeft.No, article.RightToLeft);
				Assert.Equal((MessageBoxOptions)0, LocalizedMessageBox.WithReadingDirection(0));
			}
			finally { LocalizationManager.Initialize("en-US"); }
		});
	}

	[Fact]
	public void AlreadyTranslatedControlsFollowLaterLanguageSwitches()
	{
		WorkflowUiTest.Run(() =>
		{
			try
			{
				LocalizationManager.Initialize("fr-FR");
				using Label caption = new() { Text = LocalizationManager.Get("Text.1509F561F2416598629B") };
				LocalizationManager.Apply(caption);
				LocalizationManager.SetLanguage("ar");
				LocalizationManager.Apply(caption);
				Assert.Equal(LocalizationManager.Get("Text.1509F561F2416598629B"), caption.Text);
				LocalizationManager.SetLanguage("de-DE");
				LocalizationManager.Apply(caption);
				Assert.Equal("Speichern", caption.Text);
			}
			finally { LocalizationManager.Initialize("en-US"); }
		});
	}
}
