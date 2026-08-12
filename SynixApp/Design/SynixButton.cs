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
	[ToolboxItem(true)]
	public class SynixButton : Button
	{
		public int BorderRadius { get; set; } = 8;
		public Color BorderColor { get; set; } = Color.FromArgb(0, 80, 150);
		public int BorderSize { get; set; } = 1;

		public Color FillColor { get; set; } = Color.FromArgb(10, 20, 30);
		public Color FillColorSecondary { get; set; } = Color.FromArgb(20, 35, 50);

		public SynixButton()
		{
			this.SetStyle(ControlStyles.UserPaint |
						  ControlStyles.AllPaintingInWmPaint |
						  ControlStyles.OptimizedDoubleBuffer |
						  ControlStyles.SupportsTransparentBackColor, true);

			this.FlatStyle = FlatStyle.Flat;
			this.FlatAppearance.BorderSize = 0;
			this.FlatAppearance.MouseOverBackColor = Color.Transparent;
			this.FlatAppearance.MouseDownBackColor = Color.Transparent;
			this.Size = new Size(130, 40);
			this.BackColor = Color.Transparent;
			this.ForeColor = Color.FromArgb(50, 220, 50);
			this.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			this.Cursor = Cursors.Hand;
		}

		private GraphicsPath GetFigurePath(Rectangle rect, float radius)
		{
			GraphicsPath path = new GraphicsPath();
			float curveSize = radius * 2F;
			path.StartFigure();
			path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
			path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90);
			path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90);
			path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90);
			path.CloseFigure();
			return path;
		}

		protected override void OnPaintBackground(PaintEventArgs pevent) { }

		protected override void OnPaint(PaintEventArgs pevent)
		{
			Graphics g = pevent.Graphics;
			g.SmoothingMode = SmoothingMode.AntiAlias;

			Rectangle rectSurface = this.ClientRectangle;
			Rectangle rectBorder = new Rectangle(rectSurface.X, rectSurface.Y, rectSurface.Width - 1, rectSurface.Height - 1);

			if (Application.RenderWithVisualStyles)
				ButtonRenderer.DrawParentBackground(g, rectSurface, this);
			else if (this.Parent != null)
				using (SolidBrush bgBrush = new SolidBrush(this.Parent.BackColor))
					g.FillRectangle(bgBrush, rectSurface);

			Color currentTopColor = FillColorSecondary;
			Color currentBottomColor = FillColor;
			Point cursorLocation = this.PointToClient(Cursor.Position);

			if (this.ClientRectangle.Contains(cursorLocation))
			{
				currentTopColor = ControlPaint.Light(FillColorSecondary, 0.15f);
				currentBottomColor = ControlPaint.Light(FillColor, 0.15f);
			}

			if (BorderRadius > 2)
			{
				using (GraphicsPath pathSurface = GetFigurePath(rectBorder, BorderRadius))
				using (LinearGradientBrush brushFill = new LinearGradientBrush(rectSurface, currentTopColor, currentBottomColor, 90F))
				using (Pen penBorder = new Pen(BorderColor, BorderSize))
				{
					g.FillPath(brushFill, pathSurface);

					if (BorderSize > 0)
						g.DrawPath(penBorder, pathSurface);
				}
			}

			int textX = 0;
			if (this.Image != null)
			{
				int imgY = (this.Height - this.Image.Height) / 2;
				int imgX = 15;
				g.DrawImage(this.Image, imgX, imgY, this.Image.Width, this.Image.Height);
				textX = imgX + this.Image.Width + 5;
			}

			Rectangle textRect = new Rectangle(textX, 0, this.Width - textX, this.Height);
			TextRenderer.DrawText(g, this.Text, this.Font, textRect, this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
		}
	}
}