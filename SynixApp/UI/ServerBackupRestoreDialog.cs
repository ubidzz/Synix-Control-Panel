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
		private GameServer? _server;
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

			_server = server;
			titleLabel.Text = $"Backups for {server.ServerName}";
			LoadBackups(backups);
		}

		private void LoadBackups(IReadOnlyList<ServerBackupArchive> backups)
		{
			backupGrid.Rows.Clear();
			subtitleLabel.Text = backups.Count == 0
				? "No saved backups remain for this server."
				: $"Manage or restore one of the {backups.Count} saved backups below. Newest backups are shown first.";
			foreach (ServerBackupArchive backup in backups)
			{
				int rowIndex = backupGrid.Rows.Add(
					backup.CreatedLocal.ToString("MMM d, yyyy  h:mm:ss tt"),
					backup.FileName,
					FormatBytes(backup.CompressedBytes),
					backup.UncompressedBytes > 0
						? FormatBytes(backup.UncompressedBytes)
						: "Unknown",
					backup.IntegrityText,
					backup.LastVerifiedLocal?.ToString("MMM d, yyyy  h:mm tt") ?? "Never",
					Path.GetDirectoryName(backup.ArchivePath) ?? string.Empty);
				DataGridViewRow row = backupGrid.Rows[rowIndex];
				row.Tag = backup;
				Color integrityColor = backup.Integrity switch
				{
					ServerBackupIntegrity.Recorded => SettingsPalette.Success,
					ServerBackupIntegrity.Legacy => SettingsPalette.Warning,
					_ => SettingsPalette.Danger
				};
				row.Cells[integrityColumn.Index].Style.ForeColor = integrityColor;
				row.Cells[integrityColumn.Index].Style.SelectionForeColor = integrityColor;
			}

			if (backupGrid.Rows.Count > 0)
			{
				backupGrid.Rows[0].Selected = true;
				backupGrid.CurrentCell = backupGrid.Rows[0].Cells[0];
			}
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

		private async void VerifyButton_Click(object? sender, EventArgs eventArgs)
		{
			UpdateSelection();
			if (_server == null || SelectedBackup == null)
				return;

			SetManagementButtonsEnabled(false);
			selectionLabel.ForeColor = SettingsPalette.SecondaryText;
			selectionLabel.Text = "Verifying archive paths and SHA-256 integrity...";
			ServerBackupManagementResult result =
				await Core.Instance.VerifyServerBackupAsync(_server, SelectedBackup);
			MessageBox.Show(
				this,
				result.Message,
				result.Succeeded ? "Backup Verified" : "Backup Verification Failed",
				MessageBoxButtons.OK,
				result.Succeeded ? MessageBoxIcon.Information : MessageBoxIcon.Error);
			LoadBackups(await Core.Instance.GetServerBackupsAsync(_server));
			SetManagementButtonsEnabled(true);
		}

		private async void DeleteButton_Click(object? sender, EventArgs eventArgs)
		{
			UpdateSelection();
			if (_server == null || SelectedBackup == null)
				return;

			DialogResult confirmation = MessageBox.Show(
				this,
				$"Permanently delete this saved backup?\n\n{SelectedBackup.FileName}\n{SelectedBackup.CreatedLocal:f}\n\nThe running server files will not be changed.",
				"Delete Server Backup",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning,
				MessageBoxDefaultButton.Button2);
			if (confirmation != DialogResult.Yes)
				return;

			SetManagementButtonsEnabled(false);
			ServerBackupManagementResult result =
				await Core.Instance.DeleteServerBackupAsync(_server, SelectedBackup);
			if (!result.Succeeded)
			{
				MessageBox.Show(
					this,
					result.Message,
					"Backup Could Not Be Deleted",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
			LoadBackups(await Core.Instance.GetServerBackupsAsync(_server));
			SetManagementButtonsEnabled(true);
		}

		private void ConfirmSelection()
		{
			UpdateSelection();
			if (SelectedBackup == null)
				return;
			if (SelectedBackup.Integrity == ServerBackupIntegrity.Invalid)
			{
				MessageBox.Show(
					this,
					"This backup has an invalid SHA-256 receipt and cannot be restored. Choose a different backup.",
					"Backup Integrity Failed",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				return;
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		private void UpdateSelection()
		{
			SelectedBackup = backupGrid.SelectedRows.Count > 0
				? backupGrid.SelectedRows[0].Tag as ServerBackupArchive
				: null;
			restoreButton.Enabled = SelectedBackup != null &&
				SelectedBackup.Integrity != ServerBackupIntegrity.Invalid;
			verifyButton.Enabled = SelectedBackup != null &&
				SelectedBackup.Integrity != ServerBackupIntegrity.Invalid;
			deleteButton.Enabled = SelectedBackup != null;
			selectionLabel.Text = SelectedBackup == null
				? "Select a backup to continue."
				: $"Selected: {SelectedBackup.CreatedLocal:f}  •  " +
					$"{FormatBytes(SelectedBackup.CompressedBytes)} compressed  •  {SelectedBackup.IntegrityText}";
			selectionLabel.ForeColor = SelectedBackup?.Integrity switch
			{
				ServerBackupIntegrity.Recorded => SettingsPalette.Success,
				ServerBackupIntegrity.Legacy => SettingsPalette.Warning,
				ServerBackupIntegrity.Invalid => SettingsPalette.Danger,
				_ => SettingsPalette.SecondaryText
			};
		}

		private void SetManagementButtonsEnabled(bool enabled)
		{
			backupGrid.Enabled = enabled;
			cancelButton.Enabled = enabled;
			if (enabled)
				UpdateSelection();
			else
			{
				restoreButton.Enabled = false;
				verifyButton.Enabled = false;
				deleteButton.Enabled = false;
			}
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
