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
using System.ComponentModel;
using System.Drawing.Drawing2D;
using Synix_Control_Panel.SynixApp.Design;

namespace Synix_Control_Panel.SynixApp.Design.Controls
{
	internal static class ModernComboBoxRenderer
	{
		public static void DrawArrowButton(
			Graphics graphics,
			Rectangle bounds,
			Color backgroundColor,
			Color borderColor,
			Color arrowColor,
			bool drawOuterBorder)
		{
			if (bounds.Width <= 2 || bounds.Height <= 2)
			{
				return;
			}

			int buttonWidth = Math.Min(
				Math.Max(28, Math.Min(bounds.Height, 34)),
				Math.Max(1, bounds.Width / 2));
			Rectangle buttonBounds = new(
				bounds.Right - buttonWidth,
				bounds.Top + 1,
				buttonWidth - 1,
				bounds.Height - 2);

			using SolidBrush backgroundBrush = new(backgroundColor);
			graphics.FillRectangle(backgroundBrush, buttonBounds);

			using Pen dividerPen = new(borderColor, 1F);
			graphics.DrawLine(
				dividerPen,
				buttonBounds.Left,
				buttonBounds.Top + 4,
				buttonBounds.Left,
				buttonBounds.Bottom - 4);

			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			int centerX = buttonBounds.Left + buttonBounds.Width / 2;
			int centerY = buttonBounds.Top + buttonBounds.Height / 2;
			using Pen arrowPen = new(arrowColor, 1.8F)
			{
				StartCap = LineCap.Round,
				EndCap = LineCap.Round,
				LineJoin = LineJoin.Round
			};
			graphics.DrawLines(
				arrowPen,
				new[]
				{
					new Point(centerX - 4, centerY - 2),
					new Point(centerX, centerY + 2),
					new Point(centerX + 4, centerY - 2)
				});

			if (drawOuterBorder)
			{
				using Pen outerBorderPen = new(borderColor, 1F);
				graphics.DrawRectangle(
					outerBorderPen,
					bounds.X,
					bounds.Y,
					bounds.Width - 1,
					bounds.Height - 1);
			}
		}
	}
}
