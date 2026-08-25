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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReliabilityTestDialog));
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
			(durationInput).BeginInit();
			(intervalInput).BeginInit();
			SuspendLayout();
			// 
			// titleLabel
			// 
			titleLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
			titleLabel.ForeColor = Color.FromArgb(245, 247, 251);
			titleLabel.Location = new Point(26, 20);
			titleLabel.Name = "titleLabel";
			titleLabel.Size = new Size(810, 38);
			titleLabel.TabIndex = 0;
			titleLabel.Text = "Long-Duration Reliability Test";
			// 
			// subtitleLabel
			// 
			subtitleLabel.Font = new Font("Segoe UI", 9.5F);
			subtitleLabel.ForeColor = Color.FromArgb(158, 172, 194);
			subtitleLabel.Location = new Point(26, 60);
			subtitleLabel.Name = "subtitleLabel";
			subtitleLabel.Size = new Size(810, 44);
			subtitleLabel.TabIndex = 1;
			subtitleLabel.Text = "Repeatedly samples Synix memory, handles, threads, and the read-only server health checks. It does not start, stop, install, update, or alter a server.";
			// 
			// durationLabel
			// 
			durationLabel.ForeColor = Color.FromArgb(245, 247, 251);
			durationLabel.Location = new Point(26, 116);
			durationLabel.Name = "durationLabel";
			durationLabel.Size = new Size(130, 26);
			durationLabel.TabIndex = 2;
			durationLabel.Text = "Test duration";
			// 
			// durationInput
			// 
			durationInput.AccessibleRole = AccessibleRole.SpinButton;
			durationInput.BackColor = Color.FromArgb(12, 21, 36);
			durationInput.Font = new Font("Segoe UI", 11F);
			durationInput.ForeColor = Color.FromArgb(245, 247, 251);
			durationInput.Location = new Point(156, 108);
			durationInput.Maximum = 1440;
			durationInput.Name = "durationInput";
			durationInput.Size = new Size(110, 42);
			durationInput.TabIndex = 3;
			durationInput.Value = 30;
			// 
			// minutesLabel
			// 
			minutesLabel.ForeColor = Color.FromArgb(158, 172, 194);
			minutesLabel.Location = new Point(274, 116);
			minutesLabel.Name = "minutesLabel";
			minutesLabel.Size = new Size(72, 26);
			minutesLabel.TabIndex = 4;
			minutesLabel.Text = "minutes";
			// 
			// intervalLabel
			// 
			intervalLabel.ForeColor = Color.FromArgb(245, 247, 251);
			intervalLabel.Location = new Point(370, 116);
			intervalLabel.Name = "intervalLabel";
			intervalLabel.Size = new Size(125, 26);
			intervalLabel.TabIndex = 5;
			intervalLabel.Text = "Sample every";
			// 
			// intervalInput
			// 
			intervalInput.AccessibleRole = AccessibleRole.SpinButton;
			intervalInput.BackColor = Color.FromArgb(12, 21, 36);
			intervalInput.Font = new Font("Segoe UI", 11F);
			intervalInput.ForeColor = Color.FromArgb(245, 247, 251);
			intervalInput.Location = new Point(496, 108);
			intervalInput.Maximum = 3600;
			intervalInput.Minimum = 5;
			intervalInput.Name = "intervalInput";
			intervalInput.Size = new Size(110, 42);
			intervalInput.TabIndex = 6;
			intervalInput.Value = 30;
			// 
			// secondsLabel
			// 
			secondsLabel.ForeColor = Color.FromArgb(158, 172, 194);
			secondsLabel.Location = new Point(614, 116);
			secondsLabel.Name = "secondsLabel";
			secondsLabel.Size = new Size(72, 26);
			secondsLabel.TabIndex = 7;
			secondsLabel.Text = "seconds";
			// 
			// statusLabel
			// 
			statusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			statusLabel.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
			statusLabel.ForeColor = Color.FromArgb(158, 172, 194);
			statusLabel.Location = new Point(26, 164);
			statusLabel.Name = "statusLabel";
			statusLabel.Size = new Size(810, 28);
			statusLabel.TabIndex = 8;
			statusLabel.Text = "Ready. A 30-minute run with 30-second samples is recommended for a quick check.";
			// 
			// reportBox
			// 
			reportBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			reportBox.BackColor = Color.FromArgb(12, 21, 36);
			reportBox.BorderStyle = BorderStyle.None;
			reportBox.Font = new Font("Cascadia Mono", 9.5F);
			reportBox.ForeColor = Color.FromArgb(158, 172, 194);
			reportBox.Location = new Point(26, 198);
			reportBox.Name = "reportBox";
			reportBox.ReadOnly = true;
			reportBox.ScrollBars = RichTextBoxScrollBars.ForcedBoth;
			reportBox.Size = new Size(810, 360);
			reportBox.TabIndex = 9;
			reportBox.Text = "No reliability test has been run yet.";
			reportBox.WordWrap = false;
			// 
			// closeButton
			// 
			closeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			closeButton.BackColor = Color.FromArgb(12, 21, 36);
			closeButton.DialogResult = DialogResult.Cancel;
			closeButton.FlatStyle = FlatStyle.Flat;
			closeButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			closeButton.ForeColor = Color.FromArgb(245, 247, 251);
			closeButton.Location = new Point(26, 574);
			closeButton.Name = "closeButton";
			closeButton.Size = new Size(105, 42);
			closeButton.TabIndex = 10;
			closeButton.Text = "Close";
			closeButton.UseVisualStyleBackColor = false;
			// 
			// copyButton
			// 
			copyButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			copyButton.BackColor = Color.FromArgb(12, 21, 36);
			copyButton.Enabled = false;
			copyButton.FlatStyle = FlatStyle.Flat;
			copyButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			copyButton.ForeColor = Color.FromArgb(245, 247, 251);
			copyButton.Location = new Point(442, 574);
			copyButton.Name = "copyButton";
			copyButton.Size = new Size(125, 42);
			copyButton.TabIndex = 11;
			copyButton.Text = "Copy Report";
			copyButton.UseVisualStyleBackColor = false;
			copyButton.Click += CopyButton_Click;
			// 
			// cancelButton
			// 
			cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			cancelButton.BackColor = Color.FromArgb(12, 21, 36);
			cancelButton.Enabled = false;
			cancelButton.FlatStyle = FlatStyle.Flat;
			cancelButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			cancelButton.ForeColor = Color.FromArgb(245, 247, 251);
			cancelButton.Location = new Point(577, 574);
			cancelButton.Name = "cancelButton";
			cancelButton.Size = new Size(115, 42);
			cancelButton.TabIndex = 12;
			cancelButton.Text = "Cancel";
			cancelButton.UseVisualStyleBackColor = false;
			cancelButton.Click += CancelButton_Click;
			// 
			// startButton
			// 
			startButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			startButton.BackColor = Color.FromArgb(12, 21, 36);
			startButton.FlatStyle = FlatStyle.Flat;
			startButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			startButton.ForeColor = Color.FromArgb(245, 247, 251);
			startButton.Location = new Point(702, 574);
			startButton.Name = "startButton";
			startButton.Size = new Size(134, 42);
			startButton.TabIndex = 13;
			startButton.Text = "Start Test";
			startButton.UseAccentStyle = true;
			startButton.UseVisualStyleBackColor = false;
			startButton.Click += StartButton_Click;
			// 
			// ReliabilityTestDialog
			// 
			AutoScaleDimensions = new SizeF(7F, 17F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.FromArgb(8, 13, 24);
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
			Icon = (Icon?)resources.GetObject("$this.Icon") ?? SystemIcons.Application;
			MinimizeBox = false;
			MinimumSize = new Size(760, 580);
			Name = "ReliabilityTestDialog";
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Synix Reliability Test";
			(durationInput).EndInit();
			(intervalInput).EndInit();
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
