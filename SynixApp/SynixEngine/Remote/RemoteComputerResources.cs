using System.ComponentModel;
using System.Diagnostics;
using Synix_Control_Panel.SynixApp.ServerHandler;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel.SynixEngine;

internal sealed record RemoteComputerResources(double? CpuPercent, long? RamUsedBytes)
{
    public string Scope => "game-servers";
    public RemoteSystemResources? Computer { get; init; }
}

internal sealed record RemoteProcessResourceReading(DateTime StartTimeUtc, double? CpuMilliseconds, long? RamBytes);

// Uses only locally tracked game-server processes, including their launchers/workers.
// The viewing loop owns the CPU intervals; no whole-machine counter or extra timer runs.
internal sealed class RemoteComputerResourceSampler : IDisposable
{
    private readonly Dictionary<int, (DateTime Started, double Cpu, double Time)> _previous = new();
    private readonly Func<ServerProcessIdentity, RemoteProcessResourceReading?> _readProcess;
    private readonly Func<double> _clock;
    private readonly int _processorCount;

    internal RemoteComputerResourceSampler(
        Func<ServerProcessIdentity, RemoteProcessResourceReading?>? readProcess = null,
        Func<double>? clock = null, int? processorCount = null)
    {
        _readProcess = readProcess ?? ReadProcess;
        _clock = clock ?? (() => (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency);
        _processorCount = Math.Max(1, processorCount ?? Environment.ProcessorCount);
    }

    // Capture status and registry identities on the UI thread, then read processes off it.
    internal static ServerProcessIdentity[] SnapshotProcesses(IEnumerable<GameServer> servers) => servers
        .Where(server => server.Status == StatusManager.GetStatus(ServerState.Running) ||
            server.Status == StatusManager.GetStatus(ServerState.Starting))
        .SelectMany(Servers.GetServerProcessSnapshot)
        .ToArray();

    internal RemoteComputerResources Capture(IReadOnlyList<ServerProcessIdentity> processes)
    {
        const long maxSafeInteger = 9_007_199_254_740_991;
        HashSet<int> sampled = [], active = [];
        double now = _clock(), cpu = 0;
        long memory = 0;
        bool cpuAvailable = true, memoryAvailable = true;
        foreach (var identity in processes)
        {
            if (identity.ProcessId <= 0 || identity.ProcessId == Environment.ProcessId ||
                !sampled.Add(identity.ProcessId)) continue;
            if (!identity.StartTimeUtc.HasValue)
            {
                cpuAvailable = memoryAvailable = false;
                continue;
            }
            try
            {
                var reading = _readProcess(identity);
                // A stale PID must never count a different program that reused that PID.
                if (reading == null || reading.StartTimeUtc != identity.StartTimeUtc.Value) continue;
                active.Add(identity.ProcessId);
                if (reading.RamBytes is >= 0 and <= maxSafeInteger && memory <= maxSafeInteger - reading.RamBytes.Value)
                    memory += reading.RamBytes.Value;
                else memoryAvailable = false;

                if (reading.CpuMilliseconds is double current && double.IsFinite(current) && current >= 0)
                {
                    if (_previous.TryGetValue(identity.ProcessId, out var previous) && previous.Started == reading.StartTimeUtc &&
                        now > previous.Time && current >= previous.Cpu)
                        cpu += (current - previous.Cpu) / ((now - previous.Time) * 1000 * _processorCount) * 100;
                    else cpuAvailable = false; // Wait for an interval for a newly started process.
                    _previous[identity.ProcessId] = (reading.StartTimeUtc, current, now);
                }
                else { cpuAvailable = false; _previous.Remove(identity.ProcessId); }
            }
            catch (Exception ex) when (ex is InvalidOperationException or Win32Exception or
                UnauthorizedAccessException or System.Security.SecurityException or NotSupportedException)
            { cpuAvailable = memoryAvailable = false; }
        }
        foreach (int pid in _previous.Keys.Where(pid => !active.Contains(pid)).ToArray()) _previous.Remove(pid);
        return new(cpuAvailable ? Math.Round(Math.Clamp(cpu, 0, 100), 1) : null, memoryAvailable ? memory : null);
    }

    private static RemoteProcessResourceReading? ReadProcess(ServerProcessIdentity identity)
    {
        Process process;
        try { process = Process.GetProcessById(identity.ProcessId); }
        catch (ArgumentException) { return null; }
        using (process)
        {
            if (process.HasExited) return null;
            process.Refresh();
            DateTime started = process.StartTime.ToUniversalTime();
            if (started != identity.StartTimeUtc) return null;
            double? cpu = null;
            try { cpu = process.TotalProcessorTime.TotalMilliseconds; }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or NotSupportedException) { }
            return new(started, cpu, process.WorkingSet64);
        }
    }

    internal void Reset() => _previous.Clear();
    public void Dispose() => Reset();
}
