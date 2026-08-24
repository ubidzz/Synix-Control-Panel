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

public sealed class TroubleshooterAndRecoveryTests
{
	[Fact]
	public void NewHealthAndReliabilityWindows_ConstructOnStaThread()
	{
		Exception? failure = null;
		Thread thread = new(() =>
		{
			try
			{
				using TroubleshooterDialog troubleshooter = new();
				using FirstRunGuideDialog firstRun = new();
				using ReliabilityTestDialog reliability = new();
				using AdvancedSettingsPage advanced = new();
			}
			catch (Exception exception)
			{
				failure = exception;
			}
		});
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		thread.Join();
		Assert.Null(failure);
	}

	[Fact]
	public void HealthReport_CountsEveryResultLevel()
	{
		SynixHealthReport report = new(
			DateTimeOffset.UtcNow,
			[
				new(SynixHealthLevel.Passed, "Files", "One", "Good"),
				new(SynixHealthLevel.Warning, "Ports", "Two", "Review"),
				new(SynixHealthLevel.Failed, "Config", "Three", "Bad")
			]);

		Assert.Equal(1, report.PassedCount);
		Assert.Equal(1, report.WarningCount);
		Assert.Equal(1, report.FailedCount);
		Assert.False(report.IsHealthy);
	}

	[Fact]
	public async Task Troubleshooter_CompletesItsComputerChecksWithoutServers()
	{
		SynixHealthReport report = await SynixTroubleshooter.RunAsync(
			Array.Empty<GameServer>(),
			checkForUpdates: false);

		Assert.Contains(report.Items, item => item.Area == "SteamCMD and runtimes");
		Assert.Contains(report.Items, item => item.Area == "Available disk space");
		Assert.Contains(report.Items, item => item.Area == "Synix update");
	}

	[Theory]
	[InlineData("Fatal error! server packages file not found", true)]
	[InlineData("Could not bind: address already in use", true)]
	[InlineData("Server startup completed successfully", false)]
	public void LogAnalyzer_FindsCommonStartupFailures(string log, bool expected)
	{
		Assert.Equal(expected, SynixTroubleshooter.FindLikelyLogProblem(log) != null);
	}

	[Fact]
	public void SessionMarker_DistinguishesCleanAndInterruptedRuns()
	{
		string folder = Path.Combine(Path.GetTempPath(), $"SynixSessionTest-{Guid.NewGuid():N}");
		string marker = Path.Combine(folder, "session.marker");
		try
		{
			SynixSessionRecovery.BeginSession(marker);
			Assert.False(SynixSessionRecovery.PreviousSessionWasInterrupted);
			Assert.True(File.Exists(marker));
			SynixSessionRecovery.EndSession(marker);
			Assert.False(File.Exists(marker));

			Directory.CreateDirectory(folder);
			File.WriteAllText(marker, "unfinished");
			SynixSessionRecovery.BeginSession(marker);
			Assert.True(SynixSessionRecovery.PreviousSessionWasInterrupted);
		}
		finally
		{
			SynixSessionRecovery.EndSession(marker);
			if (Directory.Exists(folder))
				Directory.Delete(folder, recursive: true);
		}
	}

	[Fact]
	public void ReliabilityReport_CalculatesResourceGrowth()
	{
		DateTimeOffset started = DateTimeOffset.UtcNow;
		ReliabilityTestReport report = new(
			started,
			started.AddMinutes(1),
			[
				new(started, 50 * 1024 * 1024, 55 * 1024 * 1024, 100, 20, 0, 1),
				new(started.AddMinutes(1), 54 * 1024 * 1024, 58 * 1024 * 1024, 102, 21, 0, 1)
			]);

		Assert.Equal(4 * 1024 * 1024, report.PrivateMemoryGrowth);
		Assert.Equal(2, report.HandleGrowth);
		Assert.Equal(1, report.ThreadGrowth);
		Assert.Contains("SYNIX RELIABILITY TEST REPORT", report.ToPlainText());
	}
}
