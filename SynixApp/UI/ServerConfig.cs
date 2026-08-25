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
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.Database.GameConfigurations;
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixApp.UI;
using Synix_Control_Panel.SynixEngine;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;

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

		private static Color TextTypeColor => ThemeManager.IsDarkMode
			? Color.FromArgb(96, 165, 250)
			: Color.FromArgb(37, 99, 168);
		private static Color NumberTypeColor => ThemeManager.IsDarkMode
			? Color.FromArgb(167, 139, 250)
			: Color.FromArgb(109, 72, 184);
		private static Color BooleanTypeColor => SettingsPalette.Accent;
		private static Color SecretTypeColor => SettingsPalette.Warning;
		private static Color NullTypeColor => SettingsPalette.MutedText;

		private readonly string _path = string.Empty;
		private readonly ConfigFormat _format = ConfigFormat.StandardINI;
		private readonly GameServer? _server;
		private readonly bool _isRuntimeInstance;
		private List<ConfigLine> _fileData = new();
		private bool _rowsAreLoading;
		private bool _dataLoaded;
		private bool _allowClose;
		private bool _openBooleanDropDownOnEdit;
		private bool _booleanDropDownOpenQueued;
		private bool _templateResetInProgress;
		private int _booleanDropDownRowIndex = -1;

		public ServerConfig()
		{
			InitializeComponent();
			ConfigureBooleanGridEditing();
			if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
				ThemeManager.Apply(this);
		}

		public ServerConfig(string filePath, ConfigFormat format)
			: this(filePath, format, null)
		{
		}

		public ServerConfig(
			string filePath,
			ConfigFormat format,
			GameServer? server)
		{
			InitializeComponent();
			ConfigureBooleanGridEditing();
			ThemeManager.Apply(this);

			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("A configuration file path is required.", nameof(filePath));
			}

			_path = filePath;
			_format = format;
			_server = server;
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
			if (_templateResetInProgress)
			{
				eventArgs.Cancel = true;
				return;
			}

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
			btnFixConfig.Visible = _server != null &&
				GameFix.CanResetManagedConfiguration(_server);
			btnRestoreBackup.Visible = _server != null;
			btnValidateConfig.Visible = _server != null;
			btnValidateConfig.Enabled = _server != null;
			UpdateFixConfigAvailability();
			UpdateRestoreBackupAvailability();
		}

		private void LoadConfiguration()
		{
			_dataLoaded = true;

			try
			{
				if (!File.Exists(_path))
				{
					if (CanFixConfiguration())
					{
						ShowConfigurationRepairState(
							"The configuration file is missing. Use Fix Config to rebuild it from the Synix template.");
						return;
					}

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
				dgvConfig.Enabled = true;
				btnStructured.Enabled = true;
				btnRawPreview.Enabled = true;
				lblPreservationTitle.Text = "Original formatting is protected";
				lblPreservationText.Text =
					"Only the value you change is replaced; comments, sections, nesting, quotes, spacing, and key order remain intact.";
				PopulateGrid();
				ShowStructuredView();
				UpdateFixConfigAvailability();
			}
			catch (Exception exception)
			{
				if (CanFixConfiguration())
				{
					ShowConfigurationRepairState(
						$"Synix could not read this configuration. Use Fix Config to rebuild it. {exception.Message}");
					return;
				}

				MessageBox.Show(
					$"Synix could not read this configuration file.\n\n{exception.Message}",
					"Config Load Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				_allowClose = true;
				Close();
			}
		}

		private bool CanFixConfiguration()
		{
			return _server != null && GameFix.CanResetManagedConfiguration(_server);
		}

		private void UpdateFixConfigAvailability()
		{
			btnFixConfig.Enabled = btnFixConfig.Visible &&
				_server != null &&
				GameFix.NeedsManagedConfigurationRepair(_server);
		}

		private void UpdateRestoreBackupAvailability()
		{
			btnRestoreBackup.Enabled = btnRestoreBackup.Visible &&
				_server != null &&
				!IsServerBusy(_server) &&
				GameFix.HasManagedConfigurationBackup(_server);
		}

		private void ShowConfigurationRepairState(string message)
		{
			_fileData = [];
			PopulateGrid();
			ShowStructuredView();
			dgvConfig.Enabled = false;
			txtSearch.Enabled = false;
			cmbTypeFilter.Enabled = false;
			btnStructured.Enabled = false;
			btnRawPreview.Enabled = false;
			btnSave.Enabled = false;
			btnReset.Enabled = false;
			btnFixConfig.Enabled = true;
			btnValidateConfig.Enabled = _server != null;
			lblSettingCount.Text = "Config unavailable";
			lblFormatState.Text = "Repair available";
			lblPreservationTitle.Text = "Configuration repair is available";
			lblPreservationText.Text = message;
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
				if (_server != null && HasUnsavedChanges())
					_ = GameFix.BackupManagedConfiguration(
						_server,
						"Before saving changes from the configuration editor");
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

		private async Task ResetConfigurationFromTemplate()
		{
			if (_server == null || !CanFixConfiguration())
			{
				MessageBox.Show(
					"Synix does not have a complete reset template for this game.",
					"Config Template Unavailable",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			if (IsServerBusy(_server))
			{
				MessageBox.Show(
					"Stop this server before resetting its configuration.",
					"Server Must Be Stopped",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			bool fileExists = File.Exists(_path);
			string backupText = fileExists
				? "\n\nSynix will keep a .synix.bak copy of each configuration file it replaces."
				: string.Empty;
			DialogResult confirmation = MessageBox.Show(
				"This will rebuild the game configuration from the Synix default template and apply the values saved in Server Settings.\n\nAny other custom configuration values will be removed." +
				backupText +
				"\n\nContinue?",
				"Reset Config to Synix Defaults?",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning,
				MessageBoxDefaultButton.Button2);
			if (confirmation != DialogResult.Yes)
			{
				return;
			}

			_templateResetInProgress = true;
			UseWaitCursor = true;
			btnFixConfig.Enabled = false;
			btnReset.Enabled = false;
			btnCancel.Enabled = false;
			btnSave.Enabled = false;

			try
			{
				ConfigurationApplyResult result =
					await GameFix.ResetManagedConfiguration(_server);
				if (!result.Succeeded || !result.Complete)
				{
					MessageBox.Show(
						result.Message,
						"Config Reset Failed",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);
					return;
				}

				_ = FileHandler.SaveServers();
				LoadConfiguration();
				MessageBox.Show(
					result.Message +
					(fileExists
						? "\n\nThe previous configuration was saved with a .synix.bak extension."
						: string.Empty),
					"Config Reset Complete",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
			catch (Exception exception)
			{
				MessageBox.Show(
					$"Synix could not reset the configuration. The existing files were preserved when possible.\n\n{exception.Message}",
					"Config Reset Failed",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
			finally
			{
				_templateResetInProgress = false;
				UseWaitCursor = false;
				btnCancel.Enabled = true;
				UpdateFixConfigAvailability();
				UpdateRestoreBackupAvailability();
				if (File.Exists(_path) && _fileData.Count > 0)
				{
					btnSave.Enabled = true;
					UpdateChangePresentation();
				}
			}
		}

		private async Task ValidateSynixConfiguration()
		{
			if (_server == null)
				return;

			if (HasUnsavedChanges())
			{
				MessageBox.Show(
					this,
					"Save or undo the changes in the editor before checking the values stored on disk.",
					"Unsaved Changes",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			UseWaitCursor = true;
			btnValidateConfig.Enabled = false;
			try
			{
				ConfigurationValidationReport report =
					await GameFix.ValidateManagedConfiguration(_server);
				using ConfigurationValidationDialog dialog = new(report);
				dialog.ShowDialog(this);
			}
			catch (Exception exception)
			{
				MessageBox.Show(
					this,
					$"Synix could not finish the configuration check.\n\n{exception.Message}",
					"Configuration Check Failed",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
			finally
			{
				UseWaitCursor = false;
				btnValidateConfig.Enabled = _server != null;
			}
		}

		private void RestorePreviousConfiguration()
		{
			if (_server == null || IsServerBusy(_server))
			{
				MessageBox.Show(
					"Stop this server before restoring a configuration backup.",
					"Server Must Be Stopped",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			DialogResult confirmation = MessageBox.Show(
				"Restore the newest Synix configuration backup?\n\nSynix will first preserve the current files so this restore can also be undone.",
				"Restore Previous Configuration?",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning,
				MessageBoxDefaultButton.Button2);
			if (confirmation != DialogResult.Yes)
				return;

			ConfigurationRestoreResult result =
				GameFix.RestorePreviousManagedConfiguration(_server);
			if (!result.Succeeded)
			{
				MessageBox.Show(
					result.Message,
					"Configuration Restore Failed",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				return;
			}

			LoadConfiguration();
			UpdateRestoreBackupAvailability();
			MessageBox.Show(
				result.Message,
				"Configuration Restored",
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);
		}

		private static bool IsServerBusy(GameServer server)
		{
			string status = server.Status ?? string.Empty;
			return (server.PID.HasValue && server.PID.Value > 0) ||
				(server.SteamPID.HasValue && server.SteamPID.Value > 0) ||
				status == Core.StatusManager.GetStatus(Core.ServerState.Running) ||
				status == Core.StatusManager.GetStatus(Core.ServerState.Starting) ||
				status == Core.StatusManager.GetStatus(Core.ServerState.Stopping) ||
				status == Core.StatusManager.GetStatus(Core.ServerState.Installing) ||
				status == Core.StatusManager.GetStatus(Core.ServerState.Updating) ||
				status == Core.StatusManager.GetStatus(Core.ServerState.Validating);
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
			if (!ThemeManager.IsDarkMode)
			{
				return type switch
				{
					ConfigValueType.Boolean => Color.FromArgb(215, 240, 237),
					ConfigValueType.Number => Color.FromArgb(236, 228, 251),
					ConfigValueType.Secret => Color.FromArgb(248, 236, 208),
					ConfigValueType.Null => Color.FromArgb(227, 232, 239),
					_ => Color.FromArgb(221, 234, 248)
				};
			}

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

			dgvConfig.ClearSelection();
			dgvConfig.InvalidateCell(eventArgs.ColumnIndex, eventArgs.RowIndex);
		}

		private void dgvConfig_Scroll(object? sender, ScrollEventArgs eventArgs)
		{

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
				modernComboBox.SelectedItemBackColor = SettingsPalette.Selection;
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
			if (eventArgs.Graphics == null)
				return;
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

		private async void btnFixConfig_Click(object? sender, EventArgs eventArgs)
		{
			await ResetConfigurationFromTemplate();
		}

		private async void btnValidateConfig_Click(object? sender, EventArgs eventArgs)
		{
			await ValidateSynixConfiguration();
		}

		private void btnRestoreBackup_Click(object? sender, EventArgs eventArgs)
		{
			RestorePreviousConfiguration();
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
