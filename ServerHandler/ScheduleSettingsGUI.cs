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
using System;
using System.Drawing;
using System.Windows.Forms;
using Synix_Control_Panel.UI;

namespace Synix_Control_Panel.ServerHandler
{
	public partial class ScheduleSettingsGUI : Form
	{
		public bool[] SelectedDays { get; private set; }
		public string SelectedTime { get; private set; }

		public ScheduleSettingsGUI(bool[] initialDays, string initialTime)
		{
			InitializeComponent();

			// 1. Setup Time Picker
			dtpRestartTime.Format = DateTimePickerFormat.Custom;
			dtpRestartTime.CustomFormat = "HH:mm";
			dtpRestartTime.ShowUpDown = true;

			// 2. Pre-fill Days
			if (initialDays != null && initialDays.Length == 7)
			{
				chkSun.Checked = initialDays[0];
				chkMon.Checked = initialDays[1];
				chkTue.Checked = initialDays[2];
				chkWed.Checked = initialDays[3];
				chkThu.Checked = initialDays[4];
				chkFri.Checked = initialDays[5];
				chkSa.Checked = initialDays[6];
			}

			// 3. Pre-fill Time
			if (DateTime.TryParseExact(initialTime, "HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime parsedTime))
				dtpRestartTime.Value = parsedTime;
			else
				dtpRestartTime.Value = DateTime.Today.AddHours(4);

			// 🎯 STYLE LINK: Calls the helper we just fixed
			Synix_Control_Panel.UI.UIStyleHelper.InitializeToggles(this);
		}

		private void btnSaveSchedule_Click(object sender, EventArgs e)
		{
			SelectedDays = new bool[]
			{
				chkSun.Checked, chkMon.Checked, chkTue.Checked,
				chkWed.Checked, chkThu.Checked, chkFri.Checked, chkSa.Checked
			};

			SelectedTime = dtpRestartTime.Value.ToString("HH:mm");
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}