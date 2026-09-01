// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.Database;
using Synix_Control_Panel.SynixApp.MonitoringHandler;
using Synix_Control_Panel.SynixEngine;
using System.Windows.Forms;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class UserGuidanceTests
{
	[Theory]
	[InlineData("The game port is already being used", "network port")]
	[InlineData("Required executable is missing", "required server file")]
	[InlineData("Server process is still running after shutdown", "parts of the game server")]
	[InlineData("Access to the path was denied", "Windows would not allow")]
	[InlineData("Game not found in database", "game definition")]
	[InlineData("This game requires AVX2", "known requirement")]
	public void PlainEnglishErrors_ExplainCommonProblems(
		string technicalDetails,
		string expectedText)
	{
		PlainEnglishError error = UserGuidance.TranslateError(
			"start the server",
			technicalDetails);

		Assert.Contains(expectedText, error.Explanation, StringComparison.OrdinalIgnoreCase);
		Assert.Equal(technicalDetails, error.TechnicalDetails);
	}

	[Fact]
	public void SetupCompletion_ReachesOneHundredOnlyWhenReady()
	{
		Assert.Equal(90, UserGuidance.CalculateSetupCompletion(new SetupCompletionState(
			true, true, true, true, true, false)));
		Assert.Equal(100, UserGuidance.CalculateSetupCompletion(new SetupCompletionState(
			true, true, true, true, true, true)));
	}

	[Fact]
	public void LiveProcessDetails_KeepEveryRegisteredPid()
	{
		GameServer server = new()
		{
			PID = 101,
			ServerProcesses =
			[
				new ServerProcessIdentity { ProcessId = 101 },
				new ServerProcessIdentity { ProcessId = 202 },
				new ServerProcessIdentity { ProcessId = 303 }
			]
		};

		Assert.Equal([101, 202, 303], ResourceMonitor.GetProcessIds(server).Order().ToArray());
	}

	[Fact]
	public void FirstStartAssistant_IncludesChecklistAndConnectionHelp()
	{
		GameServer server = new()
		{
			Game = "Unknown Test Game",
			ServerName = "My Server",
			InstallPath = Path.Combine(Path.GetTempPath(), "SynixGuidanceTest"),
			Port = 27015,
			QueryPort = 27016
		};

		string text = WarningDatabase.BuildAssistantText(server);

		Assert.Contains("SETUP COMPLETION", text);
		Assert.Contains("AUTOMATIC SAFETY CHECKLIST", text);
		Assert.Contains("CONFIGURATION SUPPORT", text);
		Assert.Contains("CONNECTION INFORMATION", text);
	}

	[Fact]
	public void ConnectionAddress_UsesTheGamePort()
	{
		Assert.Equal(
			"192.168.1.50:8777",
			ConnectionInformationDialog.FormatAddress("192.168.1.50", 8777));
	}

	[Fact]
	public void GuidanceWindows_ConstructWithoutRunningExternalChecks()
	{
		Exception? failure = null;
		Thread thread = new(() =>
		{
			try
			{
				GameServer server = new()
				{
					Game = "Unknown Test Game",
					ServerName = "Test",
					InstallPath = Path.GetTempPath(),
					Port = 27015
				};
				using PlainEnglishErrorDialog error = new("start the server", "port is in use");
				using ConnectionInformationDialog connection = new(server);
				using ResourceMonitorGUI processes = new(server);
				using ServerSettingsGUI setup = new();
				Control modeButton = setup.Controls.Find("btnExperienceMode", true).Single();
				Control modeBadge = setup.Controls.Find("lblModeBadge", true).Single();
				Control completion = setup.Controls.Find("lblSetupCompletion", true).Single();
				Control statusDetail = setup.Controls.Find("lblSidebarStatusDetail", true).Single();
				Assert.False(modeButton.Bounds.IntersectsWith(modeBadge.Bounds));
				Assert.False(completion.Bounds.IntersectsWith(statusDetail.Bounds));
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
}
