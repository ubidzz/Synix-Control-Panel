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

#nullable enable

namespace Synix_Control_Panel.SynixApp.UI.GameDefinitions
{
	partial class ArgumentVerificationDialog
	{
		private System.ComponentModel.IContainer? components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				components?.Dispose();
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
			DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
			_titleLabel = new Label();
			_subtitleLabel = new Label();
			_instanceLabel = new Label();
			_instanceCombo = new ModernSettingsComboBox();
			_executableLabel = new Label();
			_executableBox = new TextBox();
			_workingDirectoryLabel = new Label();
			_workingDirectoryBox = new TextBox();
			_argumentsLabel = new Label();
			_argumentsBox = new TextBox();
			_checksGrid = new DataGridView();
			_checkColumn = new DataGridViewTextBoxColumn();
			_resultColumn = new DataGridViewTextBoxColumn();
			_detailsColumn = new DataGridViewTextBoxColumn();
			_confirmationCheck = new CheckBox();
			_statusLabel = new Label();
			_validateButton = new ModernSettingsButton();
			_startButton = new ModernSettingsButton();
			_stopButton = new ModernSettingsButton();
			_markButton = new ModernSettingsButton();
			_closeButton = new ModernSettingsButton();
			_probeTimer = new System.Windows.Forms.Timer(components);
			((System.ComponentModel.ISupportInitialize)_checksGrid).BeginInit();
			SuspendLayout();
			_titleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_titleLabel.Font = new Font("Segoe UI", 19F, FontStyle.Bold);
			_titleLabel.ForeColor = Color.FromArgb(245, 247, 251);
			_titleLabel.Location = new Point(26, 18);
			_titleLabel.Name = "_titleLabel";
			_titleLabel.Size = new Size(930, 40);
			_titleLabel.TabIndex = 0;
			_titleLabel.Text = "Argument Test";
			_subtitleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_subtitleLabel.ForeColor = Color.FromArgb(158, 172, 194);
			_subtitleLabel.Location = new Point(28, 60);
			_subtitleLabel.Name = "_subtitleLabel";
			_subtitleLabel.Size = new Size(928, 42);
			_subtitleLabel.TabIndex = 1;
			_subtitleLabel.Text = "Synix builds the real command with this server's saved settings, hides every password, starts it normally, and waits for proof that the server accepted the launch.";
			_instanceLabel.AutoSize = true;
			_instanceLabel.ForeColor = Color.FromArgb(158, 172, 194);
			_instanceLabel.Location = new Point(28, 108);
			_instanceLabel.Name = "_instanceLabel";
			_instanceLabel.Size = new Size(139, 19);
			_instanceLabel.TabIndex = 2;
			_instanceLabel.Text = "Installed server to test";
			_instanceCombo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_instanceCombo.ArrowColor = Color.FromArgb(158, 172, 194);
			_instanceCombo.BackColor = Color.FromArgb(12, 21, 36);
			_instanceCombo.BorderColor = Color.FromArgb(38, 52, 77);
			_instanceCombo.DrawMode = DrawMode.OwnerDrawFixed;
			_instanceCombo.DropDownStyle = ComboBoxStyle.DropDownList;
			_instanceCombo.FlatStyle = FlatStyle.Flat;
			_instanceCombo.FocusBorderColor = Color.FromArgb(38, 52, 77);
			_instanceCombo.ForeColor = Color.FromArgb(245, 247, 251);
			_instanceCombo.ItemHeight = 28;
			_instanceCombo.Location = new Point(190, 100);
			_instanceCombo.Name = "_instanceCombo";
			_instanceCombo.SelectedItemBackColor = Color.FromArgb(24, 55, 73);
			_instanceCombo.Size = new Size(766, 34);
			_instanceCombo.TabIndex = 3;
			_instanceCombo.SelectedIndexChanged += InstanceCombo_SelectedIndexChanged;
			_executableLabel.AutoSize = true;
			_executableLabel.ForeColor = Color.FromArgb(158, 172, 194);
			_executableLabel.Location = new Point(28, 148);
			_executableLabel.Name = "_executableLabel";
			_executableLabel.Size = new Size(72, 19);
			_executableLabel.TabIndex = 4;
			_executableLabel.Text = "Launch file";
			_executableBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_executableBox.BackColor = Color.FromArgb(12, 21, 36);
			_executableBox.BorderStyle = BorderStyle.FixedSingle;
			_executableBox.ForeColor = Color.FromArgb(245, 247, 251);
			_executableBox.Location = new Point(116, 144);
			_executableBox.Name = "_executableBox";
			_executableBox.ReadOnly = true;
			_executableBox.Size = new Size(840, 25);
			_executableBox.TabIndex = 5;
			_workingDirectoryLabel.AutoSize = true;
			_workingDirectoryLabel.ForeColor = Color.FromArgb(158, 172, 194);
			_workingDirectoryLabel.Location = new Point(28, 182);
			_workingDirectoryLabel.Name = "_workingDirectoryLabel";
			_workingDirectoryLabel.Size = new Size(51, 19);
			_workingDirectoryLabel.TabIndex = 6;
			_workingDirectoryLabel.Text = "Folder";
			_workingDirectoryBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_workingDirectoryBox.BackColor = Color.FromArgb(12, 21, 36);
			_workingDirectoryBox.BorderStyle = BorderStyle.FixedSingle;
			_workingDirectoryBox.ForeColor = Color.FromArgb(245, 247, 251);
			_workingDirectoryBox.Location = new Point(116, 178);
			_workingDirectoryBox.Name = "_workingDirectoryBox";
			_workingDirectoryBox.ReadOnly = true;
			_workingDirectoryBox.Size = new Size(840, 25);
			_workingDirectoryBox.TabIndex = 7;
			_argumentsLabel.AutoSize = true;
			_argumentsLabel.ForeColor = Color.FromArgb(158, 172, 194);
			_argumentsLabel.Location = new Point(28, 216);
			_argumentsLabel.Name = "_argumentsLabel";
			_argumentsLabel.Size = new Size(196, 19);
			_argumentsLabel.TabIndex = 8;
			_argumentsLabel.Text = "Sanitized arguments (no secrets)";
			_argumentsBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_argumentsBox.BackColor = Color.FromArgb(7, 12, 20);
			_argumentsBox.BorderStyle = BorderStyle.FixedSingle;
			_argumentsBox.Font = new Font("Consolas", 9.5F);
			_argumentsBox.ForeColor = Color.FromArgb(114, 226, 219);
			_argumentsBox.Location = new Point(28, 240);
			_argumentsBox.Multiline = true;
			_argumentsBox.Name = "_argumentsBox";
			_argumentsBox.ReadOnly = true;
			_argumentsBox.ScrollBars = ScrollBars.Vertical;
			_argumentsBox.Size = new Size(928, 96);
			_argumentsBox.TabIndex = 9;
			_checksGrid.AllowUserToAddRows = false;
			_checksGrid.AllowUserToDeleteRows = false;
			_checksGrid.AllowUserToResizeRows = false;
			_checksGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_checksGrid.BackgroundColor = Color.FromArgb(12, 21, 36);
			_checksGrid.BorderStyle = BorderStyle.None;
			_checksGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
			_checksGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle1.BackColor = Color.FromArgb(17, 27, 45);
			dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
			dataGridViewCellStyle1.ForeColor = Color.FromArgb(158, 172, 194);
			dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(17, 27, 45);
			dataGridViewCellStyle1.SelectionForeColor = Color.FromArgb(158, 172, 194);
			_checksGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			_checksGrid.ColumnHeadersHeight = 38;
			_checksGrid.Columns.AddRange(new DataGridViewColumn[] { _checkColumn, _resultColumn, _detailsColumn });
			dataGridViewCellStyle2.BackColor = Color.FromArgb(12, 21, 36);
			dataGridViewCellStyle2.ForeColor = Color.FromArgb(245, 247, 251);
			dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(24, 55, 73);
			dataGridViewCellStyle2.SelectionForeColor = Color.FromArgb(245, 247, 251);
			_checksGrid.DefaultCellStyle = dataGridViewCellStyle2;
			_checksGrid.EnableHeadersVisualStyles = false;
			_checksGrid.Location = new Point(28, 350);
			_checksGrid.MultiSelect = false;
			_checksGrid.Name = "_checksGrid";
			_checksGrid.ReadOnly = true;
			_checksGrid.RowHeadersVisible = false;
			_checksGrid.RowTemplate.Height = 34;
			_checksGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			_checksGrid.Size = new Size(928, 210);
			_checksGrid.TabIndex = 10;
			_checksGrid.CellFormatting += ChecksGrid_CellFormatting;
			_checkColumn.HeaderText = "CHECK";
			_checkColumn.Name = "_checkColumn";
			_checkColumn.ReadOnly = true;
			_checkColumn.Width = 190;
			_resultColumn.HeaderText = "RESULT";
			_resultColumn.Name = "_resultColumn";
			_resultColumn.ReadOnly = true;
			_resultColumn.Width = 80;
			_detailsColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			_detailsColumn.HeaderText = "DETAILS";
			_detailsColumn.Name = "_detailsColumn";
			_detailsColumn.ReadOnly = true;
			_confirmationCheck.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_confirmationCheck.AutoCheck = false;
			_confirmationCheck.ForeColor = Color.FromArgb(245, 247, 251);
			_confirmationCheck.Location = new Point(28, 570);
			_confirmationCheck.Name = "_confirmationCheck";
			_confirmationCheck.Size = new Size(928, 44);
			_confirmationCheck.TabStop = false;
			_confirmationCheck.TabIndex = 11;
			_confirmationCheck.Text = "I confirmed the displayed server name, ports, player limit, and all other values used by this definition, including passwords, RCON, mode, and map/world where applicable.";
			_confirmationCheck.UseVisualStyleBackColor = true;
			_confirmationCheck.CheckedChanged += ConfirmationCheck_CheckedChanged;
			_statusLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_statusLabel.ForeColor = Color.FromArgb(158, 172, 194);
			_statusLabel.Location = new Point(28, 615);
			_statusLabel.Name = "_statusLabel";
			_statusLabel.Size = new Size(928, 44);
			_statusLabel.TabIndex = 12;
			_statusLabel.Text = "Select an installed server and validate its command.";
			_validateButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			_validateButton.Location = new Point(28, 670);
			_validateButton.Name = "_validateButton";
			_validateButton.Size = new Size(138, 42);
			_validateButton.TabIndex = 13;
			_validateButton.Text = "Validate Command";
			_validateButton.UseVisualStyleBackColor = false;
			_validateButton.Click += ValidateButton_Click;
			_startButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			_startButton.Location = new Point(176, 670);
			_startButton.Name = "_startButton";
			_startButton.Size = new Size(126, 42);
			_startButton.TabIndex = 14;
			_startButton.Text = "Start Test";
			_startButton.UseAccentStyle = true;
			_startButton.UseVisualStyleBackColor = false;
			_startButton.Click += StartButton_Click;
			_stopButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			_stopButton.Location = new Point(312, 670);
			_stopButton.Name = "_stopButton";
			_stopButton.Size = new Size(126, 42);
			_stopButton.TabIndex = 15;
			_stopButton.Text = "Stop Server";
			_stopButton.UseVisualStyleBackColor = false;
			_stopButton.Click += StopButton_Click;
			_markButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_markButton.Location = new Point(640, 670);
			_markButton.Name = "_markButton";
			_markButton.Size = new Size(170, 42);
			_markButton.TabIndex = 16;
			_markButton.Text = "Record Verification";
			_markButton.UseAccentStyle = true;
			_markButton.UseVisualStyleBackColor = false;
			_markButton.Click += MarkButton_Click;
			_closeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_closeButton.DialogResult = DialogResult.Cancel;
			_closeButton.Location = new Point(820, 670);
			_closeButton.Name = "_closeButton";
			_closeButton.Size = new Size(136, 42);
			_closeButton.TabIndex = 17;
			_closeButton.Text = "Close";
			_closeButton.UseVisualStyleBackColor = false;
			_probeTimer.Interval = 5000;
			_probeTimer.Tick += ProbeTimer_Tick;
			AutoScaleDimensions = new SizeF(7F, 17F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.FromArgb(8, 13, 24);
			CancelButton = _closeButton;
			ClientSize = new Size(984, 730);
			Controls.Add(_titleLabel);
			Controls.Add(_subtitleLabel);
			Controls.Add(_instanceLabel);
			Controls.Add(_instanceCombo);
			Controls.Add(_executableLabel);
			Controls.Add(_executableBox);
			Controls.Add(_workingDirectoryLabel);
			Controls.Add(_workingDirectoryBox);
			Controls.Add(_argumentsLabel);
			Controls.Add(_argumentsBox);
			Controls.Add(_checksGrid);
			Controls.Add(_confirmationCheck);
			Controls.Add(_statusLabel);
			Controls.Add(_validateButton);
			Controls.Add(_startButton);
			Controls.Add(_stopButton);
			Controls.Add(_markButton);
			Controls.Add(_closeButton);
			Font = new Font("Segoe UI", 10F);
			MinimumSize = new Size(860, 680);
			Name = "ArgumentVerificationDialog";
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Synix Argument Test";
			((System.ComponentModel.ISupportInitialize)_checksGrid).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		private Label _titleLabel = null!;
		private Label _subtitleLabel = null!;
		private Label _instanceLabel = null!;
		private ModernSettingsComboBox _instanceCombo = null!;
		private Label _executableLabel = null!;
		private TextBox _executableBox = null!;
		private Label _workingDirectoryLabel = null!;
		private TextBox _workingDirectoryBox = null!;
		private Label _argumentsLabel = null!;
		private TextBox _argumentsBox = null!;
		private DataGridView _checksGrid = null!;
		private DataGridViewTextBoxColumn _checkColumn = null!;
		private DataGridViewTextBoxColumn _resultColumn = null!;
		private DataGridViewTextBoxColumn _detailsColumn = null!;
		private CheckBox _confirmationCheck = null!;
		private Label _statusLabel = null!;
		private ModernSettingsButton _validateButton = null!;
		private ModernSettingsButton _startButton = null!;
		private ModernSettingsButton _stopButton = null!;
		private ModernSettingsButton _markButton = null!;
		private ModernSettingsButton _closeButton = null!;
		private System.Windows.Forms.Timer _probeTimer = null!;
	}
}
