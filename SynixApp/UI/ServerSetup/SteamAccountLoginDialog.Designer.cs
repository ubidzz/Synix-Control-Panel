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
namespace Synix_Control_Panel.SynixApp.UI.ServerSetup
{
	partial class SteamAccountLoginDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SteamAccountLoginDialog));
			titleLabel = new Label();
			descriptionLabel = new Label();
			accountNameLabel = new Label();
			accountNameTextBox = new TextBox();
			privacyMessageLabel = new Label();
			validationLabel = new Label();
			cancelButton = new Synix_Control_Panel.SynixApp.Design.Controls.ModernSettingsButton();
			continueButton = new Synix_Control_Panel.SynixApp.Design.Controls.ModernSettingsButton();
			SuspendLayout();
			// 
			// titleLabel
			// 
			titleLabel.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
			titleLabel.ForeColor = Color.FromArgb(245, 247, 251);
			titleLabel.Location = new Point(26, 22);
			titleLabel.Name = "titleLabel";
			titleLabel.Size = new Size(468, 34);
			titleLabel.TabIndex = 0;
			titleLabel.Text = LocalizationManager.Get("ServerSetup.SteamAccount.Required.Title");
			// 
			// descriptionLabel
			// 
			descriptionLabel.ForeColor = Color.FromArgb(158, 172, 194);
			descriptionLabel.Location = new Point(26, 62);
			descriptionLabel.Name = "descriptionLabel";
			descriptionLabel.Size = new Size(468, 60);
			descriptionLabel.TabIndex = 1;
			descriptionLabel.Text = LocalizationManager.Get("Text.44D8326D9590F71F8B1B");
			// 
			// accountNameLabel
			// 
			accountNameLabel.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
			accountNameLabel.ForeColor = Color.FromArgb(245, 247, 251);
			accountNameLabel.Location = new Point(26, 134);
			accountNameLabel.Name = "accountNameLabel";
			accountNameLabel.Size = new Size(468, 24);
			accountNameLabel.TabIndex = 2;
			accountNameLabel.Text = LocalizationManager.Get("Text.2ACE1D3304BA5649A5A1");
			// 
			// accountNameTextBox
			// 
			accountNameTextBox.BackColor = Color.FromArgb(12, 21, 36);
			accountNameTextBox.BorderStyle = BorderStyle.FixedSingle;
			accountNameTextBox.ForeColor = Color.FromArgb(245, 247, 251);
			accountNameTextBox.Location = new Point(26, 162);
			accountNameTextBox.Name = "accountNameTextBox";
			accountNameTextBox.PlaceholderText = LocalizationManager.Get("Text.74ECEE4668229DC04361");
			accountNameTextBox.Size = new Size(468, 25);
			accountNameTextBox.TabIndex = 3;
			// 
			// privacyMessageLabel
			// 
			privacyMessageLabel.ForeColor = Color.FromArgb(158, 172, 194);
			privacyMessageLabel.Location = new Point(26, 202);
			privacyMessageLabel.Name = "privacyMessageLabel";
			privacyMessageLabel.Size = new Size(468, 108);
			privacyMessageLabel.TabIndex = 4;
			privacyMessageLabel.Text = resources.GetString("privacyMessageLabel.Text");
			// 
			// validationLabel
			// 
			validationLabel.ForeColor = Color.FromArgb(250, 116, 128);
			validationLabel.Location = new Point(26, 318);
			validationLabel.Name = "validationLabel";
			validationLabel.Size = new Size(210, 24);
			validationLabel.TabIndex = 5;
			// 
			// cancelButton
			// 
			cancelButton.BackColor = Color.FromArgb(12, 21, 36);
			cancelButton.Cursor = Cursors.Hand;
			cancelButton.DialogResult = DialogResult.Cancel;
			cancelButton.FlatAppearance.BorderSize = 0;
			cancelButton.FlatStyle = FlatStyle.Flat;
			cancelButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			cancelButton.ForeColor = Color.FromArgb(245, 247, 251);
			cancelButton.Location = new Point(246, 348);
			cancelButton.Name = "cancelButton";
			cancelButton.Size = new Size(120, 40);
			cancelButton.TabIndex = 6;
			cancelButton.Text = LocalizationManager.Get("Text.19766ED6CCB2F4A32778");
			cancelButton.UseVisualStyleBackColor = false;
			// 
			// continueButton
			// 
			continueButton.BackColor = Color.FromArgb(12, 21, 36);
			continueButton.Cursor = Cursors.Hand;
			continueButton.FlatAppearance.BorderSize = 0;
			continueButton.FlatStyle = FlatStyle.Flat;
			continueButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			continueButton.ForeColor = Color.FromArgb(245, 247, 251);
			continueButton.Location = new Point(374, 348);
			continueButton.Name = "continueButton";
			continueButton.Size = new Size(120, 40);
			continueButton.TabIndex = 7;
			continueButton.Text = LocalizationManager.Get("Text.FAA71411B21242D2F5EA");
			continueButton.UseAccentStyle = true;
			continueButton.UseVisualStyleBackColor = false;
			continueButton.Click += ContinueButton_Click;
			// 
			// SteamAccountLoginDialog
			// 
			AcceptButton = continueButton;
			AutoScaleDimensions = new SizeF(7F, 17F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.FromArgb(8, 13, 24);
			CancelButton = cancelButton;
			ClientSize = new Size(520, 404);
			Controls.Add(continueButton);
			Controls.Add(cancelButton);
			Controls.Add(validationLabel);
			Controls.Add(privacyMessageLabel);
			Controls.Add(accountNameTextBox);
			Controls.Add(accountNameLabel);
			Controls.Add(descriptionLabel);
			Controls.Add(titleLabel);
			Font = new Font("Segoe UI", 10F);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "SteamAccountLoginDialog";
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = LocalizationManager.Get("ServerSetup.SteamAccount.Required.Title");
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Label titleLabel;
		private Label descriptionLabel;
		private Label accountNameLabel;
		private TextBox accountNameTextBox;
		private Label privacyMessageLabel;
		private Label validationLabel;
		private SynixApp.Design.Controls.ModernSettingsButton cancelButton;
		private SynixApp.Design.Controls.ModernSettingsButton continueButton;
	}
}
