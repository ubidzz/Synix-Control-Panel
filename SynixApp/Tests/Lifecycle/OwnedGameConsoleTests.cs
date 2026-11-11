using System.Diagnostics;
using System.Text.Json;
using System.Windows.Forms;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine;
using Xunit;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel.Tests;

public sealed class OwnedGameConsoleTests
{
    [Theory]
    [InlineData("@echo off\r\njava %*\r\nexit /b %errorlevel%\r\n", true)]
    [InlineData("@echo off\n\"C:\\Java\\bin\\java.exe\" %* @\"libraries\\net\\minecraftforge\\forge\\1.21-52.0\\win_args.txt\" nogui\nexit /b %errorlevel%", true)]
    [InlineData("@echo off\njava %* & cmd.exe\nexit /b %errorlevel%", false)]
    [InlineData("@echo off\ncmd.exe %*\nexit /b %errorlevel%", false)]
    [InlineData("@echo off\njava %*\npause\nexit /b %errorlevel%", false)]
    [InlineData("@echo off\n%COMSPEC% %*\nexit /b %errorlevel%", false)]
    [InlineData("@echo off\njava %* <NUL\nexit /b %errorlevel%", false)]
    public void OnlyGeneratedDirectJavaLaunchersQualify(string script, bool expected) =>
        Assert.Equal(expected, OwnedGameConsole.TryReadJavaLauncher(script, out _, out _));

    [Theory]
    [InlineData("say Café 世界 & echo literal", true)]
    [InlineData("say hello\nwhoami", false)]
    [InlineData("say hello\rwhoami", false)]
    [InlineData("say \u0085hello", false)]
    [InlineData("/stop", false)]
    [InlineData("restart now", false)]
    [InlineData(" ", false)]
    public void InputIsOneBoundedGameCommand(string command, bool expected) =>
        Assert.Equal(expected, OwnedGameConsole.ValidCommand(command));

    [Fact]
    public void PrivatePipeIsRunBoundAndCannotFollowAReplacementProcessOrConsole()
    {
        WorkflowUiTest.Run(() =>
        {
            bool previous = Synix_Control_Panel.Properties.Settings.Default.ShowServerWindow;
            Synix_Control_Panel.Properties.Settings.Default.ShowServerWindow = true;
            string executable = Path.Combine(AppContext.BaseDirectory, "ConsoleFixture", "bedrock_server.exe");
            var server = new GameServer { Game = "Minecraft Bedrock", MinecraftEdition = "Bedrock", ServerName = "Console fixture",
                InstallPath = Path.GetDirectoryName(executable)!, IsFirstBoot = false, Status = StatusManager.GetStatus(ServerState.Running) };
            using var first = Process.Start(OwnedGameConsole.Prepare(server, executable, "", server.InstallPath, false)!)!;
            using var replacement = Process.Start(OwnedGameConsole.Prepare(server, executable, "", server.InstallPath, false)!)!;
            try
            {
                Assert.False(OwnedGameConsole.Describe(server).Available);
                server.RunningProcess = first; server.PID = first.Id;
                OwnedGameConsole.Attach(server, first); OwnedGameConsole.ShowWindow(server);
                var channel = OwnedGameConsole.Describe(server); Assert.True(channel.Available);
                string original = channel.RunId!;
                var summary = JsonSerializer.Serialize(RemoteManagerProtocol.Summarize(server), RemoteManagerProtocol.Json);
                Assert.Contains(original, summary); Assert.DoesNotContain("processId", summary); Assert.DoesNotContain(executable, summary);
                Task<bool> Send(string? run, string text, bool paired = true, long? expires = null) =>
                    OwnedGameConsole.SendAsync(server, run, text, expires ?? DateTimeOffset.UtcNow.AddSeconds(30).ToUnixTimeSeconds(), () => paired, default);
                void Check(Task<bool> task, bool expected) { WorkflowUiTest.Pump(task); Assert.Equal(expected, task.Result); }
                Check(Send("different-run", "must-not-arrive"), false);
                Check(Send(original, "must-not-arrive", false), false);
                Check(Send(original, "must-not-arrive", expires: 1), false);
                Check(Send(original, "say hello & echo literal"), true);
                var output = first.StandardOutput.ReadLineAsync(); WorkflowUiTest.Pump(output);
                Assert.Equal("received:say hello & echo literal", output.Result);
                server.Status = StatusManager.GetStatus(ServerState.Stopping);
                Check(Send(original, "must-not-arrive"), false);
                OwnedGameConsole.Invalidate(server);
                server.Status = StatusManager.GetStatus(ServerState.Running);
                server.RunningProcess = replacement; server.PID = replacement.Id;
                OwnedGameConsole.Attach(server, replacement); OwnedGameConsole.ShowWindow(server);
                string next = OwnedGameConsole.Describe(server).RunId!; Assert.NotEqual(original, next);
                // A late Exited callback from an old run cannot affect the new one.
                OwnedGameConsole.Invalidate(server, first);
                Assert.Equal(next, OwnedGameConsole.Describe(server).RunId);
                Check(Send(original, "must-not-arrive"), false);
                Check(Send(next, "list"), true);
                var nextOutput = replacement.StandardOutput.ReadLineAsync(); WorkflowUiTest.Pump(nextOutput);
                Assert.Equal("received:list", nextOutput.Result);
                Synix_Control_Panel.Properties.Settings.Default.ShowServerWindow = false;
                Check(Send(next, "must-not-arrive"), false); Assert.False(OwnedGameConsole.Describe(server).Available);
                Synix_Control_Panel.Properties.Settings.Default.ShowServerWindow = true;
                Assert.False(OwnedGameConsole.Describe(server).Available);
                // Restarting Synix cannot reconstruct a pipe from a saved PID.
                server.RunningProcess = first; server.PID = first.Id;
                Assert.False(OwnedGameConsole.Describe(server).Available);
            }
            finally
            {
                OwnedGameConsole.Invalidate(server);
                foreach (Form form in Application.OpenForms.Cast<Form>().ToArray()) form.Close();
                if (!first.HasExited) first.Kill(); if (!replacement.HasExited) replacement.Kill();
                Synix_Control_Panel.Properties.Settings.Default.ShowServerWindow = previous;
            }
        });
    }

    [Fact]
    public void ShellAndUnsupportedGamesNeverBecomeCommandTargets()
    {
        var server = new GameServer { Game = "Enshrouded", Status = StatusManager.GetStatus(ServerState.Running) };
        Assert.False(OwnedGameConsole.Supported(server));
        Assert.Null(OwnedGameConsole.Prepare(server, Path.Combine(Environment.SystemDirectory, "cmd.exe"), "", Environment.SystemDirectory, false));
        server.Game = "Minecraft Bedrock"; server.MinecraftEdition = "Bedrock";
        Assert.Null(OwnedGameConsole.Prepare(server, Path.Combine(Environment.SystemDirectory, "cmd.exe"), "", Environment.SystemDirectory, false));
        using var process = Process.GetCurrentProcess();
        server.RunningProcess = process; server.PID = process.Id;
        OwnedGameConsole.Attach(server, process);
        Assert.False(OwnedGameConsole.Describe(server).Available);
    }
}
