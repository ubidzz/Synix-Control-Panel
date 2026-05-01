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
using Synix_Control_Panel.UI;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel
{
	public partial class MainGUI : Form
	{
		public static BindingList<GameServer> serverList = [];
		private static System.Net.NetworkInformation.NetworkInterface[]? _activeInterfaces = null;
		private bool isDownloadActive = false;
		private static bool isInitializing = false;
		public static MainGUI? Instance { get; private set; }
		public double systemTotalRamGb = 128;
		private int chartTickCounter = 0;
		private const int maxGraphPoints = 60;
		private static Font boldFont = new Font("Segoe UI", 9, FontStyle.Bold);
		private static Font regularFont = new Font("Segoe UI", 9, FontStyle.Regular);

		public MainGUI()
		{
			InitializeComponent();
			Instance = this;
			FileHandler.LoadServers();
			_ = Core.Instance;
			GridStyler.DarkTheme(dataGridView1);
			dataGridView1.DataSource = serverList;
			typeof(DataGridView).InvokeMember("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty, null, dataGridView1, new object[] { true });
			GridStyler.ApplyTransparentTheme(dataGridView1);
			Instance = this;
			_ = LoadNetworkInfo();
			_ = VersionCheck();
		}

		private void tmrResourceUpdates_Tick(object sender, EventArgs e)
		{
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
						_ = Core.Instance.ExecuteStartSequence(server);
					}
				}
			}

			chartTickCounter++;
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			// Use the variable that Heartbeat_Tick has been updating
			if (isDownloadActive)
			{
				e.Cancel = true;
				MessageBox.Show("Cannot close Synix while a server is installing, updating or Backing Up!",
								"Operation in Progress", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private async Task LoadNetworkInfo()
		{
			// 1. Get the LAN IP instantly
			string localIP = await Core.Instance.GetLocalIP();
			lblLocalIP1.Text = $"LAN IP: {localIP}";

			// 2. Get the Public IP in the background
			lblPublicIP.Text = "Public IP: Fetching...";
			string publicIP = await Core.Instance.GetPublicIP();
			lblPublicIP.Text = $"Public IP: {publicIP}";
		}

		private void lblPublicIP_Click(object sender, EventArgs e)
		{
			// Strip the prefix and copy just the IP
			string ip = lblPublicIP.Text.Replace("Public IP: ", "");
			if (ip != StatusManager.GetStatus(ServerState.Stopped) && ip != "Fetching...")
			{
				Clipboard.SetText(ip);
				Core.Instance.Log($"[SYSTEM] Public IP {ip} copied to clipboard.", Color.Cyan);
			}
		}

		private void lblLocalIP_Click(object sender, EventArgs e)
		{
			string LANip = lblLocalIP1.Text.Replace("LAN IP: ", "");
			Clipboard.SetText(LANip);
			Core.Instance.Log($"[SYSTEM] Local IP {LANip} copied to clipboard.", Color.Cyan);
		}

		public void AppendLog(string message, Color? textColor = null, bool isBold = false)
		{
			try
			{
				string logDirectory = @"C:\Synix\SynixData\logs";
				Directory.CreateDirectory(logDirectory);
				string logFilePath = Path.Combine(logDirectory, "synix_engine.log");
				string timeStampedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
				File.AppendAllText(logFilePath, timeStampedMessage);
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
			double physicalRam = 16.0;

			await Task.Run(() =>
			{
				physicalRam = MonitoringHandler.ResourceMonitor.GetTotalSystemRamGB();
			});

			double reserved = Math.Max(physicalRam * 0.10, 5.0);
			systemTotalRamGb = physicalRam - reserved;

			Design.GridStyler.HeartbeatChart(chartHeartbeat, systemTotalRamGb);
			Design.GridStyler.DashboardLabels(lblTotalCpu, lblTotalRam);

			//  THE GRAPH FIX: Shove a dummy point in and FORCE the heavy graphics engine to draw instantly
			chartHeartbeat.Series["TotalCPU"].Points.AddXY(chartTickCounter, 0);
			chartHeartbeat.Series["TotalRAM"].Points.AddXY(chartTickCounter, 0);
			chartHeartbeat.Update();

			chartTickCounter++;
			tmrResourceUpdates.Start();

			isDownloadActive = true;
			await Task.Delay(100);

			AppendLog($"[WARNING] Synix close window button is now Disabled!", Color.Orange, true);
			AppendLog("Checking SteamCMD dependencies...");

			await Task.Run(() => SteamCMD.EnsureSteamCMD(msg => AppendLog(msg)));

			isDownloadActive = false;
			AppendLog("Initialization complete.");
			AppendLog($"[WARNING] Synix close window button is now Enabled!", Color.Orange, true);
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

		private async void btnAddServer_Click(object sender, EventArgs e)
		{
			// UI-specific check
			if (isInitializing) return;
			isDownloadActive = true;
			AppendLog($"[WARNING] Synix close window button is now Disabled!", Color.Orange, true);
			await Core.Instance.AddServerAndReport();
			isDownloadActive = false;
			AppendLog($"[WARNING] Synix close window button is now Enabled!", Color.Orange, true);
		}

		private void btnEdit_Click(object sender, EventArgs e)
		{
			// UI-specific safety check
			if (isInitializing) return;

			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				Core.Instance.EditServerAndReport(selectedServer);
			}
			else
			{
				MessageBox.Show("Please select a server in the list first.", "No Server Selected");
			}
		}

		private async void btnUpdate_Click(object sender, EventArgs e)
		{
			if (isInitializing) return;

			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				isDownloadActive = true;
				AppendLog($"[WARNING] Synix close window button is now Disabled!", Color.Orange, true);
				await Core.Instance.UpdateServerAndReport(selectedServer);
				isDownloadActive = false;
				AppendLog($"[WARNING] Synix close window button is now Enabled!", Color.Orange, true);
			}
			else
			{
				MessageBox.Show("Please select a server in the list to update.", "No Server Selected");
			}
		}

		private async void btnFileValidation_Click(object sender, EventArgs e)
		{
			if (isInitializing) return;

			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				isDownloadActive = true;
				AppendLog($"[WARNING] Synix close window button is now Disabled!", Color.Orange, true);
				await Core.Instance.ValidationServerAndReport(selectedServer);
				isDownloadActive = false;
				AppendLog($"[WARNING] Synix close window button is now Enabled!", Color.Orange, true);
			}
			else
			{
				MessageBox.Show("Please select a server in the list to validate.", "No Server Selected");
			}
		}

		private void btnDelete_Click(object sender, EventArgs e)
		{
			if (isInitializing) return;

			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				Core.Instance.DeleteServerAndReport(selectedServer);
				dataGridView1.CurrentCell = null;
				dataGridView1.DataSource = null;
				dataGridView1.DataSource = serverList;
			}
			else
			{
				MessageBox.Show("Please select a server in the list first.", "No Server Selected");
			}
		}

		private async void btnBackup_Click(object sender, EventArgs e)
		{
			// 1. SELECTION CHECKS
			if (dataGridView1.CurrentRow == null)
			{
				AppendLog("[🚨 ERROR] No row is currently selected!", Color.Red);
				return;
			}

			if (!(dataGridView1.CurrentRow.DataBoundItem is GameServer selectedServer))
			{
				AppendLog("[🚨 ERROR] Invalid GameServer object!", Color.Red);
				return;
			}

			if (!Core.Instance.PassBackupSpamLock(selectedServer, out string lockMsg))
			{
				AppendLog(lockMsg, Color.Orange);
				return;
			}

			if (selectedServer.Status != StatusManager.GetStatus(ServerState.Stopped))
			{
				AppendLog($"[🚨 ERROR] {selectedServer.ServerName} must be Stopped to perform a backup.", Color.Orange);
				return;
			}

			selectedServer.Status = Core.StatusManager.GetStatus(Core.ServerState.BackingUp);
			isDownloadActive = true;

			AppendLog($"[WARNING] Synix close window button is now Disabled!", Color.Orange, true);
			AppendLog($"[BACKUP] Starting backup compression for {selectedServer.ServerName}...", Color.Cyan);

			await Task.Run(() =>
			{
				BackupManager.ExecuteBackup(selectedServer, StartContext.Manual);
			});

			isDownloadActive = false;
			selectedServer.Status = Core.StatusManager.GetStatus(Core.ServerState.Stopped);
			AppendLog($"[BACKUP] Finished backing up {selectedServer.ServerName}.", Color.LimeGreen);
			AppendLog($"[WARNING] Synix close window button is now Enabled!", Color.Orange, true);
			UpdateGrid();
		}

		private async void btnStart_Click(object sender, EventArgs e)
		{
			if (dataGridView1.CurrentRow == null)
			{
				AppendLog("[🚨 ERROR] No row is currently selected!", Color.Red);
				return;
			}

			if (!(dataGridView1.CurrentRow.DataBoundItem is GameServer selectedServer))
			{
				AppendLog("[🚨 ERROR] Invalid GameServer object!", Color.Red);
				return;
			}

			if (!Core.Instance.PassStartSpamLock(selectedServer, out string lockMsg))
			{
				AppendLog(lockMsg, Color.Orange);
				return;
			}

			await Core.Instance.ExecuteStartSequence(selectedServer);
		}

		private async void btnStop_Click(object sender, EventArgs e)
		{
			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				if (!Core.Instance.PassStopSpamLock(selectedServer, out string lockMsg))
				{
					AppendLog(lockMsg, Color.Orange);
					return;
				}

				await Core.Instance.StopServerAndReport(selectedServer);
			}
		}

		private void btnOpenConfig_Click(object sender, EventArgs e)
		{
			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				Core.Instance.OpenConfigEditor(selectedServer);
			}
		}

		private GameServer? GetSelectedServer()
		{
			if (dataGridView1.CurrentRow != null && dataGridView1.CurrentRow.DataBoundItem is GameServer server)
			{
				return server;
			}
			return null;
		}

		private void btnOpenFolder_Click(object sender, EventArgs e)
		{
			var selectedServer = GetSelectedServer();

			if (selectedServer != null)
			{
				Core.Instance.OpenServerFolder(selectedServer);
			}
			else
			{
				Core.Instance.Log("[SYSTEM] Please select a server from the list first.", System.Drawing.Color.Yellow);
			}
		}

		private void btnOpenBackup_Click(object sender, EventArgs e)
		{
			if (dataGridView1.CurrentRow == null || !(dataGridView1.CurrentRow.DataBoundItem is GameServer selectedServer)) return;

			string rootBackupPath = @"C:\Synix\BackupGames";

			string cleanGame = BackupManager.GetSafeName(selectedServer.Game);
			string cleanServer = BackupManager.GetSafeName(selectedServer.ServerName);

			string fullPath = Path.Combine(rootBackupPath, cleanGame, cleanServer);

			if (Directory.Exists(fullPath))
			{
				Process.Start("explorer.exe", fullPath);
				AppendLog($"[SYSTEM] Opening vault: {selectedServer.ServerName}", Color.Cyan);
			}
			else
			{
				AppendLog($"[SYNIX] Creating directory: {fullPath}", Color.Yellow);
				try
				{
					Directory.CreateDirectory(fullPath);
					Process.Start("explorer.exe", fullPath);
				}
				catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
			}
		}

		private async void btnPublicConnection_Click(object sender, EventArgs e)
		{
			var selectedServer = GetSelectedServer();
			if (selectedServer == null) return;

			Core.Instance.Log($"[NETWORK] Testing WAN Connectivity for {selectedServer.ServerName}...", Color.White);

			try
			{
				string publicIp = await Core.Instance.GetPublicIP();
				bool isResponding = await Core.Instance.TestServerConnectivity(publicIp, selectedServer.QueryPort);

				if (isResponding)
				{
					Core.Instance.Log($"[ONLINE] {selectedServer.ServerName} is visible at {publicIp}:{selectedServer.QueryPort}!", Color.Green);
				}
				else
				{
					Core.Instance.Log($"[BLOCK] {selectedServer.ServerName} is running but HIDDEN. Check Router/Firewall for UDP {selectedServer.QueryPort} or try setting a different query port.", Color.Red);
				}
			}
			catch (Exception ex)
			{
				Core.Instance.Log($"[🚨 ERROR] Could not retrieve Public IP: {ex.Message}", Color.Yellow);
			}
		}

		private async void btnLocalConnection_Click(object sender, EventArgs e)
		{
			var selectedServer = GetSelectedServer();
			if (selectedServer == null) return;

			Core.Instance.Log($"[NETWORK] Testing LAN Connectivity for {selectedServer.ServerName}...", Color.White);

			try
			{
				string localIp = await Core.Instance.GetLocalIP();
				bool isResponding = await Core.Instance.TestServerConnectivity(localIp, selectedServer.QueryPort);

				if (isResponding)
				{
					Core.Instance.Log($"[ONLINE] {selectedServer.ServerName} is visible at {localIp}:{selectedServer.QueryPort}!", Color.Green);
				}
				else
				{
					Core.Instance.Log($"[BLOCK] {selectedServer.ServerName} is running but HIDDEN. Check Router/Firewall for UDP {selectedServer.QueryPort} or try setting a different query port.", Color.Red);
				}
			}
			catch (Exception ex)
			{
				Core.Instance.Log($"[🚨 ERROR] Could not retrieve Public IP: {ex.Message}", Color.Yellow);
			}
		}

		private void btnServerActionsMenu_Click(object sender, EventArgs e)
		{
			contextMenuStrip.Show(btnServerActions, new System.Drawing.Point(0, 0), ToolStripDropDownDirection.AboveRight);
		}

		private async void btnRestart_Click(object sender, EventArgs e)
		{
			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				await Core.Instance.ExecuteStartSequence(selectedServer);
			}
		}

		private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0)
			{
				if (dataGridView1.Rows[e.RowIndex].DataBoundItem is GameServer selectedServer)
				{
					Synix_Control_Panel.Help.ServerInfo infoForm = new Synix_Control_Panel.Help.ServerInfo(selectedServer);
					infoForm.Show();
				}
			}
		}

		private void btnHelp_Click(object sender, EventArgs e)
		{
			using (Synix_Control_Panel.SynixEngine.HelpGUI helpWindow = new Synix_Control_Panel.SynixEngine.HelpGUI())
			{
				helpWindow.ShowDialog();
			}
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
	}
}
