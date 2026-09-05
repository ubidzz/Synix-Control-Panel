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
	public sealed class ModernSettingsCard : Panel
	{
		private int _cornerRadius = 12;

		[DefaultValue(12)]
		[Category("Synix Appearance")]
		public int CornerRadius
		{
			get => _cornerRadius;
			set
			{
				_cornerRadius = Math.Max(0, value);
				UpdateRoundedRegion();
				PerformLayout();
				Invalidate();
			}
		}

		[Category("Synix Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public Color FillColor { get; set; } = SettingsPalette.Card;

		[Category("Synix Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public Color BorderColor { get; set; } = SettingsPalette.Border;

		public override Rectangle DisplayRectangle
		{
			get
			{
				Rectangle bounds = base.DisplayRectangle;
				double radius = Math.Min(CornerRadius,
					Math.Max(0, Math.Min(ClientSize.Width - 1, ClientSize.Height - 1) / 2D));
				// Docked, opaque layouts must stay inside the curve as well as the
				// straight border. Otherwise child windows paint over the smoothed edge.
				int inset = radius > 0 ? (int)Math.Ceiling(radius * (1D - Math.Sqrt(0.5D))) + 2 : 1;
				int left = Math.Max(0, inset - Padding.Left);
				int top = Math.Max(0, inset - Padding.Top);
				int right = Math.Max(0, inset - Padding.Right);
				int bottom = Math.Max(0, inset - Padding.Bottom);
				return new Rectangle(bounds.X + left, bounds.Y + top,
					Math.Max(0, bounds.Width - left - right),
					Math.Max(0, bounds.Height - top - bottom));
			}
		}

		public ModernSettingsCard()
		{
			SetStyle(
				ControlStyles.UserPaint |
				ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.ResizeRedraw,
				true);

			BackColor = SettingsPalette.Card;
			Margin = new Padding(0, 0, 0, 16);
		}

		protected override void OnResize(EventArgs eventArgs)
		{
			base.OnResize(eventArgs);
			UpdateRoundedRegion();
		}

		protected override void OnHandleCreated(EventArgs eventArgs)
		{
			base.OnHandleCreated(eventArgs);
			// Designer layouts can resume without a layout pass, retaining edge-to-edge
			// child bounds until the card is resized. Apply the content inset on first use.
			PerformLayout();
		}

		protected override void OnPaintBackground(PaintEventArgs eventArgs)
		{
			// Blend the rounded edge into the surface behind the card, not its own fill.
			eventArgs.Graphics.Clear(Parent?.BackColor ?? BackColor);
			if (Parent is Control parent)
			{
				GraphicsState state = eventArgs.Graphics.Save();
				try
				{
					eventArgs.Graphics.TranslateTransform(-Left, -Top);
					Rectangle clip = eventArgs.ClipRectangle;
					clip.Offset(Left, Top);
					using PaintEventArgs parentArgs = new(eventArgs.Graphics, clip);
					InvokePaintBackground(parent, parentArgs);
				}
				finally
				{
					eventArgs.Graphics.Restore(state);
				}
			}
			else
			{
				base.OnPaintBackground(eventArgs);
			}

			if (ClientSize.Width <= 1 || ClientSize.Height <= 1)
				return;

			eventArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			using GraphicsPath path = CreateCardPath();
			using SolidBrush brush = new(FillColor);
			eventArgs.Graphics.FillPath(brush, path);
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			base.OnPaint(eventArgs);

			if (ClientSize.Width <= 1 || ClientSize.Height <= 1)
				return;

			eventArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			using GraphicsPath path = CreateCardPath();
			using Pen borderPen = new(BorderColor, 1F);
			eventArgs.Graphics.DrawPath(borderPen, path);
		}

		private GraphicsPath CreateCardPath() => RoundedGeometry.Create(
			new Rectangle(0, 0, ClientSize.Width - 1, ClientSize.Height - 1),
			CornerRadius);

		private void UpdateRoundedRegion()
		{
			if (Width <= 1 || Height <= 1)
				return;

			using GraphicsPath path = CreateCardPath();
			Region roundedRegion = new(path);
			// A hard window region must include the centered border and its antialias
			// fringe. Derive it from the same path so no corner loses edge pixels.
			using Pen edgeAllowance = new(Color.Black, 3F);
			path.Widen(edgeAllowance);
			roundedRegion.Union(path);
			roundedRegion.Intersect(ClientRectangle);

			Region? oldRegion = Region;
			Region = roundedRegion;
			oldRegion?.Dispose();
		}
	}
}
