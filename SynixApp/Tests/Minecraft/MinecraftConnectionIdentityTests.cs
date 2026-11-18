using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class MinecraftConnectionIdentityTests
{
    private const string RconPassword = "fixture-only-password";
    private const string ManagementSecret = "0123456789012345678901234567890123456789";

    [Theory]
    [InlineData("missing")]
    [InlineData("start-time")]
    [InlineData("missing-start-time")]
    [InlineData("path")]
    [InlineData("different-process")]
    [InlineData("exited")]
    public async Task RconDoesNotSendCredentialsToAnUnverifiedListener(string mismatch)
    {
        using Fixture fixture = await Fixture.StartAsync("127.0.0.1", "rcon");
        GameServer server = fixture.Server;
        TcpListener? replacement = null;
        if (mismatch == "missing") server.ServerProcesses.Clear();
        if (mismatch == "start-time") server.ServerProcesses[0].StartTimeUtc = server.ServerProcesses[0].StartTimeUtc!.Value.AddTicks(-1);
        if (mismatch == "missing-start-time") server.ServerProcesses[0].StartTimeUtc = null;
        if (mismatch == "path") server.ServerProcesses[0].ExecutablePath = Path.Combine(fixture.Root, "wrong.exe");
        if (mismatch is "different-process" or "exited")
        {
            if (mismatch == "exited") { fixture.Process.Kill(); await fixture.Process.WaitForExitAsync(); }
            replacement = new(IPAddress.Loopback, 0);
            replacement.Start();
            server.RconPort = ((IPEndPoint)replacement.LocalEndpoint).Port;
        }
        try
        {
            MinecraftRconResult result = await MinecraftRconClient.ExecuteCommandAsync(server, "list");
            Assert.False(result.Succeeded);
            Assert.DoesNotContain(RconPassword, result.Problem);
            if (replacement != null) Assert.False(replacement.Pending());
            else Assert.False(fixture.Process.HasExited); // It is still waiting for its first connection.
        }
        finally { replacement?.Stop(); }
    }

    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("::1")]
    public async Task ManagementDoesNotSendAuthorizationToAnUntrackedListener(string address)
    {
        using Fixture fixture = await Fixture.StartAsync(address, "management");
        fixture.Server.ServerProcesses.Clear();
        var result = await MinecraftManagementClient.QueryPlayersAsync(fixture.Server);
        Assert.False(result.Succeeded);
        Assert.DoesNotContain(ManagementSecret, result.Problem);
        Assert.False(fixture.Process.HasExited);
    }

    [Fact]
    public async Task VerifiedRconAuthenticatesAndSendsTheRequestedCommand()
    {
        using Fixture fixture = await Fixture.StartAsync("127.0.0.1", "rcon");
        MinecraftRconResult result = await MinecraftRconClient.ExecuteCommandAsync(fixture.Server, "list");
        Assert.True(result.Succeeded, result.Problem);
        Assert.Equal(["TestPlayer"], MinecraftRconClient.ParsePlayerNames(result.Response));
        Assert.Equal(RconPassword, await fixture.ReadLineAsync());
        Assert.Equal("list", await fixture.ReadLineAsync());
    }

    [Theory]
    [InlineData("127.0.0.1", "127.0.0.1")]
    [InlineData("::1", "::1")]
    [InlineData("::1", "[::1]")]
    [InlineData("127.0.0.1", "localhost")]
    [InlineData("::1", "localhost")]
    public async Task VerifiedManagementAuthenticatesOverTheCheckedConnection(string bind, string host)
    {
        using Fixture fixture = await Fixture.StartAsync(bind, "management", host: host);
        var result = await MinecraftManagementClient.QueryPlayersAsync(fixture.Server);
        Assert.True(result.Succeeded, result.Problem);
        Assert.Equal("TestPlayer", Assert.Single(result.Value!).Name);
        Assert.Equal("Bearer " + ManagementSecret, await fixture.ReadLineAsync());
    }

    [Fact]
    public async Task ManagementDoesNotFollowRedirectsWithTheAuthorizationHeader()
    {
        TcpListener other = new(IPAddress.Loopback, 0);
        other.Start();
        try
        {
            using Fixture fixture = await Fixture.StartAsync("127.0.0.1", "redirect",
                redirect: $"http://127.0.0.1:{((IPEndPoint)other.LocalEndpoint).Port}/");
            var result = await MinecraftManagementClient.QueryPlayersAsync(fixture.Server);
            Assert.False(result.Succeeded);
            Assert.False(other.Pending());
        }
        finally { other.Stop(); }
    }

    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("::1")]
    public async Task ConnectedPeerCheckRejectsAReplacementEvenWithTheExpectedListenerStillRunning(string address)
    {
        using Fixture expected = await Fixture.StartAsync(address, "management");
        IPAddress ip = IPAddress.Parse(address);
        TcpListener other = new(ip, 0);
        other.Start();
        try
        {
            using Socket socket = new(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(other.LocalEndpoint);
            using TcpClient accepted = await other.AcceptTcpClientAsync();
            Assert.Throws<IOException>(() => LocalServerConnectionIdentity.VerifyConnectedPeer(
                socket, expected.Server.ServerProcesses.ToArray()));
            Assert.Equal(0, accepted.Available);
        }
        finally { other.Stop(); }
    }

    private sealed class Fixture : IDisposable
    {
        internal string Root { get; } = Path.Combine(Path.GetTempPath(), "SynixConnectionIdentity-" + Guid.NewGuid().ToString("N"));
        internal Process Process { get; private set; } = null!;
        internal GameServer Server { get; private set; } = null!;

        internal static async Task<Fixture> StartAsync(string address, string protocol, string? host = null, string? redirect = null)
        {
            Fixture fixture = new();
            try
            {
                Directory.CreateDirectory(fixture.Root);
                string executable = Path.Combine(AppContext.BaseDirectory, "ConsoleFixture", "bedrock_server.exe");
                ProcessStartInfo info = new(executable) { UseShellExecute = false, CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardOutput = true, RedirectStandardError = true };
                foreach (string argument in new[] { "--synix-tcp", address, protocol }) info.ArgumentList.Add(argument);
                if (redirect != null) info.ArgumentList.Add(redirect);
                fixture.Process = Process.Start(info)!;
                int port = int.Parse(await fixture.ReadLineAsync());
                fixture.Server = new GameServer { Game = "Minecraft", MinecraftEdition = "Java", GameVersion = "1.21.9",
                    InstallPath = fixture.Root, EnableRcon = true, RconPort = port, RconPassword = RconPassword,
                    EnableMinecraftManagementProtocol = true, MinecraftManagementPort = port,
                    ServerProcesses = [new() { ProcessId = fixture.Process.Id, ExecutablePath = executable,
                        StartTimeUtc = fixture.Process.StartTime.ToUniversalTime() }] };
                File.WriteAllText(Path.Combine(fixture.Root, "server.properties"),
                    $"management-server-enabled=true\nmanagement-server-host={host ?? address}\nmanagement-server-port={port}\nmanagement-server-secret={ManagementSecret}\nmanagement-server-tls-enabled=false\n");
                return fixture;
            }
            catch { fixture.Dispose(); throw; }
        }

        internal async Task<string> ReadLineAsync() =>
            (await Process.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(10)))!;

        public void Dispose()
        {
            if (Process != null)
            {
                if (!Process.HasExited) { Process.Kill(); Process.WaitForExit(5000); }
                Process.Dispose();
            }
            if (Directory.Exists(Root)) Directory.Delete(Root, true);
        }
    }
}
