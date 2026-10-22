using System.ComponentModel;
using System.Drawing.Drawing2D;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.Design.Controls;
using Synix_Control_Panel.SynixApp.Localization;

namespace Synix_Control_Panel.SynixApp.UI.Settings;

public partial class GeneralSettingsPage
{
	private ModernSettingsCard _appearanceCard = null!;
	private readonly ModernSettingsComboBox _themeChoice = new() { Name = "appearanceTheme" };
	private readonly ModernSettingsComboBox _accentChoice = new() { Name = "appearanceAccent" };
	private readonly ModernSettingsComboBox _styleChoice = new() { Name = "appearanceStyle" };
	private readonly AppearancePreview _appearancePreview = new() { Name = "appearancePreview" };

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string ThemeName
	{
		get => SelectedValue(_themeChoice, "synix");
		set => SelectOption(_themeChoice, ThemeCatalog.NormalizeTheme(value));
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string ThemeAccent
	{
		get => SelectedValue(_accentChoice, "default");
		set => SelectOption(_accentChoice, ThemeCatalog.NormalizeAccent(value));
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string ThemeStyle
	{
		get => SelectedValue(_styleChoice, "rounded");
		set => SelectOption(_styleChoice, ThemeCatalog.NormalizeStyle(value));
	}

	[Browsable(false)]
	public event EventHandler? AppearanceChanged;

	private static string SelectedValue(ComboBox choice, string fallback) =>
		(choice.SelectedItem as LocalizedOption)?.Value ?? fallback;

	private void InitializeAppearanceCard()
	{
		_appearanceCard = new ModernSettingsCard
		{
			Name = "settingsCardAppearance", AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(18), TabIndex = 2
		};
		TableLayoutPanel layout = new()
		{
			Name = "appearanceLayout", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
			Dock = DockStyle.Top, ColumnCount = 1, RowCount = 5, Margin = Padding.Empty
		};
		layout.ColumnStyles.Add(new(SizeType.Percent, 100));
		for (int row = 0; row < 5; row++) layout.RowStyles.Add(new(SizeType.AutoSize));
		Label title = AppearanceLabel("Appearance.Title", true);
		Label description = AppearanceLabel("Appearance.Description");
		layout.Controls.Add(title, 0, 0);
		layout.Controls.Add(description, 0, 1);

		TableLayoutPanel choices = new()
		{
			AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top,
			ColumnCount = 3, RowCount = 1, Margin = new Padding(0, 12, 0, 10)
		};
		choices.RowStyles.Add(new(SizeType.AutoSize));
		AddAppearanceChoice(choices, 0, _themeChoice, "Appearance.Theme");
		AddAppearanceChoice(choices, 1, _accentChoice, "Appearance.Accent");
		AddAppearanceChoice(choices, 2, _styleChoice, "Appearance.Style");
		layout.Controls.Add(choices, 0, 2);
		_appearancePreview.Dock = DockStyle.Top;
		_appearancePreview.Height = 76;
		_appearancePreview.Margin = new Padding(0, 0, 0, 12);
		_appearancePreview.TabStop = false;
		LocalizationManager.BindAccessibleName(_appearancePreview, "Appearance.Preview");
		layout.Controls.Add(_appearancePreview, 0, 3);
		ModernSettingsButton reset = new()
		{
			Name = "appearanceReset", AutoSize = true, MinimumSize = new Size(160, 42),
			Anchor = AnchorStyles.Left, Margin = Padding.Empty, TabIndex = 3
		};
		LocalizationManager.BindText(reset, "Appearance.Reset");
		reset.Click += (_, _) => ResetAppearance();
		layout.Controls.Add(reset, 0, 4);
		_appearanceCard.Controls.Add(layout);
		foreach (ComboBox choice in new[] { _themeChoice, _accentChoice, _styleChoice })
			choice.SelectedIndexChanged += AppearanceSelectionChanged;
		ThemeManager.ThemeChanged += AppearanceThemeChanged;
		Disposed += (_, _) => ThemeManager.ThemeChanged -= AppearanceThemeChanged;
	}

	private static Label AppearanceLabel(string key, bool heading = false)
	{
		Label label = new()
		{
			Name = key.Replace('.', '_'), AutoSize = true, Dock = DockStyle.Top,
			Margin = new Padding(0, 0, 0, 6), UseMnemonic = false,
			Font = new Font("Segoe UI", heading ? 12 : 9.5f, heading ? FontStyle.Bold : FontStyle.Regular),
			ForeColor = heading ? SettingsPalette.PrimaryText : SettingsPalette.SecondaryText
		};
		LocalizationManager.BindText(label, key);
		return label;
	}

	private static void AddAppearanceChoice(TableLayoutPanel parent, int column,
		ModernSettingsComboBox choice, string key)
	{
		parent.ColumnStyles.Add(new(SizeType.Percent, 100f / 3));
		TableLayoutPanel group = new()
		{
			AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top,
			ColumnCount = 1, RowCount = 2, Margin = new Padding(0, 0, column == 2 ? 0 : 12, 0),
			TabIndex = column
		};
		group.ColumnStyles.Add(new(SizeType.Percent, 100));
		group.RowStyles.Add(new(SizeType.AutoSize));
		group.RowStyles.Add(new(SizeType.AutoSize));
		group.Controls.Add(AppearanceLabel(key), 0, 0);
		choice.Dock = DockStyle.Top;
		choice.Margin = new Padding(0, 0, 0, 6);
		choice.DropDownStyle = ComboBoxStyle.DropDownList;
		choice.TabIndex = 0;
		LocalizationManager.BindAccessibleName(choice, key);
		group.Controls.Add(choice, 0, 1);
		parent.Controls.Add(group, column, 0);
	}

	private void PopulateAppearanceChoices()
	{
		Populate(_themeChoice, ThemeCatalog.Themes, ThemeName);
		Populate(_accentChoice, ThemeCatalog.Accents, ThemeAccent);
		Populate(_styleChoice, ThemeCatalog.Styles, ThemeStyle);
		_appearancePreview.Invalidate();

		static void Populate(ModernSettingsComboBox combo, IReadOnlyList<ThemeCatalog.Choice> choices, string selected)
		{
			combo.BeginUpdate();
			try
			{
				combo.Items.Clear();
				foreach (var choice in choices) combo.Items.Add(new LocalizedOption(choice.Id, choice.ResourceKey));
				SelectOption(combo, selected);
				combo.FitDropDownToItems();
			}
			finally { combo.EndUpdate(); }
		}
	}

	private void AppearanceSelectionChanged(object? sender, EventArgs args)
	{
		if (!_updatingLocalizedOptions) AppearanceChanged?.Invoke(this, EventArgs.Empty);
		_appearancePreview.Invalidate();
	}

	private void ResetAppearance()
	{
		_updatingLocalizedOptions = true;
		try { ThemeName = "synix"; ThemeAccent = "default"; ThemeStyle = "rounded"; }
		finally { _updatingLocalizedOptions = false; }
		// Reset only the theme, keeping the user's light/dark preference.
		AppearanceChanged?.Invoke(this, EventArgs.Empty);
	}

	private void AppearanceThemeChanged(object? sender, EventArgs args) => _appearancePreview.Invalidate();

	private sealed class AppearancePreview : Control
	{
		public AppearancePreview() => SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
			ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

		protected override void OnPaint(PaintEventArgs e)
		{
			e.Graphics.Clear(SettingsPalette.Window);
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			int inset = Math.Max(6, (int)(10 * DeviceDpi / 96f));
			Rectangle card = Rectangle.Inflate(ClientRectangle, -inset, -inset);
			if (card.Width < 4 || card.Height < 4) return;
			using GraphicsPath path = RoundedGeometry.CreateThemed(card, inset);
			using SolidBrush fill = new(SettingsPalette.Card);
			using Pen border = new(SettingsPalette.Border);
			e.Graphics.FillPath(fill, path);
			e.Graphics.DrawPath(border, path);
			Rectangle badge = new(card.Right - card.Width / 3, card.Top + inset, card.Width / 3 - inset, card.Height - inset * 2);
			using GraphicsPath badgePath = RoundedGeometry.CreateThemed(badge, 6);
			using SolidBrush accent = new(SettingsPalette.Accent);
			e.Graphics.FillPath(accent, badgePath);
			TextRenderer.DrawText(e.Graphics, LocalizationManager.Get("Appearance.Preview"), Font,
				new Rectangle(card.Left + inset, card.Top, card.Width / 2, card.Height), SettingsPalette.PrimaryText,
				TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
			using Pen line = new(SettingsPalette.AccentText, 2);
			e.Graphics.DrawLine(line, badge.Left + badge.Width / 3, badge.Top + badge.Height / 2,
				badge.Right - badge.Width / 3, badge.Top + badge.Height / 2);
		}
	}
}
