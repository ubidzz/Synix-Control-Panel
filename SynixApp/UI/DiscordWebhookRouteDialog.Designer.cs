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

namespace Synix_Control_Panel
{
	partial class DiscordWebhookRouteDialog
	{
		private System.ComponentModel.IContainer components;
		private Label lblTitle;
		private Label lblDescription;
		private Label lblName;
		private TextBox txtName;
		private Label lblWebhook;
		private TextBox txtWebhook;
		private Label lblEnabled;
		private ModernSettingsToggle chkEnabled;
		private Label lblPreset;
		private ModernSettingsComboBox cmbPreset;
		private CheckedListBox lstEvents;
		private Label lblSelection;
		private Label lblStatus;
		private ModernSettingsButton btnTest;
		private ModernSettingsButton btnCancel;
		private ModernSettingsButton btnSave;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				components?.Dispose();
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			lblTitle = new Label();
			lblDescription = new Label();
			lblName = new Label();
			txtName = new TextBox();
			lblWebhook = new Label();
			txtWebhook = new TextBox();
			lblEnabled = new Label();
			chkEnabled = new ModernSettingsToggle();
			lblPreset = new Label();
			cmbPreset = new ModernSettingsComboBox();
			lstEvents = new CheckedListBox();
			lblSelection = new Label();
			lblStatus = new Label();
			btnTest = new ModernSettingsButton();
			btnCancel = new ModernSettingsButton();
			btnSave = new ModernSettingsButton();
			SuspendLayout();
			//
			// lblTitle
			//
			lblTitle.AutoSize = true;
			lblTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
			lblTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblTitle.Location = new Point(28, 22);
			lblTitle.Name = "lblTitle";
			lblTitle.Text = "Discord Destination";
			//
			// lblDescription
			//
			lblDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblDescription.Location = new Point(31, 64);
			lblDescription.Name = "lblDescription";
			lblDescription.Size = new Size(716, 40);
			lblDescription.Text = "Name this destination, paste its Discord webhook, and choose exactly which Synix events it receives.";
			//
			// lblName
			//
			lblName.AutoSize = true;
			lblName.ForeColor = Color.FromArgb(158, 172, 194);
			lblName.Location = new Point(31, 116);
			lblName.Name = "lblName";
			lblName.Text = "Destination name";
			//
			// txtName
			//
			txtName.BackColor = Color.FromArgb(12, 21, 36);
			txtName.BorderStyle = BorderStyle.FixedSingle;
			txtName.Font = new Font("Segoe UI", 10F);
			txtName.ForeColor = Color.FromArgb(245, 247, 251);
			txtName.Location = new Point(31, 140);
			txtName.Name = "txtName";
			txtName.Size = new Size(508, 29);
			//
			// lblEnabled
			//
			lblEnabled.ForeColor = Color.FromArgb(245, 247, 251);
			lblEnabled.Location = new Point(565, 140);
			lblEnabled.Name = "lblEnabled";
			lblEnabled.Size = new Size(110, 30);
			lblEnabled.Text = "Enabled";
			lblEnabled.TextAlign = ContentAlignment.MiddleLeft;
			//
			// chkEnabled
			//
			chkEnabled.Checked = true;
			chkEnabled.Location = new Point(681, 139);
			chkEnabled.Name = "chkEnabled";
			chkEnabled.Size = new Size(54, 30);
			//
			// lblWebhook
			//
			lblWebhook.AutoSize = true;
			lblWebhook.ForeColor = Color.FromArgb(158, 172, 194);
			lblWebhook.Location = new Point(31, 184);
			lblWebhook.Name = "lblWebhook";
			lblWebhook.Text = "Discord webhook URL";
			//
			// txtWebhook
			//
			txtWebhook.BackColor = Color.FromArgb(12, 21, 36);
			txtWebhook.BorderStyle = BorderStyle.FixedSingle;
			txtWebhook.Font = new Font("Segoe UI", 10F);
			txtWebhook.ForeColor = Color.FromArgb(245, 247, 251);
			txtWebhook.Location = new Point(31, 208);
			txtWebhook.Name = "txtWebhook";
			txtWebhook.Size = new Size(704, 29);
			//
			// lblPreset
			//
			lblPreset.AutoSize = true;
			lblPreset.ForeColor = Color.FromArgb(158, 172, 194);
			lblPreset.Location = new Point(31, 254);
			lblPreset.Name = "lblPreset";
			lblPreset.Text = "Quick event selection";
			//
			// cmbPreset
			//
			cmbPreset.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbPreset.Items.AddRange(new object[] { "All events", "Server status", "Maintenance", "Problems only", "Custom" });
			cmbPreset.Location = new Point(31, 278);
			cmbPreset.Name = "cmbPreset";
			cmbPreset.Size = new Size(250, 30);
			cmbPreset.SelectedIndexChanged += cmbPreset_SelectedIndexChanged;
			//
			// lstEvents
			//
			lstEvents.BackColor = Color.FromArgb(12, 21, 36);
			lstEvents.BorderStyle = BorderStyle.FixedSingle;
			lstEvents.CheckOnClick = true;
			lstEvents.Font = new Font("Segoe UI", 9.5F);
			lstEvents.ForeColor = Color.FromArgb(245, 247, 251);
			lstEvents.FormattingEnabled = true;
			lstEvents.IntegralHeight = false;
			lstEvents.Location = new Point(31, 320);
			lstEvents.Name = "lstEvents";
			lstEvents.Size = new Size(704, 210);
			lstEvents.ItemCheck += lstEvents_ItemCheck;
			//
			// lblSelection
			//
			lblSelection.ForeColor = Color.FromArgb(32, 214, 199);
			lblSelection.Location = new Point(300, 280);
			lblSelection.Name = "lblSelection";
			lblSelection.Size = new Size(435, 26);
			lblSelection.TextAlign = ContentAlignment.MiddleRight;
			//
			// lblStatus
			//
			lblStatus.ForeColor = Color.FromArgb(158, 172, 194);
			lblStatus.Location = new Point(31, 544);
			lblStatus.Name = "lblStatus";
			lblStatus.Size = new Size(704, 38);
			//
			// btnTest
			//
			btnTest.Location = new Point(31, 596);
			btnTest.Name = "btnTest";
			btnTest.Size = new Size(150, 42);
			btnTest.Text = "Send Test";
			btnTest.Click += btnTest_Click;
			//
			// btnCancel
			//
			btnCancel.DialogResult = DialogResult.Cancel;
			btnCancel.Location = new Point(465, 596);
			btnCancel.Name = "btnCancel";
			btnCancel.Size = new Size(125, 42);
			btnCancel.Text = "Cancel";
			//
			// btnSave
			//
			btnSave.Location = new Point(604, 596);
			btnSave.Name = "btnSave";
			btnSave.Size = new Size(131, 42);
			btnSave.Text = "Save Destination";
			btnSave.UseAccentStyle = true;
			btnSave.Click += btnSave_Click;
			//
			// DiscordWebhookRouteDialog
			//
			AcceptButton = btnSave;
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.FromArgb(8, 13, 24);
			CancelButton = btnCancel;
			ClientSize = new Size(770, 666);
			Controls.Add(lblTitle);
			Controls.Add(lblDescription);
			Controls.Add(lblName);
			Controls.Add(txtName);
			Controls.Add(lblEnabled);
			Controls.Add(chkEnabled);
			Controls.Add(lblWebhook);
			Controls.Add(txtWebhook);
			Controls.Add(lblPreset);
			Controls.Add(cmbPreset);
			Controls.Add(lstEvents);
			Controls.Add(lblSelection);
			Controls.Add(lblStatus);
			Controls.Add(btnTest);
			Controls.Add(btnCancel);
			Controls.Add(btnSave);
			Font = new Font("Segoe UI", 9F);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "DiscordWebhookRouteDialog";
			ShowIcon = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Discord Destination";
			ResumeLayout(false);
			PerformLayout();
		}
	}
}
