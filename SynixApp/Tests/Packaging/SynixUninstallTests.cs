using System.Diagnostics;
using System.Drawing;
using System.Text.Json;
using System.Windows.Forms;
using Synix_Control_Panel.SynixApp.Localization;
using Synix_Control_Panel.SynixApp.UI.Installation;
using Synix_Control_Panel.SynixEngine;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class SynixUninstallTests
{
	[Theory]
	[InlineData("2", false)]
	[InlineData("3", true)]
	[InlineData("4", true)]
	[InlineData("5", true)]
	[InlineData("purge", false)]
	[InlineData("", false)]
	public void OnlyInteractiveInstallerModesOfferDataRemoval(string level, bool expected) =>
		Assert.Equal(expected, SynixUninstallCommand.IsInteractive([SynixUninstallCommand.Argument, level]));

	[Fact]
	public void DeletionCannotBeAuthorizedByExtraCommandLineArguments()
	{
		Assert.False(SynixUninstallCommand.IsInteractive([SynixUninstallCommand.Argument, "5", "--delete-all"]));
		Assert.Equal(1, SynixUninstallCommand.Run([SynixUninstallCommand.Argument, "2", "--delete-all"]));
	}

	[Fact]
	public void PreviewUsesRegisteredInstallPathsAndStableBackupIdentitiesNotSharedRoots()
	{
		using Fixture fixture = new();
		fixture.WriteServers();
		fixture.CreateBackups();
		SynixUninstallPlan plan = SynixUninstallPlan.Create(fixture.Paths, deleteBackups: true);
		Assert.Contains(fixture.Server, plan.Folders);
		Assert.Contains(Path.Combine(fixture.Paths.CustomBackups!, "Example_Game", "stable_identity"), plan.Folders);
		Assert.Contains(Path.Combine(fixture.Paths.Root, "BackupGames", "Example_Game", "stable_identity"), plan.Folders);
		Assert.DoesNotContain(fixture.Paths.Root, plan.Folders);
		Assert.DoesNotContain(Path.Combine(fixture.Paths.Root, "Games"), plan.Folders);
		Assert.DoesNotContain(fixture.Paths.CustomBackups!, plan.Folders);
		Assert.Equal(fixture.Paths.Data, plan.Folders[^1]);
		Assert.True(File.Exists(fixture.Keep));
		Assert.True(File.Exists(fixture.ServerFile));
	}

	[Fact]
	public void NoConfirmationDeletesNothing()
	{
		using Fixture fixture = new();
		fixture.WriteServers();
		SynixUninstallPlan plan = SynixUninstallPlan.Create(fixture.Paths);
		Assert.Throws<InvalidOperationException>(() => plan.Execute(false, () => Assert.Fail("Must not run")));
		fixture.AssertIntact();
	}

	[Fact]
	public void FullRemovalDeletesOnlyReviewedServerAndDataFolders()
	{
		using Fixture fixture = new();
		fixture.WriteServers();
		SynixUninstallPlan plan = SynixUninstallPlan.Create(fixture.Paths);
		foreach (string folder in plan.Folders)
		{
			Directory.CreateDirectory(folder);
			File.WriteAllText(Path.Combine(folder, "owned.txt"), "owned");
		}
		int stoppedChecks = 0;
		plan.Execute(true, () => stoppedChecks++);
		Assert.Equal(2, stoppedChecks);
		Assert.All(plan.Folders, folder => Assert.False(Directory.Exists(folder)));
		Assert.True(File.Exists(fixture.Keep));
		Assert.True(File.Exists(fixture.AppFile));
		Assert.True(Directory.Exists(fixture.Paths.Root));
	}

	[Fact]
	public void ChangedRegistryBlocksDeletionUntilANewPreviewIsConfirmed()
	{
		using Fixture fixture = new();
		fixture.WriteServers();
		SynixUninstallPlan plan = SynixUninstallPlan.Create(fixture.Paths);
		File.AppendAllText(fixture.Paths.ServersFile, " ");
		Assert.Throws<InvalidOperationException>(() => plan.Execute(true, () => { }));
		fixture.AssertIntact();
	}

	[Fact]
	public void ARunningServerOrFailedPreflightDeletesNothing()
	{
		using Fixture fixture = new();
		fixture.WriteServers();
		SynixUninstallPlan plan = SynixUninstallPlan.Create(fixture.Paths);
		Assert.Throws<InvalidOperationException>(() => plan.Execute(true, () => throw new InvalidOperationException("running")));
		fixture.AssertIntact();
	}

	[Fact]
	public void ADeletionFailureKeepsTheRegistryForRecovery()
	{
		using Fixture fixture = new();
		fixture.WriteServers();
		SynixUninstallPlan plan = SynixUninstallPlan.Create(fixture.Paths);
		using FileStream locked = File.Open(fixture.ServerFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
		Assert.Throws<IOException>(() => plan.Execute(true, () => { }));
		Assert.True(File.Exists(fixture.Paths.ServersFile));
		Assert.True(File.Exists(fixture.Keep));
	}

	[Theory]
	[InlineData("drive")]
	[InlineData("profile")]
	[InlineData("app")]
	[InlineData("synix-root")]
	[InlineData("relative")]
	[InlineData("network")]
	public void UnsafeServerLocationsDisableFullRemoval(string kind)
	{
		using Fixture fixture = new();
		string path = kind switch
		{
			"drive" => Path.GetPathRoot(fixture.Root)!,
			"profile" => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
			"app" => fixture.Paths.Application,
			"synix-root" => fixture.Paths.Root,
			"network" => @"\\server\share\game",
			_ => "relative/server"
		};
		fixture.WriteServers(path);
		Assert.ThrowsAny<Exception>(() => SynixUninstallPlan.Create(fixture.Paths));
		fixture.AssertIntact();
	}

	[Theory]
	[InlineData("null")]
	[InlineData("{}")]
	[InlineData("[{}]")]
	[InlineData("[")]
	public void UnreadableOrIncompleteRegistryNeverBecomesAnEmptyDeletionPlan(string json)
	{
		using Fixture fixture = new();
		Directory.CreateDirectory(fixture.Paths.Data);
		File.WriteAllText(fixture.Paths.ServersFile, json);
		Assert.ThrowsAny<Exception>(() => SynixUninstallPlan.Create(fixture.Paths));
		fixture.AssertIntact();
	}

	[Fact]
	public void OverlappingServerFoldersAreRejected()
	{
		using Fixture fixture = new();
		Directory.CreateDirectory(fixture.Paths.Data);
		File.WriteAllText(fixture.Paths.ServersFile, JsonSerializer.Serialize(new[]
		{
			new { InstallPath = fixture.Server, Game = "Example", ServerName = "one" },
			new { InstallPath = Path.Combine(fixture.Server, "two"), Game = "Example", ServerName = "two" }
		}));
		Assert.Throws<InvalidOperationException>(() => SynixUninstallPlan.Create(fixture.Paths));
		fixture.AssertIntact();
	}

	[Fact]
	public void NestedJunctionIsRejectedBeforeAnyFolderIsDeleted()
	{
		using Fixture fixture = new();
		fixture.WriteServers();
		SynixUninstallPlan plan = SynixUninstallPlan.Create(fixture.Paths);
		string link = Path.Combine(fixture.Server, "linked");
		ProcessStartInfo info = new("cmd.exe") { UseShellExecute = false, CreateNoWindow = true };
		foreach (string argument in new[] { "/d", "/c", "mklink", "/J", link, Path.GetDirectoryName(fixture.Keep)! }) info.ArgumentList.Add(argument);
		using Process process = Process.Start(info)!;
		process.WaitForExit();
		Assert.Equal(0, process.ExitCode);
		try
		{
			Assert.ThrowsAny<Exception>(() => plan.Execute(true, () => { }));
			fixture.AssertIntact();
		}
		finally { Directory.Delete(link, recursive: false); }
	}

	[Fact]
	public void OpeningTheDialogDefaultsToAppOnlyAndDoesNotEvenReadServerData()
	{
		WorkflowUiTest.Run(() =>
		{
			using SynixUninstallDialog dialog = new(_ => throw new Exception("Must not read data"),
				_ => throw new Exception("Must not delete"), () => throw new Exception("Must not modify startup"));
			Assert.True(((RadioButton)dialog.Controls.Find("keepServerData", true).Single()).Checked);
			Assert.False(((CheckBox)dialog.Controls.Find("confirmDataRemoval", true).Single()).Checked);
			Assert.False(dialog.Controls.Find("confirmDataRemoval", true).Single().Enabled);
			Assert.True(dialog.Controls.Find("uninstallButton", true).Single().Enabled);
			Assert.Null(dialog.AcceptButton);
		});
	}

	[Fact]
	public void SwitchingBackToAppOnlyClearsDataConsent()
	{
		using Fixture fixture = new();
		fixture.WriteServers();
		WorkflowUiTest.Run(() =>
		{
			using SynixUninstallDialog dialog = new(deleteBackups => SynixUninstallPlan.Create(fixture.Paths, deleteBackups), _ => Task.CompletedTask, () => { });
			RadioButton all = (RadioButton)dialog.Controls.Find("removeServerData", true).Single();
			RadioButton keep = (RadioButton)dialog.Controls.Find("keepServerData", true).Single();
			CheckBox confirm = (CheckBox)dialog.Controls.Find("confirmDataRemoval", true).Single();
			all.Checked = true;
			WorkflowUiTest.WaitUntil(() => confirm.Enabled);
			Assert.False(dialog.Controls.Find("uninstallButton", true).Single().Enabled);
			Assert.Contains(fixture.Server, dialog.Controls.Find("removalPaths", true).Single().Text);
			confirm.Checked = true;
			Assert.True(dialog.Controls.Find("uninstallButton", true).Single().Enabled);
			keep.Checked = true;
			Assert.False(confirm.Checked);
			Assert.False(confirm.Enabled);
			Assert.Equal(string.Empty, dialog.Controls.Find("removalPaths", true).Single().Text);
		});
		fixture.AssertIntact();
	}

	[Fact]
	public void UninstallChoicesAndConsentFitEveryAppLanguage()
	{
		using Fixture fixture = new();
		fixture.WriteServers();
		fixture.CreateBackups();
		foreach (var language in LocalizationManager.SupportedLanguages)
		{
			WorkflowUiTest.Run(() =>
			{
				try
				{
					LocalizationManager.Initialize(language.Code);
					using SynixUninstallDialog dialog = new(deleteBackups => SynixUninstallPlan.Create(fixture.Paths, deleteBackups),
						_ => throw new Exception("Rendering must not delete"), () => { })
					{
						ShowInTaskbar = false, StartPosition = FormStartPosition.Manual,
						Location = new Point(-32000, -32000)
					};
					dialog.Size = dialog.MinimumSize;
					dialog.Show();
					Assert.Contains("SynixData\\servers.json", LocalizationManager.Get("Uninstall.Keep"));
					CheckLayout();
					Render("uninstall-app-only");
					((RadioButton)dialog.Controls.Find("removeServerData", true).Single()).Checked = true;
					CheckBox confirm = (CheckBox)dialog.Controls.Find("confirmDataRemoval", true).Single();
					WorkflowUiTest.WaitUntil(() => confirm.Enabled);
					CheckLayout();
					Render("uninstall");

					void CheckLayout()
					{
						// Layout is synchronous. Do not start/stop a temporary message
						// loop here before the next async choice change: WinForms can
						// tear down its UI context and marshal handles to a pool thread.
						dialog.PerformLayout();
						TableLayoutPanel layout = (TableLayoutPanel)dialog.Controls[0];
						foreach (Control control in layout.Controls)
						{
							if (!control.Visible) continue;
							Assert.True(control.Bottom <= layout.ClientSize.Height, language.Code + ": bottom clipped");
							Assert.True(control.Right <= layout.ClientSize.Width, language.Code + ": right clipped");
							if (control is not (Label or CheckBox or RadioButton)) continue;
							Size text = TextRenderer.MeasureText(control.Text, control.Font,
								new Size(control.Width - (control is Label ? 0 : 22), int.MaxValue),
								TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
							Assert.True(text.Height <= control.Height, language.Code + ": text clipped in " + control.Name);
						}
					}
					void Render(string prefix)
					{
						if (Environment.GetEnvironmentVariable("SYNIX_RENDER_UNINSTALL") != "1") return;
						using Bitmap image = new(dialog.Width, dialog.Height);
						dialog.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
						image.Save(Path.Combine(AppContext.BaseDirectory, $"{prefix}-{language.Code}.png"));
					}
				}
				finally { LocalizationManager.Initialize("en-US"); }
			});
		}
		fixture.AssertIntact();
	}

	[Fact]
	public void AppOnlyCleansAppCacheAllPreferenceVersionsAndExactTempPatternsButKeepsAllServerData()
	{
		using Fixture fixture = new();
		fixture.WriteServers();
		fixture.CreateBackups();
		string data = Fixture.Touch(Path.Combine(fixture.Paths.Data, "settings.json"));
		string steam = Fixture.Touch(Path.Combine(fixture.Paths.Root, "SteamCMD", "steamcmd.exe"));
		string runtimes = Fixture.Touch(Path.Combine(fixture.Paths.Data, "Runtimes", "java.exe"));
		string otherRootData = Fixture.Touch(Path.Combine(fixture.Paths.Root, "other-stuff", "keep.txt"));
		string cache = Fixture.Touch(Path.Combine(fixture.Paths.LocalData, "Updater", "old-download.msi"));
		string session = Fixture.Touch(Path.Combine(fixture.Paths.LocalData, "active-session.marker"));
		string restore = Fixture.Touch(Path.Combine(fixture.Paths.LocalData, "RestoreOperations", "operation.json"));
		string currentSettings = Fixture.Touch(Path.Combine(fixture.Paths.UserSettings!, "user.config"), "<configuration/>");
		string identity = Path.GetDirectoryName(fixture.Paths.UserSettings!)!;
		string oldSettings = Fixture.Touch(Path.Combine(identity, "1.0.24.0", "user.config"), "<configuration/>");
		string unknown = Fixture.Touch(Path.Combine(identity, "notes", "keep.txt"));
		string otherApp = Fixture.Touch(Path.Combine(Path.GetDirectoryName(identity)!, "OtherApp", "1.0.0.0", "user.config"));
		string tempFolder = Fixture.Touch(Path.Combine(fixture.Paths.Temp!, "SynixAddOn-" + Guid.NewGuid().ToString("N"), "copy.zip"));
		string tempFile = Fixture.Touch(Path.Combine(fixture.Paths.Temp!, "synix-import-" + Guid.NewGuid().ToString("N") + ".zip.tmp"));
		string similarName = Fixture.Touch(Path.Combine(fixture.Paths.Temp!, "SynixAddOn-not-a-guid", "keep.zip"));
		string oxide = Path.Combine(fixture.Paths.Temp!, "Synix", "Oxide", Guid.NewGuid().ToString("N"));
		string oxideArchive = Fixture.Touch(Path.Combine(oxide, "Oxide.Rust.zip"));
		string oxideRollback = Fixture.Touch(Path.Combine(oxide, "rollback", "server.dll"));
		SynixUninstallPlan plan = SynixUninstallPlan.Create(fixture.Paths, deleteBackups: true, removeServers: false);
		Assert.False(plan.RemovesServers);
		Assert.False(plan.DeletesBackups); // Backup deletion cannot be smuggled into app-only mode.
		plan.Execute(true, () => { });
		foreach (string removed in new[] { cache, session, currentSettings, oldSettings, tempFolder, tempFile, oxideArchive })
			Assert.False(File.Exists(removed), removed);
		foreach (string kept in new[] { fixture.DefaultBackup, fixture.CustomBackup, data, steam, runtimes, otherRootData,
			restore, unknown, otherApp, similarName, oxideRollback, fixture.Paths.ServersFile })
			Assert.True(File.Exists(kept), kept);
		fixture.AssertIntact();
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void AppOnlyPreservesTheCompleteServerRegistryForReinstall(bool chooseDifferentAppFolder)
	{
		using Fixture fixture = new();
		string customServer = Path.Combine(fixture.Root, "custom-servers", "second-server");
		GameServer[] originalServers =
		[
			new()
			{
				Game = "Minecraft", ServerName = "友達のサーバー", ConfigurationIdentity = "original-world",
				InstallPath = fixture.Server, Port = 25565, QueryPort = 25566, WorldName = "KeepThisWorld",
				IsFirstBoot = false, IsScheduledRestartEnabled = true, RestartTime = "05:30",
				RestartDays = [true, false, true, false, true, false, true], BackupOnStart = true,
				PreserveImportedConfiguration = true, DataSchemaVersion = ServerDataMigrator.CurrentVersion
			},
			new()
			{
				Game = "Rust", ServerName = "Custom server", ConfigurationIdentity = "second-stable-identity",
				InstallPath = customServer, IsDefaultPath = false, Port = 28015, QueryPort = 28017,
				EnableRcon = true, RconPort = 28016, IsFirstBoot = false,
				DataSchemaVersion = ServerDataMigrator.CurrentVersion
			}
		];
		SynixServerPasswords passwords = new("test-player-password", "test-admin-password", "test-rcon-password");
		foreach (GameServer server in originalServers) Core.SetServerPasswords(server, passwords);
		string originalJson = Core.SerializeServersForStorage(originalServers);
		Fixture.Touch(fixture.Paths.ServersFile, originalJson);
		Fixture.Touch(fixture.Paths.ServersFile + ".before-data-v4.bak", originalJson);
		Fixture.Touch(Path.Combine(fixture.Paths.Data, "custom-server-settings.json"), "{\"keep\":true}");
		Fixture.Touch(Path.Combine(fixture.Paths.Data, "Runtimes", "java.exe"));
		Fixture.Touch(Path.Combine(fixture.Paths.Data, "AddOns", "import-record.json"), "{\"keep\":true}");
		Fixture.Touch(Path.Combine(fixture.Server, "server.properties"), "motd=Keep this configuration");
		Fixture.Touch(Path.Combine(customServer, "world.save"));
		Fixture.Touch(Path.Combine(customServer, "server.cfg"), "hostname \"Custom server\"");
		fixture.CreateBackups();
		string preferences = Fixture.Touch(Path.Combine(fixture.Paths.UserSettings!, "user.config"), "<configuration/>");
		string appCache = Fixture.Touch(Path.Combine(fixture.Paths.LocalData, "Updater", "old-download.msi"));
		Dictionary<string, byte[]> preservedFiles = Directory.GetFiles(fixture.Paths.Root, "*", SearchOption.AllDirectories)
			.Concat(Directory.GetFiles(customServer, "*", SearchOption.AllDirectories))
			.Append(fixture.CustomBackup).ToDictionary(path => path, File.ReadAllBytes);

		SynixUninstallPlan plan = SynixUninstallPlan.Create(fixture.Paths, removeServers: false);
		Assert.Contains(fixture.Paths.Root, plan.KeptFolders);
		Assert.Contains(customServer, plan.KeptFolders);
		plan.Execute(true, () => { });
		Assert.False(File.Exists(preferences));
		Assert.False(File.Exists(appCache));
		foreach (var (path, bytes) in preservedFiles) Assert.Equal(bytes, File.ReadAllBytes(path));

		// Simulate only the MSI-owned executable being removed/replaced, in this
		// fixture. Reinstalling uses the same server data root, not the app folder.
		File.Delete(fixture.AppFile);
		SynixUninstallPaths reinstalled = fixture.Paths with
		{
			Application = chooseDifferentAppFolder ? Path.Combine(fixture.Root, "new-app-location") : fixture.Paths.Application,
			UserSettings = Path.Combine(fixture.Root, "fresh-preferences", "new-app-identity", "1.0.26.0")
		};
		Fixture.Touch(Path.Combine(reinstalled.Application, "Synix.exe"));
		Assert.False(Directory.Exists(reinstalled.UserSettings));
		Assert.Equal(fixture.Paths.ServersFile, reinstalled.ServersFile);
		// The same production deserializer used by FileHandler.LoadServers on startup.
		List<GameServer> reloaded = Core.DeserializeServersAndMigrate(
			File.ReadAllText(reinstalled.ServersFile), out ServerDataMigrationSummary migration);
		Assert.False(migration.Changed);
		Assert.Equal(originalServers.Length, reloaded.Count);
		Assert.Equal(originalJson, Core.SerializeServersForStorage(reloaded));
		foreach (GameServer server in reloaded)
		{
			Assert.True(Directory.Exists(server.InstallPath));
			Assert.Equal(passwords, Core.RevealServerPasswords(server));
			Assert.False(server.IsFirstBoot);
		}
		foreach (var (path, bytes) in preservedFiles) Assert.Equal(bytes, File.ReadAllBytes(path));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void ServerRemovalHasAnIndependentDefaultOffBackupChoice(bool deleteBackups)
	{
		using Fixture fixture = new();
		fixture.WriteServers();
		fixture.CreateBackups();
		string otherBackup = Fixture.Touch(Path.Combine(fixture.Paths.CustomBackups!, "Example_Game", "other_server", "backup.zip"));
		string recovery = Fixture.Touch(Path.Combine(fixture.Paths.Data, "AddOns", "mod", "PreviousFiles", "file.dll"));
		var restored = fixture.CreateRestoreJournal(fixture.Server);
		string journal = restored.Journal;
		SynixUninstallPlan plan = SynixUninstallPlan.Create(fixture.Paths, deleteBackups);
		Assert.Equal(deleteBackups, plan.DeletesBackups);
		Assert.Contains(Path.GetDirectoryName(fixture.DefaultBackup)!, plan.BackupFolders);
		Assert.Contains(Path.GetDirectoryName(fixture.CustomBackup)!, plan.BackupFolders);
		plan.Execute(true, () => { });
		Assert.False(File.Exists(fixture.ServerFile));
		foreach (string backup in new[] { fixture.DefaultBackup, fixture.CustomBackup, recovery, journal, restored.RollbackFile, restored.PreparedFile })
			Assert.Equal(!deleteBackups, File.Exists(backup));
		Assert.True(File.Exists(otherBackup));
		Assert.True(Directory.Exists(fixture.Paths.CustomBackups));
		Assert.True(Directory.Exists(fixture.Paths.Root));
		Assert.True(File.Exists(fixture.Keep));
		Assert.True(File.Exists(fixture.AppFile));
	}

	[Theory]
	[InlineData("server", false)]
	[InlineData("server", true)]
	[InlineData("data", false)]
	[InlineData("data", true)]
	[InlineData("cache", false)]
	[InlineData("cache", true)]
	public void NestedBackupRootsAreExcludedFromParentDeletionAndOnlySelectedServerBackupsCanBeRemoved(string location, bool deleteBackups)
	{
		using Fixture fixture = new();
		fixture.WriteServers();
		string parent = location switch { "server" => fixture.Server, "data" => fixture.Paths.Data, _ => fixture.Paths.LocalData };
		SynixUninstallPaths paths = fixture.Paths with { CustomBackups = Path.Combine(parent, "nested-backups") };
		string backup = Fixture.Touch(Path.Combine(paths.CustomBackups!, "Example_Game", "stable_identity", "backup.zip"));
		string unrelated = Fixture.Touch(Path.Combine(paths.CustomBackups!, "different_game", "different_server", "backup.zip"));
		SynixUninstallPlan plan = SynixUninstallPlan.Create(paths, deleteBackups);
		plan.Execute(true, () => { });
		Assert.Equal(!deleteBackups, File.Exists(backup));
		Assert.True(File.Exists(unrelated));
		Assert.False(File.Exists(fixture.ServerFile));
		Assert.True(Directory.Exists(parent)); // Preserved backups keep their parents alive.
	}

	[Fact]
	public void AppOnlyProtectsACustomServerInsideTheAppCacheFolder()
	{
		using Fixture fixture = new();
		string server = Path.Combine(fixture.Paths.LocalData, "my-server");
		string world = Fixture.Touch(Path.Combine(server, "world.save"));
		string cache = Fixture.Touch(Path.Combine(fixture.Paths.LocalData, "cache.txt"));
		fixture.WriteServers(server);
		SynixUninstallPlan.Create(fixture.Paths, removeServers: false).Execute(true, () => { });
		Assert.True(File.Exists(world));
		Assert.False(File.Exists(cache));
		Assert.True(File.Exists(fixture.Paths.ServersFile));
	}

	[Fact]
	public void SelectingOneBackupFolderDoesNotSweepANestedSharedBackupRoot()
	{
		using Fixture fixture = new();
		fixture.WriteServers();
		fixture.CreateBackups();
		SynixUninstallPaths paths = fixture.Paths with
		{
			CustomBackups = Path.Combine(Path.GetDirectoryName(fixture.DefaultBackup)!, "nested-shared-backups")
		};
		string selected = Fixture.Touch(Path.Combine(paths.CustomBackups, "Example_Game", "stable_identity", "backup.zip"));
		string unrelated = Fixture.Touch(Path.Combine(paths.CustomBackups, "Other_Game", "other_server", "backup.zip"));
		SynixUninstallPlan.Create(paths, deleteBackups: true).Execute(true, () => { });
		Assert.False(File.Exists(selected));
		Assert.False(File.Exists(fixture.DefaultBackup));
		Assert.True(File.Exists(unrelated));
		Assert.True(Directory.Exists(paths.CustomBackups));
	}

	[Fact]
	public void HistoricalCustomBackupLocationsAreReadFromOlderPreferencesAndPreserved()
	{
		using Fixture fixture = new();
		fixture.WriteServers();
		string oldRoot = Path.Combine(fixture.Paths.LocalData, "old-backups");
		string backup = Fixture.Touch(Path.Combine(oldRoot, "Example_Game", "stable_identity", "backup.zip"));
		string config = Path.Combine(Path.GetDirectoryName(fixture.Paths.UserSettings!)!, "1.0.24.0", "user.config");
		Fixture.Touch(config, new System.Xml.Linq.XDocument(new System.Xml.Linq.XElement("configuration",
			new System.Xml.Linq.XElement("userSettings", new System.Xml.Linq.XElement("Synix_Control_Panel.Properties.Settings",
				new System.Xml.Linq.XElement("setting", new System.Xml.Linq.XAttribute("name", "CustomBackupPath"),
					new System.Xml.Linq.XElement("value", oldRoot)))))).ToString());
		SynixUninstallPlan plan = SynixUninstallPlan.Create(fixture.Paths);
		Assert.Contains(Path.GetDirectoryName(backup)!, plan.BackupFolders);
		plan.Execute(true, () => { });
		Assert.True(File.Exists(backup));
		Assert.False(File.Exists(config));
	}

	[Fact]
	public void BackupsAppearingAfterPreviewRequireNewConsentBeforeAnythingIsDeleted()
	{
		using Fixture fixture = new();
		fixture.WriteServers();
		SynixUninstallPlan plan = SynixUninstallPlan.Create(fixture.Paths, deleteBackups: true);
		Assert.Empty(plan.BackupFolders);
		fixture.CreateBackups();
		Assert.Throws<InvalidOperationException>(() => plan.Execute(true, () => { }));
		Assert.True(File.Exists(fixture.DefaultBackup));
		Assert.True(File.Exists(fixture.CustomBackup));
		fixture.AssertIntact();
	}

	[Fact]
	public void AppOnlyAlsoProtectsServerRestoreCopiesBesideACustomServerInAppData()
	{
		using Fixture fixture = new();
		string server = Path.Combine(fixture.Paths.LocalData, "custom-server");
		Fixture.Touch(Path.Combine(server, "world.save"));
		fixture.WriteServers(server);
		var restored = fixture.CreateRestoreJournal(server);
		SynixUninstallPlan.Create(fixture.Paths, removeServers: false).Execute(true, () => { });
		Assert.True(File.Exists(restored.Journal));
		Assert.True(File.Exists(restored.RollbackFile));
		Assert.True(File.Exists(restored.PreparedFile));
	}

	[Fact]
	public void FullRemovalDoesNotEraseAnUnregisteredServersRecoveryJournalOrCopies()
	{
		using Fixture fixture = new();
		fixture.WriteServers();
		string server = Path.Combine(fixture.Paths.LocalData, "other-server");
		string world = Fixture.Touch(Path.Combine(server, "world.save"));
		var restored = fixture.CreateRestoreJournal(server);
		SynixUninstallPlan.Create(fixture.Paths, deleteBackups: true).Execute(true, () => { });
		Assert.True(File.Exists(restored.Journal));
		Assert.True(File.Exists(restored.RollbackFile));
		Assert.True(File.Exists(restored.PreparedFile));
		Assert.True(File.Exists(world));
	}

	[Fact]
	public void AnUnsafeRestoreJournalNeverAuthorizesDeletionOutsideItsServer()
	{
		using Fixture fixture = new();
		fixture.WriteServers();
		var restored = fixture.CreateRestoreJournal(fixture.Server);
		var json = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(restored.Journal))!;
		json["RollbackPath"] = Path.GetDirectoryName(fixture.Keep)!;
		File.WriteAllText(restored.Journal, json.ToJsonString());
		Assert.ThrowsAny<Exception>(() => SynixUninstallPlan.Create(fixture.Paths, deleteBackups: true));
		fixture.AssertIntact();
	}

	[Fact]
	public void ChangingARecoveryJournalAfterPreviewBlocksDeletion()
	{
		using Fixture fixture = new();
		fixture.WriteServers();
		var restored = fixture.CreateRestoreJournal(fixture.Server);
		SynixUninstallPlan plan = SynixUninstallPlan.Create(fixture.Paths, deleteBackups: true);
		File.AppendAllText(restored.Journal, " ");
		Assert.Throws<InvalidOperationException>(() => plan.Execute(true, () => { }));
		Assert.True(File.Exists(restored.RollbackFile));
		fixture.AssertIntact();
	}

	[Fact]
	public void AnAppFolderReplacedWithAFileBlocksAllDeletion()
	{
		using Fixture fixture = new();
		fixture.WriteServers();
		SynixUninstallPlan plan = SynixUninstallPlan.Create(fixture.Paths);
		Fixture.Touch(fixture.Paths.LocalData);
		Assert.ThrowsAny<Exception>(() => plan.Execute(true, () => { }));
		fixture.AssertIntact();
	}

	[Theory]
	[InlineData("SynixAddOn-00000000000000000000000000000000", true)]
	[InlineData("SynixAddOn-0123456789abcdef0123456789abcdef", true)]
	[InlineData("SynixAddOn-01234567-89ab-cdef-0123-456789abcdef", false)]
	[InlineData("SynixAddOn-0123456789abcdef0123456789abcdef-old", false)]
	[InlineData("Other-SynixAddOn-0123456789abcdef0123456789abcdef", false)]
	[InlineData("SynixAddOn-backup", false)]
	public void TempOwnershipRequiresAnExactKnownPrefixAndOperationId(string name, bool matches) =>
		Assert.Equal(matches, SynixAppCleanup.IsOperationName(name, "SynixAddOn-", ""));

	[Fact]
	public void OnlyThisCopyOwnsItsStartupCommand()
	{
		using Fixture fixture = new();
		string command = BackgroundServiceManager.BuildLaunchCommand(fixture.AppFile);
		Assert.True(SynixUninstallCommand.IsOwnedStartupCommand(command, fixture.AppFile));
		Assert.False(SynixUninstallCommand.IsOwnedStartupCommand(command, Path.Combine(fixture.Root, "portable", "Synix.exe")));
		Assert.False(SynixUninstallCommand.IsOwnedStartupCommand(command + " --other", fixture.AppFile));
		Assert.False(SynixUninstallCommand.IsOwnedStartupCommand(null, fixture.AppFile));
	}

	[Fact]
	public void BackupChoiceIsSeparateDefaultsOffAndClearsConsentWheneverChanged()
	{
		using Fixture fixture = new();
		fixture.WriteServers();
		fixture.CreateBackups();
		WorkflowUiTest.Run(() =>
		{
			using SynixUninstallDialog dialog = new(deleteBackups => SynixUninstallPlan.Create(fixture.Paths, deleteBackups),
				_ => throw new Exception("Preview must not delete"), () => throw new Exception("Preview must not uninstall"));
			CheckBox backups = (CheckBox)dialog.Controls.Find("deleteBackups", true).Single();
			CheckBox confirm = (CheckBox)dialog.Controls.Find("confirmDataRemoval", true).Single();
			Assert.False(backups.Checked);
			Assert.False(backups.Enabled);
			((RadioButton)dialog.Controls.Find("removeServerData", true).Single()).Checked = true;
			WorkflowUiTest.WaitUntil(() => confirm.Enabled);
			Assert.False(backups.Checked);
			Assert.True(backups.Enabled);
			confirm.Checked = true;
			backups.Checked = true;
			Assert.False(confirm.Checked);
			Assert.False(dialog.Controls.Find("uninstallButton", true).Single().Enabled);
			WorkflowUiTest.WaitUntil(() => confirm.Enabled);
			Assert.Contains(LocalizationManager.Get("Uninstall.BackupsDelete"), dialog.Controls.Find("removalPaths", true).Single().Text);
			confirm.Checked = true;
			((RadioButton)dialog.Controls.Find("keepServerData", true).Single()).Checked = true;
			Assert.False(backups.Checked);
			Assert.False(backups.Enabled);
			Assert.False(confirm.Checked);
		});
		fixture.AssertIntact();
	}

	[Fact]
	public void NoBackupFilesMeansNoBackupDeletionOption()
	{
		using Fixture fixture = new();
		fixture.WriteServers();
		WorkflowUiTest.Run(() =>
		{
			using SynixUninstallDialog dialog = new(deleteBackups => SynixUninstallPlan.Create(fixture.Paths, deleteBackups), _ => Task.CompletedTask, () => { });
			((RadioButton)dialog.Controls.Find("removeServerData", true).Single()).Checked = true;
			WorkflowUiTest.WaitUntil(() => dialog.Controls.Find("confirmDataRemoval", true).Single().Enabled);
			CheckBox backups = (CheckBox)dialog.Controls.Find("deleteBackups", true).Single();
			Assert.False(backups.Checked);
			Assert.False(backups.Enabled);
			Assert.False(backups.Visible);
			Assert.Contains(LocalizationManager.Get("Uninstall.NoBackups"), dialog.Controls.Find("removalPaths", true).Single().Text);
		});
	}

	private sealed class Fixture : IDisposable
	{
		internal string Root { get; } = Path.Combine(Path.GetTempPath(), "Synix-Uninstall-Tests-" + Guid.NewGuid().ToString("N"));
		internal SynixUninstallPaths Paths { get; }
		internal string Server { get; }
		internal string ServerFile => Path.Combine(Server, "world.save");
		internal string Keep => Path.Combine(Root, "unregistered", "keep.txt");
		internal string AppFile => Path.Combine(Paths.Application, "Synix.exe");
		internal string DefaultBackup => Path.Combine(Paths.Root, "BackupGames", "Example_Game", "stable_identity", "backup.zip");
		internal string CustomBackup => Path.Combine(Paths.CustomBackups!, "Example_Game", "stable_identity", "backup.zip");
		internal Fixture()
		{
			Paths = new(Path.Combine(Root, "managed"), Path.Combine(Root, "application"),
				Path.Combine(Root, "local-data"), Path.Combine(Root, "settings", "Synix-identity", "1.0.26.0"),
				Path.Combine(Root, "custom-backups"), Path.Combine(Root, "temp"));
			Server = Path.Combine(Paths.Root, "Games", "Example_Game", "server");
			foreach (string path in new[] { ServerFile, Keep, AppFile })
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path)!);
				File.WriteAllText(path, "preserve unless explicitly selected");
			}
		}
		internal void CreateBackups()
		{
			Touch(DefaultBackup);
			Touch(CustomBackup);
		}
		internal (string Journal, string RollbackFile, string PreparedFile) CreateRestoreJournal(string server)
		{
			string id = Guid.NewGuid().ToString("N");
			string rollback = server + ".synix-restore-rollback-" + id;
			string operation = Path.Combine(Path.GetDirectoryName(server)!, ".synix-restore-test-" + id);
			string journal = Path.Combine(Paths.LocalData, "RestoreOperations", id + ".json");
			Touch(journal, JsonSerializer.Serialize(new { InstallPath = server, OperationRoot = operation, RollbackPath = rollback, Phase = "OriginalPreserved" }));
			return (journal, Touch(Path.Combine(rollback, "world.save")), Touch(Path.Combine(operation, "prepared", "world.save")));
		}
		internal static string Touch(string path, string content = "keep unless explicitly selected")
		{
			Directory.CreateDirectory(Path.GetDirectoryName(path)!);
			File.WriteAllText(path, content);
			return path;
		}
		internal void WriteServers(string? installPath = null)
		{
			Directory.CreateDirectory(Paths.Data);
			File.WriteAllText(Paths.ServersFile, JsonSerializer.Serialize(new[] { new
			{
				Game = "Example Game", ServerName = "changed display name", ConfigurationIdentity = "stable identity",
				InstallPath = installPath ?? Server, PID = (int?)null
			} }));
		}
		internal void AssertIntact()
		{
			Assert.True(File.Exists(ServerFile));
			Assert.True(File.Exists(Keep));
			Assert.True(File.Exists(AppFile));
		}
		public void Dispose()
		{
			// Only the unique directory this fixture created; never any production path.
			if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true);
		}
	}
}
