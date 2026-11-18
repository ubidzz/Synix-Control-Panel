using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Synix_Control_Panel.SynixApp.ServerHandler;

/// <summary>Opens a loopback connection without releasing credentials to an unverified process.</summary>
internal static class LocalServerConnectionIdentity
{
    internal static async ValueTask<Stream> ConnectAsync(GameServer server, string host, int port,
        CancellationToken cancellationToken)
    {
        // Never resolve a name through DNS, a proxy, or an HTTP redirect.
        IPAddress[] addresses = host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            ? [IPAddress.Loopback, IPAddress.IPv6Loopback]
            : IPAddress.TryParse(host.Trim('[', ']'), out IPAddress? address) && IPAddress.IsLoopback(address)
                ? [address] : [];
        if (port is < 1 or > 65535 || addresses.Length == 0) throw IdentityError();
        ServerProcessIdentity[] identities = Servers.GetServerProcessSnapshot(server);
        if (identities.Length == 0) throw IdentityError();
        foreach (IPAddress target in addresses)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!ListenerMatches(target, port, identities)) continue;
            Socket socket = new(target.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await socket.ConnectAsync(new IPEndPoint(target, port), cancellationToken).ConfigureAwait(false);
                // Check the accepted connection's reverse endpoint tuple, not just the
                // listener again: the listener could have been replaced during ConnectAsync.
                VerifyConnectedPeer(socket, identities);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }
        throw IdentityError();
    }

    private static bool ListenerMatches(IPAddress address, int port, ServerProcessIdentity[] identities)
    {
        TcpOwner[] listeners = ReadOwners(address.AddressFamily).Where(row => row.State == 2 &&
            row.Local.Port == port && (row.Local.Address.Equals(address) ||
                row.Local.Address.Equals(IPAddress.Any) || row.Local.Address.Equals(IPAddress.IPv6Any))).ToArray();
        return listeners.Length > 0 && listeners.All(row => OwnerMatches(row.ProcessId, identities));
    }

    internal static void VerifyConnectedPeer(Socket socket, ServerProcessIdentity[] identities)
    {
        if (socket.LocalEndPoint is not IPEndPoint local || socket.RemoteEndPoint is not IPEndPoint remote ||
            !IPAddress.IsLoopback(local.Address) || !IPAddress.IsLoopback(remote.Address)) throw IdentityError();
        TcpOwner[] peers = ReadOwners(remote.AddressFamily).Where(row => row.State == 5 &&
            row.Local.Equals(remote) && row.Remote.Equals(local)).ToArray();
        if (peers.Length != 1 || !OwnerMatches(peers[0].ProcessId, identities)) throw IdentityError();
    }

    private static bool OwnerMatches(int processId, ServerProcessIdentity[] identities) =>
        identities.Any(identity => identity.ProcessId == processId && MatchesLiveProcess(identity));

    internal static bool MatchesLiveProcess(ServerProcessIdentity identity)
    {
        try
        {
            if (identity.ProcessId <= 0 || !identity.StartTimeUtc.HasValue ||
                !Path.IsPathFullyQualified(identity.ExecutablePath)) return false;
            using Process process = Process.GetProcessById(identity.ProcessId);
            // Java may be installed outside the server folder. Trust only the exact
            // executable and creation time already recorded for this server's process group.
            return !process.HasExited && process.StartTime.ToUniversalTime() == identity.StartTimeUtc &&
                process.MainModule?.FileName is { } actual &&
                string.Equals(Path.GetFullPath(actual), Path.GetFullPath(identity.ExecutablePath),
                    StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or
            Win32Exception or NotSupportedException or IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static IOException IdentityError() => new(LocalizationManager.Get("Satisfactory.Error.LocalIdentity"));

    private sealed record TcpOwner(int State, IPEndPoint Local, IPEndPoint Remote, int ProcessId);

    private static TcpOwner[] ReadOwners(AddressFamily family)
    {
        bool ipv6 = family == AddressFamily.InterNetworkV6;
        int rowSize = ipv6 ? 56 : 24;
        int length = 0;
        const uint insufficientBuffer = 122;
        // AF_INET / AF_INET6, TCP_TABLE_OWNER_PID_ALL; both listeners and established peers.
        uint result = GetExtendedTcpTable(IntPtr.Zero, ref length, false, ipv6 ? 23 : 2, 5, 0);
        for (int attempt = 0; attempt < 3 && result == insufficientBuffer; attempt++)
        {
            if (length < 4 || length > 4 * 1024 * 1024) break;
            int allocated = length;
            IntPtr table = Marshal.AllocHGlobal(allocated);
            try
            {
                result = GetExtendedTcpTable(table, ref length, false, ipv6 ? 23 : 2, 5, 0);
                if (result == insufficientBuffer) continue;
                if (result != 0 || length > allocated) break;
                int count = Marshal.ReadInt32(table);
                if (count < 0 || count > (Math.Min(length, allocated) - 4) / rowSize) break;
                List<TcpOwner> rows = [];
                for (int index = 0; index < count; index++)
                {
                    IntPtr row = IntPtr.Add(table, 4 + index * rowSize);
                    IPAddress ReadAddress(int offset, int scopeOffset)
                    {
                        byte[] bytes = new byte[ipv6 ? 16 : 4];
                        Marshal.Copy(IntPtr.Add(row, offset), bytes, 0, bytes.Length);
                        return ipv6 ? new IPAddress(bytes, unchecked((uint)Marshal.ReadInt32(row, scopeOffset))) : new IPAddress(bytes);
                    }
                    int ReadPort(int offset) => (Marshal.ReadByte(row, offset) << 8) | Marshal.ReadByte(row, offset + 1);
                    rows.Add(new(Marshal.ReadInt32(row, ipv6 ? 48 : 0),
                        new(ReadAddress(ipv6 ? 0 : 4, 16), ReadPort(ipv6 ? 20 : 8)),
                        new(ReadAddress(ipv6 ? 24 : 12, 40), ReadPort(ipv6 ? 44 : 16)),
                        Marshal.ReadInt32(row, ipv6 ? 52 : 20)));
                }
                return rows.ToArray();
            }
            finally { Marshal.FreeHGlobal(table); }
        }
        throw IdentityError();
    }

    [DllImport("iphlpapi.dll")]
    private static extern uint GetExtendedTcpTable(IntPtr table, ref int length,
        [MarshalAs(UnmanagedType.Bool)] bool sort, int addressFamily, int tableClass, uint reserved);
}
