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
	internal sealed partial class TransferPasswordDialog : Form
	{
		public string TransferPassword => passwordTextBox.Text;

		public TransferPasswordDialog()
		{
			InitializeComponent();
		}

		public TransferPasswordDialog(bool confirmPassword) : this()
		{
			Text = LocalizationManager.Get(confirmPassword
				? "TransferPassword.Protect.WindowTitle"
				: "TransferPassword.Open.WindowTitle");
			LocalizationManager.BindText(
				titleLabel,
				confirmPassword
					? "TransferPassword.Protect.Title"
					: "TransferPassword.Open.Title");
			LocalizationManager.BindText(
				descriptionLabel,
				confirmPassword
					? "TransferPassword.Protect.Description"
					: "TransferPassword.Open.Description");

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
			LocalizationManager.BindText(
				continueButton,
				confirmPassword
					? "TransferPassword.Button.Export"
					: "TransferPassword.Button.Import");
			continueButton.Click += (_, _) => ValidateAndClose(confirmPassword);
			ClientSize = new Size(440, confirmPassword ? 282 : 222);
			ThemeManager.Apply(this);
		}

		private void ValidateAndClose(bool confirmPassword)
		{
			if (passwordTextBox.Text.Length < 8)
			{
				LocalizationManager.BindText(
					validationLabel,
					"Text.9B15CE9E25E944D6F42D");
				passwordTextBox.Focus();
				return;
			}

			if (confirmPassword &&
				confirmTextBox.Text != passwordTextBox.Text)
			{
				LocalizationManager.BindText(
					validationLabel,
					"Text.F694AAA8DA2C0516E83B");
				confirmTextBox.Focus();
				return;
			}

			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
