/*
 * Copyright (c) 2026 ubidzz. All Rights Reserved.
 *
 * This file is part of Synix Control Panel.
 *
 * This code is provided for transparent viewing and personal use only.
 * Unauthorized distribution, public modification, or commercial 
 * use of this source code or the compiled executable is strictly 
 * prohibited. Please refer to the LICENSE file in the root 
 * directory for full terms.
 */
namespace Synix_Control_Panel.Help
{
	public partial class DefaultArgumentsDisplay : Form
	{
		public DefaultArgumentsDisplay(string requiredArgs)
		{
			InitializeComponent();
			txtArgs.Text = requiredArgs;
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
