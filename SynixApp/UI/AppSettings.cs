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
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class AppSettings : Form
	{
		private const int WmNcHitTest = 0x0084;
		private const int WmNcLeftButtonDown = 0x00A1;
		private const int HtCaption = 0x0002;
		private const int HtLeft = 10;
		private const int HtRight = 11;
		private const int HtTop = 12;
		private const int HtTopLeft = 13;
		private const int HtTopRight = 14;
		private const int HtBottom = 15;
		private const int HtBottomLeft = 16;
		private const int HtBottomRight = 17;
		private const int DwmWindowCornerPreference = 33;
		private const int DwmRound = 2;
		private const int ResizeBorder = 7;

		private bool _loadingSettings;
		private bool _transferInProgress;
		private string? _selectedImportPackage;
		private bool _selectedImportPasswordProtected = true;

		public AppSettings()
		{
			InitializeComponent();
			ShowPage(
				generalSettingsPage,
				btnGeneral,
				"General",
				"Configure basic Synix behavior on this computer.");

			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
			{
				return;
			}
			ThemeManager.Apply(this);

			lblVersion.Text =
				$"SYNIX CONTROL PANEL  •  v{Application.ProductVersion}";

			WireSettingsEvents();
			LoadSavedSettings();
		}

		protected override async void OnShown(EventArgs eventArgs)
		{
			base.OnShown(eventArgs);
			await RefreshExportSummaryAsync();
		}

		protected override void OnHandleCreated(EventArgs eventArgs)
		{
			base.OnHandleCreated(eventArgs);

			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
			{
				return;
			}

			try
			{
				int preference = DwmRound;
				_ = DwmSetWindowAttribute(
					Handle,
					DwmWindowCornerPreference,
					ref preference,
					sizeof(int));
			}
			catch
			{

			}
		}

		protected override void WndProc(ref Message message)
		{
			base.WndProc(ref message);

			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
			{
				return;
			}

			if (message.Msg != WmNcHitTest ||
				WindowState == FormWindowState.Maximized)
			{
				return;
			}

			Point cursor = PointToClient(Cursor.Position);
			bool left = cursor.X <= ResizeBorder;
			bool right = cursor.X >= ClientSize.Width - ResizeBorder;
			bool top = cursor.Y <= ResizeBorder;
			bool bottom = cursor.Y >= ClientSize.Height - ResizeBorder;

			if (left && top) message.Result = (IntPtr)HtTopLeft;
			else if (right && top) message.Result = (IntPtr)HtTopRight;
			else if (left && bottom) message.Result = (IntPtr)HtBottomLeft;
			else if (right && bottom) message.Result = (IntPtr)HtBottomRight;
			else if (left) message.Result = (IntPtr)HtLeft;
			else if (right) message.Result = (IntPtr)HtRight;
			else if (top) message.Result = (IntPtr)HtTop;
			else if (bottom) message.Result = (IntPtr)HtBottom;
		}

		protected override bool ProcessCmdKey(ref Message message, Keys keyData)
		{
			if (keyData == Keys.Escape)
			{
				if (_transferInProgress)
				{
					return true;
				}

				Close();
				return true;
			}

			return base.ProcessCmdKey(ref message, keyData);
		}

		private void WireSettingsEvents()
		{
			generalSettingsPage.ShowServerWindowChanged +=
				ShowServerWindowChanged;
			generalSettingsPage.DarkModeChanged +=
				DarkModeChanged;
			backupSettingsPage.CustomBackupChanged +=
				CustomBackupChanged;
			backupSettingsPage.BrowseRequested +=
				BrowseBackupRequested;
			backupSettingsPage.MaximumBackupsChanged +=
				MaximumBackupsChanged;
			backupSettingsPage.ExportSynixRequested +=
				ExportSynixRequested;
			backupSettingsPage.NormalExportRequested +=
				NormalExportRequested;
			backupSettingsPage.ImportSynixRequested +=
				ImportSynixRequested;
			backupSettingsPage.VerifyPackageRequested +=
				VerifyPackageRequested;
			privacySettingsPage.PrivacyModeChanged +=
				PrivacyModeChanged;
			privacySettingsPage.CheckForDDoSChanged +=
				CheckForDDoSChanged;
			advancedSettingsPage.ElevatedSystemTasksChanged +=
				ElevatedSystemTasksChanged;
			advancedSettingsPage.ReleaseReadinessRequested +=
				ReleaseReadinessRequested;
		}

		private void LoadSavedSettings()
		{
			_loadingSettings = true;

			try
			{
				generalSettingsPage.ShowServerWindow =
					Properties.Settings.Default.ShowServerWindow;
				generalSettingsPage.DarkMode =
					Properties.Settings.Default.DarkMode;
				backupSettingsPage.UseCustomBackupPath =
					Properties.Settings.Default.UseCustomBackupPath;

				string savedBackupPath =
					Properties.Settings.Default.CustomBackupPath;
				backupSettingsPage.BackupPath =
					string.IsNullOrWhiteSpace(savedBackupPath)
						? Core.DefaultBackupPath
						: savedBackupPath;

				backupSettingsPage.MaximumBackups =
					Properties.Settings.Default.MaxBackups;
				privacySettingsPage.PrivacyMode =
					Properties.Settings.Default.PrivacyMode;
				privacySettingsPage.CheckForDDoS =
					Properties.Settings.Default.CheckDDoS;
				advancedSettingsPage.ElevatedSystemTasks =
					Properties.Settings.Default.enableRunAsAdmin;
			}
			finally
			{
				_loadingSettings = false;
			}
		}

		private void ShowPage(
			Control page,
			ModernSettingsNavButton selectedButton,
			string heading,
			string subtitle)
		{
			generalSettingsPage.Visible =
				ReferenceEquals(page, generalSettingsPage);
			backupSettingsPage.Visible =
				ReferenceEquals(page, backupSettingsPage);
			privacySettingsPage.Visible =
				ReferenceEquals(page, privacySettingsPage);
			advancedSettingsPage.Visible =
				ReferenceEquals(page, advancedSettingsPage);
			problemReportSettingsPage.Visible =
				ReferenceEquals(page, problemReportSettingsPage);

			btnGeneral.Selected = ReferenceEquals(selectedButton, btnGeneral);
			btnBackups.Selected = ReferenceEquals(selectedButton, btnBackups);
			btnPrivacy.Selected = ReferenceEquals(selectedButton, btnPrivacy);
			btnReportProblem.Selected =
				ReferenceEquals(selectedButton, btnReportProblem);
			btnAdvanced.Selected = ReferenceEquals(selectedButton, btnAdvanced);

			lblPageHeading.Text = heading;
			lblPageSubtitle.Text = subtitle;
			page.BringToFront();
		}

		private void btnGeneral_Click(object? sender, EventArgs eventArgs)
		{
			ShowPage(
				generalSettingsPage,
				btnGeneral,
				"General",
				"Configure basic Synix behavior on this computer.");
		}

		private void btnBackups_Click(object? sender, EventArgs eventArgs)
		{
			ShowPage(
				backupSettingsPage,
				btnBackups,
				"Backups",
				"Manage server backups or move Synix to another computer.");
		}

		private void btnPrivacy_Click(object? sender, EventArgs eventArgs)
		{
			ShowPage(
				privacySettingsPage,
				btnPrivacy,
				"Privacy & Security",
				"Control how sensitive server information is displayed.");
		}

		private void btnAdvanced_Click(object? sender, EventArgs eventArgs)
		{
			ShowPage(
				advancedSettingsPage,
				btnAdvanced,
				"Advanced",
				"Configure elevated operations and advanced system behavior.");
		}

		private void btnReportProblem_Click(
			object? sender,
			EventArgs eventArgs)
		{
			ShowPage(
				problemReportSettingsPage,
				btnReportProblem,
				"Report a Problem",
				"Create a privacy-filtered compatibility report for Synix support.");
		}

		private void ReleaseReadinessRequested(
			object? sender,
			EventArgs eventArgs)
		{
			using SynixReleaseReadinessDialog dialog = new();
			dialog.ShowDialog(this);
		}

		private void btnMinimize_Click(object? sender, EventArgs eventArgs)
		{
			WindowState = FormWindowState.Minimized;
		}

		private void btnClose_Click(object? sender, EventArgs eventArgs)
		{
			if (_transferInProgress)
			{
				return;
			}

			Close();
		}

		protected override void OnFormClosing(FormClosingEventArgs eventArgs)
		{
			if (_transferInProgress)
			{
				eventArgs.Cancel = true;
				return;
			}

			base.OnFormClosing(eventArgs);
		}

		private void MaximumBackupsChanged(
			object? sender,
			EventArgs eventArgs)
		{
			if (_loadingSettings)
			{
				return;
			}

			Properties.Settings.Default.MaxBackups =
				backupSettingsPage.MaximumBackups;
			Properties.Settings.Default.Save();
		}

		private void CustomBackupChanged(
			object? sender,
			EventArgs eventArgs)
		{
			if (_loadingSettings)
			{
				return;
			}

			Properties.Settings.Default.UseCustomBackupPath =
				backupSettingsPage.UseCustomBackupPath;
			Properties.Settings.Default.Save();
		}

		private void BrowseBackupRequested(
			object? sender,
			EventArgs eventArgs)
		{
			using FolderBrowserDialog dialog = new()
			{
				Description =
					"Select a custom folder or drive for Synix server backups.",
				UseDescriptionForTitle = true
			};

			string currentPath = backupSettingsPage.BackupPath;
			if (!string.IsNullOrWhiteSpace(currentPath) &&
				Directory.Exists(currentPath))
			{
				dialog.InitialDirectory = currentPath;
			}

			if (dialog.ShowDialog(this) != DialogResult.OK)
			{
				return;
			}

			backupSettingsPage.BackupPath = dialog.SelectedPath;
			Properties.Settings.Default.CustomBackupPath =
				dialog.SelectedPath;
			Properties.Settings.Default.Save();
		}

		private async void PrivacyModeChanged(
			object? sender,
			EventArgs eventArgs)
		{
			if (_loadingSettings)
			{
				return;
			}

			Properties.Settings.Default.PrivacyMode =
				privacySettingsPage.PrivacyMode;
			Properties.Settings.Default.Save();

			if (MainGUI.Instance != null)
			{
				await MainGUI.Instance.UpdatePrivacyMode(
					privacySettingsPage.PrivacyMode);
			}
		}

		private void ElevatedSystemTasksChanged(
			object? sender,
			EventArgs eventArgs)
		{
			if (_loadingSettings)
			{
				return;
			}

			Properties.Settings.Default.enableRunAsAdmin =
				advancedSettingsPage.ElevatedSystemTasks;
			Properties.Settings.Default.Save();
		}

		private void ShowServerWindowChanged(
			object? sender,
			EventArgs eventArgs)
		{
			if (_loadingSettings)
			{
				return;
			}

			Properties.Settings.Default.ShowServerWindow =
				generalSettingsPage.ShowServerWindow;
			Properties.Settings.Default.Save();
		}

		private void TitleBar_MouseDown(
			object? sender,
			MouseEventArgs eventArgs)
		{
			if (eventArgs.Button != MouseButtons.Left)
			{
				return;
			}

			_ = ReleaseCapture();
			_ = SendMessage(Handle, WmNcLeftButtonDown, HtCaption, 0);
		}

		[DllImport("user32.dll")]
		private static extern bool ReleaseCapture();

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(
			IntPtr windowHandle,
			int message,
			int wordParameter,
			int longParameter);

		[DllImport("dwmapi.dll")]
		private static extern int DwmSetWindowAttribute(
			IntPtr windowHandle,
			int attribute,
			ref int attributeValue,
			int attributeSize);

		private void CheckForDDoSChanged(object? sender, EventArgs eventArgs)
		{
			if (_loadingSettings) return;

			Properties.Settings.Default.CheckDDoS = privacySettingsPage.CheckForDDoS;
			Properties.Settings.Default.Save();
		}

		private void DarkModeChanged(object? sender, EventArgs eventArgs)
		{
			if (_loadingSettings) return;

			Properties.Settings.Default.DarkMode = generalSettingsPage.DarkMode;
			Properties.Settings.Default.Save();
			ThemeManager.SetDarkMode(generalSettingsPage.DarkMode);
		}

		private async void ExportSynixRequested(
			object? sender,
			EventArgs eventArgs)
		{
			await ExportSynixAsync(passwordProtected: true);
		}

		private async void NormalExportRequested(
			object? sender,
			EventArgs eventArgs)
		{
			await ExportSynixAsync(passwordProtected: false);
		}

		private async Task ExportSynixAsync(bool passwordProtected)
		{
			if (!CanTransferSynix())
			{
				return;
			}

			if (!passwordProtected)
			{
				DialogResult unencryptedConfirmation = MessageBox.Show(
					this,
					"A normal export is not encrypted. Anyone who gets the file can read " +
					"settings, saved data, and any passwords written inside game configuration files.\n\n" +
					"Synix-managed passwords and Discord webhooks remain protected for this Windows " +
					"user. If this export is imported on another PC, those credentials may need " +
					"to be re-entered.\n\n" +
					"The package will still be checked for accidental damage when imported.\n\n" +
					"Do you want to create an unencrypted export?",
					"Normal Export Is Not Private",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning,
					MessageBoxDefaultButton.Button2);
				if (unencryptedConfirmation != DialogResult.Yes)
				{
					return;
				}
			}

			using SaveFileDialog fileDialog = new()
			{
				Title = "Save Synix transfer package",
				Filter = "Synix transfer package (*.synixbackup)|*.synixbackup",
				DefaultExt = "synixbackup",
				AddExtension = true,
				FileName = passwordProtected
					? $"Synix-Encrypted-{DateTime.Now:yyyy-MM-dd}.synixbackup"
					: $"Synix-Normal-{DateTime.Now:yyyy-MM-dd}.synixbackup",
				OverwritePrompt = true
			};

			if (fileDialog.ShowDialog(this) != DialogResult.OK)
			{
				return;
			}

			SynixExportEstimate? estimate = await GetExportEstimateAsync(
				fileDialog.FileName);
			if (estimate is null)
			{
				return;
			}

			string estimateMessage = BuildExportEstimateMessage(estimate);
			if (!estimate.HasEnoughSpace)
			{
				MessageBox.Show(
					this,
					estimateMessage,
					"Not Enough Free Space",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			DialogResult continueExport = MessageBox.Show(
				this,
				estimateMessage +
					(passwordProtected
						? "\n\nDo you want to continue and create a transfer password?"
						: "\n\nDo you want to continue with the normal unencrypted export?"),
				"Synix Export Size Estimate",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Information,
				MessageBoxDefaultButton.Button1);
			if (continueExport != DialogResult.Yes)
			{
				return;
			}

			string transferPassword = string.Empty;
			if (passwordProtected)
			{
				using TransferPasswordDialog passwordDialog = new(
					confirmPassword: true);
				if (passwordDialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				transferPassword = passwordDialog.TransferPassword;
			}

			if (!FileHandler.SaveServers())
			{
				MessageBox.Show(
					this,
					"Synix could not safely save the current server list. The export was not started.",
					"Unable to Save Synix",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				return;
			}

			await RunTransferOperationAsync(
				async progress =>
				{
					Core.DeleteVault(Core.RootPath);
					try
					{
						if (passwordProtected)
						{
							Core.PrepareEncryptedExport(
								Core.RootPath,
								transferPassword,
								MainGUI.serverList);

							await Core.ExportAsync(
								Core.RootPath,
								fileDialog.FileName,
								transferPassword,
								progress);
						}
						else
						{
							await Core.ExportUnencryptedAsync(
								Core.RootPath,
								fileDialog.FileName,
								progress);
						}
					}
					finally
					{
						Core.DeleteVault(Core.RootPath);
					}
				},
				"Export complete",
				passwordProtected
					? $"Synix was safely exported to:\n\n{fileDialog.FileName}\n\nSaved Synix passwords and Discord webhooks can be restored on the new PC with this transfer password. Keep it with the file; it cannot be recovered."
					: $"Synix was exported to:\n\n{fileDialog.FileName}\n\nThis file is not encrypted, so keep it somewhere private. Synix-managed passwords and Discord webhooks may need to be re-entered on another PC.");
		}

		private async void ImportSynixRequested(
			object? sender,
			EventArgs eventArgs)
		{
			if (_selectedImportPackage is null)
			{
				await SelectImportPackageAsync();
				return;
			}

			if (!CanTransferSynix())
			{
				return;
			}

			string packageFile = _selectedImportPackage;
			if (!File.Exists(packageFile))
			{
				_selectedImportPackage = null;
				_selectedImportPasswordProtected = true;
				backupSettingsPage.ShowImportSelectionPrompt();
				backupSettingsPage.SetVerifyPackageReady(false);
				MessageBox.Show(
					this,
					"The selected transfer package could not be found. Choose it again.",
					"Transfer Package Not Found",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			bool existingFiles = Directory.Exists(Core.RootPath) &&
				Directory.EnumerateFileSystemEntries(Core.RootPath).Any();
			if (existingFiles)
			{
				DialogResult confirmation = MessageBox.Show(
					this,
					"Importing will replace files with the same names in C:\\Synix. " +
					"Other files will be left in place.\n\n" +
					"Do you want to continue?",
					"Import Synix Transfer",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning,
					MessageBoxDefaultButton.Button2);

				if (confirmation != DialogResult.Yes)
				{
					return;
				}
			}

			string transferPassword = string.Empty;
			if (_selectedImportPasswordProtected)
			{
				using TransferPasswordDialog passwordDialog = new(
					confirmPassword: false);
				if (passwordDialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				transferPassword = passwordDialog.TransferPassword;
			}

			bool imported = await RunTransferOperationAsync(
				async progress =>
				{
					Core.DeleteVault(Core.RootPath);
					await Core.ImportAsync(
						packageFile,
						Core.RootPath,
						transferPassword,
						progress);

					if (_selectedImportPasswordProtected)
					{
						Core.RestoreEncryptedImport(
							Core.RootPath,
							transferPassword);
					}
					else
					{
						Core.DeleteVault(Core.RootPath);
					}
				},
				"Import complete",
				_selectedImportPasswordProtected
					? "Your Synix files, saved passwords, and Discord webhooks were restored for this Windows user. Servers that require a Steam account will ask for authorization the first time you start them on this PC."
					: "Your Synix files were restored. Passwords and Discord webhooks protected on another PC may need to be re-entered. Servers that require a Steam account will ask for authorization the first time you start them on this PC.");

			if (imported)
			{
				FileHandler.LoadServers();
				Core.MarkImportedSteamAuthenticationRequired(MainGUI.serverList);
				FileHandler.SaveServers();
				MainGUI.Instance?.UpdateGrid();
				_selectedImportPackage = null;
				_selectedImportPasswordProtected = true;
				backupSettingsPage.ShowImportSelectionPrompt();
				backupSettingsPage.SetVerifyPackageReady(false);
			}
		}

		private async void VerifyPackageRequested(
			object? sender,
			EventArgs eventArgs)
		{
			if (_selectedImportPackage is null)
			{
				await SelectImportPackageAsync();
				return;
			}

			if (Core.Instance.isDownloadActive ||
				(MainGUI.Instance?.isDownloadActive ?? false))
			{
				MessageBox.Show(
					this,
					"Wait for the current installation, update, backup, or transfer to finish before verifying a package.",
					"Synix is busy",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			string packageFile = _selectedImportPackage;
			if (!File.Exists(packageFile))
			{
				_selectedImportPackage = null;
				_selectedImportPasswordProtected = true;
				backupSettingsPage.ShowImportSelectionPrompt();
				backupSettingsPage.SetVerifyPackageReady(false);
				MessageBox.Show(
					this,
					"The selected transfer package could not be found. Choose it again.",
					"Transfer Package Not Found",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			string transferPassword = string.Empty;
			if (_selectedImportPasswordProtected)
			{
				using TransferPasswordDialog passwordDialog = new(
					confirmPassword: false);
				if (passwordDialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				transferPassword = passwordDialog.TransferPassword;
			}

			await RunTransferOperationAsync(
				async progress => await Core.VerifyAsync(
					packageFile,
					transferPassword,
					progress),
				"Package verified",
				$"Synix read and checked the entire package:\n\n" +
				$"{Path.GetFileName(packageFile)}\n\n" +
				"No damage was found, and no files were imported.");
		}

		private async Task SelectImportPackageAsync()
		{
			using OpenFileDialog fileDialog = new()
			{
				Title = "Choose a Synix transfer package",
				Filter = "Synix transfer package (*.synixbackup)|*.synixbackup",
				CheckFileExists = true,
				Multiselect = false
			};
			if (fileDialog.ShowDialog(this) != DialogResult.OK)
			{
				return;
			}

			try
			{
				SynixImportEstimate estimate = await Task.Run(() =>
					Core.EstimateImport(
						fileDialog.FileName,
						Core.RootPath));
				_selectedImportPackage = fileDialog.FileName;
				_selectedImportPasswordProtected =
					estimate.IsPasswordProtected;
				backupSettingsPage.SetVerifyPackageReady(true);
				long displayedDataBytes =
					estimate.DataBytes ?? estimate.PackageBytes;
				backupSettingsPage.ShowImportEstimate(
					Path.GetFileName(fileDialog.FileName),
					displayedDataBytes,
					estimate.AdditionalSpaceRequiredBytes,
					EstimateTransferTime(
						displayedDataBytes,
						estimate.FileCount ?? 0,
						isImport: true,
						passwordProtected: estimate.IsPasswordProtected),
					estimate.UsesLowDiskFormat,
					estimate.IsPasswordProtected);
				backupSettingsPage.SetImportReady(estimate.HasEnoughSpace);

				if (!estimate.HasEnoughSpace)
				{
					_selectedImportPackage = null;
					_selectedImportPasswordProtected = true;
					backupSettingsPage.SetVerifyPackageReady(false);
					MessageBox.Show(
						this,
						$"This import may need about " +
						$"{FormatBytes(estimate.AdditionalSpaceRequiredBytes)} of working space " +
						$"on {estimate.DestinationVolume}, but only " +
						$"{FormatBytes(estimate.AvailableBytes)} is available.\n\n" +
						"Free up space before starting the import.",
						"Not Enough Free Space",
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning);
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(
					this,
					exception.Message,
					"Package Estimate Failed",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
		}

		private async Task RefreshExportSummaryAsync()
		{
			try
			{
				string estimateDestination = Path.Combine(
					Path.GetTempPath(),
					"Synix-size-estimate.synixbackup");
				SynixExportEstimate estimate = await Task.Run(() =>
					Core.EstimateExport(
						Core.RootPath,
						estimateDestination));
				if (IsDisposed)
				{
					return;
				}

				backupSettingsPage.ShowExportEstimate(
					estimate.SourceBytes,
					estimate.FileCount,
					estimate.EstimatedPackageBytes,
					EstimateTransferTime(
						estimate.SourceBytes,
						estimate.FileCount,
						isImport: false,
						passwordProtected: true),
					EstimateTransferTime(
						estimate.SourceBytes,
						estimate.FileCount,
						isImport: false,
						passwordProtected: false));
			}
			catch (Exception exception)
			{
				if (!IsDisposed)
				{
					backupSettingsPage.ShowExportEstimate(
						0,
						0,
						0,
						$"Estimate unavailable: {exception.Message}",
						"Estimate unavailable");
				}
			}
		}

		private async Task<SynixExportEstimate?> GetExportEstimateAsync(
			string destinationFile)
		{
			_transferInProgress = true;
			Core.Instance.isDownloadActive = true;
			backupSettingsPage.SetTransferBusy(true);
			backupSettingsPage.ReportTransferProgress(new(
				"Calculating transfer size and checking free space...",
				0));

			try
			{
				SynixExportEstimate estimate = await Task.Run(() =>
					Core.EstimateExport(
						Core.RootPath,
						destinationFile));
				backupSettingsPage.ShowExportEstimate(
					estimate.SourceBytes,
					estimate.FileCount,
					estimate.EstimatedPackageBytes,
					EstimateTransferTime(
						estimate.SourceBytes,
						estimate.FileCount,
						isImport: false,
						passwordProtected: true),
					EstimateTransferTime(
						estimate.SourceBytes,
						estimate.FileCount,
						isImport: false,
						passwordProtected: false));
				backupSettingsPage.ReportTransferProgress(new(
					$"Estimated package: up to {FormatBytes(estimate.EstimatedPackageBytes)}.",
					0));
				return estimate;
			}
			catch (Exception exception)
			{
				backupSettingsPage.ReportTransferProgress(new(
					"Synix could not calculate the transfer size.",
					0));
				MessageBox.Show(
					this,
					exception.Message,
					"Export Size Check Failed",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				return null;
			}
			finally
			{
				backupSettingsPage.SetTransferBusy(false);
				Core.Instance.isDownloadActive = false;
				_transferInProgress = false;
			}
		}

		private static string BuildExportEstimateMessage(
			SynixExportEstimate estimate)
		{
			StringBuilder message = new();
			message.AppendLine(
				$"Synix found {FormatBytes(estimate.SourceBytes)} in " +
				$"{estimate.FileCount:N0} files.");
			message.AppendLine();
			message.AppendLine(
				$"Estimated transfer package: up to " +
				$"{FormatBytes(estimate.EstimatedPackageBytes)}");
			message.AppendLine();
			message.AppendLine("Free-space check:");
			foreach (SynixExportStorageRequirement requirement in
				estimate.StorageRequirements)
			{
				message.AppendLine(
					$"• {requirement.VolumeRoot} ({requirement.Purpose}): " +
					$"about {FormatBytes(requirement.RequiredBytes)} needed, " +
					$"{FormatBytes(requirement.AvailableBytes)} available");
			}

			message.AppendLine();
			if (estimate.HasEnoughSpace)
			{
				message.Append(
					"The final package may be smaller after compression.");
			}
			else
			{
				message.Append(
					"There is not enough free space. Free up space or choose " +
					"another save location, then try again.");
			}

			return message.ToString();
		}

		private static string FormatBytes(long bytes)
		{
			string[] units = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];
			double value = Math.Max(0, bytes);
			int unitIndex = 0;
			while (value >= 1024 && unitIndex < units.Length - 1)
			{
				value /= 1024;
				unitIndex++;
			}

			return $"{value:0.##} {units[unitIndex]}";
		}

		private static string EstimateTransferTime(
			long bytes,
			int fileCount,
			bool isImport,
			bool passwordProtected)
		{
			double workBytes = isImport
				? bytes * 2.0
				: bytes;
			double assumedBytesPerSecond = (isImport, passwordProtected) switch
			{
				(true, true) => 28 * 1024 * 1024,
				(true, false) => 34 * 1024 * 1024,
				(false, true) => 38 * 1024 * 1024,
				_ => 46 * 1024 * 1024
			};
			double fileOverheadSeconds = fileCount *
				(isImport ? 0.004 : 0.002);
			double centerSeconds = Math.Max(
				10,
				workBytes / assumedBytesPerSecond + fileOverheadSeconds);
			double minimumSeconds = Math.Max(5, centerSeconds * 0.65);
			double maximumSeconds = Math.Max(15, centerSeconds * 1.75);
			return $"about {FormatEstimatedDuration(minimumSeconds)}–" +
				$"{FormatEstimatedDuration(maximumSeconds)}";
		}

		private static string FormatEstimatedDuration(double seconds)
		{
			TimeSpan duration = TimeSpan.FromSeconds(Math.Max(1, seconds));
			if (duration.TotalHours >= 1)
			{
				return $"{(int)duration.TotalHours}h {duration.Minutes}m";
			}

			if (duration.TotalMinutes >= 1)
			{
				return $"{Math.Max(1, (int)Math.Ceiling(duration.TotalMinutes))}m";
			}

			return $"{Math.Max(1, (int)Math.Ceiling(duration.TotalSeconds))}s";
		}

		private bool CanTransferSynix()
		{
			bool serverBusy = MainGUI.serverList.Any(server =>
				server.Status != Core.StatusManager.GetStatus(
					Core.ServerState.Stopped));
			bool maintenanceBusy = Core.Instance.isDownloadActive ||
				(MainGUI.Instance?.isDownloadActive ?? false);

			if (!serverBusy && !maintenanceBusy)
			{
				return true;
			}

			MessageBox.Show(
				this,
				"Stop every game server and wait for installations, updates, validations, and backups to finish before transferring Synix.",
				"Synix is busy",
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);
			return false;
		}

		private async Task<bool> RunTransferOperationAsync(
			Func<IProgress<SynixTransferProgress>, Task> operation,
			string successTitle,
			string successMessage)
		{
			_transferInProgress = true;
			Core.Instance.isDownloadActive = true;
			backupSettingsPage.SetTransferBusy(true);

			Progress<SynixTransferProgress> progress = new(
				backupSettingsPage.ReportTransferProgress);

			try
			{
				await Task.Run(async () => await operation(progress));

				MessageBox.Show(
					this,
					successMessage,
					successTitle,
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return true;
			}
			catch (Exception exception)
			{
				backupSettingsPage.ReportTransferProgress(new(
					"Transfer did not complete.",
					0));
				MessageBox.Show(
					this,
					exception.Message,
					"Synix transfer failed",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				return false;
			}
			finally
			{
				backupSettingsPage.SetTransferBusy(false);
				Core.Instance.isDownloadActive = false;
				_transferInProgress = false;
			}
		}
	}
}
