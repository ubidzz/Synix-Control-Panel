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
	}
}
