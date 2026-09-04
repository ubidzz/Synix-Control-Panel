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
#pragma warning disable CS8600

namespace Synix_Control_Panel.SynixApp.UI.GameDefinitions
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameDefinitionValidationDialog));
			titleLabel = new Label();
			subtitleLabel = new Label();
			_statusLabel = new Label();
			_reportBox = new RichTextBox();
			_closeButton = new ModernSettingsButton();
			_copyButton = new ModernSettingsButton();
			_runButton = new ModernSettingsButton();
			SuspendLayout();
			// 
			// titleLabel
			// 
			titleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			titleLabel.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
			titleLabel.ForeColor = Color.FromArgb(245, 247, 251);
			titleLabel.Location = new Point(24, 20);
			titleLabel.Name = "titleLabel";
			titleLabel.Size = new Size(852, 36);
			titleLabel.TabIndex = 0;
			titleLabel.Text = "Game Definition Test Runner";
			// 
			// subtitleLabel
			// 
			subtitleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			subtitleLabel.Font = new Font("Segoe UI", 9.5F);
			subtitleLabel.ForeColor = Color.FromArgb(158, 172, 194);
			subtitleLabel.Location = new Point(24, 58);
			subtitleLabel.Name = "subtitleLabel";
			subtitleLabel.Size = new Size(852, 46);
			subtitleLabel.TabIndex = 1;
			subtitleLabel.Text = "Tests every built-in game, managed setting binding, full configuration template, revision, path, log location, and allowlisted post-install action. Installed servers are never changed.";
			// 
			// _statusLabel
			// 
			_statusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_statusLabel.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
			_statusLabel.ForeColor = Color.FromArgb(158, 172, 194);
			_statusLabel.Location = new Point(24, 112);
			_statusLabel.Name = "_statusLabel";
			_statusLabel.Size = new Size(852, 30);
			_statusLabel.TabIndex = 2;
			_statusLabel.Text = "Ready to test the built-in game-definition library.";
			// 
			// _reportBox
			// 
			_reportBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_reportBox.BackColor = Color.FromArgb(12, 21, 36);
			_reportBox.BorderStyle = BorderStyle.None;
			_reportBox.DetectUrls = false;
			_reportBox.Font = new Font("Cascadia Mono", 9.5F);
			_reportBox.ForeColor = Color.FromArgb(158, 172, 194);
			_reportBox.Location = new Point(24, 148);
			_reportBox.Name = "_reportBox";
			_reportBox.ReadOnly = true;
			_reportBox.ScrollBars = RichTextBoxScrollBars.ForcedBoth;
			_reportBox.Size = new Size(852, 480);
			_reportBox.TabIndex = 3;
			_reportBox.Text = "The validation report will appear here.";
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
			_closeButton.TabIndex = 4;
			_closeButton.Text = "Close";
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
			_copyButton.Location = new Point(558, 642);
			_copyButton.Name = "_copyButton";
			_copyButton.Size = new Size(142, 42);
			_copyButton.TabIndex = 5;
			_copyButton.Text = "Copy Report";
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
			_runButton.Location = new Point(714, 642);
			_runButton.Name = "_runButton";
			_runButton.Size = new Size(162, 42);
			_runButton.TabIndex = 6;
			_runButton.Text = "Run Tests";
			_runButton.UseAccentStyle = true;
			_runButton.UseVisualStyleBackColor = false;
			_runButton.Click += RunButton_Click;
			// 
			// GameDefinitionValidationDialog
			// 
			AutoScaleDimensions = new SizeF(7F, 17F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.FromArgb(8, 13, 24);
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
			Icon = (Icon)resources.GetObject("$this.Icon");
			MinimizeBox = false;
			MinimumSize = new Size(720, 560);
			Name = "GameDefinitionValidationDialog";
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Synix Game Definition Test Runner";
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
