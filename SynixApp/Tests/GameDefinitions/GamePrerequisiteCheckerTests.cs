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
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class GamePrerequisiteCheckerTests
{
	[Fact]
	public void CompleteSnapshotPassesEveryDeclaredRequirement()
	{
		GameInfo definition = CreateDefinition();
		GamePrerequisiteSnapshot snapshot = new(
			64,
			true,
			true,
			"Intel VT-x",
			true,
			true,
			533320,
			true,
			new HashSet<VisualCppRedistributableRequirement>
			{
				VisualCppRedistributableRequirement.VisualCpp2013X64,
				VisualCppRedistributableRequirement.VisualCpp2015To2022X64
			});

		GamePrerequisiteReport report =
			GamePrerequisiteChecker.Evaluate(definition, snapshot);

		Assert.True(report.CanStart);
		Assert.Null(report.FirstFailure);
		Assert.All(
			report.Items,
			item => Assert.Equal(GamePrerequisiteState.Passed, item.State));
	}

	[Fact]
	public void MissingSoftwareAndHardwareBlockTheServerBeforeLaunch()
	{
		GameInfo definition = CreateDefinition();
		GamePrerequisiteSnapshot snapshot = new(
			8,
			false,
			false,
			"AMD-V (SVM)",
			false,
			false,
			528040,
			true,
			new HashSet<VisualCppRedistributableRequirement>());

		GamePrerequisiteReport report =
			GamePrerequisiteChecker.Evaluate(definition, snapshot);

		Assert.False(report.CanStart);
		Assert.Contains(report.Items, item => item.Name == "System memory" &&
			item.State == GamePrerequisiteState.Failed);
		Assert.Contains(report.Items, item => item.Name == ".NET Framework 4.8.1" &&
			item.State == GamePrerequisiteState.Failed);
		Assert.Contains(report.Items, item => item.Name.Contains("Visual C++ 2013") &&
			item.State == GamePrerequisiteState.Failed);
	}

	[Fact]
	public void UnreadableWindowsInformationWarnsWithoutFalseBlocking()
	{
		GameInfo definition = CreateDefinition();
		GamePrerequisiteSnapshot snapshot = new(
			64,
			true,
			null,
			"hardware virtualization",
			null,
			null,
			null,
			false,
			new HashSet<VisualCppRedistributableRequirement>());

		GamePrerequisiteReport report =
			GamePrerequisiteChecker.Evaluate(definition, snapshot);

		Assert.True(report.CanStart);
		Assert.Contains(
			report.Items,
			item => item.State == GamePrerequisiteState.Warning);
		Assert.DoesNotContain(
			report.Items,
			item => item.State == GamePrerequisiteState.Failed);
	}

	[Fact]
	public void StartCheckIgnoresAnUnusedQueryPort()
	{
		GameInfo astroneer =
			Synix_Control_Panel.SynixApp.Database.GameDatabase.GetGame("ASTRONEER")!;
		GameServer server = new()
		{
			Game = astroneer.Game,
			Port = 8778,
			QueryPort = 8777
		};

		GamePrerequisiteReport report =
			GamePrerequisiteChecker.CheckCurrentSystem(
				astroneer,
				server,
				port => port == 8777 ? "Another Server" : null,
				_ => false);

		Assert.True(report.CanStart);
		Assert.DoesNotContain(report.Items, item => item.Name == "Port 8777");
	}

	[Fact]
	public void StartCheckBlocksASharedQueryPortOwnedByAnActiveServer()
	{
		GameInfo definition = new()
		{
			Game = "Shared Query Port Test",
			RequiredArgs = "-Port={port} -QueryPort={query}"
		};
		GameServer server = new()
		{
			Game = definition.Game,
			Port = 26900,
			QueryPort = 27015
		};

		GamePrerequisiteReport report =
			GamePrerequisiteChecker.CheckCurrentSystem(
				definition,
				server,
				port => port == 27015 ? "Running Server" : null,
				_ => false);

		Assert.False(report.CanStart);
		Assert.Contains(
			report.Items,
			item => item.Name == "Port 27015" &&
				item.State == GamePrerequisiteState.Failed &&
				item.Message.Contains("Running Server"));
	}

	private static GameInfo CreateDefinition() => new()
	{
		Game = "Prerequisite Test",
		RuntimeRequirements = new GameRuntimeRequirements
		{
			MinimumSystemMemoryGb = 16,
			RequiresAvx2 = true,
			RequiresHardwareVirtualization = true,
			RequiresHyperV = true,
			RequiresWindowsProfessionalOrHigher = true,
			MinimumDotNetFramework =
				DotNetFrameworkRequirement.NetFramework481,
			VisualCppRedistributables =
			[
				VisualCppRedistributableRequirement.VisualCpp2013X64,
				VisualCppRedistributableRequirement.VisualCpp2015To2022X64
			]
		}
	};
}
