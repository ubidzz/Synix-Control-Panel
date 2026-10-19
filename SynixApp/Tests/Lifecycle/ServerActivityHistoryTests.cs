using System.Text.Json;
using Synix_Control_Panel.SynixApp.Localization;
using Synix_Control_Panel.SynixEngine;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class ServerActivityHistoryTests : IDisposable
{
	private readonly string _root = Path.Combine(Path.GetTempPath(), "SynixHistoryTests-" + Guid.NewGuid().ToString("N"));
	private GameServer Server(string name = "Example") => new()
	{
		Game = "Minecraft", ServerName = name, InstallPath = Path.Combine(_root, "server"),
		Password = "never-store-password", AdminPassword = "never-store-admin",
		RconPassword = "never-store-rcon", AuthenticationToken = "never-store-token"
	};
	private string HistoryFile(GameServer server) => Path.Combine(_root, ServerActivityHistory.ServerKey(server) + ".jsonl");

	[Fact]
	public void HistorySurvivesReopeningAndRenamingWithoutSerializingServerData()
	{
		ServerActivityHistory history = new(_root);
		GameServer server = Server("Private server name");
		Assert.True(history.Record(server, "ConfigurationSaved"));
		Assert.True(history.Record(server, DiscordNotificationEvent.BackupCompleted));
		ServerActivityReadResult restored = new ServerActivityHistory(_root).Read(Server("Renamed"));
		Assert.False(restored.Unavailable);
		Assert.Equal(new[] { "ConfigurationSaved", "BackupCompleted" }, restored.Entries.Select(entry => entry.Event));
		string persisted = File.ReadAllText(HistoryFile(server));
		Assert.DoesNotContain("never-store", persisted);
		Assert.DoesNotContain(server.ServerName, persisted);
		Assert.DoesNotContain(server.InstallPath, persisted);
		Assert.DoesNotContain("Description", persisted);
		Assert.Empty(Directory.GetFiles(_root, "*.tmp"));
	}

	[Fact]
	public void OnlyKnownSingleEventsCanBeRecorded()
	{
		ServerActivityHistory history = new(_root);
		Assert.False(history.Record(Server(), "password=secret"));
		Assert.False(history.Record(Server(), DiscordNotificationEvent.None));
		Assert.False(history.Record(Server(), DiscordNotificationEvent.All));
		Assert.False(history.Record(Server(), DiscordNotificationEvent.BackupFailed | DiscordNotificationEvent.BackupCompleted));
		Assert.False(history.Record(Server(), "1"));
		Assert.False(Directory.Exists(_root));
	}

	[Fact]
	public async Task ConcurrentStoresDoNotLoseEvents()
	{
		ServerActivityHistory first = new(_root), second = new(_root);
		GameServer server = Server();
		Task<bool>[] writes = Enumerable.Range(0, 30).Select(index => Task.Run(() =>
			(index % 2 == 0 ? first : second).Record(server, "ConfigurationSaved"))).ToArray();
		Assert.All(await Task.WhenAll(writes), Assert.True);
		Assert.Equal(30, first.Read(server).Entries.Count);
	}

	[Fact]
	public void RetentionKeepsNewestEventsAndDoesNotMixServers()
	{
		Directory.CreateDirectory(_root);
		GameServer server = Server();
		DateTimeOffset start = DateTimeOffset.UtcNow.AddDays(-1);
		File.WriteAllLines(HistoryFile(server), Enumerable.Range(0, 1010).Select(index =>
			JsonSerializer.Serialize(new { Version = 1, OccurredUtc = start.AddSeconds(index), Event = "ServerStarting" })));
		ServerActivityHistory history = new(_root);
		Assert.True(history.Record(server, "ConfigurationSaved", start.AddSeconds(1010)));
		ServerActivityReadResult result = history.Read(server);
		Assert.Equal(ServerActivityHistory.MaximumEntries, result.Entries.Count);
		Assert.Equal(start.AddSeconds(11), result.Entries[0].OccurredUtc);
		Assert.Equal(1000, File.ReadLines(HistoryFile(server)).Count());
		GameServer other = Server(); other.InstallPath += "-other";
		Assert.Empty(history.Read(other).Entries);
	}

	[Fact]
	public void InterruptedEntryDoesNotHideReadableHistory()
	{
		ServerActivityHistory history = new(_root);
		Assert.True(history.Record(Server(), DiscordNotificationEvent.UpdateCompleted));
		File.AppendAllText(HistoryFile(Server()), "{\"Version\":1,");
		ServerActivityReadResult result = new ServerActivityHistory(_root).Read(Server());
		Assert.True(result.Recovered);
		Assert.False(result.Unavailable);
		Assert.Equal("UpdateCompleted", Assert.Single(result.Entries).Event);
	}

	[Fact]
	public void FutureFormatsAndOversizedHistoryAreNotOverwritten()
	{
		Directory.CreateDirectory(_root);
		string path = HistoryFile(Server());
		string future = JsonSerializer.Serialize(new { Version = 3, OccurredUtc = DateTimeOffset.UtcNow, Event = "ServerStarting" });
		File.WriteAllText(path, future);
		ServerActivityHistory history = new(_root);
		Assert.True(history.Read(Server()).Unavailable);
		Assert.False(history.Record(Server(), "ConfigurationSaved"));
		Assert.Equal(future, File.ReadAllText(path));
		File.WriteAllText(path, new string('x', 512 * 1024 + 1));
		Assert.True(history.Read(Server()).Unavailable);
		Assert.False(history.Record(Server(), "ConfigurationSaved"));
		Assert.Equal(512 * 1024 + 1, new FileInfo(path).Length);
	}

	[Fact]
	public void FailedReplacementKeepsOldHistoryAndASecondAttemptRecovers()
	{
		ServerActivityHistory history = new(_root);
		Assert.True(history.Record(Server(), DiscordNotificationEvent.BackupCompleted));
		using (FileStream locked = new(HistoryFile(Server()), FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			Assert.False(history.Record(Server(), DiscordNotificationEvent.BackupFailed));
			Assert.True(history.Read(Server()).Unavailable);
			Assert.Single(history.Read(Server()).Entries);
		}
		Assert.True(history.Record(Server(), DiscordNotificationEvent.BackupFailed));
		Assert.False(history.Read(Server()).Unavailable);
		Assert.Equal(2, history.Read(Server()).Entries.Count);
	}

	[Fact]
	public async Task LocalEventsDoNotRequireADiscordDestination()
	{
		GameServer server = Server();
		await Core.Instance.SendDiscordNotification(server, DiscordNotificationEvent.ServerStarting,
			"Secret title", "password=do-not-record-this", System.Drawing.Color.White);
		ServerActivityEntry entry = Assert.Single(ServerActivityHistory.Current.Read(server).Entries);
		Assert.Equal("ServerStarting", entry.Event);
		Assert.False(Directory.Exists(_root));
	}

	[Fact]
	public void AClosingViewerCannotFailTheOperation()
	{
		ServerActivityHistory history = new(_root);
		history.Changed += _ => throw new ObjectDisposedException("closed form");
		Assert.True(history.Record(Server(), "ConfigurationSaved"));
		Assert.Single(history.Read(Server()).Entries);
	}

	[Fact]
	public void OnlyBackupLifecycleChangesRequestABackupStatusScan()
	{
		ServerActivityHistory history = new(null);
		GameServer server = Server();
		List<string> changed = [], backups = [];
		history.Changed += changed.Add;
		history.BackupChanged += backups.Add;
		Assert.True(history.RecordDiagnostic(server, "Probe.Activity.A2S", null, "Enshrouded", 15637));
		Assert.True(history.RecordDiagnostic(server, "Backup.Activity.Error", null, "Disk is full"));
		Assert.True(history.Record(server, DiscordNotificationEvent.ServerOnline));
		Assert.True(history.Record(server, DiscordNotificationEvent.BackupCompleted));
		Assert.True(history.Record(server, DiscordNotificationEvent.BackupFailed));
		Assert.Equal(5, changed.Count);
		Assert.Equal(2, backups.Count);
		Assert.All(backups, key => Assert.Equal(ServerActivityHistory.ServerKey(server), key));
	}

	[Fact]
	public void DetailedDiagnosticsSurviveRestartAndFollowTheSelectedLanguage()
	{
		ServerActivityHistory history = new(_root);
		GameServer server = Server();
		Assert.True(history.Record(server, DiscordNotificationEvent.ServerStarting));
		Assert.True(history.RecordDiagnostic(server, "Probe.Activity.A2S", null, "Enshrouded", 15637));
		Assert.True(history.RecordDiagnostic(server, "Backup.Activity.Error", System.Drawing.Color.Red, "Disk is full"));
		var entries = new ServerActivityHistory(_root).Read(Server("Renamed")).Entries;
		Assert.Equal(3, entries.Count);
		Assert.Contains("15637", entries[1].Description);
		Assert.Contains("Enshrouded", entries[1].Description);
		Assert.False(entries[1].NeedsAttention);
		Assert.True(entries[2].NeedsAttention);
		Assert.Contains("Disk is full", entries[2].Description);
		try
		{
			LocalizationManager.Initialize("en-US");
			string english = entries[1].Description;
			LocalizationManager.Initialize("zh-Hans");
			Assert.NotEqual(english, entries[1].Description);
			Assert.Equal(LocalizationManager.Get("Probe.Activity.A2S", "Enshrouded", 15637), entries[1].Description);
		}
		finally { LocalizationManager.Initialize("en-US"); }
	}

	[Fact]
	public void DiagnosticsRedactLabelledAndUnlabelledCredentialsBeforePersistence()
	{
		GameServer server = Server();
		server.DiscordWebhook = "https://discord.com/api/webhooks/123/never-store-webhook";
		Core.SetServerPasswords(server, new("pass/世界?", "admin\"value", "rcon-value", "authentication-value"));
		string details = "Failure: pass/世界? " + Uri.EscapeDataString("pass/世界?") +
			" admin\"value rcon-value authentication-value password=unconfigured-secret https://discord.com/api/webhooks/123/never-store-webhook";
		ServerActivityHistory history = new(_root);
		Assert.True(history.RecordDiagnostic(server, "Backup.Activity.Error", System.Drawing.Color.Red, details));
		string file = File.ReadAllText(HistoryFile(server));
		string displayed = Assert.Single(new ServerActivityHistory(_root).Read(server).Entries).Description;
		foreach (string secret in new[] { "pass/世界?", Uri.EscapeDataString("pass/世界?"), "admin\"value", "rcon-value", "authentication-value", "unconfigured-secret", "never-store-webhook", Core.ProtectedValuePrefix })
		{
			Assert.DoesNotContain(secret, file);
			Assert.DoesNotContain(secret, displayed);
		}
		Assert.Contains(SecretRedactor.Removed, displayed);
	}

	[Fact]
	public void UnreadableSecretsAndOversizedValuesAreOmittedWithoutBreakingHistory()
	{
		ServerActivityHistory history = new(_root);
		GameServer server = Server();
		server.Password = Core.ProtectedValuePrefix + "damaged";
		Assert.True(history.RecordDiagnostic(server, "Backup.Activity.Error", null, "unlabelled-secret"));
		Assert.Contains(SecretRedactor.Omitted, Assert.Single(history.Read(server).Entries).Description);
		Assert.DoesNotContain("unlabelled-secret", File.ReadAllText(HistoryFile(server)));
		server.Password = "";
		Assert.True(history.RecordDiagnostic(server, "Backup.Activity.Error", null, new string('x', 2000)));
		Assert.Contains(SecretRedactor.Omitted, history.Read(server).Entries.Last().Description);
	}

	[Fact]
	public void ShutdownAndProcessDiagnosticsAreReadableAndMarkForcedStopsForAttention()
	{
		ServerActivityHistory history = new(_root);
		GameServer server = Server();
		foreach ((string key, bool attention, object?[] arguments) in new (string, bool, object?[])[]
		{
			("ServerProcess.Activity.Registered", false, [1, "example (PID 123)"]),
			("ServerStop.Activity.CleanStop", false, ["Example"]),
			("ServerStop.Activity.Forcing", true, ["Example", 1]),
			("ServerStop.Activity.ForcedAndVerified", true, ["Example"]),
			("ServerStop.Activity.LiveProcessRemains", true, ["Example", "123"]),
			("ServerStop.Activity.TaskkillExitCode", true, [1, 123]),
			("Minecraft.Activity.StopThroughRcon", false, []),
			("Minecraft.Activity.ConsoleFallback", true, [])
		})
		{
			Assert.True(history.RecordDiagnostic(server, key, null, arguments));
			ServerActivityEntry entry = new ServerActivityHistory(_root).Read(server).Entries.Last();
			Assert.Equal(LocalizationManager.Get(key, arguments), entry.Description);
			Assert.Equal(attention, entry.NeedsAttention);
		}
	}

	[Fact]
	public void UnknownTemplatesCommandsAndMalformedArgumentsAreNotRecorded()
	{
		ServerActivityHistory history = new(_root);
		Assert.False(history.RecordDiagnostic(Server(), "ServerStart.Activity.Arguments", null, "-password hidden"));
		Assert.False(history.RecordDiagnostic(Server(), "Probe.Activity.Unknown", null, "private"));
		Assert.False(history.RecordDiagnostic(Server(), "PasswordProtection.ProtectFailed"));
		Assert.False(history.RecordDiagnostic(Server(), "Probe.Activity.A2S"));
		Assert.False(history.RecordDiagnostic(Server(), "Probe.Activity.A2S", null, new object[13]));
		Assert.False(Directory.Exists(_root));
	}

	[Fact]
	public void DiagnosticRetentionBoundsUtf8BytesWithoutMakingItsOwnHistoryUnreadable()
	{
		ServerActivityHistory history = new(_root);
		for (int i = 0; i < 20; i++)
			Assert.True(history.RecordDiagnostic(Server(), "Backup.Activity.Error", null,
				Enumerable.Repeat<object>(new string('界', 1000), 12).ToArray()));
		var read = new ServerActivityHistory(_root).Read(Server());
		Assert.False(read.Unavailable);
		Assert.False(read.Recovered);
		Assert.InRange(read.Entries.Count, 1, 19);
		Assert.InRange(new FileInfo(HistoryFile(Server())).Length, 1, 512 * 1024);
	}

	[Fact]
	public async Task ConcurrentAndNestedScopesNeverMixServersOrCaptureGlobalLogs()
	{
		GameServer first = Server("Same name"), second = Server("Same name");
		second.InstallPath += "-second";
		using (ServerDiagnosticScope.Begin(first))
		{
			ServerDiagnosticScope.Record("Backup.Activity.Error", System.Drawing.Color.Red, ["first"]);
			using (ServerDiagnosticScope.Begin(second))
				ServerDiagnosticScope.Record("Backup.Activity.Error", null, ["nested"]);
			await Task.WhenAll(Enumerable.Range(0, 10).Select(index => Task.Run(async () =>
			{
				using var scope = ServerDiagnosticScope.Begin(index % 2 == 0 ? first : second);
				await Task.Yield();
				ServerDiagnosticScope.Record("Backup.Activity.Error", null, [index % 2 == 0 ? "first" : "second"]);
			})));
			ServerDiagnosticScope.Record("Backup.Activity.Error", null, ["restored"]);
		}
		ServerDiagnosticScope.Record("Backup.Activity.Error", null, ["global-not-a-server"]);
		var firstEntries = ServerActivityHistory.Current.Read(first).Entries;
		var secondEntries = ServerActivityHistory.Current.Read(second).Entries;
		Assert.Equal(7, firstEntries.Count);
		Assert.Equal(6, secondEntries.Count);
		Assert.DoesNotContain(firstEntries, entry => entry.Description.Contains("second") || entry.Description.Contains("nested") || entry.Description.Contains("global"));
		Assert.DoesNotContain(secondEntries, entry => entry.Description.Contains("first") || entry.Description.Contains("restored") || entry.Description.Contains("global"));
	}

	[Fact]
	public void DashboardDiagnosticsAreRecordedButRawConsoleOutputIsNot()
	{
		GameServer server = Server();
		EventHandler<ApplicationLogEventArgs> listener = (_, _) => { };
		ApplicationUiService.LogRequested += listener;
		try
		{
			using var scope = ServerDiagnosticScope.Begin(server);
			ApplicationLogService.WriteLocalized("Probe.Activity.A2S", arguments: ["Enshrouded", 15637]);
			ApplicationLogService.Write("raw console command with an unlabelled credential");
			ApplicationLogService.WriteLocalized("ServerStart.Activity.Arguments", arguments: ["private launch data"]);
			Assert.Contains("15637", Assert.Single(ServerActivityHistory.Current.Read(server).Entries).Description);
		}
		finally { ApplicationUiService.LogRequested -= listener; }
	}

	[Fact]
	public async Task BlockedUpdateRecordsItsExplanationForTheCorrectServer()
	{
		GameServer server = Server();
		server.PID = 1; // Fails the stopped-server precondition; no process is inspected or changed.
		EventHandler<ApplicationLogEventArgs> listener = (_, _) => { };
		ApplicationUiService.LogRequested += listener;
		try
		{
			Assert.False(await Core.Instance.UpdateServerAndReport(server, "UPDATE"));
			ServerActivityEntry entry = Assert.Single(ServerActivityHistory.Current.Read(server).Entries);
			Assert.Equal("ServerActions.Operation.RequiresStopped", entry.Diagnostic?.ResourceKey);
			Assert.True(entry.NeedsAttention);
			Assert.Equal(LocalizationManager.Get("ServerActions.Operation.RequiresStopped"), entry.Description);
			Assert.False(Directory.Exists(_root));
		}
		finally { ApplicationUiService.LogRequested -= listener; }
	}

	[Fact]
	public void CorruptDiagnosticDoesNotHideValidOlderEntries()
	{
		ServerActivityHistory history = new(_root);
		Assert.True(history.Record(Server(), DiscordNotificationEvent.ServerOnline));
		File.AppendAllText(HistoryFile(Server()), JsonSerializer.Serialize(new
		{
			Version = 2, OccurredUtc = DateTimeOffset.UtcNow, Event = "Diagnostic",
			Diagnostic = new { ResourceKey = "Probe.Activity.A2S", Arguments = Array.Empty<string>(), Attention = false }
		}));
		var read = history.Read(Server());
		Assert.True(read.Recovered);
		Assert.False(read.Unavailable);
		Assert.Equal("ServerOnline", Assert.Single(read.Entries).Event);
	}

	public void Dispose()
	{
		if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
	}
}
