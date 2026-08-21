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

	public sealed class SynixPasswordProtectionException : Exception
	{
		public SynixPasswordProtectionException(string message, Exception? inner = null)
			: base(message, inner)
		{
		}
	}

	/// <summary>
	/// Protects Synix-managed server passwords with Windows DPAPI, scoped to the
	/// Windows user running Synix. The prefix provides unambiguous versioning and
	/// prevents old readable values from being encrypted more than once.
	/// </summary>
	public static class SynixPasswordProtection
	{
		public const int CurrentStorageVersion = 1;
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
					"Windows could not protect the saved server passwords for this user.",
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
					"The saved password data is damaged or incomplete.",
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
					"These saved passwords belong to another Windows user or computer, or the password data is damaged.",
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

		/// <summary>
		/// Replaces all three saved values together so a protection failure cannot
		/// leave only part of a server entry migrated.
		/// </summary>
		public static void SetServerPasswords(
			GameServer server,
			SynixServerPasswords plaintextPasswords)
		{
			ArgumentNullException.ThrowIfNull(server);

			string protectedServerPassword = Protect(
				plaintextPasswords.ServerPassword);
			string protectedAdminPassword = Protect(
				plaintextPasswords.AdminPassword);
			string protectedRconPassword = Protect(
				plaintextPasswords.RconPassword);

			server.Password = protectedServerPassword;
			server.AdminPassword = protectedAdminPassword;
			server.RconPassword = protectedRconPassword;
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
				NeedsProtection(server.RconPassword);

			if (!requiresMigration)
				return false;

			SynixServerPasswords plaintextPasswords =
				server.PasswordStorageVersion < CurrentStorageVersion
					? new SynixServerPasswords(
						server.Password ?? string.Empty,
						server.AdminPassword ?? string.Empty,
						server.RconPassword ?? string.Empty)
					: new SynixServerPasswords(
						Reveal(server.Password),
						Reveal(server.AdminPassword),
						Reveal(server.RconPassword));

			SetServerPasswords(server, plaintextPasswords);

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
