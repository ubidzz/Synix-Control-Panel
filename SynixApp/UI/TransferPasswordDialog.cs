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
	internal sealed class TransferPasswordDialog : Form
	{
		private readonly TextBox _passwordTextBox;
		private readonly TextBox? _confirmTextBox;
		private readonly Label _validationLabel;

		public string TransferPassword => _passwordTextBox.Text;

		public TransferPasswordDialog(bool confirmPassword)
		{
			Text = confirmPassword
				? "Protect Synix Transfer"
				: "Open Synix Transfer";
			StartPosition = FormStartPosition.CenterParent;
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			ShowInTaskbar = false;
			ClientSize = new Size(440, confirmPassword ? 282 : 222);
			BackColor = Color.FromArgb(8, 13, 24);
			Font = new Font("Segoe UI", 10F);

			Label title = new()
			{
				AutoSize = false,
				Location = new Point(24, 20),
				Size = new Size(392, 30),
				Font = new Font("Segoe UI", 14F, FontStyle.Bold),
				ForeColor = Color.FromArgb(245, 247, 251),
				Text = confirmPassword
					? "Create a transfer password"
					: "Enter the transfer password"
			};

			Label description = new()
			{
				AutoSize = false,
				Location = new Point(24, 54),
				Size = new Size(392, 42),
				ForeColor = Color.FromArgb(158, 172, 194),
				Text = confirmPassword
					? "You will need this password when moving Synix to the new PC. It cannot be recovered."
					: "Use the password that was created when this Synix package was exported."
			};

			_passwordTextBox = CreatePasswordBox(new Point(24, 102));
			Controls.Add(title);
			Controls.Add(description);
			Controls.Add(_passwordTextBox);

			int buttonTop;
			if (confirmPassword)
			{
				_confirmTextBox = CreatePasswordBox(new Point(24, 158));
				_confirmTextBox.PlaceholderText = "Confirm password";
				Controls.Add(_confirmTextBox);
				buttonTop = 222;
			}
			else
			{
				buttonTop = 162;
			}

			_validationLabel = new Label
			{
				AutoSize = false,
				Location = new Point(24, buttonTop - 25),
				Size = new Size(220, 22),
				ForeColor = Color.FromArgb(255, 121, 121)
			};

			ModernSettingsButton cancelButton = new()
			{
				Text = "Cancel",
				Location = new Point(232, buttonTop),
				Size = new Size(88, 40),
				DialogResult = DialogResult.Cancel
			};
			ModernSettingsButton continueButton = new()
			{
				Text = confirmPassword ? "Export" : "Import",
				Location = new Point(328, buttonTop),
				Size = new Size(88, 40),
				UseAccentStyle = true
			};
			continueButton.Click += (_, _) => ValidateAndClose(confirmPassword);

			Controls.Add(_validationLabel);
			Controls.Add(cancelButton);
			Controls.Add(continueButton);

			AcceptButton = continueButton;
			CancelButton = cancelButton;
		}

		private static TextBox CreatePasswordBox(Point location)
		{
			return new TextBox
			{
				Location = location,
				Size = new Size(392, 30),
				BackColor = Color.FromArgb(17, 27, 45),
				ForeColor = Color.FromArgb(245, 247, 251),
				BorderStyle = BorderStyle.FixedSingle,
				UseSystemPasswordChar = true,
				PlaceholderText = "Password (at least 8 characters)"
			};
		}

		private void ValidateAndClose(bool confirmPassword)
		{
			if (_passwordTextBox.Text.Length < 8)
			{
				_validationLabel.Text = "Use at least 8 characters.";
				_passwordTextBox.Focus();
				return;
			}

			if (confirmPassword &&
				_confirmTextBox?.Text != _passwordTextBox.Text)
			{
				_validationLabel.Text = "The passwords do not match.";
				_confirmTextBox?.Focus();
				return;
			}

			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
