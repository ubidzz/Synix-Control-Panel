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

namespace Synix_Control_Panel.SynixApp.UI.Configuration
{
	partial class ConfigurationValidationDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigurationValidationDialog));
			_titleLabel = new Label();
			_subtitleLabel = new Label();
			_summaryLabel = new Label();
			_reportBox = new RichTextBox();
			_closeButton = new ModernSettingsButton();
			_copyButton = new ModernSettingsButton();
			SuspendLayout();
			// 
			// _titleLabel
			// 
			_titleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_titleLabel.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
			_titleLabel.ForeColor = Color.FromArgb(245, 247, 251);
			_titleLabel.Location = new Point(24, 20);
			_titleLabel.Name = "_titleLabel";
			_titleLabel.Size = new Size(852, 36);
			_titleLabel.TabIndex = 0;
			_titleLabel.Text = LocalizationManager.Get("Text.5E5DD0B908E3A1075161");
			// 
			// _subtitleLabel
			// 
			_subtitleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_subtitleLabel.ForeColor = Color.FromArgb(158, 172, 194);
			_subtitleLabel.Location = new Point(24, 58);
			_subtitleLabel.Name = "_subtitleLabel";
			_subtitleLabel.Size = new Size(852, 42);
			_subtitleLabel.TabIndex = 1;
			_subtitleLabel.Text = LocalizationManager.Get("Text.69EDD9A74D913D8F77D6");
			// 
			// _summaryLabel
			// 
			_summaryLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_summaryLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
			_summaryLabel.ForeColor = Color.FromArgb(158, 172, 194);
			_summaryLabel.Location = new Point(24, 108);
			_summaryLabel.Name = "_summaryLabel";
			_summaryLabel.Size = new Size(852, 28);
			_summaryLabel.TabIndex = 2;
			_summaryLabel.Text = LocalizationManager.Get("Text.46EE37AE5CD1AE985F98");
			// 
			// _reportBox
			// 
			_reportBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_reportBox.BackColor = Color.FromArgb(12, 21, 36);
			_reportBox.BorderStyle = BorderStyle.None;
			_reportBox.DetectUrls = false;
			_reportBox.Font = new Font("Cascadia Mono", 9.5F);
			_reportBox.ForeColor = Color.FromArgb(158, 172, 194);
			_reportBox.Location = new Point(24, 144);
			_reportBox.Name = "_reportBox";
			_reportBox.ReadOnly = true;
			_reportBox.ScrollBars = RichTextBoxScrollBars.ForcedBoth;
			_reportBox.Size = new Size(852, 476);
			_reportBox.TabIndex = 3;
			_reportBox.Text = LocalizationManager.Get("Text.9364F108EA4C3D297620");
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
			_closeButton.Location = new Point(24, 636);
			_closeButton.Name = "_closeButton";
			_closeButton.Size = new Size(120, 42);
			_closeButton.TabIndex = 4;
			_closeButton.Text = LocalizationManager.Get("ModManager.Button.Close");
			_closeButton.UseVisualStyleBackColor = false;
			// 
			// _copyButton
			// 
			_copyButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_copyButton.BackColor = Color.FromArgb(12, 21, 36);
			_copyButton.FlatStyle = FlatStyle.Flat;
			_copyButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			_copyButton.ForeColor = Color.FromArgb(245, 247, 251);
			_copyButton.Location = new Point(706, 636);
			_copyButton.Name = "_copyButton";
			_copyButton.Size = new Size(170, 42);
			_copyButton.TabIndex = 5;
			_copyButton.Text = LocalizationManager.Get("Text.54B8E0C0C268C1549AF4");
			_copyButton.UseAccentStyle = true;
			_copyButton.UseVisualStyleBackColor = false;
			_copyButton.Click += CopyButton_Click;
			// 
			// ConfigurationValidationDialog
			// 
			AutoScaleDimensions = new SizeF(7F, 17F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.FromArgb(8, 13, 24);
			CancelButton = _closeButton;
			ClientSize = new Size(900, 700);
			Controls.Add(_titleLabel);
			Controls.Add(_subtitleLabel);
			Controls.Add(_summaryLabel);
			Controls.Add(_reportBox);
			Controls.Add(_closeButton);
			Controls.Add(_copyButton);
			Font = new Font("Segoe UI", 10F);
			Icon = (Icon)resources.GetObject("$this.Icon");
			MinimizeBox = false;
			MinimumSize = new Size(720, 560);
			Name = "ConfigurationValidationDialog";
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = LocalizationManager.Get("Text.2F4A3B0705E0D01E2F8F");
			ResumeLayout(false);
		}

		#endregion

		private Label _titleLabel;
		private Label _subtitleLabel;
		private Label _summaryLabel;
		private RichTextBox _reportBox;
		private ModernSettingsButton _closeButton;
		private ModernSettingsButton _copyButton;
	}
}
