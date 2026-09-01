// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Database;
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
				UdpReceiveResult challengeResponse = await client.ReceiveAsync(timeout.Token);
				if (!TryReadPayloadHeader(challengeResponse.Buffer, out byte responseType, out int payloadOffset) ||
					responseType != 0x41 ||
					challengeResponse.Buffer.Length < payloadOffset + 4)
				{
					return new(true, false, "The server did not return an A2S player challenge.", []);
				}

				byte[] request = (byte[])PlayerChallengeRequest.Clone();
				challengeResponse.Buffer.AsSpan(payloadOffset, 4).CopyTo(request.AsSpan(5, 4));
				await client.SendAsync(request, timeout.Token);
				UdpReceiveResult response = await client.ReceiveAsync(timeout.Token);
				IReadOnlyList<GamePlayerInfo> players = ParsePlayerResponse(response.Buffer);
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
