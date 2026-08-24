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

public sealed class GameCompatibilityTests
{
	[Fact]
	public void UnknownGame_RemainsUnverified()
	{
		string directory = CreateTestDirectory();
		try
		{
			string path = Path.Combine(directory, "compatibility.json");
			GameCompatibilityVerification verification =
				Core.GetGameCompatibility("Palworld", path);

			Assert.Equal("Palworld", verification.Game);
			Assert.Null(verification.Install);
			Assert.Null(verification.Start);
			Assert.Null(verification.Stop);
			Assert.Null(verification.Monitoring);
			Assert.Null(verification.LastTested);
			Assert.False(File.Exists(path));
		}
		finally
		{
			Directory.Delete(directory, recursive: true);
		}
	}

	[Fact]
	public void SuccessfulActions_AreSavedAndLoadedTogether()
	{
		string directory = CreateTestDirectory();
		try
		{
			string path = Path.Combine(directory, "compatibility.json");
			DateTimeOffset verifiedAt = new(2026, 8, 21, 14, 30, 0, TimeSpan.Zero);

			foreach (GameVerificationKind verificationKind in Enum.GetValues<GameVerificationKind>())
			{
				Assert.True(Core.RecordGameVerification(
					"Palworld",
					verificationKind,
					"1.0.21",
					verifiedAt.AddMinutes((int)verificationKind),
					path));
			}

			GameCompatibilityVerification verification =
				Core.GetGameCompatibility("palworld", path);

			Assert.Equal("1.0.21", verification.Install?.SynixVersion);
			Assert.Equal("1.0.21", verification.Start?.SynixVersion);
			Assert.Equal("1.0.21", verification.Stop?.SynixVersion);
			Assert.Equal("1.0.21", verification.Monitoring?.SynixVersion);
			Assert.Equal("1.0.21", verification.Arguments?.SynixVersion);
			Assert.Equal("1.0.21", verification.Configuration?.SynixVersion);
			Assert.Equal(
				verifiedAt.AddMinutes((int)GameVerificationKind.Configuration),
				verification.LastTested?.VerifiedAtUtc);
		}
		finally
		{
			Directory.Delete(directory, recursive: true);
		}
	}

	[Fact]
	public void OlderOrRepeatedVersions_DoNotReplaceNewerEvidence()
	{
		string directory = CreateTestDirectory();
		try
		{
			string path = Path.Combine(directory, "compatibility.json");
			DateTimeOffset originalTime = new(2026, 8, 21, 12, 0, 0, TimeSpan.Zero);

			Assert.True(Core.RecordGameVerification(
				"Rust",
				GameVerificationKind.Start,
				"1.0.21",
				originalTime,
				path));
			Assert.False(Core.RecordGameVerification(
				"Rust",
				GameVerificationKind.Start,
				"1.0.21",
				originalTime.AddHours(1),
				path));
			Assert.False(Core.RecordGameVerification(
				"Rust",
				GameVerificationKind.Start,
				"1.0.20",
				originalTime.AddHours(2),
				path));

			GameCompatibilityVerification verification =
				Core.GetGameCompatibility("Rust", path);
			Assert.Equal("1.0.21", verification.Start?.SynixVersion);
			Assert.Equal(originalTime, verification.Start?.VerifiedAtUtc);
		}
		finally
		{
			Directory.Delete(directory, recursive: true);
		}
	}

	[Fact]
	public void NewerVersion_ReplacesOlderEvidence()
	{
		string directory = CreateTestDirectory();
		try
		{
			string path = Path.Combine(directory, "compatibility.json");
			DateTimeOffset oldTime = new(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);
			DateTimeOffset newTime = oldTime.AddDays(1);

			Assert.True(Core.RecordGameVerification(
				"Minecraft Java",
				GameVerificationKind.Install,
				"1.0.20",
				oldTime,
				path));
			Assert.True(Core.RecordGameVerification(
				"Minecraft",
				GameVerificationKind.Install,
				"1.0.21",
				newTime,
				path));

			GameCompatibilityVerification verification =
				Core.GetGameCompatibility("Minecraft Java", path);
			Assert.Equal("Minecraft", verification.Game);
			Assert.Equal("1.0.21", verification.Install?.SynixVersion);
			Assert.Equal(newTime, verification.Install?.VerifiedAtUtc);
		}
		finally
		{
			Directory.Delete(directory, recursive: true);
		}
	}

	[Fact]
	public void DamagedRecord_DoesNotCrashAndCanRecover()
	{
		string directory = CreateTestDirectory();
		try
		{
			string path = Path.Combine(directory, "compatibility.json");
			File.WriteAllText(path, "{ damaged json");

			GameCompatibilityVerification before =
				Core.GetGameCompatibility("Palworld", path);
			Assert.Null(before.LastTested);

			Assert.True(Core.RecordGameVerification(
				"Palworld",
				GameVerificationKind.Monitoring,
				"1.0.21",
				DateTimeOffset.UtcNow,
				path));

			GameCompatibilityVerification after =
				Core.GetGameCompatibility("Palworld", path);
			Assert.Equal("1.0.21", after.Monitoring?.SynixVersion);
		}
		finally
		{
			Directory.Delete(directory, recursive: true);
		}
	}

	[Fact]
	public void VerificationQueue_IncludesEveryBuiltInGameAndManualChecks()
	{
		string directory = CreateTestDirectory();
		try
		{
			string path = Path.Combine(directory, "compatibility.json");
			IReadOnlyList<GameVerificationQueueItem> initial =
				Core.GetGameVerificationQueue(path);
			Assert.True(initial.Count >= 200);

			GameVerificationQueueItem palworld = Assert.Single(
				initial,
				item => item.Game == "Palworld");
			Assert.False(palworld.IsFullyVerified);
			Assert.Null(palworld.Verification.Arguments);
			Assert.Null(palworld.Verification.Configuration);

			Assert.True(Core.RecordGameVerification(
				"Palworld",
				GameVerificationKind.Arguments,
				"1.0.22",
				DateTimeOffset.UtcNow,
				path));
			Assert.True(Core.RecordGameVerification(
				"Palworld",
				GameVerificationKind.Configuration,
				"1.0.22",
				DateTimeOffset.UtcNow,
				path));

			GameVerificationQueueItem updated = Assert.Single(
				Core.GetGameVerificationQueue(path),
				item => item.Game == "Palworld");
			Assert.Equal("1.0.22", updated.Verification.Arguments?.SynixVersion);
			Assert.Equal("1.0.22", updated.Verification.Configuration?.SynixVersion);
			Assert.Equal(2, updated.CompletedSteps);
		}
		finally
		{
			Directory.Delete(directory, true);
		}
	}

	[Fact]
	public void VerificationEvidence_CanBeClearedWithoutRemovingOtherSteps()
	{
		string directory = CreateTestDirectory();
		try
		{
			string path = Path.Combine(directory, "compatibility.json");
			Assert.True(Core.RecordGameVerification(
				"Rust",
				GameVerificationKind.Arguments,
				"1.0.22",
				DateTimeOffset.UtcNow,
				path));
			Assert.True(Core.RecordGameVerification(
				"Rust",
				GameVerificationKind.Configuration,
				"1.0.22",
				DateTimeOffset.UtcNow,
				path));

			Assert.True(Core.ClearGameVerification(
				"Rust",
				GameVerificationKind.Arguments,
				path));
			GameCompatibilityVerification verification =
				Core.GetGameCompatibility("Rust", path);
			Assert.Null(verification.Arguments);
			Assert.NotNull(verification.Configuration);
			Assert.False(Core.ClearGameVerification(
				"Rust",
				GameVerificationKind.Arguments,
				path));
		}
		finally
		{
			Directory.Delete(directory, true);
		}
	}

	[Fact]
	public void CompatibilitySummary_ProgressesFromCommunityTestingToFullyVerified()
	{
		string directory = CreateTestDirectory();
		try
		{
			string path = Path.Combine(directory, "compatibility.json");
			GameCompatibilitySummary initial =
				Core.GetGameCompatibilitySummary("Palworld", path);
			Assert.Equal(
				GameCompatibilityStatus.NeedsCommunityTesting,
				initial.Status);

			Assert.True(Core.RecordGameVerification(
				"Palworld",
				GameVerificationKind.Install,
				"1.0.22",
				DateTimeOffset.UtcNow,
				path));
			Assert.Equal(
				GameCompatibilityStatus.InstallationVerifiedOnly,
				Core.GetGameCompatibilitySummary("Palworld", path).Status);

			foreach (GameVerificationKind kind in new[]
				{
					GameVerificationKind.Start,
					GameVerificationKind.Stop,
					GameVerificationKind.Monitoring
				})
			{
				Assert.True(Core.RecordGameVerification(
					"Palworld",
					kind,
					"1.0.22",
					DateTimeOffset.UtcNow,
					path));
			}

			Assert.Equal(
				GameCompatibilityStatus.FullyVerified,
				Core.GetGameCompatibilitySummary("Palworld", path).Status);
		}
		finally
		{
			Directory.Delete(directory, true);
		}
	}

	private static string CreateTestDirectory()
	{
		string directory = Path.Combine(
			Path.GetTempPath(),
			"Synix.Tests",
			Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(directory);
		return directory;
	}
}
