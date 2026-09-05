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
using Synix_Control_Panel.SynixApp.ServerHandler;
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
				using TroubleshooterDialog readiness = new(new GameServer
				{
					Game = "Palworld",
					ServerName = "Test Server"
				});
				Assert.Equal("Server Readiness Center", readiness.Text);
				Assert.Equal(
					"Server Readiness Center",
					readiness.Controls.Find("titleLabel", true).Single().Text);
				using FirstRunGuideDialog firstRun = new();
				using ReliabilityTestDialog reliability = new();
				using ServerBackupRestoreDialog restoreBackup = new();
				using GameVerificationQueue verificationQueue = new();
				using ArgumentVerificationDialog argumentVerification = new();
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
	public void StandardDialogs_ReceiveSharedSynixWindowHeader()
	{
		Exception? failure = null;
		Thread thread = new(() =>
		{
			try
			{
				using ServerBackupRestoreDialog dialog = new();
				Assert.Equal(
					System.Windows.Forms.FormBorderStyle.None,
					dialog.FormBorderStyle);
				Assert.Single(dialog.Controls.Find("synixWindowHeader", true));
				Assert.Single(dialog.Controls.Find("synixWindowLogo", true));
				Assert.Single(dialog.Controls.Find("synixWindowTitle", true));
				Assert.Single(dialog.Controls.Find("synixWindowCloseButton", true));
				System.Windows.Forms.Control header =
					dialog.Controls.Find("synixWindowHeader", true).Single();
				System.Windows.Forms.Control content =
					dialog.Controls.Find("synixWindowContent", true).Single();
				Assert.Equal(56, header.Height);
				Assert.Equal(header.Bottom, content.Top);
				Assert.Equal(dialog.ClientSize.Height - header.Height, content.Height);
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
	public void ArgumentVerificationConfirmation_IsReadableBeforeLaunchTest()
	{
		Exception? failure = null;
		Thread thread = new(() =>
		{
			try
			{
				using ArgumentVerificationDialog dialog = new();
				System.Windows.Forms.CheckBox confirmation =
					Assert.IsType<System.Windows.Forms.CheckBox>(
						dialog.Controls.Find("_confirmationCheck", true).Single());
				Assert.True(confirmation.Enabled);
				Assert.False(confirmation.AutoCheck);
				Assert.False(confirmation.TabStop);
				Assert.NotEqual(System.Drawing.Color.Black, confirmation.ForeColor);
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

	[Fact]
	public async Task ServerReadinessChecks_DoNotAddTheGlobalUpdateResult()
	{
		SynixHealthReport report = await SynixTroubleshooter.RunAsync(
			Array.Empty<GameServer>(),
			checkForUpdates: false,
			includeUpdateStatus: false);

		Assert.DoesNotContain(report.Items, item => item.Area == "Synix update");
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
	public void FirewallCheck_MatchesConfiguredExecutableByExactPath()
	{
		string installPath = Path.Combine("C:\\Synix\\Games", "Example", "ServerOne");
		string executable = Path.Combine(installPath, "Server.exe");

		string? match = WindowsFirewallInspector.FindAllowedExecutable(
			installPath,
			executable,
			[executable]);

		Assert.Equal(Path.GetFullPath(executable), match, ignoreCase: true);
	}

	[Fact]
	public void FirewallCheck_MatchesChildServerExecutableInsideInstallFolder()
	{
		string installPath = Path.Combine("C:\\Synix\\Games", "Palworld", "ServerOne");
		string launcher = Path.Combine(installPath, "PalServer.exe");
		string childExecutable = Path.Combine(
			installPath,
			"Pal",
			"Binaries",
			"Win64",
			"PalServer-Win64-Test-Cmd.exe");

		string? match = WindowsFirewallInspector.FindAllowedExecutable(
			installPath,
			launcher,
			[childExecutable]);

		Assert.Equal(Path.GetFullPath(childExecutable), match, ignoreCase: true);
	}

	[Fact]
	public void FirewallCheck_RejectsPortRulesAndExecutablesOutsideInstallFolder()
	{
		string installPath = Path.Combine("C:\\Synix\\Games", "Example", "ServerOne");
		string executable = Path.Combine(installPath, "Server.exe");
		string unrelatedExecutable = Path.Combine(
			"C:\\Synix\\Games",
			"Example",
			"AnotherServer",
			"Server.exe");

		string? match = WindowsFirewallInspector.FindAllowedExecutable(
			installPath,
			executable,
			[unrelatedExecutable, string.Empty]);

		Assert.Null(match);
	}

	[Fact]
	public void FirewallCleanup_FindsOnlyMissingDefaultServerFolders()
	{
		string gamesRoot = Path.Combine("C:\\Synix", "Games");
		string deletedValheimExecutable = Path.Combine(
			gamesRoot,
			"Valheim",
			"Deleted Server",
			"valheim_server.exe");
		string deletedPalworldChildExecutable = Path.Combine(
			gamesRoot,
			"Palworld",
			"Gone Server",
			"Pal",
			"Binaries",
			"Win64",
			"PalServer-Win64-Test-Cmd.exe");
		string registeredRustRoot = Path.Combine(
			gamesRoot,
			"Rust",
			"Active Server");
		string onDiskMinecraftRoot = Path.Combine(
			gamesRoot,
			"Minecraft",
			"On Disk");
		HashSet<string> existingDirectories = new(StringComparer.OrdinalIgnoreCase)
		{
			Path.Combine(gamesRoot, "Valheim"),
			Path.Combine(gamesRoot, "Palworld"),
			onDiskMinecraftRoot
		};

		IReadOnlyList<string> orphaned =
			FirewallCleanupService.FindOrphanedDefaultServerExecutables(
				[
					deletedValheimExecutable,
					deletedPalworldChildExecutable,
					Path.Combine(registeredRustRoot, "RustDedicated.exe"),
					Path.Combine(onDiskMinecraftRoot, "java.exe"),
					Path.Combine("D:\\Servers", "Custom", "Server.exe"),
					Path.Combine(gamesRoot, "Utility.exe")
				],
				gamesRoot,
				[registeredRustRoot],
				path => existingDirectories.Contains(path));

		Assert.Equal(2, orphaned.Count);
		Assert.Contains(deletedValheimExecutable, orphaned, StringComparer.OrdinalIgnoreCase);
		Assert.Contains(deletedPalworldChildExecutable, orphaned, StringComparer.OrdinalIgnoreCase);
		Assert.DoesNotContain(
			Path.Combine(onDiskMinecraftRoot, "java.exe"),
			orphaned,
			StringComparer.OrdinalIgnoreCase);
	}

	[Fact]
	public void FirewallCleanupCommand_RequiresTheExactDedicatedArgument()
	{
		Assert.True(FirewallCleanupService.IsCleanupCommand(
			[FirewallCleanupService.CleanupArgument]));
		Assert.False(FirewallCleanupService.IsCleanupCommand([]));
		Assert.False(FirewallCleanupService.IsCleanupCommand(
			[FirewallCleanupService.CleanupArgument, "extra"]));
		Assert.False(FirewallCleanupService.IsCleanupCommand(["--other-command"]));
	}

	[Fact]
	public void AdvancedSettings_FirewallCleanupIsIndependentAndUsesItsOwnCard()
	{
		Exception? failure = null;
		Thread thread = new(() =>
		{
			try
			{
				using AdvancedSettingsPage page = new();
				System.Windows.Forms.Control cleanupButton =
					page.Controls.Find("btnFirewallCleanup", true).Single();
				System.Windows.Forms.Control cleanupStatus =
					page.Controls.Find("lblFirewallCleanupStatus", true).Single();
				System.Windows.Forms.Control settingsCard =
					page.Controls.Find("settingsCard", true).Single();

				System.Windows.Forms.Control cleanupCard =
					page.Controls.Find("firewallCleanupCard", true).Single();
				System.Windows.Forms.Control cleanupDescription =
					page.Controls.Find("lblFirewallCleanupDescription", true).Single();
				System.Windows.Forms.Control backgroundServiceCard =
					page.Controls.Find("backgroundServiceCard", true).Single();
				System.Windows.Forms.Control troubleshooterCard =
					page.Controls.Find("troubleshooterCard", true).Single();

				Assert.True(cleanupButton.Enabled);
				page.ElevatedSystemTasks = false;
				Assert.True(cleanupButton.Enabled);
				page.ElevatedSystemTasks = true;
				Assert.True(cleanupButton.Enabled);
				Assert.NotSame(settingsCard, cleanupCard);
				Assert.Same(cleanupCard, cleanupButton.Parent);
				Assert.Same(cleanupCard, cleanupStatus.Parent);
				Assert.Contains("C:\\Synix\\Games", cleanupDescription.Text);
				Assert.Contains("not scanned", cleanupDescription.Text);
				Assert.True(cleanupButton.Bottom <= cleanupCard.ClientSize.Height);
				Assert.True(cleanupStatus.Bottom <= cleanupCard.ClientSize.Height);
				Assert.True(settingsCard.Bottom <= cleanupCard.Top);
				Assert.True(cleanupCard.Bottom <= backgroundServiceCard.Top);
				Assert.True(backgroundServiceCard.Bottom <= troubleshooterCard.Top);
				Assert.True(troubleshooterCard.Bottom <= page.ClientSize.Height);
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
	public void FirewallCleanupConfirmation_UsesSynixChromeAndListsEveryRule()
	{
		Exception? failure = null;
		Thread thread = new(() =>
		{
			try
			{
				string firstPath = Path.Combine(
					Core.GamesPath,
					"Valheim",
					"Old Server",
					"valheim_server.exe");
				string secondPath = Path.Combine(
					Core.GamesPath,
					"Palworld",
					"Deleted Server",
					"PalServer.exe");
				using FirewallCleanupConfirmationDialog dialog = new(
					[firstPath, secondPath]);
				System.Windows.Forms.Control ruleList =
					dialog.Controls.Find("firewallRuleList", true).Single();
				System.Windows.Forms.Button removeButton =
					Assert.IsAssignableFrom<System.Windows.Forms.Button>(
						dialog.Controls.Find("confirmFirewallCleanupButton", true).Single());
				System.Windows.Forms.Control safetyText =
					dialog.Controls.Find("firewallCleanupSafetyText", true).Single();
				System.Windows.Forms.Control reasonText =
					dialog.Controls.Find("firewallInspectionReasonText", true).Single();
				System.Windows.Forms.Control actionText =
					dialog.Controls.Find("firewallCleanupActionText", true).Single();

				Assert.Equal(
					System.Windows.Forms.FormBorderStyle.None,
					dialog.FormBorderStyle);
				Assert.Single(dialog.Controls.Find("synixWindowHeader", true));
				Assert.Contains("Valheim\\Old Server\\valheim_server.exe", ruleList.Text);
				Assert.Contains("Palworld\\Deleted Server\\PalServer.exe", ruleList.Text);
				Assert.Equal("Remove Rules", removeButton.Text);
				Assert.Equal(
					System.Windows.Forms.DialogResult.OK,
					removeButton.DialogResult);
				Assert.Contains("server folder is gone", reasonText.Text);
				Assert.Contains("administrator permission", actionText.Text);
				Assert.Contains("game files", safetyText.Text);
				Assert.Contains("custom install folders", safetyText.Text);
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
