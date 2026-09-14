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
using Synix_Control_Panel.SynixEngine.ModManagement;

namespace Synix_Control_Panel.SynixApp.UI.ServerManagement;

internal sealed class ArkModPackageDialog : Form
{
	private readonly IReadOnlyList<string> _currentIds;
	private readonly int _maximumIds;
	private readonly string _game;
	private readonly TextBox _source;
	private readonly DataGridView _grid;
	private readonly RichTextBox _details;
	private readonly Label _status;
	private readonly ModernSettingsButton _zip, _folder, _review;
	private CancellationTokenSource? _reading;
	internal IReadOnlyList<string> ModIds { get; private set; } = [];

	internal ArkModPackageDialog(IReadOnlyList<string> currentIds, int maximumIds, string game = ArkModPackageReader.Ascended)
	{
		_currentIds = currentIds.ToArray();
		_maximumIds = maximumIds;
		_game = game;
		bool evolved = game.Equals(ArkModPackageReader.Evolved, StringComparison.OrdinalIgnoreCase);
		Text = LocalizationManager.Get(evolved ? "AsePackages.Title" : "AsaPackages.Title");
		Name = "arkModPackageDialog";
		StartPosition = FormStartPosition.CenterParent;
		ShowInTaskbar = false;
		MinimizeBox = false;
		ClientSize = new Size(940, 730);
		MinimumSize = new Size(880, 700);
		BackColor = SettingsPalette.Window;
		ForeColor = SettingsPalette.PrimaryText;
		Font = new Font("Segoe UI", 10F);
		TableLayoutPanel layout = new() { Dock = DockStyle.Fill, Padding = new Padding(24), ColumnCount = 1, RowCount = 9 };
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		foreach (float height in new[] { 48F, 110F, 42F, 52F }) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
		layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		foreach (float height in new[] { 126F, 68F, 40F, 48F }) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
		Controls.Add(layout);
		layout.Controls.Add(new Label { Text = Text, Dock = DockStyle.Fill, Font = new Font(Font.FontFamily, 18F, FontStyle.Bold), UseMnemonic = false }, 0, 0);
		layout.Controls.Add(new Label { Text = LocalizationManager.Get(evolved ? "AsePackages.Help" : "AsaPackages.Help"), Dock = DockStyle.Fill,
			Name = "arkPackageHelp", ForeColor = SettingsPalette.SecondaryText, UseMnemonic = false }, 0, 1);
		ModernSettingsCard sourceCard = new() { Dock = DockStyle.Fill, Padding = new Padding(10), FillColor = SettingsPalette.Input };
		_source = new TextBox { Name = "arkPackageSource", ReadOnly = true, Dock = DockStyle.Fill, BorderStyle = BorderStyle.None,
			BackColor = SettingsPalette.Input, ForeColor = SettingsPalette.PrimaryText, AccessibleName = LocalizationManager.Get("AsaPackages.Source") };
		sourceCard.Controls.Add(_source);
		layout.Controls.Add(sourceCard, 0, 2);
		FlowLayoutPanel choices = new() { Dock = DockStyle.Fill, WrapContents = false };
		_zip = Button("EmpyrionMods.Picker.Zip", "arkPackageZip");
		_folder = Button("EmpyrionMods.Picker.Folder", "arkPackageFolder");
		_zip.Click += async (_, _) => await ChooseSourceAsync(folder: false);
		_folder.Click += async (_, _) => await ChooseSourceAsync(folder: true);
		choices.Controls.AddRange([_zip, _folder]);
		layout.Controls.Add(choices, 0, 3);
		_grid = new DataGridView { Name = "arkPackagePreview", Dock = DockStyle.Fill, ReadOnly = true,
			AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false, RowHeadersVisible = false,
			AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
			SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };
		foreach ((string name, string key, float weight) in new[] { ("Name", "ModManager.Column.AddOn", 30F),
			("Id", "AsaPackages.Id", 15F), ("Version", "ModManager.Column.Version", 25F), ("Descriptor", "AsaPackages.Descriptor", 30F) })
			_grid.Columns.Add(new DataGridViewTextBoxColumn { Name = name, HeaderText = LocalizationManager.Get(key), FillWeight = weight, SortMode = DataGridViewColumnSortMode.NotSortable });
		GridStyler.DarkTheme(_grid);
		GridStyler.ApplyDashboardTheme(_grid);
		_grid.DefaultCellStyle = new DataGridViewCellStyle(_grid.DefaultCellStyle)
			{ WrapMode = DataGridViewTriState.True, Alignment = DataGridViewContentAlignment.TopLeft, Padding = new Padding(8, 8, 6, 8) };
		foreach (DataGridViewColumn column in _grid.Columns)
			column.DefaultCellStyle = new DataGridViewCellStyle(column.DefaultCellStyle) { WrapMode = DataGridViewTriState.True };
		layout.Controls.Add(_grid, 0, 4);
		ModernSettingsCard detailCard = new() { Dock = DockStyle.Fill, Padding = new Padding(10), FillColor = SettingsPalette.Input };
		_details = new RichTextBox { Name = "arkPackageDetails", Dock = DockStyle.Fill, ReadOnly = true,
			Multiline = true, WordWrap = true, DetectUrls = false, ScrollBars = RichTextBoxScrollBars.Vertical,
			BorderStyle = BorderStyle.None, BackColor = SettingsPalette.Input, ForeColor = SettingsPalette.PrimaryText };
		detailCard.Controls.Add(_details);
		layout.Controls.Add(detailCard, 0, 5);
		_grid.CurrentCellChanged += (_, _) => UpdateDetails();
		_status = new Label { Name = "arkPackageStatus", Dock = DockStyle.Fill, UseMnemonic = false,
			Text = LocalizationManager.Get("AsaPackages.Choose"), ForeColor = SettingsPalette.SecondaryText };
		layout.Controls.Add(_status, 0, 6);
		layout.Controls.Add(new Label { Text = LocalizationManager.Get("AsaPackages.Trust"), Dock = DockStyle.Fill,
			ForeColor = SettingsPalette.Warning, UseMnemonic = false }, 0, 7);
		FlowLayoutPanel actions = new() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
		_review = Button("AsaPackages.Review", "arkPackageReview");
		_review.Enabled = false;
		_review.UseAccentStyle = true;
		_review.Click += (_, _) => { if (_reading == null && ModIds.Count > 0) { DialogResult = DialogResult.OK; Close(); } };
		ModernSettingsButton cancel = Button("Text.19766ED6CCB2F4A32778", "arkPackageCancel");
		cancel.DialogResult = DialogResult.Cancel;
		CancelButton = cancel;
		actions.Controls.AddRange([_review, cancel]);
		layout.Controls.Add(actions, 0, 8);
		ThemeManager.Apply(this);
	}

	private async Task ChooseSourceAsync(bool folder)
	{
		if (_reading != null) return;
		string source;
		if (folder)
		{
			using FolderBrowserDialog picker = new() { ShowNewFolderButton = false, UseDescriptionForTitle = true,
				Description = LocalizationManager.Get("ModPackages.Picker.SelectFolder") };
			if (picker.ShowDialog(this) != DialogResult.OK) return;
			source = picker.SelectedPath;
		}
		else
		{
			using OpenFileDialog picker = new() { CheckFileExists = true, Filter = LocalizationManager.Get("ModManager.FileFilter.ArchiveOnly") };
			if (picker.ShowDialog(this) != DialogResult.OK) return;
			source = picker.FileName;
		}
		await ReadSourceAsync(source);
	}

	internal async Task ReadSourceAsync(string source)
	{
		if (_reading != null) return;
		using CancellationTokenSource reading = new(TimeSpan.FromSeconds(60));
		_reading = reading;
		ModIds = [];
		_grid.Rows.Clear();
		_details.Clear();
		_review.Enabled = _zip.Enabled = _folder.Enabled = false;
		_source.Text = source;
		_status.Text = LocalizationManager.Get("AsaPackages.Reading");
		_status.ForeColor = SettingsPalette.SecondaryText;
		try
		{
			IReadOnlyList<ArkModPackageInfo> mods = await Task.Run(() => ArkModPackageReader.Read(source, _game, reading.Token), reading.Token);
			if (IsDisposed || reading.IsCancellationRequested) return;
			ModIds = ModPackageManager.NormalizeProviderIds(string.Join(',', _currentIds.Concat(mods.Select(mod => mod.ModId))), _maximumIds);
			foreach (ArkModPackageInfo mod in mods)
			{
				int row = _grid.Rows.Add(mod.Name, mod.ModId, mod.Version, mod.DescriptorPath);
				_grid.Rows[row].Tag = mod;
				_grid.Rows[row].Cells["Descriptor"].ToolTipText = mod.DescriptorPath;
			}
			UpdateDetails();
			_status.Text = LocalizationManager.Get("AsaPackages.Found", mods.Count, ModIds.Count - _currentIds.Distinct().Count());
			_review.Enabled = true;
		}
		catch (Exception exception)
		{
			if (IsDisposed) return;
			ModIds = [];
			_status.Text = exception is OperationCanceledException ? LocalizationManager.Get("AsaPackages.Timeout") : exception.Message;
			_status.ForeColor = SettingsPalette.Warning;
		}
		finally
		{
			_reading = null;
			if (!IsDisposed) _zip.Enabled = _folder.Enabled = true;
		}
	}

	private void UpdateDetails()
	{
		if (IsDisposed || Disposing || _details.IsDisposed) return;
		_details.Text = _grid.CurrentRow?.Tag is ArkModPackageInfo mod
			? $"{mod.Name} — {LocalizationManager.Get("AsaPackages.Id")}: {mod.ModId}{Environment.NewLine}" +
			  $"{LocalizationManager.Get("ModManager.Column.Version")}: {mod.Version}{Environment.NewLine}" +
			  $"{LocalizationManager.Get("AsaPackages.Descriptor")}: {mod.DescriptorPath}"
			: string.Empty;
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		_reading?.Cancel();
		base.OnFormClosing(e);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing) _reading?.Cancel();
		base.Dispose(disposing);
	}

	private static ModernSettingsButton Button(string key, string name) => new()
	{
		Name = name, Text = LocalizationManager.Get(key), Size = new Size(192, 40), Margin = new Padding(3, 4, 5, 4)
	};
}
