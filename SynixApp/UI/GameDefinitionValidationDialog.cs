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

namespace Synix_Control_Panel.SynixEngine
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
			_statusLabel.Text = "Testing every built-in definition and template safely...";
			_reportBox.Text = "Reading and testing the project game-definition library...";

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
				_statusLabel.Text = report.IsValid
					? $"PASSED  •  {report.DefinitionCount} games  •  " +
						$"{report.TemplateCount} templates  •  " +
						$"{report.ManagedSettingBindingCount} setting bindings  •  " +
						$"{report.DefinitionTestCount} tests"
					: $"FAILED  •  {report.FailedCount} problem(s) must be corrected";
				_statusLabel.ForeColor = report.IsValid
					? SettingsPalette.Success
					: SettingsPalette.Danger;
				_copyButton.Enabled = true;
			}
			catch (Exception exception)
			{
				_reportBox.Text = exception.Message;
				_statusLabel.ForeColor = SettingsPalette.Danger;
				_statusLabel.Text = "The game-definition tests could not finish.";
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
				_statusLabel.Text = "Definition test report copied to the clipboard.";
			}
			catch
			{
				LocalizedMessageBox.Show(
					this,
					"Windows could not copy the validation report.",
					"Copy Failed",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
			}
		}
	}
}
