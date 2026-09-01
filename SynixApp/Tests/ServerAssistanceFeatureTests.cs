// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixEngine;
using System.Text;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class ServerAssistanceFeatureTests
{
	[Fact]
	public void ExistingServerImportDetectsTheExactDefinitionExecutable()
	{
		GameInfo definition = GameDatabase.GetGameList()
			.First(game => !string.IsNullOrWhiteSpace(game.ExeName));
		string folder = CreateTemporaryServerFolder(definition);
		try
		{
			IReadOnlyList<ExistingServerDetection> detections =
				ExistingServerImport.Detect(folder);

			Assert.Contains(detections, detection =>
				detection.Game.Game.Equals(definition.Game, StringComparison.OrdinalIgnoreCase) &&
				detection.ExecutablePath.Equals(
					Path.GetFullPath(Path.Combine(folder, definition.ExeName)),
					StringComparison.OrdinalIgnoreCase));
		}
		finally
		{
			Directory.Delete(folder, recursive: true);
		}
	}

	[Fact]
	public void ExistingServerImportPreservesConfigurationAndRejectsDuplicateFolders()
	{
		GameInfo definition = GameDatabase.GetGameList()
			.First(game => !string.IsNullOrWhiteSpace(game.ExeName));
		string folder = CreateTemporaryServerFolder(definition);
		try
		{
			GameServer imported = ExistingServerImport.Create(
				folder,
				definition,
				"Imported Test",
				20000,
				20001,
				[]);

			Assert.False(imported.IsFirstBoot);
			Assert.False(imported.IsDefaultPath);
			Assert.True(imported.PreserveImportedConfiguration);
			Assert.Equal(Path.GetFullPath(folder), imported.InstallPath);
			Assert.Throws<InvalidOperationException>(() => ExistingServerImport.Create(
				folder,
				definition,
				"Another Name",
				20010,
				20011,
				[imported]));
		}
		finally
		{
			Directory.Delete(folder, recursive: true);
		}
	}

	[Fact]
	public void SmartMaintenanceWaitsForPlayersThenRunsAtTheDeadline()
	{
		GameServer server = CreateScheduledServer();
		server.CurrentPlayers = 3;

		SmartMaintenancePlan waiting = SmartMaintenancePlanner.Evaluate(
			server,
			new DateTime(2026, 8, 31, 4, 10, 0));
		SmartMaintenancePlan deadline = SmartMaintenancePlanner.Evaluate(
			server,
			new DateTime(2026, 8, 31, 4, 30, 0));

		Assert.Equal(SmartMaintenanceDecision.DeferForPlayers, waiting.Decision);
		Assert.Equal(SmartMaintenanceDecision.RunNow, deadline.Decision);
	}

	[Fact]
	public void StandardMaintenanceDoesNotRunAfterItsScheduledMinute()
	{
		GameServer server = CreateScheduledServer();
		server.SmartMaintenanceEnabled = false;

		SmartMaintenancePlan plan = SmartMaintenancePlanner.Evaluate(
			server,
			new DateTime(2026, 8, 31, 4, 1, 0));

		Assert.Equal(SmartMaintenanceDecision.NotDue, plan.Decision);
	}

	[Fact]
	public void A2SPlayerResponseParsesNamesScoresAndConnectionTime()
	{
		using MemoryStream stream = new();
		using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
		writer.Write(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x44, 0x01, 0x00 });
		writer.Write(Encoding.UTF8.GetBytes("Player One"));
		writer.Write((byte)0);
		writer.Write(42);
		writer.Write(125.5F);

		GamePlayerInfo player = Assert.Single(
			PlayerQueryService.ParsePlayerResponse(stream.ToArray()));

		Assert.Equal("Player One", player.Name);
		Assert.Equal(42, player.Score);
		Assert.InRange(player.ConnectedFor.TotalSeconds, 125.49, 125.51);
	}

	[Fact]
	public void BackgroundServiceCommandQuotesThePublishedExecutablePath()
	{
		string command = BackgroundServiceManager.BuildLaunchCommand(
			@"C:\Program Files\Synix\Synix Control Panel.exe");

		Assert.Equal(
			"\"C:\\Program Files\\Synix\\Synix Control Panel.exe\" --synix-background-agent",
			command);
	}

	private static GameServer CreateScheduledServer() => new()
	{
		ServerName = "Scheduled Test",
		IsScheduledRestartEnabled = true,
		RestartDays = [true, true, true, true, true, true, true],
		RestartTime = "04:00",
		SmartMaintenanceEnabled = true,
		MaintenanceWaitForPlayers = true,
		MaintenanceMaximumDelayMinutes = 30
	};

	private static string CreateTemporaryServerFolder(GameInfo definition)
	{
		string folder = Path.Combine(
			Path.GetTempPath(),
			$"synix-import-test-{Guid.NewGuid():N}");
		string executable = Path.Combine(folder, definition.ExeName);
		Directory.CreateDirectory(Path.GetDirectoryName(executable)!);
		File.WriteAllBytes(executable, [0x4D, 0x5A]);
		return folder;
	}
}
