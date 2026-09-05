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
	[ToolboxItem(true)]
	public sealed class ModernSettingsGlyph : Control
	{
		[Category("Synix Appearance")]
		[DefaultValue("•")]
		public string Glyph { get; set; } = "•";

		public ModernSettingsGlyph()
		{
			SetStyle(
				ControlStyles.UserPaint |
				ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer,
				true);

			BackColor = SettingsPalette.Card;
			ForeColor = SettingsPalette.Accent;
			Font = new Font("Segoe UI Symbol", 15F, FontStyle.Regular);
			Size = new Size(42, 42);
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			eventArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			Rectangle bounds = new(0, 0, Width - 1, Height - 1);
			using GraphicsPath path = RoundedGeometry.Create(bounds, 10);
			using SolidBrush fillBrush = new(SettingsPalette.AccentSoft);
			using Pen borderPen = new(Color.FromArgb(45, SettingsPalette.Accent), 1F);

			eventArgs.Graphics.FillPath(fillBrush, path);
			eventArgs.Graphics.DrawPath(borderPen, path);

			TextRenderer.DrawText(
				eventArgs.Graphics,
				Glyph,
				Font,
				ClientRectangle,
				ForeColor,
				TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
		}
	}
}
