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
	[DefaultEvent(nameof(CheckedChanged))]
	[ToolboxItem(true)]
	public sealed class ModernSettingsToggle : CheckBox
	{
		private bool _hovered;

		public ModernSettingsToggle()
		{
			SetStyle(
				ControlStyles.UserPaint |
				ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.ResizeRedraw,
				true);

			AutoSize = false;
			BackColor = SettingsPalette.Card;
			Cursor = Cursors.Hand;
			Size = new Size(54, 30);
			Text = string.Empty;
			AccessibleRole = AccessibleRole.CheckButton;
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
			Invalidate();
			base.OnMouseLeave(eventArgs);
		}

		protected override void OnPaintBackground(PaintEventArgs eventArgs)
		{
			Color parentColor = Parent?.BackColor ?? SettingsPalette.Card;
			eventArgs.Graphics.Clear(parentColor);
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			OnPaintBackground(eventArgs);
			eventArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

			Rectangle trackBounds = new(1, 2, Width - 3, Height - 4);
			Color trackColor;

			if (!Enabled)
				trackColor = SettingsPalette.DisabledSurface;
			else if (Checked)
				trackColor = _hovered ? SettingsPalette.AccentHover : SettingsPalette.Accent;
			else
				trackColor = _hovered
					? SettingsPalette.BorderHover
					: SettingsPalette.DisabledSurface;

			using GraphicsPath trackPath = RoundedGeometry.Create(trackBounds, trackBounds.Height / 2);
			using SolidBrush trackBrush = new(trackColor);
			eventArgs.Graphics.FillPath(trackBrush, trackPath);

			using Pen borderPen = new(
				Checked ? SettingsPalette.AccentHover : SettingsPalette.BorderHover,
				1F);
			eventArgs.Graphics.DrawPath(borderPen, trackPath);

			int thumbSize = trackBounds.Height - 6;
			int thumbX = Checked
				? trackBounds.Right - thumbSize - 3
				: trackBounds.Left + 3;
			Rectangle thumbBounds = new(
				thumbX,
				trackBounds.Top + 3,
				thumbSize,
				thumbSize);

			using SolidBrush shadowBrush = new(Color.FromArgb(65, 0, 0, 0));
			eventArgs.Graphics.FillEllipse(
				shadowBrush,
				thumbBounds.X + 1,
				thumbBounds.Y + 2,
				thumbBounds.Width,
				thumbBounds.Height);

			using SolidBrush thumbBrush = new(
				Enabled ? Color.WhiteSmoke : SettingsPalette.DisabledText);
			eventArgs.Graphics.FillEllipse(thumbBrush, thumbBounds);

			if (Focused && ShowFocusCues)
			{
				Rectangle focusBounds = ClientRectangle;
				focusBounds.Inflate(-1, -1);
				ControlPaint.DrawFocusRectangle(eventArgs.Graphics, focusBounds);
			}
		}
	}
}
