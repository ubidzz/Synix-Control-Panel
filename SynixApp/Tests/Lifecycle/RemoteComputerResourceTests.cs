using System.ComponentModel;
using System.Text.Json;
using Synix_Control_Panel.SynixEngine;
using Xunit;

namespace Synix_Control_Panel.Tests;

public class RemoteComputerResourceTests
{
    [Fact]
    public void CpuNeedsAnIntervalAndResetDiscardsTheOldInterval()
    {
        using var sampler = new RemoteComputerResourceSampler(() => 0, () => (16UL * 1024 * 1024 * 1024, 6UL * 1024 * 1024 * 1024));
        Assert.Null(sampler.Capture().CpuPercent);
        var reading = sampler.Capture();
        Assert.Equal(0, reading.CpuPercent);
        Assert.Equal(10L * 1024 * 1024 * 1024, reading.RamUsedBytes);
        Assert.Equal(16L * 1024 * 1024 * 1024, reading.RamTotalBytes);
        sampler.Reset(); Assert.Null(sampler.Capture().CpuPercent);
        using var serialized = JsonDocument.Parse(JsonSerializer.Serialize(reading, RemoteManagerProtocol.Json));
        Assert.Equal(new[] { "cpuPercent", "ramUsedBytes", "ramTotalBytes" },
            serialized.RootElement.EnumerateObject().Select(p => p.Name));
    }

    [Fact]
    public void UnavailableCountersDoNotInventZeroUsageOrPreventMemoryReadings()
    {
        int calls = 0;
        using var sampler = new RemoteComputerResourceSampler(() => { calls++; throw new Win32Exception(); }, () => (100UL, 25UL));
        Assert.Null(sampler.Capture().CpuPercent);
        Assert.Equal(75, sampler.Capture().RamUsedBytes);
        Assert.Equal(1, calls);
        sampler.Reset(); sampler.Capture(); Assert.Equal(2, calls);
        using var unavailable = new RemoteComputerResourceSampler(() => double.NaN, () => null);
        Assert.Equal(new RemoteComputerResources(null, null, null), unavailable.Capture());
        using var invalid = new RemoteComputerResourceSampler(() => 25, () => (100UL, 101UL));
        Assert.Null(invalid.Capture().RamUsedBytes);
        Assert.Equal(25, invalid.Capture().CpuPercent);
    }
}
