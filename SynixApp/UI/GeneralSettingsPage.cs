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

namespace Synix_Control_Panel.SynixEngine
{
	public partial class GeneralSettingsPage : UserControl
	{
		public GeneralSettingsPage()
		{
			InitializeComponent();
			cmbSteamCmdDownloadMode.Items.AddRange(["Unlimited", "Limited"]);
			cmbSteamCmdDownloadMode.SelectedIndex = 0;
			cmbSteamCmdDownloadMode.SelectedIndexChanged +=
				SteamCmdDownloadModeSelectionChanged;
			numSteamCmdDownloadLimit.ValueChanged +=
				SteamCmdDownloadLimitValueChanged;
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
			get => cmbSteamCmdDownloadMode.SelectedIndex == 1;
			set
			{
				cmbSteamCmdDownloadMode.SelectedIndex = value ? 1 : 0;
				UpdateSteamCmdDownloadControls();
			}
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
	}
}
