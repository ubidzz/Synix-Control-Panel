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
using Synix_Control_Panel.SynixApp.MonitoringHandler;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixApp.SteamCMDHandler;
using Synix_Control_Panel.SynixEngine;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static Synix_Control_Panel.SynixEngine.Core;

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
		private ToolTip? _resourceGraphToolTip;
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
			UIStyleHelper.InitializeToggles(this);
			UIStyleHelper.StyleLogBox(rtbLog);

			contextMenuStrip.Renderer = new Synix_Control_Panel.SynixApp.Design.SynixMenuRenderer();
			contextMenuStrip.ShowImageMargin = false;
			contextMenuStrip.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
			ApplyMenuRoundingAndSpacing(contextMenuStrip);

			dataGridView1.AutoGenerateColumns = false;
			dataGridView1.DataSource = serverList;
			if (!dataGridView1.Columns.Contains("IconCol"))
			{
				DataGridViewImageColumn iconCol = new DataGridViewImageColumn();
				iconCol.Name = "IconCol";
				iconCol.HeaderText = "";
				iconCol.DataPropertyName = "DisplayIcon";
				iconCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
				iconCol.Width = 35;
				iconCol.DefaultCellStyle.Padding = new Padding(6);

				dataGridView1.Columns.Insert(0, iconCol);
				dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
				dataGridView1.RowTemplate.Height = 35;
				foreach (DataGridViewRow row in dataGridView1.Rows)
				{
					row.Height = 35;
				}
			}

			GridStyler.DarkTheme(dataGridView1);
			GridStyler.ApplyRoundedCorners(dataGridView1, 10);
			typeof(DataGridView).InvokeMember("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty, null, dataGridView1, new object[] { true });
			GridStyler.ApplyTransparentTheme(dataGridView1);
			GridStyler.StyleCloseButton(btnClose);
			GridStyler.StyleMinimizeButton(btnMinimize);
			GridStyler.StyleIconButton(btnDiscord, Properties.Resources.discord_icon, Color.FromArgb(88, 101, 242));
			GridStyler.StyleIconButton(btnGithub, Properties.Resources.github_icon, Color.FromArgb(200, 200, 200));
			GridStyler.StyleIconButton(btnSettings, Properties.Resources.gear_icon, Color.FromArgb(200, 200, 200));

			_resourceGraphToolTip = new ToolTip(components)
			{
				InitialDelay = 300,
				ReshowDelay = 100,
				AutoPopDelay = 8000,
				ShowAlways = true
			};

			this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 15, 15));
			_ = Core.Instance;
			_ = VersionCheck();
			InitializeVersionCheckTimer();
		}

		[DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
		internal static extern void DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int pvAttribute, uint cbAttribute);

		private void ApplyMenuRoundingAndSpacing(ToolStripDropDown menu)
		{
			void ApplyDwm()
			{
				if (Environment.OSVersion.Version.Build >= 22000)
				{
					int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
					int DWMWCP_ROUND = 2;
					DwmSetWindowAttribute(menu.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref DWMWCP_ROUND, sizeof(int));
				}
			}

			if (menu.IsHandleCreated)
			{
				ApplyDwm();
			}
			else
			{
				menu.HandleCreated += (s, e) => ApplyDwm();
			}

			foreach (ToolStripItem item in menu.Items)
			{
				item.Padding = new Padding(0, 4, 0, 4);

				if (item is ToolStripDropDownItem dropDownItem && dropDownItem.HasDropDownItems)
				{
					ApplyMenuRoundingAndSpacing(dropDownItem.DropDown);
				}
			}
		}

		private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			e.ThrowException = false;
		}

		private void tmrResourceUpdates_Tick(object sender, EventArgs e)
		{
			CheckRunningStatus();

			double cpu = Core.Instance.TotalCpuUsage;
			double ram = Core.Instance.TotalRamUsageGb;

			cpuGauge.UpdateGauge((float)cpu, "CPU %");
			ramGauge.MaxValue = (float)systemTotalRamGb;
			ramGauge.UpdateGauge((float)ram, "RAM GB");

			chartTickCounter++;
		}

		[DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
		private static extern IntPtr CreateRoundRectRgn
		(
			int nLeftRect,
			int nTopRect,
			int nRightRect,
			int nBottomRect,
			int nWidthEllipse,
			int nHeightEllipse
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
			if (isDownloadActive || Core.Instance.isDownloadActive)
			{
				e.Cancel = true;
				MessageBox.Show("Cannot close Synix while a server is installing, updating or Backing Up!",
								"Operation in Progress", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private async Task LoadNetworkInfo()
		{
			if (!isPrivacyLoading)
			{
				string localIP = await Core.Instance.GetLocalIP();
				lblLocalIP1.Text = $"LAN IP: {localIP}";

				lblPublicIP.Text = "Public IP: Fetching...";
				string publicIP = await Core.Instance.GetPublicIP();
				lblPublicIP.Text = $"Public IP: {publicIP}";
			}
		}

		private async void lblPublicIP_Click(object sender, EventArgs e)
		{
			string publicIP = await Core.Instance.GetPublicIP();
			Clipboard.SetText(publicIP);
			if (!isPrivacyLoading)
			{
				AppendLog($"[🚨 SYNIX] Public IP {publicIP} was copied to clipboard.", Color.Cyan);
			}
			else
			{
				AppendLog($"[🚨 SYNIX] Public IP [HIDDEN] was copied to clipboard.", Color.Cyan);
			}
		}

		private async void lblLocalIP_Click(object sender, EventArgs e)
		{
			string localIP = await Core.Instance.GetLocalIP();
			Clipboard.SetText(localIP);
			if (!isPrivacyLoading)
			{
				AppendLog($"[🚨 SYNIX] Local IP {localIP} was copied to clipboard.", Color.Cyan);
			}
			else
			{
				AppendLog($"[🚨 SYNIX] Local IP [HIDDEN] was copied to clipboard.", Color.Cyan);
			}
		}

		public void AppendLog(string message, Color? textColor = null, bool isBold = false)
		{
			FileHandler.QueueLog(
				"Synix_Log",
				$"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");

			if (!IsHandleCreated || IsDisposed)
				return;

			if (rtbLog.InvokeRequired)
			{
				rtbLog.BeginInvoke(
					new Action(() => AppendLogToUi(message, textColor, isBold)));
				return;
			}

			AppendLogToUi(message, textColor, isBold);
		}

		private void AppendLogToUi(string message, Color? textColor, bool isBold)
		{
			if (rtbLog.IsDisposed)
				return;

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
		}

		private async void MainGUI_Shown(object sender, EventArgs e)
		{
			await UpdatePrivacyMode(Properties.Settings.Default.PrivacyMode);

			await Core.Instance.RebindProcesses();
			double physicalRam = 16.0;
			await Task.Run(() => physicalRam = ResourceMonitor.GetTotalSystemRamGB());

			double reserved = Math.Max(physicalRam * 0.10, 5.0);
			systemTotalRamGb = physicalRam - reserved;

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
			dataGridView1.Refresh();
		}

		private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			GridStyler.SetStatusColor(dataGridView1, e);
		}

		private void ResourceGraph_Click(object sender, EventArgs e)
		{
			ResourceMonitorGUI monitor = new ResourceMonitorGUI();
			monitor.Show();
		}

		private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
		{
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
			if (selectedServer == null) return;

			AppendLog($"[📡 NETWORK] Running comprehensive WAN connectivity tests for {selectedServer.ServerName}...", Color.White);

			try
			{
				string publicIp = await Core.Instance.GetPublicIP();
				string ipText = isPrivacyLoading ? "[HIDDEN]" : publicIp;

				bool gameTcp = await Core.Instance.TestTcpConnectivity(publicIp, selectedServer.Port);
				bool queryTcp = await Core.Instance.TestTcpConnectivity(publicIp, selectedServer.QueryPort);
				bool gameUdp = await Core.Instance.TestServerConnectivity(publicIp, selectedServer.Port);
				bool queryUdp = await Core.Instance.TestServerConnectivity(publicIp, selectedServer.QueryPort);

				if (gameTcp || queryTcp || gameUdp || queryUdp)
				{
					AppendLog($"[🌐 ONLINE] {selectedServer.ServerName} is reachable locally at {ipText}! (GamePort TCP:{gameTcp} UDP:{gameUdp} | QueryPort TCP:{queryTcp} UDP:{queryUdp})", Color.Green);
				}
				else
				{
					AppendLog($"[🛡️ BLOCK] All connectivity tests failed for {selectedServer.ServerName} at {ipText} (Tested Game Port {selectedServer.Port} & Query Port {selectedServer.QueryPort} via TCP/UDP). Check Router/Firewall settings.", Color.Red);
				}
			}
			catch (Exception ex)
			{
				AppendLog($"[🚨 ERROR] Could not complete Public connectivity test: {ex.Message}", Color.Yellow);
			}
		}

		private async void btnLocalConnection_Click(object sender, EventArgs e)
		{
			var selectedServer = GetSelectedServer();
			if (selectedServer == null) return;

			AppendLog($"[📡 NETWORK] Running comprehensive LAN connectivity tests for {selectedServer.ServerName}...", Color.White);

			try
			{
				string localIp = await Core.Instance.GetLocalIP();
				string ipText = isPrivacyLoading ? "[HIDDEN]" : localIp;

				bool gameTcp = await Core.Instance.TestTcpConnectivity(localIp, selectedServer.Port);
				bool queryTcp = await Core.Instance.TestTcpConnectivity(localIp, selectedServer.QueryPort);
				bool gameUdp = await Core.Instance.TestServerConnectivity(localIp, selectedServer.Port);
				bool queryUdp = await Core.Instance.TestServerConnectivity(localIp, selectedServer.QueryPort);

				if (gameTcp || queryTcp || gameUdp || queryUdp)
				{
					AppendLog($"[🌐 ONLINE] {selectedServer.ServerName} is reachable locally at {ipText}! (GamePort TCP:{gameTcp} | QueryPort UDP:{queryUdp})", Color.Green);
				}
				else
				{
					AppendLog($"[🛡️ BLOCK] All local connectivity tests failed for {selectedServer.ServerName} at {ipText} (Tested Game Port {selectedServer.Port} & Query Port {selectedServer.QueryPort} via TCP/UDP). Ensure the server is running.", Color.Red);
				}
			}
			catch (Exception ex)
			{
				AppendLog($"[🚨 ERROR] Could not complete LAN connectivity test: {ex.Message}", Color.Yellow);
			}
		}

		private void btnServerActionsMenu_Click(object sender, EventArgs e)
		{
			if (dataGridView1.CurrentRow != null && dataGridView1.CurrentRow.DataBoundItem is GameServer selectedServer)
			{
				bool isMinecraft = selectedServer.Game.StartsWith("Minecraft Java", StringComparison.OrdinalIgnoreCase);

				updateServerToolStripMenuItem.Enabled = !isMinecraft;
				fileValidationToolStripMenuItem.Enabled = !isMinecraft;
				btnExportBatch.Enabled = !isMinecraft;
			}

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
			versionTimer.Interval = 20 * 60 * 1000;
			versionTimer.Tick += async (sender, e) =>
			{
				await VersionCheck();
			};

			versionTimer.Start();
		}

		private async Task VersionCheck()
		{
			string currentVersion = "Unknown";

			currentVersion = Application.ProductVersion.TrimEnd(".0".ToCharArray());
			string versionUrl = "https://raw.githubusercontent.com/ubidzz/Synix-Control-Panel/refs/heads/master/SynixApp/SynixEngine/version.txt";
			btnDownloadUpdate.Visible = false;
			UIStyleHelper.StyleWarningLabel(lblUpdateStatus, "MiddleLeft");
			lblUpdateStatus.Text = "Checking for updates...";

			try
			{
				using (HttpClient client = new())
				{
					client.Timeout = TimeSpan.FromSeconds(5);
					string latestVersion = (await client.GetStringAsync(versionUrl)).Trim();

					if (Version.TryParse(currentVersion, out Version vLocal) && Version.TryParse(latestVersion, out Version vRemote))
					{
						lblUpdateStatus.Text = "★ You are running the latest version " + currentVersion;
						lblUpdateStatus.ForeColor = Color.Black;
						lblUpdateStatus.TextAlign = ContentAlignment.MiddleRight;
						lblUpdateStatus.BackColor = Color.Green;
					}
					else
					{
						lblUpdateStatus.Text = "🚨 A newer Synix " + latestVersion + " version is available! Running Version: " + currentVersion + "";
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
		public async Task UpdatePrivacyMode(bool isEnabled)
		{
			isPrivacyLoading = isEnabled;

			if (isEnabled)
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

		private void btnSettings_Click(object sender, EventArgs e)
		{
			using (Synix_Control_Panel.SynixEngine.AppSettings SynixSettings = new Synix_Control_Panel.SynixEngine.AppSettings())
			{
				SynixSettings.ShowDialog();
			}
		}
	}
}
