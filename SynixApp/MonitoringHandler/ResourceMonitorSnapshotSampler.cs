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
using Synix_Control_Panel.SynixEngine;
using System.ComponentModel;
using System.Diagnostics;

namespace Synix_Control_Panel.SynixApp.MonitoringHandler
{
	internal sealed record ResourceProcessUsage(
		int ProcessId,
		string ServerName,
		string ExecutableName,
		string ProcessRole,
		string ExecutablePath,
		double CpuPercentage,
		double RamGb,
		double RamPercentage);

	internal sealed record ResourceUsageSnapshot(
		ResourceMonitor.ServerUsage TotalUsage,
		IReadOnlyList<ResourceProcessUsage> Processes);

	/// <summary>
	/// Discovers and samples managed processes away from the WinForms UI thread.
	/// Each monitor window owns one sampler so CPU deltas remain independent.
	/// </summary>
	internal sealed class ResourceMonitorSnapshotSampler
	{
		private readonly Dictionary<int, (double CpuMilliseconds, DateTime SampleTime)> _cpuSamples = new();
		private readonly Func<GameServer, IReadOnlyList<ServerProcessIdentity>> _processDiscovery;

		internal ResourceMonitorSnapshotSampler(
			Func<GameServer, IReadOnlyList<ServerProcessIdentity>>? processDiscovery = null)
		{
			_processDiscovery = processDiscovery ??
				(server => Servers.RefreshServerProcessRegistry(server));
		}

		internal Task<ResourceUsageSnapshot> CaptureAsync(
			IReadOnlyList<GameServer> servers,
			double totalSystemRamGb,
			CancellationToken cancellationToken)
		{
			return Task.Run(
				() => Capture(servers, totalSystemRamGb, cancellationToken),
				cancellationToken);
		}

		private ResourceUsageSnapshot Capture(
			IReadOnlyList<GameServer> servers,
			double totalSystemRamGb,
			CancellationToken cancellationToken)
		{
			ResourceMonitor.ServerUsage totalUsage = new();
			List<ResourceProcessUsage> processes = [];
			HashSet<int> discoveredProcessIds = [];
			HashSet<int> activeProcessIds = [];
			DateTime currentTime = DateTime.UtcNow;

			foreach (GameServer server in servers)
			{
				cancellationToken.ThrowIfCancellationRequested();
				IReadOnlyList<ServerProcessIdentity> identities = _processDiscovery(server);
				foreach (ServerProcessIdentity identity in identities)
				{
					cancellationToken.ThrowIfCancellationRequested();
					if (!discoveredProcessIds.Add(identity.ProcessId))
						continue;

					try
					{
						using Process process = Process.GetProcessById(identity.ProcessId);
						if (process.HasExited)
							continue;

						process.Refresh();
						double currentCpuMilliseconds = process.TotalProcessorTime.TotalMilliseconds;
						double cpuPercentage = CalculateCpuPercentage(
							identity.ProcessId,
							currentCpuMilliseconds,
							currentTime);
						double ramGb = process.WorkingSet64 / 1024.0 / 1024.0 / 1024.0;
						double ramPercentage = Math.Clamp(
							ramGb / Math.Max(totalSystemRamGb, 1.0) * 100.0,
							0,
							100);
						string executableName = Path.GetFileName(identity.ExecutablePath);
						if (string.IsNullOrWhiteSpace(executableName))
							executableName = process.ProcessName + ".exe";

						activeProcessIds.Add(process.Id);
						processes.Add(new ResourceProcessUsage(
							process.Id,
							server.ServerName,
							executableName,
							server.PID == process.Id ? "Primary" : "Child / worker",
							identity.ExecutablePath,
							cpuPercentage,
							ramGb,
							ramPercentage));
						totalUsage.TotalCpuPercent += cpuPercentage;
						totalUsage.TotalRamMB += ramGb * 1024.0;
					}
					catch (InvalidOperationException) { }
					catch (Win32Exception) { }
					catch (ArgumentException) { }
				}
			}

			foreach (int staleProcessId in _cpuSamples.Keys
				.Where(processId => !activeProcessIds.Contains(processId))
				.ToList())
			{
				_cpuSamples.Remove(staleProcessId);
			}

			return new ResourceUsageSnapshot(totalUsage, processes);
		}

		private double CalculateCpuPercentage(
			int processId,
			double currentCpuMilliseconds,
			DateTime currentTime)
		{
			double cpuPercentage = 0;
			if (_cpuSamples.TryGetValue(processId, out var previous))
			{
				double elapsedMilliseconds = (currentTime - previous.SampleTime).TotalMilliseconds;
				if (elapsedMilliseconds > 0)
				{
					double usedCpuMilliseconds = currentCpuMilliseconds - previous.CpuMilliseconds;
					cpuPercentage = usedCpuMilliseconds /
						(elapsedMilliseconds * Environment.ProcessorCount) * 100.0;
				}
			}

			_cpuSamples[processId] = (currentCpuMilliseconds, currentTime);
			return Math.Clamp(cpuPercentage, 0, 100);
		}
	}
}
