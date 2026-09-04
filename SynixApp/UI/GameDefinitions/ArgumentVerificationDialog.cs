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
using System.ComponentModel;

namespace Synix_Control_Panel.SynixApp.UI.GameDefinitions
{
	internal sealed partial class ArgumentVerificationDialog : Form
	{
		private string _game = string.Empty;
		private GameArgumentTestPreview? _preview;
		private DateTimeOffset? _testStartedAtUtc;
		private bool _testLaunchRequested;
		private bool _launchVerified;
		private bool _probeInProgress;

		public ArgumentVerificationDialog()
		{
			InitializeComponent();
			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
				return;

			ThemeManager.Apply(this);
		}

		public ArgumentVerificationDialog(string game) : this()
		{
			_game = GameDatabase.GetCanonicalGameName(game);
			_titleLabel.Text = $"Argument Test • {_game}";
			LoadInstalledServers();
		}

		public bool VerificationRecorded { get; private set; }

		private GameServer? SelectedServer =>
			_instanceCombo.SelectedItem is InstalledServerOption option
				? option.Server
				: null;

		private void LoadInstalledServers()
		{
			InstalledServerOption[] servers = ServerRegistry.Servers
				.Where(server => string.Equals(
					GameDatabase.GetCanonicalGameName(server.Game),
					_game,
					StringComparison.OrdinalIgnoreCase))
				.Select(server => new InstalledServerOption(server))
				.OrderBy(option => option.Server.ServerName, StringComparer.OrdinalIgnoreCase)
				.ToArray();

			_instanceCombo.DataSource = servers;
			_instanceCombo.DisplayMember = nameof(InstalledServerOption.DisplayName);
			if (servers.Length == 0)
			{
				_statusLabel.Text =
					"Install this game in Synix before testing its real launch arguments.";
				_statusLabel.ForeColor = SettingsPalette.Warning;
				UpdateButtons();
				return;
			}

			_instanceCombo.SelectedIndex = 0;
			ValidateSelectedServer();
		}

		private void InstanceCombo_SelectedIndexChanged(
			object? sender,
			EventArgs eventArgs)
		{
			ResetLaunchEvidence();
			ValidateSelectedServer();
		}

		private void ValidateButton_Click(object? sender, EventArgs eventArgs)
		{
			ResetLaunchEvidence();
			ValidateSelectedServer();
		}

		private void ValidateSelectedServer()
		{
			GameServer? server = SelectedServer;
			_checksGrid.Rows.Clear();
			_executableBox.Clear();
			_workingDirectoryBox.Clear();
			_argumentsBox.Clear();
			if (server == null)
			{
				_preview = null;
				UpdateButtons();
				return;
			}

			_preview = Core.BuildGameArgumentTestPreview(server);
			_executableBox.Text = _preview.ExecutablePath;
			_workingDirectoryBox.Text = _preview.WorkingDirectory;
			_argumentsBox.Text = _preview.SanitizedArguments;
			foreach (GameArgumentVerificationCheck check in _preview.Checks)
			{
				int rowIndex = _checksGrid.Rows.Add(
					check.Name,
					check.Passed ? "PASS" : "FAIL",
					check.Details);
				_checksGrid.Rows[rowIndex].Tag = check;
			}

			_statusLabel.Text = _preview.IsValid
				? "Command validation passed. Start the test to prove that the real server accepts it."
				: "Command validation failed. Correct the failed checks before starting the test.";
			_statusLabel.ForeColor = _preview.IsValid
				? SettingsPalette.Success
				: SettingsPalette.Danger;
			UpdateButtons();
		}

		private async void StartButton_Click(object? sender, EventArgs eventArgs)
		{
			GameServer? server = SelectedServer;
			if (server == null)
				return;

			ValidateSelectedServer();
			if (_preview?.IsValid != true)
			{
				LocalizedMessageBox.Show(
					this,
					"The command must pass every validation check before Synix can start the argument test.",
					"Argument Test Blocked",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			if (!string.Equals(
				server.Status,
				Core.StatusManager.GetStatus(Core.ServerState.Stopped),
				StringComparison.OrdinalIgnoreCase) ||
				server.PID.HasValue)
			{
				LocalizedMessageBox.Show(
					this,
					"Stop this server first. The test must observe a new launch created from the displayed arguments.",
					"Server Already Active",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			_testLaunchRequested = true;
			_testStartedAtUtc = DateTimeOffset.UtcNow;
			_launchVerified = false;
			_confirmationCheck.Checked = false;
			SetConfirmationAvailable(false);
			_statusLabel.Text =
				"Starting the server through Synix. Waiting for its configured listener to respond...";
			_statusLabel.ForeColor = SettingsPalette.Accent;
			UpdateButtons();
			await Core.Instance.ExecuteStartSequence(server);
			_probeTimer.Start();
			await CheckLaunchEvidenceAsync();
		}

		private async void StopButton_Click(object? sender, EventArgs eventArgs)
		{
			GameServer? server = SelectedServer;
			if (server == null)
				return;

			_stopButton.Enabled = false;
			_statusLabel.Text = "Stopping the test server through Synix...";
			_statusLabel.ForeColor = SettingsPalette.Accent;
			await Core.Instance.StopServerAndReport(server);
			_statusLabel.Text = "The test server is stopped. The completed argument evidence is preserved.";
			_statusLabel.ForeColor = SettingsPalette.SecondaryText;
			UpdateButtons();
		}

		private async void ProbeTimer_Tick(object? sender, EventArgs eventArgs)
		{
			await CheckLaunchEvidenceAsync();
		}

		private async Task CheckLaunchEvidenceAsync()
		{
			if (!_testLaunchRequested || _launchVerified || _probeInProgress)
				return;

			GameServer? server = SelectedServer;
			if (server == null || !_testStartedAtUtc.HasValue)
				return;

			if (string.Equals(
				server.Status,
				Core.StatusManager.GetStatus(Core.ServerState.Stopped),
				StringComparison.OrdinalIgnoreCase) &&
				!server.PID.HasValue)
			{
				_probeTimer.Stop();
				_statusLabel.Text =
					"The server stopped before startup could be verified. Review its recent logs and definition.";
				_statusLabel.ForeColor = SettingsPalette.Danger;
				_testLaunchRequested = false;
				UpdateButtons();
				return;
			}

			GameInfo? definition = GameDatabase.GetGame(server.Game);
			if (definition == null)
				return;

			TimeSpan elapsed = DateTimeOffset.UtcNow - _testStartedAtUtc.Value;
			if (!GameDatabase.SupportsManualConnectionTesting(definition))
			{
				bool processSurvived = server.PID.HasValue ||
					definition.LaunchBehavior.LifecycleTracking ==
						GameLifecycleTrackingMode.ExternalDeployment;
				if (processSurvived && elapsed >= TimeSpan.FromSeconds(30))
				{
					CompleteLaunchEvidence(
						"The server remained active for 30 seconds. This definition does not support a safe automatic network probe.");
				}
				else
				{
					_statusLabel.Text =
						$"The process is active. Waiting for the 30-second startup check ({Math.Max(0, 30 - (int)elapsed.TotalSeconds)} seconds remaining)...";
					_statusLabel.ForeColor = SettingsPalette.Accent;
				}
				UpdateButtons();
				return;
			}

			_probeInProgress = true;
			try
			{
				bool responding = await Core.Instance.ExecuteDynamicProbes(
					server,
					"127.0.0.1");
				if (!responding)
				{
					string localIp = await Core.Instance.GetLocalIP();
					if (!string.IsNullOrWhiteSpace(localIp) && localIp != "127.0.0.1")
					{
						responding = await Core.Instance.ExecuteDynamicProbes(
							server,
							localIp);
					}
				}

				if (responding)
				{
					CompleteLaunchEvidence(
						"The real server accepted the command and responded through its configured network listener.");
				}
				else
				{
					_statusLabel.Text =
						$"Server process started. Waiting for its configured listener ({elapsed:mm\\:ss} elapsed)...";
					_statusLabel.ForeColor = SettingsPalette.Accent;
				}
			}
			catch (Exception exception)
			{
				_statusLabel.Text = $"The startup probe could not complete: {exception.Message}";
				_statusLabel.ForeColor = SettingsPalette.Warning;
			}
			finally
			{
				_probeInProgress = false;
				UpdateButtons();
			}
		}

		private void CompleteLaunchEvidence(string details)
		{
			_launchVerified = true;
			_probeTimer.Stop();
			SetConfirmationAvailable(true);
			_statusLabel.Text = details +
				" Confirm the visible in-game values before recording the verification.";
			_statusLabel.ForeColor = SettingsPalette.Success;
		}

		private void ConfirmationCheck_CheckedChanged(
			object? sender,
			EventArgs eventArgs)
		{
			UpdateButtons();
		}

		private void MarkButton_Click(object? sender, EventArgs eventArgs)
		{
			GameServer? server = SelectedServer;
			if (server == null || _preview?.IsValid != true ||
				!_launchVerified || !_confirmationCheck.Checked)
			{
				return;
			}

			bool recorded = Core.RecordGameVerification(
				server.Game,
				GameVerificationKind.Arguments);
			GameCompatibilityVerification verification =
				Core.GetGameCompatibility(server.Game);
			VerificationRecorded = verification.Arguments != null;
			_statusLabel.Text = recorded
				? $"Argument verification recorded for Synix v{Core.GetCurrentVersion().ToString(3)}."
				: "This game already has argument evidence from this Synix version or a newer version.";
			_statusLabel.ForeColor = VerificationRecorded
				? SettingsPalette.Success
				: SettingsPalette.Warning;
			UpdateButtons();
		}

		private void ChecksGrid_CellFormatting(
			object? sender,
			DataGridViewCellFormattingEventArgs eventArgs)
		{
			if (eventArgs.RowIndex < 0 || eventArgs.ColumnIndex != 1)
				return;

			DataGridViewCellStyle? style = eventArgs.CellStyle;
			if (style == null)
				return;

			style.ForeColor =
				_checksGrid.Rows[eventArgs.RowIndex].Tag is GameArgumentVerificationCheck
				{
					Passed: true
				}
					? SettingsPalette.Success
					: SettingsPalette.Danger;
		}

		private void ResetLaunchEvidence()
		{
			_probeTimer.Stop();
			_testStartedAtUtc = null;
			_testLaunchRequested = false;
			_launchVerified = false;
			_confirmationCheck.Checked = false;
			SetConfirmationAvailable(false);
			VerificationRecorded = false;
		}

		private void SetConfirmationAvailable(bool available)
		{
			_confirmationCheck.Enabled = true;
			_confirmationCheck.AutoCheck = available;
			_confirmationCheck.TabStop = available;
			_confirmationCheck.Cursor = available
				? Cursors.Hand
				: Cursors.Default;
			_confirmationCheck.ForeColor = SettingsPalette.PrimaryText;
		}

		private void UpdateButtons()
		{
			GameServer? server = SelectedServer;
			bool stopped = server != null && string.Equals(
				server.Status,
				Core.StatusManager.GetStatus(Core.ServerState.Stopped),
				StringComparison.OrdinalIgnoreCase) &&
				!server.PID.HasValue;
			_validateButton.Enabled = server != null && !_testLaunchRequested;
			_startButton.Enabled = server != null && stopped &&
				_preview?.IsValid == true && !_testLaunchRequested;
			_stopButton.Enabled = server != null && !stopped;
			_markButton.Enabled = _preview?.IsValid == true &&
				_launchVerified &&
				_confirmationCheck.Checked &&
				!VerificationRecorded;
			_instanceCombo.Enabled = !_testLaunchRequested;
		}

		protected override void OnFormClosed(FormClosedEventArgs eventArgs)
		{
			_probeTimer.Stop();
			base.OnFormClosed(eventArgs);
		}

		private sealed record InstalledServerOption(GameServer Server)
		{
			public string DisplayName =>
				$"{Server.ServerName}  •  {Server.InstallPath}";
		}
	}
}
