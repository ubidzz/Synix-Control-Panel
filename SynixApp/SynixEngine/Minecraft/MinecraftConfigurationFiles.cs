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

namespace Synix_Control_Panel.SynixEngine.Minecraft;

internal static class MinecraftConfigurationFiles
{
	private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
		{ ".properties", ".json", ".yaml", ".yml", ".toml", ".cfg", ".conf", ".ini", ".xml" };

	internal static IReadOnlyList<string> List(GameServer server)
	{
		ModPathSafety.EnsureNoLinks(server.InstallPath);
		if (!Directory.Exists(server.InstallPath)) return [];
		List<string> results = [];
		int visited = 0;
		void Read(string directory, bool recursive, int depth)
		{
			if (depth > 16) throw MinecraftContentTransactions.Error("Size");
			foreach (string path in Directory.EnumerateFileSystemEntries(directory))
			{
				if (++visited > 10000) throw MinecraftContentTransactions.Error("Size");
				ModPathSafety.EnsureNoLinks(path);
				if (Directory.Exists(path)) { if (recursive) Read(path, true, depth + 1); continue; }
				if (!Path.GetFileName(path).StartsWith(".synix-", StringComparison.Ordinal) && Extensions.Contains(Path.GetExtension(path)) &&
					new FileInfo(path).Length <= 4 * 1024 * 1024)
					results.Add(Path.GetRelativePath(server.InstallPath, path));
			}
		}
		Read(server.InstallPath, false, 0);
		foreach (string folder in new[] { "config", "defaultconfigs", "plugins", MinecraftWorlds.ActiveName(server) + "/serverconfig" })
		{
			string path = ModPathSafety.Resolve(server.InstallPath, folder);
			if (Directory.Exists(path)) Read(path, true, 0);
		}
		return results.Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase).ToArray();
	}
}

