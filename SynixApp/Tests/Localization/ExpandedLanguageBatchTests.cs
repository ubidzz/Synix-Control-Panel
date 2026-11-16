using System.Collections;
using System.Globalization;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using Synix_Control_Panel.SynixApp.Localization;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class ExpandedLanguageBatchTests
{
	public static TheoryData<string> Catalogs { get; } = new()
	{
		"af", "ak", "am", "ar", "as", "ast", "az-Cyrl", "az-Latn", "ba", "be",
		"bem", "bho", "bm", "bn", "bo", "bs-Cyrl", "bs-Latn", "ceb", "ckb", "co",
		"cv", "cy", "dv", "dz", "ee", "eo", "eu", "fa", "ff-Adlm", "ff-Latn",
		"fo", "fur", "fy", "ga", "gd", "gl", "gn", "gsw", "gu", "ha",
		"haw", "he", "hi-Latn", "hy", "ig", "is", "jv", "ka", "kab", "kam",
		"kea", "ki", "kk", "km", "kn", "kr-Latn", "ks-Arab", "ks-Deva", "ky", "lb",
		"lg", "ln", "lo", "luo", "mai", "mg", "mi", "mk", "ml", "mn",
		"mni-Beng", "mr", "mt", "my", "ne", "nn", "nso", "nus", "oc", "om",
		"or", "pa-Guru", "pap", "ps", "rn", "rw", "sa", "sc", "sd-Arab", "sg",
		"si", "sn", "so", "sq", "sr-Cyrl", "sr-Latn", "ss", "st", "su-Latn", "sw",
		"ta", "te", "tg", "ti", "tk", "tn", "ts", "tt", "tzm-Tfng", "ug",
		"ur", "uz-Cyrl", "uz-Latn", "wo", "xh", "yi", "yo", "zu"
	};

	[Fact]
	public void AtLeastOneHundredFiftyCompleteCatalogsAreAvailableWithoutCountingRegionalAliases()
	{
		Assert.True(LocalizationManager.SupportedLanguages.Count >= 150);
		Assert.Equal(LocalizationManager.SupportedLanguages.Count,
			LocalizationManager.SupportedLanguages.Select(language => language.ResourceCultureName)
				.Distinct(StringComparer.OrdinalIgnoreCase).Count());
	}

	[Theory]
	[InlineData("az-AZ", "az-Latn")]
	[InlineData("az-Latn-AZ", "az-Latn")]
	[InlineData("bs-BA", "bs-Latn")]
	[InlineData("bs-Latn-BA", "bs-Latn")]
	[InlineData("sr-Cyrl", "sr-Cyrl")]
	[InlineData("sr-Cyrl-RS", "sr-Cyrl")]
	[InlineData("az-Cyrl-AZ", "az-Cyrl")]
	[InlineData("bs-Cyrl-BA", "bs-Cyrl")]
	[InlineData("uz-Cyrl-UZ", "uz-Cyrl")]
	[InlineData("az-Arab-AZ", "en-US")]
	[InlineData("bs-Arab-BA", "en-US")]
	[InlineData("sr-Latn-RS", "sr-Latn")]
	[InlineData("sr-RS", "sr-Latn")]
	public void ScriptSpecificCatalogsResolveOnlyToTheirOwnWritingSystem(string requested, string expected)
	{
		Assert.Equal(expected, LocalizationManager.NormalizeLanguageCode(requested));
	}

	[Theory]
	[MemberData(nameof(Catalogs))]
	public void NewCatalogsPreserveCommandsPlaceholdersAndDiagnosticSymbols(string culture)
	{
		ResourceManager resources = new("Synix_Control_Panel.Localization.Strings", typeof(LocalizationManager).Assembly);
		ResourceSet source = resources.GetResourceSet(CultureInfo.InvariantCulture, true, false)!;
		ResourceSet translated = resources.GetResourceSet(CultureInfo.GetCultureInfo(culture), true, false)!;
		Regex tokens = new(@"\{\{[^{}]*\}\}|\{[^{}\r\n]*\}|https?://[^\s<>]+|\b(?:server|app|config)\.[A-Za-z_]\w*|\*\.[\w.*-]+");
		Regex paths = new(@"\b[A-Za-z]:\\[\w\\.*?{}:/-]+");
		Regex artifact = new(@"ZXQ|QXZ|<2[a-z-]+>|@\s*(?:item|info|label|title|option|action|context)\s*:");
		int[] Symbols(string value) => value.EnumerateRunes()
			.Where(rune => rune.Value is >= 0x2600 and <= 0x27BF or >= 0x1F000 and <= 0x1FAFF)
			.Select(rune => rune.Value).Order().ToArray();
		foreach (DictionaryEntry entry in source)
		{
			string key = (string)entry.Key;
			string english = (string)entry.Value!;
			string text = translated.GetString(key)!;
			Assert.False(string.IsNullOrWhiteSpace(text), $"{culture}: missing {key}");
			Assert.False(artifact.IsMatch(text), $"{culture}: translation artifact in {key}");
			Assert.True(Symbols(english).SequenceEqual(Symbols(text)), $"{culture}: diagnostic symbols changed in {key}");
			Assert.True(tokens.Matches(english).Select(match => match.Value).Order(StringComparer.Ordinal)
				.SequenceEqual(tokens.Matches(text).Select(match => match.Value).Order(StringComparer.Ordinal)),
				$"{culture}: protected syntax changed in {key}");
			// Sentence punctuation and grammatical suffixes around a displayed
			// path may vary. The original path itself must remain verbatim.
			foreach (Match path in paths.Matches(english))
				Assert.Contains(path.Value, text, StringComparison.Ordinal);
			string[] filter = english.Split('|');
			if (filter.Length > 1 && filter.Length % 2 == 0 &&
				(filter[1].StartsWith('*') || filter[1].StartsWith('{')))
			{
				string[] localizedFilter = text.Split('|');
				Assert.Equal(filter.Length, localizedFilter.Length);
				for (int index = 1; index < filter.Length; index += 2)
					Assert.Equal(filter[index], localizedFilter[index]);
			}
			Assert.Equal(english.Count(character => character == '\n'), text.Count(character => character == '\n'));
		}
		foreach (string key in new[] { "RemoteManager.Description", "RemoteManager.Instructions", "RemoteManager.RecoveryInstructions",
			"RemoteManager.WaitingApproval", "Uninstall.AppOnly", "Uninstall.Everything", "Uninstall.Review", "Uninstall.Confirm" })
			Assert.NotEqual(source.GetString(key), translated.GetString(key));
		Assert.NotEqual(translated.GetString("Uninstall.AppOnly"), translated.GetString("Uninstall.Everything"));
		Assert.NotEqual(translated.GetString("Uninstall.BackupsKeep"), translated.GetString("Uninstall.BackupsDelete"));
		Assert.Equal("Mbps", translated.GetString("Text.ECF1D7C868EE46FC6DF0"));
		foreach (string key in new[] { "Size.Unit.Bytes", "Size.Unit.Kilobytes", "Size.Unit.Megabytes", "Size.Unit.Gigabytes", "Size.Unit.Terabytes" })
			Assert.Equal(source.GetString(key), translated.GetString(key));
		Assert.Contains("10", translated.GetString("RemoteManager.RecoveryInstructions"));
		Assert.Contains("2FA", translated.GetString("RemoteManager.RecoveryInstructions"));
	}
}
