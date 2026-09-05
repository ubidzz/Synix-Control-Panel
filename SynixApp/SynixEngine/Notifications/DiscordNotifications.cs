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
		string NameResourceKey,
		string GroupResourceKey)
	{
		public string Name => LocalizationManager.Get(NameResourceKey);
		public string Group => LocalizationManager.Get(GroupResourceKey);
	}
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
			new(DiscordNotificationEvent.ServerStarting, "Discord.Event.ServerStarting", "Discord.Group.ServerStatus"),
			new(DiscordNotificationEvent.ServerOnline, "Discord.Event.ServerOnline", "Discord.Group.ServerStatus"),
			new(DiscordNotificationEvent.ServerStopping, "Discord.Event.ServerStopping", "Discord.Group.ServerStatus"),
			new(DiscordNotificationEvent.ServerStopped, "Discord.Event.ServerStopped", "Discord.Group.ServerStatus"),
			new(DiscordNotificationEvent.ServerRestarting, "Discord.Event.ServerRestarting", "Discord.Group.ServerStatus"),
			new(DiscordNotificationEvent.ServerCrashed, "Discord.Event.ServerCrashed", "Discord.Group.ServerStatus"),
			new(DiscordNotificationEvent.InstallStarted, "Discord.Event.InstallStarted", "Discord.Group.Maintenance"),
			new(DiscordNotificationEvent.InstallCompleted, "Discord.Event.InstallCompleted", "Discord.Group.Maintenance"),
			new(DiscordNotificationEvent.InstallFailed, "Discord.Event.InstallFailed", "Discord.Group.Maintenance"),
			new(DiscordNotificationEvent.UpdateStarted, "Discord.Event.UpdateStarted", "Discord.Group.Maintenance"),
			new(DiscordNotificationEvent.UpdateCompleted, "Discord.Event.UpdateCompleted", "Discord.Group.Maintenance"),
			new(DiscordNotificationEvent.UpdateFailed, "Discord.Event.UpdateFailed", "Discord.Group.Maintenance"),
			new(DiscordNotificationEvent.VerificationStarted, "Discord.Event.VerificationStarted", "Discord.Group.Maintenance"),
			new(DiscordNotificationEvent.VerificationCompleted, "Discord.Event.VerificationCompleted", "Discord.Group.Maintenance"),
			new(DiscordNotificationEvent.VerificationFailed, "Discord.Event.VerificationFailed", "Discord.Group.Maintenance"),
			new(DiscordNotificationEvent.BackupStarted, "Discord.Event.BackupStarted", "Discord.Group.Backups"),
			new(DiscordNotificationEvent.BackupCompleted, "Discord.Event.BackupCompleted", "Discord.Group.Backups"),
			new(DiscordNotificationEvent.BackupFailed, "Discord.Event.BackupFailed", "Discord.Group.Backups"),
			new(DiscordNotificationEvent.RestoreStarted, "Discord.Event.RestoreStarted", "Discord.Group.Backups"),
			new(DiscordNotificationEvent.RestoreCompleted, "Discord.Event.RestoreCompleted", "Discord.Group.Backups"),
			new(DiscordNotificationEvent.RestoreFailed, "Discord.Event.RestoreFailed", "Discord.Group.Backups"),
			new(DiscordNotificationEvent.ResourceWarning, "Discord.Event.ResourceWarning", "Discord.Group.Health"),
			new(DiscordNotificationEvent.MonitoringWarning, "Discord.Event.MonitoringWarning", "Discord.Group.Health"),
			new(DiscordNotificationEvent.ConfigurationWarning, "Discord.Event.ConfigurationWarning", "Discord.Group.Health"),
			new(DiscordNotificationEvent.SecurityWarning, "Discord.Event.SecurityWarning", "Discord.Group.Health")
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
				error = LocalizationManager.Get("Discord.Validation.UrlRequired");
				return false;
			}

			if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out Uri? candidate) ||
				candidate.Scheme != Uri.UriSchemeHttps ||
				!candidate.Host.Equals("discord.com", StringComparison.OrdinalIgnoreCase) ||
				!string.IsNullOrEmpty(candidate.UserInfo) ||
				!string.IsNullOrEmpty(candidate.Fragment))
			{
				error = LocalizationManager.Get("Discord.Validation.HttpsRequired");
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
				error = LocalizationManager.Get("Discord.Validation.UrlFormat");
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
				LogLocalized("Discord.Activity.WebhookUnlockFailed", Color.Red);
				return;
			}

			foreach ((string destinationName, string webhookUrl) in destinations)
			{
				if (!TryValidateDiscordWebhookUrl(webhookUrl, out Uri? webhookUri, out string error))
				{
					LogLocalized("Discord.Activity.DestinationError", Color.Red, false, destinationName, error);
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
				ServerName = string.IsNullOrWhiteSpace(serverName)
					? LocalizationManager.Get("Discord.Test.DefaultServerName")
					: serverName.Trim()
			};
			string deliveryError = string.Empty;
			bool succeeded = await SendDiscordPayloadAsync(
				webhookUri!,
				testServer,
				DiscordNotificationEvent.ServerOnline,
				LocalizationManager.Get("Discord.Test.Title"),
				LocalizationManager.Get("Discord.Test.Body", destinationName),
				Color.LimeGreen,
				destinationName,
				logFailure: false,
				errorSink: value => deliveryError = value);
			return new DiscordWebhookTestResult(
				succeeded,
				succeeded ? LocalizationManager.Get("Discord.Test.Succeeded") : deliveryError);
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
					destinations.Add((LocalizationManager.Get("Discord.Destination.Master"), masterUrl));
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
					string.IsNullOrWhiteSpace(route.Name)
						? LocalizationManager.Get("Discord.Destination.Channel")
						: route.Name,
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
								name = LocalizationManager.Get("Discord.Payload.EventField"),
								value = GetDiscordEventName(notificationEvent),
								inline = true
							}
						},
						footer = new { text = LocalizationManager.Get("Discord.Payload.Footer") },
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

					string failure = LocalizationManager.Get(
						"Discord.Error.HttpResponse",
						destinationName,
						(int)response.StatusCode,
						response.ReasonPhrase ?? string.Empty);
					errorSink?.Invoke(failure);
					if (logFailure)
						LogLocalized("Discord.Activity.Error", Color.Red, false, failure);
					return false;
				}
			}
			catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
			{
				string failure = LocalizationManager.Get(
					"Discord.Error.DeliveryFailed",
					destinationName,
					exception.GetType().Name);
				errorSink?.Invoke(failure);
				if (logFailure)
					LogLocalized("Discord.Activity.Error", Color.Red, false, failure);
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
				return LocalizationManager.Get("Discord.Summary.None");
			if ((events & DiscordNotificationEvent.All) == DiscordNotificationEvent.All)
				return LocalizationManager.Get("Discord.Summary.All");
			if ((events & DiscordProblemEvents) == events)
				return LocalizationManager.Get("Discord.Summary.ProblemsOnly");
			if ((events & DiscordStatusEvents) == events)
				return LocalizationManager.Get("Discord.Summary.ServerStatus");
			if ((events & DiscordMaintenanceEvents) == events)
				return LocalizationManager.Get("Discord.Summary.Maintenance");

			int selected = DiscordOptions.Count(option => (events & option.Event) != 0);
			return LocalizationManager.Get(
				selected == 1 ? "Discord.Summary.SelectedOne" : "Discord.Summary.SelectedMany",
				selected);
		}

		private static string LimitDiscordText(string? value, int maximumLength)
		{
			string text = string.IsNullOrWhiteSpace(value)
				? LocalizationManager.Get("Discord.Payload.DefaultServerName")
				: value.Trim();
			return text.Length <= maximumLength
				? text
				: text[..maximumLength];
		}
	}
}
