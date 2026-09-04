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
	partial class ScheduleSettingsGUI
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				components?.Dispose();
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScheduleSettingsGUI));
			headerGlyph = new ModernSettingsGlyph();
			lblHeading = new Label();
			lblSubtitle = new Label();
			timeCard = new ModernSettingsCard();
			lblTimeTitle = new Label();
			lblTimeHelp = new Label();
			lblHour = new Label();
			lblMinute = new Label();
			lblTimeSeparator = new Label();
			numRestartHour = new ModernSettingsNumericUpDown();
			numRestartMinute = new ModernSettingsNumericUpDown();
			daysCard = new ModernSettingsCard();
			lblDaysTitle = new Label();
			lblDaysHelp = new Label();
			chkSun = new SynixToggle();
			chkMon = new SynixToggle();
			chkTue = new SynixToggle();
			chkWed = new SynixToggle();
			chkThu = new SynixToggle();
			chkFri = new SynixToggle();
			chkSa = new SynixToggle();
			lblScheduleHint = new Label();
			btnCancel = new ModernSettingsButton();
			btnSaveSchedule = new ModernSettingsButton();
			timeCard.SuspendLayout();
			(numRestartHour).BeginInit();
			(numRestartMinute).BeginInit();
			daysCard.SuspendLayout();
			SuspendLayout();
			// 
			// headerGlyph
			// 
			headerGlyph.BackColor = Color.FromArgb(17, 27, 45);
			headerGlyph.Font = new Font("Segoe UI Symbol", 15F);
			headerGlyph.ForeColor = Color.FromArgb(32, 214, 199);
			headerGlyph.Glyph = "•";
			headerGlyph.Location = new Point(24, 24);
			headerGlyph.Name = "headerGlyph";
			headerGlyph.Size = new Size(52, 52);
			headerGlyph.TabIndex = 0;
			headerGlyph.Text = LocalizationManager.Get("Text.AD244EA43ED037EF1730");
			// 
			// lblHeading
			// 
			lblHeading.Font = new Font("Segoe UI", 19F, FontStyle.Bold);
			lblHeading.ForeColor = Color.FromArgb(245, 247, 251);
			lblHeading.Location = new Point(92, 22);
			lblHeading.Name = "lblHeading";
			lblHeading.Size = new Size(570, 39);
			lblHeading.TabIndex = 1;
			lblHeading.Text = LocalizationManager.Get("Text.356D7C14F9D27E3F02B3");
			// 
			// lblSubtitle
			// 
			lblSubtitle.ForeColor = Color.FromArgb(158, 172, 194);
			lblSubtitle.Location = new Point(94, 62);
			lblSubtitle.Name = "lblSubtitle";
			lblSubtitle.Size = new Size(568, 28);
			lblSubtitle.TabIndex = 2;
			lblSubtitle.Text = LocalizationManager.Get("Text.54FE09AF7E87A247A21A");
			// 
			// timeCard
			// 
			timeCard.BackColor = Color.FromArgb(17, 27, 45);
			timeCard.BorderColor = Color.FromArgb(38, 52, 77);
			timeCard.Controls.Add(lblTimeTitle);
			timeCard.Controls.Add(lblTimeHelp);
			timeCard.Controls.Add(lblHour);
			timeCard.Controls.Add(lblMinute);
			timeCard.Controls.Add(lblTimeSeparator);
			timeCard.Controls.Add(numRestartHour);
			timeCard.Controls.Add(numRestartMinute);
			timeCard.FillColor = Color.FromArgb(17, 27, 45);
			timeCard.Location = new Point(24, 108);
			timeCard.Margin = new Padding(0, 0, 0, 16);
			timeCard.Name = "timeCard";
			timeCard.Size = new Size(652, 108);
			timeCard.TabIndex = 3;
			// 
			// lblTimeTitle
			// 
			lblTimeTitle.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
			lblTimeTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblTimeTitle.Location = new Point(20, 18);
			lblTimeTitle.Name = "lblTimeTitle";
			lblTimeTitle.Size = new Size(360, 26);
			lblTimeTitle.TabIndex = 0;
			lblTimeTitle.Text = LocalizationManager.Get("Text.BBC2611830E1BE782900");
			// 
			// lblTimeHelp
			// 
			lblTimeHelp.ForeColor = Color.FromArgb(158, 172, 194);
			lblTimeHelp.Location = new Point(20, 50);
			lblTimeHelp.Name = "lblTimeHelp";
			lblTimeHelp.Size = new Size(394, 38);
			lblTimeHelp.TabIndex = 1;
			lblTimeHelp.Text = LocalizationManager.Get("Text.F2029FAFB53EFFEE6890");
			// 
			// lblHour
			// 
			lblHour.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
			lblHour.ForeColor = Color.FromArgb(158, 172, 194);
			lblHour.Location = new Point(426, 15);
			lblHour.Name = "lblHour";
			lblHour.Size = new Size(100, 22);
			lblHour.TabIndex = 2;
			lblHour.Text = LocalizationManager.Get("Text.FAA9DECB5DCE3D4283A9");
			lblHour.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// lblMinute
			// 
			lblMinute.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
			lblMinute.ForeColor = Color.FromArgb(158, 172, 194);
			lblMinute.Location = new Point(548, 15);
			lblMinute.Name = "lblMinute";
			lblMinute.Size = new Size(84, 22);
			lblMinute.TabIndex = 3;
			lblMinute.Text = LocalizationManager.Get("Text.1EB03F3919CF225C90CE");
			lblMinute.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// lblTimeSeparator
			// 
			lblTimeSeparator.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
			lblTimeSeparator.ForeColor = Color.FromArgb(245, 247, 251);
			lblTimeSeparator.Location = new Point(526, 46);
			lblTimeSeparator.Name = "lblTimeSeparator";
			lblTimeSeparator.Size = new Size(22, 32);
			lblTimeSeparator.TabIndex = 4;
			lblTimeSeparator.Text = LocalizationManager.Get("Text.E7AC0786668E0FF0F02B");
			lblTimeSeparator.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// numRestartHour
			// 
			numRestartHour.AccessibleName = LocalizationManager.Get("Text.A95511418E3B85D2CCA6");
			numRestartHour.AccessibleRole = AccessibleRole.SpinButton;
			numRestartHour.BackColor = Color.FromArgb(12, 21, 36);
			numRestartHour.Font = new Font("Segoe UI", 11F);
			numRestartHour.ForeColor = Color.FromArgb(245, 247, 251);
			numRestartHour.Location = new Point(426, 40);
			numRestartHour.Maximum = 23;
			numRestartHour.Minimum = 0;
			numRestartHour.Name = "numRestartHour";
			numRestartHour.Size = new Size(100, 42);
			numRestartHour.TabIndex = 5;
			numRestartHour.Value = 4;
			// 
			// numRestartMinute
			// 
			numRestartMinute.AccessibleName = LocalizationManager.Get("Text.7A8126228870E99BDC43");
			numRestartMinute.AccessibleRole = AccessibleRole.SpinButton;
			numRestartMinute.BackColor = Color.FromArgb(12, 21, 36);
			numRestartMinute.Font = new Font("Segoe UI", 11F);
			numRestartMinute.ForeColor = Color.FromArgb(245, 247, 251);
			numRestartMinute.Location = new Point(548, 40);
			numRestartMinute.Maximum = 59;
			numRestartMinute.Minimum = 0;
			numRestartMinute.Name = "numRestartMinute";
			numRestartMinute.Size = new Size(84, 42);
			numRestartMinute.TabIndex = 6;
			numRestartMinute.Value = 0;
			// 
			// daysCard
			// 
			daysCard.BackColor = Color.FromArgb(17, 27, 45);
			daysCard.BorderColor = Color.FromArgb(38, 52, 77);
			daysCard.Controls.Add(lblDaysTitle);
			daysCard.Controls.Add(lblDaysHelp);
			daysCard.Controls.Add(chkSun);
			daysCard.Controls.Add(chkMon);
			daysCard.Controls.Add(chkTue);
			daysCard.Controls.Add(chkWed);
			daysCard.Controls.Add(chkThu);
			daysCard.Controls.Add(chkFri);
			daysCard.Controls.Add(chkSa);
			daysCard.FillColor = Color.FromArgb(17, 27, 45);
			daysCard.Location = new Point(24, 232);
			daysCard.Margin = new Padding(0, 0, 0, 16);
			daysCard.Name = "daysCard";
			daysCard.Size = new Size(652, 220);
			daysCard.TabIndex = 4;
			// 
			// lblDaysTitle
			// 
			lblDaysTitle.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
			lblDaysTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblDaysTitle.Location = new Point(20, 16);
			lblDaysTitle.Name = "lblDaysTitle";
			lblDaysTitle.Size = new Size(612, 26);
			lblDaysTitle.TabIndex = 0;
			lblDaysTitle.Text = LocalizationManager.Get("Text.E8A9279E12900267A1DF");
			// 
			// lblDaysHelp
			// 
			lblDaysHelp.ForeColor = Color.FromArgb(158, 172, 194);
			lblDaysHelp.Location = new Point(20, 43);
			lblDaysHelp.Name = "lblDaysHelp";
			lblDaysHelp.Size = new Size(612, 24);
			lblDaysHelp.TabIndex = 1;
			lblDaysHelp.Text = LocalizationManager.Get("Text.6440A1A6AB337CD371BA");
			// 
			// chkSun
			// 
			chkSun.BackColor = Color.Transparent;
			chkSun.Location = new Point(20, 78);
			chkSun.Name = "chkSun";
			chkSun.Size = new Size(190, 36);
			chkSun.TabIndex = 2;
			chkSun.Tag = "Sunday";
			chkSun.UseVisualStyleBackColor = false;
			// 
			// chkMon
			// 
			chkMon.BackColor = Color.Transparent;
			chkMon.Location = new Point(231, 78);
			chkMon.Name = "chkMon";
			chkMon.Size = new Size(190, 36);
			chkMon.TabIndex = 3;
			chkMon.Tag = "Monday";
			chkMon.UseVisualStyleBackColor = false;
			// 
			// chkTue
			// 
			chkTue.BackColor = Color.Transparent;
			chkTue.Location = new Point(442, 78);
			chkTue.Name = "chkTue";
			chkTue.Size = new Size(190, 36);
			chkTue.TabIndex = 4;
			chkTue.Tag = "Tuesday";
			chkTue.UseVisualStyleBackColor = false;
			// 
			// chkWed
			// 
			chkWed.BackColor = Color.Transparent;
			chkWed.Location = new Point(20, 124);
			chkWed.Name = "chkWed";
			chkWed.Size = new Size(190, 36);
			chkWed.TabIndex = 5;
			chkWed.Tag = "Wednesday";
			chkWed.UseVisualStyleBackColor = false;
			// 
			// chkThu
			// 
			chkThu.BackColor = Color.Transparent;
			chkThu.Location = new Point(231, 124);
			chkThu.Name = "chkThu";
			chkThu.Size = new Size(190, 36);
			chkThu.TabIndex = 6;
			chkThu.Tag = "Thursday";
			chkThu.UseVisualStyleBackColor = false;
			// 
			// chkFri
			// 
			chkFri.BackColor = Color.Transparent;
			chkFri.Location = new Point(442, 124);
			chkFri.Name = "chkFri";
			chkFri.Size = new Size(190, 36);
			chkFri.TabIndex = 7;
			chkFri.Tag = "Friday";
			chkFri.UseVisualStyleBackColor = false;
			// 
			// chkSa
			// 
			chkSa.BackColor = Color.Transparent;
			chkSa.Location = new Point(20, 170);
			chkSa.Name = "chkSa";
			chkSa.Size = new Size(190, 36);
			chkSa.TabIndex = 8;
			chkSa.Tag = "Saturday";
			chkSa.UseVisualStyleBackColor = false;
			// 
			// lblScheduleHint
			// 
			lblScheduleHint.ForeColor = Color.FromArgb(158, 172, 194);
			lblScheduleHint.Location = new Point(24, 474);
			lblScheduleHint.Name = "lblScheduleHint";
			lblScheduleHint.Size = new Size(390, 40);
			lblScheduleHint.TabIndex = 5;
			lblScheduleHint.Text = LocalizationManager.Get("Text.D9718558CFBB91F44CFF");
			// 
			// btnCancel
			// 
			btnCancel.BackColor = Color.FromArgb(12, 21, 36);
			btnCancel.DialogResult = DialogResult.Cancel;
			btnCancel.FlatStyle = FlatStyle.Flat;
			btnCancel.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnCancel.ForeColor = Color.FromArgb(245, 247, 251);
			btnCancel.Location = new Point(460, 478);
			btnCancel.Name = "btnCancel";
			btnCancel.Size = new Size(102, 42);
			btnCancel.TabIndex = 6;
			btnCancel.Text = LocalizationManager.Get("Text.19766ED6CCB2F4A32778");
			btnCancel.UseVisualStyleBackColor = false;
			btnCancel.Click += btnCancel_Click;
			// 
			// btnSaveSchedule
			// 
			btnSaveSchedule.BackColor = Color.FromArgb(12, 21, 36);
			btnSaveSchedule.FlatStyle = FlatStyle.Flat;
			btnSaveSchedule.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnSaveSchedule.ForeColor = Color.FromArgb(245, 247, 251);
			btnSaveSchedule.Location = new Point(574, 478);
			btnSaveSchedule.Name = "btnSaveSchedule";
			btnSaveSchedule.Size = new Size(102, 42);
			btnSaveSchedule.TabIndex = 7;
			btnSaveSchedule.Text = LocalizationManager.Get("Text.1509F561F2416598629B");
			btnSaveSchedule.UseAccentStyle = true;
			btnSaveSchedule.UseVisualStyleBackColor = false;
			btnSaveSchedule.Click += btnSaveSchedule_Click;
			// 
			// ScheduleSettingsGUI
			// 
			AcceptButton = btnSaveSchedule;
			AutoScaleDimensions = new SizeF(7F, 17F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.FromArgb(8, 13, 24);
			CancelButton = btnCancel;
			ClientSize = new Size(700, 542);
			Controls.Add(headerGlyph);
			Controls.Add(lblHeading);
			Controls.Add(lblSubtitle);
			Controls.Add(timeCard);
			Controls.Add(daysCard);
			Controls.Add(lblScheduleHint);
			Controls.Add(btnCancel);
			Controls.Add(btnSaveSchedule);
			Font = new Font("Segoe UI", 10F);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "ScheduleSettingsGUI";
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = LocalizationManager.Get("Text.356D7C14F9D27E3F02B3");
			timeCard.ResumeLayout(false);
			(numRestartHour).EndInit();
			(numRestartMinute).EndInit();
			daysCard.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private ModernSettingsGlyph headerGlyph;
		private Label lblHeading;
		private Label lblSubtitle;
		private ModernSettingsCard timeCard;
		private Label lblTimeTitle;
		private Label lblTimeHelp;
		private Label lblHour;
		private Label lblMinute;
		private Label lblTimeSeparator;
		private ModernSettingsNumericUpDown numRestartHour;
		private ModernSettingsNumericUpDown numRestartMinute;
		private ModernSettingsCard daysCard;
		private Label lblDaysTitle;
		private Label lblDaysHelp;
		private SynixToggle chkSun;
		private SynixToggle chkMon;
		private SynixToggle chkTue;
		private SynixToggle chkWed;
		private SynixToggle chkThu;
		private SynixToggle chkFri;
		private SynixToggle chkSa;
		private Label lblScheduleHint;
		private ModernSettingsButton btnCancel;
		private ModernSettingsButton btnSaveSchedule;
	}
}
