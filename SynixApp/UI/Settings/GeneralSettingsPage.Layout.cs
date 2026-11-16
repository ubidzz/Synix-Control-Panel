using Synix_Control_Panel.SynixApp.Design.Controls;

namespace Synix_Control_Panel.SynixApp.UI.Settings;

public partial class GeneralSettingsPage
{
	private bool _arrangingCards;
	private TableLayoutPanel? _responsiveCards;

	private void InitializeResponsiveCards()
	{
		AutoScroll = true;
		_responsiveCards = new TableLayoutPanel
		{
			Name = "generalSettingsCards", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
			Dock = DockStyle.Top, ColumnCount = 1, RowCount = 5, Margin = Padding.Empty
		};
		_responsiveCards.ColumnStyles.Add(new(SizeType.Percent, 100));
		ModernSettingsCard[] cards = [settingsCardDarkMode, _appearanceCard, settingsCard, settingsCardSteamDownloads, settingsCardLanguage];
		for (int index = 0; index < cards.Length; index++)
		{
			_responsiveCards.RowStyles.Add(new(SizeType.AutoSize));
			cards[index].Dock = DockStyle.Top;
			cards[index].TabIndex = index;
			cards[index].Margin = new Padding(0, 0, 0, 14);
			_responsiveCards.Controls.Add(cards[index], 0, index);
		}
		Controls.Add(_responsiveCards);
		foreach (Label label in new[] { lblTitle, lblDescription, lblTitleDarkMode, lblDescriptionDarkMode,
			lblTitleSteamDownloads, lblDescriptionSteamDownloads, lblTitleLanguage, lblDescriptionLanguage })
		{
			label.AutoEllipsis = false;
			label.UseMnemonic = false;
			label.TextChanged += (_, _) => ArrangeLocalizedCards();
			label.FontChanged += (_, _) => ArrangeLocalizedCards();
		}
		cmbLanguage.FontChanged += (_, _) => ArrangeLocalizedCards();
		cmbSteamCmdDownloadMode.FontChanged += (_, _) => ArrangeLocalizedCards();
		lblSteamCmdDownloadUnit.FontChanged += (_, _) => ArrangeLocalizedCards();
		_responsiveCards.SizeChanged += (_, _) => ArrangeLocalizedCards();
		DpiChangedAfterParent += (_, _) => ArrangeLocalizedCards();
		ArrangeLocalizedCards();
	}

	internal void ArrangeLocalizedCards()
	{
		if (_arrangingCards || _responsiveCards == null || IsDisposed || Disposing || _responsiveCards.Disposing) return;
		_arrangingCards = true;
		try
		{
			float scale = DeviceDpi / 96f;
			// The painted selector reserves a 34px arrow and 16px text inset.
			// Leave another 6px so the longest caption is not ellipsized.
			const int selectorChromeWidth = 56;
			int modeWidth = cmbSteamCmdDownloadMode.Items.Cast<object>()
				.Select(item => TextRenderer.MeasureText(item.ToString(), cmbSteamCmdDownloadMode.Font).Width)
				.DefaultIfEmpty(0).Max();
			int unitWidth = TextRenderer.MeasureText(lblSteamCmdDownloadUnit.Text, lblSteamCmdDownloadUnit.Font).Width;
			int downloadWidth = Math.Max(350,
				(int)Math.Ceiling((modeWidth + unitWidth + downloadControlsLayout.Margin.Horizontal
					+ cmbSteamCmdDownloadMode.Margin.Horizontal) / scale)
				+ 106 + 12 + selectorChromeWidth);
			int languageWidth = Math.Max(260, (int)Math.Ceiling((cmbLanguage.Items.Cast<object>()
				.Select(item => TextRenderer.MeasureText(item.ToString(), cmbLanguage.Font).Width)
				.DefaultIfEmpty(0).Max() + cmbLanguage.Margin.Horizontal) / scale) + selectorChromeWidth);
			ArrangeCard(settingsCard, cardLayout, textLayout, lblTitle, lblDescription, chkShowServerWindow, 70, false, scale);
			ArrangeCard(settingsCardDarkMode, cardLayoutDarkMode, textLayoutDarkMode, lblTitleDarkMode, lblDescriptionDarkMode, chkDarkMode, 70, false, scale);
			ArrangeCard(settingsCardSteamDownloads, cardLayoutSteamDownloads, textLayoutSteamDownloads,
				lblTitleSteamDownloads, lblDescriptionSteamDownloads, downloadControlsLayout, downloadWidth, true, scale);
			ArrangeCard(settingsCardLanguage, cardLayoutLanguage, textLayoutLanguage,
				lblTitleLanguage, lblDescriptionLanguage, cmbLanguage, languageWidth, true, scale);
			downloadControlsLayout.ColumnStyles[0].SizeType = SizeType.Percent;
			downloadControlsLayout.ColumnStyles[0].Width = 100;
			downloadControlsLayout.ColumnStyles[1].SizeType = SizeType.Absolute;
			downloadControlsLayout.ColumnStyles[1].Width = 106 * scale;
			downloadControlsLayout.ColumnStyles[2].SizeType = SizeType.Absolute;
			downloadControlsLayout.ColumnStyles[2].Width = unitWidth + 12 * scale;
			cmbLanguage.FitDropDownToItems();
			cmbSteamCmdDownloadMode.FitDropDownToItems();
		}
		finally { _arrangingCards = false; }
	}

	private static void ArrangeCard(ModernSettingsCard card, TableLayoutPanel layout, TableLayoutPanel text,
		Label title, Label description, Control input, int inputWidth, bool canStack, float scale)
	{
		if (card.IsDisposed || card.Disposing || input.IsDisposed) return;
		bool stacked = canStack && card.Width < Math.Max(780, 430 + inputWidth) * scale;
		layout.SuspendLayout();
		try
		{
			layout.ColumnStyles[0].Width = 58 * scale;
			layout.ColumnStyles[2].Width = stacked ? 0 : inputWidth * scale;
			layout.RowCount = stacked ? 2 : 1;
			layout.RowStyles.Clear();
			layout.SetCellPosition(input, new TableLayoutPanelCellPosition(stacked ? 1 : 2, stacked ? 1 : 0));
			layout.SetColumnSpan(input, stacked ? 2 : 1);
			layout.SetColumnSpan(text, stacked ? 2 : 1);
			int textWidth = Math.Max(60, card.DisplayRectangle.Width - layout.Padding.Horizontal - (int)(58 * scale)
				- (stacked ? 0 : (int)(inputWidth * scale)) - text.Margin.Horizontal - text.Padding.Horizontal
				- Math.Max(title.Margin.Horizontal, description.Margin.Horizontal));
			int titleHeight = TextHeight(title, textWidth) + (int)(8 * scale);
			int descriptionHeight = TextHeight(description, textWidth) + (int)(4 * scale);
			text.RowStyles[0].SizeType = SizeType.Absolute;
			text.RowStyles[0].Height = titleHeight;
			text.RowStyles[1].SizeType = SizeType.Percent;
			text.RowStyles[1].Height = 100;
			int textHeight = titleHeight + descriptionHeight;
			int inputHeight = (int)(68 * scale);
			layout.RowStyles.Add(new(SizeType.Absolute, stacked ? textHeight : Math.Max(textHeight, inputHeight)));
			if (stacked) layout.RowStyles.Add(new(SizeType.Absolute, inputHeight));
			int borderInset = card.ClientSize.Height - card.DisplayRectangle.Height;
			card.Height = borderInset + layout.Padding.Vertical + (stacked ? textHeight + inputHeight : Math.Max(textHeight, inputHeight));
		}
		finally { layout.ResumeLayout(true); }
	}

	private static int TextHeight(Label label, int width) => TextRenderer.MeasureText(label.Text, label.Font,
		new Size(width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height;
}
