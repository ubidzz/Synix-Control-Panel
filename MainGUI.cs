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
using Synix_Control_Panel.Design;
using Synix_Control_Panel.ServerHandler;
using Synix_Control_Panel.SteamCMDHandler;
using Synix_Control_Panel.SynixEngine;
using Synix_Control_Panel.FileFolderHandler;
using Synix_Control_Panel.UI;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;
using static Synix_Control_Panel.SynixEngine.Core;
using System.Runtime.InteropServices;

namespace Synix_Control_Panel
{
	public partial class MainGUI : Form
	{
		public static BindingList<GameServer> serverList = [];
		private static System.Net.NetworkInformation.NetworkInterface[]? _activeInterfaces = null;
		public bool isDownloadActive = false;
		private static bool isInitializing = false;
		public static MainGUI? Instance { get; private set; }
		public double systemTotalRamGb = 128;
		private int chartTickCounter = 0;
		private const int maxGraphPoints = 60;
		private static Font boldFont = new Font("Segoe UI", 9, FontStyle.Bold);
		private static Font regularFont = new Font("Segoe UI", 9, FontStyle.Regular);
		private bool isPrivacyLoading = false;
		private System.Windows.Forms.Timer? versionTimer;
		public static Dictionary<string, Image> ServerIconsCache = new Dictionary<string, Image>();

		public const int WM_NCLBUTTONDOWN = 0xA1;
		public const int HT_CAPTION = 0x2;

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
		[DllImport("user32.dll")]
		public static extern bool ReleaseCapture();

		public MainGUI()
		{
			InitializeComponent();
			Instance = this;
			FileHandler.LoadServers();
			_ = Core.Instance;
			
			GridStyler.DarkTheme(dataGridView1);
			GridStyler.ApplyRoundedCorners(dataGridView1, 10);
			UIStyleHelper.InitializeToggles(this);

			dataGridView1.DataSource = serverList;
			dataGridView1.DataError += dataGridView1_DataError;
			if (!dataGridView1.Columns.Contains("IconCol"))
			{
				DataGridViewImageColumn iconCol = new DataGridViewImageColumn();
				iconCol.Name = "IconCol";
				iconCol.HeaderText = "";
				iconCol.DataPropertyName = "DisplayIcon";
				iconCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
				iconCol.Width = 35;

				iconCol.DefaultCellStyle.Padding = new Padding(4);

				dataGridView1.Columns.Insert(0, iconCol);

				dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
				dataGridView1.RowTemplate.Height = 35;
				foreach (DataGridViewRow row in dataGridView1.Rows)
				{
					row.Height = 35;
				}
			}

			typeof(DataGridView).InvokeMember("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty, null, dataGridView1, new object[] { true });
			GridStyler.ApplyTransparentTheme(dataGridView1);
			GridStyler.StyleCloseButton(btnClose);
			GridStyler.StyleMinimizeButton(btnMinimize);
			GridStyler.StyleIconButton(btnDiscord, Properties.Resources.discord_icon, Color.FromArgb(88, 101, 242));
			GridStyler.StyleIconButton(btnGithub, Properties.Resources.github_icon, Color.FromArgb(200, 200, 200));

			Instance = this;
			chkPrivacyMode.Text = "Privacy Mode";
			chkPrivacyMode.Checked = Properties.Settings.Default.PrivacyMode;
			this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 15, 15));
			isPrivacyLoading = chkPrivacyMode.Checked;
			_ = LoadNetworkInfo();
			InitializeVersionCheckTimer();
			_ = VersionCheck();
		}

		private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			// If the grid throws a fit because the image is temporarily missing during an edit,
			// this tells it to ignore the error and not draw the ugly Red X.
			e.ThrowException = false;
		}

		private void tmrResourceUpdates_Tick(object sender, EventArgs e)
		{
			CheckRunningStatus();

			// 1. Grab telemetry
			double cpu = Core.Instance.TotalCpuUsage;
			double ram = Core.Instance.TotalRamUsageGb;

			lblTotalCpu.Text = $"CPU: {cpu:N1}%";
			lblTotalRam.Text = $"RAM: {ram:N2} GB / {systemTotalRamGb:N1} GB (Usable)";

			if (chartHeartbeat.Series.FindByName("TotalCPU") == null)
				Design.GridStyler.HeartbeatChart(chartHeartbeat, systemTotalRamGb);

			// 2. Manually append the new data directly to the existing chart collection
			chartHeartbeat.Series["TotalCPU"].Points.AddXY(chartTickCounter, cpu);
			chartHeartbeat.Series["TotalRAM"].Points.AddXY(chartTickCounter, ram);

			// 3. Remove the oldest points to keep the collection size stable and prevent managed memory growth
			if (chartHeartbeat.Series["TotalCPU"].Points.Count > 30)
			{
				chartHeartbeat.Series["TotalCPU"].Points.RemoveAt(0);
				chartHeartbeat.Series["TotalRAM"].Points.RemoveAt(0);
			}

			// 4. Scroll the view dynamically based on the actual points
			var chartArea = chartHeartbeat.ChartAreas[0];
			chartArea.AxisX.Minimum = chartHeartbeat.Series["TotalCPU"].Points.First().XValue;
			chartArea.AxisX.Maximum = chartHeartbeat.Series["TotalCPU"].Points.Last().XValue;

			// 5. Restart Check
			bool needsTimeCheck = serverList.Any(s => s.IsScheduledRestartEnabled);
			if (needsTimeCheck)
			{
				string currentExactTime = DateTime.Now.ToString("HH:mm:ss");
				foreach (var server in serverList)
				{
					if (server.IsScheduledRestartEnabled && currentExactTime == (server.RestartTime + ":00"))
					{
						_ = Core.Instance.ExecuteStartSequence(server, "MAINTENANCE");
					}
				}
			}
			chartTickCounter++;
		}

		[DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
		private static extern IntPtr CreateRoundRectRgn
		(
			int nLeftRect,     // x-coordinate of upper-left corner
			int nTopRect,      // y-coordinate of upper-left corner
			int nRightRect,    // x-coordinate of lower-right corner
			int nBottomRect,   // y-coordinate of lower-right corner
			int nWidthEllipse, // width of the rounded corner
			int nHeightEllipse // height of the rounded corner
		);

		private void Form_Drag_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				ReleaseCapture();
				SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
			}
		}

		private void CheckRunningStatus()
		{
			string[] spinFrames = { "|", "/", "--", "\\" };

			foreach (var server in serverList)
			{
				string status = server.Status ?? "";

				if (status.StartsWith("Updating"))
				{
					string currentFrame = status.Replace("Updating ", "");
					int currentIndex = Array.IndexOf(spinFrames, currentFrame);
					int nextIndex = (currentIndex + 1) % spinFrames.Length;
					server.Status = "Updating " + spinFrames[nextIndex];
				}
				else if (status.StartsWith("Validating"))
				{
					string currentFrame = status.Replace("Validating ", "");
					int currentIndex = Array.IndexOf(spinFrames, currentFrame);
					int nextIndex = (currentIndex + 1) % spinFrames.Length;
					server.Status = "Validating " + spinFrames[nextIndex];
				}
				else if (status.StartsWith("Installing"))
				{
					string currentFrame = status.Replace("Installing ", "");
					int currentIndex = Array.IndexOf(spinFrames, currentFrame);
					int nextIndex = (currentIndex + 1) % spinFrames.Length;
					server.Status = "Installing " + spinFrames[nextIndex];
				}
				else if (status.StartsWith("Backing Up"))
				{
					string currentFrame = status.Replace("Backing Up ", "");
					int currentIndex = Array.IndexOf(spinFrames, currentFrame);
					int nextIndex = (currentIndex + 1) % spinFrames.Length;
					server.Status = "Backing Up " + spinFrames[nextIndex];
				}
				else if (status.StartsWith("Stopping"))
				{
					string currentFrame = status.Replace("Stopping ", "");
					int currentIndex = Array.IndexOf(spinFrames, currentFrame);
					int nextIndex = (currentIndex + 1) % spinFrames.Length;
					server.Status = "Stopping " + spinFrames[nextIndex];
				}
			}
			UpdateGrid();
		}

		private void StreamerModeCheck()
		{
			if (isPrivacyLoading)
			{
				AppendLog("[🛡️ BLOCK] Streamer mode is active", Color.Red);
				return;
			}
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			// Use the variable that Heartbeat_Tick has been updating
			if (isDownloadActive || Core.Instance.isDownloadActive)
			{
				e.Cancel = true;
				MessageBox.Show("Cannot close Synix while a server is installing, updating or Backing Up!",
								"Operation in Progress", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private async Task LoadNetworkInfo()
		{
			// 1. Get the LAN IP instantly
			if (!isPrivacyLoading)
			{
				string localIP = await Core.Instance.GetLocalIP();
				lblLocalIP1.Text = $"LAN IP: {localIP}";

				// 2. Get the Public IP in the background
				lblPublicIP.Text = "Public IP: Fetching...";
				string publicIP = await Core.Instance.GetPublicIP();
				lblPublicIP.Text = $"Public IP: {publicIP}";
			}
		}

		private void lblPublicIP_Click(object sender, EventArgs e)
		{
			// Strip the prefix and copy just the IP
			string ip = lblPublicIP.Text.Replace("Public IP: ", "");
			if (ip != StatusManager.GetStatus(ServerState.Stopped) && ip != "Fetching...")
			{
				Clipboard.SetText(ip);
				if (!isPrivacyLoading)
				{
					AppendLog($"[🚨 SYNIX] Public IP {ip} was copied to clipboard.", Color.Cyan);
				}
				else
				{
					AppendLog($"[🚨 SYNIX] Public IP [HIDDEN] was copied to clipboard.", Color.Cyan);
				}
			}
		}

		private void lblLocalIP_Click(object sender, EventArgs e)
		{
			string LANip = lblLocalIP1.Text.Replace("LAN IP: ", "");
			Clipboard.SetText(LANip);
			if (!isPrivacyLoading)
			{
				AppendLog($"[🚨 SYNIX] Local IP {LANip} was copied to clipboard.", Color.Cyan);
			}
			else
			{
				AppendLog($"[🚨 SYNIX] Local IP [HIDDEN] was copied to clipboard.", Color.Cyan);
			}
		}

		public void AppendLog(string message, Color? textColor = null, bool isBold = false)
		{
			try
			{
				FileHandler.WriteLog("Synix_Log", $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
			}
			catch { /* Silent fail */ }

			if (!this.IsHandleCreated || this.IsDisposed) return;

			if (rtbLog.InvokeRequired)
			{
				rtbLog.BeginInvoke(new Action(() => AppendLog(message, textColor, isBold)));
				return;
			}

			string timeStamp = $"[{DateTime.Now:HH:mm:ss}] ";

			rtbLog.SelectionStart = rtbLog.TextLength;
			rtbLog.SelectionLength = 0;

			rtbLog.SelectionColor = textColor ?? rtbLog.ForeColor;

			if (rtbLog.Lines.Length > 500)
			{
				rtbLog.ReadOnly = false;
				rtbLog.Select(0, rtbLog.GetFirstCharIndexFromLine(100));
				rtbLog.SelectedText = "";
				rtbLog.ClearUndo();
				rtbLog.ReadOnly = true;
			}

			rtbLog.SelectionFont = isBold ? boldFont : regularFont;
			rtbLog.AppendText(timeStamp + message + Environment.NewLine);
			rtbLog.SelectionStart = rtbLog.Text.Length;
			rtbLog.ScrollToCaret();
			rtbLog.Update();
		}

		private async void MainGUI_Shown(object sender, EventArgs e)
		{
			Core.Instance.RebindProcesses();
			double physicalRam = 16.0;
			await Task.Run(() => physicalRam = MonitoringHandler.ResourceMonitor.GetTotalSystemRamGB());

			double reserved = Math.Max(physicalRam * 0.10, 5.0);
			systemTotalRamGb = physicalRam - reserved;

			Design.GridStyler.HeartbeatChart(chartHeartbeat, systemTotalRamGb);
			Design.GridStyler.DashboardLabels(lblTotalCpu, lblTotalRam);

			chartHeartbeat.Series["TotalCPU"].Points.AddXY(chartTickCounter, 0);
			chartHeartbeat.Series["TotalRAM"].Points.AddXY(chartTickCounter, 0);
			chartHeartbeat.Update();
			chartTickCounter++;
			tmrResourceUpdates.Start();

			await Task.Run(() => SteamCMD.EnsureSteamCMD((msg, color) => AppendLog(msg, color)));
		}

		public void UpdateGrid()
		{
			if (this.InvokeRequired)
			{
				this.BeginInvoke(new Action(UpdateGrid));
				return;
			}

			// All the "Nuclear Refresh" and scroll logic is hidden in the helper
			GridHelper.RefreshWithPersistence(dataGridView1, serverList);
		}

		private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			// Let the GridStyler handle the colors
			GridStyler.SetStatusColor(dataGridView1, e);
		}

		private void ResourceGraph_Click(object sender, EventArgs e)
		{
			// Pass the current list of servers to the new monitor window
			ResourceMonitorGUI monitor = new ResourceMonitorGUI();
			monitor.Show(); // .Show() lets them keep the panel open while using the main app
		}
		private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
		{
			// Just draw the rows using the solid colors from GridStyler
			GridStyler.PaintTransparentRows(dataGridView1, e);
		}

		private GameServer? GetSelectedServer()
		{
			if (dataGridView1.CurrentRow == null)
			{
				AppendLog("[🚨 ERROR] No row is currently selected!", Color.Red);
				MessageBox.Show("Please select a server in the list first.", "No Server Selected");
				return null;
			}

			if (!(dataGridView1.CurrentRow.DataBoundItem is GameServer selectedServer))
			{
				AppendLog("[🚨 ERROR] Invalid GameServer object!", Color.Red);
				return null;
			}

			if (dataGridView1.CurrentRow != null && dataGridView1.CurrentRow.DataBoundItem is GameServer server)
			{
				return server;
			}
			return null;
		}

		private async void btnAddServer_Click(object sender, EventArgs e)
		{
			// UI-specific check
			if (isInitializing) return;
			await Core.Instance.AddServerAndReport();
		}

		private void btnEdit_Click(object sender, EventArgs e)
		{
			if (isInitializing) return;
			var selectedServer = GetSelectedServer();
			if (!Core.Instance.PassSpamLock(selectedServer, out string lockMsg, "EditConfig"))
			{
				AppendLog(lockMsg, Color.Orange);
				return;
			}
			Core.Instance.EditServerAndReport(selectedServer);
		}

		private async void btnUpdate_Click(object sender, EventArgs e)
		{
			if (isInitializing) return;
			var selectedServer = GetSelectedServer();

			if (!Core.Instance.PassSpamLock(selectedServer, out string lockMsg, "Update"))
			{
				AppendLog(lockMsg, Color.Orange);
				return;
			}

			await Core.Instance.UpdateServerAndReport(selectedServer, "UPDATE");
		}

		private async void btnFileValidation_Click(object sender, EventArgs e)
		{
			if (isInitializing) return;
			var selectedServer = GetSelectedServer();

			if (!Core.Instance.PassSpamLock(selectedServer, out string lockMsg, "Validate"))
			{
				AppendLog(lockMsg, Color.Orange);
				return;
			}

			await Core.Instance.UpdateServerAndReport(selectedServer, "VALIDATE");
		}

		private void btnDelete_Click(object sender, EventArgs e)
		{
			if (isInitializing) return;
			var selectedServer = GetSelectedServer();
			if (!Core.Instance.PassSpamLock(selectedServer, out string lockMsg, "Delete"))
			{
				AppendLog(lockMsg, Color.Orange);
				return;
			}
			Core.Instance.DeleteServerAndReport(selectedServer);
			dataGridView1.CurrentCell = null;
			dataGridView1.DataSource = null;
			dataGridView1.DataSource = serverList;
		}

		private async void btnBackup_Click(object sender, EventArgs e)
		{
			var selectedServer = GetSelectedServer();

			if (!Core.Instance.PassSpamLock(selectedServer, out string lockMsg, "Backup"))
			{
				AppendLog(lockMsg, Color.Orange);
				return;
			}

			await Task.Run(() =>
			{
				Core.Instance.ExecuteBackup(selectedServer, StartContext.Manual);
			});
		}

		private async void btnStart_Click(object sender, EventArgs e)
		{
			var selectedServer = GetSelectedServer();

			if (!Core.Instance.PassSpamLock(selectedServer, out string lockMsg, "Start"))
			{
				AppendLog(lockMsg, Color.Orange);
				return;
			}

			await Core.Instance.ExecuteStartSequence(selectedServer);
		}

		private async void btnStop_Click(object sender, EventArgs e)
		{
			var selectedServer = GetSelectedServer();

			if (!Core.Instance.PassSpamLock(selectedServer, out string lockMsg, "Stop"))
			{
				AppendLog(lockMsg, Color.Orange);
				return;
			}

			await Core.Instance.StopServerAndReport(selectedServer);
		}

		private void btnOpenConfig_Click(object sender, EventArgs e)
		{
			StreamerModeCheck();
			if (isPrivacyLoading) return;
			var selectedServer = GetSelectedServer();
			if (!Core.Instance.PassSpamLock(selectedServer, out string lockMsg, "Config"))
			{
				AppendLog(lockMsg, Color.Orange);
				return;
			}
			Core.Instance.OpenConfigEditor(selectedServer);
		}

		private void btnOpenFolder_Click(object sender, EventArgs e)
		{
			var selectedServer = GetSelectedServer();
			Core.Instance.OpenServerFolder(selectedServer);
		}

		private void btnOpenBackup_Click(object sender, EventArgs e)
		{
			var selectedServer = GetSelectedServer();
			Core.Instance.OpenBackFolder(selectedServer);
		}

		private async void btnPublicConnection_Click(object sender, EventArgs e)
		{
			var selectedServer = GetSelectedServer();

			AppendLog($"[📡 NETWORK] Testing WAN Connectivity for {selectedServer.ServerName}...", Color.White);

			try
			{
				string publicIp = await Core.Instance.GetPublicIP();
				bool isResponding = await Core.Instance.TestServerConnectivity(publicIp, selectedServer.QueryPort);
				string ipText = "[HIDDEN]";

				if (!isPrivacyLoading)
				{
					ipText = publicIp;
				}

				if (isResponding)
				{
					AppendLog($"[🌐 ONLINE] {selectedServer.ServerName} is visible at {ipText}:{selectedServer.QueryPort}!", Color.Green);
				}
				else
				{
					AppendLog($"[🛡️ BLOCK] {selectedServer.ServerName} is running but HIDDEN. Check Router/Firewall for UDP {selectedServer.QueryPort} or try setting a different query port.", Color.Red);
				}
			}
			catch (Exception ex)
			{
				AppendLog($"[🚨 ERROR] Could not retrieve Public IP: {ex.Message}", Color.Yellow);
			}
		}

		private async void btnLocalConnection_Click(object sender, EventArgs e)
		{
			var selectedServer = GetSelectedServer();

			AppendLog($"[📡 NETWORK] Testing LAN Connectivity for {selectedServer.ServerName}...", Color.White);

			try
			{
				string localIp = await Core.Instance.GetLocalIP();
				bool isResponding = await Core.Instance.TestServerConnectivity(localIp, selectedServer.QueryPort);
				string ipText = "[HIDDEN]";

				if (!isPrivacyLoading)
				{
					ipText = localIp;
				}

				if (isResponding)
				{
					AppendLog($"[🌐 ONLINE] {selectedServer.ServerName} is visible at {ipText}:{selectedServer.QueryPort}!", Color.Green);
				}
				else
				{
					AppendLog($"[🛡️ BLOCK] {selectedServer.ServerName} is running but HIDDEN. Check Router/Firewall for UDP {selectedServer.QueryPort} or try setting a different query port.", Color.Red);
				}
			}
			catch (Exception ex)
			{
				AppendLog($"[🚨 ERROR] Could not retrieve Public IP: {ex.Message}", Color.Yellow);
			}
		}

		private void btnServerActionsMenu_Click(object sender, EventArgs e)
		{
			contextMenuStrip.Show(btnServerActions, new System.Drawing.Point(0, 0), ToolStripDropDownDirection.AboveRight);
		}

		private async void btnRestart_Click(object sender, EventArgs e)
		{
			var selectedServer = GetSelectedServer();
			if (!Core.Instance.PassSpamLock(selectedServer, out string lockMsg, "Restart"))
			{
				AppendLog(lockMsg, Color.Orange);
				return;
			}

			await Core.Instance.ExecuteStartSequence(selectedServer, "RESTART");
		}

		private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0)
			{
				var selectedServer = GetSelectedServer();
				Help.ServerInfo infoForm = new Help.ServerInfo(selectedServer);
				infoForm.Show();
			}
		}

		private void btnHelp_Click(object sender, EventArgs e)
		{
			using (Synix_Control_Panel.SynixEngine.HelpGUI helpWindow = new Synix_Control_Panel.SynixEngine.HelpGUI())
			{
				helpWindow.ShowDialog();
			}
		}

		private void InitializeVersionCheckTimer()
		{
			versionTimer = new System.Windows.Forms.Timer();

			// 20 minutes * 60 seconds * 1000 milliseconds
			versionTimer.Interval = 20 * 60 * 1000;

			versionTimer.Tick += async (sender, e) =>
			{
				// This fires every 20 minutes in the background
				await VersionCheck();
			};

			versionTimer.Start();
		}

		private async Task VersionCheck()
		{
			string currentVersion = "Unknown";
			var assembly = System.Reflection.Assembly.GetExecutingAssembly();

			string[] resourceNames = assembly.GetManifestResourceNames();
			string actualResourcePath = null;

			foreach (string name in resourceNames)
			{
				if (name.EndsWith("version.txt"))
				{
					actualResourcePath = name;
					break;
				}
			}

			if (actualResourcePath != null)
			{
				using (Stream stream = assembly.GetManifestResourceStream(actualResourcePath))
				{
					if (stream != null)
					{
						using (StreamReader reader = new StreamReader(stream))
						{
							currentVersion = reader.ReadToEnd().Trim();
						}
					}
				}
			}

			string versionUrl = "https://raw.githubusercontent.com/ubidzz/Synix-Control-Panel/refs/heads/master/SynixEngine/version.txt";
			btnDownloadUpdate.Visible = false;
			UIStyleHelper.StyleWarningLabel(lblUpdateStatus, "MiddleLeft");
			lblUpdateStatus.Text = "Checking for updates...";

			try
			{
				using (HttpClient client = new())
				{
					client.Timeout = TimeSpan.FromSeconds(5);
					string latestVersion = (await client.GetStringAsync(versionUrl)).Trim();

					if (latestVersion == currentVersion)
					{
						lblUpdateStatus.Text = "You are running the latest version " + currentVersion;
						lblUpdateStatus.ForeColor = Color.Black;
						lblUpdateStatus.TextAlign = ContentAlignment.MiddleRight;
						lblUpdateStatus.BackColor = Color.Green;
					}
					else
					{
						lblUpdateStatus.Text = "A newer Synix " + latestVersion + " version is available! Running Version: " + currentVersion + "";
						lblUpdateStatus.ForeColor = Color.White;
						lblUpdateStatus.TextAlign = ContentAlignment.MiddleRight;
						lblUpdateStatus.BackColor = Color.Red;

						btnDownloadUpdate.Visible = true;
						btnDownloadUpdate.Text = "Download from GitHub";
					}
				}
			}
			catch
			{
				lblUpdateStatus.Text = "[🚨 ERROR] Could not check for updates.";
				lblUpdateStatus.ForeColor = Color.Black;
				lblUpdateStatus.TextAlign = ContentAlignment.MiddleRight;
				lblUpdateStatus.BackColor = Color.Red;
			}
		}

		private void btnDownloadUpdate_Click(object sender, EventArgs e)
		{
			try
			{
				string url = "https://github.com/ubidzz/Synix-Control-Panel/releases";

				ProcessStartInfo psi = new ProcessStartInfo
				{
					FileName = url,
					UseShellExecute = true
				};
				Process.Start(psi);
			}
			catch (Exception ex)
			{
				AppendLog($"[🚨 ERROR] Could not open browser: {ex.Message}", Color.Red);
			}
		}
		private async void chkPrivacyMode_CheckedChanged(object sender, EventArgs e)
		{
			isPrivacyLoading = chkPrivacyMode.Checked;

			Properties.Settings.Default.PrivacyMode = chkPrivacyMode.Checked;
			Properties.Settings.Default.Save();

			if (chkPrivacyMode.Checked)
			{
				lblPublicIP.Text = "Public IP: [HIDDEN]";
				lblLocalIP1.Text = "LAN IP: [HIDDEN]";
			}
			await LoadNetworkInfo();
		}

		private void btnExportBatch_Click(object sender, EventArgs e)
		{
			if (isInitializing) return;

			var selectedServer = GetSelectedServer();
			if (selectedServer == null) return;

			if (!Core.Instance.PassSpamLock(selectedServer, out string lockMsg, "Export"))
			{
				AppendLog(lockMsg, Color.Orange);
				return;
			}

			bool success = Core.Instance.ExportServerToBatch(selectedServer);

			if (success)
			{
				MessageBox.Show($"Batch file generated successfully!\n\nSaved directly to:\n{selectedServer.InstallPath}",
								"Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void btnMinimize_Click(object sender, EventArgs e)
		{
			this.WindowState = FormWindowState.Minimized;
		}

		private void btnDiscord_Click(object sender, EventArgs e)
		{
			OpenUrl("https://discord.gg/WduKEU3j8s");
		}

		private void btnGithub_Click(object sender, EventArgs e)
		{
			OpenUrl("https://github.com/ubidzz/Synix-Control-Panel");
		}

		private void OpenUrl(string url)
		{
			try
			{
				System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
				{
					FileName = url,
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Unable to open the link automatically.\n\nError: {ex.Message}", "Link Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}
	}
}
