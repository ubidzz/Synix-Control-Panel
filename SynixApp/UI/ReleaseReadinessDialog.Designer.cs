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
			titleLabel.ForeColor = SettingsPalette.PrimaryText;
			titleLabel.Location = new Point(24, 20);
			titleLabel.Name = "titleLabel";
			titleLabel.Size = new Size(852, 34);
			titleLabel.TabIndex = 0;
			titleLabel.Text = "Release Readiness Checker";
			// 
			// subtitleLabel
			// 
			subtitleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			subtitleLabel.ForeColor = SettingsPalette.SecondaryText;
			subtitleLabel.Location = new Point(24, 56);
			subtitleLabel.Name = "subtitleLabel";
			subtitleLabel.Size = new Size(852, 24);
			subtitleLabel.TabIndex = 1;
			subtitleLabel.Text = "Checks the actual publish output without rebuilding Synix, starting the release, or accessing C:\\Synix.";
			// 
			// folderCard
			// 
			folderCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			folderCard.BorderColor = SettingsPalette.Border;
			folderCard.Controls.Add(folderTitleLabel);
			folderCard.Controls.Add(_publishPathLabel);
			folderCard.Controls.Add(_browseButton);
			folderCard.FillColor = SettingsPalette.Card;
			folderCard.Location = new Point(24, 94);
			folderCard.Name = "folderCard";
			folderCard.Size = new Size(852, 92);
			folderCard.TabIndex = 2;
			// 
			// folderTitleLabel
			// 
			folderTitleLabel.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
			folderTitleLabel.ForeColor = SettingsPalette.PrimaryText;
			folderTitleLabel.Location = new Point(18, 12);
			folderTitleLabel.Name = "folderTitleLabel";
			folderTitleLabel.Size = new Size(620, 25);
			folderTitleLabel.TabIndex = 0;
			folderTitleLabel.Text = "Published Synix folder";
			// 
			// _publishPathLabel
			// 
			_publishPathLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_publishPathLabel.AutoEllipsis = true;
			_publishPathLabel.ForeColor = SettingsPalette.SecondaryText;
			_publishPathLabel.Location = new Point(18, 44);
			_publishPathLabel.Name = "_publishPathLabel";
			_publishPathLabel.Size = new Size(635, 28);
			_publishPathLabel.TabIndex = 1;
			_publishPathLabel.Text = "No publish folder was detected.";
			// 
			// _browseButton
			// 
			_browseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_browseButton.Location = new Point(688, 25);
			_browseButton.Name = "_browseButton";
			_browseButton.Size = new Size(142, 42);
			_browseButton.TabIndex = 2;
			_browseButton.Text = "Choose Folder";
			_browseButton.Click += BrowseButton_Click;
			// 
			// _statusLabel
			// 
			_statusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_statusLabel.ForeColor = SettingsPalette.SecondaryText;
			_statusLabel.Location = new Point(24, 200);
			_statusLabel.Name = "_statusLabel";
			_statusLabel.Size = new Size(852, 28);
			_statusLabel.TabIndex = 3;
			_statusLabel.Text = "Ready to check the published files and the test receipt created during Publish.";
			// 
			// _reportBox
			// 
			_reportBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_reportBox.BackColor = SettingsPalette.Input;
			_reportBox.BorderStyle = BorderStyle.None;
			_reportBox.DetectUrls = false;
			_reportBox.Font = new Font("Cascadia Mono", 9.5F);
			_reportBox.ForeColor = SettingsPalette.SecondaryText;
			_reportBox.Location = new Point(24, 236);
			_reportBox.Name = "_reportBox";
			_reportBox.ReadOnly = true;
			_reportBox.ScrollBars = RichTextBoxScrollBars.ForcedBoth;
			_reportBox.Size = new Size(852, 392);
			_reportBox.TabIndex = 4;
			_reportBox.Text = "The readiness report will appear here.";
			_reportBox.WordWrap = false;
			// 
			// _closeButton
			// 
			_closeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			_closeButton.DialogResult = DialogResult.Cancel;
			_closeButton.Location = new Point(24, 642);
			_closeButton.Name = "_closeButton";
			_closeButton.Size = new Size(110, 42);
			_closeButton.TabIndex = 5;
			_closeButton.Text = "Close";
			// 
			// _copyButton
			// 
			_copyButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_copyButton.Enabled = false;
			_copyButton.Location = new Point(526, 642);
			_copyButton.Name = "_copyButton";
			_copyButton.Size = new Size(152, 42);
			_copyButton.TabIndex = 6;
			_copyButton.Text = "Copy Report";
			_copyButton.Click += CopyButton_Click;
			// 
			// _runButton
			// 
			_runButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_runButton.Location = new Point(690, 642);
			_runButton.Name = "_runButton";
			_runButton.Size = new Size(186, 42);
			_runButton.TabIndex = 7;
			_runButton.Text = "Run Release Check";
			_runButton.UseAccentStyle = true;
			_runButton.Click += RunButton_Click;
			// 
			// SynixReleaseReadinessDialog
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = SettingsPalette.Window;
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
			FormBorderStyle = FormBorderStyle.Sizable;
			MinimizeBox = false;
			MinimumSize = new Size(720, 560);
			Name = "SynixReleaseReadinessDialog";
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Synix Release Readiness Checker";
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
