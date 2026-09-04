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
using Synix_Control_Panel.SynixApp.MonitoringHandler;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.Localization;
using Synix_Control_Panel.SynixEngine;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Synix_Control_Panel.SynixApp.UI.Diagnostics
{
	public partial class ResourceMonitorGUI : Form
	{
		private const uint WdaExcludeFromCapture = 0x00000011;
		private const int WmNcHitTest = 0x0084;
		private const int WmNcLeftButtonDown = 0x00A1;
		private const int HtCaption = 0x0002;
		private const int HtLeft = 10;
		private const int HtRight = 11;
		private const int HtTop = 12;
		private const int HtTopLeft = 13;
		private const int HtTopRight = 14;
		private const int HtBottom = 15;
		private const int HtBottomLeft = 16;
		private const int HtBottomRight = 17;
		private const int DwmWindowCornerPreference = 33;
		private const int DwmRound = 2;
		private const int ResizeBorder = 7;

		private static Color AccentColor => SettingsPalette.Accent;
		private static Color RamColor => SettingsPalette.Ram;
		private static Color SuccessColor => SettingsPalette.Success;
		private static Color WarningColor => SettingsPalette.Warning;
		private static Color DangerColor => SettingsPalette.Danger;
		private static Color TrackColor => SettingsPalette.Divider;

		private readonly Dictionary<int, DataGridViewRow> _rowsByProcessId = new();
		private readonly GameServer? _serverFilter;
		private readonly CancellationTokenSource _refreshCancellation = new();
		private readonly ResourceMonitorSnapshotSampler _snapshotSampler = new();
		private double _currentTotalCpuPercentage;
		private double _currentTotalRamPercentage;
		private bool _isRefreshing;

		public ResourceMonitorGUI(GameServer? serverFilter = null)
		{
			_serverFilter = serverFilter;
			InitializeComponent();
			if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
			{
				ThemeManager.Apply(this);
				LocalizationManager.LanguageChanged += InterfaceLanguageChanged;
				Disposed += (_, _) =>
					LocalizationManager.LanguageChanged -= InterfaceLanguageChanged;
			}
			ApplyLocalizedChrome();
		}

		private void InterfaceLanguageChanged(object? sender, EventArgs eventArgs)
		{
			ApplyLocalizedChrome();
			resourceGrid.Invalidate();
		}

		private void ApplyLocalizedChrome()
		{
			if (_serverFilter != null)
			{
				Text = LocalizationManager.Get(
					"ResourceMonitor.WindowTitleFiltered",
					_serverFilter.ServerName);
				lblGridTitle.Text = LocalizationManager.Get(
					"ResourceMonitor.GridTitleFiltered",
					_serverFilter.ServerName);
				lblGridSubtitle.Text = LocalizationManager.Get(
					"ResourceMonitor.FilteredSubtitle");
			}

			lblActiveServersTitle.Text = LocalizationManager.TranslateKnownText(
				"Active Processes");
		}

		protected override void OnShown(EventArgs eventArgs)
		{
			base.OnShown(eventArgs);

			PropertyInfo? doubleBufferedProperty = typeof(DataGridView).GetProperty(
				"DoubleBuffered",
				BindingFlags.NonPublic | BindingFlags.Instance);
			doubleBufferedProperty?.SetValue(resourceGrid, true, null);

			UpdateMetricBars();
			_ = RefreshAsync();
			tmrRefresh.Start();
		}

		protected override void OnHandleCreated(EventArgs eventArgs)
		{
			base.OnHandleCreated(eventArgs);

			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
			{
				return;
			}

			if (Properties.Settings.Default.PrivacyMode)
			{
				_ = SetWindowDisplayAffinity(Handle, WdaExcludeFromCapture);
			}

			try
			{
				int preference = DwmRound;
				_ = DwmSetWindowAttribute(
					Handle,
					DwmWindowCornerPreference,
					ref preference,
					sizeof(int));
			}
			catch
			{

			}
		}

		protected override void WndProc(ref Message message)
		{
			base.WndProc(ref message);

			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
				message.Msg != WmNcHitTest ||
				WindowState == FormWindowState.Maximized)
			{
				return;
			}

			Point cursor = PointToClient(Cursor.Position);
			bool left = cursor.X <= ResizeBorder;
			bool right = cursor.X >= ClientSize.Width - ResizeBorder;
			bool top = cursor.Y <= ResizeBorder;
			bool bottom = cursor.Y >= ClientSize.Height - ResizeBorder;

			if (left && top) message.Result = (IntPtr)HtTopLeft;
			else if (right && top) message.Result = (IntPtr)HtTopRight;
			else if (left && bottom) message.Result = (IntPtr)HtBottomLeft;
			else if (right && bottom) message.Result = (IntPtr)HtBottomRight;
			else if (left) message.Result = (IntPtr)HtLeft;
			else if (right) message.Result = (IntPtr)HtRight;
			else if (top) message.Result = (IntPtr)HtTop;
			else if (bottom) message.Result = (IntPtr)HtBottom;
		}

		protected override bool ProcessCmdKey(ref Message message, Keys keyData)
		{
			if (keyData == Keys.Escape)
			{
				Close();
				return true;
			}

			return base.ProcessCmdKey(ref message, keyData);
		}

		private void tmrRefresh_Tick(object? sender, EventArgs eventArgs)
		{
			_ = RefreshAsync();
		}

		private async Task RefreshAsync()
		{
			if (_isRefreshing || IsDisposed || Disposing)
			{
				return;
			}

			_isRefreshing = true;
			try
			{
				List<GameServer> allServers = ServerRegistry.Snapshot();
				List<GameServer> serverSnapshot = _serverFilter == null
					? allServers
					: allServers.Where(server => ReferenceEquals(server, _serverFilter)).ToList();
				double totalSystemRamGb = GetTotalSystemRamGb();
				CancellationToken cancellationToken = _refreshCancellation.Token;
				ResourceUsageSnapshot snapshot = await _snapshotSampler.CaptureAsync(
					serverSnapshot,
					totalSystemRamGb,
					cancellationToken);

				if (cancellationToken.IsCancellationRequested || IsDisposed || Disposing)
					return;

				ApplyUsageSnapshot(snapshot);
			}
			catch (OperationCanceledException) when (_refreshCancellation.IsCancellationRequested)
			{
			}
			catch (InvalidOperationException)
			{
				if (!IsDisposed && !Disposing)
				{
					lblLastUpdated.Text =
						"Server list changed during sampling  •  Retrying automatically";
				}
			}
			catch (Exception exception)
			{
				System.Diagnostics.Debug.WriteLine(
					$"Resource Monitor sampling failed: {exception}");
				if (!IsDisposed && !Disposing)
				{
					lblLastUpdated.Text =
						"Resource sampling was delayed  •  Retrying automatically";
				}
			}
			finally
			{
				_isRefreshing = false;
			}
		}

		private void ApplyUsageSnapshot(ResourceUsageSnapshot snapshot)
		{
			resourceGrid.SuspendLayout();
			try
			{
				HashSet<int> sampledProcessIds = snapshot.Processes
					.Select(process => process.ProcessId)
					.ToHashSet();
				foreach (ResourceProcessUsage process in snapshot.Processes)
				{
					if (!_rowsByProcessId.TryGetValue(
						process.ProcessId,
						out DataGridViewRow? row))
					{
						int rowIndex = resourceGrid.Rows.Add();
						row = resourceGrid.Rows[rowIndex];
						row.Cells[colStatus.Index].Style.ForeColor = SuccessColor;
						_rowsByProcessId.Add(process.ProcessId, row);
					}

					row.SetValues(
						LocalizationManager.Get("ResourceMonitor.RowRunning"),
						process.ServerName,
						process.ProcessId.ToString(),
						$"{process.ExecutableName}  •  {process.ProcessRole}",
						$"{process.CpuPercentage:N1}%",
						LocalizationManager.Get(
							"ResourceMonitor.RamValue",
							process.RamGb));
					row.Cells[colExecutable.Index].ToolTipText =
						string.IsNullOrWhiteSpace(process.ExecutablePath)
							? process.ExecutableName
							: process.ExecutablePath;
					row.Cells[colCpuUsage.Index].Tag = process.CpuPercentage;
					row.Cells[colRamUsage.Index].Tag = process.RamPercentage;
				}

				foreach (int staleProcessId in _rowsByProcessId.Keys
					.Where(processId => !sampledProcessIds.Contains(processId))
					.ToList())
				{
					resourceGrid.Rows.Remove(_rowsByProcessId[staleProcessId]);
					_rowsByProcessId.Remove(staleProcessId);
				}

				UpdateSummaryCards(snapshot.TotalUsage, snapshot.Processes.Count);
				resourceGrid.ClearSelection();
				resourceGrid.CurrentCell = null;
				resourceGrid.Invalidate();
			}
			finally
			{
				resourceGrid.ResumeLayout();
			}
		}

		private void UpdateSummaryCards(
			ResourceMonitor.ServerUsage totalUsage,
			int runningProcessCount)
		{
			_currentTotalCpuPercentage = Math.Clamp(
				totalUsage.TotalCpuPercent,
				0,
				100);

			double totalRamGb = totalUsage.TotalRamMB / 1024.0;
			double totalSystemRamGb = GetTotalSystemRamGb();
			_currentTotalRamPercentage = Math.Clamp(
				totalRamGb / totalSystemRamGb * 100.0,
				0,
				100);

			lblTotalCpuValue.Text = $"{_currentTotalCpuPercentage:N1}%";
			lblTotalCpuCaption.Text = LocalizationManager.Get(
				"ResourceMonitor.CpuCaption");
			lblTotalRamValue.Text = LocalizationManager.Get(
				"ResourceMonitor.RamValue",
				totalRamGb);
			lblTotalRamCaption.Text = LocalizationManager.Get(
				"ResourceMonitor.RamCaption",
				_currentTotalRamPercentage,
				totalSystemRamGb);
			lblActiveServersValue.Text = runningProcessCount.ToString();
			lblActiveIndicator.ForeColor = runningProcessCount > 0
				? SuccessColor
				: SettingsPalette.DisabledText;
			lblActiveServersCaption.Text = runningProcessCount switch
			{
				0 => LocalizationManager.Get("ResourceMonitor.Active.None"),
				1 => LocalizationManager.Get("ResourceMonitor.Active.One"),
				_ => LocalizationManager.Get(
					"ResourceMonitor.Active.Many",
					runningProcessCount)
			};

			lblServerCount.Text = runningProcessCount == 1
				? LocalizationManager.Get("ResourceMonitor.ProcessCount.One")
				: LocalizationManager.Get(
					"ResourceMonitor.ProcessCount.Many",
					runningProcessCount);
			lblLastUpdated.Text = LocalizationManager.Get(
				"ResourceMonitor.LastUpdated",
				DateTime.Now);

			pnlRamFill.BackColor = _currentTotalRamPercentage switch
			{
				>= 90 => DangerColor,
				>= 75 => WarningColor,
				_ => RamColor
			};

			UpdateMetricBars();
		}

		private static double GetTotalSystemRamGb()
		{
			double totalRamGb = MainGUI.Instance?.systemTotalRamGb ?? 32.0;
			return totalRamGb > 0 ? totalRamGb : 32.0;
		}

		private void UpdateMetricBars()
		{
			UpdateMetricBar(pnlCpuTrack, pnlCpuFill, _currentTotalCpuPercentage);
			UpdateMetricBar(pnlRamTrack, pnlRamFill, _currentTotalRamPercentage);
		}

		private static void UpdateMetricBar(
			Panel track,
			Panel fill,
			double percentage)
		{
			int availableWidth = Math.Max(0, track.ClientSize.Width);
			fill.Width = (int)Math.Round(
				Math.Clamp(percentage, 0, 100) / 100.0 * availableWidth);
			fill.Height = track.ClientSize.Height;
		}

		private void MetricTrack_SizeChanged(object? sender, EventArgs eventArgs)
		{
			UpdateMetricBars();
		}

		private void resourceGrid_CellPainting(
			object? sender,
			DataGridViewCellPaintingEventArgs eventArgs)
		{
			if (eventArgs.RowIndex < 0 ||
				(eventArgs.ColumnIndex != colCpuUsage.Index &&
				eventArgs.ColumnIndex != colRamUsage.Index))
			{
				return;
			}

			eventArgs.Paint(
				eventArgs.CellBounds,
				DataGridViewPaintParts.Background |
				DataGridViewPaintParts.Border |
				DataGridViewPaintParts.SelectionBackground);

			double percentage = eventArgs.Value == null
				? 0
				: Convert.ToDouble(
					resourceGrid.Rows[eventArgs.RowIndex]
						.Cells[eventArgs.ColumnIndex].Tag ?? 0);
			percentage = Math.Clamp(percentage, 0, 100);

			Rectangle barBounds = new(
				eventArgs.CellBounds.Right - 82,
				eventArgs.CellBounds.Top + (eventArgs.CellBounds.Height - 7) / 2,
				62,
				7);
			Rectangle fillBounds = barBounds;
			fillBounds.Width = (int)Math.Round(
				barBounds.Width * percentage / 100.0);

			using SolidBrush trackBrush = new(TrackColor);
			using SolidBrush fillBrush = new(
				eventArgs.ColumnIndex == colCpuUsage.Index
					? AccentColor
					: RamColor);
			if (eventArgs.Graphics == null)
				return;
			eventArgs.Graphics.FillRectangle(trackBrush, barBounds);
			if (fillBounds.Width > 0)
			{
				eventArgs.Graphics.FillRectangle(fillBrush, fillBounds);
			}

			Rectangle textBounds = new(
				eventArgs.CellBounds.Left + 14,
				eventArgs.CellBounds.Top,
				Math.Max(0, barBounds.Left - eventArgs.CellBounds.Left - 20),
				eventArgs.CellBounds.Height);
			Color textColor = eventArgs.ColumnIndex == colCpuUsage.Index
				? AccentColor
				: RamColor;

			TextRenderer.DrawText(
				eventArgs.Graphics,
				Convert.ToString(eventArgs.FormattedValue) ?? string.Empty,
				resourceGrid.Font,
				textBounds,
				textColor,
				TextFormatFlags.Left |
				TextFormatFlags.VerticalCenter |
				TextFormatFlags.EndEllipsis);

			eventArgs.Handled = true;
		}

		private void resourceGrid_Paint(object? sender, PaintEventArgs eventArgs)
		{
			if (resourceGrid.Rows.Count > 0)
			{
				return;
			}

			Rectangle messageBounds = resourceGrid.ClientRectangle;
			messageBounds.Y += resourceGrid.ColumnHeadersHeight;
			messageBounds.Height -= resourceGrid.ColumnHeadersHeight;

			TextRenderer.DrawText(
				eventArgs.Graphics,
				LocalizationManager.Get("ResourceMonitor.Empty"),
				resourceGrid.Font,
				messageBounds,
				SettingsPalette.MutedText,
				TextFormatFlags.HorizontalCenter |
				TextFormatFlags.VerticalCenter);
		}

		private void btnMinimize_Click(object? sender, EventArgs eventArgs)
		{
			WindowState = FormWindowState.Minimized;
		}

		private void btnClose_Click(object? sender, EventArgs eventArgs)
		{
			Close();
		}

		private void TitleBar_MouseDown(object? sender, MouseEventArgs eventArgs)
		{
			if (eventArgs.Button != MouseButtons.Left)
			{
				return;
			}

			_ = ReleaseCapture();
			_ = SendMessage(Handle, WmNcLeftButtonDown, HtCaption, 0);
		}

		private void ResourceMonitorGUI_FormClosed(object? sender, FormClosedEventArgs eventArgs)
		{
			tmrRefresh.Stop();
			tmrRefresh.Tick -= tmrRefresh_Tick;
			_refreshCancellation.Cancel();
			_rowsByProcessId.Clear();
			resourceGrid.Rows.Clear();
		}

		[DllImport("user32.dll")]
		private static extern uint SetWindowDisplayAffinity(
			IntPtr windowHandle,
			uint affinity);

		[DllImport("user32.dll")]
		private static extern bool ReleaseCapture();

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(
			IntPtr windowHandle,
			int message,
			int wordParameter,
			int longParameter);

		[DllImport("dwmapi.dll")]
		private static extern int DwmSetWindowAttribute(
			IntPtr windowHandle,
			int attribute,
			ref int attributeValue,
			int attributeSize);
	}
}
