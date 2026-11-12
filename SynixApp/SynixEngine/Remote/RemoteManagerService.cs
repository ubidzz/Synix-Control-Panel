using System.Net;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Synix_Control_Panel.SynixApp.ServerHandler;

namespace Synix_Control_Panel.SynixEngine;

internal sealed record RemotePairingState(string DeviceId, string KeyName, bool Hardware,
    string Label, bool Enabled = false, bool PendingUnpair = false, string? Account = null,
    string[]? ExecutedCommands = null);

/// <summary>Outbound, opt-in manager access. Never sends GameServer models or raw log/configuration data.</summary>
internal sealed class RemoteManagerService
{
    private static readonly Lazy<RemoteManagerService> SharedInstance = new(() => new());
    internal static RemoteManagerService Shared => SharedInstance.Value;
    private static readonly string StatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Synix", "RemoteManager", "pairing.dat");
    private readonly object _gate = new();
    private readonly HttpClient _client = new(CreateTransport())
        { Timeout = TimeSpan.FromSeconds(15), MaxResponseContentBufferSize = 131072 };
    internal static HttpClientHandler CreateTransport() => new()
        { AllowAutoRedirect = false, UseCookies = false, CheckCertificateRevocationList = true };
    private RemotePairingState? _state;
    private CancellationTokenSource? _stop;
    private Task _loop = Task.CompletedTask;
    private bool _operationRunning;
    private readonly RemoteBackupCatalog _backups = new();
    private volatile bool _relayConnected, _apiConnected;
    private readonly SemaphoreSlim _wake = new(0, 1);
    private volatile string _statusKey = "RemoteManager.NotPaired";
    internal bool IsConnected => State?.Enabled == true && _relayConnected && _apiConnected;
    internal string ConnectionStatus => LocalizationManager.Get(ConnectionStatusKey(State, _relayConnected, _apiConnected, _statusKey));
    internal static string ConnectionStatusKey(RemotePairingState? state, bool relay, bool api, string fallback) =>
        state?.Enabled == true ? (relay && api ? "RemoteManager.Connected" : "RemoteManager.Reconnecting") : fallback;
    internal RemotePairingState? State { get { lock (_gate) return _state; } }

    private RemoteManagerService()
    {
        try
        {
            if (File.Exists(StatePath)) _state = JsonSerializer.Deserialize<RemotePairingState>(
                ProtectedData.Unprotect(File.ReadAllBytes(StatePath), null, DataProtectionScope.CurrentUser), RemoteManagerProtocol.Json);
            if (_state is { Enabled: true }) _statusKey = "RemoteManager.WaitingConnection";
            else if (_state is { PendingUnpair: true }) _statusKey = "RemoteManager.RemovalPending";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or CryptographicException or JsonException)
        { _statusKey = "RemoteManager.LoadFailed"; }
    }
    private void Persist(RemotePairingState state)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(StatePath)!);
        byte[] bytes = ProtectedData.Protect(JsonSerializer.SerializeToUtf8Bytes(state, RemoteManagerProtocol.Json), null, DataProtectionScope.CurrentUser);
        string temporary = StatePath + ".tmp";
        File.WriteAllBytes(temporary, bytes);
        File.Move(temporary, StatePath, true);
        _state = state;
    }
    internal static bool IsOwnedKeyName(string? name) => name is not null && name.StartsWith("Synix.Remote.", StringComparison.Ordinal) &&
        Guid.TryParseExact(name["Synix.Remote.".Length..], "N", out _);
    private static CngKey OpenKey(RemotePairingState state)
    {
        if (!IsOwnedKeyName(state.KeyName)) throw new CryptographicException(LocalizationManager.Get("RemoteManager.InvalidSettings"));
        return CngKey.Open(state.KeyName,
            new CngProvider(state.Hardware ? "Microsoft Platform Crypto Provider" : "Microsoft Software Key Storage Provider"));
    }
    private static (CngKey Key, bool Hardware) NewKey(string name)
    {
        CngKey Create(string provider) => CngKey.Create(CngAlgorithm.Rsa, name, new CngKeyCreationParameters
        {
            Provider = new CngProvider(provider), KeyUsage = CngKeyUsages.Signing, ExportPolicy = CngExportPolicies.None,
            Parameters = { new CngProperty("Length", BitConverter.GetBytes(2048), CngPropertyOptions.None) }
        });
        try { return (Create("Microsoft Platform Crypto Provider"), true); }
        catch (CryptographicException) { return (Create("Microsoft Software Key Storage Provider"), false); }
    }
    private async Task<T> SendAsync<T>(string route, object payload, RemotePairingState state, CancellationToken cancellation = default)
    {
        using CngKey key = OpenKey(state);
        using RSA rsa = new RSACng(key);
        using var request = RemoteManagerProtocol.CreateRequest(route, payload, rsa, string.IsNullOrEmpty(state.DeviceId) ? null : state.DeviceId);
        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellation).ConfigureAwait(false);
        if (route.StartsWith("device/recovery/", StringComparison.Ordinal) && !response.IsSuccessStatusCode)
            throw new InvalidOperationException(LocalizationManager.Get(response.StatusCode switch
            {
                HttpStatusCode.Forbidden => "RemoteManager.WaitingApproval",
                HttpStatusCode.TooManyRequests => "RemoteManager.RecoveryLimited",
                HttpStatusCode.Unauthorized => "RemoteManager.RecoveryExpired",
                _ => "RemoteManager.Unavailable"
            }));
        if (response.StatusCode == HttpStatusCode.Unauthorized) throw new RemoteUnpairedException();
        if (!response.IsSuccessStatusCode) throw new HttpRequestException(LocalizationManager.Get("RemoteManager.Unavailable"));
        return JsonSerializer.Deserialize<T>(await response.Content.ReadAsByteArrayAsync(cancellation).ConfigureAwait(false), RemoteManagerProtocol.Json)
            ?? throw new HttpRequestException(LocalizationManager.Get("RemoteManager.InvalidResponse"));
    }
    internal async Task<RemotePairReply> BeginPairingAsync(string label)
    {
        if (State != null) await UnpairAsync().ConfigureAwait(false);
        // Finish the old listener before approving a replacement pairing.
        Task previous;
        lock (_gate) previous = _loop;
        await previous.ConfigureAwait(false);
        if (State?.PendingUnpair == true) throw new InvalidOperationException(LocalizationManager.Get("RemoteManager.FinishUnpair"));
        string keyName = "Synix.Remote." + Guid.NewGuid().ToString("N");
        var created = NewKey(keyName);
        using CngKey key = created.Key;
        using RSA rsa = new RSACng(key);
        var state = new RemotePairingState("", keyName, created.Hardware, label.Trim()[..Math.Min(label.Trim().Length, 60)]);
        try
        {
            RSAParameters publicKey = rsa.ExportParameters(false);
            var reply = await SendAsync<RemotePairReply>("device/register", new { label = state.Label,
                hardware = state.Hardware, publicKey = new { kty = "RSA", n = RemoteManagerProtocol.Base64Url(publicKey.Modulus!), e = RemoteManagerProtocol.Base64Url(publicKey.Exponent!) } }, state).ConfigureAwait(false);
            lock (_gate) Persist(state with { DeviceId = reply.DeviceId });
            _statusKey = "RemoteManager.WaitingApproval";
            return reply;
        }
        catch { key.Delete(); throw; }
    }
    internal async Task<RemotePollReply> CheckPairingAsync()
    {
        var state = State ?? throw new InvalidOperationException(LocalizationManager.Get("RemoteManager.NewCode"));
        return await SendAsync<RemotePollReply>("device/poll", new { }, state).ConfigureAwait(false);
    }
    private RemotePairingState RecoveryState() => State is { Enabled: true, PendingUnpair: false } state
        ? state : throw new InvalidOperationException(LocalizationManager.Get("RemoteManager.NotPaired"));
    private void CheckRecoveryState(RemotePairingState original)
    {
        if (State is not { Enabled: true, PendingUnpair: false } current ||
            current.DeviceId != original.DeviceId || current.KeyName != original.KeyName)
            throw new InvalidOperationException(LocalizationManager.Get("RemoteManager.RecoveryExpired"));
    }
    internal async Task<RemoteRecoveryStatus> RecoveryStatusAsync(CancellationToken cancellation = default)
    {
        var state = RecoveryState();
        var result = await SendAsync<RemoteRecoveryStatus>("device/recovery/status", new { }, state, cancellation).ConfigureAwait(false);
        CheckRecoveryState(state);
        return result;
    }
    internal async Task<RemoteRecoveryPassword> CreateRecoveryPasswordAsync(CancellationToken cancellation = default)
    {
        var state = RecoveryState();
        var challenge = await SendAsync<RemoteRecoveryChallenge>("device/recovery/challenge", new { }, state, cancellation).ConfigureAwait(false);
        CheckRecoveryState(state);
        var result = await SendAsync<RemoteRecoveryPassword>("device/recovery/issue",
            new { recoveryId = challenge.RecoveryId, challenge = challenge.Challenge }, state, cancellation).ConfigureAwait(false);
        CheckRecoveryState(state);
        // Recovery values stay in this dialog's memory. Never persist or log them.
        return result;
    }
    internal async Task CancelRecoveryPasswordAsync(string recoveryId, CancellationToken cancellation = default)
    {
        var state = RecoveryState();
        await SendAsync<JsonElement>("device/recovery/cancel", new { recoveryId }, state, cancellation).ConfigureAwait(false);
    }
    internal async Task ApproveAsync(string claimId, string account)
    {
        var state = State ?? throw new InvalidOperationException(LocalizationManager.Get("RemoteManager.CodeExpired"));
        if (state.Enabled || state.PendingUnpair) throw new InvalidOperationException(LocalizationManager.Get("RemoteManager.NewCode"));
        await SendAsync<JsonElement>("device/approve", new { claimId }, state).ConfigureAwait(false);
        lock (_gate)
        {
            // Unpairing or replacing a pending identity while approval is in flight
            // must never let its delayed response restore remote access locally.
            if (!ReferenceEquals(_state, state))
                throw new InvalidOperationException(LocalizationManager.Get("RemoteManager.NewCode"));
            Persist(state with { Enabled = true, Account = account });
        }
        Start();
    }
    internal void Start()
    {
        lock (_gate)
        {
            if (_state?.Enabled != true || !_loop.IsCompleted) return;
            _stop?.Dispose(); _stop = new CancellationTokenSource();
            _loop = Task.Run(() => RunAsync(_stop.Token));
        }
    }
    internal static void Resume()
    {
        Shared.Start();
        if (Shared.State?.PendingUnpair == true) _ = Shared.UnpairAsync();
    }
    internal static void StopForExit()
    {
        if (!SharedInstance.IsValueCreated) return;
        lock (Shared._gate) Shared._stop?.Cancel();
    }
    internal static void PrepareForUninstall()
    {
        var service = Shared;
        var state = service.State;
        if (state is null) return;
        // Called only after uninstall consent and file-path preflight. Try remote
        // revocation first; offline removal still destroys this computer's key.
        try { service.UnpairAsync().GetAwaiter().GetResult(); }
        finally
        {
            StopForExit();
            if (IsOwnedKeyName(state.KeyName) && CngKey.Exists(state.KeyName,
                new CngProvider(state.Hardware ? "Microsoft Platform Crypto Provider" : "Microsoft Software Key Storage Provider")))
            {
                using CngKey key = OpenKey(state);
                key.Delete();
            }
        }
    }
    internal async Task<bool> UnpairAsync()
    {
        RemotePairingState? state;
        lock (_gate)
        {
            state = _state;
            _stop?.Cancel();
            if (state == null) return true;
            // Save the disabled state BEFORE any network operation. Restart must never re-enable it.
            _state = state with { Enabled = false, PendingUnpair = true };
            try { Persist(_state); }
            catch
            {
                // If local state cannot be saved, destroy this app's key so an old
                // enabled record cannot reconnect after restarting Windows.
                try { using CngKey key = OpenKey(state); key.Delete(); } catch (CryptographicException) { }
                _statusKey = "RemoteManager.SaveFailed";
                throw;
            }
            _statusKey = "RemoteManager.RemovingPairing";
        }
        try
        {
            try { await SendAsync<JsonElement>("device/unpair", new { }, state).ConfigureAwait(false); }
            catch (RemoteUnpairedException) { /* Already removed on the website. */ }
            using CngKey key = OpenKey(state); key.Delete();
            lock (_gate)
            {
                if (_state?.KeyName == state.KeyName) { if (File.Exists(StatePath)) File.Delete(StatePath); _state = null; }
                _statusKey = "RemoteManager.NotPaired";
            }
            return true;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException or CryptographicException)
        {
            _statusKey = "RemoteManager.RemovalPending";
            return false;
        }
    }
    private async Task RunAsync(CancellationToken cancellation)
    {
        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        Task notifications = ListenForWakeAsync(lifetime.Token);
        bool active = false, serverDetails = false, resourcesActive = false;
        using var resources = new RemoteComputerResourceSampler();
        using var computerResources = new RemoteSystemResourceSampler();
        try
        {
        while (!cancellation.IsCancellationRequested)
        {
            var state = State;
            if (state?.Enabled != true) return;
            int delay = Timeout.Infinite;
            try
            {
                // No enumeration, serialization or status upload while nobody is viewing.
                RemoteServerSummary[]? servers = null;
                if (serverDetails && ApplicationUiService.IsAvailable)
                {
                    var snapshot = ApplicationUiService.Invoke(() => ServerRegistry.Snapshot().Take(200)
                        .Select(server => (Server: server, Summary: RemoteManagerProtocol.Summarize(server, Core.Instance.isDownloadActive))).ToArray());
                    // Disk metadata is cached and read off the UI thread, only while a viewer is active.
                    servers = snapshot.Select(x => _backups.AddHealth(x.Server, x.Summary, DateTimeOffset.UtcNow)).ToArray();
                }
                var payload = new Dictionary<string, object>();
                if (servers != null) payload["servers"] = servers;
                if (resourcesActive)
                {
                    RemoteComputerResources serverUsage = new(null, null);
                    if (ApplicationUiService.IsAvailable)
                    {
                        var processes = ApplicationUiService.Invoke(() =>
                            RemoteComputerResourceSampler.SnapshotProcesses(ServerRegistry.Snapshot()));
                        serverUsage = resources.Capture(processes);
                    }
                    payload["resources"] = serverUsage with { Computer = computerResources.Capture() };
                }
                var reply = await SendAsync<RemotePollReply>("device/poll", payload, state, cancellation).ConfigureAwait(false);
                if (!reply.Paired) { _statusKey = "RemoteManager.WaitingApproval"; return; }
                bool firstSnapshot = (!serverDetails && (reply.ServerDetails ?? reply.Active)) ||
                    (!resourcesActive && reply.ResourcesActive == true);
                active = reply.Active;
                serverDetails = reply.ServerDetails ?? active;
                resourcesActive = reply.ResourcesActive == true;
                if (!resourcesActive) { resources.Reset(); computerResources.Reset(); }
                _apiConnected = true;
                delay = active ? 10 : Timeout.Infinite;
                _statusKey = active ? "RemoteManager.Connected" : "RemoteManager.Listening";
                foreach (var command in reply.Commands ?? [])
                {
                    if (cancellation.IsCancellationRequested || State?.Enabled != true) break;
                    _ = ExecuteAsync(command, state, cancellation);
                }
                // Send the first server list immediately after a viewer wakes this connection.
                if (active && firstSnapshot) delay = 1;
            }
            catch (RemoteUnpairedException)
            {
                lock (_gate) if (_state?.DeviceId == state.DeviceId) Persist(_state with { Enabled = false, PendingUnpair = true });
                _statusKey = "RemoteManager.PairingRemoved";
                return;
            }
            catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or IOException or CryptographicException or JsonException or InvalidOperationException)
            { active = false; serverDetails = false; resourcesActive = false; resources.Reset(); computerResources.Reset(); delay = 30; _apiConnected = false; _statusKey = "RemoteManager.Reconnecting"; }
            try { await _wake.WaitAsync(delay == Timeout.Infinite ? Timeout.InfiniteTimeSpan : TimeSpan.FromSeconds(delay), cancellation).ConfigureAwait(false); }
            catch (OperationCanceledException) { return; }
        }
        }
        finally { _apiConnected = false; lifetime.Cancel(); try { await notifications.ConfigureAwait(false); } catch (OperationCanceledException) { } }
    }
    private void Wake()
    {
        try { _wake.Release(); } catch (SemaphoreFullException) { }
    }
    private async Task ListenForWakeAsync(CancellationToken cancellation)
    {
        int backoff = 1;
        while (!cancellation.IsCancellationRequested && State is { Enabled: true } state)
        {
            try
            {
                var relay = await SendAsync<RemoteRelayReply>("device/relay", new { proof = RemoteManagerProtocol.RelayProof }, state, cancellation).ConfigureAwait(false);
                var uri = RemoteManagerProtocol.ValidateRelay(relay);
                if (!IsCurrentPairing(State, state)) return;
                using var socket = new ClientWebSocket();
                using (CngKey key = OpenKey(state))
                using (RSA rsa = new RSACng(key))
                    foreach (var header in RemoteManagerProtocol.CreateRelayHeaders(relay, rsa))
                        socket.Options.SetRequestHeader(header.Key, header.Value);
                socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(45);
                socket.Options.KeepAliveTimeout = TimeSpan.FromSeconds(20);
                using var connection = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
                connection.CancelAfter(TimeSpan.FromMinutes(61));
                // Use the same certificate and redirect policy for HTTPS and WSS.
                await socket.ConnectAsync(uri, _client, connection.Token).ConfigureAwait(false);
                backoff = 1;
                byte[] buffer = new byte[64];
                while (socket.State == WebSocketState.Open && !cancellation.IsCancellationRequested)
                {
                    var message = await socket.ReceiveAsync(buffer.AsMemory(), connection.Token).ConfigureAwait(false);
                    if (message.MessageType == WebSocketMessageType.Close) break;
                    if (!message.EndOfMessage || message.MessageType != WebSocketMessageType.Text) { socket.Abort(); break; }
                    string signal = Encoding.UTF8.GetString(buffer, 0, message.Count);
                    if (signal is "ready" or "wake") { _relayConnected = true; Wake(); }
                }
            }
            catch (RemoteUnpairedException)
            {
                lock (_gate) if (_state?.DeviceId == state.DeviceId) Persist(_state with { Enabled = false, PendingUnpair = true });
                _statusKey = "RemoteManager.PairingRemoved";
                Wake(); return;
            }
            catch (Exception ex) when (ex is HttpRequestException or WebSocketException or OperationCanceledException or IOException or CryptographicException or JsonException or UriFormatException)
            { _statusKey = "RemoteManager.Reconnecting"; }
            finally { _relayConnected = false; }
            try { await Task.Delay(TimeSpan.FromSeconds(backoff) + TimeSpan.FromMilliseconds(Random.Shared.Next(500)), cancellation).ConfigureAwait(false); }
            catch (OperationCanceledException) { return; }
            backoff = Math.Min(60, backoff * 2);
        }
    }
    private async Task ExecuteAsync(RemoteCommand command, RemotePairingState state, CancellationToken cancellation)
    {
        string result = "cancelled";
        RemoteBackupChoice[]? backups = null;
        bool acquired = false, accepted = false;
        try
        {
            lock (_gate)
            {
                if (cancellation.IsCancellationRequested || !IsCurrentPairing(_state, state) || !CanExecute(command, _state, DateTimeOffset.UtcNow)) return;
                accepted = true;
                if (_operationRunning) result = "busy";
                else
                {
                    Persist(_state! with { ExecutedCommands = (_state!.ExecutedCommands ?? []).Append(command.Id).TakeLast(100).ToArray() });
                    _operationRunning = true; acquired = true;
                }
            }
            if (acquired)
            {
                var completion = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
                if (!ApplicationUiService.TryPost(async () =>
                {
                    try
                    {
                        // UI work can wait in the queue. Authorize here, not before posting it,
                        // and check the local pairing and deadline again after the network wait.
                        JsonElement authorization = default;
                        if (!await AuthorizeExecutionAsync(command, state, () => State,
                            async token => authorization = await AuthorizeCommandAsync(command, state, token), cancellation))
                        { completion.TrySetResult("cancelled"); return; }
                        var server = ServerRegistry.Snapshot().FirstOrDefault(x => ServerActivityHistory.ServerKey(x) == command.ServerId);
                        if (server == null) { completion.TrySetResult("unsupported"); return; }
                        var summary = RemoteManagerProtocol.Summarize(server, Core.Instance.isDownloadActive);
                        if (command.Action == "console")
                        {
                            // No command text is written to activity, diagnostics or result data.
                            bool sent = summary.CanConsole && summary.ConsoleRunId == command.ConsoleRunId &&
                                authorization.TryGetProperty("consoleRunId", out var run) && run.GetString() == command.ConsoleRunId &&
                                authorization.TryGetProperty("consoleText", out var text) && text.ValueKind == JsonValueKind.String &&
                                await OwnedGameConsole.SendAsync(server, command.ConsoleRunId, text.GetString(),
                                    command.Expires, () => IsCurrentPairing(State, state), cancellation);
                            completion.TrySetResult(sent ? "sent" : "console-locked"); return;
                        }
                        if (command.Action == "update" && summary.UpdateSupport != "supported") { completion.TrySetResult("unsupported"); return; }
                        bool allowed = command.Action switch { "start" => summary.CanStart, "stop" => summary.CanStop, "restart" => summary.CanRestart, "backup" => summary.CanBackup, "list-backups" or "restore" => summary.CanRestore, "update" => summary.CanUpdate, _ => false };
                        if (!allowed) { completion.TrySetResult(server.IsFirstBoot || server.SteamAuthenticationRequired ? "setup-required" : "busy"); return; }
                        if (command.Action == "list-backups")
                        {
                            var archives = await Core.Instance.GetServerBackupsAsync(server);
                            backups = _backups.Remember(command.ServerId, archives, DateTimeOffset.UtcNow);
                            completion.TrySetResult("completed"); return;
                        }
                        using var history = ServerDiagnosticScope.Begin(server);
                        string operationKey = command.Action switch { "start" => "ServerOperation.Name.Start", "stop" => "ServerOperation.Name.Stop", "restart" => "ServerOperation.Name.Restart", "restore" => "ServerOperation.Name.Restore", "update" => "ServerOperation.Name.Update", _ => "ServerOperation.Name.Backup" };
                        ApplicationLogService.Write(LocalizationManager.Get("RemoteManager.ActionRequested", LocalizationManager.Get(operationKey), server.ServerName), Color.Cyan);
                        if (command.Action == "restore")
                        {
                            var archives = await Core.Instance.GetServerBackupsAsync(server);
                            var backup = _backups.Resolve(command.ServerId, command.BackupId, archives, DateTimeOffset.UtcNow);
                            if (backup is null) { completion.TrySetResult("backup-unavailable"); return; }
                            // Refresh authorization after catalog I/O, immediately before destructive work.
                            if (!await AuthorizeExecutionAsync(command, state, () => State,
                                token => AuthorizeCommandAsync(command, state, token), cancellation))
                            { completion.TrySetResult("cancelled"); return; }
                            var restored = await Core.Instance.RestoreServerBackupAsync(server, backup);
                            completion.TrySetResult(restored.Succeeded ? "completed" : "failed"); return;
                        }
                        bool succeeded = command.Action switch
                        {
                            "start" => await Core.Instance.ExecuteStartSequence(server, remoteRequest: true),
                            "restart" => await Core.Instance.ExecuteStartSequence(server, "RESTART", remoteRequest: true),
                            "stop" => await Core.Instance.StopServerAndReport(server),
                            "backup" => await Core.Instance.ExecuteBackup(server, StartContext.Scheduled),
                            "update" => await Core.Instance.UpdateServerAndReport(server, "UPDATE", remoteRequest: true),
                            _ => false
                        };
                        completion.TrySetResult(succeeded ? "completed" : "failed");
                    }
                    catch { completion.TrySetResult("failed"); }
                })) completion.TrySetResult("cancelled");
                result = await completion.Task.ConfigureAwait(false);
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or CryptographicException or OperationCanceledException or InvalidOperationException) { result = "failed"; }
        finally
        {
            if (acquired) { lock (_gate) _operationRunning = false; _backups.Invalidate(command.ServerId); Wake(); }
            if (accepted && !cancellation.IsCancellationRequested && IsCurrentPairing(State, state))
            {
                try { await SendAsync<JsonElement>("device/result", new { commandId = command.Id, result, backups }, state, cancellation).ConfigureAwait(false); }
                catch { /* At-most-once commands are never replayed after a missing receipt. */ }
            }
        }
    }
    private static bool IsCurrentPairing(RemotePairingState? current, RemotePairingState expected) =>
        current is { Enabled: true, PendingUnpair: false } &&
        current.DeviceId == expected.DeviceId && current.KeyName == expected.KeyName;

    private async Task<JsonElement> AuthorizeCommandAsync(RemoteCommand command, RemotePairingState state, CancellationToken cancellation)
    {
        var reply = await SendAsync<JsonElement>("device/authorize-command", new { commandId = command.Id }, state, cancellation).ConfigureAwait(false);
        if (!RemoteManagerProtocol.Authorizes(command, reply))
            throw new HttpRequestException(LocalizationManager.Get("RemoteManager.InvalidResponse"));
        return reply;
    }

    internal static async Task<bool> AuthorizeExecutionAsync(RemoteCommand command, RemotePairingState expected,
        Func<RemotePairingState?> currentState, Func<CancellationToken, Task> authorize,
        CancellationToken cancellation, Func<DateTimeOffset>? clock = null)
    {
        bool IsCurrent()
        {
            long now = (clock?.Invoke() ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds();
            return !cancellation.IsCancellationRequested && IsCurrentPairing(currentState(), expected) &&
                RemoteManagerProtocol.IsActionAllowed(command.Action) && command.Expires > now && command.Expires <= now + 60;
        }
        if (!IsCurrent()) return false;
        await authorize(cancellation);
        return IsCurrent();
    }

    internal static bool CanExecute(RemoteCommand command, RemotePairingState? state, DateTimeOffset now) =>
        state?.Enabled == true && !state.PendingUnpair && RemoteManagerProtocol.IsActionAllowed(command.Action) &&
        command.Expires > now.ToUnixTimeSeconds() && command.Expires <= now.ToUnixTimeSeconds() + 60 &&
        !(state.ExecutedCommands ?? []).Contains(command.Id, StringComparer.Ordinal);
    private sealed class RemoteUnpairedException : HttpRequestException;
}
