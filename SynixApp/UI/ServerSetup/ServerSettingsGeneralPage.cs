// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Database.GameConfigurations;
using Synix_Control_Panel.SynixApp.Design.Controls;
using Synix_Control_Panel.SynixApp.ServerHandler;
using Synix_Control_Panel.SynixEngine;

namespace Synix_Control_Panel.SynixApp.UI.ServerSetup
{
	public partial class ServerSettingsGeneralPage : UserControl
	{
		private GameServer? _existingServer;
		private bool _isEditMode;
		private bool _isLoading;
		private bool _suppressMinecraftEvents;
		private int _minecraftRequestId;
		private int _resolvedJavaVersion;
		private string _minecraftMetadataError = string.Empty;

		public event EventHandler? SettingsChanged;
		public event EventHandler? GameSelectionChanged;
		public event EventHandler? MinecraftEditionChanged;

		public ServerSettingsGeneralPage()
		{
			InitializeComponent();
			txtName.TextChanged += SettingsControlChanged;
			cmbGame.SelectedIndexChanged += SelectedGameChanged;
			cmbGameVersion.SelectedIndexChanged += GameVersionChanged;
			cmbMinecraftLoader.SelectedIndexChanged += MinecraftLoaderChanged;
			cmbMinecraftLoaderVersion.SelectedIndexChanged += SettingsControlChanged;
			cmbMinecraftEdition.SelectedIndexChanged += MinecraftEditionSelectionChanged;
			chkCrossplay.CheckedChanged += SettingsControlChanged;
			cmbWorldName.SelectedIndexChanged += SettingsControlChanged;
			cmbCompetitive.SelectedIndexChanged += SettingsControlChanged;
			numMaxPlayers.ValueChanged += SettingsControlChanged;
			numRam.ValueChanged += SettingsControlChanged;
			Disposed += (_, _) => _minecraftRequestId++;
		}

		public string ServerName => txtName.Text.Trim();
		public bool HasSelectedGame => cmbGame.SelectedIndex > 0;
		public string SelectedGame => HasSelectedGame ? cmbGame.Text : string.Empty;
		public string GameVersion => cmbGameVersion.Text.Trim();
		public string WorldName => cmbWorldName.Text;
		public string GameMode => cmbCompetitive.Text;
		public int MaximumPlayers => (int)numMaxPlayers.Value;
		public int MaximumRam => (int)numRam.Value;
		public bool CrossplayEnabled => chkCrossplay.Checked;
		public string MinecraftEdition =>
			MinecraftControlProfile.NormalizeEdition(cmbMinecraftEdition.Text);
		public string MinecraftLoader => IsMinecraftBedrockSelected
			? MinecraftMetadataService.VanillaLoader
			: MinecraftMetadataService.NormalizeLoader(cmbMinecraftLoader.Text);
		public string MinecraftLoaderVersion => IsMinecraftBedrockSelected
			? "Official"
			: cmbMinecraftLoaderVersion.Text.Trim();
		public string SelectedRuntime => cmbMinecraftLoader.Text.Trim();
		public int ResolvedMinecraftJavaVersion => _resolvedJavaVersion;
		public bool IsLoadingMinecraftMetadata => _isLoading;
		public string MinecraftMetadataError => _minecraftMetadataError;
		public bool IsMinecraftSelected =>
			HasSelectedGame &&
			SelectedGame.Equals("Minecraft", StringComparison.OrdinalIgnoreCase);
		public bool IsMinecraftBedrockSelected =>
			IsMinecraftSelected &&
			MinecraftEdition == MinecraftControlProfile.BedrockEdition;

		public void Initialize(GameServer? existingServer)
		{
			_existingServer = existingServer;
			_isEditMode = existingServer != null;
			_suppressMinecraftEvents = true;
			try
			{
				cmbMinecraftEdition.Items.Clear();
				cmbMinecraftEdition.Items.AddRange([
					MinecraftControlProfile.JavaEdition,
					MinecraftControlProfile.BedrockEdition
				]);
				cmbMinecraftEdition.SelectedItem = MinecraftControlProfile.JavaEdition;

				cmbGame.Items.Clear();
				cmbGame.Items.Add("-- Pick a Game --");
				foreach (GameInfo game in GameDatabase.GetGameList().OrderBy(game => game.Game))
					cmbGame.Items.Add(game.Game);
			}
			finally
			{
				_suppressMinecraftEvents = false;
			}
		}

		public void LoadServer(GameServer server, GameInfo? gameData)
		{
			ArgumentNullException.ThrowIfNull(server);
			_isLoading = true;
			_suppressMinecraftEvents = true;
			try
			{
				txtName.Text = server.ServerName ?? string.Empty;
				int gameIndex = cmbGame.FindStringExact(server.Game);
				if (gameIndex >= 0)
					cmbGame.SelectedIndex = gameIndex;
				if (GameDatabase.IsMinecraft(server.Game))
				{
					SelectComboBoxValue(
						cmbMinecraftEdition,
						MinecraftControlProfile.NormalizeEdition(server.MinecraftEdition),
						MinecraftControlProfile.JavaEdition);
				}

				chkCrossplay.Checked = server.CrossplayEnabled;
				numMaxPlayers.Value = Math.Clamp(
					server.MaxPlayers,
					numMaxPlayers.Minimum,
					numMaxPlayers.Maximum);
				cmbGameVersion.Text = server.GameVersion ?? "latest";
				numRam.Value = Math.Clamp(
					server.MaxRam,
					numRam.Minimum,
					numRam.Maximum);
				if (gameData != null)
				{
					string worldName = ServerSettingsWorldPage.IsSevenDaysToDie(gameData)
						? SevenDaysToDieConfiguration.NormalizeWorldName(server.WorldName)
						: server.WorldName ?? string.Empty;
					PopulateMaps(gameData, worldName);
					PopulateGameModes(gameData, server.GameMode ?? "PVE");
					ConfigureForGame(gameData);
				}
				cmbGame.Enabled = false;
			}
			finally
			{
				_suppressMinecraftEvents = false;
				_isLoading = false;
			}
		}

		public void SelectNoGame()
		{
			_isLoading = true;
			try
			{
				cmbGame.SelectedIndex = 0;
			}
			finally
			{
				_isLoading = false;
			}
		}

		public void ConfigureForGame(GameInfo? gameData)
		{
			ConfigureMaximumPlayersInput(gameData);
			ConfigureRuntimeCard(gameData);
			GameManagementCapability capabilities = gameData == null
				? GameManagementCapability.None
				: GameFix.GetManagementCapabilities(gameData);
			if (gameData != null &&
				GameDatabase.IsMinecraft(gameData.Game) &&
				IsMinecraftBedrockSelected)
			{
				capabilities =
					GameManagementCapability.GameMode |
					GameManagementCapability.MaxPlayers |
					GameManagementCapability.WorldName |
					GameManagementCapability.GameVersion;
			}

			bool Supports(GameManagementCapability capability) =>
				(capabilities & capability) != 0;
			cmbCompetitive.Tag = Supports(GameManagementCapability.GameMode)
				? "Required"
				: "Disabled";
			numMaxPlayers.Tag = Supports(GameManagementCapability.MaxPlayers)
				? "Required"
				: "Disabled";
			cmbWorldName.Tag = Supports(GameManagementCapability.WorldName)
				? "Required"
				: "Disabled";
			cmbGameVersion.Tag = Supports(GameManagementCapability.GameVersion)
				? "Required"
				: "Disabled";
			numRam.Tag = Supports(GameManagementCapability.Ram)
				? "Required"
				: "Disabled";
			bool supportsCrossplay = Supports(GameManagementCapability.Crossplay);
			chkCrossplay.Tag = supportsCrossplay ? "Required" : "Disabled";
			lblCrossplay.Visible = supportsCrossplay;
			chkCrossplay.Visible = supportsCrossplay;
		}

		public void ApplyAvailability(bool hasGame)
		{
			cmbCompetitive.Enabled = hasGame &&
				cmbCompetitive.Tag?.ToString() == "Required";
			numMaxPlayers.Enabled = hasGame &&
				numMaxPlayers.Tag?.ToString() == "Required";
			cmbWorldName.Enabled = hasGame &&
				cmbWorldName.Tag?.ToString() == "Required";
			cmbGameVersion.Enabled = hasGame &&
				cmbGameVersion.Tag?.ToString() == "Required" &&
				!_isLoading;
			numRam.Enabled = hasGame && numRam.Tag?.ToString() == "Required";
			chkCrossplay.Enabled = hasGame &&
				chkCrossplay.Tag?.ToString() == "Required";
			bool supportsFramework = GameDatabase.GetGame(SelectedGame)?
				.SupportedServerFrameworks.Count > 0;
			cmbMinecraftLoader.Enabled =
				(IsMinecraftSelected || supportsFramework) &&
				!IsMinecraftBedrockSelected &&
				!_isLoading;
			cmbMinecraftLoaderVersion.Enabled = IsMinecraftSelected &&
				!IsMinecraftBedrockSelected &&
				!_isLoading &&
				!MinecraftLoader.Equals(
					MinecraftMetadataService.VanillaLoader,
					StringComparison.OrdinalIgnoreCase);
		}

		public void PopulateMaps(GameInfo gameData, string selectedMap)
		{
			cmbWorldName.Items.Clear();
			if (gameData.Maps != null)
			{
				foreach (string map in gameData.Maps)
					cmbWorldName.Items.Add(map);
			}
			if (!string.IsNullOrWhiteSpace(selectedMap) &&
				!cmbWorldName.Items.Contains(selectedMap))
			{
				cmbWorldName.Items.Add(selectedMap);
			}
			if (!string.IsNullOrWhiteSpace(selectedMap))
				cmbWorldName.SelectedItem = selectedMap;
			else if (cmbWorldName.Items.Count > 0)
				cmbWorldName.SelectedIndex = 0;
		}

		public void PopulateGameModes(GameInfo gameData, string selectedMode)
		{
			cmbCompetitive.Items.Clear();
			if (GameDatabase.IsMinecraft(gameData.Game))
			{
				selectedMode = MinecraftControlProfile.NormalizeGameMode(selectedMode);
				foreach (string mode in MinecraftControlProfile.GameModes)
					cmbCompetitive.Items.Add(mode);
			}
			else if (gameData.GameModes != null)
			{
				foreach (string mode in gameData.GameModes)
					cmbCompetitive.Items.Add(mode);
			}
			if (!string.IsNullOrWhiteSpace(selectedMode) &&
				!cmbCompetitive.Items.Contains(selectedMode))
			{
				cmbCompetitive.Items.Add(selectedMode);
			}
			if (!string.IsNullOrWhiteSpace(selectedMode))
				cmbCompetitive.SelectedItem = selectedMode;
			else if (cmbCompetitive.Items.Count > 0)
				cmbCompetitive.SelectedIndex = 0;
		}

		public bool HasMinecraftLoaderSelection =>
			IsMinecraftBedrockSelected ||
			MinecraftLoader.Equals(
				MinecraftMetadataService.VanillaLoader,
				StringComparison.OrdinalIgnoreCase) ||
			!string.IsNullOrWhiteSpace(MinecraftLoaderVersion);

		public void RefreshCompatibilityVerification(string? game)
		{
			GameCompatibilityVerification verification = Core.GetGameCompatibility(game);
			UpdateCompatibilityLabel(lblInstallVerification, "Install", verification.Install);
			UpdateCompatibilityLabel(lblStartVerification, "Start", verification.Start);
			UpdateCompatibilityLabel(lblStopVerification, "Stop", verification.Stop);
			UpdateCompatibilityLabel(lblMonitoringVerification, "Monitoring", verification.Monitoring);
			GameVerificationEvidence? lastTested = verification.LastTested;
			if (lastTested == null)
			{
				lblLastTestedVersion.Text = "Last-tested Synix version: Not verified yet";
				lblLastTestedVersion.ForeColor = Color.FromArgb(158, 172, 194);
				return;
			}

			lblLastTestedVersion.Text =
				$"Last-tested Synix version: v{lastTested.SynixVersion}  •  {lastTested.VerifiedAtUtc.ToLocalTime():MMM d, yyyy}";
			lblLastTestedVersion.ForeColor = Color.FromArgb(32, 214, 199);
		}

		public async Task InitializeExistingMinecraftSelectionAsync()
		{
			if (_existingServer == null || !IsMinecraftSelected || IsDisposed)
				return;
			GameInfo? gameData = GameDatabase.GetGame(_existingServer.Game);
			if (gameData == null)
				return;

			try
			{
				await PopulateVersionsAsync(gameData, _existingServer.GameVersion ?? "latest");
				if (IsMinecraftBedrockSelected)
				{
					ConfigureRuntimeCard(gameData);
					return;
				}
				await RefreshMinecraftRuntimeAsync(
					_existingServer.MinecraftLoader,
					_existingServer.MinecraftLoaderVersion);
			}
			catch (Exception exception)
			{
				_minecraftMetadataError = $"Metadata could not be loaded: {exception.Message}";
				SettingsChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public async Task PopulateVersionsAsync(GameInfo gameData, string selectedVersion)
		{
			_suppressMinecraftEvents = true;
			try
			{
				cmbGameVersion.Items.Clear();
				cmbGameVersion.Items.Add("latest");
				if (gameData.Game.StartsWith("Minecraft", StringComparison.OrdinalIgnoreCase) &&
					!IsMinecraftBedrockSelected)
				{
					try
					{
						MinecraftMetadataService.MinecraftVersionCatalog catalog =
							await MinecraftMetadataService.GetVersionCatalogAsync();
						if (IsDisposed)
							return;
						foreach (string releaseVersion in catalog.ReleaseVersions)
							cmbGameVersion.Items.Add(releaseVersion);
					}
					catch (Exception exception)
					{
						_minecraftMetadataError =
							$"Mojang versions could not be loaded: {exception.Message}";
					}
				}

				string version = selectedVersion.Equals(
					"latest",
					StringComparison.OrdinalIgnoreCase)
						? "latest"
						: selectedVersion;
				if (!string.IsNullOrWhiteSpace(version) &&
					!cmbGameVersion.Items.Contains(version))
				{
					cmbGameVersion.Items.Add(version);
				}
				if (!string.IsNullOrWhiteSpace(version))
					cmbGameVersion.SelectedItem = version;
				else if (cmbGameVersion.Items.Count > 0)
					cmbGameVersion.SelectedIndex = 0;
			}
			finally
			{
				if (!IsDisposed)
					_suppressMinecraftEvents = false;
			}
		}

		public async Task RefreshMinecraftRuntimeAsync(
			string? preferredLoader,
			string? preferredLoaderVersion)
		{
			if (!IsMinecraftSelected || IsMinecraftBedrockSelected || IsDisposed)
				return;

			int requestId = ++_minecraftRequestId;
			_isLoading = true;
			_minecraftMetadataError = string.Empty;
			_suppressMinecraftEvents = true;
			ConfigureRuntimeCard(GameDatabase.GetGame("Minecraft"));
			SettingsChanged?.Invoke(this, EventArgs.Empty);
			string loader = MinecraftMetadataService.NormalizeLoader(preferredLoader);
			if (!cmbMinecraftLoader.Items.Contains(loader))
				loader = MinecraftMetadataService.VanillaLoader;

			try
			{
				SelectComboBoxValue(
					cmbMinecraftLoader,
					loader,
					MinecraftMetadataService.VanillaLoader);
				cmbMinecraftLoaderVersion.Items.Clear();
				cmbMinecraftLoaderVersion.Items.Add("Loading compatible builds...");
				cmbMinecraftLoaderVersion.SelectedIndex = 0;
				cmbMinecraftLoaderVersion.Enabled = false;
				lblMinecraftJavaValue.Text = "Resolving...";
				lblMinecraftRuntimeHelper.Text = loader == MinecraftMetadataService.VanillaLoader
					? "Synix installs the official server and matching portable Java."
					: $"Synix installs the compatible {loader} server loader. Add your own mods after installation.";

				Task<MinecraftMetadataService.MinecraftVersionMetadata> metadataTask =
					MinecraftMetadataService.GetVersionMetadataAsync(cmbGameVersion.Text);
				Task<IReadOnlyList<string>> loaderTask =
					MinecraftMetadataService.GetLoaderVersionsAsync(loader, cmbGameVersion.Text);
				await Task.WhenAll(metadataTask, loaderTask);
				if (requestId != _minecraftRequestId || IsDisposed || !IsMinecraftSelected)
					return;

				MinecraftMetadataService.MinecraftVersionMetadata metadata = await metadataTask;
				IReadOnlyList<string> compatibleBuilds = await loaderTask;
				if (compatibleBuilds.Count == 0)
				{
					throw new InvalidOperationException(
						$"No compatible {loader} server build exists for Minecraft {metadata.Version}.");
				}
				cmbMinecraftLoaderVersion.Items.Clear();
				foreach (string build in compatibleBuilds)
					cmbMinecraftLoaderVersion.Items.Add(build);

				string requestedBuild = preferredLoaderVersion?.Trim() ?? string.Empty;
				if (requestedBuild.Length == 0 ||
					requestedBuild.Equals("latest", StringComparison.OrdinalIgnoreCase) ||
					!cmbMinecraftLoaderVersion.Items.Contains(requestedBuild))
				{
					cmbMinecraftLoaderVersion.SelectedIndex = 0;
				}
				else
				{
					cmbMinecraftLoaderVersion.SelectedItem = requestedBuild;
				}
				_resolvedJavaVersion = metadata.JavaMajorVersion;
				lblMinecraftJavaValue.Text = $"Java {metadata.JavaMajorVersion}";
				lblMinecraftRuntimeHelper.Text = loader == MinecraftMetadataService.VanillaLoader
					? $"Minecraft {metadata.Version} uses the official Mojang server and Java {metadata.JavaMajorVersion}."
					: $"Minecraft {metadata.Version} + {loader} {cmbMinecraftLoaderVersion.Text} uses Java {metadata.JavaMajorVersion}. Add mods after installation.";
			}
			catch (Exception exception)
			{
				if (requestId != _minecraftRequestId || IsDisposed)
					return;
				_resolvedJavaVersion = 0;
				_minecraftMetadataError =
					$"{exception.Message} Re-select the version or loader to retry.";
				cmbMinecraftLoaderVersion.Items.Clear();
				lblMinecraftJavaValue.Text = "Unavailable";
				lblMinecraftRuntimeHelper.Text =
					"Synix could not verify this loader combination from the official metadata service.";
			}
			finally
			{
				if (requestId == _minecraftRequestId && !IsDisposed)
				{
					_suppressMinecraftEvents = false;
					_isLoading = false;
					SettingsChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		public async Task ApplyMinecraftEditionDefaultsAsync(GameInfo gameData)
		{
			ConfigureRuntimeCard(gameData);
			PopulateGameModes(
				gameData,
				_existingServer?.GameMode ?? MinecraftControlProfile.SurvivalGameMode);
			await PopulateVersionsAsync(gameData, "latest");
			if (!IsMinecraftBedrockSelected)
			{
				await RefreshMinecraftRuntimeAsync(
					MinecraftMetadataService.VanillaLoader,
					"Official");
			}
		}

		private void ConfigureRuntimeCard(GameInfo? gameData)
		{
			bool isMinecraft = gameData?.Game.Equals(
				"Minecraft",
				StringComparison.OrdinalIgnoreCase) == true;
			bool supportsFramework = gameData?.SupportedServerFrameworks.Count > 0;
			bool visible = isMinecraft || supportsFramework;
			cardMinecraftRuntime.Visible = visible;
			cardCompatibility.Location = visible
				? new Point(0, cardMinecraftRuntime.Bottom + 16)
				: new Point(0, 242);

			if (isMinecraft)
			{
				lblMinecraftRuntimeTitle.Text = "Minecraft Runtime";
				bool bedrock = IsMinecraftBedrockSelected;
				lblMinecraftEdition.Visible = true;
				cmbMinecraftEdition.Visible = true;
				lblMinecraftLoader.Text = bedrock ? "Server Package" : "Loader";
				lblMinecraftLoaderVersion.Visible = !bedrock;
				cmbMinecraftLoaderVersion.Visible = !bedrock;
				lblMinecraftJava.Visible = !bedrock;
				lblMinecraftJavaValue.Visible = !bedrock;
				cmbMinecraftLoader.Items.Clear();
				if (bedrock)
				{
					cmbMinecraftLoader.Items.Add("Official Bedrock");
					cmbMinecraftLoader.SelectedIndex = 0;
					cmbMinecraftLoader.Enabled = false;
					lblMinecraftRuntimeHelper.Text =
						"Synix installs Microsoft's official Bedrock Dedicated Server. Java and Java mod loaders do not apply.";
				}
				else
				{
					cmbMinecraftLoader.Items.AddRange(["Vanilla", "Fabric", "Forge"]);
					if (MinecraftMetadataService.IsNeoForgeCompatibleVersion(cmbGameVersion.Text))
						cmbMinecraftLoader.Items.Add(MinecraftMetadataService.NeoForgeLoader);
				}
			}
			else if (supportsFramework && gameData != null)
			{
				lblMinecraftRuntimeTitle.Text = "Server Framework";
				lblMinecraftEdition.Visible = false;
				cmbMinecraftEdition.Visible = false;
				lblMinecraftLoader.Text = "Framework";
				lblMinecraftLoaderVersion.Visible = false;
				cmbMinecraftLoaderVersion.Visible = false;
				lblMinecraftJava.Visible = false;
				lblMinecraftJavaValue.Visible = false;
				cmbMinecraftLoader.Items.Clear();
				cmbMinecraftLoader.Items.Add("Vanilla");
				foreach (string framework in gameData.SupportedServerFrameworks)
					cmbMinecraftLoader.Items.Add(framework);
				SelectComboBoxValue(
					cmbMinecraftLoader,
					_existingServer?.ServerFramework ?? "Vanilla",
					"Vanilla");
				lblMinecraftRuntimeHelper.Text =
					"Synix installs the official Oxide runtime only. Plugins remain user-managed in the server's oxide\\plugins folder.";
			}

			if (!visible)
			{
				lblMinecraftEdition.Visible = false;
				cmbMinecraftEdition.Visible = false;
				_minecraftRequestId++;
				_suppressMinecraftEvents = false;
				_isLoading = false;
				_minecraftMetadataError = string.Empty;
				_resolvedJavaVersion = 0;
			}
		}

		private void ConfigureMaximumPlayersInput(GameInfo? gameData)
		{
			int maximum = gameData?.MaximumPlayers ?? GameDefinition.DefaultMaximumPlayers;
			if (maximum > numMaxPlayers.Maximum)
				numMaxPlayers.Maximum = maximum;
			if (numMaxPlayers.Value > maximum)
				numMaxPlayers.Value = maximum;
			numMaxPlayers.Maximum = maximum;
			MaxPlayerLabel.Text = maximum < GameDefinition.DefaultMaximumPlayers
				? $"Max Players (maximum {maximum:0})"
				: "Max Players";
		}

		private static void UpdateCompatibilityLabel(
			Label label,
			string action,
			GameVerificationEvidence? evidence)
		{
			bool verified = evidence != null;
			label.Text = verified
				? $"{action}  ✓ Verified"
				: $"{action}  — Not verified yet";
			label.ForeColor = verified
				? Color.FromArgb(32, 214, 199)
				: Color.FromArgb(158, 172, 194);
		}

		private async void GameVersionChanged(object? sender, EventArgs eventArgs)
		{
			SettingsControlChanged(sender, eventArgs);
			if (!_isLoading && !_suppressMinecraftEvents && IsMinecraftSelected)
				await RefreshMinecraftRuntimeAsync(cmbMinecraftLoader.Text, "latest");
		}

		private async void MinecraftLoaderChanged(object? sender, EventArgs eventArgs)
		{
			SettingsControlChanged(sender, eventArgs);
			if (!_isLoading && !_suppressMinecraftEvents && IsMinecraftSelected)
				await RefreshMinecraftRuntimeAsync(cmbMinecraftLoader.Text, "latest");
		}

		private async void MinecraftEditionSelectionChanged(
			object? sender,
			EventArgs eventArgs)
		{
			SettingsControlChanged(sender, eventArgs);
			if (_isLoading || _suppressMinecraftEvents || !IsMinecraftSelected)
				return;
			GameInfo? minecraft = GameDatabase.GetGame("Minecraft");
			if (minecraft == null)
				return;

			try
			{
				_suppressMinecraftEvents = true;
				await ApplyMinecraftEditionDefaultsAsync(minecraft);
			}
			finally
			{
				_suppressMinecraftEvents = false;
			}
			MinecraftEditionChanged?.Invoke(this, EventArgs.Empty);
		}

		private void SelectedGameChanged(object? sender, EventArgs eventArgs)
		{
			if (!_isLoading)
				GameSelectionChanged?.Invoke(this, EventArgs.Empty);
			SettingsControlChanged(sender, eventArgs);
		}

		private void SettingsControlChanged(object? sender, EventArgs eventArgs)
		{
			if (!_isLoading)
				SettingsChanged?.Invoke(this, EventArgs.Empty);
		}

		private static void SelectComboBoxValue(
			ComboBox comboBox,
			string value,
			string fallback)
		{
			if (comboBox.Items.Contains(value))
				comboBox.SelectedItem = value;
			else if (comboBox.Items.Contains(fallback))
				comboBox.SelectedItem = fallback;
			else if (comboBox.Items.Count > 0)
				comboBox.SelectedIndex = 0;
		}
	}
}
