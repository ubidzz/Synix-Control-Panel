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
using System.Diagnostics;
using Synix_Control_Panel.SynixApp.ServerHandler;

namespace Synix_Control_Panel.SynixApp.MonitoringHandler;

internal sealed record ResourceConsoleProcess(ServerProcessIdentity Identity, ResourceProcessKey? ConsoleOwner);

/// <summary>
/// Adds console processes to the monitor only. These associations are never used
/// to authorize input or shutdown, persisted as game processes, or sent to the website.
/// </summary>
internal static class ResourceConsoleProcessDiscovery
{
	internal static IReadOnlyList<ResourceConsoleProcess> Find(
		IReadOnlyList<ServerProcessIdentity> managed,
		IReadOnlyDictionary<int, int> parents,
		Func<int, ServerProcessIdentity?>? readProcess = null,
		Func<ServerProcessIdentity, ServerConsoleConnection?>? readConsole = null)
	{
		readProcess ??= ReadProcess;
		readConsole ??= Servers.ReadConsoleConnection;
		Dictionary<int, ResourceConsoleProcess> additions = [];
		Dictionary<int, ServerProcessIdentity> live = [];
		foreach (ServerProcessIdentity identity in managed)
		{
			if (identity.ProcessId == Environment.ProcessId || !identity.StartTimeUtc.HasValue)
				continue;
			ServerProcessIdentity? actual = readProcess(identity.ProcessId);
			if (actual?.StartTimeUtc == identity.StartTimeUtc &&
				string.Equals(actual?.ExecutablePath, identity.ExecutablePath, StringComparison.OrdinalIgnoreCase))
				live[identity.ProcessId] = actual!;
		}

		foreach (ServerProcessIdentity game in live.Values.ToArray())
		{
			ServerConsoleConnection? connection = readConsole(game);
			if (connection == null || connection.HostProcessId <= 0)
				continue;
			ServerProcessIdentity owner = game;
			HashSet<int> seen = [game.ProcessId];
			// Recover a real CMD launcher only when it is both an ancestor and a
			// client of this exact console. A matching name/window title is insufficient.
			while (parents.TryGetValue(owner.ProcessId, out int parentId) && seen.Add(parentId))
			{
				ServerProcessIdentity? parent = readProcess(parentId);
				if (parent?.StartTimeUtc == null || parent.StartTimeUtc > owner.StartTimeUtc ||
					!string.Equals(parent.ExecutablePath, Path.Combine(Environment.SystemDirectory, "cmd.exe"), StringComparison.OrdinalIgnoreCase) ||
					readConsole(parent)?.HostProcessId != connection.HostProcessId)
					break;
				if (!live.ContainsKey(parentId)) additions[parentId] = new(parent, null);
				owner = parent;
			}

			if (connection.HostProcessId <= 0 || live.ContainsKey(connection.HostProcessId))
				continue;
			ServerProcessIdentity? host = readProcess(connection.HostProcessId);
			if (host?.StartTimeUtc != null && IsConsoleHost(host.ExecutablePath))
				additions.TryAdd(host.ProcessId, new(host, new(owner.ProcessId, owner.StartTimeUtc!.Value)));
		}

		// Classic console hosts are children of the launcher. This also works when
		// the OS console-host query is unavailable. No console input is opened.
		foreach ((int processId, int parentId) in parents)
		{
			if (live.ContainsKey(processId) || additions.ContainsKey(processId)) continue;
			ServerProcessIdentity? owner = live.GetValueOrDefault(parentId) ?? additions.GetValueOrDefault(parentId)?.Identity;
			if (owner == null) continue;
			ServerProcessIdentity? host = readProcess(processId);
			if (host?.StartTimeUtc != null && host.StartTimeUtc >= owner.StartTimeUtc && IsConsoleHost(host.ExecutablePath))
				additions[processId] = new(host, new(owner.ProcessId, owner.StartTimeUtc!.Value));
		}
		return additions.Values.ToArray();
	}

	private static bool IsConsoleHost(string path) =>
		string.Equals(Path.GetFileName(path), "conhost.exe", StringComparison.OrdinalIgnoreCase) ||
		string.Equals(Path.GetFileName(path), "OpenConsole.exe", StringComparison.OrdinalIgnoreCase);

	private static ServerProcessIdentity? ReadProcess(int processId)
	{
		try
		{
			using Process process = Process.GetProcessById(processId);
			string? path = Servers.TryGetProcessImagePath(processId);
			if (process.HasExited || string.IsNullOrWhiteSpace(path)) return null;
			return new() { ProcessId = processId, StartTimeUtc = process.StartTime.ToUniversalTime(), ExecutablePath = path };
		}
		catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or System.ComponentModel.Win32Exception)
		{
			return null;
		}
	}
}
