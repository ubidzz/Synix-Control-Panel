namespace Synix_Control_Panel.SynixApp.Design;

/// <summary>Stable preference IDs, independent of the translated names in the picker.</summary>
public static class ThemeCatalog
{
	public sealed record Choice(string Id, string ResourceKey);

	public static IReadOnlyList<Choice> Themes { get; } = Array.AsReadOnly<Choice>(
	[
		new("synix", "Appearance.Synix"), new("graphite", "Appearance.Graphite"),
		new("ocean", "Appearance.Ocean"), new("forest", "Appearance.Forest"),
		new("violet", "Appearance.Violet"), new("sand", "Appearance.Sand")
	]);
	public static IReadOnlyList<Choice> Accents { get; } = Array.AsReadOnly<Choice>(
	[
		new("default", "Appearance.DefaultAccent"), new("teal", "Appearance.Teal"),
		new("blue", "Appearance.Blue"), new("violet", "Appearance.Violet"),
		new("rose", "Appearance.Rose"), new("amber", "Appearance.Amber"),
		new("green", "Appearance.Green")
	]);
	public static IReadOnlyList<Choice> Styles { get; } = Array.AsReadOnly<Choice>(
	[
		new("rounded", "Appearance.Rounded"), new("square", "Appearance.Square")
	]);

	public static string NormalizeTheme(string? value) => Normalize(value, Themes);
	public static string NormalizeAccent(string? value) => Normalize(value, Accents);
	public static string NormalizeStyle(string? value) => Normalize(value, Styles);

	private static string Normalize(string? value, IReadOnlyList<Choice> choices) =>
		choices.FirstOrDefault(choice => string.Equals(choice.Id, value?.Trim(),
			StringComparison.OrdinalIgnoreCase))?.Id ?? choices[0].Id;

	internal static ThemeManager.ThemeColors Create(bool dark, string theme, string accent,
		ThemeManager.ThemeColors baseline)
	{
		string accentId = accent == "default" ? theme switch
		{
			"ocean" or "graphite" => "blue", "forest" => "green",
			"violet" => "violet", "sand" => "amber", _ => "teal"
		} : accent;
		Color accentColor = (accentId, dark) switch
		{
			("blue", true) => Color.FromArgb(100, 181, 255),
			("blue", false) => Color.FromArgb(30, 102, 190),
			("violet", true) => Color.FromArgb(191, 163, 255),
			("violet", false) => Color.FromArgb(112, 66, 190),
			("rose", true) => Color.FromArgb(255, 151, 185),
			("rose", false) => Color.FromArgb(180, 45, 94),
			("amber", true) => Color.FromArgb(242, 194, 94),
			("amber", false) => Color.FromArgb(148, 91, 8),
			("green", true) => Color.FromArgb(123, 218, 158),
			("green", false) => Color.FromArgb(28, 119, 71),
			_ => baseline.Accent
		};
		Color tint = theme switch
		{
			"graphite" => Color.FromArgb(120, 120, 120),
			"ocean" => Color.FromArgb(26, 116, 165),
			"forest" => Color.FromArgb(53, 114, 79),
			"violet" => Color.FromArgb(111, 74, 153),
			"sand" => Color.FromArgb(148, 112, 65),
			_ => Color.Empty
		};
		Color Surface(Color original)
		{
			if (tint.IsEmpty) return original;
			// Mostly neutral surfaces give each preset a coherent hue. Retain
			// a little of the original color so distinct roles (for example,
			// window and console) don't collapse to the same grey.
			int grey = (original.R + original.G + original.B) / 3;
			Color neutral = Blend(Color.FromArgb(grey, grey, grey), original, .25);
			return Blend(neutral, tint, dark ? .12 : .05);
		}
		bool originalAccent = theme == "synix" && accentId == "teal";
		return baseline with
		{
			Window = Surface(baseline.Window), TitleBar = Surface(baseline.TitleBar),
			Sidebar = Surface(baseline.Sidebar), Card = Surface(baseline.Card),
			CardHover = Surface(baseline.CardHover), Input = Surface(baseline.Input),
			AlternateInput = Surface(baseline.AlternateInput), Border = Surface(baseline.Border),
			BorderHover = Surface(baseline.BorderHover), Divider = Surface(baseline.Divider),
			DisabledSurface = Surface(baseline.DisabledSurface), Console = Surface(baseline.Console),
			Accent = accentColor,
			AccentHover = originalAccent ? baseline.AccentHover : Blend(accentColor, dark ? Color.White : Color.Black, .12),
			AccentSoft = originalAccent ? baseline.AccentSoft : Blend(Surface(baseline.Card), accentColor, dark ? .21 : .12),
			Selection = originalAccent ? baseline.Selection : Blend(Surface(baseline.Input), accentColor, dark ? .22 : .12),
			InfoSurface = originalAccent ? baseline.InfoSurface : Blend(Surface(baseline.Card), accentColor, dark ? .10 : .06)
		};
	}

	private static Color Blend(Color from, Color to, double amount) => Color.FromArgb(
		(int)Math.Round(from.R + (to.R - from.R) * amount),
		(int)Math.Round(from.G + (to.G - from.G) * amount),
		(int)Math.Round(from.B + (to.B - from.B) * amount));
}
