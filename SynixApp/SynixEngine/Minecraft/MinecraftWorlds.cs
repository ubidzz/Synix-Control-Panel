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
using System.IO.Compression;
using System.Text;

namespace Synix_Control_Panel.SynixEngine.Minecraft;

internal sealed class MinecraftStaging : IDisposable
{
	internal const int MaxFiles = 50000;
	internal const long MaxFileBytes = 1024L * 1024 * 1024;
	internal const long MaxTotalBytes = 20L * 1024 * 1024 * 1024;
	internal string Root { get; } = Path.Combine(Path.GetTempPath(), "SynixMinecraft-" + Guid.NewGuid().ToString("N"));
	internal Dictionary<string, string> Files { get; } = new(StringComparer.OrdinalIgnoreCase);
	private long _bytes;

	internal MinecraftStaging() { ModPathSafety.EnsureNoLinks(Root); Directory.CreateDirectory(Root); }

	internal void Add(string relative, Stream input, long length, bool replace = false)
	{
		if (length < 0 || length > MaxFileBytes || checked(_bytes + length) > MaxTotalBytes ||
			Files.Count >= MaxFiles) throw MinecraftContentTransactions.Error("Size");
		string normalized = relative.Replace('\\', '/');
		ModPathSafety.Resolve(Root, normalized);
		if (!replace && Files.ContainsKey(normalized)) throw MinecraftContentTransactions.Error("Layout");
		string destination = ModPathSafety.Resolve(Root, "files/" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
		using (FileStream output = new(destination, FileMode.CreateNew, FileAccess.Write))
			ModPackageFiles.CopyExactBounded(input, output, length);
		Files[normalized] = destination;
		_bytes += length;
	}

	internal IReadOnlyList<MinecraftFileChange> Changes(GameServer server) =>
		Files.Select(file => new MinecraftFileChange(file.Key, file.Value,
			MinecraftContentTransactions.HashFile(file.Value),
			MinecraftContentTransactions.HashFile(ModPathSafety.Resolve(server.InstallPath, file.Key)))).ToArray();

	internal static void ValidateArchive(ZipArchive archive)
	{
		long total = 0;
		if (archive.Entries.Count > MaxFiles) throw MinecraftContentTransactions.Error("Size");
		HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);
		foreach (ZipArchiveEntry entry in archive.Entries)
		{
			string path = entry.FullName.Replace('\\', '/').TrimEnd('/');
			int unixKind = (entry.ExternalAttributes >> 16) & 0xF000;
			if (!ModPathSafety.IsSafeRelativePath(path) || !names.Add(path) ||
				unixKind is not (0 or 0x8000 or 0x4000) ||
				(entry.ExternalAttributes & (int)FileAttributes.ReparsePoint) != 0)
				throw MinecraftContentTransactions.Error("Layout");
			if (entry.Length > MaxFileBytes || (total = checked(total + entry.Length)) > MaxTotalBytes)
				throw MinecraftContentTransactions.Error("Size");
		}
	}

	internal static string ReadSmall(ZipArchiveEntry entry)
	{
		if (entry.Length > 1024 * 1024) throw MinecraftContentTransactions.Error("Size");
		using Stream stream = entry.Open();
		using MemoryStream output = new();
		ModPackageFiles.CopyExactBounded(stream, output, entry.Length);
		return Encoding.UTF8.GetString(output.ToArray());
	}

	public void Dispose()
	{
		ModPathSafety.EnsureTreeHasNoLinks(Root);
		if (Directory.Exists(Root)) Directory.Delete(Root, true);
	}
}

internal sealed record MinecraftWorld(string Name, string RelativePath, bool Active);

internal static class MinecraftWorlds
{
	internal static IReadOnlyList<MinecraftWorld> List(GameServer server)
	{
		string root = MinecraftControlProfile.IsBedrock(server)
			? ModPathSafety.Resolve(server.InstallPath, "worlds") : server.InstallPath;
		ModPathSafety.EnsureNoLinks(root);
		if (!Directory.Exists(root)) return [];
		string active = ActiveName(server);
		List<MinecraftWorld> worlds = [];
		foreach (string folder in Directory.EnumerateDirectories(root).Take(1000))
		{
			ModPathSafety.EnsureNoLinks(folder);
			if (File.Exists(ModPathSafety.Resolve(folder, "level.dat")))
				worlds.Add(new(Path.GetFileName(folder), Path.GetRelativePath(server.InstallPath, folder),
					Path.GetFileName(folder).Equals(active, StringComparison.OrdinalIgnoreCase)));
		}
		return worlds.OrderByDescending(world => world.Active).ThenBy(world => world.Name).ToArray();
	}

	internal static string ActiveName(GameServer server)
	{
		string properties = ModPathSafety.Resolve(server.InstallPath, "server.properties");
		if (File.Exists(properties))
		{
			if (new FileInfo(properties).Length > 1024 * 1024) throw MinecraftContentTransactions.Error("Size");
			if (MinecraftConfigurationSync.ReadProperties(ConfigurationTextSnapshot.Read(properties).Text)
				.TryGetValue("level-name", out string? name)) return name;
		}
		return string.IsNullOrWhiteSpace(server.WorldName) ? "world" : server.WorldName;
	}

	internal static bool IsActiveWorldPath(GameServer server, string relative)
	{
		string active = ActiveName(server);
		string prefix = MinecraftControlProfile.IsBedrock(server) ? "worlds/" : "";
		string path = relative.Replace('\\', '/');
		return new[] { active, active + "_nether", active + "_the_end" }.Any(name =>
			path.StartsWith(prefix + name + "/", StringComparison.OrdinalIgnoreCase));
	}

	internal static MinecraftStaging PrepareImport(GameServer server, string zipPath, string name)
	{
		ValidateName(name);
		ModPathSafety.EnsureNoLinks(zipPath);
		string worldPath = MinecraftControlProfile.IsBedrock(server) ? "worlds/" + name : name;
		if (Directory.Exists(ModPathSafety.Resolve(server.InstallPath, worldPath)))
			throw MinecraftContentTransactions.Error("WorldExists");
		using ZipArchive zip = ZipFile.OpenRead(zipPath);
		MinecraftStaging.ValidateArchive(zip);
		string[] roots = zip.Entries.Where(entry => entry.FullName.Replace('\\', '/').EndsWith("level.dat", StringComparison.Ordinal) &&
			Path.GetFileName(entry.FullName.Replace('/', Path.DirectorySeparatorChar)) == "level.dat")
			.Select(entry => entry.FullName[..^"level.dat".Length].Replace('\\', '/')).ToArray();
		if (roots.Length != 1) throw MinecraftContentTransactions.Error("WorldLayout");
		string prefix = roots[0];
		bool hasBedrockDatabase = zip.Entries.Any(entry => entry.FullName.Replace('\\', '/').StartsWith(prefix + "db/", StringComparison.Ordinal));
		bool hasJavaData = zip.Entries.Any(entry => entry.FullName.Replace('\\', '/').StartsWith(prefix + "region/", StringComparison.Ordinal) ||
			entry.FullName.Replace('\\', '/').StartsWith(prefix + "dimensions/", StringComparison.Ordinal));
		if (MinecraftControlProfile.IsBedrock(server) ? !hasBedrockDatabase || hasJavaData : hasBedrockDatabase || !hasJavaData)
			throw MinecraftContentTransactions.Error("WorldEdition");
		MinecraftStaging staging = new();
		try
		{
			foreach (ZipArchiveEntry entry in zip.Entries)
			{
				string path = entry.FullName.Replace('\\', '/');
				if (entry.Name.Length == 0 || !path.StartsWith(prefix, StringComparison.Ordinal)) continue;
				string child = path[prefix.Length..];
				if (child == "session.lock") continue;
				if (MinecraftModpacks.IsExecutableFile(child)) throw MinecraftContentTransactions.Error("Layout");
				using Stream input = entry.Open();
				staging.Add(worldPath + "/" + child, input, entry.Length);
			}
			return staging;
		}
		catch { staging.Dispose(); throw; }
	}

	internal static void Switch(GameServer server, string name, Func<bool> persist)
	{
		ValidateName(name);
		using ServerOperationLease operation = ModPackageManager.BeginOperation(server);
		ModPackageManager.EnsureStopped(server);
		if (!List(server).Any(world => world.Name == name)) throw MinecraftContentTransactions.Error("WorldLayout");
		string path = ModPathSafety.Resolve(server.InstallPath, "server.properties");
		string? hash = MinecraftContentTransactions.HashFile(path);
		string original = File.Exists(path) ? ConfigurationTextSnapshot.Read(path).Text : "";
		MinecraftConfigurationSync.Save(server, path,
			MinecraftConfigurationSync.SetProperty(original, "level-name", name), hash, persist);
	}

	internal static void ValidateName(string name)
	{
		if (name.Length > 80 || !ModPathSafety.IsSafeRelativePath(name) || name.Contains('/') || name.Contains('\\') ||
			name.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not (' ' or '-' or '_')))
			throw MinecraftContentTransactions.Error("WorldName");
	}
}
