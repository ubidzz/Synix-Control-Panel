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
using System.Text.Json.Nodes;
using System.Windows.Forms;
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Localization;
using Synix_Control_Panel.SynixApp.UI.ServerManagement;
using Synix_Control_Panel.SynixEngine;
using Synix_Control_Panel.SynixEngine.ModManagement;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class GameModSupportTests
{
	[Fact]
	public void EveryDatabaseGameHasAnExplicitEvidenceReview()
	{
		string[] expected = GameDatabase.GetGames.Select(game => game.DefinitionId).Order().ToArray();
		string[] actual = GameModSupportCatalog.Entries.Select(entry => entry.GameId).Order().ToArray();
		Assert.Equal(expected, actual); // A newly added game must get an audit entry too.
		Assert.Contains(GameModSupportCatalog.Entries, entry => entry.Status == GameModEvidence.Documented);
		Assert.Contains(GameModSupportCatalog.Entries, entry => entry.Status == GameModEvidence.NeedsVerification);
		foreach (var entry in GameModSupportCatalog.Entries)
		{
			Assert.NotEqual(default, entry.ReviewedOn);
			Assert.False(string.IsNullOrWhiteSpace(entry.Notes));
			Assert.All(entry.Sources, source => Assert.True(GameModSupportCatalog.IsSafeSource(source)));
			if (entry.Status == GameModEvidence.Documented)
			{
				Assert.NotEmpty(entry.Methods);
				Assert.NotEmpty(entry.Sources);
			}
			else Assert.Empty(entry.Methods);
		}
	}

	[Theory]
	[InlineData("7 Days to Die")]
	[InlineData("ARK: Survival Evolved")]
	[InlineData("ARK: Survival Ascended")]
	[InlineData("BeamMP")]
	[InlineData("Conan Exiles")]
	[InlineData("Minecraft Java")]
	[InlineData("Palworld")]
	[InlineData("SCP: Secret Laboratory")]
	[InlineData("Valheim")]
	[InlineData("Terraria")]
	[InlineData("Don't Starve Together")]
	[InlineData("rFactor 2")]
	public void MissingInstallerDoesNotHideKnownServerMethods(string game) =>
		Assert.Equal(GameModEvidence.Documented, GameModSupportCatalog.ForGame(game)?.Status);

	[Fact]
	public void UnknownIsNotFalseAndAliasesShareDatabaseIdentity()
	{
		Assert.Null(GameModSupportCatalog.ForGame("Not in this database"));
		Assert.Equal(LocalizationManager.Get("ModSupport.NeedsVerification"), GameModSupportCatalog.StatusText("Not in this database"));
		foreach (var game in GameDatabase.GetGames)
			foreach (string alias in game.Aliases)
				Assert.Same(GameModSupportCatalog.ForGame(game.Game), GameModSupportCatalog.ForGame(alias));
		Assert.Equal(ModSystemCatalog.GetProfiles("Minecraft").Select(p => p.Id),
			ModSystemCatalog.GetProfiles("Minecraft Java").Select(p => p.Id));
		Assert.Empty(ModSystemCatalog.GetProfiles(new GameServer { Game = "Minecraft Bedrock" }));
	}

	[Fact]
	public void AuditSchemaRejectsUnsupportedClaimsAndUntrustedLinks()
	{
		JsonObject entry = new()
		{
			["gameId"] = "minecraft", ["status"] = "Documented", ["reviewedOn"] = "2026-09-14",
			["methods"] = new JsonArray("Java plugins"), ["notes"] = "Requires its loader.",
			["sources"] = new JsonArray("https://docs.papermc.io/paper/adding-plugins")
		};
		JsonObject document = new() { ["schemaVersion"] = 1, ["scope"] = "Server documentation", ["entries"] = new JsonArray(entry) };
		Assert.Single(GameModSupportCatalog.Parse(document.ToJsonString()).Entries);
		entry["status"] = "Unsupported";
		Assert.Throws<InvalidDataException>(() => GameModSupportCatalog.Parse(document.ToJsonString()));
		entry["status"] = "Documented";
		entry["sources"] = new JsonArray("file:///C:/unsafe.exe");
		Assert.Throws<InvalidDataException>(() => GameModSupportCatalog.Parse(document.ToJsonString()));
		entry["sources"] = new JsonArray();
		Assert.Throws<InvalidDataException>(() => GameModSupportCatalog.Parse(document.ToJsonString()));
		entry["status"] = "NeedsVerification";
		Assert.Throws<InvalidDataException>(() => GameModSupportCatalog.Parse(document.ToJsonString()));
		entry["methods"] = new JsonArray();
		Assert.Single(GameModSupportCatalog.Parse(document.ToJsonString()).Entries);
		entry["gameId"] = "not-a-database-game";
		Assert.Throws<InvalidDataException>(() => GameModSupportCatalog.Parse(document.ToJsonString()));
	}

	[Theory]
	[InlineData("en")]
	[InlineData("de")]
	[InlineData("es")]
	[InlineData("fr")]
	public void GuideIsReadableAndDistinguishesDocumentationFromInstallation(string language) => WorkflowUiTest.Run(() =>
	{
		string previous = LocalizationManager.CurrentLanguageCode;
		try
		{
			LocalizationManager.SetLanguage(language);
			using GameModSupportDialog dialog = new("Conan Exiles");
			dialog.StartPosition = FormStartPosition.Manual;
			dialog.Location = new(-20000, -20000);
			dialog.Show();
			Application.DoEvents();
			RichTextBox details = Assert.Single(dialog.Controls.Find("gameModSupportDetails", true).OfType<RichTextBox>());
			Assert.True(details.ReadOnly);
			Assert.True(details.WordWrap);
			Assert.Equal(RichTextBoxScrollBars.Vertical, details.ScrollBars);
			// Windows RichEdit normalizes assigned CRLF text to LF on readback.
			Assert.Contains("\n", details.Text);
			Assert.Contains("modlist.txt", details.Text);
			Assert.Contains("https://forums.funcom.com/", details.Text);
			Assert.Contains(LocalizationManager.Get("ModSupport.EvidenceNote"), details.Text);
			Assert.DoesNotContain("ModSupport.", details.Text);
			string? folder = Environment.GetEnvironmentVariable("SYNIX_TEST_UI_PREVIEW_DIR");
			if (!string.IsNullOrWhiteSpace(folder))
			{
				Directory.CreateDirectory(folder);
				using System.Drawing.Bitmap bitmap = new(dialog.Width, dialog.Height);
				dialog.DrawToBitmap(bitmap, new System.Drawing.Rectangle(System.Drawing.Point.Empty, dialog.Size));
				bitmap.Save(Path.Combine(folder, "mod-support-" + language + ".png"), System.Drawing.Imaging.ImageFormat.Png);
			}
		}
		finally { LocalizationManager.SetLanguage(previous); }
	});
}
