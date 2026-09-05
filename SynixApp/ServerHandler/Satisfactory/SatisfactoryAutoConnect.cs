// PROJECT: Synix Game Server Control Panel
// COPYRIGHT: © 2026 Jason Turner (ubidzz). All Rights Reserved.
using Synix_Control_Panel.SynixApp.Database;

namespace Synix_Control_Panel.SynixApp.ServerHandler.Satisfactory;

internal static class SatisfactoryAutoConnect
{
	internal sealed record Steps(
		Action<GameServer> VerifyEndpoint,
		Func<GameServer, CancellationToken, Task<string>> GenerateToken,
		Func<int, CancellationToken, Task<string>> InspectCertificate,
		Func<int, string, string, CancellationToken, Task<SatisfactoryServerState>> QueryState,
		Func<GameServer, string, string, bool> SaveConnection);

	private static readonly Steps Production = new(
		SatisfactoryLocalApiIdentity.Verify,
		SatisfactoryConsoleTokenReader.GenerateAsync,
		SatisfactoryApiClient.InspectCertificateAsync,
		async (port, token, fingerprint, cancellationToken) =>
		{
			using SatisfactoryApiClient client = new(port, token, fingerprint);
			return await client.QueryStateAsync(cancellationToken);
		},
		(server, token, fingerprint) => SatisfactoryIntegration.SaveConnection(server, token, fingerprint));

	internal static async Task<SatisfactoryServerState> ConnectAsync(GameServer server,
		CancellationToken cancellationToken, Steps? steps = null)
	{
		steps ??= Production;
		int port = server.Port;
		DateTime? started = server.StartTime;
		string install = server.InstallPath;
		string savedToken = server.AuthenticationToken;
		string savedCertificate = server.SatisfactoryCertificateFingerprint;
		void Verify()
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (!GameDatabase.IsSatisfactory(server.Game) || !SatisfactoryIntegration.IsLive(server) ||
				port != server.Port || started != server.StartTime || install != server.InstallPath ||
				savedToken != server.AuthenticationToken || savedCertificate != server.SatisfactoryCertificateFingerprint)
				throw new SatisfactoryApiException(SatisfactoryApiError.Connection);
			steps.VerifyEndpoint(server);
		}
		Verify();
		string token = SatisfactoryApiClient.NormalizeToken(await steps.GenerateToken(server, cancellationToken));
		Verify();
		// The user-selected, live local process owns this loopback listener. Pin the
		// certificate discovered without credentials before sending the fresh token.
		string fingerprint = await steps.InspectCertificate(port, cancellationToken);
		if (!SatisfactoryApiClient.IsFingerprint(fingerprint)) throw new SatisfactoryApiException(SatisfactoryApiError.Certificate);
		Verify();
		SatisfactoryServerState state = await steps.QueryState(port, token, fingerprint, cancellationToken);
		Verify();
		if (!steps.SaveConnection(server, token, fingerprint)) throw new SatisfactoryApiException(SatisfactoryApiError.SaveConnection);
		SatisfactoryIntegration.RecordState(server, state);
		return state;
	}
}
