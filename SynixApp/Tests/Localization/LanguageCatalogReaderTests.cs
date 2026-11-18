using Synix_Control_Panel.SynixApp.Localization;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class LanguageCatalogReaderTests
{
	[Fact]
	public void ReadsExactCultureStringsFromSatelliteFile()
	{
		string satellite = Path.Combine(AppContext.BaseDirectory, "fr",
			"Synix Control Panel.resources.dll");
		IReadOnlyDictionary<string, string>? catalog = LanguageCatalogReader.ReadSatellite(satellite, "fr");
		Assert.NotNull(catalog);
		Assert.Equal("Enregistrer", catalog["Text.1509F561F2416598629B"]);
		Assert.Null(LanguageCatalogReader.ReadSatellite(satellite, "ar"));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(100)]
	public void MissingAndTruncatedFilesCannotBecomeSelectableCatalogs(int length)
	{
		string path = Path.Combine(Path.GetTempPath(), $"synix-catalog-{Guid.NewGuid():N}.dll");
		try
		{
			Assert.Null(LanguageCatalogReader.ReadSatellite(path, "fr"));
			File.WriteAllBytes(path, new byte[length]);
			Assert.Null(LanguageCatalogReader.ReadSatellite(path, "fr"));
		}
		finally { File.Delete(path); }
	}
}
