using System.Text.Json;
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Database.GameDefinitions;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixApp.SteamCMDHandler;
using Synix_Control_Panel.SynixEngine;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class SteamDownloadAuditTests
{
	[Fact]
	public void EveryBuiltInDownloadMatchesItsReviewedIdentityAndSelectors()
	{
		using Stream stream = typeof(SteamDownloadAuditTests).Assembly
			.GetManifestResourceStream("Synix.Tests.SteamDownloadAudit.json")!;
		using JsonDocument document = JsonDocument.Parse(stream);
		JsonElement[] entries = document.RootElement.GetProperty("entries").EnumerateArray().ToArray();
		Assert.Equal(227, entries.Length);
		Assert.Equal(entries.Length, entries.Select(entry => entry.GetProperty("id").GetString()).Distinct().Count());
		JsonElement[] active = entries.Where(entry => entry.GetProperty("disposition").GetString() != "Removed").ToArray();
		Assert.Equal(GameDatabase.GetGames.Select(game => game.DefinitionId).Order(),
			active.Select(entry => entry.GetProperty("id").GetString()).Order());

		foreach (JsonElement entry in entries)
		{
			string name = entry.GetProperty("game").GetString()!;
			GameInfo? game = GameDatabase.GetGame(name);
			Assert.NotEmpty(entry.GetProperty("evidence").EnumerateArray());
			if (entry.GetProperty("disposition").GetString() == "Removed")
			{
				Assert.Null(game);
				Assert.Equal(JsonValueKind.Null, entry.GetProperty("appId").ValueKind);
				continue;
			}
			Assert.NotNull(game);
			Assert.Equal(entry.GetProperty("appId").GetString(), game.AppID);
			Assert.Equal(entry.GetProperty("steamBranch").GetString(), game.SteamBranch);
			Assert.Equal(entry.GetProperty("steamAppConfig").GetString(), game.SteamAppConfig);
			Assert.Equal(entry.GetProperty("requiresSteamLogin").GetBoolean(), game.RequiresSteamLogin);
			if (game.AppID == "0") continue;

			var args = ServerInstaller.CreateSteamProcessStartInfo(new GameServer
			{
				InstallPath = @"C:\Synix\Test server",
				SteamAccountName = "test-account"
			}, game, null).ArgumentList.ToList();
			int update = args.IndexOf("+app_update");
			Assert.True(update >= 0);
			Assert.Equal(game.AppID, args[update + 1]);
			if (game.SteamAppConfig.Length > 0)
			{
				int config = args.IndexOf("+app_set_config");
				Assert.InRange(config, 0, update - 1);
				Assert.Equal(game.SteamAppConfig, args[config + 1]);
			}
		}
	}

	[Fact]
	public void SharedSteamDownloadsHaveDistinctExplicitSelections()
	{
		var shared = GameDatabase.GetGames.Where(game => game.AppID != "0")
			.GroupBy(game => game.AppID).Where(group => group.Count() > 1).ToArray();
		Assert.Equal(new[] { "412680", "90" }, shared.Select(group => group.Key).Order());
		foreach (var group in shared)
		{
			Assert.All(group, game => Assert.True(game.SteamAppConfig.Length > 0 || game.SteamBranch.Length > 0));
			Assert.Equal(group.Count(), group.Select(game => (game.SteamAppConfig, game.SteamBranch)).Distinct().Count());
		}
		Assert.Equal(8, shared.Single(group => group.Key == "90").Count());
	}

	[Theory]
	[InlineData("The Isle", "public")]
	[InlineData("The Isle (Evrima)", "evrima")]
	public void IsleInstallationAndUpdatesSelectTheIntendedBranch(string gameName, string branch)
	{
		GameInfo game = GameDatabase.GetGame(gameName)!;
		Assert.Equal(branch, game.SteamBranch);
		var args = ServerInstaller.CreateSteamProcessStartInfo(
			new GameServer { InstallPath = @"C:\Synix\The Isle" }, game, null).ArgumentList.ToList();
		int update = args.IndexOf("+app_update");
		Assert.Equal(new[] { "+app_update", "412680", "-beta", branch, "validate", "+quit" }, args.Skip(update));
	}

	[Theory]
	[InlineData("evrima +quit")]
	[InlineData("-beta")]
	[InlineData("public\n+quit")]
	[InlineData("evrima\"")]
	[InlineData("../private")]
	[InlineData(" ")]
	[InlineData("a;quit")]
	public void UnsafeSteamBranchesCannotReachSteamCmd(string branch)
	{
		GameInfo game = new() { Game = "Test", AppID = "412680", SteamBranch = branch };
		Assert.Throws<InvalidDataException>(() => ServerInstaller.CreateSteamProcessStartInfo(new GameServer(), game, null));
		string json = JsonSerializer.Serialize(new
		{
			id = "branch-test", game = "Branch Test", appId = "412680", steamBranch = branch,
			executable = "Server.exe", catalogOrder = 0, port = 7777, queryPort = 27015
		});
		Assert.Throws<InvalidDataException>(() => TrustedGameDefinitionCatalog.ParsePackage(json, "branch-test.game.json"));
	}

	[Theory]
	[InlineData("")]
	[InlineData("public")]
	[InlineData("evrima")]
	[InlineData("spiro0.11.59.04")]
	public void DefinitionAuthoringPreservesValidBranches(string branch)
	{
		string json = GameDefinitionAuthoring.CreateDefinitionJson(new GameDefinitionDraft
		{
			Id = "branch-test", Game = "Branch Test", AppId = "412680", SteamBranch = branch,
			Executable = "Server.exe"
		});
		Assert.Equal(branch, TrustedGameDefinitionCatalog.ParsePackage(json, "branch-test.game.json").Definition.SteamBranch);
	}

	[Fact]
	public void OrdinarySteamDownloadsDoNotAddBetaOptions()
	{
		GameInfo game = GameDatabase.GetGame("Palworld")!;
		Assert.DoesNotContain("-beta", ServerInstaller.CreateSteamProcessStartInfo(new GameServer(), game, null).ArgumentList);
	}

	[Fact]
	public void BranchValidationRejectsNullAndOverlongNames()
	{
		Assert.Throws<InvalidDataException>(() => TrustedGameDefinitionCatalog.ValidateSteamBranch(null, "test"));
		Assert.Throws<InvalidDataException>(() => TrustedGameDefinitionCatalog.ValidateSteamBranch(new string('a', 65), "test"));
		TrustedGameDefinitionCatalog.ValidateSteamBranch(new string('a', 64), "test");
	}

	[Theory]
	[InlineData("en")]
	[InlineData("de")]
	[InlineData("es")]
	[InlineData("fr")]
	public void BuilderKeepsBranchSelectionAndDoesNotOverlapSteamFields(string language) => WorkflowUiTest.Run(() =>
	{
		string previous = SynixApp.Localization.LocalizationManager.CurrentLanguageCode;
		try
		{
			SynixApp.Localization.LocalizationManager.SetLanguage(language);
			using SynixApp.UI.GameDefinitions.GameDefinitionBuilder builder = new();
			var branch = Assert.IsType<System.Windows.Forms.TextBox>(Assert.Single(builder.Controls.Find("txtSteamBranch", true)));
			var config = Assert.Single(builder.Controls.Find("txtSteamAppConfig", true));
			var label = Assert.Single(builder.Controls.Find("lblSteamBranch", true));
			var next = Assert.Single(builder.Controls.Find("lblRuntimeSection", true));
			Assert.True(branch.Top >= config.Bottom);
			Assert.True(branch.Bottom <= next.Top);
			Assert.True(label.Right <= branch.Left);
			branch.Text = "evrima";
			GameDefinitionDraft draft = Assert.IsType<GameDefinitionDraft>(WorkflowUiTest.Invoke(builder, "BuildDraft"));
			Assert.Equal("evrima", draft.SteamBranch);
		}
		finally { SynixApp.Localization.LocalizationManager.SetLanguage(previous); }
	});

	[Fact]
	public void RemovedCatalogEntriesDoNotErasePersistedServerRecords()
	{
		GameServer saved = new() { Game = "Deadside", ServerName = "Keep my server", InstallPath = @"C:\Synix\Existing Deadside" };
		string json = Core.SerializeServersForStorage([saved]);
		GameServer restored = Assert.Single(Core.DeserializeServersAndMigrate(json, out int _));
		Assert.Equal(saved.Game, restored.Game);
		Assert.Equal(saved.ServerName, restored.ServerName);
		Assert.Equal(saved.InstallPath, restored.InstallPath);
	}
}
