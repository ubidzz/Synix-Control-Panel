// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Synix_Control_Panel.SynixApp.Design
{
	[ToolboxItem(true)]
	public class SynixGauge : Control
	{
		private float _value = 0;
		private float _maxValue = 100;
		private string _gaugeLabel = "CPU %";

		[Category("Synix Design"), Description("Current value of the gauge.")]
		public float Value
		{
			get => _value;
			set
			{
				_value = Math.Max(0, Math.Min(value, _maxValue));
				this.Invalidate();
			}
		}

		[Category("Synix Design"), Description("Maximum value of the gauge.")]
		public float MaxValue
		{
			get => _maxValue;
			set
			{
				_maxValue = Math.Max(1, value);
				this.Invalidate();
			}
		}

		public SynixGauge()
		{
			this.SetStyle(ControlStyles.UserPaint |
						  ControlStyles.AllPaintingInWmPaint |
						  ControlStyles.OptimizedDoubleBuffer |
						  ControlStyles.ResizeRedraw |
						  ControlStyles.SupportsTransparentBackColor, true);

			this.BackColor = Color.Transparent;
			this.Size = new Size(150, 150);
			this.ForeColor = Color.White;
		}

		public void UpdateGauge(float newValue, string newLabel)
		{
			_gaugeLabel = newLabel;
			_value = Math.Max(0, Math.Min(newValue, _maxValue));
			this.Invalidate();
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			int minDimension = Math.Min(this.Width, this.Height);
			int cx = this.Width / 2;
			int cy = this.Height / 2;

			using (GraphicsPath path = new GraphicsPath())
			{
				path.AddEllipse(cx - (minDimension / 2), cy - (minDimension / 2), minDimension, minDimension);
				this.Region?.Dispose();
				this.Region = new Region(path);
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			Graphics g = e.Graphics;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

			int minDimension = Math.Min(this.Width, this.Height);
			int cx = this.Width / 2;
			int cy = this.Height / 2;
			int radius = (minDimension / 2) - (int)(minDimension * 0.05);

			float startAngle = 135f;
			float sweepAngle = 270f;

			// --- 1. OUTER DASHED ARC ---
			using (Pen dashedPen = new Pen(Color.FromArgb(0, 100, 255), Math.Max(2f, minDimension * 0.015f)))
			{
				dashedPen.DashStyle = DashStyle.Dash;
				dashedPen.DashPattern = new float[] { 3f, 4f };
				g.DrawArc(dashedPen, cx - radius, cy - radius, radius * 2, radius * 2, startAngle, sweepAngle);
			}

			int innerRadius = (int)(radius * 0.85);
			Rectangle innerRect = new Rectangle(cx - innerRadius, cy - innerRadius, innerRadius * 2, innerRadius * 2);

			// --- 2. COLOR TRACK ---
			using (Pen trackPen = new Pen(Color.DodgerBlue, Math.Max(3f, minDimension * 0.025f)))
			{
				g.DrawArc(trackPen, innerRect, startAngle, sweepAngle * 0.6f);
				trackPen.Color = Color.MediumPurple;
				g.DrawArc(trackPen, innerRect, startAngle + (sweepAngle * 0.6f), sweepAngle * 0.2f);
				trackPen.Color = Color.Crimson;
				g.DrawArc(trackPen, innerRect, startAngle + (sweepAngle * 0.8f), sweepAngle * 0.2f);
			}

			// --- 3. DYNAMIC TICKS AND NUMBERS ---
			int textRadius = (int)(radius * 0.62);
			int tickLength = (int)(radius * 0.06);

			using (Pen tickPen = new Pen(Color.White, 1.5f))
			using (SolidBrush textBrush = new SolidBrush(this.ForeColor))
			using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
			{
				float tickFontSize = Math.Max(6f, minDimension * 0.045f);

				using (Font dynamicFont = new Font("Segoe UI", tickFontSize, FontStyle.Bold))
				{
					float tickStep = _maxValue / 5f;

					for (float i = 0; i <= _maxValue + 0.01f; i += tickStep)
					{
						float angle = startAngle + (i / _maxValue) * sweepAngle;
						double rad = angle * Math.PI / 180.0;

						int outerX = cx + (int)(innerRadius * Math.Cos(rad));
						int outerY = cy + (int)(innerRadius * Math.Sin(rad));
						int innerX = cx + (int)((innerRadius - tickLength) * Math.Cos(rad));
						int innerY = cy + (int)((innerRadius - tickLength) * Math.Sin(rad));
						g.DrawLine(tickPen, innerX, innerY, outerX, outerY);

						int textX = cx + (int)(textRadius * Math.Cos(rad));
						int textY = cy + (int)(textRadius * Math.Sin(rad));
						g.DrawString(Math.Round(i).ToString(), dynamicFont, textBrush, textX, textY, sf);
					}
				}
			}

			// --- 4. INNER GLOW RING ---
			int glowRadius = (int)(radius * 0.38);
			using (Pen glowPen = new Pen(Color.FromArgb(40, 0, 150, 255), Math.Max(4f, minDimension * 0.06f)))
			{
				g.DrawEllipse(glowPen, cx - glowRadius, cy - glowRadius, glowRadius * 2, glowRadius * 2);
				glowPen.Color = Color.DodgerBlue;
				glowPen.Width = Math.Max(2f, minDimension * 0.02f);
				g.DrawEllipse(glowPen, cx - glowRadius, cy - glowRadius, glowRadius * 2, glowRadius * 2);
			}

			// --- 5. THE EDGE-ATTACHED TAPERED NEEDLE ---
			float needleAngle = startAngle + (_value / _maxValue) * sweepAngle;
			double needleRad = needleAngle * Math.PI / 180.0;

			float cos = (float)Math.Cos(needleRad);
			float sin = (float)Math.Sin(needleRad);
			float perpCos = (float)Math.Cos(needleRad + Math.PI / 2);
			float perpSin = (float)Math.Sin(needleRad + Math.PI / 2);

			int needleTipLength = (int)(radius * 0.72);
			int needleBaseRadius = glowRadius;
			float needleWidth = Math.Max(2f, minDimension * 0.02f);

			PointF tipPoint = new PointF(cx + needleTipLength * cos, cy + needleTipLength * sin);
			PointF backPoint = new PointF(cx + needleBaseRadius * cos, cy + needleBaseRadius * sin);
			PointF leftBase = new PointF(backPoint.X + needleWidth * perpCos, backPoint.Y + needleWidth * perpSin);
			PointF rightBase = new PointF(backPoint.X - needleWidth * perpCos, backPoint.Y - needleWidth * perpSin);

			using (GraphicsPath needlePath = new GraphicsPath())
			{
				needlePath.AddPolygon(new PointF[] { tipPoint, leftBase, backPoint, rightBase });
				using (SolidBrush needleBrush = new SolidBrush(Color.FromArgb(240, 240, 255)))
				{
					g.FillPath(needleBrush, needlePath);
				}
				using (Pen needleBorder = new Pen(Color.Cyan, 1f))
				{
					g.DrawPath(needleBorder, needlePath);
				}
			}

			// --- 6. CENTRAL TEXT ---
			using (SolidBrush textBrush = new SolidBrush(Color.White))
			using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
			{
				float labelSize = Math.Max(6f, minDimension * 0.04f);
				float valueSize = Math.Max(8f, minDimension * 0.08f);

				using (Font labelFont = new Font("Segoe UI", labelSize, FontStyle.Bold))
				using (Font valueFont = new Font("Segoe UI", valueSize, FontStyle.Bold))
				{
					g.DrawString(_gaugeLabel, labelFont, textBrush, cx, cy + (int)(radius * 0.15), sf);
					g.DrawString(_value.ToString("0.0"), valueFont, textBrush, cx, cy - (int)(radius * 0.08), sf);
				}
			}
		}
	}
}