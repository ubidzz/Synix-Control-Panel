using System.Net.Http.Headers;
using System.Security.Cryptography;

namespace Synix_Control_Panel.SynixEngine;

/// <summary>
/// Optional, short-lived presence for the public aggregate at synix.info.
/// A random session credential is held only in memory; no machine or server data is sent.
/// </summary>
internal sealed class UsageStatisticsService : IAsyncDisposable
{
    private static readonly Uri Endpoint = new("https://synix.info/api/presence");
    private static readonly Lazy<UsageStatisticsService> SharedInstance = new(() => new(
        new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false })
        { Timeout = Timeout.InfiniteTimeSpan }, TimeSpan.FromSeconds(60)));

    private readonly object _gate = new();
    private readonly HttpClient _client;
    private readonly TimeSpan _interval;
    private CancellationTokenSource? _cancellation;
    private Task _work = Task.CompletedTask;
    private bool _enabled;
    private bool _disposed;

    internal UsageStatisticsService(HttpClient client, TimeSpan interval)
    {
        _client = client;
        _interval = interval;
    }

    internal static void ApplyPreference()
    {
        bool enabled = Properties.Settings.Default.ShareActivityCount;
        if (enabled || SharedInstance.IsValueCreated)
            _ = SharedInstance.Value.SetEnabledAsync(enabled);
    }

    internal static void StopForExit()
    {
        if (!SharedInstance.IsValueCreated) return;
        // Closing Synix never waits on the network beyond this small bound.
        SharedInstance.Value.SetEnabledAsync(false).Wait(TimeSpan.FromMilliseconds(750));
    }

    internal Task SetEnabledAsync(bool enabled)
    {
        lock (_gate)
        {
            if (_disposed || _enabled == enabled) return enabled ? Task.CompletedTask : _work;
            _enabled = enabled;
            _cancellation?.Cancel();
            if (enabled)
            {
                var cancellation = new CancellationTokenSource();
                _cancellation = cancellation;
                Task previous = _work;
                _work = Task.Run(() => RunAsync(previous, cancellation));
            }
            return enabled ? Task.CompletedTask : _work;
        }
    }

    private async Task RunAsync(Task previous, CancellationTokenSource cancellation)
    {
        string? credential = null;
        try
        {
            await previous.ConfigureAwait(false);
            cancellation.Token.ThrowIfCancellationRequested();
            credential = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(32));
            while (!cancellation.IsCancellationRequested)
            {
                await SendAsync(HttpMethod.Put, credential, cancellation.Token).ConfigureAwait(false);
                await Task.Delay(_interval, cancellation.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            if (credential != null)
                await SendAsync(HttpMethod.Delete, credential, CancellationToken.None).ConfigureAwait(false);
            lock (_gate)
            {
                if (ReferenceEquals(_cancellation, cancellation)) _cancellation = null;
                cancellation.Dispose();
            }
        }
    }

    private async Task SendAsync(HttpMethod method, string credential, CancellationToken cancellation)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
            timeout.CancelAfter(TimeSpan.FromSeconds(2));
            using var request = new HttpRequestMessage(method, Endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credential);
            request.Headers.UserAgent.ParseAdd("Synix");
            using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
            // Any unavailable/rate-limited response is retried only at the normal interval.
        }
        catch (HttpRequestException) { }
        catch (OperationCanceledException) { }
        catch (IOException) { }
        // This optional count must never interrupt normal app use, and credentials must not enter diagnostic logs.
    }

    public async ValueTask DisposeAsync()
    {
        Task pending;
        lock (_gate)
        {
            if (_disposed) return;
            _disposed = true;
            _enabled = false;
            _cancellation?.Cancel();
            pending = _work;
        }
        await pending.ConfigureAwait(false);
        _client.Dispose();
    }
}
