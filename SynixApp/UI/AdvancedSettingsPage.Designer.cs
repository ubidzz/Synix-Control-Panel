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

namespace Synix_Control_Panel.SynixEngine
{
	partial class AdvancedSettingsPage
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Component Designer generated code

		private void InitializeComponent()
		{
			settingsCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			cardLayout = new TableLayoutPanel();
			settingGlyph = new Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph();
			textLayout = new TableLayoutPanel();
			lblTitle = new Label();
			lblDescription = new Label();
			chkElevatedTasks = new Synix_Control_Panel.SynixApp.Design.ModernSettingsToggle();
			troubleshooterCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			troubleshooterGlyph = new Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph();
			lblTroubleshooterTitle = new Label();
			lblTroubleshooterDescription = new Label();
			btnTroubleshooter = new Synix_Control_Panel.SynixApp.Design.ModernSettingsButton();
			settingsCard.SuspendLayout();
			cardLayout.SuspendLayout();
			textLayout.SuspendLayout();
			troubleshooterCard.SuspendLayout();
			SuspendLayout();
			//
			// settingsCard
			//
			settingsCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			settingsCard.BackColor = Color.FromArgb(17, 27, 45);
			settingsCard.BorderColor = Color.FromArgb(38, 52, 77);
			settingsCard.Controls.Add(cardLayout);
			settingsCard.CornerRadius = 13;
			settingsCard.FillColor = Color.FromArgb(17, 27, 45);
			settingsCard.Location = new Point(0, 0);
			settingsCard.Margin = new Padding(0);
			settingsCard.Name = "settingsCard";
			settingsCard.Size = new Size(818, 126);
			settingsCard.TabIndex = 0;
			//
			// cardLayout
			//
			cardLayout.BackColor = Color.FromArgb(17, 27, 45);
			cardLayout.ColumnCount = 3;
			cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58F));
			cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
			cardLayout.Controls.Add(settingGlyph, 0, 0);
			cardLayout.Controls.Add(textLayout, 1, 0);
			cardLayout.Controls.Add(chkElevatedTasks, 2, 0);
			cardLayout.Dock = DockStyle.Fill;
			cardLayout.Location = new Point(0, 0);
			cardLayout.Margin = new Padding(0);
			cardLayout.Name = "cardLayout";
			cardLayout.Padding = new Padding(22, 18, 20, 18);
			cardLayout.RowCount = 1;
			cardLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			cardLayout.Size = new Size(818, 126);
			cardLayout.TabIndex = 0;
			//
			// settingGlyph
			//
			settingGlyph.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			settingGlyph.BackColor = Color.FromArgb(17, 27, 45);
			settingGlyph.Font = new Font("Segoe UI Symbol", 15F, FontStyle.Regular, GraphicsUnit.Point);
			settingGlyph.ForeColor = Color.FromArgb(32, 214, 199);
			settingGlyph.Glyph = "⚡";
			settingGlyph.Location = new Point(22, 22);
			settingGlyph.Margin = new Padding(0, 4, 12, 0);
			settingGlyph.Name = "settingGlyph";
			settingGlyph.Size = new Size(42, 42);
			settingGlyph.TabIndex = 0;
			//
			// textLayout
			//
			textLayout.BackColor = Color.FromArgb(17, 27, 45);
			textLayout.ColumnCount = 1;
			textLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			textLayout.Controls.Add(lblTitle, 0, 0);
			textLayout.Controls.Add(lblDescription, 0, 1);
			textLayout.Dock = DockStyle.Fill;
			textLayout.Location = new Point(80, 18);
			textLayout.Margin = new Padding(0);
			textLayout.Name = "textLayout";
			textLayout.RowCount = 2;
			textLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
			textLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			textLayout.Size = new Size(648, 90);
			textLayout.TabIndex = 1;
			//
			// lblTitle
			//
			lblTitle.AutoEllipsis = true;
			lblTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblTitle.Dock = DockStyle.Fill;
			lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
			lblTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblTitle.Location = new Point(0, 0);
			lblTitle.Margin = new Padding(0);
			lblTitle.Name = "lblTitle";
			lblTitle.Size = new Size(648, 31);
			lblTitle.TabIndex = 0;
			lblTitle.Text = "Elevated System Tasks";
			lblTitle.TextAlign = ContentAlignment.MiddleLeft;
			//
			// lblDescription
			//
			lblDescription.AutoEllipsis = true;
			lblDescription.BackColor = Color.FromArgb(17, 27, 45);
			lblDescription.Dock = DockStyle.Fill;
			lblDescription.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
			lblDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblDescription.Location = new Point(0, 31);
			lblDescription.Margin = new Padding(0);
			lblDescription.Name = "lblDescription";
			lblDescription.Size = new Size(648, 59);
			lblDescription.TabIndex = 1;
			lblDescription.Text = "When enabled, deleting a server requests administrator permission to remove its Windows Firewall rules. Turn this off to skip automatic cleanup during deletion.";
			//
			// chkElevatedTasks
			//
			chkElevatedTasks.AccessibleName = "Elevated system tasks";
			chkElevatedTasks.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			chkElevatedTasks.BackColor = Color.FromArgb(17, 27, 45);
			chkElevatedTasks.Cursor = Cursors.Hand;
			chkElevatedTasks.Location = new Point(739, 26);
			chkElevatedTasks.Margin = new Padding(0, 8, 0, 0);
			chkElevatedTasks.Name = "chkElevatedTasks";
			chkElevatedTasks.Size = new Size(54, 30);
			chkElevatedTasks.TabIndex = 2;
			chkElevatedTasks.UseVisualStyleBackColor = false;
			//
			// troubleshooterCard
			//
			troubleshooterCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			troubleshooterCard.BackColor = Color.FromArgb(17, 27, 45);
			troubleshooterCard.BorderColor = Color.FromArgb(38, 52, 77);
			troubleshooterCard.Controls.Add(troubleshooterGlyph);
			troubleshooterCard.Controls.Add(lblTroubleshooterTitle);
			troubleshooterCard.Controls.Add(lblTroubleshooterDescription);
			troubleshooterCard.Controls.Add(btnTroubleshooter);
			troubleshooterCard.CornerRadius = 13;
			troubleshooterCard.FillColor = Color.FromArgb(17, 27, 45);
			troubleshooterCard.Location = new Point(0, 146);
			troubleshooterCard.Name = "troubleshooterCard";
			troubleshooterCard.Size = new Size(818, 148);
			troubleshooterCard.TabIndex = 1;
			//
			// troubleshooterGlyph
			//
			troubleshooterGlyph.BackColor = Color.FromArgb(17, 27, 45);
			troubleshooterGlyph.Font = new Font("Segoe UI Symbol", 15F);
			troubleshooterGlyph.ForeColor = Color.FromArgb(32, 214, 199);
			troubleshooterGlyph.Glyph = "✓";
			troubleshooterGlyph.Location = new Point(22, 24);
			troubleshooterGlyph.Size = new Size(42, 42);
			troubleshooterGlyph.TabIndex = 0;
			//
			// lblTroubleshooterTitle
			//
			lblTroubleshooterTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblTroubleshooterTitle.AutoEllipsis = true;
			lblTroubleshooterTitle.BackColor = Color.FromArgb(17, 27, 45);
			lblTroubleshooterTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblTroubleshooterTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblTroubleshooterTitle.Location = new Point(80, 22);
			lblTroubleshooterTitle.Size = new Size(520, 31);
			lblTroubleshooterTitle.Text = "System & Server Troubleshooter";
			//
			// lblTroubleshooterDescription
			//
			lblTroubleshooterDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblTroubleshooterDescription.BackColor = Color.FromArgb(17, 27, 45);
			lblTroubleshooterDescription.Font = new Font("Segoe UI", 9.5F);
			lblTroubleshooterDescription.ForeColor = Color.FromArgb(158, 172, 194);
			lblTroubleshooterDescription.Location = new Point(80, 54);
			lblTroubleshooterDescription.Size = new Size(520, 72);
			lblTroubleshooterDescription.Text = "Check SteamCMD, runtimes, server files, configs, ports, Windows Firewall, disk space, interrupted processes, recent logs, and update health.";
			//
			// btnTroubleshooter
			//
			btnTroubleshooter.AccessibleName = "Open the Synix troubleshooter";
			btnTroubleshooter.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnTroubleshooter.Location = new Point(628, 52);
			btnTroubleshooter.Name = "btnTroubleshooter";
			btnTroubleshooter.Size = new Size(165, 42);
			btnTroubleshooter.Text = "Run Health Check";
			btnTroubleshooter.UseAccentStyle = true;
			//
			// AdvancedSettingsPage
			//
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			BackColor = Color.FromArgb(8, 13, 24);
			Controls.Add(troubleshooterCard);
			Controls.Add(settingsCard);
			Name = "AdvancedSettingsPage";
			Size = new Size(818, 520);
			settingsCard.ResumeLayout(false);
			cardLayout.ResumeLayout(false);
			textLayout.ResumeLayout(false);
			troubleshooterCard.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private Synix_Control_Panel.SynixApp.Design.ModernSettingsCard settingsCard;
		private TableLayoutPanel cardLayout;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph settingGlyph;
		private TableLayoutPanel textLayout;
		private Label lblTitle;
		private Label lblDescription;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsToggle chkElevatedTasks;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsCard troubleshooterCard;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph troubleshooterGlyph;
		private Label lblTroubleshooterTitle;
		private Label lblTroubleshooterDescription;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsButton btnTroubleshooter;
	}
}
