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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Synix_Control_Panel.SynixApp.Design
{
	public static class UIStyleHelper
	{
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
				if (ctrl.HasChildren)
				{
					InitializeToggles(ctrl);
				}
			}
		}

		public static void WarningLabel_Paint(object sender, PaintEventArgs e)
		{
			Label lbl = (Label)sender;
			if (lbl.Width <= 0 || lbl.Height <= 0) return;

			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

			if (Application.RenderWithVisualStyles)
			{
				ButtonRenderer.DrawParentBackground(e.Graphics, lbl.ClientRectangle, lbl);
			}
			else if (lbl.Parent != null)
			{
				using (SolidBrush bgBrush = new SolidBrush(lbl.Parent.BackColor))
				{
					e.Graphics.FillRectangle(bgBrush, lbl.ClientRectangle);
				}
			}

			int radius = 15;
			using (GraphicsPath path = new GraphicsPath())
			{
				path.AddArc(0, 0, radius, radius, 180, 90);
				path.AddArc(lbl.Width - radius - 1, 0, radius, radius, 270, 90);
				path.AddArc(lbl.Width - radius - 1, lbl.Height - radius - 1, radius, radius, 0, 90);
				path.AddArc(0, lbl.Height - radius - 1, radius, radius, 90, 90);
				path.CloseFigure();

				using (SolidBrush brush = new SolidBrush(lbl.BackColor))
				{
					e.Graphics.FillPath(brush, path);
				}
			}

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

			TextRenderer.DrawText(e.Graphics, lbl.Text, lbl.Font, lbl.ClientRectangle, lbl.ForeColor, flags);
		}

		public static void StyleWarningLabel(Label lbl, string alignment = "MiddleCenter")
		{
			if (lbl == null) return;

			lbl.AutoSize = false;
			lbl.FlatStyle = FlatStyle.Flat;
			lbl.BorderStyle = BorderStyle.None;
			lbl.Tag = alignment;
			lbl.Region = null;

			lbl.Paint -= WarningLabel_Paint;
			lbl.Paint += WarningLabel_Paint;

			lbl.Invalidate();
		}
	}

	public class SynixToggle : CheckBox
	{
		public SynixToggle()
		{
			this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			e.Graphics.Clear(this.Parent?.BackColor ?? Color.FromArgb(15, 15, 15));

			Rectangle rect = new Rectangle(0, (this.Height - 20) / 2, 40, 20);
			Color bgCol = this.Checked ? Color.FromArgb(0, 190, 255) : Color.FromArgb(45, 45, 45);
			Color thumbCol = Color.White;

			using (GraphicsPath path = GetRoundedPathInternal(rect, 10))
			using (SolidBrush brush = new SolidBrush(bgCol))
			{
				e.Graphics.FillPath(brush, path);
			}

			int thumbX = this.Checked ? rect.Right - 18 : rect.X + 2;
			Rectangle thumbRect = new Rectangle(thumbX, rect.Y + 2, 16, 16);

			using (GraphicsPath thumbPath = GetRoundedPathInternal(thumbRect, 8))
			using (SolidBrush thumbBrush = new SolidBrush(thumbCol))
			{
				e.Graphics.FillPath(thumbBrush, thumbPath);
			}

			if (!string.IsNullOrEmpty(this.Text))
			{
				Rectangle textRect = new Rectangle(rect.Right + 8, 0, this.Width - rect.Right - 8, this.Height);
				TextRenderer.DrawText(e.Graphics, this.Text, this.Font, textRect, this.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
			}
		}

		private GraphicsPath GetRoundedPathInternal(Rectangle rect, int radius)
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
	}
}