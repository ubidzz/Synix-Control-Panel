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
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixEngine;
using System.ComponentModel;

namespace Synix_Control_Panel
{
	public sealed record DiscordSettingsSnapshot(
		bool MasterEnabled,
		string MasterWebhook,
		DiscordNotificationEvent MasterEvents,
		IReadOnlyList<DiscordWebhookRoute> Routes);

	public partial class DiscordSettingsPage : UserControl
	{
		private readonly List<DiscordWebhookRoute> _routes = [];
		private bool _loading;
		private bool _privacyMode;
		private string _serverName = "Test Server";

		public event EventHandler? SettingsChanged;

		public DiscordSettingsPage()
		{
			InitializeComponent();
			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime || DesignMode)
				return;

			ConfigureGrid();
			LoadMasterEventOptions(DiscordNotificationEvent.All);
			cmbMasterPreset.SelectedIndex = 0;
			UpdateControlState();
		}

		public void LoadSettings(
			bool masterEnabled,
			string masterWebhook,
			DiscordNotificationEvent masterEvents,
			IEnumerable<DiscordWebhookRoute>? routes)
		{
			_loading = true;
			chkMasterEnabled.Checked = masterEnabled;
			txtMasterWebhook.Text = masterWebhook ?? string.Empty;
			LoadMasterEventOptions(masterEvents);
			_routes.Clear();
			_routes.AddRange((routes ?? []).Select(CloneRoute));
			RefreshRouteGrid();
			_loading = false;
			UpdateControlState();
		}

		public void SetServerName(string? serverName)
		{
			_serverName = string.IsNullOrWhiteSpace(serverName)
				? "Test Server"
				: serverName.Trim();
		}

		public void SetPrivacyMode(bool privacyMode)
		{
			_privacyMode = privacyMode;
			txtMasterWebhook.UseSystemPasswordChar = privacyMode;
		}

		public void SetEditingEnabled(bool enabled)
		{
			cardMaster.Enabled = enabled;
			cardAdvanced.Enabled = enabled;
			UpdateControlState();
		}

		public void ClearSecrets()
		{
			txtMasterWebhook.Clear();
			foreach (DiscordWebhookRoute route in _routes)
				route.WebhookUrl = string.Empty;
		}

		public bool TryGetSettings(
			out DiscordSettingsSnapshot settings,
			out string error)
		{
			error = string.Empty;
			string masterWebhook = txtMasterWebhook.Text.Trim();
			DiscordNotificationEvent masterEvents = GetMasterEvents();
			if (chkMasterEnabled.Checked)
			{
				if (!Core.TryValidateDiscordWebhookUrl(masterWebhook, out Uri? normalized, out error))
				{
					settings = EmptySnapshot();
					return false;
				}
				if (masterEvents == DiscordNotificationEvent.None)
				{
					error = "Select at least one event for the master Discord webhook.";
					settings = EmptySnapshot();
					return false;
				}
				masterWebhook = normalized!.AbsoluteUri;
			}

			foreach (DiscordWebhookRoute route in _routes)
			{
				if (!Core.TryValidateDiscordWebhookUrl(route.WebhookUrl, out _, out string routeError))
				{
					error = $"{route.Name}: {routeError}";
					settings = EmptySnapshot();
					return false;
				}
				if (route.Events == DiscordNotificationEvent.None)
				{
					error = $"{route.Name} must have at least one selected event.";
					settings = EmptySnapshot();
					return false;
				}
			}

			settings = new DiscordSettingsSnapshot(
				chkMasterEnabled.Checked,
				masterWebhook,
				masterEvents,
				_routes.Select(CloneRoute).ToArray());
			return true;
		}

		private static DiscordSettingsSnapshot EmptySnapshot() =>
			new(false, string.Empty, DiscordNotificationEvent.All, []);

		private void ConfigureGrid()
		{
			gridRoutes.AutoGenerateColumns = false;
			gridRoutes.Columns.Clear();
			gridRoutes.Columns.Add(new DataGridViewTextBoxColumn
			{
				Name = "EnabledColumn",
				HeaderText = "STATUS",
				Width = 95
			});
			gridRoutes.Columns.Add(new DataGridViewTextBoxColumn
			{
				Name = "NameColumn",
				HeaderText = "DESTINATION",
				Width = 220
			});
			gridRoutes.Columns.Add(new DataGridViewTextBoxColumn
			{
				Name = "EventsColumn",
				HeaderText = "EVENTS",
				AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
			});
			gridRoutes.Columns.Add(new DataGridViewTextBoxColumn
			{
				Name = "WebhookColumn",
				HeaderText = "WEBHOOK",
				Width = 175
			});

			GridStyler.DarkTheme(gridRoutes);
			GridStyler.ApplyDashboardTheme(gridRoutes);
			GridStyler.ApplyRoundedCorners(gridRoutes, 10);
			gridRoutes.ColumnHeadersHeight = 40;
			gridRoutes.RowTemplate.Height = 44;
			gridRoutes.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
			typeof(DataGridView).InvokeMember(
				"DoubleBuffered",
				System.Reflection.BindingFlags.NonPublic |
				System.Reflection.BindingFlags.Instance |
				System.Reflection.BindingFlags.SetProperty,
				null,
				gridRoutes,
				[true]);
		}

		private void LoadMasterEventOptions(DiscordNotificationEvent selected)
		{
			lstMasterEvents.Items.Clear();
			foreach (DiscordNotificationOption option in Core.GetDiscordNotificationOptions())
			{
				int index = lstMasterEvents.Items.Add(new EventItem(option));
				lstMasterEvents.SetItemChecked(index, (selected & option.Event) != 0);
			}
			UpdateMasterSummary();
		}

		private DiscordNotificationEvent GetMasterEvents()
		{
			DiscordNotificationEvent selected = DiscordNotificationEvent.None;
			foreach (EventItem item in lstMasterEvents.CheckedItems)
				selected |= item.Option.Event;
			return selected;
		}

		private void RefreshRouteGrid()
		{
			string? selectedId = gridRoutes.SelectedRows.Count > 0
				? (gridRoutes.SelectedRows[0].Tag as DiscordWebhookRoute)?.Id
				: null;
			gridRoutes.Rows.Clear();
			foreach (DiscordWebhookRoute route in _routes)
			{
				int index = gridRoutes.Rows.Add(
					route.Enabled ? "Enabled" : "Paused",
					route.Name,
					Core.SummarizeDiscordEvents(route.Events),
					"••••••••••••");
				DataGridViewRow row = gridRoutes.Rows[index];
				row.Tag = route;
				row.Height = 44;
				row.Cells[0].Style.ForeColor = route.Enabled
					? SettingsPalette.Success
					: SettingsPalette.MutedText;
				row.Cells[0].Style.SelectionForeColor = route.Enabled
					? SettingsPalette.Success
					: SettingsPalette.MutedText;
				row.Cells[2].ToolTipText = BuildDiscordEventList(route.Events);
				row.Cells[3].ToolTipText = MaskDiscordWebhook(route.WebhookUrl);
				if (route.Id == selectedId)
					row.Selected = true;
			}
			if (gridRoutes.SelectedRows.Count == 0 && gridRoutes.Rows.Count > 0)
				gridRoutes.Rows[0].Selected = true;
			UpdateRouteButtons();
		}

		private DiscordWebhookRoute? SelectedRoute =>
			gridRoutes.SelectedRows.Count == 0
				? null
				: gridRoutes.SelectedRows[0].Tag as DiscordWebhookRoute;

		private void UpdateRouteButtons()
		{
			bool isEnabled = cardAdvanced.Enabled;
			bool hasSelection = isEnabled && SelectedRoute != null;
			btnAdd.Enabled = isEnabled;
			btnEdit.Enabled = hasSelection;
			btnRemove.Enabled = hasSelection;
			btnTestRoute.Enabled = hasSelection;
			lblRouteCount.Text = _routes.Count == 1
				? "1 advanced destination"
				: $"{_routes.Count} advanced destinations";
		}

		private void gridRoutes_SelectionChanged(object? sender, EventArgs eventArgs)
		{
			UpdateRouteButtons();
		}

		private void gridRoutes_CellDoubleClick(
			object? sender,
			DataGridViewCellEventArgs eventArgs)
		{
			if (eventArgs.RowIndex >= 0)
				btnEdit_Click(gridRoutes, EventArgs.Empty);
		}

		private void UpdateControlState()
		{
			bool masterActive = cardMaster.Enabled && chkMasterEnabled.Checked;
			txtMasterWebhook.Enabled = masterActive;
			cmbMasterPreset.Enabled = masterActive;
			lstMasterEvents.Enabled = masterActive;
			btnTestMaster.Enabled = masterActive;
			UpdateRouteButtons();
		}

		private void MasterSettingChanged(object? sender, EventArgs eventArgs)
		{
			if (_loading)
				return;
			UpdateControlState();
			SettingsChanged?.Invoke(this, EventArgs.Empty);
		}

		private void cmbMasterPreset_SelectedIndexChanged(object? sender, EventArgs eventArgs)
		{
			if (_loading || cmbMasterPreset.SelectedIndex < 0)
				return;
			DiscordNotificationEvent selected = cmbMasterPreset.SelectedIndex switch
			{
				0 => DiscordNotificationEvent.All,
				1 => Core.DiscordStatusEvents,
				2 => Core.DiscordMaintenanceEvents,
				3 => Core.DiscordProblemEvents,
				_ => GetMasterEvents()
			};
			_loading = true;
			LoadMasterEventOptions(selected);
			_loading = false;
			SettingsChanged?.Invoke(this, EventArgs.Empty);
		}

		private void lstMasterEvents_ItemCheck(object? sender, ItemCheckEventArgs eventArgs)
		{
			if (_loading || !IsHandleCreated)
				return;
			BeginInvoke(() =>
			{
				UpdateMasterSummary();
				if (!_loading)
					SettingsChanged?.Invoke(this, EventArgs.Empty);
			});
		}

		private void UpdateMasterSummary()
		{
			lblMasterSummary.Text = Core.SummarizeDiscordEvents(GetMasterEvents());
		}

		private async void btnTestMaster_Click(object? sender, EventArgs eventArgs)
		{
			await TestWebhookAsync(
				txtMasterWebhook.Text.Trim(),
				"Master webhook",
				btnTestMaster);
		}

		private void btnAdd_Click(object? sender, EventArgs eventArgs)
		{
			using DiscordWebhookRouteDialog dialog = new(null, _privacyMode, _serverName);
			if (dialog.ShowDialog(this) != DialogResult.OK || dialog.SelectedRoute == null)
				return;
			_routes.Add(dialog.SelectedRoute);
			RefreshRouteGrid();
			SettingsChanged?.Invoke(this, EventArgs.Empty);
		}

		private void btnEdit_Click(object? sender, EventArgs eventArgs)
		{
			DiscordWebhookRoute? route = SelectedRoute;
			if (route == null)
				return;
			using DiscordWebhookRouteDialog dialog = new(CloneRoute(route), _privacyMode, _serverName);
			if (dialog.ShowDialog(this) != DialogResult.OK || dialog.SelectedRoute == null)
				return;
			int index = _routes.FindIndex(candidate => candidate.Id == route.Id);
			if (index >= 0)
				_routes[index] = dialog.SelectedRoute;
			RefreshRouteGrid();
			SettingsChanged?.Invoke(this, EventArgs.Empty);
		}

		private void btnRemove_Click(object? sender, EventArgs eventArgs)
		{
			DiscordWebhookRoute? route = SelectedRoute;
			if (route == null)
				return;
			if (MessageBox.Show(
				$"Remove the saved Discord destination '{route.Name}' from this server?\n\nThis does not delete the webhook from Discord.",
				"Remove Discord Destination",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question) != DialogResult.Yes)
			{
				return;
			}
			_routes.Remove(route);
			RefreshRouteGrid();
			SettingsChanged?.Invoke(this, EventArgs.Empty);
		}

		private async void btnTestRoute_Click(object? sender, EventArgs eventArgs)
		{
			DiscordWebhookRoute? route = SelectedRoute;
			if (route == null)
				return;
			await TestWebhookAsync(route.WebhookUrl, route.Name, btnTestRoute);
		}

		private async Task TestWebhookAsync(
			string webhook,
			string name,
			Control button)
		{
			button.Enabled = false;
			lblStatus.Text = $"Sending a test to {name}...";
			lblStatus.ForeColor = SettingsPalette.SecondaryText;
			DiscordWebhookTestResult result = await Core.Instance.SendDiscordTestAsync(
				webhook,
				_serverName,
				name);
			lblStatus.Text = result.Message;
			lblStatus.ForeColor = result.Succeeded
				? SettingsPalette.Success
				: SettingsPalette.Danger;
			button.Enabled = cardMaster.Enabled || cardAdvanced.Enabled;
		}

		private static DiscordWebhookRoute CloneRoute(DiscordWebhookRoute route) =>
			new()
			{
				Id = route.Id,
				Name = route.Name,
				Enabled = route.Enabled,
				WebhookUrl = route.WebhookUrl,
				Events = route.Events
			};

		private static string BuildDiscordEventList(DiscordNotificationEvent events)
		{
			string[] selected = Core.GetDiscordNotificationOptions()
				.Where(option => (events & option.Event) != 0)
				.Select(option => option.Name)
				.ToArray();
			return selected.Length == 0 ? "No messages selected" : string.Join(", ", selected);
		}

		private static string MaskDiscordWebhook(string? webhook)
		{
			if (!Core.TryValidateDiscordWebhookUrl(webhook, out Uri? uri, out _))
				return "Webhook unavailable";

			string[] segments = uri!.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
			string identifier = segments.Length >= 3 ? segments[2] : string.Empty;
			string visible = identifier.Length <= 6
				? identifier
				: identifier[^6..];
			return string.IsNullOrWhiteSpace(visible)
				? "Discord webhook"
				: $"Discord webhook ••••{visible}";
		}

		private sealed class EventItem(DiscordNotificationOption option)
		{
			public DiscordNotificationOption Option { get; } = option;
			public override string ToString() => Option.Name;
		}
	}
}
