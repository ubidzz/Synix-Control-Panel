using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.ServerHandler;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel.SynixEngine;

internal sealed record RemoteServerSummary(string Id, string Name, string Game, string Status,
    string Players, string Uptime, bool CanStart, bool CanStop, bool CanRestart, bool CanBackup,
    bool CanRestore = false, long? LastBackupUtc = null, string BackupState = "unknown",
    bool CanUpdate = false, string UpdateSupport = "unknown", bool CanConsole = false,
    string? ConsoleRunId = null, string ConsoleSupport = "unknown");
internal sealed record RemoteCommand(string Id, string ServerId, string Action, long Expires, string? BackupId = null,
    string? ConsoleRunId = null);
internal sealed record RemotePairReply(string DeviceId, string Code, int ExpiresIn, string ManagerUrl);
internal sealed record RemoteRelayReply(string Url, string Token, int ExpiresIn, string? Proof = null);
internal sealed record RemoteRecoveryStatus(bool Enabled, string Username);
internal sealed record RemoteRecoveryChallenge(string RecoveryId, string Challenge, int ExpiresIn);
internal sealed record RemoteRecoveryPassword(string RecoveryId, string TemporaryPassword, string Username, long ExpiresAt);
internal sealed record RemotePollReply(bool Paired, bool Active, int NextPollSeconds,
    string? Username, string? ClaimId, RemoteCommand[]? Commands,
    bool? ServerDetails = null, bool? ResourcesActive = null);

internal static class RemoteManagerProtocol
{
    internal static readonly Uri Origin = new("https://manager.synix.info/");
    internal const string RelayProof = "rsa-sha256-v1";
    internal static readonly Uri RelayEndpoint = new("wss://synix-wake-relay.synix-website.workers.dev/connect");
    internal static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    internal static string Base64Url(byte[] value) => Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    internal static bool IsActionAllowed(string? action) => action is "start" or "stop" or "restart" or "backup" or "list-backups" or "restore" or "update" or "console";
    internal static HttpRequestMessage CreateRequest(string route, object payload, RSA key, string? deviceId = null)
    {
        if (route is not ("device/register" or "device/poll" or "device/approve" or "device/unpair" or
            "device/relay" or "device/result" or "device/authorize-command" or "device/recovery/status" or
            "device/recovery/challenge" or "device/recovery/issue" or "device/recovery/cancel"))
            throw new ArgumentException(LocalizationManager.Get("RemoteManager.InvalidSettings"), nameof(route));
        var uri = new Uri(Origin, "api/manager/" + route);
        byte[] body = JsonSerializer.SerializeToUtf8Bytes(payload, Json);
        string time = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture);
        string nonce = Base64Url(RandomNumberGenerator.GetBytes(24));
        string message = $"2\n{Origin.GetLeftPart(UriPartial.Authority)}\n{deviceId ?? ""}\nPOST\n{uri.AbsolutePath}\n{time}\n{nonce}\n{Convert.ToHexStringLower(SHA256.HashData(body))}";
        byte[] signature = key.SignData(Encoding.UTF8.GetBytes(message), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = new ByteArrayContent(body) };
        request.Content.Headers.ContentType = new("application/json");
        request.Headers.Add("X-Synix-Time", time);
        request.Headers.Add("X-Synix-Protocol", "2");
        request.Headers.Add("X-Synix-Nonce", nonce);
        request.Headers.Add("X-Synix-Signature", Base64Url(signature));
        if (deviceId != null) request.Headers.Add("X-Synix-Device", deviceId);
        return request;
    }
    internal static Uri ValidateRelay(RemoteRelayReply relay)
    {
        if (!Uri.TryCreate(relay.Url, UriKind.Absolute, out var uri) ||
            uri.Scheme != RelayEndpoint.Scheme || uri.Host != RelayEndpoint.Host || uri.Port != 443 ||
            uri.AbsolutePath != RelayEndpoint.AbsolutePath || uri.UserInfo.Length != 0 ||
            uri.Query.Length != 0 || uri.Fragment.Length != 0 || relay.Proof != RelayProof ||
            relay.ExpiresIn is < 1 or > 3600 || string.IsNullOrEmpty(relay.Token) || relay.Token.Length > 3000 ||
            relay.Token.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.')))
            throw new HttpRequestException(LocalizationManager.Get("RemoteManager.InvalidSettings"));
        return uri;
    }
    internal static Dictionary<string, string> CreateRelayHeaders(RemoteRelayReply relay, RSA key)
    {
        var uri = ValidateRelay(relay);
        string time = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture);
        string nonce = Base64Url(RandomNumberGenerator.GetBytes(24));
        // A copied relay token is insufficient: the paired computer must prove possession of its private key.
        string message = $"1\nhttps://{uri.Host}\nGET\n{uri.AbsolutePath}\n{time}\n{nonce}\n{Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(relay.Token)))}";
        return new()
        {
            ["Authorization"] = "Bearer " + relay.Token,
            ["X-Synix-Relay-Time"] = time,
            ["X-Synix-Relay-Nonce"] = nonce,
            ["X-Synix-Relay-Signature"] = Base64Url(key.SignData(Encoding.UTF8.GetBytes(message), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
        };
    }
    internal static bool Authorizes(RemoteCommand command, JsonElement reply)
    {
        if (reply.ValueKind != JsonValueKind.Object || !reply.TryGetProperty("authorized", out var allowed) ||
            allowed.ValueKind != JsonValueKind.True || !reply.TryGetProperty("command", out var approved)) return false;
        try { return approved.Deserialize<RemoteCommand>(Json) == command; }
        catch (JsonException) { return false; }
    }
    internal static RemoteServerSummary Summarize(GameServer server, bool downloadActive = false)
    {
        string state = Enum.GetValues<ServerState>().Where(value =>
            StatusManager.GetStatus(value) == server.Status).Select(value => value.ToString()).FirstOrDefault() ?? "Unknown";
        bool stopped = server.Status == StatusManager.GetStatus(ServerState.Stopped) && !server.PID.HasValue;
        bool running = server.Status == StatusManager.GetStatus(ServerState.Running);
        bool ready = !server.IsFirstBoot && !server.SteamAuthenticationRequired;
        var console = OwnedGameConsole.Describe(server);
        string updateSupport = GetUpdateSupport(server);
        bool canUpdate = stopped && ready && !downloadActive && !server.SteamPID.HasValue &&
            !string.IsNullOrWhiteSpace(server.InstallPath) && updateSupport == "supported";
        return new(ServerActivityHistory.ServerKey(server), Clip(server.ServerName, 100), Clip(server.Game, 100),
            state, Clip(server.PlayerCount, 24), Clip(server.Uptime, 30), stopped && ready,
            running, running && ready, stopped && !string.IsNullOrWhiteSpace(server.InstallPath),
            stopped && !string.IsNullOrWhiteSpace(server.InstallPath), CanUpdate: canUpdate, UpdateSupport: updateSupport,
            CanConsole: console.Available && ready && !downloadActive, ConsoleRunId: console.RunId, ConsoleSupport: console.Support);
    }
    internal static string GetUpdateSupport(GameServer server)
    {
        var game = GameDatabase.GetGame(server.Game);
        if (game == null || string.IsNullOrWhiteSpace(game.AppID)) return "unavailable";
        // These local flows require a runtime/version choice or interactive Steam
        // authorization. A remote request must never leave a desktop prompt waiting.
        if (GameDatabase.IsMinecraft(server.Game) || game.RequiresSteamLogin) return "local-only";
        return "supported";
    }
    private static string Clip(string? value, int limit)
    {
        string safe = SecretRedactor.Redact(value ?? "");
        return safe.Length > limit ? safe[..limit] : safe;
    }
}
