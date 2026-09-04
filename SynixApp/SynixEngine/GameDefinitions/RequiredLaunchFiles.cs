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
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.FileFolderHandler;
using System.Drawing;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class Core
	{
		internal bool EnsureRequiredLaunchFilesAndReport(
			GameServer server,
			bool showDialog)
		{
			GameInfo? game = GameDatabase.GetGame(server.Game);
			if (game == null || game.RequiredLaunchFiles.Length == 0)
			{
				return true;
			}

			RequiredLaunchFileResult result = PrepareRequiredLaunchFiles(
				server,
				game,
				FindExternalDataFolders(game.ExternalDataFolderName));

			foreach (string copiedFile in result.CopiedFiles)
			{
				Log($"[SETUP] Imported {copiedFile} for {server.ServerName}.", Color.Cyan);
			}

			if (result.MissingFiles.Count == 0)
			{
				return true;
			}

			string missingFiles = string.Join(", ", result.MissingFiles);
			string destinations = string.Join(
				Environment.NewLine,
				result.MissingFiles.Select(relativeFile =>
					$"• {Path.GetFullPath(Path.Combine(server.InstallPath, relativeFile))}"));
			string message =
				$"{server.Game} needs these files before its dedicated server can start:\n\n" +
				$"{missingFiles}\n\n" +
				game.LaunchFileSetupInstructions + "\n\n" +
				$"Required destinations:\n{destinations}";

			Log($"[SETUP REQUIRED] Missing {missingFiles}. {game.LaunchFileSetupInstructions}", Color.Orange, true);
			server.Status = StatusManager.GetStatus(ServerState.Stopped);
			FileHandler.SaveServers();
			UpdateGridStatus();

			if (showDialog)
			{
				ApplicationUiService.Invoke(() =>
					LocalizedMessageBox.Show(
						ApplicationUiService.DialogOwner,
						message,
						"Additional Game Files Required",
						MessageBoxButtons.OK,
						MessageBoxIcon.Information));
			}

			return false;
		}

		internal static RequiredLaunchFileResult PrepareRequiredLaunchFiles(
			GameServer server,
			GameInfo game,
			IEnumerable<string> sourceFolders)
		{
			ArgumentNullException.ThrowIfNull(server);
			ArgumentNullException.ThrowIfNull(game);
			ArgumentNullException.ThrowIfNull(sourceFolders);

			Directory.CreateDirectory(server.InstallPath);
			List<string> copiedFiles = [];
			string[] importFiles = game.RequiredLaunchFiles
				.Concat(game.OptionalLaunchFiles)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToArray();

			foreach (string relativeFile in importFiles)
			{
				if (!TryResolveInside(server.InstallPath, relativeFile, out string destination) ||
					File.Exists(destination))
				{
					continue;
				}

				foreach (string sourceFolder in sourceFolders)
				{
					string source = string.Empty;
					bool foundSource =
						TryResolveInside(sourceFolder, relativeFile, out source) &&
						File.Exists(source);
					if (!foundSource)
					{
						foundSource =
							TryResolveInside(
								sourceFolder,
								Path.GetFileName(relativeFile),
								out source) &&
							File.Exists(source);
					}
					if (!foundSource)
					{
						continue;
					}

					string? destinationFolder = Path.GetDirectoryName(destination);
					if (!string.IsNullOrWhiteSpace(destinationFolder))
					{
						Directory.CreateDirectory(destinationFolder);
					}

					File.Copy(source, destination, overwrite: false);
					copiedFiles.Add(relativeFile);
					break;
				}
			}

			string[] missingFiles = game.RequiredLaunchFiles
				.Where(relativeFile =>
					!TryResolveInside(server.InstallPath, relativeFile, out string destination) ||
					!File.Exists(destination))
				.ToArray();

			return new RequiredLaunchFileResult(copiedFiles, missingFiles);
		}

		private static IEnumerable<string> FindExternalDataFolders(string folderName)
		{
			if (string.IsNullOrWhiteSpace(folderName) || Path.IsPathRooted(folderName))
			{
				return [];
			}

			HashSet<string> folders = new(StringComparer.OrdinalIgnoreCase);
			List<string> documentFolders =
			[
				Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents")
			];

			foreach (string variableName in new[] { "OneDrive", "OneDriveConsumer", "OneDriveCommercial" })
			{
				string? oneDrive = Environment.GetEnvironmentVariable(variableName);
				if (!string.IsNullOrWhiteSpace(oneDrive))
				{
					documentFolders.Add(Path.Combine(oneDrive, "Documents"));
				}
			}

			foreach (string documents in documentFolders.Where(path => !string.IsNullOrWhiteSpace(path)))
			{
				if (TryResolveInside(documents, folderName, out string candidate) &&
					Directory.Exists(candidate))
				{
					folders.Add(candidate);
				}
			}

			return folders;
		}

		private static bool TryResolveInside(
			string root,
			string relativePath,
			out string resolvedPath)
		{
			resolvedPath = string.Empty;
			if (string.IsNullOrWhiteSpace(root) ||
				string.IsNullOrWhiteSpace(relativePath) ||
				Path.IsPathRooted(relativePath))
			{
				return false;
			}

			try
			{
				string resolvedRoot = Path.GetFullPath(root)
					.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
					Path.DirectorySeparatorChar;
				string candidate = Path.GetFullPath(Path.Combine(resolvedRoot, relativePath));
				if (!candidate.StartsWith(resolvedRoot, StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}

				resolvedPath = candidate;
				return true;
			}
			catch
			{
				return false;
			}
		}
	}

	internal sealed record RequiredLaunchFileResult(
		IReadOnlyList<string> CopiedFiles,
		IReadOnlyList<string> MissingFiles);
}
