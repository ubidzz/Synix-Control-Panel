// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================
using System.ComponentModel;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class AdvancedSettingsPage : UserControl
	{
		public AdvancedSettingsPage()
		{
			InitializeComponent();
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ElevatedSystemTasks
		{
			get => chkElevatedTasks.Checked;
			set => chkElevatedTasks.Checked = value;
		}

		[Browsable(false)]
		public event EventHandler? ElevatedSystemTasksChanged
		{
			add => chkElevatedTasks.CheckedChanged += value;
			remove => chkElevatedTasks.CheckedChanged -= value;
		}
	}
}
