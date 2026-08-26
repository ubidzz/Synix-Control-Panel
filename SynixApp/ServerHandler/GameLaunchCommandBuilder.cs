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
using Synix_Control_Panel.SynixEngine;
using System.Diagnostics;
using System.Net;

namespace Synix_Control_Panel.SynixApp.ServerHandler
{
	internal enum GameLauncherKind
	{
		NativeExecutable,
		WindowsCommandScript
	}

	internal static class GameLaunchCommandBuilder
	{
		internal static string ResolveInvokedAppId(
			GameServer server,
			GameInfo definition,
			string executablePath)
		{
			ArgumentNullException.ThrowIfNull(server);
			ArgumentNullException.ThrowIfNull(definition);

			string invokedAppId = definition.AppID;
			string executableDirectory = Path.GetDirectoryName(executablePath) ?? string.Empty;
			string rootAppIdPath = Path.Combine(server.InstallPath, "steam_appid.txt");
			string executableAppIdPath = Path.Combine(executableDirectory, "steam_appid.txt");
			string appIdPath = rootAppIdPath;

			if (File.Exists(rootAppIdPath))
			{
				appIdPath = rootAppIdPath;
			}
			else if (File.Exists(executableAppIdPath))
			{
				appIdPath = executableAppIdPath;
			}
			else
			{
				try
				{
					appIdPath = Directory.EnumerateFiles(
						server.InstallPath,
						"steam_appid.txt",
						new EnumerationOptions
						{
							RecurseSubdirectories = true,
							IgnoreInaccessible = true,
							MaxRecursionDepth = 15,
							AttributesToSkip = FileAttributes.ReparsePoint
						})
						.FirstOrDefault() ?? rootAppIdPath;
				}
				catch (Exception exception) when (exception is IOException or
					UnauthorizedAccessException or
					DirectoryNotFoundException)
				{
					appIdPath = rootAppIdPath;
				}
			}

			if (!File.Exists(appIdPath))
				return invokedAppId;

			try
			{
				string fileContent = File.ReadLines(appIdPath)
					.FirstOrDefault(line => !string.IsNullOrWhiteSpace(line))
					?.Trim() ?? string.Empty;
				if (fileContent.All(char.IsDigit) && fileContent.Length > 0)
					invokedAppId = fileContent;
			}
			catch (Exception exception) when (exception is IOException or
				UnauthorizedAccessException)
			{
			}

			return invokedAppId;
		}

		internal static SynixServerPasswords CreateRedactedPasswords(
			SynixServerPasswords passwords)
		{
			return new SynixServerPasswords(
				string.IsNullOrEmpty(passwords.ServerPassword) ? string.Empty : "********",
				string.IsNullOrEmpty(passwords.AdminPassword) ? string.Empty : "********",
				string.IsNullOrEmpty(passwords.RconPassword) ? string.Empty : "********");
		}

		internal static bool ShouldHideServerWindow(
			GameInfo definition,
			bool showServerWindowSetting)
		{
			ArgumentNullException.ThrowIfNull(definition);
			return !showServerWindowSetting &&
				!definition.LaunchBehavior.RequiresVisibleWindow;
		}

		internal static bool TryGetLauncherKind(
			string? executablePath,
			out GameLauncherKind launcherKind)
		{
			string extension = Path.GetExtension(executablePath ?? string.Empty);
			if (extension.Equals(".exe", StringComparison.OrdinalIgnoreCase))
			{
				launcherKind = GameLauncherKind.NativeExecutable;
				return true;
			}

			if (extension.Equals(".bat", StringComparison.OrdinalIgnoreCase) ||
				extension.Equals(".cmd", StringComparison.OrdinalIgnoreCase))
			{
				launcherKind = GameLauncherKind.WindowsCommandScript;
				return true;
			}

			launcherKind = default;
			return false;
		}

		internal static bool TryBuildArguments(
			GameServer server,
			GameInfo definition,
			string invokedAppId,
			SynixServerPasswords passwords,
			out string arguments,
			out string errorMessage)
		{
			return TryBuildArguments(
				server,
				definition,
				invokedAppId,
				passwords,
				string.Empty,
				out arguments,
				out errorMessage);
		}

		internal static bool TryBuildArguments(
			GameServer server,
			GameInfo definition,
			string invokedAppId,
			SynixServerPasswords passwords,
			string? publicIp,
			out string arguments,
			out string errorMessage)
		{
			ArgumentNullException.ThrowIfNull(server);
			ArgumentNullException.ThrowIfNull(definition);

			arguments = string.Empty;
			errorMessage = string.Empty;
			if (!Core.TryValidateExtraArguments(server.ExtraArgs, out errorMessage))
				return false;

			bool isMinecraft = GameDatabase.IsMinecraft(server.Game);
			int ramToUse = isMinecraft ? server.MaxRam * 1024 : server.MaxRam;
			string targetAppId = definition.AppID ?? string.Empty;
			string cleanIdentity = Core.Instance.GetSafeName(server.ServerName ?? string.Empty);

			arguments = PreparePublicIpArgument(
				definition.RequiredArgs ?? string.Empty,
				publicIp)
				.Replace("{app_port}", server.AppPort?.ToString() ?? "0")
				.Replace("{seed}", string.IsNullOrWhiteSpace(server.WorldSeed) ? "12345" : server.WorldSeed)
				.Replace("{map}", server.WorldName ?? string.Empty)
				.Replace("{steamAppID}", invokedAppId ?? string.Empty)
				.Replace("{appid}", targetAppId)
				.Replace("{port}", server.Port.ToString())
				.Replace("{query}", server.QueryPort.ToString())
				.Replace("{MaxPlayers}", server.MaxPlayers.ToString())
				.Replace("{pass}", passwords.ServerPassword ?? string.Empty)
				.Replace("{adminpass}", passwords.AdminPassword ?? string.Empty)
				.Replace("{ServerName}", server.ServerName ?? string.Empty)
				.Replace("{InstallPath}", server.InstallPath ?? string.Empty)
				.Replace("{world_size}", server.WorldSize.ToString())
				.Replace("{Identity}", cleanIdentity)
				.Replace("{crossplay}", GameFix.ResolveCrossplayValue(definition, server.CrossplayEnabled))
				.Replace("{crossplay_public_ip}", ResolveCrossplayPublicIp(server.CrossplayEnabled, publicIp))
				.Replace("{ram}", ramToUse.ToString());

			if (isMinecraft &&
				MinecraftMetadataService.NormalizeLoader(server.MinecraftLoader)
					.Equals(MinecraftMetadataService.ForgeLoader, StringComparison.OrdinalIgnoreCase))
			{
				arguments = $"-Xmx{ramToUse}M -Xms{ramToUse}M";
			}

			if (arguments.Contains("{rcon}", StringComparison.Ordinal))
			{
				string formattedRcon = string.Empty;
				if (server.EnableRcon && !string.IsNullOrWhiteSpace(definition.RconSyntax))
				{
					formattedRcon = definition.RconSyntax
						.Replace("{rcon_port}", server.RconPort.ToString())
						.Replace("{rcon_pass}", passwords.RconPassword ?? string.Empty)
						.Replace("{rcon_enabled}", GameFix.ResolveBooleanValue(definition, true))
						.Replace("{adminpass}", passwords.AdminPassword ?? string.Empty)
						.Replace("{steamAppID}", invokedAppId ?? string.Empty);
				}

				arguments = arguments.Replace("{rcon}", formattedRcon);
			}

			if (arguments.Contains("{mode}", StringComparison.Ordinal) &&
				!string.IsNullOrWhiteSpace(server.GameMode))
			{
				arguments = arguments.Replace(
					"{mode}",
					GameFix.ResolveGameModeValue(definition, server.GameMode));
			}

			if (!string.IsNullOrWhiteSpace(server.ExtraArgs))
				arguments = $"{arguments} {server.ExtraArgs.Trim()}";

			arguments = arguments.Replace("  ", " ").Trim();
			return true;
		}

		private static string PreparePublicIpArgument(
			string arguments,
			string? publicIp)
		{
			if (!arguments.Contains("{PublicIP}", StringComparison.Ordinal))
				return arguments;

			string normalized = publicIp?.Trim() ?? string.Empty;
			if (!IPAddress.TryParse(normalized, out IPAddress? address) ||
				address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
			{
				return arguments
					.Replace("-PublicIPForEpic={PublicIP}", string.Empty, StringComparison.OrdinalIgnoreCase)
					.Replace("-publicip={PublicIP}", string.Empty, StringComparison.OrdinalIgnoreCase)
					.Replace("{PublicIP}", string.Empty, StringComparison.Ordinal);
			}

			return arguments.Replace(
				"{PublicIP}",
				address.ToString(),
				StringComparison.Ordinal);
		}

		private static string ResolveCrossplayPublicIp(
			bool crossplayEnabled,
			string? publicIp)
		{
			if (!crossplayEnabled)
				return string.Empty;

			string normalized = publicIp?.Trim() ?? string.Empty;
			if (!IPAddress.TryParse(normalized, out IPAddress? address) ||
				address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
			{
				return string.Empty;
			}

			return $"-PublicIPForEpic={address}";
		}

		internal static ProcessStartInfo CreateProcessStartInfo(
			string executablePath,
			string arguments,
			string workingDirectory,
			bool runElevated,
			bool createNoWindow,
			bool redirectStandardInput)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
			ArgumentException.ThrowIfNullOrWhiteSpace(workingDirectory);

			if (!TryGetLauncherKind(executablePath, out GameLauncherKind launcherKind))
			{
				throw new NotSupportedException(
					$"The launch file type '{Path.GetExtension(executablePath)}' is not supported.");
			}

			ProcessStartInfo startInfo;
			if (launcherKind == GameLauncherKind.WindowsCommandScript)
			{
				startInfo = new ProcessStartInfo
				{
					FileName = GetWindowsCommandProcessorPath(),
					Arguments = BuildCommandProcessorArguments(executablePath, arguments)
				};
			}
			else
			{
				startInfo = new ProcessStartInfo
				{
					FileName = executablePath,
					Arguments = arguments ?? string.Empty
				};
			}

			startInfo.WorkingDirectory = workingDirectory;
			startInfo.UseShellExecute = runElevated;
			startInfo.CreateNoWindow = !runElevated && createNoWindow;
			startInfo.WindowStyle = !runElevated && createNoWindow
				? ProcessWindowStyle.Hidden
				: ProcessWindowStyle.Normal;
			startInfo.RedirectStandardInput = !runElevated && redirectStandardInput;
			if (runElevated)
				startInfo.Verb = "runas";

			return startInfo;
		}

		internal static string BuildCommandProcessorArguments(
			string scriptPath,
			string? arguments)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(scriptPath);
			if (scriptPath.IndexOfAny(['\0', '\r', '\n', '"']) >= 0)
				throw new ArgumentException(
					"The command script path contains unsupported characters.",
					nameof(scriptPath));

			string command = $"\"{scriptPath}\"";
			if (!string.IsNullOrWhiteSpace(arguments))
				command = $"{command} {arguments.Trim()}";

			return $"/d /s /v:off /c \"{command}\"";
		}

		private static string GetWindowsCommandProcessorPath()
		{
			string? commandProcessor = Environment.GetEnvironmentVariable("ComSpec");
			if (!string.IsNullOrWhiteSpace(commandProcessor))
				return commandProcessor;

			return string.IsNullOrWhiteSpace(Environment.SystemDirectory)
				? "cmd.exe"
				: Path.Combine(Environment.SystemDirectory, "cmd.exe");
		}
	}
}
