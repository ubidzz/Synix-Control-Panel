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
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixApp.SteamCMDHandler;
using Synix_Control_Panel.SynixEngine.ModManagement;
using System.Text.Json;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel.SynixEngine.Minecraft;

internal sealed record MinecraftInstalledRuntime(string Edition, string Version, string Loader, string Build, int Java)
{
	internal static MinecraftInstalledRuntime Capture(GameServer server) => new(server.MinecraftEdition,
		server.GameVersion, server.MinecraftLoader, server.MinecraftLoaderVersion, server.RequiredJavaVersion);
	internal void Apply(GameServer server)
	{
		server.MinecraftEdition = Edition; server.GameVersion = Version; server.MinecraftLoader = Loader;
		server.MinecraftLoaderVersion = Build; server.RequiredJavaVersion = Java;
	}
}

internal static class MinecraftRuntimeUpdater
{
	internal const string ReceiptName = ".synix-minecraft-runtime.json";

	internal static MinecraftInstalledRuntime? ReadInstalled(GameServer server)
	{
		string path = ModPathSafety.Resolve(server.InstallPath, ReceiptName);
		if (!File.Exists(path)) return null;
		if (new FileInfo(path).Length > 4096) throw MinecraftContentTransactions.Error("Runtime");
		MinecraftInstalledRuntime runtime = JsonSerializer.Deserialize<MinecraftInstalledRuntime>(File.ReadAllText(path))
			?? throw MinecraftContentTransactions.Error("Runtime");
		if (runtime.Edition is not ("Java" or "Bedrock") || string.IsNullOrWhiteSpace(runtime.Version) ||
			runtime.Version.Length > 100 || runtime.Loader is not ("Vanilla" or "Fabric" or "Forge" or "NeoForge" or "Paper" or "Purpur") ||
			runtime.Build == null || runtime.Build.Length > 100 || runtime.Java is < 0 or > 100)
			throw MinecraftContentTransactions.Error("Runtime");
		return runtime;
	}

	internal static void WriteInstalled(GameServer server)
	{
		string path = ModPathSafety.Resolve(server.InstallPath, ReceiptName);
		File.WriteAllText(path, JsonSerializer.Serialize(MinecraftInstalledRuntime.Capture(server)));
	}

	internal static async Task ApplyAsync(GameServer server, Func<bool> persist, IProgress<string>? progress = null)
	{
		using ServerOperationLease operation = ServerOperationCoordinator.TryBegin(server, ServerOperationKind.Install);
		if (!operation.Acquired) throw new InvalidOperationException(operation.FailureReason);
		ModPackageManager.EnsureStopped(server);
		if (ModSecurityScanner.IsCurrentProcessElevated())
			throw new InvalidOperationException(LocalizationManager.Get("ModManager.Error.ElevatedProcess"));
		MinecraftInstalledRuntime before = MinecraftInstalledRuntime.Capture(server);
		MinecraftInstalledRuntime? previousInstalled = ReadInstalled(server);
		using MinecraftStaging staging = new();
		string installation = ModPathSafety.Resolve(staging.Root, "runtime");
		GameServer draft = new()
		{
			Game = "Minecraft", ServerName = server.ServerName, InstallPath = installation,
			MinecraftEdition = server.MinecraftEdition, MinecraftLoader = server.MinecraftLoader,
			MinecraftLoaderVersion = server.MinecraftLoaderVersion, GameVersion = server.GameVersion,
			MaxRam = server.MaxRam, RequiredJavaVersion = server.RequiredJavaVersion,
			Status = StatusManager.GetStatus(ServerState.Stopped)
		};
		// Only official runtime installers are invoked. No server or pack-supplied launcher is started.
		int result = await Task.Run(() => ServerInstaller.Install(draft, GameDatabase.GetGame("Minecraft")!,
			message => progress?.Report(message))).ConfigureAwait(true);
		if (result != 0) throw MinecraftContentTransactions.Error("Runtime");
		ModPathSafety.EnsureTreeHasNoLinks(installation);
		foreach (string file in Directory.EnumerateFiles(installation, "*", SearchOption.AllDirectories))
		{
			string relative = Path.GetRelativePath(installation, file).Replace('\\', '/');
			if (relative is "server.properties" or "allowlist.json" or "permissions.json" or "eula.txt") continue;
			using FileStream input = File.OpenRead(file);
			staging.Add(relative, input, input.Length);
		}
		MinecraftChangeReceipt receipt = await Task.Run(() => MinecraftContentTransactions.Apply(server,
			$"{draft.MinecraftLoader} {draft.GameVersion} / {draft.MinecraftLoaderVersion}", "runtime", staging.Changes(server),
			previousRuntime: previousInstalled));
		try
		{
			MinecraftInstalledRuntime.Capture(draft).Apply(server);
			if (!persist()) throw MinecraftContentTransactions.Error("SaveEntry");
		}
		catch { before.Apply(server); MinecraftContentTransactions.Undo(server, receipt.Id, rollback: true); throw; }
	}
}
