using System.Globalization;
using System.Resources;
using Synix_Control_Panel.SynixApp.Localization;
using Synix_Control_Panel.SynixEngine;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class ProcessRecoveryMessageTests
{
	public static IEnumerable<object[]> ReconnectMessages() =>
		from language in LocalizationManager.SupportedLanguages
		from key in new[] { "Status.Activity.Reconnected", "Status.Activity.ReconnectedByPath" }
		select new object[] { language.Code, language.ResourceCultureName, key };

	[Theory]
	[MemberData(nameof(ReconnectMessages))]
	public void ReconnectingIdentifiesEachServerInDashboardAndHistory(
		string languageCode, string resourceCulture, string resourceKey)
	{
		ResourceManager resources = new("Synix_Control_Panel.Localization.Strings", typeof(LocalizationManager).Assembly);
		ResourceSet catalog = Assert.IsAssignableFrom<ResourceSet>(resources.GetResourceSet(
			CultureInfo.GetCultureInfo(resourceCulture), createIfNotExists: true, tryParents: false));
		string template = Assert.IsType<string>(catalog.GetString(resourceKey));
		Assert.Contains("{0}", template);
		Assert.Contains("{1}", template);

		string previousLanguage = LocalizationManager.CurrentLanguageCode;
		List<ApplicationLogEventArgs> messages = [];
		EventHandler<ApplicationLogEventArgs> listener = (_, message) => messages.Add(message);
		ApplicationUiService.LogRequested += listener;
		try
		{
			LocalizationManager.Initialize(languageCode);
			GameServer[] servers =
			[
				new() { Game = "Minecraft", ServerName = "青い森 • Café", InstallPath = Path.Combine(Path.GetTempPath(), "SynixReconnect-" + Guid.NewGuid().ToString("N")), PID = 43210 },
				new() { Game = "Enshrouded", ServerName = "Enshrouded Friends", InstallPath = Path.Combine(Path.GetTempPath(), "SynixReconnect-" + Guid.NewGuid().ToString("N")), PID = 54321 }
			];
			foreach (GameServer server in servers)
			{
				using var scope = ServerDiagnosticScope.Begin(server);
				ApplicationLogService.WriteLocalized(resourceKey, arguments: [server.ServerName, server.PID]);
			}

			Assert.Equal(servers.Length, messages.Count);
			for (int index = 0; index < servers.Length; index++)
			{
				GameServer server = servers[index];
				ApplicationLogEventArgs message = messages[index];
				string localized = Assert.IsType<string>(message.LocalizedMessage);
				Assert.Equal(string.Format(CultureInfo.GetCultureInfo(languageCode), template, server.ServerName, server.PID), localized);
				Assert.Contains(server.ServerName, localized);
				Assert.Contains(server.PID!.Value.ToString(CultureInfo.InvariantCulture), localized);
				Assert.DoesNotContain("Status.Activity.", localized);
				Assert.Contains("Reconnected to running server", message.TechnicalMessage);
				Assert.Contains(server.ServerName, message.TechnicalMessage);
				Assert.Contains(server.PID.Value.ToString(CultureInfo.InvariantCulture), message.TechnicalMessage);
				if (languageCode != LocalizationManager.DefaultLanguageCode)
					Assert.NotEqual(message.TechnicalMessage, localized);

				ServerActivityEntry entry = Assert.Single(ServerActivityHistory.Current.Read(server).Entries);
				Assert.Equal(resourceKey, entry.Diagnostic?.ResourceKey);
				Assert.Equal(localized, entry.Description);
				Assert.DoesNotContain(servers[1 - index].ServerName, entry.Description);
			}
		}
		finally
		{
			ApplicationUiService.LogRequested -= listener;
			LocalizationManager.Initialize(previousLanguage);
		}
	}
}
