// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Design;

namespace Synix_Control_Panel.SynixApp.UI.ServerSetup
{
	partial class ServerSettingsInstallPage
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
			cardInstallLocation = new ModernSettingsCard();
			lblInstallIcon = new Label();
			lblInstallTitle = new Label();
			lblDefaultPathTitle = new Label();
			lblDefaultPathDescription = new Label();
			chkDefaultPath = new ModernSettingsToggle();
			FolderPathLabel = new Label();
			txtInstallPath = new TextBox();
			btnBrowse = new ModernSettingsButton();
			cardLaunchArguments = new ModernSettingsCard();
			lblLaunchIcon = new Label();
			lblLaunchTitle = new Label();
			lblaruments = new Label();
			btnViewArgs = new ModernSettingsButton();
			TextLabel3 = new Label();
			TextLabel7 = new Label();
			txtExtraArgs = new TextBox();
			cardInstallLocation.SuspendLayout();
			cardLaunchArguments.SuspendLayout();
			SuspendLayout();
			// cardInstallLocation
			cardInstallLocation.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardInstallLocation.BackColor = Color.FromArgb(17, 27, 45);
			cardInstallLocation.BorderColor = Color.FromArgb(38, 52, 77);
			cardInstallLocation.Controls.Add(lblInstallIcon);
			cardInstallLocation.Controls.Add(lblInstallTitle);
			cardInstallLocation.Controls.Add(lblDefaultPathTitle);
			cardInstallLocation.Controls.Add(lblDefaultPathDescription);
			cardInstallLocation.Controls.Add(chkDefaultPath);
			cardInstallLocation.Controls.Add(FolderPathLabel);
			cardInstallLocation.Controls.Add(txtInstallPath);
			cardInstallLocation.Controls.Add(btnBrowse);
			cardInstallLocation.CornerRadius = 12;
			cardInstallLocation.FillColor = Color.FromArgb(17, 27, 45);
			cardInstallLocation.Location = new Point(0, 0);
			cardInstallLocation.Name = "cardInstallLocation";
			cardInstallLocation.Size = new Size(914, 174);
			cardInstallLocation.TabIndex = 0;

			// lblInstallIcon
			lblInstallIcon.BackColor = Color.FromArgb(17, 27, 45);
			lblInstallIcon.Font = new Font("Segoe UI Symbol", 16F);
			lblInstallIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblInstallIcon.Location = new Point(20, 14);
			lblInstallIcon.Name = "lblInstallIcon";
			lblInstallIcon.Size = new Size(28, 30);
			lblInstallIcon.TabIndex = 0;
			lblInstallIcon.Text = "➜";
			lblInstallIcon.TextAlign = ContentAlignment.MiddleCenter;

			// lblInstallTitle
			lblInstallTitle.AutoSize = true;
			lblInstallTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblInstallTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblInstallTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblInstallTitle.Location = new Point(54, 19);
			lblInstallTitle.Name = "lblInstallTitle";
			lblInstallTitle.Size = new Size(127, 21);
			lblInstallTitle.TabIndex = 1;
			lblInstallTitle.Text = "Install Location";

			// lblDefaultPathTitle
			lblDefaultPathTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblDefaultPathTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblDefaultPathTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblDefaultPathTitle.Location = new Point(24, 52);
			lblDefaultPathTitle.Name = "lblDefaultPathTitle";
			lblDefaultPathTitle.Size = new Size(190, 20);
			lblDefaultPathTitle.TabIndex = 2;
			lblDefaultPathTitle.Text = "Use Synix default folder";

			// lblDefaultPathDescription
			lblDefaultPathDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblDefaultPathDescription.BackColor = Color.FromArgb(17, 27, 45);
			lblDefaultPathDescription.Font = new Font("Segoe UI", 8F);
			lblDefaultPathDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblDefaultPathDescription.Location = new Point(220, 52);
			lblDefaultPathDescription.Name = "lblDefaultPathDescription";
			lblDefaultPathDescription.Size = new Size(570, 22);
			lblDefaultPathDescription.TabIndex = 3;
			lblDefaultPathDescription.Text = "Automatically builds a safe game/server folder below the configured Games path.";

			// chkDefaultPath
			chkDefaultPath.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			chkDefaultPath.BackColor = Color.FromArgb(17, 27, 45);
			chkDefaultPath.Location = new Point(836, 45);
			chkDefaultPath.Name = "chkDefaultPath";
			chkDefaultPath.Size = new Size(54, 30);
			chkDefaultPath.TabIndex = 4;

			// FolderPathLabel
			FolderPathLabel.AutoSize = true;
			FolderPathLabel.BackColor = Color.FromArgb(17, 27, 45);
			FolderPathLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			FolderPathLabel.ForeColor = Color.FromArgb(245, 247, 251);
			FolderPathLabel.Location = new Point(24, 92);
			FolderPathLabel.Name = "FolderPathLabel";
			FolderPathLabel.Size = new Size(76, 15);
			FolderPathLabel.TabIndex = 5;
			FolderPathLabel.Text = "Folder Path";

			// txtInstallPath
			txtInstallPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			txtInstallPath.AutoSize = false;
			txtInstallPath.BackColor = Color.FromArgb(12, 21, 36);
			txtInstallPath.BorderStyle = BorderStyle.None;
			txtInstallPath.Cursor = Cursors.Default;
			txtInstallPath.Font = new Font("Segoe UI", 9.5F);
			txtInstallPath.ForeColor = Color.FromArgb(158, 172, 194);
			txtInstallPath.Location = new Point(24, 113);
			txtInstallPath.Name = "txtInstallPath";
			txtInstallPath.ReadOnly = true;
			txtInstallPath.ShortcutsEnabled = false;
			txtInstallPath.Size = new Size(690, 36);
			txtInstallPath.TabIndex = 6;
			txtInstallPath.TabStop = false;

			// btnBrowse
			btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnBrowse.BackColor = Color.FromArgb(12, 21, 36);
			btnBrowse.ForeColor = Color.FromArgb(245, 247, 251);
			btnBrowse.Location = new Point(735, 110);
			btnBrowse.Name = "btnBrowse";
			btnBrowse.Size = new Size(155, 42);
			btnBrowse.TabIndex = 7;
			btnBrowse.Text = "Browse Folder";

			// cardLaunchArguments
			cardLaunchArguments.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardLaunchArguments.BackColor = Color.FromArgb(17, 27, 45);
			cardLaunchArguments.BorderColor = Color.FromArgb(38, 52, 77);
			cardLaunchArguments.Controls.Add(lblLaunchIcon);
			cardLaunchArguments.Controls.Add(lblLaunchTitle);
			cardLaunchArguments.Controls.Add(lblaruments);
			cardLaunchArguments.Controls.Add(btnViewArgs);
			cardLaunchArguments.Controls.Add(TextLabel3);
			cardLaunchArguments.Controls.Add(TextLabel7);
			cardLaunchArguments.Controls.Add(txtExtraArgs);
			cardLaunchArguments.CornerRadius = 12;
			cardLaunchArguments.FillColor = Color.FromArgb(17, 27, 45);
			cardLaunchArguments.Location = new Point(0, 190);
			cardLaunchArguments.Name = "cardLaunchArguments";
			cardLaunchArguments.Size = new Size(914, 256);
			cardLaunchArguments.TabIndex = 1;

			// lblLaunchIcon
			lblLaunchIcon.BackColor = Color.FromArgb(17, 27, 45);
			lblLaunchIcon.Font = new Font("Segoe UI Symbol", 16F);
			lblLaunchIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblLaunchIcon.Location = new Point(20, 14);
			lblLaunchIcon.Name = "lblLaunchIcon";
			lblLaunchIcon.Size = new Size(28, 30);
			lblLaunchIcon.TabIndex = 0;
			lblLaunchIcon.Text = ">_";
			lblLaunchIcon.TextAlign = ContentAlignment.MiddleCenter;

			// lblLaunchTitle
			lblLaunchTitle.AutoSize = true;
			lblLaunchTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblLaunchTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblLaunchTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblLaunchTitle.Location = new Point(54, 19);
			lblLaunchTitle.Name = "lblLaunchTitle";
			lblLaunchTitle.Size = new Size(136, 21);
			lblLaunchTitle.TabIndex = 1;
			lblLaunchTitle.Text = "Launch Arguments";

			// lblaruments
			lblaruments.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblaruments.BackColor = Color.FromArgb(17, 27, 45);
			lblaruments.Font = new Font("Segoe UI", 8.5F);
			lblaruments.ForeColor = Color.FromArgb(158, 172, 194);
			lblaruments.Location = new Point(24, 52);
			lblaruments.Name = "lblaruments";
			lblaruments.Size = new Size(665, 55);
			lblaruments.TabIndex = 2;
			lblaruments.Text = "Required startup arguments are dynamically injected with your specific data before initialization. You may include any additional command-line flags not covered by the default string in the Extra Arguments section.";

			// btnViewArgs
			btnViewArgs.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnViewArgs.BackColor = Color.FromArgb(12, 21, 36);
			btnViewArgs.ForeColor = Color.FromArgb(245, 247, 251);
			btnViewArgs.Location = new Point(710, 58);
			btnViewArgs.Name = "btnViewArgs";
			btnViewArgs.Size = new Size(180, 42);
			btnViewArgs.TabIndex = 3;
			btnViewArgs.Text = "View Default Arguments";

			// TextLabel3
			TextLabel3.AutoSize = true;
			TextLabel3.BackColor = Color.FromArgb(17, 27, 45);
			TextLabel3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			TextLabel3.ForeColor = Color.FromArgb(245, 247, 251);
			TextLabel3.Location = new Point(24, 122);
			TextLabel3.Name = "TextLabel3";
			TextLabel3.Size = new Size(99, 15);
			TextLabel3.TabIndex = 4;
			TextLabel3.Text = "Extra Arguments";

			// TextLabel7
			TextLabel7.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			TextLabel7.BackColor = Color.FromArgb(17, 27, 45);
			TextLabel7.Font = new Font("Segoe UI", 8F);
			TextLabel7.ForeColor = Color.FromArgb(158, 172, 194);
			TextLabel7.Location = new Point(140, 121);
			TextLabel7.Name = "TextLabel7";
			TextLabel7.Size = new Size(750, 20);
			TextLabel7.TabIndex = 5;
			TextLabel7.Text = "Optional flags only — for example: -log, -nosteamclient, or -forceupdate";

			// txtExtraArgs
			txtExtraArgs.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			txtExtraArgs.BackColor = Color.FromArgb(12, 21, 36);
			txtExtraArgs.BorderStyle = BorderStyle.FixedSingle;
			txtExtraArgs.Font = new Font("Cascadia Mono", 9F);
			txtExtraArgs.ForeColor = Color.FromArgb(245, 247, 251);
			txtExtraArgs.Location = new Point(24, 149);
			txtExtraArgs.Multiline = true;
			txtExtraArgs.Name = "txtExtraArgs";
			txtExtraArgs.ScrollBars = ScrollBars.Vertical;
			txtExtraArgs.Size = new Size(866, 78);
			txtExtraArgs.TabIndex = 6;

			// ServerSettingsInstallPage
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			AutoScroll = true;
			BackColor = Color.FromArgb(8, 13, 24);
			Controls.Add(cardInstallLocation);
			Controls.Add(cardLaunchArguments);
			Name = "ServerSettingsInstallPage";
			Size = new Size(914, 496);
			cardInstallLocation.ResumeLayout(false);
			cardInstallLocation.PerformLayout();
			cardLaunchArguments.ResumeLayout(false);
			cardLaunchArguments.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		internal ModernSettingsCard cardInstallLocation;
		internal Label lblInstallIcon;
		internal Label lblInstallTitle;
		internal Label lblDefaultPathTitle;
		internal Label lblDefaultPathDescription;
		internal ModernSettingsToggle chkDefaultPath;
		internal Label FolderPathLabel;
		internal TextBox txtInstallPath;
		internal ModernSettingsButton btnBrowse;
		internal ModernSettingsCard cardLaunchArguments;
		internal Label lblLaunchIcon;
		internal Label lblLaunchTitle;
		internal Label lblaruments;
		internal ModernSettingsButton btnViewArgs;
		internal Label TextLabel3;
		internal Label TextLabel7;
		internal TextBox txtExtraArgs;
	}
}
