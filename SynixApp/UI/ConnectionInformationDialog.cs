// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.ServerHandler;

namespace Synix_Control_Panel.SynixEngine
{
	internal sealed class ConnectionInformationDialog : Form
	{
		private readonly GameServer _server;
		private readonly Label _lanValue;
		private readonly Label _publicValue;
		private string _lanAddress = string.Empty;
		private string _publicAddress = string.Empty;

		internal ConnectionInformationDialog(GameServer server)
		{
			_server = server ?? throw new ArgumentNullException(nameof(server));
			bool isBedrock = MinecraftControlProfile.IsBedrock(_server);
			Text = "Connection Information";
			StartPosition = FormStartPosition.CenterParent;
			ShowInTaskbar = false;
			MinimizeBox = false;
			MaximizeBox = false;
			FormBorderStyle = FormBorderStyle.FixedDialog;
			ClientSize = new Size(760, 520);
			BackColor = SettingsPalette.Window;
			ForeColor = SettingsPalette.PrimaryText;
			Font = new Font("Segoe UI", 9.5F);

			Controls.Add(new Label
			{
				Text = $"Connect to {_server.ServerName}",
				Font = new Font("Segoe UI", 18F, FontStyle.Bold),
				Location = new Point(28, 22),
				Size = new Size(704, 40),
				ForeColor = SettingsPalette.PrimaryText
			});
			Controls.Add(new Label
			{
				Text = "Use the address that matches where the player is connecting from.",
				Location = new Point(30, 66),
				Size = new Size(700, 28),
				ForeColor = SettingsPalette.SecondaryText
			});

			_lanValue = AddConnectionCard(
				"Same computer or home network",
				"Use this for players connected to the same router.",
				108,
				out ModernSettingsButton lanCopy);
			_publicValue = AddConnectionCard(
				"Friends connecting over the internet",
				isBedrock
					? "Your router and Windows Firewall must allow Bedrock's UDP game port."
					: "Your router and Windows Firewall must allow the game and query ports.",
				226,
				out ModernSettingsButton publicCopy);
			lanCopy.Click += (_, _) => CopyAddress(_lanAddress);
			publicCopy.Click += (_, _) => CopyAddress(_publicAddress);

			Controls.Add(new Label
			{
				Text = isBedrock
					? $"Bedrock game port: {_server.Port}/UDP. IPv6 port: {_server.QueryPort}/UDP. " +
						"Each Bedrock server needs its own pair of ports."
					: UserGuidance.GetPortSummary(_server) +
						" Some games appear in a server browser only when the query port is also forwarded.",
				Location = new Point(30, 356),
				Size = new Size(700, 54),
				ForeColor = SettingsPalette.SecondaryText
			});

			ModernSettingsButton close = new()
			{
				Text = "Close",
				Location = new Point(574, 446),
				Size = new Size(158, 44),
				DialogResult = DialogResult.OK,
				UseAccentStyle = true
			};
			Controls.Add(close);
			AcceptButton = close;
			CancelButton = close;
			ThemeManager.Apply(this);
		}

		protected override async void OnShown(EventArgs eventArgs)
		{
			base.OnShown(eventArgs);
			bool privacyMode = Properties.Settings.Default.PrivacyMode;
			try
			{
				string lanIp = await Core.Instance.GetLocalIP();
				string publicIp = privacyMode ? string.Empty : await Core.Instance.GetPublicIP();
				_lanAddress = FormatAddress(lanIp, _server.Port);
				_publicAddress = string.IsNullOrWhiteSpace(publicIp)
					? string.Empty
					: FormatAddress(publicIp.Trim(), _server.Port);
				_lanValue.Text = privacyMode ? "Hidden by Privacy Mode" : _lanAddress;
				_publicValue.Text = privacyMode
					? "Hidden by Privacy Mode"
					: string.IsNullOrWhiteSpace(_publicAddress)
						? "Public address could not be loaded"
						: _publicAddress;
			}
			catch (Exception exception)
			{
				_lanValue.Text = "Address could not be loaded";
				_publicValue.Text = "Address could not be loaded";
				_ = exception;
			}
		}

		internal static string FormatAddress(string address, int port) =>
			$"{address}:{port}";

		private Label AddConnectionCard(
			string title,
			string description,
			int top,
			out ModernSettingsButton copyButton)
		{
			ModernSettingsCard card = new()
			{
				Location = new Point(28, top),
				Size = new Size(704, 104),
				FillColor = SettingsPalette.Card,
				BorderColor = SettingsPalette.Divider
			};
			card.Controls.Add(new Label
			{
				Text = title,
				Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold),
				Location = new Point(20, 14),
				Size = new Size(460, 24),
				ForeColor = SettingsPalette.PrimaryText
			});
			Label value = new()
			{
				Text = "Loading...",
				Font = new Font("Consolas", 11F, FontStyle.Bold),
				Location = new Point(20, 43),
				Size = new Size(450, 25),
				ForeColor = SettingsPalette.Accent
			};
			card.Controls.Add(value);
			card.Controls.Add(new Label
			{
				Text = description,
				Location = new Point(20, 72),
				Size = new Size(510, 22),
				ForeColor = SettingsPalette.SecondaryText
			});
			copyButton = new ModernSettingsButton
			{
				Text = "Copy Address",
				Location = new Point(548, 31),
				Size = new Size(134, 42)
			};
			card.Controls.Add(copyButton);
			Controls.Add(card);
			return value;
		}

		private static void CopyAddress(string address)
		{
			if (string.IsNullOrWhiteSpace(address))
				return;
			try { Clipboard.SetText(address); }
			catch { }
		}
	}
}
