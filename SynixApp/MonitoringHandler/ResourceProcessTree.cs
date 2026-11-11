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
namespace Synix_Control_Panel.SynixApp.MonitoringHandler;

internal sealed record ResourceProcessTreeNode(
	ResourceProcessUsage Process,
	ResourceProcessKey? Parent,
	int Depth,
	bool HasChildren);

/// <summary>Orders already verified processes for display; never establishes process ownership.</summary>
internal static class ResourceProcessTree
{
	internal static IReadOnlyList<ResourceProcessTreeNode> Build(IReadOnlyList<ResourceProcessUsage> processes)
	{
		List<ResourceProcessTreeNode> result = [];
		// Two servers with the same display name must remain separate trees.
		foreach (var serverGroup in processes.GroupBy(process => process.Server)
			.OrderBy(group => group.First().ServerName, StringComparer.CurrentCultureIgnoreCase))
		{
			Dictionary<int, ResourceProcessUsage> members = serverGroup.ToDictionary(process => process.ProcessId);
			Dictionary<int, int?> parents = [];
			foreach (ResourceProcessUsage process in serverGroup)
			{
				// Missing/exited parents become roots. A reused parent PID must not
				// attach an older worker to a newly started, unrelated process.
				parents[process.ProcessId] = process.ParentProcessId is int parentId &&
					parentId != process.ProcessId && members.TryGetValue(parentId, out var parent) &&
					parent.StartTimeUtc <= process.StartTimeUtc ? parentId : null;
			}

			foreach (int processId in members.Keys)
			{
				HashSet<int> ancestors = [processId];
				int current = processId;
				while (parents[current] is int parentId)
				{
					if (!ancestors.Add(parentId))
					{
						parents[current] = null;
						break;
					}
					current = parentId;
				}
			}

			var ordered = serverGroup.OrderByDescending(process => process.IsPrimary)
				.ThenBy(process => process.ExecutableName, StringComparer.OrdinalIgnoreCase)
				.ThenBy(process => process.ProcessId).ToArray();
			var children = ordered.Where(process => parents[process.ProcessId].HasValue)
				.ToLookup(process => parents[process.ProcessId]!.Value);
			Stack<(ResourceProcessUsage Process, int Depth)> pending = new();
			foreach (var root in ordered.Where(process => !parents[process.ProcessId].HasValue).Reverse())
				pending.Push((root, 0));
			while (pending.TryPop(out var entry))
			{
				int processId = entry.Process.ProcessId;
				result.Add(new(entry.Process,
					parents[processId] is int parentId ? members[parentId].Key : null,
					entry.Depth, children.Contains(processId)));
				foreach (var child in children[processId].Reverse())
					pending.Push((child, entry.Depth + 1));
			}
		}
		return result;
	}

	internal static IEnumerable<ResourceProcessTreeNode> VisibleNodes(
		IReadOnlyList<ResourceProcessTreeNode> nodes, IReadOnlySet<ResourceProcessKey> collapsed)
	{
		int? hiddenBelowDepth = null;
		foreach (ResourceProcessTreeNode node in nodes)
		{
			if (hiddenBelowDepth is int depth && node.Depth > depth)
				continue;
			hiddenBelowDepth = node.HasChildren && collapsed.Contains(node.Process.Key) ? node.Depth : null;
			yield return node;
		}
	}
}
