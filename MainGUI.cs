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
using Synix_Control_Panel.Database;
using Synix_Control_Panel.Design;
using Synix_Control_Panel.FileFolderHandler;
using Synix_Control_Panel.MonitoringHandler;
using Synix_Control_Panel.ServerHandler;
using Synix_Control_Panel.SteamCMDHandler;
using Synix_Control_Panel.SynixEngine;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static Synix_Control_Panel.FileFolderHandler.FolderHandler;

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
			Instance = this;
		}

		private void tmrResourceUpdates_Tick(object sender, EventArgs e)
		{
			// Grab the numbers from the Brain
			double cpu = Synix_Control_Panel.SynixEngine.Core.Instance.TotalCpuUsage;
			double ram = Synix_Control_Panel.SynixEngine.Core.Instance.TotalRamUsageGb;

			// Update the Labels
			lblTotalCpu.Text = $"CPU: {cpu:N1}%";
			lblTotalRam.Text = $"RAM: {ram:N2} GB / {systemTotalRamGb:N1} GB (Usable)";

			// Update the Chart
			chartHeartbeat.Series["TotalCPU"].Points.AddXY(chartTickCounter, cpu);
			chartHeartbeat.Series["TotalRAM"].Points.AddXY(chartTickCounter, ram);

			// Keep the graph moving
			if (chartHeartbeat.Series["TotalCPU"].Points.Count > maxGraphPoints)
			{
				chartHeartbeat.Series["TotalCPU"].Points.RemoveAt(0);
				chartHeartbeat.Series["TotalRAM"].Points.RemoveAt(0);
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

		public void AppendLog(string message)
		{
			AppendLog(message, null, false);
		}

		public void AppendLog(string message, Color textColor)
		{
			AppendLog(message, textColor, false);
		}

		public void AppendLog(string message, Color? textColor, bool isBold)
		{
			if (!this.IsHandleCreated || this.IsDisposed) return;

			if (rtbLog.InvokeRequired)
			{
				rtbLog.BeginInvoke(new Action(() => AppendLog(message, textColor, isBold)));
				return;
			}

			string timeStamp = $"[{DateTime.Now:HH:mm:ss}] ";

			// Move to end
			rtbLog.SelectionStart = rtbLog.TextLength;
			rtbLog.SelectionLength = 0;

			// --- APPLY COLOR ---
			rtbLog.SelectionColor = textColor ?? rtbLog.ForeColor;

			// --- APPLY BOLD ---
			// We take the current font and just "add" the Bold style to it
			if (isBold)
				rtbLog.SelectionFont = new Font(rtbLog.Font, FontStyle.Bold);
			else
				rtbLog.SelectionFont = new Font(rtbLog.Font, FontStyle.Regular);

			// Print text
			rtbLog.AppendText(timeStamp + message + Environment.NewLine);

			// Reset for next time (important so next log isn't accidentally bold!)
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
			await Task.Run(() => SteamCMD.EnsureSteamCMD(AppendLog));

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

			// The AI handles the "Online" check, opens the form, and logs the result
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
				// The AI handles the "Online" check, the Confirmation, and the File Deletion
				Core.Instance.DeleteServerAndReport(selectedServer);
			}
			else
			{
				MessageBox.Show("Please select a server in the list first.", "No Selection");
			}
		}

		private void btnStart_Click(object sender, EventArgs e)
		{
			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				// 1. ENGINE INTEGRITY CHECK
				// This uses the Validator logic
				if (!Core.Instance.ValidateIntegrityAndReport(selectedServer)) return;

				// 2. CONFIG WARNING BLOCKER
				// This uses the Validator logic
				if (Core.Instance.ShouldBlockForConfig(selectedServer)) return;

				// 3. START THE SERVER
				Servers.Start(selectedServer, msg =>
				{
					this.Invoke((MethodInvoker)delegate
					{
						AppendLog(msg);

						// The Engine refreshes the grid every 1 second, 
						// but we call it here for instant feedback.
						UpdateGrid();
					});
				});
			}
		}

		private void btnStop_Click(object sender, EventArgs e)
		{
			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				// The AI handles the PID check, the stop command, and the log
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

		private void btnServerActionsMenu_Click(object sender, EventArgs e)
		{
			// Spawns the menu at the top-left corner of the button (0,0) and forces it to open UPWARDS
			contextMenuStrip.Show(btnServerActions, new System.Drawing.Point(0, 0), ToolStripDropDownDirection.AboveRight);
		}
	}
}