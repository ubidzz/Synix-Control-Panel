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
}
