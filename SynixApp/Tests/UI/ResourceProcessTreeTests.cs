using System.Drawing;
using System.Windows.Forms;
using Synix_Control_Panel.SynixApp.MonitoringHandler;
using Synix_Control_Panel.SynixApp.UI.Diagnostics;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class ResourceProcessTreeTests
{
	private static readonly DateTime Started = new(2026, 11, 11, 10, 0, 0, DateTimeKind.Utc);

	[Fact]
	public void ChildrenFollowTheirActualParentEvenWhenPidsAreOutOfOrder()
	{
		GameServer server = new() { ServerName = "Minecraft" };
		var tree = ResourceProcessTree.Build([
			Process(server, 10, 70, "java.exe"),
			Process(server, 5, 10, "worker.exe"),
			Process(server, 70, null, "cmd.exe", primary: true),
			Process(server, 20, 70, "conhost.exe")]);

		Assert.Equal([70, 20, 10, 5], tree.Select(node => node.Process.ProcessId));
		Assert.Equal([0, 1, 1, 2], tree.Select(node => node.Depth));
		Assert.Equal(70, tree[2].Parent!.Value.ProcessId);
		Assert.Equal(10, tree[3].Parent!.Value.ProcessId);
		Assert.Equal([70, 20, 10], ResourceProcessTree.VisibleNodes(tree, new HashSet<ResourceProcessKey> { tree[2].Process.Key })
			.Select(node => node.Process.ProcessId));
		Assert.Single(ResourceProcessTree.VisibleNodes(tree, new HashSet<ResourceProcessKey> { tree[0].Process.Key }));
	}

	[Fact]
	public void MissingParentsAndReusedPidsDoNotHideWorkersOrJoinDifferentServers()
	{
		GameServer first = new() { ServerName = "Same name" };
		GameServer second = new() { ServerName = "Same name" };
		var tree = ResourceProcessTree.Build([
			Process(first, 10, null, "game.exe", primary: true) with { StartTimeUtc = Started.AddMinutes(1) },
			Process(first, 20, 10, "older-worker.exe"),
			Process(first, 30, 999, "orphan.exe"),
			Process(second, 40, 10, "other-server.exe")]);
		Assert.Equal(4, tree.Count);
		Assert.All(tree, node => { Assert.Equal(0, node.Depth); Assert.Null(node.Parent); });
	}

	[Fact]
	public void UnexpectedCyclesStillShowEachProcessOnce()
	{
		GameServer server = new();
		var tree = ResourceProcessTree.Build([
			Process(server, 10, 20, "one.exe"), Process(server, 20, 10, "two.exe"),
			Process(server, 30, 30, "self.exe")]);
		Assert.Equal([10, 20, 30], tree.Select(node => node.Process.ProcessId).Order());
		Assert.All(tree, node => Assert.InRange(node.Depth, 0, 1));
	}

	[Fact]
	public void LiveGridPreservesCollapseAndSelectionWithoutDroppingTotalsAndResetsForNewProcess()
	{
		WorkflowUiTest.Run(() =>
		{
			GameServer server = new() { ServerName = "Community world" };
			ResourceProcessUsage parent = Process(server, 4000, null, "cmd.exe", primary: true);
			ResourceProcessUsage worker = Process(server, 1200, 4000, "java.exe");
			ResourceProcessUsage host = Process(server, 5000, 4000, "conhost.exe");
			ResourceProcessUsage nested = Process(server, 1300, 1200, "worker.exe");
			using ResourceMonitorGUI monitor = new(server);
			DataGridView grid = (DataGridView)monitor.Controls.Find("resourceGrid", true).Single();
			var total = new ResourceMonitor.ServerUsage { TotalCpuPercent = 8.5, TotalRamMB = 3072 };
			ResourceUsageSnapshot snapshot = new(total, [worker, nested, host, parent]);
			WorkflowUiTest.Invoke(monitor, "ApplyUsageSnapshot", snapshot);
			Assert.Equal(4, grid.Rows.Count);
			Assert.Equal("colExecutable", grid.Columns.Cast<DataGridViewColumn>().OrderBy(column => column.DisplayIndex).First().Name);
			int executableColumn = grid.Columns["colExecutable"]!.Index;
			grid.CurrentCell = grid.Rows[0].Cells[executableColumn];
			WorkflowUiTest.Invoke(monitor, "ProcessTreeKeyDown", grid, new KeyEventArgs(Keys.Left));
			Assert.Single(grid.Rows.Cast<DataGridViewRow>());
			WorkflowUiTest.Invoke(monitor, "ApplyUsageSnapshot", snapshot);
			Assert.Single(grid.Rows.Cast<DataGridViewRow>());
			Assert.Equal("4000", grid.CurrentRow!.Cells["colPid"].Value);
			Assert.Equal("4", monitor.Controls.Find("lblActiveServersValue", true).Single().Text);
			Assert.Equal(Synix_Control_Panel.SynixApp.Localization.LocalizationManager.Get("ResourceMonitor.RamValue", 3.0),
				monitor.Controls.Find("lblTotalRamValue", true).Single().Text);

			WorkflowUiTest.Invoke(monitor, "ProcessTreeKeyDown", grid, new KeyEventArgs(Keys.Right));
			Assert.Equal(4, grid.Rows.Count);
			Assert.True(grid.Rows[3].Cells[executableColumn].Style.Padding.Left > grid.Rows[2].Cells[executableColumn].Style.Padding.Left);
			string? preview = Environment.GetEnvironmentVariable("SYNIX_PROCESS_TREE_PREVIEW");
			if (!string.IsNullOrWhiteSpace(preview))
			{
				CreateHandles(monitor);
				monitor.PerformLayout();
				using Bitmap image = new(monitor.Width, monitor.Height);
				monitor.DrawToBitmap(image, monitor.ClientRectangle);
				// WinForms does not paint a hidden DataGridView through its parent.
				// Render it in an unseen viewport, then place it at its original bounds.
				Control originalParent = grid.Parent!;
				Point gridOrigin = grid.Location;
				for (Control? ancestor = grid.Parent; ancestor != null && ancestor != monitor; ancestor = ancestor.Parent)
					gridOrigin.Offset(ancestor.Location);
				Rectangle gridBounds = grid.Bounds;
				AnchorStyles gridAnchor = grid.Anchor;
				grid.Anchor = AnchorStyles.Top | AnchorStyles.Left;
				using (Panel viewport = new() { Size = grid.Size })
				{
					viewport.Controls.Add(grid);
					grid.Bounds = new Rectangle(Point.Empty, gridBounds.Size);
					grid.HorizontalScrollingOffset = 0;
					grid.FirstDisplayedScrollingColumnIndex = executableColumn;
					using Bitmap gridImage = new(grid.Width, grid.Height);
					grid.DrawToBitmap(gridImage, grid.ClientRectangle);
					using Graphics graphics = Graphics.FromImage(image);
					graphics.DrawImageUnscaled(gridImage, gridOrigin);
					originalParent.Controls.Add(grid);
					grid.Bounds = gridBounds;
					grid.Anchor = gridAnchor;
				}
				image.Save(preview);
			}

			WorkflowUiTest.Invoke(monitor, "ProcessTreeCellClick", grid, new DataGridViewCellEventArgs(executableColumn, 0));
			Assert.Single(grid.Rows.Cast<DataGridViewRow>());
			ResourceUsageSnapshot restarted = new(total, [parent with { StartTimeUtc = Started.AddMinutes(2) },
				worker with { StartTimeUtc = Started.AddMinutes(3) }]);
			WorkflowUiTest.Invoke(monitor, "ApplyUsageSnapshot", restarted);
			Assert.Equal(2, grid.Rows.Count);
			WorkflowUiTest.Invoke(monitor, "ApplyUsageSnapshot", new ResourceUsageSnapshot(default, []));
			Assert.Empty(grid.Rows.Cast<DataGridViewRow>());
			Assert.Equal("0", monitor.Controls.Find("lblActiveServersValue", true).Single().Text);
		});
	}

	private static void CreateHandles(Control control)
	{
		_ = control.Handle;
		foreach (Control child in control.Controls) CreateHandles(child);
	}

	private static ResourceProcessUsage Process(GameServer server, int pid, int? parent, string executable, bool primary = false) =>
		new(pid, Started, parent, server, server.ServerName, executable, primary,
			Path.Combine(@"C:\SynixPreview", executable), 2.125, 0.75, 2.3);
}
