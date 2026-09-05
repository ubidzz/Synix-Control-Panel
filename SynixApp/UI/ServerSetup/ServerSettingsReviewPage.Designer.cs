// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Design.Controls;

namespace Synix_Control_Panel.SynixApp.UI.ServerSetup;

partial class ServerSettingsReviewPage
{
	private void InitializeComponent()
	{
		cardSummary = new ModernSettingsCard();
		gridSummary = new DataGridView();
		columnSetting = new DataGridViewTextBoxColumn();
		columnValue = new DataGridViewTextBoxColumn();
		lblReviewNotice = new Label();
		cardSummary.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)gridSummary).BeginInit();
		SuspendLayout();
		// cardSummary
		cardSummary.Controls.Add(gridSummary);
		cardSummary.Dock = DockStyle.Fill;
		cardSummary.Name = "cardSummary";
		cardSummary.Padding = new Padding(16);
		cardSummary.TabIndex = 0;
		// gridSummary
		gridSummary.AllowUserToAddRows = false;
		gridSummary.AllowUserToDeleteRows = false;
		gridSummary.AllowUserToResizeRows = false;
		gridSummary.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
		gridSummary.BackgroundColor = SettingsPalette.Card;
		gridSummary.BorderStyle = BorderStyle.None;
		gridSummary.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
		gridSummary.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
		gridSummary.ColumnHeadersHeight = 36;
		gridSummary.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
		gridSummary.ColumnHeadersDefaultCellStyle.BackColor = SettingsPalette.Input;
		gridSummary.ColumnHeadersDefaultCellStyle.ForeColor = SettingsPalette.SecondaryText;
		gridSummary.ColumnHeadersDefaultCellStyle.SelectionBackColor = SettingsPalette.Input;
		gridSummary.ColumnHeadersDefaultCellStyle.SelectionForeColor = SettingsPalette.SecondaryText;
		gridSummary.Columns.AddRange(columnSetting, columnValue);
		gridSummary.DefaultCellStyle.BackColor = SettingsPalette.Card;
		gridSummary.DefaultCellStyle.ForeColor = SettingsPalette.PrimaryText;
		gridSummary.DefaultCellStyle.SelectionBackColor = SettingsPalette.Selection;
		gridSummary.DefaultCellStyle.SelectionForeColor = SettingsPalette.PrimaryText;
		gridSummary.DefaultCellStyle.Padding = new Padding(8, 6, 8, 6);
		gridSummary.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
		gridSummary.Dock = DockStyle.Fill;
		gridSummary.EnableHeadersVisualStyles = false;
		gridSummary.GridColor = SettingsPalette.Divider;
		gridSummary.MultiSelect = false;
		gridSummary.Name = "gridSummary";
		gridSummary.ReadOnly = true;
		gridSummary.RowHeadersVisible = false;
		gridSummary.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
		gridSummary.TabIndex = 0;
		// columnSetting
		columnSetting.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
		columnSetting.FillWeight = 30F;
		columnSetting.HeaderText = LocalizationManager.Get("ServerSetup.Review.Setting");
		columnSetting.Name = "columnSetting";
		columnSetting.SortMode = DataGridViewColumnSortMode.NotSortable;
		// columnValue
		columnValue.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
		columnValue.FillWeight = 70F;
		columnValue.HeaderText = LocalizationManager.Get("ServerSetup.Review.Value");
		columnValue.Name = "columnValue";
		columnValue.SortMode = DataGridViewColumnSortMode.NotSortable;
		// lblReviewNotice
		lblReviewNotice.Dock = DockStyle.Bottom;
		lblReviewNotice.Height = 62;
		lblReviewNotice.ForeColor = SettingsPalette.SecondaryText;
		lblReviewNotice.Name = "lblReviewNotice";
		lblReviewNotice.Padding = new Padding(4, 12, 4, 0);
		lblReviewNotice.Text = LocalizationManager.Get("ServerSetup.Review.Notice");
		lblReviewNotice.TabIndex = 1;
		// ServerSettingsReviewPage
		AutoScaleDimensions = new SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		BackColor = SettingsPalette.Window;
		Controls.Add(cardSummary);
		Controls.Add(lblReviewNotice);
		Name = "ServerSettingsReviewPage";
		Size = new Size(914, 388);
		cardSummary.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)gridSummary).EndInit();
		ResumeLayout(false);
	}

	private ModernSettingsCard cardSummary;
	private DataGridView gridSummary;
	private DataGridViewTextBoxColumn columnSetting;
	private DataGridViewTextBoxColumn columnValue;
	private Label lblReviewNotice;
}
