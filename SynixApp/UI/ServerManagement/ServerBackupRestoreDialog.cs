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

namespace Synix_Control_Panel.SynixApp.UI.ServerManagement
{
	internal sealed partial class ServerBackupRestoreDialog : Form
	{
		private GameServer? _server;
		private bool _busy;
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
			LocalizationManager.BindText(
				titleLabel,
				"ServerBackup.Title",
				server.ServerName);
			LoadBackups(backups);
		}

		private void LoadBackups(IReadOnlyList<ServerBackupArchive> backups)
		{
			string? selectedPath = SelectedBackup?.ArchivePath;
			backupGrid.Rows.Clear();
			LocalizationManager.BindText(
				subtitleLabel,
				backups.Count == 0
					? "DynamicText.B33FD36A68F065CBBB4C"
					: "ServerBackup.Subtitle.Many",
				backups.Count);
			foreach (ServerBackupArchive backup in backups)
			{
				int rowIndex = backupGrid.Rows.Add(
					backup.CreatedLocal.ToString(
						"G",
						System.Globalization.CultureInfo.CurrentUICulture),
					backup.FileName,
					FormatBytes(backup.CompressedBytes),
					backup.UncompressedBytes > 0
						? FormatBytes(backup.UncompressedBytes)
						: LocalizationManager.Get("Status.Unknown"),
					LocalizationManager.TranslateKnownText(backup.IntegrityText),
					backup.LastVerifiedLocal?.ToString(
						"g",
						System.Globalization.CultureInfo.CurrentUICulture) ??
					LocalizationManager.Get("GameDefinitions.Queue.Never"),
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
				DataGridViewRow selectedRow = backupGrid.Rows.Cast<DataGridViewRow>().FirstOrDefault(row =>
					(row.Tag as ServerBackupArchive)?.ArchivePath.Equals(selectedPath, StringComparison.OrdinalIgnoreCase) == true)
					?? backupGrid.Rows[0];
				backupGrid.ClearSelection();
				backupGrid.CurrentCell = selectedRow.Cells[0];
				selectedRow.Selected = true;
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
			if (_busy) return;
			UpdateSelection();
			if (_server == null || SelectedBackup == null)
				return;
			ServerBackupArchive backup = SelectedBackup;
			try
			{
				await RunManagementActionAsync(async () =>
				{
					selectionLabel.ForeColor = SettingsPalette.SecondaryText;
					LocalizationManager.BindText(
						selectionLabel,
						"Text.16FB4FBCA9A11AC839AE");
					ServerBackupManagementResult result =
						await Core.Instance.VerifyServerBackupAsync(_server, backup);
					LocalizedMessageBox.Show(
						this,
						LocalizationManager.TranslateRuntimeText(result.Message),
						LocalizationManager.Get(
							result.Succeeded
								? "MessageText.17A4B79ED999C004885D"
								: "MessageText.6ABC8E0D8209E250F6DE"),
						MessageBoxButtons.OK,
						result.Succeeded ? MessageBoxIcon.Information : MessageBoxIcon.Error);
					LoadBackups(await Core.Instance.GetServerBackupsAsync(_server));
				});
			}
			catch (Exception exception)
			{
				PlainEnglishErrorDialog.ShowError(this, LocalizationManager.Get("MessageText.6ABC8E0D8209E250F6DE"), exception.Message);
			}
		}

		private async void DeleteButton_Click(object? sender, EventArgs eventArgs)
		{
			if (_busy) return;
			UpdateSelection();
			if (_server == null || SelectedBackup == null)
				return;

			DialogResult confirmation = LocalizedMessageBox.Show(
				this,
				LocalizationManager.Get(
					"ServerBackup.Delete.Confirm",
					SelectedBackup.FileName,
					SelectedBackup.CreatedLocal.ToString(
						"f",
						System.Globalization.CultureInfo.CurrentUICulture)),
				LocalizationManager.Get("MessageText.88498CCE0307979DF647"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning,
				MessageBoxDefaultButton.Button2);
			if (confirmation != DialogResult.Yes)
				return;

			ServerBackupArchive backup = SelectedBackup;
			try
			{
				await RunManagementActionAsync(async () =>
				{
					ServerBackupManagementResult result =
						await Core.Instance.DeleteServerBackupAsync(_server, backup);
					if (!result.Succeeded)
					{
						LocalizedMessageBox.Show(
							this,
							LocalizationManager.TranslateRuntimeText(result.Message),
							LocalizationManager.Get("MessageText.9848B5FC9954185EE516"),
							MessageBoxButtons.OK,
							MessageBoxIcon.Error);
					}
					LoadBackups(await Core.Instance.GetServerBackupsAsync(_server));
				});
			}
			catch (Exception exception)
			{
				PlainEnglishErrorDialog.ShowError(this, LocalizationManager.Get("MessageText.9848B5FC9954185EE516"), exception.Message);
			}
		}

		internal async Task RunManagementActionAsync(Func<Task> action)
		{
			if (_busy) return;
			SetManagementButtonsEnabled(false);
			try { await action(); }
			finally { SetManagementButtonsEnabled(true); }
		}

		protected override void OnFormClosing(FormClosingEventArgs eventArgs)
		{
			if (_busy) eventArgs.Cancel = true;
			base.OnFormClosing(eventArgs);
		}

		private void ConfirmSelection()
		{
			if (_busy) return;
			UpdateSelection();
			if (SelectedBackup == null)
				return;
			if (SelectedBackup.Integrity == ServerBackupIntegrity.Invalid)
			{
				LocalizedMessageBox.Show(
					this,
					LocalizationManager.Get("MessageText.313E93838C2B67200EAB"),
					LocalizationManager.Get("MessageText.5222069B5E5C4C68451B"),
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
			restoreButton.Enabled = !_busy && SelectedBackup != null &&
				SelectedBackup.Integrity != ServerBackupIntegrity.Invalid;
			verifyButton.Enabled = !_busy && SelectedBackup != null &&
				SelectedBackup.Integrity != ServerBackupIntegrity.Invalid;
			deleteButton.Enabled = !_busy && SelectedBackup != null;
			if (_busy) return;
			LocalizationManager.BindText(
				selectionLabel,
				SelectedBackup == null
					? "Text.F00F8392B2C39AE7826E"
					: "ServerBackup.Selection",
				SelectedBackup?.CreatedLocal.ToString(
					"f",
					System.Globalization.CultureInfo.CurrentUICulture) ?? string.Empty,
				SelectedBackup == null
					? string.Empty
					: FormatBytes(SelectedBackup.CompressedBytes),
				SelectedBackup == null
					? string.Empty
					: LocalizationManager.TranslateKnownText(SelectedBackup.IntegrityText));
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
			_busy = !enabled;
			UseWaitCursor = !enabled;
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
