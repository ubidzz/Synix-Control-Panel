// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.ServerHandler;

namespace Synix_Control_Panel.SynixEngine
{
	public readonly record struct ServerDataMigrationSummary(
		int SourceVersion,
		int TargetVersion,
		int MigratedServerCount,
		int MigratedPasswordServerCount)
	{
		public bool Changed =>
			MigratedServerCount > 0 || MigratedPasswordServerCount > 0;
	}

	public static class ServerDataMigrator
	{
		public const int CurrentVersion = 4;

		public static bool Migrate(GameServer server)
		{
			ArgumentNullException.ThrowIfNull(server);
			if (server.DataSchemaVersion < 0)
				throw new InvalidDataException("The server data schema version is invalid.");
			if (server.DataSchemaVersion > CurrentVersion)
			{
				throw new InvalidDataException(
					$"This server data requires a newer Synix version (schema {server.DataSchemaVersion}).");
			}

			bool changed = false;
			bool wasLegacyValheimCrossplay = string.Equals(
				server.Game?.Trim(),
				"Valheim (Crossplay)",
				StringComparison.OrdinalIgnoreCase);
			while (server.DataSchemaVersion < CurrentVersion)
			{
				switch (server.DataSchemaVersion)
				{
					case 0:
						MigrateToVersionOne(server);
						break;
					case 1:
						MigrateToVersionTwo(server);
						break;
					case 2:
						MigrateToVersionThree(server);
						break;
					case 3:
						MigrateToVersionFour(server, wasLegacyValheimCrossplay);
						break;
					default:
						throw new InvalidDataException(
							$"No migration is available for server data schema {server.DataSchemaVersion}.");
				}

				changed = true;
			}

			return changed;
		}

		private static void MigrateToVersionOne(GameServer server)
		{
			string legacyGameName = server.Game?.Trim() ?? string.Empty;
			server.Game = GameDatabase.GetCanonicalGameName(server.Game);
			// The old catalog stored Bedrock as a separate game name. Preserve that
			// historical meaning before the alias is replaced by its canonical name.
			if (legacyGameName.Equals("Minecraft Bedrock", StringComparison.OrdinalIgnoreCase) &&
				GameCapabilityResolver.UsesMinecraftLifecycle(server))
			{
				server.MinecraftEdition = MinecraftControlProfile.BedrockEdition;
			}

			GameInfo? definition = GameDatabase.GetGame(server.Game);
			if (server.QueryPort <= 0 && definition != null)
				server.QueryPort = definition.QueryPort;

			server.ServerProcesses ??= [];
			server.DiscordWebhookRoutes ??= [];
			server.RestartDays ??= [true, true, true, true, true, true, true];
			server.DataSchemaVersion = 1;
		}

		private static void MigrateToVersionTwo(GameServer server)
		{
			if (GameCapabilityResolver.UsesMinecraftLifecycle(server))
			{
				server.MinecraftEdition =
					MinecraftControlProfile.NormalizeEdition(server.MinecraftEdition);
				server.MinecraftLoader =
					MinecraftMetadataService.NormalizeLoader(server.MinecraftLoader);
				server.GameMode = MinecraftControlProfile.NormalizeGameMode(server.GameMode);
			}

			server.DataSchemaVersion = 2;
		}

		private static void MigrateToVersionThree(GameServer server)
		{
			if (server.RestartDays == null)
			{
				server.RestartDays = [false, false, false, false, false, false, false];
			}
			else if (server.RestartDays.Length != 7)
			{
				bool[] normalizedDays = new bool[7];
				Array.Copy(
					server.RestartDays,
					normalizedDays,
					Math.Min(server.RestartDays.Length, normalizedDays.Length));
				server.RestartDays = normalizedDays;
			}

			server.MaintenanceMaximumDelayMinutes = Math.Clamp(
				server.MaintenanceMaximumDelayMinutes,
				0,
				7 * 24 * 60);
			server.DataSchemaVersion = 3;
		}

		private static void MigrateToVersionFour(
			GameServer server,
			bool wasLegacyValheimCrossplay)
		{
			if (wasLegacyValheimCrossplay)
			{
				server.Game = GameDatabase.GetCanonicalGameName("Valheim (Crossplay)");
			}
			else if (string.Equals(
				GameDatabase.GetCanonicalGameName(server.Game),
				"Valheim",
				StringComparison.OrdinalIgnoreCase))
			{
				// The former standard Valheim definition never emitted -crossplay.
				// Preserve that behavior when it adopts the unified definition.
				server.Game = "Valheim";
				server.CrossplayEnabled = false;
			}

			server.DataSchemaVersion = 4;
		}
	}
}
