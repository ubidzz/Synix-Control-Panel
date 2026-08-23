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
namespace Synix_Control_Panel.ServerHandler
{
	partial class ServerConfig
	{
		private System.ComponentModel.IContainer? components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerConfig));
			shellLayout = new TableLayoutPanel();
			titleBar = new Panel();
			picLogo = new PictureBox();
			lblWindowTitle = new Label();
			lblFileName = new Label();
			lblFormatBadge = new Label();
			btnMinimize = new Button();
			btnClose = new Button();
			titleBottomBorder = new Label();
			contentHost = new Panel();
			mainLayout = new TableLayoutPanel();
			headerPanel = new Panel();
			lblPageTitle = new Label();
			lblPageSubtitle = new Label();
			lblSafeBadge = new Label();
			toolbarLayout = new TableLayoutPanel();
			pnlSearch = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			lblSearchGlyph = new Label();
			txtSearch = new TextBox();
			cmbTypeFilter = new Synix_Control_Panel.SynixApp.Design.ModernSettingsComboBox();
			btnValidateConfig = new Synix_Control_Panel.SynixApp.Design.ModernSettingsButton();
			btnStructured = new Synix_Control_Panel.SynixApp.Design.ModernSettingsButton();
			btnRawPreview = new Synix_Control_Panel.SynixApp.Design.ModernSettingsButton();
			pnlPreservationBanner = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			lblShieldGlyph = new Label();
			lblPreservationTitle = new Label();
			lblPreservationText = new Label();
			pnlGridCard = new Synix_Control_Panel.SynixApp.Design.ModernSettingsCard();
			dgvConfig = new DataGridView();
			colSetting = new DataGridViewTextBoxColumn();
			colType = new DataGridViewTextBoxColumn();
			colValue = new DataGridViewTextBoxColumn();
			txtRawPreview = new RichTextBox();
			footerPanel = new Panel();
			footerTopBorder = new Label();
			lblSettingCount = new Label();
			lblModifiedCount = new Label();
			lblStatusGlyph = new Label();
			lblFormatState = new Label();
			btnFixConfig = new Synix_Control_Panel.SynixApp.Design.ModernSettingsButton();
			btnRestoreBackup = new Synix_Control_Panel.SynixApp.Design.ModernSettingsButton();
			btnReset = new Synix_Control_Panel.SynixApp.Design.ModernSettingsButton();
			btnCancel = new Synix_Control_Panel.SynixApp.Design.ModernSettingsButton();
			btnSave = new Synix_Control_Panel.SynixApp.Design.ModernSettingsButton();
			shellLayout.SuspendLayout();
			titleBar.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
			contentHost.SuspendLayout();
			mainLayout.SuspendLayout();
			headerPanel.SuspendLayout();
			toolbarLayout.SuspendLayout();
			pnlSearch.SuspendLayout();
			pnlPreservationBanner.SuspendLayout();
			pnlGridCard.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)dgvConfig).BeginInit();
			footerPanel.SuspendLayout();
			SuspendLayout();
			//
			// shellLayout
			//
			shellLayout.BackColor = Color.FromArgb(8, 13, 24);
			shellLayout.ColumnCount = 1;
			shellLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			shellLayout.Controls.Add(titleBar, 0, 0);
			shellLayout.Controls.Add(contentHost, 0, 1);
			shellLayout.Dock = DockStyle.Fill;
			shellLayout.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
			shellLayout.Location = new Point(1, 1);
			shellLayout.Margin = new Padding(0);
			shellLayout.Name = "shellLayout";
			shellLayout.RowCount = 2;
			shellLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
			shellLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			shellLayout.Size = new Size(1198, 778);
			shellLayout.TabIndex = 0;
			//
			// titleBar
			//
			titleBar.BackColor = Color.FromArgb(6, 12, 22);
			titleBar.Controls.Add(picLogo);
			titleBar.Controls.Add(lblWindowTitle);
			titleBar.Controls.Add(lblFileName);
			titleBar.Controls.Add(lblFormatBadge);
			titleBar.Controls.Add(btnMinimize);
			titleBar.Controls.Add(btnClose);
			titleBar.Controls.Add(titleBottomBorder);
			titleBar.Dock = DockStyle.Fill;
			titleBar.Location = new Point(0, 0);
			titleBar.Margin = new Padding(0);
			titleBar.Name = "titleBar";
			titleBar.Size = new Size(1198, 56);
			titleBar.TabIndex = 0;
			titleBar.MouseDown += TitleBar_MouseDown;
			//
			// picLogo
			//
			picLogo.BackColor = Color.FromArgb(6, 12, 22);
			picLogo.Image = Properties.Resources.synix_logo;
			picLogo.Location = new Point(18, 13);
			picLogo.Name = "picLogo";
			picLogo.Size = new Size(30, 30);
			picLogo.SizeMode = PictureBoxSizeMode.Zoom;
			picLogo.TabIndex = 0;
			picLogo.TabStop = false;
			picLogo.MouseDown += TitleBar_MouseDown;
			//
			// lblWindowTitle
			//
			lblWindowTitle.AutoSize = true;
			lblWindowTitle.BackColor = Color.FromArgb(6, 12, 22);
			lblWindowTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			lblWindowTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblWindowTitle.Location = new Point(58, 17);
			lblWindowTitle.Name = "lblWindowTitle";
			lblWindowTitle.Size = new Size(111, 21);
			lblWindowTitle.TabIndex = 1;
			lblWindowTitle.Text = "Config Editor";
			lblWindowTitle.MouseDown += TitleBar_MouseDown;
			//
			// lblFileName
			//
			lblFileName.AutoEllipsis = true;
			lblFileName.BackColor = Color.FromArgb(17, 27, 45);
			lblFileName.Font = new Font("Segoe UI", 9F);
			lblFileName.ForeColor = Color.FromArgb(158, 172, 194);
			lblFileName.Location = new Point(184, 14);
			lblFileName.Name = "lblFileName";
			lblFileName.Padding = new Padding(10, 5, 10, 5);
			lblFileName.Size = new Size(235, 30);
			lblFileName.TabIndex = 2;
			lblFileName.Text = "serverconfig.xml";
			lblFileName.TextAlign = ContentAlignment.MiddleCenter;
			lblFileName.MouseDown += TitleBar_MouseDown;
			//
			// lblFormatBadge
			//
			lblFormatBadge.BackColor = Color.FromArgb(22, 58, 70);
			lblFormatBadge.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
			lblFormatBadge.ForeColor = Color.FromArgb(32, 214, 199);
			lblFormatBadge.Location = new Point(429, 14);
			lblFormatBadge.Name = "lblFormatBadge";
			lblFormatBadge.Size = new Size(58, 30);
			lblFormatBadge.TabIndex = 3;
			lblFormatBadge.Text = "XML";
			lblFormatBadge.TextAlign = ContentAlignment.MiddleCenter;
			lblFormatBadge.MouseDown += TitleBar_MouseDown;
			//
			// btnMinimize
			//
			btnMinimize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnMinimize.BackColor = Color.FromArgb(6, 12, 22);
			btnMinimize.Cursor = Cursors.Hand;
			btnMinimize.FlatAppearance.BorderSize = 0;
			btnMinimize.FlatAppearance.MouseDownBackColor = Color.FromArgb(28, 42, 60);
			btnMinimize.FlatAppearance.MouseOverBackColor = Color.FromArgb(21, 34, 52);
			btnMinimize.FlatStyle = FlatStyle.Flat;
			btnMinimize.Font = new Font("Segoe UI", 12F);
			btnMinimize.ForeColor = Color.FromArgb(245, 247, 251);
			btnMinimize.Location = new Point(1102, 0);
			btnMinimize.Name = "btnMinimize";
			btnMinimize.Size = new Size(48, 55);
			btnMinimize.TabIndex = 4;
			btnMinimize.TabStop = false;
			btnMinimize.Text = "—";
			btnMinimize.UseVisualStyleBackColor = false;
			btnMinimize.Click += btnMinimize_Click;
			//
			// btnClose
			//
			btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnClose.BackColor = Color.FromArgb(6, 12, 22);
			btnClose.Cursor = Cursors.Hand;
			btnClose.FlatAppearance.BorderSize = 0;
			btnClose.FlatAppearance.MouseDownBackColor = Color.FromArgb(175, 35, 50);
			btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(205, 48, 64);
			btnClose.FlatStyle = FlatStyle.Flat;
			btnClose.Font = new Font("Segoe UI", 15F);
			btnClose.ForeColor = Color.FromArgb(245, 247, 251);
			btnClose.Location = new Point(1150, 0);
			btnClose.Name = "btnClose";
			btnClose.Size = new Size(48, 55);
			btnClose.TabIndex = 5;
			btnClose.TabStop = false;
			btnClose.Text = "×";
			btnClose.UseVisualStyleBackColor = false;
			btnClose.Click += btnClose_Click;
			//
			// titleBottomBorder
			//
			titleBottomBorder.BackColor = Color.FromArgb(38, 52, 77);
			titleBottomBorder.Dock = DockStyle.Bottom;
			titleBottomBorder.Location = new Point(0, 55);
			titleBottomBorder.Margin = new Padding(0);
			titleBottomBorder.Name = "titleBottomBorder";
			titleBottomBorder.Size = new Size(1198, 1);
			titleBottomBorder.TabIndex = 6;
			//
			// contentHost
			//
			contentHost.BackColor = Color.FromArgb(8, 13, 24);
			contentHost.Controls.Add(mainLayout);
			contentHost.Dock = DockStyle.Fill;
			contentHost.Location = new Point(0, 56);
			contentHost.Margin = new Padding(0);
			contentHost.Name = "contentHost";
			contentHost.Padding = new Padding(28, 22, 28, 18);
			contentHost.Size = new Size(1198, 722);
			contentHost.TabIndex = 1;
			//
			// mainLayout
			//
			mainLayout.BackColor = Color.FromArgb(8, 13, 24);
			mainLayout.ColumnCount = 1;
			mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			mainLayout.Controls.Add(headerPanel, 0, 0);
			mainLayout.Controls.Add(toolbarLayout, 0, 1);
			mainLayout.Controls.Add(pnlPreservationBanner, 0, 2);
			mainLayout.Controls.Add(pnlGridCard, 0, 3);
			mainLayout.Controls.Add(footerPanel, 0, 4);
			mainLayout.Dock = DockStyle.Fill;
			mainLayout.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
			mainLayout.Location = new Point(28, 22);
			mainLayout.Margin = new Padding(0);
			mainLayout.Name = "mainLayout";
			mainLayout.RowCount = 5;
			mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));
			mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
			mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
			mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
			mainLayout.Size = new Size(1142, 682);
			mainLayout.TabIndex = 0;
			//
			// headerPanel
			//
			headerPanel.BackColor = Color.FromArgb(8, 13, 24);
			headerPanel.Controls.Add(lblPageTitle);
			headerPanel.Controls.Add(lblPageSubtitle);
			headerPanel.Controls.Add(lblSafeBadge);
			headerPanel.Dock = DockStyle.Fill;
			headerPanel.Location = new Point(0, 0);
			headerPanel.Margin = new Padding(0);
			headerPanel.Name = "headerPanel";
			headerPanel.Size = new Size(1142, 76);
			headerPanel.TabIndex = 0;
			//
			// lblPageTitle
			//
			lblPageTitle.AutoSize = true;
			lblPageTitle.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
			lblPageTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblPageTitle.Location = new Point(0, 0);
			lblPageTitle.Name = "lblPageTitle";
			lblPageTitle.Size = new Size(310, 41);
			lblPageTitle.TabIndex = 0;
			lblPageTitle.Text = "Configuration Editor";
			//
			// lblPageSubtitle
			//
			lblPageSubtitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblPageSubtitle.AutoEllipsis = true;
			lblPageSubtitle.Font = new Font("Segoe UI", 10F);
			lblPageSubtitle.ForeColor = Color.FromArgb(158, 172, 194);
			lblPageSubtitle.Location = new Point(3, 45);
			lblPageSubtitle.Name = "lblPageSubtitle";
			lblPageSubtitle.Size = new Size(888, 23);
			lblPageSubtitle.TabIndex = 1;
			lblPageSubtitle.Text = "Edit serverconfig.xml safely without changing its XML structure.";
			//
			// lblSafeBadge
			//
			lblSafeBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lblSafeBadge.BackColor = Color.FromArgb(22, 58, 70);
			lblSafeBadge.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
			lblSafeBadge.ForeColor = Color.FromArgb(32, 214, 199);
			lblSafeBadge.Location = new Point(979, 8);
			lblSafeBadge.Name = "lblSafeBadge";
			lblSafeBadge.Size = new Size(163, 30);
			lblSafeBadge.TabIndex = 2;
			lblSafeBadge.Text = "FORMAT-AWARE EDITING";
			lblSafeBadge.TextAlign = ContentAlignment.MiddleCenter;
			//
			// toolbarLayout
			//
			toolbarLayout.BackColor = Color.FromArgb(8, 13, 24);
			toolbarLayout.ColumnCount = 5;
			toolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			toolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
			toolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
			toolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 144F));
			toolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 144F));
			toolbarLayout.Controls.Add(pnlSearch, 0, 0);
			toolbarLayout.Controls.Add(cmbTypeFilter, 1, 0);
			toolbarLayout.Controls.Add(btnValidateConfig, 2, 0);
			toolbarLayout.Controls.Add(btnStructured, 3, 0);
			toolbarLayout.Controls.Add(btnRawPreview, 4, 0);
			toolbarLayout.Dock = DockStyle.Fill;
			toolbarLayout.Location = new Point(0, 76);
			toolbarLayout.Margin = new Padding(0);
			toolbarLayout.Name = "toolbarLayout";
			toolbarLayout.RowCount = 1;
			toolbarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			toolbarLayout.Size = new Size(1142, 58);
			toolbarLayout.TabIndex = 1;
			//
			// pnlSearch
			//
			pnlSearch.BackColor = Color.FromArgb(12, 21, 36);
			pnlSearch.BorderColor = Color.FromArgb(38, 52, 77);
			pnlSearch.Controls.Add(lblSearchGlyph);
			pnlSearch.Controls.Add(txtSearch);
			pnlSearch.CornerRadius = 8;
			pnlSearch.Dock = DockStyle.Fill;
			pnlSearch.FillColor = Color.FromArgb(12, 21, 36);
			pnlSearch.Location = new Point(0, 2);
			pnlSearch.Margin = new Padding(0, 2, 12, 10);
			pnlSearch.Name = "pnlSearch";
			pnlSearch.Size = new Size(502, 46);
			pnlSearch.TabIndex = 0;
			//
			// lblSearchGlyph
			//
			lblSearchGlyph.BackColor = Color.FromArgb(12, 21, 36);
			lblSearchGlyph.Font = new Font("Segoe UI Symbol", 15F);
			lblSearchGlyph.ForeColor = Color.FromArgb(105, 124, 153);
			lblSearchGlyph.Location = new Point(12, 8);
			lblSearchGlyph.Name = "lblSearchGlyph";
			lblSearchGlyph.Size = new Size(28, 29);
			lblSearchGlyph.TabIndex = 0;
			lblSearchGlyph.Text = "⌕";
			lblSearchGlyph.TextAlign = ContentAlignment.MiddleCenter;
			//
			// txtSearch
			//
			txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			txtSearch.BackColor = Color.FromArgb(12, 21, 36);
			txtSearch.BorderStyle = BorderStyle.None;
			txtSearch.Font = new Font("Segoe UI", 10.5F);
			txtSearch.ForeColor = Color.FromArgb(245, 247, 251);
			txtSearch.Location = new Point(44, 13);
			txtSearch.Name = "txtSearch";
			txtSearch.PlaceholderText = "Search settings, paths, or values...";
			txtSearch.Size = new Size(440, 19);
			txtSearch.TabIndex = 1;
			txtSearch.TextChanged += txtSearch_TextChanged;
			//
			// cmbTypeFilter
			//
			cmbTypeFilter.ArrowColor = Color.FromArgb(158, 172, 194);
			cmbTypeFilter.BackColor = Color.FromArgb(12, 21, 36);
			cmbTypeFilter.BorderColor = Color.FromArgb(38, 52, 77);
			cmbTypeFilter.Dock = DockStyle.Fill;
			cmbTypeFilter.DrawMode = DrawMode.OwnerDrawFixed;
			cmbTypeFilter.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbTypeFilter.FlatStyle = FlatStyle.Flat;
			cmbTypeFilter.FocusBorderColor = Color.FromArgb(38, 52, 77);
			cmbTypeFilter.Font = new Font("Segoe UI", 10F);
			cmbTypeFilter.ForeColor = Color.FromArgb(245, 247, 251);
			cmbTypeFilter.FormattingEnabled = true;
			cmbTypeFilter.ItemHeight = 28;
			cmbTypeFilter.Items.AddRange(new object[] { "All types", "TEXT", "NUMBER", "BOOLEAN", "SECRET", "NULL" });
			cmbTypeFilter.Location = new Point(526, 7);
			cmbTypeFilter.Margin = new Padding(12, 7, 12, 13);
			cmbTypeFilter.Name = "cmbTypeFilter";
			cmbTypeFilter.SelectedItemBackColor = Color.FromArgb(24, 55, 73);
			cmbTypeFilter.Size = new Size(146, 34);
			cmbTypeFilter.TabIndex = 1;
			cmbTypeFilter.SelectedIndexChanged += cmbTypeFilter_SelectedIndexChanged;
			//
			// btnValidateConfig
			//
			btnValidateConfig.BackColor = Color.FromArgb(12, 21, 36);
			btnValidateConfig.Cursor = Cursors.Hand;
			btnValidateConfig.Dock = DockStyle.Fill;
			btnValidateConfig.FlatAppearance.BorderSize = 0;
			btnValidateConfig.FlatStyle = FlatStyle.Flat;
			btnValidateConfig.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnValidateConfig.ForeColor = Color.FromArgb(245, 181, 74);
			btnValidateConfig.Location = new Point(690, 2);
			btnValidateConfig.Margin = new Padding(6, 2, 6, 10);
			btnValidateConfig.Name = "btnValidateConfig";
			btnValidateConfig.Size = new Size(158, 46);
			btnValidateConfig.TabIndex = 2;
			btnValidateConfig.Text = "Check Synix Values";
			btnValidateConfig.UseVisualStyleBackColor = false;
			btnValidateConfig.Click += btnValidateConfig_Click;
			//
			// btnStructured
			//
			btnStructured.BackColor = Color.FromArgb(12, 21, 36);
			btnStructured.Cursor = Cursors.Hand;
			btnStructured.Dock = DockStyle.Fill;
			btnStructured.FlatAppearance.BorderSize = 0;
			btnStructured.FlatStyle = FlatStyle.Flat;
			btnStructured.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnStructured.ForeColor = Color.FromArgb(32, 214, 199);
			btnStructured.Location = new Point(866, 2);
			btnStructured.Margin = new Padding(12, 2, 6, 10);
			btnStructured.Name = "btnStructured";
			btnStructured.Size = new Size(126, 46);
			btnStructured.TabIndex = 3;
			btnStructured.Text = "Structured View";
			btnStructured.UseVisualStyleBackColor = false;
			btnStructured.Click += btnStructured_Click;
			//
			// btnRawPreview
			//
			btnRawPreview.BackColor = Color.FromArgb(12, 21, 36);
			btnRawPreview.Cursor = Cursors.Hand;
			btnRawPreview.Dock = DockStyle.Fill;
			btnRawPreview.FlatAppearance.BorderSize = 0;
			btnRawPreview.FlatStyle = FlatStyle.Flat;
			btnRawPreview.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnRawPreview.ForeColor = Color.FromArgb(158, 172, 194);
			btnRawPreview.Location = new Point(1004, 2);
			btnRawPreview.Margin = new Padding(6, 2, 0, 10);
			btnRawPreview.Name = "btnRawPreview";
			btnRawPreview.Size = new Size(138, 46);
			btnRawPreview.TabIndex = 4;
			btnRawPreview.Text = "Raw Preview";
			btnRawPreview.UseVisualStyleBackColor = false;
			btnRawPreview.Click += btnRawPreview_Click;
			//
			// pnlPreservationBanner
			//
			pnlPreservationBanner.BackColor = Color.FromArgb(13, 38, 49);
			pnlPreservationBanner.BorderColor = Color.FromArgb(27, 107, 111);
			pnlPreservationBanner.Controls.Add(lblShieldGlyph);
			pnlPreservationBanner.Controls.Add(lblPreservationTitle);
			pnlPreservationBanner.Controls.Add(lblPreservationText);
			pnlPreservationBanner.CornerRadius = 10;
			pnlPreservationBanner.Dock = DockStyle.Fill;
			pnlPreservationBanner.FillColor = Color.FromArgb(13, 38, 49);
			pnlPreservationBanner.Location = new Point(0, 134);
			pnlPreservationBanner.Margin = new Padding(0, 0, 0, 14);
			pnlPreservationBanner.Name = "pnlPreservationBanner";
			pnlPreservationBanner.Size = new Size(1142, 58);
			pnlPreservationBanner.TabIndex = 2;
			//
			// lblShieldGlyph
			//
			lblShieldGlyph.BackColor = Color.FromArgb(13, 38, 49);
			lblShieldGlyph.Font = new Font("Segoe UI Symbol", 18F);
			lblShieldGlyph.ForeColor = Color.FromArgb(32, 214, 199);
			lblShieldGlyph.Location = new Point(16, 10);
			lblShieldGlyph.Name = "lblShieldGlyph";
			lblShieldGlyph.Size = new Size(42, 38);
			lblShieldGlyph.TabIndex = 0;
			lblShieldGlyph.Text = "◇";
			lblShieldGlyph.TextAlign = ContentAlignment.MiddleCenter;
			//
			// lblPreservationTitle
			//
			lblPreservationTitle.AutoSize = true;
			lblPreservationTitle.BackColor = Color.FromArgb(13, 38, 49);
			lblPreservationTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
			lblPreservationTitle.ForeColor = Color.FromArgb(245, 247, 251);
			lblPreservationTitle.Location = new Point(68, 10);
			lblPreservationTitle.Name = "lblPreservationTitle";
			lblPreservationTitle.Size = new Size(223, 19);
			lblPreservationTitle.TabIndex = 1;
			lblPreservationTitle.Text = "Original formatting is protected";
			//
			// lblPreservationText
			//
			lblPreservationText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblPreservationText.AutoEllipsis = true;
			lblPreservationText.BackColor = Color.FromArgb(13, 38, 49);
			lblPreservationText.Font = new Font("Segoe UI", 9F);
			lblPreservationText.ForeColor = Color.FromArgb(158, 172, 194);
			lblPreservationText.Location = new Point(68, 31);
			lblPreservationText.Name = "lblPreservationText";
			lblPreservationText.Size = new Size(1054, 19);
			lblPreservationText.TabIndex = 2;
			lblPreservationText.Text = "Only the value you change is replaced; comments, sections, nesting, quotes, spacing, and key order remain intact.";
			//
			// pnlGridCard
			//
			pnlGridCard.BackColor = Color.FromArgb(17, 27, 45);
			pnlGridCard.BorderColor = Color.FromArgb(38, 52, 77);
			pnlGridCard.Controls.Add(dgvConfig);
			pnlGridCard.Controls.Add(txtRawPreview);
			pnlGridCard.CornerRadius = 10;
			pnlGridCard.Dock = DockStyle.Fill;
			pnlGridCard.FillColor = Color.FromArgb(17, 27, 45);
			pnlGridCard.Location = new Point(0, 206);
			pnlGridCard.Margin = new Padding(0, 0, 0, 12);
			pnlGridCard.Name = "pnlGridCard";
			pnlGridCard.Padding = new Padding(1);
			pnlGridCard.Size = new Size(1142, 392);
			pnlGridCard.TabIndex = 3;
			//
			// dgvConfig
			//
			dgvConfig.AllowUserToAddRows = false;
			dgvConfig.AllowUserToDeleteRows = false;
			dgvConfig.AllowUserToResizeRows = false;
			dataGridViewCellStyle1.BackColor = Color.FromArgb(15, 25, 42);
			dataGridViewCellStyle1.ForeColor = Color.FromArgb(245, 247, 251);
			dgvConfig.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
			dgvConfig.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			dgvConfig.BackgroundColor = Color.FromArgb(17, 27, 45);
			dgvConfig.BorderStyle = BorderStyle.None;
			dgvConfig.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
			dgvConfig.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = Color.FromArgb(12, 21, 36);
			dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			dataGridViewCellStyle2.ForeColor = Color.FromArgb(158, 172, 194);
			dataGridViewCellStyle2.Padding = new Padding(12, 0, 0, 0);
			dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(12, 21, 36);
			dataGridViewCellStyle2.SelectionForeColor = Color.FromArgb(158, 172, 194);
			dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
			dgvConfig.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
			dgvConfig.ColumnHeadersHeight = 44;
			dgvConfig.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			dgvConfig.Columns.AddRange(new DataGridViewColumn[] { colSetting, colType, colValue });
			dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle6.BackColor = Color.FromArgb(17, 27, 45);
			dataGridViewCellStyle6.Font = new Font("Segoe UI", 10F);
			dataGridViewCellStyle6.ForeColor = Color.FromArgb(245, 247, 251);
			dataGridViewCellStyle6.Padding = new Padding(12, 0, 12, 0);
			dataGridViewCellStyle6.SelectionBackColor = Color.FromArgb(24, 55, 73);
			dataGridViewCellStyle6.SelectionForeColor = Color.FromArgb(245, 247, 251);
			dataGridViewCellStyle6.WrapMode = DataGridViewTriState.False;
			dgvConfig.DefaultCellStyle = dataGridViewCellStyle6;
			dgvConfig.Dock = DockStyle.Fill;
			dgvConfig.EditMode = DataGridViewEditMode.EditOnEnter;
			dgvConfig.EnableHeadersVisualStyles = false;
			dgvConfig.GridColor = Color.FromArgb(38, 52, 77);
			dgvConfig.Location = new Point(1, 1);
			dgvConfig.Margin = new Padding(0);
			dgvConfig.MultiSelect = false;
			dgvConfig.Name = "dgvConfig";
			dgvConfig.RowHeadersVisible = false;
			dgvConfig.RowTemplate.Height = 52;
			dgvConfig.SelectionMode = DataGridViewSelectionMode.CellSelect;
			dgvConfig.Size = new Size(1140, 390);
			dgvConfig.TabIndex = 0;
			dgvConfig.CellMouseDown += dgvConfig_CellMouseDown;
			dgvConfig.CellPainting += dgvConfig_CellPainting;
			dgvConfig.CellValueChanged += dgvConfig_CellValueChanged;
			dgvConfig.CurrentCellDirtyStateChanged += dgvConfig_CurrentCellDirtyStateChanged;
			dgvConfig.DataError += dgvConfig_DataError;
			dgvConfig.EditingControlShowing += dgvConfig_EditingControlShowing;
			//
			// colSetting
			//
			dataGridViewCellStyle3.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			dataGridViewCellStyle3.ForeColor = Color.FromArgb(245, 247, 251);
			colSetting.DefaultCellStyle = dataGridViewCellStyle3;
			colSetting.FillWeight = 44F;
			colSetting.HeaderText = "SETTING";
			colSetting.MinimumWidth = 280;
			colSetting.Name = "colSetting";
			colSetting.ReadOnly = true;
			colSetting.SortMode = DataGridViewColumnSortMode.NotSortable;
			//
			// colType
			//
			colType.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle4.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
			colType.DefaultCellStyle = dataGridViewCellStyle4;
			colType.HeaderText = "TYPE";
			colType.MinimumWidth = 120;
			colType.Name = "colType";
			colType.ReadOnly = true;
			colType.SortMode = DataGridViewColumnSortMode.NotSortable;
			colType.Width = 132;
			//
			// colValue
			//
			dataGridViewCellStyle5.BackColor = Color.FromArgb(12, 21, 36);
			dataGridViewCellStyle5.ForeColor = Color.FromArgb(245, 247, 251);
			dataGridViewCellStyle5.SelectionBackColor = Color.FromArgb(29, 63, 80);
			dataGridViewCellStyle5.SelectionForeColor = Color.FromArgb(245, 247, 251);
			colValue.DefaultCellStyle = dataGridViewCellStyle5;
			colValue.FillWeight = 56F;
			colValue.HeaderText = "VALUE";
			colValue.MinimumWidth = 300;
			colValue.Name = "colValue";
			colValue.SortMode = DataGridViewColumnSortMode.NotSortable;
			//
			// txtRawPreview
			//
			txtRawPreview.BackColor = Color.FromArgb(12, 21, 36);
			txtRawPreview.BorderStyle = BorderStyle.None;
			txtRawPreview.DetectUrls = false;
			txtRawPreview.Dock = DockStyle.Fill;
			txtRawPreview.Font = new Font("Cascadia Mono", 9.5F);
			txtRawPreview.ForeColor = Color.FromArgb(203, 213, 225);
			txtRawPreview.Location = new Point(1, 1);
			txtRawPreview.Name = "txtRawPreview";
			txtRawPreview.ReadOnly = true;
			txtRawPreview.Size = new Size(1140, 390);
			txtRawPreview.TabIndex = 1;
			txtRawPreview.Text = "";
			txtRawPreview.Visible = false;
			txtRawPreview.WordWrap = false;
			//
			// footerPanel
			//
			footerPanel.BackColor = Color.FromArgb(8, 13, 24);
			footerPanel.Controls.Add(footerTopBorder);
			footerPanel.Controls.Add(lblSettingCount);
			footerPanel.Controls.Add(lblModifiedCount);
			footerPanel.Controls.Add(lblStatusGlyph);
			footerPanel.Controls.Add(lblFormatState);
			footerPanel.Controls.Add(btnFixConfig);
			footerPanel.Controls.Add(btnRestoreBackup);
			footerPanel.Controls.Add(btnReset);
			footerPanel.Controls.Add(btnCancel);
			footerPanel.Controls.Add(btnSave);
			footerPanel.Dock = DockStyle.Fill;
			footerPanel.Location = new Point(0, 610);
			footerPanel.Margin = new Padding(0);
			footerPanel.Name = "footerPanel";
			footerPanel.Size = new Size(1142, 72);
			footerPanel.TabIndex = 4;
			//
			// footerTopBorder
			//
			footerTopBorder.BackColor = Color.FromArgb(38, 52, 77);
			footerTopBorder.Dock = DockStyle.Top;
			footerTopBorder.Location = new Point(0, 0);
			footerTopBorder.Margin = new Padding(0);
			footerTopBorder.Name = "footerTopBorder";
			footerTopBorder.Size = new Size(1142, 1);
			footerTopBorder.TabIndex = 0;
			//
			// lblSettingCount
			//
			lblSettingCount.AutoSize = true;
			lblSettingCount.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			lblSettingCount.ForeColor = Color.FromArgb(245, 247, 251);
			lblSettingCount.Location = new Point(0, 15);
			lblSettingCount.Name = "lblSettingCount";
			lblSettingCount.Size = new Size(68, 17);
			lblSettingCount.TabIndex = 1;
			lblSettingCount.Text = "0 settings";
			//
			// lblModifiedCount
			//
			lblModifiedCount.AutoSize = true;
			lblModifiedCount.Font = new Font("Segoe UI", 9F);
			lblModifiedCount.ForeColor = Color.FromArgb(105, 124, 153);
			lblModifiedCount.Location = new Point(0, 38);
			lblModifiedCount.Name = "lblModifiedCount";
			lblModifiedCount.Size = new Size(107, 15);
			lblModifiedCount.TabIndex = 2;
			lblModifiedCount.Text = "0 unsaved changes";
			//
			// lblStatusGlyph
			//
			lblStatusGlyph.BackColor = Color.FromArgb(8, 13, 24);
			lblStatusGlyph.Font = new Font("Segoe UI Symbol", 10F, FontStyle.Bold);
			lblStatusGlyph.ForeColor = Color.FromArgb(32, 214, 199);
			lblStatusGlyph.Location = new Point(176, 15);
			lblStatusGlyph.Name = "lblStatusGlyph";
			lblStatusGlyph.Size = new Size(22, 22);
			lblStatusGlyph.TabIndex = 3;
			lblStatusGlyph.Text = "✓";
			lblStatusGlyph.TextAlign = ContentAlignment.MiddleCenter;
			//
			// lblFormatState
			//
			lblFormatState.AutoSize = true;
			lblFormatState.Font = new Font("Segoe UI", 9F);
			lblFormatState.ForeColor = Color.FromArgb(158, 172, 194);
			lblFormatState.Location = new Point(202, 18);
			lblFormatState.Name = "lblFormatState";
			lblFormatState.Size = new Size(135, 15);
			lblFormatState.TabIndex = 4;
			lblFormatState.Text = "XML structure preserved";
			//
			// btnFixConfig
			//
			btnFixConfig.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnFixConfig.BackColor = Color.FromArgb(12, 21, 36);
			btnFixConfig.Cursor = Cursors.Hand;
			btnFixConfig.Enabled = false;
			btnFixConfig.FlatAppearance.BorderSize = 0;
			btnFixConfig.FlatStyle = FlatStyle.Flat;
			btnFixConfig.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnFixConfig.ForeColor = Color.FromArgb(245, 181, 74);
			btnFixConfig.Location = new Point(612, 14);
			btnFixConfig.Name = "btnFixConfig";
			btnFixConfig.Size = new Size(140, 44);
			btnFixConfig.TabIndex = 5;
			btnFixConfig.Text = "Fix Config";
			btnFixConfig.UseVisualStyleBackColor = false;
			btnFixConfig.Click += btnFixConfig_Click;
			btnRestoreBackup.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnRestoreBackup.BackColor = Color.FromArgb(12, 21, 36);
			btnRestoreBackup.Cursor = Cursors.Hand;
			btnRestoreBackup.Enabled = false;
			btnRestoreBackup.FlatAppearance.BorderSize = 0;
			btnRestoreBackup.FlatStyle = FlatStyle.Flat;
			btnRestoreBackup.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnRestoreBackup.ForeColor = Color.FromArgb(96, 165, 250);
			btnRestoreBackup.Location = new Point(462, 14);
			btnRestoreBackup.Name = "btnRestoreBackup";
			btnRestoreBackup.Size = new Size(140, 44);
			btnRestoreBackup.TabIndex = 4;
			btnRestoreBackup.Text = "Restore Backup";
			btnRestoreBackup.UseVisualStyleBackColor = false;
			btnRestoreBackup.Click += btnRestoreBackup_Click;
			//
			// btnReset
			//
			btnReset.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnReset.BackColor = Color.FromArgb(12, 21, 36);
			btnReset.Cursor = Cursors.Hand;
			btnReset.Enabled = false;
			btnReset.FlatAppearance.BorderSize = 0;
			btnReset.FlatStyle = FlatStyle.Flat;
			btnReset.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnReset.ForeColor = Color.FromArgb(158, 172, 194);
			btnReset.Location = new Point(762, 14);
			btnReset.Name = "btnReset";
			btnReset.Size = new Size(110, 44);
			btnReset.TabIndex = 6;
			btnReset.Text = "Undo Edits";
			btnReset.UseVisualStyleBackColor = false;
			btnReset.Click += btnReset_Click;
			//
			// btnCancel
			//
			btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnCancel.BackColor = Color.FromArgb(12, 21, 36);
			btnCancel.Cursor = Cursors.Hand;
			btnCancel.FlatAppearance.BorderSize = 0;
			btnCancel.FlatStyle = FlatStyle.Flat;
			btnCancel.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnCancel.ForeColor = Color.FromArgb(245, 247, 251);
			btnCancel.Location = new Point(882, 14);
			btnCancel.Name = "btnCancel";
			btnCancel.Size = new Size(110, 44);
			btnCancel.TabIndex = 7;
			btnCancel.Text = "Cancel";
			btnCancel.UseVisualStyleBackColor = false;
			btnCancel.Click += btnCancel_Click;
			//
			// btnSave
			//
			btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnSave.BackColor = Color.FromArgb(12, 21, 36);
			btnSave.Cursor = Cursors.Hand;
			btnSave.Enabled = false;
			btnSave.FlatAppearance.BorderSize = 0;
			btnSave.FlatStyle = FlatStyle.Flat;
			btnSave.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
			btnSave.ForeColor = Color.FromArgb(32, 214, 199);
			btnSave.Location = new Point(1002, 14);
			btnSave.Name = "btnSave";
			btnSave.Size = new Size(140, 44);
			btnSave.TabIndex = 8;
			btnSave.Text = "Save Changes";
			btnSave.UseVisualStyleBackColor = false;
			btnSave.Click += btnSave_Click;
			//
			// ServerConfig
			//
			AcceptButton = btnSave;
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			BackColor = Color.FromArgb(38, 52, 77);
			ClientSize = new Size(1200, 780);
			Controls.Add(shellLayout);
			DoubleBuffered = true;
			Font = new Font("Segoe UI", 9F);
			ForeColor = Color.FromArgb(245, 247, 251);
			FormBorderStyle = FormBorderStyle.None;
			Icon = (Icon)resources.GetObject("$this.Icon");
			KeyPreview = true;
			MaximizeBox = false;
			MinimumSize = new Size(980, 640);
			Name = "ServerConfig";
			Padding = new Padding(1);
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Config Editor";
			shellLayout.ResumeLayout(false);
			titleBar.ResumeLayout(false);
			titleBar.PerformLayout();
			((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
			contentHost.ResumeLayout(false);
			mainLayout.ResumeLayout(false);
			headerPanel.ResumeLayout(false);
			headerPanel.PerformLayout();
			toolbarLayout.ResumeLayout(false);
			pnlSearch.ResumeLayout(false);
			pnlSearch.PerformLayout();
			pnlPreservationBanner.ResumeLayout(false);
			pnlPreservationBanner.PerformLayout();
			pnlGridCard.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)dgvConfig).EndInit();
			footerPanel.ResumeLayout(false);
			footerPanel.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		private TableLayoutPanel shellLayout;
		private Panel titleBar;
		private PictureBox picLogo;
		private Label lblWindowTitle;
		private Label lblFileName;
		private Label lblFormatBadge;
		private Button btnMinimize;
		private Button btnClose;
		private Label titleBottomBorder;
		private Panel contentHost;
		private TableLayoutPanel mainLayout;
		private Panel headerPanel;
		private Label lblPageTitle;
		private Label lblPageSubtitle;
		private Label lblSafeBadge;
		private TableLayoutPanel toolbarLayout;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsCard pnlSearch;
		private Label lblSearchGlyph;
		private TextBox txtSearch;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsComboBox cmbTypeFilter;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsButton btnValidateConfig;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsButton btnStructured;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsButton btnRawPreview;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsCard pnlPreservationBanner;
		private Label lblShieldGlyph;
		private Label lblPreservationTitle;
		private Label lblPreservationText;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsCard pnlGridCard;
		private DataGridView dgvConfig;
		private DataGridViewTextBoxColumn colSetting;
		private DataGridViewTextBoxColumn colType;
		private DataGridViewTextBoxColumn colValue;
		private RichTextBox txtRawPreview;
		private Panel footerPanel;
		private Label footerTopBorder;
		private Label lblSettingCount;
		private Label lblModifiedCount;
		private Label lblStatusGlyph;
		private Label lblFormatState;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsButton btnFixConfig;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsButton btnRestoreBackup;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsButton btnReset;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsButton btnCancel;
		private Synix_Control_Panel.SynixApp.Design.ModernSettingsButton btnSave;
	}
}
