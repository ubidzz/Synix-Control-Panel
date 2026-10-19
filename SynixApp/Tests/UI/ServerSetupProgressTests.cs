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
using System.Drawing;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.Design.Controls;
using Synix_Control_Panel.SynixApp.Localization;
using Synix_Control_Panel.SynixEngine;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class ServerSetupProgressTests
{
	[Theory]
	[InlineData(false, false, false, "Attention,Waiting,Waiting,Waiting")]
	[InlineData(true, false, false, "Complete,Attention,Waiting,Waiting")]
	[InlineData(true, true, false, "Complete,Complete,Next,Waiting")]
	[InlineData(true, true, true, "Complete,Complete,Complete,Ready")]
	[InlineData(true, false, true, "Complete,Attention,Waiting,Waiting")]
	[InlineData(false, true, true, "Attention,Waiting,Waiting,Waiting")]
	public void CheckpointsFollowValidationNotVisitedTabs(bool details, bool ready, bool reviewed, string expected)
	{
		RunOnSta(() =>
		{
			using ServerSetupProgressStrip strip = new();
			strip.UpdateState(details, ready, reviewed);
			ServerSetupProgressStrip.StepButton[] buttons = Enumerable.Range(1, 4)
				.Select(index => Find<ServerSetupProgressStrip.StepButton>(strip, $"btnSetupStep{index}")).ToArray();
			Assert.Equal(expected, string.Join(",", buttons.Select(button => button.State)));
			Assert.True(buttons[0].Enabled);
			Assert.Equal(details, buttons[1].Enabled);
			Assert.Equal(details && ready, buttons[2].Enabled);
			Assert.Equal(details && ready && reviewed, buttons[3].Enabled);
		});
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void ProgressFitsWithoutTheOldBannerGapOrCoveringPagesOrSidebar(bool minimumSize)
	{
		RunOnSta(() =>
		{
			using ServerSettingsGUI setup = new();
			if (minimumSize) setup.Size = setup.MinimumSize;
			ShowOffscreen(setup);
			Control strip = Find<ServerSetupProgressStrip>(setup, "setupProgress");
			Control description = Find<Label>(setup, "lblPageDescription");
			Control pages = Find<Panel>(setup, "pnlPageHost");
			Assert.True(description.Bottom <= strip.Top);
			Assert.Empty(setup.Controls.Find("lblTemplateBehavior", true));
			Assert.InRange(pages.Top - strip.Bottom, 1, (int)Math.Ceiling(20 * setup.DeviceDpi / 96d));
			Assert.True(pages.Height >= 300);
			Assert.True(pages.Bottom <= pages.Parent!.ClientSize.Height);
			Assert.True(strip.Right <= strip.Parent!.ClientSize.Width);
			Assert.Empty(setup.Controls.Find("lblSetupCompletion", true));
			Assert.True(Find<ModernSettingsNavButton>(setup, "btnNavInstall").Bottom < Find<Panel>(setup, "pnlSidebarStatus").Top);
		});
	}

	[Fact]
	public void RequiredCheckpointFindsEcoTokenAndReviewNeverExposesSecretsOrSaves()
	{
		RunOnSta(() =>
		{
			using ServerSettingsGUI setup = CreateEcoSetup();
			ShowOffscreen(setup);
			Step(setup, 2).PerformClick();
			Assert.True(Find<ServerSettingsSecurityPage>(setup, "pnlPageSecurity").Visible);
			Assert.False(Find<Button>(setup, "btnSave").Enabled);
			Assert.Equal(ServerSetupProgressStrip.StepState.Attention, Step(setup, 2).State);

			Find<TextBox>(setup, "txtAuthenticationToken").Text = "eco-private-token_123.test";
			Find<TextBox>(setup, "txtPassword").Text = "private-password_123";
			Navigate(setup, 2);
			Assert.True(Find<Button>(setup, "btnSave").Enabled);
			Assert.True(Find<ServerSettingsReviewPage>(setup, "pnlPageReview").Visible);
			Assert.Equal(ServerSetupProgressStrip.StepState.Current, Step(setup, 3).State);
			DataGridView grid = Find<DataGridView>(setup, "gridSummary");
			Assert.True(grid.ReadOnly);
			string[] values = grid.Rows.Cast<DataGridViewRow>().SelectMany(row => row.Cells.Cast<DataGridViewCell>())
				.Select(cell => cell.Value?.ToString() ?? string.Empty).ToArray();
			Assert.Contains("Eco Setup Progress Test", values);
			Assert.DoesNotContain("eco-private-token_123.test", values);
			Assert.DoesNotContain("private-password_123", values);

			Step(setup, 4).PerformClick();
			Assert.True(Find<Button>(setup, "btnSave").Focused);
			Assert.Null(setup.NewServer);
			Assert.Equal(DialogResult.None, setup.DialogResult);

			Find<TextBox>(setup, "txtAuthenticationToken").Text = string.Empty;
			Assert.False(Find<Button>(setup, "btnSave").Enabled);
			Assert.False(Step(setup, 4).Enabled);
			Assert.Equal(ServerSetupProgressStrip.StepState.Waiting, Step(setup, 3).State);
			// A click/Enter before the debounce completes must still respect the gate.
			typeof(ServerSettingsGUI).GetMethod("btnSave_Click", BindingFlags.Instance | BindingFlags.NonPublic)!
				.Invoke(setup, [Find<Button>(setup, "btnSave"), EventArgs.Empty]);
			Assert.False(Find<Button>(setup, "btnSave").Enabled);
			Assert.True(Find<ServerSettingsSecurityPage>(setup, "pnlPageSecurity").Visible);
			Assert.Null(setup.NewServer);
			Assert.Equal(DialogResult.None, setup.DialogResult);
		});
	}

	[Fact]
	public void EditingAfterReviewResetsTheCheckpointAndRefreshesTheSummary()
	{
		RunOnSta(() =>
		{
			using ServerSettingsGUI setup = CreateEcoSetup();
			ShowOffscreen(setup);
			Find<TextBox>(setup, "txtAuthenticationToken").Text = "eco-user-token_123.test";
			Navigate(setup, 2);
			Find<TextBox>(setup, "txtName").Text = "Updated server name";
			Assert.False(Find<Button>(setup, "btnSave").Enabled);
			Assert.False(Step(setup, 4).Enabled);
			Assert.Equal(ServerSetupProgressStrip.StepState.Waiting, Step(setup, 3).State);
			Navigate(setup, 0);
			Assert.Equal(ServerSetupProgressStrip.StepState.Next, Step(setup, 3).State);
			Assert.True(Find<Button>(setup, "btnSave").Enabled);
			Assert.Equal(LocalizationManager.Get("ServerSetup.Button.NextReview"), Find<Button>(setup, "btnSave").Text);
			Assert.Equal(LocalizationManager.Get("ServerSetup.Validation.ReviewRequired"),
				Find<Label>(setup, "lblFooterStatus").Text);
			Navigate(setup, 2);
			Assert.True(Find<Button>(setup, "btnSave").Enabled);
			Assert.Contains(Find<DataGridView>(setup, "gridSummary").Rows.Cast<DataGridViewRow>(),
				row => Equals(row.Cells[1].Value, "Updated server name"));
		});
	}

	[Theory]
	[InlineData("fr-FR", "Récapitulatif", "VÉRIFICATION")]
	[InlineData("de-DE", "Überprüfen", "IN PRÜFUNG")]
	[InlineData("es-ES", "Revisar", "REVISANDO")]
	[InlineData("zh-Hans", "检查", "检查中")]
	[InlineData("zh-Hant", "檢查", "檢查中")]
	[InlineData("ja-JP", "確認", "確認中")]
	[InlineData("ko-KR", "검토", "검토 중")]
	[InlineData("it-IT", "Verifica", "Verifica in corso")]
	[InlineData("pt-BR", "Revisar", "Revisando")]
	[InlineData("pl-PL", "Przegląd", "Przeglądanie")]
	[InlineData("ru-RU", "Проверка", "Проверка")]
	[InlineData("tr-TR", "Gözden geçir", "Gözden geçiriliyor")]
	[InlineData("nl-NL", "Controleren", "CONTROLEREN")]
	[InlineData("sv-SE", "Granska", "GRANSKAR")]
	[InlineData("cs-CZ", "Kontrola", "KONTROLA")]
	[InlineData("uk-UA", "Перевірка", "ПЕРЕВІРКА")]
	[InlineData("id-ID", "Tinjau", "MENINJAU")]
	[InlineData("vi-VN", "Xem lại", "ĐANG XEM LẠI")]
	[InlineData("da-DK", "Gennemse", "GENNEMSER")]
	[InlineData("fi-FI", "Tarkista", "TARKISTUS")]
	[InlineData("nb-NO", "Gjennomgå", "GJENNOMGANG")]
	[InlineData("el-GR", "Έλεγχος", "ΕΛΕΓΧΟΣ")]
	[InlineData("hu-HU", "Áttekintés", "ÁTTEKINTÉS")]
	[InlineData("ro-RO", "Verificare", "ÎN VERIFICARE")]
	[InlineData("hi-IN", "समीक्षा", "समीक्षा जारी")]
	[InlineData("th-TH", "ตรวจทาน", "กำลังตรวจทาน")]
	public void OpenSetupUpdatesItsGuideAndReviewLanguageWithoutTranslatingValues(string language, string title, string current)
	{
		RunOnSta(() =>
		{
			using ServerSettingsGUI setup = CreateEcoSetup();
			ShowOffscreen(setup);
			Find<TextBox>(setup, "txtName").Text = "Save";
			Find<TextBox>(setup, "txtAuthenticationToken").Text = "eco-user-token_123.test";
			Navigate(setup, 2);
			LocalizationManager.SetLanguage(language);
			LocalizationManager.Apply(setup);
			Assert.Equal(title, Step(setup, 3).Text);
			Assert.Equal(current, Step(setup, 3).StatusText);
			DataGridView grid = Find<DataGridView>(setup, "gridSummary");
			Assert.Equal(LocalizationManager.Get("ServerSetup.Review.ServerName"), grid.Rows[0].Cells[0].Value);
			Assert.Equal("Save", grid.Rows[0].Cells[1].Value);
			Assert.Equal("Eco", grid.Rows[1].Cells[1].Value);
			Assert.Equal(LocalizationManager.Get("ServerSetup.Button.SaveSettings"), Find<Button>(setup, "btnSave").Text);
			Find<Button>(setup, "btnNavGeneral").PerformClick();
			Assert.Equal(LocalizationManager.Get("ServerSetup.Button.NextReview"), Find<Button>(setup, "btnSave").Text);
		});
	}

	[Theory]
	[InlineData("nl-NL")]
	[InlineData("sv-SE")]
	[InlineData("cs-CZ")]
	[InlineData("uk-UA")]
	[InlineData("id-ID")]
	[InlineData("vi-VN")]
	[InlineData("da-DK")]
	[InlineData("fi-FI")]
	[InlineData("nb-NO")]
	[InlineData("el-GR")]
	[InlineData("hu-HU")]
	[InlineData("ro-RO")]
	[InlineData("hi-IN")]
	[InlineData("th-TH")]
	public void AdditionalLanguageNavigationFitsWithoutEllipsis(string language)
	{
		RunOnSta(() =>
		{
			LocalizationManager.Initialize(language);
			using ServerSettingsGUI setup = CreateEcoSetup();
			setup.Size = setup.MinimumSize;
			ShowOffscreen(setup);
			foreach (string name in new[]
			{
				"btnNavGeneral", "btnNavSecurity", "btnNavWorld", "btnNavNetwork",
				"btnNavAutomation", "btnNavDiscord", "btnNavInstall"
			})
			{
				var button = Find<ModernSettingsNavButton>(setup, name);
				// Match the icon and text insets used by ModernSettingsNavButton.OnPaint.
				int width = TextRenderer.MeasureText(button.Text, button.Font, Size.Empty,
					TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix).Width;
				Assert.True(width <= button.Width - 66,
					$"Navigation label clips: {language} / {name} / {button.Text}");
			}
		});
	}

	private static ServerSettingsGUI CreateEcoSetup()
	{
		(int gamePort, int queryPort) = ServerSetupTestPorts.FindAvailablePair();
		return new ServerSettingsGUI(new GameServer
		{
			Game = "Eco",
			ServerName = "Eco Setup Progress Test",
			InstallPath = Path.GetTempPath(),
			Port = gamePort,
			QueryPort = queryPort
		});
	}

	[Theory]
	[InlineData(false, "txtPassword")]
	[InlineData(true, "txtPassword")]
	[InlineData(false, "txtAdminPassword")]
	[InlineData(true, "txtAdminPassword")]
	public void PasswordTypingWithPausesKeepsFocusUntilNextIsClicked(bool editMode, string inputName)
	{
		RunOnSta(() =>
		{
			(int gamePort, int queryPort) = ServerSetupTestPorts.FindAvailablePair();
			using ServerSettingsGUI setup = new(editMode ? new GameServer
			{
				Game = "Enshrouded", ServerName = "Password review test",
				InstallPath = Path.GetTempPath(), Port = gamePort, QueryPort = queryPort
			} : null);
			ShowOffscreen(setup);
			if (!editMode)
			{
				ComboBox games = Find<ComboBox>(setup, "cmbGame");
				games.SelectedIndex = games.FindStringExact("Enshrouded");
				Find<TextBox>(setup, "txtName").Text = "Password review test";
				Find<ModernSettingsNumericUpDown>(setup, "numPort").Value = gamePort;
				Find<ModernSettingsNumericUpDown>(setup, "numQueryPort").Value = queryPort;
				Find<ModernSettingsToggle>(setup, "chkDefaultPath").Checked = true;
			}
			if (inputName == "txtPassword")
				Find<TextBox>(setup, "txtAdminPassword").Text = "existing-admin-password";
			Navigate(setup, 0);
			Find<Button>(setup, "btnNavSecurity").PerformClick();
			TextBox input = Find<TextBox>(setup, inputName);
			input.Focus();
			Button action = Find<Button>(setup, "btnSave");
			ServerSettingsReviewPage review = Find<ServerSettingsReviewPage>(setup, "pnlPageReview");
			string expected = string.Empty;
			foreach (string part in new[] { "a", "1", "-a-longer-password-" + new string('x', 64) })
			{
				input.SelectedText = part;
				expected += part;
				WaitForValidation(setup);
				Assert.False(review.Visible);
				Assert.True(input.Focused);
				Assert.Equal(expected, input.Text);
				Assert.True(action.Enabled, Find<Label>(setup, "lblFooterStatus").Text);
				Assert.Equal(LocalizationManager.Get("ServerSetup.Button.NextReview"), action.Text);
				Assert.False(Step(setup, 4).Enabled);
				Assert.Null(setup.NewServer);
			}

			action.PerformClick();
			Assert.True(review.Visible);
			Assert.Equal(LocalizationManager.Get("ServerSetup.Button.SaveSettings"), action.Text);
			Assert.True(action.Enabled);
			Assert.True(Step(setup, 4).Enabled);
			Assert.Null(setup.NewServer);
			Assert.Equal(DialogResult.None, setup.DialogResult);

			Find<Button>(setup, "btnNavSecurity").PerformClick();
			Assert.Equal(LocalizationManager.Get("ServerSetup.Button.NextReview"), action.Text);
			input.Focus();
			input.AppendText("-edited-after-review");
			WaitForValidation(setup);
			Assert.False(review.Visible);
			Assert.True(input.Focused);
			Assert.False(Step(setup, 4).Enabled);
			Assert.Equal(expected + "-edited-after-review", input.Text);
			action.PerformClick();
			Assert.True(review.Visible);
			Assert.Null(setup.NewServer);
		});
	}

	[Theory]
	[InlineData(false, "Eco", "txtAuthenticationToken", "eco-user-token_123.test")]
	[InlineData(true, "Eco", "txtAuthenticationToken", "eco-user-token_123.test")]
	[InlineData(false, "Empyrion - Galactic Survival", "txtWorldSeed", "1011345")]
	[InlineData(true, "Empyrion - Galactic Survival", "txtWorldSeed", "1011345")]
	public void LastRequiredSettingEnablesNextWithoutOpeningReviewOrSaving(
		bool editMode, string game, string lastInput, string validValue)
	{
		RunOnSta(() =>
		{
			(int gamePort, int queryPort) = ServerSetupTestPorts.FindAvailablePair();
			using ServerSettingsGUI setup = new(editMode ? new GameServer
			{
				Game = game, ServerName = "Review transition test", WorldSeed = "invalid",
				InstallPath = Path.GetTempPath(), Port = gamePort, QueryPort = queryPort
			} : null);
			ShowOffscreen(setup);
			if (!editMode)
			{
				ComboBox games = Find<ComboBox>(setup, "cmbGame");
				games.SelectedIndex = games.FindStringExact(game);
				Find<TextBox>(setup, "txtName").Text = "Review transition test";
				Find<ModernSettingsNumericUpDown>(setup, "numPort").Value = gamePort;
				Find<ModernSettingsNumericUpDown>(setup, "numQueryPort").Value = queryPort;
				Find<ModernSettingsToggle>(setup, "chkDefaultPath").Checked = true;
				if (lastInput == "txtWorldSeed")
					Find<TextBox>(setup, lastInput).Text = "invalid";
			}

			Navigate(setup, 1);
			Button save = Find<Button>(setup, "btnSave");
			ServerSettingsReviewPage review = Find<ServerSettingsReviewPage>(setup, "pnlPageReview");
			Assert.False(save.Enabled);
			Assert.False(review.Visible);
			Assert.Equal(ServerSetupProgressStrip.StepState.Attention, Step(setup, 2).State);
			TextBox input = Find<TextBox>(setup, lastInput);
			input.Focus();
			input.Text = validValue;
			Assert.False(save.Enabled);
			Assert.False(Step(setup, 4).Enabled);
			WaitForValidation(setup);
			Assert.True(save.Enabled, Find<Label>(setup, "lblFooterStatus").Text);
			Assert.Equal(LocalizationManager.Get("ServerSetup.Button.NextReview"), save.Text);
			Assert.False(review.Visible);
			Assert.True(input.Focused);
			Assert.Equal(ServerSetupProgressStrip.StepState.Complete, Step(setup, 2).State);
			Assert.Equal(ServerSetupProgressStrip.StepState.Next, Step(setup, 3).State);
			Assert.Equal(ServerSetupProgressStrip.StepState.Waiting, Step(setup, 4).State);
			Assert.Null(setup.NewServer);
			Assert.Equal(DialogResult.None, setup.DialogResult);

			save.PerformClick();
			Assert.True(review.Visible);
			Assert.True(save.Enabled);
			Assert.Equal(LocalizationManager.Get("ServerSetup.Button.SaveSettings"), save.Text);
			Assert.Equal("Review transition test", Find<DataGridView>(setup, "gridSummary").Rows[0].Cells[1].Value);
			Assert.Equal(ServerSetupProgressStrip.StepState.Current, Step(setup, 3).State);
			Assert.Equal(ServerSetupProgressStrip.StepState.Ready, Step(setup, 4).State);
			Assert.False(save.Focused);
			Assert.True(Find<DataGridView>(setup, "gridSummary").ContainsFocus);
			Assert.Null(setup.NewServer);
			Assert.Equal(DialogResult.None, setup.DialogResult);
		});
	}

	[Fact]
	public void ValidExistingSettingsStillRequireReviewAndCannotSkipToSave()
	{
		RunOnSta(() =>
		{
			(int gamePort, int queryPort) = ServerSetupTestPorts.FindAvailablePair();
			using ServerSettingsGUI setup = new(new GameServer
			{
				Game = "Empyrion - Galactic Survival", ServerName = "Saved server review test",
				WorldSeed = "1011345", InstallPath = Path.GetTempPath(), Port = gamePort, QueryPort = queryPort
			});
			ShowOffscreen(setup);
			Assert.True(Find<Button>(setup, "btnSave").Enabled);
			Assert.Equal(LocalizationManager.Get("ServerSetup.Button.NextReview"), Find<Button>(setup, "btnSave").Text);
			Assert.False(Step(setup, 4).Enabled);
			Assert.True(Step(setup, 3).Enabled, Find<Label>(setup, "lblFooterStatus").Text);
			Assert.Equal(ServerSetupProgressStrip.StepState.Next, Step(setup, 3).State);
			Step(setup, 4).PerformClick();
			Assert.False(Find<ServerSettingsReviewPage>(setup, "pnlPageReview").Visible);
			Assert.Null(setup.NewServer);

			Step(setup, 3).PerformClick();
			Assert.True(Find<ServerSettingsReviewPage>(setup, "pnlPageReview").Visible);
			Assert.True(Find<Button>(setup, "btnSave").Enabled);
			Assert.True(Step(setup, 4).Enabled);
			Navigate(setup, 0);
			Assert.Equal(ServerSetupProgressStrip.StepState.Complete, Step(setup, 3).State);
			Assert.True(Find<Button>(setup, "btnSave").Enabled);
			Assert.Equal(LocalizationManager.Get("ServerSetup.Button.NextReview"), Find<Button>(setup, "btnSave").Text);
			Find<Button>(setup, "btnSave").PerformClick();
			Assert.True(Find<ServerSettingsReviewPage>(setup, "pnlPageReview").Visible);
			Assert.Equal(LocalizationManager.Get("ServerSetup.Button.SaveSettings"), Find<Button>(setup, "btnSave").Text);
			Assert.Null(setup.NewServer);
			Assert.Equal(DialogResult.None, setup.DialogResult);
		});
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void QueuedSaveDuringAValidEditShowsReviewWithoutSaving(bool previouslyReviewed)
	{
		RunOnSta(() =>
		{
			using ServerSettingsGUI setup = CreateEcoSetup();
			ShowOffscreen(setup);
			Find<TextBox>(setup, "txtAuthenticationToken").Text = "eco-user-token_123.test";
			if (previouslyReviewed)
			{
				Navigate(setup, 2);
				Find<Button>(setup, "btnNavGeneral").PerformClick();
			}
			Find<TextBox>(setup, "txtName").Text = "Fresh review required";
			Assert.False(Find<Button>(setup, "btnSave").Enabled);
			typeof(ServerSettingsGUI).GetMethod("btnSave_Click", BindingFlags.Instance | BindingFlags.NonPublic)!
				.Invoke(setup, [Find<Button>(setup, "btnSave"), EventArgs.Empty]);
			Assert.True(Find<ServerSettingsReviewPage>(setup, "pnlPageReview").Visible);
			Assert.Contains(Find<DataGridView>(setup, "gridSummary").Rows.Cast<DataGridViewRow>(),
				row => Equals(row.Cells[1].Value, "Fresh review required"));
			Assert.True(Find<Button>(setup, "btnSave").Enabled);
			Assert.Null(setup.NewServer);
			Assert.Equal(DialogResult.None, setup.DialogResult);
		});
	}

	[Fact]
	public void CorrectedSettingsStayOnTheirPageUntilNextIsClicked()
	{
		RunOnSta(() =>
		{
			using ServerSettingsGUI setup = CreateEcoSetup();
			ShowOffscreen(setup);
			TextBox token = Find<TextBox>(setup, "txtAuthenticationToken");
			token.Text = "eco-user-token_123.test";
			Navigate(setup, 2);
			Find<Button>(setup, "btnNavSecurity").PerformClick();
			token.Text = string.Empty;
			Button save = Find<Button>(setup, "btnSave");
			Assert.False(save.Enabled);
			WorkflowUiTest.WaitUntil(() => Step(setup, 2).State == ServerSetupProgressStrip.StepState.Attention);
			Assert.True(Find<ServerSettingsSecurityPage>(setup, "pnlPageSecurity").Visible);
			Assert.False(save.Enabled);
			Assert.False(Step(setup, 4).Enabled);
			token.Text = "replacement-token";
			WorkflowUiTest.WaitUntil(() => save.Enabled);
			Assert.True(Find<ServerSettingsSecurityPage>(setup, "pnlPageSecurity").Visible);
			Assert.False(Find<ServerSettingsReviewPage>(setup, "pnlPageReview").Visible);
			Assert.Equal(LocalizationManager.Get("ServerSetup.Button.NextReview"), save.Text);
			Assert.False(Step(setup, 4).Enabled);
			save.PerformClick();
			Assert.True(Find<ServerSettingsReviewPage>(setup, "pnlPageReview").Visible);
			Assert.Null(setup.NewServer);
		});
	}

	[Theory]
	[InlineData("en-US", true, false)]
	[InlineData("fr-FR", true, true)]
	[InlineData("de-DE", true, true)]
	[InlineData("es-ES", true, true)]
	[InlineData("en-US", false, true)]
	[InlineData("zh-Hans", true, true)]
	[InlineData("zh-Hant", false, true)]
	[InlineData("ja-JP", true, true)]
	[InlineData("ko-KR", true, true)]
	[InlineData("it-IT", true, true)]
	[InlineData("pt-BR", true, true)]
	[InlineData("pl-PL", true, true)]
	[InlineData("ru-RU", true, true)]
	[InlineData("tr-TR", true, true)]
	[InlineData("nl-NL", true, true)]
	[InlineData("nl-NL", false, true)]
	[InlineData("sv-SE", true, true)]
	[InlineData("sv-SE", false, true)]
	[InlineData("cs-CZ", true, true)]
	[InlineData("cs-CZ", false, true)]
	[InlineData("uk-UA", true, true)]
	[InlineData("uk-UA", false, true)]
	[InlineData("id-ID", true, true)]
	[InlineData("id-ID", false, true)]
	[InlineData("vi-VN", true, true)]
	[InlineData("vi-VN", false, true)]
	[InlineData("da-DK", true, true)]
	[InlineData("da-DK", false, true)]
	[InlineData("fi-FI", true, true)]
	[InlineData("fi-FI", false, true)]
	[InlineData("nb-NO", true, true)]
	[InlineData("nb-NO", false, true)]
	[InlineData("el-GR", true, true)]
	[InlineData("el-GR", false, true)]
	[InlineData("hu-HU", true, true)]
	[InlineData("hu-HU", false, true)]
	[InlineData("ro-RO", true, true)]
	[InlineData("ro-RO", false, true)]
	[InlineData("hi-IN", true, true)]
	[InlineData("hi-IN", false, true)]
	[InlineData("th-TH", true, true)]
	[InlineData("th-TH", false, true)]
	public void ReviewAndCheckpointTextFitInBothThemesAndAllLanguages(string language, bool darkMode, bool minimumSize)
	{
		RunOnSta(() =>
		{
			bool previousTheme = ThemeManager.IsDarkMode;
			try
			{
				ThemeManager.Initialize(darkMode);
				LocalizationManager.Initialize(language);
				using ServerSettingsGUI setup = CreateEcoSetup();
				if (minimumSize)
					setup.Size = setup.MinimumSize;
				ShowOffscreen(setup);
				Find<TextBox>(setup, "txtAuthenticationToken").Text = "eco-user-token_123.test";
				Navigate(setup, 2);
				Assert.True(Find<ServerSettingsReviewPage>(setup, "pnlPageReview").Visible);
				DataGridView grid = Find<DataGridView>(setup, "gridSummary");
				Assert.Equal(SettingsPalette.Input, grid.ColumnHeadersDefaultCellStyle.BackColor);
				Assert.Equal(SettingsPalette.SecondaryText, grid.ColumnHeadersDefaultCellStyle.ForeColor);
				Assert.Equal(SettingsPalette.Selection, grid.DefaultCellStyle.SelectionBackColor);
				Assert.Equal(LocalizationManager.Get("ServerSetup.Button.SaveChanges"), Step(setup, 4).Text);
				foreach (int index in Enumerable.Range(1, 4))
				{
					var button = Step(setup, index);
					Assert.True(TextRenderer.MeasureText(button.Text, button.Font).Width <= button.ClientSize.Width - 8);
					Assert.True(TextRenderer.MeasureText(button.StatusText, button.Font).Width <= button.ClientSize.Width - 8);
				}
				Label notice = Find<Label>(setup, "lblReviewNotice");
				Size measured = TextRenderer.MeasureText(notice.Text, notice.Font,
					new Size(notice.Width - notice.Padding.Horizontal, int.MaxValue), TextFormatFlags.WordBreak);
				Assert.True(measured.Height <= notice.Height - notice.Padding.Vertical);
				Button action = Find<Button>(setup, "btnSave");
				Button cancel = Find<Button>(setup, "btnCancel");
				Label footer = Find<Label>(setup, "lblFooterStatus");
				Assert.Equal(LocalizationManager.Get("ServerSetup.Button.SaveSettings"), action.Text);
				Assert.True(TextRenderer.MeasureText(action.Text, action.Font).Width <= action.Width - action.Padding.Horizontal);
				Assert.True(footer.Right < cancel.Left);
				Assert.True(cancel.Right < action.Left);
				Assert.True(action.Right <= action.Parent!.ClientSize.Width);
				using Bitmap rendered = new(setup.Width, setup.Height);
				setup.DrawToBitmap(rendered, setup.ClientRectangle);
				// Optional artifacts stay in the test output folder, never in a live server.
				if (Environment.GetEnvironmentVariable("SYNIX_RENDER_SETUP_PROGRESS") == "1")
					rendered.Save(Path.Combine(AppContext.BaseDirectory, $"setup-review-{language}-{darkMode}.png"));
				Find<Button>(setup, "btnNavGeneral").PerformClick();
				Assert.Equal(LocalizationManager.Get("ServerSetup.Button.NextReview"), action.Text);
				Assert.True(TextRenderer.MeasureText(action.Text, action.Font).Width <= action.Width - action.Padding.Horizontal);
				Size footerText = TextRenderer.MeasureText(footer.Text, footer.Font,
					new Size(footer.Width, int.MaxValue), TextFormatFlags.WordBreak);
				Assert.True(footerText.Height <= footer.Height);
				if (Environment.GetEnvironmentVariable("SYNIX_RENDER_SETUP_PROGRESS") == "1")
				{
					setup.DrawToBitmap(rendered, setup.ClientRectangle);
					rendered.Save(Path.Combine(AppContext.BaseDirectory, $"setup-next-{language}-{darkMode}.png"));
				}
			}
			finally { ThemeManager.Initialize(previousTheme); }
		});
	}

	private static T Find<T>(Control parent, string name) where T : Control =>
		Assert.IsAssignableFrom<T>(parent.Controls.Find(name, true).Single());
	private static ServerSetupProgressStrip.StepButton Step(Control setup, int index) =>
		Find<ServerSetupProgressStrip.StepButton>(setup, $"btnSetupStep{index}");
	private static void Navigate(ServerSettingsGUI setup, int step) =>
		typeof(ServerSettingsGUI).GetMethod("NavigateSetupStep", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(setup, [step]);
	private static void WaitForValidation(ServerSettingsGUI setup)
	{
		var timer = (System.Windows.Forms.Timer)typeof(ServerSettingsGUI)
			.GetField("debounceTimer", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(setup)!;
		WorkflowUiTest.WaitUntil(() => !timer.Enabled);
	}

	private static void ShowOffscreen(Form form)
	{
		form.StartPosition = FormStartPosition.Manual;
		form.Location = new Point(-32000, -32000);
		form.ShowInTaskbar = false;
		form.Show();
		Application.DoEvents();
	}

	private static void RunOnSta(Action action)
	{
		Exception? failure = null;
		Thread thread = new(() =>
		{
			try { LocalizationManager.Initialize("en-US"); action(); }
			catch (Exception exception) { failure = exception; }
			finally { LocalizationManager.Initialize(LocalizationManager.DefaultLanguageCode); }
		}) { IsBackground = true };
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		Assert.True(thread.Join(TimeSpan.FromSeconds(30)), "Setup progress check did not finish.");
		if (failure is not null)
			ExceptionDispatchInfo.Capture(failure).Throw();
	}
}
