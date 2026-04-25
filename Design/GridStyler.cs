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
using System.Windows.Forms.DataVisualization.Charting;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel.Design
{
	public static class GridStyler
	{
		private static Color CpuCyan = Color.FromArgb(160, 0, 255, 255);
		private static Color RamPurple = Color.FromArgb(80, 150, 0, 200);
		private static Color PlotBg = Color.FromArgb(15, 15, 15);
		private static Color GridLineColor = Color.FromArgb(40, 40, 40);

		// Colors for a solid, no-blue theme
		private static Color RowDarkGrey = Color.FromArgb(30, 30, 30);
		private static Color HeaderGrey = Color.FromArgb(35, 35, 35);
		private static Color BackgroundBlack = Color.FromArgb(15, 15, 15);

		public static void DarkTheme(DataGridView dgv)
		{
			dgv.AutoGenerateColumns = false;

			// Map Columns to Class Properties
			if (dgv.Columns.Contains("colName")) dgv.Columns["colName"].DataPropertyName = "ServerName";
			if (dgv.Columns.Contains("colGame")) dgv.Columns["colGame"].DataPropertyName = "Game";
			if (dgv.Columns.Contains("colPort")) dgv.Columns["colPort"].DataPropertyName = "Port";
			if (dgv.Columns.Contains("colStatus")) dgv.Columns["colStatus"].DataPropertyName = "Status";
			dgv.Columns["PlayerCountDisplay"].DefaultCellStyle.ForeColor = Color.Cyan;

			// Header Style (Kills the blue Game column)
			dgv.EnableHeadersVisualStyles = false;
			dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
			dgv.ColumnHeadersDefaultCellStyle.BackColor = HeaderGrey;
			dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Cyan;
			dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = HeaderGrey;
			dgv.ColumnHeadersHeight = 40;
			dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

			foreach (DataGridViewColumn col in dgv.Columns)
			{
				col.HeaderCell.Style.BackColor = HeaderGrey;
				col.HeaderCell.Style.ForeColor = Color.Cyan;
			}
		}

		public static void ApplyTransparentTheme(DataGridView dgv)
		{
			dgv.BackgroundColor = BackgroundBlack;
			dgv.BorderStyle = BorderStyle.None;

			// Selection Fix: Matches row color so it doesn't turn blue on click
			dgv.DefaultCellStyle.BackColor = RowDarkGrey;
			dgv.DefaultCellStyle.ForeColor = Color.WhiteSmoke;
			dgv.DefaultCellStyle.SelectionBackColor = RowDarkGrey;
			dgv.DefaultCellStyle.SelectionForeColor = Color.WhiteSmoke;

			dgv.GridColor = Color.FromArgb(45, 45, 45);
			dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
		}

		public static void PaintTransparentRows(DataGridView dgv, DataGridViewCellPaintingEventArgs e)
		{
			if (e.RowIndex < 0) return;

			// Draw Solid Row
			using (SolidBrush br = new SolidBrush(RowDarkGrey))
			{
				e.Graphics.FillRectangle(br, e.CellBounds);
			}

			// Faint divider
			using (Pen p = new Pen(Color.FromArgb(45, 45, 45)))
			{
				e.Graphics.DrawLine(p, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
			}

			e.PaintContent(e.CellBounds);
			e.Handled = true;
		}

		// --- CHART METHODS (Fixes CS7036) ---
		public static void HeartbeatChart(Chart chart, double maxRamGb)
		{
			if (chart == null) return;
			chart.Series.Clear();
			chart.ChartAreas.Clear();
			chart.Legends.Clear();
			chart.BackColor = Color.Transparent;
			chart.AntiAliasing = AntiAliasingStyles.All;

			ChartArea ca = chart.ChartAreas.Add("Default");
			ca.BackColor = PlotBg;
			ca.AxisY.Minimum = 0;
			ca.AxisY.Maximum = 100;
			ca.AxisY.LabelStyle.Enabled = false;
			ca.AxisY.MajorGrid.Enabled = true;
			ca.AxisY.MajorGrid.LineColor = GridLineColor;

			ca.AxisY2.Enabled = AxisEnabled.True;
			ca.AxisY2.Minimum = 0;
			ca.AxisY2.Maximum = (maxRamGb > 0) ? maxRamGb : 98.0;
			ca.AxisY2.LabelStyle.Enabled = false;
			ca.AxisY2.MajorGrid.Enabled = false;

			ca.AxisX.LabelStyle.Enabled = false;
			ca.AxisX.MajorGrid.Enabled = false;

			Series ramSer = chart.Series.Add("TotalRAM");
			ramSer.ChartType = SeriesChartType.SplineArea;
			ramSer.YAxisType = AxisType.Secondary;
			ramSer.Color = RamPurple;
			ramSer.BorderColor = Color.MediumPurple;
			ramSer.BorderWidth = 1;

			Series cpuSer = chart.Series.Add("TotalCPU");
			cpuSer.ChartType = SeriesChartType.SplineArea;
			cpuSer.YAxisType = AxisType.Primary;
			cpuSer.Color = CpuCyan;
			cpuSer.BorderColor = Color.Cyan;
			cpuSer.BorderWidth = 2;
		}

		public static void HeartbeatChart(Chart chart)
		{
			HeartbeatChart(chart, 98.0);
		}

		// --- LABEL METHODS (Fixes CS0117) ---
		public static void DashboardLabels(Label cpuLabel, Label ramLabel)
		{
			if (cpuLabel != null)
			{
				cpuLabel.ForeColor = Color.Cyan;
				cpuLabel.BackColor = Color.Transparent;
			}
			if (ramLabel != null)
			{
				ramLabel.ForeColor = Color.MediumPurple;
				ramLabel.BackColor = Color.Transparent;
			}
		}

		// --- STATUS COLOR METHOD (Fixes CS0117) ---
		public static void SetStatusColor(DataGridView dgv, DataGridViewCellFormattingEventArgs e)
		{
			if (e.ColumnIndex < 0 || e.ColumnIndex >= dgv.Columns.Count) return;

			var column = dgv.Columns[e.ColumnIndex];
			if ((column.Name == "colStatus" || column.DataPropertyName == "Status") && e.Value != null)
			{
				// 1. Notice there is NO .ToLower() here anymore!
				string status = e.Value.ToString().Trim();
				e.CellStyle.Font = new Font(dgv.DefaultCellStyle.Font, FontStyle.Bold);

				// 2. string.Equals with OrdinalIgnoreCase ignores capitals completely
				if (string.Equals(status, StatusManager.GetStatus(ServerState.Running), StringComparison.OrdinalIgnoreCase))
				{
					e.CellStyle.ForeColor = Color.LimeGreen;
					e.CellStyle.SelectionForeColor = Color.LimeGreen;
				}
				else if (string.Equals(status, StatusManager.GetStatus(ServerState.Stopped), StringComparison.OrdinalIgnoreCase))
				{
					e.CellStyle.ForeColor = Color.LightCoral;
					e.CellStyle.SelectionForeColor = Color.LightCoral;
				}
				else if (string.Equals(status, StatusManager.GetStatus(ServerState.Installing), StringComparison.OrdinalIgnoreCase) ||
						 string.Equals(status, StatusManager.GetStatus(ServerState.Updating), StringComparison.OrdinalIgnoreCase))
				{
					e.CellStyle.ForeColor = Color.Gold;
					e.CellStyle.SelectionForeColor = Color.Gold;
				}
				else if (string.Equals(status, StatusManager.GetStatus(ServerState.Starting), StringComparison.OrdinalIgnoreCase))
				{
					e.CellStyle.ForeColor = Color.Orange;
					e.CellStyle.SelectionForeColor = Color.Orange;
				}
				else if (string.Equals(status, StatusManager.GetStatus(ServerState.Stopping), StringComparison.OrdinalIgnoreCase))
				{
					e.CellStyle.ForeColor = Color.Yellow;
					e.CellStyle.SelectionForeColor = Color.Yellow;
				}
				else if (string.Equals(status, StatusManager.GetStatus(ServerState.Crashed), StringComparison.OrdinalIgnoreCase))
				{
					e.CellStyle.ForeColor = Color.Red;
					e.CellStyle.SelectionForeColor = Color.Red;
				}
			}
		}
	}
}