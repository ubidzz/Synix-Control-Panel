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
			Assert.Equal(
				verifiedAt.AddMinutes((int)GameVerificationKind.Monitoring),
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
