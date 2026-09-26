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
using Synix_Control_Panel.SynixApp.Database.GameConfigurations;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixApp.UI.ServerManagement;
using Synix_Control_Panel.SynixEngine;
using Synix_Control_Panel.SynixEngine.Minecraft;
using Synix_Control_Panel.SynixEngine.ModManagement;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Net;
using System.Security.Cryptography;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class MinecraftWorkspaceTests : IDisposable
{
	private readonly string _root = Path.Combine(Path.GetTempPath(), "SynixMinecraftWorkspace-" + Guid.NewGuid().ToString("N"));
	private readonly string? _previousData = ModPackageManager.DataRootOverride;
	private readonly GameServer[] _previousServers = ServerRegistry.Snapshot().ToArray();
	private readonly GameServer _server;

	public MinecraftWorkspaceTests()
	{
		Directory.CreateDirectory(_root);
		ModPackageManager.DataRootOverride = Path.Combine(_root, "history");
		ServerRegistry.Servers.Clear();
		_server = new() { Game = "Minecraft", ServerName = "Local identity", InstallPath = Path.Combine(_root, "server"),
			Status = "Stopped", Port = 45001, QueryPort = 45002, RconPort = 45003, GameVersion = "1.21.8",
			MinecraftLoader = "Fabric", MinecraftLoaderVersion = "0.16.14", WorldName = "world",
			GameMode = "Survival", MaxPlayers = 20, RequiredJavaVersion = 21, EnableMinecraftManagementProtocol = false };
		Directory.CreateDirectory(_server.InstallPath);
	}

	private string FileAt(string relative, string text)
	{
		string path = ModPathSafety.Resolve(_server.InstallPath, relative);
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.WriteAllText(path, text, new UTF8Encoding(false));
		return path;
	}

	private string Archive(params (string Path, string Text)[] entries)
	{
		string path = Path.Combine(_root, Guid.NewGuid().ToString("N") + ".zip");
		using ZipArchive zip = ZipFile.Open(path, ZipArchiveMode.Create);
		foreach ((string name, string text) in entries)
		{
			using StreamWriter writer = new(zip.CreateEntry(name).Open(), new UTF8Encoding(false));
			writer.Write(text);
		}
		return path;
	}

	private MinecraftChangeReceipt Change(string relative, string text, Action<int>? after = null)
	{
		using MinecraftStaging stage = new();
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		using MemoryStream stream = new(bytes);
		stage.Add(relative, stream, bytes.Length);
		return MinecraftContentTransactions.Apply(_server, "Fixture", "config", stage.Changes(_server), after);
	}

	[Fact]
	public void PropertiesSaveUpdatesMappedSettingsAndProtectsSecretWithoutRenamingIdentity()
	{
		string path = FileAt("server.properties", "# keep me\nserver-port=45001\nlevel-name=world\n");
		string rootBefore = MinecraftContentTransactions.Root(_server);
		string updated = "# keep me\nserver-port=45011\nquery.port=45012\nmax-players=37\nlevel-name=New World\nlevel-seed=abc\n" +
			"gamemode=creative\nmotd=Welcome\\u0021\nrcon.port=45013\nenable-rcon=true\nrcon.password=fixture-only-secret\nview-distance=9\n";
		int saves = 0;
		MinecraftConfigurationSync.Save(_server, path, updated, MinecraftContentTransactions.HashFile(path), () => { saves++; return true; });
		Assert.Equal(1, saves);
		Assert.Equal(45011, _server.Port); Assert.Equal(45012, _server.QueryPort);
		Assert.Equal(37, _server.MaxPlayers); Assert.Equal("New World", _server.WorldName);
		Assert.Equal("abc", _server.WorldSeed); Assert.Equal("Creative", _server.GameMode);
		Assert.Equal("Welcome!", _server.MinecraftAdvertisedName); Assert.Equal("Local identity", _server.ServerName);
		Assert.True(Core.IsProtected(_server.RconPassword)); Assert.Equal("fixture-only-secret", Core.Reveal(_server.RconPassword));
		Assert.Equal(rootBefore, MinecraftContentTransactions.Root(_server));
		Assert.Equal(updated, File.ReadAllText(path));
		ConfigurationContext context = new(_server, Core.RevealServerPasswords(_server), "identity", "", "");
		MinecraftConfiguration definition = new();
		Assert.Equal("45011", definition.Bindings.Single(b => b.Key == "server-port").Value(context));
		Assert.Equal("Welcome!", definition.Bindings.Single(b => b.Key == "motd").Value(context));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void FailedPersistenceRestoresFileAndEntry(bool throws)
	{
		string path = FileAt("server.properties", "max-players=20\nlevel-name=world\n");
		Assert.ThrowsAny<Exception>(() => MinecraftConfigurationSync.Save(_server, path, "max-players=30\n",
			MinecraftContentTransactions.HashFile(path), () => throws ? throw new IOException("fixture") : false));
		Assert.Equal(20, _server.MaxPlayers);
		Assert.Equal("max-players=20\nlevel-name=world\n", File.ReadAllText(path));
		Assert.All(MinecraftContentTransactions.History(_server), receipt => Assert.Equal("Undone", receipt.State));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void ExternalSynchronizationRollsBackModelWhenPersistenceFails(bool throws)
	{
		FileAt("server.properties", "max-players=31\n");
		Assert.ThrowsAny<Exception>(() => MinecraftConfigurationSync.Synchronize(_server, () => throws ? throw new IOException("fixture") : false));
		Assert.Equal(20, _server.MaxPlayers);
		Assert.Equal("max-players=31\n", File.ReadAllText(Path.Combine(_server.InstallPath, "server.properties")));
	}

	[Theory]
	[InlineData("server-port=65536")]
	[InlineData("max-players=not a number")]
	[InlineData("enable-rcon=maybe")]
	[InlineData("level-name=../outside")]
	[InlineData("management-server-enabled=true\nmanagement-server-host=0.0.0.0")]
	[InlineData("gamemode=not a mode")]
	public void InvalidMappedValueChangesNeitherFileNorEntry(string text)
	{
		string path = FileAt("server.properties", "level-name=world\n");
		Assert.ThrowsAny<Exception>(() => MinecraftConfigurationSync.Save(_server, path, text, MinecraftContentTransactions.HashFile(path), () => true));
		Assert.Equal("level-name=world\n", File.ReadAllText(path)); Assert.Equal(45001, _server.Port);
		Assert.Empty(MinecraftContentTransactions.History(_server));
	}

	[Fact]
	public void SaveRefusesAFileChangedSinceEditorOpened()
	{
		string path = FileAt("server.properties", "max-players=20");
		string? hash = MinecraftContentTransactions.HashFile(path);
		File.WriteAllText(path, "max-players=25");
		Assert.ThrowsAny<Exception>(() => MinecraftConfigurationSync.Save(_server, path, "max-players=30", hash, () => true));
		Assert.Equal("max-players=25", File.ReadAllText(path)); Assert.Equal(20, _server.MaxPlayers);
	}

	[Fact]
	public void BedrockUpdatesNativePortsAndNameWithoutJavaSettings()
	{
		_server.MinecraftEdition = "Bedrock"; _server.EnableRcon = false;
		string path = FileAt("server.properties", "level-name=world");
		MinecraftConfigurationSync.Save(_server, path, "server-name=Home\nserver-port=19142\nserver-portv6=19143\n" +
			"level-name=Bedrock World\ngamemode=adventure\nmax-players=12", MinecraftContentTransactions.HashFile(path), () => true);
		Assert.Equal(19142, _server.Port); Assert.Equal(19143, _server.QueryPort);
		Assert.Equal("Home", _server.MinecraftAdvertisedName); Assert.Equal("Adventure", _server.GameMode);
		Assert.False(_server.EnableMinecraftManagementProtocol); Assert.False(_server.EnableRcon);
	}

	[Fact]
	public void JavaQueryMayShareGameplayPortButTcpManagementCannot()
	{
		FileAt("server.properties", "server-port=45001\nquery.port=45001\nenable-query=true\n");
		MinecraftConfigurationSync.Synchronize(_server, () => true);
		Assert.Equal(_server.Port, _server.QueryPort);
		FileAt("server.properties", "server-port=45001\nrcon.port=45001\nenable-rcon=true");
		Assert.ThrowsAny<Exception>(() => MinecraftConfigurationSync.Synchronize(_server, () => true));
	}

	[Fact]
	public void PropertiesCodecHandlesEscapesContinuationSeparatorsAndLastValue()
	{
		var values = MinecraftConfigurationSync.ReadProperties("! comment\nlevel\\u002dname : First\nmotd = hello\\\n  world\\u0021\nlevel-name=Second\n");
		Assert.Equal("Second", values["level-name"]); Assert.Equal("helloworld!", values["motd"]);
		Assert.Equal("New World", MinecraftConfigurationSync.ReadProperties(
			MinecraftConfigurationSync.SetProperty("level\\u002dname: Old\n", "level-name", "New World"))["level-name"]);
	}

	[Fact]
	public void ConfigSavePreservesUnicodeEncodingAndBom()
	{
		string path = FileAt("server.properties", "");
		File.WriteAllText(path, "motd=été\n", Encoding.Unicode);
		MinecraftConfigurationSync.Save(_server, path, "motd=été changé\n", MinecraftContentTransactions.HashFile(path), () => true);
		Assert.Equal(new byte[] { 0xFF, 0xFE }, File.ReadAllBytes(path).Take(2));
		Assert.Equal("été changé", _server.MinecraftAdvertisedName);
	}

	[Fact]
	public void MultilineJavaMotdRoundTripsThroughSynixWithoutBecomingInvalid()
	{
		FileAt("server.properties", "motd=First line\\nSecond line");
		MinecraftConfigurationSync.Synchronize(_server, () => true);
		Assert.Equal("First line\nSecond line", _server.MinecraftAdvertisedName);
		MinecraftConfiguration definition = new();
		ConfigurationContext context = new(_server, Core.RevealServerPasswords(_server), "identity", "", "");
		Assert.Equal("First line\\nSecond line", definition.Bindings.Single(binding => binding.Key == "motd").Value(context));
	}

	[Fact]
	public void JournalRestoresPreviousFileAndKeepsUnrelatedFiles()
	{
		string file = FileAt("config/example.toml", "before");
		string untouched = FileAt("world/region/keep.mca", "world fixture");
		MinecraftChangeReceipt receipt = Change("config/example.toml", "after");
		Assert.Equal("after", File.ReadAllText(file));
		MinecraftContentTransactions.Undo(_server, receipt.Id);
		Assert.Equal("before", File.ReadAllText(file)); Assert.Equal("world fixture", File.ReadAllText(untouched));
		Assert.Equal("Undone", MinecraftContentTransactions.History(_server).Single().State);
	}

	[Fact]
	public void InterruptedApplyRecoversAllOriginalBytes()
	{
		string path = FileAt("config/test.json", "before");
		Assert.Throws<IOException>(() => Change("config/test.json", "after", _ => throw new IOException("simulated interruption")));
		Assert.Equal("before", File.ReadAllText(path));
		Assert.Equal("Undone", MinecraftContentTransactions.History(_server).Single().State);
	}

	[Fact]
	public void ChangedFilesCannotBeSilentlyOverwrittenByUndo()
	{
		string path = FileAt("config/test.json", "before");
		MinecraftChangeReceipt receipt = Change("config/test.json", "after");
		File.WriteAllText(path, "user edit");
		Assert.ThrowsAny<Exception>(() => MinecraftContentTransactions.Undo(_server, receipt.Id));
		Assert.Equal("user edit", File.ReadAllText(path));
	}

	[Fact]
	public void InterruptedRecoveryBlocksStartUntilFileAndEntryAreReconciled()
	{
		string path = FileAt("server.properties", "max-players=20");
		MinecraftChangeReceipt receipt = Change("server.properties", "max-players=35");
		Assert.Throws<IOException>(() => MinecraftContentTransactions.Undo(_server, receipt.Id, synchronize: () => throw new IOException("simulated save failure")));
		Assert.Equal("Prepared", MinecraftContentTransactions.History(_server).Single().State);
		Assert.ThrowsAny<Exception>(() => MinecraftContentTransactions.EnsureReadyToStart(_server));
		MinecraftContentTransactions.Undo(_server, receipt.Id, synchronize: () => MinecraftConfigurationSync.SynchronizeRestored(_server, () => true));
		Assert.Equal("Undone", MinecraftContentTransactions.History(_server).Single().State);
		Assert.Equal("max-players=20", File.ReadAllText(path));
		MinecraftContentTransactions.EnsureReadyToStart(_server);
	}

	[Fact]
	public void PendingRecoveryCannotBeHiddenByFirstThousandHistoryEntries()
	{
		MinecraftChangeReceipt receipt = Change("config/a.txt", "a");
		string journal = Path.Combine(MinecraftContentTransactions.Root(_server), receipt.Id, "change.json");
		receipt.State = "Prepared"; File.WriteAllText(journal, JsonSerializer.Serialize(receipt));
		Assert.ThrowsAny<Exception>(() => Change("config/b.txt", "b"));
		Assert.False(File.Exists(Path.Combine(_server.InstallPath, "config/b.txt")));
	}

	[Fact]
	public void RuntimeProfileMismatchBlocksStart()
	{
		MinecraftRuntimeUpdater.WriteInstalled(_server);
		MinecraftContentTransactions.EnsureReadyToStart(_server);
		_server.MinecraftLoader = "Paper";
		Assert.ThrowsAny<Exception>(() => MinecraftContentTransactions.EnsureReadyToStart(_server));
	}

	[Fact]
	public void FullBackupRestoreRetiresObsoletePendingJournalsAndSynchronizesSettings()
	{
		MinecraftChangeReceipt receipt = Change("server.properties", "max-players=35");
		receipt.State = "Prepared";
		File.WriteAllText(Path.Combine(MinecraftContentTransactions.Root(_server), receipt.Id, "change.json"), JsonSerializer.Serialize(receipt));
		// Simulate the bytes returned by a successful older full backup, not an individual undo.
		FileAt("server.properties", "max-players=12");
		MinecraftConfigurationSync.SynchronizeRestored(_server, () => true, fullRestore: true);
		Assert.Equal(12, _server.MaxPlayers);
		Assert.Equal("Superseded", MinecraftContentTransactions.History(_server).Single().State);
		MinecraftContentTransactions.EnsureReadyToStart(_server);
		Assert.ThrowsAny<Exception>(() => MinecraftContentTransactions.Undo(_server, receipt.Id));
	}

	[Fact]
	public void FullBackupRestorePreservesButRetiresAnUnreadableJournal()
	{
		MinecraftChangeReceipt receipt = Change("config/example.toml", "value=1");
		File.WriteAllText(Path.Combine(MinecraftContentTransactions.Root(_server), receipt.Id, "change.json"), "interrupted metadata");
		FileAt("server.properties", "max-players=15");
		MinecraftConfigurationSync.SynchronizeRestored(_server, () => true, fullRestore: true);
		Assert.Equal(15, _server.MaxPlayers);
		MinecraftContentTransactions.EnsureReadyToStart(_server);
		string archived = Directory.EnumerateDirectories(ModPackageManager.GetServerDataFolder(_server), "MinecraftChanges-fullrestore-*").Single();
		Assert.Equal("interrupted metadata", File.ReadAllText(Path.Combine(archived, receipt.Id, "change.json")));
	}

	[Theory]
	[InlineData("../outside.txt")]
	[InlineData("config/../../outside.txt")]
	[InlineData("config/CON.txt")]
	[InlineData("config/a.txt:stream")]
	public void UnsafeArchivePathsAreRejected(string path)
	{
		string zip = Archive((path, "fixture"));
		using ZipArchive archive = ZipFile.OpenRead(zip);
		Assert.ThrowsAny<Exception>(() => MinecraftStaging.ValidateArchive(archive));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void ImportsAndSwitchesWorldOnlyForMatchingEdition(bool bedrock)
	{
		_server.MinecraftEdition = bedrock ? "Bedrock" : "Java";
		string zip = Archive(("Export/level.dat", "level fixture"), (bedrock ? "Export/db/CURRENT" : "Export/region/r.0.0.mca", "world fixture"));
		using MinecraftStaging stage = MinecraftWorlds.PrepareImport(_server, zip, "Imported");
		MinecraftChangeReceipt receipt = MinecraftContentTransactions.Apply(_server, "Imported", "world", stage.Changes(_server));
		Assert.Single(MinecraftWorlds.List(_server));
		FileAt("server.properties", "level-name=world\nmax-players=20\n");
		MinecraftWorlds.Switch(_server, "Imported", () => true);
		Assert.Equal("Imported", _server.WorldName);
		Assert.True(MinecraftWorlds.List(_server).Single().Active);
		Assert.ThrowsAny<Exception>(() => MinecraftContentTransactions.Undo(_server, receipt.Id));
	}

	[Fact]
	public void WorldSwitchPreservesPreviousConfigurationOnSaveFailure()
	{
		FileAt("Imported/level.dat", "fixture");
		string path = FileAt("server.properties", "level-name=world\n");
		Assert.ThrowsAny<Exception>(() => MinecraftWorlds.Switch(_server, "Imported", () => false));
		Assert.Equal("world", _server.WorldName); Assert.Equal("level-name=world\n", File.ReadAllText(path));
	}

	[Fact]
	public async Task ServerPackImportsContentButNotLaunchersWorldsOrServerSettings()
	{
		string zip = Archive(("Pack/mods/example.jar", "jar fixture"), ("Pack/config/example.toml", "value=1"),
			("Pack/start.bat", "not executed"), ("Pack/world/level.dat", "untouched"), ("Pack/server.properties", "server-port=1"));
		using MinecraftPreparedPack pack = await MinecraftModpacks.PrepareAsync(_server, zip);
		Assert.Equal(2, pack.Changes.Count); Assert.Equal(3, pack.Notes.Count);
		Assert.DoesNotContain(pack.Changes, change => change.RelativePath == "server.properties");
		Assert.False(File.Exists(Path.Combine(_server.InstallPath, "mods/example.jar")));
	}

	[Fact]
	public async Task ModrinthUsesServerOverridesSkipsClientAndOptionalFiles()
	{
		string index = """{"formatVersion":1,"game":"minecraft","name":"Fixture","versionId":"1","dependencies":{"minecraft":"1.21.8","fabric-loader":"0.16.14"},"files":[{"path":"mods/client.jar","env":{"server":"unsupported"}},{"path":"mods/optional.jar","env":{"server":"optional"}}]}""";
		string zip = Archive(("modrinth.index.json", index), ("overrides/config/a.toml", "common"),
			("server-overrides/config/a.toml", "server"), ("client-overrides/config/a.toml", "client"));
		using MinecraftPreparedPack pack = await MinecraftModpacks.PrepareAsync(_server, zip);
		Assert.Single(pack.Changes);
		Assert.Equal("server", File.ReadAllText(pack.Changes[0].Source!));
		Assert.Contains("mods/client.jar", pack.Notes); Assert.Contains("mods/optional.jar", pack.Notes);
	}

	[Fact]
	public async Task PackUpgradeRemovesOnlyUnchangedPreviouslyManagedJars()
	{
		string first = Archive(("mods/old.jar", "old"), ("config/kept.toml", "config"));
		using MinecraftPreparedPack old = await MinecraftModpacks.PrepareAsync(_server, first);
		old.Name = "Fixture";
		MinecraftContentTransactions.Apply(_server, "Fixture", "pack:Fixture", old.Changes);
		FileAt("mods/user.jar", "user");
		using MinecraftPreparedPack replacement = new() { Name = "Fixture" };
		using MemoryStream data = new(Encoding.UTF8.GetBytes("new"));
		replacement.Staging.Add("mods/new.jar", data, data.Length);
		replacement.Freeze(_server);
		Assert.Contains(replacement.Changes, change => change.RelativePath == "mods/old.jar" && change.Source == null);
		Assert.DoesNotContain(replacement.Changes, change => change.RelativePath == "mods/user.jar" || change.RelativePath == "config/kept.toml");
		MinecraftContentTransactions.Apply(_server, "Fixture", "pack:Fixture", replacement.Changes);
		Assert.False(File.Exists(Path.Combine(_server.InstallPath, "mods/old.jar")));
		Assert.True(File.Exists(Path.Combine(_server.InstallPath, "config/kept.toml")));
	}

	[Theory]
	[InlineData("mods/A.jar", true)]
	[InlineData("Mods/A.jar", true)]
	[InlineData("config/a.toml", true)]
	[InlineData("kubejs/server_scripts/a.js", true)]
	[InlineData("config/a.js", false)]
	[InlineData("mods/a.dll", false)]
	[InlineData("mods/sub/a.jar", false)]
	[InlineData("server.properties", false)]
	[InlineData("world/level.dat", false)]
	public void PackContentScopeIsExplicit(string path, bool allowed) => Assert.Equal(allowed, MinecraftModpacks.IsContentPath(path));

	[Theory]
	[InlineData("https://cdn.modrinth.com/a.jar", true)]
	[InlineData("https://github.com/a/b/releases/download/test/a.jar", true)]
	[InlineData("https://example.com/a.jar", false)]
	[InlineData("https://cdn.modrinth.com.attacker.example/a.jar", false)]
	[InlineData("http://cdn.modrinth.com/a.jar", false)]
	[InlineData("https://user@cdn.modrinth.com/a.jar", false)]
	[InlineData("https://127.0.0.1/a.jar", false)]
	public void ModrinthDownloadBoundaryIsExplicit(string url, bool allowed) => Assert.Equal(allowed, MinecraftModpacks.IsAllowedDownload(new Uri(url)));

	[Theory]
	[InlineData("Paper", """[{"id":4,"channel":"STABLE"},{"id":5,"channel":"EXPERIMENTAL"},{"id":2,"channel":"STABLE"}]""", "4,2")]
	[InlineData("Purpur", """{"builds":{"all":["9","2","9"]}}""", "9,2")]
	public void RuntimeBuildSelectionUsesProviderMetadata(string loader, string json, string expected)
	{
		using JsonDocument doc = JsonDocument.Parse(json);
		Assert.Equal(expected, string.Join(",", MinecraftPluginRuntime.ParseBuilds(loader, doc.RootElement)));
		Assert.Equal(loader, MinecraftMetadataService.NormalizeLoader(loader));
	}

	[Theory]
	[InlineData("1.2.3", ">=1.2 <2", true)]
	[InlineData("1.2.3", "[1.2,2)", true)]
	[InlineData("2.0", "[1.2,2)", false)]
	[InlineData("1.2.3", "1.2.*", true)]
	[InlineData("1.2.3", "1.3.*", false)]
	[InlineData("1.2.3", "^1.2", null)]
	[InlineData("1.2.3+build", ">=1.2", null)]
	public void UnknownVersionRangesAreNotMarkedCompatible(string version, string range, bool? expected) =>
		Assert.Equal(expected, MinecraftCompatibility.MatchesVersion(version, range));

	[Fact]
	public void MetadataChecksIdentifyWrongLoaderClientOnlyDuplicateAndMissingDependencies()
	{
		MinecraftAddOn[] addons = [
			new("mods/a.jar", "a", "1", "Fabric", false, [new("missing", ">=1")]),
			new("mods/b.jar", "a", "2", "Forge", true, [])];
		var findings = MinecraftCompatibility.Evaluate(_server, addons);
		Assert.Equal(4, findings.Count);
		Assert.Equal(3, findings.Count(finding => finding.DefiniteProblem));
	}

	[Fact]
	public void JarMetadataIsReadWithoutExecutingAndInactiveFoldersDoNotInterfere()
	{
		string zip = Archive(("fabric.mod.json", """{"id":"example","version":"1.2.3","depends":{"fabricloader":">=0.16","minecraft":"1.21.8"}}"""));
		Directory.CreateDirectory(Path.Combine(_server.InstallPath, "mods"));
		File.Copy(zip, Path.Combine(_server.InstallPath, "mods/example.jar"));
		var report = MinecraftCompatibility.Scan(_server);
		Assert.Single(report.AddOns); Assert.Empty(report.Findings);
		_server.MinecraftLoader = "Paper";
		Assert.Empty(MinecraftCompatibility.Scan(_server).AddOns);
	}

	[Fact]
	public void DiagnosticsDoesNotTreatNormalRequiresTextAsCrash()
	{
		Assert.Empty(MinecraftLogDiagnostics.Analyze("This command requires operator permission."));
		Assert.Single(MinecraftLogDiagnostics.Analyze("java.lang.OutOfMemoryError: Java heap space"));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task DownloadsRequireExpectedBytesAndHash(bool corruptHash)
	{
		byte[] bytes = Encoding.UTF8.GetBytes("fixture download");
		using FixtureHttpHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) });
		using HttpClient client = new(handler);
		string target = Path.Combine(_root, "download.jar");
		string hash = corruptHash ? new string('0', 128) : Convert.ToHexString(SHA512.HashData(bytes));
		Task download = MinecraftModpacks.DownloadAsync(new Uri("https://cdn.modrinth.com/fixture.jar"), target, bytes.Length, hash, default, client);
		if (corruptHash) await Assert.ThrowsAnyAsync<InvalidDataException>(() => download);
		else { await download; Assert.Equal(bytes, File.ReadAllBytes(target)); }
		Assert.Equal(1, handler.Requests);
	}

	[Fact]
	public async Task DownloadRedirectCannotLeaveApprovedHosts()
	{
		using FixtureHttpHandler handler = new(_ =>
		{
			HttpResponseMessage response = new(HttpStatusCode.Redirect);
			response.Headers.Location = new Uri("https://127.0.0.1/private");
			return response;
		});
		using HttpClient client = new(handler);
		await Assert.ThrowsAnyAsync<InvalidDataException>(() => MinecraftModpacks.DownloadAsync(new Uri("https://cdn.modrinth.com/fixture.jar"),
			Path.Combine(_root, "download.jar"), 1, new string('0', 128), default, client));
		Assert.Equal(1, handler.Requests);
		Assert.False(File.Exists(Path.Combine(_root, "download.jar")));
	}

	[Fact]
	public async Task DownloadChecksActualSizeWhenNoLengthHeaderIsPresent()
	{
		using FixtureHttpHandler handler = new(_ =>
		{
			HttpResponseMessage response = new(HttpStatusCode.OK) { Content = new UnknownLengthContent() };
			Assert.Null(response.Content.Headers.ContentLength);
			return response;
		});
		using HttpClient client = new(handler);
		await Assert.ThrowsAnyAsync<InvalidDataException>(() => MinecraftModpacks.DownloadAsync(new Uri("https://cdn.modrinth.com/fixture.jar"),
			Path.Combine(_root, "download.jar"), 2, new string('0', 128), default, client));
		Assert.True(!File.Exists(Path.Combine(_root, "download.jar")) || new FileInfo(Path.Combine(_root, "download.jar")).Length <= 2);
	}

	private sealed class FixtureHttpHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
	{
		internal int Requests;
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
		{ token.ThrowIfCancellationRequested(); Requests++; return Task.FromResult(respond(request)); }
	}

	private sealed class UnknownLengthContent : HttpContent
	{
		protected override bool TryComputeLength(out long length) { length = 0; return false; }
		protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => stream.WriteAsync(new byte[] { 1, 2, 3 }).AsTask();
	}

	[Theory]
	[InlineData("Steve", "Operator", "op Steve")]
	[InlineData("Alex_123", "Ban", "ban Alex_123")]
	public void PlayerActionsAreAllowListed(string player, string action, string command) =>
		Assert.Equal(command, MinecraftControlCenter.BuildPlayerCommand(_server, player, action));

	[Theory]
	[InlineData("Steve\nstop")]
	[InlineData("@a")]
	[InlineData("Steve;stop")]
	public void PlayerNamesCannotAddAnotherCommand(string player) =>
		Assert.ThrowsAny<Exception>(() => MinecraftControlCenter.BuildPlayerCommand(_server, player, "Operator"));

	public void Dispose()
	{
		ServerRegistry.Servers.Clear();
		foreach (GameServer server in _previousServers) ServerRegistry.Servers.Add(server);
		ModPackageManager.DataRootOverride = _previousData;
		Assert.Equal(Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.GetTempPath())), Path.GetDirectoryName(Path.GetFullPath(_root)));
		Assert.StartsWith("SynixMinecraftWorkspace-", Path.GetFileName(_root));
		ModPathSafety.EnsureTreeHasNoLinks(_root);
		Directory.Delete(_root, true);
	}
}
