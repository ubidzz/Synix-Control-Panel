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
using Synix_Control_Panel.SynixApp.Design;

namespace Synix_Control_Panel.SynixEngine
{
	internal sealed partial class FirstRunGuideDialog : Form
	{
		internal FirstRunGuideDialog()
		{
			InitializeComponent();
			ThemeManager.Apply(this);
		}

		private void TroubleshooterButton_Click(object? sender, EventArgs eventArgs)
		{
			using TroubleshooterDialog dialog = new();
			dialog.ShowDialog(this);
		}
	}
}
