using System.Diagnostics;
using System.Text.Json;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class NativeLaunchArgumentTests
{
    public static TheoryData<string> Values { get; } = new()
    {
        "", "two  words", "test\" -injected \"value", "-unwanted flag", "{pass}",
        "quoted\\\" then space", @"C:\folder with spaces\", @"C:\folder with spaces\\",
        "Unicode 朋友 Привет", "literal%NAME%! & | < > ^ ( )", "tab\tvalue"
    };

    [Theory]
    [MemberData(nameof(Values))]
    public async Task ConfiguredValuesReachTheNativeProcessAsSingleArguments(string value)
    {
        GameInfo definition = new()
        {
            Game = "Native argument fixture", ExeName = "bedrock_server.exe", AppID = "1",
            RequiredArgs = "--synix-argv -name \"{ServerName}\" -map={map} -pass \"{pass}\" -admin=\"{adminpass}\" -token \"{auth_token}\" -seed {seed} -install \"{InstallPath}\\suffix\" {rcon} -end",
            RconSyntax = "-rcon \"{rcon_pass}\""
        };
        GameServer server = new() { Game = definition.Game, ServerName = value, WorldName = value,
            WorldSeed = value, InstallPath = value, EnableRcon = true };
        Assert.True(GameLaunchCommandBuilder.TryBuildArguments(server, definition, "1", new(value, value, value, value),
            out string commandLine, out string error), error);
        string[] received = await CaptureAsync(commandLine);
        Assert.Equal(new[] { "-name", value, "-map=" + value, "-pass", value, "-admin=" + value, "-token", value,
            "-seed", string.IsNullOrWhiteSpace(value) ? "12345" : value, "-install", value + @"\suffix", "-rcon", value, "-end" }, received);
    }

    [Fact]
    public async Task AdjacentTemplateFieldsAndLiteralPlaceholdersKeepTheirArgumentBoundaries()
    {
        GameInfo definition = new() { Game = "Native argument fixture", ExeName = "fixture.exe",
            RequiredArgs = "--synix-argv {map}?Name=\"{ServerName}\"?Password=\"{pass}\" -end" };
        GameServer server = new() { Game = definition.Game, WorldName = "map\" -extra \"", ServerName = "Literal {pass}\\" };
        const string password = "Private\" -injected \"";
        Assert.True(GameLaunchCommandBuilder.TryBuildArguments(server, definition, "", new(password, "", ""),
            out string arguments, out string error), error);
        Assert.Equal([server.WorldName + "?Name=" + server.ServerName + "?Password=" + password, "-end"], await CaptureAsync(arguments));
    }

    [Theory]
    [InlineData("test\0secret")]
    [InlineData("test\r\nsecret")]
    public void InvalidNativeValuesAreRejectedWithoutExposingTheValue(string value)
    {
        GameInfo definition = new() { Game = "Native argument fixture", ExeName = "fixture.exe", RequiredArgs = "-password {pass}" };
        Assert.False(GameLaunchCommandBuilder.TryBuildArguments(new() { Game = definition.Game }, definition, "", new(value, "", ""),
            out string arguments, out string error));
        Assert.Empty(arguments);
        Assert.DoesNotContain("secret", error);
    }

    [Fact]
    public async Task ExplicitExtraArgumentsRemainSeparateFromConfiguredValues()
    {
        GameInfo definition = new() { Game = "Native argument fixture", ExeName = "fixture.exe", RequiredArgs = "--synix-argv -name \"{ServerName}\" -dedicated" };
        GameServer server = new() { Game = definition.Game, ServerName = "value\" -extra \"", ExtraArgs = "-custom \"two  words\"" };
        Assert.True(GameLaunchCommandBuilder.TryBuildArguments(server, definition, "", new("", "", ""), out string arguments, out string error), error);
        Assert.Equal(["-name", server.ServerName, "-custom", "two  words", "-dedicated"], await CaptureAsync(arguments));
    }

    private static async Task<string[]> CaptureAsync(string arguments)
    {
        string executable = Path.Combine(AppContext.BaseDirectory, "ConsoleFixture", "bedrock_server.exe");
        ProcessStartInfo info = GameLaunchCommandBuilder.CreateProcessStartInfo(executable, arguments,
            Path.GetDirectoryName(executable)!, false, true, false, true);
        Assert.Empty(info.Arguments);
        using Process process = Process.Start(info)!;
        try
        {
            string json = (await process.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(10)))!;
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10));
            Assert.Equal(0, process.ExitCode);
            return JsonSerializer.Deserialize<string[]>(json)!;
        }
        finally { if (!process.HasExited) { process.Kill(); process.WaitForExit(5000); } }
    }
}
