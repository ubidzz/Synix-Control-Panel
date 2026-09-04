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

public sealed class DiscordNotificationTests
{
	[Theory]
	[InlineData("https://discord.com/api/webhooks/123456789/secret-token")]
	[InlineData("https://DISCORD.com/api/webhooks/123456789/secret-token?wait=true")]
	public void OfficialDiscordWebhookUrl_IsAccepted(string value)
	{
		Assert.True(Core.TryValidateDiscordWebhookUrl(
			value,
			out Uri? normalized,
			out string error), error);
		Assert.NotNull(normalized);
		Assert.Equal("discord.com", normalized!.Host, ignoreCase: true);
		Assert.Empty(normalized.Query);
	}

	[Theory]
	[InlineData("http://discord.com/api/webhooks/123456789/token")]
	[InlineData("https://discord.com.example.test/api/webhooks/123456789/token")]
	[InlineData("https://example.test/api/webhooks/123456789/token")]
	[InlineData("https://discord.com/channels/123456789/987654321")]
	[InlineData("")]
	public void NonWebhookOrUnofficialUrl_IsRejected(string value)
	{
		Assert.False(Core.TryValidateDiscordWebhookUrl(
			value,
			out _,
			out string error));
		Assert.NotEmpty(error);
	}

	[Fact]
	public void MultipleWebhookRoutes_AreProtectedAndRestoredWithEvents()
	{
		const string statusWebhook =
			"https://discord.com/api/webhooks/111111111/status-token";
		const string backupWebhook =
			"https://discord.com/api/webhooks/222222222/backup-token";
		GameServer server = new();

		Core.SetServerSecrets(
			server,
			new SynixServerSecrets(default, statusWebhook));
		Core.SetDiscordWebhookRoutes(
			server,
			[
				new DiscordWebhookRoute
				{
					Name = "Backups",
					WebhookUrl = backupWebhook,
					Events = DiscordNotificationEvent.BackupCompleted
				}
			]);

		Assert.Equal(Core.CurrentStorageVersion, server.PasswordStorageVersion);
		Assert.True(Core.IsProtected(server.DiscordWebhook));
		DiscordWebhookRoute storedRoute = Assert.Single(server.DiscordWebhookRoutes);
		Assert.True(Core.IsProtected(storedRoute.WebhookUrl));
		DiscordWebhookRoute revealedRoute = Assert.Single(
			Core.RevealDiscordWebhookRoutes(server));
		Assert.Equal("Backups", revealedRoute.Name);
		Assert.Equal(backupWebhook, revealedRoute.WebhookUrl);
		Assert.Equal(
			DiscordNotificationEvent.BackupCompleted,
			revealedRoute.Events);
	}

	[Fact]
	public void EventPresets_KeepStatusMaintenanceAndProblemsSeparate()
	{
		Assert.NotEqual(
			DiscordNotificationEvent.None,
			Core.DiscordStatusEvents);
		Assert.NotEqual(
			DiscordNotificationEvent.None,
			Core.DiscordMaintenanceEvents);
		Assert.Equal(
			DiscordNotificationEvent.None,
			Core.DiscordStatusEvents & DiscordNotificationEvent.BackupCompleted);
		Assert.NotEqual(
			DiscordNotificationEvent.None,
			Core.DiscordProblemEvents & DiscordNotificationEvent.BackupFailed);
	}
}
