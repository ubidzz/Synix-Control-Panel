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
	internal static class RoundedGeometry
	{
		public static GraphicsPath Create(Rectangle bounds, int radius)
		{
			GraphicsPath path = new();

			if (bounds.Width <= 1 || bounds.Height <= 1 || radius <= 0)
			{
				path.AddRectangle(bounds);
				return path;
			}

			int diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
			Rectangle arc = new(bounds.X, bounds.Y, diameter, diameter);

			path.AddArc(arc, 180, 90);
			arc.X = bounds.Right - diameter;
			path.AddArc(arc, 270, 90);
			arc.Y = bounds.Bottom - diameter;
			path.AddArc(arc, 0, 90);
			arc.X = bounds.Left;
			path.AddArc(arc, 90, 90);
			path.CloseFigure();

			return path;
		}
	}
}
