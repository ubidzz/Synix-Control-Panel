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
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine;

namespace Synix_Control_Panel.SynixApp.Database.GameDefinitions
{
	internal sealed record TrustedPostInstallExecutionResult(
		bool Succeeded,
		bool Changed,
		IReadOnlyList<string> Messages);

	internal static class TrustedPostInstallExecutor
	{
		private static readonly string[] SteamRuntimeFiles =
		[
			"steamclient64.dll",
			"tier0_s64.dll",
			"vstdlib_s64.dll"
		];

		internal static TrustedPostInstallExecutionResult Execute(
			GameServer server,
			string? steamRuntimeDirectory = null)
		{
			ArgumentNullException.ThrowIfNull(server);
			if (string.IsNullOrWhiteSpace(server.InstallPath) ||
				!Directory.Exists(server.InstallPath))
			{
				return new TrustedPostInstallExecutionResult(
					false,
					false,
					[LocalizationManager.Get("PostInstall.InstallFolderUnavailable")]);
			}

			if (!TrustedGameDefinitionCatalog.TryGetPackage(
				server.Game,
				out EmbeddedGamePackage? package) ||
				package == null ||
				package.PostInstallActions.Count == 0)
			{
				return new TrustedPostInstallExecutionResult(true, false, []);
			}

			List<string> messages = [];
			bool changed = false;
			try
			{
				foreach (EmbeddedPostInstallAction action in package.PostInstallActions)
				{
					string targetDirectory = ResolveInsideServerFolder(
						server.InstallPath,
						action.TargetDirectory);
					switch (action.Type)
					{
						case TrustedPostInstallActionType.CopySteamRuntimeFiles:
							changed |= CopySteamRuntimeFiles(
								steamRuntimeDirectory ?? Core.SteamCmdPath,
								targetDirectory,
								messages);
							break;

						case TrustedPostInstallActionType.EnsureDirectory:
							if (!Directory.Exists(targetDirectory))
							{
								Directory.CreateDirectory(targetDirectory);
								changed = true;
							}
							messages.Add(
								LocalizationManager.Get(
									"PostInstall.DirectoryVerified",
									GetDisplayPath(server.InstallPath, targetDirectory)));
							break;

						default:
							throw new InvalidDataException(
								LocalizationManager.Get(
									"PostInstall.ActionUnsupported",
									action.Type));
					}
				}

				return new TrustedPostInstallExecutionResult(true, changed, messages);
			}
			catch (Exception exception)
			{
				messages.Add(exception.Message);
				return new TrustedPostInstallExecutionResult(false, changed, messages);
			}
		}

		private static bool CopySteamRuntimeFiles(
			string sourceDirectory,
			string targetDirectory,
			List<string> messages)
		{
			Directory.CreateDirectory(targetDirectory);
			bool changed = false;
			foreach (string fileName in SteamRuntimeFiles)
			{
				string sourcePath = Path.Combine(sourceDirectory, fileName);
				string targetPath = Path.Combine(targetDirectory, fileName);
				if (!File.Exists(sourcePath))
				{
					messages.Add(LocalizationManager.Get(
						"PostInstall.SteamRuntime.SourceMissing",
						fileName));
					continue;
				}

				if (File.Exists(targetPath))
				{
					messages.Add(LocalizationManager.Get(
						"PostInstall.SteamRuntime.AlreadyPresent",
						fileName));
					continue;
				}

				if (!FileHandler.Copy(
					sourcePath,
					targetDirectory,
					fileName,
					false))
				{
					throw new IOException(LocalizationManager.Get(
						"PostInstall.SteamRuntime.CopyFailed",
						fileName));
				}

				changed = true;
				messages.Add(LocalizationManager.Get(
					"PostInstall.SteamRuntime.Copied",
					fileName));
			}

			return changed;
		}

		private static string ResolveInsideServerFolder(
			string installPath,
			string relativePath)
		{
			string root = Path.GetFullPath(installPath)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string normalized = (relativePath ?? string.Empty)
				.Replace('/', Path.DirectorySeparatorChar)
				.Replace('\\', Path.DirectorySeparatorChar);
			string resolved = Path.GetFullPath(Path.Combine(root, normalized))
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			if (!string.Equals(root, resolved, StringComparison.OrdinalIgnoreCase) &&
				!resolved.StartsWith(
					root + Path.DirectorySeparatorChar,
					StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException(
					LocalizationManager.Get("PostInstall.TargetOutsideInstall"));
			}

			RejectReparsePointTargets(root, resolved);

			return resolved;
		}

		private static void RejectReparsePointTargets(string root, string resolved)
		{
			string current = root;
			string relative = Path.GetRelativePath(root, resolved);
			if (relative == ".")
				return;

			foreach (string segment in relative.Split(
				Path.DirectorySeparatorChar,
				StringSplitOptions.RemoveEmptyEntries))
			{
				current = Path.Combine(current, segment);
				if (!Directory.Exists(current))
					break;
				if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
				{
					throw new InvalidDataException(
						LocalizationManager.Get("PostInstall.LinkedDirectoryBlocked"));
				}
			}
		}

		private static string GetDisplayPath(string installPath, string targetPath)
		{
			string relative = Path.GetRelativePath(installPath, targetPath);
			return relative == "."
				? LocalizationManager.Get("PostInstall.ServerRoot")
				: relative;
		}
	}
}
