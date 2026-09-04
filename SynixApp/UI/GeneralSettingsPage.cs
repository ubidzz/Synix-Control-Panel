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
using Synix_Control_Panel.SynixApp.Localization;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class GeneralSettingsPage : UserControl
	{
		private const string UnlimitedDownloadMode = "unlimited";
		private const string LimitedDownloadMode = "limited";
		private bool _updatingLocalizedOptions;

		public GeneralSettingsPage()
		{
			InitializeComponent();
			PopulateLocalizedOptions();
			cmbSteamCmdDownloadMode.SelectedIndexChanged +=
				SteamCmdDownloadModeSelectionChanged;
			numSteamCmdDownloadLimit.ValueChanged +=
				SteamCmdDownloadLimitValueChanged;
			cmbLanguage.SelectedIndexChanged += LanguageSelectionChanged;
			LocalizationManager.LanguageChanged +=
				LocalizationLanguageChanged;
			Disposed += (_, _) =>
				LocalizationManager.LanguageChanged -=
					LocalizationLanguageChanged;
			UpdateSteamCmdDownloadControls();
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ShowServerWindow
		{
			get => chkShowServerWindow.Checked;
			set => chkShowServerWindow.Checked = value;
		}

		[Browsable(false)]
		public event EventHandler? ShowServerWindowChanged
		{
			add => chkShowServerWindow.CheckedChanged += value;
			remove => chkShowServerWindow.CheckedChanged -= value;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool DarkMode
		{
			get => chkDarkMode.Checked;
			set => chkDarkMode.Checked = value;
		}

		[Browsable(false)]
		public event EventHandler? DarkModeChanged
		{
			add => chkDarkMode.CheckedChanged += value;
			remove => chkDarkMode.CheckedChanged -= value;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool LimitSteamCmdDownloadSpeed
		{
			get => string.Equals(
				(cmbSteamCmdDownloadMode.SelectedItem as LocalizedOption)?.Value,
				LimitedDownloadMode,
				StringComparison.Ordinal);
			set
			{
				SelectOption(
					cmbSteamCmdDownloadMode,
					value ? LimitedDownloadMode : UnlimitedDownloadMode);
				UpdateSteamCmdDownloadControls();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string UiLanguageCode
		{
			get => (cmbLanguage.SelectedItem as LocalizedOption)?.Value
				?? LocalizationManager.DefaultLanguageCode;
			set => SelectOption(
				cmbLanguage,
				value,
				LocalizationManager.DefaultLanguageCode);
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SteamCmdDownloadLimitMbps
		{
			get => numSteamCmdDownloadLimit.Value;
			set => numSteamCmdDownloadLimit.Value = value;
		}

		[Browsable(false)]
		public event EventHandler? SteamCmdDownloadModeChanged;

		[Browsable(false)]
		public event EventHandler? SteamCmdDownloadLimitChanged;

		[Browsable(false)]
		public event EventHandler? UiLanguageChanged;

		private void SteamCmdDownloadModeSelectionChanged(
			object? sender,
			EventArgs eventArgs)
		{
			UpdateSteamCmdDownloadControls();
			SteamCmdDownloadModeChanged?.Invoke(this, EventArgs.Empty);
		}

		private void SteamCmdDownloadLimitValueChanged(
			object? sender,
			EventArgs eventArgs)
		{
			SteamCmdDownloadLimitChanged?.Invoke(this, EventArgs.Empty);
		}

		private void UpdateSteamCmdDownloadControls()
		{
			bool limited = LimitSteamCmdDownloadSpeed;
			numSteamCmdDownloadLimit.Enabled = limited;
			lblSteamCmdDownloadUnit.Enabled = limited;
		}

		private void LanguageSelectionChanged(
			object? sender,
			EventArgs eventArgs)
		{
			if (!_updatingLocalizedOptions)
			{
				UiLanguageChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		private void LocalizationLanguageChanged(
			object? sender,
			EventArgs eventArgs)
		{
			PopulateLocalizedOptions();
		}

		private void PopulateLocalizedOptions()
		{
			string selectedDownloadMode =
				(cmbSteamCmdDownloadMode.SelectedItem as LocalizedOption)?.Value
				?? UnlimitedDownloadMode;
			string selectedLanguage =
				(cmbLanguage.SelectedItem as LocalizedOption)?.Value
				?? LocalizationManager.CurrentLanguageCode;

			_updatingLocalizedOptions = true;
			try
			{
				cmbSteamCmdDownloadMode.Items.Clear();
				cmbSteamCmdDownloadMode.Items.AddRange(
				[
					new LocalizedOption(
						UnlimitedDownloadMode,
						"Option.DownloadSpeed.Unlimited"),
					new LocalizedOption(
						LimitedDownloadMode,
						"Option.DownloadSpeed.Limited")
				]);
				SelectOption(
					cmbSteamCmdDownloadMode,
					selectedDownloadMode,
					UnlimitedDownloadMode);

				cmbLanguage.Items.Clear();
				foreach (LocalizationManager.SupportedLanguage language in
					LocalizationManager.SupportedLanguages)
				{
					cmbLanguage.Items.Add(language.ResourceKey == null
						? LocalizedOption.FromDisplayText(
							language.Code,
							language.DisplayName)
						: new LocalizedOption(
							language.Code,
							language.ResourceKey));
				}
				SelectOption(
					cmbLanguage,
					selectedLanguage,
					LocalizationManager.DefaultLanguageCode);
			}
			finally
			{
				_updatingLocalizedOptions = false;
			}
		}

		private static void SelectOption(
			ComboBox comboBox,
			string? value,
			string? fallback = null)
		{
			for (int index = 0; index < comboBox.Items.Count; index++)
			{
				if (comboBox.Items[index] is LocalizedOption option &&
					string.Equals(
						option.Value,
						value,
						StringComparison.OrdinalIgnoreCase))
				{
					comboBox.SelectedIndex = index;
					return;
				}
			}

			if (!string.IsNullOrWhiteSpace(fallback) &&
				!string.Equals(value, fallback, StringComparison.OrdinalIgnoreCase))
			{
				SelectOption(comboBox, fallback);
				return;
			}

			comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
		}
	}
}
