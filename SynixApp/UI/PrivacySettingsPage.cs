// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using System.ComponentModel;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class PrivacySettingsPage : UserControl
	{
		public PrivacySettingsPage()
		{
			InitializeComponent();
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool PrivacyMode
		{
			get => chkPrivacyMode.Checked;
			set => chkPrivacyMode.Checked = value;
		}

		[Browsable(false)]
		public event EventHandler? PrivacyModeChanged
		{
			add => chkPrivacyMode.CheckedChanged += value;
			remove => chkPrivacyMode.CheckedChanged -= value;
		}
	}
}
