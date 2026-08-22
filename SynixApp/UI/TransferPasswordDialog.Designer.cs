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
			titleLabel.ForeColor = SettingsPalette.PrimaryText;
			titleLabel.Location = new Point(24, 20);
			titleLabel.Name = "titleLabel";
			titleLabel.Size = new Size(392, 30);
			titleLabel.TabIndex = 0;
			titleLabel.Text = "Create a transfer password";
			// 
			// descriptionLabel
			// 
			descriptionLabel.ForeColor = SettingsPalette.SecondaryText;
			descriptionLabel.Location = new Point(24, 54);
			descriptionLabel.Name = "descriptionLabel";
			descriptionLabel.Size = new Size(392, 42);
			descriptionLabel.TabIndex = 1;
			descriptionLabel.Text = "You will need this password when moving Synix to the new PC. It cannot be recovered.";
			// 
			// passwordTextBox
			// 
			passwordTextBox.BackColor = SettingsPalette.Input;
			passwordTextBox.BorderStyle = BorderStyle.FixedSingle;
			passwordTextBox.ForeColor = SettingsPalette.PrimaryText;
			passwordTextBox.Location = new Point(24, 102);
			passwordTextBox.Name = "passwordTextBox";
			passwordTextBox.PlaceholderText = "Password (at least 8 characters)";
			passwordTextBox.Size = new Size(392, 25);
			passwordTextBox.TabIndex = 2;
			passwordTextBox.UseSystemPasswordChar = true;
			// 
			// confirmTextBox
			// 
			confirmTextBox.BackColor = SettingsPalette.Input;
			confirmTextBox.BorderStyle = BorderStyle.FixedSingle;
			confirmTextBox.ForeColor = SettingsPalette.PrimaryText;
			confirmTextBox.Location = new Point(24, 158);
			confirmTextBox.Name = "confirmTextBox";
			confirmTextBox.PlaceholderText = "Confirm password";
			confirmTextBox.Size = new Size(392, 25);
			confirmTextBox.TabIndex = 3;
			confirmTextBox.UseSystemPasswordChar = true;
			// 
			// validationLabel
			// 
			validationLabel.ForeColor = SettingsPalette.Danger;
			validationLabel.Location = new Point(24, 197);
			validationLabel.Name = "validationLabel";
			validationLabel.Size = new Size(200, 22);
			validationLabel.TabIndex = 4;
			// 
			// cancelButton
			// 
			cancelButton.DialogResult = DialogResult.Cancel;
			cancelButton.Location = new Point(232, 222);
			cancelButton.Name = "cancelButton";
			cancelButton.Size = new Size(88, 40);
			cancelButton.TabIndex = 5;
			cancelButton.Text = "Cancel";
			// 
			// continueButton
			// 
			continueButton.Location = new Point(328, 222);
			continueButton.Name = "continueButton";
			continueButton.Size = new Size(88, 40);
			continueButton.TabIndex = 6;
			continueButton.Text = "Export";
			continueButton.UseAccentStyle = true;
			// 
			// TransferPasswordDialog
			// 
			AcceptButton = continueButton;
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = SettingsPalette.Window;
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
