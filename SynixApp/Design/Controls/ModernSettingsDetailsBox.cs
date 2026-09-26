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
namespace Synix_Control_Panel.SynixApp.Design.Controls;

// Selectable, wrapping guidance text. Unlike a log console, this uses the
// settings input surface when the application reapplies its theme.
internal sealed class ModernSettingsDetailsBox : RichTextBox
{
	internal ModernSettingsDetailsBox()
	{
		ReadOnly = Multiline = WordWrap = true;
		DetectUrls = false;
		BorderStyle = BorderStyle.None;
		ScrollBars = RichTextBoxScrollBars.Vertical;
		BackColor = SettingsPalette.Input;
		ForeColor = SettingsPalette.PrimaryText;
	}
}
