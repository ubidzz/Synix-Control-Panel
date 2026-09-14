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
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class StartupMaintenanceTests
{
	[Theory]
	[InlineData(false, true, "backup", false)]
	[InlineData(true, false, "backup,update", false)]
	[InlineData(true, true, "backup,update", true)]
	public async Task RequiredMaintenanceRunsInOrderAndOnlySuccessAllowsLaunch(
		bool backupResult, bool updateResult, string expectedOrder, bool expectedReady)
	{
		GameServer server = new() { BackupOnStart = true, UpdateOnStart = true, Status = "Stopped" };
		List<string> calls = [], messages = [];
		bool ready = await Servers.RunStartupMaintenanceAsync(server, StartContext.Manual,
			() => { calls.Add("backup"); return Task.FromResult(backupResult); },
			() => { calls.Add("update"); return Task.FromResult(updateResult); },
			(message, _) => messages.Add(message));
		Assert.Equal(expectedReady, ready);
		Assert.Equal(expectedOrder, string.Join(',', calls));
		Assert.Equal("Stopped", server.Status);
		Assert.Null(server.PID);
		Assert.Equal(expectedReady ? 0 : 1, messages.Count);
		if (!expectedReady) Assert.Contains("cancelled", messages.Single());
	}

	[Theory]
	[InlineData(StartContext.Manual, false, "")]
	[InlineData(StartContext.Scheduled, true, "backup,update")]
	[InlineData(StartContext.Scheduled, false, "")]
	[InlineData(StartContext.CrashRecovery, true, "")]
	public async Task ScheduledOptionsOnlyRunForEnabledScheduledMaintenance(StartContext context, bool enabled, string expected)
	{
		GameServer server = new() { SmartMaintenanceEnabled = enabled,
			MaintenanceBackupBeforeRestart = true, MaintenanceUpdateBeforeRestart = true };
		List<string> calls = [];
		Assert.True(await Servers.RunStartupMaintenanceAsync(server, context,
			() => { calls.Add("backup"); return Task.FromResult(true); },
			() => { calls.Add("update"); return Task.FromResult(true); }, (_, _) => { }));
		Assert.Equal(expected, string.Join(',', calls));
	}

	[Fact]
	public async Task BackupExceptionCannotFallThroughToTheUpdate()
	{
		GameServer server = new() { BackupOnStart = true, UpdateOnStart = true };
		bool updated = false;
		await Assert.ThrowsAsync<IOException>(() => Servers.RunStartupMaintenanceAsync(server, StartContext.Manual,
			() => throw new IOException("Fixture failure"),
			() => { updated = true; return Task.FromResult(true); }, (_, _) => { }));
		Assert.False(updated);
	}

	[Theory]
	[InlineData("Starting", null)]
	[InlineData("Restoring", null)]
	[InlineData("Stopped", 12345)]
	public async Task UpdatingNeverTouchesAnActiveOrInFlightServer(string status, int? pid)
	{
		GameServer server = new() { Status = status, PID = pid };
		Assert.False(await Core.Instance.UpdateServerAndReport(server, "UPDATE", autoRestart: true));
		Assert.Equal(status, server.Status);
		Assert.Equal(pid, server.PID);
	}
}
