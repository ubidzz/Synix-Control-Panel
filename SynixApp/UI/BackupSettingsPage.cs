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
	public partial class BackupSettingsPage : UserControl
	{
		public BackupSettingsPage()
		{
			InitializeComponent();
			UpdatePathState();
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool UseCustomBackupPath
		{
			get => chkCustomBackup.Checked;
			set
			{
				chkCustomBackup.Checked = value;
				UpdatePathState();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string BackupPath
		{
			get => txtBackupPath.Text;
			set => txtBackupPath.Text = value ?? string.Empty;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int MaximumBackups
		{
			get => decimal.ToInt32(numMaxBackups.Value);
			set
			{
				int clampedValue = Math.Clamp(
					value,
					decimal.ToInt32(numMaxBackups.Minimum),
					decimal.ToInt32(numMaxBackups.Maximum));
				numMaxBackups.Value = clampedValue;
			}
		}

		[Browsable(false)]
		public event EventHandler? CustomBackupChanged
		{
			add => chkCustomBackup.CheckedChanged += value;
			remove => chkCustomBackup.CheckedChanged -= value;
		}

		[Browsable(false)]
		public event EventHandler? BrowseRequested
		{
			add => btnBrowseBackup.Click += value;
			remove => btnBrowseBackup.Click -= value;
		}

		[Browsable(false)]
		public event EventHandler? MaximumBackupsChanged
		{
			add => numMaxBackups.ValueChanged += value;
			remove => numMaxBackups.ValueChanged -= value;
		}

		private void chkCustomBackup_CheckedChanged(
			object? sender,
			EventArgs eventArgs)
		{
			UpdatePathState();
		}

		private void UpdatePathState()
		{
			bool enabled = chkCustomBackup.Checked;
			btnBrowseBackup.Enabled = enabled;
			txtBackupPath.ForeColor = enabled
				? Color.FromArgb(245, 247, 251)
				: Color.FromArgb(105, 124, 153);
			backupPathHost.BorderColor = enabled
				? Color.FromArgb(55, 76, 108)
				: Color.FromArgb(38, 52, 77);
			backupPathHost.Invalidate();
		}
	}
}
