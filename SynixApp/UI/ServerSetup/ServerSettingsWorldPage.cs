// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using Synix_Control_Panel.SynixApp.Database;
using Synix_Control_Panel.SynixApp.Database.GameConfigurations;
using Synix_Control_Panel.SynixApp.Design.Controls;
using Synix_Control_Panel.SynixApp.ServerHandler;

namespace Synix_Control_Panel.SynixApp.UI.ServerSetup
{
	public partial class ServerSettingsWorldPage : UserControl
	{
		private bool _isLoading;
		private bool _digitsOnlySeed;

		public event EventHandler? SettingsChanged;

		public ServerSettingsWorldPage()
		{
			InitializeComponent();
			txtWorldSeed.KeyPress += WorldSeedKeyPress;
			txtWorldSeed.TextChanged += SettingsControlChanged;
			numWorldSize.ValueChanged += SettingsControlChanged;
		}

		public string WorldSeed => ReadManagedValue(txtWorldSeed).Trim();
		public int WorldSize => (int)numWorldSize.Value;

		public void LoadServer(GameServer server, GameInfo? gameData)
		{
			ArgumentNullException.ThrowIfNull(server);
			_isLoading = true;
			try
			{
				ConfigureForGame(gameData, isMinecraftBedrock: false);
				txtWorldSeed.Text = server.WorldSeed ?? "12345";
				int worldSize = IsSevenDaysToDie(gameData)
					? SevenDaysToDieConfiguration.NormalizeWorldSize(server.WorldSize)
					: server.WorldSize;
				numWorldSize.Value = Math.Clamp(
					worldSize,
					numWorldSize.Minimum,
					numWorldSize.Maximum);
			}
			finally
			{
				_isLoading = false;
			}
		}

		public void ConfigureForGame(GameInfo? gameData, bool isMinecraftBedrock)
		{
			_isLoading = true;
			try
			{
				ConfigureWorldSizeInput(gameData);
				_digitsOnlySeed = gameData?.Game.Equals(
					"Rust",
					StringComparison.OrdinalIgnoreCase) == true;

				GameManagementCapability capabilities = gameData == null
					? GameManagementCapability.None
					: GameFix.GetManagementCapabilities(gameData);
				if (isMinecraftBedrock)
				{
					capabilities |= GameManagementCapability.WorldSeed;
				}

				bool seedSupported =
					(capabilities & GameManagementCapability.WorldSeed) != 0;
				bool sizeSupported =
					(capabilities & GameManagementCapability.WorldSize) != 0;
				ConfigureManagedTextBox(
					txtWorldSeed,
					seedSupported,
					LocalizationManager.Get(gameData == null
						? "ServerSetup.Placeholder.SelectGame"
						: "ServerSetup.Placeholder.NotRequired"));
				numWorldSize.Tag = sizeSupported;
			}
			finally
			{
				_isLoading = false;
			}
		}

		public void ApplyDefaultWorldSize(GameInfo gameData)
		{
			ArgumentNullException.ThrowIfNull(gameData);
			ConfigureWorldSizeInput(gameData);
			int worldSize = IsSevenDaysToDie(gameData)
				? SevenDaysToDieConfiguration.NormalizeWorldSize(gameData.WorldSize)
				: gameData.WorldSize;
			if (worldSize > 0)
			{
				numWorldSize.Value = Math.Clamp(
					worldSize,
					numWorldSize.Minimum,
					numWorldSize.Maximum);
			}
		}

		public void ApplyAvailability(bool hasGame)
		{
			txtWorldSeed.Enabled = hasGame && IsRequired(txtWorldSeed);
			numWorldSize.Enabled = hasGame && numWorldSize.Tag is true;
		}

		public static bool IsSevenDaysToDie(GameInfo? gameData) =>
			gameData?.Game.Equals(
				"7 Days to Die",
				StringComparison.OrdinalIgnoreCase) == true;

		private void ConfigureWorldSizeInput(GameInfo? gameData)
		{
			if (IsSevenDaysToDie(gameData))
			{
				numWorldSize.Maximum = 10240;
				numWorldSize.Minimum = 6144;
				numWorldSize.Increment = 2048;
				return;
			}

			numWorldSize.Minimum = 50;
			numWorldSize.Maximum = 5000;
			numWorldSize.Increment = 1;
		}

		private void WorldSeedKeyPress(object? sender, KeyPressEventArgs eventArgs)
		{
			if (_digitsOnlySeed &&
				!char.IsControl(eventArgs.KeyChar) &&
				!char.IsDigit(eventArgs.KeyChar))
			{
				eventArgs.Handled = true;
			}
		}

		private void SettingsControlChanged(object? sender, EventArgs eventArgs)
		{
			if (!_isLoading)
				SettingsChanged?.Invoke(this, EventArgs.Empty);
		}

		private static string ReadManagedValue(TextBox textBox)
		{
			return textBox.Tag is ManagedTextBoxState { Required: false } state &&
				(textBox.ForeColor == Color.Gray || textBox.Text == state.Placeholder)
					? string.Empty
					: textBox.Text;
		}

		private static bool IsRequired(TextBox textBox) =>
			textBox.Tag is ManagedTextBoxState { Required: true };

		private static void ConfigureManagedTextBox(
			TextBox textBox,
			bool required,
			string placeholder)
		{
			textBox.GotFocus -= ManagedTextBoxGotFocus;
			textBox.LostFocus -= ManagedTextBoxLostFocus;
			textBox.Tag = new ManagedTextBoxState(required, placeholder);
			if (required)
			{
				if (textBox.ForeColor == Color.Gray)
				{
					textBox.Text = string.Empty;
				}
				textBox.ForeColor = SettingsPalette.PrimaryText;
				return;
			}

			textBox.ForeColor = Color.Gray;
			textBox.Text = placeholder;
			textBox.GotFocus += ManagedTextBoxGotFocus;
			textBox.LostFocus += ManagedTextBoxLostFocus;
		}

		private static void ManagedTextBoxGotFocus(object? sender, EventArgs eventArgs)
		{
			if (sender is TextBox textBox &&
				textBox.Tag is ManagedTextBoxState state &&
				textBox.Text == state.Placeholder)
			{
				textBox.Text = string.Empty;
				textBox.ForeColor = SettingsPalette.PrimaryText;
			}
		}

		private static void ManagedTextBoxLostFocus(object? sender, EventArgs eventArgs)
		{
			if (sender is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
			{
				textBox.ForeColor = Color.Gray;
				textBox.Text = textBox.Tag is ManagedTextBoxState state
					? state.Placeholder
					: string.Empty;
			}
		}

		private sealed record ManagedTextBoxState(
			bool Required,
			string Placeholder);
	}
}
