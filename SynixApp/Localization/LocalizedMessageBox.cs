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

namespace Synix_Control_Panel.SynixApp.Localization;

/// <summary>
/// Keeps confirmation and error dialogs on the selected interface language.
/// Variable server data and technical exception details remain unchanged.
/// </summary>
internal static class LocalizedMessageBox
{
	internal static MessageBoxOptions WithReadingDirection(MessageBoxOptions options) =>
		LocalizationManager.IsRightToLeft
			? options | MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
			: options;

	public static DialogResult Show(
		string text,
		string caption = "",
		MessageBoxButtons buttons = MessageBoxButtons.OK,
		MessageBoxIcon icon = MessageBoxIcon.None,
		MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1,
		MessageBoxOptions options = 0) =>
		MessageBox.Show(
			LocalizationManager.TranslateMessageText(text),
			LocalizationManager.TranslateMessageText(caption),
			buttons, icon, defaultButton, WithReadingDirection(options));

	public static DialogResult Show(
		IWin32Window? owner,
		string text,
		string caption = "",
		MessageBoxButtons buttons = MessageBoxButtons.OK,
		MessageBoxIcon icon = MessageBoxIcon.None,
		MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1,
		MessageBoxOptions options = 0) =>
		MessageBox.Show(
			owner,
			LocalizationManager.TranslateMessageText(text),
			LocalizationManager.TranslateMessageText(caption),
			buttons, icon, defaultButton, WithReadingDirection(options));
}
