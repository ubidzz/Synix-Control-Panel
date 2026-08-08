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

namespace Synix_Control_Panel.SynixEngine
{
	partial class AppSettings
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppSettings));
			chkCustomBackup = new CheckBox();
			txtBackupPath = new TextBox();
			btnBrowseBackup = new Button();
			lblBackupSaveLocation = new Label();
			lblCustomBackupLocationInfo = new Label();
			chkPrivacyMode = new CheckBox();
			label1 = new Label();
			label2 = new Label();
			lblLine = new Label();
			chkRunAsAdmin = new CheckBox();
			lblRunAsAdmin = new Label();
			label3 = new Label();
			lblRunAsAdminInfo = new Label();
			lblBackupWarning = new Label();
			SuspendLayout();
			// 
			// chkCustomBackup
			// 
			chkCustomBackup.Location = new Point(12, 299);
			chkCustomBackup.Name = "chkCustomBackup";
			chkCustomBackup.Size = new Size(88, 26);
			chkCustomBackup.TabIndex = 0;
			chkCustomBackup.Text = "Activate";
			chkCustomBackup.UseVisualStyleBackColor = true;
			chkCustomBackup.CheckedChanged += chkCustomBackup_CheckedChanged;
			// 
			// txtBackupPath
			// 
			txtBackupPath.Enabled = false;
			txtBackupPath.Location = new Point(12, 331);
			txtBackupPath.Name = "txtBackupPath";
			txtBackupPath.ReadOnly = true;
			txtBackupPath.Size = new Size(287, 23);
			txtBackupPath.TabIndex = 1;
			// 
			// btnBrowseBackup
			// 
			btnBrowseBackup.Location = new Point(305, 331);
			btnBrowseBackup.Name = "btnBrowseBackup";
			btnBrowseBackup.Size = new Size(75, 23);
			btnBrowseBackup.TabIndex = 2;
			btnBrowseBackup.Text = "Browse";
			btnBrowseBackup.UseVisualStyleBackColor = true;
			btnBrowseBackup.Click += btnBrowseBackup_Click;
			// 
			// lblBackupSaveLocation
			// 
			lblBackupSaveLocation.AutoSize = true;
			lblBackupSaveLocation.BackColor = Color.Transparent;
			lblBackupSaveLocation.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblBackupSaveLocation.ForeColor = Color.White;
			lblBackupSaveLocation.Location = new Point(12, 159);
			lblBackupSaveLocation.Name = "lblBackupSaveLocation";
			lblBackupSaveLocation.Size = new Size(203, 25);
			lblBackupSaveLocation.TabIndex = 3;
			lblBackupSaveLocation.Text = "Backup save location ";
			// 
			// lblCustomBackupLocationInfo
			// 
			lblCustomBackupLocationInfo.BackColor = Color.Transparent;
			lblCustomBackupLocationInfo.ForeColor = Color.White;
			lblCustomBackupLocationInfo.Location = new Point(12, 184);
			lblCustomBackupLocationInfo.Name = "lblCustomBackupLocationInfo";
			lblCustomBackupLocationInfo.Size = new Size(354, 42);
			lblCustomBackupLocationInfo.TabIndex = 4;
			lblCustomBackupLocationInfo.Text = "Enable this setting to direct all automated and manual server backup archives to a preferred folder or secondary drive.";
			// 
			// chkPrivacyMode
			// 
			chkPrivacyMode.Location = new Point(12, 95);
			chkPrivacyMode.Name = "chkPrivacyMode";
			chkPrivacyMode.Size = new Size(88, 26);
			chkPrivacyMode.TabIndex = 5;
			chkPrivacyMode.Text = "checkBox1";
			chkPrivacyMode.UseVisualStyleBackColor = true;
			chkPrivacyMode.CheckedChanged += chkPrivacyMode_CheckedChanged;
			// 
			// label1
			// 
			label1.BackColor = Color.Transparent;
			label1.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
			label1.ForeColor = Color.White;
			label1.Location = new Point(12, 9);
			label1.Name = "label1";
			label1.Size = new Size(142, 23);
			label1.TabIndex = 6;
			label1.Text = "Privacy Mode";
			// 
			// label2
			// 
			label2.BackColor = Color.Transparent;
			label2.ForeColor = Color.White;
			label2.Location = new Point(12, 43);
			label2.Name = "label2";
			label2.Size = new Size(368, 49);
			label2.TabIndex = 7;
			label2.Text = "Enabling Privacy Mode hides IPs, passwords, and other sensitive information within Synix, keeping your data safe while screen sharing.";
			// 
			// lblLine
			// 
			lblLine.BackColor = Color.White;
			lblLine.Location = new Point(-4, 137);
			lblLine.Name = "lblLine";
			lblLine.Size = new Size(399, 10);
			lblLine.TabIndex = 8;
			// 
			// chkRunAsAdmin
			// 
			chkRunAsAdmin.Location = new Point(12, 488);
			chkRunAsAdmin.Name = "chkRunAsAdmin";
			chkRunAsAdmin.Size = new Size(88, 26);
			chkRunAsAdmin.TabIndex = 9;
			chkRunAsAdmin.Text = "checkBox1";
			chkRunAsAdmin.UseVisualStyleBackColor = true;
			chkRunAsAdmin.CheckedChanged += chkRunAsAdmin_CheckedChanged;
			// 
			// lblRunAsAdmin
			// 
			lblRunAsAdmin.AutoSize = true;
			lblRunAsAdmin.BackColor = Color.Transparent;
			lblRunAsAdmin.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblRunAsAdmin.ForeColor = Color.White;
			lblRunAsAdmin.Location = new Point(12, 394);
			lblRunAsAdmin.Name = "lblRunAsAdmin";
			lblRunAsAdmin.Size = new Size(269, 25);
			lblRunAsAdmin.TabIndex = 10;
			lblRunAsAdmin.Text = "Enable Elevated System Tasks";
			// 
			// label3
			// 
			label3.BackColor = Color.White;
			label3.Location = new Point(-4, 369);
			label3.Name = "label3";
			label3.Size = new Size(399, 10);
			label3.TabIndex = 11;
			// 
			// lblRunAsAdminInfo
			// 
			lblRunAsAdminInfo.BackColor = Color.Transparent;
			lblRunAsAdminInfo.ForeColor = Color.White;
			lblRunAsAdminInfo.Location = new Point(12, 419);
			lblRunAsAdminInfo.Name = "lblRunAsAdminInfo";
			lblRunAsAdminInfo.Size = new Size(362, 66);
			lblRunAsAdminInfo.TabIndex = 12;
			lblRunAsAdminInfo.Text = resources.GetString("lblRunAsAdminInfo.Text");
			// 
			// lblBackupWarning
			// 
			lblBackupWarning.BackColor = Color.Transparent;
			lblBackupWarning.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblBackupWarning.ForeColor = Color.Yellow;
			lblBackupWarning.Location = new Point(12, 226);
			lblBackupWarning.Name = "lblBackupWarning";
			lblBackupWarning.Size = new Size(368, 70);
			lblBackupWarning.TabIndex = 13;
			lblBackupWarning.Text = resources.GetString("lblBackupWarning.Text");
			// 
			// AppSettings
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			AutoSizeMode = AutoSizeMode.GrowAndShrink;
			BackgroundImage = Properties.Resources.background;
			ClientSize = new Size(392, 526);
			Controls.Add(lblBackupWarning);
			Controls.Add(lblRunAsAdminInfo);
			Controls.Add(label3);
			Controls.Add(lblRunAsAdmin);
			Controls.Add(chkRunAsAdmin);
			Controls.Add(lblLine);
			Controls.Add(label2);
			Controls.Add(label1);
			Controls.Add(chkPrivacyMode);
			Controls.Add(lblCustomBackupLocationInfo);
			Controls.Add(lblBackupSaveLocation);
			Controls.Add(btnBrowseBackup);
			Controls.Add(txtBackupPath);
			Controls.Add(chkCustomBackup);
			FormBorderStyle = FormBorderStyle.Fixed3D;
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MdiChildrenMinimizedAnchorBottom = false;
			MinimizeBox = false;
			Name = "AppSettings";
			SizeGripStyle = SizeGripStyle.Hide;
			Text = "Synix Settings";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private CheckBox chkCustomBackup;
		private TextBox txtBackupPath;
		private Button btnBrowseBackup;
		private Label lblBackupSaveLocation;
		private Label lblCustomBackupLocationInfo;
		private CheckBox chkPrivacyMode;
		private Label label1;
		private Label label2;
		private Label lblLine;
		private CheckBox chkRunAsAdmin;
		private Label lblRunAsAdmin;
		private Label label3;
		private Label lblRunAsAdminInfo;
		private Label lblBackupWarning;
	}
}