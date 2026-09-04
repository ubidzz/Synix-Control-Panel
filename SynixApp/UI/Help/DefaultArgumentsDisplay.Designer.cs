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
namespace Synix_Control_Panel.SynixApp.UI.Help
{
	partial class DefaultArgumentsDisplay
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DefaultArgumentsDisplay));
			shellLayout = new TableLayoutPanel();
			titleBar = new Panel();
			picLogo = new PictureBox();
			lblWindowTitle = new Label();
			btnWindowClose = new Button();
			titleBottomBorder = new Label();
			bodyLayout = new TableLayoutPanel();
			headerPanel = new Panel();
			headerGlyph = new Synix_Control_Panel.SynixApp.Design.ModernSettingsGlyph();
			lblHeading = new Label();
			lblSubtitle = new Label();
			argumentsCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			lblArgumentsTitle = new Label();
			lblArgumentsHelp = new Label();
			txtArgs = new TextBox();
			actionPanel = new Panel();
			lblTransparencyNote = new Label();
			btnClose = new Synix_Control_Panel.SynixApp.Design.ModernSettingsButton();
			shellLayout.SuspendLayout();
			titleBar.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
			bodyLayout.SuspendLayout();
			headerPanel.SuspendLayout();
			argumentsCard.SuspendLayout();
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
			shellLayout.Size = new Size(758, 478);
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
			titleBar.Size = new Size(758, 56);
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
			lblWindowTitle.Size = new Size(207, 21);
			lblWindowTitle.TabIndex = 1;
			lblWindowTitle.Text = "Default launch arguments";
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
			btnWindowClose.Location = new Point(710, 0);
			btnWindowClose.Name = "btnWindowClose";
			btnWindowClose.Size = new Size(48, 55);
			btnWindowClose.TabIndex = 2;
			btnWindowClose.TabStop = false;
			btnWindowClose.Text = "×";
			btnWindowClose.UseVisualStyleBackColor = false;
			btnWindowClose.Click += btnClose_Click;
			titleBottomBorder.BackColor = Color.FromArgb(38, 52, 77);
			titleBottomBorder.Dock = DockStyle.Bottom;
			titleBottomBorder.Location = new Point(0, 55);
			titleBottomBorder.Name = "titleBottomBorder";
			titleBottomBorder.Size = new Size(758, 1);
			titleBottomBorder.TabIndex = 3;
			bodyLayout.BackColor = Color.FromArgb(8, 13, 24);
			bodyLayout.ColumnCount = 1;
			bodyLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			bodyLayout.Controls.Add(headerPanel, 0, 0);
			bodyLayout.Controls.Add(argumentsCard, 0, 1);
			bodyLayout.Controls.Add(actionPanel, 0, 2);
			bodyLayout.Dock = DockStyle.Fill;
			bodyLayout.Location = new Point(0, 56);
			bodyLayout.Margin = new Padding(0);
			bodyLayout.Name = "bodyLayout";
			bodyLayout.Padding = new Padding(28, 22, 28, 18);
			bodyLayout.RowCount = 3;
			bodyLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82F));
			bodyLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			bodyLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
			bodyLayout.Size = new Size(758, 422);
			bodyLayout.TabIndex = 1;
			headerPanel.Controls.Add(headerGlyph);
			headerPanel.Controls.Add(lblHeading);
			headerPanel.Controls.Add(lblSubtitle);
			headerPanel.Dock = DockStyle.Fill;
			headerPanel.Location = new Point(28, 22);
			headerPanel.Margin = new Padding(0);
			headerPanel.Name = "headerPanel";
			headerPanel.Size = new Size(702, 82);
			headerPanel.TabIndex = 0;
			headerGlyph.Glyph = ">_";
			headerGlyph.Location = new Point(0, 5);
			headerGlyph.Name = "headerGlyph";
			headerGlyph.Size = new Size(48, 48);
			headerGlyph.TabIndex = 0;
			lblHeading.AutoSize = true;
			lblHeading.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
			lblHeading.ForeColor = Color.FromArgb(245, 247, 251);
			lblHeading.Location = new Point(64, 0);
			lblHeading.Name = "lblHeading";
			lblHeading.Size = new Size(274, 32);
			lblHeading.TabIndex = 1;
			lblHeading.Text = "Default startup arguments";
			lblSubtitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblSubtitle.Font = new Font("Segoe UI", 9.5F);
			lblSubtitle.ForeColor = Color.FromArgb(158, 172, 194);
			lblSubtitle.Location = new Point(67, 36);
			lblSubtitle.Name = "lblSubtitle";
			lblSubtitle.Size = new Size(620, 38);
			lblSubtitle.TabIndex = 2;
			lblSubtitle.Text = "Review how Synix builds the command used to start this server.";
			argumentsCard.BorderColor = Color.FromArgb(38, 52, 77);
			argumentsCard.Controls.Add(lblArgumentsTitle);
			argumentsCard.Controls.Add(lblArgumentsHelp);
			argumentsCard.Controls.Add(txtArgs);
			argumentsCard.CornerRadius = 12;
			argumentsCard.Dock = DockStyle.Fill;
			argumentsCard.FillColor = Color.FromArgb(17, 27, 45);
			argumentsCard.Location = new Point(28, 104);
			argumentsCard.Margin = new Padding(0, 0, 0, 12);
			argumentsCard.Name = "argumentsCard";
			argumentsCard.Size = new Size(702, 232);
			argumentsCard.TabIndex = 1;
			lblArgumentsTitle.AutoSize = true;
			lblArgumentsTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
			lblArgumentsTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblArgumentsTitle.Location = new Point(22, 18);
			lblArgumentsTitle.Name = "lblArgumentsTitle";
			lblArgumentsTitle.Size = new Size(191, 20);
			lblArgumentsTitle.TabIndex = 0;
			lblArgumentsTitle.Text = "Startup argument template";
			lblArgumentsHelp.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblArgumentsHelp.Font = new Font("Segoe UI", 8.75F);
			lblArgumentsHelp.ForeColor = Color.FromArgb(105, 124, 153);
			lblArgumentsHelp.Location = new Point(22, 42);
			lblArgumentsHelp.Name = "lblArgumentsHelp";
			lblArgumentsHelp.Size = new Size(658, 22);
			lblArgumentsHelp.TabIndex = 1;
			lblArgumentsHelp.Text = "This informational view shows the default arguments Synix uses when building the start command.";
			txtArgs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			txtArgs.BackColor = Color.FromArgb(12, 21, 36);
			txtArgs.BorderStyle = BorderStyle.FixedSingle;
			txtArgs.Font = new Font("Consolas", 10F);
			txtArgs.ForeColor = Color.FromArgb(245, 247, 251);
			txtArgs.Location = new Point(22, 68);
			txtArgs.Multiline = true;
			txtArgs.Name = "txtArgs";
			txtArgs.ReadOnly = true;
			txtArgs.ScrollBars = ScrollBars.Vertical;
			txtArgs.Size = new Size(658, 142);
			txtArgs.TabIndex = 2;
			actionPanel.Controls.Add(lblTransparencyNote);
			actionPanel.Controls.Add(btnClose);
			actionPanel.Dock = DockStyle.Fill;
			actionPanel.Location = new Point(28, 348);
			actionPanel.Margin = new Padding(0);
			actionPanel.Name = "actionPanel";
			actionPanel.Size = new Size(702, 64);
			actionPanel.TabIndex = 2;
			lblTransparencyNote.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			lblTransparencyNote.AutoEllipsis = true;
			lblTransparencyNote.Font = new Font("Segoe UI", 8.75F);
			lblTransparencyNote.ForeColor = Color.FromArgb(105, 124, 153);
			lblTransparencyNote.Location = new Point(0, 12);
			lblTransparencyNote.Name = "lblTransparencyNote";
			lblTransparencyNote.Size = new Size(550, 34);
			lblTransparencyNote.TabIndex = 0;
			lblTransparencyNote.Text = "Shown for transparency so you can verify the startup command has no hidden arguments.";
			lblTransparencyNote.TextAlign = ContentAlignment.MiddleLeft;
			btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnClose.Location = new Point(574, 8);
			btnClose.Name = "btnClose";
			btnClose.Size = new Size(128, 44);
			btnClose.TabIndex = 1;
			btnClose.Text = "Close";
			btnClose.UseAccentStyle = true;
			btnClose.UseVisualStyleBackColor = true;
			btnClose.Click += btnClose_Click;
			AcceptButton = btnClose;
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			BackColor = Color.FromArgb(38, 52, 77);
			ClientSize = new Size(760, 480);
			Controls.Add(shellLayout);
			Font = new Font("Segoe UI", 9F);
			ForeColor = Color.FromArgb(245, 247, 251);
			FormBorderStyle = FormBorderStyle.None;
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MinimizeBox = false;
			MinimumSize = new Size(640, 420);
			Name = "DefaultArgumentsDisplay";
			Padding = new Padding(1);
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Default Launch Arguments";
			shellLayout.ResumeLayout(false);
			titleBar.ResumeLayout(false);
			titleBar.PerformLayout();
			((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
			bodyLayout.ResumeLayout(false);
			headerPanel.ResumeLayout(false);
			headerPanel.PerformLayout();
			argumentsCard.ResumeLayout(false);
			argumentsCard.PerformLayout();
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
		private Label lblHeading;
		private Label lblSubtitle;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsCard argumentsCard;
		private Label lblArgumentsTitle;
		private Label lblArgumentsHelp;
		private TextBox txtArgs;
		private Panel actionPanel;
		private Label lblTransparencyNote;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsButton btnClose;
	}
}
