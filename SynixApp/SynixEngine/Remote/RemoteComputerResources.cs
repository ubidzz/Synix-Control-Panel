using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Synix_Control_Panel.SynixEngine;

internal sealed record RemoteComputerResources(double? CpuPercent, long? RamUsedBytes, long? RamTotalBytes);

// Samples the whole Windows computer (or guest OS), without enumerating processes.
// The remote viewing loop owns this sampler; there is no extra background timer.
internal sealed class RemoteComputerResourceSampler : IDisposable
{
    private PerformanceCounter? _cpu;
    private bool _primed, _cpuFailed;
    private readonly Func<double> _readCpu;
    private readonly Func<(ulong Total, ulong Available)?> _readMemory;

    internal RemoteComputerResourceSampler(Func<double>? readCpu = null,
        Func<(ulong Total, ulong Available)?>? readMemory = null)
    {
        _readCpu = readCpu ?? ReadCpu;
        _readMemory = readMemory ?? ReadMemory;
    }

    internal RemoteComputerResources Capture()
    {
        double? cpu = null;
        if (!_cpuFailed)
        {
            try
            {
                double value = _readCpu();
                if (_primed && double.IsFinite(value) && value >= 0) cpu = Math.Round(Math.Clamp(value, 0, 100), 1);
                _primed = double.IsFinite(value) && value >= 0;
            }
            catch (Exception ex) when (ex is InvalidOperationException or Win32Exception or
                UnauthorizedAccessException or System.Security.SecurityException or NotSupportedException)
            { _cpuFailed = true; _primed = false; }
        }
        var memory = _readMemory();
        const ulong maxSafeInteger = 9_007_199_254_740_991;
        return memory is { Total: > 0 } ram && ram.Total <= maxSafeInteger && ram.Available <= ram.Total
            ? new(cpu, (long)(ram.Total - ram.Available), (long)ram.Total)
            : new(cpu, null, null);
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

    internal void Reset()
    {
        _cpu?.Dispose(); _cpu = null; _primed = false; _cpuFailed = false;
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
