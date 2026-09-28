using System.Text;
using System.Text.Json;
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Database.GameConfigurations;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixApp.Localization;
using Synix_Control_Panel.SynixEngine;
using Synix_Control_Panel.SynixEngine.Minecraft;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class GameTextSupportTests
{
	[Fact]
	public void EveryCurrentGameHasAnIndividualOnlineSearchAndSeparateFieldStatuses()
	{
		Assert.Equal(GameDatabase.GetGames.Select(game => game.DefinitionId).Order(),
			GameTextSupportCatalog.Entries.Select(entry => entry.GameId).Order());
		foreach (var entry in GameTextSupportCatalog.Entries)
		{
			Assert.NotEqual(default, entry.SearchedOn);
			Assert.False(string.IsNullOrWhiteSpace(entry.SearchQuery));
			Assert.Equal(4, entry.Fields.Count());
			foreach (var field in entry.Fields)
			{
				Assert.True(Enum.IsDefined(field.Status));
				if (field.Status == GameTextSupport.Unverified) continue;
				Assert.False(string.IsNullOrWhiteSpace(field.Notes));
				Assert.NotEmpty(field.Sources);
				Assert.All(field.Sources, source => Assert.True(GameTextSupportCatalog.IsSafeSource(source)));
			}
		}
	}

	[Fact]
	public void TitleEvidenceDoesNotAutomaticallyApprovePasswords()
	{
		var onset = GameTextSupportCatalog.ForGame("Onset")!;
		Assert.Equal(GameTextSupport.Documented, onset.ServerTitle.Status);
		Assert.Equal(GameTextSupport.Unverified, onset.JoinPassword.Status);
		Assert.Equal(GameTextSupport.Unverified, onset.AdminPassword.Status);
		Assert.Equal(GameTextSupport.Unverified, onset.RconPassword.Status);
		Assert.Equal(GameTextSupport.Restricted, GameTextSupportCatalog.ForGame("Craftopia")!.JoinPassword.Status);
		Assert.Equal(GameTextSupport.NotApplicable, GameTextSupportCatalog.ForGame("Minecraft")!.JoinPassword.Status);
		Assert.Equal(GameTextSupport.Unverified, GameTextSupportCatalog.ForGame("ARK: Survival Ascended")!.JoinPassword.Status);
	}

	[Fact]
	public void MissingOrCustomGameIsUnverifiedNotUnsupported()
	{
		Assert.Null(GameTextSupportCatalog.ForGame("Custom fixture game"));
		string help = GameTextSupportCatalog.HelpText("Custom fixture game");
		Assert.Contains(LocalizationManager.Get("GameText.Status.Unverified"), help);
		Assert.Contains(LocalizationManager.Get("GameText.UnverifiedNote"), help);
	}

	[Theory]
	[InlineData("http://example.com/doc")]
	[InlineData("https://localhost/doc")]
	[InlineData("https://example.com:444/doc")]
	[InlineData("https://user:secret@example.com/doc")]
	[InlineData("file:///C:/fixture")]
	public void UnsafeDocumentationLinksAreRejected(string source) => Assert.False(GameTextSupportCatalog.IsSafeSource(source));

	[Theory]
	[InlineData("Documented", "", "https://example.com/doc")]
	[InlineData("Documented", "An unsupported assertion", null)]
	[InlineData("NotAStatus", "Fixture", "https://example.com/doc")]
	[InlineData("Conditional", "Fixture", "file:///C:/fixture")]
	public void CatalogRejectsUnsupportedClaimsAndUnsafeSources(string status, string notes, string? source)
	{
		string json = JsonSerializer.Serialize(new
		{
			schemaVersion = 1, scope = "Fixture", entries = new[] { new {
				gameId = "minecraft", searchedOn = "2026-10-17", searchQuery = "Minecraft server name password Unicode",
				serverTitle = new { status, notes, sources = source == null ? Array.Empty<string>() : new[] { source } }
			} }
		});
		Assert.Throws<InvalidDataException>(() => GameTextSupportCatalog.Parse(json));
	}

	[Theory]
	[InlineData("朋友的伺服器", "測試密碼123", "管理密碼123")]
	[InlineData("日本語サーバー", "合言葉12345", "管理者12345")]
	[InlineData("한국어 서버", "비밀번호12345", "관리자12345")]
	[InlineData("Сервер друзей", "Пароль12345", "Админ12345")]
	[InlineData("Café e\u0301 🌍", " secret\u0301🔐 ", "مفتاح12345")]
	public void SynixDoesNotBlockOrNormalizeCompleteInternationalText(string title, string joinPassword, string adminPassword)
	{
		SynixServerPasswords passwords = new(joinPassword, adminPassword, "RCON密碼123");
		Assert.True(GameServerInputValidator.TryValidate(GameDatabase.GetGame("ARK: Survival Ascended")!, title, passwords, out var error), error);
		GameServer server = new() { Game = "ARK: Survival Ascended", ServerName = title };
		Core.SetServerPasswords(server, passwords);
		Assert.Equal(joinPassword, Core.Reveal(server.Password));
		Assert.Equal(adminPassword, Core.Reveal(server.AdminPassword));
		GameServer roundtrip = JsonSerializer.Deserialize<GameServer>(JsonSerializer.Serialize(server))!;
		Assert.Equal(title, roundtrip.ServerName);
		Assert.Equal(joinPassword, Core.Reveal(roundtrip.Password));
		Assert.Equal(adminPassword, Core.Reveal(roundtrip.AdminPassword));
	}

	[Theory]
	[InlineData("title")]
	[InlineData("join")]
	[InlineData("admin")]
	[InlineData("rcon")]
	public void IncompleteCharactersAreReportedWithoutExposingThePassword(string field)
	{
		string malformed = "fixture-secret-" + (char)0xD800;
		var passwords = new SynixServerPasswords(field == "join" ? malformed : "",
			field == "admin" ? malformed : "", field == "rcon" ? malformed : "");
		Assert.False(GameServerInputValidator.TryValidate(GameDatabase.GetGame("ARK: Survival Ascended")!,
			field == "title" ? malformed : "Fixture", passwords, out var error));
		Assert.DoesNotContain("fixture-secret", error);
		Assert.False(string.IsNullOrWhiteSpace(error));
	}

	[Theory]
	[InlineData("1.19.4")]
	[InlineData("1.20")]
	[InlineData("1.21.8")]
	public void JavaPropertiesPreserveUnicodeAcrossLegacyAndModernReaders(string version)
	{
		GameServer server = new() { Game = "Minecraft", ServerName = "中文 Привет 🌍", GameVersion = version,
			WorldName = "世界", WorldSeed = "种子", EnableRcon = true };
		const string password = "  密碼e\u0301🔐\\u1234  ";
		var context = new ConfigurationContext(server, new SynixServerPasswords("", "", password), "fixture", "127.0.0.1", "127.0.0.1");
		var definition = new MinecraftConfiguration();
		string properties = string.Join("\n", definition.Bindings.Where(binding => binding.Key is "motd" or "level-name" or "level-seed" or "rcon.password")
			.Select(binding => binding.Key + "=" + binding.Value(context)));
		Assert.All(properties, character => Assert.True(character <= 127));
		Assert.Contains("\\u", properties);
		foreach (Encoding encoding in new[] { Encoding.Latin1, Encoding.UTF8 })
		{
			var decoded = MinecraftConfigurationSync.ReadProperties(encoding.GetString(Encoding.UTF8.GetBytes(properties)));
			Assert.Equal(server.ServerName, decoded["motd"]);
			Assert.Equal(server.WorldName, decoded["level-name"]);
			Assert.Equal(server.WorldSeed, decoded["level-seed"]);
			Assert.Equal(password, decoded["rcon.password"]);
		}
	}

	[Fact]
	public void BedrockKeepsNativeUtf8InsteadOfJavaEscapes()
	{
		GameServer server = new() { Game = "Minecraft", MinecraftEdition = "Bedrock", ServerName = "朋友的伺服器", WorldName = "世界" };
		var context = new ConfigurationContext(server, new SynixServerPasswords("", "", ""), "fixture", "127.0.0.1", "127.0.0.1");
		var definition = new MinecraftBedrockConfiguration();
		Assert.Equal(server.ServerName, definition.Bindings.Single(binding => binding.Key == "server-name").Value(context));
		Assert.Contains("server-name=" + server.ServerName, definition.CreateTemplate(context));
		Assert.DoesNotContain("\\u", definition.CreateTemplate(context));
	}

	[Fact]
	public void RconPacketsCountUtf8BytesRatherThanCharacters()
	{
		const string body = "密碼e\u0301🔐123";
		byte[] utf8 = Encoding.UTF8.GetBytes(body);
		byte[] packet = MinecraftRconClient.EncodePacket(123, 3, body);
		Assert.Equal(utf8.Length + 14, packet.Length);
		Assert.Equal(utf8.Length + 10, BitConverter.ToInt32(packet, 0));
		Assert.Equal(utf8, packet[12..^2]);
		Assert.Equal(body, Encoding.UTF8.GetString(packet[12..^2]));
	}
}
