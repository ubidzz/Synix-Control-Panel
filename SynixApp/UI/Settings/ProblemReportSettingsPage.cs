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
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.Localization;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;

namespace Synix_Control_Panel.SynixApp.UI.Settings
{
	public partial class ProblemReportSettingsPage : UserControl
	{
		private static readonly string[] ProblemActionResourceKeys =
		[
			"ProblemAction.ServerInstallation",
			"ProblemAction.UpdateValidation",
			"ProblemAction.ServerStartup",
			"ProblemAction.ServerShutdown",
			"ProblemAction.RestartWatchdog",
			"ProblemAction.IncorrectStatus",
			"ProblemAction.ResourceMonitoring",
			"ProblemAction.LocalNetwork",
			"ProblemAction.PublicNetwork",
			"ProblemAction.PortsFirewallRcon",
			"ProblemAction.ServerBackups",
			"ProblemAction.TransferExport",
			"ProblemAction.TransferImport",
			"ProblemAction.TransferVerification",
			"ProblemAction.SettingsPasswords",
			"ProblemAction.DiscordAlerts",
			"ProblemAction.SynixUpdate",
			"ProblemAction.InstallationPackaging",
			"ProblemAction.WindowDisplay",
			"ProblemAction.CrashFreeze",
			"ProblemAction.TemplateLaunch",
			"ProblemAction.Other"
		];

		private readonly CancellationTokenSource _lifetimeCancellation = new();
		private bool _operationInProgress;

		public ProblemReportSettingsPage()
		{
			InitializeComponent();

			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
				return;

			PopulateProblemActionOptions();
			UpdateEnglishReportWarning();
			LocalizationManager.LanguageChanged += InterfaceLanguageChanged;
			Disposed += (_, _) =>
				LocalizationManager.LanguageChanged -= InterfaceLanguageChanged;
			RefreshServerTypes();
			RefreshAutomaticInformation();
			RefreshGitHubConnectionDisplay();
		}

		private void InterfaceLanguageChanged(
			object? sender,
			EventArgs eventArgs)
		{
			PopulateProblemActionOptions();
			UpdateEnglishReportWarning();
			RefreshAutomaticInformation();
			RefreshGitHubConnectionDisplay();
		}

		private void UpdateEnglishReportWarning()
		{
			bool showWarning = !string.Equals(
				LocalizationManager.CurrentLanguageCode,
				LocalizationManager.DefaultLanguageCode,
				StringComparison.OrdinalIgnoreCase);
			lblEnglishReportWarning.Text = LocalizationManager.Get(
				"Report.EnglishRequiredWarning");
			lblEnglishReportWarning.Visible = showWarning;
			lblPrivacyNotice.Top = showWarning ? 458 : 426;
			reportCard.Height = showWarning ? 518 : 482;
			systemCard.Top = reportCard.Bottom + 16;
			sendCard.Top = systemCard.Bottom + 16;
			pageScroll.PerformLayout();
		}

		private void PopulateProblemActionOptions()
		{
			string? selectedValue =
				(cmbFailedAction.SelectedItem as LocalizedOption)?.Value;
			cmbFailedAction.Items.Clear();

			for (int index = 0; index < Core.ProblemReportActions.Count; index++)
			{
				string resourceKey = index < ProblemActionResourceKeys.Length
					? ProblemActionResourceKeys[index]
					: throw new InvalidOperationException(
						LocalizationManager.Get(
							"Report.Error.ActionResourceMissing"));
				cmbFailedAction.Items.Add(new LocalizedOption(
					Core.ProblemReportActions[index],
					resourceKey));
			}

			if (!string.IsNullOrWhiteSpace(selectedValue))
			{
				cmbFailedAction.SelectedItem = cmbFailedAction.Items
					.Cast<LocalizedOption>()
					.FirstOrDefault(option => option.Value == selectedValue);
			}
		}

		protected override void OnVisibleChanged(EventArgs eventArgs)
		{
			base.OnVisibleChanged(eventArgs);
			if (!Visible || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
				return;

			RefreshServerTypes();
			RefreshAutomaticInformation();
			RefreshGitHubConnectionDisplay();
		}

		private void RefreshServerTypes()
		{
			string selected = cmbServerType.SelectedItem as string ?? string.Empty;
			string[] serverTypes = GameDatabase.GetGameList()
				.Select(game => GameDatabase.GetCanonicalGameName(game.Game))
				.Where(game => !string.IsNullOrWhiteSpace(game))
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.OrderBy(game => game, StringComparer.OrdinalIgnoreCase)
				.ToArray();

			cmbServerType.BeginUpdate();
			try
			{
				cmbServerType.Items.Clear();
				cmbServerType.Items.AddRange(serverTypes);
				int selectedIndex = Array.FindIndex(
					serverTypes,
					game => game.Equals(selected, StringComparison.OrdinalIgnoreCase));
				if (selectedIndex >= 0)
					cmbServerType.SelectedIndex = selectedIndex;
			}
			finally
			{
				cmbServerType.EndUpdate();
			}
		}

		private void RefreshAutomaticInformation()
		{
			LocalizationManager.BindText(
				lblSynixVersion,
				"Report.Automatic.SynixVersion",
				Core.GetProblemReportSynixVersion());
			LocalizationManager.BindText(
				lblWindowsVersion,
				"Report.Automatic.WindowsVersion",
				Core.GetProblemReportWindowsVersion());
			RefreshVerificationDisplay();
		}

		private void RefreshVerificationDisplay()
		{
			string serverType = cmbServerType.SelectedItem as string ?? string.Empty;
			if (serverType.Length == 0)
			{
				LocalizationManager.BindText(
					lblVerification,
					"Text.58ECD5D931201294DE1E");
				return;
			}

			GameCompatibilityVerification verification =
				Core.GetGameCompatibility(serverType);
			lblVerification.Text = string.Join(
				Environment.NewLine,
				FormatVerification(
					LocalizationManager.Get("VerificationStep.Install"),
					verification.Install),
				FormatVerification(
					LocalizationManager.Get("VerificationStep.Start"),
					verification.Start),
				FormatVerification(
					LocalizationManager.Get("VerificationStep.Stop"),
					verification.Stop),
				FormatVerification(
					LocalizationManager.Get("VerificationStep.Monitoring"),
					verification.Monitoring));
		}

		private void RefreshGitHubConnectionDisplay()
		{
			GitHubConnectionInfo? connection;
			try
			{
				connection = Core.GetGitHubConnectionInfo();
			}
			catch (ProblemReportException suppressedException)
			{
				ApplicationLogService.WriteSuppressedException(suppressedException);
				connection = null;
				Core.DisconnectGitHub();
			}

			bool connected = connection != null;
			btnConnectGitHub.Visible = !connected;
			btnDisconnectGitHub.Visible = connected;
			btnSubmitGitHub.Enabled = connected && !_operationInProgress;
			LocalizationManager.BindText(
				lblConnectionStatus,
				connected
					? "Report.GitHub.Connected"
					: "Text.88ED3B522C4F0A8BD740",
				connection?.UserName ?? string.Empty);
			lblConnectionStatus.ForeColor = connected
				? SettingsPalette.Success
				: SettingsPalette.SecondaryText;
		}

		private async void btnConnectGitHub_Click(
			object? sender,
			EventArgs eventArgs)
		{
			if (_operationInProgress)
				return;

			SetOperationState(
				true,
				LocalizationManager.Get("Report.GitHub.RequestingCode"));
			try
			{
				GitHubDeviceAuthorization authorization =
					await Core.BeginGitHubConnectionAsync(_lifetimeCancellation.Token);
				TryCopyText(authorization.UserCode);
				LocalizedMessageBox.Show(
					FindForm(),
					LocalizationManager.Get(
						"Report.GitHub.DeviceCode.Body",
						authorization.UserCode),
					LocalizationManager.Get("Text.4027E5B24418520F3EFE"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				OpenAllowedUrl(authorization.VerificationUri.AbsoluteUri);
				LocalizationManager.BindText(
					lblReportStatus,
					"Report.GitHub.Waiting",
					authorization.ExpiresAtUtc.ToLocalTime().ToString(
						"t",
						System.Globalization.CultureInfo.CurrentUICulture));

				GitHubConnectionInfo connection =
					await Core.CompleteGitHubConnectionAsync(
						authorization,
						_lifetimeCancellation.Token);
				LocalizationManager.BindText(
					lblReportStatus,
					"Report.GitHub.ConnectedSuccess",
					connection.UserName);
				lblReportStatus.ForeColor = SettingsPalette.Success;
			}
			catch (OperationCanceledException suppressedException) when (_lifetimeCancellation.IsCancellationRequested)
			{
				ApplicationLogService.WriteSuppressedException(suppressedException);
			}
			catch (Exception exception) when (exception is ProblemReportException or
				HttpRequestException or
				TaskCanceledException)
			{
				ShowOperationError(
					LocalizationManager.Get("Report.GitHub.ConnectionTitle"),
					exception is TaskCanceledException
						? LocalizationManager.Get("Report.GitHub.Timeout")
						: LocalizationManager.TranslateRuntimeText(exception.Message));
			}
			finally
			{
				SetOperationState(false);
				RefreshGitHubConnectionDisplay();
			}
		}

		private void btnDisconnectGitHub_Click(
			object? sender,
			EventArgs eventArgs)
		{
			if (_operationInProgress)
				return;

			DialogResult result = LocalizedMessageBox.Show(
				FindForm(),
				LocalizationManager.Get("MessageText.53B01E5FCFDE56675C9A"),
				LocalizationManager.Get("Text.6C47B175B49C03E40EB6"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button2);
			if (result != DialogResult.Yes)
				return;

			if (!Core.DisconnectGitHub())
			{
				ShowOperationError(
					LocalizationManager.Get("Text.6C47B175B49C03E40EB6"),
					LocalizationManager.Get("Report.GitHub.DisconnectFailed"));
				return;
			}

			RefreshGitHubConnectionDisplay();
			LocalizationManager.BindText(
				lblReportStatus,
				"Text.849FF4D03C300A88F143");
			lblReportStatus.ForeColor = SettingsPalette.Success;
			OpenAllowedUrl(Core.GitHubAuthorizationSettingsUrl);
		}

		private async void btnSubmitGitHub_Click(
			object? sender,
			EventArgs eventArgs)
		{
			if (_operationInProgress)
				return;

			PreparedProblemReport? report = TryPrepareReport();
			if (report == null)
				return;

			DialogResult confirmation = LocalizedMessageBox.Show(
				FindForm(),
				LocalizationManager.Get("Report.Submit.Confirm", report.Title),
				LocalizationManager.Get("MessageText.3608D4A724B6D3D6CC3C"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button2);
			if (confirmation != DialogResult.Yes)
				return;

			SetOperationState(
				true,
				LocalizationManager.Get("Report.Submit.Progress"));
			try
			{
				GitHubIssueResult issue = await Core.SubmitProblemReportToGitHubAsync(
					report,
					_lifetimeCancellation.Token);
				LocalizationManager.BindText(
					lblReportStatus,
					"Report.Submit.SuccessStatus",
					issue.Number);
				lblReportStatus.ForeColor = SettingsPalette.Success;
				LocalizedMessageBox.Show(
					FindForm(),
					LocalizationManager.Get("Report.Submit.SuccessBody", issue.Number),
					LocalizationManager.Get("MessageText.813762AC49C63A97BA7C"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
			catch (OperationCanceledException suppressedException) when (_lifetimeCancellation.IsCancellationRequested)
			{
				ApplicationLogService.WriteSuppressedException(suppressedException);
			}
			catch (Exception exception) when (exception is ProblemReportException or
				HttpRequestException or
				TaskCanceledException)
			{
				ShowOperationError(
					LocalizationManager.Get("MessageText.3608D4A724B6D3D6CC3C"),
					exception is TaskCanceledException
						? LocalizationManager.Get("Report.GitHub.Timeout")
						: LocalizationManager.TranslateRuntimeText(exception.Message));
			}
			finally
			{
				SetOperationState(false);
				RefreshGitHubConnectionDisplay();
			}
		}

		private void btnCopyReport_Click(
			object? sender,
			EventArgs eventArgs)
		{
			PreparedProblemReport? report = TryPrepareReport();
			if (report == null)
				return;

			string clipboardReport = $"{report.Title}{Environment.NewLine}{Environment.NewLine}{report.Body}";
			const int discordSafeLength = 1900;
			if (clipboardReport.Length > discordSafeLength)
				clipboardReport = clipboardReport[..discordSafeLength].TrimEnd() + "\n\n[Report shortened for Discord]";

			if (!TryCopyText(clipboardReport))
			{
				ShowOperationError(
					LocalizationManager.Get("Text.62372ADDC6612D415FFA"),
					LocalizationManager.Get("Report.Copy.Failed"));
				return;
			}

			LocalizationManager.BindText(
				lblReportStatus,
				"Text.199AA28C30C28597C6E2");
			lblReportStatus.ForeColor = SettingsPalette.Success;
		}

		private void btnOpenDiscord_Click(
			object? sender,
			EventArgs eventArgs)
		{
			OpenAllowedUrl(Core.DiscordBugForumUrl);
			LocalizationManager.BindText(
				lblReportStatus,
				"Text.84A9F83EFC6F8A9F3401");
			lblReportStatus.ForeColor = SettingsPalette.SecondaryText;
		}

		private void cmbServerType_SelectedIndexChanged(
			object? sender,
			EventArgs eventArgs)
		{
			RefreshVerificationDisplay();
		}

		private PreparedProblemReport? TryPrepareReport()
		{
			try
			{
				return Core.PrepareProblemReport(new ProblemReportDraft(
					cmbServerType.SelectedItem as string ?? string.Empty,
					(cmbFailedAction.SelectedItem as LocalizedOption)?.Value
						?? string.Empty,
					txtSummary.Text,
					txtWhatHappened.Text,
					txtExpected.Text));
			}
			catch (ProblemReportException exception)
			{
				ShowOperationError(
					LocalizationManager.Get("Report.Problem.Title"),
					LocalizationManager.TranslateRuntimeText(exception.Message));
				return null;
			}
		}

		private void SetOperationState(bool busy, string? status = null)
		{
			_operationInProgress = busy;
			btnConnectGitHub.Enabled = !busy;
			btnDisconnectGitHub.Enabled = !busy;
			btnSubmitGitHub.Enabled = !busy && Core.GetGitHubConnectionInfo() != null;
			btnCopyReport.Enabled = !busy;
			if (!string.IsNullOrWhiteSpace(status))
			{
				lblReportStatus.Text = status;
				lblReportStatus.ForeColor = SettingsPalette.SecondaryText;
			}
		}

		private void ShowOperationError(string title, string message)
		{
			lblReportStatus.Text = message;
			lblReportStatus.ForeColor = SettingsPalette.Danger;
			LocalizedMessageBox.Show(
				FindForm(),
				message,
				title,
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
		}

		private static string FormatVerification(
			string name,
			GameVerificationEvidence? evidence)
		{
			return evidence == null
				? LocalizationManager.Get("Report.Verification.NotYet", name)
				: LocalizationManager.Get(
					"Report.Verification.Recorded",
					name,
					evidence.SynixVersion,
					evidence.VerifiedAtUtc.ToLocalTime().ToString(
						"d",
						System.Globalization.CultureInfo.CurrentUICulture));
		}

		private static bool TryCopyText(string text)
		{
			try
			{
				Clipboard.SetText(text);
				return true;
			}
			catch (ExternalException suppressedException)
			{
				ApplicationLogService.WriteSuppressedException(suppressedException);
				return false;
			}
		}

		private void OpenAllowedUrl(string rawUrl)
		{
			try
			{
				if (!Uri.TryCreate(rawUrl, UriKind.Absolute, out Uri? uri) ||
					uri.Scheme != Uri.UriSchemeHttps ||
					!(uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) ||
					  uri.Host.Equals("discord.gg", StringComparison.OrdinalIgnoreCase)))
				{
					throw new ProblemReportException(LocalizationManager.Get(
						"Report.UnexpectedWebAddress"));
				}

				Process.Start(new ProcessStartInfo(uri.AbsoluteUri)
				{
					UseShellExecute = true
				});
			}
			catch (Exception exception) when (exception is Win32Exception or
				InvalidOperationException or
				ProblemReportException)
			{
				ShowOperationError(
					LocalizationManager.Get("Report.OpenPage.Title"),
					LocalizationManager.Get(
						"Report.OpenPage.Failed",
						exception.Message));
			}
		}
	}
}
