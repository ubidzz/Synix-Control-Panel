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
using Synix_Control_Panel.SynixApp.MonitoringHandler;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class ResourceConsoleProcessTests
{
	private static readonly DateTime Started = DateTime.UtcNow.AddMinutes(-5);
	private static ServerProcessIdentity Identity(int pid, string executable, int seconds = 0) => new()
	{
		ProcessId = pid, StartTimeUtc = Started.AddSeconds(seconds), ExecutablePath = executable
	};

	[Fact]
	public void DirectExecutableIncludesItsConsoleHostButNotUnrelatedConsoles()
	{
		var game = Identity(101, @"C:\Servers\Enshrouded\enshrouded_server.exe");
		var host = Identity(102, Path.Combine(Environment.SystemDirectory, "conhost.exe"), 1);
		var unrelated = Identity(103, host.ExecutablePath, 1);
		Dictionary<int, ServerProcessIdentity> live = new() { [101] = game, [102] = host, [103] = unrelated };
		var result = ResourceConsoleProcessDiscovery.Find([game], new Dictionary<int, int> { [102] = 101, [103] = 999 },
			pid => live.GetValueOrDefault(pid), _ => null);
		Assert.Equal(102, Assert.Single(result).Identity.ProcessId);
		Assert.Equal(new ResourceProcessKey(101, Started), result[0].ConsoleOwner);
	}

	[Fact]
	public void DelegatedHostAndRealCommandAncestorJoinTheTreeWithoutChangingGameOwnership()
	{
		var command = Identity(10, Path.Combine(Environment.SystemDirectory, "cmd.exe"));
		var game = Identity(20, @"C:\Servers\game.exe", 1);
		var host = Identity(30, @"C:\Program Files\WindowsApps\Terminal\OpenConsole.exe", 2);
		var unrelated = Identity(40, command.ExecutablePath);
		Dictionary<int, ServerProcessIdentity> live = new() { [10] = command, [20] = game, [30] = host, [40] = unrelated };
		Dictionary<int, int> parents = new() { [10] = 999, [20] = 10, [30] = 888 };
		var managed = new List<ServerProcessIdentity> { game };
		var result = ResourceConsoleProcessDiscovery.Find(managed, parents, pid => live.GetValueOrDefault(pid),
			_ => new ServerConsoleConnection(30));
		Assert.Equal([10, 30], result.Select(process => process.Identity.ProcessId).Order());
		Assert.Same(game, Assert.Single(managed));
		GameServer server = new();
		ResourceProcessUsage Usage(ServerProcessIdentity identity, ResourceProcessKey? owner = null) => new(
			identity.ProcessId, identity.StartTimeUtc!.Value, parents.GetValueOrDefault(identity.ProcessId), server, "Test",
			Path.GetFileName(identity.ExecutablePath), identity.ProcessId == 20, identity.ExecutablePath, 0, 0, 0, owner);
		var tree = ResourceProcessTree.Build([Usage(game), ..result.Select(process => Usage(process.Identity, process.ConsoleOwner))]);
		Assert.Equal(10, tree[0].Process.ProcessId);
		Assert.All(tree.Skip(1), node => Assert.Equal(10, node.Parent!.Value.ProcessId));
		Assert.Single(ResourceProcessTree.VisibleNodes(tree, new HashSet<ResourceProcessKey> { tree[0].Process.Key }));
	}

	[Fact]
	public void ReusedIdsAndUnrelatedTerminalClientsCannotBecomeConsoleRows()
	{
		var game = Identity(20, @"C:\Servers\game.exe", 1);
		var command = Identity(10, Path.Combine(Environment.SystemDirectory, "cmd.exe"), 2);
		var terminal = Identity(30, @"C:\Terminal\WindowsTerminal.exe");
		Dictionary<int, ServerProcessIdentity> live = new() { [10] = command, [20] = game, [30] = terminal };
		Dictionary<int, int> parents = new() { [20] = 10 };
		Assert.Empty(ResourceConsoleProcessDiscovery.Find([game], parents, pid => live.GetValueOrDefault(pid),
			_ => new ServerConsoleConnection(30)));
		// A command ancestor using another console does not qualify.
		live[10] = Identity(10, command.ExecutablePath);
		Assert.Empty(ResourceConsoleProcessDiscovery.Find([game], parents, pid => live.GetValueOrDefault(pid),
			identity => new ServerConsoleConnection(identity.ProcessId == 20 ? 30 : 40)));
		live[20] = Identity(20, game.ExecutablePath, 10);
		Assert.Empty(ResourceConsoleProcessDiscovery.Find([game], parents, pid => live.GetValueOrDefault(pid),
			_ => throw new InvalidOperationException("A reused game PID must not be queried.")));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task NativeConsoleQueryFindsOnlyTheHostForTheRequestedProcess(bool launchWithCmd)
	{
		string fixture = Path.Combine(AppContext.BaseDirectory, "ConsoleFixture", "bedrock_server.exe");
		using Process launcher = Process.Start(new ProcessStartInfo(launchWithCmd ? Path.Combine(Environment.SystemDirectory, "cmd.exe") : fixture)
		{
			Arguments = launchWithCmd ? $"/d /s /c \"\"{fixture}\"\"" : "",
			UseShellExecute = false, WindowStyle = ProcessWindowStyle.Hidden,
			RedirectStandardInput = true, RedirectStandardOutput = true
		})!;
		try
		{
			// Establish that the fixture is running before inspecting its console.
			await launcher.StandardInput.WriteLineAsync("ready"); await launcher.StandardInput.FlushAsync();
			Assert.Equal("received:ready", await launcher.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(10)));
			int gameId = launchWithCmd ? Servers.GetParentProcessIds()
				.Single(pair => pair.Value == launcher.Id && string.Equals(Servers.TryGetProcessImagePath(pair.Key), fixture, StringComparison.OrdinalIgnoreCase)).Key : launcher.Id;
			using Process game = Process.GetProcessById(gameId);
			var identity = new ServerProcessIdentity { ProcessId = game.Id, StartTimeUtc = game.StartTime.ToUniversalTime(), ExecutablePath = fixture };
			var connection = Servers.ReadConsoleConnection(identity);
			Assert.NotNull(connection);
			Assert.NotEqual(game.Id, connection.HostProcessId);
			string hostName = Path.GetFileName(Servers.TryGetProcessImagePath(connection.HostProcessId)) ?? "";
			Assert.Contains(hostName.ToLowerInvariant(), new[] { "conhost.exe", "openconsole.exe" });
			Assert.False(game.HasExited);
			await launcher.StandardInput.WriteLineAsync("still-running"); await launcher.StandardInput.FlushAsync();
			Assert.Equal("received:still-running", await launcher.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(5)));
			GameServer server = new() { ServerName = "Console fixture", InstallPath = Path.GetDirectoryName(fixture)! };
			ResourceMonitorSnapshotSampler sampler = new(_ => [identity]);
			var snapshot = await sampler.CaptureAsync([server], 32, default);
			Assert.Contains(snapshot.Processes, process => process.ProcessId == game.Id);
			Assert.Contains(snapshot.Processes, process => process.ProcessId == connection.HostProcessId);
			if (launchWithCmd)
			{
				Assert.Contains(snapshot.Processes, process => process.ProcessId == launcher.Id);
				Assert.Equal(launcher.Id, ResourceProcessTree.Build(snapshot.Processes).Single(node => node.Process.ProcessId == gameId).Parent!.Value.ProcessId);
			}
			Assert.Empty(server.ServerProcesses);
			launcher.StandardInput.Close();
			await game.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
			Assert.Empty((await sampler.CaptureAsync([server], 32, default)).Processes);
		}
		finally
		{
			launcher.StandardInput.Close();
			if (!launcher.WaitForExit(3000)) launcher.Kill(entireProcessTree: true);
		}
	}
}
