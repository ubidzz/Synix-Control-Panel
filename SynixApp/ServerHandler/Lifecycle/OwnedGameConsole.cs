using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Synix_Control_Panel.SynixEngine;
using Synix_Control_Panel.SynixApp.UI.ServerManagement;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel.SynixApp.ServerHandler;

/// <summary>
/// A private stdin pipe opened when Synix launches the game itself. This deliberately
/// never attaches to a console, injects keys, resolves an incoming PID or writes to
/// cmd.exe. Recovered processes and arbitrary batch launchers are not eligible.
/// </summary>
internal static class OwnedGameConsole
{
    private sealed class Connection(GameServer server, Process process, string executable, DateTime started)
    {
        internal readonly string ServerId = ServerActivityHistory.ServerKey(server);
        internal readonly string Game = server.Game;
        internal readonly string Edition = server.MinecraftEdition;
        internal readonly Process Process = process;
        internal readonly string Executable = executable;
        internal readonly DateTime Started = started;
        internal readonly string RunId = RemoteManagerProtocol.Base64Url(RandomNumberGenerator.GetBytes(32));
        internal bool Writing;
    }
    private static readonly object Gate = new();
    private static readonly Dictionary<GameServer, Connection> Connections = new(ReferenceEqualityComparer.Instance);
    private static readonly Dictionary<GameServer, Form> Windows = new(ReferenceEqualityComparer.Instance);

    internal static bool Supported(GameServer server) => MinecraftControlProfile.IsJava(server) || MinecraftControlProfile.IsBedrock(server);

    internal static ProcessStartInfo? Prepare(GameServer server, string executable, string arguments,
        string directory, bool elevated)
    {
        if (elevated || !Supported(server)) return null;
        if (MinecraftControlProfile.IsJava(server))
        {
            if (!Path.GetFileName(executable).Equals("Start.bat", StringComparison.OrdinalIgnoreCase)) return null;
            string script;
            try { script = File.ReadAllText(executable); } catch (IOException) { return null; }
            if (!TryReadJavaLauncher(script, out string java, out string suffix)) return null;
            executable = ResolveJava(java) ?? "";
            if (executable.Length == 0) return null;
            arguments = arguments + suffix;
        }
        else if (!Path.GetFileName(executable).Equals(MinecraftControlProfile.BedrockExecutableName, StringComparison.OrdinalIgnoreCase)) return null;
        if (!Path.IsPathFullyQualified(executable) || !File.Exists(executable)) return null;
        // No shell interprets these arguments or any subsequently submitted text.
        return new ProcessStartInfo(executable, arguments) {
            WorkingDirectory = directory, UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true,
            StandardInputEncoding = new UTF8Encoding(false)
        };
    }

    internal static bool TryReadJavaLauncher(string script, out string java, out string suffix)
    {
        java = suffix = "";
        // Only Synix's exact generated three-line launcher. Never interpret an
        // arbitrary script, environment expansion, shell chain or fallback command.
        string[] lines = script.ReplaceLineEndings("\n").TrimEnd('\n').Split('\n');
        if (lines.Length != 3 || lines[0] != "@echo off" || lines[2] != "exit /b %errorlevel%") return false;
        var match = Regex.Match(lines[1], "^(?:\"(?<java>[^\"%\\r\\n]+\\\\java\\.exe)\"|(?<java>java)) %\\*(?<suffix>(?: @\"libraries\\\\(?:net\\\\minecraftforge\\\\forge|net\\\\neoforged\\\\neoforge)\\\\[A-Za-z0-9_.-]+\\\\win_args\\.txt\" nogui| -jar \"forge-[A-Za-z0-9_.-]+\\.jar\" nogui)?)$", RegexOptions.CultureInvariant);
        if (!match.Success) return false;
        java = match.Groups["java"].Value; suffix = match.Groups["suffix"].Value;
        return java == "java" || Path.IsPathFullyQualified(java);
    }

    private static string? ResolveJava(string java)
    {
        if (java != "java") return File.Exists(java) ? Path.GetFullPath(java) : null;
        foreach (string part in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
        {
            try {
                string folder = part.Trim('"');
                if (!Path.IsPathFullyQualified(folder)) continue;
                string candidate = Path.Combine(folder, "java.exe");
                if (File.Exists(candidate)) return Path.GetFullPath(candidate);
            } catch (ArgumentException) { }
        }
        return null;
    }

    internal static void Attach(GameServer server, Process process)
    {
        lock (Gate)
        {
            Connections.Remove(server);
            try
            {
                string expected = Path.GetFullPath(process.StartInfo.FileName);
                string name = Path.GetFileName(expected);
                if (!Supported(server) || process.StartInfo.UseShellExecute || !process.StartInfo.RedirectStandardInput ||
                    !(MinecraftControlProfile.IsJava(server) ? name.Equals("java.exe", StringComparison.OrdinalIgnoreCase) : name.Equals(MinecraftControlProfile.BedrockExecutableName, StringComparison.OrdinalIgnoreCase)) ||
                    !string.Equals(process.MainModule?.FileName, expected, StringComparison.OrdinalIgnoreCase) || process.HasExited) return;
                process.StandardInput.AutoFlush = true;
                Connections[server] = new(server, process, expected, process.StartTime.ToUniversalTime());
            }
            catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception or IOException or NotSupportedException) { }
        }
    }

    internal static void ShowWindow(GameServer server)
    {
        lock (Gate)
        {
            if (!Connections.ContainsKey(server)) return;
            if (!Windows.TryGetValue(server, out var window) || window.IsDisposed)
            {
                window = new MinecraftConsoleDialog(server);
                Windows[server] = window;
                window.FormClosed += (_, _) => { lock (Gate) { Connections.Remove(server); Windows.Remove(server); } };
            }
            window.Show();
        }
    }
    internal static void Invalidate(GameServer server, Process? expected = null)
    {
        lock (Gate)
            if (expected == null || (Connections.TryGetValue(server, out var current) && ReferenceEquals(current.Process, expected)))
                Connections.Remove(server);
    }
    internal static void InvalidateAll() { lock (Gate) Connections.Clear(); }

    private static bool IsCurrent(GameServer server, Connection connection)
    {
        try
        {
            return Properties.Settings.Default.ShowServerWindow &&
                ServerActivityHistory.ServerKey(server) == connection.ServerId && server.Game == connection.Game &&
                server.MinecraftEdition == connection.Edition &&
                server.Status == StatusManager.GetStatus(ServerState.Running) &&
                ReferenceEquals(server.RunningProcess, connection.Process) && server.PID == connection.Process.Id &&
                Windows.TryGetValue(server, out var window) && !window.IsDisposed && window.Visible &&
                !connection.Process.HasExited && connection.Process.StartTime.ToUniversalTime() == connection.Started &&
                string.Equals(connection.Process.MainModule?.FileName, connection.Executable, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception or NotSupportedException) { return false; }
    }

    internal static (bool Available, string? RunId, string Support) Describe(GameServer server)
    {
        lock (Gate)
        {
            if (!Properties.Settings.Default.ShowServerWindow) { Connections.Remove(server); return (false, null, "console-disabled"); }
            if (!Supported(server)) return (false, null, "unsupported");
            if (!Connections.TryGetValue(server, out var connection)) return (false, null, "restart-required");
            if (server.Status == StatusManager.GetStatus(ServerState.Starting)) return (false, null, "not-running");
            if (!IsCurrent(server, connection)) { Connections.Remove(server); return (false, null, "not-running"); }
            return (true, connection.RunId, "supported");
        }
    }

    internal static bool ValidCommand(string? command) => command is { Length: > 0 and <= 512 } &&
        !string.IsNullOrWhiteSpace(command) && !command.Any(char.IsControl) &&
        !Regex.IsMatch(command, @"^\s*/?(stop|restart|shutdown|exit|quit)(\s|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    internal static async Task<bool> SendAsync(GameServer server, string? runId, string? command, long expires,
        Func<bool> stillPaired, CancellationToken cancellation)
    {
        if (!ValidCommand(command)) return false;
        Connection connection;
        Task write;
        lock (Gate)
        {
            if (cancellation.IsCancellationRequested || !stillPaired() || expires <= DateTimeOffset.UtcNow.ToUnixTimeSeconds() ||
                !Connections.TryGetValue(server, out connection!) || connection.Writing || connection.RunId != runId || !IsCurrent(server, connection)) return false;
            connection.Writing = true;
            // Only this already-open pipe can receive data, even if the game exits
            // between the identity check and the write. There is no fallback path.
            try { write = connection.Process.StandardInput.WriteLineAsync(command.AsMemory(), cancellation); }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or OperationCanceledException)
            {
                Connections.Remove(server);
                connection.Writing = false;
                return false;
            }
        }
        try { await write.WaitAsync(TimeSpan.FromSeconds(2), cancellation); return true; }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or OperationCanceledException or TimeoutException)
        {
            Invalidate(server, connection.Process);
            // Observe a late pipe failure without retrying the command.
            _ = write.ContinueWith(t => _ = t.Exception, TaskContinuationOptions.OnlyOnFaulted);
            return false;
        }
        finally { lock (Gate) connection.Writing = false; }
    }
}
