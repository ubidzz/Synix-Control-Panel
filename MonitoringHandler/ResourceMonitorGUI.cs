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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Synix_Control_Panel.ServerHandler;

namespace Synix_Control_Panel
{
	public partial class ResourceMonitorGUI : Form
	{
		private Image originalBg = Properties.Resources.logo;

		public ResourceMonitorGUI()
		{
			InitializeComponent();

			// 1. FLICKER FIX
			PropertyInfo cp = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);
			cp.SetValue(listViewResources, true, null);

			// 2. STYLING
			lblTotalCpu.Font = new Font(lblTotalCpu.Font, FontStyle.Bold);
			lblTotalRam.Font = new Font(lblTotalRam.Font, FontStyle.Bold);

			// 3. COLUMN SETUP (0:PID, 1:Name, 2:CPU, 3:RAM, 4:EXE)
			listViewResources.Columns[0].Width = 60;
			listViewResources.Columns[1].Width = 180;
			listViewResources.Columns[2].Width = 80;
			listViewResources.Columns[3].Width = 80;
			listViewResources.Columns[4].Width = 200;

			// 4. OWNER DRAW ENABLED
			listViewResources.OwnerDraw = true;

			// 5. ATTACH THE RESIZE LOGIC
			this.Load += (s, e) => listViewResources_Resize(this, EventArgs.Empty);

			tmrRefresh_Tick(this, EventArgs.Empty);
		}

		private void tmrRefresh_Tick(object sender, EventArgs e)
		{
			listViewResources.BeginUpdate();
			listViewResources.Items.Clear();

			var totalUsage = Synix_Control_Panel.MonitoringHandler.ResourceMonitor.GetTotalResources(MainGUI.serverList);

			foreach (var server in MainGUI.serverList.ToList())
			{
				// We only show running servers in the monitor
				if (server.RunningProcess == null || server.RunningProcess.HasExited) continue;

				try
				{
					server.RunningProcess.Refresh();

					// DATA GATHERING
					string pid = server.RunningProcess.Id.ToString();
					string name = server.ServerName;
					string exe = server.RunningProcess.ProcessName + ".exe";

					// CPU MATH
					double currentCpuMillis = server.RunningProcess.TotalProcessorTime.TotalMilliseconds;
					DateTime currentTime = DateTime.Now;
					double cpuUsedMs = currentCpuMillis - server.LastCpuMillis;
					double elapsedMs = (currentTime - server.LastSampleTime).TotalMilliseconds;
					double cpuPercent = (cpuUsedMs / (elapsedMs * Environment.ProcessorCount)) * 100;
					if (cpuPercent < 0 || server.LastCpuMillis == 0) cpuPercent = 0;

					server.LastCpuMillis = currentCpuMillis;
					server.LastSampleTime = currentTime;

					// STRINGS
					string cpuDisplay = cpuPercent.ToString("N1") + "%";
					string ramDisplay = (server.RunningProcess.WorkingSet64 / 1024.0 / 1024.0 / 1024.0).ToString("N2") + " GB";

					// MAPPING
					ListViewItem row = new ListViewItem(pid);
					row.SubItems.Add(name);
					row.SubItems.Add(cpuDisplay);
					row.SubItems.Add(ramDisplay);
					row.SubItems.Add(exe);

					// IMPORTANT: This tells the drawing code to color the row
					// We check if the server is Running (since it's running, it is!)
					row.Tag = true;

					listViewResources.Items.Add(row);
				}
				catch { continue; }
			}

			// TOTALS AND COLORS
			lblTotalCpu.Text = $"Total CPU Usage: {totalUsage.TotalCpuPercent:N1}%";
			double totalRamGb = totalUsage.TotalRamMB / 1024.0;
			double maxUsable = MainGUI.Instance?.systemTotalRamGb ?? 91.0;
			double ramPercent = (totalRamGb / maxUsable) * 100;
			lblTotalRam.Text = $"Total RAM Usage: {totalRamGb:N2} GB / {maxUsable:N1} GB ({ramPercent:N1}%)";

			if (ramPercent >= 90) lblTotalRam.ForeColor = Color.Red;
			else if (ramPercent >= 75) lblTotalRam.ForeColor = Color.Orange;
			else lblTotalRam.ForeColor = Color.Lime;

			listViewResources.EndUpdate();
		}

		// --- OWNER DRAWING METHODS ---

		private void listViewResources_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
		{
			e.DrawDefault = true; // Keep the standard header look
		}

		private void listViewResources_DrawItem(object sender, DrawListViewItemEventArgs e)
		{
			// We handle everything in DrawSubItem, so we tell this to do nothing
			e.DrawDefault = false;
		}

		private void listViewResources_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
		{
			bool isRunning = (e.Item.Tag is bool status) && status;

			// 1. DRAW THE HIGHLIGHT (If Running)
			if (isRunning)
			{
				// Alpha: 50 (Very transparent) to 100 (Noticeable). 
				using (SolidBrush brush = new SolidBrush(Color.FromArgb(50, 0, 255, 0)))
				{
					e.Graphics.FillRectangle(brush, e.Bounds);
				}
			}

			// 2. TEXT COLORS (Lime for data, Cyan for Names)
			Color txtColor = Color.Lime;
			if (e.ColumnIndex == 1 || e.ColumnIndex == 4) txtColor = Color.Cyan;

			// 3. TEXT ALIGNMENT
			TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
			if (e.ColumnIndex == 0 || e.ColumnIndex == 2 || e.ColumnIndex == 3)
				flags |= TextFormatFlags.HorizontalCenter;
			else
				flags |= TextFormatFlags.Left;

			// 4. DRAW THE TEXT
			TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.Item.Font, e.Bounds, txtColor, flags);
		}

		private void listViewResources_Resize(object sender, EventArgs e)
		{
			if (listViewResources.Width > 0 && listViewResources.Height > 0 && originalBg != null)
			{
				// 1. --- STRETCH THE EXECUTABLE COLUMN ---
				// Calculation: Total Width - (PID + ServerName + CPU + RAM)
				// We subtract an extra 20-25 pixels to account for the vertical scrollbar
				int otherColumnsWidth = listViewResources.Columns[0].Width +
										listViewResources.Columns[1].Width +
										listViewResources.Columns[2].Width +
										listViewResources.Columns[3].Width;

				int remainingWidth = listViewResources.ClientSize.Width - otherColumnsWidth;

				// Set the last column (Index 4) to fill the rest of the space
				if (remainingWidth > 100) // Safety check so it doesn't disappear
				{
					listViewResources.Columns[4].Width = remainingWidth;
				}

				// 2. --- STRETCH THE BACKGROUND IMAGE (Your existing code) ---
				Bitmap bmp = new Bitmap(originalBg, listViewResources.Width, listViewResources.Height);
				listViewResources.BackgroundImage?.Dispose();
				listViewResources.BackgroundImage = bmp;
			}
		}
	}
}