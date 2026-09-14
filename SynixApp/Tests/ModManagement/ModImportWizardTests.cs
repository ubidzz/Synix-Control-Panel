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
using System.Text;
using System.Windows.Forms;
using Synix_Control_Panel.SynixApp.Design.Controls;
using Synix_Control_Panel.SynixApp.Localization;
using Synix_Control_Panel.SynixEngine.ModManagement;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class ModImportWizardTests : IDisposable
{
	private readonly string _root = Path.Combine(Path.GetTempPath(), "SynixImportWizard-" + Guid.NewGuid().ToString("N"));
	private readonly string? _oldData = ModPackageManager.DataRootOverride, _oldProfiles = ModSystemCatalog.ExternalProfileRootOverride;
	private readonly GameServer _server;
	public ModImportWizardTests()
	{
		_server = new() { Game = "Unknown game", ServerName = "Import test", Status = "Stopped", InstallPath = Path.Combine(_root, "server") };
		Directory.CreateDirectory(_server.InstallPath);
		ModPackageManager.DataRootOverride = Path.Combine(_root, "history");
		ModSystemCatalog.ExternalProfileRootOverride = Path.Combine(_root, "profiles");
		Directory.CreateDirectory(ModSystemCatalog.ExternalProfileRootOverride);
	}

	[Fact]
	public void ManagerOffersImportBeforeAnyLocationsExistAndHidesAdvancedSettings() => WorkflowUiTest.Run(() =>
	{
		using ModPluginManager manager = new(_server);
		Show(manager);
		Control import = Find(manager, "importAddOnPackage");
		Assert.Equal(!ModSecurityScanner.IsCurrentProcessElevated(), import.Enabled);
		Assert.Contains("ZIP", import.Text);
		Assert.False(Find(manager, "configureModImportLocations").Visible);
		_server.Status = "Running";
		WorkflowUiTest.Invoke(manager, "SetBusy", false, null!);
		Assert.False(import.Enabled);
	});

	[Fact]
	public void UnknownPackageIsReadBeforeAskingForJustItsDestination()
	{
		string source = Zip(("SomeMod/main.lua", "code"), ("SomeMod/assets/image.txt", "asset"));
		ModImportAnalysis read = ModImportDiscovery.Read(_server, source);
		Assert.Equal(2, read.Files.Count);
		Assert.Empty(read.Choices);
		Assert.False(Directory.Exists(ModPackageManager.GetServerDataFolder(_server)));
		ModImportChoice choice = ModImportDiscovery.ChooseFolder(_server, read, Path.Combine(_server.InstallPath, "Extensions"));
		Assert.Equal([".lua"], choice.Target.AllowedExtensions);
		Assert.True(choice.RememberLocation);
		Assert.Equal("", choice.Selection); // Do not strip the actual mod folder.
		Assert.Null(UniversalModImports.Load(_server));
		Assert.Empty(Directory.GetFileSystemEntries(_server.InstallPath));
		Assert.ThrowsAny<Exception>(() => ModImportDiscovery.ChooseFolder(_server, read, _root));
		Assert.ThrowsAny<Exception>(() => ModImportDiscovery.ChooseFolder(_server, read, _server.InstallPath));
		RoundTrip(source, choice, 2);
	}

	[Theory]
	[InlineData("")]
	[InlineData("Download/")]
	public void SavedDestinationIsDetectedInsideDownloadWrappers(string wrapper)
	{
		SaveLocation();
		string source = Zip((wrapper + "Extensions/Mods/A/main.lua", "a"), (wrapper + "Extensions/Mods/B/main.lua", "b"));
		ModImportChoice choice = Assert.Single(ModImportDiscovery.Read(_server, source).Choices);
		Assert.Equal(wrapper + "Extensions/Mods/", choice.Selection);
		RoundTrip(source, choice, 2);
		Assert.Empty(Directory.GetFiles(_server.InstallPath, "*", SearchOption.AllDirectories));
	}

	[Fact]
	public void AmbiguousLocationsRemainChoicesInsteadOfBeingGuessed()
	{
		SaveLocation();
		UniversalModImports.SaveLocation(_server, "Alternate", "Alternate", ".lua", ModContentKind.Plugin);
		ModImportAnalysis read = ModImportDiscovery.Read(_server, Zip(("main.lua", "code")));
		Assert.Equal(2, read.Choices.Count(choice => choice.Ready));
	}

	[Theory]
	[InlineData("Paper", "plugin.yml", "plugins", true)]
	[InlineData("Forge", "plugin.yml", "plugins", false)]
	[InlineData("Forge", "META-INF/mods.toml", "mods", true)]
	[InlineData("Forge", "fabric.mod.json", "mods", false)]
	public void MinecraftReadsJarMetadataAndDoesNotRoutePluginsIntoForgeMods(string loader, string marker, string destination, bool ready)
	{
		_server.Game = "Minecraft"; _server.MinecraftLoader = loader;
		Directory.CreateDirectory(Path.Combine(_server.InstallPath, "plugins"));
		Directory.CreateDirectory(Path.Combine(_server.InstallPath, "mods"));
		string source = JarZip("Download/" + destination + "/Example.jar", marker);
		ModImportChoice choice = Assert.Single(ModImportDiscovery.Read(_server, source).Choices);
		Assert.Equal(destination, choice.Target.RelativePath);
		Assert.Equal(ready, choice.Ready);
		Assert.Equal("Download/" + destination + "/", choice.Selection);
		if (ready) RoundTrip(source, choice, 1);
	}

	[Theory]
	[InlineData("")]
	[InlineData("Outer/Example/")]
	public void SevenDaysMarkerPicksItsBuiltInInstallAreaAndPreservesAssets(string root)
	{
		_server.Game = "7 Days to Die";
		File.WriteAllText(Path.Combine(_server.InstallPath, "7DaysToDieServer.exe"), "test marker, never run");
		string source = Zip((root + "ModInfo.xml", "<xml/>"), (root + "Config/items.xml", "<items/>"), (root + "Resources/data.txt", "asset"));
		ModImportChoice choice = Assert.Single(ModImportDiscovery.Read(_server, source).Choices);
		Assert.Equal("Mods", choice.Target.RelativePath);
		Assert.True(choice.Ready);
		Assert.Equal(root.Length == 0 ? "" : "Outer/", choice.Selection);
		RoundTrip(source, choice, 3);
	}

	[Theory]
	[InlineData("../escape.lua")]
	[InlineData("A/../../escape.lua")]
	[InlineData("A/CON.lua")]
	public void UnsafePackagesFailDuringReadingBeforeAnyDestinationExists(string path)
	{
		Assert.ThrowsAny<Exception>(() => ModImportDiscovery.Read(_server, Zip((path, "code"))));
		Assert.Empty(Directory.GetFileSystemEntries(_server.InstallPath));
	}

	[Fact]
	public void CancelledReadReleasesSourceAndDoesNotSaveAnything()
	{
		string source = Zip(("mod/main.lua", "code"));
		Assert.Throws<OperationCanceledException>(() => ModImportDiscovery.Read(_server, source, new CancellationToken(true)));
		using FileStream check = new(source, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
		Assert.Null(UniversalModImports.Load(_server));
	}

	[Fact]
	public void AsaPreviewUsesProviderMetadataWithoutClaimingAFileInstallation() => WorkflowUiTest.Run(() =>
	{
		_server.Game = ArkModPackageReader.Ascended;
		string source = Zip(
			("Wrapper/Example/Example.uplugin", """{"FriendlyName":"Example","VersionName":"1","Description":"cf_ugcID=123456","MarketplaceURL":"https://www.curseforge.com/ark-survival-ascended/mods/example"}"""),
			("Wrapper/Example/Content/Paks/WindowsServer/Example.pak", "pak"),
			("Wrapper/Example/Content/Paks/WindowsServer/Example.ucas", "ucas"),
			("Wrapper/Example/Content/Paks/WindowsServer/Example.utoc", "utoc"));
		using ModImportWizard wizard = new(_server);
		Show(wizard); Read(wizard, source);
		Assert.NotNull(wizard.Selection);
		Assert.True(wizard.Selection.Target.CanManageIds);
		Assert.Equal("123456", Assert.Single(wizard.ProviderMods).ModId);
		Assert.Contains("not a local file upload", Find(wizard, "modImportWizardStatus").Text);
		Assert.False(Find(wizard, "chooseModDestination").Visible);
		Assert.Empty(_server.ExtraArgs);
		Assert.Empty(Directory.GetFileSystemEntries(_server.InstallPath));
		Capture(wizard, "provider");
		_server.Game = ArkModPackageReader.Evolved;
		Assert.Throws<InvalidDataException>(() => ModImportDiscovery.Read(_server, source));
	});

	[Theory]
	[InlineData("en-US")]
	[InlineData("de-DE")]
	[InlineData("fr-FR")]
	[InlineData("es-ES")]
	public void WizardStartsWithSourcePickerThenShowsDetectedPlanAndClearsStaleResults(string language) => WorkflowUiTest.Run(() =>
	{
		string previous = LocalizationManager.CurrentLanguageCode;
		try
		{
			LocalizationManager.Initialize(language);
			using ModImportWizard wizard = new(_server);
			Show(wizard); wizard.Size = wizard.MinimumSize;
			Assert.True(Find(wizard, "chooseModPackageFile").Enabled);
			Assert.True(Find(wizard, "chooseModPackageFolder").Enabled);
			Assert.False(Find(wizard, "chooseModDestination").Visible);
			Assert.False(Find(wizard, "confirmModImportPlan").Enabled);
			Capture(wizard, "empty-" + language);
			SaveLocation();
			string source = Zip(("Download/Extensions/Mods/A/main.lua", "code"), ("Download/Extensions/Mods/B/main.lua", "other"));
			Read(wizard, source);
			Assert.Equal("Download/Extensions/Mods/", wizard.Selection?.Selection);
			Assert.True(Find(wizard, "confirmModImportPlan").Enabled);
			DataGridView grid = Assert.IsType<DataGridView>(Find(wizard, "modImportPlanFiles"));
			Assert.Equal(2, grid.Rows.Count);
			grid.CurrentCell = grid.Rows[1].Cells[0];
			RichTextBox details = Assert.IsType<RichTextBox>(Find(wizard, "modImportPlanDetails"));
			Assert.True(details.ReadOnly && details.Multiline && details.WordWrap);
			Assert.Equal(SettingsPalette.Input, details.BackColor);
			Assert.Contains(Path.Combine(_server.InstallPath, "Extensions", "Mods", "B", "main.lua"), details.Text);
			Control confirm = Find(wizard, "confirmModImportPlan"), destination = Find(wizard, "chooseModDestination");
			Assert.False(confirm.Bounds.IntersectsWith(destination.Bounds));
			foreach (Control button in new[] { confirm, destination, Find(wizard, "chooseModPackageFolder") })
			{
				Rectangle bounds = wizard.RectangleToClient(button.RectangleToScreen(button.ClientRectangle));
				Assert.True(wizard.ClientRectangle.Contains(bounds));
			}
			Capture(wizard, "read-" + language);
			Read(wizard, Zip(("../bad.lua", "code")));
			Assert.Null(wizard.Selection);
			Assert.Empty(wizard.PackageSha256);
			Assert.Empty(grid.Rows.Cast<DataGridViewRow>());
			Assert.False(Find(wizard, "confirmModImportPlan").Enabled);
			Assert.Empty(Directory.GetFileSystemEntries(_server.InstallPath));
		}
		finally { LocalizationManager.Initialize(previous); }
	});

	[Fact]
	public void ExtractedFolderUsesTheSamePreviewAndPrivateSnapshotIsRemoved() => WorkflowUiTest.Run(() =>
	{
		SaveLocation();
		string folder = Path.Combine(_root, "download");
		Directory.CreateDirectory(Path.Combine(folder, "Alpha"));
		File.WriteAllText(Path.Combine(folder, "Alpha", "main.lua"), "code");
		string snapshot;
		using (ModImportWizard wizard = new(_server))
		{
			Show(wizard); Read(wizard, folder);
			snapshot = wizard.PackagePath;
			Assert.True(File.Exists(snapshot));
			Assert.NotNull(wizard.Selection);
			Assert.EndsWith(".zip", snapshot);
			Assert.Equal("", wizard.Selection.Selection);
			Assert.True(Find(wizard, "confirmModImportPlan").Enabled);
		}
		Assert.False(File.Exists(snapshot));
		Assert.Equal("code", File.ReadAllText(Path.Combine(folder, "Alpha", "main.lua")));
	});

	private void RoundTrip(string source, ModImportChoice choice, int count)
	{
		ModPackagePreview preview = UniversalModPackage.Preview(_server, choice.Target, source, choice.Selection);
		Assert.Equal(count, preview.Files.Count);
		ModImportResult result = ModPackageManager.Import(_server, choice.Profile, choice.Target, source,
			preview.PackageSha256, securityContext: new(false), installationFolderName: choice.Selection.Length == 0 ? null : choice.Selection);
		Assert.Equal(count, result.InstalledFileCount);
		foreach (ModPackageFilePreview file in preview.Files) Assert.True(File.Exists(Path.Combine(_server.InstallPath, file.Destination)), file.Destination);
		ModInventoryItem item = ModPackageManager.Scan(_server, choice.Profile).First(item => item.InstallationId != null);
		ModPackageManager.Remove(_server, item.InstallationId!);
		foreach (ModPackageFilePreview file in preview.Files) Assert.False(File.Exists(Path.Combine(_server.InstallPath, file.Destination)));
	}
	private void SaveLocation() => UniversalModImports.SaveLocation(_server, "Extensions", "Extensions/Mods", ".lua", ModContentKind.Mod);

	[Theory]
	[InlineData(".gma")]
	[InlineData(".tmod")]
	[InlineData(".pbo")]
	[InlineData(".mpk")]
	[InlineData(".jbeam")]
	public void GameSpecificFilesCanBeInspectedWithoutInventingAnInstallRule(string extension)
	{
		string source = Zip(("Download/Package/Content" + extension, "sample"));
		ModImportAnalysis analysis = ModImportDiscovery.Read(_server, source);
		Assert.Single(analysis.Files);
		Assert.Empty(analysis.Choices);
		Assert.Empty(Directory.EnumerateFileSystemEntries(_server.InstallPath));
	}
	private string Zip(params (string Path, string Text)[] files)
	{
		string path = Path.Combine(_root, Guid.NewGuid().ToString("N") + ".zip");
		using ZipArchive zip = ZipFile.Open(path, ZipArchiveMode.Create);
		foreach ((string name, string text) in files) { using StreamWriter writer = new(zip.CreateEntry(name).Open()); writer.Write(text); }
		return path;
	}
	private string JarZip(string path, string marker)
	{
		using MemoryStream memory = new();
		using (ZipArchive jar = new(memory, ZipArchiveMode.Create, leaveOpen: true))
		{ using StreamWriter writer = new(jar.CreateEntry(marker).Open()); writer.Write("test metadata"); }
		string file = Path.Combine(_root, Guid.NewGuid().ToString("N") + ".zip");
		using ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Create);
		using Stream output = zip.CreateEntry(path).Open(); output.Write(memory.ToArray());
		return file;
	}
	private static Control Find(Control form, string name) => form.Controls.Find(name, true).Single();
	private static void Show(Form form)
	{
		form.StartPosition = FormStartPosition.Manual;
		form.Location = new Point(-20000, -20000);
		form.Show();
		Application.DoEvents();
	}
	private static void Read(ModImportWizard wizard, string source)
	{
		// DoEvents restores the test runner's default context when its loop exits.
		// Capture the UI dispatcher when starting each asynchronous window action.
		SynchronizationContext? previous = SynchronizationContext.Current;
		try
		{
			SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
			WorkflowUiTest.Pump(wizard.ReadSourceAsync(source));
		}
		finally { SynchronizationContext.SetSynchronizationContext(previous); }
	}
	private static void Capture(Form form, string name)
	{
		string? folder = Environment.GetEnvironmentVariable("SYNIX_TEST_UI_PREVIEW_DIR");
		if (string.IsNullOrEmpty(folder)) return;
		Directory.CreateDirectory(folder);
		using Bitmap bitmap = new(form.Width, form.Height);
		form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size));
		bitmap.Save(Path.Combine(folder, "import-wizard-" + name + ".png"), ImageFormat.Png);
	}
	public void Dispose()
	{
		ModPackageManager.DataRootOverride = _oldData; ModSystemCatalog.ExternalProfileRootOverride = _oldProfiles;
		ModPathSafety.EnsureTreeHasNoLinks(_root);
		Directory.Delete(_root, recursive: true);
	}
}
