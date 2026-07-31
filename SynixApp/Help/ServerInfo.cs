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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Synix_Control_Panel.Help
{
	public partial class ServerInfo : Form
	{
		[DllImport("user32.dll")]
		private static extern uint SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);
		private const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

		private GameServer _server;
		private Process _serverProcess;
		private System.Windows.Forms.Timer _metricsTimer;
		private DateTime _lastCpuCheckTime;
		private TimeSpan _lastCpuTotalProcessorTime;

		public ServerInfo(GameServer server)
		{
			InitializeComponent();
			_server = server;

			if (Properties.Settings.Default.PrivacyMode)
			{
				SetWindowDisplayAffinity(this.Handle, WDA_EXCLUDEFROMCAPTURE);
			}

			LoadServerData();
			InitializeMetricsEngine();
		}

		// ====================================================================
		// SECTION 1: STATIC SERVER DATA (From original ServerInfo)
		// ====================================================================
		private void LoadServerData()
		{
			if (_server == null) return;

			lblMaxPlayersText.Text = _server.MaxPlayers.ToString();
			lblGamePortText.Text = _server.Port.ToString();
			lblQueryPortText.Text = _server.QueryPort.ToString();
			lblRconPortText.Text = _server.RconPort.ToString();

			lblAppPortText.Text = _server.AppPort?.ToString() ?? "N/A";

			SetStatusColor(lblRconActiveText, _server.EnableRcon);
			SetStatusColor(lblBackupOnStartText, _server.BackupOnStart);
			SetStatusColor(lbllUpdateOnStartText, _server.UpdateOnStart);
			SetStatusColor(lblDiscordActivateText, _server.IsDiscordAlertEnabled);

			lblServerNameText.Text = _server.ServerName;
			lblGameServerText.Text = _server.Game;
			lblMapText.Text = _server.WorldName;
			lblSeedText.Text = _server.WorldSeed;
			lblCompetitiveText.Text = _server.GameMode;
			lblRconPasswordText.Text = _server.RconPassword;

			lblDiscordWebhookText.Text = _server.DiscordWebhook;
			lblServerPasswordText.Text = _server.Password;
			lblServerAdminPasswordText.Text = _server.AdminPassword;
			lblServerFolderText.Text = _server.InstallPath;
			lblExtraArgsText.Text = _server.ExtraArgs;

			lblAutoRestartText.Text = GetActiveDays(_server.RestartDays);
		}

		private string GetActiveDays(bool[] days)
		{
			if (days == null || days.Length < 7) return "None";
			string[] names = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
			List<string> active = new List<string>();

			for (int i = 0; i < 7; i++)
			{
				if (days[i]) active.Add(names[i]);
			}

			return active.Count > 0 ? string.Join(", ", active) : "No Days Scheduled";
		}

		private void SetStatusColor(Label label, bool isActive)
		{
			if (isActive)
			{
				label.Text = "On";
				label.ForeColor = Color.LimeGreen;
			}
			else
			{
				label.Text = "Off";
				label.ForeColor = Color.Red;
			}
		}

		// ====================================================================
		// SECTION 2: LIVE TELEMETRY ENGINE
		// ====================================================================
		private void InitializeMetricsEngine()
		{
			_metricsTimer = new System.Windows.Forms.Timer();
			_metricsTimer.Interval = 1000;
			_metricsTimer.Tick += MetricsTimer_Tick;
			_metricsTimer.Start();
		}

		private void MetricsTimer_Tick(object sender, EventArgs e)
		{
			if (_server.PID.HasValue && _server.PID > 0)
			{
				try
				{
					if (_serverProcess == null || _serverProcess.Id != _server.PID.Value)
					{
						_serverProcess = Process.GetProcessById(_server.PID.Value);
						_lastCpuCheckTime = DateTime.Now;
						_lastCpuTotalProcessorTime = _serverProcess.TotalProcessorTime;
					}
				}
				catch
				{
					_serverProcess = null; // Process ended or is invalid
				}
			}
			else
			{
				_serverProcess = null;
			}

			double currentCpu = 0;
			double currentRamGb = 0;

			if (_serverProcess != null && !_serverProcess.HasExited)
			{
				_serverProcess.Refresh();

				TimeSpan currentTotalProcessorTime = _serverProcess.TotalProcessorTime;
				DateTime currentCheckTime = DateTime.Now;

				double cpuUsage = (currentTotalProcessorTime - _lastCpuTotalProcessorTime).TotalMilliseconds /
								  (currentCheckTime - _lastCpuCheckTime).TotalMilliseconds;

				currentCpu = (cpuUsage / Environment.ProcessorCount) * 100;

				_lastCpuCheckTime = currentCheckTime;
				_lastCpuTotalProcessorTime = currentTotalProcessorTime;

				currentRamGb = _serverProcess.WorkingSet64 / 1024.0 / 1024.0 / 1024.0;

				if (lblStatusCardValue != null)
				{
					lblStatusCardValue.Text = "ONLINE";
					lblStatusCardValue.ForeColor = Color.LimeGreen;
				}
			}
			else
			{
				if (lblStatusCardValue != null)
				{
					lblStatusCardValue.Text = "OFFLINE";
					lblStatusCardValue.ForeColor = Color.IndianRed;
				}
			}

			if (lblCpuCardValue != null) lblCpuCardValue.Text = $"{currentCpu:0.0}%";
			if (lblRamCardValue != null) lblRamCardValue.Text = $"{currentRamGb:0.00} GB";

			double totalRam = MainGUI.Instance != null ? MainGUI.Instance.systemTotalRamGb : 32.0;
			double ramPercentage = (currentRamGb / totalRam) * 100;

			int maxBarWidth = 150;

			UpdateFlatProgressBar(pnlCpuFill, currentCpu, maxBarWidth);
			UpdateFlatProgressBar(pnlRamFill, ramPercentage, maxBarWidth);
		}

		private void UpdateFlatProgressBar(Control fill, double percentage, int maxWidth)
		{
			if (fill == null) return;

			if (percentage > 100) percentage = 100;
			if (percentage < 0) percentage = 0;

			int targetWidth = (int)((percentage / 100.0) * maxWidth);

			fill.Width = targetWidth;
		}

		private void ServerInfo_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (_metricsTimer != null)
			{
				_metricsTimer.Stop();
				_metricsTimer.Dispose();
			}
		}
	}
}