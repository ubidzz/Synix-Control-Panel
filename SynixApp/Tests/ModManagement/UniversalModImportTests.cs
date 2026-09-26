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
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Windows.Forms;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.Design.Controls;
using Synix_Control_Panel.SynixApp.Localization;
using Synix_Control_Panel.SynixEngine.ModManagement;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class UniversalModImportTests : IDisposable
{
	private readonly string _root = Path.Combine(Path.GetTempPath(), "SynixUniversalImport-" + Guid.NewGuid().ToString("N"));
	private readonly string? _oldData = ModPackageManager.DataRootOverride;
	private readonly string? _oldProfiles = ModSystemCatalog.ExternalProfileRootOverride;
	private readonly GameServer _server;
	public UniversalModImportTests()
	{
		Directory.CreateDirectory(_root);
		ModPackageManager.DataRootOverride = Path.Combine(_root, "history");
		ModSystemCatalog.ExternalProfileRootOverride = Path.Combine(_root, "profiles");
		Directory.CreateDirectory(ModSystemCatalog.ExternalProfileRootOverride);
		_server = new() { Game = "Future Moddable Server", ServerName = "one", InstallPath = Path.Combine(_root, "server"), Status = "Stopped" };
		Directory.CreateDirectory(_server.InstallPath);
	}

	[Fact]
	public void UserLocationsWorkForUnlistedGamesWithoutClaimingBuiltInSupport()
	{
		Assert.True(ModSystemCatalog.CanOpenManager(_server));
		Assert.False(ModSystemCatalog.CanManageAddOns(_server));
		ModSystemProfile profile = Save();
		Assert.True(profile.UserConfigured);
		Assert.True(ModSystemCatalog.CanManageAddOns(_server));
		Assert.Equal("Extensions/Mods", Assert.Single(UniversalModImports.Load(_server)!.Targets).RelativePath);
		Assert.False(Directory.Exists(Path.Combine(_server.InstallPath, "Extensions")));
		Assert.True(ModSystemCatalog.Detect(_server, profile)!.FrameworkDetected);
		Assert.Equal(ModPackageLayout.FolderTree, profile.Targets[0].PackageLayout);
		Assert.Empty(ModSystemCatalog.GetProfiles(_server.Game));
	}

	[Fact]
	public void LocationsAreServerScopedAndTheSamePathUpdatesWithoutDuplicating()
	{
		ModSystemProfile profile = Save();
		ModSystemProfile changed = UniversalModImports.SaveLocation(_server, "Renamed", "Extensions\\Mods", ".lua, .json, .lua", ModContentKind.Plugin);
		Assert.Equal(profile.Targets[0].Id, Assert.Single(changed.Targets).Id);
		Assert.Equal("Renamed", changed.Targets[0].DisplayName);
		Assert.Equal(new[] { ".lua", ".json" }, changed.Targets[0].AllowedExtensions);
		GameServer other = new() { Game = _server.Game, InstallPath = Path.Combine(_root, "other"), ServerName = "one" };
		Directory.CreateDirectory(other.InstallPath);
		Assert.Null(UniversalModImports.Load(other));
		Assert.False(ModSystemCatalog.CanManageAddOns(other));
	}

	[Theory]
	[InlineData("../outside")]
	[InlineData(".")]
	[InlineData("C:/Windows")]
	[InlineData("Mods/CON")]
	[InlineData("Mods//Nested")]
	public void ImportDestinationsCannotEscapeTheServerOrTargetItsRoot(string path) =>
		Assert.Throws<InvalidDataException>(() => UniversalModImports.CreateTarget(_server, "Mods", path, ".lua", ModContentKind.Mod));

	[Theory]
	[InlineData(".exe")]
	[InlineData(".ps1")]
	[InlineData(".bat")]
	[InlineData("*")]
	[InlineData("dll")]
	[InlineData("")]
	public void UnsafeOrMissingMainFileTypesAreRejected(string extensions) =>
		Assert.Throws<InvalidDataException>(() => UniversalModImports.CreateTarget(_server, "Mods", "mods", extensions, ModContentKind.Mod));

	[Fact]
	public void ARuleForADifferentGameCannotWriteIntoTheSelectedServer()
	{
		ModSystemProfile profile = Save();
		string zip = Zip(("mod/main.lua", "code"));
		_server.Game = "Different Game";
		Assert.Throws<InvalidDataException>(() => ModPackageManager.Import(_server, profile, profile.Targets[0], zip, securityContext: new(false)));
		Assert.False(Directory.Exists(Path.Combine(_server.InstallPath, "Extensions")));
	}

	[Fact]
	public void RunningServersCannotChangeTheirImportRules()
	{
		_server.Status = "Running";
		Assert.Throws<InvalidOperationException>(() => Save());
		Assert.False(Directory.Exists(ModPackageManager.GetServerDataFolder(_server)));
	}

	[Theory]
	[InlineData("")]
	[InlineData("Download/")]
	[InlineData("Any/Extra/Wrapper/")]
	public void FilePreviewMatchesRealImportAndRollbackForFlatOrWrappedBundles(string wrapper)
	{
		ModSystemProfile profile = Save();
		ModInstallTarget target = profile.Targets[0];
		string oldFile = Path.Combine(_server.InstallPath, target.RelativePath, "Alpha/main.lua");
		Directory.CreateDirectory(Path.GetDirectoryName(oldFile)!);
		File.WriteAllText(oldFile, "old contents");
		string zip = Zip((wrapper + "Alpha/main.lua", "new contents"),
			(wrapper + "Alpha/assets/data.bin", "asset"), (wrapper + "Beta/main.lua", "other mod"));
		ModPackagePreview preview = UniversalModPackage.Preview(_server, target, zip, wrapper);
		Assert.Equal(3, preview.Files.Count);
		Assert.Single(preview.Files, file => file.ReplacesFile);
		Assert.Equal("old contents", File.ReadAllText(oldFile));
		ModImportResult installed = ModPackageManager.Import(_server, profile, target, zip, preview.PackageSha256,
			securityContext: new(false), installationFolderName: wrapper);
		Assert.Equal(preview.Files.Count, installed.InstalledFileCount);
		foreach (ModPackageFilePreview file in preview.Files) Assert.True(File.Exists(Path.Combine(_server.InstallPath, file.Destination)));
		Assert.Equal("new contents", File.ReadAllText(oldFile));
		ModInventoryItem item = Assert.Single(ModPackageManager.Scan(_server, profile), entry => entry.Name == "Alpha\\main.lua");
		Assert.Equal(installed.DisplayName, item.ImportedPackageName);
		Assert.Equal(3, item.ImportedFileCount);
		Assert.Equal("asset", File.ReadAllText(Path.Combine(_server.InstallPath, target.RelativePath, "Alpha/assets/data.bin")));
		ModPackageManager.Remove(_server, installed.InstallationId);
		Assert.Equal("old contents", File.ReadAllText(oldFile));
		Assert.False(File.Exists(Path.Combine(_server.InstallPath, target.RelativePath, "Beta/main.lua")));
		Assert.NotNull(UniversalModImports.Load(_server));
	}

	[Fact]
	public void ASelectedPackageSubfolderDoesNotCopyUnselectedFiles()
	{
		ModSystemProfile profile = Save();
		string zip = Zip(("Docs/readme.txt", "notes"), ("Distribution/Mods/Actual/main.lua", "mod"));
		const string root = "Distribution/Mods/";
		ModPackagePreview preview = UniversalModPackage.Preview(_server, profile.Targets[0], zip, root);
		Assert.Single(preview.Files);
		ModPackageManager.Import(_server, profile, profile.Targets[0], zip, preview.PackageSha256, securityContext: new(false), installationFolderName: root);
		Assert.Equal("mod", File.ReadAllText(Path.Combine(_server.InstallPath, "Extensions/Mods/Actual/main.lua")));
		Assert.False(Directory.Exists(Path.Combine(_server.InstallPath, "Extensions/Mods/Docs")));
	}

	[Fact]
	public void FolderAndIndividualPluginFilesUseTheSameImportAndRecoveryPipeline()
	{
		ModSystemProfile profile = Save();
		ModInstallTarget target = profile.Targets[0];
		string folder = Path.Combine(_root, "UnusualFolder");
		Directory.CreateDirectory(Path.Combine(folder, "Deep/CustomMod"));
		string source = Path.Combine(folder, "Deep/CustomMod/main.lua");
		File.WriteAllText(source, "return 1");
		using (PreparedModPackage prepared = PreparedModPackage.FromFolder(folder, target))
		{
			ModPackagePreview preview = UniversalModPackage.Preview(_server, target, prepared.Path, "Deep/");
			ModImportResult installed = ModPackageManager.Import(_server, profile, target, prepared.Path, preview.PackageSha256,
				securityContext: new(false), installationFolderName: "Deep/");
			Assert.True(File.Exists(Path.Combine(_server.InstallPath, target.RelativePath, "CustomMod/main.lua")));
			ModPackageManager.Remove(_server, installed.InstallationId);
		}
		ModPackagePreview loose = UniversalModPackage.Preview(_server, target, source);
		ModImportResult single = ModPackageManager.Import(_server, profile, target, source, loose.PackageSha256, securityContext: new(false));
		Assert.Equal(1, single.InstalledFileCount);
		Assert.Equal("return 1", File.ReadAllText(source));
	}

	[Theory]
	[InlineData("../outside.lua")]
	[InlineData("mod/file:secret")]
	[InlineData("mod/CON")]
	[InlineData("mod//main.lua")]
	[InlineData("mod//")]
	public void UnsafePackagePathsFailBeforeAnyServerWrite(string path)
	{
		ModSystemProfile profile = Save();
		string zip = Zip(("good/main.lua", "ok"), (path, "bad"));
		Assert.Throws<InvalidDataException>(() => UniversalModPackage.Preview(_server, profile.Targets[0], zip));
		Assert.False(Directory.Exists(Path.Combine(_server.InstallPath, "Extensions")));
	}

	[Fact]
	public void CaseCollisionsLinksAndFileDirectoryConflictsAreRejected()
	{
		ModInstallTarget target = Save().Targets[0];
		foreach (string path in new[] { Zip(("a/main.lua", "one"), ("A/MAIN.LUA", "two")), Zip(("a", "file"), ("a/main.lua", "nested")) })
			Assert.Throws<InvalidDataException>(() => UniversalModPackage.Preview(_server, target, path));
		string linked = Zip(("main.lua", "code"));
		using (ZipArchive zip = ZipFile.Open(linked, ZipArchiveMode.Update)) zip.Entries[0].ExternalAttributes = unchecked((int)0xa1ff0000);
		Assert.Throws<InvalidDataException>(() => UniversalModPackage.Preview(_server, target, linked));
	}

	[Fact]
	public void AnInvalidSelectionOrPackageWithoutAcceptedMainFilesIsNotImported()
	{
		ModInstallTarget target = Save().Targets[0];
		string zip = Zip(("README.txt", "notes"), ("mod/main.lua", "code"));
		Assert.Throws<InvalidDataException>(() => UniversalModPackage.Preview(_server, target, zip, "../"));
		Assert.Throws<InvalidDataException>(() => UniversalModPackage.Preview(_server, target, zip, "missing/"));
		Assert.Throws<InvalidDataException>(() => UniversalModPackage.Preview(_server, target, Zip(("only.txt", "not a mod"))));
	}

	[Fact]
	public void ChangedFilesAfterPreviewCannotBeInstalled()
	{
		ModSystemProfile profile = Save();
		string zip = Zip(("main.lua", "original"));
		ModPackagePreview preview = UniversalModPackage.Preview(_server, profile.Targets[0], zip);
		using (ZipArchive archive = ZipFile.Open(zip, ZipArchiveMode.Update))
			using (StreamWriter writer = new(archive.CreateEntry("extra.lua").Open())) writer.Write("changed");
		Assert.Throws<InvalidDataException>(() => ModPackageManager.Import(_server, profile, profile.Targets[0], zip,
			preview.PackageSha256, securityContext: new(false)));
		Assert.False(Directory.Exists(Path.Combine(_server.InstallPath, "Extensions")));
	}

	[Fact]
	public void CancelledPackageReadsReleaseTheirSourceWithoutWritingServerFiles()
	{
		ModInstallTarget target = Save().Targets[0];
		string zip = Zip(("mod/main.lua", "code"));
		Assert.Throws<OperationCanceledException>(() => UniversalModPackage.Preview(_server, target, zip,
			cancellationToken: new CancellationToken(canceled: true)));
		using FileStream exclusive = new(zip, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
		Assert.False(Directory.Exists(Path.Combine(_server.InstallPath, "Extensions")));
	}

	[Fact]
	public void InvalidCustomRulesDoNotBlockBuiltInProfilesOrGetSilentlyOverwritten()
	{
		_server.Game = "ARK: Survival Ascended";
		Save();
		string path = Path.Combine(ModPackageManager.GetServerDataFolder(_server), "import-locations.modsystem.json");
		File.WriteAllText(path, "invalid JSON");
		Assert.Contains(ModSystemCatalog.GetProfiles(_server), profile => profile.Targets.Any(target => target.CanManageIds));
		Assert.Throws<InvalidDataException>(() => Save());
		Assert.Equal("invalid JSON", File.ReadAllText(path));
	}

	[Fact]
	public void JavaScriptNeedsExplicitOptInAndInstallerScriptsRemainBlocked()
	{
		ModInstallTarget normal = Save().Targets[0];
		Assert.Throws<InvalidDataException>(() => ModSecurityScanner.InspectPackageStructure(Zip(("main.lua", "mod"), ("payload.js", "code")), normal));
		ModSystemProfile scripted = UniversalModImports.SaveLocation(_server, "JS plugins", "resources", ".js", ModContentKind.Plugin);
		ModInstallTarget js = scripted.Targets.Last();
		Assert.NotEmpty(ModSecurityScanner.InspectPackageStructure(Zip(("Resource/main.js", "exports.mod = {};")), js));
		Assert.Throws<InvalidDataException>(() => ModSecurityScanner.InspectPackageStructure(Zip(("main.js", "mod"), ("install.cmd", "exit")), js));
	}

	[Theory]
	[InlineData("ARK: Survival Ascended")]
	[InlineData("ARK: Survival Evolved")]
	[InlineData("Minecraft Bedrock")]
	public void CustomFileLocationsCoexistWithProviderProfilesWithoutConvertingTheirMods(string game)
	{
		_server.Game = game;
		Save();
		IReadOnlyList<ModSystemProfile> profiles = ModSystemCatalog.GetProfiles(_server);
		Assert.Single(profiles, profile => profile.UserConfigured);
		if (game.StartsWith("ARK")) Assert.Contains(profiles, profile => profile.Targets.Any(target => target.CanManageIds));
		Assert.True(ModSystemCatalog.CanManageAddOns(_server));
	}

	[Theory]
	[InlineData("en-US")]
	[InlineData("de-DE")]
	[InlineData("fr-FR")]
	[InlineData("es-ES")]
	public void NewDialogsUseTheThemeAndPreviewTheFullDestination(string language) => WorkflowUiTest.Run(() =>
	{
		string previous = LocalizationManager.CurrentLanguageCode;
		try
		{
			LocalizationManager.Initialize(language);
			ModSystemProfile profile = Save();
			using ModImportLocationDialog location = new(_server);
			ShowOffscreen(location);
			location.Size = location.MinimumSize;
			TextBox path = Assert.IsType<TextBox>(location.Controls.Find("importLocationPath", true).Single());
			path.Text = "Extensions/Mods";
			Assert.True(path.Parent!.DisplayRectangle.Contains(path.Bounds), "The location input must fit inside its card.");
			Assert.True(path.Height >= path.Font.Height, "The location input must show a complete line of text.");
			Assert.Equal(SettingsPalette.Input, path.BackColor);
			Assert.Equal(BorderStyle.None, path.BorderStyle);
			Control save = location.Controls.Find("saveImportLocation", true).Single();
			Assert.True(location.RectangleToClient(save.RectangleToScreen(save.ClientRectangle)).Bottom <= location.ClientSize.Height);
			Capture(location, "location-" + language);
			location.Close();
			string zip = Zip(("Outer/Alpha/main.lua", "first"), ("Outer/Beta/main.lua", "second"));
			using UniversalModImportPreview dialog = new(_server, profile.Targets[0], zip);
			ShowOffscreen(dialog);
			WorkflowUiTest.WaitUntil(() => dialog.PackageSha256.Length > 0);
			WorkflowUiTest.Pump(dialog.LoadPreviewAsync("Outer/"));
			dialog.Size = dialog.MinimumSize;
			DataGridView grid = Assert.IsType<DataGridView>(dialog.Controls.Find("universalModFiles", true).Single());
			Assert.Equal(2, grid.Rows.Count);
			grid.CurrentCell = grid.Rows[1].Cells[0];
			RichTextBox details = Assert.IsType<RichTextBox>(dialog.Controls.Find("universalModFileDetails", true).Single());
			Assert.True(details.ReadOnly && details.Multiline && details.WordWrap);
			Assert.Contains(Path.Combine(_server.InstallPath, "Extensions/Mods/Beta/main.lua").Replace('/', '\\'), details.Text);
			Assert.Equal("Outer/", dialog.SelectedRoot);
			Assert.True(dialog.Controls.Find("continueModImport", true).Single().Enabled);
			Assert.False(Directory.Exists(Path.Combine(_server.InstallPath, "Extensions")));
			Capture(dialog, "preview-" + language);
			WorkflowUiTest.Pump(dialog.LoadPreviewAsync("missing/"));
			Assert.Empty(dialog.PackageSha256);
			Assert.Empty(grid.Rows.Cast<DataGridViewRow>());
			Assert.False(dialog.Controls.Find("continueModImport", true).Single().Enabled);
			dialog.Close();
		}
		finally { LocalizationManager.Initialize(previous); }
	});

	private ModSystemProfile Save() => UniversalModImports.SaveLocation(_server, "Custom mods", "Extensions/Mods", ".lua", ModContentKind.Mod);
	private string Zip(params (string Path, string Contents)[] entries)
	{
		string path = Path.Combine(_root, Guid.NewGuid().ToString("N") + ".zip");
		using ZipArchive zip = ZipFile.Open(path, ZipArchiveMode.Create);
		foreach ((string name, string content) in entries) { using StreamWriter writer = new(zip.CreateEntry(name).Open()); writer.Write(content); }
		return path;
	}
	private static void ShowOffscreen(Form form) { form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-20000, -20000); form.Show(); Application.DoEvents(); }
	private static void Capture(Form form, string name)
	{
		string? folder = Environment.GetEnvironmentVariable("SYNIX_TEST_UI_PREVIEW_DIR");
		if (string.IsNullOrEmpty(folder)) return;
		Directory.CreateDirectory(folder);
		using Bitmap bitmap = new(form.Width, form.Height);
		form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size));
		bitmap.Save(Path.Combine(folder, "universal-" + name + ".png"), ImageFormat.Png);
	}
	public void Dispose()
	{
		ModPackageManager.DataRootOverride = _oldData;
		ModSystemCatalog.ExternalProfileRootOverride = _oldProfiles;
		ModPathSafety.EnsureTreeHasNoLinks(_root);
		Directory.Delete(_root, recursive: true);
	}
}
