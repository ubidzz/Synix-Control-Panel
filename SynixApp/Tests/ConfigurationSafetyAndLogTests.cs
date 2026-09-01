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
using Synix_Control_Panel.SynixApp.Database.GameConfigurations;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine;
using System.Text;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class ConfigurationSafetyAndLogTests
{
	[Fact]
	[Trait("Category", "Regression")]
	public void ConfigurationTextSnapshotPreservesSupportedEncodingsAndByteOrderMarks()
	{
		(Encoding Encoding, bool IncludePreamble)[] cases =
		[
			(new UTF8Encoding(false, true), false),
			(new UTF8Encoding(true, true), true),
			(new UnicodeEncoding(false, true, true), true),
			(new UnicodeEncoding(true, true, true), true),
			(new UTF32Encoding(false, true, true), true),
			(new UTF32Encoding(true, true, true), true),
			(Encoding.Latin1, false)
		];
		string root = CreateTestDirectory();
		try
		{
			for (int index = 0; index < cases.Length; index++)
			{
				(Encoding encoding, bool includePreamble) = cases[index];
				string path = Path.Combine(root, $"encoding-{index}.cfg");
				byte[] text = encoding.GetBytes("name=café\n");
				byte[] preamble = includePreamble ? encoding.GetPreamble() : [];
				File.WriteAllBytes(path, [.. preamble, .. text]);

				ConfigurationTextSnapshot snapshot = ConfigurationTextSnapshot.Read(path);
				byte[] encoded = snapshot.Encode("name=résumé\n");

				Assert.Equal(encoding.CodePage, snapshot.TextEncoding.CodePage);
				Assert.Equal(includePreamble, snapshot.HasByteOrderMark);
				Assert.True(encoded.AsSpan().StartsWith(preamble));
				Assert.Equal(
					"name=résumé\n",
					encoding.GetString(encoded, preamble.Length, encoded.Length - preamble.Length));
			}
		}
		finally
		{
			Directory.Delete(root, true);
		}
	}

	[Fact]
	public void ManagedConfigurationBackup_RestoresPreviousFile()
	{
		string root = CreateTestDirectory();
		try
		{
			GameServer server = new()
			{
				Game = "Minecraft",
				ServerName = "backup-test",
				InstallPath = root
			};
			string configurationPath = Path.Combine(root, "server.properties");
			File.WriteAllText(configurationPath, "motd=Before change\nserver-port=25565\n");

			ConfigurationBackupSnapshot? snapshot =
				GameFix.BackupManagedConfiguration(server, "Automated test");
			Assert.NotNull(snapshot);
			File.WriteAllText(configurationPath, "motd=After change\nserver-port=25566\n");

			ConfigurationRestoreResult restored =
				GameFix.RestorePreviousManagedConfiguration(server);

			Assert.True(restored.Succeeded, restored.Message);
			Assert.Equal(1, restored.RestoredFiles);
			Assert.Contains("motd=Before change", File.ReadAllText(configurationPath));
		}
		finally
		{
			Directory.Delete(root, true);
		}
	}

	[Fact]
	public void LogDiscovery_ReturnsNewestDeclaredLog()
	{
		string root = CreateTestDirectory();
		try
		{
			GameServer server = new()
			{
				Game = "Rust",
				ServerName = "log-test",
				InstallPath = root
			};
			string logDirectory = Path.Combine(root, "server", "log-test");
			Directory.CreateDirectory(logDirectory);
			string older = Path.Combine(logDirectory, "older.log");
			string newer = Path.Combine(logDirectory, "newer.log");
			File.WriteAllText(older, "older");
			File.WriteAllText(newer, "newer");
			File.SetLastWriteTimeUtc(older, DateTime.UtcNow.AddMinutes(-2));
			File.SetLastWriteTimeUtc(newer, DateTime.UtcNow.AddMinutes(-1));

			GameLogDiscoveryResult result = GameLogDiscovery.FindLatest(server);

			Assert.True(result.Found, string.Join(" ", result.Errors));
			Assert.Equal(newer, result.LatestLogPath);
		}
		finally
		{
			Directory.Delete(root, true);
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
