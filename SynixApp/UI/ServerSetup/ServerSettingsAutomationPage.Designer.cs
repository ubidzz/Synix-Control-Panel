// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Design;

namespace Synix_Control_Panel.SynixApp.UI.ServerSetup
{
	partial class ServerSettingsAutomationPage
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				components?.Dispose();

			base.Dispose(disposing);
		}

		#region Component Designer generated code

		private void InitializeComponent()
		{
			cardStartup = new ModernSettingsCard();
			lblStartupIcon = new Label();
			lblStartupTitle = new Label();
			lblUpdateTitle = new Label();
			lblUpdateDescription = new Label();
			chkUpdateOnStart = new ModernSettingsToggle();
			lblBackupTitle = new Label();
			lblBackupDescription = new Label();
			chkBackupOnStart = new ModernSettingsToggle();
			cardSchedule = new ModernSettingsCard();
			lblScheduleIcon = new Label();
			lblScheduleTitle = new Label();
			lblScheduleDescription = new Label();
			chkEnableSchedule = new ModernSettingsToggle();
			btnEditSchedule = new ModernSettingsButton();
			cardStartup.SuspendLayout();
			cardSchedule.SuspendLayout();
			SuspendLayout();
			// cardStartup
			cardStartup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardStartup.BackColor = Color.FromArgb(17, 27, 45);
			cardStartup.BorderColor = Color.FromArgb(38, 52, 77);
			cardStartup.Controls.Add(lblStartupIcon);
			cardStartup.Controls.Add(lblStartupTitle);
			cardStartup.Controls.Add(lblUpdateTitle);
			cardStartup.Controls.Add(lblUpdateDescription);
			cardStartup.Controls.Add(chkUpdateOnStart);
			cardStartup.Controls.Add(lblBackupTitle);
			cardStartup.Controls.Add(lblBackupDescription);
			cardStartup.Controls.Add(chkBackupOnStart);
			cardStartup.CornerRadius = 12;
			cardStartup.FillColor = Color.FromArgb(17, 27, 45);
			cardStartup.Location = new Point(0, 0);
			cardStartup.Name = "cardStartup";
			cardStartup.Size = new Size(914, 142);
			cardStartup.TabIndex = 0;

			// lblStartupIcon
			lblStartupIcon.BackColor = Color.FromArgb(17, 27, 45);
			lblStartupIcon.Font = new Font("Segoe UI Symbol", 16F);
			lblStartupIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblStartupIcon.Location = new Point(20, 14);
			lblStartupIcon.Name = "lblStartupIcon";
			lblStartupIcon.Size = new Size(28, 30);
			lblStartupIcon.TabIndex = 0;
			lblStartupIcon.Text = LocalizationManager.Get("Text.E5235A4A75E63AAA9740");
			lblStartupIcon.TextAlign = ContentAlignment.MiddleCenter;

			// lblStartupTitle
			lblStartupTitle.AutoSize = true;
			lblStartupTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblStartupTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblStartupTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblStartupTitle.Location = new Point(54, 19);
			lblStartupTitle.Name = "lblStartupTitle";
			lblStartupTitle.Size = new Size(114, 21);
			lblStartupTitle.TabIndex = 1;
			lblStartupTitle.Text = LocalizationManager.Get("Text.835D3FF459F432C23A57");

			// lblUpdateTitle
			lblUpdateTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblUpdateTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblUpdateTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblUpdateTitle.Location = new Point(24, 62);
			lblUpdateTitle.Name = "lblUpdateTitle";
			lblUpdateTitle.Size = new Size(180, 20);
			lblUpdateTitle.TabIndex = 2;
			lblUpdateTitle.Text = LocalizationManager.Get("ServerSetup.Automation.UpdateOnStart.AccessibleName");

			// lblUpdateDescription
			lblUpdateDescription.BackColor = Color.FromArgb(17, 27, 45);
			lblUpdateDescription.Font = new Font("Segoe UI", 8F);
			lblUpdateDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblUpdateDescription.Location = new Point(24, 84);
			lblUpdateDescription.Name = "lblUpdateDescription";
			lblUpdateDescription.Size = new Size(340, 34);
			lblUpdateDescription.TabIndex = 3;
			lblUpdateDescription.Text = LocalizationManager.Get("Text.37923AD83E636BE4C27F");

			// chkUpdateOnStart
			chkUpdateOnStart.BackColor = Color.FromArgb(17, 27, 45);
			chkUpdateOnStart.Location = new Point(374, 70);
			chkUpdateOnStart.Name = "chkUpdateOnStart";
			chkUpdateOnStart.Size = new Size(54, 30);
			chkUpdateOnStart.TabIndex = 4;

			// lblBackupTitle
			lblBackupTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblBackupTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblBackupTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblBackupTitle.Location = new Point(472, 62);
			lblBackupTitle.Name = "lblBackupTitle";
			lblBackupTitle.Size = new Size(180, 20);
			lblBackupTitle.TabIndex = 5;
			lblBackupTitle.Text = LocalizationManager.Get("ServerSetup.Automation.BackupOnStart.AccessibleName");

			// lblBackupDescription
			lblBackupDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblBackupDescription.BackColor = Color.FromArgb(17, 27, 45);
			lblBackupDescription.Font = new Font("Segoe UI", 8F);
			lblBackupDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblBackupDescription.Location = new Point(472, 84);
			lblBackupDescription.Name = "lblBackupDescription";
			lblBackupDescription.Size = new Size(340, 34);
			lblBackupDescription.TabIndex = 6;
			lblBackupDescription.Text = LocalizationManager.Get("Text.2196E21E8E2F8ABE522B");

			// chkBackupOnStart
			chkBackupOnStart.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			chkBackupOnStart.BackColor = Color.FromArgb(17, 27, 45);
			chkBackupOnStart.Location = new Point(836, 70);
			chkBackupOnStart.Name = "chkBackupOnStart";
			chkBackupOnStart.Size = new Size(54, 30);
			chkBackupOnStart.TabIndex = 7;

			// cardSchedule
			cardSchedule.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardSchedule.BackColor = Color.FromArgb(17, 27, 45);
			cardSchedule.BorderColor = Color.FromArgb(38, 52, 77);
			cardSchedule.Controls.Add(lblScheduleIcon);
			cardSchedule.Controls.Add(lblScheduleTitle);
			cardSchedule.Controls.Add(lblScheduleDescription);
			cardSchedule.Controls.Add(chkEnableSchedule);
			cardSchedule.Controls.Add(btnEditSchedule);
			cardSchedule.CornerRadius = 12;
			cardSchedule.FillColor = Color.FromArgb(17, 27, 45);
			cardSchedule.Location = new Point(0, 158);
			cardSchedule.Name = "cardSchedule";
			cardSchedule.Size = new Size(914, 118);
			cardSchedule.TabIndex = 1;

			// lblScheduleIcon
			lblScheduleIcon.BackColor = Color.FromArgb(17, 27, 45);
			lblScheduleIcon.Font = new Font("Segoe UI Symbol", 16F);
			lblScheduleIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblScheduleIcon.Location = new Point(20, 14);
			lblScheduleIcon.Name = "lblScheduleIcon";
			lblScheduleIcon.Size = new Size(28, 30);
			lblScheduleIcon.TabIndex = 0;
			lblScheduleIcon.Text = LocalizationManager.Get("Text.F9F2B6CC304F2C8B6643");
			lblScheduleIcon.TextAlign = ContentAlignment.MiddleCenter;

			// lblScheduleTitle
			lblScheduleTitle.AutoSize = true;
			lblScheduleTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblScheduleTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblScheduleTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblScheduleTitle.Location = new Point(54, 19);
			lblScheduleTitle.Name = "lblScheduleTitle";
			lblScheduleTitle.Size = new Size(151, 21);
			lblScheduleTitle.TabIndex = 1;
			lblScheduleTitle.Text = LocalizationManager.Get("Text.0637B6730F1724433551");

			// lblScheduleDescription
			lblScheduleDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblScheduleDescription.BackColor = Color.FromArgb(17, 27, 45);
			lblScheduleDescription.Font = new Font("Segoe UI", 8.5F);
			lblScheduleDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblScheduleDescription.Location = new Point(24, 58);
			lblScheduleDescription.Name = "lblScheduleDescription";
			lblScheduleDescription.Size = new Size(560, 38);
			lblScheduleDescription.TabIndex = 2;
			lblScheduleDescription.Text = LocalizationManager.Get("Text.2D41AABA41C2EF3FED5C");

			// chkEnableSchedule
			chkEnableSchedule.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			chkEnableSchedule.BackColor = Color.FromArgb(17, 27, 45);
			chkEnableSchedule.Location = new Point(662, 43);
			chkEnableSchedule.Name = "chkEnableSchedule";
			chkEnableSchedule.Size = new Size(54, 30);
			chkEnableSchedule.TabIndex = 3;

			// btnEditSchedule
			btnEditSchedule.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnEditSchedule.BackColor = Color.FromArgb(12, 21, 36);
			btnEditSchedule.ForeColor = Color.FromArgb(245, 247, 251);
			btnEditSchedule.Location = new Point(735, 36);
			btnEditSchedule.Name = "btnEditSchedule";
			btnEditSchedule.Size = new Size(155, 42);
			btnEditSchedule.TabIndex = 4;
			btnEditSchedule.Text = LocalizationManager.Get("Text.7176F50E66F1BD1104FC");

			// ServerSettingsAutomationPage
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			AutoScroll = true;
			BackColor = Color.FromArgb(8, 13, 24);
			Controls.Add(cardStartup);
			Controls.Add(cardSchedule);
			Name = "ServerSettingsAutomationPage";
			Size = new Size(914, 496);
			cardStartup.ResumeLayout(false);
			cardStartup.PerformLayout();
			cardSchedule.ResumeLayout(false);
			cardSchedule.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		internal ModernSettingsCard cardStartup;
		internal Label lblStartupIcon;
		internal Label lblStartupTitle;
		internal Label lblUpdateTitle;
		internal Label lblUpdateDescription;
		internal ModernSettingsToggle chkUpdateOnStart;
		internal Label lblBackupTitle;
		internal Label lblBackupDescription;
		internal ModernSettingsToggle chkBackupOnStart;
		internal ModernSettingsCard cardSchedule;
		internal Label lblScheduleIcon;
		internal Label lblScheduleTitle;
		internal Label lblScheduleDescription;
		internal ModernSettingsToggle chkEnableSchedule;
		internal ModernSettingsButton btnEditSchedule;
	}
}
