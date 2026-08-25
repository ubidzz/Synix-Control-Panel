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
	internal sealed partial class TransferPasswordDialog : Form
	{
		public string TransferPassword => passwordTextBox.Text;

		public TransferPasswordDialog()
		{
			InitializeComponent();
		}

		public TransferPasswordDialog(bool confirmPassword) : this()
		{
			Text = confirmPassword
				? "Protect Synix Transfer"
				: "Open Synix Transfer";
			titleLabel.Text = confirmPassword
					? "Create a transfer password"
					: "Enter the transfer password";
			descriptionLabel.Text = confirmPassword
					? "You will need this password when moving Synix to the new PC. It cannot be recovered."
					: "Use the password that was created when this Synix package was exported.";

			int buttonTop;
			if (confirmPassword)
			{
				confirmTextBox.Visible = true;
				buttonTop = 222;
			}
			else
			{
				confirmTextBox.Visible = false;
				buttonTop = 162;
			}

			validationLabel.Location = new Point(24, buttonTop - 25);
			cancelButton.Location = new Point(232, buttonTop);
			continueButton.Location = new Point(328, buttonTop);
			continueButton.Text = confirmPassword ? "Export" : "Import";
			continueButton.Click += (_, _) => ValidateAndClose(confirmPassword);
			ClientSize = new Size(440, confirmPassword ? 282 : 222);
			ThemeManager.Apply(this);
		}

		private void ValidateAndClose(bool confirmPassword)
		{
			if (passwordTextBox.Text.Length < 8)
			{
				validationLabel.Text = "Use at least 8 characters.";
				passwordTextBox.Focus();
				return;
			}

			if (confirmPassword &&
				confirmTextBox.Text != passwordTextBox.Text)
			{
				validationLabel.Text = "The passwords do not match.";
				confirmTextBox.Focus();
				return;
			}

			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
