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
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using Synix_Control_Panel.SynixApp.Localization;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine.ModManagement;
using System.Diagnostics;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel.SynixApp.UI.ServerManagement
{
	internal sealed class ModPluginManager : Form
	{
		private readonly GameServer _server;
		private IReadOnlyList<ModSystemProfile> _profiles;
		private readonly ModernSettingsComboBox _profileBox;
		private readonly ModernSettingsComboBox _targetBox;
		private readonly ModernSettingsToggle _simpleView;
		private readonly Label _supportTitle;
		private readonly Label _supportDetails;
		private readonly DataGridView _grid;
		private readonly TextBox _search;
		private readonly Label _inventorySummary;
		private readonly Label _selectionDetails;
		private readonly Label[] _safetyItems;
		private readonly ModernSettingsButton _installFile;
		private readonly ModernSettingsButton _readArkPackage;
		private readonly ModernSettingsButton _importLocations;
		private readonly ModernSettingsButton _remove;
		private readonly ModernSettingsButton _installFramework;
		private readonly ModernSettingsButton _browseCatalog;
		private readonly ModernSettingsButton _openFolder;
		private readonly ModernSettingsButton _refresh;
		private readonly ModernSettingsButton _close;
		private readonly ContextMenuStrip _catalogMenu = new();
		private ModSystemDetection? _detection;
		private IReadOnlyList<ModInventoryItem> _items = [];
		private bool _updatingSelectors;
		private bool _hasShown;
		private bool _busy;
		private bool _renderingInventory;
		private bool _scanFailed;

		internal ModPluginManager(GameServer server)
		{
			_server = server ?? throw new ArgumentNullException(nameof(server));
			_profiles = ModSystemCatalog.GetProfiles(server);
			// The menu resource escapes '&' for ToolStrip mnemonics; window text does not.
			Text = LocalizationManager.Get("Menu.ModPluginManager").Replace("&&", "&");
			StartPosition = FormStartPosition.CenterParent;
			ShowInTaskbar = false;
			MinimumSize = new Size(1240, 760);
			ClientSize = new Size(1240, 760);
			BackColor = SettingsPalette.Window;
			ForeColor = SettingsPalette.PrimaryText;
			Font = new Font("Segoe UI", 9.5F);

			Label pageHeading = Heading(
				Text,
				28, 20, 640, 42, 19F);
			pageHeading.Name = "modPluginManagerHeading";
			Controls.Add(pageHeading);
			_importLocations = Button(LocalizationManager.Get("UniversalMods.Wizard.Advanced"), 710, 20, 242);
			_importLocations.Name = "configureModImportLocations";
			_importLocations.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_importLocations.Click += (_, _) => ConfigureImportLocations();
			Controls.Add(_importLocations);
			ModernSettingsButton supportGuide = Button(LocalizationManager.Get("ModSupport.Title"), 964, 20, 248);
			supportGuide.Name = "gameModSupportGuide";
			supportGuide.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			supportGuide.Click += (_, _) => { using GameModSupportDialog guide = new(_server.Game); guide.ShowDialog(this); };
			Controls.Add(supportGuide);
			Controls.Add(Body(
				LocalizationManager.Get("ModManager.Subtitle"),
				30, 62, 890, 42));
			Controls.Add(FieldLabel(
				LocalizationManager.Get("ModManager.Field.Server"),
				30, 108, 110));
			Controls.Add(new Label
			{
				Text = LocalizationManager.Get(
					"ModManager.ServerSummary",
					_server.ServerName,
					_server.Game),
				Location = new Point(30, 130),
				Size = new Size(360, 30),
				Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
				ForeColor = SettingsPalette.PrimaryText
			});

			Controls.Add(FieldLabel(
				LocalizationManager.Get("ModManager.Field.System"),
				404, 108, 160));
			_profileBox = new ModernSettingsComboBox
			{
				Name = "modSystemSelector",
				Location = new Point(404, 130),
				Size = new Size(250, 36),
				DisplayMember = nameof(ModSystemProfile.DisplayName),
				FormattingEnabled = true,
				Enabled = _profiles.Count > 1
			};
			_profileBox.Format += (_, eventArgs) =>
			{
				if (eventArgs.ListItem is ModSystemProfile profile)
				{
					eventArgs.Value = LocalizationManager.TranslateKnownText(
						profile.DisplayName);
				}
			};
			foreach (ModSystemProfile profile in _profiles)
				_profileBox.Items.Add(profile);
			_profileBox.SelectedIndexChanged += (_, _) => ProfileChanged();
			Controls.Add(_profileBox);

			Controls.Add(FieldLabel(
				LocalizationManager.Get("ModManager.Field.InstallArea"),
				668, 108, 150));
			_targetBox = new ModernSettingsComboBox
			{
				Name = "modInstallAreaSelector",
				Location = new Point(668, 130),
				Size = new Size(238, 36),
				DisplayMember = nameof(ModInstallTarget.DisplayName),
				FormattingEnabled = true
			};
			_targetBox.Format += (_, eventArgs) =>
			{
				if (eventArgs.ListItem is ModInstallTarget target)
				{
					eventArgs.Value = LocalizationManager.TranslateKnownText(
						target.DisplayName);
				}
			};
			_targetBox.SelectedIndexChanged += (_, _) =>
			{
				if (!_updatingSelectors) RenderInventory();
			};
			Controls.Add(_targetBox);

			Controls.Add(new Label
			{
				Text = LocalizationManager.Get("Text.92079831E5CC0BF95974"),
				Location = new Point(1022, 114),
				Size = new Size(104, 24),
				ForeColor = SettingsPalette.SecondaryText,
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			});
			_simpleView = new ModernSettingsToggle
			{
				Name = "modSimpleViewToggle",
				Location = new Point(1134, 108),
				Checked = true,
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			};
			_simpleView.CheckedChanged += (_, _) => ApplySimpleView();
			Controls.Add(_simpleView);

			ModernSettingsCard supportCard = Card(28, 176, 1184, 78);
			_supportTitle = Heading(
				LocalizationManager.Get("ModManager.Support.Checking"),
				18, 12, 720, 26, 11F);
			_supportTitle.ForeColor = SettingsPalette.Accent;
			_supportDetails = Body(string.Empty, 18, 40, 1128, 26);
			supportCard.Controls.AddRange([_supportTitle, _supportDetails]);
			Controls.Add(supportCard);

			Panel workflow = new()
			{
				Location = new Point(28, 266),
				Size = new Size(1184, 46),
				BackColor = SettingsPalette.Input,
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			};
			string[] steps =
			[
				LocalizationManager.Get("ModManager.Step.Detect"),
				LocalizationManager.Get("ModManager.Step.Stop"),
				LocalizationManager.Get("ModManager.Step.Backup"),
				LocalizationManager.Get("ModManager.Step.Install"),
				LocalizationManager.Get("ModManager.Step.Verify"),
				LocalizationManager.Get("ModManager.Step.Restart")
			];
			for (int index = 0; index < steps.Length; index++)
			{
				workflow.Controls.Add(new Label
				{
					Text = steps[index],
					Location = new Point(16 + index * 190, 12),
					Size = new Size(178, 24),
					Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
					ForeColor = index == 0 ? SettingsPalette.Accent : SettingsPalette.SecondaryText
				});
			}
			Controls.Add(workflow);

			ModernSettingsCard searchCard = Card(28, 326, 844, 36);
			searchCard.FillColor = SettingsPalette.Input;
			searchCard.Controls.Add(Body(LocalizationManager.Get("ModManager.Field.Search"), 14, 9, 88, 22));
			_search = new TextBox
			{
				Name = "addOnSearchBox",
				Location = new Point(110, 8),
				Size = new Size(718, 24),
				Font = new Font("Segoe UI", 10F),
				BackColor = SettingsPalette.Input,
				ForeColor = SettingsPalette.PrimaryText,
				BorderStyle = BorderStyle.None,
				PlaceholderText = LocalizationManager.Get("ModManager.Inventory.Search"),
				AccessibleName = LocalizationManager.Get("ModManager.Inventory.Search"),
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			};
			_search.TextChanged += (_, _) => RenderInventory();
			searchCard.Controls.Add(_search);
			Controls.Add(searchCard);

			_grid = new DataGridView
			{
				Name = "addOnInventoryGrid",
				Location = new Point(28, 370),
				Size = new Size(844, 282),
				Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
				ReadOnly = true,
				AllowUserToAddRows = false,
				AllowUserToDeleteRows = false,
				AllowUserToResizeRows = false,
				AutoGenerateColumns = false,
				SelectionMode = DataGridViewSelectionMode.FullRowSelect,
				MultiSelect = false,
				RowHeadersVisible = false,
				ScrollBars = ScrollBars.Both,
				AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
				RowTemplate = { Height = 40 }
			};
			AddColumn("Name", LocalizationManager.Get("ModManager.Column.AddOn"), 220);
			AddColumn("Type", LocalizationManager.Get("ModManager.Column.Type"), 80);
			AddColumn("Version", LocalizationManager.Get("ModManager.Column.Version"), 130);
			AddColumn("Status", LocalizationManager.Get("ModManager.Column.Status"), 170);
			AddColumn("Security", LocalizationManager.Get("ModManager.Column.Security"), 190);
			AddColumn("Source", LocalizationManager.Get("ModManager.Column.Source"), 130);
			AddColumn("Location", LocalizationManager.Get("ModManager.Column.Location"), 300);
			GridStyler.DarkTheme(_grid);
			GridStyler.ApplyDashboardTheme(_grid);
			_grid.SelectionChanged += (_, _) => SelectionChanged();
			Controls.Add(_grid);

			ModernSettingsCard safetyCard = Card(890, 326, 322, 240);
			safetyCard.Name = "automaticSafetyChecklistCard";
			safetyCard.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
			safetyCard.Controls.Add(Heading(
				LocalizationManager.Get("ModManager.Safety.Title"),
				18, 14, 282, 28, 11F));
			safetyCard.Controls.Add(Body(
				LocalizationManager.Get("ModManager.Safety.Subtitle"),
				18, 44, 282, 28));
			_safetyItems = new Label[6];
			for (int index = 0; index < _safetyItems.Length; index++)
			{
				_safetyItems[index] = new Label
				{
					Name = $"safetyChecklistItem{index + 1}",
					Location = new Point(18, 78 + index * 26),
					Size = new Size(282, 25),
					UseMnemonic = false,
					ForeColor = SettingsPalette.SecondaryText
				};
				safetyCard.Controls.Add(_safetyItems[index]);
			}
			Controls.Add(safetyCard);

			ModernSettingsCard selectionCard = Card(890, 576, 322, 76);
			selectionCard.Name = "selectedAddOnDetailsCard";
			selectionCard.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_selectionDetails = Body(
				LocalizationManager.Get("ModManager.Selection.Empty"),
				18, 11, 282, 54);
			_selectionDetails.Name = "selectedAddOnDetails";
			_selectionDetails.UseMnemonic = false;
			_selectionDetails.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
			selectionCard.Controls.Add(_selectionDetails);
			Controls.Add(selectionCard);

			_inventorySummary = new Label
			{
				Name = "modInventorySummary",
				Location = new Point(30, 664),
				Size = new Size(610, 28),
				ForeColor = SettingsPalette.SecondaryText,
				Anchor = AnchorStyles.Bottom | AnchorStyles.Left
			};
			Controls.Add(_inventorySummary);

			_installFile = Button(
				LocalizationManager.Get("ModManager.Button.InstallFile"),
				28, 702, 182, accent: true);
			_installFile.Click += InstallFile_Click;
			_installFile.Name = "importAddOnPackage";
			_readArkPackage = Button(LocalizationManager.Get("ModManager.Button.ManageIds"), 220, 702, 142);
			_readArkPackage.Name = "manageProviderModIds";
			_readArkPackage.Click += async (_, _) =>
			{
				if (_targetBox.SelectedItem is ModInstallTarget target && target.CanManageIds)
					await ManageProviderIdsAsync(target);
			};
			_installFramework = Button(
				LocalizationManager.Get("ModManager.Button.InstallFramework"),
				220, 702, 142);
			_installFramework.Name = "addOnSetupAction";
			_installFramework.Click += async (_, _) =>
			{
				if (_targetBox.SelectedItem is ModInstallTarget selected && EmpyrionAddOns.IsScenario(selected))
					await ChooseEmpyrionScenarioAsync();
				else
					await InstallFrameworkAsync();
			};
			_browseCatalog = Button(
				LocalizationManager.Get("ModManager.Button.BrowseCatalog"),
				368, 702, 150);
			_browseCatalog.Name = "browseAddOnCatalog";
			_browseCatalog.Click += (_, _) => BrowseCatalog();
			_openFolder = Button(
				LocalizationManager.Get("ModManager.Button.OpenFolder"),
				528, 702, 172);
			_openFolder.Click += (_, _) => OpenAddOnsFolder();
			_openFolder.Name = "openAddOnFolder";
			_refresh = Button(
				LocalizationManager.Get("ModManager.Button.Refresh"),
				710, 702, 112);
			_refresh.Click += async (_, _) => await RefreshInventory();
			_refresh.Name = "refreshAddOnInventory";
			_remove = Button(
				LocalizationManager.Get("ModManager.Button.Remove"),
				878, 702, 150);
			_remove.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_remove.Click += RemoveSelected_Click;
			_remove.Name = "removeAddOnPackage";
			_close = Button(
				LocalizationManager.Get("ModManager.Button.Close"),
				1038, 702, 174);
			_close.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_close.DialogResult = DialogResult.OK;
			_close.Name = "closeAddOnManager";
			Controls.AddRange([
				_installFile, _readArkPackage, _installFramework, _browseCatalog, _openFolder,
				_refresh, _remove, _close]);
			CancelButton = _close;

			if (_profiles.Count > 0)
				_profileBox.SelectedIndex = 0;
			else
				ShowUnsupportedState();
			ApplySimpleView();
			ThemeManager.Apply(this);
		}

		protected override async void OnShown(EventArgs eventArgs)
		{
			base.OnShown(eventArgs);
			_hasShown = true;
			await RefreshInventory();
		}

		protected override void OnFormClosing(FormClosingEventArgs eventArgs)
		{
			if (_busy) eventArgs.Cancel = true;
			base.OnFormClosing(eventArgs);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
				_catalogMenu.Dispose();
		}

		private async void ProfileChanged()
		{
			if (_updatingSelectors || _profileBox.SelectedItem is not ModSystemProfile profile)
				return;
			try
			{
				_detection = ModSystemCatalog.Detect(_server, profile);
				_updatingSelectors = true;
				try
				{
					_targetBox.Items.Clear();
					foreach (ModInstallTarget target in profile.Targets)
						_targetBox.Items.Add(target);
					int recommended = Math.Max(0, profile.Targets.FindIndex(target =>
						target.Id.Equals(_detection?.RecommendedTarget.Id, StringComparison.OrdinalIgnoreCase)));
					if (_targetBox.Items.Count > 0)
						_targetBox.SelectedIndex = recommended;
				}
				finally
				{
					_updatingSelectors = false;
				}
				if (_hasShown)
				{
					await RefreshInventory();
				}
				else
				{
					UpdateSupportBanner();
					UpdateButtonsAndSafety();
				}
			}
			catch (Exception exception)
			{
				_detection = null;
				InvalidateInventory();
				UpdateSupportBanner();
				PlainEnglishErrorDialog.ShowError(this, LocalizationManager.Get("ModManager.ErrorAction.Scan"), exception.Message);
			}
		}

		private async Task RefreshInventory(bool duringOperation = false)
		{
			if (_busy && !duringOperation) return;
			if (_profileBox.SelectedItem is not ModSystemProfile profile)
			{
				ShowUnsupportedState();
				return;
			}
			try
			{
				SetBusy(true, "ModManager.Inventory.Scanning");
				using ServerOperationLease operation = ModPackageManager.BeginOperation(_server);
				var result = await Task.Run(() => (Detection: ModSystemCatalog.Detect(_server, profile),
					Items: ModPackageManager.Scan(_server, profile)));
				_detection = result.Detection;
				_items = result.Items;
				_scanFailed = false;
				RenderInventory();
				UpdateSupportBanner();
			}
			catch (Exception exception)
			{
				_scanFailed = true;
				_items = [];
				RenderInventory();
				_inventorySummary.Text = LocalizationManager.Get("ModManager.Inventory.RefreshFailed");
				_inventorySummary.ForeColor = SettingsPalette.Warning;
				PlainEnglishErrorDialog.ShowError(this,
					LocalizationManager.Get("ModManager.ErrorAction.Scan"), exception.Message);
			}
			finally
			{
				if (!duringOperation) SetBusy(false);
			}
		}

		private void SetBusy(bool busy, string? progressKey = null)
		{
			_busy = busy;
			UseWaitCursor = busy;
			if (progressKey != null)
			{
				_inventorySummary.Text = LocalizationManager.Get(progressKey);
				_inventorySummary.ForeColor = SettingsPalette.SecondaryText;
			}
			UpdateButtonsAndSafety();
		}

		private void RenderInventory()
		{
			if (_updatingSelectors || _renderingInventory) return;
			ModInventoryItem? selected = SelectedItem();
			string? targetId = (_targetBox.SelectedItem as ModInstallTarget)?.Id;
			string search = _search.Text.Trim();
			ModInventoryItem[] visible = _items.Where(item =>
				item.TargetId.Equals(targetId, StringComparison.OrdinalIgnoreCase) &&
				(search.Length == 0 || new[] { item.Name, item.Type, item.Source, item.RelativePath }
					.Any(value => value.Contains(search, StringComparison.CurrentCultureIgnoreCase)))).ToArray();
			_renderingInventory = true;
			try
			{
				_grid.Rows.Clear();
				DataGridViewRow? retained = null;
				foreach (ModInventoryItem item in visible)
				{
					int index = _grid.Rows.Add(item.Name, item.Type, item.Version, item.Status,
						item.SecurityStatus, item.Source, item.RelativePath);
					DataGridViewRow row = _grid.Rows[index];
					row.Tag = item;
					Color color = item.Status == LocalizationManager.Get("ModManager.Known.Healthy") ? SettingsPalette.Success :
						item.Status == LocalizationManager.Get("ModManager.Known.Changed") ? SettingsPalette.Warning : SettingsPalette.SecondaryText;
					row.Cells[3].Style.ForeColor = row.Cells[3].Style.SelectionForeColor = color;
					if (selected?.TargetId == item.TargetId && selected.RelativePath == item.RelativePath)
						retained = row;
				}
				_grid.ClearSelection();
				if (_grid.Rows.Count > 0)
				{
					retained ??= _grid.Rows[0];
					_grid.CurrentCell = retained.Cells[0];
					retained.Selected = true;
				}
			}
			finally { _renderingInventory = false; }
			_inventorySummary.ForeColor = SettingsPalette.SecondaryText;
			_inventorySummary.Text = search.Length > 0
				? LocalizationManager.Get("ModManager.Inventory.Filtered", visible.Length,
					_items.Count(item => item.TargetId.Equals(targetId, StringComparison.OrdinalIgnoreCase)))
				: visible.Length == 0 ? LocalizationManager.Get("ModManager.Inventory.Empty")
				: LocalizationManager.Get(visible.Length == 1 ? "ModManager.Inventory.One" : "ModManager.Inventory.Many",
					visible.Length, visible.Count(item => item.InstallationId != null));
			SelectionChanged();
		}

		private void UpdateSupportBanner()
		{
			if (_detection == null)
			{
				ShowUnsupportedState();
				return;
			}

			_supportTitle.Text = GetSupportText(_detection);
			_supportTitle.ForeColor = _detection.Profile.SupportLevel == ModSystemSupportLevel.DetectedOnly
				? SettingsPalette.Warning
				: _detection.FrameworkDetected ? SettingsPalette.Success : SettingsPalette.Warning;
			string framework = string.IsNullOrWhiteSpace(_detection.Profile.FrameworkName)
				? LocalizationManager.Get("ModManager.Framework.Automatic")
				: LocalizationManager.Get(
					"ModManager.Framework.Named",
					LocalizationManager.TranslateKnownText(
						_detection.Profile.FrameworkName));
			_supportDetails.Text = LocalizationManager.Get(
				"ModManager.SupportDetails",
				LocalizationManager.TranslateKnownText(_detection.Profile.Description),
				framework);
			if (_detection.Profile.UserConfigured)
			{
				_supportTitle.Text = LocalizationManager.Get("UniversalMods.Locations.Ready");
				_supportTitle.ForeColor = SettingsPalette.Warning;
				_supportDetails.Text = LocalizationManager.Get("UniversalMods.Locations.Warning");
			}
			if (_detection.Profile.Targets.FirstOrDefault()?.PackageLayout is ModPackageLayout.EmpyrionScenario)
				_supportDetails.Text = LocalizationManager.Get("EmpyrionMods.Support.Scenarios");
			else if (_detection.Profile.Targets.FirstOrDefault()?.PackageLayout is ModPackageLayout.EmpyrionMod)
				_supportDetails.Text = LocalizationManager.Get("EmpyrionMods.Support.Mods");
		}

		private static string GetSupportText(ModSystemDetection detection)
		{
			string key = detection.Profile.SupportLevel switch
			{
				_ when detection.RecommendedTarget.CanManageIds =>
					"ModManager.Support.ProviderIds",
				ModSystemSupportLevel.Managed when detection.FrameworkDetected =>
					"ModManager.Support.FileImport",
				ModSystemSupportLevel.Managed =>
					"ModManager.Support.SetupNeeded",
				_ => "ModManager.Support.DetectionOnly"
			};
			return LocalizationManager.Get(key);
		}

		private void ShowUnsupportedState()
		{
			_supportTitle.Text = GameModSupportCatalog.StatusText(_server.Game);
			_supportTitle.ForeColor = SettingsPalette.Warning;
			_supportDetails.Text = LocalizationManager.Get(
				GameModSupportCatalog.ForGame(_server.Game)?.Status == GameModEvidence.Documented
					? "ModSupport.GuidedImport" : "ModSupport.UnverifiedImport");
			_inventorySummary.Text = LocalizationManager.Get(
				"ModManager.NoFilesChanged");
			_grid.Rows.Clear();
			UpdateButtonsAndSafety();
		}

		private void UpdateButtonsAndSafety()
		{
			ModSystemProfile? profile = _profileBox.SelectedItem as ModSystemProfile;
			ModInstallTarget? target = _targetBox.SelectedItem as ModInstallTarget;
			bool stopped = _server.Status == StatusManager.GetStatus(ServerState.Stopped);
			bool standardUser = !ModSecurityScanner.IsCurrentProcessElevated();
			_importLocations.Enabled = !_busy && stopped && Directory.Exists(_server.InstallPath);
			_importLocations.Visible = !_simpleView.Checked;
			bool canManage = profile != null && target != null && target.CanManage && standardUser &&
				profile.SupportLevel == ModSystemSupportLevel.Managed &&
				(_detection?.FrameworkDetected ?? false);
			_installFile.Text = LocalizationManager.Get("UniversalMods.Wizard.Button");
			_installFile.Enabled = !_busy && stopped && standardUser && Directory.Exists(_server.InstallPath);
			bool canReadArk = target?.CanManageIds == true;
			_readArkPackage.Visible = canReadArk;
			_readArkPackage.Enabled = canReadArk && _installFile.Enabled;
			_remove.Enabled = !_busy && stopped && standardUser && SelectedItem()?.CanRemove == true;
			_remove.Text = LocalizationManager.Get(target?.CanImport == true
				? "EmpyrionMods.Button.RollBack" : "ModManager.Button.Remove");
			_profileBox.Enabled = !_busy && _profiles.Count > 1;
			_targetBox.Enabled = !_busy && _targetBox.Items.Count > 1;
			_search.Enabled = _grid.Enabled = _refresh.Enabled = _close.Enabled = !_busy;
			_openFolder.Visible = target?.CanManageIds != true;
			_openFolder.Enabled = !_busy && !_scanFailed && target != null && !target.CanManageIds &&
				(profile?.SupportLevel != ModSystemSupportLevel.DetectedOnly ||
				Directory.Exists(GetSelectedTargetPath()));
			IReadOnlyList<CatalogChoice> catalogs = GetCatalogChoices(profile);
			_browseCatalog.Enabled = !_busy && catalogs.Count > 0;
			_browseCatalog.Text = catalogs.Count > 1
				? LocalizationManager.Get("ModManager.Button.BrowseCatalogs")
				: LocalizationManager.Get("ModManager.Button.BrowseCatalog");
			bool isRustFramework = profile?.Id.Equals("rust-umod", StringComparison.OrdinalIgnoreCase) == true;
			bool isScenario = target != null && EmpyrionAddOns.IsScenario(target);
			_installFramework.Visible = isRustFramework || isScenario;
			_installFramework.Text = LocalizationManager.Get(isScenario ? "EmpyrionMods.Button.ChooseScenario" : "ModManager.Button.InstallFramework");
			_installFramework.Enabled = !_busy && !_scanFailed && stopped && standardUser &&
				(isScenario ? canManage : isRustFramework && !(_detection?.FrameworkDetected ?? false));

			if (_safetyItems.Length == 0)
				return;
			SetSafety(0, stopped, LocalizationManager.Get(stopped
				? "ModManager.Safety.ServerStopped"
				: "ModManager.Safety.StopFirst"));
			SetSafety(1, _detection?.FrameworkDetected == true,
				LocalizationManager.Get(profile?.UserConfigured == true ? "UniversalMods.Locations.Selected" : _detection?.FrameworkDetected == true
					? "ModManager.Safety.FrameworkDetected"
					: "ModManager.Safety.FrameworkRequired"));
			SetSafety(2, Directory.Exists(_server.InstallPath),
				LocalizationManager.Get(Directory.Exists(_server.InstallPath)
					? "ModManager.Safety.FolderAvailable"
					: "ModManager.Safety.FolderMissing"));
			SetSafety(3, target?.CanManageIds != true, target?.CanManageIds == true
				? LocalizationManager.Get("ModManager.Safety.ProviderTrust")
				: LocalizationManager.Get("ModManager.Safety.SecurityScan"));
			SetSafety(4, standardUser, standardUser
				? LocalizationManager.Get("ModManager.Safety.StandardPermissions")
				: LocalizationManager.Get("ModManager.Safety.RestartWithoutAdmin"));
			SetSafety(5, profile?.RestartRequired == false,
				profile?.RestartRequired != false
					? LocalizationManager.Get("ModManager.Safety.RestartRequired")
					: LocalizationManager.Get("ModManager.Safety.LiveReload"));
		}

		private void SetSafety(int index, bool passed, string text)
		{
			_safetyItems[index].Text = $"{(passed ? "✓" : "!")}  {text}";
			_safetyItems[index].ForeColor = passed ? SettingsPalette.Success : SettingsPalette.Warning;
		}

		private void SelectionChanged()
		{
			if (_renderingInventory) return;
			ModInventoryItem? item = SelectedItem();
			_selectionDetails.Text = item == null
				? LocalizationManager.Get("ModManager.Selection.Empty")
				: $"{item.Name}{Environment.NewLine}{LocalizationManager.TranslateKnownText(item.SecurityStatus)}";
			UpdateButtonsAndSafety();
		}

		private async void InstallFile_Click(object? sender, EventArgs eventArgs)
		{
			if (_busy) return;
			using ModImportWizard wizard = new(_server);
			try
			{
				if (wizard.ShowDialog(this) != DialogResult.OK || wizard.Selection is not ModImportChoice selected) return;
				ModSystemProfile profile = selected.Profile;
				ModInstallTarget target = selected.Target;
				string packagePath = wizard.PackagePath;
				string sourceName = Path.GetFileName(packagePath);
				string? scenarioFolder = selected.Selection.Length == 0 ? null : selected.Selection;
				string reviewedHash = wizard.PackageSha256;
				if (target.CanManageIds)
				{
					await ManageProviderIdsAsync(target, package: wizard);
					return;
				}
				SetBusy(true, "Text.B38F7F9752C285B4B90B");
				using ServerOperationLease operation = ModPackageManager.BeginOperation(_server);
				ModPackageManager.EnsureStopped(_server);
				ModSecurityReview review = await ModSecurityScanner.ReviewPackageAsync(
					packagePath,
					target);
				if (reviewedHash != null && !reviewedHash.Equals(review.PackageSha256, StringComparison.OrdinalIgnoreCase))
					throw new InvalidDataException(LocalizationManager.Get("ModManager.Error.ChangedAfterReview"));
				if (review.Outcome == ModSecurityOutcome.Blocked)
				{
					_inventorySummary.Text = LocalizationManager.Get("Text.4CDDB86B34725DFF943D");
					_inventorySummary.ForeColor = SettingsPalette.Warning;
					LocalizedMessageBox.Show(
						this,
						LocalizationManager.TranslateRuntimeText(review.BuildUserMessage()),
						LocalizationManager.Get("MessageText.A2AB176254809E1818BD"),
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);
					return;
				}

				bool replacingScenario = EmpyrionAddOns.IsScenario(target) && scenarioFolder != null && Directory.Exists(ModPathSafety.Resolve(
					_server.InstallPath, target.RelativePath + "/" + scenarioFolder));
				string reviewMessage = LocalizationManager.TranslateRuntimeText(review.BuildUserMessage());
				if (replacingScenario)
					reviewMessage += Environment.NewLine + Environment.NewLine + LocalizationManager.Get("EmpyrionMods.Import.ExistingScenario");
				DialogResult confirmation = LocalizedMessageBox.Show(
					this,
					LocalizationManager.Get(
						"ModManager.SecurityReview.Confirm",
						reviewMessage,
						sourceName,
						LocalizationManager.TranslateKnownText(target.DisplayName) +
							(EmpyrionAddOns.IsScenario(target) && scenarioFolder != null ? " / " + scenarioFolder : "")),
					LocalizationManager.Get("MessageText.601D435F77F129E56270"),
					MessageBoxButtons.OKCancel,
					!replacingScenario && review.Outcome == ModSecurityOutcome.Passed
						? MessageBoxIcon.Information
						: MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
				if (confirmation != DialogResult.OK)
				{
					_inventorySummary.Text = LocalizationManager.Get("Text.90C149B626C5531C627C");
					return;
				}

				ModImportSecurityContext securityContext = ModImportSecurityContext.CaptureCurrent();
				if (selected.RememberLocation)
				{
					profile = UniversalModImports.SaveLocation(_server, target.DisplayName, target.RelativePath,
						string.Join(',', target.AllowedExtensions), target.Kind);
					target = profile.Targets.Single(candidate => candidate.Id == target.Id);
				}
				SelectImportedTarget(profile.Id, target.Id);
				_inventorySummary.Text = LocalizationManager.Get("ModManager.Progress.Installing");
				ModImportResult result = await Task.Run(() => ModPackageManager.Import(
					_server,
					profile,
					target,
					packagePath,
					review.PackageSha256,
					review.AntivirusStatus, securityContext, scenarioFolder));
				ApplicationLogService.WriteLocalized(
					"ModManager.Activity.Installed",
					Color.LimeGreen,
					arguments:
					[
						result.DisplayName,
						result.InstalledFileCount,
						_server.ServerName
					]);
				await RefreshInventory(duringOperation: true);
				LocalizedMessageBox.Show(
					this,
					LocalizationManager.Get(
						target.PackageLayout == ModPackageLayout.FolderTree ? "UniversalMods.Import.Done" :
						EmpyrionAddOns.IsScenario(target) ? "EmpyrionMods.ImportedScenario" : result.RestartRequired
							? "MessageText.3679B9A889A4F6F470DA"
							: "MessageText.F23CB309225C503B6D8A") + Environment.NewLine + Environment.NewLine +
						ModInstallGuidance.Create(_server, selected).NextSteps(),
					LocalizationManager.Get("MessageText.2CEE7AAFEDF4FEA66592"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
			catch (Exception exception)
			{
				InvalidateInventory();
				PlainEnglishErrorDialog.ShowError(
					this,
					LocalizationManager.Get("ModManager.ErrorAction.Install"),
					exception.Message);
			}
			finally
			{
				SetBusy(false);
			}
		}

		private void SelectImportedTarget(string profileId, string targetId)
		{
			_profiles = ModSystemCatalog.GetProfiles(_server);
			_updatingSelectors = true;
			try
			{
				_profileBox.Items.Clear();
				foreach (ModSystemProfile profile in _profiles) _profileBox.Items.Add(profile);
				_profileBox.SelectedItem = _profiles.FirstOrDefault(profile => profile.Id == profileId);
				_targetBox.Items.Clear();
				if (_profileBox.SelectedItem is ModSystemProfile selected)
				{
					foreach (ModInstallTarget target in selected.Targets) _targetBox.Items.Add(target);
					_targetBox.SelectedItem = selected.Targets.FirstOrDefault(target => target.Id == targetId);
					_detection = ModSystemCatalog.Detect(_server, selected);
				}
			}
			finally { _updatingSelectors = false; }
		}

		private void ConfigureImportLocations()
		{
			if (_busy) return;
			try
			{
				using ModImportLocationDialog dialog = new(_server);
				if (dialog.ShowDialog(this) != DialogResult.OK || dialog.ImportLocation is not ModInstallTarget target) return;
				SetBusy(true);
				ModSystemProfile saved = UniversalModImports.SaveLocation(_server, target.DisplayName, target.RelativePath,
					string.Join(',', target.AllowedExtensions), target.Kind);
				_profiles = ModSystemCatalog.GetProfiles(_server);
				_updatingSelectors = true;
				try
				{
					_profileBox.Items.Clear();
					foreach (ModSystemProfile profile in _profiles) _profileBox.Items.Add(profile);
					_profileBox.SelectedIndex = _profiles.ToList().FindIndex(profile => profile.Id == saved.Id);
				}
				finally { _updatingSelectors = false; }
			}
			catch (Exception exception)
			{
				PlainEnglishErrorDialog.ShowError(this, LocalizationManager.Get("UniversalMods.Locations.Title"), exception.Message);
			}
			finally { SetBusy(false); }
			ProfileChanged();
		}

		private async Task ChooseEmpyrionScenarioAsync()
		{
			if (_busy) return;
			try
			{
				using EmpyrionScenarioPicker picker = new(_server);
				if (picker.ShowDialog(this) != DialogResult.OK) return;
				if (LocalizedMessageBox.Show(this,
					LocalizationManager.Get("EmpyrionMods.Selection.Confirm", picker.Scenario, picker.SaveName),
					LocalizationManager.Get("EmpyrionMods.Button.ChooseScenario"), MessageBoxButtons.OKCancel,
					MessageBoxIcon.Warning) != DialogResult.OK) return;
				SetBusy(true);
				EmpyrionAddOns.SelectScenario(_server, picker.Scenario, picker.SaveName,
					() => FileHandler.SaveServers(), picker.OriginalSelection);
				await RefreshInventory(duringOperation: true);
				LocalizedMessageBox.Show(this, LocalizationManager.Get("EmpyrionMods.Selection.Saved"),
					LocalizationManager.Get("EmpyrionMods.Button.ChooseScenario"), MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception exception)
			{
				InvalidateInventory();
				PlainEnglishErrorDialog.ShowError(this, LocalizationManager.Get("EmpyrionMods.Button.ChooseScenario"), exception.Message);
			}
			finally { SetBusy(false); }
		}

		private async void RemoveSelected_Click(object? sender, EventArgs eventArgs)
		{
			ModInventoryItem? item = SelectedItem();
			if (_busy || item?.InstallationId == null || !item.CanRemove)
				return;
			ModInstallTarget? target = (_profileBox.SelectedItem as ModSystemProfile)?.Targets
				.FirstOrDefault(candidate => candidate.Id.Equals(item.TargetId, StringComparison.OrdinalIgnoreCase));
			if (target == null) return;
			if (LocalizedMessageBox.Show(
				this,
				target.PackageLayout == ModPackageLayout.FolderTree
					? LocalizationManager.Get("UniversalMods.Import.Rollback", item.ImportedPackageName, item.ImportedFileCount)
					: LocalizationManager.Get(target.CanImport ? "EmpyrionMods.Remove.Confirm" : "ModManager.Remove.Confirm", item.Name),
				LocalizationManager.Get("MessageText.1836C66D2E22132440F9"),
				MessageBoxButtons.OKCancel,
				MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.OK)
			{
				return;
			}

			try
			{
				SetBusy(true, "ModManager.Progress.Removing");
				using ServerOperationLease operation = ModPackageManager.BeginOperation(_server);
				if (target.CanManageIds)
				{
					IReadOnlyList<string> current = ModPackageManager.GetProviderIds(_server, target);
					ModPackageManager.SaveProviderIds(
						_server,
						target,
						current.Where(id => !id.Equals(item.Name, StringComparison.Ordinal)), () => FileHandler.SaveServers());
					ApplicationLogService.WriteLocalized(
						"ModManager.Activity.ProviderIdRemoved",
						Color.LimeGreen,
						arguments: [item.Name, _server.ServerName]);
					await RefreshInventory(duringOperation: true);
					return;
				}

				string removed = await Task.Run(() => ModPackageManager.Remove(_server, item.InstallationId));
				ApplicationLogService.WriteLocalized(
					"ModManager.Activity.Removed",
					Color.LimeGreen,
					arguments: [removed, _server.ServerName]);
				await RefreshInventory(duringOperation: true);
			}
			catch (Exception exception)
			{
				InvalidateInventory();
				PlainEnglishErrorDialog.ShowError(
					this,
					LocalizationManager.Get("ModManager.ErrorAction.Remove"),
					exception.Message);
			}
			finally { SetBusy(false); }
		}

		private async Task ManageProviderIdsAsync(ModInstallTarget target, ModImportWizard? package = null)
		{
			if (_busy) return;
			try
			{
				SetBusy(true);
				using ServerOperationLease operation = ModPackageManager.BeginOperation(_server);
				IReadOnlyList<string> current = ModPackageManager.GetProviderIds(_server, target);
				if (package != null)
				{
					if (!ArkModPackageReader.Supports(_server.Game, target)) return;
					ModPackageManager.EnsureStopped(_server);
					ModPackagePreview check = await Task.Run(() => UniversalModPackage.Preview(_server, ModImportDiscovery.SnapshotTarget, package.PackagePath));
					if (check.PackageSha256 != package.PackageSha256) throw new InvalidDataException(LocalizationManager.Get("ModManager.Error.ChangedAfterReview"));
					current = ModPackageManager.NormalizeProviderIds(string.Join(',', current.Concat(package.ProviderMods.Select(mod => mod.ModId))), target.MaximumIds);
					if (package.Selection != null) SelectImportedTarget(package.Selection.Profile.Id, target.Id);
				}
				ModSystemProfile profile = (_profileBox.SelectedItem as ModSystemProfile) ??
					throw new InvalidOperationException(LocalizationManager.Get("ModManager.Error.RecordMissing"));
				ModInstallGuidance guidance = ModInstallGuidance.Create(_server, new(profile, target, "", true));
				using ProviderModIdEditor dialog = new(
					string.IsNullOrWhiteSpace(target.ProviderName) ? LocalizationManager.Get("ModManager.Known.GameProvider") : target.ProviderName,
					target.MaximumIds, current, guidance);
				if (dialog.ShowDialog(this) != DialogResult.OK)
					return;
				if (LocalizedMessageBox.Show(
					this,
					LocalizationManager.Get(
						"ModManager.Provider.Warning",
						target.ProviderName),
					LocalizationManager.Get("MessageText.468EADDB3CA79D6CD85C"),
					MessageBoxButtons.OKCancel,
					MessageBoxIcon.Warning) != DialogResult.OK)
				{
					return;
				}

				ModPackageManager.SaveProviderIds(_server, target, dialog.ModIds, () => FileHandler.SaveServers());
				ApplicationLogService.WriteLocalized(
					"ModManager.Activity.ProviderIdsSaved",
					Color.LimeGreen,
					arguments:
					[
						dialog.ModIds.Count,
						target.ProviderName,
						_server.ServerName
					]);
				await RefreshInventory(duringOperation: true);
				LocalizedMessageBox.Show(
					this,
					LocalizationManager.Get("MessageText.2A2FCD5BFFF42E6401AE") + Environment.NewLine + Environment.NewLine + guidance.NextSteps(),
					LocalizationManager.Get("MessageText.69CA929D937D47644BA3"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
			catch (Exception exception)
			{
				InvalidateInventory();
				PlainEnglishErrorDialog.ShowError(
					this,
					LocalizationManager.Get("ModManager.ErrorAction.SaveIds"),
					exception.Message);
			}
			finally { SetBusy(false); }
		}

		private async Task InstallFrameworkAsync()
		{
			if (_busy) return;
			ModSystemProfile? profile = _profileBox.SelectedItem as ModSystemProfile;
			if (profile?.Id.Equals("rust-umod", StringComparison.OrdinalIgnoreCase) != true)
				return;
			GameInfo? definition = GameDatabase.GetGame(_server.Game);
			if (definition == null)
				return;

			string previousFramework = _server.ServerFramework;
			string previousVersion = _server.ServerFrameworkVersion;
			bool frameworkApplied = false;
			try
			{
				SetBusy(true, "ModManager.Progress.Framework");
				using ServerOperationLease operation = ModPackageManager.BeginOperation(_server);
				ModPackageManager.EnsureStopped(_server);
				_server.ServerFramework = OxideRuntimeManager.FrameworkName;
				string version = await Task.Run(() => OxideRuntimeManager.InstallOrUpdateAsync(
					_server,
					definition,
					(message, color) => ApplicationLogService.Write(message, color)));
				_server.ServerFrameworkVersion = version;
				frameworkApplied = true;
				if (!FileHandler.SaveServers())
					throw new IOException(LocalizationManager.Get("ModManager.Framework.SaveFailed"));
				await RefreshInventory(duringOperation: true);
				LocalizedMessageBox.Show(
					this,
					LocalizationManager.Get("ModManager.Framework.Installed", version),
					LocalizationManager.Get("MessageText.0E2A59C8DC9861D1E462"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
			catch (Exception exception)
			{
				if (!frameworkApplied)
				{
					_server.ServerFramework = previousFramework;
					_server.ServerFrameworkVersion = previousVersion;
				}
				InvalidateInventory();
				PlainEnglishErrorDialog.ShowError(
					this,
					LocalizationManager.Get("ModManager.ErrorAction.InstallFramework"),
					exception.Message);
			}
			finally
			{
				SetBusy(false);
			}
		}

		private void InvalidateInventory()
		{
			_scanFailed = true;
			_items = [];
			RenderInventory();
			_inventorySummary.Text = LocalizationManager.Get("ModManager.Progress.Failed");
			_inventorySummary.ForeColor = SettingsPalette.Warning;
		}

		private void BrowseCatalog()
		{
			if (_profileBox.SelectedItem is not ModSystemProfile profile)
				return;
			IReadOnlyList<CatalogChoice> catalogs = GetCatalogChoices(profile);
			if (catalogs.Count == 0)
				return;
			if (catalogs.Count == 1)
			{
				OpenCatalog(catalogs[0].Uri);
				return;
			}

			_catalogMenu.Items.Clear();
			foreach (CatalogChoice catalog in catalogs)
			{
				ToolStripMenuItem item = new(catalog.Name)
				{
					ForeColor = SettingsPalette.PrimaryText,
					BackColor = SettingsPalette.Card
				};
				item.Click += (_, _) => OpenCatalog(catalog.Uri);
				_catalogMenu.Items.Add(item);
			}
			SynixMenuStyler.Apply(_catalogMenu);
			_catalogMenu.Show(_browseCatalog, new Point(0, _browseCatalog.Height + 4));
		}

		private void OpenCatalog(Uri uri)
		{
			try
			{
				Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
			}
			catch (Exception exception)
			{
				PlainEnglishErrorDialog.ShowError(
					this,
					LocalizationManager.Get("ModManager.ErrorAction.OpenCatalog"),
					exception.Message);
			}
		}

		private static IReadOnlyList<CatalogChoice> GetCatalogChoices(ModSystemProfile? profile)
		{
			if (profile == null)
				return [];
			List<CatalogChoice> choices = [];
			HashSet<string> urls = new(StringComparer.OrdinalIgnoreCase);
			foreach (ModCatalogLink catalog in profile.Catalogs)
			{
				if (IsSafeCatalogUri(catalog.Url, out Uri? uri) && urls.Add(uri!.AbsoluteUri))
					choices.Add(new CatalogChoice(catalog.Name.Trim(), uri));
			}
			if (IsSafeCatalogUri(profile.CatalogUrl, out Uri? legacyUri) &&
				urls.Add(legacyUri!.AbsoluteUri))
			{
				choices.Add(new CatalogChoice(
					LocalizationManager.Get("ModManager.Catalog.Default"),
					legacyUri));
			}
			return choices;
		}

		private void OpenAddOnsFolder()
		{
			string? path = GetSelectedTargetPath();
			if (string.IsNullOrWhiteSpace(path))
				return;
			try
			{
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
				Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"")
				{
					UseShellExecute = true
				});
			}
			catch (Exception exception)
			{
				PlainEnglishErrorDialog.ShowError(
					this,
					LocalizationManager.Get("ModManager.ErrorAction.OpenFolder"),
					exception.Message);
			}
		}

		private string? GetSelectedTargetPath()
		{
			if (_targetBox.SelectedItem is not ModInstallTarget target || target.CanManageIds)
				return null;
			return ModSystemCatalog.ResolveInsideInstallPath(_server.InstallPath, target.RelativePath);
		}

		private ModInventoryItem? SelectedItem() =>
			_grid.CurrentRow?.Tag as ModInventoryItem;

		private void ApplySimpleView()
		{
			_importLocations.Visible = !_simpleView.Checked;
			if (_grid.Columns.Count < 7)
				return;
			_grid.Columns[5].Visible = !_simpleView.Checked;
			_grid.Columns[6].Visible = !_simpleView.Checked;
		}

		private void AddColumn(string name, string heading, int width, bool frozen = false)
		{
			_grid.Columns.Add(new DataGridViewTextBoxColumn
			{
				Name = name,
				HeaderText = heading,
				Width = width,
				MinimumWidth = Math.Min(width, 80),
				AutoSizeMode = name == "Name"
					? DataGridViewAutoSizeColumnMode.Fill
					: DataGridViewAutoSizeColumnMode.None,
				FillWeight = 100,
				Frozen = frozen
			});
		}

		private static bool IsSafeCatalogUri(string value, out Uri? uri)
		{
			uri = null;
			if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? candidate) ||
				candidate.Scheme != Uri.UriSchemeHttps ||
				!candidate.IsDefaultPort ||
				!string.IsNullOrEmpty(candidate.UserInfo))
			{
				return false;
			}
			uri = candidate;
			return true;
		}

		private sealed record CatalogChoice(string Name, Uri Uri);

		private static ModernSettingsCard Card(int left, int top, int width, int height) => new()
		{
			Location = new Point(left, top),
			Size = new Size(width, height),
			FillColor = SettingsPalette.Card,
			BorderColor = SettingsPalette.Border,
			Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
		};

		private static Label Heading(
			string text,
			int left,
			int top,
			int width,
			int height,
			float size) => new()
		{
			Text = text,
			Location = new Point(left, top),
			Size = new Size(width, height),
			Font = new Font("Segoe UI", size, FontStyle.Bold),
			UseMnemonic = false,
			ForeColor = SettingsPalette.PrimaryText
		};

		private static Label Body(string text, int left, int top, int width, int height) => new()
		{
			Text = text,
			Location = new Point(left, top),
			Size = new Size(width, height),
			UseMnemonic = false,
			ForeColor = SettingsPalette.SecondaryText
		};

		private static Label FieldLabel(string text, int left, int top, int width) => new()
		{
			Text = text,
			Location = new Point(left, top),
			Size = new Size(width, 22),
			Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
			ForeColor = SettingsPalette.SecondaryText
		};

		private static ModernSettingsButton Button(
			string text,
			int left,
			int top,
			int width,
			bool accent = false) => new()
		{
			Text = text,
			Location = new Point(left, top),
			Size = new Size(width, 42),
			UseAccentStyle = accent,
			Anchor = AnchorStyles.Bottom | AnchorStyles.Left
		};
	}

	internal sealed class ProviderModIdEditor : Form
	{
		private readonly TextBox _ids;
		private readonly Label _status;
		private readonly int _maximumIds;

		internal IReadOnlyList<string> ModIds { get; private set; } = [];

		internal ProviderModIdEditor(
			string providerName,
			int maximumIds,
			IReadOnlyList<string> currentIds,
			ModInstallGuidance guidance)
		{
			_maximumIds = maximumIds;
			Text = LocalizationManager.Get("Text.FED14E20C68A4626A601");
			StartPosition = FormStartPosition.CenterParent;
			ShowInTaskbar = false;
			MinimizeBox = false;
			ClientSize = new Size(1060, 740);
			MinimumSize = new Size(960, 700);
			BackColor = SettingsPalette.Window;
			ForeColor = SettingsPalette.PrimaryText;
			Font = new Font("Segoe UI", 9.5F);
			TableLayoutPanel layout = UniversalModDialogStyle.Layout(this, [48, 58, -1, 48, 48]);
			layout.ColumnCount = 2;
			layout.ColumnStyles.Clear();
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

			Label title = new()
			{
				Text = LocalizationManager.Get(
					"ModManager.Provider.Title",
					providerName),
				Dock = DockStyle.Fill,
				UseMnemonic = false,
				Font = new Font("Segoe UI", 18F, FontStyle.Bold),
				ForeColor = SettingsPalette.PrimaryText
			};
			layout.Controls.Add(title, 0, 0);
			layout.SetColumnSpan(title, 2);
			Label help = new()
			{
				Text = LocalizationManager.Get("Text.518F7A1D974E46AED149"),
				Dock = DockStyle.Fill,
				UseMnemonic = false,
				ForeColor = SettingsPalette.SecondaryText
			};
			layout.Controls.Add(help, 0, 1);
			layout.SetColumnSpan(help, 2);
			_ids = new TextBox
			{
				Name = "providerModIds",
				AccessibleName = title.Text,
				Dock = DockStyle.Fill,
				Multiline = true,
				ScrollBars = ScrollBars.Vertical,
				AcceptsReturn = true,
				Text = string.Join(Environment.NewLine, currentIds),
				BackColor = SettingsPalette.Input,
				ForeColor = SettingsPalette.PrimaryText,
				BorderStyle = BorderStyle.FixedSingle,
				Font = new Font("Cascadia Mono", 10F)
			};
			_ids.TextChanged += (_, _) => ValidateIds();
			layout.Controls.Add(_ids, 0, 2);
			ModInstallGuidancePanel review = new() { Margin = new Padding(12, 3, 3, 3) };
			review.ShowGuidance(guidance);
			layout.Controls.Add(review, 1, 2);
			layout.SetRowSpan(review, 2);
			_status = new Label
			{
				Dock = DockStyle.Fill,
				ForeColor = SettingsPalette.SecondaryText
			};
			layout.Controls.Add(_status, 0, 3);

			ModernSettingsButton cancel = new()
			{
				Name = "cancelProviderModIds",
				Text = LocalizationManager.Get("Text.19766ED6CCB2F4A32778"),
				AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink,
				MinimumSize = new Size(180, 42),
				Padding = new Padding(12, 0, 12, 0),
				DialogResult = DialogResult.Cancel
			};
			ModernSettingsButton save = new()
			{
				Name = "saveProviderModIds",
				Text = LocalizationManager.Get("Text.A9DE31384881AF4D5B60"),
				AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink,
				MinimumSize = new Size(240, 42),
				Padding = new Padding(12, 0, 12, 0),
				UseAccentStyle = true
			};
			save.Click += (_, _) => Save();
			FlowLayoutPanel actions = UniversalModDialogStyle.Actions(layout, 4);
			layout.SetColumnSpan(actions, 2);
			actions.Controls.AddRange([save, cancel]);
			CancelButton = cancel;
			ValidateIds();
			ThemeManager.Apply(this);
		}

		private void ValidateIds()
		{
			try
			{
				ModIds = ModPackageManager.NormalizeProviderIds(_ids.Text, _maximumIds);
				LocalizationManager.BindText(
					_status,
					"ModManager.Provider.ValidCount",
					ModIds.Count);
				_status.ForeColor = SettingsPalette.Success;
			}
			catch (Exception exception)
			{
				ModIds = [];
				_status.Text = LocalizationManager.TranslateRuntimeText(
					exception.Message);
				_status.ForeColor = SettingsPalette.Warning;
			}
		}

		private void Save()
		{
			try
			{
				ModIds = ModPackageManager.NormalizeProviderIds(_ids.Text, _maximumIds);
				DialogResult = DialogResult.OK;
				Close();
			}
			catch (Exception exception)
			{
				_status.Text = LocalizationManager.TranslateRuntimeText(
					exception.Message);
				_status.ForeColor = SettingsPalette.Warning;
			}
		}
	}
}
