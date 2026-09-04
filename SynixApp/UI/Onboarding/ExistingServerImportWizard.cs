// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixApp.ServerHandler;

namespace Synix_Control_Panel.SynixApp.UI.Onboarding
{
	internal sealed class ExistingServerImportWizard : Form
	{
		private readonly TextBox _folderBox;
		private readonly ModernSettingsComboBox _gameBox;
		private readonly TextBox _nameBox;
		private readonly NumericUpDown _gamePort;
		private readonly NumericUpDown _queryPort;
		private readonly Label _detectionStatus;
		private readonly ModernSettingsButton _importButton;
		private IReadOnlyList<ExistingServerDetection> _detections = [];

		internal GameServer? ImportedServer { get; private set; }

		internal ExistingServerImportWizard()
		{
			Text = "Import Existing Server";
			StartPosition = FormStartPosition.CenterParent;
			ShowInTaskbar = false;
			MinimizeBox = false;
			MaximizeBox = false;
			FormBorderStyle = FormBorderStyle.FixedDialog;
			ClientSize = new Size(800, 590);
			BackColor = SettingsPalette.Window;
			ForeColor = SettingsPalette.PrimaryText;
			Font = new Font("Segoe UI", 9.5F);

			Controls.Add(Heading("Import an existing game server", 28, 24, 744, 42, 19F));
			Controls.Add(Body(
				"Choose the server's main folder. Synix will identify the game from its exact executable path. It will not move, delete, reinstall, or overwrite your existing configuration.",
				30, 68, 730, 50));

			ModernSettingsCard folderCard = Card(28, 126, 744, 132);
			folderCard.Controls.Add(Heading("1. Choose the existing server folder", 18, 14, 540, 26, 11F));
			_folderBox = Input(18, 52, 566, 38);
			_folderBox.TextChanged += (_, _) => DetectGames();
			folderCard.Controls.Add(_folderBox);
			ModernSettingsButton browse = new()
			{
				Text = "Browse",
				Location = new Point(596, 50),
				Size = new Size(128, 42)
			};
			browse.Click += Browse_Click;
			folderCard.Controls.Add(browse);
			_detectionStatus = Body("No folder selected yet.", 18, 98, 706, 24);
			folderCard.Controls.Add(_detectionStatus);
			Controls.Add(folderCard);

			ModernSettingsCard detailsCard = Card(28, 274, 744, 226);
			detailsCard.Controls.Add(Heading("2. Confirm the server details", 18, 14, 680, 26, 11F));
			detailsCard.Controls.Add(FieldLabel("Detected game", 18, 52, 330));
			detailsCard.Controls.Add(FieldLabel("Server name in Synix", 382, 52, 330));
			_gameBox = new ModernSettingsComboBox
			{
				Location = new Point(18, 78),
				Size = new Size(330, 40),
				Enabled = false
			};
			_gameBox.SelectedIndexChanged += GameBox_SelectedIndexChanged;
			detailsCard.Controls.Add(_gameBox);
			_nameBox = Input(382, 78, 342, 40);
			detailsCard.Controls.Add(_nameBox);

			detailsCard.Controls.Add(FieldLabel("Game port", 18, 136, 160));
			detailsCard.Controls.Add(FieldLabel("Query port", 200, 136, 160));
			_gamePort = PortInput(18, 162);
			_queryPort = PortInput(200, 162);
			detailsCard.Controls.Add(_gamePort);
			detailsCard.Controls.Add(_queryPort);
			detailsCard.Controls.Add(Body(
				"Use the ports already configured for this server. You can change the rest of the settings after import.",
				382, 144, 342, 60));
			Controls.Add(detailsCard);

			ModernSettingsButton cancel = new()
			{
				Text = "Cancel",
				Location = new Point(464, 522),
				Size = new Size(142, 44),
				DialogResult = DialogResult.Cancel
			};
			_importButton = new ModernSettingsButton
			{
				Text = "Register Server",
				Location = new Point(618, 522),
				Size = new Size(154, 44),
				UseAccentStyle = true,
				Enabled = false
			};
			_importButton.Click += Import_Click;
			Controls.AddRange([cancel, _importButton]);
			CancelButton = cancel;
			ThemeManager.Apply(this);
		}

		private void Browse_Click(object? sender, EventArgs eventArgs)
		{
			using FolderBrowserDialog browser = new()
			{
				Description = "Choose the folder containing the installed game server",
				UseDescriptionForTitle = true,
				ShowNewFolderButton = false
			};
			if (browser.ShowDialog(this) == DialogResult.OK)
				_folderBox.Text = browser.SelectedPath;
		}

		private void DetectGames()
		{
			_detections = ExistingServerImport.Detect(_folderBox.Text);
			_gameBox.Items.Clear();
			foreach (ExistingServerDetection detection in _detections)
				_gameBox.Items.Add(detection.DisplayName);

			_gameBox.Enabled = _detections.Count > 0;
			_importButton.Enabled = _detections.Count > 0;
			if (_detections.Count == 0)
			{
				_detectionStatus.Text = Directory.Exists(_folderBox.Text.Trim())
					? "No supported server executable was found in this folder. Try the folder used as the server install path."
					: "Choose an existing server folder to continue.";
				_detectionStatus.ForeColor = SettingsPalette.Warning;
				return;
			}

			_gameBox.SelectedIndex = 0;
			_detectionStatus.Text = _detections.Count == 1
				? $"Found {_detections[0].DisplayName}."
				: $"Found {_detections.Count} possible server programs. Select the correct game below.";
			_detectionStatus.ForeColor = SettingsPalette.Success;
		}

		private void GameBox_SelectedIndexChanged(object? sender, EventArgs eventArgs)
		{
			ExistingServerDetection? detection = GetSelectedDetection();
			if (detection == null)
				return;

			_nameBox.Text = MakeUniqueName(detection.DisplayName);
			int defaultGamePort = detection.MinecraftEdition == MinecraftControlProfile.BedrockEdition
				? MinecraftControlProfile.BedrockDefaultPort
				: detection.Game.Port;
			int defaultQueryPort = detection.MinecraftEdition == MinecraftControlProfile.BedrockEdition
				? MinecraftControlProfile.BedrockDefaultIpv6Port
				: detection.Game.QueryPort;
			_gamePort.Value = ExistingServerImport.FindAvailablePort(
				defaultGamePort,
				ServerRegistry.Servers);
			_queryPort.Value = ExistingServerImport.FindAvailablePort(
				defaultQueryPort,
				ServerRegistry.Servers.Concat([new GameServer { Port = (int)_gamePort.Value }]));
		}

		private async void Import_Click(object? sender, EventArgs eventArgs)
		{
			ExistingServerDetection? detection = GetSelectedDetection();
			if (detection == null)
				return;

			try
			{
				_importButton.Enabled = false;
				ImportedServer = ExistingServerImport.Create(
					_folderBox.Text,
					detection.Game,
					_nameBox.Text,
					(int)_gamePort.Value,
					(int)_queryPort.Value,
					ServerRegistry.Servers,
					detection.MinecraftEdition);
				ServerRegistry.Servers.Add(ImportedServer);
				await Core.RefreshServerIconAsync(ImportedServer);
				if (!Synix_Control_Panel.SynixApp.FileFolderHandler.FileHandler.SaveServers())
					throw new IOException("Synix could not save the imported server.");

				await Core.Instance.RebindProcesses();
				ApplicationLogService.Write(
					$"[IMPORT] Registered {ImportedServer.ServerName} without changing its existing files. Review Server Settings before its first Synix-managed start.",
					SettingsPalette.Success,
					true);
				DialogResult = DialogResult.OK;
				Close();
			}
			catch (Exception exception)
			{
				if (ImportedServer != null)
					ServerRegistry.Servers.Remove(ImportedServer);
				ImportedServer = null;
				PlainEnglishErrorDialog.ShowError(this, "import the existing server", exception.Message);
				_importButton.Enabled = true;
			}
		}

		private ExistingServerDetection? GetSelectedDetection() =>
			_gameBox.SelectedIndex >= 0 && _gameBox.SelectedIndex < _detections.Count
				? _detections[_gameBox.SelectedIndex]
				: null;

		private static string MakeUniqueName(string game)
		{
			string baseName = $"Imported {game}";
			string candidate = baseName;
			for (int suffix = 2; ServerRegistry.Servers.Any(server => server.ServerName.Equals(
				candidate,
				StringComparison.OrdinalIgnoreCase)); suffix++)
			{
				candidate = $"{baseName} {suffix}";
			}
			return candidate;
		}

		private static ModernSettingsCard Card(int left, int top, int width, int height) => new()
		{
			Location = new Point(left, top),
			Size = new Size(width, height),
			FillColor = SettingsPalette.Card,
			BorderColor = SettingsPalette.Divider
		};

		private static Label Heading(string text, int left, int top, int width, int height, float size) => new()
		{
			Text = text,
			Font = new Font("Segoe UI", size, FontStyle.Bold),
			ForeColor = SettingsPalette.PrimaryText,
			Location = new Point(left, top),
			Size = new Size(width, height)
		};

		private static Label Body(string text, int left, int top, int width, int height) => new()
		{
			Text = text,
			ForeColor = SettingsPalette.SecondaryText,
			Location = new Point(left, top),
			Size = new Size(width, height)
		};

		private static Label FieldLabel(string text, int left, int top, int width) => new()
		{
			Text = text,
			Font = new Font("Segoe UI", 9F, FontStyle.Bold),
			ForeColor = SettingsPalette.SecondaryText,
			Location = new Point(left, top),
			Size = new Size(width, 22)
		};

		private static TextBox Input(int left, int top, int width, int height) => new()
		{
			Location = new Point(left, top),
			Size = new Size(width, height),
			BackColor = SettingsPalette.Input,
			ForeColor = SettingsPalette.PrimaryText,
			BorderStyle = BorderStyle.FixedSingle,
			Font = new Font("Segoe UI", 10F)
		};

		private static NumericUpDown PortInput(int left, int top) => new()
		{
			Location = new Point(left, top),
			Size = new Size(160, 40),
			Minimum = 1,
			Maximum = 65535,
			BackColor = SettingsPalette.Input,
			ForeColor = SettingsPalette.PrimaryText,
			BorderStyle = BorderStyle.FixedSingle,
			Font = new Font("Segoe UI", 10F)
		};
	}
}
