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

namespace Synix_Control_Panel.ServerHandler
{
	public partial class ScheduleSettingsGUI : Form
	{
		public bool[] SelectedDays { get; private set; } = new bool[7];
		public string SelectedTime { get; private set; } = "04:00";

		public ScheduleSettingsGUI()
		{
			InitializeComponent();
			ThemeManager.Apply(this);
			numRestartHour.Value = 4;
			numRestartMinute.Value = 0;

			UIStyleHelper.InitializeToggles(this);
		}

		public ScheduleSettingsGUI(bool[] initialDays, string initialTime) : this()
		{

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

			if (DateTime.TryParseExact(initialTime, "HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime parsedTime))
			{
				numRestartHour.Value = parsedTime.Hour;
				numRestartMinute.Value = parsedTime.Minute;
			}
			else
			{
				numRestartHour.Value = 4;
				numRestartMinute.Value = 0;
			}
		}

		private void btnSaveSchedule_Click(object sender, EventArgs e)
		{
			SelectedDays = new bool[]
			{
				chkSun.Checked, chkMon.Checked, chkTue.Checked,
				chkWed.Checked, chkThu.Checked, chkFri.Checked, chkSa.Checked
			};

			SelectedTime = $"{numRestartHour.Value:00}:{numRestartMinute.Value:00}";
			DialogResult = DialogResult.OK;
			Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
