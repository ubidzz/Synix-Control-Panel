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

internal sealed class ModImportWizard : Form
{
	private readonly GameServer _server;
	private readonly ModernSettingsButton _file, _folder, _destination, _continue;
	private readonly ModernSettingsComboBox _choices, _roots;
	private readonly RichTextBox _source, _details;
	private readonly DataGridView _grid;
	private readonly ModInstallGuidancePanel _guidance;
	private readonly Label _status;
	private ModImportAnalysis? _analysis;
	private PreparedModPackage? _prepared;
	private CancellationTokenSource? _cancellation;
	private bool _reading, _updating, _closeAfterRead;
	internal ModImportChoice? Selection { get; private set; }
	internal string PackagePath { get; private set; } = "";
	internal string PackageSha256 { get; private set; } = "";
	internal IReadOnlyList<ArkModPackageInfo> ProviderMods => _analysis?.ProviderMods ?? [];

	internal ModImportWizard(GameServer server)
	{
		_server = server;
		Name = "modImportWizard";
		Text = T("Title");
		ClientSize = new Size(1120, 830);
		MinimumSize = new Size(1000, 760);
		StartPosition = FormStartPosition.CenterParent;
		ShowInTaskbar = false;
		Font = new Font("Segoe UI", 10F);
		BackColor = SettingsPalette.Window;
		TableLayoutPanel layout = UniversalModDialogStyle.Layout(this, [44, 56, 48, 54, 44, 44, -1, 108, 66, 48]);
		layout.ColumnCount = 2;
		layout.ColumnStyles.Clear();
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
		layout.Controls.Add(UniversalModDialogStyle.Label(Text, heading: true), 0, 0);
		layout.Controls.Add(UniversalModDialogStyle.Label(T("Help", server.Game)), 0, 1);
		layout.SetColumnSpan(layout.GetControlFromPosition(0, 0)!, 2);
		layout.SetColumnSpan(layout.GetControlFromPosition(0, 1)!, 2);
		FlowLayoutPanel pickers = new() { Dock = DockStyle.Fill, WrapContents = false };
		_file = Button("File", "chooseModPackageFile", 250);
		_folder = Button("Folder", "chooseModPackageFolder", 230);
		_file.UseAccentStyle = true;
		_file.Click += async (_, _) =>
		{
			using OpenFileDialog picker = new() { Title = T("File"), Filter = T("Filter"), CheckFileExists = true, Multiselect = false };
			if (picker.ShowDialog(this) == DialogResult.OK) await ReadSourceAsync(picker.FileName);
		};
		_folder.Click += async (_, _) =>
		{
			using FolderBrowserDialog picker = new() { Description = T("Folder"), UseDescriptionForTitle = true, ShowNewFolderButton = false };
			if (picker.ShowDialog(this) == DialogResult.OK) await ReadSourceAsync(picker.SelectedPath);
		};
		pickers.Controls.AddRange([_file, _folder]);
		ModernSettingsButton guide = new() { Name = "importGameSupportGuide", Text = LocalizationManager.Get("ModSupport.Title"),
			Size = new Size(270, 40), Margin = new Padding(8, 3, 3, 3) };
		guide.Click += (_, _) => { using GameModSupportDialog dialog = new(server.Game); dialog.ShowDialog(this); };
		pickers.Controls.Add(guide);
		layout.Controls.Add(pickers, 0, 2);
		layout.SetColumnSpan(pickers, 2);
		_guidance = new() { Margin = new Padding(12, 3, 3, 3) };
		layout.Controls.Add(_guidance, 1, 3);
		layout.SetRowSpan(_guidance, 5);
		_source = Details("modImportSource");
		_source.Text = T("Empty");
		layout.Controls.Add(_source, 0, 3);
		_choices = new() { Name = "detectedImportDestination", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "Label" };
		_choices.SelectedIndexChanged += async (_, _) =>
		{
			if (!_updating && !_reading && _choices.SelectedItem is ModImportChoice choice) await PreviewAsync(choice, choice.Selection);
		};
		layout.Controls.Add(_choices, 0, 4);
		_roots = new() { Name = "detectedPackageRoot", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "Label" };
		_roots.SelectedIndexChanged += async (_, _) =>
		{
			if (!_updating && !_reading && _choices.SelectedItem is ModImportChoice choice && _roots.SelectedItem is Root root)
				await PreviewAsync(choice, root.Path);
		};
		layout.Controls.Add(_roots, 0, 5);
		_grid = new() { Name = "modImportPlanFiles", Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
			AllowUserToDeleteRows = false, RowHeadersVisible = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
			AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
		foreach ((string key, float weight) in new[] { ("Import.Source", 38F), ("Import.Destination", 44F), ("Import.Change", 18F) })
			_grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = UniversalModDialogStyle.Text(key), FillWeight = weight, SortMode = DataGridViewColumnSortMode.NotSortable });
		GridStyler.DarkTheme(_grid);
		GridStyler.ApplyDashboardTheme(_grid);
		layout.Controls.Add(_grid, 0, 6);
		_details = Details("modImportPlanDetails");
		_grid.CurrentCellChanged += (_, _) =>
		{
			if (!_updating && !Disposing && !IsDisposed && !_details.IsDisposed)
				_details.Text = _grid.CurrentRow?.Tag as string ?? "";
		};
		layout.Controls.Add(_details, 0, 7);
		_status = UniversalModDialogStyle.Label(T("Empty"));
		_status.Name = "modImportWizardStatus";
		layout.Controls.Add(_status, 0, 8);
		layout.SetColumnSpan(_status, 2);
		FlowLayoutPanel actions = UniversalModDialogStyle.Actions(layout, 9);
		layout.SetColumnSpan(actions, 2);
		_continue = Button("Continue", "confirmModImportPlan", 240);
		_continue.UseAccentStyle = true;
		_continue.Click += (_, _) => { if (!_reading && Selection != null && PackageSha256.Length > 0) { DialogResult = DialogResult.OK; Close(); } };
		_destination = Button("Destination", "chooseModDestination", 230);
		_destination.Click += async (_, _) =>
		{
			if (_analysis == null || _reading) return;
			using FolderBrowserDialog picker = new() { SelectedPath = server.InstallPath, Description = T("DestinationHelp"), UseDescriptionForTitle = true };
			if (picker.ShowDialog(this) != DialogResult.OK) return;
			try
			{
				ModImportChoice choice = ModImportDiscovery.ChooseFolder(server, _analysis, picker.SelectedPath);
				_updating = true;
				_choices.Items.Add(choice); _choices.SelectedItem = choice;
				_updating = false;
				await PreviewAsync(choice, choice.Selection);
			}
			catch (Exception exception) { ClearPlan(); _status.Text = exception.Message; }
			finally { _updating = false; }
		};
		actions.Controls.Add(_continue);
		UniversalModDialogStyle.Cancel(this, actions);
		actions.Controls.Add(_destination);
		SetReading(false);
		ThemeManager.Apply(this);
		_source.BackColor = _details.BackColor = SettingsPalette.Input;
		_source.ForeColor = _details.ForeColor = SettingsPalette.PrimaryText;
	}

	internal async Task ReadSourceAsync(string path)
	{
		if (_reading) return;
		_analysis = null;
		ClearPlan();
		_updating = true; _choices.Items.Clear(); _roots.Items.Clear(); _updating = false;
		_source.Text = path;
		using CancellationTokenSource cancellation = BeginRead();
		try
		{
			_prepared?.Dispose(); _prepared = null;
			PackagePath = "";
			if (Directory.Exists(path))
			{
				_prepared = await Task.Run(() => PreparedModPackage.FromFolder(path, ModImportDiscovery.SnapshotTarget, cancellation.Token), cancellation.Token);
				PackagePath = _prepared.Path;
			}
			else PackagePath = path;
			_analysis = await Task.Run(() => ModImportDiscovery.Read(_server, PackagePath, cancellation.Token), cancellation.Token);
			_updating = true;
			foreach (ModImportChoice choice in _analysis.Choices) _choices.Items.Add(choice);
			// A ready loader wins over leftover folders. Multiple viable routes need a choice.
			ModImportChoice[] ready = _analysis.Choices.Where(choice => choice.Ready).ToArray();
			if (ready.Length == 1) _choices.SelectedItem = ready[0];
			else if (_analysis.Choices.Count == 1) _choices.SelectedIndex = 0;
			_updating = false;
			if (_choices.SelectedItem is ModImportChoice selected) await ReadPreviewCoreAsync(selected, selected.Selection, cancellation.Token);
			else
			{
				foreach (string file in _analysis.Files.Take(2000))
				{
					int index = _grid.Rows.Add(file, T("Undecided"), "");
					_grid.Rows[index].Tag = file;
				}
				_status.Text = T(_analysis.Choices.Count == 0 ? "Unknown" : "Ambiguous", _analysis.Files.Count);
			}
		}
		catch (Exception exception) { ClearPlan(); _analysis = null; _status.Text = exception is OperationCanceledException ? T("Cancelled") : exception.Message; }
		finally { _updating = false; EndRead(); }
	}

	private async Task PreviewAsync(ModImportChoice choice, string root)
	{
		using CancellationTokenSource cancellation = BeginRead();
		try { await ReadPreviewCoreAsync(choice, root, cancellation.Token); }
		catch (Exception exception) { ClearPlan(); _status.Text = exception is OperationCanceledException ? T("Cancelled") : exception.Message; }
		finally { EndRead(); }
	}

	private async Task ReadPreviewCoreAsync(ModImportChoice choice, string root, CancellationToken cancellation)
	{
		ClearPlan();
		if (_analysis == null) return;
		if (choice.Target.CanManageIds)
		{
			foreach (ArkModPackageInfo mod in _analysis.ProviderMods)
			{
				int row = _grid.Rows.Add(mod.Name, choice.Target.ProviderName + " · " + mod.ModId, T("ProviderAction"));
				_grid.Rows[row].Tag = T("ProviderDetails", mod.Name, mod.ModId, choice.Target.ProviderName);
			}
			_status.Text = T("ProviderWarning", choice.Target.ProviderName);
			PackageSha256 = _analysis.PackageSha256;
			_guidance.ShowGuidance(ModInstallGuidance.Create(_server, choice));
		}
		else
		{
			ModPackagePreview preview = await Task.Run(() => UniversalModPackage.Preview(_server, choice.Target, PackagePath, root, cancellation), cancellation);
			if (preview.PackageSha256 != _analysis.PackageSha256) throw new InvalidDataException(LocalizationManager.Get("ModManager.Error.ChangedAfterReview"));
			_updating = true; _roots.Items.Clear();
			bool adjustable = choice.Target.PackageLayout is ModPackageLayout.FolderTree or ModPackageLayout.Default;
			foreach (string folder in adjustable ? preview.Roots : [root]) _roots.Items.Add(new Root(folder));
			_roots.SelectedItem = _roots.Items.Cast<Root>().FirstOrDefault(item => item.Path == root);
			_updating = false;
			foreach (ModPackageFilePreview file in preview.Files.Take(2000))
			{
				int row = _grid.Rows.Add(file.Source, file.Destination, UniversalModDialogStyle.Text(file.ReplacesFile ? "Import.Replace" : "Import.New"));
				_grid.Rows[row].Tag = file.Source + Environment.NewLine + Path.Combine(_server.InstallPath, file.Destination);
			}
			_status.Text = !choice.Ready ? T("LoaderMissing") : choice.Profile.UserConfigured ? T("ManualWarning", choice.Target.RelativePath) :
				T("Summary", preview.Files.Count, preview.Files.Count(file => file.ReplacesFile));
			PackageSha256 = preview.PackageSha256;
			_guidance.ShowGuidance(ModInstallGuidance.Create(_server, choice, preview));
		}
		if (_grid.Rows.Count > 0) { _grid.CurrentCell = _grid.Rows[0].Cells[0]; _details.Text = _grid.Rows[0].Tag as string ?? ""; }
		if (choice.Ready) Selection = choice with { Selection = root };
	}

	private CancellationTokenSource BeginRead()
	{
		SetReading(true); _status.Text = T("Reading");
		return _cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(5));
	}
	private void EndRead()
	{
		_cancellation = null; SetReading(false);
		if (_closeAfterRead) { DialogResult = DialogResult.Cancel; Close(); }
	}
	private void ClearPlan()
	{
		Selection = null; PackageSha256 = "";
		_grid.Rows.Clear(); _details.Clear(); _continue.Enabled = false;
		_guidance.ShowGuidance(ModInstallGuidance.Pending);
	}
	private void SetReading(bool reading)
	{
		_reading = reading;
		_file.Enabled = _folder.Enabled = !reading;
		_choices.Enabled = !reading && _choices.Items.Count > 1;
		_choices.Visible = _analysis != null && _choices.Items.Count > 0;
		_roots.Visible = _analysis != null && _choices.SelectedItem is ModImportChoice { Target.CanImport: true };
		_roots.Enabled = !reading && _roots.Items.Count > 1 && _choices.SelectedItem is ModImportChoice { Target.CanImport: true };
		_destination.Visible = _analysis != null && _analysis.ProviderMods.Count == 0;
		_destination.Enabled = !reading && _analysis != null;
		_continue.Enabled = !reading && Selection != null && PackageSha256.Length > 0;
	}
	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		if (_reading) { e.Cancel = true; DialogResult = DialogResult.None; _closeAfterRead = true; _cancellation?.Cancel(); }
		base.OnFormClosing(e);
	}
	protected override void Dispose(bool disposing)
	{
		_updating = true;
		if (disposing)
		{
			_cancellation?.Cancel();
			try { _prepared?.Dispose(); }
			catch (Exception exception) { ApplicationLogService.WriteSuppressedException(exception); }
		}
		base.Dispose(disposing);
	}
	private static RichTextBox Details(string name) => new() { Name = name, Dock = DockStyle.Fill, ReadOnly = true, WordWrap = true,
		Multiline = true, DetectUrls = false, BorderStyle = BorderStyle.None, BackColor = SettingsPalette.Input, ForeColor = SettingsPalette.PrimaryText };
	private static ModernSettingsButton Button(string key, string name, int width) => new() { Name = name, Text = T(key), Width = width, Height = 40, Margin = new Padding(4) };
	private static string T(string key, params object[] arguments) => LocalizationManager.Get("UniversalMods.Wizard." + key, arguments);
	private sealed record Root(string Path) { public string Label => T("Root", Path.Length == 0 ? T("Whole") : Path); }
}
