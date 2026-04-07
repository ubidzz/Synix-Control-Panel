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
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Synix_Control_Panel.Design
{
	public static class GridStyler
	{
		public static Color CpuColor = Color.Cyan;
		public static Color RamColor = Color.MediumPurple;
		public static Color PanelBg = Color.FromArgb(25, 25, 25);
		public static Color ChartAreaBg = Color.FromArgb(15, 15, 15);

		public static void DarkTheme(DataGridView dgv)
		{
			// 1. Data Mapping & Core Logic
			dgv.AutoGenerateColumns = false;

			// Map the Columns to the Class Properties
			// Note: These must match the (Name) property in your Designer
			if (dgv.Columns.Contains("colName")) dgv.Columns["colName"].DataPropertyName = "ServerName";
			if (dgv.Columns.Contains("colGame")) dgv.Columns["colGame"].DataPropertyName = "Game";
			if (dgv.Columns.Contains("colPort")) dgv.Columns["colPort"].DataPropertyName = "Port";
			if (dgv.Columns.Contains("colPassword")) dgv.Columns["colPassword"].DataPropertyName = "Password";
			if (dgv.Columns.Contains("colAdminPassword")) dgv.Columns["colAdminPassword"].DataPropertyName = "AdminPassword";
			if (dgv.Columns.Contains("colStatus")) dgv.Columns["colStatus"].DataPropertyName = "Status";

			// 2. Dark Theme Backgrounds
			dgv.BackgroundColor = Color.FromArgb(25, 25, 25);
			dgv.DefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
			dgv.DefaultCellStyle.ForeColor = Color.WhiteSmoke;
			dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(60, 60, 60);
			dgv.DefaultCellStyle.SelectionForeColor = Color.White;

			// 3. Grid & Row Header Configuration
			dgv.RowHeadersVisible = false;
			dgv.BorderStyle = BorderStyle.None;
			dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
			dgv.GridColor = Color.FromArgb(50, 50, 50);
			dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dgv.AllowUserToResizeRows = false;

			// 4. Modern Column Header Styling
			dgv.EnableHeadersVisualStyles = false;
			dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
			dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);
			dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Cyan;
			dgv.ColumnHeadersHeight = 40;
			dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

			// 5. Automated Column Cleanup & Formatting
			if (dgv.Columns.Count > 0)
			{
				// Hide technical data
				if (dgv.Columns.Contains("PID")) dgv.Columns["PID"].Visible = false;
				if (dgv.Columns.Contains("AppID")) dgv.Columns["AppID"].Visible = false;

				// Stretch the path to fill space
				if (dgv.Columns.Contains("InstallPath"))
					dgv.Columns["InstallPath"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			}
		}

		public static void HeartbeatChart(Chart chart, double maxRamGb)
		{
			if (chart == null) return;

			// 1. Base Setup
			chart.Series.Clear();
			chart.ChartAreas.Clear();
			chart.Legends.Clear();
			chart.BackColor = Color.Transparent;
			chart.AntiAliasing = AntiAliasingStyles.All;

			ChartArea ca = chart.ChartAreas.Add("Default");
			ca.BackColor = Color.FromArgb(15, 15, 15); // Deep dark plot area

			// 2. CPU Axis (Left Side: 0-100%)
			ca.AxisY.Minimum = 0;
			ca.AxisY.Maximum = 100;
			ca.AxisY.LabelStyle.Enabled = false;
			ca.AxisY.MajorGrid.LineColor = Color.FromArgb(40, 40, 40);

			// 3. RAM Axis (Right Side: 0 to Total System GB)
			ca.AxisY2.Enabled = AxisEnabled.True;
			ca.AxisY2.Minimum = 0;
			ca.AxisY2.Maximum = maxRamGb; // This prevents the "Filled Block" issue
			ca.AxisY2.LabelStyle.Enabled = false;
			ca.AxisY2.MajorGrid.Enabled = false;

			// 4. X-Axis (Timeline)
			ca.AxisX.LabelStyle.Enabled = false;
			ca.AxisX.MajorGrid.LineColor = Color.FromArgb(40, 40, 40);

			// 5. RAM Series (Background Layer - Purple)
			Series ramSer = chart.Series.Add("TotalRAM");
			ramSer.ChartType = SeriesChartType.SplineArea;
			ramSer.YAxisType = AxisType.Secondary; // Link to GB scale
			ramSer.Color = Color.FromArgb(80, 150, 0, 200); // Semi-transparent
			ramSer.BorderColor = Color.MediumPurple;
			ramSer.BorderWidth = 1;

			// 6. CPU Series (Foreground Layer - Cyan)
			Series cpuSer = chart.Series.Add("TotalCPU");
			cpuSer.ChartType = SeriesChartType.SplineArea;
			cpuSer.YAxisType = AxisType.Primary; // Link to % scale
			cpuSer.Color = Color.FromArgb(160, 0, 255, 255); // Glowing Cyan
			cpuSer.BorderColor = Color.Cyan;
			cpuSer.BorderWidth = 2;
		}

		public static void DashboardLabels(Label cpuLabel, Label ramLabel)
		{
			// CPU Label
			cpuLabel.BackColor = Color.Transparent;
			cpuLabel.ForeColor = CpuColor;

			// RAM Label
			ramLabel.BackColor = Color.Transparent;
			ramLabel.ForeColor = RamColor;
		}

		public static void SetStatusColor(DataGridView dgv, DataGridViewCellFormattingEventArgs e)
		{
			var column = dgv.Columns[e.ColumnIndex];
			if ((column.Name == "colStatus" || column.DataPropertyName == "Status") && e.Value != null)
			{
				string status = e.Value.ToString().Trim().ToLower();

				// Create a bold version of the current font
				// We use the existing font size and family to keep it consistent
				e.CellStyle.Font = new Font(dgv.DefaultCellStyle.Font, FontStyle.Bold);

				if (status == "online")
				{
					e.CellStyle.ForeColor = Color.LimeGreen;
					e.CellStyle.SelectionForeColor = Color.LimeGreen;
				}
				else if (status == "offline")
				{
					e.CellStyle.ForeColor = Color.LightCoral;
					e.CellStyle.SelectionForeColor = Color.LightCoral;
				}
				else if (status == "installing" || status == "updating")
				{
					e.CellStyle.ForeColor = Color.Gold;
					e.CellStyle.SelectionForeColor = Color.Gold;
				}
			}
		}
	}
}