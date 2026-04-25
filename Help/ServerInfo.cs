// Copyright (c) 2026 ubidzz. All Rights Reserved.
//
// This file is part of Synix Control Panel.
//
// This code is provided for transparent viewing and personal use only.
// Unauthorized distribution, public modification, or commercial
// use of this source code or the compiled executable is strictly
// prohibited. Please refer to the LICENSE file in the root
// directory for full terms.

namespace Synix_Control_Panel.Help
{
	public partial class ServerInfo : Form
	{
		private GameServer _server;

		public ServerInfo(GameServer server)
		{
			InitializeComponent();
			_server = server;
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

		// Helper function to turn your bool array into a readable string like "Mon, Wed, Fri"
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
				label.ForeColor = Color.LimeGreen; // Bright green for "On"
			}
			else
			{
				label.Text = "Off";
				label.ForeColor = Color.Red; // Red for "Off"
			}
		}
	}
}
