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
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine.ModManagement;
using System.Security.Cryptography;
using System.Text.Json;

namespace Synix_Control_Panel.SynixEngine.Minecraft;

internal sealed record MinecraftFileChange(string RelativePath, string? Source, string? SourceHash,
	string? ExpectedPreviousHash);

internal sealed class MinecraftChangeReceipt
{
	public string Id { get; set; } = "";
	public string Title { get; set; } = "";
	public string Kind { get; set; } = "";
	public DateTime CreatedUtc { get; set; }
	public string State { get; set; } = "Prepared";
	public MinecraftInstalledRuntime? PreviousRuntime { get; set; }
	public List<MinecraftChangedFile> Files { get; set; } = [];
}

internal sealed class MinecraftChangedFile
{
	public string Path { get; set; } = "";
	public string? Before { get; set; }
	public string? After { get; set; }
}

// A journal and verified previous files exist before the first destination changes.
// Recovery is also available after a process interruption; unrelated edits are never overwritten.
internal static class MinecraftContentTransactions
{
	internal static Exception Error(string key) => new InvalidDataException(LocalizationManager.Get("MinecraftWorkspace.Error." + key));
	internal static string Root(GameServer server) =>
		ModPathSafety.Resolve(ModPackageManager.GetServerDataFolder(server), "MinecraftChanges");

	internal static string? HashFile(string path)
	{
		ModPathSafety.EnsureNoLinks(path);
		if (!File.Exists(path)) return null;
		using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		return Convert.ToHexString(SHA256.HashData(stream));
	}

	internal static IReadOnlyList<MinecraftChangeReceipt> History(GameServer server)
	{
		string root = Root(server);
		if (!Directory.Exists(root)) return [];
		List<MinecraftChangeReceipt> result = [];
		foreach (string directory in Directory.EnumerateDirectories(root))
		{
			ModPathSafety.EnsureNoLinks(directory);
			string path = ModPathSafety.Resolve(directory, "change.json");
			if (!File.Exists(path)) continue;
			if (new FileInfo(path).Length > 16 * 1024 * 1024) throw Error("Size");
			MinecraftChangeReceipt receipt = JsonSerializer.Deserialize<MinecraftChangeReceipt>(File.ReadAllText(path)) ?? throw Error("Recovery");
			Validate(receipt, server);
			if (receipt.Id != Path.GetFileName(directory)) throw Error("Recovery");
			result.Add(receipt);
		}
		return result.OrderByDescending(item => item.CreatedUtc).ToArray();
	}

	internal static MinecraftChangeReceipt Apply(GameServer server, string title, string kind,
		IReadOnlyList<MinecraftFileChange> changes, Action<int>? afterWrite = null,
		MinecraftInstalledRuntime? previousRuntime = null)
	{
		using ServerOperationLease operation = ModPackageManager.BeginOperation(server);
		ModPackageManager.EnsureStopped(server);
		if (changes.Count is 0 or > 50000) throw Error("Size");
		if (History(server).Any(item => item.State == "Prepared")) throw Error("PendingRecovery");
		string id = Guid.NewGuid().ToString("N");
		string root = ModPathSafety.Resolve(Root(server), id);
		MinecraftChangeReceipt receipt = new() { Id = id, Title = title, Kind = kind, CreatedUtc = DateTime.UtcNow, PreviousRuntime = previousRuntime };
		HashSet<string> paths = new(StringComparer.OrdinalIgnoreCase);
		foreach (MinecraftFileChange change in changes)
		{
			if ((change.Source == null) != (change.SourceHash == null) || !IsHash(change.SourceHash) || !IsHash(change.ExpectedPreviousHash)) throw Error("Layout");
			if (!paths.Add(change.RelativePath.Replace('\\', '/'))) throw Error("Layout");
			string destination = ModPathSafety.Resolve(server.InstallPath, change.RelativePath);
			if (Directory.Exists(destination) || HashFile(destination) != change.ExpectedPreviousHash) throw Error("Changed");
			if (change.Source != null && (change.SourceHash == null || HashFile(change.Source) != change.SourceHash)) throw Error("Changed");
			receipt.Files.Add(new() { Path = change.RelativePath, Before = change.ExpectedPreviousHash, After = change.SourceHash });
		}
		Directory.CreateDirectory(root);
		for (int i = 0; i < changes.Count; i++)
		{
			MinecraftFileChange change = changes[i];
			if (change.Source != null) CopyVerified(change.Source, ModPathSafety.Resolve(root, $"Next/{i}"), change.SourceHash!);
			if (change.ExpectedPreviousHash != null)
				CopyVerified(ModPathSafety.Resolve(server.InstallPath, change.RelativePath),
					ModPathSafety.Resolve(root, $"Previous/{i}"), change.ExpectedPreviousHash);
		}
		WriteReceipt(root, receipt);
		try
		{
			ModPackageManager.EnsureStopped(server);
			for (int i = 0; i < receipt.Files.Count; i++)
			{
				MinecraftChangedFile file = receipt.Files[i];
				string destination = ModPathSafety.Resolve(server.InstallPath, file.Path);
				if (HashFile(destination) != file.Before) throw Error("Changed");
				Replace(destination, file.After == null ? null : ModPathSafety.Resolve(root, $"Next/{i}"), file.After);
				afterWrite?.Invoke(i);
			}
			receipt.State = "Applied";
			WriteReceipt(root, receipt);
			return receipt;
		}
		catch
		{
			// Keep the Prepared journal even if recovery fails. Nothing is discarded.
			Restore(server, receipt, root, protectActiveWorld: false);
			throw;
		}
	}

	internal static void Undo(GameServer server, string id, bool rollback = false, Action? synchronize = null)
	{
		using ServerOperationLease operation = ModPackageManager.BeginOperation(server);
		ModPackageManager.EnsureStopped(server);
		MinecraftChangeReceipt receipt = History(server).SingleOrDefault(item => item.Id == id) ?? throw Error("Recovery");
		if (receipt.State is "Undone" or "Superseded") throw Error("Recovery");
		if (!rollback && receipt.Kind == "runtime" && receipt.PreviousRuntime == null) throw Error("RuntimeRecovery");
		Restore(server, receipt, ModPathSafety.Resolve(Root(server), receipt.Id), protectActiveWorld: !rollback,
			synchronize: synchronize);
	}

	internal static void EnsureReadyToStart(GameServer server)
	{
		if (History(server).Any(item => item.State == "Prepared")) throw Error("PendingRecovery");
		if (MinecraftRuntimeUpdater.ReadInstalled(server) is MinecraftInstalledRuntime installed &&
			installed != MinecraftInstalledRuntime.Capture(server)) throw Error("RuntimeMismatch");
	}

	internal static void RecordFullRestore(GameServer server)
	{
		using ServerOperationLease operation = ModPackageManager.BeginOperation(server);
		ModPackageManager.EnsureStopped(server);
		string root = Root(server);
		if (!Directory.Exists(root)) return;
		IReadOnlyList<MinecraftChangeReceipt> history;
		try { history = History(server); }
		catch (Exception exception) when (exception is IOException or JsonException or InvalidDataException or ArgumentException)
		{
			// A verified full restore supersedes an unreadable journal too. Preserve it
			// as opaque recovery data; never follow the paths it claims to contain.
			history = [];
		}
		string archive = ModPathSafety.Resolve(ModPackageManager.GetServerDataFolder(server),
			"MinecraftChanges-fullrestore-" + Guid.NewGuid().ToString("N"));
		ModPathSafety.EnsureNoLinks(root);
		Directory.Move(root, archive);
		foreach (MinecraftChangeReceipt receipt in history)
		{
			receipt.State = "Superseded";
			string reference = ModPathSafety.Resolve(root, receipt.Id);
			Directory.CreateDirectory(reference);
			WriteReceipt(reference, receipt);
		}
	}

	private static void Restore(GameServer server, MinecraftChangeReceipt receipt, string root, bool protectActiveWorld, Action? synchronize = null)
	{
		Validate(receipt, server);
		if (protectActiveWorld && receipt.Kind == "world" &&
			receipt.Files.Any(file => MinecraftWorlds.IsActiveWorldPath(server, file.Path)))
			throw Error("ActiveWorld");
		for (int i = 0; i < receipt.Files.Count; i++)
		{
			MinecraftChangedFile file = receipt.Files[i];
			string destination = ModPathSafety.Resolve(server.InstallPath, file.Path);
			string? current = HashFile(destination);
			if (Directory.Exists(destination) || (current != file.After && current != file.Before)) throw Error("Changed");
			if (file.Before != null && HashFile(ModPathSafety.Resolve(root, $"Previous/{i}")) != file.Before) throw Error("Recovery");
		}
		// Mark recovery intent before mutating; retry permits both before/after hashes.
		receipt.State = "Prepared";
		WriteReceipt(root, receipt);
		for (int i = receipt.Files.Count - 1; i >= 0; i--)
		{
			MinecraftChangedFile file = receipt.Files[i];
			string destination = ModPathSafety.Resolve(server.InstallPath, file.Path);
			string? current = HashFile(destination);
			if (current != file.After && current != file.Before) throw Error("Changed");
			if (current != file.Before)
				Replace(destination, file.Before == null ? null : ModPathSafety.Resolve(root, $"Previous/{i}"), file.Before);
		}
		synchronize?.Invoke();
		receipt.State = "Undone";
		WriteReceipt(root, receipt);
	}

	private static void Validate(MinecraftChangeReceipt receipt, GameServer server)
	{
		if (!Guid.TryParseExact(receipt.Id, "N", out _) || receipt.Files == null || receipt.Files.Count is 0 or > 50000 ||
			receipt.State is not ("Prepared" or "Applied" or "Undone" or "Superseded")) throw Error("Recovery");
		HashSet<string> unique = new(StringComparer.OrdinalIgnoreCase);
		foreach (MinecraftChangedFile file in receipt.Files)
		{
			if (file == null) throw Error("Recovery");
			ModPathSafety.Resolve(server.InstallPath, file.Path);
			if (!unique.Add(file.Path.Replace('\\', '/')) || !IsHash(file.Before) || !IsHash(file.After)) throw Error("Recovery");
		}
	}

	private static bool IsHash(string? hash) => hash == null || (hash.Length == 64 && hash.All(Uri.IsHexDigit));

	private static void Replace(string destination, string? source, string? hash)
	{
		ModPathSafety.EnsureNoLinks(destination);
		if (source == null) { if (File.Exists(destination)) File.Delete(destination); return; }
		string temporary = destination + ".synix-" + Guid.NewGuid().ToString("N");
		try
		{
			CopyVerified(source, temporary, hash!);
			ModPathSafety.EnsureNoLinks(destination);
			File.Move(temporary, destination, true);
		}
		finally { ModPathSafety.EnsureNoLinks(temporary); if (File.Exists(temporary)) File.Delete(temporary); }
	}

	private static void CopyVerified(string source, string destination, string hash)
	{
		ModPathSafety.EnsureNoLinks(source);
		ModPathSafety.EnsureNoLinks(destination);
		Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
		File.Copy(source, destination, false);
		if (HashFile(destination) != hash) throw Error("Changed");
	}

	private static void WriteReceipt(string root, MinecraftChangeReceipt receipt)
	{
		string path = ModPathSafety.Resolve(root, "change.json");
		string temporary = ModPathSafety.Resolve(root, "journal-" + Guid.NewGuid().ToString("N"));
		File.WriteAllText(temporary, JsonSerializer.Serialize(receipt));
		ModPathSafety.EnsureNoLinks(path);
		File.Move(temporary, path, true);
	}
}
