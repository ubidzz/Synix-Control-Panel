// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
//
// LEGAL NOTICE:
// This source code is proprietary and confidential.
// 1. Permission is granted for PERSONAL, NON-COMMERCIAL use only.
// 2. You may modify this code for your own use, but you may NOT redistribute,
//    rebrand, or sell this code or derivative works without written consent.
// 3. The "Synix" brand and logic remain the property of Jason Turner.
// ============================================================================
using System.Runtime.CompilerServices;
using Synix_Control_Panel.SynixApp.Localization;

namespace Synix_Control_Panel.SynixApp.Design
{
	public static class ThemeManager
	{
		private sealed class ThemeRegistration
		{
			public int AppliedVersion { get; set; } = -1;
			public bool IsWatching { get; set; }
			public bool BackgroundImageCaptured { get; set; }
			public Image? DarkBackgroundImage { get; set; }
			public int? CornerRadius { get; set; }
		}

		internal sealed record ThemeColors
		{
			public required Color Window { get; init; }
			public required Color TitleBar { get; init; }
			public required Color Sidebar { get; init; }
			public required Color Card { get; init; }
			public required Color CardHover { get; init; }
			public required Color Input { get; init; }
			public required Color AlternateInput { get; init; }
			public required Color Border { get; init; }
			public required Color BorderHover { get; init; }
			public required Color PrimaryText { get; init; }
			public required Color SecondaryText { get; init; }
			public required Color MutedText { get; init; }
			public required Color Accent { get; init; }
			public required Color AccentHover { get; init; }
			public required Color AccentSoft { get; init; }
			public required Color Warning { get; init; }
			public required Color Selection { get; init; }
			public required Color Divider { get; init; }
			public required Color InfoSurface { get; init; }
			public required Color DisabledSurface { get; init; }
			public required Color DisabledText { get; init; }
			public required Color Console { get; init; }
		}

		private static readonly ThemeColors DarkColors = new()
		{
			Window = Color.FromArgb(8, 13, 24),
			TitleBar = Color.FromArgb(6, 12, 22),
			Sidebar = Color.FromArgb(10, 18, 32),
			Card = Color.FromArgb(17, 27, 45),
			CardHover = Color.FromArgb(20, 33, 54),
			Input = Color.FromArgb(12, 21, 36),
			AlternateInput = Color.FromArgb(14, 24, 40),
			Border = Color.FromArgb(38, 52, 77),
			BorderHover = Color.FromArgb(55, 76, 108),
			PrimaryText = Color.FromArgb(245, 247, 251),
			SecondaryText = Color.FromArgb(158, 172, 194),
			MutedText = Color.FromArgb(105, 124, 153),
			Accent = Color.FromArgb(32, 214, 199),
			AccentHover = Color.FromArgb(50, 231, 216),
			AccentSoft = Color.FromArgb(28, 75, 91),
			Warning = Color.FromArgb(245, 185, 76),
			Selection = Color.FromArgb(24, 55, 73),
			Divider = Color.FromArgb(31, 45, 67),
			InfoSurface = Color.FromArgb(13, 38, 49),
			DisabledSurface = Color.FromArgb(25, 34, 48),
			DisabledText = Color.FromArgb(105, 124, 153),
			Console = Color.FromArgb(15, 15, 15)
		};

		private static readonly ThemeColors LightColors = new()
		{
			Window = Color.FromArgb(229, 234, 241),
			TitleBar = Color.FromArgb(247, 249, 252),
			Sidebar = Color.FromArgb(240, 244, 248),
			Card = Color.FromArgb(248, 250, 252),
			CardHover = Color.FromArgb(238, 243, 248),
			Input = Color.FromArgb(232, 238, 245),
			AlternateInput = Color.FromArgb(245, 248, 251),
			Border = Color.FromArgb(185, 198, 214),
			BorderHover = Color.FromArgb(143, 163, 186),
			PrimaryText = Color.FromArgb(23, 32, 51),
			SecondaryText = Color.FromArgb(71, 85, 105),
			MutedText = Color.FromArgb(100, 116, 139),
			Accent = Color.FromArgb(0, 128, 115),
			AccentHover = Color.FromArgb(0, 121, 107),
			AccentSoft = Color.FromArgb(215, 240, 237),
			Warning = Color.FromArgb(169, 101, 19),
			Selection = Color.FromArgb(220, 234, 242),
			Divider = Color.FromArgb(211, 221, 232),
			InfoSurface = Color.FromArgb(226, 240, 242),
			DisabledSurface = Color.FromArgb(224, 230, 238),
			DisabledText = Color.FromArgb(112, 128, 151),
			Console = Color.FromArgb(226, 234, 243)
		};

		private static readonly ConditionalWeakTable<Control, ThemeRegistration>
			Registrations = new();
		private static ThemeColors _current = DarkColors;
		private static ThemeColors _previous = DarkColors;
		private sealed record ColorBinding(Color Source, Color Applied);
		private static readonly ConditionalWeakTable<object, Dictionary<string, ColorBinding>> ColorBindings = new();
		private static int _themeVersion;
		private static bool _isApplying;

		public static bool IsDarkMode { get; private set; } = true;
		public static string ThemeId { get; private set; } = "synix";
		public static string AccentId { get; private set; } = "default";
		public static string StyleId { get; private set; } = "rounded";
		public static int CornerRadius(int roundedRadius) => StyleId == "square" ? 0 : roundedRadius;

		internal static ThemeColors Colors => _current;

		public static event EventHandler? ThemeChanged;

		public static void Initialize(bool darkMode, string? theme = null, string? accent = null, string? style = null)
		{
			_previous = _current;
			IsDarkMode = darkMode;
			ThemeId = ThemeCatalog.NormalizeTheme(theme);
			AccentId = ThemeCatalog.NormalizeAccent(accent);
			StyleId = ThemeCatalog.NormalizeStyle(style);
			_current = ThemeCatalog.Create(darkMode, ThemeId, AccentId, darkMode ? DarkColors : LightColors);
			_themeVersion++;
		}

		public static void SetDarkMode(bool darkMode) => SetAppearance(darkMode, ThemeId, AccentId, StyleId);

		public static void SetAppearance(bool darkMode, string? theme, string? accent, string? style)
		{
			Initialize(darkMode, theme, accent, style);
			ApplyToOpenForms(force: true);
			ThemeChanged?.Invoke(null, EventArgs.Empty);
		}

		public static void ApplyToOpenForms(bool force = false)
		{
			if (_isApplying)
				return;

			_isApplying = true;
			try
			{
				foreach (Form form in Application.OpenForms.Cast<Form>().ToArray())
				{
					if (form.IsDisposed)
						continue;

					ThemeRegistration registration = GetRegistration(form);
					WatchControlTree(form);
					if (force || registration.AppliedVersion != _themeVersion)
					{
						Apply(form);
						registration.AppliedVersion = _themeVersion;
					}
				}
			}
			finally
			{
				_isApplying = false;
			}
		}

		public static void Apply(Control root)
		{
			if (root.IsDisposed)
				return;

			if (root is Form form)
			{
				SynixWindowHeader.Apply(form);
				WindowMemoryReclaimer.Track(form);
			}

			root.SuspendLayout();
			try
			{
				ApplyControlTree(root);
				WatchControlTree(root);
				GetRegistration(root).AppliedVersion = _themeVersion;
			}
			finally
			{
				root.ResumeLayout(false);
				root.Invalidate(true);
			}

			LocalizationManager.Apply(root);
		}

		private static void ApplyControlTree(Control control)
		{
			ApplyControl(control);
			foreach (Control child in control.Controls)
				ApplyControlTree(child);
		}

		private static void WatchControlTree(Control control)
		{
			ThemeRegistration registration = GetRegistration(control);
			if (!registration.IsWatching)
			{
				control.ControlAdded += Control_ControlAdded;
				registration.IsWatching = true;
			}

			foreach (Control child in control.Controls)
				WatchControlTree(child);
		}

		private static void Control_ControlAdded(object? sender, ControlEventArgs eventArgs)
		{
			if (eventArgs.Control == null || eventArgs.Control.IsDisposed)
				return;

			ApplyControlTree(eventArgs.Control);
			WatchControlTree(eventArgs.Control);
			LocalizationManager.Apply(eventArgs.Control);
			GetRegistration(eventArgs.Control).AppliedVersion = _themeVersion;
			eventArgs.Control.Invalidate(true);
		}

		private static ThemeRegistration GetRegistration(Control control)
		{
			return Registrations.GetValue(control, _ => new ThemeRegistration());
		}

		private static void ApplyControl(Control control)
		{
			Color originalForeground = control.ForeColor;
			if (control is Form)
			{
				ThemeRegistration registration = GetRegistration(control);
				if (!registration.BackgroundImageCaptured)
				{
					registration.DarkBackgroundImage = control.BackgroundImage;
					registration.BackgroundImageCaptured = true;
				}
				control.BackgroundImage = IsDarkMode && ThemeId == "synix"
					? registration.DarkBackgroundImage
					: null;
			}

			control.BackColor = MapColor(control, "BackColor", control.BackColor, color => MapBackground(color, control));
			control.ForeColor = MapColor(control, "ForeColor", control.ForeColor, MapForeground);

			if (control is ModernSettingsCard card)
			{
				card.FillColor = MapColor(card, "FillColor", card.FillColor, color => MapBackground(color, card));
				card.BorderColor = MapColor(card, "BorderColor", card.BorderColor, MapPaletteColor);
				ThemeRegistration registration = GetRegistration(card);
				registration.CornerRadius ??= card.CornerRadius;
				card.CornerRadius = CornerRadius(registration.CornerRadius.Value);
			}
			else if (control is ModernSettingsComboBox comboBox)
			{
				comboBox.BackColor = SettingsPalette.Input;
				comboBox.ForeColor = SettingsPalette.PrimaryText;
				comboBox.BorderColor = SettingsPalette.Border;
				comboBox.FocusBorderColor = SettingsPalette.BorderHover;
				comboBox.ArrowColor = SettingsPalette.SecondaryText;
				comboBox.SelectedItemBackColor = SettingsPalette.Selection;
			}
			else if (control is ModernSettingsNumericUpDown numericUpDown)
			{
				numericUpDown.BackColor = SettingsPalette.Input;
				numericUpDown.ForeColor = SettingsPalette.PrimaryText;
			}
			else if (control is ModernSettingsToggle toggle)
			{
				toggle.BackColor = toggle.Parent?.BackColor ?? SettingsPalette.Card;
			}
			else if (control is ModernSettingsNavButton navigationButton)
			{
				navigationButton.BackColor = SettingsPalette.Sidebar;
				navigationButton.ForeColor = SettingsPalette.SecondaryText;
			}
			else if (control is ModernSettingsGlyph glyph)
			{
				glyph.BackColor = glyph.Parent?.BackColor ?? SettingsPalette.Card;
				glyph.ForeColor = SettingsPalette.Accent;
			}
			else if (control is SynixButton synixButton)
			{
				synixButton.FillColor = MapColor(synixButton, "FillColor", synixButton.FillColor, color => MapBackground(color, synixButton));
				synixButton.FillColorSecondary = MapColor(synixButton, "FillColorSecondary", synixButton.FillColorSecondary,
					color => MapBackground(color, synixButton));
				synixButton.BorderColor = MapColor(synixButton, "BorderColor", synixButton.BorderColor, MapPaletteColor);
				if (synixButton.FillColor.ToArgb() == SettingsPalette.Accent.ToArgb())
					synixButton.ForeColor = SettingsPalette.AccentText;
				if (!IsDarkMode && IsDarkSurface(synixButton.FillColor))
				{
					if (IsSuccessColor(originalForeground))
						synixButton.ForeColor = Color.FromArgb(80, 230, 164);
					else if (IsDangerColor(originalForeground))
						synixButton.ForeColor = Color.FromArgb(250, 116, 128);
					else if (synixButton.ForeColor.ToArgb() == SettingsPalette.PrimaryText.ToArgb())
						synixButton.ForeColor = Color.White;
				}
			}
			else if (control is SynixGauge gauge)
			{
				gauge.BackColor = gauge.Parent?.BackColor ?? SettingsPalette.Card;
				gauge.ForeColor = SettingsPalette.PrimaryText;
			}

			if (control is TextBoxBase textBox)
			{
				textBox.BackColor = textBox is RichTextBox and not ModernSettingsDetailsBox
					? SettingsPalette.Console
					: SettingsPalette.Input;
				textBox.ForeColor = SettingsPalette.PrimaryText;
			}
			else if (control is ListControl listControl)
			{
				listControl.BackColor = SettingsPalette.Input;
				listControl.ForeColor = SettingsPalette.PrimaryText;
			}
			else if (control is TreeView treeView)
			{
				treeView.BackColor = SettingsPalette.Sidebar;
				treeView.ForeColor = SettingsPalette.SecondaryText;
				treeView.LineColor = SettingsPalette.Border;
			}

			if (control is LinkLabel linkLabel)
			{
				linkLabel.LinkColor = SettingsPalette.Accent;
				linkLabel.ActiveLinkColor = SettingsPalette.AccentHover;
				linkLabel.VisitedLinkColor = SettingsPalette.SecondaryText;
			}

			if (control is Button standardButton &&
				control is not ModernSettingsButton &&
				control is not ModernSettingsNavButton &&
				control is not SynixButton &&
				standardButton.FlatStyle is FlatStyle.Standard or FlatStyle.System)
			{
				standardButton.UseVisualStyleBackColor = false;
				standardButton.FlatStyle = FlatStyle.Flat;
				standardButton.BackColor = SettingsPalette.Card;
				standardButton.FlatAppearance.BorderSize = 1;
				standardButton.FlatAppearance.BorderColor = SettingsPalette.Border;
			}

			if (control is ButtonBase button)
			{
				button.FlatAppearance.BorderColor =
					MapColor(button, "FlatBorder", button.FlatAppearance.BorderColor, MapPaletteColor);
				button.FlatAppearance.MouseOverBackColor =
					MapColor(button, "FlatHover", button.FlatAppearance.MouseOverBackColor, MapPaletteColor);
				button.FlatAppearance.MouseDownBackColor =
					MapColor(button, "FlatPressed", button.FlatAppearance.MouseDownBackColor, MapPaletteColor);
			}

			if (control is DataGridView grid)
				ApplyGrid(grid);
			if (control is ToolStrip toolStrip)
				ApplyToolStrip(toolStrip);
			if (control.ContextMenuStrip is { } menu)
				ApplyToolStrip(menu);

			control.Invalidate();
		}

		private static void ApplyGrid(DataGridView grid)
		{
			grid.BackgroundColor = MapColor(grid, "BackgroundColor", grid.BackgroundColor, color => MapBackground(color, grid));
			grid.GridColor = MapColor(grid, "GridColor", grid.GridColor, MapPaletteColor);
			if (grid.Tag is int radius)
				GridStyler.ApplyRoundedCorners(grid, radius);
			grid.DefaultCellStyle = ApplyCellStyle(grid, "Default", grid.DefaultCellStyle);
			grid.AlternatingRowsDefaultCellStyle = ApplyCellStyle(grid, "Alternate", grid.AlternatingRowsDefaultCellStyle);
			grid.RowsDefaultCellStyle = ApplyCellStyle(grid, "Rows", grid.RowsDefaultCellStyle);
			grid.ColumnHeadersDefaultCellStyle = ApplyCellStyle(grid, "ColumnHeaders", grid.ColumnHeadersDefaultCellStyle);
			grid.RowHeadersDefaultCellStyle = ApplyCellStyle(grid, "RowHeaders", grid.RowHeadersDefaultCellStyle);

			foreach (DataGridViewColumn column in grid.Columns)
				column.DefaultCellStyle = ApplyCellStyle(column, "Default", column.DefaultCellStyle);
			foreach (DataGridViewRow row in grid.Rows)
			{
				row.DefaultCellStyle = ApplyCellStyle(row, "Default", row.DefaultCellStyle);
				foreach (DataGridViewCell cell in row.Cells)
					if (cell.HasStyle) cell.Style = ApplyCellStyle(cell, "Default", cell.Style);
			}
		}

		private static DataGridViewCellStyle ApplyCellStyle(object owner, string prefix, DataGridViewCellStyle style)
		{
			// WinForms may return an ambient-style copy. Keep role bindings on
			// the stable owner, then assign the completed style back to it.
			style.BackColor = MapColor(owner, prefix + ".BackColor", style.BackColor, color => MapBackground(color, null));
			style.ForeColor = MapColor(owner, prefix + ".ForeColor", style.ForeColor, MapForeground);
			style.SelectionBackColor = MapColor(owner, prefix + ".SelectionBackColor", style.SelectionBackColor, MapPaletteColor);
			style.SelectionForeColor = MapColor(owner, prefix + ".SelectionForeColor", style.SelectionForeColor, MapForeground);
			return style;
		}

		// Retain the original semantic color per property. Mapping only from the
		// last RGB value loses roles when two theme colors coincide, causing
		// progressively incorrect colors after repeatedly changing themes.
		private static Color MapColor(object owner, string property, Color value, Func<Color, Color> map)
		{
			Dictionary<string, ColorBinding> bindings = ColorBindings.GetOrCreateValue(owner);
			Color source = bindings.TryGetValue(property, out ColorBinding? binding) && binding.Applied.ToArgb() == value.ToArgb()
				? binding.Source : CanonicalColor(value);
			Color applied = map(source);
			bindings[property] = new(source, applied);
			return applied;
		}

		private static Color CanonicalColor(Color color)
		{
			int role = FindPaletteRole(color);
			return role >= 0 ? PaletteColors(DarkColors)[role] : color;
		}

		private static void ApplyToolStrip(ToolStrip toolStrip)
		{
			toolStrip.BackColor = SettingsPalette.Card;
			toolStrip.ForeColor = SettingsPalette.PrimaryText;
			if (toolStrip is ToolStripDropDown && toolStrip.Renderer is not SynixMenuRenderer)
				toolStrip.Renderer = new SynixMenuRenderer();

			foreach (ToolStripItem item in toolStrip.Items)
			{
				item.BackColor = SettingsPalette.Card;
				item.ForeColor = SettingsPalette.PrimaryText;
				if (item is ToolStripDropDownItem dropDown && dropDown.HasDropDownItems)
					ApplyToolStrip(dropDown.DropDown);
			}
		}

		private static Color MapBackground(Color color, Control? control)
		{
			if (color == Color.Transparent || color.IsEmpty)
				return color;

			Color mapped = MapPaletteColor(color);
			if (mapped.ToArgb() != color.ToArgb())
				return mapped;

			if (color.ToArgb() == Color.Black.ToArgb())
				return control is RichTextBox ? SettingsPalette.Console : SettingsPalette.Window;
			if (color.ToArgb() == Color.White.ToArgb() ||
				color.ToArgb() == Color.WhiteSmoke.ToArgb())
			{
				if (control is PictureBox)
					return Color.White;

				return control is TextBoxBase || control is ListControl
					? SettingsPalette.Input
					: SettingsPalette.Card;
			}
			if (color.IsSystemColor)
				return control is Form ? SettingsPalette.Window
					: control is TextBoxBase or ListControl or DataGridView ? SettingsPalette.Input : SettingsPalette.Card;

			return color;
		}

		private static Color MapForeground(Color color)
		{
			if (color == Color.Transparent || color.IsEmpty)
				return color;

			Color mapped = MapPaletteColor(color);
			if (mapped.ToArgb() != color.ToArgb())
				return mapped;

			if (color.ToArgb() == Color.White.ToArgb() ||
				color.ToArgb() == Color.WhiteSmoke.ToArgb() ||
				color.ToArgb() == Color.Black.ToArgb())
			{
				return SettingsPalette.PrimaryText;
			}
			if (color.IsSystemColor)
				return SettingsPalette.PrimaryText;

			return color;
		}

		private static Color MapPaletteColor(Color color)
		{
			if (color.IsEmpty || color == Color.Transparent)
				return color;

			int role = FindPaletteRole(color);
			Color roleColor = role switch
			{
				0 => SettingsPalette.Window,
				1 => SettingsPalette.TitleBar,
				2 => SettingsPalette.Sidebar,
				3 => SettingsPalette.Card,
				4 => SettingsPalette.CardHover,
				5 => SettingsPalette.Input,
				6 => SettingsPalette.AlternateInput,
				7 => SettingsPalette.Border,
				8 => SettingsPalette.BorderHover,
				9 => SettingsPalette.PrimaryText,
				10 => SettingsPalette.SecondaryText,
				11 => SettingsPalette.MutedText,
				12 => SettingsPalette.Accent,
				13 => SettingsPalette.AccentHover,
				14 => SettingsPalette.AccentSoft,
				15 => SettingsPalette.Warning,
				16 => SettingsPalette.Selection,
				17 => SettingsPalette.Divider,
				18 => SettingsPalette.InfoSurface,
				19 => SettingsPalette.DisabledSurface,
				20 => SettingsPalette.DisabledText,
				21 => SettingsPalette.Console,
				_ => Color.Empty
			};
			if (!roleColor.IsEmpty)
				return roleColor;

			int argb = color.ToArgb();
			if (MatchesEither(argb, Color.FromArgb(96, 165, 250), Color.FromArgb(37, 99, 168)))
				return IsDarkMode ? Color.FromArgb(96, 165, 250) : Color.FromArgb(37, 99, 168);
			if (MatchesEither(argb, Color.FromArgb(125, 165, 213), Color.FromArgb(37, 99, 168)))
				return IsDarkMode ? Color.FromArgb(125, 165, 213) : Color.FromArgb(37, 99, 168);
			if (MatchesEither(argb, Color.FromArgb(167, 139, 250), Color.FromArgb(109, 72, 184)))
				return IsDarkMode ? Color.FromArgb(167, 139, 250) : Color.FromArgb(109, 72, 184);
			if (MatchesEither(argb, Color.FromArgb(164, 125, 245), Color.FromArgb(109, 72, 184)))
				return IsDarkMode ? Color.FromArgb(164, 125, 245) : Color.FromArgb(109, 72, 184);
			if (argb == Color.FromArgb(52, 211, 153).ToArgb() ||
				argb == Color.FromArgb(80, 230, 164).ToArgb() ||
				argb == Color.FromArgb(17, 124, 82).ToArgb())
			{
				return SettingsPalette.Success;
			}
			if (argb == Color.FromArgb(248, 113, 113).ToArgb() ||
				argb == Color.FromArgb(250, 116, 128).ToArgb() ||
				argb == Color.FromArgb(242, 91, 103).ToArgb() ||
				argb == Color.FromArgb(190, 45, 60).ToArgb())
			{
				return SettingsPalette.Danger;
			}
			if (MatchesEither(argb, Color.FromArgb(15, 61, 66), Color.FromArgb(215, 240, 237)))
				return IsDarkMode ? Color.FromArgb(15, 61, 66) : Color.FromArgb(215, 240, 237);
			if (MatchesEither(argb, Color.FromArgb(48, 39, 77), Color.FromArgb(236, 228, 251)))
				return IsDarkMode ? Color.FromArgb(48, 39, 77) : Color.FromArgb(236, 228, 251);
			if (MatchesEither(argb, Color.FromArgb(68, 52, 24), Color.FromArgb(248, 236, 208)))
				return IsDarkMode ? Color.FromArgb(68, 52, 24) : Color.FromArgb(248, 236, 208);
			if (MatchesEither(argb, Color.FromArgb(40, 48, 61), Color.FromArgb(227, 232, 239)))
				return IsDarkMode ? Color.FromArgb(40, 48, 61) : Color.FromArgb(227, 232, 239);
			if (MatchesEither(argb, Color.FromArgb(24, 48, 72), Color.FromArgb(221, 234, 248)))
				return IsDarkMode ? Color.FromArgb(24, 48, 72) : Color.FromArgb(221, 234, 248);
			if (argb == Color.FromArgb(214, 222, 234).ToArgb() ||
				argb == Color.FromArgb(203, 213, 225).ToArgb() ||
				argb == Color.FromArgb(220, 226, 237).ToArgb())
			{
				return SettingsPalette.SecondaryText;
			}

			return color;
		}

		private static bool MatchesEither(int argb, Color first, Color second)
		{
			return argb == first.ToArgb() || argb == second.ToArgb();
		}

		private static bool IsDarkSurface(Color color)
		{
			double luminance =
				(0.2126 * color.R) +
				(0.7152 * color.G) +
				(0.0722 * color.B);
			return luminance < 135;
		}

		private static bool IsSuccessColor(Color color)
		{
			int argb = color.ToArgb();
			return argb == Color.FromArgb(52, 211, 153).ToArgb() ||
				argb == Color.FromArgb(80, 230, 164).ToArgb() ||
				argb == Color.FromArgb(17, 124, 82).ToArgb();
		}

		private static bool IsDangerColor(Color color)
		{
			int argb = color.ToArgb();
			return argb == Color.FromArgb(248, 113, 113).ToArgb() ||
				argb == Color.FromArgb(250, 116, 128).ToArgb() ||
				argb == Color.FromArgb(242, 91, 103).ToArgb() ||
				argb == Color.FromArgb(190, 45, 60).ToArgb();
		}

		private static int FindPaletteRole(Color color)
		{
			int argb = color.ToArgb();
			ThemeColors[] palettes = { DarkColors, LightColors, _previous, _current };
			foreach (ThemeColors palette in palettes)
			{
				if (argb == palette.Window.ToArgb()) return 0;
				if (argb == palette.TitleBar.ToArgb()) return 1;
				if (argb == palette.Sidebar.ToArgb()) return 2;
				if (argb == palette.Card.ToArgb()) return 3;
				if (argb == palette.CardHover.ToArgb()) return 4;
				if (argb == palette.Input.ToArgb()) return 5;
				if (argb == palette.AlternateInput.ToArgb()) return 6;
				if (argb == palette.Border.ToArgb()) return 7;
				if (argb == palette.BorderHover.ToArgb()) return 8;
				if (argb == palette.PrimaryText.ToArgb()) return 9;
				if (argb == palette.SecondaryText.ToArgb()) return 10;
				if (argb == palette.MutedText.ToArgb()) return 11;
				if (argb == palette.Accent.ToArgb()) return 12;
				if (argb == palette.AccentHover.ToArgb()) return 13;
				if (argb == palette.AccentSoft.ToArgb()) return 14;
				if (argb == palette.Warning.ToArgb()) return 15;
				if (argb == palette.Selection.ToArgb()) return 16;
				if (argb == palette.Divider.ToArgb()) return 17;
				if (argb == palette.InfoSurface.ToArgb()) return 18;
				if (argb == palette.DisabledSurface.ToArgb()) return 19;
				if (argb == palette.DisabledText.ToArgb()) return 20;
				if (argb == palette.Console.ToArgb()) return 21;
			}

			if (argb == Color.FromArgb(13, 23, 39).ToArgb()) return 6;
			if (argb == Color.FromArgb(15, 25, 42).ToArgb()) return 6;
			if (argb == Color.FromArgb(24, 55, 73).ToArgb()) return 16;
			if (argb == Color.FromArgb(22, 50, 67).ToArgb()) return 16;
			if (argb == Color.FromArgb(29, 63, 80).ToArgb()) return 16;
			if (argb == Color.FromArgb(16, 30, 48).ToArgb()) return 4;
			if (argb == Color.FromArgb(21, 34, 52).ToArgb()) return 4;
			if (argb == Color.FromArgb(24, 47, 63).ToArgb()) return 4;
			if (argb == Color.FromArgb(28, 42, 60).ToArgb()) return 16;
			if (argb == Color.FromArgb(32, 45, 66).ToArgb()) return 17;
			if (argb == Color.FromArgb(30, 43, 63).ToArgb()) return 17;
			if (argb == Color.FromArgb(13, 38, 49).ToArgb()) return 18;
			if (argb == Color.FromArgb(22, 58, 70).ToArgb()) return 18;
			if (argb == Color.FromArgb(12, 47, 59).ToArgb()) return 18;
			if (argb == Color.FromArgb(11, 35, 47).ToArgb()) return 18;
			if (argb == Color.FromArgb(22, 31, 45).ToArgb()) return 19;
			if (argb == Color.FromArgb(36, 45, 60).ToArgb()) return 19;
			if (argb == Color.FromArgb(40, 48, 61).ToArgb()) return 19;
			if (argb == Color.FromArgb(66, 80, 101).ToArgb()) return 20;
			if (argb == Color.FromArgb(150, 158, 170).ToArgb()) return 20;
			if (argb == Color.FromArgb(25, 25, 30).ToArgb()) return 21;
			if (argb == Color.FromArgb(15, 15, 18).ToArgb()) return 21;
			if (argb == Color.FromArgb(50, 50, 60).ToArgb()) return 17;
			if (argb == Color.FromArgb(0, 190, 255).ToArgb()) return 12;
			if (argb == Color.FromArgb(0, 137, 123).ToArgb()) return 12;

			return -1;
		}

		private static Color[] PaletteColors(ThemeColors palette) =>
		[
			palette.Window, palette.TitleBar, palette.Sidebar, palette.Card, palette.CardHover,
			palette.Input, palette.AlternateInput, palette.Border, palette.BorderHover, palette.PrimaryText,
			palette.SecondaryText, palette.MutedText, palette.Accent, palette.AccentHover, palette.AccentSoft,
			palette.Warning, palette.Selection, palette.Divider, palette.InfoSurface, palette.DisabledSurface,
			palette.DisabledText, palette.Console
		];
	}
}
