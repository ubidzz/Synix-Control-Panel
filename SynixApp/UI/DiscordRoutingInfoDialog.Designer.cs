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

namespace Synix_Control_Panel.Help
{
	partial class DiscordRoutingInfoDialog
	{
		private System.ComponentModel.IContainer components;
		private Panel titleBar;
		private PictureBox picLogo;
		private Label lblWindowTitle;
		private Button btnTitleClose;
		private Label titleBottomBorder;
		private Label lblHeading;
		private Label lblDescription;
		private ModernSettingsCard informationCard;
		private Label lblInformationIcon;
		private Label lblInformationTitle;
		private Label lblInformationText;
		private DataGridView gridRoutes;
		private DataGridViewTextBoxColumn statusColumn;
		private DataGridViewTextBoxColumn destinationColumn;
		private DataGridViewTextBoxColumn eventsColumn;
		private DataGridViewTextBoxColumn webhookColumn;
		private Label lblCount;
		private ModernSettingsButton btnClose;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				components?.Dispose();
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			titleBar = new Panel();
			picLogo = new PictureBox();
			lblWindowTitle = new Label();
			btnTitleClose = new Button();
			titleBottomBorder = new Label();
			lblHeading = new Label();
			lblDescription = new Label();
			informationCard = new ModernSettingsCard();
			lblInformationIcon = new Label();
			lblInformationTitle = new Label();
			lblInformationText = new Label();
			gridRoutes = new DataGridView();
			statusColumn = new DataGridViewTextBoxColumn();
			destinationColumn = new DataGridViewTextBoxColumn();
			eventsColumn = new DataGridViewTextBoxColumn();
			webhookColumn = new DataGridViewTextBoxColumn();
			lblCount = new Label();
			btnClose = new ModernSettingsButton();
			titleBar.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
			informationCard.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)gridRoutes).BeginInit();
			SuspendLayout();
			//
			// titleBar
			//
			titleBar.BackColor = Color.FromArgb(6, 12, 22);
			titleBar.Controls.Add(picLogo);
			titleBar.Controls.Add(lblWindowTitle);
			titleBar.Controls.Add(btnTitleClose);
			titleBar.Controls.Add(titleBottomBorder);
			titleBar.Dock = DockStyle.Top;
			titleBar.Location = new Point(1, 1);
			titleBar.Name = "titleBar";
			titleBar.Size = new Size(938, 56);
			titleBar.TabIndex = 0;
			titleBar.MouseDown += TitleBar_MouseDown;
			//
			// picLogo
			//
			picLogo.Image = Properties.Resources.synix_logo;
			picLogo.Location = new Point(18, 13);
			picLogo.Name = "picLogo";
			picLogo.Size = new Size(30, 30);
			picLogo.SizeMode = PictureBoxSizeMode.Zoom;
			picLogo.TabStop = false;
			picLogo.MouseDown += TitleBar_MouseDown;
			//
			// lblWindowTitle
			//
			lblWindowTitle.AutoSize = true;
			lblWindowTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblWindowTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblWindowTitle.Location = new Point(58, 17);
			lblWindowTitle.Name = "lblWindowTitle";
			lblWindowTitle.Text = "Discord Webhooks";
			lblWindowTitle.MouseDown += TitleBar_MouseDown;
			//
			// btnTitleClose
			//
			btnTitleClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnTitleClose.FlatAppearance.BorderSize = 0;
			btnTitleClose.FlatStyle = FlatStyle.Flat;
			btnTitleClose.Location = new Point(888, 0);
			btnTitleClose.Name = "btnTitleClose";
			btnTitleClose.Size = new Size(50, 55);
			btnTitleClose.TabIndex = 1;
			btnTitleClose.Text = "✕";
			btnTitleClose.Click += btnClose_Click;
			//
			// titleBottomBorder
			//
			titleBottomBorder.BackColor = Color.FromArgb(38, 52, 77);
			titleBottomBorder.Dock = DockStyle.Bottom;
			titleBottomBorder.Location = new Point(0, 55);
			titleBottomBorder.Name = "titleBottomBorder";
			titleBottomBorder.Size = new Size(938, 1);
			//
			// lblHeading
			//
			lblHeading.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblHeading.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
			lblHeading.ForeColor = Color.FromArgb(245, 247, 251);
			lblHeading.Location = new Point(28, 78);
			lblHeading.Name = "lblHeading";
			lblHeading.Size = new Size(884, 42);
			lblHeading.Text = "Discord Webhooks";
			//
			// lblDescription
			//
			lblDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblDescription.Location = new Point(31, 120);
			lblDescription.Name = "lblDescription";
			lblDescription.Size = new Size(881, 38);
			lblDescription.Text = "See which Discord destination receives each type of Synix notification for this server.";
			//
			// informationCard
			//
			informationCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			informationCard.BackColor = Color.FromArgb(17, 27, 45);
			informationCard.BorderColor = Color.FromArgb(38, 52, 77);
			informationCard.Controls.Add(lblInformationIcon);
			informationCard.Controls.Add(lblInformationTitle);
			informationCard.Controls.Add(lblInformationText);
			informationCard.FillColor = Color.FromArgb(17, 27, 45);
			informationCard.Location = new Point(28, 158);
			informationCard.Name = "informationCard";
			informationCard.Size = new Size(884, 76);
			//
			// lblInformationIcon
			//
			lblInformationIcon.BackColor = Color.FromArgb(28, 75, 91);
			lblInformationIcon.Font = new Font("Segoe UI Symbol", 15F, FontStyle.Bold);
			lblInformationIcon.ForeColor = Color.FromArgb(32, 214, 199);
			lblInformationIcon.Location = new Point(16, 14);
			lblInformationIcon.Name = "lblInformationIcon";
			lblInformationIcon.Size = new Size(48, 48);
			lblInformationIcon.Text = "◆";
			lblInformationIcon.TextAlign = ContentAlignment.MiddleCenter;
			//
			// lblInformationTitle
			//
			lblInformationTitle.AutoSize = true;
			lblInformationTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
			lblInformationTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblInformationTitle.Location = new Point(80, 14);
			lblInformationTitle.Name = "lblInformationTitle";
			lblInformationTitle.Text = "Webhook secrets stay protected";
			//
			// lblInformationText
			//
			lblInformationText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblInformationText.ForeColor = Color.FromArgb(158, 172, 194);
			lblInformationText.Location = new Point(80, 36);
			lblInformationText.Name = "lblInformationText";
			lblInformationText.Size = new Size(782, 30);
			lblInformationText.Text = "Only a masked webhook identifier is shown. Open Server Settings to view or edit the saved destination.";
			//
			// gridRoutes
			//
			gridRoutes.AllowUserToAddRows = false;
			gridRoutes.AllowUserToDeleteRows = false;
			gridRoutes.AllowUserToResizeRows = false;
			gridRoutes.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			gridRoutes.BackgroundColor = Color.FromArgb(12, 21, 36);
			gridRoutes.BorderStyle = BorderStyle.None;
			gridRoutes.ColumnHeadersHeight = 40;
			gridRoutes.Columns.AddRange(new DataGridViewColumn[] { statusColumn, destinationColumn, eventsColumn, webhookColumn });
			gridRoutes.Location = new Point(28, 250);
			gridRoutes.MultiSelect = false;
			gridRoutes.Name = "gridRoutes";
			gridRoutes.ReadOnly = true;
			gridRoutes.RowHeadersVisible = false;
			gridRoutes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			gridRoutes.Size = new Size(884, 232);
			gridRoutes.TabIndex = 1;
			//
			// statusColumn
			//
			statusColumn.HeaderText = "STATUS";
			statusColumn.Name = "statusColumn";
			statusColumn.ReadOnly = true;
			statusColumn.Width = 90;
			//
			// destinationColumn
			//
			destinationColumn.HeaderText = "DESTINATION";
			destinationColumn.Name = "destinationColumn";
			destinationColumn.ReadOnly = true;
			destinationColumn.Width = 190;
			//
			// eventsColumn
			//
			eventsColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			eventsColumn.HeaderText = "MESSAGES SENT";
			eventsColumn.Name = "eventsColumn";
			eventsColumn.ReadOnly = true;
			//
			// webhookColumn
			//
			webhookColumn.HeaderText = "WEBHOOK";
			webhookColumn.Name = "webhookColumn";
			webhookColumn.ReadOnly = true;
			webhookColumn.Width = 190;
			//
			// lblCount
			//
			lblCount.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			lblCount.ForeColor = Color.FromArgb(125, 165, 213);
			lblCount.Location = new Point(28, 492);
			lblCount.Name = "lblCount";
			lblCount.Size = new Size(700, 32);
			lblCount.TextAlign = ContentAlignment.MiddleLeft;
			//
			// btnClose
			//
			btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			btnClose.Location = new Point(782, 492);
			btnClose.Name = "btnClose";
			btnClose.Size = new Size(130, 42);
			btnClose.TabIndex = 2;
			btnClose.Text = "Close";
			btnClose.Click += btnClose_Click;
			//
			// DiscordRoutingInfoDialog
			//
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			BackColor = Color.FromArgb(38, 52, 77);
			ClientSize = new Size(940, 560);
			Controls.Add(titleBar);
			Controls.Add(lblHeading);
			Controls.Add(lblDescription);
			Controls.Add(informationCard);
			Controls.Add(gridRoutes);
			Controls.Add(lblCount);
			Controls.Add(btnClose);
			Font = new Font("Segoe UI", 9F);
			ForeColor = Color.FromArgb(245, 247, 251);
			FormBorderStyle = FormBorderStyle.None;
			KeyPreview = true;
			MinimumSize = new Size(760, 480);
			Name = "DiscordRoutingInfoDialog";
			Padding = new Padding(1);
			StartPosition = FormStartPosition.CenterParent;
			Text = "Discord Webhooks";
			titleBar.ResumeLayout(false);
			titleBar.PerformLayout();
			((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
			informationCard.ResumeLayout(false);
			informationCard.PerformLayout();
			((System.ComponentModel.ISupportInitialize)gridRoutes).EndInit();
			ResumeLayout(false);
		}
	}
}
