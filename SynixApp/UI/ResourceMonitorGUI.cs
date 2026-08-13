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
using Synix_Control_Panel.SynixApp.MonitoringHandler;
using System.Reflection;

namespace Synix_Control_Panel
{
	public partial class ResourceMonitorGUI : Form
	{
		private Image originalBg = Properties.Resources.logo;

		public ResourceMonitorGUI()
		{
			InitializeComponent();
			this.FormClosed += ResourceMonitorGUI_FormClosed;

			PropertyInfo cp = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);
			cp.SetValue(listViewResources, true, null);

			lblTotalCpu.Font = new Font(lblTotalCpu.Font, FontStyle.Bold);
			lblTotalRam.Font = new Font(lblTotalRam.Font, FontStyle.Bold);

			listViewResources.Columns[0].Width = 60;
			listViewResources.Columns[1].Width = 180;
			listViewResources.Columns[2].Width = 80;
			listViewResources.Columns[3].Width = 80;
			listViewResources.Columns[4].Width = 200;
			listViewResources.OwnerDraw = true;

			this.Load += (s, e) => listViewResources_Resize(this, EventArgs.Empty);

			tmrRefresh_Tick(this, EventArgs.Empty);
		}

		private void tmrRefresh_Tick(object sender, EventArgs e)
		{
			listViewResources.BeginUpdate();
			listViewResources.Items.Clear();

			var totalUsage = ResourceMonitor.GetTotalResources(MainGUI.serverList);

			foreach (var server in MainGUI.serverList.ToList())
			{
				if (server.RunningProcess == null || server.RunningProcess.HasExited) continue;

				try
				{
					server.RunningProcess.Refresh();
					string pid = server.RunningProcess.Id.ToString();
					string name = server.ServerName;
					string exe = server.RunningProcess.ProcessName + ".exe";

					double currentCpuMillis = server.RunningProcess.TotalProcessorTime.TotalMilliseconds;
					DateTime currentTime = DateTime.Now;
					double cpuUsedMs = currentCpuMillis - server.LastCpuMillis;
					double elapsedMs = (currentTime - server.LastSampleTime).TotalMilliseconds;
					double cpuPercent = (cpuUsedMs / (elapsedMs * Environment.ProcessorCount)) * 100;
					if (cpuPercent < 0 || server.LastCpuMillis == 0) cpuPercent = 0;
					server.LastCpuMillis = currentCpuMillis;
					server.LastSampleTime = currentTime;
					string cpuDisplay = cpuPercent.ToString("N1") + "%";
					string ramDisplay = (server.RunningProcess.WorkingSet64 / 1024.0 / 1024.0 / 1024.0).ToString("N2") + " GB";
					ListViewItem row = new ListViewItem(pid);
					row.SubItems.Add(name);
					row.SubItems.Add(cpuDisplay);
					row.SubItems.Add(ramDisplay);
					row.SubItems.Add(exe);
					row.Tag = true;

					listViewResources.Items.Add(row);
				}
				catch { continue; }
			}

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

		private void listViewResources_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
		{
			e.DrawDefault = true;
		}

		private void listViewResources_DrawItem(object sender, DrawListViewItemEventArgs e)
		{
			e.DrawDefault = false;
		}

		private void listViewResources_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
		{
			bool isRunning = (e.Item.Tag is bool status) && status;

			if (isRunning)
			{
				using (SolidBrush brush = new SolidBrush(Color.FromArgb(50, 0, 255, 0)))
				{
					e.Graphics.FillRectangle(brush, e.Bounds);
				}
			}

			Color txtColor = Color.Lime;
			if (e.ColumnIndex == 1 || e.ColumnIndex == 4) txtColor = Color.Cyan;

			TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
			if (e.ColumnIndex == 0 || e.ColumnIndex == 2 || e.ColumnIndex == 3)
				flags |= TextFormatFlags.HorizontalCenter;
			else
				flags |= TextFormatFlags.Left;

			TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.Item.Font, e.Bounds, txtColor, flags);
		}

		private void listViewResources_Resize(object sender, EventArgs e)
		{
			if (listViewResources.Width > 0 && listViewResources.Height > 0 && originalBg != null)
			{
				int otherColumnsWidth = listViewResources.Columns[0].Width +
										listViewResources.Columns[1].Width +
										listViewResources.Columns[2].Width +
										listViewResources.Columns[3].Width;

				int remainingWidth = listViewResources.ClientSize.Width - otherColumnsWidth;

				if (remainingWidth > 100)
				{
					listViewResources.Columns[4].Width = remainingWidth;
				}

				Bitmap bmp = new Bitmap(originalBg, listViewResources.Width, listViewResources.Height);
				listViewResources.BackgroundImage?.Dispose();
				listViewResources.BackgroundImage = bmp;
			}
		}

		private void ResourceMonitorGUI_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (originalBg != null)
			{
				originalBg.Dispose();
				originalBg = null;
			}

			if (lblTotalCpu.Font != null) lblTotalCpu.Font.Dispose();
			if (lblTotalRam.Font != null) lblTotalRam.Font.Dispose();
			if (tmrRefresh != null)
			{
				tmrRefresh.Stop();
				tmrRefresh.Dispose();
			}

			this.Dispose();

			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
	}
}
