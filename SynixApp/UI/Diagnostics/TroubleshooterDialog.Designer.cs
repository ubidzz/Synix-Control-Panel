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
#pragma warning disable CS8600

namespace Synix_Control_Panel.SynixApp.UI.Diagnostics
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
			DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TroubleshooterDialog));
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
			// 
			// titleLabel
			// 
			titleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			titleLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
			titleLabel.ForeColor = Color.FromArgb(245, 247, 251);
			titleLabel.Location = new Point(28, 20);
			titleLabel.Name = "titleLabel";
			titleLabel.Size = new Size(1064, 38);
			titleLabel.TabIndex = 0;
			titleLabel.Text = "Synix Troubleshooter";
			// 
			// subtitleLabel
			// 
			subtitleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			subtitleLabel.Font = new Font("Segoe UI", 9.5F);
			subtitleLabel.ForeColor = Color.FromArgb(158, 172, 194);
			subtitleLabel.Location = new Point(28, 60);
			subtitleLabel.Name = "subtitleLabel";
			subtitleLabel.Size = new Size(1064, 42);
			subtitleLabel.TabIndex = 1;
			subtitleLabel.Text = "Check shared runtimes, server files, configurations, ports, Windows Firewall, disk space, interrupted processes, recent logs, and Synix update health from one place.";
			// 
			// statusLabel
			// 
			statusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			statusLabel.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
			statusLabel.ForeColor = Color.FromArgb(158, 172, 194);
			statusLabel.Location = new Point(28, 108);
			statusLabel.Name = "statusLabel";
			statusLabel.Size = new Size(1064, 28);
			statusLabel.TabIndex = 2;
			statusLabel.Text = "Ready to check this computer.";
			// 
			// resultsGrid
			// 
			resultsGrid.AllowUserToAddRows = false;
			resultsGrid.AllowUserToDeleteRows = false;
			resultsGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			resultsGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
			resultsGrid.BackgroundColor = Color.FromArgb(12, 21, 36);
			resultsGrid.BorderStyle = BorderStyle.None;
			resultsGrid.ColumnHeadersHeight = 40;
			resultsGrid.Columns.AddRange(new DataGridViewColumn[] { resultColumn, areaColumn, subjectColumn, detailsColumn, actionColumn });
			resultsGrid.Location = new Point(28, 142);
			resultsGrid.MultiSelect = false;
			resultsGrid.Name = "resultsGrid";
			resultsGrid.ReadOnly = true;
			resultsGrid.RowHeadersVisible = false;
			resultsGrid.RowTemplate.MinimumHeight = 36;
			resultsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			resultsGrid.Size = new Size(1064, 516);
			resultsGrid.TabIndex = 0;
			resultsGrid.SelectionChanged += ResultsGrid_SelectionChanged;
			// 
			// resultColumn
			// 
			resultColumn.HeaderText = "RESULT";
			resultColumn.MinimumWidth = 90;
			resultColumn.Name = "resultColumn";
			resultColumn.ReadOnly = true;
			resultColumn.Width = 90;
			// 
			// areaColumn
			// 
			areaColumn.HeaderText = "AREA";
			areaColumn.MinimumWidth = 150;
			areaColumn.Name = "areaColumn";
			areaColumn.ReadOnly = true;
			areaColumn.Width = 170;
			// 
			// subjectColumn
			// 
			subjectColumn.HeaderText = "SERVER / ITEM";
			subjectColumn.MinimumWidth = 170;
			subjectColumn.Name = "subjectColumn";
			subjectColumn.ReadOnly = true;
			subjectColumn.Width = 190;
			// 
			// detailsColumn
			// 
			detailsColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
			detailsColumn.DefaultCellStyle = dataGridViewCellStyle1;
			detailsColumn.HeaderText = "DETAILS";
			detailsColumn.MinimumWidth = 260;
			detailsColumn.Name = "detailsColumn";
			detailsColumn.ReadOnly = true;
			// 
			// actionColumn
			// 
			actionColumn.HeaderText = "SAFE ACTION";
			actionColumn.MinimumWidth = 150;
			actionColumn.Name = "actionColumn";
			actionColumn.ReadOnly = true;
			actionColumn.Width = 160;
			// 
			// closeButton
			// 
			closeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			closeButton.BackColor = Color.FromArgb(12, 21, 36);
			closeButton.DialogResult = DialogResult.Cancel;
			closeButton.FlatStyle = FlatStyle.Flat;
			closeButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			closeButton.ForeColor = Color.FromArgb(245, 247, 251);
			closeButton.Location = new Point(28, 676);
			closeButton.Name = "closeButton";
			closeButton.Size = new Size(110, 42);
			closeButton.TabIndex = 3;
			closeButton.Text = "Close";
			closeButton.UseVisualStyleBackColor = false;
			// 
			// copyButton
			// 
			copyButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			copyButton.BackColor = Color.FromArgb(12, 21, 36);
			copyButton.Enabled = false;
			copyButton.FlatStyle = FlatStyle.Flat;
			copyButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			copyButton.ForeColor = Color.FromArgb(245, 247, 251);
			copyButton.Location = new Point(628, 676);
			copyButton.Name = "copyButton";
			copyButton.Size = new Size(138, 42);
			copyButton.TabIndex = 4;
			copyButton.Text = "Copy Report";
			copyButton.UseVisualStyleBackColor = false;
			copyButton.Click += CopyButton_Click;
			// 
			// actionButton
			// 
			actionButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			actionButton.BackColor = Color.FromArgb(12, 21, 36);
			actionButton.Enabled = false;
			actionButton.FlatStyle = FlatStyle.Flat;
			actionButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			actionButton.ForeColor = Color.FromArgb(245, 247, 251);
			actionButton.Location = new Point(776, 676);
			actionButton.Name = "actionButton";
			actionButton.Size = new Size(154, 42);
			actionButton.TabIndex = 5;
			actionButton.Text = "Select a Repair";
			actionButton.UseVisualStyleBackColor = false;
			actionButton.Click += ActionButton_Click;
			// 
			// runButton
			// 
			runButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			runButton.BackColor = Color.FromArgb(12, 21, 36);
			runButton.FlatStyle = FlatStyle.Flat;
			runButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			runButton.ForeColor = Color.FromArgb(245, 247, 251);
			runButton.Location = new Point(940, 676);
			runButton.Name = "runButton";
			runButton.Size = new Size(152, 42);
			runButton.TabIndex = 6;
			runButton.Text = "Run All Checks";
			runButton.UseAccentStyle = true;
			runButton.UseVisualStyleBackColor = false;
			runButton.Click += RunButton_Click;
			// 
			// TroubleshooterDialog
			// 
			AutoScaleDimensions = new SizeF(7F, 17F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.FromArgb(8, 13, 24);
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
			Icon = (Icon)resources.GetObject("$this.Icon");
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
