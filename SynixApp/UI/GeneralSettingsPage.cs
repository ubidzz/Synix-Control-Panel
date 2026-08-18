// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
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
	}
}
