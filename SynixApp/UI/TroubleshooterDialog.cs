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
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixApp.Database.GameConfigurations;
using Synix_Control_Panel.SynixApp.SteamCMDHandler;
using System.Diagnostics;

namespace Synix_Control_Panel.SynixEngine
{
	internal sealed partial class TroubleshooterDialog : Form
	{
		private SynixHealthReport? _report;
		private bool _running;
		private readonly GameServer? _readinessServer;
		private bool IsReadinessMode => _readinessServer != null;

		internal TroubleshooterDialog()
		{
			InitializeComponent();
			GridStyler.DarkTheme(resultsGrid);
			GridStyler.ApplyDashboardTheme(resultsGrid);
			GridStyler.ApplyRoundedCorners(resultsGrid, 10);
			ThemeManager.Apply(this);
		}

		internal TroubleshooterDialog(GameServer server) : this()
		{
			_readinessServer = server ?? throw new ArgumentNullException(nameof(server));
			Text = "Server Readiness Center";
			titleLabel.Text = "Server Readiness Center";
			subtitleLabel.Text =
				$"Check whether {server.ServerName} ({server.Game}) is ready to install, start, stop, recover, and connect. Select any problem to see its safe action.";
			resultColumn.HeaderText = "STATUS";
			subjectColumn.HeaderText = "CHECK";
			runButton.Text = "Check Again";
			actionButton.Text = "Select an Action";
		}

		protected override async void OnShown(EventArgs eventArgs)
		{
			base.OnShown(eventArgs);
			await RunChecksAsync();
		}

		private async void RunButton_Click(object? sender, EventArgs eventArgs) =>
			await RunChecksAsync();

		private async Task RunChecksAsync()
		{
			if (_running)
				return;

			_running = true;
			runButton.Enabled = false;
			actionButton.Enabled = false;
			copyButton.Enabled = false;
			closeButton.Enabled = false;
			statusLabel.ForeColor = SettingsPalette.SecondaryText;
			statusLabel.Text = IsReadinessMode
				? $"Checking {_readinessServer!.ServerName}..."
				: "Checking this PC and every installed server...";
			resultsGrid.Rows.Clear();

			try
			{
				Progress<string> progress = new(message => statusLabel.Text = message);
				GameServer[] servers = IsReadinessMode
					? [_readinessServer!]
					: MainGUI.serverList.ToArray();
				_report = await Task.Run(() => SynixTroubleshooter.RunAsync(
					servers,
					checkForUpdates: !IsReadinessMode,
					progress: progress,
					includeUpdateStatus: !IsReadinessMode));
				PopulateReport(_report);
				statusLabel.Text = IsReadinessMode
					? GetReadinessSummary(_report)
					: _report.FailedCount > 0
						? $"ATTENTION NEEDED  •  {_report.FailedCount} failed  •  {_report.WarningCount} warnings  •  {_report.PassedCount} passed"
						: $"HEALTHY  •  {_report.WarningCount} warnings  •  {_report.PassedCount} passed";
				statusLabel.ForeColor = _report.FailedCount > 0
					? SettingsPalette.Danger
					: _report.WarningCount > 0
						? SettingsPalette.Warning
						: SettingsPalette.Success;
				copyButton.Enabled = true;
			}
			catch (Exception exception)
			{
				statusLabel.Text = "The health check could not finish: " + exception.Message;
				statusLabel.ForeColor = SettingsPalette.Danger;
			}
			finally
			{
				_running = false;
				runButton.Enabled = true;
				closeButton.Enabled = true;
				UpdateActionButton();
			}
		}

		private static string GetReadinessSummary(SynixHealthReport report)
		{
			int total = report.Items.Count;
			int readyPercent = total == 0
				? 0
				: (int)Math.Round(report.PassedCount * 100d / total);
			return report.FailedCount > 0
				? $"NOT READY  •  {readyPercent}% passed  •  {report.FailedCount} blocked  •  {report.WarningCount} to review"
				: report.WarningCount > 0
					? $"READY WITH ITEMS TO REVIEW  •  {readyPercent}% passed  •  {report.WarningCount} to review"
					: $"READY  •  {readyPercent}% passed  •  all {report.PassedCount} checks completed";
		}

		private void PopulateReport(SynixHealthReport report)
		{
			resultsGrid.SuspendLayout();
			try
			{
				foreach (SynixHealthItem item in report.Items
					.OrderByDescending(item => item.Level)
					.ThenBy(item => item.Area, StringComparer.OrdinalIgnoreCase)
					.ThenBy(item => item.Subject, StringComparer.OrdinalIgnoreCase))
				{
					int index = resultsGrid.Rows.Add(
						IsReadinessMode ? GetReadinessResultText(item.Level) : item.ResultText,
						item.Area,
						item.Subject,
						item.Details,
						GetActionText(item.Action));
					DataGridViewRow row = resultsGrid.Rows[index];
					row.Tag = item;
					Color color = item.Level switch
					{
						SynixHealthLevel.Passed => SettingsPalette.Success,
						SynixHealthLevel.Warning => SettingsPalette.Warning,
						_ => SettingsPalette.Danger
					};
					row.Cells[0].Style.ForeColor = color;
					row.Cells[0].Style.SelectionForeColor = color;
				}
			}
			finally
			{
				resultsGrid.ResumeLayout();
			}
		}

		private static string GetReadinessResultText(SynixHealthLevel level) => level switch
		{
			SynixHealthLevel.Passed => "Ready",
			SynixHealthLevel.Warning => "Review",
			_ => "Blocked"
		};

		private static string GetActionText(SynixHealthAction action) => action switch
		{
			SynixHealthAction.RepairSteamCmd => "Repair SteamCMD",
			SynixHealthAction.ValidateServerFiles => "Validate Files",
			SynixHealthAction.FixConfiguration => "Fix Config",
			SynixHealthAction.OpenServerFolder => "Open Folder",
			SynixHealthAction.OpenFirewallSettings => "Firewall Settings",
			SynixHealthAction.RecoverProcesses => "Recover Processes",
			SynixHealthAction.OpenLatestLog => "Open Log",
			SynixHealthAction.OpenUpdate => "Open Updates",
			_ => string.Empty
		};

		private void ResultsGrid_SelectionChanged(object? sender, EventArgs eventArgs) =>
			UpdateActionButton();

		private void UpdateActionButton()
		{
			SynixHealthItem? item = GetSelectedItem();
			actionButton.Enabled = !_running && item?.Action != SynixHealthAction.None;
			actionButton.Text = item == null || item.Action == SynixHealthAction.None
				? (IsReadinessMode ? "Select an Action" : "Select a Repair")
				: GetActionText(item.Action);
		}

		private SynixHealthItem? GetSelectedItem() =>
			resultsGrid.SelectedRows.Count > 0
				? resultsGrid.SelectedRows[0].Tag as SynixHealthItem
				: null;

		private async void ActionButton_Click(object? sender, EventArgs eventArgs)
		{
			SynixHealthItem? item = GetSelectedItem();
			if (item == null || item.Action == SynixHealthAction.None || _running)
				return;

			try
			{
				switch (item.Action)
				{
					case SynixHealthAction.RepairSteamCmd:
						statusLabel.Text = "Repairing SteamCMD...";
						await Task.Run(() => SteamCMD.EnsureSteamCMD((message, color) =>
						{
							if (IsHandleCreated)
								BeginInvoke(() => statusLabel.Text = message);
						}));
						break;

					case SynixHealthAction.ValidateServerFiles when item.Server != null:
						await Core.Instance.UpdateServerAndReport(item.Server, "VALIDATE");
						break;

					case SynixHealthAction.FixConfiguration when item.Server != null:
						await FixConfigurationAsync(item.Server);
						break;

					case SynixHealthAction.OpenServerFolder when item.Server != null:
						Core.Instance.OpenServerFolder(item.Server);
						return;

					case SynixHealthAction.OpenFirewallSettings:
						Process.Start(new ProcessStartInfo("firewall.cpl") { UseShellExecute = true });
						return;

					case SynixHealthAction.RecoverProcesses:
						await Core.Instance.RebindProcesses();
						break;

					case SynixHealthAction.OpenLatestLog when item.Server != null:
						Core.Instance.OpenLatestGameLog(item.Server);
						return;

					case SynixHealthAction.OpenUpdate:
						Process.Start(new ProcessStartInfo(Core.ReleasesUri.AbsoluteUri) { UseShellExecute = true });
						return;
				}

				await RunChecksAsync();
			}
			catch (Exception exception)
			{
				MessageBox.Show(
					this,
					exception.Message,
					"Repair Could Not Finish",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
			}
		}

		private async Task FixConfigurationAsync(GameServer server)
		{
			if (!string.Equals(
				server.Status,
				Core.StatusManager.GetStatus(Core.ServerState.Stopped),
				StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException("Stop the server before rebuilding its configuration.");
			}

			DialogResult confirmation = MessageBox.Show(
				this,
				"Synix will rebuild the complete configuration from its trusted template, reapply the saved server values, and preserve a backup. Continue?",
				"Fix Server Configuration",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning);
			if (confirmation != DialogResult.Yes)
				return;

			ConfigurationApplyResult result = await GameFix.ResetManagedConfiguration(server);
			if (!result.Succeeded)
				throw new InvalidOperationException(result.Message);
			FileHandler.SaveServers();
			MessageBox.Show(this, result.Message, "Configuration Rebuilt", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void CopyButton_Click(object? sender, EventArgs eventArgs)
		{
			if (_report == null)
				return;
			try
			{
				Clipboard.SetText(_report.ToPlainText(
					IsReadinessMode
						? "SYNIX SERVER READINESS REPORT"
						: "SYNIX TROUBLESHOOTER REPORT"));
				statusLabel.Text = IsReadinessMode
					? "Server readiness report copied to the clipboard."
					: "Troubleshooter report copied to the clipboard.";
			}
			catch
			{
				MessageBox.Show(this, "Windows could not copy the report.", "Copy Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}
	}
}
