using Synix_Control_Panel.SynixApp.ServerHandler;

namespace Synix_Control_Panel.SynixEngine;

internal sealed record ServerBackupHealth(
	DateTimeOffset? LatestCompletedUtc = null,
	DateTimeOffset? LatestUnverifiedUtc = null,
	DateTimeOffset? LastFailureUtc = null,
	bool Unavailable = false,
	bool InvalidReceipt = false)
{
	internal string Summary(DateTimeOffset now)
	{
		if (Unavailable) return LocalizationManager.Get("BackupHealth.Unavailable");
		if (LastFailureUtc.HasValue) return LocalizationManager.Get("Discord.Event.BackupFailed") +
			(LatestCompletedUtc is { } previous ? Environment.NewLine + Age(previous, now) : "");
		if (InvalidReceipt) return LocalizationManager.Get("Backup.Integrity.Invalid");
		if (LatestCompletedUtc is { } completed) return Age(completed, now);
		return LocalizationManager.Get(LatestUnverifiedUtc.HasValue ? "BackupHealth.Unverified" : "BackupHealth.None");
	}
	internal bool NeedsAttention(DateTimeOffset now) => Unavailable || InvalidReceipt || LastFailureUtc.HasValue ||
		LatestCompletedUtc == null || now - LatestCompletedUtc > TimeSpan.FromDays(7);

	internal string Details(DateTimeOffset now)
	{
		List<string> lines = [];
		if (LatestCompletedUtc is { } completed)
			lines.Add(LocalizationManager.Get("BackupHealth.Latest", FormatDate(completed), Age(completed, now)));
		else lines.Add(LocalizationManager.Get("BackupHealth.None"));
		if (LatestUnverifiedUtc is { } unverified)
			lines.Add(LocalizationManager.Get("BackupHealth.Unverified") + ": " + FormatDate(unverified));
		if (LastFailureUtc is { } failure)
			lines.Add(LocalizationManager.Get("Discord.Event.BackupFailed") + ": " + FormatDate(failure));
		if (Unavailable) lines.Add(LocalizationManager.Get("BackupHealth.Unavailable"));
		if (InvalidReceipt) lines.Add(LocalizationManager.Get("Backup.Integrity.Invalid"));
		lines.Add(LocalizationManager.Get("BackupHealth.Notice"));
		return string.Join(Environment.NewLine, lines);
	}
	private static string FormatDate(DateTimeOffset timestamp) => timestamp.ToLocalTime().ToString("g", System.Globalization.CultureInfo.CurrentUICulture);
	private static string Age(DateTimeOffset timestamp, DateTimeOffset now)
	{
		TimeSpan age = now - timestamp;
		return age.TotalDays >= 1
			? LocalizationManager.Get("BackupHealth.Days", (int)age.TotalDays)
			: LocalizationManager.Get("BackupHealth.Hours", Math.Max(0, (int)age.TotalHours));
	}
}

public partial class Core
{
	internal ServerBackupHealth GetServerBackupHealth(GameServer server) => ReadServerBackupHealth(
		[GetActiveServerBackupFolder(server), Path.Combine(DefaultBackupPath, GetSafeName(server.Game), GetServerIdentity(server))],
		ServerActivityHistory.Current.Read(server));

	// Metadata-only snapshot: never open/hash potentially huge ZIPs on a dashboard refresh.
	// A recorded completion is not a fresh integrity check; restore still verifies the archive.
	internal static ServerBackupHealth ReadServerBackupHealth(
		IEnumerable<string> folders, ServerActivityReadResult history)
	{
		DateTimeOffset? completed = null, unverified = null;
		bool unavailable = history.Unavailable || history.Recovered, invalid = false;
		foreach (string folder in folders.Distinct(StringComparer.OrdinalIgnoreCase))
		{
			try
			{
				// Enumerating directly distinguishes access denied from a missing directory.
				foreach (FileInfo file in new DirectoryInfo(folder).EnumerateFiles("*.zip", SearchOption.TopDirectoryOnly))
				{
					ServerBackupIntegrity integrity = InspectBackupIntegrity(file.FullName, out BackupReceipt? receipt);
					if (file.Length == 0 || integrity == ServerBackupIntegrity.Invalid)
					{
						invalid = true;
						continue;
					}
					DateTimeOffset timestamp = receipt?.CreatedUtc ?? new DateTimeOffset(file.LastWriteTimeUtc, TimeSpan.Zero);
					if (integrity == ServerBackupIntegrity.Recorded && receipt?.VerifiedUtc != null)
						completed = !completed.HasValue || timestamp > completed ? timestamp : completed;
					else unverified = !unverified.HasValue || timestamp > unverified ? timestamp : unverified;
				}
			}
			catch (DirectoryNotFoundException)
			{
				// An offline custom/network backup drive is unknown, not "no backups".
				string? root = Path.GetPathRoot(Path.GetFullPath(folder));
				if (root == null || !Directory.Exists(root)) unavailable = true;
			}
			catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException)
			{ unavailable = true; }
		}
		ServerActivityEntry? lastAttempt = history.Entries
			.Where(entry => entry.Event is "BackupCompleted" or "BackupFailed")
			.OrderByDescending(entry => entry.OccurredUtc).FirstOrDefault();
		return new(completed, unverified,
			lastAttempt?.Event == "BackupFailed" ? lastAttempt.OccurredUtc : null, unavailable, invalid);
	}
}
