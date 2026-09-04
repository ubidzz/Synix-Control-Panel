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
	internal sealed partial class SynixReleaseReadinessDialog : Form
	{
		private readonly string? _projectDirectory;
		private string? _publishDirectory;
		private CancellationTokenSource? _checkCancellation;
		private bool _checkInProgress;

		public SynixReleaseReadinessDialog()
		{
			InitializeComponent();
			_projectDirectory = Core.FindProjectDirectory(
				AppContext.BaseDirectory);
			_publishDirectory = _projectDirectory is null
				? null
				: Core.FindPublishDirectory(
					_projectDirectory);

			_publishPathLabel.Text = _publishDirectory ?? "No publish folder was detected.";
			ThemeManager.Apply(this);
		}

		protected override async void OnShown(EventArgs eventArgs)
		{
			base.OnShown(eventArgs);
			if (_projectDirectory is not null &&
				!string.IsNullOrWhiteSpace(_publishDirectory))
			{
				await RunCheckAsync();
			}
		}

		protected override void OnFormClosing(FormClosingEventArgs eventArgs)
		{
			if (_checkInProgress)
			{
				_checkCancellation?.Cancel();
				eventArgs.Cancel = true;
				_statusLabel.Text = "Canceling the release check safely...";
			}

			base.OnFormClosing(eventArgs);
		}

		private void BrowseButton_Click(object? sender, EventArgs eventArgs)
		{
			using FolderBrowserDialog browser = new()
			{
				Description = "Select the folder containing the published Synix Control Panel.exe.",
				UseDescriptionForTitle = true,
				ShowNewFolderButton = false,
				SelectedPath = Directory.Exists(_publishDirectory)
					? _publishDirectory
					: string.Empty
			};
			if (browser.ShowDialog(this) != DialogResult.OK)
				return;

			_publishDirectory = browser.SelectedPath;
			_publishPathLabel.Text = _publishDirectory;
			_statusLabel.Text = "Publish folder selected. Run the check when ready.";
			_copyButton.Enabled = false;
		}

		private async void RunButton_Click(object? sender, EventArgs eventArgs)
		{
			if (_checkInProgress)
			{
				_checkCancellation?.Cancel();
				_statusLabel.Text = "Canceling the release check safely...";
				return;
			}

			await RunCheckAsync();
		}

		private async Task RunCheckAsync()
		{
			if (_projectDirectory is null)
			{
				LocalizedMessageBox.Show(
					this,
					"Synix could not find the project folder. Run this checker from a Visual Studio Build or Rebuild of the Synix project.",
					"Project Folder Not Found",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}
			if (string.IsNullOrWhiteSpace(_publishDirectory))
			{
				LocalizedMessageBox.Show(
					this,
					"Choose the folder containing the published Synix files first.",
					"Publish Folder Required",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			_checkInProgress = true;
			_checkCancellation = new CancellationTokenSource();
			_runButton.Text = "Cancel Check";
			_browseButton.Enabled = false;
			_copyButton.Enabled = false;
			_closeButton.Enabled = false;
			_reportBox.Text = "Checking release files...";
			_statusLabel.ForeColor = SettingsPalette.SecondaryText;

			try
			{
				Progress<string> progress = new(message =>
					_statusLabel.Text = message);
				SynixReleaseReadinessReport report = await Core.CheckReleaseReadinessAsync(
					_projectDirectory,
					_publishDirectory,
					progress,
					_checkCancellation.Token);

				_reportBox.Text = report.ToPlainText();
				_reportBox.SelectionStart = 0;
				_reportBox.ScrollToCaret();
				_copyButton.Enabled = true;
				_statusLabel.Text = report.IsReady
					? $"READY TO RELEASE  •  {report.PassedCount} checks passed"
					: $"NOT READY  •  {report.FailedCount} item(s) need attention";
				_statusLabel.ForeColor = report.IsReady
					? SettingsPalette.Success
					: SettingsPalette.Warning;
			}
			catch (OperationCanceledException)
			{
				_reportBox.Text = "The release check was canceled. No release files were changed.";
				_statusLabel.Text = "Release check canceled.";
				_statusLabel.ForeColor = SettingsPalette.Warning;
			}
			catch (Exception exception)
			{
				_reportBox.Text = exception.Message;
				_statusLabel.Text = "The release check could not finish.";
				_statusLabel.ForeColor = SettingsPalette.Warning;
			}
			finally
			{
				_checkCancellation?.Dispose();
				_checkCancellation = null;
				_checkInProgress = false;
				_runButton.Text = "Run Release Check";
				_browseButton.Enabled = true;
				_closeButton.Enabled = true;
			}
		}

		private void CopyButton_Click(object? sender, EventArgs eventArgs)
		{
			try
			{
				Clipboard.SetText(_reportBox.Text);
				_statusLabel.Text = "Release report copied to the clipboard.";
			}
			catch
			{
				LocalizedMessageBox.Show(
					this,
					"Windows could not copy the release report.",
					"Copy Failed",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
			}
		}
	}
}
