using System.ComponentModel;
using System.Text.Json;
using Synix_Control_Panel.SynixEngine;
using static Synix_Control_Panel.SynixEngine.Core;
using Xunit;

namespace Synix_Control_Panel.Tests;

public class RemoteComputerResourceTests
{
    private static readonly DateTime Started = new(2026, 11, 11, 0, 0, 0, DateTimeKind.Utc);
    private static ServerProcessIdentity Identity(int pid, DateTime? started = null) => new()
        { ProcessId = pid, StartTimeUtc = started ?? Started, ExecutablePath = "C:\\Synix\\Games\\test\\server.exe" };

    [Fact]
    public void OnlyRunningAndStartingServersSupplyProcessesAndStoppedServersReadNothing()
    {
        var running = new GameServer { Status = StatusManager.GetStatus(ServerState.Running), ServerProcesses = [Identity(11)] };
        var starting = new GameServer { Status = StatusManager.GetStatus(ServerState.Starting), ServerProcesses = [Identity(12)] };
        var stopped = new GameServer { Status = StatusManager.GetStatus(ServerState.Stopped), PID = 13, ServerProcesses = [Identity(13)] };
        Assert.Equal(new[] { 11, 12 }, RemoteComputerResourceSampler.SnapshotProcesses([running, starting, stopped]).Select(p => p.ProcessId));
        using var sampler = new RemoteComputerResourceSampler(_ => throw new Exception("Stopped server must not be sampled"));
        var usage = sampler.Capture(RemoteComputerResourceSampler.SnapshotProcesses([stopped]));
        Assert.Equal(0, usage.CpuPercent);
        Assert.Equal(0, usage.RamUsedBytes);
    }

    [Fact]
    public void SumsDistinctServerProcessesAndNormalizesCpuAcrossAllCores()
    {
        double now = 0;
        int reads = 0;
        using var sampler = new RemoteComputerResourceSampler(p =>
        {
            reads++;
            return new(Started, p.ProcessId == 11 ? 1000 + now * 250 : 2000 + now * 750,
                p.ProcessId == 11 ? 1073741824 : 2L * 1073741824);
        }, () => now, 4);
        ServerProcessIdentity[] processes = [Identity(11), Identity(12), Identity(11)];
        Assert.Null(sampler.Capture(processes).CpuPercent);
        now = 10;
        var usage = sampler.Capture(processes);
        Assert.Equal(25, usage.CpuPercent);
        Assert.Equal(3L * 1073741824, usage.RamUsedBytes);
        Assert.Equal(4, reads);
        Assert.Equal(new RemoteComputerResources(0, 0), sampler.Capture([]));
        now = 20;
        Assert.Null(sampler.Capture(processes).CpuPercent);
    }

    [Fact]
    public void StaleAndReusedPidsCannotCountAnUnrelatedProgram()
    {
        double now = 0;
        var actualStart = Started;
        using var sampler = new RemoteComputerResourceSampler(_ => new(actualStart, now * 1000, 256), () => now, 1);
        sampler.Capture([Identity(11)]);
        actualStart = Started.AddMinutes(1);
        now = 10;
        Assert.Equal(new RemoteComputerResources(0, 0), sampler.Capture([Identity(11)]));
        Assert.Null(sampler.Capture([Identity(11, actualStart)]).CpuPercent);
        now = 20;
        Assert.Equal(100, sampler.Capture([Identity(11, actualStart)]).CpuPercent);
        using var exited = new RemoteComputerResourceSampler(_ => null);
        Assert.Equal(new RemoteComputerResources(0, 0), exited.Capture([Identity(12)]));
    }

    [Fact]
    public void ResetDiscardsCpuIntervalsWithoutLosingTheNextRamReading()
    {
        double now = 0;
        using var sampler = new RemoteComputerResourceSampler(_ => new(Started, now * 100, 512), () => now, 2);
        sampler.Capture([Identity(11)]);
        now = 10;
        Assert.Equal(5, sampler.Capture([Identity(11)]).CpuPercent);
        sampler.Reset();
        var usage = sampler.Capture([Identity(11)]);
        Assert.Null(usage.CpuPercent);
        Assert.Equal(512, usage.RamUsedBytes);
    }

    [Fact]
    public void InaccessibleOrUnverifiedProcessesDoNotInventZeroUsage()
    {
        using var inaccessible = new RemoteComputerResourceSampler(_ => throw new Win32Exception());
        Assert.Equal(new RemoteComputerResources(null, null), inaccessible.Capture([Identity(11)]));
        using var noCpu = new RemoteComputerResourceSampler(_ => new(Started, null, 512));
        Assert.Equal(new RemoteComputerResources(null, 512), noCpu.Capture([Identity(11)]));
        using var unverified = new RemoteComputerResourceSampler(_ => throw new Exception("No trusted creation time"));
        Assert.Equal(new RemoteComputerResources(null, null), unverified.Capture([new() { ProcessId = 11 }]));
    }

    [Fact]
    public void PayloadSeparatesGameServersFromComputerTotalsWithoutProcessOrAdapterDetails()
    {
        var usage = new RemoteComputerResources(12.5, 1024) { Computer = new(50, 8192, 16384, 1250, 2500) };
        using var serialized = JsonDocument.Parse(JsonSerializer.Serialize(usage, RemoteManagerProtocol.Json));
        Assert.Equal(new[] { "cpuPercent", "ramUsedBytes", "scope", "computer" }, serialized.RootElement.EnumerateObject().Select(p => p.Name));
        Assert.Equal("game-servers", serialized.RootElement.GetProperty("scope").GetString());
        Assert.Equal(new[] { "cpuPercent", "ramUsedBytes", "ramTotalBytes", "networkSentBytesPerSecond", "networkReceivedBytesPerSecond" },
            serialized.RootElement.GetProperty("computer").EnumerateObject().Select(p => p.Name));
    }

    [Fact]
    public void ComputerUsageStaysIndependentOfStoppedServersAndNetworkRatesUseElapsedTime()
    {
        double now = 0;
        using var computer = new RemoteSystemResourceSampler(() => 37.5, () => (16UL * 1073741824, 6UL * 1073741824),
            () => [new("ethernet", 1000 + (long)(now * 125000), 2000 + (long)(now * 250000))], () => now);
        var first = computer.Capture();
        Assert.Null(first.CpuPercent);
        Assert.Null(first.NetworkSentBytesPerSecond);
        Assert.Equal(10L * 1073741824, first.RamUsedBytes);
        now = 10;
        var usage = computer.Capture();
        Assert.Equal(37.5, usage.CpuPercent);
        Assert.Equal(125000, usage.NetworkSentBytesPerSecond);
        Assert.Equal(250000, usage.NetworkReceivedBytesPerSecond);
        using var servers = new RemoteComputerResourceSampler(_ => throw new Exception("No running servers"));
        Assert.Equal(new RemoteComputerResources(0, 0), servers.Capture([]));
        computer.Reset();
        var reset = computer.Capture();
        Assert.Null(reset.CpuPercent);
        Assert.Null(reset.NetworkReceivedBytesPerSecond);
        Assert.Equal(first.RamUsedBytes, reset.RamUsedBytes);
    }

    [Fact]
    public void NetworkAdaptersResetReconnectAndDisappearWithoutSpikesOrDoubleCounting()
    {
        double now = 0;
        RemoteNetworkReading[] readings = [new("a", 100, 1000), new("b", 20, 200), new("a", 100, 1000)];
        using var computer = new RemoteSystemResourceSampler(() => 0, () => (16UL, 8UL), () => readings, () => now);
        Assert.Null(computer.Capture().NetworkSentBytesPerSecond);
        now = 10; readings = [new("a", 300, 2000), new("b", 120, 600), new("a", 300, 2000)];
        var usage = computer.Capture();
        Assert.Equal(30, usage.NetworkSentBytesPerSecond);
        Assert.Equal(140, usage.NetworkReceivedBytesPerSecond);
        now = 20; readings = [new("a", 50, 100)]; // Adapter counter restarted; b disconnected.
        Assert.Null(computer.Capture().NetworkSentBytesPerSecond);
        now = 30; readings = [new("a", 150, 300)];
        Assert.Equal(10, computer.Capture().NetworkSentBytesPerSecond);
        now = 40; readings = [];
        Assert.Equal(0, computer.Capture().NetworkSentBytesPerSecond);
        now = 50; readings = [new("a", 500000, 900000)];
        Assert.Null(computer.Capture().NetworkSentBytesPerSecond);
    }

    [Fact]
    public void UnavailableSystemCountersDoNotBreakOtherReadingsOrReportFakeNetworkZeros()
    {
        int calls = 0;
        using var computer = new RemoteSystemResourceSampler(() => { calls++; throw new Win32Exception(); },
            () => (16UL, 6UL), () => throw new System.Net.NetworkInformation.NetworkInformationException());
        var usage = computer.Capture();
        Assert.Null(usage.CpuPercent);
        Assert.Null(usage.NetworkReceivedBytesPerSecond);
        Assert.Equal(10, usage.RamUsedBytes);
        computer.Capture(); Assert.Equal(1, calls);
        computer.Reset(); computer.Capture(); Assert.Equal(2, calls);
    }
}
