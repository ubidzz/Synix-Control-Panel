using System.Collections;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Synix_Control_Panel.SynixApp.Localization;
using Synix_Control_Panel.SynixApp.UI.Help;
using Synix_Control_Panel.SynixEngine.ModManagement;
using Synix_Control_Panel.SynixEngine;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixApp.Database;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class CompleteLanguageCoverageTests
{
	public static IEnumerable<object[]> Languages() => LocalizationManager.SupportedLanguages
		.Where(language => language.Code != LocalizationManager.DefaultLanguageCode)
		.Select(language => new object[] { language.Code, language.ResourceCultureName });

	private static ResourceManager Resources() => new(
		"Synix_Control_Panel.Localization.Strings", typeof(LocalizationManager).Assembly);

	[Theory]
	[MemberData(nameof(Languages))]
	public void EverySelectableLanguageHasEveryEnglishEntryWithoutParentFallback(string code, string resourceCulture)
	{
		ResourceManager manager = Resources();
		ResourceSet english = manager.GetResourceSet(CultureInfo.InvariantCulture, true, false)!;
		ResourceSet translated = manager.GetResourceSet(CultureInfo.GetCultureInfo(resourceCulture), true, false)!;
		Assert.NotNull(translated);
		Assert.False(LocalizationManager.SupportedLanguages.Single(language => language.Code == code).IsPreview);
		foreach (DictionaryEntry entry in english)
		{
			string key = (string)entry.Key;
			if (key.StartsWith('_')) continue;
			Assert.False(string.IsNullOrWhiteSpace(translated.GetString(key)), $"{code} has no translation for {key}");
		}
	}

	[Fact]
	public void LiteralContentLookupsHaveEnglishResourcesInsteadOfSilentlyFallingBack()
	{
		string project = Core.FindProjectDirectory(AppContext.BaseDirectory)!;
		ResourceSet english = Resources().GetResourceSet(CultureInfo.InvariantCulture, true, false)!;
		Regex literal = new("LocalizationManager\\.(?:GetContent|FormatContent)\\(\\s*\"(?<source>(?:\\\\.|[^\"\\\\])*)\"\\s*[,)]");
		foreach (string path in Directory.EnumerateFiles(project, "*.cs", SearchOption.AllDirectories))
		{
			string relative = Path.GetRelativePath(project, path);
			if (new[] { "Tests", "obj", "bin" }.Any(folder => relative.StartsWith(folder + Path.DirectorySeparatorChar))) continue;
			foreach (Match match in literal.Matches(File.ReadAllText(path)))
			{
				string source = Regex.Unescape(match.Groups["source"].Value).ReplaceLineEndings("\n");
				Assert.True(source == english.GetString(LocalizationManager.ContentKey(source))?.ReplaceLineEndings("\n"),
					$"Missing static content in {relative}: {source}");
			}
		}
	}

	[Fact]
	public void EveryOfflineArticleAndBuiltInSupportDescriptionHasACatalogEntry()
	{
		ResourceSet english = Resources().GetResourceSet(CultureInfo.InvariantCulture, true, false)!;
		List<string> content = [HelpGUI.WelcomeText];
		foreach (var (title, article) in HelpGUI.CreateHelpArticles()) { content.Add(title); content.Add(article.Answer); }
		foreach (var entry in GameModSupportCatalog.Entries) { content.AddRange(entry.Methods); content.Add(entry.Notes); }
		foreach (var entry in GameTextSupportCatalog.Entries)
			content.AddRange(entry.Fields.Select(field => field.Notes).Where(note => note.Length > 0));
		Dictionary<string, string> firstStart = (Dictionary<string, string>)typeof(WarningDatabase)
			.GetField("_messages", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
		content.AddRange(firstStart.Values);
		foreach (var game in GameDatabase.GetGames)
		{
			if (!string.IsNullOrWhiteSpace(game.WarningMessage)) content.Add(game.WarningMessage);
			if (!string.IsNullOrWhiteSpace(game.LaunchFileSetupInstructions)) content.Add(game.LaunchFileSetupInstructions);
		}
		foreach (string text in content)
			Assert.Equal(text.ReplaceLineEndings("\n"), english.GetString(LocalizationManager.ContentKey(text))?.ReplaceLineEndings("\n"));
		foreach (DictionaryEntry entry in english)
			if (((string)entry.Key).StartsWith("Content.", StringComparison.Ordinal))
				Assert.Equal((string)entry.Key, LocalizationManager.ContentKey((string)entry.Value!));
	}

	[Theory]
	[MemberData(nameof(Languages))]
	public void TranslatedFormatTemplatesAreValidAndPreserveEscapedPlaceholders(string code, string resourceCulture)
	{
		ResourceManager manager = Resources();
		ResourceSet english = manager.GetResourceSet(CultureInfo.InvariantCulture, true, false)!;
		ResourceSet translated = manager.GetResourceSet(CultureInfo.GetCultureInfo(resourceCulture), true, false)!;
		Regex formatArgument = new(@"\{\d+(?:,[^}:]+)?(?::[^}]+)?\}");
		Regex escapedPlaceholder = new(@"\{\{[^{}]*\}\}");
		foreach (DictionaryEntry entry in english)
		{
			string source = (string)entry.Value!;
			if (!formatArgument.IsMatch(source)) continue;
			string text = translated.GetString((string)entry.Key)!;
			object[] arguments = Enumerable.Repeat<object>("value", CompositeFormat.Parse(source).MinimumArgumentCount).ToArray();
			Exception? error = Record.Exception(() => { _ = string.Format(CultureInfo.InvariantCulture, text, arguments); });
			Assert.True(error is null, $"{code}: invalid format template {entry.Key}: {error?.Message}");
			Assert.True(escapedPlaceholder.Matches(source).Select(match => match.Value)
				.OrderBy(value => value, StringComparer.Ordinal)
				.SequenceEqual(escapedPlaceholder.Matches(text).Select(match => match.Value)
					.OrderBy(value => value, StringComparer.Ordinal)),
				$"{code}: escaped game placeholder changed in {entry.Key}");
		}
	}

	[Theory]
	[MemberData(nameof(Languages))]
	public void FilePickersTranslateDescriptionsWithoutChangingFilePatterns(string code, string resourceCulture)
	{
		Assert.Equal(resourceCulture, LocalizationManager.SupportedLanguages.Single(language => language.Code == code).ResourceCultureName);
		ResourceManager manager = Resources();
		ResourceSet english = manager.GetResourceSet(CultureInfo.InvariantCulture, true, false)!;
		ResourceSet translated = manager.GetResourceSet(CultureInfo.GetCultureInfo(resourceCulture), true, false)!;
		foreach (DictionaryEntry entry in english)
		{
			string key = (string)entry.Key;
			string[] expected = ((string)entry.Value!).Split('|');
			if (expected.Length < 2 || !expected[1].StartsWith('*')) continue;
			string[] actual = translated.GetString(key)!.Split('|');
			Assert.Equal(expected.Length, actual.Length);
			for (int index = 1; index < expected.Length; index += 2)
				Assert.True(expected[index] == actual[index], $"{code}: file patterns changed for {key}");
			Regex wildcard = new(@"\*\.[\w.*-]+");
			for (int index = 0; index < expected.Length; index += 2)
				Assert.True(wildcard.Matches(expected[index]).Select(match => match.Value)
					.SequenceEqual(wildcard.Matches(actual[index]).Select(match => match.Value)),
					$"{code}: file-filter description names different extensions for {key}");
		}
	}

	[Theory]
	[MemberData(nameof(Languages))]
	public void FirstStartGuidanceRefreshesWithoutChangingTheGameOrInputData(string code, string resourceCulture)
	{
		Assert.Equal(resourceCulture, LocalizationManager.SupportedLanguages.Single(language => language.Code == code).ResourceCultureName);
		WorkflowUiTest.Run(() =>
		{
			try
			{
				LocalizationManager.Initialize("en-US");
				var server = new GameServer { Game = "Minecraft", ServerName = "Save", Port = 25565, QueryPort = 25565 };
				using WarningDatabase window = new(server);
				var guidance = (RichTextBox)window.Controls.Find("txtWarningText", true).Single();
				string english = guidance.Text;
				LocalizationManager.SetLanguage(code);
				Assert.Equal(WarningDatabase.BuildAssistantText(server).ReplaceLineEndings("\n"), guidance.Text.ReplaceLineEndings("\n"));
				Assert.NotEqual(english, guidance.Text);
				Assert.Contains("https://aka.ms/MinecraftEULA", guidance.Text);
				Assert.Equal("Minecraft", window.Controls.Find("lblGameName", true).Single().Text);
				Assert.Equal("Save", server.ServerName);
				if (Environment.GetEnvironmentVariable("SYNIX_RENDER_LANGUAGES") == "1")
				{
					window.StartPosition = FormStartPosition.Manual;
					window.Location = new Point(-32000, -32000);
					window.ShowInTaskbar = false;
					window.Show();
					Application.DoEvents();
					using Bitmap screenshot = new(window.Width, window.Height);
					window.DrawToBitmap(screenshot, new Rectangle(Point.Empty, screenshot.Size));
					screenshot.Save(Path.Combine(AppContext.BaseDirectory, $"first-start-{code}.png"));
				}
			}
			finally { LocalizationManager.Initialize("en-US"); }
		});
	}

	[Theory]
	[MemberData(nameof(Languages))]
	public void HelpArticlesAreTranslatedButCommandsLinksAndPlaceholdersAreUnchanged(string code, string resourceCulture)
	{
		ResourceSet translated = Resources().GetResourceSet(CultureInfo.GetCultureInfo(resourceCulture), true, false)!;
		Regex technical = new(@"https?://[^\s]+|\bwinget (?:install|list|upgrade|uninstall)[^\r\n]+|\bserver\.(?:GenerateAPIToken|InvalidateAPITokens)\b|\{[^{}\r\n]+\}");
		foreach (var (title, article) in HelpGUI.CreateHelpArticles())
		{
			string text = translated.GetString(LocalizationManager.ContentKey(article.Answer))!;
			Assert.False(string.IsNullOrWhiteSpace(text), $"{code}: {title}");
			Assert.NotEqual(article.Answer, text);
			Assert.Equal(article.Answer.Count(c => c == '\n'), text.Count(c => c == '\n'));
			foreach (Match token in technical.Matches(article.Answer))
				Assert.Contains(token.Value, text, StringComparison.Ordinal);
		}
	}

	[Theory]
	[MemberData(nameof(Languages))]
	public void GameGuidanceKeepsExternalToolCommandsAndFolderNames(string code, string resourceCulture)
	{
		ResourceManager manager = Resources();
		ResourceSet english = manager.GetResourceSet(CultureInfo.InvariantCulture, true, false)!;
		ResourceSet translated = manager.GetResourceSet(CultureInfo.GetCultureInfo(resourceCulture), true, false)!;
		Regex technical = new(@"\binitial-setup\b|\bLocal/Console\b|Continue to server configuration|Save & start|\bInstance(?= folder)");
		foreach (DictionaryEntry entry in english)
		{
			if (!((string)entry.Key).StartsWith("Content.", StringComparison.Ordinal)) continue;
			foreach (Match token in technical.Matches((string)entry.Value!))
				Assert.True(translated.GetString((string)entry.Key)!.Contains(token.Value, StringComparison.Ordinal),
					$"{code}: external command or folder changed in {entry.Key}: {token.Value}");
		}
	}

	[Theory]
	[MemberData(nameof(Languages))]
	public void HelpWindowLocalizesArticlesAndSearchAndPreservesSelectionWhenLanguageChanges(string code, string resourceCulture)
	{
		Assert.Equal(resourceCulture, LocalizationManager.SupportedLanguages.Single(language => language.Code == code).ResourceCultureName);
		WorkflowUiTest.Run(() =>
		{
			try
			{
				LocalizationManager.Initialize("en-US");
				using HelpGUI window = new() { ShowInTaskbar = false };
				TreeView tree = (TreeView)window.Controls.Find("treeNavigation", true).Single();
				RichTextBox answer = (RichTextBox)window.Controls.Find("lblAnswer", true).Single();
				Label title = (Label)window.Controls.Find("lblTopicTitle", true).Single();
				TextBox search = (TextBox)window.Controls.Find("txtSearch", true).Single();
				_ = tree.Handle;
				tree.SelectedNode = tree.Nodes[0].Nodes[0];
				string selected = (string)tree.SelectedNode.Tag!;
				LocalizationManager.SetLanguage(code);
				Assert.Equal(selected, tree.SelectedNode!.Tag);
				Assert.Equal(HelpGUI.LocalizedTitle(selected), title.Text);
				Assert.NotEqual(selected, title.Text);
				foreach (TreeNode category in tree.Nodes)
					foreach (TreeNode topic in category.Nodes)
					{
						string key = (string)topic.Tag!;
						tree.SelectedNode = topic;
						Assert.Equal(HelpGUI.LocalizedTitle(key), title.Text);
						Assert.Equal(HelpGUI.LocalizedAnswer(HelpGUI.CreateHelpArticles()[key]).ReplaceLineEndings("\n"), answer.Text.ReplaceLineEndings("\n"));
					}
				search.Text = HelpGUI.LocalizedTitle("First-Time Setup Guide");
				Assert.Contains(tree.Nodes.Cast<TreeNode>().SelectMany(node => node.Nodes.Cast<TreeNode>()),
					node => Equals(node.Tag, "First-Time Setup Guide"));
				search.Text = "server.InvalidateAPITokens";
				Assert.Contains("server.InvalidateAPITokens", answer.Text);
				LocalizationManager.SetLanguage("en-US");
				Assert.Equal("server.InvalidateAPITokens", search.Text);
				Assert.Equal("Satisfactory: Connection Problems and Token Safety", title.Text);
			}
			finally { LocalizationManager.Initialize("en-US"); }
		});
	}

	[Theory]
	[MemberData(nameof(Languages))]
	public void TranslatedHelpHeadingsWrapAtTheSmallestWindowSize(string code, string resourceCulture)
	{
		Assert.Equal(resourceCulture, LocalizationManager.SupportedLanguages.Single(language => language.Code == code).ResourceCultureName);
		WorkflowUiTest.Run(() =>
		{
			try
			{
				LocalizationManager.Initialize(code);
				using HelpGUI window = new() { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual,
					Location = new Point(-32000, -32000) };
				window.Size = window.MinimumSize;
				window.Show();
				Application.DoEvents();
				TreeView tree = (TreeView)window.Controls.Find("treeNavigation", true).Single();
				Label title = (Label)window.Controls.Find("lblTopicTitle", true).Single();
				foreach (TreeNode topic in tree.Nodes.Cast<TreeNode>().SelectMany(category => category.Nodes.Cast<TreeNode>()))
				{
					tree.SelectedNode = topic;
					Application.DoEvents();
					Size text = TextRenderer.MeasureText(title.Text, title.Font,
						new Size(title.Width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
					Assert.True(text.Height <= title.Height, $"{code}: clipped heading {topic.Tag}");
					Assert.True(title.Bottom <= title.Parent!.ClientSize.Height, $"{code}: heading outside header {topic.Tag}");
				}
				Label description = (Label)window.Controls.Find("lblSidebarDescription", true).Single();
				Assert.True(description.Bottom < window.Controls.Find("searchCard", true).Single().Top);
				if (Environment.GetEnvironmentVariable("SYNIX_RENDER_LANGUAGES") == "1")
				{
					tree.SelectedNode = tree.Nodes[0].Nodes[0];
					Application.DoEvents();
					using Bitmap screenshot = new(window.Width, window.Height);
					window.DrawToBitmap(screenshot, new Rectangle(Point.Empty, screenshot.Size));
					screenshot.Save(Path.Combine(AppContext.BaseDirectory, $"help-{code}.png"));
				}
			}
			finally { LocalizationManager.Initialize("en-US"); }
		});
	}

	[Theory]
	[MemberData(nameof(Languages))]
	public void DisplayedUserValuesThatLookLikeInterfaceWordsAreNeverTranslated(string code, string resourceCulture)
	{
		Assert.Equal(resourceCulture, LocalizationManager.SupportedLanguages.Single(language => language.Code == code).ResourceCultureName);
		WorkflowUiTest.Run(() =>
		{
			try
			{
				LocalizationManager.Initialize("en-US");
				using Label value = new();
				LocalizationManager.Apply(value);
				LocalizationManager.SetDataText(value, "Save");
				LocalizationManager.SetLanguage(code);
				LocalizationManager.Apply(value);
				Assert.Equal("Save", value.Text);
				LocalizationManager.SetDataText(value, "No Days Scheduled");
				LocalizationManager.Apply(value);
				Assert.Equal("No Days Scheduled", value.Text);
				LocalizationManager.BindText(value, "ServerSetup.Button.SaveSettings");
				Assert.Equal(LocalizationManager.Get("ServerSetup.Button.SaveSettings"), value.Text);
			}
			finally { LocalizationManager.Initialize("en-US"); }
		});
	}
}
