using System.Net;
using System.Threading.Channels;
using Synix_Control_Panel.SynixEngine;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class UsageStatisticsTests
{
    private sealed record Sent(HttpMethod Method, Uri? Uri, string? Credential, bool HasBody, string[] Headers);
    private sealed class Handler : HttpMessageHandler
    {
        internal readonly Channel<Sent> Requests = Channel.CreateUnbounded<Sent>();
        internal Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Response =
            (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Writer.TryWrite(new(request.Method, request.RequestUri, request.Headers.Authorization?.Parameter,
                request.Content != null, request.Headers.Select(header => header.Key).Order().ToArray()));
            return Response(request, cancellationToken);
        }
        internal async Task<Sent> Next() => await Requests.Reader.ReadAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task SharingIsEnabledByDefaultButTurningItOffPreventsNetworkRequests()
    {
        var settings = new Properties.Settings();
        Assert.True(settings.ShareActivityCount);
        settings.ShareActivityCount = false;
        using var handler = new Handler();
        await using var service = new UsageStatisticsService(new HttpClient(handler), TimeSpan.FromHours(1));
        await service.SetEnabledAsync(settings.ShareActivityCount);
        Assert.False(handler.Requests.Reader.TryRead(out _));
    }

    [Fact]
    public async Task OptInSendsOnlyARandomSessionCredentialAndOptOutRemovesThatSession()
    {
        using var handler = new Handler();
        await using var service = new UsageStatisticsService(new HttpClient(handler), TimeSpan.FromHours(1));
        await service.SetEnabledAsync(true);
        Sent heartbeat = await handler.Next();
        Assert.Equal(HttpMethod.Put, heartbeat.Method);
        Assert.Equal("https://synix.info/api/presence", heartbeat.Uri?.AbsoluteUri);
        Assert.Matches("^[0-9a-f]{64}$", heartbeat.Credential!);
        Assert.False(heartbeat.HasBody);
        Assert.Equal(new[] { "Authorization", "User-Agent" }, heartbeat.Headers);
        await service.SetEnabledAsync(true); // Reapplying a setting must not create a second reporting loop.
        await service.SetEnabledAsync(false);
        Sent removal = await handler.Next();
        Assert.Equal(HttpMethod.Delete, removal.Method);
        Assert.Equal(heartbeat.Credential, removal.Credential);
        Assert.False(handler.Requests.Reader.TryRead(out _));
    }

    [Fact]
    public async Task EnablingAgainUsesANewCredentialInsteadOfAPermanentDeviceIdentifier()
    {
        using var handler = new Handler();
        await using var service = new UsageStatisticsService(new HttpClient(handler), TimeSpan.FromHours(1));
        await service.SetEnabledAsync(true);
        Sent first = await handler.Next();
        await service.SetEnabledAsync(false);
        Assert.Equal(HttpMethod.Delete, (await handler.Next()).Method);
        await service.SetEnabledAsync(true);
        Assert.NotEqual(first.Credential, (await handler.Next()).Credential);
    }

    [Fact]
    public async Task UnavailableWebsiteDoesNotStopTheAppAndTheNextIntervalRetries()
    {
        using var handler = new Handler { Response = (_, _) => throw new HttpRequestException("offline") };
        await using var service = new UsageStatisticsService(new HttpClient(handler), TimeSpan.FromMilliseconds(30));
        await service.SetEnabledAsync(true);
        Sent first = await handler.Next();
        Sent next = await handler.Next();
        Assert.Equal(HttpMethod.Put, next.Method);
        Assert.Equal(first.Credential, next.Credential);
        await service.SetEnabledAsync(false);
    }

    [Fact]
    public async Task OptOutCancelsAnInFlightRequestAndDoesNotSendMoreHeartbeats()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var handler = new Handler { Response = async (request, cancellation) =>
        {
            if (request.Method == HttpMethod.Put)
            {
                entered.TrySetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellation);
            }
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }};
        await using var service = new UsageStatisticsService(new HttpClient(handler), TimeSpan.FromHours(1));
        await service.SetEnabledAsync(true);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await service.SetEnabledAsync(false).WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(HttpMethod.Put, (await handler.Next()).Method);
        Assert.Equal(HttpMethod.Delete, (await handler.Next()).Method);
        Assert.False(handler.Requests.Reader.TryRead(out _));
    }
}
