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
using System.Diagnostics;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixEngine.ModManagement;

namespace Synix_Control_Panel.SynixApp.UI.ServerManagement;

internal sealed class GameModSupportDialog : Form
{
	internal GameModSupportDialog(string gameName)
	{
		Name = "gameModSupportDialog";
		Text = LocalizationManager.Get("ModSupport.Title");
		ClientSize = new Size(950, 690);
		MinimumSize = new Size(780, 570);
		StartPosition = FormStartPosition.CenterParent;
		ShowInTaskbar = false;
		Font = new Font("Segoe UI", 10F);
		BackColor = SettingsPalette.Window;
		TableLayoutPanel layout = UniversalModDialogStyle.Layout(this, [48, -1, 52]);
		layout.Controls.Add(UniversalModDialogStyle.Label(Text, heading: true), 0, 0);
		RichTextBox details = new()
		{
			Name = "gameModSupportDetails", Dock = DockStyle.Fill, ReadOnly = true, WordWrap = true,
			DetectUrls = true, BorderStyle = BorderStyle.None, ScrollBars = RichTextBoxScrollBars.Vertical,
			Text = GameModSupportCatalog.HelpText(gameName), Font = Font, Margin = new Padding(8)
		};
		details.LinkClicked += (_, e) =>
		{
			// Only user-clicked, reviewed documentation links; never a package command.
			if (e.LinkText == null || !GameModSupportCatalog.IsSafeSource(e.LinkText) ||
				GameModSupportCatalog.ForGame(gameName)?.Sources.Contains(e.LinkText) != true) return;
			try { Process.Start(new ProcessStartInfo(e.LinkText) { UseShellExecute = true }); }
			catch (Exception exception) { PlainEnglishErrorDialog.ShowError(this, Text, exception.Message); }
		};
		layout.Controls.Add(details, 0, 1);
		FlowLayoutPanel actions = UniversalModDialogStyle.Actions(layout, 2);
		ModernSettingsButton close = new() { Text = LocalizationManager.Get("ModManager.Button.Close"),
			Size = new Size(160, 42), DialogResult = DialogResult.OK };
		actions.Controls.Add(close);
		CancelButton = close;
		ThemeManager.Apply(this);
		details.BackColor = SettingsPalette.Input;
		details.ForeColor = SettingsPalette.PrimaryText;
	}
}

