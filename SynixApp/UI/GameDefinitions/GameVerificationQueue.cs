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
using Synix_Control_Panel.SynixApp.Localization;
using System.ComponentModel;

namespace Synix_Control_Panel.SynixApp.UI.GameDefinitions
{
	internal sealed partial class GameVerificationQueue : Form
	{
		private const string NeedsWorkFilter = "needs-work";
		private const string UnknownConfigurationFilter = "unknown-configuration";
		private const string PartiallyVerifiedFilter = "partially-verified";
		private const string FullyVerifiedFilter = "fully-verified";
		private const string AllGamesFilter = "all-games";

		private IReadOnlyList<GameVerificationQueueItem> _items = [];

		public GameVerificationQueue()
		{
			InitializeComponent();
			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
				return;

			ThemeManager.Apply(this);
			PopulateLocalizedOptions();
			LocalizationManager.LanguageChanged += InterfaceLanguageChanged;
			Disposed += (_, _) =>
				LocalizationManager.LanguageChanged -= InterfaceLanguageChanged;
			_exportButton.Visible = !Core.IsOfficialRelease;
			RefreshQueue();
		}

		private void InterfaceLanguageChanged(
			object? sender,
			EventArgs eventArgs)
		{
			PopulateLocalizedOptions();
			RefreshQueue();
		}

		private void PopulateLocalizedOptions()
		{
			string selectedFilter =
				(_filterCombo.SelectedItem as LocalizedOption)?.Value
				?? NeedsWorkFilter;
			GameVerificationKind selectedStep = SelectedStep;

			_filterCombo.Items.Clear();
			_filterCombo.Items.AddRange(
			[
				new LocalizedOption(
					NeedsWorkFilter,
					"Option.VerificationFilter.NeedsWork"),
				new LocalizedOption(
					UnknownConfigurationFilter,
					"Option.VerificationFilter.UnknownConfiguration"),
				new LocalizedOption(
					PartiallyVerifiedFilter,
					"Option.VerificationFilter.PartiallyVerified"),
				new LocalizedOption(
					FullyVerifiedFilter,
					"Option.VerificationFilter.FullyVerified"),
				new LocalizedOption(
					AllGamesFilter,
					"Option.VerificationFilter.AllGames")
			]);
			_filterCombo.SelectedItem = _filterCombo.Items
				.Cast<LocalizedOption>()
				.First(option => option.Value == selectedFilter);

			_stepCombo.DataSource = Enum.GetValues<GameVerificationKind>()
				.Select(kind => new VerificationStepOption(kind))
				.ToArray();
			_stepCombo.DisplayMember = nameof(VerificationStepOption.DisplayName);
			_stepCombo.SelectedItem = (_stepCombo.DataSource as
				IEnumerable<VerificationStepOption>)?.FirstOrDefault(
					option => option.Kind == selectedStep);
		}

		private GameVerificationQueueItem? SelectedItem =>
			_grid.SelectedRows.Count == 1
				? _grid.SelectedRows[0].Tag as GameVerificationQueueItem
				: null;

		private GameVerificationKind SelectedStep =>
			_stepCombo.SelectedItem is VerificationStepOption option
				? option.Kind
				: GameVerificationKind.Install;

		private void RefreshQueue()
		{
			_items = Core.GetGameVerificationQueue();
			ApplyFilter();

			int fullyVerified = _items.Count(item => item.IsFullyVerified);
			int unknownConfiguration = _items.Count(item =>
				!item.HasKnownConfigurationBehavior);
			_summaryLabel.Text =
				$"{_items.Count} games  •  {fullyVerified} complete  •  " +
				$"{_items.Count - fullyVerified} need work  •  " +
				$"{unknownConfiguration} need configuration research";
		}

		private void ApplyFilter()
		{
			string search = _searchBox.Text.Trim();
			string filter =
				(_filterCombo.SelectedItem as LocalizedOption)?.Value
				?? NeedsWorkFilter;
			IEnumerable<GameVerificationQueueItem> visibleItems = _items.Where(item =>
				search.Length == 0 ||
				item.Game.Contains(search, StringComparison.OrdinalIgnoreCase));

			visibleItems = filter switch
			{
				UnknownConfigurationFilter => visibleItems.Where(item =>
					!item.HasKnownConfigurationBehavior),
				PartiallyVerifiedFilter => visibleItems.Where(item =>
					item.CompletedSteps > 0 && !item.IsFullyVerified),
				FullyVerifiedFilter => visibleItems.Where(item => item.IsFullyVerified),
				AllGamesFilter => visibleItems,
				_ => visibleItems.Where(item => !item.IsFullyVerified)
			};

			_grid.Rows.Clear();
			foreach (GameVerificationQueueItem item in visibleItems)
			{
				GameCompatibilityVerification verification = item.Verification;
				int rowIndex = _grid.Rows.Add(
					item.Game,
					$"{item.CompletedSteps}/{item.RequiredSteps}",
					FormatConfigurationMode(item.ConfigurationMode),
					FormatEvidence(verification.Install),
					FormatEvidence(verification.Start),
					FormatEvidence(verification.Stop),
					FormatEvidence(verification.Monitoring),
					FormatEvidence(verification.Arguments),
					FormatConfigurationEvidence(item),
					FormatLastTested(verification.LastTested));
				_grid.Rows[rowIndex].Tag = item;
			}

			_visibleLabel.Text = $"Showing {_grid.Rows.Count} game(s)";
			if (_grid.Rows.Count > 0)
			{
				_grid.Rows[0].Selected = true;
			}
			UpdateActionState();
		}

		private void SearchBox_TextChanged(object? sender, EventArgs eventArgs)
		{
			ApplyFilter();
		}

		private void FilterCombo_SelectedIndexChanged(
			object? sender,
			EventArgs eventArgs)
		{
			ApplyFilter();
		}

		private void Grid_SelectionChanged(object? sender, EventArgs eventArgs)
		{
			UpdateActionState();
		}

		private void StepCombo_SelectedIndexChanged(
			object? sender,
			EventArgs eventArgs)
		{
			UpdateActionState();
		}

		private void UpdateActionState()
		{
			GameVerificationQueueItem? item = SelectedItem;
			bool hasSelection = item != null;
			bool configurationAllowed = SelectedStep != GameVerificationKind.Configuration ||
				item is { ConfigurationApplicable: true, HasKnownConfigurationBehavior: true };
			bool manualActionAvailable = SelectedStep is
				GameVerificationKind.Arguments or
				GameVerificationKind.Configuration;

			_markButton.Enabled = hasSelection && configurationAllowed && manualActionAvailable;
			_clearButton.Enabled = hasSelection &&
				GetEvidence(item?.Verification, SelectedStep) != null;
			_markButton.Text = SelectedStep switch
			{
				GameVerificationKind.Arguments => "Test Arguments",
				GameVerificationKind.Configuration => "Mark Configuration",
				_ => "Recorded Automatically"
			};
			_selectedLabel.Text = item == null
				? "Select a game to update its verification evidence."
				: SelectedStep is GameVerificationKind.Install or
					GameVerificationKind.Start or
					GameVerificationKind.Stop or
					GameVerificationKind.Monitoring
					? $"Selected: {item.Game} • This step is recorded by the real server workflow."
					: $"Selected: {item.Game}";
		}

		private void MarkButton_Click(object? sender, EventArgs eventArgs)
		{
			GameVerificationQueueItem? item = SelectedItem;
			if (item == null)
				return;

			if (SelectedStep == GameVerificationKind.Arguments)
			{
				using ArgumentVerificationDialog dialog = new(item.Game);
				dialog.ShowDialog(this);
				if (dialog.VerificationRecorded)
				{
					_statusLabel.Text =
						$"{item.Game}: argument verification recorded from the real-server test.";
					_statusLabel.ForeColor = SettingsPalette.Success;
				}
				RefreshQueueAndRestoreSelection(item.Game);
				return;
			}

			if (SelectedStep is GameVerificationKind.Install or
				GameVerificationKind.Start or
				GameVerificationKind.Stop or
				GameVerificationKind.Monitoring)
			{
				LocalizedMessageBox.Show(
					this,
					"Install, Start, Stop, and Monitoring evidence is recorded automatically by the real Synix server workflow and cannot be marked manually.",
					"Automatic Verification",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			if (SelectedStep == GameVerificationKind.Configuration &&
				!item.HasKnownConfigurationBehavior)
			{
				LocalizedMessageBox.Show(
					this,
					"Set the game's configuration behavior and add any required template information before marking its configuration as verified.",
					"Configuration Definition Required",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			if (SelectedStep == GameVerificationKind.Configuration &&
				!item.ConfigurationApplicable)
			{
				LocalizedMessageBox.Show(
					this,
					"This game is managed entirely through launch arguments, so a separate configuration-file test is not required.",
					"Configuration Test Not Required",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			bool recorded = Core.RecordGameVerification(item.Game, SelectedStep);
			_statusLabel.Text = recorded
				? $"{item.Game}: {FormatStepName(SelectedStep)} verified for Synix v{Core.GetCurrentVersion().ToString(3)}."
				: $"{item.Game}: that step is already verified for this Synix version or a newer version.";
			_statusLabel.ForeColor = recorded
				? SettingsPalette.Success
				: SettingsPalette.SecondaryText;
			RefreshQueueAndRestoreSelection(item.Game);
		}

		private void ClearButton_Click(object? sender, EventArgs eventArgs)
		{
			GameVerificationQueueItem? item = SelectedItem;
			if (item == null)
				return;

			DialogResult confirmation = LocalizedMessageBox.Show(
				this,
				$"Remove the {FormatStepName(SelectedStep).ToLowerInvariant()} verification from {item.Game}?",
				"Clear Verification Evidence",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning,
				MessageBoxDefaultButton.Button2);
			if (confirmation != DialogResult.Yes)
				return;

			bool cleared = Core.ClearGameVerification(item.Game, SelectedStep);
			_statusLabel.Text = cleared
				? $"{item.Game}: {FormatStepName(SelectedStep)} verification cleared."
				: "No saved verification was found for that step.";
			_statusLabel.ForeColor = cleared
				? SettingsPalette.Warning
				: SettingsPalette.SecondaryText;
			RefreshQueueAndRestoreSelection(item.Game);
		}

		private void RefreshButton_Click(object? sender, EventArgs eventArgs)
		{
			string? selectedGame = SelectedItem?.Game;
			RefreshQueueAndRestoreSelection(selectedGame);
			_statusLabel.Text = "Verification queue refreshed from the saved Synix evidence.";
			_statusLabel.ForeColor = SettingsPalette.SecondaryText;
		}

		private void ExportButton_Click(object? sender, EventArgs eventArgs)
		{
			if (Core.IsOfficialRelease)
				return;

			try
			{
				string? projectDirectory =
					Core.FindProjectDirectory(AppContext.BaseDirectory) ??
					Core.FindProjectDirectory(Environment.CurrentDirectory);
				if (projectDirectory == null)
				{
					throw new DirectoryNotFoundException(
						"Synix Control Panel.csproj could not be found from this development build.");
				}

				GameVerificationProjectExportResult result =
					Core.ExportGameVerificationToProject(projectDirectory);
				_statusLabel.Text =
					$"Exported {result.EvidenceCount} verification checks for {result.GameCount} games into the project.";
				_statusLabel.ForeColor = SettingsPalette.Success;
				LocalizedMessageBox.Show(
					this,
					$"Saved project verification evidence to:\n\n{result.FilePath}\n\nRebuild Synix and run the built-in definition tests before releasing.",
					"Verification Exported",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
			catch (Exception exception) when (exception is IOException or
				UnauthorizedAccessException or
				DirectoryNotFoundException or
				InvalidOperationException or
				NotSupportedException)
			{
				_statusLabel.Text = exception.Message;
				_statusLabel.ForeColor = SettingsPalette.Danger;
				LocalizedMessageBox.Show(
					this,
					exception.Message,
					"Verification Export Failed",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
		}

		private void RefreshQueueAndRestoreSelection(string? selectedGame)
		{
			RefreshQueue();
			if (string.IsNullOrWhiteSpace(selectedGame))
				return;

			foreach (DataGridViewRow row in _grid.Rows)
			{
				if (row.Tag is GameVerificationQueueItem item &&
					string.Equals(
						item.Game,
						selectedGame,
						StringComparison.OrdinalIgnoreCase))
				{
					row.Selected = true;
					_grid.CurrentCell = row.Cells[0];
					break;
				}
			}
		}

		private void Grid_CellFormatting(
			object? sender,
			DataGridViewCellFormattingEventArgs eventArgs)
		{
			if (eventArgs.RowIndex < 0 || eventArgs.Value is not string value)
				return;
			DataGridViewCellStyle? style = eventArgs.CellStyle;
			if (style == null)
				return;

			if (value == "Verified" || value == "Not required")
				style.ForeColor = SettingsPalette.Success;
			else if (value is "Needs test" or "Definition needed")
				style.ForeColor = SettingsPalette.Warning;
			else if (eventArgs.ColumnIndex == _progressColumn.Index)
			{
				GameVerificationQueueItem? item =
					_grid.Rows[eventArgs.RowIndex].Tag as GameVerificationQueueItem;
				style.ForeColor = item?.IsFullyVerified == true
					? SettingsPalette.Success
					: SettingsPalette.Warning;
			}
		}

		private static string FormatEvidence(GameVerificationEvidence? evidence)
		{
			return evidence == null ? "Needs test" : "Verified";
		}

		private static string FormatConfigurationEvidence(
			GameVerificationQueueItem item)
		{
			if (!item.ConfigurationApplicable)
				return "Not required";
			if (!item.HasKnownConfigurationBehavior)
				return "Definition needed";
			return FormatEvidence(item.Verification.Configuration);
		}

		private static string FormatConfigurationMode(ConfigFileCreationMode mode)
		{
			return mode switch
			{
				ConfigFileCreationMode.GameGenerated => "Game generated",
				ConfigFileCreationMode.SynixTemplate => "Synix template",
				ConfigFileCreationMode.LaunchArgumentsOnly => "Arguments only",
				_ => "Unknown"
			};
		}

		private static string FormatLastTested(GameVerificationEvidence? evidence)
		{
			return evidence == null
				? "Never"
				: $"v{evidence.SynixVersion}  •  {evidence.VerifiedAtUtc.ToLocalTime():g}";
		}

		private static string FormatStepName(GameVerificationKind kind)
		{
			return kind switch
			{
				GameVerificationKind.Install =>
					LocalizationManager.Get("VerificationStep.Install"),
				GameVerificationKind.Start =>
					LocalizationManager.Get("VerificationStep.Start"),
				GameVerificationKind.Stop =>
					LocalizationManager.Get("VerificationStep.Stop"),
				GameVerificationKind.Monitoring =>
					LocalizationManager.Get("VerificationStep.Monitoring"),
				GameVerificationKind.Arguments =>
					LocalizationManager.Get("VerificationStep.Arguments"),
				GameVerificationKind.Configuration =>
					LocalizationManager.Get("VerificationStep.Configuration"),
				_ => kind.ToString()
			};
		}

		private static GameVerificationEvidence? GetEvidence(
			GameCompatibilityVerification? verification,
			GameVerificationKind kind)
		{
			if (verification == null)
				return null;

			return kind switch
			{
				GameVerificationKind.Install => verification.Install,
				GameVerificationKind.Start => verification.Start,
				GameVerificationKind.Stop => verification.Stop,
				GameVerificationKind.Monitoring => verification.Monitoring,
				GameVerificationKind.Arguments => verification.Arguments,
				GameVerificationKind.Configuration => verification.Configuration,
				_ => null
			};
		}

		private sealed record VerificationStepOption(GameVerificationKind Kind)
		{
			public string DisplayName => FormatStepName(Kind);
		}
	}
}
