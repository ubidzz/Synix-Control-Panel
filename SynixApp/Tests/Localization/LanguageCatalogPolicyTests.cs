using System.Globalization;
using Synix_Control_Panel.SynixApp.Localization;
using Xunit;
using static Synix_Control_Panel.SynixApp.Localization.LanguageCatalogPolicy;

namespace Synix_Control_Panel.Tests;

public sealed class LanguageCatalogPolicyTests
{
	private static Dictionary<string, string> English() => new(StringComparer.Ordinal)
	{
		["Save"] = "Save", ["Cancel"] = "Cancel", ["Command"] = "app_update",
		["_Comment"] = "Metadata is not interface text."
	};

	private static Dictionary<string, string> Translation() => new(StringComparer.Ordinal)
	{
		["Save"] = "Enregistrer", ["Cancel"] = "Annuler", ["Command"] = "app_update"
	};

	[Fact]
	public void CompleteCatalogRetainsUntranslatedTechnicalData()
	{
		CatalogCoverage result = CheckCatalog(English(), Translation());
		Assert.Equal(CatalogState.Complete, result.State);
		Assert.Equal(3, result.RequiredCount);
		Assert.Empty(result.MissingKeys);
		Assert.Empty(result.UnexpectedKeys);
	}

	[Fact]
	public void MissingCatalogDoesNotBorrowEnglishEntries()
	{
		CatalogCoverage result = CheckCatalog(English(), null);
		Assert.Equal(CatalogState.Missing, result.State);
		Assert.Equal(3, result.MissingKeys.Count);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData(" \r\n\t")]
	public void MissingOrBlankEntryIsNotComplete(string? value)
	{
		Dictionary<string, string> catalog = Translation();
		if (value == null) catalog.Remove("Cancel"); else catalog["Cancel"] = value;
		CatalogCoverage result = CheckCatalog(English(), catalog);
		Assert.Equal(CatalogState.Incomplete, result.State);
		Assert.Equal(["Cancel"], result.MissingKeys);
	}

	[Fact]
	public void StaleKeysAndEmptySourceAreNotComplete()
	{
		Dictionary<string, string> catalog = Translation();
		catalog["OldText"] = "Old translation";
		CatalogCoverage result = CheckCatalog(English(), catalog);
		Assert.Equal(CatalogState.Incomplete, result.State);
		Assert.Equal(["OldText"], result.UnexpectedKeys);
		Assert.Equal(CatalogState.Incomplete, CheckCatalog(new Dictionary<string, string>(), new Dictionary<string, string>()).State);
	}

	[Theory]
	[InlineData("true")]
	[InlineData(" TRUE ")]
	public void PreviewFlagHidesEvenAFullCatalog(string value)
	{
		Dictionary<string, string> catalog = Translation();
		catalog["_Language.IsPreview"] = value;
		Assert.Equal(CatalogState.Preview, CheckCatalog(English(), catalog).State);
	}

	[Fact]
	public void EnglishCopyIsNotClaimedAsATranslation()
	{
		Dictionary<string, string> catalog = English();
		catalog["Cancel"] += " \r\n";
		Assert.Equal(CatalogState.EnglishCopy, CheckCatalog(English(), catalog).State);
		Assert.Equal(CatalogState.Complete, CheckCatalog(English(), catalog, isEnglish: true).State);
	}

	[Theory]
	[InlineData("sr-Latn-RS", "sr-Latn", "sr-Cyrl")]
	[InlineData("sr-Cyrl-RS", "sr-Cyrl", "sr-Latn")]
	[InlineData("az-Cyrl-AZ", "az-Cyrl", "az-Latn")]
	[InlineData("uz-Arab-AF", "uz-Arab", "uz-Latn")]
	[InlineData("pa-Arab-PK", "pa-Arab", "pa-Guru")]
	[InlineData("mn-Mong-CN", "mn-Mong", "mn")]
	[InlineData("hi-Latn-IN", "hi-Latn", "hi")]
	[InlineData("ff-Adlm-GN", "ff-Adlm", "ff-Latn")]
	[InlineData("ks-Deva-IN", "ks-Deva", "ks-Arab")]
	[InlineData("sd-Deva-IN", "sd-Deva", "sd-Arab")]
	[InlineData("zh-HK", "zh-Hant", "zh-Hans")]
	[InlineData("zh-CN", "zh-Hans", "zh-Hant")]
	public void WritingSystemsNeverSilentlyCrossFallbacks(string requested, string expected, string other)
	{
		Assert.Equal(expected, FindBestCatalog(requested, [other, expected]));
		Assert.Equal(expected, FindBestCatalog(requested, [expected, other]));
		Assert.Null(FindBestCatalog(requested, [other]));
	}

	[Fact]
	public void DifferentReadingDirectionsDoNotBorrowANeutralCatalog()
	{
		Assert.Null(FindBestCatalog("pa-PK", ["pa"]));
	}

	[Fact]
	public void AmbiguousRegionalScriptsAreNotGuessedFromADifferentRegion()
	{
		Assert.Null(FindBestCatalog("mn-CN", ["mn"]));
		Assert.Equal("mn", FindBestCatalog("mn-MN", ["mn"]));
		Assert.Equal("az-Latn", FindBestCatalog("az-AZ", ["az-Cyrl", "az-Latn"]));
	}

	[Theory]
	[InlineData("zh", "zh-Hans")]
	[InlineData("zh-CHT", "zh-Hant")]
	[InlineData("no-NO", "nb")]
	[InlineData("no", "nb")]
	[InlineData("fr-CA", "fr")]
	[InlineData("pt", "pt-BR")]
	[InlineData("pt-PT", "pt-BR")]
	[InlineData("nl-BE", "nl")]
	public void ExistingRegionalAndLegacyAliasesStillResolve(string requested, string expected)
	{
		Assert.Equal(expected, FindBestCatalog(requested, ["nb", "fr", "pt-BR", "nl", "zh-Hans", "zh-Hant"]));
	}

	[Fact]
	public void SpecificTranslationWinsAndOtherRegionsAreDeterministic()
	{
		Assert.Equal("fr-CA", FindBestCatalog("fr-CA", ["fr", "fr-CA"]));
		Assert.Equal("pt-PT", FindBestCatalog("pt-PT", ["pt-BR", "pt-PT"]));
		Assert.Equal(FindBestCatalog("pt-AO", ["pt-PT", "pt-BR"]), FindBestCatalog("pt-AO", ["pt-BR", "pt-PT"]));
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData("not_a_culture!?")]
	[InlineData("unsupported-language")]
	[InlineData("nn-NO")]
	public void MissingOrUnknownLanguagesDoNotInventSupport(string? requested)
	{
		Assert.Null(FindBestCatalog(requested, ["en-US", "nb", "fr"]));
	}

	[Fact]
	public void EveryWindowsLanguageHasANonemptySelectionAndScriptsStayDistinct()
	{
		foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.NeutralCultures).Where(c => c.Name.Length > 0))
		{
			CultureInfo selection = GetSelectionCulture(culture);
			Assert.NotEmpty(selection.Name);
			if (GetExplicitScript(culture.Name) != null) Assert.Equal(culture.Name, selection.Name);
			Assert.Equal(culture.Name, FindBestCatalog(culture.Name, [culture.Name]));
		}
	}

	[Fact]
	public void UntranslatedScriptDoesNotAppearSupportedInTheApp()
	{
		Assert.Equal("en-US", LocalizationManager.NormalizeLanguageCode("hi-Latn"));
		Assert.DoesNotContain(LocalizationManager.SupportedLanguages, language => language.Code == "hi-Latn");
		Assert.Equal("hi-IN", LocalizationManager.NormalizeLanguageCode("hi"));
	}
}
