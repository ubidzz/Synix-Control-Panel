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
using System.Drawing.Drawing2D;
using System.Windows.Forms.DataVisualization.Charting;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel.SynixApp.Design
{
	public static class GridStyler
	{
		private static Color CpuCyan = Color.FromArgb(160, 0, 255, 255);
		private static Color RamPurple = Color.FromArgb(80, 150, 0, 200);
		private static Color PlotBg = Color.FromArgb(15, 15, 15);
		private static Color GridLineColor = Color.FromArgb(40, 40, 40);
		private static Color RowDarkGrey = Color.FromArgb(30, 30, 30);
		private static Color HeaderGrey = Color.FromArgb(35, 35, 35);
		private static Color BackgroundBlack = Color.FromArgb(15, 15, 15);
		private static Font _boldStatusFont = null;
		private static readonly SolidBrush _rowDarkGreyBrush = new SolidBrush(RowDarkGrey);
		private static readonly Pen _faintDividerPen = new Pen(Color.FromArgb(45, 45, 45));

		public static void DarkTheme(DataGridView dgv)
		{
			dgv.AutoGenerateColumns = false;

			if (dgv.Columns.Contains("colIcon")) dgv.Columns["colIcon"].DataPropertyName = "";
			if (dgv.Columns.Contains("colName")) dgv.Columns["colName"].DataPropertyName = "ServerName";
			if (dgv.Columns.Contains("colGame")) dgv.Columns["colGame"].DataPropertyName = "Game";
			if (dgv.Columns.Contains("colPort")) dgv.Columns["colPort"].DataPropertyName = "Port";
			if (dgv.Columns.Contains("colStatus")) dgv.Columns["colStatus"].DataPropertyName = "Status";
			if (dgv.Columns.Contains("colPlayerCount")) dgv.Columns["colPlayerCount"].DataPropertyName = "PlayerCount";
			if (dgv.Columns.Contains("colUptime")) dgv.Columns["colUptime"].DataPropertyName = "Uptime";

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

		public static void StyleMinimizeButton(Button btn)
		{
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderSize = 0;
			btn.BackColor = Color.Transparent;
			btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
			btn.FlatAppearance.MouseDownBackColor = Color.Transparent;
			btn.Text = "";
			btn.TabStop = false;
			btn.Paint += (s, e) =>
			{
				Button b = (Button)s;
				e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

				Point mousePos = b.PointToClient(System.Windows.Forms.Cursor.Position);
				bool isHovering = b.ClientRectangle.Contains(mousePos);
				bool isPressed = isHovering && (Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left;

				Color bgColor = Color.WhiteSmoke;
				Color fgColor = Color.Black;

				if (isPressed)
				{
					bgColor = Color.FromArgb(160, 160, 160);
				}
				else if (isHovering)
				{
					bgColor = Color.FromArgb(200, 200, 200);
				}

				using (var path = GetRoundedPath(b.ClientRectangle, 6))
				using (var brush = new SolidBrush(bgColor))
				{
					e.Graphics.FillPath(brush, path);
				}

				int lineWidth = 12;
				int lineThickness = 2;
				int xPos = (b.Width / 2) - (lineWidth / 2);
				int yPos = (b.Height / 2) - (lineThickness / 2) + 2;

				using (SolidBrush lineBrush = new SolidBrush(fgColor))
				{
					e.Graphics.FillRectangle(lineBrush, xPos, yPos, lineWidth, lineThickness);
				}
			};

			btn.MouseEnter += (s, e) => btn.Invalidate();
			btn.MouseLeave += (s, e) => btn.Invalidate();
			btn.MouseDown += (s, e) => btn.Invalidate();
			btn.MouseUp += (s, e) => btn.Invalidate();
		}

		public static void StyleIconButton(Button btn, Image icon, Color hoverColor)
		{
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderSize = 0;
			btn.BackColor = Color.Transparent;
			btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
			btn.FlatAppearance.MouseDownBackColor = Color.Transparent;
			btn.Text = "";
			btn.TabStop = false;

			btn.Paint += (s, e) =>
			{
				Button b = (Button)s;
				e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

				e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

				Point mousePos = b.PointToClient(System.Windows.Forms.Cursor.Position);
				bool isHovering = b.ClientRectangle.Contains(mousePos);
				bool isPressed = isHovering && (Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left;

				Color bgColor = Color.WhiteSmoke;

				if (isPressed)
				{
					bgColor = Color.DarkGray;
				}
				else if (isHovering)
				{
					bgColor = hoverColor;
				}

				using (var path = GetRoundedPath(b.ClientRectangle, 6))
				using (var brush = new SolidBrush(bgColor))
				{
					e.Graphics.FillPath(brush, path);
				}

				if (icon != null)
				{
					int iconSize = Math.Min(b.Width, b.Height) - 8;
					int x = (b.Width - iconSize) / 2;
					int y = (b.Height - iconSize) / 2;

					e.Graphics.DrawImage(icon, new Rectangle(x, y, iconSize, iconSize));
				}
			};

			btn.MouseEnter += (s, e) => btn.Invalidate();
			btn.MouseLeave += (s, e) => btn.Invalidate();
			btn.MouseDown += (s, e) => btn.Invalidate();
			btn.MouseUp += (s, e) => btn.Invalidate();
		}

		public static void StyleCloseButton(Button btn)
		{
			btn.FlatStyle = FlatStyle.Flat;
			btn.FlatAppearance.BorderSize = 0;
			btn.BackColor = Color.Transparent;
			btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
			btn.FlatAppearance.MouseDownBackColor = Color.Transparent;
			btn.Text = "";
			btn.TabStop = false;
			btn.Paint += (s, e) =>
			{
				Button b = (Button)s;

				e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

				Point mousePos = b.PointToClient(System.Windows.Forms.Cursor.Position);
				bool isHovering = b.ClientRectangle.Contains(mousePos);
				bool isPressed = isHovering && (Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left;

				Color bgColor = Color.WhiteSmoke;
				Color fgColor = Color.Black;

				if (isPressed)
				{
					bgColor = Color.FromArgb(178, 11, 22);
					fgColor = Color.White;
				}
				else if (isHovering)
				{
					bgColor = Color.FromArgb(232, 17, 35);
					fgColor = Color.White;
				}

				using (var path = GetRoundedPath(b.ClientRectangle, 6))
				using (var brush = new SolidBrush(bgColor))
				{
					e.Graphics.FillPath(brush, path);
				}

				TextRenderer.DrawText(
					e.Graphics,
					"✕",
					new Font("Segoe UI", 10, FontStyle.Bold),
					b.ClientRectangle,
					fgColor,
					TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
				);
			};

			btn.MouseEnter += (s, e) => btn.Invalidate();
			btn.MouseLeave += (s, e) => btn.Invalidate();
			btn.MouseDown += (s, e) => btn.Invalidate();
			btn.MouseUp += (s, e) => btn.Invalidate();
		}

		private static System.Drawing.Drawing2D.GraphicsPath GetRoundedPath(Rectangle rect, int radius)
		{
			System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
			int d = radius * 2;
			path.StartFigure();
			path.AddArc(rect.X, rect.Y, d, d, 180, 90);
			path.AddArc(rect.Width - d - 1, rect.Y, d, d, 270, 90);
			path.AddArc(rect.Width - d - 1, rect.Height - d - 1, d, d, 0, 90);
			path.AddArc(rect.X, rect.Height - d - 1, d, d, 90, 90);
			path.CloseFigure();
			return path;
		}

		public static void ApplyRoundedCorners(DataGridView dgv, int radius)
		{
			UpdateGridRegion(dgv, radius);
			dgv.Resize += (s, e) => UpdateGridRegion(dgv, radius);
		}

		private static void UpdateGridRegion(DataGridView dgv, int radius)
		{
			if (dgv == null || dgv.Width == 0 || dgv.Height == 0) return;

			int diameter = radius * 2;
			GraphicsPath path = new GraphicsPath();

			path.StartFigure();
			path.AddArc(new Rectangle(0, 0, diameter, diameter), 180, 90);
			path.AddArc(new Rectangle(dgv.Width - diameter, 0, diameter, diameter), 270, 90);
			path.AddArc(new Rectangle(dgv.Width - diameter, dgv.Height - diameter, diameter, diameter), 0, 90);
			path.AddArc(new Rectangle(0, dgv.Height - diameter, diameter, diameter), 90, 90);
			path.CloseFigure();

			Region oldRegion = dgv.Region;
			dgv.Region = new Region(path);
			oldRegion?.Dispose();
		}

		public static void ApplyTransparentTheme(DataGridView dgv)
		{
			dgv.RowHeadersVisible = false;
			dgv.BackgroundColor = BackgroundBlack;
			dgv.BorderStyle = BorderStyle.None;
			dgv.DefaultCellStyle.BackColor = RowDarkGrey;
			dgv.DefaultCellStyle.ForeColor = Color.WhiteSmoke;
			dgv.DefaultCellStyle.SelectionBackColor = RowDarkGrey;
			dgv.DefaultCellStyle.SelectionForeColor = Color.Cyan;
			dgv.GridColor = Color.FromArgb(45, 45, 45);
			dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dgv.RowPostPaint -= Dgv_PaintGlowingSelection;
			dgv.RowPostPaint += Dgv_PaintGlowingSelection;
		}

		private static void Dgv_PaintGlowingSelection(object sender, DataGridViewRowPostPaintEventArgs e)
		{
			DataGridView dgv = sender as DataGridView;
			if (dgv == null) return;

			if ((e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected)
			{
				int startX = dgv.RowHeadersVisible ? dgv.RowHeadersWidth : 0;
				int width = dgv.Columns.GetColumnsWidth(DataGridViewElementStates.Visible) - dgv.HorizontalScrollingOffset;

				Rectangle bounds = new Rectangle(startX + 2, e.RowBounds.Y + 2, width - 5, e.RowBounds.Height - 5);

				Color neonColor = Color.DarkCyan;

				using (Pen outerGlow = new Pen(Color.FromArgb(40, neonColor), 5))
				{
					e.Graphics.DrawRectangle(outerGlow, bounds);
				}

				using (Pen innerGlow = new Pen(Color.FromArgb(100, neonColor), 3))
				{
					e.Graphics.DrawRectangle(innerGlow, bounds);
				}

				using (Pen corePen = new Pen(Color.White, 1))
				{
					e.Graphics.DrawRectangle(corePen, bounds);
				}
			}
		}

		public static void PaintTransparentRows(DataGridView dgv, DataGridViewCellPaintingEventArgs e)
		{
			if (e.RowIndex < 0) return;

			e.Graphics.FillRectangle(_rowDarkGreyBrush, e.CellBounds);

			e.Graphics.DrawLine(_faintDividerPen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);

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
			HeartbeatChart(chart, 128.0);
		}

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

		public static void SetStatusColor(DataGridView dgv, DataGridViewCellFormattingEventArgs e)
		{
			if (e.ColumnIndex < 0 || e.ColumnIndex >= dgv.Columns.Count) return;

			var column = dgv.Columns[e.ColumnIndex];
			if ((column.Name == "colStatus" || column.DataPropertyName == "Status") && e.Value != null)
			{
				string status = e.Value.ToString().Trim();

				if (_boldStatusFont == null || _boldStatusFont.FontFamily.Name != dgv.DefaultCellStyle.Font.Name)
				{
					_boldStatusFont?.Dispose();
					_boldStatusFont = new Font(dgv.DefaultCellStyle.Font, FontStyle.Bold);
				}

				e.CellStyle.Font = _boldStatusFont;

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
