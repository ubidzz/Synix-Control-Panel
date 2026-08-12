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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Synix_Control_Panel.SynixApp.Design
{
	public class SynixMenuRenderer : ToolStripProfessionalRenderer
	{
		private readonly Color bgColor = Color.FromArgb(25, 25, 30); // Slightly darker base for better contrast
		private readonly Color hoverTop = Color.FromArgb(20, 35, 50);
		private readonly Color hoverBottom = Color.FromArgb(10, 20, 30);
		private readonly Color cyanBorder = Color.FromArgb(0, 190, 255);

		public SynixMenuRenderer()
		{
			this.RoundedEdges = false;
		}

		private GraphicsPath GetRoundedRect(Rectangle rect, float radius)
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

		protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
		{
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			// Just clear the background with the dark color, no jagged Region cuts!
			e.Graphics.Clear(bgColor);
		}

		// 2. THE ARTIFACT FIX: Force Windows to completely ignore the image gutter
		protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
		{
			// Leave this entirely blank! This kills those broken grey boxes on the left.
		}

		// 3. THE SEPARATOR FIX: Clean, simple center lines
		protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
		{
			using (Pen pen = new Pen(Color.FromArgb(50, 50, 60), 1))
			{
				int y = e.Item.Height / 2;
				// Add 10px of padding to the left and right so the line doesn't touch the walls
				e.Graphics.DrawLine(pen, 10, y, e.Item.Width - 10, y);
			}
		}

		// 4. THE HOVER EFFECT
		protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
		{
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

			if (e.Item.Selected || e.Item.Pressed)
			{
				Rectangle rect = new Rectangle(4, 2, e.Item.Width - 8, e.Item.Height - 4);
				using (GraphicsPath path = GetRoundedRect(rect, 5))
				using (LinearGradientBrush brushFill = new LinearGradientBrush(rect, hoverTop, hoverBottom, 90F))
				using (Pen penBorder = new Pen(cyanBorder, 1))
				{
					e.Graphics.FillPath(brushFill, path);
					e.Graphics.DrawPath(penBorder, path);
				}
			}
		}

		// 5. PERFECT TEXT
		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

			// THE FIX: Ignore the padded TextRectangle. 
			// Force the text to use the absolute full height (e.Item.Height) so it perfectly centers.
			// Starts 12 pixels from the left so it has nice breathing room.
			Rectangle textRect = new Rectangle(12, 0, e.Item.Width - 24, e.Item.Height);

			Color textColor = (e.Item.Selected || e.Item.Pressed) ? cyanBorder : Color.White;

			TextRenderer.DrawText(e.Graphics, e.Item.Text, e.Item.Font, textRect, textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
		}

		// 6. DYNAMIC SUBMENU ARROWS
		protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
		{
			// Make the arrow glow cyan when hovered to match the text!
			e.ArrowColor = (e.Item.Selected || e.Item.Pressed) ? cyanBorder : Color.White;
			base.OnRenderArrow(e);
		}

		// 7. REMOVE OUTER BORDER
		protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
		{
			// A crisp, dark border to frame the menu perfectly
			using (Pen borderPen = new Pen(Color.FromArgb(15, 15, 18), 1))
			{
				Rectangle rect = new Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
				e.Graphics.DrawRectangle(borderPen, rect);
			}
		}
	}
}