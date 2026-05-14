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
using System.Runtime.InteropServices;

namespace Synix_Control_Panel.Help
{
	public partial class ServerInfo : Form
	{
		[DllImport("user32.dll")]
		private static extern uint SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);
		private const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

		private GameServer _server;

		public ServerInfo(GameServer server)
		{
			InitializeComponent();
			_server = server;
			if (Properties.Settings.Default.PrivacyMode)
			{
				SetWindowDisplayAffinity(this.Handle, WDA_EXCLUDEFROMCAPTURE);
			}
			LoadServerData();
		}

		private void LoadServerData()
		{
			if (_server == null) return;

			lblMaxPlayersText.Text = _server.MaxPlayers.ToString();
			lblGamePortText.Text = _server.Port.ToString();
			lblQueryPortText.Text = _server.QueryPort.ToString();
			lblRconPortText.Text = _server.RconPort.ToString();

			lblAppPortText.Text = _server.AppPort?.ToString() ?? "N/A";

			SetStatusColor(lblRconActiveText, _server.EnableRcon);
			SetStatusColor(lblBackupOnStartText, _server.BackupOnStart);
			SetStatusColor(lbllUpdateOnStartText, _server.UpdateOnStart);
			SetStatusColor(lblDiscordActivateText, _server.IsDiscordAlertEnabled);

			lblServerNameText.Text = _server.ServerName;
			lblGameServerText.Text = _server.Game;
			lblMapText.Text = _server.WorldName;
			lblSeedText.Text = _server.WorldSeed;
			lblCompetitiveText.Text = _server.GameMode;
			lblRconPasswordText.Text = _server.RconPassword;

			lblDiscordWebhookText.Text = _server.DiscordWebhook;
			lblServerPasswordText.Text = _server.Password;
			lblServerAdminPasswordText.Text = _server.AdminPassword;
			lblServerFolderText.Text = _server.InstallPath;
			lblExtraArgsText.Text = _server.ExtraArgs;

			lblAutoRestartText.Text = GetActiveDays(_server.RestartDays);
		}

		private string GetActiveDays(bool[] days)
		{
			if (days == null || days.Length < 7) return "None";
			string[] names = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
			List<string> active = new List<string>();

			for (int i = 0; i < 7; i++)
			{
				if (days[i]) active.Add(names[i]);
			}

			return active.Count > 0 ? string.Join(", ", active) : "No Days Scheduled";
		}

		private void SetStatusColor(Label label, bool isActive)
		{
			if (isActive)
			{
				label.Text = "On";
				label.ForeColor = Color.LimeGreen;
			}
			else
			{
				label.Text = "Off";
				label.ForeColor = Color.Red;
			}
		}
	}
}
