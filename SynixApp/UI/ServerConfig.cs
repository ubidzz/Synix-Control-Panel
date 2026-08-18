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
using System.Reflection;
using System.Runtime.InteropServices;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.ServerHandler;

namespace Synix_Control_Panel.ServerHandler
{
	public partial class ServerConfig : Form
	{
		private const uint WdaExcludeFromCapture = 0x00000011;
		private const int WmNcHitTest = 0x0084;
		private const int WmNcLeftButtonDown = 0x00A1;
		private const int HtCaption = 0x0002;
		private const int HtLeft = 10;
		private const int HtRight = 11;
		private const int HtTop = 12;
		private const int HtTopLeft = 13;
		private const int HtTopRight = 14;
		private const int HtBottom = 15;
		private const int HtBottomLeft = 16;
		private const int HtBottomRight = 17;
		private const int DwmWindowCornerPreference = 33;
		private const int DwmRound = 2;
		private const int ResizeBorder = 7;

		private static readonly Color TextTypeColor = Color.FromArgb(96, 165, 250);
		private static readonly Color NumberTypeColor = Color.FromArgb(167, 139, 250);
		private static readonly Color BooleanTypeColor = Color.FromArgb(32, 214, 199);
		private static readonly Color SecretTypeColor = Color.FromArgb(245, 185, 76);
		private static readonly Color NullTypeColor = Color.FromArgb(148, 163, 184);

		private readonly string _path = string.Empty;
		private readonly ConfigFormat _format = ConfigFormat.StandardINI;
		private readonly bool _isRuntimeInstance;
		private List<ConfigLine> _fileData = new();
		private bool _rowsAreLoading;
		private bool _dataLoaded;
		private bool _allowClose;
		private bool _openBooleanDropDownOnEdit;
		private bool _booleanDropDownOpenQueued;
		private int _booleanDropDownRowIndex = -1;

		/// <summary>
		/// Parameterless constructor used by the Windows Forms Designer.
		/// The application should open this form with ServerConfig(string, ConfigFormat).
		/// </summary>
		public ServerConfig()
		{
			InitializeComponent();
			ConfigureBooleanGridEditing();
		}

		public ServerConfig(string filePath, ConfigFormat format)
		{
			InitializeComponent();
			ConfigureBooleanGridEditing();

			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("A configuration file path is required.", nameof(filePath));
			}

			_path = filePath;
			_format = format;
			_isRuntimeInstance = true;
			ConfigureFilePresentation();
		}

		private void ConfigureBooleanGridEditing()
		{
			dgvConfig.EditMode = DataGridViewEditMode.EditOnEnter;
			colSetting.ReadOnly = true;
			colType.ReadOnly = true;
			colSetting.CellTemplate = new ModernSettingsDataGridViewInformationalCell();
			colType.CellTemplate = new ModernSettingsDataGridViewInformationalCell();

			// InitializeComponent may already contain Designer event wiring. Remove
			// before adding so the reused grid/editor never receives duplicate calls.
			dgvConfig.CellMouseDown -= dgvConfig_CellMouseDown;
			dgvConfig.CellMouseDown += dgvConfig_CellMouseDown;
			dgvConfig.CellEnter -= dgvConfig_CellEnter;
			dgvConfig.CellEnter += dgvConfig_CellEnter;
			dgvConfig.EditingControlShowing -= dgvConfig_EditingControlShowing;
			dgvConfig.EditingControlShowing += dgvConfig_EditingControlShowing;
			dgvConfig.Scroll -= dgvConfig_Scroll;
			dgvConfig.Scroll += dgvConfig_Scroll;
		}

		protected override void OnShown(EventArgs eventArgs)
		{
			base.OnShown(eventArgs);

			EnableGridDoubleBuffering();
			if (_isRuntimeInstance && !_dataLoaded)
			{
				LoadConfiguration();
			}
		}

		protected override void OnHandleCreated(EventArgs eventArgs)
		{
			base.OnHandleCreated(eventArgs);

			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
				DesignMode ||
				Site?.DesignMode == true)
			{
				return;
			}

			if (Properties.Settings.Default.PrivacyMode)
			{
				_ = SetWindowDisplayAffinity(Handle, WdaExcludeFromCapture);
			}

			try
			{
				int preference = DwmRound;
				_ = DwmSetWindowAttribute(
					Handle,
					DwmWindowCornerPreference,
					ref preference,
					sizeof(int));
			}
			catch
			{
				// Rounded DWM corners are unavailable on older Windows versions.
			}
		}

		protected override void WndProc(ref Message message)
		{
			base.WndProc(ref message);

			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
				DesignMode ||
				Site?.DesignMode == true ||
				message.Msg != WmNcHitTest ||
				WindowState == FormWindowState.Maximized)
			{
				return;
			}

			Point cursor = PointToClient(Cursor.Position);
			bool left = cursor.X <= ResizeBorder;
			bool right = cursor.X >= ClientSize.Width - ResizeBorder;
			bool top = cursor.Y <= ResizeBorder;
			bool bottom = cursor.Y >= ClientSize.Height - ResizeBorder;

			if (left && top) message.Result = (IntPtr)HtTopLeft;
			else if (right && top) message.Result = (IntPtr)HtTopRight;
			else if (left && bottom) message.Result = (IntPtr)HtBottomLeft;
			else if (right && bottom) message.Result = (IntPtr)HtBottomRight;
			else if (left) message.Result = (IntPtr)HtLeft;
			else if (right) message.Result = (IntPtr)HtRight;
			else if (top) message.Result = (IntPtr)HtTop;
			else if (bottom) message.Result = (IntPtr)HtBottom;
		}

		protected override bool ProcessCmdKey(ref Message message, Keys keyData)
		{
			if (keyData == (Keys.Control | Keys.F) && dgvConfig.Visible)
			{
				txtSearch.Focus();
				txtSearch.SelectAll();
				return true;
			}

			if (keyData == (Keys.Control | Keys.S) && btnSave.Enabled)
			{
				SaveConfiguration();
				return true;
			}

			if (keyData == Keys.Escape)
			{
				Close();
				return true;
			}

			return base.ProcessCmdKey(ref message, keyData);
		}

		protected override void OnFormClosing(FormClosingEventArgs eventArgs)
		{
			dgvConfig.EndEdit();

			if (_isRuntimeInstance && !_allowClose && HasUnsavedChanges())
			{
				DialogResult result = MessageBox.Show(
					"You have unsaved configuration changes. Discard them and close?",
					"Discard Changes?",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning,
					MessageBoxDefaultButton.Button2);

				if (result != DialogResult.Yes)
				{
					eventArgs.Cancel = true;
				}
				else
				{
					_allowClose = true;
				}
			}

			base.OnFormClosing(eventArgs);
		}

		private void ConfigureFilePresentation()
		{
			string fileName = Path.GetFileName(_path);
			string formatName = ConfigHandler.GetFormatDisplayName(_format);

			Text = $"Config Editor - {fileName}";
			lblFileName.Text = fileName;
			lblFormatBadge.Text = formatName;
			lblPageSubtitle.Text =
				$"Edit {fileName} safely without changing its {formatName} structure.";
			lblFormatState.Text = $"{formatName} structure preserved";
		}

		private void LoadConfiguration()
		{
			_dataLoaded = true;

			try
			{
				if (!File.Exists(_path))
				{
					MessageBox.Show(
						$"The configuration file does not exist:\n\n{_path}",
						"File Not Found",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);
					_allowClose = true;
					Close();
					return;
				}

				_fileData = ConfigHandler.LoadConfig(_path, _format);
				PopulateGrid();
			}
			catch (Exception exception)
			{
				MessageBox.Show(
					$"Synix could not read this configuration file.\n\n{exception.Message}",
					"Config Load Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				_allowClose = true;
				Close();
			}
		}

		private void PopulateGrid()
		{
			_rowsAreLoading = true;
			dgvConfig.SuspendLayout();

			try
			{
				dgvConfig.Rows.Clear();
				foreach (ConfigLine line in _fileData)
				{
					int rowIndex = dgvConfig.Rows.Add();
					DataGridViewRow row = dgvConfig.Rows[rowIndex];
					row.Tag = line;
					row.Cells[colSetting.Index].Value = GetCleanSettingName(line);
					row.Cells[colSetting.Index].ToolTipText = line.Path;
					row.Cells[colType.Index].Value = GetTypeDisplayName(line.Type);

					if (line.Type == ConfigValueType.Boolean)
					{
						ModernSettingsDataGridViewComboBoxCell booleanCell = CreateBooleanCell();
						booleanCell.Value = NormalizeBooleanDisplay(line.Value);
						row.Cells[colValue.Index] = booleanCell;
					}
					else
					{
						row.Cells[colValue.Index].Value = line.Value;
					}
				}

				ApplyFilters();
				UpdateChangePresentation();
				btnSave.Enabled = _fileData.Count > 0;
			}
			finally
			{
				dgvConfig.ResumeLayout();
				_rowsAreLoading = false;
			}

			UpdateChangePresentation();
		}

		private static ModernSettingsDataGridViewComboBoxCell CreateBooleanCell()
		{
			ModernSettingsDataGridViewComboBoxCell cell = new()
			{
				DropDownWidth = 180
			};
			cell.Items.AddRange("True", "False");
			return cell;
		}

		private static string GetCleanSettingName(ConfigLine line)
		{
			string key = line.Key.Trim();
			if (!string.IsNullOrWhiteSpace(key) &&
				!string.Equals(key, line.Path, StringComparison.Ordinal))
			{
				return key;
			}

			string path = string.IsNullOrWhiteSpace(line.Path)
				? key
				: line.Path.Trim();
			int sectionSeparator = path.LastIndexOf(" / ", StringComparison.Ordinal);
			if (sectionSeparator >= 0)
			{
				path = path[(sectionSeparator + 3)..];
			}

			int leafSeparator = path.LastIndexOf('.');
			return leafSeparator >= 0 && leafSeparator + 1 < path.Length
				? path[(leafSeparator + 1)..]
				: path;
		}

		private static string NormalizeBooleanDisplay(string value)
		{
			return bool.TryParse(value, out bool parsed) && parsed ? "True" : "False";
		}

		private static string GetTypeDisplayName(ConfigValueType type)
		{
			return type switch
			{
				ConfigValueType.Boolean => "BOOLEAN",
				ConfigValueType.Number => "NUMBER",
				ConfigValueType.Secret => "SECRET",
				ConfigValueType.Null => "NULL",
				_ => "TEXT"
			};
		}

		private void ApplyFilters()
		{
			if (_rowsAreLoading && dgvConfig.Rows.Count == 0)
			{
				return;
			}

			string searchText = txtSearch.Text.Trim();
			string selectedType = cmbTypeFilter.SelectedItem?.ToString() ?? "All types";
			int visibleCount = 0;

			dgvConfig.CurrentCell = null;
			foreach (DataGridViewRow row in dgvConfig.Rows)
			{
				if (row.Tag is not ConfigLine line)
				{
					continue;
				}

				bool searchMatches = string.IsNullOrWhiteSpace(searchText) ||
					line.Key.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
					line.Path.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
					line.Section.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
					GetRowValue(row).Contains(searchText, StringComparison.OrdinalIgnoreCase);
				bool typeMatches = selectedType.Equals("All types", StringComparison.OrdinalIgnoreCase) ||
					GetTypeDisplayName(line.Type).Equals(selectedType, StringComparison.OrdinalIgnoreCase);

				row.Visible = searchMatches && typeMatches;
				if (row.Visible)
				{
					visibleCount++;
				}
			}

			lblSettingCount.Text = $"{visibleCount} of {_fileData.Count} settings";
		}

		private List<ConfigLine> CollectUpdatedData()
		{
			dgvConfig.EndEdit();
			List<ConfigLine> updatedData = new(dgvConfig.Rows.Count);

			foreach (DataGridViewRow row in dgvConfig.Rows)
			{
				if (row.Tag is not ConfigLine source)
				{
					continue;
				}

				updatedData.Add(new ConfigLine
				{
					Id = source.Id,
					Key = source.Key,
					Path = source.Path,
					Section = source.Section,
					OriginalValue = source.OriginalValue,
					HasOriginalValue = source.HasOriginalValue,
					Type = source.Type,
					Value = GetRowValue(row)
				});
			}

			return updatedData;
		}

		private bool HasUnsavedChanges()
		{
			if (!_dataLoaded || dgvConfig.Rows.Count == 0)
			{
				return false;
			}

			return dgvConfig.Rows.Cast<DataGridViewRow>().Any(RowHasChanged);
		}

		private static bool ValuesMatch(ConfigLine source, string currentValue)
		{
			if (source.Type == ConfigValueType.Boolean &&
				bool.TryParse(source.OriginalValue, out bool originalBoolean) &&
				bool.TryParse(currentValue, out bool currentBoolean))
			{
				return originalBoolean == currentBoolean;
			}

			return string.Equals(source.OriginalValue, currentValue, StringComparison.Ordinal);
		}

		private static string GetRowValue(DataGridViewRow row)
		{
			return row.Cells[2].Value?.ToString() ?? string.Empty;
		}

		private static bool RowHasChanged(DataGridViewRow row)
		{
			return row.Tag is ConfigLine source && !ValuesMatch(source, GetRowValue(row));
		}

		private void UpdateChangePresentation()
		{
			if (_rowsAreLoading)
			{
				return;
			}

			int changedCount = dgvConfig.Rows
				.Cast<DataGridViewRow>()
				.Count(RowHasChanged);
			lblModifiedCount.Text = changedCount == 1
				? "1 unsaved change"
				: $"{changedCount} unsaved changes";
			lblModifiedCount.ForeColor = changedCount > 0
				? SettingsPalette.Warning
				: SettingsPalette.MutedText;
			btnReset.Enabled = changedCount > 0;
		}

		private void ResetChanges()
		{
			_rowsAreLoading = true;
			try
			{
				foreach (DataGridViewRow row in dgvConfig.Rows)
				{
					if (row.Tag is ConfigLine source)
					{
						row.Cells[colValue.Index].Value = source.Type == ConfigValueType.Boolean
							? NormalizeBooleanDisplay(source.OriginalValue)
							: source.OriginalValue;
					}
				}
			}
			finally
			{
				_rowsAreLoading = false;
			}

			UpdateChangePresentation();
			ApplyFilters();
		}

		private void ShowStructuredView()
		{
			txtRawPreview.Visible = false;
			dgvConfig.Visible = true;
			dgvConfig.BringToFront();
			txtSearch.Enabled = true;
			cmbTypeFilter.Enabled = true;
			btnStructured.ForeColor = SettingsPalette.Accent;
			btnRawPreview.ForeColor = SettingsPalette.SecondaryText;
		}

		private void ShowRawPreview()
		{
			try
			{
				txtRawPreview.Text = ConfigHandler.CreatePreview(
					_path,
					CollectUpdatedData(),
					_format);
				dgvConfig.Visible = false;
				txtRawPreview.Visible = true;
				txtRawPreview.BringToFront();
				txtSearch.Enabled = false;
				cmbTypeFilter.Enabled = false;
				btnStructured.ForeColor = SettingsPalette.SecondaryText;
				btnRawPreview.ForeColor = SettingsPalette.Accent;
			}
			catch (Exception exception)
			{
				MessageBox.Show(
					$"Synix could not build a safe preview.\n\n{exception.Message}",
					"Preview Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
		}

		private void SaveConfiguration()
		{
			try
			{
				ConfigHandler.SaveConfig(_path, CollectUpdatedData(), _format);
				_allowClose = true;
				DialogResult = DialogResult.OK;
				Close();
			}
			catch (Exception exception)
			{
				MessageBox.Show(
					$"The configuration was not saved. The original file is unchanged.\n\n{exception.Message}",
					"Config Save Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
		}

		private void EnableGridDoubleBuffering()
		{
			PropertyInfo? doubleBufferedProperty = typeof(DataGridView).GetProperty(
				"DoubleBuffered",
				BindingFlags.NonPublic | BindingFlags.Instance);
			doubleBufferedProperty?.SetValue(dgvConfig, true, null);
		}

		private static Color GetTypeColor(ConfigValueType type)
		{
			return type switch
			{
				ConfigValueType.Boolean => BooleanTypeColor,
				ConfigValueType.Number => NumberTypeColor,
				ConfigValueType.Secret => SecretTypeColor,
				ConfigValueType.Null => NullTypeColor,
				_ => TextTypeColor
			};
		}

		private static Color GetTypeBadgeColor(ConfigValueType type)
		{
			return type switch
			{
				ConfigValueType.Boolean => Color.FromArgb(15, 61, 66),
				ConfigValueType.Number => Color.FromArgb(48, 39, 77),
				ConfigValueType.Secret => Color.FromArgb(68, 52, 24),
				ConfigValueType.Null => Color.FromArgb(40, 48, 61),
				_ => Color.FromArgb(24, 48, 72)
			};
		}

		private void TitleBar_MouseDown(object? sender, MouseEventArgs eventArgs)
		{
			if (eventArgs.Button != MouseButtons.Left)
			{
				return;
			}

			_ = ReleaseCapture();
			_ = SendMessage(Handle, WmNcLeftButtonDown, (IntPtr)HtCaption, IntPtr.Zero);
		}

		private void btnMinimize_Click(object? sender, EventArgs eventArgs)
		{
			WindowState = FormWindowState.Minimized;
		}

		private void btnClose_Click(object? sender, EventArgs eventArgs)
		{
			Close();
		}

		private void txtSearch_TextChanged(object? sender, EventArgs eventArgs)
		{
			ApplyFilters();
		}

		private void cmbTypeFilter_SelectedIndexChanged(object? sender, EventArgs eventArgs)
		{
			ApplyFilters();
		}

		private void btnStructured_Click(object? sender, EventArgs eventArgs)
		{
			ShowStructuredView();
		}

		private void btnRawPreview_Click(object? sender, EventArgs eventArgs)
		{
			ShowRawPreview();
		}

		private void dgvConfig_CurrentCellDirtyStateChanged(object? sender, EventArgs eventArgs)
		{
			if (dgvConfig.IsCurrentCellDirty &&
				dgvConfig.CurrentCell is DataGridViewComboBoxCell)
			{
				dgvConfig.CommitEdit(DataGridViewDataErrorContexts.Commit);
			}
		}

		private void dgvConfig_CellMouseDown(
			object? sender,
			DataGridViewCellMouseEventArgs eventArgs)
		{
			if (eventArgs.RowIndex >= 0 &&
				IsInformationalColumn(eventArgs.ColumnIndex))
			{
				dgvConfig.ClearSelection();
				dgvConfig.InvalidateCell(eventArgs.ColumnIndex, eventArgs.RowIndex);
				return;
			}

			if (eventArgs.Button != MouseButtons.Left ||
				!IsBooleanValueCell(eventArgs.ColumnIndex, eventArgs.RowIndex))
			{
				return;
			}

			_openBooleanDropDownOnEdit = true;
			_booleanDropDownRowIndex = eventArgs.RowIndex;

			DataGridViewCell targetCell =
				dgvConfig.Rows[eventArgs.RowIndex].Cells[eventArgs.ColumnIndex];
			if (!ReferenceEquals(dgvConfig.CurrentCell, targetCell))
			{
				dgvConfig.CurrentCell = targetCell;
			}

			dgvConfig.Focus();
			if (!dgvConfig.IsCurrentCellInEditMode)
			{
				_ = dgvConfig.BeginEdit(true);
			}

			QueueBooleanDropDownOpen();
		}

		private void dgvConfig_CellEnter(
			object? sender,
			DataGridViewCellEventArgs eventArgs)
		{
			if (eventArgs.RowIndex < 0 ||
				!IsInformationalColumn(eventArgs.ColumnIndex))
			{
				return;
			}

			// WinForms has no per-column Selectable property. Keep these read-only
			// label/badge cells out of the SelectedCells collection, while their
			// custom cell painter also suppresses the current-cell focus rectangle.
			dgvConfig.ClearSelection();
			dgvConfig.InvalidateCell(eventArgs.ColumnIndex, eventArgs.RowIndex);
		}

		private void dgvConfig_Scroll(object? sender, ScrollEventArgs eventArgs)
		{
			// A queued first-click request must not reopen a list after its row has
			// moved. Closing and ending the active Boolean edit also removes the
			// editing panel before WinForms reuses the viewport during scrolling.
			_openBooleanDropDownOnEdit = false;
			_booleanDropDownRowIndex = -1;

			if (dgvConfig.EditingControl is not ComboBox comboBox)
			{
				return;
			}

			comboBox.DroppedDown = false;
			if (dgvConfig.IsCurrentCellDirty)
			{
				dgvConfig.CommitEdit(DataGridViewDataErrorContexts.Commit);
			}

			if (dgvConfig.IsCurrentCellInEditMode && !dgvConfig.EndEdit())
			{
				dgvConfig.CancelEdit();
			}

			dgvConfig.Invalidate();
		}

		private void dgvConfig_EditingControlShowing(
			object? sender,
			DataGridViewEditingControlShowingEventArgs eventArgs)
		{
			if (!IsCurrentBooleanValueCell() ||
				eventArgs.Control is not ComboBox comboBox)
			{
				return;
			}

			ApplyBooleanEditingStyle(comboBox, eventArgs.CellStyle);
			if (_openBooleanDropDownOnEdit)
			{
				QueueBooleanDropDownOpen();
			}
		}

		private bool IsCurrentBooleanValueCell()
		{
			DataGridViewCell? currentCell = dgvConfig.CurrentCell;
			return currentCell != null &&
				IsBooleanValueCell(currentCell.ColumnIndex, currentCell.RowIndex);
		}

		private bool IsInformationalColumn(int columnIndex)
		{
			return columnIndex == colSetting.Index ||
				columnIndex == colType.Index;
		}

		private bool IsBooleanValueCell(int columnIndex, int rowIndex)
		{
			return rowIndex >= 0 &&
				rowIndex < dgvConfig.Rows.Count &&
				columnIndex == colValue.Index &&
				dgvConfig.Rows[rowIndex].Tag is ConfigLine line &&
				line.Type == ConfigValueType.Boolean &&
				!dgvConfig.Rows[rowIndex].Cells[columnIndex].ReadOnly;
		}

		private void ApplyBooleanEditingStyle(
			ComboBox comboBox,
			DataGridViewCellStyle cellStyle)
		{
			cellStyle.BackColor = SettingsPalette.Input;
			cellStyle.ForeColor = SettingsPalette.PrimaryText;
			cellStyle.SelectionBackColor = SettingsPalette.Input;
			cellStyle.SelectionForeColor = SettingsPalette.PrimaryText;
			cellStyle.Padding = Padding.Empty;

			comboBox.BackColor = SettingsPalette.Input;
			comboBox.ForeColor = SettingsPalette.PrimaryText;
			comboBox.FlatStyle = FlatStyle.Flat;
			comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			comboBox.Dock = DockStyle.None;
			comboBox.Margin = Padding.Empty;
			comboBox.Padding = Padding.Empty;
			comboBox.Cursor = Cursors.Hand;

			Panel editingPanel = dgvConfig.EditingPanel;
			editingPanel.BackColor = SettingsPalette.Input;
			editingPanel.BorderStyle = BorderStyle.None;
			editingPanel.Margin = Padding.Empty;
			editingPanel.Padding = Padding.Empty;
			if (comboBox.Parent is Panel parentPanel &&
				!ReferenceEquals(parentPanel, editingPanel))
			{
				parentPanel.BackColor = SettingsPalette.Input;
				parentPanel.BorderStyle = BorderStyle.None;
				parentPanel.Margin = Padding.Empty;
				parentPanel.Padding = Padding.Empty;
			}

			if (comboBox is ModernSettingsComboBox modernComboBox)
			{
				modernComboBox.BorderColor = SettingsPalette.Border;
				modernComboBox.FocusBorderColor = SettingsPalette.Border;
				modernComboBox.ArrowColor = SettingsPalette.SecondaryText;
				modernComboBox.SelectedItemBackColor = Color.FromArgb(24, 55, 73);
				modernComboBox.Invalidate();
			}

			if (comboBox is ModernSettingsDataGridViewComboBoxEditingControl gridEditor)
			{
				gridEditor.PrepareForFullCellHeight(
					dgvConfig.CurrentCell?.Size.Height ?? dgvConfig.RowTemplate.Height);
			}
		}

		private void QueueBooleanDropDownOpen()
		{
			if (_booleanDropDownOpenQueued ||
				!_openBooleanDropDownOnEdit ||
				!dgvConfig.IsHandleCreated)
			{
				return;
			}

			_booleanDropDownOpenQueued = true;
			dgvConfig.BeginInvoke(new Action(OpenPendingBooleanDropDown));
		}

		private void OpenPendingBooleanDropDown()
		{
			_booleanDropDownOpenQueued = false;
			int targetRowIndex = _booleanDropDownRowIndex;
			_openBooleanDropDownOnEdit = false;
			_booleanDropDownRowIndex = -1;

			if (IsDisposed || Disposing)
			{
				return;
			}

			DataGridViewCell? currentCell = dgvConfig.CurrentCell;
			if (targetRowIndex < 0 ||
				currentCell == null ||
				currentCell.RowIndex != targetRowIndex ||
				!IsCurrentBooleanValueCell())
			{
				return;
			}

			if (!dgvConfig.IsCurrentCellInEditMode && !dgvConfig.BeginEdit(true))
			{
				return;
			}

			if (dgvConfig.EditingControl is ComboBox comboBox)
			{
				ApplyBooleanEditingStyle(comboBox, currentCell.InheritedStyle);
				comboBox.Focus();
				comboBox.DroppedDown = true;
			}
		}

		private void dgvConfig_CellValueChanged(
			object? sender,
			DataGridViewCellEventArgs eventArgs)
		{
			if (!_rowsAreLoading && eventArgs.RowIndex >= 0)
			{
				UpdateChangePresentation();
			}
		}

		private void dgvConfig_CellPainting(
			object? sender,
			DataGridViewCellPaintingEventArgs eventArgs)
		{
			if (eventArgs.RowIndex < 0)
			{
				return;
			}

			if (eventArgs.ColumnIndex == colSetting.Index)
			{
				eventArgs.PaintBackground(eventArgs.ClipBounds, false);
				eventArgs.PaintContent(eventArgs.ClipBounds);
				eventArgs.Handled = true;
				return;
			}

			if (eventArgs.ColumnIndex != colType.Index ||
				dgvConfig.Rows[eventArgs.RowIndex].Tag is not ConfigLine line)
			{
				return;
			}

			eventArgs.PaintBackground(eventArgs.ClipBounds, false);
			Color typeColor = GetTypeColor(line.Type);
			Rectangle badgeBounds = new(
				eventArgs.CellBounds.X + 12,
				eventArgs.CellBounds.Y + 13,
				Math.Min(92, eventArgs.CellBounds.Width - 24),
				26);
			using SolidBrush badgeBrush = new(GetTypeBadgeColor(line.Type));
			eventArgs.Graphics.FillRectangle(badgeBrush, badgeBounds);
			TextRenderer.DrawText(
				eventArgs.Graphics,
				GetTypeDisplayName(line.Type),
				colType.DefaultCellStyle.Font ?? dgvConfig.Font,
				badgeBounds,
				typeColor,
				TextFormatFlags.HorizontalCenter |
				TextFormatFlags.VerticalCenter |
				TextFormatFlags.EndEllipsis);
			eventArgs.Handled = true;
		}

		private void dgvConfig_DataError(
			object? sender,
			DataGridViewDataErrorEventArgs eventArgs)
		{
			eventArgs.ThrowException = false;
		}

		private void btnReset_Click(object? sender, EventArgs eventArgs)
		{
			ResetChanges();
		}

		private void btnCancel_Click(object? sender, EventArgs eventArgs)
		{
			Close();
		}

		private void btnSave_Click(object? sender, EventArgs eventArgs)
		{
			SaveConfiguration();
		}

		[DllImport("user32.dll")]
		private static extern bool ReleaseCapture();

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(
			IntPtr windowHandle,
			uint message,
			IntPtr wordParameter,
			IntPtr longParameter);

		[DllImport("user32.dll")]
		private static extern bool SetWindowDisplayAffinity(
			IntPtr windowHandle,
			uint affinity);

		[DllImport("dwmapi.dll")]
		private static extern int DwmSetWindowAttribute(
			IntPtr windowHandle,
			int attribute,
			ref int attributeValue,
			int attributeSize);
	}
}
