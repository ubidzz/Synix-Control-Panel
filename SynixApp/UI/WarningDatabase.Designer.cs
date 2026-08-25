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
namespace Synix_Control_Panel.Database
{
	partial class WarningDatabase
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WarningDatabase));
			shellLayout = new TableLayoutPanel();
			titleBar = new Panel();
			picLogo = new PictureBox();
			lblWindowTitle = new Label();
			btnWindowClose = new Button();
			titleBottomBorder = new Label();
			bodyLayout = new TableLayoutPanel();
			headerPanel = new Panel();
			headerGlyph = new Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph();
			lblWarningTitle = new Label();
			lblWarningSubtitle = new Label();
			lblGameName = new Label();
			warningCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			lblInstructionHeading = new Label();
			txtWarningText = new RichTextBox();
			actionPanel = new Panel();
			lblActionHint = new Label();
			btnNo = new Synix_Control_Panel.SynixApp.Design.ModernSettingsButton();
			btnStart = new Synix_Control_Panel.SynixApp.Design.ModernSettingsButton();
			shellLayout.SuspendLayout();
			titleBar.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
			bodyLayout.SuspendLayout();
			headerPanel.SuspendLayout();
			warningCard.SuspendLayout();
			actionPanel.SuspendLayout();
			SuspendLayout();
			shellLayout.BackColor = Color.FromArgb(8, 13, 24);
			shellLayout.ColumnCount = 1;
			shellLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			shellLayout.Controls.Add(titleBar, 0, 0);
			shellLayout.Controls.Add(bodyLayout, 0, 1);
			shellLayout.Dock = DockStyle.Fill;
			shellLayout.Location = new Point(1, 1);
			shellLayout.Margin = new Padding(0);
			shellLayout.Name = "shellLayout";
			shellLayout.RowCount = 2;
			shellLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
			shellLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			shellLayout.Size = new Size(818, 618);
			shellLayout.TabIndex = 0;
			titleBar.BackColor = Color.FromArgb(6, 12, 22);
			titleBar.Controls.Add(picLogo);
			titleBar.Controls.Add(lblWindowTitle);
			titleBar.Controls.Add(btnWindowClose);
			titleBar.Controls.Add(titleBottomBorder);
			titleBar.Dock = DockStyle.Fill;
			titleBar.Location = new Point(0, 0);
			titleBar.Margin = new Padding(0);
			titleBar.Name = "titleBar";
			titleBar.Size = new Size(818, 56);
			titleBar.TabIndex = 0;
			titleBar.MouseDown += TitleBar_MouseDown;
			picLogo.BackColor = Color.FromArgb(6, 12, 22);
			picLogo.Image = global::Synix_Control_Panel.Properties.Resources.synix_logo;
			picLogo.Location = new Point(18, 13);
			picLogo.Name = "picLogo";
			picLogo.Size = new Size(30, 30);
			picLogo.SizeMode = PictureBoxSizeMode.Zoom;
			picLogo.TabIndex = 0;
			picLogo.TabStop = false;
			picLogo.MouseDown += TitleBar_MouseDown;
			lblWindowTitle.AutoSize = true;
			lblWindowTitle.BackColor = Color.FromArgb(6, 12, 22);
			lblWindowTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblWindowTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblWindowTitle.Location = new Point(58, 17);
			lblWindowTitle.Name = "lblWindowTitle";
			lblWindowTitle.Size = new Size(154, 21);
			lblWindowTitle.TabIndex = 1;
			lblWindowTitle.Text = "Launch preparation";
			lblWindowTitle.MouseDown += TitleBar_MouseDown;
			btnWindowClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnWindowClose.BackColor = Color.FromArgb(6, 12, 22);
			btnWindowClose.Cursor = Cursors.Hand;
			btnWindowClose.FlatAppearance.BorderSize = 0;
			btnWindowClose.FlatAppearance.MouseDownBackColor = Color.FromArgb(175, 35, 50);
			btnWindowClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(205, 48, 64);
			btnWindowClose.FlatStyle = FlatStyle.Flat;
			btnWindowClose.Font = new Font("Segoe UI", 15F);
			btnWindowClose.ForeColor = Color.FromArgb(245, 247, 251);
			btnWindowClose.Location = new Point(770, 0);
			btnWindowClose.Name = "btnWindowClose";
			btnWindowClose.Size = new Size(48, 55);
			btnWindowClose.TabIndex = 2;
			btnWindowClose.TabStop = false;
			btnWindowClose.Text = "×";
			btnWindowClose.UseVisualStyleBackColor = false;
			btnWindowClose.Click += btnNo_Click;
			titleBottomBorder.BackColor = Color.FromArgb(38, 52, 77);
			titleBottomBorder.Dock = DockStyle.Bottom;
			titleBottomBorder.Location = new Point(0, 55);
			titleBottomBorder.Name = "titleBottomBorder";
			titleBottomBorder.Size = new Size(818, 1);
			titleBottomBorder.TabIndex = 3;
			bodyLayout.BackColor = Color.FromArgb(8, 13, 24);
			bodyLayout.ColumnCount = 1;
			bodyLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			bodyLayout.Controls.Add(headerPanel, 0, 0);
			bodyLayout.Controls.Add(warningCard, 0, 1);
			bodyLayout.Controls.Add(actionPanel, 0, 2);
			bodyLayout.Dock = DockStyle.Fill;
			bodyLayout.Location = new Point(0, 56);
			bodyLayout.Margin = new Padding(0);
			bodyLayout.Name = "bodyLayout";
			bodyLayout.Padding = new Padding(28, 22, 28, 18);
			bodyLayout.RowCount = 3;
			bodyLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
			bodyLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			bodyLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 74F));
			bodyLayout.Size = new Size(818, 562);
			bodyLayout.TabIndex = 1;
			headerPanel.Controls.Add(headerGlyph);
			headerPanel.Controls.Add(lblWarningTitle);
			headerPanel.Controls.Add(lblWarningSubtitle);
			headerPanel.Controls.Add(lblGameName);
			headerPanel.Dock = DockStyle.Fill;
			headerPanel.Location = new Point(28, 22);
			headerPanel.Margin = new Padding(0);
			headerPanel.Name = "headerPanel";
			headerPanel.Size = new Size(762, 92);
			headerPanel.TabIndex = 0;
			headerGlyph.ForeColor = Color.FromArgb(245, 185, 76);
			headerGlyph.Glyph = "!";
			headerGlyph.Location = new Point(0, 7);
			headerGlyph.Name = "headerGlyph";
			headerGlyph.Size = new Size(48, 48);
			headerGlyph.TabIndex = 0;
			lblWarningTitle.AutoSize = true;
			lblWarningTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
			lblWarningTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblWarningTitle.Location = new Point(64, 0);
			lblWarningTitle.Name = "lblWarningTitle";
			lblWarningTitle.Size = new Size(258, 32);
			lblWarningTitle.TabIndex = 1;
			lblWarningTitle.Text = "First-launch preparation";
			lblWarningSubtitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblWarningSubtitle.Font = new Font("Segoe UI", 9.5F);
			lblWarningSubtitle.ForeColor = Color.FromArgb(158, 172, 194);
			lblWarningSubtitle.Location = new Point(67, 38);
			lblWarningSubtitle.Name = "lblWarningSubtitle";
			lblWarningSubtitle.Size = new Size(518, 42);
			lblWarningSubtitle.TabIndex = 2;
			lblWarningSubtitle.Text = "Review these setup requirements before continuing.";
			lblGameName.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lblGameName.AutoEllipsis = true;
			lblGameName.BackColor = Color.FromArgb(28, 75, 91);
			lblGameName.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
			lblGameName.ForeColor = Color.FromArgb(32, 214, 199);
			lblGameName.Location = new Point(598, 8);
			lblGameName.Name = "lblGameName";
			lblGameName.Padding = new Padding(8, 0, 8, 0);
			lblGameName.Size = new Size(164, 30);
			lblGameName.TabIndex = 3;
			lblGameName.Text = "Server";
			lblGameName.TextAlign = ContentAlignment.MiddleCenter;
			warningCard.BorderColor = Color.FromArgb(38, 52, 77);
			warningCard.Controls.Add(lblInstructionHeading);
			warningCard.Controls.Add(txtWarningText);
			warningCard.CornerRadius = 12;
			warningCard.Dock = DockStyle.Fill;
			warningCard.FillColor = Color.FromArgb(17, 27, 45);
			warningCard.Location = new Point(28, 114);
			warningCard.Margin = new Padding(0, 0, 0, 12);
			warningCard.Name = "warningCard";
			warningCard.Size = new Size(762, 344);
			warningCard.TabIndex = 1;
			lblInstructionHeading.AutoSize = true;
			lblInstructionHeading.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
			lblInstructionHeading.ForeColor = Color.FromArgb(245, 247, 251);
			lblInstructionHeading.Location = new Point(22, 18);
			lblInstructionHeading.Name = "lblInstructionHeading";
			lblInstructionHeading.Size = new Size(155, 20);
			lblInstructionHeading.TabIndex = 0;
			lblInstructionHeading.Text = "Before you continue";
			txtWarningText.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			txtWarningText.BackColor = Color.FromArgb(15, 15, 15);
			txtWarningText.BorderStyle = BorderStyle.None;
			txtWarningText.DetectUrls = true;
			txtWarningText.Font = new Font("Segoe UI", 10F);
			txtWarningText.ForeColor = Color.FromArgb(245, 247, 251);
			txtWarningText.Location = new Point(22, 52);
			txtWarningText.Name = "txtWarningText";
			txtWarningText.ReadOnly = true;
			txtWarningText.ScrollBars = RichTextBoxScrollBars.Vertical;
			txtWarningText.Size = new Size(718, 270);
			txtWarningText.TabIndex = 1;
			txtWarningText.Text = "";
			txtWarningText.LinkClicked += TxtWarningText_LinkClicked;
			actionPanel.Controls.Add(lblActionHint);
			actionPanel.Controls.Add(btnNo);
			actionPanel.Controls.Add(btnStart);
			actionPanel.Dock = DockStyle.Fill;
			actionPanel.Location = new Point(28, 470);
			actionPanel.Margin = new Padding(0);
			actionPanel.Name = "actionPanel";
			actionPanel.Size = new Size(762, 74);
			actionPanel.TabIndex = 2;
			lblActionHint.Anchor = AnchorStyles.Left;
			lblActionHint.AutoEllipsis = true;
			lblActionHint.Font = new Font("Segoe UI", 8.75F);
			lblActionHint.ForeColor = Color.FromArgb(105, 124, 153);
			lblActionHint.Location = new Point(0, 18);
			lblActionHint.Name = "lblActionHint";
			lblActionHint.Size = new Size(400, 34);
			lblActionHint.TabIndex = 0;
			lblActionHint.Text = "Continue only after you have reviewed the required setup steps.";
			lblActionHint.TextAlign = ContentAlignment.MiddleLeft;
			btnNo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnNo.DialogResult = DialogResult.Cancel;
			btnNo.Location = new Point(420, 10);
			btnNo.Name = "btnNo";
			btnNo.Size = new Size(158, 46);
			btnNo.TabIndex = 1;
			btnNo.Text = "Remind Me Later";
			btnNo.UseVisualStyleBackColor = true;
			btnNo.Click += btnNo_Click;
			btnStart.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnStart.Location = new Point(592, 10);
			btnStart.Name = "btnStart";
			btnStart.Size = new Size(170, 46);
			btnStart.TabIndex = 2;
			btnStart.Text = "Start Server";
			btnStart.UseAccentStyle = true;
			btnStart.UseVisualStyleBackColor = true;
			btnStart.Click += btnStart_Click;
			AcceptButton = btnStart;
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			BackColor = Color.FromArgb(38, 52, 77);
			CancelButton = btnNo;
			ClientSize = new Size(820, 620);
			Controls.Add(shellLayout);
			Font = new Font("Segoe UI", 9F);
			ForeColor = Color.FromArgb(245, 247, 251);
			FormBorderStyle = FormBorderStyle.None;
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MinimizeBox = false;
			MinimumSize = new Size(700, 540);
			Name = "WarningDatabase";
			Padding = new Padding(1);
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Launch Preparation";
			shellLayout.ResumeLayout(false);
			titleBar.ResumeLayout(false);
			titleBar.PerformLayout();
			((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
			bodyLayout.ResumeLayout(false);
			headerPanel.ResumeLayout(false);
			headerPanel.PerformLayout();
			warningCard.ResumeLayout(false);
			warningCard.PerformLayout();
			actionPanel.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private TableLayoutPanel shellLayout;
		private Panel titleBar;
		private PictureBox picLogo;
		private Label lblWindowTitle;
		private Button btnWindowClose;
		private Label titleBottomBorder;
		private TableLayoutPanel bodyLayout;
		private Panel headerPanel;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph headerGlyph;
		private Label lblWarningTitle;
		private Label lblWarningSubtitle;
		private Label lblGameName;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsCard warningCard;
		private Label lblInstructionHeading;
		private RichTextBox txtWarningText;
		private Panel actionPanel;
		private Label lblActionHint;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsButton btnNo;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsButton btnStart;
	}
}
