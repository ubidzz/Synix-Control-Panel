// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.ServerHandler;
using static Synix_Control_Panel.SynixEngine.Core;

namespace Synix_Control_Panel.SynixEngine
{
	internal sealed record ExistingServerDetection(
		GameInfo Game,
		string ExecutablePath,
		string MinecraftEdition = "Java")
	{
		internal string DisplayName => GameDatabase.IsMinecraft(Game.Game)
			? $"Minecraft {MinecraftControlProfile.NormalizeEdition(MinecraftEdition)}"
			: Game.Game;
	}

	internal static class ExistingServerImport
	{
		internal static IReadOnlyList<ExistingServerDetection> Detect(string? folder)
		{
			if (string.IsNullOrWhiteSpace(folder))
				return [];

			string normalizedFolder;
			try
			{
				normalizedFolder = Path.GetFullPath(folder.Trim());
			}
			catch
			{
				return [];
			}

			if (!Directory.Exists(normalizedFolder))
				return [];

			List<ExistingServerDetection> detections = GameDatabase.GetGameList()
				.Where(game => !string.IsNullOrWhiteSpace(game.ExeName))
				.Select(game => new ExistingServerDetection(
					game,
					Path.GetFullPath(Path.Combine(normalizedFolder, game.ExeName))))
				.Where(detection => File.Exists(detection.ExecutablePath))
				.ToList();
			GameInfo? minecraft = GameDatabase.GetGame("Minecraft");
			string bedrockPath = Path.Combine(
				normalizedFolder,
				MinecraftControlProfile.BedrockExecutableName);
			if (minecraft != null && File.Exists(bedrockPath))
			{
				detections.Add(new ExistingServerDetection(
					minecraft,
					Path.GetFullPath(bedrockPath),
					MinecraftControlProfile.BedrockEdition));
			}

			return detections
				.OrderBy(detection => detection.Game.CatalogOrder)
				.ThenBy(detection => detection.Game.Game, StringComparer.OrdinalIgnoreCase)
				.ToArray();
		}

		internal static GameServer Create(
			string folder,
			GameInfo game,
			string serverName,
			int gamePort,
			int queryPort,
			IEnumerable<GameServer> existingServers,
			string minecraftEdition = MinecraftControlProfile.JavaEdition)
		{
			ArgumentNullException.ThrowIfNull(game);
			ArgumentNullException.ThrowIfNull(existingServers);

			string normalizedFolder = Path.GetFullPath(
				folder?.Trim() ?? throw new ArgumentNullException(nameof(folder)));
			if (!Directory.Exists(normalizedFolder))
				throw new DirectoryNotFoundException("Choose the folder that contains the existing server files.");

			GameServer editionProbe = new()
			{
				Game = game.Game,
				MinecraftEdition = minecraftEdition
			};
			string executablePath = Path.GetFullPath(Path.Combine(
				normalizedFolder,
				MinecraftControlProfile.ResolveExecutableName(editionProbe, game)));
			if (!File.Exists(executablePath))
			{
				throw new FileNotFoundException(
					$"Synix could not find the expected {game.Game} server program.",
					executablePath);
			}

			GameServer[] registeredServers = existingServers.ToArray();
			if (registeredServers.Any(server => PathsEqual(server.InstallPath, normalizedFolder)))
				throw new InvalidOperationException("This server folder is already registered in Synix.");

			string requestedName = string.IsNullOrWhiteSpace(serverName)
				? $"Imported {game.Game}"
				: serverName.Trim();
			if (registeredServers.Any(server => server.ServerName.Equals(
				requestedName,
				StringComparison.OrdinalIgnoreCase)))
			{
				throw new InvalidOperationException("A server with this name is already registered in Synix.");
			}

			ValidatePort(gamePort, "game");
			ValidatePort(queryPort, "query");
			if (gamePort == queryPort && gamePort > 0)
				throw new InvalidOperationException("The game and query ports must be different.");
			GameServer? gamePortOwner = registeredServers.FirstOrDefault(server =>
				Core.HasConfiguredPort(server, gamePort));
			if (gamePortOwner != null)
			{
				throw new InvalidOperationException(
					$"The game port {gamePort} is already assigned to '{gamePortOwner.ServerName}'. Choose a unique port.");
			}
			GameServer? queryPortOwner = registeredServers.FirstOrDefault(server =>
				Core.HasConfiguredPort(server, queryPort));
			if (queryPortOwner != null)
			{
				throw new InvalidOperationException(
					$"The query port {queryPort} is already assigned to '{queryPortOwner.ServerName}'. Every server needs a unique query port.");
			}

			return new GameServer
			{
				Game = game.Game,
				MinecraftEdition = GameDatabase.IsMinecraft(game.Game)
					? MinecraftControlProfile.NormalizeEdition(minecraftEdition)
					: MinecraftControlProfile.JavaEdition,
				ServerName = requestedName,
				InstallPath = normalizedFolder,
				Port = gamePort,
				QueryPort = queryPort,
				AppPort = game.AppPort,
				WorldSize = game.WorldSize,
				WorldSeed = game.WorldSeed,
				WorldName = game.Maps.FirstOrDefault() ?? "NewWorld",
				GameMode = game.GameModes.FirstOrDefault() ?? "PVE",
				MaxPlayers = 10,
				RconPort = FindAvailablePort(
					Math.Clamp(queryPort + 1, 1, 65535),
					registeredServers),
				IsDefaultPath = false,
				IsFirstBoot = false,
				PreserveImportedConfiguration = true,
				Status = StatusManager.GetStatus(ServerState.Stopped)
			};
		}

		internal static int FindAvailablePort(
			int preferredPort,
			IEnumerable<GameServer> existingServers)
		{
			HashSet<int> usedPorts = existingServers
				.SelectMany(server => new[]
				{
					server.Port,
					server.QueryPort,
					server.EnableRcon ? server.RconPort : 0,
					server.AppPort ?? 0,
					MinecraftControlProfile.IsJava(server)
						? server.MinecraftManagementPort
						: 0
				})
				.Where(port => port is >= 1 and <= 65535)
				.ToHashSet();

			int start = Math.Clamp(preferredPort, 1, 65535);
			for (int port = start; port <= 65535; port++)
			{
				if (!usedPorts.Contains(port))
					return port;
			}

			for (int port = 1; port < start; port++)
			{
				if (!usedPorts.Contains(port))
					return port;
			}

			throw new InvalidOperationException("No unused network port is available.");
		}

		private static bool PathsEqual(string? first, string second)
		{
			if (string.IsNullOrWhiteSpace(first))
				return false;

			try
			{
				return Path.GetFullPath(first.Trim())
					.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
					.Equals(
						second.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
						StringComparison.OrdinalIgnoreCase);
			}
			catch
			{
				return false;
			}
		}

		private static void ValidatePort(int port, string label)
		{
			if (port is < 1 or > 65535)
				throw new ArgumentOutOfRangeException(label, $"The {label} port must be between 1 and 65535.");
		}
	}
}
