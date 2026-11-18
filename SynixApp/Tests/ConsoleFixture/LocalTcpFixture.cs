using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;

// Isolated loopback test server; accepts only the fixture protocols, never executes commands.
internal static class LocalTcpFixture
{
    internal static async Task RunAsync(string address, string protocol, string redirect)
    {
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(25));
        TcpListener listener = new(IPAddress.Parse(address), 0);
        listener.Start();
        Console.WriteLine(((IPEndPoint)listener.LocalEndpoint).Port);
        try
        {
            using TcpClient client = await listener.AcceptTcpClientAsync(timeout.Token);
            using NetworkStream stream = client.GetStream();
            if (protocol == "rcon")
            {
                for (int index = 0; index < 2; index++)
                {
                    byte[] length = new byte[4];
                    await stream.ReadExactlyAsync(length, timeout.Token);
                    int size = BinaryPrimitives.ReadInt32LittleEndian(length);
                    if (size < 10 || size > 4096) throw new InvalidDataException();
                    byte[] packet = new byte[size];
                    await stream.ReadExactlyAsync(packet, timeout.Token);
                    Console.WriteLine(Encoding.UTF8.GetString(packet, 8, size - 10));
                    byte[] body = Encoding.UTF8.GetBytes(index == 0 ? "" : "There are 1 of 20 players online: TestPlayer");
                    byte[] response = new byte[14 + body.Length];
                    BinaryPrimitives.WriteInt32LittleEndian(response, response.Length - 4);
                    packet.AsSpan(0, 4).CopyTo(response.AsSpan(4));
                    BinaryPrimitives.WriteInt32LittleEndian(response.AsSpan(8), index == 0 ? 2 : 0);
                    body.CopyTo(response.AsSpan(12));
                    await stream.WriteAsync(response, timeout.Token);
                }
                return;
            }

            List<byte> header = [];
            byte[] one = new byte[1];
            while (header.Count < 16384)
            {
                if (await stream.ReadAsync(one, timeout.Token) == 0) return;
                header.Add(one[0]);
                if (header.Count >= 4 && Encoding.ASCII.GetString(header.TakeLast(4).ToArray()) == "\r\n\r\n") break;
            }
            Dictionary<string, string> headers = Encoding.ASCII.GetString(header.ToArray()).Split("\r\n")
                .Where(line => line.Contains(':')).Select(line => line.Split(':', 2))
                .ToDictionary(parts => parts[0], parts => parts[1].Trim(), StringComparer.OrdinalIgnoreCase);
            Console.WriteLine(headers.GetValueOrDefault("Authorization", "missing"));
            if (protocol == "redirect")
            {
                await stream.WriteAsync(Encoding.ASCII.GetBytes($"HTTP/1.1 302 Found\r\nLocation: {redirect}\r\nContent-Length: 0\r\nConnection: close\r\n\r\n"), timeout.Token);
                return;
            }
            string accept = Convert.ToBase64String(SHA1.HashData(Encoding.ASCII.GetBytes(
                headers["Sec-WebSocket-Key"] + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
            await stream.WriteAsync(Encoding.ASCII.GetBytes($"HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Accept: {accept}\r\nSec-WebSocket-Protocol: minecraft-v1\r\n\r\n"), timeout.Token);
            using WebSocket socket = WebSocket.CreateFromStream(stream, true, "minecraft-v1", TimeSpan.Zero);
            byte[] buffer = new byte[4096];
            await socket.ReceiveAsync(buffer, timeout.Token);
            await socket.SendAsync(Encoding.UTF8.GetBytes("{\"jsonrpc\":\"2.0\",\"id\":1,\"result\":[{\"id\":\"test-id\",\"name\":\"TestPlayer\"}]}"),
                WebSocketMessageType.Text, true, timeout.Token);
            await Task.Delay(200, timeout.Token);
        }
        finally { listener.Stop(); }
    }
}
