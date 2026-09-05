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
using Synix_Control_Panel.SynixApp.Database.GameDefinitions;

namespace Synix_Control_Panel.SynixApp.Database
{
	public static class GameDatabase
	{
		public static IReadOnlyList<GameInfo> GetGames => games;

		private static readonly IReadOnlyList<GameInfo> games =
			TrustedGameDefinitionCatalog.LoadDefinitions();

		public static IReadOnlyList<GameInfo> GetGameList()
		{
			return games;
		}

		private static readonly Dictionary<string, GameInfo> _gameDict =
			CreateGameIndex();
		private static readonly Dictionary<string, string> _canonicalNames =
			CreateCanonicalNameIndex();
		public static string GetCanonicalGameName(string? gameName)
		{
			string normalizedName = gameName?.Trim() ?? string.Empty;

			return _canonicalNames.TryGetValue(normalizedName, out string? canonicalName)
				? canonicalName
				: normalizedName;
		}

		public static bool IsMinecraft(string? gameName)
		{
			return GameCapabilityResolver.UsesMinecraftLifecycle(GetGame(gameName ?? string.Empty));
		}

		public static GameInfo? GetGame(string gameName)
		{
			string canonicalName = GetCanonicalGameName(gameName);
			return _gameDict.TryGetValue(canonicalName, out GameInfo? game)
				? game
				: null;
		}

		private static Dictionary<string, GameInfo> CreateGameIndex()
		{
			Dictionary<string, GameInfo> index = new(StringComparer.OrdinalIgnoreCase);
			foreach (GameInfo game in games)
			{
				index.Add(game.Game, game);
				foreach (string alias in game.Aliases)
					index.Add(alias, game);
			}

			return index;
		}

		private static Dictionary<string, string> CreateCanonicalNameIndex()
		{
			Dictionary<string, string> index = new(StringComparer.OrdinalIgnoreCase);
			foreach (GameInfo game in games)
			{
				index.Add(game.Game, game.Game);
				foreach (string alias in game.Aliases)
					index.Add(alias, game.Game);
			}

			return index;
		}

		public static ServerProbeProtocol GetProbeProtocol(GameInfo? game)
		{
			if (game == null)
				return ServerProbeProtocol.Tcp;

			if (game.ProbeProtocol != ServerProbeProtocol.Auto)
				return game.ProbeProtocol;

			return game.IsQueryable
				? ServerProbeProtocol.A2S
				: ServerProbeProtocol.Tcp;
		}

		public static bool SupportsManualConnectionTesting(GameInfo? game)
		{
			if (game?.SupportsManualConnectionTesting != true)
				return false;

			if (game.ProbeProtocol == ServerProbeProtocol.Auto)
				return game.IsQueryable;

			return game.ProbeProtocol is
				ServerProbeProtocol.A2S or
				ServerProbeProtocol.RestApi or
				ServerProbeProtocol.Tcp;
		}

		public static bool IsSatisfactory(string? gameName) =>
			GetGame(gameName ?? string.Empty)?.DefinitionId == "satisfactory";

		public static bool SupportsPlayerCountMonitoring(GameInfo? game)
		{
			if (game == null)
				return false;

			return game.DefinitionId == "satisfactory" ||
				game.ControlCapabilities.Players == GamePlayerControllerKind.Minecraft ||
				GetProbeProtocol(game) == ServerProbeProtocol.A2S;
		}

		public static bool IsPlayerTrackingDisabledByCrossplay(GameServer? server)
		{
			if (server == null || !server.CrossplayEnabled)
				return false;

			return GetGame(server.Game)?.CrossplayDisablesPlayerTracking == true;
		}

		public static bool SupportsPlayerCountMonitoring(GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);
			return !IsPlayerTrackingDisabledByCrossplay(server) &&
				SupportsPlayerCountMonitoring(GetGame(server.Game));
		}

		public static bool SupportsPlayerManagement(GameInfo? game)
		{
			return game != null &&
				GetProbeProtocol(game) == ServerProbeProtocol.A2S;
		}

		internal static bool SupportsPlayerManagement(GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);
			if (IsPlayerTrackingDisabledByCrossplay(server))
				return false;

			if (GameCapabilityResolver.UsesMinecraftPlayers(server))
			{
				return MinecraftControlProfile.IsJava(server) &&
					(MinecraftControlProfile.ShouldEnableManagementProtocol(server) ||
					 server.EnableRcon);
			}

			return SupportsPlayerManagement(GetGame(server.Game));
		}

	}
}
