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
using Synix_Control_Panel.SynixApp.Design;

namespace Synix_Control_Panel.SynixEngine
{
	partial class SynixUpdateDialog
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				components?.Dispose();
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SynixUpdateDialog));
			titleLabel = new Label();
			subtitleLabel = new Label();
			summaryCard = new ModernSettingsCard();
			highlightsTitleLabel = new Label();
			highlightsTextBox = new RichTextBox();
			verificationLabel = new Label();
			safetyMessageLabel = new Label();
			laterButton = new ModernSettingsButton();
			githubButton = new ModernSettingsButton();
			fullNotesButton = new ModernSettingsButton();
			installButton = new ModernSettingsButton();
			summaryCard.SuspendLayout();
			SuspendLayout();
			// 
			// titleLabel
			// 
			titleLabel.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
			titleLabel.ForeColor = Color.FromArgb(245, 247, 251);
			titleLabel.Location = new Point(26, 22);
			titleLabel.Name = "titleLabel";
			titleLabel.Size = new Size(668, 34);
			titleLabel.TabIndex = 0;
			titleLabel.Text = "Synix update is available";
			// 
			// subtitleLabel
			// 
			subtitleLabel.ForeColor = Color.FromArgb(158, 172, 194);
			subtitleLabel.Location = new Point(26, 58);
			subtitleLabel.Name = "subtitleLabel";
			subtitleLabel.Size = new Size(668, 25);
			subtitleLabel.TabIndex = 1;
			subtitleLabel.Text = "Current version and installation type";
			// 
			// summaryCard
			// 
			summaryCard.BackColor = Color.FromArgb(17, 27, 45);
			summaryCard.BorderColor = Color.FromArgb(38, 52, 77);
			summaryCard.Controls.Add(highlightsTitleLabel);
			summaryCard.Controls.Add(highlightsTextBox);
			summaryCard.Controls.Add(verificationLabel);
			summaryCard.FillColor = Color.FromArgb(17, 27, 45);
			summaryCard.Location = new Point(26, 96);
			summaryCard.Margin = new Padding(0, 0, 0, 16);
			summaryCard.Name = "summaryCard";
			summaryCard.Size = new Size(668, 256);
			summaryCard.TabIndex = 2;
			// 
			// highlightsTitleLabel
			// 
			highlightsTitleLabel.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
			highlightsTitleLabel.ForeColor = Color.FromArgb(245, 247, 251);
			highlightsTitleLabel.Location = new Point(18, 14);
			highlightsTitleLabel.Name = "highlightsTitleLabel";
			highlightsTitleLabel.Size = new Size(632, 26);
			highlightsTitleLabel.TabIndex = 0;
			highlightsTitleLabel.Text = "Release highlights";
			// 
			// highlightsTextBox
			// 
			highlightsTextBox.BackColor = Color.FromArgb(12, 21, 36);
			highlightsTextBox.BorderStyle = BorderStyle.None;
			highlightsTextBox.DetectUrls = false;
			highlightsTextBox.ForeColor = Color.FromArgb(158, 172, 194);
			highlightsTextBox.Location = new Point(18, 44);
			highlightsTextBox.Name = "highlightsTextBox";
			highlightsTextBox.ReadOnly = true;
			highlightsTextBox.ScrollBars = RichTextBoxScrollBars.Vertical;
			highlightsTextBox.Size = new Size(632, 160);
			highlightsTextBox.TabIndex = 1;
			highlightsTextBox.Text = "Release notes will appear here.";
			// 
			// verificationLabel
			// 
			verificationLabel.AutoEllipsis = true;
			verificationLabel.ForeColor = Color.FromArgb(80, 230, 164);
			verificationLabel.Location = new Point(18, 215);
			verificationLabel.Name = "verificationLabel";
			verificationLabel.Size = new Size(632, 25);
			verificationLabel.TabIndex = 2;
			verificationLabel.Text = "Verified update package details";
			// 
			// safetyMessageLabel
			// 
			safetyMessageLabel.ForeColor = Color.FromArgb(158, 172, 194);
			safetyMessageLabel.Location = new Point(26, 365);
			safetyMessageLabel.Name = "safetyMessageLabel";
			safetyMessageLabel.Size = new Size(668, 52);
			safetyMessageLabel.TabIndex = 3;
			safetyMessageLabel.Text = "Update safety information appears here.";
			// 
			// laterButton
			// 
			laterButton.BackColor = Color.FromArgb(12, 21, 36);
			laterButton.DialogResult = DialogResult.Cancel;
			laterButton.FlatStyle = FlatStyle.Flat;
			laterButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			laterButton.ForeColor = Color.FromArgb(245, 247, 251);
			laterButton.Location = new Point(26, 440);
			laterButton.Name = "laterButton";
			laterButton.Size = new Size(90, 40);
			laterButton.TabIndex = 4;
			laterButton.Text = "Later";
			laterButton.UseVisualStyleBackColor = false;
			// 
			// githubButton
			// 
			githubButton.BackColor = Color.FromArgb(12, 21, 36);
			githubButton.FlatStyle = FlatStyle.Flat;
			githubButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			githubButton.ForeColor = Color.FromArgb(245, 247, 251);
			githubButton.Location = new Point(124, 440);
			githubButton.Name = "githubButton";
			githubButton.Size = new Size(116, 40);
			githubButton.TabIndex = 5;
			githubButton.Text = "Open GitHub";
			githubButton.UseVisualStyleBackColor = false;
			// 
			// fullNotesButton
			// 
			fullNotesButton.BackColor = Color.FromArgb(12, 21, 36);
			fullNotesButton.FlatStyle = FlatStyle.Flat;
			fullNotesButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			fullNotesButton.ForeColor = Color.FromArgb(245, 247, 251);
			fullNotesButton.Location = new Point(248, 440);
			fullNotesButton.Name = "fullNotesButton";
			fullNotesButton.Size = new Size(160, 40);
			fullNotesButton.TabIndex = 6;
			fullNotesButton.Text = "Full Release Notes";
			fullNotesButton.UseVisualStyleBackColor = false;
			// 
			// installButton
			// 
			installButton.BackColor = Color.FromArgb(12, 21, 36);
			installButton.FlatStyle = FlatStyle.Flat;
			installButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			installButton.ForeColor = Color.FromArgb(245, 247, 251);
			installButton.Location = new Point(530, 440);
			installButton.Name = "installButton";
			installButton.Size = new Size(164, 40);
			installButton.TabIndex = 7;
			installButton.Text = "Install Update";
			installButton.UseAccentStyle = true;
			installButton.UseVisualStyleBackColor = false;
			// 
			// SynixUpdateDialog
			// 
			AcceptButton = installButton;
			AutoScaleDimensions = new SizeF(7F, 17F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.FromArgb(8, 13, 24);
			CancelButton = laterButton;
			ClientSize = new Size(720, 500);
			Controls.Add(titleLabel);
			Controls.Add(subtitleLabel);
			Controls.Add(summaryCard);
			Controls.Add(safetyMessageLabel);
			Controls.Add(laterButton);
			Controls.Add(githubButton);
			Controls.Add(fullNotesButton);
			Controls.Add(installButton);
			Font = new Font("Segoe UI", 10F);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MaximumSize = new Size(736, 539);
			MinimizeBox = false;
			MinimumSize = new Size(736, 539);
			Name = "SynixUpdateDialog";
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Synix Update";
			summaryCard.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private Label titleLabel;
		private Label subtitleLabel;
		private ModernSettingsCard summaryCard;
		private Label highlightsTitleLabel;
		private RichTextBox highlightsTextBox;
		private Label verificationLabel;
		private Label safetyMessageLabel;
		private ModernSettingsButton laterButton;
		private ModernSettingsButton githubButton;
		private ModernSettingsButton fullNotesButton;
		private ModernSettingsButton installButton;
	}
}
