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
using System.ComponentModel;

namespace Synix_Control_Panel.SynixEngine
{
	internal sealed partial class GameVerificationQueue : Form
	{
		private const string NeedsWorkFilter = "Needs work";
		private const string UnknownConfigurationFilter = "Unknown configuration";
		private const string PartiallyVerifiedFilter = "Partially verified";
		private const string FullyVerifiedFilter = "Fully verified";
		private const string AllGamesFilter = "All games";

		private IReadOnlyList<GameVerificationQueueItem> _items = [];

		public GameVerificationQueue()
		{
			InitializeComponent();
			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
				return;

			ThemeManager.Apply(this);
			_filterCombo.Items.AddRange(
			[
				NeedsWorkFilter,
				UnknownConfigurationFilter,
				PartiallyVerifiedFilter,
				FullyVerifiedFilter,
				AllGamesFilter
			]);
			_filterCombo.SelectedItem = NeedsWorkFilter;
			_stepCombo.DataSource = Enum.GetValues<GameVerificationKind>()
				.Select(kind => new VerificationStepOption(kind))
				.ToArray();
			_stepCombo.DisplayMember = nameof(VerificationStepOption.DisplayName);
			RefreshQueue();
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
			string filter = _filterCombo.SelectedItem?.ToString() ?? NeedsWorkFilter;
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

			_markButton.Enabled = hasSelection && configurationAllowed;
			_clearButton.Enabled = hasSelection &&
				GetEvidence(item?.Verification, SelectedStep) != null;
			_selectedLabel.Text = item == null
				? "Select a game to update its verification evidence."
				: $"Selected: {item.Game}";
		}

		private void MarkButton_Click(object? sender, EventArgs eventArgs)
		{
			GameVerificationQueueItem? item = SelectedItem;
			if (item == null)
				return;

			if (SelectedStep == GameVerificationKind.Configuration &&
				!item.HasKnownConfigurationBehavior)
			{
				MessageBox.Show(
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
				MessageBox.Show(
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

			DialogResult confirmation = MessageBox.Show(
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
				GameVerificationKind.Install => "Install",
				GameVerificationKind.Start => "Start",
				GameVerificationKind.Stop => "Stop",
				GameVerificationKind.Monitoring => "Monitoring",
				GameVerificationKind.Arguments => "Arguments",
				GameVerificationKind.Configuration => "Configuration",
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
