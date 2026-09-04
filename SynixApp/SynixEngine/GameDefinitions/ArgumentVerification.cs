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
using Synix_Control_Panel.SynixApp.Database.GameDefinitions;
using Synix_Control_Panel.SynixApp.ServerHandler;

namespace Synix_Control_Panel.SynixEngine
{
	public sealed record GameArgumentVerificationCheck(
		string Name,
		bool Passed,
		string Details);

	public sealed record GameArgumentTestPreview(
		string Game,
		string ServerName,
		string ExecutablePath,
		string WorkingDirectory,
		string SanitizedArguments,
		string SanitizedCommand,
		string InvokedAppId,
		IReadOnlyList<GameArgumentVerificationCheck> Checks)
	{
		public bool IsValid => Checks.Count > 0 && Checks.All(check => check.Passed);
	}

	public partial class Core
	{
		public static GameArgumentTestPreview BuildGameArgumentTestPreview(
			GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);

			List<GameArgumentVerificationCheck> checks = [];
			GameInfo? definition = GameDatabase.GetGame(server.Game);
			if (definition == null)
			{
				checks.Add(new GameArgumentVerificationCheck(
					LocalizationManager.Get(
						"GameDefinitions.ArgumentCheck.Definition.Name"),
					false,
					LocalizationManager.Get(
						"GameDefinitions.ArgumentCheck.Definition.Missing",
						server.Game)));
				return EmptyArgumentPreview(server, checks);
			}

			checks.Add(new GameArgumentVerificationCheck(
				LocalizationManager.Get(
					"GameDefinitions.ArgumentCheck.Definition.Name"),
				true,
				LocalizationManager.Get(
					"GameDefinitions.ArgumentCheck.Definition.Loaded",
					definition.DefinitionRevision)));

			string installRoot;
			string executablePath;
			try
			{
				ArgumentException.ThrowIfNullOrWhiteSpace(server.InstallPath);
				installRoot = Path.GetFullPath(server.InstallPath);
				executablePath = Path.GetFullPath(
					GameLaunchCommandBuilder.ResolveExecutablePath(server, definition));
				string relativeExecutable = Path.GetRelativePath(
					installRoot,
					executablePath);
				bool staysInsideInstall = !Path.IsPathRooted(relativeExecutable) &&
					!relativeExecutable.Equals("..", StringComparison.Ordinal) &&
					!relativeExecutable.StartsWith(
						".." + Path.DirectorySeparatorChar,
						StringComparison.Ordinal);
				checks.Add(new GameArgumentVerificationCheck(
					LocalizationManager.Get(
						"GameDefinitions.ArgumentCheck.PathContainment.Name"),
					staysInsideInstall,
					staysInsideInstall
						? LocalizationManager.Get(
							"GameDefinitions.ArgumentCheck.PathContainment.Safe")
						: LocalizationManager.Get(
							"GameDefinitions.ArgumentCheck.PathContainment.Escapes")));
				if (!staysInsideInstall)
					return EmptyArgumentPreview(server, checks);
			}
			catch (Exception exception) when (exception is ArgumentException or
				NotSupportedException or
				PathTooLongException)
			{
				checks.Add(new GameArgumentVerificationCheck(
					LocalizationManager.Get(
						"GameDefinitions.ArgumentCheck.LaunchPath.Name"),
					false,
					exception.Message));
				return EmptyArgumentPreview(server, checks);
			}

			bool launchFileExists = File.Exists(executablePath);
			checks.Add(new GameArgumentVerificationCheck(
				LocalizationManager.Get(
					"GameDefinitions.ArgumentCheck.InstalledFile.Name"),
				launchFileExists,
				launchFileExists
					? executablePath
					: LocalizationManager.Get(
						"GameDefinitions.ArgumentCheck.InstalledFile.Missing",
						executablePath)));

			bool supportedLauncher = GameLaunchCommandBuilder.TryGetLauncherKind(
				executablePath,
				out _);
			checks.Add(new GameArgumentVerificationCheck(
				LocalizationManager.Get(
					"GameDefinitions.ArgumentCheck.Launcher.Name"),
				supportedLauncher,
				supportedLauncher
					? LocalizationManager.Get(
						"GameDefinitions.ArgumentCheck.Launcher.Supported")
					: LocalizationManager.Get(
						"GameDefinitions.ArgumentCheck.Launcher.Unsupported",
						Path.GetExtension(executablePath))));

			bool extraArgumentsSafe = TryValidateExtraArguments(
				server.ExtraArgs,
				out string extraArgumentsError);
			checks.Add(new GameArgumentVerificationCheck(
				LocalizationManager.Get(
					"GameDefinitions.ArgumentCheck.ExtraArguments.Name"),
				extraArgumentsSafe,
				extraArgumentsSafe
					? LocalizationManager.Get(
						"GameDefinitions.ArgumentCheck.ExtraArguments.Safe")
					: extraArgumentsError));

			if (!TryRevealServerPasswords(
				server,
				out SynixServerPasswords passwords))
			{
				checks.Add(new GameArgumentVerificationCheck(
					LocalizationManager.Get(
						"GameDefinitions.ArgumentCheck.Passwords.Name"),
					false,
					LocalizationManager.Get(
						"GameDefinitions.ArgumentCheck.Passwords.UnlockFailed")));
				return CreateArgumentPreview(
					server,
					executablePath,
					string.Empty,
					string.Empty,
					checks);
			}

			checks.Add(new GameArgumentVerificationCheck(
				LocalizationManager.Get(
					"GameDefinitions.ArgumentCheck.Passwords.Name"),
				true,
				LocalizationManager.Get(
					"GameDefinitions.ArgumentCheck.Passwords.Hidden")));

			string invokedAppId = GameLaunchCommandBuilder.ResolveInvokedAppId(
				server,
				definition,
				executablePath);
			bool argumentsBuilt = GameLaunchCommandBuilder.TryBuildArguments(
				server,
				definition,
				invokedAppId,
				passwords,
				out string arguments,
				out string argumentError);
			checks.Add(new GameArgumentVerificationCheck(
				LocalizationManager.Get(
					"GameDefinitions.ArgumentCheck.CompleteArguments.Name"),
				argumentsBuilt,
				argumentsBuilt
					? LocalizationManager.Get(
						"GameDefinitions.ArgumentCheck.CompleteArguments.Inserted")
					: argumentError));

			string sanitizedArguments = string.Empty;
			if (argumentsBuilt)
			{
				_ = GameLaunchCommandBuilder.TryBuildArguments(
					server,
					definition,
					invokedAppId,
					GameLaunchCommandBuilder.CreateRedactedPasswords(passwords),
					out sanitizedArguments,
					out _);
			}

			string[] unresolvedTokens = GameDefinitionArgumentTags.LaunchArguments
				.Concat(GameDefinitionArgumentTags.RconSyntax)
				.Select(tag => tag.Token)
				.Distinct(StringComparer.Ordinal)
				.Where(token => arguments.Contains(token, StringComparison.Ordinal))
				.ToArray();
			checks.Add(new GameArgumentVerificationCheck(
				LocalizationManager.Get(
					"GameDefinitions.ArgumentCheck.Tags.Name"),
				argumentsBuilt && unresolvedTokens.Length == 0,
				!argumentsBuilt
					? LocalizationManager.Get(
						"GameDefinitions.ArgumentCheck.Tags.BuildFirst")
					: unresolvedTokens.Length == 0
						? LocalizationManager.Get(
							"GameDefinitions.ArgumentCheck.Tags.Resolved")
						: LocalizationManager.Get(
							"GameDefinitions.ArgumentCheck.Tags.Unresolved",
							string.Join(", ", unresolvedTokens))));

			bool processSettingsBuilt = false;
			string processSettingsDetails;
			try
			{
				string workingDirectory = Path.GetDirectoryName(executablePath) ?? installRoot;
				_ = GameLaunchCommandBuilder.CreateProcessStartInfo(
					executablePath,
					arguments,
					workingDirectory,
					definition.LaunchBehavior.RunElevated,
					createNoWindow: true,
					redirectStandardInput: GameCapabilityResolver.UsesMinecraftConsole(server));
				processSettingsBuilt = true;
				processSettingsDetails = definition.LaunchBehavior.RunElevated
					? LocalizationManager.Get(
						"GameDefinitions.ArgumentCheck.Process.Elevated")
					: LocalizationManager.Get(
						"GameDefinitions.ArgumentCheck.Process.Direct");
			}
			catch (Exception exception) when (exception is ArgumentException or
				NotSupportedException or
				InvalidOperationException)
			{
				processSettingsDetails = exception.Message;
			}
			checks.Add(new GameArgumentVerificationCheck(
				LocalizationManager.Get(
					"GameDefinitions.ArgumentCheck.Process.Name"),
				processSettingsBuilt,
				processSettingsDetails));

			return CreateArgumentPreview(
				server,
				executablePath,
				sanitizedArguments,
				invokedAppId,
				checks);
		}

		private static GameArgumentTestPreview EmptyArgumentPreview(
			GameServer server,
			IReadOnlyList<GameArgumentVerificationCheck> checks)
		{
			return new GameArgumentTestPreview(
				server.Game,
				server.ServerName,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				checks);
		}

		private static GameArgumentTestPreview CreateArgumentPreview(
			GameServer server,
			string executablePath,
			string sanitizedArguments,
			string invokedAppId,
			IReadOnlyList<GameArgumentVerificationCheck> checks)
		{
			string workingDirectory = Path.GetDirectoryName(executablePath) ?? string.Empty;
			string command = executablePath.Length == 0
				? string.Empty
				: $"\"{executablePath}\"";
			if (!string.IsNullOrWhiteSpace(sanitizedArguments))
				command += " " + sanitizedArguments;

			return new GameArgumentTestPreview(
				server.Game,
				server.ServerName,
				executablePath,
				workingDirectory,
				sanitizedArguments,
				command,
				invokedAppId,
				checks);
		}
	}
}
