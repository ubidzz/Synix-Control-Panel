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
using System.ComponentModel;

namespace Synix_Control_Panel.SynixApp.UI.ServerSetup
{
	internal sealed partial class SteamAccountLoginDialog : Form
	{
		public string SteamAccountName => accountNameTextBox.Text.Trim();

		public SteamAccountLoginDialog()
		{
			InitializeComponent();
		}

		public SteamAccountLoginDialog(
			string gameName,
			string existingAccountName = "",
			bool restoringImportedServer = false)
			: this()
		{
			Text = restoringImportedServer
				? "Restore Steam Authorization"
				: "Steam Account Required";
			titleLabel.Text = restoringImportedServer
				? "Restore Steam authorization"
				: "Steam account required";
			descriptionLabel.Text = restoringImportedServer
				? $"{gameName} was imported to this PC. Confirm the Steam account name so SteamCMD can restore access before the first start."
				: $"{gameName} requires a Steam account for installation. Enter the account name that SteamCMD should use.";
			accountNameTextBox.Text = existingAccountName?.Trim() ?? string.Empty;

			if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
				ThemeManager.Apply(this);
		}

		protected override void OnShown(EventArgs eventArgs)
		{
			base.OnShown(eventArgs);
			accountNameTextBox.Focus();
			accountNameTextBox.SelectAll();
		}

		private void ContinueButton_Click(object? sender, EventArgs eventArgs) =>
			ValidateAndClose();

		private void ValidateAndClose()
		{
			string accountName = SteamAccountName;
			bool valid = accountName.Length is >= 3 and <= 64 &&
				accountName.All(character =>
					char.IsLetterOrDigit(character) || character == '_');

			if (!valid)
			{
				validationLabel.Text = "Enter a valid Steam account name.";
				accountNameTextBox.Focus();
				return;
			}

			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
