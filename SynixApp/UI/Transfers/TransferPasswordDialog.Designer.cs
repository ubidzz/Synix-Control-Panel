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

namespace Synix_Control_Panel.SynixApp.UI.Transfers
{
	partial class TransferPasswordDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TransferPasswordDialog));
			titleLabel = new Label();
			descriptionLabel = new Label();
			passwordTextBox = new TextBox();
			confirmTextBox = new TextBox();
			validationLabel = new Label();
			cancelButton = new ModernSettingsButton();
			continueButton = new ModernSettingsButton();
			SuspendLayout();
			// 
			// titleLabel
			// 
			titleLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
			titleLabel.ForeColor = Color.FromArgb(245, 247, 251);
			titleLabel.Location = new Point(24, 20);
			titleLabel.Name = "titleLabel";
			titleLabel.Size = new Size(392, 30);
			titleLabel.TabIndex = 0;
			titleLabel.Text = "Create a transfer password";
			// 
			// descriptionLabel
			// 
			descriptionLabel.ForeColor = Color.FromArgb(158, 172, 194);
			descriptionLabel.Location = new Point(24, 54);
			descriptionLabel.Name = "descriptionLabel";
			descriptionLabel.Size = new Size(392, 42);
			descriptionLabel.TabIndex = 1;
			descriptionLabel.Text = "You will need this password when moving Synix to the new PC. It cannot be recovered.";
			// 
			// passwordTextBox
			// 
			passwordTextBox.BackColor = Color.FromArgb(12, 21, 36);
			passwordTextBox.BorderStyle = BorderStyle.FixedSingle;
			passwordTextBox.ForeColor = Color.FromArgb(245, 247, 251);
			passwordTextBox.Location = new Point(24, 102);
			passwordTextBox.Name = "passwordTextBox";
			passwordTextBox.PlaceholderText = "Password (at least 8 characters)";
			passwordTextBox.Size = new Size(392, 25);
			passwordTextBox.TabIndex = 2;
			passwordTextBox.UseSystemPasswordChar = true;
			// 
			// confirmTextBox
			// 
			confirmTextBox.BackColor = Color.FromArgb(12, 21, 36);
			confirmTextBox.BorderStyle = BorderStyle.FixedSingle;
			confirmTextBox.ForeColor = Color.FromArgb(245, 247, 251);
			confirmTextBox.Location = new Point(24, 158);
			confirmTextBox.Name = "confirmTextBox";
			confirmTextBox.PlaceholderText = "Confirm password";
			confirmTextBox.Size = new Size(392, 25);
			confirmTextBox.TabIndex = 3;
			confirmTextBox.UseSystemPasswordChar = true;
			// 
			// validationLabel
			// 
			validationLabel.ForeColor = Color.FromArgb(250, 116, 128);
			validationLabel.Location = new Point(24, 197);
			validationLabel.Name = "validationLabel";
			validationLabel.Size = new Size(200, 22);
			validationLabel.TabIndex = 4;
			// 
			// cancelButton
			// 
			cancelButton.BackColor = Color.FromArgb(12, 21, 36);
			cancelButton.DialogResult = DialogResult.Cancel;
			cancelButton.FlatStyle = FlatStyle.Flat;
			cancelButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			cancelButton.ForeColor = Color.FromArgb(245, 247, 251);
			cancelButton.Location = new Point(232, 222);
			cancelButton.Name = "cancelButton";
			cancelButton.Size = new Size(88, 40);
			cancelButton.TabIndex = 5;
			cancelButton.Text = "Cancel";
			cancelButton.UseVisualStyleBackColor = false;
			// 
			// continueButton
			// 
			continueButton.BackColor = Color.FromArgb(12, 21, 36);
			continueButton.FlatStyle = FlatStyle.Flat;
			continueButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			continueButton.ForeColor = Color.FromArgb(245, 247, 251);
			continueButton.Location = new Point(328, 222);
			continueButton.Name = "continueButton";
			continueButton.Size = new Size(88, 40);
			continueButton.TabIndex = 6;
			continueButton.Text = "Export";
			continueButton.UseAccentStyle = true;
			continueButton.UseVisualStyleBackColor = false;
			// 
			// TransferPasswordDialog
			// 
			AcceptButton = continueButton;
			AutoScaleDimensions = new SizeF(7F, 17F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.FromArgb(8, 13, 24);
			CancelButton = cancelButton;
			ClientSize = new Size(440, 282);
			Controls.Add(titleLabel);
			Controls.Add(descriptionLabel);
			Controls.Add(passwordTextBox);
			Controls.Add(confirmTextBox);
			Controls.Add(validationLabel);
			Controls.Add(cancelButton);
			Controls.Add(continueButton);
			Font = new Font("Segoe UI", 10F);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "TransferPasswordDialog";
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Protect Synix Transfer";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Label titleLabel;
		private Label descriptionLabel;
		private TextBox passwordTextBox;
		private TextBox confirmTextBox;
		private Label validationLabel;
		private ModernSettingsButton cancelButton;
		private ModernSettingsButton continueButton;
	}
}
