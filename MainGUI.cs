/*
 * Copyright (c) 2026 ubidzz. All Rights Reserved.
 *
 * This file is part of Synix Control Panel.
 *
 * This code is provided for transparent viewing and personal use only.
 * Unauthorized distribution, public modification, or commercial 
 * use of this source code or the compiled executable is strictly 
 * prohibited. Please refer to the LICENSE file in the root 
 * directory for full terms.
 */
using Synix_Control_Panel.Design;
using Synix_Control_Panel.ServerHandler;
using Synix_Control_Panel.SteamCMDHandler;
using Synix_Control_Panel.SynixEngine;
using System.ComponentModel;

namespace Synix_Control_Panel
{
	public partial class MainGUI : Form
	{
		public static BindingList<GameServer> serverList = new();
		private bool isDownloadActive = false;
		private static bool isInitializing = false;
		public static MainGUI? Instance { get; private set; }
		public double systemTotalRamGb = 98.0;
		private int chartTickCounter = 0;
		private const int maxGraphPoints = 60;

		public MainGUI()
		{
			InitializeComponent();
			Instance = this;
			FileHandler.LoadServers();
			var engine = Synix_Control_Panel.SynixEngine.Core.Instance;
			GridStyler.DarkTheme(dataGridView1);
			GridStyler.HeartbeatChart(chartHeartbeat);
			chartHeartbeat.Series["TotalCPU"].Points.Clear();
			dataGridView1.DataSource = serverList;
			typeof(DataGridView).InvokeMember("DoubleBuffered",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty, null, dataGridView1, new object[] { true });
			GridStyler.ApplyTransparentTheme(dataGridView1);
			Core.Instance.RebindProcesses();
			Instance = this;
			_ = LoadNetworkInfo();
		}

		private void tmrResourceUpdates_Tick(object sender, EventArgs e)
		{
			// 1. Grab telemetry from the Singleton Engine
			double cpu = Synix_Control_Panel.SynixEngine.Core.Instance.TotalCpuUsage;
			double ram = Synix_Control_Panel.SynixEngine.Core.Instance.TotalRamUsageGb;

			// 2. Update the Text Labels
			lblTotalCpu.Text = $"CPU: {cpu:N1}%";
			lblTotalRam.Text = $"RAM: {ram:N2} GB / {systemTotalRamGb:N1} GB (Usable)";

			// 3. Add new data points
			chartHeartbeat.Series["TotalCPU"].Points.AddXY(chartTickCounter, cpu);
			chartHeartbeat.Series["TotalRAM"].Points.AddXY(chartTickCounter, ram);

			// 4. ANIMATION LOGIC: Lock the Viewport to the last 'maxGraphPoints'
			// This creates the right-to-left scrolling effect
			var chartArea = chartHeartbeat.ChartAreas[0];
			chartArea.AxisX.Minimum = chartTickCounter - maxGraphPoints;
			chartArea.AxisX.Maximum = chartTickCounter;

			// 5. Cleanup: Keep the data buffer small to save memory
			if (chartHeartbeat.Series["TotalCPU"].Points.Count > maxGraphPoints + 10)
			{
				chartHeartbeat.Series["TotalCPU"].Points.RemoveAt(0);
				chartHeartbeat.Series["TotalRAM"].Points.RemoveAt(0);
			}

			// 6. Handle Scheduled Restarts from servers.json
			foreach (var server in serverList)
			{
				if (server.IsScheduledRestartEnabled)
				{
					string currentTime = DateTime.Now.ToString("HH:mm");
					if (server.RestartTime == currentTime)
					{
						// Autonomous recovery sequence: Save -> Shutdown -> Reboot
						_ = Core.Instance.ExecuteRestartSequence(server);
					}
				}
			}

			chartTickCounter++;
		}

		private void MainGUI_Load(object sender, EventArgs e)
		{
			try
			{
				// 1. Get real hardware total
				double physicalRam = MonitoringHandler.ResourceMonitor.GetTotalSystemRamGB();
				if (physicalRam <= 0) physicalRam = 98.0;

				// 2. The 7GB Buffer: Subtract 7 so Windows stays happy
				double reserved = Math.Max(physicalRam * 0.15, 3.0); // Reserve 15% or at least 3GB
				systemTotalRamGb = physicalRam - reserved;

				// 3. Apply styles with the NEW limit
				Design.GridStyler.HeartbeatChart(chartHeartbeat, systemTotalRamGb);
				Design.GridStyler.DashboardLabels(lblTotalCpu, lblTotalRam);

				UpdateGrid();
				tmrResourceUpdates.Start();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error loading Synix: " + ex.Message);
			}
		}

		private async Task LoadNetworkInfo()
		{
			// 1. Get the LAN IP instantly
			string localIP = Core.Instance.GetLocalIP();
			lblLocalIP1.Text = $"LAN IP: {localIP}";

			// 2. Get the Public IP in the background
			lblPublicIP.Text = "Public IP: Fetching..."; // Fix: was using lblLocalIP
			string publicIP = await Core.Instance.GetPublicIP();
			lblPublicIP.Text = $"Public IP: {publicIP}"; // Fix: was using lblLocalIP
		}

		private void lblPublicIP_Click(object sender, EventArgs e)
		{
			// Strip the prefix and copy just the IP
			string ip = lblPublicIP.Text.Replace("Public IP: ", "");
			if (ip != "Offline" && ip != "Fetching...")
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
				System.IO.Directory.CreateDirectory(logDirectory);
				string logFilePath = System.IO.Path.Combine(logDirectory, "synix_engine.log");
				string timeStampedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
				System.IO.File.AppendAllText(logFilePath, timeStampedMessage);
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

			if (isBold)
				rtbLog.SelectionFont = new Font(rtbLog.Font, FontStyle.Bold);
			else
				rtbLog.SelectionFont = new Font(rtbLog.Font, FontStyle.Regular);

			// Print text
			rtbLog.AppendText(timeStamp + message + Environment.NewLine);

			rtbLog.SelectionFont = rtbLog.Font;

			// Scroll and Refresh
			rtbLog.SelectionStart = rtbLog.Text.Length;
			rtbLog.ScrollToCaret();
			rtbLog.Update();
		}

		private async void MainGUI_Shown(object sender, EventArgs e)
		{
			// 1. Set the lock immediately
			isDownloadActive = true;
			await Task.Delay(100);
			AppendLog("Checking SteamCMD dependencies...");
			AppendLog($"--- [WARNING] Synix close window button is now Disabled! ---", Color.Orange, true);
			// 2. Run the check on a background thread
			// This allows the 'X' button to stay active and trigger GUI_FormClosing
			await Task.Run(() => SteamCMD.EnsureSteamCMD(msg => AppendLog(msg)));

			// 3. Release the lock once the background task is done
			isDownloadActive = false;
			AppendLog($"--- [WARNING] Synix close window button is now Enabled! ---", Color.Orange, true);
			AppendLog("Initialization complete.");
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

			// The AI handles the window, the download, the fixes, and the logging
			await Core.Instance.AddServerAndReport();
		}

		private void btnEdit_Click(object sender, EventArgs e)
		{
			// UI-specific safety check
			if (isInitializing) return;

			// The AI handles the "Running" check, opens the form, and logs the result
			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				Core.Instance.EditServerAndReport(selectedServer);
			}
			else
			{
				MessageBox.Show("Please select a server in the list first.", "No Selection");
			}
		}

		private async void btnUpdate_Click(object sender, EventArgs e)
		{
			// UI-specific check
			if (isInitializing) return;

			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				// The AI handles everything: Safety, Database, SteamCMD, and Logging
				await Core.Instance.UpdateServerAndReport(selectedServer);
			}
			else
			{
				MessageBox.Show("Please select a server in the list to update.", "No Server Selected");
			}
		}

		private void btnDelete_Click(object sender, EventArgs e)
		{
			// 1. Check if the app is still loading
			if (isInitializing) return;

			// 2. Hand over the server object to the AI
			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				// The AI handles the "Running" check, the Confirmation, and the File Deletion
				Core.Instance.DeleteServerAndReport(selectedServer);
				dataGridView1.CurrentCell = null;
				dataGridView1.DataSource = null;
				dataGridView1.DataSource = serverList;
			}
			else
			{
				MessageBox.Show("Please select a server in the list first.", "No Selection");
			}
		}

		// 🎯 1. Change 'void' to 'async void' (standard for event handlers)
		private async void btnStart_Click(object sender, EventArgs e)
		{
			AppendLog("Start button clicked...", Color.Cyan);

			// 1. SELECTION CHECKS
			if (dataGridView1.CurrentRow == null)
			{
				AppendLog("[ERROR] No row is currently selected!", Color.Red);
				return;
			}

			if (!(dataGridView1.CurrentRow.DataBoundItem is GameServer selectedServer))
			{
				AppendLog("[ERROR] Invalid GameServer object!", Color.Red);
				return;
			}

			// 2. SAFETY & RESOURCE CHECKS
			if (!Core.Instance.PassStartSpamLock(selectedServer, out string lockMsg))
			{
				AppendLog(lockMsg, Color.Orange);
				return;
			}

			// 🎯 THE RESOURCE SAFEGUARD: Check CPU and RAM before we do anything else
			// This respects your 7GB Windows buffer logic
			if (!Core.Instance.PassResourceGuard(out string guardMsg))
			{
				AppendLog(guardMsg, Color.Red, true); // Bold red for critical resource warnings
				MessageBox.Show(guardMsg, "System Resource Exhaustion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return; // 🛑 Launch aborted
			}

			if (!Core.Instance.ValidateIntegrityAndReport(selectedServer)) return;
			if (Core.Instance.ShouldBlockForConfig(selectedServer)) return;

			// 3. UI PRE-FLIGHT: Show "Backing up" in the grid immediately
			if (selectedServer.BackupOnStart)
			{
				selectedServer.Status = Core.StatusManager.GetStatus(Core.ServerState.BackingUp);
				UpdateGrid();
			}

			// 🚀 4. EXECUTION: The actual launch process
			await Servers.Start(selectedServer, msg =>
			{
				this.Invoke((MethodInvoker)delegate
				{
					selectedServer.StartTime = DateTime.Now;
					AppendLog(msg);
					UpdateGrid(); // Final refresh for "Running"
				});
			});
		}

		private void btnStop_Click(object sender, EventArgs e)
		{
			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				// 🛡️ 1. THE SPAM LOCK
				if (!Core.Instance.PassStopSpamLock(selectedServer, out string lockMsg))
				{
					// If it's already stopping or dead, print the orange warning and block the click!
					AppendLog(lockMsg, Color.Orange);
					return;
				}

				// 🚀 2. SENDS COMMAND: MainGUI -> Engine -> Servers.cs
				// If it passes the bouncer, hand it off to the Engine to do the graceful shutdown.
				Core.Instance.StopServerAndReport(selectedServer);
			}
		}

		private void btnOpenConfig_Click(object sender, EventArgs e)
		{
			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				// One line. The AI handles the lookup, the file check, and the window.
				Core.Instance.OpenConfigEditor(selectedServer);
			}
		}

		private GameServer? GetSelectedServer()
		{
			// Checks if a row is selected and pulls the GameServer object from it
			if (dataGridView1.CurrentRow != null && dataGridView1.CurrentRow.DataBoundItem is GameServer server)
			{
				return server;
			}
			return null;
		}

		private void btnOpenFolder_Click(object sender, EventArgs e)
		{
			// 1. Get the server from the grid
			var selectedServer = GetSelectedServer();

			if (selectedServer != null)
			{
				// 2. Call your function in Actions.cs
				// Note: Make sure your Actions class has a public static Instance!
				Core.Instance.OpenServerFolder(selectedServer);
			}
			else
			{
				// Safety log if they click with no server selected
				Core.Instance.Log("[SYSTEM] Please select a server from the list first.", System.Drawing.Color.Yellow);
			}
		}

		private async void btnTestConnection_Click(object sender, EventArgs e)
		{
			var selectedServer = GetSelectedServer();
			if (selectedServer == null) return;

			Core.Instance.Log($"[NETWORK] Testing WAN Connectivity for {selectedServer.ServerName}...", Color.White);

			try
			{
				// 1. Get the actual Public IP of the Texas machine
				string publicIp = await Core.Instance.GetPublicIP();

				// 2. Probe the Query Port over the internet (UDP)
				bool isResponding = await Core.Instance.TestServerConnectivity(publicIp, selectedServer.QueryPort);

				if (isResponding)
				{
					Core.Instance.Log($"[ONLINE] {selectedServer.ServerName} is visible at {publicIp}:{selectedServer.QueryPort}!", Color.Green);
				}
				else
				{
					Core.Instance.Log($"[BLOCK] {selectedServer.ServerName} is running but HIDDEN. Check Router/Firewall for UDP {selectedServer.QueryPort}.", Color.Red);
				}
			}
			catch (Exception ex)
			{
				Core.Instance.Log($"[ERROR] Could not retrieve Public IP: {ex.Message}", Color.Yellow);
			}
		}

		// Helper to make sure the button knows which server you clicked

		private void btnServerActionsMenu_Click(object sender, EventArgs e)
		{
			// Spawns the menu at the top-left corner of the button (0,0) and forces it to open UPWARDS
			contextMenuStrip.Show(btnServerActions, new System.Drawing.Point(0, 0), ToolStripDropDownDirection.AboveRight);
		}

		private async void btnRestart_Click(object sender, EventArgs e)
		{
			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				await Core.Instance.ExecuteRestartSequence(selectedServer);
			}
		}

		private void btnHelp_Click(object sender, EventArgs e)
		{
			using (Synix_Control_Panel.SynixEngine.HelpGUI helpWindow = new Synix_Control_Panel.SynixEngine.HelpGUI())
			{
				helpWindow.ShowDialog();
			}
		}
	}
}