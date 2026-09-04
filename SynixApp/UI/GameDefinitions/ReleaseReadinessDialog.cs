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

namespace Synix_Control_Panel.SynixApp.UI.GameDefinitions
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

			_publishPathLabel.Text = _publishDirectory ??
				LocalizationManager.Get("Text.ECE11BCA5230C237A53C");
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
				LocalizationManager.BindText(
					_statusLabel,
					"Text.ED928782624AFCE2078E");
			}

			base.OnFormClosing(eventArgs);
		}

		private void BrowseButton_Click(object? sender, EventArgs eventArgs)
		{
			using FolderBrowserDialog browser = new()
			{
				Description = LocalizationManager.Get("ReleaseReadiness.FolderPicker"),
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
			LocalizationManager.BindText(
				_statusLabel,
				"Text.49CDA0DED2FDB6421A49");
			_copyButton.Enabled = false;
		}

		private async void RunButton_Click(object? sender, EventArgs eventArgs)
		{
			if (_checkInProgress)
			{
				_checkCancellation?.Cancel();
				LocalizationManager.BindText(
					_statusLabel,
					"Text.ED928782624AFCE2078E");
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
					LocalizationManager.Get("MessageText.6B5EDE5E1A10BF24CA11"),
					LocalizationManager.Get("MessageText.56FF45D5FB6A4400524B"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}
			if (string.IsNullOrWhiteSpace(_publishDirectory))
			{
				LocalizedMessageBox.Show(
					this,
					LocalizationManager.Get("MessageText.2022398CDE720D78BF2A"),
					LocalizationManager.Get("MessageText.1F510840383E97897648"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			_checkInProgress = true;
			_checkCancellation = new CancellationTokenSource();
			LocalizationManager.BindText(_runButton, "Text.2222C9E93C86C44F7B3E");
			_browseButton.Enabled = false;
			_copyButton.Enabled = false;
			_closeButton.Enabled = false;
			LocalizationManager.BindText(_reportBox, "Text.D230F2F5F0AC7821A70C");
			_statusLabel.ForeColor = SettingsPalette.SecondaryText;

			try
			{
				Progress<string> progress = new(message =>
					_statusLabel.Text = LocalizationManager.TranslateRuntimeText(message));
				SynixReleaseReadinessReport report = await Core.CheckReleaseReadinessAsync(
					_projectDirectory,
					_publishDirectory,
					progress,
					_checkCancellation.Token);

				_reportBox.Text = report.ToPlainText();
				_reportBox.SelectionStart = 0;
				_reportBox.ScrollToCaret();
				_copyButton.Enabled = true;
				LocalizationManager.BindText(
					_statusLabel,
					report.IsReady
						? "ReleaseReadiness.Summary.Ready"
						: "ReleaseReadiness.Summary.NotReady",
					report.IsReady ? report.PassedCount : report.FailedCount);
				_statusLabel.ForeColor = report.IsReady
					? SettingsPalette.Success
					: SettingsPalette.Warning;
			}
			catch (OperationCanceledException)
			{
				LocalizationManager.BindText(_reportBox, "Text.0117F5327FA39CD0DABB");
				LocalizationManager.BindText(_statusLabel, "Text.2C3115623E07C95FD45C");
				_statusLabel.ForeColor = SettingsPalette.Warning;
			}
			catch (Exception exception)
			{
				_reportBox.Text = LocalizationManager.TranslateRuntimeText(
					exception.Message);
				LocalizationManager.BindText(_statusLabel, "Text.3CDC839AB03D7080ED58");
				_statusLabel.ForeColor = SettingsPalette.Warning;
			}
			finally
			{
				_checkCancellation?.Dispose();
				_checkCancellation = null;
				_checkInProgress = false;
				LocalizationManager.BindText(_runButton, "Text.A4EEB7A5AB5896C51F91");
				_browseButton.Enabled = true;
				_closeButton.Enabled = true;
			}
		}

		private void CopyButton_Click(object? sender, EventArgs eventArgs)
		{
			try
			{
				Clipboard.SetText(_reportBox.Text);
				LocalizationManager.BindText(_statusLabel, "Text.696DA5B5195A0FB2544E");
			}
			catch (Exception suppressedException)
			{
				ApplicationLogService.WriteSuppressedException(suppressedException);
				LocalizedMessageBox.Show(
					this,
					LocalizationManager.Get("MessageText.C4163B7C6E2B1B662C17"),
					LocalizationManager.Get("MessageText.2C58B2D4975AADC6042D"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
			}
		}
	}
}
