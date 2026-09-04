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

namespace Synix_Control_Panel.SynixEngine
{
	partial class GeneralSettingsPage
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Component Designer generated code

		private void InitializeComponent()
		{
			settingsCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			cardLayout = new TableLayoutPanel();
			settingGlyph = new Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph();
			textLayout = new TableLayoutPanel();
			lblTitle = new Label();
			lblDescription = new Label();
			chkShowServerWindow = new Synix_Control_Panel.SynixApp.Design.ModernSettingsToggle();
			settingsCardDarkMode = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			cardLayoutDarkMode = new TableLayoutPanel();
			settingGlyphDarkMode = new Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph();
			textLayoutDarkMode = new TableLayoutPanel();
			lblTitleDarkMode = new Label();
			lblDescriptionDarkMode = new Label();
			chkDarkMode = new Synix_Control_Panel.SynixApp.Design.ModernSettingsToggle();
			settingsCardSteamDownloads = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			cardLayoutSteamDownloads = new TableLayoutPanel();
			settingGlyphSteamDownloads = new Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph();
			textLayoutSteamDownloads = new TableLayoutPanel();
			lblTitleSteamDownloads = new Label();
			lblDescriptionSteamDownloads = new Label();
			downloadControlsLayout = new TableLayoutPanel();
			cmbSteamCmdDownloadMode = new Synix_Control_Panel.SynixApp.Design.ModernSettingsComboBox();
			numSteamCmdDownloadLimit = new Synix_Control_Panel.SynixApp.Design.ModernSettingsNumericUpDown();
			lblSteamCmdDownloadUnit = new Label();
			settingsCardLanguage = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			cardLayoutLanguage = new TableLayoutPanel();
			settingGlyphLanguage = new Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph();
			textLayoutLanguage = new TableLayoutPanel();
			lblTitleLanguage = new Label();
			lblDescriptionLanguage = new Label();
			cmbLanguage = new Synix_Control_Panel.SynixApp.Design.ModernSettingsComboBox();
			settingsCard.SuspendLayout();
			cardLayout.SuspendLayout();
			textLayout.SuspendLayout();
			settingsCardDarkMode.SuspendLayout();
			cardLayoutDarkMode.SuspendLayout();
			textLayoutDarkMode.SuspendLayout();
			settingsCardSteamDownloads.SuspendLayout();
			cardLayoutSteamDownloads.SuspendLayout();
			textLayoutSteamDownloads.SuspendLayout();
			downloadControlsLayout.SuspendLayout();
			(numSteamCmdDownloadLimit).BeginInit();
			settingsCardLanguage.SuspendLayout();
			cardLayoutLanguage.SuspendLayout();
			textLayoutLanguage.SuspendLayout();
			SuspendLayout();
			// 
			// settingsCard
			// 
			settingsCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			settingsCard.BackColor = Color.FromArgb(17, 27, 45);
			settingsCard.BorderColor = Color.FromArgb(38, 52, 77);
			settingsCard.Controls.Add(cardLayout);
			settingsCard.CornerRadius = 13;
			settingsCard.FillColor = Color.FromArgb(17, 27, 45);
			settingsCard.Location = new Point(0, 0);
			settingsCard.Margin = new Padding(0);
			settingsCard.Name = "settingsCard";
			settingsCard.Size = new Size(818, 122);
			settingsCard.TabIndex = 0;
			// 
			// cardLayout
			// 
			cardLayout.BackColor = Color.FromArgb(17, 27, 45);
			cardLayout.ColumnCount = 3;
			cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58F));
			cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
			cardLayout.Controls.Add(settingGlyph, 0, 0);
			cardLayout.Controls.Add(textLayout, 1, 0);
			cardLayout.Controls.Add(chkShowServerWindow, 2, 0);
			cardLayout.Dock = DockStyle.Fill;
			cardLayout.Location = new Point(0, 0);
			cardLayout.Margin = new Padding(0);
			cardLayout.Name = "cardLayout";
			cardLayout.Padding = new Padding(22, 18, 20, 18);
			cardLayout.RowCount = 1;
			cardLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			cardLayout.Size = new Size(818, 122);
			cardLayout.TabIndex = 0;
			// 
			// settingGlyph
			// 
			settingGlyph.BackColor = Color.FromArgb(17, 27, 45);
			settingGlyph.Font = new Font("Segoe UI Symbol", 15F);
			settingGlyph.ForeColor = Color.FromArgb(32, 214, 199);
			settingGlyph.Glyph = ">_";
			settingGlyph.Location = new Point(22, 22);
			settingGlyph.Margin = new Padding(0, 4, 12, 0);
			settingGlyph.Name = "settingGlyph";
			settingGlyph.Size = new Size(42, 42);
			settingGlyph.TabIndex = 0;
			// 
			// textLayout
			// 
			textLayout.BackColor = Color.FromArgb(17, 27, 45);
			textLayout.ColumnCount = 1;
			textLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			textLayout.Controls.Add(lblTitle, 0, 0);
			textLayout.Controls.Add(lblDescription, 0, 1);
			textLayout.Dock = DockStyle.Fill;
			textLayout.Location = new Point(80, 18);
			textLayout.Margin = new Padding(0);
			textLayout.Name = "textLayout";
			textLayout.RowCount = 2;
			textLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
			textLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			textLayout.Size = new Size(648, 86);
			textLayout.TabIndex = 1;
			// 
			// lblTitle
			// 
			lblTitle.AutoEllipsis = true;
			lblTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblTitle.Dock = DockStyle.Fill;
			lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblTitle.Location = new Point(0, 0);
			lblTitle.Margin = new Padding(0);
			lblTitle.Name = "lblTitle";
			lblTitle.Size = new Size(648, 31);
			lblTitle.TabIndex = 0;
			lblTitle.Text = "Show Server Console Window";
			lblTitle.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblDescription
			// 
			lblDescription.AutoEllipsis = true;
			lblDescription.BackColor = Color.FromArgb(17, 27, 45);
			lblDescription.Dock = DockStyle.Fill;
			lblDescription.Font = new Font("Segoe UI", 9.5F);
			lblDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblDescription.Location = new Point(0, 31);
			lblDescription.Margin = new Padding(0);
			lblDescription.Name = "lblDescription";
			lblDescription.Size = new Size(648, 55);
			lblDescription.TabIndex = 1;
			lblDescription.Text = "Open the native console when a game server starts. Disable this to run servers silently in the background.";
			// 
			// chkShowServerWindow
			// 
			chkShowServerWindow.AccessibleName = "Show server console window";
			chkShowServerWindow.AccessibleRole = AccessibleRole.CheckButton;
			chkShowServerWindow.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			chkShowServerWindow.BackColor = Color.FromArgb(17, 27, 45);
			chkShowServerWindow.Cursor = Cursors.Hand;
			chkShowServerWindow.Location = new Point(744, 26);
			chkShowServerWindow.Margin = new Padding(0, 8, 0, 0);
			chkShowServerWindow.Name = "chkShowServerWindow";
			chkShowServerWindow.Size = new Size(54, 30);
			chkShowServerWindow.TabIndex = 2;
			chkShowServerWindow.UseVisualStyleBackColor = false;
			// 
			// settingsCardDarkMode
			// 
			settingsCardDarkMode.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			settingsCardDarkMode.BackColor = Color.FromArgb(17, 27, 45);
			settingsCardDarkMode.BorderColor = Color.FromArgb(38, 52, 77);
			settingsCardDarkMode.Controls.Add(cardLayoutDarkMode);
			settingsCardDarkMode.CornerRadius = 13;
			settingsCardDarkMode.FillColor = Color.FromArgb(17, 27, 45);
			settingsCardDarkMode.Location = new Point(0, 138);
			settingsCardDarkMode.Margin = new Padding(0);
			settingsCardDarkMode.Name = "settingsCardDarkMode";
			settingsCardDarkMode.Size = new Size(818, 116);
			settingsCardDarkMode.TabIndex = 1;
			// 
			// cardLayoutDarkMode
			// 
			cardLayoutDarkMode.BackColor = Color.FromArgb(17, 27, 45);
			cardLayoutDarkMode.ColumnCount = 3;
			cardLayoutDarkMode.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58F));
			cardLayoutDarkMode.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			cardLayoutDarkMode.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
			cardLayoutDarkMode.Controls.Add(settingGlyphDarkMode, 0, 0);
			cardLayoutDarkMode.Controls.Add(textLayoutDarkMode, 1, 0);
			cardLayoutDarkMode.Controls.Add(chkDarkMode, 2, 0);
			cardLayoutDarkMode.Dock = DockStyle.Fill;
			cardLayoutDarkMode.Location = new Point(0, 0);
			cardLayoutDarkMode.Margin = new Padding(0);
			cardLayoutDarkMode.Name = "cardLayoutDarkMode";
			cardLayoutDarkMode.Padding = new Padding(22, 18, 20, 18);
			cardLayoutDarkMode.RowCount = 1;
			cardLayoutDarkMode.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			cardLayoutDarkMode.Size = new Size(818, 116);
			cardLayoutDarkMode.TabIndex = 0;
			// 
			// settingGlyphDarkMode
			// 
			settingGlyphDarkMode.BackColor = Color.FromArgb(17, 27, 45);
			settingGlyphDarkMode.Font = new Font("Segoe UI Symbol", 15F);
			settingGlyphDarkMode.ForeColor = Color.FromArgb(32, 214, 199);
			settingGlyphDarkMode.Glyph = "◐";
			settingGlyphDarkMode.Location = new Point(22, 22);
			settingGlyphDarkMode.Margin = new Padding(0, 4, 12, 0);
			settingGlyphDarkMode.Name = "settingGlyphDarkMode";
			settingGlyphDarkMode.Size = new Size(42, 42);
			settingGlyphDarkMode.TabIndex = 0;
			// 
			// textLayoutDarkMode
			// 
			textLayoutDarkMode.BackColor = Color.FromArgb(17, 27, 45);
			textLayoutDarkMode.ColumnCount = 1;
			textLayoutDarkMode.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			textLayoutDarkMode.Controls.Add(lblTitleDarkMode, 0, 0);
			textLayoutDarkMode.Controls.Add(lblDescriptionDarkMode, 0, 1);
			textLayoutDarkMode.Dock = DockStyle.Fill;
			textLayoutDarkMode.Location = new Point(80, 18);
			textLayoutDarkMode.Margin = new Padding(0);
			textLayoutDarkMode.Name = "textLayoutDarkMode";
			textLayoutDarkMode.RowCount = 2;
			textLayoutDarkMode.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
			textLayoutDarkMode.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			textLayoutDarkMode.Size = new Size(648, 80);
			textLayoutDarkMode.TabIndex = 1;
			// 
			// lblTitleDarkMode
			// 
			lblTitleDarkMode.AutoEllipsis = true;
			lblTitleDarkMode.BackColor = Color.FromArgb(17, 27, 45);
			lblTitleDarkMode.Dock = DockStyle.Fill;
			lblTitleDarkMode.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblTitleDarkMode.ForeColor = Color.FromArgb(245, 247, 251);
			lblTitleDarkMode.Location = new Point(0, 0);
			lblTitleDarkMode.Margin = new Padding(0);
			lblTitleDarkMode.Name = "lblTitleDarkMode";
			lblTitleDarkMode.Size = new Size(648, 31);
			lblTitleDarkMode.TabIndex = 0;
			lblTitleDarkMode.Text = "Dark Mode";
			lblTitleDarkMode.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblDescriptionDarkMode
			// 
			lblDescriptionDarkMode.AutoEllipsis = true;
			lblDescriptionDarkMode.BackColor = Color.FromArgb(17, 27, 45);
			lblDescriptionDarkMode.Dock = DockStyle.Fill;
			lblDescriptionDarkMode.Font = new Font("Segoe UI", 9.5F);
			lblDescriptionDarkMode.ForeColor = Color.FromArgb(158, 172, 194);
			lblDescriptionDarkMode.Location = new Point(0, 31);
			lblDescriptionDarkMode.Margin = new Padding(0);
			lblDescriptionDarkMode.Name = "lblDescriptionDarkMode";
			lblDescriptionDarkMode.Size = new Size(648, 49);
			lblDescriptionDarkMode.TabIndex = 1;
			lblDescriptionDarkMode.Text = "Switch the Synix dashboard between light and dark visual themes.";
			// 
			// chkDarkMode
			// 
			chkDarkMode.AccessibleName = "Dark mode toggle";
			chkDarkMode.AccessibleRole = AccessibleRole.CheckButton;
			chkDarkMode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			chkDarkMode.BackColor = Color.FromArgb(17, 27, 45);
			chkDarkMode.Cursor = Cursors.Hand;
			chkDarkMode.Location = new Point(744, 26);
			chkDarkMode.Margin = new Padding(0, 8, 0, 0);
			chkDarkMode.Name = "chkDarkMode";
			chkDarkMode.Size = new Size(54, 30);
			chkDarkMode.TabIndex = 2;
			chkDarkMode.UseVisualStyleBackColor = false;
			// 
			// settingsCardSteamDownloads
			// 
			settingsCardSteamDownloads.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			settingsCardSteamDownloads.BackColor = Color.FromArgb(17, 27, 45);
			settingsCardSteamDownloads.BorderColor = Color.FromArgb(38, 52, 77);
			settingsCardSteamDownloads.Controls.Add(cardLayoutSteamDownloads);
			settingsCardSteamDownloads.CornerRadius = 13;
			settingsCardSteamDownloads.FillColor = Color.FromArgb(17, 27, 45);
			settingsCardSteamDownloads.Location = new Point(0, 270);
			settingsCardSteamDownloads.Margin = new Padding(0);
			settingsCardSteamDownloads.Name = "settingsCardSteamDownloads";
			settingsCardSteamDownloads.Size = new Size(818, 128);
			settingsCardSteamDownloads.TabIndex = 2;
			// 
			// cardLayoutSteamDownloads
			// 
			cardLayoutSteamDownloads.BackColor = Color.FromArgb(17, 27, 45);
			cardLayoutSteamDownloads.ColumnCount = 3;
			cardLayoutSteamDownloads.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58F));
			cardLayoutSteamDownloads.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			cardLayoutSteamDownloads.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 304F));
			cardLayoutSteamDownloads.Controls.Add(settingGlyphSteamDownloads, 0, 0);
			cardLayoutSteamDownloads.Controls.Add(textLayoutSteamDownloads, 1, 0);
			cardLayoutSteamDownloads.Controls.Add(downloadControlsLayout, 2, 0);
			cardLayoutSteamDownloads.Dock = DockStyle.Fill;
			cardLayoutSteamDownloads.Location = new Point(0, 0);
			cardLayoutSteamDownloads.Margin = new Padding(0);
			cardLayoutSteamDownloads.Name = "cardLayoutSteamDownloads";
			cardLayoutSteamDownloads.Padding = new Padding(22, 18, 20, 18);
			cardLayoutSteamDownloads.RowCount = 1;
			cardLayoutSteamDownloads.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			cardLayoutSteamDownloads.Size = new Size(818, 128);
			cardLayoutSteamDownloads.TabIndex = 0;
			// 
			// settingGlyphSteamDownloads
			// 
			settingGlyphSteamDownloads.BackColor = Color.FromArgb(17, 27, 45);
			settingGlyphSteamDownloads.Font = new Font("Segoe UI Symbol", 15F);
			settingGlyphSteamDownloads.ForeColor = Color.FromArgb(32, 214, 199);
			settingGlyphSteamDownloads.Glyph = "↓";
			settingGlyphSteamDownloads.Location = new Point(22, 22);
			settingGlyphSteamDownloads.Margin = new Padding(0, 4, 12, 0);
			settingGlyphSteamDownloads.Name = "settingGlyphSteamDownloads";
			settingGlyphSteamDownloads.Size = new Size(42, 42);
			settingGlyphSteamDownloads.TabIndex = 0;
			// 
			// textLayoutSteamDownloads
			// 
			textLayoutSteamDownloads.BackColor = Color.FromArgb(17, 27, 45);
			textLayoutSteamDownloads.ColumnCount = 1;
			textLayoutSteamDownloads.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			textLayoutSteamDownloads.Controls.Add(lblTitleSteamDownloads, 0, 0);
			textLayoutSteamDownloads.Controls.Add(lblDescriptionSteamDownloads, 0, 1);
			textLayoutSteamDownloads.Dock = DockStyle.Fill;
			textLayoutSteamDownloads.Location = new Point(80, 18);
			textLayoutSteamDownloads.Margin = new Padding(0);
			textLayoutSteamDownloads.Name = "textLayoutSteamDownloads";
			textLayoutSteamDownloads.RowCount = 2;
			textLayoutSteamDownloads.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
			textLayoutSteamDownloads.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			textLayoutSteamDownloads.Size = new Size(414, 92);
			textLayoutSteamDownloads.TabIndex = 1;
			// 
			// lblTitleSteamDownloads
			// 
			lblTitleSteamDownloads.AutoEllipsis = true;
			lblTitleSteamDownloads.BackColor = Color.FromArgb(17, 27, 45);
			lblTitleSteamDownloads.Dock = DockStyle.Fill;
			lblTitleSteamDownloads.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblTitleSteamDownloads.ForeColor = Color.FromArgb(245, 247, 251);
			lblTitleSteamDownloads.Location = new Point(0, 0);
			lblTitleSteamDownloads.Margin = new Padding(0);
			lblTitleSteamDownloads.Name = "lblTitleSteamDownloads";
			lblTitleSteamDownloads.Size = new Size(414, 31);
			lblTitleSteamDownloads.TabIndex = 0;
			lblTitleSteamDownloads.Text = "SteamCMD Download Speed";
			lblTitleSteamDownloads.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblDescriptionSteamDownloads
			// 
			lblDescriptionSteamDownloads.AutoEllipsis = true;
			lblDescriptionSteamDownloads.BackColor = Color.FromArgb(17, 27, 45);
			lblDescriptionSteamDownloads.Dock = DockStyle.Fill;
			lblDescriptionSteamDownloads.Font = new Font("Segoe UI", 9.5F);
			lblDescriptionSteamDownloads.ForeColor = Color.FromArgb(158, 172, 194);
			lblDescriptionSteamDownloads.Location = new Point(0, 31);
			lblDescriptionSteamDownloads.Margin = new Padding(0);
			lblDescriptionSteamDownloads.Name = "lblDescriptionSteamDownloads";
			lblDescriptionSteamDownloads.Size = new Size(414, 61);
			lblDescriptionSteamDownloads.TabIndex = 1;
			lblDescriptionSteamDownloads.Text = "Use full speed or limit game-server installs, updates, repairs, and validations.";
			// 
			// downloadControlsLayout
			// 
			downloadControlsLayout.ColumnCount = 3;
			downloadControlsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 126F));
			downloadControlsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 106F));
			downloadControlsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			downloadControlsLayout.Controls.Add(cmbSteamCmdDownloadMode, 0, 0);
			downloadControlsLayout.Controls.Add(numSteamCmdDownloadLimit, 1, 0);
			downloadControlsLayout.Controls.Add(lblSteamCmdDownloadUnit, 2, 0);
			downloadControlsLayout.Dock = DockStyle.Fill;
			downloadControlsLayout.Location = new Point(494, 18);
			downloadControlsLayout.Margin = new Padding(0);
			downloadControlsLayout.Name = "downloadControlsLayout";
			downloadControlsLayout.RowCount = 1;
			downloadControlsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			downloadControlsLayout.Size = new Size(304, 92);
			downloadControlsLayout.TabIndex = 2;
			// 
			// cmbSteamCmdDownloadMode
			// 
			cmbSteamCmdDownloadMode.AccessibleName = "SteamCMD download speed mode";
			cmbSteamCmdDownloadMode.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cmbSteamCmdDownloadMode.ArrowColor = Color.FromArgb(158, 172, 194);
			cmbSteamCmdDownloadMode.BackColor = Color.FromArgb(12, 21, 36);
			cmbSteamCmdDownloadMode.BorderColor = Color.FromArgb(38, 52, 77);
			cmbSteamCmdDownloadMode.DrawMode = DrawMode.OwnerDrawFixed;
			cmbSteamCmdDownloadMode.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbSteamCmdDownloadMode.FlatStyle = FlatStyle.Flat;
			cmbSteamCmdDownloadMode.FocusBorderColor = Color.FromArgb(38, 52, 77);
			cmbSteamCmdDownloadMode.Font = new Font("Segoe UI", 10F);
			cmbSteamCmdDownloadMode.ForeColor = Color.FromArgb(245, 247, 251);
			cmbSteamCmdDownloadMode.ItemHeight = 28;
			cmbSteamCmdDownloadMode.Location = new Point(0, 20);
			cmbSteamCmdDownloadMode.Margin = new Padding(0, 20, 10, 0);
			cmbSteamCmdDownloadMode.Name = "cmbSteamCmdDownloadMode";
			cmbSteamCmdDownloadMode.SelectedItemBackColor = Color.FromArgb(24, 55, 73);
			cmbSteamCmdDownloadMode.Size = new Size(116, 34);
			cmbSteamCmdDownloadMode.TabIndex = 0;
			// 
			// numSteamCmdDownloadLimit
			// 
			numSteamCmdDownloadLimit.AccessibleName = "SteamCMD download speed in megabits per second";
			numSteamCmdDownloadLimit.AccessibleRole = AccessibleRole.SpinButton;
			numSteamCmdDownloadLimit.BackColor = Color.FromArgb(12, 21, 36);
			numSteamCmdDownloadLimit.Font = new Font("Segoe UI", 10.5F);
			numSteamCmdDownloadLimit.ForeColor = Color.FromArgb(245, 247, 251);
			numSteamCmdDownloadLimit.Location = new Point(126, 17);
			numSteamCmdDownloadLimit.Margin = new Padding(0, 17, 10, 0);
			numSteamCmdDownloadLimit.Maximum = 10000;
			numSteamCmdDownloadLimit.Name = "numSteamCmdDownloadLimit";
			numSteamCmdDownloadLimit.Size = new Size(96, 42);
			numSteamCmdDownloadLimit.TabIndex = 1;
			numSteamCmdDownloadLimit.Value = 100;
			// 
			// lblSteamCmdDownloadUnit
			// 
			lblSteamCmdDownloadUnit.BackColor = Color.FromArgb(17, 27, 45);
			lblSteamCmdDownloadUnit.Dock = DockStyle.Fill;
			lblSteamCmdDownloadUnit.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			lblSteamCmdDownloadUnit.ForeColor = Color.White;
			lblSteamCmdDownloadUnit.Location = new Point(232, 0);
			lblSteamCmdDownloadUnit.Margin = new Padding(0);
			lblSteamCmdDownloadUnit.Name = "lblSteamCmdDownloadUnit";
			lblSteamCmdDownloadUnit.Size = new Size(72, 92);
			lblSteamCmdDownloadUnit.TabIndex = 2;
			lblSteamCmdDownloadUnit.Text = "Mbps";
			lblSteamCmdDownloadUnit.TextAlign = ContentAlignment.MiddleLeft;
			//
			// settingsCardLanguage
			//
			settingsCardLanguage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			settingsCardLanguage.BackColor = Color.FromArgb(17, 27, 45);
			settingsCardLanguage.BorderColor = Color.FromArgb(38, 52, 77);
			settingsCardLanguage.Controls.Add(cardLayoutLanguage);
			settingsCardLanguage.CornerRadius = 13;
			settingsCardLanguage.FillColor = Color.FromArgb(17, 27, 45);
			settingsCardLanguage.Location = new Point(0, 414);
			settingsCardLanguage.Margin = new Padding(0);
			settingsCardLanguage.Name = "settingsCardLanguage";
			settingsCardLanguage.Size = new Size(818, 106);
			settingsCardLanguage.TabIndex = 3;
			//
			// cardLayoutLanguage
			//
			cardLayoutLanguage.BackColor = Color.FromArgb(17, 27, 45);
			cardLayoutLanguage.ColumnCount = 3;
			cardLayoutLanguage.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58F));
			cardLayoutLanguage.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			cardLayoutLanguage.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
			cardLayoutLanguage.Controls.Add(settingGlyphLanguage, 0, 0);
			cardLayoutLanguage.Controls.Add(textLayoutLanguage, 1, 0);
			cardLayoutLanguage.Controls.Add(cmbLanguage, 2, 0);
			cardLayoutLanguage.Dock = DockStyle.Fill;
			cardLayoutLanguage.Location = new Point(0, 0);
			cardLayoutLanguage.Margin = new Padding(0);
			cardLayoutLanguage.Name = "cardLayoutLanguage";
			cardLayoutLanguage.Padding = new Padding(22, 14, 20, 14);
			cardLayoutLanguage.RowCount = 1;
			cardLayoutLanguage.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			cardLayoutLanguage.Size = new Size(818, 106);
			cardLayoutLanguage.TabIndex = 0;
			//
			// settingGlyphLanguage
			//
			settingGlyphLanguage.BackColor = Color.FromArgb(17, 27, 45);
			settingGlyphLanguage.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
			settingGlyphLanguage.ForeColor = Color.FromArgb(32, 214, 199);
			settingGlyphLanguage.Glyph = "A";
			settingGlyphLanguage.Location = new Point(22, 18);
			settingGlyphLanguage.Margin = new Padding(0, 4, 12, 0);
			settingGlyphLanguage.Name = "settingGlyphLanguage";
			settingGlyphLanguage.Size = new Size(42, 42);
			settingGlyphLanguage.TabIndex = 0;
			//
			// textLayoutLanguage
			//
			textLayoutLanguage.BackColor = Color.FromArgb(17, 27, 45);
			textLayoutLanguage.ColumnCount = 1;
			textLayoutLanguage.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			textLayoutLanguage.Controls.Add(lblTitleLanguage, 0, 0);
			textLayoutLanguage.Controls.Add(lblDescriptionLanguage, 0, 1);
			textLayoutLanguage.Dock = DockStyle.Fill;
			textLayoutLanguage.Location = new Point(80, 14);
			textLayoutLanguage.Margin = new Padding(0);
			textLayoutLanguage.Name = "textLayoutLanguage";
			textLayoutLanguage.RowCount = 2;
			textLayoutLanguage.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
			textLayoutLanguage.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			textLayoutLanguage.Size = new Size(498, 78);
			textLayoutLanguage.TabIndex = 1;
			//
			// lblTitleLanguage
			//
			lblTitleLanguage.AutoEllipsis = true;
			lblTitleLanguage.BackColor = Color.FromArgb(17, 27, 45);
			lblTitleLanguage.Dock = DockStyle.Fill;
			lblTitleLanguage.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblTitleLanguage.ForeColor = Color.FromArgb(245, 247, 251);
			lblTitleLanguage.Location = new Point(0, 0);
			lblTitleLanguage.Margin = new Padding(0);
			lblTitleLanguage.Name = "lblTitleLanguage";
			lblTitleLanguage.Size = new Size(498, 31);
			lblTitleLanguage.TabIndex = 0;
			lblTitleLanguage.Text = "Language";
			lblTitleLanguage.TextAlign = ContentAlignment.MiddleLeft;
			//
			// lblDescriptionLanguage
			//
			lblDescriptionLanguage.AutoEllipsis = true;
			lblDescriptionLanguage.BackColor = Color.FromArgb(17, 27, 45);
			lblDescriptionLanguage.Dock = DockStyle.Fill;
			lblDescriptionLanguage.Font = new Font("Segoe UI", 9.5F);
			lblDescriptionLanguage.ForeColor = Color.FromArgb(158, 172, 194);
			lblDescriptionLanguage.Location = new Point(0, 31);
			lblDescriptionLanguage.Margin = new Padding(0);
			lblDescriptionLanguage.Name = "lblDescriptionLanguage";
			lblDescriptionLanguage.Size = new Size(498, 47);
			lblDescriptionLanguage.TabIndex = 1;
			lblDescriptionLanguage.Text = "Choose the language used by Synix. Game settings and configuration values remain in English.";
			//
			// cmbLanguage
			//
			cmbLanguage.AccessibleName = "Interface language";
			cmbLanguage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cmbLanguage.ArrowColor = Color.FromArgb(158, 172, 194);
			cmbLanguage.BackColor = Color.FromArgb(12, 21, 36);
			cmbLanguage.BorderColor = Color.FromArgb(38, 52, 77);
			cmbLanguage.DrawMode = DrawMode.OwnerDrawFixed;
			cmbLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbLanguage.FlatStyle = FlatStyle.Flat;
			cmbLanguage.FocusBorderColor = Color.FromArgb(32, 214, 199);
			cmbLanguage.Font = new Font("Segoe UI", 10F);
			cmbLanguage.ForeColor = Color.FromArgb(245, 247, 251);
			cmbLanguage.ItemHeight = 28;
			cmbLanguage.Location = new Point(588, 34);
			cmbLanguage.Margin = new Padding(10, 20, 0, 0);
			cmbLanguage.Name = "cmbLanguage";
			cmbLanguage.SelectedItemBackColor = Color.FromArgb(24, 55, 73);
			cmbLanguage.Size = new Size(190, 34);
			cmbLanguage.TabIndex = 2;
			//
			// GeneralSettingsPage
			//
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			BackColor = Color.FromArgb(8, 13, 24);
			Controls.Add(settingsCardLanguage);
			Controls.Add(settingsCardSteamDownloads);
			Controls.Add(settingsCardDarkMode);
			Controls.Add(settingsCard);
			Name = "GeneralSettingsPage";
			Size = new Size(818, 520);
			settingsCard.ResumeLayout(false);
			cardLayout.ResumeLayout(false);
			textLayout.ResumeLayout(false);
			settingsCardDarkMode.ResumeLayout(false);
			cardLayoutDarkMode.ResumeLayout(false);
			textLayoutDarkMode.ResumeLayout(false);
			settingsCardSteamDownloads.ResumeLayout(false);
			cardLayoutSteamDownloads.ResumeLayout(false);
			textLayoutSteamDownloads.ResumeLayout(false);
			downloadControlsLayout.ResumeLayout(false);
			(numSteamCmdDownloadLimit).EndInit();
			settingsCardLanguage.ResumeLayout(false);
			cardLayoutLanguage.ResumeLayout(false);
			textLayoutLanguage.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private Synix_Control_Panel.SynixApp.Design.ModernSettingsCard settingsCard;
		private TableLayoutPanel cardLayout;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph settingGlyph;
		private TableLayoutPanel textLayout;
		private Label lblTitle;
		private Label lblDescription;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsToggle chkShowServerWindow;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsCard settingsCardDarkMode;
		private TableLayoutPanel cardLayoutDarkMode;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph settingGlyphDarkMode;
		private TableLayoutPanel textLayoutDarkMode;
		private Label lblTitleDarkMode;
		private Label lblDescriptionDarkMode;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsToggle chkDarkMode;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsCard settingsCardSteamDownloads;
		private TableLayoutPanel cardLayoutSteamDownloads;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph settingGlyphSteamDownloads;
		private TableLayoutPanel textLayoutSteamDownloads;
		private Label lblTitleSteamDownloads;
		private Label lblDescriptionSteamDownloads;
		private TableLayoutPanel downloadControlsLayout;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsComboBox cmbSteamCmdDownloadMode;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsNumericUpDown numSteamCmdDownloadLimit;
		private Label lblSteamCmdDownloadUnit;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsCard settingsCardLanguage;
		private TableLayoutPanel cardLayoutLanguage;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph settingGlyphLanguage;
		private TableLayoutPanel textLayoutLanguage;
		private Label lblTitleLanguage;
		private Label lblDescriptionLanguage;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsComboBox cmbLanguage;
	}
}
