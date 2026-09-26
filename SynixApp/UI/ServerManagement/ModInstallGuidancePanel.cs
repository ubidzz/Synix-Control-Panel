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
using Synix_Control_Panel.SynixEngine.ModManagement;

namespace Synix_Control_Panel.SynixApp.UI.ServerManagement;

internal sealed class ModInstallGuidancePanel : UserControl
{
	private readonly RichTextBox[] _sections;

	internal ModInstallGuidancePanel()
	{
		Name = "modInstallGuidance";
		Dock = DockStyle.Fill;
		BackColor = SettingsPalette.Window;
		TableLayoutPanel layout = new() { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Margin = Padding.Empty };
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		_sections = new RichTextBox[4];
		string[] keys = ["Compatibility", "Destination", "Activation", "Recovery"];
		for (int i = 0; i < keys.Length; i++)
		{
			layout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
			ModernSettingsCard card = new() { Dock = DockStyle.Fill, Padding = new Padding(10, 6, 10, 8),
				Margin = new Padding(0, 0, 0, i == 3 ? 0 : 6), FillColor = SettingsPalette.Input };
			TableLayoutPanel content = new() { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = Padding.Empty };
			content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			content.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
			content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
			string title = ModInstallGuidance.Text(keys[i]);
			Label heading = new() { Text = title, Name = "modGuide" + keys[i] + "Heading", Dock = DockStyle.Fill,
				Margin = Padding.Empty, UseMnemonic = false, ForeColor = SettingsPalette.PrimaryText,
				Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
			ModernSettingsDetailsBox detail = new() { Name = "modGuide" + keys[i], AccessibleName = title, Dock = DockStyle.Fill,
				Margin = Padding.Empty, ReadOnly = true, Multiline = true, WordWrap = true, DetectUrls = false,
				ScrollBars = RichTextBoxScrollBars.Vertical, BorderStyle = BorderStyle.None,
				BackColor = SettingsPalette.Input, ForeColor = SettingsPalette.PrimaryText, Font = new Font("Segoe UI", 9F) };
			_sections[i] = detail;
			content.Controls.Add(heading, 0, 0);
			content.Controls.Add(detail, 0, 1);
			card.Controls.Add(content);
			layout.Controls.Add(card, 0, i);
		}
		Controls.Add(layout);
		ShowGuidance(ModInstallGuidance.Pending);
	}

	internal void ShowGuidance(ModInstallGuidance guidance)
	{
		string[] text = [guidance.Compatibility, guidance.Destination, guidance.Activation, guidance.Recovery];
		for (int i = 0; i < text.Length; i++)
		{
			_sections[i].Text = text[i];
			_sections[i].Select(0, 0);
			_sections[i].ScrollToCaret();
		}
	}
}
