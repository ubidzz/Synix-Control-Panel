using System.Globalization;

namespace Synix_Control_Panel.SynixApp.Localization;

/// <summary>
/// Shared by the app and the offline coverage tool. A culture supported by
/// Windows is a translation target, not evidence that Synix has translated it.
/// </summary>
internal static class LanguageCatalogPolicy
{
	private static readonly HashSet<string> ScriptFamilies = CultureInfo.GetCultures(CultureTypes.NeutralCultures)
		.Where(culture => GetExplicitScript(culture.Name) != null)
		.Select(culture => culture.TwoLetterISOLanguageName).ToHashSet(StringComparer.OrdinalIgnoreCase);

	internal enum CatalogState { Missing, Preview, Incomplete, EnglishCopy, Complete }

	internal sealed record CatalogCoverage(
		CatalogState State, int RequiredCount, IReadOnlyList<string> MissingKeys,
		IReadOnlyList<string> UnexpectedKeys);

	internal static CatalogCoverage CheckCatalog(
		IReadOnlyDictionary<string, string> english,
		IReadOnlyDictionary<string, string>? catalog,
		bool isEnglish = false)
	{
		string[] required = english.Keys.Where(IsTextKey).Order(StringComparer.Ordinal).ToArray();
		if (catalog == null)
			return new(CatalogState.Missing, required.Length, required, []);

		string[] missing = required.Where(key => !catalog.TryGetValue(key, out string? value)
			|| string.IsNullOrWhiteSpace(value)).ToArray();
		string[] unexpected = catalog.Keys.Where(key => IsTextKey(key) && !english.ContainsKey(key))
			.Order(StringComparer.Ordinal).ToArray();
		CatalogState state;
		if (catalog.TryGetValue("_Language.IsPreview", out string? preview)
			&& string.Equals(preview.Trim(), "true", StringComparison.OrdinalIgnoreCase))
			state = CatalogState.Preview;
		else if (required.Length == 0 || missing.Length > 0 || unexpected.Length > 0)
			state = CatalogState.Incomplete;
		else if (!isEnglish && required.All(key => Normalize(catalog[key]) == Normalize(english[key])))
			state = CatalogState.EnglishCopy;
		else
			state = CatalogState.Complete;
		return new(state, required.Length, missing, unexpected);
	}

	internal static bool IsTextKey(string key) => !key.StartsWith('_');
	private static string Normalize(string text) => text.Replace("\r\n", "\n").Trim();

	internal static CultureInfo GetSelectionCulture(CultureInfo resourceCulture)
	{
		// Keep ALL explicit writing systems, not just the two Chinese catalogs.
		// Some neutral cultures have no specific culture; never turn those into
		// the invariant culture (an empty selection code).
		if (!resourceCulture.IsNeutralCulture || GetExplicitScript(resourceCulture.Name) != null)
			return resourceCulture;
		CultureInfo specific = CultureInfo.CreateSpecificCulture(resourceCulture.Name);
		return ParentsAndSelf(specific).Any(parent => SameName(parent.Name, resourceCulture.Name))
			? specific : resourceCulture;
	}

	internal static string? FindBestCatalog(string? requestedName, IEnumerable<string> availableNames)
	{
		if (string.IsNullOrWhiteSpace(requestedName)) return null;
		CultureInfo requested;
		try { requested = CultureInfo.GetCultureInfo(requestedName); }
		catch (CultureNotFoundException) { return null; }
		if (requested.Name.Length == 0) return null;

		CultureInfo[] available = availableNames.Select(CultureInfo.GetCultureInfo)
			.Where(culture => culture.Name.Length > 0)
			.DistinctBy(culture => culture.Name, StringComparer.OrdinalIgnoreCase)
			.OrderBy(culture => culture.Name, StringComparer.OrdinalIgnoreCase).ToArray();
		CultureInfo? exact = available.FirstOrDefault(culture => SameName(culture.Name, requested.Name));
		if (exact != null) return exact.Name;

		// Generic Norwegian aliases Bokmål, never the separate Nynorsk language.
		if (requested.TwoLetterISOLanguageName == "no")
			requested = CultureInfo.GetCultureInfo("nb-NO");

		foreach (CultureInfo parent in ParentsAndSelf(requested))
		{
			CultureInfo? match = available.FirstOrDefault(candidate => SameName(candidate.Name, parent.Name)
				&& CanShareCatalog(requested, candidate));
			if (match != null) return match.Name;
		}

		CultureInfo specific = CultureInfo.CreateSpecificCulture(requested.Name);
		CultureInfo? preferred = available.FirstOrDefault(candidate => SameName(candidate.Name, specific.Name)
			&& CanShareCatalog(requested, candidate));
		return preferred?.Name ?? available.FirstOrDefault(candidate => CanShareCatalog(requested, candidate))?.Name;
	}

	private static bool CanShareCatalog(CultureInfo requested, CultureInfo candidate)
	{
		if (!SameName(requested.TwoLetterISOLanguageName, candidate.TwoLetterISOLanguageName))
			return false;
		string? requestedScript = GetKnownScript(requested);
		string? candidateScript = GetKnownScript(candidate);
		// Unknown is not a wildcard: an explicit Latin request must not quietly
		// receive a default Devanagari catalog, for example hi-Latn -> hi.
		if (!SameName(requestedScript, candidateScript)) return false;
		if (requestedScript == null && !requested.IsNeutralCulture
			&& ScriptFamilies.Contains(requested.TwoLetterISOLanguageName))
		{
			CultureInfo defaultRegion = CultureInfo.CreateSpecificCulture(requested.TwoLetterISOLanguageName);
			// Some regional aliases (for example mn-CN) have no script parent in
			// .NET. Do not guess another region's default writing system for them.
			if (!SameRegion(requested, defaultRegion)) return false;
		}
		return requested.TextInfo.IsRightToLeft == candidate.TextInfo.IsRightToLeft;
	}

	internal static string? GetKnownScript(CultureInfo culture)
	{
		string? script = ParentsAndSelf(culture).Select(parent => GetExplicitScript(parent.Name))
			.FirstOrDefault(value => value != null);
		if (script != null) return script;
		// .NET supplies script parents for defaults such as zh -> zh-CN -> zh-Hans.
		// Do not infer a regional script from a DIFFERENT region's default.
		CultureInfo specific = CultureInfo.CreateSpecificCulture(culture.Name);
		script = ParentsAndSelf(specific).Select(parent => GetExplicitScript(parent.Name))
			.FirstOrDefault(value => value != null);
		if (script != null || culture.IsNeutralCulture) return script;
		CultureInfo defaultRegion = CultureInfo.CreateSpecificCulture(culture.TwoLetterISOLanguageName);
		return SameRegion(culture, defaultRegion)
			? ParentsAndSelf(defaultRegion).Select(parent => GetExplicitScript(parent.Name)).FirstOrDefault(value => value != null)
			: null;
	}

	private static bool SameRegion(CultureInfo left, CultureInfo right)
	{
		if (left.IsNeutralCulture || right.IsNeutralCulture || right.Name.Length == 0) return false;
		try { return SameName(new RegionInfo(left.Name).Name, new RegionInfo(right.Name).Name); }
		catch (ArgumentException) { return false; }
	}

	internal static string? GetExplicitScript(string name)
	{
		// A BCP-47 script subtag occurs immediately after the language (not in
		// private-use extensions, country codes, or variants).
		string[] parts = name.Split('-');
		return parts.Length > 1 && parts[1].Length == 4 && parts[1].All(char.IsAsciiLetter)
			? parts[1] : null;
	}

	private static bool SameName(string? left, string? right) =>
		string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

	private static IEnumerable<CultureInfo> ParentsAndSelf(CultureInfo culture)
	{
		for (; culture.Name.Length > 0; culture = culture.Parent)
			yield return culture;
	}
}
