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
#pragma warning disable CS8600

namespace Synix_Control_Panel.SynixApp.UI.Onboarding
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FirstRunGuideDialog));
			titleLabel = new Label();
			subtitleLabel = new Label();
			guidePanel = new Panel();
			stepOne = new Label();
			stepTwo = new Label();
			stepThree = new Label();
			stepFour = new Label();
			stepFive = new Label();
			privacyLabel = new Label();
			troubleshooterButton = new ModernSettingsButton();
			finishButton = new ModernSettingsButton();
			guidePanel.SuspendLayout();
			SuspendLayout();
			// 
			// titleLabel
			// 
			titleLabel.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
			titleLabel.ForeColor = Color.FromArgb(245, 247, 251);
			titleLabel.Location = new Point(30, 24);
			titleLabel.Name = "titleLabel";
			titleLabel.Size = new Size(760, 44);
			titleLabel.TabIndex = 0;
			titleLabel.Text = LocalizationManager.Get("Text.EFA8EF66DE8A78C422CB");
			// 
			// subtitleLabel
			// 
			subtitleLabel.Font = new Font("Segoe UI", 10F);
			subtitleLabel.ForeColor = Color.FromArgb(158, 172, 194);
			subtitleLabel.Location = new Point(32, 70);
			subtitleLabel.Name = "subtitleLabel";
			subtitleLabel.Size = new Size(756, 44);
			subtitleLabel.TabIndex = 1;
			subtitleLabel.Text = LocalizationManager.Get("Text.93F24BC2C28FEF1DDC28");
			// 
			// guidePanel
			// 
			guidePanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			guidePanel.BackColor = Color.FromArgb(17, 27, 45);
			guidePanel.Controls.Add(stepOne);
			guidePanel.Controls.Add(stepTwo);
			guidePanel.Controls.Add(stepThree);
			guidePanel.Controls.Add(stepFour);
			guidePanel.Controls.Add(stepFive);
			guidePanel.Location = new Point(30, 124);
			guidePanel.Name = "guidePanel";
			guidePanel.Size = new Size(760, 330);
			guidePanel.TabIndex = 0;
			// 
			// stepOne
			// 
			stepOne.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			stepOne.BackColor = Color.FromArgb(17, 27, 45);
			stepOne.Font = new Font("Segoe UI", 9.5F);
			stepOne.ForeColor = Color.FromArgb(158, 172, 194);
			stepOne.Location = new Point(22, 18);
			stepOne.Name = "stepOne";
			stepOne.Size = new Size(716, 58);
			stepOne.TabIndex = 0;
			stepOne.Text = LocalizationManager.Get("Text.1A759326BD5A56996BF1");
			// 
			// stepTwo
			// 
			stepTwo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			stepTwo.BackColor = Color.FromArgb(17, 27, 45);
			stepTwo.Font = new Font("Segoe UI", 9.5F);
			stepTwo.ForeColor = Color.FromArgb(158, 172, 194);
			stepTwo.Location = new Point(22, 78);
			stepTwo.Name = "stepTwo";
			stepTwo.Size = new Size(716, 64);
			stepTwo.TabIndex = 1;
			stepTwo.Text = LocalizationManager.Get("Text.CD33F03B524794FFF78C");
			// 
			// stepThree
			// 
			stepThree.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			stepThree.BackColor = Color.FromArgb(17, 27, 45);
			stepThree.Font = new Font("Segoe UI", 9.5F);
			stepThree.ForeColor = Color.FromArgb(158, 172, 194);
			stepThree.Location = new Point(22, 142);
			stepThree.Name = "stepThree";
			stepThree.Size = new Size(716, 60);
			stepThree.TabIndex = 2;
			stepThree.Text = LocalizationManager.Get("Text.61A00FCEB996AF15FF21");
			// 
			// stepFour
			// 
			stepFour.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			stepFour.BackColor = Color.FromArgb(17, 27, 45);
			stepFour.Font = new Font("Segoe UI", 9.5F);
			stepFour.ForeColor = Color.FromArgb(158, 172, 194);
			stepFour.Location = new Point(22, 202);
			stepFour.Name = "stepFour";
			stepFour.Size = new Size(716, 60);
			stepFour.TabIndex = 3;
			stepFour.Text = LocalizationManager.Get("Text.CA9A7AA7C8AF17BABA8C");
			// 
			// stepFive
			// 
			stepFive.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			stepFive.BackColor = Color.FromArgb(17, 27, 45);
			stepFive.Font = new Font("Segoe UI", 9.5F);
			stepFive.ForeColor = Color.FromArgb(158, 172, 194);
			stepFive.Location = new Point(22, 262);
			stepFive.Name = "stepFive";
			stepFive.Size = new Size(716, 60);
			stepFive.TabIndex = 4;
			stepFive.Text = LocalizationManager.Get("Text.C638A97009ED3C9CCB29");
			// 
			// privacyLabel
			// 
			privacyLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			privacyLabel.Font = new Font("Segoe UI", 9F);
			privacyLabel.ForeColor = Color.FromArgb(158, 172, 194);
			privacyLabel.Location = new Point(32, 468);
			privacyLabel.Name = "privacyLabel";
			privacyLabel.Size = new Size(756, 42);
			privacyLabel.TabIndex = 2;
			privacyLabel.Text = LocalizationManager.Get("Text.030EBC77E93FA53151CE");
			// 
			// troubleshooterButton
			// 
			troubleshooterButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			troubleshooterButton.BackColor = Color.FromArgb(12, 21, 36);
			troubleshooterButton.FlatStyle = FlatStyle.Flat;
			troubleshooterButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			troubleshooterButton.ForeColor = Color.FromArgb(245, 247, 251);
			troubleshooterButton.Location = new Point(30, 520);
			troubleshooterButton.Name = "troubleshooterButton";
			troubleshooterButton.Size = new Size(176, 44);
			troubleshooterButton.TabIndex = 3;
			troubleshooterButton.Text = LocalizationManager.Get("Text.6AB14A52545C53E3EA71");
			troubleshooterButton.UseVisualStyleBackColor = false;
			troubleshooterButton.Click += TroubleshooterButton_Click;
			// 
			// finishButton
			// 
			finishButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			finishButton.BackColor = Color.FromArgb(12, 21, 36);
			finishButton.DialogResult = DialogResult.OK;
			finishButton.FlatStyle = FlatStyle.Flat;
			finishButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			finishButton.ForeColor = Color.FromArgb(245, 247, 251);
			finishButton.Location = new Point(620, 520);
			finishButton.Name = "finishButton";
			finishButton.Size = new Size(170, 44);
			finishButton.TabIndex = 4;
			finishButton.Text = LocalizationManager.Get("Text.5F194B4249C159AD665F");
			finishButton.UseAccentStyle = true;
			finishButton.UseVisualStyleBackColor = false;
			// 
			// FirstRunGuideDialog
			// 
			AcceptButton = finishButton;
			AutoScaleDimensions = new SizeF(7F, 17F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.FromArgb(8, 13, 24);
			ClientSize = new Size(820, 590);
			Controls.Add(titleLabel);
			Controls.Add(subtitleLabel);
			Controls.Add(guidePanel);
			Controls.Add(privacyLabel);
			Controls.Add(troubleshooterButton);
			Controls.Add(finishButton);
			Font = new Font("Segoe UI", 10F);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "FirstRunGuideDialog";
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = LocalizationManager.Get("Text.4C11C80313DBD04B9AFD");
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
