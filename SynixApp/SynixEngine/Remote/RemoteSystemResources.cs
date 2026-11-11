using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace Synix_Control_Panel.SynixEngine;

internal sealed record RemoteSystemResources(double? CpuPercent, long? RamUsedBytes, long? RamTotalBytes,
    double? NetworkSentBytesPerSecond, double? NetworkReceivedBytesPerSecond);
internal sealed record RemoteNetworkReading(string Id, long Sent, long Received);

// Whole-computer metrics belong to the computer selector, separately from game-server totals.
// Samples only on the existing active-view interval; no packet contents or adapter identities leave the PC.
internal sealed class RemoteSystemResourceSampler : IDisposable
{
    private PerformanceCounter? _cpu;
    private bool _primed, _cpuFailed;
    private readonly Func<double> _readCpu, _clock;
    private readonly Func<(ulong Total, ulong Available)?> _readMemory;
    private readonly Func<IReadOnlyList<RemoteNetworkReading>> _readNetwork;
    private readonly Dictionary<string, (long Sent, long Received, double Time)> _networkPrevious = new();
    private bool _networkPrimed;

    internal RemoteSystemResourceSampler(Func<double>? readCpu = null,
        Func<(ulong Total, ulong Available)?>? readMemory = null,
        Func<IReadOnlyList<RemoteNetworkReading>>? readNetwork = null, Func<double>? clock = null)
    {
        _readCpu = readCpu ?? ReadCpu;
        _readMemory = readMemory ?? ReadMemory;
        _readNetwork = readNetwork ?? ReadNetwork;
        _clock = clock ?? (() => (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency);
    }

    internal RemoteSystemResources Capture()
    {
        const ulong maxSafeInteger = 9_007_199_254_740_991;
        double? cpu = null;
        if (!_cpuFailed)
        {
            try
            {
                double value = _readCpu();
                if (_primed && double.IsFinite(value) && value >= 0) cpu = Math.Round(Math.Clamp(value, 0, 100), 1);
                _primed = double.IsFinite(value) && value >= 0;
            }
            catch (Exception ex) when (IsUnavailable(ex)) { _cpuFailed = true; _primed = false; }
        }
        long? used = null, total = null;
        try
        {
            if (_readMemory() is { Total: > 0 } ram && ram.Total <= maxSafeInteger && ram.Available <= ram.Total)
            { used = (long)(ram.Total - ram.Available); total = (long)ram.Total; }
        }
        catch (Exception ex) when (IsUnavailable(ex)) { }
        var network = CaptureNetwork();
        return new(cpu, used, total, network.Sent, network.Received);
    }

    private (double? Sent, double? Received) CaptureNetwork()
    {
        try
        {
            var readings = _readNetwork();
            double now = _clock(), sent = 0, received = 0;
            bool available = _networkPrimed;
            HashSet<string> seen = [];
            foreach (var adapter in readings)
            {
                if (!seen.Add(adapter.Id)) continue;
                if (adapter.Sent < 0 || adapter.Received < 0)
                { available = false; _networkPrevious.Remove(adapter.Id); continue; }
                if (_networkPrevious.TryGetValue(adapter.Id, out var previous) && now > previous.Time &&
                    adapter.Sent >= previous.Sent && adapter.Received >= previous.Received)
                {
                    sent += (adapter.Sent - previous.Sent) / (now - previous.Time);
                    received += (adapter.Received - previous.Received) / (now - previous.Time);
                }
                else available = false; // New/reconnected/reset adapters need their own interval.
                _networkPrevious[adapter.Id] = (adapter.Sent, adapter.Received, now);
            }
            foreach (var id in _networkPrevious.Keys.Where(id => !seen.Contains(id)).ToArray()) _networkPrevious.Remove(id);
            _networkPrimed = true;
            if (!available || !double.IsFinite(sent) || !double.IsFinite(received)) return (null, null);
            return (Math.Round(sent, 1), Math.Round(received, 1));
        }
        catch (Exception ex) when (IsUnavailable(ex))
        { _networkPrevious.Clear(); _networkPrimed = false; return (null, null); }
    }

    private double ReadCpu()
    {
        _cpu ??= new PerformanceCounter("Processor", "% Processor Time", "_Total", readOnly: true);
        return _cpu.NextValue();
    }

    private static (ulong Total, ulong Available)? ReadMemory()
    {
        MemoryStatus status = new() { Length = (uint)Marshal.SizeOf<MemoryStatus>() };
        return GlobalMemoryStatusEx(ref status) ? (status.TotalPhysical, status.AvailablePhysical) : null;
    }

    private static IReadOnlyList<RemoteNetworkReading> ReadNetwork()
    {
        List<RemoteNetworkReading> readings = [];
        foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (adapter.OperationalStatus != OperationalStatus.Up ||
                adapter.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel) continue;
            // Default-gateway adapters avoid also counting internal VM/container-only links.
            // Their counters include LAN traffic; these are network rates, not a speed test.
            if (!adapter.GetIPProperties().GatewayAddresses.Any(g =>
                !g.Address.Equals(IPAddress.Any) && !g.Address.Equals(IPAddress.IPv6Any))) continue;
            var stats = adapter.GetIPStatistics();
            readings.Add(new(adapter.Id, stats.BytesSent, stats.BytesReceived));
        }
        return readings;
    }

    private static bool IsUnavailable(Exception ex) => ex is InvalidOperationException or Win32Exception or
        UnauthorizedAccessException or System.Security.SecurityException or NotSupportedException;

    internal void Reset()
    {
        _cpu?.Dispose(); _cpu = null; _primed = false; _cpuFailed = false;
        _networkPrevious.Clear(); _networkPrimed = false;
    }
    public void Dispose() => Reset();

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatus
    {
        public uint Length, Load;
        public ulong TotalPhysical, AvailablePhysical, TotalPageFile, AvailablePageFile,
            TotalVirtual, AvailableVirtual, AvailableExtendedVirtual;
    }
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatus status);
}
