// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Design;

namespace Synix_Control_Panel.SynixEngine
{
	internal sealed class SynixReleaseReadinessDialog : Form
	{
		private readonly Label _publishPathLabel;
		private readonly Label _statusLabel;
		private readonly RichTextBox _reportBox;
		private readonly ModernSettingsButton _browseButton;
		private readonly ModernSettingsButton _runButton;
		private readonly ModernSettingsButton _copyButton;
		private readonly ModernSettingsButton _closeButton;
		private readonly string? _projectDirectory;
		private string? _publishDirectory;
		private CancellationTokenSource? _checkCancellation;
		private bool _checkInProgress;

		public SynixReleaseReadinessDialog()
		{
			Text = "Synix Release Readiness Checker";
			StartPosition = FormStartPosition.CenterParent;
			FormBorderStyle = FormBorderStyle.Sizable;
			MinimizeBox = false;
			ShowInTaskbar = false;
			ClientSize = new Size(900, 700);
			MinimumSize = new Size(720, 560);
			BackColor = SettingsPalette.Window;
			Font = new Font("Segoe UI", 10F);

			_projectDirectory = SynixReleaseReadinessChecker.FindProjectDirectory(
				AppContext.BaseDirectory);
			_publishDirectory = _projectDirectory is null
				? null
				: SynixReleaseReadinessChecker.FindPublishDirectory(
					_projectDirectory);

			Label title = new()
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				Location = new Point(24, 20),
				Size = new Size(852, 34),
				Font = new Font("Segoe UI", 17F, FontStyle.Bold),
				ForeColor = SettingsPalette.PrimaryText,
				Text = "Release Readiness Checker"
			};
			Label subtitle = new()
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				Location = new Point(24, 56),
				Size = new Size(852, 24),
				ForeColor = SettingsPalette.SecondaryText,
				Text = "Checks the actual publish output without rebuilding Synix, starting the release, or accessing C:\\Synix."
			};

			ModernSettingsCard folderCard = new()
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				Location = new Point(24, 94),
				Size = new Size(852, 92),
				FillColor = SettingsPalette.Card,
				BorderColor = SettingsPalette.Border
			};
			Label folderTitle = new()
			{
				Location = new Point(18, 12),
				Size = new Size(620, 25),
				ForeColor = SettingsPalette.PrimaryText,
				Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
				Text = "Published Synix folder"
			};
			_publishPathLabel = new Label
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				AutoEllipsis = true,
				Location = new Point(18, 44),
				Size = new Size(635, 28),
				ForeColor = SettingsPalette.SecondaryText,
				Text = _publishDirectory ?? "No publish folder was detected."
			};
			_browseButton = new ModernSettingsButton
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
				Location = new Point(688, 25),
				Size = new Size(142, 42),
				Text = "Choose Folder"
			};
			_browseButton.Click += BrowseButton_Click;
			folderCard.Controls.Add(folderTitle);
			folderCard.Controls.Add(_publishPathLabel);
			folderCard.Controls.Add(_browseButton);

			_statusLabel = new Label
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				Location = new Point(24, 200),
				Size = new Size(852, 28),
				ForeColor = SettingsPalette.SecondaryText,
				Text = "Ready to check the published files and the test receipt created during Publish."
			};
			_reportBox = new RichTextBox
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
				Location = new Point(24, 236),
				Size = new Size(852, 392),
				BackColor = SettingsPalette.Input,
				ForeColor = SettingsPalette.SecondaryText,
				BorderStyle = BorderStyle.None,
				ReadOnly = true,
				DetectUrls = false,
				Font = new Font("Cascadia Mono", 9.5F),
				ScrollBars = RichTextBoxScrollBars.ForcedBoth,
				WordWrap = false,
				Text = "The readiness report will appear here."
			};

			_runButton = new ModernSettingsButton
			{
				Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
				Location = new Point(690, 642),
				Size = new Size(186, 42),
				Text = "Run Release Check",
				UseAccentStyle = true
			};
			_runButton.Click += RunButton_Click;
			_copyButton = new ModernSettingsButton
			{
				Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
				Location = new Point(526, 642),
				Size = new Size(152, 42),
				Text = "Copy Report",
				Enabled = false
			};
			_copyButton.Click += CopyButton_Click;
			_closeButton = new ModernSettingsButton
			{
				Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
				Location = new Point(24, 642),
				Size = new Size(110, 42),
				Text = "Close",
				DialogResult = DialogResult.Cancel
			};

			Controls.Add(title);
			Controls.Add(subtitle);
			Controls.Add(folderCard);
			Controls.Add(_statusLabel);
			Controls.Add(_reportBox);
			Controls.Add(_closeButton);
			Controls.Add(_copyButton);
			Controls.Add(_runButton);
			CancelButton = _closeButton;

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
				MessageBox.Show(
					this,
					"Synix could not find the project folder. Run this checker from a Visual Studio Build or Rebuild of the Synix project.",
					"Project Folder Not Found",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}
			if (string.IsNullOrWhiteSpace(_publishDirectory))
			{
				MessageBox.Show(
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
				SynixReleaseReadinessChecker checker = new();
				SynixReleaseReadinessReport report = await checker.CheckAsync(
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
				MessageBox.Show(
					this,
					"Windows could not copy the release report.",
					"Copy Failed",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
			}
		}
	}
}
