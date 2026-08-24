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
			locationColumn = new DataGridViewTextBoxColumn();
			selectionLabel = new Label();
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
			titleLabel.Size = new Size(884, 38);
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
			subtitleLabel.Size = new Size(884, 40);
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
			warningCard.Size = new Size(884, 102);
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
			warningTitle.Size = new Size(770, 25);
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
			warningText.Size = new Size(770, 46);
			warningText.TabIndex = 2;
			warningText.Text = "Synix safely stages the selected archive and automatically rolls back if restoration fails. The saved Synix server entry and its settings are not changed.";
			// 
			// backupGrid
			// 
			backupGrid.AllowUserToAddRows = false;
			backupGrid.AllowUserToDeleteRows = false;
			backupGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			backupGrid.BackgroundColor = Color.FromArgb(12, 21, 36);
			backupGrid.BorderStyle = BorderStyle.None;
			backupGrid.ColumnHeadersHeight = 40;
			backupGrid.Columns.AddRange(new DataGridViewColumn[] { createdColumn, fileColumn, sizeColumn, locationColumn });
			backupGrid.Location = new Point(28, 226);
			backupGrid.MultiSelect = false;
			backupGrid.Name = "backupGrid";
			backupGrid.ReadOnly = true;
			backupGrid.RowHeadersVisible = false;
			backupGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			backupGrid.Size = new Size(884, 314);
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
			selectionLabel.Location = new Point(28, 552);
			selectionLabel.Name = "selectionLabel";
			selectionLabel.Size = new Size(884, 28);
			selectionLabel.TabIndex = 3;
			selectionLabel.Text = "Select a backup to continue.";
			// 
			// cancelButton
			// 
			cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			cancelButton.BackColor = Color.FromArgb(12, 21, 36);
			cancelButton.DialogResult = DialogResult.Cancel;
			cancelButton.FlatStyle = FlatStyle.Flat;
			cancelButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			cancelButton.ForeColor = Color.FromArgb(245, 247, 251);
			cancelButton.Location = new Point(652, 592);
			cancelButton.Name = "cancelButton";
			cancelButton.Size = new Size(120, 42);
			cancelButton.TabIndex = 4;
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
			restoreButton.Location = new Point(784, 592);
			restoreButton.Name = "restoreButton";
			restoreButton.Size = new Size(128, 42);
			restoreButton.TabIndex = 5;
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
			ClientSize = new Size(940, 656);
			Controls.Add(titleLabel);
			Controls.Add(subtitleLabel);
			Controls.Add(warningCard);
			Controls.Add(backupGrid);
			Controls.Add(selectionLabel);
			Controls.Add(cancelButton);
			Controls.Add(restoreButton);
			Font = new Font("Segoe UI", 10F);
			Icon = (Icon)resources.GetObject("$this.Icon");
			MinimizeBox = false;
			MinimumSize = new Size(760, 560);
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
		private DataGridViewTextBoxColumn locationColumn = null!;
		private Label selectionLabel = null!;
		private ModernSettingsButton cancelButton = null!;
		private ModernSettingsButton restoreButton = null!;
	}
}
