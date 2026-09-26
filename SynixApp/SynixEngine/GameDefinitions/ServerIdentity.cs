namespace Synix_Control_Panel.SynixEngine;

public partial class Core
{
	internal static string GetServerIdentity(GameServer server) => Instance.GetSafeName(
		string.IsNullOrWhiteSpace(server.ConfigurationIdentity) ? server.ServerName : server.ConfigurationIdentity);
}
