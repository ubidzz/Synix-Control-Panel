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
using System.ComponentModel;
using System.Runtime.InteropServices;

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
				// Older Windows versions do not support rounded DWM corners.
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
			privacySettingsPage.PrivacyModeChanged +=
				PrivacyModeChanged;
			privacySettingsPage.CheckForDDoSChanged +=
				CheckForDDoSChanged;
			advancedSettingsPage.ElevatedSystemTasksChanged +=
				ElevatedSystemTasksChanged;
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

			btnGeneral.Selected = ReferenceEquals(selectedButton, btnGeneral);
			btnBackups.Selected = ReferenceEquals(selectedButton, btnBackups);
			btnPrivacy.Selected = ReferenceEquals(selectedButton, btnPrivacy);
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
				"Choose where backups are stored and how many Synix retains.");
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

		private void btnMinimize_Click(object? sender, EventArgs eventArgs)
		{
			WindowState = FormWindowState.Minimized;
		}

		private void btnClose_Click(object? sender, EventArgs eventArgs)
		{
			Close();
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
	}
}
