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
			LocalizationManager.BindText(
				_summaryLabel,
				"GameDefinitions.Queue.Summary",
				_items.Count,
				fullyVerified,
				_items.Count - fullyVerified,
				unknownConfiguration);
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

			LocalizationManager.BindText(
				_visibleLabel,
				"GameDefinitions.Queue.Showing",
				_grid.Rows.Count);
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
			LocalizationManager.BindText(
				_markButton,
				SelectedStep switch
				{
					GameVerificationKind.Arguments => "DynamicText.5C906B4070E09E4F5EE9",
					GameVerificationKind.Configuration => "DynamicText.6216460DC46050B514AA",
					_ => "DynamicText.5C49F319B870768820E5"
				});
			LocalizationManager.BindText(
				_selectedLabel,
				item == null
					? "Text.FFF9C013BFE465B863CB"
					: SelectedStep is GameVerificationKind.Install or
						GameVerificationKind.Start or
						GameVerificationKind.Stop or
						GameVerificationKind.Monitoring
						? "GameDefinitions.Queue.SelectedAutomatic"
						: "GameDefinitions.Queue.Selected",
				item?.Game ?? string.Empty);
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
					LocalizationManager.BindText(
						_statusLabel,
						"GameDefinitions.Queue.ArgumentRecorded",
						item.Game);
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
					LocalizationManager.Get("MessageText.FD1FB8A609DE5750E995"),
					LocalizationManager.Get("MessageText.CFACB27EC40395E98A71"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			if (SelectedStep == GameVerificationKind.Configuration &&
				!item.HasKnownConfigurationBehavior)
			{
				LocalizedMessageBox.Show(
					this,
					LocalizationManager.Get("MessageText.9E4EAF41685A751D0C5D"),
					LocalizationManager.Get("MessageText.A2605A701380C408F3A9"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			if (SelectedStep == GameVerificationKind.Configuration &&
				!item.ConfigurationApplicable)
			{
				LocalizedMessageBox.Show(
					this,
					LocalizationManager.Get("MessageText.F94014E8EAE83ED1AC43"),
					LocalizationManager.Get("MessageText.EB44D36BC8B5BCCD5CFA"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			bool recorded = Core.RecordGameVerification(item.Game, SelectedStep);
			LocalizationManager.BindText(
				_statusLabel,
				recorded
					? "GameDefinitions.Queue.StepRecorded"
					: "GameDefinitions.Queue.StepAlreadyRecorded",
				item.Game,
				FormatStepName(SelectedStep),
				Core.GetCurrentVersion().ToString(3));
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
				LocalizationManager.Get(
					"GameDefinitions.Queue.ClearConfirm",
					FormatStepName(SelectedStep),
					item.Game),
				LocalizationManager.Get("MessageText.EEBC6BCDF6B34F44F7A3"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning,
				MessageBoxDefaultButton.Button2);
			if (confirmation != DialogResult.Yes)
				return;

			bool cleared = Core.ClearGameVerification(item.Game, SelectedStep);
			LocalizationManager.BindText(
				_statusLabel,
				cleared
					? "GameDefinitions.Queue.StepCleared"
					: "DynamicText.019DEC65766E8437A2BC",
				item.Game,
				FormatStepName(SelectedStep));
			_statusLabel.ForeColor = cleared
				? SettingsPalette.Warning
				: SettingsPalette.SecondaryText;
			RefreshQueueAndRestoreSelection(item.Game);
		}

		private void RefreshButton_Click(object? sender, EventArgs eventArgs)
		{
			string? selectedGame = SelectedItem?.Game;
			RefreshQueueAndRestoreSelection(selectedGame);
			LocalizationManager.BindText(
				_statusLabel,
				"Text.229193F097F563A43177");
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
						LocalizationManager.Get(
							"GameDefinitions.Builder.ProjectNotFound"));
				}

				GameVerificationProjectExportResult result =
					Core.ExportGameVerificationToProject(projectDirectory);
				LocalizationManager.BindText(
					_statusLabel,
					"GameDefinitions.Queue.Exported",
					result.EvidenceCount,
					result.GameCount);
				_statusLabel.ForeColor = SettingsPalette.Success;
				LocalizedMessageBox.Show(
					this,
					LocalizationManager.Get(
						"GameDefinitions.Queue.ExportBody",
						result.FilePath),
					LocalizationManager.Get("MessageText.928E0E14F24067F7A176"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
			catch (Exception exception) when (exception is IOException or
				UnauthorizedAccessException or
				DirectoryNotFoundException or
				InvalidOperationException or
				NotSupportedException)
			{
				_statusLabel.Text = LocalizationManager.TranslateRuntimeText(
					exception.Message);
				_statusLabel.ForeColor = SettingsPalette.Danger;
				LocalizedMessageBox.Show(
					this,
					LocalizationManager.TranslateRuntimeText(exception.Message),
					LocalizationManager.Get("MessageText.B32413A80382548BEC79"),
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

			if (value == LocalizationManager.Get("GameDefinitions.Queue.Evidence.Verified") ||
				value == LocalizationManager.Get("Text.1B3F2F7A383F81F4A567"))
				style.ForeColor = SettingsPalette.Success;
			else if (value == LocalizationManager.Get("GameDefinitions.Queue.Evidence.NeedsTest") ||
				value == LocalizationManager.Get("GameDefinitions.Queue.Evidence.DefinitionNeeded"))
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
			return LocalizationManager.Get(
				evidence == null
					? "GameDefinitions.Queue.Evidence.NeedsTest"
					: "GameDefinitions.Queue.Evidence.Verified");
		}

		private static string FormatConfigurationEvidence(
			GameVerificationQueueItem item)
		{
			if (!item.ConfigurationApplicable)
				return LocalizationManager.Get("Text.1B3F2F7A383F81F4A567");
			if (!item.HasKnownConfigurationBehavior)
				return LocalizationManager.Get("GameDefinitions.Queue.Evidence.DefinitionNeeded");
			return FormatEvidence(item.Verification.Configuration);
		}

		private static string FormatConfigurationMode(ConfigFileCreationMode mode)
		{
			return mode switch
			{
				ConfigFileCreationMode.GameGenerated => LocalizationManager.Get("GameDefinitions.Queue.Mode.GameGenerated"),
				ConfigFileCreationMode.SynixTemplate => LocalizationManager.Get("GameDefinitions.Queue.Mode.SynixTemplate"),
				ConfigFileCreationMode.LaunchArgumentsOnly => LocalizationManager.Get("GameDefinitions.Queue.Mode.ArgumentsOnly"),
				_ => LocalizationManager.Get("Status.Unknown")
			};
		}

		private static string FormatLastTested(GameVerificationEvidence? evidence)
		{
			return evidence == null
				? LocalizationManager.Get("GameDefinitions.Queue.Never")
				: LocalizationManager.Get(
					"GameDefinitions.Queue.LastTested",
					evidence.SynixVersion,
					evidence.VerifiedAtUtc.ToLocalTime().ToString(
						"g",
						System.Globalization.CultureInfo.CurrentUICulture));
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
