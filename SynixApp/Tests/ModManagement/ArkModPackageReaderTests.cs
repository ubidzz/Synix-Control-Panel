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
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Drawing;
using System.Drawing.Imaging;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.Design.Controls;
using Synix_Control_Panel.SynixApp.Localization;
using Synix_Control_Panel.SynixEngine.ModManagement;
using System.Windows.Forms;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class ArkModPackageReaderTests : IDisposable
{
	private readonly string _root = Path.Combine(Path.GetTempPath(), "SynixAsaReader-" + Guid.NewGuid().ToString("N"));
	public ArkModPackageReaderTests() => Directory.CreateDirectory(_root);

	[Theory]
	[InlineData("")]
	[InlineData("SomeMod/")]
	[InlineData("Download/Server/SomeMod/")]
	[InlineData("83374/123456_7654321/SomeMod/")]
	[InlineData("SomeMod\\")]
	public void ReadsFlatAndWrappedZipsWithoutDependingOnTheFilenameOrExtractingFiles(string prefix)
	{
		string path = Zip(Package(prefix));
		byte[] before = SHA256.HashData(File.ReadAllBytes(path));
		ArkModPackageInfo mod = Assert.Single(ArkModPackageReader.Read(path));
		Assert.Equal("123456", mod.ModId);
		Assert.Equal("Example server mod", mod.Name);
		Assert.Equal("version-fingerprint", mod.Version);
		Assert.Equal(before, SHA256.HashData(File.ReadAllBytes(path)));
		Assert.Single(Directory.GetFiles(_root));
		Assert.Empty(Directory.GetDirectories(_root));
	}

	[Theory]
	[InlineData("")]
	[InlineData("Extra/Wrapper/SomeMod/")]
	public void ReadsExtractedFoldersWithoutChangingThem(string prefix)
	{
		string folder = Path.Combine(_root, "source");
		foreach ((string name, string content) in Package(prefix))
		{
			string path = Path.Combine(folder, name.Replace('/', Path.DirectorySeparatorChar));
			Directory.CreateDirectory(Path.GetDirectoryName(path)!);
			File.WriteAllText(path, content);
		}
		Assert.Equal("123456", Assert.Single(ArkModPackageReader.Read(folder)).ModId);
		Assert.Equal(4, Directory.GetFiles(folder, "*", SearchOption.AllDirectories).Length);
	}

	[Fact]
	public void ReadsEveryModInABundleRatherThanSilentlyTakingTheFirst()
	{
		var entries = Package("B/", "777").Concat(Package("A/", "888"));
		Assert.Equal(new[] { "888", "777" }, ArkModPackageReader.Read(Zip(entries)).Select(mod => mod.ModId));
	}

	[Fact]
	public void DuplicateVersionsOfTheSameProjectAreRejected() =>
		Assert.Throws<InvalidDataException>(() => ArkModPackageReader.Read(Zip(Package("A/").Concat(Package("B/")))));

	[Theory]
	[InlineData("../outside.uplugin")]
	[InlineData("/absolute.uplugin")]
	[InlineData("C:/absolute.uplugin")]
	[InlineData("SomeMod/file:stream")]
	[InlineData("SomeMod/CON.txt")]
	[InlineData("SomeMod/dot./file")]
	[InlineData("SomeMod//file")]
	public void RejectsUnsafePathsAnywhereInThePackage(string name) =>
		Assert.Throws<InvalidDataException>(() => ArkModPackageReader.Read(Zip(Package("SomeMod/").Append((name, "x")))));

	[Fact]
	public void RejectsCaseCollisionsAndFileDirectoryCollisions()
	{
		Assert.Throws<InvalidDataException>(() => ArkModPackageReader.Read(Zip(Package().Append(("SOMEMOD.UPLUGIN", Descriptor())))));
		Assert.Throws<InvalidDataException>(() => ArkModPackageReader.Read(Zip(Package().Append(("Content", "not a directory")))));
	}

	[Theory]
	[InlineData(0xa1ff0000u)]
	[InlineData(0x400u)]
	public void RejectsUnixSymlinksAndWindowsReparseEntries(uint attributes)
	{
		string path = Zip(Package());
		using (ZipArchive zip = ZipFile.Open(path, ZipArchiveMode.Update))
			zip.GetEntry("SomeMod.uplugin")!.ExternalAttributes = unchecked((int)attributes);
		Assert.Throws<InvalidDataException>(() => ArkModPackageReader.Read(path));
	}

	[Theory]
	[InlineData("Windows")]
	[InlineData("WindowsNoEditor")]
	[InlineData("LinuxServer")]
	public void ClientOrWrongPlatformPackagesAreNotTreatedAsWindowsServerMods(string platform) =>
		Assert.Throws<InvalidDataException>(() => ArkModPackageReader.Read(Zip(Package(platform: platform))));

	[Theory]
	[InlineData(".ucas")]
	[InlineData(".utoc")]
	[InlineData(".pak")]
	public void IncompleteServerContentIsRejected(string missing) =>
		Assert.Throws<InvalidDataException>(() => ArkModPackageReader.Read(Zip(Package().Where(entry => !entry.Name.EndsWith(missing)))));

	[Theory]
	[InlineData("{}")]
	[InlineData("[]")]
	[InlineData("not JSON")]
	[InlineData("{\"Description\":\"a\",\"description\":\"b\"}")]
	public void InvalidOrAmbiguousMetadataIsRejected(string json) =>
		Assert.Throws<InvalidDataException>(() => ArkModPackageReader.Read(Zip(Package(json: json))));

	[Theory]
	[InlineData("https://www.curseforge.com/minecraft/mc-mods/example")]
	[InlineData("https://www.curseforge.com.evil.invalid/ark-survival-ascended/mods/example")]
	[InlineData("https://user@www.curseforge.com/ark-survival-ascended/mods/example")]
	public void ADescriptorMustIdentifyTheCorrectGameAndProvider(string url) =>
		Assert.Throws<InvalidDataException>(() => ArkModPackageReader.Read(Zip(Package(json: Descriptor(url: url)))));

	[Theory]
	[InlineData("Missing the ID")]
	[InlineData("Text&cf_ugcID=0")]
	[InlineData("Text&cf_ugcID=12 -mods=34")]
	[InlineData("Text&cf_ugcID=4294967296")]
	[InlineData("Text&cf_ugcID=12&cf_ugcID=34")]
	public void MissingOrAmbiguousIdsAreNeverInferredFromTheFilename(string description)
	{
		// Command-like prose does not get copied to launch arguments; an ID before it is numeric only.
		if (description.Contains("-mods"))
			Assert.Equal("12", Assert.Single(ArkModPackageReader.Read(Zip(Package(json: Descriptor(description: description))))).ModId);
		else
			Assert.Throws<InvalidDataException>(() => ArkModPackageReader.Read(Zip(Package(json: Descriptor(description: description)))));
	}

	[Fact]
	public void OversizedMetadataAndNestedArchivesFailWithoutExtracting()
	{
		Assert.Throws<InvalidDataException>(() => ArkModPackageReader.Read(Zip(Package(json: new string(' ', ArkModPackageReader.MaximumDescriptorBytes + 1)))));
		Assert.Throws<InvalidDataException>(() => ArkModPackageReader.Read(Zip([("SomeMod.zip", "nested content")])));
	}

	[Fact]
	public void SupportsUtf8BomAndCancellation()
	{
		string path = Zip(Package(json: "\uFEFF" + Descriptor()));
		Assert.Single(ArkModPackageReader.Read(path));
		Assert.Throws<OperationCanceledException>(() => ArkModPackageReader.Read(path, new CancellationToken(canceled: true)));
	}

	[Fact]
	public void ExcessiveNestingAndDescriptorCountsAreRejected()
	{
		Assert.Throws<InvalidDataException>(() => ArkModPackageReader.Read(Zip(Package(string.Concat(Enumerable.Repeat("wrapper/", 25))))));
		Assert.Throws<InvalidDataException>(() => ArkModPackageReader.Read(Zip(Enumerable.Range(1, 101).SelectMany(index => Package(index + "/", index.ToString())))));
	}

	[Fact]
	public void PreviewMergesIdsPreservingOrderAndClearsStaleResultsOnFailure() => WorkflowUiTest.Run(() =>
	{
		using ArkModPackageDialog dialog = new(["42", "123456"], 100);
		dialog.StartPosition = FormStartPosition.Manual;
		dialog.Location = new Point(-20000, -20000);
		dialog.Show();
		string package = Zip(Package("A/").Concat(Package("B/", "777")));
		WorkflowUiTest.Pump(dialog.ReadSourceAsync(package));
		Assert.Equal(new[] { "42", "123456", "777" }, dialog.ModIds);
		DataGridView grid = Assert.IsType<DataGridView>(dialog.Controls.Find("arkPackagePreview", true).Single());
		Assert.Equal(2, grid.Rows.Count);
		Assert.Equal(DataGridViewTriState.True, grid.DefaultCellStyle.WrapMode);
		RichTextBox details = Assert.IsType<RichTextBox>(dialog.Controls.Find("arkPackageDetails", true).Single());
		Assert.True(details.Multiline && details.WordWrap && details.ReadOnly);
		grid.CurrentCell = grid.Rows[1].Cells[0];
		Assert.Contains("B/SomeMod.uplugin", details.Text);
		Assert.True(dialog.Controls.Find("arkPackageReview", true).Single().Enabled);
		Assert.Contains("not copied or installed", dialog.Controls.Find("arkPackageHelp", true).Single().Text);
		Assert.NotEqual(DialogResult.OK, dialog.DialogResult);
		WorkflowUiTest.Pump(dialog.ReadSourceAsync(Zip([("bad.txt", "not a mod")])));
		Assert.Empty(dialog.ModIds);
		Assert.Empty(grid.Rows.Cast<DataGridViewRow>());
		Assert.Empty(details.Text);
		Assert.False(dialog.Controls.Find("arkPackageReview", true).Single().Enabled);
		dialog.Close();
	});

	[Fact]
	public void PreviewCannotExceedTheTargetIdLimit() => WorkflowUiTest.Run(() =>
	{
		using ArkModPackageDialog dialog = new(["42"], 1);
		WorkflowUiTest.Pump(dialog.ReadSourceAsync(Zip(Package())));
		Assert.Empty(dialog.ModIds);
		Assert.False(dialog.Controls.Find("arkPackageReview", true).Single().Enabled);
	});

	[Theory]
	[InlineData("en-US", true)]
	[InlineData("fr-FR", false)]
	[InlineData("de-DE", true)]
	[InlineData("es-ES", true)]
	[InlineData("en-US", true, ArkModPackageReader.Evolved)]
	[InlineData("de-DE", false, ArkModPackageReader.Evolved)]
	public void PackagePreviewUsesTheSynixThemeAndKeepsDetailsVisibleAtMinimumSize(string language, bool dark,
		string game = ArkModPackageReader.Ascended) => WorkflowUiTest.Run(() =>
	{
		string previousLanguage = LocalizationManager.CurrentLanguageCode;
		bool previousTheme = ThemeManager.IsDarkMode;
		try
		{
			LocalizationManager.Initialize(language);
			ThemeManager.Initialize(dark);
			using ArkModPackageDialog dialog = new(["42"], 100, game);
			dialog.StartPosition = FormStartPosition.Manual;
			dialog.Location = new Point(-20000, -20000);
			dialog.Show();
			WorkflowUiTest.Pump(dialog.ReadSourceAsync(game == ArkModPackageReader.Evolved
				? Zip(EvolvedPackage("Download/ModBundle/")) : Zip(Package("Download/ModBundle/SomeMod/"))));
			dialog.Size = dialog.MinimumSize;
			Application.DoEvents();
			dialog.Update();
			DataGridView grid = Assert.IsType<DataGridView>(dialog.Controls.Find("arkPackagePreview", true).Single());
			Assert.True(grid.Height >= 90, $"Preview grid was reduced to {grid.Height} pixels.");
			RichTextBox details = Assert.IsType<RichTextBox>(dialog.Controls.Find("arkPackageDetails", true).Single());
			Assert.Contains(game == ArkModPackageReader.Evolved ? "Download/ModBundle/9876543210.mod" : "Download/ModBundle/SomeMod/SomeMod.uplugin", details.Text);
			Assert.True(details.GetLineFromCharIndex(details.TextLength - 1) >= 2);
			Assert.True(details.GetPositionFromCharIndex(details.TextLength - 1).Y + details.Font.Height <= details.ClientSize.Height,
				"The selected mod's full example path must be visible without scrolling at minimum size.");
			foreach (string name in new[] { "arkPackageHelp", "arkPackageStatus" })
			{
				Label label = Assert.IsType<Label>(dialog.Controls.Find(name, true).Single());
				Size measured = TextRenderer.MeasureText(label.Text, label.Font, new Size(label.Width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
				Assert.True(label.Height >= measured.Height, $"{name} clipped for {language}: {label.Height} < {measured.Height}");
			}
			TextBox source = Assert.IsType<TextBox>(dialog.Controls.Find("arkPackageSource", true).Single());
			Assert.True(source.ReadOnly);
			Assert.Equal(BorderStyle.None, source.BorderStyle);
			Assert.Equal(SettingsPalette.Input, source.BackColor);
			string? previews = Environment.GetEnvironmentVariable("SYNIX_TEST_UI_PREVIEW_DIR");
			if (!string.IsNullOrWhiteSpace(previews))
			{
				Directory.CreateDirectory(previews);
				using Bitmap bitmap = new(dialog.Width, dialog.Height);
				dialog.DrawToBitmap(bitmap, new Rectangle(Point.Empty, dialog.Size));
				bitmap.Save(Path.Combine(previews, "ark-package-" + (game == ArkModPackageReader.Evolved ? "ase-" : "asa-") + language + ".png"), ImageFormat.Png);
			}
			dialog.Close();
		}
		finally { LocalizationManager.Initialize(previousLanguage); ThemeManager.Initialize(previousTheme); }
	});

	[Fact]
	public void ReaderIsOnlyOfferedForTheAsaCurseForgeArgumentTarget()
	{
		ModInstallTarget target = new() { Mode = ModTargetMode.ArgumentIds, ArgumentName = "-mods", ProviderName = "CurseForge" };
		Assert.True(ArkModPackageReader.Supports("ARK: Survival Ascended", target));
		Assert.False(ArkModPackageReader.Supports("ARK: Survival Evolved", target));
		Assert.False(ArkModPackageReader.Supports("Minecraft", target));
		Assert.False(ArkModPackageReader.Supports("ARK: Survival Ascended", new ModInstallTarget
			{ Mode = ModTargetMode.ArgumentIds, ArgumentName = "-unrelated", ProviderName = "CurseForge" }));
	}

	[Theory]
	[InlineData("")]
	[InlineData("Server/ShooterGame/Content/Mods/")]
	[InlineData("Unrelated/Wrapper/")]
	public void ReadsEvolvedBinaryDescriptorsWithMatchingContentAtDifferentDepths(string prefix)
	{
		ArkModPackageInfo mod = Assert.Single(ArkModPackageReader.Read(Zip(EvolvedPackage(prefix)), ArkModPackageReader.Evolved));
		Assert.Equal("9876543210", mod.ModId);
		Assert.Equal("Example ASE mod", mod.Name);
		Assert.EndsWith("9876543210.mod", mod.DescriptorPath);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void ReadsAnInternalDotModMarkerAndUnrealUtf16Strings(bool wide)
	{
		var mod = Assert.Single(ArkModPackageReader.Read(Zip(EvolvedPackage("Workshop/9876543210/WindowsNoEditor/", inlineMarker: true, wide: wide)), ArkModPackageReader.Evolved));
		Assert.Equal("9876543210", mod.ModId);
		Assert.Equal(wide ? "Mod de prueba 世界" : "Example ASE mod", mod.Name);
	}

	[Fact]
	public void EvolvedIdComesFromTheBinaryHeaderAndNumericFilenameMustAgree()
	{
		var data = EvolvedPackage().ToArray();
		byte[] descriptor = data[0].Content;
		System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(descriptor, 111);
		Assert.Throws<InvalidDataException>(() => ArkModPackageReader.Read(Zip(data), ArkModPackageReader.Evolved));
	}

	[Fact]
	public void DoesNotConfuseAscendedAndEvolvedOrAcceptMixedBundles()
	{
		string ascended = Zip(Package());
		string evolved = Zip(EvolvedPackage());
		Assert.Throws<InvalidDataException>(() => ArkModPackageReader.Read(ascended, ArkModPackageReader.Evolved));
		Assert.Throws<InvalidDataException>(() => ArkModPackageReader.Read(evolved, ArkModPackageReader.Ascended));
		Assert.Throws<InvalidDataException>(() => ArkModPackageReader.Read(Zip(EvolvedPackage().Concat(
			Package("ASA/").Select(entry => (entry.Name, Encoding.UTF8.GetBytes(entry.Content))))), ArkModPackageReader.Evolved));
	}

	[Theory]
	[InlineData("mod.info")]
	[InlineData(".uasset")]
	[InlineData(".mod")]
	public void EvolvedExportsRequireBothDescriptorAndPreparedContent(string missing) =>
		Assert.Throws<InvalidDataException>(() => ArkModPackageReader.Read(Zip(EvolvedPackage().Where(entry => !entry.Name.EndsWith(missing))), ArkModPackageReader.Evolved));

	[Fact]
	public void TruncatedAndOversizedEvolvedStringsAreRejected()
	{
		Assert.Throws<InvalidDataException>(() => ArkModPackageReader.Read(Zip([("123.mod", new byte[] { 1, 2, 3 })]), ArkModPackageReader.Evolved));
		var data = EvolvedPackage().ToArray();
		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(data[0].Content.AsSpan(8), int.MinValue);
		Assert.Throws<InvalidDataException>(() => ArkModPackageReader.Read(Zip(data), ArkModPackageReader.Evolved));
	}

	[Fact]
	public void EvolvedPackageIdsUseTheExistingIniWorkflowAndCanRollBack()
	{
		string? before = ModSystemCatalog.ExternalProfileRootOverride;
		try
		{
			ModSystemCatalog.ExternalProfileRootOverride = Path.Combine(_root, "profiles");
			Directory.CreateDirectory(ModSystemCatalog.ExternalProfileRootOverride);
			var server = new GameServer
				{ Game = ArkModPackageReader.Evolved, ServerName = "ase-package-test", InstallPath = Path.Combine(_root, "server"), Status = "Stopped", ExtraArgs = "-log" };
			Directory.CreateDirectory(server.InstallPath);
			ModInstallTarget target = Assert.Single(Assert.Single(ModSystemCatalog.GetProfiles(server.Game)).Targets);
			Assert.True(ArkModPackageReader.Supports(server.Game, target));
			Assert.False(ArkModPackageReader.Supports(ArkModPackageReader.Ascended, target));
			string[] ids = ArkModPackageReader.Read(Zip(EvolvedPackage()), server.Game).Select(mod => mod.ModId).ToArray();
			ProviderIdConfigurationChange change = ModPackageManager.ConfigureProviderIds(server, target, ids);
			Assert.Equal(ids, ModPackageManager.GetProviderIds(server, target));
			Assert.Equal("-log -automanagedmods", server.ExtraArgs);
			Assert.Contains("ActiveMods=9876543210", File.ReadAllText(Path.Combine(server.InstallPath, "ShooterGame/Saved/Config/WindowsServer/GameUserSettings.ini")));
			Assert.Contains("ModIDS=9876543210", File.ReadAllText(Path.Combine(server.InstallPath, "ShooterGame/Saved/Config/WindowsServer/Game.ini")));
			change.Rollback();
			Assert.Equal("-log", server.ExtraArgs);
			Assert.Empty(ModPackageManager.GetProviderIds(server, target));
		}
		finally { ModSystemCatalog.ExternalProfileRootOverride = before; }
	}

	private string Zip(IEnumerable<(string Name, byte[] Content)> entries)
	{
		string path = Path.Combine(_root, Guid.NewGuid().ToString("N") + ".zip");
		using ZipArchive zip = ZipFile.Open(path, ZipArchiveMode.Create);
		foreach ((string name, byte[] content) in entries)
		{
			using Stream stream = zip.CreateEntry(name).Open();
			stream.Write(content);
		}
		return path;
	}

	private static IEnumerable<(string Name, byte[] Content)> EvolvedPackage(string prefix = "", bool inlineMarker = false, bool wide = false)
	{
		using MemoryStream descriptor = new();
		using (BinaryWriter writer = new(descriptor, Encoding.UTF8, leaveOpen: true))
		{
			writer.Write(9876543210UL);
			UnrealString(writer, "ModName", wide);
			UnrealString(writer, string.Empty, wide);
			writer.Write(1);
			UnrealString(writer, "TestMap", wide);
			writer.Write(0xff22ff33u);
			writer.Write(2);
			writer.Write((byte)0);
			writer.Write(0);
		}
		yield return (prefix + (inlineMarker ? ".mod" : "9876543210.mod"), descriptor.ToArray());
		using MemoryStream info = new();
		using (BinaryWriter writer = new(info, Encoding.UTF8, leaveOpen: true))
		{
			UnrealString(writer, wide ? "Mod de prueba 世界" : "Example ASE mod", wide);
			writer.Write(1);
			UnrealString(writer, "TestMap", wide);
		}
		string content = prefix + (inlineMarker ? string.Empty : "9876543210/");
		yield return (content + "mod.info", info.ToArray());
		yield return (content + "PrimalGameData_Test.uasset", new byte[] { 1 });
	}

	private static void UnrealString(BinaryWriter writer, string value, bool wide)
	{
		byte[] bytes = (wide ? Encoding.Unicode : Encoding.UTF8).GetBytes(value + '\0');
		writer.Write(wide ? -(value.Length + 1) : bytes.Length);
		writer.Write(bytes);
	}

	private string Zip(IEnumerable<(string Name, string Content)> entries)
	{
		string path = Path.Combine(_root, Guid.NewGuid().ToString("N") + ".zip");
		using ZipArchive zip = ZipFile.Open(path, ZipArchiveMode.Create);
		foreach ((string name, string content) in entries)
		{
			using Stream stream = zip.CreateEntry(name).Open();
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			stream.Write(bytes);
		}
		return path;
	}

	private static IEnumerable<(string Name, string Content)> Package(string prefix = "", string id = "123456", string? json = null, string platform = "WindowsServer")
	{
		yield return (prefix + "SomeMod.uplugin", json ?? Descriptor(description: "Example&cf_ugcID=" + id));
		foreach (string extension in new[] { ".pak", ".ucas", ".utoc" })
			yield return (prefix + $"Content/Paks/{platform}/SomeMod{extension}", "fixture, not executable mod code");
	}

	private static string Descriptor(string description = "Example&cf_ugcID=123456", string url = "https://legacy.curseforge.com/ark-survival-ascended/mods/example") =>
		JsonSerializer.Serialize(new { FileVersion = 3, FriendlyName = "Example server mod", VersionName = "version-fingerprint", Description = description, MarketplaceURL = url });

	public void Dispose()
	{
		ModPathSafety.EnsureTreeHasNoLinks(_root);
		Directory.Delete(_root, recursive: true);
	}
}
