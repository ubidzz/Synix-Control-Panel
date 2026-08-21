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

public sealed class ProblemReportingTests
{
	[Fact]
	public void PreparedReport_RemovesCommonPrivateValues()
	{
		const string webhook =
			"https://discord.com/api/webhooks/123456789/secret-webhook-value";
		PreparedProblemReport report = Core.PrepareProblemReport(
			new ProblemReportDraft(
				"Palworld",
				"Server startup",
				"The server did not stay running",
				$"password=TopSecret {webhook} 192.168.1.25 C:\\Users\\PrivateName\\Desktop --rcon-password HiddenValue",
				"The server should stay running."));

		Assert.DoesNotContain("TopSecret", report.Body);
		Assert.DoesNotContain("secret-webhook-value", report.Body);
		Assert.DoesNotContain("192.168.1.25", report.Body);
		Assert.DoesNotContain("PrivateName", report.Body);
		Assert.DoesNotContain("HiddenValue", report.Body);
		Assert.Contains("[secret removed]", report.Body);
		Assert.Contains("[Discord webhook removed]", report.Body);
		Assert.Contains("[IP address removed]", report.Body);
		Assert.Contains(@"C:\Users\[user]", report.Body);
	}

	[Fact]
	public void PreparedReport_UsesFixedLabelsWithoutGameNames()
	{
		PreparedProblemReport report = Core.PrepareProblemReport(
			new ProblemReportDraft(
				"ARK: Survival Ascended",
				"Incorrect server status",
				"Status stayed stopped",
				"The server window was running, but Synix showed stopped.",
				"Synix should show the running status."));

		Assert.Equal(
			new[] { "compatibility-report", "needs-triage" },
			report.Labels);
		Assert.DoesNotContain(
			report.Labels,
			label => label.Contains("ARK", StringComparison.OrdinalIgnoreCase));
	}

	[Fact]
	public void PreparedReport_AddsVersionsAndLocalVerificationSection()
	{
		PreparedProblemReport report = Core.PrepareProblemReport(
			new ProblemReportDraft(
				"Minecraft",
				"Server installation",
				"Installation stopped",
				"The progress bar stopped before installation completed.",
				"Installation should complete."));

		Assert.Contains("## Automatic system information", report.Body);
		Assert.Contains("**Synix version:**", report.Body);
		Assert.Contains("**Windows version:**", report.Body);
		Assert.Contains("## Local verification history", report.Body);
		Assert.Contains("**Install verified:**", report.Body);
		Assert.Contains("**Start verified:**", report.Body);
		Assert.Contains("**Stop verified:**", report.Body);
		Assert.Contains("**Monitoring verified:**", report.Body);
	}

	[Fact]
	public void GitHubConnectionStorage_EncryptsTokensAndUserName()
	{
		string directory = CreateTestDirectory();
		try
		{
			string path = Path.Combine(directory, "github.json");
			Core.GitHubConnectionState state = new()
			{
				ProtectedAccessToken = Core.Protect("ghu_access_value"),
				ProtectedRefreshToken = Core.Protect("ghr_refresh_value"),
				ProtectedUserName = Core.Protect("private-user-name"),
				AccessTokenExpiresAtUtc = DateTimeOffset.UtcNow.AddHours(8),
				RefreshTokenExpiresAtUtc = DateTimeOffset.UtcNow.AddMonths(6)
			};

			Core.SaveGitHubConnection(state, path);

			string storedJson = File.ReadAllText(path);
			Assert.DoesNotContain("ghu_access_value", storedJson);
			Assert.DoesNotContain("ghr_refresh_value", storedJson);
			Assert.DoesNotContain("private-user-name", storedJson);
			Assert.Contains(Core.ProtectedValuePrefix, storedJson);

			Core.GitHubConnectionState restored =
				Core.LoadGitHubConnection(path)!;
			Assert.Equal("ghu_access_value", Core.Reveal(restored.ProtectedAccessToken));
			Assert.Equal("ghr_refresh_value", Core.Reveal(restored.ProtectedRefreshToken));
			Assert.Equal("private-user-name", restored.UserName);
		}
		finally
		{
			Directory.Delete(directory, recursive: true);
		}
	}

	[Fact]
	public void GitHubConnectionStorage_RejectsPlaintextTokens()
	{
		string directory = CreateTestDirectory();
		try
		{
			string path = Path.Combine(directory, "github.json");
			Core.GitHubConnectionState state = new()
			{
				ProtectedAccessToken = "ghu_plaintext",
				ProtectedUserName = Core.Protect("user")
			};

			Assert.Throws<ProblemReportException>(() =>
				Core.SaveGitHubConnection(state, path));
			Assert.False(File.Exists(path));
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
