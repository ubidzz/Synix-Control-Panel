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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Synix_Control_Panel.SynixEngine
{
	public partial class AppSettings : Form
	{
		public AppSettings()
		{
			UIStyleHelper.InitializeToggles(this);

			InitializeComponent();

			UIStyleHelper.InitializeToggles(this);
			chkCustomBackup.Checked = Properties.Settings.Default.UseCustomBackupPath;
			txtBackupPath.Text = Properties.Settings.Default.CustomBackupPath;

			// Grey out the textbox and button if the toggle is off
			txtBackupPath.Enabled = chkCustomBackup.Checked;
			btnBrowseBackup.Enabled = chkCustomBackup.Checked;

			chkPrivacyMode.Text = "Privacy Mode";
			chkPrivacyMode.Checked = Properties.Settings.Default.PrivacyMode;
		}

		private void chkCustomBackup_CheckedChanged(object sender, EventArgs e)
		{
			// 1. Toggle the UI elements
			txtBackupPath.Enabled = chkCustomBackup.Checked;
			btnBrowseBackup.Enabled = chkCustomBackup.Checked;

			// 2. Save the bool state
			Properties.Settings.Default.UseCustomBackupPath = chkCustomBackup.Checked;
			Properties.Settings.Default.Save();
		}

		private void btnBrowseBackup_Click(object sender, EventArgs e)
		{
			using (FolderBrowserDialog fbd = new FolderBrowserDialog())
			{
				fbd.Description = "Select a custom folder or drive to save all Synix server backups.";
				fbd.UseDescriptionForTitle = true;

				if (fbd.ShowDialog() == DialogResult.OK)
				{
					// 1. Display the string in the textbox
					txtBackupPath.Text = fbd.SelectedPath;

					// 2. Save the string path to settings
					Properties.Settings.Default.CustomBackupPath = fbd.SelectedPath;
					Properties.Settings.Default.Save();
				}
			}
		}

		private async void chkPrivacyMode_CheckedChanged(object sender, EventArgs e)
		{
			// 1. Save the setting immediately
			Properties.Settings.Default.PrivacyMode = chkPrivacyMode.Checked;
			Properties.Settings.Default.Save();

			// 2. Send the call to MainGUI to update its labels and network state
			if (MainGUI.Instance != null)
			{
				await MainGUI.Instance.UpdatePrivacyMode(chkPrivacyMode.Checked);
			}
		}
	}
}
