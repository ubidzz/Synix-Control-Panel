// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================

namespace Synix_Control_Panel.SynixApp.Localization;

/// <summary>
/// Keeps confirmation and error dialogs on the selected interface language.
/// Variable server data and technical exception details remain unchanged.
/// </summary>
internal static class LocalizedMessageBox
{
	public static DialogResult Show(string text) =>
		MessageBox.Show(LocalizationManager.TranslateMessageText(text));

	public static DialogResult Show(string text, string caption) =>
		MessageBox.Show(
			LocalizationManager.TranslateMessageText(text),
			LocalizationManager.TranslateMessageText(caption));

	public static DialogResult Show(
		string text,
		string caption,
		MessageBoxButtons buttons) =>
		MessageBox.Show(
			LocalizationManager.TranslateMessageText(text),
			LocalizationManager.TranslateMessageText(caption),
			buttons);

	public static DialogResult Show(
		string text,
		string caption,
		MessageBoxButtons buttons,
		MessageBoxIcon icon) =>
		MessageBox.Show(
			LocalizationManager.TranslateMessageText(text),
			LocalizationManager.TranslateMessageText(caption),
			buttons,
			icon);

	public static DialogResult Show(
		string text,
		string caption,
		MessageBoxButtons buttons,
		MessageBoxIcon icon,
		MessageBoxDefaultButton defaultButton) =>
		MessageBox.Show(
			LocalizationManager.TranslateMessageText(text),
			LocalizationManager.TranslateMessageText(caption),
			buttons,
			icon,
			defaultButton);

	public static DialogResult Show(
		string text,
		string caption,
		MessageBoxButtons buttons,
		MessageBoxIcon icon,
		MessageBoxDefaultButton defaultButton,
		MessageBoxOptions options) =>
		MessageBox.Show(
			LocalizationManager.TranslateMessageText(text),
			LocalizationManager.TranslateMessageText(caption),
			buttons,
			icon,
			defaultButton,
			options);

	public static DialogResult Show(
		IWin32Window? owner,
		string text) =>
		MessageBox.Show(
			owner,
			LocalizationManager.TranslateMessageText(text));

	public static DialogResult Show(
		IWin32Window? owner,
		string text,
		string caption) =>
		MessageBox.Show(
			owner,
			LocalizationManager.TranslateMessageText(text),
			LocalizationManager.TranslateMessageText(caption));

	public static DialogResult Show(
		IWin32Window? owner,
		string text,
		string caption,
		MessageBoxButtons buttons) =>
		MessageBox.Show(
			owner,
			LocalizationManager.TranslateMessageText(text),
			LocalizationManager.TranslateMessageText(caption),
			buttons);

	public static DialogResult Show(
		IWin32Window? owner,
		string text,
		string caption,
		MessageBoxButtons buttons,
		MessageBoxIcon icon) =>
		MessageBox.Show(
			owner,
			LocalizationManager.TranslateMessageText(text),
			LocalizationManager.TranslateMessageText(caption),
			buttons,
			icon);

	public static DialogResult Show(
		IWin32Window? owner,
		string text,
		string caption,
		MessageBoxButtons buttons,
		MessageBoxIcon icon,
		MessageBoxDefaultButton defaultButton) =>
		MessageBox.Show(
			owner,
			LocalizationManager.TranslateMessageText(text),
			LocalizationManager.TranslateMessageText(caption),
			buttons,
			icon,
			defaultButton);

	public static DialogResult Show(
		IWin32Window? owner,
		string text,
		string caption,
		MessageBoxButtons buttons,
		MessageBoxIcon icon,
		MessageBoxDefaultButton defaultButton,
		MessageBoxOptions options) =>
		MessageBox.Show(
			owner,
			LocalizationManager.TranslateMessageText(text),
			LocalizationManager.TranslateMessageText(caption),
			buttons,
			icon,
			defaultButton,
			options);
}
