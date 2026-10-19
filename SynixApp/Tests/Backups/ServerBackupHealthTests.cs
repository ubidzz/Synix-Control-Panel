using System.IO.Compression;
using System.Security.Cryptography;
using Synix_Control_Panel.SynixEngine;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class ServerBackupHealthTests : IDisposable
{
	private readonly string _root = Path.Combine(Path.GetTempPath(), "SynixBackupHealth-" + Guid.NewGuid().ToString("N"));
	private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;
	private string Archive(string name, DateTimeOffset created, bool receipt = true)
	{
		Directory.CreateDirectory(_root);
		string path = Path.Combine(_root, name + ".zip");
		using (ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Create))
		using (StreamWriter writer = new(archive.CreateEntry("world.txt").Open())) writer.Write("fixture world");
		File.SetLastWriteTimeUtc(path, created.UtcDateTime);
		if (receipt)
			File.WriteAllText(path + ".sha256", Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))) + "  " + Path.GetFileName(path) +
				$"\nUncompressedBytes=13\nCreatedUtc={created:O}\nVerifiedUtc={created:O}\n");
		return path;
	}
	private ServerBackupHealth Read(params ServerActivityEntry[] entries) => Core.ReadServerBackupHealth([_root, _root], new(entries));

	[Fact]
	public void ReceiptDateWinsOverChangedFileTimestampAndReportsAge()
	{
		DateTimeOffset created = _now.AddDays(-2);
		string archive = Archive("backup", created);
		File.SetLastWriteTimeUtc(archive, _now.AddYears(1).UtcDateTime);
		ServerBackupHealth health = Read();
		Assert.Equal(created, health.LatestCompletedUtc);
		Assert.False(health.NeedsAttention(_now));
		Assert.Contains("2", health.Summary(_now));
		Assert.Contains("7", health.Details(_now));
		Assert.True(health.NeedsAttention(_now.AddDays(6)));
	}

	[Fact]
	public void FailureDoesNotHideOlderBackupAndCompletionClearsFailure()
	{
		Archive("backup", _now.AddDays(-1));
		ServerActivityEntry failure = new(_now, "BackupFailed");
		ServerBackupHealth failed = Read(failure);
		Assert.NotNull(failed.LatestCompletedUtc);
		Assert.Equal(_now, failed.LastFailureUtc);
		Assert.True(failed.NeedsAttention(_now));
		ServerBackupHealth recovered = Read(failure, new(_now.AddMinutes(1), "BackupCompleted"));
		Assert.Null(recovered.LastFailureUtc);
	}

	[Fact]
	public void MissingAndPartialArchivesAreNotReportedAsCompleted()
	{
		string path = Archive("backup", _now);
		File.Move(path, path + ".partial");
		ServerBackupHealth health = Read(new ServerActivityEntry(_now, "BackupCompleted"));
		Assert.Null(health.LatestCompletedUtc);
		Assert.Null(health.LatestUnverifiedUtc);
		Assert.True(health.NeedsAttention(_now));
	}

	[Fact]
	public void LegacyArchivesAreUnverifiedAndInvalidReceiptsNeedAttention()
	{
		string path = Archive("legacy", _now, receipt: false);
		ServerBackupHealth legacy = Read();
		Assert.Null(legacy.LatestCompletedUtc);
		Assert.NotNull(legacy.LatestUnverifiedUtc);
		File.WriteAllText(path + ".sha256", "not a valid receipt");
		Assert.True(Read().InvalidReceipt);
		Archive("good", _now.AddDays(-1));
		ServerBackupHealth mixed = Read();
		Assert.True(mixed.InvalidReceipt);
		Assert.NotNull(mixed.LatestCompletedUtc);
	}

	[Fact]
	public void EmptyArchiveCannotBeShownAsACompletedBackup()
	{
		string path = Archive("empty", _now);
		File.WriteAllBytes(path, []);
		Assert.True(Read().InvalidReceipt);
		Assert.Null(Read().LatestCompletedUtc);
	}

	[Fact]
	public void InaccessibleOrUnreliableSourcesAreNotReportedHealthy()
	{
		Archive("backup", _now);
		Assert.True(Core.ReadServerBackupHealth([_root], new([], Unavailable: true)).Unavailable);
		Assert.True(Core.ReadServerBackupHealth([_root], new([], Recovered: true)).Unavailable);
		string notFolder = Path.Combine(_root, "not-a-folder"); File.WriteAllText(notFolder, "fixture");
		Assert.True(Core.ReadServerBackupHealth([notFolder], new([])).Unavailable);
	}

	public void Dispose() { if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true); }
}
