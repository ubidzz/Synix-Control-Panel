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
namespace Synix_Control_Panel.SynixEngine
{
	internal enum ServerOperationKind
	{
		Install,
		Start,
		Restart,
		Stop,
		Update,
		Validate,
		Backup,
		Restore,
		Delete
	}

	internal sealed class ServerOperationLease : IDisposable
	{
		private readonly Guid _ownerId;
		private readonly string[] _acquiredResources;
		private readonly bool _ownsContext;
		private bool _disposed;

		internal ServerOperationLease(
			bool acquired,
			string failureReason,
			Guid ownerId,
			string[] acquiredResources,
			bool ownsContext)
		{
			Acquired = acquired;
			FailureReason = failureReason;
			_ownerId = ownerId;
			_acquiredResources = acquiredResources;
			_ownsContext = ownsContext;
		}

		internal bool Acquired { get; }
		internal string FailureReason { get; }

		public void Dispose()
		{
			if (_disposed)
				return;
			_disposed = true;
			ServerOperationCoordinator.Release(
				_ownerId,
				_acquiredResources,
				_ownsContext);
		}
	}

	internal static class ServerOperationCoordinator
	{
		private const string SteamCmdResource = "global:steamcmd";
		private static readonly object SyncRoot = new();
		private static readonly Dictionary<string, ActiveOperation> ActiveResources =
			new(StringComparer.OrdinalIgnoreCase);
		private static readonly AsyncLocal<OperationContext?> CurrentContext = new();

		internal static ServerOperationLease TryBegin(
			GameServer server,
			ServerOperationKind kind)
		{
			ArgumentNullException.ThrowIfNull(server);

			OperationContext? context = CurrentContext.Value;
			bool ownsContext = context == null;
			context ??= new OperationContext(Guid.NewGuid());
			string serverResource = GetServerResource(server);
			string[] requestedResources = UsesSteamCmd(kind)
				? [serverResource, SteamCmdResource]
				: [serverResource];

			lock (SyncRoot)
			{
				foreach (string resource in requestedResources)
				{
					if (ActiveResources.TryGetValue(resource, out ActiveOperation? active) &&
						active.OwnerId != context.OwnerId)
					{
						return new ServerOperationLease(
							false,
							BuildBusyMessage(server, active),
							context.OwnerId,
							[],
							false);
					}
				}

				List<string> acquiredResources = [];
				foreach (string resource in requestedResources)
				{
					if (ActiveResources.ContainsKey(resource))
						continue;
					ActiveResources.Add(
						resource,
						new ActiveOperation(
							context.OwnerId,
							kind,
							server.ServerName));
					acquiredResources.Add(resource);
				}

				if (ownsContext)
					CurrentContext.Value = context;
				return new ServerOperationLease(
					true,
					string.Empty,
					context.OwnerId,
					acquiredResources.ToArray(),
					ownsContext);
			}
		}

		internal static void Release(
			Guid ownerId,
			IReadOnlyList<string> acquiredResources,
			bool ownsContext)
		{
			lock (SyncRoot)
			{
				foreach (string resource in acquiredResources)
				{
					if (ActiveResources.TryGetValue(resource, out ActiveOperation? active) &&
						active.OwnerId == ownerId)
					{
						ActiveResources.Remove(resource);
					}
				}
			}

			if (ownsContext && CurrentContext.Value?.OwnerId == ownerId)
				CurrentContext.Value = null;
		}

		private static string GetServerResource(GameServer server)
		{
			string identity = !string.IsNullOrWhiteSpace(server.InstallPath)
				? Path.GetFullPath(server.InstallPath)
				: $"{server.Game}|{server.ServerName}";
			return "server:" + identity;
		}

		private static bool UsesSteamCmd(ServerOperationKind kind) =>
			kind is ServerOperationKind.Install or
				ServerOperationKind.Update or
				ServerOperationKind.Validate;

		private static string BuildBusyMessage(
			GameServer requestedServer,
			ActiveOperation active)
		{
			if (active.Kind is ServerOperationKind.Install or
				ServerOperationKind.Update or
				ServerOperationKind.Validate)
			{
				return LocalizationManager.Get(
					"ServerOperation.Busy.SteamCmd",
					GetDisplayName(active.Kind),
					active.ServerName);
			}

			return LocalizationManager.Get(
				"ServerOperation.Busy.Server",
				requestedServer.ServerName,
				GetDisplayName(active.Kind));
		}

		private static string GetDisplayName(ServerOperationKind kind) =>
			LocalizationManager.Get($"ServerOperation.Name.{kind}");

		private sealed record ActiveOperation(
			Guid OwnerId,
			ServerOperationKind Kind,
			string ServerName);

		private sealed record OperationContext(Guid OwnerId);
	}
}
