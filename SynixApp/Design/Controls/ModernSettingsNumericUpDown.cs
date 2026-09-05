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
	[DefaultEvent(nameof(ValueChanged))]
	[ToolboxItem(true)]
	public sealed class ModernSettingsNumericUpDown : Control, ISupportInitialize
	{
		private int _minimum = 1;
		private int _maximum = 100;
		private int _value = 1;
		private int _increment = 1;
		private int _hoveredButton;
		private int _pressedButton;
		private bool _initializing;
		private bool _replaceOnNextInput = true;
		private string _editBuffer = "1";

		public event EventHandler? ValueChanged;

		[DefaultValue(1)]
		public int Increment
		{
			get => _increment;
			set => _increment = Math.Max(1, value);
		}

		[DefaultValue(1)]
		public int Minimum
		{
			get => _minimum;
			set
			{
				_minimum = value;
				if (_maximum < _minimum)
					_maximum = _minimum;
				Value = _value;
			}
		}

		[DefaultValue(100)]
		public int Maximum
		{
			get => _maximum;
			set
			{
				_maximum = Math.Max(value, _minimum);
				Value = _value;
			}
		}

		[DefaultValue(1)]
		public int Value
		{
			get => _value;
			set
			{
				int clampedValue = Math.Clamp(value, Minimum, Maximum);
				if (_value == clampedValue)
					return;

				_value = clampedValue;
				if (!Focused)
					_editBuffer = _value.ToString();
				Invalidate();
				if (!_initializing)
					ValueChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public ModernSettingsNumericUpDown()
		{
			SetStyle(
				ControlStyles.UserPaint |
				ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.Opaque |
				ControlStyles.ResizeRedraw |
				ControlStyles.Selectable,
				true);

			AccessibleRole = AccessibleRole.SpinButton;
			BackColor = SettingsPalette.Input;
			Cursor = Cursors.IBeam;
			Font = new Font("Segoe UI", 11F, FontStyle.Regular);
			ForeColor = SettingsPalette.PrimaryText;
			Size = new Size(112, 42);
			TabStop = true;
		}

		protected override void OnPaintBackground(PaintEventArgs eventArgs)
		{

		}

		protected override void OnEnabledChanged(EventArgs eventArgs)
		{
			base.OnEnabledChanged(eventArgs);
			BackColor = SettingsPalette.Input;
			Invalidate();
		}

		public void BeginInit()
		{
			_initializing = true;
		}

		public void EndInit()
		{
			_initializing = false;
			Value = _value;
			_editBuffer = Value.ToString();
			Invalidate();
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			int hoveredButton = 0;
			if (eventArgs.X >= Width - 34)
				hoveredButton = eventArgs.Y < Height / 2 ? 1 : 2;

			Cursor = hoveredButton == 0 ? Cursors.IBeam : Cursors.Hand;

			if (_hoveredButton != hoveredButton)
			{
				_hoveredButton = hoveredButton;
				Invalidate();
			}

			base.OnMouseMove(eventArgs);
		}

		protected override void OnMouseLeave(EventArgs eventArgs)
		{
			_hoveredButton = 0;
			_pressedButton = 0;
			Cursor = Cursors.IBeam;
			Invalidate();
			base.OnMouseLeave(eventArgs);
		}

		protected override void OnMouseEnter(EventArgs eventArgs)
		{
			Invalidate();
			base.OnMouseEnter(eventArgs);
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			Focus();

			if (eventArgs.Button == MouseButtons.Left && eventArgs.X >= Width - 34)
			{
				_pressedButton = eventArgs.Y < Height / 2 ? 1 : 2;
				Value += _pressedButton == 1 ? Increment : -Increment;
				_editBuffer = Value.ToString();
				_replaceOnNextInput = true;
				Invalidate();
			}
			else if (eventArgs.Button == MouseButtons.Left)
			{
				_replaceOnNextInput = true;
				Invalidate();
			}

			base.OnMouseDown(eventArgs);
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			_pressedButton = 0;
			Invalidate();
			base.OnMouseUp(eventArgs);
		}

		protected override void OnMouseWheel(MouseEventArgs eventArgs)
		{
			if (Focused)
			{
				Value += eventArgs.Delta > 0 ? Increment : -Increment;
				_editBuffer = Value.ToString();
				_replaceOnNextInput = true;
			}
			base.OnMouseWheel(eventArgs);
		}

		protected override void OnGotFocus(EventArgs eventArgs)
		{
			_editBuffer = Value.ToString();
			_replaceOnNextInput = true;
			Invalidate();
			base.OnGotFocus(eventArgs);
		}

		protected override void OnLostFocus(EventArgs eventArgs)
		{
			CommitEditBuffer();
			_replaceOnNextInput = true;
			Invalidate();
			base.OnLostFocus(eventArgs);
		}

		protected override void OnKeyPress(KeyPressEventArgs eventArgs)
		{
			if (char.IsDigit(eventArgs.KeyChar))
			{
				if (_replaceOnNextInput)
				{
					_editBuffer = string.Empty;
					_replaceOnNextInput = false;
				}

				if (_editBuffer.Length < 10)
					_editBuffer += eventArgs.KeyChar;

				ApplyValidEditBuffer();
				Invalidate();
				eventArgs.Handled = true;
			}

			base.OnKeyPress(eventArgs);
		}

		protected override void OnKeyDown(KeyEventArgs eventArgs)
		{
			if (eventArgs.KeyCode == Keys.Up)
			{
				Value += Increment;
				_editBuffer = Value.ToString();
				_replaceOnNextInput = true;
				eventArgs.Handled = true;
			}
			else if (eventArgs.KeyCode == Keys.Down)
			{
				Value -= Increment;
				_editBuffer = Value.ToString();
				_replaceOnNextInput = true;
				eventArgs.Handled = true;
			}
			else if (eventArgs.KeyCode == Keys.Back)
			{
				if (_replaceOnNextInput)
				{
					_editBuffer = string.Empty;
					_replaceOnNextInput = false;
				}
				else if (_editBuffer.Length > 0)
				{
					_editBuffer = _editBuffer[..^1];
				}

				ApplyValidEditBuffer();
				Invalidate();
				eventArgs.Handled = true;
				eventArgs.SuppressKeyPress = true;
			}
			else if (eventArgs.KeyCode == Keys.Delete)
			{
				_editBuffer = string.Empty;
				_replaceOnNextInput = false;
				Invalidate();
				eventArgs.Handled = true;
			}
			else if (eventArgs.KeyCode == Keys.Enter)
			{
				CommitEditBuffer();
				_replaceOnNextInput = true;
				eventArgs.Handled = true;
				eventArgs.SuppressKeyPress = true;
			}
			else if (eventArgs.KeyCode == Keys.Escape)
			{
				_editBuffer = Value.ToString();
				_replaceOnNextInput = true;
				Invalidate();
				eventArgs.Handled = true;
			}
			else if (eventArgs.Control && eventArgs.KeyCode == Keys.A)
			{
				_replaceOnNextInput = true;
				eventArgs.Handled = true;
			}

			base.OnKeyDown(eventArgs);
		}

		private void ApplyValidEditBuffer()
		{
			if (int.TryParse(_editBuffer, out int parsedValue) &&
				parsedValue >= Minimum &&
				parsedValue <= Maximum)
			{
				Value = parsedValue;
			}
		}

		private void CommitEditBuffer()
		{
			if (int.TryParse(_editBuffer, out int parsedValue))
				Value = Math.Clamp(parsedValue, Minimum, Maximum);

			_editBuffer = Value.ToString();
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			Color parentColor = Parent?.BackColor ?? SettingsPalette.Card;
			eventArgs.Graphics.Clear(parentColor);
			eventArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

			Rectangle bounds = new(0, 0, Width - 1, Height - 1);
			using GraphicsPath path = RoundedGeometry.Create(bounds, 8);
			Color fillColor = SettingsPalette.Input;
			using SolidBrush fillBrush = new(fillColor);
			Color borderColor = SettingsPalette.Border;
			using Pen borderPen = new(borderColor, 1F);
			eventArgs.Graphics.FillPath(fillBrush, path);
			eventArgs.Graphics.DrawPath(borderPen, path);

			int buttonLeft = Width - 34;
			if (Enabled && _hoveredButton != 0)
			{
				Rectangle hoverBounds = _hoveredButton == 1
					? new Rectangle(buttonLeft + 1, 1, 32, Height / 2 - 1)
					: new Rectangle(buttonLeft + 1, Height / 2, 32, Height / 2 - 1);
				Color hoverColor = _pressedButton == _hoveredButton
					? Color.FromArgb(52, SettingsPalette.Accent)
					: Color.FromArgb(30, SettingsPalette.Accent);
				using SolidBrush hoverBrush = new(hoverColor);
				eventArgs.Graphics.FillRectangle(hoverBrush, hoverBounds);
			}

			using Pen dividerPen = new(SettingsPalette.Border, 1F);
			eventArgs.Graphics.DrawLine(dividerPen, buttonLeft, 1, buttonLeft, Height - 2);
			eventArgs.Graphics.DrawLine(
				dividerPen,
				buttonLeft,
				Height / 2,
				Width - 2,
				Height / 2);

			TextRenderer.DrawText(
				eventArgs.Graphics,
				Focused && !_replaceOnNextInput
					? _editBuffer
					: Value.ToString(),
				Font,
				new Rectangle(12, 0, buttonLeft - 16, Height),
				Enabled ? ForeColor : SettingsPalette.MutedText,
				TextFormatFlags.Left |
				TextFormatFlags.VerticalCenter |
				TextFormatFlags.EndEllipsis |
				TextFormatFlags.NoPrefix);

			using Pen arrowPen = new(
				Enabled ? SettingsPalette.PrimaryText : SettingsPalette.MutedText,
				1.7F)
			{
				StartCap = LineCap.Round,
				EndCap = LineCap.Round
			};
			int centerX = buttonLeft + 17;
			int upperY = Height / 4;
			int lowerY = (Height * 3) / 4;
			eventArgs.Graphics.DrawLines(
				arrowPen,
				new[]
				{
					new Point(centerX - 4, upperY + 2),
					new Point(centerX, upperY - 2),
					new Point(centerX + 4, upperY + 2)
				});
			eventArgs.Graphics.DrawLines(
				arrowPen,
				new[]
				{
					new Point(centerX - 4, lowerY - 2),
					new Point(centerX, lowerY + 2),
					new Point(centerX + 4, lowerY - 2)
				});

		}
	}
}
