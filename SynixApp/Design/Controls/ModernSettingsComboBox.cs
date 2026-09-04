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
	[DefaultEvent(nameof(SelectedIndexChanged))]
	[ToolboxItem(true)]
	public class ModernSettingsComboBox : ComboBox
	{
		private const int WmPaint = 0x000F;
		private const int WmNcPaint = 0x0085;
		private const int WmEraseBackground = 0x0014;
		private const int WsBorder = 0x00800000;
		private const int WsExClientEdge = 0x00000200;
		private bool _mouseInside;

		[Category("Synix Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public Color BorderColor { get; set; } = SettingsPalette.Border;

		[Category("Synix Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public Color FocusBorderColor { get; set; } = SettingsPalette.Border;

		[Category("Synix Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public Color ArrowColor { get; set; } = SettingsPalette.SecondaryText;

		[Category("Synix Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public Color SelectedItemBackColor { get; set; } =
			SettingsPalette.Selection;

		public ModernSettingsComboBox()
		{
			SetStyle(
				ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.ResizeRedraw,
				true);

			BackColor = SettingsPalette.Input;
			ForeColor = SettingsPalette.PrimaryText;
			Font = new Font("Segoe UI", 10F, FontStyle.Regular);
			Cursor = Cursors.Hand;
			DrawMode = DrawMode.OwnerDrawFixed;
			DropDownStyle = ComboBoxStyle.DropDownList;
			FlatStyle = FlatStyle.Flat;
			IntegralHeight = true;
			ItemHeight = 28;
			MaxDropDownItems = 8;
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams parameters = base.CreateParams;
				parameters.Style &= ~WsBorder;
				parameters.ExStyle &= ~WsExClientEdge;
				return parameters;
			}
		}

		protected override void OnDrawItem(DrawItemEventArgs eventArgs)
		{
			Color itemBackColor;
			Color itemForeColor;
			bool selected = (eventArgs.State & DrawItemState.Selected) != 0;

			if (!Enabled)
			{
				itemBackColor = SettingsPalette.Input;
				itemForeColor = SettingsPalette.MutedText;
			}
			else
			{
				itemBackColor = selected
					? SelectedItemBackColor
					: SettingsPalette.Input;
				itemForeColor = selected
					? SettingsPalette.Accent
					: SettingsPalette.PrimaryText;
			}

			using SolidBrush backgroundBrush = new(itemBackColor);
			eventArgs.Graphics.FillRectangle(backgroundBrush, eventArgs.Bounds);

			string itemText = (eventArgs.Index >= 0 && eventArgs.Index < Items.Count
				? GetItemText(Items[eventArgs.Index])
				: Text) ?? string.Empty;
			bool drawingEditArea =
				(eventArgs.State & DrawItemState.ComboBoxEdit) != 0;
			int rightPadding = drawingEditArea ? 44 : 18;
			Rectangle textBounds = new(
				eventArgs.Bounds.X + 10,
				eventArgs.Bounds.Y,
				Math.Max(0, eventArgs.Bounds.Width - rightPadding),
				eventArgs.Bounds.Height);
			TextRenderer.DrawText(
				eventArgs.Graphics,
				itemText,
				Font,
				textBounds,
				itemForeColor,
				TextFormatFlags.Left |
				TextFormatFlags.VerticalCenter |
				TextFormatFlags.EndEllipsis |
				TextFormatFlags.NoPrefix);
		}

		protected override void OnMouseEnter(EventArgs eventArgs)
		{
			_mouseInside = true;
			Invalidate();
			base.OnMouseEnter(eventArgs);
		}

		protected override void OnMouseLeave(EventArgs eventArgs)
		{
			_mouseInside = false;
			Invalidate();
			base.OnMouseLeave(eventArgs);
		}

		protected override void OnGotFocus(EventArgs eventArgs)
		{
			Invalidate();
			base.OnGotFocus(eventArgs);
		}

		protected override void OnLostFocus(EventArgs eventArgs)
		{
			Invalidate();
			base.OnLostFocus(eventArgs);
		}

		protected override void OnDropDown(EventArgs eventArgs)
		{
			Invalidate();
			base.OnDropDown(eventArgs);
		}

		protected override void OnDropDownClosed(EventArgs eventArgs)
		{
			Invalidate();
			base.OnDropDownClosed(eventArgs);
		}

		protected override void OnSelectedIndexChanged(EventArgs eventArgs)
		{
			Invalidate();
			base.OnSelectedIndexChanged(eventArgs);
		}

		protected override void OnEnabledChanged(EventArgs eventArgs)
		{
			base.OnEnabledChanged(eventArgs);
			BackColor = SettingsPalette.Input;
			Invalidate();
		}

		protected override void OnTextChanged(EventArgs eventArgs)
		{
			base.OnTextChanged(eventArgs);
			Invalidate();
		}

		protected override void WndProc(ref Message message)
		{
			if (message.Msg == WmNcPaint)
			{

				message.Result = IntPtr.Zero;
				return;
			}

			if (message.Msg == WmEraseBackground)
			{

				message.Result = new IntPtr(1);
				return;
			}

			base.WndProc(ref message);

			if (message.Msg == WmPaint &&
				IsHandleCreated &&
				ClientSize.Width > 2 &&
				ClientSize.Height > 2)
			{
				using Graphics graphics = CreateGraphics();
				DrawModernChrome(graphics);
			}
		}

		private void DrawModernChrome(Graphics graphics)
		{
			using SolidBrush inputBrush = new(SettingsPalette.Input);
			graphics.FillRectangle(inputBrush, ClientRectangle);

			Color borderColor = Focused || DroppedDown
				? FocusBorderColor
				: BorderColor;
			Color arrowColor = !Enabled
				? SettingsPalette.MutedText
				: Focused || DroppedDown || _mouseInside
					? SettingsPalette.Accent
					: ArrowColor;
			Color arrowBackground = SettingsPalette.Input;
			int buttonWidth = Math.Min(
				Math.Max(28, Math.Min(ClientSize.Height, 34)),
				Math.Max(1, ClientSize.Width / 2));
			Rectangle textBounds = new(
				10,
				0,
				Math.Max(0, ClientSize.Width - buttonWidth - 16),
				ClientSize.Height);
			TextRenderer.DrawText(
				graphics,
				Text,
				Font,
				textBounds,
				Enabled ? SettingsPalette.PrimaryText : SettingsPalette.MutedText,
				TextFormatFlags.Left |
				TextFormatFlags.VerticalCenter |
				TextFormatFlags.EndEllipsis |
				TextFormatFlags.NoPrefix);

			ModernComboBoxRenderer.DrawArrowButton(
				graphics,
				ClientRectangle,
				arrowBackground,
				borderColor,
				arrowColor,
				true);
		}
	}
}
