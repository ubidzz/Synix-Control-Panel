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
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixEngine;
using Synix_Control_Panel.SynixEngine.ModManagement;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Windows.Forms;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class ManagementWorkflowUiTests : IDisposable
{
	private readonly string _root = Path.Combine(Path.GetTempPath(), "SynixWorkflowUi-" + Guid.NewGuid().ToString("N"));
	private readonly string? _previousData = ModPackageManager.DataRootOverride;
	private readonly string? _previousProfiles = ModSystemCatalog.ExternalProfileRootOverride;
	private readonly GameServer _server;

	public ManagementWorkflowUiTests()
	{
		_server = new() { Game = "Minecraft", ServerName = "Workflow fixture", Status = "Stopped",
			MinecraftLoader = "Paper", InstallPath = Path.Combine(_root, "server") };
		Directory.CreateDirectory(_server.InstallPath);
		ModPackageManager.DataRootOverride = Path.Combine(_root, "history");
		ModSystemCatalog.ExternalProfileRootOverride = Path.Combine(_root, "profiles");
		Directory.CreateDirectory(ModSystemCatalog.ExternalProfileRootOverride);
		CreateJar("plugins", "Alpha");
		CreateJar("plugins", "Beta");
		CreateJar("mods", "Other");
	}

	[Fact]
	public void ModInventorySearchAndRefreshKeepTheCorrectInstallAreaAndSelection() => WorkflowUiTest.Run(() =>
	{
		using ModPluginManager manager = new(_server);
		WorkflowUiTest.Pump((Task)WorkflowUiTest.Invoke(manager, "RefreshInventory", false)!);
		DataGridView grid = Assert.IsType<DataGridView>(manager.Controls.Find("addOnInventoryGrid", true).Single());
		TextBox search = Assert.IsType<TextBox>(manager.Controls.Find("addOnSearchBox", true).Single());
		Assert.Equal(2, grid.Rows.Count);
		grid.CurrentCell = grid.Rows.Cast<DataGridViewRow>().Single(row => row.Cells[0].Value?.ToString() == "Beta").Cells[0];
		WorkflowUiTest.Pump((Task)WorkflowUiTest.Invoke(manager, "RefreshInventory", false)!);
		Assert.Equal("Beta", grid.CurrentRow!.Cells[0].Value);
		search.Text = "beta";
		Assert.Single(grid.Rows.Cast<DataGridViewRow>());
		Assert.Equal("Beta", grid.CurrentRow!.Cells[0].Value);
		search.Text = "no matching addon";
		Assert.Empty(grid.Rows.Cast<DataGridViewRow>());
		search.Clear();
		Assert.Equal(2, grid.Rows.Count);
		ComboBox targets = Assert.IsAssignableFrom<ComboBox>(manager.Controls.Find("modInstallAreaSelector", true).Single());
		targets.SelectedItem = targets.Items.Cast<ModInstallTarget>().Single(target => target.RelativePath == "mods");
		Assert.Single(grid.Rows.Cast<DataGridViewRow>());
		Assert.Equal("Other", grid.CurrentRow!.Cells[0].Value);
		CapturePreview(manager, "mod-manager-workflow.png");
	});

	[Theory]
	[InlineData("Paper", "plugins")]
	[InlineData("Purpur", "plugins")]
	[InlineData("Forge", "mods")]
	[InlineData("Fabric", "mods")]
	public void CurrentLoaderTakesPriorityOverLeftoverModFolders(string loader, string area)
	{
		_server.MinecraftLoader = loader;
		Assert.Equal(area, ModSystemCatalog.Detect(_server)!.RecommendedTarget.RelativePath);
	}

	[Fact]
	public void ModBusyStateBlocksClosingAndActionsThenRestoresThem() => WorkflowUiTest.Run(() =>
	{
		using ModPluginManager manager = new(_server);
		WorkflowUiTest.Invoke(manager, "SetBusy", true, "ModManager.Progress.Removing");
		FormClosingEventArgs closing = new(CloseReason.UserClosing, false);
		WorkflowUiTest.Invoke(manager, "OnFormClosing", closing);
		Assert.True(closing.Cancel);
		foreach (string name in new[] { "importAddOnPackage", "addOnSetupAction", "browseAddOnCatalog", "openAddOnFolder",
			"refreshAddOnInventory", "removeAddOnPackage", "closeAddOnManager" })
			Assert.False(manager.Controls.Find(name, true).Single().Enabled);
		Assert.False(manager.Controls.Find("addOnInventoryGrid", true).Single().Enabled);
		WorkflowUiTest.Invoke(manager, "SetBusy", false, null!);
		Assert.True(manager.Controls.Find("addOnInventoryGrid", true).Single().Enabled);
	});

	[Fact]
	public void BackupRefreshRetainsTheSelectedArchive() => WorkflowUiTest.Run(() =>
	{
		ServerBackupArchive newest = Backup("new.zip", DateTime.UtcNow);
		ServerBackupArchive older = Backup("old.zip", DateTime.UtcNow.AddDays(-1));
		using ServerBackupRestoreDialog dialog = new(_server, [newest, older]);
		DataGridView grid = dialog.Controls.Find("backupGrid", true).OfType<DataGridView>().Single();
		grid.ClearSelection();
		grid.CurrentCell = grid.Rows[1].Cells[0];
		grid.Rows[1].Selected = true;
		WorkflowUiTest.Invoke(dialog, "UpdateSelection");
		Assert.Equal(older.ArchivePath, dialog.SelectedBackup!.ArchivePath);
		WorkflowUiTest.Invoke(dialog, "LoadBackups", (object)new[] { newest, older });
		Assert.Equal(older.ArchivePath, dialog.SelectedBackup!.ArchivePath);
		WorkflowUiTest.Invoke(dialog, "LoadBackups", (object)new[] { newest });
		Assert.Equal(newest.ArchivePath, dialog.SelectedBackup!.ArchivePath);
	});

	[Fact]
	public void FailedBackupActionUnlocksTheDialogAndCannotRunTwice() => WorkflowUiTest.Run(() =>
	{
		using ServerBackupRestoreDialog dialog = new(_server, [Backup("fixture.zip", DateTime.UtcNow)]);
		TaskCompletionSource pending = new();
		Task operation = dialog.RunManagementActionAsync(() => pending.Task);
		Assert.True(dialog.UseWaitCursor);
		FormClosingEventArgs closing = new(CloseReason.UserClosing, false);
		WorkflowUiTest.Invoke(dialog, "OnFormClosing", closing);
		Assert.True(closing.Cancel);
		bool ranAgain = false;
		WorkflowUiTest.Pump(dialog.RunManagementActionAsync(() => { ranAgain = true; return Task.CompletedTask; }));
		Assert.False(ranAgain);
		WorkflowUiTest.Invoke(dialog, "UpdateSelection");
		Assert.False(dialog.Controls.Find("restoreButton", true).Single().Enabled);
		pending.SetException(new IOException("Test verification failure"));
		Assert.Throws<IOException>(() => WorkflowUiTest.Pump(operation));
		Assert.False(dialog.UseWaitCursor);
		Assert.True(dialog.Controls.Find("verifyButton", true).Single().Enabled);
		closing = new(CloseReason.UserClosing, false);
		WorkflowUiTest.Invoke(dialog, "OnFormClosing", closing);
		Assert.False(closing.Cancel);
	});

	private ServerBackupArchive Backup(string name, DateTime created) =>
		new(Path.Combine(_root, name), created, 2048, 4096, ServerBackupIntegrity.Recorded, created);

	private void CreateJar(string folder, string name)
	{
		string path = Path.Combine(_server.InstallPath, folder);
		Directory.CreateDirectory(path);
		using ZipArchive archive = ZipFile.Open(Path.Combine(path, name + ".jar"), ZipArchiveMode.Create);
		using StreamWriter writer = new(archive.CreateEntry("META-INF/MANIFEST.MF").Open());
		writer.Write("Implementation-Version: 1.0");
	}

	private static void CapturePreview(Form form, string fileName)
	{
		string? folder = Environment.GetEnvironmentVariable("SYNIX_TEST_UI_PREVIEW_DIR");
		if (string.IsNullOrWhiteSpace(folder)) return;
		Directory.CreateDirectory(folder);
		form.StartPosition = FormStartPosition.Manual;
		form.Location = new Point(-20000, -20000);
		form.ShowInTaskbar = false;
		form.Show();
		WorkflowUiTest.WaitUntil(() => !form.UseWaitCursor);
		form.Update();
		using Bitmap image = new(form.Width, form.Height);
		form.DrawToBitmap(image, new Rectangle(Point.Empty, form.Size));
		image.Save(Path.Combine(folder, fileName), ImageFormat.Png);
		form.Hide();
	}

	public void Dispose()
	{
		ModPackageManager.DataRootOverride = _previousData;
		ModSystemCatalog.ExternalProfileRootOverride = _previousProfiles;
		ModPathSafety.EnsureTreeHasNoLinks(_root);
		Directory.Delete(_root, recursive: true);
	}
}
