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
	public sealed class ModernSettingsNavButton : Button
	{
		private bool _selected;
		private bool _hovered;
		private bool _attentionRequired;
		private float _attentionPulse;
		private readonly Font _glyphFont =
			new("Segoe UI Symbol", 14F, FontStyle.Regular);

		[Category("Synix Appearance")]
		[DefaultValue("•")]
		public string IconGlyph { get; set; } = "•";

		[Category("Synix Appearance")]
		[DefaultValue(false)]
		public bool Selected
		{
			get => _selected;
			set
			{
				_selected = value;
				Invalidate();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool AttentionRequired
		{
			get => _attentionRequired;
			set
			{
				if (_attentionRequired == value)
					return;

				_attentionRequired = value;
				Invalidate();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public float AttentionPulse
		{
			get => _attentionPulse;
			set
			{
				float normalized = Math.Clamp(value, 0F, 1F);
				if (Math.Abs(_attentionPulse - normalized) < 0.01F)
					return;

				_attentionPulse = normalized;
				if (AttentionRequired)
					Invalidate();
			}
		}

		public ModernSettingsNavButton()
		{
			SetStyle(
				ControlStyles.UserPaint |
				ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.ResizeRedraw,
				true);

			FlatStyle = FlatStyle.Flat;
			FlatAppearance.BorderSize = 0;
			BackColor = SettingsPalette.Sidebar;
			ForeColor = SettingsPalette.SecondaryText;
			Font = new Font("Segoe UI", 10.5F, FontStyle.Regular);
			TextAlign = ContentAlignment.MiddleLeft;
			UseMnemonic = false;
			Cursor = Cursors.Hand;
			Size = new Size(180, 54);
			Margin = new Padding(0, 0, 0, 8);
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

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				_glyphFont.Dispose();

			base.Dispose(disposing);
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			eventArgs.Graphics.Clear(SettingsPalette.Sidebar);
			eventArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

			Rectangle bounds = new(0, 0, Width - 1, Height - 1);
			if (Selected || _hovered)
			{
				Color fill = Selected
					? SettingsPalette.Selection
					: SettingsPalette.CardHover;
				using GraphicsPath path = RoundedGeometry.Create(bounds, 9);
				using SolidBrush fillBrush = new(fill);
				eventArgs.Graphics.FillPath(fillBrush, path);
			}

			if (Selected)
			{
				using SolidBrush accentBrush = new(SettingsPalette.Accent);
				using GraphicsPath accentPath = RoundedGeometry.Create(
					new Rectangle(0, 10, 4, Height - 20),
					2);
				eventArgs.Graphics.FillPath(accentBrush, accentPath);
			}

			Color normalTextColor = Selected
				? SettingsPalette.Accent
				: SettingsPalette.SecondaryText;
			Color textColor = AttentionRequired
				? BlendColor(
					normalTextColor,
					SettingsPalette.Warning,
					0.35F + (AttentionPulse * 0.65F))
				: normalTextColor;

			TextRenderer.DrawText(
				eventArgs.Graphics,
				IconGlyph,
				_glyphFont,
				new Rectangle(18, 0, 30, Height),
				textColor,
				TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

			TextRenderer.DrawText(
				eventArgs.Graphics,
				Text,
				Font,
				new Rectangle(58, 0, Width - 66, Height),
				textColor,
				TextFormatFlags.Left |
				TextFormatFlags.VerticalCenter |
				TextFormatFlags.EndEllipsis |
				TextFormatFlags.NoPrefix);
		}

		private static Color BlendColor(Color from, Color to, float amount)
		{
			float normalized = Math.Clamp(amount, 0F, 1F);
			return Color.FromArgb(
				(int)Math.Round(from.A + ((to.A - from.A) * normalized)),
				(int)Math.Round(from.R + ((to.R - from.R) * normalized)),
				(int)Math.Round(from.G + ((to.G - from.G) * normalized)),
				(int)Math.Round(from.B + ((to.B - from.B) * normalized)));
		}
	}
}
