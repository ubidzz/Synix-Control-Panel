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
using Synix_Control_Panel.SynixApp.Database.GameDefinitions;
using Synix_Control_Panel.SynixApp.Design;

namespace Synix_Control_Panel.SynixApp.UI.GameDefinitions
{
	internal sealed partial class GameDefinitionValidationDialog : Form
	{
		private bool _validationInProgress;

		public GameDefinitionValidationDialog()
		{
			InitializeComponent();
			ThemeManager.Apply(this);
		}

		protected override async void OnShown(EventArgs eventArgs)
		{
			base.OnShown(eventArgs);
			await RunValidationAsync();
		}

		private async void RunButton_Click(object? sender, EventArgs eventArgs)
		{
			await RunValidationAsync();
		}

		private async Task RunValidationAsync()
		{
			if (_validationInProgress)
				return;

			_validationInProgress = true;
			_runButton.Enabled = false;
			_copyButton.Enabled = false;
			_closeButton.Enabled = false;
			_statusLabel.ForeColor = SettingsPalette.SecondaryText;
			LocalizationManager.BindText(_statusLabel, "Text.D25BF1DE94C6CFCB40D4");
			LocalizationManager.BindText(_reportBox, "Text.D4BEB0C27C58D53CCFA7");

			try
			{
				string? projectDirectory =
					Core.FindProjectDirectory(AppContext.BaseDirectory) ??
					Core.FindProjectDirectory(Environment.CurrentDirectory);
				GameDefinitionValidationReport report = await Task.Run(() =>
					projectDirectory == null
						? GameDefinitionValidator.ValidateEmbeddedLibrary()
						: GameDefinitionValidator.ValidateSourceDirectory(projectDirectory));

				_reportBox.Text = report.ToPlainText();
				_reportBox.SelectionStart = 0;
				_reportBox.ScrollToCaret();
				LocalizationManager.BindText(
					_statusLabel,
					report.IsValid
						? "GameDefinitions.Validation.Summary.Passed"
						: "GameDefinitions.Validation.Summary.Failed",
					report.DefinitionCount,
					report.TemplateCount,
					report.ManagedSettingBindingCount,
					report.DefinitionTestCount,
					report.FailedCount);
				_statusLabel.ForeColor = report.IsValid
					? SettingsPalette.Success
					: SettingsPalette.Danger;
				_copyButton.Enabled = true;
			}
			catch (Exception exception)
			{
				_reportBox.Text = LocalizationManager.TranslateRuntimeText(
					exception.Message);
				_statusLabel.ForeColor = SettingsPalette.Danger;
				LocalizationManager.BindText(_statusLabel, "Text.D613F5D4C11D7D3ACD78");
			}
			finally
			{
				_validationInProgress = false;
				_runButton.Enabled = true;
				_closeButton.Enabled = true;
			}
		}

		private void CopyButton_Click(object? sender, EventArgs eventArgs)
		{
			try
			{
				Clipboard.SetText(_reportBox.Text);
				LocalizationManager.BindText(_statusLabel, "Text.692AE6A41D7574B63232");
			}
			catch (Exception suppressedException)
			{
				ApplicationLogService.WriteSuppressedException(suppressedException);
				LocalizedMessageBox.Show(
					this,
					LocalizationManager.Get("MessageText.E30D1FF9B8D6F1EEE71C"),
					LocalizationManager.Get("MessageText.2C58B2D4975AADC6042D"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
			}
		}
	}
}
