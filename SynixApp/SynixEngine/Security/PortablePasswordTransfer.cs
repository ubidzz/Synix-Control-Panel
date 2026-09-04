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
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		private static readonly byte[] PortablePasswordMagic =
			Encoding.ASCII.GetBytes("SXPASS01");
		private const int PortablePasswordFormatVersion = 2;
		private const int PortablePasswordPbkdf2Iterations = 600_000;
		private const int PortablePasswordSaltSize = 16;
		private const int PortablePasswordNonceSize = 12;
		private const int PortablePasswordTagSize = 16;
		private const int PortablePasswordKeySize = 32;
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
					"A transfer password is required for portable saved credentials.",
					nameof(transferPassword));
			}

			List<PortablePasswordEntry> entries = [];
			int index = 0;
			foreach (GameServer server in servers)
			{
				SynixServerSecrets secrets = Core
					.RevealServerSecrets(server);
				SynixServerPasswords passwords = secrets.Passwords;

				entries.Add(new PortablePasswordEntry
				{
					Index = index++,
					Game = server.Game ?? string.Empty,
					ServerName = server.ServerName ?? string.Empty,
					InstallPath = server.InstallPath ?? string.Empty,
					ServerPassword = passwords.ServerPassword,
					AdminPassword = passwords.AdminPassword,
					RconPassword = passwords.RconPassword,
					AuthenticationToken = passwords.AuthenticationToken,
					DiscordWebhook = secrets.DiscordWebhook,
					DiscordWebhookRoutes = Core
						.RevealDiscordWebhookRoutes(server)
						.ToList()
				});
			}

			byte[] plaintext = JsonSerializer.SerializeToUtf8Bytes(
				new PortablePasswordBundle
				{
					Version = PortablePasswordFormatVersion,
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
					"The transfer contains saved credentials but no server list to restore them into.");
			}

			byte[] encryptedVault = File.ReadAllBytes(vaultPath);
			byte[] plaintext = Decrypt(encryptedVault, transferPassword);
			try
			{
				PortablePasswordBundle bundle =
					JsonSerializer.Deserialize<PortablePasswordBundle>(plaintext) ??
					throw new SynixPasswordProtectionException(
						"The portable saved-credential list is incomplete.");

				if (bundle.Version is < 1 or > PortablePasswordFormatVersion ||
					bundle.Servers is null)
				{
					throw new SynixPasswordProtectionException(
						"This saved-credential transfer version is not supported.");
				}

				List<GameServer> importedServers =
					JsonSerializer.Deserialize<List<GameServer>>(
						File.ReadAllText(serversPath)) ?? [];

				foreach (PortablePasswordEntry entry in bundle.Servers)
				{
					GameServer server = FindMatchingServer(importedServers, entry);

					string discordWebhook = entry.DiscordWebhook ??
						Core.RevealDiscordWebhook(server);

					Core.SetServerSecrets(
						server,
						new SynixServerSecrets(
							new SynixServerPasswords(
								entry.ServerPassword,
								entry.AdminPassword,
								entry.RconPassword,
								entry.AuthenticationToken),
							discordWebhook));
					Core.SetDiscordWebhookRoutes(
						server,
						entry.DiscordWebhookRoutes ?? []);
				}

				string protectedJson = Core
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
					"Synix could not restore the portable saved credentials.",
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
					$"The saved credentials for '{entry.ServerName}' could not be matched safely to one imported server.");
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
					"The saved-credential list is unexpectedly large.");
			}

			byte[] salt = RandomNumberGenerator.GetBytes(PortablePasswordSaltSize);
			byte[] nonce = RandomNumberGenerator.GetBytes(PortablePasswordNonceSize);
			byte[] key = DerivePortablePasswordKey(password, salt);
			byte[] ciphertext = new byte[plaintext.Length];
			byte[] tag = new byte[PortablePasswordTagSize];
			byte[] header = BuildHeader(salt, nonce, ciphertext.Length);

			try
			{
				using AesGcm aes = new(key, PortablePasswordTagSize);
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
			int headerLength = PortablePasswordMagic.Length + sizeof(int) * 3 +
				PortablePasswordSaltSize + PortablePasswordNonceSize;
			if (vaultBytes.Length < headerLength + PortablePasswordTagSize)
				throw InvalidVault();

			using MemoryStream stream = new(vaultBytes, writable: false);
			using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: true);
			byte[] magic = reader.ReadBytes(PortablePasswordMagic.Length);
			int formatVersion = reader.ReadInt32();
			int iterations = reader.ReadInt32();
			if (!magic.SequenceEqual(PortablePasswordMagic) ||
				formatVersion is < 1 or > PortablePasswordFormatVersion ||
				iterations != PortablePasswordPbkdf2Iterations)
			{
				throw InvalidVault();
			}

			byte[] salt = reader.ReadBytes(PortablePasswordSaltSize);
			byte[] nonce = reader.ReadBytes(PortablePasswordNonceSize);
			int ciphertextLength = reader.ReadInt32();
			if (salt.Length != PortablePasswordSaltSize ||
				nonce.Length != PortablePasswordNonceSize ||
				ciphertextLength < 0 ||
				ciphertextLength > MaximumPayloadBytes ||
				stream.Length - stream.Position != ciphertextLength + PortablePasswordTagSize)
			{
				throw InvalidVault();
			}

			byte[] header = vaultBytes[..headerLength];
			byte[] ciphertext = reader.ReadBytes(ciphertextLength);
			byte[] tag = reader.ReadBytes(PortablePasswordTagSize);
			byte[] key = DerivePortablePasswordKey(password, salt);
			byte[] plaintext = new byte[ciphertextLength];

			try
			{
				using AesGcm aes = new(key, PortablePasswordTagSize);
				aes.Decrypt(nonce, ciphertext, tag, plaintext, header);
				return plaintext;
			}
			catch (CryptographicException exception)
			{
				CryptographicOperations.ZeroMemory(plaintext);
				throw new SynixPasswordProtectionException(
					"The portable saved credentials could not be unlocked with this transfer password.",
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
			writer.Write(PortablePasswordMagic);
			writer.Write(PortablePasswordFormatVersion);
			writer.Write(PortablePasswordPbkdf2Iterations);
			writer.Write(salt);
			writer.Write(nonce);
			writer.Write(ciphertextLength);
			writer.Flush();
			return stream.ToArray();
		}

		private static byte[] DerivePortablePasswordKey(string password, byte[] salt)
		{
			return Rfc2898DeriveBytes.Pbkdf2(
				password,
				salt,
				PortablePasswordPbkdf2Iterations,
				HashAlgorithmName.SHA256,
				PortablePasswordKeySize);
		}

		private static SynixPasswordProtectionException InvalidVault()
		{
			return new SynixPasswordProtectionException(
				"The portable saved-credential data is damaged or incomplete.");
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
				catch (Exception suppressedException)
				{
					Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException);
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
			public string AuthenticationToken { get; set; } = string.Empty;

			public string? DiscordWebhook { get; set; }
			public List<DiscordWebhookRoute>? DiscordWebhookRoutes { get; set; }
		}
	}
}
