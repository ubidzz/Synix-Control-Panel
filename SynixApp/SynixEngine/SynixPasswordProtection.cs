// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Synix_Control_Panel.SynixEngine
{
	/// <summary>
	/// Plaintext copies returned by this type should be kept only for the short
	/// operation that needs them (display, editing, launch, or config creation).
	/// </summary>
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

	/// <summary>
	/// Protects Synix-managed server credentials with Windows DPAPI, scoped to
	/// the Windows user running Synix. The prefix provides unambiguous versioning
	/// and prevents old readable values from being encrypted more than once.
	/// </summary>
	public static class SynixPasswordProtection
	{
		public const int CurrentStorageVersion = 2;
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

		/// <summary>
		/// Reveals both new protected values and legacy readable values. Supporting
		/// the legacy case keeps an older file usable until its atomic migration has
		/// completed successfully.
		/// </summary>
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

		/// <summary>
		/// Storage version 0 and 1 predate webhook protection, so their webhook is
		/// still readable legacy text until the automatic version 2 migration.
		/// </summary>
		public static string RevealDiscordWebhook(GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);

			return server.PasswordStorageVersion < CurrentStorageVersion
				? server.DiscordWebhook ?? string.Empty
				: Reveal(server.DiscordWebhook);
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

		/// <summary>
		/// Replaces all three saved values together so a protection failure cannot
		/// leave only part of a server entry migrated.
		/// </summary>
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

		/// <summary>
		/// Replaces every Synix-managed credential together so a protection failure
		/// cannot leave only part of a server entry upgraded.
		/// </summary>
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

		/// <summary>
		/// Upgrades a server loaded from an older Synix release. Returns true when
		/// the JSON needs to be rewritten.
		/// </summary>
		public static bool MigrateLegacyServer(GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);

			bool requiresMigration =
				server.PasswordStorageVersion < CurrentStorageVersion ||
				NeedsProtection(server.Password) ||
				NeedsProtection(server.AdminPassword) ||
				NeedsProtection(server.RconPassword) ||
				NeedsProtection(server.DiscordWebhook);

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
			string plaintextWebhook = previousStorageVersion < CurrentStorageVersion
				? server.DiscordWebhook ?? string.Empty
				: Reveal(server.DiscordWebhook);

			SetServerSecrets(
				server,
				new SynixServerSecrets(
					plaintextPasswords,
					plaintextWebhook));

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
