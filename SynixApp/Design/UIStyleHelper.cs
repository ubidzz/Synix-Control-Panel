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
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Synix_Control_Panel.SynixApp.Design
{
	public static class UIStyleHelper
	{
		private static readonly Font _sliderFont = new Font("Segoe UI", 8F, FontStyle.Bold);

		public static void StyleLogBox(RichTextBox rtb)
		{
			if (rtb == null) return;

			rtb.BorderStyle = BorderStyle.None;
			rtb.BackColor = Color.FromArgb(15, 15, 15);
			rtb.ForeColor = Color.WhiteSmoke;
			rtb.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

			Control parent = rtb.Parent;
			if (parent != null)
			{
				Panel container;

				if (parent is Panel existingPanel)
				{
					container = existingPanel;
				}
				else
				{
					container = new Panel();
					container.Bounds = rtb.Bounds;
					container.Anchor = rtb.Anchor;
					container.Dock = rtb.Dock;

					parent.Controls.Add(container);
					parent.Controls.Remove(rtb);
					container.Controls.Add(rtb);
				}

				container.BackColor = Color.FromArgb(15, 15, 15);

				int margin = 5;
				rtb.Dock = DockStyle.None;
				rtb.Location = new Point(margin, margin);
				rtb.Width = Math.Max(10, container.Width - (margin * 2));
				rtb.Height = Math.Max(10, container.Height - (margin * 2));
				rtb.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

				UpdateLogRegion(container, 8);

				container.Resize -= LogContainer_Resize;
				container.Resize += LogContainer_Resize;

				container.Paint -= LogContainer_Paint;
				container.Paint += LogContainer_Paint;
				container.Invalidate();
			}
		}

		private static void LogContainer_Resize(object sender, EventArgs e)
		{
			if (sender is Panel container)
			{
				UpdateLogRegion(container, 8);
				int margin = 5;
				foreach (Control ctrl in container.Controls)
				{
					if (ctrl is RichTextBox rtb)
					{
						rtb.Location = new Point(margin, margin);
						rtb.Width = Math.Max(10, container.Width - (margin * 2));
						rtb.Height = Math.Max(10, container.Height - (margin * 2));
					}
				}
				container.Invalidate();
			}
		}

		private static void UpdateLogRegion(Panel container, int radius)
		{
			if (container == null || container.Width == 0 || container.Height == 0) return;

			int d = radius * 2;
			using (GraphicsPath path = new GraphicsPath())
			{
				path.StartFigure();
				path.AddArc(new Rectangle(0, 0, d, d), 180, 90);
				path.AddArc(new Rectangle(container.Width - d, 0, d, d), 270, 90);
				path.AddArc(new Rectangle(container.Width - d, container.Height - d, d, d), 0, 90);
				path.AddArc(new Rectangle(0, container.Height - d, d, d), 90, 90);
				path.CloseFigure();

				Region oldRegion = container.Region;
				container.Region = new Region(path);
				oldRegion?.Dispose();
			}
		}

		private static void LogContainer_Paint(object sender, PaintEventArgs e)
		{
			Panel p = sender as Panel;
			if (p == null || p.Width <= 0 || p.Height <= 0) return;

			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			int radius = 8;

			using (GraphicsPath outline = GetRoundedPath(new Rectangle(0, 0, p.Width - 1, p.Height - 1), radius))
			using (Pen cyanPen = new Pen(Color.FromArgb(0, 190, 255), 1f))
			{
				e.Graphics.DrawPath(cyanPen, outline);
			}
		}

		private static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
		{
			GraphicsPath path = new GraphicsPath();
			int d = radius * 2;
			path.StartFigure();
			path.AddArc(rect.X, rect.Y, d, d, 180, 90);
			path.AddArc(rect.Width - d - 1, rect.Y, d, d, 270, 90);
			path.AddArc(rect.Width - d - 1, rect.Height - d - 1, d, d, 0, 90);
			path.AddArc(rect.X, rect.Height - d - 1, d, d, 90, 90);
			path.CloseFigure();
			return path;
		}

		public static void InitializeToggles(Control parent)
		{
			foreach (Control ctrl in parent.Controls)
			{
				if (ctrl is CheckBox chk && chk.Name.StartsWith("chk", StringComparison.OrdinalIgnoreCase))
				{
					chk.Paint -= Chk_CustomPaint;
					StyleToggleButton(chk, chk.Tag?.ToString() ?? "");
				}
				if (ctrl.HasChildren) InitializeToggles(ctrl);
			}
		}

		public static void StyleToggleButton(CheckBox chk, string labelPrefix)
		{
			chk.Cursor = Cursors.Hand;
			chk.AutoSize = false;
			chk.BackColor = Color.Transparent;
			chk.Tag = labelPrefix;
			chk.Paint -= Chk_CustomPaint;
			chk.Paint += Chk_CustomPaint;

			chk.Invalidate();
		}

		private static void Chk_CustomPaint(object sender, PaintEventArgs e)
		{
			if (sender is CheckBox chk)
			{
				string labelPrefix = chk.Tag?.ToString() ?? "";
				DrawRoundedSlider(e.Graphics, chk, labelPrefix);
			}
		}

		public static void DrawRoundedSlider(Graphics g, CheckBox chk, string label)
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;

			if (Application.RenderWithVisualStyles)
				ButtonRenderer.DrawParentBackground(g, chk.ClientRectangle, chk);
			else
			{
				using (var b = new SolidBrush(chk.Parent?.BackColor ?? Color.FromArgb(32, 32, 32)))
					g.FillRectangle(b, chk.ClientRectangle);
			}

			Rectangle rect = new Rectangle(2, 2, chk.Width - 6, chk.Height - 6);
			int diameter = rect.Height;

			using (GraphicsPath path = new GraphicsPath())
			{
				path.AddArc(rect.X, rect.Y, diameter, diameter, 90, 180);
				path.AddArc(rect.Width - diameter + rect.X, rect.Y, diameter, diameter, 270, 180);
				path.CloseFigure();

				// Changed from cyan to green when checked
				Color trackColor = chk.Checked ? Color.FromArgb(40, 150, 40) : Color.FromArgb(45, 45, 45);
				using (var brush = new SolidBrush(trackColor))
				{
					g.FillPath(brush, path);
				}

				using (var pen = new Pen(Color.FromArgb(30, 30, 30), 2.2f))
				{
					g.DrawPath(pen, path);
				}

				float thumbSize = rect.Height - 8;
				float xPos = chk.Checked ? (rect.Right - thumbSize - 4) : (rect.Left + 4);
				g.FillEllipse(Brushes.White, xPos, rect.Y + 4, thumbSize, thumbSize);

				string text = !string.IsNullOrEmpty(chk.Text) ? chk.Text : (string.IsNullOrEmpty(label) ? (chk.Checked ? "ON" : "OFF") : label);

				Rectangle textRect = chk.Checked ? new Rectangle(rect.X, rect.Y, rect.Width - 22, rect.Height)
											   : new Rectangle(rect.X + 22, rect.Y, rect.Width - 22, rect.Height);

				TextRenderer.DrawText(g, text, _sliderFont, textRect, Color.White,
					TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
			}
		}

		public static void WarningLabel_Paint(object sender, PaintEventArgs e)
		{
			if (sender is not Label lbl || lbl.Width <= 1 || lbl.Height <= 1)
				return;

			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			e.Graphics.Clear(lbl.Parent?.BackColor ?? SettingsPalette.Window);

			Rectangle bannerBounds = new(0, 0, lbl.ClientSize.Width - 1, lbl.ClientSize.Height - 1);
			using GraphicsPath path = RoundedGeometry.Create(bannerBounds, 12);
			using SolidBrush brush = new(lbl.BackColor);
			e.Graphics.FillPath(brush, path);

			TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak;
			string align = lbl.Tag?.ToString() ?? "MiddleCenter";

			if (align == "MiddleRight")
			{
				flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Right | TextFormatFlags.WordBreak;
			}
			else if (align == "MiddleLeft")
			{
				flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.WordBreak;
			}
			else if (align == "TopCenter")
			{
				flags = TextFormatFlags.Top | TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak;
			}
			else if (align == "TopLeft")
			{
				flags = TextFormatFlags.Top | TextFormatFlags.Left | TextFormatFlags.WordBreak;
			}

			flags |= TextFormatFlags.NoPrefix;
			Rectangle textBounds = new(
				lbl.Padding.Left,
				lbl.Padding.Top,
				Math.Max(0, lbl.ClientSize.Width - lbl.Padding.Horizontal),
				Math.Max(0, lbl.ClientSize.Height - lbl.Padding.Vertical));
			TextRenderer.DrawText(e.Graphics, lbl.Text, lbl.Font, textBounds, lbl.ForeColor, flags);
		}

		public static void RefreshWarningLabelShape(Label lbl, int cornerRadius = 12)
		{
			if (lbl == null || lbl.IsDisposed || lbl.Width <= 1 || lbl.Height <= 1)
				return;

			Rectangle bounds = new(0, 0, lbl.ClientSize.Width - 1, lbl.ClientSize.Height - 1);
			using GraphicsPath path = RoundedGeometry.Create(bounds, cornerRadius);
			Region? oldRegion = lbl.Region;
			lbl.Region = new Region(path);
			oldRegion?.Dispose();
			lbl.Invalidate();
		}

		private static void WarningLabel_ShapeChanged(object? sender, EventArgs e)
		{
			if (sender is Label lbl)
				RefreshWarningLabelShape(lbl);
		}

		public static void StyleWarningLabel(Label lbl, string alignment = "MiddleCenter")
		{
			if (lbl == null) return;

			lbl.AutoSize = false;
			lbl.FlatStyle = FlatStyle.Flat;
			lbl.Tag = alignment;

			lbl.Paint -= WarningLabel_Paint;
			lbl.Paint += WarningLabel_Paint;
			lbl.SizeChanged -= WarningLabel_ShapeChanged;
			lbl.SizeChanged += WarningLabel_ShapeChanged;
			lbl.VisibleChanged -= WarningLabel_ShapeChanged;
			lbl.VisibleChanged += WarningLabel_ShapeChanged;
			lbl.BackColorChanged -= WarningLabel_ShapeChanged;
			lbl.BackColorChanged += WarningLabel_ShapeChanged;

			RefreshWarningLabelShape(lbl);
		}
	}

	[ToolboxItem(true)]
	public class SynixToggle : CheckBox
	{
		public SynixToggle()
		{
			this.SetStyle(ControlStyles.UserPaint |
						  ControlStyles.AllPaintingInWmPaint |
						  ControlStyles.OptimizedDoubleBuffer |
						  ControlStyles.SupportsTransparentBackColor, true);

			this.BackColor = Color.Transparent;
			this.Size = new Size(60, 28);
			this.Cursor = Cursors.Hand;
		}

		protected override void OnPaintBackground(PaintEventArgs pevent) { /* Handled in OnPaint */ }

		protected override void OnPaint(PaintEventArgs e)
		{
			UIStyleHelper.DrawRoundedSlider(e.Graphics, this, this.Tag?.ToString() ?? "");
		}
	}
}
