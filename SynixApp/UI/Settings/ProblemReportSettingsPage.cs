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
						"A problem-report action is missing its language resource key.");
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
			lblSynixVersion.Text =
				$"Synix version: {Core.GetProblemReportSynixVersion()}";
			lblWindowsVersion.Text =
				$"Windows version: {Core.GetProblemReportWindowsVersion()}";
			RefreshVerificationDisplay();
		}

		private void RefreshVerificationDisplay()
		{
			string serverType = cmbServerType.SelectedItem as string ?? string.Empty;
			if (serverType.Length == 0)
			{
				lblVerification.Text =
					"Choose a server type to show its local verification history.";
				return;
			}

			GameCompatibilityVerification verification =
				Core.GetGameCompatibility(serverType);
			lblVerification.Text = string.Join(
				Environment.NewLine,
				FormatVerification("Install", verification.Install),
				FormatVerification("Start", verification.Start),
				FormatVerification("Stop", verification.Stop),
				FormatVerification("Monitoring", verification.Monitoring));
		}

		private void RefreshGitHubConnectionDisplay()
		{
			GitHubConnectionInfo? connection;
			try
			{
				connection = Core.GetGitHubConnectionInfo();
			}
			catch (ProblemReportException)
			{
				connection = null;
				Core.DisconnectGitHub();
			}

			bool connected = connection != null;
			btnConnectGitHub.Visible = !connected;
			btnDisconnectGitHub.Visible = connected;
			btnSubmitGitHub.Enabled = connected && !_operationInProgress;
			lblConnectionStatus.Text = connected
				? $"Connected to GitHub as {connection!.UserName}. Reports can be posted directly."
				: "GitHub is not connected. Copy and Discord options still work.";
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

			SetOperationState(true, "Requesting a secure sign-in code from GitHub...");
			try
			{
				GitHubDeviceAuthorization authorization =
					await Core.BeginGitHubConnectionAsync(_lifetimeCancellation.Token);
				TryCopyText(authorization.UserCode);
				LocalizedMessageBox.Show(
					FindForm(),
					$"GitHub sign-in code:\n\n{authorization.UserCode}\n\nThe code was copied to the clipboard. Select OK to open GitHub, paste the code, and approve Synix.",
					"Connect GitHub",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				OpenAllowedUrl(authorization.VerificationUri.AbsoluteUri);
				lblReportStatus.Text =
					$"Waiting for GitHub approval. The code expires at {authorization.ExpiresAtUtc.ToLocalTime():h:mm tt}.";

				GitHubConnectionInfo connection =
					await Core.CompleteGitHubConnectionAsync(
						authorization,
						_lifetimeCancellation.Token);
				lblReportStatus.Text =
					$"GitHub connected successfully as {connection.UserName}.";
				lblReportStatus.ForeColor = SettingsPalette.Success;
			}
			catch (OperationCanceledException) when (_lifetimeCancellation.IsCancellationRequested)
			{
			}
			catch (Exception exception) when (exception is ProblemReportException or
				HttpRequestException or
				TaskCanceledException)
			{
				ShowOperationError(
					"GitHub Connection",
					exception is TaskCanceledException
						? "GitHub did not respond in time. Check the internet connection and try again."
						: exception.Message);
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
				"Disconnect GitHub from Synix?\n\nSynix will delete the encrypted connection saved on this computer, then open GitHub so you can revoke the authorization from your account.",
				"Disconnect GitHub",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button2);
			if (result != DialogResult.Yes)
				return;

			if (!Core.DisconnectGitHub())
			{
				ShowOperationError(
					"Disconnect GitHub",
					"Windows could not remove the saved GitHub connection. Close other Synix windows and try again.");
				return;
			}

			RefreshGitHubConnectionDisplay();
			lblReportStatus.Text =
				"The local connection was removed. Revoke Synix on the GitHub page that opened.";
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
				$"Submit this public GitHub issue?\n\n{report.Title}\n\nReview the description fields first and make sure they do not contain private information.",
				"Submit Problem Report",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button2);
			if (confirmation != DialogResult.Yes)
				return;

			SetOperationState(true, "Submitting the report directly to GitHub...");
			try
			{
				GitHubIssueResult issue = await Core.SubmitProblemReportToGitHubAsync(
					report,
					_lifetimeCancellation.Token);
				lblReportStatus.Text =
					$"Report submitted successfully as GitHub issue #{issue.Number}. No browser was opened.";
				lblReportStatus.ForeColor = SettingsPalette.Success;
				LocalizedMessageBox.Show(
					FindForm(),
					$"The report was submitted successfully as GitHub issue #{issue.Number}.",
					"Report Submitted",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
			catch (OperationCanceledException) when (_lifetimeCancellation.IsCancellationRequested)
			{
			}
			catch (Exception exception) when (exception is ProblemReportException or
				HttpRequestException or
				TaskCanceledException)
			{
				ShowOperationError(
					"Submit Problem Report",
					exception is TaskCanceledException
						? "GitHub did not respond in time. Check the internet connection and try again."
						: exception.Message);
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
					"Copy Problem Report",
					"Windows could not access the clipboard. Try again after closing another clipboard program.");
				return;
			}

			lblReportStatus.Text =
				"The privacy-filtered report was copied and is ready to paste into the Discord bug forum.";
			lblReportStatus.ForeColor = SettingsPalette.Success;
		}

		private void btnOpenDiscord_Click(
			object? sender,
			EventArgs eventArgs)
		{
			OpenAllowedUrl(Core.DiscordBugForumUrl);
			lblReportStatus.Text =
				"Discord opened. Select New Post in the bug-reporting forum and paste the copied report.";
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
				ShowOperationError("Problem Report", exception.Message);
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
				? $"{name} verified: Not yet"
				: $"{name} verified: Synix v{evidence.SynixVersion} on {evidence.VerifiedAtUtc.ToLocalTime():d}";
		}

		private static bool TryCopyText(string text)
		{
			try
			{
				Clipboard.SetText(text);
				return true;
			}
			catch (ExternalException)
			{
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
					throw new ProblemReportException("Synix rejected an unexpected web address.");
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
					"Open Web Page",
					$"Windows could not open the page. {exception.Message}");
			}
		}
	}
}
