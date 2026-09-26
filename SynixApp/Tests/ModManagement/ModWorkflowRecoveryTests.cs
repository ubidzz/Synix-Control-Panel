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
using Synix_Control_Panel.SynixEngine;
using Synix_Control_Panel.SynixEngine.ModManagement;
using System.IO.Compression;
using System.Text.Json.Nodes;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class ModWorkflowRecoveryTests : IDisposable
{
	private readonly string _root = Path.Combine(Path.GetTempPath(), "SynixModWorkflow-" + Guid.NewGuid().ToString("N"));
	private readonly string? _previousData = ModPackageManager.DataRootOverride;
	private readonly GameServer _server;
	private readonly ModInstallTarget _target;
	private readonly ModSystemProfile _profile;
	private string Installed => Path.Combine(_server.InstallPath, "plugins", "Example.cs");
	private string Ledger => Path.Combine(ModPackageManager.GetServerDataFolder(_server), "installed.json");

	public ModWorkflowRecoveryTests()
	{
		_server = new() { Game = "Test Game", ServerName = "Workflow fixture", Status = "Stopped",
			InstallPath = Path.Combine(_root, "server") };
		Directory.CreateDirectory(Path.Combine(_server.InstallPath, "plugins"));
		ModPackageManager.DataRootOverride = Path.Combine(_root, "history");
		_target = new() { Id = "plugins", DisplayName = "Plugins", RelativePath = "plugins",
			Kind = ModContentKind.Plugin, Mode = ModTargetMode.FileImport, AllowedExtensions = [".cs"], AllowArchives = true };
		_profile = new() { Id = "fixture", DisplayName = "Fixture", SupportLevel = ModSystemSupportLevel.Managed,
			GameNames = ["Test Game"], Targets = [_target] };
	}

	[Fact]
	public void IdenticalImportsMustBeRolledBackNewestFirst()
	{
		ModImportResult first = Import("same content");
		ModImportResult second = Import("same content");
		Assert.Throws<InvalidOperationException>(() => ModPackageManager.Remove(_server, first.InstallationId));
		Assert.Equal("same content", File.ReadAllText(Installed));
		ModPackageManager.Remove(_server, second.InstallationId);
		Assert.Equal(first.InstallationId, Assert.Single(ModPackageManager.Scan(_server, _profile)).InstallationId);
		ModPackageManager.Remove(_server, first.InstallationId);
		Assert.False(File.Exists(Installed));
	}

	[Fact]
	public void CorruptOriginalStopsRollbackBeforeChangingAnyFiles()
	{
		File.WriteAllText(Installed, "original");
		ModImportResult installed = Import("replacement");
		File.WriteAllText(Path.Combine(installed.BackupFolder, "Example.cs"), "damaged backup");
		Assert.Throws<InvalidDataException>(() => ModPackageManager.Remove(_server, installed.InstallationId));
		Assert.Equal("replacement", File.ReadAllText(Installed));
		Assert.Equal(installed.InstallationId, Assert.Single(ModPackageManager.Scan(_server, _profile)).InstallationId);
	}

	[Fact]
	public void ClockChangesCannotReorderCommittedImports()
	{
		ModImportResult first = Import("first");
		ModImportResult second = Import("second");
		JsonNode document = JsonNode.Parse(File.ReadAllText(Ledger))!;
		document["installations"]![0]!["installedAtUtc"] = DateTime.UtcNow.AddDays(1);
		File.WriteAllText(Ledger, document.ToJsonString());
		Assert.Equal(second.InstallationId, Assert.Single(ModPackageManager.Scan(_server, _profile)).InstallationId);
		Assert.Throws<InvalidOperationException>(() => ModPackageManager.Remove(_server, first.InstallationId));
		ModPackageManager.Remove(_server, second.InstallationId);
		Assert.Equal("first", File.ReadAllText(Installed));
	}

	[Fact]
	public void LegacyHistoryWithoutOriginalHashStillRestores()
	{
		File.WriteAllText(Installed, "original");
		ModImportResult installed = Import("replacement");
		JsonNode document = JsonNode.Parse(File.ReadAllText(Ledger))!;
		document["installations"]![0]!["files"]![0]!.AsObject().Remove("previousSha256");
		File.WriteAllText(Ledger, document.ToJsonString());
		ModPackageManager.Remove(_server, installed.InstallationId);
		Assert.Equal("original", File.ReadAllText(Installed));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void FailedHistorySaveRestoresTheStateBeforeRemoval(bool fileWasMissing)
	{
		File.WriteAllText(Installed, "original");
		ModImportResult installed = Import("replacement");
		if (fileWasMissing) File.Delete(Installed);
		using (FileStream historyLock = new(Ledger, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			Assert.ThrowsAny<IOException>(() => ModPackageManager.Remove(_server, installed.InstallationId));
			Assert.Equal(!fileWasMissing, File.Exists(Installed));
			if (!fileWasMissing) Assert.Equal("replacement", File.ReadAllText(Installed));
		}
		ModPackageManager.Remove(_server, installed.InstallationId);
		Assert.Equal("original", File.ReadAllText(Installed));
	}

	[Fact]
	public void PartialPackageOverlapDisablesRollbackOfEveryOlderPackageMember()
	{
		string archivePath = Path.Combine(_root, "package.zip");
		using (ZipArchive archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
		{
			foreach (string file in new[] { "Example.cs", "Other.cs" })
			{
				using StreamWriter writer = new(archive.CreateEntry(file).Open());
				writer.Write("same content");
			}
		}
		ModImportResult first = ModPackageManager.Import(_server, _profile, _target, archivePath, securityContext: new(false));
		ModImportResult second = Import("same content");
		var inventory = ModPackageManager.Scan(_server, _profile);
		Assert.False(inventory.Single(item => item.Name == "Other").CanRemove);
		Assert.True(inventory.Single(item => item.Name == "Example").CanRemove);
		Assert.All(inventory, item => Assert.Equal(_target.Id, item.TargetId));
		Assert.Throws<InvalidOperationException>(() => ModPackageManager.Remove(_server, first.InstallationId));
		ModPackageManager.Remove(_server, second.InstallationId);
		Assert.All(ModPackageManager.Scan(_server, _profile), item => Assert.True(item.CanRemove));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void FailedProviderSaveRestoresArgumentsAndReleasesOperation(bool throws)
	{
		ModInstallTarget target = new() { Id = "ids", Mode = ModTargetMode.ArgumentIds, ArgumentName = "-mods" };
		_server.ExtraArgs = "-mods=123 -log";
		Assert.ThrowsAny<IOException>(() => ModPackageManager.SaveProviderIds(_server, target, ["456"],
			() => throws ? throw new IOException("Test save failed") : false));
		Assert.Equal("-mods=123 -log", _server.ExtraArgs);
		ModPackageManager.SaveProviderIds(_server, target, ["789"], () => true);
		Assert.Equal(["789"], ModPackageManager.GetProviderIds(_server, target));
	}

	[Theory]
	[InlineData("scan")]
	[InlineData("import")]
	[InlineData("remove")]
	[InlineData("provider")]
	public async Task ModWorkDoesNotOverlapAnIndependentServerOperation(string action)
	{
		ModImportResult installed = Import("fixture");
		using ServerOperationLease other = ServerOperationCoordinator.TryBegin(_server, ServerOperationKind.Restore);
		Assert.True(other.Acquired);
		Task blocked;
		using (ExecutionContext.SuppressFlow())
		{
			blocked = Task.Run(() => Assert.Throws<InvalidOperationException>(() =>
			{
				switch (action)
				{
					case "scan": ModPackageManager.Scan(_server, _profile); break;
					case "import": Import("attempt"); break;
					case "remove": ModPackageManager.Remove(_server, installed.InstallationId); break;
					default: ModPackageManager.ConfigureProviderIds(_server,
						new ModInstallTarget { Mode = ModTargetMode.ArgumentIds, ArgumentName = "-mods" }, ["123"]); break;
				}
			}));
		}
		await blocked;
		Assert.Equal("fixture", File.ReadAllText(Installed));
	}

	private ModImportResult Import(string content)
	{
		string path = Path.Combine(_root, "Example.cs");
		File.WriteAllText(path, content);
		return ModPackageManager.Import(_server, _profile, _target, path, securityContext: new(false));
	}

	public void Dispose()
	{
		ModPackageManager.DataRootOverride = _previousData;
		ModPathSafety.EnsureTreeHasNoLinks(_root);
		Directory.Delete(_root, recursive: true);
	}
}
