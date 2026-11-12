using System.Security.Cryptography;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel.SynixEngine;

internal sealed record RemoteBackupChoice(string Id, string Name, long CreatedUtc, long Bytes, string Integrity);

/// <summary>Short-lived opaque selections. A website never supplies a filesystem path.</summary>
internal sealed class RemoteBackupCatalog
{
    private sealed record Selection(string Server, ServerBackupArchive Archive, DateTimeOffset Expires);
    private readonly Dictionary<string, Selection> _selections = new(StringComparer.Ordinal);
    private readonly Dictionary<string, (DateTimeOffset Expires, ServerBackupHealth Health)> _health = new(StringComparer.Ordinal);
    private readonly object _gate = new();

    internal RemoteServerSummary AddHealth(GameServer server, RemoteServerSummary summary, DateTimeOffset now)
    {
        ServerBackupHealth health;
        lock (_gate)
        {
            if (_health.TryGetValue(summary.Id, out var cached) && cached.Expires > now) health = cached.Health;
            else
            {
                health = Core.Instance.GetServerBackupHealth(server);
                foreach (var key in _health.Where(x => x.Value.Expires <= now).Select(x => x.Key).ToArray()) _health.Remove(key);
                _health[summary.Id] = (now.AddSeconds(60), health);
            }
        }
        var latest = health.LatestCompletedUtc;
        bool unverified = health.LatestUnverifiedUtc.HasValue && (!latest.HasValue || health.LatestUnverifiedUtc > latest);
        if (unverified) latest = health.LatestUnverifiedUtc;
        return summary with { LastBackupUtc = latest?.ToUnixTimeSeconds(),
            BackupState = health.Unavailable ? "unavailable" : unverified ? "unverified" : latest.HasValue ? "completed" : "none" };
    }

    internal void Invalidate(string serverId) { lock (_gate) _health.Remove(serverId); }

    internal RemoteBackupChoice[] Remember(string serverId, IEnumerable<ServerBackupArchive> archives, DateTimeOffset now)
    {
        lock (_gate)
        {
            foreach (var key in _selections.Where(x => x.Value.Expires <= now || x.Value.Server == serverId).Select(x => x.Key).ToArray()) _selections.Remove(key);
            return archives.Where(x => x.Integrity != ServerBackupIntegrity.Invalid).OrderByDescending(x => x.CreatedUtc).Take(50).Select(archive =>
            {
                string id = RemoteManagerProtocol.Base64Url(RandomNumberGenerator.GetBytes(24));
                _selections[id] = new(serverId, archive, now.AddMinutes(10));
                string name = SecretRedactor.Redact(archive.FileName);
                name = new string(name.Where(c => !char.IsControl(c)).Take(100).ToArray());
                return new RemoteBackupChoice(id, name, new DateTimeOffset(DateTime.SpecifyKind(archive.CreatedUtc, DateTimeKind.Utc)).ToUnixTimeSeconds(),
                    archive.CompressedBytes, archive.Integrity == ServerBackupIntegrity.Recorded ? "recorded" : "legacy");
            }).ToArray();
        }
    }

    internal ServerBackupArchive? Resolve(string serverId, string? id, IEnumerable<ServerBackupArchive> current, DateTimeOffset now)
    {
        lock (_gate)
        {
            if (id is null || !_selections.TryGetValue(id, out var selected) || selected.Server != serverId || selected.Expires <= now) return null;
            // Re-read the locally approved catalog; removed/replaced archives are not silently substituted.
            return current.FirstOrDefault(x => x.Integrity != ServerBackupIntegrity.Invalid &&
                string.Equals(x.ArchivePath, selected.Archive.ArchivePath, StringComparison.OrdinalIgnoreCase) &&
                x.CreatedUtc == selected.Archive.CreatedUtc && x.CompressedBytes == selected.Archive.CompressedBytes &&
                x.Integrity == selected.Archive.Integrity);
        }
    }
}
