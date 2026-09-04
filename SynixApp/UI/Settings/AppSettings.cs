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
using Synix_Control_Panel.SynixApp.Database.GameConfigurations;
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using Synix_Control_Panel.SynixApp.Localization;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Synix_Control_Panel.SynixApp.UI.Settings
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
		private bool _configurationCollectionInProgress;
		private string? _selectedImportPackage;
		private bool _selectedImportPasswordProtected = true;
		private string _currentPageHeadingKey =
			"SettingsPage.General.Heading";
		private string _currentPageSubtitleKey =
			"SettingsPage.General.Subtitle";

		public AppSettings()
		{
			InitializeComponent();
			btnDevelopment.Visible = !Core.IsOfficialRelease;
			ShowPage(
				generalSettingsPage,
				btnGeneral,
				"SettingsPage.General.Heading",
				"SettingsPage.General.Subtitle");

			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
			{
				return;
			}
			ThemeManager.Apply(this);

			lblVersion.Text = LocalizationManager.Get(
				"Settings.VersionLabel",
				Application.ProductVersion);

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
			catch (Exception suppressedException)
			{
				Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException);
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
			generalSettingsPage.SteamCmdDownloadModeChanged +=
				SteamCmdDownloadModeChanged;
			generalSettingsPage.SteamCmdDownloadLimitChanged +=
				SteamCmdDownloadLimitChanged;
			generalSettingsPage.UiLanguageChanged +=
				UiLanguageChanged;
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
			advancedSettingsPage.FirewallCleanupRequested +=
				FirewallCleanupRequested;
			advancedSettingsPage.BackgroundServiceEnabledChanged +=
				BackgroundServiceEnabledChanged;
			advancedSettingsPage.TroubleshooterRequested +=
				TroubleshooterRequested;
			developmentSettingsPage.UsePremadeConfigurationsChanged +=
				UsePremadeConfigurationsChanged;
			developmentSettingsPage.CollectGeneratedConfigurationsChanged +=
				CollectGeneratedConfigurationsChanged;
			developmentSettingsPage.CollectGeneratedConfigurationsRequested +=
				CollectGeneratedConfigurationsRequested;
			developmentSettingsPage.ReleaseReadinessRequested +=
				ReleaseReadinessRequested;
			developmentSettingsPage.GameDefinitionValidationRequested +=
				GameDefinitionValidationRequested;
			developmentSettingsPage.GameDefinitionBuilderRequested +=
				GameDefinitionBuilderRequested;
			developmentSettingsPage.GameVerificationQueueRequested +=
				GameVerificationQueueRequested;
			developmentSettingsPage.ReliabilityTestRequested +=
				ReliabilityTestRequested;
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
				generalSettingsPage.SteamCmdDownloadLimitMbps =
					Properties.Settings.Default.SteamCmdDownloadLimitMbps;
				generalSettingsPage.LimitSteamCmdDownloadSpeed =
					Properties.Settings.Default.LimitSteamCmdDownloadSpeed;
				generalSettingsPage.UiLanguageCode =
					Properties.Settings.Default.UiLanguage;
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
				advancedSettingsPage.BackgroundServiceEnabled =
					Properties.Settings.Default.BackgroundServiceEnabled;
				developmentSettingsPage.UsePremadeConfigurations =
					!Properties.Settings.Default.DisablePremadeConfigurationsForDevelopment;
				developmentSettingsPage.CollectGeneratedConfigurations =
					Properties.Settings.Default.CollectGeneratedConfigurationsForDevelopment;
			}
			finally
			{
				_loadingSettings = false;
			}
		}

		private void ShowPage(
			Control page,
			ModernSettingsNavButton selectedButton,
			string headingKey,
			string subtitleKey)
		{
			generalSettingsPage.Visible =
				ReferenceEquals(page, generalSettingsPage);
			backupSettingsPage.Visible =
				ReferenceEquals(page, backupSettingsPage);
			privacySettingsPage.Visible =
				ReferenceEquals(page, privacySettingsPage);
			advancedSettingsPage.Visible =
				ReferenceEquals(page, advancedSettingsPage);
			developmentSettingsPage.Visible =
				ReferenceEquals(page, developmentSettingsPage);
			problemReportSettingsPage.Visible =
				ReferenceEquals(page, problemReportSettingsPage);

			btnGeneral.Selected = ReferenceEquals(selectedButton, btnGeneral);
			btnBackups.Selected = ReferenceEquals(selectedButton, btnBackups);
			btnPrivacy.Selected = ReferenceEquals(selectedButton, btnPrivacy);
			btnReportProblem.Selected =
				ReferenceEquals(selectedButton, btnReportProblem);
			btnAdvanced.Selected = ReferenceEquals(selectedButton, btnAdvanced);
			btnDevelopment.Selected =
				ReferenceEquals(selectedButton, btnDevelopment);

			_currentPageHeadingKey = headingKey;
			_currentPageSubtitleKey = subtitleKey;
			UpdateCurrentPageHeader();
			page.BringToFront();
		}

		private void UpdateCurrentPageHeader()
		{
			lblPageHeading.Text =
				LocalizationManager.Get(_currentPageHeadingKey);
			lblPageSubtitle.Text =
				LocalizationManager.Get(_currentPageSubtitleKey);
		}

		private void btnGeneral_Click(object? sender, EventArgs eventArgs)
		{
			ShowPage(
				generalSettingsPage,
				btnGeneral,
				"SettingsPage.General.Heading",
				"SettingsPage.General.Subtitle");
		}

		private void btnBackups_Click(object? sender, EventArgs eventArgs)
		{
			ShowPage(
				backupSettingsPage,
				btnBackups,
				"SettingsPage.Backups.Heading",
				"SettingsPage.Backups.Subtitle");
		}

		private void btnPrivacy_Click(object? sender, EventArgs eventArgs)
		{
			ShowPage(
				privacySettingsPage,
				btnPrivacy,
				"SettingsPage.Privacy.Heading",
				"SettingsPage.Privacy.Subtitle");
		}

		private void btnAdvanced_Click(object? sender, EventArgs eventArgs)
		{
			ShowPage(
				advancedSettingsPage,
				btnAdvanced,
				"SettingsPage.Advanced.Heading",
				"SettingsPage.Advanced.Subtitle");
		}

		private void btnReportProblem_Click(
			object? sender,
			EventArgs eventArgs)
		{
			ShowPage(
				problemReportSettingsPage,
				btnReportProblem,
				"SettingsPage.ReportProblem.Heading",
				"SettingsPage.ReportProblem.Subtitle");
		}

		private void ReleaseReadinessRequested(
			object? sender,
			EventArgs eventArgs)
		{
			using SynixReleaseReadinessDialog dialog = new();
			dialog.ShowDialog(this);
		}

		private void TroubleshooterRequested(
			object? sender,
			EventArgs eventArgs)
		{
			using TroubleshooterDialog dialog = new();
			dialog.ShowDialog(this);
		}

		private void GameDefinitionValidationRequested(
			object? sender,
			EventArgs eventArgs)
		{
			if (Core.IsOfficialRelease)
				return;

			using GameDefinitionValidationDialog dialog = new();
			dialog.ShowDialog(this);
		}

		private void GameDefinitionBuilderRequested(
			object? sender,
			EventArgs eventArgs)
		{
			if (Core.IsOfficialRelease)
				return;

			using GameDefinitionBuilder builder = new();
			builder.ShowDialog(this);
		}

		private void GameVerificationQueueRequested(
			object? sender,
			EventArgs eventArgs)
		{
			if (Core.IsOfficialRelease)
				return;

			using GameVerificationQueue queue = new();
			queue.ShowDialog(this);
		}

		private void ReliabilityTestRequested(object? sender, EventArgs eventArgs)
		{
			if (Core.IsOfficialRelease)
				return;
			using ReliabilityTestDialog dialog = new();
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
				Description = LocalizationManager.Get(
					"Settings.Backup.FolderPicker"),
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

			await ApplicationUiService.UpdatePrivacyModeAsync(
				privacySettingsPage.PrivacyMode);
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

		private async void FirewallCleanupRequested(
			object? sender,
			EventArgs eventArgs)
		{
			advancedSettingsPage.SetFirewallCleanupState(
				LocalizationManager.Get("Advanced.Firewall.CheckingPaths"),
				false,
				inProgress: true);
			FirewallOrphanScanResult scan = await Task.Run(() =>
				FirewallCleanupService.ScanCurrentRules());
			if (!scan.Succeeded)
			{
				advancedSettingsPage.SetFirewallCleanupState(
					LocalizationManager.TranslateRuntimeText(scan.Message),
					false);
				PlainEnglishErrorDialog.ShowError(
					this,
					LocalizationManager.Get(
						"Settings.Advanced.ErrorAction.InspectFirewall"),
					scan.Message);
				return;
			}

			if (scan.ExecutablePaths.Count == 0)
			{
				advancedSettingsPage.SetFirewallCleanupState(
					LocalizationManager.TranslateRuntimeText(scan.Message),
					true);
				return;
			}

			using FirewallCleanupConfirmationDialog confirmation = new(
				scan.ExecutablePaths);
			if (confirmation.ShowDialog(this) != DialogResult.OK)
			{
				advancedSettingsPage.SetFirewallCleanupState(
					LocalizationManager.Get("Advanced.Firewall.Canceled"),
					false);
				return;
			}

			advancedSettingsPage.SetFirewallCleanupState(
				LocalizationManager.Get("Advanced.Firewall.WaitingForAdmin"),
				false,
				inProgress: true);
			ElevatedFirewallCleanupResult cleanup =
				await FirewallCleanupService.RunElevatedCleanupAsync();
			if (!cleanup.Succeeded)
			{
				advancedSettingsPage.SetFirewallCleanupState(
					cleanup.Message,
					false);
				if (!cleanup.Canceled)
				{
					PlainEnglishErrorDialog.ShowError(
						this,
						LocalizationManager.Get(
							"Settings.Advanced.ErrorAction.CleanFirewall"),
						cleanup.Message);
				}
				return;
			}

			FirewallOrphanScanResult verification = await Task.Run(() =>
				FirewallCleanupService.ScanCurrentRules());
			bool verified = verification.Succeeded &&
				verification.ExecutablePaths.Count == 0;
			string resultMessage = verified
				? LocalizationManager.Get(
					"Advanced.Firewall.RemovedVerified",
					scan.ExecutablePaths.Count)
				: cleanup.Message;
			advancedSettingsPage.SetFirewallCleanupState(
				resultMessage,
				verified);
			if (verified)
			{
				ApplicationLogService.WriteLocalized(
					"Settings.Firewall.Activity.Removed",
					Color.LimeGreen,
					arguments: scan.ExecutablePaths.Count);
			}
		}

		private void BackgroundServiceEnabledChanged(
			object? sender,
			EventArgs eventArgs)
		{
			if (_loadingSettings)
				return;

			bool enabled = advancedSettingsPage.BackgroundServiceEnabled;
			if (!BackgroundServiceManager.SetEnabled(enabled, out string message))
			{
				_loadingSettings = true;
				advancedSettingsPage.BackgroundServiceEnabled =
					Properties.Settings.Default.BackgroundServiceEnabled;
				_loadingSettings = false;
				advancedSettingsPage.SetBackgroundServiceStatus(message, false);
				return;
			}

			Properties.Settings.Default.BackgroundServiceEnabled = enabled;
			Properties.Settings.Default.Save();
			advancedSettingsPage.SetBackgroundServiceStatus(message, enabled);
		}

		private void btnDevelopment_Click(
			object? sender,
			EventArgs eventArgs)
		{
			if (Core.IsOfficialRelease)
				return;

			ShowPage(
				developmentSettingsPage,
				btnDevelopment,
				"SettingsPage.Development.Heading",
				"SettingsPage.Development.Subtitle");
		}

		private void UsePremadeConfigurationsChanged(
			object? sender,
			EventArgs eventArgs)
		{
			if (_loadingSettings || Core.IsOfficialRelease)
			{
				return;
			}

			Properties.Settings.Default.DisablePremadeConfigurationsForDevelopment =
				!developmentSettingsPage.UsePremadeConfigurations;
			Properties.Settings.Default.Save();
		}

		private void CollectGeneratedConfigurationsChanged(
			object? sender,
			EventArgs eventArgs)
		{
			if (_loadingSettings || Core.IsOfficialRelease)
			{
				return;
			}

			Properties.Settings.Default.CollectGeneratedConfigurationsForDevelopment =
				developmentSettingsPage.CollectGeneratedConfigurations;
			Properties.Settings.Default.Save();
		}

		private async void CollectGeneratedConfigurationsRequested(
			object? sender,
			EventArgs eventArgs)
		{
			if (Core.IsOfficialRelease || _configurationCollectionInProgress)
			{
				return;
			}

			_configurationCollectionInProgress = true;
			UseWaitCursor = true;
			try
			{
				GameServer[] servers = ServerRegistry.Servers.ToArray();
				GeneratedConfigurationCaptureResult result = await Task.Run(() =>
					GeneratedConfigurationCollector.Collect(servers));
				StringBuilder message = new();
				if (result.FoundFiles)
				{
					message.AppendLine(
						LocalizationManager.Get(
							"Settings.Development.Collect.Summary",
							result.CopiedFiles,
							result.UnchangedFiles));
					message.AppendLine();
					message.AppendLine(result.DestinationRoot);
					message.AppendLine();
					message.AppendLine(
						LocalizationManager.Get(
							"Settings.Development.Collect.Secrets"));
				}
				else
				{
					message.AppendLine(
						LocalizationManager.Get(
							"Settings.Development.Collect.None"));
				}

				if (result.Errors.Count > 0)
				{
					message.AppendLine();
					message.AppendLine(LocalizationManager.Get(
						"Settings.Development.Collect.FailuresHeader"));
					foreach (string error in result.Errors.Take(5))
					{
						message.AppendLine($"• {error}");
					}
					if (result.Errors.Count > 5)
					{
						message.AppendLine(
							LocalizationManager.Get(
								"Settings.Development.Collect.AdditionalFiles",
								result.Errors.Count - 5));
					}
				}

				MessageBoxButtons buttons = result.FoundFiles
					? MessageBoxButtons.YesNo
					: MessageBoxButtons.OK;
				if (result.FoundFiles)
				{
					message.AppendLine();
					message.Append(LocalizationManager.Get(
						"Settings.Development.Collect.OpenFolder"));
				}

				DialogResult response = LocalizedMessageBox.Show(
					this,
					message.ToString(),
					LocalizationManager.Get(
						"Settings.Development.Collect.Title"),
					buttons,
					result.Errors.Count == 0
						? MessageBoxIcon.Information
						: MessageBoxIcon.Warning);
				if (result.FoundFiles && response == DialogResult.Yes)
				{
					Process.Start(new ProcessStartInfo
					{
						FileName = result.DestinationRoot,
						UseShellExecute = true
					});
				}
			}
			catch (Exception exception)
			{
				LocalizedMessageBox.Show(
					this,
					LocalizationManager.TranslateRuntimeText(exception.Message),
					LocalizationManager.Get(
						"Settings.Development.Collect.FailedTitle"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
			finally
			{
				UseWaitCursor = false;
				_configurationCollectionInProgress = false;
			}
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

		private void SteamCmdDownloadModeChanged(
			object? sender,
			EventArgs eventArgs)
		{
			if (_loadingSettings) return;

			Properties.Settings.Default.LimitSteamCmdDownloadSpeed =
				generalSettingsPage.LimitSteamCmdDownloadSpeed;
			Properties.Settings.Default.Save();
		}

		private void SteamCmdDownloadLimitChanged(
			object? sender,
			EventArgs eventArgs)
		{
			if (_loadingSettings) return;

			Properties.Settings.Default.SteamCmdDownloadLimitMbps =
				generalSettingsPage.SteamCmdDownloadLimitMbps;
			Properties.Settings.Default.Save();
		}

		private void UiLanguageChanged(
			object? sender,
			EventArgs eventArgs)
		{
			if (_loadingSettings)
			{
				return;
			}

			string languageCode = generalSettingsPage.UiLanguageCode;
			Properties.Settings.Default.UiLanguage = languageCode;
			Properties.Settings.Default.Save();
			LocalizationManager.SetLanguage(languageCode);
			UpdateCurrentPageHeader();
			lblVersion.Text = LocalizationManager.Get(
				"Settings.VersionLabel",
				Application.ProductVersion);
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
				DialogResult unencryptedConfirmation = LocalizedMessageBox.Show(
					this,
					LocalizationManager.Get(
						"Settings.Transfer.NormalWarning"),
					LocalizationManager.Get(
						"Settings.Transfer.NormalWarningTitle"),
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
				Title = LocalizationManager.Get(
					"Settings.Transfer.SavePicker.Title"),
				Filter = LocalizationManager.Get(
					"Settings.Transfer.FileFilter"),
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
				LocalizedMessageBox.Show(
					this,
					estimateMessage,
					LocalizationManager.Get(
						"Settings.Transfer.NotEnoughSpaceTitle"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			DialogResult continueExport = LocalizedMessageBox.Show(
				this,
				estimateMessage +
					(passwordProtected
						? "\n\n" + LocalizationManager.Get(
							"Settings.Transfer.ContinueEncrypted")
						: "\n\n" + LocalizationManager.Get(
							"Settings.Transfer.ContinueNormal")),
				LocalizationManager.Get(
					"Settings.Transfer.EstimateTitle"),
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
				LocalizedMessageBox.Show(
					this,
					LocalizationManager.Get(
						"Settings.Transfer.SaveFailed"),
					LocalizationManager.Get(
						"Settings.Transfer.SaveFailedTitle"),
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
								ServerRegistry.Servers);

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
				LocalizationManager.Get(
					"Settings.Transfer.ExportCompleteTitle"),
				passwordProtected
					? LocalizationManager.Get(
						"Settings.Transfer.ExportCompleteEncrypted",
						fileDialog.FileName)
					: LocalizationManager.Get(
						"Settings.Transfer.ExportCompleteNormal",
						fileDialog.FileName));
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
				LocalizedMessageBox.Show(
					this,
					LocalizationManager.Get(
						"Settings.Transfer.PackageNotFound"),
					LocalizationManager.Get(
						"Settings.Transfer.PackageNotFoundTitle"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			bool existingFiles = Directory.Exists(Core.RootPath) &&
				Directory.EnumerateFileSystemEntries(Core.RootPath).Any();
			if (existingFiles)
			{
				DialogResult confirmation = LocalizedMessageBox.Show(
					this,
					LocalizationManager.Get(
						"Settings.Transfer.ImportConfirm"),
					LocalizationManager.Get(
						"Settings.Transfer.ImportConfirmTitle"),
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
				LocalizationManager.Get(
					"Settings.Transfer.ImportCompleteTitle"),
				_selectedImportPasswordProtected
					? LocalizationManager.Get(
						"Settings.Transfer.ImportCompleteEncrypted")
					: LocalizationManager.Get(
						"Settings.Transfer.ImportCompleteNormal"));

			if (imported)
			{
				FileHandler.LoadServers();
				Core.MarkImportedSteamAuthenticationRequired(ServerRegistry.Servers);
				FileHandler.SaveServers();
				ApplicationUiService.RequestGridRefresh();
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

			if (Core.Instance.isDownloadActive)
			{
				LocalizedMessageBox.Show(
					this,
					LocalizationManager.Get(
						"Settings.Transfer.BusyVerify"),
					LocalizationManager.Get(
						"Settings.Transfer.BusyTitle"),
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
				LocalizedMessageBox.Show(
					this,
					LocalizationManager.Get(
						"Settings.Transfer.PackageNotFound"),
					LocalizationManager.Get(
						"Settings.Transfer.PackageNotFoundTitle"),
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
				LocalizationManager.Get(
					"Settings.Transfer.VerifyCompleteTitle"),
				LocalizationManager.Get(
					"Settings.Transfer.VerifyCompleteBody",
					Path.GetFileName(packageFile)));
		}

		private async Task SelectImportPackageAsync()
		{
			using OpenFileDialog fileDialog = new()
			{
				Title = LocalizationManager.Get(
					"Settings.Transfer.OpenPicker.Title"),
				Filter = LocalizationManager.Get(
					"Settings.Transfer.FileFilter"),
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
					LocalizedMessageBox.Show(
						this,
						LocalizationManager.Get(
							"Settings.Transfer.ImportNoSpace",
							FormatBytes(estimate.AdditionalSpaceRequiredBytes),
							estimate.DestinationVolume,
							FormatBytes(estimate.AvailableBytes)),
						LocalizationManager.Get(
							"Settings.Transfer.NotEnoughSpaceTitle"),
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning);
				}
			}
			catch (Exception exception)
			{
				LocalizedMessageBox.Show(
					this,
					LocalizationManager.TranslateRuntimeText(exception.Message),
					LocalizationManager.Get(
						"Settings.Transfer.EstimateFailedTitle"),
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
						LocalizationManager.Get(
							"Settings.Transfer.EstimateUnavailable",
							LocalizationManager.TranslateRuntimeText(
								exception.Message)),
						LocalizationManager.Get(
							"Settings.Transfer.EstimateUnavailableShort"));
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
				LocalizationManager.Get(
					"Settings.Transfer.Calculating"),
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
					LocalizationManager.Get(
						"Settings.Transfer.EstimatedPackageStatus",
						FormatBytes(estimate.EstimatedPackageBytes)),
					0));
				return estimate;
			}
			catch (Exception exception)
			{
				backupSettingsPage.ReportTransferProgress(new(
					LocalizationManager.Get(
						"Settings.Transfer.SizeCalculationFailed"),
					0));
				LocalizedMessageBox.Show(
					this,
					LocalizationManager.TranslateRuntimeText(exception.Message),
					LocalizationManager.Get(
						"Settings.Transfer.ExportSizeCheckFailedTitle"),
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
				LocalizationManager.Get(
					"Settings.Transfer.EstimateSummary",
					FormatBytes(estimate.SourceBytes),
					estimate.FileCount));
			message.AppendLine();
			message.AppendLine(
				LocalizationManager.Get(
					"Settings.Transfer.EstimatePackage",
					FormatBytes(estimate.EstimatedPackageBytes)));
			message.AppendLine();
			message.AppendLine(LocalizationManager.Get(
				"Settings.Transfer.FreeSpaceHeader"));
			foreach (SynixExportStorageRequirement requirement in
				estimate.StorageRequirements)
			{
				message.AppendLine(
					LocalizationManager.Get(
						"Settings.Transfer.FreeSpaceLine",
						requirement.VolumeRoot,
						LocalizationManager.TranslateRuntimeText(
							requirement.Purpose),
						FormatBytes(requirement.RequiredBytes),
						FormatBytes(requirement.AvailableBytes)));
			}

			message.AppendLine();
			if (estimate.HasEnoughSpace)
			{
				message.Append(LocalizationManager.Get(
					"Settings.Transfer.EnoughSpace"));
			}
			else
			{
				message.Append(LocalizationManager.Get(
					"Settings.Transfer.InsufficientSpace"));
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
			return LocalizationManager.Get(
				"Settings.Transfer.ApproximateRange",
				FormatEstimatedDuration(minimumSeconds),
				FormatEstimatedDuration(maximumSeconds));
		}

		private static string FormatEstimatedDuration(double seconds)
		{
			TimeSpan duration = TimeSpan.FromSeconds(Math.Max(1, seconds));
			if (duration.TotalHours >= 1)
			{
				return LocalizationManager.Get(
					"Settings.Transfer.DurationHoursMinutes",
					(int)duration.TotalHours,
					duration.Minutes);
			}

			if (duration.TotalMinutes >= 1)
			{
				return LocalizationManager.Get(
					"Settings.Transfer.DurationMinutes",
					Math.Max(1, (int)Math.Ceiling(duration.TotalMinutes)));
			}

			return LocalizationManager.Get(
				"Settings.Transfer.DurationSeconds",
				Math.Max(1, (int)Math.Ceiling(duration.TotalSeconds)));
		}

		private bool CanTransferSynix()
		{
			bool serverBusy = ServerRegistry.Servers.Any(server =>
				server.Status != Core.StatusManager.GetStatus(
					Core.ServerState.Stopped));
			bool maintenanceBusy = Core.Instance.isDownloadActive;

			if (!serverBusy && !maintenanceBusy)
			{
				return true;
			}

			LocalizedMessageBox.Show(
				this,
				LocalizationManager.Get(
					"Settings.Transfer.Busy"),
				LocalizationManager.Get(
					"Settings.Transfer.BusyTitle"),
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

				LocalizedMessageBox.Show(
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
					LocalizationManager.Get(
						"Settings.Transfer.FailedStatus"),
					0));
				LocalizedMessageBox.Show(
					this,
					LocalizationManager.TranslateRuntimeText(exception.Message),
					LocalizationManager.Get(
						"Settings.Transfer.FailedTitle"),
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
