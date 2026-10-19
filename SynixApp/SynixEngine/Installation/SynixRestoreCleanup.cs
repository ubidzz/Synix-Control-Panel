using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Synix_Control_Panel.SynixEngine.ModManagement;

namespace Synix_Control_Panel.SynixEngine;

/// <summary>Restores keep rollback copies beside servers, with journals in the user's app data.</summary>
internal sealed class SynixRestoreCleanup
{
	internal List<string> ProtectedFolders { get; } = [];
	internal List<string> BackupFolders { get; } = [];
	internal List<string> JournalFiles { get; } = [];
	internal string Fingerprint { get; private set; } = "";

	internal static SynixRestoreCleanup Discover(SynixUninstallPaths paths, IReadOnlyList<SynixUninstallServer> servers)
	{
		SynixRestoreCleanup inventory = new();
		string journalRoot = Path.Combine(paths.LocalData, "RestoreOperations");
		StringBuilder signature = new();
		foreach (string file in SynixAppCleanup.Entries(journalRoot).Order(StringComparer.OrdinalIgnoreCase))
		{
			string id = Path.GetFileNameWithoutExtension(file);
			if (!Path.GetExtension(file).Equals(".json", StringComparison.OrdinalIgnoreCase) || !Guid.TryParseExact(id, "N", out _)) continue;
			ModPathSafety.EnsureNoLinks(file);
			using FileStream input = new(file, FileMode.Open, FileAccess.Read, FileShare.Read);
			if (input.Length > 64 * 1024) throw SynixUninstallPlan.Unsafe(file);
			byte[] bytes = new byte[checked((int)input.Length)];
			input.ReadExactly(bytes);
			signature.Append(file).Append(Convert.ToHexString(SHA256.HashData(bytes)));
			using JsonDocument document = JsonDocument.Parse(bytes);
			JsonElement journal = document.RootElement;
			string install = ReadPath("InstallPath");
			string operation = ReadPath("OperationRoot");
			string rollback = ReadPath("RollbackPath");
			string parent = Path.GetDirectoryName(install)!;
			if (!string.Equals(Path.GetDirectoryName(operation), parent, StringComparison.OrdinalIgnoreCase) ||
				!Path.GetFileName(operation).StartsWith(".synix-restore-", StringComparison.OrdinalIgnoreCase) ||
				!Path.GetFileName(operation).EndsWith("-" + id, StringComparison.OrdinalIgnoreCase) ||
				!string.Equals(rollback, install + ".synix-restore-rollback-" + id, StringComparison.OrdinalIgnoreCase) ||
				operation.Equals(rollback, StringComparison.OrdinalIgnoreCase) || operation.Equals(install, StringComparison.OrdinalIgnoreCase))
				throw SynixUninstallPlan.Unsafe(file);
			inventory.ProtectedFolders.AddRange([operation, rollback]);
			// Unknown/de-registered servers' recoveries remain untouched, including their journal.
			if (!servers.Any(server => server.InstallPath.Equals(install, StringComparison.OrdinalIgnoreCase)))
			{
				inventory.ProtectedFolders.Add(install);
				continue;
			}
			inventory.BackupFolders.AddRange([operation, rollback]);
			inventory.JournalFiles.Add(file);

			string ReadPath(string name) => journal.TryGetProperty(name, out JsonElement value) &&
				value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetString())
				? SynixUninstallPlan.Normalize(value.GetString()!) : throw SynixUninstallPlan.Unsafe(file);
		}
		inventory.Fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(signature.ToString())));
		return inventory;
	}
}
