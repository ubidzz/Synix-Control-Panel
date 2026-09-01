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
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine;

namespace Synix_Control_Panel.Help
{
	public partial class ServerInfo : Form
	{
		private const uint WdaExcludeFromCapture = 0x00000011;
		private const int WmNcHitTest = 0x0084;
		private const int WmNcLeftButtonDown = 0x00A1;
		private const int HtCaption = 0x0002;
		private const int HtLeft = 10;
		private const int HtRight = 11;
		private const int HtTop = 12;
		private const int HtTopLeft = 13;
		private const int HtTopRight = 14;
		private const int HtBottom = 15;
		private const int HtBottomLeft = 16;
		private const int HtBottomRight = 17;
		private const int DwmWindowCornerPreference = 33;
		private const int DwmRound = 2;
		private const int ResizeBorder = 7;

		private static Color SuccessColor => SettingsPalette.Success;
		private static Color DangerColor => SettingsPalette.Danger;
		private static Color BusyColor => SettingsPalette.Warning;
		private static Color IdleColor => ThemeManager.IsDarkMode
			? Color.FromArgb(96, 165, 250)
			: Color.FromArgb(37, 99, 168);

		private readonly GameServer _server;
		private Process? _serverProcess;
		private System.Windows.Forms.Timer? _metricsTimer;
		private DateTime _lastCpuCheckTime;
		private TimeSpan _lastCpuTotalProcessorTime;
		private int _busyStatusFrame;
		private bool _statusIndicatorBusy;
		private Color _statusIndicatorColor = SettingsPalette.Danger;
		private double _currentCpuPercentage;
		private double _currentRamPercentage;

		public ServerInfo()
		{
			InitializeComponent();
			InitializeStatusIndicator();
			_server = new GameServer();
			if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
				ThemeManager.Apply(this);
		}

		public ServerInfo(GameServer server)
		{
			InitializeComponent();
			InitializeStatusIndicator();
			_server = server ?? throw new ArgumentNullException(nameof(server));
			ThemeManager.Apply(this);

			LoadServerData();
			UpdateStatusPresentation(_server.Status);
			UpdatePerformanceDisplay(0, 0);
			InitializeMetricsEngine();
		}

		protected override void OnHandleCreated(EventArgs eventArgs)
		{
			base.OnHandleCreated(eventArgs);

			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
			{
				return;
			}

			if (Properties.Settings.Default.PrivacyMode)
			{
				_ = SetWindowDisplayAffinity(Handle, WdaExcludeFromCapture);
			}

			try
			{
				int preference = DwmRound;
				_ = DwmSetWindowAttribute(
					Handle,
					DwmWindowCornerPreference,
					ref preference,
					sizeof(int));
			}
			catch
			{

			}
		}

		protected override void WndProc(ref Message message)
		{
			base.WndProc(ref message);

			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
				message.Msg != WmNcHitTest ||
				WindowState == FormWindowState.Maximized)
			{
				return;
			}

			Point cursor = PointToClient(Cursor.Position);
			bool left = cursor.X <= ResizeBorder;
			bool right = cursor.X >= ClientSize.Width - ResizeBorder;
			bool top = cursor.Y <= ResizeBorder;
			bool bottom = cursor.Y >= ClientSize.Height - ResizeBorder;

			if (left && top) message.Result = (IntPtr)HtTopLeft;
			else if (right && top) message.Result = (IntPtr)HtTopRight;
			else if (left && bottom) message.Result = (IntPtr)HtBottomLeft;
			else if (right && bottom) message.Result = (IntPtr)HtBottomRight;
			else if (left) message.Result = (IntPtr)HtLeft;
			else if (right) message.Result = (IntPtr)HtRight;
			else if (top) message.Result = (IntPtr)HtTop;
			else if (bottom) message.Result = (IntPtr)HtBottom;
		}

		protected override bool ProcessCmdKey(ref Message message, Keys keyData)
		{
			if (keyData == Keys.Escape)
			{
				Close();
				return true;
			}

			return base.ProcessCmdKey(ref message, keyData);
		}

		private void LoadServerData()
		{
			GameCompatibilitySummary compatibility =
				Core.GetGameCompatibilitySummary(_server.Game);
			bool secretsAvailable = Core
				.TryRevealServerSecrets(
					_server,
					out SynixServerSecrets secrets);
			bool routesAvailable = Core.TryRevealDiscordWebhookRoutes(
				_server,
				out IReadOnlyList<DiscordWebhookRoute> discordRoutes);
			SynixServerPasswords passwords = secrets.Passwords;
			int enabledDiscordRoutes = routesAvailable
				? discordRoutes.Count(route => route.Enabled)
				: 0;

			lblPageHeading.Text = DisplayOrFallback(_server.ServerName, "Server Overview");
			lblPageSubtitle.Text =
				$"{DisplayOrFallback(_server.Game, "Dedicated server")}  •  " +
				$"{compatibility.DisplayName}  •  Live performance and configuration details";

			SetStatusColor(lblRconActiveText, _server.EnableRcon);
			SetStatusColor(lblBackupOnStartText, _server.BackupOnStart);
			SetStatusColor(lbllUpdateOnStartText, _server.UpdateOnStart);
			SetStatusColor(
				lblDiscordActivateText,
				_server.IsDiscordAlertEnabled || enabledDiscordRoutes > 0);

			lblMaxPlayersText.Text = _server.MaxPlayers.ToString();
			lblGamePortText.Text = _server.Port.ToString();
			lblQueryPortText.Text = _server.QueryPort.ToString();
			lblRconPortText.Text = _server.RconPort.ToString();
			lblAppPortText.Text = _server.AppPort?.ToString() ?? "N/A";
			lblServerNameText.Text = DisplayOrFallback(_server.ServerName);
			lblGameServerText.Text = DisplayOrFallback(_server.DisplayGameName);
			lblMapText.Text = DisplayOrFallback(_server.WorldName, "Not Required");
			lblSeedText.Text = DisplayOrFallback(_server.WorldSeed, "Not Required");
			lblCompetitiveText.Text = DisplayOrFallback(
				GameDatabase.IsMinecraft(_server.Game)
					? MinecraftControlProfile.NormalizeGameMode(_server.GameMode)
					: _server.GameMode,
				"Not Required");
			lblRconPasswordText.Text = secretsAvailable
				? DisplayOrFallback(passwords.RconPassword, "Not Required")
				: "Password unavailable";
			lblServerPasswordText.Text = secretsAvailable
				? DisplayOrFallback(passwords.ServerPassword, "Not Required")
				: "Password unavailable";
			lblServerAdminPasswordText.Text = secretsAvailable
				? DisplayOrFallback(passwords.AdminPassword, "Not Required")
				: "Password unavailable";
			lblAutoRestartText.Text = GetActiveDays(_server.RestartDays);
			lblGameVersion.Text = GameDatabase.IsMinecraft(_server.Game)
				? $"{MinecraftControlProfile.NormalizeEdition(_server.MinecraftEdition)} • " +
					DisplayOrFallback(_server.GameVersion, "Latest")
				: "N/A";

			txtServerFolderValue.Text = DisplayOrFallback(_server.InstallPath);
			btnDiscordRoutes.Visible = secretsAvailable &&
				routesAvailable &&
				(!string.IsNullOrWhiteSpace(secrets.DiscordWebhook) || discordRoutes.Count > 0);
			txtExtraArgsValue.Text = DisplayOrFallback(_server.ExtraArgs, "No extra arguments");
		}

		private static string DisplayOrFallback(string? value, string fallback = "N/A")
		{
			return string.IsNullOrWhiteSpace(value) ? fallback : value;
		}

		private static string GetActiveDays(bool[]? days)
		{
			if (days == null || days.Length < 7)
			{
				return "No Days Scheduled";
			}

			string[] names = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
			List<string> active = new();

			for (int index = 0; index < 7; index++)
			{
				if (days[index])
				{
					active.Add(names[index]);
				}
			}

			return active.Count > 0
				? string.Join(", ", active)
				: "No Days Scheduled";
		}

		private static void SetStatusColor(Label label, bool isActive)
		{
			label.Text = isActive ? "On" : "Off";
			label.ForeColor = isActive ? SuccessColor : DangerColor;
		}

		private void InitializeMetricsEngine()
		{
			_metricsTimer = new System.Windows.Forms.Timer
			{
				Interval = 500
			};
			_metricsTimer.Tick += MetricsTimer_Tick;
			_metricsTimer.Start();
		}

		private void InitializeStatusIndicator()
		{
			pnlStatusIndicator.BackColor = SettingsPalette.Card;
			pnlStatusIndicator.Paint += StatusIndicator_Paint;
		}

		private void StatusIndicator_Paint(object? sender, PaintEventArgs eventArgs)
		{
			BusyStatusPresentation.DrawIndicator(
				eventArgs.Graphics,
				pnlStatusIndicator.ClientRectangle,
				_statusIndicatorColor,
				_statusIndicatorBusy,
				_busyStatusFrame);
		}

		private void MetricsTimer_Tick(object? sender, EventArgs eventArgs)
		{
			UpdateStatusPresentation(_server.Status);
			EnsureServerProcessAttached();

			double currentCpu = 0;
			double currentRamGb = 0;

			if (_serverProcess != null)
			{
				try
				{
					if (_serverProcess.HasExited)
					{
						ReleaseServerProcess();
					}
					else
					{
						_serverProcess.Refresh();
						DateTime currentCheckTime = DateTime.UtcNow;
						TimeSpan currentProcessorTime = _serverProcess.TotalProcessorTime;
						double elapsedMilliseconds =
							(currentCheckTime - _lastCpuCheckTime).TotalMilliseconds;

						if (elapsedMilliseconds > 0)
						{
							double processorMilliseconds =
								(currentProcessorTime - _lastCpuTotalProcessorTime).TotalMilliseconds;
							currentCpu = processorMilliseconds /
								elapsedMilliseconds /
								Environment.ProcessorCount * 100.0;
						}

						_lastCpuCheckTime = currentCheckTime;
						_lastCpuTotalProcessorTime = currentProcessorTime;
						currentRamGb = _serverProcess.WorkingSet64 /
							1024.0 / 1024.0 / 1024.0;
					}
				}
				catch (InvalidOperationException)
				{
					ReleaseServerProcess();
				}
				catch (System.ComponentModel.Win32Exception)
				{
					ReleaseServerProcess();
				}
			}

			UpdatePerformanceDisplay(currentCpu, currentRamGb);
		}

		private void EnsureServerProcessAttached()
		{
			if (!_server.PID.HasValue || _server.PID.Value <= 0)
			{
				ReleaseServerProcess();
				return;
			}

			try
			{
				if (_serverProcess == null || _serverProcess.Id != _server.PID.Value)
				{
					ReleaseServerProcess();
					_serverProcess = Process.GetProcessById(_server.PID.Value);
					_lastCpuCheckTime = DateTime.UtcNow;
					_lastCpuTotalProcessorTime = _serverProcess.TotalProcessorTime;
				}
			}
			catch (ArgumentException)
			{
				ReleaseServerProcess();
			}
			catch (InvalidOperationException)
			{
				ReleaseServerProcess();
			}
			catch (System.ComponentModel.Win32Exception)
			{
				ReleaseServerProcess();
			}
		}

		private void UpdatePerformanceDisplay(double cpuPercentage, double ramGb)
		{
			_currentCpuPercentage = Math.Clamp(cpuPercentage, 0, 100);
			lblCpuCardValue.Text = $"{_currentCpuPercentage:0.0}%";
			lblCpuCaption.Text = _serverProcess != null
				? "Live CPU usage  •  Updates twice per second"
				: "Waiting for a running server process";

			double totalRam = MainGUI.Instance != null
				? MainGUI.Instance.systemTotalRamGb
				: 32.0;
			if (totalRam <= 0)
			{
				totalRam = 32.0;
			}

			_currentRamPercentage = Math.Clamp(ramGb / totalRam * 100.0, 0, 100);
			lblRamCardValue.Text = $"{ramGb:0.00} GB";
			lblRamCaption.Text = $"{_currentRamPercentage:0.0}% of {totalRam:0.#} GB system memory";
			lblProcessIdValue.Text = _server.PID.HasValue && _server.PID.Value > 0 ? _server.PID.Value.ToString() : "—";

			UpdateMetricBar(pnlCpuTrack, pnlCpuFill, _currentCpuPercentage);
			UpdateMetricBar(pnlRamTrack, pnlRamFill, _currentRamPercentage);
		}

		private static void UpdateMetricBar(
			Panel track,
			Panel fill,
			double percentage)
		{
			int availableWidth = Math.Max(0, track.ClientSize.Width);
			fill.Width = (int)Math.Round(
				Math.Clamp(percentage, 0, 100) / 100.0 * availableWidth);
			fill.Height = track.ClientSize.Height;
		}

		private void UpdateStatusPresentation(string? rawStatus)
		{
			string status = DisplayOrFallback(rawStatus, "Stopped");
			bool isBusy = BusyStatusPresentation.TryGetBusyState(
				status,
				out string busyState);

			Color statusColor;
			string displayedStatus;

			if (isBusy)
			{
				displayedStatus = busyState;
				statusColor = BusyColor;
				lblStatusCaption.Text = "A server operation is currently in progress";
				_busyStatusFrame =
					(_busyStatusFrame + 1) % BusyStatusPresentation.FrameCount;
			}
			else if (status.Equals("Running", StringComparison.OrdinalIgnoreCase))
			{
				displayedStatus = "Running";
				statusColor = SuccessColor;
				lblStatusCaption.Text = "The game server process is online";
			}
			else if (status.Equals("Stopped", StringComparison.OrdinalIgnoreCase) ||
				status.Equals("Offline", StringComparison.OrdinalIgnoreCase))
			{
				displayedStatus = status;
				statusColor = DangerColor;
				lblStatusCaption.Text = "The game server process is not running";
			}
			else
			{
				displayedStatus = status;
				statusColor = IdleColor;
				lblStatusCaption.Text = "Current state reported by the Synix engine";
			}

			lblStatusCardValue.Text = displayedStatus;
			lblStatusCardValue.ForeColor = statusColor;
			_statusIndicatorBusy = isBusy;
			_statusIndicatorColor = statusColor;
			pnlStatusIndicator.Invalidate();
		}

		private void MetricTrack_SizeChanged(object? sender, EventArgs eventArgs)
		{
			UpdateMetricBar(pnlCpuTrack, pnlCpuFill, _currentCpuPercentage);
			UpdateMetricBar(pnlRamTrack, pnlRamFill, _currentRamPercentage);
		}

		private void ReleaseServerProcess()
		{
			_serverProcess?.Dispose();
			_serverProcess = null;
		}

		private void StopMetricsEngine()
		{
			if (_metricsTimer != null)
			{
				_metricsTimer.Stop();
				_metricsTimer.Tick -= MetricsTimer_Tick;
				_metricsTimer.Dispose();
				_metricsTimer = null;
			}

			ReleaseServerProcess();
		}

		private void ServerInfo_FormClosing(object? sender, FormClosingEventArgs eventArgs)
		{
			StopMetricsEngine();
		}

		private void btnMinimize_Click(object? sender, EventArgs eventArgs)
		{
			WindowState = FormWindowState.Minimized;
		}

		private void btnClose_Click(object? sender, EventArgs eventArgs)
		{
			Close();
		}

		private void btnDiscordRoutes_Click(object? sender, EventArgs eventArgs)
		{
			if (!Core.TryRevealServerSecrets(_server, out SynixServerSecrets secrets) ||
				!Core.TryRevealDiscordWebhookRoutes(
					_server,
					out IReadOnlyList<DiscordWebhookRoute> routes))
			{
				MessageBox.Show(
					this,
					"Synix could not unlock the saved Discord webhook information. Open Server Settings and save the webhooks again.",
					"Discord Webhooks Unavailable",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			if (string.IsNullOrWhiteSpace(secrets.DiscordWebhook) && routes.Count == 0)
			{
				btnDiscordRoutes.Visible = false;
				return;
			}

			using DiscordRoutingInfoDialog dialog = new(
				_server,
				secrets.DiscordWebhook,
				routes);
			dialog.ShowDialog(this);
		}

		private void TitleBar_MouseDown(object? sender, MouseEventArgs eventArgs)
		{
			if (eventArgs.Button != MouseButtons.Left)
			{
				return;
			}

			_ = ReleaseCapture();
			_ = SendMessage(Handle, WmNcLeftButtonDown, HtCaption, 0);
		}

		[DllImport("user32.dll")]
		private static extern uint SetWindowDisplayAffinity(
			IntPtr windowHandle,
			uint affinity);

		[DllImport("user32.dll")]
		private static extern bool ReleaseCapture();

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(
			IntPtr windowHandle,
			int message,
			int wordParameter,
			int longParameter);

		[DllImport("dwmapi.dll")]
		private static extern int DwmSetWindowAttribute(
			IntPtr windowHandle,
			int attribute,
			ref int attributeValue,
			int attributeSize);
	}
}
