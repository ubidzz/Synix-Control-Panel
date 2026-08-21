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
using System.Drawing.Text;

namespace Synix_Control_Panel.SynixApp.Design
{
	public class SynixMenuRenderer : ToolStripProfessionalRenderer
	{
		private static Color BackgroundColor => SettingsPalette.Card;
		private static Color HoverTop => SettingsPalette.Selection;
		private static Color HoverBottom => SettingsPalette.CardHover;
		private static Color AccentBorder => SettingsPalette.Accent;

		public SynixMenuRenderer()
		{
			this.RoundedEdges = false;
		}

		protected override void InitializeItem(ToolStripItem item)
		{
			base.InitializeItem(item);

			item.MouseEnter -= Item_MouseEnter;
			item.MouseEnter += Item_MouseEnter;

			item.MouseLeave -= Item_MouseLeave;
			item.MouseLeave += Item_MouseLeave;
		}

		private void Item_MouseEnter(object sender, EventArgs e)
		{
			if (sender is ToolStripItem item && !(item is ToolStripSeparator))
			{
				ToolStrip parent = item.GetCurrentParent();
				if (parent != null)
				{
					parent.Cursor = Cursors.Hand;
				}
			}
		}

		private void Item_MouseLeave(object sender, EventArgs e)
		{
			if (sender is ToolStripItem item)
			{
				ToolStrip parent = item.GetCurrentParent();
				if (parent != null)
				{
					parent.Cursor = Cursors.Default;
				}
			}
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
			e.Graphics.Clear(BackgroundColor);
		}

		protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
		{

		}

		protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
		{
			using (Pen pen = new Pen(SettingsPalette.Divider, 1))
			{
				int y = e.Item.Height / 2;
				e.Graphics.DrawLine(pen, 10, y, e.Item.Width - 10, y);
			}
		}

		protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
		{
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

			if (e.Item.Selected || e.Item.Pressed)
			{
				Rectangle rect = new Rectangle(4, 2, e.Item.Width - 8, e.Item.Height - 4);
				using (GraphicsPath path = GetRoundedRect(rect, 5))
				using (LinearGradientBrush brushFill = new LinearGradientBrush(rect, HoverTop, HoverBottom, 90F))
				using (Pen penBorder = new Pen(AccentBorder, 1))
				{
					e.Graphics.FillPath(brushFill, path);
					e.Graphics.DrawPath(penBorder, path);
				}
			}
		}

		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

			Rectangle textRect = new Rectangle(12, 0, e.Item.Width - 24, e.Item.Height);
			Color textColor = (e.Item.Selected || e.Item.Pressed)
				? AccentBorder
				: SettingsPalette.PrimaryText;

			TextRenderer.DrawText(e.Graphics, e.Item.Text, e.Item.Font, textRect, textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
		}

		protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
		{
			e.ArrowColor = (e.Item.Selected || e.Item.Pressed)
				? AccentBorder
				: SettingsPalette.PrimaryText;
			base.OnRenderArrow(e);
		}

		protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
		{
			using (Pen borderPen = new Pen(SettingsPalette.Border, 1))
			{
				Rectangle rect = new Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
				e.Graphics.DrawRectangle(borderPen, rect);
			}
		}
	}
}
