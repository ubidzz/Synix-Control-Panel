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

namespace Synix_Control_Panel.SynixApp.UI.Settings
{
	partial class PrivacySettingsPage
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
			chkPrivacyMode = new Synix_Control_Panel.SynixApp.Design.ModernSettingsToggle();
			settingsCardDDoS = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			cardLayoutDDoS = new TableLayoutPanel();
			settingGlyphDDoS = new Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph();
			textLayoutDDoS = new TableLayoutPanel();
			lblTitleDDoS = new Label();
			lblDescriptionDDoS = new Label();
			lblExperimentalBadge = new Label();
			chkCheckForDDoS = new Synix_Control_Panel.SynixApp.Design.ModernSettingsToggle();
			settingsCard.SuspendLayout();
			cardLayout.SuspendLayout();
			textLayout.SuspendLayout();
			settingsCardDDoS.SuspendLayout();
			cardLayoutDDoS.SuspendLayout();
			textLayoutDDoS.SuspendLayout();
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
			settingsCard.Size = new Size(818, 116);
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
			cardLayout.Controls.Add(chkPrivacyMode, 2, 0);
			cardLayout.Dock = DockStyle.Fill;
			cardLayout.Location = new Point(0, 0);
			cardLayout.Margin = new Padding(0);
			cardLayout.Name = "cardLayout";
			cardLayout.Padding = new Padding(22, 18, 20, 18);
			cardLayout.RowCount = 1;
			cardLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			cardLayout.Size = new Size(818, 116);
			cardLayout.TabIndex = 0;
			// 
			// settingGlyph
			// 
			settingGlyph.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			settingGlyph.BackColor = Color.FromArgb(17, 27, 45);
			settingGlyph.Font = new Font("Segoe UI Symbol", 15F, FontStyle.Regular, GraphicsUnit.Point);
			settingGlyph.ForeColor = Color.FromArgb(32, 214, 199);
			settingGlyph.Glyph = "◇";
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
			textLayout.Size = new Size(648, 80);
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
			lblTitle.Text = "Privacy Mode";
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
			lblDescription.Size = new Size(648, 49);
			lblDescription.TabIndex = 1;
			lblDescription.Text = "Hide IP addresses, passwords, and other sensitive information while screen sharing.";
			// 
			// chkPrivacyMode
			// 
			chkPrivacyMode.AccessibleName = "Privacy mode";
			chkPrivacyMode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			chkPrivacyMode.BackColor = Color.FromArgb(17, 27, 45);
			chkPrivacyMode.Cursor = Cursors.Hand;
			chkPrivacyMode.Location = new Point(739, 26);
			chkPrivacyMode.Margin = new Padding(0, 8, 0, 0);
			chkPrivacyMode.Name = "chkPrivacyMode";
			chkPrivacyMode.Size = new Size(54, 30);
			chkPrivacyMode.TabIndex = 2;
			chkPrivacyMode.UseVisualStyleBackColor = false;
			// 
			// settingsCardDDoS
			// 
			settingsCardDDoS.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			settingsCardDDoS.BackColor = Color.FromArgb(17, 27, 45);
			settingsCardDDoS.BorderColor = Color.FromArgb(38, 52, 77);
			settingsCardDDoS.Controls.Add(cardLayoutDDoS);
			settingsCardDDoS.CornerRadius = 13;
			settingsCardDDoS.FillColor = Color.FromArgb(17, 27, 45);
			settingsCardDDoS.Location = new Point(0, 132);
			settingsCardDDoS.Margin = new Padding(0);
			settingsCardDDoS.Name = "settingsCardDDoS";
			settingsCardDDoS.Size = new Size(818, 116);
			settingsCardDDoS.TabIndex = 1;
			// 
			// cardLayoutDDoS
			// 
			cardLayoutDDoS.BackColor = Color.FromArgb(17, 27, 45);
			cardLayoutDDoS.ColumnCount = 3;
			cardLayoutDDoS.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58F));
			cardLayoutDDoS.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			cardLayoutDDoS.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
			cardLayoutDDoS.Controls.Add(settingGlyphDDoS, 0, 0);
			cardLayoutDDoS.Controls.Add(textLayoutDDoS, 1, 0);
			cardLayoutDDoS.Controls.Add(chkCheckForDDoS, 2, 0);
			cardLayoutDDoS.Dock = DockStyle.Fill;
			cardLayoutDDoS.Location = new Point(0, 0);
			cardLayoutDDoS.Margin = new Padding(0);
			cardLayoutDDoS.Name = "cardLayoutDDoS";
			cardLayoutDDoS.Padding = new Padding(22, 18, 20, 18);
			cardLayoutDDoS.RowCount = 1;
			cardLayoutDDoS.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			cardLayoutDDoS.Size = new Size(818, 116);
			cardLayoutDDoS.TabIndex = 0;
			// 
			// settingGlyphDDoS
			// 
			settingGlyphDDoS.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			settingGlyphDDoS.BackColor = Color.FromArgb(17, 27, 45);
			settingGlyphDDoS.Font = new Font("Segoe UI Symbol", 15F, FontStyle.Regular, GraphicsUnit.Point);
			settingGlyphDDoS.ForeColor = Color.FromArgb(32, 214, 199);
			settingGlyphDDoS.Glyph = "🛡";
			settingGlyphDDoS.Location = new Point(22, 22);
			settingGlyphDDoS.Margin = new Padding(0, 4, 12, 0);
			settingGlyphDDoS.Name = "settingGlyphDDoS";
			settingGlyphDDoS.Size = new Size(42, 42);
			settingGlyphDDoS.TabIndex = 0;
			// 
			// textLayoutDDoS
			// 
			textLayoutDDoS.BackColor = Color.FromArgb(17, 27, 45);
			textLayoutDDoS.ColumnCount = 2;
			textLayoutDDoS.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
			textLayoutDDoS.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			textLayoutDDoS.Controls.Add(lblTitleDDoS, 0, 0);
			textLayoutDDoS.Controls.Add(lblExperimentalBadge, 1, 0);
			textLayoutDDoS.Controls.Add(lblDescriptionDDoS, 0, 1);
			textLayoutDDoS.Dock = DockStyle.Fill;
			textLayoutDDoS.Location = new Point(80, 18);
			textLayoutDDoS.Margin = new Padding(0);
			textLayoutDDoS.Name = "textLayoutDDoS";
			textLayoutDDoS.RowCount = 2;
			textLayoutDDoS.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
			textLayoutDDoS.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			textLayoutDDoS.Size = new Size(648, 80);
			textLayoutDDoS.TabIndex = 1;
			// 
			// lblTitleDDoS
			// 
			lblTitleDDoS.AutoSize = true;
			lblTitleDDoS.BackColor = Color.FromArgb(17, 27, 45);
			lblTitleDDoS.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
			lblTitleDDoS.ForeColor = Color.FromArgb(245, 247, 251);
			lblTitleDDoS.Location = new Point(0, 0);
			lblTitleDDoS.Margin = new Padding(0);
			lblTitleDDoS.Name = "lblTitleDDoS";
			lblTitleDDoS.Size = new Size(185, 21);
			lblTitleDDoS.TabIndex = 0;
			lblTitleDDoS.Text = "DDoS Attack Detection";
			lblTitleDDoS.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblExperimentalBadge
			// 
			lblExperimentalBadge.AutoSize = true;
			lblExperimentalBadge.BackColor = Color.FromArgb(45, 35, 15);
			lblExperimentalBadge.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold, GraphicsUnit.Point);
			lblExperimentalBadge.ForeColor = Color.FromArgb(245, 185, 76);
			lblExperimentalBadge.Location = new Point(197, 4);
			lblExperimentalBadge.Margin = new Padding(12, 4, 0, 0);
			lblExperimentalBadge.Name = "lblExperimentalBadge";
			lblExperimentalBadge.Padding = new Padding(6, 2, 6, 2);
			lblExperimentalBadge.Size = new Size(95, 17);
			lblExperimentalBadge.TabIndex = 2;
			lblExperimentalBadge.Text = "EXPERIMENTAL";
			lblExperimentalBadge.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// lblDescriptionDDoS
			// 
			lblDescriptionDDoS.AutoEllipsis = true;
			lblDescriptionDDoS.BackColor = Color.FromArgb(17, 27, 45);
			textLayoutDDoS.SetColumnSpan(lblDescriptionDDoS, 2);
			lblDescriptionDDoS.Dock = DockStyle.Fill;
			lblDescriptionDDoS.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
			lblDescriptionDDoS.ForeColor = Color.FromArgb(158, 172, 194);
			lblDescriptionDDoS.Location = new Point(0, 31);
			lblDescriptionDDoS.Margin = new Padding(0);
			lblDescriptionDDoS.Name = "lblDescriptionDDoS";
			lblDescriptionDDoS.Size = new Size(648, 49);
			lblDescriptionDDoS.TabIndex = 1;
			lblDescriptionDDoS.Text = "Monitor active server ports for incoming packet floods and notify on abnormal traffic bursts.";
			// 
			// chkCheckForDDoS
			// 
			chkCheckForDDoS.AccessibleName = "Check for DDoS";
			chkCheckForDDoS.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			chkCheckForDDoS.BackColor = Color.FromArgb(17, 27, 45);
			chkCheckForDDoS.Cursor = Cursors.Hand;
			chkCheckForDDoS.Location = new Point(739, 26);
			chkCheckForDDoS.Margin = new Padding(0, 8, 0, 0);
			chkCheckForDDoS.Name = "chkCheckForDDoS";
			chkCheckForDDoS.Size = new Size(54, 30);
			chkCheckForDDoS.TabIndex = 2;
			chkCheckForDDoS.UseVisualStyleBackColor = false;
			// 
			// PrivacySettingsPage
			// 
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			BackColor = Color.FromArgb(8, 13, 24);
			Controls.Add(settingsCardDDoS);
			Controls.Add(settingsCard);
			Name = "PrivacySettingsPage";
			Size = new Size(818, 520);
			settingsCard.ResumeLayout(false);
			cardLayout.ResumeLayout(false);
			textLayout.ResumeLayout(false);
			settingsCardDDoS.ResumeLayout(false);
			cardLayoutDDoS.ResumeLayout(false);
			textLayoutDDoS.ResumeLayout(false);
			textLayoutDDoS.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		private Synix_Control_Panel.SynixApp.Design.ModernSettingsCard settingsCard;
		private TableLayoutPanel cardLayout;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph settingGlyph;
		private TableLayoutPanel textLayout;
		private Label lblTitle;
		private Label lblDescription;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsToggle chkPrivacyMode;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsCard settingsCardDDoS;
		private TableLayoutPanel cardLayoutDDoS;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph settingGlyphDDoS;
		private TableLayoutPanel textLayoutDDoS;
		private Label lblTitleDDoS;
		private Label lblDescriptionDDoS;
		private Label lblExperimentalBadge;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsToggle chkCheckForDDoS;
	}
}
