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
	/// Compact, opaque dashboard gauge. The old speedometer-style control was
	/// visually heavy and relied on transparency; this ring gauge remains stable
	/// inside WinForms cards and at high DPI.
	/// </summary>
	[ToolboxItem(true)]
	public sealed class SynixGauge : Control
	{
		private float _value;
		private float _maxValue = 100F;
		private string _gaugeLabel = "CPU %";

		[Category("Synix Design")]
		[Description("Current value of the gauge.")]
		[DefaultValue(0F)]
		public float Value
		{
			get => _value;
			set
			{
				_value = Math.Clamp(value, 0F, _maxValue);
				Invalidate();
			}
		}

		[Category("Synix Design")]
		[Description("Maximum value of the gauge.")]
		[DefaultValue(100F)]
		public float MaxValue
		{
			get => _maxValue;
			set
			{
				_maxValue = Math.Max(1F, value);
				_value = Math.Min(_value, _maxValue);
				Invalidate();
			}
		}

		public SynixGauge()
		{
			SetStyle(
				ControlStyles.UserPaint |
				ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.ResizeRedraw,
				true);

			BackColor = SettingsPalette.Card;
			ForeColor = SettingsPalette.PrimaryText;
			Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			Size = new Size(76, 76);
		}

		public void UpdateGauge(float newValue, string newLabel)
		{
			_gaugeLabel = string.IsNullOrWhiteSpace(newLabel) ? string.Empty : newLabel;
			Value = newValue;
		}

		protected override void OnPaintBackground(PaintEventArgs eventArgs)
		{
			eventArgs.Graphics.Clear(BackColor);
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			base.OnPaint(eventArgs);

			Graphics graphics = eventArgs.Graphics;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

			int diameter = Math.Max(18, Math.Min(Width, Height) - 14);
			int left = (Width - diameter) / 2;
			int top = (Height - diameter) / 2;
			Rectangle ringBounds = new(left, top, diameter, diameter);
			float strokeWidth = Math.Max(5F, diameter * 0.105F);

			using Pen trackPen = new(SettingsPalette.Border, strokeWidth)
			{
				StartCap = LineCap.Round,
				EndCap = LineCap.Round
			};
			graphics.DrawArc(trackPen, ringBounds, -90F, 360F);

			float ratio = _maxValue <= 0F ? 0F : Math.Clamp(_value / _maxValue, 0F, 1F);
			Color progressColor = ratio >= 0.90F
				? SettingsPalette.Danger
				: ratio >= 0.72F
					? SettingsPalette.Warning
					: SettingsPalette.Accent;

			if (ratio > 0.001F)
			{
				using Pen progressPen = new(progressColor, strokeWidth)
				{
					StartCap = LineCap.Round,
					EndCap = LineCap.Round
				};
				graphics.DrawArc(progressPen, ringBounds, -90F, Math.Max(2F, ratio * 360F));
			}

			string unit = _gaugeLabel.Contains("RAM", StringComparison.OrdinalIgnoreCase)
				? "GB"
				: "%";
			string valueText = _value.ToString("0.0");

			Rectangle valueBounds = new(0, Height / 2 - 15, Width, 22);
			Rectangle unitBounds = new(0, Height / 2 + 5, Width, 15);
			using Font valueFont = new("Segoe UI", Math.Max(9F, diameter * 0.18F), FontStyle.Bold);
			using Font unitFont = new("Segoe UI", Math.Max(6.5F, diameter * 0.10F), FontStyle.Bold);

			TextRenderer.DrawText(
				graphics,
				valueText,
				valueFont,
				valueBounds,
				ForeColor,
				TextFormatFlags.HorizontalCenter |
				TextFormatFlags.VerticalCenter |
				TextFormatFlags.NoPadding);
			TextRenderer.DrawText(
				graphics,
				unit,
				unitFont,
				unitBounds,
				SettingsPalette.SecondaryText,
				TextFormatFlags.HorizontalCenter |
				TextFormatFlags.VerticalCenter |
				TextFormatFlags.NoPadding);
		}
	}
}
