/*
 * Copyright (c) 2026 ubidzz. All Rights Reserved.
 *
 * This file is part of Synix Control Panel.
 *
 * This code is provided for transparent viewing and personal use only.
 * Unauthorized distribution, public modification, or commercial 
 * use of this source code or the compiled executable is strictly 
 * prohibited. Please refer to the LICENSE file in the root 
 * directory for full terms.
 */
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Synix_Control_Panel.UI
{

	[ToolboxItem(true)]
	public class SynixToggle : CheckBox
	{
		public SynixToggle()
		{
			this.SetStyle(ControlStyles.UserPaint |
						  ControlStyles.AllPaintingInWmPaint |
						  ControlStyles.OptimizedDoubleBuffer |
						  ControlStyles.SupportsTransparentBackColor, true);

			this.BackColor = Color.Transparent;
			this.Size = new Size(60, 28);
			this.Cursor = Cursors.Hand;
		}

		protected override void OnPaintBackground(PaintEventArgs pevent) { /* Handled in OnPaint */ }

		protected override void OnPaint(PaintEventArgs e)
		{
			UIStyleHelper.DrawRoundedSlider(e.Graphics, this, this.Tag?.ToString() ?? "");
		}
	}

	public static class UIStyleHelper
	{
		private static readonly Font _sliderFont = new Font("Segoe UI", 8F, FontStyle.Bold);

		public static void StyleToggleButton(CheckBox chk, string labelPrefix)
		{
			chk.Cursor = Cursors.Hand;
			chk.AutoSize = false;
			chk.BackColor = Color.Transparent;
			chk.Tag = labelPrefix; // Ensure the prefix is stored in the Tag for the paint handler

			// 🎯 THE FIX: Use a named method instead of an anonymous lambda to prevent event stacking leaks
			chk.Paint -= Chk_CustomPaint;
			chk.Paint += Chk_CustomPaint;

			chk.Invalidate();
		}

		private static void Chk_CustomPaint(object sender, PaintEventArgs e)
		{
			if (sender is CheckBox chk)
			{
				string labelPrefix = chk.Tag?.ToString() ?? "";
				DrawRoundedSlider(e.Graphics, chk, labelPrefix);
			}
		}

		public static void InitializeToggles(Control parent)
		{
			foreach (Control ctrl in parent.Controls)
			{
				if (ctrl is CheckBox chk && chk.Name.StartsWith("chk"))
				{
					StyleToggleButton(chk, chk.Tag?.ToString() ?? "");
				}
				if (ctrl.HasChildren) InitializeToggles(ctrl);
			}
		}

		public static void DrawRoundedSlider(Graphics g, CheckBox chk, string label)
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;

			if (Application.RenderWithVisualStyles)
				ButtonRenderer.DrawParentBackground(g, chk.ClientRectangle, chk);
			else
			{
				using (var b = new SolidBrush(chk.Parent?.BackColor ?? Color.FromArgb(32, 32, 32)))
					g.FillRectangle(b, chk.ClientRectangle);
			}

			Rectangle rect = new Rectangle(2, 2, chk.Width - 6, chk.Height - 6);
			int diameter = rect.Height;

			using (GraphicsPath path = new GraphicsPath())
			{
				path.AddArc(rect.X, rect.Y, diameter, diameter, 90, 180);
				path.AddArc(rect.Width - diameter + rect.X, rect.Y, diameter, diameter, 270, 180);
				path.CloseFigure();

				Color trackColor = chk.Checked ? Color.FromArgb(40, 150, 40) : Color.FromArgb(60, 60, 60);
				using (var brush = new SolidBrush(trackColor))
				{
					g.FillPath(brush, path);
				}

				using (var pen = new Pen(Color.FromArgb(30, 30, 30), 2.2f))
				{
					g.DrawPath(pen, path);
				}

				float thumbSize = rect.Height - 8;
				float xPos = chk.Checked ? (rect.Right - thumbSize - 4) : (rect.Left + 4);
				g.FillEllipse(Brushes.White, xPos, rect.Y + 4, thumbSize, thumbSize);

				string text = !string.IsNullOrEmpty(chk.Text) ? chk.Text : (string.IsNullOrEmpty(label) ? (chk.Checked ? "ON" : "OFF") : label);

				// 🎯 THE FIX: Use the cached static font here instead of instantiating a new Font
				Rectangle textRect = chk.Checked ? new Rectangle(rect.X, rect.Y, rect.Width - 22, rect.Height)
											   : new Rectangle(rect.X + 22, rect.Y, rect.Width - 22, rect.Height);

				TextRenderer.DrawText(g, text, _sliderFont, textRect, Color.White,
					TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
			}
		}

		public static void WarningLabel_Paint(object sender, PaintEventArgs e)
		{
			Label lbl = (Label)sender;
			if (lbl.Width <= 0 || lbl.Height <= 0) return;

			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			int radius = 15;
			using (GraphicsPath path = new GraphicsPath())
			{
				path.AddArc(0, 0, radius, radius, 180, 90);
				path.AddArc(lbl.Width - radius - 1, 0, radius, radius, 270, 90);
				path.AddArc(lbl.Width - radius - 1, lbl.Height - radius - 1, radius, radius, 0, 90);
				path.AddArc(0, lbl.Height - radius - 1, radius, radius, 90, 90);
				path.CloseFigure();

				// 🎯 THE FIX: Store the old region and dispose of it explicitly before overwriting it
				if (lbl.Region == null || lbl.Region.GetBounds(e.Graphics).Width != lbl.Width)
				{
					var oldRegion = lbl.Region;
					lbl.Region = new Region(path);
					oldRegion?.Dispose();
				}

				using (SolidBrush brush = new SolidBrush(lbl.BackColor))
					e.Graphics.FillPath(brush, path);
			}

			TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;

			string align = lbl.Tag?.ToString() ?? "MiddleCenter";

			if (align == "MiddleRight")
			{
				flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Right;
			}
			else if (align == "MiddleLeft")
			{
				flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Left;
			}
			else if (align == "TopCenter")
			{
				flags = TextFormatFlags.Top | TextFormatFlags.HorizontalCenter;
			}

			TextRenderer.DrawText(e.Graphics, lbl.Text, lbl.Font, lbl.ClientRectangle, lbl.ForeColor, flags);
		}

		public static void StyleWarningLabel(Label lbl, string alignment = "MiddleCenter")
		{
			if (lbl == null) return;

			lbl.AutoSize = false;
			lbl.FlatStyle = FlatStyle.Flat;

			lbl.Tag = alignment;

			lbl.Paint -= WarningLabel_Paint;
			lbl.Paint += WarningLabel_Paint;

			lbl.Invalidate();
		}
	}
}