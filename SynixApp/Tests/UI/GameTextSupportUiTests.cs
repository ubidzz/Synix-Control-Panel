using System.Drawing;
using System.Windows.Forms;
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.Localization;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixApp.UI.ServerManagement;
using Synix_Control_Panel.SynixApp.UI.ServerSetup;
using Synix_Control_Panel.SynixEngine;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class GameTextSupportUiTests
{
	[Theory]
	[InlineData("en-US")]
	[InlineData("de-DE")]
	[InlineData("fr-FR")]
	[InlineData("es-ES")]
	[InlineData("zh-Hans")]
	[InlineData("zh-Hant")]
	public void CharacterSupportIsVisibleWithoutReadingOrRevealingPasswords(string language) => WorkflowUiTest.Run(() =>
	{
		try
		{
			LocalizationManager.Initialize(language);
			using Form host = new() { ClientSize = new Size(930, 620), ShowInTaskbar = false,
				StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000) };
			using ServerSettingsSecurityPage security = new() { Dock = DockStyle.Fill };
			host.Controls.Add(security);
			security.ConfigureForGame(GameDatabase.GetGame("ARK: Survival Ascended"));
			security.LoadSecrets(new SynixServerPasswords("朋友密碼123", "管理密碼123", ""), "");
			security.SetPrivacyMode(true);
			host.Show(); Application.DoEvents();
			Assert.True(security.txtPassword.UseSystemPasswordChar);
			Assert.True(security.txtAdminPassword.UseSystemPasswordChar);
			Assert.Equal("朋友密碼123", security.ServerPassword);
			Assert.Equal("管理密碼123", security.AdminPassword);
			Button detailsButton = Find<Button>(security, "btnPasswordCharacters");
			Assert.True(detailsButton.Enabled);
			Assert.True(detailsButton.Bottom < detailsButton.Parent!.ClientSize.Height);
			AssertFits(detailsButton);
			foreach (string name in new[] { "lblPasswordCharacters", "lblAdminPasswordCharacters" })
			{
				Label label = Find<Label>(security, name);
				Assert.Contains(LocalizationManager.Get("GameText.Status.Unverified"), label.Text);
				AssertFits(label);
			}
			Assert.True(Find<Label>(security, "lblPasswordCharacters").Top > security.txtPassword.Bottom);
			using GameTextSupportDialog dialog = new("Onset");
			dialog.StartPosition = FormStartPosition.Manual; dialog.Location = new Point(-32000, -32000);
			dialog.Show(); Application.DoEvents();
			RichTextBox details = Find<RichTextBox>(dialog, "gameTextSupportDetails");
			Assert.True(details.ReadOnly); Assert.True(details.WordWrap);
			Assert.Contains(LocalizationManager.Get("GameText.Status.Documented"), details.Text);
			Assert.Contains(LocalizationManager.Get("GameText.Status.Unverified"), details.Text);
			Assert.DoesNotContain(security.ServerPassword, details.Text);
			Assert.DoesNotContain(security.AdminPassword, details.Text);
			ComboBox picker = Find<ComboBox>(dialog, "gameTextSupportPicker");
			Assert.Equal(GameDatabase.GetGames.Count(), picker.Items.Count);
			picker.SelectedItem = "Minecraft";
			Assert.StartsWith("Minecraft", details.Text);
			Assert.Contains(LocalizationManager.Get("GameText.Status.NotApplicable"), details.Text);
			if (Environment.GetEnvironmentVariable("SYNIX_RENDER_GAME_TEXT") == "1")
			{
				using Bitmap bitmap = new(dialog.Width, dialog.Height);
				dialog.DrawToBitmap(bitmap, dialog.ClientRectangle);
				bitmap.Save(Path.Combine(AppContext.BaseDirectory, $"game-text-help-{language}.png"));
				using Bitmap page = new(host.Width, host.Height);
				host.DrawToBitmap(page, host.ClientRectangle);
				page.Save(Path.Combine(AppContext.BaseDirectory, $"game-text-passwords-{language}.png"));
			}
		}
		finally { LocalizationManager.Initialize(LocalizationManager.DefaultLanguageCode); }
	});

	[Fact]
	public void GeneralPageShowsTheSelectedGamesTitleEvidence() => WorkflowUiTest.Run(() =>
	{
		using ServerSettingsGeneralPage page = new();
		page.Initialize(null);
		page.ConfigureForGame(GameDatabase.GetGame("Onset"));
		Button button = Find<Button>(page, "btnNameCharacters");
		Assert.True(button.Enabled);
		Assert.Contains(LocalizationManager.Get("GameText.Status.Documented"), button.Text);
		AssertFits(button);
		Assert.True(button.Bottom < button.Parent!.ClientSize.Height);
		page.ConfigureForGame(GameDatabase.GetGame("ARK: Survival Ascended"));
		Assert.Contains(LocalizationManager.Get("GameText.Status.Unverified"), button.Text);
		page.ConfigureForGame(null);
		Assert.False(button.Enabled);
	});

	private static T Find<T>(Control parent, string name) where T : Control =>
		Assert.IsAssignableFrom<T>(Assert.Single(parent.Controls.Find(name, true)));

	private static void AssertFits(Control control)
	{
		Size measured = TextRenderer.MeasureText(control.Text, control.Font,
			new Size(control.Width - 10, int.MaxValue), TextFormatFlags.WordBreak);
		Assert.True(measured.Height <= control.Height, $"{control.Name} clips its text.");
	}
}
