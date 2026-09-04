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
using Synix_Control_Panel.SynixApp.Localization;
using Synix_Control_Panel.SynixEngine;
using System.ComponentModel;

namespace Synix_Control_Panel
{
	public partial class DiscordWebhookRouteDialog : Form
	{
		private readonly string _serverName = "Test Server";
		private readonly string _routeId = Guid.NewGuid().ToString("N");
		private bool _loading;

		public DiscordWebhookRoute? SelectedRoute { get; private set; }

		public DiscordWebhookRouteDialog()
		{
			InitializeComponent();
			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime || DesignMode)
				return;

			ThemeManager.Apply(this);
			PopulatePresetOptions();
			LocalizationManager.LanguageChanged += InterfaceLanguageChanged;
			Disposed += (_, _) =>
				LocalizationManager.LanguageChanged -= InterfaceLanguageChanged;
			LoadEventOptions(DiscordNotificationEvent.All);
			cmbPreset.SelectedIndex = 0;
		}

		private void InterfaceLanguageChanged(
			object? sender,
			EventArgs eventArgs)
		{
			PopulatePresetOptions();
		}

		private void PopulatePresetOptions()
		{
			int selectedIndex = Math.Max(0, cmbPreset.SelectedIndex);
			bool previousLoading = _loading;
			_loading = true;
			try
			{
				cmbPreset.Items.Clear();
				cmbPreset.Items.AddRange(
				[
					LocalizationManager.Get("Option.Discord.AllEvents"),
					LocalizationManager.Get("Option.Discord.ServerStatus"),
					LocalizationManager.Get("Option.Discord.Maintenance"),
					LocalizationManager.Get("Option.Discord.ProblemsOnly"),
					LocalizationManager.Get("Option.Discord.Custom")
				]);
				cmbPreset.SelectedIndex = Math.Min(
					selectedIndex,
					cmbPreset.Items.Count - 1);
			}
			finally
			{
				_loading = previousLoading;
			}
		}

		public DiscordWebhookRouteDialog(
			DiscordWebhookRoute? route,
			bool privacyMode,
			string serverName) : this()
		{
			_serverName = string.IsNullOrWhiteSpace(serverName)
				? "Test Server"
				: serverName.Trim();
			_routeId = string.IsNullOrWhiteSpace(route?.Id)
				? Guid.NewGuid().ToString("N")
				: route.Id;
			_loading = true;
			txtName.Text = route?.Name ?? string.Empty;
			txtWebhook.Text = route?.WebhookUrl ?? string.Empty;
			txtWebhook.UseSystemPasswordChar = privacyMode;
			chkEnabled.Checked = route?.Enabled ?? true;
			LoadEventOptions(route?.Events ?? DiscordNotificationEvent.All);
			_loading = false;
		}

		private void LoadEventOptions(DiscordNotificationEvent selected)
		{
			lstEvents.Items.Clear();
			foreach (DiscordNotificationOption option in Core.GetDiscordNotificationOptions())
			{
				int index = lstEvents.Items.Add(new EventItem(option));
				lstEvents.SetItemChecked(index, (selected & option.Event) != 0);
			}
			UpdateSelectionStatus();
		}

		private DiscordNotificationEvent GetSelectedEvents()
		{
			DiscordNotificationEvent selected = DiscordNotificationEvent.None;
			foreach (EventItem item in lstEvents.CheckedItems)
				selected |= item.Option.Event;
			return selected;
		}

		private void cmbPreset_SelectedIndexChanged(object? sender, EventArgs eventArgs)
		{
			if (_loading || cmbPreset.SelectedIndex < 0)
				return;

			DiscordNotificationEvent selected = cmbPreset.SelectedIndex switch
			{
				0 => DiscordNotificationEvent.All,
				1 => Core.DiscordStatusEvents,
				2 => Core.DiscordMaintenanceEvents,
				3 => Core.DiscordProblemEvents,
				_ => GetSelectedEvents()
			};
			LoadEventOptions(selected);
		}

		private void lstEvents_ItemCheck(object? sender, ItemCheckEventArgs eventArgs)
		{
			if (_loading || !IsHandleCreated)
				return;
			BeginInvoke(UpdateSelectionStatus);
		}

		private void UpdateSelectionStatus()
		{
			DiscordNotificationEvent events = GetSelectedEvents();
			lblSelection.Text = Core.SummarizeDiscordEvents(events);
			lblSelection.ForeColor = events == DiscordNotificationEvent.None
				? SettingsPalette.Warning
				: SettingsPalette.Accent;
		}

		private async void btnTest_Click(object? sender, EventArgs eventArgs)
		{
			btnTest.Enabled = false;
			lblStatus.Text = "Sending a safe test message...";
			lblStatus.ForeColor = SettingsPalette.SecondaryText;
			DiscordWebhookTestResult result = await Core.Instance.SendDiscordTestAsync(
				txtWebhook.Text.Trim(),
				_serverName,
				string.IsNullOrWhiteSpace(txtName.Text) ? "Discord channel" : txtName.Text.Trim());
			lblStatus.Text = result.Message;
			lblStatus.ForeColor = result.Succeeded
				? SettingsPalette.Success
				: SettingsPalette.Danger;
			btnTest.Enabled = true;
		}

		private void btnSave_Click(object? sender, EventArgs eventArgs)
		{
			string name = txtName.Text.Trim();
			string webhook = txtWebhook.Text.Trim();
			DiscordNotificationEvent events = GetSelectedEvents();
			if (string.IsNullOrWhiteSpace(name))
			{
				ShowValidation("Give this Discord destination a name, such as Backups or Server Status.");
				return;
			}
			if (!Core.TryValidateDiscordWebhookUrl(webhook, out Uri? normalized, out string error))
			{
				ShowValidation(error);
				return;
			}
			if (events == DiscordNotificationEvent.None)
			{
				ShowValidation("Select at least one event for this Discord destination.");
				return;
			}

			SelectedRoute = new DiscordWebhookRoute
			{
				Id = _routeId,
				Name = name,
				Enabled = chkEnabled.Checked,
				WebhookUrl = normalized!.AbsoluteUri,
				Events = events
			};
			DialogResult = DialogResult.OK;
			Close();
		}

		private void ShowValidation(string message)
		{
			lblStatus.Text = message;
			lblStatus.ForeColor = SettingsPalette.Warning;
		}

		private sealed class EventItem(DiscordNotificationOption option)
		{
			public DiscordNotificationOption Option { get; } = option;
			public override string ToString() => $"{Option.Group}  •  {Option.Name}";
		}
	}
}
