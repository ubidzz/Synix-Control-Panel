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
		private System.ComponentModel.IContainer? components = null;

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
			settingsCard.SuspendLayout();
			cardLayout.SuspendLayout();
			textLayout.SuspendLayout();
			settingsCardDarkMode.SuspendLayout();
			cardLayoutDarkMode.SuspendLayout();
			textLayoutDarkMode.SuspendLayout();
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
			settingGlyph.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			settingGlyph.BackColor = Color.FromArgb(17, 27, 45);
			settingGlyph.Font = new Font("Segoe UI Symbol", 15F, FontStyle.Regular, GraphicsUnit.Point);
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
			lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
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
			lblDescription.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
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
			chkShowServerWindow.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			chkShowServerWindow.BackColor = Color.FromArgb(17, 27, 45);
			chkShowServerWindow.Cursor = Cursors.Hand;
			chkShowServerWindow.Location = new Point(739, 26);
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
			settingGlyphDarkMode.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			settingGlyphDarkMode.BackColor = Color.FromArgb(17, 27, 45);
			settingGlyphDarkMode.Font = new Font("Segoe UI Symbol", 15F, FontStyle.Regular, GraphicsUnit.Point);
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
			lblTitleDarkMode.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
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
			lblDescriptionDarkMode.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
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
			chkDarkMode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			chkDarkMode.BackColor = Color.FromArgb(17, 27, 45);
			chkDarkMode.Cursor = Cursors.Hand;
			chkDarkMode.Location = new Point(739, 26);
			chkDarkMode.Margin = new Padding(0, 8, 0, 0);
			chkDarkMode.Name = "chkDarkMode";
			chkDarkMode.Size = new Size(54, 30);
			chkDarkMode.TabIndex = 2;
			chkDarkMode.UseVisualStyleBackColor = false;
			//
			// GeneralSettingsPage
			//
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			BackColor = Color.FromArgb(8, 13, 24);
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
	}
}
