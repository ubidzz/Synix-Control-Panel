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
			InitializeComponent();

			UIStyleHelper.InitializeToggles(this);
			chkCustomBackup.Text = "Activate";
			chkRunAsAdmin.Text = "Activate";
			chkPrivacyMode.Text = "Activate";
			chkShowServerWindow.Text = "Activate";

			chkCustomBackup.Checked = Properties.Settings.Default.UseCustomBackupPath;
			txtBackupPath.Text = Properties.Settings.Default.CustomBackupPath;
			btnBrowseBackup.Enabled = chkCustomBackup.Checked;
			chkPrivacyMode.Checked = Properties.Settings.Default.PrivacyMode;
			chkRunAsAdmin.Checked = Properties.Settings.Default.enableRunAsAdmin;
			numMaxBackups.Value = Properties.Settings.Default.MaxBackups;
			chkShowServerWindow.Checked = Properties.Settings.Default.ShowServerWindow;
		}

		private void numMaxBackups_ValueChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.MaxBackups = (int)numMaxBackups.Value;
			Properties.Settings.Default.Save();
		}

		private void chkCustomBackup_CheckedChanged(object sender, EventArgs e)
		{
			btnBrowseBackup.Enabled = chkCustomBackup.Checked;

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
					txtBackupPath.Text = fbd.SelectedPath;

					Properties.Settings.Default.CustomBackupPath = fbd.SelectedPath;
					Properties.Settings.Default.Save();
				}
			}
		}

		private async void chkPrivacyMode_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.PrivacyMode = chkPrivacyMode.Checked;
			Properties.Settings.Default.Save();

			if (MainGUI.Instance != null)
			{
				await MainGUI.Instance.UpdatePrivacyMode(chkPrivacyMode.Checked);
			}
		}

		private void chkRunAsAdmin_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.enableRunAsAdmin = chkRunAsAdmin.Checked;
			Properties.Settings.Default.Save();
		}

		private void chkShowServerWindow_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.ShowServerWindow = chkShowServerWindow.Checked;
			Properties.Settings.Default.Save();
		}
	}
}
