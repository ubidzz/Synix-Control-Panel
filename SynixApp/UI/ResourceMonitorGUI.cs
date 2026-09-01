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
using Synix_Control_Panel.SynixApp.ServerHandler;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Synix_Control_Panel
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
		private readonly Dictionary<int, (double CpuMilliseconds, DateTime SampleTime)> _cpuSamples = new();
		private readonly GameServer? _serverFilter;
		private double _currentTotalCpuPercentage;
		private double _currentTotalRamPercentage;
		private bool _isRefreshing;

		public ResourceMonitorGUI(GameServer? serverFilter = null)
		{
			_serverFilter = serverFilter;
			InitializeComponent();
			if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
				ThemeManager.Apply(this);
			if (_serverFilter != null)
			{
				Text = $"Live Process Details - {_serverFilter.ServerName}";
				lblGridTitle.Text = $"Live Process Details  •  {_serverFilter.ServerName}";
				lblGridSubtitle.Text = "Every launcher, console host, and game process Synix has verified inside this server group.";
			}
			lblActiveServersTitle.Text = "Active Processes";
		}

		protected override void OnShown(EventArgs eventArgs)
		{
			base.OnShown(eventArgs);

			PropertyInfo? doubleBufferedProperty = typeof(DataGridView).GetProperty(
				"DoubleBuffered",
				BindingFlags.NonPublic | BindingFlags.Instance);
			doubleBufferedProperty?.SetValue(resourceGrid, true, null);

			UpdateMetricBars();
			tmrRefresh_Tick(this, EventArgs.Empty);
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
			if (_isRefreshing || IsDisposed || Disposing)
			{
				return;
			}

			_isRefreshing = true;
			resourceGrid.SuspendLayout();

			try
			{
				var serverSnapshot = (_serverFilter == null
					? MainGUI.serverList
					: MainGUI.serverList.Where(server => ReferenceEquals(server, _serverFilter)))
					.ToList();
				var totalUsage = ResourceMonitor.CalculateUsage(serverSnapshot);
				HashSet<int> sampledProcessIds = new();

				foreach (var server in serverSnapshot)
				{
					IReadOnlyList<ServerProcessIdentity> identities =
						Servers.RefreshServerProcessRegistry(server);
					foreach (ServerProcessIdentity identity in identities)
					{
						try
						{
							using Process process = Process.GetProcessById(identity.ProcessId);
							if (process.HasExited)
								continue;

							process.Refresh();
							double currentCpuMilliseconds = process.TotalProcessorTime.TotalMilliseconds;
							DateTime currentTime = DateTime.Now;
							double cpuPercentage = 0;

							if (_cpuSamples.TryGetValue(identity.ProcessId, out var previous))
							{
								double elapsedMilliseconds =
									(currentTime - previous.SampleTime).TotalMilliseconds;
								if (elapsedMilliseconds > 0)
								{
									double usedCpuMilliseconds =
										currentCpuMilliseconds - previous.CpuMilliseconds;
									cpuPercentage = usedCpuMilliseconds /
										(elapsedMilliseconds * Environment.ProcessorCount) * 100.0;
								}
							}
							_cpuSamples[identity.ProcessId] = (currentCpuMilliseconds, currentTime);

							cpuPercentage = Math.Clamp(cpuPercentage, 0, 100);
							double ramGb = process.WorkingSet64 /
								1024.0 / 1024.0 / 1024.0;
							double totalSystemRamGb = GetTotalSystemRamGb();
							double ramPercentage = Math.Clamp(
								ramGb / totalSystemRamGb * 100.0,
								0,
								100);

							int processId = process.Id;
							string executableName = Path.GetFileName(identity.ExecutablePath);
							if (string.IsNullOrWhiteSpace(executableName))
								executableName = process.ProcessName + ".exe";
							string processRole = server.PID == processId ? "Primary" : "Child / worker";
							if (!sampledProcessIds.Add(processId))
								continue;

							if (!_rowsByProcessId.TryGetValue(processId, out DataGridViewRow? row))
							{
								int rowIndex = resourceGrid.Rows.Add();
								row = resourceGrid.Rows[rowIndex];
								row.Cells[colStatus.Index].Style.ForeColor = SuccessColor;
								_rowsByProcessId.Add(processId, row);
							}

							row.SetValues(
								"●  Running",
								server.ServerName,
								processId.ToString(),
								$"{executableName}  •  {processRole}",
								$"{cpuPercentage:N1}%",
								$"{ramGb:N2} GB");
							row.Cells[colExecutable.Index].ToolTipText =
								string.IsNullOrWhiteSpace(identity.ExecutablePath)
									? executableName
									: identity.ExecutablePath;

							row.Cells[colCpuUsage.Index].Tag = cpuPercentage;
							row.Cells[colRamUsage.Index].Tag = ramPercentage;
						}
						catch (InvalidOperationException) { }
						catch (Win32Exception) { }
						catch (ArgumentException) { }
					}
				}

				foreach (int staleProcessId in _rowsByProcessId.Keys
					.Where(processId => !sampledProcessIds.Contains(processId))
					.ToList())
				{
					DataGridViewRow staleRow = _rowsByProcessId[staleProcessId];
					resourceGrid.Rows.Remove(staleRow);
					_rowsByProcessId.Remove(staleProcessId);
					_cpuSamples.Remove(staleProcessId);
				}

				UpdateSummaryCards(totalUsage, sampledProcessIds.Count);
				resourceGrid.ClearSelection();
				resourceGrid.CurrentCell = null;
				resourceGrid.Invalidate();
			}
			catch (InvalidOperationException)
			{
				lblLastUpdated.Text =
					"Server list changed during sampling  •  Retrying automatically";
			}
			finally
			{
				resourceGrid.ResumeLayout();
				_isRefreshing = false;
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
			lblTotalCpuCaption.Text = "Across all managed server processes";
			lblTotalRamValue.Text = $"{totalRamGb:N2} GB";
			lblTotalRamCaption.Text =
				$"{_currentTotalRamPercentage:N1}% of {totalSystemRamGb:N1} GB system memory";
			lblActiveServersValue.Text = runningProcessCount.ToString();
			lblActiveIndicator.ForeColor = runningProcessCount > 0
				? SuccessColor
				: SettingsPalette.DisabledText;
			lblActiveServersCaption.Text = runningProcessCount switch
			{
				0 => "No running server processes detected",
				1 => "1 server process is currently online",
				_ => $"{runningProcessCount} server processes are currently online"
			};

			lblServerCount.Text = runningProcessCount == 1
				? "1 running process"
				: $"{runningProcessCount} running processes";
			lblLastUpdated.Text =
				$"Updated {DateTime.Now:h:mm:ss tt}  •  Auto-refresh every 1 second";

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
				"No running game servers detected",
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
			_rowsByProcessId.Clear();
			_cpuSamples.Clear();
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
