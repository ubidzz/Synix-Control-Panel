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
using Synix_Control_Panel.SynixEngine;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class ServerOperationCoordinatorTests
{
	[Fact]
	public async Task SameServer_BlocksIndependentOperation_AndAllowsNestedWork()
	{
		GameServer server = CreateServer("one");
		using ServerOperationLease start =
			ServerOperationCoordinator.TryBegin(server, ServerOperationKind.Start);
		Assert.True(start.Acquired, start.FailureReason);

		using ServerOperationLease nestedBackup =
			ServerOperationCoordinator.TryBegin(server, ServerOperationKind.Backup);
		Assert.True(nestedBackup.Acquired, nestedBackup.FailureReason);

		Task<ServerOperationLease> blockedTask;
		using (ExecutionContext.SuppressFlow())
		{
			blockedTask = Task.Run(() =>
				ServerOperationCoordinator.TryBegin(server, ServerOperationKind.Stop));
		}
		using ServerOperationLease blocked = await blockedTask;
		Assert.False(blocked.Acquired);
		Assert.Contains("already performing", blocked.FailureReason, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public async Task DifferentServers_CanRunIndependentNonSteamOperations()
	{
		GameServer first = CreateServer("first");
		GameServer second = CreateServer("second");
		using ServerOperationLease firstLease =
			ServerOperationCoordinator.TryBegin(first, ServerOperationKind.Backup);
		Assert.True(firstLease.Acquired, firstLease.FailureReason);

		Task<ServerOperationLease> secondTask;
		using (ExecutionContext.SuppressFlow())
		{
			secondTask = Task.Run(() =>
				ServerOperationCoordinator.TryBegin(second, ServerOperationKind.Start));
		}
		using ServerOperationLease secondLease = await secondTask;
		Assert.True(secondLease.Acquired, secondLease.FailureReason);
	}

	[Fact]
	public async Task SteamCmdOperations_AreLimitedToOneGlobalJob()
	{
		GameServer first = CreateServer("steam-one");
		GameServer second = CreateServer("steam-two");
		using ServerOperationLease update =
			ServerOperationCoordinator.TryBegin(first, ServerOperationKind.Update);
		Assert.True(update.Acquired, update.FailureReason);

		Task<ServerOperationLease> validateTask;
		using (ExecutionContext.SuppressFlow())
		{
			validateTask = Task.Run(() =>
				ServerOperationCoordinator.TryBegin(second, ServerOperationKind.Validate));
		}
		using ServerOperationLease validate = await validateTask;
		Assert.False(validate.Acquired);
		Assert.Contains("SteamCMD", validate.FailureReason, StringComparison.OrdinalIgnoreCase);
	}

	private static GameServer CreateServer(string name) => new()
	{
		Game = "Coordinator Test",
		ServerName = name,
		InstallPath = Path.Combine(
			Path.GetTempPath(),
			"SynixCoordinatorTests",
			name,
			Guid.NewGuid().ToString("N"))
	};
}
