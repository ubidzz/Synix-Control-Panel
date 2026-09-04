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

namespace Synix_Control_Panel.SynixApp.UI.Discord
{
	partial class DiscordSettingsPage
	{
		private System.ComponentModel.IContainer components;
		private ModernSettingsCard cardMaster;
		private Label lblMasterTitle;
		private Label lblMasterDescription;
		private Label lblMasterEnabled;
		private ModernSettingsToggle chkMasterEnabled;
		private Label lblMasterWebhook;
		private TextBox txtMasterWebhook;
		private ModernSettingsButton btnTestMaster;
		private Label lblMasterPreset;
		private ModernSettingsComboBox cmbMasterPreset;
		private Label lblMasterEvents;
		private CheckedListBox lstMasterEvents;
		private Label lblMasterSummary;
		private ModernSettingsCard cardAdvanced;
		private Label lblAdvancedTitle;
		private Label lblAdvancedDescription;
		private Label lblRouteCount;
		private DataGridView gridRoutes;
		private ModernSettingsButton btnAdd;
		private ModernSettingsButton btnEdit;
		private ModernSettingsButton btnRemove;
		private ModernSettingsButton btnTestRoute;
		private Label lblStatus;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				components?.Dispose();
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			cardMaster = new ModernSettingsCard();
			lblMasterTitle = new Label();
			lblMasterDescription = new Label();
			lblMasterEnabled = new Label();
			chkMasterEnabled = new ModernSettingsToggle();
			lblMasterWebhook = new Label();
			txtMasterWebhook = new TextBox();
			btnTestMaster = new ModernSettingsButton();
			lblMasterPreset = new Label();
			cmbMasterPreset = new ModernSettingsComboBox();
			lblMasterEvents = new Label();
			lstMasterEvents = new CheckedListBox();
			lblMasterSummary = new Label();
			cardAdvanced = new ModernSettingsCard();
			lblAdvancedTitle = new Label();
			lblAdvancedDescription = new Label();
			lblRouteCount = new Label();
			gridRoutes = new DataGridView();
			btnAdd = new ModernSettingsButton();
			btnEdit = new ModernSettingsButton();
			btnRemove = new ModernSettingsButton();
			btnTestRoute = new ModernSettingsButton();
			lblStatus = new Label();
			cardMaster.SuspendLayout();
			cardAdvanced.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)gridRoutes).BeginInit();
			SuspendLayout();
			//
			// cardMaster
			//
			cardMaster.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardMaster.BackColor = Color.FromArgb(17, 27, 45);
			cardMaster.BorderColor = Color.FromArgb(38, 52, 77);
			cardMaster.Controls.Add(lblMasterTitle);
			cardMaster.Controls.Add(lblMasterDescription);
			cardMaster.Controls.Add(lblMasterEnabled);
			cardMaster.Controls.Add(chkMasterEnabled);
			cardMaster.Controls.Add(lblMasterWebhook);
			cardMaster.Controls.Add(txtMasterWebhook);
			cardMaster.Controls.Add(btnTestMaster);
			cardMaster.Controls.Add(lblMasterPreset);
			cardMaster.Controls.Add(cmbMasterPreset);
			cardMaster.Controls.Add(lblMasterEvents);
			cardMaster.Controls.Add(lstMasterEvents);
			cardMaster.Controls.Add(lblMasterSummary);
			cardMaster.FillColor = Color.FromArgb(17, 27, 45);
			cardMaster.Location = new Point(0, 0);
			cardMaster.Name = "cardMaster";
			cardMaster.Size = new Size(890, 270);
			//
			// lblMasterTitle
			//
			lblMasterTitle.AutoSize = true;
			lblMasterTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblMasterTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblMasterTitle.Location = new Point(24, 20);
			lblMasterTitle.Name = "lblMasterTitle";
			lblMasterTitle.Text = "Master Discord Webhook";
			//
			// lblMasterDescription
			//
			lblMasterDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblMasterDescription.Location = new Point(24, 49);
			lblMasterDescription.Name = "lblMasterDescription";
			lblMasterDescription.Size = new Size(470, 38);
			lblMasterDescription.Text = "Use one webhook for this server and choose the messages it should receive.";
			//
			// lblMasterEnabled
			//
			lblMasterEnabled.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lblMasterEnabled.ForeColor = Color.FromArgb(245, 247, 251);
			lblMasterEnabled.Location = new Point(734, 18);
			lblMasterEnabled.Name = "lblMasterEnabled";
			lblMasterEnabled.Size = new Size(72, 30);
			lblMasterEnabled.Text = "Enabled";
			lblMasterEnabled.TextAlign = ContentAlignment.MiddleRight;
			//
			// chkMasterEnabled
			//
			chkMasterEnabled.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			chkMasterEnabled.Location = new Point(812, 18);
			chkMasterEnabled.Name = "chkMasterEnabled";
			chkMasterEnabled.Size = new Size(54, 30);
			chkMasterEnabled.CheckedChanged += MasterSettingChanged;
			//
			// lblMasterWebhook
			//
			lblMasterWebhook.AutoSize = true;
			lblMasterWebhook.ForeColor = Color.FromArgb(158, 172, 194);
			lblMasterWebhook.Location = new Point(24, 86);
			lblMasterWebhook.Name = "lblMasterWebhook";
			lblMasterWebhook.Text = "Discord webhook URL";
			//
			// txtMasterWebhook
			//
			txtMasterWebhook.BackColor = Color.FromArgb(12, 21, 36);
			txtMasterWebhook.BorderStyle = BorderStyle.FixedSingle;
			txtMasterWebhook.Font = new Font("Segoe UI", 10F);
			txtMasterWebhook.ForeColor = Color.FromArgb(245, 247, 251);
			txtMasterWebhook.Location = new Point(24, 106);
			txtMasterWebhook.Name = "txtMasterWebhook";
			txtMasterWebhook.Size = new Size(470, 29);
			txtMasterWebhook.TextChanged += MasterSettingChanged;
			//
			// btnTestMaster
			//
			btnTestMaster.Location = new Point(344, 209);
			btnTestMaster.Name = "btnTestMaster";
			btnTestMaster.Size = new Size(150, 42);
			btnTestMaster.Text = "Send Test";
			btnTestMaster.Click += btnTestMaster_Click;
			//
			// lblMasterPreset
			//
			lblMasterPreset.AutoSize = true;
			lblMasterPreset.ForeColor = Color.FromArgb(158, 172, 194);
			lblMasterPreset.Location = new Point(24, 145);
			lblMasterPreset.Name = "lblMasterPreset";
			lblMasterPreset.Text = "Messages to send";
			//
			// cmbMasterPreset
			//
			cmbMasterPreset.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbMasterPreset.Location = new Point(24, 166);
			cmbMasterPreset.Name = "cmbMasterPreset";
			cmbMasterPreset.Size = new Size(250, 30);
			cmbMasterPreset.SelectedIndexChanged += cmbMasterPreset_SelectedIndexChanged;
			//
			// lblMasterEvents
			//
			lblMasterEvents.AutoSize = true;
			lblMasterEvents.ForeColor = Color.FromArgb(158, 172, 194);
			lblMasterEvents.Location = new Point(520, 44);
			lblMasterEvents.Name = "lblMasterEvents";
			lblMasterEvents.Text = "Individual events";
			//
			// lstMasterEvents
			//
			lstMasterEvents.BackColor = Color.FromArgb(12, 21, 36);
			lstMasterEvents.BorderStyle = BorderStyle.FixedSingle;
			lstMasterEvents.CheckOnClick = true;
			lstMasterEvents.ForeColor = Color.FromArgb(245, 247, 251);
			lstMasterEvents.FormattingEnabled = true;
			lstMasterEvents.IntegralHeight = false;
			lstMasterEvents.Location = new Point(520, 66);
			lstMasterEvents.Name = "lstMasterEvents";
			lstMasterEvents.Size = new Size(346, 185);
			lstMasterEvents.ItemCheck += lstMasterEvents_ItemCheck;
			//
			// lblMasterSummary
			//
			lblMasterSummary.ForeColor = Color.FromArgb(32, 214, 199);
			lblMasterSummary.Location = new Point(24, 202);
			lblMasterSummary.Name = "lblMasterSummary";
			lblMasterSummary.Size = new Size(250, 24);
			//
			// cardAdvanced
			//
			cardAdvanced.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cardAdvanced.BackColor = Color.FromArgb(17, 27, 45);
			cardAdvanced.BorderColor = Color.FromArgb(38, 52, 77);
			cardAdvanced.Controls.Add(lblAdvancedTitle);
			cardAdvanced.Controls.Add(lblAdvancedDescription);
			cardAdvanced.Controls.Add(lblRouteCount);
			cardAdvanced.Controls.Add(gridRoutes);
			cardAdvanced.Controls.Add(btnAdd);
			cardAdvanced.Controls.Add(btnEdit);
			cardAdvanced.Controls.Add(btnRemove);
			cardAdvanced.Controls.Add(btnTestRoute);
			cardAdvanced.Controls.Add(lblStatus);
			cardAdvanced.FillColor = Color.FromArgb(17, 27, 45);
			cardAdvanced.Location = new Point(0, 286);
			cardAdvanced.Name = "cardAdvanced";
			cardAdvanced.Size = new Size(890, 330);
			//
			// lblAdvancedTitle
			//
			lblAdvancedTitle.AutoSize = true;
			lblAdvancedTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblAdvancedTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblAdvancedTitle.Location = new Point(24, 18);
			lblAdvancedTitle.Name = "lblAdvancedTitle";
			lblAdvancedTitle.Text = "Advanced Discord Destinations";
			//
			// lblAdvancedDescription
			//
			lblAdvancedDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblAdvancedDescription.Location = new Point(24, 47);
			lblAdvancedDescription.Name = "lblAdvancedDescription";
			lblAdvancedDescription.Size = new Size(620, 35);
			lblAdvancedDescription.Text = "Send status, backups, maintenance, and problems to different Discord channels.";
			//
			// lblRouteCount
			//
			lblRouteCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lblRouteCount.ForeColor = Color.FromArgb(125, 165, 213);
			lblRouteCount.Location = new Point(650, 25);
			lblRouteCount.Name = "lblRouteCount";
			lblRouteCount.Size = new Size(216, 22);
			lblRouteCount.TextAlign = ContentAlignment.MiddleRight;
			//
			// gridRoutes
			//
			gridRoutes.AllowUserToAddRows = false;
			gridRoutes.AllowUserToDeleteRows = false;
			gridRoutes.AllowUserToResizeRows = false;
			gridRoutes.BackgroundColor = Color.FromArgb(12, 21, 36);
			gridRoutes.BorderStyle = BorderStyle.None;
			gridRoutes.ColumnHeadersHeight = 34;
			gridRoutes.Location = new Point(24, 86);
			gridRoutes.Name = "gridRoutes";
			gridRoutes.MultiSelect = false;
			gridRoutes.ReadOnly = true;
			gridRoutes.RowHeadersVisible = false;
			gridRoutes.RowTemplate.Height = 34;
			gridRoutes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			gridRoutes.Size = new Size(842, 154);
			gridRoutes.SelectionChanged += gridRoutes_SelectionChanged;
			gridRoutes.CellDoubleClick += gridRoutes_CellDoubleClick;
			//
			// buttons
			//
			btnAdd.Location = new Point(24, 254);
			btnAdd.Name = "btnAdd";
			btnAdd.Size = new Size(130, 42);
			btnAdd.Text = "Add Destination";
			btnAdd.UseAccentStyle = true;
			btnAdd.Click += btnAdd_Click;
			btnEdit.Location = new Point(164, 254);
			btnEdit.Name = "btnEdit";
			btnEdit.Size = new Size(100, 42);
			btnEdit.Text = "Edit";
			btnEdit.Click += btnEdit_Click;
			btnRemove.Location = new Point(274, 254);
			btnRemove.Name = "btnRemove";
			btnRemove.Size = new Size(100, 42);
			btnRemove.Text = "Remove";
			btnRemove.Click += btnRemove_Click;
			btnTestRoute.Location = new Point(384, 254);
			btnTestRoute.Name = "btnTestRoute";
			btnTestRoute.Size = new Size(120, 42);
			btnTestRoute.Text = "Send Test";
			btnTestRoute.Click += btnTestRoute_Click;
			//
			// lblStatus
			//
			lblStatus.ForeColor = Color.FromArgb(158, 172, 194);
			lblStatus.Location = new Point(522, 254);
			lblStatus.Name = "lblStatus";
			lblStatus.Size = new Size(344, 52);
			lblStatus.TextAlign = ContentAlignment.MiddleRight;
			//
			// DiscordSettingsPage
			//
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			AutoScroll = true;
			BackColor = Color.FromArgb(8, 13, 24);
			Controls.Add(cardMaster);
			Controls.Add(cardAdvanced);
			Name = "DiscordSettingsPage";
			Size = new Size(914, 496);
			cardMaster.ResumeLayout(false);
			cardMaster.PerformLayout();
			cardAdvanced.ResumeLayout(false);
			cardAdvanced.PerformLayout();
			((System.ComponentModel.ISupportInitialize)gridRoutes).EndInit();
			ResumeLayout(false);
		}
	}
}
