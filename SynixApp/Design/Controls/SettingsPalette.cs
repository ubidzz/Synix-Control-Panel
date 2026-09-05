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
using System.ComponentModel;
using System.Drawing.Drawing2D;
using Synix_Control_Panel.SynixApp.Design;

namespace Synix_Control_Panel.SynixApp.Design.Controls
{
	public static class SettingsPalette
	{
		public static Color Window => ThemeManager.Colors.Window;
		public static Color TitleBar => ThemeManager.Colors.TitleBar;
		public static Color Sidebar => ThemeManager.Colors.Sidebar;
		public static Color Card => ThemeManager.Colors.Card;
		public static Color CardHover => ThemeManager.Colors.CardHover;
		public static Color Input => ThemeManager.Colors.Input;
		public static Color AlternateInput => ThemeManager.Colors.AlternateInput;
		public static Color Border => ThemeManager.Colors.Border;
		public static Color BorderHover => ThemeManager.Colors.BorderHover;
		public static Color PrimaryText => ThemeManager.Colors.PrimaryText;
		public static Color SecondaryText => ThemeManager.Colors.SecondaryText;
		public static Color MutedText => ThemeManager.Colors.MutedText;
		public static Color Accent => ThemeManager.Colors.Accent;
		public static Color AccentHover => ThemeManager.Colors.AccentHover;
		public static Color AccentSoft => ThemeManager.Colors.AccentSoft;
		public static Color Warning => ThemeManager.Colors.Warning;
		public static Color Selection => ThemeManager.Colors.Selection;
		public static Color Divider => ThemeManager.Colors.Divider;
		public static Color InfoSurface => ThemeManager.Colors.InfoSurface;
		public static Color DisabledSurface => ThemeManager.Colors.DisabledSurface;
		public static Color DisabledText => ThemeManager.Colors.DisabledText;
		public static Color Console => ThemeManager.Colors.Console;
		public static Color AccentText => ThemeManager.IsDarkMode ? Window : Color.White;
		public static Color Success => ThemeManager.IsDarkMode
			? Color.FromArgb(80, 230, 164)
			: Color.FromArgb(17, 124, 82);
		public static Color Danger => ThemeManager.IsDarkMode
			? Color.FromArgb(250, 116, 128)
			: Color.FromArgb(190, 45, 60);
		public static Color Ram => ThemeManager.IsDarkMode
			? Color.FromArgb(167, 139, 250)
			: Color.FromArgb(109, 72, 184);
	}
}
