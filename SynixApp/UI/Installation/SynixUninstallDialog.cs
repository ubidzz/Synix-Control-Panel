using Synix_Control_Panel.SynixEngine;

namespace Synix_Control_Panel.SynixApp.UI.Installation;

/// <summary>Separate opt-in consent and exact folder preview for MSI data removal.</summary>
internal sealed class SynixUninstallDialog : Form
{
	private readonly RadioButton _keep;
	private readonly RadioButton _all;
	private readonly CheckBox _confirm;
	private readonly CheckBox _backups;
	private readonly TextBox _preview;
	private readonly Label _description;
	private readonly Button _remove;
	private readonly Button _cancel;
	private readonly Func<bool, SynixUninstallPlan> _createPlan;
	private readonly Func<SynixUninstallPlan, Task> _removeData;
	private readonly Action _prepareAppRemoval;
	private SynixUninstallPlan? _plan;
	private int _request;
	private bool _busy;
	private bool _changingChoices;

	internal SynixUninstallDialog(Func<bool, SynixUninstallPlan> createPlan,
		Func<SynixUninstallPlan, Task> removeData, Action prepareAppRemoval)
	{
		_createPlan = createPlan;
		_removeData = removeData;
		_prepareAppRemoval = prepareAppRemoval;
		Text = LocalizationManager.Get("Uninstall.Title");
		Font = new Font("Segoe UI", 10F);
		AutoScaleMode = AutoScaleMode.Dpi;
		StartPosition = FormStartPosition.CenterScreen;
		ClientSize = new Size(800, 640);
		MinimumSize = new Size(700, 580);
		MinimizeBox = false;
		MaximizeBox = false;

		TableLayoutPanel layout = new() { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 1, RowCount = 7 };
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		for (int row = 0; row < 7; row++) layout.RowStyles.Add(new RowStyle(row == 4 ? SizeType.Percent : SizeType.AutoSize, row == 4 ? 100 : 0));
		_keep = new() { Name = "keepServerData", Text = LocalizationManager.Get("Uninstall.AppOnly"), Checked = true, Dock = DockStyle.Fill };
		_all = new() { Name = "removeServerData", Text = LocalizationManager.Get("Uninstall.Everything"), Dock = DockStyle.Fill };
		_description = new() { Name = "uninstallDescription", Text = LocalizationManager.Get("Uninstall.Keep"), Dock = DockStyle.Fill, Margin = new Padding(3, 14, 3, 14) };
		_preview = new() { Name = "removalPaths", Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false, Dock = DockStyle.Fill, TabStop = true };
		_confirm = new() { Name = "confirmDataRemoval", Text = LocalizationManager.Get("Uninstall.Confirm"), Dock = DockStyle.Fill, Enabled = false, Margin = new Padding(3, 14, 3, 14) };
		_backups = new() { Name = "deleteBackups", Text = LocalizationManager.Get("Uninstall.BackupsOption"), Dock = DockStyle.Fill, Visible = false, Enabled = false };
		FlowLayoutPanel buttons = new() { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
		_cancel = new() { Text = LocalizationManager.Get("Text.19766ED6CCB2F4A32778"), AutoSize = true, MinimumSize = new Size(110, 36), DialogResult = DialogResult.Cancel };
		_remove = new() { Name = "uninstallButton", Text = LocalizationManager.Get("Uninstall.Remove"), AutoSize = true, MinimumSize = new Size(140, 36) };
		buttons.Controls.AddRange([_cancel, _remove]);
		layout.Controls.Add(_keep, 0, 0);
		layout.Controls.Add(_all, 0, 1);
		layout.Controls.Add(_description, 0, 2);
		layout.Controls.Add(_backups, 0, 3);
		layout.Controls.Add(_preview, 0, 4);
		layout.Controls.Add(_confirm, 0, 5);
		layout.Controls.Add(buttons, 0, 6);
		Controls.Add(layout);
		// Measure text rows explicitly so long translations, including the server
		// preservation explanation, remain visible at the minimum window size.
		void SizeChoiceRows()
		{
			foreach ((Control control, int row) in new[] { (_keep as Control, 0), (_all as Control, 1), (_description as Control, 2), (_backups as Control, 3), (_confirm as Control, 5) })
			{
				int width = Math.Max(1, layout.ClientSize.Width - layout.Padding.Horizontal - control.Margin.Horizontal - (control is Label ? 0 : 22));
				int height = TextRenderer.MeasureText(control.Text, control.Font, new Size(width, int.MaxValue),
					TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height + 8;
				layout.RowStyles[row].SizeType = SizeType.Absolute;
				layout.RowStyles[row].Height = control == _backups && !_backups.Visible ? 0 : height + control.Margin.Vertical;
			}
		}
		layout.SizeChanged += (_, _) => SizeChoiceRows();
		Shown += (_, _) => SizeChoiceRows();
		_backups.VisibleChanged += (_, _) => SizeChoiceRows();
		_description.TextChanged += (_, _) => SizeChoiceRows();
		SizeChoiceRows();
		CancelButton = _cancel;
		// No AcceptButton: pressing Enter must never authorize data deletion.
		_all.CheckedChanged += async (_, _) =>
		{
			_changingChoices = true;
			_backups.Checked = false;
			_changingChoices = false;
			await RefreshPreviewAsync();
		};
		_backups.CheckedChanged += async (_, _) => { if (!_changingChoices) await RefreshPreviewAsync(); };
		_confirm.CheckedChanged += (_, _) => UpdateButtons();
		_remove.Click += async (_, _) => await RemoveAsync();
		FormClosing += (_, e) => { if (_busy) e.Cancel = true; };
	}

	private async Task RefreshPreviewAsync()
	{
		int request = ++_request;
		_plan = null;
		_confirm.Checked = false;
		_confirm.Enabled = false;
		_backups.Enabled = false;
		if (!_all.Checked) _backups.Visible = false;
		_preview.Clear();
		_description.Text = LocalizationManager.Get(_all.Checked ? "Uninstall.Review" : "Uninstall.Keep");
		UpdateButtons();
		if (!_all.Checked) return;
		try
		{
			bool deleteBackups = _backups.Checked;
			SynixUninstallPlan plan = await Task.Run(() => _createPlan(deleteBackups));
			if (IsDisposed || request != _request) return;
			_plan = plan;
			_backups.Visible = _backups.Enabled = plan.HasBackups;
			_preview.Text = string.Join(Environment.NewLine, plan.Files.Concat(plan.Folders)) + Environment.NewLine + Environment.NewLine +
				LocalizationManager.Get(!plan.HasBackups ? "Uninstall.NoBackups" :
					plan.DeletesBackups ? "Uninstall.BackupsDelete" : "Uninstall.BackupsKeep") + Environment.NewLine +
				string.Join(Environment.NewLine, plan.BackupFolders.Concat(plan.BackupFiles)) + Environment.NewLine + Environment.NewLine +
				LocalizationManager.Get("Uninstall.KeptLocations") + Environment.NewLine +
				string.Join(Environment.NewLine, plan.KeptFolders.Where(path => !plan.DeletesBackups || !plan.BackupFolders.Contains(path, StringComparer.OrdinalIgnoreCase)));
			_confirm.Enabled = true;
		}
		catch (Exception exception)
		{
			if (!IsDisposed && request == _request) _preview.Text = exception.Message;
		}
		if (!IsDisposed) UpdateButtons();
	}

	private void UpdateButtons() => _remove.Enabled = !_busy &&
		(_keep.Checked || (_plan != null && _confirm.Checked));

	private async Task RemoveAsync()
	{
		if (!_remove.Enabled) return;
		bool removeData = _all.Checked;
		if (removeData && MessageBox.Show(this, LocalizationManager.Get("Uninstall.Review"), Text,
			MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
		_busy = true;
		_keep.Enabled = _all.Enabled = _confirm.Enabled = _backups.Enabled = _cancel.Enabled = false;
		UseWaitCursor = true;
		UpdateButtons();
		try
		{
			if (removeData) await _removeData(_plan!);
			else await Task.Run(_prepareAppRemoval);
			_busy = false;
			DialogResult = DialogResult.OK;
			Close();
		}
		catch (Exception exception)
		{
			_busy = false;
			UseWaitCursor = false;
			_keep.Enabled = _all.Enabled = _cancel.Enabled = true;
			MessageBox.Show(this, LocalizationManager.Get("Uninstall.Failed") + Environment.NewLine + exception.Message,
				Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			// A partial failure or changed registry requires a fresh preview and consent.
			await RefreshPreviewAsync();
		}
	}
}
