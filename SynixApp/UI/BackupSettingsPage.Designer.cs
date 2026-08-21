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
	partial class BackupSettingsPage
	{
		private System.ComponentModel.IContainer? components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Component Designer generated code

		private void InitializeComponent()
		{
			settingsCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			rootLayout = new TableLayoutPanel();
			headerPanel = new Panel();
			headerGlyph = new Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph();
			lblHeader = new Label();
			lblCustomTitle = new Label();
			chkCustomBackup = new Synix_Control_Panel.SynixApp.Design.ModernSettingsToggle();
			lblCustomDescription = new Label();
			backupPathHost = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			txtBackupPath = new TextBox();
			btnBrowseBackup = new Synix_Control_Panel.SynixApp.Design.ModernSettingsButton();
			lblWarning = new Label();
			separator = new Label();
			retentionLayout = new TableLayoutPanel();
			retentionTextLayout = new TableLayoutPanel();
			lblMaxBackupsTitle = new Label();
			lblMaxBackupsDescription = new Label();
			numMaxBackups = new Synix_Control_Panel.SynixApp.Design.ModernSettingsNumericUpDown();
			lblRange = new Label();
			settingsCard.SuspendLayout();
			rootLayout.SuspendLayout();
			headerPanel.SuspendLayout();
			backupPathHost.SuspendLayout();
			retentionLayout.SuspendLayout();
			retentionTextLayout.SuspendLayout();
			(numMaxBackups).BeginInit();
			SuspendLayout();
			// 
			// settingsCard
			// 
			settingsCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			settingsCard.BackColor = Color.FromArgb(17, 27, 45);
			settingsCard.BorderColor = Color.FromArgb(38, 52, 77);
			settingsCard.Controls.Add(rootLayout);
			settingsCard.CornerRadius = 13;
			settingsCard.FillColor = Color.FromArgb(17, 27, 45);
			settingsCard.Location = new Point(0, 0);
			settingsCard.Margin = new Padding(0);
			settingsCard.Name = "settingsCard";
			settingsCard.Size = new Size(818, 310);
			settingsCard.TabIndex = 0;
			// 
			// rootLayout
			// 
			rootLayout.BackColor = Color.FromArgb(17, 27, 45);
			rootLayout.ColumnCount = 2;
			rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108F));
			rootLayout.Controls.Add(headerPanel, 0, 0);
			rootLayout.Controls.Add(lblCustomTitle, 0, 1);
			rootLayout.Controls.Add(chkCustomBackup, 1, 1);
			rootLayout.Controls.Add(lblCustomDescription, 0, 2);
			rootLayout.Controls.Add(backupPathHost, 0, 3);
			rootLayout.Controls.Add(btnBrowseBackup, 1, 3);
			rootLayout.Controls.Add(lblWarning, 0, 4);
			rootLayout.Controls.Add(separator, 0, 5);
			rootLayout.Controls.Add(retentionLayout, 0, 6);
			rootLayout.Dock = DockStyle.Fill;
			rootLayout.Location = new Point(0, 0);
			rootLayout.Margin = new Padding(0);
			rootLayout.Name = "rootLayout";
			rootLayout.Padding = new Padding(22, 14, 22, 14);
			rootLayout.RowCount = 7;
			rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
			rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
			rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
			rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
			rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 43F));
			rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
			rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			rootLayout.Size = new Size(818, 310);
			rootLayout.TabIndex = 0;
			// 
			// headerPanel
			// 
			headerPanel.BackColor = Color.FromArgb(17, 27, 45);
			rootLayout.SetColumnSpan(headerPanel, 2);
			headerPanel.Controls.Add(headerGlyph);
			headerPanel.Controls.Add(lblHeader);
			headerPanel.Dock = DockStyle.Fill;
			headerPanel.Location = new Point(22, 14);
			headerPanel.Margin = new Padding(0);
			headerPanel.Name = "headerPanel";
			headerPanel.Size = new Size(774, 46);
			headerPanel.TabIndex = 0;
			// 
			// headerGlyph
			// 
			headerGlyph.BackColor = Color.FromArgb(17, 27, 45);
			headerGlyph.Font = new Font("Segoe UI Symbol", 15F);
			headerGlyph.ForeColor = Color.FromArgb(32, 214, 199);
			headerGlyph.Glyph = "▤";
			headerGlyph.Location = new Point(0, 0);
			headerGlyph.Name = "headerGlyph";
			headerGlyph.Size = new Size(38, 38);
			headerGlyph.TabIndex = 0;
			// 
			// lblHeader
			// 
			lblHeader.AutoSize = true;
			lblHeader.BackColor = Color.FromArgb(17, 27, 45);
			lblHeader.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
			lblHeader.ForeColor = Color.FromArgb(245, 247, 251);
			lblHeader.Location = new Point(52, 6);
			lblHeader.Name = "lblHeader";
			lblHeader.Size = new Size(83, 25);
			lblHeader.TabIndex = 1;
			lblHeader.Text = "Backups";
			// 
			// lblCustomTitle
			// 
			lblCustomTitle.AutoEllipsis = true;
			lblCustomTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblCustomTitle.Dock = DockStyle.Fill;
			lblCustomTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblCustomTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblCustomTitle.Location = new Point(22, 60);
			lblCustomTitle.Margin = new Padding(0);
			lblCustomTitle.Name = "lblCustomTitle";
			lblCustomTitle.Size = new Size(666, 30);
			lblCustomTitle.TabIndex = 1;
			lblCustomTitle.Text = "Custom backup location";
			lblCustomTitle.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// chkCustomBackup
			// 
			chkCustomBackup.AccessibleName = "Custom backup location";
			chkCustomBackup.AccessibleRole = AccessibleRole.CheckButton;
			chkCustomBackup.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			chkCustomBackup.BackColor = Color.FromArgb(17, 27, 45);
			chkCustomBackup.Cursor = Cursors.Hand;
			chkCustomBackup.Location = new Point(742, 60);
			chkCustomBackup.Margin = new Padding(0);
			chkCustomBackup.Name = "chkCustomBackup";
			chkCustomBackup.Size = new Size(54, 30);
			chkCustomBackup.TabIndex = 2;
			chkCustomBackup.UseVisualStyleBackColor = false;
			chkCustomBackup.CheckedChanged += chkCustomBackup_CheckedChanged;
			// 
			// lblCustomDescription
			// 
			lblCustomDescription.AutoEllipsis = true;
			lblCustomDescription.BackColor = Color.FromArgb(17, 27, 45);
			rootLayout.SetColumnSpan(lblCustomDescription, 2);
			lblCustomDescription.Dock = DockStyle.Fill;
			lblCustomDescription.Font = new Font("Segoe UI", 9.5F);
			lblCustomDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblCustomDescription.Location = new Point(22, 90);
			lblCustomDescription.Margin = new Padding(0);
			lblCustomDescription.Name = "lblCustomDescription";
			lblCustomDescription.Size = new Size(774, 36);
			lblCustomDescription.TabIndex = 3;
			lblCustomDescription.Text = "Store automated and manual server backup archives in a custom folder.";
			lblCustomDescription.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// backupPathHost
			// 
			backupPathHost.BackColor = Color.FromArgb(12, 21, 36);
			backupPathHost.BorderColor = Color.FromArgb(38, 52, 77);
			backupPathHost.Controls.Add(txtBackupPath);
			backupPathHost.CornerRadius = 8;
			backupPathHost.Dock = DockStyle.Fill;
			backupPathHost.FillColor = Color.FromArgb(12, 21, 36);
			backupPathHost.Location = new Point(22, 126);
			backupPathHost.Margin = new Padding(0, 0, 12, 0);
			backupPathHost.Name = "backupPathHost";
			backupPathHost.Padding = new Padding(12, 11, 12, 8);
			backupPathHost.Size = new Size(654, 44);
			backupPathHost.TabIndex = 4;
			// 
			// txtBackupPath
			// 
			txtBackupPath.BackColor = Color.FromArgb(12, 21, 36);
			txtBackupPath.BorderStyle = BorderStyle.None;
			txtBackupPath.Dock = DockStyle.Fill;
			txtBackupPath.Font = new Font("Segoe UI", 10F);
			txtBackupPath.ForeColor = Color.FromArgb(105, 124, 153);
			txtBackupPath.Location = new Point(12, 11);
			txtBackupPath.Name = "txtBackupPath";
			txtBackupPath.ReadOnly = true;
			txtBackupPath.Size = new Size(630, 18);
			txtBackupPath.TabIndex = 0;
			txtBackupPath.TabStop = false;
			// 
			// btnBrowseBackup
			// 
			btnBrowseBackup.BackColor = Color.FromArgb(12, 21, 36);
			btnBrowseBackup.Cursor = Cursors.Hand;
			btnBrowseBackup.Dock = DockStyle.Fill;
			btnBrowseBackup.Enabled = false;
			btnBrowseBackup.FlatAppearance.BorderSize = 0;
			btnBrowseBackup.FlatStyle = FlatStyle.Flat;
			btnBrowseBackup.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnBrowseBackup.ForeColor = Color.FromArgb(245, 247, 251);
			btnBrowseBackup.Location = new Point(688, 126);
			btnBrowseBackup.Margin = new Padding(0);
			btnBrowseBackup.Name = "btnBrowseBackup";
			btnBrowseBackup.Size = new Size(108, 44);
			btnBrowseBackup.TabIndex = 5;
			btnBrowseBackup.Text = "Browse";
			btnBrowseBackup.UseVisualStyleBackColor = false;
			// 
			// lblWarning
			// 
			lblWarning.AutoEllipsis = true;
			lblWarning.BackColor = Color.FromArgb(17, 27, 45);
			rootLayout.SetColumnSpan(lblWarning, 2);
			lblWarning.Dock = DockStyle.Fill;
			lblWarning.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			lblWarning.ForeColor = Color.FromArgb(245, 185, 76);
			lblWarning.Location = new Point(22, 170);
			lblWarning.Margin = new Padding(0);
			lblWarning.Name = "lblWarning";
			lblWarning.Size = new Size(774, 43);
			lblWarning.TabIndex = 6;
			lblWarning.Text = "⚠ Changing this location does not delete backups from the previous folder.";
			lblWarning.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// separator
			// 
			separator.BackColor = Color.FromArgb(38, 52, 77);
			rootLayout.SetColumnSpan(separator, 2);
			separator.Dock = DockStyle.Fill;
			separator.Location = new Point(22, 213);
			separator.Margin = new Padding(0);
			separator.Name = "separator";
			separator.Size = new Size(774, 1);
			separator.TabIndex = 7;
			// 
			// retentionLayout
			// 
			retentionLayout.BackColor = Color.FromArgb(17, 27, 45);
			retentionLayout.ColumnCount = 3;
			rootLayout.SetColumnSpan(retentionLayout, 2);
			retentionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			retentionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 116F));
			retentionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 126F));
			retentionLayout.Controls.Add(retentionTextLayout, 0, 0);
			retentionLayout.Controls.Add(numMaxBackups, 1, 0);
			retentionLayout.Controls.Add(lblRange, 2, 0);
			retentionLayout.Dock = DockStyle.Fill;
			retentionLayout.Location = new Point(22, 222);
			retentionLayout.Margin = new Padding(0, 8, 0, 0);
			retentionLayout.Name = "retentionLayout";
			retentionLayout.RowCount = 1;
			retentionLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			retentionLayout.Size = new Size(774, 74);
			retentionLayout.TabIndex = 8;
			// 
			// retentionTextLayout
			// 
			retentionTextLayout.BackColor = Color.FromArgb(17, 27, 45);
			retentionTextLayout.ColumnCount = 1;
			retentionTextLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			retentionTextLayout.Controls.Add(lblMaxBackupsTitle, 0, 0);
			retentionTextLayout.Controls.Add(lblMaxBackupsDescription, 0, 1);
			retentionTextLayout.Dock = DockStyle.Fill;
			retentionTextLayout.Location = new Point(0, 0);
			retentionTextLayout.Margin = new Padding(0);
			retentionTextLayout.Name = "retentionTextLayout";
			retentionTextLayout.RowCount = 2;
			retentionTextLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 29F));
			retentionTextLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			retentionTextLayout.Size = new Size(532, 74);
			retentionTextLayout.TabIndex = 0;
			// 
			// lblMaxBackupsTitle
			// 
			lblMaxBackupsTitle.AutoEllipsis = true;
			lblMaxBackupsTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblMaxBackupsTitle.Dock = DockStyle.Fill;
			lblMaxBackupsTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblMaxBackupsTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblMaxBackupsTitle.Location = new Point(0, 0);
			lblMaxBackupsTitle.Margin = new Padding(0);
			lblMaxBackupsTitle.Name = "lblMaxBackupsTitle";
			lblMaxBackupsTitle.Size = new Size(532, 29);
			lblMaxBackupsTitle.TabIndex = 0;
			lblMaxBackupsTitle.Text = "Max saved backups";
			lblMaxBackupsTitle.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblMaxBackupsDescription
			// 
			lblMaxBackupsDescription.AutoEllipsis = true;
			lblMaxBackupsDescription.BackColor = Color.FromArgb(17, 27, 45);
			lblMaxBackupsDescription.Dock = DockStyle.Fill;
			lblMaxBackupsDescription.Font = new Font("Segoe UI", 9.5F);
			lblMaxBackupsDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblMaxBackupsDescription.Location = new Point(0, 29);
			lblMaxBackupsDescription.Margin = new Padding(0);
			lblMaxBackupsDescription.Name = "lblMaxBackupsDescription";
			lblMaxBackupsDescription.Size = new Size(532, 45);
			lblMaxBackupsDescription.TabIndex = 1;
			lblMaxBackupsDescription.Text = "Limit the number of backups retained per server.";
			lblMaxBackupsDescription.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// numMaxBackups
			// 
			numMaxBackups.AccessibleRole = AccessibleRole.SpinButton;
			numMaxBackups.BackColor = Color.FromArgb(12, 21, 36);
			numMaxBackups.Dock = DockStyle.Fill;
			numMaxBackups.Font = new Font("Segoe UI", 10.5F);
			numMaxBackups.ForeColor = Color.FromArgb(245, 247, 251);
			numMaxBackups.Location = new Point(544, 8);
			numMaxBackups.Margin = new Padding(12, 8, 12, 8);
			numMaxBackups.Name = "numMaxBackups";
			numMaxBackups.Size = new Size(92, 58);
			numMaxBackups.TabIndex = 1;
			// 
			// lblRange
			// 
			lblRange.AutoEllipsis = true;
			lblRange.BackColor = Color.FromArgb(17, 27, 45);
			lblRange.Dock = DockStyle.Fill;
			lblRange.Font = new Font("Segoe UI", 9.5F);
			lblRange.ForeColor = Color.FromArgb(105, 124, 153);
			lblRange.Location = new Point(648, 0);
			lblRange.Margin = new Padding(0);
			lblRange.Name = "lblRange";
			lblRange.Size = new Size(126, 74);
			lblRange.TabIndex = 2;
			lblRange.Text = "1–100 per server";
			lblRange.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// BackupSettingsPage
			// 
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			BackColor = Color.FromArgb(8, 13, 24);
			Controls.Add(settingsCard);
			Name = "BackupSettingsPage";
			Size = new Size(818, 520);
			settingsCard.ResumeLayout(false);
			rootLayout.ResumeLayout(false);
			headerPanel.ResumeLayout(false);
			headerPanel.PerformLayout();
			backupPathHost.ResumeLayout(false);
			backupPathHost.PerformLayout();
			retentionLayout.ResumeLayout(false);
			retentionTextLayout.ResumeLayout(false);
			(numMaxBackups).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private Synix_Control_Panel.SynixApp.Design.ModernSettingsCard settingsCard;
		private TableLayoutPanel rootLayout;
		private Panel headerPanel;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph headerGlyph;
		private Label lblHeader;
		private Label lblCustomTitle;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsToggle chkCustomBackup;
		private Label lblCustomDescription;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsCard backupPathHost;
		private TextBox txtBackupPath;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsButton btnBrowseBackup;
		private Label lblWarning;
		private Label separator;
		private TableLayoutPanel retentionLayout;
		private TableLayoutPanel retentionTextLayout;
		private Label lblMaxBackupsTitle;
		private Label lblMaxBackupsDescription;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsNumericUpDown numMaxBackups;
		private Label lblRange;
	}
}
