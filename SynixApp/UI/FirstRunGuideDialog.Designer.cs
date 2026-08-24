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
	partial class FirstRunGuideDialog
	{
		private System.ComponentModel.IContainer? components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				components?.Dispose();
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			titleLabel = new Label();
			subtitleLabel = new Label();
			guidePanel = new Panel();
			stepFive = new Label();
			stepFour = new Label();
			stepThree = new Label();
			stepTwo = new Label();
			stepOne = new Label();
			privacyLabel = new Label();
			troubleshooterButton = new ModernSettingsButton();
			finishButton = new ModernSettingsButton();
			guidePanel.SuspendLayout();
			SuspendLayout();
			titleLabel.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
			titleLabel.ForeColor = SettingsPalette.PrimaryText;
			titleLabel.Location = new Point(30, 24);
			titleLabel.Size = new Size(760, 44);
			titleLabel.Text = "Welcome to Synix";
			subtitleLabel.Font = new Font("Segoe UI", 10F);
			subtitleLabel.ForeColor = SettingsPalette.SecondaryText;
			subtitleLabel.Location = new Point(32, 70);
			subtitleLabel.Size = new Size(756, 44);
			subtitleLabel.Text = "Synix is designed to make personal game-server hosting understandable without hiding what it changes on your computer.";
			guidePanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			guidePanel.BackColor = SettingsPalette.Card;
			guidePanel.Location = new Point(30, 124);
			guidePanel.Size = new Size(760, 330);
			guidePanel.TabIndex = 0;
			stepOne.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			stepOne.BackColor = SettingsPalette.Card;
			stepOne.Font = new Font("Segoe UI", 9.5F);
			stepOne.ForeColor = SettingsPalette.SecondaryText;
			stepOne.Location = new Point(22, 18);
			stepOne.Size = new Size(716, 58);
			stepOne.Text = "1   YOUR DATA STAYS SEPARATE\r\nServers, settings, backups, runtimes, and SteamCMD are stored under C:\\Synix so application updates do not replace them.";
			stepTwo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			stepTwo.BackColor = SettingsPalette.Card;
			stepTwo.Font = new Font("Segoe UI", 9.5F);
			stepTwo.ForeColor = SettingsPalette.SecondaryText;
			stepTwo.Location = new Point(22, 78);
			stepTwo.Size = new Size(716, 64);
			stepTwo.Text = "2   ADD A SERVER\r\nChoose a game, enter the friendly settings, and let Synix install it. Steam login is requested only when that game requires it.";
			stepThree.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			stepThree.BackColor = SettingsPalette.Card;
			stepThree.Font = new Font("Segoe UI", 9.5F);
			stepThree.ForeColor = SettingsPalette.SecondaryText;
			stepThree.Location = new Point(22, 142);
			stepThree.Size = new Size(716, 60);
			stepThree.Text = "3   START, STOP, AND VERIFY\r\nSynix shows the exact launch arguments, verifies startup, uses safe stop behavior where supported, and keeps recent logs available.";
			stepFour.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			stepFour.BackColor = SettingsPalette.Card;
			stepFour.Font = new Font("Segoe UI", 9.5F);
			stepFour.ForeColor = SettingsPalette.SecondaryText;
			stepFour.Location = new Point(22, 202);
			stepFour.Size = new Size(716, 60);
			stepFour.Text = "4   NETWORK ACCESS\r\nWindows Firewall permission and router port forwarding are different. Synix checks local conflicts, but never changes your router.";
			stepFive.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			stepFive.BackColor = SettingsPalette.Card;
			stepFive.Font = new Font("Segoe UI", 9.5F);
			stepFive.ForeColor = SettingsPalette.SecondaryText;
			stepFive.Location = new Point(22, 262);
			stepFive.Size = new Size(716, 60);
			stepFive.Text = "5   RECOVERY AND BACKUPS\r\nUse Settings > Advanced > Troubleshooter for safe health checks and repairs. Use Backups before moving Synix or making large changes.";
			guidePanel.Controls.Add(stepOne);
			guidePanel.Controls.Add(stepTwo);
			guidePanel.Controls.Add(stepThree);
			guidePanel.Controls.Add(stepFour);
			guidePanel.Controls.Add(stepFive);
			privacyLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			privacyLabel.Font = new Font("Segoe UI", 9F);
			privacyLabel.ForeColor = SettingsPalette.SecondaryText;
			privacyLabel.Location = new Point(32, 468);
			privacyLabel.Size = new Size(756, 42);
			privacyLabel.Text = "Synix does not open a public web-control port. Passwords stored by Synix are protected locally, and sensitive values are masked from its activity logs.";
			troubleshooterButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			troubleshooterButton.Location = new Point(30, 520);
			troubleshooterButton.Size = new Size(176, 44);
			troubleshooterButton.Text = "Run Health Check";
			troubleshooterButton.Click += TroubleshooterButton_Click;
			finishButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			finishButton.DialogResult = DialogResult.OK;
			finishButton.Location = new Point(620, 520);
			finishButton.Size = new Size(170, 44);
			finishButton.Text = "Start Using Synix";
			finishButton.UseAccentStyle = true;
			AcceptButton = finishButton;
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = SettingsPalette.Window;
			ClientSize = new Size(820, 590);
			Controls.Add(titleLabel);
			Controls.Add(subtitleLabel);
			Controls.Add(guidePanel);
			Controls.Add(privacyLabel);
			Controls.Add(troubleshooterButton);
			Controls.Add(finishButton);
			Font = new Font("Segoe UI", 10F);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "FirstRunGuideDialog";
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Getting Started with Synix";
			guidePanel.ResumeLayout(false);
			ResumeLayout(false);
		}

		private Label titleLabel = null!;
		private Label subtitleLabel = null!;
		private Panel guidePanel = null!;
		private Label stepOne = null!;
		private Label stepTwo = null!;
		private Label stepThree = null!;
		private Label stepFour = null!;
		private Label stepFive = null!;
		private Label privacyLabel = null!;
		private ModernSettingsButton troubleshooterButton = null!;
		private ModernSettingsButton finishButton = null!;
	}
}
