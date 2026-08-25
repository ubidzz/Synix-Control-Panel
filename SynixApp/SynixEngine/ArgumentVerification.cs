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
					"Built-in definition",
					false,
					$"No built-in definition was found for {server.Game}."));
				return EmptyArgumentPreview(server, checks);
			}

			checks.Add(new GameArgumentVerificationCheck(
				"Built-in definition",
				true,
				$"Loaded revision {definition.DefinitionRevision}."));

			string installRoot;
			string executablePath;
			try
			{
				ArgumentException.ThrowIfNullOrWhiteSpace(server.InstallPath);
				installRoot = Path.GetFullPath(server.InstallPath);
				executablePath = Path.GetFullPath(Path.Combine(
					installRoot,
					definition.ExeName));
				string relativeExecutable = Path.GetRelativePath(
					installRoot,
					executablePath);
				bool staysInsideInstall = !Path.IsPathRooted(relativeExecutable) &&
					!relativeExecutable.Equals("..", StringComparison.Ordinal) &&
					!relativeExecutable.StartsWith(
						".." + Path.DirectorySeparatorChar,
						StringComparison.Ordinal);
				checks.Add(new GameArgumentVerificationCheck(
					"Launch path containment",
					staysInsideInstall,
					staysInsideInstall
						? "The launch file remains inside the selected server folder."
						: "The definition launch path escapes the selected server folder."));
				if (!staysInsideInstall)
					return EmptyArgumentPreview(server, checks);
			}
			catch (Exception exception) when (exception is ArgumentException or
				NotSupportedException or
				PathTooLongException)
			{
				checks.Add(new GameArgumentVerificationCheck(
					"Launch path",
					false,
					exception.Message));
				return EmptyArgumentPreview(server, checks);
			}

			bool launchFileExists = File.Exists(executablePath);
			checks.Add(new GameArgumentVerificationCheck(
				"Installed launch file",
				launchFileExists,
				launchFileExists
					? executablePath
					: $"The launch file is missing: {executablePath}"));

			bool supportedLauncher = GameLaunchCommandBuilder.TryGetLauncherKind(
				executablePath,
				out _);
			checks.Add(new GameArgumentVerificationCheck(
				"Supported launcher",
				supportedLauncher,
				supportedLauncher
					? "Synix can start this executable or command script safely."
					: $"The launch file type {Path.GetExtension(executablePath)} is not supported."));

			bool extraArgumentsSafe = TryValidateExtraArguments(
				server.ExtraArgs,
				out string extraArgumentsError);
			checks.Add(new GameArgumentVerificationCheck(
				"Extra argument safety",
				extraArgumentsSafe,
				extraArgumentsSafe
					? "No Windows command-injection operators were found."
					: extraArgumentsError));

			if (!TryRevealServerPasswords(
				server,
				out SynixServerPasswords passwords))
			{
				checks.Add(new GameArgumentVerificationCheck(
					"Protected passwords",
					false,
					"Synix could not unlock this server's saved passwords."));
				return CreateArgumentPreview(
					server,
					executablePath,
					string.Empty,
					string.Empty,
					checks);
			}

			checks.Add(new GameArgumentVerificationCheck(
				"Protected passwords",
				true,
				"Saved passwords were unlocked only in memory and will remain hidden in this preview."));

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
				"Complete launch arguments",
				argumentsBuilt,
				argumentsBuilt
					? "The installed server values were inserted into the definition."
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
				"Resolved Synix tags",
				argumentsBuilt && unresolvedTokens.Length == 0,
				!argumentsBuilt
					? "The command must build before its tags can be checked."
					: unresolvedTokens.Length == 0
						? "Every supported Synix argument tag was replaced."
						: $"Unresolved tags: {string.Join(", ", unresolvedTokens)}"));

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
					redirectStandardInput: GameDatabase.IsMinecraft(server.Game));
				processSettingsBuilt = true;
				processSettingsDetails = definition.LaunchBehavior.RunElevated
					? "The command can be passed through the Windows elevation prompt."
					: "The command can be passed directly to the server process.";
			}
			catch (Exception exception) when (exception is ArgumentException or
				NotSupportedException or
				InvalidOperationException)
			{
				processSettingsDetails = exception.Message;
			}
			checks.Add(new GameArgumentVerificationCheck(
				"Process launch construction",
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
