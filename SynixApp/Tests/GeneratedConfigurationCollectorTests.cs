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
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class GeneratedConfigurationCollectorTests : IDisposable
{
	private readonly string _testRoot = Path.Combine(
		Path.GetTempPath(),
		"SynixGeneratedConfigurationTests",
		Guid.NewGuid().ToString("N"));

	public GeneratedConfigurationCollectorTests()
	{
		Directory.CreateDirectory(_testRoot);
	}

	[Fact]
	public void CollectServer_CopiesGameGeneratedJsonAndRedactsSecrets()
	{
		string installPath = Path.Combine(_testRoot, "server");
		string configPath = Path.Combine(
			installPath,
			"WS",
			"Saved",
			"GameplaySettings",
			"GameXishu.json");
		Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
		const string original =
			"{\"name\":\"Keep Me\",\"password\":\"server-secret\",\"admin_password\":\"admin-secret\",\"rcon\":{\"password\":\"rcon-secret\"}}";
		File.WriteAllText(configPath, original);
		GameServer server = new()
		{
			Game = "Soulmask",
			ServerName = "Capture Test",
			InstallPath = installPath
		};
		string destinationRoot = Path.Combine(_testRoot, "captures");

		GeneratedConfigurationCaptureResult result =
			GeneratedConfigurationCollector.CollectServer(server, destinationRoot);

		Assert.Equal(1, result.CopiedFiles);
		Assert.Empty(result.Errors);
		Assert.Equal(original, File.ReadAllText(configPath));
		string capturedPath = Path.Combine(
			destinationRoot,
			"Soulmask",
			"GameXishu.json");
		string captured = File.ReadAllText(capturedPath);
		Assert.Contains("Keep Me", captured, StringComparison.Ordinal);
		Assert.Contains("{Password}", captured, StringComparison.Ordinal);
		Assert.Contains("{AdminPassword}", captured, StringComparison.Ordinal);
		Assert.Contains("{RCONPassword}", captured, StringComparison.Ordinal);
		Assert.DoesNotContain("server-secret", captured, StringComparison.Ordinal);
		Assert.DoesNotContain("admin-secret", captured, StringComparison.Ordinal);
		Assert.DoesNotContain("rcon-secret", captured, StringComparison.Ordinal);
	}

	[Fact]
	public void CollectServer_CollectsEveryGeneratedConfigurationPath()
	{
		string installPath = Path.Combine(_testRoot, "astroneer");
		string configFolder = Path.Combine(
			installPath,
			"Astro",
			"Saved",
			"Config",
			"WindowsServer");
		Directory.CreateDirectory(configFolder);
		File.WriteAllText(
			Path.Combine(configFolder, "AstroServerSettings.ini"),
			"OwnerName=Synix Test\r\nOwnerGuid=0\r\n");
		File.WriteAllText(
			Path.Combine(configFolder, "Engine.ini"),
			"[URL]\r\nPort=8777\r\n");
		GameServer server = new()
		{
			Game = "ASTRONEER",
			ServerName = "Two Files",
			InstallPath = installPath
		};
		string destinationRoot = Path.Combine(_testRoot, "captures");

		GeneratedConfigurationCaptureResult result =
			GeneratedConfigurationCollector.CollectServer(server, destinationRoot);

		Assert.Equal(2, result.CopiedFiles);
		Assert.Empty(result.Errors);
		Assert.True(File.Exists(Path.Combine(
			destinationRoot,
			"ASTRONEER",
			"AstroServerSettings.ini")));
		Assert.True(File.Exists(Path.Combine(
			destinationRoot,
			"ASTRONEER",
			"Engine.ini")));
	}

	[Fact]
	public void CollectServer_PreservesXmlLayoutWhileRedactingPasswords()
	{
		string installPath = Path.Combine(_testRoot, "seven-days");
		string configPath = Path.Combine(installPath, "serverconfig.xml");
		Directory.CreateDirectory(installPath);
		const string original =
			"<?xml version=\"1.0\"?><ServerSettings><property name=\"ServerName\" value=\"Keep Me\"/><property name=\"ServerPassword\" value=\"private-value\"/></ServerSettings>";
		File.WriteAllText(configPath, original);
		GameServer server = new()
		{
			Game = "7 Days to Die",
			ServerName = "XML Test",
			InstallPath = installPath
		};
		string destinationRoot = Path.Combine(_testRoot, "captures");

		GeneratedConfigurationCaptureResult result =
			GeneratedConfigurationCollector.CollectServer(server, destinationRoot);

		Assert.Equal(1, result.CopiedFiles);
		Assert.Empty(result.Errors);
		Assert.Equal(original, File.ReadAllText(configPath));
		string captured = File.ReadAllText(Path.Combine(
			destinationRoot,
			"7 Days to Die",
			"serverconfig.xml"));
		Assert.Contains("Keep Me", captured, StringComparison.Ordinal);
		Assert.Contains("{Password}", captured, StringComparison.Ordinal);
		Assert.DoesNotContain("private-value", captured, StringComparison.Ordinal);
	}

	[Fact]
	public void CollectServer_DoesNotCaptureSynixCreatedTemplates()
	{
		string installPath = Path.Combine(_testRoot, "palworld");
		string configPath = Path.Combine(
			installPath,
			"Pal",
			"Saved",
			"Config",
			"WindowsServer",
			"PalWorldSettings.ini");
		Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
		File.WriteAllText(configPath, "ServerName=Test");
		GameServer server = new()
		{
			Game = "Palworld",
			ServerName = "Template Game",
			InstallPath = installPath
		};

		GeneratedConfigurationCaptureResult result =
			GeneratedConfigurationCollector.CollectServer(
				server,
				Path.Combine(_testRoot, "captures"));

		Assert.False(result.FoundFiles);
		Assert.Equal(0, result.CopiedFiles);
		Assert.Empty(result.Errors);
	}

	[Fact]
	public void CollectServer_DoesNotRewriteAnUnchangedCapture()
	{
		string installPath = Path.Combine(_testRoot, "repeat");
		string configPath = Path.Combine(
			installPath,
			"WS",
			"Saved",
			"GameplaySettings",
			"GameXishu.json");
		Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
		File.WriteAllText(configPath, "{\"password\":\"secret\"}");
		GameServer server = new()
		{
			Game = "Soulmask",
			ServerName = "Repeat",
			InstallPath = installPath
		};
		string destinationRoot = Path.Combine(_testRoot, "captures");

		GeneratedConfigurationCollector.CollectServer(server, destinationRoot);
		GeneratedConfigurationCaptureResult second =
			GeneratedConfigurationCollector.CollectServer(server, destinationRoot);

		Assert.Equal(0, second.CopiedFiles);
		Assert.Equal(1, second.UnchangedFiles);
		Assert.Empty(second.Errors);
	}

	public void Dispose()
	{
		if (Directory.Exists(_testRoot))
		{
			Directory.Delete(_testRoot, true);
		}
	}
}
