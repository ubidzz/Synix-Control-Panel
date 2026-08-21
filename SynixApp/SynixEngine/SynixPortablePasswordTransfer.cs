// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Synix_Control_Panel.SynixEngine
{
	/// <summary>
	/// Bridges current-user DPAPI protection across computers. The temporary
	/// vault is itself password-encrypted and exists only while an encrypted
	/// Synix transfer package is being created or restored.
	/// </summary>
	public static class SynixPortablePasswordTransfer
	{
		private static readonly byte[] Magic =
			Encoding.ASCII.GetBytes("SXPASS01");
		private const int FormatVersion = 1;
		private const int Pbkdf2Iterations = 600_000;
		private const int SaltSize = 16;
		private const int NonceSize = 12;
		private const int TagSize = 16;
		private const int KeySize = 32;
		private const int MaximumPayloadBytes = 16 * 1024 * 1024;
		private const string VaultFileName = ".synix-password-transfer.vault";

		public static string GetVaultPath(string synixRoot)
		{
			return Path.Combine(
				Path.GetFullPath(synixRoot),
				"SynixData",
				VaultFileName);
		}

		public static void PrepareEncryptedExport(
			string synixRoot,
			string transferPassword,
			IEnumerable<GameServer> servers)
		{
			if (string.IsNullOrEmpty(transferPassword))
			{
				throw new ArgumentException(
					"A transfer password is required for portable saved passwords.",
					nameof(transferPassword));
			}

			List<PortablePasswordEntry> entries = [];
			int index = 0;
			foreach (GameServer server in servers)
			{
				SynixServerPasswords passwords = SynixPasswordProtection
					.RevealServerPasswords(server);

				entries.Add(new PortablePasswordEntry
				{
					Index = index++,
					Game = server.Game ?? string.Empty,
					ServerName = server.ServerName ?? string.Empty,
					InstallPath = server.InstallPath ?? string.Empty,
					ServerPassword = passwords.ServerPassword,
					AdminPassword = passwords.AdminPassword,
					RconPassword = passwords.RconPassword
				});
			}

			byte[] plaintext = JsonSerializer.SerializeToUtf8Bytes(
				new PortablePasswordBundle
				{
					Version = FormatVersion,
					Servers = entries
				});

			try
			{
				byte[] vaultBytes = Encrypt(plaintext, transferPassword);
				try
				{
					WriteBytesAtomically(GetVaultPath(synixRoot), vaultBytes);
				}
				finally
				{
					CryptographicOperations.ZeroMemory(vaultBytes);
				}
			}
			finally
			{
				CryptographicOperations.ZeroMemory(plaintext);
			}
		}

		/// <summary>
		/// Returns false for older encrypted packages that have no portable vault.
		/// The imported servers.json is changed only after the vault is authenticated
		/// and every entry has been matched successfully.
		/// </summary>
		public static bool RestoreEncryptedImport(
			string synixRoot,
			string transferPassword)
		{
			string vaultPath = GetVaultPath(synixRoot);
			if (!File.Exists(vaultPath))
				return false;

			string serversPath = Path.Combine(
				Path.GetFullPath(synixRoot),
				"SynixData",
				"servers.json");
			if (!File.Exists(serversPath))
			{
				throw new SynixPasswordProtectionException(
					"The transfer contains saved passwords but no server list to restore them into.");
			}

			byte[] encryptedVault = File.ReadAllBytes(vaultPath);
			byte[] plaintext = Decrypt(encryptedVault, transferPassword);
			try
			{
				PortablePasswordBundle bundle =
					JsonSerializer.Deserialize<PortablePasswordBundle>(plaintext) ??
					throw new SynixPasswordProtectionException(
						"The portable saved-password list is incomplete.");

				if (bundle.Version != FormatVersion || bundle.Servers is null)
				{
					throw new SynixPasswordProtectionException(
						"This saved-password transfer version is not supported.");
				}

				List<GameServer> importedServers =
					JsonSerializer.Deserialize<List<GameServer>>(
						File.ReadAllText(serversPath)) ?? [];

				foreach (PortablePasswordEntry entry in bundle.Servers)
				{
					GameServer server = FindMatchingServer(importedServers, entry);
					SynixPasswordProtection.SetServerPasswords(
						server,
						new SynixServerPasswords(
							entry.ServerPassword,
							entry.AdminPassword,
							entry.RconPassword));
				}

				string protectedJson = SynixPasswordProtection
					.SerializeServersForStorage(importedServers);
				FileHandler.WriteTextAtomically(serversPath, protectedJson);
				DeleteVault(synixRoot);
				return true;
			}
			catch (SynixPasswordProtectionException)
			{
				throw;
			}
			catch (Exception exception) when (
				exception is JsonException or IOException or InvalidOperationException)
			{
				throw new SynixPasswordProtectionException(
					"Synix could not restore the portable saved passwords.",
					exception);
			}
			finally
			{
				CryptographicOperations.ZeroMemory(plaintext);
				CryptographicOperations.ZeroMemory(encryptedVault);
			}
		}

		public static void DeleteVault(string synixRoot)
		{
			string vaultPath = GetVaultPath(synixRoot);
			if (File.Exists(vaultPath))
				File.Delete(vaultPath);
		}

		private static GameServer FindMatchingServer(
			IReadOnlyList<GameServer> servers,
			PortablePasswordEntry entry)
		{
			if (entry.Index >= 0 && entry.Index < servers.Count)
			{
				GameServer indexed = servers[entry.Index];
				if (IdentityMatches(indexed, entry))
					return indexed;
			}

			List<GameServer> matches = servers
				.Where(server => IdentityMatches(server, entry))
				.ToList();

			if (matches.Count != 1)
			{
				throw new SynixPasswordProtectionException(
					$"The saved passwords for '{entry.ServerName}' could not be matched safely to one imported server.");
			}

			return matches[0];
		}

		private static bool IdentityMatches(
			GameServer server,
			PortablePasswordEntry entry)
		{
			return string.Equals(
					server.Game,
					entry.Game,
					StringComparison.OrdinalIgnoreCase) &&
				string.Equals(
					server.ServerName,
					entry.ServerName,
					StringComparison.OrdinalIgnoreCase) &&
				string.Equals(
					server.InstallPath,
					entry.InstallPath,
					StringComparison.OrdinalIgnoreCase);
		}

		private static byte[] Encrypt(byte[] plaintext, string password)
		{
			if (plaintext.Length > MaximumPayloadBytes)
			{
				throw new SynixPasswordProtectionException(
					"The saved-password list is unexpectedly large.");
			}

			byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
			byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
			byte[] key = DeriveKey(password, salt);
			byte[] ciphertext = new byte[plaintext.Length];
			byte[] tag = new byte[TagSize];
			byte[] header = BuildHeader(salt, nonce, ciphertext.Length);

			try
			{
				using AesGcm aes = new(key, TagSize);
				aes.Encrypt(nonce, plaintext, ciphertext, tag, header);

				byte[] result = new byte[
					header.Length + ciphertext.Length + tag.Length];
				Buffer.BlockCopy(header, 0, result, 0, header.Length);
				Buffer.BlockCopy(
					ciphertext,
					0,
					result,
					header.Length,
					ciphertext.Length);
				Buffer.BlockCopy(
					tag,
					0,
					result,
					header.Length + ciphertext.Length,
					tag.Length);
				return result;
			}
			finally
			{
				CryptographicOperations.ZeroMemory(key);
				CryptographicOperations.ZeroMemory(ciphertext);
				CryptographicOperations.ZeroMemory(tag);
			}
		}

		private static byte[] Decrypt(byte[] vaultBytes, string password)
		{
			int headerLength = Magic.Length + sizeof(int) * 3 +
				SaltSize + NonceSize;
			if (vaultBytes.Length < headerLength + TagSize)
				throw InvalidVault();

			using MemoryStream stream = new(vaultBytes, writable: false);
			using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: true);
			if (!reader.ReadBytes(Magic.Length).SequenceEqual(Magic) ||
				reader.ReadInt32() != FormatVersion ||
				reader.ReadInt32() != Pbkdf2Iterations)
			{
				throw InvalidVault();
			}

			byte[] salt = reader.ReadBytes(SaltSize);
			byte[] nonce = reader.ReadBytes(NonceSize);
			int ciphertextLength = reader.ReadInt32();
			if (salt.Length != SaltSize ||
				nonce.Length != NonceSize ||
				ciphertextLength < 0 ||
				ciphertextLength > MaximumPayloadBytes ||
				stream.Length - stream.Position != ciphertextLength + TagSize)
			{
				throw InvalidVault();
			}

			byte[] header = vaultBytes[..headerLength];
			byte[] ciphertext = reader.ReadBytes(ciphertextLength);
			byte[] tag = reader.ReadBytes(TagSize);
			byte[] key = DeriveKey(password, salt);
			byte[] plaintext = new byte[ciphertextLength];

			try
			{
				using AesGcm aes = new(key, TagSize);
				aes.Decrypt(nonce, ciphertext, tag, plaintext, header);
				return plaintext;
			}
			catch (CryptographicException exception)
			{
				CryptographicOperations.ZeroMemory(plaintext);
				throw new SynixPasswordProtectionException(
					"The portable saved passwords could not be unlocked with this transfer password.",
					exception);
			}
			finally
			{
				CryptographicOperations.ZeroMemory(key);
				CryptographicOperations.ZeroMemory(ciphertext);
				CryptographicOperations.ZeroMemory(tag);
			}
		}

		private static byte[] BuildHeader(
			byte[] salt,
			byte[] nonce,
			int ciphertextLength)
		{
			using MemoryStream stream = new();
			using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
			writer.Write(Magic);
			writer.Write(FormatVersion);
			writer.Write(Pbkdf2Iterations);
			writer.Write(salt);
			writer.Write(nonce);
			writer.Write(ciphertextLength);
			writer.Flush();
			return stream.ToArray();
		}

		private static byte[] DeriveKey(string password, byte[] salt)
		{
			return Rfc2898DeriveBytes.Pbkdf2(
				password,
				salt,
				Pbkdf2Iterations,
				HashAlgorithmName.SHA256,
				KeySize);
		}

		private static SynixPasswordProtectionException InvalidVault()
		{
			return new SynixPasswordProtectionException(
				"The portable saved-password data is damaged or incomplete.");
		}

		private static void WriteBytesAtomically(
			string fullPath,
			byte[] content)
		{
			string? directory = Path.GetDirectoryName(fullPath);
			if (string.IsNullOrWhiteSpace(directory))
				throw new InvalidOperationException("The password vault folder is missing.");

			Directory.CreateDirectory(directory);
			string temporaryPath = Path.Combine(
				directory,
				$".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");

			try
			{
				using (FileStream output = new(
					temporaryPath,
					FileMode.CreateNew,
					FileAccess.Write,
					FileShare.None,
					4096,
					FileOptions.WriteThrough))
				{
					output.Write(content);
					output.Flush(flushToDisk: true);
				}

				if (File.Exists(fullPath))
					File.Replace(temporaryPath, fullPath, null, true);
				else
					File.Move(temporaryPath, fullPath);
			}
			finally
			{
				try
				{
					if (File.Exists(temporaryPath))
						File.Delete(temporaryPath);
				}
				catch
				{
				}
			}
		}

		private sealed class PortablePasswordBundle
		{
			public int Version { get; set; }
			public List<PortablePasswordEntry> Servers { get; set; } = [];
		}

		private sealed class PortablePasswordEntry
		{
			public int Index { get; set; }
			public string Game { get; set; } = string.Empty;
			public string ServerName { get; set; } = string.Empty;
			public string InstallPath { get; set; } = string.Empty;
			public string ServerPassword { get; set; } = string.Empty;
			public string AdminPassword { get; set; } = string.Empty;
			public string RconPassword { get; set; } = string.Empty;
		}
	}
}
