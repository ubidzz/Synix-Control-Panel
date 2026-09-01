// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.ServerHandler;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Synix_Control_Panel.SynixEngine
{
	internal sealed record GamePlayerInfo(
		string Name,
		int Score,
		TimeSpan ConnectedFor);

	internal sealed record PlayerQueryResult(
		bool IsSupported,
		bool IsSuccessful,
		string Message,
		IReadOnlyList<GamePlayerInfo> Players);

	internal static class PlayerQueryService
	{
		private static readonly byte[] PlayerChallengeRequest =
			[0xFF, 0xFF, 0xFF, 0xFF, 0x55, 0xFF, 0xFF, 0xFF, 0xFF];

		internal static async Task<PlayerQueryResult> QueryAsync(
			GameServer server,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(server);
			GameInfo? game = GameDatabase.GetGame(server.Game);
			if (game == null)
				return Unsupported("The game definition is unavailable.");

			if (GameDatabase.IsMinecraft(server.Game))
				return await QueryMinecraftAsync(server, cancellationToken);

			ServerProbeProtocol protocol = GameDatabase.GetProbeProtocol(game);
			if (protocol != ServerProbeProtocol.A2S)
			{
				string message = GameDatabase.IsMinecraft(server.Game)
					? $"Minecraft reports {server.CurrentPlayers} connected player(s), but this server query does not publish player names."
					: "This game's current query protocol does not provide a safe, universal player-name list.";
				return Unsupported(message);
			}

			if (server.Status != Core.StatusManager.GetStatus(Core.ServerState.Running))
				return new(true, false, "Start the server before refreshing player details.", []);

			int port = server.QueryPort > 0 ? server.QueryPort : server.Port;
			try
			{
				using CancellationTokenSource timeout =
					CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
				timeout.CancelAfter(TimeSpan.FromSeconds(3));
				using UdpClient client = new(AddressFamily.InterNetwork);
				client.Connect(IPAddress.Loopback, port);

				await client.SendAsync(PlayerChallengeRequest, timeout.Token);
				UdpReceiveResult received = await client.ReceiveAsync(timeout.Token);
				byte[] response = received.Buffer;
				for (int challengeAttempt = 0; challengeAttempt < 2; challengeAttempt++)
				{
					if (!TryReadPayloadHeader(response, out byte responseType, out int payloadOffset))
						return new(true, false, "The server returned an invalid A2S player response.", []);

					// Some compatible servers accept the legacy -1 challenge and return
					// A2S_PLAYER data immediately. Others require the challenge exchange.
					if (responseType == 0x44)
						break;

					if (responseType != 0x41 || response.Length < payloadOffset + 4)
						return new(true, false, "The server query works, but it did not provide a compatible player list.", []);

					byte[] request = (byte[])PlayerChallengeRequest.Clone();
					response.AsSpan(payloadOffset, 4).CopyTo(request.AsSpan(5, 4));
					await client.SendAsync(request, timeout.Token);
					response = (await client.ReceiveAsync(timeout.Token)).Buffer;
				}

				IReadOnlyList<GamePlayerInfo> players = ParsePlayerResponse(response);
				return new(
					true,
					true,
					players.Count == 0
						? "The server responded and no named players are connected."
						: $"Loaded {players.Count} connected player(s).",
					players);
			}
			catch (OperationCanceledException)
			{
				return new(true, false, $"The player query on UDP port {port} timed out.", []);
			}
			catch (SocketException exception)
			{
				return new(true, false, $"The player query could not connect: {exception.Message}", []);
			}
			catch (Exception exception)
			{
				return new(true, false, $"Player details could not be read: {exception.Message}", []);
			}
		}

		private static async Task<PlayerQueryResult> QueryMinecraftAsync(
			GameServer server,
			CancellationToken cancellationToken)
		{
			if (MinecraftControlProfile.IsBedrock(server))
			{
				return Unsupported(
					$"Minecraft Bedrock reports {server.CurrentPlayers} connected player(s), but its built-in status response does not publish player names.");
			}

			if (server.Status != Core.StatusManager.GetStatus(Core.ServerState.Running))
				return new(true, false, "Start the server before refreshing player details.", []);

			MinecraftManagementResult<IReadOnlyList<MinecraftManagedPlayer>> managed =
				await MinecraftManagementClient.QueryPlayersAsync(server, cancellationToken);
			if (managed.Succeeded && managed.Value != null)
			{
				IReadOnlyList<GamePlayerInfo> players = managed.Value
					.Select(player => new GamePlayerInfo(player.Name, 0, TimeSpan.Zero))
					.ToArray();
				return new(
					true,
					true,
					players.Count == 0
						? "Minecraft's local management service reports no connected players."
						: $"Loaded {players.Count} player(s) through Minecraft's local management service.",
					players);
			}

			MinecraftRconResult rcon = await MinecraftRconClient.ExecuteCommandAsync(
				server,
				"list",
				cancellationToken);
			if (rcon.Succeeded)
			{
				IReadOnlyList<GamePlayerInfo> players = MinecraftRconClient
					.ParsePlayerNames(rcon.Response)
					.Select(name => new GamePlayerInfo(name, 0, TimeSpan.Zero))
					.ToArray();
				return new(
					true,
					true,
					players.Count == 0
						? "Minecraft RCON reports no connected players."
						: $"Loaded {players.Count} player(s) through local Minecraft RCON.",
					players);
			}

			string problem = string.Join(
				" ",
				new[] { managed.Problem, rcon.Problem }
					.Where(value => !string.IsNullOrWhiteSpace(value)));
			return new(
				true,
				false,
				string.IsNullOrWhiteSpace(problem)
					? "Minecraft player details are not available yet."
					: problem,
				[]);
		}

		internal static IReadOnlyList<GamePlayerInfo> ParsePlayerResponse(
			ReadOnlySpan<byte> response)
		{
			if (!TryReadPayloadHeader(response, out byte responseType, out int offset) ||
				responseType != 0x44 ||
				offset >= response.Length)
			{
				throw new InvalidDataException("The server returned an invalid A2S player response.");
			}

			int count = response[offset++];
			List<GamePlayerInfo> players = new(count);
			for (int index = 0; index < count; index++)
			{
				if (offset >= response.Length)
					throw new InvalidDataException("The player response ended unexpectedly.");

				offset++; // Player index supplied by the server.
				int nameEnd = response[offset..].IndexOf((byte)0);
				if (nameEnd < 0)
					throw new InvalidDataException("A player name was not terminated correctly.");

				string name = Encoding.UTF8.GetString(response.Slice(offset, nameEnd));
				offset += nameEnd + 1;
				if (response.Length < offset + 8)
					throw new InvalidDataException("A player record was incomplete.");

				int score = BinaryPrimitives.ReadInt32LittleEndian(response.Slice(offset, 4));
				offset += 4;
				int durationBits = BinaryPrimitives.ReadInt32LittleEndian(response.Slice(offset, 4));
				offset += 4;
				float seconds = BitConverter.Int32BitsToSingle(durationBits);
				players.Add(new(
					string.IsNullOrWhiteSpace(name) ? "Unnamed player" : name,
					score,
					TimeSpan.FromSeconds(Math.Max(0, seconds))));
			}

			return players;
		}

		private static PlayerQueryResult Unsupported(string message) =>
			new(false, false, message, []);

		private static bool TryReadPayloadHeader(
			ReadOnlySpan<byte> response,
			out byte responseType,
			out int payloadOffset)
		{
			responseType = 0;
			payloadOffset = 0;
			if (response.Length < 5 ||
				response[0] != 0xFF || response[1] != 0xFF ||
				response[2] != 0xFF || response[3] != 0xFF)
			{
				return false;
			}

			responseType = response[4];
			payloadOffset = 5;
			return true;
		}
	}
}
