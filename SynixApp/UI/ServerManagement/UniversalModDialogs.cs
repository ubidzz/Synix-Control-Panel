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

internal sealed class ModImportLocationDialog : Form
{
	private readonly TextBox _name, _path, _extensions;
	private readonly ModernSettingsComboBox _kind;
	private readonly Label _status;
	internal ModInstallTarget? ImportLocation { get; private set; }

	internal ModImportLocationDialog(GameServer server)
	{
		Name = "modImportLocationDialog";
		Text = UniversalModDialogStyle.Text("Locations.Title");
		StartPosition = FormStartPosition.CenterParent;
		ShowInTaskbar = false;
		ClientSize = new Size(900, 700);
		MinimumSize = new Size(860, 700);
		Font = new Font("Segoe UI", 10F);
		BackColor = SettingsPalette.Window;
		TableLayoutPanel layout = UniversalModDialogStyle.Layout(this, [48, 88, 44, 66, 66, 66, 46, 70, 48]);
		layout.Controls.Add(UniversalModDialogStyle.Label(Text, heading: true), 0, 0);
		layout.Controls.Add(UniversalModDialogStyle.Label(UniversalModDialogStyle.Text("Locations.Help")), 0, 1);
		ModernSettingsComboBox existing = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Name = "existingImportLocation" };
		List<ModInstallTarget> targets = UniversalModImports.Load(server)?.Targets ?? [];
		existing.Items.Add(UniversalModDialogStyle.Text("Locations.New"));
		foreach (ModInstallTarget target in targets) existing.Items.Add(target.DisplayName + " — " + target.RelativePath);
		layout.Controls.Add(existing, 0, 2);
		_name = AddField(layout, 3, "Locations.Name", "importLocationName");
		_path = AddField(layout, 4, "Locations.Path", "importLocationPath");
		_extensions = AddField(layout, 5, "Locations.Extensions", "importLocationExtensions");
		TableLayoutPanel kindRow = new() { Dock = DockStyle.Fill, ColumnCount = 2 };
		kindRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
		kindRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
		_kind = new() { Dock = DockStyle.Fill, Name = "importLocationKind", DropDownStyle = ComboBoxStyle.DropDownList };
		_kind.Items.Add(LocalizationManager.Get("ModManager.Known.Mod"));
		_kind.Items.Add(LocalizationManager.Get("ModManager.Known.Plugin"));
		_kind.SelectedIndex = 0;
		ModernSettingsButton browse = UniversalModDialogStyle.Button("Locations.Browse", "browseImportLocation");
		browse.Dock = DockStyle.Fill;
		browse.Click += (_, _) =>
		{
			using FolderBrowserDialog picker = new() { SelectedPath = server.InstallPath, ShowNewFolderButton = false };
			if (picker.ShowDialog(this) == DialogResult.OK) _path.Text = Path.GetRelativePath(server.InstallPath, picker.SelectedPath).Replace('\\', '/');
		};
		kindRow.Controls.Add(_kind, 0, 0);
		kindRow.Controls.Add(browse, 1, 0);
		layout.Controls.Add(kindRow, 0, 6);
		_status = UniversalModDialogStyle.Label(UniversalModDialogStyle.Text("Locations.Warning"));
		_status.Name = "importLocationStatus";
		_status.ForeColor = SettingsPalette.Warning;
		layout.Controls.Add(_status, 0, 7);
		FlowLayoutPanel actions = UniversalModDialogStyle.Actions(layout, 8);
		ModernSettingsButton save = UniversalModDialogStyle.Button("Locations.Save", "saveImportLocation");
		save.UseAccentStyle = true;
		save.Click += (_, _) =>
		{
			try
			{
				ImportLocation = UniversalModImports.CreateTarget(server, _name.Text, _path.Text, _extensions.Text,
					_kind.SelectedIndex == 0 ? ModContentKind.Mod : ModContentKind.Plugin);
				DialogResult = DialogResult.OK;
				Close();
			}
			catch (Exception exception) { ImportLocation = null; _status.Text = exception.Message; }
		};
		actions.Controls.Add(save);
		UniversalModDialogStyle.Cancel(this, actions);
		existing.SelectedIndexChanged += (_, _) =>
		{
			ModInstallTarget? target = existing.SelectedIndex > 0 ? targets[existing.SelectedIndex - 1] : null;
			_name.Text = target?.DisplayName ?? string.Empty;
			_path.Text = target?.RelativePath ?? string.Empty;
			_extensions.Text = target == null ? string.Empty : string.Join(", ", target.AllowedExtensions);
			_kind.SelectedIndex = target?.Kind == ModContentKind.Plugin ? 1 : 0;
		};
		existing.SelectedIndex = 0;
		ThemeManager.Apply(this);
	}

	private static TextBox AddField(TableLayoutPanel layout, int row, string key, string name)
	{
		TableLayoutPanel field = new() { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
		field.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		field.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
		field.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		Label label = UniversalModDialogStyle.Label(UniversalModDialogStyle.Text(key));
		label.Margin = Padding.Empty;
		field.Controls.Add(label, 0, 0);
		ModernSettingsCard card = new() { Dock = DockStyle.Fill, Margin = Padding.Empty, Padding = new Padding(9, 5, 9, 5), FillColor = SettingsPalette.Input };
		TextBox input = new() { Name = name, Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, BackColor = SettingsPalette.Input,
			ForeColor = SettingsPalette.PrimaryText, AccessibleName = UniversalModDialogStyle.Text(key) };
		card.Controls.Add(input);
		field.Controls.Add(card, 0, 1);
		layout.Controls.Add(field, 0, row);
		return input;
	}
}

internal sealed class UniversalModImportPreview : Form
{
	private readonly GameServer _server;
	private readonly ModInstallTarget _target;
	private readonly string _source;
	private readonly ModernSettingsComboBox _roots;
	private readonly DataGridView _grid;
	private readonly RichTextBox _details;
	private readonly Label _status;
	private readonly ModernSettingsButton _continue;
	private bool _reading;
	private bool _updating;
	private bool _closeAfterRead;
	private CancellationTokenSource? _readingCancellation;
	internal string SelectedRoot { get; private set; } = string.Empty;
	internal string PackageSha256 { get; private set; } = string.Empty;

	internal UniversalModImportPreview(GameServer server, ModInstallTarget target, string source)
	{
		_server = server; _target = target; _source = source;
		Name = "universalModImportPreview";
		Text = UniversalModDialogStyle.Text("Import.Title");
		StartPosition = FormStartPosition.CenterParent;
		ShowInTaskbar = false;
		ClientSize = new Size(1040, 690);
		MinimumSize = new Size(900, 650);
		Font = new Font("Segoe UI", 10F);
		BackColor = SettingsPalette.Window;
		TableLayoutPanel layout = UniversalModDialogStyle.Layout(this, [48, 92, 44, -1, 108, 56, 48]);
		layout.Controls.Add(UniversalModDialogStyle.Label(Text, heading: true), 0, 0);
		layout.Controls.Add(UniversalModDialogStyle.Label(UniversalModDialogStyle.Text("Import.Help")), 0, 1);
		_roots = new() { Name = "modPackageRoot", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "Label" };
		_roots.SelectedIndexChanged += async (_, _) =>
		{
			if (!_updating && !_reading && _roots.SelectedItem is RootChoice root) await LoadPreviewAsync(root.Path);
		};
		layout.Controls.Add(_roots, 0, 2);
		_grid = new() { Name = "universalModFiles", Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
			AllowUserToDeleteRows = false, RowHeadersVisible = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
			AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
		foreach ((string key, float weight) in new[] { ("Import.Source", 36F), ("Import.Destination", 44F), ("Import.Change", 20F) })
			_grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = UniversalModDialogStyle.Text(key), FillWeight = weight, SortMode = DataGridViewColumnSortMode.NotSortable });
		GridStyler.DarkTheme(_grid);
		GridStyler.ApplyDashboardTheme(_grid);
		layout.Controls.Add(_grid, 0, 3);
		ModernSettingsCard card = new() { Dock = DockStyle.Fill, Padding = new Padding(10), FillColor = SettingsPalette.Input };
		_details = new() { Name = "universalModFileDetails", Dock = DockStyle.Fill, Multiline = true, WordWrap = true, DetectUrls = false,
			ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = SettingsPalette.Input, ForeColor = SettingsPalette.PrimaryText };
		card.Controls.Add(_details);
		layout.Controls.Add(card, 0, 4);
		_grid.CurrentCellChanged += (_, _) => ShowDetails();
		_status = UniversalModDialogStyle.Label(string.Empty);
		_status.Name = "universalModPreviewStatus";
		layout.Controls.Add(_status, 0, 5);
		FlowLayoutPanel actions = UniversalModDialogStyle.Actions(layout, 6);
		_continue = UniversalModDialogStyle.Button("Import.Continue", "continueModImport");
		_continue.Width = 270;
		_continue.Enabled = false;
		_continue.UseAccentStyle = true;
		_continue.Click += (_, _) => { if (!_reading && PackageSha256.Length > 0) { DialogResult = DialogResult.OK; Close(); } };
		actions.Controls.Add(_continue);
		UniversalModDialogStyle.Cancel(this, actions);
		ThemeManager.Apply(this);
	}

	protected override async void OnShown(EventArgs e) { base.OnShown(e); await LoadPreviewAsync(string.Empty); }

	internal async Task LoadPreviewAsync(string root)
	{
		if (_reading) return;
		_reading = true;
		using CancellationTokenSource reading = new(TimeSpan.FromMinutes(2));
		_readingCancellation = reading;
		_continue.Enabled = _roots.Enabled = false;
		PackageSha256 = string.Empty;
		_grid.Rows.Clear();
		_details.Clear();
		_status.Text = UniversalModDialogStyle.Text("Import.Reading");
		try
		{
			ModPackagePreview preview = await Task.Run(() => UniversalModPackage.Preview(_server, _target, _source, root, reading.Token), reading.Token);
			if (IsDisposed) return;
			SelectedRoot = root;
			PackageSha256 = preview.PackageSha256;
			_updating = true;
			try
			{
				_roots.Items.Clear();
				foreach (string path in preview.Roots) _roots.Items.Add(new RootChoice(path,
					path.Length == 0 ? UniversalModDialogStyle.Text("Import.WholePackage") : path));
				_roots.SelectedIndex = Math.Max(0, preview.Roots.ToList().FindIndex(path => path == root));
			}
			finally { _updating = false; }
			foreach (ModPackageFilePreview file in preview.Files)
			{
				int row = _grid.Rows.Add(file.Source, file.Destination, UniversalModDialogStyle.Text(file.ReplacesFile ? "Import.Replace" : "Import.New"));
				_grid.Rows[row].Tag = file;
			}
			ShowDetails();
			_status.Text = LocalizationManager.Get("UniversalMods.Import.Summary", preview.Files.Count, preview.Files.Count(file => file.ReplacesFile));
			_continue.Enabled = true;
		}
		catch (Exception exception) { if (!IsDisposed) { _status.Text = exception.Message; _status.ForeColor = SettingsPalette.Warning; } }
		finally
		{
			_readingCancellation = null;
			_reading = false;
			if (!IsDisposed)
			{
				_roots.Enabled = true;
				if (_closeAfterRead) { DialogResult = DialogResult.Cancel; Close(); }
			}
		}
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		base.OnFormClosing(e);
		if (_reading)
		{
			e.Cancel = true;
			DialogResult = DialogResult.None;
			_closeAfterRead = true;
			_readingCancellation?.Cancel();
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing) _readingCancellation?.Cancel();
		base.Dispose(disposing);
	}

	private void ShowDetails()
	{
		if (IsDisposed || Disposing || _details.IsDisposed) return;
		_details.Text = _grid.CurrentRow?.Tag is ModPackageFilePreview file
			? UniversalModDialogStyle.Text("Import.Source") + ": " + file.Source + Environment.NewLine +
			  UniversalModDialogStyle.Text("Import.Destination") + ": " + Path.Combine(_server.InstallPath, file.Destination) + Environment.NewLine +
			  UniversalModDialogStyle.Text(file.ReplacesFile ? "Import.Replace" : "Import.New") : string.Empty;
	}

	private sealed record RootChoice(string Path, string Label);
}

internal static class UniversalModDialogStyle
{
	internal static string Text(string key) => LocalizationManager.Get("UniversalMods." + key);
	internal static TableLayoutPanel Layout(Form form, float[] heights)
	{
		TableLayoutPanel layout = new() { Dock = DockStyle.Fill, Padding = new Padding(24), ColumnCount = 1, RowCount = heights.Length };
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		foreach (float height in heights) layout.RowStyles.Add(new RowStyle(height < 0 ? SizeType.Percent : SizeType.Absolute, height < 0 ? 100 : height));
		form.Controls.Add(layout);
		return layout;
	}
	internal static Label Label(string text, bool heading = false) => new()
	{
		Text = text, UseMnemonic = false, Dock = DockStyle.Fill, ForeColor = heading ? SettingsPalette.PrimaryText : SettingsPalette.SecondaryText,
		Font = new Font("Segoe UI", heading ? 18F : 10F, heading ? FontStyle.Bold : FontStyle.Regular)
	};
	internal static ModernSettingsButton Button(string key, string name) => new()
		{ Text = Text(key), Name = name, Size = new Size(220, 40), Margin = new Padding(4) };
	internal static FlowLayoutPanel Actions(TableLayoutPanel layout, int row)
	{
		FlowLayoutPanel actions = new() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
		layout.Controls.Add(actions, 0, row);
		return actions;
	}
	internal static void Cancel(Form form, FlowLayoutPanel actions)
	{
		ModernSettingsButton cancel = new() { Text = LocalizationManager.Get("Text.19766ED6CCB2F4A32778"), Size = new Size(180, 40), Margin = new Padding(4), DialogResult = DialogResult.Cancel };
		form.CancelButton = cancel;
		actions.Controls.Add(cancel);
	}
}
