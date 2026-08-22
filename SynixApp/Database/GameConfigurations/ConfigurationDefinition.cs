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
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine;
using System.Text;

namespace Synix_Control_Panel.SynixApp.Database.GameConfigurations
{
	[Flags]
	internal enum ManagedConfigurationInput
	{
		None = 0,
		ServerPassword = 1 << 0,
		AdminPassword = 1 << 1,
		WorldSeed = 1 << 2,
		GameMode = 1 << 3,
		MaxPlayers = 1 << 4,
		QueryPort = 1 << 5,
		WorldName = 1 << 6,
		Rcon = 1 << 7,
		WorldSize = 1 << 8,
		Port = 1 << 9,
		AppPort = 1 << 10
	}

	internal sealed class ConfigurationContext
	{
		public ConfigurationContext(
			GameServer server,
			SynixServerPasswords passwords,
			string identity,
			string localIp,
			string publicIp)
		{
			Server = server;
			Passwords = passwords;
			Identity = identity;
			LocalIp = localIp;
			PublicIp = publicIp;
		}

		public GameServer Server { get; }
		public SynixServerPasswords Passwords { get; }
		public string Identity { get; }
		public string LocalIp { get; }
		public string PublicIp { get; }
	}

	internal sealed class ConfigurationBinding
	{
		public ConfigurationBinding(
			string key,
			Func<ConfigurationContext, string> value,
			string? path = null)
		{
			Key = key;
			Value = value;
			Path = path;
		}

		public string Key { get; }
		public string? Path { get; }
		public Func<ConfigurationContext, string> Value { get; }
	}

	internal readonly record struct ConfigurationApplyResult(
		bool Succeeded,
		bool Complete,
		bool Changed,
		bool Created,
		string Message)
	{
		public static ConfigurationApplyResult ArgumentsOnly() =>
			new(true, true, false, false, "Basic settings are managed through launch arguments.");

		public static ConfigurationApplyResult Failure(string message) =>
			new(false, false, false, false, message);
	}

	internal abstract class ConfigurationDefinition
	{
		public abstract string GameName { get; }
		public virtual IReadOnlyList<string> Aliases => [];
		public virtual int SchemaVersion => 1;
		public virtual bool UsesConfigurationFile => true;
		public virtual bool RequiresNetworkAddresses => false;
		public virtual ManagedConfigurationInput SupportedInputs =>
			ManagedConfigurationInput.None;
		public virtual string RelativePath => string.Empty;
		public virtual ConfigFormat Format => ConfigFormat.StandardINI;
		public virtual IReadOnlyList<ConfigurationBinding> Bindings => [];

		public virtual string? CreateTemplate(ConfigurationContext context) => null;

		public virtual ConfigurationApplyResult Apply(ConfigurationContext context)
		{
			if (!UsesConfigurationFile)
			{
				return ConfigurationApplyResult.ArgumentsOnly();
			}

			try
			{
				string fullPath = ResolveFullPath(context.Server);
				bool created = false;
				if (!File.Exists(fullPath))
				{
					string? template = CreateTemplate(context);
					if (template == null)
					{
						return ConfigurationApplyResult.Failure(
							$"The {GameName} configuration has not been generated yet.");
					}

					WriteNewFile(fullPath, template);
					created = true;
				}

				List<ConfigLine> values = ConfigHandler.LoadConfig(fullPath, Format);
				List<string> missing = [];
				bool changed = created;

				foreach (ConfigurationBinding binding in Bindings)
				{
					List<ConfigLine> matches = values.Where(value =>
						string.Equals(value.Key, binding.Key, StringComparison.Ordinal) &&
						(binding.Path == null ||
						 string.Equals(value.Path, binding.Path, StringComparison.Ordinal)))
						.ToList();

					if (matches.Count != 1)
					{
						missing.Add(binding.Path ?? binding.Key);
						continue;
					}

					ConfigLine target = matches[0];
					string newValue = RequireSingleLine(binding.Value(context), binding.Key);
					if (!ValuesMatch(target.Value, newValue))
					{
						target.Value = newValue;
						changed = true;
					}
				}

				if (changed)
				{
					ConfigHandler.SaveConfig(fullPath, values, Format);
				}

				if (missing.Count > 0)
				{
					return new ConfigurationApplyResult(
						true,
						false,
						changed,
						created,
						$"The file was preserved, but these managed settings were not found: {string.Join(", ", missing)}.");
				}

				return new ConfigurationApplyResult(
					true,
					true,
					changed,
					created,
					created
						? $"Created and verified the {GameName} configuration."
						: changed
							? $"Updated and verified the {GameName} configuration."
							: $"The {GameName} configuration is already current.");
			}
			catch (Exception ex)
			{
				return ConfigurationApplyResult.Failure(
					$"The {GameName} configuration could not be applied: {ex.Message}");
			}
		}

		public virtual bool ConfigurationFileExists(GameServer server)
		{
			return !UsesConfigurationFile || File.Exists(ResolveFullPath(server));
		}

		internal string ResolveFullPath(GameServer server)
		{
			return ResolveFullPath(server, RelativePath);
		}

		protected string ResolveFullPath(GameServer server, string relativePathTemplate)
		{
			if (string.IsNullOrWhiteSpace(server.InstallPath))
			{
				throw new InvalidOperationException("The server installation path is missing.");
			}

			string identity = Core.Instance.GetSafeName(server.ServerName);
			string relativePath = relativePathTemplate
				.Replace("{Identity}", identity, StringComparison.Ordinal)
				.Replace("{ServerName}", identity, StringComparison.Ordinal)
				.Replace("{map}", server.WorldName ?? string.Empty, StringComparison.Ordinal)
				.Replace("{port}", server.Port.ToString(), StringComparison.Ordinal)
				.Replace("{query}", server.QueryPort.ToString(), StringComparison.Ordinal)
				.Replace('/', Path.DirectorySeparatorChar)
				.Replace('\\', Path.DirectorySeparatorChar);

			string installRoot = Path.GetFullPath(server.InstallPath)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string fullPath = Path.GetFullPath(Path.Combine(installRoot, relativePath));
			string requiredPrefix = installRoot + Path.DirectorySeparatorChar;
			if (!fullPath.StartsWith(requiredPrefix, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException("The configuration path leaves the server installation folder.");
			}

			return fullPath;
		}

		protected static string RequireSingleLine(string? value, string settingName)
		{
			string safeValue = value ?? string.Empty;
			if (safeValue.Contains('\r') || safeValue.Contains('\n') || safeValue.Contains('\0'))
			{
				throw new InvalidDataException($"'{settingName}' cannot contain line breaks or null characters.");
			}

			return safeValue;
		}

		protected static string EscapeQuoted(string? value)
		{
			return RequireSingleLine(value, "Text value")
				.Replace("\"", "\\\"", StringComparison.Ordinal);
		}

		protected static string EscapeProperty(string? value)
		{
			return RequireSingleLine(value, "Property value")
				.Replace("\\", "\\\\", StringComparison.Ordinal);
		}

		private static bool ValuesMatch(string currentValue, string requestedValue)
		{
			if (bool.TryParse(currentValue, out bool currentBoolean) &&
				bool.TryParse(requestedValue, out bool requestedBoolean))
			{
				return currentBoolean == requestedBoolean;
			}

			return string.Equals(currentValue, requestedValue, StringComparison.Ordinal);
		}

		protected static void WriteNewFile(string path, string content)
		{
			string? directory = Path.GetDirectoryName(path);
			if (string.IsNullOrWhiteSpace(directory))
			{
				throw new InvalidOperationException("The configuration directory is unavailable.");
			}

			Directory.CreateDirectory(directory);
			string temporaryPath = Path.Combine(
				directory,
				$".{Path.GetFileName(path)}.{Guid.NewGuid():N}.synix.tmp");

			try
			{
				File.WriteAllText(temporaryPath, content, new UTF8Encoding(false, true));
				File.Move(temporaryPath, path, false);
			}
			finally
			{
				if (File.Exists(temporaryPath))
				{
					File.Delete(temporaryPath);
				}
			}
		}
	}
}
