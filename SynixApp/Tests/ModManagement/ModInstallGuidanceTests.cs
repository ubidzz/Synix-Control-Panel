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
using Synix_Control_Panel.SynixApp.Localization;
using Synix_Control_Panel.SynixEngine.ModManagement;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class ModInstallGuidanceTests
{
	private static GameServer Server(string game = "Minecraft") => new()
	{
		Game = game, ServerName = "Review fixture", GameVersion = "1.21.1", MinecraftLoader = "Fabric",
		InstallPath = Path.Combine(Path.GetTempPath(), "SynixReview-" + Guid.NewGuid().ToString("N")), Status = "Stopped"
	};

	[Fact]
	public void FileReviewUsesTheFullDestinationAndDoesNotPromiseCompatibilityOrWorldRecovery()
	{
		GameServer server = Server();
		ModInstallTarget target = new() { RelativePath = "mods", Mode = ModTargetMode.FileImport };
		ModPackagePreview preview = new([], [new("A.jar", "mods/A.jar", 10, false), new("B.jar", "mods/B.jar", 20, true)], "review-hash");
		ModInstallGuidance guide = ModInstallGuidance.Create(server, new(new(), target, "", true), preview);
		Assert.Contains("Layout/loader route recognized", guide.Compatibility);
		Assert.Contains("not fully verified", guide.Compatibility);
		Assert.Contains("1.21.1", guide.Compatibility);
		Assert.Contains("Fabric", guide.Compatibility);
		Assert.Contains(Path.Combine(server.InstallPath, "mods"), guide.Destination);
		Assert.Contains("2 files in the plan; 1 existing", guide.Destination);
		Assert.Contains("does not start", guide.Activation);
		Assert.Contains("later edits can block rollback", guide.Recovery);
		Assert.Contains("does not restore world changes", guide.Recovery);
		Assert.False(Directory.Exists(server.InstallPath));
		Assert.Empty(server.ExtraArgs);
	}

	[Theory]
	[InlineData(true, "Manual destination")]
	[InlineData(false, "Import blocked")]
	public void ManualAndMissingLoaderRoutesNeverClaimVerifiedCompatibility(bool manual, string expected)
	{
		GameServer server = Server();
		ModInstallGuidance guide = ModInstallGuidance.Create(server,
			new(new() { UserConfigured = manual }, new() { RelativePath = "Extensions", Mode = ModTargetMode.FileImport }, "", manual));
		Assert.Contains(expected, guide.Compatibility);
		if (manual) Assert.Contains("does not install a loader", guide.Activation);
		Assert.DoesNotContain("Layout/loader route recognized", guide.Compatibility);
		Assert.False(Directory.Exists(server.InstallPath));
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void ProviderReviewExplainsTheExactSettingsAndProviderOwnedFiles(bool argumentIds)
	{
		ModTargetMode mode = argumentIds ? ModTargetMode.ArgumentIds : ModTargetMode.ConfigurationIds;
		GameServer server = Server(mode == ModTargetMode.ArgumentIds ? ArkModPackageReader.Ascended : ArkModPackageReader.Evolved);
		server.MinecraftLoader = "";
		ModInstallTarget target = new()
		{
			Mode = mode, ProviderName = "Provider", ArgumentName = "-mods", RequiredArguments = ["-automanagedmods"],
			IdStores = [new() { RelativePath = "Config/Game.ini", Section = "ModInstaller", Key = "ModIDS" }]
		};
		ModInstallGuidance guide = ModInstallGuidance.Create(server, new(new(), target, "", true));
		Assert.Contains("not the ZIP's mod files", guide.Destination);
		if (mode == ModTargetMode.ArgumentIds)
			Assert.Contains("-mods=<ordered IDs>", guide.Destination);
		else
		{
			Assert.Contains(Path.Combine(server.InstallPath, "Config", "Game.ini"), guide.Destination);
			Assert.Contains("[ModInstaller] ModIDS", guide.Destination);
			Assert.Contains("-automanagedmods", guide.Destination);
		}
		Assert.Contains("not checked", guide.Compatibility);
		Assert.Contains("does not pin the version", guide.Activation);
		Assert.Contains("does not delete provider files", guide.Recovery);
		Assert.DoesNotContain("Roll Back Import", guide.Recovery);
		Assert.Empty(server.ExtraArgs);
		Assert.False(Directory.Exists(server.InstallPath));
	}

	[Fact]
	public void ScenarioReviewRequiresSeparateActivationAndExplainsProtectedSaves()
	{
		ModInstallTarget target = new() { Mode = ModTargetMode.FileImport, RelativePath = "Content/Scenarios",
			PackageLayout = ModPackageLayout.EmpyrionScenario };
		ModInstallGuidance guide = ModInstallGuidance.Create(Server("Empyrion - Galactic Survival"), new(new(), target, "", true));
		Assert.Contains("does not switch the active world", guide.Activation);
		Assert.Contains("Choose Scenario", guide.Activation);
		Assert.Contains("existing saves may prevent removal", guide.Recovery);
		Assert.Contains(guide.Activation, guide.NextSteps());
		Assert.Contains(guide.Recovery, guide.NextSteps());
	}

	[Theory]
	[InlineData("en-US")]
	[InlineData("de-DE")]
	[InlineData("fr-FR")]
	[InlineData("es-ES")]
	public void GuidanceIsLocalizedWithoutUnresolvedKeysOrFormatArguments(string language)
	{
		string previous = LocalizationManager.CurrentLanguageCode;
		try
		{
			LocalizationManager.Initialize(language);
			GameServer server = Server();
			foreach (bool manual in new[] { true, false })
			{
				ModInstallGuidance guide = ModInstallGuidance.Create(server,
					new(new() { UserConfigured = manual }, new() { RelativePath = "mods" }, "", true));
				string text = guide.Compatibility + guide.Destination + guide.NextSteps();
				Assert.DoesNotContain("ModInstallGuide.", text);
				Assert.DoesNotContain("{0}", text);
				Assert.Contains("1.21.1", text);
				Assert.Contains("Fabric", text);
			}
		}
		finally { LocalizationManager.Initialize(previous); }
	}
}
