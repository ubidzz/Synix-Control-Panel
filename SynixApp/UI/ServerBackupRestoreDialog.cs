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

namespace Synix_Control_Panel.SynixEngine
{
	internal sealed partial class ServerBackupRestoreDialog : Form
	{
		internal ServerBackupArchive? SelectedBackup { get; private set; }

		internal ServerBackupRestoreDialog()
		{
			InitializeComponent();
			GridStyler.DarkTheme(backupGrid);
			GridStyler.ApplyDashboardTheme(backupGrid);
			GridStyler.ApplyRoundedCorners(backupGrid, 10);
			ThemeManager.Apply(this);
		}

		internal ServerBackupRestoreDialog(
			GameServer server,
			IReadOnlyList<ServerBackupArchive> backups) : this()
		{
			ArgumentNullException.ThrowIfNull(server);
			ArgumentNullException.ThrowIfNull(backups);

			titleLabel.Text = $"Restore {server.ServerName}";
			subtitleLabel.Text =
				$"Choose one of the {backups.Count} saved server backups below. Newest backups are shown first.";
			foreach (ServerBackupArchive backup in backups)
			{
				int rowIndex = backupGrid.Rows.Add(
					backup.CreatedLocal.ToString("MMM d, yyyy  h:mm:ss tt"),
					backup.FileName,
					FormatBytes(backup.CompressedBytes),
					Path.GetDirectoryName(backup.ArchivePath) ?? string.Empty);
				backupGrid.Rows[rowIndex].Tag = backup;
			}

			if (backupGrid.Rows.Count > 0)
				backupGrid.Rows[0].Selected = true;
			UpdateSelection();
		}

		private void BackupGrid_SelectionChanged(object? sender, EventArgs eventArgs) =>
			UpdateSelection();

		private void BackupGrid_CellDoubleClick(
			object? sender,
			DataGridViewCellEventArgs eventArgs)
		{
			if (eventArgs.RowIndex < 0)
				return;
			backupGrid.Rows[eventArgs.RowIndex].Selected = true;
			ConfirmSelection();
		}

		private void RestoreButton_Click(object? sender, EventArgs eventArgs) =>
			ConfirmSelection();

		private void ConfirmSelection()
		{
			UpdateSelection();
			if (SelectedBackup == null)
				return;

			DialogResult = DialogResult.OK;
			Close();
		}

		private void UpdateSelection()
		{
			SelectedBackup = backupGrid.SelectedRows.Count > 0
				? backupGrid.SelectedRows[0].Tag as ServerBackupArchive
				: null;
			restoreButton.Enabled = SelectedBackup != null;
			selectionLabel.Text = SelectedBackup == null
				? "Select a backup to continue."
				: $"Selected: {SelectedBackup.CreatedLocal:f}  •  {FormatBytes(SelectedBackup.CompressedBytes)}";
		}

		private static string FormatBytes(long bytes)
		{
			string[] units = ["B", "KB", "MB", "GB", "TB"];
			double value = Math.Max(0, bytes);
			int unit = 0;
			while (value >= 1024 && unit < units.Length - 1)
			{
				value /= 1024;
				unit++;
			}
			return $"{value:0.##} {units[unit]}";
		}
	}
}
