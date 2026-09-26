// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
//
// LEGAL NOTICE:
// This source code is proprietary and confidential.
// 1. Permission is granted for PERSONAL, NON-COMMERCIAL use only.
// 2. You may modify this code for your own use, but you may NOT redistribute,
//    rebrand, or sell this code or derivative works without written consent.
// 3. The "Synix" brand and logic remain the property of Jason Turner.
// ============================================================================
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine;
using Synix_Control_Panel.SynixEngine.ModManagement;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class ServerProcessDiscoveryTests
{
	[Theory]
	[InlineData("Windrose", "WindroseServer.exe")]
	[InlineData("Minecraft", "cmd.exe")]
	public async Task Stop_DoesNotTerminateAnUnrelatedProcessWithAStaleSavedPid(string game, string executableName)
	{
		string fixture = Path.Combine(Path.GetTempPath(), "SynixStalePidTests", Guid.NewGuid().ToString("N"));
		string unrelatedDirectory = Path.Combine(fixture, "unrelated");
		Directory.CreateDirectory(unrelatedDirectory);
		string executable = Path.Combine(unrelatedDirectory, executableName);
		File.Copy(Path.Combine(Environment.SystemDirectory, "cmd.exe"), executable);
		using Process unrelated = Process.Start(new ProcessStartInfo
		{
			FileName = executable,
			Arguments = "/d /c ping 127.0.0.1 -n 120 > nul",
			UseShellExecute = false,
			CreateNoWindow = true,
			WindowStyle = ProcessWindowStyle.Hidden
		}) ?? throw new InvalidOperationException("Could not start the isolated process fixture.");
		try
		{
			await Task.Delay(200);
			GameServer staleEntry = new()
			{
				Game = game,
				ServerName = "Stale PID regression test",
				InstallPath = Path.Combine(fixture, "different-server"),
				PID = unrelated.Id
			};
			Assert.True(await Servers.Stop(staleEntry, (_, _) => { }));
			Assert.False(unrelated.HasExited, "A matching executable name does not prove server ownership.");
			Assert.Empty(staleEntry.ServerProcesses);
			Assert.Null(staleEntry.PID);
		}
		finally
		{
			try
			{
				if (!unrelated.HasExited)
					unrelated.Kill(entireProcessTree: true);
				await unrelated.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10));
			}
			finally
			{
				// Release our process handle before trying to remove its copied executable.
				unrelated.Dispose();
			}
			await DeleteFixtureAsync(fixture);
		}
	}

	[Fact]
	public async Task FixtureCleanup_RetriesUntilATemporaryFileLockIsReleased()
	{
		string fixture = Path.Combine(Path.GetTempPath(), "SynixStalePidTests", Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(fixture);
		try
		{
			using FileStream locked = new(Path.Combine(fixture, "locked.exe"), FileMode.CreateNew,
				FileAccess.ReadWrite, FileShare.Read);
			Task cleanup = DeleteFixtureAsync(fixture);
			Assert.False(cleanup.IsCompleted, "Cleanup must wait for the lock rather than fail or ignore the file.");
			locked.Dispose();
			await cleanup;
			Assert.False(Directory.Exists(fixture));
		}
		finally { await DeleteFixtureAsync(fixture); }
	}

	[Fact]
	public async Task FixtureCleanup_ReportsALockThatOutlastsTheRetryWindow()
	{
		string fixture = Path.Combine(Path.GetTempPath(), "SynixStalePidTests", Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(fixture);
		try
		{
			using FileStream locked = new(Path.Combine(fixture, "locked.exe"), FileMode.CreateNew,
				FileAccess.ReadWrite, FileShare.Read);
			Exception? failure = await Record.ExceptionAsync(() => DeleteFixtureAsync(fixture, TimeSpan.FromMilliseconds(150)));
			Assert.True(failure is IOException or UnauthorizedAccessException,
				"A persistent lock must fail cleanup, not turn a failed deletion into a passing test.");
			Assert.True(Directory.Exists(fixture));
		}
		finally { await DeleteFixtureAsync(fixture); }
	}

	[Fact]
	public async Task FixtureCleanup_RejectsPathsOutsideAnIndividualFixture()
	{
		string fixtureRoot = Path.Combine(Path.GetTempPath(), "SynixStalePidTests");
		foreach (string path in new[] { Path.GetTempPath(), fixtureRoot, Path.Combine(fixtureRoot, "not-a-fixture") })
			await Assert.ThrowsAsync<InvalidOperationException>(() => DeleteFixtureAsync(path));
	}

	private static async Task DeleteFixtureAsync(string fixture, TimeSpan? retryWindow = null)
	{
		string fullPath = Path.GetFullPath(fixture);
		string fixtureRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "SynixStalePidTests"));
		if (!string.Equals(Path.GetDirectoryName(fullPath), fixtureRoot, StringComparison.OrdinalIgnoreCase) ||
			!Guid.TryParseExact(Path.GetFileName(fullPath), "N", out _))
			throw new InvalidOperationException("Cleanup is restricted to an individual stale-PID test fixture.");

		Stopwatch elapsed = Stopwatch.StartNew();
		TimeSpan limit = retryWindow ?? TimeSpan.FromSeconds(10);
		while (Directory.Exists(fullPath))
		{
			ModPathSafety.EnsureTreeHasNoLinks(fullPath);
			try
			{
				Directory.Delete(fullPath, recursive: true);
				return;
			}
			catch (Exception exception) when ((exception is IOException or UnauthorizedAccessException) && elapsed.Elapsed < limit)
			{
				// Windows can briefly retain an executable/file lock after the process exits.
				// Retry only this disposable fixture; persistent failures still fail the test.
				await Task.Delay(100);
			}
		}
	}

	[Fact]
	public void ImagePathQuery_ResolvesTheCurrentExecutableWithLimitedAccess()
	{
		Assert.Equal(Environment.ProcessPath, Servers.TryGetProcessImagePath(Environment.ProcessId),
			ignoreCase: true);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	[InlineData(int.MaxValue)]
	public void ImagePathQuery_UnavailableProcessIsANormalMiss(int processId)
	{
		Assert.Null(Servers.TryGetProcessImagePath(processId));
	}

	[Fact]
	public void ImagePathQuery_DoesNotLeakNativeProcessHandles()
	{
		using Process current = Process.GetCurrentProcess();
		for (int i = 0; i < 16; i++)
			Assert.NotNull(Servers.TryGetProcessImagePath(current.Id));
		current.Refresh();
		int handlesBefore = current.HandleCount;
		for (int i = 0; i < 256; i++)
			Assert.NotNull(Servers.TryGetProcessImagePath(current.Id));
		current.Refresh();
		Assert.True(current.HandleCount <= handlesBefore + 8,
			"Repeated process-image queries must dispose their native handles.");
	}

	[Fact]
	public void InstallDirectoryDiscovery_DoesNotThrowForUnrelatedProtectedProcesses()
	{
		GameServer server = new()
		{
			Game = "Windrose",
			ServerName = "Read-only discovery regression test",
			InstallPath = Path.Combine(Path.GetTempPath(), "SynixDiscoveryTests", Guid.NewGuid().ToString("N"))
		};
		int accessFailures = 0;
		int dashboardMessages = 0;
		int threadId = Environment.CurrentManagedThreadId;
		EventHandler<FirstChanceExceptionEventArgs> exceptionHandler = (_, args) =>
		{
			if (Environment.CurrentManagedThreadId == threadId &&
				args.Exception is Win32Exception { NativeErrorCode: 5 })
				accessFailures++;
		};
		EventHandler<ApplicationLogEventArgs> logHandler = (_, _) => dashboardMessages++;
		AppDomain.CurrentDomain.FirstChanceException += exceptionHandler;
		ApplicationUiService.LogRequested += logHandler;
		try
		{
			// This only discovers processes for a unique, nonexistent installation;
			// it cannot bind to or operate on a user's server.
			Assert.Empty(Servers.RefreshServerProcessRegistry(server, forceDiscovery: true));
			Assert.Equal(0, accessFailures);
			Assert.Equal(0, dashboardMessages);
		}
		finally
		{
			AppDomain.CurrentDomain.FirstChanceException -= exceptionHandler;
			ApplicationUiService.LogRequested -= logHandler;
		}
	}
}
