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

namespace Synix_Control_Panel.SynixApp.Design
{
	/// <summary>
	/// Opaque, owner-drawn dashboard button. It never asks Windows to paint a
	/// native border, which keeps every normal, hover, focused, and disabled
	/// state inside the Synix dark design system.
	/// </summary>
	[ToolboxItem(true)]
	public class SynixButton : Button
	{
		private bool _hovered;
		private bool _pressed;

		[Category("Synix Design")]
		[DefaultValue(8)]
		public int BorderRadius { get; set; } = 8;

		[Category("Synix Design")]
		public Color BorderColor { get; set; } = SettingsPalette.BorderHover;

		[Category("Synix Design")]
		[DefaultValue(1)]
		public int BorderSize { get; set; } = 1;

		[Category("Synix Design")]
		public Color FillColor { get; set; } = SettingsPalette.Input;

		[Category("Synix Design")]
		public Color FillColorSecondary { get; set; } = SettingsPalette.CardHover;

		public SynixButton()
		{
			SetStyle(
				ControlStyles.UserPaint |
				ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.ResizeRedraw,
				true);

			FlatStyle = FlatStyle.Flat;
			FlatAppearance.BorderSize = 0;
			FlatAppearance.MouseOverBackColor = SettingsPalette.Input;
			FlatAppearance.MouseDownBackColor = SettingsPalette.Input;
			BackColor = SettingsPalette.Card;
			ForeColor = SettingsPalette.PrimaryText;
			Font = new Font("Segoe UI", 10F, FontStyle.Bold);
			Cursor = Cursors.Hand;
			Size = new Size(130, 42);
			TabStop = false;
			UseMnemonic = false;
		}

		protected override void OnMouseEnter(EventArgs eventArgs)
		{
			_hovered = true;
			Invalidate();
			base.OnMouseEnter(eventArgs);
		}

		protected override void OnMouseLeave(EventArgs eventArgs)
		{
			_hovered = false;
			_pressed = false;
			Invalidate();
			base.OnMouseLeave(eventArgs);
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			_pressed = eventArgs.Button == MouseButtons.Left;
			Invalidate();
			base.OnMouseDown(eventArgs);
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			_pressed = false;
			Invalidate();
			base.OnMouseUp(eventArgs);
		}

		protected override void OnEnabledChanged(EventArgs eventArgs)
		{
			base.OnEnabledChanged(eventArgs);
			Invalidate();
		}

		protected override void OnPaintBackground(PaintEventArgs eventArgs)
		{
			eventArgs.Graphics.Clear(Parent?.BackColor ?? BackColor);
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			OnPaintBackground(eventArgs);
			Graphics graphics = eventArgs.Graphics;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

			Rectangle bounds = new(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
			int radius = Math.Max(0, Math.Min(BorderRadius, Math.Min(bounds.Width, bounds.Height) / 2));
			Color fillColor;
			Color borderColor;
			Color textColor;

			if (!Enabled)
			{
				fillColor = SettingsPalette.DisabledSurface;
				borderColor = SettingsPalette.Border;
				textColor = SettingsPalette.MutedText;
			}
			else
			{
				fillColor = _pressed
					? ControlPaint.Dark(FillColor, 0.12F)
					: _hovered
						? FillColorSecondary
						: FillColor;
				borderColor = _hovered
					? ControlPaint.Light(BorderColor, 0.12F)
					: BorderColor;
				textColor = ForeColor;
			}

			using GraphicsPath path = RoundedGeometry.Create(bounds, radius);
			using SolidBrush fillBrush = new(fillColor);
			graphics.FillPath(fillBrush, path);

			if (BorderSize > 0)
			{
				using Pen borderPen = new(borderColor, BorderSize);
				graphics.DrawPath(borderPen, path);
			}

			Rectangle contentBounds = new(
				Padding.Left,
				Padding.Top,
				Math.Max(0, Width - Padding.Horizontal),
				Math.Max(0, Height - Padding.Vertical));

			if (Image != null)
			{
				int iconSize = Math.Min(Image.Width, Math.Max(16, Height - 18));
				int combinedWidth = iconSize + 8 + TextRenderer.MeasureText(Text, Font).Width;
				int iconX = Math.Max(8, (Width - combinedWidth) / 2);
				int iconY = (Height - iconSize) / 2;
				graphics.DrawImage(Image, new Rectangle(iconX, iconY, iconSize, iconSize));
				contentBounds.X = iconX + iconSize + 8;
				contentBounds.Width = Math.Max(0, Width - contentBounds.X - 8);
			}

			TextRenderer.DrawText(
				graphics,
				Text,
				Font,
				contentBounds,
				textColor,
				TextFormatFlags.HorizontalCenter |
				TextFormatFlags.VerticalCenter |
				TextFormatFlags.EndEllipsis |
				TextFormatFlags.NoPrefix);
		}
	}
}
