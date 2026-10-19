using System.Diagnostics;
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.ServerHandler;

namespace Synix_Control_Panel.SynixApp.UI.ServerManagement;

internal sealed class GameTextSupportDialog : Form
{
	internal GameTextSupportDialog(string gameName)
	{
		Name = "gameTextSupportDialog";
		Text = LocalizationManager.Get("GameText.Title");
		ClientSize = new Size(950, 720);
		MinimumSize = new Size(780, 570);
		StartPosition = FormStartPosition.CenterParent;
		ShowInTaskbar = false;
		Font = new Font("Segoe UI", 10F);
		BackColor = SettingsPalette.Window;
		TableLayoutPanel layout = UniversalModDialogStyle.Layout(this, [48, 48, -1, 52]);
		layout.Controls.Add(UniversalModDialogStyle.Label(Text, heading: true), 0, 0);
		ModernSettingsComboBox games = new()
		{
			Name = "gameTextSupportPicker", Dock = DockStyle.Top,
			DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = LocalizationManager.Get("GameText.SelectGame")
		};
		games.Items.AddRange(GameDatabase.GetGames.Select(game => game.Game).OrderBy(name => name).Cast<object>().ToArray());
		// Custom definitions without a catalog entry stay explicitly unverified.
		if (!games.Items.Contains(gameName)) games.Items.Add(gameName);
		games.SelectedItem = gameName;
		layout.Controls.Add(games, 0, 1);
		RichTextBox details = new()
		{
			Name = "gameTextSupportDetails", Dock = DockStyle.Fill, ReadOnly = true, WordWrap = true,
			DetectUrls = true, BorderStyle = BorderStyle.None, ScrollBars = RichTextBoxScrollBars.Vertical,
			Text = GameTextSupportCatalog.HelpText(gameName), Font = Font, Margin = new Padding(8)
		};
		games.SelectedIndexChanged += (_, _) =>
		{
			details.Text = GameTextSupportCatalog.HelpText(games.Text);
			details.SelectionStart = 0;
			details.ScrollToCaret();
		};
		details.LinkClicked += (_, e) =>
		{
			if (!GameTextSupportCatalog.IsSafeSource(e.LinkText) ||
				GameTextSupportCatalog.ForGame(games.Text)?.Fields.Any(field => field.Sources.Contains(e.LinkText!)) != true) return;
			try { Process.Start(new ProcessStartInfo(e.LinkText!) { UseShellExecute = true }); }
			catch (Exception exception) { PlainEnglishErrorDialog.ShowError(this, Text, exception.Message); }
		};
		layout.Controls.Add(details, 0, 2);
		FlowLayoutPanel actions = UniversalModDialogStyle.Actions(layout, 3);
		ModernSettingsButton close = new()
		{
			Text = LocalizationManager.Get("ModManager.Button.Close"), Size = new Size(160, 42), DialogResult = DialogResult.OK
		};
		actions.Controls.Add(close);
		CancelButton = close;
		ThemeManager.Apply(this);
		details.BackColor = SettingsPalette.Input;
		details.ForeColor = SettingsPalette.PrimaryText;
		EventHandler languageChanged = (_, _) => details.Text = GameTextSupportCatalog.HelpText(games.Text);
		LocalizationManager.LanguageChanged += languageChanged;
		Disposed += (_, _) => LocalizationManager.LanguageChanged -= languageChanged;
	}
}
