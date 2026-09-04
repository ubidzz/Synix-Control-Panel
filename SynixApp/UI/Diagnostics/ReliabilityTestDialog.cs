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

namespace Synix_Control_Panel.SynixApp.UI.Diagnostics
{
	internal sealed partial class ReliabilityTestDialog : Form
	{
		private CancellationTokenSource? _cancellation;

		internal ReliabilityTestDialog()
		{
			InitializeComponent();
			ThemeManager.Apply(this);
		}

		private async void StartButton_Click(object? sender, EventArgs eventArgs)
		{
			if (_cancellation != null)
				return;
			_cancellation = new CancellationTokenSource();
			startButton.Enabled = false;
			cancelButton.Enabled = true;
			closeButton.Enabled = false;
			copyButton.Enabled = false;
			LocalizationManager.BindText(
				reportBox,
				"Text.AB89F96B667CE80E48CB");
			try
			{
				Progress<string> progress = new(message =>
					statusLabel.Text = LocalizationManager.TranslateRuntimeText(message));
				ReliabilityTestReport report = await Task.Run(() => ReliabilityTestRunner.RunAsync(
					ServerRegistry.Servers.ToArray(),
					TimeSpan.FromMinutes(durationInput.Value),
					TimeSpan.FromSeconds(intervalInput.Value),
					progress,
					_cancellation.Token));
				reportBox.Text = report.ToPlainText();
				LocalizationManager.BindText(
					statusLabel,
					"Diagnostics.Reliability.Completed",
					report.Samples.Count,
					report.PrivateMemoryGrowth / 1024d / 1024d);
				statusLabel.ForeColor = SettingsPalette.Success;
				copyButton.Enabled = true;
			}
			catch (OperationCanceledException)
			{
				LocalizationManager.BindText(
					statusLabel,
					"Text.4CB636A73FAD7D71BC94");
				statusLabel.ForeColor = SettingsPalette.Warning;
			}
			catch (Exception exception)
			{
				statusLabel.Text =
					LocalizationManager.Get("DynamicText.FEB1FFB864616C071102") +
					exception.Message;
				statusLabel.ForeColor = SettingsPalette.Danger;
			}
			finally
			{
				_cancellation.Dispose();
				_cancellation = null;
				startButton.Enabled = true;
				cancelButton.Enabled = false;
				closeButton.Enabled = true;
			}
		}

		private void CancelButton_Click(object? sender, EventArgs eventArgs) => _cancellation?.Cancel();

		private void CopyButton_Click(object? sender, EventArgs eventArgs)
		{
			if (!string.IsNullOrWhiteSpace(reportBox.Text))
				Clipboard.SetText(reportBox.Text);
		}
	}
}
