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
	public sealed class ModernSettingsButton : Button
	{
		private bool _hovered;
		private bool _pressed;
		private bool _useAccentStyle;

		[DefaultValue(false)]
		[Category("Synix Appearance")]
		public bool UseAccentStyle
		{
			get => _useAccentStyle;
			set
			{
				_useAccentStyle = value;
				Invalidate();
			}
		}

		public ModernSettingsButton()
		{
			SetStyle(
				ControlStyles.UserPaint |
				ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.ResizeRedraw,
				true);

			BackColor = SettingsPalette.Input;
			FlatStyle = FlatStyle.Flat;
			FlatAppearance.BorderSize = 0;
			ForeColor = SettingsPalette.PrimaryText;
			Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			Cursor = Cursors.Hand;
			Size = new Size(96, 42);
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
			_pressed = true;
			Invalidate();
			base.OnMouseDown(eventArgs);
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			_pressed = false;
			Invalidate();
			base.OnMouseUp(eventArgs);
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			Color parentColor = Parent?.BackColor ?? SettingsPalette.Card;
			eventArgs.Graphics.Clear(parentColor);
			eventArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

			Rectangle bounds = new(0, 0, Width - 1, Height - 1);
			Color fillColor;
			if (!Enabled)
			{
				fillColor = SettingsPalette.DisabledSurface;
			}
			else if (UseAccentStyle)
			{
				fillColor = _pressed
					? Color.FromArgb(24, 176, 165)
					: _hovered
						? SettingsPalette.AccentHover
						: SettingsPalette.Accent;
			}
			else
			{
				fillColor = _pressed
					? SettingsPalette.Selection
					: _hovered
						? SettingsPalette.CardHover
						: SettingsPalette.Input;
			}

			using GraphicsPath path = RoundedGeometry.Create(bounds, 8);
			using SolidBrush fillBrush = new(fillColor);
			Color borderColor = UseAccentStyle && Enabled
				? fillColor
				: _hovered && Enabled
					? SettingsPalette.Accent
					: SettingsPalette.BorderHover;
			using Pen borderPen = new(borderColor, 1F);

			eventArgs.Graphics.FillPath(fillBrush, path);
			eventArgs.Graphics.DrawPath(borderPen, path);

			Rectangle textBounds = new(
				Padding.Left,
				Padding.Top,
				Math.Max(0, Width - Padding.Horizontal),
				Math.Max(0, Height - Padding.Vertical));
			Color textColor = !Enabled
				? SettingsPalette.MutedText
				: UseAccentStyle
					? SettingsPalette.AccentText
					: ForeColor;
			TextRenderer.DrawText(
				eventArgs.Graphics,
				Text,
				Font,
				textBounds,
				textColor,
				TextFormatFlags.HorizontalCenter |
				TextFormatFlags.VerticalCenter |
				TextFormatFlags.EndEllipsis |
				TextFormatFlags.NoPrefix);
		}
	}
}
