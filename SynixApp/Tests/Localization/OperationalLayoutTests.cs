using System.Drawing;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Synix_Control_Panel.SynixApp.Design.Controls;
using Synix_Control_Panel.SynixApp.Localization;
using Synix_Control_Panel.SynixApp.UI.Settings;
using Synix_Control_Panel.SynixApp.UI.ServerManagement;
using Synix_Control_Panel.SynixEngine;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class OperationalLayoutTests
{
	public static IEnumerable<object[]> Languages() => LocalizationManager.SupportedLanguages.Select(language => new object[] { language.Code });

	[Theory]
	[MemberData(nameof(Languages))]
	public void SettingsDescriptionsFitAtNarrowAndWideWidths(string language)
	{
		WorkflowUiTest.Run(() =>
		{
			try
			{
				LocalizationManager.Initialize(language);
				using Form host = new() { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000) };
				using GeneralSettingsPage page = new() { Dock = DockStyle.Fill };
				host.Controls.Add(page); host.Show();
				foreach (int width in new[] { 600, 1000 })
				{
					host.ClientSize = new Size(width, 420);
					page.ArrangeLocalizedCards(); Application.DoEvents(); page.PerformLayout();
					foreach (string name in new[] { "lblTitle", "lblDescription", "lblTitleDarkMode", "lblDescriptionDarkMode",
						"lblTitleSteamDownloads", "lblDescriptionSteamDownloads", "lblTitleLanguage", "lblDescriptionLanguage" })
					{
						Label label = (Label)page.Controls.Find(name, true).Single();
						int needed = TextRenderer.MeasureText(label.Text, label.Font,
							new Size(label.Width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height;
						Assert.True(label.Height >= needed, $"{language}/{width}: {name} requires {needed}px, has {label.Height}px at width {label.Width}");
					}
					var cards = page.Controls.Find("generalSettingsCards", true).Single().Controls.Cast<Control>().OrderBy(card => card.Top).ToArray();
					for (int index = 1; index < cards.Length; index++) Assert.True(cards[index].Top >= cards[index - 1].Bottom);
					Assert.True(page.AutoScroll);
				}
				if (Environment.GetEnvironmentVariable("SYNIX_RENDER_LANGUAGES") == "1" && language is "en-US" or "de-DE" or "hi-IN" or "th-TH")
				{
					host.ClientSize = new Size(650, 800); page.ArrangeLocalizedCards(); Application.DoEvents();
					using Bitmap image = new(page.Width, page.Height); page.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
					image.Save(Path.Combine(AppContext.BaseDirectory, "settings-layout-" + language + ".png"));
				}
			}
			finally { LocalizationManager.Initialize("en-US"); }
		});
	}

	[Theory]
	[MemberData(nameof(Languages))]
	public void HistoryIsLocalizedSearchableAndKeepsUserValues(string language)
	{
		WorkflowUiTest.Run(() =>
		{
			try
			{
				LocalizationManager.Initialize(language);
				ServerActivityHistory history = new(null);
				GameServer server = new() { Game = "Minecraft", ServerName = "Save" };
				history.Record(server, "ConfigurationSaved"); history.Record(server, DiscordNotificationEvent.BackupFailed);
				using ServerActivityDialog dialog = new(server, history) { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000) };
				dialog.Size = dialog.MinimumSize; dialog.Show();
				DataGridView grid = (DataGridView)dialog.Controls.Find("historyGrid", true).Single();
				WorkflowUiTest.WaitUntil(() => grid.Rows.Count == 2);
				TextBox search = (TextBox)dialog.Controls.Find("historySearch", true).Single();
				search.Text = LocalizationManager.Get("History.ConfigurationSaved");
				Assert.Single(grid.Rows.Cast<DataGridViewRow>());
				search.Clear();
				((CheckBox)dialog.Controls.Find("historyAttention", true).Single()).Checked = true;
				Assert.Single(grid.Rows.Cast<DataGridViewRow>());
				Assert.Equal(LocalizationManager.Get("Discord.Event.BackupFailed"), grid.Rows[0].Cells[1].Value);
				Assert.Equal("Save", server.ServerName);
				Assert.NotNull(dialog.CancelButton);
				Assert.True(search.TabStop);
			}
			finally { LocalizationManager.Initialize("en-US"); }
		});
	}

	[Fact]
	public void HistorySearchUsesTheInterfaceLanguageWithoutRequiringAccents()
	{
		WorkflowUiTest.Run(() =>
		{
			try
			{
				LocalizationManager.Initialize("fr-FR");
				ServerActivityHistory history = new(null);
				GameServer server = new() { Game = "Minecraft", ServerName = "History search fixture" };
				history.Record(server, "ConfigurationSaved");
				using ServerActivityDialog dialog = new(server, history);
				WorkflowUiTest.Pump(dialog.ReloadAsync());
				TextBox search = (TextBox)dialog.Controls.Find("historySearch", true).Single();
				search.Text = "ENREGISTREE";
				DataGridView grid = (DataGridView)dialog.Controls.Find("historyGrid", true).Single();
				Assert.Single(grid.Rows.Cast<DataGridViewRow>());
				history.Record(server, DiscordNotificationEvent.BackupCompleted);
				Task firstReload = dialog.ReloadAsync();
				WorkflowUiTest.Pump(dialog.ReloadAsync());
				WorkflowUiTest.Pump(firstReload);
				search.Clear();
				Assert.Equal(2, grid.Rows.Count);
			}
			finally { LocalizationManager.Initialize("en-US"); }
		});
	}

	[Theory]
	[MemberData(nameof(Languages))]
	public void DashboardHistoryActionsAndBackupColumnFitWithoutStartingServices(string language)
	{
		WorkflowUiTest.Run(() =>
		{
			LicenseContext originalContext = LicenseManager.CurrentContext;
			try
			{
				LocalizationManager.Initialize(language);
				LicenseManager.CurrentContext = new DesigntimeLicenseContext();
				using MainGUI dashboard = new();
				dashboard.InitializeActivityHistory(monitorBackups: false);
				WorkflowUiTest.Invoke(dashboard, "UpdateDashboardAccessibleNames");
				DataGridView grid = (DataGridView)dashboard.Controls.Find("dataGridView1", true).Single();
				_ = grid.Handle; grid.PerformLayout();
				Assert.InRange(grid.Columns["BackupHealth"]!.Width, 135, grid.Width / 3);
				Assert.True(grid.AllowUserToResizeColumns);
				Assert.Equal(DataGridViewColumnHeadersHeightSizeMode.AutoSize, grid.ColumnHeadersHeightSizeMode);
				foreach (string name in new[] { "btnHelp", "btnSettings", "btnGithub", "btnDiscord", "btnMinimize", "btnClose" })
					Assert.False(string.IsNullOrWhiteSpace(dashboard.Controls.Find(name, true).Single().AccessibleName));
				Control card = dashboard.Controls.Find("activityCard", true).Single();
				using Form host = new() { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-32000, -32000) };
				card.Dock = DockStyle.Fill; host.Controls.Add(card); host.ClientSize = new Size(340, 330); host.Show();
				Application.DoEvents();
				Control title = card.Controls.Find("lblActivityTitle", true).Single();
				Control history = card.Controls.Find("btnServerHistory", true).Single();
				Control clear = card.Controls.Find("btnClearLog", true).Single();
				Assert.Equal(LocalizationManager.Get("Text.9CC3A043B6A91D450BC3"), clear.Text);
				Control log = card.Controls.Find("rtbLog", true).Single();
				Rectangle Bounds(Control control) => new(host.PointToClient(control.PointToScreen(Point.Empty)), control.Size);
				Assert.True(Bounds(title).Bottom <= Bounds(history).Top);
				Assert.False(Bounds(history).IntersectsWith(Bounds(clear)));
				Assert.True(Bounds(history).Bottom <= Bounds(log).Top);
				Assert.True(Bounds(clear).Bottom <= Bounds(log).Top);
				Assert.True(log.Height >= 100);
				Assert.False(dashboard.Visible);
				if (Environment.GetEnvironmentVariable("SYNIX_RENDER_LANGUAGES") == "1" && language is "en-US" or "de-DE")
				{
					using Bitmap image = new(card.Width, card.Height); card.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
					image.Save(Path.Combine(AppContext.BaseDirectory, "activity-layout-" + language + ".png"));
				}
			}
			finally { LicenseManager.CurrentContext = originalContext; LocalizationManager.Initialize("en-US"); }
		});
	}

	[Fact]
	public void DropDownMeasuresLongTranslatedItemsAndActionButtonsAcceptKeyboardFocus()
	{
		WorkflowUiTest.Run(() =>
		{
			using ModernSettingsComboBox combo = new() { Width = 90, Font = new Font("Segoe UI", 16) };
			combo.Items.Add("Sicherungskopie wiederherstellen — ภาษาไทย 日本語");
			combo.FitDropDownToItems();
			Assert.True(combo.DropDownWidth > combo.Width);
			Assert.True(combo.ItemHeight >= TextRenderer.MeasureText(combo.Items[0]?.ToString(), combo.Font).Height);
			using SynixButton action = new(); using ModernSettingsButton settingsAction = new();
			Assert.True(action.TabStop); Assert.True(settingsAction.TabStop);
		});
	}
}
