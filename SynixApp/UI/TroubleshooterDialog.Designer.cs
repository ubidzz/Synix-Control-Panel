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
	partial class TroubleshooterDialog
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
			titleLabel = new Label();
			subtitleLabel = new Label();
			statusLabel = new Label();
			resultsGrid = new DataGridView();
			resultColumn = new DataGridViewTextBoxColumn();
			areaColumn = new DataGridViewTextBoxColumn();
			subjectColumn = new DataGridViewTextBoxColumn();
			detailsColumn = new DataGridViewTextBoxColumn();
			actionColumn = new DataGridViewTextBoxColumn();
			closeButton = new ModernSettingsButton();
			copyButton = new ModernSettingsButton();
			actionButton = new ModernSettingsButton();
			runButton = new ModernSettingsButton();
			((System.ComponentModel.ISupportInitialize)resultsGrid).BeginInit();
			SuspendLayout();
			titleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			titleLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
			titleLabel.ForeColor = SettingsPalette.PrimaryText;
			titleLabel.Location = new Point(28, 20);
			titleLabel.Size = new Size(1064, 38);
			titleLabel.Text = "Synix Troubleshooter";
			subtitleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			subtitleLabel.Font = new Font("Segoe UI", 9.5F);
			subtitleLabel.ForeColor = SettingsPalette.SecondaryText;
			subtitleLabel.Location = new Point(28, 60);
			subtitleLabel.Size = new Size(1064, 42);
			subtitleLabel.Text = "Check shared runtimes, server files, configurations, ports, Windows Firewall, disk space, interrupted processes, recent logs, and Synix update health from one place.";
			statusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			statusLabel.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
			statusLabel.ForeColor = SettingsPalette.SecondaryText;
			statusLabel.Location = new Point(28, 108);
			statusLabel.Size = new Size(1064, 28);
			statusLabel.Text = "Ready to check this computer.";
			resultsGrid.AllowUserToAddRows = false;
			resultsGrid.AllowUserToDeleteRows = false;
			resultsGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			resultsGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
			resultsGrid.BackgroundColor = SettingsPalette.Input;
			resultsGrid.BorderStyle = BorderStyle.None;
			resultsGrid.ColumnHeadersHeight = 40;
			resultsGrid.Columns.AddRange(resultColumn, areaColumn, subjectColumn, detailsColumn, actionColumn);
			resultsGrid.Location = new Point(28, 142);
			resultsGrid.MultiSelect = false;
			resultsGrid.Name = "resultsGrid";
			resultsGrid.ReadOnly = true;
			resultsGrid.RowHeadersVisible = false;
			resultsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			resultsGrid.Size = new Size(1064, 516);
			resultsGrid.TabIndex = 0;
			resultsGrid.SelectionChanged += ResultsGrid_SelectionChanged;
			resultColumn.HeaderText = "RESULT";
			resultColumn.MinimumWidth = 90;
			resultColumn.Name = "resultColumn";
			resultColumn.ReadOnly = true;
			resultColumn.Width = 90;
			areaColumn.HeaderText = "AREA";
			areaColumn.MinimumWidth = 150;
			areaColumn.Name = "areaColumn";
			areaColumn.ReadOnly = true;
			areaColumn.Width = 170;
			subjectColumn.HeaderText = "SERVER / ITEM";
			subjectColumn.MinimumWidth = 170;
			subjectColumn.Name = "subjectColumn";
			subjectColumn.ReadOnly = true;
			subjectColumn.Width = 190;
			detailsColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			detailsColumn.HeaderText = "DETAILS";
			detailsColumn.MinimumWidth = 260;
			detailsColumn.Name = "detailsColumn";
			detailsColumn.ReadOnly = true;
			detailsColumn.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
			actionColumn.HeaderText = "SAFE ACTION";
			actionColumn.MinimumWidth = 150;
			actionColumn.Name = "actionColumn";
			actionColumn.ReadOnly = true;
			actionColumn.Width = 160;
			closeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			closeButton.DialogResult = DialogResult.Cancel;
			closeButton.Location = new Point(28, 676);
			closeButton.Size = new Size(110, 42);
			closeButton.Text = "Close";
			copyButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			copyButton.Enabled = false;
			copyButton.Location = new Point(628, 676);
			copyButton.Size = new Size(138, 42);
			copyButton.Text = "Copy Report";
			copyButton.Click += CopyButton_Click;
			actionButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			actionButton.Enabled = false;
			actionButton.Location = new Point(776, 676);
			actionButton.Size = new Size(154, 42);
			actionButton.Text = "Select a Repair";
			actionButton.Click += ActionButton_Click;
			runButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			runButton.Location = new Point(940, 676);
			runButton.Size = new Size(152, 42);
			runButton.Text = "Run All Checks";
			runButton.UseAccentStyle = true;
			runButton.Click += RunButton_Click;
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = SettingsPalette.Window;
			CancelButton = closeButton;
			ClientSize = new Size(1120, 740);
			Controls.Add(titleLabel);
			Controls.Add(subtitleLabel);
			Controls.Add(statusLabel);
			Controls.Add(resultsGrid);
			Controls.Add(closeButton);
			Controls.Add(copyButton);
			Controls.Add(actionButton);
			Controls.Add(runButton);
			Font = new Font("Segoe UI", 10F);
			MinimizeBox = false;
			MinimumSize = new Size(900, 620);
			Name = "TroubleshooterDialog";
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Synix Troubleshooter";
			((System.ComponentModel.ISupportInitialize)resultsGrid).EndInit();
			ResumeLayout(false);
		}

		private Label titleLabel = null!;
		private Label subtitleLabel = null!;
		private Label statusLabel = null!;
		private DataGridView resultsGrid = null!;
		private DataGridViewTextBoxColumn resultColumn = null!;
		private DataGridViewTextBoxColumn areaColumn = null!;
		private DataGridViewTextBoxColumn subjectColumn = null!;
		private DataGridViewTextBoxColumn detailsColumn = null!;
		private DataGridViewTextBoxColumn actionColumn = null!;
		private ModernSettingsButton closeButton = null!;
		private ModernSettingsButton copyButton = null!;
		private ModernSettingsButton actionButton = null!;
		private ModernSettingsButton runButton = null!;
	}
}
