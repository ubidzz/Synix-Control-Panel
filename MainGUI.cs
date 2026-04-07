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
using System;
using System.Windows.Forms;
using Synix_Control_Panel.ServerHandler;
using Synix_Control_Panel.FileFolderHandler;
using Synix_Control_Panel.SteamCMDHandler;
using Synix_Control_Panel.MonitoringHandler;
using Synix_Control_Panel.Design;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms.DataVisualization.Charting;

namespace Synix_Control_Panel
{
	public partial class MainGUI : Form
	{
		public static BindingList<GameServer> serverList = new();
		private bool isDownloadActive = false;
		private static bool isInitializing = false;
		public static MainGUI? Instance { get; private set; }
		private const int maxGraphPoints = 60;

		public MainGUI()
		{
			InitializeComponent();
			CreateFiles.LoadServers();
			GridStyler.DarkTheme(dataGridView1);
			GridStyler.StyleHeartbeatChart(chartHeartbeat);
			chartHeartbeat.Series["TotalCPU"].Points.Clear();
			dataGridView1.DataSource = serverList;
			Instance = this;
		}

		private void tmrResourceUpdates_Tick(object sender, EventArgs e)
		{
			// A. Use your existing PID-based monitor to get the Total CPU
			var usage = Synix_Control_Panel.MonitoringHandler.ResourceMonitor.GetTotalResources(serverList);

			// B. Get the series object
			Series s = chartHeartbeat.Series["TotalCPU"];

			// C. Add the new "Heartbeat" point
			s.Points.AddY(usage.TotalCpuPercent);

			// D. "Shift" the graph: Remove the oldest point to make it roll
			if (s.Points.Count > maxGraphPoints)
			{
				s.Points.RemoveAt(0); // This creates the rolling effect
			}

			// E. Keep the labels updated too
			lblTotalCpu.Text = $"System Load: {usage.TotalCpuPercent:N1}%";
			lblTotalRam.Text = $"Total RAM: {usage.TotalRamMB:N0} MB";
		}

		private void UpdateGrid()
		{
			if (this.InvokeRequired)
			{
				this.BeginInvoke(new Action(UpdateGrid));
				return;
			}

			// All the "Nuclear Refresh" and scroll logic is hidden in the helper
			GridHelper.RefreshWithPersistence(dataGridView1, serverList);
		}

		public void AppendLog(string message)
		{
			if (!this.IsHandleCreated || this.IsDisposed) return;

			if (rtbLog.InvokeRequired)
			{
				rtbLog.BeginInvoke(new Action(() => AppendLog(message)));
				return;
			}

			string timeStamp = $"[{DateTime.Now:HH:mm:ss}] ";
			rtbLog.AppendText(timeStamp + message + Environment.NewLine);

			rtbLog.SelectionStart = rtbLog.Text.Length;
			rtbLog.ScrollToCaret();

			// ADD THIS: Forces the box to draw the new text IMMEDIATELY
			// This stops the "Frozen" look during SteamCMD installs
			rtbLog.Update();
		}

		private async void MainGUI_Shown(object sender, EventArgs e)
		{
			// 1. Set the lock immediately
			isDownloadActive = true;

			AppendLog("Checking SteamCMD dependencies...");

			// 2. Run the check on a background thread
			// This allows the 'X' button to stay active and trigger GUI_FormClosing
			await Task.Run(() => SteamCMD.EnsureSteamCMD(AppendLog));

			// 3. Release the lock once the background task is done
			isDownloadActive = false;

			AppendLog("Initialization complete.");
		}

		private void timerMonitor_Tick(object sender, EventArgs e)
		{
			// Pass your list and the log method
			Check.ServerStatus();

			// Refresh the Grid so the "Status" column updates visually
			UpdateGrid();
		}

		private async void btnAddServer_Click(object sender, EventArgs e)
		{
			using ServerSettingsGUI settingsForm = new();
			if (settingsForm.ShowDialog() == DialogResult.OK && settingsForm.NewServer != null)
			{
				GameServer newServer = settingsForm.NewServer;
				var gameData = GameDatabase.GetGame(newServer.Game);
				string correctAppId = gameData?.AppID ?? "";

				// 1. Set status to Installing immediately
				newServer.Status = "Installing";
				isDownloadActive = true;
				dataGridView1.Refresh();

				AppendLog($"--- AUTO-INSTALL STARTED: {newServer.Game} ---");
				string steamPath = @"C:\Games\SteamCMD\steamcmd.exe";

				// 2. Run SteamCMD in the background
				await Task.Run(() =>
				{
					ServerInstaller.Install(
						steamPath,
						newServer.InstallPath,
						correctAppId,
						AppendLog
					);
				});

				newServer.Status = "Offline";
				isDownloadActive = false;

				dataGridView1.Invalidate();
				dataGridView1.Refresh();

				CreateFiles.SaveServers();
				AppendLog($"--- AUTO-INSTALL FINISHED: {newServer.Game} ---");
			}
		}

		private void btnEdit_Click(object sender, EventArgs e)
		{
			if (isInitializing) return;

			if (dataGridView1.SelectedRows.Count > 0)
			{
				var selectedServer = (GameServer)dataGridView1.SelectedRows[0].DataBoundItem;

				if (selectedServer.Status == "Online")
				{
					MessageBox.Show("Please stop the server before editing.", "Server Active");
					return;
				}

				using var editForm = new ServerSettingsGUI(selectedServer);
				if (editForm.ShowDialog() == DialogResult.OK && editForm.NewServer != null)
				{
					// ServerSettingsGUI.cs handles the list swap and JSON save internally.
					dataGridView1.Refresh();
					AppendLog($"[SUCCESS] {editForm.NewServer.ServerName} updated and saved.");
				}
			}
			else
			{
				MessageBox.Show("Please select a server in the list first.");
			}
		}

		private void btnDelete_Click(object sender, EventArgs e)
		{
			if (isInitializing) return;

			if (dataGridView1.SelectedRows.Count > 0)
			{
				var selectedServer = (GameServer)dataGridView1.SelectedRows[0].DataBoundItem;

				if (selectedServer.Status == "Online")
				{
					MessageBox.Show("Stop the server before deleting it!", "Server Active");
					return;
				}

				// 1. Expanded Safety Confirmation (Stay in GUI)
				var confirm = MessageBox.Show($"Are you sure you want to delete '{selectedServer.ServerName}'?\n\n" +
											$"THIS WILL PERMANENTLY REMOVE ALL SERVER FILES AT:\n{selectedServer.InstallPath}",
											"Confirm Total Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

				if (confirm == DialogResult.Yes)
				{
					try
					{
						// 2. Call the backend to do the dirty work
						Delete.Server(selectedServer, AppendLog);

						// 3. Update the UI
						UpdateGrid();
					}
					catch (Exception ex)
					{
						// 4. Show your specific error message if the physical delete fails
						MessageBox.Show($"Files were deleted from the list, but some physical files couldn't be removed: {ex.Message}", "Cleanup Error");
					}
				}
			}
			else
			{
				MessageBox.Show("Click a server in the list first!");
			}
		}

		private void GUI_FormClosing(object sender, FormClosingEventArgs e)
		{
			// 1. Check if the 'steamcmd' process exists on the PC right now
			bool isSteamRunning = Process.GetProcessesByName("steamcmd").Any();

			// 2. Check your manual flag (for internal initialization)
			if (isDownloadActive || isSteamRunning)
			{
				MessageBox.Show("SteamCMD is currently active and performing operations.\n\n" +
								"Closing now will corrupt your game files or SteamCMD installation. " +
								"Please wait for the console to finish.",
								"Process in Progress",
								MessageBoxButtons.OK,
								MessageBoxIcon.Warning);

				e.Cancel = true; // LOCK THE X
			}
		}

		private void btnStart_Click(object sender, EventArgs e)
		{

			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				AppendLog($"Launching {selectedServer.Game}: {selectedServer.ServerName}...");

				// Set to running immediately
				selectedServer.Status = "Online";
				selectedServer.PID = startedProcess.Id;
				UpdateGrid(); 

				// 2. Hand it off to the ServerManager (The Expert)
				Servers.Start(selectedServer, msg =>
				{
					// Use Invoke to make sure the log updates on the UI thread safely
					if (this.InvokeRequired)
					{
						this.Invoke((System.Windows.Forms.MethodInvoker)delegate
						{
							AppendLog(msg);
							UpdateGrid(); // Refresh the GUI
						});
					}
					else
					{
						AppendLog(msg);
						UpdateGrid();
					}
				});
			}
			else
			{
				MessageBox.Show("Please select a server in the list first.");
			}
		}

		private void btnStop_Click(object sender, EventArgs e)
		{
			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				// Allow the stop attempt as long as there is a PID or a process object
				if (selectedServer.RunningProcess == null && !selectedServer.PID.HasValue)
				{
					MessageBox.Show("No active process found for this server.", "Info");
					return;
				}

				Servers.Stop(selectedServer, AppendLog);
				UpdateGrid();
			}
		}
		private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			// Let the GridStyler handle the colors
			GridStyler.SetStatusColor(dataGridView1, e);
		}
	}
}