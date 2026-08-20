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
