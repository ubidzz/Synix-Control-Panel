using System.Collections;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;
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
		{ "pl-PL", "Polski" }, { "ru-RU", "Русский" }, { "tr-TR", "Türkçe" },
		{ "nl-NL", "Nederlands" }, { "sv-SE", "Svenska" },
		{ "cs-CZ", "Čeština" }, { "uk-UA", "Українська" },
		{ "id-ID", "Bahasa Indonesia" }, { "vi-VN", "Tiếng Việt" },
		{ "da-DK", "Dansk" }, { "fi-FI", "Suomi" },
		{ "nb-NO", "Norsk bokmål" }, { "el-GR", "Ελληνικά" },
		{ "hu-HU", "Magyar" }, { "ro-RO", "Română" },
		{ "hi-IN", "हिन्दी" }, { "th-TH", "ไทย" },
		{ "bg-BG", "Български" }, { "hr-HR", "Hrvatski" }
	};

	private static ResourceManager CreateManager() => new(
		"Synix_Control_Panel.Localization.Strings", typeof(LocalizationManager).Assembly);

	[Theory]
	[InlineData("bg", "Напред: Преглед", "Запазване на настройките на сървъра", "Външен вид")]
	[InlineData("hr", "Dalje: Pregled", "Spremi postavke poslužitelja", "Izgled")]
	public void AddedLanguagesIncludeReviewAndAppearanceWithoutChangingPreservedPaths(
		string culture, string review, string save, string appearance)
	{
		ResourceSet catalog = CreateManager().GetResourceSet(
			CultureInfo.GetCultureInfo(culture), true, false)!;
		Assert.Equal(review, catalog.GetString("ServerSetup.Button.NextReview"));
		Assert.Equal(save, catalog.GetString("ServerSetup.Button.SaveSettings"));
		Assert.Equal(appearance, catalog.GetString("Appearance.Title"));
		Assert.Contains(@"C:\Synix", catalog.GetString("Uninstall.Keep"));
		Assert.Contains(@"SynixData\servers.json", catalog.GetString("Uninstall.Keep"));
		Assert.NotEqual(catalog.GetString("Uninstall.AppOnly"), catalog.GetString("Uninstall.Everything"));
		Assert.NotEqual(catalog.GetString("Uninstall.BackupsKeep"), catalog.GetString("Uninstall.BackupsDelete"));
		foreach (string key in new[]
		{
			"Configuration.Check.FixConfig", "MinecraftConsole.Quick.Kick.Label",
			"Text.7CB94F75343DF207439A", "DynamicText.B83EC0A78559E32C96D2"
		})
			Assert.NotEqual(CreateManager().GetString(key, CultureInfo.InvariantCulture), catalog.GetString(key));
	}

	[Theory]
	[InlineData("bg")]
	[InlineData("hr")]
	public void AddedLanguagesKeepLiteralWindowsPathsAndConfigurationExamples(string culture)
	{
		ResourceManager manager = CreateManager();
		ResourceSet english = manager.GetResourceSet(CultureInfo.InvariantCulture, true, false)!;
		ResourceSet catalog = manager.GetResourceSet(CultureInfo.GetCultureInfo(culture), true, false)!;
		Regex literal = new(@"\b[A-Za-z]:\\[\w\\.-]+|\b(?:server|app|config)\.[A-Za-z_]\w*");
		foreach (DictionaryEntry entry in english)
		{
			foreach (Match match in literal.Matches((string)entry.Value!))
				Assert.True(catalog.GetString((string)entry.Key)!.Contains(match.Value.TrimEnd('.'), StringComparison.Ordinal),
					$"{culture}: literal path or configuration example changed in {entry.Key}: {match.Value}");
		}
	}

	[Theory]
	[InlineData("nl")]
	[InlineData("sv")]
	[InlineData("cs")]
	[InlineData("uk")]
	[InlineData("id")]
	[InlineData("vi")]
	[InlineData("da")]
	[InlineData("fi")]
	[InlineData("nb")]
	[InlineData("el")]
	[InlineData("hu")]
	[InlineData("ro")]
	[InlineData("hi")]
	[InlineData("th")]
	[InlineData("bg")]
	[InlineData("hr")]
	public void AdditionalLanguagesCoverTheEntireNeutralCatalog(string resourceCulture)
	{
		ResourceManager manager = CreateManager();
		ResourceSet baseline = manager.GetResourceSet(CultureInfo.InvariantCulture, true, false)!;
		ResourceSet catalog = manager.GetResourceSet(CultureInfo.GetCultureInfo(resourceCulture), true, false)!;
		Assert.NotNull(catalog);
		foreach (DictionaryEntry entry in baseline)
		{
			string key = (string)entry.Key;
			if (key.StartsWith("_", StringComparison.Ordinal)) continue;
			Assert.False(string.IsNullOrWhiteSpace(catalog.GetString(key)),
				$"{resourceCulture} is missing core translation {key}");
		}
	}

	[Theory]
	[InlineData("da")]
	[InlineData("fi")]
	[InlineData("nb")]
	[InlineData("el")]
	[InlineData("hu")]
	[InlineData("ro")]
	[InlineData("hi")]
	[InlineData("th")]
	[InlineData("bg")]
	[InlineData("hr")]
	public void NewCatalogsPreserveTheDownloadSpeedUnit(string resourceCulture)
	{
		ResourceSet catalog = CreateManager().GetResourceSet(
			CultureInfo.GetCultureInfo(resourceCulture), true, false)!;
		Assert.Equal("Mbps", catalog.GetString("Text.ECF1D7C868EE46FC6DF0"));
	}

	[Theory]
	[MemberData(nameof(NewLanguages))]
	public void CompleteCatalogsAreDiscoveredWithNativeNames(string code, string name)
	{
		var language = Assert.Single(LocalizationManager.SupportedLanguages, item => item.Code == code);
		Assert.Equal(name, language.DisplayName);
		Assert.False(language.IsPreview);
		Assert.True(string.IsNullOrWhiteSpace(language.PreviewNotice));
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
			// "Backups" is normal Portuguese and "General" is normal Romanian;
			// valid shared words must not be changed just to differ from English.
			if (key != "MinecraftWorkspace.Backups" &&
				!(code == "ro-RO" && key == "SettingsPage.General.Heading"))
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
	[InlineData("nl", "nl-NL")]
	[InlineData("nl-BE", "nl-NL")]
	[InlineData("sv", "sv-SE")]
	[InlineData("sv-FI", "sv-SE")]
	[InlineData("cs", "cs-CZ")]
	[InlineData("cs-CZ", "cs-CZ")]
	[InlineData("uk", "uk-UA")]
	[InlineData("uk-UA", "uk-UA")]
	[InlineData("id", "id-ID")]
	[InlineData("id-ID", "id-ID")]
	[InlineData("vi", "vi-VN")]
	[InlineData("VI-vn", "vi-VN")]
	[InlineData("da", "da-DK")]
	[InlineData("da-DK", "da-DK")]
	[InlineData("fi", "fi-FI")]
	[InlineData("fi-FI", "fi-FI")]
	[InlineData("nb", "nb-NO")]
	[InlineData("nb-NO", "nb-NO")]
	[InlineData("no", "nb-NO")]
	[InlineData("no-NO", "nb-NO")]
	[InlineData("nn-NO", "en-US")]
	[InlineData("el", "el-GR")]
	[InlineData("el-CY", "el-GR")]
	[InlineData("hu", "hu-HU")]
	[InlineData("hu-HU", "hu-HU")]
	[InlineData("ro", "ro-RO")]
	[InlineData("ro-MD", "ro-RO")]
	[InlineData("hi", "hi-IN")]
	[InlineData("hi-IN", "hi-IN")]
	[InlineData("th", "th-TH")]
	[InlineData("TH-th", "th-TH")]
	[InlineData("bg", "bg-BG")]
	[InlineData("bg-BG", "bg-BG")]
	[InlineData("hr", "hr-HR")]
	[InlineData("hr-BA", "hr-HR")]
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
				Assert.NotEqual(LocalizationManager.GetEnglish("Configuration.Editor.Error.LineBreak", "example"),
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
	public void LanguagePickerPreservesSelectionAndShowsDescriptionWithoutClipping(string code, string name)
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
				Assert.Equal(LocalizationManager.Get("Language.Description"), notice.Text);
				Assert.True(TextRenderer.MeasureText(name, picker.Font).Width <= picker.Width - 44,
					$"Language name clips: {name}");
				Size noticeSize = TextRenderer.MeasureText(notice.Text, notice.Font,
					new Size(notice.Width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
				Assert.True(noticeSize.Height <= notice.Height, $"Language description clips: {code}");

				if (Environment.GetEnvironmentVariable("SYNIX_RENDER_LANGUAGES") == "1")
				{
					using Bitmap image = new(host.Width, host.Height);
					host.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
					image.Save(Path.Combine(AppContext.BaseDirectory, $"language-{code}.png"));
				}
				page.UiLanguageCode = "fr-FR";
				LocalizationManager.SetLanguage("fr-FR");
				LocalizationManager.Apply(host);
				Assert.Equal(LocalizationManager.Get("Language.Description"), notice.Text);
				page.UiLanguageCode = "zh-TW";
				Assert.Equal("zh-Hant", page.UiLanguageCode);
			}
			finally { LocalizationManager.Initialize("en-US"); }
		});
	}

	[Fact]
	public void CatalogsHaveUniqueKeysAndEveryEntryHasAnEnglishSource()
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
			foreach (var entry in entries.Where(data => !((string)data.Attribute("name")!).StartsWith("_")))
				Assert.Contains((string)entry.Attribute("name")!, englishKeys);
		}
		Assert.All(LocalizationManager.SupportedLanguages.Where(language =>
			language.Code is "en-US" or "fr-FR" or "de-DE" or "es-ES"), language => Assert.False(language.IsPreview));
	}
}
