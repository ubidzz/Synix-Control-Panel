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
using System.Globalization;
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine;

namespace Synix_Control_Panel.SynixApp.UI.Help
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
			LocalizationManager.LanguageChanged += InterfaceLanguageChanged;
			Disposed += (_, _) => LocalizationManager.LanguageChanged -= InterfaceLanguageChanged;
		}

		private void InterfaceLanguageChanged(object? sender, EventArgs e)
		{
			if (IsDisposed) return;
			LoadServerData();
			UpdateStatusPresentation(_server.Status);
		}

		private static void ShowServerValue(Control control, string? value, string fallback = "N/A")
		{
			if (string.IsNullOrWhiteSpace(value))
				LocalizationManager.BindText(control, LocalizationManager.ContentKey(fallback));
			else LocalizationManager.SetDataText(control, value);
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
			catch (Exception suppressedException)
			{
				Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException);
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

			ShowServerValue(lblPageHeading, _server.ServerName, "Server Overview");
			lblPageSubtitle.Text = LocalizationManager.FormatContent(
				"{0}  •  {1}  •  Live performance and configuration details",
				DisplayOrFallback(_server.Game, "Dedicated server"), compatibility.DisplayName);

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
			ShowServerValue(lblServerNameText, _server.ServerName);
			ShowServerValue(lblGameServerText, _server.DisplayGameName);
			ShowServerValue(lblMapText, _server.WorldName, "Not Required");
			ShowServerValue(lblSeedText, _server.WorldSeed, "Not Required");
			ShowServerValue(lblCompetitiveText,
				GameDatabase.IsMinecraft(_server.Game)
					? MinecraftControlProfile.NormalizeGameMode(_server.GameMode)
					: _server.GameMode,
				"Not Required");
			ShowServerValue(lblRconPasswordText, secretsAvailable ? passwords.RconPassword : null,
				secretsAvailable ? "Not Required" : "Password unavailable");
			ShowServerValue(lblServerPasswordText, secretsAvailable ? passwords.ServerPassword : null,
				secretsAvailable ? "Not Required" : "Password unavailable");
			ShowServerValue(lblServerAdminPasswordText, secretsAvailable ? passwords.AdminPassword : null,
				secretsAvailable ? "Not Required" : "Password unavailable");
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
			return string.IsNullOrWhiteSpace(value) ? LocalizationManager.GetContent(fallback) : value;
		}

		private static string GetActiveDays(bool[]? days)
		{
			if (days == null || days.Length < 7)
			{
				return LocalizationManager.GetContent("No Days Scheduled");
			}

			string[] names = CultureInfo.GetCultureInfo(LocalizationManager.CurrentLanguageCode).DateTimeFormat.AbbreviatedDayNames;
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
				: LocalizationManager.GetContent("No Days Scheduled");
		}

		private static void SetStatusColor(Label label, bool isActive)
		{
			label.Text = LocalizationManager.GetContent(isActive ? "On" : "Off");
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
			lblCpuCaption.Text = LocalizationManager.GetContent(_serverProcess != null
				? "Live CPU usage  •  Updates twice per second"
				: "Waiting for a running server process");

			double totalRam = Core.TotalRamGb > 0
				? Core.TotalRamGb
				: 32.0;
			if (totalRam <= 0)
			{
				totalRam = 32.0;
			}

			_currentRamPercentage = Math.Clamp(ramGb / totalRam * 100.0, 0, 100);
			lblRamCardValue.Text = $"{ramGb:0.00} GB";
			lblRamCaption.Text = LocalizationManager.FormatContent("{0:0.0}% of {1:0.#} GB system memory", _currentRamPercentage, totalRam);
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
				lblStatusCaption.Text = LocalizationManager.GetContent("A server operation is currently in progress");
				_busyStatusFrame =
					(_busyStatusFrame + 1) % BusyStatusPresentation.FrameCount;
			}
			else if (status.Equals("Running", StringComparison.OrdinalIgnoreCase))
			{
				displayedStatus = "Running";
				statusColor = SuccessColor;
				lblStatusCaption.Text = LocalizationManager.GetContent("The game server process is online");
			}
			else if (status.Equals("Stopped", StringComparison.OrdinalIgnoreCase) ||
				status.Equals("Offline", StringComparison.OrdinalIgnoreCase))
			{
				displayedStatus = status;
				statusColor = DangerColor;
				lblStatusCaption.Text = LocalizationManager.GetContent("The game server process is not running");
			}
			else
			{
				displayedStatus = status;
				statusColor = IdleColor;
				lblStatusCaption.Text = LocalizationManager.GetContent("Current state reported by the Synix engine");
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
				LocalizedMessageBox.Show(
					this,
					LocalizationManager.GetContent("Synix could not unlock the saved Discord webhook information. Open Server Settings and save the webhooks again."),
					LocalizationManager.GetContent("Discord Webhooks Unavailable"),
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
