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
using Synix_Control_Panel.SynixEngine.Minecraft;

namespace Synix_Control_Panel.SynixApp.UI.ServerManagement;

internal sealed class MinecraftChangePreview : Form
{
	internal MinecraftChangePreview(string title, IReadOnlyList<MinecraftFileChange> changes,
		IReadOnlyList<string> notes, bool canApply = true)
		: this(title, changes, notes, canApply, null) { }

	internal MinecraftChangePreview(string title, MinecraftCompatibilityReport report)
		: this(title, [], [MinecraftControlCenter.TextFor("CompatibilityNotice")], false, report) { }

	private MinecraftChangePreview(string title, IReadOnlyList<MinecraftFileChange> changes,
		IReadOnlyList<string> notes, bool canApply, MinecraftCompatibilityReport? report)
	{
		Text = title;
		StartPosition = FormStartPosition.CenterParent;
		ShowInTaskbar = false;
		MinimumSize = new Size(760, 540);
		ClientSize = new Size(960, 690);
		Font = new Font("Segoe UI", 10);
		BackColor = SettingsPalette.Window;
		ForeColor = SettingsPalette.PrimaryText;
		TableLayoutPanel layout = new() { Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 4, ColumnCount = 1,
			BackColor = SettingsPalette.Window };
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
		layout.RowStyles.Add(new RowStyle(SizeType.Percent, 65));
		layout.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
		layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
		string summary = report == null ? MinecraftControlCenter.TextFor("PreviewSummary",
			changes.Count(change => change.Source != null && change.ExpectedPreviousHash == null),
			changes.Count(change => change.Source != null && change.ExpectedPreviousHash != null),
			changes.Count(change => change.Source == null)) : MinecraftControlCenter.TextFor("CompatibilitySummary",
				report.AddOns.Count(addon => !addon.Bundled), report.Findings.Count);
		layout.Controls.Add(new Label { Name = "minecraftPreviewSummary", Dock = DockStyle.Fill, Text = summary,
			ForeColor = SettingsPalette.SecondaryText, Padding = new Padding(4, 4, 4, 8) }, 0, 0);
		DataGridView grid = new() { Name = "minecraftChangePreviewGrid", Dock = DockStyle.Fill, ReadOnly = true,
			AllowUserToAddRows = false, AllowUserToDeleteRows = false, RowHeadersVisible = false, MultiSelect = false };
		if (report == null)
		{
			grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "File", HeaderText = MinecraftControlCenter.TextFor("File"), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
			grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Change", HeaderText = MinecraftControlCenter.TextFor("Change"), Width = 150 });
		}
		else
		{
			foreach (string column in new[] { "Area", "Detail" })
				grid.Columns.Add(new DataGridViewTextBoxColumn { Name = column, HeaderText = MinecraftControlCenter.TextFor(column),
					AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 95 });
		}
		MinecraftControlCenter.StyleContentGrid(grid);
		if (report == null)
		{
			foreach (MinecraftFileChange change in changes)
				grid.Rows.Add(change.RelativePath, MinecraftControlCenter.TextFor(change.Source == null ? "Remove" :
					change.ExpectedPreviousHash == null ? "Add" : "Replace"));
		}
		else if (report.Findings.Count == 0)
			grid.Rows.Add(MinecraftControlCenter.TextFor("Checks"), MinecraftControlCenter.TextFor(
				report.AddOns.Count == 0 ? "NoCompatibilityAddOns" : "NoCompatibilityFindings"));
		else
			foreach (MinecraftFinding finding in report.Findings) grid.Rows.Add(finding.Area, finding.Detail);
		ModernSettingsCard tableCard = new() { Dock = DockStyle.Fill, Padding = new Padding(4),
			Margin = new Padding(3, 3, 3, 10), FillColor = SettingsPalette.Input, BackColor = SettingsPalette.Input };
		tableCard.Controls.Add(grid);
		layout.Controls.Add(tableCard, 0, 1);
		TextBox warnings = new() { Name = "minecraftPreviewNotes", Dock = DockStyle.Fill, Multiline = true, ReadOnly = true,
			BorderStyle = BorderStyle.None, ScrollBars = ScrollBars.Vertical, WordWrap = true,
			Text = string.Join(Environment.NewLine, notes), BackColor = SettingsPalette.Input, ForeColor = SettingsPalette.PrimaryText };
		ModernSettingsCard notesCard = new() { Dock = DockStyle.Fill, Padding = new Padding(14),
			Margin = new Padding(3, 0, 3, 10), FillColor = SettingsPalette.Input, BackColor = SettingsPalette.Input };
		notesCard.Controls.Add(warnings);
		layout.Controls.Add(notesCard, 0, 2);
		FlowLayoutPanel buttons = new() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
		ModernSettingsButton apply = new() { Name = "minecraftApplyChanges", Width = 170, Height = 42,
			Text = MinecraftControlCenter.TextFor("Apply"), DialogResult = DialogResult.OK, UseAccentStyle = true,
			Enabled = canApply && changes.Count > 0 };
		ModernSettingsButton cancel = new() { Width = 160, Height = 42, Text = MinecraftControlCenter.TextFor("Close"), DialogResult = DialogResult.Cancel };
		if (report == null) buttons.Controls.Add(apply);
		else apply.Dispose();
		buttons.Controls.Add(cancel);
		CancelButton = cancel;
		layout.Controls.Add(buttons, 0, 3);
		Controls.Add(layout);
		ThemeManager.Apply(this);
	}
}
