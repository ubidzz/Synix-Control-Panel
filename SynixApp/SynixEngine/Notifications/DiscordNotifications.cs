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
using System.Net;
using System.Text;
using System.Text.Json;

namespace Synix_Control_Panel.SynixEngine
{
	public sealed record DiscordNotificationOption(
		DiscordNotificationEvent Event,
		string Name,
		string Group);
	public sealed record DiscordWebhookTestResult(bool Succeeded, string Message);

	public partial class Core
	{
		public static readonly DiscordNotificationEvent DiscordStatusEvents =
			DiscordNotificationEvent.ServerStarting |
			DiscordNotificationEvent.ServerOnline |
			DiscordNotificationEvent.ServerStopping |
			DiscordNotificationEvent.ServerStopped |
			DiscordNotificationEvent.ServerRestarting |
			DiscordNotificationEvent.ServerCrashed;

		public static readonly DiscordNotificationEvent DiscordMaintenanceEvents =
			DiscordNotificationEvent.InstallStarted |
			DiscordNotificationEvent.InstallCompleted |
			DiscordNotificationEvent.InstallFailed |
			DiscordNotificationEvent.UpdateStarted |
			DiscordNotificationEvent.UpdateCompleted |
			DiscordNotificationEvent.UpdateFailed |
			DiscordNotificationEvent.VerificationStarted |
			DiscordNotificationEvent.VerificationCompleted |
			DiscordNotificationEvent.VerificationFailed |
			DiscordNotificationEvent.BackupStarted |
			DiscordNotificationEvent.BackupCompleted |
			DiscordNotificationEvent.BackupFailed |
			DiscordNotificationEvent.RestoreStarted |
			DiscordNotificationEvent.RestoreCompleted |
			DiscordNotificationEvent.RestoreFailed;

		public static readonly DiscordNotificationEvent DiscordProblemEvents =
			DiscordNotificationEvent.ServerCrashed |
			DiscordNotificationEvent.InstallFailed |
			DiscordNotificationEvent.UpdateFailed |
			DiscordNotificationEvent.VerificationFailed |
			DiscordNotificationEvent.BackupFailed |
			DiscordNotificationEvent.RestoreFailed |
			DiscordNotificationEvent.ResourceWarning |
			DiscordNotificationEvent.MonitoringWarning |
			DiscordNotificationEvent.ConfigurationWarning |
			DiscordNotificationEvent.SecurityWarning;

		private static readonly DiscordNotificationOption[] DiscordOptions =
		[
			new(DiscordNotificationEvent.ServerStarting, "Server starting", "Server status"),
			new(DiscordNotificationEvent.ServerOnline, "Server online", "Server status"),
			new(DiscordNotificationEvent.ServerStopping, "Server stopping", "Server status"),
			new(DiscordNotificationEvent.ServerStopped, "Server stopped", "Server status"),
			new(DiscordNotificationEvent.ServerRestarting, "Server restarting", "Server status"),
			new(DiscordNotificationEvent.ServerCrashed, "Crash detected", "Server status"),
			new(DiscordNotificationEvent.InstallStarted, "Install started", "Installation and maintenance"),
			new(DiscordNotificationEvent.InstallCompleted, "Install completed", "Installation and maintenance"),
			new(DiscordNotificationEvent.InstallFailed, "Install failed", "Installation and maintenance"),
			new(DiscordNotificationEvent.UpdateStarted, "Update started", "Installation and maintenance"),
			new(DiscordNotificationEvent.UpdateCompleted, "Update completed", "Installation and maintenance"),
			new(DiscordNotificationEvent.UpdateFailed, "Update failed", "Installation and maintenance"),
			new(DiscordNotificationEvent.VerificationStarted, "File verification started", "Installation and maintenance"),
			new(DiscordNotificationEvent.VerificationCompleted, "File verification completed", "Installation and maintenance"),
			new(DiscordNotificationEvent.VerificationFailed, "File verification failed", "Installation and maintenance"),
			new(DiscordNotificationEvent.BackupStarted, "Backup started", "Backups and restoration"),
			new(DiscordNotificationEvent.BackupCompleted, "Backup completed", "Backups and restoration"),
			new(DiscordNotificationEvent.BackupFailed, "Backup failed", "Backups and restoration"),
			new(DiscordNotificationEvent.RestoreStarted, "Restore started", "Backups and restoration"),
			new(DiscordNotificationEvent.RestoreCompleted, "Restore completed", "Backups and restoration"),
			new(DiscordNotificationEvent.RestoreFailed, "Restore failed", "Backups and restoration"),
			new(DiscordNotificationEvent.ResourceWarning, "CPU or RAM warning", "Health and security"),
			new(DiscordNotificationEvent.MonitoringWarning, "Monitoring or connectivity warning", "Health and security"),
			new(DiscordNotificationEvent.ConfigurationWarning, "Configuration warning", "Health and security"),
			new(DiscordNotificationEvent.SecurityWarning, "Security action blocked", "Health and security")
		];

		private readonly SemaphoreSlim _discordDeliveryLock = new(1, 1);

		public static IReadOnlyList<DiscordNotificationOption> GetDiscordNotificationOptions() =>
			DiscordOptions;

		public static bool TryValidateDiscordWebhookUrl(
			string? value,
			out Uri? webhookUri,
			out string error)
		{
			webhookUri = null;
			error = string.Empty;
			if (string.IsNullOrWhiteSpace(value))
			{
				error = "Enter a Discord webhook URL.";
				return false;
			}

			if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out Uri? candidate) ||
				candidate.Scheme != Uri.UriSchemeHttps ||
				!candidate.Host.Equals("discord.com", StringComparison.OrdinalIgnoreCase) ||
				!string.IsNullOrEmpty(candidate.UserInfo) ||
				!string.IsNullOrEmpty(candidate.Fragment))
			{
				error = "Use an HTTPS webhook URL created by discord.com.";
				return false;
			}

			string[] segments = candidate.AbsolutePath
				.Split('/', StringSplitOptions.RemoveEmptyEntries);
			if (segments.Length != 4 ||
				!segments[0].Equals("api", StringComparison.OrdinalIgnoreCase) ||
				!segments[1].Equals("webhooks", StringComparison.OrdinalIgnoreCase) ||
				!long.TryParse(segments[2], out _) ||
				string.IsNullOrWhiteSpace(segments[3]))
			{
				error = "The Discord webhook URL is incomplete or has an unsupported format.";
				return false;
			}

			webhookUri = new UriBuilder(candidate)
			{
				Query = string.Empty,
				Fragment = string.Empty
			}.Uri;
			return true;
		}

		public async Task SendDiscordNotification(
			GameServer server,
			DiscordNotificationEvent notificationEvent,
			string title,
			string message,
			Color color)
		{
			ArgumentNullException.ThrowIfNull(server);
			if (notificationEvent == DiscordNotificationEvent.None)
				return;

			List<(string Name, string Url)> destinations;
			try
			{
				destinations = GetDiscordDestinations(server, notificationEvent);
			}
			catch (SynixPasswordProtectionException)
			{
				Log(
					"[👾 DISCORD ERROR] Synix could not unlock one or more saved Discord webhooks. Re-enter them in Server Settings.",
					Color.Red);
				return;
			}

			foreach ((string destinationName, string webhookUrl) in destinations)
			{
				if (!TryValidateDiscordWebhookUrl(webhookUrl, out Uri? webhookUri, out string error))
				{
					Log($"[👾 DISCORD ERROR] {destinationName}: {error}", Color.Red);
					continue;
				}

				await SendDiscordPayloadAsync(
					webhookUri!,
					server,
					notificationEvent,
					title,
					message,
					color,
					destinationName);
			}
		}

		public async Task<DiscordWebhookTestResult> SendDiscordTestAsync(
			string webhookUrl,
			string serverName,
			string destinationName)
		{
			if (!TryValidateDiscordWebhookUrl(webhookUrl, out Uri? webhookUri, out string error))
				return new DiscordWebhookTestResult(false, error);

			GameServer testServer = new()
			{
				ServerName = string.IsNullOrWhiteSpace(serverName) ? "Test Server" : serverName.Trim()
			};
			string deliveryError = string.Empty;
			bool succeeded = await SendDiscordPayloadAsync(
				webhookUri!,
				testServer,
				DiscordNotificationEvent.ServerOnline,
				"TEST CONNECTION",
				$"Synix successfully reached {destinationName}.",
				Color.LimeGreen,
				destinationName,
				logFailure: false,
				errorSink: value => deliveryError = value);
			return new DiscordWebhookTestResult(
				succeeded,
				succeeded ? "Discord received the test message." : deliveryError);
		}

		private static List<(string Name, string Url)> GetDiscordDestinations(
			GameServer server,
			DiscordNotificationEvent notificationEvent)
		{
			List<(string Name, string Url)> destinations = [];
			HashSet<string> uniqueUrls = new(StringComparer.OrdinalIgnoreCase);

			if (server.IsDiscordAlertEnabled &&
				(server.DiscordEvents & notificationEvent) != 0)
			{
				string masterUrl = RevealDiscordWebhook(server);
				if (!string.IsNullOrWhiteSpace(masterUrl) && uniqueUrls.Add(masterUrl))
					destinations.Add(("Master webhook", masterUrl));
			}

			foreach (DiscordWebhookRoute route in RevealDiscordWebhookRoutes(server))
			{
				if (!route.Enabled ||
					(route.Events & notificationEvent) == 0 ||
					string.IsNullOrWhiteSpace(route.WebhookUrl) ||
					!uniqueUrls.Add(route.WebhookUrl))
				{
					continue;
				}

				destinations.Add((
					string.IsNullOrWhiteSpace(route.Name) ? "Discord channel" : route.Name,
					route.WebhookUrl));
			}

			return destinations;
		}

		private async Task<bool> SendDiscordPayloadAsync(
			Uri webhookUri,
			GameServer server,
			DiscordNotificationEvent notificationEvent,
			string title,
			string message,
			Color color,
			string destinationName,
			bool logFailure = true,
			Action<string>? errorSink = null)
		{
			int discordColor = (color.R << 16) | (color.G << 8) | color.B;
			var payload = new
			{
				allowed_mentions = new { parse = Array.Empty<string>() },
				embeds = new[]
				{
					new
					{
						title = $"🛰️ {LimitDiscordText(server.ServerName, 80)} | {LimitDiscordText(title, 100)}",
						description = LimitDiscordText(message, 3500),
						color = discordColor,
						fields = new[]
						{
							new
							{
								name = "Synix event",
								value = GetDiscordEventName(notificationEvent),
								inline = true
							}
						},
						footer = new { text = "Synix Engine • Secure Server Automation" },
						timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
					}
				}
			};

			string json = JsonSerializer.Serialize(payload);
			await _discordDeliveryLock.WaitAsync();
			try
			{
				for (int attempt = 0; attempt < 3; attempt++)
				{
					using StringContent content = new(json, Encoding.UTF8, "application/json");
					using HttpResponseMessage response = await _discordClient.PostAsync(webhookUri, content);
					if (response.IsSuccessStatusCode)
						return true;

					if (response.StatusCode == HttpStatusCode.TooManyRequests && attempt < 2)
					{
						TimeSpan delay = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(2);
						await Task.Delay(TimeSpan.FromMilliseconds(Math.Clamp(delay.TotalMilliseconds, 250, 5000)));
						continue;
					}

					string failure = $"{destinationName} returned {(int)response.StatusCode} {response.ReasonPhrase}.";
					errorSink?.Invoke(failure);
					if (logFailure)
						Log($"[👾 DISCORD] {failure}", Color.Red);
					return false;
				}
			}
			catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
			{
				string failure = $"Discord delivery to {destinationName} failed ({exception.GetType().Name}).";
				errorSink?.Invoke(failure);
				if (logFailure)
					Log($"[👾 DISCORD ERROR] {failure}", Color.Red);
				return false;
			}
			finally
			{
				_discordDeliveryLock.Release();
			}

			return false;
		}

		public static string GetDiscordEventName(DiscordNotificationEvent notificationEvent) =>
			DiscordOptions.FirstOrDefault(option => option.Event == notificationEvent)?.Name ??
			notificationEvent.ToString();

		public static string SummarizeDiscordEvents(DiscordNotificationEvent events)
		{
			if (events == DiscordNotificationEvent.None)
				return "No events";
			if ((events & DiscordNotificationEvent.All) == DiscordNotificationEvent.All)
				return "All events";
			if ((events & DiscordProblemEvents) == events)
				return "Problems only";
			if ((events & DiscordStatusEvents) == events)
				return "Server status";
			if ((events & DiscordMaintenanceEvents) == events)
				return "Maintenance";

			int selected = DiscordOptions.Count(option => (events & option.Event) != 0);
			return $"{selected} selected event{(selected == 1 ? string.Empty : "s")}";
		}

		private static string LimitDiscordText(string? value, int maximumLength)
		{
			string text = string.IsNullOrWhiteSpace(value) ? "Synix server" : value.Trim();
			return text.Length <= maximumLength
				? text
				: text[..maximumLength];
		}
	}
}
