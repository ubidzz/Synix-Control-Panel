using System.Collections;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Windows.Forms;
using System.Xml.Linq;
using Synix_Control_Panel.SynixApp.Localization;
using Synix_Control_Panel.SynixEngine;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class AdditionalLanguageTests
{
	public static TheoryData<string, string> NewLanguages => new()
	{
		{ "zh-Hans", "简体中文" }, { "zh-Hant", "繁體中文" },
		{ "ja-JP", "日本語" }, { "ko-KR", "한국어" },
		{ "it-IT", "Italiano" }, { "pt-BR", "Português (Brasil)" },
		{ "pl-PL", "Polski" }, { "ru-RU", "Русский" }, { "tr-TR", "Türkçe" }
	};

	private static ResourceManager CreateManager() => new(
		"Synix_Control_Panel.Localization.Strings", typeof(LocalizationManager).Assembly);

	[Theory]
	[MemberData(nameof(NewLanguages))]
	public void NewCatalogsAreDiscoveredWithNativeNamesAndAnHonestPreviewNotice(string code, string name)
	{
		var language = Assert.Single(LocalizationManager.SupportedLanguages, item => item.Code == code);
		Assert.Equal(name, language.DisplayName);
		Assert.True(language.IsPreview);
		Assert.False(string.IsNullOrWhiteSpace(language.PreviewNotice));
		using Stream manifest = typeof(LocalizationManager).Assembly.GetManifestResourceStream(
			"Synix_Control_Panel.Localization.SupportedLanguages.txt")!;
		using StreamReader reader = new(manifest);
		Assert.Contains("Strings." + language.ResourceCultureName, reader.ReadToEnd());

		ResourceSet catalog = CreateManager().GetResourceSet(
			CultureInfo.GetCultureInfo(language.ResourceCultureName), true, false)!;
		Assert.NotNull(catalog);
		Assert.True(catalog.Cast<DictionaryEntry>().Count() >= 450);
		foreach (string key in new[]
		{
			"Language.PreviewNotice", "Text.A4FE65264EF7DBB38D10",
			"SettingsPage.General.Heading", "ServerSetup.Button.NextReview",
			"ServerSetup.Button.SaveSettings", "ServerSetup.Review.Description",
			"ServerSetup.Review.Notice", "ServerSetup.Validation.ReviewRequired",
			"ServerSetup.Page.Security.Title", "ServerSetup.Progress.Details",
			"ServerSetup.Progress.Required", "ServerSetup.Progress.Review",
			"ServerSetup.Progress.Save", "AddServer.Title", "AddServer.Import.Description",
			"ModManager.Button.InstallFile", "ModManager.Safety.StopFirst",
			"ModManager.Safety.RestartRequired", "MinecraftWorkspace.Settings",
			"MinecraftWorkspace.Backups", "MinecraftWorkspace.Recovery",
			"Dashboard.NoServerSelected.Body", "Status.Running", "Status.Stopped"
		})
		{
			string? value = catalog.GetString(key);
			Assert.False(string.IsNullOrWhiteSpace(value), $"{code} is missing {key}");
			// "Backups" is also normal Portuguese; borrowed words need not differ.
			if (key != "MinecraftWorkspace.Backups")
				Assert.NotEqual(CreateManager().GetString(key, CultureInfo.InvariantCulture), value);
		}
	}

	[Theory]
	[InlineData("zh", "zh-Hans")]
	[InlineData("zh-Hans", "zh-Hans")]
	[InlineData("zh-Hant", "zh-Hant")]
	[InlineData("zh-CN", "zh-Hans")]
	[InlineData("zh-SG", "zh-Hans")]
	[InlineData("zh-TW", "zh-Hant")]
	[InlineData("zh-HK", "zh-Hant")]
	[InlineData("zh-MO", "zh-Hant")]
	[InlineData("zh-Hant-HK", "zh-Hant")]
	[InlineData("zh-Hans-CN", "zh-Hans")]
	[InlineData("zh-CHT", "zh-Hant")]
	[InlineData("zh-CHS", "zh-Hans")]
	[InlineData("ZH-hAnt", "zh-Hant")]
	[InlineData("ja", "ja-JP")]
	[InlineData("ko", "ko-KR")]
	[InlineData("pt", "pt-BR")]
	[InlineData("pl", "pl-PL")]
	[InlineData("ru", "ru-RU")]
	[InlineData("tr", "tr-TR")]
	[InlineData("it", "it-IT")]
	[InlineData("fr-CA", "fr-FR")]
	[InlineData("", "en-US")]
	[InlineData("unsupported-language", "en-US")]
	public void LanguageAliasesResolveWithoutMixingChineseWritingSystems(string requested, string expected)
	{
		Assert.Equal(expected, LocalizationManager.NormalizeLanguageCode(requested));
		try
		{
			LocalizationManager.Initialize(requested);
			Assert.Equal(expected, LocalizationManager.CurrentLanguageCode);
			if (expected is "zh-Hans" or "zh-Hant")
				Assert.Equal(expected == "zh-Hans" ? "保存服务器设置" : "儲存伺服器設定",
					LocalizationManager.Get("ServerSetup.Button.SaveSettings"));
		}
		finally { LocalizationManager.Initialize(LocalizationManager.DefaultLanguageCode); }
	}

	[Theory]
	[MemberData(nameof(NewLanguages))]
	public void LiveLanguageSwitchKeepsInputsAndMachineValuesAndRestoresEnglish(string code, string name)
	{
		Assert.Equal(name, LocalizationManager.SupportedLanguages.Single(language => language.Code == code).DisplayName);
		WorkflowUiTest.Run(() =>
		{
			CultureInfo original = CultureInfo.CurrentCulture;
			try
			{
				CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
				LocalizationManager.Initialize("en-US");
				using Form form = new();
				using Button button = new();
				using TextBox password = new() { Text = "Save_密码_密碼_123" };
				using TextBox command = new() { Text = "say Server maintenance in 5 minutes" };
				using TextBox path = new() { Text = @"C:\Synix\Games\世界\server.properties" };
				form.Controls.AddRange([button, password, command, path]);
				LocalizationManager.BindText(button, "ServerSetup.Button.SaveSettings");
				LocalizationManager.SetLanguage(code);
				LocalizationManager.Apply(form);
				Assert.Equal(LocalizationManager.Get("ServerSetup.Button.SaveSettings"), button.Text);
				Assert.NotEqual("Save Server Settings", button.Text);
				Assert.Equal("Save_密码_密碼_123", password.Text);
				Assert.Equal("say Server maintenance in 5 minutes", command.Text);
				Assert.Equal(@"C:\Synix\Games\世界\server.properties", path.Text);
				Assert.Equal("en-US", CultureInfo.CurrentCulture.Name);
				Assert.Equal("Save Server Settings", LocalizationManager.GetEnglish("ServerSetup.Button.SaveSettings"));
				Assert.Equal("Server Installation", new LocalizedOption(
					"Server Installation", "ProblemAction.ServerInstallation").Value);
				Assert.Equal(LocalizationManager.GetEnglish("Configuration.Editor.Error.LineBreak", "example"),
					LocalizationManager.Get("Configuration.Editor.Error.LineBreak", "example"));
				LocalizationManager.SetLanguage("en-US");
				LocalizationManager.Apply(form);
				Assert.Equal("Save Server Settings", button.Text);
			}
			finally
			{
				CultureInfo.CurrentCulture = original;
				LocalizationManager.Initialize("en-US");
			}
		});
	}

	[Theory]
	[MemberData(nameof(NewLanguages))]
	public void LanguagePickerPreservesSelectionAndShowsPreviewWithoutClipping(string code, string name)
	{
		WorkflowUiTest.Run(() =>
		{
			try
			{
				LocalizationManager.Initialize("en-US");
				using Form host = new()
				{
					ClientSize = new Size(850, 550), StartPosition = FormStartPosition.Manual,
					Location = new Point(-32000, -32000), ShowInTaskbar = false
				};
				using GeneralSettingsPage page = new() { Dock = DockStyle.Fill };
				host.Controls.Add(page);
				host.Show();
				page.UiLanguageCode = code;
				LocalizationManager.SetLanguage(code);
				LocalizationManager.Apply(host);
				Application.DoEvents();
				Assert.Equal(code, page.UiLanguageCode);
				var picker = (ComboBox)page.Controls.Find("cmbLanguage", true).Single();
				Assert.Equal(name, picker.SelectedItem!.ToString());
				Assert.Equal(LocalizationManager.SupportedLanguages.Count, picker.Items.Count);
				Assert.Contains(picker.Items.Cast<LocalizedOption>(), option =>
					option.Value == "en-US" && option.ToString() == "English");
				var downloadMode = (ComboBox)page.Controls.Find("cmbSteamCmdDownloadMode", true).Single();
				foreach (object option in downloadMode.Items)
					Assert.True(TextRenderer.MeasureText(option.ToString(), downloadMode.Font).Width <= downloadMode.Width - 44,
						$"Download mode clips: {code} / {option}");
				var downloadTitle = (Label)page.Controls.Find("lblTitleSteamDownloads", true).Single();
				Assert.True(TextRenderer.MeasureText(downloadTitle.Text, downloadTitle.Font).Width <= downloadTitle.Width,
					$"Download title clips: {code}");
				var downloadDescription = (Label)page.Controls.Find("lblDescriptionSteamDownloads", true).Single();
				Assert.True(TextRenderer.MeasureText(downloadDescription.Text, downloadDescription.Font,
					new Size(downloadDescription.Width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height
					<= downloadDescription.Height, $"Download description clips: {code}");
				var notice = (Label)page.Controls.Find("lblDescriptionLanguage", true).Single();
				Assert.Equal(LocalizationManager.Get("Language.PreviewNotice"), notice.Text);
				Assert.True(TextRenderer.MeasureText(name, picker.Font).Width <= picker.Width - 44,
					$"Language name clips: {name}");
				Size noticeSize = TextRenderer.MeasureText(notice.Text, notice.Font,
					new Size(notice.Width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
				Assert.True(noticeSize.Height <= notice.Height, $"Preview notice clips: {code}");

				if (Environment.GetEnvironmentVariable("SYNIX_RENDER_LANGUAGES") == "1")
				{
					using Bitmap image = new(host.Width, host.Height);
					host.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
					image.Save(Path.Combine(AppContext.BaseDirectory, $"language-{code}.png"));
				}
				page.UiLanguageCode = "fr-FR";
				LocalizationManager.SetLanguage("fr-FR");
				LocalizationManager.Apply(host);
				Assert.Equal(LocalizationManager.Get("Text.30F3BBCB43AC48EBFD1C"), notice.Text);
				page.UiLanguageCode = "zh-TW";
				Assert.Equal("zh-Hant", page.UiLanguageCode);
			}
			finally { LocalizationManager.Initialize("en-US"); }
		});
	}

	[Fact]
	public void CatalogsHaveUniqueKeysAndEveryPreviewEntryHasAnEnglishSource()
	{
		string project = Core.FindProjectDirectory(AppContext.BaseDirectory)!;
		string directory = Path.Combine(project, "Localization");
		HashSet<string> englishKeys = XDocument.Load(Path.Combine(directory, "Strings.resx"))
			.Root!.Elements("data").Select(data => (string)data.Attribute("name")!).ToHashSet();
		foreach (var language in LocalizationManager.SupportedLanguages)
		{
			string suffix = language.ResourceCultureName.Length == 0 ? "" : "." + language.ResourceCultureName;
			var entries = XDocument.Load(Path.Combine(directory, "Strings" + suffix + ".resx"))
				.Root!.Elements("data").ToArray();
			Assert.Equal(entries.Length, entries.Select(data => (string)data.Attribute("name")!).Distinct().Count());
			if (!language.IsPreview) continue;
			foreach (var entry in entries.Where(data => !((string)data.Attribute("name")!).StartsWith("_")))
				Assert.Contains((string)entry.Attribute("name")!, englishKeys);
		}
		Assert.All(LocalizationManager.SupportedLanguages.Where(language =>
			language.Code is "en-US" or "fr-FR" or "de-DE" or "es-ES"), language => Assert.False(language.IsPreview));
	}
}
