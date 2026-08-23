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

#nullable enable

namespace Synix_Control_Panel.SynixEngine
{
	partial class GameDefinitionValidationDialog
	{
		private System.ComponentModel.IContainer? components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				components?.Dispose();
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			titleLabel = new Label();
			subtitleLabel = new Label();
			_statusLabel = new Label();
			_reportBox = new RichTextBox();
			_closeButton = new ModernSettingsButton();
			_copyButton = new ModernSettingsButton();
			_runButton = new ModernSettingsButton();
			SuspendLayout();
			titleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			titleLabel.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
			titleLabel.ForeColor = SettingsPalette.PrimaryText;
			titleLabel.Location = new Point(24, 20);
			titleLabel.Size = new Size(852, 36);
			titleLabel.Text = "Game Definition Validator";
			subtitleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			subtitleLabel.Font = new Font("Segoe UI", 9.5F);
			subtitleLabel.ForeColor = SettingsPalette.SecondaryText;
			subtitleLabel.Location = new Point(24, 58);
			subtitleLabel.Size = new Size(852, 46);
			subtitleLabel.Text = "Checks every built-in game, full configuration template, revision, path, alias, catalog position, and allowlisted post-install action. Nothing is downloaded or executed.";
			_statusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_statusLabel.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
			_statusLabel.ForeColor = SettingsPalette.SecondaryText;
			_statusLabel.Location = new Point(24, 112);
			_statusLabel.Size = new Size(852, 30);
			_statusLabel.Text = "Ready to validate the game-definition library.";
			_reportBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_reportBox.BackColor = SettingsPalette.Input;
			_reportBox.BorderStyle = BorderStyle.None;
			_reportBox.DetectUrls = false;
			_reportBox.Font = new Font("Cascadia Mono", 9.5F);
			_reportBox.ForeColor = SettingsPalette.SecondaryText;
			_reportBox.Location = new Point(24, 148);
			_reportBox.ReadOnly = true;
			_reportBox.ScrollBars = RichTextBoxScrollBars.ForcedBoth;
			_reportBox.Size = new Size(852, 480);
			_reportBox.Text = "The validation report will appear here.";
			_reportBox.WordWrap = false;
			_closeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			_closeButton.DialogResult = DialogResult.Cancel;
			_closeButton.Location = new Point(24, 642);
			_closeButton.Size = new Size(110, 42);
			_closeButton.Text = "Close";
			_copyButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_copyButton.Enabled = false;
			_copyButton.Location = new Point(558, 642);
			_copyButton.Size = new Size(142, 42);
			_copyButton.Text = "Copy Report";
			_copyButton.Click += CopyButton_Click;
			_runButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_runButton.Location = new Point(714, 642);
			_runButton.Size = new Size(162, 42);
			_runButton.Text = "Run Validation";
			_runButton.UseAccentStyle = true;
			_runButton.Click += RunButton_Click;
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = SettingsPalette.Window;
			CancelButton = _closeButton;
			ClientSize = new Size(900, 700);
			Controls.Add(titleLabel);
			Controls.Add(subtitleLabel);
			Controls.Add(_statusLabel);
			Controls.Add(_reportBox);
			Controls.Add(_closeButton);
			Controls.Add(_copyButton);
			Controls.Add(_runButton);
			Font = new Font("Segoe UI", 10F);
			MinimizeBox = false;
			MinimumSize = new Size(720, 560);
			Name = "GameDefinitionValidationDialog";
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Synix Game Definition Validator";
			ResumeLayout(false);
		}

		private Label titleLabel = null!;
		private Label subtitleLabel = null!;
		private Label _statusLabel = null!;
		private RichTextBox _reportBox = null!;
		private ModernSettingsButton _closeButton = null!;
		private ModernSettingsButton _copyButton = null!;
		private ModernSettingsButton _runButton = null!;
	}
}
