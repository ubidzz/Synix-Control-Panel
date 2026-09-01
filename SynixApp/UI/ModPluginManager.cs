// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine.ModManagement;
using System.Diagnostics;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel.SynixEngine
{
	internal sealed class ModPluginManager : Form
	{
		private readonly GameServer _server;
		private readonly IReadOnlyList<ModSystemProfile> _profiles;
		private readonly ModernSettingsComboBox _profileBox;
		private readonly ModernSettingsComboBox _targetBox;
		private readonly ModernSettingsToggle _simpleView;
		private readonly Label _supportTitle;
		private readonly Label _supportDetails;
		private readonly DataGridView _grid;
		private readonly Label _inventorySummary;
		private readonly Label _selectionDetails;
		private readonly Label[] _safetyItems;
		private readonly ModernSettingsButton _installFile;
		private readonly ModernSettingsButton _remove;
		private readonly ModernSettingsButton _installFramework;
		private readonly ModernSettingsButton _browseCatalog;
		private readonly ModernSettingsButton _openFolder;
		private ModSystemDetection? _detection;
		private IReadOnlyList<ModInventoryItem> _items = [];
		private bool _updatingSelectors;
		private bool _hasShown;

		internal ModPluginManager(GameServer server)
		{
			_server = server ?? throw new ArgumentNullException(nameof(server));
			_profiles = ModSystemCatalog.GetProfiles(server);
			Text = "Mod & Plugin Manager";
			StartPosition = FormStartPosition.CenterParent;
			ShowInTaskbar = false;
			MinimumSize = new Size(1240, 760);
			ClientSize = new Size(1240, 760);
			BackColor = SettingsPalette.Window;
			ForeColor = SettingsPalette.PrimaryText;
			Font = new Font("Segoe UI", 9.5F);

			Label pageHeading = Heading("Mod & Plugin Manager", 28, 20, 640, 42, 19F);
			pageHeading.Name = "modPluginManagerHeading";
			Controls.Add(pageHeading);
			Controls.Add(Body(
				"Discover what is already installed, safely add local packages, and keep a rollback record without maintaining a list of every mod.",
				30, 62, 890, 42));
			Controls.Add(FieldLabel("SERVER", 30, 108, 110));
			Controls.Add(new Label
			{
				Text = $"{_server.ServerName}  •  {_server.Game}",
				Location = new Point(30, 130),
				Size = new Size(360, 30),
				Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
				ForeColor = SettingsPalette.PrimaryText
			});

			Controls.Add(FieldLabel("ADD-ON SYSTEM", 404, 108, 160));
			_profileBox = new ModernSettingsComboBox
			{
				Location = new Point(404, 130),
				Size = new Size(250, 36),
				DisplayMember = nameof(ModSystemProfile.DisplayName),
				Enabled = _profiles.Count > 1
			};
			foreach (ModSystemProfile profile in _profiles)
				_profileBox.Items.Add(profile);
			_profileBox.SelectedIndexChanged += (_, _) => ProfileChanged();
			Controls.Add(_profileBox);

			Controls.Add(FieldLabel("INSTALL AREA", 668, 108, 150));
			_targetBox = new ModernSettingsComboBox
			{
				Location = new Point(668, 130),
				Size = new Size(238, 36),
				DisplayMember = nameof(ModInstallTarget.DisplayName)
			};
			_targetBox.SelectedIndexChanged += (_, _) => UpdateButtonsAndSafety();
			Controls.Add(_targetBox);

			Controls.Add(new Label
			{
				Text = "Simple view",
				Location = new Point(1022, 114),
				Size = new Size(104, 24),
				ForeColor = SettingsPalette.SecondaryText,
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			});
			_simpleView = new ModernSettingsToggle
			{
				Location = new Point(1134, 108),
				Checked = true,
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			};
			_simpleView.CheckedChanged += (_, _) => ApplySimpleView();
			Controls.Add(_simpleView);

			ModernSettingsCard supportCard = Card(28, 176, 1184, 78);
			_supportTitle = Heading("Checking support…", 18, 12, 720, 26, 11F);
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
			string[] steps = ["1  Detect", "2  Stop server", "3  Back up files", "4  Install", "5  Verify", "6  Restart if needed"];
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

			_grid = new DataGridView
			{
				Name = "addOnInventoryGrid",
				Location = new Point(28, 326),
				Size = new Size(844, 326),
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
			AddColumn("Name", "ADD-ON", 220);
			AddColumn("Type", "TYPE", 80);
			AddColumn("Version", "VERSION", 130);
			AddColumn("Status", "STATUS", 170);
			AddColumn("Security", "SECURITY", 190);
			AddColumn("Source", "SOURCE", 130);
			AddColumn("Location", "LOCATION", 300);
			GridStyler.DarkTheme(_grid);
			GridStyler.ApplyDashboardTheme(_grid);
			_grid.SelectionChanged += (_, _) => SelectionChanged();
			Controls.Add(_grid);

			ModernSettingsCard safetyCard = Card(890, 326, 322, 240);
			safetyCard.Name = "automaticSafetyChecklistCard";
			safetyCard.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
			safetyCard.Controls.Add(Heading("Automatic Safety Checklist", 18, 14, 282, 28, 11F));
			safetyCard.Controls.Add(Body(
				"Synix checks these before it changes anything.",
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
				"Select an add-on to see where it was found.",
				18, 11, 282, 54);
			_selectionDetails.Name = "selectedAddOnDetails";
			_selectionDetails.UseMnemonic = false;
			_selectionDetails.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
			selectionCard.Controls.Add(_selectionDetails);
			Controls.Add(selectionCard);

			_inventorySummary = new Label
			{
				Location = new Point(30, 664),
				Size = new Size(610, 28),
				ForeColor = SettingsPalette.SecondaryText,
				Anchor = AnchorStyles.Bottom | AnchorStyles.Left
			};
			Controls.Add(_inventorySummary);

			_installFile = Button("Install From File", 28, 702, 156, accent: true);
			_installFile.Click += InstallFile_Click;
			_installFramework = Button("Install Framework", 194, 702, 164);
			_installFramework.Click += async (_, _) => await InstallFrameworkAsync();
			_browseCatalog = Button("Browse Catalog", 368, 702, 150);
			_browseCatalog.Click += (_, _) => BrowseCatalog();
			_openFolder = Button("Open Add-ons Folder", 528, 702, 172);
			_openFolder.Click += (_, _) => OpenAddOnsFolder();
			ModernSettingsButton refresh = Button("Refresh", 710, 702, 112);
			refresh.Click += (_, _) => RefreshInventory();
			_remove = Button("Remove Selected", 878, 702, 150);
			_remove.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_remove.Click += RemoveSelected_Click;
			ModernSettingsButton close = Button("Close", 1038, 702, 174);
			close.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			close.DialogResult = DialogResult.OK;
			Controls.AddRange([
				_installFile, _installFramework, _browseCatalog, _openFolder,
				refresh, _remove, close]);
			CancelButton = close;

			if (_profiles.Count > 0)
				_profileBox.SelectedIndex = 0;
			else
				ShowUnsupportedState();
			ApplySimpleView();
			ThemeManager.Apply(this);
		}

		protected override void OnShown(EventArgs eventArgs)
		{
			base.OnShown(eventArgs);
			_hasShown = true;
			RefreshInventory();
		}

		private void ProfileChanged()
		{
			if (_updatingSelectors || _profileBox.SelectedItem is not ModSystemProfile profile)
				return;
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
				RefreshInventory();
			}
			else
			{
				UpdateSupportBanner();
				UpdateButtonsAndSafety();
			}
		}

		private void RefreshInventory()
		{
			if (_profileBox.SelectedItem is not ModSystemProfile profile)
			{
				ShowUnsupportedState();
				return;
			}

			try
			{
				_detection = ModSystemCatalog.Detect(_server, profile);
				_items = ModPackageManager.Scan(_server, profile);
				_grid.Rows.Clear();
				foreach (ModInventoryItem item in _items)
				{
					int rowIndex = _grid.Rows.Add(
						item.Name,
						item.Type,
						item.Version,
						item.Status,
						item.SecurityStatus,
						item.Source,
						item.RelativePath);
					_grid.Rows[rowIndex].Tag = item;
					_grid.Rows[rowIndex].Cells[3].Style.ForeColor = item.Status switch
					{
						"Healthy" => SettingsPalette.Success,
						"Changed outside Synix" => SettingsPalette.Warning,
						_ => SettingsPalette.SecondaryText
					};
				}
				_inventorySummary.Text = _items.Count == 0
					? "No add-ons were found in the active profile folders."
					: $"{_items.Count} add-on{(_items.Count == 1 ? string.Empty : "s")} found  •  {_items.Count(item => item.Status == "Healthy")} tracked by Synix";
				UpdateSupportBanner();
				SelectionChanged();
				UpdateButtonsAndSafety();
			}
			catch (Exception exception)
			{
				_inventorySummary.Text = "Synix could not refresh the add-on folders.";
				_inventorySummary.ForeColor = SettingsPalette.Warning;
				PlainEnglishErrorDialog.ShowError(this, "scan the server add-ons", exception.Message);
			}
		}

		private void UpdateSupportBanner()
		{
			if (_detection == null)
			{
				ShowUnsupportedState();
				return;
			}

			_supportTitle.Text = _detection.SupportText;
			_supportTitle.ForeColor = _detection.Profile.SupportLevel == ModSystemSupportLevel.DetectedOnly
				? SettingsPalette.Warning
				: _detection.FrameworkDetected ? SettingsPalette.Success : SettingsPalette.Warning;
			string framework = string.IsNullOrWhiteSpace(_detection.Profile.FrameworkName)
				? "The server loader and existing folders choose the install area automatically."
				: $"Framework: {_detection.Profile.FrameworkName}.";
			_supportDetails.Text = $"{_detection.Profile.Description}  {framework}";
		}

		private void ShowUnsupportedState()
		{
			_supportTitle.Text = "NO ADD-ON PROFILE YET";
			_supportTitle.ForeColor = SettingsPalette.Warning;
			_supportDetails.Text =
				"Synix will not guess where this game stores mods. A small data profile can add support later without rewriting this window.";
			_inventorySummary.Text = "No files were changed.";
			_grid.Rows.Clear();
			UpdateButtonsAndSafety();
		}

		private void UpdateButtonsAndSafety()
		{
			ModSystemProfile? profile = _profileBox.SelectedItem as ModSystemProfile;
			ModInstallTarget? target = _targetBox.SelectedItem as ModInstallTarget;
			bool stopped = _server.Status == StatusManager.GetStatus(ServerState.Stopped);
			bool standardUser = !ModSecurityScanner.IsCurrentProcessElevated();
			bool canManage = profile != null && target != null && target.CanManage && standardUser &&
				profile.SupportLevel == ModSystemSupportLevel.Managed &&
				(_detection?.FrameworkDetected ?? false);
			_installFile.Text = target?.CanManageIds == true ? "Manage Mod IDs" : "Install From File";
			_installFile.Enabled = stopped && canManage;
			_remove.Enabled = stopped && SelectedItem()?.CanRemove == true;
			_openFolder.Visible = target?.CanManageIds != true;
			_openFolder.Enabled = target != null && !target.CanManageIds &&
				(profile?.SupportLevel != ModSystemSupportLevel.DetectedOnly ||
				Directory.Exists(GetSelectedTargetPath()));
			_browseCatalog.Enabled = profile != null && IsSafeCatalogUri(profile.CatalogUrl, out _);
			bool isRustFramework = profile?.Id.Equals("rust-umod", StringComparison.OrdinalIgnoreCase) == true;
			_installFramework.Visible = isRustFramework;
			_installFramework.Enabled = isRustFramework && stopped && !(_detection?.FrameworkDetected ?? false);

			if (_safetyItems.Length == 0)
				return;
			SetSafety(0, stopped, stopped ? "Server is stopped" : "Stop the server before changes");
			SetSafety(1, _detection?.FrameworkDetected == true,
				_detection?.FrameworkDetected == true ? "Framework detected" : "Framework setup required");
			SetSafety(2, Directory.Exists(_server.InstallPath),
				Directory.Exists(_server.InstallPath) ? "Server folder available" : "Server folder missing");
			SetSafety(3, target?.CanManageIds != true, target?.CanManageIds == true
				? "Provider download needs manual trust"
				: "Security scan runs before install");
			SetSafety(4, standardUser, standardUser
				? "Standard Windows permissions"
				: "Restart without administrator access");
			SetSafety(5, profile?.RestartRequired != true,
				profile?.RestartRequired == true ? "Restart required after changes" : "Framework supports live reload");
		}

		private void SetSafety(int index, bool passed, string text)
		{
			_safetyItems[index].Text = $"{(passed ? "✓" : "!")}  {text}";
			_safetyItems[index].ForeColor = passed ? SettingsPalette.Success : SettingsPalette.Warning;
		}

		private void SelectionChanged()
		{
			ModInventoryItem? item = SelectedItem();
			_selectionDetails.Text = item == null
				? "Select an add-on to see where it was found."
				: $"{item.Name}{Environment.NewLine}{item.SecurityStatus}";
			UpdateButtonsAndSafety();
		}

		private async void InstallFile_Click(object? sender, EventArgs eventArgs)
		{
			if (_profileBox.SelectedItem is not ModSystemProfile profile ||
				_targetBox.SelectedItem is not ModInstallTarget target)
				return;
			if (target.CanManageIds)
			{
				ManageProviderIds(target);
				return;
			}

			string extensions = string.Join(';', target.AllowedExtensions.Select(extension => $"*{extension}"));
			string filter = target.ArchiveOnly
				? "Complete add-on package (*.zip)|*.zip|All files (*.*)|*.*"
				: target.AllowArchives
				? $"Supported add-ons ({extensions};*.zip)|{extensions};*.zip|All files (*.*)|*.*"
				: $"Supported add-ons ({extensions})|{extensions}|All files (*.*)|*.*";
			using OpenFileDialog picker = new()
			{
				Title = $"Choose a file for {target.DisplayName}",
				Filter = filter,
				CheckFileExists = true,
				Multiselect = false
			};
			if (picker.ShowDialog(this) != DialogResult.OK)
				return;

			try
			{
				UseWaitCursor = true;
				_installFile.Enabled = false;
				_inventorySummary.Text = "Running package structure, SHA-256, and antivirus checks…";
				ModSecurityReview review = await ModSecurityScanner.ReviewPackageAsync(
					picker.FileName,
					target);
				if (review.Outcome == ModSecurityOutcome.Blocked)
				{
					_inventorySummary.Text = "Security review blocked the package. No files were changed.";
					_inventorySummary.ForeColor = SettingsPalette.Warning;
					MessageBox.Show(
						this,
						review.BuildUserMessage(),
						"Add-on security review blocked",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);
					return;
				}

				DialogResult confirmation = MessageBox.Show(
					this,
					review.BuildUserMessage() +
					$"\n\nInstall {Path.GetFileName(picker.FileName)} into {target.DisplayName}?",
					"Add-on security review",
					MessageBoxButtons.OKCancel,
					review.Outcome == ModSecurityOutcome.Passed
						? MessageBoxIcon.Information
						: MessageBoxIcon.Warning);
				if (confirmation != DialogResult.OK)
				{
					_inventorySummary.Text = "Installation canceled. No files were changed.";
					return;
				}

				ModImportResult result = ModPackageManager.Import(
					_server,
					profile,
					target,
					picker.FileName,
					review.PackageSha256,
					review.AntivirusStatus);
				MainGUI.Instance?.AppendLog(
					$"[ADD-ONS] Installed {result.DisplayName} ({result.InstalledFileCount} file(s)) for {_server.ServerName}.",
					Color.LimeGreen);
				RefreshInventory();
				MessageBox.Show(
					this,
					result.RestartRequired
						? "The add-on was installed and verified. Start the server when you are ready."
						: "The add-on was installed and verified. This framework can usually reload it without a full restart.",
					"Installation complete",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
			catch (Exception exception)
			{
				_inventorySummary.Text = "The add-on was not installed.";
				_inventorySummary.ForeColor = SettingsPalette.Warning;
				PlainEnglishErrorDialog.ShowError(this, "install the selected add-on", exception.Message);
			}
			finally
			{
				UseWaitCursor = false;
				UpdateButtonsAndSafety();
			}
		}

		private void RemoveSelected_Click(object? sender, EventArgs eventArgs)
		{
			ModInventoryItem? item = SelectedItem();
			if (item?.InstallationId == null || !item.CanRemove)
				return;
			if (MessageBox.Show(
				this,
				$"Remove {item.Name}?\n\nSynix will restore the file that existed before this installation, when available.",
				"Remove add-on",
				MessageBoxButtons.OKCancel,
				MessageBoxIcon.Warning) != DialogResult.OK)
			{
				return;
			}

			try
			{
				if (_targetBox.SelectedItem is ModInstallTarget target && target.CanManageIds)
				{
					IReadOnlyList<string> current = ModPackageManager.GetProviderIds(_server, target);
					ProviderIdConfigurationChange change = ModPackageManager.ConfigureProviderIds(
						_server,
						target,
						current.Where(id => !id.Equals(item.Name, StringComparison.Ordinal)));
					if (!FileHandler.SaveServers())
					{
						change.Rollback();
						throw new IOException("Synix could not save the updated provider mod ID list.");
					}
					MainGUI.Instance?.AppendLog(
						$"[ADD-ONS] Removed provider mod ID {item.Name} from {_server.ServerName}.",
						Color.LimeGreen);
					RefreshInventory();
					return;
				}

				string removed = ModPackageManager.Remove(_server, item.InstallationId);
				MainGUI.Instance?.AppendLog(
					$"[ADD-ONS] Removed {removed} from {_server.ServerName} using its Synix rollback record.",
					Color.LimeGreen);
				RefreshInventory();
			}
			catch (Exception exception)
			{
				PlainEnglishErrorDialog.ShowError(this, "remove the selected add-on", exception.Message);
			}
		}

		private void ManageProviderIds(ModInstallTarget target)
		{
			IReadOnlyList<string> current = ModPackageManager.GetProviderIds(_server, target);
			using ProviderModIdEditor dialog = new(
				string.IsNullOrWhiteSpace(target.ProviderName) ? "game provider" : target.ProviderName,
				target.MaximumIds,
				current);
			if (dialog.ShowDialog(this) != DialogResult.OK)
				return;
			if (MessageBox.Show(
				this,
				$"{target.ProviderName} will download and run the mods represented by these IDs. " +
				"Synix cannot scan provider content before it is downloaded.\n\n" +
				"Continue only if every ID came from a source you trust.",
				"Provider mod security warning",
				MessageBoxButtons.OKCancel,
				MessageBoxIcon.Warning) != DialogResult.OK)
			{
				return;
			}

			ProviderIdConfigurationChange? change = null;
			bool saved = false;
			try
			{
				change = ModPackageManager.ConfigureProviderIds(_server, target, dialog.ModIds);
				if (!FileHandler.SaveServers())
				{
					change.Rollback();
					throw new IOException("Synix could not save the provider mod ID list.");
				}
				saved = true;
				MainGUI.Instance?.AppendLog(
					$"[ADD-ONS] Saved {dialog.ModIds.Count} ordered {target.ProviderName} mod ID(s) for {_server.ServerName}.",
					Color.LimeGreen);
				RefreshInventory();
				MessageBox.Show(
					this,
					"The ordered mod ID list is ready. The game will download or update provider-owned content when the server starts.",
					"Mod IDs saved",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
			catch (Exception exception)
			{
				if (!saved)
					change?.Rollback();
				PlainEnglishErrorDialog.ShowError(this, "save the provider mod ID list", exception.Message);
			}
		}

		private async Task InstallFrameworkAsync()
		{
			ModSystemProfile? profile = _profileBox.SelectedItem as ModSystemProfile;
			if (profile?.Id.Equals("rust-umod", StringComparison.OrdinalIgnoreCase) != true)
				return;
			GameInfo? definition = GameDatabase.GetGame(_server.Game);
			if (definition == null)
				return;

			string previousFramework = _server.ServerFramework;
			string previousVersion = _server.ServerFrameworkVersion;
			try
			{
				_installFramework.Enabled = false;
				UseWaitCursor = true;
				_server.ServerFramework = OxideRuntimeManager.FrameworkName;
				string version = await OxideRuntimeManager.InstallOrUpdateAsync(
					_server,
					definition,
					(message, color) => MainGUI.Instance?.AppendLog(message, color));
				_server.ServerFrameworkVersion = version;
				FileHandler.SaveServers();
				RefreshInventory();
				MessageBox.Show(
					this,
					$"Oxide/uMod {version} is ready. Synix did not install any plugins.",
					"Framework installed",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
			catch (Exception exception)
			{
				_server.ServerFramework = previousFramework;
				_server.ServerFrameworkVersion = previousVersion;
				FileHandler.SaveServers();
				PlainEnglishErrorDialog.ShowError(this, "install the Rust plugin framework", exception.Message);
			}
			finally
			{
				UseWaitCursor = false;
				UpdateButtonsAndSafety();
			}
		}

		private void BrowseCatalog()
		{
			if (_profileBox.SelectedItem is not ModSystemProfile profile ||
				!IsSafeCatalogUri(profile.CatalogUrl, out Uri? uri))
				return;
			try
			{
				Process.Start(new ProcessStartInfo(uri!.AbsoluteUri) { UseShellExecute = true });
			}
			catch (Exception exception)
			{
				PlainEnglishErrorDialog.ShowError(this, "open the add-on catalog", exception.Message);
			}
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
				PlainEnglishErrorDialog.ShowError(this, "open the add-on folder", exception.Message);
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
			IReadOnlyList<string> currentIds)
		{
			_maximumIds = maximumIds;
			Text = "Manage Provider Mod IDs";
			StartPosition = FormStartPosition.CenterParent;
			ShowInTaskbar = false;
			MinimizeBox = false;
			MaximizeBox = false;
			FormBorderStyle = FormBorderStyle.FixedDialog;
			ClientSize = new Size(650, 430);
			BackColor = SettingsPalette.Window;
			ForeColor = SettingsPalette.PrimaryText;
			Font = new Font("Segoe UI", 9.5F);

			Controls.Add(new Label
			{
				Text = $"Manage {providerName} mod IDs",
				Location = new Point(28, 22),
				Size = new Size(594, 42),
				Font = new Font("Segoe UI", 18F, FontStyle.Bold),
				ForeColor = SettingsPalette.PrimaryText
			});
			Controls.Add(new Label
			{
				Text = "Enter IDs in the order they should load. Use commas, spaces, or one ID per line. Synix does not need a database of mod names.",
				Location = new Point(30, 68),
				Size = new Size(580, 48),
				ForeColor = SettingsPalette.SecondaryText
			});
			_ids = new TextBox
			{
				Location = new Point(28, 128),
				Size = new Size(594, 184),
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
			Controls.Add(_ids);
			_status = new Label
			{
				Location = new Point(30, 322),
				Size = new Size(400, 48),
				ForeColor = SettingsPalette.SecondaryText
			};
			Controls.Add(_status);

			ModernSettingsButton cancel = new()
			{
				Text = "Cancel",
				Location = new Point(328, 370),
				Size = new Size(138, 42),
				DialogResult = DialogResult.Cancel
			};
			ModernSettingsButton save = new()
			{
				Text = "Save Ordered IDs",
				Location = new Point(478, 370),
				Size = new Size(144, 42),
				UseAccentStyle = true
			};
			save.Click += (_, _) => Save();
			Controls.AddRange([cancel, save]);
			CancelButton = cancel;
			ValidateIds();
			ThemeManager.Apply(this);
		}

		private void ValidateIds()
		{
			try
			{
				ModIds = ModPackageManager.NormalizeProviderIds(_ids.Text, _maximumIds);
				_status.Text = $"{ModIds.Count} unique numeric mod ID(s) • order is preserved";
				_status.ForeColor = SettingsPalette.Success;
			}
			catch (Exception exception)
			{
				ModIds = [];
				_status.Text = exception.Message;
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
				_status.Text = exception.Message;
				_status.ForeColor = SettingsPalette.Warning;
			}
		}
	}
}
