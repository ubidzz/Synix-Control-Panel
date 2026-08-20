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
	public static class SettingsPalette
	{
		public static readonly Color Window = Color.FromArgb(8, 13, 24);
		public static readonly Color TitleBar = Color.FromArgb(6, 12, 22);
		public static readonly Color Sidebar = Color.FromArgb(10, 18, 32);
		public static readonly Color Card = Color.FromArgb(17, 27, 45);
		public static readonly Color CardHover = Color.FromArgb(20, 33, 54);
		public static readonly Color Input = Color.FromArgb(12, 21, 36);
		public static readonly Color Border = Color.FromArgb(38, 52, 77);
		public static readonly Color BorderHover = Color.FromArgb(55, 76, 108);
		public static readonly Color PrimaryText = Color.FromArgb(245, 247, 251);
		public static readonly Color SecondaryText = Color.FromArgb(158, 172, 194);
		public static readonly Color MutedText = Color.FromArgb(105, 124, 153);
		public static readonly Color Accent = Color.FromArgb(32, 214, 199);
		public static readonly Color AccentHover = Color.FromArgb(50, 231, 216);
		public static readonly Color AccentSoft = Color.FromArgb(28, 75, 91);
		public static readonly Color Warning = Color.FromArgb(245, 185, 76);
	}

	internal static class RoundedGeometry
	{
		public static GraphicsPath Create(Rectangle bounds, int radius)
		{
			GraphicsPath path = new();

			if (bounds.Width <= 1 || bounds.Height <= 1 || radius <= 0)
			{
				path.AddRectangle(bounds);
				return path;
			}

			int diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
			Rectangle arc = new(bounds.X, bounds.Y, diameter, diameter);

			path.AddArc(arc, 180, 90);
			arc.X = bounds.Right - diameter;
			path.AddArc(arc, 270, 90);
			arc.Y = bounds.Bottom - diameter;
			path.AddArc(arc, 0, 90);
			arc.X = bounds.Left;
			path.AddArc(arc, 90, 90);
			path.CloseFigure();

			return path;
		}
	}

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
				Invalidate();
			}
		}

		[Category("Synix Appearance")]
		public Color FillColor { get; set; } = SettingsPalette.Card;

		[Category("Synix Appearance")]
		public Color BorderColor { get; set; } = SettingsPalette.Border;

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

		protected override void OnPaintBackground(PaintEventArgs eventArgs)
		{
			base.OnPaintBackground(eventArgs);

			if (ClientSize.Width <= 1 || ClientSize.Height <= 1)
				return;

			eventArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			using GraphicsPath path = RoundedGeometry.Create(
				new Rectangle(0, 0, Width - 1, Height - 1),
				CornerRadius);
			using SolidBrush brush = new(FillColor);
			eventArgs.Graphics.FillPath(brush, path);
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			base.OnPaint(eventArgs);

			if (ClientSize.Width <= 1 || ClientSize.Height <= 1)
				return;

			eventArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			using GraphicsPath path = RoundedGeometry.Create(
				new Rectangle(0, 0, Width - 1, Height - 1),
				CornerRadius);
			using Pen borderPen = new(BorderColor, 1F);
			eventArgs.Graphics.DrawPath(borderPen, path);
		}

		private void UpdateRoundedRegion()
		{
			if (Width <= 1 || Height <= 1)
				return;

			using GraphicsPath path = RoundedGeometry.Create(
				new Rectangle(0, 0, Width, Height),
				CornerRadius);

			Region? oldRegion = Region;
			Region = new Region(path);
			oldRegion?.Dispose();
		}
	}

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
				trackColor = Color.FromArgb(36, 45, 60);
			else if (Checked)
				trackColor = _hovered ? SettingsPalette.AccentHover : SettingsPalette.Accent;
			else
				trackColor = _hovered ? Color.FromArgb(66, 80, 101) : Color.FromArgb(48, 59, 78);

			using GraphicsPath trackPath = RoundedGeometry.Create(trackBounds, trackBounds.Height / 2);
			using SolidBrush trackBrush = new(trackColor);
			eventArgs.Graphics.FillPath(trackBrush, trackPath);

			using Pen borderPen = new(
				Checked ? Color.FromArgb(80, 255, 239) : SettingsPalette.BorderHover,
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
				Enabled ? Color.WhiteSmoke : Color.FromArgb(150, 158, 170));
			eventArgs.Graphics.FillEllipse(thumbBrush, thumbBounds);

			if (Focused && ShowFocusCues)
			{
				Rectangle focusBounds = ClientRectangle;
				focusBounds.Inflate(-1, -1);
				ControlPaint.DrawFocusRectangle(eventArgs.Graphics, focusBounds);
			}
		}
	}

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
				fillColor = Color.FromArgb(25, 34, 48);
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
					? Color.FromArgb(27, 48, 66)
					: _hovered
						? Color.FromArgb(25, 42, 60)
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
					? SettingsPalette.Window
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

	internal static class ModernComboBoxRenderer
	{
		public static void DrawArrowButton(
			Graphics graphics,
			Rectangle bounds,
			Color backgroundColor,
			Color borderColor,
			Color arrowColor,
			bool drawOuterBorder)
		{
			if (bounds.Width <= 2 || bounds.Height <= 2)
			{
				return;
			}

			int buttonWidth = Math.Min(
				Math.Max(28, Math.Min(bounds.Height, 34)),
				Math.Max(1, bounds.Width / 2));
			Rectangle buttonBounds = new(
				bounds.Right - buttonWidth,
				bounds.Top + 1,
				buttonWidth - 1,
				bounds.Height - 2);

			using SolidBrush backgroundBrush = new(backgroundColor);
			graphics.FillRectangle(backgroundBrush, buttonBounds);

			using Pen dividerPen = new(borderColor, 1F);
			graphics.DrawLine(
				dividerPen,
				buttonBounds.Left,
				buttonBounds.Top + 4,
				buttonBounds.Left,
				buttonBounds.Bottom - 4);

			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			int centerX = buttonBounds.Left + buttonBounds.Width / 2;
			int centerY = buttonBounds.Top + buttonBounds.Height / 2;
			using Pen arrowPen = new(arrowColor, 1.8F)
			{
				StartCap = LineCap.Round,
				EndCap = LineCap.Round,
				LineJoin = LineJoin.Round
			};
			graphics.DrawLines(
				arrowPen,
				new[]
				{
					new Point(centerX - 4, centerY - 2),
					new Point(centerX, centerY + 2),
					new Point(centerX + 4, centerY - 2)
				});

			if (drawOuterBorder)
			{
				using Pen outerBorderPen = new(borderColor, 1F);
				graphics.DrawRectangle(
					outerBorderPen,
					bounds.X,
					bounds.Y,
					bounds.Width - 1,
					bounds.Height - 1);
			}
		}
	}

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
		public Color BorderColor { get; set; } = SettingsPalette.Border;

		[Category("Synix Appearance")]
		public Color FocusBorderColor { get; set; } = SettingsPalette.Border;

		[Category("Synix Appearance")]
		public Color ArrowColor { get; set; } = SettingsPalette.SecondaryText;

		[Category("Synix Appearance")]
		public Color SelectedItemBackColor { get; set; } =
			Color.FromArgb(24, 55, 73);

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

			string itemText = eventArgs.Index >= 0 && eventArgs.Index < Items.Count
				? GetItemText(Items[eventArgs.Index])
				: Text;
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
				// The native non-client frame is where Windows injects the bright
				// focused/disabled border. The control paints its own dark frame.
				message.Result = IntPtr.Zero;
				return;
			}

			if (message.Msg == WmEraseBackground)
			{
				// Prevent a system-color flash before the owner-drawn frame appears.
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

	/// <summary>
	/// Grid cell used by read-only label and badge columns. It deliberately removes
	/// the Selected and Focus paint states so informational columns never look
	/// interactive, even when WinForms temporarily makes one the current cell.
	/// </summary>
	[ToolboxItem(false)]
	public sealed class ModernSettingsDataGridViewInformationalCell :
		DataGridViewTextBoxCell
	{
		protected override void Paint(
			Graphics graphics,
			Rectangle clipBounds,
			Rectangle cellBounds,
			int rowIndex,
			DataGridViewElementStates cellState,
			object? value,
			object? formattedValue,
			string? errorText,
			DataGridViewCellStyle cellStyle,
			DataGridViewAdvancedBorderStyle advancedBorderStyle,
			DataGridViewPaintParts paintParts)
		{
			DataGridViewElementStates informationalState =
				cellState & ~DataGridViewElementStates.Selected;
			DataGridViewPaintParts informationalPaintParts =
				paintParts & ~DataGridViewPaintParts.Focus;

			base.Paint(
				graphics,
				clipBounds,
				cellBounds,
				rowIndex,
				informationalState,
				value,
				formattedValue,
				errorText,
				cellStyle,
				advancedBorderStyle,
				informationalPaintParts);
		}
	}

	[ToolboxItem(false)]
	public sealed class ModernSettingsDataGridViewComboBoxCell : DataGridViewComboBoxCell
	{
		public ModernSettingsDataGridViewComboBoxCell()
		{
			DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox;
			DisplayStyleForCurrentCellOnly = false;
			FlatStyle = FlatStyle.Flat;
			ValueType = typeof(string);
			Style.BackColor = SettingsPalette.Input;
			Style.ForeColor = SettingsPalette.PrimaryText;
			Style.Padding = Padding.Empty;
			Style.SelectionBackColor = SettingsPalette.Input;
			Style.SelectionForeColor = SettingsPalette.PrimaryText;
		}

		public override Type EditType =>
			typeof(ModernSettingsDataGridViewComboBoxEditingControl);

		public override void InitializeEditingControl(
			int rowIndex,
			object? initialFormattedValue,
			DataGridViewCellStyle dataGridViewCellStyle)
		{
			base.InitializeEditingControl(
				rowIndex,
				initialFormattedValue,
				dataGridViewCellStyle);

			if (DataGridView?.EditingControl is not
				ModernSettingsDataGridViewComboBoxEditingControl editor)
			{
				return;
			}

			editor.BeginValueInitialization();
			editor.BeginUpdate();
			try
			{
				editor.Items.Clear();
				foreach (object item in Items)
				{
					editor.Items.Add(item);
				}

				editor.DropDownWidth = DropDownWidth;
				editor.MaxDropDownItems = MaxDropDownItems;
				editor.Sorted = Sorted;
				editor.EditingControlRowIndex = rowIndex;

				string selectedText = Value?.ToString() ??
					initialFormattedValue?.ToString() ??
					string.Empty;
				int selectedIndex = editor.FindStringExact(selectedText);
				editor.SelectedIndex = selectedIndex;
				editor.EditingControlValueChanged = false;
			}
			finally
			{
				editor.EndUpdate();
				editor.EndValueInitialization();
			}
		}

		public override void PositionEditingControl(
			bool setLocation,
			bool setSize,
			Rectangle cellBounds,
			Rectangle cellClip,
			DataGridViewCellStyle cellStyle,
			bool singleVerticalBorderAdded,
			bool singleHorizontalBorderAdded,
			bool isFirstDisplayedColumn,
			bool isFirstDisplayedRow)
		{
			base.PositionEditingControl(
				setLocation,
				setSize,
				cellBounds,
				cellClip,
				cellStyle,
				singleVerticalBorderAdded,
				singleHorizontalBorderAdded,
				isFirstDisplayedColumn,
				isFirstDisplayedRow);

			DataGridView? owner = DataGridView;
			if (owner?.EditingControl is not
				ModernSettingsDataGridViewComboBoxEditingControl editor)
			{
				return;
			}

			Rectangle visibleCellBounds = Rectangle.Intersect(cellBounds, cellClip);
			if (visibleCellBounds.Width <= 0 || visibleCellBounds.Height <= 0)
			{
				return;
			}

			Panel editingPanel = owner.EditingPanel;
			editingPanel.SuspendLayout();
			try
			{
				editingPanel.BackColor = SettingsPalette.Input;
				editingPanel.BorderStyle = BorderStyle.None;
				editingPanel.Margin = Padding.Empty;
				editingPanel.Padding = Padding.Empty;
				editingPanel.Bounds = visibleCellBounds;

				editor.PrepareForFullCellHeight(cellBounds.Height);
				editor.Dock = DockStyle.None;
				editor.Margin = Padding.Empty;
				editor.Padding = Padding.Empty;
				editor.Bounds = new Rectangle(
					cellBounds.X - visibleCellBounds.X,
					cellBounds.Y - visibleCellBounds.Y,
					cellBounds.Width,
					cellBounds.Height);
				editor.BringToFront();
			}
			finally
			{
				editingPanel.ResumeLayout(false);
			}
		}

		protected override void Paint(
			Graphics graphics,
			Rectangle clipBounds,
			Rectangle cellBounds,
			int rowIndex,
			DataGridViewElementStates cellState,
			object? value,
			object? formattedValue,
			string? errorText,
			DataGridViewCellStyle cellStyle,
			DataGridViewAdvancedBorderStyle advancedBorderStyle,
			DataGridViewPaintParts paintParts)
		{
			base.Paint(
				graphics,
				clipBounds,
				cellBounds,
				rowIndex,
				cellState,
				value,
				formattedValue,
				errorText,
				cellStyle,
				advancedBorderStyle,
				paintParts);

			if ((paintParts & DataGridViewPaintParts.ContentForeground) == 0)
			{
				return;
			}

			bool selected = (cellState & DataGridViewElementStates.Selected) != 0;
			DataGridView? owner = DataGridView;
			bool currentCell = owner != null &&
				owner.CurrentCellAddress.X == ColumnIndex &&
				owner.CurrentCellAddress.Y == rowIndex;
			Color arrowBackground = selected
				? cellStyle.SelectionBackColor
				: cellStyle.BackColor;

			ModernComboBoxRenderer.DrawArrowButton(
				graphics,
				cellBounds,
				arrowBackground,
				SettingsPalette.Border,
				currentCell ? SettingsPalette.Accent : SettingsPalette.SecondaryText,
				false);
		}
	}

	[ToolboxItem(false)]
	public sealed class ModernSettingsDataGridViewComboBoxEditingControl :
		ModernSettingsComboBox,
		IDataGridViewEditingControl
	{
		private DataGridView? _editingDataGridView;
		private bool _initializingValue;

		public ModernSettingsDataGridViewComboBoxEditingControl()
		{
			BackColor = SettingsPalette.Input;
			ForeColor = SettingsPalette.PrimaryText;
			FlatStyle = FlatStyle.Flat;
			BorderColor = SettingsPalette.Border;
			FocusBorderColor = SettingsPalette.Border;
			ArrowColor = SettingsPalette.SecondaryText;
			SelectedItemBackColor = Color.FromArgb(24, 55, 73);
			Margin = Padding.Empty;
		}

		public object EditingControlFormattedValue
		{
			get => SelectedItem == null ? Text : GetItemText(SelectedItem);
			set
			{
				string formattedValue = value?.ToString() ?? string.Empty;
				bool wasInitializing = _initializingValue;
				_initializingValue = true;
				try
				{
					SelectedIndex = FindStringExact(formattedValue);
					EditingControlValueChanged = false;
				}
				finally
				{
					_initializingValue = wasInitializing;
				}
			}
		}

		public object GetEditingControlFormattedValue(
			DataGridViewDataErrorContexts context)
		{
			return EditingControlFormattedValue;
		}

		public void ApplyCellStyleToEditingControl(
			DataGridViewCellStyle dataGridViewCellStyle)
		{
			Font = dataGridViewCellStyle.Font ?? Font;
			BackColor = SettingsPalette.Input;
			ForeColor = SettingsPalette.PrimaryText;
			FlatStyle = FlatStyle.Flat;
			BorderColor = SettingsPalette.Border;
			FocusBorderColor = SettingsPalette.Border;
			ArrowColor = SettingsPalette.SecondaryText;
			SelectedItemBackColor = Color.FromArgb(24, 55, 73);
			Margin = Padding.Empty;
			dataGridViewCellStyle.BackColor = SettingsPalette.Input;
			dataGridViewCellStyle.ForeColor = SettingsPalette.PrimaryText;
			dataGridViewCellStyle.SelectionBackColor = SettingsPalette.Input;
			dataGridViewCellStyle.SelectionForeColor = SettingsPalette.PrimaryText;
			dataGridViewCellStyle.Padding = Padding.Empty;
		}

		public bool EditingControlWantsInputKey(
			Keys keyData,
			bool dataGridViewWantsInputKey)
		{
			switch (keyData & Keys.KeyCode)
			{
				case Keys.Down:
				case Keys.Up:
				case Keys.Home:
				case Keys.End:
				case Keys.PageDown:
				case Keys.PageUp:
				case Keys.F4:
					return true;
				default:
					return !dataGridViewWantsInputKey;
			}
		}

		public void PrepareEditingControlForEdit(bool selectAll)
		{
			// The user can open the list with the custom arrow, F4, or Alt+Down.
		}

		public int EditingControlRowIndex { get; set; }

		public bool RepositionEditingControlOnValueChange => false;

		public DataGridView? EditingControlDataGridView
		{
			get => _editingDataGridView;
			set => _editingDataGridView = value;
		}

		public bool EditingControlValueChanged { get; set; }

		public Cursor EditingPanelCursor => Cursors.Default;

		internal void BeginValueInitialization()
		{
			_initializingValue = true;
		}

		internal void EndValueInitialization()
		{
			_initializingValue = false;
		}

		internal void PrepareForFullCellHeight(int cellHeight)
		{
			int chromeHeight = Math.Max(0, PreferredHeight - ItemHeight);
			int desiredItemHeight = Math.Max(18, cellHeight - chromeHeight);
			if (ItemHeight != desiredItemHeight)
			{
				ItemHeight = desiredItemHeight;
			}

			MinimumSize = Size.Empty;
			MaximumSize = Size.Empty;
			Margin = Padding.Empty;
			Padding = Padding.Empty;
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			bool wasDroppedDown = DroppedDown;
			base.OnMouseDown(eventArgs);

			if (eventArgs.Button == MouseButtons.Left &&
				!wasDroppedDown &&
				!DroppedDown)
			{
				DroppedDown = true;
			}
		}

		protected override void OnSelectedIndexChanged(EventArgs eventArgs)
		{
			base.OnSelectedIndexChanged(eventArgs);

			DataGridView? owner = EditingControlDataGridView;
			if (_initializingValue || owner == null)
			{
				return;
			}

			EditingControlValueChanged = true;
			owner.NotifyCurrentCellDirty(true);
		}
	}

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
		private bool _mouseInside;
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
			// OnPaint covers every pixel. Suppressing the native background pass
			// prevents disabled/focus states from flashing a system-white frame.
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
			_mouseInside = false;
			_hoveredButton = 0;
			_pressedButton = 0;
			Cursor = Cursors.IBeam;
			Invalidate();
			base.OnMouseLeave(eventArgs);
		}

		protected override void OnMouseEnter(EventArgs eventArgs)
		{
			_mouseInside = true;
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

	[ToolboxItem(true)]
	public sealed class ModernSettingsNavButton : Button
	{
		private bool _selected;
		private bool _hovered;
		private readonly Font _glyphFont =
			new("Segoe UI Symbol", 14F, FontStyle.Regular);

		[Category("Synix Appearance")]
		public string IconGlyph { get; set; } = "•";

		[Category("Synix Appearance")]
		public bool Selected
		{
			get => _selected;
			set
			{
				_selected = value;
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
					? Color.FromArgb(22, 50, 67)
					: Color.FromArgb(16, 30, 48);
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

			Color textColor = Selected
				? SettingsPalette.Accent
				: SettingsPalette.SecondaryText;

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
	}

	[ToolboxItem(true)]
	public sealed class ModernSettingsGlyph : Control
	{
		[Category("Synix Appearance")]
		public string Glyph { get; set; } = "•";

		public ModernSettingsGlyph()
		{
			SetStyle(
				ControlStyles.UserPaint |
				ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer,
				true);

			BackColor = SettingsPalette.Card;
			ForeColor = SettingsPalette.Accent;
			Font = new Font("Segoe UI Symbol", 15F, FontStyle.Regular);
			Size = new Size(42, 42);
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			eventArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			Rectangle bounds = new(0, 0, Width - 1, Height - 1);
			using GraphicsPath path = RoundedGeometry.Create(bounds, 10);
			using SolidBrush fillBrush = new(SettingsPalette.AccentSoft);
			using Pen borderPen = new(Color.FromArgb(45, SettingsPalette.Accent), 1F);

			eventArgs.Graphics.FillPath(fillBrush, path);
			eventArgs.Graphics.DrawPath(borderPen, path);

			TextRenderer.DrawText(
				eventArgs.Graphics,
				Glyph,
				Font,
				ClientRectangle,
				ForeColor,
				TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
		}
	}
}
