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
using System.Text.Json;
using System.IO.Compression;
using System.Reflection;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class InterruptedServerRestoreTests : IDisposable
{
	private readonly string _root = Path.Combine(Path.GetTempPath(), "SynixRestoreRecovery-" + Guid.NewGuid().ToString("N"));
	private readonly string? _previousJournals = Core.RestoreJournalFolderOverride;
	private readonly string _id = Guid.NewGuid().ToString("N");
	private string Install => Path.Combine(_root, "server");
	private string Operation => Path.Combine(_root, ".synix-restore-server-" + _id);
	private string Rollback => Install + ".synix-restore-rollback-" + _id;
	private string Journal => Path.Combine(Core.RestoreJournalFolderOverride!, _id + ".json");

	public InterruptedServerRestoreTests()
	{
		Directory.CreateDirectory(_root);
		Core.RestoreJournalFolderOverride = Path.Combine(_root, "journals");
		Directory.CreateDirectory(Core.RestoreJournalFolderOverride);
	}

	[Theory]
	[InlineData("Prepared")]
	[InlineData("OriginalPreserved")]
	[InlineData("Activating")]
	public void InterruptedActivationReturnsTheOriginalAndRecoveryIsIdempotent(string phase)
	{
		WriteFolder(Rollback, "original");
		WriteFolder(Install, "incomplete replacement");
		WriteFolder(Operation, "staging");
		WriteJournal(phase);
		Assert.Equal(1, Core.RecoverInterruptedServerRestores());
		Assert.Equal("original", File.ReadAllText(Path.Combine(Install, "world.dat")));
		Assert.False(Directory.Exists(Rollback));
		Assert.False(Directory.Exists(Operation));
		Assert.False(File.Exists(Journal));
		Assert.Equal(0, Core.RecoverInterruptedServerRestores());
	}

	[Fact]
	public void CompletedRestoreKeepsTheNewInstallation()
	{
		WriteFolder(Install, "restored");
		WriteFolder(Rollback, "old");
		WriteFolder(Operation, "staging");
		WriteJournal("Restored");
		Assert.Equal(0, Core.RecoverInterruptedServerRestores());
		Assert.Equal("restored", File.ReadAllText(Path.Combine(Install, "world.dat")));
		Assert.False(Directory.Exists(Rollback));
		Assert.False(File.Exists(Journal));
	}

	[Fact]
	public void MissingActivatedFolderCannotDiscardItsPreservedOriginalDuringSettingsSync()
	{
		WriteFolder(Rollback, "original");
		WriteFolder(Operation, "staging");
		WriteJournal("Restored", needsSettingsSync: true);
		GameServer server = new() { Game = "Fixture", ServerName = "server", InstallPath = Install, Status = "Stopped" };
		Assert.Throws<IOException>(() => Core.SynchronizePendingServerRestores(server, () => true));
		Assert.True(File.Exists(Journal));
		Assert.Equal("original", File.ReadAllText(Path.Combine(Rollback, "world.dat")));
		Assert.Equal(1, Core.RecoverInterruptedServerRestores());
		Assert.Equal("original", File.ReadAllText(Path.Combine(Install, "world.dat")));
		Assert.True(File.Exists(Journal));
		Assert.True(Core.SynchronizePendingServerRestores(server, () => true));
		Assert.False(File.Exists(Journal));
	}

	[Fact]
	public void PreparedRestoreWithNoOriginalOnlyCleansItsOwnStaging()
	{
		WriteFolder(Operation, "staging");
		WriteJournal("Prepared");
		Assert.Equal(0, Core.RecoverInterruptedServerRestores());
		Assert.False(Directory.Exists(Install));
		Assert.False(Directory.Exists(Operation));
	}

	[Theory]
	[InlineData("Unknown", false)]
	[InlineData("Activating", true)]
	public void InvalidPhaseOrMismatchedOperationLeavesAllFilesUntouched(string phase, bool mismatch)
	{
		WriteFolder(Install, "current");
		WriteFolder(Rollback, "original");
		WriteFolder(Operation, "staging");
		WriteJournal(phase, mismatch ? Install + ".synix-restore-rollback-" + Guid.NewGuid().ToString("N") : null);
		Assert.Throws<InvalidDataException>(() => Core.RecoverInterruptedServerRestores());
		Assert.Equal("current", File.ReadAllText(Path.Combine(Install, "world.dat")));
		Assert.Equal("original", File.ReadAllText(Path.Combine(Rollback, "world.dat")));
		Assert.True(File.Exists(Journal));
	}

	[Fact]
	public void BadRecordStopsTheWholeRecoveryQueueBeforeChanges()
	{
		WriteFolder(Install, "current");
		WriteFolder(Rollback, "original");
		WriteJournal("Activating");
		File.WriteAllText(Path.Combine(Core.RestoreJournalFolderOverride!, "invalid.json"), "{}");
		Assert.ThrowsAny<Exception>(() => Core.RecoverInterruptedServerRestores());
		Assert.Equal("current", File.ReadAllText(Path.Combine(Install, "world.dat")));
		Assert.Equal("original", File.ReadAllText(Path.Combine(Rollback, "world.dat")));
		Assert.True(File.Exists(Journal));
	}

	[Fact]
	public void LockedPreservedFolderDoesNotDeleteTheCurrentInstallation()
	{
		WriteFolder(Install, "current");
		WriteFolder(Rollback, "original");
		WriteFolder(Operation, "staging");
		WriteJournal("Activating");
		using (FileStream locked = new(Path.Combine(Rollback, "world.dat"), FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			Assert.ThrowsAny<IOException>(() => Core.RecoverInterruptedServerRestores());
			Assert.Equal("current", File.ReadAllText(Path.Combine(Install, "world.dat")));
			Assert.True(File.Exists(Journal));
		}
		Assert.Equal(1, Core.RecoverInterruptedServerRestores());
		Assert.Equal("original", File.ReadAllText(Path.Combine(Install, "world.dat")));
	}

	[Fact]
	public void MissingRecoveryFoldersAreNotSilentlyDiscarded()
	{
		WriteFolder(Operation, "staging");
		WriteJournal("Activating");
		Assert.Throws<IOException>(() => Core.RecoverInterruptedServerRestores());
		Assert.True(File.Exists(Journal));
		Assert.True(Directory.Exists(Operation));
	}

	[Fact]
	public void FailedInitialFolderMoveMustNotDeleteTheExistingInstallation()
	{
		WriteFolder(Install, "original");
		File.WriteAllText(Path.Combine(Install, "settings.ini"), "original settings");
		string archivePath = Path.Combine(_root, "fixture.zip");
		using (ZipArchive archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
		{
			using StreamWriter writer = new(archive.CreateEntry("world.dat").Open());
			writer.Write("replacement");
		}
		GameServer server = new() { Game = "Fixture", ServerName = "server", InstallPath = Install, Status = "Stopped" };
		ServerBackupArchive backup = new(archivePath, DateTime.UtcNow, new FileInfo(archivePath).Length,
			11, ServerBackupIntegrity.Legacy, null);
		using FileStream locked = new(Path.Combine(Install, "world.dat"), FileMode.Open, FileAccess.Read, FileShare.Read);
		TargetInvocationException failure = Assert.Throws<TargetInvocationException>(() =>
			typeof(Core).GetMethod("RestoreServerBackup", BindingFlags.Instance | BindingFlags.NonPublic)!
				.Invoke(Core.Instance, [server, backup, null]));
		Assert.IsAssignableFrom<IOException>(failure.InnerException);
		Assert.Equal("original", File.ReadAllText(Path.Combine(Install, "world.dat")));
		Assert.Equal("original settings", File.ReadAllText(Path.Combine(Install, "settings.ini")));
	}

	private void WriteJournal(string phase, string? rollback = null, bool needsSettingsSync = false) =>
		File.WriteAllText(Journal, JsonSerializer.Serialize(new
		{
			InstallPath = Install, OperationRoot = Operation, RollbackPath = rollback ?? Rollback, Phase = phase,
			NeedsSettingsSync = needsSettingsSync
		}));

	private static void WriteFolder(string path, string content)
	{
		Directory.CreateDirectory(path);
		File.WriteAllText(Path.Combine(path, "world.dat"), content);
	}

	public void Dispose()
	{
		Core.RestoreJournalFolderOverride = _previousJournals;
		ModPathSafety.EnsureTreeHasNoLinks(_root);
		Directory.Delete(_root, recursive: true);
	}
}
