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

#nullable enable

namespace Synix_Control_Panel.SynixEngine
{
	partial class GameVerificationQueue
	{
		private System.ComponentModel.IContainer? components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				components?.Dispose();
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			DataGridViewCellStyle headerStyle = new();
			DataGridViewCellStyle cellStyle = new();
			DataGridViewCellStyle alternateStyle = new();
			_titleLabel = new Label();
			_subtitleLabel = new Label();
			_summaryLabel = new Label();
			_searchBox = new TextBox();
			_filterCombo = new ModernSettingsComboBox();
			_visibleLabel = new Label();
			_grid = new DataGridView();
			_gameColumn = new DataGridViewTextBoxColumn();
			_progressColumn = new DataGridViewTextBoxColumn();
			_configModeColumn = new DataGridViewTextBoxColumn();
			_installColumn = new DataGridViewTextBoxColumn();
			_startColumn = new DataGridViewTextBoxColumn();
			_stopColumn = new DataGridViewTextBoxColumn();
			_monitoringColumn = new DataGridViewTextBoxColumn();
			_argumentsColumn = new DataGridViewTextBoxColumn();
			_configurationColumn = new DataGridViewTextBoxColumn();
			_lastTestedColumn = new DataGridViewTextBoxColumn();
			_selectedLabel = new Label();
			_stepLabel = new Label();
			_stepCombo = new ModernSettingsComboBox();
			_markButton = new ModernSettingsButton();
			_clearButton = new ModernSettingsButton();
			_refreshButton = new ModernSettingsButton();
			_closeButton = new ModernSettingsButton();
			_statusLabel = new Label();
			((System.ComponentModel.ISupportInitialize)_grid).BeginInit();
			SuspendLayout();
			_titleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_titleLabel.Font = new Font("Segoe UI", 19F, FontStyle.Bold);
			_titleLabel.ForeColor = SettingsPalette.PrimaryText;
			_titleLabel.Location = new Point(24, 18);
			_titleLabel.Name = "_titleLabel";
			_titleLabel.Size = new Size(1172, 40);
			_titleLabel.TabIndex = 0;
			_titleLabel.Text = "Game Verification Queue";
			_subtitleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_subtitleLabel.ForeColor = SettingsPalette.SecondaryText;
			_subtitleLabel.Location = new Point(26, 61);
			_subtitleLabel.Name = "_subtitleLabel";
			_subtitleLabel.Size = new Size(1170, 44);
			_subtitleLabel.TabIndex = 1;
			_subtitleLabel.Text = "Install, start, stop, and monitoring checks are recorded automatically. After manually checking launch arguments or a configuration file, select that step below and mark it verified.";
			_summaryLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_summaryLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
			_summaryLabel.ForeColor = SettingsPalette.SecondaryText;
			_summaryLabel.Location = new Point(26, 108);
			_summaryLabel.Name = "_summaryLabel";
			_summaryLabel.Size = new Size(1170, 28);
			_summaryLabel.TabIndex = 2;
			_summaryLabel.Text = "Loading the built-in game verification queue...";
			_searchBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			_searchBox.BackColor = SettingsPalette.Input;
			_searchBox.BorderStyle = BorderStyle.FixedSingle;
			_searchBox.Font = new Font("Segoe UI", 10F);
			_searchBox.ForeColor = SettingsPalette.PrimaryText;
			_searchBox.Location = new Point(26, 144);
			_searchBox.Name = "_searchBox";
			_searchBox.PlaceholderText = "Search game name...";
			_searchBox.Size = new Size(360, 25);
			_searchBox.TabIndex = 3;
			_searchBox.TextChanged += SearchBox_TextChanged;
			_filterCombo.DropDownStyle = ComboBoxStyle.DropDownList;
			_filterCombo.Location = new Point(400, 136);
			_filterCombo.Name = "_filterCombo";
			_filterCombo.Size = new Size(224, 42);
			_filterCombo.TabIndex = 4;
			_filterCombo.SelectedIndexChanged += FilterCombo_SelectedIndexChanged;
			_visibleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_visibleLabel.ForeColor = SettingsPalette.SecondaryText;
			_visibleLabel.Location = new Point(944, 144);
			_visibleLabel.Name = "_visibleLabel";
			_visibleLabel.Size = new Size(252, 25);
			_visibleLabel.TabIndex = 5;
			_visibleLabel.Text = "Showing 0 games";
			_visibleLabel.TextAlign = ContentAlignment.MiddleRight;
			_grid.AllowUserToAddRows = false;
			_grid.AllowUserToDeleteRows = false;
			_grid.AllowUserToResizeRows = false;
			alternateStyle.BackColor = SettingsPalette.Card;
			_grid.AlternatingRowsDefaultCellStyle = alternateStyle;
			_grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
			_grid.BackgroundColor = SettingsPalette.Input;
			_grid.BorderStyle = BorderStyle.None;
			_grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
			_grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
			headerStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
			headerStyle.BackColor = SettingsPalette.Card;
			headerStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			headerStyle.ForeColor = SettingsPalette.SecondaryText;
			headerStyle.Padding = new Padding(8, 0, 0, 0);
			headerStyle.SelectionBackColor = SettingsPalette.Card;
			headerStyle.SelectionForeColor = SettingsPalette.SecondaryText;
			headerStyle.WrapMode = DataGridViewTriState.False;
			_grid.ColumnHeadersDefaultCellStyle = headerStyle;
			_grid.ColumnHeadersHeight = 42;
			_grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			_grid.Columns.AddRange(new DataGridViewColumn[] { _gameColumn, _progressColumn, _configModeColumn, _installColumn, _startColumn, _stopColumn, _monitoringColumn, _argumentsColumn, _configurationColumn, _lastTestedColumn });
			cellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
			cellStyle.BackColor = SettingsPalette.Input;
			cellStyle.Font = new Font("Segoe UI", 9.5F);
			cellStyle.ForeColor = SettingsPalette.PrimaryText;
			cellStyle.Padding = new Padding(8, 0, 0, 0);
			cellStyle.SelectionBackColor = SettingsPalette.Selection;
			cellStyle.SelectionForeColor = SettingsPalette.PrimaryText;
			cellStyle.WrapMode = DataGridViewTriState.False;
			_grid.DefaultCellStyle = cellStyle;
			_grid.EnableHeadersVisualStyles = false;
			_grid.Location = new Point(26, 184);
			_grid.MultiSelect = false;
			_grid.Name = "_grid";
			_grid.ReadOnly = true;
			_grid.RowHeadersVisible = false;
			_grid.RowTemplate.Height = 38;
			_grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			_grid.Size = new Size(1170, 408);
			_grid.TabIndex = 6;
			_grid.CellFormatting += Grid_CellFormatting;
			_grid.SelectionChanged += Grid_SelectionChanged;
			_gameColumn.Frozen = true;
			_gameColumn.HeaderText = "GAME";
			_gameColumn.MinimumWidth = 190;
			_gameColumn.Name = "_gameColumn";
			_gameColumn.ReadOnly = true;
			_gameColumn.Width = 220;
			_progressColumn.HeaderText = "PROGRESS";
			_progressColumn.Name = "_progressColumn";
			_progressColumn.ReadOnly = true;
			_progressColumn.Width = 82;
			_configModeColumn.HeaderText = "CONFIG SOURCE";
			_configModeColumn.Name = "_configModeColumn";
			_configModeColumn.ReadOnly = true;
			_configModeColumn.Width = 130;
			_installColumn.HeaderText = "INSTALL";
			_installColumn.Name = "_installColumn";
			_installColumn.ReadOnly = true;
			_installColumn.Width = 88;
			_startColumn.HeaderText = "START";
			_startColumn.Name = "_startColumn";
			_startColumn.ReadOnly = true;
			_startColumn.Width = 88;
			_stopColumn.HeaderText = "STOP";
			_stopColumn.Name = "_stopColumn";
			_stopColumn.ReadOnly = true;
			_stopColumn.Width = 88;
			_monitoringColumn.HeaderText = "MONITOR";
			_monitoringColumn.Name = "_monitoringColumn";
			_monitoringColumn.ReadOnly = true;
			_monitoringColumn.Width = 92;
			_argumentsColumn.HeaderText = "ARGUMENTS";
			_argumentsColumn.Name = "_argumentsColumn";
			_argumentsColumn.ReadOnly = true;
			_argumentsColumn.Width = 96;
			_configurationColumn.HeaderText = "CONFIGURATION";
			_configurationColumn.Name = "_configurationColumn";
			_configurationColumn.ReadOnly = true;
			_configurationColumn.Width = 122;
			_lastTestedColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			_lastTestedColumn.HeaderText = "LAST TESTED";
			_lastTestedColumn.MinimumWidth = 165;
			_lastTestedColumn.Name = "_lastTestedColumn";
			_lastTestedColumn.ReadOnly = true;
			_selectedLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_selectedLabel.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			_selectedLabel.ForeColor = SettingsPalette.PrimaryText;
			_selectedLabel.Location = new Point(26, 604);
			_selectedLabel.Name = "_selectedLabel";
			_selectedLabel.Size = new Size(590, 25);
			_selectedLabel.TabIndex = 7;
			_selectedLabel.Text = "Select a game to update its verification evidence.";
			_stepLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			_stepLabel.ForeColor = SettingsPalette.SecondaryText;
			_stepLabel.Location = new Point(26, 636);
			_stepLabel.Name = "_stepLabel";
			_stepLabel.Size = new Size(112, 42);
			_stepLabel.TabIndex = 8;
			_stepLabel.Text = "Verification step";
			_stepLabel.TextAlign = ContentAlignment.MiddleLeft;
			_stepCombo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			_stepCombo.DropDownStyle = ComboBoxStyle.DropDownList;
			_stepCombo.Location = new Point(140, 636);
			_stepCombo.Name = "_stepCombo";
			_stepCombo.Size = new Size(170, 42);
			_stepCombo.TabIndex = 9;
			_stepCombo.SelectedIndexChanged += StepCombo_SelectedIndexChanged;
			_markButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			_markButton.Location = new Point(322, 636);
			_markButton.Name = "_markButton";
			_markButton.Size = new Size(146, 42);
			_markButton.TabIndex = 10;
			_markButton.Text = "Mark Verified";
			_markButton.UseAccentStyle = true;
			_markButton.Click += MarkButton_Click;
			_clearButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			_clearButton.Location = new Point(478, 636);
			_clearButton.Name = "_clearButton";
			_clearButton.Size = new Size(130, 42);
			_clearButton.TabIndex = 11;
			_clearButton.Text = "Clear Mark";
			_clearButton.Click += ClearButton_Click;
			_refreshButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_refreshButton.Location = new Point(924, 636);
			_refreshButton.Name = "_refreshButton";
			_refreshButton.Size = new Size(126, 42);
			_refreshButton.TabIndex = 12;
			_refreshButton.Text = "Refresh";
			_refreshButton.Click += RefreshButton_Click;
			_closeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_closeButton.DialogResult = DialogResult.Cancel;
			_closeButton.Location = new Point(1062, 636);
			_closeButton.Name = "_closeButton";
			_closeButton.Size = new Size(134, 42);
			_closeButton.TabIndex = 13;
			_closeButton.Text = "Close";
			_statusLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_statusLabel.ForeColor = SettingsPalette.SecondaryText;
			_statusLabel.Location = new Point(26, 688);
			_statusLabel.Name = "_statusLabel";
			_statusLabel.Size = new Size(1170, 26);
			_statusLabel.TabIndex = 14;
			_statusLabel.Text = "Select only checks you have completed successfully on this computer.";
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = SettingsPalette.Window;
			CancelButton = _closeButton;
			ClientSize = new Size(1222, 728);
			Controls.Add(_titleLabel);
			Controls.Add(_subtitleLabel);
			Controls.Add(_summaryLabel);
			Controls.Add(_searchBox);
			Controls.Add(_filterCombo);
			Controls.Add(_visibleLabel);
			Controls.Add(_grid);
			Controls.Add(_selectedLabel);
			Controls.Add(_stepLabel);
			Controls.Add(_stepCombo);
			Controls.Add(_markButton);
			Controls.Add(_clearButton);
			Controls.Add(_refreshButton);
			Controls.Add(_closeButton);
			Controls.Add(_statusLabel);
			Font = new Font("Segoe UI", 10F);
			MinimizeBox = false;
			MinimumSize = new Size(980, 620);
			Name = "GameVerificationQueue";
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Synix Game Verification Queue";
			((System.ComponentModel.ISupportInitialize)_grid).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		private Label _titleLabel = null!;
		private Label _subtitleLabel = null!;
		private Label _summaryLabel = null!;
		private TextBox _searchBox = null!;
		private ModernSettingsComboBox _filterCombo = null!;
		private Label _visibleLabel = null!;
		private DataGridView _grid = null!;
		private DataGridViewTextBoxColumn _gameColumn = null!;
		private DataGridViewTextBoxColumn _progressColumn = null!;
		private DataGridViewTextBoxColumn _configModeColumn = null!;
		private DataGridViewTextBoxColumn _installColumn = null!;
		private DataGridViewTextBoxColumn _startColumn = null!;
		private DataGridViewTextBoxColumn _stopColumn = null!;
		private DataGridViewTextBoxColumn _monitoringColumn = null!;
		private DataGridViewTextBoxColumn _argumentsColumn = null!;
		private DataGridViewTextBoxColumn _configurationColumn = null!;
		private DataGridViewTextBoxColumn _lastTestedColumn = null!;
		private Label _selectedLabel = null!;
		private Label _stepLabel = null!;
		private ModernSettingsComboBox _stepCombo = null!;
		private ModernSettingsButton _markButton = null!;
		private ModernSettingsButton _clearButton = null!;
		private ModernSettingsButton _refreshButton = null!;
		private ModernSettingsButton _closeButton = null!;
		private Label _statusLabel = null!;
	}
}
