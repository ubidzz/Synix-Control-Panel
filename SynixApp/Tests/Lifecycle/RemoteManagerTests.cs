using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using Synix_Control_Panel.SynixApp.Localization;
using Synix_Control_Panel.SynixApp.UI.Settings;
using Synix_Control_Panel.SynixEngine;
using Xunit;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel.Tests;

public sealed class RemoteManagerTests
{
    [Fact]
    public void BadgeSeparatesSavedPairingFromAnActualConnection()
    {
        var paired = new RemotePairingState("device", "key", false, "VM", Enabled: true);
        Assert.Equal("RemoteManager.Reconnecting", RemoteManagerService.ConnectionStatusKey(paired, false, false, "RemoteManager.Listening"));
        Assert.Equal("RemoteManager.Reconnecting", RemoteManagerService.ConnectionStatusKey(paired, false, true, "RemoteManager.Listening"));
        Assert.Equal("RemoteManager.Reconnecting", RemoteManagerService.ConnectionStatusKey(paired, true, false, "RemoteManager.Connected"));
        Assert.Equal("RemoteManager.Connected", RemoteManagerService.ConnectionStatusKey(paired, true, true, "RemoteManager.Listening"));
        Assert.Equal("RemoteManager.NotPaired", RemoteManagerService.ConnectionStatusKey(null, true, true, "RemoteManager.NotPaired"));
    }

    [Fact]
    public void RestoreChoicesAreOpaqueScopedExpiringAndCannotSelectChangedOrInvalidArchives()
    {
        var now = DateTimeOffset.UtcNow;
        var archive = new ServerBackupArchive(@"C:\Private\Backups\World.zip", now.UtcDateTime, 1000, 2000, ServerBackupIntegrity.Recorded, now.UtcDateTime);
        var invalid = archive with { ArchivePath = @"C:\Private\Backups\Broken.zip", Integrity = ServerBackupIntegrity.Invalid };
        var catalog = new RemoteBackupCatalog();
        var options = catalog.Remember("server-one", [archive, invalid], now);
        var choice = Assert.Single(options);
        Assert.DoesNotContain("Private", JsonSerializer.Serialize(choice));
        Assert.NotEqual(archive.ArchivePath, choice.Id);
        Assert.Equal(archive, catalog.Resolve("server-one", choice.Id, [archive], now));
        Assert.Null(catalog.Resolve("server-two", choice.Id, [archive], now));
        Assert.Null(catalog.Resolve("server-one", archive.ArchivePath, [archive], now));
        Assert.Null(catalog.Resolve("server-one", choice.Id, [], now));
        Assert.Null(catalog.Resolve("server-one", choice.Id, [archive with { CompressedBytes = 2000 }], now));
        Assert.Null(catalog.Resolve("server-one", choice.Id, [archive with { CreatedUtc = now.AddSeconds(1).UtcDateTime }], now));
        Assert.Null(catalog.Resolve("server-one", choice.Id, [archive], now.AddMinutes(10)));
        catalog.Remember("server-one", [archive], now);
        Assert.Null(catalog.Resolve("server-one", choice.Id, [archive], now));
    }

    [Fact]
    public void RestoreIsOnlyAvailableForAStoppedServerWithoutAProcess()
    {
        var server = new GameServer { ServerName = "test", Game = "Enshrouded", InstallPath = @"C:\Test", Status = StatusManager.GetStatus(ServerState.Stopped) };
        Assert.True(RemoteManagerProtocol.Summarize(server).CanRestore);
        server.Status = StatusManager.GetStatus(ServerState.Running);
        Assert.False(RemoteManagerProtocol.Summarize(server).CanRestore);
        server.Status = StatusManager.GetStatus(ServerState.Stopped); server.PID = 123;
        Assert.False(RemoteManagerProtocol.Summarize(server).CanRestore);
    }

    [Fact]
    public void RemoteUpdateRequiresAReadyStoppedServerAndNoActiveDownloadOrProcess()
    {
        var server = new GameServer { Game = "Enshrouded", ServerName = "test", InstallPath = @"C:\Fixture\Server",
            IsFirstBoot = false, Status = StatusManager.GetStatus(ServerState.Stopped) };
        Assert.Equal("supported", RemoteManagerProtocol.Summarize(server).UpdateSupport);
        Assert.True(RemoteManagerProtocol.Summarize(server).CanUpdate);
        Assert.False(RemoteManagerProtocol.Summarize(server, downloadActive: true).CanUpdate);
        foreach (ServerState state in Enum.GetValues<ServerState>().Where(x => x != ServerState.Stopped))
        {
            server.Status = StatusManager.GetStatus(state);
            Assert.False(RemoteManagerProtocol.Summarize(server).CanUpdate);
        }
        server.Status = StatusManager.GetStatus(ServerState.Stopped);
        server.PID = 123; Assert.False(RemoteManagerProtocol.Summarize(server).CanUpdate); server.PID = null;
        server.SteamPID = 123; Assert.False(RemoteManagerProtocol.Summarize(server).CanUpdate); server.SteamPID = null;
        server.IsFirstBoot = true; Assert.False(RemoteManagerProtocol.Summarize(server).CanUpdate); server.IsFirstBoot = false;
        server.SteamAuthenticationRequired = true; Assert.False(RemoteManagerProtocol.Summarize(server).CanUpdate); server.SteamAuthenticationRequired = false;
        server.InstallPath = ""; Assert.False(RemoteManagerProtocol.Summarize(server).CanUpdate);
    }

    [Theory]
    [InlineData("Minecraft Java", "local-only")]
    [InlineData("Minecraft Bedrock", "local-only")]
    [InlineData("Arma 3", "local-only")]
    [InlineData("Unknown game", "unavailable")]
    public async Task RemoteUpdatesCannotOpenInteractiveLocalUpdateOrSteamLoginFlows(string game, string support)
    {
        var server = new GameServer { Game = game, ServerName = "test", InstallPath = @"C:\Fixture\Server",
            IsFirstBoot = false, Status = StatusManager.GetStatus(ServerState.Stopped), SteamAccountName = "already_configured" };
        Assert.Equal(support, RemoteManagerProtocol.Summarize(server).UpdateSupport);
        Assert.False(RemoteManagerProtocol.Summarize(server).CanUpdate);
        Assert.False(await Core.Instance.UpdateServerAndReport(server, "UPDATE", remoteRequest: true));
        Assert.Equal(StatusManager.GetStatus(ServerState.Stopped), server.Status);
        Assert.Null(server.SteamPID);
    }

    [Fact]
    public async Task RemoteUpdateEntryPointRechecksStateBeforeConfirmationOrDiskChanges()
    {
        var server = new GameServer { Game = "Enshrouded", InstallPath = @"C:\Fixture\Server", IsFirstBoot = true,
            Status = StatusManager.GetStatus(ServerState.Stopped) };
        Assert.False(await Core.Instance.UpdateServerAndReport(server, "UPDATE", remoteRequest: true));
        server.IsFirstBoot = false;
        Assert.False(await Core.Instance.UpdateServerAndReport(server, "VALIDATE", remoteRequest: true));
        server.PID = 123;
        Assert.False(await Core.Instance.UpdateServerAndReport(server, "UPDATE", remoteRequest: true));
        Assert.Equal(123, server.PID);
        Assert.Null(server.SteamPID);
    }

    [Fact]
    public void LanguageSwitchTranslatesRemoteControlsAndStatusWithoutChangingPairingData()
    {
        WorkflowUiTest.Run(() =>
        {
            try
            {
                LocalizationManager.Initialize("en-US");
                using var dialog = new RemoteManagerDialog();
                using var recovery = new RemoteRecoveryDialog(RemoteManagerService.Shared);
                T Field<T>(string name) where T : Control => Assert.IsType<T>(
                    typeof(RemoteManagerDialog).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(dialog));
                TextBox name = Field<TextBox>("_name"), code = Field<TextBox>("_code");
                Label claim = Field<Label>("_claim"), status = Field<Label>("_status");
                const string computerName = "Save_世界_сервер", pairingCode = "ABCD-1234", username = "Save";
                name.Text = computerName;
                LocalizationManager.SetDataText(code, pairingCode);
                LocalizationManager.BindText(claim, "RemoteManager.ClaimFormat", username);
                string englishStatus = RemoteManagerService.Shared.ConnectionStatus;
                ResourceManager resources = new("Synix_Control_Panel.Localization.Strings", typeof(LocalizationManager).Assembly);
                ResourceSet english = resources.GetResourceSet(CultureInfo.InvariantCulture, true, false)!;
                string[] keys = english.Cast<DictionaryEntry>().Select(entry => (string)entry.Key)
                    .Where(key => key.StartsWith("RemoteManager.", StringComparison.Ordinal)).ToArray();
                Assert.NotEmpty(keys);

                foreach (var language in LocalizationManager.SupportedLanguages)
                {
                    bool isEnglish = language.Code == LocalizationManager.DefaultLanguageCode;
                    ResourceSet catalog = isEnglish ? english : resources.GetResourceSet(
                        CultureInfo.GetCultureInfo(language.ResourceCultureName), true, false)!;
                    Assert.NotNull(catalog);
                    foreach (string key in keys)
                        Assert.False(string.IsNullOrWhiteSpace(catalog.GetString(key)), $"{language.Code} is missing {key}.");
                    LocalizationManager.SetLanguage(language.Code);
                    LocalizationManager.Apply(dialog);
                    LocalizationManager.Apply(recovery);
                    Assert.Equal(LocalizationManager.Get("RemoteManager.RecoveryTitle"), recovery.Text);
                    Assert.Equal(LocalizationManager.Get("RemoteManager.Title"), dialog.Text);
                    Assert.Equal(LocalizationManager.Get("RemoteManager.GenerateCode"), Field<Button>("_create").Text);
                    Assert.Equal(LocalizationManager.Get("RemoteManager.Approve"), Field<Button>("_approve").Text);
                    Assert.Equal(LocalizationManager.Get("RemoteManager.Unpair"), Field<Button>("_unpair").Text);
                    Assert.Equal(LocalizationManager.Get("RemoteManager.ClaimFormat", username), claim.Text);
                    Assert.StartsWith(RemoteManagerService.Shared.ConnectionStatus, status.Text);
                    if (!isEnglish) Assert.NotEqual(englishStatus, RemoteManagerService.Shared.ConnectionStatus);
                    Assert.Equal(computerName, name.Text);
                    Assert.Equal(pairingCode, code.Text);
                }
            }
            finally { LocalizationManager.Initialize(LocalizationManager.DefaultLanguageCode); }
        });
    }

    [Fact]
    public void CounterHeartbeatUsesTenMinutes() => Assert.Equal(TimeSpan.FromMinutes(10), UsageStatisticsService.HeartbeatInterval);

    [Fact]
    public void KeyCleanupAcceptsOnlyThisFeaturesGeneratedKeyNames()
    {
        Assert.True(RemoteManagerService.IsOwnedKeyName("Synix.Remote." + Guid.NewGuid().ToString("N")));
        Assert.False(RemoteManagerService.IsOwnedKeyName(null));
        Assert.False(RemoteManagerService.IsOwnedKeyName("Synix.Remote."));
        Assert.False(RemoteManagerService.IsOwnedKeyName("Another application key"));
        Assert.False(RemoteManagerService.IsOwnedKeyName("Synix.Remote.other-app-key"));
    }

    [Fact]
    public async Task ComputerProofBindsMethodPathTimestampNonceAndExactBody()
    {
        using RSA key = RSA.Create(2048);
        using var request = RemoteManagerProtocol.CreateRequest("device/poll", new { servers = new[] { "hello" } }, key, "test-device");
        byte[] body = await request.Content!.ReadAsByteArrayAsync();
        string when = request.Headers.GetValues("X-Synix-Time").Single();
        string nonce = request.Headers.GetValues("X-Synix-Nonce").Single();
        string message = $"2\nhttps://manager.synix.info\ntest-device\nPOST\n/api/manager/device/poll\n{when}\n{nonce}\n{Convert.ToHexStringLower(SHA256.HashData(body))}";
        string encoded = request.Headers.GetValues("X-Synix-Signature").Single().Replace('-', '+').Replace('_', '/');
        byte[] signature = Convert.FromBase64String(encoded.PadRight((encoded.Length + 3) / 4 * 4, '='));
        Assert.True(key.VerifyData(Encoding.UTF8.GetBytes(message), signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
        Assert.False(key.VerifyData(Encoding.UTF8.GetBytes(message.Replace("poll", "unpair")), signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
        Assert.False(key.VerifyData(Encoding.UTF8.GetBytes(message.Replace("test-device", "other-device")), signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
        Assert.False(key.VerifyData(Encoding.UTF8.GetBytes(message.Replace("manager.synix.info", "mirror.example")), signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
        Assert.Equal("2", request.Headers.GetValues("X-Synix-Protocol").Single());
        Assert.Equal("https", request.RequestUri!.Scheme);
        Assert.DoesNotContain("Authorization", request.Headers.Select(header => header.Key));
        Assert.False(RemoteManagerProtocol.IsActionAllowed("recovery/issue"));
    }

    [Theory]
    [InlineData("device/poll?redirect=other")]
    [InlineData("device/poll#unpair")]
    [InlineData("device/../account/delete")]
    [InlineData("device/unknown")]
    public void SigningIsLimitedToKnownComputerEndpoints(string route)
    {
        using RSA key = RSA.Create(2048);
        Assert.Throws<ArgumentException>(() => RemoteManagerProtocol.CreateRequest(route, new { }, key));
    }

    [Theory]
    [InlineData("wss://another-tenant.workers.dev/connect")]
    [InlineData("wss://synix-wake-relay.synix-website.workers.dev.evil.example/connect")]
    [InlineData("ws://synix-wake-relay.synix-website.workers.dev/connect")]
    [InlineData("wss://synix-wake-relay.synix-website.workers.dev:8443/connect")]
    [InlineData("wss://synix-wake-relay.synix-website.workers.dev/other")]
    [InlineData("wss://synix-wake-relay.synix-website.workers.dev/connect?token=leak")]
    [InlineData("wss://synix-wake-relay.synix-website.workers.dev/connect#fragment")]
    [InlineData("wss://user@synix-wake-relay.synix-website.workers.dev/connect")]
    public void RelayCredentialsNeverGoToAnUnapprovedEndpoint(string endpoint)
    {
        Assert.Throws<HttpRequestException>(() => RemoteManagerProtocol.ValidateRelay(
            new(endpoint, "test.token.signature", 3600, RemoteManagerProtocol.RelayProof)));
    }

    [Fact]
    public void RelayHandshakeRequiresKeyProofAndBindsTheExactTokenAndDestination()
    {
        using RSA key = RSA.Create(2048), otherKey = RSA.Create(2048);
        var reply = new RemoteRelayReply(RemoteManagerProtocol.RelayEndpoint.AbsoluteUri, "test.token.signature", 3600, RemoteManagerProtocol.RelayProof);
        var headers = RemoteManagerProtocol.CreateRelayHeaders(reply, key);
        string message = $"1\nhttps://synix-wake-relay.synix-website.workers.dev\nGET\n/connect\n{headers["X-Synix-Relay-Time"]}\n{headers["X-Synix-Relay-Nonce"]}\n{Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(reply.Token)))}";
        string encoded = headers["X-Synix-Relay-Signature"].Replace('-', '+').Replace('_', '/');
        byte[] proof = Convert.FromBase64String(encoded.PadRight((encoded.Length + 3) / 4 * 4, '='));
        Assert.True(key.VerifyData(Encoding.UTF8.GetBytes(message), proof, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
        Assert.False(otherKey.VerifyData(Encoding.UTF8.GetBytes(message), proof, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
        Assert.False(key.VerifyData(Encoding.UTF8.GetBytes(message.Replace("/connect", "/disconnect")), proof, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
        Assert.False(key.VerifyData(Encoding.UTF8.GetBytes(message + "changed token"), proof, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
        Assert.NotEqual(headers["X-Synix-Relay-Nonce"], RemoteManagerProtocol.CreateRelayHeaders(reply, key)["X-Synix-Relay-Nonce"]);
        foreach (var invalid in new[] { reply with { Proof = null }, reply with { ExpiresIn = 3601 }, reply with { ExpiresIn = 0 }, reply with { Token = "token\r\nheader" } })
            Assert.Throws<HttpRequestException>(() => RemoteManagerProtocol.ValidateRelay(invalid));
    }

    [Fact]
    public void WebsiteApprovalMustExplicitlyMatchEveryCommandField()
    {
        var command = new RemoteCommand("command", "server", "restore", 1800000030, "backup", "run");
        JsonElement Reply(object value) => JsonSerializer.SerializeToElement(value, RemoteManagerProtocol.Json);
        Assert.True(RemoteManagerProtocol.Authorizes(command, Reply(new { authorized = true, command })));
        foreach (var changed in new[] { command with { Id = "other" }, command with { ServerId = "other" },
            command with { Action = "stop" }, command with { Expires = command.Expires + 1 },
            command with { BackupId = "other" }, command with { ConsoleRunId = "other" } })
            Assert.False(RemoteManagerProtocol.Authorizes(command, Reply(new { authorized = true, command = changed })));
        foreach (var invalid in new[] { Reply(new { authorized = false, command }), Reply(new { command }),
            Reply(new { authorized = "true", command }), Reply(new { authorized = true }), Reply(new { authorized = true, command = "invalid" }), Reply(new { }) })
            Assert.False(RemoteManagerProtocol.Authorizes(command, invalid));
    }

    [Theory]
    [InlineData("start")]
    [InlineData("update")]
    public void CommandsRequireEnabledPairingAllowlistedActionFreshExpiryAndNoReplay(string action)
    {
        var now = DateTimeOffset.UtcNow;
        var state = new RemotePairingState("device", "key", true, "PC", Enabled: true);
        var command = new RemoteCommand("command", "server", action, now.ToUnixTimeSeconds() + 30);
        Assert.True(RemoteManagerService.CanExecute(command, state, now));
        Assert.False(RemoteManagerService.CanExecute(command, state with { Enabled = false }, now));
        Assert.False(RemoteManagerService.CanExecute(command, state with { PendingUnpair = true }, now));
        Assert.False(RemoteManagerService.CanExecute(command, state with { ExecutedCommands = ["command"] }, now));
        Assert.False(RemoteManagerService.CanExecute(command with { Action = "shell" }, state, now));
        Assert.False(RemoteManagerService.CanExecute(command with { Expires = now.ToUnixTimeSeconds() - 1 }, state, now));
        Assert.False(RemoteManagerService.CanExecute(command with { Expires = now.ToUnixTimeSeconds() + 3600 }, state, now));
    }

    [Theory]
    [InlineData("unpaired")]
    [InlineData("disabled")]
    [InlineData("pending-unpair")]
    [InlineData("different-computer")]
    [InlineData("different-key")]
    [InlineData("expired")]
    [InlineData("cancelled")]
    public async Task ACommandLosesPermissionWhenPairingOrDeadlineChangesDuringAuthorization(string change)
    {
        var now = DateTimeOffset.UtcNow;
        var expected = new RemotePairingState("device", "key", true, "PC", Enabled: true);
        RemotePairingState? current = expected;
        var command = new RemoteCommand("command", "server", "start", now.ToUnixTimeSeconds() + 30);
        using var cancellation = new CancellationTokenSource();
        var response = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var checking = RemoteManagerService.AuthorizeExecutionAsync(command, expected, () => current,
            _ => response.Task, cancellation.Token, () => now);
        Assert.False(checking.IsCompleted);
        switch (change)
        {
            case "unpaired": current = null; break;
            case "disabled": current = expected with { Enabled = false }; break;
            case "pending-unpair": current = expected with { PendingUnpair = true }; break;
            case "different-computer": current = expected with { DeviceId = "replacement" }; break;
            case "different-key": current = expected with { KeyName = "replacement" }; break;
            case "expired": now = now.AddSeconds(30); break;
            case "cancelled": cancellation.Cancel(); break;
        }
        response.SetResult();
        Assert.False(await checking);
    }

    [Theory]
    [InlineData("restore")]
    [InlineData("update")]
    public async Task ExecutionRequiresFreshWebsitePermissionAndDoesNotAuthorizeExpiredQueuedWork(string action)
    {
        var now = DateTimeOffset.UtcNow;
        var state = new RemotePairingState("device", "key", true, "PC", Enabled: true);
        var command = new RemoteCommand("command", "server", action, now.ToUnixTimeSeconds() + 30);
        int calls = 0;
        Task Authorize(CancellationToken _) { calls++; return Task.CompletedTask; }
        Assert.True(await RemoteManagerService.AuthorizeExecutionAsync(command, state, () => state, Authorize, default, () => now));
        Assert.Equal(1, calls);
        now = now.AddSeconds(30);
        Assert.False(await RemoteManagerService.AuthorizeExecutionAsync(command, state, () => state, Authorize, default, () => now));
        Assert.Equal(1, calls);
        now = now.AddSeconds(-30);
        await Assert.ThrowsAsync<HttpRequestException>(() => RemoteManagerService.AuthorizeExecutionAsync(command, state, () => state,
            _ => Task.FromException(new HttpRequestException("Permission revoked")), default, () => now));
    }

    [Fact]
    public void ServerSummaryOmitsSecretsAndRequiresLocalSetupBeforeRemoteStart()
    {
        var server = new GameServer { ServerName = "Friends", Game = "Enshrouded", InstallPath = "C:\\Private\\Server", IsFirstBoot = true,
            Status = StatusManager.GetStatus(ServerState.Stopped) };
        var summary = RemoteManagerProtocol.Summarize(server);
        Assert.False(summary.CanStart);
        string json = JsonSerializer.Serialize(summary, RemoteManagerProtocol.Json);
        Assert.DoesNotContain("Private", json);
        Assert.DoesNotContain("password", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("installPath", json);
        server.IsFirstBoot = false;
        Assert.True(RemoteManagerProtocol.Summarize(server).CanStart);
        server.SteamAuthenticationRequired = true;
        Assert.False(RemoteManagerProtocol.Summarize(server).CanStart);
    }
}
