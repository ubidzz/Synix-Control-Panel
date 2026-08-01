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
using static System.Runtime.InteropServices.JavaScript.JSType;

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

		private void CheckRunningStatus()
		{
			// Read the global object to see if another form updated it, but DO NOT modify it!
			if (_server == null || string.IsNullOrEmpty(_server.Status)) return;

			string[] spinFrames = { "|", "/", "--", "\\" };
			string status = _server.Status;
			bool isBusy = false;
			string nextSpinnerText = "";

			// Read what is currently on the screen so we can calculate the next animation frame locally
			string currentVisualText = lblStatusCardValue != null ? lblStatusCardValue.Text : "";

			if (status.StartsWith("Updating"))
			{
				string currentFrame = currentVisualText.Replace("Updating ", "");
				int currentIndex = Array.IndexOf(spinFrames, currentFrame);
				int nextIndex = (currentIndex >= 0 ? currentIndex + 1 : 0) % spinFrames.Length;
				nextSpinnerText = "Updating " + spinFrames[nextIndex];
				isBusy = true;
			}
			else if (status.StartsWith("Validating"))
			{
				string currentFrame = currentVisualText.Replace("Validating ", "");
				int currentIndex = Array.IndexOf(spinFrames, currentFrame);
				int nextIndex = (currentIndex >= 0 ? currentIndex + 1 : 0) % spinFrames.Length;
				nextSpinnerText = "Validating " + spinFrames[nextIndex];
				isBusy = true;
			}
			else if (status.StartsWith("Installing"))
			{
				string currentFrame = currentVisualText.Replace("Installing ", "");
				int currentIndex = Array.IndexOf(spinFrames, currentFrame);
				int nextIndex = (currentIndex >= 0 ? currentIndex + 1 : 0) % spinFrames.Length;
				nextSpinnerText = "Installing " + spinFrames[nextIndex];
				isBusy = true;
			}
			else if (status.StartsWith("Backing Up"))
			{
				string currentFrame = currentVisualText.Replace("Backing Up ", "");
				int currentIndex = Array.IndexOf(spinFrames, currentFrame);
				int nextIndex = (currentIndex >= 0 ? currentIndex + 1 : 0) % spinFrames.Length;
				nextSpinnerText = "Backing Up " + spinFrames[nextIndex];
				isBusy = true;
			}
			else if (status.StartsWith("Stopping"))
			{
				string currentFrame = currentVisualText.Replace("Stopping ", "");
				int currentIndex = Array.IndexOf(spinFrames, currentFrame);
				int nextIndex = (currentIndex >= 0 ? currentIndex + 1 : 0) % spinFrames.Length;
				nextSpinnerText = "Stopping " + spinFrames[nextIndex];
				isBusy = true;
			}
			else if (status.StartsWith("Starting"))
			{
				string currentFrame = currentVisualText.Replace("Starting ", "");
				int currentIndex = Array.IndexOf(spinFrames, currentFrame);
				int nextIndex = (currentIndex >= 0 ? currentIndex + 1 : 0) % spinFrames.Length;
				nextSpinnerText = "Starting " + spinFrames[nextIndex];
				isBusy = true;
			}

			// ONLY update the local UI label. Leave the global _server.Status completely untouched!
			if (isBusy && lblStatusCardValue != null)
			{
				lblStatusCardValue.Text = nextSpinnerText;
				lblStatusCardValue.ForeColor = Color.Orange;
			}
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
			_metricsTimer.Interval = 150;
			_metricsTimer.Tick += MetricsTimer_Tick;
			_metricsTimer.Start();
		}

		private void MetricsTimer_Tick(object sender, EventArgs e)
		{
			if (_server.Status == "Running")
			{
				lblStatusCardValue.Text = _server.Status;
				lblStatusCardValue.ForeColor = Color.Green;
			}
			else
			{
				CheckRunningStatus();
			}

			// 1. Hook the active process
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

			// 2. Extract metrics and set base ONLINE/OFFLINE status
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
			}
			else
			{
				// Default to OFFLINE
				if (lblStatusCardValue != null)
				{
					lblStatusCardValue.Text = "Stopped";
					lblStatusCardValue.ForeColor = Color.IndianRed;
				}
			}

			// 3. Update the UI Metric Labels
			if (lblCpuCardValue != null) lblCpuCardValue.Text = $"{currentCpu:0.0}%";
			if (lblRamCardValue != null) lblRamCardValue.Text = $"{currentRamGb:0.00} GB";

			// 4. Animate the flat progress bars
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