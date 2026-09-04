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

namespace Synix_Control_Panel.SynixApp.UI.GameDefinitions
{
	partial class SynixReleaseReadinessDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SynixReleaseReadinessDialog));
			titleLabel = new Label();
			subtitleLabel = new Label();
			folderCard = new ModernSettingsCard();
			folderTitleLabel = new Label();
			_publishPathLabel = new Label();
			_browseButton = new ModernSettingsButton();
			_statusLabel = new Label();
			_reportBox = new RichTextBox();
			_closeButton = new ModernSettingsButton();
			_copyButton = new ModernSettingsButton();
			_runButton = new ModernSettingsButton();
			folderCard.SuspendLayout();
			SuspendLayout();
			// 
			// titleLabel
			// 
			titleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			titleLabel.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
			titleLabel.ForeColor = Color.FromArgb(245, 247, 251);
			titleLabel.Location = new Point(24, 20);
			titleLabel.Name = "titleLabel";
			titleLabel.Size = new Size(852, 34);
			titleLabel.TabIndex = 0;
			titleLabel.Text = LocalizationManager.Get("Text.E8986299CC046EAA3D40");
			// 
			// subtitleLabel
			// 
			subtitleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			subtitleLabel.ForeColor = Color.FromArgb(158, 172, 194);
			subtitleLabel.Location = new Point(24, 56);
			subtitleLabel.Name = "subtitleLabel";
			subtitleLabel.Size = new Size(852, 24);
			subtitleLabel.TabIndex = 1;
			subtitleLabel.Text = LocalizationManager.Get("Text.1DF3BE2046D08A780F73");
			// 
			// folderCard
			// 
			folderCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			folderCard.BackColor = Color.FromArgb(17, 27, 45);
			folderCard.BorderColor = Color.FromArgb(38, 52, 77);
			folderCard.Controls.Add(folderTitleLabel);
			folderCard.Controls.Add(_publishPathLabel);
			folderCard.Controls.Add(_browseButton);
			folderCard.FillColor = Color.FromArgb(17, 27, 45);
			folderCard.Location = new Point(24, 94);
			folderCard.Margin = new Padding(0, 0, 0, 16);
			folderCard.Name = "folderCard";
			folderCard.Size = new Size(852, 92);
			folderCard.TabIndex = 2;
			// 
			// folderTitleLabel
			// 
			folderTitleLabel.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
			folderTitleLabel.ForeColor = Color.FromArgb(245, 247, 251);
			folderTitleLabel.Location = new Point(18, 12);
			folderTitleLabel.Name = "folderTitleLabel";
			folderTitleLabel.Size = new Size(620, 25);
			folderTitleLabel.TabIndex = 0;
			folderTitleLabel.Text = LocalizationManager.Get("Text.D986018F30352387AC70");
			// 
			// _publishPathLabel
			// 
			_publishPathLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_publishPathLabel.AutoEllipsis = true;
			_publishPathLabel.ForeColor = Color.FromArgb(158, 172, 194);
			_publishPathLabel.Location = new Point(18, 44);
			_publishPathLabel.Name = "_publishPathLabel";
			_publishPathLabel.Size = new Size(635, 28);
			_publishPathLabel.TabIndex = 1;
			_publishPathLabel.Text = LocalizationManager.Get("Text.ECE11BCA5230C237A53C");
			// 
			// _browseButton
			// 
			_browseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_browseButton.BackColor = Color.FromArgb(12, 21, 36);
			_browseButton.FlatStyle = FlatStyle.Flat;
			_browseButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			_browseButton.ForeColor = Color.FromArgb(245, 247, 251);
			_browseButton.Location = new Point(688, 25);
			_browseButton.Name = "_browseButton";
			_browseButton.Size = new Size(142, 42);
			_browseButton.TabIndex = 2;
			_browseButton.Text = LocalizationManager.Get("Text.6ABB0361F0B32BEF8441");
			_browseButton.UseVisualStyleBackColor = false;
			_browseButton.Click += BrowseButton_Click;
			// 
			// _statusLabel
			// 
			_statusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_statusLabel.ForeColor = Color.FromArgb(158, 172, 194);
			_statusLabel.Location = new Point(24, 200);
			_statusLabel.Name = "_statusLabel";
			_statusLabel.Size = new Size(852, 28);
			_statusLabel.TabIndex = 3;
			_statusLabel.Text = LocalizationManager.Get("Text.2B2FF7B1710C1AE35612");
			// 
			// _reportBox
			// 
			_reportBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_reportBox.BackColor = Color.FromArgb(12, 21, 36);
			_reportBox.BorderStyle = BorderStyle.None;
			_reportBox.DetectUrls = false;
			_reportBox.Font = new Font("Cascadia Mono", 9.5F);
			_reportBox.ForeColor = Color.FromArgb(158, 172, 194);
			_reportBox.Location = new Point(24, 236);
			_reportBox.Name = "_reportBox";
			_reportBox.ReadOnly = true;
			_reportBox.ScrollBars = RichTextBoxScrollBars.ForcedBoth;
			_reportBox.Size = new Size(852, 392);
			_reportBox.TabIndex = 4;
			_reportBox.Text = LocalizationManager.Get("Text.CFA163A0DFA468756CF4");
			_reportBox.WordWrap = false;
			// 
			// _closeButton
			// 
			_closeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			_closeButton.BackColor = Color.FromArgb(12, 21, 36);
			_closeButton.DialogResult = DialogResult.Cancel;
			_closeButton.FlatStyle = FlatStyle.Flat;
			_closeButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			_closeButton.ForeColor = Color.FromArgb(245, 247, 251);
			_closeButton.Location = new Point(24, 642);
			_closeButton.Name = "_closeButton";
			_closeButton.Size = new Size(110, 42);
			_closeButton.TabIndex = 5;
			_closeButton.Text = LocalizationManager.Get("ModManager.Button.Close");
			_closeButton.UseVisualStyleBackColor = false;
			// 
			// _copyButton
			// 
			_copyButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_copyButton.BackColor = Color.FromArgb(12, 21, 36);
			_copyButton.Enabled = false;
			_copyButton.FlatStyle = FlatStyle.Flat;
			_copyButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			_copyButton.ForeColor = Color.FromArgb(245, 247, 251);
			_copyButton.Location = new Point(526, 642);
			_copyButton.Name = "_copyButton";
			_copyButton.Size = new Size(152, 42);
			_copyButton.TabIndex = 6;
			_copyButton.Text = LocalizationManager.Get("Text.54B8E0C0C268C1549AF4");
			_copyButton.UseVisualStyleBackColor = false;
			_copyButton.Click += CopyButton_Click;
			// 
			// _runButton
			// 
			_runButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_runButton.BackColor = Color.FromArgb(12, 21, 36);
			_runButton.FlatStyle = FlatStyle.Flat;
			_runButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			_runButton.ForeColor = Color.FromArgb(245, 247, 251);
			_runButton.Location = new Point(690, 642);
			_runButton.Name = "_runButton";
			_runButton.Size = new Size(186, 42);
			_runButton.TabIndex = 7;
			_runButton.Text = LocalizationManager.Get("Text.A4EEB7A5AB5896C51F91");
			_runButton.UseAccentStyle = true;
			_runButton.UseVisualStyleBackColor = false;
			_runButton.Click += RunButton_Click;
			// 
			// SynixReleaseReadinessDialog
			// 
			AutoScaleDimensions = new SizeF(7F, 17F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.FromArgb(8, 13, 24);
			CancelButton = _closeButton;
			ClientSize = new Size(900, 700);
			Controls.Add(titleLabel);
			Controls.Add(subtitleLabel);
			Controls.Add(folderCard);
			Controls.Add(_statusLabel);
			Controls.Add(_reportBox);
			Controls.Add(_closeButton);
			Controls.Add(_copyButton);
			Controls.Add(_runButton);
			Font = new Font("Segoe UI", 10F);
			Icon = (Icon)resources.GetObject("$this.Icon");
			MinimizeBox = false;
			MinimumSize = new Size(720, 560);
			Name = "SynixReleaseReadinessDialog";
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = LocalizationManager.Get("Text.3D07CCD88E27236748F3");
			folderCard.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private Label titleLabel;
		private Label subtitleLabel;
		private ModernSettingsCard folderCard;
		private Label folderTitleLabel;
		private Label _publishPathLabel;
		private ModernSettingsButton _browseButton;
		private Label _statusLabel;
		private RichTextBox _reportBox;
		private ModernSettingsButton _closeButton;
		private ModernSettingsButton _copyButton;
		private ModernSettingsButton _runButton;
	}
}
