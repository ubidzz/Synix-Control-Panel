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
using Synix_Control_Panel.FileFolderHandler;
using Synix_Control_Panel.MonitoringHandler;
using Synix_Control_Panel.ServerHandler;
using Synix_Control_Panel.SteamCMDHandler;
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
			GridStyler.DarkTheme(dataGridView1);
			GridStyler.HeartbeatChart(chartHeartbeat);
			chartHeartbeat.Series["TotalCPU"].Points.Clear();
			dataGridView1.DataSource = serverList;
			typeof(DataGridView).InvokeMember("DoubleBuffered",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty, null, dataGridView1, new object[] { true });
			GridStyler.ApplyTransparentTheme(dataGridView1);
			Instance = this;
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

		private void tmrResourceUpdates_Tick(object sender, EventArgs e)
		{
			// 1. Get current stats
			var usage = Synix_Control_Panel.MonitoringHandler.ResourceMonitor.GetTotalResources(serverList);
			double ramGB = usage.TotalRamMB / 1024.0;

			// 2. Chart Waves & Sliding Window
			var cpuSer = chartHeartbeat.Series["TotalCPU"];
			var ramSer = chartHeartbeat.Series["TotalRAM"];
			var ca = chartHeartbeat.ChartAreas[0];

			cpuSer.Points.AddXY(chartTickCounter, usage.TotalCpuPercent);
			ramSer.Points.AddXY(chartTickCounter, ramGB);
			chartTickCounter++;

			ca.AxisX.Minimum = chartTickCounter - maxGraphPoints;
			ca.AxisX.Maximum = chartTickCounter;

			// 3. Update Text (Using your 'N1' and 'N2' styles for that clean decimal look)
			lblTotalCpu.Text = $"CPU: {usage.TotalCpuPercent:N1}%";

			// We label this as "Usable" so the user knows why it's lower than their hardware total
			lblTotalRam.Text = $"RAM: {ramGB:N2} GB / {systemTotalRamGb:N1} GB (Usable)";

			// 4. Final Grid Refresh
			UpdateGrid();
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
			Servers.RebindProcesses(serverList);
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
				AppendLog($"--- [INFO] INSTALL {newServer.Game} can take up to 2-10 minutes ---", Color.DeepSkyBlue, true);
				AppendLog($"--- [WARNING] Synix close window button is disabled while the installation is in progress and will be disabled once the installation is complete! ---", Color.Orange, true);
				AppendLog($"--- [INFO] If it looks like it's not working steamCMD is running in the background and will not return real time results in till it finishes. ---", Color.OrangeRed, true);
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

				bool fixApplied = GameFix.PostInstall(newServer);

				if (fixApplied)
				{
					AppendLog($"[SUCCESS] Added the missing files to the {newServer.Game} server.");
				}
				newServer.Status = "Offline";
				isDownloadActive = false;
				AppendLog($"--- [WARNING] Synix close window button is now Enabled! ---", Color.Orange, true);

				dataGridView1.Invalidate();
				dataGridView1.Refresh();

				FileHandler.SaveServers();
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

		private async void btnUpdate_Click(object sender, EventArgs e)
		{
			// Prevent clicking if the app is still loading
			if (isInitializing) return;

			// 1. Make sure they actually selected a server in the grid
			if (dataGridView1.SelectedRows.Count > 0)
			{
				// Grab the server they clicked on
				var selectedServer = (GameServer)dataGridView1.SelectedRows[0].DataBoundItem;

				// 2. Safety Check: Don't update a running server!
				if (selectedServer.Status == "Online")
				{
					MessageBox.Show("You must stop the server before updating it.", "Server Active", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				// Safety Check: Don't update if it's already busy
				if (selectedServer.Status == "Updating" || selectedServer.Status == "Installing")
				{
					MessageBox.Show("This server is already busy.", "Busy", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}

				// Ask for confirmation just in case they clicked it by accident
				var confirmResult = MessageBox.Show($"Are you sure you want to update {selectedServer.ServerName}?",
										 "Confirm Update",
										 MessageBoxButtons.YesNo, MessageBoxIcon.Question);

				if (confirmResult != DialogResult.Yes) return;

				// 3. Get the AppID from the database
				var gameData = GameDatabase.GetGame(selectedServer.Game);
				string correctAppId = gameData?.AppID ?? "";

				if (string.IsNullOrEmpty(correctAppId))
				{
					MessageBox.Show("Could not find the AppID for this game. Cannot update.", "Error");
					return;
				}

				// 4. Update the UI to show it's working
				selectedServer.Status = "Updating";
				isDownloadActive = true;
				dataGridView1.Refresh();
				AppendLog($"--- UPDATE STARTED: {selectedServer.Game} ---");
				AppendLog($"--- [WARNING] Synix close window button is disabled while the update is in progress and will be disabled once the update is complete! ---", Color.Orange, true);
				AppendLog($"--- [INFO] Updating {selectedServer.Game} can take up to 5 minutes ---", Color.DeepSkyBlue, true);

				string steamPath = @"C:\Games\SteamCMD\steamcmd.exe";

				// 5. Run SteamCMD in the background (Re-using your Installer!)
				await Task.Run(() =>
				{
					ServerInstaller.Install(
						steamPath,
						selectedServer.InstallPath,
						correctAppId,
						AppendLog
					);
				});

				// 6. STEAMCMD IS DONE! 
				// Re-apply our custom game fixes just in case Steam wiped out the DLLs or configs!
				bool fixApplied = GameFix.PostInstall(selectedServer);
				if (fixApplied)
				{
					AppendLog($"[SUCCESS] Re-applied missing files to the {selectedServer.Game} server after the update.");
				}

				// 7. Put the server back to normal
				selectedServer.Status = "Offline";
				AppendLog($"--- [WARNING] Synix close window button is now Enabled! ---", Color.Orange, true);
				isDownloadActive = false;

				dataGridView1.Invalidate();
				dataGridView1.Refresh();

				AppendLog($"--- UPDATE FINISHED: {selectedServer.Game} ---");
			}
			else
			{
				MessageBox.Show("Please select a server in the list to update.", "No Server Selected");
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
						ServerFolder.Delete(selectedServer, AppendLog);

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

		private void btnStart_Click(object sender, EventArgs e)
		{
			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				// 1. Just call your method. It handles the PID and Status internally!
				Servers.Start(selectedServer, msg =>
				{
					this.Invoke((MethodInvoker)delegate
					{
						AppendLog(msg);
						UpdateGrid();
					});
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

		private void btnOpenConfig_Click(object sender, EventArgs e)
		{
			// 1. Get the server the user has highlighted in the grid
			if (dataGridView1.CurrentRow?.DataBoundItem is GameServer selectedServer)
			{
				// 2. Look up the Game Database to find the file path for this specific game
				// Note: Use your actual public list name (e.g., Games or GetGames)
				var blueprint = GameDatabase.GetGames.FirstOrDefault(g => g.Game == selectedServer.Game);

				if (blueprint != null && !string.IsNullOrEmpty(blueprint.RelativeConfigPath))
				{
					// 3. Stitch the Install Path and the Config Path together
					// Result: C:\Games\MySoulmask\WS\Saved\Config\...\GameUserSettings.ini
					string fullConfigPath = Path.Combine(selectedServer.InstallPath, blueprint.RelativeConfigPath);

					// 4. Double-check the file actually exists before trying to open it
					if (File.Exists(fullConfigPath))
					{
						// Open the editor form and pass it the path
						ServerConfig editor = new ServerConfig(fullConfigPath, blueprint.Format);
						editor.ShowDialog();
					}
					else
					{
						MessageBox.Show($"Could not find the config file at:\n{fullConfigPath}", "Missing Config");
					}
				}
				else
				{
					MessageBox.Show("This game does not have a config path defined in the GameDatabase.", "No Config Path");
				}
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
	}
}