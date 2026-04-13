namespace Synix_Control_Panel.ServerHandler
{
	partial class ScheduleSettingsGUI
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScheduleSettingsGUI));
			dtpRestartTime = new DateTimePicker();
			btnSaveSchedule = new Button();
			btnCancel = new Button();
			chkSun = new Synix_Control_Panel.UI.SynixToggle();
			chkMon = new Synix_Control_Panel.UI.SynixToggle();
			chkTue = new Synix_Control_Panel.UI.SynixToggle();
			chkWed = new Synix_Control_Panel.UI.SynixToggle();
			chkThu = new Synix_Control_Panel.UI.SynixToggle();
			chkFri = new Synix_Control_Panel.UI.SynixToggle();
			chkSa = new Synix_Control_Panel.UI.SynixToggle();
			SuspendLayout();
			// 
			// dtpRestartTime
			// 
			dtpRestartTime.Location = new Point(20, 20);
			dtpRestartTime.Name = "dtpRestartTime";
			dtpRestartTime.Size = new Size(100, 23);
			dtpRestartTime.TabIndex = 9;
			// 
			// btnSaveSchedule
			// 
			btnSaveSchedule.Location = new Point(73, 174);
			btnSaveSchedule.Name = "btnSaveSchedule";
			btnSaveSchedule.Size = new Size(80, 30);
			btnSaveSchedule.TabIndex = 8;
			btnSaveSchedule.Text = "Save";
			btnSaveSchedule.Click += btnSaveSchedule_Click;
			// 
			// btnCancel
			// 
			btnCancel.Location = new Point(159, 174);
			btnCancel.Name = "btnCancel";
			btnCancel.Size = new Size(80, 30);
			btnCancel.TabIndex = 7;
			btnCancel.Text = "Cancel";
			btnCancel.Click += btnCancel_Click;
			// 
			// chkSun
			// 
			chkSun.BackColor = Color.Transparent;
			chkSun.Location = new Point(12, 60);
			chkSun.Name = "chkSun";
			chkSun.Size = new Size(100, 28);
			chkSun.TabIndex = 6;
			chkSun.Tag = "Sunday";
			chkSun.UseVisualStyleBackColor = false;
			// 
			// chkMon
			// 
			chkMon.BackColor = Color.Transparent;
			chkMon.Location = new Point(118, 60);
			chkMon.Name = "chkMon";
			chkMon.Size = new Size(100, 28);
			chkMon.TabIndex = 5;
			chkMon.Tag = "Monday";
			chkMon.UseVisualStyleBackColor = false;
			// 
			// chkTue
			// 
			chkTue.BackColor = Color.Transparent;
			chkTue.Location = new Point(224, 60);
			chkTue.Name = "chkTue";
			chkTue.Size = new Size(100, 28);
			chkTue.TabIndex = 4;
			chkTue.Tag = "Tuesday";
			chkTue.UseVisualStyleBackColor = false;
			// 
			// chkWed
			// 
			chkWed.BackColor = Color.Transparent;
			chkWed.Location = new Point(12, 94);
			chkWed.Name = "chkWed";
			chkWed.Size = new Size(100, 28);
			chkWed.TabIndex = 3;
			chkWed.Tag = "Wednesday";
			chkWed.UseVisualStyleBackColor = false;
			// 
			// chkThu
			// 
			chkThu.BackColor = Color.Transparent;
			chkThu.Location = new Point(118, 94);
			chkThu.Name = "chkThu";
			chkThu.Size = new Size(100, 28);
			chkThu.TabIndex = 2;
			chkThu.Tag = "Thursday";
			chkThu.UseVisualStyleBackColor = false;
			// 
			// chkFri
			// 
			chkFri.BackColor = Color.Transparent;
			chkFri.Location = new Point(224, 94);
			chkFri.Name = "chkFri";
			chkFri.Size = new Size(100, 28);
			chkFri.TabIndex = 1;
			chkFri.Tag = "Friday";
			chkFri.UseVisualStyleBackColor = false;
			// 
			// chkSa
			// 
			chkSa.BackColor = Color.Transparent;
			chkSa.Location = new Point(12, 128);
			chkSa.Name = "chkSa";
			chkSa.Size = new Size(100, 28);
			chkSa.TabIndex = 0;
			chkSa.Tag = "Saturday";
			chkSa.UseVisualStyleBackColor = false;
			// 
			// ScheduleSettingsGUI
			// 
			BackgroundImage = Properties.Resources.background;
			BackgroundImageLayout = ImageLayout.Stretch;
			ClientSize = new Size(341, 215);
			Controls.Add(chkSa);
			Controls.Add(chkFri);
			Controls.Add(chkThu);
			Controls.Add(chkWed);
			Controls.Add(chkTue);
			Controls.Add(chkMon);
			Controls.Add(chkSun);
			Controls.Add(btnCancel);
			Controls.Add(btnSaveSchedule);
			Controls.Add(dtpRestartTime);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "ScheduleSettingsGUI";
			Text = "Maintence Schedule";
			ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.DateTimePicker dtpRestartTime;
		private System.Windows.Forms.Button btnSaveSchedule;
		private System.Windows.Forms.Button btnCancel;

		// 🎯 FIXED: Declared as SynixToggles for the Designer to recognize them
		private Synix_Control_Panel.UI.SynixToggle chkSun;
		private Synix_Control_Panel.UI.SynixToggle chkMon;
		private Synix_Control_Panel.UI.SynixToggle chkTue;
		private Synix_Control_Panel.UI.SynixToggle chkWed;
		private Synix_Control_Panel.UI.SynixToggle chkThu;
		private Synix_Control_Panel.UI.SynixToggle chkFri;
		private Synix_Control_Panel.UI.SynixToggle chkSa;
	}
}