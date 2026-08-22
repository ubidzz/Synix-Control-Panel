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
	internal sealed record GeneratedConfigurationCaptureResult(
		string DestinationRoot,
		int CopiedFiles,
		int UnchangedFiles,
		int MissingFiles,
		IReadOnlyList<string> Errors)
	{
		public bool FoundFiles => CopiedFiles + UnchangedFiles > 0;
	}

	internal static class GeneratedConfigurationCollector
	{
		private const string CaptureFolderName = "Synix Generated Configurations";

		internal static bool AutomaticCollectionEnabled =>
			!Core.IsOfficialRelease &&
			Properties.Settings.Default.CollectGeneratedConfigurationsForDevelopment;

		internal static string DefaultDestinationRoot
		{
			get
			{
				string documents = Environment.GetFolderPath(
					Environment.SpecialFolder.MyDocuments);
				if (string.IsNullOrWhiteSpace(documents))
				{
					documents = Environment.GetFolderPath(
						Environment.SpecialFolder.LocalApplicationData);
				}

				return Path.Combine(documents, CaptureFolderName);
			}
		}

		internal static GeneratedConfigurationCaptureResult Collect(
			IEnumerable<GameServer> servers,
			string? destinationRoot = null)
		{
			ArgumentNullException.ThrowIfNull(servers);

			string captureRoot = Path.GetFullPath(
				string.IsNullOrWhiteSpace(destinationRoot)
					? DefaultDestinationRoot
					: destinationRoot);
			int copiedFiles = 0;
			int unchangedFiles = 0;
			int missingFiles = 0;
			List<string> errors = [];

			foreach (GameServer server in servers)
			{
				GeneratedConfigurationCaptureResult result = CollectServer(
					server,
					captureRoot);
				copiedFiles += result.CopiedFiles;
				unchangedFiles += result.UnchangedFiles;
				missingFiles += result.MissingFiles;
				errors.AddRange(result.Errors);
			}

			return new GeneratedConfigurationCaptureResult(
				captureRoot,
				copiedFiles,
				unchangedFiles,
				missingFiles,
				errors);
		}

		internal static GeneratedConfigurationCaptureResult CollectServer(
			GameServer server,
			string? destinationRoot = null)
		{
			ArgumentNullException.ThrowIfNull(server);

			string captureRoot = Path.GetFullPath(
				string.IsNullOrWhiteSpace(destinationRoot)
					? DefaultDestinationRoot
					: destinationRoot);
			ConfigFileCreationMode creationMode =
				GameFix.GetConfigFileCreationMode(server.Game);
			if (creationMode is ConfigFileCreationMode.SynixTemplate or
				ConfigFileCreationMode.LaunchArgumentsOnly ||
				string.IsNullOrWhiteSpace(server.InstallPath) ||
				!Directory.Exists(server.InstallPath))
			{
				return new GeneratedConfigurationCaptureResult(
					captureRoot,
					0,
					0,
					0,
					[]);
			}

			List<string> errors = [];
			IReadOnlyList<(string Path, ConfigFormat Format)> candidates;
			try
			{
				candidates = GetCandidateFiles(server);
			}
			catch (Exception exception)
			{
				return new GeneratedConfigurationCaptureResult(
					captureRoot,
					0,
					0,
					0,
					[$"{server.Game} / {server.ServerName}: {exception.Message}"]);
			}

			int copiedFiles = 0;
			int unchangedFiles = 0;
			int missingFiles = 0;
			string installRoot = Path.GetFullPath(server.InstallPath)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string gameFolder = MakeSafeFolderName(server.Game, "Game");
			HashSet<string> usedFileNames = new(StringComparer.OrdinalIgnoreCase);

			foreach ((string sourcePath, ConfigFormat format) in candidates)
			{
				if (!File.Exists(sourcePath))
				{
					missingFiles++;
					continue;
				}

				try
				{
					string relativePath = Path.GetRelativePath(installRoot, sourcePath);
					if (relativePath.StartsWith("..", StringComparison.Ordinal) ||
						Path.IsPathRooted(relativePath))
					{
						throw new InvalidDataException(
							"The generated configuration is outside the server installation folder.");
					}

					string destinationFileName = GetFlatDestinationFileName(
						sourcePath,
						usedFileNames);
					string destinationPath = Path.GetFullPath(Path.Combine(
						captureRoot,
						gameFolder,
						destinationFileName));
					EnsureInsideDestination(captureRoot, destinationPath);
					string capturedText = CreateRedactedTemplate(sourcePath, format);
					string? destinationDirectory = Path.GetDirectoryName(destinationPath);
					if (string.IsNullOrWhiteSpace(destinationDirectory))
					{
						throw new InvalidOperationException(
							"The capture destination is unavailable.");
					}

					Directory.CreateDirectory(destinationDirectory);
					if (File.Exists(destinationPath) &&
						string.Equals(
							File.ReadAllText(destinationPath),
							capturedText,
							StringComparison.Ordinal))
					{
						unchangedFiles++;
						continue;
					}

					WriteAtomically(destinationPath, capturedText);
					copiedFiles++;
				}
				catch (Exception exception)
				{
					errors.Add(
						$"{server.Game} / {server.ServerName} / {Path.GetFileName(sourcePath)}: {exception.Message}");
				}
			}

			return new GeneratedConfigurationCaptureResult(
				captureRoot,
				copiedFiles,
				unchangedFiles,
				missingFiles,
				errors);
		}

		private static IReadOnlyList<(string Path, ConfigFormat Format)> GetCandidateFiles(
			GameServer server)
		{
			Dictionary<string, ConfigFormat> files =
				new(StringComparer.OrdinalIgnoreCase);

			if (GameFix.TryGetConfiguration(
				server.Game,
				out ConfigurationDefinition? definition) &&
				definition?.UsesConfigurationFile == true)
			{
				foreach (string path in definition.ResolveConfigurationPaths(server))
				{
					files[path] = definition.Format;
				}
			}

			GameInfo? game = GameDatabase.GetGame(server.Game);
			if (game != null && !string.IsNullOrWhiteSpace(game.RelativeConfigPath))
			{
				files[ResolveDatabasePath(server, game.RelativeConfigPath)] = game.Format;
			}

			return files
				.Select(file => (file.Key, file.Value))
				.ToArray();
		}

		private static string ResolveDatabasePath(
			GameServer server,
			string relativePathTemplate)
		{
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
			if (!fullPath.StartsWith(
				installRoot + Path.DirectorySeparatorChar,
				StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException(
					"The configuration path leaves the server installation folder.");
			}

			return fullPath;
		}

		private static string CreateRedactedTemplate(
			string sourcePath,
			ConfigFormat format)
		{
			if (LooksBinary(sourcePath))
			{
				throw new InvalidDataException(
					"Binary configuration files are not copied as text templates.");
			}

			List<ConfigLine> values = ConfigHandler.LoadConfig(sourcePath, format);
			foreach (ConfigLine value in values.Where(value =>
				value.Type == ConfigValueType.Secret))
			{
				value.Value = GetSecretPlaceholder(value);
			}

			return ConfigHandler.CreatePreview(sourcePath, values, format);
		}

		private static string GetSecretPlaceholder(ConfigLine value)
		{
			string name = $"{value.Path} {value.Key}"
				.Replace("_", string.Empty, StringComparison.Ordinal)
				.Replace("-", string.Empty, StringComparison.Ordinal)
				.Replace(".", string.Empty, StringComparison.Ordinal)
				.ToLowerInvariant();

			if (name.Contains("rcon", StringComparison.Ordinal))
			{
				return "{RCONPassword}";
			}

			if (name.Contains("admin", StringComparison.Ordinal))
			{
				return "{AdminPassword}";
			}

			if (name.Contains("password", StringComparison.Ordinal) ||
				name.Contains("passwd", StringComparison.Ordinal))
			{
				return "{Password}";
			}

			return "{Secret}";
		}

		private static bool LooksBinary(string path)
		{
			using FileStream stream = new(
				path,
				FileMode.Open,
				FileAccess.Read,
				FileShare.ReadWrite | FileShare.Delete);
			Span<byte> buffer = stackalloc byte[4096];
			int length = stream.Read(buffer);
			if (length >= 2 &&
				((buffer[0] == 0xFF && buffer[1] == 0xFE) ||
				 (buffer[0] == 0xFE && buffer[1] == 0xFF)))
			{
				return false;
			}

			return buffer[..length].Contains((byte)0);
		}

		private static string MakeSafeFolderName(string value, string fallback)
		{
			string safeName = (value ?? string.Empty).Trim();
			foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
			{
				safeName = safeName.Replace(invalidCharacter, '-');
			}

			safeName = safeName.TrimEnd(' ', '.');
			return string.IsNullOrWhiteSpace(safeName) ? fallback : safeName;
		}

		private static string GetFlatDestinationFileName(
			string sourcePath,
			ISet<string> usedFileNames)
		{
			string fileName = Path.GetFileName(sourcePath);
			if (usedFileNames.Add(fileName))
			{
				return fileName;
			}

			string parentName = MakeSafeFolderName(
				Path.GetFileName(Path.GetDirectoryName(sourcePath) ?? string.Empty),
				"Config");
			string candidate = $"{parentName}_{fileName}";
			int suffix = 2;
			while (!usedFileNames.Add(candidate))
			{
				candidate = $"{parentName}_{suffix}_{fileName}";
				suffix++;
			}

			return candidate;
		}

		private static void EnsureInsideDestination(
			string destinationRoot,
			string destinationPath)
		{
			string root = Path.GetFullPath(destinationRoot)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			if (!destinationPath.StartsWith(
				root + Path.DirectorySeparatorChar,
				StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException(
					"The capture path leaves the selected destination folder.");
			}
		}

		private static void WriteAtomically(string path, string content)
		{
			string directory = Path.GetDirectoryName(path)
				?? throw new InvalidOperationException(
					"The capture destination is unavailable.");
			string temporaryPath = Path.Combine(
				directory,
				$".{Path.GetFileName(path)}.{Guid.NewGuid():N}.synix.tmp");

			try
			{
				File.WriteAllText(
					temporaryPath,
					content,
					new UTF8Encoding(false, true));
				File.Move(temporaryPath, path, true);
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
