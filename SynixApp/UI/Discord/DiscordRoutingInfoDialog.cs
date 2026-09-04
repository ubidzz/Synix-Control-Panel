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
using System.ComponentModel;
using System.Runtime.InteropServices;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixEngine;

namespace Synix_Control_Panel.SynixApp.UI.Discord
{
	public partial class DiscordRoutingInfoDialog : Form
	{
		private const int WmNcLeftButtonDown = 0x00A1;
		private const int HtCaption = 0x0002;

		public DiscordRoutingInfoDialog()
		{
			InitializeComponent();
			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime || DesignMode)
				return;

			ConfigureGrid();
			ThemeManager.Apply(this);
			GridStyler.StyleCloseButton(btnTitleClose);
		}

		public DiscordRoutingInfoDialog(
			GameServer server,
			string masterWebhook,
			IReadOnlyList<DiscordWebhookRoute> routes) : this()
		{
			ArgumentNullException.ThrowIfNull(server);
			ArgumentNullException.ThrowIfNull(routes);

			LocalizationManager.BindText(
				lblHeading,
				"Discord.Routing.Heading",
				server.ServerName);
			LoadRoutes(server, masterWebhook, routes);
		}

		private void ConfigureGrid()
		{
			gridRoutes.AutoGenerateColumns = false;
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

		private void LoadRoutes(
			GameServer server,
			string masterWebhook,
			IReadOnlyList<DiscordWebhookRoute> routes)
		{
			gridRoutes.Rows.Clear();
			if (!string.IsNullOrWhiteSpace(masterWebhook))
			{
				AddRoute(
					LocalizationManager.Get(
						server.IsDiscordAlertEnabled
							? "Text.92C1CDFDF4CB9CF6FCCA"
							: "Discord.Status.Paused"),
					LocalizationManager.Get("Discord.MasterWebhook"),
					BuildEventList(server.DiscordEvents),
					MaskWebhook(masterWebhook),
					server.IsDiscordAlertEnabled
						? SettingsPalette.Success
						: SettingsPalette.MutedText);
			}

			foreach (DiscordWebhookRoute route in routes)
			{
				AddRoute(
					LocalizationManager.Get(
						route.Enabled
							? "Text.92C1CDFDF4CB9CF6FCCA"
							: "Discord.Status.Paused"),
					string.IsNullOrWhiteSpace(route.Name)
						? LocalizationManager.Get("Text.A8726569C87C6C5A3BFE")
						: route.Name,
					BuildEventList(route.Events),
					MaskWebhook(route.WebhookUrl),
					route.Enabled ? SettingsPalette.Success : SettingsPalette.MutedText);
			}

			LocalizationManager.BindText(
				lblCount,
				gridRoutes.Rows.Count == 1
					? "DynamicText.36134FEFDC7A2322ADAF"
					: "Discord.Routing.Count.Many",
				gridRoutes.Rows.Count);
			gridRoutes.ClearSelection();
		}

		private void AddRoute(
			string status,
			string destination,
			string events,
			string webhook,
			Color statusColor)
		{
			int index = gridRoutes.Rows.Add(status, destination, events, webhook);
			DataGridViewRow row = gridRoutes.Rows[index];
			row.Height = 44;
			row.Cells[0].Style.ForeColor = statusColor;
			row.Cells[0].Style.SelectionForeColor = statusColor;
			row.Cells[2].ToolTipText = events;
			row.Cells[3].ToolTipText = webhook;
		}

		private static string BuildEventList(DiscordNotificationEvent events)
		{
			string[] selected = Core.GetDiscordNotificationOptions()
				.Where(option => (events & option.Event) != 0)
				.Select(option => LocalizationManager.TranslateKnownText(option.Name))
				.ToArray();
			return selected.Length == 0
				? LocalizationManager.Get("Discord.NoMessagesSelected")
				: string.Join(" • ", selected);
		}

		private static string MaskWebhook(string? webhook)
		{
			if (!Core.TryValidateDiscordWebhookUrl(webhook, out Uri? uri, out _))
				return LocalizationManager.Get("Discord.WebhookUnavailable");

			string[] segments = uri!.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
			string identifier = segments.Length >= 3 ? segments[2] : string.Empty;
			string visible = identifier.Length <= 6 ? identifier : identifier[^6..];
			return string.IsNullOrWhiteSpace(visible)
				? LocalizationManager.Get("Discord.Webhook")
				: LocalizationManager.Get("Discord.WebhookMasked", visible);
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

		private void btnClose_Click(object? sender, EventArgs eventArgs) => Close();

		private void TitleBar_MouseDown(object? sender, MouseEventArgs eventArgs)
		{
			if (eventArgs.Button != MouseButtons.Left)
				return;

			_ = ReleaseCapture();
			_ = SendMessage(Handle, WmNcLeftButtonDown, HtCaption, 0);
		}

		[DllImport("user32.dll")]
		private static extern bool ReleaseCapture();

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(
			IntPtr windowHandle,
			int message,
			int wordParameter,
			int longParameter);
	}
}
