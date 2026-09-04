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

namespace Synix_Control_Panel.SynixApp.UI.ServerManagement
{
	partial class ServerBackupRestoreDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerBackupRestoreDialog));
			titleLabel = new Label();
			subtitleLabel = new Label();
			warningCard = new ModernSettingsCard();
			warningIcon = new Label();
			warningTitle = new Label();
			warningText = new Label();
			backupGrid = new DataGridView();
			createdColumn = new DataGridViewTextBoxColumn();
			fileColumn = new DataGridViewTextBoxColumn();
			sizeColumn = new DataGridViewTextBoxColumn();
			uncompressedColumn = new DataGridViewTextBoxColumn();
			integrityColumn = new DataGridViewTextBoxColumn();
			verifiedColumn = new DataGridViewTextBoxColumn();
			locationColumn = new DataGridViewTextBoxColumn();
			selectionLabel = new Label();
			deleteButton = new ModernSettingsButton();
			verifyButton = new ModernSettingsButton();
			cancelButton = new ModernSettingsButton();
			restoreButton = new ModernSettingsButton();
			warningCard.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)backupGrid).BeginInit();
			SuspendLayout();
			// 
			// titleLabel
			// 
			titleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			titleLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
			titleLabel.ForeColor = Color.FromArgb(245, 247, 251);
			titleLabel.Location = new Point(28, 22);
			titleLabel.Name = "titleLabel";
			titleLabel.Size = new Size(1104, 38);
			titleLabel.TabIndex = 0;
			titleLabel.Text = "Restore Server Backup";
			// 
			// subtitleLabel
			// 
			subtitleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			subtitleLabel.Font = new Font("Segoe UI", 9.5F);
			subtitleLabel.ForeColor = Color.FromArgb(158, 172, 194);
			subtitleLabel.Location = new Point(28, 62);
			subtitleLabel.Name = "subtitleLabel";
			subtitleLabel.Size = new Size(1104, 40);
			subtitleLabel.TabIndex = 1;
			subtitleLabel.Text = "Choose the backup that should replace the server's current files.";
			// 
			// warningCard
			// 
			warningCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			warningCard.BackColor = Color.FromArgb(17, 27, 45);
			warningCard.BorderColor = Color.FromArgb(38, 52, 77);
			warningCard.Controls.Add(warningIcon);
			warningCard.Controls.Add(warningTitle);
			warningCard.Controls.Add(warningText);
			warningCard.FillColor = Color.FromArgb(17, 27, 45);
			warningCard.Location = new Point(28, 108);
			warningCard.Margin = new Padding(0, 0, 0, 16);
			warningCard.Name = "warningCard";
			warningCard.Size = new Size(1104, 102);
			warningCard.TabIndex = 2;
			// 
			// warningIcon
			// 
			warningIcon.BackColor = Color.FromArgb(28, 75, 91);
			warningIcon.Font = new Font("Segoe UI Symbol", 16F, FontStyle.Bold);
			warningIcon.ForeColor = Color.FromArgb(245, 185, 76);
			warningIcon.Location = new Point(18, 22);
			warningIcon.Name = "warningIcon";
			warningIcon.Size = new Size(52, 52);
			warningIcon.TabIndex = 0;
			warningIcon.Text = "↺";
			warningIcon.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// warningTitle
			// 
			warningTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			warningTitle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
			warningTitle.ForeColor = Color.FromArgb(245, 247, 251);
			warningTitle.Location = new Point(88, 17);
			warningTitle.Name = "warningTitle";
			warningTitle.Size = new Size(990, 25);
			warningTitle.TabIndex = 1;
			warningTitle.Text = "The server must remain stopped during restoration";
			// 
			// warningText
			// 
			warningText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			warningText.Font = new Font("Segoe UI", 9F);
			warningText.ForeColor = Color.FromArgb(158, 172, 194);
			warningText.Location = new Point(88, 43);
			warningText.Name = "warningText";
			warningText.Size = new Size(990, 46);
			warningText.TabIndex = 2;
			warningText.Text = "Synix verifies backups with integrity receipts, safely stages the selected archive, and automatically rolls back if restoration fails. The saved Synix server entry and its settings are not changed.";
			// 
			// backupGrid
			// 
			backupGrid.AllowUserToAddRows = false;
			backupGrid.AllowUserToDeleteRows = false;
			backupGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			backupGrid.BackgroundColor = Color.FromArgb(12, 21, 36);
			backupGrid.BorderStyle = BorderStyle.None;
			backupGrid.ColumnHeadersHeight = 40;
			backupGrid.Columns.AddRange(new DataGridViewColumn[] { createdColumn, fileColumn, sizeColumn, uncompressedColumn, integrityColumn, verifiedColumn, locationColumn });
			backupGrid.Location = new Point(28, 226);
			backupGrid.MultiSelect = false;
			backupGrid.Name = "backupGrid";
			backupGrid.ReadOnly = true;
			backupGrid.RowHeadersVisible = false;
			backupGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			backupGrid.Size = new Size(1104, 334);
			backupGrid.TabIndex = 0;
			backupGrid.CellDoubleClick += BackupGrid_CellDoubleClick;
			backupGrid.SelectionChanged += BackupGrid_SelectionChanged;
			// 
			// createdColumn
			// 
			createdColumn.HeaderText = "CREATED";
			createdColumn.MinimumWidth = 190;
			createdColumn.Name = "createdColumn";
			createdColumn.ReadOnly = true;
			createdColumn.Width = 210;
			// 
			// fileColumn
			// 
			fileColumn.HeaderText = "BACKUP FILE";
			fileColumn.MinimumWidth = 210;
			fileColumn.Name = "fileColumn";
			fileColumn.ReadOnly = true;
			fileColumn.Width = 230;
			// 
			// sizeColumn
			// 
			sizeColumn.HeaderText = "SIZE";
			sizeColumn.MinimumWidth = 90;
			sizeColumn.Name = "sizeColumn";
			sizeColumn.ReadOnly = true;
			sizeColumn.Width = 100;
			//
			// uncompressedColumn
			//
			uncompressedColumn.HeaderText = "ORIGINAL";
			uncompressedColumn.MinimumWidth = 100;
			uncompressedColumn.Name = "uncompressedColumn";
			uncompressedColumn.ReadOnly = true;
			uncompressedColumn.Width = 110;
			//
			// integrityColumn
			//
			integrityColumn.HeaderText = "INTEGRITY";
			integrityColumn.MinimumWidth = 130;
			integrityColumn.Name = "integrityColumn";
			integrityColumn.ReadOnly = true;
			integrityColumn.Width = 145;
			//
			// verifiedColumn
			//
			verifiedColumn.HeaderText = "LAST VERIFIED";
			verifiedColumn.MinimumWidth = 180;
			verifiedColumn.Name = "verifiedColumn";
			verifiedColumn.ReadOnly = true;
			verifiedColumn.Width = 190;
			//
			// locationColumn
			// 
			locationColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			locationColumn.HeaderText = "LOCATION";
			locationColumn.MinimumWidth = 220;
			locationColumn.Name = "locationColumn";
			locationColumn.ReadOnly = true;
			// 
			// selectionLabel
			// 
			selectionLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			selectionLabel.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
			selectionLabel.ForeColor = Color.FromArgb(158, 172, 194);
			selectionLabel.Location = new Point(28, 572);
			selectionLabel.Name = "selectionLabel";
			selectionLabel.Size = new Size(1104, 28);
			selectionLabel.TabIndex = 3;
			selectionLabel.Text = "Select a backup to continue.";
			//
			// deleteButton
			//
			deleteButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			deleteButton.BackColor = Color.FromArgb(12, 21, 36);
			deleteButton.Enabled = false;
			deleteButton.FlatStyle = FlatStyle.Flat;
			deleteButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			deleteButton.ForeColor = Color.FromArgb(245, 247, 251);
			deleteButton.Location = new Point(28, 612);
			deleteButton.Name = "deleteButton";
			deleteButton.Size = new Size(128, 42);
			deleteButton.TabIndex = 4;
			deleteButton.Text = "Delete Backup";
			deleteButton.UseVisualStyleBackColor = false;
			deleteButton.Click += DeleteButton_Click;
			//
			// verifyButton
			//
			verifyButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			verifyButton.BackColor = Color.FromArgb(12, 21, 36);
			verifyButton.Enabled = false;
			verifyButton.FlatStyle = FlatStyle.Flat;
			verifyButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			verifyButton.ForeColor = Color.FromArgb(245, 247, 251);
			verifyButton.Location = new Point(168, 612);
			verifyButton.Name = "verifyButton";
			verifyButton.Size = new Size(136, 42);
			verifyButton.TabIndex = 5;
			verifyButton.Text = "Verify Backup";
			verifyButton.UseVisualStyleBackColor = false;
			verifyButton.Click += VerifyButton_Click;
			// 
			// cancelButton
			// 
			cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			cancelButton.BackColor = Color.FromArgb(12, 21, 36);
			cancelButton.DialogResult = DialogResult.Cancel;
			cancelButton.FlatStyle = FlatStyle.Flat;
			cancelButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			cancelButton.ForeColor = Color.FromArgb(245, 247, 251);
			cancelButton.Location = new Point(872, 612);
			cancelButton.Name = "cancelButton";
			cancelButton.Size = new Size(120, 42);
			cancelButton.TabIndex = 6;
			cancelButton.Text = "Cancel";
			cancelButton.UseVisualStyleBackColor = false;
			// 
			// restoreButton
			// 
			restoreButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			restoreButton.BackColor = Color.FromArgb(12, 21, 36);
			restoreButton.Enabled = false;
			restoreButton.FlatStyle = FlatStyle.Flat;
			restoreButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			restoreButton.ForeColor = Color.FromArgb(245, 247, 251);
			restoreButton.Location = new Point(1004, 612);
			restoreButton.Name = "restoreButton";
			restoreButton.Size = new Size(128, 42);
			restoreButton.TabIndex = 7;
			restoreButton.Text = "Restore Backup";
			restoreButton.UseAccentStyle = true;
			restoreButton.UseVisualStyleBackColor = false;
			restoreButton.Click += RestoreButton_Click;
			// 
			// ServerBackupRestoreDialog
			// 
			AcceptButton = restoreButton;
			AutoScaleDimensions = new SizeF(7F, 17F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.FromArgb(8, 13, 24);
			CancelButton = cancelButton;
			ClientSize = new Size(1160, 676);
			Controls.Add(titleLabel);
			Controls.Add(subtitleLabel);
			Controls.Add(warningCard);
			Controls.Add(backupGrid);
			Controls.Add(selectionLabel);
			Controls.Add(deleteButton);
			Controls.Add(verifyButton);
			Controls.Add(cancelButton);
			Controls.Add(restoreButton);
			Font = new Font("Segoe UI", 10F);
			Icon = (Icon)resources.GetObject("$this.Icon");
			MinimizeBox = false;
			MinimumSize = new Size(1000, 620);
			Name = "ServerBackupRestoreDialog";
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Restore Server Backup";
			warningCard.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)backupGrid).EndInit();
			ResumeLayout(false);
		}

		private Label titleLabel = null!;
		private Label subtitleLabel = null!;
		private ModernSettingsCard warningCard = null!;
		private Label warningIcon = null!;
		private Label warningTitle = null!;
		private Label warningText = null!;
		private DataGridView backupGrid = null!;
		private DataGridViewTextBoxColumn createdColumn = null!;
		private DataGridViewTextBoxColumn fileColumn = null!;
		private DataGridViewTextBoxColumn sizeColumn = null!;
		private DataGridViewTextBoxColumn uncompressedColumn = null!;
		private DataGridViewTextBoxColumn integrityColumn = null!;
		private DataGridViewTextBoxColumn verifiedColumn = null!;
		private DataGridViewTextBoxColumn locationColumn = null!;
		private Label selectionLabel = null!;
		private ModernSettingsButton deleteButton = null!;
		private ModernSettingsButton verifyButton = null!;
		private ModernSettingsButton cancelButton = null!;
		private ModernSettingsButton restoreButton = null!;
	}
}
