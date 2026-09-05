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
			SelectedItemBackColor = SettingsPalette.Selection;
			Margin = Padding.Empty;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[System.Diagnostics.CodeAnalysis.AllowNull]
		public object EditingControlFormattedValue
		{
			get => (SelectedItem == null ? Text : GetItemText(SelectedItem)) ?? string.Empty;
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
			return EditingControlFormattedValue ?? string.Empty;
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
			SelectedItemBackColor = SettingsPalette.Selection;
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

		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int EditingControlRowIndex { get; set; }

		public bool RepositionEditingControlOnValueChange => false;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DataGridView? EditingControlDataGridView
		{
			get => _editingDataGridView;
			set => _editingDataGridView = value;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
}
