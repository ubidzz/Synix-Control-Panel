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
		AppPort = 1 << 10,
		ServerName = 1 << 11,
		Crossplay = 1 << 12,
		InviteCode = 1 << 13
	}

	[Flags]
	internal enum GameManagementCapability
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
		AppPort = 1 << 10,
		Ram = 1 << 11,
		GameVersion = 1 << 12,
		Crossplay = 1 << 13,
		InviteCode = 1 << 14
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
			new(
				true,
				true,
				false,
				false,
				LocalizationManager.Get(
					"Configuration.Apply.ArgumentsOnly"));

		public static ConfigurationApplyResult Failure(string message) =>
			new(false, false, false, false, message);
	}

	internal abstract class ConfigurationDefinition
	{
		protected readonly record struct ResetTemplate(
			string RelativePath,
			string Content,
			ConfigFormat Format);

		public abstract string GameName { get; }
		public virtual IReadOnlyList<string> Aliases => [];
		public virtual int SchemaVersion => 1;
		public virtual bool UsesConfigurationFile => true;
		public virtual bool SupportsFullReset => false;
		public virtual bool PreservesInstalledTemplate => false;
		public virtual bool RequiresNetworkAddresses => false;
		public virtual ManagedConfigurationInput SupportedInputs =>
			ManagedConfigurationInput.None;
		public virtual string RelativePath => string.Empty;
		public virtual ConfigFormat Format => ConfigFormat.StandardINI;
		public virtual IReadOnlyList<ConfigurationBinding> Bindings => [];

		public virtual string? CreateTemplate(ConfigurationContext context)
		{
			if (!PreservesInstalledTemplate)
			{
				return null;
			}

			string templatePath = GetPreservedTemplatePath(
				ResolveFullPath(context.Server));
			return File.Exists(templatePath)
				? File.ReadAllText(templatePath)
				: null;
		}

		public virtual bool NeedsStructuralRepair(ConfigurationContext context)
		{
			if (!SupportsFullReset)
			{
				return false;
			}

			string? template = CreateTemplate(context);
			if (template == null)
			{
				return false;
			}

			string path = ResolveFullPath(context.Server);
			return !ConfigHandler.HasRequiredStructure(path, template, Format);
		}

		public virtual ConfigurationApplyResult ResetToTemplate(
			ConfigurationContext context)
		{
			if (!SupportsFullReset)
			{
				return ConfigurationApplyResult.Failure(
					LocalizationManager.Get(
						"Configuration.Apply.ResetTemplateMissing",
						GameName));
			}

			string? template = CreateTemplate(context);
			if (template == null)
			{
				return ConfigurationApplyResult.Failure(
					LocalizationManager.Get(
						"Configuration.Apply.ResetTemplateUnavailable",
						GameName));
			}

			ConfigurationApplyResult reset = ReplaceWithTemplates(
				context,
				[new ResetTemplate(RelativePath, template, Format)]);
			if (!reset.Succeeded)
			{
				return reset;
			}

			string backupPath = ResolveFullPath(context.Server) + ".synix.bak";
			byte[]? originalBackup = File.Exists(backupPath)
				? File.ReadAllBytes(backupPath)
				: null;
			ConfigurationApplyResult applied;
			try
			{
				applied = Apply(context);
			}
			finally
			{
				if (originalBackup != null)
				{
					File.WriteAllBytes(backupPath, originalBackup);
				}
			}
			if (!applied.Succeeded || !applied.Complete)
			{
				return new ConfigurationApplyResult(
					applied.Succeeded,
					applied.Complete,
					true,
					reset.Created,
					LocalizationManager.Get(
						"Configuration.Apply.RestoredReapplyFailed",
						GameName,
						applied.Message));
			}

			return reset with
			{
				Message = LocalizationManager.Get(
					"Configuration.Apply.Reapplied",
					reset.Message)
			};
		}

		public virtual IReadOnlyList<ConfigurationValidationItem> Validate(
			ConfigurationContext context)
		{
			List<ConfigurationValidationItem> items = [];
			if (!UsesConfigurationFile)
			{
				items.Add(new ConfigurationValidationItem(
					ConfigurationValidationState.Passed,
					LocalizationManager.Get(
						"Configuration.Check.LaunchArguments"),
					LocalizationManager.Get(
						"Configuration.Check.SynixValuesThroughArguments")));
				return items;
			}

			try
			{
				string fullPath = ResolveFullPath(context.Server);
				if (!File.Exists(fullPath))
				{
					items.Add(new ConfigurationValidationItem(
						ConfigurationValidationState.Failed,
						Path.GetFileName(fullPath),
						LocalizationManager.Get(
							"Configuration.Check.ManagedFile.Missing")));
					return items;
				}

				if (SupportsFullReset)
				{
					bool structuralRepairRequired = NeedsStructuralRepair(context);
					items.Add(new ConfigurationValidationItem(
						structuralRepairRequired
							? ConfigurationValidationState.Failed
							: ConfigurationValidationState.Passed,
						LocalizationManager.Get(
							"Configuration.Check.TemplateStructure"),
						structuralRepairRequired
							? LocalizationManager.Get(
								"Configuration.Check.TemplateStructure.Repairable")
							: LocalizationManager.Get(
								"Configuration.Check.TemplateStructure.Present")));
				}

				if (Bindings.Count == 0)
				{
					items.Add(new ConfigurationValidationItem(
						ConfigurationValidationState.Passed,
						LocalizationManager.Get(
							"Configuration.Check.ManagedValues"),
						LocalizationManager.Get(
							"Configuration.Check.ManagedValues.None")));
					return items;
				}

				List<ConfigLine> values = ConfigHandler.LoadConfig(fullPath, Format);
				foreach (ConfigurationBinding binding in Bindings)
				{
					string setting = binding.Path ?? binding.Key;
					List<ConfigLine> matches = values.Where(value =>
						string.Equals(value.Key, binding.Key, StringComparison.Ordinal) &&
						(binding.Path == null ||
						 string.Equals(value.Path, binding.Path, StringComparison.Ordinal)))
						.ToList();

					if (matches.Count == 0)
					{
						items.Add(new ConfigurationValidationItem(
							ConfigurationValidationState.Failed,
							setting,
							LocalizationManager.Get(
								"Configuration.Check.ManagedTag.Missing")));
						continue;
					}

					if (matches.Count > 1)
					{
						items.Add(new ConfigurationValidationItem(
							ConfigurationValidationState.Failed,
							setting,
							LocalizationManager.Get(
								"Configuration.Check.ManagedTag.Duplicate")));
						continue;
					}

					string expected = RequireSingleLine(
						binding.Value(context),
						binding.Key);
					bool matchesSavedValue = ValuesMatch(matches[0].Value, expected);
					items.Add(new ConfigurationValidationItem(
						matchesSavedValue
							? ConfigurationValidationState.Passed
							: ConfigurationValidationState.Failed,
						setting,
						matchesSavedValue
							? LocalizationManager.Get(
								"Configuration.Check.Value.Matches")
							: LocalizationManager.Get(
								"Configuration.Check.Value.Differs")));
				}
			}
			catch (Exception exception)
			{
				items.Add(new ConfigurationValidationItem(
					ConfigurationValidationState.Failed,
					LocalizationManager.Get(
						"Configuration.Check.ConfigurationRead"),
					LocalizationManager.Get(
						"Configuration.Check.ConfigurationRead.Failed",
						exception.Message)));
			}

			return items;
		}

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
				if (File.Exists(fullPath) && PreservesInstalledTemplate)
				{
					PreserveInstalledTemplate(fullPath);
				}

				if (!File.Exists(fullPath))
				{
					string? template = CreateTemplate(context);
					if (template == null)
					{
						return ConfigurationApplyResult.Failure(
							LocalizationManager.Get(
								"Configuration.Apply.NotGenerated",
								GameName));
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
						LocalizationManager.Get(
							"Configuration.Apply.ManagedSettingsMissing",
							string.Join(", ", missing)));
				}

				return new ConfigurationApplyResult(
					true,
					true,
					changed,
					created,
					created
						? LocalizationManager.Get(
							"Configuration.Apply.Created",
							GameName)
						: changed
							? LocalizationManager.Get(
								"Configuration.Apply.Updated",
								GameName)
							: LocalizationManager.Get(
								"Configuration.Apply.Current",
								GameName));
			}
			catch (Exception ex)
			{
				return ConfigurationApplyResult.Failure(
					LocalizationManager.Get(
						"Configuration.Apply.Failed",
						GameName,
						ex.Message));
			}
		}

		public virtual bool ConfigurationFileExists(GameServer server)
		{
			return !UsesConfigurationFile || File.Exists(ResolveFullPath(server));
		}

		internal virtual void PrepareConfigurationFilesForEditing(GameServer server)
		{
		}

		internal virtual IReadOnlyList<string> ResolveConfigurationPaths(
			GameServer server)
		{
			return UsesConfigurationFile && !string.IsNullOrWhiteSpace(RelativePath)
				? [ResolveFullPath(server)]
				: [];
		}

		internal string ResolveFullPath(GameServer server)
		{
			return ResolveFullPath(server, RelativePath);
		}

		protected string ResolveFullPath(GameServer server, string relativePathTemplate)
		{
			if (string.IsNullOrWhiteSpace(server.InstallPath))
			{
				throw new InvalidOperationException(LocalizationManager.Get(
					"Configuration.Error.InstallPathMissing"));
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
				throw new InvalidDataException(LocalizationManager.Get(
					"Configuration.Error.PathOutsideServer"));
			}

			return fullPath;
		}

		protected static string RequireSingleLine(string? value, string settingName)
		{
			string safeValue = value ?? string.Empty;
			if (safeValue.Contains('\r') || safeValue.Contains('\n') || safeValue.Contains('\0'))
			{
				throw new InvalidDataException(LocalizationManager.Get(
					"Configuration.Error.SingleLineRequired",
					settingName));
			}

			return safeValue;
		}

		protected static string EscapeQuoted(string? value)
		{
			return RequireSingleLine(
				value,
				LocalizationManager.Get("Configuration.ValueLabel.Text"))
				.Replace("\"", "\\\"", StringComparison.Ordinal);
		}

		protected static string EscapeProperty(string? value)
		{
			return RequireSingleLine(
				value,
				LocalizationManager.Get("Configuration.ValueLabel.Property"))
				.Replace("\\", "\\\\", StringComparison.Ordinal);
		}

		protected static bool ValuesMatch(string currentValue, string requestedValue)
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
				throw new InvalidOperationException(LocalizationManager.Get(
					"Configuration.Error.DirectoryUnavailable"));
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

		private static string GetPreservedTemplatePath(string path)
		{
			return path + ".synix.template";
		}

		private static void PreserveInstalledTemplate(string path)
		{
			string templatePath = GetPreservedTemplatePath(path);
			if (File.Exists(templatePath))
			{
				return;
			}

			WriteNewFile(templatePath, File.ReadAllText(path));
		}

		protected ConfigurationApplyResult ReplaceWithTemplates(
			ConfigurationContext context,
			IReadOnlyList<ResetTemplate> templates)
		{
			if (templates.Count == 0)
			{
				return ConfigurationApplyResult.Failure(
					LocalizationManager.Get(
						"Configuration.Apply.ResetTemplateEmpty",
						GameName));
			}

			List<(string TargetPath, string StagedPath, string? RollbackPath, bool Existed)> stagedFiles = [];
			List<(string TargetPath, string? RollbackPath, bool Existed)> replacedFiles = [];

			try
			{
				foreach (ResetTemplate template in templates)
				{
					string targetPath = ResolveFullPath(context.Server, template.RelativePath);
					string? directory = Path.GetDirectoryName(targetPath);
					if (string.IsNullOrWhiteSpace(directory))
					{
						throw new InvalidOperationException(
							LocalizationManager.Get(
								"Configuration.Error.DirectoryUnavailable"));
					}

					Directory.CreateDirectory(directory);
					string stagedPath = Path.Combine(
						directory,
						$".{Path.GetFileName(targetPath)}.{Guid.NewGuid():N}.synix.reset.tmp");
					bool existed = File.Exists(targetPath);
					string? rollbackPath = null;
					stagedFiles.Add((targetPath, stagedPath, rollbackPath, existed));
					File.WriteAllText(
						stagedPath,
						template.Content,
						new UTF8Encoding(false, true));
					_ = ConfigHandler.LoadConfig(stagedPath, template.Format);

					if (existed)
					{
						rollbackPath = Path.Combine(
							directory,
							$".{Path.GetFileName(targetPath)}.{Guid.NewGuid():N}.synix.rollback.tmp");
						stagedFiles[^1] = (targetPath, stagedPath, rollbackPath, existed);
						File.Copy(targetPath, rollbackPath, false);
					}
				}

				foreach ((string targetPath, string stagedPath, string? rollbackPath, bool existed) in stagedFiles)
				{
					replacedFiles.Add((targetPath, rollbackPath, existed));
					if (existed)
					{
						File.Copy(targetPath, targetPath + ".synix.bak", true);
						File.Move(stagedPath, targetPath, true);
					}
					else
					{
						File.Move(stagedPath, targetPath, false);
					}
				}

				bool created = stagedFiles.Any(file => !file.Existed);
				return new ConfigurationApplyResult(
					true,
					true,
					true,
					created,
					created
						? LocalizationManager.Get(
							"Configuration.Apply.RebuiltDefaults",
							GameName)
						: LocalizationManager.Get(
							"Configuration.Apply.ResetDefaults",
							GameName));
			}
			catch (Exception exception)
			{
				for (int index = replacedFiles.Count - 1; index >= 0; index--)
				{
					(string targetPath, string? rollbackPath, bool existed) = replacedFiles[index];
					try
					{
						if (existed && rollbackPath != null && File.Exists(rollbackPath))
						{
							File.Copy(rollbackPath, targetPath, true);
						}
						else if (!existed && File.Exists(targetPath))
						{
							File.Delete(targetPath);
						}
					}
					catch (Exception suppressedException)
					{
						Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException);
					}
				}

				return ConfigurationApplyResult.Failure(
					LocalizationManager.Get(
						"Configuration.Apply.ResetFailed",
						GameName,
						exception.Message));
			}
			finally
			{
				foreach ((string _, string stagedPath, string? rollbackPath, bool _) in stagedFiles)
				{
					try
					{
						if (File.Exists(stagedPath))
						{
							File.Delete(stagedPath);
						}

						if (rollbackPath != null && File.Exists(rollbackPath))
						{
							File.Delete(rollbackPath);
						}
					}
					catch (Exception suppressedException)
					{
						Synix_Control_Panel.SynixEngine.ApplicationLogService.WriteSuppressedException(suppressedException);
					}
				}
			}
		}
	}
}
