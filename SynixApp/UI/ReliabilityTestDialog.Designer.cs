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
	partial class ReliabilityTestDialog
	{
		private System.ComponentModel.IContainer? components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_cancellation?.Cancel();
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			titleLabel = new Label();
			subtitleLabel = new Label();
			durationLabel = new Label();
			durationInput = new ModernSettingsNumericUpDown();
			minutesLabel = new Label();
			intervalLabel = new Label();
			intervalInput = new ModernSettingsNumericUpDown();
			secondsLabel = new Label();
			statusLabel = new Label();
			reportBox = new RichTextBox();
			closeButton = new ModernSettingsButton();
			copyButton = new ModernSettingsButton();
			cancelButton = new ModernSettingsButton();
			startButton = new ModernSettingsButton();
			SuspendLayout();
			titleLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
			titleLabel.ForeColor = SettingsPalette.PrimaryText;
			titleLabel.Location = new Point(26, 20);
			titleLabel.Size = new Size(810, 38);
			titleLabel.Text = "Long-Duration Reliability Test";
			subtitleLabel.Font = new Font("Segoe UI", 9.5F);
			subtitleLabel.ForeColor = SettingsPalette.SecondaryText;
			subtitleLabel.Location = new Point(26, 60);
			subtitleLabel.Size = new Size(810, 44);
			subtitleLabel.Text = "Repeatedly samples Synix memory, handles, threads, and the read-only server health checks. It does not start, stop, install, update, or alter a server.";
			durationLabel.ForeColor = SettingsPalette.PrimaryText;
			durationLabel.Location = new Point(26, 116);
			durationLabel.Size = new Size(130, 26);
			durationLabel.Text = "Test duration";
			durationInput.Location = new Point(156, 108);
			durationInput.Minimum = 1;
			durationInput.Maximum = 1440;
			durationInput.Value = 30;
			durationInput.Size = new Size(110, 42);
			minutesLabel.ForeColor = SettingsPalette.SecondaryText;
			minutesLabel.Location = new Point(274, 116);
			minutesLabel.Size = new Size(72, 26);
			minutesLabel.Text = "minutes";
			intervalLabel.ForeColor = SettingsPalette.PrimaryText;
			intervalLabel.Location = new Point(370, 116);
			intervalLabel.Size = new Size(125, 26);
			intervalLabel.Text = "Sample every";
			intervalInput.Location = new Point(496, 108);
			intervalInput.Minimum = 5;
			intervalInput.Maximum = 3600;
			intervalInput.Value = 30;
			intervalInput.Size = new Size(110, 42);
			secondsLabel.ForeColor = SettingsPalette.SecondaryText;
			secondsLabel.Location = new Point(614, 116);
			secondsLabel.Size = new Size(72, 26);
			secondsLabel.Text = "seconds";
			statusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			statusLabel.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
			statusLabel.ForeColor = SettingsPalette.SecondaryText;
			statusLabel.Location = new Point(26, 164);
			statusLabel.Size = new Size(810, 28);
			statusLabel.Text = "Ready. A 30-minute run with 30-second samples is recommended for a quick check.";
			reportBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			reportBox.BackColor = SettingsPalette.Input;
			reportBox.BorderStyle = BorderStyle.None;
			reportBox.Font = new Font("Cascadia Mono", 9.5F);
			reportBox.ForeColor = SettingsPalette.SecondaryText;
			reportBox.Location = new Point(26, 198);
			reportBox.ReadOnly = true;
			reportBox.ScrollBars = RichTextBoxScrollBars.ForcedBoth;
			reportBox.Size = new Size(810, 360);
			reportBox.Text = "No reliability test has been run yet.";
			reportBox.WordWrap = false;
			closeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			closeButton.DialogResult = DialogResult.Cancel;
			closeButton.Location = new Point(26, 574);
			closeButton.Size = new Size(105, 42);
			closeButton.Text = "Close";
			copyButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			copyButton.Enabled = false;
			copyButton.Location = new Point(442, 574);
			copyButton.Size = new Size(125, 42);
			copyButton.Text = "Copy Report";
			copyButton.Click += CopyButton_Click;
			cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			cancelButton.Enabled = false;
			cancelButton.Location = new Point(577, 574);
			cancelButton.Size = new Size(115, 42);
			cancelButton.Text = "Cancel";
			cancelButton.Click += CancelButton_Click;
			startButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			startButton.Location = new Point(702, 574);
			startButton.Size = new Size(134, 42);
			startButton.Text = "Start Test";
			startButton.UseAccentStyle = true;
			startButton.Click += StartButton_Click;
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = SettingsPalette.Window;
			CancelButton = closeButton;
			ClientSize = new Size(862, 638);
			Controls.Add(titleLabel);
			Controls.Add(subtitleLabel);
			Controls.Add(durationLabel);
			Controls.Add(durationInput);
			Controls.Add(minutesLabel);
			Controls.Add(intervalLabel);
			Controls.Add(intervalInput);
			Controls.Add(secondsLabel);
			Controls.Add(statusLabel);
			Controls.Add(reportBox);
			Controls.Add(closeButton);
			Controls.Add(copyButton);
			Controls.Add(cancelButton);
			Controls.Add(startButton);
			Font = new Font("Segoe UI", 10F);
			MinimizeBox = false;
			MinimumSize = new Size(760, 580);
			Name = "ReliabilityTestDialog";
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Synix Reliability Test";
			ResumeLayout(false);
		}

		private Label titleLabel = null!;
		private Label subtitleLabel = null!;
		private Label durationLabel = null!;
		private ModernSettingsNumericUpDown durationInput = null!;
		private Label minutesLabel = null!;
		private Label intervalLabel = null!;
		private ModernSettingsNumericUpDown intervalInput = null!;
		private Label secondsLabel = null!;
		private Label statusLabel = null!;
		private RichTextBox reportBox = null!;
		private ModernSettingsButton closeButton = null!;
		private ModernSettingsButton copyButton = null!;
		private ModernSettingsButton cancelButton = null!;
		private ModernSettingsButton startButton = null!;
	}
}
