namespace Synix_Control_Panel.SynixApp.UI.Dashboard;

public partial class MainGUI
{
	private int GridPixels(int logicalPixels) => Math.Max(1, (int)Math.Round(logicalPixels * dataGridView1.DeviceDpi / 96d));

	private void ConfigureServerGridLayout()
	{
		dataGridView1.SuspendLayout();
		try
		{
			// LanguageChanged runs before the general form-localization pass. Update
			// these headings first so their minimum widths use the new language.
			colGame.HeaderText = LocalizationManager.Get("Text.1FD5F5A768D30B9B6651");
			colName.HeaderText = LocalizationManager.Get("Text.41CFA866FAA62B2104E6");
			colPort.HeaderText = LocalizationManager.Get("Text.72E9A59F5A1D6289DBAC");
			colQueryPort.HeaderText = LocalizationManager.Get("Text.B80A37564FBB3C075320");
			colPlayerCount.HeaderText = LocalizationManager.Get("Text.84E12AC655DD8D75DD44");
			colUptime.HeaderText = LocalizationManager.Get("Text.D63AB4711473B0398FEB");
			colStatus.HeaderText = LocalizationManager.Get("ModManager.Column.Status");
			if (dataGridView1.Columns["BackupHealth"] is { } backupHeading)
				backupHeading.HeaderText = LocalizationManager.Get("SettingsPage.Backups.Heading");
			dataGridView1.AllowUserToResizeColumns = true;
			dataGridView1.AllowUserToResizeRows = false;
			dataGridView1.ScrollBars = ScrollBars.Both;
			dataGridView1.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
			dataGridView1.ColumnHeadersDefaultCellStyle.Padding = new Padding(GridPixels(8), GridPixels(8), GridPixels(4), GridPixels(8));
			dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridView1.DefaultCellStyle.Padding = new Padding(GridPixels(6));
			dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
			dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
			dataGridView1.RowTemplate.MinimumHeight = GridPixels(44);

			// Names and descriptions share spare space and wrap. Numeric values retain
			// their own width instead of taking space away from long game titles.
			WrapColumn(colGame, 160, 32);
			WrapColumn(colName, 110, 28);
			WrapColumn(colStatus, 104, 18);
			if (dataGridView1.Columns["BackupHealth"] is { } backup) WrapColumn(backup, 135, 22);
			foreach (DataGridViewColumn column in new[] { colPort, colQueryPort, colPlayerCount, colUptime })
			{
				column.MinimumWidth = Math.Max(HeaderMinimumWidth(column), GridPixels(column == colUptime ? 80 : column == colPlayerCount ? 68 : 56));
				column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;
				// WinForms can retain the previous auto-sized width after the minimum
				// grows (for example, Uptime -> Betriebszeit). Remeasure immediately.
				dataGridView1.AutoResizeColumn(column.Index, DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader);
			}
			if (dataGridView1.Columns["IconCol"] is { } icon)
			{
				icon.MinimumWidth = GridPixels(46);
				icon.Width = icon.MinimumWidth;
				icon.DefaultCellStyle.Padding = new Padding(GridPixels(8));
			}
			ResizeServerGridRows();
		}
		finally { dataGridView1.ResumeLayout(); }
	}

	private void WrapColumn(DataGridViewColumn column, int minimumWidth, float weight)
	{
		column.MinimumWidth = Math.Max(HeaderMinimumWidth(column), GridPixels(minimumWidth));
		column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
		column.FillWeight = weight;
		column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
	}

	private int HeaderMinimumWidth(DataGridViewColumn column)
	{
		DataGridViewCellStyle style = column.HeaderCell.InheritedStyle;
		return TextRenderer.MeasureText(column.HeaderText, style.Font ?? dataGridView1.Font).Width + style.Padding.Horizontal + GridPixels(2);
	}

	private void ResizeServerGridRows()
	{
		int minimumHeight = GridPixels(44);
		foreach (DataGridViewRow row in dataGridView1.Rows) row.MinimumHeight = minimumHeight;
		// Backup summaries are unbound cells, so repainting alone does not resize
		// rows when an asynchronous scan replaces the initial checking message.
		dataGridView1.AutoResizeRows(DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders);
	}
}
