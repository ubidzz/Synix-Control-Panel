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
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Synix_Control_Panel.SynixEngine
{
	public readonly record struct SynixServerPasswords(
   string ServerPassword,
   string AdminPassword,
   string RconPassword);

	public readonly record struct SynixServerSecrets(
		SynixServerPasswords Passwords,
		string DiscordWebhook);

	public sealed class SynixPasswordProtectionException : Exception
	{
		public SynixPasswordProtectionException(string message, Exception? inner = null)
			: base(message, inner)
		{
		}
	}

	public partial class Core
	{
		public const int CurrentStorageVersion = 3;
		private const int DiscordWebhookStorageVersion = 2;
		private const int DiscordRouteStorageVersion = 3;
		public const string ProtectedValuePrefix = "synix-dpapi-v1:";

		private static readonly byte[] AdditionalEntropy =
			SHA256.HashData(Encoding.UTF8.GetBytes(
				"Synix Control Panel|Server Passwords|v1"));

		public static bool IsProtected(string? storedValue)
		{
			return !string.IsNullOrEmpty(storedValue) &&
				storedValue.StartsWith(
					ProtectedValuePrefix,
					StringComparison.Ordinal);
		}

		public static string Protect(string? plaintext)
		{
			if (string.IsNullOrEmpty(plaintext))
				return string.Empty;

			byte[] clearBytes = Encoding.UTF8.GetBytes(plaintext);
			try
			{
				byte[] protectedBytes = ProtectedData.Protect(
					clearBytes,
					AdditionalEntropy,
					DataProtectionScope.CurrentUser);

				try
				{
					return ProtectedValuePrefix +
						Convert.ToBase64String(protectedBytes);
				}
				finally
				{
					CryptographicOperations.ZeroMemory(protectedBytes);
				}
			}
			catch (Exception exception) when (
				exception is CryptographicException or PlatformNotSupportedException)
			{
				throw new SynixPasswordProtectionException(
					"Windows could not protect the saved server credentials for this user.",
					exception);
			}
			finally
			{
				CryptographicOperations.ZeroMemory(clearBytes);
			}
		}

		public static string Reveal(string? storedValue)
		{
			if (string.IsNullOrEmpty(storedValue))
				return string.Empty;

			if (!IsProtected(storedValue))
				return storedValue;

			byte[] protectedBytes;
			try
			{
				protectedBytes = Convert.FromBase64String(
					storedValue[ProtectedValuePrefix.Length..]);
			}
			catch (FormatException exception)
			{
				throw new SynixPasswordProtectionException(
					"The saved credential data is damaged or incomplete.",
					exception);
			}

			try
			{
				byte[] clearBytes = ProtectedData.Unprotect(
					protectedBytes,
					AdditionalEntropy,
					DataProtectionScope.CurrentUser);

				try
				{
					return Encoding.UTF8.GetString(clearBytes);
				}
				finally
				{
					CryptographicOperations.ZeroMemory(clearBytes);
				}
			}
			catch (Exception exception) when (
				exception is CryptographicException or PlatformNotSupportedException)
			{
				throw new SynixPasswordProtectionException(
					"These saved credentials belong to another Windows user or computer, or the credential data is damaged.",
					exception);
			}
			finally
			{
				CryptographicOperations.ZeroMemory(protectedBytes);
			}
		}

		public static SynixServerPasswords RevealServerPasswords(GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);

			return new SynixServerPasswords(
				Reveal(server.Password),
				Reveal(server.AdminPassword),
				Reveal(server.RconPassword));
		}

		public static string RevealDiscordWebhook(GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);

			return server.PasswordStorageVersion < DiscordWebhookStorageVersion
				? server.DiscordWebhook ?? string.Empty
				: Reveal(server.DiscordWebhook);
		}

		public static IReadOnlyList<DiscordWebhookRoute> RevealDiscordWebhookRoutes(
			GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);
			List<DiscordWebhookRoute> routes = [];
			foreach (DiscordWebhookRoute route in server.DiscordWebhookRoutes ?? [])
			{
				routes.Add(new DiscordWebhookRoute
				{
					Id = string.IsNullOrWhiteSpace(route.Id)
						? Guid.NewGuid().ToString("N")
						: route.Id,
					Name = route.Name ?? string.Empty,
					Enabled = route.Enabled,
					WebhookUrl = server.PasswordStorageVersion < DiscordRouteStorageVersion
						? route.WebhookUrl ?? string.Empty
						: Reveal(route.WebhookUrl),
					Events = route.Events
				});
			}

			return routes;
		}

		public static SynixServerSecrets RevealServerSecrets(GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);

			return new SynixServerSecrets(
				RevealServerPasswords(server),
				RevealDiscordWebhook(server));
		}

		public static bool TryRevealServerPasswords(
			GameServer server,
			out SynixServerPasswords passwords)
		{
			try
			{
				passwords = RevealServerPasswords(server);
				return true;
			}
			catch (SynixPasswordProtectionException)
			{
				passwords = default;
				return false;
			}
		}

		public static bool TryRevealServerSecrets(
			GameServer server,
			out SynixServerSecrets secrets)
		{
			try
			{
				secrets = RevealServerSecrets(server);
				return true;
			}
			catch (SynixPasswordProtectionException)
			{
				secrets = default;
				return false;
			}
		}

		public static bool TryRevealDiscordWebhookRoutes(
			GameServer server,
			out IReadOnlyList<DiscordWebhookRoute> routes)
		{
			try
			{
				routes = RevealDiscordWebhookRoutes(server);
				return true;
			}
			catch (SynixPasswordProtectionException)
			{
				routes = [];
				return false;
			}
		}

		public static void SetServerPasswords(
   GameServer server,
   SynixServerPasswords plaintextPasswords)
		{
			ArgumentNullException.ThrowIfNull(server);
			string plaintextWebhook = RevealDiscordWebhook(server);

			SetServerSecrets(
				server,
				new SynixServerSecrets(
					plaintextPasswords,
					plaintextWebhook));
		}

		public static void SetServerSecrets(
   GameServer server,
   SynixServerSecrets plaintextSecrets)
		{
			ArgumentNullException.ThrowIfNull(server);

			string protectedServerPassword = Protect(
				plaintextSecrets.Passwords.ServerPassword);
			string protectedAdminPassword = Protect(
				plaintextSecrets.Passwords.AdminPassword);
			string protectedRconPassword = Protect(
				plaintextSecrets.Passwords.RconPassword);
			string protectedDiscordWebhook = Protect(
				plaintextSecrets.DiscordWebhook);

			server.Password = protectedServerPassword;
			server.AdminPassword = protectedAdminPassword;
			server.RconPassword = protectedRconPassword;
			server.DiscordWebhook = protectedDiscordWebhook;
			server.PasswordStorageVersion = CurrentStorageVersion;
		}

		public static void SetDiscordWebhookRoutes(
			GameServer server,
			IEnumerable<DiscordWebhookRoute>? plaintextRoutes)
		{
			ArgumentNullException.ThrowIfNull(server);
			List<DiscordWebhookRoute> protectedRoutes = [];
			foreach (DiscordWebhookRoute route in plaintextRoutes ?? [])
			{
				protectedRoutes.Add(new DiscordWebhookRoute
				{
					Id = string.IsNullOrWhiteSpace(route.Id)
						? Guid.NewGuid().ToString("N")
						: route.Id,
					Name = (route.Name ?? string.Empty).Trim(),
					Enabled = route.Enabled,
					WebhookUrl = Protect(route.WebhookUrl),
					Events = route.Events
				});
			}

			server.DiscordWebhookRoutes = protectedRoutes;
			server.PasswordStorageVersion = CurrentStorageVersion;
		}

		public static bool MigrateLegacyServer(GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);

			bool requiresMigration =
				server.PasswordStorageVersion < CurrentStorageVersion ||
				NeedsProtection(server.Password) ||
				NeedsProtection(server.AdminPassword) ||
				NeedsProtection(server.RconPassword) ||
				NeedsProtection(server.DiscordWebhook) ||
				(server.DiscordWebhookRoutes ?? []).Any(route =>
					NeedsProtection(route.WebhookUrl));

			if (!requiresMigration)
				return false;

			int previousStorageVersion = server.PasswordStorageVersion;
			SynixServerPasswords plaintextPasswords =
				previousStorageVersion == 0
					? new SynixServerPasswords(
						server.Password ?? string.Empty,
						server.AdminPassword ?? string.Empty,
						server.RconPassword ?? string.Empty)
					: new SynixServerPasswords(
						Reveal(server.Password),
						Reveal(server.AdminPassword),
						Reveal(server.RconPassword));
			string plaintextWebhook = previousStorageVersion < DiscordWebhookStorageVersion
				? server.DiscordWebhook ?? string.Empty
				: Reveal(server.DiscordWebhook);
			List<DiscordWebhookRoute> plaintextRoutes = (server.DiscordWebhookRoutes ?? [])
				.Select(route => new DiscordWebhookRoute
				{
					Id = route.Id,
					Name = route.Name,
					Enabled = route.Enabled,
					WebhookUrl = previousStorageVersion < DiscordRouteStorageVersion
						? route.WebhookUrl ?? string.Empty
						: Reveal(route.WebhookUrl),
					Events = route.Events
				})
				.ToList();

			SetServerSecrets(
				server,
				new SynixServerSecrets(
					plaintextPasswords,
					plaintextWebhook));
			SetDiscordWebhookRoutes(server, plaintextRoutes);

			return true;
		}

		public static string SerializeServersForStorage(
			IEnumerable<GameServer> servers,
			bool writeIndented = true)
		{
			ArgumentNullException.ThrowIfNull(servers);

			List<GameServer> serverList = servers.ToList();
			foreach (GameServer server in serverList)
				MigrateLegacyServer(server);

			return JsonSerializer.Serialize(
				serverList,
				new JsonSerializerOptions { WriteIndented = writeIndented });
		}

		public static List<GameServer> DeserializeServersAndMigrate(
			string json,
			out int migratedServerCount)
		{
			List<GameServer> servers =
				JsonSerializer.Deserialize<List<GameServer>>(json) ?? [];

			migratedServerCount = 0;
			foreach (GameServer server in servers)
			{
				if (MigrateLegacyServer(server))
					migratedServerCount++;
			}

			return servers;
		}

		private static bool NeedsProtection(string? storedValue)
		{
			return !string.IsNullOrEmpty(storedValue) &&
				!IsProtected(storedValue);
		}
	}
}
