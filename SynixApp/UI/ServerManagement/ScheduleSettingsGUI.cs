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

namespace Synix_Control_Panel.SynixApp.UI.ServerManagement
{
	public partial class ScheduleSettingsGUI : Form
	{
		public bool[] SelectedDays { get; private set; } = new bool[7];
		public string SelectedTime { get; private set; } = "04:00";
		public bool SmartMaintenanceEnabled { get; private set; } = true;
		public bool WaitForPlayers { get; private set; } = true;
		public int MaximumDelayMinutes { get; private set; } = 30;
		public bool BackupBeforeRestart { get; private set; } = true;
		public bool UpdateBeforeRestart { get; private set; }
		private ModernSettingsToggle _smartToggle = null!;
		private ModernSettingsToggle _waitToggle = null!;
		private ModernSettingsToggle _backupToggle = null!;
		private ModernSettingsToggle _updateToggle = null!;
		private NumericUpDown _delayMinutes = null!;

		public ScheduleSettingsGUI()
		{
			InitializeComponent();
			AddSmartMaintenanceControls();
			ThemeManager.Apply(this);
			numRestartHour.Value = 4;
			numRestartMinute.Value = 0;

			UIStyleHelper.InitializeToggles(this);
		}

		public ScheduleSettingsGUI(
			bool[] initialDays,
			string initialTime,
			bool smartMaintenanceEnabled = true,
			bool waitForPlayers = true,
			int maximumDelayMinutes = 30,
			bool backupBeforeRestart = true,
			bool updateBeforeRestart = false) : this()
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

			_smartToggle.Checked = smartMaintenanceEnabled;
			_waitToggle.Checked = waitForPlayers;
			_delayMinutes.Value = Math.Clamp(
				maximumDelayMinutes,
				(int)_delayMinutes.Minimum,
				(int)_delayMinutes.Maximum);
			_backupToggle.Checked = backupBeforeRestart;
			_updateToggle.Checked = updateBeforeRestart;
			UpdateSmartControlState();
		}

		private void btnSaveSchedule_Click(object sender, EventArgs e)
		{
			SelectedDays = new bool[]
			{
				chkSun.Checked, chkMon.Checked, chkTue.Checked,
				chkWed.Checked, chkThu.Checked, chkFri.Checked, chkSa.Checked
			};

			SelectedTime = $"{numRestartHour.Value:00}:{numRestartMinute.Value:00}";
			SmartMaintenanceEnabled = _smartToggle.Checked;
			WaitForPlayers = _waitToggle.Checked;
			MaximumDelayMinutes = (int)_delayMinutes.Value;
			BackupBeforeRestart = _backupToggle.Checked;
			UpdateBeforeRestart = _updateToggle.Checked;
			DialogResult = DialogResult.OK;
			Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void AddSmartMaintenanceControls()
		{
			ClientSize = new Size(700, 728);
			ModernSettingsCard smartCard = new()
			{
				Location = new Point(24, 466),
				Size = new Size(652, 170),
				FillColor = SettingsPalette.Card,
				BorderColor = SettingsPalette.Divider
			};
			smartCard.Controls.Add(CreateLocalizedLabel(
				"Schedule.Smart.Title",
				20, 14, 300, 26, true));
			smartCard.Controls.Add(CreateLocalizedLabel(
				"Schedule.Smart.Description",
				20, 42, 500, 40));
			_smartToggle = CreateToggle(
				574,
				18,
				"Schedule.Smart.Toggle.AccessibleName");
			_smartToggle.Checked = true;
			_smartToggle.CheckedChanged += (_, _) => UpdateSmartControlState();
			smartCard.Controls.Add(_smartToggle);

			smartCard.Controls.Add(CreateLocalizedLabel(
				"Schedule.Smart.WaitForPlayers",
				20, 92, 150, 28, true));
			_waitToggle = CreateToggle(
				164,
				90,
				"Schedule.Smart.WaitToggle.AccessibleName");
			_waitToggle.Checked = true;
			_waitToggle.CheckedChanged += (_, _) => UpdateSmartControlState();
			smartCard.Controls.Add(_waitToggle);
			smartCard.Controls.Add(CreateLocalizedLabel(
				"Schedule.Smart.MaximumDelay",
				242, 92, 140, 28, true));
			_delayMinutes = new NumericUpDown
			{
				Location = new Point(374, 90),
				Size = new Size(76, 32),
				Minimum = 0,
				Maximum = 720,
				Value = 30,
				BackColor = SettingsPalette.Input,
				ForeColor = SettingsPalette.PrimaryText,
				BorderStyle = BorderStyle.FixedSingle
			};
			smartCard.Controls.Add(_delayMinutes);
			smartCard.Controls.Add(CreateLocalizedLabel(
				"Schedule.Smart.Minutes",
				456, 94, 64, 24));

			smartCard.Controls.Add(CreateLocalizedLabel(
				"Schedule.Smart.Backup",
				20, 134, 70, 26, true));
			_backupToggle = CreateToggle(
				90,
				130,
				"Schedule.Smart.BackupToggle.AccessibleName");
			_backupToggle.Checked = true;
			smartCard.Controls.Add(_backupToggle);
			smartCard.Controls.Add(CreateLocalizedLabel(
				"Schedule.Smart.Update",
				242, 134, 70, 26, true));
			_updateToggle = CreateToggle(
				310,
				130,
				"Schedule.Smart.UpdateToggle.AccessibleName");
			smartCard.Controls.Add(_updateToggle);
			Controls.Add(smartCard);

			lblScheduleHint.Top = 656;
			btnCancel.Top = 662;
			btnSaveSchedule.Top = 662;
		}

		private void UpdateSmartControlState()
		{
			bool enabled = _smartToggle.Checked;
			_waitToggle.Enabled = enabled;
			_delayMinutes.Enabled = enabled && _waitToggle.Checked;
			_backupToggle.Enabled = enabled;
			_updateToggle.Enabled = enabled;
		}

		private static ModernSettingsToggle CreateToggle(
			int left,
			int top,
			string accessibleNameKey)
		{
			ModernSettingsToggle toggle = new()
			{
				Location = new Point(left, top)
			};
			LocalizationManager.BindAccessibleName(toggle, accessibleNameKey);
			return toggle;
		}

		private static Label CreateLocalizedLabel(
			string resourceKey,
			int left,
			int top,
			int width,
			int height,
			bool bold = false)
		{
			Label label = CreateLabel(
				LocalizationManager.Get(resourceKey),
				left,
				top,
				width,
				height,
				bold);
			LocalizationManager.BindText(label, resourceKey);
			return label;
		}

		private static Label CreateLabel(
			string text,
			int left,
			int top,
			int width,
			int height,
			bool bold = false) => new()
		{
			Text = text,
			Location = new Point(left, top),
			Size = new Size(width, height),
			ForeColor = bold ? SettingsPalette.PrimaryText : SettingsPalette.SecondaryText,
			Font = new Font("Segoe UI", 9.25F, bold ? FontStyle.Bold : FontStyle.Regular)
		};
	}
}
