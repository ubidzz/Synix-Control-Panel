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
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

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
		public static void StyleToggleButton(CheckBox chk, string labelPrefix)
		{
			chk.Cursor = Cursors.Hand;
			chk.AutoSize = false;
			chk.BackColor = Color.Transparent;

			chk.Paint -= (s, e) => DrawRoundedSlider(e.Graphics, chk, labelPrefix);
			chk.Paint += (s, e) => DrawRoundedSlider(e.Graphics, chk, labelPrefix);

			chk.Invalidate();
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

			// 🎯 THE TRANSPARENCY: Forces the parent's texture onto the button area
			if (Application.RenderWithVisualStyles)
				ButtonRenderer.DrawParentBackground(g, chk.ClientRectangle, chk);
			else
			{
				using (var b = new SolidBrush(chk.Parent?.BackColor ?? Color.FromArgb(32, 32, 32)))
					g.FillRectangle(b, chk.ClientRectangle);
			}

			// Inset by 2.5 pixels to give the thicker border room to breathe
			Rectangle rect = new Rectangle(2, 2, chk.Width - 6, chk.Height - 6);
			int diameter = rect.Height;

			using (GraphicsPath path = new GraphicsPath())
			{
				path.AddArc(rect.X, rect.Y, diameter, diameter, 90, 180);
				path.AddArc(rect.Width - diameter + rect.X, rect.Y, diameter, diameter, 270, 180);
				path.CloseFigure();

				// 1. Draw the Track (Green for ON, Dark Gray for OFF)
				Color trackColor = chk.Checked ? Color.FromArgb(40, 150, 40) : Color.FromArgb(60, 60, 60);
				using (var brush = new SolidBrush(trackColor))
				{
					g.FillPath(brush, path);
				}

				// 🎯 THE THICKER BORDER: Bumped to 2.2f for that high-end look
				using (var pen = new Pen(Color.FromArgb(30, 30, 30), 2.2f))
				{
					g.DrawPath(pen, path);
				}

				// 2. Draw the Sliding Circle (Thumb)
				// Slightly smaller now to account for the thicker border
				float thumbSize = rect.Height - 8;
				float xPos = chk.Checked ? (rect.Right - thumbSize - 4) : (rect.Left + 4);
				g.FillEllipse(Brushes.White, xPos, rect.Y + 4, thumbSize, thumbSize);

				// 3. Draw Labels (Sun, Mon, etc.)
				string text = !string.IsNullOrEmpty(chk.Text) ? chk.Text : (string.IsNullOrEmpty(label) ? (chk.Checked ? "ON" : "OFF") : label);
				Font font = new Font("Segoe UI", 8F, FontStyle.Bold);

				Rectangle textRect = chk.Checked ? new Rectangle(rect.X, rect.Y, rect.Width - 22, rect.Height)
											   : new Rectangle(rect.X + 22, rect.Y, rect.Width - 22, rect.Height);

				TextRenderer.DrawText(g, text, font, textRect, Color.White,
					TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
			}
		}

		public static void WarningLabel_Paint(object sender, PaintEventArgs e)
		{
			Label lbl = (Label)sender;
			if (lbl.Width <= 0 || lbl.Height <= 0) return;

			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

			int radius = 15; // Buffalo-sized roundness
			using (GraphicsPath path = new GraphicsPath())
			{
				path.AddArc(0, 0, radius, radius, 180, 90);
				path.AddArc(lbl.Width - radius - 1, 0, radius, radius, 270, 90);
				path.AddArc(lbl.Width - radius - 1, lbl.Height - radius - 1, radius, radius, 0, 90);
				path.AddArc(0, lbl.Height - radius - 1, radius, radius, 90, 90);
				path.CloseFigure();

				// 🎯 1. CLIP THE REGION (This makes it actually round)
				if (lbl.Region == null || lbl.Region.GetBounds(e.Graphics).Width != lbl.Width)
				{
					lbl.Region = new Region(path);
				}

				// 🎯 2. FILL BACKGROUND
				using (SolidBrush brush = new SolidBrush(lbl.BackColor))
				{
					e.Graphics.FillPath(brush, path);
				}
			}

			// 🎯 3. DRAW TEXT (Using the center alignment)
			TextRenderer.DrawText(e.Graphics, lbl.Text, lbl.Font, lbl.ClientRectangle, lbl.ForeColor,
				TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
		}

		public static void StyleWarningLabel(Label lbl)
		{
			if (lbl == null) return;

			lbl.AutoSize = false;
			lbl.FlatStyle = FlatStyle.Flat;

			// 🎯 Explicitly use System.Drawing to kill the ambiguity
			lbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

			lbl.Paint -= WarningLabel_Paint;
			lbl.Paint += WarningLabel_Paint;

			lbl.Invalidate();
		}
	}
}